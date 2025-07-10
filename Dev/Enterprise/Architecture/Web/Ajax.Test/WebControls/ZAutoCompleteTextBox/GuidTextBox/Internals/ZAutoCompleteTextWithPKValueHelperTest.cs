using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.GUI.WebControls.GuidTextBox.Internals;

namespace Enterprise.ZArchitecture.Web.GUI.Ajax.Testing
{
	sealed class ZAutoCompleteTextWithPKValueHelperTest : TestCaseWithFactory
	{
		public void TestGetPK()
		{
			ZGuid testGuid = ZGuid.NewZGuid();
			string testString = string.Format("some kind - of informa-tio{0}n whi\th diffirent stuff in it", testGuid.ToString());
			ZGuid result = (ZGuid)testString.TryGetKey();
			AssertEquals(testGuid, result);
		}

		public void TestGetValue()
		{
			ZString value = "THE FABRIC & GROUP";
			ZString code = "Code1";
			ZString valueWithPK = ZAutoCompleteTextWithGuidValueHelper.GetValueWithHiddenPK(code, value);
			ZString resultValue = valueWithPK.ToString().GetValue();
			AssertEquals("THE FABRIC &amp; GROUP", resultValue);
		}
	}
}
