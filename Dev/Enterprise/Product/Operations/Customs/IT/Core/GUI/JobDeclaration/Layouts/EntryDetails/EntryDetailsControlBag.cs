using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

sealed class EntryDetailsControlBag : ControlBag
{
	protected override Control CreateTemplate() => new EntryDetailsUserControl();

	public static EntryDetailsControlBag Instance => instance ?? (instance = new EntryDetailsControlBag());

	[ThreadStatic]
	static EntryDetailsControlBag instance;

	EntryDetailsControlBag()
	{
		EntryTypeTextBox = RegisterControl(nameof(EntryDetailsUserControl.EntryTypeTextBox));
		ReferenceNumberTextBox = RegisterControl(nameof(EntryDetailsUserControl.ReferenceNumberTextBox));
		IssueDateDateEdit = RegisterControl(nameof(EntryDetailsUserControl.IssueDateDateEdit));
		IncotermTextBox = RegisterControl(nameof(EntryDetailsUserControl.IncotermTextBox));
		GrossWeightUserControl = RegisterControl(nameof(EntryDetailsUserControl.GrossWeightUserControl));
		NetWeightUserControl = RegisterControl(nameof(EntryDetailsUserControl.NetWeightUserControl));
		CustomsQuantityUserControl = RegisterControl(nameof(EntryDetailsUserControl.CustomsQuantityUserControl));
		InvoiceAmountUserControl = RegisterControl(nameof(EntryDetailsUserControl.InvoiceAmountUserControl));
		FreightAdjustmentCalcEdit = RegisterControl(nameof(EntryDetailsUserControl.FreightAdjustmentCalcEdit));
		MessageStatusUserControl = RegisterControl(nameof(EntryDetailsUserControl.MessageStatusUserControl));
		ControlChannelDropEdit = RegisterControl(nameof(EntryDetailsUserControl.ControlChannelDropEdit));
		MessageTypeDropEdit = RegisterControl(nameof(EntryDetailsUserControl.MessageTypeDropEdit));
		WarehouseStatusDropEdit = RegisterControl(nameof(EntryDetailsUserControl.WarehouseStatusDropEdit));
		RegistrationNumberTextBox = RegisterControl(nameof(EntryDetailsUserControl.RegistrationNumberTextBox));
		CustomsOfficeTextBox = RegisterControl(nameof(EntryDetailsUserControl.CustomsOfficeTextBox));
		ReleaseCodeTextBox = RegisterControl(nameof(EntryDetailsUserControl.ReleaseCodeTextBox));
		A93Grid = RegisterControl(nameof(EntryDetailsUserControl.A93Grid));
		ExitDateDateEdit = RegisterControl(nameof(EntryDetailsUserControl.ExitDateDateEdit));
		ExitOfficeUserControl = RegisterControl(nameof(EntryDetailsUserControl.ExitOfficeUserControl));
		ExitStatusUserControl = RegisterControl(nameof(EntryDetailsUserControl.ExitStatusUserControl));
		ReferenceLabel = RegisterControl(nameof(EntryDetailsUserControl.ReferenceLabel));
		TotalsLabel = RegisterControl(nameof(EntryDetailsUserControl.TotalsLabel));
		StatusLabel = RegisterControl(nameof(EntryDetailsUserControl.StatusLabel));
		CustomsLabel = RegisterControl(nameof(EntryDetailsUserControl.CustomsLabel));
		A93Label = RegisterControl(nameof(EntryDetailsUserControl.A93Label));
		ExitLabel = RegisterControl(nameof(EntryDetailsUserControl.ExitLabel));
		SubmittedDateDateEdit = RegisterControl(nameof(EntryDetailsUserControl.SubmittedDateDateEdit));
		MRNTextBox = RegisterControl(nameof(EntryDetailsUserControl.MRNTextBox));
		ReleaseDateDateEdit = RegisterControl(nameof(EntryDetailsUserControl.ReleaseDateDateEdit));
		EntryStatusDropEdit = RegisterControl(nameof(EntryDetailsUserControl.EntryStatusDropEdit));
	}

	public ControlReference EntryTypeTextBox { get; }

	public ControlReference ReferenceNumberTextBox { get; }

	public ControlReference IssueDateDateEdit { get; }

	public ControlReference IncotermTextBox { get; }

	public ControlReference GrossWeightUserControl { get; }

	public ControlReference NetWeightUserControl { get; }

	public ControlReference CustomsQuantityUserControl { get; }

	public ControlReference InvoiceAmountUserControl { get; }

	public ControlReference FreightAdjustmentCalcEdit { get; }

	public ControlReference MessageStatusUserControl { get; }

	public ControlReference ControlChannelDropEdit { get; }

	public ControlReference MessageTypeDropEdit { get; }

	public ControlReference WarehouseStatusDropEdit { get; }

	public ControlReference RegistrationNumberTextBox { get; }

	public ControlReference CustomsOfficeTextBox { get; }

	public ControlReference ReleaseCodeTextBox { get; }

	public ControlReference A93Grid { get; }

	public ControlReference ExitDateDateEdit { get; }

	public ControlReference ExitOfficeUserControl { get; }

	public ControlReference ExitStatusUserControl { get; }

	public ControlReference ReferenceLabel { get; }

	public ControlReference TotalsLabel { get; }

	public ControlReference StatusLabel { get; }

	public ControlReference CustomsLabel { get; }

	public ControlReference A93Label { get; }

	public ControlReference ExitLabel { get; }

	public ControlReference SubmittedDateDateEdit { get; }

	public ControlReference MRNTextBox { get; }

	public ControlReference ReleaseDateDateEdit { get; }

	public ControlReference EntryStatusDropEdit { get; }
}
