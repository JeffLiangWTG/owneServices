using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Customs.IT.Business.SADConstants;

namespace Enterprise.Customs.IT.Business;

public class SADTraderWrapper : ITrader
{
	public SADTraderWrapper(JobDocAddress jobDocAddress)
		: this(jobDocAddress, jobDocAddress?.Organisation, jobDocAddress?.Country)
	{ }

	public SADTraderWrapper(OrgAddress orgAddress)
		: this(orgAddress, orgAddress?.Header, orgAddress?.Country)
	{ }

	SADTraderWrapper(IDocAddress docAddress, OrgHeader orgHeader, RefCountry refCountry)
	{
		this.docAddress = docAddress;
		Organisation = orgHeader;
		this.refCountry = refCountry;
	}
	readonly IDocAddress docAddress;
	readonly RefCountry refCountry;

	protected OrgHeader Organisation { get; }

	public ZString IdCountryCode => CustomsCodeInfo?.IdCountryCode ?? ZString.Empty;

	public ZString ID => IDCore;
	protected virtual ZString IDCore => CustomsCodeInfo?.Id ?? ZString.Empty;

	public ZString Name => docAddress?.E2_CompanyName.Left(CustomsFieldMaxLength.Trader.Name) ?? ZString.Empty;

	public ZString Address => docAddress?.GetAddressAsASingleLineForCustomsMessage().Left(CustomsFieldMaxLength.Trader.Address) ?? ZString.Empty;

	public ZString Postcode => docAddress?.E2_Postcode.Left(CustomsFieldMaxLength.Trader.PostCode) ?? ZString.Empty;

	public ZString City => docAddress?.E2_City.Left(CustomsFieldMaxLength.Trader.City) ?? ZString.Empty;

	public ZString CountryCode => refCountry?.Code ?? ZString.Empty;

	#region Implementation

	OrgHeaderCustomsCodeInfo CustomsCodeInfo => customsCodeInfo ?? (customsCodeInfo = Organisation?.GetCustomsCodeInfo(refCountry, ShouldUseZeroCustomsCodePlaceholderIfNoneFound));
	OrgHeaderCustomsCodeInfo customsCodeInfo;

	protected virtual ZBool ShouldUseZeroCustomsCodePlaceholderIfNoneFound => ZBool.True;

	#endregion
}
