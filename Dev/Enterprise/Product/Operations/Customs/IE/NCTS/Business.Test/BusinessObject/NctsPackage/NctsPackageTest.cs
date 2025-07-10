using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsPackage))]
	sealed class NctsPackageTest : CusInvPackTest<NctsDepartureCargoDesc>
	{
		public void TestPhase5ValidationType()
		{
			AssertType<NctsPackagePhase5Validation>(packagePivot.Validation);
		}

		protected override NctsDepartureCargoDesc GetNewParent()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			return nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		}
	}
}
