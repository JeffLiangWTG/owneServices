using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business.ServiceTask;

namespace Enterprise.Customs.EU.NCTS.Business.Interfaces
{
	public interface INctsIE29CusdecParser
	{
		NctsEdiMessage EdiMessage { set; get; }
		BusinessObjectFactory Factory { set; get; }
		NctsIE29CusdecResponseData Parse();
	}
}
