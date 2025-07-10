using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Controls;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Grid.Internal;

namespace Enterprise.ZArchitecture
{
	public abstract partial class ZCustomControlColumnStyle : ZTextBoxColumnStyle, INavigatingGridColumn
	{
		protected ZCustomControlColumnStyle(Func<Control> initializeEditControl, ZTextBoxColumnStyleInfo columnInfo)
			: base(columnInfo)
		{
			this.initializeEditControl = initializeEditControl;
			infoName = columnInfo.ColumnName;
		}

		public override Control EditControl
		{
			get
			{
				if (editControl == null && !disposed)
				{
					InitControl();
				}
				return editControl;
			}
		}
		readonly Func<Control> initializeEditControl;
		readonly string infoName;
		Control editControl;

		void InitControl()
		{
			editControl = initializeEditControl();
			try
			{
				editControl.Visible = false;
				editControl.Enter += EnterEditControl;
				editControl.Leave += LeaveEditControl;
				editControl.TabStop = false;
				editControl.Name = infoName;

				var isOnGridControl = editControl as IIsOnGrid;
				if (isOnGridControl != null)
				{
					isOnGridControl.IsOnGrid = true;
				}
				OnInit(editControl);
			}
			catch (Exception)
			{
				editControl.Dispose();
				throw;
			}
		}

		protected virtual void OnInit(Control control)
		{
		}

		public override event KeyEventHandler KeyDown
		{
			add { GridControl.KeyDown += value; }
			remove { GridControl.KeyDown -= value; }
		}

		#region Invalid Code

		protected void SaveInvalidCodeOnInfo(CurrencyManager source, int rowNum, string value)
		{
			FieldInvalidTextMemory.SetInvalidText(source.List[rowNum], MappingName, value);
		}

		protected virtual string GetInvalidCodeFromInfo(CurrencyManager source, int rowNum)
		{
			return GetInvalidCodeFromInfo(source.List[rowNum]);
		}

		protected string GetInvalidCodeFromInfo(object rowValue)
		{
			return FieldInvalidTextMemory.GetInvalidText(rowValue, MappingName);
		}

		#endregion

		#region Implementation

		protected Form GetTopLevelForm()
		{
			if (parentZGrid != null)
			{
				return FindTopForm(parentZGrid.FindForm());
			}

			return null;
		}

		Form FindTopForm(Form form)
		{
			if (form.Owner != null)
			{
				return FindTopForm(form.Owner);
			}

			return form;
		}

		protected internal override void HideEditControl()
		{
			parentZGrid.IsHidingControl = true;

			if (EditControl?.Visible ?? false)
			{
				EditControl.Bounds = Rectangle.Empty;
#if WINZOR
				EditControl.DetachFromGrid();
#endif
			}

			if (TextBox.Visible)
			{
				TextBox.Bounds = Rectangle.Empty;
#if WINZOR
				TextBox.DetachFromGrid();
#endif
			}

			parentZGrid.IsHidingControl = false;
		}

		protected internal IGridControl GridControl => (IGridControl)EditControl;

		protected internal override bool SelectEnd()
		{
			return false;
		}

		protected abstract object EditValue { get; }

		#region Width

		protected virtual int VisibleWidth
		{
			get
			{
				var result = TextBox.Width;
				var scrollBarLeft = TextBox.Parent.Width - VerticalScrollBarWidthOnGrid;

				if (TextBox.Right >= scrollBarLeft && TextBox.Right > scrollBarLeft)
				{
					result -= TextBox.Right - scrollBarLeft + ControlDpiScalingHelper.ScaleToCurrentDpiX(2);
				}

				return result;
			}
		}

		protected int VerticalScrollBarWidthOnGrid
		{
			get
			{
				var parentGridVerticalScrollBarVisible = parentZGrid != null && parentZGrid.IsVerticalScrollBarVisible;
				return parentGridVerticalScrollBarVisible ? ZGUISystemInformation.VerticalScrollBarWidth : 0;
			}
		}

