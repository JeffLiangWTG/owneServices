using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(ConsolCusCAeMHController))]
	sealed class ConsolCusCAeMHControllerTest : ZControllerBasherTest
	{
		public void TestGetLoadedBusinessEntityInLocalFactory_WithConsol()
		{
			Consol.Factory.Save();
			AssertEquals(Consol.PK, Controller.GetLoadedBusinessEntityInLocalFactory(Consol).Identifier);
			AssertEquals(Controller.Factory, Controller.GetLoadedBusinessEntityInLocalFactory(Consol).Factory);
		}

		public void TestGetLoadedBusinessEntityInLocalFactory_WithCusCAeMHMaster()
		{
			CusCAeMHMaster.BP_ParentID = Consol.PK;
			CusCAeMHMaster.BP_ParentTableCode = Consol.TablePrefix;
			Factory.Save();
			AssertEquals(Consol.PK, Controller.GetLoadedBusinessEntityInLocalFactory(CusCAeMHMaster).Identifier);
			AssertEquals(Controller.Factory, Controller.GetLoadedBusinessEntityInLocalFactory(CusCAeMHMaster).Factory);
		}

		public override Type ControllerToBashType => typeof(ConsolCusCAeMHController);

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var bizO = Factory.New<CusCAeMHMaster>();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			bizO.BP_ParentID = consol.PK;
			bizO.BP_ParentTableCode = consol.TablePrefix;
			Factory.Save();
			return bizO;
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.CA.CAConsoleManifest;

		ConsolCusCAeMHControllerForTest controller;
		new ConsolCusCAeMHControllerForTest Controller => controller ?? (controller = new ConsolCusCAeMHControllerForTest());

		ForwardingConsol consol;
		ForwardingConsol Consol => consol ?? (consol = Factory.New<ForwardingConsol>());

		CusCAeMHMaster cusCAeMHMaster;
		CusCAeMHMaster CusCAeMHMaster => cusCAeMHMaster ?? (cusCAeMHMaster = Factory.New<CusCAeMHMaster>());

		sealed class ConsolCusCAeMHControllerForTest : ConsolCusCAeMHController
		{
			public new IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity) => base.GetLoadedBusinessEntityInLocalFactory(sourceEntity);
		}
	}
}
