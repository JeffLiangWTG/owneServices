using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageBuildingBlocks
{
	public sealed class MessageBlockDecimalAttribute : MessageBlockAttribute
	{
		public MessageBlockDecimalAttribute(int position, int length)
			: base(position, length)
		{
		}

		#region Implementation
		protected override string SerialiseCore(IZType value, bool humanFriendly)
		{
			ZDecimal valueAsDecimal = (ZDecimal)value;
			string result = valueAsDecimal.ToStringTrimZeros();
			int availableDecimalPlaces = Length - 2; // 1 for the '0' and 1 for the '.'
			while (result.Length > Length && availableDecimalPlaces >= 0)
			{
				result = valueAsDecimal.Round(availableDecimalPlaces--).ToStringTrimZeros();
			}

			if (result.Length > Length || valueAsDecimal < ZDecimal.Zero)
			{
				result = new string(InvalidPaddingCharacter, Length);
			}
			else if (!humanFriendly && result.Length < Length)
			{
				result = result.PadRight(Length);
			}
			return result;
		}

		protected override IZType DeSerialiseCore(string value)
		{
			if (value.Length == 0 || value[0] == InvalidPaddingCharacter)
			{
				return ZDecimal.Zero;
			}
			else
			{
				return (ZDecimal)ZDecimalTypeConverter.Instance.ConvertFromString(value);
			}
		}
		#endregion Implementation
	}
}