		#endregion

		#region Event Handlers

		protected virtual void HandleSelectFromPopup(object sender, EventArgs e)
		{
			if (sourceData != null)
			{
				Commit(sourceData, EditingRowNum);
			}

			EditControl.Bounds = GetEditControlBounds(InitialBounds);
		}

		protected virtual void ColumnTextBoxChanged(object sender, EventArgs e)
		{
			if (shouldFireTextChangedEvents && !IsEditing)
			{
				IsEditing = true;
				if (parentZGrid != null)
				{
					CurrentText = GetCurrentText();
				}
				if (!parentZGrid.IsChangingEditControl)
				{
					ColumnStartedEditing(EditControl);
				}
			}
		}

		protected void EnterEditControl(object sender, EventArgs e)
		{
			UnHookControlEvents();
			HookControlEvents();

			ErrorText = "";
			ErrorRow = -1;
		}

		protected virtual void LeaveEditControl(object sender, EventArgs e)
		{
			shouldFireTextChangedEvents = false;
			UnHookControlEvents();

			if (!parentZGrid?.IsColumnStartingEdit ?? false)
			{
				parentZGrid.EndEdit();
				EditControl.Bounds = Rectangle.Empty;
			}

			if (!IsValid())
			{
				ErrorText = GetCurrentText();
				ErrorRow = LastFocusedCell.RowNumber;
			}

			NotificationProvider.OnLeaveEditControl();
		}

		protected override void NotifyNotificationsOnTextBoxLeave()
		{
			// do nothing
		}
#if DEBUG
		internal
#endif
		protected string CurrentText = "";
		protected string ErrorText = "";
		protected int ErrorRow = -1;
		bool shouldFireTextChangedEvents { get; set; }

		protected virtual string GetCurrentText()
		{
			return GridControl.Text;
		}

		protected virtual void HookControlEvents()
		{
			GridControl.TextChanged += ColumnTextBoxChanged;
		}

		protected virtual void UnHookControlEvents()
		{
			GridControl.TextChanged -= ColumnTextBoxChanged;
		}

		#endregion

		#region Commit / Abort / Edit

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message")]
		protected override bool Commit(CurrencyManager source, int rowNum)
		{
			if (IsEditing)
			{
				IsEditing = false;
				if (source != null && source.Position == rowNum)
				{
					object currentEditValue;
					if (PropertyDescriptor != null && typeof(MultilingualString).IsAssignableFrom(PropertyDescriptor.PropertyType))
					{
						currentEditValue = (NoResString)EditValue.ToString();
					}
					else
					{
						currentEditValue = EditValue;
					}

					if (ShouldSetValue(currentEditValue, source, rowNum) && !IsCellReadOnly(source, rowNum))
					{
						try
						{
							SetCurrentEditValue(source, rowNum, currentEditValue);
						}
						catch (ArgumentException ex) when (ex.Message.Contains("cannot be converted to type") || ex.Message.Contains("Position of ListManager must be equal to 'rowNum'"))
						{
							var errorMessage =
$@"ArgumentException occured while calling ZCustomControlColumnStyle Commit
GridControl:{ControlDescription.GetControlPath(GridControl as Control)},
HeaderText:{HeaderText},
TextBox Value: {TextBox.Text},
Current Row Number: {rowNum},
MappingName : {MappingName},
Read Only: {ReadOnly},
Is Submissive: {IsSubmissive},
Last Focus Cell Column Number: {LastFocusedCell.ColumnNumber},
Last Focus Cell Row Number: {LastFocusedCell.RowNumber},
Current Cell Column Number: {parentDataGrid.CurrentCell.ColumnNumber},
Current Cell Row Number: {parentDataGrid.CurrentCell.RowNumber},
Data Source : {source?.List?.GetType().FullName},
source Type:{source?.List[rowNum]?.GetType().FullName},
Data Source Position: {source.Position},
Data Source Count :{source?.List?.Count ?? -1},
currentEditValue Type: {currentEditValue.GetType().FullName}
Stack Trace : {ex.StackTrace}";
							var key = "ZCustomControlColumnStyle_Commit_SetCurrentEditValue_Error|" + ex.Message;
							ErrorReporter.ReportOnce(key, errorMessage, ex);
						}
					}
				}
			}

			return true;
		}

