using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using XMLTools;
using CusTransactionNumber = Enterprise.Customs.Business.CusTransactionNumber;
using CusTransactionNumberTypeList = Enterprise.Customs.Business.CusTransactionNumberTypeList;

namespace Enterprise.Customs.IE.ServiceTasks.Testing
{
	[TestedType(typeof(TransactionIDManagerServiceTask))]
	class TransactionIDManagerServiceTaskTest : ServiceTaskTestCase<TransactionIDManagerServiceTask>
	{
		public void TestMinimumPeriod()
		{
			var hostedServiceAttribute = GetHostedServiceAttributes().Single();
			AssertEquals("MinimumPeriod", "60Seconds", hostedServiceAttribute.MinimumPeriod);
		}

		public void TestHandlingMessageThatIsOlderThanCertainNoOfDays()
		{
			using (IECustomsDataRegistry.Instance.MessageProcessingNoOfDays.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3))
			{
				var factory = new BusinessObjectFactory();
				(GlbCompany ieCompany, GlbBranch ieBranch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(factory, Core.Constants.CountryCodes.Ireland, "T1");
				InterchangeProcessorTestHelper.CreateValidCredential(ieCompany);
				var helper = new WebServiceEndPointProviderTestHelper(factory);
				var url = helper.SetupTransactionIDURL();
				var declaration = factory.New<JobDeclaration>();
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
				var entry1 = declaration.CustomsEntryHeaders.AddNew();
				var entry2 = declaration.CustomsEntryHeaders.AddNew();
				var outgoingMessage1 = factory.New<AESOutboundEDIMessage>();
				outgoingMessage1.EM_GB = ieBranch.PK;
				outgoingMessage1.EM_MessageType = AESOutgoingMessageTypeList.Codes.ArrivalAtExit;
				outgoingMessage1.EM_MessageText = "<Greeting>HELLO</Greeting>";
				outgoingMessage1.EM_Status = AESOutboundEDIMessage.Status.Queued;
				outgoingMessage1.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Receive; // to allow saving of message number
				outgoingMessage1.EM_MessageNum = "TEST1234567890";
				outgoingMessage1.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-5);
				outgoingMessage1.EM_LinkedObject = entry1;
				var outgoingMessage2 = factory.New<AESOutboundEDIMessage>();
				outgoingMessage2.EM_GB = ieBranch.PK;
				outgoingMessage2.EM_MessageType = AESOutgoingMessageTypeList.Codes.ArrivalAtExit;
				outgoingMessage2.EM_MessageText = "<Greeting>HELLO</Greeting>";
				outgoingMessage2.EM_Status = AESOutboundEDIMessage.Status.Queued;
				outgoingMessage2.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Receive; // to allow saving of message number
				outgoingMessage2.EM_MessageNum = "TEST1234567891";
				outgoingMessage2.EM_LinkedObject = entry2;
				const string transactionID1 = "A02A8604-2D0B-4FEE-8EDF-61DB6AFC7639";
				const string transactionID2 = "C02A8604-2D0B-4FEE-8EDF-61DB6AFC7639";
				var transactionNumber1 = factory.New<CusTransactionNumber>();
				transactionNumber1.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
				transactionNumber1.TN_GC_Company = ieCompany.PK;
				transactionNumber1.TN_TransactionReference = transactionID1;
				var transactionNumber2 = factory.New<CusTransactionNumber>();
				transactionNumber2.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
				transactionNumber2.TN_GC_Company = ieCompany.PK;
				transactionNumber2.TN_TransactionReference = transactionID2;
				factory.Save();
				outgoingMessage1.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Transmit;
				outgoingMessage2.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Transmit;
				factory.Save();
				var task = new TransactionIDManagerServiceTask();
				InitialiseTaskSchedule(task);
				RunTaskSchedule(task);
				CombineAssertions(() =>
				{
					var anotherFactory = new BusinessObjectFactory();
					AssertEDIMessage("outgoingMessage1", (AESOutboundEDIMessage)anotherFactory.Load<Enterprise.Messaging.Business.EDIMessage>(outgoingMessage1.PK), AESOutboundEDIMessage.Status.Discarded, ZString.Empty);
					AssertEDIMessage("outgoingMessage2", (AESOutboundEDIMessage)anotherFactory.Load<Enterprise.Messaging.Business.EDIMessage>(outgoingMessage2.PK), AESOutboundEDIMessage.Status.Pending, transactionID1);
					AssertEquals("entry1.CH_Status", LogicalStatusList.Codes.Failed, anotherFactory.Load<CusEntryHeader>(entry1.PK).CH_Status);
					AssertEquals("entry2.CH_Status", ZString.Empty, anotherFactory.Load<CusEntryHeader>(entry2.PK).CH_Status);
					AssertContains("Logs", "Information|TEST T1 COMP: Found 1 message(s) waiting for Transaction ID allocation\r\nInformation|Allocating Transaction ID 'A02A8604-2D0B-4FEE-8EDF-61DB6AFC7639' to Message #TEST1234567891", task.ServiceLogger.ToString());
				});
			}
		}

