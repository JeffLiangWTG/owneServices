using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentEngine.Testing
{
	static class TestMethodExtensions
	{
		internal static DummyChildBusinessObject AddNew(
			this DummyChildBusinessObjectCollection collection,
			string code,
			string description = "",
			int number = 0,
			decimal decimalValue = 0)
		{
			var result = collection.AddNew();

			result.Z0_Code = code;
			result.Z0_Description = description;
			result.Z0_Number = number;
			result.Z0_Decimal = decimalValue;

			return result;
		}
	}
}
