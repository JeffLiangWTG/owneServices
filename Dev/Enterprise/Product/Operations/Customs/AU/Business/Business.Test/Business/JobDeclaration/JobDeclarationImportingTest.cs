using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class JobDeclarationImportingTest : TestCaseWithFactory
	{
		public void TestJE_VesselNameDuringImporting()
		{
			OrgHeader originalShipper = GetShippingLine();
			OrgHeader testShipper1 = GetShippingLine();
			OrgHeader testShipper2 = GetShippingLine();
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_OH_ShippingLine = originalShipper.PK;
			((ISupportDataImporting)testDec).IsImportingData = true;
			testDec.JE_VesselName = GetVesselNKWithShippingLine(testShipper1);
			AssertEquals("Shipping Line should still be Original Shipper while importing", originalShipper.PK, testDec.JE_OH_ShippingLine);
			((ISupportDataImporting)testDec).IsImportingData = false;
			testDec.JE_VesselName = GetVesselNKWithShippingLine(testShipper2);
			AssertEquals("Shipping Line should still be Test Shipper 2 when not importing", originalShipper.PK, testDec.JE_OH_ShippingLine);
		}

		public void TestMarkAsNeedingValidationForImportDeliveryAddress()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_RL_NKClosestPort = "AUSYD";

			var address = importer.Addresses.AddNew();
			address.OA_Address1 = "127 Church St";
			address.OA_City = "Hay";
			address.OA_PostCode = "2711";
			address.OA_State = "NSW";
			address.AddressCapability[0].Enabled = true;
			address.RunPreSaveValidation();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.ImporterDeliveryAddress.E2_OA_Address = address.PK;
			AssertNoMessageErrors(declaration.ImporterDeliveryAddress);

			declaration.ImporterDeliveryAddress.RunPreSaveValidation();
			Assert(declaration.ImporterDeliveryAddress.LightValidationIsValid);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			Assert(!declaration.ImporterDeliveryAddress.LightValidationIsValid);
		}

		#region Implementation

		OrgHeader GetShippingLine()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_IsShippingLine = true;
			result.OH_FullName = "Shipping Line";
			return result;
		}

		ZString GetVesselNKWithShippingLine(OrgHeader shipper)
		{
			RefVessel result = RefVessel.New(Factory);
			result.RV_OH = shipper.PK;
			return result.RV_Code;
		}

		#endregion
	}
}
