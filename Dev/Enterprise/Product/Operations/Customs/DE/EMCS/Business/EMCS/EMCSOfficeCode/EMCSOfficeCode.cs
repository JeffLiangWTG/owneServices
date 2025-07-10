using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class EMCSOfficeCode : OfficeCode
	{
		public EMCSOfficeCode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new EuOfficeCodeLookups Lookups => (EMCSOfficeCodeLookups)base.Lookups;

		public new EuOfficeCodeValidation Validation => (EMCSOfficeCodeValidation)base.Validation;

		protected override CusCodeDataLookups GetNewLookups() => new EMCSOfficeCodeLookups(this);

		protected override CusCodeDataValidation GetNewValidation() => new EMCSOfficeCodeValidation(this);
	}
}
