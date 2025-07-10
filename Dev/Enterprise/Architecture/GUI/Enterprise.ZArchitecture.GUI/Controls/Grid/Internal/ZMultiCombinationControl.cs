using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI.Grid.Internal
{
	/// <summary>
	/// A parent grid control that hosts different editors for display inside a Grid
	/// </summary>
	[SuppressFormDesignerAnalysis]
	[ToolboxItem(false)]
	[DefaultDataSourceBindingMember(null)]
	public partial class ZMultiCombinationControl : ZUserControl, IGridControl, IBindTo
	{
		public ZMultiCombinationControl()
		{
			InitializeComponent();
			ControlType = FieldType.Text;
		}

		#region Control Type / Binding

		public IGridControl CurrentEditor { get; private set; }

		public FieldType ControlType
		{
			get { return controlType; }
			set
			{
				controlType = value;

				if (CurrentEditor != null)
				{
					Controls.Remove((Control)CurrentEditor);
				}

				CurrentEditor = (IGridControl)GetControlForControlType(value);
				Controls.Add((Control)CurrentEditor);
			}
		}

		FieldType controlType = FieldType.Text;

		public void Edit(CurrencyManager source, string mappingName)
		{
			EditCore(source, mappingName);
		}

		protected virtual void EditCore(CurrencyManager source, string mappingName)
		{
			if (controlType == FieldType.Guid || controlType == FieldType.TextCodeFindBox || controlType == FieldType.OrganisationGuid || controlType == FieldType.GuidDropEdit)
			{
				var listUserControl = (ZListUserControl)CurrentEditor;
				listUserControl.DataPropertyName = mappingName;
				listUserControl.CurrentItem = source.GetCurrent();
				listUserControl.PullList();
			}
			if (controlType == FieldType.Text || controlType == FieldType.TextMultiLine)
			{
				MaybeBindTextTemplatesFactory((ZTextBox)CurrentEditor, source, mappingName);
			}
			if ((controlType == FieldType.Decimal) && !string.IsNullOrEmpty(BindToDecimalPlaces))
			{
				var calcEdit = (ZCalcEdit)CurrentEditor;
				calcEdit.BindDecimalsInternal(source.GetCurrent(), BindToDecimalPlaces);
			}
		}

		protected void MaybeBindTextTemplatesFactory(ZTextBox textBox, CurrencyManager source, string mappingName)
		{
			if (textBox.contextMenuManager.TextTemplatesFactory.dataSource == null)
			{
				textBox.contextMenuManager.TextTemplatesFactory.Bind(source, mappingName);
			}
		}

		internal Control GetControlForControlType(FieldType controlType)
		{
			IGridControl gridControl;
			if (!gridControlsCache.TryGetValue(controlType, out gridControl))
			{
				gridControl = CreateGridControl(controlType);
				((Control)gridControl).Dock = DockStyle.Fill;

				gridControlsCache[controlType] = gridControl;
			}

			return (Control)gridControl;
		}

		internal ZString GetValue(BusinessObject bizObj, string mappingName)
		{
			ZString result;
			if (ControlType == FieldType.OrganisationGuid)
			{
				var findBox = (ZGridFindBox)CurrentEditor;
				var oldList = findBox.List;
				try
				{
					findBox.CurrentItem = bizObj;
					findBox.DataPropertyName = mappingName;
					findBox.PullList();
					result = ((IFindBox)CurrentEditor).ListProvider.PrimaryKeyFromCode(CurrentEditor.Text).ToString();
				}
				finally
				{
					((ZGridFindBox)CurrentEditor).List = oldList;
				}
			}
			else if (ControlType == FieldType.Guid)
			{
				var findBox = (ZGridGuidFindBox)CurrentEditor;
				var oldList = findBox.List;
				try
				{
					findBox.CurrentItem = bizObj;
					findBox.DataPropertyName = mappingName;
					findBox.PullList();
					result = findBox.Guid.ToString();
				}
				finally
				{
					((ZGridFindBox)CurrentEditor).List = oldList;
				}
			}
			else if (ControlType == FieldType.GuidDropEdit)
			{
				var dropEdit = (ZGridGuidDropEdit)CurrentEditor;
				var oldList = dropEdit.List;
				try
				{
					dropEdit.CurrentItem = bizObj;
					dropEdit.DataPropertyName = mappingName;
					dropEdit.PullList();
					result = dropEdit.GetPK(CurrentEditor.Text).ToString();
				}
				finally
				{
					((ZGridGuidDropEdit)CurrentEditor).List = oldList;
				}
			}
			else if (ControlType == FieldType.DateTime || ControlType == FieldType.Date)
			{
				var dateEdit = (ZDateEdit)CurrentEditor;
				if (ZDateTime.TryParseExact(CurrentEditor.Text, out _, dateEdit.FormatString))
				{
					result = CurrentEditor.Text;
				}
				else
				{
					var date = ZDateEditColumnStyle.ConvertToZDateTime(dateEdit.DateTimeValue);
					if (date.HasValue)
					{
						result = date.Value.ToString(dateEdit.FormatString).ToUpperInvariant();
						dateEdit.Text = result;
					}
					else
					{
						result = CurrentEditor.Text;
					}
				}
			}
			else
			{
				result = CurrentEditor.Text;
			}

			return result;
		}

		public string BindTo { get; set; }

		public string BindToList
		{
			get { return bindToList; }
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					BindToListAndModuleIdWarner.ShowOnChangedBindToListWarning(this);
				}
				bindToList = value;
				UpdateFindBox();
			}
		}

		string bindToList;

		public ModuleIdentifier ModuleID
		{
			get { return moduleID; }
			set
			{
				if (value != ModuleIDs.NotAssigned)
				{
					BindToListAndModuleIdWarner.ShowOnChangedModuleIDWarning(this);
				}
				moduleID = value;
				UpdateFindBox();
			}
		}

		ModuleIdentifier moduleID;

		public CharacterCasing CharacterCasing
		{
			get { return characterCasing; }
			set
			{
				characterCasing = value;
				if (CurrentEditor is ZTextBox)
				{
					((ZTextBox)CurrentEditor).CharacterCasing = value;
				}
			}
		}

		CharacterCasing characterCasing;

		public char PasswordChar
		{
			get { return passwordChar; }
			set
			{
				passwordChar = value;
				if (CurrentEditor is ZTextBox)
				{
					((ZTextBox)CurrentEditor).PasswordChar = value;
				}
			}
		}

		char passwordChar;

		void UpdateFindBox()
		{
			UpdateFindBoxFromControlType(FieldType.Guid);
			UpdateFindBoxFromControlType(FieldType.GuidDropEdit);
			UpdateFindBoxFromControlType(FieldType.OrganisationGuid);
			UpdateFindBoxFromControlType(FieldType.TextCodeFindBox);
			UpdateFindBoxFromControlType(FieldType.TextDropEdit);
		}

		void UpdateFindBoxFromControlType(FieldType controlType)
		{
			IGridControl control;
			if (gridControlsCache.TryGetValue(controlType, out control))
			{
				var bindToListControl = (IBindToList)control;

				((Control)control).SetBindingMember(BindTo);
				bindToListControl.BindToList = BindToList;

				var findBox = control as ZGridFindBox;
				if (findBox != null)
				{
					findBox.ModuleID = ModuleID;
				}

				var calcEdit = control as ZCalcEdit;
				if (calcEdit != null && !string.IsNullOrWhiteSpace(BindToDecimalPlaces))
				{
					calcEdit.BindToDecimalPlaces = BindToDecimalPlaces;
				}
			}
		}

		#endregion

		#region Control Creation

		readonly Dictionary<FieldType, IGridControl> gridControlsCache = new Dictionary<FieldType, IGridControl>();

		ZCalcEdit CreateCalcEdit(Type bindToType, int decimals)
		{
			ZCalcEdit result = new ZCalcEdit.Bare();
			result.IsOnGrid = true;
			result.BorderStyle = BorderStyle.None;
			result.Core.SetBindToType(bindToType);
			result.Decimals = decimals;
			if (!string.IsNullOrWhiteSpace(BindToDecimalPlaces))
			{
				result.BindToDecimalPlaces = BindToDecimalPlaces;
			}
			result.BindTo = BindTo;
			return result;
		}

		ZDateEdit CreateDateEdit(ZDateTimePickerFormat dateTimeFormat)
		{
			ZDateEdit result = new ZDateEdit.Bare();
			result.BindTo = BindTo;
			result.IsOnGrid = true;
			result.DateTimeFormat = dateTimeFormat;
			return result;
		}

		ZDateTimeOffsetEdit CreateDateTimeOffsetEdit()
		{
			ZDateTimeOffsetEdit result = new ZDateTimeOffsetEdit.Bare();
			result.BindTo = BindTo;
			result.IsOnGrid = true;
			result.DateTimeFormat = ZDateTimePickerFormat.Long;
			return result;
		}

		ZTimeEdit CreateTimeEdit()
		{
			var result = new ZTimeEdit();
			result.BindTo = BindTo;
			result.IsOnGrid = true;
			return result;
		}

		ZGeographyEdit CreateGeographyEdit()
		{
			ZGeographyEdit result = new ZGeographyEdit.Bare();
			result.BindTo = BindTo;
			result.IsOnGrid = true;
			return result;
		}

		protected virtual IGridControl CreateGridControl(FieldType typeToCreate)
		{
			IGridControl result;

			switch (typeToCreate)
			{
				case FieldType.Date:
					result = CreateDateEdit(ZDateTimePickerFormat.Short);
					break;

				case FieldType.DateTime:
					result = CreateDateEdit(ZDateTimePickerFormat.Long);
					break;

				case FieldType.Time:
					result = CreateTimeEdit();
					break;

				case FieldType.DateTimeOffset:
					result = CreateDateTimeOffsetEdit();
					break;

				case FieldType.Geography:
					result = CreateGeographyEdit();
					break;

				case FieldType.Byte:
					result = CreateCalcEdit(typeof(ZByte), 0);
					break;

				case FieldType.Decimal:
					result = CreateCalcEdit(typeof(ZDecimal), 6);
					break;

				case FieldType.Integer:
					result = CreateCalcEdit(typeof(ZInt), 0);
					break;

				case FieldType.Boolean:
					var checkBox = new XPCheckBox();
					checkBox.CheckedChanged += CheckBox_CheckedChanged;
					result = checkBox;
					break;

				case FieldType.Guid:
					var guidFindBox = new ZGridGuidFindBox();
					guidFindBox.ModuleShowing += FindBox_ModuleShowing;
					guidFindBox.BindToList = BindToList;
					guidFindBox.ModuleID = ModuleID;
					guidFindBox.PopupSelected += guidFindBox_PopupSelected;
					result = guidFindBox;
					break;

				case FieldType.TextCodeFindBox:
					var findBox = new ZGridFindBox();
					findBox.ModuleShowing += FindBox_ModuleShowing;
					findBox.BindToList = BindToList;
					findBox.ModuleID = ModuleID;
					result = findBox;
					break;

				case FieldType.OrganisationGuid:
					var control = (ZPopupFindBox)ObjectFactory.Get<MasterFiles.Integration.IZOrganisationGridFindBox>();
					control.ModuleShowing += FindBox_ModuleShowing;
					control.BindToList = BindToList;
					control.ModuleID = ModuleID;
					result = (IGridControl)control;
					break;

				case FieldType.GuidDropEdit:
					var guidDropEdit = new ZGridGuidDropEdit();
					guidDropEdit.DataPropertyName = BindTo;
					guidDropEdit.BindToList = BindToList;
					guidDropEdit.IsOnGrid = true;
					result = guidDropEdit;
					break;

				case FieldType.Text:
					ZTextBox textBox = new ZTextBox.Bare();
					textBox.BindTo = BindTo;
					textBox.IsOnGrid = true;
					textBox.BorderStyle = BorderStyle.None;
					textBox.CharacterCasing = CharacterCasing;
					result = textBox;
					break;

				case FieldType.TextMacro:
				case FieldType.AntlrMacro:
					var macroType = typeToCreate == FieldType.TextMacro ? MacroType.DocEngine : MacroType.Antlr;
					var macrosFindBox = new ZMacrosFindBox(macroType);
					macrosFindBox.BindTo = BindTo;
					macrosFindBox.IsOnGrid = true;
					macrosFindBox.BorderStyle = BorderStyle.None;
					macrosFindBox.AllowMultipleMacroses = true;
					macrosFindBox.DataFieldsOnly = false;
					macrosFindBox.IsUsedForExpressions = true;
					macrosFindBox.CodeBox.CharacterCasing = CharacterCasing.Normal;
					macrosFindBox.UseMcrEvaluator = typeToCreate == FieldType.AntlrMacro;
					macrosFindBox.OpeningMacroTag = typeToCreate == FieldType.AntlrMacro ? "" : "<";
					macrosFindBox.ClosingMacroTag = typeToCreate == FieldType.AntlrMacro ? "" : ">";
					result = macrosFindBox;
					break;

				case FieldType.LinkLabel:
					var linkLabel = new ZLinkLabel();
					linkLabel.BorderStyle = BorderStyle.None;
					result = linkLabel;
					break;

				case FieldType.TextDropEdit:
					var gridDropEdit = new ZGridDropEdit();
					gridDropEdit.BindTo = BindTo;
					gridDropEdit.BindToList = BindToList;
					gridDropEdit.PasswordChar = PasswordChar;
					result = gridDropEdit;
					break;

				case FieldType.TextMultiLine:
					textBox = new ZTextBox.Bare();
					textBox.BindTo = BindTo;
					textBox.BorderStyle = BorderStyle.None;
					textBox.CharacterCasing = CharacterCasing.Normal;
					MultiLineTextBoxGridHelper.SetupTextBoxAsMultiline(textBox);
					result = textBox;
					break;

				default:
					throw new ArgumentException(typeToCreate.ToString(), nameof(typeToCreate));
			}

			return result;
		}

		public event EventHandler GuidFindBoxPopupSelected;

		void guidFindBox_PopupSelected(object sender, EmbeddedModulePopup.SelectedEventArgs e)
		{
			if (GuidFindBoxPopupSelected != null)
			{
				GuidFindBoxPopupSelected(this, e);
			}
		}

		public event EventHandler CheckBoxCheckedChanged;

		void CheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (CheckBoxCheckedChanged != null)
			{
				CheckBoxCheckedChanged(this, e);
			}
		}

		public event EventHandler<ModuleShowingEventArgs> FindBoxModuleShowing;

		void FindBox_ModuleShowing(object sender, ModuleShowingEventArgs e)
		{
			OnFindBoxModuleShowing(e);
		}

		void OnFindBoxModuleShowing(ModuleShowingEventArgs e)
		{
			if (FindBoxModuleShowing != null)
			{
				FindBoxModuleShowing(this, e);
			}
		}

		internal bool ShouldHandleKey(Keys keyData)
		{
			return CurrentEditor.ShouldHandleKey(keyData);
		}

		#region Preventing Double-Tabbing Column Skipping Bug

		public const int TabKey = 9;

		protected override bool ProcessKeyMessage(ref Message m)
		{
			return IsTabbingThroughGridColumn(ref m) || base.ProcessKeyMessage(ref m);
		}

		protected override bool ProcessKeyPreview(ref Message m)
		{
			return IsTabbingThroughGridColumn(ref m) || base.ProcessKeyPreview(ref m);
		}

		protected bool IsTabbingThroughGridColumn(ref Message m)
		{
			var keyCode = (int)m.WParam;
			return (keyCode == TabKey);
		}

		#endregion

		#endregion

		#region IGridControl Members

		int IGridControl.SelectionStart
		{
			get { return CurrentEditor.SelectionStart; }
			set { CurrentEditor.SelectionStart = value; }
		}

		int IGridControl.SelectionLength
		{
			get { return CurrentEditor.SelectionLength; }
			set { CurrentEditor.SelectionLength = value; }
		}

		int IGridControl.ButtonWidth
		{
			get { return CurrentEditor.ButtonWidth; }
		}

		int IGridControl.MaxLength
		{
			get { return CurrentEditor.MaxLength; }
			set { CurrentEditor.MaxLength = value; }
		}

		string IGridControl.Text
		{
			get { return CurrentEditor.Text; }
			set { CurrentEditor.Text = value; }
		}

		event EventHandler IGridControl.TextChanged
		{
			add { CurrentEditor.TextChanged += value; }
			remove { CurrentEditor.TextChanged -= value; }
		}

		event KeyEventHandler IGridControl.KeyDown
		{
			add { CurrentEditor.KeyDown += value; }
			remove { CurrentEditor.KeyDown -= value; }
		}

		void IGridControl.ActivateEditControl()
		{
			CurrentEditor.ActivateEditControl();
		}

		bool IGridControl.ShouldHandleKey(Keys keyData)
		{
			return ShouldHandleKey(keyData);
		}

		bool IGridControl.ShownForReadOnly
		{
			get { return CurrentEditor.ShownForReadOnly; }
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}

				foreach (var cachedControl in gridControlsCache.Values)
				{
					((Control)cachedControl).Dispose();
				}

				gridControlsCache.Clear();
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Bind to Decimals

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindTo))]
		public string BindToDecimalPlaces { get; set; }

		#endregion
	}
}

