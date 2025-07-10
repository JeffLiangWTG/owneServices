using System;
using Enterprise.Customs.AE.Manifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.Manifest.GUI;

public partial class BillsSelectionDialog : ASYCUDA.GUI.AsycudaItemSelectionDialog
{
	[Obsolete("This constructor is just for the designer")]
	public BillsSelectionDialog()
	{
		InitializeComponent();
	}

	public BillsSelectionDialog(MessageChooser messageChooser, string itemsType)
		: base(messageChooser, itemsType)
	{
		ItemsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
		MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 472);
		InitializeComponent();
		InitializeColumns();
		HideSelectButtonAndLabel();
		this.SplitContainer.BringToFront();
	}

	void InitializeColumns()
	{
		var descriptionColumn = ItemsGrid.GetColumnStyle("Description");
		ItemsGrid.ColumnStyles.Remove(descriptionColumn);

		var entryTypeColumn = new ZDropEditColumnStyleInfo();
		entryTypeColumn.ColumnName = nameof(MessageChooserItem.EntryType);
		entryTypeColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

		var billnumberColumn = new ZArchitecture.ZTextBoxColumnStyleInfo();
		billnumberColumn.ColumnName = nameof(MessageChooserItem.BOLNumber);
		billnumberColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);

		var customsStatusColumn = new ZArchitecture.ZTextBoxColumnStyleInfo();
		customsStatusColumn.ColumnName = nameof(MessageChooserItem.CustomsStatus);
		customsStatusColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);

		var subjectCodeColumn = new ZDropEditColumnStyleInfo();
		subjectCodeColumn.ColumnName = nameof(MessageChooserItem.SubjectCode);
		subjectCodeColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);

		var subjectColumn = new ZArchitecture.ZTextBoxColumnStyleInfo();
		subjectColumn.ColumnName = nameof(MessageChooserItem.Subject);
		subjectColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

		var cargoTypeColumn = new ZDropEditColumnStyleInfo();
		cargoTypeColumn.ColumnName = nameof(MessageChooserItem.CargoType);
		cargoTypeColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);

		ItemsGrid.ColumnStyles.Add(entryTypeColumn);
		ItemsGrid.ColumnStyles.Add(billnumberColumn);
		ItemsGrid.ColumnStyles.Add(customsStatusColumn);
		ItemsGrid.ColumnStyles.Add(subjectCodeColumn);
		ItemsGrid.ColumnStyles.Add(subjectColumn);
		ItemsGrid.ColumnStyles.Add(cargoTypeColumn);
	}

	protected override bool IsValidToSend()
	{
		if (!base.IsValidToSend())
		{
			return false;
		}

		BusinessEntity.RunPreSaveValidation();
		if (BusinessEntity.HasErrors)
		{
			ShowErrorsDialog();
			return false;
		}
		return true;
	}

	void HideSelectButtonAndLabel()
	{
		DeselectAllButton.Visible = false;
		SelectAllButton.Visible = false;
		DescPanel.Visible = false;
	}
}
