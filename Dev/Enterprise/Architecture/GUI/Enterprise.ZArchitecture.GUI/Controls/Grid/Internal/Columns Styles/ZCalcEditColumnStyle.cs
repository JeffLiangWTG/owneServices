using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.GUI;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture
{
	#region ZCalcEditColumnStyleInfo class

	public class ZCalcEditColumnStyleInfo : ZTextBoxColumnStyleInfo
	{
		#region Constructors

		public ZCalcEditColumnStyleInfo(string columnName, int width, int decimals, bool allowNegative)
			: base(columnName, width)
		{
			this.Decimals = decimals;
		}

		public ZCalcEditColumnStyleInfo(string columnName, int width, int decimals) : this(columnName, width, decimals, false) { }

		public ZCalcEditColumnStyleInfo() { }

		#endregion

		#region Properties
		[ZColumnBindingMemberType(typeof(INumericZType))]
		public string BindToDecimalPlaces { get; set; }

		[ZColumnBindingMemberType(typeof(INumericZType))]
		public override string ColumnName
		{
			get { return base.ColumnName; }
			set { base.ColumnName = value; }
		}

		#region ShowGroupSeparators
		[DefaultValue(true)]
		public bool ShowGroupSeparators
		{
			get { return showGroupSeparators; }
			set { showGroupSeparators = value; }
		}

		bool showGroupSeparators = true;
		#endregion

		[DefaultValue(false)]
		public bool ShowEmptyStringForEmptyValue { get; set; }

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType
		{
			get { return typeof(ZCalcEditColumnStyle); }
		}

		#region Decimals

		[DefaultValue(DefaultDecimals)]
		public int Decimals
		{
			get { return fDecimals; }
			set
			{
				fDecimals = value;
				DecimalsOverridden = true;
			}
		}

		/// <summary>
		/// Keep at 0 for 'use the maximum value of the numerical type of the calc edit core'.
		/// </summary>
		[DefaultValue(typeof(decimal), "0")]
		public decimal MaxValue
		{
			get
			{
				return maxValue;
			}
			set
			{
				maxValue = value;
			}
		}

		protected decimal maxValue;
		protected int minValue;

		protected const int DefaultDecimals = 2;
		protected int fDecimals = DefaultDecimals;

		internal bool DecimalsOverridden { get; private set; }

		#endregion

		#region AllowNegative

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool AllowNegative
		{
			get { return fAllowNegative; }
			set { }
		}
		protected bool fAllowNegative = true;

		#endregion
		#endregion
	}

	#endregion

	#region ZCalcEditColumnStyle class

	public partial class ZCalcEditColumnStyle : ZTextBoxColumnStyle, IPastableControl
	{
		public ZCalcEditColumnStyle(ZCalcEditColumnStyleInfo columnInfo)
			: base(columnInfo)
		{
			Format = GetFormatString();
			Alignment = HorizontalAlignment.Right;

			TextBox.KeyPress += TextBox_KeyPress;
			TextBox.Leave += TextBox_Leave;
			TextBox.KeyDown += TextBox_KeyDown;

			RegisterHotkeys();
		}

		protected override void SetDataGrid(DataGrid value)
		{
			base.SetDataGrid(value);
			ParentZGrid = value as ZGrid;
		}

		protected override void SetDataGridInColumn(DataGrid value)
		{
			base.SetDataGridInColumn(value);
			ParentZGrid = value as ZGrid;
			InitializeDataGridInColumn = true;
		}

		internal ZGrid ParentZGrid { get; private set; }

		internal bool InitializeDataGridInColumn { get; private set; }

		#region Properties

		public int Decimals
		{
			get { return Core.Decimals; }
			set
			{
				Core.Decimals = value;
				Format = GetFormatString();

				if (sourceData != null)
				{
					var dataGridTextBox = (DataGridTextBox)TextBox;
					var wasInEditOrNavigateMode = dataGridTextBox.IsInEditOrNavigateMode;

					dataGridTextBox.IsInEditOrNavigateMode = false;
					try
					{
						Commit(sourceData, EditingRowNum);
					}
					finally
					{
						dataGridTextBox.IsInEditOrNavigateMode = wasInEditOrNavigateMode;
					}
				}
			}
		}

		#endregion

		#region Core

		protected internal ZCalcEditCore Core
		{
			get { return core ?? (core = GetNewCalcEditCore()); }
		}
		ZCalcEditCore core;

		protected bool CoreIsCreated
		{
			get { return core != null; }
		}

		protected virtual ZCalcEditCore GetNewCalcEditCore()
		{
			return new ZCalcEditCore(TextBox, this);
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing && core != null)
			{
				core.Dispose();
			}

			base.Dispose(disposing);
		}

		#endregion

		internal ZCalcEditColumnStyleInfo CalcEditColumnInfo
		{
			get { return (ZCalcEditColumnStyleInfo)ColumnInfo; }
		}

		internal void ColumnStartedEditing()
		{
			((DataGridTextBox)TextBox).IsInEditOrNavigateMode = false;
			ColumnStartedEditing(TextBox);
		}

		#region Commit / Abort / Edit

		protected override bool Commit(CurrencyManager dataSource, int rowNum)
		{
			sourceData = dataSource;

			if (dataSource != null && dataSource.Position == rowNum)
			{
				var textBox = (DataGridTextBox)TextBox;
				if (!textBox.IsInEditOrNavigateMode && !textBox.ReadOnly && !IsCurrentCellReadOnly)
				{
					var valueAsString = (string.IsNullOrEmpty(TextBox.Text) || TextBox.Text == "-") ? "0" : TextBox.Text;

					object value;
					try
					{
						value = PropertyDescriptor.Converter.ConvertFrom(Core.Parse(valueAsString));
						if (value is ZDecimal decimalValue)
						{
							value = decimalValue.Round(Decimals);
						}
					}
					catch (Exception ex1) when (!ex1.IsCriticalException())
					{
						try
						{
							value = PropertyDescriptor.Converter.ConvertFromString(valueAsString);
							if (value is ZDecimal decimalValue)
							{
								value = decimalValue.Round(Decimals);
							}
						}
						catch (FormatException)
						{
							EndEdit();
							return true;
						}
						catch (OverflowException)
						{
							EndEdit();
							return true;
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							Enterprise.ZArchitecture.Environment.Globals.Message.ShowDeveloperExceptionOnce("ZCalcEditColumnStyle.Commit.ConvertFromString", "", ex);
							return true;
						}
					}

					if (value == null)
					{
						throw new InvalidOperationException("object \"Value\" should not be equal to null.");
					}
					SetColumnValueAtRow(dataSource, rowNum, value);
					var formattableValue = value as IFormattable;
					TextBox.Text = formattableValue.ToString(Format, Enterprise.ZArchitecture.Core.Culture.CurrentCompanyCountryCulture);

					EndEdit();
				}
			}
			return true;
		}

		protected override void Abort(int rowNum)
		{
			if (sourceData != null && rowNum < sourceData.Count)
			{
				var formattableValue = GetColumnValueAtRow(sourceData, rowNum) as IFormattable;
				if (formattableValue != null)
				{
					TextBox.Text = formattableValue.ToString(Format, null);
				}
				else
				{
					Enterprise.ZArchitecture.Environment.Globals.Message.ShowDeveloperErrorOnce(MappingName + "AllowsNull", MappingName + " should not allow nulls in a numeric column.", MappingName + " should not allow nulls in a numeric column.");
					TextBox.Text = "";
				}
			}
			else
			{
				TextBox.Text = "";
			}
		}

		protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText, bool cellVisible)
		{
			if (instantText != null && instantText.Any(c => !IsValidChar(c)))
			{
				return;
			}

			var bo = source.Position > -1 ? source.GetCurrent() as BusinessObject : null;
			if (bo != null)
			{
				Core.Decimals = GetDecimalPlacesForCurrent(bo);
			}

			Format = GetFormatString();
			Core.SetBindToType(PropertyDescriptor.PropertyType);

			base.Edit(source, rowNum, bounds, readOnly, instantText, cellVisible);

			if (parentZGrid.IsColumnStartingEdit)
			{
				if (ShouldRestoreText)
				{
					TextBox.Text = ZCalcEditCore.LeadingDecimalReplacement;
					TextBox.SelectionStart = ZCalcEditCore.LeadingDecimalReplacement.Length;
				}
#if !WINZOR
				else
				{
					TextBox.SelectionStart = 0;
					TextBox.SelectionLength = TextBox.Text.Length;
				}
#endif
			}
		}

		#endregion

		#region Implementation

		protected override string FormatValueObjectCore(object source, object propertyValue)
		{
			if (propertyValue == null || Equals(string.Empty, propertyValue))
			{
				propertyValue = ZInt.Zero;
			}

			var bo = source as BusinessObject;
			if ((bo == null) || !((propertyValue is ZDecimal) || (propertyValue is ZInt) || (propertyValue is ZShort) || (propertyValue is ZByte) || (propertyValue is ZLong)))
			{
				//string message = string.Format("The calculated property {0} <{1}> does not use a ZType. Type = " + value.GetType().FullName, ColumnInfo.ColumnName, value);
				//throw new ArgumentException(message, "value");
				return propertyValue.ToString();
			}

			return Core.Format(propertyValue, GetDecimalPlacesForCurrent(bo));
		}

		protected internal int GetDecimalPlacesForCurrent(BusinessObject current)
		{
			if (ColumnIsInteger)
			{
				return 0;
			}

			var result = -1;
			if (!CalcEditColumnInfo.DecimalsOverridden)
			{
				if (current != null && ColumnProperty != null)
				{
					result = MetaData.GetDecimalPlaces(current, ColumnProperty);
				}
				if (result < 0 && ColumnSchema != null)
				{
					result = ColumnSchema.Scale;
				}
			}
			if (result < 0)
			{
				result = Core.Decimals;
			}

			if (current != null && !current.IsDeleted && !string.IsNullOrEmpty(CalcEditColumnInfo.BindToDecimalPlaces))
			{
				result = ((INumericZType)current[CalcEditColumnInfo.BindToDecimalPlaces]).ToZInt();
			}

			return result;
		}

		#region ColumnSchema and PropertyDescriptor

		void CacheValues()
		{
			if (!cached && ParentZGrid != null)
			{
				var keyCalculator = new ResourceStringKeyCalculator(ParentZGrid, MappingName);
				if (!string.IsNullOrEmpty(keyCalculator.FinalPropertyTableName))
				{
					var column = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(keyCalculator.PropertyName, keyCalculator.FinalPropertyTableName);
					columnSchema = column as SchemaDecimalColumn;
					columnIsInteger = columnSchema == null && column is SchemaNumericColumn;
				}

				columnProperty = ZCustomTypeDescriptor.GetProperties(ParentZGrid.ElementTypeFromCollection)[MappingName];
				if (!columnIsInteger && columnProperty != null)
				{
					columnIsInteger = typeof(INumericZType).IsAssignableFrom(columnProperty.PropertyType) && columnProperty.PropertyType != typeof(ZDecimal);
				}

				cached = true;
			}
		}
		bool cached;

		SchemaDecimalColumn ColumnSchema
		{
			get
			{
				CacheValues();
				return columnSchema;
			}
		}
		SchemaDecimalColumn columnSchema;

		PropertyDescriptor ColumnProperty
		{
			get
			{
				CacheValues();
				return columnProperty;
			}
		}
		PropertyDescriptor columnProperty;

		bool ColumnIsInteger
		{
			get
			{
				CacheValues();
				return columnIsInteger;
			}
		}
		bool columnIsInteger;

		#endregion

		protected string GetFormatString()
		{
			return (Core.Decimals > 0) ? "0.".PadRight(Core.Decimals + 2, '0') : "0";
		}

		static bool IsDecimalSeparatorCharacter(char @char)
		{
			foreach (var c in Enterprise.ZArchitecture.Core.Culture.Current.NumberFormat.NumberDecimalSeparator)
			{
				if (c == @char)
				{
					return true;
				}
			}
			return false;
		}

		bool IsValidChar(char c)
		{
			return Char.IsDigit(c) || IsDecimalSeparatorCharacter(c) || c == '-';
		}

		void TextBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			parentZGrid.IgnoreNextKeyStroke = !(IsValidChar(e.KeyChar));
		}

		void TextBox_Leave(object sender, EventArgs e)
		{
			ShouldRestoreText = (TextBox.Text == ZCalcEditCore.LeadingDecimalReplacement);
		}

		void TextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Insert && TextBox.ReadOnly)
			{
				e.SuppressKeyPress = true;
				e.Handled = true;
			}
		}

		bool ShouldRestoreText;

		#endregion

		#region Hotkeys

		public override string TypeNameForDisplay => Res.GetString("652caa14-7ae2-4892-b79d-7bd4eb694550", "Number Field");

		void RegisterHotkeys()
		{
			Hotkeys.RegisterHotKey(Keys.F4, ShowCalculator, Res.GetString("a8e19518-48c3-4491-aa4d-60844d457d16", "Show calculator (if enabled)"));
			Hotkeys.RegisterHotKey(Keys.F5, ShowUnitConverter, Res.GetString("fe215a4a-6a4b-405e-bd7d-c6d5836c728a", "Show unit converter (if enabled)"));
#if !WINZOR
			Hotkeys.RegisterHotKey(Keys.Control | Keys.V, () =>
			{
				if (InitializeDataGridInColumn)
				{
					TextBox.Paste();
				}
			}
			);
#endif
		}

		bool CanModifyData => !ReadOnly && !TextBox.ReadOnly && DataSource != null;

		bool ShowCalculator(object sender, Keys keyData)
		{
			if (CanModifyData && Core.IsCalculatorEnabled)
			{
				Core.Calculator.PopUp();
				Core.Calculator.Owner = ParentZGrid?.FindForm();
				return true;
			}
			return false;
		}

		bool ShowUnitConverter(object sender, Keys keyData)
		{
			if (CanModifyData)
			{
				Core.ShowUnitConverter(DataSource, ColumnInfo.ColumnName);
				return true;
			}
			return false;
		}

		object DataSource => ParentZGrid?.ListManager?.GetCurrent();

		#endregion

		#region IPastableControl Members

