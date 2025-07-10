using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(DependenceCollectionProvider))]
	sealed class DependenceCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		public void TestCreateCollectionForDependenceCollectionProvider()
		{
			AssertNull("Default collection should not be set", Provider.Collection);

			SetUpProvider(Provider);

			Provider.SetDependencyValue("staff and resource");
			AssertEquals(typeof(GlbStaffAndResourceCollection), Provider.Collection.GetType());

			Provider.SetDependencyValue("blahblah");
			AssertNull("Default collection should not be set", Provider.Collection);
		}

		public void TestModuleIDForDependenceCollectionProvider()
		{
			AssertEquals(ModuleIDs.NotAssigned, Provider.ModuleID);

			SetUpProvider(Provider);

			Provider.SetDependencyValue("staff and resource");
			AssertEquals(ModuleIDs.GlbStaff, Provider.ModuleID);

			Provider.SetDependencyValue("org");
			AssertEquals(ModuleIDs.Organisation, Provider.ModuleID);

			Provider.SetDependencyValue("blahblah");
			AssertEquals(ModuleIDs.NotAssigned, Provider.ModuleID);
		}

		public void TestGetCollectionForFindboxForDependenceCollectionProvider()
		{
			AssertNull("Default findbox collection should not be set", Provider.CollectionForFindbox);

			SetUpProvider(Provider);

			Provider.SetDependencyValue("staff and resource");
			AssertEquals(typeof(GlbStaffAndResourceCollection), Provider.CollectionForFindbox.GetType());

			Provider.SetDependencyValue("org");
			AssertEquals(typeof(OrgHeaderCollection), Provider.CollectionForFindbox.GetType());

			Provider.SetDependencyValue("blahblah");
			AssertNull("Default findbox collection should not be set", Provider.CollectionForFindbox);
		}

		public void TestMaxLengthForDependenceCollectionProvider()
		{
			AssertEquals(-1, Provider.MaxLength);

			SetUpProvider(Provider);

			Provider.SetDependencyValue("staff and resource");
			AssertEquals(GlbStaffSchema.GS_Code.MaxLength, Provider.MaxLength);

			Provider.SetDependencyValue("org");
			AssertEquals(-1, Provider.MaxLength);

			Provider.SetDependencyValue("blahblah");
			AssertEquals(-1, Provider.MaxLength);
		}

		void SetUpProvider(DependenceCollectionProvider provider)
		{
			provider.List.Add("staff and resource", new StaffAndResourceCollectionProvider(Factory));
			provider.List.Add("org", new OrgHeaderCollectionProvider(Factory));
			provider.List.Add("gl", new AccGLHeaderCollectionProvider(Factory));
		}

		protected override Type ExpectedCollectionType => null;

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.NotAssigned;

		protected override int ExpectedMaxLength => -1;

		new DependenceCollectionProvider Provider => (DependenceCollectionProvider)base.Provider;
	}
}
