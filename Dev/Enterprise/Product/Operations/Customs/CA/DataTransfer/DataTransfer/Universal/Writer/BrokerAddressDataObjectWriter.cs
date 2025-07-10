using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.CA.DataTransfer.Universal
{
	sealed class BrokerAddressDataObjectWriter : DataObjectWriter<OrgHeader, OrganizationAddress>
	{
		public BrokerAddressDataObjectWriter(IDataWritingManager writeManager, ZString addressType, GlbStaff staff = null)
			: base(writeManager)
		{
			this.addressType = addressType;
			this.staff = staff;
		}

		readonly ZString addressType;
		readonly GlbStaff staff;

		protected override OrganizationAddress PopulateDataObject(OrgHeader broker)
		{
			if (broker == null)
			{
				return null;
			}

			var mainAddress = broker.MainAddress;

			var addressData = new OrganizationAddress(writeManager.WriterStrategy)
			{
				AddressType = addressType,
				AddressOverride = ZBool.False,
				OrganizationCode = broker.OH_Code,

				Port = ListHelper.GetWithName(mainAddress?.OA_RL_NKRelatedPortCode ?? ZString.Empty, broker.Factory.GetRefUNLOCOList()),
				ScreeningStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(broker.OH_ScreeningStatus, broker.Lookups.ScreeningStatusesList),

				Country = Country.New(broker.Country),
				AddressShortCode = mainAddress?.OA_Code,
				Address1 = mainAddress?.Address1,
				Address2 = mainAddress?.Address2,
				City = mainAddress?.City,
				Postcode = mainAddress?.Postcode,
				State = mainAddress?.OA_State
			};

			var companyNameOverride = mainAddress != null ? mainAddress.OA_CompanyNameOverrideTruncated : ZString.Empty;
			addressData.CompanyName = companyNameOverride.IsEmpty ? broker.OH_FullNameTruncated : companyNameOverride;

			if (staff != null)
			{
				addressData.Contact = staff.GS_FullName;
				addressData.Email = staff.GS_EmailAddress;
				addressData.Fax = staff.GS_FaxNum.IsEmpty ? staff.HomeBranch?.GB_Fax ?? ZString.Empty : staff.GS_FaxNum;
				addressData.Mobile = staff.GS_MobilePhone;
				addressData.Phone = staff.GS_WorkPhone.IsEmpty ? staff.HomeBranch?.GB_Phone ?? ZString.Empty : staff.GS_WorkPhone;
			}
			else
			{
				addressData.Email = mainAddress?.OA_Email;
				addressData.Fax = mainAddress?.OA_Fax;
				addressData.Phone = mainAddress?.OA_Phone;
			}

			return addressData;
		}
	}
}
