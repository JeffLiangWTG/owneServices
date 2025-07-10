using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageBuildingBlocks.Testing
{
	sealed class MessageBlockDummy : MessageBlock
	{
		public MessageBlockDummy()
			: base("F109")
		{
		}

		[MessageBlockString(6, 10)]
		public ZString Dummy = "1234567890";

		[MessageBlockDecimal(16, 10)]
		public ZDecimal DecimalField;
	}
}
