using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.GUI
{
	public partial class JobDeclarationForm : BaseJobDeclarationForm
	{
		public JobDeclarationForm()
			: base()
		{
		}

		public JobDeclarationForm(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitializeComponent();
			base.InitialiseForm();
			AddNewBottomPanel();
		}

		void AddNewBottomPanel()
		{
			if (this.DataSource is BaseJobDeclaration jobDeclaration && jobDeclaration.EnableAttachCommercialInvoice)
			{
				InitializeBottomPanel();
				AttachTotalValuesPanelToBottom();
			}
		}

		void InitializeBottomPanel()
		{
			PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel = new ZPanel
			{
				Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(311, 1, true),
				Name = "bottomPanel",
				Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 30, true),
				TabIndex = 10,
				Visible = false
			};

			invoiceLineBottomPanelUserControl = new InvoiceLineBottomPanelUserControl();
			PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Controls.Add(invoiceLineBottomPanelUserControl);
		}

		void AttachTotalValuesPanelToBottom()
		{
			BottomButtonPanel.Controls.Add(PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel);
		}

		protected override void AddPlugins()
		{
			base.AddPlugins();
			PlugIns.Add(ControllerIDs.DocumentVisualizer);
			PlugIns.Add(ControllerIDs.Customs.EU.ExitSummaryController);
		}

		protected override IEDIMenu GetNewTopLevelMenuCore() => new EDIMenu();

		protected override BaseCustomsBrokerageUserControl GetBrokerageUserControl() => new CustomsBrokerageUserControl();

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = base.ShowPreSaveDialogs();
			if (result == ContinueWithSave.Yes)
			{
				result = Declaration.CanSaveBasedOnAwaitingMessageAndAnyChangesThatWouldAffectMessages();
				if (result == ContinueWithSave.No)
				{
					Globals.Message.ShowError(
						Res.GetString("d2959bf3-3d86-4010-a77b-171d6330725b", @"The changes cannot be saved. The changes you made would affect messaging and would mean that the pending response is out-of-date.
To ensure that the response can be applied to the declaration in the same state in which it was at time of sending, the changes you have made have been denied.
Either close without saving and await the response, or if you really wish to modify the data first set the entry as 'failed from transmission'.
Note that if the original outgoing message was accepted you may be unable to re-transmit without changing reference numbers."),
						Res.GetString("9c78aa0c-d46b-4a87-bf36-f9357589ecac", "The changes cannot be saved"));
				}
			}
			return result;
		}

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		InvoiceLineBottomPanelUserControl invoiceLineBottomPanelUserControl;
		public ZPanel PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel;
	}
}
