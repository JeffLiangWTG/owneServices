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
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

sealed class HouseConsignmentCustomsStatusUserControlTest : TestCaseWithFactory
{
	public void TestStatusDropEdit()
	{
		using var control = new HouseConsignmentCustomsStatusUserControl();
		var statusDropEdit = control.AssertContainsControl<ZDropEdit>("StatusDropEdit", x => x
				.WithBindTo(nameof(NctsBill.B0_BillStatus))
		);

		AssertNull("CaptionResourceString", statusDropEdit.CaptionResourceString.Caption);
		var labelCaptionRenderProvider = new LabelCaptionRenderProvider();
		AssertEquals("HouseConsignmentCustomsStatusUserControl label is visible?", false, labelCaptionRenderProvider.GetLabelCaptionVisible(statusDropEdit));
	}

	public void TestDeleteRestoreToggleButtonLayout()
	{
		using var form = new ZForm(nctsBill);
		using var control = new HouseConsignmentCustomsStatusUserControl();
		form.Controls.Add(control);
		form.Show();

		CombineAssertions(() =>
		{
			AssertToggleButtonIsNotVisible(control);

			nctsBill.Header.MovementHeader.BM_Phase = "013";
			AssertToggleButtonCaption(control, "Delete Request");

			nctsBill.Header.MovementHeader.BM_MessageStatus = "SNT";
			AssertToggleButtonIsNotVisible(control);

			nctsBill.Header.MovementHeader.BM_MessageStatus = "ERR";
			nctsBill.B0_BillStatus = "DLR";
			AssertToggleButtonCaption(control, "Restore Item");

			nctsBill.B0_BillStatus = "DEL";
			AssertToggleButtonCaption(control, "Restore Item");
		});
	}

	public void TestDeleteRestoreToggleButtonLayout_WhenDataChangeInAnotherFactory()
	{
		using var form = new ZForm(nctsBill);
		using var control = new HouseConsignmentCustomsStatusUserControl();
		form.Controls.Add(control);
		form.Show();

		CombineAssertions(() =>
		{
			nctsBill.Header.MovementHeader.BM_Phase = "013";
			nctsBill.B0_BillStatus = "DEL";
			AssertToggleButtonCaption(control, "Restore Item");

			Factory.Save();

			var differentFactory = new BusinessObjectFactory();
			var movementHeaderLoadedInDifferentFactory = differentFactory.Load<NctsBill>(nctsBill.PK);
			movementHeaderLoadedInDifferentFactory.Header.MovementHeader.BM_MessageStatus = "SNT";
			differentFactory.Save();

			nctsBill.Header.Reload();
			AssertToggleButtonIsNotVisible(control);
		});
	}

