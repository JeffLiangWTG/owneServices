using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZClientWebCargoWiseEDI;

namespace Testing
{
	public class BulkUpdateModeAsBindableBooleanTest : IBindableBooleanItemTest
	{
		public override void TestText()
		{
			AssertEquals("hi (5)", TestBizObj.Text);
		}

		protected override IBindableBooleanItem GetNewBusinessObject()
		{
			return new BulkUpdateMode("hi", 5);
		}
	}
}