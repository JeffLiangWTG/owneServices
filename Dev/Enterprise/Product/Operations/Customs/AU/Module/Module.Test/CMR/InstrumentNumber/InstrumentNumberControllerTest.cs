using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(InstrumentNumberController))]
	sealed class InstrumentNumberControllerTest : CMRSearchOnlyControllerTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.AU.InstrumentNumber;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var result = CMRInstrument.New(Factory);
			Factory.Save();
			return result;
		}
	}
}