		protected virtual void SetCurrentEditValue(CurrencyManager source, int rowNum, object currentEditValue)
		{
			SetColumnValueAtRow(source, rowNum, currentEditValue);
		}

		protected virtual bool ShouldSetValue(object currentEditValue, CurrencyManager source, int rowNum)
		{
			var existingValue = GetColumnValueAtRow(source, rowNum);
			var alreadyNull = existingValue.Equals(DBNull.Value) && currentEditValue.Equals(NullText);

			return (!existingValue.Equals(currentEditValue) || currentEditValue.Equals(GuidEmpty)) && !alreadyNull;
		}

		string guidEmpty;
		string GuidEmpty
		{
			get
			{
				if (guidEmpty == null)
				{
					guidEmpty = Guid.Empty.ToString();
				}
				return guidEmpty;
			}
		}

		protected override void Abort(int rowNum)
		{
			HideEditControl();
		}

		bool openedEdit = true;

		protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText, bool cellVisible)
		{
			shouldFireTextChangedEvents = false;

			base.Edit(source, rowNum, bounds, readOnly, instantText, cellVisible);

			if (CanEdit && (parentZGrid == null || (!parentZGrid.GetState(ZGrid.GridState.IsScrolling)) || parentZGrid.SelectedRowCount < 1))
			{
				PrepareControlData(source, rowNum);

				readOnly = IsCellReadOnly(source, rowNum);

				if (!readOnly || EditControlShownForReadOnly)
				{
					PrepareEditControl(source, rowNum, bounds, readOnly);
					ShowEditControl(cellVisible);
					shouldFireTextChangedEvents = true;
				}
				openedEdit = true;
			}
			else
			{
				openedEdit = false;
			}
		}

		bool EditControlShownForReadOnly
		{
			get
			{
				var result = EditControlShownForReadOnlyCore;
				if (!result)
				{
					var combinationControl = EditControl as ZMultiCombinationControl;
					result = EditControlShownForReadOnlyCore || combinationControl != null && combinationControl.CurrentEditor != null && combinationControl.CurrentEditor.ShownForReadOnly;
				}

				return result;
			}
		}

		protected virtual bool EditControlShownForReadOnlyCore
		{
			get { return false; }
		}

		protected virtual void PrepareControlData(CurrencyManager source, int rowNum) { }

		protected virtual void PrepareEditControl(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly)
		{
#if !WINZOR
			TextBox.SelectionLength = 0;
#endif
			EditControl.Parent = TextBox.Parent;
			EditControl.Bounds = GetEditControlBounds(bounds);

			if (!IsEditing)
			{
				InitialiseEditControlForNewPositionWithCurrent(source, readOnly);
			}
		}

		protected void InitialiseEditControlForNewPositionWithCurrent(CurrencyManager source, bool readOnly)
		{
			if (source.Position != -1)
			{
				var bizObj = source.GetCurrent() as BusinessObject;
				if (bizObj != null)
				{
					InitialiseEditControlForNewPosition(bizObj, readOnly);
				}
			}
		}

		protected virtual void ShowEditControl(bool cellVisible)
		{
			if (cellVisible)
			{
				EditControl.Visible = cellVisible;
			}

			EditControl.BringToFront();
			ActivateEditControl();

			TextBox.Visible = false;

			if (parentZGrid.IsColumnStartingEdit)
			{
				GridControl.Text = CurrentText;
				GridControl.SelectionLength = 0;
				GridControl.SelectionStart = CurrentText != null ? CurrentText.Length : 0;
				CurrentText = "";
			}
		}

