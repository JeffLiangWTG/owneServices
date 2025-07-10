using System;
using CargoWise.Customs.NL.MessageContracts.CI;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Testing;

public class GuaranteeAmountInformationMessageSenderTest : TestCaseWithFactory
{
	public void TestGuaranteeAmountInformationSender_Success()
	{
		var guaranteeMessageSender = new GuaranteeAmountInformationMessageSender(new JobDeclarationMessageSendingObjectParent(declaration));

		AssertEquals(0, declaration.Messages.Count);
		var resultMessage = guaranteeMessageSender.SendMessage();

		CombineAssertions(() =>
		{
			AssertContains("Message sent successfully", resultMessage);
			AssertEquals(1, declaration.Messages.Count);
		});
	}

	public void TestCreateEDIMessage()
	{
		using (NLCustomsRegistry.Instance.IsNLTestingSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
		{
			var guaranteeMessageSender = new GuaranteeAmountInformationMessageSenderForTest(new JobDeclarationMessageSendingObjectParent(declaration));
			var ediMessage = guaranteeMessageSender.CreateEDIMessage_Exposed();

			AssertType<NLEDIMessage>(ediMessage);
			CombineAssertions(() =>
			{
				AssertEquals("Application Code", NLEDIMessage.ApplicationCodes.NLCustoms, ediMessage.EM_ApplicationCode);
				AssertNullOrEmpty("Message Owner", ediMessage.EM_MessageOwner);
				AssertEquals("Message Type", NLEDIMessageTypes.Codes.DMS, ediMessage.EM_MessageType);
				AssertEquals("Message SubType", NLConstants.EdiMessageSubTypes.GuaranteeAmountInformation, ediMessage.EM_MessageSubType);
				AssertEquals("Receive/Transmit", NLEDIMessage.Direction.Transmit, ediMessage.EM_ReceiveTransmit);
				AssertNullOrEmpty("Application Reference", ediMessage.EM_ApplicationReference);
				AssertEquals("Status", NLEDIMessage.Status.Pending, ediMessage.EM_Status);
				Assert("Held Untill Date", ediMessage.EM_HeldUntilDate.IsEmpty);
				AssertEquals("Message Text", true, ediMessage.EM_MessageText.StartsWith("<?xml"));
				AssertEquals("GB (Branch)", GlbBranch.CurrentBranch.PK, ediMessage.EM_GB);
				AssertEquals("GE (Department)", GlbDepartment.CurrentDepartment.PK, ediMessage.EM_GE);
				AssertEquals("Link Table", JobDeclaration.Schema.TableName, ediMessage.EM_LinkTable);
				AssertEquals("Link Unique ID", declaration.PK, ediMessage.EM_LinkUniqueID);
				AssertEquals("Is Active", ZBool.True, ediMessage.EM_IsActive);
				AssertEquals("Test Message", false, ediMessage.EM_IsTestMessage);
				AssertEquals("Send With Message Errors", ZBool.False, ediMessage.EM_SendWithMessageErrors);
			});
		}

		using (NLCustomsRegistry.Instance.IsNLTestingSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
		{
			var guaranteeMessageSender = new GuaranteeAmountInformationMessageSenderForTest(new JobDeclarationMessageSendingObjectParent(declaration));
			var ediMessage = guaranteeMessageSender.CreateEDIMessage_Exposed();
			AssertEquals("Test Message", true, ediMessage.EM_IsTestMessage);
		}
	}

	public void TestMessageBuilderType()
	{
		var guaranteeMessageSender = new GuaranteeAmountInformationMessageSenderForTest(new JobDeclarationMessageSendingObjectParent(declaration));
		AssertType<GuaranteeAmountInformationMessageBuilder>(guaranteeMessageSender.MessageBuilder_Exposed);
	}

	public void TestMessageLinkedToMessageSender()
	{
		var guaranteeMessageSender = new GuaranteeAmountInformationMessageSender(new JobDeclarationMessageSendingObjectParent(declaration));

		AssertEquals(0, declaration.Messages.Count);
		var resultMessage = guaranteeMessageSender.SendMessage();
		AssertContains("Message sent successfully", resultMessage);
		AssertEquals(1, declaration.Messages.Count);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var orgImporter = Factory.NewWithValidTestData<OrgHeader>();
		AddCustomsCodeForTest(orgImporter, Core.Constants.CountryCodes.Netherlands, "NL100004064");
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
	JobDeclaration declaration;

	OrgCusCode AddCustomsCodeForTest(OrgHeader organisation, ZString country, ZString customsRegNo, string codeType = null)
	{
		var taxCode = organisation.CustomsCodes.AddNew();
		taxCode.OK_RN_NKCodeCountry = country;
		taxCode.OK_CodeType = codeType ?? OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
		taxCode.OK_CustomsRegNo = customsRegNo;
		return taxCode;
	}

	#region GuaranteeMessageSenderForTest
	sealed class GuaranteeAmountInformationMessageSenderForTest : GuaranteeAmountInformationMessageSender
	{
		public GuaranteeAmountInformationMessageSenderForTest(JobDeclarationMessageSendingObjectParent provider) : base(provider)
		{
		}

		public NLEDIMessage CreateEDIMessage_Exposed() => base.CreateEDIMessage();

		public GuaranteeAmountInformationMessageBuilder MessageBuilder_Exposed => messageBuilder as GuaranteeAmountInformationMessageBuilder;
	}
	#endregion
}
