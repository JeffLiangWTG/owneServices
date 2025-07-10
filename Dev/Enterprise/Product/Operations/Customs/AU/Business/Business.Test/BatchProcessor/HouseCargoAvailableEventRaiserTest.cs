using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.LogWalker.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class HouseCargoAvailableEventRaiserTest<T> : LogSubscriberTest<HouseCargoAvailableEventRaiser>
			where T : BusinessObject, ICargoDepotEventParent
	{
		public void TestCVDCreatedIf_CLR_And_CAD()
		{
			var parent = CreateParent();
			parent.CargoReceivedAtDepotLogs.AddNew();
			((BusinessObject)parent)[StatusSchemaColumn] = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;

			Factory.Save();
			RunLogWalkerCycleForTest();
			parent = new BusinessObjectFactory().Load<T>(parent.PK);
			AssertEquals("1 CVD event created", 1, parent.CargoAvailableAtDepotLogs.Count);
		}

		public void TestCVDNotCreatedIfNoCAD()
		{
			var parent = CreateParent();
			((BusinessObject)parent)[StatusSchemaColumn] = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;

			Factory.Save();
			RunLogWalkerCycleForTest();
			parent = new BusinessObjectFactory().Load<T>(parent.PK);
			AssertEquals("CVD event not created", 0, parent.CargoAvailableAtDepotLogs.Count);
		}

		public void TestCVDCancelledIfNoCAD()
		{
			var parent = CreateParent();
			((BusinessObject)parent)[StatusSchemaColumn] = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;

			var cdvEvent = parent.CargoAvailableAtDepotLogs.AddNew();

			Factory.Save();
			AssertEquals("CVD event exists", 1, parent.CargoAvailableAtDepotLogs.Count);
			RunLogWalkerCycleForTest();
			parent = new BusinessObjectFactory().Load<T>(parent.PK);
			AssertEquals("CVD event was cancelled", 0, parent.CargoAvailableAtDepotLogs.Count);
		}

		public void TestCVDNotCreatedIfNotCLR()
		{
			var parent = CreateParent();
			parent.CargoReceivedAtDepotLogs.AddNew();
			((BusinessObject)parent)[StatusSchemaColumn] = CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation;

			Factory.Save();
			RunLogWalkerCycleForTest();
			parent = new BusinessObjectFactory().Load<T>(parent.PK);
			AssertEquals("CVD event not created", 0, parent.CargoAvailableAtDepotLogs.Count);
		}

		public void TestCVDCancelledIfNotCLR()
		{
			var parent = CreateParent();
			parent.CargoReceivedAtDepotLogs.AddNew();
			parent.CargoAvailableAtDepotLogs.AddNew();
			Factory.Save();
			AssertEquals("CVD event exists", 1, parent.CargoAvailableAtDepotLogs.Count);
			RunLogWalkerCycleForTest();
			parent = new BusinessObjectFactory().Load<T>(parent.PK);
			AssertEquals("CVD event was cancelled", 0, parent.CargoAvailableAtDepotLogs.Count);
		}

		T CreateParent()
		{
			var result = CreateParentCore();
			Factory.Save();
			return result;
		}

		protected abstract T CreateParentCore();

		protected abstract SchemaColumn StatusSchemaColumn { get; }
	}
}
