using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.DataTransfer.Universal;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.UniversalDataBuss.ServiceTasks;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.DataTransfer.Testing.Universal;

public class PayableDraftInvoiceImporterTest : TestCaseWithFactory
{
	public void TestSettingDraftInvoicePropertiesCorrectlyFromXut()
	{
		TransactionType[] transactionTypesToTest = [TransactionType.INV, TransactionType.CRD];

		var inputUniversalTransactions = new TransactionInfo[transactionTypesToTest.Length];
		var outputDraftInvoiceHeaders = new AccDraftInvoiceHeader[transactionTypesToTest.Length];

		var importResult = new bool[transactionTypesToTest.Length];
		var numbersOfImportedDraftInvoiceHeaders = new int[transactionTypesToTest.Length];
		var processedLoggers = new XmlSessionTracker[transactionTypesToTest.Length];

		for (int i = 0; i < transactionTypesToTest.Length; i++)
		{
			var testTransactionType = transactionTypesToTest[i];
			var transactionNumber = "ART123" + testTransactionType;
			var multiplier = GetMultiplier(testTransactionType);
			var universalTransaction = GetUniversalTransaction(transactionNumber, transactionType: testTransactionType, multiplier: multiplier);

			inputUniversalTransactions[i] = universalTransaction;

			var importer = CreateImporterForTest();
			var message = new BusinessObjectFactory().New<EDIMessage>();
			message.EM_GB = TestObjectCreator.NonCurrentBranch.PK;
			message.EM_GE = TestObjectCreator.NonCurrentDepartment.PK;

			var universalFactory = new UniversalObjectFactory();
			var serviceLogger = new ServiceTaskLogForTesting();
			var logger = new XmlSessionTracker(serviceLogger);
			var result = importer.ImportPayableDraftInvoice(message, universalTransaction, logger, universalFactory);
			importResult[i] = result;
			var createdInvoices = universalFactory.BOFactory.Load<AccDraftInvoiceHeader>(new ZQuery(AccDraftInvoiceHeaderSchema.AIH_TransactionNumber, transactionNumber));
			numbersOfImportedDraftInvoiceHeaders[i] = createdInvoices.Length;

			var newFactory = new BusinessObjectFactory();
			var savedInvoice = newFactory.Load<AccDraftInvoiceHeader>(createdInvoices[0].PK);

			outputDraftInvoiceHeaders[i] = savedInvoice;
			processedLoggers[i] = logger;
		}

		CombineAssertions(() =>
		{
			for (int i = 0; i < transactionTypesToTest.Length; i++)
			{
				var testTransactionType = transactionTypesToTest[i];

				Assert($"For test [{testTransactionType}] type: Import result", importResult[i]);
				AssertEquals($"For test [{testTransactionType}] type: Only one draft invoice header created.", 1, numbersOfImportedDraftInvoiceHeaders[i]);

				var logger = processedLoggers[i];

				Assert($"For test [{testTransactionType}] type: logger should not have errors", !logger.HasErrors);
				Assert($"For test [{testTransactionType}] type: logger should not have warnings", !logger.HasWarnings);

				var multiplier = GetMultiplier(testTransactionType);
				var universalTransaction = inputUniversalTransactions[i];
				var savedInvoice = outputDraftInvoiceHeaders[i];

				AssertEquals($"For test [{testTransactionType}] type: Company should be set from header branch company.", TestObjectCreator.NonCurrentCompanyBranch.Company.PK, savedInvoice.AIH_GC_Company);
				AssertEquals($"For test [{testTransactionType}] type: Branch should be set from header.", TestObjectCreator.NonCurrentCompanyBranch.PK, savedInvoice.AIH_GB_Branch);
				AssertEquals($"For test [{testTransactionType}] type: Department should be from Header Department.", TestObjectCreator.MiscDepartment.PK, savedInvoice.AIH_GE_Department);

				AssertEquals($"For test [{testTransactionType}] type: Transaction type should be from the universal transaction's transaction type.", universalTransaction.TransactionType.ToString(), savedInvoice.AIH_TransactionType);
				AssertEquals($"For test [{testTransactionType}] type: Creditor should be from Header organization address.", TestObjectCreator.Creditor1.PK, savedInvoice.AIH_OH_Creditor);
				AssertEquals($"For test [{testTransactionType}] type: Transaction number should be from the universal transaction's number.", universalTransaction.Number, savedInvoice.AIH_TransactionNumber);
				AssertEquals($"For test [{testTransactionType}] type: Transaction description should be from the universal transaction's description.", universalTransaction.Description, savedInvoice.AIH_Description);

				AssertEquals($"For test [{testTransactionType}] type: Currency should be from Header Currency.", universalTransaction.OSCurrency.Code, savedInvoice.AIH_RX_NKTransactionCurrency);
				AssertEquals($"For test [{testTransactionType}] type: osExTaxAmount should be from the universal transaction's osExTaxAmount.", universalTransaction.OSExGSTVATAmount * multiplier, savedInvoice.AIH_ExpectedOSExTaxAmount);
				AssertEquals($"For test [{testTransactionType}] type: osTaxAmount number should be from the universal transaction's osTaxAmount.", universalTransaction.OSGSTVATAmount * multiplier, savedInvoice.AIH_ExpectedOSTaxAmount);
				AssertEquals($"For test [{testTransactionType}] type: osTotal description should be from the universal transaction's osTotal.", universalTransaction.OSTotal * multiplier, savedInvoice.AIH_ExpectedOSTotalAmount);

				AssertEquals($"For test [{testTransactionType}] type: Transaction date should be from the universal transaction's transaction date.", universalTransaction.TransactionDate, savedInvoice.AIH_TransactionDate);
				AssertEquals($"For test [{testTransactionType}] type: Post date should be from the universal transaction's post date.", universalTransaction.PostDate, savedInvoice.AIH_PostDate);
				AssertEquals($"For test [{testTransactionType}] type: Due date should be from the universal transaction's Due date.", universalTransaction.DueDate, savedInvoice.AIH_DueDate);
			}
		});
	}

