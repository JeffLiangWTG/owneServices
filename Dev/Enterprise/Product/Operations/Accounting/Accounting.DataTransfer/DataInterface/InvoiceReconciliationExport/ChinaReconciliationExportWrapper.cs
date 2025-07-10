using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.DataInterface
{
	public class ChinaReconciliationExportWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ChinaReconciliationExportWrapper()
			: this(new BusinessObjectFactory())
		{
		}

		public ChinaReconciliationExportWrapper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ComplianceSubType = "ALL";
			ExportStatus = "NEX";
		}

		#region Schema

		public abstract class Schema
		{
			public const string PostDateFrom = "PostDateFrom";
			public const string PostDateTo = "PostDateTo";
			public const string Branch = "Branch";
			public const string ComplianceSubType = "ComplianceSubType";
			public const string ExportStatus = "ExportStatus";
		}

		#endregion

		#region Public Properties

		#region Post Date From

		public ZDate PostDateFrom
		{
			get { return fPostDateFrom; }
			set
			{
				if (fPostDateFrom != value)
				{
					SetNonPersistentPropertyValue(PostDateFromInfo, ref fPostDateFrom, value);
					if (!IsValidationSuspended)
					{
						ValidatePostDateFrom();
						ValidatePostDateTo();
					}
				}
			}
		}

		public ZPropertyInfo PostDateFromInfo
		{
			get { return GetZPropertyInfo(Schema.PostDateFrom); }
		}

		#endregion

		#region Post Date To

		public ZDate PostDateTo
		{
			get { return fPostDateTo; }
			set
			{
				if (fPostDateTo != value)
				{
					SetNonPersistentPropertyValue(PostDateToInfo, ref fPostDateTo, value);
					if (!IsValidationSuspended)
					{
						ValidatePostDateTo();
						ValidatePostDateFrom();
					}
				}
			}
		}

		public ZPropertyInfo PostDateToInfo
		{
			get { return GetZPropertyInfo(Schema.PostDateTo); }
		}

		#endregion

		#region Branch

		[List("Branches")]
		public ZGuid Branch
		{
			get { return fBranch; }
			set
			{
				if (fBranch != value)
				{
					SetNonPersistentPropertyValue(BranchInfo, ref fBranch, value);
					if (!IsValidationSuspended)
					{
						ValidateBranch();
					}
				}
			}
		}

		public ZPropertyInfo BranchInfo
		{
			get { return GetZPropertyInfo(Schema.Branch); }
		}

		public ZString BranchCode
		{
			get
			{
				var selectedBranch = Factory.Load<GlbBranch>(Branch);
				return selectedBranch == null ? ZString.Empty : selectedBranch.GB_Code;
			}
		}

		#endregion

		#region Compliance Sub Type

		[List("ComplianceSubTypeList")]
		public ZString ComplianceSubType
		{
			get
			{
				return fComplianceSubType;
			}
			set
			{
				if (fComplianceSubType != value)
				{
					SetNonPersistentPropertyValue(ComplianceSubTypeInfo, ref fComplianceSubType, value);
					if (!IsValidationSuspended)
					{
						ValidateComplianceSubType();
					}
				}
			}
		}

		public ZPropertyInfo ComplianceSubTypeInfo
		{
			get { return GetZPropertyInfo(Schema.ComplianceSubType); }
		}

		ICodeDescriptionPairList fComplianceSubTypeList;

		public ICodeDescriptionPairList ComplianceSubTypeList
		{
			get
			{
				if (fComplianceSubTypeList == null)
				{
					fComplianceSubTypeList = new CodeDescriptionPairList();
					fComplianceSubTypeList.Add(new CodeDescriptionPair("ALL", ResString.GetMultilingualString("c82efb2b-cad4-41d7-a97f-f3b654c06983", "All Invoices")));// Hard Code Type
					fComplianceSubTypeList.Add(new CodeDescriptionPair("TAX", ResString.GetMultilingualString("9c317640-c004-4e7b-9572-4175cdc23828", "Tax Invoices Only")));// Hard Code Type
					fComplianceSubTypeList.Add(new CodeDescriptionPair("NTX", ResString.GetMultilingualString("109b2b1e-85c3-4b0f-abde-f7c2e1cd4e59", "Non-Tax Invoices Only")));// Hard Code Type
					fComplianceSubTypeList.Add(new CodeDescriptionPair("TXA", ResString.GetMultilingualString("149f9e98-eca5-4a86-a861-477a003d808f", "'TXA' Invoices Only")));// Hard Code Type
					fComplianceSubTypeList.Add(new CodeDescriptionPair("TXB", ResString.GetMultilingualString("5d368bed-ee5a-4568-94ce-fae09ee5f23d", "'TXB' Invoices Only")));// Hard Code Type
				}
				return fComplianceSubTypeList;
			}
		}

		#endregion

		#region Export Status

		[List("ExportStatusList")]
		public ZString ExportStatus
		{
			get
			{
				return fExportStatus;
			}
			set
			{
				if (fExportStatus != value)
				{
					SetNonPersistentPropertyValue(ExportStatusInfo, ref fExportStatus, value);
					if (!IsValidationSuspended)
					{
						ValidateExportStatus();
					}
				}
			}
		}

		public ZPropertyInfo ExportStatusInfo
		{
			get { return GetZPropertyInfo(Schema.ExportStatus); }
		}

		ICodeDescriptionPairList fExportStatusList;

		public ICodeDescriptionPairList ExportStatusList
		{
			get
			{
				if (fExportStatusList == null)
				{
					fExportStatusList = new CodeDescriptionPairList();
					fExportStatusList.Add(new CodeDescriptionPair("NEX", ResString.GetMultilingualString("94f7d4e4-6319-40fa-b263-4653ec2c234a", "Not Exported Previously Only")));// Hard Code Type
					fExportStatusList.Add(new CodeDescriptionPair("PEX", ResString.GetMultilingualString("65ea2619-1c02-434a-ac13-3cd39e44dae2", "Exported Previously Only")));// Hard Code Type
					fExportStatusList.Add(new CodeDescriptionPair("BTH", ResString.GetMultilingualString("e2f8a691-a203-4f16-8199-c82de75e7703", "Both Exported and Not Exported")));// Hard Code Type
				}
				return fExportStatusList;
			}
		}

		#endregion

		#endregion

		ZDate fPostDateFrom;
		ZDate fPostDateTo;
		ZGuid fBranch;
		ZString fComplianceSubType;
		ZString fExportStatus;

		GlbBranchCollection fBranchesList;

		public GlbBranchCollection Branches
		{
			get { return fBranchesList ?? (fBranchesList = new GlbBranchCollection(Factory, new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK))); }
		}

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			ValidateBranch();
			ValidatePostDateFrom();
			ValidatePostDateTo();
			ValidateComplianceSubType();
			ValidateExportStatus();
		}

		protected void ValidatePostDateFrom()
		{
			PostDateFromInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(PostDateFromInfo);
			if (!PostDateFromInfo.HasErrors())
			{
				if (!IsDateInValidPeriod(PostDateFrom))
				{
					PostDateFromInfo.AddError(InvalidPeriodDateErrorMessage);
				}
				else if (PostDateFrom.IsValid && PostDateTo.IsValid)
				{
					if (PostDateFrom > PostDateTo)
					{
						PostDateFromInfo.AddError(InvalidDateRangeErrorMessage);
					}
					if (!AreDatesInTheSameFinancialYear())
					{
						PostDateFromInfo.AddError(DateRangeDifferentYearErrorMessage);
					}
				}
			}
		}

		protected void ValidatePostDateTo()
		{
			PostDateToInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(PostDateToInfo);
			if (!PostDateToInfo.HasErrors())
			{
				if (!IsDateInValidPeriod(PostDateTo))
				{
					PostDateToInfo.AddError(InvalidPeriodDateErrorMessage);
				}
				else if (PostDateFrom.IsValid && PostDateTo.IsValid)
				{
					if (PostDateFrom > PostDateTo)
					{
						PostDateToInfo.AddError(InvalidDateRangeErrorMessage);
					}
					if (!AreDatesInTheSameFinancialYear())
					{
						PostDateToInfo.AddError(DateRangeDifferentYearErrorMessage);
					}
				}
			}
		}

		protected void ValidateBranch()
		{
			BranchInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(BranchInfo, Branches);
		}

		protected void ValidateComplianceSubType()
		{
			ComplianceSubTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ComplianceSubTypeInfo);
			ListValidation.ErrorIfInvalidCode(ComplianceSubTypeInfo, ComplianceSubTypeList);
		}

		protected void ValidateExportStatus()
		{
			ExportStatusInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ExportStatusInfo);
			ListValidation.ErrorIfInvalidCode(ExportStatusInfo, ExportStatusList);
		}

		bool AreDatesInTheSameFinancialYear()
		{
			bool result = true;
			if (IsDateInValidPeriod(PostDateFrom) && IsDateInValidPeriod(PostDateTo))
			{
				string periodFrom = PeriodCalculator.GetPeriodFromDate(PostDateFrom).ToString();
				string periodTo = PeriodCalculator.GetPeriodFromDate(PostDateTo).ToString();
				if (periodFrom.Substring(0, 4) != periodTo.Substring(0, 4))
				{
					result = false;
				}
			}
			return result;
		}

		bool IsDateInValidPeriod(ZDateTime date)
		{
			return !date.IsValid || (PeriodCalculator.GetPeriodFromDate(date) != 0);
		}

		AccountingPeriodCalculator PeriodCalculator
		{
			get
			{
				if (periodCalculator == null)
				{
					periodCalculator = new AccountingPeriodCalculator(Factory);
				}
				return periodCalculator;
			}
		}

		AccountingPeriodCalculator periodCalculator;

		static string InvalidDateRangeErrorMessage
		{
			get { return Res.GetString("c021c00c-8e57-4de9-ad11-41bce6dbc2b7", "'Post Date To' must be greater than 'Post Date From'"); }
		}

		static string DateRangeDifferentYearErrorMessage
		{
			get { return Res.GetString("86b2d349-d640-4323-b599-7b7a9ed9f489", "'Post Date From' and 'Post Date To' must be within a same financial year"); }
		}

		static string InvalidPeriodDateErrorMessage
		{
			get { return Res.GetString("18d5e688-2a5b-42f3-af57-77fa7db4729a", "The date entered is not in the accounting periods"); }
		}

		#endregion
	}
}
