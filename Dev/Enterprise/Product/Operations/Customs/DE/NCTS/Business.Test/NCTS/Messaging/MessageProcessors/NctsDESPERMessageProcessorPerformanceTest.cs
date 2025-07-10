using System.Linq;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NctsDESPERMessageProcessorPerformanceTest : TestCaseWithFactory
	{
		[DeveloperOnlyTest]
		[SnailTest]
		public void TestProcessDESPER()
		{
			processor.ExecuteBatch();
			AssertMultilineASCIIEquals("Logg", @"	Pre-Process Message #7000000000007
	Saving...
	1 message pre-processed
	Processing Message #7000000000007
	Successfully Added eDoc: DES-1-DE9000348-0001-DE005875_7000000000007.pdf.
	Saving...
	1 message processed",
			string.Join("\r\n", processor.Logger.UserLogStrings.Cast<string>()));
			var newFactory = new BusinessObjectFactory();
			message = newFactory.Load<AtlasInboundEDIMessage<IDESPER>>(message.PK);
			AssertEquals("message.EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
		}

		protected override void SetUp()
		{
			base.SetUp();
			processor = new ATLASBranchCustomsMessageProcessor
			{
				Logger = new LoggingInformation()
			};

			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = MRN;

			var outboundMessage = Factory.New<AtlasEDIMessage>();
			Factory.Save();
			outboundMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAtlasSystem;
			outboundMessage.EM_ApplicationReference = nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.DETSPC);
			outboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundMessage.EM_MessageType = "NCT";
			outboundMessage.EM_MessageNum = "HYEZNTCMT00000000002089";
			outboundMessage.EM_MessageSubType = "DES";
			outboundMessage.EM_Status = EDIMessage.Status.Sent;
			outboundMessage.EM_LinkedObject = nctsHeader;

			message = Factory.New<AtlasInboundEDIMessage<IDESPER>>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAtlasSystem;
			message.EM_ApplicationReference = nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.DETSPC);
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = "NCT";
			message.EM_MessageNum = MESSAGENUM;
			message.EM_MessageSubType = "DES";
			message.EM_MessageText = LargeDesperMessageForTestCreator.GetLargeMessage();
			message.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();
		}

		ATLASBranchCustomsMessageProcessor processor;
		AtlasInboundEDIMessage<IDESPER> message;
		const string MESSAGENUM = "7000000000007";
		const string MRN = "23DE587500031238M9";
	}
}
