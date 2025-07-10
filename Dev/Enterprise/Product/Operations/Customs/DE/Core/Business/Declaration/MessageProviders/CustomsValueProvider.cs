using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using CusEntryHeader = Enterprise.Customs.DE.Business.Declaration.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction;
using NonPersistentCusDV1DetailPivot = Enterprise.Customs.EU.Business.Declaration.NonPersistentCusDV1DetailPivot;

namespace Enterprise.Customs.DE.Business
{
	public sealed class CustomsValueProvider : ICustomsValue
	{
		public CustomsValueProvider(CusEntryHeader entryHeader)
		{
			Argument.NotNull(entryHeader, nameof(entryHeader));
			entryInstruction = Argument.NotNull(entryHeader.EntryInstruction, nameof(entryHeader.EntryInstruction));
			declaration = entryHeader.Declaration;
		}
		readonly CusEntryInstruction entryInstruction;
		readonly JobDeclaration declaration;

		public string FormerDecisions => CachedValueHelper.GetValue(ref formerDecisions, () => DV1DetailForEntryInstruction?.CustomsDecisionNumber);
		CachedValue<string> formerDecisions;

		public string AffiliationType => CachedValueHelper.GetValue(ref affiliationType, () =>
		{
			if (DV1DetailForEntryInstruction != null)
			{
				switch (DV1DetailForEntryInstruction.Relationship)
				{
					case YesNoList.Codes.No:
						return "0";
					case YesNoList.Codes.Yes:
						switch (DV1DetailForEntryInstruction.PriceInfluence)
						{
							case YesNoList.Codes.No:
								return "1";
							case YesNoList.Codes.Yes:
								return "2";
							default:
								return null;
						}
					default:
						return null;
				}
			}
			return null;
		});
		CachedValue<string> affiliationType;

		public string AffiliationDescription => CachedValueHelper.GetValue(ref affiliationDescription, () => DV1DetailForEntryInstruction?.RelationDetails);
		CachedValue<string> affiliationDescription;

		public bool RestrictionFlag => CachedValueHelper.GetValue(ref restrictionFlag, () => DV1DetailForEntryInstruction?.Restrictions.ToString() == YesNoList.Codes.Yes);
		CachedValue<bool> restrictionFlag;

		public bool ConditionFlag => CachedValueHelper.GetValue(ref conditionFlag, () => DV1DetailForEntryInstruction?.Consideration.ToString() == YesNoList.Codes.Yes);
		CachedValue<bool> conditionFlag;

		public string RestrictionOrConditionDescription => CachedValueHelper.GetValue(ref restrictionOrConditionDescription, () => DV1DetailForEntryInstruction?.RestrictionConsiderationDetails);
		CachedValue<string> restrictionOrConditionDescription;

		public bool LicenseFeeFlag => CachedValueHelper.GetValue(ref licenseFeeFlag, () => DV1DetailForEntryInstruction?.RoyaltiesLicence.ToString() == YesNoList.Codes.Yes);
		CachedValue<bool> licenseFeeFlag;

		public string LicenseFeeDescription => CachedValueHelper.GetValue(ref licenseFeeDescription, () => DV1DetailForEntryInstruction?.RoyaltiesLicenceDetails);
		CachedValue<string> licenseFeeDescription;

		public bool ResaleFlag => CachedValueHelper.GetValue(ref resaleFlag, () => DV1DetailForEntryInstruction?.Resale.ToString() == YesNoList.Codes.Yes);
		CachedValue<bool> resaleFlag;

		public string ResaleDescription => CachedValueHelper.GetValue(ref resaleDescription, () => DV1DetailForEntryInstruction?.ResaleDetails);
		CachedValue<string> resaleDescription;

		public IImportParty Vendor => CachedValueHelper.GetValue(ref vendorCached, () => ImportPartyProvider.NewOrNull(SellerAddress));
		CachedValue<IImportParty> vendorCached;

		public Guid VendorPK => SellerAddress?.PK.ToGuid() ?? Guid.Empty;

		public IImportParty Vendee => CachedValueHelper.GetValue(ref vendeeCached, () => ImportPartyProvider.NewOrNull(ConsigneeAddress));
		CachedValue<IImportParty> vendeeCached;

		public Guid VendeePK => ConsigneeAddress?.PK.ToGuid() ?? Guid.Empty;

		internal NonPersistentCusDV1DetailPivot DV1DetailForEntryInstruction => CachedValueHelper.GetValue(ref dv1DetailForEntryInstruction, () => entryInstruction.DV1DetailsPivots.Cast<NonPersistentCusDV1DetailPivot>().FirstOrDefault(x => x.IsForEntryInstruction));
		CachedValue<NonPersistentCusDV1DetailPivot> dv1DetailForEntryInstruction;

		OrgAddress SellerAddress => entryInstruction.Invoices.FirstOrDefault()?.SellerAddress ?? declaration.SellerAddress;

		OrgAddress ConsigneeAddress => entryInstruction.Invoices.FirstOrDefault()?.BuyerAddress ?? declaration.ConsigneeAddress;
	}
}
