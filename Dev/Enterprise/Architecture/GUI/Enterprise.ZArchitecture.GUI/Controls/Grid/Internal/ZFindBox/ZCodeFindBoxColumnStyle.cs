using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZCodeFindBoxColumnStyle : ZBaseFindBoxColumnStyle, IListColumnStyle, ICustomKeyHandlingGridColumn
	{
		public ZCodeFindBoxColumnStyle(ZCodeFindBoxColumnStyleInfo columnInfo)
			: this(() => new ZGridFindBox(), columnInfo)
		{
		}

		protected ZCodeFindBoxColumnStyle(Func<ZGridFindBox> gridFindBox1, ZCodeFindBoxColumnStyleInfo columnInfo)
			: base(gridFindBox1, columnInfo)
		{
			this.columnInfo = columnInfo;
		}

		readonly ZCodeFindBoxColumnStyleInfo columnInfo;

		protected override void OnInit(Control control)
		{
			base.OnInit(control);
			var gridFindBox = ((ZGridFindBox)control);
			gridFindBox.ColumnStyle = this;
			gridFindBox.PopupCaption = columnInfo.PopupCaption;
			gridFindBox.BindToList = columnInfo.BindToList;
			gridFindBox.ModuleID = columnInfo.ModuleID;
			gridFindBox.AutoCompleteDisabled = columnInfo.AutoCompleteDisabled;
			gridFindBox.AllowModuleMultiSelect = columnInfo.AllowModuleMultiSelect;
			gridFindBox.ModuleShowing += new EventHandler<ModuleShowingEventArgs>(ZBaseFindBoxColumnStyle_ModuleShowing);
		}

		protected override void PrepareControlData(CurrencyManager source, int rowNum)
		{
			base.PrepareControlData(source, rowNum);

			var businessObject = source.Position == -1 ? null : source.GetCurrent();
			if (businessObject != null)
			{
				var findBox = (ZGridFindBox)EditControl;
				findBox.CurrentItem = businessObject;
				findBox.DataPropertyName = DataPropertyNameForFindBoxList;
				findBox.PullList();
			}

			SetValueInFindBox(source, rowNum);
		}

		protected virtual string DataPropertyNameForFindBoxList
		{
			get { return MappingName; }
		}

		protected ZGridFindBox GridFindBox => (ZGridFindBox)base.FindBox;

		protected override void HookControlEvents()
		{
			base.HookControlEvents();
			GridFindBox.PopupSelected += GridFindBox_PopupSelected;
		}

		protected override void UnHookControlEvents()
		{
			GridFindBox.PopupSelected -= GridFindBox_PopupSelected;
			base.UnHookControlEvents();
		}

		void GridFindBox_PopupSelected(object sender, EmbeddedModulePopup.SelectedEventArgs e)
		{
			if (e?.SelectedBusinessObjects is BusinessObject[] selectedBusinessObjects && selectedBusinessObjects.Length > 0)
			{
				GridFindBox.SetCodeDescription(selectedBusinessObjects[0]);
				if (PopupSelected != null)
				{
					PopupSelected(sender, e);
				}
			}
		}

		public event EmbeddedModulePopup.SelectedEventHandler PopupSelected;

		#region Implementation

		protected internal virtual bool HasCustomEditForm
		{
			get { return false; }
		}

		protected internal virtual void ShowEditOrViewForm(IFindBox findBox, bool readOnly)
		{
		}

		void ZBaseFindBoxColumnStyle_ModuleShowing(object sender, ModuleShowingEventArgs e)
		{
			var parentGrid = parentDataGrid as ZGrid;
			if (parentGrid != null)
			{
				parentGrid.OnFindBoxColumnModuleShowing(this, e);
			}
		}

		protected void MaybeBindTextTemplatesFactory(ZTextBox textBox, CurrencyManager source, string mappingName)
		{
			if (textBox.contextMenuManager.TextTemplatesFactory.dataSource == null)
			{
				textBox.contextMenuManager.TextTemplatesFactory.Bind(source, mappingName);
			}
		}

		#region IListColumnStyle Members

		CurrencyManager IListColumnStyle.ListManager
		{
			get { return parentZGrid.ListManager; }
		}

		#endregion

		#endregion

		#region ICustomKeyHandlingGridColumn Members	

		bool ShouldPassThroughToCodeBox(Keys key) => key == Keys.F3 && (ReadOnly || IsCurrentCellReadOnly);

		public override bool ShouldProcessCmdKey(ref Message m, Keys keyData)
		{
			return base.ShouldProcessCmdKey(ref m, keyData) || ShouldPassThroughToCodeBox(keyData);
		}

		public override bool ProcessCmdKey(ref Message m, Keys keyData)
		{
			if (ShouldPassThroughToCodeBox(keyData))
			{
				return ((ZGridFindBox)EditControl).GetCodeBox().HandleKey(keyData);
			}

			return base.ProcessCmdKey(ref m, keyData);
		}

		#endregion
	}

	#region ZCodeFindBoxColumnStyleInfo

	public class ZCodeFindBoxColumnStyleInfo : ZBaseFindBoxColumnStyleInfo, IZColumnStyleInfoWithModuleID
	{
		[ZColumnBindingMemberType(typeof(ZString))]
		public override string ColumnName
		{
			get { return base.ColumnName; }
			set { base.ColumnName = value; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType
		{
			get { return typeof(ZCodeFindBoxColumnStyle); }
		}

		#region ModuleID

		[Editor(typeof(ModuleIDEditor), typeof(UITypeEditor))]
		public ModuleIdentifier ModuleID
		{
			get { return moduleID; }
			set { moduleID = value; }
		}

		bool ShouldSerializeModuleID()
		{
			return (ModuleID != ModuleIDs.NotAssigned);
		}

		[DefaultValue(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool AutoCompleteDisabled { get; set; }

		[Browsable(true), DefaultValue(false)]
		public bool AllowModuleMultiSelect { get; set; }

		ModuleIdentifier moduleID = ModuleIDs.NotAssigned;

		#endregion
	}

	#endregion
}
