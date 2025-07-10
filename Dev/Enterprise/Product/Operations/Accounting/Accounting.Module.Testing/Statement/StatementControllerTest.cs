using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(StatementController))]
	public class StatementControllerTest : ZSingletonControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Statement;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return Statement.New(GlbBranch.CurrentBranch);
		}
	}
}
