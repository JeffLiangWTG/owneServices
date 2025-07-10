using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using EuAdditionalInfoCollection = Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class CusEntryLineSadAttachmentPrintingSupporterTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new CusEntryLineSadAttachmentPrintingSupporter(null));
		AssertNoExceptionThrown(() => new CusEntryLineSadAttachmentPrintingSupporter(Factory.New<CusEntryLine>()));
	}

	public void TestRequiresAttachment_WhenRequiresContainersSection()
	{
		var container1 = declaration.CusContainers.AddNew();
		container1.CO_ContainerNumber = "CNT1";
		var container2 = declaration.CusContainers.AddNew();
		container2.CO_ContainerNumber = "CNT2";
		var container3 = declaration.CusContainers.AddNew();
		container3.CO_ContainerNumber = "CNT3";
		var container4 = declaration.CusContainers.AddNew();
		container4.CO_ContainerNumber = "CNT4";
		var container5 = declaration.CusContainers.AddNew();
		container5.CO_ContainerNumber = "CNT5";
		var container6 = declaration.CusContainers.AddNew();
		container6.CO_ContainerNumber = "CNT6";

		var attachmentPrintingSupporter = new CusEntryLineSadAttachmentPrintingSupporter(entryLine);
		Assert("No containers for the entry line", !attachmentPrintingSupporter.RequiresAttachment);

		invoiceLine.ContainersPivot.AddNew().C2_CO = container1.PK;
		invoiceLine.ContainersPivot.AddNew().C2_CO = container2.PK;
		invoiceLine.ContainersPivot.AddNew().C2_CO = container3.PK;
		invoiceLine.ContainersPivot.AddNew().C2_CO = container4.PK;
		invoiceLine.ContainersPivot.AddNew().C2_CO = container5.PK;
		attachmentPrintingSupporter = new CusEntryLineSadAttachmentPrintingSupporter(entryLine);
		Assert("5 containers for the entry line", !attachmentPrintingSupporter.RequiresAttachment);

		invoiceLine.ContainersPivot.AddNew().C2_CO = container6.PK;
		attachmentPrintingSupporter = new CusEntryLineSadAttachmentPrintingSupporter(entryLine);
		Assert("6 containers for the entry line", attachmentPrintingSupporter.RequiresAttachment);
	}

	public void TestRequiresAttachment_WhenRequiresSupportingDocumentsSection()
	{
		var attachmentPrintingSupporter = new CusEntryLineSadAttachmentPrintingSupporter(entryLine);
		Assert("No supporting documents for the entry line", !attachmentPrintingSupporter.RequiresAttachment);

		var entrySuppDoc1 = declaration.SupportingDocuments.AddNew();
		entrySuppDoc1.CSI_Code = "N380";
		entrySuppDoc1.CSI_RN_NKCountryCode = "IT";
		entrySuppDoc1.CSI_YearOfIssue = "2020";
		entrySuppDoc1.CSI_ReferenceNumber = new string('1', 50);
		entrySuppDoc1.CSI_UnitOfQuantity = "KGM";
		entrySuppDoc1.CSI_Quantity = 100.1234m;
		attachmentPrintingSupporter = new CusEntryLineSadAttachmentPrintingSupporter(entryLine);
		Assert("Supporting Documents total length doesn't exceed the threshold", !attachmentPrintingSupporter.RequiresAttachment);

		var entrySuppDoc2 = declaration.SupportingDocuments.AddNew();
		entrySuppDoc2.CSI_Code = "N380";
		entrySuppDoc2.CSI_RN_NKCountryCode = "IT";
		entrySuppDoc2.CSI_YearOfIssue = "2020";
		entrySuppDoc2.CSI_ReferenceNumber = new string('2', 10);
		entrySuppDoc2.CSI_UnitOfQuantity = "KGM";
		entrySuppDoc2.CSI_Quantity = 100.1234m;
		attachmentPrintingSupporter = new CusEntryLineSadAttachmentPrintingSupporter(entryLine);
		Assert("Supporting Documents total length doesn't exceed (yet) the threshold", !attachmentPrintingSupporter.RequiresAttachment);

		var entrySuppDoc3 = declaration.SupportingDocuments.AddNew();
		entrySuppDoc3.CSI_Code = "N380";
		entrySuppDoc3.CSI_RN_NKCountryCode = "IT";
		entrySuppDoc3.CSI_YearOfIssue = "2020";
		entrySuppDoc3.CSI_ReferenceNumber = new string('3', 50);
		entrySuppDoc3.CSI_UnitOfQuantity = "KGM";
		entrySuppDoc3.CSI_Quantity = 100.1234m;
		attachmentPrintingSupporter = new CusEntryLineSadAttachmentPrintingSupporter(entryLine);
		Assert("Supporting Documents total length exceeds the threshold", attachmentPrintingSupporter.RequiresAttachment);
	}

	public void TestRequiresAttachment_WhenRequiresFeesSection()
	{
		var attachmentPrintingSupporter = new CusEntryLineSadAttachmentPrintingSupporter(entryLine);
		AssertEquals("No fees", 0, entryLine.Fees.Count);
		Assert("Fees section not required (no fees)", !attachmentPrintingSupporter.RequiresAttachment);

		for (int i = 0; i < 8; i++)
		{
			entryLine.Fees.AddNew();
		}
		attachmentPrintingSupporter = new CusEntryLineSadAttachmentPrintingSupporter(entryLine);
		AssertEquals("Fees Count (limit reached)", 8, entryLine.Fees.Count);
		Assert("Fees section not required (limit reached)", !attachmentPrintingSupporter.RequiresAttachment);

		entryLine.Fees.AddNew();
		attachmentPrintingSupporter = new CusEntryLineSadAttachmentPrintingSupporter(entryLine);
		AssertEquals("Fees Count (limit exceeded)", 9, entryLine.Fees.Count);
		Assert("Fees section required (limit exceeded)", attachmentPrintingSupporter.RequiresAttachment);
	}

	public void TestRequiresContainersSection()
	{
		var container1 = declaration.CusContainers.AddNew();
		container1.CO_ContainerNumber = "CNT1";
		var container2 = declaration.CusContainers.AddNew();
		container2.CO_ContainerNumber = "CNT2";
		var container3 = declaration.CusContainers.AddNew();
		container3.CO_ContainerNumber = "CNT3";
		var container4 = declaration.CusContainers.AddNew();
		container4.CO_ContainerNumber = "CNT4";
		var container5 = declaration.CusContainers.AddNew();
		container5.CO_ContainerNumber = "CNT5";
		var container6 = declaration.CusContainers.AddNew();
		container6.CO_ContainerNumber = "CNT6";

		var attachmentPrintingSupporter = new CusEntryLineSadAttachmentPrintingSupporter(entryLine);
		Assert("No containers for the entry line", !attachmentPrintingSupporter.RequiresContainersSection);

		invoiceLine.ContainersPivot.AddNew().C2_CO = container1.PK;
		invoiceLine.ContainersPivot.AddNew().C2_CO = container2.PK;
		invoiceLine.ContainersPivot.AddNew().C2_CO = container3.PK;
		invoiceLine.ContainersPivot.AddNew().C2_CO = container4.PK;
		invoiceLine.ContainersPivot.AddNew().C2_CO = container5.PK;
		attachmentPrintingSupporter = new CusEntryLineSadAttachmentPrintingSupporter(entryLine);
		Assert("5 containers for the entry line. Threshold is not exceeded", !attachmentPrintingSupporter.RequiresContainersSection);

		invoiceLine.ContainersPivot.AddNew().C2_CO = container6.PK;
		attachmentPrintingSupporter = new CusEntryLineSadAttachmentPrintingSupporter(entryLine);
		Assert("6 containers for the entry line. Threshold is exceeded", attachmentPrintingSupporter.RequiresContainersSection);
	}

	public void TestRequiresSupportingDocumentsSection()
	{
		var attachmentPrintingSupporter = new CusEntryLineSadAttachmentPrintingSupporter(entryLine);
		Assert("No supporting documents for the entry line", !attachmentPrintingSupporter.RequiresSupportingDocumentsSection);

		var entrySuppDoc1 = declaration.SupportingDocuments.AddNew();
		entrySuppDoc1.CSI_Code = "N380";
		entrySuppDoc1.CSI_RN_NKCountryCode = "IT";
		entrySuppDoc1.CSI_YearOfIssue = "2020";
		entrySuppDoc1.CSI_ReferenceNumber = new string('1', 50);
		entrySuppDoc1.CSI_UnitOfQuantity = "KGM";
		entrySuppDoc1.CSI_Quantity = 100.1234m;
		attachmentPrintingSupporter = new CusEntryLineSadAttachmentPrintingSupporter(entryLine);
		AssertEquals(77, attachmentPrintingSupporter.SupportingDocumentsFormatted.Sum(x => x.Length) + attachmentPrintingSupporter.SupportingDocumentsFormatted.Count());
		Assert("Supporting Documents total length doesn't exceed the threshold", !attachmentPrintingSupporter.RequiresSupportingDocumentsSection);

		var entrySuppDoc2 = declaration.SupportingDocuments.AddNew();
		entrySuppDoc2.CSI_Code = "N380";
		entrySuppDoc2.CSI_RN_NKCountryCode = "IT";
		entrySuppDoc2.CSI_YearOfIssue = "2020";
		entrySuppDoc2.CSI_ReferenceNumber = new string('2', 10);
		entrySuppDoc2.CSI_UnitOfQuantity = "KGM";
		entrySuppDoc2.CSI_Quantity = 100.1234m;
		attachmentPrintingSupporter = new CusEntryLineSadAttachmentPrintingSupporter(entryLine);
		AssertEquals(114, attachmentPrintingSupporter.SupportingDocumentsFormatted.Sum(x => x.Length) + attachmentPrintingSupporter.SupportingDocumentsFormatted.Count());
		Assert("Supporting Documents total length doesn't exceed (yet) the threshold", !attachmentPrintingSupporter.RequiresSupportingDocumentsSection);

		var entrySuppDoc3 = declaration.SupportingDocuments.AddNew();
		entrySuppDoc3.CSI_Code = "N380";
		entrySuppDoc3.CSI_RN_NKCountryCode = "IT";
		entrySuppDoc3.CSI_YearOfIssue = "2020";
		entrySuppDoc3.CSI_ReferenceNumber = new string('3', 50);
		entrySuppDoc3.CSI_UnitOfQuantity = "KGM";
		entrySuppDoc3.CSI_Quantity = 100.1234m;
		attachmentPrintingSupporter = new CusEntryLineSadAttachmentPrintingSupporter(entryLine);
		AssertEquals(191, attachmentPrintingSupporter.SupportingDocumentsFormatted.Sum(x => x.Length) + attachmentPrintingSupporter.SupportingDocumentsFormatted.Count());
		Assert("Supporting Documents total length exceeds the threshold", attachmentPrintingSupporter.RequiresSupportingDocumentsSection);
	}

	public void TestSupportingDocumentsFormatted()
	{
		AddNewInvoiceLineSupportingDocument("N380", "IT", "2020", "1", 0m, "", 0m, "");
		AddNewInvoiceLineSupportingDocument("N380", "IT", "2020", "2", 0m, "KGM", 0m, "EUR");
		AddNewInvoiceLineSupportingDocument("N380", "IT", "2020", "3", 100m, "KGM", 77m, "EUR");
		AddNewInvoiceLineSupportingDocument("N380", "IT", "2020", "4", 100.12m, "KGM", 77.3m, "EUR");
		AddNewInvoiceLineSupportingDocument("N380", "IT", "2020", "5", 100.123456m, "KGM", 77.44m, "EUR");

		var attachmentPrintingSupporter = new CusEntryLineSadAttachmentPrintingSupporter(entryLine);
		AssertArrayEqualsByElements(new ZString[]
		{
			"N380-IT-2020-1",
			"N380-IT-2020-2-KGM-0-EUR-0.00",
			"N380-IT-2020-3-KGM-100-EUR-77.00",
			"N380-IT-2020-4-KGM-100.12-EUR-77.30",
			"N380-IT-2020-5-KGM-100.123456-EUR-77.44",
		}, attachmentPrintingSupporter.SupportingDocumentsFormatted.ToArray());
	}

	public void TestRequiresFeesSection()
	{
		var attachmentPrintingSupporter = new CusEntryLineSadAttachmentPrintingSupporter(entryLine);
		AssertEquals("No fees", 0, entryLine.Fees.Count);
		Assert("Fees section not required (no fees)", !attachmentPrintingSupporter.RequiresFeesSection);

		for (int i = 0; i < 8; i++)
		{
			entryLine.Fees.AddNew();
		}
		attachmentPrintingSupporter = new CusEntryLineSadAttachmentPrintingSupporter(entryLine);
		AssertEquals("Fees Count (limit reached)", 8, entryLine.Fees.Count);
		Assert("Fees section not required (limit reached)", !attachmentPrintingSupporter.RequiresFeesSection);

		entryLine.Fees.AddNew();
		attachmentPrintingSupporter = new CusEntryLineSadAttachmentPrintingSupporter(entryLine);
		AssertEquals("Fees Count (limit exceeded)", 9, entryLine.Fees.Count);
		Assert("Fees section required (limit exceeded)", attachmentPrintingSupporter.RequiresFeesSection);
	}

	public void TestAdditionalInfosFormattedFormatted()
	{
		var attachmentPrintingSupporter = new CusEntryLineSadAttachmentPrintingSupporter(entryLine);
		AssertArrayEqualsByElements(Array.Empty<ZString>(), attachmentPrintingSupporter.AdditionalInfosFormatted.ToArray());

		AddAdditionalInfo(invoiceLine.AdditionalInfos, "INF", "30500", "", "456");
		AddAdditionalInfo(invoiceLine.AdditionalInfos, "TRA", "N722", "789", "");
		attachmentPrintingSupporter = new CusEntryLineSadAttachmentPrintingSupporter(entryLine);

		AssertArrayEqualsByElements("AdditionalInfos is empty In Sad", Array.Empty<ZString>(), attachmentPrintingSupporter.AdditionalInfosFormatted.ToArray());
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
	}

	JobDeclaration declaration;
	CusEntryLine entryLine;
	JobComInvoiceLine invoiceLine;

	void AddNewInvoiceLineSupportingDocument(
		string code,
		string countryCode,
		string yearOfIssue,
		string referenceNumber,
		decimal quantity,
		string unitOfQuantity,
		decimal value,
		string currency)
	{
		var supportingDocument = invoiceLine.SupportingDocuments.AddNew();
		supportingDocument.CSI_Code = code;
		supportingDocument.CSI_RN_NKCountryCode = countryCode;
		supportingDocument.CSI_YearOfIssue = yearOfIssue;
		supportingDocument.CSI_ReferenceNumber = referenceNumber;
		supportingDocument.CSI_Quantity = quantity;
		supportingDocument.CSI_UnitOfQuantity = unitOfQuantity;
		supportingDocument.CSI_Value = value;
		supportingDocument.CSI_RX_NKCurrency = currency;
	}

	void AddAdditionalInfo(
			EuAdditionalInfoCollection documentCollection,
			string subType,
			string code,
			string referenceNumber,
			string description)
	{
		var additionalDocument = documentCollection.AddNew();
		additionalDocument.CSI_SubType = subType;
		additionalDocument.CSI_Code = code;
		additionalDocument.CSI_ReferenceNumber = referenceNumber;
		additionalDocument.CSI_Description = description;
	}
}
