using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business;

sealed class NE131ResponsePrettyFormatter : DecisionResponsePrettyFormatter<INE131ResponseDetail>, IMessagePrettyFormatter
{
	public NE131ResponsePrettyFormatter(BusinessObjectFactory factory, INE131ResponseDetail responseDetail) : base(factory, responseDetail) { }

	protected override ZString ResponseTitle => Res.GetString("0796e611-e7fa-4f23-bd72-fed997f82364", "Data transfer e-Dec to Passar response");
}
