using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ICusSCAContainerInformationProvider
	{
		ZString LloydsNumber { get; }
		ZString VoyageNumber { get; }
		ZString ContainerNumber { get; }
	}
}
