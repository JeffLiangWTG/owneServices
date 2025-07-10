using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.EMCS.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	public abstract class EMCSMessageSenderProviderAbstractTest : TestCaseWithFactory
	{
		protected abstract string ResourcePathIE819Sample { get; }

		protected abstract string MessageTextVersion { get; }

		public void TestSendCancellation()
		{
			messageSenderProvider.SendCancellation(new CancellationSendingAction(emcsDeclaration));
			AssertMessageStatusAndEDIMessageDetails("IE810", EMCSOutgoingMessageTypeList.Codes.CancellationOfEAD);
		}

		public void TestSendDraftMovementRequest()
		{
			var action = new EMCSMessageSendingAction(emcsDeclaration);
			messageSenderProvider.SendDraftMovementRequest(action);
			AssertMessageStatusAndEDIMessageDetails("IE815", EMCSOutgoingMessageTypeList.Codes.SubmitDraftEAD);
		}

		public void TestSendReportOfReceipt()
		{
			messageSenderProvider.SendReportOfReceipt(new ReportOfReceiptSendingAction(emcsDeclaration));
			AssertMessageStatusAndEDIMessageDetails("IE818", EMCSOutgoingMessageTypeList.Codes.ReportOfReceipt);
		}

		[TestDate(2022, 06, 20, 23, 48, 01)]
		public void TestSendAlertOrRejectEad()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "O234";
			importer.MainAddress.OA_Address1 = "123 Main Street";
			emcsDeclaration.JE_OH_Importer = importer.PK;
			emcsDeclaration.SequenceNumber = "2";
			emcsDeclaration.CustomsOffices.AddNew(OfficeCodes_EMCS.Codes.OfficeOfDestination, "OFF123");

			var alertOrReject = new AlertOrRejectSendingAction(emcsDeclaration);
			alertOrReject.DateOfAlertOrRejection = new ZDateTime(2022, 06, 19, 15, 30, 22);
			alertOrReject.RejectedFlag = true;
			var rejectionReason = alertOrReject.AlertOrRejectionReasons.AddNew();
			rejectionReason.Reason = "C";
			rejectionReason.Information = "I dont like it";

			messageSenderProvider.SendAlertOrRejectEad(alertOrReject);
			AssertMessageStatusAndEDIMessageDetails("IE819", EMCSOutgoingMessageTypeList.Codes.AlertOrRejectionOfAnEAD);

			var expectedMessageText = ManifestResourceReadHelper.ReadManifestResourceContent(ResourcePathIE819Sample);
			var message = (Enterprise.Messaging.Business.EDIMessage)emcsDeclaration.Messages.Single();
			AssertXMLEquals("EM_MessageText", expectedMessageText, message.EM_MessageText);
		}

		public void TestSendChangeOfDestination()
		{
			var action = new EMCSMessageSendingAction(emcsDeclaration);
			messageSenderProvider.SendChangeOfDestination(action);
			AssertMessageStatusAndEDIMessageDetails("IE813", EMCSOutgoingMessageTypeList.Codes.ChangeOfDestination);
		}

		public void TestSendDeliveryDelayExplanation_Consignor()
		{
			emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			messageSenderProvider.SendDeliveryDelayExplanation(new ExplanationOnDelaySendingAction(emcsDeclaration));
			AssertMessageStatusAndEDIMessageDetails("IE837", EMCSOutgoingMessageTypeList.Codes.ExplanationOnDelayForDelivery);
		}

		public void TestSendDeliveryDelayExplanation_Consignee()
		{
			emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
			messageSenderProvider.SendDeliveryDelayExplanation(new ExplanationOnDelaySendingAction(emcsDeclaration));
			AssertMessageStatusAndEDIMessageDetails("IE837", EMCSOutgoingMessageTypeList.Codes.ExplanationOnDelayForDelivery);
		}

		public void TestSendReasonForShortageExplanation_Consignor()
		{
			emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			messageSenderProvider.SendReasonForShortageExplanation(new ReasonForShortageSendingAction(emcsDeclaration));
			AssertMessageStatusAndEDIMessageDetails("IE871", EMCSOutgoingMessageTypeList.Codes.ExplanationOnReasonForShortage);
		}

		public void TestSendReasonForShortageExplanation_Consignee()
		{
			emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
			messageSenderProvider.SendReasonForShortageExplanation(new ReasonForShortageSendingAction(emcsDeclaration));
			AssertMessageStatusAndEDIMessageDetails("IE871", EMCSOutgoingMessageTypeList.Codes.ExplanationOnReasonForShortage);
		}

		protected override void SetUp()
		{
			base.SetUp();

			companyCredential = CreateValidCredential(Env.CurrentCompany as GlbCompany);
			companyCredential.Factory.Save();
			emcsDeclaration = Factory.New<EMCSJobDeclaration>();
			emcsDeclaration.JE_CustomsProfile = companyCredential.GP_MailBoxID;
			emcsDeclaration.JE_OwnerRef = "B123456";
			emcsDeclaration.EADNumber = "EADNUM1234";
			messageSenderProvider = new EMCSMessageSenderProvider(emcsDeclaration);
		}
		EMCSJobDeclaration emcsDeclaration;
		EMCSGlbCompanyCredential companyCredential;
		ISendEMCSMessages messageSenderProvider;

		void AssertMessageStatusAndEDIMessageDetails(ZString expectedMessageName, ZString expectedMessageType)
		{
			CombineAssertions(() =>
			{
				var message = (Enterprise.Messaging.Business.EDIMessage)emcsDeclaration.Messages.Single();
				AssertEquals("EM_ApplicationCode", Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.IECustomsEMCS, message.EM_ApplicationCode);
				AssertEquals("EM_MessageType", expectedMessageType, message.EM_MessageType);
				AssertEquals("EM_ReceiveTransmit", Enterprise.Messaging.Business.EDIInterchange.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("JE_MessageStatus", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Sent, emcsDeclaration.JE_MessageStatus);
				AssertEquals("EM_GP", companyCredential.PK, message.EM_GP);
				AssertStartsWith("EM_MessageText", $@"<ie:{expectedMessageName} xmlns:tcl=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TCL:{MessageTextVersion}"" xmlns:emcs=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:EMCS:{MessageTextVersion}"" xmlns:doc=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:DOC:{MessageTextVersion}"" xmlns:tms=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:{MessageTextVersion}"" xmlns:ie=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:{expectedMessageName}:{MessageTextVersion}"">", message.EM_MessageText);
			});
		}

		EMCSGlbCompanyCredential CreateValidCredential(GlbCompany company)
		{
			var companyCredentialCollection = IE.Business.GlbCompanyWrapper.Get(company)?.EMCSGlbExternalPasswordCollection;
			var credential = companyCredentialCollection.AddNew();
			if (!credential.GP_PasswordStatus.EqualsIgnoringCase(PasswordStatusList.Codes.Valid))
			{
				credential.CurrentDecryptedCertificatePassphrase = ROSCertificateTestHelper.ValidPassword;
				credential.GP_Certificate = ROSCertificateTestHelper.ValidCertificate;
			}
			credential.GP_MailBoxID = "Certificate Identifier";
			return credential;
		}
	}
}
