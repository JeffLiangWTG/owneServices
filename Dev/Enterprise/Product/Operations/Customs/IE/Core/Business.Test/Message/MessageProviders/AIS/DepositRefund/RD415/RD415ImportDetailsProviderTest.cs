using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class RD415ImportDetailsProviderTest : DataProviderTestCase<RD415ImportDetailsProvider>
	{
		public void TestIRD415ImportDetails()
		{
			Assert("Should implement IRD415ImportDetails", Provider is IRD415ImportDetails);
		}

		public void TestProcedure()
		{
			SetUpTestData();
			invoiceLine.JI_Procedure = "4400100";
			AssertEquals("44", Provider.Procedure);

			invoiceLine.JI_Procedure = "4001000";
			AssertEquals("40", Provider.Procedure);
		}

		public void TestGoodsInformations()
		{
			AssertType<RD415GoodsInformationImportProvider[]>(Provider.GoodsInformations);
			AssertEquals("Count", 1, Provider.GoodsInformations.Count);

			entryHeader.MergedLines.AddNew();
			AssertEquals("Count", 2, GetProvider().GoodsInformations.Count);
		}

		public void TestAmountsHeldDeposit()
		{
			var amountsHeldDeposit = Provider.AmountsHeldDeposit;
			AssertType<RD415AmountsHeldDepositProvider>(amountsHeldDeposit);
			AssertSame("Cached", amountsHeldDeposit, Provider.AmountsHeldDeposit);
		}

		public void TestAdditionalInformation()
		{
			var additionalInformation = Provider.AdditionalInformation;
			AssertType<RD415AdditionalInformationProvider>(additionalInformation);
			AssertSame("Cached", additionalInformation, Provider.AdditionalInformation);
		}

		protected override RD415ImportDetailsProvider GetProvider()
		{
			SetUpTestData();
			return new RD415ImportDetailsProvider(sendingAction);
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
