using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class QuarantineExDocShipsCompartmentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckQC_Compartments()
		{
			Assert("Pre-Condition", !shipsCompartment.QC_CompartmentsInfo.HasMessageErrors());
			shipsCompartment.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			shipsCompartment.QC_Compartments = "12,45,89";
			Assert("Compartments cannot be set for produce type of dairy", shipsCompartment.QC_CompartmentsInfo.HasMessageErrors());
			shipsCompartment.QC_Compartments = ZString.Empty;
			Assert("Compartments must be blank for produce type of dairy", !shipsCompartment.QC_CompartmentsInfo.HasMessageErrors());
			shipsCompartment.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			shipsCompartment.QC_Compartments = ZString.Empty;
			Assert("Compartments cannot be empty for produce type grains and plants", shipsCompartment.QC_CompartmentsInfo.HasMessageErrors());
			shipsCompartment.QC_Compartments = "12,34";
			Assert("Correct produce type and compartments", !shipsCompartment.QC_CompartmentsInfo.HasMessageErrors());
		}

		public void TestCheckQC_RL_NKInspectionPort()
		{
			Assert("Pre-Condition", !shipsCompartment.QC_RL_NKInspectionPortInfo.HasMessageErrors());
			shipsCompartment.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			shipsCompartment.QC_RL_NKInspectionPort = "AUSYD";
			Assert("Inspection port cannot be set for produce type of meat", shipsCompartment.QC_RL_NKInspectionPortInfo.HasMessageErrors());
			shipsCompartment.QC_RL_NKInspectionPort = ZString.Empty;
			Assert("Inspection port must be blank for produce type of meat", !shipsCompartment.QC_RL_NKInspectionPortInfo.HasMessageErrors());
			shipsCompartment.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			shipsCompartment.QC_RL_NKInspectionPort = ZString.Empty;
			Assert("Inspection port cannot be empty for produce type grains and plants", shipsCompartment.QC_RL_NKInspectionPortInfo.HasMessageErrors());
			shipsCompartment.QC_RL_NKInspectionPort = "ZZZZZ";
			Assert("Inspection port must be a valid port for produce type grains and plants", shipsCompartment.QC_RL_NKInspectionPortInfo.HasMessageErrors());
			shipsCompartment.QC_RL_NKInspectionPort = "AUSYD";
			Assert("Correct produce type and inspection port", !shipsCompartment.QC_RL_NKInspectionPortInfo.HasMessageErrors());
		}

		public void TestCheckQC_InspectionDate()
		{
			Assert("Pre-Condition", !shipsCompartment.QC_InspectionDateInfo.HasMessageErrors());
			shipsCompartment.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			shipsCompartment.QC_InspectionDate = ZDateTime.Now;
			Assert("Inspection date cannot be set for produce type of fish", shipsCompartment.QC_InspectionDateInfo.HasMessageErrors());
			shipsCompartment.QC_InspectionDate = ZDateTime.Empty;
			Assert("Inspection date must be blank for produce type of fish", !shipsCompartment.QC_InspectionDateInfo.HasMessageErrors());
			shipsCompartment.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			shipsCompartment.QC_InspectionDate = ZDateTime.Empty;
			Assert("Inspection date cannot be empty for produce type of fish", shipsCompartment.QC_InspectionDateInfo.HasMessageErrors());
			shipsCompartment.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			shipsCompartment.QC_InspectionDate = ZDateTime.Now;
			Assert("Correct produce type and inspection date", !shipsCompartment.QC_InspectionDateInfo.HasMessageErrors());
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			var quarantineHeader = helper.Header1.QuarantineExDocHeader;
			shipsCompartment = quarantineHeader.Compartments.AddNew();
			quarantineHeader.Compartments.Add(shipsCompartment);
		}

		QuarantineExDocShipsCompartment shipsCompartment;
	}
}
