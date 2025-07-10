using CargoWise.EntityFramework;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.EndpointManagement.Module.Testing
{
	[TestedType(typeof(EdiTrustedMessagingConfigModule))]
	public class EdiTrustedMessagingConfigModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return Modules.ClientModuleRegistration.EdiTrustedMessagingConfig;
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			var item = collection.AddNew() as EdiTrustedMessagingConfig;
			item.ETM_Product = "CW1";
			collection.Factory.Save();
			base.AddTestObjects(collection);
		}
	}
}
