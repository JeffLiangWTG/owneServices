using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class EntryAllowedSendingValidationTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When sendingObjectInfos is null", () => new EntryAllowedSendingValidation(null));
	}

	public void TestCheckAndWarnWhenSingleSendingObjectIsAllowedToBeSent()
	{
		var warner = new EntryAllowedSendingValidation(new List<IEntryMessageSendingObjectInfo>() { GetNewSendingObjectInfo("", "MDS", "0001", true) });
		var result = warner.CheckAndWarn();
		AssertEquals("CheckAndWarn result", true, result);
		AssertNull("User confirmation message box text", UnitTestUserNotification.Instance.LastMessage.Text);
	}

	public void TestCheckAndWarnWhenSingleSendingObjectIsNotAllowedToBeSentAndUserCancelAction()
	{
		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
		var warner = new EntryAllowedSendingValidation(new List<IEntryMessageSendingObjectInfo>() { GetNewSendingObjectInfo("", "MDS", "0001", false) });
		var result = warner.CheckAndWarn();
		AssertEquals("CheckAndWarn result", false, result);
		AssertEquals("User confirmation message box text", GetExpectedWarningMessage("0001", "MDS"), UnitTestUserNotification.Instance.LastMessage.Text);
	}

	public void TestCheckAndWarnWhenSingleSendingObjectIsNotAllowedToBeSentAndUserContinueAction()
	{
		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
		var warner = new EntryAllowedSendingValidation(new List<IEntryMessageSendingObjectInfo>() { GetNewSendingObjectInfo("", "MDS", "0001", false) });
		var result = warner.CheckAndWarn();
		AssertEquals("CheckAndWarn result", true, result);
		AssertEquals("User confirmation message box text", GetExpectedWarningMessage("0001", "MDS"), UnitTestUserNotification.Instance.LastMessage.Text);
	}

	public void TestCheckAndWarnWhenMixedSendingObjects()
	{
		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
		var warner = new EntryAllowedSendingValidation(new List<IEntryMessageSendingObjectInfo>() { GetNewSendingObjectInfo("DRL", "MDS", "0001", false), GetNewSendingObjectInfo("", "MDS", "0002", true) });
		var result = warner.CheckAndWarn();
		AssertEquals("CheckAndWarn result", false, result);
		AssertEquals("User confirmation message box text", GetExpectedWarningMessage("0001", "DRL"), UnitTestUserNotification.Instance.LastMessage.Text);
	}

	string GetExpectedWarningMessage(string entryReference, string entryStatus)
	{
		return $@"The following entries were already sent and are already registered or waiting for messages from Customs.
Resending these entries could result in duplicated declarations.

Entries:
{entryReference}: {entryStatus}";
	}

	IEntryMessageSendingObjectInfo GetNewSendingObjectInfo(ZString customsStatus, ZString messageStatus, ZString entryReference, ZBool entryStatusAllowsSending)
	{
		var mock = new Mock<IEntryMessageSendingObjectInfo>();
		mock.Setup(m => m.EntryCustomsStatus).Returns(customsStatus);
		mock.Setup(m => m.EntryMessageStatus).Returns(messageStatus);
		mock.Setup(m => m.EntryReference).Returns(entryReference);
		mock.Setup(m => m.EntryStatusAllowsSending).Returns(entryStatusAllowsSending);
		return mock.Object;
	}
}
