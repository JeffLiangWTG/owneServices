using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(CMREstablishmentCodesController))]
	sealed class CMREstablishmentCodesControllerTest : CMRSearchOnlyControllerTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.AU.CMREstablishmentCodes;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var establishmentCodes = Factory.New<CMREstablishmentCodes>();
			Factory.Save();
			return establishmentCodes;
		}
	}
}
