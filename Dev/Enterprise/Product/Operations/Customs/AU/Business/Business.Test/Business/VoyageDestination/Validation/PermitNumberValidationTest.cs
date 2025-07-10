using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class PermitNumberValidationTest : TestCase
	{
		public void TestValidDODPermitNumber1()
		{
			line.AddInfo.ZA_PermitNumbers_Hidden = "DODABC12345678X";
			Assert("No Warning notification", line.AddInfo.ZA_PermitNumbers_HiddenInfo.GetWarnings().GetUniqueMessageList().Length == 0);
		}

		public void TestInvalidPermitNumber1()
		{
			line.AddInfo.ZA_PermitNumbers_Hidden = "AHC1F111";
			Assert("Warning notification", line.AddInfo.ZA_PermitNumbers_HiddenInfo.GetWarnings().GetUniqueMessageList().Length == 1);
		}

		public void TestInvalidPermitNumber2()
		{
			line.AddInfo.ZA_PermitNumbers_Hidden = "PWSB111111";
			Assert("Warning notification", line.AddInfo.ZA_PermitNumbers_HiddenInfo.GetWarnings().GetUniqueMessageList().Length == 1);
		}

		public void TestInvalidFormatOZOCode()
		{
			line.AddInfo.ZA_PermitNumbers_Hidden = "OZO11112";
			Assert("Warning notification", line.AddInfo.ZA_PermitNumbers_HiddenInfo.GetWarnings().GetUniqueMessageList().Length == 1);
		}

		public void TestInvalidPermitNumberWithStar()
		{
			line.AddInfo.ZA_PermitNumbers_Hidden = "DEC*BBB1111111111111";
			Assert("Warning notification", line.AddInfo.ZA_PermitNumbers_HiddenInfo.GetWarnings().GetUniqueMessageList().Length == 1);
		}

		public void TestInvalidMEPPermit()
		{
			line.AddInfo.ZA_PermitNumbers_Hidden = "MEP?111/BBB/111S";
			Assert("Warning notification", line.AddInfo.ZA_PermitNumbers_HiddenInfo.GetWarnings().GetUniqueMessageList().Length == 1);
		}

		public void TestEmptyPermitNumber()
		{
			line.AddInfo.ZA_PermitNumbers_Hidden = "";
			Assert("Warning notification", line.AddInfo.ZA_PermitNumbers_HiddenInfo.GetWarnings().GetUniqueMessageList().Length == 0);
		}

		public void TestShortPermitNumber()
		{
			line.AddInfo.ZA_PermitNumbers_Hidden = "ME";
			Assert("Warning notification", line.AddInfo.ZA_PermitNumbers_HiddenInfo.GetWarnings().GetUniqueMessageList().Length == 1);
		}

		public void TestValidPermitNumber()
		{
			line.AddInfo.ZA_PermitNumbers_Hidden = "";
			Assert("Valid Permt", !line.AddInfo.ZA_PermitNumbers_HiddenInfo.HasWarnings());
		}

		public void TestMultipleValidPermitNumbers()
		{
			line.AddInfo.ZA_PermitNumbers_Hidden = "AHC11111,PWSA111111,PIL12345";
			Assert("Valid permits", !line.AddInfo.ZA_PermitNumbers_HiddenInfo.HasWarnings());
		}

		JobComInvoiceLine line;

		protected override void SetUp()
		{
			base.SetUp();
			var factory = new BusinessObjectFactory();
			var testDec = factory.New<JobDeclaration>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			var invoice = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			line = invoice.JobComInvoiceLines.AddNew();
		}
	}
}
