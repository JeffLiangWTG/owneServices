using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IE.Business.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(CusClassPartPivotConfiguration))]
	public class CusClassPartPivotConfigurationTest : CusClassPartPivotConfigurationAbstractTest
	{
		public override void TestUCCAdditionalInfosSupport()
		{
			CombineAssertions(() =>
			{
				partPivot.CI_ChildType = ClassificationType.EXP;
				AssertEquals("Enabled", true, partPivot.Configuration.UCCAdditionalInfosSupport(partPivot));
				partPivot.CI_ChildType = ClassificationType.IMP;
				AssertEquals("Disabled", false, partPivot.Configuration.UCCAdditionalInfosSupport(partPivot));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			partPivot = Factory.New<CusClassPartPivot>();
		}

		CusClassPartPivot partPivot;
	}
}
