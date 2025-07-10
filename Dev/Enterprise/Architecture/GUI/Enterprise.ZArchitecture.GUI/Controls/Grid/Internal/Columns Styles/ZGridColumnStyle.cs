using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.Core.Forms
{
	public abstract partial class ZGridColumnStyle : DataGridTextBoxColumn, IGridColumnStyle, IResCaptionedControl, IHotkeyProvider, ICustomKeyHandlingGridColumn
	{
		public ZGridColumnStyle(ZGridColumnInfo columnInfo)
		{
			MappingName = columnInfo.ColumnName;
			ControlDpiScalingHelper.SetWidth(this, columnInfo.Width, false);
			ReadOnly = columnInfo.IsReadOnly;
			IsSubmissive = columnInfo.IsSubmissive;
			IsSortable = columnInfo.IsSortable;
			IsCustomColumn = columnInfo.IsCustomColumn;
			CaptionResourceString = columnInfo.CaptionResourceString;
			IsSensitiveValue = columnInfo.IsSensitiveValue;

			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly)
		{
			Edit(source, rowNum, bounds, readOnly, null, true);
		}

		protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string displayText)
		{
			Edit(source, rowNum, bounds, readOnly, displayText, true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Track info for developers")]
		protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string displayText, bool cellIsVisible)
		{
			UserEventTracker.Instance.AddUserEvent(this.parentDataGrid,
				"DataGrid Cell Begin Edit",
				MappingName + "<" + ToString() + ">");
#if WINZOR
			if (GetColumnValueAtRow(source, rowNum) == null)
			{
				return;
			}
#endif

			base.Edit(source, rowNum, bounds, readOnly, displayText, cellIsVisible);
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
				TextBox?.Dispose();
			}

			base.Dispose(isNotFinalizing);
		}

		public ResourceStringData CaptionResourceString { get; set; }

		public override bool ReadOnly
		{
			get { return base.ReadOnly; }
			set
			{
				base.ReadOnly = value;
				if (parentZGrid != null)
				{
					parentZGrid.SetTabStop();
				}
			}
		}

		[Browsable(false)]
		public Font HeaderFont { get; set; }

		public string GetValueAsString(CurrencyManager source, int rowNum)
		{
			return ColumnTextAtRow(source, rowNum);
		}

		protected virtual void OnGettingColumnTextAtRow(CurrencyManager source, int rowNum)
		{
		}

		internal string FormatValueObject(object source, object propertyValue)
		{
			return FormatValueObjectCore(source, propertyValue);
		}

		protected virtual string FormatValueObjectCore(object source, object propertyValue)
		{
			return propertyValue != null ? propertyValue.ToString() : string.Empty;
		}
#if DEBUG
		internal
#endif
		protected string ColumnTextAtRow(CurrencyManager source, int rowNum)
		{
			OnGettingColumnTextAtRow(source, rowNum);

			var sourceList = (source == null) ? null : source.List;
			return sourceList != null && rowNum > -1 && rowNum < sourceList.Count
				? FormatValueObject(source.List[rowNum], GetColumnValueAtRowCore(source, rowNum))
				: string.Empty;
		}

		#region Implementation
#if DEBUG
		internal
#endif
		protected CurrencyManager sourceData;
		internal protected DataGrid parentDataGrid;
		protected ZGrid parentZGrid;
		protected override object GetColumnValueAtRow(CurrencyManager source, int rowNum)
		{
			return GetColumnValueAtRowCore(source, rowNum);
		}
#if DEBUG
		internal object GetColumnValueAtRowExposed(CurrencyManager source, int rowNum)
		{
			return GetColumnValueAtRow(source, rowNum);
		}
#endif
		object GetColumnValueAtRowCore(CurrencyManager source, int rowNum)
		{
			try
			{
				return
#if DEBUG
 GetColumnValueAtRowOverride ??
#endif
 ((source.Position > -1 && rowNum < source.Count) ? base.GetColumnValueAtRow(source, rowNum) : GetEmptyColumnValue());
			}
			catch (NoConcreteTypeException ex)
			{
				throw new NoConcreteTypeException(FormattableString.Invariant($"{parentZGrid.NameForDebugging};/t Column name: {MappingName}"), ex);
			}
		}

#if DEBUG
		internal object GetColumnValueAtRowOverride;
#endif

		protected virtual object GetEmptyColumnValue()
		{
			var propertyType = PropertyDescriptor?.PropertyType;
			if (propertyType != null && propertyType.IsValueType)
			{
				return Activator.CreateInstance(propertyType);
			}
			else if (propertyType == typeof(ZString) || propertyType == typeof(string))
			{
				return string.Empty;
			}
			else
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "GetEmptyColumnValue() must be overriden in the column style \"{0}\" when the corresponding property type \"{1}\" is null or a class.", GetType().FullName, propertyType?.FullName ?? "null"));
			}
		}

