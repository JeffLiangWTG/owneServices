using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.CH.NCTS.Business;

[CodeAlive("implementation for further use")]
public class NT515DataProvider : BaseTransitDeclarationDataProvider, INT515
{
	public NT515DataProvider(NctsHeaderDepartureMessageSendingObject sendingObject) : base(sendingObject)
	{
	}
}
