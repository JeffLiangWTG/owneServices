using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Security.Testing
{
	sealed class SecurityIteratorNodeContextTest : TestCaseWithFactory
	{
		public void TestSecurities()
		{
			SecurityTestHelper helper = new SecurityTestHelper(Factory);

			var group1 = Factory.New<GlbGroup>();
			var group2 = Factory.New<GlbGroup>();
			var staffA = Factory.New<GlbStaff>();
			staffA.Groups.AddRange(group1, group2);

			GlbSecurity[] security1 = { helper.CreateSecurity(Env.Security.Operations, group1, null, null, null, false) };
			var ctx1 = new SecurityIteratorNodeContext<int>(null, security1);

			GlbSecurity[] security11 =
			{
				helper.CreateSecurity(Env.Security.Warehouse, staffA, GlbCompany.CurrentCompany, null, null, true),
				helper.CreateSecurity(Env.Security.Warehouse, staffA, null, GlbBranch.CurrentBranch, null, true),
				helper.CreateSecurity(Env.Security.Warehouse, staffA, null, null, GlbDepartment.CurrentDepartment, true),
				helper.CreateSecurity(Env.Security.Warehouse, staffA, GlbCompany.CurrentCompany, null, GlbDepartment.CurrentDepartment, true),
				helper.CreateSecurity(Env.Security.Warehouse, staffA, null, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment, true),
			};
			var ctx11 = new SecurityIteratorNodeContext<int>(ctx1, security11);

			GlbSecurity[] security111 =
			{
				helper.CreateSecurity(Env.Security.WhsAdjustment, group2, GlbCompany.CurrentCompany, null, null, true),
				helper.CreateSecurity(Env.Security.WhsAdjustment, staffA, null, GlbBranch.CurrentBranch, null, true),
			};
			var ctx111 = new SecurityIteratorNodeContext<int>(ctx11, security111);

			GlbSecurity[] security2 =
			{
				helper.CreateSecurity(Env.Security.System, group1, null, null, null, false),
				helper.CreateSecurity(Env.Security.System, group2, null, null, null, true)
			};
			var ctx2 = new SecurityIteratorNodeContext<int>(null, security2);

			GlbSecurity[] security22 = { helper.CreateSecurity(Env.Security.System, group1, null, null, null, false) };
			var ctx22 = new SecurityIteratorNodeContext<int>(ctx2, security22);

			GlbSecurity[] security23 = { helper.CreateSecurity(Env.Security.System, group2, null, null, null, false) };
			var ctx23 = new SecurityIteratorNodeContext<int>(ctx2, security23);

			AssertNodeContext(new SecurityIteratorNodeContext<int>(null, null));
			AssertNodeContext(new SecurityIteratorNodeContext<int>(null, System.Array.Empty<GlbSecurity>()));
			AssertNodeContext(ctx1, security1[0]);
			AssertNodeContext(ctx11, security1[0], security11[0], security11[1], security11[2], security11[3], security11[4]);
			AssertNodeContext(ctx111, security1[0], security11[0], security111[1], security11[2], security11[3], security11[4]);
			AssertNodeContext(ctx2, security2[1]);
			AssertNodeContext(ctx22, security22[0], security2[1]); // Has base granted security for different group
			AssertNodeContext(ctx23, security23[0]);
		}

		public void TestParentGrantedSecuritiesFromOtherGroupsDoNotOverrideDeniedChildren()
		{
			SecurityTestHelper helper = new SecurityTestHelper(Factory);

			var group1 = Factory.New<GlbGroup>();
			var group2 = Factory.New<GlbGroup>();

			GlbSecurity[] previousSecurities =
			{
				helper.CreateSecurity(Env.Security.CustomsFiles, group2, null, null, null, true),
			};
			SecurityIteratorNodeContext<int> prevContext = new SecurityIteratorNodeContext<int>(null, previousSecurities);

			GlbSecurity[] securities =
			{
				helper.CreateSecurity(Env.Security.AUCustomsSCA, group1, null, null, null, false),
				helper.CreateSecurity(Env.Security.AUCustomsSCA, group2, null, null, null, false),
			};
			SecurityIteratorNodeContext<int> context = new SecurityIteratorNodeContext<int>(prevContext, securities);

			AssertNodeContext(context, securities[0]);
		}

		void AssertNodeContext<T>(SecurityIteratorNodeContext<T> ctx, params GlbSecurity[] expectedSecurity)
		{
			var securities = ctx.Securities;
			if (expectedSecurity.Length > 0)
			{
				AssertEquals(expectedSecurity.Length, securities.Count());
				foreach (var security in securities)
				{
					Assert(expectedSecurity.Contains(security));
				}
			}
			else
			{
				AssertNull(securities);
			}
		}
	}
}
