using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(ExportCustomsManifestController))]
	sealed class ExportCustomsManifestControllerTest : ZControllerBasherTest
	{
		public void TestGetNewBusinessEntityInLocalFactory()
		{
			ExportCustomsManifestController controller = new ExportCustomsManifestController();
			var header1 = Factory.New<ExportCustomsManifestHeader>();
			controller.SetNewBusinessObjectToReturn(header1);
			var header2 = (ExportCustomsManifestHeader)((ZControllerInternals)controller).GetNewBusinessEntityInLocalFactory();
			var header3 = (ExportCustomsManifestHeader)((ZControllerInternals)controller).GetNewBusinessEntityInLocalFactory();
			AssertSame(header1, header2);
			AssertNotSame(header2, header3);
		}

		protected override string CountryCode => "AU";

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.AU.ExportCustomsManifest;
	}
}
