using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(NFeEntryExportObject))]
	class NFeEntryExportObjectTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2023, 1, 1)]
		public void TestNFeEntryExportObject()
		{
			entryHeader.MovementReferenceNumberSetter("2000010001", ZDateTime.Today);
			entryHeader.CH_BGMReference = "000001";
			entryHeader.CH_Status = BRMessageStatusList.Codes.Accepted;
			entryHeader.CH_EntryReleaseDate = ZDateTime.Today.AddDays(-1);

			var nfe = GetNewBusinessObject() as NFeEntryExportObject;
			CombineAssertions(() =>
			{
				AssertEquals("EntryNumber should be", "2000010001", nfe.EntryNumber);
				AssertEquals("EntryNumberIssueDate should be", ZDateTime.Today, nfe.EntryNumberIssueDate);
				AssertEquals("ReleaseDate should be", ZDateTime.Today.AddDays(-1), nfe.ReleaseDate);
				AssertEquals("ReferenceNumber should be", "000001", nfe.ReferenceNumber);
			});
		}

		public void TestTotalValues()
		{
			invoiceLine1.Charges.AddNew(ImportCustomsChargeTypeList.Codes.OverseasFreightCollect, 20.0m, declaration.LocalCurrencyCode);
			invoiceLine2.Charges.AddNew(ImportCustomsChargeTypeList.Codes.OverseasFreightCollect, 20.0m, declaration.LocalCurrencyCode);
			invoiceLine1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 10.0m, declaration.LocalCurrencyCode);
			invoiceLine2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 50.0m, declaration.LocalCurrencyCode);

			var nfe = GetNewBusinessObject() as NFeEntryExportObject;
			CombineAssertions(() =>
			{
				AssertEquals("TotalTotalFOBValue should be", 1000.0m, nfe.TotalFOBValue);
				AssertEquals("TotalFreightValue should be", 40.0m, nfe.TotalFreightValue);
				AssertEquals("TotalInsuranceValue should be", 60.0m, nfe.TotalInsuranceValue);
				AssertEquals("TotalCIFValue should be", 1100.0m, nfe.TotalCIFValue);
				AssertEquals("TotalGrossWeight should be", 140.0m, nfe.TotalGrossWeight);
				AssertEquals("TotalNetWeight should be", 120.0m, nfe.TotalNetWeight);
			});
		}

		#region Overrides of BusinessObjectBaseTestCase

		protected override BusinessObject GetNewBusinessObject()
		{
			return new NFeEntryExportObject(entryHeader);
		}

		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine1;
		JobComInvoiceLine invoiceLine2;
		CusEntryHeader entryHeader;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 500m;
			invoiceLine1.JI_Weight = 70000m;
			invoiceLine1.JI_WeightUQ = "G";
			invoiceLine1.JI_NetWeight = 60m;
			invoiceLine1.JI_NetWeightUQ = "KG";
			invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 500m;
			invoiceLine2.JI_Weight = 70m;
			invoiceLine2.JI_WeightUQ = "KG";
			invoiceLine2.JI_NetWeight = 60000m;
			invoiceLine2.JI_NetWeightUQ = "G";

			entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("2000010001");
			entryHeader.CH_Status = BRMessageStatusList.Codes.Accepted;

			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.InvoiceLines.Add(invoiceLine1);
			entryLine1.InvoiceLines.Add(invoiceLine2);
		}

		#endregion
	}
}
