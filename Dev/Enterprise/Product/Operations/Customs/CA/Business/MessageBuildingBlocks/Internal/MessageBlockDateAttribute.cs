using System;

using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageBuildingBlocks
{
	public sealed class MessageBlockDateAttribute : MessageBlockAttribute
	{
		public MessageBlockDateAttribute(int position)
			: this(position, "yyyyMMdd")
		{
		}

		public MessageBlockDateAttribute(int position, string dateFormat)
			: base(position, dateFormat.Length)
		{
			DateFormat = dateFormat;
		}

		public readonly string DateFormat;

		#region Implementation

		protected override string SerialiseCore(IZType value, bool humanFriendly)
		{
			ZDate date = (ZDate)value;
			if (humanFriendly)
			{
				return date.ToShortDateString();
			}
			else
			{
				return date.ToString(DateFormat).PadRight(Length);
			}
		}

		protected override IZType DeSerialiseCore(string value)
		{
			ZDate result;
			if (value == new string('9', Length))
			{
				result = new ZDate(2099, 12, 31);
			}
			else if (value == new string(ZeroPaddingCharacter, Length) || value == new string(SpacePaddingCharacter, Length))
			{
				result = ZDate.Empty;
			}
			else
			{
				ZDateTime dateTimeResult;
				if (ZDateTime.TryParseExact(value, out dateTimeResult, DateFormat))
				{
					result = dateTimeResult.Date;
				}
				else
				{
					throw new ArgumentException("'" + value + "' is not a valid " + DateFormat + " format", nameof(value));
				}
			}
			return result;
		}

		const char ZeroPaddingCharacter = '0';
		const char SpacePaddingCharacter = ' ';

		#endregion
	}
}
