using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(AddInfoJobComInvoiceLine))]
	sealed class AddInfoJobComInvoiceLineBOTest : AddInfoBOTest
	{
		public void TestCA_TradeNameSize()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("CA_TradeName Length", 50, invoiceLine.CA_TradeNameInfo.MaxLength);
		}

		public void TestValidationTypeChanges()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Export Validation", typeof(ExportAddInfoJobComInvoiceLineValidation), invoiceLine.AddInfoValidation.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Import Validation", typeof(ImportAddInfoJobComInvoiceLineValidation), invoiceLine.AddInfoValidation.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Misc;
			AssertEquals("Misc Validation", typeof(AddInfoJobComInvoiceLineValidation), invoiceLine.AddInfoValidation.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			AssertEquals("LVS Validation", typeof(ImportAddInfoJobComInvoiceLineValidation), invoiceLine.AddInfoValidation.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			AssertEquals("LVX Validation", typeof(ImportAddInfoJobComInvoiceLineValidation), invoiceLine.AddInfoValidation.GetType());
		}

		protected override Type GetExpectedLookupsType()
		{
			return typeof(AddInfoJobComInvoiceLineLookups);
		}

		protected override Type GetExpectedValidationType()
		{
			return typeof(ExportAddInfoJobComInvoiceLineValidation);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			return new AddInfoJobComInvoiceLine(invoiceLine.JI_AddInfoInfo);
		}
	}
}
