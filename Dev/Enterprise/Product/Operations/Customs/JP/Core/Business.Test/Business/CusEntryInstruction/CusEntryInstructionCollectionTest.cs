using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(CusEntryInstructionCollection))]
	sealed class CusEntryInstructionCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusEntryInstructionCollection(Factory.New<JobDeclaration>());
		}

		public void TestDefaultValues()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GoodsDescription = "Desc";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals("Desc", instruction.CEI_GoodsDescription);

			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Kilograms;
			declaration.JE_TotalWeight = 2025;
			instruction.CEI_GrossWeightUnit = Core.Constants.Weight.Tonnes;
			instruction.CEI_GrossWeight = 2;

			declaration.JE_TotalVolumeUnit = Core.Constants.Volume.Litre;
			declaration.JE_TotalVolume = 2025;
			instruction.CEI_VolumeUnit = Core.Constants.Volume.Litre;
			instruction.CEI_Volume = 2000;

			var anotherInstruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals(25m, anotherInstruction.CEI_GrossWeight);
			AssertEquals(Core.Constants.Weight.Kilograms, anotherInstruction.CEI_GrossWeightUnit);
			AssertEquals(25m, anotherInstruction.CEI_Volume);
			AssertEquals(Core.Constants.Volume.Litre, anotherInstruction.CEI_VolumeUnit);

			declaration.JE_TotalWeightUnit = "";
			declaration.JE_TotalVolumeUnit = "";
			anotherInstruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals(0m, anotherInstruction.CEI_GrossWeight);
			AssertEquals("", anotherInstruction.CEI_GrossWeightUnit);
			AssertEquals(0m, anotherInstruction.CEI_Volume);
			AssertEquals("", anotherInstruction.CEI_VolumeUnit);
		}

		public void TestDefaultCEI_CargoQuantity()
		{
			var refPacks = Factory.New<CusRefPacks>();
			refPacks.RP_ConversionFactor = 2m;
			refPacks.RP_Type = RPTypeList.Codes.DeclarationTotal;
			refPacks.RP_CustomsPack = "CT";
			refPacks.RP_CommercialPack = "PLT";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TotalNoOfPacks = 2;
			declaration.JE_TotalNoOfPacksPackType = "PLT";

			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals(4m, instruction1.CEI_CargoQuantity);
			AssertEquals("CT", instruction1.CEI_CargoQuantityUnit);

			instruction1.CEI_CargoQuantity = 2m;
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals(2m, instruction2.CEI_CargoQuantity);
			AssertEquals("CT", instruction2.CEI_CargoQuantityUnit);

			instruction2.CEI_CargoQuantityUnit = ZString.Empty;
			var instruction3 = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals(2m, instruction3.CEI_CargoQuantity);
			AssertEquals("CT", instruction3.CEI_CargoQuantityUnit);

			declaration.JE_TotalNoOfPacksPackType = "PKG";
			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			var instruction4 = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals(0m, instruction4.CEI_CargoQuantity);
			AssertEquals(ZString.Empty, instruction4.CEI_CargoQuantityUnit);
		}
	}
}
