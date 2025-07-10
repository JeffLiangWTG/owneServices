using System;
using CargoWise.Common;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public partial class LayoutSupportingDocumentsFieldsControl : EU.GUI.PlugIn.LayoutSupportingDocumentsFieldsControl
{
	public LayoutSupportingDocumentsFieldsControl(JobDeclaration declaration) : base(declaration)
	{
		Declaration = Argument.NotNull(declaration, nameof(declaration));
		InitializeComponent();
	}

	[Obsolete("Use the constructor that takes the business object, this constructor is just for the designer")]
	public LayoutSupportingDocumentsFieldsControl()
	{
		InitializeComponent();
	}

	JobDeclaration Declaration { get; }

	protected override IPanelLayoutProvider GetLayout()
	{
		return Declaration.IsUCC6
		? new Ucc6SupportingDocumentFieldsLayout()
		: new NonUcc6SupportingDocumentFieldsLayout();
	}
}
