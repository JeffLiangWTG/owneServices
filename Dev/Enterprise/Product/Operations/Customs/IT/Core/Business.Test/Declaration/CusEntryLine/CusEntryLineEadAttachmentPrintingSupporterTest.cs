using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using EuAdditionalInfoCollection = Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class CusEntryLineEadAttachmentPrintingSupporterTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new CusEntryLineSadAttachmentPrintingSupporter(null));
		AssertNoExceptionThrown(() => new CusEntryLineSadAttachmentPrintingSupporter(Factory.New<CusEntryLine>()));
	}

	public void TestFirstEntryLineRequiresAttachment_WhenRequiresContainersSection()
	{
		var containersLength = 13;
		entryLine.CL_LineNumber = 1;

		EU.Business.Declaration.CusContainer[] containers = new EU.Business.Declaration.CusContainer[containersLength];

		for (int i = 0; i < containersLength; i++)
		{
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CNT" + i;
			containers[i] = container;
		}

		var attachmentPrintingSupporter = new CusEntryLineEadAttachmentPrintingSupporter(entryLine);
		Assert("No containers for first entry line", !attachmentPrintingSupporter.RequiresAttachment);

		for (int i = 0; i < containersLength - 1; i++)
		{
			invoiceLine.ContainersPivot.AddNew().C2_CO = containers[i].PK;
		}

		attachmentPrintingSupporter = new CusEntryLineEadAttachmentPrintingSupporter(entryLine);
		Assert("12 containers for first entry line", !attachmentPrintingSupporter.RequiresAttachment);

		invoiceLine.ContainersPivot.AddNew().C2_CO = containers[12].PK;
		attachmentPrintingSupporter = new CusEntryLineEadAttachmentPrintingSupporter(entryLine);
		Assert("13 containers for first entry line", attachmentPrintingSupporter.RequiresAttachment);
	}

	public void TestGenericEntryLineRequiresAttachment_WhenRequiresContainersSection()
	{
		var containersLength = 4;
		entryLine.CL_LineNumber = 2;

		EU.Business.Declaration.CusContainer[] containers = new EU.Business.Declaration.CusContainer[containersLength];

		for (int i = 0; i < containersLength; i++)
		{
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CNT" + i;
			containers[i] = container;
		}

		var attachmentPrintingSupporter = new CusEntryLineEadAttachmentPrintingSupporter(entryLine);
		Assert("No containers for generic entry line", !attachmentPrintingSupporter.RequiresAttachment);

		for (int i = 0; i < containersLength - 1; i++)
		{
			invoiceLine.ContainersPivot.AddNew().C2_CO = containers[i].PK;
		}

		attachmentPrintingSupporter = new CusEntryLineEadAttachmentPrintingSupporter(entryLine);
		Assert("3 containers for generic entry line", !attachmentPrintingSupporter.RequiresAttachment);

		invoiceLine.ContainersPivot.AddNew().C2_CO = containers[3].PK;
		attachmentPrintingSupporter = new CusEntryLineEadAttachmentPrintingSupporter(entryLine);
		Assert("4 containers for generic entry line", attachmentPrintingSupporter.RequiresAttachment);
	}

	public void TestFirstEntryLineRequiresAttachment_WhenRequiresSupportingDocumentsSection()
	{
		entryLine.CL_LineNumber = 1;

		var attachmentPrintingSupporter = new CusEntryLineEadAttachmentPrintingSupporter(entryLine);
		Assert("No supporting documents for the entry line", !attachmentPrintingSupporter.RequiresAttachment);

		var entrySuppDoc1 = declaration.SupportingDocuments.AddNew();
		entrySuppDoc1.CSI_Code = "N380";
		entrySuppDoc1.CSI_RN_NKCountryCode = "IT";
		entrySuppDoc1.CSI_YearOfIssue = "2020";
		entrySuppDoc1.CSI_ReferenceNumber = new string('1', 50);
		entrySuppDoc1.CSI_UnitOfQuantity = "KGM";
		entrySuppDoc1.CSI_Quantity = 100.1234m;
		attachmentPrintingSupporter = new CusEntryLineEadAttachmentPrintingSupporter(entryLine);
		Assert("Supporting Documents total length doesn't exceed the threshold", !attachmentPrintingSupporter.RequiresAttachment);

		var entrySuppDoc2 = declaration.SupportingDocuments.AddNew();
		entrySuppDoc2.CSI_Code = "N380";
		entrySuppDoc2.CSI_RN_NKCountryCode = "IT";
		entrySuppDoc2.CSI_YearOfIssue = "2020";
		entrySuppDoc2.CSI_ReferenceNumber = new string('2', 50);
		entrySuppDoc2.CSI_UnitOfQuantity = "KGM";
		entrySuppDoc2.CSI_Quantity = 100.1234m;
		attachmentPrintingSupporter = new CusEntryLineEadAttachmentPrintingSupporter(entryLine);
		Assert("Supporting Documents total length doesn't exceed (yet) the threshold", !attachmentPrintingSupporter.RequiresAttachment);

		for (int i = 3; i < 10; i++)
		{
			var entrySuppDoci = declaration.SupportingDocuments.AddNew();
			entrySuppDoci.CSI_Code = "N380";
			entrySuppDoci.CSI_RN_NKCountryCode = "IT";
			entrySuppDoci.CSI_YearOfIssue = "2020";
			entrySuppDoci.CSI_ReferenceNumber = new string(Convert.ToChar(i), 50);
			entrySuppDoci.CSI_UnitOfQuantity = "KGM";
			entrySuppDoci.CSI_Quantity = 100.1234m;
		}

		attachmentPrintingSupporter = new CusEntryLineEadAttachmentPrintingSupporter(entryLine);
		Assert("Supporting Documents total length exceeds the threshold", attachmentPrintingSupporter.RequiresAttachment);
	}

	public void TestFirstEntryLineRequiresAttachment_WhenRequiresSupportingDocumentsSection_MixWithAdditionalInfos()
	{
		entryLine.CL_LineNumber = 1;

		var attachmentPrintingSupporter = new CusEntryLineEadAttachmentPrintingSupporter(entryLine);
		Assert("No supporting documents for the entry line", !attachmentPrintingSupporter.RequiresAttachment);

		var entrySuppDoc1 = declaration.SupportingDocuments.AddNew();
		entrySuppDoc1.CSI_Code = "N380";
		entrySuppDoc1.CSI_RN_NKCountryCode = "IT";
		entrySuppDoc1.CSI_YearOfIssue = "2020";
		entrySuppDoc1.CSI_ReferenceNumber = new string('1', 50);
		entrySuppDoc1.CSI_UnitOfQuantity = "KGM";
		entrySuppDoc1.CSI_Quantity = 100.1234m;
		entrySuppDoc1.CSI_RX_NKCurrency = "EUR";
		entrySuppDoc1.CSI_Value = 123.34m;
		attachmentPrintingSupporter = new CusEntryLineEadAttachmentPrintingSupporter(entryLine);
		Assert("Supporting Document total length doesn't exceed the threshold", !attachmentPrintingSupporter.RequiresAttachment);

		AddAdditionalInfo(invoiceLine.AdditionalInfos, "REF", "Y001", "123", "");
		attachmentPrintingSupporter = new CusEntryLineEadAttachmentPrintingSupporter(entryLine);
		Assert("Supporting Document + Additional Document total length doesn't exceed the threshold", !attachmentPrintingSupporter.RequiresAttachment);

		for (int i = 1; i < 7; i++)
		{
			AddAdditionalInfo(invoiceLine.AdditionalInfos, "INF", "30500", "", new string(Convert.ToChar(i), 50));
		}

		attachmentPrintingSupporter = new CusEntryLineEadAttachmentPrintingSupporter(entryLine);
		Assert("Supporting Document + Additional Documents total length exceeds the threshold", attachmentPrintingSupporter.RequiresAttachment);
	}

	public void TestGenericEntryLineRequiresAttachment_WhenRequiresSupportingDocumentsSection()
	{
		entryLine.CL_LineNumber = 2;

		var attachmentPrintingSupporter = new CusEntryLineEadAttachmentPrintingSupporter(entryLine);
		Assert("No supporting documents for the entry line", !attachmentPrintingSupporter.RequiresAttachment);

		var entrySuppDoc1 = declaration.SupportingDocuments.AddNew();
		entrySuppDoc1.CSI_Code = "N380";
		entrySuppDoc1.CSI_RN_NKCountryCode = "IT";
		entrySuppDoc1.CSI_YearOfIssue = "2020";
		entrySuppDoc1.CSI_ReferenceNumber = new string('1', 50);
		entrySuppDoc1.CSI_UnitOfQuantity = "KGM";
		entrySuppDoc1.CSI_Quantity = 100.1234m;
		attachmentPrintingSupporter = new CusEntryLineEadAttachmentPrintingSupporter(entryLine);
		Assert("Supporting Documents total length doesn't exceed the threshold", !attachmentPrintingSupporter.RequiresAttachment);

		var entrySuppDoc2 = declaration.SupportingDocuments.AddNew();
		entrySuppDoc2.CSI_Code = "N380";
		entrySuppDoc2.CSI_RN_NKCountryCode = "IT";
		entrySuppDoc2.CSI_YearOfIssue = "2020";
		entrySuppDoc2.CSI_ReferenceNumber = new string('2', 50);
		entrySuppDoc2.CSI_UnitOfQuantity = "KGM";
		entrySuppDoc2.CSI_Quantity = 100.1234m;
		attachmentPrintingSupporter = new CusEntryLineEadAttachmentPrintingSupporter(entryLine);
		Assert("Supporting Documents total length exceeds the threshold", attachmentPrintingSupporter.RequiresAttachment);
	}

	public void TestFirstEntryLineRequiresContainersSection()
	{
		var containersLength = 13;
		entryLine.CL_LineNumber = 1;

		EU.Business.Declaration.CusContainer[] containers = new EU.Business.Declaration.CusContainer[containersLength];

		for (int i = 0; i < containersLength; i++)
		{
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CNT" + i;
			containers[i] = container;
		}

		var attachmentPrintingSupporter = new CusEntryLineEadAttachmentPrintingSupporter(entryLine);
		Assert("No containers for first entry line", !attachmentPrintingSupporter.RequiresContainersSection);

		for (int i = 0; i < containersLength - 1; i++)
		{
			invoiceLine.ContainersPivot.AddNew().C2_CO = containers[i].PK;
		}

		attachmentPrintingSupporter = new CusEntryLineEadAttachmentPrintingSupporter(entryLine);
		Assert("12 containers for first entry line", !attachmentPrintingSupporter.RequiresContainersSection);

		invoiceLine.ContainersPivot.AddNew().C2_CO = containers[12].PK;
		attachmentPrintingSupporter = new CusEntryLineEadAttachmentPrintingSupporter(entryLine);
		Assert("13 containers for first entry line", attachmentPrintingSupporter.RequiresContainersSection);
	}

	public void TestGenericEntryLineRequiresContainersSection()
	{
		var containersLength = 4;
		entryLine.CL_LineNumber = 2;

		EU.Business.Declaration.CusContainer[] containers = new EU.Business.Declaration.CusContainer[containersLength];

		for (int i = 0; i < containersLength; i++)
		{
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CNT" + i;
			containers[i] = container;
		}

		var attachmentPrintingSupporter = new CusEntryLineEadAttachmentPrintingSupporter(entryLine);
		Assert("No containers for generic entry line", !attachmentPrintingSupporter.RequiresContainersSection);

		for (int i = 0; i < containersLength - 1; i++)
		{
			invoiceLine.ContainersPivot.AddNew().C2_CO = containers[i].PK;
		}

		attachmentPrintingSupporter = new CusEntryLineEadAttachmentPrintingSupporter(entryLine);
		Assert("3 containers for generic entry line", !attachmentPrintingSupporter.RequiresContainersSection);

		invoiceLine.ContainersPivot.AddNew().C2_CO = containers[3].PK;
		attachmentPrintingSupporter = new CusEntryLineEadAttachmentPrintingSupporter(entryLine);
		Assert("4 containers for generic entry line", attachmentPrintingSupporter.RequiresContainersSection);
	}

	public void TestFirstEntryLineRequiresSupportingDocumentsSection()
	{
		entryLine.CL_LineNumber = 1;

		var attachmentPrintingSupporter = new CusEntryLineEadAttachmentPrintingSupporter(entryLine);
		Assert("No supporting documents for the entry line", !attachmentPrintingSupporter.RequiresSupportingDocumentsSection);

		var entrySuppDoc1 = declaration.SupportingDocuments.AddNew();
		entrySuppDoc1.CSI_Code = "N380";
		entrySuppDoc1.CSI_RN_NKCountryCode = "IT";
		entrySuppDoc1.CSI_YearOfIssue = "2020";
		entrySuppDoc1.CSI_ReferenceNumber = new string('1', 50);
		entrySuppDoc1.CSI_UnitOfQuantity = "KGM";
		entrySuppDoc1.CSI_Quantity = 100.1234m;
		attachmentPrintingSupporter = new CusEntryLineEadAttachmentPrintingSupporter(entryLine);
		Assert("Supporting Documents total length doesn't exceed the threshold", !attachmentPrintingSupporter.RequiresSupportingDocumentsSection);

		var entrySuppDoc2 = declaration.SupportingDocuments.AddNew();
		entrySuppDoc2.CSI_Code = "N380";
		entrySuppDoc2.CSI_RN_NKCountryCode = "IT";
		entrySuppDoc2.CSI_YearOfIssue = "2020";
		entrySuppDoc2.CSI_ReferenceNumber = new string('2', 50);
		entrySuppDoc2.CSI_UnitOfQuantity = "KGM";
		entrySuppDoc2.CSI_Quantity = 100.1234m;
		attachmentPrintingSupporter = new CusEntryLineEadAttachmentPrintingSupporter(entryLine);
		Assert("Supporting Documents total length doesn't exceed (yet) the threshold", !attachmentPrintingSupporter.RequiresSupportingDocumentsSection);

		for (int i = 3; i < 10; i++)
		{
			var entrySuppDoci = declaration.SupportingDocuments.AddNew();
			entrySuppDoci.CSI_Code = "N380";
			entrySuppDoci.CSI_RN_NKCountryCode = "IT";
			entrySuppDoci.CSI_YearOfIssue = "2020";
			entrySuppDoci.CSI_ReferenceNumber = new string(Convert.ToChar(i), 50);
			entrySuppDoci.CSI_UnitOfQuantity = "KGM";
			entrySuppDoci.CSI_Quantity = 100.1234m;
		}

		attachmentPrintingSupporter = new CusEntryLineEadAttachmentPrintingSupporter(entryLine);
		Assert("Supporting Documents total length exceeds the threshold", attachmentPrintingSupporter.RequiresSupportingDocumentsSection);
	}

	public void TestGenericEntryLineRequiresSupportingDocumentsSection()
	{
		entryLine.CL_LineNumber = 2;

		var attachmentPrintingSupporter = new CusEntryLineEadAttachmentPrintingSupporter(entryLine);
		Assert("No supporting documents for the entry line", !attachmentPrintingSupporter.RequiresAttachment);

		var entrySuppDoc1 = declaration.SupportingDocuments.AddNew();
		entrySuppDoc1.CSI_Code = "N380";
		entrySuppDoc1.CSI_RN_NKCountryCode = "IT";
		entrySuppDoc1.CSI_YearOfIssue = "2020";
		entrySuppDoc1.CSI_ReferenceNumber = new string('1', 50);
		entrySuppDoc1.CSI_UnitOfQuantity = "KGM";
		entrySuppDoc1.CSI_Quantity = 100.1234m;
		attachmentPrintingSupporter = new CusEntryLineEadAttachmentPrintingSupporter(entryLine);
		Assert("Supporting Documents total length doesn't exceed the threshold", !attachmentPrintingSupporter.RequiresSupportingDocumentsSection);

		var entrySuppDoc2 = declaration.SupportingDocuments.AddNew();
		entrySuppDoc2.CSI_Code = "N380";
		entrySuppDoc2.CSI_RN_NKCountryCode = "IT";
		entrySuppDoc2.CSI_YearOfIssue = "2020";
		entrySuppDoc2.CSI_ReferenceNumber = new string('2', 50);
		entrySuppDoc2.CSI_UnitOfQuantity = "KGM";
		entrySuppDoc2.CSI_Quantity = 100.1234m;
		attachmentPrintingSupporter = new CusEntryLineEadAttachmentPrintingSupporter(entryLine);
		Assert("Supporting Documents total length exceeds the threshold", attachmentPrintingSupporter.RequiresSupportingDocumentsSection);
	}

	public void TestSupportingDocumentsFormatted()
	{
		var entrySuppDoc1 = declaration.SupportingDocuments.AddNew();
		entrySuppDoc1.CSI_Code = "N380";
		entrySuppDoc1.CSI_RN_NKCountryCode = "IT";
		entrySuppDoc1.CSI_YearOfIssue = "2020";
		entrySuppDoc1.CSI_ReferenceNumber = new string('1', 50);
		entrySuppDoc1.CSI_UnitOfQuantity = "KGM";
		entrySuppDoc1.CSI_Quantity = 100.1234m;
		var entrySuppDoc2 = declaration.SupportingDocuments.AddNew();
		entrySuppDoc2.CSI_Code = "N380";
		entrySuppDoc2.CSI_RN_NKCountryCode = "IT";
		entrySuppDoc2.CSI_YearOfIssue = "2020";
		entrySuppDoc2.CSI_ReferenceNumber = new string('2', 10);
		entrySuppDoc2.CSI_UnitOfQuantity = "KGM";
		entrySuppDoc2.CSI_Quantity = 100.1234m;
		var entrySuppDoc3 = declaration.SupportingDocuments.AddNew();
		entrySuppDoc3.CSI_Code = "N380";
		entrySuppDoc3.CSI_RN_NKCountryCode = "IT";
		entrySuppDoc3.CSI_YearOfIssue = "2020";
		entrySuppDoc3.CSI_ReferenceNumber = new string('3', 50);
		entrySuppDoc3.CSI_UnitOfQuantity = "KGM";
		entrySuppDoc3.CSI_Quantity = 100.1234m;
		var attachmentPrintingSupporter = new CusEntryLineEadAttachmentPrintingSupporter(entryLine);
		AssertArrayEqualsByElements(new ZString[]
		{
			"N380-IT-2020-11111111111111111111111111111111111111111111111111-KGM-100.1234",
			"N380-IT-2020-2222222222-KGM-100.1234",
			"N380-IT-2020-33333333333333333333333333333333333333333333333333-KGM-100.1234"
		}, attachmentPrintingSupporter.SupportingDocumentsFormatted.ToArray());
	}

	public void TestSupportingDocumentsFormatted_SameAsSAD()
	{
		AddNewInvoiceLineSupportingDocument("N380", "IT", "2020", "1", 0m, "", 0m, "");
		AddNewInvoiceLineSupportingDocument("N380", "IT", "2020", "2", 0m, "KGM", 0m, "EUR");
		AddNewInvoiceLineSupportingDocument("N380", "IT", "2020", "3", 100m, "KGM", 77m, "EUR");
		AddNewInvoiceLineSupportingDocument("N380", "IT", "2020", "4", 100.12m, "KGM", 77.3m, "EUR");
		AddNewInvoiceLineSupportingDocument("N380", "IT", "2020", "5", 100.123456m, "KGM", 77.44m, "EUR");

		var attachmentPrintingSupporter = new CusEntryLineEadAttachmentPrintingSupporter(entryLine);
		AssertArrayEqualsByElements(new ZString[]
		{
			"N380-IT-2020-1",
			"N380-IT-2020-2-KGM-0-EUR-0.00",
			"N380-IT-2020-3-KGM-100-EUR-77.00",
			"N380-IT-2020-4-KGM-100.12-EUR-77.30",
			"N380-IT-2020-5-KGM-100.123456-EUR-77.44",
		}, attachmentPrintingSupporter.SupportingDocumentsFormatted.ToArray());
	}

	public void TestAdditionalInfosFormattedFormatted()
	{
		AddAdditionalInfo(invoice.AdditionalInfos, "REF", "Y001", "123", "");
		AddAdditionalInfo(invoiceLine.AdditionalInfos, "INF", "30500", "", "456");
		AddAdditionalInfo(invoiceLine.AdditionalInfos, "TRA", "N722", "789", "");
		var attachmentPrintingSupporter = new CusEntryLineEadAttachmentPrintingSupporter(entryLine);

		AssertArrayEqualsByElements(new ZString[]
		{
			"30500-456",
			"N722-789" ,
			"Y001-123"
		}, attachmentPrintingSupporter.AdditionalInfosFormatted.ToArray());
	}

	public void TestRequiresFeesSection()
	{
		var attachmentPrintingSupporter = new CusEntryLineEadAttachmentPrintingSupporter(entryLine);
		Assert("Fees section never required", !attachmentPrintingSupporter.RequiresFeesSection);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryLine = entryHeader.MergedLines.AddNew();
		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
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
	JobDeclaration declaration;
	CusEntryLine entryLine;
	JobComInvoiceHeader invoice;
	JobComInvoiceLine invoiceLine;
}
