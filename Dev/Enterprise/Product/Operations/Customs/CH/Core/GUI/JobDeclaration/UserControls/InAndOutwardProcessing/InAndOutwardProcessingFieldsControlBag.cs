using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public sealed class InAndOutwardProcessingFieldsControlBag : ControlBag
{
	public static InAndOutwardProcessingFieldsControlBag Instance => instance ??= new InAndOutwardProcessingFieldsControlBag();

	[ThreadStatic]
	static InAndOutwardProcessingFieldsControlBag instance;

	InAndOutwardProcessingFieldsControlBag()
	{
		SubTypeDropEdit = RegisterControl(nameof(InAndOutwardProcessingFieldsUserControl.SubTypeDropEdit));
		CodeDropEdit = RegisterControl(nameof(InAndOutwardProcessingFieldsUserControl.CodeDropEdit));
		ProcedureDropEdit = RegisterControl(nameof(InAndOutwardProcessingFieldsUserControl.ProcedureDropEdit));
		IssuerTypeDropEdit = RegisterControl(nameof(InAndOutwardProcessingFieldsUserControl.IssuerTypeDropEdit));
		DescriptionTextBox = RegisterControl(nameof(InAndOutwardProcessingFieldsUserControl.DescriptionTextBox));
		StatusCheckBox = RegisterControl(nameof(InAndOutwardProcessingFieldsUserControl.StatusCheckBox));
		CustomsOfficeCodeFindBox = RegisterControl(nameof(InAndOutwardProcessingFieldsUserControl.CustomsOfficeCodeFindBox));
	}

	protected override Control CreateTemplate() => new InAndOutwardProcessingFieldsUserControl();

	public ControlReference SubTypeDropEdit { get; }
	public ControlReference CodeDropEdit { get; }
	public ControlReference ProcedureDropEdit { get; }
	public ControlReference IssuerTypeDropEdit { get; }
	public ControlReference DescriptionTextBox { get; }
	public ControlReference StatusCheckBox { get; }
	public ControlReference CustomsOfficeCodeFindBox { get; }
}
