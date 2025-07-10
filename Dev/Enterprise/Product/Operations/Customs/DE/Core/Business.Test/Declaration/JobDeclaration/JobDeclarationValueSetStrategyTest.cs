using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class JobDeclarationValueSetStrategyTest : TestCaseWithFactory
	{
		public void TestDefaultImporterChanged_UpdateFinalDestinationPortFromImporter()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_RL_NKClosestPort = "DEBER";
			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_RL_NKClosestPort = "GBLON";

			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_OverrideFreightDefaults = false;

			CombineAssertions(() =>
			{
				declaration.JE_OH_Importer = orgHeader.PK;
				AssertEquals("Import", "DEBER", declaration.JE_RL_NKFinalDestination);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				declaration.JE_OH_Importer = orgHeader2.PK;
				AssertEquals("Not Import", ZString.Empty, declaration.JE_RL_NKFinalDestination);
			});
		}
	}
}
