using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class RunSheetWrapperFromCommonRunSheet : RunSheetWrapper
	{
		public RunSheetWrapperFromCommonRunSheet(CommonWorkSheet runSheet, BusinessObjectFactory factory)
			: base(runSheet ?? factory.GetNull<CommonWorkSheet>(), factory)
		{
		}

		#region Properties

		protected override ZString GetDriversName()
		{
			return RunSheetBO.EY_DriversName;
		}

		protected override ZString GetVehicleRegistration()
		{
			return RunSheetBO.EY_TruckRegistration;
		}

		protected override ZString GetTransportCompanyName()
		{
			return RunSheetBO.TransportCompanyName;
		}

		protected override ZDateTime GetStartDate()
		{
			return RunSheetBO.EY_StartTime;
		}

		protected override ZDateTime GetEndDate()
		{
			return RunSheetBO.EY_EndTime;
		}

		protected override EquipmentWrapper GetVehicle()
		{
			return RunSheetBO.Truck != null ? new EquipmentWrapper(RunSheetBO.Truck, Factory) : null;
		}

		protected override ZString GetDuration()
		{
			return ZString.Empty;
		}

		#endregion

		#region RunSheetBO

		CommonWorkSheet RunSheetBO
		{
			get { return (CommonWorkSheet)WrappedBO; }
		}

		#endregion
	}
}
