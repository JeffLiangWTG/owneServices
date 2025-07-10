using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(VesselVoyageAddInfo))]
	class VesselVoyageAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new VesselVoyageAddInfo(Factory.New<VesselVoyage>().B7_AddInfoDataInfo);
	}
}
