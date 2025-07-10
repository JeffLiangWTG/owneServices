using System.Collections;
using CargoWise.Application;
using Enterprise.Messaging.Business;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	public static class FailedMessageHandlerFactory
	{
		public static IFailedMessageHandler GetHandler(string applicationCode, string countryCode)
		{
			switch (applicationCode)
			{
				case EDIInterchange.ApplicationCodes.GlobalElectronicInvoice:
					return GetHandlerForGlobalElectronicInvoice(countryCode);

				default:
					return null;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		static IFailedMessageHandler GetHandlerForGlobalElectronicInvoice(string countryCode)
		{
			var handlers = ObjectFactory.Get<Hashtable>("GEIFailedMessageHandlers");
			var key = string.IsNullOrWhiteSpace(countryCode) || !handlers.ContainsKey(countryCode) ? "Default" : countryCode;

			var objectHandle = (ObjectHandle)handlers[key];
			return (IFailedMessageHandler)objectHandle.GetObject();
		}
	}
}
