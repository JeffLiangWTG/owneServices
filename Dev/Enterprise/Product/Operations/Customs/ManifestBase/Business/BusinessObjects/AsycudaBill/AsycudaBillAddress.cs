using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Integration.Customs.ManifestBase;

namespace Enterprise.Customs.ManifestBase
{
	public class AsycudaBillAddress : NonPersistentBusinessObject, IManifestBillAddress, IAddressDetails
	{
		public enum AddressType
		{
			None = 0,
			Consignee = 1,
			Shipper = 2,
			NotifyParty = 3,
			FreightForwarder = 4,
			Buyer = 5,
			Seller = 6,
		}

		public AsycudaBillAddress(AddressType type, ZPropertyInfo addressPKInfo)
		{
			Argument.NotNull(addressPKInfo, nameof(addressPKInfo));
			addressType = type;
			factory = addressPKInfo.BizObj.Factory;
			this.addressPKInfo = addressPKInfo;
			addressPKInfo.ValueChanged += AddressPKInfo_ValueChanged;
			LoadAddressIfNeeded();
		}

		public AsycudaBillAddress(AddressType type, ZPropertyInfo addressPKInfo, ZPropertyInfo partyNameInfo, ZPropertyInfo partyStreet1Info, ZPropertyInfo partyStreet2Info, ZPropertyInfo partyCityInfo, ZPropertyInfo partyStateInfo, ZPropertyInfo partyPostcodeInfo, ZPropertyInfo partyCountryInfo, ZPropertyInfo partyPhoneInfo = null)
		{
			Argument.NotNull(addressPKInfo, nameof(addressPKInfo));
			Argument.NotNull(addressPKInfo.BizObj, nameof(addressPKInfo.BizObj));
			Argument.NotNull(partyNameInfo, nameof(partyNameInfo));
			Argument.NotNull(partyStreet1Info, nameof(partyStreet1Info));
			Argument.NotNull(partyStreet2Info, nameof(partyStreet2Info));
			Argument.NotNull(partyCityInfo, nameof(partyCityInfo));
			Argument.NotNull(partyStateInfo, nameof(partyStateInfo));
			Argument.NotNull(partyPostcodeInfo, nameof(partyPostcodeInfo));
			Argument.NotNull(partyCountryInfo, nameof(partyCountryInfo));
			addressType = type;
			factory = addressPKInfo.BizObj.Factory;
			this.addressPKInfo = addressPKInfo;
			this.partyNameInfo = partyNameInfo;
			this.partyStreet1Info = partyStreet1Info;
			this.partyStreet2Info = partyStreet2Info;
			this.partyCityInfo = partyCityInfo;
			this.partyStateInfo = partyStateInfo;
			this.partyPostcodeInfo = partyPostcodeInfo;
			this.partyCountryInfo = partyCountryInfo;
			this.partyPhoneInfo = partyPhoneInfo;
			addressPKInfo.ValueChanged += AddressPKInfo_ValueChanged;
			LoadAddressIfNeeded();
		}

		public ZString CompanyName => addressDetails?.CompanyName ?? ZString.Empty;
		public ZString Address1 => addressDetails?.AddressLine1 ?? ZString.Empty;
		public ZString Address2 => addressDetails?.AddressLine2 ?? ZString.Empty;
		public ZString City => addressDetails?.City ?? ZString.Empty;
		public ZString State => addressDetails?.State ?? ZString.Empty;
		public ZString Postcode => addressDetails?.PostCode ?? ZString.Empty;
		public ZString RN_NKCountryCode => addressDetails?.Country ?? ZString.Empty;
		public ZString Phone => addressDetails?.Phone ?? ZString.Empty;
		public OrgAddress PostalAddress => address?.Header.Addresses.Cast<OrgAddress>().FirstOrDefault(x => x.AddressCapability.GetCapabilityEnabled(OrgAddressType.Postal.Code)) ?? address;

		IAddressDetails addressDetails => useRealAddress ? address : this;

		#region IAddressDetails

		ZString IAddressDetails.CompanyName => (ZString)(partyNameInfo?.Value ?? ZString.Empty);

		ZString IAddressDetails.ContactName => ZString.Empty;

		// there is no null-safe cast operator between IZType and ZString
		ZString IAddressDetails.Phone => (ZString)(partyPhoneInfo?.Value ?? ZString.Empty);

		ZString IAddressDetails.Fax => ZString.Empty;

		ZString IAddressDetails.Email => ZString.Empty;

		ZString IAddressDetails.AddressLine1 => (ZString)(partyStreet1Info?.Value ?? ZString.Empty);

		ZString IAddressDetails.AddressLine2 => (ZString)(partyStreet2Info?.Value ?? ZString.Empty);

		ZString IAddressDetails.City => (ZString)(partyCityInfo?.Value ?? ZString.Empty);

		ZString IAddressDetails.State => (ZString)(partyStateInfo?.Value ?? ZString.Empty);

		ZString IAddressDetails.PostCode => (ZString)(partyPostcodeInfo?.Value ?? ZString.Empty);

		ZString IAddressDetails.Country => (ZString)(partyCountryInfo?.Value ?? ZString.Empty);

		#endregion

		void AddressPKInfo_ValueChanged(object sender, EventArgs e)
		{
			LoadAddressIfNeeded();
		}

		void LoadAddressIfNeeded()
		{
			var addressPK = (ZGuid)addressPKInfo.Value;
			useRealAddress = !addressPK.IsEmpty;
			address = useRealAddress ? factory.Load<OrgAddress>(addressPK) : null;
		}

		public AddressType AsycudaBillAddressType
		{
			get { return addressType; }
		}

		ZPropertyInfo IManifestBillAddress.OA_AddressInfo
		{
			get { return addressPKInfo; }
		}

		OrgAddress address;
		bool useRealAddress;
		readonly AddressType addressType;
		readonly BusinessObjectFactory factory;
		readonly ZPropertyInfo addressPKInfo;
		readonly ZPropertyInfo partyNameInfo;
		readonly ZPropertyInfo partyStreet1Info;
		readonly ZPropertyInfo partyStreet2Info;
		readonly ZPropertyInfo partyCityInfo;
		readonly ZPropertyInfo partyStateInfo;
		readonly ZPropertyInfo partyPostcodeInfo;
		readonly ZPropertyInfo partyCountryInfo;
		readonly ZPropertyInfo partyPhoneInfo;
	}
}
