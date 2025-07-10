using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.LocalCartage.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class LocalTransportBookedMoveWrapper : GenericWrapper
	{
		#region Constructors

		public LocalTransportBookedMoveWrapper(CommonBookedCtgMove bookedMove, BusinessObjectFactory factory)
			: base(bookedMove ?? factory.GetNull<CommonBookedCtgMove>(), factory)
		{
		}

		#endregion

		#region Properties

		public ZString BookedDimensionUnits
		{
			get { return BookedMoveBO.EW_DimUnit; }
		}

		public ZDecimal BookedHeight
		{
			get { return BookedMoveBO.EW_BookedHeight; }
		}

		public ZDecimal BookedLength
		{
			get { return BookedMoveBO.EW_BookedLength; }
		}

		public ZInt BookedPackages
		{
			get { return BookedMoveBO.EW_BookedPackCount; }
		}

		public ZString BookedPackType
		{
			get { return BookedMoveBO.EW_F3_NKPackType; }
		}

		public ZDecimal BookedWeight
		{
			get { return BookedMoveBO.EW_BookedWeight; }
		}

		public ZString BookedWeightUnit
		{
			get { return BookedMoveBO.EW_WeightUQ; }
		}

		public ZDecimal BookedWidth
		{
			get { return BookedMoveBO.EW_BookedWidth; }
		}

		public ZDecimal BookedVolume
		{
			get { return BookedMoveBO.EW_BookedVolume; }
		}

		public ZString BookedVolumeUnit
		{
			get { return BookedMoveBO.EW_VolumeUQ; }
		}

		public ZString DropMode
		{
			get { return BookedMoveBO.EW_DropMode; }
		}

		public ZShort DisplayOrder
		{
			get { return BookedMoveBO.EW_DisplayOrder; }
		}

		#endregion

		#region Wrapped Entities

		#region BookedMoveBO

		CommonBookedCtgMove BookedMoveBO
		{
			get { return (CommonBookedCtgMove)WrappedBO; }
		}

		#endregion

		#endregion

		#region Related Objects

		#region Cartage

		public FreightWrapperFromCartage Cartage
		{
			get { return new FreightWrapperFromCartage(BookedMoveBO.Cartage, Factory); }
		}

		#endregion

		#region Container

		public ContainerWrapperFromCartage Container
		{
			get { return new ContainerWrapperFromCartage(new FreightWrapperFromCartage(BookedMoveBO.Cartage, Factory), BookedMoveBO.Container, Factory); }
		}

		#endregion

		#region DeliveryDocAddress

		public AddressWrapper DeliveryDocAddress
		{
			get { return new AddressWrapper(BookedMoveBO.WaitPointDocAddress, Factory); } //WaitPoint is currently used as Delivery
		}

		#endregion

		#region LocalTransportLegs

		public LocalTransportLegWrapperCollection LocalTransportLegs
		{
			get
			{
				LocalTransportLegWrapperCollection result = new LocalTransportLegWrapperCollection(Cartage, Factory, BookedMoveBO.CartageLegs);
				result.Sort(LocalTransportLegWrapperCollection.SortBy.PlannedPickup);
				return result;
			}
		}

		#endregion

		#region PickupDocAddress

		public AddressWrapper PickupDocAddress
		{
			get { return new AddressWrapper(BookedMoveBO.PickupFromDocAddress, Factory); }
		}

		#endregion

		#endregion
	}
}
