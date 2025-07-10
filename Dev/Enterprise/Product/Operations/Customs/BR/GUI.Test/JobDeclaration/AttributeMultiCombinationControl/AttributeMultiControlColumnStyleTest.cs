using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.GUI.Testing
{
	public class AttributeMultiControlColumnStyleTest : TestCaseWithFactory
	{
		public void TestEditControl()
		{
			using (var columnStyle = new AttributeMultiControlColumnStyle(new AttributeMultiControlColumnStyleInfo()))
			{
				AssertType<AttributeMultiCombinationControl>(columnStyle.EditControl);
			}
		}
	}
}
