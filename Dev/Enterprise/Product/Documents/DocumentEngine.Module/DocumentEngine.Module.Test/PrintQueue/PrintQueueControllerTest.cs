using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Module.Testing
{
	[TestedType(typeof(PrintQueueController))]
	sealed class PrintQueueControllerTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			StmPrintQueue printQueue = Factory.New<StmPrintQueue>();
			Factory.Save();
			return printQueue;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.PrintQueue;
		}
	}
}
