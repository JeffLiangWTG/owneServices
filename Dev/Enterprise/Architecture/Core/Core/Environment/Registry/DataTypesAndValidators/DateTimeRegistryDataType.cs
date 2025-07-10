using System;
using System.Globalization;
using System.Text;
using CargoWise.Data;
using Enterprise.Integration;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment
{
	public class DateTimeRegistryDataType : RegistryDataType<DateTime>
	{
		public DateTimeRegistryDataType()
			: base(RegistryDataTypes.Codes.DateTime, DateTime.MinValue)
		{
		}

		public override bool IsDefaultValueImmutable => true;

		protected override bool HasDefaultEditorInfoCore
		{
			get { return false; }
		}

		protected override bool ValuesAreEqualCore(DateTime a, DateTime b)
		{
			return a == b;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Format string, Developer Exception Message")]
		protected override DateTime DeserialiseCore(byte[] value)
		{
			string valueAsString = Encoding.Unicode.GetString(value);
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
					const string templateFormat = "dd{0}MM{0}yyyy HH:mm:ss:fff";
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
						catch (FormatException) { }
					}

					if (!converted)
					{
						try
						{
							result = Convert.ToDateTime(valueAsString);
						}
						catch (FormatException)
						{
							throw new RegistryParsingException(string.Format("Unable to parse DateTime registry value [{0}]", valueAsString), originalEx);
						}
					}
				}
			}

			return result;
		}

		protected override void ValidateCore(IRegistryItem registryItem, DateTime proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
			if (proposedValue == DateTime.MinValue)
			{
				throw new RegistryValidationException(Res.GetString("7E60D3D6-FD49-4AC4-93B4-6922AFCF37E8", "Please select a valid date."));
			}
		}

		protected override byte[] SerialiseCore(DateTime value)
		{
			return Encoding.Unicode.GetBytes(SqlFormatInfo.ToSqlDateTimeString(value));
		}
	}
}
