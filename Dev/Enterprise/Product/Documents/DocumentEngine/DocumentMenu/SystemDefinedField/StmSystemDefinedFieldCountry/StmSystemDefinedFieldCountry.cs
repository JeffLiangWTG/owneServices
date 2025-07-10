using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.SDF
{
	public class StmSystemDefinedFieldCountry : AutoStmSystemDefinedField
	{
		public StmSystemDefinedFieldCountry(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override StmSystemDefinedFieldValidation GetNewValidation()
		{
			return new StmSystemDefinedFieldCountryValidation(this);
		}
	}
}
