#if DEBUG
using System.ComponentModel;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class DecimalPrecisionAttributeTests : TestCase
	{
		public void TestGetMetaDataValue()
		{
			var property = (KPropertyDescriptor)TypeDescriptor.GetProperties(new TestComponent())["PropertyWithConstantDecimalPrecision"];
			int decimalPrecisionConstant = MetaData.GetDecimalPrecision(new TestComponent(), property);
			AssertEquals("Constant DecimalPrecision value", 6, decimalPrecisionConstant);
		}

		public void TestGetMetaDataMember()
		{
			var property = (KPropertyDescriptor)TypeDescriptor.GetProperties(new TestComponent())["PropertyWithCalculatedDecimalPrecision"];
			var comp = new TestComponent();
			int value = MetaData.GetDecimalPrecision(comp, property);
			AssertEquals("Calculated DecimalPrecision value", 5, value);
			comp.PropertyDecimalPrecision = 3;
			value = MetaData.GetDecimalPrecision(comp, property);
			AssertEquals("Calculated DecimalPrecision value", 3, value);
		}

		internal class TestComponent : KComponent
		{
			public TestComponent()
			{
				PropertyDecimalPrecision = 5;
			}

			[DecimalPrecision(6)]
			public string PropertyWithConstantDecimalPrecision
			{ get { return ""; } }

			[DecimalPrecision("PropertyDecimalPrecision")]
			public string PropertyWithCalculatedDecimalPrecision
			{ get { return ""; } }

			public int PropertyDecimalPrecision
			{ get; set; }
		}
	}
}
#endif
