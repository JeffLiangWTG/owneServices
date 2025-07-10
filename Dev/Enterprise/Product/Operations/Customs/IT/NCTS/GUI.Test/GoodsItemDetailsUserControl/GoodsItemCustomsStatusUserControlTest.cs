using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

sealed class GoodsItemCustomsStatusUserControlTest : TestCaseWithFactory
{
	public void TestStatusDropEdit()
	{
		using var control = new GoodsItemCustomsStatusUserControl();
		var statusDropEdit = control.AssertContainsControl<ZDropEdit>("StatusDropEdit", x => x
				.WithBindTo(nameof(NctsDepartureCargoDesc.BY_Status))
		);

		AssertNull("CaptionResourceString", statusDropEdit.CaptionResourceString.Caption);
		var labelCaptionRenderProvider = new LabelCaptionRenderProvider();
		AssertEquals("GoodsItemCustomsStatusUserControl label is visible?", false, labelCaptionRenderProvider.GetLabelCaptionVisible(statusDropEdit));
	}

	public void TestDeleteRestoreToggleButtonLayout()
	{
		using var form = new ZForm(goodsItem);
		using var control = new GoodsItemCustomsStatusUserControl();
		form.Controls.Add(control);
		form.Show();

		CombineAssertions(() =>
		{
			AssertToggleButtonIsNotVisible(control);

			movementHeader.BM_Phase = "013";
			AssertToggleButtonCaption(control, "Delete Request");

			movementHeader.BM_MessageStatus = "SNT";
			AssertToggleButtonIsNotVisible(control);

			movementHeader.BM_MessageStatus = "ERR";
			goodsItem.BY_Status = "DLR";
			AssertToggleButtonCaption(control, "Restore Item");

			goodsItem.BY_Status = "DEL";
			AssertToggleButtonCaption(control, "Restore Item");
		});
	}

	public void TestDeleteRestoreToggleButtonLayout_WhenDataChangeInAnotherFactory()
	{
		using var form = new ZForm(goodsItem);
		using var control = new GoodsItemCustomsStatusUserControl();
		form.Controls.Add(control);
		form.Show();

		CombineAssertions(() =>
		{
			movementHeader.BM_Phase = "013";
			goodsItem.BY_Status = "DEL";
			AssertToggleButtonCaption(control, "Restore Item");

			Factory.Save();

			var differentFactory = new BusinessObjectFactory();
			var movementHeaderLoadedInDifferentFactory = differentFactory.Load<NctsDepartureMovementHeader>(movementHeader.PK);
			movementHeaderLoadedInDifferentFactory.BM_MessageStatus = "SNT";
			differentFactory.Save();

			movementHeader.Header.Reload();
			AssertToggleButtonIsNotVisible(control);
		});
	}

	public void TestDeleteRestoreToggleButtonLayout_OnChangingSelectedRow()
	{
		var bill = goodsItem.Bill;
		var goodsItem2 = bill.GoodsItems.AddNew();
		movementHeader.BM_Phase = "013";
		goodsItem.BY_Status = "";
		goodsItem2.BY_Status = "DEL";

		using var form = new ZForm(bill);
		using var grid = new ZGrid();
		using var control = new GoodsItemCustomsStatusUserControl();

		grid.ColumnStyles.Add(new ZCodeFindBoxColumnStyleInfo());

		form.Controls.Add(grid);
		form.Controls.Add(control);
		form.SetDataBinding(bill, "");

		grid.SetBindingMember("GoodsItems");
		control.SetBindingMember("GoodsItems");

		form.Show();

		CombineAssertions(() =>
		{
			AssertToggleButtonCaption(control, "Delete Request");

			grid.SelectSingleElement(goodsItem2);
			AssertToggleButtonCaption(control, "Restore Item");
		});
	}

