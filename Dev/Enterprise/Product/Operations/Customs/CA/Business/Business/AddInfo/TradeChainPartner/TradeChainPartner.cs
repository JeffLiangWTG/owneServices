using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.CA;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class TradeChainPartner : AutoTradeChainPartner, ITradeChainPartner
	{
		public TradeChainPartner(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override CusAddInfoValidation GetNewValidation()
		{
			return new TradeChainPartnerValidation(this);
		}

		public new TradeChainPartnerValidation Validation
		{
			get { return (TradeChainPartnerValidation)base.Validation; }
		}

		public override string TablePrefix => CusAddInfoSchema.Constants.Prefix;

		public JobDocAddress CAOrgAddress
		{
			get
			{
				if (caOrgAddress == null ||
					caOrgAddress.IsDeleted ||
					caOrgAddress.E2_AddressType != DocAddressTypes.Codes.DropOffAddress ||
					caOrgAddress.E2_AddressSequence != 0 ||
					caOrgAddress.E2_ParentID != PK ||
					caOrgAddress.E2_ParentTableCode != TablePrefix)
				{
					caOrgAddress = LoadOrCreateCAOrgAddress();
					caOrgAddress.DocAddressChanged += delegate
					{ MarkAsNeedingValidation(); };
					caOrgAddress.OnRelationshipFieldsChanged += delegate
					{ MarkAsNeedingValidation(); };
					caOrgAddress.E2_AddressSequenceInfo.ValueChanged += delegate
					{ MarkAsNeedingValidation(); };
					RegisterEditableChildObject(caOrgAddress);
				}
				return caOrgAddress;
			}
		}
		JobDocAddress caOrgAddress;

		JobDocAddress LoadOrCreateCAOrgAddress()
		{
			JobDocAddress result = null;
			var query = new ZQuery(JobDocAddressSchema.E2_ParentID, PK);
			query.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, TablePrefix);
			query.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.DropOffAddress);
			query.AddToFilter(JobDocAddressSchema.E2_AddressSequence, 0);
			query.FetchOnlyFromLocalCache = !IsInDatabase;
			result = Factory.LoadTop1<JobDocAddress>(query);
			if (result == null)
			{
				result = Factory.New<JobDocAddress>();
				result.E2_ParentTableCode = TablePrefix;
				result.E2_ParentID = PK;
				result.DocAddressType = DocAddressType.DropOffAddress;
			}
			return result;
		}

		public OrgHeader Organization => Factory.Load<OrgHeader>(CA_Org);

		[RelatedBusinessObject("Organization")]
		[ResourceStringData("TradeChainPartner|a13a7e91-c25d-4d9b-968a-86f558f42be8", Caption = "Organization")]
		public ZGuid CA_Org
		{
			get
			{
				return CAOrgAddress.OrganisationPK;
			}
			set
			{
				var oldValue = CAOrgAddress.OrganisationPK;
				CAOrgAddress.OrganisationPK = value;
				if (!IsCopying && oldValue != value)
				{
					SetCACSAID();
					CA_OrgInfo.RefreshBinding(oldValue);
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateCA_Org();
				}
			}
		}

		public ZPropertyInfo CA_OrgInfo
		{
			get { return GetZPropertyInfo(nameof(CA_Org)); }
		}

		public OrgAddress OrganizationAddress => Factory.Load<OrgAddress>(CA_Address);

		[ReadOnlyMember(nameof(CA_AddressReadOnly))]
		[RelatedBusinessObject("OrganizationAddress")]
		[ResourceStringData("TradeChainPartner|bc63ca86-82ce-406e-a763-c85e94f8165a", Caption = "Org. Address")]
		public ZGuid CA_Address
		{
			get
			{
				return CAOrgAddress.E2_OA_Address;
			}
			set
			{
				var oldValue = CAOrgAddress.E2_OA_Address;
				CAOrgAddress.E2_OA_Address = value;
				if (!IsCopying && oldValue != value)
				{
					SetCACSAID();
					CA_AddressInfo.RefreshBinding(oldValue);
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateAll();
				}
			}
		}

		public ZPropertyInfo CA_AddressInfo
		{
			get { return GetZPropertyInfo(nameof(CA_Address)); }
		}

		public bool CA_AddressReadOnly => !CA_Org.IsValid;

		[List(nameof(AddInfoLookups) + "." + nameof(TradeChainPartnerAddInfoLookups.TradeChainPartnersTypeList))]
		[ResourceStringData("TradeChainPartner|b95e046d-2b95-44b0-91ab-9c0a0486d90e", Caption = "Type")]
		public override ZString CA_Type
		{
			get
			{
				return base.CA_Type;
			}
			set
			{
				var oldValue = base.CA_Type;
				base.CA_Type = value;
				if (!IsCopying && oldValue != value)
				{
					CA_TypeInfo.RefreshBinding(oldValue);
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateAll();
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(TradeChainPartnerAddInfoLookups.CSAIDTypeList))]
		[ResourceStringData("TradeChainPartner|22b13a07-f036-47e7-8056-84f3a9a8e1d2", Caption = "CSA ID Type")]
		[MaxLength(3)]
		public override ZString CA_CSAIDType
		{
			get
			{
				return base.CA_CSAIDType;
			}
			set
			{
				var oldValue = base.CA_CSAIDType;
				base.CA_CSAIDType = value;
				if (!IsCopying && oldValue != value)
				{
					SetCACSAID();
					CA_CSAIDTypeInfo.RefreshBinding(oldValue);
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateAll();
				}
			}
		}

		void SetCACSAID()
		{
			CA_CSAID = ZString.Empty;
			if (CA_Org.IsValid && !string.IsNullOrEmpty(CA_CSAIDType))
			{
				switch (CA_CSAIDType)
				{
					case CSAConsigneeIDTypeList.Codes.ORG:
						CA_CSAID = CAOrgAddress.Organisation.OH_Code;
						break;
					case CSAVendorIDTypeList.Codes.DUN:
					case CSAVendorIDTypeList.Codes.EIN:
					case CSAVendorIDTypeList.Codes.SSN:
					case CSAVendorIDTypeList.Codes.CCC:
						CA_CSAID = CAOrgAddress.Organisation.CustomsCodes.GetCustomsRegNo(CA_CSAIDType, Core.Constants.CountryCodes.UnitedStates, CAOrgAddress.E2_OA_Address);
						break;
					case CSAConsigneeIDTypeList.Codes.BRM:
					case CSAConsigneeIDTypeList.Codes.CSA:
						CA_CSAID = CAOrgAddress.Organisation.CustomsCodes.GetCustomsRegNo(CA_CSAIDType, Core.Constants.CountryCodes.Canada, CAOrgAddress.E2_OA_Address);
						break;
					default:
						break;
				}
			}
		}

		[ResourceStringData("TradeChainPartner|bc8bd771-b590-48c4-a21c-a7029b5c6f0e", Caption = "CSA ID")]
		[MaxLength(35)]
		[ReadOnlyMember(nameof(CA_CSAID_ReadOnly))]
		public override ZString CA_CSAID
		{
			get
			{
				return base.CA_CSAID;
			}
			set
			{
				var oldValue = base.CA_CSAID;
				base.CA_CSAID = value;
				if (!IsCopying && oldValue != value)
				{
					CA_CSAIDInfo.RefreshBinding(oldValue);
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateAll();
				}
			}
		}

		bool CA_CSAID_ReadOnly => CA_CSAIDType != OrgCusCode.CACodeTypes.CSAReferenceID;

		[List(nameof(AddInfoLookups) + "." + nameof(TradeChainPartnerAddInfoLookups.CSAStatusEditableByUserList))]
		[ResourceStringData("TradeChainPartner|add90fc9-d8b9-4b37-8f16-b41e194ba571", Caption = "CSA Status")]
		[MaxLength(7)]
		[ReadOnlyMember(nameof(CA_CSAStatus_ReadOnly))]
		public override ZString CA_CSAStatus
		{
			get
			{
				return base.CA_CSAStatus;
			}
			set
			{
				var oldValue = base.CA_CSAStatus;
				base.CA_CSAStatus = value;
				if (!IsCopying && oldValue != value)
				{
					CA_CSAStatusInfo.RefreshBinding(oldValue);
				}
				if (value == CSAStatusList.Codes.Added && base.CA_Action == CSAActionTypeList.Codes.ReqAdd
					|| value == CSAStatusList.Codes.AwaitingAdd
					|| value == CSAStatusList.Codes.AwaitingDelete)
				{
					base.CA_Action = ZString.Empty;
				}
			}
		}

		bool CA_CSAStatus_ReadOnly
		{
			get
			{
				return CA_CSAStatus == CSAStatusList.Codes.AwaitingAdd || CA_CSAStatus == CSAStatusList.Codes.AwaitingDelete
					|| CA_CSAStatus == CSAStatusList.Codes.Deleted || CA_CSAStatus == CSAStatusList.Codes.ErrorAdded
					|| CA_CSAStatus == CSAStatusList.Codes.ErrorDeleted;
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(TradeChainPartnerAddInfoLookups.CSAStatusList))]
		[ResourceStringData("TradeChainPartner|B0355A0E-AFB7-4B50-A458-166CD62321E0", Caption = "CSA Status Description")]
		[MaxLength(35)]
		[ReadOnlyMember(nameof(StatusDescriptionReadOnly))]
		public ZString StatusDescription
		{
			get
			{
				return AddInfoLookups.CSAStatusList.GetDescriptionFromCode(base.CA_CSAStatus) ?? ZString.Empty;
			}
		}

		public bool StatusDescriptionReadOnly => true;

		[List(nameof(AddInfoLookups) + "." + nameof(TradeChainPartnerAddInfoLookups.CSAActionTypeList))]
		[ResourceStringData("TradeChainPartner|BE88E864-B9E9-43FC-8684-F2A39229EEB6", Caption = "Action")]
		[MaxLength(6)]
		[ReadOnlyMember(nameof(CA_Action_ReadOnly))]
		public override ZString CA_Action
		{
			get
			{
				return base.CA_Action;
			}
			set
			{
				var oldValue = base.CA_Action;
				base.CA_Action = value;
				if (!IsCopying && oldValue != value)
				{
					CA_ActionInfo.RefreshBinding(oldValue);
				}
			}
		}

		bool CA_Action_ReadOnly
		{
			get
			{
				return CA_CSAStatus == CSAStatusList.Codes.AwaitingAdd || CA_CSAStatus == CSAStatusList.Codes.AwaitingDelete;
			}
		}

		public OrgHeader ParentOrgHeader => Parent as OrgHeader;
	}
}
