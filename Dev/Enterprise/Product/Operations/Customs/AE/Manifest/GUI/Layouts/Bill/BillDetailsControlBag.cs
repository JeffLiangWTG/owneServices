using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.Manifest.GUI;

public sealed class BillDetailsControlBag : ControlBag
{
	BillDetailsControlBag()
	{
		SplitBillCheckBox = RegisterControl(nameof(BillDetailsControlBag.SplitBillCheckBox));
		SplitBillNumberCodeFindBox = RegisterControl(nameof(BillDetailsControlBag.SplitBillNumberCodeFindBox));
		ForwarderMPCITextBox = RegisterControl(nameof(BillDetailsControlBag.ForwarderMPCITextBox));
	}

	public static BillDetailsControlBag Instance => instance ??= new BillDetailsControlBag();

	[ThreadStatic]
	static BillDetailsControlBag instance;

	protected override Control CreateTemplate() => new BillDetailsUserControl();

	public ControlReference SplitBillCheckBox { get; }
	public ControlReference SplitBillNumberCodeFindBox { get; }
	public ControlReference ForwarderMPCITextBox { get; }
}
