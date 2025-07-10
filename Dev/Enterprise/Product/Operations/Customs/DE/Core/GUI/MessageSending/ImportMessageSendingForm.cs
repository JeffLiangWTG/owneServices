using System.Linq;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public partial class ImportMessageSendingForm : MessageSendingForm<ImportDeclarationMessageSendingActionParent>
	{
		public ImportMessageSendingForm(ImportDeclarationMessageSendingActionParent parent) : base(parent, Res.GetString("3042d597-3382-4ef6-b1af-80616195cc72", "Import"))
		{
			OnRegistrationNumberSetChangeSendAvailability(parent);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override void ChangeSendButtonAvailability()
		{
			base.ChangeSendButtonAvailability();

			var importAction = (ImportDeclarationMessageSendingActionParent)MessageSendingObjectParent;
			var canSendSelectedActions = importAction.SelectedSendingObjects.Cast<ImportEntryMessageSendingAction>()
				.All(action => action.CanSend);

			var sendButton = GetEffectiveSendButton();
			sendButton.Enabled = sendButton.Enabled && canSendSelectedActions;
		}

		protected override ZUserControl GetBottomSectionUserControl()
		{
			var importBottomSectionUserControl = new ImportBottomSectionUserControl();
			importBottomSectionUserControl.Name = "ImportBottomSectionUserControl";
			BindingSource.SetBindingMember(importBottomSectionUserControl, nameof(ImportDeclarationMessageSendingActionParent.SendingObjectsCollection));
			return importBottomSectionUserControl;
		}

		void OnRegistrationNumberSetChangeSendAvailability(ImportDeclarationMessageSendingActionParent parent)
		{
			foreach (var importAction in parent.SendingObjectsCollection.Cast<ImportEntryMessageSendingAction>())
			{
				importAction.RegistrationNumberInfo.ValueChanged += (sender, args) => ChangeSendButtonAvailability();
			}
		}
	}
}
