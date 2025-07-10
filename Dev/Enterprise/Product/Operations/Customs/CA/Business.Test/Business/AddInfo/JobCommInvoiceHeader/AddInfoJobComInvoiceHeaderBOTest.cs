using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(AddInfoJobComInvoiceHeader))]
	sealed class AddInfoJobComInvoiceHeaderBOTest : AddInfoBOTest
	{
		public void TestGetNewValidation()
		{
			var addInfo = (AddInfoJobComInvoiceHeader)GetNewBusinessObject();
			addInfo.Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Export Validation", typeof(ExportAddInfoJobComInvoiceHeaderValidation), addInfo.Validation.GetType());
			addInfo.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Import Validation", typeof(ImportAddInfoJobComInvoiceHeaderValidation), addInfo.Validation.GetType());
			addInfo.Declaration.JE_MessageType = JobMessageTypeList.Codes.Misc;
			AssertEquals("Misc Validation", typeof(AddInfoJobComInvoiceHeaderValidation), addInfo.Validation.GetType());
			addInfo.Declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			AssertEquals("LVX Validation", typeof(ImportAddInfoJobComInvoiceHeaderValidation), addInfo.Validation.GetType());

			addInfo.Declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			AssertEquals("B2 Validation", typeof(B2AddInfoJobComInvoiceHeaderValidation), addInfo.Validation.GetType());
			addInfo.Declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			AssertEquals("B3X Validation", typeof(B2AddInfoJobComInvoiceHeaderValidation), addInfo.Validation.GetType());
		}

		protected override Type GetExpectedLookupsType()
		{
			return typeof(AddInfoJobComInvoiceHeaderLookups);
		}

		protected override Type GetExpectedValidationType()
		{
			return typeof(ExportAddInfoJobComInvoiceHeaderValidation);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			return new AddInfoJobComInvoiceHeader(invoice.JZ_AddInfoInfo);
		}
	}
}
