using Enterprise.Customs.BE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI;

public partial class ExportMessageSendingForm : MessageSendingForm<ExportDeclarationMessageSendingActionParent>
{
	public ExportMessageSendingForm(ExportDeclarationMessageSendingActionParent parent) : base(parent)
	{
	}

	protected override void InitialiseForm()
	{
		base.InitialiseForm();
		InitializeComponent();
	}

	protected override ZUserControl GetBottomSectionUserControl()
	{
		bottomSectionUserControl = new ExportBottomSectionUserControl();
		return bottomSectionUserControl;
	}
	ExportBottomSectionUserControl bottomSectionUserControl;
}
