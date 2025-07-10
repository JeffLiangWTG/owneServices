using System.Collections;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.JAS.Business.Testing
{
	public class JASPredefinedNoteTypesTest : TestCaseWithFactory
	{
		public void TestInstance()
		{
			AssertEquals("JASPredefinedNoteTypes.Instance should return the correct type", typeof(JASPredefinedNoteTypes), JASPredefinedNoteTypes.Instance.GetType());
		}

		public void TestJXCExportLog()
		{
			AssertEquals("Should exist in the 'All' array", true, ((IList)JASPredefinedNoteTypes.Instance.All).Contains(JASPredefinedNoteTypes.Instance.JXCExportLog));
			AssertEquals("Description", "JXC Export Log", JASPredefinedNoteTypes.Instance.JXCExportLog.Description);
			AssertEquals("Should be Private", StmNoteVisibility.PRV, JASPredefinedNoteTypes.Instance.JXCExportLog.DefaultVisibility);
			AssertEquals("Should be IsTextOnly=true", true, JASPredefinedNoteTypes.Instance.JXCExportLog.IsTextOnly);
			AssertEquals("Should be IsReadOnlyAfterAdd=true", true, JASPredefinedNoteTypes.Instance.JXCExportLog.IsReadOnlyAfterAdd);
		}
	}
}
