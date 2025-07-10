using CargoWise.Common;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI
{
	[SuppressBindingMemberBashingTest] // to suppress PreviewMessageCheckBox & SendWithValidationErrorsCheckBox
	public partial class EMCSMessageSendingForm<TSendingAction> : MessageSendingFormWithValidationDetails where TSendingAction : EMCSMessageSendingAction
	{
		public EMCSMessageSendingForm(EMCSMessageSendingActionParent<TSendingAction> parent) : base(parent)
		{
			MessageSendingActionParent = Argument.NotNull(parent, nameof(parent));
			InitializeComponent();
		}
		EMCSMessageSendingActionParent<TSendingAction> MessageSendingActionParent { get; }

		protected override bool PreviewMessageCheckboxVisible => true;

		protected override bool CheckIsOKToSend()
		{
			return Configuration.IsOKToSend(MessageSendingActionParent) && base.CheckIsOKToSend();
		}

		IEMCSMessageSendingFormConfiguration Configuration => configuration ??= EMCSMessageSendingFormConfiguration.GetConfiguration(MessageSendingActionParent.JobDeclaration.GetDefaultDataGroupingCode());
		IEMCSMessageSendingFormConfiguration configuration;
	}
}
