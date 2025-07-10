using System;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.ES.Business.ESConstants;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.ES.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(CusStorageDocPivot))]
	public class CusStorageDocPivotTest : Customs.Business.Testing.BaseCusStorageDocPivotTest
	{
		public void TestConflictResolution_Entry()
		{
			Factory.RefreshEnabled = false;
			var anotherFactory = new BusinessObjectFactory { RefreshEnabled = false };

			((IBusinessObjectFactoryInternals)anotherFactory).DisableQueryCacheReset = true;

			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");

			declaration.DocManagerInfo.Save();
			Factory.Save();

			var pivot = CreatePivot(entryHeader, "TY1", eDoc.UniqueKey);
			var declarationInAnotherFactory = anotherFactory.Load<JobDeclaration>(declaration.PK);
			var entryInAnotherFactory = declarationInAnotherFactory.CustomsEntryHeaders[0];
			var pivotInAnotherFactory = CreatePivot(entryInAnotherFactory, "TY1", declarationInAnotherFactory.DocManagerInfo.AllEDocs.GetFromUniqueKey(eDoc.UniqueKey.ToGuid()).UniqueKey);

			Factory.Save();

			((IBusinessObjectFactoryInternals)anotherFactory).DisableQueryCacheReset = false;

			CombineAssertions(() =>
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				try
				{
					anotherFactory.Save();
					Fail("First save should not have succeeded.");
				}
				catch (Exception ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}

				AssertEquals("User should have been notified", false, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("Message to notify user", $"The type 'PDF' eDoc (Invoice.pdf) has already been linked to {entryHeader.HumanReadableName} by another user. Your changes have been merged, please review your changes and save again.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Pivot 2 should be deleted", true, pivotInAnotherFactory.IsDeleted);

				anotherFactory.Save();

				AssertEquals("Should be using the existing Pivot now", pivot.PK, entryInAnotherFactory.EDocPivotCollection.Single().PK);
			});
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreatePivot(entryHeader, "TY1", ZGuid.Empty);

		public override void TestParent()
		{
			var pivot = entryHeader.EDocPivotCollection.AddNew();
			AssertEquals(entryHeader, pivot.Parent);
		}

		public void TestMessage()
		{
			var pivot = CreatePivot(entryHeader, "TY1", ZGuid.Empty);

			CombineAssertions(() =>
			{
				AssertNull("Message is null", pivot.Message);

				var message = SetEDIMessageAndGenPivot(entryHeader, pivot);
				Factory.Save();

				AssertNotNull("Message is not null (with one edimessage)", pivot.Message);
				AssertEquals("Message PK is correct (message1)", message.PK, pivot.Message.PK);

				// 5ms sleep to ensure message2 create time is after message1's, taking into account SQL datetime precision (3ms).
				Thread.Sleep(5);

				var message2 = SetEDIMessageAndGenPivot(entryHeader, pivot);
				Factory.Save();

				AssertNotNull("Message is not null (with 2 edimessages)", pivot.Message);
				AssertEquals("Message PK is correct (message2)", message2.PK, pivot.Message.PK);
			});
		}

		public void TestDocument()
		{
			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var pivot = CreatePivot(entryHeader, "CIV", eDoc.UniqueKey);
			AssertEquals(eDoc, pivot.Document);
		}

		public void TestDocumentSize()
		{
			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var pivot = CreatePivot(entryHeader, "CIV", eDoc.UniqueKey);
			AssertEquals("1B", pivot.DocumentSize);
		}

		public void TestDocumentExtension()
		{
			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var pivot = CreatePivot(entryHeader, "CIV", eDoc.UniqueKey);
			AssertEquals("PDF", pivot.DocumentExtension);
		}

		public void TestMessageStatus()
		{
			var pivot = CreatePivot(entryHeader, "TY1", ZGuid.Empty);
			CombineAssertions(() =>
			{
				AssertEquals("MessageStatus is empty", ZString.Empty, pivot.MessageStatus);

				var message = SetEDIMessageAndGenPivot(entryHeader, pivot);
				message.EM_Status = EDIMessageStatusList.Codes.Error;
				Factory.Save();

				AssertEquals("MessageStatus is not empty", EDIMessageStatusList.Codes.Error, pivot.MessageStatus);
			});
		}

		public void TestIsReadOnly_T2LEntry()
		{
			var pivot = CreatePivot(entryHeader, "TY1", ZGuid.Empty);
			CombineAssertions(() =>
			{
				AssertEquals("CusStorageDocPivot is not readonly when there are no messages associated to the pivot", false, pivot.ReadOnly);

				var message = SetEDIMessageAndGenPivot(entryHeader, pivot);
				message.EM_Status = EDIMessageStatusList.Codes.Received;
				Factory.Save();

				AssertEquals("CusStorageDocPivot is readonly when there is a message associated to the pivot and message status is received", true, pivot.ReadOnly);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				AssertEquals("CusStorageDocPivot is not readonly when there is a message associated to the pivot and message status is received but entry's CEI_SubStyle is A", false, pivot.ReadOnly);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				AssertEquals("CusStorageDocPivot is readonly when there is a message associated to the pivot and message status is received and entry's CEI_SubStyle is T2L", true, pivot.ReadOnly);

				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message.EM_Status = EDIMessageStatusList.Codes.Error;

				AssertEquals("CusStorageDocPivot is not readonly when there is a message associated to the pivot, EM_ReceiveTransmit is TRX but message status is different from received or awaiting repsonse", false, pivot.ReadOnly);

				message.EM_Status = EDIMessageStatusList.Codes.Sent;

				AssertEquals("CusStorageDocPivot is readonly when there is a message associated to the pivot, EM_ReceiveTransmit is TRX and status is awaiting response", true, pivot.ReadOnly);

				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

				AssertEquals("CusStorageDocPivot is not readonly when there is a message associated to the pivot but EM_ReceiveTransmit is not TRX", false, pivot.ReadOnly);
			});
		}

		public void TestIsReadOnly_ExportUCC6Entry()
		{
			var pivot = CreatePivot(entryHeader, "TY1", ZGuid.Empty);
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			entryHeader.ZG_UCC6Version = 1;
			entryHeader.MovementReferenceNumber = "MRN-TEST";

			CombineAssertions(() =>
			{
				AssertEquals("CusStorageDocPivot is not readonly when there are no messages associated to the pivot", false, pivot.ReadOnly);

				var message = SetEDIMessageAndGenPivot(entryHeader, pivot);
				message.EM_Status = EDIMessageStatusList.Codes.Received;
				Factory.Save();

				AssertEquals("CusStorageDocPivot is readonly when there is a message associated to the pivot and message status is received", true, pivot.ReadOnly);

				entryHeader.ZG_UCC6Version = 0;
				AssertEquals("CusStorageDocPivot is not readonly when there is a message associated to the pivot and message status is received but entry's UCC6Version = 0", false, pivot.ReadOnly);

				entryHeader.ZG_UCC6Version = 1;
				AssertEquals("CusStorageDocPivot is readonly when there is a message associated to the pivot and message status is received and entry's UCC6Version = 1", true, pivot.ReadOnly);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				AssertEquals("CusStorageDocPivot is not readonly when there is a message associated to the pivot and message status is received but entry's CEI_SubStyle is T2C", false, pivot.ReadOnly);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				AssertEquals("CusStorageDocPivot is readonly when there is a message associated to the pivot and message status is received and entry's CEI_SubStyle is A", true, pivot.ReadOnly);

				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				AssertEquals("CusStorageDocPivot is not readonly when there is a message associated to the pivot and message status is received but JE_MessageType is import", false, pivot.ReadOnly);

				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
				AssertEquals("CusStorageDocPivot is readonly when there is a message associated to the pivot and message status is received and JE_MessageType is export", true, pivot.ReadOnly);

				entryHeader.MovementReferenceNumber = ZString.Empty;
				AssertEquals("CusStorageDocPivot is not readonly when there is a message associated to the pivot and message status is received but entry's MRN is empty", false, pivot.ReadOnly);

				entryHeader.MovementReferenceNumber = "MRN-TEST";
				AssertEquals("CusStorageDocPivot is readonly when there is a message associated to the pivot and message status is received and entry's MRN is not empty", true, pivot.ReadOnly);

				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message.EM_Status = EDIMessageStatusList.Codes.Error;

				AssertEquals("CusStorageDocPivot is not readonly when there is a message associated to the pivot, EM_ReceiveTransmit is TRX but message status is different from received or awaiting repsonse", false, pivot.ReadOnly);

				message.EM_Status = EDIMessageStatusList.Codes.Sent;

				AssertEquals("CusStorageDocPivot is readonly when there is a message associated to the pivot, EM_ReceiveTransmit is TRX and status is awaiting response", true, pivot.ReadOnly);

				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

				AssertEquals("CusStorageDocPivot is not readonly when there is a message associated to the pivot but EM_ReceiveTransmit is not TRX", false, pivot.ReadOnly);
			});
		}

		public void TestIsAccepted()
		{
			var pivot = CreatePivot(entryHeader, "TY1", ZGuid.Empty);
			CombineAssertions(() =>
			{
				AssertEquals("IsAccepted is false when there are no messages associated to the pivot", false, pivot.IsAccepted);

				var message = SetEDIMessageAndGenPivot(entryHeader, pivot);
				Factory.Save();

				AssertEquals("IsAccepted is false when there is a message associated to the pivot but message status is empty", false, pivot.IsAccepted);

				message.EM_Status = EDIMessageStatusList.Codes.Received;

				AssertEquals("IsAccepted is true when there is a message associated to the pivot and message status is received", true, pivot.IsAccepted);

				message.EM_Status = EDIMessageStatusList.Codes.Rejected;

				AssertEquals("IsAccepted is false when there is a message associated to the pivot but message status is different from received", false, pivot.IsAccepted);
			});
		}

		public void TestIsSentWithoutResponse()
		{
			var pivot = CreatePivot(entryHeader, "TY1", ZGuid.Empty);
			CombineAssertions(() =>
			{
				AssertEquals("IsSentWithoutResponse is false when there are no messages associated to the pivot", false, pivot.IsSentWithoutResponse);

				var message = SetEDIMessageAndGenPivot(entryHeader, pivot);
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				Factory.Save();

				AssertEquals("IsSentWithoutResponse is false when there is a message associated to the pivot but EM_ReceiveTransmit is not TRX", false, pivot.IsSentWithoutResponse);

				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message.EM_Status = EDIMessageStatusList.Codes.Sent;

				AssertEquals("IsSentWithoutResponse is true when there is a message associated to the pivot, EM_ReceiveTransmit is TRX and status is awaiting response", true, pivot.IsSentWithoutResponse);

				message.EM_Status = EDIMessageStatusList.Codes.Rejected;

				AssertEquals("IsSentWithoutResponse is false when there is a message associated to the pivot, EM_ReceiveTransmit is TRX but status is not awaiting response (REJ)", false, pivot.IsSentWithoutResponse);

				message.EM_Status = "AAA";

				AssertEquals("IsSentWithoutResponse is true when there is a message associated to the pivot, EM_ReceiveTransmit is TRX and status is not an error status", true, pivot.IsSentWithoutResponse);

				message.EM_Status = MessageStatusList.Codes.FailedFromTransmission;

				AssertEquals("IsSentWithoutResponse is false when there is a message associated to the pivot, EM_ReceiveTransmit is TRX and status is but awaiting response (FFT)", false, pivot.IsSentWithoutResponse);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge failed", true, mergeResult);
			Factory.Save();

			entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.ZG_POUSVersion = POUSVersionCodes.POUS;
			entryHeader.MovementReferenceNumber = "MRN-TEST";
		}
		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusEntryHeader entryHeader;

		static CusStorageDocPivot CreatePivot(BusinessObject parent, string docType, ZGuid reference)
		{
			var result = ((ICusStorageDocPivotParent)parent).EDocPivotCollection.AddNew();
			result.CSD_DocType = docType;
			result.CSD_StorageDocReference = reference;
			return result;
		}

		ESEDIMessage SetEDIMessageAndGenPivot(CusEntryHeader entryHeader, CusStorageDocPivot pivot)
		{
			var message = Factory.New<ESEDIMessage>();
			message.EM_MessageType = DeclarationMessageTypeList.Codes.T2lAnnex;
			message.EM_MessageSubType = DeclarationMessageSubTypeList.Codes.OriginalDeclaration;
			entryHeader.Messages.Add(message);
			var messagePivot = Factory.New<GenPivot>();
			messagePivot.XX_RelationType = GenPivotTypes.CusStorageDocPivotEdiMessage;
			messagePivot.XX_Relation1ID = pivot.PK;
			messagePivot.XX_Relation1TableCode = pivot.TablePrefix;
			messagePivot.XX_Relation2ID = message.PK;
			messagePivot.XX_Relation2TableCode = message.TablePrefix;

			return message;
		}
	}
}
