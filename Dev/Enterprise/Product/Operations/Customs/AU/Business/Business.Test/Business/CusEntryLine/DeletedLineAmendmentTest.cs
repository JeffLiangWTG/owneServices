using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class DeletedLineAmendmentTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CL_LineNumber", (short)1, amendment.CL_LineNumber);
				AssertEquals("ActionCodeForMessage", LineAction.Delete, amendment.ActionCodeForMessage);
				AssertEquals("Refund reason should be 126A", "126A", amendment.RefundReasonCode);
				AssertEquals("WAR", ZString.Empty, amendment.WAR);
				AssertEquals("Import Permit Numbers", Enumerable.Empty<ZString>(), amendment.ImportPermitNumbers);
			});
		}

		#region Implemenation

		protected override void SetUp()
		{
			base.SetUp();

			declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			OrgHeader orgHeader = OrgHeader.New(Factory);
			orgHeader.OH_FullName = "Test";
			orgHeader.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientID, "AAA3336347E");

			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			header.JZ_OH_Supplier = orgHeader.PK;

			JobComInvoiceLine invoiceLine = header.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2003.30.40 05";
			invoiceLine.JI_LinePrice = 123.45M;
			invoiceLine.JI_CustomsUnitQty = "LA";
			invoiceLine.JI_CustomsQuantity = 300M;
			invoiceLine.JI_IsPackToBondForLine = true;
			invoiceLine.AddInfo.ZA_WRU = "NO";
			invoiceLine.AddInfo.ZA_WRQ = 500M;
			invoiceLine.JI_IsPackToBondForLine = true;
			invoiceLine.AddInfo.ZA_TILV = "123.45USD";
			invoiceLine.AddInfo.ZA_ISS = 40m;
			declaration.DoMerge();

			CusEntryLine line = declaration.CustomsEntryHeaders[0].AllEntryLines[0];
			line.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.DeletePending;
			line.RefundReasonCode = DeletedLineAmendment.RefundReasonForDeletedLines;
			amendment = new DeletedLineAmendment(line, declaration.CustomsEntryHeaders[0]);
		}
		ICusEntryLine amendment;
		JobDeclaration declaration;

		#endregion
	}
}
