using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace CargoWise.Bi.Product.Manager.Business
{
	[TestExcludeBusinessObjectsAllHaveTestCases]
	public class EdwTestTable : EdwTable
	{
		public EdwTestTable(ZString schema, ZString name)
			: base(schema, name)
		{ }
	}
}
