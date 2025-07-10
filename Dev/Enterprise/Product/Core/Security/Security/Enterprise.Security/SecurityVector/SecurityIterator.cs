using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using WTG.SecurityRights;

namespace Enterprise.Security
{
	public class SecurityIterator<T>
	{
		public SecurityIterator(BusinessObjectFactory factory, ISecuritySummaryGenerator<T> generator, bool treatLocalAdministratorCheckPointDifferently = true)
		{
			this.factory = factory;
			this.generator = generator;
			this.treatLocalAdministratorCheckPointDifferently = treatLocalAdministratorCheckPointDifferently;

			factory.Load<GlbCompany>(new ZQuery());
			factory.Load<GlbBranch>(new ZQuery());
			factory.Load<GlbDepartment>(new ZQuery());
		}

		public void SetupNoInitialize(IEnumerable<GlbStaff> staffs, ZArchitecture.Modules.CheckpointLookupKey startFrom = default)
		{
			Staffs = staffs.ToArray();
			StartFrom = startFrom;
		}

		public void Setup(GlbStaff staff, ZArchitecture.Modules.CheckpointLookupKey startFrom = default)
		{
			Staffs = new GlbStaff[] { staff };
			InitializeSecurityCollection(startFrom, Staffs, staff.ActiveGroups.Cast<GlbGroup>().ToArray());
		}

		public void Setup(IEnumerable<GlbStaff> staffs, ZArchitecture.Modules.CheckpointLookupKey startFrom = default)
		{
			Staffs = staffs.ToArray();
			InitializeSecurityCollection(startFrom, staffs, staffs.SelectMany(staff => staff.ActiveGroups.Cast<GlbGroup>()).ToArray());
		}

		public void Setup(IEnumerable<GlbGroup> groups, ZArchitecture.Modules.CheckpointLookupKey startFrom = default)
		{
			securities = new GlbSecurityCollection(factory);

			if (groups != null && groups.Any())
			{
				foreach (var group in groups)
				{
					factory.ImportFromAnotherFactory(group, typeof(GlbGroup));
				}

				var glbGroupLinks = factory.Load<GlbGroupLink>(new ZQuery().AddToFilter(JoinCondition.And, GlbGroupLinkSchema.GK_GG, SQLComparisonOperator.Equal,
					groups.Select(x => x.PK)));

				var staffsQuery = new ZDBOnlyQuery(typeof(GlbStaff));

				foreach (var group in groups)
				{
					staffsQuery.AddSubQuery(GlbStaffSchema.PK,
						(ZDBOnlySubQuery)new ZDBOnlySubQuery(typeof(GlbGroupLink), GlbGroupLinkSchema.GK_GS).AddToFilter(GlbGroupLinkSchema.GK_GG, group.PK),
						JoinCondition.And);
				}

				staffsQuery.AddToFilter(JoinCondition.And, GlbStaffSchema.GS_IsSystemAccount, ZBool.False);
				staffsQuery.AddToFilter(JoinCondition.And, GlbStaffSchema.GS_IsActive, ZBool.True);
				Staffs = factory.Load<GlbStaff>(staffsQuery).OrderBy(x => x.GS_FullName).ToArray();

				var relatedGroups = new HashSet<GlbGroup>(groups);
				foreach (var staff in Staffs)
				{
					foreach (GlbGroup group in staff.ActiveGroups)
					{
						if (!relatedGroups.Any(g => g.PK == group.PK))
						{
							relatedGroups.Add(group);
						}
					}
				}

				InitializeSecurityCollection(startFrom, Staffs, relatedGroups);
			}
			else
			{
				factory.Load<GlbGroup>(new ZQuery());
				Staffs = factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.GS_IsSystemAccount, ZBool.False)
					.AddToFilter(JoinCondition.And, GlbStaffSchema.GS_IsActive, ZBool.True)
					).OrderBy(x => x.GS_FullName).ToArray();
				factory.Load<GlbGroupLink>(new ZQuery());

