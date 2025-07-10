using System;
using System.Linq;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.DE.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class GUACODSenderTest : TestCaseWithFactory
	{
		public void TestSendMessageATLASVersion10_1()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = ATLASVersionNumberList.Codes._101 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				AssertEDIMessageAndLogbookDetails(nameof(DETGCE));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			messagingObject = new SendAccessCodeViewModel(guaranteeHeader);
			messagingObject.NewMainAccessCode = "newC";

			var proxy = GlbBranch.CurrentBranch.OrgProxy;
			proxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EOR1", Core.Constants.CountryCodes.Germany);
			proxy.MainAddress.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, "EBS1", Core.Constants.CountryCodes.Germany);
			proxy.MainAddress.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, "9877700310012345123456000", Core.Constants.CountryCodes.Germany);
		}
		SendAccessCodeViewModel messagingObject;
		CusGuaranteeHeader guaranteeHeader;

		void AssertEDIMessageAndLogbookDetails(ZString expectedMessageType)
		{
			GetNctsDeclarationSender().Send();

			var message = Factory.LoadTop1<AtlasEDIMessage>(new ZQuery());

			CombineAssertions(() =>
			{
				AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.DECustomsAtlasSystem, message.EM_ApplicationCode);
				AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("EM_MessageType", EDIMessageTypeList.Codes.NCTS, message.EM_MessageType);
				AssertEquals("EM_ApplicationReference", expectedMessageType, message.EM_ApplicationReference);
				AssertEquals("EM_MessageSubType", NctsMessageSubTypeList.Codes.GuaranteeAccessHandling, message.EM_MessageSubType);
				AssertEquals("LinkedObject", guaranteeHeader.PK, message.EM_LinkedObject.PK);
				AssertEquals("LogbookEORIBranchSuffix exists", "EBS1", message.GetLogbookEORIBranchSuffix());
				AssertEquals("LogbookLocalReferenceNumber exists", "12345", message.GetLogbookLocalReferenceNumber());
				AssertEquals("NewAccessCode", "newC", message.Notes.FindByDescription(LogbookHelper.LogbookGUAMainAccessCode).Single().ST_NoteText);
			});
		}

		ZString ExpectedMessageSubType => NctsMessageSubTypeList.Codes.DestinationMessage;

		GUACODSender GetNctsDeclarationSender() => new GUACODSender(messagingObject);
	}
}
