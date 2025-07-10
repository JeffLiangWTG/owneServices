using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(PreviousDocument))]
	sealed class PreviousDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<PreviousDocument>
	{
		public void TestLookupsType()
		{
			var previousDocument = (PreviousDocument)GetNewBusinessObject();
			AssertType<PreviousDocumentLookups>(previousDocument.Lookups);
		}

		public void TestValidation()
		{
			var jobComInvoiceLinePreviousDocument = (PreviousDocument)GetNewBusinessObject();
			AssertType<JobComInvoiceLinePreviousDocumentValidation>(jobComInvoiceLinePreviousDocument.Validation);

			var cusEntryInstruction = Factory.New<CusEntryInstruction>();
			AssertType<CusEntryInstructionPreviousDocumentValidation>(cusEntryInstruction.PreviousDocuments.AddNew().Validation);

			var previousDocument = Factory.New<PreviousDocument>();
			previousDocument.CSI_ParentID = ZGuid.Empty;
			previousDocument.CSI_ParentTableCode = ZString.Empty;
			AssertType<PreviousDocumentValidation>(previousDocument.Validation);
		}

		public void TestCSI_Code_Attributes()
		{
			CombineAssertions(() =>
			{
				AssertEntity<PreviousDocument>()
				.HasProperty(p => p.CSI_Code)
				.WithCaption("Declaration Type")
				.WithList("Lookups.CodeList")
				.WithMaxLength(3);
			});

			var cusEntryInstruction = Factory.New<CusEntryInstruction>();
			var previousDocument = cusEntryInstruction.PreviousDocuments.AddNew();
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(previousDocument.CSI_CodeInfo,
				Constants.CusEntryInstruction.PreviousDocumentCaption,
				caption: "Type"
			);
		}

		public void TestCSI_ReferenceNumber_Attributes()
		{
			CombineAssertions(() =>
			{
				AssertEntity<PreviousDocument>()
				.HasProperty(p => p.CSI_ReferenceNumber)
				.WithCaption("Declaration Number")
				.WithMaxLength(35);
			});

			var cusEntryInstruction = Factory.New<CusEntryInstruction>();
			var previousDocument = cusEntryInstruction.PreviousDocuments.AddNew();
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(previousDocument.CSI_ReferenceNumberInfo,
				Constants.CusEntryInstruction.PreviousDocumentCaption,
				caption: "Reference"
			);
		}

		public void TestReferenceNumberFieldType()
		{
			var cusEntryInstruction = Factory.New<CusEntryInstruction>();
			var previousDocument = cusEntryInstruction.PreviousDocuments.AddNew();
			AssertEquals("Text", previousDocument.ReferenceNumberFieldType);
		}

		public void TestCSI_ReferenceNumber2_Attributes()
			=> CombineAssertions(() =>
			{
				AssertEntity<PreviousDocument>()
				.HasProperty(p => p.CSI_ReferenceNumber2)
				.WithCaption("Invoice Header Seq #")
				.WithMaxLength(5);
			});

		public void TestCSI_CSI_ItemNumber_Attributes()
			=> CombineAssertions(() =>
			{
				AssertEntity<PreviousDocument>()
				.HasProperty(p => p.CSI_ItemNumber)
				.WithCaption("Invoice Line Seq #")
				.WithMaxLength(4);
			});

		public void TestCSI_Quantity_Attributes()
			=> CombineAssertions(() =>
			{
				AssertEntity<PreviousDocument>()
				.HasProperty(p => p.CSI_Quantity)
				.WithCaption("Quantity")
				.WithMaxLength(16);
			});

		public void TestCSI_UnitOfQuantity_Attributes()
			=> CombineAssertions(() =>
			{
				AssertEntity<PreviousDocument>()
				.HasProperty(p => p.CSI_UnitOfQuantity)
				.WithCaption("UQ")
				.WithMaxLength(4);
			});

		public void TestSetDefaultValues()
		{
			var previousDocument = Factory.New<PreviousDocument>();
			AssertEquals("PRE", previousDocument.CSI_Type);
			previousDocument = (PreviousDocument)GetNewBusinessObject();
			AssertEquals("PRE", previousDocument.CSI_Type);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var jobComInvoiceHeader = jobDeclaration.Invoices.AddNew();
			var jobComInvoiceLine = (JobComInvoiceLine)jobComInvoiceHeader.InvoiceLines.AddNew();
			return jobComInvoiceLine.PreviousDocuments.AddNew();
		}

		protected override IEnumerable<PreviousDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var jobComInvoiceHeader = jobDeclaration.Invoices.AddNew();
			var jobComInvoiceLine = (JobComInvoiceLine)jobComInvoiceHeader.InvoiceLines.AddNew();
			var previousDocument = jobComInvoiceLine.PreviousDocuments.AddNew();
			Factory.Save();
			yield return previousDocument;
		}
	}
}