				InitializeSecurityCollection(startFrom);
			}
		}

		void InitializeSecurityCollection(ZArchitecture.Modules.CheckpointLookupKey startFrom, IEnumerable<GlbStaff> staffs, IEnumerable<GlbGroup> groups)
		{
			StartFrom = startFrom;
			var checkpoint = !string.IsNullOrEmpty(startFrom.Code) ? Env.Security.FindCheckPoint(startFrom) : null;
			var localSecurities = new GlbSecurityCollection(factory);
			if (checkpoint != null)
			{
				var securityList = ParentsAndChildrenOfSecurityCheckpoint(checkpoint);
				if (securityList.Count > 0)
				{
					var query = new ZQuery();
					var securityHalf = new ZQuery().AddToFilter(JoinCondition.And, GlbSecuritySchema.GU_SecurityRight, SQLComparisonOperator.Equal,
					securityList);
					var staffGroupHalf = new ZQuery().AddToFilter(JoinCondition.Or, GlbSecuritySchema.GU_GS, SQLComparisonOperator.Equal,
					staffs.Select(x => x.PK))
					.AddToFilter(JoinCondition.Or, GlbSecuritySchema.GU_GG, SQLComparisonOperator.Equal,
					groups.Select(x => x.PK));

					query.AddToFilter(securityHalf, JoinCondition.And);
					query.AddToFilter(staffGroupHalf, JoinCondition.And);

					localSecurities.Load(query);
					securities = localSecurities;
					return;
				}
			}

			localSecurities.Load(new ZQuery().AddToFilter(JoinCondition.Or, GlbSecuritySchema.GU_GS, SQLComparisonOperator.Equal,
				staffs.Select(x => x.PK))
				.AddToFilter(JoinCondition.Or, GlbSecuritySchema.GU_GG, SQLComparisonOperator.Equal,
				groups.Select(x => x.PK)));
			securities = localSecurities;
		}

		void InitializeSecurityCollection(ZArchitecture.Modules.CheckpointLookupKey startFrom)
		{
			StartFrom = startFrom;
			var checkpoint = !string.IsNullOrEmpty(startFrom.Code) ? Env.Security.FindCheckPoint(startFrom) : null;
			var localSecurities = new GlbSecurityCollection(factory);

			if (checkpoint != null)
			{
				var securityList = ParentsAndChildrenOfSecurityCheckpoint(checkpoint);
				if (securityList.Count > 0)
				{
					localSecurities.Load(new ZQuery().AddToFilter(JoinCondition.And, GlbSecuritySchema.GU_SecurityRight, SQLComparisonOperator.Equal,
					securityList));
					securities = localSecurities;
					return;
				}
			}

			localSecurities.Load(new ZQuery());
			securities = localSecurities;
		}

		const int MaxSecurityRights = 64; //arbitrary cutoff point so we don't try to filter on 1000s+ of rights. but haven't profiled to figure out where the cutoff ought to be

		static List<string> ParentsAndChildrenOfSecurityCheckpoint(ISecurityCheckpoint start)
		{
			var result = new List<string>();
			result.Add(start.Code);
			var parent = start.Parent;

			while (parent != null)
			{
				result.Add(parent.Code);
				parent = parent.Parent;
			}

			ChildrenOfSecurityCheckpoint(start, result);

			if (result.Count > MaxSecurityRights)
			{
				return new List<string>();
			}
			return result;
		}

		static List<string> ChildrenOfSecurityCheckpoint(ISecurityCheckpoint start, List<string> result)
		{
			foreach (var child in start.ChildCheckPoints)
			{
				if (result.Count > MaxSecurityRights)
				{
					break;
				}

				result.Add(child.Code);

				ChildrenOfSecurityCheckpoint(child, result);
			}
			return result;
		}

		public IEnumerable<ISecuritySummary<T>> GetSummaries()
		{
			if (securities == null)
			{
				throw new InvalidOperationException("Setup securities before calling GetSummaries(). (InitializeSecurityCollection or pass one in)");
			}

			var securityCore = new SecurityCore(securities, Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);

			// Initialize all of the lazy properties.
			foreach (var info in typeof(SecurityCore).GetProperties())
			{
				if (typeof(SecurityCheckpoint).IsAssignableFrom(info.PropertyType))
				{
					info.GetValue(securityCore);
				}
			}

			return GetSummaries(securities, securityCore);
		}

#if DEBUG
		[ThreadStatic]
		public static GlbSecurityCollection LastSecurityCollection;
