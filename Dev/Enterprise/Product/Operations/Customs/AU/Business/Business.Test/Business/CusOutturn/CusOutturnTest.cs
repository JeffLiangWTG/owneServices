using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.CFS.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusOutturn))]
	class CusOutturnTest : EnterpriseBusinessObjectTestCase
	{
		public void TestITransitWarehouseSyncEventParent()
		{
			var outturn = Factory.New<CusOutturn>();
			outturn.C5_CustomsStatus = "HLD";

			var eventParent = (ITransitWarehouseSyncEventParent)outturn;
			CombineAssertions(() =>
			{
				AssertEquals("CustomsStatus", "HLD", eventParent.CustomsStatus);
				AssertSame("Factory", outturn.Factory, eventParent.Factory);
			});
		}

		public void TestHumanReadableName()
		{
			var outturn = Factory.New<CusOutturn>();
			AssertEquals("Outturn Bill", outturn.HumanReadableName);
			outturn.C5_HouseBill = "123";
			AssertEquals("Outturn Bill 123", outturn.HumanReadableName);
		}

		public void TestUniversalDataContext()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_Voyage = "A12";
			oceanBill.CB_LloydsIMO = "12345";
			var container = oceanBill.Containers.AddNew();
			var underbond = container.Underbonds.AddNew();
			underbond.C4_DestinationPremiseID = "192K";
			var outturn = Factory.New<CusOutturn>();
			underbond.Outturns.Add(outturn);
			IDataContextManager manager = null;
			AssertNoExceptionThrown(() => { manager = outturn.GetUniversalDataContextManager(); });
			AssertNotNull("CusOutturn should have [UniversalDataContext(DataContextType.CusOutturn)] attribute", manager);
			AssertEquals(DataContextType.Outturn, manager.DataContextType);
			AssertEquals("", manager.DataContextKey);
		}

		public void TestStatusReadOnly()
		{
			CusOutturn outturn = Factory.New<CusOutturn>();
			AssertEquals("Customs Status should never be editable", true, outturn.C5_CustomsStatusInfo.ReadOnly);
			AssertEquals("Commercial Status should be editable", false, outturn.C5_CommercialStatusInfo.ReadOnly);
		}

		public void TestCustomsStatusCore()
		{
			AssertEquals(typeof(CargoCusStatus), Outturn.CustomsStatus.GetType());
		}

		public void TestLookups()
		{
			AssertEquals("LookupsType", typeof(CusOutturnLookups), Outturn.Lookups.GetType());
		}

		public void TestValidation()
		{
			AssertEquals("ValidationType", typeof(CusOutturnValidation), Outturn.Validation.GetType());
		}

		public void TestLinkedObjectAsCTOCusHAWB()
		{
			Outturn.Parent = CTOHAWB;
			AssertEquals("Parent", CTOHAWB, Outturn.Parent);
		}

		public void TestStatusNeedsRecalculation()
		{
			AssertEquals(false, ((Customs.Business.IStatusNeedsRecalculationProvider)Outturn).StatusNeedsRecalculation);
			EDIMessage message = Outturn.Messages.AddNew();
			message.EM_MessageText = "Cuckoo Squeaker";
			AssertEquals(true, ((Customs.Business.IStatusNeedsRecalculationProvider)Outturn).StatusNeedsRecalculation);
		}

		public void TestStatusNeedsRecalculation_LinkWithoutCollection()
		{
			AssertEquals(false, ((Customs.Business.IStatusNeedsRecalculationProvider)Outturn).StatusNeedsRecalculation);
			var message = Factory.New<CMRAIRCRMessage>();
			message.EM_LinkUniqueID = Outturn.PK;
			message.EM_MessageText = "Cuckoo Squeaker";
			AssertEquals(true, ((Customs.Business.IStatusNeedsRecalculationProvider)Outturn).StatusNeedsRecalculation);
		}

		public void TestDefaultValues()
		{
			CusOutturn newOutturn = Factory.New<CusOutturn>();
			AssertEquals("Should default to NIL", CMROutturnResultType.Codes.NilDiscrepancy, newOutturn.C5_OutturnResultType);
			AssertEquals("Should default to CMR", Customs.Business.CusOutturnApplicationCodeList.Codes.CMR, newOutturn.C5_ApplicationCode);
		}

		public void TestLinkedObjectAsCusSCADepotContainer()
		{
			CusSCADepotContainer container = Factory.New<CusSCADepotContainer>();
			Outturn.Parent = container;
			AssertEquals(container, Outturn.Parent);
		}

		public void TestLinkebObjectAsCusSCADepotHouse()
		{
			CusSCADepotHouse house = Factory.New<CusSCADepotHouse>();
			Outturn.Parent = house;
			AssertEquals(house, Outturn.Parent);
		}

		public void TestLinkedObjectAsCusHAWB()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			Outturn.Parent = hawb;
			AssertEquals("Parent", hawb, Outturn.Parent);
		}

		public void TestLinkedObjectAsCusPartShip()
		{
			var partShip = CTOHAWB.PartShips.AddNew();
			Outturn.Parent = partShip;
			AssertEquals("Parent", partShip, Outturn.Parent);
		}

		public void TestLinkedObjectAsCusMAWB()
		{
			var mawb = Factory.New<CusMAWB>();
			Outturn.Parent = mawb;
			AssertEquals("Parent", mawb, Outturn.Parent);
		}

		public void TestTestLinkedObjectAsCusSCAContainer()
		{
			Outturn.Parent = OceanBill.Containers.AddNew();
			AssertEquals("Parent", OceanBill.Containers[0], Outturn.Parent);
		}

		public void TestTestLinkedObjectAsCusSCAPivot()
		{
			Outturn.Parent = OceanBill.HouseBills.AddNew().Pivot.AddNew();
			AssertEquals("Parent", OceanBill.HouseBills[0].Pivot[0], Outturn.Parent);
		}

		public void TestLinkedObjectAsCFSContainerWrapper()
		{
			CFSContainer container = Factory.New<CFSContainer>();
			CFSContainerWrapper wrapper = CFSContainerWrapper.Load(container);
			Outturn.Parent = wrapper;
			AssertEquals("Parent", wrapper.GetType(), Outturn.Parent.GetType());
		}

		public void TestLinkedObjectAsCFSShipmentWrapper()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			CFSShipmentWrapper wrapper = CFSShipmentWrapper.Load(shipment);
			Outturn.Parent = wrapper;
			AssertEquals("Parent", wrapper.GetType(), Outturn.Parent.GetType());
		}

		public void TestCloneClearsUnderbondFK()
		{
			Outturn.C5_C4_Underbond = Factory.New(typeof(CusUnderbond)).PK;
			AssertEquals("C5_C4_Underbond", ZGuid.Empty, ((CusOutturn)Outturn.Clone()).C5_C4_Underbond);
		}

		public void TestC5_PackagesOutturned()
		{
			AssertEquals("PreCondition: C5_OutturnResultType should be NilDiscrepancy", CMROutturnResultType.Codes.NilDiscrepancy, Outturn.C5_OutturnResultType);
			Outturn.C5_OuterPacks = 0;
			Outturn.C5_PackagesOutturned = 1;
			AssertEquals("C5_OutturnResultType should not change", CMROutturnResultType.Codes.NilDiscrepancy, Outturn.C5_OutturnResultType);

			Outturn.C5_OuterPacks = 2;
			Outturn.C5_PackagesOutturned = 1;
			AssertEquals("OutturnResultType should be ShortLanded", CMROutturnResultType.Codes.ShortLanded, Outturn.C5_OutturnResultType);

			Outturn.C5_PackagesOutturned = 2;
			AssertEquals("OutturnResultType should be NilDiscrepancy", CMROutturnResultType.Codes.NilDiscrepancy, Outturn.C5_OutturnResultType);

			Outturn.C5_PackagesOutturned = 3;
			AssertEquals("OutturnResultType should be SurplusPackages", CMROutturnResultType.Codes.SurplusPackages, Outturn.C5_OutturnResultType);

			Outturn.C5_OuterPacks = 3;
			AssertEquals("OutturnResultType should be NilDiscrepancy", CMROutturnResultType.Codes.NilDiscrepancy, Outturn.C5_OutturnResultType);

			Outturn.C5_OuterPacks = 4;
			AssertEquals("OutturnResultType should be ShortLanded", CMROutturnResultType.Codes.ShortLanded, Outturn.C5_OutturnResultType);

			Outturn.C5_OuterPacks = 2;
			AssertEquals("OutturnResultType should be SurplusPackages", CMROutturnResultType.Codes.SurplusPackages, Outturn.C5_OutturnResultType);
		}

		public void TestAutoLoggedForSTLBilling()
		{
			Outturn.Factory.Save();
			AssertNotNull("Will have to log for STL billing", Outturn.Logs.AddedLog);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var mawb = factory.New<CTOCusMAWB>();
			return mawb.FakeFlightOuturnUnderbond.Outturns.AddNew();
		}

		CusSCAOceanBill oceanBill;
		CusSCAOceanBill OceanBill => oceanBill ?? (oceanBill = Factory.New<CusSCAOceanBill>());

		CTOCusHAWB ctoHAWB;
		CTOCusHAWB CTOHAWB
		{
			get
			{
				if (ctoHAWB == null)
				{
					var mawb = Factory.New<CTOCusMAWB>();
					ctoHAWB = mawb.ChildBills.AddNew();
				}
				return ctoHAWB;
			}
		}

		CusOutturn outturn;
		CusOutturn Outturn => outturn ?? (outturn = (CusOutturn)GetNewBusinessObject());
	}
}
