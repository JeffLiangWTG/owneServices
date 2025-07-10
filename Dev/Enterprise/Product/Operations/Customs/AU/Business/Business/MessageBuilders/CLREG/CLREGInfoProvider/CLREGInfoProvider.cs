using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.AU.Declaration.Business
{
#pragma warning disable IDE0001 // Prevent simplification of explicit generic type
	public class CLREGInfoProvider : Customs.Business.MultiLineAddInfos.CusAddInfo<CLREGInfoProviderAddInfo>
	#pragma warning restore IDE0001 // Prevent simplification of explicit generic type
		, ISupplyCLREGInfo
		, ICusAddInfoTypeSupporter
		, Integration.Customs.AU.ICLREGInfoProvider
	{
		public CLREGInfoProvider(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema
		#pragma warning disable IDE0001 // Prevent simplification of explicit generic type
		public new class Schema : Customs.Business.MultiLineAddInfos.CusAddInfo<CLREGInfoProviderAddInfo>.Schema
		#pragma warning restore IDE0001 // Prevent simplification of explicit generic type
		{
			public const string ZA_ABN = AUCLREGInfoProviderAddInfoSchema.Constants.ZA_ABN;
			public const string ZA_ABNInd = AUCLREGInfoProviderAddInfoSchema.Constants.ZA_ABNInd;
			public const string ZA_Bsn1 = AUCLREGInfoProviderAddInfoSchema.Constants.ZA_Bsn1;
			public const string ZA_Bsn2 = AUCLREGInfoProviderAddInfoSchema.Constants.ZA_Bsn2;
			public const string ZA_BsnCity = AUCLREGInfoProviderAddInfoSchema.Constants.ZA_BsnCity;
			public const string ZA_BsnPort = AUCLREGInfoProviderAddInfoSchema.Constants.ZA_BsnPort;
			public const string ZA_BsnPostCode = AUCLREGInfoProviderAddInfoSchema.Constants.ZA_BsnPostCode;
			public const string ZA_BsnState = AUCLREGInfoProviderAddInfoSchema.Constants.ZA_BsnState;
			public const string ZA_BusinessName = AUCLREGInfoProviderAddInfoSchema.Constants.ZA_BusinessName;
			public const string ZA_CAC = AUCLREGInfoProviderAddInfoSchema.Constants.ZA_CAC;
			public const string ZA_CACType = AUCLREGInfoProviderAddInfoSchema.Constants.ZA_CACType;
			public const string ZA_DateofBirth = AUCLREGInfoProviderAddInfoSchema.Constants.ZA_DateofBirth;
			public const string ZA_FamilyName = AUCLREGInfoProviderAddInfoSchema.Constants.ZA_FamilyName;
			public const string ZA_FirstName = AUCLREGInfoProviderAddInfoSchema.Constants.ZA_FirstName;
			public const string ZA_Gender = AUCLREGInfoProviderAddInfoSchema.Constants.ZA_Gender;
			public const string ZA_IsEvidenceOfID = AUCLREGInfoProviderAddInfoSchema.Constants.ZA_IsEvidenceOfID;
			public const string ZA_IsExDocsUser = AUCLREGInfoProviderAddInfoSchema.Constants.ZA_IsExDocsUser;
			public const string ZA_IsIndiv = AUCLREGInfoProviderAddInfoSchema.Constants.ZA_IsIndiv;
			public const string ZA_IsOrg = AUCLREGInfoProviderAddInfoSchema.Constants.ZA_IsOrg;
			public const string ZA_Post1 = AUCLREGInfoProviderAddInfoSchema.Constants.ZA_Post1;
			public const string ZA_Post2 = AUCLREGInfoProviderAddInfoSchema.Constants.ZA_Post2;
			public const string ZA_PostCity = AUCLREGInfoProviderAddInfoSchema.Constants.ZA_PostCity;
			public const string ZA_PostPort = AUCLREGInfoProviderAddInfoSchema.Constants.ZA_PostPort;
			public const string ZA_PostPostCode = AUCLREGInfoProviderAddInfoSchema.Constants.ZA_PostPostCode;
			public const string ZA_PostState = AUCLREGInfoProviderAddInfoSchema.Constants.ZA_PostState;
			public const string ZA_SecondName = AUCLREGInfoProviderAddInfoSchema.Constants.ZA_SecondName;
			public const string ZA_Suffix = AUCLREGInfoProviderAddInfoSchema.Constants.ZA_Suffix;
			public const string ZA_Title = AUCLREGInfoProviderAddInfoSchema.Constants.ZA_Title;
		}
		#endregion

		public CLREGContactInfoProvider CLREGContactInfoProvider
		{
			get
			{
				if (clREGContactInfoProvider == null)
				{
					var query = new ZQuery(CusAddInfoSchema.B7_Type, CusAddInfoTypeAttribute.Codes.AUREGContact);
					query.AddToFilter(CusAddInfoSchema.B7_ParentID, this.PK);
					clREGContactInfoProvider = Factory.LoadTop1<CLREGContactInfoProvider>(query);

					if (clREGContactInfoProvider == null)
					{
						clREGContactInfoProvider = Factory.New<CLREGContactInfoProvider>();
						clREGContactInfoProvider.B7_Type = CusAddInfoTypeAttribute.Codes.AUREGContact;
						clREGContactInfoProvider.B7_ParentID = this.PK;
						clREGContactInfoProvider.B7_ParentTableCode = this.TablePrefix;
					}
					RegisterEditableChildObject(clREGContactInfoProvider);
				}
				return clREGContactInfoProvider;
			}
		}
		CLREGContactInfoProvider clREGContactInfoProvider;

		[ChildEditable(true)]
		public TravelDocumentCollection TravelDocuments
		{
			get
			{
				if (travelDocs == null)
				{
					travelDocs = new TravelDocumentCollection(this);
					travelDocs.Load();
					RegisterEditableChildObject(travelDocs);
				}
				return travelDocs;
			}
		}
		TravelDocumentCollection travelDocs;

		[ChildEditable(true)]
		public RollCollection Rolls
		{
			get
			{
				if (rolls == null)
				{
					rolls = new RollCollection(this);
					rolls.Load();
					rolls.CountChanged += rolls_CountChanged;
					RegisterEditableChildObject(rolls);
				}
				return rolls;
			}
		}
		RollCollection rolls;

		void rolls_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (!IsValidationSuspended)
			{
				AddInfoValidation.ValidateZA_IsExDocsUser();
				AddInfoValidation.ValidateZA_ABN();
			}
		}

		public void SetUpDefaultValuesIfNeeded(OrgHeaderWrapper orgWrapper)
		{
			var organizationHeader = orgWrapper.OrgHeader;
			if (!organizationHeader.LocalBusinessRegNo.IsEmpty)
			{
				var localBusinessNumber = organizationHeader.LocalBusinessRegNo.Replace(" ", "");
				if (ZA_ABN.IsEmpty)
				{
					ZA_ABN = localBusinessNumber.SubstringSafe(0, 11);
				}

				if (ZA_CAC.IsEmpty)
				{
					ZA_CAC = localBusinessNumber.SubstringSafe(11, 3);
				}

				if (ZA_CACType.IsEmpty)
				{
					ZA_CACType = localBusinessNumber.SubstringSafe(14, 2);
				}
			}

			if (ZA_BusinessName.IsEmpty)
			{
				ZA_BusinessName = organizationHeader.OH_FullNameTruncated;
			}

			organizationAddresses = organizationHeader.Addresses;
			organizationPK = organizationHeader.PK;
			organizationMainAddress = organizationHeader.MainAddress;
			contacts = organizationHeader.Contacts;
			messages = orgWrapper.Messages;
		}

		public OrgAddressDependentCollection OrganizationAddresses
		{
			get { return organizationAddresses; }
		}
		OrgAddressDependentCollection organizationAddresses;

		public OrgContactDependentCollection Contacts
		{
			get { return contacts; }
		}
		OrgContactDependentCollection contacts;

		public OrgAddress OrganizationMainAddress
		{
			get { return organizationMainAddress; }
		}
		OrgAddress organizationMainAddress;

		#region AddInfo Properties

		public ZBool ZA_IsIndiv
		{
			get { return AddInfo.ZA_IsIndiv; }
			set
			{
				AddInfo.ZA_IsIndiv = value;
				ZA_IsIndivInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ZA_IsIndivInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_IsIndiv, x => AddInfo.ZA_IsIndivInfo); }
		}

		public ZBool ZA_IsOrg
		{
			get { return AddInfo.ZA_IsOrg; }
			set
			{
				AddInfo.ZA_IsOrg = value;
				ZA_IsOrgInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ZA_IsOrgInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_IsOrg, x => AddInfo.ZA_IsOrgInfo); }
		}

		public ZString ZA_ABN
		{
			get { return AddInfo.ZA_ABN; }
			set
			{
				AddInfo.ZA_ABN = value;
				if (AddInfo.ZA_ABN.IsEmpty)
				{
					ZA_ABNInd = ZString.Empty;
				}
			}
		}

		public ZPropertyInfo ZA_ABNInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_ABN, x => AddInfo.ZA_ABNInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AUCLREGInfoProviderAddInfoLookups.ABNNominatedClientTypeList))]
		public ZString ZA_ABNInd
		{
			get { return AddInfo.ZA_ABNInd; }
			set { AddInfo.ZA_ABNInd = value; }
		}

		public ZBool IsABNIndividual
		{
			get { return ZA_ABNInd == "I"; }
		}

		public ZPropertyInfo ZA_ABNIndInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_ABNInd, x => AddInfo.ZA_ABNIndInfo); }
		}

		public ZString ZA_CAC
		{
			get { return AddInfo.ZA_CAC; }
			set
			{
				AddInfo.ZA_CAC = value;
				if (AddInfo.ZA_CAC.IsEmpty)
				{
					ZA_Post1 = ZString.Empty;
					ZA_Post2 = ZString.Empty;
					ZA_PostCity = ZString.Empty;
					ZA_PostPort = ZString.Empty;
					ZA_PostPostCode = ZString.Empty;
					ZA_PostState = ZString.Empty;
				}
			}
		}

		public ZPropertyInfo ZA_CACInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_CAC, x => AddInfo.ZA_CACInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AUCLREGInfoProviderAddInfoLookups.CACTypeList))]
		public ZString ZA_CACType
		{
			get { return AddInfo.ZA_CACType; }
			set { AddInfo.ZA_CACType = value; }
		}

		public ZPropertyInfo ZA_CACTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_CACType, x => AddInfo.ZA_CACTypeInfo); }
		}

		public ZString ZA_Title
		{
			get { return AddInfo.ZA_Title; }
			set { AddInfo.ZA_Title = value; }
		}

		public ZPropertyInfo ZA_TitleInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_Title, x => AddInfo.ZA_TitleInfo); }
		}

		public ZString ZA_FirstName
		{
			get { return AddInfo.ZA_FirstName; }
			set { AddInfo.ZA_FirstName = value; }
		}

		public ZPropertyInfo ZA_FirstNameInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_FirstName, x => AddInfo.ZA_FirstNameInfo); }
		}

		public ZString ZA_SecondName
		{
			get { return AddInfo.ZA_SecondName; }
			set { AddInfo.ZA_SecondName = value; }
		}

		public ZPropertyInfo ZA_SecondNameInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_SecondName, x => AddInfo.ZA_SecondNameInfo); }
		}

		public ZString ZA_FamilyName
		{
			get { return AddInfo.ZA_FamilyName; }
			set { AddInfo.ZA_FamilyName = value; }
		}

		public ZPropertyInfo ZA_FamilyNameInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_FamilyName, x => AddInfo.ZA_FamilyNameInfo); }
		}

		public ZString ZA_Suffix
		{
			get { return AddInfo.ZA_Suffix; }
			set { AddInfo.ZA_Suffix = value; }
		}

		public ZPropertyInfo ZA_SuffixInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_Suffix, x => AddInfo.ZA_SuffixInfo); }
		}

		public ZString ZA_BusinessName
		{
			get { return AddInfo.ZA_BusinessName; }
			set { AddInfo.ZA_BusinessName = value; }
		}

		public ZPropertyInfo ZA_BusinessNameInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_BusinessName, x => AddInfo.ZA_BusinessNameInfo); }
		}

		public ZBool ZA_IsEvidenceOfID
		{
			get { return AddInfo.ZA_IsEvidenceOfID; }
			set { AddInfo.ZA_IsEvidenceOfID = value; }
		}

		public ZBool ZA_IsExDocsUser
		{
			get { return AddInfo.ZA_IsExDocsUser; }
			set
			{
				AddInfo.ZA_IsExDocsUser = value;
				ZA_IsExDocsUserInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ZA_IsExDocsUserInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_IsExDocsUser, x => AddInfo.ZA_IsExDocsUserInfo); }
		}

		#region Business Address

		[List(nameof(AddInfoLookups) + "." + nameof(AUCLREGInfoProviderAddInfoLookups.Addresses))]
		public ZGuid ZA_OA_BusinessAddress
		{
			get { return oa_BusinessAddress; }
			set
			{
				oa_BusinessAddress = value;

				var address = GetAddress(oa_BusinessAddress);
				if (address != null)
				{
					ZA_Bsn1 = GetBsn1FromOrgAddress(address);
					ZA_Bsn2 = GetBsn2FromOrgAddress(address);
					ZA_BsnCity = GetBsnCityFromOrgAddress(address);
					ZA_BsnPostCode = GetBsnPostCodeFromOrgAddress(address);
					ZA_BsnPort = GetBsnPortFromOrgAddress(address);
					ZA_BsnState = GetBsnStateFromOrgAddress(address);
				}
			}
		}
		ZGuid oa_BusinessAddress;

		internal static ZString GetBsn1FromOrgAddress(OrgAddress address) => address.OA_Address1.Left(CLREGInfoProviderAddInfo.Schema.ZA_Bsn1MaxLength);

		internal static ZString GetBsn2FromOrgAddress(OrgAddress address) => address.OA_Address2.Left(CLREGInfoProviderAddInfo.Schema.ZA_Bsn2MaxLength);

		internal static ZString GetBsnCityFromOrgAddress(OrgAddress address) => address.OA_City.Left(CLREGInfoProviderAddInfo.Schema.ZA_BsnCityMaxLength);

		internal static ZString GetBsnPostCodeFromOrgAddress(OrgAddress address) => address.OA_PostCode;

		internal static ZString GetBsnPortFromOrgAddress(OrgAddress address) => address.OA_RL_NKRelatedPortCode;

		internal static ZString GetBsnStateFromOrgAddress(OrgAddress address) => address.OA_State.Left(CLREGInfoProviderAddInfo.Schema.ZA_BsnStateMaxLength);

		public OrgAddress GetAddress(ZGuid addressPK)
		{
			return Factory.Load<OrgAddress>(addressPK);
		}

		public ZString ZA_Bsn1
		{
			get { return AddInfo.ZA_Bsn1; }
			set { AddInfo.ZA_Bsn1 = value; }
		}

		public ZPropertyInfo ZA_Bsn1Info
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_Bsn1, x => AddInfo.ZA_Bsn1Info); }
		}

		public ZString ZA_Bsn2
		{
			get { return AddInfo.ZA_Bsn2; }
			set { AddInfo.ZA_Bsn2 = value; }
		}

		public ZPropertyInfo ZA_Bsn2Info
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_Bsn2, x => AddInfo.ZA_Bsn2Info); }
		}

		public ZString ZA_BsnCity
		{
			get { return AddInfo.ZA_BsnCity; }
			set { AddInfo.ZA_BsnCity = value; }
		}

		public ZPropertyInfo ZA_BsnCityInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_BsnCity, x => AddInfo.ZA_BsnCityInfo); }
		}

		public ZString ZA_BsnPostCode
		{
			get { return AddInfo.ZA_BsnPostCode; }
			set { AddInfo.ZA_BsnPostCode = value; }
		}

		public ZPropertyInfo ZA_BsnPostCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_BsnPostCode, x => AddInfo.ZA_BsnPostCodeInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AUCLREGInfoProviderAddInfoLookups.RefUNLOCOs))]
		public ZString ZA_BsnPort
		{
			get { return AddInfo.ZA_BsnPort; }
			set
			{
				AddInfo.ZA_BsnPort = value;
				AddInfoValidation.ValidateZA_BsnPostCode();
			}
		}

		public ZPropertyInfo ZA_BsnPortInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_BsnPort, x => AddInfo.ZA_BsnPortInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AUCLREGInfoProviderAddInfoLookups.OA_StateListForBusinessAddress))]
		public ZString ZA_BsnState
		{
			get { return AddInfo.ZA_BsnState; }
			set { AddInfo.ZA_BsnState = value; }
		}

		public ZPropertyInfo ZA_BsnStateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_BsnState, x => AddInfo.ZA_BsnStateInfo); }
		}

		#endregion

		#region Postal Address

		[List(nameof(AddInfoLookups) + "." + nameof(AUCLREGInfoProviderAddInfoLookups.Addresses))]
		public ZGuid ZA_OA_PostalAddress
		{
			get { return oa_PostalAddress; }
			set
			{
				oa_PostalAddress = value;

				var address = GetAddress(oa_PostalAddress);
				if (address != null)
				{
					ZA_Post1 = address.OA_Address1.Left(ZA_Post1Info.MaxLength);
					ZA_Post2 = address.OA_Address2.Left(ZA_Post2Info.MaxLength);
					ZA_PostCity = address.OA_City.Left(ZA_PostCityInfo.MaxLength);
					ZA_PostPostCode = address.OA_PostCode;
					ZA_PostPort = address.OA_RL_NKRelatedPortCode;
					ZA_PostState = address.OA_State.Left(ZA_PostStateInfo.MaxLength);
				}
			}
		}
		ZGuid oa_PostalAddress;

		public ZString ZA_Post1
		{
			get { return AddInfo.ZA_Post1; }
			set { AddInfo.ZA_Post1 = value; }
		}

		public ZPropertyInfo ZA_Post1Info
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_Post1, x => AddInfo.ZA_Post1Info); }
		}

		public ZString ZA_Post2
		{
			get { return AddInfo.ZA_Post2; }
			set { AddInfo.ZA_Post2 = value; }
		}

		public ZPropertyInfo ZA_Post2Info
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_Post2, x => AddInfo.ZA_Post2Info); }
		}

		public ZString ZA_PostCity
		{
			get { return AddInfo.ZA_PostCity; }
			set { AddInfo.ZA_PostCity = value; }
		}

		public ZPropertyInfo ZA_PostCityInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_PostCity, x => AddInfo.ZA_PostCityInfo); }
		}

		public ZString ZA_PostPostCode
		{
			get { return AddInfo.ZA_PostPostCode; }
			set { AddInfo.ZA_PostPostCode = value; }
		}

		public ZPropertyInfo ZA_PostPostCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_PostPostCode, x => AddInfo.ZA_PostPostCodeInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AUCLREGInfoProviderAddInfoLookups.RefUNLOCOs))]
		public ZString ZA_PostPort
		{
			get { return AddInfo.ZA_PostPort; }
			set
			{
				AddInfo.ZA_PostPort = value;
				AddInfoValidation.ValidateZA_PostPostCode();
			}
		}

		public ZPropertyInfo ZA_PostPortInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_PostPort, x => AddInfo.ZA_PostPortInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AUCLREGInfoProviderAddInfoLookups.OA_StateListForPostalAddress))]
		public ZString ZA_PostState
		{
			get { return AddInfo.ZA_PostState; }
			set { AddInfo.ZA_PostState = value; }
		}

		public ZPropertyInfo ZA_PostStateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_PostState, x => AddInfo.ZA_PostStateInfo); }
		}

		#endregion

		public ZDateTime ZA_DateofBirth
		{
			get { return AddInfo.ZA_DateofBirth; }
			set { AddInfo.ZA_DateofBirth = value; }
		}

		public ZPropertyInfo ZA_DateofBirthInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_DateofBirth, x => AddInfo.ZA_DateofBirthInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AUCLREGInfoProviderAddInfoLookups.GenderList))]
		public ZString ZA_Gender
		{
			get { return AddInfo.ZA_Gender; }
			set { AddInfo.ZA_Gender = value; }
		}

		public ZPropertyInfo ZA_GenderInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_Gender, x => AddInfo.ZA_GenderInfo); }
		}

		#endregion

		#region AddInfo object/Validation and Lookups objects

		public AUCLREGInfoProviderAddInfoLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		public AUCLREGInfoProviderAddInfoValidation AddInfoValidation
		{
			get { return AddInfo.Validation; }
		}

		CLREGInfoProviderAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new CLREGInfoProviderAddInfo(B7_AddInfoDataInfo);
					RegisterEditableChildObject(fAddInfo);
					fAddInfo.CLREGInfoProvider = this;
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		CLREGInfoProviderAddInfo fAddInfo;

		#endregion

		#region ISupplyCLREGInfo Members

		ZGuid ISupplyCLREGInfo.OrganizationPK
		{
			get { return organizationPK; }
		}
		ZGuid organizationPK;

		EDIMessageCollection ISupplyCLREGInfo.Messages
		{
			get { return messages; }
		}
		EDIMessageCollection messages;

		public ZBool IsIndividual
		{
			get { return ZA_IsIndiv; }
		}

		public ZBool IsOrganisation
		{
			get { return ZA_IsOrg; }
		}

		public ZString ABN
		{
			get { return ZA_ABN; }
		}

		public ZString CAC
		{
			get { return ZA_CAC; }
		}

		ZString ISupplyCLREGInfo.CACType
		{
			get { return ZA_CACType; }
		}

		ZString ISupplyCLREGInfo.Title
		{
			get { return ZA_Title; }
		}

		ZString ISupplyCLREGInfo.FirstName
		{
			get { return ZA_FirstName; }
		}

		ZString ISupplyCLREGInfo.SecondName
		{
			get { return ZA_SecondName; }
		}

		ZString ISupplyCLREGInfo.FamilyName
		{
			get { return ZA_FamilyName; }
		}

		ZString ISupplyCLREGInfo.Suffix
		{
			get { return ZA_Suffix; }
		}

		ZString ISupplyCLREGInfo.ContactName
		{
			get { return CLREGContactInfoProvider.ZA_ContName; }
		}

		ZString ISupplyCLREGInfo.ContactPurpose
		{
			get { return CLREGContactInfoProvider.ZA_ContPurpose; }
		}

		ZString ISupplyCLREGInfo.BusinessName
		{
			get { return ZA_BusinessName; }
		}

		ZBool ISupplyCLREGInfo.IsEvidenceOfID
		{
			get { return ZA_IsEvidenceOfID; }
		}

		ZBool ISupplyCLREGInfo.IsExDocsUser
		{
			get { return ZA_IsExDocsUser; }
		}

		List<TravelDocument> ISupplyCLREGInfo.TravelDocuments
		{
			get
			{
				var result = new List<TravelDocument>();
				result.AddRange(new TypedEnumerable<TravelDocument>(TravelDocuments));
				return result;
			}
		}

		List<ZString> ISupplyCLREGInfo.Rolls
		{
			get
			{
				var result = new List<ZString>();
				foreach (Roll roll in Rolls)
				{
					result.Add(roll.ZA_Roll);
				}
				return result;
			}
		}

		ZString ISupplyCLREGInfo.BusinessAddress1
		{
			get { return ZA_Bsn1; }
		}

		ZString ISupplyCLREGInfo.BusinessAddress2
		{
			get { return ZA_Bsn2; }
		}

		ZString ISupplyCLREGInfo.BusinessAddressCity
		{
			get { return ZA_BsnCity; }
		}

		ZString ISupplyCLREGInfo.BusinessAddressPostCode
		{
			get { return ZA_BsnPostCode; }
		}

		ZString ISupplyCLREGInfo.BusinessAddressCountry
		{
			get { return ZA_BsnPort.SubstringSafe(0, 2); }
		}

		ZString ISupplyCLREGInfo.BusinessAddressState
		{
			get { return ZA_BsnState; }
		}

		ZString ISupplyCLREGInfo.PostalAddress1
		{
			get { return ZA_Post1; }
		}

		ZString ISupplyCLREGInfo.PostalAddress2
		{
			get { return ZA_Post2; }
		}

		ZString ISupplyCLREGInfo.PostalAddressCity
		{
			get { return ZA_PostCity; }
		}

		ZString ISupplyCLREGInfo.PostalAddressPostCode
		{
			get { return ZA_PostPostCode; }
		}

		ZString ISupplyCLREGInfo.PostalAddressCountry
		{
			get { return ZA_PostPort.SubstringSafe(0, 2); }
		}

		ZString ISupplyCLREGInfo.PostalAddressState
		{
			get { return ZA_PostState; }
		}

		ZString ISupplyCLREGInfo.ContactAddress1
		{
			get { return CLREGContactInfoProvider.ZA_Cont1; }
		}

		ZString ISupplyCLREGInfo.ContactAddress2
		{
			get { return CLREGContactInfoProvider.ZA_Cont2; }
		}

		ZString ISupplyCLREGInfo.ContactAddressCity
		{
			get { return CLREGContactInfoProvider.ZA_ContCity; }
		}

		ZString ISupplyCLREGInfo.ContactAddressPostCode
		{
			get { return CLREGContactInfoProvider.ZA_ContPostCode; }
		}

		ZString ISupplyCLREGInfo.ContactAddressCountry
		{
			get { return CLREGContactInfoProvider.ZA_ContPort.SubstringSafe(0, 2); }
		}

		ZString ISupplyCLREGInfo.ContactAddressState
		{
			get { return CLREGContactInfoProvider.ZA_ContState; }
		}

		ZString ISupplyCLREGInfo.ContactPostalAddress1
		{
			get { return CLREGContactInfoProvider.ZA_ContPost1; }
		}

		ZString ISupplyCLREGInfo.ContactPostalAddress2
		{
			get { return CLREGContactInfoProvider.ZA_ContPost2; }
		}

		ZString ISupplyCLREGInfo.ContactPostalAddressCity
		{
			get { return CLREGContactInfoProvider.ZA_ContPostCity; }
		}

		ZString ISupplyCLREGInfo.ContactPostalAddressPostCode
		{
			get { return CLREGContactInfoProvider.ZA_ContPostPostCode; }
		}

		ZString ISupplyCLREGInfo.ContactPostalAddressCountry
		{
			get { return CLREGContactInfoProvider.ZA_ContPostPort.SubstringSafe(0, 2); }
		}

		ZString ISupplyCLREGInfo.ContactPostalAddressState
		{
			get { return CLREGContactInfoProvider.ZA_ContPostState; }
		}

		ZString ISupplyCLREGInfo.ContactPhPrefix
		{
			get { return CLREGContactInfoProvider.ZA_ContPhPref; }
		}

		ZString ISupplyCLREGInfo.ContactPh
		{
			get { return CLREGContactInfoProvider.ZA_ContPh; }
		}

		ZString ISupplyCLREGInfo.ContactPhComment
		{
			get { return CLREGContactInfoProvider.ZA_ContPhComment; }
		}

		ZString ISupplyCLREGInfo.ContactFaxPrefix
		{
			get { return CLREGContactInfoProvider.ZA_ContFaxPref; }
		}

		ZString ISupplyCLREGInfo.ContactFax
		{
			get { return CLREGContactInfoProvider.ZA_ContFax; }
		}

		ZString ISupplyCLREGInfo.ContactFaxComment
		{
			get { return CLREGContactInfoProvider.ZA_ContFaxComment; }
		}

		ZString ISupplyCLREGInfo.ContactAHPrefix
		{
			get { return CLREGContactInfoProvider.ZA_ContAHPref; }
		}

		ZString ISupplyCLREGInfo.ContactAH
		{
			get { return CLREGContactInfoProvider.ZA_ContAH; }
		}

		ZString ISupplyCLREGInfo.ContactAHComment
		{
			get { return CLREGContactInfoProvider.ZA_ContAHComment; }
		}

		ZString ISupplyCLREGInfo.ContactMobile
		{
			get { return CLREGContactInfoProvider.ZA_ContMob; }
		}

		ZString ISupplyCLREGInfo.ContactMobileComment
		{
			get { return CLREGContactInfoProvider.ZA_ContMobComment; }
		}

		ZString ISupplyCLREGInfo.ContactEmail
		{
			get { return CLREGContactInfoProvider.ZA_ContEmail; }
		}

		ZDate ISupplyCLREGInfo.DateofBirth
		{
			get { return ZA_DateofBirth.Date; }
		}

		ZString ISupplyCLREGInfo.Gender
		{
			get { return ZA_Gender; }
		}

		#endregion

		#region ICusAddInfoTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.AUREGContact, typeof(CLREGContactInfoProvider));
			result.Add(CusAddInfoTypeAttribute.Codes.AUROLL, typeof(Roll));
			result.Add(CusAddInfoTypeAttribute.Codes.AUCLR, typeof(TravelDocument));
			return result;
		}

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			CLREGContactInfoProvider.AddInfoValidation.ValidateAll();
		}
	}
}
