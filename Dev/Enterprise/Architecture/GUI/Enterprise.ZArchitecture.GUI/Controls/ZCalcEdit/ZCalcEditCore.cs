using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
#if WINZOR
using System.Collections.Generic;
using System.Globalization;
#endif
using System.Media;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture
{
	public class ZCalcEditCore : IDisposable
	{
		#region Initialization

		public ZCalcEditCore(TextBox editor)
		{
			TextBox = editor;

			TextBox.KeyPress += HandleKeyPress;
			TextBox.Validating += ValidationHandler;

			#if WINZOR
			TextBox.MatchExpression = GetMatchExpression();
			TextBox.ReplacementCharacters = CurrencySeparator != null ? new Dictionary<char, char>() { { '.', CurrencySeparator[0] } } : null;
			#endif

			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public ZCalcEditCore(TextBox editor, int decimals, decimal maxValue = 0)
			: this(editor)
		{
			Decimals = decimals;
			MaxValue = maxValue;
		}

		public ZCalcEditCore(TextBox editor, ZCalcEditColumnStyle columnStyle)
			: this(editor, columnStyle.CalcEditColumnInfo.Decimals, columnStyle.CalcEditColumnInfo.MaxValue)
		{
			this.columnStyle = columnStyle;
			columnInfo = columnStyle.CalcEditColumnInfo;
		}

		public TextBox TextBox { get; private set; }

		readonly ZCalcEditColumnStyleInfo columnInfo;
		readonly ZCalcEditColumnStyle columnStyle;

		#endregion

		#region Properties

		#region MatchExpression

		#if WINZOR
		public string GetMatchExpression()
		{
			var allowNegative = TextBox is ZCalcEdit calcEdit && !calcEdit.AllowNegative;
			if (Decimals == 0 && !allowNegative) {
				return @"[^0-9]";
			} else if (Decimals == 0 && allowNegative) {
				return @"[^0-9-]";
			} else if (Decimals > 0 && !allowNegative) {
				return @"[^0-9,.]";
			} else {
				return @"[^0-9,.-]";
			}
		}
		#endif

		#endregion

		#region Decimals

		public virtual int Decimals
		{
			get { return decimals; }
			set
			{
				if (value <= 9)
				{
					if (decimals != value)
					{
						decimals = value;
						if (updateTextOnDecimalsChanged)
						{
							TextBox.Text = Format(CalcValue);
						}
						OnDecimalsChanged();
					}
				}
				else
				{
					throw new FormatException("Cannot have more than 9 decimal places");
				}
			}
		}
		int decimals = DefaultDecimals;

		public const int DefaultDecimals = 2;

		public event EventHandler DecimalsChanged;

		void OnDecimalsChanged()
		{
			if (DecimalsChanged != null)
			{
				DecimalsChanged(this, EventArgs.Empty);
			}
		}

		protected internal bool UpdateTextOnDecimalsChanged
		{
			get { return updateTextOnDecimalsChanged; }
			set { updateTextOnDecimalsChanged = value; }
		}

		bool updateTextOnDecimalsChanged = true;

		#endregion

		#region Allow Null

		public bool AllowNull
		{
			get { return allowNull; }
			set { allowNull = value; }
		}
		bool allowNull = true;

		#endregion

		#region CalcValue

		public object CalcValue
		{
			get { return Parse(TextBox.Text); }
			set
			{
				var formattedValue = Format(value);
				if (formattedValue != TextBox.Text)
				{
					TextBox.Text = formattedValue;
				}
			}
		}

		#endregion

		#region Calculator

		public bool IsCalculatorCreated
		{
			get { return calculator != null; }
		}

		public virtual ZCalculator Calculator
		{
			get
			{
				if (calculator == null)
				{
					calculator =
						new ZCalculator(this)
						{
							ShowExtraButtons = showExtraButtons,
							Extra1LabelText = extra1LabelText,
							Extra2LabelText = extra2LabelText
						};
					calculator.CalculationComplete += Calculator_CalculationComplete;
				}

				return calculator;
			}
		}

		public void UpdateCalculatorTitle()
		{
			if (calculator != null)
			{
				calculator.UpdateText();
			}
		}

		protected void Calculator_CalculationComplete(object sender, ZCalculator.CalculationCompleteEventArgs e)
		{
			if (IsInGrid)
			{
				columnStyle.ColumnStartedEditing();
			}
			var valueToSet = IsInGrid ? Utilities.Round(e.Value, columnStyle.Decimals) : e.Value;
			TextBox.Text = valueToSet.ToString(Culture.CurrentCompanyCountryCulture);
		}

		protected ZCalculator calculator;

		#region Calculator Properties

		public bool IsCalculatorEnabled
		{
			get { return calculatorEnabled; }
			set { calculatorEnabled = value; }
		}

		public string BindToExtra1
		{
			get { return bindToExtra1; }
			set { bindToExtra1 = value; }
		}

		public string BindToExtra2
		{
			get { return bindToExtra2; }
			set { bindToExtra2 = value; }
		}

		public bool ShowExtraButtons
		{
			get { return (calculator == null) ? showExtraButtons : calculator.ShowExtraButtons; }
			set
			{
				if (calculator == null)
				{
					showExtraButtons = value;
				}
				else
				{
					calculator.ShowExtraButtons = value;
				}
			}
		}

		public string Extra1LabelText
		{
			get { return (calculator == null) ? extra1LabelText : calculator.Extra1LabelText; }
			set
			{
				if (calculator == null)
				{
					extra1LabelText = value;
				}
				else
				{
					calculator.Extra1LabelText = value;
				}
			}
		}

		public string Extra2LabelText
		{
			get { return (calculator == null) ? extra2LabelText : calculator.Extra2LabelText; }
			set
			{
				if (calculator == null)
				{
					extra2LabelText = value;
				}
				else
				{
					calculator.Extra2LabelText = value;
				}
			}
		}

		bool calculatorEnabled = true;
		protected bool showExtraButtons;
		string bindToExtra1 = "";
		string bindToExtra2 = "";
		protected string extra1LabelText = "";
		protected string extra2LabelText = "";

		#endregion

		#endregion

		#region UnitConversion

		internal void ShowUnitConverter(object dataSource, string propertyName)
		{
			if (dataSource == null)
			{
				return;
			}

			var dataSourceBO = dataSource as BusinessObject;

			if (dataSourceBO != null)
			{
				UnitConversionHelper.ShowMeasureUnitConversion(dataSourceBO, propertyName);
			}
			else
			{
				throw new ArgumentException(string.Format("DataSource must be BusinessObject, DataSoureType : {0}", dataSource.GetType()?.FullName));
			}
		}

		UnitConversionHelper UnitConversionHelper
		{
			get { return unitConversionHelper ?? (unitConversionHelper = new UnitConversionHelper(this)); }
		}

		UnitConversionHelper unitConversionHelper;

		#endregion

		#region Show Group Separators?

		bool ShowGroupSeparators
		{
			get
			{
				var result = true;

				if (IsInGrid)
				{
					result = columnInfo.ShowGroupSeparators;
				}
				else
				{
					var calcEdit = TextBox as ZCalcEdit;
					if (calcEdit != null)
					{
						result = calcEdit.ShowGroupSeparators;
					}
				}

				return result;
			}
		}

		internal bool IsInGrid
		{
			get { return (columnInfo != null); }
		}

		#endregion

		#region ShowEmptyStringForEmptyValue
		bool ShowEmptyStringForEmptyValue
		{
			get
			{
				var result = false;

				if (IsInGrid)
				{
					result = columnInfo.ShowEmptyStringForEmptyValue;
				}
				else
				{
					var calcEdit = TextBox as ZCalcEdit;
					if (calcEdit != null)
					{
						result = calcEdit.ShowEmptyStringForEmptyValue;
					}
				}

				return result;
			}
		}
		#endregion

		#region Friendly Column Name

		public virtual string FriendlyColumnName
		{
			get
			{
				var columnName = string.Empty;
				var tableName = string.Empty;

				if (IsInGrid)
				{
					if (columnStyle != null)
					{
						columnName = columnStyle.ColumnCaption;
						if (string.IsNullOrEmpty(columnName))
						{
							var grid = columnStyle.ParentZGrid;
							var current = grid.ListManager.GetCurrent() as BusinessObject;
							if (grid != null && grid.ListManager != null && current != null)
							{
								// columnInfo cannot be null when IsInGrid == true
								columnName = columnInfo.ColumnName;
								tableName = current.TableName;
							}
						}
					}
				}
				else if (TextBox != null)
				{
					var dataBinding = TextBox.DataBindings["Text"];
					if (dataBinding != null)
					{
						var bmb = dataBinding.BindingManagerBase;
						var current = bmb.GetCurrent() as BusinessObject;
						if (bmb != null && current != null)
						{
							var dataBoundControl = DataBoundControl.Get(TextBox);
							if (dataBoundControl != null)
							{
								columnName = new KBindingMemberInfo(dataBoundControl.DataMember).BindingField;
							}
							tableName = current.TableName;
						}
					}
				}

				if (string.IsNullOrEmpty(tableName))
				{
					return columnName;
				}
				else
				{
					return DataBoundResourceStrings.GetColumnDescriptiveName(tableName, columnName);
				}
			}
		}

		#endregion

		#region Min & Max Values

		internal protected decimal? MinValue
		{
			get
			{
				if (minValue == 0 && BindToType != null)
				{
					if (BindToType == typeof(ZDecimal) || BindToType == typeof(ZDecimal?))
					{
						minValue = decimal.MinValue;
					}
					else
					{
						var valueRangeAttribute = BindToType.GetCustomAttributes(typeof(ValueRangeAttribute), true);
						if (valueRangeAttribute.Length > 0)
						{
							minValue = ((ValueRangeAttribute)valueRangeAttribute[0]).MinValue;
						}
						else
						{
							throw new NotSupportedException("ZCalcEditCore does not support the type <" + BindToType.FullName + ">.");
						}
					}
				}

				return minValue;
			}
		}
		decimal minValue;

		internal protected decimal MaxValue
		{
			get
			{
				if (maxValue == 0 && BindToType != null)
				{
					if (BindToType == typeof(ZDecimal) || BindToType == typeof(ZDecimal?))
					{
						maxValue = decimal.MaxValue;
					}
					else
					{
						var valueRangeAttributes = BindToType.GetCustomAttributes(typeof(ValueRangeAttribute), true);
						if (valueRangeAttributes.Length > 0)
						{
							maxValue = ((ValueRangeAttribute)valueRangeAttributes[0]).MaxValue;
						}
						else
						{
							throw new NotSupportedException("ZCalcEditCore does not support the type <" + BindToType.FullName + ">.");
						}
					}
				}

				return maxValue;
			}
			set
			{
				if (bindToType != null)
				{
					maxValue = 0; //to recalculate maximum for type
					maxValue = Math.Min(MaxValue, value); //so we can't have a max outside of type bounds
				}
				else
				{
					maxValue = value;
				}
			}
		}
		decimal maxValue;

		internal virtual void SetBindToType(Type type)
		{
			BindToType = type;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message")]
		protected virtual Type BindToType
		{
			get
			{
				if (bindToType == null)
				{
					var textBinding = TextBox.DataBindings["Text"];
					if (textBinding != null)
					{
						var bindToObject = (typeof(Binding).GetProperty("BindToObject", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(textBinding, null)) ?? (typeof(Binding).GetField("_bindToObject", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(textBinding));
						bindToType = (Type)bindToObject.GetType().GetProperty("BindToType", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(bindToObject, null);
					}
				}

				return bindToType;
			}

			set
			{
				if (value != null && value != typeof(ZDecimal?) && !typeof(INumericZType).IsAssignableFrom(value))
				{
					var parents = new ZStringBuilder();
					var parentControl = TextBox.Parent;
					while (parentControl != null)
					{
						if (!string.IsNullOrEmpty(parentControl.Name))
						{
							parents.Prepend(parentControl.Name);
						}

						parentControl = parentControl.Parent;
					}

					var additional = "\r\nName: " + TextBox.Name + "\r\nParents: " + parents.ToStringWithDelimiterBetweenAppends(".");

					throw new NotSupportedException("ZCalcEditCore does not support the type <" + value.FullName + ">." + additional);
				}

				bindToType = value;
			}
		}
		Type bindToType;

		#endregion

		#endregion

		#region Paste

#if !WINZOR

		internal bool Paste()
		{
			var dataObject = SafeClipboard.GetDataObject();
			var value = dataObject != null ? (string)dataObject.GetData(typeof(string)) : null;

			if (value != null)
			{
				if (double.TryParse(value, System.Globalization.NumberStyles.Number, null, out var _))
				{
					CalcValue = value;
					return true;
				}
			}
			return false;
		}

#endif

		#endregion

		#region Key Press

		protected string CurrencySeparator
		{
			get { return currencySeparator ?? (currencySeparator = Culture.CurrentCompanyCountryCulture.NumberFormat.NumberDecimalSeparator); }
		}
		string currencySeparator;

		internal static string LeadingDecimalReplacement
		{
			get { return leadingDecimalReplacement ?? (leadingDecimalReplacement = "0" + Culture.CurrentCompanyCountryCulture.NumberFormat.NumberDecimalSeparator); }
		}
		[ThreadStatic]
		static string leadingDecimalReplacement;

		protected void HandleKeyPress(object sender, KeyPressEventArgs e)
		{
			var textbox = sender as TextBox;
			if (textbox != null && textbox.ReadOnly)
			{
				e.Handled = true;
			}
			else if (!char.IsControl(e.KeyChar))
			{
				if (e.KeyChar == '.')
				{
					e.KeyChar = CurrencySeparator[0];
				}

				var existingTextBeforeSelection = ((ZString)TextBox.Text).SubstringSafe(0, TextBox.SelectionStart);
				var existingTextAfterSelection = ((ZString)TextBox.Text).SubstringSafe(TextBox.SelectionStart + TextBox.SelectionLength, TextBox.Text.Length - (TextBox.SelectionStart + TextBox.SelectionLength));
				var userInput = existingTextBeforeSelection + e.KeyChar + existingTextAfterSelection;

				var separatorPosition = userInput.IndexOf(CurrencySeparator);

				if (IsValidCharacter(existingTextBeforeSelection + existingTextAfterSelection, e.KeyChar))
				{
					if (separatorPosition == -1) // separator doesn't exist
					{
						if (userInput != "-")
						{
							e.Handled = !IsValidIntPart(userInput);
						}
					}
					else // separator exists
					{
						var hasNegativeSign = userInput.Length > 0 && userInput[0] == '-';
						var integerSubString = ((ZString)userInput.Replace("-", "")).SubstringSafe(0, hasNegativeSign ? separatorPosition - 1 : separatorPosition);
						var decimalSubString = ((ZString)userInput).SubstringSafe(separatorPosition + 1, userInput.Length - (separatorPosition + 1));

						if (integerSubString == "" && decimalSubString == "" && userInput.Replace("-", "") == CurrencySeparator)
						{
							e.Handled = true;
							if (hasNegativeSign)
							{
								TextBox.Text = "-" + LeadingDecimalReplacement;
								TextBox.SelectionStart = 3;
							}
							else
							{
								TextBox.Text = LeadingDecimalReplacement;
								TextBox.SelectionStart = 2;
							}
						}
						else
						{
							e.Handled = !IsValidIntPart(integerSubString) || !IsValidDecimalPart(decimalSubString);
						}
					}
				}
				else
				{
					e.Handled = true;
				}
			}
		}

		#endregion

		#region Format Number & Parse

		public string Format(object value, int decimalsToShow)
		{
			decimal? result = 0m;

			if (value == null || value == DBNull.Value)
			{
				result = null;
			}
			else
			{
				try
				{
					var culture = Culture.CurrentCompanyCountryCulture;

					string valueAsString;
					if (value is int)
					{
						valueAsString = ((int)value).ToString(culture);
					}
					else if (value is double)
					{
						valueAsString = ((double)value).ToString(culture);
					}
					else if (value is float)
					{
						valueAsString = ((float)value).ToString(culture);
					}
					else if (value is decimal)
					{
						valueAsString = ((decimal)value).ToString(culture);
					}
					else if (value is ZDecimal)
					{
						valueAsString = ((ZDecimal)value).ToString(null, culture);
					}
					else
					{
						valueAsString = value.ToString();
					}
					if (valueAsString.Length > 0)
					{
						result = Convert.ToDecimal(valueAsString, culture); // Value could be int/double/decimal/Z-type/string etc.
					}
				}
				catch (FormatException) { }
				catch (OverflowException) { }
			}

			return FormatNumber(result, decimalsToShow);
		}

		public string Format(object value)
		{
			return Format(value, Decimals);
		}

		protected string FormatNumber(object number, int decimalsToShow)
		{
			if (BindToType == typeof(ZDecimal?) && number == null)
			{
				return null;
			}

			// perhaps replace with BoundType is IZTypeIntegral?
			if (BindToType == typeof(ZLong) || BindToType == typeof(ZInt) || BindToType == typeof(ZShort) || BindToType == typeof(ZByte))
			{
				decimalsToShow = 0;
			}

			if (ShowEmptyStringForEmptyValue)
			{
				if (number != null && number is decimal numberDecimal && numberDecimal == 0m)
				{
					return ZString.Empty;
				}
			}

			return ShowGroupSeparators ?
				Utilities.FormatNumberNationalWithGroupSeparators(number, decimalsToShow) :
				Utilities.FormatNumberNational(number, decimalsToShow);
		}

		public decimal? Parse(object value)
		{
			var result = 0m;
			var valueAsString = value.ToString().Replace(Culture.CurrentCompanyCountryCulture.NumberFormat.NumberGroupSeparator, "");

			if (!decimal.TryParse(valueAsString, System.Globalization.NumberStyles.Any, Culture.CurrentCompanyCountryCulture, out result))
			{
				if (BindToType == typeof(ZDecimal?))
				{
					return null;
				}
				else if (valueAsString.Length > 0)
				{
					SystemSounds.Beep.Play();
				}
			}

			return result;
		}

		protected virtual void ValidationHandler(object sender, CancelEventArgs e)
		{
			if (!string.IsNullOrEmpty(TextBox.Text))
			{
				var number = FormatNumber(CalcValue, Decimals);

				number = CheckNumberCanBeNegative(number);

				if (TextBox.Text != number)
				{
					TextBox.Text = number;
				}

				if (columnStyle != null)
				{
					columnStyle.HideEditControl();
				}
			}
		}

		string CheckNumberCanBeNegative(string number)
		{
			if (TextBox is ZCalcEdit calcEdit &&
				!calcEdit.AllowNegative &&
				!string.IsNullOrEmpty(number) &&
				number.IndexOf('-') > -1)
			{
				number = number.Replace("-", "");
			}

			return number;
		}

		#endregion

		#region Is Valid Int / Decimal, Is Hyphen, Is Separator

		protected bool IsValidIntPart(string userInput)
		{
			var result = false;

			try
			{
				var inputValue = Convert.ToDecimal(string.IsNullOrEmpty(userInput) ? "0" : userInput, Culture.CurrentCompanyCountryCulture);
				result = (MaxValue == 0 || (inputValue >= MinValue && inputValue <= MaxValue));
			}
			catch (FormatException)
			{
			}
			catch (OverflowException)
			{
			}

			return result;
		}

		protected bool IsValidDecimalPart(string userInput)
		{
			return userInput.Length <= Decimals;
		}

		protected bool IsValidCharacter(string userInput, char c)
		{
			return char.IsDigit(c) || IsHyphenValid(userInput, c) || IsSeparatorValid(userInput, c);
		}

		protected bool IsHyphenValid(string userInput, char c)
		{
			var result = c == '-' && TextBox.SelectionStart == 0 && userInput.IndexOf("-") == -1;

			if (MinValue != MaxValue)
			{
				result = result && (MinValue < 0);
			}

			if (TextBox is ZCalcEdit calcEdit)
			{
				result = result && calcEdit.AllowNegative;
			}

			return result;
		}

		protected bool IsSeparatorValid(string userInput, char c)
		{
			var isCurrencySeparator = c.ToString() == CurrencySeparator;
			return isCurrencySeparator && userInput.IndexOf(CurrencySeparator) == -1 && Decimals > 0;
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			DisposableLeakListener.Instance.UnRegisterDisposable(this);
			if (calculator != null)
			{
				calculator.Dispose();
			}
		}

		#endregion
	}
}
