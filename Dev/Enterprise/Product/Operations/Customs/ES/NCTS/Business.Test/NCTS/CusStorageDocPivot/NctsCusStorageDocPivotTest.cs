using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	[TestedType(typeof(NctsCusStorageDocPivot))]
	class NctsCusStorageDocPivotTest : Customs.Business.Testing.BaseCusStorageDocPivotTest
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreatePivot(nctsHeader, "TY1", ZGuid.Empty);

		public override void TestParent()
		{
			var pivot = nctsHeader.EDocPivotCollection.AddNew();
			AssertEquals(nctsHeader, pivot.Parent);
		}

		public void TestMessage()
		{
			var pivot = CreatePivot(nctsHeader, "TY1", ZGuid.Empty);

			CombineAssertions(() =>
			{
				AssertNull("Message is null", pivot.Message);

				var message = SetEDIMessageAndGenPivot(nctsHeader, pivot);
				Factory.Save();

				AssertNotNull("Message is not null (with one edimessage)", pivot.Message);
				AssertEquals("Message PK is correct (message1)", message.PK, pivot.Message.PK);

				// 5ms sleep to ensure message2 create time is after message1's, taking into account SQL datetime precision (3ms).
				Thread.Sleep(5);

				var message2 = SetEDIMessageAndGenPivot(nctsHeader, pivot);
				Factory.Save();

				AssertNotNull("Message is not null (with 2 edimessages)", pivot.Message);
				AssertEquals("Message PK is correct (message2)", message2.PK, pivot.Message.PK);
			});
		}

		public void TestMessageStatus()
		{
			var pivot = CreatePivot(nctsHeader, "TY1", ZGuid.Empty);
			CombineAssertions(() =>
			{
				AssertEquals("MessageStatus is empty", ZString.Empty, pivot.MessageStatus);

				var message = SetEDIMessageAndGenPivot(nctsHeader, pivot);
				message.EM_Status = EDIMessageStatusList.Codes.Error;
				Factory.Save();

				AssertEquals("MessageStatus is not empty", EDIMessageStatusList.Codes.Error, pivot.MessageStatus);
			});
		}

		public void TestIsReadOnly()
		{
			var pivot = CreatePivot(nctsHeader, "TY1", ZGuid.Empty);
			CombineAssertions(() =>
			{
				AssertEquals("CusStorageDocPivot is not readonly when there are no messages associated to the pivot", false, pivot.ReadOnly);

				var message = SetEDIMessageAndGenPivot(nctsHeader, pivot);
				message.EM_Status = EDIMessageStatusList.Codes.Received;
				Factory.Save();

				AssertEquals("CusStorageDocPivot is readonly when there is a message associated to the pivot and message status is received", true, pivot.ReadOnly);

				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message.EM_Status = EDIMessageStatusList.Codes.Error;

				AssertEquals("CusStorageDocPivot is not readonly when there is a message associated to the pivot, EM_ReceiveTransmit is TRX but message status is different from received or awaiting repsonse", false, pivot.ReadOnly);

				message.EM_Status = EDIMessageStatusList.Codes.Sent;

				AssertEquals("CusStorageDocPivot is readonly when there is a message associated to the pivot, EM_ReceiveTransmit is TRX and status is awaiting response", true, pivot.ReadOnly);

				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

				AssertEquals("CusStorageDocPivot is not readonly when there is a message associated to the pivot but EM_ReceiveTransmit is not TRX", false, pivot.ReadOnly);
			});
		}

		public void TestLookups()
		{
			var pivot = CreatePivot(nctsHeader, "TY1", ZGuid.Empty);
			AssertType<NctsCusStorageDocPivotLookups>("Lookups Type", pivot.Lookups);
		}

		NctsCusStorageDocPivot CreatePivot(BusinessObject parent, string docType, ZGuid reference)
		{
			var result = ((INctsCusStorageDocPivotParent)parent).EDocPivotCollection.AddNew();
			result.CSD_DocType = docType;
			result.CSD_StorageDocReference = reference;
			return result;
		}

		ESEDIMessage SetEDIMessageAndGenPivot(NctsHeader nctsHeader, NctsCusStorageDocPivot pivot)
		{
			var message = Factory.New<ESEDIMessage>();
			message.EM_MessageType = DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes;
			message.EM_MessageSubType = DeclarationMessageSubTypeList.Codes.OriginalDeclaration;
			nctsHeader.Messages.Add(message);
			var messagePivot = Factory.New<GenPivot>();
			messagePivot.XX_RelationType = GenPivotTypes.CusStorageDocPivotEdiMessage;
			messagePivot.XX_Relation1ID = pivot.PK;
			messagePivot.XX_Relation1TableCode = pivot.TablePrefix;
			messagePivot.XX_Relation2ID = message.PK;
			messagePivot.XX_Relation2TableCode = message.TablePrefix;

			return message;
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			Factory.Save();
		}
		NctsHeader nctsHeader;
	}
}
