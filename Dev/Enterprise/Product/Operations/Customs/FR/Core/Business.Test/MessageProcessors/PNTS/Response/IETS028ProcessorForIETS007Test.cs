using System;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS028;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class IETS028ProcessorForIETS007Test : PNTSBaseProcessorTest<Iets028, IETS028Processor>
	{
		protected override void AddPropertiesForTemporaryStorageHeader(TemporaryStorageHeader header)
		{
			header.LRN = "IETS007VALIDMESSAGE002";
		}

		protected override CusEntryNumber GetCusEntryNumber(TemporaryStorageHeader header) => CusEntryNumber.Load(header, "LRN", Core.Constants.CountryCodes.France);

		protected override ZString GetExpectedErrorTextIfEntryNumberNotFound() => new ZString("Couldn't locate Job using provided LRN# or CRN# or MRN#");

		public void TestGetLinkedCusEntryNumber()
		{
			var tempHeader = Factory.New<TemporaryStorageHeader>();
			tempHeader.AMA_RN_NKCountry = GlbCompany.CurrentCompany.Country.Code;
			tempHeader.AMA_JobReference = "JobRef001";
			tempHeader.CustomsStatusDate = ZDateTime.BrettsBirthday;
			tempHeader.LRN = "123";
			tempHeader.CRN = "456";
			tempHeader.MRN = "789";

			var tempHeader2 = Factory.New<TemporaryStorageHeader>();
			tempHeader2.AMA_RN_NKCountry = GlbCompany.CurrentCompany.Country.Code;
			tempHeader2.AMA_JobReference = "JobRef002";
			tempHeader2.CustomsStatusDate = ZDateTime.BrettsBirthday;
			tempHeader2.LRN = "321";
			tempHeader2.LRN = ZString.Empty;
			tempHeader2.CRN = "654";
			tempHeader2.CRN = ZString.Empty;
			tempHeader2.MRN = "987";
			tempHeader2.MRN = ZString.Empty;
			Factory.Save();

			var processor = GetPNTSBaseProcessor();

			var message = Factory.New<PNTSEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.FRCustomsMessage;
			message.EM_MessageType = "STO";
			message.EM_MessageSubType = GetMessageSubType();
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = $"<IETS028><lrn></lrn><mrn>789</mrn></IETS028>";
			message.MessageNumberStrategy = new FRMessageNumberStrategy(Factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);
			processor.ProcessMessage(message);
			AssertNotEquals("LRN is empty, MRN is empty, CRN is empty", tempHeader2.PK, message.EM_LinkedObject.PK);
			AssertEquals("TemporaryHeader should be linked to the message by MRN.", tempHeader.PK, message.EM_LinkedObject.PK);

			var message2 = Factory.New<PNTSEDIMessage>();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.FRCustomsMessage;
			message2.EM_MessageType = "STO";
			message2.EM_MessageSubType = GetMessageSubType();
			message2.EM_Status = EDIMessage.Status.Queued;
			message2.EM_MessageText = $"<IETS028><crn>456</crn></IETS028>";
			message2.MessageNumberStrategy = new FRMessageNumberStrategy(Factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);
			processor.ProcessMessage(message2);
			AssertNotEquals("LRN is empty, MRN is empty, CRN is empty", tempHeader2.PK, message2.EM_LinkedObject.PK);
			AssertEquals("TemporaryHeader should be linked to the message by CRN.", tempHeader.PK, message2.EM_LinkedObject.PK);

			var message3 = Factory.New<PNTSEDIMessage>();
			message3.EM_ApplicationCode = EDIMessage.ApplicationCodes.FRCustomsMessage;
			message3.EM_MessageType = "STO";
			message3.EM_MessageSubType = GetMessageSubType();
			message3.EM_Status = EDIMessage.Status.Queued;
			message3.EM_MessageText = $"<IETS028><lrn>123</lrn></IETS028>";
			message3.MessageNumberStrategy = new FRMessageNumberStrategy(Factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);
			processor.ProcessMessage(message3);
			AssertNotEquals("LRN is empty, MRN is empty, CRN is empty", tempHeader2.PK, message3.EM_LinkedObject.PK);
			AssertEquals("TemporaryHeader should be linked to the message by LRN.", tempHeader.PK, message3.EM_LinkedObject.PK);
		}

		protected override void PrepareTestData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TransportDocumentTemporaryStorage, "TD44T");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TransportDocumentTemporaryStorage, "C624", "C624 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			base.PrepareTestData();
		}

		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>Proof of Union Status Presented<br><strong>FRN: </strong>FRN21BEPN000000C3FMU4<br><strong>Notification Date: </strong>1/05/2021 12:34:56 PM</p>");

		protected override ZString GetExpectedCustomsStatus() => Enterprise.Customs.EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.ProofOfUnionStatusPresented;

		protected override ZDateTime GetExpectedCustomsStatusDate() => new ZDateTime(2021, 5, 1, 12, 34, 56);

		protected override ZString GetExpectedNewMessageStatus() => PNTSMessageStatusList.Codes.Acknowledged;

		protected override ZString GetExpectedCRN() => ZString.Empty;

		protected override ZString GetExpectedMRN() => ZString.Empty;

		protected override ZString GetExpectedFRN() => "FRN21BEPN000000C3FMU4";

		protected override ZString GetMessageSubType() => "028";

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.PNTS_IETS028ResponseMessageForIETS007.xml");

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
