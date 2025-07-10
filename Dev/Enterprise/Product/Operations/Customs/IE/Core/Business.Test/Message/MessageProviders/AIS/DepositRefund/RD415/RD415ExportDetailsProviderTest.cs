using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class RD415ExportDetailsProviderTest : DataProviderTestCase<RD415ExportDetailsProvider>
	{
		public void TestIRD415ExportDetails()
		{
			Assert("Should implement IRD415ExportDetails", Provider is IRD415ExportDetails);
		}

		public void TestMRN()
		{
			SetUpTestData();
			sendingAction.ExportMovementReferenceNumber = "IE24ROS10012345678";
			AssertEquals("IE24ROS10012345678", Provider.MRN);
		}

		public void TestDate()
		{
			SetUpTestData();
			sendingAction.ExportDate = new CargoWise.Types.ZDateTime(2024, 5, 13, 15, 50, 0);
			AssertEquals(new System.DateTime(2024, 5, 13, 15, 50, 0), Provider.Date);
		}

		public void TestGoodsInformations()
		{
			AssertType<RD415GoodsInformationExportProvider[]>(Provider.GoodsInformations);
			AssertEquals("Count", 1, Provider.GoodsInformations.Count);

			entryHeader.MergedLines.AddNew();
			AssertEquals("Count", 2, GetProvider().GoodsInformations.Count);
		}

		protected override RD415ExportDetailsProvider GetProvider()
		{
			SetUpTestData();
			return new RD415ExportDetailsProvider(sendingAction);
		}

		void SetUpTestData()
		{
			if (sendingAction == null)
			{
				declaration = Factory.New<JobDeclaration>();
				entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				invoice = declaration.Invoices.AddNew();
				invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				sendingAction = new DepositRefundApplicationMessageSendingAction(entryHeader);
			}
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		DepositRefundApplicationMessageSendingAction sendingAction;
	}
}
