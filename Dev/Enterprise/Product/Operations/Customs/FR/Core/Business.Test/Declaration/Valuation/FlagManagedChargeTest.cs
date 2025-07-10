using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class FlagManagedChargeTest : TestCaseWithFactory
	{
		public void TestMessageChargeKeyPropertiesMatchChargeProperties()
		{
			var charge = new FlagManagedCharge("BLA", (NoResString)"blabla");
			charge.IsDutiable = true;
			charge.IsVATible = true;
			charge.IsIncludedInITOT = true;
			charge.IsStatisticalValueApplicable = true;

			var actualMessageChargeKey = charge.MessageChargeKey;
			AssertEquals(true, actualMessageChargeKey.IsDutiable);
			AssertEquals(true, actualMessageChargeKey.IsIncludedInITOT);
			AssertEquals(true, actualMessageChargeKey.IsStatisticalValueApplicable);
			AssertEquals(true, actualMessageChargeKey.IsVATible);

			charge.IsDutiable = false;
			charge.IsVATible = false;
			charge.IsIncludedInITOT = false;
			charge.IsStatisticalValueApplicable = false;
			actualMessageChargeKey = charge.MessageChargeKey;
			AssertEquals(false, actualMessageChargeKey.IsDutiable);
			AssertEquals(false, actualMessageChargeKey.IsIncludedInITOT);
			AssertEquals(false, actualMessageChargeKey.IsStatisticalValueApplicable);
			AssertEquals(false, actualMessageChargeKey.IsVATible);
		}
	}
}
