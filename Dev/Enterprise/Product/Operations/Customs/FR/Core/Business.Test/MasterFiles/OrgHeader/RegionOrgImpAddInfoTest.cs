using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MasterFiles.Testing
{
	[TestedType(typeof(RegionOrgImpAddInfo))]
	public class RegionOrgImpAddInfoTest : EU.Business.Testing.EUOrgImpAddInfoAbstractTest
	{
		protected override BusinessObject GetNewBusinessObject() => new RegionOrgImpAddInfo(Factory);
	}
}
