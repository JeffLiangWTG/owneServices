using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Microsoft.SqlServer.Types;

namespace Enterprise.DataTransfer.Business
{
	public class FlatFileDataRow : ICloneable
	{
		protected FlatFileDataRow() { }

		public FlatFileDataRow(int fieldCount)
		{
			this.DataRow = new string[fieldCount];
		}

		public FlatFileDataRow(string[] dataRow)
		{
			this.DataRow = dataRow;
		}

		public FlatFileDataRow(FlatFileDataRow flatFileRow)
		{
			this.DataRow = flatFileRow.DataRow;
		}

		public ZString this[int position]
		{
			get { return GetField(position); }
			set { SetField(position, value); }
		}

		#region Set Methods

		public void SetField(int position, string value)
		{
			SetFieldCore(position, value);
		}

		public void SetField(int position, int value)
		{
			SetFieldCore(position, value.ToString());
		}

		public void SetField(int position, int value, string format)
		{
			SetFieldCore(position, value.ToString(format));
		}

		public void SetField(int position, decimal value)
		{
			SetFieldCore(position, value.ToString());
		}

		public void SetField(int position, decimal value, int placesToRoundTo)
		{
			ZDecimal result = Utilities.Round(value, placesToRoundTo);
			SetFieldCore(position, result.ToString(placesToRoundTo));
		}

		public void SetField(int position, ZDateTime value, string format)
		{
			SetFieldCore(position, value.ToString(format));
		}

		public void SetField(int position, DateTime value, string format)
		{
			SetField(position, new ZDateTime(value), format);
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Same as ZDateTime version")]
		public void SetField(int position, ZDateTimeOffset value, string format)
		{
			SetFieldCore(position, value.ToString(format));
		}

		public void SetField(int position, DateTimeOffset value, string format)
		{
			SetField(position, new ZDateTimeOffset(value), format);
		}

		public void SetField(int position, ZTime value, string format)
		{
			SetFieldCore(position, value.ToString(format));
		}

		public void SetField(int position, TimeSpan value, string format)
		{
			SetField(position, new ZTime(value), format);
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Same as ZDateTime version")]
		public void SetField(int position, ZGeography value)
		{
			SetFieldCore(position, value.ToString());
		}

		public void SetField(int position, SqlGeography value)
		{
			SetField(position, new ZGeography(value));
		}

		#endregion

		#region Get Values

		public ZString GetField(int position)
		{
			return GetFieldCore(position);
		}

		public ZString GetFieldAndTrim(int position)
		{
			return GetFieldCore(position).Trim();
		}

		public ZDateTime GetFieldAsZDateTime(int position, string format)
		{
			if (string.IsNullOrEmpty(format))
			{
				throw new ArgumentNullException(nameof(format));
			}

			ZDateTime result;
			if (!ZDateTime.TryParseExact(GetFieldCore(position), out result, format))
			{
				result = ZDateTime.Empty;
			}

			return result;
		}

		public ZDateTimeOffset GetFieldAsZDateTimeOffset(int position, string format)
		{
			if (string.IsNullOrEmpty(format))
			{
				throw new ArgumentNullException(nameof(format));
			}

			ZDateTimeOffset result;
			if (!ZDateTimeOffset.TryParseExact(GetFieldCore(position), out result, format))
			{
				result = ZDateTimeOffset.Empty;
			}

			return result;
		}

		public ZTime GetFieldAsZTime(int position, string format)
		{
			if (string.IsNullOrEmpty(format))
			{
				throw new ArgumentNullException(nameof(format));
			}

			ZTime result;
			if (!ZTime.TryParseExact(GetFieldCore(position), out result, format))
			{
				result = ZTime.Empty;
			}

			return result;
		}

		public ZGeography GetFieldAsZGeography(int position)
		{
			ZGeography result;
			if (!ZGeography.TryParse(GetFieldCore(position), out result))
			{
				result = ZGeography.Empty;
			}

			return result;
		}

		public ZDecimal GetFieldAsZDecimal(int position)
		{
			ZDecimal result = ZDecimal.Zero;

			ZString value = GetFieldCore(position);
			if (value != ZString.Empty)
			{
				ZDecimal.TryParse(value, out result);
			}

			return result;
		}

		public ZDecimal GetFieldAsZDecimal(int position, int decimals)
		{
			ZDecimal result = GetFieldAsZDecimal(position);
			result = Decimal.Round(result, decimals);
			return result;
		}

		public ZDecimal GetFieldAsZDecimal(int position, CultureInfo culture)
		{
			ZString value = GetFieldCore(position);
			if (!decimal.TryParse(value, NumberStyles.AllowDecimalPoint, culture.NumberFormat, out var decimalResult))
			{
				return ZDecimal.Zero;
			}
			return decimalResult;
		}

		public ZInt GetFieldAsZInt(int position)
		{
			ZInt result;
			ZInt.TryParse(GetFieldCore(position), out result);
			return result;
		}

		public ZShort GetFieldAsZShort(int position)
		{
			ZShort result;
			ZShort.TryParse(GetFieldCore(position), out result);
			return result;
		}

		public ZBool GetFieldAsZBool(int position)
		{
			ZBool result = false;

			var stringValue = GetFieldCore(position).Trim();
			if (!stringValue.IsEmpty && stringValue.Length == 1)
			{
				var charValue = stringValue.ToUpper()[0];
				if (charValue == 'Y' || charValue == 'N')
				{
					result = new ZBool(charValue);
				}
			}

			return result;
		}

		#endregion

		#region IsEmpty

		public bool IsEmpty
		{
			get
			{
				bool result = true;
				for (int i = 0; i < DataRow.Length; i++)
				{
					string field = DataRow[i];
					if (!IsFieldEmpty(field, i))
					{
						result = false;
						break;
					}
				}
				return result;
			}
		}

		protected virtual bool IsFieldEmpty(string value, int position)
		{
			return String.IsNullOrEmpty(value);
		}

		#endregion

		#region Core Methods

		protected virtual ZString GetFieldCore(int position)
		{
			ZString result = ZString.Empty;

			if (position < DataRow.Length)
			{
				result = DataRow[position];
			}

			return result;
		}

		protected virtual void SetFieldCore(int position, ZString value)
		{
			DataRow[position] = value;
		}

		#endregion

		#region ICloneable Members

		public FlatFileDataRow Clone()
		{
			return new FlatFileDataRow((string[])DataRow.Clone());
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		#endregion

		public virtual int FieldCount
		{
			get { return DataRow.Length; }
		}

		public virtual char WhiteSpace
		{
			get { return ' '; }
		}

		protected string[] DataRow;
	}
}
