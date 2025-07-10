using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public struct ZCodeMappedZString
	{
		public ZCodeMappedZString(ZString value)
		{
			SourceValue = value;
			MappedValue = null;
		}

		public ZString SourceValue;
		public ZString? MappedValue;

		public bool IsMapped
		{
			get { return MappedValue.HasValue; }
		}

		public static implicit operator ZString(ZCodeMappedZString value)
		{
			return value.MappedValue.GetValueOrDefault(value.SourceValue);
		}

		public static implicit operator string(ZCodeMappedZString value)
		{
			return (ZString)value;
		}

		public static implicit operator ZCodeMappedZString(ZString value)
		{
			return new ZCodeMappedZString(value);
		}

		public static implicit operator ZCodeMappedZString(string value)
		{
			return (ZString)value;
		}

		public override string ToString()
		{
			return (ZString)this;
		}

		public override bool Equals(object obj)
		{
			bool result;
			if (obj is ZCodeMappedZString && (ZCodeMappedZString)obj == this)
			{
				result = true;
			}
			else if (obj is ZString && (ZString)obj == this)
			{
				result = true;
			}
			else
			{
				string o = obj as string;
				result = o != null && o == this;
			}

			return result;
		}

		public static bool operator ==(ZCodeMappedZString a, ZCodeMappedZString b)
		{
			return (ZString)a == (ZString)b;
		}

		public static bool operator !=(ZCodeMappedZString a, ZCodeMappedZString b)
		{
			return (ZString)a != (ZString)b;
		}

		public static bool operator ==(ZCodeMappedZString a, ZString b)
		{
			return (ZString)a == b;
		}

		public static bool operator !=(ZCodeMappedZString a, ZString b)
		{
			return (ZString)a != b;
		}

		public static bool operator ==(ZCodeMappedZString a, string b)
		{
			return (ZString)a == b;
		}

		public static bool operator !=(ZCodeMappedZString a, string b)
		{
			return (ZString)a != b;
		}

		public static bool operator ==(ZString a, ZCodeMappedZString b)
		{
			return a == (ZString)b;
		}

		public static bool operator !=(ZString a, ZCodeMappedZString b)
		{
			return a != (ZString)b;
		}

		public static bool operator ==(string a, ZCodeMappedZString b)
		{
			return a == (ZString)b;
		}

		public static bool operator !=(string a, ZCodeMappedZString b)
		{
			return a != (ZString)b;
		}

		public override int GetHashCode()
		{
			return ((ZString)this).GetHashCode();
		}
	}
}
