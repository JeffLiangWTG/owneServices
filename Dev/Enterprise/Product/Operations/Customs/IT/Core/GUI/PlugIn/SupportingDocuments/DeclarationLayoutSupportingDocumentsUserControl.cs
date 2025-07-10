using System.Collections.Generic;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.GUI;

public partial class DeclarationLayoutSupportingDocumentsUserControl : EU.GUI.PlugIn.DeclarationLayoutSupportingDocumentsUserControl
{
	public DeclarationLayoutSupportingDocumentsUserControl()
	{
		InitializeComponent();
		InitializeGridLayout();
	}

	public new JobDeclaration JobDeclaration
	{
		get => (JobDeclaration)base.JobDeclaration;
		set => base.JobDeclaration = value;
	}

	protected override EU.GUI.PlugIn.LayoutSupportingDocumentsFieldsControl GetSupportingDocumentsFieldsControl(EU.Business.Declaration.JobDeclaration declaration)
		=> new LayoutSupportingDocumentsFieldsControl(JobDeclaration);

	protected override void InitializeGridLayoutCore()
	{
		base.InitializeGridLayoutCore();
		new SupportingDocumentsGridInitializer(SupportingDocumentsGrid).Initialize();
	}

	protected override IReadOnlyList<string> AvailableColumnNames => new SupportingDocumentsGridColumnStylesHelper().GetColumnStyles(JobDeclaration);
}