		protected virtual void ActivateEditControl()
		{
			GridControl.ActivateEditControl();
		}

		protected virtual int GetAvailableWidthForButton(int requestedWidth, Rectangle currentBounds)
		{
#if WINZOR
			var maxAvailableSpaceForButton = parentDataGrid.ClientRectangle.Right - GetRowCellBoundsWithActualX().Right - ControlDpiScalingHelper.ScaleToCurrentDpiX(2);
			return maxAvailableSpaceForButton < requestedWidth ? 0 : requestedWidth;
#else
			var maxAvailableSpaceForButton = parentDataGrid.ClientRectangle.Right - currentBounds.Right - ControlDpiScalingHelper.ScaleToCurrentDpiX(2);
			return maxAvailableSpaceForButton < requestedWidth ? maxAvailableSpaceForButton : requestedWidth;
#endif
		}

		#endregion

		[return: DpiState(DpiState.ScaledVariant)]
#if DEBUG
		internal
#endif
		protected override Rectangle GetEditControlBounds(Rectangle bounds)
		{
			bounds = base.GetEditControlBounds(bounds);
			ControlDpiScalingHelper.SetWidth(ref bounds, bounds.Width + (IsCellReadOnly(sourceData, EditingRowNum) ? 0 : GetAvailableWidthForButton(GridControl.ButtonWidth, bounds)), false);

			return bounds;
		}
#if DEBUG
		internal
#endif
		protected virtual void InitialiseEditControlForNewPosition(BusinessObject bizObj, bool readOnly)
		{
			if (!readOnly)
			{
				var maxLength = GetMaxLength(bizObj);
				if (maxLength >= 0)
				{
					GridControl.MaxLength = maxLength;
				}
			}
		}
#if DEBUG
		internal
#endif
		protected virtual int GetMaxLength(BusinessObject bizObj)
		{
			var info = ZPropertyInfoRetriever.GetZPropertyInfo(this, bizObj);
			if (info == null && ((IBusinessObjectInternals)bizObj).IsUnCommittedRow)
			{
				return -1;
			}
			else if (info == null)
			{
				throw new Exception(ZGUIConstants.GetPropertyInfoDoesNotExistError(MappingName));
			}
			return info.MaxLength;
		}

		internal protected bool IsEditing;

		#region Paint

		/// <summary>
		/// Used in Paint method
		/// </summary>
		protected virtual bool IsEditControlFocused
		{
			get { return EditControl?.Focused ?? false; }
		}

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

