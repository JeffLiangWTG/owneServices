using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class DEOfficeCode : EuOfficeCode
	{
		public DEOfficeCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override CusCodeDataValidation GetNewValidation() => new DEOfficeCodeValidation(this);

		protected override CusCodeDataLookups GetNewLookups() => new DEOfficeCodeLookups(this);
	}
}
