using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.LocalCartage.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocCommonBookedMove : DocumentWrapper
	{
		protected DocCommonBookedMove(CommonBookedCtgMove move, BusinessObjectFactory factoryToWrap)
			: base(move, factoryToWrap)
		{
		}

		public static DocCommonBookedMove New(CommonBookedCtgMove move, BusinessObjectFactory factoryToWrap)
		{
			return (move != null) ? new DocCommonBookedMove(move, factoryToWrap) : null;
		}

		#region CommonBookedCtgMove

		CommonBookedCtgMove CommonBookedCtgMove
		{
			get { return (CommonBookedCtgMove)WrappedObject; }
		}

		#endregion

		#region Related Objects

		#region Cartage

		public DocCommonCartage Cartage
		{
			get { return DocCommonCartage.New(CommonBookedCtgMove.Cartage, CommonBookedCtgMove.Factory); }
		}

		#endregion

		#region Container

		public DocCommonContainer Container
		{
			get { return DocCommonContainer.New(CommonBookedCtgMove.Container, CommonBookedCtgMove.Cartage, CommonBookedCtgMove.Factory); }
		}

		#endregion

		#region CartageLegs

		public DocCommonCartageLegCollection CartageLegs
		{
			get
			{
				DocCommonCartageLegCollection result = new DocCommonCartageLegCollection(CommonBookedCtgMove);
				result.SortOnPlannedPickupTime();
				return result;
			}
		}

		#endregion

		#region PickupCartageLeg

		public DocCommonCartageLeg PickupCartageLeg
		{
			get { return DocCommonCartageLeg.New(CommonBookedCtgMove.FirstCartageLeg, Factory); }
		}

		#endregion

		#region DeliveryCartageLeg

		public DocCommonCartageLeg DeliveryCartageLeg
		{
			get { return DocCommonCartageLeg.New(CommonBookedCtgMove.LastCartageLeg, Factory); }
		}

		#endregion

		#region PickupDocAddress

		public DocDocAddress PickupDocAddress
		{
			get { return DocDocAddress.New(CommonBookedCtgMove.PickupFromDocAddress, CommonBookedCtgMove.Factory); }
		}

		#endregion

		#region DeliveryDocAddress

		public DocDocAddress DeliveryDocAddress
		{
			get { return DocDocAddress.New(CommonBookedCtgMove.DeliverToDocAddress, CommonBookedCtgMove.Factory); }
		}

		#endregion

		#endregion

		#region Properties

		public ZString PickupFromOrgCode
		{
			get { return PickupCartageLeg != null ? PickupCartageLeg.PickupFromOrgCode : null; }
		}

		public ZString DeliverToOrgCode
		{
			get { return DeliveryCartageLeg != null ? DeliveryCartageLeg.DeliverToOrgCode : null; }
		}

		public ZString PickupTimeInOutText
		{
			get
			{
				ZString result = " - ";

				if (PickupCartageLeg != null && PickupCartageLeg.PickupTimeIn.IsValid)
				{
					result = PickupCartageLeg.PickupTimeIn.ToString("dd-MMM HH:mm") + " / ";
					result += (PickupCartageLeg.PickupTimeOut.IsValid) ? PickupCartageLeg.PickupTimeOut.ToShortTimeString() : "-";
				}

				return result;
			}
		}

		public ZString PickupTimeInText
		{
			get { return PickupCartageLeg != null && PickupCartageLeg.PickupTimeIn.IsValid ? PickupCartageLeg.PickupTimeIn.ToString("dd-MMM HH:mm") : " - "; }
		}

		public ZString PickupTimeOutText
		{
			get { return PickupCartageLeg != null && PickupCartageLeg.PickupTimeOut.IsValid ? PickupCartageLeg.PickupTimeOut.ToString("dd-MMM HH:mm") : " - "; }
		}

		public ZString DeliverTimeInOutText
		{
			get
			{
				ZString result = " - ";
				if (DeliveryCartageLeg != null && DeliveryCartageLeg.DeliverTimeIn.IsValid)
				{
					result = DeliveryCartageLeg.DeliverTimeIn.ToString("dd-MMM HH:mm") + " / ";
					result += (DeliveryCartageLeg.DeliverTimeOut.IsValid) ? DeliveryCartageLeg.DeliverTimeOut.ToShortTimeString() : "-";
				}

				return result;
			}
		}

		public ZString DeliverTimeInText
		{
			get { return DeliveryCartageLeg != null && DeliveryCartageLeg.DeliverTimeIn.IsValid ? DeliveryCartageLeg.DeliverTimeIn.ToString("dd-MMM HH:mm") : " - "; }
		}

		public ZString DeliverTimeOutText
		{
			get { return DeliveryCartageLeg != null && DeliveryCartageLeg.DeliverTimeOut.IsValid ? DeliveryCartageLeg.DeliverTimeOut.ToString("dd-MMM HH:mm") : " - "; }
		}

		public ZString PickupDemurrage
		{
			get { return PickupCartageLeg != null ? PickupCartageLeg.PickupDemurrage : ZString.Empty; }
		}

		public ZString DeliveryDemurrage
		{
			get { return DeliveryCartageLeg != null ? DeliveryCartageLeg.DeliveryDemurrage : ZString.Empty; }
		}

		public ZInt BookedPackages
		{
			get { return CommonBookedCtgMove.EW_BookedPackCount; }
		}

		public ZString BookedPackType
		{
			get { return CommonBookedCtgMove.EW_F3_NKPackType; }
		}

		public ZDecimal BookedWeight
		{
			get { return CommonBookedCtgMove.EW_BookedWeight; }
		}

		public ZString BookedWeightUnit
		{
			get { return CommonBookedCtgMove.EW_WeightUQ; }
		}

		public ZDecimal BookedVolume
		{
			get { return CommonBookedCtgMove.EW_BookedVolume; }
		}

		public ZString BookedVolumeUnit
		{
			get { return CommonBookedCtgMove.EW_VolumeUQ; }
		}

		public ZDecimal BookedLength
		{
			get { return CommonBookedCtgMove.EW_BookedLength; }
		}

		public ZDecimal BookedWidth
		{
			get { return CommonBookedCtgMove.EW_BookedWidth; }
		}

		public ZDecimal BookedHeight
		{
			get { return CommonBookedCtgMove.EW_BookedHeight; }
		}

		public ZString BookedDimensionUnits
		{
			get { return CommonBookedCtgMove.EW_DimUnit; }
		}

		#endregion
	}
}
