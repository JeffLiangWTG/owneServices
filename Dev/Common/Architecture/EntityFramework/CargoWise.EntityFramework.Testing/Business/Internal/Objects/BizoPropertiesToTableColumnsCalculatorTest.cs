using System.ComponentModel;
using CargoWise.ComponentModel;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class BizoPropertiesToTableColumnsCalculatorTest : TestCase
	{
		public void TestGetBusinessObjectTableColumns_WhenPropertyDescriptorsIsNull()
		{
			AssertNoExceptionThrown(() => BizoPropertiesToTableColumnsCalculator.GetBusinessObjectTableColumns(null));
		}

		public void TestGetBusinessObjectTableColumns_WhenPropertyDescriptorInArrayIsNull()
		{
			PropertyDescriptor[] testArray = { null };
			AssertNoExceptionThrown(() => BizoPropertiesToTableColumnsCalculator.GetBusinessObjectTableColumns(testArray));
		}

		public void TestElementsAreReturned()
		{
			var propertyDescriptor = KPropertyDescriptorCollection.FromType(typeof(string))[0];
			var result = BizoPropertiesToTableColumnsCalculator.GetBusinessObjectTableColumns(null, propertyDescriptor);
			Assert(result.Length == 1);
			AssertEquals("Length", result[0].ColumnName);
		}
	}
}
