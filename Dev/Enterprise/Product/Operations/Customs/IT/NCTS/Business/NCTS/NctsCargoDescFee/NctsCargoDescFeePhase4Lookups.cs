using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsCargoDescFeePhase4Lookups : EU.NCTS.Business.NctsCargoDescFeeLookups, INctsCargoDescFeeLookups
{
	public NctsCargoDescFeePhase4Lookups(NctsCargoDescFee parent) : base(parent)
	{
	}

	new NctsCargoDescFee Parent => (NctsCargoDescFee)base.Parent;

	public override CodeDescriptionPairList ChargeTypeList => Factory.GetCachedValue<NctsCargoDescFeeChargeTypeList>();

	public CodeDescriptionPairList RateOverrideReasonList => Factory.GetCachedValue<RateOverrideReasonList>();

	public CodeDescriptionPairList MethodOfPaymentList => Factory.GetCachedValue("Enterprise.Customs.IT.NCTS.Business.CusInBondFeeLookups.MethodOfPaymentList", () => GetMethodsOfPayment());

	public CodeDescriptionPairList MethodOfCalculationList => Factory.GetCachedValue<NctsCargoDescFeeMethodOfCalculationList>();

	#region Implementation

	CodeDescriptionPairList GetMethodsOfPayment()
	{
		var countryCode = Parent.CargoDesc?.Header?.CountryCode ?? GlbCompany.CurrentCompany.Country.Code.ToString();
		var attributeNameValuePairs = new[] { new KeyValuePair<ZString, ZString>(RefCusCodeListAttributeTypes.Codes.Category, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47) };

		var result = new CodeDescriptionPairList();
		foreach (ICodeDescription item in Factory.GetCachedListMatchAllAttributes(countryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, attributeNameValuePairs))
		{
			if (supportedMethodOfPayments.Contains(item.Code))
			{
				result.AddPair(item.Code, item.Description);
			}
		}
		result.Sort();
		return result;
	}

	readonly ImmutableArray<string> supportedMethodOfPayments = ImmutableArray.Create("A", "B", "C", "D", "E", "F", "G", "H", "J", "K");

	#endregion
}
