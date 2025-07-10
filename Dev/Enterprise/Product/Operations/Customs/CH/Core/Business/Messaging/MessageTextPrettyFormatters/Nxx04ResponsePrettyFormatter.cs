using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business;

public sealed class Nxx04ResponsePrettyFormatter : DecisionResponsePrettyFormatter<INxx04ResponseDetail>, IMessagePrettyFormatter
{
	public Nxx04ResponsePrettyFormatter(BusinessObjectFactory factory, INxx04ResponseDetail responseDetail) : base(factory, responseDetail) { }

	protected override ZString ResponseTitle => Res.GetString("19208760-A7C4-44A2-AB7B-5F803E8C66D7", "Amendment response");
}
