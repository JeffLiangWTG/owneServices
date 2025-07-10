using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.Manifest.GUI;

public class CGMBillControlBag : ControlBag
{
	CGMBillControlBag()
	{
		BondDetailsUserControl = RegisterControl(nameof(CGMBillUserControl.BondDetailsUserControl));
		FinalDestinationDetailsUserControl = RegisterControl(nameof(CGMBillUserControl.FinalDestinationDetailsUserControl));
		TransshipmentDetailsUserControl = RegisterControl(nameof(CGMBillUserControl.TransshipmentDetailsUserControl));
	}

	public static CGMBillControlBag Instance => instance ??= new CGMBillControlBag();

	[ThreadStatic]
	static CGMBillControlBag instance;

	protected override Control CreateTemplate() => new CGMBillUserControl();

	public ControlReference BondDetailsUserControl { get; }
	public ControlReference FinalDestinationDetailsUserControl { get; }
	public ControlReference TransshipmentDetailsUserControl { get; }
}
