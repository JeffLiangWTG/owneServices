using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public sealed class DeclarationOtherDetailsControlBag : ControlBag
{
	public static DeclarationOtherDetailsControlBag Instance => instance ??= new DeclarationOtherDetailsControlBag();

	[ThreadStatic]
	static DeclarationOtherDetailsControlBag instance;

	DeclarationOtherDetailsControlBag()
	{
		OriginStateDropEdit = RegisterControl(nameof(DeclarationOtherDetailsUserControl.OriginStateDropEdit));
		ExporterClassTextBox = RegisterControl(nameof(DeclarationOtherDetailsUserControl.ExporterClassTextBox));
		IECCodeTextBox = RegisterControl(nameof(DeclarationOtherDetailsUserControl.IECCodeTextBox));
		EPZCodeDropEdit = RegisterControl(nameof(DeclarationOtherDetailsUserControl.EPZCodeDropEdit));
		BranchSerialNumberTextBox = RegisterControl(nameof(DeclarationOtherDetailsUserControl.BranchSerialNumberTextBox));
		AuthorizedDealerCodeTextBox = RegisterControl(nameof(DeclarationOtherDetailsUserControl.AuthorizedDealerCodeTextBox));
		TypeOfExporterDropEdit = RegisterControl(nameof(DeclarationOtherDetailsUserControl.TypeOfExporterDropEdit));
		SealByDropEdit = RegisterControl(nameof(DeclarationOtherDetailsUserControl.SealByDropEdit));
		RotationNumberTextBox = RegisterControl(nameof(DeclarationOtherDetailsUserControl.RotationNumberTextBox));
		RotationDateDateEdit = RegisterControl(nameof(DeclarationOtherDetailsUserControl.RotationDateDateEdit));
		StuffingAtDropEdit = RegisterControl(nameof(DeclarationOtherDetailsUserControl.StuffingAtDropEdit));
		SampleAccompaniedDropEdit = RegisterControl(nameof(DeclarationOtherDetailsUserControl.SampleAccompaniedDropEdit));
		GoodsRegistrationSeparatorUserControl = RegisterControl(nameof(DeclarationOtherDetailsUserControl.GoodsRegistrationSeparatorUserControl));
		TranshipperGuidFindBox = RegisterControl(nameof(DeclarationOtherDetailsUserControl.TranshipperGuidFindBox));
	}

	protected override Control CreateTemplate() => new DeclarationOtherDetailsUserControl();

	public ControlReference OriginStateDropEdit { get; }
	public ControlReference ExporterClassTextBox { get; }
	public ControlReference IECCodeTextBox { get; }
	public ControlReference EPZCodeDropEdit { get; }
	public ControlReference BranchSerialNumberTextBox { get; }
	public ControlReference AuthorizedDealerCodeTextBox { get; }
	public ControlReference TypeOfExporterDropEdit { get; }
	public ControlReference SealByDropEdit { get; }
	public ControlReference RotationNumberTextBox { get; }
	public ControlReference RotationDateDateEdit { get; }
	public ControlReference StuffingAtDropEdit { get; }
	public ControlReference SampleAccompaniedDropEdit { get; }
	public ControlReference GoodsRegistrationSeparatorUserControl { get; }
	public ControlReference TranshipperGuidFindBox { get; }
}
