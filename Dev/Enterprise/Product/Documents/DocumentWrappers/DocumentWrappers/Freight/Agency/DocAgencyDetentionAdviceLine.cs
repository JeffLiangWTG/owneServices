using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Agency.Business;

namespace Enterprise.DocumentWrappers
{
	public abstract class DocAgencyDetentionAdviceLine : GenericWrapper
	{
		protected DocAgencyDetentionAdviceLine(BusinessObject wrappedObject, BusinessObjectFactory factory)
			: base(wrappedObject, factory) { }

		public static DocAgencyDetentionAdviceLine New(BillOfLadingContainer container, BusinessObjectFactory factory)
		{
			return container == null ? null : new DocAgencyDetentionAdviceLine_FromContainer(container, factory);
		}

		public static DocAgencyDetentionAdviceLine New(ContainerMovement movement, BusinessObjectFactory factory)
		{
			return movement == null ? null : new DocAgencyDetentionAdviceLine_FromMovement(movement, factory);
		}

		// should really be a CodeAndDescriptionWrapper but then it cant be used for filtering.
		public ZString DetentionType
		{
			get { return GetDetentionType(); }
		}
		protected abstract ZString GetDetentionType();

		public ZString ContainerNo
		{
			get { return GetContainerNo(); }
		}
		protected abstract ZString GetContainerNo();

		public ContainerTypeWrapper ContainerType
		{
			get { return containerType ?? (containerType = GetContainerType()); }
		}
		protected abstract ContainerTypeWrapper GetContainerType();
		ContainerTypeWrapper containerType;

		public ZString VesselName
		{
			get { return GetVesselName(); }
		}
		protected abstract ZString GetVesselName();

		public ZString VoyageNo
		{
			get { return GetVoyageNo(); }
		}
		protected abstract ZString GetVoyageNo();

		public LocationWrapper DetentionPort
		{
			get { return GetDetentionPort(); }
		}
		protected abstract LocationWrapper GetDetentionPort();

		public ZString BillNumber
		{
			get { return GetBillNumber(); }
		}
		protected abstract ZString GetBillNumber();

		public ZDateTime ReleaseDate
		{
			get { return GetReleaseDate(); }
		}
		protected abstract ZDateTime GetReleaseDate();

		public ZDateTime RequiredDate
		{
			get { return GetRequiredDate(); }
		}
		protected abstract ZDateTime GetRequiredDate();

		public ZDateTime PickupDate
		{
			get { return GetPickupDate(); }
		}
		protected abstract ZDateTime GetPickupDate();
	}
}
