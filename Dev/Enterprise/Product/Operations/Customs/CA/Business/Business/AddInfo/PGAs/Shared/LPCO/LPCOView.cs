using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class LPCOView : NonPersistentBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public LPCOView(CusCALPCO lpco) : base(lpco.Factory)
		{
			Argument.NotNull(lpco, nameof(lpco));
			this.lpco = lpco;
			RegisterEditableChildObject(lpco);
		}

		public LPCOView(CusCALPCO lpco, IPGAHeader pgaHeader) : this(lpco)
		{
			this.pgaHeader = pgaHeader;
		}

		readonly CusCALPCO lpco;
		readonly IPGAHeader pgaHeader;

		public ZBool HasPGAHeader => pgaHeader != null;
		public CusCALPCO LPCO => lpco;
		public OrgAddress Applicant => LPCO.Applicant;
		public OrgAddress Holder => LPCO.Holder;
		public OrgHeader OthLPCOApplicant => LPCO.OthLPCOApplicant;
		public OrgHeader OthLPCOHolder => LPCO.OthLPCOHolder;
		PGAContactDetails GetHolderPartyContactDetails(ZString partyType, Func<OrgHeader> getOTHLPCOParty) => LPCO.GetHolderPartyContactDetails(partyType, getOTHLPCOParty);
		OrgHeader GetHolderParty(string partyType, Func<OrgHeader> getOTHLPCOParty) => LPCO.GetHolderParty(partyType, getOTHLPCOParty);

		#region Properties

		[MaxLength(CusCALPCO.Schema.CLP_TypeMaxLength)]
		public ZString CLP_Type
		{
			get { return LPCO.CLP_Type; }
			set
			{
				var oldValue = LPCO.CLP_Type;
				if (oldValue != value)
				{
					LPCO.CLP_Type = value;
					if (LPCO.Parent is JobDeclaration && !LPCO.AgencyIDCode.IsEmpty)
					{
						ClearReadOnlyFields();
					}
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateCLP_Type();
				}
				CLP_TypeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CLP_TypeInfo
		{
			get { return GetZPropertyInfo(nameof(CLP_Type)); }
		}

		[MaxLength(CusCALPCO.Schema.CLP_RefNoMaxLength)]
		[ReadOnlyMember(nameof(CLP_RefNo_ReadOnly))]
		[ResourceStringData("LPCOView|CLP_RefNo", Caption = "Ref No", FullDescription = "A permit, movement document, allowance or allowance transfer will have a unique number assigned and this enables validation of the permit and/or the shipment reported to ensure the document is valid and not expired.")]
		public ZString CLP_RefNo
		{
			get { return LPCO.CLP_RefNo; }
			set
			{
				var oldValue = LPCO.CLP_RefNo;
				LPCO.CLP_RefNo = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCLP_RefNo();
				}
				CLP_RefNoInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CLP_RefNoInfo
		{
			get { return GetZPropertyInfo(nameof(CLP_RefNo)); }
		}

		[MaxLength(CusCALPCO.Schema.CLP_HolderTypeMaxLength)]
		[ReadOnlyMember(nameof(CLP_HolderType_ReadOnly))]
		public ZString CLP_HolderType
		{
			get { return LPCO.CLP_HolderType; }
			set
			{
				var oldValue = LPCO.CLP_HolderType;
				LPCO.CLP_HolderType = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCLP_HolderType();
				}
				CLP_HolderTypeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CLP_HolderTypeInfo
		{
			get { return GetZPropertyInfo(nameof(CLP_HolderType)); }
		}

		[MaxLength(CusCALPCO.Schema.CLP_HolderContactNameMaxLength)]
		[ReadOnlyMember(nameof(CLP_HolderContactName_ReadOnly))]
		public ZString CLP_HolderContactName
		{
			get
			{
				if (cachedHolderContactName == null)
				{
					cachedHolderContactName = new CachedProperty<ZString>(Factory, () =>
					{
						var result = ZString.Empty;

						if (CLP_IsHolderOverridden)
						{
							result = LPCO.CLP_HolderContactName;
						}
						else
						{
							var contactDetails = GetHolderPartyContactDetails(CLP_HolderType, () => OthLPCOHolder);
							result = contactDetails?.ContactName ?? ZString.Empty;
						}

						return result;
					});
				}

				return cachedHolderContactName.Value;
			}
			set
			{
				var oldValue = LPCO.CLP_HolderContactName;
				LPCO.CLP_HolderContactName = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCLP_AuthorizedPartyContactName();
				}
				CLP_HolderContactNameInfo.RefreshBinding(oldValue);
			}
		}
		CachedProperty<ZString> cachedHolderContactName;

		public ZPropertyInfo CLP_HolderContactNameInfo
		{
			get { return GetZPropertyInfo(nameof(CLP_HolderContactName)); }
		}

		[MaxLength(CusCALPCO.Schema.CLP_HolderContactPhoneMaxLength)]
		[ReadOnlyMember(nameof(CLP_HolderContactPhone_ReadOnly))]
		public ZString CLP_HolderContactPhone
		{
			get
			{
				if (cachedHolderContactPhone == null)
				{
					cachedHolderContactPhone = new CachedProperty<ZString>(Factory, () =>
					{
						var result = ZString.Empty;

						if (CLP_IsHolderOverridden)
						{
							result = LPCO.CLP_HolderContactPhone;
						}
						else
						{
							var contactDetails = GetHolderPartyContactDetails(CLP_HolderType, () => OthLPCOHolder);
							result = contactDetails?.PhoneNumber ?? ZString.Empty;
						}

						return result;
					});
				}

				return cachedHolderContactPhone.Value;
			}
			set
			{
				var oldValue = LPCO.CLP_HolderContactPhone;
				LPCO.CLP_HolderContactPhone = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCLP_AuthorizedPartyContactPhone();
				}
				CLP_HolderContactPhoneInfo.RefreshBinding(oldValue);
			}
		}
		CachedProperty<ZString> cachedHolderContactPhone;

		public ZPropertyInfo CLP_HolderContactPhoneInfo
		{
			get { return GetZPropertyInfo(nameof(CLP_HolderContactPhone)); }
		}

		[MaxLength(CusCALPCO.Schema.CLP_HolderContactEmailMaxLength)]
		[ReadOnlyMember(nameof(CLP_HolderContactEmail_ReadOnly))]
		public ZString CLP_HolderContactEmail
		{
			get
			{
				if (cachedHolderContactEmail == null)
				{
					cachedHolderContactEmail = new CachedProperty<ZString>(Factory, () =>
					{
						var result = ZString.Empty;

						if (CLP_IsHolderOverridden)
						{
							result = LPCO.CLP_HolderContactEmail;
						}
						else
						{
							var contactDetails = GetHolderPartyContactDetails(CLP_HolderType, () => OthLPCOHolder);
							result = contactDetails?.EmailAddress ?? ZString.Empty;
						}

						return result;
					});
				}

				return cachedHolderContactEmail.Value;
			}
			set
			{
				var oldValue = LPCO.CLP_HolderContactEmail;
				LPCO.CLP_HolderContactEmail = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCLP_AuthorizedPartyContactEmail();
				}
				CLP_HolderContactEmailInfo.RefreshBinding(oldValue);
			}
		}
		CachedProperty<ZString> cachedHolderContactEmail;

		public ZPropertyInfo CLP_HolderContactEmailInfo
		{
			get { return GetZPropertyInfo(nameof(CLP_HolderContactEmail)); }
		}

		[ReadOnlyMember(nameof(CLP_IsHolderOverridden_ReadOnly))]
		public ZBool CLP_IsHolderOverridden
		{
			get => LPCO.CLP_IsHolderOverridden;
			set
			{
				var oldValue = LPCO.CLP_IsHolderOverridden;
				LPCO.CLP_IsHolderOverridden = value;
				CLP_IsHolderOverriddenInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CLP_IsHolderOverriddenInfo
		{
			get { return GetZPropertyInfo(nameof(CLP_IsHolderOverridden)); }
		}

		[MaxLength(CusCALPCO.Schema.CLP_ApplicantNameMaxLength)]
		[ReadOnlyMember(nameof(CLP_ApplicantName_ReadOnly))]
		public ZString CLP_ApplicantName
		{
			get
			{
				if (cachedApplicantName == null)
				{
					cachedApplicantName = new CachedProperty<ZString>(Factory, () =>
					{
						var result = ZString.Empty;

						if (CLP_IsApplicantOverridden)
						{
							result = LPCO.CLP_ApplicantName;
						}
						else
						{
							var applicant = GetHolderParty(CLP_ApplicantType, () => OthLPCOApplicant);
							if (applicant != null)
							{
								var addressCompanyNameOverride = Applicant?.OA_CompanyNameOverride ?? ZString.Empty;
								result = (!addressCompanyNameOverride.IsEmpty ? addressCompanyNameOverride : applicant.OH_FullName).Left(JobDocAddress.Schema.E2_CompanyNameMaxLength);
							}
						}

						return result;
					});
				}
				return cachedApplicantName.Value;
			}
			set
			{
				var oldValue = LPCO.CLP_ApplicantName;
				LPCO.CLP_ApplicantName = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCLP_LPCOApplicantName();
				}
				CLP_ApplicantNameInfo.RefreshBinding(oldValue);
			}
		}
		CachedProperty<ZString> cachedApplicantName;

		public ZPropertyInfo CLP_ApplicantNameInfo
		{
			get { return GetZPropertyInfo(nameof(CLP_ApplicantName)); }
		}

		[ReadOnlyMember(nameof(CLP_OA_Applicant_ReadOnly))]
		public ZGuid CLP_OA_Applicant
		{
			get { return LPCO.CLP_OA_Applicant; }
			set
			{
				var oldValue = LPCO.CLP_OA_Applicant;
				LPCO.CLP_OA_Applicant = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCLP_OA_LPCOApplicant();
				}
				CLP_OA_ApplicantInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CLP_OA_ApplicantInfo
		{
			get { return GetZPropertyInfo(nameof(CLP_OA_Applicant)); }
		}

		[MaxLength(CusCALPCO.Schema.CLP_HolderNameMaxLength)]
		[ReadOnlyMember(nameof(CLP_HolderName_ReadOnly))]
		public ZString CLP_HolderName
		{
			get
			{
				if (cachedHolderName == null)
				{
					cachedHolderName = new CachedProperty<ZString>(Factory, () =>
					{
						var result = ZString.Empty;

						if (CLP_IsHolderOverridden)
						{
							result = LPCO.CLP_HolderName;
						}
						else
						{
							var holder = GetHolderParty(CLP_HolderType, () => OthLPCOHolder);
							if (holder != null)
							{
								var addressCompanyNameOverride = Holder?.OA_CompanyNameOverride ?? ZString.Empty;
								result = (!addressCompanyNameOverride.IsEmpty ? addressCompanyNameOverride : holder.OH_FullName).Left(JobDocAddress.Schema.E2_CompanyNameMaxLength);
							}
						}

						return result;
					});
				}
				return cachedHolderName.Value;
			}
			set
			{
				var oldValue = LPCO.CLP_HolderName;
				LPCO.CLP_HolderName = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCLP_LPCOHolderName();
				}
				CLP_HolderNameInfo.RefreshBinding(oldValue);
			}
		}
		CachedProperty<ZString> cachedHolderName;

		public ZPropertyInfo CLP_HolderNameInfo
		{
			get { return GetZPropertyInfo(nameof(CLP_HolderName)); }
		}

		[ReadOnlyMember(nameof(CLP_OA_Holder_ReadOnly))]
		public ZGuid CLP_OA_Holder
		{
			get { return LPCO.CLP_OA_Holder; }
			set
			{
				var oldValue = LPCO.CLP_OA_Holder;
				LPCO.CLP_OA_Holder = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCLP_OA_LPCOHolder();
				}
				CLP_OA_HolderInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CLP_OA_HolderInfo
		{
			get { return GetZPropertyInfo(nameof(CLP_OA_Holder)); }
		}

		public ZString CLP_OA_LPCOHolder_Address
		{
			get { return LPCO.CLP_OA_Holder_ZAddress.OrgPK.IsValid ? LPCO.CLP_OA_Holder_ZAddress.AddressDetailed.Replace(System.Environment.NewLine, ", ").TrimEndIncludingWhiteSpace(',') : ZString.Empty; }
		}

		public ZPropertyInfo CLP_OA_LPCOHolder_AddressInfo
		{
			get { return GetZPropertyInfo(nameof(CLP_OA_LPCOHolder_Address)); }
		}

		[MaxLength(CusCALPCO.Schema.CLP_SecondaryRefNoMaxLength)]
		[ReadOnlyMember(nameof(CLP_SecondaryRefNo_ReadOnly))]
		public ZString CLP_SecondaryRefNo
		{
			get { return LPCO.CLP_SecondaryRefNo; }
			set
			{
				var oldValue = LPCO.CLP_SecondaryRefNo;
				LPCO.CLP_SecondaryRefNo = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCLP_SecondaryRefNo();
				}
				CLP_SecondaryRefNoInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CLP_SecondaryRefNoInfo
		{
			get { return GetZPropertyInfo(nameof(CLP_SecondaryRefNo)); }
		}

		[ReadOnlyMember(nameof(CLP_IssueDate_ReadOnly))]
		public ZDate CLP_IssueDate
		{
			get { return LPCO.CLP_IssueDate; }
			set
			{
				var oldValue = LPCO.CLP_IssueDate;
				LPCO.CLP_IssueDate = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCLP_IssueDate();
				}
				CLP_IssueDateInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CLP_IssueDateInfo
		{
			get { return GetZPropertyInfo(nameof(CLP_IssueDate)); }
		}

		[MaxLength(CusCALPCO.Schema.CLP_RN_NKIssuanceCountryCodeMaxLength)]
		[ReadOnlyMember(nameof(CLP_RN_NKIssuanceCountryCode_ReadOnly))]
		public ZString CLP_RN_NKIssuanceCountryCode
		{
			get { return LPCO.CLP_RN_NKIssuanceCountryCode; }
			set
			{
				var oldValue = LPCO.CLP_RN_NKIssuanceCountryCode;
				LPCO.CLP_RN_NKIssuanceCountryCode = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCLP_RN_NKIssuanceCountryCode();
				}
				CLP_RN_NKIssuanceCountryCodeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CLP_RN_NKIssuanceCountryCodeInfo
		{
			get { return GetZPropertyInfo(nameof(CLP_RN_NKIssuanceCountryCode)); }
		}

		[MaxLength(CusCALPCO.Schema.CLP_RN_NKOriginCountryCodeMaxLength)]
		[ReadOnlyMember(nameof(CLP_RN_NKOriginCountryCode_ReadOnly))]
		public ZString CLP_RN_NKOriginCountryCode
		{
			get { return LPCO.CLP_RN_NKOriginCountryCode; }
			set
			{
				var oldValue = LPCO.CLP_RN_NKOriginCountryCode;
				LPCO.CLP_RN_NKOriginCountryCode = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCLP_RN_NKOriginCountryCode();
				}
				CLP_RN_NKOriginCountryCodeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CLP_RN_NKOriginCountryCodeInfo
		{
			get { return GetZPropertyInfo(nameof(CLP_RN_NKOriginCountryCode)); }
		}

		[MaxLength(CusCALPCO.Schema.CLP_RN_NKAuthorizationCountryMaxLength)]
		[ReadOnlyMember(nameof(CLP_RN_NKAuthorizationCountry_ReadOnly))]
		public ZString CLP_RN_NKAuthorizationCountry
		{
			get { return LPCO.CLP_RN_NKAuthorizationCountry; }
			set
			{
				var oldValue = LPCO.CLP_RN_NKAuthorizationCountry;
				LPCO.CLP_RN_NKAuthorizationCountry = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCLP_RN_NKAuthorizationCountry();
				}
				CLP_RN_NKAuthorizationCountryInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CLP_RN_NKAuthorizationCountryInfo
		{
			get { return GetZPropertyInfo(nameof(CLP_RN_NKAuthorizationCountry)); }
		}

		[MaxLength(CusCALPCO.Schema.CLP_RN_NKSmeltAndPourCountryCodeMaxLength)]
		public ZString CLP_RN_NKSmeltAndPourCountryCode
		{
			get { return LPCO.CLP_RN_NKSmeltAndPourCountryCode; }
			set
			{
				var oldValue = LPCO.CLP_RN_NKSmeltAndPourCountryCode;
				LPCO.CLP_RN_NKSmeltAndPourCountryCode = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCLP_RN_NKSmeltAndPourCountryCode();
				}
				CLP_RN_NKSmeltAndPourCountryCodeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CLP_RN_NKSmeltAndPourCountryCodeInfo
		{
			get { return GetZPropertyInfo(nameof(CLP_RN_NKSmeltAndPourCountryCode)); }
		}

		[ReadOnlyMember(nameof(CLP_EndDate_ReadOnly))]
		public ZDate CLP_EndDate
		{
			get { return LPCO.CLP_EndDate; }
			set
			{
				var oldValue = LPCO.CLP_EndDate;
				LPCO.CLP_EndDate = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCLP_EndDate();
				}
				CLP_EndDateInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CLP_EndDateInfo
		{
			get { return GetZPropertyInfo(nameof(CLP_EndDate)); }
		}

		[ReadOnlyMember(nameof(CLP_StartDate_ReadOnly))]
		public ZDate CLP_StartDate
		{
			get { return LPCO.CLP_StartDate; }
			set
			{
				var oldValue = LPCO.CLP_StartDate;
				LPCO.CLP_StartDate = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCLP_StartDate();
				}
				CLP_StartDateInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CLP_StartDateInfo
		{
			get { return GetZPropertyInfo(nameof(CLP_StartDate)); }
		}

		[MaxLength(CusCALPCO.Schema.CLP_ApplicantTypeMaxLength)]
		[ReadOnlyMember(nameof(CLP_ApplicantType_ReadOnly))]
		public ZString CLP_ApplicantType
		{
			get { return LPCO.CLP_ApplicantType; }
			set
			{
				var oldValue = LPCO.CLP_ApplicantType;
				LPCO.CLP_ApplicantType = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCLP_ApplicantType();
				}
				CLP_ApplicantTypeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CLP_ApplicantTypeInfo
		{
			get { return GetZPropertyInfo(nameof(CLP_ApplicantType)); }
		}

		[ReadOnlyMember(nameof(CLP_AlternativeQuotaQuantity_ReadOnly))]
		public ZDecimal CLP_AlternativeQuotaQuantity
		{
			get { return LPCO.CLP_AlternativeQuotaQuantity; }
			set
			{
				var oldValue = LPCO.CLP_AlternativeQuotaQuantity;
				LPCO.CLP_AlternativeQuotaQuantity = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCLP_AlternativeQuotaQuantity();
				}
				CLP_AlternativeQuotaQuantityInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CLP_AlternativeQuotaQuantityInfo
		{
			get { return GetZPropertyInfo(nameof(CLP_AlternativeQuotaQuantity)); }
		}

		[MaxLength(CusCALPCO.Schema.CLP_DIFRefNumberOrLocationMaxLength)]
		[ReadOnlyMember(nameof(CLP_DIFRefNumberOrLocation_ReadOnly))]
		public ZString CLP_DIFRefNumberOrLocation
		{
			get { return LPCO.CLP_DIFRefNumberOrLocation; }
			set
			{
				var oldValue = LPCO.CLP_DIFRefNumberOrLocation;
				LPCO.CLP_DIFRefNumberOrLocation = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCLP_DIFRefNumberOrLocation();
				}
				CLP_DIFRefNumberOrLocationInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CLP_DIFRefNumberOrLocationInfo
		{
			get { return GetZPropertyInfo(nameof(CLP_DIFRefNumberOrLocation)); }
		}

		[MaxLength(CusCALPCO.Schema.CLP_CommodityTypeCodeMaxLength)]
		[ReadOnlyMember(nameof(CLP_CommodityTypeCode_ReadOnly))]
		public ZString CLP_CommodityTypeCode
		{
			get { return LPCO.CLP_CommodityTypeCode; }
			set
			{
				var oldValue = LPCO.CLP_CommodityTypeCode;
				LPCO.CLP_CommodityTypeCode = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCLP_CommodityTypeCode();
				}
				CLP_CommodityTypeCodeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CLP_CommodityTypeCodeInfo
		{
			get { return GetZPropertyInfo(nameof(CLP_CommodityTypeCode)); }
		}

		[ReadOnlyMember(nameof(CLP_IsMixedCountryOfOrigin_ReadOnly))]
		public ZBool CLP_IsMixedCountryOfOrigin
		{
			get { return LPCO.CLP_IsMixedCountryOfOrigin; }
			set
			{
				var oldValue = LPCO.CLP_IsMixedCountryOfOrigin;
				LPCO.CLP_IsMixedCountryOfOrigin = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCLP_IsMixedCountryOfOrigin();
				}
				CLP_IsMixedCountryOfOriginInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CLP_IsMixedCountryOfOriginInfo
		{
			get { return GetZPropertyInfo(nameof(CLP_IsMixedCountryOfOrigin)); }
		}

		[ReadOnlyMember(nameof(CLP_OA_Holder_ReadOnly))]
		public ZGuid LPCOHolderOrgPK
		{
			get { return LPCO.LPCOHolderOrgPK; }
			set
			{
				var oldValue = LPCO.LPCOHolderOrgPK;
				LPCO.LPCOHolderOrgPK = value;
				LPCOHolderOrgPKInfo.RefreshBinding(oldValue);
				LPCOHolderOrgCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo LPCOHolderOrgPKInfo
		{
			get { return GetZPropertyInfo(nameof(LPCOHolderOrgPK)); }
		}

		public ZString LPCOHolderOrgCode
		{
			get { return CLP_HolderType == LPCOHolderPartyTypeCodes.Codes.Other && LPCO.OthLPCOHolder != null ? LPCO.OthLPCOHolder.OH_Code : ZString.Empty; }
		}

		public ZPropertyInfo LPCOHolderOrgCodeInfo
		{
			get { return GetZPropertyInfo(nameof(LPCOHolderOrgCode)); }
		}

		[MaxLength(CusCALPCO.Schema.CLP_AlternativeQuotaUQMaxLength)]
		[ReadOnlyMember(nameof(CLP_AlternativeQuotaUQ_ReadOnly))]
		public ZString CLP_AlternativeQuotaUQ
		{
			get { return LPCO.CLP_AlternativeQuotaUQ; }
			set
			{
				var oldValue = LPCO.CLP_AlternativeQuotaUQ;
				LPCO.CLP_AlternativeQuotaUQ = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCLP_AlternativeQuotaUQ();
				}
				CLP_AlternativeQuotaUQInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CLP_AlternativeQuotaUQInfo
		{
			get { return GetZPropertyInfo(nameof(CLP_AlternativeQuotaUQ)); }
		}

		[ReadOnlyMember(nameof(CLP_OA_Applicant_ReadOnly))]
		public ZGuid LPCOApplicantOrgPK
		{
			get { return LPCO.LPCOApplicantOrgPK; }
			set
			{
				var oldValue = LPCO.LPCOApplicantOrgPK;
				LPCO.LPCOApplicantOrgPK = value;
				LPCOApplicantOrgPKInfo.RefreshBinding(oldValue);
				LPCOApplicantOrgCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo LPCOApplicantOrgPKInfo
		{
			get { return GetZPropertyInfo(nameof(LPCOApplicantOrgPK)); }
		}

		public ZString LPCOApplicantOrgCode
		{
			get { return CLP_ApplicantType == LPCOHolderPartyTypeCodes.Codes.Other && LPCO.OthLPCOApplicant != null ? LPCO.OthLPCOApplicant.OH_Code : ZString.Empty; }
		}

		public ZPropertyInfo LPCOApplicantOrgCodeInfo
		{
			get { return GetZPropertyInfo(nameof(LPCOApplicantOrgCode)); }
		}

		public ZString CLP_OA_LPCOApplicant_Address
		{
			get { return LPCO.CLP_OA_Applicant_ZAddress.OrgPK.IsValid ? LPCO.CLP_OA_Applicant_ZAddress.AddressDetailed.Replace(System.Environment.NewLine, ", ").TrimEndIncludingWhiteSpace(',') : ZString.Empty; }
		}

		public ZPropertyInfo CLP_OA_LPCOApplicant_AddressInfo
		{
			get { return GetZPropertyInfo(nameof(CLP_OA_LPCOApplicant_Address)); }
		}

		[MaxLength(CusCALPCO.Schema.CLP_ApplicantContactNameMaxLength)]
		[ReadOnlyMember(nameof(CLP_ApplicantContactName_ReadOnly))]
		public ZString CLP_ApplicantContactName
		{
			get
			{
				if (cachedApplicantContactName == null)
				{
					cachedApplicantContactName = new CachedProperty<ZString>(Factory, () =>
					{
						var result = ZString.Empty;

						if (CLP_IsApplicantOverridden)
						{
							result = LPCO.CLP_ApplicantContactName;
						}
						else
						{
							var contactDetails = GetHolderPartyContactDetails(CLP_ApplicantType, () => OthLPCOApplicant);
							result = contactDetails?.ContactName ?? ZString.Empty;
						}

						return result;
					});
				}

				return cachedApplicantContactName.Value;
			}
			set
			{
				var oldValue = LPCO.CLP_ApplicantContactName;
				LPCO.CLP_ApplicantContactName = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCLP_ApplicantContactName();
				}
				CLP_ApplicantContactNameInfo.RefreshBinding(oldValue);
			}
		}
		CachedProperty<ZString> cachedApplicantContactName;

		public ZPropertyInfo CLP_ApplicantContactNameInfo
		{
			get { return GetZPropertyInfo(nameof(CLP_ApplicantContactName)); }
		}

		[MaxLength(CusCALPCO.Schema.CLP_ApplicantContactPhoneMaxLength)]
		[ReadOnlyMember(nameof(CLP_ApplicantContactPhone_ReadOnly))]
		public ZString CLP_ApplicantContactPhone
		{
			get
			{
				if (cachedApplicantContactPhone == null)
				{
					cachedApplicantContactPhone = new CachedProperty<ZString>(Factory, () =>
					{
						var result = ZString.Empty;

						if (CLP_IsApplicantOverridden)
						{
							result = LPCO.CLP_ApplicantContactPhone;
						}
						else
						{
							var contactDetails = GetHolderPartyContactDetails(CLP_ApplicantType, () => OthLPCOApplicant);
							result = contactDetails?.PhoneNumber ?? ZString.Empty;
						}

						return result;
					});
				}

				return cachedApplicantContactPhone.Value;
			}
			set
			{
				var oldValue = LPCO.CLP_ApplicantContactPhone;
				LPCO.CLP_ApplicantContactPhone = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCLP_ApplicantContactPhone();
				}
				CLP_ApplicantContactPhoneInfo.RefreshBinding(oldValue);
			}
		}
		CachedProperty<ZString> cachedApplicantContactPhone;

		public ZPropertyInfo CLP_ApplicantContactPhoneInfo
		{
			get { return GetZPropertyInfo(nameof(CLP_ApplicantContactPhone)); }
		}

		[MaxLength(CusCALPCO.Schema.CLP_ApplicantContactEmailMaxLength)]
		[ReadOnlyMember(nameof(CLP_ApplicantContactEmail_ReadOnly))]
		public ZString CLP_ApplicantContactEmail
		{
			get
			{
				if (cachedApplicantContactEmail == null)
				{
					cachedApplicantContactEmail = new CachedProperty<ZString>(Factory, () =>
					{
						var result = ZString.Empty;

						if (CLP_IsApplicantOverridden)
						{
							result = LPCO.CLP_ApplicantContactEmail;
						}
						else
						{
							var contactDetails = GetHolderPartyContactDetails(CLP_ApplicantType, () => OthLPCOApplicant);
							result = contactDetails?.EmailAddress ?? ZString.Empty;
						}

						return result;
					});
				}

				return cachedApplicantContactEmail.Value;
			}
			set
			{
				var oldValue = LPCO.CLP_ApplicantContactEmail;
				LPCO.CLP_ApplicantContactEmail = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCLP_ApplicantContactEmail();
				}
				CLP_ApplicantContactEmailInfo.RefreshBinding(oldValue);
			}
		}
		CachedProperty<ZString> cachedApplicantContactEmail;

		public ZPropertyInfo CLP_ApplicantContactEmailInfo
		{
			get { return GetZPropertyInfo(nameof(CLP_ApplicantContactEmail)); }
		}

		[ReadOnlyMember(nameof(CLP_IsApplicantOverridden_ReadOnly))]
		public ZBool CLP_IsApplicantOverridden
		{
			get => LPCO.CLP_IsApplicantOverridden;
			set
			{
				var oldValue = LPCO.CLP_IsApplicantOverridden;
				LPCO.CLP_IsApplicantOverridden = value;
				CLP_IsApplicantOverriddenInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CLP_IsApplicantOverriddenInfo
		{
			get { return GetZPropertyInfo(nameof(CLP_IsApplicantOverridden)); }
		}
		#endregion

		#region ReadOnly Properties
		ZBool CLP_AlternativeQuotaQuantity_ReadOnly => GetReadonlyProperty(CusCALPCO.Schema.CLP_AlternativeQuotaQuantity);
		ZBool CLP_RefNo_ReadOnly => GetReadonlyProperty(CusCALPCO.Schema.CLP_RefNo);
		ZBool CLP_HolderType_ReadOnly => GetReadonlyProperty(CusCALPCO.Schema.CLP_HolderType);
		ZBool CLP_HolderName_ReadOnly => !CLP_IsHolderOverridden || GetReadonlyProperty(CusCALPCO.Schema.CLP_HolderName);
		ZBool CLP_OA_Holder_ReadOnly => LPCO.DoesLPCOHolderReferenceDecOrgs || GetReadonlyProperty(CusCALPCO.Schema.CLP_OA_Holder);
		ZBool CLP_ApplicantType_ReadOnly => GetReadonlyProperty(CusCALPCO.Schema.CLP_ApplicantType);
		ZBool CLP_ApplicantName_ReadOnly => !CLP_IsApplicantOverridden || GetReadonlyProperty(CusCALPCO.Schema.CLP_ApplicantName);
		ZBool CLP_OA_Applicant_ReadOnly => LPCO.DoesLPCOApplicantReferenceDecOrgs || GetReadonlyProperty(CusCALPCO.Schema.CLP_OA_Applicant);
		ZBool CLP_SecondaryRefNo_ReadOnly => LPCO.IsGACNonForeignExportLicense || GetReadonlyProperty(CusCALPCO.Schema.CLP_SecondaryRefNo);
		ZBool CLP_IssueDate_ReadOnly => LPCO.IsGACNonForeignExportLicense || GetReadonlyProperty(CusCALPCO.Schema.CLP_IssueDate);
		ZBool CLP_RN_NKAuthorizationCountry_ReadOnly => GetReadonlyProperty(CusCALPCO.Schema.CLP_RN_NKAuthorizationCountry);
		ZBool CLP_CommodityTypeCode_ReadOnly => GetReadonlyProperty(CusCALPCO.Schema.CLP_CommodityTypeCode);
		ZBool CLP_HolderContactEmail_ReadOnly => !CLP_IsHolderOverridden || GetReadonlyProperty(CusCALPCO.Schema.CLP_HolderContactEmail);
		ZBool CLP_HolderContactName_ReadOnly => !CLP_IsHolderOverridden || GetReadonlyProperty(CusCALPCO.Schema.CLP_HolderContactName);
		ZBool CLP_HolderContactPhone_ReadOnly => !CLP_IsHolderOverridden || GetReadonlyProperty(CusCALPCO.Schema.CLP_HolderContactPhone);
		ZBool CLP_RN_NKIssuanceCountryCode_ReadOnly => GetReadonlyProperty(CusCALPCO.Schema.CLP_RN_NKIssuanceCountryCode);
		ZBool CLP_RN_NKOriginCountryCode_ReadOnly => GetReadonlyProperty(CusCALPCO.Schema.CLP_RN_NKOriginCountryCode);
		ZBool CLP_DIFRefNumberOrLocation_ReadOnly => GetReadonlyProperty(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation);
		ZBool CLP_IsMixedCountryOfOrigin_ReadOnly => GetReadonlyProperty(CusCALPCO.Schema.CLP_IsMixedCountryOfOrigin);
		ZBool CLP_EndDate_ReadOnly => GetReadonlyProperty(CusCALPCO.Schema.CLP_EndDate);
		ZBool CLP_StartDate_ReadOnly => GetReadonlyProperty(CusCALPCO.Schema.CLP_StartDate);
		ZBool CLP_ApplicantContactEmail_ReadOnly => !CLP_IsApplicantOverridden || GetReadonlyProperty(CusCALPCO.Schema.CLP_ApplicantContactEmail);
		ZBool CLP_ApplicantContactName_ReadOnly => !CLP_IsApplicantOverridden || GetReadonlyProperty(CusCALPCO.Schema.CLP_ApplicantContactName);
		ZBool CLP_ApplicantContactPhone_ReadOnly => !CLP_IsApplicantOverridden || GetReadonlyProperty(CusCALPCO.Schema.CLP_ApplicantContactPhone);
		ZBool CLP_AlternativeQuotaUQ_ReadOnly => GetReadonlyProperty(CusCALPCO.Schema.CLP_AlternativeQuotaUQ);
		ZBool CLP_IsHolderOverridden_ReadOnly => false;
		ZBool CLP_IsApplicantOverridden_ReadOnly => false;

		ZBool GetReadonlyProperty(ZString column)
		{
			var result = false;
			if ((LPCO.Parent is JobDeclaration) && !CLP_Type.IsEmpty && AvailableLPCOFieldsDictionaryForEachPGA.ContainsKey(LPCO.AgencyIDCode))
			{
				var availableLPCOFields = AvailableLPCOFieldsDictionaryForEachPGA[LPCO.AgencyIDCode];
				result = availableLPCOFields.Any() && !availableLPCOFields.Contains(column);
			}
			return result;
		}

		static ImmutableDictionary<string, ImmutableList<string>> AvailableLPCOFieldsDictionaryForEachPGA { get; } = new Dictionary<string, ImmutableList<string>>
			{
				{ PGACodes.Codes.CFIA, CFIAPGAHeader.AvailableLPCOFields },
				{ PGACodes.Codes.GAC, GACPGAHeader.AvailableLPCOFields },
				{ PGACodes.Codes.HC, HCPGAHeader.AvailableLPCOFields },
				{ PGACodes.Codes.TC, TCPGAHeader.AvailableLPCOFields },
				{ PGACodes.Codes.DFO, DFOPGAHeader.AvailableLPCOFields },
				{ PGACodes.Codes.NRCan, NRCanPGAHeader.AvailableLPCOFields },
				{ PGACodes.Codes.ECCC, ECCCPGAHeader.AvailableLPCOFields },
				{ PGACodes.Codes.PHAC, PHACPGAHeader.AvailableLPCOFields },
				{ PGACodes.Codes.CNSC, CNSCPGAHeader.AvailableLPCOFields }
			}.ToImmutableDictionary();

		void ClearReadOnlyFields()
		{
			var checkPropertyInfos = new[]
			{
				CLP_RN_NKAuthorizationCountryInfo,
				CLP_CommodityTypeCodeInfo,
				CLP_HolderContactEmailInfo,
				CLP_HolderContactNameInfo,
				CLP_HolderContactPhoneInfo,
				CLP_IsHolderOverriddenInfo,
				CLP_RN_NKIssuanceCountryCodeInfo,
				CLP_RN_NKOriginCountryCodeInfo,
				CLP_DIFRefNumberOrLocationInfo,
				CLP_IsMixedCountryOfOriginInfo,
				CLP_ApplicantTypeInfo,
				CLP_EndDateInfo,
				CLP_HolderTypeInfo,
				CLP_IssueDateInfo,
				CLP_StartDateInfo,
				CLP_ApplicantContactEmailInfo,
				CLP_ApplicantContactNameInfo,
				CLP_ApplicantContactPhoneInfo,
				CLP_IsApplicantOverriddenInfo,
				CLP_AlternativeQuotaQuantityInfo,
				CLP_RefNoInfo,
				CLP_SecondaryRefNoInfo,
				CLP_AlternativeQuotaUQInfo
			};

			foreach (var info in checkPropertyInfos)
			{
				if (info.ReadOnly)
				{
					info.ClearValue();
				}
			}
		}
		#endregion

		#region Validation
		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			if (!LPCO.IsDeleted)
			{
				Validation.ValidateAll();
			}
		}

		public IPGAProgramRequirementProvider PGAHeader => pgaHeader as IPGAProgramRequirementProvider ?? LPCO.Parent as IPGAProgramRequirementProvider;

		public LPCOViewValidation Validation
		{
			get
			{
				var agencyCode = (PGAHeader as IPGAHeader)?.GovAgencyIDCode ?? ZString.Empty;
				switch (agencyCode)
				{
					case PGACodes.Codes.TC:
						{
							return new TCLPCOViewValidation(this, this.pgaHeader);
						}
					case PGACodes.Codes.ECCC:
						{
							return new ECCCLPCOViewValidation(this, this.pgaHeader);
						}
					case PGACodes.Codes.HC:
						{
							return new HCLPCOViewValidation(this, this.pgaHeader);
						}
					case PGACodes.Codes.GAC:
						{
							return new GACLPCOViewValidation(this, this.pgaHeader);
						}
					default:
						{
							return new LPCOViewValidation(this, this.pgaHeader);
						}
				}
			}
		}

		protected override bool IsValidationEnabledCore(ZPropertyInfo propertyInfo)
		{
			return base.IsValidationEnabledCore(propertyInfo) && LPCO.IsValidationEnabled(propertyInfo);
		}

		#endregion
	}
}
