using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.XmlIO.XmlWriting;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public static class PreValidateTraderHelper
	{
		public static DeliveryContext NewDeliveryContext(EU.EMCS.Business.EMCSJobDeclaration declaration, MessageSendingNotificationCollection notificationCollection)
		{
			return new DeliveryContext(declaration.Factory)
			{
				ParentInfo = EntityInfo.New(declaration),
				ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging,
				MessageTypeCode = EDIMessageSubTypeList.Codes.XmlUniversalEvent,
				MessageSubTypeCode = EDIMessageSubTypeList.Codes.XmlUniversalEvent,
				Notifications = new PreValidateTraderNotifications(notificationCollection)
			};
		}

		public static void SetOutgoingUniversalEventInterpretation(ZGuid interchangePK, BusinessObjectFactory factory)
		{
			var query = new ZQuery(EDIMessageSchema.EM_EI, interchangePK);
			query.FetchOnlyFromLocalCache = true;
			var msg = factory.LoadTop1<EDIMessage>(query);

			if (msg != null)
			{
				_ = msg.EM_MessageInterpretation;
				factory.Save();
			}
		}

		public static string ConvertToXml(UniversalEvent universalEvent)
		{
			var xml = string.Empty;
			using (var stream = (SubStreamableStream)new MemoryStream())
			using (var reader = new StreamReader(stream))
			{
				new XmlWriter().WriteXML(universalEvent, stream, writeXMLDeclaration: false, UniversalXmlInfo.Namespace_2011_11);
				stream.Flush();
				stream.Position = 0;
				xml = reader.ReadToEnd();
			}
			return xml;
		}

		public static IEnumerable<PreValidateTraderInfo.TraderData> GetDistinctTraders(PreValidateTraderInfo traderInfo)
		{
			return CargoWise.Common.IEnumerableExtensions.DistinctBy(traderInfo.Traders, ti => ti.Key);
		}

		public static IEnumerable<string> GetProductCodes(PreValidateTraderInfo traderInfo, int chunk = 10)
		{
			var productCodes = traderInfo.ProductCodes;
			for (var i = 0; i < productCodes.Count(); i += chunk)
			{
				var codes = productCodes.Skip(i).Take(chunk);
				yield return new ZStringBuilder(codes).ToStringWithDelimiterBetweenAppends(",");
			}
		}

		public static class Constants
		{
			public const string MessageType = "QUERY";
			public const string Service = "GBEMCS";
			public const string Key = "Key";
			public const string EHubID = "GBCustoms";

			public static class ContextTypes
			{
				public const string PreValidateTraderBody = "PreValidateTraderBody";
			}
		}
	}
}
