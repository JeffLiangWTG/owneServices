using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;
using static Enterprise.Customs.CH.Business.MessagingConstants;
using GlbCompanyWrapper = Enterprise.Customs.CH.Business.GlbCompanyWrapper;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

internal class NCTSCHInterchangeProviderTest : InterchangeProviderTestCase
{
	protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection)
	{
		return CHInterchangeProvider.New(collection);
	}

	public override void TestMessagesPopulateNewInterchange()
	{
		var linkedObject = Factory.NewWithValidTestData<NctsHeader>();

		var nctMessage = CreateAndPopulateMessage(linkedObject, ApplicationCodeList.Codes.CHCustomsPassar);

		var messages = new NonDependentEDIMessageCollection(Factory);
		messages.Add(nctMessage);
		var provider = CHInterchangeProvider.New(messages);
		provider.PackCollatedMessagesIntoInterchanges();
		Factory.Save();

		var interchanges = provider.Interchanges;

		CombineAssertions(() =>
		{
			AssertEquals("Number Of Interchanges", 1, interchanges.Length);

			var interchange1 = interchanges.FirstOrDefault(x => x.PK == nctMessage.EM_EI);
			assertInterchange("#1", interchange1, nctMessage, MessagingConstants.CustomsDestinationCodes.CustomsPassar);
		});
	}

	public void TestSetHeaderTextForPassar()
	{
		const string bid = "1000088059";
		GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SwissCodeTypes.BID, bid, "CH");
		assertSetHeaderTextForPassar(bid);
	}

	void assertSetHeaderTextForPassar(string bid)
	{
		var glbCompanyWrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany);
		var linkedObject = Factory.NewWithValidTestData<NctsHeader>();

		var nctMessage = CreateAndPopulateMessage(linkedObject, ApplicationCodeList.Codes.CHCustomsPassar);

		var messages = new NonDependentEDIMessageCollection(Factory);
		messages.Add(nctMessage);
		var provider = CHInterchangeProvider.New(messages);
		provider.PackCollatedMessagesIntoInterchanges();
		Factory.Save();

		var interchanges = provider.Interchanges;
		var interchange = interchanges.FirstOrDefault(x => x.PK == nctMessage.EM_EI);

		var expectedAttributes = new Dictionary<string, string>();
		expectedAttributes[CustomMsgAttributes.BpId] = bid;
		expectedAttributes[CustomMsgAttributes.MessageType] = "NT" + nctMessage.EM_MessageSubType;
		expectedAttributes[CustomMsgAttributes.MessageID] = nctMessage.EM_ApplicationReference;
		JsonTestHelper.AssertEquals("Attributes", expectedAttributes, interchange.EI_HeaderText);
	}

	public void TestSetHeaderTextForPassar_ShouldUseParentCompany_WhenCurrentBranchBIDNotFound()
	{
		const string bid = "123456";

		GlbBranch.CurrentBranch.GB_OH_OrgProxy = CreateBranchOrgHeaderWithoutBID().PK;
		GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, bid);

		var linkedObject = Factory.NewWithValidTestData<NctsHeader>();
		var nctMessage = CreateAndPopulateMessage(linkedObject, ApplicationCodeList.Codes.CHCustomsPassar);
		var messages = new NonDependentEDIMessageCollection(Factory);
		messages.Add(nctMessage);
		var provider = CHInterchangeProvider.New(messages);
		provider.PackCollatedMessagesIntoInterchanges();
		Factory.Save();

		var interchanges = provider.Interchanges;
		var interchange = interchanges.First(x => x.PK == nctMessage.EM_EI);

		AssertNullOrEmpty("Precondition", GlbBranch.CurrentBranch.OrgProxy.GetCHCustomsRegNo(OrgCusCode.SwissCodeTypes.BID));
		AssertContains($"\"custom.CH.bpId\":\"{bid}\"", interchange.EI_HeaderText);

		OrgHeader CreateBranchOrgHeaderWithoutBID()
		{
			var branchOrgHeader = OrgHeader.New(Factory);
			branchOrgHeader.OH_Code = "111";
			return branchOrgHeader;
		}
	}

	CHEDIMessage CreateAndPopulateMessage(BusinessObject linkedObject, string applicationCode)
	{
		var message = Factory.New<CHEDIMessage>();
		message.EM_ApplicationCode = applicationCode;
		message.EM_GB = GlbBranch.CurrentBranch.PK;
		message.EM_IsTestMessage = true;
		message.EM_LinkTable = NctsHeader.Schema.TableName;
		message.EM_MessageOwner = ZString.Empty;
		message.EM_MessageText = "Message Text";
		message.EM_MessageType = MessageTypeCodeList.Codes.PassarNcts;
		message.EM_MessageSubType = MessageSubTypeCodeList.Codes.PassarDeclaration;
		message.EM_ReceiveTransmit = CHEDIMessage.Direction.Transmit;
		message.EM_Status = CHEDIMessage.Status.Sent;
		message.EM_GP = Password.PK;
		message.EM_LinkedObject = linkedObject;
		message.EM_ApplicationReference = "APREF000001";
		return message;
	}

	void assertInterchange(string interchangeNo, EDIInterchange interchange, EDIMessage message, ZString expectedEI_To)
	{
		AssertEquals($"{interchangeNo} message is queued", CHEDIMessage.Status.Sent, message.EM_Status);
		AssertNotNull($"{interchangeNo} linked to message", interchange);

		if (interchange != null)
		{
			AssertEquals($"{interchangeNo} {nameof(interchange.EI_ApplicationCode)}", message.EM_ApplicationCode, interchange.EI_ApplicationCode);
			AssertEquals($"{interchangeNo} {nameof(interchange.EI_InterchangeType)}", message.EM_MessageType, interchange.EI_InterchangeType);
			AssertEquals($"{interchangeNo} {nameof(interchange.EI_ReceiveTransmit)}", EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
			AssertEquals($"{interchangeNo} {nameof(interchange.EI_From)}", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
			AssertEquals($"{interchangeNo} {nameof(interchange.EI_To)}", expectedEI_To, interchange.EI_To);
			AssertEquals($"{interchangeNo} {nameof(interchange.EI_TransportType)}", EDIInterchange.TransportType.xT, interchange.EI_TransportType);
			AssertEquals($"{interchangeNo} {nameof(interchange.EI_Status)}", EDIInterchange.Status.Queued, interchange.EI_Status);
			AssertEquals($"{interchangeNo} {nameof(interchange.EI_Priority)}", EDIInterchangePriorityList.Codes.High, interchange.EI_Priority);
			AssertEquals($"{interchangeNo} {nameof(interchange.EI_IsActive)}", true, interchange.EI_IsActive);
			AssertEquals($"{interchangeNo} {nameof(interchange.EI_BodyText)}", message.EM_MessageText, interchange.EI_BodyText);
			AssertEquals($"{interchangeNo} {nameof(interchange.EI_FooterText)}", ZString.Empty, interchange.EI_FooterText);
			AssertEquals($"{interchangeNo} {nameof(interchange.EI_GP)}", message.EM_GP, interchange.EI_GP);
			Assert($"{interchangeNo} {nameof(interchange.EI_InterchangeNum)}", !interchange.EI_InterchangeNum.IsEmpty);
			Assert($"{interchangeNo} {nameof(interchange.EI_SessionGUID)}", !interchange.EI_SessionGUID.IsEmpty);
		}
	}

	GlbExternalPassword Password => password ?? (password = Factory.NewWithValidTestData<GlbExternalPassword>());
	GlbExternalPassword password;
}
