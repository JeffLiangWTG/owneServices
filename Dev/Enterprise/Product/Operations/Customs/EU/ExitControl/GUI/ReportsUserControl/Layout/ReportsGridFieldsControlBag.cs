using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI;

public sealed class ReportsGridFieldsControlBag : ControlBag
{
	protected override Control CreateTemplate() => new ReportsGridFieldsControl();

	[ThreadStatic]
	static ReportsGridFieldsControlBag instance;
	public static ReportsGridFieldsControlBag Instance => instance ?? (instance = new ReportsGridFieldsControlBag());

	public ReportsGridFieldsControlBag()
	{
		ConsignmentGuidDropEdit = RegisterControl(nameof(ReportsGridFieldsControl.ConsignmentGuidDropEdit));
		OfficeOfExitCodeFindBox = RegisterControl(nameof(ReportsGridFieldsControl.OfficeOfExitCodeFindBox));
		TransportTypeDropEdit = RegisterControl(nameof(ReportsGridFieldsControl.TransportTypeDropEdit));
		TransportIDTextBox = RegisterControl(nameof(ReportsGridFieldsControl.TransportIDTextBox));
		TransportNationalityDropEdit = RegisterControl(nameof(ReportsGridFieldsControl.TransportNationalityDropEdit));
		FormattedDateTimeDateEdit = RegisterControl(nameof(ReportsGridFieldsControl.FormattedDateTimeDateEdit));
		DiscrepanciesCheckBox = RegisterControl(nameof(ReportsGridFieldsControl.DiscrepanciesCheckBox));
		TransportModeDropEdit = RegisterControl(nameof(ReportsGridFieldsControl.TransportModeDropEdit));
		LocationCodeFindBox = RegisterControl(nameof(ReportsGridFieldsControl.LocationCodeFindBox));
		LocationOfGoodsUserControl = RegisterControl(nameof(ReportsGridFieldsControl.LocationOfGoodsUserControl));
	}

	public ControlReference ConsignmentGuidDropEdit { get; }
	public ControlReference OfficeOfExitCodeFindBox { get; }
	public ControlReference TransportTypeDropEdit { get; }
	public ControlReference TransportIDTextBox { get; }
	public ControlReference TransportNationalityDropEdit { get; }
	public ControlReference FormattedDateTimeDateEdit { get; }
	public ControlReference DiscrepanciesCheckBox { get; }
	public ControlReference TransportModeDropEdit { get; }
	public ControlReference LocationCodeFindBox { get; }
	public ControlReference LocationOfGoodsUserControl { get; }
}
