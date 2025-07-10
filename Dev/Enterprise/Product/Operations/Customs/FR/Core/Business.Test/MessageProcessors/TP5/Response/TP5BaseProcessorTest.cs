using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	public abstract class TP5BaseProcessorTest<T, TProcessor> : TestCaseWithFactory where TProcessor : TP5BaseProcessor<T>
		where T : class
	{
		public void TestProcessor()
		{
			var processor = GetNCTSBaseProcessor();

			AssertContainsExactElementsInAnyOrder("MessageTypesToInclude of NCTSBaseProcessor should contains TP5.", new string[] { MessageTypeList.Codes.TP5 }, processor.MessageTypesToInclude);
			AssertEquals("ApplicationCode of NCTSBaseProcessor should be FRC.", EDIMessage.ApplicationCodes.FRCustomsMessage, processor.ApplicationCode);
			AssertEquals("MessageFriendlyName of NCTSBaseProcessor should be FR NCTS Base Processor.", "FR NCTS Base Processor", processor.MessageFriendlyName);
		}

		public void TestDepartureCustomsStatus()
		{
			var header = GetNCTSHeader();
			var message = GetNCTSFREDIMessage(header);
			var processor = GetNCTSBaseProcessor();
			processor.ProcessMessage(message);
			AssertEquals("CustomsStatus should be set depending the response.", ExpectedDepartureCustomsStatus, header.MovementHeader.BM_CustomsStatus);

			header = GetNCTSHeader();
			header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;
			message = GetNCTSFREDIMessage(header);
			processor.ProcessMessage(message);
			AssertEquals("When CustomsStatus is REL, it will not be set to MRN based on the response.", ExpectedDepartureCustomsStatusWhenBM_CustomsStatusIsREL, header.MovementHeader.BM_CustomsStatus);
		}

		public void TestArrivalCustomsStatus()
		{
			var header = GetNCTSHeader();
			var message = GetNCTSFREDIMessage(header);
			var processor = GetNCTSBaseProcessor();
			processor.ProcessMessage(message);
			AssertEquals("CustomsStatus should be set depending the response.", ExpectedArrivalCustomsStatus, header.ArrivalMovementHeader.BM_CustomsStatus);
		}

		public void TestMRNIsUpdated()
		{
			var header = GetNCTSHeader();
			var message = GetNCTSFREDIMessage(header);
			var processor = GetNCTSBaseProcessor();
			processor.ProcessMessage(message);
			AssertEquals("MRN should be set depending the response.", ExpectedMRN, header.MovementReferenceNumber);
		}

		public void TestReleaseDateIsUpdated()
		{
			var header = GetNCTSHeader();
			var message = GetNCTSFREDIMessage(header);
			var processor = GetNCTSBaseProcessor();
			processor.ProcessMessage(message);
			AssertEquals("MovementReferenceIssueDate should be set depending the response.", ExpectedReleaseDate, header.MovementReferenceIssueDate);
		}

		public void TestEntryDate()
		{
			var header = GetNCTSHeader();
			header.MovementHeader.BM_EntryDate = ZDateTime.BrettsBirthday;
			var message = GetNCTSFREDIMessage(header);
			var processor = GetNCTSBaseProcessor();
			processor.ProcessMessage(message);

			AssertEquals("EntryDate should be set depending on the response", ExpectedEntryDate, header.MovementHeader.BM_EntryDate);
		}

		public void TestMessageStatusIsUpdated()
		{
			var header = GetNCTSHeader();
			var message = GetNCTSFREDIMessage(header);
			var processor = GetNCTSBaseProcessor();
			processor.ProcessMessage(message);

			AssertEquals("MessageStatus should be set depending the response.", ExpectedMessageStatus, header.MovementHeader.BM_MessageStatus);
		}

		public void TestPhaseIdIsUpdated()
		{
			var header = GetNCTSHeader();
			var message = GetNCTSFREDIMessage(header);
			var processor = GetNCTSBaseProcessor();
			processor.ProcessMessage(message);

			AssertEquals("PhaseId should be set depending the response.", ExpectedPhaseId, header.MovementHeader.BM_Phase);
		}

		protected NctsHeader GetNCTSHeader()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.DepartureAndArrival);
			return nctsHeader;
		}

		protected TestNCTSFREDIMessage GetNCTSFREDIMessage(NctsHeader nctsHeader)
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_BodyText = GetMessageText();

			var message = Factory.New<TestNCTSFREDIMessage>();
			message.EM_EI = interchange.PK;
			message.EM_MessageSubType = GetMessageSubType();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.FRCustomsMessage;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_MessageText = GetMessageText();
			message.EM_LinkedObject = IsArrival ? nctsHeader : nctsHeader.MovementHeader;
			return message;
		}

		protected CusGuaranteeHeader GetGuaranteeHeader(OrgHeader org, ZString pW_BondNumber)
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = pW_BondNumber;
			guaranteeHeader.CPH_OH_PermitHolder = org.PK;
			guaranteeHeader.CPH_Type = GuaranteeTypeList.Codes.COD;
			guaranteeHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			guaranteeHeader.CPH_SubType = "1";
			return guaranteeHeader;
		}

		protected ApplicationTypeMessageProcessor GetNCTSBaseProcessor() => (TProcessor)Activator.CreateInstance(typeof(TProcessor), new BatchProcessor.LoggingInformation());

		protected abstract ZString GetMessageSubType();

		protected abstract ZString GetMessageText();

		protected virtual ZString ExpectedMRN => ZString.Empty;

		protected virtual ZDateTime ExpectedReleaseDate => ZDateTime.Empty;

		protected virtual ZDateTime ExpectedEntryDate => ZDateTime.BrettsBirthday;

		protected virtual ZString ExpectedMessageStatus => ZString.Empty;

		protected virtual ZString ExpectedPhaseId => ZString.Empty;

		protected virtual ZString ExpectedDepartureCustomsStatus => ZString.Empty;

		protected virtual ZString ExpectedDepartureCustomsStatusWhenBM_CustomsStatusIsREL => ExpectedDepartureCustomsStatus.IsEmpty ? NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit : ExpectedDepartureCustomsStatus;

		protected virtual ZString ExpectedArrivalCustomsStatus => ZString.Empty;

		protected virtual ZBool IsArrival => false;

		protected readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		protected class TestNCTSFREDIMessage : NCTSFREDIMessage
		{
			public TestNCTSFREDIMessage(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override string GetMessageReferenceNumber() => "111";
		}
	}
}
