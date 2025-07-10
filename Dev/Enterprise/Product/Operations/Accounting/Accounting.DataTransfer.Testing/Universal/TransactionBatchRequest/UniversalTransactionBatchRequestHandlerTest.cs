using System;
using System.Globalization;
using System.IO;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Aggregator;
using Enterprise.Accounting.Business.Aggregator.Test;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.DataTransfer.AccBatchRequest;
using Enterprise.Accounting.DataTransfer.AccBatchRequest.Testing;
using Enterprise.Accounting.DataTransfer.AccBatchRequets;
using Enterprise.Accounting.DataTransfer.Universal;
using Enterprise.Accounting.Export.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Testing.Universal
{
	class UniversalTransactionBatchRequestHandlerTest : TestCaseWithFactory
	{
		public void TestCreateRequestAndResponseMessage()
		{
			var handler = new UniversalTransactionBatchRequestHandler(ObjectFactory.Get<IXmlSessionTracker>("IXmlSessionTracker", new SimpleLogger()));
			var request = (EDIMessage)handler.CreateRequestMessage();
			AssertEquals(ApplicationCodeList.Codes.UniversalDataQuery, request.EM_ApplicationCode);
			AssertEquals(ReceiveTransmitList.Codes.Receive, request.EM_ReceiveTransmit);
			AssertEquals(EDIMessageTypeList.Codes.XMS, request.EM_MessageType);
			AssertEquals(EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatchRequest, request.EM_MessageSubType);
			AssertEquals(EDIMessageStatusList.Codes.Recognised, request.EM_Status);

			var response = (EDIMessage)handler.CreateResponseMessage();
			AssertEquals(ApplicationCodeList.Codes.UniversalDataQuery, response.EM_ApplicationCode);
			AssertEquals(ReceiveTransmitList.Codes.Transmit, response.EM_ReceiveTransmit);
			AssertEquals(EDIMessageTypeList.Codes.XMS, response.EM_MessageType);
			AssertEquals(EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch, response.EM_MessageSubType);
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, response.EM_Status);
		}

		public void TestProcessCreate()
		{
			var xmlSessionTracker = ObjectFactory.Get<IXmlSessionTracker>("IXmlSessionTracker", new SimpleLogger());
			var handler = new UniversalTransactionBatchRequestHandlerForTest(xmlSessionTracker);
			var request = CreateRequest(handler, AccBatchRequestTypePairList.Codes.Create);

			var processingResult = handler.Process(request);
			using (processingResult)
			{
				AssertNotNull(processingResult);
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, processingResult.Status);
				AssertContains("Nothing to be batched.", xmlSessionTracker.ToString());

				var creator = new TestObjectCreator(Factory);

				var shipment = creator.CreateShipment("S0001");
				var job = creator.CreateJob(shipment);
				var creditNote = creator.CreateARCreditNoteWithLine("0001", creator.ABIGAS, creator.AUD, 1m,
					"Transaction to batch", job, creator.CC1, 100m, ZDateTime.Today, false);
				creator.CreateJobCharge(creditNote.Lines[0], job, creator.CC1);
				Factory.Save();
			}

			using (processingResult = handler.Process(request))
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, processingResult.Status);
				AssertContains("New batch 1 is created", xmlSessionTracker.ToString());
				processingResult.ResponseMessageText.Position = 0;
				var responseXml = new StreamReader(processingResult.ResponseMessageText).ReadToEnd();

				AssertContains(
					@"<UniversalTransactionBatch xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionBatch>
    <BatchType>
      <Code>CRE</Code>
      <Description>Transaction Batch Created</Description>
    </BatchType>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>BatchNumber</Type>
          <Key>1</Key>
        </DataSource>
      </DataSourceCollection>

      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>EDIDATEDI</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>
    <TransactionCollection>
    </TransactionCollection>
  </TransactionBatch>
</UniversalTransactionBatch>", responseXml);
			}
		}

		public void TestProcessCreate_WithRequestXmlContainingNewlineChar()
		{
			var xmlSessionTracker = ObjectFactory.Get<IXmlSessionTracker>("IXmlSessionTracker", new SimpleLogger());
			var handler = new UniversalTransactionBatchRequestHandler(xmlSessionTracker);
			var request = CreateRequestWithXml(handler, GetRequestXmlWithNewlineChar(AccBatchRequestTypePairList.Codes.Create));

			var processingResult = handler.Process(request);
			AssertNotNull(processingResult);
			AssertEquals(EDIMessageStatusList.Codes.Error, processingResult.Status);
			var expectedErrorMsg = @"Error - Could not get DataContext from Universal Transaction Batch Request";
			AssertMultilineASCIIEquals(expectedErrorMsg, xmlSessionTracker.ToString());
		}

		[TestDate(2016, 3, 30, 3, 53, 0)]
		public void TestProcessExport_DisableSupplyType()
		{
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertProcessExport(false);
		}

		[TestDate(2016, 3, 30, 3, 53, 0)]
		public void TestProcessExport_EnableSupplyType()
		{
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertProcessExport(true);
		}

		void AssertProcessExport(bool enableSupplyType)
		{
			var xml = GetRequestXml(AccBatchRequestTypePairList.Codes.Create);
			var xmlSessionTracker = ObjectFactory.Get<IXmlSessionTracker>("IXmlSessionTracker", new SimpleLogger());
			var handler = new UniversalTransactionBatchRequestHandlerForTest(xmlSessionTracker);
			var request = CreateRequest(handler, AccBatchRequestTypePairList.Codes.Create);

			var creator = new TestObjectCreator(Factory);
			creator.ABIGAS.CompanyData.SetARTaxApplicable(true);
			var shipment = creator.CreateShipment("S0001");
			var job = creator.CreateJob(shipment);
			creator.ABIGAS.CompanyData.SetAPTaxApplicable(true);
			var creditNote = creator.CreateARCreditNoteWithLine("0001", creator.ABIGAS, creator.AUD, 1m, "Transaction to batch", job, creator.CC1, 100m, ZDateTime.Today, false);
			creditNote.Lines[0].AL_SupplyType = "DSB";
			creator.CreateJobCharge(creditNote.Lines[0], job, creator.CC1);
			creator.GLHeader1.AG_AccountNum = "XXXX.00.01";
			Factory.Save();

			var processingResult = handler.Process(request);

			using (processingResult)
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, processingResult.Status);
				AssertContains("New batch 1 is created", xmlSessionTracker.ToString());

				request = CreateRequest(handler, AccBatchRequestTypePairList.Codes.Export, 1);
			}

			using (processingResult = handler.Process(request))
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, processingResult.Status);
				AssertContains("Batch 1 is exported", xmlSessionTracker.ToString());

				processingResult.ResponseMessageText.Position = 0;
				var responseXml = new StreamReader(processingResult.ResponseMessageText).ReadToEnd();

				AssertContains(
					GetExpectedBatchExportXml(enableSupplyType, false, AccBatchRequestTypePairList.Codes.Export,
						AccBatchRequestTypePairList.Descriptions.Export), responseXml);
			}
		}

		[TestDate(2016, 3, 30, 3, 53, 0)]
		public void TestProcessExport_TaxBranch()
		{
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var xml = GetRequestXml(AccBatchRequestTypePairList.Codes.Create);
			var xmlSessionTracker = ObjectFactory.Get<IXmlSessionTracker>("IXmlSessionTracker", new SimpleLogger());
			var handler = new UniversalTransactionBatchRequestHandlerForTest(xmlSessionTracker);
			var request = CreateRequest(handler, AccBatchRequestTypePairList.Codes.Create);

			var creator = new TestObjectCreator(Factory);
			creator.ABIGAS.CompanyData.SetARTaxApplicable(true);
			var taxOrgProxy = creator.CreateOrgHeader("", true, false, "AUSYD");
			var taxBranchAddress = creator.CreateAddress(taxOrgProxy, OrgAddressType.Office, true);
			taxBranchAddress.OA_Address1 = "Test Address Line 1";
			taxBranchAddress.OA_Address2 = "Test Address Line 2";
			taxBranchAddress.OA_Code = "Test Short Code";
			taxBranchAddress.OA_City = "TestCity";
			taxBranchAddress.OA_State = "NSW";
			taxBranchAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			taxBranchAddress.Header.OH_Code = "TAXORGCODE";
			var taxBranch = creator.CreateBranch("B1", "TaxBranch", GlbCompany.CurrentCompany, taxOrgProxy);
			var shipment = creator.CreateShipment("S0001");
			var job = creator.CreateJob(shipment);
			creator.ABIGAS.CompanyData.SetAPTaxApplicable(true);
			var creditNote = creator.CreateARCreditNoteWithLine("0001", creator.ABIGAS, creator.AUD, 1m, "Transaction to batch", job, creator.CC1, 100m, ZDateTime.Today, false);
			creditNote.AH_GB_TaxBranch = taxBranch.PK;
			creditNote.Lines[0].AL_GB_TaxBranch = taxBranch.PK;
			var charge = creator.CreateJobCharge(creditNote.Lines[0], job, creator.CC1);
			creator.GLHeader1.AG_AccountNum = "XXXX.00.01";
			charge.JR_GB_SellTaxBranch = taxBranch.PK;
			Factory.Save();

			var processingResult = handler.Process(request);

			using (processingResult)
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, processingResult.Status);
				AssertContains("New batch 1 is created", xmlSessionTracker.ToString());

				request = CreateRequest(handler, AccBatchRequestTypePairList.Codes.Export, 1);
			}

			using (processingResult = handler.Process(request))
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, processingResult.Status);
				AssertContains("Batch 1 is exported", xmlSessionTracker.ToString());

				processingResult.ResponseMessageText.Position = 0;
				var responseXml = new StreamReader(processingResult.ResponseMessageText).ReadToEnd();

				AssertContains(
					GetExpectedBatchExportXml(false, true, AccBatchRequestTypePairList.Codes.Export,
						AccBatchRequestTypePairList.Descriptions.Export), responseXml);
			}
		}

		[TestDate(2018, 4, 25, 0, 0, 0)]
		public void TestProcessExportWithNullUserContext()
		{
			var xml = GetRequestXml(AccBatchRequestTypePairList.Codes.Create);
			var xmlSessionTracker = ObjectFactory.Get<IXmlSessionTracker>("IXmlSessionTracker", new SimpleLogger());
			var handler = new UniversalTransactionBatchRequestHandlerForTest(xmlSessionTracker);
			var request = CreateRequest(handler, AccBatchRequestTypePairList.Codes.Create);

			var creator = new TestObjectCreator(Factory);
			var shipment = creator.CreateShipment("S0001");
			var job = creator.CreateJob(shipment);
			var creditNote = creator.CreateARCreditNoteWithLine("0001", creator.ABIGAS, creator.AUD, 1m, "Transaction to batch", job, creator.CC1, 100m, ZDateTime.Today, false);
			creator.CreateJobCharge(creditNote.Lines[0], job, creator.CC1);
			creator.GLHeader1.AG_AccountNum = "XXXX.00.01";
			Factory.Save();

			var processingResult = handler.Process(request);
			using (processingResult)
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, processingResult.Status);
				AssertContains("New batch 1 is created", xmlSessionTracker.ToString());

				request = CreateRequest(handler, AccBatchRequestTypePairList.Codes.Export, 1);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(null))
			using (processingResult = handler.Process(request))
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, processingResult.Status);
				AssertContains("Batch 1 is exported", xmlSessionTracker.ToString());
			}
		}

		[TestDate(2016, 3, 30, 3, 53, 0)]
		public void TestProcessExportWhenThereAreMissingControlAccounts()
		{
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);

			var xml = GetRequestXml(AccBatchRequestTypePairList.Codes.Create);
			var xmlSessionTracker = ObjectFactory.Get<IXmlSessionTracker>("IXmlSessionTracker", new SimpleLogger());
			var handler = new UniversalTransactionBatchRequestHandlerForTest(xmlSessionTracker);
			var request = CreateRequest(handler, AccBatchRequestTypePairList.Codes.Create);

			var creator = new TestObjectCreator(Factory);
			var shipment = creator.CreateShipment("S0001");
			var job = creator.CreateJob(shipment);
			var creditNote = creator.CreateARCreditNoteWithLine("0001", creator.ABIGAS, creator.AUD, 1m, "Transaction to batch", job, creator.CC1, 100m, ZDateTime.Today, false);
			creator.CreateJobCharge(creditNote.Lines[0], job, creator.CC1);
			Factory.Save();

			var processingResult = handler.Process(request);

			using (processingResult)
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, processingResult.Status);
				AssertContains("New batch 1 is created", xmlSessionTracker.ToString());

				request = CreateRequest(handler, AccBatchRequestTypePairList.Codes.Export, 1);
			}

			using (processingResult = handler.Process(request))
			{
				AssertEquals(EDIMessageStatusList.Codes.Error, processingResult.Status);
				var expectedErrorMsg =
					@"New batch 1 is created
Error - Please set up the control account(s) in the registry Accounting > General Ledger Defaults > Control Account: 

- AR Control Account.
- AR Suspense Control Account.
- AP Suspense Control Account.
- Job Revenue Journal Control Account.";
				AssertEquals(expectedErrorMsg, xmlSessionTracker.ToString());
			}
		}

		[TestDate(2016, 3, 30, 3, 53, 0)]
		public void TestProcessExportWhenCFXControlAccountIsMissing()
		{
			using (AccountingConfigurationRegistry.Instance.CFXAccount.DataType.SuspendValidation())
			{
				AccountingConfigurationRegistry.Instance.CFXAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			}
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			var xml = GetRequestXml(AccBatchRequestTypePairList.Codes.Create);
			var xmlSessionTracker = ObjectFactory.Get<IXmlSessionTracker>("IXmlSessionTracker", new SimpleLogger());
			var handler = new UniversalTransactionBatchRequestHandlerForTest(xmlSessionTracker);
			var request = CreateRequest(handler, AccBatchRequestTypePairList.Codes.Create);

			var creator = new TestObjectCreator(Factory);
			var shipment = creator.CreateShipment("S0001");
			var job = creator.CreateJob(shipment);

			var exchangeRate = job.ExchangeRates.AddNew();
			exchangeRate.JF_RX_NKRateCurrency = creator.USD.RX_Code;
			exchangeRate.JF_BaseRate = 0.4m;

			var charge = job.Charges.AddNew();
			charge.JR_LocalSellAmt = 125m;
			var cFXHeader = Factory.New<JCJournalHeader>();
			charge.JR_AC = creator.FRT.PK;
			charge.CreateCFXTransactionLine(cFXHeader, ZDateTime.Now);
			charge.CFXLine.AL_LineAmount = -32m;
			charge.CFXLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			charge.CFXLine.AL_RX_NKTransactionCurrency = creator.USD.RX_Code;
			charge.CFXLine.AL_ExchangeRate = 0.4m;
			Factory.Save();

			var processingResult = handler.Process(request);

			using (processingResult)
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, processingResult.Status);
				AssertContains("New batch 1 is created", xmlSessionTracker.ToString());

				request = CreateRequest(handler, AccBatchRequestTypePairList.Codes.Export, 1);
			}

			using (processingResult = handler.Process(request))
			{
				AssertEquals(EDIMessageStatusList.Codes.Error, processingResult.Status);
				var expectedErrorMsg =
					string.Format(
						"Error - Unable to get CFX Account for Company={0} Branch={1} Department={2}",
						GlbCompany.CurrentCompany.PK,
						Env.CurrentBranchPK,
						Env.CurrentDepartmentPK);
				AssertContains(expectedErrorMsg, xmlSessionTracker.ToString());
			}
		}

		[ExpectNoExceptions]
		public void TestNoExceptionThrownWithErrorCompanyCode()
		{
			var xmlSessionTracker = ObjectFactory.Get<IXmlSessionTracker>("IXmlSessionTracker", new SimpleLogger());
			var handler = new UniversalTransactionBatchRequestHandlerForTest(xmlSessionTracker);
			var request = CreateRequestWithErrorCompanyCode(handler, AccBatchRequestTypePairList.Codes.CreateAndExport);

			var result = handler.Process(request);
			AssertEquals("Error - Unable to load company: XXX", xmlSessionTracker.ToString());
		}

		[TestDate(2016, 3, 30, 3, 53, 0)]
		public void TestProcessCreateAndExport_DisableSupplyType()
		{
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertProcessCreateAndExport_SupplyType(false);
		}

		[TestDate(2016, 3, 30, 3, 53, 0)]
		public void TestProcessCreateAndExport_EnableSupplyType()
		{
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertProcessCreateAndExport_SupplyType(true);
		}

		void AssertProcessCreateAndExport_SupplyType(bool enableSupplyType)
		{
			var xmlSessionTracker = ObjectFactory.Get<IXmlSessionTracker>("IXmlSessionTracker", new SimpleLogger());
			var handler = new UniversalTransactionBatchRequestHandlerForTest(xmlSessionTracker);
			var request = CreateRequest(handler, AccBatchRequestTypePairList.Codes.CreateAndExport);

			var creator = new TestObjectCreator(Factory);
			creator.ABIGAS.CompanyData.SetARTaxApplicable(true);
			var shipment = creator.CreateShipment("S0001");
			var job = creator.CreateJob(shipment);
			creator.ABIGAS.CompanyData.SetAPTaxApplicable(true);
			var creditNote = creator.CreateARCreditNoteWithLine("0001", creator.ABIGAS, creator.AUD, 1m, "Transaction to batch", job, creator.CC1, 100m, ZDateTime.Today, false);
			creditNote.Lines[0].AL_SupplyType = "DSB";
			creator.CreateJobCharge(creditNote.Lines[0], job, creator.CC1);
			creator.GLHeader1.AG_AccountNum = "XXXX.00.01";
			Factory.Save();

			using (var processingResult = handler.Process(request))
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, processingResult.Status);
				AssertContains("New batch 1 is created and exported", xmlSessionTracker.ToString());

				processingResult.ResponseMessageText.Position = 0;
				var responseXml = new StreamReader(processingResult.ResponseMessageText).ReadToEnd();

				AssertContains(
					GetExpectedBatchExportXml(enableSupplyType, false, AccBatchRequestTypePairList.Codes.CreateAndExport,
						AccBatchRequestTypePairList.Descriptions.CreateAndExport), responseXml);
			}
		}

		[TestDate(2016, 3, 30, 3, 53, 0)]
		public void TestProcessCreateAndExport_TaxBranch()
		{
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var xmlSessionTracker = ObjectFactory.Get<IXmlSessionTracker>("IXmlSessionTracker", new SimpleLogger());
			var handler = new UniversalTransactionBatchRequestHandlerForTest(xmlSessionTracker);
			var request = CreateRequest(handler, AccBatchRequestTypePairList.Codes.CreateAndExport);

			var creator = new TestObjectCreator(Factory);
			creator.ABIGAS.CompanyData.SetARTaxApplicable(true);
			var taxOrgProxy = creator.CreateOrgHeader("", true, false, "AUSYD");
			var taxBranchAddress = creator.CreateAddress(taxOrgProxy, OrgAddressType.Office, true);
			taxBranchAddress.OA_Address1 = "Test Address Line 1";
			taxBranchAddress.OA_Address2 = "Test Address Line 2";
			taxBranchAddress.OA_Code = "Test Short Code";
			taxBranchAddress.OA_City = "TestCity";
			taxBranchAddress.OA_State = "NSW";
			taxBranchAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			taxBranchAddress.Header.OH_Code = "TAXORGCODE";
			var taxBranch = creator.CreateBranch("B1", "TaxBranch", GlbCompany.CurrentCompany, taxOrgProxy);
			var shipment = creator.CreateShipment("S0001");
			var job = creator.CreateJob(shipment);
			creator.ABIGAS.CompanyData.SetAPTaxApplicable(true);
			var creditNote = creator.CreateARCreditNoteWithLine("0001", creator.ABIGAS, creator.AUD, 1m, "Transaction to batch", job, creator.CC1, 100m, ZDateTime.Today, false);
			creditNote.AH_GB_TaxBranch = taxBranch.PK;
			creditNote.Lines[0].AL_SupplyType = "DSB";
			creditNote.Lines[0].AL_GB_TaxBranch = taxBranch.PK;
			var charge = creator.CreateJobCharge(creditNote.Lines[0], job, creator.CC1);
			creator.GLHeader1.AG_AccountNum = "XXXX.00.01";
			charge.JR_GB_SellTaxBranch = taxBranch.PK;
			Factory.Save();

			using (var processingResult = handler.Process(request))
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, processingResult.Status);
				AssertContains("New batch 1 is created and exported", xmlSessionTracker.ToString());

				processingResult.ResponseMessageText.Position = 0;
				var responseXml = new StreamReader(processingResult.ResponseMessageText).ReadToEnd();

				AssertContains(
					GetExpectedBatchExportXml(false, true, AccBatchRequestTypePairList.Codes.CreateAndExport,
						AccBatchRequestTypePairList.Descriptions.CreateAndExport), responseXml);
			}
		}

		[TestDate(2016, 3, 30, 3, 53, 0)]
		public void TestProcessUnknownAction()
		{
			var xmlSessionTracker = ObjectFactory.Get<IXmlSessionTracker>("IXmlSessionTracker", new SimpleLogger());
			var handler = new UniversalTransactionBatchRequestHandlerForTest(xmlSessionTracker);
			var request = CreateRequest(handler, "UNK"); // Unknown Action Code

			IHttpXmlProcessingResult processingResult = null;

			AssertNoExceptionThrown("Should not throw exception", () => processingResult = handler.Process(request));
			AssertNotEquals("Result should not be null", null, processingResult);
			AssertEquals(EDIMessageStatusList.Codes.Error, processingResult.Status);
			AssertContains("Not supported Action Type", xmlSessionTracker.ToString());
			AssertEquals("ResponseMessageText should be null", null, processingResult.ResponseMessageText);
		}

		[TestDate(2018, 3, 30, 3, 53, 0)]
		public void TestDataTargetCollectionIsNull()
		{
			var xmlSessionTracker = ObjectFactory.Get<IXmlSessionTracker>("IXmlSessionTracker", new SimpleLogger());
			var handler = new UniversalTransactionBatchRequestHandlerForTest(xmlSessionTracker);
			var request = CreateRequestCore(handler, xmlNoDataCollectionType);

			IHttpXmlProcessingResult processingResult = null;

			AssertNoExceptionThrown("Should not throw exception", () => processingResult = handler.Process(request));
			AssertNotEquals("Result should not be null", null, processingResult);
			AssertEquals(EDIMessageStatusList.Codes.Error, processingResult.Status);
			AssertContains("Data Target Collection is empty", xmlSessionTracker.ToString());
			AssertEquals("ResponseMessageText should be null", null, processingResult.ResponseMessageText);
		}

		[TestDate(2016, 3, 30, 3, 53, 0)]
		public void TestProcessRequestWithoutDataContext()
		{
			var xmlSessionTracker = ObjectFactory.Get<IXmlSessionTracker>("IXmlSessionTracker", new SimpleLogger());
			var handler = new UniversalTransactionBatchRequestHandlerForTest(xmlSessionTracker);
			var request = CreateRequestCore(handler, xmlNoDataContext);

			IHttpXmlProcessingResult processingResult = null;

			AssertNoExceptionThrown("Should not throw exception", () => processingResult = handler.Process(request));
			AssertNotEquals("Result should not be null", null, processingResult);
			AssertEquals(EDIMessageStatusList.Codes.Error, processingResult.Status);
			AssertContains("Could not get DataContext from Universal Transaction Batch Request", xmlSessionTracker.ToString());
			AssertEquals("ResponseMessageText should be null", null, processingResult.ResponseMessageText);
		}

		[TestDate(2016, 3, 30, 3, 53, 0)]
		public void TestProcessEmptyRequest()
		{
			var xmlSessionTracker = ObjectFactory.Get<IXmlSessionTracker>("IXmlSessionTracker", new SimpleLogger());
			var handler = new UniversalTransactionBatchRequestHandlerForTest(xmlSessionTracker);
			var request = CreateRequestCore(handler, xmlEmptyRequest);

			IHttpXmlProcessingResult processingResult = null;

			AssertNoExceptionThrown("Should not throw exception", () => processingResult = handler.Process(request));
			AssertNotEquals("Result should not be null", null, processingResult);
			AssertEquals(EDIMessageStatusList.Codes.Error, processingResult.Status);
			AssertContains("Could not get DataContext from Universal Transaction Batch Request", xmlSessionTracker.ToString());
			AssertEquals("ResponseMessageText should be null", null, processingResult.ResponseMessageText);
		}

		#region Implementation

		IHttpXmlRequestResponse CreateRequestWithErrorCompanyCode(UniversalTransactionBatchRequestHandler handler, string action)
		{
			var xml = GetRequestXmlWithErrorCompanyCode(action);
			return CreateRequestCore(handler, xml);
		}

		IHttpXmlRequestResponse CreateRequest(UniversalTransactionBatchRequestHandler handler, string action, int batchNumber = 0)
		{
			var xml = batchNumber != 0 && action == AccBatchRequestTypePairList.Codes.Export ? GetExportRequestXml(batchNumber) : GetRequestXml(action);
			return CreateRequestCore(handler, xml);
		}

		IHttpXmlRequestResponse CreateRequestCore(UniversalTransactionBatchRequestHandler handler, string xml)
		{
			var result = handler.CreateRequestMessage();
			return CreateRequestWithXml(handler, xml);
		}

		IHttpXmlRequestResponse CreateRequestWithXml(UniversalTransactionBatchRequestHandler handler, string requestXml)
		{
			var result = handler.CreateRequestMessage();
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				new StreamWriter(stream) { AutoFlush = true }.Write(requestXml);
				result.SetMessageTextSource(stream);
				result.Save();
			}

			return result;
		}

		const string xmlEmptyRequest = "<TransactionBatchRequest/>";
		const string xmlNoDataContext = @"<UniversalTransactionBatchRequest>
