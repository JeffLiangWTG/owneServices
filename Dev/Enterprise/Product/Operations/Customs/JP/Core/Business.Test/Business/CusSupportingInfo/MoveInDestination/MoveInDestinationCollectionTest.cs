using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(MoveInDestinationCollection))]
	sealed class MoveInDestinationCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<MoveInDestination>
	{
		protected override CusSupportingInfoCollection<MoveInDestination> GetCusSupportingInfoCollection()
		{
			declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			return new MoveInDestinationCollection(entryInstruction);
		}

		public void TestSetDefaultValuesForNewChild_CSI_Code()
		{
			var collection = GetCusSupportingInfoCollection();
			var containerYardOrg = Factory.NewWithValidTestData<OrgHeader>();
			containerYardOrg.OH_Code = "OrgTest1";
			containerYardOrg.MainAddress.CustomsCodes.AddNew();

			var bondedWarehouseOrg = Factory.New<OrgHeader>();
			bondedWarehouseOrg.OH_Code = "OrgTest2";
			bondedWarehouseOrg.MainAddress.CustomsCodes.AddNew();

			var orgCusCode1 = containerYardOrg.MainAddress.CustomsCodes.AddNew();
			orgCusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Japan;
			orgCusCode1.OK_CodeType = OrgCusCode.JapanCodeTypes.CIE;
			orgCusCode1.OK_CustomsRegNo = "54321";

			var orgCusCode2 = bondedWarehouseOrg.MainAddress.CustomsCodes.AddNew();
			orgCusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Japan;
			orgCusCode2.OK_CodeType = OrgCusCode.JapanCodeTypes.CIE;
			orgCusCode2.OK_CustomsRegNo = "09876";

			declaration.ContainerYardDocAddress.OrganisationPK = containerYardOrg.PK;
			AssertEquals(ZString.Empty, collection.AddNew().CSI_Code);

			declaration.WarehouseDocAddress.OrganisationPK = bondedWarehouseOrg.PK;
			AssertEquals(ZString.Empty, collection.AddNew().CSI_Code);

			var orgCusCode3 = containerYardOrg.MainAddress.CustomsCodes.AddNew();
			orgCusCode3.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Japan;
			orgCusCode3.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			orgCusCode3.OK_CustomsRegNo = "12345";

			var orgCusCode4 = bondedWarehouseOrg.MainAddress.CustomsCodes.AddNew();
			orgCusCode4.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Japan;
			orgCusCode4.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			orgCusCode4.OK_CustomsRegNo = "67890";

			declaration.WarehouseDocAddress.OrganisationPK = ZGuid.Empty;
			declaration.ContainerYardDocAddress.OrganisationPK = containerYardOrg.PK;
			AssertEquals("12345", collection.AddNew().CSI_Code);

			declaration.WarehouseDocAddress.OrganisationPK = bondedWarehouseOrg.PK;
			AssertEquals("67890", collection.AddNew().CSI_Code);
		}

		public void TestDefaultValues()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_CargoQuantity = 5;
			entryInstruction.CEI_GrossWeight = 6;
			entryInstruction.CEI_Volume = 7;
			CombineAssertions(() =>
			{
				var moveInDestination = entryInstruction.MoveInDestinationInfos.AddNew();
				AssertEquals("First CSI_Quantity", entryInstruction.CEI_CargoQuantity, moveInDestination.CSI_Quantity);
				AssertEquals("First CSI_Quantity2", entryInstruction.CEI_CustomsWeight, moveInDestination.CSI_Quantity2);
				AssertEquals("First CSI_Quantity3", entryInstruction.CEI_CustomsVolume, moveInDestination.CSI_Quantity3);

				var moveInDestination2 = entryInstruction.MoveInDestinationInfos.AddNew();
				AssertEquals("Second CSI_Quantity", ZDecimal.Zero, moveInDestination2.CSI_Quantity);
				AssertEquals("Second CSI_Quantity2", ZDecimal.Zero, moveInDestination2.CSI_Quantity2);
				AssertEquals("Second CSI_Quantity3", ZDecimal.Zero, moveInDestination2.CSI_Quantity3);
			});
		}

		JobDeclaration declaration;
	}
}
