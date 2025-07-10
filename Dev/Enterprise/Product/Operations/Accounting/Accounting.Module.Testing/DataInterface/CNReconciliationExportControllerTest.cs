using CargoWise.EntityFramework;
using Enterprise.Accounting.DataTransfer.DataInterface;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(CNReconciliationExportController))]
	public class CNReconciliationExportControllerTest : ZSingletonControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.CNReconciliationExport;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new ChinaReconciliationExportWrapper();
		}
	}
}
