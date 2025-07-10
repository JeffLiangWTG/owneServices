using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.GeneralLedger.GLConsolidations;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(GLConsolidationGroupController))]
	public class GLConsolidationGroupControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.GLConsolidationGroups;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var result = Factory.NewWithValidTestData<AccConsolidationGroup>();
			Factory.Save();
			return result;
		}
	}
}
