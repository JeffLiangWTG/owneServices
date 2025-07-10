using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(GuaranteeGroupBoxUserControl))]
	sealed class GuaranteeGroupBoxUserControlTest : TestCaseWithFactory
	{
		public void TestBondNumberCodeFindBox()
		{
			CombineAssertions(() =>
			{
				_ = control.AssertContainsControl<ZCodeFindBox>("BondNumberCodeFindBox", x => x
					.WithBindTo(nameof(CommonGuarantee.PW_BondNumber))
				);
			});
		}

		public void TestAmountCalcDropEdit()
		{
			CombineAssertions(() =>
			{
				_ = control.AssertContainsControl<ZCalcDropEdit>("AmountCalcDropEdit", x => x
					.WithBindToAmount(nameof(CommonGuarantee.PW_BondAmount))
					.WithBindToUnit(nameof(CommonGuarantee.PW_RX_NKCurrency))
				);
			});
		}

		public void TestOverrideCheckBox()
		{
			CombineAssertions(() =>
			{
				_ = control.AssertContainsControl<ZCheckBox>("OverrideCheckBox", x => x
					.WithBindTo(nameof(CommonGuarantee.PW_Override))
				);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new GuaranteeGroupBoxUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		GuaranteeGroupBoxUserControl control;
	}
}
