#if NET
using System.Windows.Forms;
#endif
using Enterprise.ZArchitecture.GUI;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI;

public sealed class LinesDetailsControlBag : ControlBag
{
	protected override Control CreateTemplate() => new LinesDetailsUserControl();

	public static LinesDetailsControlBag Instance => instance ??= new();

	[ThreadSafe]
	static LinesDetailsControlBag instance;

	LinesDetailsControlBag()
	{
		UnionStatusDropEdit = RegisterControl(nameof(LinesDetailsUserControl.UnionStatusDropEdit));
		PackagesRemainingCalcEdit = RegisterControl(nameof(LinesDetailsUserControl.PackagesRemainingCalcEdit));
		LineNumberCalcEdit = RegisterControl(nameof(LinesDetailsUserControl.LineNumberCalcEdit));
		CustomsStatusDropEdit = RegisterControl(nameof(LinesDetailsUserControl.CustomsStatusDropEdit));
		PackageTypeDropEdit = RegisterControl(nameof(LinesDetailsUserControl.PackageTypeDropEdit));
		OwnerReferenceTypeDropEdit = RegisterControl(nameof(LinesDetailsUserControl.OwnerReferenceTypeDropEdit));
		LimitDateEdit = RegisterControl(nameof(LinesDetailsUserControl.LimitDateEdit));
		CustodianEORIBranchUserControl = RegisterControl(nameof(LinesDetailsUserControl.CustodianEORIBranchUserControl));
		DisposalEntitledTraderEORIBranchUserControl = RegisterControl(nameof(LinesDetailsUserControl.DisposalEntitledTraderEORIBranchUserControl));
		GoodsDescriptionTextBox = RegisterControl(nameof(LinesDetailsUserControl.GoodsDescriptionTextBox));
		LocationofGoodsTextBox = RegisterControl(nameof(LinesDetailsUserControl.LocationofGoodsTextBox));
		OwnerReferenceNumberTextBox = RegisterControl(nameof(LinesDetailsUserControl.OwnerReferenceNumberTextBox));
	}

	public ControlReference UnionStatusDropEdit { get; }
	public ControlReference PackagesRemainingCalcEdit { get; }
	public ControlReference LineNumberCalcEdit { get; }
	public ControlReference CustomsStatusDropEdit { get; }
	public ControlReference PackageTypeDropEdit { get; }
	public ControlReference OwnerReferenceTypeDropEdit { get; }
	public ControlReference LimitDateEdit { get; }
	public ControlReference CustodianEORIBranchUserControl { get; }
	public ControlReference DisposalEntitledTraderEORIBranchUserControl { get; }
	public ControlReference GoodsDescriptionTextBox { get; }
	public ControlReference LocationofGoodsTextBox { get; }
	public ControlReference OwnerReferenceNumberTextBox { get; }
}
