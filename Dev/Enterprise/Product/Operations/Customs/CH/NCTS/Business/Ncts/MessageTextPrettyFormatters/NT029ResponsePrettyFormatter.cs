using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class NT029ResponsePrettyFormatter : CustomsStatusUpdatePrettyFormatter<INT029ResponseDetail>, IMessagePrettyFormatter
{
	public NT029ResponsePrettyFormatter(BusinessObjectFactory factory, INT029ResponseDetail responseDetail) : base(factory, responseDetail) { }

	protected override string Title => Res.GetString("28C7368F-FE91-4106-8C7B-8C205FE0324E", "Transit Released");
}
