using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public sealed class SupportingDocumentDetailsControlBag : ControlBag
{
	public static SupportingDocumentDetailsControlBag Instance => instance ??= new SupportingDocumentDetailsControlBag();

	[ThreadStatic]
	static SupportingDocumentDetailsControlBag instance;

	public SupportingDocumentDetailsControlBag()
	{
		ImageReferenceNumberTextBox = RegisterControl(nameof(SupportingDocumentDetailsUserControl.ImageReferenceNumberTextBox));
		DocumentTypeCodeFindBox = RegisterControl(nameof(SupportingDocumentDetailsUserControl.DocumentTypeCodeFindBox));
		IssuingPartyGroupBox = RegisterControl(nameof(SupportingDocumentDetailsUserControl.IssuingPartyGroupBox));
	}

	protected override Control CreateTemplate() => new SupportingDocumentDetailsUserControl();

	public ControlReference ImageReferenceNumberTextBox { get; }
	public ControlReference DocumentTypeCodeFindBox { get; }
	public ControlReference IssuingPartyGroupBox { get; }
}

