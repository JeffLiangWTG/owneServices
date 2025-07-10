using System;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public partial class Phase5DepartureMovementForm : EU.NCTS.GUI.Phase5DepartureMovementForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public Phase5DepartureMovementForm() : base()
		{
			InitializeComponent();
		}

		public Phase5DepartureMovementForm(NctsHeader nctsMovement)
			: base(nctsMovement)
		{
			InitializeComponent();
			InsertAnnexTabInCorrectIndexAndSetVisibility();
			HookStatusChangeEvents();
		}

		void InsertAnnexTabInCorrectIndexAndSetVisibility()
		{
			var index = MainTabControl.TabPages.IndexOf(HouseConsignmentsTabPage);
			MainTabControl.TabPages.Insert(AnnexTabPage, index + 1);

			AnnexTabPage.TabVisible = ((NctsHeader)nctsHeader).RequiresAnnexes();
		}

		void HookStatusChangeEvents()
		{
			nctsHeader.MovementHeader.BM_CustomsStatusInfo.ValueChanged += StatusInfo_ValueChanged;
			nctsHeader.MovementHeader.BM_MessageStatusInfo.ValueChanged += StatusInfo_ValueChanged;
		}

		void UnhookStatusChangeEvents()
		{
			nctsHeader.MovementHeader.BM_CustomsStatusInfo.ValueChanged -= StatusInfo_ValueChanged;
			nctsHeader.MovementHeader.BM_MessageStatusInfo.ValueChanged -= StatusInfo_ValueChanged;
		}

		void StatusInfo_ValueChanged(object sender, EventArgs e)
		{
			ToggleDepartureDeclarationRelatedTabsEditableState();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ToggleDepartureDeclarationRelatedTabsEditableState();
		}

		public void ToggleDepartureDeclarationRelatedTabsEditableState()
		{
			var isDepartureTabEditable = !nctsHeader.IsDepartureTabReadOnly;
			MainTabPage.UpdateEditableIncludingChildren(isDepartureTabEditable, MainTabPageControlsAllowedToRemainEditableAfterSending);
			TransportAndPackagingTabPage.UpdateEditableIncludingChildren(isDepartureTabEditable);
			HouseConsignmentsTabPage.UpdateEditableIncludingChildren(isDepartureTabEditable);
			//TODO: Waiting for WorkItem to decide when lock
			//AnnexTabPage.UpdateEditableIncludingChildren(isDepartureTabEditable);
		}

		string[] MainTabPageControlsAllowedToRemainEditableAfterSending => new string[] { nameof(TraderDetailsUserControl.CertificateDropEdit), nameof(TraderDetailsUserControl.BrokerCodeFindBox) };
	}
}