	public void TestDeleteRestoreToggleButtonActions()
	{
		movementHeader.BM_Phase = "013";

		using var form = new ZForm(goodsItem);
		using var control = new GoodsItemCustomsStatusUserControl();
		form.Controls.Add(control);
		form.Show();

		CombineAssertions(() =>
		{
			ClickButton();
			AssertEquals("After clicking Delete Request", "DLR", goodsItem.BY_Status);

			UnitTestUserNotification.Instance.AddYesAnswer();
			ClickButton();
			AssertEquals("After clicking Restore Item", "", goodsItem.BY_Status);
			AssertNull("When BY_Status was DLR, Feedback message",
				UnitTestUserNotification.Instance.LastMessage.Text);

			goodsItem.BY_Status = "DEL";
			UnitTestUserNotification.Instance.AddYesAnswer();
			ClickButton();
			AssertEquals("After clicking Restore Item", "", goodsItem.BY_Status);

			AssertUserNotificationMessage("When BY_Status was DEL. Confirmation",
				UnitTestUserNotification.Instance.PreviousMessages.Skip(1).First(),
				"Are you sure you want to reactivate deleted (DEL) goods item number 1?",
				"Confirm Reactivation");

			AssertUserNotificationMessage("When BY_Status was DEL, Feedback",
				UnitTestUserNotification.Instance.LastMessage,
				"Goods Item number 1 is reactivated",
				"Reactivation Feedback");

			void ClickButton()
				=> control.FindSingleOrDefault<ZButton>(ButtonName)?.PerformClick();

			void AssertUserNotificationMessage(string assertionMessage,
				UnitTestUserNotification.PreviousMessage userNotification,
				string expectedMessage,
				string expectedCaption)
			{
				AssertEquals($"{assertionMessage} Message", expectedMessage, userNotification?.Text);
				AssertEquals($"{assertionMessage} Caption", expectedCaption, userNotification?.Caption);
			}
		});
	}

	public void TestImplementIExtendedControl()
	{
		using var control = new GoodsItemCustomsStatusUserControl();
		var goodsItemCustomsStatusExtendedControl = control as IExtendedControl;

		AssertNotNull("GoodsItemCustomsStatusUserControl implements IExtendedControl?", goodsItemCustomsStatusExtendedControl);

		AssertEquals("Host", goodsItemCustomsStatusExtendedControl, goodsItemCustomsStatusExtendedControl.Host);
		AssertNotNull("Extensions", goodsItemCustomsStatusExtendedControl.Extensions);
	}

	public void TestImplementIResourceStringBindingMember()
	{
		using var control = new GoodsItemCustomsStatusUserControl();
		var goodsItemCustomsStatusResourceStringBindingMember = control as IResourceStringBindingMember;

		AssertNotNull("GoodsItemCustomsStatusUserControl implements IResourceStringBindingMember?", goodsItemCustomsStatusResourceStringBindingMember);
		AssertEquals("ResourceStringBindingMember", "BY_Status", goodsItemCustomsStatusResourceStringBindingMember.ResourceStringBindingMember);
	}

	public void TestDataSourceType()
	{
		using var control = new GoodsItemCustomsStatusUserControl();
		AssertEquals("DataSourceType", typeof(NctsDepartureCargoDesc), control.DataSourceType);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var header = Factory.New<NctsHeader>();
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		movementHeader = header.MovementHeader;
		var bill = header.Bills.AddNew();
		goodsItem = bill.GoodsItems.AddNew();
	}

	NctsDepartureMovementHeader movementHeader;
	NctsDepartureCargoDesc goodsItem;

	void AssertToggleButtonIsNotVisible(Control control)
		=> control.AssertContainsControl(ButtonName,
		new UserControlAssertStrategies<ZButton>().WithIsVisible(isVisible: false));

	void AssertToggleButtonCaption(Control control, string caption)
	{
		control.AssertContainsControl<ZButton>(ButtonName, x => x
				.WithIsVisible(isVisible: true)
				.WithStrategy("CaptionResourceString", caption, (self, button, buttonName) => AssertEquals("Control " + buttonName + " " + self.name, self.expected, button.CaptionResourceString?.Caption))
		);
	}

	const string ButtonName = "DeleteRestoreToggleButton";
}
