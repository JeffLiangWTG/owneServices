using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class RadioactiveTransportIndexComponentTest : TestCaseWithFactory
	{
		public void TestWrite_WhenTransportIndexIsNullOrEmpty()
		{
			var undg = new UNDGSubstanceWrapper(null, Factory);
			var component = new RadioactiveTransportIndexComponent();

			AssertNull("Precondition.", undg.DGData);
			AssertEquals(string.Empty, component.Write(undg));
		}

		public void TestWrite_WhenTransportIndexNotNullOrEmpty()
		{
			var cfrSubstance = Factory.New<UNDGSubstanceCFR>();
			cfrSubstance.CFR_UNNO = "000";
			Factory.Save();

			var dgSubstance = UNDGSubstanceLoader.LoadSubstances(Factory, "000", standard: UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR).First();
			var dgItem = Factory.New<UNDGDataItem>();
			dgItem.DI_RadioactiveTransportIndex = 69.69;
			dgItem.LinkDefault(dgSubstance);

			var undg = new UNDGSubstanceWrapper(dgItem, Factory);
			var component = new RadioactiveTransportIndexComponent();

			AssertNotNull("Precondition.", undg.DGData);
			AssertEquals("TI = 69.69", component.Write(undg));
		}

		public void TestWrite_WhenTransportIndexIsNotSet()
		{
			var cfrSubstance = Factory.New<UNDGSubstanceCFR>();
			cfrSubstance.CFR_UNNO = "000";
			Factory.Save();

			var dgSubstance = UNDGSubstanceLoader.LoadSubstances(Factory, "000", standard: UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR).First();
			var dgItem = Factory.New<UNDGDataItem>();
			dgItem.DI_RadioactiveTransportIndex = 0;
			dgItem.LinkDefault(dgSubstance);

			var undg = new UNDGSubstanceWrapper(dgItem, Factory);
			var component = new RadioactiveTransportIndexComponent();

			AssertNotNull("Precondition.", undg.DGData);
			AssertEquals(ZString.Empty, component.Write(undg));
		}
	}
}