	public void TestExceptionsForUnsupportedTransactionTypesInPayableDraftInvoiceImporter()
	{
		TransactionType?[] transactionTypesToTest = [TransactionType.ADJ, null];
		string[] expectedExceptionMessages =
		[
			$"Provided transaction type is [{transactionTypesToTest[0]}].Only transaction types [{TransactionType.CRD}] and [{TransactionType.INV}] can be imported to Payables Invoice Processing Portal",
			"Imported Universal Transaction has no TransactionType set",
		];

		CombineAssertions(() =>
		{
			for (int i = 0; i < transactionTypesToTest.Length; i++)
			{
				var testTransactionType = transactionTypesToTest[i];
				var transactionNumber = "ART123" + testTransactionType;
				var universalTransaction = GetUniversalTransaction(transactionNumber, transactionType: testTransactionType, multiplier: 1);

				var importer = CreateImporterForTest();
				var message = new BusinessObjectFactory().New<EDIMessage>();
				message.EM_GB = TestObjectCreator.NonCurrentBranch.PK;
				message.EM_GE = TestObjectCreator.NonCurrentDepartment.PK;

				var universalFactory = new UniversalObjectFactory();
				var serviceLogger = new ServiceTaskLogForTesting();
				var logger = new XmlSessionTracker(serviceLogger);
				AssertExceptionThrown<MessageProcessingBusinessFailureException>($"For test [{testTransactionType}] type: MessageProcessingBusinessFailureException is expected", expectedExceptionMessages[i], () =>
				{
					var result =
						importer.ImportPayableDraftInvoice(message, universalTransaction, logger, universalFactory);
				});
			}
		});
	}

