using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing;

[TestedType(typeof(CommonBillUserControl))]
sealed class CommonBillUserControlTest : TestCaseWithFactory
{
	public void TestCargoTypeDropEdit()
	{
		var bill = Factory.New<AsycudaBill>();

		using (var form = new ZForm())
		using (var userControl = new CommonBillUserControl())
		{
			form.Controls.Add(userControl);
			form.Show();

			var dropEdit = userControl.FindSingle<ZDropEdit>("CargoTypeDropEdit");
			CombineAssertions(() =>
			{
				AssertEquals("BindTo", nameof(AsycudaBill.ABL_CargoType), dropEdit.BindTo);
				AssertEquals(2, dropEdit.PreBoundMaxLength);
				AssertEquals(true, dropEdit.Visible);
				Assert(dropEdit.ShowDescriptionInDropDown);
			});
		}
	}

	public void TestDeliveryAgentAddressControl()
	{
		var bill = Factory.New<AsycudaBill>();

		using (var form = new ZForm())
		using (var userControl = new CommonBillUserControl())
		{
			form.Controls.Add(userControl);
			form.Show();

			var addressControl = userControl.FindSingle<ZAddressControl>("DeliveryAgentAddressControl");
			CombineAssertions(() =>
			{
				AssertEquals("BindTo", nameof(AsycudaBill.ABL_OA_DeliveryAgent), addressControl.BindTo);
				AssertEquals("BindToOrgList", "Lookups.Organisations", addressControl.BindToOrgList);
				AssertEquals("Caption", "Delivery Agent", addressControl.CaptionResourceString.Caption);
				AssertEquals(true, addressControl.Visible);
			});
		}
	}
}
