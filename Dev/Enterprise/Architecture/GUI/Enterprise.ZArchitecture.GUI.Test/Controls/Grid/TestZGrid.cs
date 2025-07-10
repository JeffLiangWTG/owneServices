using System;
using System.Reflection;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Excel;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class TestZGrid : ZGrid
	{
		public bool HasLeft;

		public void PopupContextMenu()
		{
			typeof(ZGrid).GetMethod("ContextMenu_Popup", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(this, new object[] { this, EventArgs.Empty });
		}

		public void OnLeave()
		{
			base.OnLeave(EventArgs.Empty);
			HasLeft = true;
		}

		public void OnEnter()
		{
			base.OnEnter(EventArgs.Empty);
		}

#if !WINZOR
		public void WndProc(Message message)
		{
			base.WndProc(ref message);
		}
#endif

		public new void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
		}

		public new void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
		}

		public void DeleteRow(HitTestInfo hitInfo)
		{
			MouseUpInfo = hitInfo;
			DeleteMenuItem_Click(null, new EventArgs());
		}

		public void PreProcessMessage(Message testMessage, Keys key)
		{
#if !WINZOR
			base.PreProcessMessage(ref testMessage);
#else
			KeySender.PostKeyDown(this, this.Handle, key);
			Application.DoEvents();
#endif
		}

		public new MenuItem DeleteMenuItem
		{
			get { return base.DeleteMenuItem; }
		}

#if !WINZOR

		public void PostToWndProc(Message msg)
		{
			base.WndProc(ref msg);
		}

#endif

		public void SetCellEdited()
		{
			IsCellEdited = true;
		}

		public void SetCustomiseBizObj(ZGridCustomiseBizObj customiseBizObj)
		{
			this.customiseBizObj = customiseBizObj;
		}

		protected override int RowUnderMouse()
		{
			return 0;
		}

		internal override ExcelExporter ExcelExporter => (lastExporter = base.ExcelExporter);
		public ExcelExporter lastExporter;
	}
}
