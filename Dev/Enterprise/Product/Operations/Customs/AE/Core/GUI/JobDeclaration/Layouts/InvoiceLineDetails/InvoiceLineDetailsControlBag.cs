using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.GUI;

public sealed class InvoiceLineDetailsControlBag : ControlBag
{
	InvoiceLineDetailsControlBag()
	{
		GoodsConditionDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.GoodsConditionDropEdit));
	}
	public static InvoiceLineDetailsControlBag Instance => instance ??= new InvoiceLineDetailsControlBag();

	[ThreadStatic]
	static InvoiceLineDetailsControlBag instance;

	protected override Control CreateTemplate() => new InvoiceLineDetailsUserControl();

	public ControlReference GoodsConditionDropEdit { get; }
}
