using System;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Chartera.Outgoing;
using CargoWise.Customs.CH.MessageDefinitions.Chartera.documentsearchrequest_v1;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CharteraOutputDocumentSearchSendingObject))]
sealed class CharteraOutputDocumentSearchSendingObjectTest : NonPersistentBusinessObjectTestCase
{
	public void TestCreationTimeTo() => CombineAssertions(() =>
	{
		sendingObject.CreationTimeTo = ZDateTime.Empty;
		sendingObject.CreationTimeFrom = new ZDateTime(2023, 7, 5, 12, 30, 50);
		AssertEquals("default when empty", new ZDateTime(2023, 7, 6, 12, 30, 49), sendingObject.CreationTimeTo);

		sendingObject.CreationTimeFrom = new ZDateTime(2023, 1, 5, 12, 30, 50);
		AssertEquals("not changed when not empty", new ZDateTime(2023, 7, 6, 12, 30, 49), sendingObject.CreationTimeTo);

		sendingObject.CreationTimeTo = ZDateTime.Empty;
		AssertEquals("changed when empty", new ZDateTime(2023, 1, 6, 12, 30, 49), sendingObject.CreationTimeTo);
	});

	public void TestProcessId() => CombineAssertions(() =>
	{
		AssertNotEquals(ZString.Empty, sendingObject.ProcessId);
		AssertEquals("Value does not change", sendingObject.ProcessId, sendingObject.ProcessId);
	});

	public void TestIDocumentSearchRequest() => CombineAssertions(() =>
	{
		sendingObject.CreationTimeFrom = new ZDateTime(2023, 1, 12, 13, 1, 1, DateTimeKind.Utc);
		sendingObject.CreationTimeTo = new ZDateTime(2023, 1, 12, 13, 2, 2, DateTimeKind.Utc);

		IDocumentSearchRequest documentSearchRequest = sendingObject;

		AssertEquals("From", new DateTime(2023, 1, 12, 13, 1, 1), documentSearchRequest.From);
		AssertEquals("To", new DateTime(2023, 1, 12, 13, 2, 2), documentSearchRequest.To);

		AssertEquals("IsHistoricalQuery", false, documentSearchRequest.IsHistoricalQuery);
		sendingObject.IsHistoricalQuery = true;
		AssertEquals("IsHistoricalQuery", true, documentSearchRequest.IsHistoricalQuery);

		AssertEquals("ProcessId", sendingObject.ProcessId, documentSearchRequest.ProcessId);

		AssertNull("DocumentTypes", documentSearchRequest.DocumentTypes);
	});

	public void TestIDocumentSearchRequest_LocalDateTime() => CombineAssertions(() =>
	{
		sendingObject.CreationTimeFrom = new ZDateTime(2023, 1, 12, 13, 1, 1);
		sendingObject.CreationTimeTo = new ZDateTime(2023, 1, 12, 13, 2, 2);

		IDocumentSearchRequest documentSearchRequest = sendingObject;

		AssertEquals("From", new ZDateTime(2023, 1, 12, 13, 1, 1).ToUniversalBranchTime(), documentSearchRequest.From);
		AssertEquals("To", new ZDateTime(2023, 1, 12, 13, 2, 2).ToUniversalBranchTime(), documentSearchRequest.To);
	});

	public void TestCaptions() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions(sendingObject.CreationTimeFromInfo, caption: "Creation Time From", fullDescription: "Search documents with creation time greater than or equal to the specified time.");
		CaptionTestHelper.AssertCaptions(sendingObject.CreationTimeToInfo, caption: "Creation Time To", shortCaption: "To", fullDescription: "Search documents with creation time lower than or equal to the specified time.");
		CaptionTestHelper.AssertCaptions(sendingObject.IsHistoricalQueryInfo, caption: "Historical Query", fullDescription: "Tick Historical Query to search in a time range earlier than 1 month ago.");
	});

	public void TestSelectedSendingObjects() => CombineAssertions(() =>
	{
		var sendingObjects = sendingObject.SelectedSendingObjects.ToArray();
		AssertEquals("count", 1, sendingObjects.Length);
		AssertEquals("sending object", sendingObject, sendingObjects[0]);
	});

	public void TestApplicationCode() => AssertEquals(ApplicationCodes.CHCustomsCharteraOutput, sendingObject.ApplicationCode);

	public void TestMessageTypeForEDIMessage() => AssertEquals(MessageTypeCodeList.Codes.REQ, sendingObject.MessageTypeForEDIMessage);

	public void TestSubMessageTypeForEDIMessage() => AssertEquals(MessageSubTypeCodeList.Codes.CharteraOutputDocumentSearchRequest, sendingObject.MessageSubTypeForEDIMessage);

	public void TestCanSendMessage() => CombineAssertions(() =>
	{
		const string noBIDMessage = "Business Partner ID (BID) is not configured for the company or branch. Please contact your system administrator.";
		const string noTokenMessage = "Communication Tokens are not configured for the company. Please contact your system administrator.";

		AssertEquals("No BID", noBIDMessage, sendingObject.CanSendMessage());

		GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, "123");
		AssertEquals("No token", noTokenMessage, sendingObject.CanSendMessage());

		CredentialsTestHelper.CreateCurrentCompanyTokenCredential();
		AssertEquals("No error", ZString.Empty, sendingObject.CanSendMessage());
	});

	[TestDate(2023, 12, 7, 12, 0, 0)]
	[TestUtcOffset(3, 0, 0)]
	public void TestToMessageString() => CombineAssertions(() =>
	{
		sendingObject.CreationTimeFrom = new ZDateTime(2023, 12, 5, 10, 13, 0);
		sendingObject.CreationTimeTo = new ZDateTime(2023, 12, 6, 11, 13, 0);
		var xmlMessage = sendingObject.ToMessageString();
		var xmlDocument = XmlObjectSerializer.Deserialize<DocumentSearchRequest>(xmlMessage);
		AssertEquals("ProcessId", sendingObject.ProcessId, xmlDocument.CurrentQuery.ProcessId);
		AssertEquals("From", sendingObject.CreationTimeFrom.ToUniversalBranchTime(), xmlDocument.CurrentQuery.From);
		AssertEquals("To", sendingObject.CreationTimeTo.ToUniversalBranchTime(), xmlDocument.CurrentQuery.To);
	});

	public void TestGetApplicationReference() => AssertEquals(sendingObject.ProcessId, sendingObject.GetApplicationReference());

	public void TestGetGlbExternalPasswordPK()
	{
		var token = CredentialsTestHelper.CreateCurrentCompanyTokenCredential();
		AssertEquals(token.PK, sendingObject.GetCredentialPK());
	}

	public void TestCompany()
	{
		AssertEquals("current company", GlbCompany.CurrentCompany.PK, sendingObject.Company.PK);
	}

	protected override void SetUp()
	{
		base.SetUp();
		sendingObject = new CharteraOutputDocumentSearchSendingObject(Factory);
	}

	CharteraOutputDocumentSearchSendingObject sendingObject;
}
