using NUnit.Framework;

namespace Enterprise.Client.OIA.Business.Testing
{
	public class OIAGLHeadingDataRowTest : TestCase
	{
		public void TestProperties()
		{
			OIAGLHeadingDataRow row = new OIAGLHeadingDataRow();
			AssertEquals("Row should have 4 fields", 4, row.FieldCount);
			AssertEquals("LocalClientOrgCode", "LocalClient", row.GetField(OIAGLDataRow.Schema.LocalClientOrgCode));
			AssertEquals("ARAccountGroup", "ARGroup", row.GetField(OIAGLDataRow.Schema.ARAccountGroup));
			AssertEquals("Customisable1", "Text1", row.GetField(OIAGLDataRow.Schema.CustomisableText1));
			AssertEquals("Customisable2", "Text2", row.GetField(OIAGLDataRow.Schema.CustomisableText2));
		}
	}
}
