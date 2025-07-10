using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.GLJournals
{
	public abstract class GLJournalEnvironmentForADAW : NonPersistentBusinessObject, IJournalCompanyAndBranchForImport
	{
		public GLJournalEnvironmentForADAW(BusinessObjectFactory factory) : base(factory)
		{
		}

		[BusinessObjectTestExclude]
		public ZString CompanyCode { get; set; }
		public ZPropertyInfo CompanyCodeInfo
		{
			get { return GetZPropertyInfo(nameof(CompanyCode)); }
		}

		public GlbCompany Company
		{
			get
			{
				if (company == null && !CompanyCode.IsEmpty)
				{
					company = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, CompanyCode);
				}

				return company;
			}
		}

		GlbCompany company;

		[BusinessObjectTestExclude]
		public ZString BranchCode { get; set; }
		public ZPropertyInfo BranchCodeInfo
		{
			get { return GetZPropertyInfo(nameof(BranchCode)); }
		}

		public GlbBranch Branch
		{
			get
			{
				if (branch == null && !BranchCode.IsEmpty)
				{
					branch = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, BranchCode);
				}

				return branch;
			}
		}

		GlbBranch branch;

		public ZInt Sequence { get; set; }

		#region Validation

		public string ValidateCompany()
		{
			if (!CompanyCode.IsEmpty && !(Company?.GC_IsActive ?? false))
			{
				return Res.GetString("01C5451E-8BC1-4CB6-84AD-95FAAE79A713", "The company code {0} is invalid or inactive.", CompanyCode);
			}
			else
			{
				return string.Empty;
			}
		}

		public string ValidateBranch()
		{
			if (!BranchCode.IsEmpty && !(Branch?.GB_IsActive ?? false))
			{
				return Res.GetString("2FB15199-F46F-4B99-BC88-4E2E31DEFA8A", "The branch code {0} is invalid or inactive.", BranchCode);
			}
			else
			{
				return string.Empty;
			}
		}

		public string ValidateCompanyMatchBranch()
		{
			if (Company != null && Branch != null && Company.GC_Code != Branch.Company.GC_Code)
			{
				return Res.GetString("b349a928-ac27-470c-b829-d5181e929ca8", "The branch code {0} is not valid in company {1}.", BranchCode, CompanyCode);
			}
			else
			{
				return string.Empty;
			}
		}

		#endregion
	}

	interface IJournalCompanyAndBranchForImport
	{
		ZString CompanyCode { get; set; }
		GlbCompany Company { get; }
		ZString BranchCode { get; set; }
		GlbBranch Branch { get; }
	}
}
