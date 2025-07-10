using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Core.Environment;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Business.Test
{
	public static class BMSecurityTestHelper
	{
		public static SecurityCore GetSecurityInstance(BusinessObjectFactory factory)
		{
			var staff = factory.Load<GlbStaff>(Env.CurrentUser.PK);
			staff.GS_IsController = false;

			var securityCollection = new GlbSecurityCollection(factory);
			securityCollection.Load();
			var collection = new GlbSecurityCollection(factory);
			collection.Load();
			collection.RemoveAndDeleteAll();

			return CreateSecurity(securityCollection, staff);
		}

		public static void AllowPermissionsForCheckpoint(ISecurityCheckpoint checkpoint)
		{
			foreach (var checkPoint in checkpoint.GetAllChildren())
			{
				checkPoint.IsAllowed = true;
			}
		}

		public static void DenyPermissionsForCheckpoint(ISecurityCheckpoint checkpoint)
		{
			foreach (var checkPoint in checkpoint.GetAllChildren())
			{
				checkPoint.IsAllowed = false;
			}
		}

		public static IEnumerable<ISecurityCheckpoint> GetAllChildren(this ISecurityCheckpoint checkpoint)
		{
			return checkpoint.SelectRecursive(c => c.ChildCheckPoints);
		}

		public static SecurityCore CreateSecurity(IZGlbSecurityCollection securityCollection, object staffOrGroup, bool cachingEnabled = false)
		{
			return CreateSecurity(securityCollection, staffOrGroup, Guid.Empty, Guid.Empty, Guid.Empty, cachingEnabled);
		}

		public static SecurityCore CreateSecurity(IZGlbSecurityCollection securityCollection, object staffOrGroup, Guid branchPK, Guid departmentPK, Guid companyPK, bool cachingEnabled = false)
		{
			return new SecurityCore(securityCollection, staffOrGroup, branchPK, departmentPK, companyPK)
			{
				CachingEnabled = cachingEnabled
			};
		}

		public static GlbGroup CreateGroup(BusinessObjectFactory factory, string code)
		{
			var group = factory.New<GlbGroup>();
			group.GG_Code = code;
			return group;
		}
	}
}
