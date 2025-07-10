using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.Business.AccountingConstants;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class ARCreditNoteApprovalRequest : InvoicingBaseApprovalRequest<ARCreditNoteApprovalRequestDetails>, IDataVersionLoggingSupported
	{
		public ARCreditNoteApprovalRequest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public void InitializeTransactionLinesFromPostingDetails(InvoicingBase amendingCreditNote)
		{
			amendingCreditNote.Lines.RemoveAndDeleteAll();

			var jobNumberToPKMappingResult = PrepareJobNumberToPKMappingForConsolOrPeriodicInvoice();
			var isAmendingConsolOrPeriodicInvoice = jobNumberToPKMappingResult.Item1;
			var jobPksMapping = jobNumberToPKMappingResult.Item2;

			foreach (ARCreditNoteApprovalRequestChargeDetails chargeDetails in PostingDetails.Charges)
			{
				var arCreditNoteLine = amendingCreditNote.Lines.AddNew() as ARCreditNoteLine;
				bool isDetailsSetOnTransactionHeader = false;
				if (!isDetailsSetOnTransactionHeader)
				{
					amendingCreditNote.AH_RX_NKTransactionCurrency = chargeDetails.SellCurrency;
					amendingCreditNote.ExchangeRate.Rate = chargeDetails.ExchangeRate;
					IAmending amending = (IAmending)amendingCreditNote;
					amending.AmendingReasonCode = XP_ReasonCode;
					amending.AmendingReason = AccountingMasterFilesRegistry.Instance.CreditNoteReasonCodesList.Value.GetDescriptionFromCode(XP_ReasonCode);
					amendingCreditNote.AH_InvoiceDate = PostingDetails.InvoiceDate;
					amendingCreditNote.AH_PostDate = PostingDetails.PostDate;
					isDetailsSetOnTransactionHeader = true;
				}

				if (!chargeDetails.JobNumber.IsEmpty)
				{
					if (!isAmendingConsolOrPeriodicInvoice)    //we can only set AL_JH from AH_JH if the original AR invoice is not a consol invoice or periodic invoice
					{
						arCreditNoteLine.AL_JH = amendingCreditNote.AH_JH;
					}
					else
					{
						if (jobPksMapping.TryGetValue(chargeDetails.JobNumber, out var jobPk))
						{
							arCreditNoteLine.AL_JH = jobPk;
						}
					}
				}

				SetPKFromCode(chargeDetails.ChargeCode, arCreditNoteLine.GenericChargeInfo, arCreditNoteLine);
				SetPKFromCode(chargeDetails.Branch, arCreditNoteLine.AL_GBInfo, arCreditNoteLine);
				SetPKFromCode(chargeDetails.Department, arCreditNoteLine.AL_GEInfo, arCreditNoteLine);

				arCreditNoteLine.AL_PlaceOfSupply = chargeDetails.PlaceOfSupply;
				arCreditNoteLine.AL_PlaceOfSupplyType = chargeDetails.PlaceOfSupplyType;
				arCreditNoteLine.AL_SupplyType = chargeDetails.SupplyType;

				SetPKFromCode(chargeDetails.TaxCode, arCreditNoteLine.AL_ATInfo, arCreditNoteLine);

				if (!chargeDetails.Description.IsEmpty)
				{
					arCreditNoteLine.AL_Desc = chargeDetails.Description;
				}
				if (!chargeDetails.TaxDate.IsEmpty)
				{
					arCreditNoteLine.AL_TaxDate = chargeDetails.TaxDate;
				}
				arCreditNoteLine.AL_A9_VATClass = chargeDetails.AccInvMsgPK;

				arCreditNoteLine.AL_OSExTaxAmount = -(chargeDetails.OSSellAmount - chargeDetails.OSTaxAmount);
				arCreditNoteLine.AL_LocalExTaxAmount = -(chargeDetails.LocalSellAmount - chargeDetails.LocalTaxAmount);
				arCreditNoteLine.AL_OSTaxAmount = -(chargeDetails.OSTaxAmount);
				arCreditNoteLine.AL_LocalTaxAmount = -(chargeDetails.LocalTaxAmount);
			}

			if (!PostingDetails.Description.IsEmpty)
			{
				amendingCreditNote.AH_Desc = PostingDetails.Description;
			}
			if (!PostingDetails.InvoiceTerm.IsEmpty)
			{
				amendingCreditNote.AH_InvoiceTerm = PostingDetails.InvoiceTerm;
				amendingCreditNote.AH_InvoiceTermDays = PostingDetails.InvoiceTermDays;
			}

			void SetPKFromCode(ZString code, ZPropertyInfo property, BusinessObject bo)
			{
				var collection = MetaData.GetListDataSource(bo, property.PropertyDescriptor) as IFindBoxListProvider;
				property.Value = collection.PrimaryKeyFromCode(code);
			}
		}

		Tuple<bool, Dictionary<ZString, ZGuid>> PrepareJobNumberToPKMappingForConsolOrPeriodicInvoice()
		{
			var isAmendingConsolOrPeriodicInvoice = false;
			var jobPksMapping = new Dictionary<ZString, ZGuid>();
			var parent = Parent as TransactionHeader;
			if (parent != null)
			{
				var originalInvoice = Factory.Load<InvoicingBase>(parent.PK);
				if (originalInvoice != null)
				{
					if (originalInvoice.IsConsolInvoice || originalInvoice.IsPeriodicInvoice)
					{
						isAmendingConsolOrPeriodicInvoice = true;

						var jobNumbers = PostingDetails.Charges.OfType<ARCreditNoteApprovalRequestChargeDetails>()
							.Where(x => !x.JobNumber.IsEmpty)
							.Select(x => x.JobNumber).Distinct();
#if NETFRAMEWORK
						var jobNumberChunks = jobNumbers.Chunk(20);
#else
						var jobNumberChunks = System.Linq.Enumerable.Chunk(jobNumbers, 20);
#endif

						foreach (var jobNumberChunk in jobNumberChunks)
						{
							var query = new ZQuery(JobHeaderSchema.JH_GC, RequestingBranch.GB_GC);
							query.AddToFilter(JobHeaderSchema.JH_JobNum, jobNumberChunk);
							var jobHeaders = Factory.Load<JobHeader>(query);
							jobHeaders.ForEach(x => jobPksMapping.Add(x.JH_JobNum, x.PK));
						}
					}
				}
			}
			return new Tuple<bool, Dictionary<ZString, ZGuid>>(isAmendingConsolOrPeriodicInvoice, jobPksMapping);
		}

		public void SetLevel1ApproverToCurrentUserForSEQRequest()
		{
			if (PostingDetails.ApprovingOption == ApprovalCredentialOption.SequentialLogin)
			{
				// special case for level 1 approver when mode is SEQ
				Guid toGuid(ZGuid pk) => pk.IsValid ? pk.ToGuid() : Guid.Empty;
				var currentStaffCode = Factory.GetCachedReadOnlyFactory().Load<GlbStaff>(Env.CurrentUser.PK).GS_Code;
				var loginController = new UserLoginController();
				var userSecurity = loginController.GetSecurityForUser(Env.CurrentUser.LoginName, toGuid(XP_GB_JobBranch), toGuid(XP_GE_JobDepartment));
				if (MaxAuthorisationLevelRequired == 0)
				{
					XP_GS_NKApprovingUser1 = currentStaffCode;
					XP_ApprovalStatus = GenApprovalRequestApprovalStatus.Approved;
				}
				else if (userSecurity.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed)
				{
					XP_GS_NKApprovingUser1 = currentStaffCode;
				}
			}
		}

		public bool IsEligibleToAutoPostARCreditNote => XP_ApprovalType == Constants.GenApprovalRequestApprovalType.ARCreditNote && XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Approved && IsJobRelated;

		public bool IsEligibleToAutoPostAmendingARCreditNote => XP_ApprovalType == Constants.GenApprovalRequestApprovalType.ARCreditNote && XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Approved && IsTransactionRelated
			&& !XP_ReasonCode.IsEmpty && PostingDetails.IsEligibleToAutoPostAmendingARCreditNote;

		public bool IsEligibleToAutoPostARCreditNoteReversal => XP_ApprovalType == Constants.GenApprovalRequestApprovalType.ARCreditNoteForReversal && XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Approved && IsTransactionRelated;

		public InvoicingBase CreateARCreditNoteFromRequest()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var transaction = (InvoicingBase)factory.GetCachedReadOnlyFactory().New(ApprovingTransactionType);
			transaction.AH_GB = XP_GB_JobBranch.IsEmpty ? Env.CurrentBranchPK : XP_GB_JobBranch;
			transaction.AH_GE = XP_GE_JobDepartment.IsEmpty ? Env.CurrentDepartmentPK : XP_GE_JobDepartment;
			transaction.AH_LocalExTaxAmount = PostingDetails.MaxAmountToApprove;
			if (!PostingDetails.Description.IsEmpty)
			{
				transaction.AH_Desc = PostingDetails.Description;
			}
			if (!PostingDetails.InvoiceTerm.IsEmpty)
			{
				transaction.AH_InvoiceTerm = PostingDetails.InvoiceTerm;
				transaction.AH_InvoiceTermDays = PostingDetails.InvoiceTermDays;
			}
			if (transaction is ARInvoice)
			{
				transaction.IsCreatingCreditNoteForReversal = true;
			}
			return transaction;
		}

		public void Initialize(InvoicingBase[] creditNotes, ZGuid parentID, string parentTableCode, JobInvoicingPostingOption postingOption)
		{
			Initialize(creditNotes, parentID, parentTableCode, postingOption, string.Empty, string.Empty);
		}

