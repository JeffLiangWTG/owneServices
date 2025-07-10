using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	public abstract class ColumnValueRankerBaseTest : TestCaseWithFactory
	{
		protected string SpecificCode = "COD1";
		protected string SpecificDescription = "Very Long Description";
		protected ZDateTime SpecificDate = ZDateTime.Today;
	}
}
