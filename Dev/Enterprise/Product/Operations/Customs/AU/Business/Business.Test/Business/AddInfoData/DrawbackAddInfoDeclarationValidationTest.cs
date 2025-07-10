using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class DrawbackAddInfoDeclarationValidationTest : AUAddInfoValidationTest
	{
		public void TestCheckZA_DARC_Hidden()
		{
			AssertEquals("No errors", false, declaration.AddInfo.ZA_DARC_HiddenInfo.HasMessageErrors());

			declaration.AddInfo.ZA_DARC_Hidden = "X";
			AssertEquals("Errors", true, declaration.AddInfo.ZA_DARC_HiddenInfo.HasMessageErrors());

			declaration.AddInfo.ZA_DARC_Hidden = JobDeclaration.DrawbackAmberReasonTypes.Declaration;
			AssertEquals("No Errors", false, declaration.AddInfo.ZA_DARC_HiddenInfo.HasMessageErrors());

			declaration.AddInfo.ZA_DARC_Hidden = ZString.Empty;
			AssertNoMessageErrors(declaration.AddInfo.ZA_DARC_HiddenInfo);

			declaration.AddInfo.ZA_DARC_Hidden = JobDeclaration.DrawbackAmberReasonTypes.Declaration;
			AssertNoMessageErrors(declaration.AddInfo.ZA_DARC_HiddenInfo);

			declaration.JE_AmberStatement = "Foo";
			declaration.AddInfo.ZA_DARC_Hidden = ZString.Empty;
			AssertHasMessageErrors(declaration.AddInfo.ZA_DARC_HiddenInfo);

			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine.AddInfo.ZA_DARC_Hidden = "C";
			declaration.AddInfo.ZA_DARC_Hidden = "C";
			AssertHasMessageErrors(declaration.AddInfo.ZA_DARC_HiddenInfo);

			invoiceLine.AddInfo.ZA_DARC_Hidden = "C";
			declaration.AddInfo.ZA_DARC_Hidden = ZString.Empty;
			AssertNoMessageErrors(declaration.AddInfo.ZA_DARC_HiddenInfo);

			invoiceLine.AddInfo.ZA_DARC_Hidden = "";
			declaration.AddInfo.ZA_DARC_Hidden = ZString.Empty;
			AssertHasMessageErrors(declaration.AddInfo.ZA_DARC_HiddenInfo);

			declaration.AddInfo.ZA_DARC_Hidden = JobDeclaration.DrawbackAmberReasonTypes.Declaration;
			AssertNoMessageErrors(declaration.AddInfo.ZA_DARC_HiddenInfo);
		}

		public void TestCheckZA_DAM_Hidden()
		{
			AssertEquals("No errors", false, declaration.AddInfo.ZA_DAM_HiddenInfo.HasMessageErrors());

			declaration.AddInfo.ZA_DAM_Hidden = "X";
			AssertEquals("Errors", true, declaration.AddInfo.ZA_DAM_HiddenInfo.HasMessageErrors());

			declaration.AddInfo.ZA_DAM_Hidden = JobDeclaration.DrawbackAssessmentMethods.RepresentativeShipment;
			AssertEquals("No Errors", false, declaration.AddInfo.ZA_DAM_HiddenInfo.HasMessageErrors());
		}

		public void TestNoMessageErrorOnPRF()
		{
			declaration.AddInfo.ZA_PRF = "";
			AssertEquals("No Errors", false, declaration.AddInfo.ZA_PRFInfo.HasMessageErrors());
		}

		public void TestEDN()
		{
			declaration.AddInfo.ZA_EDN_Hidden = "";
			AssertEquals("No Error if blank", false, declaration.AddInfo.ZA_EDN_HiddenInfo.HasMessageErrors());
			declaration.AddInfo.ZA_EDN_Hidden = "AAACFMAHX";
			AssertEquals("Valid CAN", false, declaration.AddInfo.ZA_EDN_HiddenInfo.HasMessageErrors());
			declaration.AddInfo.ZA_EDN_Hidden = "AAACFMAHA";
			AssertEquals("Invalid CAN", true, declaration.AddInfo.ZA_EDN_HiddenInfo.HasMessageErrors());
			declaration.AddInfo.ZA_EDN_Hidden = "AAACFMAH";
			AssertEquals("Invalid CAN", true, declaration.AddInfo.ZA_EDN_HiddenInfo.HasMessageErrors());
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoiceHeader = declaration.Invoices.AddNew();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
		}
		JobComInvoiceHeader invoiceHeader;
	}
}
