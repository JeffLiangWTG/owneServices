using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocIMOShipment : DocForwardingShipment, IDocIMO
	{
		protected DocIMOShipment(ForwardingShipment shipment, ForwardingConsol consol, BusinessObjectFactory factoryToWrap)
			: base(shipment, factoryToWrap)
		{
			this.IMOConsol = DocForwardingConsol.New(consol, Factory);
		}

		public new static DocIMOShipment New(ForwardingShipment shipment, BusinessObjectFactory factoryToWrap)
		{
			return (shipment != null) ? new DocIMOShipment(shipment, null, factoryToWrap) : null;
		}

		public static DocIMOShipment New(ForwardingShipment shipment, ForwardingConsol consol, BusinessObjectFactory factoryToWrap)
		{
			return (shipment != null) ? new DocIMOShipment(shipment, consol, factoryToWrap) : null;
		}

		#region IDocIMO Members

		public DocOrganisation IMOSender
		{
			get { return Consignor; }
		}

		public DocOrganisation IMOConsignee
		{
			get { return Consignee; }
		}

		public DocOrganisation IMOCarrier
		{
			get { return IMOConsol != null ? IMOConsol.ShippingLine : null; }
		}

		public ZString IMOConsolNumber
		{
			get
			{
				ZString result = ZString.Empty;
				if (IMOConsol != null)
				{
					result = Res.GetString("0ee2c670-6278-492e-b427-195af4d3c18d", "Consol: {0}", IMOConsol.ConsolNumber) + "\n";
					if (!IMOConsol.MasterBillNum.IsEmpty)
					{
						result += IMOConsol.MasterBillHeading + ": " + IMOConsol.MasterBillNum;
					}
				}
				return result.TrimEnd();
			}
		}

		public ZString IMOShippersRef
		{
			get
			{
				if (ReportName == CFSIMO)
				{
					return IMOConsol != null ? IMOConsol.AgentsReference : ZString.Empty;
				}
				else
				{
					return IMOConsol != null ? IMOConsol.BookingReference : ZString.Empty;
				}
			}
		}

		public ZString IMOForwardersRef
		{
			get
			{
				if (ReportName == CFSIMO)
				{
					return ZString.Empty;
				}
				else
				{
					return IMOConsol != null ? IMOConsol.AgentsReference : ZString.Empty;
				}
			}
		}

		public ZString IMOVesselVoyage
		{
			get
			{
				ZString result = ZString.Empty;
				if (IMOConsol != null)
				{
					if (IMOConsol.TransportMode == Core.Constants.TransportModes.Air)
					{
						result = IMOConsol.VoyageNumber;
					}
					else
					{
						result = IMOConsol.VesselName;
						if (!IMOConsol.VoyageNumber.IsEmpty)
						{
							result += result.IsEmpty ? IMOConsol.VoyageNumber : new ZString(" / " + IMOConsol.VoyageNumber);
						}
					}
				}
				return result;
			}
		}

		public ZString IMOETD
		{
			get { return IMOConsol != null ? IMOConsol.ETDString : ZString.Empty; }
		}

		public ZString IMOPortOfLoading
		{
			get { return IMOConsol != null && IMOConsol.PortOfLoading != null ? IMOConsol.PortOfLoading.Code + ", " + IMOConsol.PortOfLoading.PortName : ""; }
		}

		public ZString IMOPortOfDischarge
		{
			get { return IMOConsol != null && IMOConsol.PortOfDischarge != null ? IMOConsol.PortOfDischarge.Code + ", " + IMOConsol.PortOfDischarge.PortName : ""; }
		}

		public ZString IMODestination
		{
			get { return base.DestinationLoco != null ? base.DestinationLoco.Code + ", " + base.DestinationLoco.PortName : ""; }
		}

		public ZString IMOHandlingInstructions
		{
			get
			{
				ZString result = DangerousGoodsHandlingInstruction;
				if (result.IsEmpty && IMOConsol != null)
				{
					result = IMOConsol.DangerousGoodsHandlingInstruction;
				}

				return result;
			}
		}

		public ZString IMOContainerNum
		{
			get { return ZString.Empty; }
		}

		public ZString IMOSealNum
		{
			get { return ZString.Empty; }
		}

		public ZString IMOContainerType
		{
			get { return ZString.Empty; }
		}

		public ZString IMOContainerTare
		{
			get { return ZString.Empty; }
		}

		public ZString IMOTotalGrossMassAndTare
		{
			get { return ZString.Empty; }
		}

		public DocIMOBodyCollection IMOShipments
		{
			get
			{
				if (ReportName == ShipmentIMO)
				{
					return GetDocIMOBodyCollection(GetHazPackLines(ZBool.False));
				}
				else
				{
					return GetDocIMOBodyCollection(GetHazPackLines(ZBool.True));
				}
			}
		}

		#endregion

		protected DocForwardingConsol IMOConsol;
		protected DocPackLinesCollection GetHazPackLines(ZBool allPackLines)
		{
			DocPackLinesCollection coll = new DocPackLinesCollection(CommonShipment.Factory);
			foreach (DocPackLines line in OuterPackLineCollection)
			{
				if (line.IsHazardous)
				{
					if (allPackLines)
					{
						coll.Add(line);
					}
					else
					{
						if (line.ContainerNumber.IsEmpty)
						{
							coll.Add(line);
						}
					}
				}
			}

			return coll;
		}

		protected DocIMOBodyCollection GetDocIMOBodyCollection(DocPackLinesCollection packLines)
		{
			DocIMOBodyCollection coll = new DocIMOBodyCollection(CommonShipment.Factory);
			ZString description = Res.GetString("03e5b838-b285-43fa-a673-f17a8d6809bb", "Job Reference: {0}", ShipmentNumber) + "\n";
			DocLoadListPackLineCollection packs = new DocLoadListPackLineCollection(IMOConsol, packLines, CommonShipment.Factory);
			ZDecimal totalWeightInKG = 0M;
			ZDecimal totalVolumeInM3 = 0M;
			foreach (DocLoadListPackLine lines in packs)
			{
				description += lines.PackageDetailsWithHazCat;
				totalWeightInKG += lines.TotalHazWeight;
				totalVolumeInM3 += lines.TotalHazVolume;
			}

			description += description.EndsWith("\n") ? "" : "\n";
			description += Res.GetString("0caa699f-2ea6-4aff-9487-ca3e0c0633b1", "Description: {0}", DetailedDescriptionOfGoods);
			if (packs.Count > 0)
			{
				DocIMOBody iMOBody = new DocIMOBody(MarksAndNumbers, description, FormatNumber(totalWeightInKG), "", FormatNumber(totalVolumeInM3));
				coll.Add(iMOBody);
			}
			else
			{
				DocIMOBody emptyIMOBody = new DocIMOBody(Res.GetString("6432901d-91f3-439d-b0d0-1187d6371979", "No Uncontainerized Hazardous packlines available for Shipment {0}.", ShipmentNumber), "", "", "", "");
				coll.Add(emptyIMOBody);
			}
			return coll;
		}
	}
}