#endif

		public IEnumerable<ISecuritySummary<T>> GetSummaries(GlbSecurityCollection securityCollection, SecurityCore securityCore)
		{
			securities = securityCollection;
#if DEBUG
			if (ZArchitecture.Environment.Globals.IsTest)
			{
				LastSecurityCollection = securityCollection;
			}
#endif

			securityCore.LoadRegistrySecurityCheckPoints();

			var vector = new SecurityVector();
			vector.Initialise(securityCore);
			var securitiesCache = new SecurityCache(securities, securityCore);

			ISecurityInfo startFromInfo = null;
			if (!StartFrom.IsEmpty)
			{
				startFromInfo = vector.FirstOrDefault(info => info.Checkpoint.LookupKey.Equals(StartFrom));
			}

			var stack = new Stack<SecurityIteratorNodeContext<T>[]>();
			return EnumerateByCheckpointAndStaff(securities, securitiesCache, stack, vector.Nodes, startFromInfo, false);
		}

		IEnumerable<ISecuritySummary<T>> EnumerateByCheckpointAndStaff(GlbSecurityCollection securityCollection, SecurityCache securitiesCache, Stack<SecurityIteratorNodeContext<T>[]> stack, IEnumerable<ISecurityInfo> securityInfos, ISecurityInfo startFromInfo, bool parentUsingBranchOrDepartmentSpecificRights)
		{
			if (SecurityCalculator == null)
			{
				SecurityCalculator = new SecurityCalculatorForStaff(factory, securityCollection, Staffs);
			}

			foreach (var securityInfo in securityInfos)
			{
				var islocalAdministratorInfo = securityInfo.Checkpoint.LookupKey.Equals(securitiesCache.LocalAdministratorLookupKey);
				var bYield = true;
				if (startFromInfo != null)
				{
					var isParentOfStartFrom = startFromInfo.IsChildOf(securityInfo);
					var isStartFrom = securityInfo.Equals(startFromInfo);
					var isChildOfStartFrom = securityInfo.IsChildOf(startFromInfo);
					if (!isParentOfStartFrom && !isStartFrom && !isChildOfStartFrom)
					{
						continue;
					}

					bYield = isStartFrom || isChildOfStartFrom;
				}

				var currentUsingBranchOrDepartmentSpecificRights = false;

				var ctx = new SecurityIteratorNodeContext<T>[Staffs.Length];
				for (int i = 0; i < Staffs.Length; i++)
				{
					var staff = Staffs[i];

					if (islocalAdministratorInfo && treatLocalAdministratorCheckPointDifferently)
					{
						if (bYield)
						{
							var localAdminSecurities = securitiesCache[securitiesCache.LocalAdministratorLookupKey, staff];
							yield return new SecuritySummary<T>(securityInfo.Checkpoint, staff, generator.GenerateLocalAdministratorSummary(staff, localAdminSecurities), true);
						}
						continue;
					}

					ISecuritySummary<T> securitySummary;

					var prevCtx = stack.Count > 0
						? stack.Peek()[i]
						: new SecurityIteratorNodeContext<T>(null, staff.ActiveGroups.Select(group => new InitialGroupSecurity(group.PK, securityInfo.Checkpoint.Code, true)).Cast<IGlbSecurity>()) { Summary = staff.ActiveGroups.Count > 0 ? generator.CalculateInitialValue(staff) : generator.DeniedValue };

					var nodeSecurities = securitiesCache[securityInfo.Checkpoint, staff];
					ctx[i] = new SecurityIteratorNodeContext<T>(prevCtx, nodeSecurities);

					if (!staff.GS_IsOperational)
					{
						securitySummary = new SecuritySummary<T>(securityInfo.Checkpoint, staff, securityInfo.Checkpoint is SecurityCheckpointNonOperationalAllowed ?
							generator.GrantedValue : generator.DeniedValue, false);
					}
					else if (nodeSecurities != null)
					{
						currentUsingBranchOrDepartmentSpecificRights = nodeSecurities.Any(x => x.GU_GC != ZGuid.Empty || x.GU_GB != ZGuid.Empty || x.GU_GE != ZGuid.Empty);
						securitySummary = new SecuritySummary<T>(securityInfo.Checkpoint, staff, generator.GenerateSummary(staff, DepartmentsGuids, BranchesGuids, Departments, Branches,
						ctx[i].Securities, SecurityCalculator, (SecurityCheckpoint)securityInfo.Checkpoint, startFromInfo != null, parentUsingBranchOrDepartmentSpecificRights || currentUsingBranchOrDepartmentSpecificRights), true);
					}
					else
					{
						securitySummary = new SecuritySummary<T>(securityInfo.Checkpoint, staff, prevCtx != null ? prevCtx.Summary : generator.DeniedValue, false);
					}

					ctx[i].Summary = securitySummary.Summary;

					if (bYield)
					{
						yield return securitySummary;
					}
				}

				stack.Push(ctx);
				foreach (var securitySummary in EnumerateByCheckpointAndStaff(securityCollection, securitiesCache, stack, securityInfo.Nodes, startFromInfo, startFromInfo != null || parentUsingBranchOrDepartmentSpecificRights || currentUsingBranchOrDepartmentSpecificRights))
				{
					yield return securitySummary;
				}
				stack.Pop();
			}
		}

		internal SecurityCalculatorForStaff SecurityCalculator { get; set; }

		ZArchitecture.Modules.CheckpointLookupKey StartFrom { get; set; }

		GlbStaff[] Staffs;

		GlbSecurityCollection securities;

		internal GlbDepartment[] DepartmentBizOs
		{
			get
			{
				if (departmentBizOs == null)
				{
					var filter = new ZQuery(GlbDepartmentSchema.GE_IsActive, ZBool.True);
					departmentBizOs = factory.Load<GlbDepartment>(filter);
					Array.Sort(departmentBizOs, (x, y) => string.Compare(x.GE_Code, y.GE_Code, StringComparison.Ordinal));
				}
				return departmentBizOs;
			}
		}
		GlbDepartment[] departmentBizOs;

		public ZGuid[] DepartmentsGuids
		{
			get
			{
				if (departmentsGuids == null)
				{
					departmentsGuids = Array.ConvertAll(DepartmentBizOs, department => department.PK);
					Array.Resize(ref departmentsGuids, departmentsGuids.Length + 1);
					for (int i = departmentsGuids.Length - 2; i >= 0; --i)
					{
						departmentsGuids[i + 1] = departmentsGuids[i];
					}
					departmentsGuids[0] = ZGuid.Empty;
				}

				return departmentsGuids;
			}
		}
		ZGuid[] departmentsGuids;

		public string[] Departments
		{
			get
			{
				if (departments == null)
				{
					departments = Array.ConvertAll(DepartmentBizOs, department => (string)department.GE_Code);
					Array.Resize(ref departments, departments.Length + 1);
					for (int i = departments.Length - 2; i >= 0; --i)
					{
						departments[i + 1] = departments[i];
					}
					departments[0] = string.Empty;
				}

				return departments;
			}
		}
		string[] departments;

		internal GlbBranch[] BranchBizOs
		{
			get
			{
				if (branchBizOs == null)
				{
					var companiesFilter = new ZQuery(GlbCompanySchema.GC_IsActive, ZBool.True);
					var activeCompanies = Array.ConvertAll(factory.Load<GlbCompany>(companiesFilter), company => company.PK);
					var filter = new ZQuery(GlbBranchSchema.GB_IsActive, ZBool.True);
					filter.AddToFilter(GlbBranchSchema.GB_GC, activeCompanies);
					branchBizOs = factory.Load<GlbBranch>(filter);
					Array.Sort(branchBizOs, (x, y) => string.Compare(x.GB_Code, y.GB_Code, StringComparison.Ordinal));
				}
				return branchBizOs;
			}
		}
		GlbBranch[] branchBizOs;

		public ZGuid[] BranchesGuids
		{
			get
			{
				if (branchesGuids == null)
				{
					branchesGuids = Array.ConvertAll(BranchBizOs, branch => branch.PK);
					Array.Resize(ref branchesGuids, branchesGuids.Length + 1);
					for (int i = branchesGuids.Length - 2; i >= 0; --i)
					{
						branchesGuids[i + 1] = branchesGuids[i];
					}
					branchesGuids[0] = ZGuid.Empty;
				}

				return branchesGuids;
			}
		}
		ZGuid[] branchesGuids;

		public string[] Branches
		{
			get
			{
				if (branches == null)
				{
					branches = Array.ConvertAll(BranchBizOs, branch => (string)branch.GB_Code);
					Array.Resize(ref branches, branches.Length + 1);
					for (int i = branches.Length - 2; i >= 0; --i)
					{
						branches[i + 1] = branches[i];
					}
					branches[0] = string.Empty;
				}

				return branches;
			}
		}
		string[] branches;

		readonly BusinessObjectFactory factory;
		readonly ISecuritySummaryGenerator<T> generator;
		readonly bool treatLocalAdministratorCheckPointDifferently;

		#region InitialGroupSecurity

		class InitialGroupSecurity : IGlbSecurity
		{
			public InitialGroupSecurity(ZGuid groupPK, ZString securityRight, ZBool isAllowed)
			{
				GU_GG = groupPK;
				GU_SecurityRight = securityRight;
				GU_SecurityItemIsAllowed = isAllowed;
			}

			public ZBool GU_SecurityItemIsAllowed { get; set; }
			public ZString GU_SecurityRight { get; set; }
			public ZGuid GU_GG { get; set; }
			public ZGuid GU_GS { get { return ZGuid.Empty; } set { } }
			public ZGuid GU_GB { get { return ZGuid.Empty; } set { } }
			public ZGuid GU_GE { get { return ZGuid.Empty; } set { } }
			public ZGuid GU_GC { get { return ZGuid.Empty; } set { } }
		}

		#endregion
	}

	#region SecurityCalculatorForStaff

	//Adapted from CargoWise.Glow.Service.SecurityRights/SecurityCalculatorForStaff.cs

	public class SecurityCalculatorForStaff
	{
		readonly WTG.SecurityRights.SecurityCalculator innerCalculator;

		public SecurityCalculatorForStaff(BusinessObjectFactory factory, GlbSecurityCollection securityCollection, IEnumerable<GlbStaff> staffs)
		{
			this.securityCollection = securityCollection;
			this.staffs = staffs;
			this.factory = factory;
			innerCalculator = GetInnerCalculator();
		}

		readonly GlbSecurityCollection securityCollection;
		readonly BusinessObjectFactory factory;
		readonly IEnumerable<GlbStaff> staffs;

		#region IsRightAllowed

		public bool IsRightAllowed(ICheckpoint right, Guid staffPK, Guid branchPK, Guid departmentPK, Guid companyPK)
		{
			if (right == null)
			{
				return false;
			}

			var result = innerCalculator.IsAllowedForStaff(right, staffPK, branchPK, departmentPK, companyPK);
			if (result == SecurityState.Implicit)
			{
				result = innerCalculator.IsAllowedForStaffGroups(right, staffPK, branchPK, departmentPK, companyPK);
			}

			return result != SecurityState.Denied;
		}

		public List<string> RightGrantedGroups(ICheckpoint right, Guid staffPK, Guid branchPK, Guid departmentPK, Guid companyPK)
		{
			var grantedSource = new List<string>();
			if (right == null)
			{
				return grantedSource;
			}

			var result = innerCalculator.IsAllowedForStaff(right, staffPK, branchPK, departmentPK, companyPK);
			if (result == SecurityState.Granted)
			{
				grantedSource.Add("*StaffProfile");
			}
			if (result != SecurityState.Denied)
			{
				var grantedGroupPKs = innerCalculator.IsAllowedForStaffGroupsWithGroupsReturned(right, staffPK, branchPK, departmentPK, companyPK);
				grantedSource.AddRange(grantedGroupPKs.Select(pk => factory.Load<GlbGroup>(pk).GG_Code.ToString()));
			}
			return grantedSource;
		}

		#endregion

		#region GetInnerCalculator

		WTG.SecurityRights.SecurityCalculator GetInnerCalculator()
		{
			return new WTG.SecurityRights.SecurityCalculator(
				GetStaffDefaultSecurityLookup(),
				GetStaffGroupPKsLookup(),
				GetCheckpointSecurityLookup());
		}

		IReadOnlyDictionary<Guid, SecurityState> GetStaffDefaultSecurityLookup()
		{
			var staffDefaultSecurityLookup = new Dictionary<Guid, SecurityState>();

			foreach (var staff in staffs)
			{
				staffDefaultSecurityLookup.Add(
					staff.PK.ToGuid(),
					(staff.GS_IsController || !staff.GS_IsOperational || staff.GS_IsSystemAccount)
						? SecurityState.Granted
						: SecurityState.Implicit);
			}

			return staffDefaultSecurityLookup;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		IReadOnlyDictionary<Guid, IEnumerable<Guid>> GetStaffGroupPKsLookup()
		{
			if (staffGroupPKsLookup == null)
			{
				staffGroupPKsLookup = new Dictionary<Guid, IEnumerable<Guid>>();
				if (staffs.Any())
				{
					var list = new List<(ZGuid GK_GS, ZGuid GK_GG)>();
					var staffsNotInDatabase = staffs.Where(x => !x.IsInDatabase);
					foreach (var staff in staffsNotInDatabase)
					{
						staffGroupPKsLookup.Add(staff.PK.ToGuid(), staff.ActiveGroups.Select(x => x.PK.ToGuid()));
					}
					var staffsInDatabase = staffs.Where(x => x.IsInDatabase).ToArray();
					var staffPKsInDatabase = staffsInDatabase.Select(x => x.PK.ToSqlGuid()).ToArray();
					if (staffPKsInDatabase.Length > 0)
					{
						var staffPKsString = string.Join(", ", staffPKsInDatabase);
						var collection = new DynamicBusinessObjectCollection(factory);
						collection.Load(System.FormattableString.Invariant($@"SELECT {GlbGroupLinkSchema.Constants.GK_GS}, {GlbGroupLinkSchema.Constants.GK_GG}
FROM {GlbGroupLinkSchema.Constants.SqlSchemaName}.{GlbGroupLinkSchema.Constants.TableName}
WHERE {GlbGroupLinkSchema.Constants.GK_GS} IN ({staffPKsString})
AND {GlbGroupLinkSchema.Constants.GK_GG} IN (SELECT {GlbGroupSchema.Constants.PK} FROM {GlbGroupSchema.Constants.SqlSchemaName}.{GlbGroupSchema.Constants.TableName} WHERE {GlbGroupSchema.Constants.GG_IsActive} = 1 AND {GlbGroupSchema.Constants.GG_IsSales} = 0)"));
						list.AddRange(collection.Cast<DynamicBusinessObject>().Select(x => ((ZGuid)x[GlbGroupLinkSchema.GK_GS], (ZGuid)x[GlbGroupLinkSchema.GK_GG])));
					}
					var existingGroupLinks = factory.Load<GlbGroupLink>(new ZQuery(GlbGroupLinkSchema.GK_GS, staffsInDatabase.Select(x => x.PK)) { FetchOnlyFromLocalCache = true });
					if (existingGroupLinks.Length > 0)
					{
						foreach (var groupPK in existingGroupLinks.Select(x => x.GK_GG).Distinct())
						{
							factory.AddFetchHint(GlbGroupSchema.PK, groupPK);
						}
						list.AddRange(existingGroupLinks.Where(x => (x.Group?.GG_IsActive ?? false) && (!x.Group.GG_IsSales)).Select(x => (x.GK_GS, x.GK_GG)));
					}
					foreach (var data in list.GroupBy(x => x.GK_GS))
					{
						staffGroupPKsLookup.Add(data.Key.ToGuid(), data.Select(y => y.GK_GG).Distinct().Select(x => x.ToGuid()));
					}
				}
			}
			return staffGroupPKsLookup;
		}
		Dictionary<Guid, IEnumerable<Guid>> staffGroupPKsLookup;

		IReadOnlyDictionary<WTG.SecurityRights.CheckpointLookupKey, IReadOnlyDictionary<SecurityLookupKey, SecurityState>> GetCheckpointSecurityLookup()
		{
			var checkpointSecurityLookup = new Dictionary<WTG.SecurityRights.CheckpointLookupKey, IReadOnlyDictionary<SecurityLookupKey, SecurityState>>();

			var lastCheckpointLookupKey = new WTG.SecurityRights.CheckpointLookupKey();
			Dictionary<SecurityLookupKey, SecurityState> dict = null;
			var securities = securityCollection.Cast<GlbSecurity>().OrderBy(x => x.GU_SecurityRight).ThenBy(x => x.GU_ItemGUID);
			foreach (var security in securities)
			{
				var checkpointLookupKey = new WTG.SecurityRights.CheckpointLookupKey(security.GU_SecurityRight, security.GU_ItemGUID.IsEmpty ? Guid.Empty : security.GU_ItemGUID.ToGuid());
				if (lastCheckpointLookupKey != checkpointLookupKey)
				{
					if (dict != null)
					{
						checkpointSecurityLookup.Add(lastCheckpointLookupKey, dict);
					}

					lastCheckpointLookupKey = checkpointLookupKey;
					dict = new Dictionary<SecurityLookupKey, SecurityState>();
				}

				dict.Add(
					new SecurityLookupKey(
						security.GU_GS.IsEmpty ? Guid.Empty : security.GU_GS.ToGuid(),
						security.GU_GG.IsEmpty ? Guid.Empty : security.GU_GG.ToGuid(),
						security.GU_GB.IsEmpty ? Guid.Empty : security.GU_GB.ToGuid(),
						security.GU_GE.IsEmpty ? Guid.Empty : security.GU_GE.ToGuid(),
						security.GU_GC.IsEmpty ? Guid.Empty : security.GU_GC.ToGuid()),
					security.GU_SecurityItemIsAllowed ? SecurityState.Granted : SecurityState.Denied);
			}

			if (dict != null)
			{
				checkpointSecurityLookup.Add(lastCheckpointLookupKey, dict);
			}

			return checkpointSecurityLookup;
		}

		#endregion
	}

	#endregion
}
