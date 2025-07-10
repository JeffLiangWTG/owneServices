using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageBuildingBlocks
{
	public sealed class MessageBlockStringAttribute : MessageBlockAttribute
	{
		public MessageBlockStringAttribute(int position, int length)
			: this(position, length, true)
		{
		}

		public MessageBlockStringAttribute(int position, int length, bool shouldTruncate)
			: base(position, length)
		{
			this.shouldTruncate = shouldTruncate;
		}

		readonly bool shouldTruncate;

		protected override string SerialiseCore(IZType value, bool humanFriendly)
		{
			ZString result = value.ToString();
			if (!humanFriendly && result.Length < Length)
			{
				result = result.PadRight(Length);
			}
			else if (result.Length > Length)
			{
				if (shouldTruncate)
				{
					result = result.Left(Length);
				}
				else
				{
					throw new MessageBlockSerialisationException(string.Format("Data provided exceeded allowable maximum length:{0}Maximum Length:{1}{0}Actual Length:{2}", System.Environment.NewLine, Length, result.Length), new string('*', Length));
				}
			}
			return result;
		}

		protected override IZType DeSerialiseCore(string value)
		{
			return new ZString(value);
		}
	}
}
