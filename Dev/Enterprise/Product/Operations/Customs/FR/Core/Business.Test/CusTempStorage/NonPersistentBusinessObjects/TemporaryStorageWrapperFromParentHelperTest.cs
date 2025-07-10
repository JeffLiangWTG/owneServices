using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageWrapperFromParentHelper))]
	public class TemporaryStorageWrapperFromParentHelperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new TemporaryStorageWrapperFromParentHelper(Factory);

		public void TestDeltaT_Readonly()
		{
			var bizO = new TemporaryStorageWrapperFromParentHelper(Factory);
			Assert(!bizO.DeltaT_Readonly);

			var shipment = Factory.New<ForwardingShipment>();
			Factory.Save();
			bizO.ShipmentPK = shipment.PK;
			Assert(bizO.DeltaT_Readonly);

			bizO.ShipmentPK = ZGuid.Empty;
			Assert(!bizO.DeltaT_Readonly);

			var declaration = Factory.New<JobDeclaration>();
			Factory.Save();
			bizO.DeclarationPK = declaration.PK;
			Assert(bizO.DeltaT_Readonly);

			bizO.DeclarationPK = ZGuid.Empty;
			Assert(!bizO.DeltaT_Readonly);
		}

		public void TestShipment_Readonly()
		{
			var bizO = new TemporaryStorageWrapperFromParentHelper(Factory);
			Assert(!bizO.Shipment_Readonly);

			var nctHeader = Factory.New<NctsHeader>();
			nctHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			Factory.Save();
			bizO.DeltaTPK = nctHeader.PK;
			Assert(bizO.Shipment_Readonly);

			bizO.DeltaTPK = ZGuid.Empty;
			Assert(!bizO.Shipment_Readonly);

			var declaration = Factory.New<JobDeclaration>();
			Factory.Save();
			bizO.DeclarationPK = declaration.PK;
			Assert(bizO.Shipment_Readonly);

			bizO.DeclarationPK = ZGuid.Empty;
			Assert(!bizO.Shipment_Readonly);
		}

		public void TestDeclaration_Readonly()
		{
			var bizO = new TemporaryStorageWrapperFromParentHelper(Factory);
			Assert(!bizO.Declaration_Readonly);

			var nctHeader = Factory.New<NctsHeader>();
			nctHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			Factory.Save();
			bizO.DeltaTPK = nctHeader.PK;
			Assert(bizO.Declaration_Readonly);

			bizO.DeltaTPK = ZGuid.Empty;
			Assert(!bizO.Declaration_Readonly);

			var shipment = Factory.New<ForwardingShipment>();
			Factory.Save();
			bizO.ShipmentPK = shipment.PK;
			Assert(bizO.Declaration_Readonly);

			bizO.ShipmentPK = ZGuid.Empty;
			Assert(!bizO.Declaration_Readonly);
		}

		public void TestDeltaTs()
		{
			var departureHeader = Factory.NewWithValidTestData<NctsHeader>();
			departureHeader.SetMovementType(Common.EU.NctsMoveHeaderType.Codes.Departure);
			var arrivalHeader = Factory.NewWithValidTestData<NctsHeader>();
			arrivalHeader.SetMovementType(Common.EU.NctsMoveHeaderType.Codes.Arrival);
			var depatureArrivalHeader = Factory.NewWithValidTestData<NctsHeader>();
			depatureArrivalHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.DepartureAndArrival);
			Factory.Save();
			var helper = new TemporaryStorageWrapperFromParentHelper(Factory);
			helper.DeltaTs.Load();
			AssertEquals(false, helper.DeltaTs.Contains(departureHeader));
			AssertEquals(true, helper.DeltaTs.Contains(arrivalHeader));
			AssertEquals(true, helper.DeltaTs.Contains(depatureArrivalHeader));
		}
	}
}
