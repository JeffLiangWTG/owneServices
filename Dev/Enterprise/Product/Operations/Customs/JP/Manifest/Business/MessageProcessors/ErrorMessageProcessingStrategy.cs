using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.JP.Common;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.JP.Common.JPProcedureCodeList;

namespace Enterprise.Customs.JP.Manifest.Business;

sealed class ErrorMessageProcessingStrategy : IErrorMessageProcessingStrategy
{
	public ErrorMessageProcessingStrategy(AsycudaManifestHeader header)
	{
		Header = Argument.NotNull(header, nameof(header));
	}

	AsycudaManifestHeader Header { get; }

	void IErrorMessageProcessingStrategy.ProcessMessage(EDIMessage message)
	{
		const string HCH01HouseBillNumFieldName = "HAWB";

		const string HDF01HouseBillNumFieldName = "HAWB Number";

		const string MasterBillNumFieldId = "AWB";

		var interchange = message.Interchange;

		if (interchange != null && interchange.EI_SessionGUID.IsValid)
		{
			var query = new ZQuery(EDIInterchangeSchema.EI_SessionGUID, interchange.EI_SessionGUID)
					.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, interchange.EI_ApplicationCode)
					.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit)
					.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchange.TransportType.xT);

			var outgoingInterchange = message.Factory.LoadTop1<EDIInterchange>(query);

			if (outgoingInterchange != null)
			{
				var messageQuery = new ZQuery(EDIMessageSchema.EM_EI, outgoingInterchange.PK)
					.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit)
					.AddToFilter(EDIMessageSchema.EM_Status, EDIMessageStatusList.Codes.Sent)
					.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.JPCustoms);
				var outgoingMessage = message.Factory.LoadTop1<EDIMessage>(messageQuery);
				var messageData = outgoingMessage?.EM_MessageData;

				if (messageData != null)
				{
					var outboundParseResult = NACCSFactoryService.GetMessageFlatParser(message.Factory).ParseOutbound(messageData);
					var procedureCode = outboundParseResult.Header?.ProcedureCode ?? string.Empty;

					var billNums = new List<string>();

					switch (procedureCode)
					{
						case JPManifestProcedureCodeList.Codes.HCH01:
							billNums.AddRange(outboundParseResult.MessageBody.Where(c => c.FieldDefinition.Name == HCH01HouseBillNumFieldName).Select(c => c.Data).Distinct());
							break;
						case JPManifestProcedureCodeList.Codes.HDF01:
							billNums.AddRange(outboundParseResult.MessageBody.Where(c => c.FieldDefinition.Name == HDF01HouseBillNumFieldName).Select(c => c.Data).Distinct());
							break;
						case JPManifestProcedureCodeList.Codes.HDE:
							billNums.AddRange(outboundParseResult.MessageBody.Where(c => c.FieldDefinition.ID == MasterBillNumFieldId).Select(c => c.Data).Distinct());
							break;
						default:
							break;
					}

					if (billNums.Count > 0)
					{
						switch (procedureCode)
						{
							case JPManifestProcedureCodeList.Codes.HCH01:
							case JPManifestProcedureCodeList.Codes.HDF01:
								Header.Bills
								.Where(c => billNums.Any(billNum => c.ABL_BillNumber.EqualsIgnoringCase(billNum)))
								.ForEach(c => c.ABL_MessageStatus = JPMessageStatusList.Codes.Error);
								break;
							case JPManifestProcedureCodeList.Codes.HDE:
								if (billNums.Any(billNum => Header.AMA_MasterBill == billNum))
								{
									Header.MasterBill.ABL_MessageStatus = JPMessageStatusList.Codes.Error;
								}
								break;
							default:
								break;
						}

						Header.AddEventIgnoringMsgNum(Events.InterchangeFailedToBeSent, outgoingMessage);
					}
				}
			}
		}

		message.EM_Status = EDIMessage.Status.ProcessedOK;
	}
}
