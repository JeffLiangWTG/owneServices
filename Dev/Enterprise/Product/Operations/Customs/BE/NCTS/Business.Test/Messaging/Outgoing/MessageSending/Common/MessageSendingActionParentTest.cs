using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(MessageSendingActionParent))]
	sealed class MessageSendingActionParentTest : EU.NCTS.Business.Testing.NctsHeaderMessageSendingObjectParentTest
	{
		public void TestGetBizObjValidationMessageErrors()
		{
			var sendingObjectParent = new MessageSendingActionParent(nctsHeader);
			var action = sendingObjectParent.SendingObjectsCollection[0];
			action.EntryType = NctsMessageTypeList.Codes.Declaration;
			action.ShouldSend = true;
			CombineAssertions(() =>
			{
				AssertNotNullOrEmpty(sendingObjectParent.BizObjValidationMessageErrors);

				action.EntryType = NctsMessageTypeList.Codes.InvalidationCancellation;
				AssertEquals(ZString.Empty, sendingObjectParent.BizObjValidationMessageErrors);

				action.EntryType = NctsMessageTypeList.Codes.Amendment;
				action.ShouldSend = true;
				AssertNotNullOrEmpty(sendingObjectParent.BizObjValidationMessageErrors);

				action.EntryType = NctsMessageTypeList.Codes.RequestARelease;
				AssertEquals(ZString.Empty, sendingObjectParent.BizObjValidationMessageErrors);

				action.EntryType = NctsMessageTypeList.Codes.ArrivalNotification;
				action.ShouldSend = true;
				AssertNotNullOrEmpty(sendingObjectParent.BizObjValidationMessageErrors);

				action.EntryType = NctsMessageTypeList.Codes.ResponseOnRequestForNonArrivedMovement;
				AssertEquals(ZString.Empty, sendingObjectParent.BizObjValidationMessageErrors);
			});
		}

		public void TestShowValidationErrors()
		{
			var sendingObjectParent = new MessageSendingActionParent(nctsHeader);
			var action = sendingObjectParent.SendingObjectsCollection[0];
			CombineAssertions(() =>
			{
				action.EntryType = NctsMessageTypeList.Codes.Declaration;
				AssertEquals(true, sendingObjectParent.ShowValidationErrors);

				action.EntryType = NctsMessageTypeList.Codes.InvalidationCancellation;
				AssertEquals(false, sendingObjectParent.ShowValidationErrors);

				action.EntryType = NctsMessageTypeList.Codes.RequestARelease;
				AssertEquals(false, sendingObjectParent.ShowValidationErrors);

				action.EntryType = NctsMessageTypeList.Codes.ResponseOnRequestForNonArrivedMovement;
				AssertEquals(false, sendingObjectParent.ShowValidationErrors);
			});
		}

		public new void TestTopLevelBusinessObject()
		{
			AssertType<NctsHeader>(messageSendingActionParent.TopLevelBusinessObject);
		}

		public void TestMessageSendingActions()
		{
			var messageSendingActionParentForTest = new MessageSendingActionParentForTest(nctsHeader);
			CombineAssertions(() =>
			{
				AssertType<MessageSendingActionCollection>(messageSendingActionParentForTest.GetSendingObjectsCollectionCoreExposed);
				AssertType<MessageSendingActionCollection>(messageSendingActionParentForTest.SendingObjectsCollection);
			});
		}

		public new void TestSecurityCheckpointToSendWithMessageError()
		{
			AssertEquals(messageSendingActionParent.SecurityCheckpointToSendWithMessageError, Env.Security.CustomsDeclarationSendWithMessageErrors);
		}

		public void TestMessageSendingObjectProperties()
		{
			var testItem = messageSendingActionParent.MessageSendingObjectProperties.ToList();

			CombineAssertions(() =>
			{
				AssertEquals("Count", 6, testItem.Count);
				AssertEquals("MessageSendingObjectProperties should contain \"IsTestDeclaration\"", true, testItem.Exists(item => item.PropertyName == "IsTestDeclaration"));
				AssertEquals("MessageSendingObjectProperties should contain \"AdditionalDeclarationType\"", true, testItem.Exists(item => item.PropertyName == "AdditionalDeclarationType"));
				AssertEquals("MessageSendingObjectProperties should contain \"LRN\"", true, testItem.Exists(item => item.PropertyName == "LRN"));
				AssertEquals("MessageSendingObjectProperties should contain \"MRN\"", true, testItem.Exists(item => item.PropertyName == "MRN"));
				AssertEquals("MessageSendingObjectProperties should contain \"EntryType\"", true, testItem.Exists(item => item.PropertyName == "EntryType"));
				AssertEquals("MessageSendingObjectProperties should contain \"EntryStatus\"", true, testItem.Exists(item => item.PropertyName == "EntryStatus"));
			});
		}

		protected override BusinessObject GetNewBusinessObject() => messageSendingActionParent;

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			messageSendingActionParent = new MessageSendingActionParentForTest(nctsHeader);
		}
		NctsHeader nctsHeader;
		MessageSendingActionParent messageSendingActionParent;
	}

	class MessageSendingActionParentForTest : MessageSendingActionParent
	{
		public MessageSendingActionParentForTest(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		public NonPersistentBusinessObjectCollection<NctsHeaderMessageSendingObject> GetSendingObjectsCollectionCoreExposed => GetSendingObjectsCollectionCore();
	}
}
