using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class RFPContainerForTesting : IRFPContainer
	{
		public ZString ContainerNumber { get; set; }
		public ZString Seal { get; set; }
	}
}
