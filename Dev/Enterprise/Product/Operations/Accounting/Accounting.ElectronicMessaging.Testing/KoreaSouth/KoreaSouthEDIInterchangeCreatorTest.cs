using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Testing
{
	public class KoreaSouthEDIInterchangeCreatorTest : GEIEDIInterchangeCreatorTest
	{
		public void TestSubmitId_QueryResult_Production()
		{
			var mockProductRegistrationKey = new Mock<IProductRegistrationKey>();
			mockProductRegistrationKey.Setup(x => x.EnterpriseCode).Returns("AAA");
			mockProductRegistrationKey.Setup(x => x.ServerCode).Returns("111");
			var mockProductRegistration = new Mock<IProductRegistration>();
			mockProductRegistration.Setup(x => x.Key).Returns(mockProductRegistrationKey.Object);
			GlbCompany.CurrentCompany.GC_Code = "DKR";
			GlbCompany.CurrentCompany.Factory.Save();

			using (ObjectFactory.Substitute(mockProductRegistration.Object))
			using (AccountingMasterFilesRegistry.Instance.EReportingGEIMessageSystemType.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.AlwaysProductionSystem))
			{
				AssertSubmitId_QueryResult("41000193", "414141444b5231313100000000000001");
			}
		}

		public void TestSubmitId_QueryResult_Testing()
		{
			var mockProductRegistrationKey = new Mock<IProductRegistrationKey>();
			mockProductRegistrationKey.Setup(x => x.EnterpriseCode).Returns("BBB");
			mockProductRegistrationKey.Setup(x => x.ServerCode).Returns("222");
			var mockProductRegistration = new Mock<IProductRegistration>();
			mockProductRegistration.Setup(x => x.Key).Returns(mockProductRegistrationKey.Object);
			GlbCompany.CurrentCompany.GC_Code = "DKR";
			GlbCompany.CurrentCompany.Factory.Save();

			using (ObjectFactory.Substitute(mockProductRegistration.Object))
			using (AccountingMasterFilesRegistry.Instance.EReportingGEIMessageSystemType.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.AlwaysTestSystem))
			{
				AssertSubmitId_QueryResult("12345678", "424242444b5232323200000000000001");
			}
		}

		public void TestSubmitId_SubmitInvoice_Production()
		{
			var mockProductRegistrationKey = new Mock<IProductRegistrationKey>();
			mockProductRegistrationKey.Setup(x => x.EnterpriseCode).Returns("CCC");
			mockProductRegistrationKey.Setup(x => x.ServerCode).Returns("333");
			var mockProductRegistration = new Mock<IProductRegistration>();
			mockProductRegistration.Setup(x => x.Key).Returns(mockProductRegistrationKey.Object);
			GlbCompany.CurrentCompany.GC_Code = "DKR";
			GlbCompany.CurrentCompany.Factory.Save();

			using (ObjectFactory.Substitute(mockProductRegistration.Object))
			using (AccountingMasterFilesRegistry.Instance.EReportingGEIMessageSystemType.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.AlwaysProductionSystem))
			{
				AssertSubmitId_SubmitInvoice("41000193", "434343444b5233333300000000000001");
			}
		}

		public void TestSubmitId_SubmitInvoice_Testing()
		{
			var mockProductRegistrationKey = new Mock<IProductRegistrationKey>();
			mockProductRegistrationKey.Setup(x => x.EnterpriseCode).Returns("DDD");
			mockProductRegistrationKey.Setup(x => x.ServerCode).Returns("444");
			var mockProductRegistration = new Mock<IProductRegistration>();
			mockProductRegistration.Setup(x => x.Key).Returns(mockProductRegistrationKey.Object);
			GlbCompany.CurrentCompany.GC_Code = "DKR";
			GlbCompany.CurrentCompany.Factory.Save();

			using (ObjectFactory.Substitute(mockProductRegistration.Object))
			using (AccountingMasterFilesRegistry.Instance.EReportingGEIMessageSystemType.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.AlwaysTestSystem))
			{
				AssertSubmitId_SubmitInvoice("12345678", "444444444b5234343400000000000001");
			}
		}

		void AssertSubmitId_QueryResult(string expectedRegistryId, string expectedSubmitIdLastPart)
		{
			var (batch, submitBatch) = GetBatchWithReadyStatus(EInvoicingPivotActionType.StatusCheck);
			submitBatch.AIB_SystemCreateTimeUtc = batch.AIB_SystemCreateTimeUtc.AddMonths(1);
			batch.AIB_GC = TestObjectCreator.NonCurrentCompanyBranch.Company.PK;
			Factory.Save();

			AssertNotEquals(batch, submitBatch);
			AssertEquals(EInvoicingBatchState.Ready, batch.AIB_Status);
			AssertEquals(EInvoicingBatchState.Sent, submitBatch.AIB_Status);
			AssertNotEquals(batch.AIB_SystemCreateTimeUtc, submitBatch.AIB_SystemCreateTimeUtc);
			AssertNotEquals("PreCondition, test submit id will get submit batch license code.", batch.Company.GC_Code, submitBatch.Company.GC_Code);
			AssertEquals(0, Factory.Load<IXmlEDIInterchange>(new ZQuery()).Length);
			AssertEquals(0, Factory.Load<EDIMessage>(new ZQuery()).Length);

			var logger = new TestServiceLogger();
			using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser, TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartmentPK)))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				Func<IAccEInvoiceBatchToGEIConverter> converter = () => new MockAccEInvoiceBatchToGEIConverter();
				var processor = GetInterchangeProcessor(converter);
				processor.Process(logger);
			}
			AssertEquals(EInvoicingBatchState.Sent, batch.AIB_Status);
			AssertEquals(EInvoicingBatchState.Sent, submitBatch.AIB_Status);

			AssertSubmitIdBase($"{expectedRegistryId}-{submitBatch.AIB_SystemCreateTimeUtc:yyyyMMdd}-{expectedSubmitIdLastPart}");
		}

		void AssertSubmitId_SubmitInvoice(string expectedRegistryId ,string expectedSubmitIdLastPart)
		{
			var (batch, submitBatch) = GetBatchWithReadyStatus(EInvoicingPivotActionType.Submit);

			AssertEquals(batch, submitBatch);
			AssertEquals(EInvoicingBatchState.Ready, batch.AIB_Status);
			AssertEquals(0, Factory.Load<IXmlEDIInterchange>(new ZQuery()).Length);
			AssertEquals(0, Factory.Load<EDIMessage>(new ZQuery()).Length);

			var logger = new TestServiceLogger();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				Func<IAccEInvoiceBatchToGEIConverter> converter = () => new MockAccEInvoiceBatchToGEIConverter();
				var processor = GetInterchangeProcessor(converter);
				processor.Process(logger);
			}
			AssertEquals(EInvoicingBatchState.Sent, batch.AIB_Status);

			AssertSubmitIdBase($"{expectedRegistryId}-{submitBatch.AIB_SystemCreateTimeUtc:yyyyMMdd}-{expectedSubmitIdLastPart}");
		}

		void AssertSubmitIdBase(string expectedSubmitId)
		{
			var ediInterchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery());
			AssertEquals("expect 1 interchange created", 1, ediInterchanges.Length);
			EInvoicingTestHelper.AssertGEIEInvoicingEDIInterchange(ediInterchanges[0], expectedMessageType: EInvoiceAPICommandList.Codes.GenerateInvoiceRequest, expectedTo: ExpectedServicePoint);

			var ediMessages = ediInterchanges.First().LoadMessages();
			AssertEquals("expect 1 messages created", 1, ediMessages.Length);
			AssertContains("AdditionalDataItems", $@"<AdditionalDataItems>
        <AdditionalDataItem>
          <Key>SubmitID</Key>
          <Value>{expectedSubmitId}</Value>
        </AdditionalDataItem>
      </AdditionalDataItems>", ediMessages[0].EM_MessageText);
		}

		public override void TestIsBillingSupported()
		{
			var processor = new InterchangeProcessorWithIsBillingSupportedExposed();
			Assert("Billing transactions are created for E-invoicing.", processor.GetIsBillingSupported());
		}

		[TestDate(2022, 11, 08, 10, 35, 00)]
		public void TestPerformAfterCreatingEDIMessageAndInterchangeSuccessfully()
		{
			var batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, EInvoicingPivotState.Batched);

			Factory.Save();

			var logger = new TestServiceLogger();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				Func<IAccEInvoiceBatchToGEIConverter> converter = () => new MockAccEInvoiceBatchToGEIConverter();
				var processor = GetInterchangeProcessor(converter);
				processor.Process(logger);

				AssertEquals(EInvoicingBatchState.Sent, batch.AIB_Status);
				AssertEquals(new ZDate(2022, 11, 08), arInvoice.AH_ComplianceDocumentDate);
			}
		}

		public void TestErrorNotificationEmailSentWithTransactionSpecifiedMessage()
		{
			AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Helper.CreateNotificationGroup("Test User 2", "company2user@abc.com").ToGuid());

			var arInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var arInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var arInvoice3 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001002", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);

			var batch1 = GetBatchWithReadyStatus();

			TestObjectCreator.CreateEInvoicingTransactionPivot(batch1, arInvoice1, EInvoicingPivotState.Batched);
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch1, arInvoice2, EInvoicingPivotState.Batched);
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch1, arInvoice3, EInvoicingPivotState.Batched);

			Factory.Save();

			var errorMessagesSomeUnGrouped = new Dictionary<string, string[]>()
			{
				{ string.Empty, new [] { "TestBatchError." } },
				{ $"{arInvoice1.PK}", new [] { "Error1 for 00001000", "Error2 for 00001000" } },
				{ $"{arInvoice2.PK}", new [] { "Error1 for 00001001", "Error2 for 00001001" } },
				{ $"{arInvoice3.PK}", new [] { "Error1 for 00001002" } },
			};

			var expectedEmailPartsSomeUnGrouped = new []
			{
				"Error Details:\r\n</b>\r\n<div style='width:1200px; margin-left:30px;'>\r\nTestBatchError.",
				"Transaction AR INV 00001000</a><br/>\r\n<b>\r\n&nbsp;&nbsp;\r\nError Details:\r\n</b>\r\n<div style='width:1200px; margin-left:30px;'>\r\nError1 for 00001000<br/>Error2 for 00001000\r\n</div>",
				"Transaction AR INV 00001001</a><br/>\r\n<b>\r\n&nbsp;&nbsp;\r\nError Details:\r\n</b>\r\n<div style='width:1200px; margin-left:30px;'>\r\nError1 for 00001001<br/>Error2 for 00001001\r\n</div>",
				"Transaction AR INV 00001002</a><br/>\r\n<b>\r\n&nbsp;&nbsp;\r\nError Details:\r\n</b>\r\n<div style='width:1200px; margin-left:30px;'>\r\nError1 for 00001002\r\n</div>"
			};

			AssertNotificationEmail(JsonConvert.SerializeObject(errorMessagesSomeUnGrouped), false, expectedEmailPartsSomeUnGrouped, $"E-Reporting error notification for Electronic Invoicing Batch {batch1.AIB_BatchNumber} [EDI]");

			var arInvoice4 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001004", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var batch2 = GetBatchWithReadyStatus();
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch2, arInvoice4, EInvoicingPivotState.Batched);
			Factory.Save();

			var errorMessagesAllHaveGroupKey = new Dictionary<string, string[]>()
			{
				{ $"{arInvoice4.PK}", new [] { "Error1 for 00001004" } },
			};

			var expectedEmailPartsAllGrouped = new []
			{
				"Error Details:\r\n</b>\r\n<div style='width:1200px; margin-left:30px;'>\r\nFollowing invoices were failed.",
				"Transaction AR INV 00001004</a><br/>\r\n<b>\r\n&nbsp;&nbsp;\r\nError Details:\r\n</b>\r\n<div style='width:1200px; margin-left:30px;'>\r\nError1 for 00001004\r\n</div>",
			};

			AssertNotificationEmail(JsonConvert.SerializeObject(errorMessagesAllHaveGroupKey), false, expectedEmailPartsAllGrouped, $"E-Reporting error notification for Electronic Invoicing Batch {batch2.AIB_BatchNumber} [EDI]");
		}

		public void TestErrorNotificationEmailSentWithTransactionSpecifiedMessage_VlidationErrorNotLoggerWithGroupKey()
		{
			AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Helper.CreateNotificationGroup("Test User 2", "company2user@abc.com").ToGuid());

			var arInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var arInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var arInvoice3 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001002", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);

			var batch1 = GetBatchWithReadyStatus();

			TestObjectCreator.CreateEInvoicingTransactionPivot(batch1, arInvoice1, EInvoicingPivotState.Batched);
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch1, arInvoice2, EInvoicingPivotState.Batched);
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch1, arInvoice3, EInvoicingPivotState.Batched);

			Factory.Save();

			var errorMessagesNotDictionartFormated = @"TestBatchError.
