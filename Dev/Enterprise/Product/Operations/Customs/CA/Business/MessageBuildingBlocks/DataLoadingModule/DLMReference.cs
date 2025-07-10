using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageBuildingBlocks
{
	public class DLMReference : MessageBlock
	{
		public DLMReference()
			: base("R")
		{
		}

		[MessageBlockString(2, 35)]
		public ZString ReferenceNumber;
	}
}
