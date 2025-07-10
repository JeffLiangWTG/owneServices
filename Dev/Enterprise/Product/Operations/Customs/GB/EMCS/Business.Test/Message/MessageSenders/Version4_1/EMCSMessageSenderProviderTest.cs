using System.Linq;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.GB.EMCS.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.EMCS.Business.Testing.Version4_1
{
	sealed class EMCSMessageSenderProviderTest : TestCaseWithFactory
	{
		public void TestSendCancellation()
		{
			messageSenderProvider.SendCancellation(new CancellationSendingAction(emcsDeclaration));
			AssertMessageStatusAndEDIMessageDetails(EMCSMessageBuilderLoader.CancellationOfEAD, EMCSGBOutgoingMessageTypeList.Codes.CancellationOfEAD);
		}

		public void TestSendChangeOfDestination()
		{
			var action = new EMCSMessageSendingAction(emcsDeclaration);
			messageSenderProvider.SendChangeOfDestination(action);
			AssertMessageStatusAndEDIMessageDetails(EMCSMessageBuilderLoader.ChangeOfDestination, EMCSGBOutgoingMessageTypeList.Codes.ChangeOfDestination);
		}

		public void TestSendDraftMovementRequest()
		{
			messageSenderProvider.SendDraftMovementRequest(new EMCSMessageSendingAction(emcsDeclaration));
			AssertMessageStatusAndEDIMessageDetails(EMCSMessageBuilderLoader.SubmitDraftEAD, EMCSGBOutgoingMessageTypeList.Codes.SubmitDraftEAD);
		}

		public void TestSendReportOfReceipt()
		{
			messageSenderProvider.SendReportOfReceipt(new ReportOfReceiptSendingAction(emcsDeclaration));
			AssertMessageStatusAndEDIMessageDetails(EMCSMessageBuilderLoader.ReportOfReceipt, EMCSGBOutgoingMessageTypeList.Codes.ReportOfReceipt);
		}

		public void TestSendAlertOrRejectEad()
		{
			messageSenderProvider.SendAlertOrRejectEad(new AlertOrRejectSendingAction(emcsDeclaration));
			AssertMessageStatusAndEDIMessageDetails(EMCSMessageBuilderLoader.RejectionOfEAD, EMCSGBOutgoingMessageTypeList.Codes.AlertOrRejectionOfAnEAD);
		}

		public void TestSendSplitting()
		{
			messageSenderProvider.SendSplitting();
			AssertMessageStatusAndEDIMessageDetails(EMCSMessageBuilderLoader.Splitting, EMCSGBOutgoingMessageTypeList.Codes.Splitting);
		}

		public void TestSendDeliveryDelayExplanation_Consignor()
		{
			emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			messageSenderProvider.SendDeliveryDelayExplanation(new ExplanationOnDelaySendingAction(emcsDeclaration));
			AssertMessageStatusAndEDIMessageDetails(EMCSMessageBuilderLoader.ExplanationOnDelayForDelivery, EMCSGBOutgoingMessageTypeList.Codes.ExplanationOnDelayForDelivery);
		}

		public void TestSendDeliveryDelayExplanation_Consignee()
		{
			emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
			messageSenderProvider.SendDeliveryDelayExplanation(new ExplanationOnDelaySendingAction(emcsDeclaration));
			AssertMessageStatusAndEDIMessageDetails(EMCSMessageBuilderLoader.ExplanationOnDelayForDelivery, EMCSGBOutgoingMessageTypeList.Codes.ExplanationOnDelayForDelivery);
		}

		public void TestSendReasonForShortageExplanation_Consignor()
		{
			emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			messageSenderProvider.SendReasonForShortageExplanation(new ReasonForShortageSendingAction(emcsDeclaration));
			AssertMessageStatusAndEDIMessageDetails(EMCSMessageBuilderLoader.ExplanationOnReasonForShortage, EMCSGBOutgoingMessageTypeList.Codes.ExplanationOnReasonForShortage);
		}

		public void TestSendReasonForShortageExplanation_Consignee()
		{
			emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
			messageSenderProvider.SendReasonForShortageExplanation(new ReasonForShortageSendingAction(emcsDeclaration));
			AssertMessageStatusAndEDIMessageDetails(EMCSMessageBuilderLoader.ExplanationOnReasonForShortage, EMCSGBOutgoingMessageTypeList.Codes.ExplanationOnReasonForShortage);
		}

		void AssertMessageStatusAndEDIMessageDetails(ZString expectedIEMessage, ZString expectedMessageType)
		{
			CombineAssertions(() =>
			{
				var message = (EDIMessage)emcsDeclaration.Messages.Single();
				var xmlDoc = XDocument.Parse(message.EM_MessageText);
				AssertEquals(nameof(message.EM_ApplicationCode), EDIMessage.ApplicationCodes.GbCustomsEMCS, message.EM_ApplicationCode);
				AssertEquals(nameof(message.EM_ReceiveTransmit), EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals(nameof(message.EM_MessageType), expectedMessageType, message.EM_MessageType);
				AssertEquals(nameof(message.EM_GB), emcsDeclaration.JE_GB, message.EM_GB);
				AssertEquals(nameof(emcsDeclaration.JE_MessageStatus), EDIMessage.Status.Sent, emcsDeclaration.JE_MessageStatus);
				AssertNotNull(nameof(message.EM_MessageText), xmlDoc);
				AssertEquals(expectedIEMessage, xmlDoc.Root.Name.LocalName);
				AssertEquals($"urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:{expectedIEMessage}:V3.13", xmlDoc.Root.Name.NamespaceName);
				AssertEquals(nameof(message.EM_MessageOwner), companyCredential.Code, message.EM_MessageOwner);

				static bool ElementIsEmpty(XElement element) => (element.IsEmpty || string.IsNullOrWhiteSpace(element.Value))
					&& !element.HasAttributes
					&& (!element.HasElements || !element.Elements().Any(e => !ElementIsEmpty(e)))
					&& element.Name.LocalName != "Attributes";

				var emptyElements = xmlDoc.Descendants().Where(e => ElementIsEmpty(e)).Select(e => e.ToString()).ToList();
				var elementStringForMessage = emptyElements.Count == 0 ? string.Empty : emptyElements.Aggregate((a, b) => string.Join("\r\n", a, b));

				AssertEquals($"Should not be any empty tags, except Attributes\r\n{elementStringForMessage}", 0, emptyElements.Count);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			disposablePhase41Functionality = SetPhase41Functionality();
			companyCredential = TestHelper.CreateValidCredential();
			emcsDeclaration = Factory.New<EMCSJobDeclaration>();
			emcsDeclaration.JE_CustomsProfile = companyCredential.Code;
			emcsDeclaration.JE_OwnerRef = "B123456";
			emcsDeclaration.EADNumber = EADNumber;
			emcsDeclaration.InvoiceLines.AddNew();
			messageSenderProvider = new EMCSMessageSenderProvider(emcsDeclaration);
		}

		protected override void TearDown()
		{
			base.TearDown();
			disposablePhase41Functionality?.Dispose();
			disposablePhase41Functionality = null;
		}

		DisposableAction SetPhase41Functionality()
		{
			var disposableFunctionality = ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.EMCS_Phase4_1, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, true);
			var systemCode = GlbCompany.CurrentCompany.LicenceEnterpriseCode + GlbCompany.CurrentCompany.LicenceServerID;
			var disposableFunctionalityAttribute = ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionalityAttribute(Constants.FunctionalityTypes.EMCS_Phase4_1,
				Core.Constants.CountryCodes.UnitedKingdom,
				ZDateTime.Today,
				Core.Constants.Customs.Universal.RefCusCodeList.Attributes.System,
				systemCode);
			return new DisposableAction(() =>
			{
				disposableFunctionalityAttribute.Dispose();
				disposableFunctionality.Dispose();
			});
		}

		EMCSJobDeclaration emcsDeclaration;
		CodeDescriptionPair companyCredential;
		ISendGBEMCSMessages messageSenderProvider;
		DisposableAction disposablePhase41Functionality;

		const string EADNumber = "EADNUM1234";
	}
}
