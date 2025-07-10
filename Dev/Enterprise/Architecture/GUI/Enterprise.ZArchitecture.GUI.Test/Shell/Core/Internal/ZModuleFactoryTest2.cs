using System.Linq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class ZModuleFactoryTest2 : TestCase
	{
		#region IsModuleOfType

		public void TestIsZFilterModule()
		{
			Assert(ZModuleFactory.Instance.IsZFilterModule(DummyModuleIDs.Dummy));
			Assert(!ZModuleFactory.Instance.IsZFilterModule(ModuleIDs.Registry));
		}

		public void TestIsZFilterGridModule()
		{
			Assert(ZModuleFactory.Instance.IsZFilterGridModule(DummyModuleIDs.Dummy));
			Assert(!ZModuleFactory.Instance.IsZFilterGridModule(ModuleIDs.Registry));
		}

		public void TestIsModuleOfType()
		{
			Assert(ZModuleFactory.Instance.IsModuleOfType<ZFilterGridModule>(DummyModuleIDs.Dummy));
			Assert(!ZModuleFactory.Instance.IsModuleOfType<ZFilterGridModule>(ModuleIDs.Registry));
		}

		#endregion

		#region IsZPopupModuleNonSingleton

		public void IsZPopupModuleNonSingleton()
		{
			Assert(ZModuleFactory.Instance.IsZPopupModuleNonSingleton(ModuleIDs.CartageLegPlanner));
			Assert(!ZModuleFactory.Instance.IsZPopupModuleNonSingleton(DummyModuleIDs.Dummy));
			Assert(!ZModuleFactory.Instance.IsZPopupModuleNonSingleton(ModuleIDs.Registry));
		}

		#endregion

		#region GetRegisteredIdentifier

		public void TestGetRegisteredIdentifierByTableName()
		{
			AssertEquals("Dummy", ZModuleFactory.Instance.GetRegisteredIdentifierByTableName("DummyBizo").Name);
		}

		public void TestGetRegisteredIdentifiersByTableName()
		{
			AssertContainsExactElementsInAnyOrder(["Dummy", "DummyNoPopup", "DummyWithTemplates", "DummyThatHitsFilterBizoOnDispose"], ZModuleFactory.Instance.GetRegisteredIdentifiersByTableName("DummyBizo").Select(i => i.Name));
		}

		public void TestGetRegisteredIdentifierByColumnNamePrefix()
		{
			AssertEquals("Dummy", ZModuleFactory.Instance.GetRegisteredIdentifierByColumnNamePrefix("Z0").Name);
		}

		public void TestGetRegisteredIdentifiersByColumnNamePrefix()
		{
			AssertContainsExactElementsInAnyOrder(["Dummy", "DummyNoPopup", "DummyWithTemplates", "DummyThatHitsFilterBizoOnDispose"], ZModuleFactory.Instance.GetRegisteredIdentifiersByColumnNamePrefix("Z0").Select(i => i.Name));
		}

		#endregion
	}
}
