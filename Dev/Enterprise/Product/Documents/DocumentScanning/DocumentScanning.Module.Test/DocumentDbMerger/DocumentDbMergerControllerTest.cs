using CargoWise.EntityFramework;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Module.Testing
{
	[TestedType(typeof(DocumentDbMergerController))]
	internal sealed class DocumentDbMergerControllerTest : ZSingletonControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new DocumentDbMerger(new DocumentFactoryProvider().GetFactory(Factory));
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.DocumentDbMerger;
		}
	}
}