<TransactionBatchRequest>
<ActionType>
<Code>XXX</Code>
</ActionType>
</TransactionBatchRequest>
</UniversalTransactionBatchRequest>";

		const string xmlNoDataCollectionType = @"<UniversalTransactionBatchRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionBatchRequest>
    <DataContext>
      <Company>
        <Code>EXP</Code>
        <Name>Test_name</Name>
      </Company>
    </DataContext>
    <ActionType>
      <Code>EXP</Code>
    </ActionType>
  </TransactionBatchRequest>
</UniversalTransactionBatchRequest>";

		string GetRequestXmlWithErrorCompanyCode(string action)
		{
			return string.Format(@"<UniversalTransactionBatchRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionBatchRequest>
    <DataContext>
      <Company>
        <Code>{0}</Code>
        <Name>{1}</Name>
      </Company>
      <EnterpriseID>HYE</EnterpriseID>
      <ServerID>BEN</ServerID>
    </DataContext>
    <ActionType>
      <Code>{2}</Code>
    </ActionType>
  </TransactionBatchRequest>
</UniversalTransactionBatchRequest>", "XXX", GlbCompany.CurrentCompany.GC_Name, action);
		}

		string GetRequestXml(string action)
		{
			return string.Format(@"<UniversalTransactionBatchRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionBatchRequest>
    <DataContext>
      <Company>
        <Code>{0}</Code>
        <Name>{1}</Name>
      </Company>
      <EnterpriseID>HYE</EnterpriseID>
      <ServerID>BEN</ServerID>
    </DataContext>
    <ActionType>
      <Code>{2}</Code>
    </ActionType>
  </TransactionBatchRequest>
</UniversalTransactionBatchRequest>", GlbCompany.CurrentCompany.GC_Code, GlbCompany.CurrentCompany.GC_Name, action);
		}

		string GetRequestXmlWithNewlineChar(string action)
		{
			return string.Format(@"<UniversalTransactionBatchRequest 
xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" 
version=""1.1"">
  <TransactionBatchRequest>
    <DataContext>
      <Company>
        <Code>{0}</Code>
        <Name>{1}</Name>
      </Company>
      <EnterpriseID>HYE</EnterpriseID>
      <ServerID>BEN</ServerID>
    </DataContext>
    <ActionType>
      <Code>{2}</Code>
    </ActionType>
  </TransactionBatchRequest>
</UniversalTransactionBatchRequest>", GlbCompany.CurrentCompany.GC_Code, GlbCompany.CurrentCompany.GC_Name, action);
		}

		string GetExportRequestXml(int batchNumber)
		{
			return string.Format(@"<UniversalTransactionBatchRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionBatchRequest>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>BatchNumber</Type>
          <Key>{3}</Key>
        </DataTarget>
      </DataTargetCollection>

      <Company>
        <Code>{0}</Code>
        <Name>{1}</Name>
      </Company>
      <EnterpriseID>HYE</EnterpriseID>
      <ServerID>BEN</ServerID>
    </DataContext>
    <ActionType>
      <Code>{2}</Code>
    </ActionType>
  </TransactionBatchRequest>
</UniversalTransactionBatchRequest>", GlbCompany.CurrentCompany.GC_Code, GlbCompany.CurrentCompany.GC_Name, AccBatchRequestTypePairList.Codes.Export, batchNumber);
		}

		string GetExpectedBatchExportXml(bool enableSupplyType, bool enableTaxBranchReporting, string code, string description)
		{
			return string.Format(CultureInfo.InvariantCulture, $@"<UniversalTransactionBatch xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionBatch>
    <BatchType>
      <Code>{{0}}</Code>
      <Description>{{1}}</Description>
    </BatchType>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>BatchNumber</Type>
          <Key>1</Key>
        </DataSource>
      </DataSourceCollection>

      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>EDIDATEDI</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>
    <TransactionCollection>
      <Transaction>

        <Branch>
          <Code>BNE</Code>
          <Name>BN - AUBNE</Name>
        </Branch>
        <Department>
          <Code>BRN</Code>
          <Name>Branch</Name>
        </Department>
        <Description>Charge Code 1</Description>
        <Job>
          <Type>Job</Type>
          <Key>S0001</Key>
        </Job>
        <Ledger>JC</Ledger>
        <LocalExVATAmount>100.0000</LocalExVATAmount>
        <LocalTotal>100.0000</LocalTotal>
        <OSCurrency>
          <Code>AUD</Code>
          <Description>Australian Dollar</Description>
        </OSCurrency>
        <OSExGSTVATAmount>100.0000</OSExGSTVATAmount>
        <OSTotal>100.0000</OSTotal>
        <PostDate>2016-03-30T03:53:00</PostDate>
        <TransactionDate>2016-03-30T03:53:00</TransactionDate>
        <TransactionType>ACR</TransactionType>
        <PostingJournalCollection>
          <PostingJournal>
            <BatchSequence>1</BatchSequence>
            <Branch>
              <Code>BNE</Code>
              <Name>BN - AUBNE</Name>
            </Branch>
            <ChargeCode>
              <Code>ZZCC1</Code>
              <ChargeType>
                <Code>MRG</Code>
                <Description>Margin</Description>
              </ChargeType>
              <Class>
                <Code>SRV</Code>
                <Description>Service</Description>
              </Class>
              <Description>Charge Code 1</Description>
            </ChargeCode>
            <ChargeCurrency>
              <Code>AUD</Code>
              <Description>Australian Dollar</Description>
            </ChargeCurrency>
            <ChargeExchangeRate>1.000000000</ChargeExchangeRate>
            <ChargeTotalAmount>-100.0000</ChargeTotalAmount>
            <ChargeTotalExVATAmount>-100.0000</ChargeTotalExVATAmount>
            <ChargeTotalVATAmount>0</ChargeTotalVATAmount>
            <Department>
              <Code>BRN</Code>
              <Name>Branch</Name>
            </Department>
            <Description>Charge Code 1</Description>
            <GLAccount>
              <AccountCode>XXXX.00.01</AccountCode>
              <Description></Description>
            </GLAccount>
            <GLPostDate>2016-03-30T03:53:00</GLPostDate>
            <Job>
              <Type>Job</Type>
              <Key>S0001</Key>
            </Job>
            <LocalAmount>100.0000</LocalAmount>
            <LocalCurrency>
              <Code>AUD</Code>
              <Description>Australian Dollar</Description>
            </LocalCurrency>
            <LocalTotalAmount>100.0000</LocalTotalAmount>
            <OSAmount>100.0000</OSAmount>
            <OSCurrency>
              <Code>AUD</Code>
              <Description>Australian Dollar</Description>
            </OSCurrency>
            <OSTotalAmount>100.0000</OSTotalAmount>
            <RevenueRecognitionType>IMM</RevenueRecognitionType>
            <Sequence>1</Sequence>{(
	enableSupplyType
	? @"
            <SupplyType>
              <Code></Code>
              <Description></Description>
            </SupplyType>"
	: string.Empty)}
            <TransactionType>ACR</TransactionType>
            <PostingJournalDetailCollection>
              <PostingJournalDetail>
                <CreditGLAccount>
                  <AccountCode>XXXX.00.01</AccountCode>
                  <Description></Description>
                </CreditGLAccount>
                <DebitGLAccount>
                  <AccountCode>8410.10.00</AccountCode>
                  <Description>ACCRUAL - JOB COSTING</Description>
                </DebitGLAccount>
                <PostingAmount>100.0000</PostingAmount>
                <PostingCurrency>
                  <Code>AUD</Code>
                  <Description>Australian Dollar</Description>
                </PostingCurrency>
                <PostingDate>2016-03-30T03:53:00</PostingDate>
              </PostingJournalDetail>
            </PostingJournalDetailCollection>
          </PostingJournal>
        </PostingJournalCollection>
      </Transaction>
      <Transaction>

        <Branch>
          <Code>BNE</Code>
          <Name>BN - AUBNE</Name>
        </Branch>
        <BranchAddress>
          <AddressType>OFC</AddressType>
          <Address1>10 HUTCHESON STREET</Address1>
          <Address2>ALBION  QLD</Address2>
          <AddressOverride>false</AddressOverride>
          <AddressShortCode>PST: 10 HUTCHESON STREET</AddressShortCode>
          <City></City>
          <CompanyName>EDI CUSTOMS BROKERS</CompanyName>
          <Country>
            <Code>AU</Code>
            <Name>Australia</Name>
          </Country>
          <Email></Email>
          <Fax></Fax>
          <OrganizationCode>EDICUS</OrganizationCode>
          <Phone></Phone>
          <Port>
            <Code>AUBNE</Code>
            <Name>Brisbane</Name>
          </Port>
          <Postcode>4010</Postcode>
          <ScreeningStatus>
            <Code>UNK</Code>
            <Description>Unknown</Description>
          </ScreeningStatus>
          <State></State>
        </BranchAddress>
        <Category></Category>
        <CheckDrawer></CheckDrawer>
        <CheckNumberOrPaymentRef></CheckNumberOrPaymentRef>
        <CreateTime>2016-03-30T03:53:00</CreateTime>
        <CreateUser>E</CreateUser>
        <Department>
          <Code>BRN</Code>
          <Name>Branch</Name>
        </Department>
        <Description>Transaction to batch</Description>
        <DrawerBank></DrawerBank>
        <DrawerBranch></DrawerBranch>
        <DueDate>2016-03-30T00:00:00</DueDate>
        <ExchangeRate>1.000000</ExchangeRate>
        <InvoiceTerm>COD</InvoiceTerm>
        <InvoiceTermDays>0</InvoiceTermDays>
        <IsCancelled>false</IsCancelled>
        <IsCreatedByMatchingProcess>false</IsCreatedByMatchingProcess>
        <IsPrinted>false</IsPrinted>
        <Job>
          <Type>Job</Type>
        </Job>
        <JobInvoiceNumber></JobInvoiceNumber>
        <Ledger>AR</Ledger>
        <LocalCurrency>
          <Code>AUD</Code>
          <Description>Australian Dollar</Description>
        </LocalCurrency>
        <LocalExVATAmount>-100.0000</LocalExVATAmount>
        <LocalTaxTransactionsAmount>0.0000</LocalTaxTransactionsAmount>
        <LocalTotal>-110.0000</LocalTotal>
        <LocalVATAmount>-10.0000</LocalVATAmount>
        <Number>00001000</Number>
        <NumberOfSupportingDocuments>1</NumberOfSupportingDocuments>
        <OrganizationAddress>
          <AddressType>OFC</AddressType>
          <Address1>171 ABBOTSFORD ROAD</Address1>
          <Address2>MAYNE, QLD</Address2>
          <AddressOverride>false</AddressOverride>
          <AddressShortCode>PST: 171 ABBOTSFORD ROAD</AddressShortCode>
          <City></City>
          <CompanyName>ABI GAS &amp; TOOLS</CompanyName>
          <Contact>LYNN MCVIE</Contact>
          <Country>
            <Code>AU</Code>
            <Name>Australia</Name>
          </Country>
          <Email></Email>
          <Fax></Fax>
          <OrganizationCode>ABIGAS</OrganizationCode>
          <Phone></Phone>
          <Port>
            <Code>AUBNE</Code>
            <Name>Brisbane</Name>
          </Port>
          <Postcode>4006</Postcode>
          <ScreeningStatus>
            <Code>UNK</Code>
            <Description>Unknown</Description>
          </ScreeningStatus>
          <State></State>
        </OrganizationAddress>
        <OSCurrency>
          <Code>AUD</Code>
          <Description>Australian Dollar</Description>
        </OSCurrency>
        <OSExGSTVATAmount>-100.0000</OSExGSTVATAmount>
        <OSGSTVATAmount>-10.00</OSGSTVATAmount>
        <OSTaxTransactionsAmount>0.0000</OSTaxTransactionsAmount>
        <OSTotal>-110.0000</OSTotal>
        <OutstandingAmount>-110.0000</OutstandingAmount>
        <PlaceOfIssue>Brisbane</PlaceOfIssue>
        <PostDate>2016-03-30T00:00:00</PostDate>
        <ReceiptOrDirectDebitNumber></ReceiptOrDirectDebitNumber>
        <RequisitionStatus></RequisitionStatus>{(
	enableTaxBranchReporting
	? @"
        <TaxBranch>
          <Code>B1</Code>
          <Name>TaxBranch</Name>
        </TaxBranch>
        <TaxBranchAddress>
          <AddressType>OFC</AddressType>
          <Address1>Test Address Line 1</Address1>
          <Address2>Test Address Line 2</Address2>
          <AddressOverride>false</AddressOverride>
          <AddressShortCode>Test Short Code</AddressShortCode>
          <City>TestCity</City>
          <CompanyName>Test Company Name</CompanyName>
          <Country>
            <Code>AU</Code>
            <Name>Australia</Name>
          </Country>
          <Email></Email>
          <Fax></Fax>
          <OrganizationCode>TAXORGCODE</OrganizationCode>
          <Phone></Phone>
          <Port>
            <Code>AUSYD</Code>
            <Name>Sydney</Name>
          </Port>
          <Postcode></Postcode>
          <ScreeningStatus>
            <Code>NOT</Code>
            <Description>Not Screened</Description>
          </ScreeningStatus>
          <State>NSW</State>
        </TaxBranchAddress>"
	: string.Empty)}
        <TransactionDate>2016-03-30T00:00:00</TransactionDate>
        <TransactionType>CRD</TransactionType>
        <PostingJournalCollection>
          <PostingJournal>
            <Branch>
              <Code>BNE</Code>
              <Name>BN - AUBNE</Name>
            </Branch>
            <ChargeCode>
              <Code>ZZCC1</Code>
              <ChargeType>
                <Code>MRG</Code>
                <Description>Margin</Description>
              </ChargeType>
              <Class>
                <Code>SRV</Code>
                <Description>Service</Description>
              </Class>
              <Description>Charge Code 1</Description>
            </ChargeCode>
            <ChargeCurrency>
              <Code>AUD</Code>
              <Description>Australian Dollar</Description>
            </ChargeCurrency>
            <ChargeExchangeRate>1.000000000</ChargeExchangeRate>
            <ChargeTotalAmount>-110.0000</ChargeTotalAmount>
            <ChargeTotalExVATAmount>-100.0000</ChargeTotalExVATAmount>
            <Department>
              <Code>BRN</Code>
              <Name>Branch</Name>
            </Department>
            <Description>Transaction to batch</Description>
            <GLAccount>
              <AccountCode>XXXX.00.01</AccountCode>
              <Description></Description>
            </GLAccount>
            <GLPostDate>2016-03-30T00:00:00</GLPostDate>
            <IsFinalCharge>false</IsFinalCharge>
            <Job>
              <Type>Job</Type>
              <Key>S0001</Key>
            </Job>
            <LocalAmount>-100.0000</LocalAmount>
            <LocalCurrency>
              <Code>AUD</Code>
              <Description>Australian Dollar</Description>
            </LocalCurrency>
            <LocalGSTVATAmount>-10.0000</LocalGSTVATAmount>
            <LocalTotalAmount>-110.0000</LocalTotalAmount>
            <Organization>
              <Type>Organization</Type>
              <Key>ABIGAS</Key>
            </Organization>
            <OSAmount>-100.00</OSAmount>
            <OSCurrency>
              <Code>AUD</Code>
              <Description>Australian Dollar</Description>
            </OSCurrency>
            <OSGSTVATAmount>-10.0000</OSGSTVATAmount>
            <OSTotalAmount>-110.0000</OSTotalAmount>
            <RevenueRecognitionType>IMM</RevenueRecognitionType>
            <Sequence>1</Sequence>{(
	enableSupplyType
	? @"
            <SupplyType>
              <Code>DSB</Code>
              <Description>DSB - Disbursement/Reimbursement</Description>
            </SupplyType>"
	: string.Empty)}{(
	enableTaxBranchReporting
		? @"
            <TaxBranch>
              <Code>B1</Code>
              <Name>TaxBranch</Name>
            </TaxBranch>"
	: string.Empty)}
            <TaxDate>2016-03-30</TaxDate>
            <TransactionCategory></TransactionCategory>
            <TransactionType>REV</TransactionType>
            <VATTaxID>
              <TaxCode>ZZGST1</TaxCode>
              <Description>GST Rate 1</Description>
              <TaxRate>10</TaxRate>
              <TaxType>
                <Code>RAT</Code>
              </TaxType>
            </VATTaxID>
            <PostingJournalDetailCollection>
              <PostingJournalDetail>
                <CreditGLAccount>
                  <AccountCode>1030.00.00</AccountCode>
                  <Description>GROSS PORT &amp; TERMINAL REVENUE</Description>
                </CreditGLAccount>
                <DebitGLAccount>
                  <AccountCode>1040.20.00</AccountCode>
                  <Description>DOCUMENTATION COSTS</Description>
                </DebitGLAccount>
                <PostingAmount>100.0000</PostingAmount>
                <PostingCurrency>
                  <Code>AUD</Code>
                  <Description>Australian Dollar</Description>
                </PostingCurrency>
                <PostingDate>2016-03-30T00:00:00</PostingDate>
              </PostingJournalDetail>
              <PostingJournalDetail>
                <CreditGLAccount>
                  <AccountCode>1030.00.00</AccountCode>
                  <Description>GROSS PORT &amp; TERMINAL REVENUE</Description>
                </CreditGLAccount>
                <DebitGLAccount>
                  <AccountCode>8310.00.00</AccountCode>
                  <Description>OUTPUT TAX PAYABLE </Description>
                </DebitGLAccount>
                <PostingAmount>10.0000</PostingAmount>
                <PostingCurrency>
                  <Code>AUD</Code>
                  <Description>Australian Dollar</Description>
                </PostingCurrency>
                <PostingDate>2016-03-30T00:00:00</PostingDate>
              </PostingJournalDetail>
              <PostingJournalDetail>
                <CreditGLAccount>
                  <AccountCode>1040.20.00</AccountCode>
                  <Description>DOCUMENTATION COSTS</Description>
                </CreditGLAccount>
                <DebitGLAccount>
                  <AccountCode>XXXX.00.01</AccountCode>
                  <Description></Description>
                </DebitGLAccount>
                <PostingAmount>100.0000</PostingAmount>
                <PostingCurrency>
                  <Code>AUD</Code>
                  <Description>Australian Dollar</Description>
                </PostingCurrency>
                <PostingDate>2016-03-30T00:00:00</PostingDate>
              </PostingJournalDetail>
            </PostingJournalDetailCollection>
          </PostingJournal>
        </PostingJournalCollection>
      </Transaction>
    </TransactionCollection>
  </TransactionBatch>
</UniversalTransactionBatch>", code, description);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var testHelper = new BatchTestHelper(Factory);
			testHelper.SetControlAccounts();
			var testAggregator = new TestBatchAggregator(testHelper);
			testAggregator.SetControlAccount(AccountingUtils.ARControl, testHelper.ARControlAccount);
			testAggregator.SetControlAccount(AccountingUtils.ARSuspenseControlAccount, testHelper.ARSuspenseControlAccount);
			testAggregator.SetControlAccount(AccountingUtils.APSuspenseControlAccount, testHelper.APSuspenseControlAccount);
			testAggregator.SetControlAccount(AccountingUtils.JobRevenueJournalControlAccount, testHelper.JobRevenueJournalControlAccount);
		}

		#endregion
	}

	class UniversalTransactionBatchRequestHandlerForTest : UniversalTransactionBatchRequestHandler
	{
		public UniversalTransactionBatchRequestHandlerForTest(IXmlSessionTracker xmlSessionTracker) : base(xmlSessionTracker)
		{
		}

		protected override AccountingTransactionEAdaptorExporter GetExporter()
		{
			var conn = (IDbConnectionInternals)Db.Connection;
			var dataAccess = new BatchExportDataAccess(conn.ADOConnection, conn.ADOTransaction);

			var exporter = new AccountingTransactionEAdaptorExporterForTest(dataAccess);
			return exporter;
		}
	}
}

