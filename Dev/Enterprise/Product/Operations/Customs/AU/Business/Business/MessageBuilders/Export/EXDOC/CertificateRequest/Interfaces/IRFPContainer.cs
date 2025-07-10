using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	interface IRFPContainer
	{
		ZString ContainerNumber { get; }
		ZString Seal { get; }
	}
}
