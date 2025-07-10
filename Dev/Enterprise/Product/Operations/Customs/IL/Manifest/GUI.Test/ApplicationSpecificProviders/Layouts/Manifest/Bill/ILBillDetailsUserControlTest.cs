using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.IL.Manifest.Business;

namespace Enterprise.Customs.IL.Manifest.GUI.Testing
{
	sealed class ILBillDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestDischargePortCodeFindBox()
		{
			var dischargePortCodeFindBox = form.DischargePortCodeFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZArchitecture.GUI.ZCodeFindBox>(dischargePortCodeFindBox);
				AssertEquals("BindingMember", nameof(AsycudaBill.ABL_RL_NKPortOfDischarge), dischargePortCodeFindBox.GetBindingMember());
				AssertEquals("Caption", "Discharge Port", dischargePortCodeFindBox.CaptionResourceString.Caption);
			});
		}

		public void TestConditionDropEdit()
		{
			var conditionDropEdit = form.ConditionDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZArchitecture.GUI.ZDropEdit>(conditionDropEdit);
				AssertEquals("BindingMember", nameof(AsycudaBill.ABL_Condition), conditionDropEdit.GetBindingMember());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			form = new ILBillDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();

			form.Dispose();
		}

		ILBillDetailsUserControl form;
	}
}
