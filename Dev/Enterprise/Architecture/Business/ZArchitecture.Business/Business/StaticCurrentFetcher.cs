using System;
using System.ComponentModel;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.ZArchitecture.Business
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public sealed class StaticCurrentFetcher
	{
		StaticCurrentFetcher()
		{
		}

		public static readonly StaticCurrentFetcher Instance = new StaticCurrentFetcher();

		public IGlbCompany CurrentCompany
		{
			get { return currentCompany ?? (IGlbCompany)CurrentCompanyInfo.GetValue(null, null); }
		}
		IGlbCompany currentCompany;

		public IGlbBranch CurrentBranch
		{
			get { return currentBranch ?? (IGlbBranch)CurrentBranchInfo.GetValue(null, null); }
		}
		IGlbBranch currentBranch;

		public IGlbStaff CurrentUser
		{
			get { return currentUser ?? (IGlbStaff)CurrentUserInfo.GetValue(null, null); }
		}
		IGlbStaff currentUser;

		public IGlbDepartment CurrentDepartment
		{
			get { return currentDepartment ?? (IGlbDepartment)CurrentDepartmentInfo.GetValue(null, null); }
		}
		IGlbDepartment currentDepartment;

		public ZString CurrentUserCode
		{
			get
			{
				if (!currentUserCode.IsEmpty)
				{
					return currentUserCode;
				}

				IGlbStaff user = CurrentUser;
				return user == null ? ZString.Empty : user.GS_Code;
			}
		}
		ZString currentUserCode;

		public ZString CurrentBranchCode
		{
			get
			{
				if (!currentBranchCode.IsEmpty)
				{
					return currentBranchCode;
				}
				IGlbBranch branch = CurrentBranch;
				return branch == null ? ZString.Empty : branch.GB_Code;
			}
		}
		ZString currentBranchCode;

		public ZString CurrentDepartmentCode
		{
			get
			{
				if (!currentDepartmentCode.IsEmpty)
				{
					return currentDepartmentCode;
				}
				IGlbDepartment department = CurrentDepartment;
				return department == null ? ZString.Empty : department.GE_Code;
			}
		}
		ZString currentDepartmentCode;

		#region Implementation

		static PropertyInfo CurrentCompanyInfo
		{
			get
			{
				if (fCurrentCompanyInfo == null)
				{
					fCurrentCompanyInfo = ObjectFactory.GetType<IGlbCompany>().GetProperty("CurrentCompany", BindingFlags.Static | BindingFlags.Public);
				}
				return fCurrentCompanyInfo;
			}
		}
		[SuppressThreadStaticFieldMessage]
		static PropertyInfo fCurrentCompanyInfo;

		static PropertyInfo CurrentBranchInfo
		{
			get
			{
				if (fCurrentBranchInfo == null)
				{
					fCurrentBranchInfo = ObjectFactory.GetType<IGlbBranch>().GetProperty("CurrentBranch", BindingFlags.Static | BindingFlags.Public);
				}
				return fCurrentBranchInfo;
			}
		}
		[SuppressThreadStaticFieldMessage]
		static PropertyInfo fCurrentBranchInfo;

		static PropertyInfo CurrentDepartmentInfo
		{
			get
			{
				if (fCurrentDepartmentInfo == null)
				{
					fCurrentDepartmentInfo = ObjectFactory.GetType<IGlbDepartment>().GetProperty("CurrentDepartment", BindingFlags.Static | BindingFlags.Public);
				}
				return fCurrentDepartmentInfo;
			}
		}
		[SuppressThreadStaticFieldMessage]
		static PropertyInfo fCurrentDepartmentInfo;

		static PropertyInfo CurrentUserInfo
		{
			get
			{
				if (fCurrentUserInfo == null)
				{
					fCurrentUserInfo = ObjectFactory.GetType<IGlbStaff>().GetProperty("CurrentUser", BindingFlags.Static | BindingFlags.Public);
				}
				return fCurrentUserInfo;
			}
		}
		[SuppressThreadStaticFieldMessage]
		static PropertyInfo fCurrentUserInfo;

		#endregion

		#region Caching

		public IDisposable GetTemporaryCachingHolder()
		{
			return inCaching ? new EmptyAction() : new CurrentFetcherCachingHolder(this);
		}

		bool inCaching;

		class CurrentFetcherCachingHolder : IDisposable
		{
			public CurrentFetcherCachingHolder(StaticCurrentFetcher currentFetcher)
			{
				this.currentFetcher = currentFetcher;

				currentFetcher.inCaching = true;

				currentFetcher.currentCompany = currentFetcher.CurrentCompany;
				currentFetcher.currentBranch = currentFetcher.CurrentBranch;
				currentFetcher.currentDepartment = currentFetcher.CurrentDepartment;
				currentFetcher.currentUser = currentFetcher.CurrentUser;
				currentFetcher.currentUserCode = currentFetcher.CurrentUserCode;
				currentFetcher.currentBranchCode = currentFetcher.CurrentBranchCode;
				currentFetcher.currentDepartmentCode = currentFetcher.CurrentDepartmentCode;
			}

			readonly StaticCurrentFetcher currentFetcher;

			public void Dispose()
			{
				currentFetcher.inCaching = false;

				currentFetcher.currentCompany = null;
				currentFetcher.currentBranch = null;
				currentFetcher.currentDepartment = null;
				currentFetcher.currentUser = null;
				currentFetcher.currentUserCode = ZString.Empty;
				currentFetcher.currentBranchCode = ZString.Empty;
				currentFetcher.currentDepartmentCode = ZString.Empty;
			}
		}

		class EmptyAction : IDisposable
		{
			public void Dispose() { }
		}

		#endregion
	}
}
