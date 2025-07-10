using System;
using System.ComponentModel;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Text box to contain numeric values.
	/// </summary>
	[DefaultProperty("Text"), ToolboxData("<{0}:ZNumericTextBox runat=server></{0}:ZNumericTextBox>")]
	public class ZNumericTextBox : ZTextBoxBase
	{
		#region IBindTo Members

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindTo")]
		public string BindToDecimals
		{
			get { return fBindToDecimals; }
			set
			{
				fBindToDecimals = value;

				if (!string.IsNullOrEmpty(value))
				{
					Decimals = ZCalcEditCore.DefaultDecimals;
				}
			}
		}

		protected string fBindToDecimals;

		protected override void BindCore(object dataSource)
		{
			if (!string.IsNullOrEmpty(BindToDecimals))
			{
				Decimals = int.Parse(ZPropertyAccessor.Get(dataSource, BindToDecimals).ToString());
			}
			ValueType = ZPropertyAccessor.GetPropertyType(dataSource, BindTo);
			base.BindCore(dataSource);
		}

		#endregion

		#region Properties

		#region Decimals

		[DefaultValue(2), Category("Appearance"), Description("The number of decimal places to display.")]
		public virtual int Decimals
		{
			get
			{
				object obj1 = this.ViewState["Decimals"];
				return (obj1 != null) ? (int)obj1 : DefaultDecimals;
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}
				this.ViewState["Decimals"] = value;
			}
		}

		#endregion Decimals

		#region SelectedValue

		[Browsable(false)]
		protected override IZType SelectedValue
		{
			get
			{
				return StringToValue(Text);
			}
			set
			{
				if (value is INumericZType)
				{
					string formattedValue = Format(value);

					if (formattedValue != Text)
					{
						Text = formattedValue;
					}
				}
				else
				{
					Text = "";
				}
			}
		}

		Type ValueType;

		#endregion SelectedValue

		public bool ValidateBindToDecimals
		{
			get
			{
				return validateBindToDecimals;
			}
			set
			{
				validateBindToDecimals = value;
			}
		}

		bool validateBindToDecimals;

		#endregion Properties

		#region Control Overrides

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			this.Style.Add("text-align", nameof(HorizontalAlign.Right));
		}

		protected override string ValidationMessage
		{
			get
			{
				if (ValueType == typeof(ZDecimal))
				{
					string msg = Res.GetString("b7d1ee64-3b02-4d56-9f0f-c2ff77641baa", "Please enter numeric characters only");
					if (ValidateBindToDecimals)
					{
						msg += " " + Res.GetString("9f0d0e8a-0e7c-453d-a892-285aea9fabc3", "(allowed number of decimal places is {0})", Decimals.ToString());
					}
					return msg;
				}
				else
				{
					return Res.GetString("41974f79-6c09-4f91-9083-85f7e8b5ec41", "Please enter a whole number");
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "regular expression pattern with only symbols")]
		protected override string ValidationPattern
		{
			get
			{
				if (ValueType == typeof(ZDecimal))
				{
					string pattern = string.Format("^\\\\d+\\\\{0}?\\\\d$", Shared.WebEnvShared.ClientCulture.NumberFormat.NumberDecimalSeparator);
					return pattern.Insert(pattern.Length - 1, ValidateBindToDecimals ? "{0," + Decimals.ToString() + "}" : "*");
				}
				else
				{
					return "^\\\\d+$";
				}
			}
		}

		protected override ZWebResource GetNewValidationScriptFile()
		{
			return new ZWebResource(typeof(ZNumericTextBox), "ZNumericTextBoxValidation.js", Page, "Enterprise.ZArchitecture.Web.GUI.WebControls.ZTextBox");
		}

		protected override string ValidationScriptKey
		{
			get { return "ZNumericTextBoxBase_ValidationScript"; }
		}

		#endregion Control Overrides

		#region String conversion routines

		INumericZType StringToValue(string valueWithSeparators)
		{
			string valueWithoutSeparators = valueWithSeparators.Replace(Shared.WebEnvShared.ClientCulture.NumberFormat.NumberGroupSeparator, string.Empty);
			INumericZType value;
			if (ValueType == typeof(ZDecimal))
			{
				value = StringToZDecimal(valueWithoutSeparators);
			}
			else if (ValueType == typeof(ZInt))
			{
				value = StringToZInt(valueWithoutSeparators);
			}
			else if (ValueType == typeof(ZShort))
			{
				value = StringToZShort(valueWithoutSeparators);
			}
			else if (ValueType == typeof(ZByte))
			{
				value = StringToZByte(valueWithoutSeparators);
			}
			else
			{
				throw new NotSupportedException("The type you are binding to is not supported by the ZNumericTextBox: <" + ValueType.FullName + ">.");
			}
			return value;
		}

		ZDecimal StringToZDecimal(string value)
		{
			double junk;
			decimal actualValue = 0;

			if (double.TryParse(value, NumberStyles.Number, Shared.WebEnvShared.ClientCulture, out junk))
			{
				try
				{
					actualValue = Convert.ToDecimal(value, Shared.WebEnvShared.ClientCulture);
				}
				catch (OverflowException)
				{
				}
			}
			return new ZDecimal(actualValue);
		}

		string RemoveLeadingZerosFromWholeNumberString(string value)
		{
			string result = value;
			int decimalPosition = value.IndexOf(".");
			if (decimalPosition >= 0)
			{
				var fraction = value.Substring(decimalPosition + 1);
				if ((string.IsNullOrEmpty(fraction)) || double.Parse(fraction) == 0)
				{
					result = value.Substring(0, decimalPosition);
				}
			}
			return result;
		}

		ZInt StringToZInt(string value)
		{
			double junk;
			int actualValue = 0;
			value = RemoveLeadingZerosFromWholeNumberString(value);
			if (double.TryParse(value, NumberStyles.Number ^ NumberStyles.AllowDecimalPoint, Shared.WebEnvShared.ClientCulture, out junk))
			{
				try
				{
					actualValue = Convert.ToInt32(value, Shared.WebEnvShared.ClientCulture);
				}
				catch (OverflowException)
				{
				}
			}
			return new ZInt(actualValue);
		}

		ZShort StringToZShort(string value)
		{
			double junk;
			short actualValue = 0;
			value = RemoveLeadingZerosFromWholeNumberString(value);
			if (double.TryParse(value, NumberStyles.Number ^ NumberStyles.AllowDecimalPoint, Shared.WebEnvShared.ClientCulture, out junk))
			{
				try
				{
					actualValue = Convert.ToInt16(value, Shared.WebEnvShared.ClientCulture);
				}
				catch (OverflowException)
				{
				}
			}
			return new ZShort(actualValue);
		}

		ZByte StringToZByte(string value)
		{
			double junk;
			byte actualValue = 0;

			if (double.TryParse(value, NumberStyles.Number ^ NumberStyles.AllowDecimalPoint, Shared.WebEnvShared.ClientCulture, out junk))
			{
				try
				{
					actualValue = Convert.ToByte(value, Shared.WebEnvShared.ClientCulture);
				}
				catch (OverflowException)
				{
				}
			}
			return new ZByte(actualValue);
		}

		string Format(object value)
		{
			decimal result = 0m;

			if (value != null && value != DBNull.Value) // DBNull check is not necessary but facilitates debugging when using break on exception
			{
				try
				{
					string valueAsString = value.ToString();
					if (valueAsString.Length > 0)
					{
						result = Convert.ToDecimal(valueAsString); // Value could be int/double/decimal/Z-type/string etc.
					}
				}
				catch (FormatException)
				{
					// leave result as 0
				}
				catch (OverflowException)
				{
					// leave result as 0
				}
			}

			//These are done to prevent rounding in web
			string res = Enterprise.ZArchitecture.Core.Utilities.FormatNumber(result, Decimals + 1, Shared.WebEnvShared.ClientCulture);
			res = Decimals == 0 ? res.Substring(0, res.Length - 2) : res.Substring(0, res.Length - 1);
			return res;
		}

		#endregion

		int DefaultDecimals
		{
			get { return (ValueType == typeof(ZDecimal)) ? 2 : 0; }
		}
	}
}
