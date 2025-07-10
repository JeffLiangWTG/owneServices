using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageBuildingBlocks
{
	public sealed class MessageBlockIntAttribute : MessageBlockAttribute
	{
		public MessageBlockIntAttribute(int position, int length)
			: base(position, length)
		{
		}

		protected override string SerialiseCore(IZType value, bool humanFriendly)
		{
			ZInt valueAsInt = (ZInt)value;
			string result = valueAsInt.ToString();
			if (result.Length > Length || valueAsInt < ZInt.Zero)
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
			return (value.Length == 0 || value[0] == InvalidPaddingCharacter) ? ZInt.Zero : ZInt.Parse(value);
		}
	}
}
