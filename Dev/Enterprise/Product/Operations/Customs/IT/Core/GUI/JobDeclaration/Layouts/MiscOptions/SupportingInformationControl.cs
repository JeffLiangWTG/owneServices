using System;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.GUI;

public partial class SupportingInformationControl : EU.GUI.SupportingInformationControl
{
	public SupportingInformationControl()
	{
		InitializeComponent();
	}

	protected override Type GetSupportingDocumentsUserControlType() => typeof(DeclarationLayoutSupportingDocumentsUserControl);

	protected override Type GetPreviousDocumentsUserControlType()
	{
		if (IsImport)
		{
			return typeof(LayoutPreviousDocumentsUserControl);
		}
		return typeof(PreviousDocumentsUserControl);
	}

	bool IsImport => CurrentDataItem is JobDeclaration jobDeclaration && jobDeclaration.IsImport;
}

