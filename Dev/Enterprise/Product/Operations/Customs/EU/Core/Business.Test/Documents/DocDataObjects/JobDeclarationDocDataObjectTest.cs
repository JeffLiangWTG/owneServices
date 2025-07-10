using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Customs.EU.Business.Documents.DocDataObjects.Testing.DocDataObjectTestUtility;

namespace Enterprise.Customs.EU.Business.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(JobDeclarationDocDataObject))]
	public class JobDeclarationDocDataObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new JobDeclarationDocDataObject(new EURCertificateOfOriginWrapper(declaration.CustomsEntryHeaders.AddNew()));
		}

		[ExpectNoExceptions]
		public void TestDeclarationReference()
		{
			var certificateOfOriginMock = new Mock<IEURCertificateOfOrigin>();
			certificateOfOriginMock
			.Setup(x => x.DeclarationReference)
			.Returns("ARG");

			var declarationDataObject = new JobDeclarationDocDataObject(certificateOfOriginMock.Object);
			NUnit.Framework.Assert.That(declarationDataObject.DeclarationReference, NUnit.Framework.Is.EqualTo("ARG").Using(CustomComparers.TypeComparison), "DeclarationReference");
		}

		[ExpectNoExceptions]
		public void TestReferenceDateFormat()
		{
			var declarationDataObject = (JobDeclarationDocDataObject)GetNewBusinessObject();
			NUnit.Framework.Assert.That(declarationDataObject.ReferenceDateFormat, NUnit.Framework.Is.EqualTo("dd/MM/yyyy"), "Reference Date Format");
		}

		[ExpectNoExceptions]
		public void TestEUR1Pg1Box11TextEntryNumber()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var declarationDataObject = new JobDeclarationDocDataObject(new EURCertificateOfOriginWrapper(declaration));
			NUnit.Framework.Assert.That(declarationDataObject.EUR1Pg1Box11TextEntryNumber, NUnit.Framework.Is.EqualTo("No. of"), "EUR1Pg1Box11TextEntryNumber");
		}

		[ExpectNoExceptions]
		public void TestEntryNumber()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew().EntryNumber = "test";
			var declarationDataObject = new JobDeclarationDocDataObject(new EURCertificateOfOriginWrapper(declaration));
			NUnit.Framework.Assert.That(declarationDataObject.EntryNumber, NUnit.Framework.Is.EqualTo("test"), "EntryNumber");
		}

		[ExpectNoExceptions]
		public void TestUrl()
		{
			var certificateOfOriginMock = new Mock<IEURCertificateOfOrigin>();
			certificateOfOriginMock
			.Setup(x => x.Url)
			.Returns(ZString.Empty);

			var declarationDataObject = new JobDeclarationDocDataObject(certificateOfOriginMock.Object);
			NUnit.Framework.Assert.That(declarationDataObject.Url, NUnit.Framework.Is.EqualTo(ZString.Empty), "Url");
		}

		[ExpectNoExceptions]
		public void TestCumulationApplied()
		{
			var eurCertificateOfOriginWrapperMock = new Mock<IEURCertificateOfOrigin>();
			eurCertificateOfOriginWrapperMock
				.Setup(x => x.Remarks)
				.Returns(ZString.Empty);

			var declarationDataObject = new JobDeclarationDocDataObject(eurCertificateOfOriginWrapperMock.Object);
			NUnit.Framework.Assert.That(declarationDataObject.CumulationApplied, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "When Remarks are empty, CumulationApplied must be false");

			eurCertificateOfOriginWrapperMock = new Mock<IEURCertificateOfOrigin>();
			eurCertificateOfOriginWrapperMock
				.Setup(x => x.Remarks)
				.Returns("ARG01");

			declarationDataObject = new JobDeclarationDocDataObject(eurCertificateOfOriginWrapperMock.Object);
			NUnit.Framework.Assert.That(declarationDataObject.CumulationApplied, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "When Remarks are not empty, CumulationApplied must be true");
		}

		[ExpectNoExceptions]
		public void TestDocumentRemarks()
		{
			var eurCertificateOfOriginWrapperMock = new Mock<IEURCertificateOfOrigin>();
			eurCertificateOfOriginWrapperMock
				.Setup(x => x.Remarks)
				.Returns(ZString.Empty);

			var declarationDataObject = new JobDeclarationDocDataObject(eurCertificateOfOriginWrapperMock.Object);
			NUnit.Framework.Assert.That(declarationDataObject.Remarks, NUnit.Framework.Is.EqualTo(ZString.Empty), "When Remarks are empty, DocumentRemarks must be empty");

			eurCertificateOfOriginWrapperMock = new Mock<IEURCertificateOfOrigin>();
			eurCertificateOfOriginWrapperMock
				.Setup(x => x.Remarks)
				.Returns("ARG01");

			declarationDataObject = new JobDeclarationDocDataObject(eurCertificateOfOriginWrapperMock.Object);
			NUnit.Framework.Assert.That(declarationDataObject.Remarks, NUnit.Framework.Is.EqualTo("ARG01").Using(CustomComparers.TypeComparison), "When Remarks are not empty, DocumentRemarks must be populated");
		}

		[ExpectNoExceptions]
		public void TestMaxLengthExporterInDocument()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_FullName = JobDeclarationDocDataObject.Constants.FieldForMaxLengthTest100;
			supplier.MainAddress.OA_Address1 = JobDeclarationDocDataObject.Constants.FieldForMaxLengthTest100.Substring(0, 50);
			supplier.MainAddress.OA_Address2 = JobDeclarationDocDataObject.Constants.FieldForMaxLengthTest100.Substring(0, 50);
			supplier.MainAddress.PrimaryOrgAddressAdditionalInfoDetail = JobDeclarationDocDataObject.Constants.FieldForMaxLengthTest100.Substring(0, 50);
			supplier.MainAddress.OA_City = JobDeclarationDocDataObject.Constants.FieldForMaxLengthTest100.Substring(0, 50);
			supplier.MainAddress.OA_State = JobDeclarationDocDataObject.Constants.FieldForMaxLengthTest100.Substring(0, 25);
			supplier.MainAddress.OA_PostCode = "2000251487";
			supplier.MainAddress.OA_RN_NKCountryCode = "AU";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_OH_Supplier = supplier.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var declarationDataObject = new JobDeclarationDocDataObject(new EURCertificateOfOriginWrapper(entryHeader));

			NUnit.Framework.Assert.That(declarationDataObject.Exporter.Length, NUnit.Framework.Is.EqualTo(JobDeclarationDocDataObject.Constants.MaxLinesExporterInDocument * JobDeclarationDocDataObject.Constants.MaxLengthLineExporterInDocument), "The max length of Exporter should be " + (JobDeclarationDocDataObject.Constants.MaxLinesExporterInDocument * JobDeclarationDocDataObject.Constants.MaxLengthLineExporterInDocument));
		}

		[ExpectNoExceptions]
		public void TestMaxLengthOriginCountryInDocument()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var declarationDataObject = new JobDeclarationDocDataObject(new EURCertificateOfOriginWrapper(entryHeader));

			declarationDataObject.OriginCountry = JobDeclarationDocDataObject.Constants.FieldForMaxLengthTest100;
			NUnit.Framework.Assert.That(declarationDataObject.OriginCountry.Length, NUnit.Framework.Is.EqualTo(JobDeclarationDocDataObject.Constants.MaxLengthOriginCountryInDocument), "The max length of Origin Country should be " + JobDeclarationDocDataObject.Constants.MaxLengthOriginCountryInDocument);
		}

		[ExpectNoExceptions]
		public void TestMaxLengthDestinationCountryInDocument()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var declarationDataObject = new JobDeclarationDocDataObject(new EURCertificateOfOriginWrapper(entryHeader));

			declarationDataObject.DestinationCountry = JobDeclarationDocDataObject.Constants.FieldForMaxLengthTest100;
			NUnit.Framework.Assert.That(declarationDataObject.DestinationCountry.Length, NUnit.Framework.Is.EqualTo(JobDeclarationDocDataObject.Constants.MaxLengthDestinationCountryInDocument), "The max length of Destination Country should be " + JobDeclarationDocDataObject.Constants.MaxLengthDestinationCountryInDocument);
		}

		[ExpectNoExceptions]
		public void TestMaxLengthOriginGroupInDocument()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var declarationDataObject = new JobDeclarationDocDataObject(new EURCertificateOfOriginWrapper(entryHeader));

			declarationDataObject.OriginGroup = JobDeclarationDocDataObject.Constants.FieldForMaxLengthTest100;
			NUnit.Framework.Assert.That(declarationDataObject.OriginGroup.Length, NUnit.Framework.Is.EqualTo(JobDeclarationDocDataObject.Constants.MaxLengthOriginGroupInDocument), "The max length of Origin Group should be " + JobDeclarationDocDataObject.Constants.MaxLengthOriginGroupInDocument);
		}

		[ExpectNoExceptions]
		public void TestMaxLengthDestinationGroupInDocument()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var declarationDataObject = new JobDeclarationDocDataObject(new EURCertificateOfOriginWrapper(entryHeader));

			declarationDataObject.DestinationGroup = JobDeclarationDocDataObject.Constants.FieldForMaxLengthTest100;
			NUnit.Framework.Assert.That(declarationDataObject.DestinationGroup.Length, NUnit.Framework.Is.EqualTo(JobDeclarationDocDataObject.Constants.MaxLengthDestinationGroupInDocument), "The max length of Destination Group should be " + JobDeclarationDocDataObject.Constants.MaxLengthDestinationGroupInDocument);
		}

		[ExpectNoExceptions]
		public void TestMaxLengthImporterInDocument()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = JobDeclarationDocDataObject.Constants.FieldForMaxLengthTest100;
			importer.MainAddress.OA_Address1 = JobDeclarationDocDataObject.Constants.FieldForMaxLengthTest100.Substring(0, 50);
			importer.MainAddress.OA_Address2 = JobDeclarationDocDataObject.Constants.FieldForMaxLengthTest100.Substring(0, 50);
			importer.MainAddress.OA_AdditionalAddressInformation = JobDeclarationDocDataObject.Constants.FieldForMaxLengthTest100.Substring(0, 50);
			importer.MainAddress.OA_City = JobDeclarationDocDataObject.Constants.FieldForMaxLengthTest100.Substring(0, 50);
			importer.MainAddress.OA_State = JobDeclarationDocDataObject.Constants.FieldForMaxLengthTest100.Substring(0, 25);
			importer.MainAddress.OA_PostCode = "2000251487";
			importer.MainAddress.OA_RN_NKCountryCode = "AU";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var declarationDataObject = new JobDeclarationDocDataObject(new EURCertificateOfOriginWrapper(entryHeader));

			NUnit.Framework.Assert.That(declarationDataObject.Importer.Length, NUnit.Framework.Is.EqualTo(JobDeclarationDocDataObject.Constants.MaxLinesImporterInDocument * JobDeclarationDocDataObject.Constants.MaxLengthLineImporterInDocument), "The max length of Importer should be " + (JobDeclarationDocDataObject.Constants.MaxLinesImporterInDocument * JobDeclarationDocDataObject.Constants.MaxLengthLineImporterInDocument));
		}

		[ExpectNoExceptions]
		public void TestMaxLengthVoyageFlightInDocument()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var declarationDataObject = new JobDeclarationDocDataObject(new EURCertificateOfOriginWrapper(entryHeader));

			declarationDataObject.VoyageFlight = JobDeclarationDocDataObject.Constants.FieldForMaxLengthTest100;
			NUnit.Framework.Assert.That(declarationDataObject.VoyageFlight.Length, NUnit.Framework.Is.EqualTo(JobDeclarationDocDataObject.Constants.MaxLengthVoyageFlightInDocument), "The max length of Voyage Flight should be " + JobDeclarationDocDataObject.Constants.MaxLengthVoyageFlightInDocument);
		}

		[ExpectNoExceptions]
		public void TestMaxLengthItemInDocument()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var declarationDataObject = new JobDeclarationDocDataObject(new EURCertificateOfOriginWrapper(entryHeader));

			declarationDataObject.Items += "1" + System.Environment.NewLine;
			declarationDataObject.Items += "2" + System.Environment.NewLine;
			while (declarationDataObject.Items.Length < (JobDeclarationDocDataObject.Constants.MaxLinesItemsInDocument * JobDeclarationDocDataObject.Constants.MaxLengthLineItemsInDocument))
			{
				declarationDataObject.Items += JobDeclarationDocDataObject.Constants.FieldForMaxLengthTest100;
			}
			NUnit.Framework.Assert.That(declarationDataObject.Items.Length, NUnit.Framework.Is.EqualTo(JobDeclarationDocDataObject.Constants.MaxLinesItemsInDocument * JobDeclarationDocDataObject.Constants.MaxLengthLineItemsInDocument), "The max length of Items and Description should be " + (JobDeclarationDocDataObject.Constants.MaxLinesItemsInDocument * JobDeclarationDocDataObject.Constants.MaxLengthLineItemsInDocument));
		}

		[ExpectNoExceptions]
		public void TestMaxLengthGrossMassInDocument()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var declarationDataObject = new JobDeclarationDocDataObject(new EURCertificateOfOriginWrapper(entryHeader));

			declarationDataObject.ItemMassVolume += "1" + System.Environment.NewLine;
			declarationDataObject.ItemMassVolume += "2" + System.Environment.NewLine;
			while (declarationDataObject.ItemMassVolume.Length < (JobDeclarationDocDataObject.Constants.MaxLinesItemsInDocument * JobDeclarationDocDataObject.Constants.MaxLengthLineGrossMassInDocument))
			{
				declarationDataObject.ItemMassVolume += JobDeclarationDocDataObject.Constants.FieldForMaxLengthTest100;
			}
			NUnit.Framework.Assert.That(declarationDataObject.ItemMassVolume.Length, NUnit.Framework.Is.EqualTo(JobDeclarationDocDataObject.Constants.MaxLinesItemsInDocument * JobDeclarationDocDataObject.Constants.MaxLengthLineGrossMassInDocument), "The max length of Gross and Mass should be " + (JobDeclarationDocDataObject.Constants.MaxLinesItemsInDocument * JobDeclarationDocDataObject.Constants.MaxLengthLineGrossMassInDocument));
		}

		[ExpectNoExceptions]
		public void TestMaxLengthInvoicesInDocument()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var declarationDataObject = new JobDeclarationDocDataObject(new EURCertificateOfOriginWrapper(entryHeader));

			declarationDataObject.InvoiceNumbers += "ARG01" + System.Environment.NewLine;
			declarationDataObject.InvoiceNumbers += "ARG02" + System.Environment.NewLine;
			while (declarationDataObject.InvoiceNumbers.Length < (JobDeclarationDocDataObject.Constants.MaxLinesItemsInDocument * JobDeclarationDocDataObject.Constants.MaxLengthLineInvoiceInDocument))
			{
				declarationDataObject.InvoiceNumbers += JobDeclarationDocDataObject.Constants.FieldForMaxLengthTest100;
			}
			NUnit.Framework.Assert.That(declarationDataObject.InvoiceNumbers.Length, NUnit.Framework.Is.EqualTo(JobDeclarationDocDataObject.Constants.MaxLinesItemsInDocument * JobDeclarationDocDataObject.Constants.MaxLengthLineInvoiceInDocument), "The max length of Invoices should be " + (JobDeclarationDocDataObject.Constants.MaxLinesItemsInDocument * JobDeclarationDocDataObject.Constants.MaxLengthLineInvoiceInDocument));
		}

		[ExpectNoExceptions]
		public void TestMaxLengthCustomsEndorsementInDocument()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var declarationDataObject = new JobDeclarationDocDataObject(new EURCertificateOfOriginWrapper(entryHeader));
			var box11CustomsEndorsement = declarationDataObject.EUR1Pg1Box11CustomsEndorsement;

			CombineAssertions(() =>
			{
				box11CustomsEndorsement.Form = JobDeclarationDocDataObject.Constants.FieldForMaxLengthTest100;
				box11CustomsEndorsement.FormNo = JobDeclarationDocDataObject.Constants.FieldForMaxLengthTest100;
				box11CustomsEndorsement.CustomsOffice = JobDeclarationDocDataObject.Constants.FieldForMaxLengthTest100;
				box11CustomsEndorsement.IssuingCountry = JobDeclarationDocDataObject.Constants.FieldForMaxLengthTest100;
				box11CustomsEndorsement.Place = JobDeclarationDocDataObject.Constants.FieldForMaxLengthTest100;
				NUnit.Framework.Assert.That(box11CustomsEndorsement.Form.Length, NUnit.Framework.Is.EqualTo(JobDeclarationDocDataObject.Constants.MaxLength11FormInDocument), "The max length of (11)Form should be " + JobDeclarationDocDataObject.Constants.MaxLength11FormInDocument);
				NUnit.Framework.Assert.That(box11CustomsEndorsement.FormNo.Length, NUnit.Framework.Is.EqualTo(JobDeclarationDocDataObject.Constants.MaxLength11NumberInDocument), "The max length of (11)Number should be " + JobDeclarationDocDataObject.Constants.MaxLength11NumberInDocument);
				NUnit.Framework.Assert.That(box11CustomsEndorsement.CustomsOffice.Length, NUnit.Framework.Is.EqualTo(JobDeclarationDocDataObject.Constants.MaxLength11CustomsInDocument), "The max length of (11)CustomsOffice should be " + JobDeclarationDocDataObject.Constants.MaxLength11CustomsInDocument);
				NUnit.Framework.Assert.That(box11CustomsEndorsement.IssuingCountry.Length, NUnit.Framework.Is.EqualTo(JobDeclarationDocDataObject.Constants.MaxLength11IssuingInDocument), "The max length of (11)IssuingCountry should be " + JobDeclarationDocDataObject.Constants.MaxLength11IssuingInDocument);
				NUnit.Framework.Assert.That(box11CustomsEndorsement.Place.Length, NUnit.Framework.Is.EqualTo(JobDeclarationDocDataObject.Constants.MaxLength11PlaceInDocument), "The max length of (11)Place should be " + JobDeclarationDocDataObject.Constants.MaxLength11PlaceInDocument);
			});
		}

		[ExpectNoExceptions]
		public void TestMaxLengthDeclarationByExporterInDocument()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var declarationDataObject = new JobDeclarationDocDataObject(new EURCertificateOfOriginWrapper(entryHeader));
			var box12DeclarationExporter = declarationDataObject.EUR1Pg1Box12DeclarationExporter;

			CombineAssertions(() =>
			{
				box12DeclarationExporter.Place = JobDeclarationDocDataObject.Constants.FieldForMaxLengthTest100;
				box12DeclarationExporter.SupplierDetails = JobDeclarationDocDataObject.Constants.FieldForMaxLengthTest100;
				NUnit.Framework.Assert.That(JobDeclarationDocDataObject.Constants.MaxLength12PlaceInDocument, NUnit.Framework.Is.EqualTo(12));
				NUnit.Framework.Assert.That(box12DeclarationExporter.Place.Length, NUnit.Framework.Is.EqualTo(JobDeclarationDocDataObject.Constants.MaxLength12PlaceInDocument), "The max length of (12)Place should be " + JobDeclarationDocDataObject.Constants.MaxLength12PlaceInDocument);
				NUnit.Framework.Assert.That(JobDeclarationDocDataObject.Constants.MaxLength12SupplierDetailsInDocument, NUnit.Framework.Is.EqualTo(66));
				NUnit.Framework.Assert.That(box12DeclarationExporter.SupplierDetails.Length, NUnit.Framework.Is.EqualTo(JobDeclarationDocDataObject.Constants.MaxLength12SupplierDetailsInDocument), "The max length of (12)Supplier Details should be " + JobDeclarationDocDataObject.Constants.MaxLength12SupplierDetailsInDocument);
			});
		}

		[ExpectNoExceptions]
		public void TestPropertiesAreAlterable()
		{
			RefZoneHeader zoneEUR1 = Factory.New<RefZoneHeader>();
			zoneEUR1.FZ_Code = "EUR1";
			zoneEUR1.FZ_Description = "Europe EUR1";

			var supplier = Factory.New<OrgHeader>();
			supplier.OH_FullName = "Freds Supply Co";
			supplier.MainAddress.OA_Address1 = "367 George St";
			supplier.MainAddress.OA_City = "Sydney";
			supplier.MainAddress.OA_State = "NSW";
			supplier.MainAddress.OA_PostCode = "2000";
			supplier.MainAddress.OA_RN_NKCountryCode = "AU";

			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "Jims Import Co";
			importer.MainAddress.OA_Address1 = "Petuelring 130";
			importer.MainAddress.OA_City = "München";
			importer.MainAddress.OA_State = "";
			importer.MainAddress.OA_PostCode = "80809";
			importer.MainAddress.OA_RN_NKCountryCode = "DE";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKFinalDestination = "DEBER";
			declaration.JE_TransportMode = "AIR";
			declaration.JE_VoyageFlightNo = "KL123";

			var billPackingGroup = declaration.Bills.AddNew().PackingGroups.AddNew();
			var package1 = declaration.Packages.AddNew();
			package1.CW_CR_HouseContainer = billPackingGroup.PK;
			package1.CW_PackQty = 100;
			package1.CW_PackType = "VG";
			package1.CW_MarksAndNos = "M&N1";
			var package2 = declaration.Packages.AddNew();
			package2.CW_CR_HouseContainer = billPackingGroup.PK;
			package2.CW_PackQty = 200;
			package2.CW_PackType = "CT";
			package2.CW_MarksAndNos = "M&N2";

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "INV2";
			var invoice2line1 = invoice2.InvoiceLines.AddNew();
			invoice2line1.JI_LineNo = 1;
			invoice2line1.JI_Description = "invoice2line1 stuff";
			invoice2line1.JI_Weight = 2.1m;
			invoice2line1.JI_WeightUQ = "KG";

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV1";
			var invoice1line2 = invoice1.InvoiceLines.AddNew();
			invoice1line2.JI_LineNo = 2;
			invoice1line2.JI_InvoiceQuantity = 120m;
			invoice1line2.JI_InvoiceUQ = "PAC";
			invoice1line2.JI_Description = "invoice1line2 stuff";
			invoice1line2.JI_Weight = 1.2m;
			invoice1line2.JI_WeightUQ = "KG";
			var packageCtInvoiceLine2 = invoice1line2.PackagesPivot.AddNew();
			packageCtInvoiceLine2.CHC_CW = package2.PK;
			packageCtInvoiceLine2.CHC_NumberOfPacks = 150;

			var invoice1line1 = invoice1.InvoiceLines.AddNew();
			invoice1line1.JI_LineNo = 1;
			invoice1line1.JI_InvoiceQuantity = 110m;
			invoice1line1.JI_InvoiceUQ = "BOX";
			invoice1line1.JI_Description = "invoice1line1 stuff";
			invoice1line1.JI_Weight = 1.1m;
			invoice1line1.JI_WeightUQ = "KG";
			invoice1line1.JI_Volume = 22m;
			invoice1line1.JI_VolumeUQ = "M3";
			var packageVgInvoiceLine1 = invoice1line1.PackagesPivot.AddNew();
			packageVgInvoiceLine1.CHC_CW = package1.PK;
			packageVgInvoiceLine1.CHC_NumberOfPacks = 99;
			var packageCtInvoiceLine1 = invoice1line1.PackagesPivot.AddNew();
			packageCtInvoiceLine1.CHC_CW = package2.PK;
			packageCtInvoiceLine1.CHC_NumberOfPacks = 50;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			var entryLine2 = entryHeader.MergedLines.AddNew();
			var entryLine3 = entryHeader.MergedLines.AddNew();

			invoice1line1.JI_CL = entryLine1.PK;
			invoice1line2.JI_CL = entryLine2.PK;
			invoice2line1.JI_CL = entryLine3.PK;

			CombineAssertions(() =>
			{
				var declarationDataObject = new JobDeclarationDocDataObject(new EURCertificateOfOriginWrapper(entryHeader));
				AssertDataObjectPropertyIsAlterable(declarationDataObject, "Australia", nameof(declarationDataObject.OriginCountry));
				AssertDataObjectPropertyIsAlterable(declarationDataObject, "Australia", nameof(declarationDataObject.OriginGroup));
				AssertDataObjectPropertyIsAlterable(declarationDataObject, "Germany", nameof(declarationDataObject.DestinationCountry));
				AssertDataObjectPropertyIsAlterable(declarationDataObject, "Europe EUR1", nameof(declarationDataObject.DestinationGroup));
				AssertDataObjectPropertyIsAlterable(declarationDataObject, "KL123", nameof(declarationDataObject.VoyageFlight));

				var exporterDetails = @"FREDS SUPPLY CO
367 GEORGE ST
SYDNEY NSW 2000
AUSTRALIA";
				AssertDataObjectPropertyIsAlterable(declarationDataObject, exporterDetails, nameof(declarationDataObject.Exporter));

				var importerDetails = @"JIMS IMPORT CO
PETUELRING 130
80809 MÜNCHEN
GERMANY";
				AssertDataObjectPropertyIsAlterable(declarationDataObject, importerDetails, nameof(declarationDataObject.Importer));

				var items = @"1; M&N1, M&N2; 99 VG, 50 CT; INVOICE1LINE1 STUFF
2; M&N2; 150 CT; INVOICE1LINE2 STUFF
3; INVOICE2LINE1 STUFF
-------------------------------------------------------------------------------------------------------
";
				AssertDataObjectPropertyIsAlterable(declarationDataObject, items, nameof(declarationDataObject.Items));

				var massEntries = @"1.1 KG
1.2 KG
2.1 KG";
				AssertDataObjectPropertyIsAlterable(declarationDataObject, massEntries, nameof(declarationDataObject.ItemMassVolume));

				var invoices = @"INV1
INV2";
				AssertDataObjectPropertyIsAlterable(declarationDataObject, invoices, nameof(declarationDataObject.InvoiceNumbers));
			});
		}

		[ExpectNoExceptions]
		public void TestEUR1Pg1Box11CustomsEndorsement()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var declarationDataObject = new JobDeclarationDocDataObject(new EURCertificateOfOriginWrapper(entryHeader));

			NUnit.Framework.Assert.That(declarationDataObject.EUR1Pg1Box11CustomsEndorsement, NUnit.Framework.Is.Not.EqualTo(default(CustomsEndorsement)), "EUR1Pg1Box11CustomsEndorsement - should not be [null]");
			var eur1Pg1Box11CustomsEndorsement = declarationDataObject.EUR1Pg1Box11CustomsEndorsement;
			NUnit.Framework.Assert.That(declarationDataObject.EUR1Pg1Box11CustomsEndorsement, NUnit.Framework.Is.SameAs(eur1Pg1Box11CustomsEndorsement), "EUR1Pg1Box11CustomsEndorsement must be cached");
		}

		[ExpectNoExceptions]
		public void TestEUR1Pg1Box12DeclarationExporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var declarationDataObject = new JobDeclarationDocDataObject(new EURCertificateOfOriginWrapper(entryHeader));

			NUnit.Framework.Assert.That(declarationDataObject.EUR1Pg1Box12DeclarationExporter, NUnit.Framework.Is.Not.EqualTo(default(DeclarationExporter)), "EUR1Pg1Box12DeclarationExporter - should not be [null]");
			var eur1Pg1Box12DeclarationExporter = declarationDataObject.EUR1Pg1Box12DeclarationExporter;
			NUnit.Framework.Assert.That(declarationDataObject.EUR1Pg1Box12DeclarationExporter, NUnit.Framework.Is.SameAs(eur1Pg1Box12DeclarationExporter), "EUR1Pg1Box12DeclarationExporter must be cached");
		}

		[ExpectNoExceptions]
		public void TestVoyageFlight()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_VesselName = "VESSEL 1";
			declaration.JE_VoyageFlightNo = "VOYAGE 1";
			var declarationDataObject = new JobDeclarationDocDataObject(new EURCertificateOfOriginWrapper(entryHeader));
			NUnit.Framework.Assert.That(declarationDataObject.VoyageFlight, NUnit.Framework.Is.EqualTo("VESSEL 1 VOYAGE 1").Using(CustomComparers.TypeComparison), "When Transport mode is SEA, VoyageFlight");

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_VoyageFlightNo = "AZ123";
			declarationDataObject = new JobDeclarationDocDataObject(new EURCertificateOfOriginWrapper(entryHeader));
			NUnit.Framework.Assert.That(declarationDataObject.VoyageFlight, NUnit.Framework.Is.EqualTo("AZ123").Using(CustomComparers.TypeComparison), "When Transport mode is AIR, VoyageFlight");

			declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
			declaration.JE_VesselName = "RAIL";
			declarationDataObject = new JobDeclarationDocDataObject(new EURCertificateOfOriginWrapper(entryHeader));
			NUnit.Framework.Assert.That(declarationDataObject.VoyageFlight, NUnit.Framework.Is.EqualTo("RAIL").Using(CustomComparers.TypeComparison), "When Transport mode is \"Other transports (RAIL, ROA, ...)\", VoyageFlight");

			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			declaration.JE_VesselName = "ROAD";
			declarationDataObject = new JobDeclarationDocDataObject(new EURCertificateOfOriginWrapper(entryHeader));
			NUnit.Framework.Assert.That(declarationDataObject.VoyageFlight, NUnit.Framework.Is.EqualTo("ROAD").Using(CustomComparers.TypeComparison), "When Transport mode is \"Other transports (RAIL, ROA, ...)\", VoyageFlight");
		}
	}
}
