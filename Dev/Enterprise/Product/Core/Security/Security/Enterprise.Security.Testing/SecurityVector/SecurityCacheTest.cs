using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Security.Testing
{
	sealed class SecurityCacheTest : TestCaseWithFactory
	{
		public void TestCache()
		{
			var group1 = Factory.New<GlbGroup>();
			var group2 = Factory.New<GlbGroup>();
			var group3 = Factory.New<GlbGroup>();
			var groupInactive = Factory.New<GlbGroup>();
			groupInactive.GG_IsActive = false;

			var staffA = Factory.New<GlbStaff>();
			staffA.Groups.AddRange(group1, group2, groupInactive);
			var staffB = Factory.New<GlbStaff>();
			staffB.Groups.AddRange(group2, group3);
			var staffC = Factory.New<GlbStaff>();

			var securities = new GlbSecurityCollection(Factory);
			var security1 = CreateSecurities(securities, Env.Security.Operations, staffA);
			var security2 = CreateSecurities(securities, Env.Security.Forwarding, staffA, staffB);
			var security3 = CreateSecurities(securities, Env.Security.LinerAndAgency, staffA, group1, groupInactive);
			var security4 = CreateSecurities(securities, Env.Security.ForwardingContainer, staffA, group1, group2, groupInactive);
			var security5 = CreateSecurities(securities, Env.Security.ForwardingContainerEdit, staffA, group1, group2, group3);
			ZGuid itemGuid = ZGuid.NewZGuid();
			var checkpointWithGuid = new SecurityCheckpoint("", (NoResString)"", null, null, false, "AU", itemGuid.ToGuid());
			var security6 = CreateSecurities(securities, checkpointWithGuid, staffC, group1, group2, group3);
			var cache = new SecurityCache(securities, Env.Security);

			AssertCacheEntry(cache[Env.Security.SystemRegistry, staffA]);
			AssertCacheEntry(cache[Env.Security.SystemRegistry, staffB]);
			AssertCacheEntry(cache[Env.Security.SystemRegistry, staffC]);

			AssertCacheEntry(cache[Env.Security.Operations, staffA], security1[0]);
			AssertCacheEntry(cache[Env.Security.Operations, staffB]);
			AssertCacheEntry(cache[Env.Security.Operations, staffC]);

			AssertCacheEntry(cache[Env.Security.Forwarding, staffA], security2[0]);
			AssertCacheEntry(cache[Env.Security.Forwarding, staffB], security2[1]);
			AssertCacheEntry(cache[Env.Security.Forwarding, staffC]);

			AssertCacheEntry(cache[Env.Security.LinerAndAgency, staffA], security3[0], security3[1]);
			AssertCacheEntry(cache[Env.Security.LinerAndAgency, staffB]);
			AssertCacheEntry(cache[Env.Security.LinerAndAgency, staffC]);

			AssertCacheEntry(cache[Env.Security.ForwardingContainer, staffA], security4[0], security4[1], security4[2]);
			AssertCacheEntry(cache[Env.Security.ForwardingContainer, staffB], security4[2]);
			AssertCacheEntry(cache[Env.Security.ForwardingContainer, staffC]);

			AssertCacheEntry(cache[Env.Security.ForwardingContainerEdit, staffA], security5[0], security5[1], security5[2]);
			AssertCacheEntry(cache[Env.Security.ForwardingContainerEdit, staffB], security5[2], security5[3]);
			AssertCacheEntry(cache[Env.Security.ForwardingContainerEdit, staffC]);

			AssertCacheEntry(cache[checkpointWithGuid, staffA], security6[1], security6[2]);
			AssertCacheEntry(cache[checkpointWithGuid, staffB], security6[2], security6[3]);
			AssertCacheEntry(cache[checkpointWithGuid, staffC], security6[0]);
		}

		void AssertCacheEntry(IEnumerable<IGlbSecurity> securities, params IGlbSecurity[] expectedSecurity)
		{
			if (expectedSecurity.Length > 0)
			{
				AssertEquals(expectedSecurity.Length, securities.Count());
				int i = 0;
				foreach (var security in securities)
				{
					AssertEquals(expectedSecurity[i++], security);
				}
			}
			else
			{
				AssertNull(securities);
			}
		}

		GlbSecurity[] CreateSecurities(GlbSecurityCollection securities, ISecurityCheckpoint checkpoint, params BusinessObject[] targets)
		{
			SecurityTestHelper helper = new SecurityTestHelper(Factory);
			GlbSecurity[] result = Array.ConvertAll(targets, target => helper.CreateSecurity(checkpoint, target, null, null, null, true));
			Array.ForEach(result, security => securities.Add(security));

			return result;
		}
	}
}
