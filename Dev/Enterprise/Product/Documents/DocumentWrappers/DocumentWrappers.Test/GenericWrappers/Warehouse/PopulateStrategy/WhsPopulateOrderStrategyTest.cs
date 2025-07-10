using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	sealed class WhsPopulateOrderStrategyTest : WhsPopulateStrategyTest<WhsPopulateOrderStrategy, WhsOrder>
	{
		#region Test Properties (default strategy is an empty strategy)

		public void TestPickableDocketStrategy()
		{
			AssertNotNull("Consignee", Strategy.Consignee);
			AssertNotNull("ConsigneeAddress", Strategy.ConsigneeAddress);
			AssertNotNull("IncoTerm", Strategy.IncoTerm);
			AssertEquals("JobNumberHeading", "Order Number", Strategy.JobNumberHeading);
		}

		#endregion

		protected override WhsOrder NewWrappedBO()
		{
			if (wrappedBO == null)
			{
				wrappedBO = Factory.NewWithValidTestData<WhsOrder>();
				wrappedBO.WD_WW_Whs = Warehouse.PK;
				wrappedBO.WD_ShipperCODAmount = 100.00m;
				wrappedBO.WD_LocalCartInsuranceCost = 200.00m;
				wrappedBO.WD_CubicSent = 50m;
				wrappedBO.WD_TotalCubicUnit = "M3";
				wrappedBO.WD_WeightSent = 25m;
				wrappedBO.WD_TotalWeightUnit = "KG";
				wrappedBO.WD_PackagesSent = 5;
				wrappedBO.WD_F3_NKTotalPackType = "UNT";
				wrappedBO.WD_PalletsSent = 6;
				wrappedBO.WD_UnitsSent = 7.5m;
				wrappedBO.WD_TransportReference = "TransRef#";
				wrappedBO.WD_INCO = "FOB";
				wrappedBO.WD_ExternalReference = "ExtRef#";
				wrappedBO.WD_CustomerReference = "CusRef#";
				wrappedBO.WD_RequiredDate = new ZDateTimeOffset(2010, 6, 25);
				wrappedBO.WD_BOLNo = "BOL123";
				wrappedBO.WD_WhsOrderFulfillmentRule = "NON";
				wrappedBO.WD_PickOption = "AUT";
				wrappedBO.WD_DropMode = "HSL";
				wrappedBO.WD_CODPayMethod = "CHQ";
				wrappedBO.WD_ShipperCODAmount = 123.45m;
				wrappedBO.WD_LocalCartInsuranceCost = 22.22m;

				WhsDocketReference reference = wrappedBO.References.AddNew();
				reference.WX_Reference = "VHN123";
				reference.WX_RefType = "VHN";

				OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
				OrgAddress orgaddress = consignee.Addresses.AddNew();
				orgaddress.OA_Address1 = "Add1-1";
			}
			return wrappedBO;
		}
	}
}
