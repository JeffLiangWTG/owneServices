using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class DrawbackAddinfoInvLineValidationTest : AUAddInfoValidationTest
	{
		public void TestCheckZA_DARC_Hidden()
		{
			invoiceLine.AddInfo.ZA_DARC_Hidden = "";
			AssertEquals("No errors", false, invoiceLine.AddInfo.ZA_DARC_HiddenInfo.HasMessageErrors());

			invoiceLine.AddInfo.ZA_DARC_Hidden = "X";
			AssertHasMessageErrorContaining(invoiceLine.AddInfo.ZA_DARC_HiddenInfo, "The code you have selected is not in the list");

			invoiceLine.AddInfo.ZA_DARC_Hidden = JobDeclaration.DrawbackAmberReasonTypes.Declaration;
			AssertEquals("No Errors", false, invoiceLine.AddInfo.ZA_DARC_HiddenInfo.HasMessageErrors());
		}

		public void TestCheckZA_DAM_Hidden()
		{
			invoiceLine.AddInfo.ZA_DAM_Hidden = "";
			AssertHasMessageErrorContaining(invoiceLine.AddInfo.ZA_DAM_HiddenInfo, "You have not entered a " + invoiceLine.AddInfo.ZA_DAM_HiddenInfo.Description);

			invoiceLine.AddInfo.ZA_DAM_Hidden = "X";
			AssertHasMessageErrorContaining(invoiceLine.AddInfo.ZA_DAM_HiddenInfo, "The code you have selected is not in the list");

			invoiceLine.AddInfo.ZA_DAM_Hidden = JobDeclaration.DrawbackAssessmentMethods.RepresentativeShipment;
			AssertEquals("No Errors", false, invoiceLine.AddInfo.ZA_DAM_HiddenInfo.HasMessageErrors());
		}

		public void TestNoMessageErrorOnPRF()
		{
			invoiceLine.AddInfo.ZA_PRF = "";
			AssertEquals("No Errors", false, invoiceLine.AddInfo.ZA_PRFInfo.HasMessageErrors());
		}

		public void TestImportDecNumber()
		{
			invoiceLine.AddInfo.ZA_DDN_Hidden = "";
			AssertEquals("No Errors if blank", false, invoiceLine.AddInfo.ZA_DDN_HiddenInfo.HasMessageErrors());
			invoiceLine.AddInfo.ZA_DDN_Hidden = "AAACFGRAC";
			AssertEquals("Valid Dec Number", false, invoiceLine.AddInfo.ZA_DDN_HiddenInfo.HasMessageErrors());
			invoiceLine.AddInfo.ZA_DDN_Hidden = "AAACFGRAX";
			AssertEquals("Invalid Dec Number", true, invoiceLine.AddInfo.ZA_DDN_HiddenInfo.HasMessageErrors());
			invoiceLine.AddInfo.ZA_DDN_Hidden = "AAACFGRA";
			AssertEquals("Invalid Dec Number", true, invoiceLine.AddInfo.ZA_DDN_HiddenInfo.HasMessageErrors());
		}

		public void TestEDN()
		{
			invoiceLine.AddInfo.ZA_EDN_Hidden = "";
			AssertEquals("Error if blank", true, invoiceLine.AddInfo.ZA_EDN_HiddenInfo.HasMessageErrors());
			invoiceLine.AddInfo.ZA_EDN_Hidden = "AAACFMAHX";
			AssertEquals("Valid CAN", false, invoiceLine.AddInfo.ZA_EDN_HiddenInfo.HasMessageErrors());
			invoiceLine.AddInfo.ZA_EDN_Hidden = "AAACFMAHA";
			AssertEquals("Invalid CAN", true, invoiceLine.AddInfo.ZA_EDN_HiddenInfo.HasMessageErrors());
			invoiceLine.AddInfo.ZA_EDN_Hidden = "AAACFMAH";
			AssertEquals("Invalid CAN", true, invoiceLine.AddInfo.ZA_EDN_HiddenInfo.HasMessageErrors());
		}

		public void TestZA_DDNandCollectionAtSameTime()
		{
			invoiceLine.AddInfo.ZA_DAM_Hidden = JobDeclaration.DrawbackAssessmentMethods.RepresentativeShipment;
			invoiceLine.AddInfo.ZA_DDN_Hidden = "ABC";
			AssertNoErrors(invoiceLine.AddInfo.ZA_DDN_HiddenInfo);
			invoiceLine.DrawbackCusEntryLineCollection.AddNew();
			invoiceLine.AddInfo.ZA_DDN_Hidden = "";
			AssertNoErrors(invoiceLine.AddInfo.ZA_DDN_HiddenInfo);
			invoiceLine.AddInfo.ZA_DDN_Hidden = "ABC";
			AssertHasErrors(invoiceLine.AddInfo.ZA_DDN_HiddenInfo);
			invoiceLine.AddInfo.ZA_DAM_Hidden = JobDeclaration.DrawbackAssessmentMethods.ActualShipment;
			invoiceLine.AddInfo.ZA_DDN_Hidden = "XYZ";
			AssertNoErrors(invoiceLine.AddInfo.ZA_DDN_HiddenInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			var header = declaration.JobComInvoiceGroupHeaders[0];
			invoice = header.JobComInvoiceHeaders.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			addInfo = invoiceLine.AddInfo;
		}
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
	}
}
