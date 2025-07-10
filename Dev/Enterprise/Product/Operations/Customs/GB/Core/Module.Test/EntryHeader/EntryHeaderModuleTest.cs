using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Module.Testing
{
	[TestedType(typeof(EntryHeaderModuleForTest))]
	internal class EntryHeaderModuleTest : Customs.Module.Testing.EntryHeaderModuleTest
	{
		public void TestGetNewController()
		{
			AssertType<EntryHeaderController>("Controller must be of type EntryHeaderController", module.GetNewController());
		}

		public void TestGetNewFilterControl()
		{
			using (EntryHeaderModuleForTest module = new EntryHeaderModuleForTest())
			{
				ZArchitecture.GUI.IFilterControl filterControl = module.GetNewFilterControl_Exposed();
				Assert("Filter Control must be of type EntryHeaderFilterUserControl", filterControl is EntryHeaderFilterUserControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			AssertType<EntryHeaderFilterBusinessObject>("Filter Business Object must be of type EntryHeaderFilterBusinessObject", module.GetNewFilterBusinessObject_Exposed());
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
			public ZArchitecture.GUI.IFilterControl GetNewFilterControl_Exposed() => base.GetNewFilterControl();

			public FilterBusinessObject GetNewFilterBusinessObject_Exposed() => base.GetNewFilterBusinessObject();
		}
	}
}
