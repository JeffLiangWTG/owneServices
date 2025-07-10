using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIOrgCodeListsTest : TestCaseWithFactory
	{
		public void TestContactTypeList()
		{
			var list = OrgCodeLists.ContactType_List;

			foreach (CodeDescriptionPair p in new EDIOrgDocumentGroupTypes())
			{
				Assert(list.ContainsCode(p.Code));
			}

			AssertEquals("No duplicates", true, list.Cast<CodeDescriptionPair>().GroupBy(x => x.Code).All(x => x.Count() == 1));
		}
	}
}
