using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageBuildingBlocks
{
	public class DLMContainer : MessageBlock
	{
		public DLMContainer()
			: base("C")
		{
		}

		[MessageBlockString(2, 25)]
		public ZString ContainerNumber;
	}
}
