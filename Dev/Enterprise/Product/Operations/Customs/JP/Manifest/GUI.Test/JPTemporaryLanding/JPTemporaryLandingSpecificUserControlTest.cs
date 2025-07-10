using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI.UserControls.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.GUI.Testing
{
	[TestedType(typeof(JPTemporaryLandingSpecificUserControl))]
	sealed class JPTemporaryLandingSpecificUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new JPTemporaryLandingSpecificUserControl())
			{
				TestHelper.AssertControlExists(control, "TemporaryLandingReasonDropEdit", "TemporaryLandingReason");
				TestHelper.AssertControlExists(control, "TemporaryLandingPeriodDaysCalcEdit", "TemporaryLandingPeriodDays");
				TestHelper.AssertControlExists(control, "TemporaryLandingStartDateEdit", "TemporaryLandingStartDate");
				TestHelper.AssertControlExists(control, "TemporaryLandingEndDateEdit", "TemporaryLandingEndDate");
				TestHelper.AssertControlExists(control, "TemporaryLandingBondedTransportCodeDropEdit", "TemporaryLandingBondedTransportCode");
				TestHelper.AssertControlExists(control, "GoodsLocationCodeFindBox", "ABL_GoodsLocation");
			}
		}
	}
}
