using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Documents.CertificateOfOrigin;
using Enterprise.Customs.EU.Business.Documents.DocDataObjects;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Moq;

namespace Enterprise.Customs.ES.Business.Documents.DocDataObjects.Testing
{
	class CustomsDocDataObjectProviderTest : TestCaseWithFactory
	{
		public void TestGetATRCertificateOfOriginForEntry()
		{
			var entryHeader = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			var provider = new CustomsDocDataObjectProviderForTest();
			var parameters = new Mock<IDocDataObjectParameters>().Object;
			AssertType<ATRCertificateOfOriginWrapper>("ATR Certificate of origin wrapper (for entry header) Type", provider.GetATRCertificateOfOriginForEntryExposed(entryHeader, parameters));
		}

		public void TestGetJobDeclarationDocDataObjectDataProviderForEntry()
		{
			var entryHeader = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			var provider = new CustomsDocDataObjectProviderForTest();

			var parameters = new Mock<IDocDataObjectParameters>().Object;
			AssertType<EURCertificateOfOriginWrapper>("JobDeclarationDocDataObjectDataProvider (for entry header) Type", provider.GetEURCertificateOfOriginForEntryExposed(entryHeader, parameters));
		}

		public void TestGetDocDataObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var provider = new CustomsDocDataObjectProvider();

			var parameters = new DocDataObjectParameters("DataObjectParameters", "a");

			CombineAssertions(() =>
			{
				var eur1Parameters = new DocDataObjectParameters("EUR1 Certificate", "X");
				var dataObject = provider.GetDocDataObject(entryHeader, DataContext.JobDeclaration, eur1Parameters);
				AssertNotNull("should return a data object for entry header", dataObject);
				AssertType<JobDeclarationDocDataObject>("should return a Enterprise.Customs.EU.Business.Documents.DocDataObjects.JobDeclarationDocDataObject object when DocumentTitle is EUR1", dataObject);
			});
		}

		public void TestGetATRCertificateDataObjectForEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var provider = new CustomsDocDataObjectProviderForTest();

			var parameters = new DocDataObjectParameters("DataObjectParameters", "a");

