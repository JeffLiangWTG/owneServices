using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class NE060ResponsePrettyFormatter : CustomsStatusUpdatePrettyFormatter<INE060ResponseDetail>, IMessagePrettyFormatter
{
	public NE060ResponsePrettyFormatter(BusinessObjectFactory factory, INE060ResponseDetail responseDetail) : base(factory, responseDetail) { }

	protected override IEnumerable<(string label, string text)> GetDetails()
	{
		if (ResponseDetail.CustomsOfficeOfDepartureReferenceNumber != null)
		{
			yield return (CustomsOfficeLabel, ResponseDetail.CustomsOfficeOfDepartureReferenceNumber);
		}

		yield return (SelectionStatusLabel, ResponseDetail.SelectionNotificationStatus);
		yield return (InspectionDecisionLabel, ResponseDetail.SelectionNotificationDecision);
	}

	protected override string Title => Res.GetString("18968141-3C54-464A-9185-D861A38DA42E", "Decision to Control");
	static string CustomsOfficeLabel => Res.GetString("8F5009F6-8028-4C7F-9118-BFA0763123ED", "Customs Office of Departure");
	static string SelectionStatusLabel => Res.GetString("3EE04FCB-BB4A-4E6C-B828-D7877E347DB7", "Selection Status");
	static string InspectionDecisionLabel => Res.GetString("6EB509A9-1E54-432D-A6C8-FF8326D36030", "Inspection Decision");
}
