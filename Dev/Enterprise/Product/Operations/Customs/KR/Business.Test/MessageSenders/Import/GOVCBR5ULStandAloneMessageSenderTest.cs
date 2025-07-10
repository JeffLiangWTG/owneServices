using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5ULStandAloneMessageSenderTest : TestCaseWithFactory
	{
		GOVCBR5ULStandAloneSender GetMessageSender() => IsExceptionTest ? new GOVCBR5ULStandAloneSenderForTest(messageSendingObject, Factory) : new GOVCBR5ULStandAloneSender(messageSendingObject, Factory);

		public void TestSendMessage()
		{
			var cusEntryNumber = Factory.New<CusEntryNumber>();
			cusEntryNumber.CE_EntryIsSystemGenerated = true;
			cusEntryNumber.CE_ParentID = cusReconDeclaration.PK;
			cusEntryNumber.CE_ParentTable = CusReconDeclarationSchema.Constants.TableName;
			cusEntryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			cusEntryNumber.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			cusEntryNumber.CE_EntryNum = "6N0022400002U";
			GetMessageSender().Send();

			var message = cusReconDeclaration.Messages.Where(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5UL).Single();
			var entryNumber = cusReconDeclaration.RefundDeclarationNumber;
			AssertEquals(cusReconDeclaration.CRD_MessageStatus, CustomsMessageStatusTypeList.Codes.OriginalSent);
			AssertEquals("STA", message.EM_MessageSubType);
			AssertEquals(entryNumber, message.EM_ApplicationReference);
		}

		public void TestStatusIsUpdated()
		{
			GetMessageSender().Send();
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalSent, cusReconDeclaration.CRD_MessageStatus);
		}

		public void TestSendException()
		{
			IsExceptionTest = ZBool.True;
			var messages = cusReconDeclaration.Messages;
			AssertEquals("One message exits before sending a message.", 0, messages.Count);
			AssertExceptionThrown<Exception>(() => GetMessageSender().Send());
			AssertEquals("Exception occurred when sending a message, and no new message has been created.", 0, messages.Count);
		}
		public ZBool IsExceptionTest;

		protected override void SetUp()
		{
			base.SetUp();
			cusReconDeclaration = Factory.New<CusReconDeclaration>();
			messageSendingObject = new List<CusReconDeclaration> { cusReconDeclaration };
		}
		CusReconDeclaration cusReconDeclaration;
		IEnumerable<CusReconDeclaration> messageSendingObject;

		class GOVCBR5ULStandAloneSenderForTest : GOVCBR5ULStandAloneSender
		{
			public GOVCBR5ULStandAloneSenderForTest(IEnumerable<CusReconDeclaration> sendingObjects, BusinessObjectFactory factory) : base(sendingObjects, factory)
			{
			}

			protected override Import5ULHeader GetMessageDataProvider(CusReconDeclaration parent, ZString messageID) => throw new Exception();
		}
	}
}
