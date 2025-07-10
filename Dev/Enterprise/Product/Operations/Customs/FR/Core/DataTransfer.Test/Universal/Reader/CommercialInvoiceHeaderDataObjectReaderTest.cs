using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Shipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.FR.DataTransfer.Universal.Testing;

sealed class CommercialInvoiceHeaderDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
{
	#region TestReadIntoBusinessObject_JZ_Properties

	public void TestReadIntoBusinessObject_JZ_Properties_DefaultBehaviour()
	{
		AssertReadIntoBusinessObject_JZ_Properties("After reading, the value can be read into the existing invoice1.", reader1, invoice1, 100);

		AssertReadIntoBusinessObject_JZ_Properties("After reading, the value can be read into the existing invoice2.", reader2, invoice2, 50);
	}

	public void TestReadIntoBusinessObject_JZ_Properties_SnapshotRevertingWithStrategyOverride()
	{
		using (jobDeclarationReader.TemporarilyMarkJobDeclarationReaderIsBeingUsedForSnapshotReverting(entryHeaderToRevert, Customs.Business.SnapshotRevertingStrategy.Override, new TestErrorLogger()))
		{
			AssertReadIntoBusinessObject_JZ_Properties("When reader is used for snapshot reverting and the strategy is Override, invoice1 should be reverted.", reader1, invoice1, 100);

			AssertReadIntoBusinessObject_JZ_Properties("When reader is used for snapshot reverting and the strategy is Override, invoice1 should be reverted.", reader2, invoice2, 50);
		}
	}

	public void TestReadIntoBusinessObject_JZ_Properties_SnapshotRevertingWithStrategySkip()
	{
		using (jobDeclarationReader.TemporarilyMarkJobDeclarationReaderIsBeingUsedForSnapshotReverting(entryHeaderToRevert, Customs.Business.SnapshotRevertingStrategy.Skip, new TestErrorLogger()))
		{
			AssertReadIntoBusinessObject_JZ_Properties("When reader is used for snapshot reverting and the strategy is Skip, invoice1 should be reverted, because all its inner invoice lines are included in entryHeaderToRevert.", reader1, invoice1, 100);

			AssertReadIntoBusinessObject_JZ_Properties("When reader is used for snapshot reverting and the strategy is Skip, invoice2 should not be reverted, because not all its inner invoice lines are included in entryHeaderToRevert.", reader2, invoice2, 0);
		}
	}

	void AssertReadIntoBusinessObject_JZ_Properties(string message, CommercialInvoiceHeaderDataObjectReader_ForTest reader, JobComInvoiceHeader invoice, ZDecimal expectedValueAfterReading)
	{
		AssertEquals("Prerequisite: JZ_InvoiceAmount should be 0.", ZDecimal.Zero, invoice.JZ_InvoiceAmount);
		reader.ReadIntoBusinessObject(true);
		AssertEquals(message, expectedValueAfterReading, invoice.JZ_InvoiceAmount);
	}

	#endregion

	#region TestReadIntoBusinessObject_InvoiceLines

	public void TestReadIntoBusinessObject_InvoiceLines_DefaultBehaviour()
	{
		AssertReadIntoBusinessObject_InvoiceLines("When not used for snapshot reverting, all invoice lines under invoice1 should be reverted.", reader1, invoice1, new ZString[] { "11111111", "22222222" }, new ZString[] { "66666666", "77777777" });

		AssertReadIntoBusinessObject_InvoiceLines("When not used for snapshot reverting, all invoice lines under invoice2 should be reverted.", reader2, invoice2, new ZString[] { "33333333", "44444444" }, new ZString[] { "88888888", "99999999" });
	}

	public void TestReadIntoBusinessObject_InvoiceLines_SnapshotRevertingWithStrategyOverride()
	{
		using (jobDeclarationReader.TemporarilyMarkJobDeclarationReaderIsBeingUsedForSnapshotReverting(entryHeaderToRevert, Customs.Business.SnapshotRevertingStrategy.Override, new TestErrorLogger()))
		{
			AssertReadIntoBusinessObject_InvoiceLines("When used for snapshot reverting and the strategy is Override, all invoice lines under invoice1 should be reverted.", reader1, invoice1, new ZString[] { "11111111", "22222222" }, new ZString[] { "66666666", "77777777" });

			AssertReadIntoBusinessObject_InvoiceLines("When used for snapshot reverting and the strategy is Override, only invoiceLine22 should be reverted as it's included in entryHeaderToRevert.", reader2, invoice2, new ZString[] { "33333333", "44444444" }, new ZString[] { "33333333", "99999999" });
		}
	}

	public void TestReadIntoBusinessObject_InvoiceLines_SnapshotRevertingWithStrategySkip()
	{
		using (jobDeclarationReader.TemporarilyMarkJobDeclarationReaderIsBeingUsedForSnapshotReverting(entryHeaderToRevert, Customs.Business.SnapshotRevertingStrategy.Skip, new TestErrorLogger()))
		{
			AssertReadIntoBusinessObject_InvoiceLines("When used for snapshot reverting and the strategy is Skip, all invoice lines under invoice1 should be reverted.", reader1, invoice1, new ZString[] { "11111111", "22222222" }, new ZString[] { "66666666", "77777777" });

			AssertReadIntoBusinessObject_InvoiceLines("When used for snapshot reverting and the strategy is Skip, only invoiceLine22 should be reverted as it's included in entryHeaderToRevert.", reader2, invoice2, new ZString[] { "33333333", "44444444" }, new ZString[] { "33333333", "99999999" });
		}
	}

