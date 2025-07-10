using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.Business
{
	public class OfficeCode : EuOfficeCode
	{
		public OfficeCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public JobDeclaration Declaration => Parent as JobDeclaration;

		public new OfficeCodeValidation Validation => (OfficeCodeValidation)base.Validation;

		protected override CusCodeDataValidation GetNewValidation() => new OfficeCodeValidation(this);

		public new OfficeCodeLookups Lookups => (OfficeCodeLookups)base.Lookups;

		protected override CusCodeDataLookups GetNewLookups() => new OfficeCodeLookups(this);
	}
}
