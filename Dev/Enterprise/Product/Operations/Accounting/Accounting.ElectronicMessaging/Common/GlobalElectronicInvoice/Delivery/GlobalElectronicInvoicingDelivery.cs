using System.Collections.Generic;
using System.IO;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public class GlobalElectronicInvoicingDelivery : IGlobalElectronicInvoicingDelivery
	{
		public GlobalElectronicInvoicingDelivery(GEIDeliveryModeAndContextProvider provider)
		{
			Provider = provider;
		}

		GEIDeliveryModeAndContextProvider Provider { get; }

		public IXmlEDIInterchange[] Deliver(GlobalElectronicInvoicing eInvoice, bool hasValidationError = false)
		{
			var serializedInvoice = Provider.Serializer.Serialize(eInvoice);
			var interchanges = new List<IXmlEDIInterchange>();
			using (var ms = (SubStreamableStream)new MemoryStream(MessageEncoding.UTF8WithoutBOM.GetBytes(serializedInvoice)))
			{
				foreach (var mode in Provider.Modes)
				{
					var interchangeCreated = DeliverIndividualMode(mode, ms, hasValidationError);
					if (interchangeCreated != null)
					{
						interchanges.Add(interchangeCreated);
					}
				}
			}
			return interchanges.ToArray();
		}

		IXmlEDIInterchange DeliverIndividualMode(IEDICommunicationsMode mode, SubStreamableStream stream, bool hasValidationError)
		{
			var postman = DeliveryDecider.GetDeliveryMode(mode, hasValidationError);
			return ProcessDeliveryResult(mode, ExecuteDelivery(postman, Provider.Context, mode, stream)).Succeeded
					? postman.GetCreatedInterchange()
					: null;
		}

		IDeliveryResult ProcessDeliveryResult(IEDICommunicationsMode mode, IDeliveryResult failTime)
		{
			if (!failTime.Succeeded)
			{
				mode.EK_LastFailed = failTime.FailTime;
			}

			return failTime;
		}

		IDeliveryResult ExecuteDelivery(IDelivery delivery, DeliveryContext context, IEDICommunicationsMode mode, SubStreamableStream stream)
		{
			Argument.NotNull(context, nameof(context));
			Argument.NotNull(mode, nameof(mode));
			Argument.NotNull(stream, nameof(stream));
			Argument.NotNull(delivery, nameof(delivery));

			using (SetPurpose())
			{
				stream.Position = 0;
				return delivery.Deliver(context, mode, new DeliveryStreamWrapperUXML(stream, context.ParentInfo));
			}

			DisposableAction SetPurpose()
			{
				var oldPurpose = context.PurposeCode;
				return new DisposableAction(() => context.PurposeCode = mode.EK_MessagePurpose, () => context.PurposeCode = oldPurpose);
			}
		}

		protected virtual IGEIDeliveryModeDecider DeliveryDecider => deliveryTypeDecider ?? (deliveryTypeDecider = new GEIDeliveryModeDecider(Provider));
		IGEIDeliveryModeDecider deliveryTypeDecider;
	}
}
