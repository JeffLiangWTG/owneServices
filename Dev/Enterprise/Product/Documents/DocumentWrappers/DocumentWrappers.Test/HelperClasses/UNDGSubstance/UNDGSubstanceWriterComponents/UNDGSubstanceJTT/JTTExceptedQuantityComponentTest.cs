using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class JTTExceptedQuantityComponentTest : TestCaseWithFactory
	{
		public void TestDGInExceptedQuantities()
		{
			var substance = Factory.New<UNDGSubstanceJTT>();
			substance.JTT_UNNO = "9999";
			substance.JTT_PSN = "English name";
			substance.JTT_ExceptedQuantityCode = "E1";

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var undgDataItem = shipment.OuterPackLines.AddNew().UNDGs.AddNew();
			undgDataItem.DI_DG = substance.PK;
			undgDataItem.LinkDefault(substance);

			undgDataItem.DI_DGWeight = 0.5;
			undgDataItem.DI_UnitOfWeight = "KG";
			undgDataItem.DI_PackageCount = 5;
			undgDataItem.DI_F3_NKPackType = "BOX";

			var wrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);
			var component = new JTTExceptedQuantityComponent() as IUNDGSummaryWriterComponent;
			var result = component.Write(wrapper);

			AssertEquals("Is in excepted quantities", "DANGEROUS GOODS IN EXCEPTED QUANTITIES: 5 BOX", result);
		}

		public void TestDGInExceptedQuantities_WeightExceeds()
		{
			var substance = Factory.New<UNDGSubstanceJTT>();
			substance.JTT_UNNO = "9999";
			substance.JTT_PSN = "English name";
			substance.JTT_ExceptedQuantityCode = "E1";

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var undgDataItem = shipment.OuterPackLines.AddNew().UNDGs.AddNew();
			undgDataItem.DI_DG = substance.PK;
			undgDataItem.LinkDefault(substance);
			undgDataItem.DI_DGWeight = 4;
			undgDataItem.DI_UnitOfWeight = "KG";
			undgDataItem.DI_DGVolume = 0.5;
			undgDataItem.DI_UnitOfVolume = "L";
			undgDataItem.DI_PackageCount = 5;
			undgDataItem.DI_F3_NKPackType = "BOX";

			var wrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);
			var component = new JTTExceptedQuantityComponent() as IUNDGSummaryWriterComponent;
			var result = component.Write(wrapper);

			AssertEquals("Is not in excepted quantities", ZString.Empty, result);
		}

		public void TestDGInExceptedQuantities_VolumeExceeds()
		{
			var substance = Factory.New<UNDGSubstanceJTT>();
			substance.JTT_UNNO = "9999";
			substance.JTT_PSN = "English name";
			substance.JTT_ExceptedQuantityCode = "E1";

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var undgDataItem = shipment.OuterPackLines.AddNew().UNDGs.AddNew();
			undgDataItem.DI_DG = substance.PK;
			undgDataItem.LinkDefault(substance);
			undgDataItem.DI_DGWeight = 0.5;
			undgDataItem.DI_UnitOfWeight = "KG";
			undgDataItem.DI_DGVolume = 4;
			undgDataItem.DI_UnitOfVolume = "L";
			undgDataItem.DI_PackageCount = 5;
			undgDataItem.DI_F3_NKPackType = "BOX";

			var wrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);
			var component = new JTTExceptedQuantityComponent() as IUNDGSummaryWriterComponent;
			var result = component.Write(wrapper);

			AssertEquals("Is not in excepted quantities", ZString.Empty, result);
		}
	}
}
