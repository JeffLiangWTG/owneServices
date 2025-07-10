using System;

namespace Enterprise.DocumentEngine.FlexCelInterface
{
	class FormattedCellValue
	{
		public FormattedCellValue(object value, string format)
		{
			this.Value = value;
			this.Format = format;
		}

		public readonly object Value;
		public readonly string Format;

		public override string ToString()
		{
			if (Value != null)
			{
				System.Reflection.MethodInfo methodToStringWithFormat = Value.GetType().GetMethod("ToString", new Type[] { typeof(string) });
				if (methodToStringWithFormat != null && !String.IsNullOrEmpty(Format))
				{
					return (string)methodToStringWithFormat.Invoke(Value, new object[] { Format });
				}

				string textValue = Value.ToString();
				if (textValue.StartsWith("=") && !string.IsNullOrEmpty(Format))
				{
					var startIndex = Format.IndexOf('.');
					var numberOfDecimal = 0;
					if (startIndex > -1)
					{
						numberOfDecimal = Format.Length - startIndex - 1;
					}
					textValue = textValue.Insert(1, "ROUND(") + "," + numberOfDecimal + ")";
				}
				return textValue;
			}

			return String.Empty;
		}
	}
}
