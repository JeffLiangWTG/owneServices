using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	public abstract class MyAccountWebContractEmailParserTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestParse()
		{
			MyAccountWebContractEmailParser parser = new MyAccountWebContractEmailParser(Factory);
			MyAccountWebContract webContract = GetMyAccountWebContractForTest(Factory.New<OrgContact>());
			parser.Parse(webContract, "");
		}

		protected abstract MyAccountWebContract GetMyAccountWebContractForTest(OrgContact contact);
	}

	public class MyAccountWebContractEmailParserOrgLevelTest : MyAccountWebContractEmailParserTest
	{
		protected override MyAccountWebContract GetMyAccountWebContractForTest(OrgContact contact) => new EDIOrgHeaderWebContract(contact);
	}

	public class MyAccountWebContractEmailParserContactLevelTest : MyAccountWebContractEmailParserTest
	{
		protected override MyAccountWebContract GetMyAccountWebContractForTest(OrgContact contact) => new EDIOrgContactWebContract(contact);
	}
}