	public void TestErrorLogsForArInvoiceFromPayableDraftInvoiceImporter()
	{
		var universalTransaction = GetUniversalTransaction("ART123", LedgerTypes.AccountsReceivable);

		var importer = CreateImporterForTest();
		var message = new BusinessObjectFactory().New<EDIMessage>();
		message.EM_GB = TestObjectCreator.NonCurrentBranch.PK;
		message.EM_GE = TestObjectCreator.NonCurrentDepartment.PK;

		var universalFactory = new UniversalObjectFactory();
		var serviceLogger = new ServiceTaskLogForTesting();
		var logger = new XmlSessionTracker(serviceLogger);

		var result = importer.ImportPayableDraftInvoice(message, universalTransaction, logger, universalFactory);
		Assert($"For test ledger [{LedgerTypes.AccountsReceivable}] type: import result should be false", !result);

		Assert($"For test ledger [{LedgerTypes.AccountsReceivable}] type: logger should have errors", logger.HasErrors);
		AssertContains("Error - Transaction Ledger is not supported", logger.ToString());
	}

	public void TestErrorLogsForNullDataObjectFromPayableDraftInvoiceImporter()
	{
		var importer = CreateImporterForTest();
		var message = new BusinessObjectFactory().New<EDIMessage>();
		message.EM_GB = TestObjectCreator.NonCurrentBranch.PK;
		message.EM_GE = TestObjectCreator.NonCurrentDepartment.PK;

		var universalFactory = new UniversalObjectFactory();
		var serviceLogger = new ServiceTaskLogForTesting();
		var logger = new XmlSessionTracker(serviceLogger);

		var result = importer.ImportPayableDraftInvoice(message, null, logger, universalFactory);
		Assert("For null dataObject: import result should be false", !result);

		Assert("For null dataObject: logger should have errors", logger.HasErrors);
		AssertContains("Error - Data object type is not Universal Transaction type.", logger.ToString());
	}

