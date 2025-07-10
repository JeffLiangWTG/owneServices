using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class CFRSpecialPermitNumberComponentTest : TestCaseWithFactory
	{
		public void TestSpecialPermitNumber_EmptyValue()
		{
			var undgDataItem = Factory.NewWithValidTestData<UNDGDataItem>();
			undgDataItem.DI_SpecialPermitNumber = ZString.Empty;

			var wrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);
			var specialPermitNumberComponent = new CFRSpecialPermitNumberComponent() as IUNDGSummaryWriterComponent;
			var result = specialPermitNumberComponent.Write(wrapper);

			AssertEquals("No permit number set so component should return empty string", ZString.Empty, result);
		}

		public void TestSpecialPermitNumber_ValueIsSet()
		{
			var undgDataItem = Factory.NewWithValidTestData<UNDGDataItem>();
			undgDataItem.DI_SpecialPermitNumber = "1234";

			var wrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);
			var specialPermitNumberComponent = new CFRSpecialPermitNumberComponent() as IUNDGSummaryWriterComponent;
			var result = specialPermitNumberComponent.Write(wrapper);

			AssertEquals("Permit number is set - component should return number prefixed with DOT-SP", "DOT-SP 1234", result);
		}
	}
}
