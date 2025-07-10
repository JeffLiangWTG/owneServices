using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class DrawbackAddinfoInvHeaderValidationTest : AUAddInfoValidationTest
	{
		public void TestCheckZA_DAM_Hidden()
		{
			invoice.AddInfo.ZA_DAM_Hidden = "X";
			AssertEquals("Errors", true, invoice.AddInfo.ZA_DAM_HiddenInfo.HasMessageErrors());

			invoice.AddInfo.ZA_DAM_Hidden = JobDeclaration.DrawbackAssessmentMethods.RepresentativeShipment;
			AssertEquals("No Errors", false, invoice.AddInfo.ZA_DAM_HiddenInfo.HasMessageErrors());
		}

		public void TestEDN()
		{
			invoice.AddInfo.ZA_EDN_Hidden = "";
			AssertEquals("Error if blank", true, invoice.AddInfo.ZA_EDN_HiddenInfo.HasMessageErrors());
			invoice.AddInfo.ZA_EDN_Hidden = "AAACFMAHX";
			AssertEquals("Valid CAN", false, invoice.AddInfo.ZA_EDN_HiddenInfo.HasMessageErrors());
			invoice.AddInfo.ZA_EDN_Hidden = "AAACFMAHA";
			AssertEquals("Invalid CAN", true, invoice.AddInfo.ZA_EDN_HiddenInfo.HasMessageErrors());
			invoice.AddInfo.ZA_EDN_Hidden = "AAACFMAH";
			AssertEquals("Invalid CAN", true, invoice.AddInfo.ZA_EDN_HiddenInfo.HasMessageErrors());
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			JobComInvoiceGroupHeader header = declaration.JobComInvoiceGroupHeaders[0];
			invoice = header.JobComInvoiceHeaders.AddNew();
			addInfo = invoice.AddInfo;
		}
		JobComInvoiceHeader invoice;
	}
}
