using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class CFRClassComponentTest : TestCaseWithFactory
	{
		public void TestHazardClassNotDisplayedForUNNO1993D()
		{
			var substance = Factory.New<UNDGSubstanceCFR>();
			substance.CFR_UNNO = "1993";
			substance.CFR_Variant = "d";
			substance.CFR_PrimaryClass = "Comb";

			Factory.Save();

			var dataItem = Factory.New<ForwardingUNDGDataItem>();
			dataItem.DI_DG = substance.PK;
			dataItem.LinkDefault(substance);

			var wrapper = new UNDGSubstanceWrapper(dataItem, Factory);

			var component = new CFRClassComponent() as IUNDGSummaryWriterComponent;

			AssertEquals(component.Write(wrapper), ZString.Empty);
		}

		public void TestHazardClassDisplayedForNonUNNO1993D()
		{
			var substance = Factory.New<UNDGSubstanceCFR>();
			substance.CFR_UNNO = "2000";
			substance.CFR_Variant = "d";
			substance.CFR_PrimaryClass = "1";

			Factory.Save();

			var dataItem = Factory.New<ForwardingUNDGDataItem>();
			dataItem.DI_DG = substance.PK;
			dataItem.LinkDefault(substance);

			var wrapper = new UNDGSubstanceWrapper(dataItem, Factory);

			var component = new CFRClassComponent() as IUNDGSummaryWriterComponent;

			AssertEquals(component.Write(wrapper), "class 1");
		}
	}
}
