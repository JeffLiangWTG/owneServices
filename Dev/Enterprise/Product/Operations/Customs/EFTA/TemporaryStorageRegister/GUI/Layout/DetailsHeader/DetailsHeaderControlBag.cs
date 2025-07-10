#if NET
using System.Windows.Forms;
#endif
using Enterprise.ZArchitecture.GUI;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI;

public sealed class DetailsHeaderControlBag : ControlBag
{
	protected override Control CreateTemplate() => new DetailsHeaderUserControl();

	public static DetailsHeaderControlBag Instance => instance ??= new ();

	[ThreadSafe]
	static DetailsHeaderControlBag instance;

	DetailsHeaderControlBag()
	{
		StatusDropEdit = RegisterControl(nameof(DetailsHeaderUserControl.StatusDropEdit));
		CustomsOfficeCodeFindBox = RegisterControl(nameof(DetailsHeaderUserControl.CustomsOfficeCodeFindBox));
		PreviousReferenceTypeDropEdit = RegisterControl(nameof(DetailsHeaderUserControl.PreviousReferenceTypeDropEdit));
		PresentationDateEdit = RegisterControl(nameof(DetailsHeaderUserControl.PresentationDateEdit));
		ArrvialDateEdit = RegisterControl(nameof(DetailsHeaderUserControl.ArrvialDateEdit));
		ATBNumberTextBox = RegisterControl(nameof(DetailsHeaderUserControl.ATBNumberTextBox));
		PreviousReferenceNumberTextBox = RegisterControl(nameof(DetailsHeaderUserControl.PreviousReferenceNumberTextBox));
		CustomerReferenceTextBox = RegisterControl(nameof(DetailsHeaderUserControl.CustomerReferenceTextBox));
	}

	public ControlReference StatusDropEdit { get; }
	public ControlReference CustomsOfficeCodeFindBox { get; }
	public ControlReference PreviousReferenceTypeDropEdit { get; }
	public ControlReference PresentationDateEdit { get; }
	public ControlReference ArrvialDateEdit { get; }
	public ControlReference ATBNumberTextBox { get; }
	public ControlReference PreviousReferenceNumberTextBox { get; }
	public ControlReference CustomerReferenceTextBox { get; }
}
