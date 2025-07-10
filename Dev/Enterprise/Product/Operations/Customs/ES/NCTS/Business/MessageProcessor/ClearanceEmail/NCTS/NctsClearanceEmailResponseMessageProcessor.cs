using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.NCTS.Messaging.MessageProcessors;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsClearanceEmailResponseMessageProcessor : ESNCTSResponseMessageProcessor<INctsClearanceEmailProvider>
	{
		public NctsClearanceEmailResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		const int CSVClearanceLength = 16;
		const string CSVClearanceString = "ndeLevante:";
		const int ClearanceDateLength = 10;
		const string ClearanceDateString = "FechadeLevante:";
		const int ArrivalLimitDateLength = 10;
		const string ArrivalLimitDateString = "ximadeLlegada:";
		const int ClearanceProcedureLength = 2;
		const string ClearanceProcedureString = "ResultadoalDespacho:";

		protected override string MessageFriendlyNameCore => (NoResString)"Ncts Clearance Email Message Processor";

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { DeclarationMessageTypeList.Codes.NctsClearanceEmail };
		protected override ZBool ShouldHaveSentInterchange => false;

		protected override INctsClearanceEmailProvider GetMessageProviderCore(EDIMessage message)
		{
			var messageText = message.EM_MessageText.Replace(" ", "");

			var csvClearance = GetSpecificData(messageText, CSVClearanceString, CSVClearanceLength);

			var clearanceDateStringData = GetSpecificData(messageText, ClearanceDateString, ClearanceDateLength);
			var clearanceDateCorrect = ZDateTime.TryParseExact(clearanceDateStringData, out var clearanceDate, CustomsDateTimeExtension.DateFormatSpainWithDash);

			if (csvClearance.IsEmpty || !clearanceDateCorrect || clearanceDate.IsEmpty)
			{
				throw new InvalidOperationException(Res.GetString("206A3B1E-DC0D-4764-A1BA-80B8CDF595B0", "Email Body doesn't have the correct data"));
			}

			var clearanceProcedure = GetSpecificData(messageText, ClearanceProcedureString, ClearanceProcedureLength);

			var arrivalLimitDateStringData = GetSpecificData(messageText, ArrivalLimitDateString, ArrivalLimitDateLength);
			var arrivalLimitDateCorrect = ZDateTime.TryParseExact(arrivalLimitDateStringData, out var arrivalLimitDate, CustomsDateTimeExtension.DateFormatSpainWithDash);

			if (!arrivalLimitDateCorrect)
			{
				arrivalLimitDate = ZDateTime.Empty;
			}

			return new NctsClearanceEmailObject(csvClearance, clearanceProcedure, clearanceDate, arrivalLimitDate);
		}

		ZString GetSpecificData(ZString fullText, ZString initialLineText, ZInt dataLength)
		{
			if (fullText.Contains(initialLineText))
			{
				int startIndex = fullText.IndexOf(initialLineText, StringComparison.OrdinalIgnoreCase) + initialLineText.Length;
				return fullText.SubstringSafe(startIndex, dataLength);
			}
			else
			{
				return ZString.Empty;
			}
		}

		protected override NctsHeader FindRelevantBusinessObjectCore(EDIMessage message, BusinessObject[] sentBusinessObjects, bool isDirectxTMessage)
			=> MessageProcessorHelper.GetRelevantBusinessObjectForEmailResponse<NctsHeader>(message, CusEntryNumberTypes.Standard.MovementReferenceNumber);

		protected override void ProcessMessageCore(EDIMessage message, NctsHeader linkedBusinessObject, INctsClearanceEmailProvider provider)
		{
			CreateOrUpdateCusEntryNumber(linkedBusinessObject, CusEntryNumberTypes.Spain.ClearanceCSV, provider.CSVClearance, ZString.Empty, provider.ClearanceDate, provider.ArrivalLimitDate);
			linkedBusinessObject.ESNctsHeader.CEN_ClearanceCriteria = provider.ClearanceProcedure;
			linkedBusinessObject.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture;

			TriggerMisingDocumentRequestForEmails(linkedBusinessObject, message);
			SetMessageStatusAsReceived(message);
			SetMessageSubTypeAsAccepted(message);
		}

		protected override CommonDocumentRequest<NctsHeader> GetNewDocumentRequest(NctsHeader businessObject, ZString certName, EDIMessage message) => new NCTSDepartureDocumentRequest(businessObject, certName);
	}
}
