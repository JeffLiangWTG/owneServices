using System;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public class APDraftInvoicesGrid : ZGrid
	{
		public APDraftInvoicesGrid() : base()
		{
			IsWholeRowSelectedOnClick = true;
		}

		protected override void OnDoubleClick(EventArgs e)
		{
			if (Columns[CurrentCell.ColumnNumber].ColumnName != "OpenInPortal")
			{
				OpenDraftInvoiceGlowLink(CurrentRowIndex);
			}

			base.OnDoubleClick(e);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			var hitInfo = HitTest(e.X, e.Y);

			if (hitInfo.Type == HitTestType.Cell && Columns[hitInfo.Column].ColumnName == "OpenInPortal" && e.Button == MouseButtons.Left)
			{
				OpenDraftInvoiceGlowLink(hitInfo.Row);
			}

			base.OnMouseDown(e);
		}

#if !WINZOR
		protected override void OnMouseHover(EventArgs e)
		{
			var hitInfo = HitTest(GetMousePosition());

			if (hitInfo.Type == HitTestType.Cell && Columns[hitInfo.Column].ColumnName == "OpenInPortal")
			{
				Cursor = Cursors.Hand;
			}
			else
			{
				Cursor = Cursors.Default;
			}

			base.OnMouseHover(e);
		}
#endif

		protected override void SetupContextMenu()
		{
			base.SetupContextMenu();
			AddOpenInPortalMenuItem();
			AddMenuSeparator();
		}

		protected virtual Point GetMousePosition()
		{
			return PointToClient(MousePosition);
		}

		void OpenDraftInvoiceGlowLink(int rowNum)
		{
			var draftInvoice = ((JobDraftInvoicePrintingFilter)this.DataSource).Transactions[rowNum];

			if (draftInvoice != null)
			{
				GlowLinksHelper.OpenEnityInGlow(GlobalNotificationsWrapper.Instance, "Goto/OpenDraftInvoiceProxy_51D729D30A234F0281658C4120CAC554", draftInvoice);
			}
		}

		void AddOpenInPortalMenuItem()
		{
			var openInPortalMenuItem = new ZMenuItem(ResString.GetMultilingualString("BAA65F9A-23E9-40B2-9B31-1860480C3C9A", "Open in Portal"), (s, e) => OpenDraftInvoiceGlowLink(CurrentRowIndex));
			ContextMenu.MenuItems.Add(0, openInPortalMenuItem);
		}

		void AddMenuSeparator()
		{
			var menuSeparator = new ZMenuItem("-");
			ContextMenu.MenuItems.Add(1, menuSeparator);
		}
	}
}
