using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.NCTS.Business.MessageGeneration
{
	public interface INctsNativeBuilder
	{
		ZString NativeMessage<T>(T nctsHeader, NctsMessageFunctionSet messageFunction, ErrorCollector errorCollector);
	}
}