#if !WINZOR

		public bool TryPaste()
		{
			ColumnStartedEditing(TextBox);
			((DataGridTextBox)TextBox).IsInEditOrNavigateMode = false;
			Core.Paste();
			TextBox.SelectionStart = TextBox.Text.Length;
			TextBox.SelectionLength = 0;
			return true;
		}

#endif

		#endregion

		#region Paste

#if !WINZOR

		bool ShouldPassThroughTo(Keys key) => key == (Keys.Control | Keys.V);

		public override bool ShouldProcessCmdKey(ref Message m, Keys keyData)
		{
			return base.ShouldProcessCmdKey(ref m, keyData) || ShouldPassThroughTo(keyData);
		}

		public override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == (Keys.Control | Keys.V) && CalcEditColumnInfo.MaxValue != 0)
			{
				if (decimal.TryParse(SafeClipboard.GetText(), out var value) && value > CalcEditColumnInfo.MaxValue)
				{
					var dataGridTextBox = (DataGridTextBox)TextBox;
					var wasInEditOrNavigateMode = dataGridTextBox.IsInEditOrNavigateMode;

					TextBox.Text = CalcEditColumnInfo.MaxValue.ToString(CultureInfo.CurrentCulture);
					dataGridTextBox.IsInEditOrNavigateMode = false;
					try
					{
						Commit(sourceData, EditingRowNum);
					}
					finally
					{
						dataGridTextBox.IsInEditOrNavigateMode = wasInEditOrNavigateMode;
					}
					return true;
				}
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

#endif

		#endregion
	}

	#endregion
}
