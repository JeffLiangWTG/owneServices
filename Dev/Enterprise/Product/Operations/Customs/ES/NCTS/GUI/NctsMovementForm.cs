using System;
using Enterprise.Customs.ES.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public partial class NctsMovementForm : EU.NCTS.GUI.NctsMovementForm
	{
		public NctsMovementForm(NctsHeader nctsMovement)
			: base(nctsMovement)
		{
			InitializeComponent();
		}

		protected override Type GetDeclarationDetailsUserControlType() => typeof(DeclarationDetailsTabUserControl);

		protected override Type GetGoodsItemUserControlType() => typeof(NctsGoodsItemsUserControl);

		protected override Type GetArrivalNotificationUserControlType() => typeof(NctsArrivalUserControl);

		protected override Type GetUnloadingRemarksUserControlType() => typeof(UnloadingRemarksUserControl);

		protected override Type GetSecurityUserControlType() => typeof(SecurityTabUserControl);

		protected override string[] ControlsAllowedToRemainEditableAfterSending => new string[] { "CertificateDropEdit", "BrokerFindBox" };

		void NotesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.NotesTabPage.SuspendLayout();
			this.NotesTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(true);
		}
	}
}
