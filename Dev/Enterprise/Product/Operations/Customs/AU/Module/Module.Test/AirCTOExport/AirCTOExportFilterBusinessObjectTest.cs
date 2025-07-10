using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(AirCTOExportFilterBusinessObject))]
	sealed class AirCTOExportFilterBusinessObjectTest : ExportCustomsManifestFilterBusinessObjectTest
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new AirCTOExportFilterBusinessObject(true, true);
	}
}
