using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DeltaIECusEntryLineFeeLookups : CusEntryLineFeeLookups
	{
		public DeltaIECusEntryLineFeeLookups(CusEntryLineFee parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList NationalFeeTypeCodeList
		{
			get
			{
				var date = Parent?.EntryLine?.Header?.EntryInstruction?.CEI_DateForDuty ?? ZDateTime.Empty;
				date = date.IsEmpty ? ZDateTime.Today : date;

				return RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.France, UniversalReferenceConstants.RefCusCodeListTypes.Codes.FrenchNationalTaxCode, date, includeParentDataGrouping: false);
			}
		}
	}
}
