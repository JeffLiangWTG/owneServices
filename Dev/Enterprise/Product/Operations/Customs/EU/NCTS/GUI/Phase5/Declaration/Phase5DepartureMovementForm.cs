using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.GUI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class Phase5DepartureMovementForm : ZTemplateForm, IDevToolMessageBuilderMappingPathConfigurator
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public Phase5DepartureMovementForm()
		{
			InitializeComponent();
		}

		public Phase5DepartureMovementForm(NctsHeader nctsHeader)
			: base(nctsHeader)
		{
			this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
			InitializeComponent();
			InitializeTabsLazyCreate();
			AddDeclarationDetailsTabUserControl();
			AddMessagingMenuItem();
			WorkflowTabPage.Initialize(nctsHeader);
			PlugIns.AddJobInvoicing(nctsHeader.InvoicingSupporter);
			AddDocDataPlugInIfSupported();
			SetMiscTabPageVisibility(nctsHeader);
			SetMovementsTabPageVisibility(nctsHeader);

			if (CustomsDataRegistry.Instance.DeclarationLockForEdit.Value.Any())
			{
				RegisterControlsForLock();
			}

			this.MainTabControl.SelectedIndexChanged += new EventHandler(this.MainTabControl_SelectedIndexChanged);

			nctsHeader.SynchronizeMonetaryValue();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!DesignModeFinder.IsDesigning)
			{
				new TabConfigurationManager(MainMenu, TopLevelTabControl).Enabled = true;
			}
		}

		void RegisterControlsForLock()
		{
			LockManager.Register(Core.Constants.Customs.DeclarationTabPages.Codes.NctsDepartureHeader, MainTabPage,
				new[]
				{
					nameof(Phase5DeclarationDetailsTabUserControl.DeclarationDetailsGroupBox)
				});
			LockManager.Register(Core.Constants.Customs.DeclarationTabPages.Codes.NctsDepartureTransportAndEquipment, TransportAndPackagingTabPage);
			LockManager.Register(Core.Constants.Customs.DeclarationTabPages.Codes.NctsDepartureHouse, HouseConsignmentsTabPage,
				new[]
				{
					nameof(Enterprise.Customs.EU.NCTS.GUI.HouseConsignmentsTabUserControl.GoodsItemsTabPage),
				});
			LockManager.Register(Core.Constants.Customs.DeclarationTabPages.Codes.NctsDepartureGoodsItem, HouseConsignmentsTabPage,
				new[]
				{
					nameof(Enterprise.Customs.EU.NCTS.GUI.HouseConsignmentsGridUserControl.HouseConsignmentsGrid),
					nameof(Enterprise.Customs.EU.NCTS.GUI.HouseConsignmentsTabUserControl.HouseConsignmentDetailsTabPage),
					nameof(Enterprise.Customs.EU.NCTS.GUI.HouseConsignmentsTabUserControl.HouseConsignmentSupportingDocumentsTabPage),
					nameof(Enterprise.Customs.EU.NCTS.GUI.HouseConsignmentsTabUserControl.HouseConsignmentAdditionalDocumentsTabPage),
					nameof(Enterprise.Customs.EU.NCTS.GUI.HouseConsignmentsTabUserControl.HouseConsignmentPreviousDocumentsTabPage),
					nameof(Enterprise.Customs.EU.NCTS.GUI.HouseConsignmentsTabUserControl.HouseConsignmentSupplyChainActorsTabPage),
				});
		}

		void MainTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			ZTabPage[] bottomPanelVisibleTabs =
			{
				MainTabPage,
				MovementsTabPage,
				TransportAndPackagingTabPage,
				HouseConsignmentsTabPage
			};

			PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Visible = bottomPanelVisibleTabs.Contains(MainTabControl.SelectedTab);
		}

		void AddDocDataPlugInIfSupported()
		{
			if (nctsHeader.Configuration.DocDataPlugInSupport(nctsHeader))
			{
				PlugIns.Add(ZArchitecture.Modules.ControllerIDs.DocDataPlugIn);
				PlugIns.Add(ZArchitecture.Modules.ControllerIDs.DocumentVisualizer);
			}
		}

		void AddMessagingMenuItem()
		{
			var messagingMenuItem = new Phase5NctsMessagingMenuItem();
			messagingMenuItem.NctsHeader = nctsHeader;
			MainMenu.MenuItems.Add(MainMenu.MenuItems.IndexOf(HelpMenuItem), messagingMenuItem);
		}

		void SetMiscTabPageVisibility(NctsHeader nctsHeader)
		{
			MiscTabPage.TabVisible = nctsHeader.Configuration.MiscTabPageSupport(nctsHeader);
		}

		void SetMovementsTabPageVisibility(NctsHeader nctsHeader)
		{
			MovementsTabPage.TabVisible = nctsHeader.Configuration.IsMultipleMovementsEnabled;
		}

		protected readonly NctsHeader nctsHeader;

		public override string FormCaption => nctsHeader.HumanReadableName.ToString();

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = base.ShowPreSaveDialogs();
			if (result == ContinueWithSave.Yes && nctsHeader.IsConditionR0520_UserShouldNotSaveAmendments)
			{
				var message = Res.GetString("FC61D566-4A0A-45F7-B1EA-6D3B7E952F37", "One or more fields of the declaration that have been amended are not allowed to change when the IE015 has already been accepted by customs. Do you wish to continue saving despite this rule?");
				var caption = Res.GetString("5254B03E-7D6E-4D84-BAAB-7D7E88249250", "Warning: Field(s) should not be amended");
				var dialogResult = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				result = dialogResult == DialogResult.Yes ? ContinueWithSave.Yes : ContinueWithSave.No;
			}
			if (result == ContinueWithSave.Yes && nctsHeader.IsConditionR0520_UserShouldNotSaveAmendmentsToGuarantees)
			{
				var message = Res.GetString("8590CFB2-1E3E-4DC3-A85C-8AB178A92BFC", "Either 'Guarantee' can be amended or 'Transit Operation's fields', both can't be amended simultaneously. Do you wish to continue saving despite this rule?");
				var caption = Res.GetString("191E0D90-2B89-4025-91EA-9EC1BFE848A7", "Warning: Guarantee(s) should not be amended");
				var dialogResult = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				result = dialogResult == DialogResult.Yes ? ContinueWithSave.Yes : ContinueWithSave.No;
			}
			return result;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			lockManager?.Dispose();
		}

		void InitializeTabsLazyCreate()
		{
			MessagesTabPage.RunWhenBindingOrFirstShown((s, args) => MessagesTabDynamicUserControl.UserControlType = LayoutProvider.MessageTabUserControlType);
			TransportAndPackagingTabPage.RunWhenBindingOrFirstShown((s, args) => SetTransportandPackagingTabUserControl());
			HouseConsignmentsTabPage.RunWhenBindingOrFirstShown((s, args) => SetHouseConsignmentsTabUserControl());
		}

		void SetTransportandPackagingTabUserControl()
		{
			var userControl = (ZUserControl)Activator.CreateInstance(LayoutProvider.TransportAndPackagingTabUserControlType);
			userControl.Dock = System.Windows.Forms.DockStyle.Fill;
			BindingSource.SetBindingMember(userControl, ".");
			TransportAndPackagingTabPage.Controls.Add(userControl);
		}

		void SetHouseConsignmentsTabUserControl()
		{
			var userControl = (ZUserControl)Activator.CreateInstance(LayoutProvider.HouseConsignmentsTabUserControlType);
			userControl.Dock = System.Windows.Forms.DockStyle.Fill;
			BindingSource.SetBindingMember(userControl, nameof(NctsHeader.Bills));
			HouseConsignmentsTabPage.Controls.Add(userControl);
		}

		internal CustomsControlLockManager LockManager => lockManager ?? (lockManager = new CustomsControlLockManager(nctsHeader, this)
		{
			UnlockCommandDescription = Res.GetString("DFD5E5BE-8BCE-47B6-B87A-60945B92DF20", "NCTS – Unlock Customs Declaration")
		});
		CustomsControlLockManager lockManager;

		void AddDeclarationDetailsTabUserControl()
		{
			var declarationDetailsTabUserControl = (ZUserControl)Activator.CreateInstance(LayoutProvider.DeclarationDetailsTabUserControlType);
			declarationDetailsTabUserControl.Name = "DeclarationDetailsTabUserControl";
			declarationDetailsTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			MainTabPage.Controls.Add(declarationDetailsTabUserControl);
		}

		INctsPhase5LayoutProvider LayoutProvider => layoutProvider ?? (layoutProvider = NctsPhase5LayoutProvider.GetLayoutProvider(nctsHeader?.DefaultDataGroupingCode));
		INctsPhase5LayoutProvider layoutProvider;

		bool IDevToolMessageBuilderMappingPathConfigurator.CanDisplayMessageBuilderMappingPath => true;
	}
}
