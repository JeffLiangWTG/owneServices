using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(GridColourStripBusinessObject.GridColourStripLayoutsHelper))]
	class GridColourStripLayoutsHelperTest : FilterStripLayoutsHelperTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var scheme = Factory.New<GridColourScheme>();
			var fbo = new FilterStripBusinessObjectForTest();
			var colorStrip = new GridColourStripBusinessObject(fbo, scheme, null);
			return new GridColourStripBusinessObject.GridColourStripLayoutsHelper(colorStrip);
		}
	}
}
