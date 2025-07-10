using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Registry;

namespace Enterprise.Customs.CA.Business
{
	public class ConsolidateToOneFTypePerCLVSEntryStrategy : ConsolidationStrategy
	{
		#region Constructors

		public ConsolidateToOneFTypePerCLVSEntryStrategy(IConsolidationOptionsWrapper wrapper)
			: base(wrapper)
		{
		}

#if DEBUG
		public ConsolidateToOneFTypePerCLVSEntryStrategy(IConsolidationOptionsWrapper wrapper, bool hasAcknowledged)
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
				query.AddToFilter(ZQuery.NoResultQuery);
			}
		}

		public override bool IsMatching(JobDeclaration declaration)
		{
			return !HasAcknowledged;
		}

		public override void FillDataForNewDeclaration(JobDeclaration declaration)
		{
			if (HasAcknowledged)
			{
				var importer = Wrapper.ImporterPK;
				if (!importer.IsEmpty)
				{
					declaration.CA_UseImporterAccountSecurityNumber = true;
					declaration.JE_OH_Importer = importer;
				}
				declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
				declaration.CA_AllowOIC = Wrapper.IsAllowOIC;
			}
		}
		#endregion

		#region GetEffectiveSetting

		protected override bool GetSettingFromRegistry()
		{
			return CACustomsDataRegistry.Instance.ConsolidateToOneFTypePerCLVSEntry.GetFallBackValueAtAllLevels(Wrapper.CompanyPK.ToGuid(), Wrapper.BranchPK.ToGuid(), Guid.Empty);
		}

		protected override bool GetSettingOfImporterCore(OrgImpAddInfo importerAddInfo)
		{
			return importerAddInfo.ZO_IsConsolidateToOneFTypePerCLVSEntry;
		}
		#endregion
	}
}
