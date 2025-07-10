using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI;

public sealed class InvoiceLineDetailsControlBag : ControlBag
{
	InvoiceLineDetailsControlBag()
	{
		UCRReferenceTextBox = RegisterControl(nameof(InvoiceLineDetails.UCRReferenceTextBox));
	}

	public static InvoiceLineDetailsControlBag Instance => instance ?? (instance = new InvoiceLineDetailsControlBag());

	[ThreadStatic]
	static InvoiceLineDetailsControlBag instance;

	protected override Control CreateTemplate() => new InvoiceLineDetails();

	public ControlReference UCRReferenceTextBox { get; }
}
