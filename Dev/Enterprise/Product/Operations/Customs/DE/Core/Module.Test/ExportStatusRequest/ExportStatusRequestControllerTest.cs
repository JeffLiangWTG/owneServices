using Enterprise.Customs.DE.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Module.Testing
{
	[TestedType(typeof(ExportStatusRequestController))]
	public class ExportStatusRequestControllerTest : ZArchitecture.Modules.Testing.ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.DE.ExportStatusRequest;

		public void TestBusinessObject()
		{
			AssertEquals(typeof(StatusRequest), GetBusinessObjectType());
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Germany;
	}
}
