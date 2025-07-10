using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.NCTS.GUI;

public class PreviousDocumentControlBag : ControlBag
{
	PreviousDocumentControlBag()
	{
		ReferenceNumberN785UserControl = RegisterControl(nameof(PreviousDocumentUserControl.ReferenceNumberN785UserControl));
	}

	public static PreviousDocumentControlBag Instance => instance ?? (instance = new PreviousDocumentControlBag());

	[ThreadStatic]
	static PreviousDocumentControlBag instance;

	protected override Control CreateTemplate() => new PreviousDocumentUserControl();

	public ControlReference ReferenceNumberN785UserControl { get; }
}