		public void TestHandlingInvalidCredential()
		{
			using (IECustomsDataRegistry.Instance.MessageProcessingNoOfDays.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3))
			{
				var factory = new BusinessObjectFactory();
				(GlbCompany ieCompany, GlbBranch ieBranch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(factory, Core.Constants.CountryCodes.Ireland, "T1");
				var credential = InterchangeProcessorTestHelper.CreateValidCredential(ieCompany);
				credential.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
				var helper = new WebServiceEndPointProviderTestHelper(factory);
				var url = helper.SetupTransactionIDURL();
				var declaration = factory.New<JobDeclaration>();
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
				var entry = declaration.CustomsEntryHeaders.AddNew();
				var outgoingMessage = factory.New<AESOutboundEDIMessage>();
				outgoingMessage.EM_GB = ieBranch.PK;
				outgoingMessage.EM_MessageType = AESOutgoingMessageTypeList.Codes.ArrivalAtExit;
				outgoingMessage.EM_MessageText = "<Greeting>HELLO</Greeting>";
				outgoingMessage.EM_Status = AESOutboundEDIMessage.Status.Queued;
				outgoingMessage.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Receive; // to allow saving of message number
				outgoingMessage.EM_MessageNum = "TEST1234567890";
				outgoingMessage.EM_LinkedObject = entry;
				const string transactionID1 = "A02A8604-2D0B-4FEE-8EDF-61DB6AFC7639";
				var transactionNumber1 = factory.New<CusTransactionNumber>();
				transactionNumber1.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
				transactionNumber1.TN_GC_Company = ieCompany.PK;
				transactionNumber1.TN_TransactionReference = transactionID1;
				factory.Save();
				outgoingMessage.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Transmit;
				factory.Save();
				var task = new TransactionIDManagerServiceTask();
				InitialiseTaskSchedule(task);
				RunTaskSchedule(task);
				CombineAssertions(() =>
				{
					var anotherFactory = new BusinessObjectFactory();
					AssertEDIMessage("outgoingMessage", (AESOutboundEDIMessage)anotherFactory.Load<Enterprise.Messaging.Business.EDIMessage>(outgoingMessage.PK), AESOutboundEDIMessage.Status.Discarded, ZString.Empty);
					AssertEquals("entry.CH_Status", LogicalStatusList.Codes.Failed, anotherFactory.Load<CusEntryHeader>(entry.PK).CH_Status);
					AssertEquals("Logs", "", task.ServiceLogger.ToString());
				});
			}
		}

