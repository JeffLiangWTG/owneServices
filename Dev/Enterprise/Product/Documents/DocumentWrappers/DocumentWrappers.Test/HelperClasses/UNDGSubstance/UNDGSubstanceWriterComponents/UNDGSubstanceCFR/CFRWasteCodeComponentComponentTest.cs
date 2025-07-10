using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class CFRWasteCodeComponentComponentTest : TestCaseWithFactory
	{
		public void TestWasteCode_IsEmpty()
		{
			var undgDataItem = Factory.NewWithValidTestData<UNDGDataItem>();
			undgDataItem.DI_HazardousWasteCode = ZString.Empty;

			var wrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);
			var wasteCodeComponent = new CFRWasteCodeComponent() as IUNDGSummaryWriterComponent;
			var result = wasteCodeComponent.Write(wrapper);

			AssertEquals("No permit number set so component should return empty string", ZString.Empty, result);
		}

		public void TestWasteCode_ValueIsSet()
		{
			var undgDataItem = Factory.NewWithValidTestData<UNDGDataItem>();
			undgDataItem.DI_HazardousWasteCode = "1234";

			var wrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);
			var wasteCodeComponent = new CFRWasteCodeComponent() as IUNDGSummaryWriterComponent;
			var result = wasteCodeComponent.Write(wrapper);

			AssertEquals("Waste code is set - Component should just return waste code", "1234", result);
		}
	}
}
