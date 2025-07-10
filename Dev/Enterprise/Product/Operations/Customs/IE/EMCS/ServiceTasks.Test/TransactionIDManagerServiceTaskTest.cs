using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.EMCS.Business;
using Enterprise.Customs.IE.EMCS.Messaging;
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
using EDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.IE.EMCS.ServiceTasks.Testing
{
	[TestedType(typeof(TransactionIDManagerServiceTask))]
	class TransactionIDManagerServiceTaskTest : ServiceTaskTestCase<TransactionIDManagerServiceTask>
	{
		public void TestProcessAllocateTransactionID()
		{
			var factory = new BusinessObjectFactory();
			(GlbCompany ieCompany1, GlbBranch ieBranch1) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(factory, Core.Constants.CountryCodes.Ireland, "T1");
			var company1Wrapper = IE.Business.GlbCompanyWrapper.Get(ieCompany1);
			var emcsCredential1 = company1Wrapper.EMCSGlbExternalPasswordCollection.AddNew();
			emcsCredential1.CurrentDecryptedCertificatePassphrase = ROSCertificateTestHelper.ValidPassword;
			emcsCredential1.GP_Certificate = ROSCertificateTestHelper.ValidCertificate;
			(GlbCompany ieCompany2, GlbBranch ieBranch2) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(factory, Core.Constants.CountryCodes.Ireland, "T2");
			var company2Wrapper = IE.Business.GlbCompanyWrapper.Get(ieCompany2);
			var emcsCredential2 = company2Wrapper.EMCSGlbExternalPasswordCollection.AddNew();
			emcsCredential2.CurrentDecryptedCertificatePassphrase = ROSCertificateTestHelper.ValidPassword;
			emcsCredential2.GP_Certificate = ROSCertificateTestHelper.ValidCertificate;

			var helper = new WebServiceEndPointProviderTestHelper(factory);
			var url = helper.SetupTransactionIDURL(true);
			var outgoingMessage1 = factory.New<EMCSOutboundEDIMessage>();
			outgoingMessage1.EM_GB = ieBranch1.PK;
			outgoingMessage1.EM_GP = emcsCredential1.PK;
			outgoingMessage1.EM_MessageType = EMCSOutgoingMessageTypeList.Codes.SubmitDraftEAD;
			outgoingMessage1.EM_MessageText = "<Greeting>HELLO</Greeting>";
			outgoingMessage1.EM_Status = EMCSOutboundEDIMessage.Status.Queued;
			outgoingMessage1.EM_ReceiveTransmit = EMCSOutboundEDIMessage.Direction.Receive; // to allow saving of message number
			outgoingMessage1.EM_MessageNum = "TEST1234567890";
			var outgoingMessage2 = factory.New<EMCSOutboundEDIMessage>();
			outgoingMessage2.EM_GB = ieBranch1.PK;
			outgoingMessage2.EM_GP = emcsCredential1.PK;
			outgoingMessage2.EM_MessageType = EMCSOutgoingMessageTypeList.Codes.SubmitDraftEAD;
			outgoingMessage2.EM_MessageText = "<Greeting>HELLO</Greeting>";
			outgoingMessage2.EM_Status = EMCSOutboundEDIMessage.Status.Queued;
			outgoingMessage2.EM_ReceiveTransmit = EMCSOutboundEDIMessage.Direction.Receive; // to allow saving of message number
			outgoingMessage2.EM_MessageNum = "TEST1234567891";
			var outgoingMessage3 = factory.New<EMCSOutboundEDIMessage>();
			outgoingMessage3.EM_GB = ieBranch1.PK;
			outgoingMessage3.EM_GP = emcsCredential1.PK;
			outgoingMessage3.EM_MessageType = EMCSOutgoingMessageTypeList.Codes.SubmitDraftEAD;
			outgoingMessage3.EM_MessageText = "<Greeting>HELLO</Greeting>";
			outgoingMessage3.EM_Status = EMCSOutboundEDIMessage.Status.Queued;
			outgoingMessage3.EM_ReceiveTransmit = EMCSOutboundEDIMessage.Direction.Receive; // to allow saving of message number
			outgoingMessage3.EM_MessageNum = "TEST1234567892";
			const string transactionID1 = "A02A8604-2D0B-4FEE-8EDF-61DB6AFC7639";
			const string transactionID2 = "C02A8604-2D0B-4FEE-8EDF-61DB6AFC7639";
			const string transactionID3 = "B02A8604-2D0B-4FEE-8EDF-61DB6AFC7639";
			var transactionNumber1 = factory.New<CusTransactionNumber>();
			transactionNumber1.TN_Type = CusTransactionNumberTypeList.Codes.IECustomsEMCS;
			transactionNumber1.TN_GP_ExternalPassword = emcsCredential1.PK;
			transactionNumber1.TN_GC_Company = ieCompany1.PK;
			transactionNumber1.TN_TransactionReference = transactionID1;
			var transactionNumber2 = factory.New<CusTransactionNumber>();
			transactionNumber2.TN_Type = CusTransactionNumberTypeList.Codes.IECustomsEMCS;
			transactionNumber2.TN_GP_ExternalPassword = emcsCredential1.PK;
			transactionNumber2.TN_GC_Company = ieCompany1.PK;
			transactionNumber2.TN_TransactionReference = transactionID2;
			var transactionNumber3 = factory.New<CusTransactionNumber>();
			transactionNumber3.TN_Type = CusTransactionNumberTypeList.Codes.IECustomsEMCS;
			transactionNumber3.TN_GP_ExternalPassword = emcsCredential2.PK;
			transactionNumber3.TN_GC_Company = ieCompany2.PK;
			transactionNumber3.TN_TransactionReference = transactionID3;
			factory.Save();
			outgoingMessage1.EM_ReceiveTransmit = EMCSOutboundEDIMessage.Direction.Transmit;
			outgoingMessage2.EM_ReceiveTransmit = EMCSOutboundEDIMessage.Direction.Transmit;
			outgoingMessage3.EM_ReceiveTransmit = EMCSOutboundEDIMessage.Direction.Transmit;
			factory.Save();
			var task = new TransactionIDManagerServiceTask();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);
			CombineAssertions(() =>
			{
				var anotherFactory = new BusinessObjectFactory();
				AssertEDIMessage("outgoingMessage1", (EMCSOutboundEDIMessage)anotherFactory.Load<EDIMessage>(outgoingMessage1.PK), EMCSOutboundEDIMessage.Status.Pending, transactionID1);
				AssertEDIMessage("outgoingMessage2", (EMCSOutboundEDIMessage)anotherFactory.Load<EDIMessage>(outgoingMessage2.PK), EMCSOutboundEDIMessage.Status.Pending, transactionID2);
				AssertEDIMessage("outgoingMessage3", (EMCSOutboundEDIMessage)anotherFactory.Load<EDIMessage>(outgoingMessage3.PK), EMCSOutboundEDIMessage.Status.Queued, ZString.Empty);
				var interchanges = anotherFactory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.IECustomsEMCS)
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
				var blankTransactionNumbers = anotherFactory.Load<CusTransactionNumber>(new ZQuery(CusTransactionNumberSchema.TN_Type, CusTransactionNumberTypeList.Codes.IECustomsEMCS)
					.AddToFilter(CusTransactionNumberSchema.TN_IsUsed, ZBool.False)
					.AddToFilter(CusTransactionNumberSchema.TN_TransactionReference, ZString.Empty));
				AssertEquals("blankTransactionNumbers.Length", 100, blankTransactionNumbers.Length);
				AssertEquals("All blankTransactionNumbers", true, blankTransactionNumbers.All(x => x.TN_TrackingReference == trackingReference && x.TN_GC_Company == ieCompany1.PK));
				AssertEquals("transactionNumber1.TN_IsUsed", ZBool.True, anotherFactory.Load<CusTransactionNumber>(transactionNumber1.PK).TN_IsUsed);
				AssertEquals("transactionNumber2.TN_IsUsed", ZBool.True, anotherFactory.Load<CusTransactionNumber>(transactionNumber2.PK).TN_IsUsed);
				AssertEquals("transactionNumber3.TN_IsUsed", ZBool.False, anotherFactory.Load<CusTransactionNumber>(transactionNumber3.PK).TN_IsUsed);
				AssertContains("Logs", "Information|TEST T1 COMP: 1 TID message(s) created\r\nInformation|TEST T1 COMP: Found 3 message(s) waiting for Transaction ID allocation\r\nInformation|Allocating Transaction ID 'A02A8604-2D0B-4FEE-8EDF-61DB6AFC7639' to Message #TEST1234567890\r\nInformation|Allocating Transaction ID 'C02A8604-2D0B-4FEE-8EDF-61DB6AFC7639' to Message #TEST1234567891\r\n", task.ServiceLogger.ToString());
			});
		}

		void AssertEDIMessage(string reference, EMCSOutboundEDIMessage message, string expectedStatus, string expectedApplicationReference)
		{
			AssertEquals(reference + ".EM_Status", expectedStatus, message.EM_Status);
			AssertEquals(reference + ".EM_ApplicationReference", expectedApplicationReference, message.EM_ApplicationReference);
		}

		public void TestHandlingMessageThatIsOlderThanCertainNoOfDays()
		{
			using (IE.Business.IECustomsDataRegistry.Instance.MessageProcessingNoOfDays.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3))
			{
				var factory = new BusinessObjectFactory();
				(GlbCompany ieCompany, GlbBranch ieBranch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(factory, Core.Constants.CountryCodes.Ireland, "T1");
				InterchangeProcessorTestHelper.CreateValidCredential(ieCompany);
				var companyWrapper = IE.Business.GlbCompanyWrapper.Get(ieCompany);
				var emcsCredential = companyWrapper.EMCSGlbExternalPasswordCollection.AddNew();
				emcsCredential.CurrentDecryptedCertificatePassphrase = ROSCertificateTestHelper.ValidPassword;
				emcsCredential.GP_Certificate = ROSCertificateTestHelper.ValidCertificate;
				var helper = new WebServiceEndPointProviderTestHelper(factory);
				var url = helper.SetupTransactionIDURL(true);
				var declaration1 = factory.New<EMCSJobDeclaration>();
				declaration1.JE_GB = ieBranch.PK;
				var declaration2 = factory.New<EMCSJobDeclaration>();
				declaration2.JE_GB = ieBranch.PK;
				var outgoingMessage1 = factory.New<EMCSOutboundEDIMessage>();
				outgoingMessage1.EM_GB = ieBranch.PK;
				outgoingMessage1.EM_GP = emcsCredential.PK;
				outgoingMessage1.EM_MessageType = EMCSOutgoingMessageTypeList.Codes.SubmitDraftEAD;
				outgoingMessage1.EM_MessageText = "<Greeting>HELLO</Greeting>";
				outgoingMessage1.EM_Status = EMCSOutboundEDIMessage.Status.Queued;
				outgoingMessage1.EM_ReceiveTransmit = EMCSOutboundEDIMessage.Direction.Receive; // to allow saving of message number
				outgoingMessage1.EM_MessageNum = "TEST1234567890";
				outgoingMessage1.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-5);
				outgoingMessage1.EM_LinkedObject = declaration1;
				var outgoingMessage2 = factory.New<EMCSOutboundEDIMessage>();
				outgoingMessage2.EM_GB = ieBranch.PK;
				outgoingMessage2.EM_GP = emcsCredential.PK;
				outgoingMessage2.EM_MessageType = EMCSOutgoingMessageTypeList.Codes.SubmitDraftEAD;
				outgoingMessage2.EM_MessageText = "<Greeting>HELLO</Greeting>";
				outgoingMessage2.EM_Status = EMCSOutboundEDIMessage.Status.Queued;
				outgoingMessage2.EM_ReceiveTransmit = EMCSOutboundEDIMessage.Direction.Receive; // to allow saving of message number
				outgoingMessage2.EM_MessageNum = "TEST1234567891";
				outgoingMessage2.EM_LinkedObject = declaration2;
				const string transactionID1 = "A02A8604-2D0B-4FEE-8EDF-61DB6AFC7639";
				const string transactionID2 = "C02A8604-2D0B-4FEE-8EDF-61DB6AFC7639";
				var transactionNumber1 = factory.New<CusTransactionNumber>();
				transactionNumber1.TN_Type = CusTransactionNumberTypeList.Codes.IECustomsEMCS;
				transactionNumber1.TN_GC_Company = ieCompany.PK;
				transactionNumber1.TN_GP_ExternalPassword = emcsCredential.PK;
				transactionNumber1.TN_TransactionReference = transactionID1;
				var transactionNumber2 = factory.New<CusTransactionNumber>();
				transactionNumber2.TN_Type = CusTransactionNumberTypeList.Codes.IECustomsEMCS;
				transactionNumber2.TN_GC_Company = ieCompany.PK;
				transactionNumber2.TN_GP_ExternalPassword = emcsCredential.PK;
				transactionNumber2.TN_TransactionReference = transactionID2;
				factory.Save();
				outgoingMessage1.EM_ReceiveTransmit = EMCSOutboundEDIMessage.Direction.Transmit;
				outgoingMessage2.EM_ReceiveTransmit = EMCSOutboundEDIMessage.Direction.Transmit;
				factory.Save();
				var task = new TransactionIDManagerServiceTask();
				InitialiseTaskSchedule(task);
				RunTaskSchedule(task);
				CombineAssertions(() =>
				{
					var anotherFactory = new BusinessObjectFactory();
					AssertEDIMessage("outgoingMessage1", (EMCSOutboundEDIMessage)anotherFactory.Load<EDIMessage>(outgoingMessage1.PK), EMCSOutboundEDIMessage.Status.Discarded, ZString.Empty);
					AssertEDIMessage("outgoingMessage2", (EMCSOutboundEDIMessage)anotherFactory.Load<EDIMessage>(outgoingMessage2.PK), EMCSOutboundEDIMessage.Status.Pending, transactionID1);
					AssertEquals("declaration1.JE_MessageStatus", LogicalStatusList.Codes.Failed, anotherFactory.Load<EMCSJobDeclaration>(declaration1.PK).JE_MessageStatus);
					AssertEquals("declaration2.JE_MessageStatus", ZString.Empty, anotherFactory.Load<EMCSJobDeclaration>(declaration2.PK).JE_MessageStatus);
					AssertContains("Logs", "Information|TEST T1 COMP: Found 1 message(s) waiting for Transaction ID allocation\r\nInformation|Allocating Transaction ID 'A02A8604-2D0B-4FEE-8EDF-61DB6AFC7639' to Message #TEST1234567891\r\n", task.ServiceLogger.ToString());
				});
			}
		}

		public void TestHandlingInvalidCredential()
		{
			using (IE.Business.IECustomsDataRegistry.Instance.MessageProcessingNoOfDays.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3))
			{
				var factory = new BusinessObjectFactory();
				(GlbCompany ieCompany, GlbBranch ieBranch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(factory, Core.Constants.CountryCodes.Ireland, "T1");
				InterchangeProcessorTestHelper.CreateValidCredential(ieCompany);
				var companyWrapper = IE.Business.GlbCompanyWrapper.Get(ieCompany);
				var emcsCredential = companyWrapper.EMCSGlbExternalPasswordCollection.AddNew();
				emcsCredential.CurrentDecryptedCertificatePassphrase = ROSCertificateTestHelper.ValidPassword;
				emcsCredential.GP_Certificate = ROSCertificateTestHelper.ValidCertificate;
				emcsCredential.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
				var helper = new WebServiceEndPointProviderTestHelper(factory);
				var url = helper.SetupTransactionIDURL(true);
				var declaration = factory.New<EMCSJobDeclaration>();
				declaration.JE_GB = ieBranch.PK;
				var outgoingMessage = factory.New<EMCSOutboundEDIMessage>();
				outgoingMessage.EM_GB = ieBranch.PK;
				outgoingMessage.EM_GP = emcsCredential.PK;
				outgoingMessage.EM_MessageType = EMCSOutgoingMessageTypeList.Codes.SubmitDraftEAD;
				outgoingMessage.EM_MessageText = "<Greeting>HELLO</Greeting>";
				outgoingMessage.EM_Status = EMCSOutboundEDIMessage.Status.Queued;
				outgoingMessage.EM_ReceiveTransmit = EMCSOutboundEDIMessage.Direction.Receive; // to allow saving of message number
				outgoingMessage.EM_MessageNum = "TEST1234567890";
				outgoingMessage.EM_LinkedObject = declaration;
				const string transactionID1 = "A02A8604-2D0B-4FEE-8EDF-61DB6AFC7639";
				var transactionNumber1 = factory.New<CusTransactionNumber>();
				transactionNumber1.TN_Type = CusTransactionNumberTypeList.Codes.IECustomsEMCS;
				transactionNumber1.TN_GC_Company = ieCompany.PK;
				transactionNumber1.TN_GP_ExternalPassword = emcsCredential.PK;
				transactionNumber1.TN_TransactionReference = transactionID1;
				factory.Save();
				outgoingMessage.EM_ReceiveTransmit = EMCSOutboundEDIMessage.Direction.Transmit;
				factory.Save();
				var task = new TransactionIDManagerServiceTask();
				InitialiseTaskSchedule(task);
				RunTaskSchedule(task);
				CombineAssertions(() =>
				{
					var anotherFactory = new BusinessObjectFactory();
					AssertEDIMessage("outgoingMessage", (EMCSOutboundEDIMessage)anotherFactory.Load<EDIMessage>(outgoingMessage.PK), EMCSOutboundEDIMessage.Status.Discarded, ZString.Empty);
					AssertEquals("declaration.JE_MessageStatus", LogicalStatusList.Codes.Failed, anotherFactory.Load<EMCSJobDeclaration>(declaration.PK).JE_MessageStatus);
					AssertContains("Logs", string.Empty, task.ServiceLogger.ToString());
				});
			}
		}

		public void TestServiceAttribute()
		{
			AssertSingleHostedServiceAttribute("IEE", "IE EMCS Customs Transaction ID Manager", "IEC");
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("60Seconds", GetHostedServiceAttributes().Single().MinimumPeriod);
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
				queueName: "IE EMCS Customs TransactionID",
				EDIMessageSchema.Constants.EM_Status + "=QUE",
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=TRX",
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_ApplicationReference + "=",
				EDIMessageSchema.Constants.EM_ApplicationCode + "=IEM"
			),
			new TaskNudgeInformationForTest(
				table: EDIInterchangeSchema.Constants.TableName,
				queueName: "IE EMCS Customs TransactionID Request",
				EDIInterchangeSchema.Constants.EI_Status + "=QUE",
				EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=RCV",
				EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
				EDIInterchangeSchema.Constants.EI_ApplicationCode + "=IEM",
				EDIInterchangeSchema.Constants.EI_TransportType + "=XTT",
				EDIInterchangeSchema.Constants.EI_InterchangeType + "=TID"
			),
		};
	}
}
