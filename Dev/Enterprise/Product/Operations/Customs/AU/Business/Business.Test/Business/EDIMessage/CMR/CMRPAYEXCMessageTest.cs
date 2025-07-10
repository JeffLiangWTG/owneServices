using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRPAYEXCMessage))]
	sealed class CMRPAYEXCMessageTest : CMRCUSRESMessageTest
	{
		public void TestGetReport()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_OH_Importer = Factory.New(typeof(OrgHeader)).PK;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();

			CMRPAYEXCMessage pAYEXCMessage = Factory.New<CMRPAYEXCMessage>();
			pAYEXCMessage.EM_LinkedObject = entryHeader;

			ZString report = pAYEXCMessage.GetReport();
			AssertEquals("Report should have an informative message about payment exceeded", true, report.Contains("The Electronic Funds Transfer (EFT) payment for this entry has failed as the EFT limit for the Importer "));
		}
	}
}
