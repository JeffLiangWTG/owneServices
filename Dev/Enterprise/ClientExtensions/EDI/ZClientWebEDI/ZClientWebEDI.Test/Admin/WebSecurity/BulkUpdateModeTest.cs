using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZClientWebCargoWiseEDI;
using NUnit.Framework;

namespace Testing
{
	[TestedType(typeof(BulkUpdateMode))]
	public class BulkUpdateModeTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new BulkUpdateMode(string.Empty, 0);
		}

		public void TestDisplayName()
		{
			AssertEquals("disp", new BulkUpdateMode("disp", 1).DisplayName);
		}

		public void TestText()
		{
			AssertEquals("disp (1)", new BulkUpdateMode("disp", 1).Text);
		}

		public void TestModeCode()
		{
			AssertEquals("ALL", new BulkUpdateMode("1", 3, BulkUpdateModeCodes.AllContacts).ModeCode);
			AssertEquals("SEL", new BulkUpdateMode("2", 4, BulkUpdateModeCodes.SelectedContacts).ModeCode);
		}
	}
}
