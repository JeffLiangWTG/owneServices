using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class NT057ResponsePrettyFormatter : DecisionResponsePrettyFormatter<INT057ResponseDetail>, IMessagePrettyFormatter
{
	public NT057ResponsePrettyFormatter(BusinessObjectFactory factory, INT057ResponseDetail responseDetail) : base(factory, responseDetail) { }

	protected override ZString ResponseTitle => Res.GetString("7C25A6A4-CB0F-43BB-B094-3BA5E6173284", "Unloading Remarks response");
}
