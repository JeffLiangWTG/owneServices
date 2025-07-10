#if DEBUG
using System.ComponentModel;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class DateTimeOffsetPlacesAttributeTests : TestCase
	{
		public void TestGetMetaDataValue()
		{
			KPropertyDescriptor property = (KPropertyDescriptor)TypeDescriptor.GetProperties(new TestComponent())["PropertyWithConstantDateTimeOffsetPlaces"];
			int dateTimeOffsetPlacesConstant = MetaData.GetDateTimeOffsetPlaces(new TestComponent(), property);
			AssertEquals("Constant DateTimeOffsetPlaces value", 1, dateTimeOffsetPlacesConstant);
		}

		public void TestGetMetaDataMember()
		{
			KPropertyDescriptor property = (KPropertyDescriptor)TypeDescriptor.GetProperties(new TestComponent())["PropertyWithCalculatedDateTimeOffsetPlaces"];
			var comp = new TestComponent();
			int value = MetaData.GetDateTimeOffsetPlaces(comp, property);
			AssertEquals("Calculated DateTimeOffsetPlaces value", 5, value);
			comp.PropertyDateTimeOffsetPlaces = 3;
			value = MetaData.GetDateTimeOffsetPlaces(comp, property);
			AssertEquals("Calculated DateTimeOffsetPlaces value", 3, value);
		}

		internal class TestComponent : KComponent
		{
			public TestComponent()
			{
				PropertyDateTimeOffsetPlaces = 5;
			}

			[DateTimeOffsetPlaces(1)]
			public string PropertyWithConstantDateTimeOffsetPlaces
			{ get { return ""; } }

			[DateTimeOffsetPlaces("PropertyDateTimeOffsetPlaces")]
			public string PropertyWithCalculatedDateTimeOffsetPlaces
			{ get { return ""; } }

			public int PropertyDateTimeOffsetPlaces
			{ get; set; }
		}
	}
}
#endif
