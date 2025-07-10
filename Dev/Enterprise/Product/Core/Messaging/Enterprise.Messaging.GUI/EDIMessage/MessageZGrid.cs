using System;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Messaging.GUI
{
	public class MessageZGrid : ZGrid
	{
		public delegate void ResendInterchange();
		public ResendInterchange OnResendInterchange;

		protected override void SetupContextMenu()
		{
			base.SetupContextMenu();
			ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("0A7E4AE6-4564-4979-8B88-09D9FA614A0B", "Resend Interchange"), ResendInterchange_Click));
			OnResendInterchange = this.DoResendInterchange;
		}

		void ResendInterchange_Click(object sender, EventArgs e)
		{
			OnResendInterchange();
		}

		protected override void GetToolTip(ToolTipInfo info)
		{
			HitTestInfo hitInfo = HitTest(info.Coords.X, info.Coords.Y);
			base.GetToolTip(info);
			if ((hitInfo.Type == HitTestType.Cell || hitInfo.Type == HitTestType.ColumnHeader) && hitInfo.Column != -1)
			{
				ZForm parentForm = FindForm() as ZForm;
				if (parentForm != null)
				{
					IDynamicToolTipWithRowInfo hitTestGridColumn = TableStyles[0].GridColumnStyles[hitInfo.Column] as IDynamicToolTipWithRowInfo;
					if (hitTestGridColumn != null)
					{
						hitTestGridColumn.RowNumber = hitInfo.Row;
						hitTestGridColumn.GetToolTip(info);
					}
				}
			}
		}
	}
}
