using Enterprise.Customs.BE.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.BE.GUI;

[CodeAlive("Will be used in EDIMenu, in WI00733453 - WF3 (Enable to show form)")]
public partial class ImportMessageSendingForm : MessageSendingForm<ImportDeclarationMessageSendingActionParent>
{
	public ImportMessageSendingForm(ImportDeclarationMessageSendingActionParent parent) : base(parent)
	{
	}

	protected override void InitialiseForm()
	{
		base.InitialiseForm();
		InitializeComponent();
	}
}