	public void TestDeleteRestoreToggleButtonLayout_OnChangingSelectedRow()
	{
		var nctsBill2 = header.Bills.AddNew();
		movementHeader.BM_Phase = "013";
		nctsBill.B0_BillStatus = "";
		nctsBill2.B0_BillStatus = "DLR";

		using var form = new ZForm(header);
		using var grid = new ZGrid();
		using var control = new HouseConsignmentCustomsStatusUserControl();

		grid.ColumnStyles.Add(new ZCodeFindBoxColumnStyleInfo());

		form.Controls.Add(grid);
		form.Controls.Add(control);
		form.SetDataBinding(header, "");

		grid.SetBindingMember("Bills");
		control.SetBindingMember("Bills");

		form.Show();

		CombineAssertions(() =>
		{
			AssertToggleButtonCaption(control, "Delete Request");

			grid.SelectSingleElement(nctsBill2);
			AssertToggleButtonCaption(control, "Restore Item");
		});
	}
	[RequiresSTA]
	public void TestDeleteRestoreToggleButtonActions()
	{
		nctsBill.Header.MovementHeader.BM_Phase = "013";

		using var form = new ZForm(nctsBill);
		using var control = new HouseConsignmentCustomsStatusUserControl();
		form.Controls.Add(control);
		form.Show();

		CombineAssertions(() =>
		{
			ClickButton();
			AssertEquals("After clicking Delete Request", "DLR", nctsBill.B0_BillStatus);

			UnitTestUserNotification.Instance.AddYesAnswer();
			ClickButton();
			AssertEquals("After clicking Restore Item", "", nctsBill.B0_BillStatus);
			AssertNull("When B0_BillStatus was DLR, Feedback message",
				UnitTestUserNotification.Instance.LastMessage.Text);

			nctsBill.B0_BillStatus = "DEL";
			UnitTestUserNotification.Instance.AddYesAnswer();
			ClickButton();
			AssertEquals("After clicking Restore Item", "", nctsBill.B0_BillStatus);

			AssertUserNotificationMessage("When B0_BillStatus was DEL. Confirmation",
				UnitTestUserNotification.Instance.PreviousMessages.Skip(1).First(),
				"Are you sure you want to reactivate deleted (DEL) House Consignment number 1?",
				"Confirm Reactivation");

			AssertUserNotificationMessage("When B0_BillStatus was DEL, Feedback",
				UnitTestUserNotification.Instance.LastMessage,
				"House Consignment number 1 is reactivated",
				"Reactivation Feedback");

			void ClickButton() => control.FindSingleOrDefault<ZButton>(ButtonName)?.PerformClick();

			void AssertUserNotificationMessage(string assertionMessage, UnitTestUserNotification.PreviousMessage userNotification, string expectedMessage, string expectedCaption)
			{
				AssertEquals($"{assertionMessage} Message", expectedMessage, userNotification?.Text);
				AssertEquals($"{assertionMessage} Caption", expectedCaption, userNotification?.Caption);
			}
		});
	}

	public void TestImplementIExtendedControl()
	{
		using var control = new HouseConsignmentCustomsStatusUserControl();
		var houseConsignmentCustomsStatusExtendedControl = control as IExtendedControl;

		AssertNotNull("HouseConsignmentCustomsStatusUserControl implements IExtendedControl?", houseConsignmentCustomsStatusExtendedControl);

		AssertEquals("Host", houseConsignmentCustomsStatusExtendedControl, houseConsignmentCustomsStatusExtendedControl.Host);
		AssertNotNull("Extensions", houseConsignmentCustomsStatusExtendedControl.Extensions);
	}

	public void TestImplementIResourceStringBindingMember()
	{
		using var control = new HouseConsignmentCustomsStatusUserControl();
		var houseConsignmentCustomsStatusResourceStringBindingMember = control as IResourceStringBindingMember;

		AssertNotNull("HouseConsignmentCustomsStatusUserControl implements IResourceStringBindingMember?", houseConsignmentCustomsStatusResourceStringBindingMember);
		AssertEquals("ResourceStringBindingMember", "B0_BillStatus", houseConsignmentCustomsStatusResourceStringBindingMember.ResourceStringBindingMember);
	}

	public void TestDataSourceType()
	{
		using var control = new HouseConsignmentCustomsStatusUserControl();
		AssertEquals("DataSourceType", typeof(NctsBill), control.DataSourceType);
	}

	protected override void SetUp()
	{
		base.SetUp();

		header = Factory.New<NctsHeader>();
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		movementHeader = header.MovementHeader;
		nctsBill = header.Bills.AddNew();
	}
	NctsDepartureMovementHeader movementHeader;
	NctsHeader header;
	NctsBill nctsBill;

	void AssertToggleButtonIsNotVisible(Control control ) => control.AssertContainsControl(ButtonName, new UserControlAssertStrategies<ZButton>().WithIsVisible(isVisible: false));

	void AssertToggleButtonCaption(Control control, string caption)
	{
		control.AssertContainsControl<ZButton>(ButtonName, x => x
			.WithIsVisible(isVisible: true)
			.WithStrategy("CaptionResourceString", caption, (self, button, buttonName) => AssertEquals("Control " + buttonName + " " + self.name, self.expected, button.CaptionResourceString?.Caption))
			);
	}

	const string ButtonName = "DeleteRestoreToggleButton";
}
