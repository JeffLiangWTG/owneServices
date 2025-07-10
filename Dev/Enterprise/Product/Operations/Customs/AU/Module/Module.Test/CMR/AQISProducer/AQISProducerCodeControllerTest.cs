using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(AQISProducerCodeController))]
	sealed class AQISProducerCodeControllerTest : CMRSearchOnlyControllerTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.AU.AQISProducerCode;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var result = CMRAqisProducer.New(Factory);
			Factory.Save();
			return result;
		}
	}
}
