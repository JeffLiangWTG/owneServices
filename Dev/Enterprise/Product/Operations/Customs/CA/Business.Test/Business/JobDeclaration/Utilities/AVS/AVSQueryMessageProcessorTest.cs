using System;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.CA.Services;
using Enterprise.Customs.CA.Services.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class AVSQueryMessageProcessorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestProcessInvalidUriThrowNoException()
		{
			using (CACustomsDataRegistry.Instance.AIRSValidationKey.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "TESTKEY"))
			using (CACustomsDataRegistry.Instance.AIRSValidationRequestURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Invalid URL"))
			using (CACustomsDataRegistry.Instance.WebProxyAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Invalid Proxy"))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
				declaration.JE_DeclarationReference = "B00000001";

				var submitAVSQueryProcessor = new SubmitAVSQueryProcessor(declaration);
				submitAVSQueryProcessor.Process(new Notifications());
				Factory.Save();
				var messages = Factory.Load<AVSQueryMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.CFIAQuery));

				var logger = new DummyLogger();
				var proxy = new AIRSValidationServiceProxyForTesting(new InvalidAIRSValidationServiceSettingsForTesting());
				var message = Factory.Load<AVSQueryMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.CFIAQuery))[0];
				message.EM_HeldUntilDate = ZDateTime.UtcNow;
				message.EM_Status = EDIMessage.Status.Queued;
				Factory.Save();
				var queryProcessor = new AVSQueryValidateMessageProcessorForTesting(logger, proxy);
				queryProcessor.Process();
				AssertEquals(@"Error - An invalid URL entered in Registry -> Customs -> Country or Region Specific -> Canada -> Import -> Declaration -> CFIA -> AIRS Validation Request URL
An invalid URL entered in Registry -> Customs -> Country or Region Specific -> Canada -> Import -> Declaration -> CFIA -> Web Proxy Address", logger.Take(2).Last());
			}
		}

		public void TestProcessThrowZSaveConcurrencyException()
		{
			using (CACustomsDataRegistry.Instance.AIRSValidationKey.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "TESTKEY"))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
				declaration.JE_DeclarationReference = "B00000001";

				var submitAVSQueryProcessor = new SubmitAVSQueryProcessor(declaration);
				submitAVSQueryProcessor.Process(new Notifications());
				Factory.Save();
				var messages = Factory.Load<AVSQueryMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.CFIAQuery));
				AssertEquals("Precondition: An AVS Query message should be added", 1, messages.Length);

				var logger = new DummyLogger();
				var proxy = new AIRSValidationServiceProxyForTesting(new AIRSOGDValidationServiceSettingsForTesting());
				var queryProcessor = new AVSQueryMessageProcessorForTesting(logger, proxy);
				queryProcessor.Process();
				AssertEquals("Information - AIRS Validation Query executing for Company EDI.", logger.First());
				AssertEquals("Error - AIRS Validation Query for Declaration B00000001: There's no invoice line for AIRS Validation.", logger.Last());
				var message = Factory.Load<AVSQueryMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.CFIAQuery))[0];

				var invoice = declaration.Invoices.AddNew();
				invoice.FillWithValidTestData();
				var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
				invoiceLine1.FillWithValidTestData();
				var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
				invoiceLine2.FillWithValidTestData();
				invoiceLine2.JI_Tariff = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.CFIA);
				invoiceLine2.CA_OGDStatus = AVSStatusList.Codes.Blank;
				var invoiceLine3 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
				invoiceLine3.FillWithValidTestData();
				invoiceLine3.JI_Tariff = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.CFIA);
				invoiceLine3.CA_OGDStatus = AVSStatusList.Codes.Blank;

				message.EM_HeldUntilDate = ZDateTime.UtcNow;
				message.EM_Status = EDIMessage.Status.Queued;
				Factory.Save();

				queryProcessor.additonalActionForTesting = new Action<JobDeclaration>((dec) =>
				{
					dec.Factory.Saving += (factory) =>
					{
						throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception("Dummy exception thrown during process AVS Query message"), ((IBusinessObjectInternals)dec).Row, Db.Connection), Factory);
					};
				});

				queryProcessor.Process();
				AssertEquals("Error - AIRS Validation Query failed for Declaration B00000001. There was a conflict with another users changes and the changes could not be saved.", logger.Last());
			}
		}

		public void TestProcess()
		{
			using (CACustomsDataRegistry.Instance.AIRSValidationKey.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "TESTKEY"))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
				declaration.JE_DeclarationReference = "B00000001";

				var submitAVSQueryProcessor = new SubmitAVSQueryProcessor(declaration);
				submitAVSQueryProcessor.Process(new Notifications());
				Factory.Save();
				var messages = Factory.Load<AVSQueryMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.CFIAQuery));
				AssertEquals("Precondition: An AVS Query message should be added", 1, messages.Length);

				var logger = new DummyLogger();
				var proxy = new AIRSValidationServiceProxyForTesting(new AIRSOGDValidationServiceSettingsForTesting());
				var queryProcessor = new AVSQueryMessageProcessorForTesting(logger, proxy);
				queryProcessor.Process();
				AssertEquals("Information - AIRS Validation Query executing for Company EDI.", logger.First());
				AssertEquals("Error - AIRS Validation Query for Declaration B00000001: There's no invoice line for AIRS Validation.", logger.Last());
				var message = Factory.Load<AVSQueryMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.CFIAQuery))[0];
				AssertEquals("EM_MessageText", "<AVSQueryRetryInfo><RetryTimes>0</RetryTimes><LastFailure>There's no invoice line for AIRS Validation.</LastFailure></AVSQueryRetryInfo>", message.EM_MessageText);
				AssertEquals("EM_Status", EDIMessage.Status.Warning, message.EM_Status);

				var invoice = declaration.Invoices.AddNew();
				invoice.FillWithValidTestData();
				var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
				invoiceLine1.FillWithValidTestData();
				var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
				invoiceLine2.FillWithValidTestData();
				invoiceLine2.JI_Tariff = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.CFIA);
				invoiceLine2.CA_OGDStatus = AVSStatusList.Codes.Blank;
				var invoiceLine3 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
				invoiceLine3.FillWithValidTestData();
				invoiceLine3.JI_Tariff = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.CFIA);
				invoiceLine3.CA_OGDStatus = AVSStatusList.Codes.Blank;

				message.EM_HeldUntilDate = ZDateTime.UtcNow;
				message.EM_Status = EDIMessage.Status.Queued;
				Factory.Save();

				logger = new DummyLogger();
				queryProcessor = new AVSQueryMessageProcessorForTesting(logger, proxy);
				queryProcessor.Process();
				AssertEquals("Information - AIRS Validation Query executing for Company EDI.", logger.First());
				AssertEquals("Information - AIRS Validation Query Succeed for Declaration B00000001.", logger.Last());
				AssertEquals("invoiceLine2.CA_OGDStatus", "OKA", invoiceLine2.CA_OGDStatus);
				AssertEquals("invoiceLine3.CA_OGDStatus", "OKA", invoiceLine3.CA_OGDStatus);
				AssertEquals("EM_MessageText", ZString.Empty, message.EM_MessageText);
				AssertEquals("EM_Status", EDIMessage.Status.Acknowledged, message.EM_Status);
				AssertEquals("EM_HeldUntilDate", ZDateTime.Empty, message.EM_HeldUntilDate);
			}
		}

		[TestDate(2016, 2, 19)]
		public void TestProcess_Retry()
		{
			using (CACustomsDataRegistry.Instance.AVSRetryTimes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			using (CACustomsDataRegistry.Instance.AVSRetryIntervalInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 10))
			using (CACustomsDataRegistry.Instance.AIRSValidationKey.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "TESTKEY"))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
				declaration.JE_DeclarationReference = "B00000001";
				var invoice = declaration.Invoices.AddNew();
				invoice.FillWithValidTestData();
				var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
				invoiceLine1.FillWithValidTestData();
				var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
				invoiceLine2.FillWithValidTestData();
				invoiceLine2.JI_Tariff = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.CFIA);
				invoiceLine2.CA_OGDStatus = AVSStatusList.Codes.Blank;
				var invoiceLine3 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
				invoiceLine3.FillWithValidTestData();
				invoiceLine3.JI_Tariff = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.CFIA);
				invoiceLine3.CA_OGDStatus = AVSStatusList.Codes.Blank;

				var submitAVSQueryProcessor = new SubmitAVSQueryProcessor(declaration);
				submitAVSQueryProcessor.Process(new Notifications());
				Factory.Save();
				var messages = Factory.Load<AVSQueryMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.CFIAQuery));
				AssertEquals("Precondition: An AVS Query message should be added", 1, messages.Length);

				var logger = new DummyLogger();
				var proxy = new AIRSValidationServiceProxyForTesting(new AIRSOGDValidationServiceSettingsForTesting());
				proxy.TaskDelay = 100;
				var queryProcessor = new AVSQueryMessageProcessorForTesting(logger, proxy);
				queryProcessor.AIRSValidationTimeout = 50;
				queryProcessor.Process();
				AssertEquals("Information - AIRS Validation Query executing for Company EDI.", logger.First());
				AssertEquals("Error - AIRS Validation Query for Declaration B00000001: AIRS Validation Query Aborted", logger.Last());
				var message = Factory.Load<AVSQueryMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.CFIAQuery))[0];
				AssertEquals("EM_MessageText", "<AVSQueryRetryInfo><RetryTimes>0</RetryTimes><LastFailure>AIRS Validation Query Aborted</LastFailure></AVSQueryRetryInfo>", message.EM_MessageText);
				AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);
				AssertEquals("EM_HeldUntilDate", ZDateTime.UtcNow.AddMinutes(10), message.EM_HeldUntilDate);

				message.EM_HeldUntilDate = ZDateTime.UtcNow;
				Factory.Save();
				queryProcessor.Process();
				AssertEquals("Information - AIRS Validation Query executing for Company EDI.", logger.First());
				AssertEquals("Error - AIRS Validation Query for Declaration B00000001: AIRS Validation Query Aborted", logger.Last());
				message = Factory.Load<AVSQueryMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.CFIAQuery))[0];
				AssertEquals("EM_MessageText", "<AVSQueryRetryInfo><RetryTimes>1</RetryTimes><LastFailure>AIRS Validation Query Aborted</LastFailure></AVSQueryRetryInfo>", message.EM_MessageText);
				AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);
				AssertEquals("EM_HeldUntilDate", ZDateTime.UtcNow.AddMinutes(10), message.EM_HeldUntilDate);

				message.EM_HeldUntilDate = ZDateTime.UtcNow;
				Factory.Save();
				queryProcessor.Process();
				AssertEquals("Information - AIRS Validation Query executing for Company EDI.", logger.First());
				AssertEquals("Error - AIRS Validation Query for Declaration B00000001: AIRS Validation Query Aborted", logger.Last());
				message = Factory.Load<AVSQueryMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.CFIAQuery))[0];
				AssertEquals("EM_MessageText", "<AVSQueryRetryInfo><RetryTimes>2</RetryTimes><LastFailure>AIRS Validation Query Aborted</LastFailure></AVSQueryRetryInfo>", message.EM_MessageText);
				AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);
				AssertEquals("EM_HeldUntilDate", ZDateTime.UtcNow.AddHours(12), message.EM_HeldUntilDate);

				message.EM_HeldUntilDate = ZDateTime.UtcNow;
				Factory.Save();
				var proxy2 = new AIRSValidationServiceProxyForTesting(new AIRSOGDValidationServiceSettingsForTesting());
				proxy2.SetCommunicationException();
				queryProcessor = new AVSQueryMessageProcessorForTesting(logger, proxy2);
				queryProcessor.Process();
				AssertEquals("Information - AIRS Validation Query executing for Company EDI.", logger.First());
				AssertEquals("Error - AIRS Validation Query for Declaration B00000001: Cannot connect to the AIRS Validation Service", logger.Last());
				message = Factory.Load<AVSQueryMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.CFIAQuery))[0];
				AssertEquals("EM_MessageText", "<AVSQueryRetryInfo><RetryTimes>3</RetryTimes><LastFailure>Cannot connect to the AIRS Validation Service</LastFailure></AVSQueryRetryInfo>", message.EM_MessageText);
				AssertEquals("EM_Status", EDIMessage.Status.Failed, message.EM_Status);
				AssertEquals("EM_HeldUntilDate", ZDateTime.Empty, message.EM_HeldUntilDate);
			}
		}

		sealed class InvalidAIRSValidationServiceSettingsForTesting : IAIRSValidationServiceSettings
		{
			public ZString Uri => "Invalid Uri";

			public string SchemaVersion => "1.0";

			public string Key => "";

			public bool FrenchPreferred => false;

			public string UserName => string.Empty;

			public string Password => string.Empty;

			public ZString WebProxyUri => "Invalid Web Proxy";
		}

		sealed class AVSQueryMessageProcessorForTesting : AVSQueryMessageProcessor
		{
			public AVSQueryMessageProcessorForTesting(ILogger serviceLogger, AIRSValidationServiceProxy proxy) : base(serviceLogger)
			{
				AIRSValidationServiceProxy = proxy;
			}

			public Action<JobDeclaration> additonalActionForTesting;

			public int AIRSValidationTimeout;

			public AIRSValidationServiceProxy AIRSValidationServiceProxy;

			protected override AIRSValidationRunner GetAIRSValidationRunner(JobDeclaration declaration)
			{
				if (additonalActionForTesting != null)
				{
					additonalActionForTesting(declaration);
				}
				return new AIRSValidationRunner(declaration, AIRSValidationServiceProxy);
			}

			protected override CancellationTokenSource CreateNewCancellationTokenSource(JobDeclaration declaration) => new CancellationTokenSource(AIRSValidationTimeout);
		}

		sealed class AVSQueryValidateMessageProcessorForTesting : AVSQueryMessageProcessor
		{
			public AVSQueryValidateMessageProcessorForTesting(ILogger serviceLogger, AIRSValidationServiceProxy proxy) : base(serviceLogger)
			{
				AIRSValidationServiceProxy = proxy;
			}

			public int AIRSValidationTimeout;

			public AIRSValidationServiceProxy AIRSValidationServiceProxy;

			protected override AIRSValidationRunner GetAIRSValidationRunner(JobDeclaration declaration) => new AIRSValidationRunner(declaration);

			protected override CancellationTokenSource CreateNewCancellationTokenSource(JobDeclaration declaration) => new CancellationTokenSource(AIRSValidationTimeout);
		}
	}
}
