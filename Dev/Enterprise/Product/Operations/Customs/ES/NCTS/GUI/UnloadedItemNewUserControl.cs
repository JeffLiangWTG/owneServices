using System;
using Enterprise.Customs.ES.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public partial class UnloadedItemNewUserControl : EU.NCTS.GUI.UnloadedItemNewUserControl
	{
		public UnloadedItemNewUserControl()
		{
			InitializeComponent();
		}

		NctsHeader Header => (NctsHeader)CurrentDataItem;

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			UnhookEvents();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			HookEvents();
		}

		#region Hook / Unhook Events

		void HookEvents()
		{
			var header = Header;
			if (header != null)
			{
				header.ESNctsHeader.CEN_PreviousSummaryDeclarationInfo.ValueChanged += CEN_PreviousSummaryDeclarationInfo_ValueChanged;
				CEN_PreviousSummaryDeclarationInfo_ValueChanged(null, null);
			}
		}

		void UnhookEvents()
		{
			var header = Header;
			if (header != null)
			{
				header.ESNctsHeader.CEN_PreviousSummaryDeclarationInfo.ValueChanged -= CEN_PreviousSummaryDeclarationInfo_ValueChanged;
			}
		}

		#endregion

		void CEN_PreviousSummaryDeclarationInfo_ValueChanged(object sender, EventArgs e)
		{
			BillOfLadingVisibility(Header.ESNctsHeader.CEN_PreviousSummaryDeclaration.IsEmpty);
		}

		void BillOfLadingVisibility(bool previousSummaryEmpty)
		{
			BillOfLadingTextBox.Visible = !previousSummaryEmpty;
		}
	}
}
