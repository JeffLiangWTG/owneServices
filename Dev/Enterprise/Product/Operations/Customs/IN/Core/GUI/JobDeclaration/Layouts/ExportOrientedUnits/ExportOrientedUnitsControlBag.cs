using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public sealed class ExportOrientedUnitsControlBag : ControlBag
{
	public static ExportOrientedUnitsControlBag Instance => instance ?? (instance = new ExportOrientedUnitsControlBag());

	[ThreadStatic]
	static ExportOrientedUnitsControlBag instance;

	ExportOrientedUnitsControlBag()
	{
		ExportOrientedUnitsDocAddressControl = RegisterControl(nameof(ExportOrientedUnitsUserControl.ExportOrientedUnitsDocAddressControl));
		ExaminationDateEdit = RegisterControl(nameof(ExportOrientedUnitsUserControl.ExaminationDateEdit));
		ExaminingOfficerNameTextBox = RegisterControl(nameof(ExportOrientedUnitsUserControl.ExaminingOfficerNameTextBox));
		ExaminingOfficerDesignationTextBox = RegisterControl(nameof(ExportOrientedUnitsUserControl.ExaminingOfficerDesignationTextBox));
		SupervisingOfficerNameTextBox = RegisterControl(nameof(ExportOrientedUnitsUserControl.SupervisingOfficerNameTextBox));
		SupervisingOfficerDesignationTextBox = RegisterControl(nameof(ExportOrientedUnitsUserControl.SupervisingOfficerDesignationTextBox));
		CommissionerateTextBox = RegisterControl(nameof(ExportOrientedUnitsUserControl.CommissionerateTextBox));
		DivisionTextBox = RegisterControl(nameof(ExportOrientedUnitsUserControl.DivisionTextBox));
		RangeTextBox = RegisterControl(nameof(ExportOrientedUnitsUserControl.RangeTextBox));
		SealNoTextBox = RegisterControl(nameof(ExportOrientedUnitsUserControl.SealNoTextBox));
		VerifiedDropEdit = RegisterControl(nameof(ExportOrientedUnitsUserControl.VerifiedDropEdit));
		SampleForwardedDropEdit = RegisterControl(nameof(ExportOrientedUnitsUserControl.SampleForwardedDropEdit));
	}

	protected override Control CreateTemplate() => new ExportOrientedUnitsUserControl();

	public ControlReference ExportOrientedUnitsDocAddressControl { get; }
	public ControlReference ExaminationDateEdit;
	public ControlReference ExaminingOfficerNameTextBox;
	public ControlReference ExaminingOfficerDesignationTextBox;
	public ControlReference SupervisingOfficerNameTextBox;
	public ControlReference SupervisingOfficerDesignationTextBox;
	public ControlReference CommissionerateTextBox { get; }
	public ControlReference DivisionTextBox { get; }
	public ControlReference RangeTextBox { get; }
	public ControlReference SealNoTextBox { get; }
	public ControlReference VerifiedDropEdit { get; }
	public ControlReference SampleForwardedDropEdit { get; }
}