Error1 for 00001000
Error2 for 00001000
Error1 for 00001001
Error2 for 00001001
Error1 for 00001002
";

			var expectedEmailParts = new[]
			{
				"Error Details:\r\n</b>\r\n<div style='width:1200px; '>",
				"\r\nTestBatchError.",
				"\r\nError1 for 00001000",
				"\r\nError2 for 00001000",
				"\r\nError1 for 00001001",
				"\r\nError2 for 00001001",
				"\r\nError1 for 00001002\r\n\r\n</div>\r\n</br>",
			};

			AssertNotificationEmail(errorMessagesNotDictionartFormated, false, expectedEmailParts, "E-Reporting error notification for Transaction AR INV ");
		}

		public void TestErrorNotificationEmailSentWithTransactionSpecifiedMessage_ContainsExceptionMessage()
		{
			AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Helper.CreateNotificationGroup("Test User 2", "company2user@abc.com").ToGuid());

			var arInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var arInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var arInvoice3 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001002", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);

			var batch1 = GetBatchWithReadyStatus();

			TestObjectCreator.CreateEInvoicingTransactionPivot(batch1, arInvoice1, EInvoicingPivotState.Batched);
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch1, arInvoice2, EInvoicingPivotState.Batched);
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch1, arInvoice3, EInvoicingPivotState.Batched);

			Factory.Save();

			var errorMessagesInDictionary = new Dictionary<string, string[]>()
			{
				{ string.Empty, new [] { "TestBatchError." } },
				{ $"{arInvoice1.PK}", new [] { "Error1 for 00001000", "Error2 for 00001000" } },
				{ $"{arInvoice2.PK}", new [] { "Error1 for 00001001", "Error2 for 00001001" } },
				{ $"{arInvoice3.PK}", new [] { "Error1 for 00001002" } },
			};

			var validaitonErrorMsg = JsonConvert.SerializeObject(errorMessagesInDictionary);
			var expectedEmailPartsWhenExceptionOccurred = new[]
			{
				"Error Details:\r\n</b>\r\n<div style='width:1200px; '>",
				$"{validaitonErrorMsg}<br/>",
				"StackTrace:",
			};

			AssertNotificationEmail(validaitonErrorMsg, true, expectedEmailPartsWhenExceptionOccurred, "E-Reporting error notification for Transaction AR INV ");
		}

		void AssertNotificationEmail(string validationErrorMsg, bool isExceptionOccurredWhenProcessing, IEnumerable<string> expectedEmailParts, string expectedEmailSubject)
		{
			var converter = GetMockErrorMessageAccEInvoiceBatchToGEIConverter(validationErrorMsg);
			var processor = GetInterchangeProcessorCore(() => converter.Object);

			if (isExceptionOccurredWhenProcessing)
			{
				processor.ExceptionOccuredWhileCreatingEDIMessageDefautValue = new Exception("Dummy Exception Message");
			}

			var logger = new TestServiceLogger();
			processor.Process(logger);

			converter.Verify(x => x.Convert(It.IsAny<AccEInvoicingBatch>()), Times.Once);

			var email = (EInvoicingTransactionErrorNotificationEmail)EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Last();
			var subject = email.GetSubject_forTest();
			var body = email.GetBody_forTest();

			AssertContains("Email Subject match.", expectedEmailSubject, subject);
			foreach (var part in expectedEmailParts)
			{
				AssertContains(part, body);
			}
		}

		public override void TestProcessPartOfBatchesFailed()
		{
			Assert("Each batch contains only one invoice, so partial failure of a batch is not possible for a Korea company", true);
		}

		#region Implementation

		protected override string CountryCode => CountryCodes.KoreaSouth;

		protected override string ExpectedServicePoint => "XHUB_KR_EINVOICING";

		public override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeProcessor(GlbCompany company)
		{
			return new StubKoreaSouthEDIInterchangeCreator(GlbCompany.CurrentCompany, CountryFactoryMock.Object, () => new MockAccEInvoiceBatchToGEIConverter());
		}

		protected override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeProcessor(
			Func<IAccEInvoiceBatchToGEIConverter> converter = null,
			ExceptionTypes? forceThisErrorWhileCreatingEDIInterchange = null,
			AccEInvoicingBatch batchWithErrors = null)
		{
			return GetInterchangeProcessorCore(converter, forceThisErrorWhileCreatingEDIInterchange, batchWithErrors);
		}

		StubKoreaSouthEDIInterchangeCreator GetInterchangeProcessorCore(
			Func<IAccEInvoiceBatchToGEIConverter> converter = null,
			ExceptionTypes? forceThisErrorWhileCreatingEDIInterchange = null,
			AccEInvoicingBatch batchWithErrors = null)
		{
			var interchangeCreator = new StubKoreaSouthEDIInterchangeCreator(GlbCompany.CurrentCompany, CountryFactoryMock.Object, converter, forceThisErrorWhileCreatingEDIInterchange);
			interchangeCreator.BatchForWhichErrorWillBeForced = batchWithErrors;
			return interchangeCreator;
		}

		(AccEInvoicingBatch Batch, AccEInvoicingBatch SubmitBatch) GetBatchWithReadyStatus(string pivotActionType)
		{
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), Business.TestObjectCreator.GetRandomString(5), TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);

			AccEInvoicingBatch submitBatch;

			AccEInvoicingBatch batch;
			if (pivotActionType != EInvoicingPivotActionType.Submit)
			{
				submitBatch = TestObjectCreator.CreateEInvoicingBatch(++batchNumberCounter, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				TestObjectCreator.CreateEInvoicingTransactionPivot(submitBatch, arInvoice, Core.Constants.EInvoicingPivotState.Batched, EInvoicingPivotActionType.Submit);
				batch = TestObjectCreator.CreateEInvoicingBatch(++batchNumberCounter, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			}
			else
			{
				batch = submitBatch = TestObjectCreator.CreateEInvoicingBatch(++batchNumberCounter, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			}

			TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Batched, pivotActionType);
			Factory.Save();

			return (batch, submitBatch);
		}

		protected override AccEInvoicingBatch GetBatchWithReadyStatus()
		{
			return GetBatchWithReadyStatus(EInvoicingPivotActionType.Submit).Batch;
		}

		int batchNumberCounter;

		protected override AccEInvoicingBatch GetBatchWithReadyStatusForSending(out string number)
		{
			var batch = TestObjectCreator.CreateEInvoicingBatch(100, EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			arInvoice.AH_ConsolidatedInvoiceRef = "STEST00001";
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, EInvoicingPivotState.Batched);
			Factory.Save();

			number = arInvoice.AH_TransactionNum;
			return batch;
		}

		class InterchangeProcessorWithIsBillingSupportedExposed : KoreaSouthEDIInterchangeCreator
		{
			public InterchangeProcessorWithIsBillingSupportedExposed()
				: base(GlbCompany.CurrentCompany, null)
			{ }

			public bool GetIsBillingSupported() => IsBillingSupported;
		}

		class StubKoreaSouthEDIInterchangeCreator : KoreaSouthEDIInterchangeCreator, IEDIIntechangeCreatorForTest
		{
			public StubKoreaSouthEDIInterchangeCreator(GlbCompany company, ICountryEInvoicingObjectFactory countryEInvoicingObjectFactory, Func<IAccEInvoiceBatchToGEIConverter> converter, ExceptionTypes? forceThisErrorWhileCreatingEDIInterchange = null)
				: base(company, countryEInvoicingObjectFactory)
			{
				Converter = converter;
				ForceThisError = forceThisErrorWhileCreatingEDIInterchange;
			}

			Func<IAccEInvoiceBatchToGEIConverter> Converter { get; }

			ExceptionTypes? ForceThisError { get; }

			public AccEInvoicingBatch BatchForWhichErrorWillBeForced { get; set; }

			protected override void CreateEDIMessageAndInterchangeCore(TransactionBatchProcessContext batchProcessContext, INotifications notifications)
			{
				base.CreateEDIMessageAndInterchangeCore(batchProcessContext, notifications);
				EInvoicingTestHelper.ForceErrorWhileCreatingEDIInterchange(batchProcessContext, notifications, ForceThisError, BatchForWhichErrorWillBeForced);
			}

			protected override TransactionBatchProcessContext GetTransactionBatchProcessContext(BusinessObjectFactory ediInterchangeCreationFactory, AccEInvoicingBatch batch, IEnumerable<ZGuid> transactionPKsReadyToSend, ILogger logger)
			{
				var result = base.GetTransactionBatchProcessContext(ediInterchangeCreationFactory, batch, transactionPKsReadyToSend, logger).As<GEIProcessContext>();
				result.ExceptionOccuredWhileCreatingEDIMessage = ExceptionOccuredWhileCreatingEDIMessageDefautValue;
				return result;
			}

			public Exception ExceptionOccuredWhileCreatingEDIMessageDefautValue { get; set; }

			protected override IAccEInvoiceBatchToGEIConverter BatchToGEIConverter => Converter();

			bool IEDIIntechangeCreatorForTest.AddErrorToEDIMessageNotesIfAny => AddErrorToEDIMessageNotesIfAny;
		}

		Mock<IAccEInvoiceBatchToGEIConverter> GetMockErrorMessageAccEInvoiceBatchToGEIConverter(string validationErrorMsg)
		{
			Mock<IAccEInvoiceBatchToGEIConverter> converter = new Mock<IAccEInvoiceBatchToGEIConverter>();

			converter.Setup(x => x.Convert(It.IsAny<AccEInvoicingBatch>())).Returns(() =>
			{
				var eInvoice = EInvoicingTestHelper.GetEInvoice();

				var validationErrors = validationErrorMsg;
				var validationWarnings = ZString.Empty;
				return (eInvoice, validationErrors, validationWarnings);
			});

			return converter;
		}

		Mock<ICountryEInvoicingObjectFactory> CountryFactoryMock
		{
			get
			{
				if (countryFactoryMock == null)
				{
					countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
					countryFactoryMock.Setup(x => x.CountryCode).Returns(CountryCode);
					countryFactoryMock.Setup(x => x.GetEInvoicingServicePoint(It.IsAny<ZString>())).Returns(ExpectedServicePoint);
					countryFactoryMock.Setup(x => x.CommunicationTransport).Returns(ExpectedCommunicationTransport);

					var globalFactoryMock = new Mock<IGlobalEInvoicingObjectFactory>();
					globalFactoryMock.Setup(x => x.GetCountryEInvoicingObjectFactorySettings(It.IsAny<ZString>())).Returns(countryFactoryMock.Object);

					ObjectFactory.Substitute(globalFactoryMock.Object);
				}

				return countryFactoryMock;
			}
		}
		Mock<ICountryEInvoicingObjectFactory> countryFactoryMock;

		#endregion
	}
}
