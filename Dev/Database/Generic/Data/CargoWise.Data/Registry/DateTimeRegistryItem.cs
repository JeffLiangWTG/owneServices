using System;
using System.Globalization;
using System.Text;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Data
{
	[Immutable]
	internal sealed class DateTimeRegistryItem : BaseDbRegistryItem<DateTime>
	{
		public DateTimeRegistryItem(string name, bool preserveForTest = false)
			: this(name, DateTime.MinValue, preserveForTest)
		{
		}

		public DateTimeRegistryItem(string name, DateTime defaultValue, bool preserveTestValue = false)
			 : base(preserveTestValue)
		{
			ItemName = name ?? throw new ArgumentNullException(nameof(name));
			DefaultValue = defaultValue;
		}

		public override string ItemName { get; }

		protected override DateTime DefaultValue { get; }

		protected override string TypeCode => "DT";

		protected override DateTime GetValueFromBytes(byte[] binaryValue)
		{
			string valueAsString = Encoding.Unicode.GetString(binaryValue);
			DateTime result = DateTime.MinValue;

			if (!string.IsNullOrEmpty(valueAsString))
			{
				try
				{
					result = SqlFormatInfo.FromSqlDateTime(valueAsString);
				}
				catch (FormatException originalEx)
				{
					string[] possibleSeparators = new string[] { "/", "-", "." };
					const string templateFormat = "dd{0}MM{0}yyyy HH:mm:ss:fff"; // Format string
					bool converted = false;
					foreach (string separator in possibleSeparators)
					{
						try
						{
							string format = string.Format(templateFormat, separator);
							result = DateTime.ParseExact(valueAsString, format, CultureInfo.InvariantCulture);
							converted = true;
							break;
						}
						catch (FormatException)
						{
						}
					}

					if (!converted)
					{
						try
						{
							result = Convert.ToDateTime(valueAsString);
						}
						catch (FormatException)
						{
							throw new Exception(string.Format("Unable to parse DateTime registry value [{0}]", valueAsString), originalEx); // Developer Exception Message
						}
					}
				}
			}

			return result;
		}

		protected override byte[] GetBytesFromValue(DateTime value)
		{
			byte[] result = Encoding.Unicode.GetBytes(SqlFormatInfo.ToSqlDateTimeString(value));
			return result;
		}
	}
}
