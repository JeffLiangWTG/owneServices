using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	class RNSColumnsHelper
	{
		#region AddColumns

		public void AddColumns(ZGrid filteredGrid, IBusinessObjectCollection gridCollection)
		{
			var propertyContainer = new CustomPropertyContainer();
			propertyContainer.AddCustomProperty(ColumnSchema.RNSReleaseStatus, Captions.RNSReleaseStatusMultilingualDescription, typeof(ZString), GetRNSReleaseStatus);
			propertyContainer.AddCustomProperty(ColumnSchema.RNSReleaseDate, Captions.RNSReleaseDateMultilingualDescription, typeof(ZDateTime), GetRNSReleaseDate);
			new CAExternalColumnsHelper.CAExternalModuleCustomColumnsInitializer(filteredGrid, gridCollection, Captions.RNSReleaseNotificationsGroup, propertyContainer).AddCustomColumns();
			SetFetchForView(gridCollection);
		}

		static object GetRNSReleaseStatus(BusinessObject shipment)
		{
			var bo = GetManualReleaseBO(shipment);
			if (bo != null && !bo.ManualReleaseReason.IsEmpty)
			{
				return bo.ManualReleaseReason;
			}
			else
			{
				var message = GetMostInterestingMessage(shipment);
				var releaseMessage = message as EDIReleaseMessage;
				return releaseMessage != null
					? CAExternalColumnsHelper.GetCodeDescriptionFormatted(releaseMessage.EM_MessageSubType,
						releaseMessage.EM_MessageSubTypeDescription)
					: message != null && message.IsTransmitMessage
						? CAExternalColumnsHelper.GetCodeDescriptionFormatted(CAExternalColumnsHelper.Constants.AwaitingResponseCode,
							CAExternalColumnsHelper.Constants.AwaitingResponseDescription)
						: CAExternalColumnsHelper.GetCodeDescriptionFormatted(CAExternalColumnsHelper.Constants.NotSentCode,
							CAExternalColumnsHelper.Constants.NotSentDescription);
			}
		}

		static object GetRNSReleaseDate(BusinessObject shipment)
		{
			var bo = GetManualReleaseBO(shipment);
			if (bo != null && bo.ManualReleaseDate.IsValid)
			{
				return bo.ManualReleaseDate;
			}
			else
			{
				var releaseMessage = GetMostInterestingMessage(shipment) as EDIReleaseMessage;
				return releaseMessage != null ? releaseMessage.RNSReleaseDate : ZDateTime.Empty;
			}
		}

		static ManualReleaseCancelBO GetManualReleaseBO(BusinessObject shipment)
		{
			var cfsShipment = shipment as Integration.Customs.CA.IManualReleaseSupport;
			if (cfsShipment != null)
			{
				return new ManualReleaseCancelBO(cfsShipment.ManualReleaseNoteText, shipment.Factory);
			}
			return null;
		}

		static EDIMessage GetMostInterestingMessage(BusinessObject shipment)
		{
			return shipment.Factory.GetCachedValue(
				"MostInterestingMessage:" + shipment.PK,
				() => GetMostInterestingMessageCore(shipment));
		}

		static EDIMessage GetMostInterestingMessageCore(BusinessObject shipment)
			=> shipment.Factory.LoadTop1<EDIMessage>(GetMostInterestingResponseMessageQuery(shipment))
				?? shipment.Factory.LoadTop1<EDIMessage>(GetMostInterestingMessageQuery(shipment));

		static ZQuery GetMostInterestingResponseMessageQuery(BusinessObject shipment)
		{
			var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, shipment.PK);
			query.AddToFilter(EDIMessageSchema.EM_LinkTable, shipment.TableName);
			query.AddToFilter(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.Equal, MessageTypeList.Codes.EDIRelease);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, SQLComparisonOperator.Equal, EDIMessage.Direction.Receive);
			query.AddToFilter(EDIMessageSchema.EM_MessageSubType, SQLComparisonOperator.NotEqual, RNSMessagingBO.ReleaseSubTypesToIgnore);
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, SQLComparisonOperator.Equal, Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.CAIMP);
			query.OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name + " DESC";
			return query;
		}

		static ZQuery GetMostInterestingMessageQuery(BusinessObject shipment)
		{
			var messageTypes = new[] { MessageTypeList.Codes.RNSRequest, MessageTypeList.Codes.EDIRelease };
			var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, shipment.PK);
			query.AddToFilter(EDIMessageSchema.EM_LinkTable, JobShipmentSchema.Constants.TableName);
			query.AddToFilter(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.Equal, messageTypes);
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, SQLComparisonOperator.Equal, Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.CAIMP);
			query.OrderBy = EDIMessageSchema.EM_ReceiveTransmit.Name + " ASC, " + EDIMessageSchema.EM_SystemCreateTimeUtc.Name + " DESC";
			return query;
		}

		static void SetFetchForView(IBusinessObjectCollection collection)
		{
			collection.FetchStrategy.AdditionalFetchForView +=
				(sender, eventArgs) =>
				{
					if (eventArgs.TableColumns.Any(c => c.ColumnName == ColumnSchema.RNSReleaseStatus || c.ColumnName == ColumnSchema.RNSReleaseDate))
					{
						var factory = collection.Factory;
						foreach (var shipment in eventArgs.BusinessObjects)
						{
							factory.AddFetchHint(typeof(EDIMessage), GetMostInterestingResponseMessageQuery(shipment));
							factory.AddFetchHint(typeof(EDIMessage), GetMostInterestingMessageQuery(shipment));
						}
					}
				};
		}

		#endregion

		#region Constants

		public static class Captions
		{
			internal static MultilingualString RNSReleaseStatusMultilingualDescription
			{
				get { return ResString.GetMultilingualString("59ac7a21-53d1-4208-b335-6b0dbb35c32c", RNSReleaseStatusFilterId); }
			}

			internal const string RNSReleaseStatusFilterId = "RNS Release Status";

			internal static MultilingualString RNSReleaseDateMultilingualDescription
			{
				get { return ResString.GetMultilingualString("0fbbf3e1-fb91-4a1e-bfb4-5cf5b40f5496", RNSReleaseDateFilterId); }
			}

			internal const string RNSReleaseDateFilterId = "RNS Release Date";

			internal static ResourceStringData RNSReleaseNotificationsGroup
			{
				get { return Res.GetData("a1708c78-59ad-4607-835d-322f28c4c283", "Release Notifications (RNS)"); }
			}
		}

		public static class ColumnSchema
		{
			internal const string RNSReleaseStatus = "RNSReleaseStatus";
			internal const string RNSReleaseDate = "RNSReleaseDate";
		}

		#endregion
	}
}
