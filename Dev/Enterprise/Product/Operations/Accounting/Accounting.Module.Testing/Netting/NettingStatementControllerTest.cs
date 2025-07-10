using CargoWise.EntityFramework;
using Enterprise.Accounting.Netting;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(NettingStatementController))]
	public class NettingStatementControllerTest : ZSingletonControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.NettingStatement;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new NettingDocumentPrinter(new BusinessObjectFactory());
		}
	}
}
