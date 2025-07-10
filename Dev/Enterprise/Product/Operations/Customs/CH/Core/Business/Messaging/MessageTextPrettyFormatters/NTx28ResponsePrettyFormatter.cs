using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business;

public sealed class NTx28ResponsePrettyFormatter : DecisionResponsePrettyFormatter<INTx28ResponseDetail>, IMessagePrettyFormatter
{
	public NTx28ResponsePrettyFormatter(BusinessObjectFactory factory, INTx28ResponseDetail responseDetail) : base(factory, responseDetail) { }

	protected override ZString ResponseTitle => Res.GetString("65F63799-3D2F-4B2C-9334-643E8A259385", "Departure response");
}
