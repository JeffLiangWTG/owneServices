using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(UnloadingRemarkAddInfo))]
	class UnloadingRemarkAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLookups()
		{
			AssertType<UnloadingRemarkAddInfoLookups>(nctsHeader.UnloadingRemark.Lookups);
		}

		protected override BusinessObject GetNewBusinessObject() => nctsHeader.UnloadingRemark;

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		}
		NctsHeader nctsHeader;
	}
}