	void AssertReadIntoBusinessObject_InvoiceLines(string message, CommercialInvoiceHeaderDataObjectReader_ForTest reader, JobComInvoiceHeader invoice, ZString[] originalValues, ZString[] expectedValueAfterReading)
	{
		AssertSequencesEqual(originalValues.Cast<ZString>(), invoice.InvoiceLines.Select(line => line.JI_Tariff));
		reader.ReadIntoBusinessObject(true);
		AssertSequencesEqual(message, expectedValueAfterReading.Cast<ZString>(), invoice.InvoiceLines.Select(line => line.JI_Tariff));
	}

	#endregion

	public void TestCreateNewCustomsSupportingInformationCollectionDataObjectReader()
	{
		AssertType<CustomsSupportingInformationCollectionDataObjectReader>(reader1.CreateNewCustomsSupportingInformationCollectionDataObjectReader_Exposed());
	}

	protected override void SetUp()
	{
		base.SetUp();
		var helper = new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.France);
		jobDeclarationReader = new JobDeclarationDataObjectReader_ForTest(JobDeclarationDataObjectReaderTest.CreateShipment(Factory), new TestErrorLogger(), Factory);
		declaration = JobDeclarationDataObjectReaderTest.GetJobDeclaration(Factory);
		invoice1 = declaration.Invoices.Cast<JobComInvoiceHeader>().First(invoice => invoice.JZ_InvoiceNumber == "INV1");
		invoice2 = declaration.Invoices.Cast<JobComInvoiceHeader>().First(invoice => invoice.JZ_InvoiceNumber == "INV2");
		entryHeaderToRevert = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First(entry => entry.InvoiceLines.Any(line => line.EntryInstruction.CEI_Style == "A"));

		var topGroupInvoice = declaration.TopGroupInvoice;
		var commercialHeader1 = GetCommercialHeader1();
		reader1 = new CommercialInvoiceHeaderDataObjectReader_ForTest(commercialHeader1, new TestErrorLogger(), helper, topGroupInvoice, parentDeclarationReader: jobDeclarationReader);

		var commercialHeader2 = GetCommercialHeader2();
		reader2 = new CommercialInvoiceHeaderDataObjectReader_ForTest(commercialHeader2, new TestErrorLogger(), helper, topGroupInvoice, parentDeclarationReader: jobDeclarationReader);
	}
	JobDeclarationDataObjectReader_ForTest jobDeclarationReader;
	CommercialInvoiceHeaderDataObjectReader_ForTest reader1;
	CommercialInvoiceHeaderDataObjectReader_ForTest reader2;
	JobDeclaration declaration;
	JobComInvoiceHeader invoice1;
	JobComInvoiceHeader invoice2;
	CusEntryHeader entryHeaderToRevert;

	internal static CommercialInvoiceHeader GetCommercialHeader1()
	{
		var commercialHeader1 = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
		{
			InvoiceNumber = "INV1",
			InvoiceAmount = 100
		};
		commercialHeader1.SetCommercialInvoiceLineCollection(() =>
		{
			var invoiceLines = new DataObjectList<CommercialInvoiceLine>();
			invoiceLines.Content = CollectionContent.Complete;
			invoiceLines.Add(new CommercialInvoiceLine() { LineNo = 1, Link = 1, HarmonisedCode = "66666666", DataImportMatchingKey = "MK11" });
			invoiceLines.Add(new CommercialInvoiceLine() { LineNo = 2, Link = 3,  HarmonisedCode = "77777777", DataImportMatchingKey = "MK12" });
			return invoiceLines;
		});
		return commercialHeader1;
	}

	internal static CommercialInvoiceHeader GetCommercialHeader2()
	{
		var commercialHeader2 = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
		{
			InvoiceNumber = "INV2",
			InvoiceAmount = 50
		};
		commercialHeader2.SetCommercialInvoiceLineCollection(() =>
		{
			var invoiceLines = new DataObjectList<CommercialInvoiceLine>();
			invoiceLines.Content = CollectionContent.Complete;
			invoiceLines.Add(new CommercialInvoiceLine() { LineNo = 1, Link = 2, HarmonisedCode = "88888888", DataImportMatchingKey = "MK21" });
			invoiceLines.Add(new CommercialInvoiceLine() { LineNo = 2, Link = 4, HarmonisedCode = "99999999", DataImportMatchingKey = "MK22" });
			return invoiceLines;
		});
		return commercialHeader2;
	}
}

class CommercialInvoiceHeaderDataObjectReader_ForTest : CommercialInvoiceHeaderDataObjectReader
{
	public CommercialInvoiceHeaderDataObjectReader_ForTest(CommercialInvoiceHeader invoiceDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, JobComInvoiceGroupHeader groupHeader, Shipment topLevelObject = null, ILandedCostDataReader landedCostDataReader = null, JobDeclarationDataObjectReader parentDeclarationReader = null) : base(invoiceDataObject, logger, helper, groupHeader, topLevelObject, landedCostDataReader, parentDeclarationReader)
	{
	}

	public Customs.DataTransfer.Universal.CustomsSupportingInformationCollectionDataObjectReader CreateNewCustomsSupportingInformationCollectionDataObjectReader_Exposed() => new CustomsSupportingInformationCollectionDataObjectReader(logger, helper);
}
