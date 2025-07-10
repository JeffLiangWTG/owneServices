using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class CusEntryLineConfirmedFeeCollection : NonPersistentBusinessObjectCollection<CusEntryLineConfirmedFee>
	{
		public CusEntryLineConfirmedFeeCollection(CusEntryLine entryLine)
		{
			if (entryLine != null)
			{ 
				Add(new CusEntryLineConfirmedFee(Res.GetString("D9C79652-0C95-4BDC-BFDE-85081E825F3D", "Customs Value"), entryLine.CL_ConfirmedCustomsValue, entryLine.LocalCurrency));
				Add(new CusEntryLineConfirmedFee(Res.GetString("0024FDA1-C507-4904-A3A2-64A97DD22EEF", "Stat. Value"), entryLine.CL_ConfirmedStatisticalValue, entryLine.LocalCurrency));
				Add(new CusEntryLineConfirmedFee(Res.GetString("9B15906F-2856-4F92-BF47-1BF73E79321C", "VAT Value"), entryLine.CL_ConfirmedValueForVAT, entryLine.LocalCurrency));
				Add(new CusEntryLineConfirmedFee(Res.GetString("F050682F-2AF2-4FDF-B3F3-79DEAA5D7ACA", "CIF Value"), entryLine.CL_ConfirmedCIFValue, entryLine.LocalCurrency));
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CusEntryLineConfirmedFee(ZString.Empty, ZDecimal.Zero, ZString.Empty);
		}
	}
}
