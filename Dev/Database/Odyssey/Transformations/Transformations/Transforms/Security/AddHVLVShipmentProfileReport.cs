namespace Enterprise.DbUpgrader.Transformations.Transforms.Security
{
	public class AddHVLVShipmentProfileReport : AddReportsToNeoGroup
	{
		protected override string ReportType => "RepFreightReport";

		protected override string ReportName => "Client - HVLV Shipment Profile Report";

		protected override string GroupCode => "RepFreight00062";
	}
}
