using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(CopyDocumentsSelectionHeader))]
	sealed class CopyDocumentsSelectionHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSelectAll()
		{
			var header = new CopyDocumentsSelectionHeader(invoiceLine);

			CombineAssertions(() =>
			{
				AssertEquals("Before Select All", 0,
					header.Lines.Cast<CopyDocumentsSelectionLine>().Count(e => e.IsSelected));

				header.SelectAll();

				AssertEquals("After Select All", 8,
					header.Lines.Cast<CopyDocumentsSelectionLine>().Count(e => e.IsSelected));
			});
		}

		public void TestCopyAll()
		{
			var header = new CopyDocumentsSelectionHeader(invoiceLine);
			header.SelectAll();

			header.Copy();

			CombineAssertions(() =>
			{
				AssertEquals("line1 SupportingDocuments", 4, otherInvoiceLine1.SupportingDocuments.Count);
				AssertEquals("line2 SupportingDocuments", 3, otherInvoiceLine2.SupportingDocuments.Count);
				AssertEquals("line1 AdditionalInfos", 6, otherInvoiceLine1.AdditionalInfos.Count);
				AssertEquals("line2 AdditionalInfos", 5, otherInvoiceLine2.AdditionalInfos.Count);
			});
		}

		public void TestCopyExportSup()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			var header = new CopyDocumentsSelectionHeader(invoiceLine);
			header.Lines
				.Cast<CopyDocumentsSelectionLine>()
				.First(e => e.CSI_ReferenceNumber == "REF1")
				.IsSelected = true;

			header.Copy();

			CombineAssertions(() =>
			{
				var supportingDocument = otherInvoiceLine2.SupportingDocuments[0];
				AssertEquals(nameof(SupportingDocument.CSI_ReferenceNumber), "REF1", supportingDocument.CSI_ReferenceNumber);
				AssertEquals(nameof(SupportingDocument.CSI_DateOfIssue), ZDateTime.BrettsBirthday, supportingDocument.CSI_DateOfIssue);
				AssertEquals(nameof(SupportingDocument.CSI_UnitOfQuantity), "UQ1", supportingDocument.CSI_UnitOfQuantity);
				AssertEquals(nameof(SupportingDocument.CSI_Code), "CD1", supportingDocument.CSI_Code);
				AssertEquals(nameof(SupportingDocument.CSI_RX_NKCurrency), "EUR", supportingDocument.CSI_RX_NKCurrency);
				AssertEquals(nameof(SupportingDocument.CSI_ReferenceNumber2), "2REF", supportingDocument.CSI_ReferenceNumber2);
				AssertEquals(nameof(SupportingDocument.CSI_DateOfExpiry), new ZDateTime(2024, 11, 05, 22, 01, 02), supportingDocument.CSI_DateOfExpiry);
				AssertEquals(nameof(SupportingDocument.CSI_Description), "DESC", supportingDocument.CSI_Description);
				AssertEquals(nameof(SupportingDocument.CSI_SubType), "ST1", supportingDocument.CSI_SubType);
				AssertEquals(nameof(SupportingDocument.CSI_UnitOfQuantity2), "UQ2", supportingDocument.CSI_UnitOfQuantity2);
				AssertEquals(nameof(SupportingDocument.CSI_AdditionalDescription), "ADESC", supportingDocument.CSI_AdditionalDescription);
				AssertEquals(nameof(SupportingDocument.CSI_Status), ZString.Empty, supportingDocument.CSI_Status);
			});
		}

		public void TestCopyExportOth()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			var header = new CopyDocumentsSelectionHeader(invoiceLine);
			header.Lines
				.Cast<CopyDocumentsSelectionLine>()
				.First(e => e.CSI_ReferenceNumber == "REF4")
				.IsSelected = true;

			header.Copy();

			CombineAssertions(() =>
			{
				var additionalInfo = otherInvoiceLine2.AdditionalInfos[0];
				AssertEquals(nameof(AdditionalInfo.CSI_SubType), "ST4", additionalInfo.CSI_SubType);
				AssertEquals(nameof(AdditionalInfo.CSI_Code), "CD4", additionalInfo.CSI_Code);
				AssertEquals(nameof(AdditionalInfo.CSI_ReferenceNumber), "REF4", additionalInfo.CSI_ReferenceNumber);
				AssertEquals(nameof(AdditionalInfo.CSI_Description), "DESC", additionalInfo.CSI_Description);
				AssertEquals(nameof(AdditionalInfo.CSI_RX_NKCurrency), "EUR", additionalInfo.CSI_RX_NKCurrency);
				AssertEquals(nameof(AdditionalInfo.CSI_ReferenceNumber2), "2REF", additionalInfo.CSI_ReferenceNumber2);
			});
		}

		public void TestCopyImportSup()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			var header = new CopyDocumentsSelectionHeader(invoiceLine);
			header.Lines
				.Cast<CopyDocumentsSelectionLine>()
				.First(e => e.CSI_ReferenceNumber == "REF1")
				.IsSelected = true;

			header.Copy();

			CombineAssertions(() =>
			{
				var supportingDocument = otherInvoiceLine2.SupportingDocuments[0];
				AssertEquals(nameof(SupportingDocument.CSI_ReferenceNumber), "REF1", supportingDocument.CSI_ReferenceNumber);
				AssertEquals(nameof(SupportingDocument.CSI_DateOfIssue), ZDateTime.BrettsBirthday, supportingDocument.CSI_DateOfIssue);
				AssertEquals(nameof(SupportingDocument.CSI_UnitOfQuantity), "UQ1", supportingDocument.CSI_UnitOfQuantity);
				AssertEquals(nameof(SupportingDocument.CSI_Code), "CD1", supportingDocument.CSI_Code);
				AssertEquals(nameof(SupportingDocument.CSI_RX_NKCurrency), ZString.Empty, supportingDocument.CSI_RX_NKCurrency);
				AssertEquals(nameof(SupportingDocument.CSI_ReferenceNumber2), ZString.Empty, supportingDocument.CSI_ReferenceNumber2);
				AssertEquals(nameof(SupportingDocument.CSI_DateOfExpiry), ZDateTime.Empty, supportingDocument.CSI_DateOfExpiry);
				AssertEquals(nameof(SupportingDocument.CSI_Description), ZString.Empty,supportingDocument.CSI_Description);
				AssertEquals(nameof(SupportingDocument.CSI_SubType), ZString.Empty, supportingDocument.CSI_SubType);
				AssertEquals(nameof(SupportingDocument.CSI_UnitOfQuantity2), ZString.Empty, supportingDocument.CSI_UnitOfQuantity2);
				AssertEquals(nameof(SupportingDocument.CSI_AdditionalDescription), ZString.Empty, supportingDocument.CSI_AdditionalDescription);
				AssertEquals(nameof(SupportingDocument.CSI_Status), "STS", supportingDocument.CSI_Status);
			});
		}

		public void TestCopyImportOth()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			var header = new CopyDocumentsSelectionHeader(invoiceLine);
			header.Lines
				.Cast<CopyDocumentsSelectionLine>()
				.First(e => e.CSI_ReferenceNumber == "REF4")
				.IsSelected = true;

			header.Copy();

			CombineAssertions(() =>
			{
				var additionalInfo = otherInvoiceLine2.AdditionalInfos[0];
				AssertEquals(nameof(AdditionalInfo.CSI_SubType), "ST4", additionalInfo.CSI_SubType);
				AssertEquals(nameof(AdditionalInfo.CSI_Code), "CD4", additionalInfo.CSI_Code);
				AssertEquals(nameof(AdditionalInfo.CSI_ReferenceNumber), "REF4", additionalInfo.CSI_ReferenceNumber);
				AssertEquals(nameof(AdditionalInfo.CSI_Description), "DESC", additionalInfo.CSI_Description);
				AssertEquals(nameof(AdditionalInfo.CSI_RX_NKCurrency), ZString.Empty, additionalInfo.CSI_RX_NKCurrency);
				AssertEquals(nameof(AdditionalInfo.CSI_ReferenceNumber2), ZString.Empty,
					additionalInfo.CSI_ReferenceNumber2);
			});
		}

		public void TestInvoiceNumberTargetInvoice()
		{
			var header = new CopyDocumentsSelectionHeader(invoiceLine);

			CombineAssertions(() =>
			{
				AssertNull(header.TargetInvoice);
				header.InvoiceNumber = nameof(otherInvoice1);
				AssertEquals(otherInvoice1.PK, header.TargetInvoice.PK);
			});
		}

		public void TestLookup()
		{
			var header = new CopyDocumentsSelectionHeader(invoiceLine);
			AssertType<CopyDocumentsSelectionHeaderLookups>(header.Lookups);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var supportingDocument = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument.CSI_ReferenceNumber = "REF1";
			supportingDocument.CSI_DateOfIssue = ZDateTime.BrettsBirthday;
			supportingDocument.CSI_UnitOfQuantity = "UQ1";
			supportingDocument.CSI_Code = "CD1";
			supportingDocument.CSI_RX_NKCurrency = "EUR";
			supportingDocument.CSI_ReferenceNumber2 = "2REF";
			supportingDocument.CSI_DateOfExpiry = new ZDateTime(2024, 11, 05, 22, 01, 02);
			supportingDocument.CSI_Description = "DESC";
			supportingDocument.CSI_SubType = "ST1";
			supportingDocument.CSI_UnitOfQuantity2 = "UQ2";
			supportingDocument.CSI_AdditionalDescription = "ADESC";
			supportingDocument.CSI_Status = "STS";

			var supportingDocumentThatExistsOnOtherCode = invoiceLine.SupportingDocuments.AddNew();
			supportingDocumentThatExistsOnOtherCode.CSI_Code = "CD2";
			supportingDocumentThatExistsOnOtherCode.CSI_ReferenceNumber = "Ref2";

			var supportingDocumentThatExistsOnOtherCodeAndRef = invoiceLine.SupportingDocuments.AddNew();
			supportingDocumentThatExistsOnOtherCodeAndRef.CSI_Code = "CD3";
			supportingDocumentThatExistsOnOtherCodeAndRef.CSI_ReferenceNumber = "REF3";

			var additionalInfo = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo.CSI_SubType = "ST4";
			additionalInfo.CSI_Code = "CD4";
			additionalInfo.CSI_ReferenceNumber = "REF4";
			additionalInfo.CSI_Description = "DESC";
			additionalInfo.CSI_RX_NKCurrency = "EUR";
			additionalInfo.CSI_ReferenceNumber2 = "2REF";

			var additionalInfoKindInfThatExistsOnOtherCode = invoiceLine.AdditionalInfos.AddNew();
			additionalInfoKindInfThatExistsOnOtherCode.CSI_Code = "CD5";
			additionalInfoKindInfThatExistsOnOtherCode.CSI_ReferenceNumber = "REF5";
			additionalInfoKindInfThatExistsOnOtherCode.CSI_SubType = "INF";

			var additionalInfoKindRefThatExistsOnOtherCode = invoiceLine.AdditionalInfos.AddNew();
			additionalInfoKindRefThatExistsOnOtherCode.CSI_Code = "CD6";
			additionalInfoKindRefThatExistsOnOtherCode.CSI_ReferenceNumber = "REF6";
			additionalInfoKindRefThatExistsOnOtherCode.CSI_SubType = "REF";

			var additionalInfoKindInfThatExistsOnOtherCodeAndRef = invoiceLine.AdditionalInfos.AddNew();
			additionalInfoKindInfThatExistsOnOtherCodeAndRef.CSI_Code = "CD7";
			additionalInfoKindInfThatExistsOnOtherCodeAndRef.CSI_ReferenceNumber = "REF7";
			additionalInfoKindInfThatExistsOnOtherCodeAndRef.CSI_SubType = "INF";

			var additionalInfoKindRefThatExistsOnOtherCodeAndRef = invoiceLine.AdditionalInfos.AddNew();
			additionalInfoKindRefThatExistsOnOtherCodeAndRef.CSI_Code = "CD8";
			additionalInfoKindRefThatExistsOnOtherCodeAndRef.CSI_ReferenceNumber = "REF8";
			additionalInfoKindRefThatExistsOnOtherCodeAndRef.CSI_SubType = "REF";

			var previousDocumentThatIsNotSelectable = invoiceLine.PreviousDocuments.AddNew();
			previousDocumentThatIsNotSelectable.CSI_Code = "CD5";

			otherInvoice1 = declaration.Invoices.AddNew();
			otherInvoice1.JZ_InvoiceNumber = nameof(otherInvoice1);
			otherInvoiceLine1 = otherInvoice1.InvoiceLines.AddNew();

			otherInvoiceLine1.SupportingDocuments.AddNew("CD2", "REF");
			otherInvoiceLine1.SupportingDocuments.AddNew("CD3", "REF3");
			otherInvoiceLine1.AdditionalInfos.AddNew("CD5", "REF");
			otherInvoiceLine1.AdditionalInfos.AddNew("CD6", "REF");
			otherInvoiceLine1.AdditionalInfos.AddNew("CD7", "REF7");
			otherInvoiceLine1.AdditionalInfos.AddNew("CD8", "REF8");

			otherInvoice2 = declaration.Invoices.AddNew();
			otherInvoice2.JZ_InvoiceNumber = nameof(otherInvoice2);
			otherInvoiceLine2 = otherInvoice2.InvoiceLines.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CopyDocumentsSelectionHeader(invoiceLine);
		}

		JobComInvoiceLine invoiceLine;
		JobComInvoiceHeader otherInvoice1;
		JobComInvoiceHeader otherInvoice2;
		JobComInvoiceLine otherInvoiceLine1;
		JobComInvoiceLine otherInvoiceLine2;
		JobDeclaration declaration;
	}
}
