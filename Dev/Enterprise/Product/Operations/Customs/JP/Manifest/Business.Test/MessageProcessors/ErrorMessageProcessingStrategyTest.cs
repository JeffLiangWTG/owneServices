using System;
using System.IO;
using System.Linq;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.JP.Common;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.Business.Testing;

[TestedType(typeof(ErrorMessageProcessingStrategy))]
sealed class ErrorMessageProcessingStrategyTest : TestCaseWithFactory
{
	public void TestProcessHCH01Message()
	{
		var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		var bill = manifestHeader.Bills.AddNew();
		bill.ABL_BillNumber = "2000002222200001";

		var sessionGuid = Guid.NewGuid();
		var incomingInterchange = Factory.New<EDIInterchange>();
		incomingInterchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.JPCustoms;
		incomingInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		incomingInterchange.EI_Status = EDIInterchange.Status.Queued;
		incomingInterchange.EI_IsActive = true;
		incomingInterchange.EI_InterchangeType = "XER";
		incomingInterchange.EI_SessionGUID = sessionGuid;

		var xTErrorMessage = Factory.New<EDIMessage>();
		xTErrorMessage.EM_EI = incomingInterchange.PK;
		xTErrorMessage.EM_MessageType = "XER";
		xTErrorMessage.EM_LinkTable = AsycudaManifestHeader.Schema.TableName;
		xTErrorMessage.EM_LinkUniqueID = manifestHeader.PK;

		var outgoingMessage = Factory.New<EDIMessage>();
		outgoingMessage.EM_Status = EDIInterchange.Status.Sent;
		outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
		outgoingMessage.EM_MessageData = JPMessageUtils.ConvertStringToMessage(ReadTestFile("HCH01_BillNum.txt"));
		outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.JPCustoms;
		outgoingMessage.EM_MessageNum = "123456789";

		var outgoingInterchange = EDIInterchange.CreateFromMessage(outgoingMessage);
		outgoingInterchange.EI_SessionGUID = sessionGuid;

		AssertEquals(string.Empty, bill.ABL_MessageStatus);

		IErrorMessageProcessingStrategy strategy = new ErrorMessageProcessingStrategy(manifestHeader);
		strategy.ProcessMessage(xTErrorMessage);

		AssertEquals(JPMessageStatusList.Codes.Error, bill.ABL_MessageStatus);

		var log = manifestHeader.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.InterchangeFailedToBeSent.Code)).OrderByDescending(a => a.SL_PostedTimeUtc).First();
		AssertEquals("SL_Reference", "123456789", log.SL_Reference);
	}

	public void TestProcessHDEMessage()
	{
		var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		manifestHeader.AMA_MasterBill = "S3E000056201";
		var bill = manifestHeader.Bills.AddNew();
		bill.ABL_BillNumber = "2000002222200001";

		var sessionGuid = Guid.NewGuid();
		var incomingInterchange = Factory.New<EDIInterchange>();
		incomingInterchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.JPCustoms;
		incomingInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		incomingInterchange.EI_Status = EDIInterchange.Status.Queued;
		incomingInterchange.EI_IsActive = true;
		incomingInterchange.EI_InterchangeType = "XER";
		incomingInterchange.EI_SessionGUID = sessionGuid;

		var xTErrorMessage = Factory.New<EDIMessage>();
		xTErrorMessage.EM_EI = incomingInterchange.PK;
		xTErrorMessage.EM_MessageType = "XER";
		xTErrorMessage.EM_LinkTable = AsycudaManifestHeader.Schema.TableName;
		xTErrorMessage.EM_LinkUniqueID = manifestHeader.PK;

		var outgoingMessage = Factory.New<EDIMessage>();
		outgoingMessage.EM_Status = EDIInterchange.Status.Sent;
		outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
		outgoingMessage.EM_MessageData = JPMessageUtils.ConvertStringToMessage(ReadTestFile("HDE_BillNum.txt"));
		outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.JPCustoms;
		outgoingMessage.EM_MessageNum = "123456789";

		var outgoingInterchange = EDIInterchange.CreateFromMessage(outgoingMessage);
		outgoingInterchange.EI_SessionGUID = sessionGuid;

		AssertEquals(string.Empty, manifestHeader.MasterBill.ABL_MessageStatus);

		IErrorMessageProcessingStrategy strategy = new ErrorMessageProcessingStrategy(manifestHeader);
		strategy.ProcessMessage(xTErrorMessage);

		AssertEquals(JPMessageStatusList.Codes.Error, manifestHeader.MasterBill.ABL_MessageStatus);

		var log = manifestHeader.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.InterchangeFailedToBeSent.Code)).OrderByDescending(a => a.SL_PostedTimeUtc).First();
		AssertEquals("SL_Reference", "123456789", log.SL_Reference);
	}

	public void TestProcessMessage_HDF01()
	{
		var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		var bill = manifestHeader.Bills.AddNew();
		bill.ABL_BillNumber = "AAAAAAAAA1AAAAAAAAA2";

		var sessionGuid = Guid.NewGuid();
		var incomingInterchange = Factory.New<EDIInterchange>();
		incomingInterchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.JPCustoms;
		incomingInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		incomingInterchange.EI_Status = EDIInterchange.Status.Queued;
		incomingInterchange.EI_IsActive = true;
		incomingInterchange.EI_InterchangeType = "XER";
		incomingInterchange.EI_SessionGUID = sessionGuid;

		var xTErrorMessage = Factory.New<EDIMessage>();
		xTErrorMessage.EM_EI = incomingInterchange.PK;
		xTErrorMessage.EM_MessageType = "XER";
		xTErrorMessage.EM_LinkTable = AsycudaManifestHeader.Schema.TableName;
		xTErrorMessage.EM_LinkUniqueID = manifestHeader.PK;

		var outgoingMessage = Factory.New<EDIMessage>();
		outgoingMessage.EM_Status = EDIInterchange.Status.Sent;
		outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
		outgoingMessage.EM_MessageData = JPMessageUtils.ConvertStringToMessage(ReadTestFile("HDF01.txt"));
		outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.JPCustoms;
		outgoingMessage.EM_MessageNum = "987654321";

		var outgoingInterchange = EDIInterchange.CreateFromMessage(outgoingMessage);
		outgoingInterchange.EI_SessionGUID = sessionGuid;

		AssertEquals(string.Empty, bill.ABL_MessageStatus);

		IErrorMessageProcessingStrategy strategy = new ErrorMessageProcessingStrategy(manifestHeader);
		strategy.ProcessMessage(xTErrorMessage);

		AssertEquals(JPMessageStatusList.Codes.Error, bill.ABL_MessageStatus);

		var log = manifestHeader.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.InterchangeFailedToBeSent.Code)).OrderByDescending(a => a.SL_PostedTimeUtc).First();
		AssertEquals("SL_Reference", "987654321", log.SL_Reference);
	}

	string ReadTestFile(string fileName)
	{
		using var stream = GetType().Assembly.GetManifestResourceStream($"Enterprise.Customs.JP.Manifest.Business.Test.MessageProcessors.Test.{fileName}");
		return new StreamReader(stream).ReadToEnd();
	}
}
