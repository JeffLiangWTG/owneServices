using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public sealed class AdditionalInformationFieldsControlBag : ControlBag
{
	public static AdditionalInformationFieldsControlBag Instance => instance ?? (instance = new AdditionalInformationFieldsControlBag());

	[ThreadStatic]
	static AdditionalInformationFieldsControlBag instance;

	AdditionalInformationFieldsControlBag()
	{
		CodeDropEdit = RegisterControl(nameof(AdditionalInformationFieldsUserControl.CodeDropEdit));
		ReferenceNumberDropEdit = RegisterControl(nameof(AdditionalInformationFieldsUserControl.ReferenceNumberDropEdit));
		DescriptionTextBox = RegisterControl(nameof(AdditionalInformationFieldsUserControl.DescriptionTextBox));
	}

	protected override Control CreateTemplate() => new AdditionalInformationFieldsUserControl();

	public ControlReference CodeDropEdit { get; }
	public ControlReference ReferenceNumberDropEdit { get; }
	public ControlReference DescriptionTextBox { get; }
}
