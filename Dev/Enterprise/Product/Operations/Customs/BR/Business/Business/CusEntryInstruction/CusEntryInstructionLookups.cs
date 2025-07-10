using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business
{
	public class CusEntryInstructionLookups : Customs.Business.CusEntryInstructionLookups
	{
		public CusEntryInstructionLookups(CusEntryInstruction cusEntryInstruction)
			: base(cusEntryInstruction)
		{
		}

		public new CusEntryInstruction Parent
		{
			get { return (CusEntryInstruction)base.Parent; }
		}

		public OrgHeaderCollection JustificationContactDetailOrganisations => fJustificationContactDetailOrganisations ?? (fJustificationContactDetailOrganisations = new OrgHeaderCollection(Factory));

		OrgHeaderCollection fJustificationContactDetailOrganisations;

		public CodeDescriptionPairList AdditionalInformationOptions => Factory.GetCachedValue<AdditionalInformationOptions>();

		public CodeDescriptionPairList BillTypeList => Factory.GetCachedValue<BillTypeList>();

		public CodeDescriptionPairList SpecialCustomsClearanceList => Factory.GetCachedValue<SpecialCustomsClearanceList>();

		public CodeDescriptionPairList LegalDocumentList => Factory.GetCachedValue<LegalDocumentList>();

		public CodeDescriptionPairList DetailWithoutLegalDocList => Factory.GetCachedValue<DetailWithoutLegalDocList>();

		public CodeDescriptionPairList AFRMMMethodOfCalculationList
		{
			get
			{
				var effectiveDate = Parent.DateOfValuation;

				return Factory.GetCachedValue($"CEI_AFRMMMethodOfCalculationList_{effectiveDate}", () =>
				{
					var afrmmList = new CodeDescriptionPairList();
					afrmmList.AddRange(BRRefCusTaxOrFee.GetAfrmmTaxs(Factory, effectiveDate));
					afrmmList.RemoveCode(Core.Constants.Customs.Universal.RefCusTaxOrFee.Codes.MerchantSystemUtilizationFee);
					return afrmmList;
				});
			}
		}
	}
}
