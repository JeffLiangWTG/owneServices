namespace Enterprise.DbUpgrader.Transformations.Transforms.Security
{
	public class AddHVLVShipmentsItemDetailsReport : AddReportsToNeoGroup
	{
		protected override string ReportType => "RepFreightReport";

		protected override string ReportName => "Client - HVLV Shipments Item Details Report";

		protected override string GroupCode => "RepFreight00063";
	}
}