#if DEBUG

		internal object GetEmptyColumnValue_Exposed()
		{
			return GetEmptyColumnValue();
		}

#endif

		public void SetParentGrid(DataGrid grid)
		{
			SetDataGrid(grid);
		}

		protected override void SetDataGrid(DataGrid grid)
		{
			parentDataGrid = grid;
			parentZGrid = grid as ZGrid;
		}
#if DEBUG && !WINZOR
		internal void SetDataGridExposed(DataGrid grid)
		{
			SetDataGrid(grid);
		}

		internal void PaintExposed(Graphics graphics, Rectangle rectangle, CurrencyManager currencyManager, int rowNum, Brush brush1, Brush brush2, bool alignToRight)
		{
			Paint(graphics, rectangle, currencyManager, rowNum, brush1, brush2, alignToRight);
		}
#endif
		protected override void SetDataGridInColumn(DataGrid grid)
		{
			suspendQueryHeaders = true;
			try
			{
				SetDataGrid(grid);

				if (parentZGrid != null)
				{
					parentZGrid.SetTabStop();
				}

				base.SetDataGridInColumn(grid);
			}
			finally
			{
				suspendQueryHeaders = false;
			}
		}

		protected
#if WINZOR
		override
#endif
		bool IsCurrentCellReadOnly
		{
			get
			{
				return
#if DEBUG
 isCurrentCellReadOnlyForTest ||
#endif
 parentZGrid != null && parentZGrid.ListManager != null && IsCellReadOnly(parentZGrid.ListManager, parentZGrid.CurrentCell.RowNumber);
			}
		}

#if DEBUG
		protected internal bool isCurrentCellReadOnlyForTest;
#endif

		protected internal bool IsCellReadOnly(CurrencyManager source, int rowNum)
		{
			var result = parentDataGrid.ReadOnly || ReadOnly;
			if (!result)
			{
				if (IsValidSourceForRowNum(source, rowNum))
				{
					var current = source.List[rowNum];
					result = IsCellReadOnlyCore(current);
					if (!result)
					{
						var currentAsBusinessObject = current as IAccessBusinessObject;
						if (currentAsBusinessObject != null)
						{
							result = currentAsBusinessObject.IsPropertyReadOnly(MappingName);
						}
					}
				}
			}
			return result;
		}

		protected virtual bool IsCellReadOnlyCore(object current)
		{
			return false;
		}

		protected bool IsValidSourceForRowNum(CurrencyManager source, int rowNum)
		{
			return (source != null && source.Count > 0 && rowNum < source.Count);
		}

		protected StringFormat StringFormat(bool isRightToLeft, bool useEllipsis, bool wordWrap)
		{
			var stringFormat = new StringFormat();

			if (isRightToLeft)
			{
				stringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
			}
			if (!wordWrap)
			{
				stringFormat.FormatFlags |= StringFormatFlags.NoWrap;
			}

			switch (Alignment)
			{
				case HorizontalAlignment.Center:
					stringFormat.Alignment = StringAlignment.Center;
					break;
				case HorizontalAlignment.Left:
					stringFormat.Alignment = StringAlignment.Near;
					break;
				case HorizontalAlignment.Right:
					stringFormat.Alignment = StringAlignment.Far;
					break;
			}

			if (useEllipsis)
			{
				stringFormat.Trimming = StringTrimming.EllipsisCharacter;
			}

			return stringFormat;
		}

		protected string PadRightIfRightAligned(string text)
		{
#if DEBUG
			if (Globals.IsTest && !PadRightForTesting)
			{
				return text;
			}
#endif

#if !WINZOR
			// This is a hack to fix .Net Bug with right justified Column captions being chopped off at the end.
			return Alignment == HorizontalAlignment.Right ? text + PadRightPadding : text;
#else
			return text;
#endif
		}
		internal bool PadRightForTesting;

		internal static readonly string PadRightPadding = new string(new char[] { (char)32, (char)31 });

		internal IDisposable UsePadRightForTesting()
		{
			PadRightForTesting = true;
			return new DisposableAction(() => PadRightForTesting = false);
		}

