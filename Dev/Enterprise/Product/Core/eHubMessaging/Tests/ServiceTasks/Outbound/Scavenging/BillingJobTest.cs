using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.eHubMessaging.ServiceTasks.Outbound.Scavenging.Billing;
using Enterprise.eHubMessaging.Tests.EndToEndMessageProcessing.Mocks;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.Outbound.Scavenging
{
	[TestedType(typeof(BillingJob))]
	class BillingJobTest : ScavengingSubmissionJobTest<BillingJob, StmUsageData, LightweightOutboundBillingItem>
	{
		public void TestJobName()
		{
			AssertEquals("Billing", GetPropertyValue(CreateMockJob_Moq().Object, "JobName"));
		}

		[UseSnapshotProtection]
		public void TestExecuteValidationExceptionProductionSystemHasErrorReport()
		{
			ExceptionReporter.Instance.TestingDoReportException.Value = true;
			using (MockProductionSystem())
			{
				AssertExecuteValidationException(ValidationErrorExceptionString);
				AssertEquals("BillingTransaction.ExternalValidationError: TST/TST - Category, ClientID, Reference1, Version", ErrorReporter.LastKeyReported);
				AssertEquals(@"BILLING DATA
----------
" + ValidationErrorExceptionString, ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestExecuteValidationExceptionTestSystemNoErrorReport()
		{
			AssertExecuteValidationException(ValidationErrorExceptionString);
		}

		[UseSnapshotProtection]
		public void TestExecuteValidationExceptionProductionSystemHasErrorReportForUsage()
		{
			ExceptionReporter.Instance.TestingDoReportException.Value = true;
			using (MockProductionSystem())
			{
				AssertExecuteValidationException(ValidationErrorExceptionStringForUsage);
				AssertEquals("BillingTransaction.ExternalValidationError: TST/TST - Category, ClientID, Reference1, Version", ErrorReporter.LastKeyReported);
				AssertEquals(@"BILLING DATA
----------
" + ValidationErrorExceptionStringForUsage, ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestExecuteValidationExceptionTestSystemNoErrorReportForUsage()
		{
			AssertExecuteValidationException(ValidationErrorExceptionStringForUsage);
		}

		public void AssertExecuteValidationException(string validationErrorExceptionString)
		{
			var stmUsageData = SetupForExecuteTest().FullItem;

			var list = new List<IeHubMessage>();
			var mockOutbox = new Mock<IMessageOutbox>(MockBehavior.Strict);
			mockOutbox
				.Setup(x => x.AddMessage(It.IsAny<IeHubMessage>()))
				.Callback(new Action<IeHubMessage>(list.Add));
			mockOutbox.Setup(m => m.Count).Returns(1);
			mockOutbox.Setup(m => m.Clear());

			var mockAdapter = new Mock<IeHubAdapter>(MockBehavior.Strict);
			mockAdapter.Setup(m => m.Outbox).Returns(mockOutbox.Object);
			mockAdapter.Setup(x => x.SendMessages()).Callback(new Action(() =>
			{
				throw new eHubAdapterException(
					string.Format("1 errors occured during processing send request:{0}{0}{1}", System.Environment.NewLine, validationErrorExceptionString),
					new CargoWise.eHub.Common.SerializableDictionary<Guid, string> { { list[0].TrackingID, validationErrorExceptionString } });
			}));
			mockAdapter.Setup(x => x.Dispose());
			var mockJob = CreateMockJob_Moq(mockInterchangeCandidates: false, adaptorFactory: new AdaptorFactoryMockWithOneAdaptor(mockAdapter.Object));
			mockJob.Object.Execute(CancellationToken.None);
			Assert("Next execute iteration scheduled", mockJob.Object.NextExecuteIterationIsScheduled);
			Assert("StmUsageData is not deleted", !stmUsageData.IsDeleted);
			Assert("StmUsageData is marked as failed", stmUsageData.SUD_Fail);

			mockOutbox.VerifyAll();
			mockAdapter.VerifyAll();
			mockJob.VerifyAll();
		}

		const string ValidationErrorExceptionString = @"CargoWise.eHub.Gateway.BillingTransactionValidationException: Billing transaction validation failed:
The element 'BillingTransaction' in namespace 'http://www.edi.com.au/EnterpriseService/#Billing_1.4' has incomplete content. List of possible elements expected: 'Version, Category' in namespace 'http://www.edi.com.au/EnterpriseService/#Billing_1.4'.
The 'http://www.edi.com.au/EnterpriseService/#Billing_1.4:Reference1' element is invalid - The value '' is invalid according to its datatype 'String' - The actual length is less than the MinLength value.
The 'ClientID' element is invalid - The value '' is invalid according to its datatype 'String' - The actual length is not equal to the specified length.
   at CallStack...";

		const string ValidationErrorExceptionStringForUsage = @"CargoWise.eHub.Gateway.BillingTransactionValidationException: Usage transaction validation failed:
The element 'BillingTransaction' in namespace 'http://www.edi.com.au/EnterpriseService/#Billing_1.4' has incomplete content. List of possible elements expected: 'Version, Category' in namespace 'http://www.edi.com.au/EnterpriseService/#Billing_1.4'.
The 'http://www.edi.com.au/EnterpriseService/#Billing_1.4:Reference1' element is invalid - The value '' is invalid according to its datatype 'String' - The actual length is less than the MinLength value.
The 'ClientID' element is invalid - The value '' is invalid according to its datatype 'String' - The actual length is not equal to the specified length.
   at CallStack...";

		[UseSnapshotProtection]
		public void TestExecuteValidationException_MultipleBatchWhenExceedingGatewayLimit()
		{
			ExceptionReporter.Instance.TestingDoReportException.Value = true;
			using (MockProductionSystem())
			{
				var billingItems = new List<StmUsageData>();
				for (int i = 0; i < 15; i++)
				{
					var encoding = new UTF8Encoding(false);
					var stmUsageData = Factory.New<StmUsageData>();
					stmUsageData.SUD_Data = BillingDataEncryptor.Encrypt(encoding.GetBytes("T"));
					stmUsageData.SUD_Category = "TST";
					stmUsageData.SUD_Code = "TST";
					billingItems.Add(stmUsageData);
				}
				Factory.Save();

				const string validationErrorExceptionString = @"CargoWise.eHub.Gateway.BillingTransactionValidationException: Billing transaction validation failed:
The element 'BillingTransaction' in namespace 'http://www.edi.com.au/EnterpriseService/#Billing_1.4' has incomplete content. List of possible elements expected: 'Version, Category' in namespace 'http://www.edi.com.au/EnterpriseService/#Billing_1.4'.
The 'http://www.edi.com.au/EnterpriseService/#Billing_1.4:Reference1' element is invalid - The value '' is invalid according to its datatype 'String' - The actual length is less than the MinLength value.
The 'ClientID' element is invalid - The value '' is invalid according to its datatype 'String' - The actual length is not equal to the specified length.
   at CallStack...";
				var mockAdapter = new Mock<EHubAdapterMock>(MockBehavior.Strict);
				var numberOfCalls = 0;
				mockAdapter.Setup(x => x.SendMessages())
					.Callback(new Action(() =>
					{
						numberOfCalls++;
						if (numberOfCalls == 1)
						{
							AssertEquals(10, mockAdapter.Object.Outbox.Count);
							var failedTrackingId = new Guid(((MessageOutboxMock)mockAdapter.Object.Outbox).MessageList[4].TrackingID.ToString());
							throw new eHubAdapterException(
								string.Format("1 errors occured during processing send request:{0}{0}{1}", System.Environment.NewLine, validationErrorExceptionString),
								new CargoWise.eHub.Common.SerializableDictionary<Guid, string> { { failedTrackingId, validationErrorExceptionString } });
						} else if (numberOfCalls == 2)
						{
							AssertEquals(5, mockAdapter.Object.Outbox.Count);
						}
					}));

				var mockJob = CreateMockJob_Moq(mockInterchangeCandidates: false, adaptorFactory: new GatewayAdaptorFactoryMockWithOneAdaptor(mockAdapter.Object));
				mockJob.Setup(x => x.AdapterOutboxCountLimit).Returns(20);
				mockJob.Setup(x => x.GatewayMaxReceivedMessageLimitInBytes).Returns(10);
				mockJob.Object.Execute(CancellationToken.None);
				Assert("Next execute iteration scheduled", mockJob.Object.NextExecuteIterationIsScheduled);
				Assert("StmUsageData is not deleted", !billingItems[4].IsDeleted);
				Assert("StmUsageData is marked as failed", billingItems[4].SUD_Fail);
				AssertEquals("BillingTransaction.ExternalValidationError: TST/TST - Category, ClientID, Reference1, Version", ErrorReporter.LastKeyReported);
				AssertEquals(@"T
----------
" + validationErrorExceptionString, ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();

				mockAdapter.VerifyAll();
				mockJob.Verify();
			}
		}

		public void TestScavengingSendMessages_MultipleBatchWhenExceedingGatewayLimit()
		{
			for (int i = 0; i < 15; i++)
			{
				var encoding = new UTF8Encoding(false);
				var stmUsageData = Factory.New<StmUsageData>();
				stmUsageData.SUD_Data = BillingDataEncryptor.Encrypt(encoding.GetBytes("T"));
				stmUsageData.SUD_Category = "TST";
				stmUsageData.SUD_Code = "TST";
				Factory.Save();
			}

			var adapterMock = new Mock<EHubAdapterMock>() { CallBase = true };
			var numberOfCalls = 0;
			adapterMock.Setup(x => x.SendMessages()).Callback(() =>
			{
				numberOfCalls++;
				if (numberOfCalls == 1)
				{
					AssertEquals(10, adapterMock.Object.Outbox.Count);
				}
				else
				{
					AssertEquals(5, adapterMock.Object.Outbox.Count);
				}
			});

			var serviceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false, adaptorFactory: new GatewayAdaptorFactoryMockWithOneAdaptor(adapterMock.Object));
			serviceTaskJob.Setup(x => x.AdapterOutboxCountLimit).Returns(20);
			serviceTaskJob.Setup(x => x.GatewayMaxReceivedMessageLimitInBytes).Returns(10);
			serviceTaskJob.Object.Execute(CancellationToken.None);
			adapterMock.VerifyAll();
		}

		[UseSnapshotProtection]
		public void TestScavengingSendMessages_OneOutgoingMessageSizeOverGatewayMaxReceiveSize()
		{
			ExceptionReporter.Instance.TestingDoReportException.Value = true;
			using (MockProductionSystem())
			{
				var billingItems = new List<StmUsageData>();
				for (int i = 0; i < 15; i++)
				{
					var encoding = new UTF8Encoding(false);
					var stmUsageData = Factory.New<StmUsageData>();
					stmUsageData.SUD_Category = "TST";
					stmUsageData.SUD_Code = "TST";
					stmUsageData.SUD_Data = BillingDataEncryptor.Encrypt(i == 10 ? encoding.GetBytes(new string('T', 11)) : encoding.GetBytes("T"));
					billingItems.Add(stmUsageData);

					Factory.Save();
				}

				var adapterMock = new Mock<EHubAdapterMock>() { CallBase = true };
				var numberOfCalls = 0;
				adapterMock.Setup(x => x.SendMessages()).Callback(() =>
				{
					numberOfCalls++;
					if (numberOfCalls == 1)
					{
						AssertEquals(10, adapterMock.Object.Outbox.Count);
					} else if (numberOfCalls == 2)
					{
						AssertEquals(1, adapterMock.Object.Outbox.Count);
						throw new eHubAdapterException("The maximum message size quota for incoming messages (10) has been exceeded");
					} else if (numberOfCalls == 3)
					{
						AssertEquals(4, adapterMock.Object.Outbox.Count);
					}
				});

				var mockSericeTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false, adaptorFactory: new GatewayAdaptorFactoryMockWithOneAdaptor(adapterMock.Object));
				mockSericeTaskJob.Setup(x => x.AdapterOutboxCountLimit).Returns(20);
				mockSericeTaskJob.Setup(x => x.GatewayMaxReceivedMessageLimitInBytes).Returns(10);
				mockSericeTaskJob.Object.Execute(CancellationToken.None);
				Assert("StmUsageData is not deleted", !billingItems[10].IsDeleted);
				Assert("StmUsageData is marked as failed", billingItems[10].SUD_Fail);
				AssertEquals("BillingTransaction.ExternalValidationError: TST/TST - Unknown problem", ErrorReporter.LastKeyReported);
				AssertEquals($@"TTTTTTTTTTT
----------
StmUsageData {billingItems[10].PK.ToString()} prepared outgoing message larger than the maximum receive size for the outbound service (10 bytes).", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
				adapterMock.VerifyAll();
				mockSericeTaskJob.Verify();
			}
		}

		public void TestHighPriorityUsagesAreSubmittedFirst()
		{
			var utcTime = ZDateTime.UtcNow;
			var encoding = new UTF8Encoding(false);
			for (int i = 0; i < 5; i++)
			{
				var stmUsageData = Factory.New<StmUsageData>();
				stmUsageData.SUD_Data = BillingDataEncryptor.Encrypt(encoding.GetBytes("TS" + i.ToString()));
				stmUsageData.SUD_Category = "TST";
				stmUsageData.SUD_Code = "TST";
				stmUsageData.SUD_SubmissionPriority = (ZByte)(4 - i);
				stmUsageData.SUD_PostedTimeUtc = utcTime.AddSeconds(i);
				Factory.Save();
			}

			var adapterMock = new Mock<EHubAdapterMock>() { CallBase = true };
			adapterMock.Setup(m => m.SendMessages())
				.Callback(() =>
				{
					var mockOutbox = (MessageOutboxMock)adapterMock.Object.Outbox;
					AssertEquals(5, mockOutbox.Count);
					for (int i = 0; i < 5; i++)
					{
						var expectedData = "TS" + (4 - i).ToString();
						var actualData = encoding.GetString(mockOutbox.MessageList[i].MessageStream.ReadFully());
						AssertEquals("Records were not sent in priority order", expectedData, actualData);
					}
				});

			var serviceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false, adaptorFactory: new AdaptorFactoryMockWithOneAdaptor(adapterMock.Object));
			serviceTaskJob.Object.Execute(CancellationToken.None);
			adapterMock.VerifyAll();
			serviceTaskJob.VerifyAll();
		}

		public void TestSchemaNameForUsageTransaction()
		{
			var encoding = new UTF8Encoding(false);
			var stmUsageData = Factory.New<StmUsageData>();
			stmUsageData.SUD_Data = BillingDataEncryptor.Encrypt(encoding.GetBytes("Usage Transaction"));
			stmUsageData.SUD_Category = "USG";
			stmUsageData.SUD_Code = "USG";
			stmUsageData.SUD_PostedTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			var adapterMock = new Mock<EHubAdapterMock>() { CallBase = true };
			adapterMock.Setup(m => m.SendMessages())
				.Callback(() =>
				{
					var mockOutbox = (MessageOutboxMock)adapterMock.Object.Outbox;
					AssertEquals(1, mockOutbox.Count);
					var mockMessage = mockOutbox.MessageList.Single();
					var actualData = encoding.GetString(mockMessage.MessageStream.ReadFully());
					AssertEquals("Wrong data", "Usage Transaction", actualData);
					AssertEquals("Wrong schema name", "http://www.edi.com.au/EnterpriseService/#Usage_2.1", mockMessage.SchemaName);
				});

			var serviceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false, adaptorFactory: new AdaptorFactoryMockWithOneAdaptor(adapterMock.Object));
			serviceTaskJob.Object.Execute(CancellationToken.None);
			adapterMock.VerifyAll();
			serviceTaskJob.VerifyAll();
		}

		public override void TesteHubMessage()
		{
			var stmUsageData = Factory.New<StmUsageData>();
			stmUsageData.SUD_Category = "STL";
			stmUsageData.SUD_Code = "TST";

			var job = CreateMockJob_Moq();
			job.Object.CurrentCompany = GlbCompany.CurrentCompany;
			var message = (eHubMessage)CallMethod(job.Object, "CreateMessage", null, new LightweightOutboundBillingItem(stmUsageData));
			AssertEquals("SCV", message.ApplicationCode);
			AssertEquals(ExpectedSchemaName, message.SchemaName);
		}

		protected override string ExpectedSchemaName
		{
			get { return BillingManager.CurrentSchema; }
		}

		protected override LightweightOutboundBillingItem SetupForExecuteTest()
		{
			var encoding = new UTF8Encoding(false);
			var stmUsageData = Factory.New<StmUsageData>();
			stmUsageData.SUD_Data = BillingDataEncryptor.Encrypt(encoding.GetBytes("BILLING DATA"));
			stmUsageData.SUD_Category = "TST";
			stmUsageData.SUD_Code = "TST";
			Factory.Save();
			return new LightweightOutboundBillingItem(stmUsageData);
		}

		protected override void AssertEHubMessageForItem(IeHubMessage message, LightweightOutboundBillingItem item)
		{
			CombineAssertions(() =>
			{
				AssertEquals("ApplicationCode", "SCV", message.ApplicationCode);
				AssertEquals("RecipientID", "eHubASService", message.RecipientID);
				AssertEquals("SchemaName", EDIMessageSchemaNameList.Descriptions.Billing, message.SchemaName);
				AssertEquals("SchemaType", MessageSchemaType.Xml, message.SchemaType);
				AssertEquals("SenderID", GlbCompany.GetCurrentCompany(Factory).LicenceKeyIdentifier, message.SenderID);
				AssertEquals("Message contents", item.BillingData, ReadAllData(message.MessageStream));
			});
		}

		protected override string ScavengingItemsTableName
		{
			get { return StmUsageDataSchema.Constants.TableName; }
		}

		protected override string ScavengingItemsPKName
		{
			get { return StmUsageDataSchema.Constants.PK; }
		}

		protected override void AdditionalServiceTaskJobSetup_Moq(Mock<BillingJob> mockServiceTaskJob, GlbCompany company, bool mockInterchangeCandidates = true)
		{
			base.AdditionalServiceTaskJobSetup_Moq(mockServiceTaskJob, company, mockInterchangeCandidates);
			if (mockInterchangeCandidates)
			{
				new BillingManager().AddTransactions(new List<BillingTransaction>
				{
					new BillingTransaction
					{
						BillableCount = 1,
						ClientID = "TCLIENTID",
						Category = "TST",
						PriceItemCode = "TST",
						Reference1 = "REF1",
						ReportingSource = "TST",
						ServiceOccuredUTC = DateTime.UtcNow
					}
				}, Db.Connection);
			}
		}

		static byte[] ReadAllData(Stream stream)
		{
			using (var memoryStream = new MemoryStream())
			{
				stream.CopyTo(memoryStream);
				return memoryStream.ToArray();
			}
		}

		protected IDisposable MockProductionSystem()
		{
			var keyMock = new Mock<IProductRegistrationKey>();
			var productRegistrationMock = new Mock<IProductRegistration>();
			productRegistrationMock.Setup(m => m.IsWiseTechGlobalInternalSystem()).Returns(false);
			productRegistrationMock.Setup(m => m.Key).Returns(keyMock.Object);
			keyMock.Setup(m => m.DatabaseType).Returns(DatabaseTypes.Codes.Production);
			keyMock.Setup(m => m.EnterpriseCode).Returns("BLA");
			keyMock.Setup(m => m.ServerCode).Returns("BLA");

			return new DisposableList(new[] { ObjectFactory.Substitute(productRegistrationMock.Object), Globals.TemporaryOverrideForIsTest(false), Globals.TemporaryOverrideForIsDebugMode(false), Globals.SetIsUserInteractiveForTest(false) });
		}
	}
}
