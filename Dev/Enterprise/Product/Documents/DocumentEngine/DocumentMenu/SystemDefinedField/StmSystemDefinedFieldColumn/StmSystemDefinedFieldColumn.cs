using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.SDF
{
	public class StmSystemDefinedFieldColumn : StmSystemDefinedFieldBase
	{
		public StmSystemDefinedFieldColumn(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override StmSystemDefinedFieldValidation GetNewValidation()
		{
			return new StmSystemDefinedFieldColumnValidation(this);
		}

		protected override StmSystemDefinedFieldLookups GetNewLookups()
		{
			return new StmSystemDefinedFieldColumnLookups(this);
		}
	}
}
