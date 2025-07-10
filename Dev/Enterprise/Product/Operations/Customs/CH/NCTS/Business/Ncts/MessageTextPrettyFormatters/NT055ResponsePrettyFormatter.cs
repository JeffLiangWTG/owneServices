using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using RefCusCodeList = Enterprise.Customs.CH.Business.UniversalReferenceConstants.RefCusCodeList;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class NT055ResponsePrettyFormatter : CustomsStatusUpdatePrettyFormatter<INT055ResponseDetail>, IMessagePrettyFormatter
{
	public NT055ResponsePrettyFormatter(BusinessObjectFactory factory, INT055ResponseDetail responseDetail) : base(factory, responseDetail) { }

	protected override IEnumerable<(string label, string text)> GetDetails()
	{
		var guaranteeReferenceLabel = GuaranteeReferenceLabel;
		var reasonLabel = ReasonLabel;

		foreach (var guarantee in ResponseDetail.GuaranteeReferences)
		{
			yield return (guaranteeReferenceLabel, guarantee.GRN);
			yield return (reasonLabel, $"{guarantee.InvalidGuaranteeReasonCode} - {ReasonCodes.GetDescriptionFromCode(guarantee.InvalidGuaranteeReasonCode)}");
		}
	}

	CodeDescriptionPairList ReasonCodes => reasonCodes ?? (reasonCodes = RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, RefCusCodeList.PassarTypes.N0252, ZDateTime.Today));
	CodeDescriptionPairList reasonCodes;

	protected override string Title => Res.GetString("F199D822-E6F6-4B8B-BA3B-6A6CBF601A4A", "Invalid Guarantee(s)");
	static string GuaranteeReferenceLabel => Res.GetString("23C64972-40F5-4ACB-AD67-4E367B5570A3", "Guarantee Reference Number");
	static string ReasonLabel => Res.GetString("AAA32156-EC03-4C5B-AC1B-187785CCE8F3", "Reason");
}
