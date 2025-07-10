using CargoWise.EntityFramework.Testing;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusSCAContainerLookupsTest : TestCaseWithFactory
	{
		public void TestSCRContainerModes()
		{
			var container = Factory.New<CusSCAContainer>();
			AssertEquals("SCRContainerModes List", typeof(CodeDescriptionPairList), container.Lookups.SCRContainerModes.GetType());
			CodeDescriptionPairList list = container.Lookups.SCRContainerModes;
			AssertEquals("4 SCRContainerModes in list", 4, list.Count);
			Assert("SCRContainerModes Air", list.ContainsCode(Core.Constants.ContainerModes.AIR));
			Assert("SCRContainerModes MT", list.ContainsCode(Core.Constants.ContainerModes.Empty));
			Assert("SCRContainerModes Containerised", list.ContainsCode(Core.Constants.ContainerModes.Containerised));
			Assert("SCRContainerModes NonContainerised", list.ContainsCode(Core.Constants.ContainerModes.NonContainerised));
		}

		public void TestSCRContainerisedModes()
		{
			var container = Factory.New<CusSCAContainer>();
			AssertEquals("SCRContainerisedModes List", typeof(CodeDescriptionPairList), container.Lookups.SCRContainerisedModes.GetType());
			CodeDescriptionPairList list = container.Lookups.SCRContainerisedModes;
			AssertEquals("2 SCRContainerisedModes in list", 2, list.Count);
			Assert("SCRContainerisedModes MT", list.ContainsCode(Core.Constants.ContainerModes.Empty));
			Assert("SCRContainerisedModes Containerised", list.ContainsCode(Core.Constants.ContainerModes.Containerised));
		}

		public void TestSCRNonContainerModes()
		{
			var container = Factory.New<CusSCAContainer>();
			CusSCATestHelper helper = new CusSCATestHelper();
			AssertEquals("SCRNonContainerModes List", typeof(CodeDescriptionPairList), container.Lookups.SCRNonContainerModes.GetType());
			CodeDescriptionPairList list = container.Lookups.SCRNonContainerModes;
			AssertEquals("2 SCRNonContainerModes in list", 2, list.Count);
			Assert("SCRNonContainerModes Air", list.ContainsCode(Core.Constants.ContainerModes.AIR));
			Assert("SCRNonContainerModes NonContainerised", list.ContainsCode(Core.Constants.ContainerModes.NonContainerised));
		}
	}
}
