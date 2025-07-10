using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI.Grid.Internal;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;
using KeyEventHandler = System.Windows.Forms.KeyEventHandler;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	interface IGridDropEdit
	{
		IListColumnStyle ColumnStyle { get; set; }
	}

	#region ZGrid DropEdit

	[CargoWise.Windows.UI.Testing.SuppressFormDesignerAnalysis]
	[ToolboxItem(false)]
	public class ZGridDropEdit : ZDropEdit.Bare, IGridControl, IGridDropEdit
	{
		public ZGridDropEdit()
		{
			CodeBox.IsOnGrid = true;
			ControlDpiScalingHelper.SetLeft((Control)CodeBox, CodeBox.Left - ControlDpiScalingHelper.ScaleToCurrentDpiX(1), false);
			ControlDpiScalingHelper.SetTop((Control)CodeBox, CodeBox.Top - ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
			CodeBox.KeyDown += new KeyEventHandler(CodeBox_KeyDown);
		}

		public bool IsDropButtonVisible
		{
			get { return DropButton.IsVisible; }
			set { DropButton.IsVisible = value; }
		}

		#region IGridControl Members

		int IGridControl.SelectionStart
		{
			get { return CodeBox.SelectionStart; }
			set { CodeBox.SelectionStart = value; }
		}

		int IGridControl.SelectionLength
		{
			get { return CodeBox.SelectionLength; }
			set { CodeBox.SelectionLength = value; }
		}

		string IGridControl.Text
		{
			get { return CodeBox.Text; }
			set { CodeBox.Text = value; }
		}

		event EventHandler IGridControl.TextChanged
		{
			add { CodeBox.TextChanged += value; }
			remove { CodeBox.TextChanged -= value; }
		}

		int IGridControl.ButtonWidth
		{
			get { return ZGUISystemInformation.VerticalScrollBarWidth; }
		}

		event KeyEventHandler IGridControl.KeyDown
		{
			add { CodeBox.KeyDown += value; }
			remove { CodeBox.KeyDown -= value; }
		}

		int IGridControl.MaxLength
		{
			get { return CodeBox.MaxLength; }
			set { CodeBox.MaxLength = value; }
		}

		void IGridControl.ActivateEditControl()
		{
			CodeBox.Focus();
			if (TranslationFeedbackManager.InTranslationFeedbackMode() && IsLeftMouseButtonClicked() && IsMouseOnAValidRow())
			{
				SelectItem(Text);
			}
		}

		protected virtual bool IsMouseOnAValidRow()
		{
			return Parent is ZGrid grid && grid.IsMouseOnAValidRow;
		}

		protected virtual bool IsLeftMouseButtonClicked()
		{
			return (Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left;
		}

		bool IGridControl.ShouldHandleKey(Keys keyData)
		{
			return ZGridDropEdit.ShouldHandleKey(this, keyData);
		}

		internal static bool ShouldHandleKey(ZDropEdit dropEdit, Keys keyData)
		{
			return (dropEdit.DropButton.IsDroppedDown && (keyData == Keys.Up || keyData == Keys.Down || keyData == Keys.PageUp || keyData == Keys.PageDown || keyData == Keys.Enter || keyData == Keys.Escape))
					|| keyData == (Keys.Alt | Keys.Down);
		}

		bool IGridControl.ShownForReadOnly
		{
			get { return false; }
		}

		#endregion

		#region IGridDropEdit Members

		public IListColumnStyle ColumnStyle
		{
			get { return columnStyle; }
			set { columnStyle = value; }
		}

		IListColumnStyle columnStyle;

		#endregion

		#region Implementation

		public override void CommitBoundValue()
		{
			// do nothing, unbound
		}

		protected override BindingManagerBase BindingManager
		{
			get { return ColumnStyle.ListManager; }
		}

		protected override void SynchroniseControlSizes()
		{
			var dropButton = DropButton;
			var codeBox = CodeBox;
			var descriptionBox = DescriptionBox;
			if (DropButton != null && CodeBox != null)
			{
				SuspendLayout();
				ControlDpiScalingHelper.SetHeight((Control)dropButton, Height + ControlDpiScalingHelper.ScaleToCurrentDpiX(1), false);
				ControlDpiScalingHelper.SetWidth((Control)dropButton, Width, false);

				ControlDpiScalingHelper.SetWidth((Control)codeBox, CodeBoxWidth, false);

				ControlDpiScalingHelper.SetLeft((Control)descriptionBox, DropButton.Right, false);
				ControlDpiScalingHelper.SetWidth((Control)descriptionBox, Width - DescriptionBox.Left, false);
				ResumeLayout();
			}
		}

		protected virtual int CodeBoxWidth
		{
			get { return DropButton.Width - ZGUISystemInformation.VerticalScrollBarWidth - ControlDpiScalingHelper.ScaleToCurrentDpiX(4); }
		}

		protected override ZDropButton NewDropButton()
		{
			return new ZGridDropButton();
		}

		protected override bool ProcessKeyPreview(ref Message m)
		{
			return IsTabbingThroughGridColumn(m) || base.ProcessKeyPreview(ref m);
		}

		protected bool IsTabbingThroughGridColumn(Message m)
		{
			return (Keys)(int)m.WParam == Keys.Tab;
		}

		void CodeBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.F2)
			{
				CodeBox.SelectionLength = 0;
				CodeBox.SelectionStart = CodeBox.Text.Length;
			}
		}

		new ZGridDropButton DropButton
		{
			get { return (ZGridDropButton)base.DropButton; }
		}

		protected override IList GetList(object dataSource, string listMember, string dataMemberForErrorReporting)
		{
			IList result = null;
			var dataGridColumnStyle = ColumnStyle as DataGridColumnStyle;

			if (dataGridColumnStyle == null && Parent is ZMultiCombinationControl)
			{
				dataGridColumnStyle = ((ZGrid)Parent.Parent)?.GetCurrentColumnStyle();
			}

			if (dataSource is BusinessObject && dataGridColumnStyle != null && !dataGridColumnStyle.MappingName.Equals(dataMemberForErrorReporting, StringComparison.OrdinalIgnoreCase))
			{
				if (!CodeBox.Focused || CodeBox.ReadOnly)
				{
					return null;
				}

				var grid = (ZGrid)Parent.Parent;
				var lastCell = grid.LastFocusedColumn?.LastFocusedCell;
				var columns = string.Join(",", grid.Columns.Select((x, i) => $"[{i},{x.ColumnName},{x.IsVisible},StackTraceWhenSettingInvisible:{x.StackTraceWhenSettingInvisible},StackTraceWhenSettingVisible:{x.StackTraceWhenSettingVisible}]")); // Log Information;
				var tableStyles = string.Join(",", grid.TableStyles[0].GridColumnStyles.Cast<DataGridColumnStyle>().Select((info, i) => $"[{i},{info.MappingName}, {info.GetType().FullName}]")); // Log Information
				var message = FormattableString.Invariant($@"GetCurrentColumnStyle() returned a column which is not the editing column.
dataGridColumnStyle Type: {dataGridColumnStyle.GetType().FullName}
dataGridColumnStyle MappingName: {dataGridColumnStyle.MappingName}
dataMember: {dataMemberForErrorReporting}
ColumnStyle Type: {ColumnStyle?.GetType().FullName}
CurrentCell.Column: {grid.CurrentCell.ColumnNumber}
CurrentCell.Row: {grid.CurrentCell.RowNumber}
LastCell.Column: {lastCell?.ColumnNumber}
LastCell.Row: {lastCell?.RowNumber}
HasLayoutChanged: {grid.Columns.HasLayoutChanged}
LastLayoutChangedStackTrace: {grid.Columns.LastLayoutChangedStackTrace}
LastFocusedColumn: {grid.LastFocusedColumn?.MappingName}
Columns.Now: {columns}
VisibleColumn Styles: {tableStyles}"); // Log Information

				ErrorReporter.ReportOnce("GetCurrentColumnStyle_ReturnedAnIncorrectColumn", message);
			}

			//matching ZMultiControlColumnStyle.cs ControlTypeForBizObj method
			if (dataSource is BusinessObject bizObj && dataGridColumnStyle is ZMultiControlColumnStyle)
			{
				var customColumnPropertyInfo = ZPropertyInfoRetriever.GetZPropertyInfo(dataGridColumnStyle, bizObj);

				if (customColumnPropertyInfo.BizObj != bizObj)
				{
					if (customColumnPropertyInfo.BizObj is IDynamicBusinessObject customBusinessObject)
					{
						var metaDataTypeFullName = customColumnPropertyInfo.Name + PropertyDescriptorCollectionWithMetaData.MetaDataPropertyNameSeparator + MetaDataTypes.ListDataSource;
						var listDataSourceMetaData = customBusinessObject.GetMetaData(metaDataTypeFullName);
						result = (IList)(listDataSourceMetaData != null ? listDataSourceMetaData.Value : null);
					}
				}
			}

			//fallback method
			if (result == null)
			{
				var zPropertyInfoRetriever = dataGridColumnStyle?.PropertyDescriptor as IZPropertyInfoRetriever;
				var propertyInfo = zPropertyInfoRetriever?.GetZPropertyInfo(dataSource as BusinessObject);

				if (propertyInfo != null && propertyInfo.BizObj != dataSource)
				{
					result = (IList)MetaData.GetMetaData(propertyInfo.BizObj, propertyInfo.PropertyDescriptor, MetaDataTypes.ListDataSource);
				}
			}

			return result ?? base.GetList(dataSource, listMember, dataMemberForErrorReporting);
		}

		#endregion
	}

	#endregion

	#region ZGrid GuidDropEdit

	[ToolboxItem(false)]
	public class ZGridGuidDropEdit : ZGuidDropEdit, IGridControl, IGridDropEdit
	{
		public ZGridGuidDropEdit()
		{
			CodeBox.IsOnGrid = true;
			ControlDpiScalingHelper.SetLeft((Control)CodeBox, CodeBox.Left - ControlDpiScalingHelper.ScaleToCurrentDpiX(1), false);
			ControlDpiScalingHelper.SetTop((Control)CodeBox, CodeBox.Top - ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
			CodeBox.KeyDown += new KeyEventHandler(CodeBox_KeyDown);
		}

		protected override IControlExtensionCollection NewExtensionCollection()
		{
			// grid handles everything by itself
			// duplicated functionality in ZGridColumnNotificationProvider !
			return new ControlExtensionCollection(this);
		}

		public override void CommitBoundValue()
		{
			// do nothing, unbound
		}

		protected override void SynchroniseControlSizes()
		{
			if (DropButton != null && CodeBox != null)
			{
				ControlDpiScalingHelper.SetHeight((Control)DropButton, Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				ControlDpiScalingHelper.SetWidth((Control)DropButton, Width, false);
				ControlDpiScalingHelper.SetWidth((Control)CodeBox, CodeBoxWidth, false);
			}
		}

		protected virtual int CodeBoxWidth
		{
			get { return DropButton.Width - ZGUISystemInformation.VerticalScrollBarWidth - ControlDpiScalingHelper.ScaleToCurrentDpiX(4); }
		}

		protected override ZDropButton NewDropButton()
		{
			return new ZGridDropButton();
		}

		protected override bool ProcessKeyPreview(ref Message m)
		{
			return IsTabbingThroughGridColumn(m) || base.ProcessKeyPreview(ref m);
		}

		protected bool IsTabbingThroughGridColumn(Message m)
		{
			return (Keys)(int)m.WParam == Keys.Tab;
		}

		void CodeBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.F2)
			{
				CodeBox.SelectionLength = 0;
				CodeBox.SelectionStart = CodeBox.Text.Length;
			}
		}

		protected override BindingManagerBase BindingManager
		{
			get { return ColumnStyle.ListManager; }
		}

		#region IGridControl Members

		int IGridControl.SelectionStart
		{
			get { return CodeBox.SelectionStart; }
			set { CodeBox.SelectionStart = value; }
		}

		int IGridControl.SelectionLength
		{
			get { return CodeBox.SelectionLength; }
			set { CodeBox.SelectionLength = value; }
		}

		string IGridControl.Text
		{
			get { return CodeBox.Text; }
			set { CodeBox.Text = value; }
		}

		event EventHandler IGridControl.TextChanged
		{
			add { CodeBox.TextChanged += value; }
			remove { CodeBox.TextChanged -= value; }
		}

		int IGridControl.ButtonWidth
		{
			get { return ZGUISystemInformation.VerticalScrollBarWidth; }
		}

		event KeyEventHandler IGridControl.KeyDown
		{
			add { CodeBox.KeyDown += value; }
			remove { CodeBox.KeyDown -= value; }
		}

		int IGridControl.MaxLength
		{
			get { return CodeBox.MaxLength; }
			set { CodeBox.MaxLength = value; }
		}

		void IGridControl.ActivateEditControl()
		{
			CodeBox.Focus();
		}

		bool IGridControl.ShouldHandleKey(Keys keyData)
		{
			return ZGridDropEdit.ShouldHandleKey(this, keyData);
		}

		bool IGridControl.ShownForReadOnly
		{
			get { return false; }
		}

		#endregion

		#region IGridDropEdit Members

		public IListColumnStyle ColumnStyle
		{
			get { return columnStyle; }
			set { columnStyle = value; }
		}

		IListColumnStyle columnStyle;

		#endregion
	}

	#endregion
}
