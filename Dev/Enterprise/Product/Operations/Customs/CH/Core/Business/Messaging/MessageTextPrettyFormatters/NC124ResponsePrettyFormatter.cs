using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business;

public sealed class NC124ResponsePrettyFormatter : DecisionResponsePrettyFormatter<INC124ResponseDetail>, IMessagePrettyFormatter
{
	public NC124ResponsePrettyFormatter(BusinessObjectFactory factory, INC124ResponseDetail responseDetail) : base(factory, responseDetail) { }

	protected override ZString ResponseTitle => Res.GetString("74E4187C-7AB9-46E1-8187-F5AA3C0CCE9E", "Activation response");
}