		public void TestProcessAllocateTransactionID()
		{
			var factory = new BusinessObjectFactory();
			(GlbCompany ieCompany1, GlbBranch ieBranch1) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(factory, Core.Constants.CountryCodes.Ireland, "T1");
			InterchangeProcessorTestHelper.CreateValidCredential(ieCompany1);
			(GlbCompany ieCompany2, GlbBranch ieBranch2) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(factory, Core.Constants.CountryCodes.Ireland, "T2");
			InterchangeProcessorTestHelper.CreateValidCredential(ieCompany2);
			var helper = new WebServiceEndPointProviderTestHelper(factory);
			var url = helper.SetupTransactionIDURL();
			var outgoingMessage1 = factory.New<AESOutboundEDIMessage>();
			outgoingMessage1.EM_GB = ieBranch1.PK;
			outgoingMessage1.EM_MessageType = AESOutgoingMessageTypeList.Codes.ArrivalAtExit;
			outgoingMessage1.EM_MessageText = "<Greeting>HELLO</Greeting>";
			outgoingMessage1.EM_Status = AESOutboundEDIMessage.Status.Queued;
			outgoingMessage1.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Receive; // to allow saving of message number
			outgoingMessage1.EM_MessageNum = "TEST1234567890";
			var outgoingMessage2 = factory.New<AESOutboundEDIMessage>();
			outgoingMessage2.EM_GB = ieBranch1.PK;
			outgoingMessage2.EM_MessageType = AESOutgoingMessageTypeList.Codes.ArrivalAtExit;
			outgoingMessage2.EM_MessageText = "<Greeting>HELLO</Greeting>";
			outgoingMessage2.EM_Status = AESOutboundEDIMessage.Status.Queued;
			outgoingMessage2.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Receive; // to allow saving of message number
			outgoingMessage2.EM_MessageNum = "TEST1234567891";
			var outgoingMessage3 = factory.New<AESOutboundEDIMessage>();
			outgoingMessage3.EM_GB = ieBranch1.PK;
			outgoingMessage3.EM_MessageType = AESOutgoingMessageTypeList.Codes.ArrivalAtExit;
			outgoingMessage3.EM_MessageText = "<Greeting>HELLO</Greeting>";
			outgoingMessage3.EM_Status = AESOutboundEDIMessage.Status.Queued;
			outgoingMessage3.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Receive; // to allow saving of message number
			outgoingMessage3.EM_MessageNum = "TEST1234567892";
			const string transactionID1 = "A02A8604-2D0B-4FEE-8EDF-61DB6AFC7639";
			const string transactionID2 = "C02A8604-2D0B-4FEE-8EDF-61DB6AFC7639";
			const string transactionID3 = "B02A8604-2D0B-4FEE-8EDF-61DB6AFC7639";
			var transactionNumber1 = factory.New<CusTransactionNumber>();
			transactionNumber1.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
			transactionNumber1.TN_GC_Company = ieCompany1.PK;
			transactionNumber1.TN_TransactionReference = transactionID1;
			var transactionNumber2 = factory.New<CusTransactionNumber>();
			transactionNumber2.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
			transactionNumber2.TN_GC_Company = ieCompany1.PK;
			transactionNumber2.TN_TransactionReference = transactionID2;
			var transactionNumber3 = factory.New<CusTransactionNumber>();
			transactionNumber3.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
			transactionNumber3.TN_GC_Company = ieCompany2.PK;
			transactionNumber3.TN_TransactionReference = transactionID3;
			factory.Save();
			outgoingMessage1.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Transmit;
			outgoingMessage2.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Transmit;
			outgoingMessage3.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Transmit;
			factory.Save();
			var task = new TransactionIDManagerServiceTask();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);
			CombineAssertions(() =>
			{
				var anotherFactory = new BusinessObjectFactory();
				AssertEDIMessage("outgoingMessage1", (AESOutboundEDIMessage)anotherFactory.Load<Enterprise.Messaging.Business.EDIMessage>(outgoingMessage1.PK), AESOutboundEDIMessage.Status.Pending, transactionID1);
				AssertEDIMessage("outgoingMessage2", (AESOutboundEDIMessage)anotherFactory.Load<Enterprise.Messaging.Business.EDIMessage>(outgoingMessage2.PK), AESOutboundEDIMessage.Status.Pending, transactionID2);
				AssertEDIMessage("outgoingMessage3", (AESOutboundEDIMessage)anotherFactory.Load<Enterprise.Messaging.Business.EDIMessage>(outgoingMessage3.PK), AESOutboundEDIMessage.Status.Queued, ZString.Empty);
				var interchanges = anotherFactory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.IECustomsCommon)
					.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, CommonInterchangeTypeList.Codes.TransactionID)
					.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit)
					.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.Queued)
					.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT));
				AssertEquals("interchanges.Length", 1, interchanges.Length);
				var interchange = interchanges[0];
				var trackingReference = interchange.EI_SessionGUID.ToString();
				AssertEquals("interchange.EI_HeaderText", $@"{{""custom.IE.Endpoint"":""{url}""}}", interchange.EI_HeaderText);
				var xmlComparer = new XmlComparer(XmlComparer.XmlCompareOptions.IgnoreNamespaces);
				var res = xmlComparer.CompareXml(@"<q1:TransactionIDRequest xmlns:q1=""http://www.ros.ie/schemas/customs/transactionidrequest/v1"">
  <q1:Transactions>
    <q1:NumberOfTxIds>100</q1:NumberOfTxIds>
  </q1:Transactions>
</q1:TransactionIDRequest>", interchange.EI_BodyText);
				Assert("interchange.EI_BodyText", res.result);
				AssertEquals("interchange.EI_GB", ieBranch1.PK, interchange.EI_GB);
				var blankTransactionNumbers = anotherFactory.Load<CusTransactionNumber>(new ZQuery(CusTransactionNumberSchema.TN_Type, CusTransactionNumberTypeList.Codes.IECustoms)
					.AddToFilter(CusTransactionNumberSchema.TN_IsUsed, ZBool.False)
					.AddToFilter(CusTransactionNumberSchema.TN_TransactionReference, ZString.Empty));
				AssertEquals("blankTransactionNumbers.Length", 100, blankTransactionNumbers.Length);
				AssertEquals("All blankTransactionNumbers", true, blankTransactionNumbers.All(x => x.TN_TrackingReference == trackingReference && x.TN_GC_Company == ieCompany1.PK));
				AssertEquals("transactionNumber1.TN_IsUsed", ZBool.True, anotherFactory.Load<CusTransactionNumber>(transactionNumber1.PK).TN_IsUsed);
				AssertEquals("transactionNumber2.TN_IsUsed", ZBool.True, anotherFactory.Load<CusTransactionNumber>(transactionNumber2.PK).TN_IsUsed);
				AssertEquals("transactionNumber3.TN_IsUsed", ZBool.False, anotherFactory.Load<CusTransactionNumber>(transactionNumber3.PK).TN_IsUsed);
				AssertContains("Logs", "Information|TEST T1 COMP: 1 TID message(s) created\r\nInformation|TEST T1 COMP: Found 3 message(s) waiting for Transaction ID allocation\r\nInformation|Allocating Transaction ID 'A02A8604-2D0B-4FEE-8EDF-61DB6AFC7639' to Message #TEST1234567890\r\nInformation|Allocating Transaction ID 'C02A8604-2D0B-4FEE-8EDF-61DB6AFC7639' to Message #TEST1234567891", task.ServiceLogger.ToString());
			});
		}

		void AssertEDIMessage(string reference, Enterprise.Messaging.Business.EDIMessage message, string expectedStatus, string expectedApplicationReference)
		{
			AssertEquals(reference + ".EM_Status", expectedStatus, message.EM_Status);
			AssertEquals(reference + ".EM_ApplicationReference", expectedApplicationReference, message.EM_ApplicationReference);
		}

		public void TestServiceAttribute()
		{
			AssertSingleHostedServiceAttribute("IET", "IE Customs Transaction ID Manager", "IEC");
		}

		public void TestHostedServiceRequirement()
		{
			var methodInfo = typeof(TransactionIDManagerServiceTask).GetMethod(nameof(TransactionIDManagerServiceTask.IsRequired));
			Assert("HostedServiceRequirement is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => new[]
		{
			new TaskNudgeInformationForTest(
				table: EDIMessageSchema.Constants.TableName,
				queueName: "IE Customs Export TransactionID",
				EDIMessageSchema.Constants.EM_Status + "=QUE",
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=TRX",
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_ApplicationReference + "=",
				EDIMessageSchema.Constants.EM_ApplicationCode + "=IEE"
			),
			new TaskNudgeInformationForTest(
				table: EDIMessageSchema.Constants.TableName,
				queueName: "IE Customs Import TransactionID",
				EDIMessageSchema.Constants.EM_Status + "=QUE",
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=TRX",
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_ApplicationReference + "=",
				EDIMessageSchema.Constants.EM_ApplicationCode + "=IEI"
			),
			new TaskNudgeInformationForTest(
				table: EDIMessageSchema.Constants.TableName,
				queueName: "IE Customs Import UCC5 TransactionID",
				EDIMessageSchema.Constants.EM_Status + "=QUE",
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=TRX",
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_ApplicationReference + "=",
				EDIMessageSchema.Constants.EM_ApplicationCode + "=IE5"
			),
			new TaskNudgeInformationForTest(
				table: EDIInterchangeSchema.Constants.TableName,
				queueName: "IE Customs TransactionID Request",
				EDIInterchangeSchema.Constants.EI_Status + "=QUE",
				EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=RCV",
				EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
				EDIInterchangeSchema.Constants.EI_ApplicationCode + "=IEC",
				EDIInterchangeSchema.Constants.EI_TransportType + "=XTT",
				EDIInterchangeSchema.Constants.EI_InterchangeType + "=TID"
			),
			new TaskNudgeInformationForTest(
				table: EDIMessageSchema.Constants.TableName,
				queueName: "IE Customs NCTS TransactionID",
				EDIMessageSchema.Constants.EM_Status + "=QUE",
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=TRX",
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_ApplicationReference + "=",
				EDIMessageSchema.Constants.EM_ApplicationCode + "=IEN"
			)
		};
	}
}
