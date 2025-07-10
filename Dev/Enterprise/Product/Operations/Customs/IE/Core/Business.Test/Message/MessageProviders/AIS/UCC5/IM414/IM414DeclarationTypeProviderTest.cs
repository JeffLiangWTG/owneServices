using System;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	sealed class IM414DeclarationTypeProviderTest : DataProviderTestCase<IM414DeclarationTypeProvider>
	{
		public void TestIIM414DeclarationType()
		{
			Assert("Should implement IIM414DeclarationType", Provider is IIM414DeclarationType);
		}

		public void TestMRN()
		{
			SetUpTestData();
			entryHeader.MovementReferenceNumberSetter("12MRN345ABCDE678R9");
			AssertEquals("12MRN345ABCDE678R9", Provider.MRN);
		}

		[TestDate(2024, 04, 03, 10, 05, 35)]
		public void TestDateOfInvalidationRequest()
		{
			AssertEquals(new DateTime(2024, 04, 03, 10, 05, 35), Provider.DateOfInvalidationRequest);
		}

		public void TestInvalidationReason()
		{
			SetUpTestData();
			sendingAction.Annotation = "Invalidation Reason";
			AssertEquals("Invalidation Reason", Provider.InvalidationReason);
		}

		public void TestCustomsOffices()
		{
			var customsOffices = Provider.CustomsOffices;
			AssertType<DeclarationTypeCustomsOfficesProvider>(customsOffices);
			AssertSame("Cached", customsOffices, Provider.CustomsOffices);
		}

		public void TestParties()
		{
			var parties = Provider.Parties;
			AssertType<IM414DeclarationTypePartiesProvider>(parties);
			AssertSame("Cached", parties, Provider.Parties);
		}

		protected override IM414DeclarationTypeProvider GetProvider()
		{
			SetUpTestData();
			return new IM414DeclarationTypeProvider(headerProvider.entryHeaderWrapper, headerProvider.PreparationDateAndTime, sendingAction);
		}

		void SetUpTestData()
		{
			if (headerProvider == null)
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
				sendingAction = new AISUCC5MessageSendingAction(entryHeader);
				headerProvider = new IM414HeaderProvider(sendingAction);
			}
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		AISUCC5MessageSendingAction sendingAction;
		IM414HeaderProvider headerProvider;
	}
}
