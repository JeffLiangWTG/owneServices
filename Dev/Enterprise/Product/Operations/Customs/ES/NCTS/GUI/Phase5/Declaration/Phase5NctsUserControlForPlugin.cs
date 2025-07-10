using System;
using Enterprise.ZArchitecture.GUI;
using NctsHeader = Enterprise.Customs.ES.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public partial class Phase5NctsUserControlForPlugin : EU.NCTS.GUI.Phase5NctsUserControlForPlugin
	{
		[Obsolete("Just for the Designer")]
		public Phase5NctsUserControlForPlugin()
		{
			InitializeComponent();
		}

		public Phase5NctsUserControlForPlugin(NctsHeader nctsMovement) : base(nctsMovement)
		{
			InitializeComponent();
			InsertAnnexTabInCorrectIndexAndSetVisibility();
		}

		void InsertAnnexTabInCorrectIndexAndSetVisibility()
		{
			var index = MainTabControl.TabPages.IndexOf(HouseConsignmentsTabPage);
			MainTabControl.TabPages.Insert(AnnexTabPage, index + 1);

			AnnexTabPage.TabVisible = ((NctsHeader)nctsMovement).RequiresAnnexes();
		}

		protected override ZUserControl GetNctsArrivalUserControl() => new Phase5ArrivalNotificationTabUserControl();
	}
}
