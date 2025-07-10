using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Transforms.Security;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Security
{
	[TestedType(typeof(AddHVLVShipmentsItemDetailsReport))]
	public class AddHVLVShipmentsItemDetailsReportTest : AddReportsToNeoGroupTest
	{
		protected override string ReportType => "RepFreightReport";

		protected override string ReportName => "Client - HVLV Shipments Item Details Report";

		protected override string GroupCode => "RepFreight00063";

		protected override DataTransformation GetNewTestTransformationInstance() => new AddHVLVShipmentsItemDetailsReport();
	}
}
