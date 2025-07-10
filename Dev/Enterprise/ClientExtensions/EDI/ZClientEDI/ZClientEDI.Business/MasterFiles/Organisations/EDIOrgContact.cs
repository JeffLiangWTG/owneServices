using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Certification.Business;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Organisations.EDIAccountVerificationStatus;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIOrgContact : OrgContact
	{
		string NotSetDisplayString => Res.GetString("7d366848-6055-43f5-ba85-58d68e0675b6", "Not Set");
		string SetDisplayString => Res.GetString("a9a0e480-d936-45cd-b398-b98b2881db87", "Set");

		public EDIOrgContact(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		ZBool hasISTChange = ZBool.False;

		ZBool hasISTPreviousValue = ZBool.False;

		ZBool hasISTBeInitialized = ZBool.False;

		EdiAccountVerificationStatusCollection accountVerificationStatusCollection;
		public EdiAccountVerificationStatusCollection AccountVerificationStatusCollection => accountVerificationStatusCollection ?? (accountVerificationStatusCollection = new EdiAccountVerificationStatusCollection(Factory, this));

		public ZBool RelationshipPromptRequired => AccountVerificationStatusCollection.RelationshipPromptRequired;

		public bool CanSendResetPasswordEmail
		{
			get
			{
				if (Person.ContactCollection.Count == 1)
				{
					return true;
				}

				var query = new ZQuery(EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, PK);

				var userAccounts = Factory.Load<EdiCustomerUserAccount>(query);

				return userAccounts.Any(x => x.EUA_IsActive && x.EUA_IsContactRelationshipActive) || userAccounts.All(x => x.EUA_ContactRelationshipStatus != ContactRelationshipStatusList.Codes.SelfDeactivation);
			}
		}

		public ZString CompanyName => BranchAddress?.CompanyName ?? Header.OH_FullName;

		public override ZString FromDisplayName => IncidentConstants.SupportDisplayName;

		public override ZString FromAddress => SupportIncidentLookups.SupportEmailAddress;

		public override ZString GeneratePasswordInstructionUrl(string token, PasswordInstructionType instructionType)
		{
			var myAccountUrl = WebDataRegistry.Instance.CargoWiseUserPortalUrl.Value;
			if (string.IsNullOrEmpty(myAccountUrl))
			{
				throw new WebSiteUrlNotSetException(Res.GetString("f1b94beb-4513-438c-8d6f-f1ea90b38e27", "You need to provide a My Account URL in the System Registry under path {0}", "Web > Web Component URLs > CargoWise User Portal URL"));
			}

			var shouldSendToMasterPasswordPage = ((IPasswordInstructionEmailSource)this).ShouldSendMasterPassword;
			string setResetPagePath;

			if (instructionType == PasswordInstructionType.Reset)
			{
				setResetPagePath = shouldSendToMasterPasswordPage ? "/Admin/ResetMasterPassword.aspx?ResetKey=" : "/Admin/ResetPassword.aspx?ResetKey=";
			}
			else
			{
				setResetPagePath = shouldSendToMasterPasswordPage ? "/Admin/SetMasterPassword.aspx?SetKey=" : "/Admin/SetPassword.aspx?SetKey=";
			}

			return FormattableString.Invariant($"{myAccountUrl.TrimEnd('/')}{setResetPagePath}{token}");
		}

		public ZString PersonalRecoveryEmail => string.IsNullOrEmpty(Person?.PER_EmailAddress) ? NotSetDisplayString : SetDisplayString;

		public bool PersonPasswordHashIsSet => Person?.PER_PasswordHash != null && !Person.PER_PasswordHash.IsEmpty;

		public ZString PersonPassword => PersonPasswordHashIsSet ? SetDisplayString : NotSetDisplayString;

		public bool PasswordHashIsSet => OC_PasswordHash != null && !OC_PasswordHash.IsEmpty;

		public ZString ContactPassword => PasswordHashIsSet ? SetDisplayString : NotSetDisplayString;

		public ZString PasswordIsSet => PersonPasswordHashIsSet || PasswordHashIsSet ? SetDisplayString : NotSetDisplayString;

		public new class Schema : OrgContact.Schema
		{
			public const string CompanyNameForBindingOnly = "CompanyNameForBindingOnly";
			public const string BranchForBindingOnly = "BranchForBindingOnly";
			public const string IsCustomerServiceContact = "IsCustomerServiceContact";
			public const string IsAccountsReceivableContact = "IsAccountsReceivableContact";
			public const string IsBorderWiseAdministrator = "IsBorderWiseAdministrator";
			public const string IsERequestApprover = "IsERequestApprover";
			public const string IsInformationServicesTechnicalAdministrator = "IsInformationServicesTechnicalAdministrator";
			public const string IsCertificationProgramContact = "IsCertificationProgramContact";
		}

		public CertificateApplicant RelatedCertificateApplicant
		{
			get
			{
				ZQuery query = new ZQuery(HRJobApplicantSchema.HA_EmailAddress, OC_Email);
				return Factory.LoadTop1<CertificateApplicant>(query);
			}
		}

		public override OrgDocumentCollection Documents
		{
			get
			{
				var documents = base.Documents;
				if (!hasISTBeInitialized)
				{
					var result = documents.ContainsDocGroup(EDIOrgDocumentGroupTypes.Codes.InformationServicesTechnicalAdministrator);
					hasISTPreviousValue = result;
					hasISTBeInitialized = ZBool.True;
				}
				return documents;
			}
		}

		#region My Account

		public void LogMyAccountDisclaimerAcknowledgementIfRequired(LicenceDatabase database)
		{
			if (database != null)
			{
				var hasAcknowledgement = HasLoggedMyAccountDisclaimerAcknowledgement();
				if (!hasAcknowledgement)
				{
					LogMyAccountDisclaimerAcknowledgement(database);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "This is meant to specify an exact product name.")]
		bool HasLoggedMyAccountDisclaimerAcknowledgement()
		{
			var query = new ZQuery(StmALogSchema.SL_Parent, this.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ClickThroughAgreementExecuted.Code);
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, WebContractLogReference);
			return Factory.ExistsInDatabase(StmALogSchema.Constants.TableName, query);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "This is meant to specify an exact product name.")]
		internal StmALog LogMyAccountDisclaimerAcknowledgement(LicenceDatabase database)
		{
			var newLog = Logs.AddNew(Events.ClickThroughAgreementExecuted, FormattableString.Invariant($"{WebContractLogReference} {database.EnterpriseCode}-{database.LD_ServerCode}"));
			return newLog;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		internal const string WebContractLogReference = "MyAccount Disclaimer Authentication Staff Profile Collection Acknowledged by User via CargoWise One System";

		#region Notification Roles

		public ZBool IsAccountsReceivableContact
		{
			get => HasDocumentGroup(ContactType.Receivables.Code);
			set => AddDocumentGroup(ContactType.Receivables.Code, value, IsAccountsReceivableContactInfo);
		}

		public ZBool IsBorderWiseAdministrator
		{
			get => HasDocumentGroup(EDIOrgDocumentGroupTypes.Codes.BorderWiseAdministrator);
			set => AddDocumentGroup(EDIOrgDocumentGroupTypes.Codes.BorderWiseAdministrator, value, IsBorderWiseAdministratorInfo);
		}

		public ZBool IsERequestApprover
		{
			get => HasDocumentGroup(EDIOrgDocumentGroupTypes.Codes.ERequestPendingApprovals);
			set => AddDocumentGroup(EDIOrgDocumentGroupTypes.Codes.ERequestPendingApprovals, value, IsERequestApproverInfo);
		}

		public ZBool IsInformationServicesTechnicalAdministrator
		{
			get => HasDocumentGroup(EDIOrgDocumentGroupTypes.Codes.InformationServicesTechnicalAdministrator);
			set => AddDocumentGroup(EDIOrgDocumentGroupTypes.Codes.InformationServicesTechnicalAdministrator, value, IsInformationServicesTechnicalAdministratorInfo);
		}

		public ZBool IsCertificationProgramContact
		{
			get => HasDocumentGroup(EDIOrgDocumentGroupTypes.Codes.CertificationProgramContact);
			set => AddDocumentGroup(EDIOrgDocumentGroupTypes.Codes.CertificationProgramContact, value, IsCertificationProgramContactInfo);
		}

		public ZPropertyInfo IsAccountsReceivableContactInfo => GetZPropertyInfo(Schema.IsAccountsReceivableContact);
		public ZPropertyInfo IsBorderWiseAdministratorInfo => GetZPropertyInfo(Schema.IsBorderWiseAdministrator);
		public ZPropertyInfo IsERequestApproverInfo => GetZPropertyInfo(Schema.IsERequestApprover);
		public ZPropertyInfo IsInformationServicesTechnicalAdministratorInfo => GetZPropertyInfo(Schema.IsInformationServicesTechnicalAdministrator);
		public ZPropertyInfo IsCertificationProgramContactInfo => GetZPropertyInfo(Schema.IsCertificationProgramContact);

		#endregion

		#endregion

		#region User Management

		public EdiCustomerUserAccount GetMostRecentUnlinkedUserAccount()
		{
			var query = new ZDBOnlyQuery(typeof(EdiCustomerUserAccount));
			query.AddToFilter(EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, PK);
			query.AddToFilter(EdiCustomerUserAccountSchema.EUA_IsContactRelationshipActive, false);
			query.AddToFilter(EdiCustomerUserAccountSchema.EUA_ContactRelationshipStatus, SQLComparisonOperator.NotEqual, ContactRelationshipStatusList.Codes.SelfDeactivation);

			var subQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.PK);
			subQuery.AddToFilter(LicenceDatabaseSchema.LD_OH_WebAccessOrg, OC_OH);
			query.AddSubQuery(EdiCustomerUserAccountSchema.EUA_LD, subQuery, JoinCondition.And);
			query.OrderBy = EdiCustomerUserAccountSchema.Constants.EUA_SystemVerifiedDateUtc + OrderByClause.Descending;

			return Factory.LoadTop1<EdiCustomerUserAccount>(query);
		}

		#endregion

		#region Web Access Superseded

		protected override bool CanSupersede
		{
			get
			{
				var dbQuery = new ZQuery(LicenceDatabaseSchema.LD_OH_WebAccessOrg, OC_OH);
				var userAccountQuery = new ZQuery(EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, PK);
				return !Factory.Exists(typeof(LicenceDatabase), dbQuery) || !Factory.Exists(typeof(EdiCustomerUserAccount), userAccountQuery);
			}
		}

		public override string CannotSupersedeReason => Res.GetString("cfa00a5e-6ae3-42d2-9656-2ee7013be88c", "The following contact(s) could not be superseded because they are part of a Master Organization");

		#endregion

		protected override OrgContactValidation GetNewValidation() => new EDIOrgContactValidation(this);

		#region BindingOnly

		public ZString CompanyNameForBindingOnly => ((IGlbPersonPrimarySource)this).CompanyName;
		public ZPropertyInfo CompanyNameForBindingOnlyInfo => GetZPropertyInfo(Schema.CompanyNameForBindingOnly);
		public ZString BranchForBindingOnly => BranchAddress != null ? BranchAddress.OA_Code : Header.MainAddress.OA_Code;
		public ZPropertyInfo BranchForBindingOnlyInfo => GetZPropertyInfo(Schema.BranchForBindingOnly);

		#endregion

		protected override void OnFactorySaving()
		{
			if (!HasChanges)
			{
				base.OnFactorySaving();
				return;
			}
			if (IsInformationServicesTechnicalAdministrator != hasISTPreviousValue)
			{
				hasISTChange = ZBool.True;
			}
			LogChanges();
			base.OnFactorySaving();
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded && HasChanges)
			{
				hasISTPreviousValue = IsInformationServicesTechnicalAdministrator;
				hasISTChange = ZBool.False;
			}
		}

		void LogChanges()
		{
			if (IsInDatabase && OC_IsActiveInfo.HasChanges && !OC_IsActive)
			{
				var isVersionReportContext = Factory.HasContext(EDIConstants.BusinessContext.VersionReportContactImporter);
				foreach (var database in Factory.Load<LicenceDatabase>(new ZQuery(LicenceDatabaseSchema.LD_OC_ContractInstallerOrInternalTechContact, PK)))
				{
					var reference = $"Tech Contact {OC_ContactName} set to Inactive{(isVersionReportContext ? " via Version Report" : "")}.";
					database.Logs.AddNew(AutoEvents.SetToInactive, reference);
				}
			}
			if (IsInDatabase && IsInformationServicesTechnicalAdministrator && !OC_IsActive && OC_IsActiveInfo.HasChanges)
			{
				var orgLog = $"InformationServicesTechnicalAdministrator {OC_ContactName} set to Inactive.";
				var orgHead = Factory.Load<OrgHeader>(OC_OH);
				orgHead.Logs.AddNew(AutoEvents.SetToInactive, orgLog);
			}
			if (IsInDatabase && !IsInformationServicesTechnicalAdministrator && OC_IsActive && hasISTChange)
			{
				var orgLog = $"Active Contact: {OC_ContactName} unassigned InformationServicesTechnicalAdministrator.";
				var orgHead = Factory.Load<OrgHeader>(OC_OH);
				orgHead.Logs.AddNew(AutoEvents.SetToInactive, orgLog);
			}
		}
	}
}
