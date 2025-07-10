using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class ConsolidateByImporterStrategy : ConsolidationStrategy
	{
		#region Constructors

		public ConsolidateByImporterStrategy(IConsolidationOptionsWrapper wrapper)
			: base(wrapper)
		{
		}

#if DEBUG
		public ConsolidateByImporterStrategy(IConsolidationOptionsWrapper wrapper, bool hasAcknowledged)
			: base(wrapper, hasAcknowledged)
		{
		}
#endif

		#endregion

		#region IConsolidationStrategy Members

		public override void AddMatchingFilter(ZQuery query)
		{
			if (HasAcknowledged)
			{
				query.AddToFilter(JobDeclarationSchema.JE_MessageSubType, LowValueShipmentsTypes.Codes.ConsolidationByImporter);
				var importer = Wrapper.ImporterPK;
				if (!importer.IsEmpty)
				{
					query.AddToFilter(JobDeclarationSchema.JE_OH_Importer, importer);

					var importerAddInfo = OrgImpAddInfo.Get(Wrapper.Importer);
					if (importerAddInfo != null && importerAddInfo.HasAccountSecurityNumber)
					{
						var declarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
						var cusEntryNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
						cusEntryNumberQuery.AddFilterAndZSQLParameterCollection(ZString.Format("{0} Like '{1}%'", CusEntryNumSchema.CE_EntryNum.Name, importerAddInfo.ZO_AccountSecurityNumber), null);
						cusEntryNumberQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Constants.CountryCodes.Canada);
						cusEntryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumber.EntryType.CATransactionNumber);
						declarationQuery.AddSubQuery(cusEntryNumberQuery, JoinCondition.And);
						query.AddToFilter(declarationQuery);
					}
				}

				query.AddToFilter(LVXJobsConsolidateHelper.ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, LVXJobsConsolidateHelper.ModelViewPK, LVXJobsConsolidateHelper.ModelView, "JE_AllowOIC", SQLComparisonOperator.Equal, Wrapper.IsAllowOIC));
			}
			else
			{
				query.AddToFilter(JobDeclarationSchema.JE_MessageSubType, LowValueShipmentsTypes.Codes.TotalConsolidation);
			}
		}

		public override bool IsMatching(JobDeclaration declaration)
		{
			if (HasAcknowledged)
			{
				var importer = Wrapper.ImporterPK;
				return declaration.JE_MessageSubType == LowValueShipmentsTypes.Codes.ConsolidationByImporter
					&& (declaration.JE_OH_Importer.IsEmpty || importer.IsEmpty || declaration.JE_OH_Importer == importer);
			}
			else
			{
				return declaration.JE_MessageSubType == LowValueShipmentsTypes.Codes.TotalConsolidation;
			}
		}

		public override void FillDataForNewDeclaration(JobDeclaration declaration)
		{
			if (HasAcknowledged && declaration.JE_OH_Importer.IsEmpty)
			{
				var importer = Wrapper.ImporterPK;
				if (!importer.IsEmpty)
				{
					declaration.CA_UseImporterAccountSecurityNumber = true;
					declaration.JE_OH_Importer = importer;
				}
			}

			var messageSubType = HasAcknowledged ? LowValueShipmentsTypes.Codes.ConsolidationByImporter : LowValueShipmentsTypes.Codes.TotalConsolidation;
			if (declaration.JE_MessageSubType != messageSubType)
			{
				declaration.JE_MessageSubType = messageSubType;
			}

			declaration.CA_AllowOIC = HasAcknowledged && Wrapper.IsAllowOIC;
		}

		#endregion

		#region GetEffectiveSetting

		protected override bool GetSettingFromRegistry()
		{
			return CACustomsDataRegistry.Instance.ConsolidateByImporter.GetFallBackValueAtAllLevels(Wrapper.CompanyPK.ToGuid(), Wrapper.BranchPK.ToGuid(), Guid.Empty);
		}

		protected override bool GetSettingOfImporterCore(OrgImpAddInfo importerAddInfo)
		{
			return importerAddInfo.ZO_IsLVSConsolidated || importerAddInfo.ZO_IsImporterDirectPayment || importerAddInfo.ZO_IsLVSImporterDirectPayment || importerAddInfo.ZO_IsGSTDirectPayment;
		}

		protected override bool GetHasAcknowledged()
		{
			var lvsType = Wrapper.LVSType;
			return (lvsType.IsEmpty ? base.GetHasAcknowledged() : lvsType == LowValueShipmentsTypes.Codes.ConsolidationByImporter) || Wrapper.IsAllowOIC;
		}

		#endregion
	}
}
