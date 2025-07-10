using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class NT060ResponsePrettyFormatter : CustomsStatusUpdatePrettyFormatter<INT060ResponseDetail>, IMessagePrettyFormatter
{
	public NT060ResponsePrettyFormatter(BusinessObjectFactory factory, INT060ResponseDetail responseDetail) : base(factory, responseDetail) { }

	protected override IEnumerable<(string label, string text)> GetDetails()
	{
		if (ResponseDetail.CustomsOfficeOfDepartureReferenceNumber != null)
		{
			yield return (CustomsOfficeLabel, $"{ResponseDetail.CustomsOfficeOfDepartureReferenceNumber} - {CustomsOfficeCodes.GetDescriptionFromCode(ResponseDetail.CustomsOfficeOfDepartureReferenceNumber)}");
		}

		yield return (SelectionStatusLabel, ResponseDetail.SelectionStatus);
		yield return (InspectionDecisionLabel, ResponseDetail.SelectionInspectionDecision);
	}

	CodeDescriptionPairList CustomsOfficeCodes => customsOfficeCodes ?? (customsOfficeCodes = RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today));
	CodeDescriptionPairList customsOfficeCodes;

	protected override string Title => Res.GetString("8F28F87C-233B-44FD-85F7-FB6B82412B9E", "Intention to Control");
	static string CustomsOfficeLabel => Res.GetString("265F5847-9355-44B7-BC9E-BF902960E54B", "Customs Office of Departure");
	static string SelectionStatusLabel => Res.GetString("099282DE-D320-476A-8AA7-E027C5FC98FE", "Selection Status");
	static string InspectionDecisionLabel => Res.GetString("ED53F2B9-21C1-4407-B8D4-05E75CFCD5EF", "Inspection Decision");
}
