using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.FR.H7.Business.Testing
{
	[TestedType(typeof(MessageSendingObject))]
	sealed class MessageSendingObjectTest : EU.H7.Business.Testing.MessageSendingObjectTest
	{
		public void TestDefaultAction()
		{
			var messageSendingObject = GetNewBusinessObject() as MessageSendingObject;

			AssertEquals("Default action should be H7D", H7MessageTypeList.Codes.H7Declaration, messageSendingObject.Action);
		}

		public void TestAmendmentInvalidationReasonIsEnabledOnlyForH7MAndH7C()
		{
			var messageSendingObject = GetNewBusinessObject() as MessageSendingObject;

			AssertNullOrEmpty("Should default to empty", messageSendingObject.AmendmentInvalidationReason);
			TestPropertyAvailability(messageSendingObject, messageSendingObject.AmendmentInvalidationReasonInfo, H7MessageTypeList.Codes.H7Amendment, H7MessageTypeList.Codes.H7Invalidation);
		}

		public void TestMotivationIsEnabledOnlyForH7MAndH7C()
		{
			var messageSendingObject = GetNewBusinessObject() as MessageSendingObject;

			AssertNullOrEmpty("Should default to empty", messageSendingObject.Motivation);
			TestPropertyAvailability(messageSendingObject, messageSendingObject.MotivationInfo, H7MessageTypeList.Codes.H7Amendment, H7MessageTypeList.Codes.H7Invalidation);
		}

		public void TestMotivationList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("H7INV", "H7 - Invalidation Motivation");
			helper.CreateCusCodeType("H7AMD", "H7 - Amendment Motivation");
			helper.CreateCusCodeList("DIE", "H7INV", "INV99", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("FR", "H7INV", "INV00", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("DIE", "H7AMD", "REC99", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("FR", "H7AMD", "REC00", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			Factory.Save();

			var messageSendingObject = GetNewBusinessObject() as MessageSendingObject;
			messageSendingObject.Action = H7MessageTypeList.Codes.H7Amendment;
			AssertEquals("H7AMD list for H7M action", "REC00", messageSendingObject.MotivitationList.GetAllCodes().Single());

			messageSendingObject.Action = H7MessageTypeList.Codes.H7Invalidation;
			AssertEquals("INV99 list for H7M action", "INV00", messageSendingObject.MotivitationList.GetAllCodes().Single());
		}

		public void TestManifestLodgementDateTimeEnabledOnlyForH7F()
		{
			var messageSendingObject = GetNewBusinessObject() as MessageSendingObject;

			AssertEquals("Should default to empty", true, messageSendingObject.ManifestLodgementDateTime.IsEmpty);
			TestPropertyAvailability(messageSendingObject, messageSendingObject.ManifestLodgementDateTimeInfo, H7MessageTypeList.Codes.H7Fallback);
		}

		public void TestFallbackReferenceNumberEnabledOnlyForH7F()
		{
			var messageSendingObject = GetNewBusinessObject() as MessageSendingObject;

			AssertNullOrEmpty("Should default to empty", messageSendingObject.FallbackReferenceNumber);
			TestPropertyAvailability(messageSendingObject, messageSendingObject.FallbackReferenceNumberInfo, H7MessageTypeList.Codes.H7Fallback);
		}

		void TestPropertyAvailability(MessageSendingObject messageSendingObject, ZPropertyInfo propertyInfo, params string[] enabledActionType)
		{
			foreach (var action in new H7MessageTypeList().GetAllCodes())
			{
				messageSendingObject.Action = action;

				if (enabledActionType.Contains(action))
				{
					AssertEquals($"{propertyInfo.Name} should be enabled for action {action}", false, propertyInfo.ReadOnly);
				}
				else
				{
					AssertEquals($"{propertyInfo.Name} should be read-only for action {action}", true, propertyInfo.ReadOnly);
				}
			}
		}

		public void TestFRMessageSendingObjectPropertiesMaxLength()
		{
			var messageSendingObject = GetNewBusinessObject() as MessageSendingObject;
			AssertEquals("Max length of Motivation is 5", 5, messageSendingObject.MotivationInfo.MaxLength);
			AssertEquals("Max length of FallbackReferenceNumber is 5", 10, messageSendingObject.FallbackReferenceNumberInfo.MaxLength);
		}

		protected override int ExpectedMaxLengthOfAmendmentInvalidationReason => 350;

		protected override CodeDescriptionPairList ExpectedActionList => new H7MessageTypeList();

		protected override CodeDescriptionPairList ExpectedSubStyleList => new CodeDescriptionPairList();

		protected override EU.H7.Business.MessageSendingObject GetMessageSendingObject(EU.H7.Business.AsycudaBill bill)
		{
			return new MessageSendingObject((H7Bill)bill);
		}

		protected override EU.H7.Business.AsycudaBill GetMessageSendingObjectParentBill()
		{
			var header = Factory.New<H7ManifestHeader>();

			return header.Bills.AddNew();
		}
	}
}
