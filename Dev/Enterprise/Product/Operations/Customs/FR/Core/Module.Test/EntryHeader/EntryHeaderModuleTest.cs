using System;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Module.EntryHeader.Testing
{
	[TestedType(typeof(EntryHeaderModuleForTest))]
	public class EntryHeaderModuleTest : Customs.Module.Testing.EntryHeaderModuleTest
	{
		protected override Type ExpectedOperationalActionSupporterType => typeof(EntryHeaderOperationalActionSupporter);

		public void TestGetNewController()
		{
			AssertType<EntryHeaderController>("Controller must be of type EntryHeaderController", module.GetNewController());
		}

		public void TestHasActions()
		{
			AssertEquals("HasActions must be true", true, module.HasActions);
		}

		public void TestGetNewFilterControl()
		{
			using (EntryHeaderModuleForTest module = new EntryHeaderModuleForTest())
			{
				IFilterControl filterControl = module.GetNewFilterControl_Exposed();
				Assert("Filter Control must be of type EntryHeaderFilterUserControl", filterControl is EntryHeaderFilterUserControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			AssertType<EntryHeaderFilterBusinessObject>("Filter Business Object must be of type EntryHeaderFilterBusinessObject", module.GetNewFilterBusinessObject_Exposed());
		}

		public void TestAllowNew()
		{
			AssertEquals("AllowNew must be false", false, module.AllowNew);
		}

		public void TestAllowDelete()
		{
			AssertEquals("AllowDelete must be false", false, module.AllowDelete);
		}

		protected override void SetUp()
		{
			base.SetUp();
			module = new EntryHeaderModuleForTest();
		}

		protected override void TearDown()
		{
			module.Dispose();
			base.TearDown();
		}

		EntryHeaderModuleForTest module;

		class EntryHeaderModuleForTest : EntryHeaderModule
		{
			public IFilterControl GetNewFilterControl_Exposed() => base.GetNewFilterControl();

			public FilterBusinessObject GetNewFilterBusinessObject_Exposed() => base.GetNewFilterBusinessObject();
		}
	}
}
