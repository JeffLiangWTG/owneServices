using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.CA.DIF;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CusCALPCO : AutoCusCALPCO
	{
		public CusCALPCO(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			CLP_TypeInfo.HumanReadableName = Res.GetString("67C9E0A8-FF39-4051-A00B-F2E58D5D5B7F", "Document Type");
			CLP_RefNoInfo.HumanReadableName = Res.GetString("BBBEE743-74A8-4B6F-901B-424D7F833F82", "Ref No");
			CLP_DIFRefNumberOrLocationInfo.HumanReadableName = Res.GetString("743A7443-EE88-4015-BBF9-A14BD70A6F8F", "DIF URN");
		}

		#region Schema

		public new class Schema : AutoCusCALPCO.Schema
		{
			public const string LPCOHolderOrgPK = "LPCOHolderOrgPK";
			public const string LPCOApplicantOrgPK = "LPCOApplicantOrgPK";
		}

		#endregion

		#region Properties

		[List(nameof(Lookups) + "." + nameof(CusCALPCOLookups.DocumentTypeCodes))]
		public override ZString CLP_Type
		{
			get => base.CLP_Type;
			set
			{
				if (base.CLP_Type != value)
				{
					base.CLP_Type = value;
					if (!IsCopying)
					{
						if (GACPGAHeader != null)
						{
							if (Is_SecondaryRefNo_ReadOnly)
							{
								CLP_SecondaryRefNo = ZString.Empty;
							}
							if (Is_LPCOIssueDate_ReadOnly)
							{
								CLP_IssueDate = ZDate.Empty;
							}
						}
						else if (CLP_RefNo.IsEmpty)
						{
							if (CFIAPGAHeader != null)
							{
								if (RegistrationNumberHelper.IsConfirmationCFIALPCO(Factory, CLP_Type))
								{
									CLP_RefNo = YesNoList.Codes.Yes;
								}
								RegistrationNumberHelper.DefaultSafeFoodLicence(CLP_Type, (x) => CLP_RefNo = x, Lookups.RefNumbers);
							}
							else if (Parent is JobDeclaration)
							{
								RegistrationNumberHelper.DefaultSafeFoodLicence(CLP_Type, (x) => CLP_RefNo = x, Lookups.RefNumbers);
							}
						}
					}

					if (CLP_RefNo.IsEmpty && Parent is IPGAHeader pgaHeader)
					{
						var attributes = DocumentType?.GetAttributesValues(RefCusCodeListAttributeTypes.Codes.CADocumentTypeValuesAllowed).ToArray();
						if (attributes?.Length == 1)
						{
							CLP_RefNo = attributes[0];
						}
					}

					if (CLP_DIFRefNumberOrLocation.IsEmpty && !value.IsEmpty && (Parent as IDeclarationProvider)?.Declaration is ICADIFHost host)
					{
						var provider = ObjectFactory.Get<ICADIFDocumentProvider>();
						var documentList = provider.GetDIFDocuments(host).Where(x => x.DocumentType == value).ToArray();
						if (documentList.Length == 1)
						{
							CLP_DIFRefNumberOrLocation = documentList[0].URN;
						}
					}

					ValidateRelatedPropertiesFromDocumentType();
					cachedCFIALPCO = null;
				}
			}
		}

		public bool IsTypeSafeFoodForCanadiansLicence => CLP_Type == RegistrationNumberHelper.SafeFoodForCanadiansLicence;

		[List(nameof(Lookups) + "." + nameof(CusCALPCOLookups.RefNumbers))]
		public override ZString CLP_RefNo { get => base.CLP_RefNo; set => base.CLP_RefNo = value; }

		public override ZDecimal CLP_AlternativeQuotaQuantity
		{
			get => base.CLP_AlternativeQuotaQuantity;
			set
			{
				base.CLP_AlternativeQuotaQuantity = value;

				if (CLP_AlternativeQuotaQuantity > 0 &&
					CLP_AlternativeQuotaUQ.IsEmpty &&
					GACPGAHeader?.InvoiceLine is JobComInvoiceLine invoiceLine &&
					!invoiceLine.JI_CustomsUnitQty.IsEmpty)
				{
					CLP_AlternativeQuotaUQ = invoiceLine.JI_CustomsUnitQty.Left(3);
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusCALPCOLookups.HolderPartyTypeCodes))]
		public override ZString CLP_HolderType
		{
			get => base.CLP_HolderType;
			set
			{
				var oldValue = CLP_HolderType;
				base.CLP_HolderType = value;
				ActionWhenDataIsChangedAndIsNotCopying(oldValue, CLP_HolderType, (old, newValue) =>
				{
					DefaultHolderDetailsIfNeeded();
					ResetLPCOHolderPKIfTypeIsOther();
				});
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusCALPCOLookups.LPCOApplicantCodes))]
		public override ZString CLP_ApplicantType
		{
			get => base.CLP_ApplicantType;
			set
			{
				var oldValue = CLP_ApplicantType;
				base.CLP_ApplicantType = value;
				ActionWhenDataIsChangedAndIsNotCopying(oldValue, CLP_ApplicantType, (old, newValue) =>
				{
					DefaultApplicantDataIfNeeded();
					ResetLPCOApplicantPKIfTypeIsOther();
				});
			}
		}

		public override ZString CLP_DIFRefNumberOrLocation
		{
			get => base.CLP_DIFRefNumberOrLocation;
			set
			{
				var oldValue = CLP_DIFRefNumberOrLocation;
				base.CLP_DIFRefNumberOrLocation = value;
				ActionWhenDataIsChangedAndIsNotCopying(oldValue, CLP_DIFRefNumberOrLocation, (old, newValue) =>
				{
					if (!newValue.IsEmpty)
					{
						var declaration = (Parent as IDeclarationProvider)?.Declaration as JobDeclaration;
						var difDocument = ObjectFactory.Get<ICADIFDocumentProvider>().GetDIFDocument(Factory, value, Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF, declaration?.EffectiveBranch.Company.PK ?? GlbCompany.CurrentCompany.PK);
						if (difDocument != null)
						{
							UpdateLPCODetails(difDocument);
						}
					}
				});
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusCALPCOLookups.UQList))]
		public override ZString CLP_AlternativeQuotaUQ { get => base.CLP_AlternativeQuotaUQ; set => base.CLP_AlternativeQuotaUQ = value; }

		#endregion

		public BusinessObject Parent
		{
			get
			{
				if (fParent == null && !CLP_ParentTableCode.IsEmpty && !CLP_ParentID.IsEmpty)
				{
					fParent = Factory.Load(CLP_ParentTableCode, CLP_ParentID);
				}
				return fParent;
			}
			internal set
			{
				SetParent(value);
			}
		}
		BusinessObject fParent;

		void SetParent(BusinessObject parent)
		{
			fParent = parent;
			if (fParent != null && (CLP_ParentID != fParent.PK || CLP_ParentTableCode != fParent.TablePrefix))
			{
				CLP_ParentID = fParent.PK;
				CLP_ParentTableCode = fParent.TablePrefix;
			}
			if (parent == null)
			{
				ErrorReporter.ReportOnce("CusCALPCO-NullParent", string.Format("Parent of CusCALPCO was set to null."));
			}
		}

		public JobDeclaration Declaration
		{
			get
			{
				if (!IsDeleted)
				{
					switch (CLP_ParentTableCode)
					{
						case JobDeclarationSchema.Constants.Prefix:
							{
								return Parent as JobDeclaration;
							}

						case CusAddInfoSchema.Constants.Prefix:
							{
								var pgaHeader = Parent as IPGAHeader;
								var invoiceLine = pgaHeader?.Parent as JobComInvoiceLine;
								return invoiceLine?.Declaration;
							}
					}
				}

				return null;
			}
		}

		GACPGAHeader GACPGAHeader
		{
			get
			{
				GACPGAHeader result = null;
				var header = Parent as IPGAHeader;
				if (header != null && header.GovAgencyIDCode == PGACodes.Codes.GAC)
				{
					result = Parent as GACPGAHeader;
				}
				return result;
			}
		}

		CFIAPGAHeader CFIAPGAHeader
		{
			get
			{
				CFIAPGAHeader result = null;
				var header = Parent as IPGAHeader;
				if (header != null && header.GovAgencyIDCode == PGACodes.Codes.CFIA)
				{
					result = Parent as CFIAPGAHeader;
				}
				return result;
			}
		}

		public OrgHeader OthLPCOHolder => (OrgHeader)CLP_OA_Holder_ZAddress.OrgHeader;

		public OrgHeader OthLPCOApplicant => (OrgHeader)CLP_OA_Applicant_ZAddress.OrgHeader;

		public ZGuid LPCOHolderOrgPK
		{
			get { return CLP_OA_Holder_ZAddress.OrgPK; }
			set
			{
				var oldValue = CLP_OA_Holder_ZAddress.OrgPK;
				CLP_OA_Holder_ZAddress.OrgPK = value;
				ActionWhenDataIsChangedAndIsNotCopying(oldValue, value, (old, newValue) =>
				{
					DefaultHolderContactDetails(CLP_HolderType);
				});
			}
		}

		public ZPropertyInfo LPCOHolderOrgPKInfo => GetWrappedZPropertyInfo(Schema.LPCOHolderOrgPK, x => CLP_OA_Holder_ZAddress.OrgPKInfo);

		protected override ZAddress GetNewCLP_OA_Holder_ZAddress()
		{
			var zAddress = base.GetNewCLP_OA_Holder_ZAddress();
			zAddress.IsOrgVisible = true;
			zAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(x => ((OrgHeader)x)?.MainAddress?.PK ?? ZGuid.Empty);
			return zAddress;
		}

		public override ZGuid CLP_OA_Holder
		{
			get => base.CLP_OA_Holder;
			set
			{
				var oldValue = base.CLP_OA_Holder;
				base.CLP_OA_Holder = value;
				ActionWhenDataIsChangedAndIsNotCopying(oldValue, value, (old, newValue) =>
				{
					UpdateLPCOHolderName();
				});
			}
		}

		[BusinessObjectTestExclude]
		public override ZString CLP_HolderName
		{
			get => base.CLP_HolderName;
			set => base.CLP_HolderName = CLP_IsHolderOverridden ? value : ZString.Empty;
		}

		[BusinessObjectTestExclude]
		public override ZString CLP_HolderContactName
		{
			get => base.CLP_HolderContactName;
			set => base.CLP_HolderContactName = CLP_IsHolderOverridden ? value : ZString.Empty;
		}

		[BusinessObjectTestExclude]
		public override ZString CLP_HolderContactEmail
		{
			get => base.CLP_HolderContactEmail;
			set => base.CLP_HolderContactEmail = CLP_IsHolderOverridden ? value : ZString.Empty;
		}

		[BusinessObjectTestExclude]
		public override ZString CLP_HolderContactPhone
		{
			get => base.CLP_HolderContactPhone;
			set => base.CLP_HolderContactPhone = CLP_IsHolderOverridden ? value : ZString.Empty;
		}

		public override ZBool CLP_IsHolderOverridden
		{
			get => base.CLP_IsHolderOverridden;
			set
			{
				var oldValue = base.CLP_IsHolderOverridden;
				base.CLP_IsHolderOverridden = value;
				ActionWhenDataIsChangedAndIsNotCopying(oldValue, value, (old, newValue) =>
				{
					DefaultHolderDetailsIfNeeded();
					ResetLPCOHolderPKIfTypeIsOther();
				});
			}
		}

		public ZGuid LPCOApplicantOrgPK
		{
			get { return CLP_OA_Applicant_ZAddress.OrgPK; }
			set
			{
				var oldValue = CLP_OA_Applicant_ZAddress.OrgPK;
				CLP_OA_Applicant_ZAddress.OrgPK = value;
				ActionWhenDataIsChangedAndIsNotCopying(oldValue, value, (old, newValue) =>
				{
					DefaultApplicantContactDetails(CLP_ApplicantType);
				});
			}
		}

		public ZPropertyInfo LPCOApplicantOrgPKInfo => GetWrappedZPropertyInfo(Schema.LPCOApplicantOrgPK, x => CLP_OA_Applicant_ZAddress.OrgPKInfo);

		protected override ZAddress GetNewCLP_OA_Applicant_ZAddress()
		{
			var zAddress = base.GetNewCLP_OA_Applicant_ZAddress();
			zAddress.IsOrgVisible = true;
			zAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(x => ((OrgHeader)x)?.MainAddress?.PK ?? ZGuid.Empty);
			return zAddress;
		}

		public override ZGuid CLP_OA_Applicant
		{
			get => base.CLP_OA_Applicant;
			set
			{
				var oldValue = base.CLP_OA_Applicant;
				base.CLP_OA_Applicant = value;
				ActionWhenDataIsChangedAndIsNotCopying(oldValue, value, (old, newValue) =>
				{
					UpdateLPCOApplicantName();
				});
			}
		}

		[BusinessObjectTestExclude]
		public override ZString CLP_ApplicantName
		{
			get => base.CLP_ApplicantName;
			set => base.CLP_ApplicantName = CLP_IsApplicantOverridden ? value : ZString.Empty;
		}

		[BusinessObjectTestExclude]
		public override ZString CLP_ApplicantContactName
		{
			get => base.CLP_ApplicantContactName;
			set => base.CLP_ApplicantContactName = CLP_IsApplicantOverridden ? value : ZString.Empty;
		}

		[BusinessObjectTestExclude]
		public override ZString CLP_ApplicantContactEmail
		{
			get => base.CLP_ApplicantContactEmail;
			set => base.CLP_ApplicantContactEmail = CLP_IsApplicantOverridden ? value : ZString.Empty;
		}

		[BusinessObjectTestExclude]
		public override ZString CLP_ApplicantContactPhone
		{
			get => base.CLP_ApplicantContactPhone;
			set => base.CLP_ApplicantContactPhone = CLP_IsApplicantOverridden ? value : ZString.Empty;
		}

		public override ZBool CLP_IsApplicantOverridden
		{
			get => base.CLP_IsApplicantOverridden;
			set
			{
				var oldValue = base.CLP_IsApplicantOverridden;
				base.CLP_IsApplicantOverridden = value;
				ActionWhenDataIsChangedAndIsNotCopying(oldValue, value, (old, newValue) =>
				{
					DefaultApplicantDataIfNeeded();
					ResetLPCOApplicantPKIfTypeIsOther();
				});
			}
		}

		internal ZGuid DocsAndCartageParentPK
		{
			get
			{
				var result = ZGuid.Empty;
				var declaration = (Parent as IDeclarationProvider)?.Declaration;
				if (declaration != null)
				{
					result = declaration.JE_JS.IsEmpty ? declaration.PK : declaration.JE_JS;
				}
				return result;
			}
		}

		internal IEnumerable<ZGuid> HolderOrgHeadersPks
		{
			get
			{
				if (holderOrgHeadersPks == null)
				{
					holderOrgHeadersPks = new List<ZGuid>();
					var invoiceLine = (Parent as IPGAHeader)?.Parent as JobComInvoiceLine;
					if (invoiceLine != null)
					{
						holderOrgHeadersPks.AddRange(invoiceLine.GetHolderOrgHeadersPks());
					}
					holderOrgHeadersPks.Add(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
					holderOrgHeadersPks.RemoveAll(x => !x.IsValid);
				}
				return holderOrgHeadersPks;
			}
		}
		List<ZGuid> holderOrgHeadersPks;

		public bool DoesLPCOHolderReferenceDecOrgs => CLP_HolderType != LPCOHolderPartyTypeCodes.Codes.Other;

		public bool DoesLPCOApplicantReferenceDecOrgs => CLP_ApplicantType != LPCOHolderPartyTypeCodes.Codes.Other;

		public ZString TypeDescription
		{
			get
			{
				return DocumentType?.ZZD_Description ?? CFIALPCO?.ZZD_Description ?? ZString.Empty;
			}
		}

		public ZString AgencyIDCode
		{
			get
			{
				return DocumentType?.GetAttribute(RefCusCodeListAttributeTypes.Codes.CADocumentTypePGAType) ?? (CFIALPCO != null ? (ZString)(PGACodes.Codes.CFIA) : ZString.Empty);
			}
		}

		ZZRefCusCodeListCombined CFIALPCO
		{
			get
			{
				if (cachedCFIALPCO == null)
				{
					cachedCFIALPCO = new CachedValue<ZZRefCusCodeListCombined>(() =>
					{
						if (!IsDeleted)
						{
							return RegistrationNumberHelper.LoadCFIALPCOType(Factory, CLP_Type);
						}
						return null;
					});
				}
				return cachedCFIALPCO.Value;
			}
		}
		CachedValue<ZZRefCusCodeListCombined> cachedCFIALPCO;

		public ZZRefCusCodeListCombined DocumentType
		{
			get
			{
				if (IsDeleted)
				{
					return null;
				}

				var date = ZDateTime.UtcToday.Date;
				return Factory.GetCachedValue(string.Format("CADocumentType_{0}_{1}", CLP_Type, date.ToShortDateString()), () =>
				{
					return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(Factory, CLP_Type, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, date);
				});
			}
		}

		public ZBool Is_SecondaryRefNo_ReadOnly => IsGACNonForeignExportLicense;

		public ZBool Is_LPCOIssueDate_ReadOnly => IsGACNonForeignExportLicense;

		public ZBool IsGACNonForeignExportLicense => GACPGAHeader != null && CLP_Type != ForeignExportLicenseCode;

		public const string ForeignExportLicenseCode = LPCODocumentTypeQualifier.Codes._2007;

		public const string DefaultRefNo = "XXX";

		public static readonly ImmutableList<string> AllLPCOFields = ImmutableList.Create
		(
			CusCALPCO.Schema.CLP_RN_NKAuthorizationCountry,
			CusCALPCO.Schema.CLP_CommodityTypeCode,
			CusCALPCO.Schema.CLP_HolderContactEmail,
			CusCALPCO.Schema.CLP_HolderContactName,
			CusCALPCO.Schema.CLP_HolderContactPhone,
			CusCALPCO.Schema.CLP_IsHolderOverridden,
			CusCALPCO.Schema.CLP_RN_NKIssuanceCountryCode,
			CusCALPCO.Schema.CLP_RN_NKOriginCountryCode,
			CusCALPCO.Schema.CLP_DIFRefNumberOrLocation,
			CusCALPCO.Schema.CLP_IsMixedCountryOfOrigin,
			CusCALPCO.Schema.CLP_ApplicantType,
			CusCALPCO.Schema.CLP_ApplicantName,
			CusCALPCO.Schema.CLP_OA_Applicant,
			CusCALPCO.Schema.LPCOApplicantOrgPK,
			CusCALPCO.Schema.CLP_EndDate,
			CusCALPCO.Schema.CLP_HolderType,
			CusCALPCO.Schema.CLP_HolderName,
			CusCALPCO.Schema.CLP_OA_Holder,
			CusCALPCO.Schema.LPCOHolderOrgPK,
			CusCALPCO.Schema.CLP_IssueDate,
			CusCALPCO.Schema.CLP_StartDate,
			CusCALPCO.Schema.CLP_ApplicantContactEmail,
			CusCALPCO.Schema.CLP_ApplicantContactName,
			CusCALPCO.Schema.CLP_ApplicantContactPhone,
			CusCALPCO.Schema.CLP_IsApplicantOverridden,
			CusCALPCO.Schema.CLP_AlternativeQuotaQuantity,
			CusCALPCO.Schema.CLP_RefNo,
			CusCALPCO.Schema.CLP_SecondaryRefNo,
			CusCALPCO.Schema.CLP_Type,
			CusCALPCO.Schema.CLP_AlternativeQuotaUQ,
			CusCALPCO.Schema.CLP_RN_NKSmeltAndPourCountryCode
		);

		public JobRequiredDocumentAddInfo DIFDocument
		{
			get
			{
				return Factory.GetCachedValue(string.Format("CA_DIFDocument_URN_{0}", CLP_DIFRefNumberOrLocation), () =>
				{
					if (CLP_DIFRefNumberOrLocation.IsEmpty)
					{
						return null;
					}
					else
					{
						var companyPK = Declaration.RegistryCompanyPK;
						var pgaHeader = Parent as IPGAHeader;
						var query = JobRequiredDocumentAddInfoLoadHelper.GetJobRequiredDocumentAddInfoFilterForJobDocsAndCartage(DocsAndCartageParentPK, companyPK, Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF, pgaHeader?.GovAgencyIDCode ?? ZString.Empty, CLP_DIFRefNumberOrLocation);
						return Factory.Load<JobRequiredDocumentAddInfo>(query).FirstOrDefault();
					}
				});
			}
		}

		#region Methods

		void ValidateRelatedPropertiesFromDocumentType()
		{
			Validation.ValidateCLP_AlternativeQuotaQuantity();
			Validation.ValidateCLP_RefNo();
			Validation.ValidateCLP_SecondaryRefNo();
			Validation.ValidateCLP_HolderType();
			Validation.ValidateCLP_ApplicantType();
			Validation.ValidateCLP_IssueDate();
			Validation.ValidateCLP_StartDate();
			Validation.ValidateCLP_EndDate();
			Validation.ValidateCLP_RN_NKIssuanceCountryCode();
			Validation.ValidateCLP_DIFRefNumberOrLocation();
		}

		void DefaultHolderDetailsIfNeeded()
		{
			if (CLP_IsHolderOverridden)
			{
				var holder = GetHolderParty(CLP_HolderType, () => OthLPCOHolder);
				UpdateLPCOHolderName(holder);
				DefaultHolderContactDetails(GetPGAContactDetails(holder));
			}
			else
			{
				ClearLPCOHolderName();
				ClearHolderContactDetails();
			}
		}

		void DefaultHolderContactDetails(ZString partyType)
		{
			if (CLP_IsHolderOverridden)
			{
				DefaultHolderContactDetails(GetHolderPartyContactDetails(partyType, () => OthLPCOHolder));
			}
			else
			{
				ClearHolderContactDetails();
			}
		}

		void ClearHolderContactDetails()
		{
			base.CLP_HolderContactEmail = ZString.Empty;
			base.CLP_HolderContactName = ZString.Empty;
			base.CLP_HolderContactPhone = ZString.Empty;
		}

		void DefaultHolderContactDetails(PGAContactDetails contactDetails)
		{
			var holderContactEmail = ZString.Empty;
			var holderContactPhone = ZString.Empty;
			var holderContactName = ZString.Empty;
			if (contactDetails != null)
			{
				holderContactEmail = contactDetails.EmailAddress;
				holderContactPhone = contactDetails.PhoneNumber;
				holderContactName = contactDetails.ContactName;
			}
			if (base.CLP_HolderContactEmail != holderContactEmail)
			{
				base.CLP_HolderContactEmail = holderContactEmail;
			}
			if (base.CLP_HolderContactPhone != holderContactPhone)
			{
				base.CLP_HolderContactPhone = holderContactPhone;
			}
			if (base.CLP_HolderContactName != holderContactName)
			{
				base.CLP_HolderContactName = holderContactName;
			}
		}

		void DefaultApplicantDataIfNeeded()
		{
			if (CLP_IsApplicantOverridden)
			{
				var applicant = GetHolderParty(CLP_ApplicantType, () => OthLPCOApplicant);
				UpdateLPCOApplicantName(applicant);
				DefaultApplicantContactDetails(GetPGAContactDetails(applicant));
			}
			else
			{
				ClearLPCOApplicantName();
				ClearApplicantContactDetails();
			}
		}

		void DefaultApplicantContactDetails(ZString partyType)
		{
			if (CLP_IsApplicantOverridden)
			{
				DefaultApplicantContactDetails(GetHolderPartyContactDetails(partyType, () => OthLPCOApplicant));
			}
			else
			{
				ClearApplicantContactDetails();
			}
		}

		void ClearApplicantContactDetails()
		{
			base.CLP_ApplicantContactEmail = ZString.Empty;
			base.CLP_ApplicantContactName = ZString.Empty;
			base.CLP_ApplicantContactPhone = ZString.Empty;
		}

		void DefaultApplicantContactDetails(PGAContactDetails contactDetails)
		{
			var applicantContactEmail = ZString.Empty;
			var applicantContactPhone = ZString.Empty;
			var applicantContactName = ZString.Empty;
			if (contactDetails != null)
			{
				applicantContactEmail = contactDetails.EmailAddress;
				applicantContactPhone = contactDetails.PhoneNumber;
				applicantContactName = contactDetails.ContactName;
			}

			if (base.CLP_ApplicantContactEmail != applicantContactEmail)
			{
				base.CLP_ApplicantContactEmail = applicantContactEmail;
			}

			if (base.CLP_ApplicantContactPhone != applicantContactPhone)
			{
				base.CLP_ApplicantContactPhone = applicantContactPhone;
			}

			if (base.CLP_ApplicantContactName != applicantContactName)
			{
				base.CLP_ApplicantContactName = applicantContactName;
			}
		}

		public PGAContactDetails GetHolderPartyContactDetails(ZString partyType, Func<OrgHeader> getOTHLPCOParty) => GetPGAContactDetails(GetHolderParty(partyType, getOTHLPCOParty));
		PGAContactDetails GetPGAContactDetails(OrgHeader org) => org == null ? null : new PGAContactDetails(org);

		public OrgHeader GetHolderParty(string partyType, Func<OrgHeader> getOTHLPCOParty)
		{
			var declaration = Declaration;
			if (partyType == LPCOHolderPartyTypeCodes.Codes.Broker)
			{
				return declaration?.GetOrgProxyWithCABusinessNumber();
			}
			else if (partyType == LPCOHolderPartyTypeCodes.Codes.Other)
			{
				return getOTHLPCOParty();
			}
			else
			{
				var pgaHeader = Parent as IPGAHeader;
				var invoiceLine = pgaHeader?.Parent as JobComInvoiceLine;

				return invoiceLine != null ? invoiceLine.GetLPCOHolderParty(partyType) : declaration?.GetLPCOHolderParty(partyType);
			}
		}

		void ResetLPCOHolderPKIfTypeIsOther()
		{
			if (DoesLPCOHolderReferenceDecOrgs)
			{
				CLP_OA_Holder = ZGuid.Empty;
				LPCOHolderOrgPK = ZGuid.Empty;
			}
		}

		void UpdateLPCOHolderName()
		{
			if (CLP_IsHolderOverridden)
			{
				UpdateLPCOHolderName(GetHolderParty(CLP_HolderType, () => OthLPCOHolder));
			}
			else
			{
				ClearLPCOHolderName();
			}
		}

		void ClearLPCOHolderName()
		{
			base.CLP_HolderName = ZString.Empty;
		}

		void UpdateLPCOHolderName(OrgHeader holderOrg)
		{
			var name = ZString.Empty;
			if (holderOrg != null)
			{
				var addressCompanyNameOverride = Holder?.OA_CompanyNameOverride ?? ZString.Empty;
				name = (!addressCompanyNameOverride.IsEmpty ? addressCompanyNameOverride : holderOrg.OH_FullName).Left(Schema.CLP_HolderNameMaxLength);
			}
			if (base.CLP_HolderName != name)
			{
				base.CLP_HolderName = name;
			}
		}

		void ResetLPCOApplicantPKIfTypeIsOther()
		{
			if (DoesLPCOApplicantReferenceDecOrgs)
			{
				CLP_OA_Applicant = ZGuid.Empty;
				LPCOApplicantOrgPK = ZGuid.Empty;
			}
		}

		void UpdateLPCOApplicantName()
		{
			if (CLP_IsApplicantOverridden)
			{
				UpdateLPCOApplicantName(GetHolderParty(CLP_ApplicantType, () => OthLPCOApplicant));
			}
			else
			{
				ClearLPCOApplicantName();
			}
		}

		void ClearLPCOApplicantName()
		{
			base.CLP_ApplicantName = ZString.Empty;
		}

		void UpdateLPCOApplicantName(OrgHeader applicantOrg)
		{
			var name = ZString.Empty;
			if (applicantOrg != null)
			{
				var addressCompanyNameOverride = Applicant?.OA_CompanyNameOverride ?? ZString.Empty;
				name = (!addressCompanyNameOverride.IsEmpty ? addressCompanyNameOverride : applicantOrg.OH_FullName).Left(Schema.CLP_ApplicantNameMaxLength);
			}

			if (base.CLP_ApplicantName != name)
			{
				base.CLP_ApplicantName = name;
			}
		}

		void UpdateLPCODetails(IDIFDocument dIFDocument)
		{
			var parent = Parent as ILPCODefaulter;
			if (parent != null && parent.ShouldDefaultLPCOFields)
			{
				if (CLP_Type.IsEmpty && parent.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_Type))
				{
					CLP_Type = dIFDocument.DocumentType;
				}

				if (CLP_RefNo.IsEmpty && parent.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_RefNo))
				{
					CLP_RefNo = dIFDocument.DocumentNumber;
				}

				if (!CLP_StartDate.IsValid && parent.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_StartDate))
				{
					CLP_StartDate = (ZDate)dIFDocument.EffectiveDate;
				}

				if (!CLP_IssueDate.IsValid && parent.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_IssueDate))
				{
					CLP_IssueDate = (ZDate)dIFDocument.EffectiveDate;
				}

				if (!CLP_EndDate.IsValid && parent.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_EndDate))
				{
					CLP_EndDate = (ZDate)dIFDocument.ExpiryDate;
				}

				var invoiceLine = (Parent as IPGAHeader)?.Parent as JobComInvoiceLine;
				if (invoiceLine != null && CLP_HolderType.IsEmpty && parent.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_HolderType))
				{
					CLP_HolderType = invoiceLine.GetLPCOHolderType(dIFDocument.StakeHolderOrg);
				}
			}
		}

		#endregion

		#region Validation

		protected override bool IsValidationEnabledCore(ZPropertyInfo propertyInfo)
		{
			return base.IsValidationEnabledCore(propertyInfo) && (((ICADeclarationProvider)Parent)?.IsValidationEnabled ?? ZBool.True);
		}

		#endregion

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			List<string> result = new List<string>(base.GetPropertiesToExcludeFromCloning());
			result.Add(CusCALPCOSchema.CLP_ParentID.Name);
			result.Add(CusCALPCOSchema.CLP_ParentTableCode.Name);
			return result;
		}
	}
}
