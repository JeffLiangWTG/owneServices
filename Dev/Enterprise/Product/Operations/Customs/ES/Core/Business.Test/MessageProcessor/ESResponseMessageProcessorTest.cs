using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Schema;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;
using CusEntryLine = Enterprise.Customs.ES.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.ES.Business.Testing
{
	public abstract class ESResponseMessageProcessorTest<TResponseMessageProcessor, TResponseProvider> : ESCommonResponseMessageProcessorTest<TResponseMessageProcessor, CusEntryHeader, TResponseProvider>
	where TResponseMessageProcessor : ESResponseMessageProcessor<TResponseProvider>
	{
		protected SupportingDocument[] GetCLSupportingDocuments(CusEntryLine entryLine = null)
		{
			var query = new ZQuery(CusSupportingInfoSchema.CSI_Type, Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument);
			query.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, CusEntryLineSchema.Constants.Prefix);
			if (entryLine != null)
			{
				query.AddToFilter(CusSupportingInfoSchema.CSI_ParentID, entryLine.PK);
			}
			return Factory.Load<SupportingDocument>(query);
		}

		protected SupportingDocument[] GetCHSupportingDocuments(CusEntryHeader entryHeader = null)
		{
			var query = new ZQuery(CusSupportingInfoSchema.CSI_Type, Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument);
			query.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, CusEntryHeaderSchema.Constants.Prefix);
			if (entryHeader != null)
			{
				query.AddToFilter(CusSupportingInfoSchema.CSI_ParentID, entryHeader.PK);
			}
			return Factory.Load<SupportingDocument>(query);
		}

		protected AdditionalInfo[] GetCHAdditionalInfo(CusEntryHeader entryHeader = null)
		{
			var query = new ZQuery(CusSupportingInfoSchema.CSI_Type, Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo);
			query.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, CusEntryHeaderSchema.Constants.Prefix);
			if (entryHeader != null)
			{
				query.AddToFilter(CusSupportingInfoSchema.CSI_ParentID, entryHeader.PK);
			}
			return Factory.Load<AdditionalInfo>(query);
		}

		protected PreviousDocument[] GetCLPreviousDocuments(CusEntryLine entryLine = null)
		{
			var query = new ZQuery(CusSupportingInfoSchema.CSI_Type, Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument);
			query.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, CusEntryLineSchema.Constants.Prefix);
			if (entryLine != null)
			{
				query.AddToFilter(CusSupportingInfoSchema.CSI_ParentID, entryLine.PK);
			}
			return Factory.Load<PreviousDocument>(query);
		}

		protected AdditionalInfo[] GetCLAdditionalInfos(CusEntryLine entryLine = null)
		{
			var query = new ZQuery(CusSupportingInfoSchema.CSI_Type, Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo);
			query.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, CusEntryLineSchema.Constants.Prefix);
			if (entryLine != null)
			{
				query.AddToFilter(CusSupportingInfoSchema.CSI_ParentID, entryLine.PK);
			}
			return Factory.Load<AdditionalInfo>(query);
		}

		public void GenericCommonAssertProcessEntryData(TestEdiMessage responseMessage, CusEntryHeader entryHeader = null, string expectedMessageInterpretation = "", string emStatus = EDIMessage.Status.Received, string chStatus = "RCV", string messageSubType = "", string messageNum = "", string movementReferenceNumber = "", string entryStatusCode = "", string circuit = "", ZDateTime? acceptanceDate = null, ZDateTime? entryReleaseDate = null, string csvClearance = "", string circuitCan = "", ZDateTime? limitPaymentDate = null, string csvImportCertificate = "", ZDateTime? atcLimitPaymentDate = null, bool parallel = false, string paymentProofNumber = "", string atcPaymentProofNumber = "", string exportMRN = "", string clearanceResult = "", string eadPrintProcedure = "", string csvT2L = "", ZDateTime? limitDateOfArrival = null)
		{
			GenericCommonAssertProcessResponseEntryHeader(responseMessage, entryHeader: entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, emStatus: emStatus, messageNum: messageNum, messageSubType: messageSubType, chStatus: chStatus, chEntryStatus: entryStatusCode, csvClearance: csvClearance, csvImportCertificate: csvImportCertificate, clearanceDate: entryReleaseDate, arrivalLimit: limitDateOfArrival, clearanceResult: clearanceResult);
			CombineAssertions(() =>
			{
				var factory = new BusinessObjectFactory();
				var esCode = Core.Constants.CountryCodes.Spain;
				var entryStatusList = RefCusCodeListTypes.GetCachedList(factory, esCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, ZDateTime.Today);
				AssertEquals("MovementReferenceNumber", movementReferenceNumber, entryHeader.MovementReferenceNumber);
				AssertEquals("EntryHeaderStatusDescription", string.IsNullOrEmpty(entryStatusCode) ? string.Empty : entryStatusList.GetDescriptionFromCode(entryStatusCode), entryHeader.EntryHeaderStatusDescription);
				AssertEquals("MovementReferenceNumberEntryStatus", circuit, entryHeader.MovementReferenceNumberEntryStatus);
				AssertEquals("MovementReferenceNumberIssueDate", acceptanceDate == null ? ZDateTime.Empty : acceptanceDate, entryHeader.MovementReferenceNumberIssueDate);
				AssertEquals("ZG_LimitPaymentDate", limitPaymentDate == null ? ZDateTime.Empty : limitPaymentDate, entryHeader.ZG_LimitPaymentDate);
				AssertEquals("CircuitCan", circuitCan, entryHeader.CircuitCan);
				AssertEquals("ZG_PaymentProofNumber", paymentProofNumber, entryHeader.ZG_PaymentProofNumber);
				AssertEquals("ZG_ATCPaymentProofNumber", atcPaymentProofNumber, entryHeader.ZG_ATCPaymentProofNumber);
				AssertEquals("ZG_ATCLimitPaymentDate", atcLimitPaymentDate == null ? ZDateTime.Empty : atcLimitPaymentDate, entryHeader.ZG_ATCLimitPaymentDate);
				AssertEquals("ZG_Parallel", parallel, entryHeader.ZG_Parallel);
				AssertEquals("ZG_ExportMRN", exportMRN, entryHeader.ZG_ExportMRN);
				AssertEquals("EUH_EADPrintProcedure", eadPrintProcedure, entryHeader.EUH_EADPrintProcedure);
				AssertEquals("ZG_CSVT2L", csvT2L, entryHeader.ZG_CSVT2L);
			});
		}
	}

	sealed class ESResponseMessageProcessorDoNotInheritTest : TestCaseWithFactory
	{
		public void TestGetRelevantBusinessObjectFindQuery_EHub()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_BGMReference = "TEST";

			var interchangeID = ZGuid.NewZGuid();

			var sentInterchange = Factory.New<EDIInterchange>();
			sentInterchange.EI_SessionGUID = interchangeID;
			sentInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			sentInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			var sentMessage = Factory.New<TestEdiMessage>();
			sentMessage.EM_LinkedObject = entryHeader;
			sentInterchange.ContainedMessages.Add(sentMessage);

			var responseInterchange = Factory.New<EDIInterchange>();
			responseInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			responseInterchange.EI_SessionGUID = interchangeID;
			responseInterchange.EI_TransportType = EDIInterchange.TransportType.eHub;
			var message = Factory.New<TestEdiMessage>();
			message.EM_ApplicationReference = "TEST";
			responseInterchange.ContainedMessages.Add(message);

			var sentBusinessObjects = new BusinessObject[] { entryHeader };

			var messageProcessor = new ESResponseMessageProcessorForTest(new LoggingInformation());
			var foudBusinessObject = messageProcessor.FindRelevantBusinessObjectCoreExposed(message, sentBusinessObjects, false);

			AssertEquals("FindRelevantBusinessObjectCore finds the correct entryHeader", entryHeader, foudBusinessObject);
		}

		public void TestGetRelevantBusinessObjectFindQuery_DirectxT()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_BGMReference = "TEST";

			var interchangeID = ZGuid.NewZGuid();

			var sentInterchange = Factory.New<EDIInterchange>();
			sentInterchange.EI_SessionGUID = interchangeID;
			sentInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			sentInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			var sentMessage = Factory.New<TestEdiMessage>();
			sentMessage.EM_LinkedObject = entryHeader;
			sentInterchange.ContainedMessages.Add(sentMessage);

			var responseInterchange = Factory.New<EDIInterchange>();
			responseInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			responseInterchange.EI_SessionGUID = interchangeID;
			responseInterchange.EI_TransportType = EDIInterchange.TransportType.xT;
			var message = Factory.New<TestEdiMessage>();
			message.EM_ApplicationReference = ZString.Empty;
			responseInterchange.ContainedMessages.Add(message);

			var sentBusinessObjects = new BusinessObject[] { entryHeader };

			var messageProcessor = new ESResponseMessageProcessorForTest(new LoggingInformation());
			var foudBusinessObject = messageProcessor.FindRelevantBusinessObjectCoreExposed(message, sentBusinessObjects, true);

			AssertEquals("FindRelevantBusinessObjectCore finds the correct entryHeader", entryHeader, foudBusinessObject);
		}
	}

	class ESResponseMessageProcessorForTest : ESResponseMessageProcessor<object>
	{
		public ESResponseMessageProcessorForTest(LoggingInformation logger) : base(logger)
		{
		}

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { ZString.Empty };

		protected override object GetMessageProviderCore(EDIMessage message) => null;

		public CusEntryHeader FindRelevantBusinessObjectCoreExposed(EDIMessage message, BusinessObject[] sentBusinessObjects, bool isDirectxTMessage) => FindRelevantBusinessObjectCore(message, sentBusinessObjects, isDirectxTMessage);

		protected override void ProcessMessageCore(EDIMessage message, CusEntryHeader entryHeader, object response)
		{
		}
	}
}
