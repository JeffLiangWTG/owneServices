using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI;

public sealed class ArrivalDeclarationDetailsControlBag : ControlBag
{
	ArrivalDeclarationDetailsControlBag()
	{
		CircuitTextBox = RegisterControl(nameof(ArrivalDeclarationDetailsUserControl.CircuitTextBox));
		ArrivalSummaryDeclarationUserControl = RegisterControl(nameof(ArrivalDeclarationDetailsUserControl.ArrivalSummaryDeclarationUserControl));
	}

	public static ArrivalDeclarationDetailsControlBag Instance => instance ??= new ArrivalDeclarationDetailsControlBag();

	[ThreadStatic]
	static ArrivalDeclarationDetailsControlBag instance;

	public ControlReference CircuitTextBox { get; }

	public ControlReference ArrivalSummaryDeclarationUserControl { get; }

	protected override Control CreateTemplate() => new ArrivalDeclarationDetailsUserControl();
}
