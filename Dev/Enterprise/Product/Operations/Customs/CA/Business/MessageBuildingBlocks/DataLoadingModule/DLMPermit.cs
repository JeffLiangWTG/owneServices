using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageBuildingBlocks
{
	public class DLMPermit : MessageBlock
	{
		public DLMPermit()
			: base("P")
		{
		}

		[MessageBlockString(2, 35)]
		public ZString PermitNumber;
	}
}