#region Resource Headers

		public override string HeaderText
		{
			get
			{
				if (parentDataGrid == null || parentDataGrid.DataSource == null)
				{
					return base.HeaderText;
				}

				return !suspendQueryHeaders
						 ? PadRightIfRightAligned(ResourceHeader.GetHeaderText(base.HeaderText, HeaderTextWasDefaulted))
						 : base.HeaderText;
			}
			set
			{
				base.HeaderText = value;
				HeaderTextWasDefaulted = string.IsNullOrEmpty(value);
			}
		}

		internal string ColumnCaption
		{
			get
			{
				if (parentDataGrid == null || parentDataGrid.DataSource == null)
				{
					return base.HeaderText;
				}

				return !suspendQueryHeaders
						 ? ResourceHeader.GetHeaderText(base.HeaderText, HeaderTextWasDefaulted, true)
						 : base.HeaderText;
			}
		}

		public virtual void RefreshHeader()
		{
			ResourceHeader.RefreshHeader();
		}

		internal ZGridColumnStyleResourceHeader ResourceHeader
		{
			get { return resourceHeader ?? (resourceHeader = new ZGridColumnStyleResourceHeader(this)); }
		}
		ZGridColumnStyleResourceHeader resourceHeader;

		bool suspendQueryHeaders;
		internal bool HeaderTextWasDefaulted;

		bool IGridColumnStyle.HeaderTextWasDefaulted
		{
			get { return HeaderTextWasDefaulted; }
		}

		internal int ColumnWidthForHeader
		{
			get { return Alignment == HorizontalAlignment.Right ? Width : Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(WithOfNonWritibleAreaInColumnHeader); }
		}

		internal const int WithOfNonWritibleAreaInColumnHeader = 8;

#endregion

#region NotificationProvider

#if DEBUG
		public GridColumnNotificationProvider NotificationProviderForTesting
		{
			get { return NotificationProvider; }
		}
#endif

		public GridColumnNotificationProvider NotificationProvider
		{
			get { return gridColumnNotificationProvider ?? (gridColumnNotificationProvider = GetNewGridColumnNotificationProvider()); }
		}
		GridColumnNotificationProvider gridColumnNotificationProvider;

		protected virtual GridColumnNotificationProvider GetNewGridColumnNotificationProvider()
		{
			return new ZGridColumnNotificationProvider(this);
		}

#endregion

#endregion

#region IGridColumnStyle Members

		DataGrid IGridColumnStyle.Grid
		{
			get { return parentDataGrid; }
		}

#endregion

#region ISubmissiveColumn

		public bool IsSubmissive { get; protected set; }

#endregion

#region Hotkeys

		public HotkeyRegister Hotkeys { get; } = new HotkeyRegister();

		public virtual string TypeNameForDisplay => Res.GetString("2298e999-7f71-494a-8a62-bee6e62f34ee", "Grid Column");

		public virtual bool ShouldProcessCmdKey(ref Message m, Keys keyData)
		{
			return Hotkeys.IsRegistered(keyData);
		}

		public virtual bool ProcessCmdKey(ref Message m, Keys keyData)
		{
			return Hotkeys.ProcessCmdKey(this, keyData);
		}

#endregion

#region IsSortable

		internal bool IsSortable { get; private set; }

#endregion

#region IsCustomColumn

		internal bool IsCustomColumn { get; set; }

#endregion

#region IsSensitiveValue
		public bool IsSensitiveValue { get; private set; }

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant for parsing")]
		internal static bool CoerceToBool(object o)
		{
			if (o == null)
			{
				return false;
			}
			else if (o is bool || o is ZBool)
			{
				return new ZBool(o);
			}
			else if (o is string || o is ZString)
			{
				var oAsString = o.ToString();
				return oAsString == Constants.BooleanTrueString || oAsString.ToLower().Equals("true");
			}

			throw new ArgumentException(string.Format("Object cannot be coerced into a boolean. Type: {0}, Value: \"{1}\"", o.GetType().FullName, o));
		}
	}
}
