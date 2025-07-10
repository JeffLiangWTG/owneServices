using System.Collections.Generic;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using GlbCompanyWrapper = Enterprise.Customs.IT.Business.GlbCompanyWrapper;
using GlbStaffWrapper = Enterprise.Customs.IT.Business.GlbStaffWrapper;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

[TestedType(typeof(NctsHeaderMessageSendingObject))]
sealed class NctsHeaderMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
{
	public void TestLookups()
	{
		var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
		AssertType<NctsHeaderMessageSendingObjectLookups>("Lookups Type", sendingObject.Lookups);
	}

	public void TestValidation()
	{
		var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
		AssertType<NctsHeaderMessageSendingObjectValidation>("Validation Type", sendingObject.Validation);
	}

	public void TestJobReferenceNumberCaptions()
	{
		var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
		EU.NCTS.Business.Testing.NCTSTestHelper.AssertCaptions(sendingObject.JobReferenceNumberInfo, "Job Reference Number", "Job Ref. No.", "Ref. No.");
	}

	public void TestMessageSubTypeCaptions()
	{
		var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
		EU.NCTS.Business.Testing.NCTSTestHelper.AssertCaptions(sendingObject.MessageSubTypeInfo, "Message Sub Type", "", "Msg. Sub Type");
	}

	public void TestMessageSubTypeMaxLength()
	{
		var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
		AssertEquals("MaxLength MessageSubType", 2, sendingObject.MessageSubTypeInfo.MaxLength);
	}

	public void TestDefaultMessageSubType()
	{
		var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
		AssertEquals("Default MessageSubType", "D1", sendingObject.MessageSubType);
	}

	public void TestIEntryMessageSendingObjectInfo()
	{
		nctsHeader.BH_JobReference = "A00";
		nctsHeader.BH_MessageStatus = "MDN";
		nctsHeader.MovementHeader.BM_CustomsStatus = "DRL";
		CombineAssertions(() =>
		{
			var entryMessageSendingObjectInfo = (IEntryMessageSendingObjectInfo)GetNewBusinessObject();
			AssertEquals("EntryStatusAllowsSending", true, entryMessageSendingObjectInfo.EntryStatusAllowsSending);
			AssertEquals("EntryReference", "A00", entryMessageSendingObjectInfo.EntryReference);
			AssertEquals("EntryMessageStatus", "MDN", entryMessageSendingObjectInfo.EntryMessageStatus);
			AssertEquals("EntryCustomsStatus", "DRL", entryMessageSendingObjectInfo.EntryCustomsStatus);
		});
	}

	public void TestEntryStatusAllowsSending()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;

