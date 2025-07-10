using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class CFRPoisonInhalationHazardComponentTest : TestCaseWithFactory
	{
		public void TestPoisonInhalationHazard_IsEmpty()
		{
			var substance = Factory.New<UNDGSubstanceCFR>();
			substance.CFR_UNNO = "1234";
			substance.CFR_PoisonInhalationHazard = ZString.Empty;

			Factory.Save();

			var cfrSubstance = UNDGSubstanceLoader.LoadSubstances(Factory, "1234", standard: UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR).First();

			var undgDataItem = Factory.NewWithValidTestData<ForwardingUNDGDataItem>();
			undgDataItem.DI_DG = cfrSubstance.PK;
			undgDataItem.LinkDefault(cfrSubstance);

			var wrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);
			var poisonInhalationHazardComponent = new CFRPoisonInhalationHazardComponent() as IUNDGSummaryWriterComponent;
			var result = poisonInhalationHazardComponent.Write(wrapper);

			AssertEquals("PIH is not on substance", ZString.Empty, result);
		}

		public void TestPoisonInhalationHazard_SubstanceHasPIH()
		{
			var substance = Factory.New<UNDGSubstanceCFR>();
			substance.CFR_UNNO = "1234";
			substance.CFR_PoisonInhalationHazard = UNDGSubstanceCFRLookups.PoisonInhalationHazardProvisions.Codes.SP1Raw;

			Factory.Save();

			var cfrSubstance = UNDGSubstanceLoader.LoadSubstances(Factory, "1234", standard: UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR).First();

			var undgDataItem = Factory.NewWithValidTestData<ForwardingUNDGDataItem>();
			undgDataItem.DI_DG = cfrSubstance.PK;
			undgDataItem.LinkDefault(cfrSubstance);

			var wrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);
			var poisonInhalationHazardComponent = new CFRPoisonInhalationHazardComponent() as IUNDGSummaryWriterComponent;
			var result = poisonInhalationHazardComponent.Write(wrapper);

			AssertEquals("PIH is set", "Poison-Inhalation Hazard Zone A", result);
		}
	}
}
