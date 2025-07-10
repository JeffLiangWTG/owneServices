using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

public class DocTraderDataWrapper : DocBaseWrapper
{
	public static DocTraderDataWrapper New(JobDocAddress jobDocAddress, BusinessObjectFactory factory, int addressAttributeMaxLength) => new DocTraderDataWrapper(jobDocAddress, factory, addressAttributeMaxLength);
	public static DocTraderDataWrapper New(JobDocAddress jobDocAddress, BusinessObjectFactory factory) => new DocTraderDataWrapper(jobDocAddress, factory, 29);

	public static DocTraderDataWrapper New(OrgAddress orgAddress, BusinessObjectFactory factory, int addressAttributeMaxLength) => new DocTraderDataWrapper(orgAddress, factory, addressAttributeMaxLength);
	public static DocTraderDataWrapper New(OrgAddress orgAddress, BusinessObjectFactory factory) => new DocTraderDataWrapper(orgAddress, factory, 29);

	DocTraderDataWrapper(OrgAddress orgAddress, BusinessObjectFactory factory, int addressAttributeMaxLength) : base(orgAddress, factory)
	{
		this.organisation = orgAddress?.Header;
		this.name = orgAddress?.CompanyName ?? ZString.Empty;
		this.address1 = orgAddress?.Address1 ?? ZString.Empty;
		this.address2 = orgAddress?.Address2 ?? ZString.Empty;
		this.postCode = orgAddress?.Postcode ?? ZString.Empty;
		this.city = orgAddress?.City ?? ZString.Empty;
		this.countryCode = orgAddress?.Country?.Code ?? ZString.Empty;
		this.addressAttributeMaxLength = addressAttributeMaxLength;
	}

	DocTraderDataWrapper(JobDocAddress jobDocAddress, BusinessObjectFactory factory, int addressAttributeMaxLength) : base(jobDocAddress, factory)
	{
		this.organisation = jobDocAddress?.Organisation;
		this.name = jobDocAddress?.E2_CompanyName ?? ZString.Empty;
		this.address1 = jobDocAddress?.Address1 ?? ZString.Empty;
		this.address2 = jobDocAddress?.Address2 ?? ZString.Empty;
		this.postCode = jobDocAddress?.Postcode ?? ZString.Empty;
		this.city = jobDocAddress?.City ?? ZString.Empty;
		this.countryCode = jobDocAddress?.Country?.Code ?? ZString.Empty;
		this.addressAttributeMaxLength = addressAttributeMaxLength;
	}

	readonly OrgHeader organisation;
	readonly ZString name;
	readonly ZString address1;
	readonly ZString address2;
	readonly ZString postCode;
	readonly ZString city;
	readonly ZString countryCode;
	readonly int addressAttributeMaxLength;

	public ZString Name => GetAddressAttribute(name);

	public ZString StreetAndNumber => GetAddressAttribute((ZString)($"{address1} {address2}"));

	public ZString Destination => GetAddressAttribute((ZString)($"{countryCode}-{postCode} {city}"));

	public ZString UID => identificationNumberUID ??= organisation?.GetCHCustomsRegNo(OrgCusCode.SwissCodeTypes.UID);
	string identificationNumberUID;

	public ZString BID => identificationNumberBID ??= organisation?.GetCHCustomsRegNo(OrgCusCode.SwissCodeTypes.BID);
	string identificationNumberBID;

	ZString GetAddressAttribute(ZString attribute)
	{
		return attribute.Length > addressAttributeMaxLength ? attribute.Substring(0, addressAttributeMaxLength - 3) + "..." : attribute;
	}
}
