using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageBuildingBlocks
{
	public sealed class MessageBlockShortAttribute : MessageBlockAttribute
	{
		public MessageBlockShortAttribute(int position, int length)
			: base(position, length)
		{
		}

		protected override string SerialiseCore(IZType value, bool humanFriendly)
		{
			ZShort valueAsShort = (ZShort)value;
			string result = valueAsShort.ToString();
			if (result.Length > Length || valueAsShort < ZShort.Zero)
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
			return (value.Length == 0 || value[0] == InvalidPaddingCharacter) ? ZShort.Zero : new ZShort(short.Parse(value));
		}
	}
}
