using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Transforms.Security;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Security
{
	[TestedType(typeof(AddHVLVShipmentProfileReport))]
	public class AddHVLVShipmentProfileReportTest : AddReportsToNeoGroupTest
	{
		protected override string ReportType => "RepFreightReport";

		protected override string ReportName => "Client - HVLV Shipment Profile Report";

		protected override string GroupCode => "RepFreight00062";

		protected override DataTransformation GetNewTestTransformationInstance() => new AddHVLVShipmentProfileReport();
	}
}
