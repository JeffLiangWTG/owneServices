using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NT045ResponsePrettyFormatter : CustomsStatusUpdatePrettyFormatter<INT045ResponseDetail>, IMessagePrettyFormatter
{
	public NT045ResponsePrettyFormatter(BusinessObjectFactory factory, INT045ResponseDetail responseDetail) : base(factory, responseDetail) { }

	protected override string Title => Res.GetString("BACEBDC8-B0F2-4B84-B504-5AAEB24F0368", "Transit Movement successfully closed");

	protected override IEnumerable<(string label, string text)> GetDetails() => new (string, string)[] { (Res.GetString("553451A3-4F39-438B-B8F0-4C87774C7533", "Write-off date"), ResponseDetail.WriteOffDate.ToString("dd.MM.yyyy")) };
}
