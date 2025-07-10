using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZMultiLineTextBoxColumnInfo : ZTextBoxColumnStyleInfo
	{
		public ZMultiLineTextBoxColumnInfo(string columnName, int width)
			: base(columnName, width)
		{
		}

		public ZMultiLineTextBoxColumnInfo()
		{
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType
		{
			get { return typeof(ZMultiLineTextBoxColumnStyle); }
		}

		public int MinimumEditControlWidth
		{
			get { return minWidth; }
			set { minWidth = value; }
		}

		#region Implementation

		int minWidth = 300;

		#endregion
	}

	public partial class ZMultiLineTextBoxColumnStyle : ZTextBoxColumnStyle, ICustomKeyHandlingGridColumn, INavigatingGridColumn
	{
		public ZMultiLineTextBoxColumnStyle(ZMultiLineTextBoxColumnInfo columnInfo)
			: base(columnInfo)
		{
			MultiLineTextBoxGridHelper.SetupTextBoxAsMultiline(TextBox);
		}

		#region Implementation

		protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText, bool cellVisible)
		{
			base.Edit(source, rowNum, bounds, readOnly, instantText, cellVisible);
			if (!IsCellReadOnly(source, rowNum))
			{
				TextBox.SelectionStart = 0;
				TextBox.SelectionLength = 0;
			}
		}
#if DEBUG
		internal
#endif
		protected override Rectangle GetEditControlBounds(Rectangle bounds)
		{
			var minHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(200);

			InitialBounds = ControlDpiScalingHelper.NewScaledRectangle(bounds.Location, bounds.Size, false);
			var editControlBounds = ControlDpiScalingHelper.NewScaledRectangle(bounds.Location, bounds.Size, false);

			editControlBounds = NotificationProvider.AdjustEditControlBoundsForNotificationIconIfRequired(editControlBounds, sourceData, sourceData.Position);

			if (editControlBounds.Width < ColumnInfo.MinimumEditControlWidth)
			{
				ControlDpiScalingHelper.SetWidth(ref editControlBounds, ColumnInfo.MinimumEditControlWidth, false);
			}

			var scrollBarWidth = parentZGrid.IsVerticalScrollBarVisible ? SystemInformation.VerticalScrollBarArrowHeight : 0;
			if (editControlBounds.Width + editControlBounds.Left > parentZGrid.ClientRectangle.Width - scrollBarWidth)
			{
				ControlDpiScalingHelper.SetWidth(ref editControlBounds, parentZGrid.ClientRectangle.Width - editControlBounds.X - scrollBarWidth - ControlDpiScalingHelper.ScaleToCurrentDpiX(1), false);
			}

			if (editControlBounds.Height < minHeight)
			{
				ControlDpiScalingHelper.SetHeight(ref editControlBounds, minHeight, false);
			}

			var scrollBarHeight = parentZGrid.IsHorizontalScrollBarVisible ? SystemInformation.HorizontalScrollBarHeight : 0;
			if (editControlBounds.Height + editControlBounds.Y > parentZGrid.ClientRectangle.Height - scrollBarHeight)
			{
				ControlDpiScalingHelper.SetHeight(ref editControlBounds, parentZGrid.ClientRectangle.Height - editControlBounds.Y - scrollBarHeight - ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
			}

			return editControlBounds;
		}

		new ZMultiLineTextBoxColumnInfo ColumnInfo
		{
			get { return (ZMultiLineTextBoxColumnInfo)base.ColumnInfo; }
		}

		#endregion

		#region ICustomKeyHandlingGridColumn Members

		#if !WINZOR

		public override bool ShouldProcessCmdKey(ref Message m, Keys keyData)
		{
			return (MultiLineTextBoxGridHelper.ShouldProcessCmdKey(ref m, keyData) && !IsCellReadOnly(sourceData, EditingRowNum)) || base.ProcessCmdKey(ref m, keyData);
		}

		public override bool ProcessCmdKey(ref Message m, Keys keyData)
		{
			return MultiLineTextBoxGridHelper.ProcessCmdKey((DataGridTextBox)TextBox, ref m, keyData) || base.ProcessCmdKey(ref m, keyData);
		}

		#endif

		#endregion

		#region INavigatingGridColumn Members

		bool INavigatingGridColumn.ShouldColumnHandleKey(Keys keyData)
		{
			return (keyData == Keys.Enter) || (keyData == Keys.Up) || (keyData == Keys.Down);
		}

		#endregion
	}
}
