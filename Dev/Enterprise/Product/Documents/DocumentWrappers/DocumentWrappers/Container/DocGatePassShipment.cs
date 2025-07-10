using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentWrappers
{
	/// <summary>
	/// Gate pass document.
	/// </summary>
	public class DocGatePassShipment : DocShipmentReceival, Integration.DocumentWrappers.IDocGatePassShipment
	{
		#region Constructor stuff

		protected DocGatePassShipment(GatePassShipment shipment, BusinessObjectFactory factoryToWrap)
			: base(shipment, factoryToWrap)
		{
		}

		public static DocGatePassShipment New(GatePassShipment shipment, BusinessObjectFactory factoryToWrap)
		{
			if (shipment == null)
			{
				return null;
			}
			else
			{
				return new DocGatePassShipment(shipment, factoryToWrap);
			}
		}

		GatePassShipment Shipment
		{
			get { return (GatePassShipment)WrappedObject; }
		}

		#endregion

		#region macro providers

		public ZString Vessel
		{
			get { return ExportVessel; }
		}

		public ZString VoyageNo
		{
			get { return ExportVoyage; }
		}

		public ZInt TotalOutturned
		{
			get
			{
				ZInt total = 0;

				foreach (GatePassPackLine pack in Shipment.OuterPackLines)
				{
					total += pack.JL_Outturn;
				}

				return total;
			}
		}

		public ZInt TotalPackagesDelivered
		{
			get
			{
				ZInt total = 0;

				foreach (CommonPickupDeliveryConfirm delivery in Shipment.DestinationCFSDepartures)
				{
					foreach (CommonConfirmDivot divot in delivery.Divots)
					{
						total += divot.J8_PackagesDelivered;
					}
				}

				return total;
			}
		}

		public ZString TotalPackTypeDescription
		{
			get
			{
				ZString total = "";

				foreach (CommonPickupDeliveryConfirm delivery in Shipment.DestinationCFSDepartures)
				{
					foreach (CommonConfirmDivot divot in delivery.Divots)
					{
						if (total.IsEmpty)
						{
							total = BindToLists.GetCachedLists(Factory).OuterPackTypes.GetDescriptionFromCode(divot.PackLine.JL_F3_NKPackType);
						}
						else
						{
							if (total != (ZString)BindToLists.GetCachedLists(Factory).OuterPackTypes.GetDescriptionFromCode(divot.PackLine.JL_F3_NKPackType))
							{
								total = BindToLists.GetCachedLists(Factory).OuterPackTypes.GetDescriptionFromCode(Constants.PkgUnit.Package);
							}
						}
					}
				}

				return total;
			}
		}

		public ZString GatePassNotes
		{
			get { return GetNotes(PredefinedNoteTypes.Instance.GatePassNotes.Description, Shipment); }
		}

		public ZString GatePassStatusShort
		{
			get { return Shipment.JS_GatePassStatusShort; }
		}

		#endregion
	}
}
