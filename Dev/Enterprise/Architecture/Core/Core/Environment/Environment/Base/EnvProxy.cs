using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.ZArchitecture.Environment
{
	public static class EnvProxy
	{
		static EnvProxy()
		{
		}

		public static IEnvironment Instance
		{
			get { return Env.Instance; }
		}

		static IEnv Env
		{
			get => env ??= ObjectFactory.Get<IEnv>();
		}
		static IEnv env;

		public static string HostedLocation
		{
			get
			{
				return ObjectFactory.Get<IProductRegistration>().Key.HostedLocation;
			}
		}

#if DEBUG
		/// <summary>
		/// Any change to hosted location will reset after each test.
		/// </summary>
		public static void SetHostedLocationForTest(string hostedLocation)
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.HostedLocationForTest = hostedLocation;
		}

		/// <summary>
		/// Any changes reset after each test.
		/// </summary>
		public static void SetIsInternalSystemForTest(bool? value)
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.IsInternalSystemForTest = value;
		}

		/// <summary>
		/// Any changes reset after each test.
		/// </summary>
		public static void SetIsUATSystemForTest(bool? value)
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.EnterpriseCodeForTest = (value ?? false) ? WiseTechGlobalInternalSystemCodes.WUT : string.Empty;
		}

		public static IDisposable SetTemporaryEnvForTest(IEnv tempEnv)
		{
			var savedEnv = env;
			env = tempEnv;

			return new DisposableAction(() => { env = savedEnv; });
		}
#endif

		public static bool IsHostedWithCargowise
		{
			get
			{
				var hostedLocation = HostedLocation;
				return !string.IsNullOrEmpty(hostedLocation)
					&& hostedLocation != Enterprise.Core.Constants.LicenceConstants.NotHostedWithCargoWise;
			}
		}

		public static bool? IsInternalSystem
		{
			get => ObjectFactory.Get<IProductRegistration>().Key.IsInternalSystem;
		}

		public static bool? IsUATSystem
		{
			get => ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalUATSystem();
		}

		public static bool? IsWiseTechGlobalInternalSystem
		{
			get => ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalSystem();
		}

		public static bool IsRDSInstalled
		{
			get
			{
				return RegisteredRemoteMessageTypesCopyForGlobal != null && RegisteredRemoteMessageTypesCopyForGlobal.Length > 0;
			}
		}

		public static Guid GetAnyBranch(BusinessObjectFactory factory)
		{
			var branchFilter = new ZDBOnlyQuery(ObjectFactory.GetType("IGlbBranch"));
			branchFilter.AddToFilter(GlbBranchSchema.GB_IsActive, true);
			branchFilter.OrderBy = GlbBranchSchema.GB_BranchName.Name;

			var companySubQuery = new ZDBOnlySubQuery(ObjectFactory.GetType("IGlbCompany"), GlbBranchSchema.GB_GC);
			companySubQuery.AddToFilter(GlbCompanySchema.GC_IsActive, true);

			branchFilter.AddSubQuery(GlbBranchSchema.GB_GC, GlbCompanySchema.PK, companySubQuery, JoinCondition.And);

			var result = (IBranch)factory.LoadTop1(ObjectFactory.GetType("IGlbBranch"), branchFilter);
			return result == null ? Guid.Empty : result.PK;
		}

		public static Guid GetAnyBranchWithWebAddress(BusinessObjectFactory factory)
		{
			var branchFilterWithWebAddress = new ZDBOnlyQuery(ObjectFactory.GetType("IGlbBranch"));
			branchFilterWithWebAddress.AddToFilter(GlbBranchSchema.GB_IsActive, true);
			branchFilterWithWebAddress.AddToFilter(JoinCondition.And, GlbBranchSchema.GB_WebAddress, SQLComparisonOperator.NotEqual, ZString.Empty);
			branchFilterWithWebAddress.OrderBy = GlbBranchSchema.GB_BranchName.Name;

			var companySubQuery = new ZDBOnlySubQuery(ObjectFactory.GetType("IGlbCompany"), GlbBranchSchema.GB_GC);
			companySubQuery.AddToFilter(GlbCompanySchema.GC_IsActive, true);
			branchFilterWithWebAddress.AddSubQuery(GlbBranchSchema.GB_GC, GlbCompanySchema.PK, companySubQuery, JoinCondition.And);

			var result = (IBranch)factory.LoadTop1(ObjectFactory.GetType("IGlbBranch"), branchFilterWithWebAddress);
			return result?.PK ?? GetAnyBranch(factory);
		}

		public static Guid GetAnyDepartment(BusinessObjectFactory factory)
		{
			var departmentFilter = new ZQuery();
			departmentFilter.AddToFilter(GlbDepartmentSchema.GE_IsActive, true);
			departmentFilter.OrderBy = GlbDepartmentSchema.GE_Desc.Name;
			var result = factory.LoadTop1(ObjectFactory.GetType("IGlbDepartment"), departmentFilter);
			return result == null ? Guid.Empty : result.PK.ToGuid();
		}

		//this property should only be set in InitializationMessageHandler
		public static string[] RegisteredRemoteMessageTypesCopyForGlobal { get; set; }
	}
}
