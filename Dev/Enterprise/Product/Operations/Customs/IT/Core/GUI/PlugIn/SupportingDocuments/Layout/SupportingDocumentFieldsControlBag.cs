using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public sealed class SupportingDocumentFieldsControlBag : ControlBag
{
	protected override Control CreateTemplate() => new SupportingDocumentFieldsControl();

	[ThreadStatic]
	static SupportingDocumentFieldsControlBag instance;

	public static SupportingDocumentFieldsControlBag Instance => instance ?? (instance = new SupportingDocumentFieldsControlBag());

	SupportingDocumentFieldsControlBag()
	{
		YearOfIssueTextBox = RegisterControl(nameof(SupportingDocumentFieldsControl.YearOfIssueTextBox));
		IssuingAuthorityTextBox = RegisterControl(nameof(SupportingDocumentFieldsControl.IssuingAuthorityTextBox));
		CountryCodeCodeFindBox = RegisterControl(nameof(SupportingDocumentFieldsControl.CountryCodeCodeFindBox));
		AvailabilityDropEdit = RegisterControl(nameof(SupportingDocumentFieldsControl.AvailabilityDropEdit));
		LineNoCalcEdit = RegisterControl(nameof(SupportingDocumentFieldsControl.LineNoCalcEdit));
		ValueCalcFindBox = RegisterControl(nameof(SupportingDocumentFieldsControl.ValueCalcFindBox));
	}

	public ControlReference YearOfIssueTextBox { get; }
	public ControlReference IssuingAuthorityTextBox { get; }
	public ControlReference CountryCodeCodeFindBox { get; }
	public ControlReference AvailabilityDropEdit { get; }
	public ControlReference LineNoCalcEdit { get; }
	public ControlReference ValueCalcFindBox { get; }
}
