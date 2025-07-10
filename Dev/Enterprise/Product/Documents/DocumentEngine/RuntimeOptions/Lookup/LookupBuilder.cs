using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class LookupBuilder : LookupBuilderBase, ICustomBuilder
	{
		const string LinkToScheduledReportRecipientForOrganisation = FilterBuilderPropertyCodeDescriptionList.Codes.LinkToScheduledReportRecipientForOrganisation;

		public LookupBuilder(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
		{
			ExpectedProperties.Add(DefaultValue);
			if (runtimeReportStyle != ReportRunningType.Document || runtimeReportStyle == ReportRunningType.ReportReferenceGuide)
			{
				ExpectedProperties.Add(LinkToScheduledReportRecipientForOrganisation);
			}

			this.EvaluatorForDefaultValues = evaluatorForDefaultValues;
		}

		readonly MatchEvaluator EvaluatorForDefaultValues;

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"{lookup type} Lookup", Res.GetString("FilterDocumentation|51A69D94-3B79-485A-A020-8CB48A0E62D0", "Generates a lookup filter whose value will be a GUID, with available selections from the lookup list. Filters data matching that GUID.\r\nPlease note, property {0} only acts for {1} Lookup.", LinkToScheduledReportRecipientForOrganisation, ModuleIDs.Organisation), supportedProperties, true);
		}

		public override void DoCustomBuilding(StringTreeNode fieldTree, FilterField newField)
		{
			base.DoCustomBuilding(fieldTree, newField);
			string lookupTypeName = GetLookupTypeName(fieldTree);

			var collectionProvider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, lookupTypeName.ToLower().Trim());
			var validator = newField.Validators.OfType<OnlyCurrentCompanyIfSetInCommissionRegistryValidator>().FirstOrDefault();
			if (validator != null && validator.OnlyAllowCurrentLoginCompany)
			{
				((LookupField)newField).ZValue = GlbCompany.CurrentCompany.PK;
			}
			else if (fieldTree.ChildExists(DefaultValue))
			{
				var findBoxListProvider = collectionProvider.CollectionForFindbox as IFindBoxListProvider;
				((LookupField)newField).ZValue = findBoxListProvider.PrimaryKeyFromCode(fieldTree.FindChild(DefaultValue).Child().Value);
			}

			SetLinkToScheduledReportRecipientForOrganisationIfNeeded(fieldTree, collectionProvider, (LookupField)newField);

			((LookupField)newField).Validators.Add(new LookupTypeValidator(collectionProvider.GetFilterDescription()));
		}

		void SetLinkToScheduledReportRecipientForOrganisationIfNeeded(StringTreeNode fieldTree, CollectionProvider collectionProvider, LookupField newField)
		{
			if ((reportRunningType == ReportRunningType.NormalScheduledReport || reportRunningType == ReportRunningType.OneOffScheduledReport || reportRunningType == ReportRunningType.Report)
				&& fieldTree.ChildExists(LinkToScheduledReportRecipientForOrganisation) && collectionProvider != null && collectionProvider.ModuleID == ModuleIDs.Organisation)
			{
				newField.LinkToScheduledReportRecipientForOrganisation = true;
			}
		}

		protected override string RegularExpressionToMatchFilterType
		{
			get { return (NoResString)@"lookup$"; }
		}

		MultipleSelectionLookupBuilder multipleSelectionLookupBuilder;
		DependenceLookupBuilder dependenceLookupBuilder;
		public override bool CanBuild(string filterType)
		{
			if (multipleSelectionLookupBuilder == null)
			{
				multipleSelectionLookupBuilder = new MultipleSelectionLookupBuilder(fValidators, Factory, EvaluatorForDefaultValues, reportRunningType);
			}
			if (dependenceLookupBuilder == null)
			{
				dependenceLookupBuilder = new DependenceLookupBuilder(fValidators, Factory, EvaluatorForDefaultValues, reportRunningType);
			}
			var canMatchMultipleSelectLookup = multipleSelectionLookupBuilder.CanBuild(filterType);
			var canMatchDependenceLookupBuilder = dependenceLookupBuilder.CanBuild(filterType);
			return filterType.ToLower().EndsWith((NoResString)"lookup") && !canMatchMultipleSelectLookup && !canMatchDependenceLookupBuilder;
		}

		protected override FilterField GetFilterField()
		{
			return new LookupField(fBusinessObjectFactory);
		}
	}
}