		protected Brush GetBackgroundColourBrush(Brush originalBackBrush, CurrencyManager source, int paintingRowNum)
		{
			var newBackBrush = originalBackBrush;
			if (((ZGrid)parentDataGrid).IsIndexInRange(paintingRowNum))
			{
				var isHasRowsAndRowHighlighted = parentDataGrid.IsSelected(paintingRowNum);
				if (!isHasRowsAndRowHighlighted)
				{
					var isFocusedCell = source.Position == paintingRowNum && IsEditControlFocused;
					var focusedBrush = isFocusedCell ? BrushProvider.FromColor(EnterpriseFormLookStrategy.SelectedControlColor) : originalBackBrush;
					var customRowBackgroundColor = parentZGrid.GetCustomRowBackgroundColour(paintingRowNum, IsCellReadOnly(source, paintingRowNum));

					if (customRowBackgroundColor.ToArgb() != 0)
					{
						newBackBrush = BrushProvider.FromColor(customRowBackgroundColor);
					}
					else
					{
						if (IsCellReadOnly(source, paintingRowNum))
						{
							newBackBrush = parentZGrid.ReadOnlyBrushFromRowNum(paintingRowNum);
						}
						else
						{
							newBackBrush = focusedBrush;
						}
					}
				}
			}
			return newBackBrush;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "See Issue 00852474., See Issue 00852474. Imperfectly correlated with low available physical || virtual memory.")]
		protected override void Paint(Graphics graphics, Rectangle bounds, CurrencyManager source, int paintingRowNum, Brush backBrush, Brush foreBrush, bool alignToRight)
		{
			try
			{
				var newBackBrush = GetBackgroundColourBrush(backBrush, source, paintingRowNum);

				graphics.FillRectangle(newBackBrush, bounds);

				if (IsValidSource(source))
				{
					newBackBrush = NotificationProvider.DecideBackgroundColorBrush(source, paintingRowNum, newBackBrush);
					bounds = NotificationProvider.PaintNotificationIconIfRequiredAndReturnRemainingAreaForPainting(graphics, source, paintingRowNum, bounds, newBackBrush);
				}

				var cellFont = parentZGrid.GetCustomCellFont(paintingRowNum, ColumnInfo.ColumnName);
				var cellText = ColumnTextAtRow(source, paintingRowNum);
				var textBox = TextBox;
				if (textBox != null
					&& !textBox.IsDisposed
					&& textBox.PasswordChar != '\0')
				{
					cellText = new string(textBox.PasswordChar, cellText.Length);
				}

				PaintText(graphics, bounds, source, paintingRowNum, cellText, cellFont, newBackBrush, foreBrush, false);
			}
			catch (InvalidOperationException ex) { if (ex.Message != "Object is currently in use elsewhere.") { throw; } ZUserControl.ReportGraphicsDisplayFailure(ex); }
			catch (System.Runtime.InteropServices.ExternalException ex) { if (ex.Message != "A generic error occurred in GDI+." && ex.Message != "External component has thrown an exception.") { throw; } ZUserControl.ReportGraphicsDisplayFailure(ex); }
		}

#endif

		#endregion

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (!disposed)
				{
					editControl?.Dispose();
					disposed = true;
				}
			}

			base.Dispose(disposing);
		}
		bool disposed;

		#endregion

		#region INavigatingGridColumn Members

		bool INavigatingGridColumn.ShouldColumnHandleKey(Keys keyData)
		{
			return ShouldColumnHandleKey(keyData);
		}

		protected virtual bool ShouldColumnHandleKey(Keys keyData)
		{
			return ShouldMoveCursor(keyData) || keyData == Keys.F2;
		}

		protected virtual bool ShouldMoveCursor(Keys keyData)
		{
			if (!openedEdit)
			{
				return false;
			}

			try
			{
				var selectionStart = GridControl.SelectionStart;
				var selectionLength = GridControl.SelectionLength;
				if (selectionStart == 0 && selectionLength == 0 && TextBox != null)
				{
					selectionStart = TextBox.SelectionStart;
					selectionLength = TextBox.SelectionLength;
				}

				var textLength = GridControlTextLength;
				if (textLength == 0 && TextBox != null)
				{
					textLength = TextBox.TextLength;
				}

				var shouldNavigateLeft = (selectionStart == 0 && selectionLength == 0 && keyData == Keys.Left);
				var shouldNavigateRight = (selectionStart == textLength && selectionLength == 0 && keyData == Keys.Right);
				var isSelecting = ((keyData & Keys.Modifiers) == Keys.Shift) && ((keyData & Keys.KeyCode) != Keys.Up) && ((keyData & Keys.KeyCode) != Keys.Down);
				var isMovingCursor = ((keyData == Keys.Left || keyData == Keys.Right) && (selectionLength != textLength));

				return (isSelecting || isMovingCursor) && !shouldNavigateLeft && !shouldNavigateRight;
			}
			catch (NullReferenceException ex)
			{
				var message = FormattableString.Invariant($@"{ex.Message}
GridControl == null ? {editControl == null}
Disposed: {disposed}"); // Log Information, do not need to be translated
				throw new NullReferenceException(message, ex);
			}
		}

		protected virtual int GridControlTextLength
		{
			get
			{
				return GridControl.Text != null ? GridControl.Text.Length : 0;
			}
		}

		#endregion
	}
}
