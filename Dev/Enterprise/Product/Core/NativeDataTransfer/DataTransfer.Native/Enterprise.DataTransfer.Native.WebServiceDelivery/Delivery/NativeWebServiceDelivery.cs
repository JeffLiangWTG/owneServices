using System;
using System.IO;
using System.ServiceModel;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.Core;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.DataTransfer.Native.WebServiceDelivery.LocalService;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DataTransfer.Native.WebServiceDelivery
{
	public class NativeWebServiceDelivery : Delivery
	{
		public NativeWebServiceDelivery()
		{
			MessageConverter = new EDICommunicationsModeConverter();
			ErrorNotifier = new EmailNotifier();
			Service = new NativeDataService();
		}

		#region Dependency

		public IEDICommunicationsModeConverter MessageConverter { get; internal set; }
		public IErrorNotifier<IEDICommunicationsMode> ErrorNotifier { get; internal set; }
		public INativeDataService Service { get; internal set; }

		#endregion

		public override IDeliveryResult Deliver(DeliveryContext context, IEDICommunicationsMode mode, IDeliveryStreamWrapper stream, Func<Messaging.Integration.IEDIMessage> getMessageFunc = null)
		{
#if DEBUG
			if (Service == null)
			{
				ErrorReporter.ReportOnce("Service is null, please remember to initial Service before you call Deliver()");
			}
#endif

			IResponseMessage response = null;
			try
			{
				try
				{
					var uriString = mode.EK_Destination;
					var uri = new Uri(uriString);
					using (var serviceClient = new EnterpriseNativeDataServiceClient(uri))
					{
						Service.Client = serviceClient;
						var request = ConvertRequest(mode, stream?.Content);
						response = Service.Update(request);
						if (response == null)
						{
							throw new KnownErrorException("No response (null) returned from external service.");
						}

						if (response.HasError)
						{
							throw new KnownErrorException(response.ErrorMessage);
						}

						LogResponse(response, context);
						return DeliveryResult.Success;
					}
				}
				catch (UriFormatException uriFormatException)
				{
					throw new KnownErrorException("Could not create client for Web Service: Invalid format of URI", uriFormatException);
				}
				catch (EndpointNotFoundException endpointNotFoundException)
				{
					throw new KnownErrorException("Could not find Native Web Service with address: " + mode.EK_Destination, endpointNotFoundException);
				}
				catch (ProtocolException protocolException)
				{
					throw new KnownErrorException("Protocol does not match between " + Constants.ProductName + " and destination Web Service. Most likely is because SOAP protocol version is not correct", protocolException);
				}
			}
			catch (Exception exception) when (!exception.IsCriticalException())
			{
				using (var responseStream = new MemoryStream())
				using (var streamWrapper = new UnclosableStream(responseStream))
				{
					if (response != null)
					{
						WriteResponseToResponseStream(response, streamWrapper);
					}

					ErrorNotifier.Notify(context?.Factory, exception, mode, stream?.Content, streamWrapper);
				}
				LogResponse(response, context);
				return DeliveryResult.Error(exception);
			}
		}

		static void WriteResponseToResponseStream(IResponseMessage response, Stream responseStream)
		{
			try
			{
				using (var writer = new StreamWriter(responseStream))
				{
					writer.Write(response.ResponseMessage);
				}
			}
			catch (Exception exception)
			{
				if (exception.IsCriticalException())
				{
					throw;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
#if DEBUG
		internal virtual
#endif
 void LogResponse(IResponseMessage response, DeliveryContext context)
		{
			if (context != null)
			{
				var log = context.Factory.New<BaseStmALog>();
				log.SL_Table = context.ParentInfo.TableName;
				log.SL_Parent = context.ParentInfo.InternalPK;
				log.SL_SE_NKEvent = Events.DataExportCode;
				log.SL_Reference = "Native XML Connector Export" + " " + (response == null || response.HasError ? "Failed" : "Succeeded");
			}
		}

		IRequestMessage ConvertRequest(IEDICommunicationsMode mode, Stream stream)
		{
			var updateMessage = MessageConverter.Convert(mode);
			var message = stream.WriteToString();
			updateMessage.Message = message;
			return updateMessage;
		}
	}
}
