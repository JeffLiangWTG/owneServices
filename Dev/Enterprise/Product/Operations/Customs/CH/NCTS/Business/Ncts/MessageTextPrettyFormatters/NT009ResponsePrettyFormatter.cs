using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class NT009ResponsePrettyFormatter : DecisionResponsePrettyFormatter<INT009ResponseDetail>, IMessagePrettyFormatter
{
	public NT009ResponsePrettyFormatter(BusinessObjectFactory factory, INT009ResponseDetail responseDetail) : base(factory, responseDetail) { }

	protected override ZString ResponseTitle => Res.GetString("3967CD1D-9172-4CD5-9440-F17E50C48BA8", "Withdrawal response");
}
