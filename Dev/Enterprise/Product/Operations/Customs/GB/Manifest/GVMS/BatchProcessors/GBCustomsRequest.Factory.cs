using System;
using CargoWise.Customs.GB.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.GVMS
{
	public partial class GBCustomsRequest
	{
		static GVMSRequestDataProvider GetRequestDataProvider(EDIMessage message)
		{
			switch (message.EM_LinkedObject)
			{
				case AsycudaManifestHeader manifest:
					return new GVMSRequestDataProvider(manifest);
			}

			return null;
		}

		public static GBCustomsRequest New(EDIMessage message)
		{
			GBCustomsRequest request = null;

			var dataProvider = GetRequestDataProvider(message);
			if (dataProvider != null)
			{
				request = new GBCustomsRequest
				{
					JobNumber = dataProvider.JobNumber,
					Provider = GetProviderType(dataProvider.Gateway),
					Credentials = dataProvider.Credentials,
					Service = GetServiceType(message),
					ServiceReference = dataProvider.ServiceReference,
					ContentType = GetContentType(),
					Version = GetVersion()
				};
			}
			return request;
		}

		internal static ServiceType GetServiceType(EDIMessage message)
		{
			switch (message.EM_MessageSubType)
			{
				case Constants.GVMSMessageSubTypes.NEW:
					return ServiceType.Create;
				case Constants.GVMSMessageSubTypes.AMEND:
					return ServiceType.Update;
				case Constants.GVMSMessageSubTypes.CANCEL:
					return ServiceType.Delete;
				case Constants.GVMSMessageSubTypes.FINALISE:
					return ServiceType.Finalise;
				default:
					throw new NotSupportedException(FormattableString.Invariant($"{message.EM_MessageType} is not supported."));
			}
		}

		public static ProviderType GetProviderType(ZString gateWay)
		{
			switch (gateWay)
			{
				case GatewayList.Codes.GVMS:
					return ProviderType.GVMS;
				default:
					return ProviderType.Direct;
			}
		}

		static ZString GetVersion() => "1.0";

		static ZString GetContentType() => "json";
	}
}
