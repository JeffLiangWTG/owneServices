using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Registry;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	abstract class NctsHeaderSenderTest : TestCaseWithFactory
	{
		public void TestSendMessageATLASVersion10_1()
		{
			if (!ExpectedMessageTypeATLASVersion10_1.IsEmpty)
			{
				var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = ATLASVersionNumberList.Codes._101 } };
				using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
				{
					AssertEDIMessageAndLogbookDetails(ExpectedMessageTypeATLASVersion10_1);
				}
			}
			else
			{
				Assert("ATLAS Version 10.1 Sending not implemented yet for Sending Type", true);
			}
		}

		public void TestSendMessageATLASVersion10_2()
		{
			if (!ExpectedMessageTypeATLASVersion10_2.IsEmpty)
			{
				var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = ATLASVersionNumberList.Codes._102 } };
				using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
				{
					AssertEDIMessageAndLogbookDetails(ExpectedMessageTypeATLASVersion10_2);
				}
			}
			else
			{
				Assert("ATLAS Version 10.2 Sending not implemented yet for Sending Type", true);
			}
		}

		public void TestAtLeastOneATLASVersionMustBeSpecified()
		{
			Assert("ExpectedMessageTypeATLASVersion10_1 and/or ExpectedMessageTypeATLASVersion10_2 must be specified", !ExpectedMessageTypeATLASVersion10_1.IsEmpty || !ExpectedMessageTypeATLASVersion10_2.IsEmpty);
		}

		protected NctsHeaderSender NctsDeclarationSender => nctsDeclarationSender ?? (nctsDeclarationSender = GetNctsDeclarationSender());
		NctsHeaderSender nctsDeclarationSender;

		protected abstract NctsHeaderSender GetNctsDeclarationSender();

		protected virtual ZString ExpectedMessageTypeATLASVersion10_1 => ZString.Empty;

		protected virtual ZString ExpectedMessageTypeATLASVersion10_2 => ZString.Empty;

		protected abstract ZString ExpectedMessageSubType { get; }

		protected abstract ZString MovementType { get; }

		protected virtual ZString ExpectedLogbookRegistrationNumber => ZString.Empty;

		protected virtual ZString ExpectedPhaseStatus => ZString.Empty;
		protected virtual ZString ExpectedCustomsStatus => "PRV";

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(MovementType);
			if (MovementType == NctsMovementType.Codes.Arrival)
			{
				nctsHeader.LocalReferenceNumber = "WTG1234";
			}
			else
			{
				nctsHeader.MovementHeader.BM_PaperlessInbondNum = "WTG1234";
			}

			nctsHeader.CommonMovementHeader.BM_CustomsStatus = "PRV";
			messagingObject = new NctsHeaderMessageSendingObject(nctsHeader);
			SetupDeclarantForLogbookEORIBranchSuffix();
		}

		protected NctsHeaderMessageSendingObject messagingObject;
		protected NctsHeader nctsHeader;

		void AssertEDIMessageAndLogbookDetails(ZString expectedMessageType)
		{
			NctsDeclarationSender.Send();

			CombineAssertions(() =>
			{
				var messages = nctsHeader.IsPhase5Departure ? nctsHeader.MovementHeader.Messages : nctsHeader.Messages;
				AssertEquals("Message count", 1, messages.Count);
				var message = messages[0];
				AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.DECustomsAtlasSystem, message.EM_ApplicationCode);
				AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("EM_MessageType", Messaging.EDIMessageTypeList.Codes.NCTS, message.EM_MessageType);
				AssertEquals("EM_ApplicationReference", expectedMessageType, message.EM_ApplicationReference);
				AssertEquals("EM_MessageSubType", ExpectedMessageSubType, message.EM_MessageSubType);
				var expectedLinkedObject = nctsHeader.IsPhase5Departure ? nctsHeader.MovementHeader.PK : nctsHeader.PK;
				AssertEquals("LinkedObject", expectedLinkedObject, message.EM_LinkedObject.PK);
				AssertEquals("LogbookEORIBranchSuffix exists", "EBS1", message.GetLogbookEORIBranchSuffix());
				AssertEquals("LogbookLocalReferenceNumber exists", "WTG1234", message.GetLogbookLocalReferenceNumber());
				AssertEquals("LogbookRegistrationNumber exists", ExpectedLogbookRegistrationNumber, message.GetLogbookRegistrationNumber());
				AssertEquals(ExpectedPhaseStatus, nctsHeader.CommonMovementHeader.BM_Phase);
				AssertEquals(ExpectedCustomsStatus, nctsHeader.CommonMovementHeader.BM_CustomsStatus);
			});
		}

		void SetupDeclarantForLogbookEORIBranchSuffix()
		{
			var declarant = Factory.GetOrgHeaderWithEoriNumberAndEORIBranch("OHTEST", "EOR1", Core.Constants.CountryCodes.Germany, "EBS1");
			declarant.MainAddress.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, "9877700310012345123456000", Core.Constants.CountryCodes.Germany);
			if (MovementType == NctsMovementType.Codes.Arrival)
			{
				nctsHeader.DestinationTrader.E2_OA_Address = declarant.MainAddress.PK;
			}
			else
			{
				nctsHeader.Principal.E2_OA_Address = declarant.MainAddress.PK;
			}
		}
	}
}
