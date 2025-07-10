using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

sealed class SupportingDocumentControlBag : ControlBag
{
	public SupportingDocumentControlBag()
	{
		YearOfIssueTextBox = RegisterControl(nameof(SupportingDocumentUserControl.YearOfIssueTextBox));
	}

	public static SupportingDocumentControlBag Instance => instance ?? (instance = new SupportingDocumentControlBag());

	[ThreadStatic]
	static SupportingDocumentControlBag instance;

	public ControlReference YearOfIssueTextBox { get; }

	protected override Control CreateTemplate() => new SupportingDocumentUserControl();
}
