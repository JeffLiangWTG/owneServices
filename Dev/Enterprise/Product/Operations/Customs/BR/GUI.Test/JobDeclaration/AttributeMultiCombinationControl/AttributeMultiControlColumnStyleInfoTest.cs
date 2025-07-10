using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	public class AttributeMultiControlColumnStyleInfoTest : TestCase
	{
		public void TestColumnStyleType()
		{
			var styleInfoToTest = new AttributeMultiControlColumnStyleInfo();

			var actualValue = styleInfoToTest.ColumnStyleType;
			AssertEquals("The column style has an expected type", typeof(AttributeMultiControlColumnStyle), actualValue);
		}
	}
}
