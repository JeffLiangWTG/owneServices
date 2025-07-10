using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class CusPersonCountry : Customs.Business.CusPersonCountry
	{
		public CusPersonCountry(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CPC_RN_NKCountry = Core.Constants.CountryCodes.KoreaSouth;
		}
	}
}
