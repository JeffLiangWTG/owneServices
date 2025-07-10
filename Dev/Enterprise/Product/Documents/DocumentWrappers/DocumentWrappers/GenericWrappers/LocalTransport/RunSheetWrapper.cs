using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	/// <remarks>
	/// Non Standard Run Sheet Only fields
	/// </remarks>
	public abstract class RunSheetWrapper : GenericWrapper
	{
		protected RunSheetWrapper(BusinessObject businessObjectToWrap, BusinessObjectFactory factory)
			: base(businessObjectToWrap, factory)
		{
		}

		#region Properties

		public ZString DriversName
		{
			get { return GetDriversName(); }
		}
		protected abstract ZString GetDriversName();

		public ZString VehicleRegistration
		{
			get { return GetVehicleRegistration(); }
		}
		protected abstract ZString GetVehicleRegistration();

		public ZString TransportCompanyName
		{
			get { return GetTransportCompanyName(); }
		}
		protected abstract ZString GetTransportCompanyName();

		public ZDateTime StartDate
		{
			get { return GetStartDate(); }
		}
		protected abstract ZDateTime GetStartDate();

		public ZDateTime EndDate
		{
			get { return GetEndDate(); }
		}
		protected abstract ZDateTime GetEndDate();

		public EquipmentWrapper Vehicle
		{
			get { return GetVehicle(); }
		}
		protected abstract EquipmentWrapper GetVehicle();

		public ZString Duration
		{
			get { return GetDuration(); }
		}
		protected abstract ZString GetDuration();

		#endregion
	}
}
