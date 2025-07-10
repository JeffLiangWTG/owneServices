using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocCommonWorkSheet : DocBaseWrapper
	{
		protected DocCommonWorkSheet(CommonWorkSheet workSheet, BusinessObjectFactory factoryToWrap)
			: base(workSheet, factoryToWrap)
		{
		}

		public override string ToString()
		{
			return ZString.Empty;
		}

		public static DocCommonWorkSheet New(CommonWorkSheet workSheet, BusinessObjectFactory factoryToWrap)
		{
			return (workSheet != null) ? new DocCommonWorkSheet(workSheet, factoryToWrap) : null;
		}

		public DocCommonCartageLegCollection CartageLegs
		{
			get { return GetDocCartageLegs(); }
		}

		public DocCommonCartageLegCollection ContainerLegs
		{
			get { return GetDocCartageLegs(); }
		}

		protected virtual DocCommonCartageLegCollection GetDocCartageLegs()
		{
			DocCommonCartageLegCollection result = new DocCommonCartageLegCollection(WorkSheet);
			result.Sort("Sequence");
			return result;
		}

		public ZString ContainerLegDriverName
		{
			get { return WorkSheet.EY_DriversName; }
		}

		public ZString ContainerLegVehicleRego
		{
			get { return WorkSheet.EY_TruckRegistration; }
		}

		public ZString TransportCompanyName
		{
			get { return WorkSheet.TransportCompanyName; }
		}

		public ZString WorkSheetDate
		{
			get { return WorkSheet.EY_StartTime.ToShortDateString() + " / " + WorkSheet.EY_EndTime.ToShortDateString(); }
		}

		public ZString RunSheetNumber
		{
			get { return WorkSheet.EY_RunSheetNumber; }
		}

		CommonWorkSheet WorkSheet
		{
			get { return (CommonWorkSheet)WrappedObject; }
		}
	}
}
