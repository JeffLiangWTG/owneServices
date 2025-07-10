using System;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class RD415ProviderTest : DataProviderTestCase<RD415Provider>
	{
		public void TestIRD415Header()
		{
			Assert("Should implement IRD415Header", Provider is IRD415Header);
		}

		public void TestHeader()
		{
			var header = Provider.Header;
			AssertType<RD415HeaderTypeProvider>(Provider.Header);
			AssertSame("Cached", header, Provider.Header);
		}

		public void TestImportDetails()
		{
			var importDetails = Provider.ImportDetails;
			AssertType<RD415ImportDetailsProvider>(importDetails);
			AssertSame("Cached", importDetails, Provider.ImportDetails);
		}

		public void TestExportDetails()
		{
			var exportDetails = Provider.ExportDetails;
			AssertType<RD415ExportDetailsProvider>(exportDetails);
			AssertSame("Cached", exportDetails, Provider.ExportDetails);
		}

		public void TestOtherMethodOfDischarges()
		{
			AssertEquals(Array.Empty<IRD415OtherDocumentsForDischarge>(), Provider.OtherMethodOfDischarges);
		}

		public void TestDepositRefundDetails()
		{
			var depositRefundDetails = Provider.DepositRefundDetails;
			AssertType<RD415DepositRefundDetailsProvider>(depositRefundDetails);
			AssertSame("Cached", depositRefundDetails, Provider.DepositRefundDetails);
		}

		public void TestFallbackProcedure()
		{
			AssertNull(Provider.FallbackProcedure);
		}

		protected override RD415Provider GetProvider()
		{
			SetUpTestData();
			return new RD415Provider(sendingAction);
		}

		void SetUpTestData()
		{
			if (sendingAction == null)
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				var entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				sendingAction = new DepositRefundApplicationMessageSendingAction(entryHeader);
			}
		}

		DepositRefundApplicationMessageSendingAction sendingAction;
	}
}
