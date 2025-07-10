using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(PremisesController))]
	sealed class PremisesControllerTest : CMRSearchOnlyControllerTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.AU.Premises;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var result = CMRAqisPremises.New(Factory);
			Factory.Save();
			return result;
		}
	}
}
