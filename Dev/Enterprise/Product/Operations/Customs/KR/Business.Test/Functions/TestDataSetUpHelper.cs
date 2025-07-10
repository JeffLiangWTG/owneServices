using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	[CodeAlive("Soon to be used")]
	public class TestDataSetUpHelper
	{
		public TestDataSetUpHelper(BusinessObjectFactory factory, Type testClassType)
		{
			this.factory = factory;
			this.testClassType = testClassType;
		}
		readonly BusinessObjectFactory factory;
		readonly Type testClassType;

		public byte[] ReturnEmbeddedFileData(string fileName)
		{
			return new TestFileReader(testClassType).GetEmbeddedFileData(TestFilesPath, fileName);
		}
		public string ReturnEmbeddedFileText(string fileName)
		{
			return new TestFileReader(testClassType).GetEmbeddedFileText(TestFilesPath, fileName);
		}

		public EDIInterchange CreateIncomingEDIInterchangeForTest(string fileName, string interchangeType)
		{
			var inInterchange = factory.New<EDIInterchange>();
			inInterchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;
			inInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			inInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.KRCustoms;
			inInterchange.EI_InterchangeType = interchangeType;
			inInterchange.EI_SessionGUID = new ZGuid("81CCDFE2-246B-410A-BF9A-033EFFF34301");
			inInterchange.EI_To = "xT";
			inInterchange.EI_From = "KRCustoms";
			inInterchange.EI_IsActive = true;
			if (interchangeType == EDIInterchangeType.XER)
			{
				inInterchange.SetEI_BodyTextOrDataSource(new MemoryStream(ReturnEmbeddedFileData(fileName)));
			}
			else
			{
				inInterchange.EI_HeaderText = ReturnEmbeddedFileText(fileName);
			}
			return inInterchange;
		}

		public EDIInterchange CreateOutgoingEDIInterchangeForTest(string messageType)
		{
			var outInterchange = factory.New<EDIInterchange>();
			outInterchange.EI_Status = EDIInterchangeStatusList.Codes.Sent;
			outInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.KRCustoms;
			outInterchange.EI_InterchangeType = messageType;
			outInterchange.EI_SessionGUID = new ZGuid("81CCDFE2-246B-410A-BF9A-033EFFF34301");
			outInterchange.EI_GB = GlbBranch.CurrentBranch.PK;
			outInterchange.EI_To = "KRCustoms";
			outInterchange.EI_From = "xT";

			var outgoingMessage = CreateOutgoingMessageForTest(messageType);
			outInterchange.ContainedMessages.Add(outgoingMessage);
			return outInterchange;
		}

		public EDIMessage CreateOutgoingMessageForTest(string messageType)
		{
			var outgoingMessage = factory.New<EDIMessage>();
			outgoingMessage.EM_MessageType = messageType;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			return outgoingMessage;
		}

		public EDIMessage CreateIncomingMessageForTest(string fileName, string messageType)
		{
			var incomingMessage = factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = messageType;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			if (messageType == EDIInterchangeType.XER)
			{
				incomingMessage.EM_MessageData = ReturnEmbeddedFileData(fileName);
			}
			else
			{
				incomingMessage.EM_MessageText = ReturnEmbeddedFileText(fileName);
			}

			return incomingMessage;
		}
		const string TestFilesPath = "Enterprise.Customs.KR.Business.Testing.TestFiles.Interchange";
	}
}
