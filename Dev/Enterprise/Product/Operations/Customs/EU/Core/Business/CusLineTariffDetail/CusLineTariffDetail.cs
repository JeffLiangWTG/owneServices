using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business
{
	public partial class CusLineTariffDetail : AutoCusLineTariffDetail
	{
		public CusLineTariffDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new CusLineTariffDetailValidation Validation => (CusLineTariffDetailValidation)base.Validation;

		protected override Customs.Business.CusLineTariffDetailValidation GetNewValidation()
		{
			return new CusLineTariffDetailValidation(this);
		}

		public new CusLineTariffDetailLookups Lookups => (CusLineTariffDetailLookups)base.Lookups;

		protected override Customs.Business.CusLineTariffDetailLookups GetNewLookups()
		{
			return new CusLineTariffDetailLookups(this);
		}
	}
}


