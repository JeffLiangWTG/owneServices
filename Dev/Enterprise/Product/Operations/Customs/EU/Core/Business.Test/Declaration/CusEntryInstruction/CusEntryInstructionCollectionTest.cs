using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryInstructionCollection<CusEntryInstruction>))]
	public class CusEntryInstructionCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetDefaultsForFirstNewChild_ShouldSetTotalInnerPackages_ForDeclarationAttachedToShipment()
		{
			var localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.Country.RN_Code)).Code;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "GBANY";
			consol.JK_RL_NKDischargePort = localPort;
			consol.JK_ConsolMode = ContainerModes.BuyersConsol;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_PackingMode = ContainerModes.BuyersConsol;
			shipment.JS_TotalPackageCount = 78;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			declaration.JE_JS = shipment.PK;

			CombineAssertions("CEI_TotalInnerPackages of first added entry instruction should be defaulted from shipment if applicable and set to readonly.", () =>
			{
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				AssertEquals("First CEI.CEI_TotalInnerPackages: ", 78, entryInstruction.CEI_TotalInnerPackages);
				Assert("First CEI.CEI_TotalInnerPackagesInfo.ReadOnly should be true.", entryInstruction.CEI_TotalInnerPackagesInfo.ReadOnly);

				var secondEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
				AssertEquals("Second CEI.CEI_TotalInnerPackages: ", 0, secondEntryInstruction.CEI_TotalInnerPackages);
				Assert("Second CEI.CEI_TotalInnerPackagesInfo.ReadOnly should be false.", !secondEntryInstruction.CEI_TotalInnerPackagesInfo.ReadOnly);
			});
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			return new CusEntryInstructionCollection<CusEntryInstruction>(testDeclaration);
		}
	}
}
