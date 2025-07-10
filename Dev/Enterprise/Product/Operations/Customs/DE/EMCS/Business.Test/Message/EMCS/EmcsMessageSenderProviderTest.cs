using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Registry;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	sealed class EMCSMessageSenderProviderTest : TestCaseWithFactory
	{
		public void TestSendAlertOrRejectEad_Version2_4()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.EmcsSystemCode, VersionNumber = EmcsVersionNumberList.Codes._24 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				messageSenderProvider.SendAlertOrRejectEad(new AlertOrRejectSendingAction(emcsDeclaration));
				AssertMessageStatusAndEDIMessageDetails(nameof(CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4.ED819C), Messaging.EmcsMessageSubTypeList.Codes.Emb, EADNumber);
			}
		}

		public void TestSendCancellation_Version2_4()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.EmcsSystemCode, VersionNumber = EmcsVersionNumberList.Codes._24 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				messageSenderProvider.SendCancellation(new CancellationSendingAction(emcsDeclaration));
				AssertMessageStatusAndEDIMessageDetails(nameof(CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4.ED810C), Messaging.EmcsMessageSubTypeList.Codes.Eme, EADNumber);
			}
		}

		public void TestSendChangeOfDestination_Version2_4()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.EmcsSystemCode, VersionNumber = EmcsVersionNumberList.Codes._24 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				var action = new EMCSMessageSendingAction(emcsDeclaration);
				messageSenderProvider.SendChangeOfDestination(action);
				AssertMessageStatusAndEDIMessageDetails(nameof(CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4.ED813E), Messaging.EmcsMessageSubTypeList.Codes.Eme, EADNumber);
			}
		}

		public void TestSendDeliveryDelayExplanation_Consignor_Version2_4()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.EmcsSystemCode, VersionNumber = EmcsVersionNumberList.Codes._24 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
				messageSenderProvider.SendDeliveryDelayExplanation(new ExplanationOnDelaySendingAction(emcsDeclaration));
				AssertMessageStatusAndEDIMessageDetails(nameof(CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4.ED837B), Messaging.EmcsMessageSubTypeList.Codes.Eme, EADNumber);
			}
		}

		public void TestSendDeliveryDelayExplanation_Consignee_Version2_4()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.EmcsSystemCode, VersionNumber = EmcsVersionNumberList.Codes._24 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
				messageSenderProvider.SendDeliveryDelayExplanation(new ExplanationOnDelaySendingAction(emcsDeclaration));
				AssertMessageStatusAndEDIMessageDetails(nameof(CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4.ED837B), Messaging.EmcsMessageSubTypeList.Codes.Emb, EADNumber);
			}
		}

		public void TestSendDraftMovementRequest_Version2_4()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.EmcsSystemCode, VersionNumber = EmcsVersionNumberList.Codes._24 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				var action = new EMCSMessageSendingAction(emcsDeclaration);
				emcsDeclaration.EADNumber = ZString.Empty;
				messageSenderProvider.SendDraftMovementRequest(action);
				AssertMessageStatusAndEDIMessageDetails(nameof(CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4.ED815D), Messaging.EmcsMessageSubTypeList.Codes.Eme, ZString.Empty, emcsDeclaration.JE_OwnerRef);
			}
		}

		public void TestSendReasonForShortageExplanation_Consignor_Version2_4()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.EmcsSystemCode, VersionNumber = EmcsVersionNumberList.Codes._24 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
				messageSenderProvider.SendReasonForShortageExplanation(new ReasonForShortageSendingAction(emcsDeclaration));
				AssertMessageStatusAndEDIMessageDetails(nameof(CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4.ED871C), Messaging.EmcsMessageSubTypeList.Codes.Eme, EADNumber);
			}
		}

		public void TestSendReasonForShortageExplanation_Consignee_Version2_4()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.EmcsSystemCode, VersionNumber = EmcsVersionNumberList.Codes._24 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
				messageSenderProvider.SendReasonForShortageExplanation(new ReasonForShortageSendingAction(emcsDeclaration));
				AssertMessageStatusAndEDIMessageDetails(nameof(CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4.ED871C), Messaging.EmcsMessageSubTypeList.Codes.Emb, EADNumber);
			}
		}

		public void TestSendReportOfReceipt_Version2_4()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.EmcsSystemCode, VersionNumber = EmcsVersionNumberList.Codes._24 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				messageSenderProvider.SendReportOfReceipt(new ReportOfReceiptSendingAction(emcsDeclaration));
				AssertMessageStatusAndEDIMessageDetails(nameof(CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4.ED818C), Messaging.EmcsMessageSubTypeList.Codes.Emb, EADNumber);
			}
		}

		public void TestSendAlertOrRejectEad_Version2_5()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.EmcsSystemCode, VersionNumber = EmcsVersionNumberList.Codes._25 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				messageSenderProvider.SendAlertOrRejectEad(new AlertOrRejectSendingAction(emcsDeclaration));
				AssertMessageStatusAndEDIMessageDetails(nameof(CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5.ED819D), Messaging.EmcsMessageSubTypeList.Codes.Emb, EADNumber);
			}
		}

		public void TestSendCancellation_Version2_5()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.EmcsSystemCode, VersionNumber = EmcsVersionNumberList.Codes._25 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				messageSenderProvider.SendCancellation(new CancellationSendingAction(emcsDeclaration));
				AssertMessageStatusAndEDIMessageDetails(nameof(CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5.ED810C), Messaging.EmcsMessageSubTypeList.Codes.Eme, EADNumber);
			}
		}

		public void TestSendChangeOfDestination_Version2_5()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.EmcsSystemCode, VersionNumber = EmcsVersionNumberList.Codes._25 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				var action = new EMCSMessageSendingAction(emcsDeclaration);
				messageSenderProvider.SendChangeOfDestination(action);
				AssertMessageStatusAndEDIMessageDetails(nameof(CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5.ED813F), Messaging.EmcsMessageSubTypeList.Codes.Eme, EADNumber);
			}
		}

		public void TestSendDeliveryDelayExplanation_Consignor_Version2_5()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.EmcsSystemCode, VersionNumber = EmcsVersionNumberList.Codes._25 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
				messageSenderProvider.SendDeliveryDelayExplanation(new ExplanationOnDelaySendingAction(emcsDeclaration));
				AssertMessageStatusAndEDIMessageDetails(nameof(CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5.ED837C), Messaging.EmcsMessageSubTypeList.Codes.Eme, EADNumber);
			}
		}

		public void TestSendDeliveryDelayExplanation_Consignee_Version2_5()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.EmcsSystemCode, VersionNumber = EmcsVersionNumberList.Codes._25 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
				messageSenderProvider.SendDeliveryDelayExplanation(new ExplanationOnDelaySendingAction(emcsDeclaration));
				AssertMessageStatusAndEDIMessageDetails(nameof(CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5.ED837C), Messaging.EmcsMessageSubTypeList.Codes.Emb, EADNumber);
			}
		}

		public void TestSendDraftMovementRequest_Version2_5()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.EmcsSystemCode, VersionNumber = EmcsVersionNumberList.Codes._25 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				var action = new EMCSMessageSendingAction(emcsDeclaration);
				emcsDeclaration.EADNumber = ZString.Empty;
				messageSenderProvider.SendDraftMovementRequest(action);
				AssertMessageStatusAndEDIMessageDetails(nameof(CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5.ED815E), Messaging.EmcsMessageSubTypeList.Codes.Eme, ZString.Empty, emcsDeclaration.JE_OwnerRef);
			}
		}

		public void TestSendReasonForShortageExplanation_Consignor_Version2_5()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.EmcsSystemCode, VersionNumber = EmcsVersionNumberList.Codes._25 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
				messageSenderProvider.SendReasonForShortageExplanation(new ReasonForShortageSendingAction(emcsDeclaration));
				AssertMessageStatusAndEDIMessageDetails(nameof(CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5.ED871D), Messaging.EmcsMessageSubTypeList.Codes.Eme, EADNumber);
			}
		}

		public void TestSendReasonForShortageExplanation_Consignee_Version2_5()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.EmcsSystemCode, VersionNumber = EmcsVersionNumberList.Codes._25 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
				messageSenderProvider.SendReasonForShortageExplanation(new ReasonForShortageSendingAction(emcsDeclaration));
				AssertMessageStatusAndEDIMessageDetails(nameof(CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5.ED871D), Messaging.EmcsMessageSubTypeList.Codes.Emb, EADNumber);
			}
		}

		public void TestSendReportOfReceipt_Version2_5()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.EmcsSystemCode, VersionNumber = EmcsVersionNumberList.Codes._25 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				messageSenderProvider.SendReportOfReceipt(new ReportOfReceiptSendingAction(emcsDeclaration));
				AssertMessageStatusAndEDIMessageDetails(nameof(CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5.ED818D), Messaging.EmcsMessageSubTypeList.Codes.Emb, EADNumber);
			}
		}

		void AssertMessageStatusAndEDIMessageDetails(ZString expectedMessageType, ZString expectedMessageSubType, ZString expectedEADNumber, string expectedLogbookLocalReferenceNumber = "")
		{
			CombineAssertions(() =>
			{
				var message = (EDIMessage)emcsDeclaration.Messages.Single();
				AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.DECustomsEmcsSystem, message.EM_ApplicationCode);
				AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("EM_MessageType", DE.Messaging.EDIMessageTypeList.Codes.EMCS, message.EM_MessageType);
				AssertEquals("EM_ApplicationReference", expectedMessageType, message.EM_ApplicationReference);
				AssertEquals("EM_MessageSubType", expectedMessageSubType, message.EM_MessageSubType);
				AssertEquals("LogbookLocalReferenceNumber", expectedLogbookLocalReferenceNumber, message.GetLogbookLocalReferenceNumber());
				AssertEquals("LogbookRegistrationNumber", expectedEADNumber, message.GetLogbookRegistrationNumber());
				AssertEquals("JE_MessageStatus", EDIMessage.Status.Sent, emcsDeclaration.JE_MessageStatus);
				AssertStartsWith("EM_MessageText", $@"<?xml version=""1.0"" encoding=""utf-8""?>
<{expectedMessageType}>", message.EM_MessageText);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			emcsDeclaration = Factory.New<EMCSJobDeclaration>();
			emcsDeclaration.JE_OwnerRef = "B123456";
			emcsDeclaration.EADNumber = EADNumber;
			messageSenderProvider = new EMCSMessageSenderProvider(emcsDeclaration);
		}
		EMCSJobDeclaration emcsDeclaration;
		ISendEMCSMessages messageSenderProvider;

		const string EADNumber = "EADNUM1234";
	}
}
