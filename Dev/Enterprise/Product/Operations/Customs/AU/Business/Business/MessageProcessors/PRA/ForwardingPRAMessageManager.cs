using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ForwardingPRAMessageManager : IPRAMessageManager
	{
		public ForwardingPRAMessageManager(ZString consolNumber, ZString containerNumber, BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, "factory");
			JobReference = consolNumber;
			ContainerNumber = containerNumber;
		}
		readonly BusinessObjectFactory factory;

		public string LastMessageTypeSent
		{
			get
			{
				var result = string.Empty;

				if (Container != null)
				{
					result = Container.LastPRAMessageSentWasCancellation
						? PRAConstants.Cancellation
						: PRAConstants.Submission;
				}
				return result;
			}
		}

		public EDIMessageCollection Messages
		{
			get { return Container != null ? Container.Messages : null; }
		}

		public ContainerMessagingData ContainerMessagingData
		{
			get
			{
				return dataLayer ?? (dataLayer = new FreightDataLayer(Container));
			}
		}
		FreightDataLayer dataLayer;

		public ZDateTime DepartureDate
		{
			get
			{
				return Consol != null
					? Consol.Transports.DepartureTransport.JW_ETD
					: ZDateTime.Empty;
			}
		}

		public ZString JobType
		{
			get { return "Consol No"; }
		}

		public ZString JobReference { get; set; }
		public ZString ContainerNumber { get; set; }

		#region Interface

		ForwardingConsol Consol
		{
			get
			{
				return consol ?? (consol = factory.LoadFromNaturalKey<ForwardingConsol>(JobConsolSchema.JK_UniqueConsignRef, JobReference));
			}
		}
		ForwardingConsol consol;

		BusinessObject IPRAMessageManager.Parent
		{
			get { return Consol; }
		}

		BusinessObject IPRAMessageManager.Container
		{
			get { return Container; }
		}

		ForwardingContainer Container
		{
			get
			{
				if (Consol != null && container == null)
				{
					ZQuery sQLFilter = new ZQuery();
					sQLFilter.AddToFilter(JobContainerSchema.JC_JK, Consol.PK);
					sQLFilter.AddToFilter(JobContainerSchema.JC_ContainerNum, ContainerNumber);
					container = factory.LoadTop1<ForwardingContainer>(sQLFilter);
				}
				return container;
			}
		}
		ForwardingContainer container;

		#endregion
	}
}
