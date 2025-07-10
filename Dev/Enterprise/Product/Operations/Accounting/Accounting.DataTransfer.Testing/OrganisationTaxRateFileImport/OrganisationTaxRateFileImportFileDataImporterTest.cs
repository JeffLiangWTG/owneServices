using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.Invoices.Testing;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.DataTransfer.Testing
{	
	sealed class OrganisationTaxRateFileImportFileDataImporterTest : FlatFileDataImporterTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestIGlobalAccountingCountryFactory_Parameters()
		{
			var orgTaxConfiguration = SetupTaxFrameworkAndGetTaxConfiguration();

			var mockIOrgTaxRateImportFileFormat = new Mock<IOrgTaxRateImportFileFormat>();
			var mockIOrgTaxRateImportFileFormatProvider = new Mock<IOrgTaxRateImportFileFormatProvider>();
			mockIOrgTaxRateImportFileFormatProvider.Setup(x => x.GetFileFormat(It.IsAny<ZString>())).Returns(mockIOrgTaxRateImportFileFormat.Object);

			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			mockIAccountingCountryFactory.As<IInstanceProvider<IOrgTaxRateImportFileFormatProvider>>().Setup(x => x.Get()).Returns(mockIOrgTaxRateImportFileFormatProvider.Object);

			var mockAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			mockAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

			ObjectFactory.Substitute(mockAccountingCountryFactory.Object);

			var importBizo = new OrganisationTaxRateFileImport();
			importBizo.TaxConfiguration = orgTaxConfiguration.PK;

			var importer = ObjectFactory.Get<IOrganisationTaxRateFileImportFileDataImporter>();
			importer.ImportData(PathToTestFile, new NotificationBuffer(), importBizo);

			mockAccountingCountryFactory.Verify(x => x.GetCountryFactory("XX"), Times.Once);

			importBizo.TaxConfigurationObject.ETC_RN_NKCountry = "UY";
			importer.ImportData(PathToTestFile, new NotificationBuffer(), importBizo);
			mockAccountingCountryFactory.Verify(x => x.GetCountryFactory("UY"), Times.Once);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestIOrgTaxRateImportFileFormatProvider_Parameters()
		{
			var expectedTaxAuthorityCodeToGetFormat = "TA01";

			var orgTaxConfiguration = SetupTaxFrameworkAndGetTaxConfiguration(expectedTaxAuthorityCodeToGetFormat);

			var mockIOrgTaxRateImportFileFormat = new Mock<IOrgTaxRateImportFileFormat>();
			var mockIOrgTaxRateImportFileFormatProvider = new Mock<IOrgTaxRateImportFileFormatProvider>();
			mockIOrgTaxRateImportFileFormatProvider.Setup(x => x.GetFileFormat(It.IsAny<ZString>())).Returns(mockIOrgTaxRateImportFileFormat.Object);

			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			mockIAccountingCountryFactory.As<IInstanceProvider<IOrgTaxRateImportFileFormatProvider>>().Setup(x => x.Get()).Returns(mockIOrgTaxRateImportFileFormatProvider.Object);

			var mockAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			mockAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

			ObjectFactory.Substitute(mockAccountingCountryFactory.Object);

			var importBizo = new OrganisationTaxRateFileImport();
			importBizo.TaxConfiguration = orgTaxConfiguration.PK;

			var importer = ObjectFactory.Get<IOrganisationTaxRateFileImportFileDataImporter>();
			importer.ImportData(PathToTestFile, new NotificationBuffer(), importBizo);

			mockIOrgTaxRateImportFileFormatProvider.Verify(x => x.GetFileFormat(expectedTaxAuthorityCodeToGetFormat), Times.Once);

			expectedTaxAuthorityCodeToGetFormat = "XX123";
			orgTaxConfiguration = SetupTaxFrameworkAndGetTaxConfiguration(expectedTaxAuthorityCodeToGetFormat);
			importBizo.TaxConfiguration = orgTaxConfiguration.PK;

			importer.ImportData(PathToTestFile, new NotificationBuffer(), importBizo);
			mockIOrgTaxRateImportFileFormatProvider.Verify(x => x.GetFileFormat(expectedTaxAuthorityCodeToGetFormat), Times.Once);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNotificationsContainsErrorMessage()
		{
			var orgTaxConfiguration = SetupTaxFrameworkAndGetTaxConfiguration();
			var regNumbersToImport = GetRegNumbersToImport("20000948676", orgTaxConfiguration.PK);
			AssertEquals("Precondition: RegNumberToImport dictionary must be have only one key element.", 1, regNumbersToImport.Count);

			SetUpMock_For_OrganisationTaxRateImportDataProvider(regNumbersToImport, false);

			var importBizo = new OrganisationTaxRateFileImport();

			var notifications = new NotificationBuffer();
			NotificationTestHelper notificationTestHelper = new NotificationTestHelper();

			var importer = ObjectFactory.Get<IOrganisationTaxRateFileImportFileDataImporter>();

			importer.ImportData(PathToTestFile, notifications, importBizo);
			AssertEquals("Notification should have errors - Buffer:" + notifications.AsString, true, notifications.HasErrors);
			notificationTestHelper.AssertNotificationsContainsErrorMessage(notifications, "Tax Authority Code as part of the Tax Configuration cannot be null.");

			importBizo.TaxConfiguration = orgTaxConfiguration.PK;
			importer.ImportData(PathToTestFile, notifications, importBizo);

			AssertEquals("Notification should have errors - Buffer:" + notifications.AsString, true, notifications.HasErrors);
			notificationTestHelper.AssertNotificationsContainsErrorMessage(notifications, "Tax Rate File Format does not exist for country 'XX', tax authority 'XXX'.");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportLineWithCorrectProperties()
		{
			var orgTaxConfiguration = SetupTaxFrameworkAndGetTaxConfiguration();
			var regNumbersToImport = GetRegNumbersToImport("20000948676", orgTaxConfiguration.PK);
			AssertEquals("Precondition: RegNumberToImport dictionary must be have only one key element.", 1, regNumbersToImport.Count);

			SetUpMock_For_OrganisationTaxRateImportDataProvider(regNumbersToImport);

			var importBizo = new OrganisationTaxRateFileImport();
			importBizo.TaxConfiguration = orgTaxConfiguration.PK;

			var importer = ObjectFactory.Get<IOrganisationTaxRateFileImportFileDataImporter>();

			var notifications = new NotificationBuffer();
			int expectedCountingLines = 1;

			importer.ImportData(PathToTestFile, notifications, importBizo);
			Assert(!notifications.HasErrors);
			Assert(!notifications.HasWarnings);

			AssertEquals(expectedCountingLines, importBizo.ImportLines.Count);

			AssertEquals(new ZDate(2022, 02, 01), importBizo.ImportLines[0].StartDate);
			AssertEquals(new ZDate(2022, 02, 28), importBizo.ImportLines[0].EndDate);
			AssertEquals("AROR1", importBizo.ImportLines[0].OrganizationCode);
			AssertEquals("ORGANISATION CW1 ONE", importBizo.ImportLines[0].OrganizationName);
			AssertEquals(12, importBizo.ImportLines[0].RateNumerator);
			AssertEquals(10, importBizo.ImportLines[0].RateDenominator);
			AssertEquals("", importBizo.ImportLines[0].RateSource);
			AssertEquals("20000948676", importBizo.ImportLines[0].RegistrationCode);
			AssertEquals(orgTaxConfiguration.PK, importBizo.ImportLines[0].TaxConfigurationPK);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportLineWithCorrectProperties_ManyOrg()
		{
			var orgTaxConfiguration = SetupTaxFrameworkAndGetTaxConfiguration("XXX");
			var expectedCUIT = "20000948676";
			var expectedCountingLines = 3;

			var regNumbersToImport = GetRegNumbersToImportWithManyOrgs(expectedCUIT, orgTaxConfiguration.PK);
			regNumbersToImport.TryGetValue(expectedCUIT, out IReadOnlyList<(ZString OrgCode, ZString OrgName, ZGuid AccOrgTaxConfigurationPK)> expectedOrgsList);

			AssertEquals("Precondition: RegNumberToImport dictionary must be have only one key element.", 1, regNumbersToImport.Count);
			AssertEquals("Precondition: RegNumberToImport dictionary must be have three orgs in list value with the same orgcus code value.", expectedCountingLines, expectedOrgsList.Count);

			SetUpMock_For_OrganisationTaxRateImportDataProvider(regNumbersToImport);

			var importBizo = new OrganisationTaxRateFileImport();
			importBizo.TaxConfiguration = orgTaxConfiguration.PK;

			var importer = ObjectFactory.Get<IOrganisationTaxRateFileImportFileDataImporter>();
			var notifications = new NotificationBuffer();

			importer.ImportData(PathToTestFile, notifications, importBizo);
			Assert(!notifications.HasErrors);
			Assert(!notifications.HasWarnings);

			for (var i = 0; i < importBizo.ImportLines.Count; i++)
			{
				var currentBizoLine = importBizo.ImportLines[i];

				AssertEquals(new ZDate(2022, 02, 01), currentBizoLine.StartDate);
				AssertEquals(new ZDate(2022, 02, 28), currentBizoLine.EndDate);
				AssertEquals(12, currentBizoLine.RateNumerator);
				AssertEquals(10, currentBizoLine.RateDenominator);
				AssertEquals("", currentBizoLine.RateSource);

				AssertEquals(expectedOrgsList[i].OrgCode, currentBizoLine.OrganizationCode);
				AssertEquals(expectedOrgsList[i].OrgName, currentBizoLine.OrganizationName);
				AssertEquals(expectedOrgsList[i].AccOrgTaxConfigurationPK, currentBizoLine.TaxConfigurationPK);

				AssertEquals(expectedCUIT, currentBizoLine.RegistrationCode);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOrganisationDoesNotExistInPadronesFile()
		{
			AssertFileImport("00000000000", PathToTestFile);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWrongFileStructure()
		{
			AssertFileImport("20000948676", PathToWrongStructureTestFile);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDistinctDelimiter()
		{
			AssertFileImport("20000948676", PathToFileWithPipeDelimiter);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWrongDataContent()
		{
			AssertFileImport("20000948676", PathToWrongDataTestFile);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDistinctNumberOfColumnsInFormat()
		{
			AssertFileImport("20000948676", PathToFileWithDistinctNumberOfColumnsInFormat);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestValidationIsSuspendedDuringImport()
		{
			var orgTaxConfiguration = SetupTaxFrameworkAndGetTaxConfiguration();
			var regNumbersToImport = GetRegNumbersToImport("20000948676", orgTaxConfiguration.PK);
			AssertEquals("Precondition: RegNumberToImport dictionary must be have only one key element.", 1, regNumbersToImport.Count);

			SetUpMock_For_OrganisationTaxRateImportDataProvider(regNumbersToImport);

			var expectedStartDate = new ZDate(2022, 12, 01);
			var expectedEndDate = new ZDate(2021, 12, 01);

			var importBizo = new OrganisationTaxRateFileImport();
			importBizo.TaxConfiguration = orgTaxConfiguration.PK;
			var notifications = new NotificationBuffer();

			var precondtionTestLine = new OrganisationTaxRateFileImportLine(Factory);
			precondtionTestLine.StartDate = expectedStartDate;
			precondtionTestLine.EndDate = expectedEndDate;

			AssertHasErrors("Precondition", precondtionTestLine.EndDateInfo);

			var importer = ObjectFactory.Get<IOrganisationTaxRateFileImportFileDataImporter>();
			importer.ImportData(PathToWrongDataValidationSuspendedTestFile, notifications, importBizo);

			Assert(!notifications.HasErrors);
			Assert(!notifications.HasWarnings);
			AssertEquals(1, importBizo.ImportLines.Count);

			var importLine = importBizo.ImportLines[0];

			AssertEquals("Postcondition: OTR_StartDate", new ZDate(2022, 12, 01), importLine.StartDate);
			AssertEquals("Postcondition: OTR_EndDate", new ZDate(2022, 12, 01), importLine.EndDate);
			AssertNoErrors(nameof(importLine), importLine.EndDateInfo);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestStartDateWithInvalidFormat()
		{
			var importBizo = AssertFileImport("20000948676", PathToStartDateInvalidFormatFile, expectedCountingLines: 1);
			Assert("Start date is empty when is not imported in ddMMyyyy format", importBizo.ImportLines[0].StartDate.IsEmpty);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEndtDateWithInvalidFormat()
		{
			var importBizo = AssertFileImport("20000948676", PathToEndDateInvalidFormatFile, expectedCountingLines: 1);
			Assert("End date is empty when is not imported in ddMMyyyy format", importBizo.ImportLines[0].EndDate.IsEmpty);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRateWithDiferentCultureInfo()
		{
			var importBizo = AssertFileImport("20000948676", PathToRateWithInvalidCultureFile, expectedCountingLines: 1);
			AssertEquals(0, importBizo.ImportLines[0].RateNumerator);
			AssertEquals(1, importBizo.ImportLines[0].RateDenominator);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUserNotificationErrorFileNotFound()
		{
			var orgTaxConfiguration = SetupTaxFrameworkAndGetTaxConfiguration();
			var regNumbersToImport = GetRegNumbersToImport("20000948676", orgTaxConfiguration.PK);
			AssertEquals("Precondition: RegNumberToImport dictionary must be have only one key element.", 1, regNumbersToImport.Count);

			SetUpMock_For_OrganisationTaxRateImportDataProvider(regNumbersToImport);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var importBizo = new OrganisationTaxRateFileImport();
			importBizo.TaxConfiguration = orgTaxConfiguration.PK;
			var importer = ObjectFactory.Get<IOrganisationTaxRateFileImportFileDataImporter>();

			var notifications = new NotificationBuffer();
			importer.ImportData(PathToFileDoesNotExist, notifications, importBizo);

			Assert("Expected error", notifications.HasErrors);
			Assert(!notifications.HasWarnings);

			AssertStartsWith("Expected error message start.", "Could not find file '", notifications.AsString);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCleanupDataRow()
		{
			var orgTaxConfiguration = SetupTaxFrameworkAndGetTaxConfiguration();

			var regNumbersToImport = GetRegNumbersToImport("20000948676", orgTaxConfiguration.PK);
			AssertEquals("Precondition: RegNumberToImport dictionary must be have only one key element.", 1, regNumbersToImport.Count);

			#region Mocks

			var mockOrganisation = new Mock<IOrganisationTaxRateImportDataProvider>();
			mockOrganisation.Setup(x => x.RegNumbersToImport(It.IsAny<BusinessObjectFactory>(), It.IsAny<ZGuid>())).Returns(regNumbersToImport);
			var mockFramework = new Mock<ITaxFrameworkDependencyFactory>();
			mockFramework.Setup(x => x.GetOrganisationTaxRateImportDataProvider()).Returns(mockOrganisation.Object);

			var mockIOrgTaxRateImportFileFormat = new Mock<IOrgTaxRateImportFileFormat>();
			var mockIOrgTaxRateImportFileFormatProvider = new Mock<IOrgTaxRateImportFileFormatProvider>();
			mockIOrgTaxRateImportFileFormat.Setup(x => x.StartDate).Returns(1);
			mockIOrgTaxRateImportFileFormat.Setup(x => x.EndDate).Returns(2);
			mockIOrgTaxRateImportFileFormat.Setup(x => x.RegistrationCode).Returns(3);
			mockIOrgTaxRateImportFileFormat.Setup(x => x.PerceptionRate).Returns(7);
			mockIOrgTaxRateImportFileFormat.Setup(x => x.Delimiter).Returns(';');
			mockIOrgTaxRateImportFileFormat.Setup(x => x.NumberOfColumnsInFormat).Returns(12);

			mockIOrgTaxRateImportFileFormatProvider.Setup(x => x.GetFileFormat(It.IsAny<ZString>())).Returns(mockIOrgTaxRateImportFileFormat.Object);

			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			mockIAccountingCountryFactory.As<IInstanceProvider<IOrgTaxRateImportFileFormatProvider>>().Setup(x => x.Get()).Returns(mockIOrgTaxRateImportFileFormatProvider.Object);

			var mockAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			mockAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

			#endregion Mocks

			using (ObjectFactory.Substitute(mockAccountingCountryFactory.Object))
			using (ObjectFactory.Substitute(mockFramework.Object))
			{
				var importBizo = new OrganisationTaxRateFileImport();
				importBizo.TaxConfiguration = orgTaxConfiguration.PK;
				var importer = ObjectFactory.Get<IOrganisationTaxRateFileImportFileDataImporter>();
				var notifications = new NotificationBuffer();
				int countLines = 1;

				importer.ImportData(PathToTestFile, new NotificationBuffer(), importBizo);
				AssertEquals(countLines, importBizo.ImportLines.Count);

				regNumbersToImport = new Dictionary<ZString, IReadOnlyList<(ZString OrgCode, ZString OrgName, ZGuid AccOrgTaxConfigurationPK)>>();
				mockOrganisation.Setup(x => x.RegNumbersToImport(It.IsAny<BusinessObjectFactory>(), It.IsAny<ZGuid>())).Returns(regNumbersToImport);

				importer.ImportData(PathToTestFile, notifications, importBizo);
				importBizo.TaxConfiguration = orgTaxConfiguration.PK;
				AssertEquals(0, importBizo.ImportLines.Count);

				Assert(!notifications.HasErrors);
				Assert(!notifications.HasWarnings);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetTaxRateAsFraction()
		{
			var orgTaxConfiguration = SetupTaxFrameworkAndGetTaxConfiguration();
			var regNumbersToImport = new Dictionary<ZString, IReadOnlyList<(ZString OrgCode, ZString OrgName, ZGuid AccOrgTaxConfigurationPK)>>
			{
				{ "20000948670", new List<(ZString, ZString, ZGuid)>() { ("AROR0", "ORGANISATION 1", orgTaxConfiguration.PK) } },
				{ "20000948671", new List<(ZString, ZString, ZGuid)>() { ("AROR1", "ORGANISATION 2", orgTaxConfiguration.PK) } },
				{ "20000948672", new List<(ZString, ZString, ZGuid)>() { ("AROR2", "ORGANISATION 3", orgTaxConfiguration.PK) } },
				{ "20000948673", new List<(ZString, ZString, ZGuid)>() { ("AROR3", "ORGANISATION 4", orgTaxConfiguration.PK) } },
				{ "20000948674", new List<(ZString, ZString, ZGuid)>() { ("AROR4", "ORGANISATION 5", orgTaxConfiguration.PK) } },
			};
			AssertEquals("Precondition: RegNumberToImport dictionary must be have five key element.", 5, regNumbersToImport.Count);

			SetUpMock_For_OrganisationTaxRateImportDataProvider(regNumbersToImport);

			var importBizo = new OrganisationTaxRateFileImport();
			importBizo.TaxConfiguration = orgTaxConfiguration.PK;

			var importer = ObjectFactory.Get<IOrganisationTaxRateFileImportFileDataImporter>();
			var notifications = new NotificationBuffer();
			int expectedCountingLines = 5;

			importer.ImportData(PathToTaxRateAsFractionFile, notifications, importBizo);
			AssertEquals(expectedCountingLines, importBizo.ImportLines.Count);
			Assert(!notifications.HasErrors);
			Assert(!notifications.HasWarnings);

			var testCases = new[]
			{
				 new { ExpectedNumerator = 0,    ExpectedDenominator =  1,   CurrentIndex = 0 },
				 new { ExpectedNumerator = 0,    ExpectedDenominator =  1,   CurrentIndex = 1 },
				 new { ExpectedNumerator = 3000, ExpectedDenominator =  1,   CurrentIndex = 2 },
				 new { ExpectedNumerator = 4,    ExpectedDenominator =  1,   CurrentIndex = 3 },
				 new { ExpectedNumerator = 1,    ExpectedDenominator =  100, CurrentIndex = 4 },
			};

			foreach (var test in testCases)
			{
				AssertEquals(test.ExpectedNumerator, importBizo.ImportLines[test.CurrentIndex].RateNumerator);
				AssertEquals(test.ExpectedDenominator, importBizo.ImportLines[test.CurrentIndex].RateDenominator);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRemoveNonNumericCharactersFromOrganisation()
		{
			var orgTaxConfiguration = SetupTaxFrameworkAndGetTaxConfiguration();
			var regNumbersToImport = new Dictionary<ZString, IReadOnlyList<(ZString OrgCode, ZString OrgName, ZGuid AccOrgTaxConfigurationPK)>>
			{
				{ "20000948670", new List<(ZString, ZString, ZGuid)>()
					{
						("AROR0", "ORGANISATION 1", ZGuid.NewZGuid()),
						("ARORX", "ORGANISATION X", ZGuid.NewZGuid()),
					}
				},
				{ "2000094867-0", new List<(ZString, ZString, ZGuid)>()
					{
						("AROR1", "ORGANISATION 2", ZGuid.NewZGuid()),
						("ARORY", "ORGANISATION Y", ZGuid.NewZGuid()),
					}
				},
				{ "2.000.094.867-0", new List<(ZString, ZString, ZGuid)>() { ("AROR2", "ORGANISATION 3", ZGuid.NewZGuid()) } },
				{ "20-00094867-1", new List<(ZString, ZString, ZGuid)>() { ("AROR3", "ORGANISATION 4", ZGuid.NewZGuid()) } },
			};

			SetUpMock_For_OrganisationTaxRateImportDataProvider(regNumbersToImport);

			var importBizo = new OrganisationTaxRateFileImport();
			importBizo.TaxConfiguration = orgTaxConfiguration.PK;
			var notifications = new NotificationBuffer();
			var importer = ObjectFactory.Get<IOrganisationTaxRateFileImportFileDataImporter>();
			int expectedCountingLines = 6;

			importer.ImportData(PathToTestFileWithNonNumericCharacters, new NotificationBuffer(), importBizo);

			Assert(!notifications.HasErrors);
			Assert(!notifications.HasWarnings);
			AssertEquals(expectedCountingLines, importBizo.ImportLines.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportLineWithCorrectFactory()
		{
			var orgTaxConfiguration = SetupTaxFrameworkAndGetTaxConfiguration();

			var regNumbersToImport = GetRegNumbersToImport("20000948676", orgTaxConfiguration.PK);
			AssertEquals("Precondition: RegNumberToImport dictionary must be have only one key element.", 1, regNumbersToImport.Count);

			SetUpMock_For_OrganisationTaxRateImportDataProvider(regNumbersToImport);

			var importBizo = new OrganisationTaxRateFileImport();
			importBizo.TaxConfiguration = orgTaxConfiguration.PK;
			var importer = ObjectFactory.Get<IOrganisationTaxRateFileImportFileDataImporter>();
			var notifications = new NotificationBuffer();
			int expectedCountingLines = 1;

			importer.ImportData(PathToTestFile, notifications, importBizo);
			Assert(!notifications.HasErrors);
			Assert(!notifications.HasWarnings);
			AssertEquals(expectedCountingLines, importBizo.ImportLines.Count);

			foreach (var biZoItem in importBizo.ImportLines)
			{
				AssertEquals("Every Line has the same Factory.", importBizo.Factory, biZoItem.Factory);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportLineWithTaxRateZero()
		{
			var orgTaxConfiguration = SetupTaxFrameworkAndGetTaxConfiguration();
			var regNumbersToImport = GetRegNumbersToImport("20000898636", orgTaxConfiguration.PK);
			AssertEquals("Precondition", 1, regNumbersToImport.Count);

			SetUpMock_For_OrganisationTaxRateImportDataProvider(regNumbersToImport);

			var importBizo = new OrganisationTaxRateFileImport();
			importBizo.TaxConfiguration = orgTaxConfiguration.PK;
			var importer = ObjectFactory.Get<IOrganisationTaxRateFileImportFileDataImporter>();
			var notifications = new NotificationBuffer();
			int countLines = 1;

			importer.ImportData(PathToTestFile, notifications, importBizo);
			Assert(!notifications.HasErrors);
			Assert(!notifications.HasWarnings);
			AssertEquals(countLines, importBizo.ImportLines.Count);

			AssertEquals(0, importBizo.ImportLines[0].RateNumerator);
			AssertEquals(1, importBizo.ImportLines[0].RateDenominator);
		}

		AccTaxConfiguration SetupTaxFrameworkAndGetTaxConfiguration(ZString? expectedTaxAuthorityCode = null)
		{
			var taxTestHelper = new AccountingTestObjectCreator(Factory);
			var taxAuthority = taxTestHelper.CreateTaxAuthority(expectedTaxAuthorityCode ?? "XXX");
			var taxSystemsConfiguration = taxTestHelper.CreateTaxSystem("TS", taxRateSource: TaxRateSources.OrganisationOnly.Code, country: "XX");
			var taxConfiguration = taxTestHelper.CreateTaxConfiguration(GlbCompany.CurrentCompany, taxAuthority, taxSystemsConfiguration, "AR", true);
			Factory.Save();

			return taxConfiguration;
		}

		void SetUpMock_For_OrganisationTaxRateImportDataProvider(Dictionary<ZString, IReadOnlyList<(ZString OrgCode, ZString OrgName, ZGuid AccOrgTaxConfigurationPK)>> regNumbersToImport, bool setupFileFormat = true)
		{
			var mockOrganisation = new Mock<IOrganisationTaxRateImportDataProvider>();
			var mockFramework = new Mock<ITaxFrameworkDependencyFactory>();

			mockOrganisation.Setup(x => x.RegNumbersToImport(It.IsAny<BusinessObjectFactory>(), It.IsAny<ZGuid>())).Returns(regNumbersToImport);
			mockFramework.Setup(x => x.GetOrganisationTaxRateImportDataProvider()).Returns(mockOrganisation.Object);

			var mockIOrgTaxRateImportFileFormat = new Mock<IOrgTaxRateImportFileFormat>();
			var mockIOrgTaxRateImportFileFormatProvider = new Mock<IOrgTaxRateImportFileFormatProvider>();

			if (setupFileFormat)
			{
				mockIOrgTaxRateImportFileFormat.Setup(x => x.StartDate).Returns(1);
				mockIOrgTaxRateImportFileFormat.Setup(x => x.EndDate).Returns(2);
				mockIOrgTaxRateImportFileFormat.Setup(x => x.RegistrationCode).Returns(3);
				mockIOrgTaxRateImportFileFormat.Setup(x => x.PerceptionRate).Returns(7);
				mockIOrgTaxRateImportFileFormat.Setup(x => x.Delimiter).Returns(';');
				mockIOrgTaxRateImportFileFormat.Setup(x => x.NumberOfColumnsInFormat).Returns(12);

				mockIOrgTaxRateImportFileFormatProvider.Setup(x => x.GetFileFormat(It.IsAny<ZString>())).Returns(mockIOrgTaxRateImportFileFormat.Object);
			}

			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			mockIAccountingCountryFactory.As<IInstanceProvider<IOrgTaxRateImportFileFormatProvider>>().Setup(x => x.Get()).Returns(mockIOrgTaxRateImportFileFormatProvider.Object);

			var mockAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			mockAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

			ObjectFactory.Substitute(mockAccountingCountryFactory.Object);
			ObjectFactory.Substitute(mockFramework.Object);
		}

		Dictionary<ZString, IReadOnlyList<(ZString OrgCode, ZString OrgName, ZGuid AccOrgTaxConfigurationPK)>> GetRegNumbersToImport(ZString cuit, ZGuid? guid = null)
			=> new Dictionary<ZString, IReadOnlyList<(ZString OrgCode, ZString OrgName, ZGuid AccOrgTaxConfigurationPK)>>
			{
				{
					cuit, new List<(ZString OrgCode, ZString OrgName, ZGuid AccOrgTaxConfigPK)>
					{
						("AROR1", "ORGANISATION CW1 ONE", guid ?? ZGuid.NewZGuid()),
					}
				}
			};

		Dictionary<ZString, IReadOnlyList<(ZString OrgCode, ZString OrgName, ZGuid AccOrgTAxConfigurationPK)>> GetRegNumbersToImportWithManyOrgs(ZString cuit, ZGuid? guid = null)
			=> new Dictionary<ZString, IReadOnlyList<(ZString OrgCode, ZString OrgName, ZGuid AccOrgTAxConfigurationPK)>>
			{
				{
					cuit, new List<(ZString OrgCode, ZString OrgName, ZGuid AccOrgTaxConfigPK)>
						{
							("AROR1", "ORGANISATION CW1 ONE", guid ?? ZGuid.NewZGuid()),
							("AROR2", "ORGANISATION CW1 TWO", guid ?? ZGuid.NewZGuid()),
							("AROR3", "ORGANISATION CW1 THREE", guid ?? ZGuid.NewZGuid())
						}
				}
			};

		OrganisationTaxRateFileImport AssertFileImport(ZString cuit, string filename, int expectedCountingLines = 0)
		{
			var orgTaxConfiguration = SetupTaxFrameworkAndGetTaxConfiguration();
			var regNumbersToImport = GetRegNumbersToImport(cuit, orgTaxConfiguration.PK);
			AssertEquals("Precondition: RegNumberToImport dictionary must be have only one key element.", 1, regNumbersToImport.Count);

			SetUpMock_For_OrganisationTaxRateImportDataProvider(regNumbersToImport);

			var importBizo = new OrganisationTaxRateFileImport();
			importBizo.TaxConfiguration = orgTaxConfiguration.PK;

			var notifications = new NotificationBuffer();
			var importer = ObjectFactory.Get<IOrganisationTaxRateFileImportFileDataImporter>();

			importer.ImportData(filename, new NotificationBuffer(), importBizo);
			Assert(!notifications.HasErrors);
			Assert(!notifications.HasWarnings);
			AssertEquals(expectedCountingLines, importBizo.ImportLines.Count);
			return importBizo;
		}

		protected override FlatFileDataImporter GetDataImporter() => new OrganisationTaxRateFileImportFileDataImporter();

		protected override string PathToTestFile => BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\OrganisationTaxRateFileImport\TestFiles\PadronesCABA_022022_Small.txt";

		string PathToWrongStructureTestFile => BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\OrganisationTaxRateFileImport\TestFiles\PadronesCABA_022022_WrongFileStructure.txt";

		string PathToWrongDataTestFile => BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\OrganisationTaxRateFileImport\TestFiles\PadronesCABA_022022_WrongData.txt";

		string PathToWrongDataValidationSuspendedTestFile => BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\OrganisationTaxRateFileImport\TestFiles\PadronesCABA_022022_ValidationIsSuspended.txt";

		string PathToTaxRateAsFractionFile => BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\OrganisationTaxRateFileImport\TestFiles\PadronesCABA_022022_TaxRateAsFraction.txt";

		string PathToStartDateInvalidFormatFile => BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\OrganisationTaxRateFileImport\TestFiles\PadronesCABA_StartDateInvalidFormat.txt";

		string PathToEndDateInvalidFormatFile => BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\OrganisationTaxRateFileImport\TestFiles\PadronesCABA_EndDateInvalidFormat.txt";

		string PathToRateWithInvalidCultureFile => BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\OrganisationTaxRateFileImport\TestFiles\PadronesCABA_RateWithInvalidCulture.txt";

		string PathToFileDoesNotExist => BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\OrganisationTaxRateFileImport\TestFiles\FileDoesNotExist.txt";

		string PathToTestFileWithNonNumericCharacters => BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\OrganisationTaxRateFileImport\TestFiles\PadronesCABA_OrganizationWithNonNumericCharacters.txt";

		string PathToFileWithPipeDelimiter => BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\OrganisationTaxRateFileImport\TestFiles\PadronesCABA_PipeDelimiter.txt";

		string PathToFileWithDistinctNumberOfColumnsInFormat => BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\OrganisationTaxRateFileImport\TestFiles\PadronesCABA_DistinctNumberOfColumnsInFormat .txt";
	}
}
