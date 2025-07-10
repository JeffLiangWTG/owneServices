using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	[TestedType(typeof(MessageSendingObjectParent<MessageSendingObject>))]
	sealed class MessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetSendingObjectsCollection()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "123";
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "321";
			var bill3 = header.Bills.AddNew();
			bill3.ABL_BillNumber = "456";
			bill3.EntrySummaryReferenceNumber = "123";
			bill3.ABL_IsActive = false;

			var sendingObjectParent = new MessageSendingObjectParent<MessageSendingObject>(header);

			CombineAssertions("Should create message sending object collection from bills", () =>
			{
				AssertEquals(2, sendingObjectParent.SendingObjectsCollection.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "123", "321" }, sendingObjectParent.SendingObjectsCollection.Select(x => x.BillNumber));
			});
		}

		public void TestOverrideMessageType()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			var sendingObjectParent = new MessageSendingObjectParent<MessageSendingObject>(header);
			sendingObjectParent.Action = "XXX";
			sendingObjectParent.OverrideDefaultAction = false;

			AssertEquals("Action should be reset to empty after OverrideDefaultAction is set as false", string.Empty, sendingObjectParent.Action);
		}

		public void TestMessageSendingObjectHasSameActionListAsParent()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var sendingObject = new MessageSendingObject(bill);
			var sendingObjectParent = new MessageSendingObjectParent<MessageSendingObject>(header);

			Assert("ActionList should be same between sendingObjectParent and sendingObject", sendingObjectParent.ActionList.Equals(sendingObject.ActionList));
		}

		public void TestSetMessageTypeOverrideForAllMessageSendingObjects()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			var sendingObjectParent = new MessageSendingObjectParent<MessageSendingObject>(header);

			CombineAssertions("Should be defaulted as empty string", () =>
			{
				AssertEquals(string.Empty, sendingObjectParent.SendingObjectsCollection[0].Action);
				AssertEquals(string.Empty, sendingObjectParent.SendingObjectsCollection[1].Action);
			});

			sendingObjectParent.Action = "XXX";

			CombineAssertions("Should be set as target action for all message sending objects", () =>
			{
				AssertEquals("XXX", sendingObjectParent.SendingObjectsCollection[0].Action);
				AssertEquals("XXX", sendingObjectParent.SendingObjectsCollection[1].Action);
			});
		}

		public void TestAmendmentReason_Attributes()
		{
			var sendingObjectParent = GetNewBusinessObject() as MessageSendingObjectParent<MessageSendingObject>;

			CombineAssertions(() =>
			{
				var resourceStringDataAttribute = sendingObjectParent.AmendmentReasonInfo.GetAttribute<ResourceStringDataAttribute>();
				AssertNotNull("ResourceString should be defined", resourceStringDataAttribute);
				AssertEquals("Expected Caption", "Amendment Reason", resourceStringDataAttribute.Caption);

				var listAttribute = sendingObjectParent.AmendmentReasonInfo.GetAttribute<ListAttribute>();
				AssertNotNull("List should be defined", listAttribute);
				AssertEquals("Expected List", "AmendmentReasonList", listAttribute.ListDataSourceMember);
			});
		}

		public void TestAmendmentReason_ReadOnly()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			var sendingObjectParent = new MessageSendingObjectParent<MessageSendingObjectForTest>(header);
			var sendingObject1 = sendingObjectParent.SendingObjectsCollection[0];

			CombineAssertions(() =>
			{
				sendingObjectParent.OverrideAmendmentReason = false;
				Assert("Should be true when OverrideAmendmentReason is false", sendingObjectParent.AmendmentReasonInfo.ReadOnly);

				sendingObjectParent.OverrideAmendmentReason = true;
				Assert("Should be true when no H7M message exists", sendingObjectParent.AmendmentReasonInfo.ReadOnly);

				sendingObject1.Action = "H7M";
				Assert("Should be false when an H7M message exists and OverrideAmendmentReason is true", !sendingObjectParent.AmendmentReasonInfo.ReadOnly);
			});
		}

		public void TestAmendmentReason_Setter_ShouldOverrideAmendmentMessages()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var bill3 = header.Bills.AddNew();
			var bill4 = header.Bills.AddNew();

			var sendingObjectParent = new MessageSendingObjectParent<MessageSendingObjectForTest>(header);

			var sendingObject1 = sendingObjectParent.SendingObjectsCollection[0];
			sendingObject1.Action = "H7M";
			sendingObject1.AmendmentReasonCode = "A1";

			var sendingObject2 = sendingObjectParent.SendingObjectsCollection[1];
			sendingObject2.Action = "H7M";
			sendingObject2.AmendmentReasonCode = "A2";

			var sendingObject3 = sendingObjectParent.SendingObjectsCollection[2];
			sendingObject3.Action = "H7C";
			sendingObject3.AmendmentReasonCode = "C1";

			sendingObjectParent.OverrideAmendmentReason = true;
			sendingObjectParent.AmendmentReason = "AA";

			CombineAssertions(() =>
			{
				AssertEquals("Should override H7M message", "AA", sendingObjectParent.SendingObjectsCollection[0].AmendmentReasonCode);
				AssertEquals("Should override H7M message", "AA", sendingObjectParent.SendingObjectsCollection[1].AmendmentReasonCode);
				AssertEquals("Should not override H7C message", "C1", sendingObjectParent.SendingObjectsCollection[2].AmendmentReasonCode);
				AssertEquals("Should not override other messages", string.Empty, sendingObjectParent.SendingObjectsCollection[3].AmendmentReasonCode);
			});
		}

		public void TestAmendmentReasonList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var bill3 = header.Bills.AddNew();

			var sendingObjectParent = new MessageSendingObjectParent<MessageSendingObjectForTest>(header);

			var sendingObject1 = sendingObjectParent.SendingObjectsCollection[0];
			var sendingObject2 = sendingObjectParent.SendingObjectsCollection[1];
			sendingObject2.Action = "H7C";

			CombineAssertions(() =>
			{
				AssertEquals("Should be empty when there is no Amendment message", 0, sendingObjectParent.AmendmentReasonList.Count);

				sendingObject1.Action = "H7M";
				AssertArrayEqualsByElements("Should include Amendment codes if there is an Amendment message", ["A1", "A2"], sendingObjectParent.AmendmentReasonList.GetAllCodesZString());
			});
		}

		public void TestOverrideAmendmentReason_Caption()
		{
			var sendingObjectParent = GetNewBusinessObject() as MessageSendingObjectParent<MessageSendingObject>;
			var resourceStringDataAttribute = sendingObjectParent.OverrideAmendmentReasonInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("Override Amendment Reason default values", resourceStringDataAttribute.Caption);
		}

		public void TestOverrideAmendmentReason_WhenFalse_ShouldClearAmendmentReason()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();

			var sendingObjectParent = new MessageSendingObjectParent<MessageSendingObjectForTest>(header);
			var sendingObject1 = sendingObjectParent.SendingObjectsCollection[0];
			sendingObject1.Action = "H7M";

			sendingObjectParent.OverrideAmendmentReason = true;
			sendingObjectParent.AmendmentReason = "A1";

			CombineAssertions(() =>
			{
				AssertEquals("Precondition", "A1", sendingObjectParent.AmendmentReason);

				sendingObjectParent.OverrideAmendmentReason = false;
				AssertEquals("Should be cleared", string.Empty, sendingObjectParent.AmendmentReason);
			});
		}

		public void TestOverrideAmendmentReason_ReadOnly()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			var sendingObjectParent = new MessageSendingObjectParent<MessageSendingObjectForTest>(header);
			var sendingObject1 = sendingObjectParent.SendingObjectsCollection[0];

			CombineAssertions(() =>
			{
				Assert("Should be true when no H7M message exists", sendingObjectParent.OverrideAmendmentReasonInfo.ReadOnly);

				sendingObject1.Action = "H7M";
				Assert("Should be false when any H7M message exists", !sendingObjectParent.OverrideAmendmentReasonInfo.ReadOnly);

				sendingObjectParent.OverrideAmendmentReason = true;
				sendingObject1.Action = "H7C";
				Assert("Should be true when no H7M message exists", sendingObjectParent.OverrideAmendmentReasonInfo.ReadOnly);
				Assert("OverrideAmendmentReason should be false", !sendingObjectParent.OverrideAmendmentReason);
			});
		}

		public void TestCancellationReason_Attributes()
		{
			var sendingObjectParent = GetNewBusinessObject() as MessageSendingObjectParent<MessageSendingObject>;

			CombineAssertions(() =>
			{
				var resourceStringDataAttribute = sendingObjectParent.CancellationReasonInfo.GetAttribute<ResourceStringDataAttribute>();
				AssertNotNull("ResourceString should be defined", resourceStringDataAttribute);
				AssertEquals("Expected Caption", "Cancellation Reason", resourceStringDataAttribute.Caption);

				var listAttribute = sendingObjectParent.CancellationReasonInfo.GetAttribute<ListAttribute>();
				AssertNotNull("List should be defined", listAttribute);
				AssertEquals("Expected List", "CancellationReasonList", listAttribute.ListDataSourceMember);
			});
		}

		public void TestCancellationReason_ReadOnly()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			var sendingObjectParent = new MessageSendingObjectParent<MessageSendingObjectForTest>(header);
			var sendingObject1 = sendingObjectParent.SendingObjectsCollection[0];

			CombineAssertions(() =>
			{
				sendingObjectParent.OverrideCancellationReason = false;
				Assert("Should be true when OverrideCancellationReason is false", sendingObjectParent.CancellationReasonInfo.ReadOnly);

				sendingObjectParent.OverrideCancellationReason = true;
				Assert("Should be false when no H7C message exists", sendingObjectParent.CancellationReasonInfo.ReadOnly);

				sendingObject1.Action = "H7C";
				Assert("Should be false when an H7C message exists and OverrideCancellationReason is true", !sendingObjectParent.CancellationReasonInfo.ReadOnly);
			});
		}

		public void TestCancellationReason_Setter_ShouldOverrideCancellationMessages()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var bill3 = header.Bills.AddNew();
			var bill4 = header.Bills.AddNew();

			var sendingObjectParent = new MessageSendingObjectParent<MessageSendingObjectForTest>(header);

			var sendingObject1 = sendingObjectParent.SendingObjectsCollection[0];
			sendingObject1.Action = "H7C";
			sendingObject1.AmendmentReasonCode = "C1";

			var sendingObject2 = sendingObjectParent.SendingObjectsCollection[1];
			sendingObject2.Action = "H7C";
			sendingObject2.AmendmentReasonCode = "C2";

			var sendingObject3 = sendingObjectParent.SendingObjectsCollection[2];
			sendingObject3.Action = "H7M";
			sendingObject3.AmendmentReasonCode = "A1";

			sendingObjectParent.CancellationReason = "CC";

			CombineAssertions(() =>
			{
				AssertEquals("Should override H7C message", "CC", sendingObjectParent.SendingObjectsCollection[0].AmendmentReasonCode);
				AssertEquals("Should override H7C message", "CC", sendingObjectParent.SendingObjectsCollection[1].AmendmentReasonCode);
				AssertEquals("Should not override H7M message", "A1", sendingObjectParent.SendingObjectsCollection[2].AmendmentReasonCode);
				AssertEquals("Should not override other messages", string.Empty, sendingObjectParent.SendingObjectsCollection[3].AmendmentReasonCode);
			});
		}

		public void TestCancellationReasonList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var bill3 = header.Bills.AddNew();

			var sendingObjectParent = new MessageSendingObjectParent<MessageSendingObjectForTest>(header);

			var sendingObject1 = sendingObjectParent.SendingObjectsCollection[0];
			var sendingObject2 = sendingObjectParent.SendingObjectsCollection[1];
			sendingObject2.Action = "H7M";

			CombineAssertions(() =>
			{
				AssertEquals("Should be empty when there is no Cancellation message", 0, sendingObjectParent.CancellationReasonList.Count);

				sendingObject1.Action = "H7C";
				AssertArrayEqualsByElements("Should include Cancellation codes if there is a Cancellation message", ["C1", "C2"], sendingObjectParent.CancellationReasonList.GetAllCodesZString());
			});
		}

		public void TestOverrideCancellationReason_Caption()
		{
			var sendingObjectParent = GetNewBusinessObject() as MessageSendingObjectParent<MessageSendingObject>;
			var resourceStringDataAttribute = sendingObjectParent.OverrideCancellationReasonInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("Override Cancellation Reason default values", resourceStringDataAttribute.Caption);
		}

		public void TestOverrideCancellationReason_WhenFalse_ShouldClearCancellationReason()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();

			var sendingObjectParent = new MessageSendingObjectParent<MessageSendingObjectForTest>(header);
			var sendingObject1 = sendingObjectParent.SendingObjectsCollection[0];
			sendingObject1.Action = "H7C";

			sendingObjectParent.OverrideCancellationReason = true;
			sendingObjectParent.CancellationReason = "C1";

			CombineAssertions(() =>
			{
				AssertEquals("Precondition", "C1", sendingObjectParent.CancellationReason);

				sendingObjectParent.OverrideCancellationReason = false;
				AssertEquals("Should be cleared", string.Empty, sendingObjectParent.CancellationReason);
			});
		}

		public void TestOverrideCancellationReason_ReadOnly()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			var sendingObjectParent = new MessageSendingObjectParent<MessageSendingObjectForTest>(header);
			var sendingObject1 = sendingObjectParent.SendingObjectsCollection[0];

			CombineAssertions(() =>
			{
				Assert("Should be true when no H7C message exists", sendingObjectParent.OverrideCancellationReasonInfo.ReadOnly);

				sendingObject1.Action = "H7C";
				Assert("Should be false when any H7C message exists", !sendingObjectParent.OverrideCancellationReasonInfo.ReadOnly);

				sendingObjectParent.OverrideCancellationReason = true;
				sendingObject1.Action = "H7M";
				Assert("Should be true when no H7C message exists", sendingObjectParent.OverrideCancellationReasonInfo.ReadOnly);
				Assert("OverrideCancellationReason should be false", !sendingObjectParent.OverrideCancellationReason);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			return new MessageSendingObjectParent<MessageSendingObject>(header);
		}
	}
}
