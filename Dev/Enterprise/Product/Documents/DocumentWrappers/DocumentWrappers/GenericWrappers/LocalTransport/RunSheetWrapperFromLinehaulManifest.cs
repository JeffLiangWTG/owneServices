using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.TransportConsignment.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class RunSheetWrapperFromLinehaulManifest : RunSheetWrapper
	{
		public RunSheetWrapperFromLinehaulManifest(DtbLinehaulManifest linehaulManifest, BusinessObjectFactory factory)
			: base(linehaulManifest ?? factory.GetNull<DtbLinehaulManifest>(), factory)
		{
		}

		#region Properties

		protected override ZString GetDriversName()
		{
			return LinehaulManifestBO.DriversName;
		}

		protected override ZString GetVehicleRegistration()
		{
			return LinehaulManifestBO.TruckRegistration;
		}

		protected override ZString GetTransportCompanyName()
		{
			return LinehaulManifestBO.TransportCompany != null ? LinehaulManifestBO.TransportCompany.OH_Code : ZString.Empty;
		}

		protected override ZDateTime GetStartDate()
		{
			return LinehaulManifestBO.LHM_StartDateTimeUtc;
		}

		protected override ZDateTime GetEndDate()
		{
			return LinehaulManifestBO.EndDateTimeUTC;
		}

		protected override EquipmentWrapper GetVehicle()
		{
			return null; // LinehaulManifestBO.PrimaryEquipment != null ? new EquipmentWrapper(LinehaulManifestBO.PrimaryEquipment, Factory) : null;
		}

		protected override ZString GetDuration()
		{
			var duration = LinehaulManifestBO.EndDateTimeUTC - LinehaulManifestBO.LHM_StartDateTimeUtc;
			var time = new TotalHoursHelper();
			return time.GetTextFromTime(duration);
		}

		#endregion

		#region RunSheetBO

		DtbLinehaulManifest LinehaulManifestBO
		{
			get { return (DtbLinehaulManifest)WrappedBO; }
		}

		#endregion
	}
}
