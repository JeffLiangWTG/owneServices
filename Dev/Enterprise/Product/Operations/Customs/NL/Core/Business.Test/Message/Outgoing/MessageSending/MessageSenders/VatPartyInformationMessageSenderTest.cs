using System;
using CargoWise.Customs.NL.MessageContracts.CI;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Testing;

public class VatPartyInformationMessageSenderTest : TestCaseWithFactory
{
	JobDeclaration declaration;
	protected override void SetUp()
	{
		base.SetUp();

		var orgImporter = Factory.NewWithValidTestData<OrgHeader>();
		orgImporter.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Netherlands;
		var addressImporter = Factory.New<OrgAddress>();
		addressImporter.OA_Code = "AAA";
		addressImporter.OA_OH = orgImporter.PK;
		addressImporter.Address1 = "Dummy Address";
		addressImporter.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_OA_ImporterAddress = addressImporter.PK;
		declaration.JE_OH_Importer = orgImporter.PK;
		declaration.JE_PaymentMethod = DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14;
	}

	public void TestVatPartyInformationMessageSender_Success()
	{
		var vatPartyInformationMessageSender = new VatPartyInformationMessageSender(new JobDeclarationMessageSendingObjectParent(declaration));
		AssertEquals(0, declaration.Messages.Count);

		var result = vatPartyInformationMessageSender.SendMessage();
		CombineAssertions(() =>
		{
			AssertContains("Message sent successfully", result);
			AssertEquals(1, declaration.Messages.Count);
		});
	}

	public void TestCreateEDIMessage()
	{
		using (NLCustomsRegistry.Instance.IsNLTestingSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
		{
			var vatPartyInformationMessageSender = new VatPartyInformationMessageSenderForTest(new JobDeclarationMessageSendingObjectParent(declaration));
			var message = vatPartyInformationMessageSender.CreateEDIMessageExposer();

			CombineAssertions(() =>
			{
				AssertEquals("Application Code", NLEDIMessage.ApplicationCodes.NLCustoms, message.EM_ApplicationCode);
				AssertNullOrEmpty("Message Owner", message.EM_MessageOwner);
				AssertEquals("Message Type", NLEDIMessageTypes.Codes.DMS, message.EM_MessageType);
				AssertEquals("Message SubType", NLConstants.EdiMessageSubTypes.VATPartyInformation, message.EM_MessageSubType);
				AssertEquals("Receive/Transmit", NLEDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertNullOrEmpty("Application Reference", message.EM_ApplicationReference);
				AssertEquals("Status", NLEDIMessage.Status.Pending, message.EM_Status);
				AssertEquals("Link Table", "JobDeclaration", message.EM_LinkTable);
				AssertEquals("Link Unique ID", declaration.PK, message.EM_LinkUniqueID);
				AssertEquals("Test Message", false, message.EM_IsTestMessage);
			});
		}

		using (NLCustomsRegistry.Instance.IsNLTestingSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
		{
			var vatPartyInformationMessageSender = new VatPartyInformationMessageSenderForTest(new JobDeclarationMessageSendingObjectParent(declaration));
			var message = vatPartyInformationMessageSender.CreateEDIMessageExposer();

			AssertEquals("Test Message", true, message.EM_IsTestMessage);
		}
	}

	public void TestMessageBuilderType()
	{
		var vatPartyInformationMessageSender = new VatPartyInformationMessageSenderForTest(new JobDeclarationMessageSendingObjectParent(declaration));
		AssertType<VatPartyInformationMessageBuilder>(vatPartyInformationMessageSender.MessageBuilderExposer);
	}

	public void TestMessageLinkedToMessageSender()
	{
		var vatPartyInformationMessageSender = new VatPartyInformationMessageSender(new JobDeclarationMessageSendingObjectParent(declaration));
		AssertEquals(0, declaration.Messages.Count);
		var resultMessage = vatPartyInformationMessageSender.SendMessage();
		AssertContains("Message sent successfully", resultMessage);
		AssertEquals(1, declaration.Messages.Count);
	}
}

sealed class VatPartyInformationMessageSenderForTest : VatPartyInformationMessageSender
{
	public VatPartyInformationMessageSenderForTest(JobDeclarationMessageSendingObjectParent provider) : base(provider)
	{
	}

	public NLEDIMessage CreateEDIMessageExposer() => base.CreateEDIMessage();

	public VatPartyInformationMessageBuilder MessageBuilderExposer => messageBuilder as VatPartyInformationMessageBuilder;
}
