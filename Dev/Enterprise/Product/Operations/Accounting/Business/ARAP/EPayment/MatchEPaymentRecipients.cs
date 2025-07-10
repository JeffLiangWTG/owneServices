using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.Accounting.Business
{
	public class MatchEPaymentRecipients : NonPersistentBusinessObject, IObsoleteValidation
	{
		public MatchEPaymentRecipients(BusinessObjectFactory factory)
			: base(factory)
		{
			RefreshRequests();
		}

		public MatchEPaymentRecipients(BusinessObjectFactory factory, AccAPAccountDetails accAccountDetails)
			: base(factory)
		{
			accountDetails = accAccountDetails;
			RefreshRequests();
		}

		public AccAPAccountDetails AccountDetails => accountDetails ?? (accountDetails = Factory.New<AccAPAccountDetails>());
		AccAPAccountDetails accountDetails;

		public EPaymentBeneficiaryRequest CurrentRequest;
		public EPaymentBeneficiaryRequest LastReceivedRequest;

		public void RefreshRequests()
		{
			CurrentRequest = RetrieveLatestCreatedBeneficiaryRequest();
			LastReceivedRequest = RetrieveLatestReceivedBeneficiaryRequest();
		}

		public EPaymentBeneficiaryRequest RetrieveLatestCreatedBeneficiaryRequest()
		{
			var query = new ZQuery(AccEPaymentBeneficiaryRequestSchema.ABR_GC_Company, GlbCompany.CurrentCompany.PK);
			query.OrderBy = AccEPaymentBeneficiaryRequestSchema.ABR_SystemCreateTimeUtc.Name + OrderByClause.Descending;
			var request = new BusinessObjectFactory().LoadTop1<EPaymentBeneficiaryRequest>(query);
			return request;
		}

		public EPaymentBeneficiaryRequest RetrieveLatestReceivedBeneficiaryRequest()
		{
			var query = new ZQuery(AccEPaymentBeneficiaryRequestSchema.ABR_GC_Company, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(AccEPaymentBeneficiaryRequestSchema.ABR_Status, EPaymentStatusCodes.BeneficiaryRequest.Received);
			query.OrderBy = AccEPaymentBeneficiaryRequestSchema.ABR_LastResponseReceivedUtc.Name + OrderByClause.Descending;
			var request = new BusinessObjectFactory().LoadTop1<EPaymentBeneficiaryRequest>(query);
			return request;
		}

		public void SetAccountDetailsValuesFromBeneficiary(AccEPaymentBeneficiary beneficiary) => SetAccountDetailsValuesFromBeneficiary(AccountDetails, beneficiary);

		public static void SetAccountDetailsValuesFromBeneficiary(AccAPAccountDetails accountDetails, AccEPaymentBeneficiary beneficiary)
		{
			if (beneficiary != null)
			{
				accountDetails.SetAccountDetailsValuesFromBeneficiary(beneficiary);
			}
		}

		public EPaymentBeneficiaryRequest CreateNewBeneficiaryRequestInNewFactory()
		{
			var newFactory = new BusinessObjectFactory();
			var request = newFactory.New<EPaymentBeneficiaryRequest>();
			request.ABR_ProviderCode = EPaymentProviderCodes.Codes.OFX;
			request.ABR_GC_Company = GlbCompany.CurrentCompany.PK;
			request.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Queued;
			request.ABR_SystemCreateTimeUtc = ZDateTime.UtcNow;
			newFactory.Save();
			return request;
		}

		public (bool result, AccEPaymentStaffToken token) CheckUserHasEPaymentAccountAndToken()
		{
			var result = false;
			AccEPaymentStaffToken staffToken = null;

			var query = new ZQuery(AccBankAccountSchema.AB_GC, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(AccBankAccountSchema.AB_AccountType, AccountTypeCodeDescriptionPairList.Codes.EPA);
			if (Factory.Exists(typeof(AccBankAccount), query))
			{
				query = new ZQuery(AccEPaymentStaffTokenSchema.TK_GC, GlbCompany.CurrentCompany.PK);
				query.AddToFilter(AccEPaymentStaffTokenSchema.TK_GS_NKStaffCode, GlbStaff.CurrentUser.GS_Code);
				staffToken = Factory.LoadTop1<AccEPaymentStaffToken>(query);
				if (staffToken != null)
				{
					result = true;
				}
			}
			return (result, staffToken);
		}

		public event EventHandler OnChangeFilteredRecipients;

		void RaiseOnChangeFilteredRecipients()
		{
			if (OnChangeFilteredRecipients != null)
			{
				OnChangeFilteredRecipients(this, EventArgs.Empty);
			}
		}

		#region Overrides

		#endregion

		#region Filter Business Object

		public MatchEPaymentRecipientsFilterBusinessObject RecipientsFilter
		{
			get { return FilterRecipients_internal ?? (FilterRecipients_internal = GetRecipientFilterBusinessObject()); }
		}
		MatchEPaymentRecipientsFilterBusinessObject FilterRecipients_internal;

		MatchEPaymentRecipientsFilterBusinessObject GetRecipientFilterBusinessObject()
		{
			return new MatchEPaymentRecipientsFilterBusinessObject(this);
		}

		#endregion

		#region Properties

		public ZString RecipientListLastUpdatedDateForDisplay => LastReceivedRequest?.ABR_LastResponseReceivedUtc.ToLocalBranchTime().ToBestReadableDateTimeString() ?? ZString.Empty;
		public ZPropertyInfo RecipientListLastUpdatedDateForDisplayInfo => GetZPropertyInfo(nameof(RecipientListLastUpdatedDateForDisplay));

		public ZString ProviderCodeForDisplay => CurrentRequest?.ABR_ProviderCode ?? ZString.Empty;
		public ZPropertyInfo ProviderCodeForDisplayInfo => GetZPropertyInfo(nameof(ProviderCodeForDisplay));

		public ZString LastRequestedDateForDisplay => CurrentRequest?.ABR_SystemCreateTimeUtc.ToLocalBranchTime().ToBestReadableDateTimeString() ?? ZString.Empty;
		public ZPropertyInfo LastRequestedDateForDisplayInfo => GetZPropertyInfo(nameof(LastRequestedDateForDisplay));

		public ZString LastResponseDateForDisplay => CurrentRequest?.ABR_LastResponseReceivedUtc.ToLocalBranchTime().ToBestReadableDateTimeString() ?? ZString.Empty;
		public ZPropertyInfo LastResponseDateForDisplayInfo => GetZPropertyInfo(nameof(LastResponseDateForDisplay));

		public ZString RequestedByForDisplay => CurrentRequest?.CreatingUser?.GS_FullName ?? ZString.Empty;
		public ZPropertyInfo RequestedByForDisplayInfo => GetZPropertyInfo(nameof(RequestedByForDisplay));

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1121:DoNotIncludeColumnValuesOrNamesInErrorReporterKey", Justification = "Baseline")]
		public ZString StatusForDisplay
		{
			get
			{
				var result = ZString.Empty;
				if (CurrentRequest != null)
				{
					switch (CurrentRequest.ABR_Status)
					{
						case EPaymentStatusCodes.BeneficiaryRequest.Queued:
							result = Res.GetString("6B5CE268-FFC4-436A-823E-8F1898A8CEBD", "Queued");
							break;
						case EPaymentStatusCodes.BeneficiaryRequest.Requested:
							result = Res.GetString("681BCDEC-17CD-4DAC-A4A0-3027CB0A53B9", "Requested");
							break;
						case EPaymentStatusCodes.BeneficiaryRequest.Received:
							result = Res.GetString("AAE0559F-7B21-4E9A-A5EF-C85814BC76A7", "Received");
							break;
						case EPaymentStatusCodes.BeneficiaryRequest.Partial:
							result = Res.GetString("0749F8C0-4539-4F7E-B05B-5B12A0876433", "Partial");
							break;
						case EPaymentStatusCodes.BeneficiaryRequest.Error:
							result = Res.GetString("0D551B05-66F8-47BD-A25E-CEC5263D2459", "Error");
							break;
						default:
							result = CurrentRequest.ABR_Status;
							ErrorReporter.ReportOnce(Invariant($"MatchEPaymentRecipients.StatusForDisplay is called for invalid BeneficiaryRequest Status {CurrentRequest.ABR_Status}."));
							break;
					}
				}
				return result;
			}
		}
		public ZPropertyInfo StatusForDisplayInfo => GetZPropertyInfo(nameof(StatusForDisplay));

		public ZString ErrorDescriptionForDisplay => CurrentRequest?.ABR_ErrorDescription ?? ZString.Empty;
		public ZPropertyInfo ErrorDescriptionForDisplayInfo => GetZPropertyInfo(nameof(ErrorDescriptionForDisplay));

		#endregion

		#region Lines

		public AccEPaymentBeneficiaryCollection FilteredRecipients
		{
			get
			{
				if (FilteredRecipients_internal == null)
				{
					FilteredRecipients_internal = new AccEPaymentBeneficiaryCollection(Factory);
					RegisterEditableChildObject(FilteredRecipients_internal);
				}

				return FilteredRecipients_internal;
			}
		}
		AccEPaymentBeneficiaryCollection FilteredRecipients_internal;

		#endregion

		#region Lines Methods

		public void LoadFilteredRecipients()
		{
			ClearFilteredRecipients();
			LoadRecipientsCore();
			RaiseOnChangeFilteredRecipients();
		}

		void LoadRecipientsCore()
		{
			FilteredRecipients.Load(GetFilteredEPaymentBeneficiaries);
		}

		public ZQuery GetFilteredEPaymentBeneficiaries
		{
			get
			{
				ZQuery filter = RecipientsFilter.Filter;
				filter.AddToFilter(AccEPaymentBeneficiarySchema.ABF_GC_Company, GlbCompany.CurrentCompany.PK);
				return filter;
			}
		}

		#endregion

		#region Validation

		public void ValidateBeforeFindingFilteredRecipients()
		{
			ClearAllNotificationsIncludingChildren();
		}

		protected void ClearAllNotificationsIncludingChildren()
		{
			ClearAllNotifications();
			ClearFilteredRecipients();
		}

		public void ClearFilteredRecipients()
		{
			FilteredRecipients.RemoveAll();
			RaiseOnChangeFilteredRecipients();
		}

		#endregion
	}
}
