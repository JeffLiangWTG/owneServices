using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers
{
	[AllowPublicConstructor, AllowNoStaticNew]
	public class DocTranshipment : DocumentWrapper, IObsoleteValidation
	{
		public DocTranshipment(DocForwardingConsol currentConsol, DocForwardingShipment shipment)
			: base(null, currentConsol.Factory)
		{
			if (currentConsol != null && shipment != null)
			{
				fCurrentConsol = currentConsol;
				fShipment = shipment;
				SetNextConsol();
			}
		}

		public override string ToString()
		{
			return GetType().ToString();
		}

		public DocForwardingConsol CurrentConsol
		{
			get { return fCurrentConsol; }
		}

		public DocForwardingConsol NextConsol
		{
			get { return fNextConsol; }
		}

		public DocShipment Shipment
		{
			get { return fShipment; }
		}

		public ZBool PrintTranshipmentDetails
		{
			get { return fPrintTranshipmentDetails; }
		}

		public ZString ContainerNumbersOnCurrentConsol
		{
			get { return GetContainerNumbers(CurrentConsol); }
		}

		public ZString ContainerNumbersOnNextConsol
		{
			get { return GetContainerNumbers(NextConsol); }
		}

		public ZString ShipmentPackLineDetails
		{
			get
			{
				ZString result = ZString.Empty;
				if (Shipment != null && CurrentConsol != null)
				{
					result = Shipment.GetOuterPacksDetail(CurrentConsol);
					if (CurrentConsol.ReportName.Contains((NoResString)"Forwarding Instruction") && !Shipment.PrintContainerDetailsOnForwardingInstruction)
					{
						result = ZString.Empty;
					}
				}

				return result;
			}
		}

		protected void SetNextConsol()
		{
			if (CurrentConsol != null)
			{
				foreach (DocForwardingConsol consol in Shipment.ShipmentConsols)
				{
					if (!consol.NKPortOfLoading.IsEmpty &&
						!CurrentConsol.NKPortOfDischarge.IsEmpty &&
						consol.NKPortOfLoading == CurrentConsol.NKPortOfDischarge)
					{
						if (Shipment.ColoadMasterShipment == null)
						{
							fPrintTranshipmentDetails = ZBool.True;
							fNextConsol = consol;
						}
					}
				}
			}
		}

		protected ZString GetContainerNumbers(DocForwardingConsol consol)
		{
			ZString result = ZString.Empty;
			if (Shipment != null && consol != null)
			{
				IDocContainerCollection containers = Shipment.GetContainersInConsol(consol);
				containers.Sort("ContainerNumber", System.ComponentModel.ListSortDirection.Ascending);
				foreach (IDocContainer container in containers)
				{
					if (!result.Contains(container.ContainerNumber) && !container.ContainerNumber.IsEmpty)
					{
						result += container.ContainerNumber + ", ";
					}
				}
			}

			return result.TrimEndIncludingWhiteSpace(',');
		}

		protected DocForwardingConsol fCurrentConsol;
		protected DocForwardingConsol fNextConsol;
		protected DocShipment fShipment;
		protected ZBool fPrintTranshipmentDetails;
	}
}
