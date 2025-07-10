using System;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public class GEIDeliveryModeDecider : IGEIDeliveryModeDecider
	{
		public GEIDeliveryModeDecider(GEIDeliveryModeAndContextProvider provider)
		{
			Provider = provider;
		}

		GEIDeliveryModeAndContextProvider Provider { get; }

		public IDelivery GetDeliveryMode(IEDICommunicationsMode ediCommunicationMode, bool hasValidationError)
		{
			IDelivery delivery = null;

			switch (ediCommunicationMode.EK_CommunicationsTransport)
			{
				case EDICommunicationsModeCommunicationsTransportList.Codes.EHubService:
					delivery = GetEhubDelivery();
					break;
				case EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface:
				case EDICommunicationsModeCommunicationsTransportList.Codes.FTP:
					delivery = GetEAdaptorDelivery();
					break;
				case EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface:
					delivery = GetDirectXTDelivery();
					break;
				default:
					ThrowDoNotSupportException();
					break;
			}

			return delivery;

			#region Local Functions

			EServicesDelivery GetEhubDelivery()
			{
				if (SaveAndDeliver())
				{
					return new GEIEHubDelivery();
				}
				else if (OnlySaveDoNotDeliver())
				{
					return new FailedGEIEHubDelivery();
				}
				else
				{
					ThrowDoNotSupportException();
				}

				return null;
			}

			EServicesDelivery GetEAdaptorDelivery()
			{
				if (SaveAndDeliver())
				{
					return new GEIEAdaptorDelivery();
				}
				else if (OnlySaveDoNotDeliver())
				{
					return new FailedGEIEAdaptorDelivery();
				}
				else
				{
					ThrowDoNotSupportException();
				}

				return null;
			}

			EServicesDelivery GetDirectXTDelivery()
			{
				if (SaveAndDeliver())
				{
					return new GEIDirectXTDelivery();
				}
				else if (OnlySaveDoNotDeliver())
				{
					return new FailedGEIDirectXTDelivery();
				}
				else
				{
					ThrowDoNotSupportException();
				}

				return null;
			}

			Exception ThrowDoNotSupportException() => throw new NotSupportedException("Could not decide the delivery mode.");

			bool OnlySaveDoNotDeliver() => hasValidationError && Provider.CreateEDIMessageEvenIfThereIsError && !Provider.DeliverEDIMessageEvenIfThereIsError;

			bool SaveAndDeliver() => !hasValidationError || Provider.DeliverEDIMessageEvenIfThereIsError;

			#endregion
		}
	}
}
