using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class LPCOViewValidation : ZValidation
	{
		public LPCOViewValidation(LPCOView parent, IPGAHeader pgaHeader) : base(parent)
		{
			this.parent = parent;
			this.pgaHeader = pgaHeader;
			this.zValidationInternals = this;
			this.parentListInternals = parent;
		}

		readonly IPGAHeader pgaHeader;
		protected IPGAProgramRequirementProvider PGAHeader => Parent.PGAHeader;

		ZBool IsFromMiscOnPGAHeader => pgaHeader != null;

		public void Add(LPCOViewValidation validation)
		{
			zValidationInternals.Add(validation);
		}
		public void Remove(LPCOViewValidation validation)
		{
			zValidationInternals.Remove(validation);
		}

		#region ValidateAll
		public override void ValidateAll()
		{
			IDisposable suspender = parentListInternals.SuspendListChanged();
			try
			{
				ValidateAllCore();
			}
			finally
			{
				suspender.Dispose();
			}
		}
		protected void ValidateAllCore()
		{
			ValidateCLP_Type();
			ValidateCLP_RefNo();
			ValidateCLP_HolderType();
			ValidateCLP_AuthorizedPartyContactName();
			ValidateCLP_AuthorizedPartyContactPhone();
			ValidateCLP_AuthorizedPartyContactEmail();
			ValidateCLP_LPCOApplicantName();
			ValidateCLP_OA_LPCOApplicant();
			ValidateCLP_LPCOHolderName();
			ValidateCLP_OA_LPCOHolder();
			ValidateCLP_SecondaryRefNo();
			ValidateCLP_IssueDate();
			ValidateCLP_RN_NKIssuanceCountryCode();
			ValidateCLP_RN_NKOriginCountryCode();
			ValidateCLP_RN_NKAuthorizationCountry();
			ValidateCLP_EndDate();
			ValidateCLP_StartDate();
			ValidateCLP_ApplicantType();
			ValidateCLP_ApplicantContactName();
			ValidateCLP_ApplicantContactPhone();
			ValidateCLP_ApplicantContactEmail();
			ValidateCLP_AlternativeQuotaQuantity();
			ValidateCLP_DIFRefNumberOrLocation();
			ValidateCLP_CommodityTypeCode();
			ValidateCLP_IsMixedCountryOfOrigin();
			ValidateCLP_AlternativeQuotaUQ();
			ValidateCLP_RN_NKSmeltAndPourCountryCode();
		}
		#endregion

		CusCALPCOValidation LPCOValidation => Parent.LPCO.Validation;

		#region ValidateCLP_Type
		public void ValidateCLP_Type()
		{
			LPCOValidation.ValidateCLP_Type();
			zValidationInternals.Validate(Parent.CLP_TypeInfo, new RunValidationInvoker(() =>
			{
				CheckCLP_Type();
				AddFromMiscMessageErrorIfRequired(Parent.CLP_TypeInfo);
			}));
		}

		protected virtual void CheckCLP_Type()
		{
			Parent.CLP_TypeInfo.AddAllNotificationsFrom(Parent.LPCO.CLP_TypeInfo);
			if (!Parent.CLP_Type.IsEmpty)
			{
				var collectionParent = Parent.LPCO?.Parent as ILPCOCollectionParent;
				var lpcoViewCollection = collectionParent?.LPCOViews;

				if (lpcoViewCollection != null)
				{
					if (lpcoViewCollection.Cast<LPCOView>().Where(x => x.CLP_Type == Parent.CLP_Type).Take(2).Count() > 1)
					{
						Parent.CLP_TypeInfo.AddMessageError(Res.GetString("68502109-8e3f-4b57-b2e7-2cbc5dd794fb", "Type: {0} is duplicated.", Parent.CLP_Type));
					}
					else if (PGAHeader != null)
					{
						CheckCLP_TypeForPGAHeader(lpcoViewCollection);
					}
				}
			}
		}

		void CheckCLP_TypeForPGAHeader(LPCOViewCollection lpcoViewCollection)
		{
			foreach (var programCode in PGAHeader.GetEnabledProgramCodes())
			{
				var programInfo = PGAHeader.GetProgramCodesList()[programCode, StringComparison.OrdinalIgnoreCase].Description;
				var defaultDocumentTypes = PGAHeader.GetDefaultDocumentTypes(programCode);
				var groupedAlternativeTypes = defaultDocumentTypes.Where(x => x.RequiredType == DocumentTypeRequieredType.Alternative).GroupBy(x => x.Category);

				if (defaultDocumentTypes.Any() && defaultDocumentTypes.All(x => x.Code != Parent.CLP_Type) && PGAHeader.NeedsDocumentTypeValidation())
				{
					Parent.CLP_TypeInfo.AddMessageError(Res.GetString("80d3c3dd-a745-426a-8fc9-4e268198bfd1", "The selected document type is invalid for the program: {0}", programInfo));
				}
				else if (groupedAlternativeTypes.Any())
				{
					foreach (var alternativeTypes in groupedAlternativeTypes)
					{
						if (alternativeTypes.Any(x => x.Code == Parent.CLP_Type) && lpcoViewCollection.Cast<LPCOView>().Any(x => x.CLP_Type != Parent.CLP_Type && alternativeTypes.Any(y => y.Code == x.CLP_Type)))
						{
							var message = Res.GetString("7440ee18-3f25-49dd-b88b-4b79ffceacce", "One of [{0}] should be added for program: {1}.", string.Join(", ", alternativeTypes.Select(x => x.Code)), programInfo);
							Parent.CLP_TypeInfo.AddMessageError(message);
						}
					}
				}
			}
		}
		#endregion

		#region ValidateCLP_RefNo
		public void ValidateCLP_RefNo()
		{
			LPCOValidation.ValidateCLP_RefNo();
			zValidationInternals.Validate(Parent.CLP_RefNoInfo, new RunValidationInvoker(() =>
			{
				CheckCLP_RefNo();
				AddFromMiscMessageErrorIfRequired(Parent.CLP_RefNoInfo);
			}));
		}

		protected virtual void CheckCLP_RefNo()
		{
			Parent.CLP_RefNoInfo.AddAllNotificationsFrom(Parent.LPCO.CLP_RefNoInfo);
			if (PGAHeader is PHACPGAHeader && !Parent.CLP_Type.IsEmpty)
			{
				var phacHeader = PGAHeader as PHACPGAHeader;
				if (phacHeader.IsPathogenToxinRequired)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.CLP_RefNoInfo);
				}
				else if (phacHeader.CA_ExceptPathogenToxin && !Parent.CLP_RefNo.IsEmpty)
				{
					Parent.CLP_RefNoInfo.AddMessageError(Res.GetString("0BF72DE6-9050-4494-9A8E-5982F5ACC4F3", "This license number is not required as it is indicated as exempt."));
				}
			}
		}
		#endregion

		#region ValidateCLP_LPCOHolderType
		public void ValidateCLP_HolderType()
		{
			LPCOValidation.ValidateCLP_HolderType();
			zValidationInternals.Validate(Parent.CLP_HolderTypeInfo, new RunValidationInvoker(() =>
			{
				CheckCLP_HolderType();
				AddFromMiscMessageErrorIfRequired(Parent.CLP_HolderTypeInfo);
			}));
		}

		protected virtual void CheckCLP_HolderType()
		{
			Parent.CLP_HolderTypeInfo.AddAllNotificationsFrom(Parent.LPCO.CLP_HolderTypeInfo);
		}
		#endregion

		#region ValidateCLP_AuthorizedPartyContactName
		public void ValidateCLP_AuthorizedPartyContactName()
		{
			LPCOValidation.ValidateCLP_HolderContactName();
			zValidationInternals.Validate(Parent.CLP_HolderContactNameInfo, new RunValidationInvoker(() =>
			{
				CheckCLP_AuthorizedPartyContactName();
				AddFromMiscMessageErrorIfRequired(Parent.CLP_HolderContactNameInfo);
			}));
		}

		protected virtual void CheckCLP_AuthorizedPartyContactName()
		{
			Parent.CLP_HolderContactNameInfo.AddAllNotificationsFrom(Parent.LPCO.CLP_HolderContactNameInfo);
			CheckAuthorizedPartyContactDetail(Parent.CLP_HolderContactNameInfo, OrganisationValidation.AuthorizedPartyContactNameIsRequired);
		}
		#endregion

		#region ValidateCLP_AuthorizedPartyContactPhone
		public void ValidateCLP_AuthorizedPartyContactPhone()
		{
			LPCOValidation.ValidateCLP_HolderContactPhone();
			zValidationInternals.Validate(Parent.CLP_HolderContactPhoneInfo, new RunValidationInvoker(() =>
			{
				CheckCLP_AuthorizedPartyContactPhone();
				AddFromMiscMessageErrorIfRequired(Parent.CLP_HolderContactPhoneInfo);
			}));
		}

		protected virtual void CheckCLP_AuthorizedPartyContactPhone()
		{
			Parent.CLP_HolderContactPhoneInfo.AddAllNotificationsFrom(Parent.LPCO.CLP_HolderContactPhoneInfo);
			CheckAuthorizedPartyContactDetail(Parent.CLP_HolderContactPhoneInfo, OrganisationValidation.AuthorizedPartyContactPhoneNumberIsRequired);
		}
		#endregion

		#region ValidateCLP_AuthorizedPartyContactEmail
		public void ValidateCLP_AuthorizedPartyContactEmail()
		{
			LPCOValidation.ValidateCLP_HolderContactEmail();
			zValidationInternals.Validate(Parent.CLP_HolderContactEmailInfo, new RunValidationInvoker(() =>
			{
				CheckCLP_AuthorizedPartyContactEmail();
				AddFromMiscMessageErrorIfRequired(Parent.CLP_HolderContactEmailInfo);
			}));
		}

		protected virtual void CheckCLP_AuthorizedPartyContactEmail()
		{
			Parent.CLP_HolderContactEmailInfo.AddAllNotificationsFrom(Parent.LPCO.CLP_HolderContactEmailInfo);
			CheckAuthorizedPartyContactDetail(Parent.CLP_HolderContactEmailInfo, OrganisationValidation.AuthorizedPartyContactEmailIsRequired);
		}
		#endregion

		#region ValidateCLP_LPCOApplicantName
		public void ValidateCLP_LPCOApplicantName()
		{
			LPCOValidation.ValidateCLP_ApplicantName();
			zValidationInternals.Validate(Parent.CLP_ApplicantNameInfo, new RunValidationInvoker(() =>
			{
				CheckCLP_LPCOApplicantName();
				AddFromMiscMessageErrorIfRequired(Parent.CLP_ApplicantNameInfo);
			}));
		}

		protected virtual void CheckCLP_LPCOApplicantName()
		{
			Parent.CLP_ApplicantNameInfo.AddAllNotificationsFrom(Parent.LPCO.CLP_ApplicantNameInfo);
		}
		#endregion

		#region ValidateCLP_OA_LPCOApplicant
		public void ValidateCLP_OA_LPCOApplicant()
		{
			LPCOValidation.ValidateCLP_OA_Applicant();
			zValidationInternals.Validate(Parent.CLP_OA_ApplicantInfo, new RunValidationInvoker(() =>
			{
				CheckCLP_OA_LPCOApplicant();
				AddFromMiscMessageErrorIfRequired(Parent.CLP_OA_ApplicantInfo);
			}));
		}

		protected virtual void CheckCLP_OA_LPCOApplicant()
		{
			Parent.CLP_OA_ApplicantInfo.AddAllNotificationsFrom(Parent.LPCO.CLP_OA_ApplicantInfo);
		}
		#endregion

		#region ValidateCLP_OA_LPCOHolder
		public void ValidateCLP_OA_LPCOHolder()
		{
			LPCOValidation.ValidateCLP_OA_Holder();
			zValidationInternals.Validate(Parent.CLP_OA_HolderInfo, new RunValidationInvoker(() =>
			{
				CheckCLP_OA_LPCOHolder();
				AddFromMiscMessageErrorIfRequired(Parent.CLP_OA_HolderInfo);
			}));
		}

		protected virtual void CheckCLP_OA_LPCOHolder()
		{
			Parent.CLP_OA_HolderInfo.AddAllNotificationsFrom(Parent.LPCO.CLP_OA_HolderInfo);
		}
		#endregion

		#region ValidateCLP_SecondaryRefNo
		public void ValidateCLP_SecondaryRefNo()
		{
			LPCOValidation.ValidateCLP_SecondaryRefNo();
			zValidationInternals.Validate(Parent.CLP_SecondaryRefNoInfo, new RunValidationInvoker(() =>
			{
				CheckCLP_SecondaryRefNo();
				AddFromMiscMessageErrorIfRequired(Parent.CLP_SecondaryRefNoInfo);
			}));
		}

		protected virtual void CheckCLP_SecondaryRefNo()
		{
			Parent.CLP_SecondaryRefNoInfo.AddAllNotificationsFrom(Parent.LPCO.CLP_SecondaryRefNoInfo);
		}
		#endregion

		#region ValidateCLP_LPCOIssueDate
		public void ValidateCLP_IssueDate()
		{
			LPCOValidation.ValidateCLP_IssueDate();
			zValidationInternals.Validate(Parent.CLP_IssueDateInfo, new RunValidationInvoker(() =>
			{
				CheckCLP_IssueDate();
				AddFromMiscMessageErrorIfRequired(Parent.CLP_IssueDateInfo);
			}));
		}

		protected virtual void CheckCLP_IssueDate()
		{
			Parent.CLP_IssueDateInfo.AddAllNotificationsFrom(Parent.LPCO.CLP_IssueDateInfo);
		}
		#endregion

		#region ValidateCLP_CountryOfIssuance
		public void ValidateCLP_RN_NKIssuanceCountryCode()
		{
			LPCOValidation.ValidateCLP_RN_NKIssuanceCountryCode();
			zValidationInternals.Validate(Parent.CLP_RN_NKIssuanceCountryCodeInfo, new RunValidationInvoker(() =>
			{
				CheckCLP_RN_NKIssuanceCountryCode();
				AddFromMiscMessageErrorIfRequired(Parent.CLP_RN_NKIssuanceCountryCodeInfo);
			}));
		}

		protected virtual void CheckCLP_RN_NKIssuanceCountryCode()
		{
			Parent.CLP_RN_NKIssuanceCountryCodeInfo.AddAllNotificationsFrom(Parent.LPCO.CLP_RN_NKIssuanceCountryCodeInfo);
		}
		#endregion

		#region ValidateCLP_CountryOfOrigin
		public void ValidateCLP_RN_NKOriginCountryCode()
		{
			LPCOValidation.ValidateCLP_RN_NKOriginCountryCode();
			zValidationInternals.Validate(Parent.CLP_RN_NKOriginCountryCodeInfo, new RunValidationInvoker(() =>
			{
				CheckCLP_RN_NKOriginCountryCode();
				AddFromMiscMessageErrorIfRequired(Parent.CLP_RN_NKOriginCountryCodeInfo);
			}));
		}

		protected virtual void CheckCLP_RN_NKOriginCountryCode()
		{
			Parent.CLP_RN_NKOriginCountryCodeInfo.AddAllNotificationsFrom(Parent.LPCO.CLP_RN_NKOriginCountryCodeInfo);
		}
		#endregion

		#region ValidateCLP_AuthorizationCountry
		public void ValidateCLP_RN_NKAuthorizationCountry()
		{
			LPCOValidation.ValidateCLP_RN_NKAuthorizationCountry();
			zValidationInternals.Validate(Parent.CLP_RN_NKAuthorizationCountryInfo, new RunValidationInvoker(() =>
			{
				CheckCLP_RN_NKAuthorizationCountry();
				AddFromMiscMessageErrorIfRequired(Parent.CLP_RN_NKAuthorizationCountryInfo);
			}));
		}

		protected virtual void CheckCLP_RN_NKAuthorizationCountry()
		{
			Parent.CLP_RN_NKAuthorizationCountryInfo.AddAllNotificationsFrom(Parent.LPCO.CLP_RN_NKAuthorizationCountryInfo);
		}
		#endregion

		#region ValidateCLP_RN_NKSmeltAndPourCountryCode

		public void ValidateCLP_RN_NKSmeltAndPourCountryCode()
		{
			LPCOValidation.ValidateCLP_RN_NKSmeltAndPourCountryCode();
			zValidationInternals.Validate(Parent.CLP_RN_NKSmeltAndPourCountryCodeInfo, new RunValidationInvoker(() =>
			{
				CheckCLP_RN_NKSmeltAndPourCountryCode();
				AddFromMiscMessageErrorIfRequired(Parent.CLP_RN_NKSmeltAndPourCountryCodeInfo);
			}));
		}

		protected virtual void CheckCLP_RN_NKSmeltAndPourCountryCode()
		{
			Parent.CLP_RN_NKSmeltAndPourCountryCodeInfo.AddAllNotificationsFrom(Parent.LPCO.CLP_RN_NKSmeltAndPourCountryCodeInfo);
		}

		#endregion

		#region ValidateCLP_EndDate
		public void ValidateCLP_EndDate()
		{
			LPCOValidation.ValidateCLP_EndDate();
			zValidationInternals.Validate(Parent.CLP_EndDateInfo, new RunValidationInvoker(() =>
			{
				CheckCLP_EndDate();
				AddFromMiscMessageErrorIfRequired(Parent.CLP_EndDateInfo);
			}));
		}

		protected virtual void CheckCLP_EndDate()
		{
			Parent.CLP_EndDateInfo.AddAllNotificationsFrom(Parent.LPCO.CLP_EndDateInfo);
			var phacHeader = PGAHeader as PHACPGAHeader;
			if (phacHeader != null && !Parent.CLP_Type.IsEmpty)
			{
				if (phacHeader.IsPathogenToxinRequired || !Parent.CLP_RefNo.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.CLP_EndDateInfo);
				}
			}
		}
		#endregion

		#region ValidateCLP_StartDate
		public void ValidateCLP_StartDate()
		{
			LPCOValidation.ValidateCLP_StartDate();
			zValidationInternals.Validate(Parent.CLP_StartDateInfo, new RunValidationInvoker(() =>
			{
				CheckCLP_StartDateInfo();
				AddFromMiscMessageErrorIfRequired(Parent.CLP_StartDateInfo);
			}));
		}

		protected virtual void CheckCLP_StartDateInfo()
		{
			Parent.CLP_StartDateInfo.AddAllNotificationsFrom(Parent.LPCO.CLP_StartDateInfo);
			var phacHeader = PGAHeader as PHACPGAHeader;
			if (phacHeader != null && !Parent.CLP_Type.IsEmpty)
			{
				if (phacHeader.IsPathogenToxinRequired || !Parent.CLP_RefNo.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.CLP_StartDateInfo);
				}
			}
		}
		#endregion

		#region ValidateCLP_LPCOApplicant
		public void ValidateCLP_ApplicantType()
		{
			LPCOValidation.ValidateCLP_ApplicantType();
			zValidationInternals.Validate(Parent.CLP_ApplicantTypeInfo, new RunValidationInvoker(() =>
			{
				CheckCLP_LPCOApplicant();
				AddFromMiscMessageErrorIfRequired(Parent.CLP_ApplicantTypeInfo);
			}));
		}

		protected virtual void CheckCLP_LPCOApplicant()
		{
			Parent.CLP_ApplicantTypeInfo.AddAllNotificationsFrom(Parent.LPCO.CLP_ApplicantTypeInfo);
		}
		#endregion

		#region ValidateCLP_AlternativeQuotaQuantity
		public void ValidateCLP_AlternativeQuotaQuantity()
		{
			LPCOValidation.ValidateCLP_AlternativeQuotaQuantity();
			zValidationInternals.Validate(Parent.CLP_AlternativeQuotaQuantityInfo, new RunValidationInvoker(() =>
			{
				CheckCLP_AlternativeQuotaQuantity();
				AddFromMiscMessageErrorIfRequired(Parent.CLP_AlternativeQuotaQuantityInfo);
			}));
		}

		protected virtual void CheckCLP_AlternativeQuotaQuantity()
		{
			Parent.CLP_AlternativeQuotaQuantityInfo.AddAllNotificationsFrom(Parent.LPCO.CLP_AlternativeQuotaQuantityInfo);
		}
		#endregion

		#region ValidateCLP_DIFRefNumberOrLocation
		public void ValidateCLP_DIFRefNumberOrLocation()
		{
			LPCOValidation.ValidateCLP_DIFRefNumberOrLocation();
			zValidationInternals.Validate(Parent.CLP_DIFRefNumberOrLocationInfo, new RunValidationInvoker(() =>
			{
				CheckCLP_DIFRefNumberOrLocation();
				AddFromMiscMessageErrorIfRequired(Parent.CLP_DIFRefNumberOrLocationInfo);
			}));
		}

		protected virtual void CheckCLP_DIFRefNumberOrLocation()
		{
			Parent.CLP_DIFRefNumberOrLocationInfo.AddAllNotificationsFrom(Parent.LPCO.CLP_DIFRefNumberOrLocationInfo);
			var phacHeader = PGAHeader as PHACPGAHeader;
			if (phacHeader != null && !Parent.CLP_Type.IsEmpty)
			{
				if (phacHeader.IsPathogenToxinRequired && !Parent.CLP_RefNo.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.CLP_DIFRefNumberOrLocationInfo);
				}
			}
		}
		#endregion

		#region ValidateCLP_CommodityTypeCode
		public void ValidateCLP_CommodityTypeCode()
		{
			LPCOValidation.ValidateCLP_CommodityTypeCode();
			zValidationInternals.Validate(Parent.CLP_CommodityTypeCodeInfo, new RunValidationInvoker(() =>
			{
				CheckCLP_CommodityTypeCode();
				AddFromMiscMessageErrorIfRequired(Parent.CLP_CommodityTypeCodeInfo);
			}));
		}

		protected virtual void CheckCLP_CommodityTypeCode()
		{
			Parent.CLP_CommodityTypeCodeInfo.AddAllNotificationsFrom(Parent.LPCO.CLP_CommodityTypeCodeInfo);
		}
		#endregion

		#region ValidateCLP_IsMixedCountryOfOrigin
		public void ValidateCLP_IsMixedCountryOfOrigin()
		{
			LPCOValidation.ValidateCLP_IsMixedCountryOfOrigin();
			zValidationInternals.Validate(Parent.CLP_IsMixedCountryOfOriginInfo, new RunValidationInvoker(() =>
			{
				CheckCLP_IsMixedCountryOfOrigin();
				AddFromMiscMessageErrorIfRequired(Parent.CLP_IsMixedCountryOfOriginInfo);
			}));
		}

		protected virtual void CheckCLP_IsMixedCountryOfOrigin()
		{
			Parent.CLP_IsMixedCountryOfOriginInfo.AddAllNotificationsFrom(Parent.LPCO.CLP_IsMixedCountryOfOriginInfo);
		}
		#endregion

		#region ValidateCLP_AlternativeQuotaUQ
		public void ValidateCLP_AlternativeQuotaUQ()
		{
			LPCOValidation.ValidateCLP_AlternativeQuotaUQ();
			zValidationInternals.Validate(Parent.CLP_AlternativeQuotaUQInfo, new RunValidationInvoker(() =>
			{
				CheckCLP_AlternativeQuotaUQ();
				AddFromMiscMessageErrorIfRequired(Parent.CLP_AlternativeQuotaUQInfo);
			}));
		}

		protected virtual void CheckCLP_AlternativeQuotaUQ()
		{
			Parent.CLP_AlternativeQuotaUQInfo.AddAllNotificationsFrom(Parent.LPCO.CLP_AlternativeQuotaUQInfo);
		}
		#endregion

		#region ValidateCLP_LPCOHolderName
		public void ValidateCLP_LPCOHolderName()
		{
			LPCOValidation.ValidateCLP_HolderName();
			zValidationInternals.Validate(Parent.CLP_HolderNameInfo, new RunValidationInvoker(() =>
			{
				CheckCLP_LPCOHolderName();
				AddFromMiscMessageErrorIfRequired(Parent.CLP_HolderNameInfo);
			}));
		}

		protected virtual void CheckCLP_LPCOHolderName()
		{
			Parent.CLP_HolderNameInfo.AddAllNotificationsFrom(Parent.LPCO.CLP_HolderNameInfo);
		}
		#endregion

		#region ValidateCLP_ApplicantContactName
		public void ValidateCLP_ApplicantContactName()
		{
			LPCOValidation.ValidateCLP_ApplicantContactName();
			zValidationInternals.Validate(Parent.CLP_ApplicantContactNameInfo, new RunValidationInvoker(() =>
			{
				CheckCLP_ApplicantContactName();
				AddFromMiscMessageErrorIfRequired(Parent.CLP_ApplicantContactNameInfo);
			}));
		}

		protected virtual void CheckCLP_ApplicantContactName()
		{
			Parent.CLP_ApplicantContactNameInfo.AddAllNotificationsFrom(Parent.LPCO.CLP_ApplicantContactNameInfo);
			CheckApplicantContactDetail(Parent.CLP_ApplicantContactNameInfo, OrganisationValidation.ApplicantContactNameIsRequired);
		}
		#endregion

		#region ValidateCLP_ApplicantContactPhone
		public void ValidateCLP_ApplicantContactPhone()
		{
			LPCOValidation.ValidateCLP_ApplicantContactPhone();
			zValidationInternals.Validate(Parent.CLP_ApplicantContactPhoneInfo, new RunValidationInvoker(() =>
			{
				CheckCLP_ApplicantContactPhone();
				AddFromMiscMessageErrorIfRequired(Parent.CLP_ApplicantContactPhoneInfo);
			}));
		}

		protected virtual void CheckCLP_ApplicantContactPhone()
		{
			Parent.CLP_ApplicantContactPhoneInfo.AddAllNotificationsFrom(Parent.LPCO.CLP_ApplicantContactPhoneInfo);
			CheckApplicantContactDetail(Parent.CLP_ApplicantContactPhoneInfo, OrganisationValidation.ApplicantContactPhoneNumberIsRequired);
		}
		#endregion

		#region ValidateCLP_ApplicantContactEmail
		public void ValidateCLP_ApplicantContactEmail()
		{
			LPCOValidation.ValidateCLP_ApplicantContactEmail();
			zValidationInternals.Validate(Parent.CLP_ApplicantContactEmailInfo, new RunValidationInvoker(() =>
			{
				CheckCLP_ApplicantContactEmail();
				AddFromMiscMessageErrorIfRequired(Parent.CLP_ApplicantContactEmailInfo);
			}));
		}

		protected virtual void CheckCLP_ApplicantContactEmail()
		{
			Parent.CLP_ApplicantContactEmailInfo.AddAllNotificationsFrom(Parent.LPCO.CLP_ApplicantContactEmailInfo);
			CheckApplicantContactDetail(Parent.CLP_ApplicantContactEmailInfo, OrganisationValidation.ApplicantContactEmailIsRequired);
		}
		#endregion

		void CheckAuthorizedPartyContactDetail(ZPropertyInfo contactInfo, string message)
		{
			if (!Parent.CLP_HolderType.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(contactInfo, message);
			}
		}

		void CheckApplicantContactDetail(ZPropertyInfo contactInfo, string message)
		{
			if (!Parent.CLP_ApplicantType.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(contactInfo, message);
			}
		}

		void AddFromMiscMessageErrorIfRequired(ZPropertyInfo info)
		{
			if (IsFromMiscOnPGAHeader && info.HasNotifications())
			{
				info.AddMessageError(MessageSuffix);
			}
		}

		ZString MessageSuffix => Res.GetString("4E4337F8-8370-482E-9CE4-72DCB657D770", "Please fix the errors on this field in the LPCOs grid on Misc. tab, Document Type: {0}", Parent.CLP_Type);

		#region Implementation
		public override Type AutoValidationType
		{
			get
			{
				return typeof(LPCOViewValidation);
			}
		}

		public LPCOView Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return parent;
			}
		}
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly LPCOView parent;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly IValidationInternals zValidationInternals;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly ISingleElementListInternal parentListInternals;
		#endregion
	}
}
