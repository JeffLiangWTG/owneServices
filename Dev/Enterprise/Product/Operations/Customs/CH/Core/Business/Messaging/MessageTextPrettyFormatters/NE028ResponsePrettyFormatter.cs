using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business;

public sealed class NE028ResponsePrettyFormatter : DecisionResponsePrettyFormatter<INE028ResponseDetail>, IMessagePrettyFormatter
{
	public NE028ResponsePrettyFormatter(BusinessObjectFactory factory, INE028ResponseDetail responseDetail) : base(factory, responseDetail) { }

	protected override ZString ResponseTitle => Res.GetString("347843ed-7340-4593-a197-9b71e56f4d3f", "Export response");
}