#if DEBUG
		internal
#endif
		void Initialize(InvoicingBase[] creditNotes, ZGuid parentID, string parentTableCode, JobInvoicingPostingOption postingOption, string branchCode, string departmentCode)
		{
			base.Initialize(parentID, parentTableCode);
			var firstInvoice = creditNotes.FirstOrDefault();
			InitializeTransactionInfo(firstInvoice);
			InitializeBranchAndDepartment(creditNotes, branchCode, departmentCode);
			InitializePostingDetailsCharges(creditNotes, branchCode, departmentCode);
			InitializeChildRequests(creditNotes, postingOption);
			PostingDetails.PostingOption = postingOption.ToString();

			Guid toGuidSave(ZGuid pk) => pk.IsEmpty ? Guid.Empty : pk.ToGuid();

			var modeAndSetting = firstInvoice?.GetAuthorizationModeAndSettings(toGuidSave(XP_GB_JobBranch), toGuidSave(XP_GE_JobDepartment));
			if (modeAndSetting != null)
			{
				switch (modeAndSetting.AuthorizationMode)
				{
					case Constants.AuthorizationMode.Codes.Default:
						PostingDetails.ApprovingOption = ApprovalCredentialOption.SingleLogin;
						break;
					case Constants.AuthorizationMode.Codes.TwoApprovers:
						PostingDetails.ApprovingOption = ApprovalCredentialOption.DoubleLogin;
						break;
					case Constants.AuthorizationMode.Codes.SequentialApprovers:
						PostingDetails.ApprovingOption = ApprovalCredentialOption.SequentialLogin;
						break;
					default:
						throw new NotSupportedException(Res.GetString("25C6690C-9CFD-49B7-8EF8-8AE4A1B08D2F", "Unsupported authorization mode {0}", modeAndSetting.AuthorizationMode));
				}

				var setting = modeAndSetting.AuthorisationSettings.ToArray<AmountBasedSixLevelAuthorisationRequirement>();
				var requirement = setting.GetAuthorisationRequired(PostingDetails.MaxAmountToApprove);
				if (requirement != null)
				{
					PostingDetails.MaxAuthorisationLevelRequired = requirement.GetAuthorisationRequirementWeight(requirement.AuthorisationRequirement);
				}
			}

			UpdateIsFinalApproval();

			if (!IsParentRequest)
			{
				new ARCreditNoteApprovalBulk(Factory, new DefaultAccessSecurityProvider(), this).Approve(true);
			}
		}

		void InitializeBranchAndDepartment(InvoicingBase[] creditNotes, string branchCode, string departmentCode)
		{
			switch (XP_ParentTableCode)
			{
				case JobHeaderSchema.Constants.Prefix:
					XP_GB_JobBranch = (Parent as Job)?.Branch?.PK ?? ZGuid.Empty;
					XP_GE_JobDepartment = (Parent as Job)?.Department?.PK ?? ZGuid.Empty;
					break;
				case AccTransactionHeaderSchema.Constants.Prefix:
					XP_GB_JobBranch = (Parent as TransactionHeader)?.Branch?.PK ?? ZGuid.Empty;
					XP_GE_JobDepartment = (Parent as TransactionHeader)?.Department?.PK ?? ZGuid.Empty;
					break;
				case JobConsolSchema.Constants.Prefix:
					XP_GB_JobBranch = creditNotes?.FirstOrDefault()?.AH_GB ?? ZGuid.Empty;
					XP_GE_JobDepartment = creditNotes?.FirstOrDefault()?.AH_GE ?? ZGuid.Empty;
					break;
				case GenApprovalRequestSchema.Constants.Prefix:
					XP_GB_JobBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, branchCode))?.PK ?? ZGuid.Empty;
					XP_GE_JobDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, departmentCode))?.PK ?? ZGuid.Empty;
					break;
				default:
					XP_GE_JobDepartment = ZGuid.Empty;
					XP_GB_JobBranch = ZGuid.Empty;
					break;
			}
		}

		void InitializeChildRequests(InvoicingBase[] creditNotes, JobInvoicingPostingOption postingOption)
		{
			if (IsParentRequest && AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.Value)
			{
				var multiplier = GetMultiplier(creditNotes.FirstOrDefault());

				foreach (var chargesGroupedByBranchAndDept in PostingDetails.Charges.Cast<ARCreditNoteApprovalRequestChargeDetails>().GroupBy(x => new { Branch = x.Branch, Department = x.Department }))
				{
					if (chargesGroupedByBranchAndDept.Sum(x => x.LocalSellAmount * multiplier) > 0)
					{
						var childRequest = Factory.New<ARCreditNoteApprovalRequest>();
						childRequest.XP_ApprovalType = XP_ApprovalType;
						using (childRequest.GetValidationSuspender())
						{
							childRequest.XP_ReasonCode = XP_ReasonCode;
							childRequest.XP_ReasonDescription = XP_ReasonDescription;
						}
						var creditNotesChildList = creditNotes.Where(x => chargesGroupedByBranchAndDept.Select(y => y.SellAccount).Contains(x.Header.OH_Code)).ToArray();
						childRequest.Initialize(creditNotesChildList, PK, GenApprovalRequestSchema.Constants.Prefix, postingOption, chargesGroupedByBranchAndDept.Key.Branch, chargesGroupedByBranchAndDept.Key.Department);
					}
				}
			}
		}

		void InitializePostingDetailsCharges(InvoicingBase[] creditNotes, string branchCode, string departmentCode)
		{
			PostingDetails.Charges.RemoveAndDeleteAll();
			ZDecimal maxAmountForAuthorisation = ZDecimal.Zero;

			foreach (InvoicingBase creditNote in creditNotes)
			{
				var lines = !IsParentRequest ?
					creditNote.Lines.Cast<InvoicingLineBase>().Where(x => x.Branch.GB_Code == branchCode && x.Department.GE_Code == departmentCode) :
					creditNote.Lines.Cast<InvoicingLineBase>();

				var linesLocalTotalAmount = !IsParentRequest ?
					Math.Abs(lines.Sum(x => x.AL_LineAmount + x.AL_GSTVAT)) :
					(decimal)creditNote.AH_LocalTotalAmount;

				foreach (var line in lines)
				{
					var charge = PostingDetails.Charges.AddNew();
					charge.JobNumber = line.JobNumber;
					charge.ChargeCode = line.GenericChargeBizO.VC_Code;
					charge.Branch = line.Branch.GB_Code;
					charge.Department = line.Department.GE_Code;
					charge.SellAccount = creditNote.Header.OH_Code;
					charge.SellCurrency = line.AL_RX_NKTransactionCurrency;
					charge.OSSellAmount = line.AL_OSAmount;
					charge.LocalSellAmount = line.AL_LineAmount + line.AL_GSTVAT;
					charge.InvoiceType = creditNote.AH_TransactionCategory;
					charge.OSTaxAmount = GetMultiplier(creditNote) * line.AL_OSTaxAmount;
					charge.LocalTaxAmount = line.AL_GSTVAT;
					charge.PlaceOfSupply = line.AL_PlaceOfSupply;
					charge.PlaceOfSupplyType = line.AL_PlaceOfSupplyType;
					charge.SupplyType = line.AL_SupplyType;
					if (line.TaxRate != null)
					{
						charge.TaxCode = line.TaxRate.AT_Code;
						if (!line.AL_TaxDate.IsEmpty)
						{
							charge.TaxDate = line.AL_TaxDate;
						}
					}
					charge.ExchangeRate = line.AL_ExchangeRate;
					if (!line.AL_Desc.IsEmpty)
					{
						charge.Description = line.AL_Desc;
					}
					charge.AccInvMsgPK = line.AL_A9_VATClass;
				}

				if (linesLocalTotalAmount > maxAmountForAuthorisation)
				{
					maxAmountForAuthorisation = linesLocalTotalAmount;
				}
			}

			PostingDetails.MaxAmountToApprove = maxAmountForAuthorisation;
		}

		int GetMultiplier(InvoicingBase creditNote)
		{
			var result = 1;
			if (creditNote != null && creditNote.AH_TransactionType == TransactionTypes.CreditNote)
			{
				result = -1;
			}
			return result;
		}

		void InitializeTransactionInfo(InvoicingBase creditNote)
		{
			if (creditNote != null)
			{
				if (!string.IsNullOrEmpty(creditNote.AH_ReceiptType))
				{
					XP_ReasonCode = creditNote.AH_ReceiptType;
				}
				PostingDetails.InvoiceDate = creditNote.AH_InvoiceDate;
				PostingDetails.PostDate = creditNote.AH_PostDate;
				if (!creditNote.AH_Desc.IsEmpty)
				{
					PostingDetails.Description = creditNote.AH_Desc;
				}
				if (!creditNote.AH_InvoiceTerm.IsEmpty)
				{
					PostingDetails.InvoiceTerm = creditNote.AH_InvoiceTerm;
					PostingDetails.InvoiceTermDays = creditNote.AH_InvoiceTermDays;
				}
			}
		}

		public void ChangeApprovalTypeForInvoiceReversal()
		{
			XP_ApprovalType = Constants.GenApprovalRequestApprovalType.ARCreditNoteForReversal;
		}

		#region Properties

		#region FormattedChargeDetails

		[ResourceStringData("ChargeDetails", Caption = "Charge Codes")]
		public ZString FormattedChargeDetails
		{
			get
			{
				if (!FormattedChargeDetails_cached.HasValue)
				{
					var amounts =
						from ARCreditNoteApprovalRequestChargeDetails charge in PostingDetails.Charges
						group charge by new { SellAccount = charge.SellAccount, SellCurrency = charge.SellCurrency, InvoiceType = charge.InvoiceType } into chargeGroup
						let osCurrency = RefCurrency.LoadFromCurrencyCode(Factory, chargeGroup.Key.SellCurrency)
						let decimals = osCurrency != null ? osCurrency.Decimals : Env.CurrentCompany.LocalCurrency.Decimals
						select string.Format("{0} {1} {2}: {3}", chargeGroup.Key.SellAccount, chargeGroup.Key.SellCurrency, chargeGroup.Key.InvoiceType,
																((ZDecimal)chargeGroup.Sum(charge => charge.OSSellAmountForDisplay)).ToString(decimals));
					FormattedChargeDetails_cached = new ZStringBuilder(amounts).ToStringWithDelimiterBetweenAppends(", ");
				}
				return FormattedChargeDetails_cached ?? ZString.Empty;
			}
		}
		ZString? FormattedChargeDetails_cached;

		void ResetFormattedChargeDetails()
		{
			FormattedChargeDetails_cached = null;
		}

		#endregion

		#region MaxAuthorisationLevelRequired

		[ResourceStringData("MaxAuthorisationLevelRequired", Caption = "Approval Level Req.")]
		public ZInt MaxAuthorisationLevelRequired
		{
			get
			{
				return PostingDetails.MaxAuthorisationLevelRequired != 0
					? PostingDetails.MaxAuthorisationLevelRequired
					: this.GetRequiredAuthorisationLevelFromRegistry();
			}
		}

		[ResourceStringData("MaxAuthorisationLevelRequiredForDisplay", Caption = "Approval Level Req.")]
		public ZString MaxAuthorisationLevelRequiredForDisplay => MaxAuthorisationLevelRequired == 0
																? string.Empty
																: MaxAuthorisationLevelRequired.ToString();

		#endregion

		#region NextAuthorisationLevelRequired

		[ResourceStringData("NextAuthorisationLevelRequired", Caption = "Next Level Req.")]
		public ZInt NextAuthorisationLevelRequired
		{
			get
			{
				ZInt result = 0;
				if (XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Requested)
				{
					if (ApprovingOption == Constants.AuthorizationMode.Codes.SequentialApprovers)
					{
						if (MaxAuthorisationLevelRequired == 0)
						{
							result = 0;
						}
						else if (MaxAuthorisationLevelRequired == 1) // special case for level 1 which requires 2 approvers both has level 1 right
						{
							result = 1;
						}
						else
						{
							result = this.GetNumApproversFilled() + 1;
							if (result == 7)
							{
								result = 0;
							}
						}
					}
					else
					{
						result = MaxAuthorisationLevelRequired;
					}
				}
				return result;
			}
		}

		[ResourceStringData("NextAuthorisationLevelRequiredForDisplay", Caption = "Next Level Req.")]
		public ZString NextAuthorisationLevelRequiredForDisplay => NextAuthorisationLevelRequired == 0
																? string.Empty
																: NextAuthorisationLevelRequired.ToString();

		#endregion

		#region ApprovingOption

		[ResourceStringData("ApprovingOption", Caption = "Approving Option")]
		public ZString ApprovingOption => PostingDetails.ApprovingOption;

		[ResourceStringData("ApprovingMode", Caption = "Approving Mode")]
		public ZString ApprovingModeForDisplay
		{
			get
			{
				switch (PostingDetails.ApprovingOption)
				{
					case ApprovalCredentialOption.SingleLogin:
						return FormattableString.Invariant($"{AuthorizationMode.Codes.Default} - {AuthorizationMode.Descriptions.Default}");
					case ApprovalCredentialOption.DoubleLogin:
						return FormattableString.Invariant($"{AuthorizationMode.Codes.TwoApprovers} - {AuthorizationMode.Descriptions.TwoApprovers}");
					case ApprovalCredentialOption.SequentialLogin:
						return FormattableString.Invariant($"{AuthorizationMode.Codes.SequentialApprovers} - {AuthorizationMode.Descriptions.SequentialApprovers}");
					default:
						return "";
				}
			}
		}

		#endregion

		#endregion

		[ResourceStringData("ApprovingOptionForDisplay", Caption = "Approving Option", ShortCaption = "Approving Opt.")]
		public ZString ApprovingOptionForDisplay
		{
			get
			{
				var transaction = CreateARCreditNoteFromRequest();
				var levelRequired = transaction.RequiredAuthorisationLevel;
				return AccountingUtils.ConvertApprovingOptionToHumanReadableName(levelRequired, PostingDetails.ApprovingOption);
			}
		}

		#region Overrides

		public override ZString XP_ApprovalType
		{
			get => base.XP_ApprovalType;
			set
			{
				base.XP_ApprovalType = value;
				ChildRequests.ForEach(x => x.XP_ApprovalType = XP_ApprovalType);
			}
		}

		public override ZString XP_ReasonCode
		{
			get => base.XP_ReasonCode;
			set
			{
				base.XP_ReasonCode = value;
				ChildRequests.ForEach(x => x.XP_ReasonCode = XP_ReasonCode);
			}
		}

		public override ZString XP_ReasonDescription
		{
			get => base.XP_ReasonDescription;
			set
			{
				base.XP_ReasonDescription = value;
				ChildRequests.ForEach(x => x.XP_ReasonDescription = XP_ReasonDescription);
			}
		}

		public override ZString XP_ApprovalStatus
		{
			get { return base.XP_ApprovalStatus; }
			set
			{
				if (base.XP_ApprovalStatus != value)
				{
					if (value == Constants.GenApprovalRequestApprovalStatus.Cancelled)
					{
						ChildRequests.Where(y => y.XP_ApprovalStatus != Constants.GenApprovalRequestApprovalStatus.Rejected).ForEach(x => x.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Cancelled);
					}
					else if (value == Constants.GenApprovalRequestApprovalStatus.Posted)
					{
						ChildRequests.ForEach(x => x.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Posted);
					}
					base.XP_ApprovalStatus = value;
				}
			}
		}
		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		internal BusinessObject Parent
		{
			get
			{
				switch (XP_ParentTableCode)
				{
					case JobHeaderSchema.Constants.Prefix:
						return Factory.Load<Job>(XP_ParentID);
					case AccTransactionHeaderSchema.Constants.Prefix:
						return Factory.Load<TransactionHeader>(XP_ParentID);
					case GenApprovalRequestSchema.Constants.Prefix:
						return Factory.Load<GenApprovalRequest>(XP_ParentID);
					default:
						return Factory.Load<CommonConsol>(XP_ParentID);
				}
			}
		}

		public ZBool IsParentRequest => !IsApprovalRequestRelated;

		public ARCreditNoteApprovalRequestCollection ChildRequests => childRequests ?? (childRequests = new ARCreditNoteApprovalRequestCollection(Factory, ChildRequestsFilter));
		ARCreditNoteApprovalRequestCollection childRequests;

		ZQuery ChildRequestsFilter => new ZQuery(GenApprovalRequestSchema.XP_ParentID, PK);

		void RejectLinkedRequests()
		{
			if (!IsParentRequest)
			{
				var rejectedByUser = XP_GS_NKApprovingUser2.IsEmpty ? XP_GS_NKApprovingUser1 : XP_GS_NKApprovingUser2;

				var parentRequest = Factory.Load<ARCreditNoteApprovalRequest>(XP_ParentID);
				parentRequest.XP_GS_NKApprovingUser1 = rejectedByUser;
				parentRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Rejected;
				parentRequest.XP_ApprovalDate = XP_ApprovalDate;

				parentRequest.ChildRequests.Where(x => x.PK != PK).ForEach(childRequest =>
				{
					if (childRequest.XP_GS_NKApprovingUser1.IsEmpty)
					{
						childRequest.XP_GS_NKApprovingUser1 = rejectedByUser;
					}
					else
					{
						childRequest.XP_GS_NKApprovingUser2 = rejectedByUser;
					}
					childRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Cancelled;
					childRequest.XP_ApprovalDate = XP_ApprovalDate;
				});
			}
			else
			{
				ChildRequests.ForEach(childRequest =>
				{
					if (childRequest.XP_GS_NKApprovingUser1.IsEmpty)
					{
						childRequest.XP_GS_NKApprovingUser1 = XP_GS_NKApprovingUser1;
					}
					else
					{
						childRequest.XP_GS_NKApprovingUser2 = XP_GS_NKApprovingUser1;
					}
					childRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Cancelled;
					childRequest.XP_ApprovalDate = XP_ApprovalDate;
				});
			}
		}

		public override bool IsAllowedToChangeStatus(ZString status)
		{
			var result = true;
			if ((!IsParentRequest && status == Constants.GenApprovalRequestApprovalStatus.Cancelled)
				|| (!IsAllowedToApproveRequest && status == Constants.GenApprovalRequestApprovalStatus.Approved))
			{
				result = false;
			}

			return result;
		}

		public override bool IsAllowedToApproveRequest => !IsParentRequest || ChildRequests.All(x => x.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Approved);

		public override bool IsAllowedToCancelRequest => IsParentRequest;

		public override void Delete()
		{
			ChildRequests.DeleteAll();
			base.Delete();
		}

		protected override ZString PostingOptionCore
		{
			get
			{
				return PostingDetails.PostingOption;
			}
		}

		#region IDataVersionLoggingSupported

		bool IDataVersionLoggingSupported.IsDataVersionsAutoLogged => true;

		DataVersionLogValueFormatter IDataVersionLoggingSupported.DataVersionLogValueFormatter => this.GetDefaultDataVersionLogFormatter();

		#endregion

		protected override bool ShouldEmailBeSent
		{
			get
			{
				return base.ShouldEmailBeSent && IsParentRequest;
			}
		}

		public JobInvoicingPostingOption PostingOption
		{
			get
			{
				JobInvoicingPostingOption option;
				if (!Enum.TryParse(PostingOptionCore, out option))
				{
					option = JobInvoicingPostingOption.Revenue;
				}
				return option;
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			XP_ApprovalType = Constants.GenApprovalRequestApprovalType.ARCreditNote;
		}

		protected override GenApprovalRequestValidation GetNewValidation()
		{
			return new ARTransactionApprovalRequestValidation(this);
		}

		protected override void OnAfterReadPostingDetails()
		{
			ResetFormattedChargeDetails();
		}

		protected override ARCreditNoteApprovalRequestDetails CreatePostingApprovalDetails()
		{
			var approvalDetails = new ARCreditNoteApprovalRequestDetails(Factory);
			approvalDetails.ApprovalType = XP_ApprovalType;
			return approvalDetails;
		}

		protected override Type ApprovingTransactionTypeCore
		{
			get
			{
				if (XP_ApprovalType == Constants.GenApprovalRequestApprovalType.ARCreditNoteForReversal)
				{
					return typeof(ARInvoice);
				}
				else
				{
					return typeof(ARCreditNote);
				}
			}
		}

		protected override TransactionApprovalRequestEmail CreateEmail()
		{
			return new ARCreditNoteApprovalRequestEmail(this);
		}

		public bool IsFinalApproval
		{
			get
			{
				if (!isFinalApproval.HasValue)
				{
					UpdateIsFinalApproval();
				}
				return isFinalApproval.Value;
			}
		}
		bool? isFinalApproval;

		public void UpdateIsFinalApproval()
		{
			var finalValue = false;
			if (PostingDetails != null)
			{
				if (PostingDetails.ApprovingOption == ApprovalCredentialOption.SingleLogin)
				{
					finalValue = true;
				}
				else
				{
					var requiredApprovers = PostingDetails.ApprovingOption == ApprovalCredentialOption.SequentialLogin && MaxAuthorisationLevelRequired > 1
						? MaxAuthorisationLevelRequired
						: new ZInt(2);

					if (MaxAuthorisationLevelRequired == 0 || requiredApprovers == this.GetNumApproversFilled() + 1)
					{
						finalValue = true;
					}
				}
			}

			isFinalApproval = finalValue;
		}

		protected override void UpdateApprovalUserAndStatusCore(ZString approvingUser, ZString status)
		{
			if (PostingDetails.ApprovingOption == ApprovalCredentialOption.DoubleLogin
				|| PostingDetails.ApprovingOption == ApprovalCredentialOption.SequentialLogin)
			{
				var requiredApprovers = PostingDetails.ApprovingOption == ApprovalCredentialOption.SequentialLogin && MaxAuthorisationLevelRequired > 1
					? MaxAuthorisationLevelRequired
					: new ZInt(2);

				if (MaxAuthorisationLevelRequired == 0 || requiredApprovers == this.GetNumApproversFilled() + 1 || status != GenApprovalRequestApprovalStatus.Approved)
				{
					XP_ApprovalStatus = status;
					XP_ApprovalDate = ZDateTime.Now;
					UpdatePostDateWhenApproved(status);
				}
				else
				{
					XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
				}

				switch (this.GetNumApproversFilled())
				{
					case 0:
						XP_GS_NKApprovingUser1 = approvingUser;
						break;
					case 1:
						XP_GS_NKApprovingUser2 = approvingUser;
						break;
					case 2:
						XP_GS_NKApprovingUser3 = approvingUser;
						break;
					case 3:
						XP_GS_NKApprovingUser4 = approvingUser;
						break;
					case 4:
						XP_GS_NKApprovingUser5 = approvingUser;
						break;
					case 5:
						XP_GS_NKApprovingUser6 = approvingUser;
						break;
				}
			}
			else
			{
				base.UpdateApprovalUserAndStatusCore(approvingUser, status);
				UpdatePostDateWhenApproved(status);
			}
		}

		void UpdatePostDateWhenApproved(string status)
		{
			if (status == Constants.GenApprovalRequestApprovalStatus.Approved)
			{
				PostingDetails.PostDate = XP_ApprovalDate;
			}
		}

		protected override void FinalizeCancellingCore()
		{
			base.FinalizeCancellingCore();

			if (XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Rejected)
			{
				RejectLinkedRequests();
			}
		}

		#endregion

		#region Job Number, Job Branch and Job Department Text Box Caption

		public ResourceStringData JobNumberTextBoxResString
		{
			get
			{
				var result = JobNumberTextBoxDefaultResString;
				switch (XP_ParentTableCode)
				{
					case JobHeaderSchema.Constants.Prefix:
					case AccTransactionHeaderSchema.Constants.Prefix:
						result = JobNumberTextBoxResStringCore(XP_ParentTableCode);
						break;
					case GenApprovalRequestSchema.Constants.Prefix:
						var parentRequest = Parent as GenApprovalRequest;
						if (parentRequest != null)
						{
							result = JobNumberTextBoxResStringCore(parentRequest.XP_ParentTableCode);
						}
						break;
					default:
						break;
				}
				return result;
			}
		}

		ResourceStringData JobNumberTextBoxResStringCore(ZString parentTableCode)
		{
			var result = JobNumberTextBoxDefaultResString;
			switch (parentTableCode)
			{
				case JobHeaderSchema.Constants.Prefix:
					result = Res.GetData("f9a57bd8-3d22-47ef-accd-e19b4d8551bd", "Job Number");
					break;
				case AccTransactionHeaderSchema.Constants.Prefix:
					result = Res.GetData("c128a08a-4a15-42c1-85bf-6cc1425362f6", "Invoice Number");
					break;
			}
			return result;
		}

		ResourceStringData JobNumberTextBoxDefaultResString => Res.GetData("699aad2b-71ff-4a85-82dd-563fd9b2f53d", "Job/Invoice Number");

		public ResourceStringData JobBranchTextBoxResString
		{
			get
			{
				var result = JobBranchTextBoxDefaultResString;
				switch (XP_ParentTableCode)
				{
					case JobHeaderSchema.Constants.Prefix:
					case AccTransactionHeaderSchema.Constants.Prefix:
						result = JobBranchTextBoxResStringCore(XP_ParentTableCode);
						break;
					case GenApprovalRequestSchema.Constants.Prefix:
						var parentRequest = Parent as GenApprovalRequest;
						if (parentRequest != null)
						{
							result = JobBranchTextBoxResStringCore(parentRequest.XP_ParentTableCode);
						}
						break;
					default:
						break;
				}
				return result;
			}
		}

		ResourceStringData JobBranchTextBoxResStringCore(ZString parentTableCode)
		{
			var result = JobBranchTextBoxDefaultResString;
			switch (parentTableCode)
			{
				case JobHeaderSchema.Constants.Prefix:
					result = Res.GetData("06522599-240f-4f0f-a94a-a007a7ffa1ff", "Job Branch");
					break;
				case AccTransactionHeaderSchema.Constants.Prefix:
					result = Res.GetData("5e73de69-3ccc-4a82-be0d-e11e23d2857d", "Invoice Branch");
					break;
			}
			return result;
		}

		ResourceStringData JobBranchTextBoxDefaultResString => Res.GetData("04eea5cf-effb-46d2-a8ec-de9db68e4f37", "Job/Invoice Branch");

		public ResourceStringData JobDepartmentTextBoxResString
		{
			get
			{
				var result = JobDepartmentTextBoxDefaultResString;
				switch (XP_ParentTableCode)
				{
					case JobHeaderSchema.Constants.Prefix:
					case AccTransactionHeaderSchema.Constants.Prefix:
						result = JobDepartmentTextBoxResStringCore(XP_ParentTableCode);
						break;
					case GenApprovalRequestSchema.Constants.Prefix:
						var parentRequest = Parent as GenApprovalRequest;
						if (parentRequest != null)
						{
							result = JobDepartmentTextBoxResStringCore(parentRequest.XP_ParentTableCode);
						}
						break;
					default:
						break;
				}
				return result;
			}
		}

		ResourceStringData JobDepartmentTextBoxResStringCore(ZString parentTableCode)
		{
			var result = JobDepartmentTextBoxDefaultResString;
			switch (parentTableCode)
			{
				case JobHeaderSchema.Constants.Prefix:
					result = Res.GetData("63f75b5b-db35-4f26-8cbb-282bd88037dd", "Job Department");
					break;
				case AccTransactionHeaderSchema.Constants.Prefix:
					result = Res.GetData("6fb5e233-fab6-4ac0-8c47-3a2030661d9f", "Invoice Department");
					break;
			}
			return result;
		}

		ResourceStringData JobDepartmentTextBoxDefaultResString => Res.GetData("63644dcf-1f83-4e9e-a73b-380f8e8742a7", "Job/Invoice Department");

		#endregion

		#region Realed Requests Grid Title And Description

		public string RelatedRequestsGridDescription
		{
			get
			{
				var result = string.Empty;
				if (IsParentRequest)
				{
					result = Res.GetString("04b1e643-6312-47ab-ab98-e6630a95baf4", "This request can be approved once the associated line-level requests are in the Approved status");
				}
				else
				{
					result = Res.GetString("0d8599d2-aeed-4e62-82e5-40d83feb2af4", "This request is associated with the following header request. Credit note can be posted once the header request is approved.");
				}
				return result;
			}
		}

		public ResourceStringData RelatedRequestsdGridTitle
		{
			get
			{
				var result = ResourceStringData.Empty;
				if (IsParentRequest)
				{
					result = Res.GetData("11826a00-2dc9-4374-83a1-3c4637dc09dc", "Related line-level requests");
				}
				else
				{
					result = Res.GetData("fd90a6da-9aa3-41c9-bcd4-f4d39d2fca31", "Related header request");
				}
				return result;
			}
		}

		public ARCreditNoteApprovalRequestCollection RelatedRequests => IsParentRequest ? ChildRequests : new ARCreditNoteApprovalRequestCollection(Factory, new ZQuery(GenApprovalRequestSchema.PK, XP_ParentID));

		#endregion
	}
}
