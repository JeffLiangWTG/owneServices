using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.CH;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(ExportDeclarationMessageSendingObjectParent))]
sealed class ExportDeclarationMessageSendingObjectParentTest : DeclarationMessageSendingObjectParentTest
{
	protected override string MessageType => JobMessageTypeList.Codes.Export;

	protected override BusinessObject GetNewBusinessObject() => new ExportDeclarationMessageSendingObjectParent(Declaration);

	public override void TestCanSendMessage_CheckMessageSendingEnvironment() => CombineAssertions(() =>
	{
		const string noBIDMessage = "Business Partner ID (BID) is not configured for the company or branch. Please contact your system administrator.";
		const string noTokenMessage = "Communication Tokens are not configured for the company. Please contact your system administrator.";
		var companyWrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany);
		var sendingObjectParent = (IMessageSendingObjectParent)GetNewBusinessObject();

		AssertCanSendMessage("Proxy and Company BID", ZString.Empty, proxyBID: true, companyBID: true, communicationToken: true);
		AssertCanSendMessage("Proxy BID", ZString.Empty, proxyBID: true, communicationToken: true);
		AssertCanSendMessage("Company BID", ZString.Empty, companyBID: true, communicationToken: true);
		AssertCanSendMessage("No BID", noBIDMessage, communicationToken: true);
		AssertCanSendMessage("Communication token", ZString.Empty, proxyBID: true, communicationToken: true);
		AssertCanSendMessage("No Communication token", noTokenMessage, proxyBID: true);

		void AssertCanSendMessage(string testMessage, ZString expectedErrorMessage, bool proxyBID = false, bool companyBID = false, bool communicationToken = false)
		{
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.RemoveAll();
			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.RemoveAll();
			if (proxyBID)
			{
				GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, "123");
			}
			if (companyBID)
			{
				GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, "123");
			}
			companyWrapper.TokenCredentialsEnabled = communicationToken;

