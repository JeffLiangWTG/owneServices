using CargoWise.EntityFramework;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.EndpointManagement.Module.Testing
{
	[TestedType(typeof(EdiTrustedSystemModule))]
	public class EdiTrustedSystemModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return Modules.ClientModuleRegistration.EdiTrustedSystem;
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			var item = collection.AddNew() as EdiTrustedSystem;
			item.ETS_Product = "CW1";
			collection.Factory.Save();
			base.AddTestObjects(collection);
		}
	}
}
