#if DEBUG
using System.ComponentModel;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class DecimalPlacesAttributeTests : TestCase
	{
		public void TestGetMetaDataValue()
		{
			KPropertyDescriptor property = (KPropertyDescriptor)TypeDescriptor.GetProperties(new TestComponent())["PropertyWithConstantDecimalPlaces"];
			int decimalPlacesConstant = MetaData.GetDecimalPlaces(new TestComponent(), property);
			AssertEquals("Constant DecimalPlaces value", 1, decimalPlacesConstant);
		}

		public void TestGetMetaDataMember()
		{
			KPropertyDescriptor property = (KPropertyDescriptor)TypeDescriptor.GetProperties(new TestComponent())["PropertyWithCalculatedDecimalPlaces"];
			var comp = new TestComponent();
			int value = MetaData.GetDecimalPlaces(comp, property);
			AssertEquals("Calculated DecimalPlaces value", 5, value);
			comp.PropertyDecimalPlaces = 3;
			value = MetaData.GetDecimalPlaces(comp, property);
			AssertEquals("Calculated DecimalPlaces value", 3, value);
		}

		internal class TestComponent : KComponent
		{
			public TestComponent()
			{
				PropertyDecimalPlaces = 5;
			}

			[DecimalPlaces(1)]
			public string PropertyWithConstantDecimalPlaces
			{ get { return ""; } }

			[DecimalPlaces("PropertyDecimalPlaces")]
			public string PropertyWithCalculatedDecimalPlaces
			{ get { return ""; } }

			public int PropertyDecimalPlaces
			{ get; set; }
		}
	}
}
#endif
