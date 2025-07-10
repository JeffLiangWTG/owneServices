using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI;

public sealed class MiscOptionsControlBag : ControlBag
{
	public static MiscOptionsControlBag Instance => instance ?? (instance = new MiscOptionsControlBag());

	[ThreadStatic]
	static MiscOptionsControlBag instance;

	protected override Control CreateTemplate() => new MiscOptionsFieldsUserControl();

	MiscOptionsControlBag()
	{
		AuthPerDeclarationCheckBox = RegisterControl(nameof(MiscOptionsFieldsUserControl.AuthPerDeclarationCheckBox));
		CertificateDropEdit = RegisterControl(nameof(MiscOptionsFieldsUserControl.CertificateDropEdit));
		DeclEmailAddrTextBox = RegisterControl(nameof(MiscOptionsFieldsUserControl.DeclEmailAddrTextBox));
		OtherEmailAddrTextBox = RegisterControl(nameof(MiscOptionsFieldsUserControl.OtherEmailAddrTextBox));
		SupportingInformationUserControl = RegisterControl(nameof(MiscOptionsFieldsUserControl.SupportingInformationUserControl));
		DontSendImporterIdCheckBox = RegisterControl(nameof(MiscOptionsFieldsUserControl.DontSendImporterIdCheckBox));
	}

	public ControlReference AuthPerDeclarationCheckBox { get; }

	public ControlReference CertificateDropEdit { get; }

	public ControlReference DeclEmailAddrTextBox { get; }

	public ControlReference OtherEmailAddrTextBox { get; }

	public ControlReference SupportingInformationUserControl { get; }

	public ControlReference DontSendImporterIdCheckBox { get; }
}
