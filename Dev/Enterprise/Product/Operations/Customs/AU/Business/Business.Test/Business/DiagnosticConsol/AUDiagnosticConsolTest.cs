using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AUDiagnosticConsol))]
	sealed class AUDiagnosticConsolTest : DiagnosticConsolTest
	{
		public void TestProcessTimerTick()
		{
			var auDiagnosticConsolTesting = new AUDiagnosticConsolForTest(Factory);
			auDiagnosticConsolTesting.AllocateKeyAndDoInitialChecks();
			var messagePK = auDiagnosticConsolTesting.GetTestMessagePK();
			var consolKey = auDiagnosticConsolTesting.GetConsolKey();
			var message = Factory.Load<CMRSTREQMessage>(messagePK);
			auDiagnosticConsolTesting.ProcessTimerTick(out var action);
			AssertEquals("", action);
			message.EM_Status = "XXX";
			Factory.Save();
			auDiagnosticConsolTesting.ProcessTimerTick(out action);
			AssertEquals(DiagnosticConsolActions.Codes.Failed, action);

			var diagnosticNote = auDiagnosticConsolTesting.GetDiagnosticNote();
			diagnosticNote.Status = DiagnosticStatusList.Codes.TestMessageAtQUEStatus;
			message.EM_Status = EDIMessage.Status.Sent;
			var intx = Factory.New<EDIInterchange>();
			intx.EI_Status = EDIInterchange.Status.Queued;
			intx.EI_From = "AUTest1";
			intx.EI_To = "Test2";
			message.EM_EI = intx.PK;
			Factory.Save();
			var result = auDiagnosticConsolTesting.ProcessTimerTick(out _);
			AssertEquals(DiagnosticStatusList.Descriptions.LookingForOutgoingItem, result);

			diagnosticNote.Status = DiagnosticStatusList.Codes.LookingForOutgoingItem;
			message.Interchange.EI_Status = EDIInterchange.Status.Sent;
			Factory.Save();
			result = auDiagnosticConsolTesting.ProcessTimerTick(out _);
			AssertEquals(DiagnosticStatusList.Descriptions.OutgoingItemAtQUEStatus, result);

			diagnosticNote.Status = DiagnosticStatusList.Codes.OutgoingItemAtQUEStatus;
			result = auDiagnosticConsolTesting.ProcessTimerTick(out _);
			AssertEquals(DiagnosticStatusList.Descriptions.WaitingForTestMessageResponse, result);

			diagnosticNote.Status = DiagnosticStatusList.Codes.WaitingForTestMessageResponse;
			var receivedInterchanged = Factory.New<EDIInterchange>();
			message.Interchange.EI_Status = EDIInterchange.Status.Sent;
			receivedInterchanged.EI_ApplicationCode = EDIInterchange.ApplicationCodes.CMR;
			receivedInterchanged.EI_InterchangeType = "AUC";
			receivedInterchanged.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			receivedInterchanged.EI_InterchangeNum = message.Interchange.EI_InterchangeNum;
			receivedInterchanged.EI_SystemCreateTimeUtc = DateTime.Now;
			receivedInterchanged.EI_SystemLastEditTimeUtc = DateTime.Now;
			receivedInterchanged.EI_BodyText = "RFF+ABO:" + consolKey.Replace("-", "");
			receivedInterchanged.EI_From = "Test2";
			receivedInterchanged.EI_To = "AUTest1";
			Factory.Save();
			result = auDiagnosticConsolTesting.ProcessTimerTick(out _);
			AssertEquals(DiagnosticStatusList.Descriptions.InComingResponseReceived, result);

			diagnosticNote.Status = DiagnosticStatusList.Codes.InComingResponseReceived;
			var receivedMessage = Factory.New<CMRSTREQMessage>();
			receivedMessage.EM_EI = receivedInterchanged.PK;
			receivedInterchanged.ContainedMessages.Add(receivedMessage);
			Factory.Save();
			result = auDiagnosticConsolTesting.ProcessTimerTick(out _);
			AssertEquals(DiagnosticStatusList.Descriptions.InInterchangeProcessed, result);

			diagnosticNote.Status = DiagnosticStatusList.Codes.InInterchangeProcessed;
			receivedMessage.EM_Status = EDIMessage.Status.Error;
			Factory.Save();
			result = auDiagnosticConsolTesting.ProcessTimerTick(out action);
			AssertEquals(DiagnosticStatusList.Descriptions.OKSuccess, result);
			AssertEquals(DiagnosticConsolActions.Codes.Success, action);

			auDiagnosticConsolTesting.ReleaseDiagnosticLock();
		}

		[TestDate]
		public void TestDiagnosticConsolShouldSendTestMessageWhenCertificateIsExpiring()
		{
			var auDiagnosticConsolTesting = new AUDiagnosticConsolForTest(Factory);
			auDiagnosticConsolTesting.AllocateKeyAndDoInitialChecks();
			var messagePK = auDiagnosticConsolTesting.GetTestMessagePK();
			var certificatesHelper = new CertificateManagerHelper(Factory);
			var certData = certificatesHelper.GetEmbeddedFileData(CertificateManagerHelper.AUCryptCrt2025);
			Env.Registry.RawRegistry.AUCCompanyCertificateData.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, certData);
			DateTime startDate;
			DateTime endDate;
			certificatesHelper.SetupValidCompanyCertificatesForTest(out startDate, out endDate);
			certificatesHelper.CreateCustomsCertificates2023();
			Factory.Save();
			using (CertificateManager certificateManager = new CertificateManager(Factory))
			{
				startDate = certificateManager.CustomsCertificate.ValidFromDate;
				endDate = certificateManager.CustomsCertificate.ValidToDate;
			}

			TestDateAttribute.Date = endDate.AddDays(-27);
			var (messages, allHealthyOrWarning) = auDiagnosticConsolTesting.ProcessRegistryAndCertificateCheckResults();
			Assert(messages[0].Contains("The Australian Customs Key File (Certificate) will expire soon"));
			AssertEquals("All check are either Healthy or Warning", true, allHealthyOrWarning);

			var diagnosticNote = auDiagnosticConsolTesting.GetDiagnosticNote();
			diagnosticNote.Status = DiagnosticStatusList.Codes.LookingForOutgoingItem;
			var message = Factory.Load<CMRSTREQMessage>(messagePK);
			message.EM_Status = EDIMessage.Status.Sent;
			var intx = Factory.New<EDIInterchange>();
			intx.EI_Status = EDIInterchange.Status.Queued;
			intx.EI_From = "AUTest1";
			intx.EI_To = "Test2";
			message.EM_EI = intx.PK;
			Factory.Save();

			var resultAUDiagnosticConsol = auDiagnosticConsolTesting.ProcessTimerTick(out _);
			AssertEquals(DiagnosticStatusList.Descriptions.TestMessageAtQUEStatus, resultAUDiagnosticConsol);

			auDiagnosticConsolTesting.ReleaseDiagnosticLock();
		}

		protected override BusinessObject GetNewBusinessObject() => new AUDiagnosticConsol(Factory);

		sealed class AUDiagnosticConsolForTest : AUDiagnosticConsol
		{
			public AUDiagnosticConsolForTest(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public ZGuid GetTestMessagePK() => diagnosticNote.MessagePK;

			public ZString GetConsolKey() => ConsolKey;

			public DiagnosticNote GetDiagnosticNote() => diagnosticNote;
		}
	}
}