			AssertEquals(testMessage, expectedErrorMessage, sendingObjectParent.CanSendMessage());
		}
	});

	public void TestCanSendMessage_ExportDeclarationActivation() => CombineAssertions(() =>
	{
		const string message = "Activation Type is empty. Please choose an Activation Type and try again.";
		const string noCustomsRegistrationNumberMessage = "Customs Registration Number is not configured for the current company. Please contact your system administrator.";

		SetMessageSendingEnvironment();
		GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.UID, "123");
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		var sendingObjectParent = (IMessageSendingObjectParent)GetNewBusinessObject();

		Declaration.JE_MessageSubType = ZString.Empty;
		AssertEquals("Activation Type is empty", message, sendingObjectParent.CanSendMessage());

		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Passar;
		AssertEquals("Activation Type PAS, No UID", ZString.Empty, sendingObjectParent.CanSendMessage());
		GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "123";
		AssertEquals("Activation Type PAS, UID", ZString.Empty, sendingObjectParent.CanSendMessage());

		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Edec;
		GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = ZString.Empty;
		AssertEquals("Activation Type EDC, No UID", noCustomsRegistrationNumberMessage, sendingObjectParent.CanSendMessage());
		GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "123";
		AssertEquals("Activation Type EDC, UID", ZString.Empty, sendingObjectParent.CanSendMessage());

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		Declaration.JE_MessageSubType = ZString.Empty;
		AssertNotEquals("Not EDA", message, sendingObjectParent.CanSendMessage());
	});

	public void TestIdentificationNumber() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions<ExportDeclarationMessageSendingObjectParent>(nameof(SendingObjectParent.IdentificationNumber), caption: "Identification Number");

		SendingObjectParent.ParentDeclaration.DeclarantAddress.Header.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, "BID123");
		AssertEquals("BID123", SendingObjectParent.IdentificationNumber);
	});

	public void TestContactName() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions<ExportDeclarationMessageSendingObjectParent>(nameof(SendingObjectParent.ContactName), caption: "Contact Name");

		AssertEquals("No agent", ZString.Empty, SendingObjectParent.ContactName);

		SetCusAgent().GS_FullName = "full-name";
		AssertEquals("With agent", "full-name", SendingObjectParent.ContactName);
	});

	public void TestPhoneNumber() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions<ExportDeclarationMessageSendingObjectParent>(nameof(SendingObjectParent.PhoneNumber), caption: "Phone Number");

		AssertEquals("No agent", ZString.Empty, SendingObjectParent.PhoneNumber);

		SetCusAgent().GS_WorkPhone = "+41212345678";
		AssertEquals("Formatted phone number", "+41 21 234 56 78", SendingObjectParent.PhoneNumber);
	});

	public void TestEmailAddress() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions<ExportDeclarationMessageSendingObjectParent>(nameof(SendingObjectParent.EmailAddress), caption: "Email");

		AssertEquals("No agent", ZString.Empty, SendingObjectParent.EmailAddress);

		SetCusAgent().GS_EmailAddress = "email-address";
		AssertEquals("email-address", SendingObjectParent.EmailAddress);
	});

	public void TestMessageSendingObjectProperties()
	{
		var properties = SendingObjectParent.MessageSendingObjectProperties;

		CombineAssertions(() =>
		{
			AssertEquals("Properties count", 8, properties.Count());

			AssertMessageObjectProperty(0, nameof(ExportDeclarationMessageSendingObject.DeclarationNumber), 130, true);
			AssertMessageObjectProperty(1, nameof(ExportDeclarationMessageSendingObject.MessageType), 60, true);
			AssertMessageObjectProperty(2, nameof(ExportDeclarationMessageSendingObject.VOCReason), 160, true);
			AssertMessageObjectProperty(3, nameof(ExportDeclarationMessageSendingObject.DeclarationType), 100, true);
			AssertMessageObjectProperty(4, nameof(ExportDeclarationMessageSendingObject.SubStyle), 100, true);
			AssertMessageObjectProperty(5, nameof(ExportDeclarationMessageSendingObject.Description), 200, false);
			AssertMessageObjectProperty(6, nameof(ExportDeclarationMessageSendingObject.LocalReferenceNumber), 180, true);
			AssertMessageObjectProperty(7, nameof(ExportDeclarationMessageSendingObject.EntryStatus), 100, true);

			void AssertMessageObjectProperty(int index, string propertyName, int width, bool isMandatory)
			{
				AssertEquals($"{propertyName}.PropertyName", propertyName, properties.ElementAt(index).PropertyName);
				AssertEquals($"{propertyName}.ColumnWidth", width, properties.ElementAt(index).ColumnWidth);
				AssertEquals($"{propertyName}.IsMandatory", isMandatory, properties.ElementAt(index).IsMandatory);
			}
		});
	}

	public void TestSendingDeclaration()
	{
		AssertType<MessageSendingDeclaration>(SendingObjectParent.SendingDeclaration);
	}

	public void TestIsExportAndAnyNC123() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		Declaration.ActiveEntryHeaders.AddNew();
		Declaration.ActiveEntryHeaders.AddNew();

		var sendingObject1 = SendingObjectParent.SendingObjectsCollection[0];
		sendingObject1.MessageType = PassarMessageTypeList.Codes.NC016;
		sendingObject1.ShouldSend = ZBool.True;
		AssertEquals("Has no NC123", false, SendingObjectParent.IsExportAndAnyNC123);

		var sendingObject2 = SendingObjectParent.SendingObjectsCollection[1];
		sendingObject2.MessageType = PassarMessageTypeList.Codes.NC123;
		sendingObject2.ShouldSend = ZBool.True;
		AssertEquals("Has NC123 selected", true, SendingObjectParent.IsExportAndAnyNC123);

		sendingObject2.ShouldSend = ZBool.False;
		AssertEquals("Has NC123 no selected", false, SendingObjectParent.IsExportAndAnyNC123);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		sendingObject2.ShouldSend = ZBool.True;
		AssertEquals("EDA", false, SendingObjectParent.IsExportAndAnyNC123);
	});

	ExportDeclarationMessageSendingObjectParent SendingObjectParent => sendingObjectParent ??= (ExportDeclarationMessageSendingObjectParent)GetNewBusinessObject();
	ExportDeclarationMessageSendingObjectParent sendingObjectParent;

	protected override void SetMessageSendingEnvironment()
	{
		var company = GlbCompany.CurrentCompany;
		company.OrgProxy.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, "123");
		GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company).TokenCredentialsEnabled = true;
	}

	GlbStaff SetCusAgent()
	{
		var staff = Factory.New<GlbStaff>();
		staff.GS_Code = "A01";
		SendingObjectParent.ParentDeclaration.JE_GS_NKCusAgent = staff.GS_Code;
		return staff;
	}
}
