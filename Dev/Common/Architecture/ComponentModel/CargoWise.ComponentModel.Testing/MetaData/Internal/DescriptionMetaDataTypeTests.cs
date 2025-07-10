#if DEBUG
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class DescriptionMetaDataTypeTests : TestCase
	{
		public void TestGetDescriptionOfMaxLength()
		{
			DescriptionMetaDataType info = new DescriptionMetaDataType(MetaDataTypes.Description, null);
			TestDescription invalidDescription1 = new TestDescription();
			TestDescription invalidDescription2 = new TestDescription("a", "bb", "a");
			TestDescription invalidDescription3 = new TestDescription("a", "bb", "ccc", "ddd");
			TestDescription validDescription = new TestDescription("a", "bb", "ccc", "dddd");

			AssertNotNull(info.ValidateMetaDataValue(null, null, invalidDescription1));
			AssertNotNull(info.ValidateMetaDataValue(null, null, invalidDescription2));
			AssertNotNull(info.ValidateMetaDataValue(null, null, invalidDescription3));
			AssertNull(info.ValidateMetaDataValue(null, null, validDescription));
		}

		class TestDescription : IDescription
		{
			readonly string[] descriptions;

			public TestDescription(params string[] descriptions)
			{ this.descriptions = descriptions; }

			public int Count
			{ get { return descriptions.Length; } }

			public string GetDescription(int index)
			{ return descriptions[index]; }

			public string GetDescription(int index, System.Globalization.CultureInfo culture)
			{ return descriptions[index]; }
		}
	}
}
#endif
