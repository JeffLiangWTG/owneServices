using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Controls;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
#if !WINZOR
using Enterprise.ZArchitecture.GUI.Drawing;
#endif
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture
{
	public class ZTextBoxColumnStyleInfo : ZGridColumnInfo, IZColumnStyleInfo, IOverridablePropertyDescriptor
	{
		public ZTextBoxColumnStyleInfo() // required for ZGrid column designer
		{
		}

		public ZTextBoxColumnStyleInfo(string columnName, int width)
			: base(columnName, width)
		{
		}

		[ZColumnBindingMemberType(typeof(ZString))]
		public override string ColumnName
		{
			get { return base.ColumnName; }
			set { base.ColumnName = value; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType
		{
			get { return typeof(ZTextBoxColumnStyle); }
		}

		[DefaultValue(HorizontalAlignment.Left)]
		public HorizontalAlignment TextAlign { get; set; }

		[DefaultValue(false)]
		public bool WordWrap { get; set; }

		[DefaultValue('\0')]
		public char PasswordChar { get; set; }

		public override bool IsSensitiveValue => PasswordChar != '\0' || base.IsSensitiveValue;

		[DefaultValue(-1)]
		public int MaxLengthOverride
		{
			get
			{
				return maxLengthOverride;
			}

			set
			{
				maxLengthOverride = value;
			}
		}
		int maxLengthOverride = -1;

#region Macro Templates

		[DefaultValue(false)]
		public bool SupportsMacroTemplates { get; set; }

		[DefaultValue(MacroConstants.DefaultMacroOpeningBracket)]
		public string MacroOpeningBracket
		{
			get { return macroOpeningBracket; }
			set { macroOpeningBracket = value; }
		}
		string macroOpeningBracket = MacroConstants.DefaultMacroOpeningBracket;

		[DefaultValue(MacroConstants.DefaultMacroClosingBracket)]
		public string MacroClosingBracket
		{
			get { return macroClosingBracket; }
			set { macroClosingBracket = value; }
		}
		string macroClosingBracket = MacroConstants.DefaultMacroClosingBracket;

		[DefaultValue(true)]
		public bool AllowMultipleMacroses
		{
			get { return allowMultipleMacroses; }
			set { allowMultipleMacroses = value; }
		}

		[DefaultValue(false)]
		public bool HideMacroFields { get; set; }

		[DefaultValue(false)]
		public bool HideDataFields { get; set; }

		[DefaultValue(false)]
		public bool ShowXmlFields { get; set; }

		[DefaultValue(1)]
		public int DefaultCollectionIndex { get; set; }

		[DefaultValue(null), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Type XmlType { get; set; }

		[DefaultValue(false)]
		public bool UseMcrEvaluator { get; set; }

		bool allowMultipleMacroses = true;

#endregion

#region IPropertyDescriptorOverridable Members

		PropertyDescriptor IOverridablePropertyDescriptor.PropertyDescriptor { get; set; }

#endregion
	}

	public partial class ZTextBoxColumnStyle : ZGridColumnStyle, IZColumn, IMacroBox
	{
		public ZTextBoxColumnStyle(ZTextBoxColumnStyleInfo columnInfo)
			: base(columnInfo)
		{
			CharacterCasing = columnInfo.CharacterCasing;
			Alignment = columnInfo.TextAlign;
			NullText = "";
			TextBox.Multiline = false;

			TextBox.KeyDown += TextBox_KeyDown;
			TextBox.Leave += TextBox_Leave;
			TextBox.TextChanged += TextBox_TextChanged;
			TextBox.PasswordChar = columnInfo.PasswordChar;
			ColumnInfo = columnInfo;
			TextBox.Name = ColumnInfo.ColumnName;
			FormatInfo = Enterprise.ZArchitecture.Core.Culture.CurrentCompanyCountryCulture;
			WordWrap = columnInfo.WordWrap;
			SetColumnMaxLengthToEditControl(columnInfo.MaxLengthOverride);
		}

		bool hasHadKeyPress;

		void TextBox_TextChanged(object sender, EventArgs e)
		{
			if (hasHadKeyPress && TextBox != null && (TextBox.Bounds.Width == 0 || !TextBox.Visible) && !TextBox.ReadOnly && parentZGrid != null && parentZGrid.DataGridRowsLength > 1)
			{
				TextBox.Bounds = lastBounds;
			}
			hasHadKeyPress = false;
		}

		void TextBox_KeyDown(object sender, KeyEventArgs e)
		{
			hasHadKeyPress = true;
			if (e.KeyCode == Keys.Insert && TextBox.ReadOnly)
			{
				e.SuppressKeyPress = true;
				e.Handled = true;
			}
		}

		public ZTextBoxColumnStyle(ZTextBoxColumnStyle columnStyle)
			: this(columnStyle.ColumnInfo)
		{ }

		public bool WordWrap
		{
			get;
			set;
		}

#if DEBUG
		internal void ColumnStartedEditingForTest(Control editingControl)
		{
			ColumnStartedEditing(editingControl);
		}
#endif

		#region Implementation

		protected internal ZTextBoxColumnStyleInfo ColumnInfo;
		public DataGridCell LastFocusedCell;

		public virtual bool IsValid()
		{
			return true;
		}

		public virtual event KeyEventHandler KeyDown
		{
			add { TextBox.KeyDown += value; }
			remove { TextBox.KeyDown -= value; }
		}

		public void ShowBalloon(CurrencyManager manager, int rowNumber, Rectangle bounds)
		{
			NotificationProvider.ShowHelpBalloon(manager, rowNumber, bounds);
		}

#region Edit Control

		public virtual Control EditControl
		{
			get { return TextBox; }
		}

		protected internal virtual void HideEditControl()
		{
			if (TextBox.Visible)
			{
				parentZGrid.IsHidingControl = true;
				TextBox.Bounds = Rectangle.Empty;
				#if WINZOR
				TextBox.DetachFromGrid();
				#endif
				parentZGrid.IsHidingControl = false;
			}
		}

		protected internal virtual bool SelectEnd()
		{
			TextBox.SelectionLength = 0;
			TextBox.SelectionStart = TextBox.Text.Length;

			return true;
		}

#endregion

#region Characters Casing

		protected virtual string GetTextWithCasing(string value)
		{
			switch (CharacterCasing)
			{
				case CharacterCasing.Upper:
					return value.ToUpper();
				case CharacterCasing.Lower:
					return value.ToLower();
				default:
					return value;
			}
		}

		public CharacterCasing CharacterCasing
		{
			get { return TextBox.CharacterCasing; }
			set { TextBox.CharacterCasing = value; }
		}

#endregion

#region Get/Set Value
#if DEBUG
		public bool Commit_DebugAccess(CurrencyManager dataSource, int rowNum)
		{
			return Commit(dataSource, rowNum);
		}
#endif

		protected override bool Commit(CurrencyManager dataSource, int rowNum)
		{
			if (dataSource != null && dataSource.Position == rowNum && PropertyDescriptor.HasSetter())
			{
				var textBox = (DataGridTextBox)TextBox;
				if (!textBox.IsInEditOrNavigateMode && !textBox.ReadOnly && !IsCurrentCellReadOnly)
				{
					object value = null;
					if (typeof(MultilingualString).IsAssignableFrom(PropertyDescriptor.PropertyType))
					{
						value = (NoResString)TextBox.Text;
					}
					else
					{
						if (PropertyDescriptor.Converter.IsValid(TextBox.Text))
						{
							value = PropertyDescriptor.Converter.ConvertFromString(TextBox.Text);
						}
					}
					if (value == null)
					{
						return false;
					}

					try
					{
						SetColumnValueAtRow(dataSource, rowNum, value);	
					}
					catch (IndexOutOfRangeException ex)
					{
						ErrorReporter.ReportOnce("IndexOutOfRangeException_ZTextBoxColumnStyle_Commit",
							"IndexOutOfRangeException occured while calling SetColumnValueAtRow\r\n"
							+ "TextBox Value: " + TextBox.Text + "\r\n"
							+ "Current Row Number: " + rowNum.ToString(CultureInfo.InvariantCulture) + "\r\n"
							+ "MappingName : " + MappingName + "\r\n"
							+ "Read Only: " + ReadOnly.ToString(CultureInfo.InvariantCulture) + "\r\n"
							+ "Is Submissive: " + IsSubmissive.ToString(CultureInfo.InvariantCulture) + "\r\n"
							+ "Font: " + HeaderFont.ToString() + "\r\n"
							+ "Header Text: " + HeaderText + "\r\n"
							+ "Last Focus Cell Column Number: " + LastFocusedCell.ColumnNumber.ToString(CultureInfo.InvariantCulture) + "\r\n"
							+ "Last Focus Cell Row Number: " + LastFocusedCell.RowNumber.ToString(CultureInfo.InvariantCulture) + "\r\n"
							+ "Current Cell Column Number: " + parentDataGrid.CurrentCell.ColumnNumber.ToString(CultureInfo.InvariantCulture) + "\r\n"
							+ "Current Cell Row Number: " + parentDataGrid.CurrentCell.RowNumber.ToString(CultureInfo.InvariantCulture) + "\r\n"
							+ "Data Source : " + dataSource?.List?.GetType().FullName + "\r\n"
							+ "Data Source Position: " + dataSource.Position.ToString(CultureInfo.InvariantCulture) + "\r\n"
							+ "Data Source Count :" + (dataSource?.List?.Count ?? -1).ToString(CultureInfo.InvariantCulture) + "\r\n"
							+ "Data Source Filter : " + (dataSource?.List as IBusinessObjectCollection)?.CompleteFilter.LiteralTextADO + "\r\n"
							+ "Data Source Equals Source Data: " + dataSource.Equals(sourceData).ToString(CultureInfo.InvariantCulture) + "\r\n"
							+ "Last List Change Stack Trace : " + parentZGrid?.lastListChangedStackTrace,//Issue 01241835 
							ex);
					}

					EndEdit();
				}
			}
			return true;
		}

		protected override void Abort(int rowNum)
		{
			if (sourceData != null && rowNum < sourceData.Count)
			{
				TextBox.Text = GetColumnValueAtRow(sourceData, rowNum).ToString();
			}
			else
			{
				TextBox.Text = "";
			}
		}

		protected override string FormatValueObjectCore(object source, object propertyValue)
		{
			return GetTextWithCasing(base.FormatValueObjectCore(source, propertyValue));
		}

#endregion

#region ParentDataGrid Assignment & ParentDataGrid Dependant Initialisation

		protected override void SetDataGridInColumn(DataGrid grid)
		{
			base.SetDataGridInColumn(grid);
			SetColumnMaxLength();
			AddEmailColumnIfApplicable();
		}

		void AddEmailColumnIfApplicable()
		{
			var propertyDescriptor = PropertyDescriptor;
			if (!hasRegisteredEmailAddress && propertyDescriptor != null && propertyDescriptor.HasSetter() && propertyDescriptor.Attributes.OfType<EmailAddressAttribute>().Any())
			{
				hasRegisteredEmailAddress = true;
				Hotkeys.RegisterHotKey(Keys.Control | Keys.E, SetEmail, Res.GetString("b0d46ff0-475a-4a22-8af0-c4211c30b1d1", "Set to your email address"));
			}
		}
		bool hasRegisteredEmailAddress;

		bool SetEmail(object sender, Keys keyData)
		{
			if (EditControl.GetReadOnly() || !EditControl.Enabled)
			{
				return false;
			}

			var emailAddress = Env.CurrentUser.EmailAddress;
			if (!string.IsNullOrEmpty(emailAddress))
			{
				SetEmailCore(emailAddress);
			}
			else
			{
				Globals.Message.ShowInformation(Res.GetString("29c82a85-acc1-4cec-a814-b624a875da70", "Your email address has not been set"));
			}

			return true;
		}
#if DEBUG
		internal
#endif
		protected virtual void SetEmailCore(string emailAddress)
		{
			TextBox.Text = emailAddress;

			var bindBusinessObject = parentZGrid.ListManager.GetCurrent() as BusinessObject;
			using (bindBusinessObject?.GetValidationSuspender())
			{
				SetColumnValueAtRow(parentZGrid.ListManager, parentZGrid.CurrentRowIndex, (ZString)emailAddress);
				EndEdit();
			}
		}

		void SetColumnMaxLength()
		{
			if (parentZGrid != null && parentZGrid.ListManager != null)
			{
				if (parentZGrid.ListManager.List is DataView)
				{
					var maxLength = ((DataView)parentZGrid.ListManager.List).Table.Columns[MappingName].MaxLength;
					SetColumnMaxLengthToEditControl(maxLength);
				}
			}
		}

		protected virtual void SetColumnMaxLengthToEditControl(int maxLength)
		{
			if (maxLength > -1)
			{
				TextBox.MaxLength = maxLength;
			}
		}

#endregion

#region Edit

		[DpiState(DpiState.ScaledVariant)]
		Rectangle lastBounds = Rectangle.Empty;

		protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText, bool cellVisible)
		{
			if (TextBox.IsDisposed)
			{
				return;
			}

			sourceData = source;
			EditingRowNum = rowNum;

			var editBounds = GetEditControlBounds(bounds);
			lastBounds = editBounds;

			if (CanEdit && (parentZGrid == null || (!parentZGrid.GetState(ZGrid.GridState.IsScrolling)) || parentZGrid.SelectedRowCount < 1))
			{
				parentZGrid.ResetSelection();
				parentZGrid.AssignCurrentColumn(this);
				readOnly |= IsCellReadOnly(source, EditingRowNum);

				LastFocusedCell = parentDataGrid.CurrentCell;

				base.Edit(source, rowNum, editBounds, readOnly, instantText, cellVisible);

				TextBox.BackColor = readOnly ? SystemColors.Control : EnterpriseFormLookStrategy.SelectedControlColor;
			}

			EditControl.Parent = TextBox.Parent;
			NotificationProvider.OnEnterEditControl(source, rowNum, bounds, EditControl);

			if (CanEdit && source.Position > -1 && source.Count > 0)
			{
				var current = source.GetCurrent() as BusinessObject;
				if (current != null)
				{
					var maxLength = CargoWise.ComponentModel.MetaData.GetMaxLength(current, TypeDescriptor.GetProperties(current)[MappingName]);
					if (maxLength > -1)
					{
						TextBox.MaxLength = maxLength;
					}
				}
			}

			if (contextMenuManager == null)
			{
				contextMenuManager = new ZTextBoxBaseContextMenuManager(source, MappingName, TextBox, this);
				contextMenuManager.InsertingTemplateText += new EventHandler(delegate { ColumnStartedEditing(TextBox); ((DataGridTextBox)TextBox).IsInEditOrNavigateMode = false; });
			}
		}

		protected bool CanEdit
		{
			get { return parentZGrid == null || parentZGrid.CanEdit; }
		}
#if DEBUG
		internal
#endif
		protected virtual Rectangle GetEditControlBounds(Rectangle bounds)
		{
			var editControlBounds = ControlDpiScalingHelper.NewScaledRectangle(bounds.Location.X, bounds.Location.Y, bounds.Size.Width, bounds.Size.Height, false);

			if (ShouldDrawNotifications(sourceData))
			{
				editControlBounds = NotificationProvider.AdjustEditControlBoundsForNotificationIconIfRequired(editControlBounds, sourceData, sourceData.Position);
			}

			return editControlBounds;
		}

		protected bool ShouldDrawNotifications(CurrencyManager source)
		{
			return parentZGrid != null && parentZGrid.ShouldShowNotifications && IsValidSource(source);
		}
#if DEBUG
		internal
#endif
		protected int EditingRowNum;
		protected Rectangle InitialBounds;

#endregion

#region Macro Templates

		string IMacroBox.MacroOpeningBracket => ColumnInfo.MacroOpeningBracket;
		string IMacroBox.MacroClosingBracket => ColumnInfo.MacroClosingBracket;
		bool IMacroBox.IsMacroControl => ColumnInfo.SupportsMacroTemplates;
		bool IMacroBox.AllowMultipleMacros => ColumnInfo.AllowMultipleMacroses;
		public bool DataFieldsOnly { get; set; }
		public bool MacroFieldsOnly { get; set; }
		ResourceStringData IMacroBox.InsertMacroCaption => null;
		TextBox IMacroBox.TextBox => TextBox;

		public bool HideMacroFields => ColumnInfo.HideMacroFields;
		public bool HideDataFields => ColumnInfo.HideDataFields;
		public bool ShowXmlFields => ColumnInfo.ShowXmlFields;
		public int DefaultCollectionIndex => ColumnInfo.DefaultCollectionIndex;
		public Type XmlType => ColumnInfo.XmlType;
		public bool UseMcrEvaluator => ColumnInfo.UseMcrEvaluator;

		void IMacroBox.StartedEditing(TextBox t)
		{
			ColumnStartedEditing(t);
			((DataGridTextBox)t).IsInEditOrNavigateMode = false;
		}
		IMapTreePresentationManager IMacroBox.MacroManager { get; set; }
		EventHandler IMacroBox.InsertMacroHandler => this.GetInsertMacroHandler();
		EventHandler IMacroBox.PreviewMacroHandler => this.GetPreviewMacroHandler();

		object IMacroBox.DataSource
		{
			get
			{
				var rowNum = EditingRowNum;
				return rowNum >= 0 && rowNum < sourceData.List.Count ? sourceData.List[rowNum] : null;
			}
		}

		void PreviewMacro(object sender, EventArgs e) => this.PreviewMacro();

		public bool ShouldEscapeAllSpecialCharacters { get; set; }

		#endregion

		#region Paint

#if !WINZOR

		protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum)
		{
			Paint(g, bounds, source, rowNum, false);
		}

		protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, bool alignToRight)
		{
			var backBrush = BrushProvider.FromColor(TextBox.BackColor);
			var foreBrush = BrushProvider.FromColor(TextBox.ForeColor);
			Paint(g, bounds, source, rowNum, backBrush, foreBrush, alignToRight);
		}

		protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int paintingRowNum, Brush backBrush, Brush foreBrush, bool alignedToRight)
		{
			if (!parentZGrid.IsSelected(paintingRowNum))
			{
				var customRowBackgroundColour = parentZGrid.GetCustomRowBackgroundColour(paintingRowNum, IsCellReadOnly(source, paintingRowNum));

				if (IsCellReadOnly(source, paintingRowNum))
				{
					backBrush = (customRowBackgroundColour.ToArgb() != 0) ? BrushProvider.FromColor(customRowBackgroundColour) : parentZGrid.ReadOnlyBrushFromRowNum(paintingRowNum);
					g.FillRectangle(backBrush, bounds);
				}
				else if (customRowBackgroundColour.ToArgb() != 0)
				{
					backBrush = BrushProvider.FromColor(customRowBackgroundColour);
					g.FillRectangle(backBrush, bounds);
				}
			}

			if (ShouldDrawNotifications(source))
			{
				backBrush = NotificationProvider.DecideBackgroundColorBrush(source, paintingRowNum, backBrush);
				bounds = NotificationProvider.PaintNotificationIconIfRequiredAndReturnRemainingAreaForPainting(g, source, paintingRowNum, bounds, backBrush);
			}

			var cellText = GetColumnTextAtRow(source, paintingRowNum);
			var textBox = TextBox;
			if (textBox != null
				&& !textBox.IsDisposed
				&& textBox.PasswordChar != '\0')
			{
				cellText = new string(textBox.PasswordChar, cellText.Length);
			}

			var cellFont = parentZGrid.GetCustomCellFont(paintingRowNum, ColumnInfo.ColumnName);
			PaintText(g, bounds, source, paintingRowNum, cellText, cellFont, backBrush, foreBrush, false);
		}

		[Obsolete("This method is here to prevent calling the DataGrid.PaintText method which should not be used.", true)]
		protected
#if DEBUG
		internal
#endif
		new void PaintText(Graphics g, Rectangle bounds, string cellText, Brush backBrush, Brush foreBrush, bool rightToLeft)
		{
			PaintText(g, bounds, null, -1, cellText, DataGridTableStyle.DataGrid.Font, backBrush, foreBrush, rightToLeft, true);
		}

		protected
#if DEBUG
		internal
#endif
		virtual void PaintText(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, string cellText, Font cellFont, Brush backBrush, Brush foreBrush, bool rightToLeft)
		{
			PaintText(g, bounds, source, rowNum, cellText, cellFont, backBrush, foreBrush, rightToLeft, true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "See Issue 00852474., See Issue 00852474. Imperfectly correlated with low available physical || virtual memory.")]
		protected
#if DEBUG
		internal
#endif
		void PaintText(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, string cellText, Font cellFont, Brush backBrush, Brush foreBrush, bool rightToLeft, bool useEllipsis)
		{
			try
			{
				using (var format = StringFormat(rightToLeft, useEllipsis, WordWrap))
				{
					g.FillRectangle(backBrush, bounds);

					bounds.Offset(0, 2);
					ControlDpiScalingHelper.SetHeight(ref bounds, bounds.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(2), false);

					if (cellFont == null)
					{
						cellFont = DataGridTableStyle.DataGrid.Font;
					}
					if (!WordWrap)
					{
						cellText = cellText.Trim();
					}
					cellText = g.GetDrawableStringByTrimmingEnd(cellText);

					if (PaintHighlights != null && source != null)
					{
						PaintHighlights(g, bounds, source, rowNum, cellText, format, cellFont, backBrush, foreBrush, rightToLeft, useEllipsis);
					}

					TextRendererHelper.DrawText(g, cellText, cellFont, bounds, foreBrush, format);
				}
			}
			catch (InvalidOperationException ex) { if (ex.Message != "Object is currently in use elsewhere.") { throw; } ZUserControl.ReportGraphicsDisplayFailure(ex); }
			catch (System.Runtime.InteropServices.ExternalException ex) { if (ex.Message != "A generic error occurred in GDI+." && ex.Message != "External component has thrown an exception.") { throw; } ZUserControl.ReportGraphicsDisplayFailure(ex); }
		}

		public event PaintHighlightsDelegate PaintHighlights;

		public delegate void PaintHighlightsDelegate(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, string cellText, StringFormat format, Font cellFont, Brush backBrush, Brush foreBrush, bool rightToLeft, bool useEllipsis);
#if DEBUG
		internal Size GetPreferredSizeExposed(Graphics g, object value)
		{
			return GetPreferredSize(g, value);
		}
#endif
		protected override Size GetPreferredSize(Graphics g, object value) // for double click resizing
		{
			if (value is string || value is ZString || value is Decimal || value is ZDecimal)
			{
				var valueString = value.ToString();
				return ControlDpiScalingHelper.NewScaledSize(TextRenderer.MeasureText(valueString, DataGridTableStyle.DataGrid.Font), true)
					+ ControlDpiScalingHelper.NewScaledSize(new Size(4 + (DataGridTableStyle.GridLineStyle == DataGridLineStyle.Solid ? 1 : 0), 1));
			}
			else
			{
				return base.GetPreferredSize(g, value);
			}
		}

#endif

		#region Paint Error Providers

		public bool IsValidSource(CurrencyManager source)
		{
			return (source != null && source.Count > 0 && source.Position > -1);
		}

		#endregion

		protected bool HasReportedArgumentException;

		protected string GetColumnTextAtRow(CurrencyManager source, int rowNum)
		{
			try
			{
				return ColumnTextAtRow(source, rowNum);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (!HasReportedArgumentException)
				{
					Globals.Message.ShowWarning(Res.GetString("30860bd7-2208-4554-a00a-c4e8717efb4f",
						"Exception occurred while getting the cell text: {0}. Column name: {1}, row number: {2}.",
						ex.Message, this.MappingName, rowNum));

					var message = FormattableString.Invariant($@"{ex.Message}
Grid: {this.parentDataGrid.Name} ({ControlDescription.GetControlPath(this.parentDataGrid)})
Current Column Name: {this.MappingName}
Current Row Number: {rowNum}
Data Source: {source?.List?.GetType()?.FullName}");
					ErrorReporter.ReportOnce("ArgumentException_ZTextBoxColumnStyle_GetColumnTextAtRow", message, ex);

					HasReportedArgumentException = true;
				}

				return (NoResString)"*ERROR*";
			}
		}

#endregion

#region TextBox Events

		void TextBox_Leave(object sender, EventArgs e)
		{
			if (!leaving)
			{
				leaving = true;

				try
				{
					TextBox.BackColor = TextBox.ReadOnly ? SystemColors.Control : SystemColors.Window;
					parentZGrid.EndEdit(this, LastFocusedCell.RowNumber, false);
					NotifyNotificationsOnTextBoxLeave();
				}
				finally
				{
					leaving = false;
				}
			}
		}

		protected virtual void NotifyNotificationsOnTextBoxLeave()
		{
			NotificationProvider.OnLeaveEditControl();
		}

		bool leaving;

#endregion

#region Mouse handlers

		public void OnMouseHover(CurrencyManager listManager, int row, Point mouseLocationInCell, Rectangle cellBounds)
		{
			NotificationProvider.OnMouseHover(listManager, row, mouseLocationInCell, cellBounds);
		}

		public void OnMouseLeave(CurrencyManager listManager, int row)
		{
			NotificationProvider.OnMouseLeave(listManager, row);
		}

#endregion

#region Dispose

		internal ZTextBoxBaseContextMenuManager contextMenuManager;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (EditControl != null)
				{
					EditControl.Dispose();
				}
				if (contextMenuManager != null)
				{
					contextMenuManager.Dispose();
				}
			}

			base.Dispose(disposing);
		}

#endregion

#region Overridable PropertyDescriptor

		public override PropertyDescriptor PropertyDescriptor
		{
			get
			{
				return ColumnInfo != null && ((IOverridablePropertyDescriptor)ColumnInfo).PropertyDescriptor != null
					? ((IOverridablePropertyDescriptor)ColumnInfo).PropertyDescriptor
					: base.PropertyDescriptor;
			}
			set { base.PropertyDescriptor = value; }
		}

		protected override bool IsCellReadOnlyCore(object current)
		{
			var readonlyOnComponentRetriver = PropertyDescriptor as IPropertyReadonlyOnComponentRetriver;
			return base.IsCellReadOnlyCore(current) || readonlyOnComponentRetriver != null && readonlyOnComponentRetriver.IsReadOnlyOnComponent(current);
		}

#endregion

#endregion
	}
}
