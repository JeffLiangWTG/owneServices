using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business;

sealed class NE009ResponsePrettyFormatter : DecisionResponsePrettyFormatter<INE009ResponseDetail>, IMessagePrettyFormatter
{
	public NE009ResponsePrettyFormatter(BusinessObjectFactory factory, INE009ResponseDetail responseDetail) : base(factory, responseDetail) { }

	protected override ZString ResponseTitle => Res.GetString("6C63053E-6409-4E5B-96E7-27C9077688E8", "Withdrawal response");
}
