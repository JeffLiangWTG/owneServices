using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class ProfitLossFilterProvider : NonPersistentBusinessObject, IObsoleteValidation, IProfitLossFilterProvider
	{
		public ProfitLossFilterProvider(JobProfitLoss parentProfitLoss)
			: base(parentProfitLoss.Factory)
		{
			this.ParentProfitLoss = parentProfitLoss;
		}

		readonly JobProfitLoss ParentProfitLoss;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			RecognizedChargesFilter = "ALL";
		}

		#region Filters

		#region Charge Code

		[List("ChargeCodes")]
		[MaxLength(AccChargeCode.Schema.AC_CodeMaxLength)]
		public ZString ChargeCodeFilter
		{
			get { return fChargeCodeFilter; }
			set
			{
				value = value.TrimEnd(' ');
				CheckMaximumLength(ChargeCodeFilterInfo, value);
				fChargeCodeFilter = value;
				ChargeCodeFilterInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateChargeCodeFilter();
				}
			}
		}

		ZString fChargeCodeFilter;

		public ZPropertyInfo ChargeCodeFilterInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(ChargeCodeFilter));
			}
		}

		public void ValidateChargeCodeFilter()
		{
			ChargeCodeFilterInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(ChargeCodeFilterInfo, ChargeCodes);
		}

		#endregion

		#region Branch

		[List("Branches")]
		public ZGuid BranchFilter
		{
			get { return fBranchFilter; }
			set
			{
				fBranchFilter = value;
				BranchFilterInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateBranchFilter();
				}
			}
		}

		ZGuid fBranchFilter;

		public ZPropertyInfo BranchFilterInfo
		{
			get { return GetZPropertyInfo(nameof(BranchFilter)); }
		}

		public void ValidateBranchFilter()
		{
			BranchFilterInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(BranchFilterInfo, Branches);
		}

		#endregion

		#region Department

		[List("Departments")]
		public ZGuid DepartmentFilter
		{
			get { return fDepartmentFilter; }
			set
			{
				fDepartmentFilter = value;
				DepartmentFilterInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateDepartmentFilter();
				}
			}
		}

		ZGuid fDepartmentFilter;

		public ZPropertyInfo DepartmentFilterInfo
		{
			get { return GetZPropertyInfo(nameof(DepartmentFilter)); }
		}

		public void ValidateDepartmentFilter()
		{
			DepartmentFilterInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(DepartmentFilterInfo, Departments);
		}

		#endregion

		#region Show Reversed

		public ZBool ShowReversedFilter
		{
			get { return fShowReversedFilter; }
			set
			{
				fShowReversedFilter = value;
				ShowReversedFilterInfo.RefreshBinding();
			}
		}

		ZBool fShowReversedFilter = true;

		public ZPropertyInfo ShowReversedFilterInfo
		{
			get { return GetZPropertyInfo(nameof(ShowReversedFilter)); }
		}

		#endregion

		#region Job Number

		[List("Jobs")]
		public ZGuid JobNumberFilter
		{
			get { return fJobNumberFilter; }
			set
			{
				fJobNumberFilter = value;
				JobNumberFilterInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateJobNumber();
				}
			}
		}

		ZGuid fJobNumberFilter;

		public ZPropertyInfo JobNumberFilterInfo
		{
			get { return GetZPropertyInfo(nameof(JobNumberFilter)); }
		}

		public void ValidateJobNumber()
		{
			JobNumberFilterInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(JobNumberFilterInfo, Jobs);
		}

		#endregion

		#region Company

		public ZGuid CompanyFilter
		{
			get { return fCompanyFilter; }
			set
			{
				fCompanyFilter = value;
				CompanyFilterInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateCompanyFilter();
				}
			}
		}

		ZGuid fCompanyFilter;

		public ZPropertyInfo CompanyFilterInfo
		{
			get { return GetZPropertyInfo(nameof(CompanyFilter)); }
		}

		public void ValidateCompanyFilter()
		{
			CompanyFilterInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(CompanyFilterInfo, Companies);
		}

		#endregion

		#region Recognized Charges

		[MaxLength(3)]
		[List("RecognizedChargesList")]
		public ZString RecognizedChargesFilter
		{
			get { return fRecognizedChargesFilter; }
			set
			{
				SetNonPersistentPropertyValue(RecognizedChargesFilterInfo, ref fRecognizedChargesFilter, value);
				ValidateRecognizedCharges();
			}
		}
		ZString fRecognizedChargesFilter;

		public ZPropertyInfo RecognizedChargesFilterInfo
		{
			get { return GetZPropertyInfo(nameof(RecognizedChargesFilter)); }
		}

		public void ValidateRecognizedCharges()
		{
			if (!IsValidationSuspended)
			{
				RecognizedChargesFilterInfo.ClearAllNotifications();
				CheckMaximumLength(RecognizedChargesFilterInfo, RecognizedChargesFilter);
				ListValidation.ErrorIfInvalidCode(RecognizedChargesFilterInfo, RecognizedChargesList);
			}
		}

		#endregion

		public void ClearFilterValues()
		{
			ChargeCodeFilter = ZString.Empty;
			BranchFilter = ZGuid.Empty;
			DepartmentFilter = ZGuid.Empty;
			ShowReversedFilter = true;
			JobNumberFilter = ZGuid.Empty;
			CompanyFilter = ZGuid.Empty;
			RecognizedChargesFilter = ZString.Empty;

			RefreshBinding();
			ParentProfitLoss.RefreshBinding();
		}

		#endregion

		#region Lookups & Lists

		#region Charge Codes

		public AccChargeCodeCollection ChargeCodes
		{
			get { return FindboxLookupCollections.GetChargeCodeCollection(Factory); }
		}

		#endregion

		#region Branches

		public GlbBranchCollection Branches
		{
			get { return fBranches ?? (fBranches = new GlbBranchCollection(Factory)); }
		}
		GlbBranchCollection fBranches;

		#endregion

		#region Departments

		public GlbDepartmentCollection Departments
		{
			get { return fDepartments ?? (fDepartments = new GlbDepartmentCollection(Factory)); }
		}
		GlbDepartmentCollection fDepartments;

		#endregion

		#region Jobs

		public JobHeaderCollection Jobs => jobs ?? (jobs = new JobHeaderCollection(Factory, new ZQuery(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK)));
		JobHeaderCollection jobs;

		#endregion

		#region Companies

		public GlbCompanyCollection Companies
		{
			get { return fCompanies ?? (fCompanies = new GlbCompanyCollection(Factory)); }
		}
		GlbCompanyCollection fCompanies;

		#endregion

		#region Recognized Charges

		CodeDescriptionPairList fRecognizedChargesList;
		public CodeDescriptionPairList RecognizedChargesList
		{
			get
			{
				if (fRecognizedChargesList == null)
				{
					fRecognizedChargesList = new ChargeRecognitionFilterOptionList();
				}
				return fRecognizedChargesList;
			}
		}

		#endregion

		#endregion
	}
}