using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using GlbCompanyWrapper = Enterprise.Customs.CH.Business.GlbCompanyWrapper;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsHeaderCommonMessageSendingObject))]
abstract class NctsHeaderCommonMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
{
	public void TestApplicationCode() => AssertEquals("Application Code", EDIMessage.ApplicationCodes.CHCustomsPassar, SendingObject.ApplicationCode);

	public void TestGetApplicationReference() => AssertNoExceptionThrown("Message Identification new GUID", () => new Guid(SendingObject.MessageIdentification));

	public void TestMessageTypeForEDIMessage() => AssertEquals("Message Type for EDIMessage", MessageTypeCodeList.Codes.PassarNcts, SendingObject.MessageTypeForEDIMessage);

	public void TestGetGlbExternalPasswordPK()
	{
		var companyWrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany);
		companyWrapper.TokenCredentialsEnabled = true;
		var tokenCredentials = companyWrapper.TokenCredentials;

		Factory.Save();

		AssertEquals("TokenCredentials", tokenCredentials.PK, SendingObject.GetCredentialPK());
	}

	public void TestShouldSend() => CombineAssertions(() =>
	{
		AssertEquals("Ticked by default", true, SendingObject.ShouldSend);
		AssertEquals("ReadOnly", true, SendingObject.ShouldSendInfo.ReadOnly);
	});

	public void TestCanSendMessage() => CombineAssertions(() =>
	{
		const string noBIDMessage = "Business Partner ID (BID) is not configured for the company or branch. Please contact your system administrator.";
		const string noTokenMessage = "Communication Tokens are not configured for the company. Please contact your system administrator.";

		var companyWrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany);

		void SetProperties(bool branchBID = true, bool companyBID = true, bool communicationToken = true)
		{
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.RemoveAll();
			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.RemoveAll();
			if (branchBID)
			{
				GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, "123");
			}
			if (companyBID)
			{
				GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, "123");
			}
			companyWrapper.TokenCredentialsEnabled = communicationToken;
		}

		SetProperties();
		AssertEquals("Configuration is complete", ZString.Empty, EnvironmentHelper.CheckMessageSendingEnvironmentForPassarAndChartera());

		SetProperties(branchBID: false);
		AssertEquals("No branch BID", ZString.Empty, EnvironmentHelper.CheckMessageSendingEnvironmentForPassarAndChartera());

		SetProperties(companyBID: false);
		AssertEquals("No company BID", ZString.Empty, EnvironmentHelper.CheckMessageSendingEnvironmentForPassarAndChartera());

		SetProperties(branchBID: false, companyBID: false);
		AssertEquals("No BID at all", noBIDMessage, EnvironmentHelper.CheckMessageSendingEnvironmentForPassarAndChartera());

		SetProperties(communicationToken: false);
		AssertEquals("No communication token", noTokenMessage, EnvironmentHelper.CheckMessageSendingEnvironmentForPassarAndChartera());
	});

	protected void AssertMessageSubTypeForEDIMessage(string messageType, string expectedMessageSubType, NctsHeaderCommonMessageSendingObject sendingObject)
	{
		sendingObject.MessageType = messageType;
		AssertEquals("Message SubType for EDIMessage", expectedMessageSubType, sendingObject.MessageSubTypeForEDIMessage);
	}

	protected void AssertTestToMessageString(string messageType, NctsHeaderCommonMessageSendingObject sendingObject) => CombineAssertions(() =>
	{
		sendingObject.MessageType = messageType;
		var ediMessageText = sendingObject.ToMessageString();

		AssertEquals("Produces a XML", true, ediMessageText.StartsWith("<?xml"));
		AssertEquals("contains MessageType", true, ediMessageText.Contains($"<{messageType}"));
	});

	protected override BusinessObject GetNewBusinessObject() => new NctsHeaderCommonMessageSendingObject(NctsHeader);

	NctsHeader NctsHeader => nctsHeader ?? (nctsHeader = CreateNctsHeader());
	NctsHeader nctsHeader;
	NctsHeader CreateNctsHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		return nctsHeader;
	}

	NctsHeaderCommonMessageSendingObject SendingObject => sendingObject ?? (sendingObject = new NctsHeaderCommonMessageSendingObject(NctsHeader));
	NctsHeaderCommonMessageSendingObject sendingObject;
}
