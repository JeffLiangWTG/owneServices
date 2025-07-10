using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	internal sealed class DocAgencyDetentionAdviceLine_FromContainer : DocAgencyDetentionAdviceLine
	{
		public DocAgencyDetentionAdviceLine_FromContainer(BillOfLadingContainer container, BusinessObjectFactory factory)
			: base(container, factory) { }

		protected override ZString GetDetentionType()
		{
			return "IMP";
		}

		protected override ZString GetContainerNo()
		{
			return WrappedContainer.JC_ContainerNum;
		}

		protected override ContainerTypeWrapper GetContainerType()
		{
			RefContainer type = WrappedContainer.Container;
			return type == null ? null : new ContainerTypeWrapper(WrappedContainer.Container, Factory);
		}

		protected override ZString GetVesselName()
		{
			BillOfLading bill;
			JobSailing sailing;
			JobVoyage voyage;

			if ((bill = WrappedContainer.Booking) != null && (sailing = bill.Sailing) != null && (voyage = sailing.Voyage) != null)
			{
				return voyage.JV_RV_NKVessel;
			}
			else
			{
				return ZString.Empty;
			}
		}

		protected override ZString GetVoyageNo()
		{
			BillOfLading bill;
			JobSailing sailing;
			JobVoyage voyage;

			if ((bill = WrappedContainer.Booking) != null && (sailing = bill.Sailing) != null && (voyage = sailing.Voyage) != null)
			{
				return voyage.JV_VoyageFlight;
			}
			else
			{
				return ZString.Empty;
			}
		}

		protected override LocationWrapper GetDetentionPort()
		{
			BillOfLading bill;
			ZString port;

			if ((bill = WrappedContainer.Booking) != null && !(port = bill.JS_RL_NKDestination).IsEmpty)
			{
				return new LocationWrapper(port, Factory);
			}
			else
			{
				return null;
			}
		}

		protected override ZString GetBillNumber()
		{
			BillOfLading bill;

			if ((bill = WrappedContainer.Booking) != null)
			{
				return bill.JS_HouseBill;
			}
			else
			{
				return ZString.Empty;
			}
		}

		protected override ZDateTime GetReleaseDate()
		{
			BillOfLading bill;
			Transport transport;

			if ((bill = WrappedContainer.Booking) != null && (transport = bill.TransportsIncludingRelated.ArrivalTransport) != null)
			{
				return transport.JW_TerminalAvailabilityDate;
			}
			else
			{
				return ZDateTime.Empty;
			}
		}

		protected override ZDateTime GetRequiredDate()
		{
			return WrappedContainer.JC_EmptyReturnedBy;
		}

		protected override ZDateTime GetPickupDate()
		{
			return WrappedContainer.JC_FCLWharfGateOut;
		}

		#region Implementation

		BillOfLadingContainer WrappedContainer
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (BillOfLadingContainer)WrappedObject; }
		}

		#endregion
	}
}
