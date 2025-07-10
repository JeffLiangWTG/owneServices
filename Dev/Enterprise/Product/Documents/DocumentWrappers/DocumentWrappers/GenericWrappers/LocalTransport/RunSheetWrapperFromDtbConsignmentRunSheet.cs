using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.TransportConsignment.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class RunSheetWrapperFromDtbConsignmentRunSheet : RunSheetWrapper
	{
		public RunSheetWrapperFromDtbConsignmentRunSheet(DtbConsignmentRunSheet runSheet, BusinessObjectFactory factory)
			: base(runSheet ?? factory.GetNull<DtbConsignmentRunSheet>(), factory)
		{
		}

		#region Properties

		protected override ZString GetDriversName()
		{
			return RunSheetBO.DriversName;
		}

		protected override ZString GetVehicleRegistration()
		{
			return RunSheetBO.TruckRegistration;
		}

		protected override ZString GetTransportCompanyName()
		{
			return RunSheetBO.TransportCoName;
		}

		protected override ZDateTime GetStartDate()
		{
			return RunSheetBO.KG_StartTime.ToLocalZDateTime();
		}

		protected override ZDateTime GetEndDate()
		{
			return RunSheetBO.KG_EndTime.ToLocalZDateTime();
		}

		protected override EquipmentWrapper GetVehicle()
		{
			return RunSheetBO.Truck != null ? new EquipmentWrapper(RunSheetBO.Truck, Factory) : null;
		}

		protected override ZString GetDuration()
		{
			var time = new TotalHoursHelper();
			return time.GetTextFromTime(RunSheetBO.KG_Duration);
		}

		#endregion

		#region RunSheetBO

		DtbConsignmentRunSheet RunSheetBO
		{
			get { return (DtbConsignmentRunSheet)WrappedBO; }
		}

		#endregion
	}
}
