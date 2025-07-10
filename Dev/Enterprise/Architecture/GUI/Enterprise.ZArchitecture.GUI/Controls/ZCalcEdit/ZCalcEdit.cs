using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Media;
using System.Text;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Notifications;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	[ToolboxItem(true)]
	public class ZCalcEdit : ZTextBox, IPastableControl
	{
		#region Bare

		[ToolboxItem(false)]
		public new class Bare : ZCalcEdit
		{
			protected override IControlExtensionCollection NewExtensionCollection()
			{
				return new ControlExtensionCollection(this);
			}
		}

		#endregion

		#region Custom Adornment Layout

		class ZCalcEditAdornmentLayout : AdornmentLayout<ZCalcEdit>
		{
			public override IEnumerable<Control> GetBackroundAdornmentTargets(ZCalcEdit source)
			{
				yield return source;
			}

			public override IEnumerable<IIconLayout> GetIconAdornmentTargets(ZCalcEdit source)
			{
				yield return new IconLayout(source, IconAlignment.Left);
			}
		}

		#endregion

		#region Constructors

		static ZCalcEdit()
		{
			NotificationAdornmentFactory.RegisterCustomLayout(new ZCalcEditAdornmentLayout());
		}

		public ZCalcEdit()
		{
			TextAlign = HorizontalAlignment.Right;
			DisposableLeakListener.Instance.RegisterDisposable(this);
			RegisterHotkeys();

			TrackDisposedAccess = true;
		}

		#endregion

		#region Properties

		[Category(ZGUIConstants.DesignerCategory)]
		[DefaultValue(ZCalcEditCore.DefaultDecimals)]
		public int Decimals
		{
			get { return Core.Decimals; }
			set
			{
				if (!IsSettingBindToDecimalPlaces && DesignModeFinder.IsDesigning && !string.IsNullOrEmpty(BindToDecimalPlaces))
				{
					throw new ArgumentException("You cannot set decimals if using the BindToDecimalPlaces property. Clear BindToDecimalPlaces if you want to hard-code Decimals here.");
				}

				if (value > 9)
				{
					var errorMessageBuilder = new StringBuilder();
					errorMessageBuilder.AppendLine((NoResString)"ZCalcEditCore Cannot Have More Than 9 Decimal Places");
					errorMessageBuilder.AppendLine($"Top Level Control type: {TopLevelControl?.GetType()}");
					errorMessageBuilder.AppendLine($"Top Level Control Heading: {TopLevelControl?.Text}");
					errorMessageBuilder.AppendLine($"Top Level Control Data Source: {(TopLevelControl as IDataBoundControl)?.DataSource}");
					errorMessageBuilder.AppendLine($"Parent Control: {Parent?.GetType()}");
					errorMessageBuilder.AppendLine($"Binding Member: {BindTo}");
					errorMessageBuilder.AppendLine($"Control Type: {GetType()}");
					errorMessageBuilder.AppendLine($"Control Name: {Name}");
					errorMessageBuilder.AppendLine($"BindToDecimals property name: {BindToDecimalPlaces}");
					errorMessageBuilder.AppendLine($"value: {value}");
					ErrorReporter.ReportOnce("ZCalcEditCoreCannotHaveMoreThan9DecimalPlaces", errorMessageBuilder.ToString());
					Core.Decimals = 9;
				}
				else
				{
					Core.Decimals = value;
				}

				decimalsOverridden = true;
			}
		}

		bool decimalsOverridden;
		bool decimalsGotFromMetaData;

		[Browsable(false)]
		public override int DecimalPlaces
		{
			get { return Decimals; }
			set
			{
				if (!decimalsOverridden)
				{
					Decimals = value;
					decimalsGotFromMetaData = true;
					decimalsOverridden = false;
				}
			}
		}

		public event EventHandler DecimalsChanged
		{
			add { Core.DecimalsChanged += value; }
			remove
			{
				if (CoreIsCreated)
				{
					Core.DecimalsChanged -= value;
				}
			}
		}

		/// <summary>
		/// Keep at 0 for 'use the maximum value of the numerical type of the calc edit core'.
		/// </summary>
		[DefaultValue(typeof(decimal), "0")]
		public decimal MaxValue
		{
			get { return Core.MaxValue; }
			set { Core.MaxValue = value; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public object CalcValue
		{
			get { return Core.CalcValue; }
			set { Core.CalcValue = value; }
		}

		[DefaultValue(true)]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool AllowNegative
		{
			get { return allowNegative; }
			set { allowNegative = value; }
		}

		bool allowNegative = true;

		#endregion

		#region ShowEmptyStringForEmptyValue
		[Category(ZGUIConstants.DesignerCategory)]
		[DefaultValue(false)]
		public bool ShowEmptyStringForEmptyValue { get; set; }
		#endregion

		#region Show Group Separators?

		[Category(ZGUIConstants.DesignerCategory)]
		[DefaultValue(true)]
		public bool ShowGroupSeparators
		{
			get { return fShowGroupSeparators; }
			set { fShowGroupSeparators = value; }
		}

		bool fShowGroupSeparators = true;

		#endregion

		#region ZDecimal + ZInt + ZShort + ZByte Support

		void TextBinding_Parse(object sender, ConvertEventArgs e)
		{
			// remove group separator for parsing
			var valueWithoutSeparators = e.Value.ToString().Replace(Culture.CurrentCompanyCountryCulture.NumberFormat.NumberGroupSeparator, "");

			if (e.DesiredType == typeof(ZDecimal))
			{
				e.Value = StringToZDecimal(valueWithoutSeparators);
			}
			else if (e.DesiredType == typeof(ZDecimal?))
			{
				e.Value = StringToZDecimalNullAble(valueWithoutSeparators);
			}
			else if (e.DesiredType == typeof(ZLong))
			{
				e.Value = StringToZLong(valueWithoutSeparators);
			}
			else if (e.DesiredType == typeof(ZInt))
			{
				e.Value = StringToZInt(valueWithoutSeparators);
			}
			else if (e.DesiredType == typeof(ZShort))
			{
				e.Value = StringToZShort(valueWithoutSeparators);
			}
			else if (e.DesiredType == typeof(ZByte))
			{
				e.Value = StringToZByte(valueWithoutSeparators);
			}
			else
			{
				throw new NotSupportedException("The type you are binding to is not supported by the ZCalcEdit: <" + e.DesiredType.FullName + ">.");
			}
		}

		#region Implementation

		ZDecimal StringToZDecimal(object value)
		{
			decimal result;

			if (decimal.TryParse(value.ToString(), NumberStyles.Number, Culture.CurrentCompanyCountryCulture, out result))
			{
				return result;
			}
			else
			{
				SystemSounds.Beep.Play();
				return 0;
			}
		}

		ZDecimal? StringToZDecimalNullAble(object value)
		{
			decimal result;

			if (decimal.TryParse(value.ToString(), NumberStyles.Number, Culture.CurrentCompanyCountryCulture, out result))
			{
				return result;
			}

			return null;
		}

		ZLong StringToZLong(object value)
		{
			if (long.TryParse(value.ToString(), NumberStyles.Number, Culture.CurrentCompanyCountryCulture, out var result))
			{
				return result;
			}
			else
			{
				SystemSounds.Beep.Play();
				return 0L;
			}
		}

		ZInt StringToZInt(object value)
		{
			if (int.TryParse(value.ToString(), NumberStyles.Number, Culture.CurrentCompanyCountryCulture, out var result))
			{
				return result;
			}
			else
			{
				SystemSounds.Beep.Play();
				return 0;
			}
		}

		ZShort StringToZShort(object value)
		{
			if (short.TryParse(value.ToString(), NumberStyles.Number, Culture.CurrentCompanyCountryCulture, out var result))
			{
				return result;
			}
			else
			{
				SystemSounds.Beep.Play();
				return 0;
			}
		}

		ZByte StringToZByte(object value)
		{
			if (byte.TryParse(value.ToString(), NumberStyles.Number, Culture.CurrentCompanyCountryCulture, out var result))
			{
				return result;
			}
			else
			{
				SystemSounds.Beep.Play();
				return 0;
			}
		}

		#endregion

		#endregion

		#region Bind to Decimals

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindTo")]
		public string BindToDecimalPlaces
		{
			get { return fBindToDecimalPlaces; }
			set
			{
				fBindToDecimalPlaces = value;

				if (DesignModeFinder.IsDesigning && !string.IsNullOrEmpty(value))
				{
					IsSettingBindToDecimalPlaces = true;
					Decimals = ZCalcEditCore.DefaultDecimals;
					IsSettingBindToDecimalPlaces = false;
				}
			}
		}

		string fBindToDecimalPlaces = "";
		bool IsSettingBindToDecimalPlaces;

		#endregion

		#region Selecting text on Enter - Please do not modify without speaking to Geoff
		bool SelectAllViaMouse;
		protected override void OnEnter(EventArgs e)
		{
			SelectAll();

			base.OnEnter(e);

			if (!SelectAllViaMouse)
			{
				this.SelectAll();
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			if (SelectAllViaMouse)
			{
				this.SelectAll();
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			SelectAllViaMouse = false;
		}

		#region WndProc

#if !WINZOR

		const int WM_MOUSEMOVE = 0x200;
		const int WM_MOUSEACTIVATE = 0x21;

		protected override void WndProc(ref Message m)
		{
			if (!ReadOnly)
			{
				if (!(SelectAllViaMouse && m.Msg == WM_MOUSEMOVE)) // force user to click again before dragging + selecting text with the mouse
				{
					if (!this.Focused && m.Msg == WM_MOUSEACTIVATE)
					{
						SelectAllViaMouse = true;
					}

					base.WndProc(ref m);
				}
			}
			else
			{
				base.WndProc(ref m);
			}
		}

#endif

		#endregion

		#endregion

		#region Calculator/Converter

		#region GUI events

		protected virtual bool EnableConverterUnit { get; } = true;

		void RegisterHotkeys()
		{
			Hotkeys.RegisterHotKey(Keys.F4, ShowCalculatorHotkey, Res.GetString("8fc205d3-a5a9-4206-a8ea-ac7134e4cefb", "Show calculator"));

			if (EnableConverterUnit)
			{
				Hotkeys.RegisterHotKey(Keys.F5, ShowUnitConverterHotkey, Res.GetString("2c671f7b-65e7-40b4-8d49-bc9f6f30ed1d", "Show unit converter"));
			}
		}

#if !WINZOR

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			var maxValue = MaxValue;
			if (keyData == (Keys.Control | Keys.V) && maxValue != 0)
			{
				if (decimal.TryParse(SafeClipboard.GetText(), out var value) && value > maxValue)
				{
					Text = maxValue.ToString(CultureInfo.CurrentCulture);
					return true;
				}
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

#endif

		bool ShowCalculatorHotkey(object sender, Keys key)
		{
			if (!ReadOnly && IsCalculatorEnabled)
			{
				Core.Calculator.PopUp();
				Core.Calculator.Owner = FindForm();
				return true;
			}
			return false;
		}

#if DEBUG
		internal
#endif
		bool ShowUnitConverterHotkey(object sender, Keys key)
		{
			if (!ReadOnly)
			{
				Core.ShowUnitConverter(BindingManager.GetCurrent(), new KBindingMemberInfo(DataMember).BindingField);
				return true;
			}
			return false;
		}

		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);

			if (Core.IsCalculatorCreated)
			{
				Core.Calculator.Reset();
				Core.Calculator.SetCurrentValue(Text);
			}
		}

		#endregion

		#region Calculator Properties

		[Category(CalculatorDesignerCategory)]
		[DefaultValue(true)]
		public bool IsCalculatorEnabled
		{
			get { return Core.IsCalculatorEnabled; }
			set { Core.IsCalculatorEnabled = value; }
		}

		[Category(CalculatorDesignerCategory)]
		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindTo")]
		public string BindToExtra1
		{
			get { return Core.BindToExtra1; }
			set { Core.BindToExtra1 = value; }
		}

		[Category(CalculatorDesignerCategory)]
		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindTo")]
		public string BindToExtra2
		{
			get { return Core.BindToExtra2; }
			set { Core.BindToExtra2 = value; }
		}

		[Category(CalculatorDesignerCategory)]
		[DefaultValue(false)]
		public bool ShowExtraButtons
		{
			get { return Core.ShowExtraButtons; }
			set { Core.ShowExtraButtons = value; }
		}

		[Category(CalculatorDesignerCategory)]
		[DefaultValue("")]
		public string Extra1LabelText
		{
			get { return Core.Extra1LabelText; }
			set { Core.Extra1LabelText = value; }
		}

		[Category(CalculatorDesignerCategory)]
		[DefaultValue("")]
		public string Extra2LabelText
		{
			get { return Core.Extra2LabelText; }
			set { Core.Extra2LabelText = value; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		const string CalculatorDesignerCategory = ZGUIConstants.DesignerCategory + " Calculator";

		#endregion

		#endregion

		#region Dispose()

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				Extensions.Dispose();
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
				if (CoreIsCreated)
				{
					Core.Dispose();
				}
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion

		#region PropertyDescriptors

		public new static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZCalcEdit>()
				.Property("Text", "") // Property name
				.Property("ReadOnlyForBinding", false, false)
				.Property("ReadOnlyForBindingIsNull", false, false)
				.Property("Decimals", 0) // Programmatic constant
				.Result;
		}

		#endregion

		#region IDataBoundControl Members

		[BindingMetaDataProperty(MetaDataTypes.MaxLength, "MaxLength", Enabled = false)]
		public override string Text
		{
			get { return base.Text; }
			set { base.Text = value; }
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource != null)
			{
				// it is imperative that .Decimals be bound *before* .Text is bound (so that decimals is always pulled before format occurs)
				if (!string.IsNullOrEmpty(BindToDecimalPlaces))
				{
					BindDecimals(dataSource, BindToDecimalPlaces);
					decimalsOverridden = true;
				}
			}

			DataBindings.CollectionChanging += new CollectionChangeEventHandler(DataBindings_CollectionChanging);
			try
			{
				if (dataSource != null)
				{
					var keyCalculator = new ResourceStringKeyCalculator(this);
					if (!string.IsNullOrEmpty(keyCalculator.FinalPropertyTableName))
					{
						var columnSchema = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(keyCalculator.PropertyName, keyCalculator.FinalPropertyTableName);

						var decimalColumnSchema = columnSchema as SchemaDecimalColumn;
						if (decimalColumnSchema != null)
						{
							MaxLength = Math.Min(MaxLength, CalcMaxLength(decimalColumnSchema.Precision, decimalColumnSchema.Scale));

							if (!decimalsOverridden && !decimalsGotFromMetaData)
							{
								Core.Decimals = decimalColumnSchema.Scale;
							}
						}

						var property = TypeDescriptor.GetProperties(dataSource)[dataMember];
						var maxLength = MetaData.GetMaxLength(dataSource, property);
						if (maxLength > 0)
						{
							MaxLength = Math.Min(MaxLength, maxLength);
						}
					}
				}

				base.SetDataBinding(dataSource, dataMember);
			}
			finally
			{
				DataBindings.CollectionChanging -= new CollectionChangeEventHandler(DataBindings_CollectionChanging);
			}
		}

		int CalcMaxLength(int precision, int scale)
		{
			var leftHandDigits = precision - scale;
			//You can't type thousandths separator (, in AUS/US, . in NL, etc) but you can paste them in.
			//0 digits -> 0 length, 1 -> 1, 2 -> 2, 3 -> 3, 4 -> 5, 5 -> 6, 6 -> 7, 7 -> 9, etc
			var result = Math.Max((leftHandDigits * 4 - 1) / 3, 0);
			result += scale > 0 ? scale + 1 : 0;
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		void DataBindings_CollectionChanging(object sender, CollectionChangeEventArgs e)
		{
			var binding = e.Element as Binding;
			if (binding != null && binding.PropertyName == "Text")
			{
				binding.Parse -= TextBinding_Parse;
				binding.Parse += TextBinding_Parse;
				binding.Format -= CalcBinding_Format;
				binding.Format += CalcBinding_Format;
			}
		}

		protected virtual void BindDecimals(object dataSource, string dataMember)
		{
			DataBindings.RemoveBinding(nameof(Decimals)); // Programmatic constant
			if (dataSource != null)
			{
				Binding binding = new KBinding(nameof(Decimals), dataSource, dataMember);
				binding.Parse += DecimalPlacesBinding_Parse;
				binding.Format += DecimalPlacesBinding_Format;
				DataBindings.Add(binding);
			}
		}

		internal void BindDecimalsInternal(object dataSource, string dataMember)
		{
			BindDecimals(dataSource, dataMember);
		}

		protected virtual void DecimalPlacesBinding_Parse(object sender, ConvertEventArgs e)
		{
			int i;
			e.Value = int.TryParse(e.Value.ToString(), out i) ? i : 0;
			e.Value = (ZInt)e.Value;
		}

		protected virtual void DecimalPlacesBinding_Format(object sender, ConvertEventArgs e)
		{
			int i;
			e.Value = int.TryParse(e.Value.ToString(), out i) ? i : 0;
		}

		protected override Type DataSourceType
		{
			get { return typeof(INumericZType); }
		}

		#endregion

		#region IGridControl Member Overrides

		protected override bool ShouldHandleKey(Keys keyData)
		{
			return false;
		}

		#endregion

		#region IPastableControl Members

#if !WINZOR

		bool IPastableControl.TryPaste()
		{
			if (!this.ReadOnly)
			{
				return Core.Paste();
			}
			return false;
		}

#endif

		#endregion

		#region Implementation

		void CalcBinding_Format(object sender, ConvertEventArgs e)
		{
			e.Value = Core.Format(e.Value);
		}

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
			return new ZCalcEditCore(this);
		}

		protected override void SetVisibleCore(bool value)
		{
			base.SetVisibleCore(value);
			NotificationBroadcaster.Instance.BroadcastVisibilityChange(this);
		}

		#endregion
	}
}
