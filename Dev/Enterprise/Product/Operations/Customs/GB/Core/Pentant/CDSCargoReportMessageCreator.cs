using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.Pentant
{
	public class CDSCargoReportMessageCreator : CargoReportMessageCreator
	{
		public CDSCargoReportMessageCreator(JobDeclaration declaration, CargoReportType reportType)
			: base(declaration, reportType)
		{ }

		protected override string GetPort() => wrapper.SubLocationOfGoods;

		protected override string GetVehicleNoPlate() => wrapper.Box18TransportID;

		protected override string GetPlaceArrivalExport() => wrapper.SubLocationOfGoods;
	}
}
