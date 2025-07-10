using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public sealed class PreviousDocumentsFieldsControlBag : ControlBag
{
	public PreviousDocumentsFieldsControlBag()
	{
		ItemNumberCalcEdit = RegisterControl(nameof(PreviousDocumentsFieldsUserControl.ItemNumberCalcEdit));
	}

	public static PreviousDocumentsFieldsControlBag Instance => instance ?? (instance = new PreviousDocumentsFieldsControlBag());

	[ThreadStatic]
	static PreviousDocumentsFieldsControlBag instance;

	protected override Control CreateTemplate() => new PreviousDocumentsFieldsUserControl();

	public ControlReference ItemNumberCalcEdit { get; }
}
