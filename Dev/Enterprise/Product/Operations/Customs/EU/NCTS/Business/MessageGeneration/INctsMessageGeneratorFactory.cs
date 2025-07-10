using Enterprise.Customs.EU.Business.MessageBuilders;

namespace Enterprise.Customs.EU.NCTS.Business.MessageGeneration
{
	public interface INctsMessageGeneratorFactory
	{
		IMessageGenerator<NctsHeader> Get(NctsMessageFunctionSet messageFunction);
	}
}
