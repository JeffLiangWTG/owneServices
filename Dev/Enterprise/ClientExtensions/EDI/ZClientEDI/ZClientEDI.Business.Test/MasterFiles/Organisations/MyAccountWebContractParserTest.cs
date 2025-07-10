using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	public abstract class MyAccountWebContractParserTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestParse()
		{
			MyAccountWebContractParser parser = new MyAccountWebContractParser(Factory);
			MyAccountWebContract webContract = GetMyAccountWebContractForTest(Factory.New<OrgContact>());
			parser.Parse(webContract, "");
		}

		protected abstract MyAccountWebContract GetMyAccountWebContractForTest(OrgContact contact);
	}

	public class MyAccountWebContractParserOrgLevelTest : MyAccountWebContractParserTest
	{
		protected override MyAccountWebContract GetMyAccountWebContractForTest(OrgContact contact) => new EDIOrgHeaderWebContract(contact);
	}

	public class MyAccountWebContractParserContactLevelTest : MyAccountWebContractParserTest
	{
		protected override MyAccountWebContract GetMyAccountWebContractForTest(OrgContact contact) => new EDIOrgContactWebContract(contact);
	}
}