using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class JobDeclarationCollectionFetchStrategy : Customs.Business.FetchStrategies.BaseJobDeclarationCollectionFetchStrategy
	{
		public JobDeclarationCollectionFetchStrategy(JobDeclarationCollection collection)
			: base(collection)
		{
		}

		public class EURequiredFetchForViewData : RequiredFetchForViewData
		{
			public bool CustomsDocStatusFetchForView
			{
				get => customsDocStatusFetchForView && !CusEntryInstructionCusSupportingInfoRequiredFetchForView;
				set => customsDocStatusFetchForView = value;
			}
			bool customsDocStatusFetchForView;
			public bool CusEntryInstructionCusSupportingInfoRequiredFetchForView;
			public bool CusExitHeaderRequiredFetchForView;
			public bool CusExitReportRequiredFetchForView;
		}

		protected new JobDeclarationCollection Collection => (JobDeclarationCollection)base.Collection;

		protected override RequiredFetchForViewData CreateNewRequiredFetchForViewData() => new EURequiredFetchForViewData();

		protected override bool IsCusDecHouseContainerPackRelatedColumn(string columnName)
		{
			return base.IsCusDecHouseContainerPackRelatedColumn(columnName) || columnName == JobDeclaration.Schema.PackTypes;
		}

		protected override void FetchForViewDeclarationAdditionalDataComputation(RequiredFetchForViewData requiredFetchForViewData, string columnName)
		{
			base.FetchForViewDeclarationAdditionalDataComputation(requiredFetchForViewData, columnName);
			var euRequiredFetchForViewData = (EURequiredFetchForViewData)requiredFetchForViewData;
			if (columnName.EqualsAny(JobDeclaration.Schema.CustomsDocStatus, JobDeclaration.Schema.CustomsDocStatusDesc))
			{
				euRequiredFetchForViewData.CusEntryInstructionRequiredFetchForView = true;
				euRequiredFetchForViewData.CustomsDocStatusFetchForView = true;
			}
			else if (columnName.EqualsAny(JobDeclaration.Schema.ExitPresentationStatus, JobDeclaration.Schema.ExitPresentationStatusDesc) || columnName.StartsWith("WorkflowItems+MilestonesIncludingRelated", StringComparison.OrdinalIgnoreCase))
			{
				euRequiredFetchForViewData.CusExitHeaderRequiredFetchForView = true;
				euRequiredFetchForViewData.CusExitReportRequiredFetchForView = true;
			}
		}

		protected override bool ShouldAddRelatedDataFetchHintsForFirstParse(RequiredFetchForViewData requiredFetchForViewData)
		{
			return base.ShouldAddRelatedDataFetchHintsForFirstParse(requiredFetchForViewData) || (requiredFetchForViewData is EURequiredFetchForViewData euRequiredFetchForViewData && (euRequiredFetchForViewData.CusExitHeaderRequiredFetchForView));
		}

		protected override void AddRelatedDataFetchHintsForFirstParse(BusinessObjectFactory factory, BaseJobDeclaration declaration, RequiredFetchForViewData requiredFetchForViewData)
		{
			base.AddRelatedDataFetchHintsForFirstParse(factory, declaration, requiredFetchForViewData);
			var euRequiredFetchForViewData = (EURequiredFetchForViewData)requiredFetchForViewData;
			if (euRequiredFetchForViewData.CusExitHeaderRequiredFetchForView && declaration.IsExport)
			{
				(var mainQuery, var secondaryQuery) = Collection.ExitHeaderLoader.GetLoadQuery(declaration.PK, declaration.TablePrefix); // TODO: Use JE_ClusterKey when CXH_ClusterKey has been updated to use parent clusterkey
				factory.AddFetchHint(CusExitHeaderSchema.Instance, mainQuery, secondaryQuery);
			}
		}

		protected override bool ShouldAddRelatedDataFetchHintsForSecondParse(RequiredFetchForViewData requiredFetchForViewData)
		{
			return base.ShouldAddRelatedDataFetchHintsForSecondParse(requiredFetchForViewData) ||
				(
					requiredFetchForViewData is EURequiredFetchForViewData euRequiredFetchForViewData && (
						euRequiredFetchForViewData.CusEntryInstructionCusSupportingInfoRequiredFetchForView
						|| euRequiredFetchForViewData.CustomsDocStatusFetchForView
						|| euRequiredFetchForViewData.CusExitReportRequiredFetchForView
					));
		}

		protected override void AddRelatedDataFetchHintsForSecondParse(BusinessObjectFactory factory, BaseJobDeclaration baseDeclaration, RequiredFetchForViewData requiredFetchForViewData)
		{
			base.AddRelatedDataFetchHintsForSecondParse(factory, baseDeclaration, requiredFetchForViewData);
			var declaration = (JobDeclaration)baseDeclaration;
			var euRequiredFetchForViewData = (EURequiredFetchForViewData)requiredFetchForViewData;
			if (euRequiredFetchForViewData.CusEntryInstructionCusSupportingInfoRequiredFetchForView || (euRequiredFetchForViewData.CustomsDocStatusFetchForView && declaration.IsExport))
			{
				foreach (var cusEntryInstruction in declaration.CustomsEntryInstructions)
				{
					factory.AddFetchHint(CusSupportingInfoSchema.CSI_ParentID, cusEntryInstruction.PK);
				}
			}
			if (euRequiredFetchForViewData.CusExitReportRequiredFetchForView && declaration.IsExport) // TODO: Use JE_ClusterKey when CER_ClusterKey has been updated to use parent clusterkey
			{
				foreach (var exitHeader in declaration.ExitHeaders)
				{
					factory.AddFetchHint(CusExitReportSchema.CER_ClusterKey, exitHeader.CXH_ClusterKey);
				}
			}
		}
	}
}
