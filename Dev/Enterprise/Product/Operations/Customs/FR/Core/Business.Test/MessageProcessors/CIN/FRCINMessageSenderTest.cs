using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	public class FRCINMessageSenderTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestConstructorThrowsArgumentException()
		{
			Activator.CreateInstance(typeof(FRCINImportMessageSender), new object[] { null, null, Messaging.MessageBuilders.CIN.CINImportMessageBuilder.MessageBuilderType.MovementIn });
		}

		public void TestConstructorValid()
		{
			var jobHeader = CreateCusTempStorageJobHeader();

			AssertNoExceptionThrown("Creating instance of FRCINMessageSender with valid CusTempStorageJobHeader",
				() => Activator.CreateInstance(typeof(FRCINImportMessageSender), new object[] { jobHeader, null, Messaging.MessageBuilders.CIN.CINImportMessageBuilder.MessageBuilderType.MovementIn })
			);
		}

		public void TestCanSendCommon()
		{
			var jobHeader = CreateCusTempStorageJobHeader();
			var messageSender = new FRCINImportMessageSender(jobHeader, null, Messaging.MessageBuilders.CIN.CINImportMessageBuilder.MessageBuilderType.MovementIn);

			var dec = jobHeader.CusTempStorageDec;
			var line = dec.CusTempStorageLines.FirstOrDefault() as CusTempStorageLine;

			line.TSL_OwnerReferenceType = "AWB";
			line.TSL_OwnerReferenceNumber = "UNITTEST";
			line.TSL_LocationOfGoods = "ORIGINAL";
			line.TSL_GoodsDescription = "Original Goods";
			line.TSL_PackageQty = 25;
			line.TSL_GrossWeight = 50.0m;

			var (canSend, reason) = messageSender.CanSend();
			AssertEquals("Should be ready to send", true, canSend);
			AssertEquals("Reason should be empty", string.Empty, reason);

			line.Delete();
			(canSend, reason) = messageSender.CanSend();
			AssertEquals("Lines should be missing", false, canSend);
			AssertEquals("Lines should be missing", FRCINImportMessageSender.ValidationNoLines, reason);
		}

		public void TestCanSendCorrection()
		{
			var jobHeader = CreateCusTempStorageJobHeader();
			var dec = jobHeader.CusTempStorageDec;

			var line = dec.CusTempStorageLines.FirstOrDefault() as CusTempStorageLine;

			line.TSL_OwnerReferenceType = "AWB";
			line.TSL_OwnerReferenceNumber = "UNITTEST";
			line.TSL_LocationOfGoods = "ORIGINAL";
			line.TSL_GoodsDescription = "Original Goods";
			line.TSL_PackageQty = 25;
			line.TSL_GrossWeight = 50.0m;

			Factory.Save();
			var prevFac = Factory.CreateNewFactory();
			var prevJobHeader = prevFac.Load<CusTempStorageJobHeader>(jobHeader.PK);

			var messageSender = new FRCINImportMessageSender(jobHeader, prevJobHeader, Messaging.MessageBuilders.CIN.CINImportMessageBuilder.MessageBuilderType.Correction);

			var (canSend, reason) = messageSender.CanSend();

			AssertEquals("Should not be ready to send", false, canSend);
			AssertEquals("Reason should be empty (silent)", string.Empty, reason);

			jobHeader.CusTempStorageDec.CusTempStorageLines[0].TSL_PackageQty -= 3;

			(canSend, reason) = messageSender.CanSend();

			AssertEquals("Should be not ready to send (status)", false, canSend);
			AssertEquals("Reason should be empty (silent)", string.Empty, reason);

			jobHeader.CusTempStorageDec.STH_MessageStatus = CusTempStorageDec.DeclarationStatusForCorrectionMessage;
			(canSend, reason) = messageSender.CanSend();

			AssertEquals("Should now be ready to send", true, canSend);
			AssertEquals("Reason should be empty (silent)", string.Empty, reason);
		}

		public void TestSend()
		{
			var jobHeader = CreateCusTempStorageJobHeader();
			jobHeader.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;
			var dec = jobHeader.CusTempStorageDec;

			var line = dec.CusTempStorageLines.FirstOrDefault() as CusTempStorageLine;

			line.TSL_OwnerReferenceType = "AWB";
			line.TSL_OwnerReferenceNumber = "UNITTEST";
			line.TSL_LocationOfGoods = "ORIGINAL";
			line.TSL_GoodsDescription = "Original Goods";
			line.TSL_PackageQty = 25;
			line.TSL_GrossWeight = 50.0m;

			var messageSender = new FRCINImportMessageSender(jobHeader, null, Messaging.MessageBuilders.CIN.CINImportMessageBuilder.MessageBuilderType.MovementIn);

			var (success, result) = messageSender.Send();
			AssertEquals("Message sending should be successful", true, success);
			AssertEquals("Message sent successfully", result);

			var loadedJob = Factory.Load<CusTempStorageJobHeader>(jobHeader.PK);

			AssertEquals(1, loadedJob.CusTempStorageDec.Messages.Count);
			var msg = loadedJob.CusTempStorageDec.Messages[0];

			AssertContains($"messageId=\"{msg.EM_MessageNum}\"", msg.EM_MessageText);
		}

		public void TestSendFailure()
		{
			var jobHeader = CreateCusTempStorageJobHeader();
			jobHeader.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;
			var dec = jobHeader.CusTempStorageDec;

			var line = dec.CusTempStorageLines.FirstOrDefault() as CusTempStorageLine;

			line.TSL_OwnerReferenceType = "AWB";
			line.TSL_OwnerReferenceNumber = "UNITTEST";
			line.TSL_LocationOfGoods = "ORIGINAL";
			line.TSL_GoodsDescription = "Original Goods";
			line.TSL_PackageQty = 25;
			line.TSL_GrossWeight = 50.0m;

			var messageSender = new FRCINImportMessageSender(jobHeader, null, Messaging.MessageBuilders.CIN.CINImportMessageBuilder.MessageBuilderType.MovementIn);

			jobHeader.SJH_OH_Customer = new CargoWise.Types.ZGuid(); // FK Constraint

			var (success, result) = messageSender.Send();
			AssertEquals("Message sending should be successful", false, success);
			AssertContains("Failed to send message", result);

			AssertNotNull("Message should exist", messageSender.message);
		}

		public void TestMessageCancelled()
		{
			var jobHeader = CreateCusTempStorageJobHeader();
			jobHeader.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;
			var dec = jobHeader.CusTempStorageDec;

			var line = dec.CusTempStorageLines.FirstOrDefault() as CusTempStorageLine;

			line.TSL_OwnerReferenceType = "AWB";
			line.TSL_OwnerReferenceNumber = "UNITTEST";
			line.TSL_LocationOfGoods = "ORIGINAL";
			line.TSL_GoodsDescription = "Original Goods";
			line.TSL_PackageQty = 25;
			line.TSL_GrossWeight = 50.0m;

			var messageSender = new FRCINImportMessageSender(jobHeader, null, Messaging.MessageBuilders.CIN.CINImportMessageBuilder.MessageBuilderType.MovementIn);

			messageSender.PrepareMessage();

			AssertNotNull("Message should exist", messageSender.message);

			messageSender.CancelMessageOnFailure();

			AssertEquals("Message should be cancelled", EDIMessage.Status.Cancelled, messageSender.message.EM_Status);
		}

		CusTempStorageJobHeader CreateCusTempStorageJobHeader()
		{
			var jobHeader = CusTempStorageJobHeader.New(Factory);
			jobHeader.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;
			return jobHeader;
		}
	}
}
