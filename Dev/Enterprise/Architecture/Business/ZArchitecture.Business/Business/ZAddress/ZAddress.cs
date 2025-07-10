using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	[ProvideMetaDataProperty("PropertyReadonlyness", MetaDataTypes.ReadOnly)]
	public class ZAddress : NonPersistentBusinessObject, IObsoleteValidation, IZAddress
	{
		#region Schema

		public abstract class Schema
		{
			public const string Address = BindingSuffix + "+Address";
			public const string OrgPK = BindingSuffix + "+" + OrgPKColumnName;
			public const string AddressBindingPK = BindingSuffix + "+" + AddressFK;
			public const string OrgAddress_List = BindingSuffix + "+OrgAddress_List";
			public const string IsOrgVisible = BindingSuffix + "+IsOrgVisible";

			public const string BindingSuffix = "_ZAddress";
			public const string AddressFK = "AddressFK";
			public const string OrgPKColumnName = "OrgPK";
		}

		#endregion

		public delegate void Validate(ZPropertyInfo info);

		public ZAddress(ZPropertyInfo addressFKInfo)
			: base(addressFKInfo.BizObj.Factory)
		{
			addressFKInfo.AdditionalValidation += ValidateDelegate;
			fDefaultAddressType = AddressType.NoDefault;
			this.addressFKInfo = GetWrappedZPropertyInfo(Schema.AddressFK, x => addressFKInfo);
		}

		#region Address Text and Type Properties

		public AddressType DefaultAddressType
		{
			get { return fDefaultAddressType; }
			set { fDefaultAddressType = value; }
		}

		public ZString Address
		{
			get
			{
				ZString result;

				if (!OrgPK.IsValid && !OrgPK.IsEmpty)
				{
					result = Res.GetString("EmptyAddress.SelectedOrgIsInvalid", "* THE SELECTED ORGANIZATION IS NOT VALID");
				}
				else if (OrgHeader == null)
				{
					result = Res.GetString("EmptyAddress.NoOrganisationSelected", "* NO ORGANIZATION IS SELECTED");
				}
				else if (OrgHeaderAsIOrgHeader.Address_List.Count == 0)
				{
					result = Res.GetString("EmptyAddress.SelectedOrgHasNoAddresses", "* THE SELECTED ORGANIZATION HAS NO ADDRESSES ON FILE");
				}
				else if (OrgAddress == null)
				{
					result = Res.GetString("EmptyAddress.NoAddressSelected", "* NO ADDRESS IS SELECTED");
				}
				else if (OrgAddressAsIOrgAddress.Address.IsEmpty)
				{
					result = Res.GetString("EmptyAddress.NoAddress2FoundOnFile", "* NO ADDITIONAL ADDRESS DETAILS ON FILE");
				}
				else
				{
					result = OrgAddressAsIOrgAddress.Address;
				}

				return result;
			}
		}

		public ZString AddressDetailed
		{
			get
			{
				ZString result;

				if (!OrgPK.IsValid && !OrgPK.IsEmpty)
				{
					result = Res.GetString("EmptyAddress.SelectedOrgIsInvalid", "* THE SELECTED ORGANIZATION IS NOT VALID");
				}
				else if (OrgHeader == null)
				{
					result = Res.GetString("EmptyAddress.NoOrganisationSelected", "* NO ORGANIZATION IS SELECTED");
				}
				else if (OrgHeaderAsIOrgHeader.Address_List.Count == 0)
				{
					result = Res.GetString("EmptyAddress.SelectedOrgHasNoAddresses", "* THE SELECTED ORGANIZATION HAS NO ADDRESSES ON FILE");
				}
				else if (OrgAddress == null)
				{
					result = Res.GetString("EmptyAddress.NoAddressSelected", "* NO ADDRESS IS SELECTED");
				}
				else
				{
					result = OrgAddressAsIOrgAddress.AddressDetailed;
				}

				return result;
			}
		}

		public ZPropertyInfo AddressInfo
		{
			get { return GetZPropertyInfo(nameof(Address)); }
		}

		public ZString AddressFull
		{
			get
			{
				ZString result;

				if (!OrgPK.IsValid && !OrgPK.IsEmpty)
				{
					result = Res.GetString("EmptyAddress.SelectedOrgIsInvalid", "* THE SELECTED ORGANIZATION IS NOT VALID");
				}
				else if (OrgHeader == null)
				{
					result = Res.GetString("EmptyAddress.NoOrganisationSelected", "* NO ORGANIZATION IS SELECTED");
				}
				else if (OrgHeaderAsIOrgHeader.Address_List.Count == 0)
				{
					result = Res.GetString("EmptyAddress.SelectedOrgHasNoAddresses", "* THE SELECTED ORGANIZATION HAS NO ADDRESSES ON FILE");
				}
				else if (OrgAddress == null)
				{
					result = Res.GetString("EmptyAddress.NoAddressSelected", "* NO ADDRESS IS SELECTED");
				}
				else if (OrgAddressAsIOrgAddress.AddressFull.IsEmpty)
				{
					result = Res.GetString("EmptyAddress.NoAddress2FoundOnFile", "* NO ADDITIONAL ADDRESS DETAILS ON FILE");
				}
				else
				{
					result = OrgAddressAsIOrgAddress.AddressFull;
				}

				return result;
			}
		}

		public ZString AddressFullFormatted
		{
			get
			{
				ZString result;

				if (!OrgPK.IsValid && !OrgPK.IsEmpty)
				{
					result = Res.GetString("EmptyAddress.SelectedOrgIsInvalid", "* THE SELECTED ORGANIZATION IS NOT VALID");
				}
				else if (OrgHeader == null)
				{
					result = Res.GetString("EmptyAddress.NoOrganisationSelected", "* NO ORGANIZATION IS SELECTED");
				}
				else if (OrgHeaderAsIOrgHeader.Address_List.Count == 0)
				{
					result = Res.GetString("EmptyAddress.SelectedOrgHasNoAddresses", "* THE SELECTED ORGANIZATION HAS NO ADDRESSES ON FILE");
				}
				else if (OrgAddress == null)
				{
					result = Res.GetString("EmptyAddress.NoAddressSelected", "* NO ADDRESS IS SELECTED");
				}
				else if (OrgAddressAsIOrgAddress.AddressFullFormatted.IsEmpty)
				{
					result = Res.GetString("EmptyAddress.NoAddress2FoundOnFile", "* NO ADDITIONAL ADDRESS DETAILS ON FILE");
				}
				else
				{
					result = OrgAddressAsIOrgAddress.AddressFullFormatted;
				}

				return result;
			}
		}

		public ZPropertyInfo AddressFullInfo
		{
			get { return GetZPropertyInfo(nameof(AddressFull)); }
		}

		#region Phone

		public ZString PhoneAndMobile
		{
			get { return OrgHeaderAsIOrgHeader != null ? Phone + " / " + Mobile : ""; }
		}

		public ZPropertyInfo PhoneAndMobileInfo
		{
			get { return GetZPropertyInfo(nameof(PhoneAndMobile)); }
		}

		public ZString Phone
		{
			get { return OrgHeaderAsIOrgHeader != null ? OrgHeaderAsIOrgHeader.Phone : ZString.Empty; }
		}

		public ZPropertyInfo PhoneInfo
		{
			get { return GetZPropertyInfo(nameof(Phone)); }
		}

		public ZString Mobile
		{
			get { return OrgHeaderAsIOrgHeader != null ? OrgHeaderAsIOrgHeader.Mobile : ZString.Empty; }
		}

		public ZPropertyInfo MobileInfo
		{
			get { return GetZPropertyInfo(nameof(Mobile)); }
		}

		public ZString Fax
		{
			get { return OrgHeaderAsIOrgHeader != null ? OrgHeaderAsIOrgHeader.Fax : ZString.Empty; }
		}

		public ZPropertyInfo FaxInfo
		{
			get { return GetZPropertyInfo(nameof(Fax)); }
		}

		public ZString Email
		{
			get { return OrgHeaderAsIOrgHeader != null ? OrgHeaderAsIOrgHeader.Email : ZString.Empty; }
		}

		public ZPropertyInfo EmailInfo
		{
			get { return GetZPropertyInfo(nameof(Email)); }
		}

		public ZString Web
		{
			get { return OrgHeaderAsIOrgHeader != null ? OrgHeaderAsIOrgHeader.Web : ZString.Empty; }
		}

		public ZPropertyInfo WebInfo
		{
			get { return GetZPropertyInfo(nameof(Web)); }
		}

		#endregion

		#endregion

		#region Org PK

		public event EventHandler OnOrgChanged;

		public Validate OrgPKValidation
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return orgPKValidation; }
			set
			{
				if (orgPKValidation != null)
				{
					throw new InvalidOperationException("OrgPKValidation has already been set for this ZAddress");
				}

				orgPKValidation = value;
			}
		}
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		Validate orgPKValidation;

		public void SetOrgWithoutSettingDefaultAddress(ZGuid organisationPK)
		{
			if (OrgPK != organisationPK)
			{
				fOrgPK = organisationPK;
				if (OnOrgChanged != null)
				{
					OnOrgChanged(this, EventArgs.Empty);
				}

				if (!IsValidationSuspended)
				{
					ValidateOrgPK();
				}
			}
		}

		internal void SetOrgPKForUniversalCopy(ZGuid organisationPK)
		{
			if (OrgPKInitialized && BusinessObjectUniversalCopyFactoryService.IsRunningUniversalCopy(Factory))
			{
				fOrgPK = organisationPK;
			}
		}

		/// <summary>
		/// FOR *BINDING ONLY* - DO NOT USE, INSTEAD, USE SetOrgWithoutSettingDefaultAddress(ZGuid);
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member AddressFKInfo.InnerInfo.ReadOnly")]
		[ReadOnlyMember("AddressFKInfo.InnerInfo.ReadOnly")]
		[List("OrganisationsList")]
		public ZGuid OrgPK
		{
			get
			{
				if (!OrgPKInitialized)
				{
					OrgPKInitialized = true;
					if (OrgAddress != null)
					{
						fOrgPK = OrgAddressAsIOrgAddress.OrganisationPK;
					}
					AddressFKInfo.ValueChanged += AddressFKInfo_ValueChanged;
				}

				return fOrgPK;
			}
			set
			{
				var oldValue = OrgPK;
				if (oldValue != value)
				{
					SetOrgWithoutSettingDefaultAddress(value);
					SetDefaultAddress();
					OrgPKInfo.RefreshBinding(oldValue);
				}
			}
		}

		internal bool OrgPKInitialized { get; private set; }
		ZGuid fOrgPK;

		public IEnumerable OrganisationsList
		{
			get { return MetaData.GetListDataSource(AddressFKInfo.InnerInfo.BizObj, AddressFKInfo.InnerInfo.PropertyDescriptor); }
		}

		public ZPropertyInfo OrgPKInfo
		{
			get { return GetZPropertyInfo(nameof(OrgPK)); }
		}

		public void ValidateOrgPK()
		{
			OrgPKInfo.ClearAllNotifications();

			if (OrgPKValidation != null)
			{
				OrgPKValidation(OrgPKInfo);
			}
		}

		#region Validate

		void ValidateDelegate()
		{
			if (IsOrgVisible)
			{
				if (OrgPK.IsMissing)
				{
					AddressFKInfo.AddError(Res.GetString("218bde24-2d67-416e-8bb4-a7590283cebc", "The selected organization is not valid. Please choose a new organization or amend the organization using F3."));
				}
				else if (!OrgPK.IsValid && !OrgPK.IsEmpty)
				{
					string invalidOrgErrorMessage = string.Format(TypeValidation.InvalidTypeMessage, Res.GetString("1ffc386d-14f0-49aa-867f-39b2ad23bf3a", "Organization"));
					AddressFKInfo.AddError(invalidOrgErrorMessage);
				}
				else if (OrgPK.IsValid && AddressFKInfo.Value.IsEmpty) // only do this if the Org is visible to the user
				{
					if (AddressIsMandatoryIfOrgIsValid)
					{
						string noAddressForOrgError = string.Format(TypeValidation.InvalidTypeMessage, TypeValidation.GetHumanReadablePropertyName(AddressFKInfo));
						AddressFKInfo.AddError(noAddressForOrgError); // to ensure users don't enter an Org and assume they will have an address saved
					}
					else
					{
						string noAddressForOrgWarning = Res.GetString("3866204e-6c8d-4397-b3a4-b62135c231a8", "The current organization will not be saved because no address is selected.");
						AddressFKInfo.AddWarning(noAddressForOrgWarning); // to ensure users don't enter an Org and assume they will have an address saved
					}
				}
			}

			if (!AddressFKInfo.InnerInfo.BizObj.IsDeleted)
			{
				var list = MetaData.GetListDataSource(AddressFKInfo.InnerInfo.BizObj, AddressFKInfo.InnerInfo.PropertyDescriptor) as IBusinessObjectCollection;
				if (list != null && !typeof(IOrgAddress).IsAssignableFrom(list.TypeOfElements))
				{
					// Ensure there is validation about inactive address
					ListValidation.ErrorIfCancelledAndEditable(AddressFKInfo.InnerInfo, OrgAddress_List);
				}
			}
		}

		#endregion

		#endregion

		#region Org Address List

		ZAddressList IZAddress.AddressList
		{
			get { return OrgAddress_List; }
		}

		public ZAddressList OrgAddress_List
		{
			get { return GetOrgAddress_List(); }
		}

		ZAddressList GetOrgAddress_List()
		{
			ZAddressList list = OrgHeader != null ? (ZAddressList)OrgHeaderAsIOrgHeader.Address_List : new ZAddressList();
			AddSelectedInactiveAddress(list);
			if (AddresssListOverride != null)
			{
				list = AddresssListOverride(Factory, list);
			}
			return list;
		}

		public AddresssListOverrideDelegate AddresssListOverride;
		public delegate ZAddressList AddresssListOverrideDelegate(BusinessObjectFactory factory, ZAddressList addressList);

		void AddSelectedInactiveAddress(ZAddressList list)
		{
			if (OrgAddress != null && !OrgAddress.IsDeleted && OrgAddress[OrgAddressSchema.OA_IsActive].Equals(false) &&
				list.List.Cast<ZAddressItem>().All(item => item.PK != OrgAddress.PK))
			{
				ZString headerInactive = Res.GetString("ce3a87e2-83b2-44d9-87ea-730eb93c3214", "Inactive");
				ZString addressCode = headerInactive + ": " + OrgAddressAsIOrgAddress.OA_Code;
				ZString addressDescription = headerInactive + ": " + OrgAddressAsIOrgAddress.AddressDetailedOnSingleLine;
				list.AddAddress(OrgAddress.PK, addressCode, addressDescription, new AddressCapabilityItem { Capability = headerInactive, IsDefault = false });
			}
		}

		#endregion

		public bool AddressIsMandatoryIfOrgIsValid { get; set; }

		protected bool IsAddressReadOnly
		{
			get { return !OrgPK.IsValid; }
		}

		protected bool GetPropertyReadonlyness(PropertyDescriptor property)
		{
			bool result = false;
			if (property.Name == addressFKInfo.Name)
			{
				result = IsAddressReadOnly;
			}
			result = result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
			return result;
		}

		#region Implementation

		public ZGuid AddressFK
		{
			get { return (ZGuid)AddressFKInfo.InnerInfo.Value; }
			set { AddressFKInfo.InnerInfo.Value = value; }
		}

		public ZWrappedPropertyInfo AddressFKInfo
		{
			get { return addressFKInfo; }
		}
		readonly ZWrappedPropertyInfo addressFKInfo;

		void AddressFKInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateOrgPK();
		}

		void UpdateOrgPK()
		{
			if (OrgAddress != null)
			{
				SetOrgWithoutSettingDefaultAddress(OrgAddressAsIOrgAddress.OrganisationPK);
			}
			AddressInfo.RefreshBinding();
			PhoneInfo.RefreshBinding();
			MobileInfo.RefreshBinding();
			WebInfo.RefreshBinding();
			EmailInfo.RefreshBinding();
			FaxInfo.RefreshBinding();
		}

		bool IZAddress.IsDeleted
		{
			get { return this.IsDeleted; }
		}

		#region Default Address

		public DefaultAddressHandler GetDefaultAddress;
		public delegate ZGuid DefaultAddressHandler(IOrgHeader orgHeader);

		void SetDefaultAddress()
		{
			ZGuid value = GetDefaultAddress != null ? GetDefaultAddress(OrgHeaderAsIOrgHeader) : GetDefaultAddressFromDefaultAddressType();
			AddressFKInfo.Value = value;
			AddressFKInfo.RefreshBinding(); // OuterAddressInfo.Value uses BusinessObject indexer which does not raise the changed event (so do this manually)
			if (!AddressFKInfo.Value.Equals(value)) // when the OuterAddressInfo.Value has an overridden get method
			{
				UpdateOrgPK();
			}
		}

		ZGuid GetDefaultAddressFromDefaultAddressType()
		{
			ZGuid value = ZGuid.Empty;
			switch (DefaultAddressType)
			{
				case AddressType.NoDefault:
					value = ZGuid.Empty;
					break;
				case AddressType.APM:
					value = OrgAddress_List.APMAddressOrFallback;
					break;
				case AddressType.ARM:
					value = OrgAddress_List.ARMAddressOrFallback;
					break;
				case AddressType.DLV:
					value = OrgAddress_List.DLVAddressOrFallback;
					break;
				case AddressType.PIC:
					value = OrgAddress_List.PICAddressOrFallback;
					break;
				case AddressType.SQM:
					value = OrgAddress_List.SQMAddressOrFallback;
					break;
				case AddressType.OFC:
					value = OrgAddress_List.OFCAddressOrFallback;
					break;
				case AddressType.CST:
					value = OrgAddress_List.CSTAddressOrFallback;
					break;
			}
			return value;
		}

		AddressType fDefaultAddressType;

		#endregion

		#region Org is Visible

		public ZBool IsOrgVisible
		{
			get { return fIsOrgVisible; }
			set
			{
				fIsOrgVisible = value;
				// if (value) update everything as we need to now consider the org
			}
		}

		ZBool fIsOrgVisible;

		#endregion

		#region IOrgAddress

		protected IOrgAddress OrgAddressAsIOrgAddress
		{
			get { return (IOrgAddress)OrgAddress; }
		}

		public BusinessObject OrgAddress
		{
			get
			{
				ZGuid addressPK = (ZGuid)AddressFKInfo.Value;

				if (fOrgAddress == null || fOrgAddress.IsDeleted || addressPK != fOrgAddress.PK)
				{
					fOrgAddress = (BusinessObject)Factory.Load<IOrgAddress>(addressPK);
				}
				return fOrgAddress;
			}
		}

		BusinessObject fOrgAddress;

		#endregion

		#region IOrgHeader

		protected IOrgHeader OrgHeaderAsIOrgHeader
		{
			get { return (IOrgHeader)OrgHeader; }
		}

		public virtual BusinessObject OrgHeader
		{
			get { return (BusinessObject)Factory.Load<IOrgHeader>(OrgPK); }
		}

		#endregion

		#endregion
	}
}