		CombineAssertions("When NCTS5 Departure", () =>
		{
			nctsHeader.MovementHeader.BM_CustomsStatus = string.Empty;
			nctsHeader.MovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Failed;

			nctsHeader.MovementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			var nctsHeaderMessageSendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
			nctsHeaderMessageSendingObject.MessageType = EDIMessageTypeList.Codes.NewDeclaration;
			AssertStatusAllowsSending(nctsHeaderMessageSendingObject, true);

			nctsHeader.MovementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Cancellation;
			nctsHeaderMessageSendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
			nctsHeaderMessageSendingObject.MessageType = EDIMessageTypeList.Codes.Cancellation;
			AssertStatusAllowsSending(nctsHeaderMessageSendingObject, true);

			nctsHeader.MovementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Amendment;
			nctsHeaderMessageSendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
			nctsHeaderMessageSendingObject.MessageType = EDIMessageTypeList.Codes.Amendment;
			AssertStatusAllowsSending(nctsHeaderMessageSendingObject, true);

			nctsHeader.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
			nctsHeader.MovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Error;

			nctsHeader.MovementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			nctsHeaderMessageSendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
			nctsHeaderMessageSendingObject.MessageType = EDIMessageTypeList.Codes.NewDeclaration;
			AssertStatusAllowsSending(nctsHeaderMessageSendingObject, true);

			nctsHeader.MovementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Cancellation;
			nctsHeaderMessageSendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
			nctsHeaderMessageSendingObject.MessageType = EDIMessageTypeList.Codes.Cancellation;
			AssertStatusAllowsSending(nctsHeaderMessageSendingObject, true);

			nctsHeaderMessageSendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
			nctsHeaderMessageSendingObject.MessageType = EDIMessageTypeList.Codes.NewDeclaration;
			AssertStatusAllowsSending(nctsHeaderMessageSendingObject, false);

			nctsHeader.MovementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Amendment;
			nctsHeaderMessageSendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
			nctsHeaderMessageSendingObject.MessageType = EDIMessageTypeList.Codes.Amendment;
			AssertStatusAllowsSending(nctsHeaderMessageSendingObject, true);

			nctsHeaderMessageSendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
			nctsHeaderMessageSendingObject.MessageType = EDIMessageTypeList.Codes.NewDeclaration;
			AssertStatusAllowsSending(nctsHeaderMessageSendingObject, false);

			var msgEvalScenarios = new List<(string MessageType, string DepartureStatus, string MessageStatus, string PhaseStatus, bool AllowsSendingExpected)>();

			msgEvalScenarios.Add((MessageType: "CAN", DepartureStatus: "AMR", MessageStatus: "FAL", PhaseStatus: "013", AllowsSendingExpected: true));
			msgEvalScenarios.Add((MessageType: "CAN", DepartureStatus: "ACK", MessageStatus: "FAL", PhaseStatus: "013", AllowsSendingExpected: true));
			msgEvalScenarios.Add((MessageType: "CAN", DepartureStatus: "CAN", MessageStatus: "FAL", PhaseStatus: "013", AllowsSendingExpected: true));
			msgEvalScenarios.Add((MessageType: "CAN", DepartureStatus: string.Empty, MessageStatus: "FAL", PhaseStatus: "013", AllowsSendingExpected: true));
			msgEvalScenarios.Add((MessageType: "CAN", DepartureStatus: null, MessageStatus: "FAL", PhaseStatus: "013", AllowsSendingExpected: true));

			msgEvalScenarios.Add((MessageType: "CAN", DepartureStatus: "AMR", MessageStatus: "ERR", PhaseStatus: "013", AllowsSendingExpected: true));
			msgEvalScenarios.Add((MessageType: "CAN", DepartureStatus: "ACK", MessageStatus: "ERR", PhaseStatus: "013", AllowsSendingExpected: true));
			msgEvalScenarios.Add((MessageType: "CAN", DepartureStatus: "CAN", MessageStatus: "ERR", PhaseStatus: "013", AllowsSendingExpected: true));
			msgEvalScenarios.Add((MessageType: "CAN", DepartureStatus: string.Empty, MessageStatus: "ERR", PhaseStatus: "013", AllowsSendingExpected: true));
			msgEvalScenarios.Add((MessageType: "CAN", DepartureStatus: null, MessageStatus: "ERR", PhaseStatus: "013", AllowsSendingExpected: true));

			msgEvalScenarios.Add((MessageType: "CAN", DepartureStatus: "AMR", MessageStatus: "FAL", PhaseStatus: "015", AllowsSendingExpected: true));
			msgEvalScenarios.Add((MessageType: "CAN", DepartureStatus: "ACK", MessageStatus: "FAL", PhaseStatus: "015", AllowsSendingExpected: true));
			msgEvalScenarios.Add((MessageType: "CAN", DepartureStatus: "CAN", MessageStatus: "FAL", PhaseStatus: "015", AllowsSendingExpected: true));
			msgEvalScenarios.Add((MessageType: "CAN", DepartureStatus: string.Empty, MessageStatus: "FAL", PhaseStatus: "015", AllowsSendingExpected: true));
			msgEvalScenarios.Add((MessageType: "CAN", DepartureStatus: null, MessageStatus: "FAL", PhaseStatus: "015", AllowsSendingExpected: true));

			msgEvalScenarios.Add((MessageType: "CAN", DepartureStatus: "AMR", MessageStatus: "ERR", PhaseStatus: "015", AllowsSendingExpected: true));
			msgEvalScenarios.Add((MessageType: "CAN", DepartureStatus: "ACK", MessageStatus: "ERR", PhaseStatus: "015", AllowsSendingExpected: true));
			msgEvalScenarios.Add((MessageType: "CAN", DepartureStatus: "CAN", MessageStatus: "ERR", PhaseStatus: "015", AllowsSendingExpected: true));
			msgEvalScenarios.Add((MessageType: "CAN", DepartureStatus: string.Empty, MessageStatus: "ERR", PhaseStatus: "015", AllowsSendingExpected: true));
			msgEvalScenarios.Add((MessageType: "CAN", DepartureStatus: null, MessageStatus: "ERR", PhaseStatus: "015", AllowsSendingExpected: true));

			msgEvalScenarios.Add((MessageType: "NEW", DepartureStatus: "AMR", MessageStatus: "FAL", PhaseStatus: "015", AllowsSendingExpected: true));
			msgEvalScenarios.Add((MessageType: "NEW", DepartureStatus: "ACK", MessageStatus: "FAL", PhaseStatus: "015", AllowsSendingExpected: true));
			msgEvalScenarios.Add((MessageType: "NEW", DepartureStatus: "CAN", MessageStatus: "FAL", PhaseStatus: "015", AllowsSendingExpected: true));
			msgEvalScenarios.Add((MessageType: "NEW", DepartureStatus: string.Empty, MessageStatus: "FAL", PhaseStatus: "015", AllowsSendingExpected: true));
			msgEvalScenarios.Add((MessageType: "NEW", DepartureStatus: null, MessageStatus: "FAL", PhaseStatus: "015", AllowsSendingExpected: true));

			msgEvalScenarios.Add((MessageType: "NEW", DepartureStatus: "AMR", MessageStatus: "ERR", PhaseStatus: "015", AllowsSendingExpected: true));
			msgEvalScenarios.Add((MessageType: "NEW", DepartureStatus: "ACK", MessageStatus: "ERR", PhaseStatus: "015", AllowsSendingExpected: true));
			msgEvalScenarios.Add((MessageType: "NEW", DepartureStatus: "CAN", MessageStatus: "ERR", PhaseStatus: "015", AllowsSendingExpected: true));
			msgEvalScenarios.Add((MessageType: "NEW", DepartureStatus: string.Empty, MessageStatus: "ERR", PhaseStatus: "015", AllowsSendingExpected: true));
			msgEvalScenarios.Add((MessageType: "NEW", DepartureStatus: null, MessageStatus: "ERR", PhaseStatus: "015", AllowsSendingExpected: true));

			foreach (var scenario in msgEvalScenarios)
			{
				nctsHeader.MovementHeader.BM_CustomsStatus = scenario.DepartureStatus;
				nctsHeader.MovementHeader.BM_MessageStatus = scenario.MessageStatus;
				nctsHeader.MovementHeader.BM_Phase = scenario.PhaseStatus;

				nctsHeaderMessageSendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
				nctsHeaderMessageSendingObject.MessageType = scenario.MessageType;

				var allowsSendingExpected = scenario.AllowsSendingExpected;
				AssertStatusAllowsSending(nctsHeaderMessageSendingObject, allowsSendingExpected);
			}
		});
	}

	void AssertStatusAllowsSending(NctsHeaderMessageSendingObject nctsHeaderMessageSendingObject, bool expectedResult)
	{
		var nctsHeader = nctsHeaderMessageSendingObject.NctsHeader;
		var movementHeader = nctsHeader.CommonMovementHeader;
		var entryMessageSendingObjectInfo = (IEntryMessageSendingObjectInfo)nctsHeaderMessageSendingObject;

		AssertEquals($"When MessageType: {nctsHeaderMessageSendingObject.MessageType}, " +
			$"BM_CustomsStatus: {movementHeader.BM_CustomsStatus}, " +
			$"BM_MessageStatus: {movementHeader.BM_MessageStatus}, " +
			$"BM_Phase: {movementHeader.BM_Phase} " +
			$"StatusAllowsSending", expectedResult, entryMessageSendingObjectInfo.EntryStatusAllowsSending);
	}

	public void TestReasonIsReadOnly()
	{
		var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
		AssertEquals("Reason must be readonly when messageType is not CAN nor AMD", true, sendingObject.ReasonInfo.ReadOnly);

		sendingObject.MessageType = "CAN";
		AssertEquals("Reason must not be readonly when messageType is CAN", false, sendingObject.ReasonInfo.ReadOnly);

		sendingObject.MessageType = "AMD";
		AssertEquals("Reason must not be readonly when messageType is AMD", false, sendingObject.ReasonInfo.ReadOnly);
	}

	public void TestLegislativeReferenceIsReadOnly()
	{
		var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
		AssertEquals("Reference must be readonly when messageType is not CAN nor AMD", true, sendingObject.LegislativeReferenceInfo.ReadOnly);

		sendingObject.MessageType = "CAN";
		AssertEquals("Reference must not be readonly when messageType is CAN", false, sendingObject.LegislativeReferenceInfo.ReadOnly);

		sendingObject.MessageType = "AMD";
		AssertEquals("Reference must not be readonly when messageType is AMD", false, sendingObject.LegislativeReferenceInfo.ReadOnly);
	}

	public void TestReasonReset()
	{
		var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
		sendingObject.MessageType = "CAN";
		sendingObject.Reason = "B";
		sendingObject.MessageType = "NEW";
		AssertEquals("Reason must be set to empty string when messageType is changed to NEW from CAN", string.Empty, sendingObject.Reason);

		sendingObject.MessageType = "AMD";
		sendingObject.Reason = "C";
		sendingObject.MessageType = "NEW";
		AssertEquals("Reason must be set to empty string when messageType is changed to NEW from AMD", string.Empty, sendingObject.Reason);
	}

	public void TestLegislativeReferenceReset()
	{
		var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
		sendingObject.MessageType = "CAN";
		sendingObject.LegislativeReference = "2";
		sendingObject.MessageType = "NEW";
		AssertEquals("LegislativeReference must be set to empty string when messageType is changed to NEW from CAN", string.Empty, sendingObject.LegislativeReference);

		sendingObject.MessageType = "AMD";
		sendingObject.LegislativeReference = "1";
		sendingObject.MessageType = "NEW";
		AssertEquals("LegislativeReference must be set to empty string when messageType is changed to NEW from AMD", string.Empty, sendingObject.LegislativeReference);
	}

	public void TestIsCancellation()
	{
		var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
		CombineAssertions("IsCancellation", () =>
		{
			AssertEquals("When messageType is not CAN", false, sendingObject.IsCancel);

			sendingObject.MessageType = "CAN";
			AssertEquals("When messageType is CAN", true, sendingObject.IsCancel);
		});
	}

	public void TestIsAmend()
	{
		var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
		CombineAssertions("IsAmend", () =>
		{
			AssertEquals("When messageType is not AMD", false, sendingObject.IsAmend);

			sendingObject.MessageType = "AMD";
			AssertEquals("When messageType is AMD", true, sendingObject.IsAmend);
		});
	}

	public void TestGetMessageSubTypeForCancellationMessages()
	{
		var sendingObject = (NctsHeaderMessageSendingObject)GetNewBusinessObject();
		sendingObject.MessageType = "CAN";
		var valuesProvider = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)sendingObject;
		AssertEquals("GetSubType", "CAN", valuesProvider.GetSubType());
	}

	public void TestGetServiceIdForCancellationMessages()
	{
		var sendingObject = (NctsHeaderMessageSendingObject)GetNewBusinessObject();
		sendingObject.MessageType = "CAN";
		var valuesProvider = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)sendingObject;
		AssertEquals("GetServiceId", "annullaDichiarazione", valuesProvider.GetServiceId());
	}

	public void TestGetServiceIdForNewDeclarationMessages()
	{
		var sendingObject = (NctsHeaderMessageSendingObject)GetNewBusinessObject();
		sendingObject.MessageType = "NEW";
		var valuesProvider = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)sendingObject;
		AssertEquals("GetServiceId", "invioDichiarazione", valuesProvider.GetServiceId());
	}

	public void TestGetServiceIdForAmendmentDeclarationMessages()
	{
		var sendingObject = (NctsHeaderMessageSendingObject)GetNewBusinessObject();
		sendingObject.MessageType = "AMD";
		var valuesProvider = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)sendingObject;
		AssertEquals("GetServiceId", "rettificaDichiarazione", valuesProvider.GetServiceId());
	}

	public void TestCryptokiCertificate()
	{
		using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
		{
			var currentStaff = GlbStaff.CurrentUser;
			var staffWrapper = GlbStaffWrapper.Get(currentStaff);
			var cryptokiCertificate = staffWrapper.CryptokiCertificateCollection.AddNew();
			cryptokiCertificate.GP_Name = ChipsetList.Codes.Bit4id;
			Factory.Save();

			var sendingObject = (NctsHeaderMessageSendingObject)GetNewBusinessObject();
			var valuesProvider = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)sendingObject;
			AssertNotNull("Cryptoki Certificate", valuesProvider.CryptokiCertificate);
			AssertEquals("GP_Name", "BIT4ID", valuesProvider.CryptokiCertificate.GP_Name);
		}
	}

	public void TestMauCertificate()
	{
		using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
		{
			var currentCompany = GlbCompany.CurrentCompany;
			var companyWrapper = GlbCompanyWrapper.Get(currentCompany);
			var mauPassword = companyWrapper.PasswordCollection.AddNew();
			mauPassword.GP_UserID = "1234";
			currentCompany.Factory.Save();

			nctsHeader.BH_CustomsProfile = "1234";

			var sendingObject = (NctsHeaderMessageSendingObject)GetNewBusinessObject();
			var valuesProvider = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)sendingObject;
			AssertNotNull("MAU Certificate", valuesProvider.MauCertificate);
			AssertEquals("GP_UserID", "1234", valuesProvider.MauCertificate.GP_UserID);
		}
	}

	public void TestServiceTypeNamespace()
	{
		var sendingObject = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)GetNewBusinessObject();
		AssertEquals("ServiceTypeNamespace", "http://transitoservice.domest.sogei.it", sendingObject.ServiceTypeNamespace);
	}

	public void TestServiceTypePrefix()
	{
		var sendingObject = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)GetNewBusinessObject();
		AssertEquals("ServiceTypePrefix", "tns", sendingObject.ServiceTypePrefix);
	}

	public void TestXmlSigner()
	{
		var sendingObject = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)GetNewBusinessObject();
		AssertType<AidaXmlSigner>("XmlSigner Type", sendingObject.XmlSigner);
	}

	public void TestGetMessageType()
	{
		var sendingObject = (NctsHeaderMessageSendingObject)GetNewBusinessObject();
		sendingObject.MessageType = "ABC";
		var valuesProvider = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)sendingObject;
		AssertEquals("GetMessageType", "ABC", valuesProvider.GetMessageType());
	}

	public void TestCustomsMessageTextForCancellationMessages()
	{
		var sendingObject = (NctsHeaderMessageSendingObject)GetNewBusinessObject();
		sendingObject.MessageType = "CAN";
		var valuesProvider = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)sendingObject;
		AssertContains("CustomsMessageText when MessageType is CAN", "<Cancellation />", valuesProvider.CustomsMessageText);
	}

	public void TestGetCancellationXmlMessageBuilderType()
	{
		var sendingObject = new NctsMessageSendingObjectForTest(nctsHeader);
		AssertType<CancellationMessageBuilder>(sendingObject.GetCancellationXmlMessageBuilder_Exposed());
	}

	public void TestGetApplicationReference()
	{
		var sendingObject = (NctsHeaderMessageSendingObject)GetNewBusinessObject();
		var valuesProvider = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)sendingObject;
		AssertEquals("GetApplicationReference", "TRA", valuesProvider.GetApplicationReference());
	}

	public void TestGetMessageBuilder()
	{
		var sendingObject = new NctsMessageSendingObjectForTest(nctsHeader);
		sendingObject.MessageSubType = "D1";
		AssertType<D1MessageBuilder>("MessageSubType = D1", sendingObject.GetMessageBuilder_Exposed());

		sendingObject.MessageSubType = "D2";
		AssertType<D2MessageBuilder>("MessageSubType = D2", sendingObject.GetMessageBuilder_Exposed());

		sendingObject.MessageSubType = "D4";
		AssertNull("MessageSubType = D4", sendingObject.GetMessageBuilder_Exposed());
	}

	public void TestGetMessageBuilder_WhenTransitionPeriodIsOn()
	{
		UniversalReferenceTestDataHelper.RunAssertionsInPhase5TransitionPeriod(() =>
				SetEntryStyleAndAssertMessageBuilderType<D1TransitionPeriodMessageBuilder>("D1"));
	}

	public void TestGetMessageBuilder_WhenTransitionPeriodIsOff()
	{
		UniversalReferenceTestDataHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
				SetEntryStyleAndAssertMessageBuilderType<D1MessageBuilder>("D1"));
	}

	public void TestAmendment()
	{
		var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
		AssertNull("When MessageType is not AMD, Amendment", sendingObject.Amendment);

		sendingObject.MessageType = "AMD";
		AssertType<NctsAmendmentWrapper>("When MessageType is AMD, Amendment", sendingObject.Amendment);
		var amendment = sendingObject.Amendment;
		AssertSame("Cached Amendment", amendment, sendingObject.Amendment);

		sendingObject.MessageType = "";
		AssertNull("Amendment", sendingObject.Amendment);
	}

	public void TestGetMessageBuilder_WhenAmendment()
	{
		var sendingObject = new NctsMessageSendingObjectForTest(nctsHeader);
		sendingObject.MessageType = "AMD";
		sendingObject.Reason = "#";

		sendingObject.MessageSubType = "D1";
		var message = sendingObject.GetMessageBuilder_Exposed().GenerateXmlMessage();
		AssertContains("Causal", "#", message.GetSerializedString());

		sendingObject.Reason = "*";
		sendingObject.MessageSubType = "D2";
		message = sendingObject.GetMessageBuilder_Exposed().GenerateXmlMessage();
		AssertContains("Causal", "*", message.GetSerializedString());
	}

	public void TestHasValidAutomaticSignature()
	{
		using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
		{
			var sendingObject = new NctsMessageSendingObjectForTest(nctsHeader);
			var valuesProvider = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)sendingObject;
			AssertEquals("When user does not have any Automatic Signature Password, HasValidAutomaticSignature",
				false,
				valuesProvider.HasValidAutomaticSignature);

			var currentStaff = GlbStaff.CurrentUser;
			var staffWrapper = GlbStaffWrapper.Get(currentStaff);
			var automaticSignature = staffWrapper.AutomaticSignaturePasswordCollection.AddNew();

			automaticSignature.IsConfigurationActive = true;
			AssertEquals("When user has an active Automatic Signature Password, HasValidAutomaticSignature",
				true,
				valuesProvider.HasValidAutomaticSignature);

			automaticSignature.IsConfigurationActive = false;
			AssertEquals("When user has an Automatic Signature Password but it is not Active, HasValidAutomaticSignature",
				false,
				valuesProvider.HasValidAutomaticSignature);
		}
	}

	public void TestLocalReferenceNumberGenerator()
	{
		var sendingObject = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)GetNewBusinessObject();
		AssertType<LocalReferenceNumberGenerator>("LocalReferenceNumberGenerator Type", sendingObject.LocalReferenceNumberGenerator);
	}

	protected override BusinessObject GetNewBusinessObject() => new NctsHeaderMessageSendingObject(nctsHeader);

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewDepartureNctsHeader();
	}

	void SetEntryStyleAndAssertMessageBuilderType<TExpectedType>(ZString entryStyle)
	{
		var sendingObject = new NctsMessageSendingObjectForTest(nctsHeader);
		sendingObject.MessageSubType = entryStyle;

		var messageBuilder = sendingObject.GetMessageBuilder_Exposed();
		AssertNotNull($"MessageBuilder for Declaration Type {entryStyle}", messageBuilder);
		AssertType<TExpectedType>($"Message Builder Type for Declaration  {entryStyle} ", messageBuilder);
	}

	NctsHeader nctsHeader;

	sealed class NctsMessageSendingObjectForTest : NctsHeaderMessageSendingObject
	{
		public NctsMessageSendingObjectForTest(NctsHeader header)
			: base(header)
		{
		}

		internal IXmlMessageBuilder GetCancellationXmlMessageBuilder_Exposed() => GetCancellationXmlMessageBuilder(new Mock<ICancellation>().Object);

		internal IXmlMessageBuilder GetMessageBuilder_Exposed() => GetMessageBuilder();
	}
}
