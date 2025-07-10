using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class CusEntryHeaderChargesLookups : Customs.Business.CusEntryHeaderChargesLookups
	{
		public CusEntryHeaderChargesLookups(AutoCusEntryHeaderCharges parent) : base(parent)
		{
		}

		protected new CusEntryHeaderCharges Parent => (CusEntryHeaderCharges)base.Parent;

		public CodeDescriptionPairList RateOverrideReasonCodeList => Factory.GetCachedValue<RateOverrideReasonList>();

		public CodeDescriptionPairList NationalFeeTypeCodeList
		{
			get
			{
				var date = Parent.EntryHeader?.EntryInstruction?.CEI_DateForDuty ?? ZDateTime.Empty;
				date = date.IsEmpty ? ZDateTime.Today : date;

				return RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.France, UniversalReferenceConstants.RefCusCodeListTypes.Codes.FrenchNationalTaxCode, date, includeParentDataGrouping: false);
			}
		}
	}
}
