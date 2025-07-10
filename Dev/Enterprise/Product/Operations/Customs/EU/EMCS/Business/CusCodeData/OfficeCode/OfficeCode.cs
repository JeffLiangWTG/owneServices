using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class OfficeCode : EuOfficeCode
	{
		public OfficeCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override CusCodeDataLookups GetNewLookups() => new OfficeCodeLookups(this);

		protected override CusCodeDataValidation GetNewValidation() => new OfficeCodeValidation(this);

		public new OfficeCodeLookups Lookups => (OfficeCodeLookups)base.Lookups;

		public new OfficeCodeValidation Validation => (OfficeCodeValidation)base.Validation;
	}
}
