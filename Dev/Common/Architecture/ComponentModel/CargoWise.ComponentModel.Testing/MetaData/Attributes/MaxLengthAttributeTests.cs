#if DEBUG
using System.ComponentModel;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class MaxLengthAttributeTests : TestCase
	{
		public void TestGetMetaDataValue()
		{
			KPropertyDescriptor property = (KPropertyDescriptor)TypeDescriptor.GetProperties(new TestComponent())["PropertyWithConstantMaxLength"];
			int maxLengthConstant = MetaData.GetMaxLength(new TestComponent(), property);
			AssertEquals("Constant MaxLength value", 1, maxLengthConstant);
		}

		public void TestGetMetaDataMember()
		{
			KPropertyDescriptor property = (KPropertyDescriptor)TypeDescriptor.GetProperties(new TestComponent())["PropertyWithCalculatedMaxLength"];
			int maxLengthConstant = MetaData.GetMaxLength(new TestComponent(), property);
			AssertEquals("Calculated MaxLength value", 5, maxLengthConstant);
		}

		internal class TestComponent : KComponent
		{
			[MaxLength(1)]
			public string PropertyWithConstantMaxLength
			{ get { return ""; } }

			[MaxLength("PropertyMaxLength")]
			public string PropertyWithCalculatedMaxLength
			{ get { return ""; } }

			public int PropertyMaxLength
			{ get { return 5; } }
		}
	}
}
#endif
