using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Sub-class of Binding that better supports TypeConverters.
	/// </summary>
	public class KBinding : Binding
	{
		public KBinding(string propertyName, object dataSource, string dataMember)
			: base(propertyName, dataSource, dataMember)
		{
		}

		public KBinding(string propertyName, object dataSource, string dataMember, bool formattingEnabled)
			: base(propertyName, dataSource, dataMember, formattingEnabled)
		{
		}

		public KBinding(string propertyName, object dataSource, string dataMember, bool formattingEnabled, DataSourceUpdateMode dataSourceUpdateMode)
			: base(propertyName, dataSource, dataMember, formattingEnabled, dataSourceUpdateMode)
		{
		}

		[DefaultValue(false)]
		public new bool FormattingEnabled
		{
			get { return base.FormattingEnabled; }
			set
			{
				base.FormattingEnabled = value;
				formattingEnabledValueManuallySet = value;
			}
		}
		bool? formattingEnabledValueManuallySet;

		public bool UseTypeConverters
		{
			get { return useTypeConverters; }
			set
			{
				useTypeConverters = value;
				useTypeConvertersValueManuallySet = value;
			}
		}
		bool useTypeConverters;
		bool? useTypeConvertersValueManuallySet;

		#region Format / Parse

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
		protected override void OnFormat(ConvertEventArgs e)
		{
			try
			{
				var originValue = e.Value;

				CheckConfigurationForNumericAndNullableTypes(e.Value);
				if (UseTypeConverters)
				{
					Format += Convert;
					base.OnFormat(e);
					Format -= Convert;
				}
				else
				{
					base.OnFormat(e);
				}

				if (!FormattingEnabled)
				{
					ReportErrorIfWouldOccursFormatException(originValue, e);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ReportFormattingError(ex, e);
				throw;
			}
		}

		protected override void OnBindingComplete(BindingCompleteEventArgs e)
		{
			if (e.Exception is Data.DatabaseUpgradeException)
			{
				System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(e.Exception)
					.Throw();
			}
			base.OnBindingComplete(e);
		}

		void ReportErrorIfWouldOccursFormatException(object originValue, ConvertEventArgs e)
		{
			// Try to report the subsequent FormatException here
			var successful = false;
			var value = e.Value;
			var propertyType = e.DesiredType;

			if (propertyType == typeof(object))
			{
				successful = true;
			}
			else if (value != null && (value.GetType().IsSubclassOf(propertyType) || value.GetType() == propertyType))
			{
				successful = true;
			}
			else if (TypeDescriptor.GetConverter((originValue != null) ? originValue.GetType() : typeof(object))?.CanConvertTo(propertyType) ?? false)
			{
				successful = true;
			}
			else if (originValue is IConvertible)
			{
				var convertedValue = System.Convert.ChangeType(originValue, propertyType, CultureInfo.CurrentCulture);
				if (convertedValue != null && (convertedValue.GetType().IsSubclassOf(propertyType) || convertedValue.GetType() == propertyType))
				{
					successful = true;
				}
			}

			if (!successful)
			{
				ReportFormattingError(null, e);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
		protected override void OnParse(ConvertEventArgs e)
		{
			try
			{
				CheckConfigurationForNumericAndNullableTypes(e.Value);
				if (UseTypeConverters)
				{
					Parse += Convert;
					base.OnParse(e);
					Parse -= Convert;
				}
				else
				{
					base.OnParse(e);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ReportFormattingError(ex, e);
				throw;
			}
		}

		void ReportFormattingError(Exception ex, ConvertEventArgs e)
		{
			string dataMember = BindingMemberInfo.BindingMember;
			string errorText = "KBinding Formatting Error" + "\r\n" +
					"DataSource    : " + DataSource + "\r\n" +
					"BindingMember : " + dataMember + "\r\n" +
					"PropertyName  : " + PropertyName + "\r\n" +
					"DesiredType   : " + e.DesiredType.FullName + "\r\n" +
					"Value         : " + e.Value ?? "NULL" + "\r\n";
			ErrorReporter.ReportOnce(errorText, ex);
		}

		#endregion

		#region Converting

		internal void Convert(object sender, ConvertEventArgs e)
		{
			Convert(e);
		}

		internal void Convert(ConvertEventArgs e)
		{
			if (isNumeric && e.Value != null && e.DesiredType == typeof(string) && NumericUtil.IsNumeric(e.Value.GetType()))
			{
				e.Value = NumericUtil.ToStringWithDecimalPlaces(e.Value, DecimalPlaces);
			}
			else
			{
				ConvertUsingTypeConverters(e);
				if (isNumeric && e.Value != null && NumericUtil.IsNumeric(e.Value.GetType()))
				{
					e.Value = NumericUtil.ChangeDecimalPlaces(e.Value, DecimalPlaces);
				}
			}
		}

		void ConvertUsingTypeConverters(ConvertEventArgs e)
		{
			if (e.Value != null && e.DesiredType != null && !(e.Value is DBNull) && !e.DesiredType.IsInstanceOfType(e.Value))
			{
				bool found = false;
				TypeConverter converter = GetConverter(e.Value.GetType());
				if (converter != null)
				{
					if (converter.CanConvertTo(e.DesiredType))
					{
						e.Value = converter.ConvertTo(e.Value, e.DesiredType);
						found = true;
					}
				}
				if (!found && e.DesiredType != null)
				{
					converter = GetConverter(e.DesiredType);
					if (converter != null)
					{
						if (converter.CanConvertFrom(e.Value.GetType()))
						{
							e.Value = converter.ConvertFrom(e.Value);
						}
					}
				}
			}
		}

		TypeConverter GetConverter(Type type)
		{
			for (int i = 0; i < Converters.Count; i++)
			{
				KeyValuePair<Type, TypeConverter> item = Converters[i];
				if (item.Key == type)
				{
					return item.Value;
				}
			}
			TypeConverter result = TypeDescriptor.GetConverter(type);
			Converters.Add(new KeyValuePair<Type, TypeConverter>(type, result));
			Converters.TrimExcess();
			return result;
		}

		List<KeyValuePair<Type, TypeConverter>> Converters
		{
			get { return converters ?? (converters = new List<KeyValuePair<Type, TypeConverter>>()); }
		}
		List<KeyValuePair<Type, TypeConverter>> converters;

		#endregion

		#region DecimalPlaces Meta-data

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1020:DontUseCurrencyManagerCurrentRule")]
		int DecimalPlaces
		{
			get
			{
				PropertyDescriptor property = TypeDescriptor.GetProperties(BindingManagerBase.Current)[BindingMemberInfo.BindingField];
				int result = MetaData.GetDecimalPlaces(BindingManagerBase.Current, property);
				return result;
			}
		}

		#endregion

		#region Implementation

		bool isNumeric;

		void CheckConfigurationForNumericAndNullableTypes(object value)
		{
			if (!configurationChecked)
			{
				Type valueType = null;
				if (value == null || value is ValueType)
				{
					PropertyDescriptor dataProperty = BindingManagerBase.GetItemProperties()[BindingMemberInfo.BindingField];
					valueType = dataProperty == null ? null : dataProperty.PropertyType;
				}
				else if (value != null)
				{
					valueType = value.GetType();
				}

				isNumeric = NumericUtil.IsNumeric(valueType);
				if (valueType != null && valueType.IsGenericType && Nullable.GetUnderlyingType(valueType) != null)
				{
					if (formattingEnabledValueManuallySet == false) // bool? is nullable
					{
						Exception ex = new InvalidOperationException("FormattingEnabled was explicitly set to false while the binding member is a Nullable<> type. To enable Nullable<> objects to be bound correctly, FormattingEnabled will be forced to 'true' and should not be set manually.");
						ErrorReporter.ReportOnce(ex.Message, ex);
					}
					if (useTypeConvertersValueManuallySet == false) // bool? is nullable
					{
						Exception ex = new InvalidOperationException("UseTypeConverters was explicitly set to false while the binding member is a Nullable<> type. To enable Nullable<> objects to be bound correctly, UseTypeConverters will be forced to 'true' and should not be set manually.");
						ErrorReporter.ReportOnce(ex.Message, ex);
					}
					FormattingEnabled = true;
					UseTypeConverters = true;
				}
				configurationChecked = true;
			}
		}
		bool configurationChecked;

		#endregion
	}
}
