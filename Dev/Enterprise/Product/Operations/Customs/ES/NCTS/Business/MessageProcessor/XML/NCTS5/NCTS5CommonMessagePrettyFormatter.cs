using System;
using System.Text;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Messaging;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public abstract class NCTS5CommonMessagePrettyFormatter : CommonMessagePrettyFormatter
	{
		protected void AppendAcceptanceDataIfNotEmpty(StringBuilder messageDetails, ZString mrn, ZDateTime acceptanceDate, string referenceTextMrn = null)
		{
			if (!mrn.IsEmpty || !acceptanceDate.IsEmpty || !acceptanceDate.IsValid)
			{
				var tableCreator = GetNewNonVisibleTableCreator();

				WriteRowIfNotEmpty(tableCreator, AcceptanceText, GetAcceptanceDateFormatted(acceptanceDate));
				WriteRowIfNotEmpty(tableCreator, referenceTextMrn ?? ReferenceText, mrn);

				messageDetails.Append(tableCreator.ToHtml());
			}
		}

		protected void AppendMessageType(StringBuilder messageDetails, ZString messageTypeDescription) => AppendDataInNewTableIfNotEmpty(messageDetails, MessageTypeText, messageTypeDescription);

		protected virtual ZString GetAcceptanceDateFormatted(ZDateTime acceptanceDate) => acceptanceDate.ToCustomsFormatDateStringddMMyyyyHHmmssWithDash();

		protected void AppendLimitDateOfArrivalIfNotEmpty(StringBuilder messageDetails, DateTime? limitlDateOfArrival)
		{
			if (limitlDateOfArrival != null)
			{
				AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, LimitDateOfArrivalText, ((ZDateTime)limitlDateOfArrival).ToCustomsFormatDateStringddMMyyyyWithDash());
			}
		}

		protected override ZString GetStatusDescription(string statusCode)
		{
			var description = ZString.Empty;
			switch (statusCode)
			{
				case Ncts5TransitStatusList.Codes.PreDeclaration:
					description = Ncts5TransitStatusList.Descriptions.PreDeclaration;
					break;
				case Ncts5TransitStatusList.Codes.PendingGuarantee:
					description = Ncts5TransitStatusList.Descriptions.PendingGuarantee;
					break;
				case Ncts5TransitStatusList.Codes.PendingDispatch:
					description = Ncts5TransitStatusList.Descriptions.PendingDispatch;
					break;
				case Ncts5TransitStatusList.Codes.Dispatch:
					description = Ncts5TransitStatusList.Descriptions.Dispatch;
					break;
				case Ncts5TransitStatusList.Codes.DeclarationCancelled:
					description = Ncts5TransitStatusList.Descriptions.DeclarationCancelled;
					break;
				case Ncts5TransitStatusList.Codes.PreDeclarationInvalidated:
					description = Ncts5TransitStatusList.Descriptions.PreDeclarationInvalidated;
					break;
				case Ncts5TransitStatusList.Codes.InvalidatedByGuarantee:
					description = Ncts5TransitStatusList.Descriptions.InvalidatedByGuarantee;
					break;
				case Ncts5TransitStatusList.Codes.Invalidated:
					description = Ncts5TransitStatusList.Descriptions.Invalidated;
					break;
				case Ncts5TransitStatusList.Codes.NotCleared:
					description = Ncts5TransitStatusList.Descriptions.NotCleared;
					break;
				case Ncts5TransitStatusList.Codes.RequestingToTheCountryOfDeparture:
					description = Ncts5TransitStatusList.Descriptions.RequestingToTheCountryOfDeparture;
					break;
				case Ncts5TransitStatusList.Codes.DeviationRejectedByUe:
					description = Ncts5TransitStatusList.Descriptions.DeviationRejectedByUe;
					break;
				case Ncts5TransitStatusList.Codes.Received:
					description = Ncts5TransitStatusList.Descriptions.Received;
					break;
				case Ncts5TransitStatusList.Codes.LiquidationInitiated:
					description = Ncts5TransitStatusList.Descriptions.LiquidationInitiated;
					break;
				case Ncts5TransitStatusList.Codes.UltimatedForPayment:
					description = Ncts5TransitStatusList.Descriptions.UltimatedForPayment;
					break;
				case Ncts5TransitStatusList.Codes.PendingResolutionOfDiscrepancy:
					description = Ncts5TransitStatusList.Descriptions.PendingResolutionOfDiscrepancy;
					break;
				case Ncts5TransitStatusList.Codes.Completed:
					description = Ncts5TransitStatusList.Descriptions.Completed;
					break;
				default:
					break;
			}
			return statusCode.IsNullOrEmpty() ? statusCode : statusCode + " - " + description;
		}

		protected string ArrivalText => GetH4Text(ResString.GetMultilingualString("946240C3-858B-429A-9FDB-1EFD6D571B2B", "Arrival"));
		protected string DepartureText => GetH4Text(ResString.GetMultilingualString("DBEB1E37-9D27-4670-97DF-6E7F5B2FE347", "Departure"));
		protected string UnloadingText => GetH4Text(ResString.GetMultilingualString("AC7E618A-C04B-4B48-8C07-73333113B6A6", "Unloading"));

		protected const string ResponseCodeRequestingDataFromCustoms = "S";
	}
}
