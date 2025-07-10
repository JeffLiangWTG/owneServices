using System.Linq;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeclarationGoodsShipmentConsignmentDmExtensionsWrapperTest : DataProviderTestCase<DeclarationGoodsShipmentConsignmentDmExtensionsWrapper>
	{
		public void TestNewOrNull()
		{
			var factory = Factory;
			var jobDeclaration = factory.New<JobDeclaration>();
			var cusEntryInstruction = factory.New<CusEntryInstruction>();
			AssertNull("entryInstruction is must", DeclarationGoodsShipmentConsignmentDmExtensionsWrapper.NewOrNull(null, jobDeclaration));
			AssertNull("jobDeclaration  is must", DeclarationGoodsShipmentConsignmentDmExtensionsWrapper.NewOrNull(cusEntryInstruction, null));
			AssertNotNull(DeclarationGoodsShipmentConsignmentDmExtensionsWrapper.NewOrNull(cusEntryInstruction, jobDeclaration));
		}

		public void TestCargoDescription()
		{
			AssertNull("CargoDescription", Provider.CargoDescription);
		}

		public void TestExportationCountryCode()
		{
			AssertNotNull("ExportationCountryCode", Provider.ExportationCountryCode);
			AssertEquals("ExportationCountryCode should be set as expected", "IL", Provider.ExportationCountryCode.Value);
		}

		public void TestLastReleaseFromWarehousInd()
		{
			AssertNull("LastReleaseFromWarehousInd", Provider.LastReleaseFromWarehousInd);
		}

		public void TestPackagesMeasure()
		{
			AssertNotNull("PackagesMeasure", Provider.PackagesMeasure);
			AssertEquals("PackagesMeasure count should be as expected", 1, Provider.PackagesMeasure.Count);
		}

		public void TestRegisteredFacility()
		{
			AssertNotNull("RegisteredFacility", Provider.RegisteredFacility);
			AssertEquals("RegisteredFacility count should be as expected", 1, Provider.RegisteredFacility.Count);
			AssertType<DeclarationGoodsShipmentConsignmentDmExtensionsRegisteredFacilityStorageSiteWrapper>(Provider.RegisteredFacility.First());
		}

		protected override DeclarationGoodsShipmentConsignmentDmExtensionsWrapper GetProvider()
		{
			var factory = Factory;
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "IL123";
			declaration.JE_MessageType = "IMP";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.EntryNumber = "CE123";
			var cusEntryInstruction = factory.New<CusEntryInstruction>();
			entryHeader.CH_CEI_Instruction = cusEntryInstruction.PK;
			declaration.JE_GoodsOrigin = "IL";
			var package1 = declaration.Packages.AddNew();
			var package2 = declaration.Packages.AddNew();

			return DeclarationGoodsShipmentConsignmentDmExtensionsWrapper.NewOrNull(cusEntryInstruction, declaration);
		}
	}
}