	public void TestWhenApaRegistriesEnabledARInvImportsHandledByTransactionImporter()
	{
		var factory = new BusinessObjectFactory();
		var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;

		AccountingConfigurationRegistry.Instance.EnablePayablesInvoiceProcessingPortal.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
		AccountingConfigurationRegistry.Instance.EnableImportingUniversalTransactionIntoPayableDraftInvoices.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

		var inboundXml = GetInboundArXml(registrationKey);

		var message = factory.New<EDIMessage>();

		message.EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
		message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
		message.EM_Status = XmlEDIMessage.Status.Queued;
		message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalTransaction;
		message.EM_MessageText = inboundXml;

		factory.Save();

		var logger = new TestServiceLogger();
		var serviceTask = new UMIServiceTask { ServiceLogger = logger };
		using (eAdaptorRegistry.Instance.UniversalXMLExtendedTransactionProtectionEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
		{
			serviceTask.RunTask();
		}

		var messageLoad = new BusinessObjectFactory().Load<IEDIMessage>(message.PK);
		AssertEquals("Message EM_Status: ", EDIMessageStatusList.Codes.Rejected, messageLoad.EM_Status);

		var logger1 = (TestServiceLogger)serviceTask.ServiceLogger;

		AssertContains("Information|Starting processing Message #", logger1.ToString());
		// Following warning is generated from the TransactionImporter, showing that the AR invoice is handled by TransactionImporter even when APA registries are enabled.
		AssertContains("Warning|Exception processing message : [Import failed because transaction has validation errors:", logger1.ToString());
		AssertContains("Error - Generic Charge: Enter a valid selection.", logger1.ToString());
		AssertContains("Error - Exchange Rate: Exchange Rate cannot be zero.", logger1.ToString());
		AssertContains("Error - Total Amount: The sum of the transaction lines should be greater than zero.", logger1.ToString());
		AssertContains("Error - Exchange Rate: Please enter an Exchange Rate.", logger1.ToString());
		AssertContains("Error - Terms: Please enter a Terms.", logger1.ToString());
		AssertContains("Error - Address Override: Please enter an Account.", logger1.ToString());
		AssertContains("Error - Account: Please enter an Account.", logger1.ToString());
		AssertContains("Error - Invoice Post Date: This date does not fall into a valid accounting period’s date range.", logger1.ToString());
		AssertContains("Please go to Manage > General Ledger > Period Management > Set Up Next Accounting Year, to ensure there is an accounting period for the date you wish to post to.", logger1.ToString());

		AssertContains("Information|Finished processing Message #", logger1.ToString());
	}

	public void TestWhenOnlyOneApaRegistriesEnabledAPInvImportsHandledByTransactionImporter()
	{
		var factory = new BusinessObjectFactory();
		var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;

		bool[] enablePayablesInvoiceProcessingPortalValues = [true, false];

		foreach (var enablePayablesInvoiceProcessingPortalValue in enablePayablesInvoiceProcessingPortalValues)
		{
			AccountingConfigurationRegistry.Instance.EnablePayablesInvoiceProcessingPortal.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, enablePayablesInvoiceProcessingPortalValue);
			AccountingConfigurationRegistry.Instance.EnableImportingUniversalTransactionIntoPayableDraftInvoices.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, !enablePayablesInvoiceProcessingPortalValue);

			var inboundXml = GetInboundApXml(registrationKey, "ART123" + enablePayablesInvoiceProcessingPortalValue);

			var message = factory.New<EDIMessage>();

			message.EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_Status = XmlEDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalTransaction;
			message.EM_MessageText = inboundXml;

			factory.Save();

			var logger = new TestServiceLogger();
			var serviceTask = new UMIServiceTask { ServiceLogger = logger };
			using (eAdaptorRegistry.Instance.UniversalXMLExtendedTransactionProtectionEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				serviceTask.RunTask();
			}

			var messageLoad = new BusinessObjectFactory().Load<IEDIMessage>(message.PK);
			AssertEquals("Message EM_Status: ", EDIMessageStatusList.Codes.ProcessedOK, messageLoad.EM_Status);

			var logger1 = (TestServiceLogger)serviceTask.ServiceLogger;

			AssertContains("Information|Starting processing Message #", logger1.ToString());
			AssertContains("Information|Successfully saved Unallocated Transaction.", logger1.ToString());
			AssertContains("Information|Successfully saved Note - Data Import Log Text.", logger1.ToString());
			AssertContains("Information|Finished processing Message #", logger1.ToString());
		}
	}

	public void TestWhenApaRegistriesEnabledAdjImportsHandledByTransactionImporter()
	{
		var factory = new BusinessObjectFactory();
		var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;

		AccountingConfigurationRegistry.Instance.EnablePayablesInvoiceProcessingPortal.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
		AccountingConfigurationRegistry.Instance.EnableImportingUniversalTransactionIntoPayableDraftInvoices.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

		var inboundXml = GetInboundApXml(registrationKey, transactionType: TransactionType.ADJ);

		var message = factory.New<EDIMessage>();

		message.EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
		message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
		message.EM_Status = XmlEDIMessage.Status.Queued;
		message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalTransaction;
		message.EM_MessageText = inboundXml;

		factory.Save();

		var logger = new TestServiceLogger();
		var serviceTask = new UMIServiceTask { ServiceLogger = logger };
		using (eAdaptorRegistry.Instance.UniversalXMLExtendedTransactionProtectionEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
		{
			serviceTask.RunTask();
		}

		var messageLoad = new BusinessObjectFactory().Load<IEDIMessage>(message.PK);
		AssertEquals("Message EM_Status: ", EDIMessageStatusList.Codes.ProcessedOK, messageLoad.EM_Status);

		var logger1 = (TestServiceLogger)serviceTask.ServiceLogger;

		AssertContains("Information|Starting processing Message #", logger1.ToString());
		AssertContains("Information|Successfully saved Unallocated Transaction.", logger1.ToString());
		AssertContains("Information|Successfully saved Note - Data Import Log Text.", logger1.ToString());
		AssertContains("Information|Finished processing Message #", logger1.ToString());
	}

	public void TestWhenApaRegistryEnabledInvAndCrdImportsSavesToDraftInvoice()
	{
		var factory = new BusinessObjectFactory();
		var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;

		AccountingConfigurationRegistry.Instance.EnablePayablesInvoiceProcessingPortal.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
		AccountingConfigurationRegistry.Instance.EnableImportingUniversalTransactionIntoPayableDraftInvoices.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

		TransactionType[] transactionTypesToTest = [TransactionType.INV, TransactionType.CRD];

		var inputTransactionNumber = new string[transactionTypesToTest.Length];
		var outputDraftInvoiceHeaders = new AccDraftInvoiceHeader[transactionTypesToTest.Length];

		var importResult = new string[transactionTypesToTest.Length];
		var numbersOfImportedDraftInvoiceHeaders = new int[transactionTypesToTest.Length];
		var processedLoggers = new TestServiceLogger[transactionTypesToTest.Length];

		for (int i = 0; i < transactionTypesToTest.Length; i++)
		{
			var testTransactionType = transactionTypesToTest[i];
			var transactionNumber = "ART123" + testTransactionType;
			inputTransactionNumber[i] = transactionNumber;
			var inboundXml = GetInboundApXml(registrationKey, transactionNumber, testTransactionType);

			var message = factory.New<EDIMessage>();

			message.EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_Status = XmlEDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalTransaction;
			message.EM_MessageText = inboundXml;

			factory.Save();

			var logger = new TestServiceLogger();
			var serviceTask = new UMIServiceTask { ServiceLogger = logger };
			using (eAdaptorRegistry.Instance.UniversalXMLExtendedTransactionProtectionEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				serviceTask.RunTask();
			}

			var messageLoad = new BusinessObjectFactory().Load<IEDIMessage>(message.PK);
			importResult[i] = messageLoad.EM_Status;

			var logger1 = (TestServiceLogger)serviceTask.ServiceLogger;
			processedLoggers[i] = logger1;

			var universalFactory = new UniversalObjectFactory();
			var createdInvoices = universalFactory.BOFactory.Load<AccDraftInvoiceHeader>(new ZQuery(AccDraftInvoiceHeaderSchema.AIH_TransactionNumber, transactionNumber));
			numbersOfImportedDraftInvoiceHeaders[i] = createdInvoices.Length;

			var newFactory = new BusinessObjectFactory();
			var savedInvoice = newFactory.Load<AccDraftInvoiceHeader>(createdInvoices[0].PK);

			outputDraftInvoiceHeaders[i] = savedInvoice;
		}

		CombineAssertions(() =>
		{
			for (int i = 0; i < transactionTypesToTest.Length; i++)
			{
				var testTransactionType = transactionTypesToTest[i];

				AssertEquals("Message EM_Status: ", EDIMessageStatusList.Codes.ProcessedOK, importResult[i]);
				AssertEquals($"For test [{testTransactionType}] type: Only one draft invoice header created.", 1, numbersOfImportedDraftInvoiceHeaders[i]);

				var logger = processedLoggers[i];
				var savedInvoice = outputDraftInvoiceHeaders[i];

				AssertContains("Information|Starting processing Message #", logger.ToString());
				AssertContains($"Information|Successfully saved {savedInvoice.HumanReadableName}.", logger.ToString());
				AssertContains("Information|Finished processing Message #", logger.ToString());

				AssertEquals("Company should be set from header branch company.", TestObjectCreator.NonCurrentCompanyBranch.Company.PK, savedInvoice.AIH_GC_Company);
				AssertEquals("Branch should be set from header.", TestObjectCreator.NonCurrentCompanyBranch.PK, savedInvoice.AIH_GB_Branch);
				AssertEquals("Department should be from Header Department.", TestObjectCreator.MiscDepartment.PK, savedInvoice.AIH_GE_Department);

				AssertEquals("Transaction type should be from the universal transaction's transaction type.", testTransactionType.ToString(), savedInvoice.AIH_TransactionType);
				AssertEquals("Creditor should be from Header organization address.", TestObjectCreator.Creditor1.PK, savedInvoice.AIH_OH_Creditor);
				AssertEquals("Transaction number should be from the universal transaction's number.", inputTransactionNumber[i], savedInvoice.AIH_TransactionNumber);
				AssertEquals("Transaction description should be from the universal transaction's description.", "AP INVOICE to be converted to Draft invoice", savedInvoice.AIH_Description);
			}
		});
	}

	IPayableDraftInvoiceImporter CreateImporterForTest() => new PayableDraftInvoiceImporter();

	TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
	TestObjectCreator testObjectCreator;

	int GetMultiplier(TransactionType transactionType)
	{
		return transactionType == TransactionType.CRD ? 1 : -1;
	}

	TransactionInfo GetUniversalTransaction(string transactionNumber, string ledger = LedgerTypes.AccountsPayable, TransactionType? transactionType = TransactionType.INV, int multiplier = -1)
	{
		var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
		{
			Ledger = ledger,
			Number = transactionNumber,
			OSCurrency = new Currency { Code = "AUD" },
			OSExGSTVATAmount = 100 * multiplier,
			OSGSTVATAmount = 10 * multiplier,
			OSTotal = 110 * multiplier,
			OrganizationAddress =
				new OrganizationAddress
				{
					AddressType = nameof(DocAddressType.None),
					OrganizationCode = TestObjectCreator.Creditor1.OH_Code
				},
			Branch = new Branch { Code = TestObjectCreator.NonCurrentCompanyBranch.GB_Code },
			BranchAddress =
				new OrganizationAddress
				{
					AddressType = nameof(DocAddressType.None),
					OrganizationCode = TestObjectCreator.NonCurrentCompany.OrgProxy.OH_Code
				},
			Department = new Department { Code = TestObjectCreator.MiscDepartment.GE_Code },
			TransactionType = transactionType,
			Description = "Example test transaction 1",
			TransactionDate = ZDateTime.Today.AddDays(-3),
			DueDate = ZDateTime.Today.AddDays(2),
			PostDate = ZDateTime.Today.AddDays(-1),
		};

		return universalTransaction;
	}

	string GetInboundApXml(IProductRegistrationKey registrationKey, string transactionNumber = "ART123", TransactionType transactionType = TransactionType.INV)
	{
		var multiplier = GetMultiplier(transactionType);
		var inboundApXml = FormattableString.Invariant($@"<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>AccountingInvoice</Type>
          <Key>AP INV ART123</Key>
        </DataSource>
      </DataSourceCollection>

      <Company>
				<Code>{GlbCompany.CurrentCompany.GC_Code}</Code>
				<Name>{GlbCompany.CurrentCompany.GC_Name}</Name>
			</Company>
			<EnterpriseID>{registrationKey.EnterpriseCode}</EnterpriseID>
			<ServerID>{registrationKey.ServerCode}</ServerID>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ORP</Code>
          <Description>Organization Proxy</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <Branch>
      <Code>{TestObjectCreator.NonCurrentCompanyBranch.GB_Code}</Code>
      <!--Name>BN - AUBNE</Name-->
    </Branch>
    <BranchAddress>
      <AddressType>OFC</AddressType>
      <Country>
        <Code>AU</Code>
        <Name>Australia</Name>
      </Country>
    </BranchAddress>
    <Category>STD</Category>
    <CheckDrawer></CheckDrawer>
    <CheckNumberOrPaymentRef></CheckNumberOrPaymentRef>
    <CreateTime>2019-06-05T08:15:00</CreateTime>
    <CreateUser>E</CreateUser>
    <Department>
      <Code>{TestObjectCreator.MiscDepartment.GE_Code}</Code>
    </Department>
    <Description>AP INVOICE to be converted to Draft invoice</Description>
    <DrawerBank></DrawerBank>
    <DrawerBranch></DrawerBranch>
    <DueDate>2019-06-05T18:15:00</DueDate>
    <ExchangeRate>1.000000</ExchangeRate>
    <InvoiceTerm>COD</InvoiceTerm>
    <InvoiceTermDays>0</InvoiceTermDays>
    <IsCancelled>false</IsCancelled>
    <IsCreatedByMatchingProcess>false</IsCreatedByMatchingProcess>
    <IsPrinted>false</IsPrinted>
    <Job>
      <Type>Job</Type>
    </Job>
    <JobInvoiceNumber>00001000</JobInvoiceNumber>
    <Ledger>AP</Ledger>
    <LocalCurrency>
      <Code>AUD</Code>
      <Description>Australian Dollar</Description>
    </LocalCurrency>
    <LocalExVATAmount>{971.0000 * multiplier}</LocalExVATAmount>
    <LocalTotal>{971.0000 * multiplier}</LocalTotal>
    <LocalVATAmount>{0.0000 * multiplier}</LocalVATAmount>
    <Number>{transactionNumber}</Number>
    <NumberOfSupportingDocuments>1</NumberOfSupportingDocuments>
    
    <OrganizationAddress>
      <AddressType>None</AddressType>
      <Address1></Address1>
      <Address2></Address2>
      <City></City>
      <CompanyName></CompanyName>
      <Country>
        <Code></Code>
      </Country>
      <OrganizationCode>{TestObjectCreator.Creditor1.OH_Code}</OrganizationCode>
      <Phone></Phone>
      <Postcode></Postcode>
    </OrganizationAddress>
    <OSCurrency>
      <Code>AUD</Code>
      <Description>Australian Dollar</Description>
    </OSCurrency>
    <OSExGSTVATAmount>{971.0000 * multiplier}</OSExGSTVATAmount>
    <OSGSTVATAmount>{0.00 * multiplier}</OSGSTVATAmount>
    <OSTotal>{971.0000 * multiplier}</OSTotal>
    <OutstandingAmount>{971.0000 * multiplier}</OutstandingAmount>
    <PlaceOfIssue>Brisbane</PlaceOfIssue>
    <PostDate>2019-06-05T18:15:00</PostDate>
    <TransactionDate>2019-06-05T18:15:00</TransactionDate>
    <TransactionType>{transactionType}</TransactionType>

    <PostingJournalCollection>
      <PostingJournal>
        <Branch>
          <Code>BNE</Code>
          <Name>BN - AUBNE</Name>
        </Branch>
        <Department>
          <Code>BRN</Code>
          <Name>Branch</Name>
        </Department>
        <Description>WAREHOUSE HANDLING COSTS ACTUAL</Description>
        <GLAccount>
          <AccountCode>1210.20.10</AccountCode>
          <Description>WAREHOUSE HANDLING COSTS ACTUAL</Description>
        </GLAccount>
        <GLPostDate>2019-06-05T18:15:00</GLPostDate>
        <IsFinalCharge>false</IsFinalCharge>
        <Job>
          <Type>Job</Type>
        </Job>
        <LocalAmount>-971.0000</LocalAmount>
        <LocalCurrency>
          <Code>AUD</Code>
          <Description>Australian Dollar</Description>
        </LocalCurrency>
        <LocalGSTVATAmount>0.0000</LocalGSTVATAmount>
        <LocalTotalAmount>-971.0000</LocalTotalAmount>
        <Organization>
          <Type>Organization</Type>
          <Key>AALSHI</Key>
        </Organization>
        <OSAmount>-971.00</OSAmount>
        <OSCurrency>
          <Code>AUD</Code>
          <Description>Australian Dollar</Description>
        </OSCurrency>
        <OSGSTVATAmount>0.00</OSGSTVATAmount>
        <OSTotalAmount>-971.0000</OSTotalAmount>
        <RevenueRecognitionType>IMM</RevenueRecognitionType>
        <Sequence>1</Sequence>
        <TransactionCategory>STD</TransactionCategory>
        <TransactionType>INV</TransactionType>

        <PostingJournalDetailCollection>
          <PostingJournalDetail>
            <CreditGLAccount>
              <AccountCode>8210.00.00</AccountCode>
              <Description>TRADE CREDITORS CONTROL</Description>
            </CreditGLAccount>
            <DebitGLAccount>
              <AccountCode>4900.00.00</AccountCode>
              <Description>RETAINED EARNINGS FROM PREVIOUS YR</Description>
            </DebitGLAccount>
            <PostingAmount>971.0000</PostingAmount>
            <PostingCurrency>
              <Code>AUD</Code>
              <Description>Australian Dollar</Description>
            </PostingCurrency>
            <PostingDate>2019-06-05T18:15:00</PostingDate>
            <PostingPeriod>202001</PostingPeriod>
          </PostingJournalDetail>
          <PostingJournalDetail>
            <CreditGLAccount>
              <AccountCode>4900.00.00</AccountCode>
              <Description>RETAINED EARNINGS FROM PREVIOUS YR</Description>
            </CreditGLAccount>
            <DebitGLAccount>
              <AccountCode>1210.20.10</AccountCode>
              <Description>WAREHOUSE HANDLING COSTS ACTUAL</Description>
            </DebitGLAccount>
            <PostingAmount>971.0000</PostingAmount>
            <PostingCurrency>
              <Code>AUD</Code>
              <Description>Australian Dollar</Description>
            </PostingCurrency>
            <PostingDate>2019-06-05T18:15:00</PostingDate>
            <PostingPeriod>202001</PostingPeriod>
          </PostingJournalDetail>
        </PostingJournalDetailCollection>
      </PostingJournal>
    </PostingJournalCollection>
  </TransactionInfo>
</UniversalTransaction>");

		return inboundApXml;
	}

	string GetInboundArXml(IProductRegistrationKey registrationKey)
	{
		var inboundArXml = FormattableString.Invariant($@"<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>AccountingInvoice</Type>
          <Key>AP INV ART123</Key>
        </DataSource>
      </DataSourceCollection>

      <Company>
				<Code>{GlbCompany.CurrentCompany.GC_Code}</Code>
				<Name>{GlbCompany.CurrentCompany.GC_Name}</Name>
			</Company>
			<EnterpriseID>{registrationKey.EnterpriseCode}</EnterpriseID>
			<ServerID>{registrationKey.ServerCode}</ServerID>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ORP</Code>
          <Description>Organization Proxy</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

	<Branch>
	  <Code>BER</Code>
	  <Name>EDI - Sydney Training</Name>
	</Branch>
	<Department>
	  <Code>BRN</Code>
	  <Name>Branch</Name>
	</Department>
	<Description>AR INVOICE</Description>
	<DueDate>2015-05-01T19:09:00</DueDate>
	<Ledger>AR</Ledger>
	<LocalExVATAmount>-60.0000</LocalExVATAmount>
	<Number>11112222</Number>
	<NumberOfSupportingDocuments>2</NumberOfSupportingDocuments>
	<OrganizationAddress>
	  <AddressType>None</AddressType>
	  <OrganizationCode>ABCFRESYD</OrganizationCode>
	</OrganizationAddress>
	<OSCurrency>
	  <Code>USD</Code>
	  <Description>United States Dollar</Description>
	</OSCurrency>
	<OSExGSTVATAmount>-120.0000</OSExGSTVATAmount>
	<PostDate>2015-04-30T19:09:00</PostDate>
	<TransactionDate>2015-04-28T19:09:00</TransactionDate>
	<TransactionType>INV</TransactionType>
	<DocumentReceivedDate>2020-02-13T11:11:00</DocumentReceivedDate>

	<PostingJournalCollection>
	  <PostingJournal>
		<Branch>
		  <Code>SYD</Code>
		  <Name>EDI - Sydney Training</Name>
		</Branch>
		<Department>
		  <Code>BRN</Code>
		  <Name>Branch</Name>
		</Department>
		<Description>FREIGHT REVENUE ACTUAL</Description>
		<GLAccount>
		  <AccountCode>1010.10.10</AccountCode>
		  <Description>FREIGHT REVENUE ACTUAL</Description>
		</GLAccount>
		<IsFinalCharge>true</IsFinalCharge>
		<LocalAmount>-60.0000</LocalAmount>
		<OSAmount>-120.00</OSAmount>
		<OSCurrency>
		  <Code>USD</Code>
		  <Description>United States Dollar</Description>
		</OSCurrency>
		<Sequence>2</Sequence>
	  </PostingJournal>
	</PostingJournalCollection>
  </TransactionInfo>
</UniversalTransaction>");

		return inboundArXml;
	}
}
