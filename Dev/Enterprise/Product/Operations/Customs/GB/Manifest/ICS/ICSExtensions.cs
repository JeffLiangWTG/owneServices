using System;
using System.Globalization;
using System.IO;
using System.Xml;
using CargoWise.Customs.GB.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Customs.GB.ICS
{
	public static class ICSExtensions
	{
		public static GBCustomsRequest NewGBCustomsRequest(EDIMessage message)
		{
			GBCustomsRequest request = null;

			var dataProvider = GetRequestDataProvider(message);
			if (dataProvider != null)
			{
				request = new GBCustomsRequest
				{
					JobNumber = dataProvider.JobNumber,
					Provider = message.EM_ApplicationCode == EDIMessage.ApplicationCodes.GbMessageICSNorthernIreland ? ProviderType.ICSNI : ProviderType.ICSGB,
					Credentials = dataProvider.Credentials,
					Service = GetServiceType(message),
					ContentType = "XML",
					Version = "1.0"
				};

				if (request.Service == ServiceType.Create || request.Service == ServiceType.Update)
				{
					request.ServiceReference = dataProvider.ServiceReference;
				}
			}
			return request;
		}

		public static ICSRequestDataProvider GetRequestDataProvider(EDIMessage message)
		{
			switch (message.EM_LinkedObject)
			{
				case AsycudaManifestHeaderBase manifest:
					return new ICSRequestDataProvider(manifest);
			}

			return null;
		}

		static ServiceType GetServiceType(EDIMessage message)
		{
			switch (message.EM_MessageSubType)
			{
				case Constants.ICSMessageSubTypes.NEW:
					return ServiceType.Create;
				case Constants.ICSMessageSubTypes.AMEND:
					return ServiceType.Update;
				case Constants.ICSMessageSubTypes.CANCEL:
					return ServiceType.Delete;
				default:
					throw new NotSupportedException(FormattableString.Invariant($"Message sub type {message.EM_MessageSubType} is not supported."));
			}
		}
		public static ZString Serialize<T>(T dataObj)
		{
			var settings = new XmlWriterSettings();
			settings.OmitXmlDeclaration = true;
			settings.Indent = true;

			using (var stream = new StringWriter(CultureInfo.InvariantCulture))
			using (var xmlWritter = XmlWriter.Create(stream, settings))
			{
				var serializer = ZXmlSerializer.New(typeof(T));
				serializer.Serialize(xmlWritter, dataObj);
				return stream.ToString();
			}
		}

		public static ZString Serialize(this GBCustomsRequest requestData) => Serialize<GBCustomsRequest>(requestData);
	}
}
