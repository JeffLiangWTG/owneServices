using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeclarationGoodsShipmentConsignmentDMExtensionsPackagesMeasureWrapperTest : DataProviderTestCase<DeclarationGoodsShipmentConsignmentDmExtensionsPackagesMeasureWrapper>
	{
		public void TestNewOrNull()
		{
			var factory = Factory;
			var jobDeclaration = factory.New<JobDeclaration>();
			var cusEntryInstruction = factory.New<CusEntryInstruction>();
			AssertNull("entryInstruction is must", DeclarationGoodsShipmentConsignmentDmExtensionsPackagesMeasureWrapper.NewOrNull(null, jobDeclaration));
			AssertNull("package is must", DeclarationGoodsShipmentConsignmentDmExtensionsPackagesMeasureWrapper.NewOrNull(cusEntryInstruction, null));
			AssertNotNull(DeclarationGoodsShipmentConsignmentDmExtensionsPackagesMeasureWrapper.NewOrNull(cusEntryInstruction, jobDeclaration));
		}

		public void TestGrossMassMeasure()
		{
			AssertNotNull("GrossMassMeasure", Provider.GrossMassMeasure);
			AssertEquals("GrossMassMeasure value should be set as expected", 100m, Provider.GrossMassMeasure.Value);
			AssertEquals("GrossMassMeasure unit should be set as expected", MeasurementUnitCommonCodeContentType.Kg, Provider.GrossMassMeasure.UnitCode);
		}

		public void TestMarksNumbers()
		{
			AssertNull("MarksNumbers", Provider.MarksNumbers);
		}

		public void TestPackageMeasureQualifier()
		{
			AssertNotNull("PackageMeasureQualifier", Provider.PackageMeasureQualifier);
			AssertEquals("PackageMeasureQualifier should be set as expected", "2", Provider.PackageMeasureQualifier.Value);
		}

		public void TestTotalPackageQuantity()
		{
			AssertNotNull("TotalPackageQuantity", Provider.TotalPackageQuantity);
			AssertEquals("TotalPackageQuantity value should be set as expected", 12m, Provider.TotalPackageQuantity.Value);
			AssertEquals("TotalPackageQuantity unit should be set as expected", null, Provider.TotalPackageQuantity.UnitCode);
		}

		public void TestTypeCode()
		{
			AssertNotNull("TypeCode", Provider.TypeCode);
			AssertEquals("TypeCode value should be set as expected", "VN", Provider.TypeCode.Value);
		}

		protected override DeclarationGoodsShipmentConsignmentDmExtensionsPackagesMeasureWrapper GetProvider()
		{
			var factory = Factory;
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "IL123";
			declaration.JE_MessageType = "IMP";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.EntryNumber = "CE123";
			var cusEntryInstruction = factory.New<CusEntryInstruction>();
			cusEntryInstruction.CEI_NumberOfPackages = 12;
			cusEntryInstruction.CEI_CustomsPackType = "VN";
			entryHeader.CH_CEI_Instruction = cusEntryInstruction.PK;
			declaration.JE_TotalWeight = 100;
			declaration.JE_TotalWeightUnit = "KG";
			
			return DeclarationGoodsShipmentConsignmentDmExtensionsPackagesMeasureWrapper.NewOrNull(cusEntryInstruction, declaration);
		}
	}
}
