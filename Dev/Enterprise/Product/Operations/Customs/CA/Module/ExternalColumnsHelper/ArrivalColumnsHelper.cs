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
	class ArrivalColumnsHelper
	{
		#region AddColumns

		public void AddColumns(ZGrid filteredGrid, IBusinessObjectCollection gridCollection)
		{
			var propertyContainer = new CustomPropertyContainer();
			propertyContainer.AddCustomProperty(ColumnSchema.ArrivalCertificationStatus, Captions.ArrivalCertificationStatusMultilingualDescription, typeof(ZString), GetArrivalCertificationStatus);
			propertyContainer.AddCustomProperty(ColumnSchema.ArrivalCertificationDate, Captions.ArrivalCertificationDateMultilingualDescription, typeof(ZDateTime), GetArrivalCertificationDate);
			new CAExternalColumnsHelper.CAExternalModuleCustomColumnsInitializer(filteredGrid, gridCollection, Captions.ArrivalNotificationsGroup, propertyContainer).AddCustomColumns();
		}

		static object GetArrivalCertificationStatus(BusinessObject shipment)
		{
			var releaseMessage = GetMostRecentMessage(shipment);
			if (releaseMessage == null)
			{
				return CAExternalColumnsHelper.Constants.NotSentDescription;
			}
			else if (releaseMessage.EM_Status == CAExternalColumnsHelper.Constants.SentCode)
			{
				return CAExternalColumnsHelper.Constants.SentDescription;
			}
			else if (releaseMessage.EM_Status == CAExternalColumnsHelper.Constants.RejectedCode)
			{
				return CAExternalColumnsHelper.Constants.RejectedDescription;
			}
			else
			{
				return releaseMessage.EM_Status;
			}
		}

		static object GetArrivalCertificationDate(BusinessObject shipment)
		{
			var releaseMessage = GetMostRecentMessage(shipment);
			return releaseMessage != null && releaseMessage.EM_Status == CAExternalColumnsHelper.Constants.SentCode ?
				releaseMessage.EM_MessageDateTime : ZDateTime.Empty;
		}

		static EDIMessage GetMostRecentMessage(BusinessObject shipment)
		{
			return shipment.Factory.GetCachedValue(
				"MostRecentMessage:" + shipment.PK,
				() => GetMostRecentMessageCore(shipment));
		}

		static EDIMessage GetMostRecentMessageCore(BusinessObject shipment)
		{
			return shipment.Factory.LoadTop1<EDIMessage>(GetMostRecentMessageQuery(shipment));
		}

		static ZQuery GetMostRecentMessageQuery(BusinessObject shipment)
		{
			var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, shipment.PK);
			query.AddToFilter(EDIMessageSchema.EM_LinkTable, shipment.TableName);
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, SQLComparisonOperator.Equal, EDIMessage.ApplicationCodes.CAIMP);
			query.AddToFilter(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.Equal, MessageTypeList.Codes.RNSRequest);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, SQLComparisonOperator.Equal, EDIMessage.Direction.Transmit);
			query.AddToFilter(EDIMessageSchema.EM_MessageSubType, SQLComparisonOperator.Equal, RNSMessageTypes.Codes.ArrivalCertification);
			query.OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name + " DESC";
			return query;
		}
		#endregion

		#region Constants

		public static class Captions
		{
			internal static MultilingualString ArrivalCertificationStatusMultilingualDescription
			{
				get { return ResString.GetMultilingualString("3a02c291-3cc8-46a3-a783-fd645beb2040", ArrivalCertificationStatusId); }
			}

			internal const string ArrivalCertificationStatusId = "Arrival Certification Status";

			internal static MultilingualString ArrivalCertificationDateMultilingualDescription
			{
				get { return ResString.GetMultilingualString("33b2b640-1aa3-4b9f-ab1f-791b4da46986", ArrivalCertificationDateId); }
			}

			internal const string ArrivalCertificationDateId = "Arrival Certification Date";

			internal static ResourceStringData ArrivalNotificationsGroup
			{
				get { return Res.GetData("de720174-1572-4f1d-a576-ae62517f25cd", "Arrival Notifications"); }
			}
		}

		public static class ColumnSchema
		{
			internal const string ArrivalCertificationStatus = "ArrivalCertificationStatus";
			internal const string ArrivalCertificationDate = "ArrivalCertificationDate";
		}

		#endregion
	}
}
