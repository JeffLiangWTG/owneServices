using CargoWise.EntityFramework;
using PopulatedFilterStripBizO = Enterprise.ZArchitecture.Web.Business.FilterStrips.Testing.FilterStripURLParameterHelperTest.FilterStripBusinessObjectForTest;

namespace Enterprise.ZArchitecture.Web.GUI.FilterStrips.Testing
{
	public sealed class PageForTestWithPopulatedFilterStrip : PageForTest
	{
		protected override BusinessObject GetNewDataSource()
		{
			return new PopulatedFilterStripBizO();
		}
	}
}
