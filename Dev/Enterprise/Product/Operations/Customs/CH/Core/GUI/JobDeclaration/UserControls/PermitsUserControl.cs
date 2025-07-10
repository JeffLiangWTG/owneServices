using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public partial class PermitsUserControl : ZUserControl
{
	public PermitsUserControl()
	{
		InitializeComponent();
		InitializeGridLayout();
	}

	void InitializeGridLayout()
	{
		PermitsGrid.ColumnStyles.Add(new ZCodeFindBoxColumnStyleInfo
		{
			ColumnName = Permit.Schema.CSI_IssuerType,
		});

		PermitsGrid.ColumnStyles.Add(new ZDateEditColumnStyleInfo
		{
			ColumnName = Permit.Schema.CSI_DateOfIssue,
			DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short
		});

		PermitsGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
		{
			ColumnName = Permit.Schema.CSI_Description,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200),
		});

		PermitsGrid.ReOrderColumns(columnOrder);
	}

	readonly string[] columnOrder =
	{
			Permit.Schema.CSI_Code,
			Permit.Schema.CSI_IssuerType,
			Permit.Schema.CSI_ReferenceNumber,
			Permit.Schema.CSI_DateOfIssue,
			Permit.Schema.CSI_Description
		};

	const string IsVisibleForBindingString = nameof(ZGroupBox.IsVisibleForBinding);

	public override void SetDataBinding(object dataSource, string dataMember)
	{
		base.SetDataBinding(dataSource, dataMember);

		PermitItemDetailsGroupBox.DataBindings.RemoveBinding(IsVisibleForBindingString);

		if (dataSource != null)
		{
			PermitItemDetailsGroupBox.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, dataMember + "." + nameof(Permit.SupportsPermitItemDetails), false, DataSourceUpdateMode.Never));
		}
	}
}