			CombineAssertions(() =>
			{
				var atrParameters = new DocDataObjectParameters("ATR Certificate", "X");
				var atrDataObject = provider.GetDocDataObject(entryHeader, DataContext.ATRCertificate, atrParameters);
				AssertNotNull("should return a data object for declaration", atrDataObject);
				AssertType<ATRCertificateDocDataObject>("should return a Enterprise.Customs.ES.Business.Documents.DocDataObjects.ATRCertificateDocDataObject object when DocumentTitle is ATR", atrDataObject);
			});
		}

		public void TestEUR1FormIsTranslatedToSpanish()
		{
			var spanish = Res.GetLanguageInstance(Enterprise.Core.SharedConstants.Languages.Spanish);
			var spanishMock = spanish.UseMockData();
			spanishMock.Put("B376EB9E-A2E1-457F-BD2D-C00F700959DF", new ResourceStringData("B376EB9E-A2E1-457F-BD2D-C00F700959DF", "I am first Spanish translation: MOVEMENT CERTIFICATE"));
			spanishMock.Put("D6DDFD33-192C-4ACD-88A5-CEE6E1C4EFEF", new ResourceStringData("D6DDFD33-192C-4ACD-88A5-CEE6E1C4EFEF", "I am second Spanish translation: APPLICATION FOR A MOVEMENT CERTIFICATE"));
			spanishMock.Put("516A9519-B7D4-400A-8AEB-8155ABB69D08", new ResourceStringData("516A9519-B7D4-400A-8AEB-8155ABB69D08", "I am Spanish translation: TOTAL"));
			spanishMock.Put("10A58664-29C0-48A6-B9C5-87EC7EF24F50", new ResourceStringData("10A58664-29C0-48A6-B9C5-87EC7EF24F50", "I am Spanish translation: PACKAGES"));
			spanishMock.Put("BD27AE69-96C3-4165-AD95-BB07668BACF1", new ResourceStringData("BD27AE69-96C3-4165-AD95-BB07668BACF1", "I am Spanish translation: TOTAL GROSS WEIGHT"));
			spanishMock.Put("1BEB56AA-A439-4FE1-92C0-2A60DCEC6B99", new ResourceStringData("1BEB56AA-A439-4FE1-92C0-2A60DCEC6B99", "I am Spanish translation: FRAME"));

			var expectedItemsText = @"1 PDTA: 123
1 I am Spanish translation: FRAME, 123 BRAND-AH MODEL-C.
line1 stuff

I am Spanish translation: TOTAL GROSS WEIGHT 1,2 Kg 

I am Spanish translation: TOTAL 1 I am Spanish translation: PACKAGES
-------------------------------------------------------------------------------------------------------";

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var vehicle = invoiceLine.Vehicles.AddNew();
			invoiceLine.JI_Tariff = "123";
			invoiceLine.JI_LineNo = 2;
			invoiceLine.JI_InvoiceQuantity = 120m;
			invoiceLine.JI_InvoiceUQ = "PAC";
			invoiceLine.JI_Description = "line1 stuff";
			invoiceLine.JI_Weight = 1.2m;
			invoiceLine.JI_WeightUQ = "KG";

			vehicle.CVH_VehicleIdentificationNumber = "123";
			vehicle.CVH_BrandName = "BRAND-AH";
			vehicle.CVH_ModelName = "MODEL-C";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var eur1Parameters = new DocDataObjectParameters("EUR1 Certificate", "X");

			AssertEUR1Translations("For entry as parent", entry, eur1Parameters, expectedItemsText);
		}

		void AssertEUR1Translations(string message, object parent, DocDataObjectParameters eur1Parameters, string expectedItemsText)
		{
			CombineAssertions(message, () =>
			{
				var provider = new CustomsDocDataObjectProvider();
				var docDataObject = provider.GetDocDataObject(parent, DataContext.JobDeclaration, eur1Parameters);
				var declarationDocDataObject = (JobDeclarationDocDataObject)docDataObject;
				AssertEquals("I am first Spanish translation: MOVEMENT CERTIFICATE", declarationDocDataObject.EUR1Pg1DocumentTitle);
				AssertEquals("I am second Spanish translation: APPLICATION FOR A MOVEMENT CERTIFICATE", declarationDocDataObject.EUR1Pg3DocumentTitle);
				AssertEquals(expectedItemsText, declarationDocDataObject.Items);
			});
		}

		public void TestEURMEDFormIsTranslatedToSpanish()
		{
			var spanish = Res.GetLanguageInstance(Enterprise.Core.SharedConstants.Languages.Spanish);
			var spanishMock = spanish.UseMockData();
			spanishMock.Put("D6DDFD33-192C-4ACD-88A5-CEE6E1C4EFEF", new ResourceStringData("D6DDFD33-192C-4ACD-88A5-CEE6E1C4EFEF", "I am first Spanish translation: APPLICATION FOR A MOVEMENT CERTIFICATE"));
			spanishMock.Put("584840A1-F17B-40F5-AB37-446A3D9D2FC5", new ResourceStringData("584840A1-F17B-40F5-AB37-446A3D9D2FC5", "I am second Spanish translation: (Name, full address, country)"));
			spanishMock.Put("516A9519-B7D4-400A-8AEB-8155ABB69D08", new ResourceStringData("516A9519-B7D4-400A-8AEB-8155ABB69D08", "I am Spanish translation: TOTAL"));
			spanishMock.Put("10A58664-29C0-48A6-B9C5-87EC7EF24F50", new ResourceStringData("10A58664-29C0-48A6-B9C5-87EC7EF24F50", "I am Spanish translation: PACKAGES"));
			spanishMock.Put("BD27AE69-96C3-4165-AD95-BB07668BACF1", new ResourceStringData("BD27AE69-96C3-4165-AD95-BB07668BACF1", "I am Spanish translation: TOTAL GROSS WEIGHT"));
			spanishMock.Put("1BEB56AA-A439-4FE1-92C0-2A60DCEC6B99", new ResourceStringData("1BEB56AA-A439-4FE1-92C0-2A60DCEC6B99", "I am Spanish translation: FRAME"));
			spanishMock.Put("D239BB93-9FAF-4F4F-B9B3-0B76A6BB77BF", new ResourceStringData("D239BB93-9FAF-4F4F-B9B3-0B76A6BB77BF", "I am Spanish translation: SPAIN"));

			var expectedItemsText = @"1 PDTA: 123
1 I am Spanish translation: FRAME, 123 BRAND-AH MODEL-C.
line1 stuff

I am Spanish translation: TOTAL GROSS WEIGHT 1,2 Kg 

I am Spanish translation: TOTAL 1 I am Spanish translation: PACKAGES
-------------------------------------------------------------------------------------------------------";

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var vehicle = invoiceLine.Vehicles.AddNew();
			invoiceLine.JI_Tariff = "123";
			invoiceLine.JI_LineNo = 2;
			invoiceLine.JI_InvoiceQuantity = 120m;
			invoiceLine.JI_InvoiceUQ = "PAC";
			invoiceLine.JI_Description = "line1 stuff";
			invoiceLine.JI_Weight = 1.2m;
			invoiceLine.JI_WeightUQ = "KG";

			vehicle.CVH_VehicleIdentificationNumber = "123";
			vehicle.CVH_BrandName = "BRAND-AH";
			vehicle.CVH_ModelName = "MODEL-C";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var eurmedParameters = new DocDataObjectParameters("EUR-MED Certificate", "X");

			AssertEURMEDTranslations("For entry as parent", entry, eurmedParameters, expectedItemsText);
		}

		void AssertEURMEDTranslations(string message, object parent, DocDataObjectParameters eurmedParameters, string expectedItemsText)
		{
			CombineAssertions(message, () =>
			{
				var provider = new CustomsDocDataObjectProvider();
				var docDataObject = provider.GetDocDataObject(parent, DataContext.EURMEDCertificate, eurmedParameters);
				var declarationDocDataObject = (JobDeclarationDocDataObject)docDataObject;
				AssertEquals("I am first Spanish translation: APPLICATION FOR A MOVEMENT CERTIFICATE", declarationDocDataObject.EUR1Pg3DocumentTitle);
				AssertEquals("I am second Spanish translation: (Name, full address, country)", declarationDocDataObject.EUR1Pg1Box1CaptionHint);
				AssertEquals(expectedItemsText, declarationDocDataObject.Items);
				AssertEquals("I AM SPANISH TRANSLATION: SPAIN", declarationDocDataObject.EUR1Pg1Box11CustomsEndorsement.IssuingCountry);
			});
		}

		public void TestATRFormIsTranslatedToSpanish()
		{
			var spanish = Res.GetLanguageInstance(Enterprise.Core.SharedConstants.Languages.Spanish);
			var spanishMock = spanish.UseMockData();
			spanishMock.Put("E819ACB7-9779-49B4-9FF1-A8D7B61A8932", new ResourceStringData("E819ACB7-9779-49B4-9FF1-A8D7B61A8932", "I am first Spanish translation: Movement Certificate"));
			spanishMock.Put("2756FDCE-52AD-4A65-B780-26B7D1EA477D", new ResourceStringData("2756FDCE-52AD-4A65-B780-26B7D1EA477D", "I am second Spanish translation: A.TR.1 N�"));
			spanishMock.Put("516A9519-B7D4-400A-8AEB-8155ABB69D08", new ResourceStringData("516A9519-B7D4-400A-8AEB-8155ABB69D08", "I am Spanish translation: TOTAL"));
			spanishMock.Put("10A58664-29C0-48A6-B9C5-87EC7EF24F50", new ResourceStringData("10A58664-29C0-48A6-B9C5-87EC7EF24F50", "I am Spanish translation: PACKAGES"));
			spanishMock.Put("BD27AE69-96C3-4165-AD95-BB07668BACF1", new ResourceStringData("BD27AE69-96C3-4165-AD95-BB07668BACF1", "I am Spanish translation: TOTAL GROSS WEIGHT"));
			spanishMock.Put("1BEB56AA-A439-4FE1-92C0-2A60DCEC6B99", new ResourceStringData("1BEB56AA-A439-4FE1-92C0-2A60DCEC6B99", "I am Spanish translation: FRAME"));
			spanishMock.Put("D239BB93-9FAF-4F4F-B9B3-0B76A6BB77BF", new ResourceStringData("D239BB93-9FAF-4F4F-B9B3-0B76A6BB77BF", "I am Spanish translation: SPAIN"));

			var expectedMarksNumbersText = @"PDTA: 123
1 I AM SPANISH TRANSLATION: FRAME, 123 BRAND-AH MODEL-C.
LINE1 STUFF

I am Spanish translation: TOTAL GROSS WEIGHT 1,2 KG

I am Spanish translation: TOTAL 1 I am Spanish translation: PACKAGES";

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var vehicle = invoiceLine.Vehicles.AddNew();
			invoiceLine.JI_Tariff = "123";
			invoiceLine.JI_LineNo = 2;
			invoiceLine.JI_InvoiceQuantity = 120m;
			invoiceLine.JI_InvoiceUQ = "PAC";
			invoiceLine.JI_Description = "line1 stuff";
			invoiceLine.JI_Weight = 1.2m;
			invoiceLine.JI_WeightUQ = "KG";

			vehicle.CVH_VehicleIdentificationNumber = "123";
			vehicle.CVH_BrandName = "BRAND-AH";
			vehicle.CVH_ModelName = "MODEL-C";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var atrParameters = new DocDataObjectParameters("ATR Certificate", "X");

			AssertATRTranslations("For entry as parent", entry, atrParameters, expectedMarksNumbersText);
		}

		void AssertATRTranslations(string message, object parent, DocDataObjectParameters atrParameters, string expectedMarksNumbersText)
		{
			CombineAssertions(message, () =>
			{
				var provider = new CustomsDocDataObjectProvider();
				var docDataObject = provider.GetDocDataObject(parent, DataContext.ATRCertificate, atrParameters);
				var declarationDocDataObject = (ATRCertificateDocDataObject)docDataObject;
				AssertEquals("I am first Spanish translation: Movement Certificate", declarationDocDataObject.ARTMovementCertificateCaption);
				AssertEquals("I am second Spanish translation: A.TR.1 N�", declarationDocDataObject.ARTNumberCaption);
				AssertEquals(expectedMarksNumbersText, declarationDocDataObject.MarksNumbers);
				AssertEquals("I AM SPANISH TRANSLATION: SPAIN", declarationDocDataObject.Box12CustomsEndorsement.IssuingCountry);
			});
		}

		public void TestDV1CertificateForEntryType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var esEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var provider = new CustomsDocDataObjectProviderForTest();

			var parameters = new DocDataObjectParameters("DataObjectParameters", "a");

			var dV1Certificate = provider.GetDV1CertificateForEntryExposed(esEntryHeader, parameters);
			AssertType<DV1CertificateWrapper>("DV1Certificate type", dV1Certificate);
		}

		#region CustomsDocDataObjectProviderForTest

		public class CustomsDocDataObjectProviderForTest : CustomsDocDataObjectProvider
		{
			public EU.Business.Documents.CertificateOfOrigin.IEURCertificateOfOrigin GetEURCertificateOfOriginForEntryExposed(CusEntryHeader entryHeader, IDocDataObjectParameters parameters)
			{
				return GetEURCertificateOfOriginForEntry(entryHeader, parameters);
			}

			public EU.Business.Documents.CertificateOfOrigin.IATRCertificateOfOrigin GetATRCertificateOfOriginForEntryExposed(CusEntryHeader entryHeader, IDocDataObjectParameters parameters) => GetATRCertificateOfOriginForEntry(entryHeader, parameters);

			public EU.Business.Documents.CertificateOfOrigin.IDV1Certificate GetDV1CertificateForEntryExposed(CusEntryHeader entryHeader, IDocDataObjectParameters parameters) => GetDV1CertificateForEntry(entryHeader, parameters);
		}

		#endregion
	}
}
