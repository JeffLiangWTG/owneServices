using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Manifest.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.ICS.Business
{
	public class AsycudaBillSS : AsycudaBill
	{
		public AsycudaBillSS(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override Type GetPackTypeCore() => typeof(AsycudaPackSS);

		protected override ManifestBase.AsycudaBillLookups GetNewLookups() => new AsycudaBillSSLookups(this);
		public new AsycudaBillSSLookups Lookups => (AsycudaBillSSLookups)base.Lookups;

		public new AsycudaManifestHeaderSS Header => (AsycudaManifestHeaderSS)base.Header;

		public override OrgAddress Shipper
		{
			get
			{
				var value = base.Shipper;
				DefaultPartyOrgAddressDetails(value, ManifestBase.AsycudaBillAddress.AddressType.Shipper);
				return value;
			}
		}

		public override OrgAddress Consignee
		{
			get
			{
				var value = base.Consignee;
				DefaultPartyOrgAddressDetails(value, ManifestBase.AsycudaBillAddress.AddressType.Consignee);
				return value;
			}
		}

		public bool ConsigneeDeclared => ConsigneeUseRealOrg ||
			(!ABL_ConsigneeName.IsEmpty &&
			!ABL_ConsigneeStreet1.IsEmpty &&
			!ABL_ConsigneeCity.IsEmpty &&
			!ABL_RN_NKConsigneeCountry.IsEmpty &&
			!ABL_ConsigneePostcode.IsEmpty);

		public override OrgAddress NotifyParty
		{
			get
			{
				var value = base.NotifyParty;
				DefaultPartyOrgAddressDetails(value, ManifestBase.AsycudaBillAddress.AddressType.NotifyParty);
				return value;
			}
		}

		public override ZGuid ABL_OA_NotifyParty
		{
			get => base.ABL_OA_NotifyParty;
			set
			{
				base.ABL_OA_NotifyParty = value;
				PopulateSpecialMentions();
			}
		}

		public override ZString ABL_NotifyPartyName
		{
			get => base.ABL_NotifyPartyName;
			set
			{
				base.ABL_NotifyPartyName = value;
				PopulateSpecialMentions();
			}
		}

		public override ZString ABL_NotifyPartyStreet1
		{
			get => base.ABL_NotifyPartyStreet1;
			set
			{
				base.ABL_NotifyPartyStreet1 = value;
				PopulateSpecialMentions();
			}
		}

		public override ZString ABL_NotifyPartyCity
		{
			get => base.ABL_NotifyPartyCity;
			set
			{
				base.ABL_NotifyPartyCity = value;
				PopulateSpecialMentions();
			}
		}

		public override ZString ABL_RN_NKNotifyPartyCountry
		{
			get => base.ABL_RN_NKNotifyPartyCountry;
			set
			{
				base.ABL_RN_NKNotifyPartyCountry = value;
				PopulateSpecialMentions();
			}
		}

		public override ZString ABL_NotifyPartyPostcode
		{
			get => base.ABL_NotifyPartyPostcode;
			set
			{
				base.ABL_NotifyPartyPostcode = value;
				PopulateSpecialMentions();
			}
		}

		public bool NotifyPartyDeclared => NotifyPartyUseRealOrg ||
			(!ABL_NotifyPartyName.IsEmpty &&
			!ABL_NotifyPartyStreet1.IsEmpty &&
			!ABL_NotifyPartyCity.IsEmpty &&
			!ABL_RN_NKNotifyPartyCountry.IsEmpty &&
			!ABL_NotifyPartyPostcode.IsEmpty);

		void PopulateSpecialMentions()
		{
			if (NotifyPartyDeclared)
			{
				SpecialMentions = SpecialMentionsForSS;
			}
		}

		public override ZString ABL_RL_NKFinalDestination
		{
			get => base.ABL_RL_NKFinalDestination;
			set
			{
				if (ABL_RL_NKFinalDestination != value)
				{
					base.ABL_RL_NKFinalDestination = value;
					Header?.Validation?.ValidateAMA_RL_NKPortOfDischarge();
				}
			}
		}

		public override ZString ABL_RL_NKOrigin
		{
			get => base.ABL_RL_NKOrigin;
			set
			{
				if (ABL_RL_NKOrigin != value)
				{
					base.ABL_RL_NKOrigin = value;
					Header?.Validation?.ValidateAMA_RL_NKPortOfLoading();
				}
			}
		}

		protected override ManifestBase.AsycudaBillValidation GetNewValidationForRegularBill() => new AsycudaBillSSValidationForRegularBill(this);

		protected override ManifestBase.AsycudaBillValidation GetNewValidationForMasterChild() => new AsycudaBillSSValidationForMasterChild(this);

		protected override ZBool ClearShipperValueWhenEmpty => true;

		protected override ZBool ClearConsigneeValueWhenEmpty => true;

		protected override ZBool ClearNotifyPartyValueWhenEmpty => true;

		const string SpecialMentionsForSS = "10600";
	}
}
