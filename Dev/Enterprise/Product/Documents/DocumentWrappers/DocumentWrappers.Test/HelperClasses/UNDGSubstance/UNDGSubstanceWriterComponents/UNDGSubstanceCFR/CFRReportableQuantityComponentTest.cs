using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class CFRReportableQuantityComponentTest : TestCaseWithFactory
	{
		public void TestReportableQuantity_WeightExceedsReportableQuantity()
		{
			var substance = Factory.New<UNDGSubstanceCFR>();
			substance.CFR_UNNO = "1234";
			substance.CFR_ReportableQuantity = 100;
			substance.CFR_ReportableQuantityUnit = "lb";

			Factory.Save();

			var cfrSubstance = UNDGSubstanceLoader.LoadSubstances(Factory, "1234", standard: UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR).First();

			var undgDataItem = Factory.NewWithValidTestData<ForwardingUNDGDataItem>();
			undgDataItem.DI_DGWeight = 101;
			undgDataItem.DI_UnitOfWeight = "lb";

			undgDataItem.DI_DG = cfrSubstance.PK;
			undgDataItem.LinkDefault(cfrSubstance);

			var wrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);
			var reportableQuantityComponent = new CFRReportableQuantityComponent() as IUNDGSummaryWriterComponent;
			var result = reportableQuantityComponent.Write(wrapper);

			AssertEquals("Reportable Quantity is exceeded", "RQ", result);
		}

		public void TestReportableQuantity_WeightDoesNotExceedReportableQuantity_UnitConversion()
		{
			var substance = Factory.New<UNDGSubstanceCFR>();
			substance.CFR_UNNO = "1234";
			substance.CFR_ReportableQuantity = 100;
			substance.CFR_ReportableQuantityUnit = "lb";

			Factory.Save();

			var cfrSubstance = UNDGSubstanceLoader.LoadSubstances(Factory, "1234", standard: UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR).First();

			var undgDataItem = Factory.NewWithValidTestData<UNDGDataItem>();
			undgDataItem.DI_DGWeight = 20;
			undgDataItem.DI_UnitOfWeight = "kg";

			undgDataItem.DI_DG = cfrSubstance.PK;
			undgDataItem.LinkDefault(cfrSubstance);

			Factory.Save();

			var wrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);
			var reportableQuantityComponent = new CFRReportableQuantityComponent() as IUNDGSummaryWriterComponent;
			var result = reportableQuantityComponent.Write(wrapper);

			AssertEquals("Reportable Quantity is not exceeded", ZString.Empty, result);
		}

		public void TestReportableQuantity_WeightExceedsReportableQuantity_UnitConversion()
		{
			var substance = Factory.New<UNDGSubstanceCFR>();
			substance.CFR_UNNO = "1234";
			substance.CFR_ReportableQuantity = 100;
			substance.CFR_ReportableQuantityUnit = "lb";

			Factory.Save();

			var cfrSubstance = UNDGSubstanceLoader.LoadSubstances(Factory, "1234", standard: UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR).First();

			var undgDataItem = Factory.NewWithValidTestData<UNDGDataItem>();
			undgDataItem.DI_DGWeight = 80;
			undgDataItem.DI_UnitOfWeight = "kg";

			undgDataItem.DI_DG = cfrSubstance.PK;
			undgDataItem.LinkDefault(cfrSubstance);

			Factory.Save();

			var wrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);
			var reportableQuantityComponent = new CFRReportableQuantityComponent() as IUNDGSummaryWriterComponent;
			var result = reportableQuantityComponent.Write(wrapper);

			AssertEquals("Reportable Quantity is exceeded", "RQ", result);
		}

		public void TestReportableQuantity_WeightDoesNotExceedReportableQuantity()
		{
			var substance = Factory.New<UNDGSubstanceCFR>();
			substance.CFR_UNNO = "1234";
			substance.CFR_ReportableQuantity = 100;
			substance.CFR_ReportableQuantityUnit = "lb";

			Factory.Save();

			var cfrSubstance = UNDGSubstanceLoader.LoadSubstances(Factory, "1234", standard: UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR).First();

			var undgDataItem = Factory.NewWithValidTestData<UNDGDataItem>();
			undgDataItem.DI_DGWeight = 80;
			undgDataItem.DI_UnitOfWeight = "lb";

			undgDataItem.DI_DG = cfrSubstance.PK;
			undgDataItem.LinkDefault(cfrSubstance);

			Factory.Save();

			var wrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);
			var reportableQuantityComponent = new CFRReportableQuantityComponent() as IUNDGSummaryWriterComponent;
			var result = reportableQuantityComponent.Write(wrapper);

			AssertEquals("Reportable Quantity is not exceeded", ZString.Empty, result);
		}
	}
}
