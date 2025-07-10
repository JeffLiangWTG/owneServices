using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	[TestedType(typeof(NctsHeaderMessageSendingObjectParent))]
	sealed class NctsHeaderMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMessageSendingObjectProperties()
		{
			var sendingObjectParent = new NctsHeaderMessageSendingObjectParent(sendingObject);
			var sendingObjectPropertyNames = sendingObjectParent.MessageSendingObjectProperties.Select(x => x.PropertyName).ToArray();
			AssertArrayEqualsByElements("MessageSendingObjectProperties", ["LRN", "MRN", "MessageType", "Justification", "CustomsStatus", "MessageStatus", "MessageSubType"], sendingObjectPropertyNames);
		}

		public void TestSendingObjectsCollection()
		{
			var sendingObjectParent = new NctsHeaderMessageSendingObjectParent(sendingObject);
			var sendingObjectsCollection = sendingObjectParent.SendingObjectsCollection;
			AssertType<NctsHeaderMessageSendingObjectCollection>("SendingObjectsCollection Type", sendingObjectsCollection);
			AssertEquals("SendingObjectsCollection Count", 1, sendingObjectsCollection.Count);
			AssertType<NctsHeaderMessageSendingObject>("SendingObject Type", sendingObjectsCollection[0]);
		}

		public void TestMessageErrorCollector()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var sendingObject = new NctsMessageSendingObject(nctsHeader, GlbStaff.CurrentUser);

			var targetInfo = nctsHeader.ESNctsHeader.CEN_SummaryTypeInfo;
			CombineAssertions(() =>
			{
				nctsHeader.RunPreSaveValidation();
				AssertEquals("NctsHeader has MessageErrors", expected: true, nctsHeader.HasMessageErrors());
				AssertEquals("CEN_SummaryTypeInfo has MessageErrors", expected: true, targetInfo.HasMessageErrors());
				var messageSendingObjectParent = new NctsHeaderMessageSendingObjectParent(sendingObject);
				messageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				AssertContains("CEN_SummaryTypeInfo MessageError is collected and displayed", targetInfo.HumanReadableName + ": " + targetInfo.GetMessageErrors().First().Message, messageSendingObjectParent.BizObjValidationMessageErrors);

				nctsHeader.RunPreSaveValidation();
				AssertEquals("NctsHeader has MessageErrors", expected: true, nctsHeader.HasMessageErrors());
				AssertEquals("CEN_SummaryTypeInfo has MessageErrors", expected: true, targetInfo.HasMessageErrors());
				messageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = false;
				AssertNullOrEmpty("BizObjValidationMessageErrors empty", messageSendingObjectParent.BizObjValidationMessageErrors);
			});
		}

		public void TestMessageErrorCollector_TNN()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.ESNctsHeader.CEN_TNNArrival = true;
			var nctsHeaderTNN = Factory.New<NctsHeader>();
			nctsHeaderTNN.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderTNN.ESNctsHeader.CEN_TNNArrival = true;
			var tnnMovement = nctsHeaderTNN.MovementHeader;
			tnnMovement.BM_Phase = DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration;

			nctsHeader.ArrivalMovementHeader.BM_BM_DepartureMovement = tnnMovement.PK;

			var sendingObject = new NctsMessageSendingObject(nctsHeader, GlbStaff.CurrentUser);

			var targetInfo = nctsHeader.ESNctsHeader.CEN_SummaryTypeInfo;
			CombineAssertions(() =>
			{
				nctsHeader.RunPreSaveValidation();
				AssertEquals("NctsHeader has MessageErrors", expected: true, nctsHeader.HasMessageErrors());
				AssertEquals("CEN_SummaryTypeInfo has MessageErrors", expected: true, targetInfo.HasMessageErrors());
				var messageSendingObjectParent = new NctsHeaderMessageSendingObjectParent(sendingObject);
				messageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				AssertNotContains("CEN_SummaryTypeInfo MessageError is not collected and displayed since it is an arrival error and we are sending TNN (departure)", targetInfo.HumanReadableName + ": " + targetInfo.GetMessageErrors().First().Message, messageSendingObjectParent.BizObjValidationMessageErrors);

				nctsHeader.RunPreSaveValidation();
				AssertEquals("NctsHeader has MessageErrors", expected: true, nctsHeader.HasMessageErrors());
				AssertEquals("CEN_SummaryTypeInfo has MessageErrors", expected: true, targetInfo.HasMessageErrors());
				messageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = false;
				AssertNullOrEmpty("BizObjValidationMessageErrors empty", messageSendingObjectParent.BizObjValidationMessageErrors);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => new NctsHeaderMessageSendingObjectParent(sendingObject);

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			sendingObject = new NctsMessageSendingObject(nctsHeader, GlbStaff.CurrentUser);
		}

		NctsMessageSendingObject sendingObject;
	}
}
