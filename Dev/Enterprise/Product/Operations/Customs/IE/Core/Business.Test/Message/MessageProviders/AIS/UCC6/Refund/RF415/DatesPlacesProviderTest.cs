using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	public class DatesPlacesProviderTest : DataProviderTestCase<DatesPlacesProvider>
	{
		[TestDate(2023, 12, 01)]
		public void TestDate()
		{
			SetUpTestData();
			AssertEquals(new DateTime(2023, 12, 01), Provider.Date);
		}

		public void TestOfficeOfDebt()
		{
			SetUpTestData();
			using (sendingAction.GetValidationSuspender())
			{
				sendingAction.OfficeOfDebt = "OOD12345";
				AssertEquals("OOD12345", Provider.OfficeOfDebt);
			}
		}

		public void TestOfficeOfResponsibility()
		{
			SetUpTestData();
			using (sendingAction.GetValidationSuspender())
			{
				sendingAction.OfficeOfResponsibility = "OOR12345";
				AssertEquals("OOR12345", Provider.OfficeOfResponsibility);
			}
		}

		public void TestLocationOfGoods()
		{
			SetUpTestData();
			var location = entryInstruction.GoodsLocation;
			location.CGL_Qualifier = "T";
			AssertType<RF415GoodsLocationProvider>(Provider.LocationOfGoods);
			AssertEquals("QualifierIdentification", "T", Provider.LocationOfGoods.QualifierIdentification);
		}

		protected override DatesPlacesProvider GetProvider()
		{
			SetUpTestData();
			return new DatesPlacesProvider(sendingAction, entryInstruction);
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
				sendingAction = new RefundApplicationMessageSendingAction(entryHeader);
			}
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		RefundApplicationMessageSendingAction sendingAction;
	}
}
