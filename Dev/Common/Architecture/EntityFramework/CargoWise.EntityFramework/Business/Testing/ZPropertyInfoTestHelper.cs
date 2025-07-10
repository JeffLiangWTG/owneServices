#if DEBUG
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace CargoWise.EntityFramework.Testing
{
	public static class ZPropertyInfoTestHelper
	{
		public static void SetValue(ZPropertyInfo info)
		{
			IZType value1 = info.Value;

			if (value1 is ZBlob)
			{
				info.Value = ZBlob.FromUTF8(((ZBlob)value1).ToUTF8() == "~a1~" ? "`a1`" : "~a1~");
			}
			else if (value1 is ZBool)
			{
				info.Value = new ZBool(!(ZBool)value1);
			}
			else if (value1 is ZByte)
			{
				ZByte value = new ZByte(43);
				info.Value = value.Equals(value1) ? new ZByte(44) : value;
			}
			else if (value1 is ZDateTime)
			{
				info.Value = (value1.IsValid ? (ZDateTime)value1 : ZDateTime.Now).AddMinutes(10);
			}
			else if (value1 is ZDateTimeOffset)
			{
				info.Value = (value1.IsValid ? (ZDateTimeOffset)value1 : ZDateTimeOffset.Now).AddMinutes(10);
			}
			else if (value1 is ZDate)
			{
				info.Value = (!value1.IsValid ? ZDate.Today : (ZDate)value1).AddDays(10);
			}
			else if (value1 is ZDecimal)
			{
				info.Value = new ZDecimal(((ZDecimal)value1) + 41.56m);
			}
			else if (value1 is ZGeography)
			{
				info.Value = new ZGeography("POINT(-122 47)");
			}
			else if (value1 is ZGuid)
			{
				info.Value = ZGuid.NewZGuid();
			}
			else if (value1 is ZLong)
			{
				info.Value = new ZLong(((ZLong)value1) + 1L);
			}
			else if (value1 is ZInt)
			{
				info.Value = new ZInt(((ZInt)value1) + 1);
			}
			else if (value1 is ZShort)
			{
				info.Value = new ZShort(((ZShort)value1) + 1);
			}
			else if (value1 is ZString)
			{
				ZString value = "~a1~";
				int maxLength = info.MaxLength;
				if (maxLength > 0)
				{
					value = value.Left(maxLength);
				}
				if (string.Compare(value.ToString(), value1.ToString(), true) == 0)
				{
					value = "`a1`";
					if (maxLength > 0)
					{
						value = value.Left(maxLength);
					}
				}
				info.Value = value;
			}
			else if (value1 is IMultilingualString)
			{
				info.Value = null;
			}
			else if (value1 is ZTime)
			{
				info.Value = (value1.IsValid ? (ZTime)value1 : new ZTime(ObjectCache.DateTimeProvider.CurrentLocalDateTime)).AddMinutes(10);
			}
		}
	}
}
#endif
