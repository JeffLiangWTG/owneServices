using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class NT008ResponsePrettyFormatter : DecisionResponsePrettyFormatter<INT008ResponseDetail>, IMessagePrettyFormatter
{
	public NT008ResponsePrettyFormatter(BusinessObjectFactory factory, INT008ResponseDetail responseDetail) : base(factory, responseDetail) { }

	protected override ZString ResponseTitle => Res.GetString("E3B39608-2522-471A-B0F7-FE68ADAAE5EE", "Arrival Notification response");

	protected override void AppendFormatterSpecificText(ZStringBuilder htmlBuilder) => AppendHtmlEncodedTag(htmlBuilder, "h3", $"{ArrivalReferenceNumberSubTitle}: {ResponseDetail.ArrivalReferenceNumber}");

	static string ArrivalReferenceNumberSubTitle => Res.GetString("BCA7CAF7-A6E1-4615-A859-79CCF52592E7", "Customs Arrival Reference Number");
}
