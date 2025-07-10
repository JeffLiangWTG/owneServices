using System;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Common;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public class JobDeclarationMessageSendingObjectValidationTest : Customs.Business.Testing.MessageSendingObjectValidationTest
	{
		static readonly string[] ILECodes = new string[]
		{
			GbCusDecMessageFunctionsList.Codes.Associate,
			GbCusDecMessageFunctionsList.Codes.Disassociate,
			GbCusDecMessageFunctionsList.Codes.Close,
			GbCusDecMessageFunctionsList.Codes.QueryDeclaration,
			GbCusDecMessageFunctionsList.Codes.ArrivalAtLocation,
			GbCusDecMessageFunctionsList.Codes.DepartureFromLocation,
			GbCusDecMessageFunctionsList.Codes.AnticipatedArrivalAtLocation,
			CDSEDIMessageTypeList.Codes.MasterQueryDeclaration
		};

		public void TestCheckMessageType()
		{
			var entry = Factory.New<CusEntryHeader>();
			var messageObject = new JobDeclarationMessageSendingObject(entry);
			messageObject.MessageType = "NEW";
			AssertNoErrors(messageObject.MessageTypeInfo);
			messageObject.MessageType = "ABC";
			AssertHasErrors(messageObject.MessageTypeInfo);
			messageObject.MessageType = string.Empty;
			AssertHasErrors(messageObject.MessageTypeInfo);
			messageObject.MessageType = "NEW";
			AssertNoErrors(messageObject.MessageTypeInfo);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_EntryStatus = Customs.Common.EU.EntryStatusList.Codes.Cancelled;
			Assert("PreReq - is cancelled", entry.IsCancelledWithCustoms);
			messageObject = new JobDeclarationMessageSendingObject(entry);
			messageObject.ShouldSend = true;
			messageObject.MessageType = CDSEDIMessageTypeList.Codes.NewDeclaration;  // NEW
			AssertHasMessageErrorContaining(messageObject.MessageTypeInfo, JobDeclarationMessageSendingObjectValidation.ShouldNotSendThatMessageTypeWhenCancelled);
			messageObject.MessageType = CDSEDIMessageTypeList.Codes.AmendDeclaration;  // AMD
			AssertHasMessageErrorContaining(messageObject.MessageTypeInfo, JobDeclarationMessageSendingObjectValidation.ShouldNotSendThatMessageTypeWhenCancelled);
			messageObject.MessageType = CDSEDIMessageTypeList.Codes.InventoryLinkingQueryRequest; // LQQ // OK to send a query for a cancelled dec
			AssertNoMessageErrorContaining(messageObject.MessageTypeInfo, JobDeclarationMessageSendingObjectValidation.ShouldNotSendThatMessageTypeWhenCancelled);
			messageObject.MessageType = CDSEDIMessageTypeList.Codes.MasterQueryDeclaration; // LQM // OK to send a query for a cancelled dec
			AssertNoMessageErrorContaining(messageObject.MessageTypeInfo, JobDeclarationMessageSendingObjectValidation.ShouldNotSendThatMessageTypeWhenCancelled);
			AssertHasError(messageObject.MessageTypeInfo, JobDeclarationMessageSendingObjectValidation.CannotSendMasterQueryDeclarationWithoutMUCR);
			entry.CH_MasterUCR = "A MUCR";
			AssertNoMessageErrorContaining(messageObject.MessageTypeInfo, JobDeclarationMessageSendingObjectValidation.CannotSendMasterQueryDeclarationWithoutMUCR);

			entry.CH_MasterUCR = ZString.Empty;
			messageObject.MessageType = CDSEDIMessageTypeList.Codes.NilAmendment;
			AssertHasError(messageObject.MessageTypeInfo, JobDeclarationMessageSendingObjectValidation.CannotSendNilAmendmentWithoutMUCR);
			AssertEquals("A NIL amendment cannot be sent without an inventory consignment reference (Master UCR). To provide confirmation that the current version of the declaration is correct following a rejected amendment request, use message type 'FEC'.", JobDeclarationMessageSendingObjectValidation.CannotSendNilAmendmentWithoutMUCR);
			entry.CH_MasterUCR = "A MUCR";
			messageObject.Validation.ValidateMessageType();
			AssertNoError(messageObject.MessageTypeInfo, JobDeclarationMessageSendingObjectValidation.CannotSendNilAmendmentWithoutMUCR);

			messageObject.MovementReferenceNumber = ZString.Empty;
			messageObject.Validation.ValidateMessageType();
			AssertHasError(messageObject.MessageTypeInfo, JobDeclarationMessageSendingObjectValidation.CannotSendNilAmendmentWithEmptyMRN);

			messageObject.MovementReferenceNumber = "A MRN";
			messageObject.Validation.ValidateMessageType();
			AssertNoMessageError(messageObject.MessageTypeInfo, JobDeclarationMessageSendingObjectValidation.CannotSendNilAmendmentWithEmptyMRN);
			AssertNoMessageError(messageObject.MessageTypeInfo, JobDeclarationMessageSendingObjectValidation.CannotSendNilAmendmentWithEmptyMRN);

			messageObject.MovementReferenceNumber = ZString.Empty;
			messageObject.MessageType = CDSEDIMessageTypeList.Codes.AmendDeclaration;
			AssertHasError(messageObject.MessageTypeInfo, JobDeclarationMessageSendingObjectValidation.CannotSendAmendDeclarationWithEmptyMRN);

			messageObject.MessageType = CDSEDIMessageTypeList.Codes.CancelDeclaration;
			AssertHasError(messageObject.MessageTypeInfo, JobDeclarationMessageSendingObjectValidation.CannotSendCancelDeclarationWithEmptyMRN);

			messageObject.MessageType = CDSEDIMessageTypeList.Codes.ArrivalNotification;
			AssertHasError(messageObject.MessageTypeInfo, JobDeclarationMessageSendingObjectValidation.CannotSendArrivalNotificationWithEmptyMRN);

			messageObject.MessageType = CDSEDIMessageTypeList.Codes.FecChallenge;
			AssertHasError(messageObject.MessageTypeInfo, JobDeclarationMessageSendingObjectValidation.CannotSendFecChallengeWithEmptyMRN);

			messageObject.MovementReferenceNumber = "A MRN";
			messageObject.MessageType = CDSEDIMessageTypeList.Codes.AmendDeclaration;
			AssertNoErrors(messageObject.MessageTypeInfo);

			messageObject.MessageType = CDSEDIMessageTypeList.Codes.CancelDeclaration;
			AssertNoErrors(messageObject.MessageTypeInfo);

			messageObject.MessageType = CDSEDIMessageTypeList.Codes.FecChallenge;
			AssertNoErrors(messageObject.MessageTypeInfo);
		}

		public void TestCheckAmendmentReason()
		{
			var entry = Factory.New<CusEntryHeader>();
			var messageObject = new JobDeclarationMessageSendingObject(entry);
			messageObject.MessageType = CDSEDIMessageTypeList.Codes.NewDeclaration;

			AssertNoErrors(messageObject.VOCReasonInfo);
			messageObject.MessageType = CDSEDIMessageTypeList.Codes.AmendDeclaration;
			messageObject.Validation.ValidateVOCReason();
			AssertHasErrors(messageObject.VOCReasonInfo);

			messageObject.VOCReason = "ABC";
			AssertNoErrors(messageObject.VOCReasonInfo);

			messageObject.MessageType = CDSEDIMessageTypeList.Codes.CancelDeclaration;
			AssertNoErrors(messageObject.VOCReasonInfo);

			messageObject.VOCReason = "";
			AssertHasErrors(messageObject.VOCReasonInfo);

			messageObject.MessageType = CDSEDIMessageTypeList.Codes.NilAmendment;
			messageObject.Validation.ValidateVOCReason();
			AssertHasErrors(messageObject.VOCReasonInfo);

			messageObject.MessageType = CDSEDIMessageTypeList.Codes.FecChallenge;
			messageObject.Validation.ValidateVOCReason();
			AssertHasErrors(messageObject.VOCReasonInfo);

			messageObject.MessageType = CDSEDIMessageTypeList.Codes.ArrivalNotification;
			messageObject.Validation.ValidateVOCReason();
			AssertNoErrors(messageObject.VOCReasonInfo);
		}

		public void TestCheckAmendmentReasonCode()
		{
			var entry = Factory.New<CusEntryHeader>();
			entry.MovementReferenceNumberSetter("MRN123", ZDateTime.BrettsBirthday);
			var messageObject = new JobDeclarationMessageSendingObject(entry);
			messageObject.MessageType = CDSEDIMessageTypeList.Codes.NewDeclaration;

			AssertNoMessageErrors(messageObject.ChangeAcknowledgementIndicatorInfo);
			messageObject.MessageType = CDSEDIMessageTypeList.Codes.AmendDeclaration;
			messageObject.Validation.ValidateChangeAcknowledgementIndicator();
			AssertHasMessageErrors(messageObject.ChangeAcknowledgementIndicatorInfo);

			messageObject.ChangeAcknowledgementIndicator = "20";
			AssertNoMessageErrors(messageObject.ChangeAcknowledgementIndicatorInfo);
			messageObject.ChangeAcknowledgementIndicator = "1";
			AssertHasMessageErrors(messageObject.ChangeAcknowledgementIndicatorInfo);

			messageObject.MessageType = CDSEDIMessageTypeList.Codes.CancelDeclaration;
			messageObject.Validation.ValidateChangeAcknowledgementIndicator();
			AssertNoMessageErrors(messageObject.ChangeAcknowledgementIndicatorInfo);

			messageObject.ChangeAcknowledgementIndicator = "20";
			AssertHasMessageErrors(messageObject.ChangeAcknowledgementIndicatorInfo);

			messageObject.MessageType = CDSEDIMessageTypeList.Codes.NilAmendment;
			messageObject.ChangeAcknowledgementIndicator = "";
			messageObject.Validation.ValidateChangeAcknowledgementIndicator();
			AssertHasMessageErrors(messageObject.ChangeAcknowledgementIndicatorInfo);

			messageObject.MessageType = CDSEDIMessageTypeList.Codes.FecChallenge;
			messageObject.ChangeAcknowledgementIndicator = "";
			messageObject.Validation.ValidateChangeAcknowledgementIndicator();
			AssertHasMessageErrors(messageObject.ChangeAcknowledgementIndicatorInfo);

			messageObject.MessageType = CDSEDIMessageTypeList.Codes.ArrivalNotification;
			messageObject.ChangeAcknowledgementIndicator = "";
			messageObject.Validation.ValidateChangeAcknowledgementIndicator();
			AssertNoMessageErrors(messageObject.ChangeAcknowledgementIndicatorInfo);
		}

		public void TestCheckMessageType_SendILEToMCP()
		{
			var badge = new BadgeCodeSetting();
			badge.CSPCode = GatewayList.Codes.MCP_CUSDECOnly;
			badge.BadgeCode = "A95";
			var credential = new CredentialsSetting();
			credential.BadgeCode = badge.BadgeCode;
			credential.PIMA = "CUKFFW98000A95";
			credential.IsMaritimeLoader = true;
			credential.Company = GlbCompany.CurrentCompany.GC_Code;
			var badges = new BadgeCodeSettingCollection();
			badges.Add(badge);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);
			var credentials = new CredentialsSettingCollection();
			credential.Username = "user";
			credential.Password = "password";
			credentials.Add(credential);
			GBCustomsDataRegistry.Instance.Credentials.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, credentials);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.ZG_Gateway = GatewayList.Codes.MCP_CUSDECOnly;
			declaration.JE_CustomsProfile = "A95";

			AssertEquals(GatewayList.Codes.MCP_CUSDECOnly, declaration.ZG_Gateway);

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MasterUCR = "A MUCR";
			var mcpErrorMessage = "Do not send export inventory linking messages via MCP. Select a direct-to-CDS profile. MCP have taken the decision not to offer the exports inventory linking API externally.";
			var messageObject = new JobDeclarationMessageSendingObject(entry);

			CombineAssertions(() =>
			{
				foreach (var ileCode in ILECodes)
				{
					using (GBCustomsDataRegistry.Instance.CcsukShowDEPProfiles.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
					{
						using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.GBAllowSendILEToMCP, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, false))
						{
							AssertNoErrors(messageObject.MessageTypeInfo);

							messageObject.MessageType = ileCode;

							AssertHasErrorContaining("Error expected for " + ileCode, messageObject.MessageTypeInfo, mcpErrorMessage);
						}

						using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.GBAllowSendILEToMCP, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, true))
						{
							messageObject.MessageType = ileCode;

							AssertNoErrors("No Error expected for " + ileCode, messageObject.MessageTypeInfo);
						}
					}
				}
			});
		}
	}
}
