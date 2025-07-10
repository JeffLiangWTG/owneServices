using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

sealed class InvoiceDetailsControlBag : ControlBag
{
	protected override Control CreateTemplate() => new InvoiceDetailsUserControl();

	[ThreadStatic]
	static InvoiceDetailsControlBag instance;
	public static InvoiceDetailsControlBag Instance => instance ?? (instance = new InvoiceDetailsControlBag());

	InvoiceDetailsControlBag()
	{
		AgreedPlaceCodeDropEdit = RegisterControl(nameof(InvoiceDetailsUserControl.AgreedPlaceCodeDropEdit));
	}

	public ControlReference AgreedPlaceCodeDropEdit { get; }
}
