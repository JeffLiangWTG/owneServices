using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class BillPartyProviderTest : DataProviderTestCase<BillPartyProvider>
	{
		public void TestId()
		{
			SetUpTestData();

			AssertEquals("Consignee default", DefaultConsigneeId, consigneeProvider.Id);
			bill.ABL_ConsigneeRegNo = "123456";
			CombineAssertions("When ConsigneeRegNo is not empty", () =>
			{
				TestConsigneeImporterIdWhenRegNoType(ImporterIdentificationTypes.Codes.EOR, "123456");
				TestConsigneeImporterIdWhenRegNoType(ImporterIdentificationTypes.Codes.CGT, "CGT123456");
				TestConsigneeImporterIdWhenRegNoType(ImporterIdentificationTypes.Codes.ITX, "ITX123456");
				TestConsigneeImporterIdWhenRegNoType(ImporterIdentificationTypes.Codes.PYE, "PYE123456");
				TestConsigneeImporterIdWhenRegNoType(string.Empty, "NR");
			});

			bill.ABL_ConsigneeRegNo = ZString.Empty;
			CombineAssertions("When ConsigneeRegNo is empty", () =>
			{
				TestConsigneeImporterIdWhenRegNoType(ImporterIdentificationTypes.Codes.EOR, DefaultConsigneeId);
				TestConsigneeImporterIdWhenRegNoType(ImporterIdentificationTypes.Codes.CGT, DefaultConsigneeId);
				TestConsigneeImporterIdWhenRegNoType(ImporterIdentificationTypes.Codes.ITX, DefaultConsigneeId);
				TestConsigneeImporterIdWhenRegNoType(ImporterIdentificationTypes.Codes.PYE, DefaultConsigneeId);
				TestConsigneeImporterIdWhenRegNoType(string.Empty, DefaultConsigneeId);
			});

			void TestConsigneeImporterIdWhenRegNoType(string type, string expectedID)
			{
				bill.ABL_ConsigneeRegNoType = type;
				AssertEquals(type, expectedID, consigneeProvider.Id);
			}

			CombineAssertions("Shipper provider", () =>
			{
				AssertEquals("Shipper default", DefaultShipperId, shipperProvider.Id);

				bill.ABL_ShipperRegNo = "9876";
				AssertEquals("When shipper org address is default, this should still be default", DefaultShipperId, shipperProvider.Id);

				bill.ABL_OA_Shipper = Factory.New<OrgAddress>().PK;
				bill.ABL_ShipperRegNo = "9876";
				AssertEquals("9876", shipperProvider.Id);
			});
		}

		public void TestId_DefaultForConsignee_DependsOnNameOrAddressDetailsEntered()
		{
			SetUpTestData(consigneeName: "");

			AssertNull("When No Name or Address details entered, Id is null", consigneeProvider.Id);

			var propertiesToCheck = new[]
			{
				AsycudaBillSchema.Constants.ABL_ConsigneeName,
				AsycudaBillSchema.Constants.ABL_ConsigneeStreet1,
				AsycudaBillSchema.Constants.ABL_ConsigneeStreet2,
				AsycudaBillSchema.Constants.ABL_ConsigneeCity,
				AsycudaBillSchema.Constants.ABL_ConsigneePostcode,
				AsycudaBillSchema.Constants.ABL_RN_NKConsigneeCountry,
			};

			CombineAssertions("If any single name or address is entered, Id defaults to NR", () =>
			{
				foreach (var property in propertiesToCheck)
				{
					bill.SetPropertyValue(property, (ZString)"AU");
					AssertEquals(property, DefaultConsigneeId, consigneeProvider.Id);
					bill.SetPropertyValue(property, ZString.Empty);
				}
			});
		}

		public void TestName_WhenIdIsDefault()
		{
			SetUpTestData();

			AssertEquals("Precondition: Consignee Id is default", DefaultConsigneeId, consigneeProvider.Id);
			AssertEquals("Precondition: Shipper Id is default", DefaultShipperId, shipperProvider.Id);

			AssertEquals("ConsigneeName", "ConsigneeName", consigneeProvider.Name);
			AssertEquals("ShipperName", "ShipperName", shipperProvider.Name);
		}

		public void TestName_WhenIdIsNotDefault()
		{
			SetUpTestData();
			bill.ABL_ConsigneeRegNo = "123456";
			bill.ABL_ConsigneeRegNoType = ImporterIdentificationTypes.Codes.EOR;

			AssertNotEquals("Precondition: Consignee Id is Not default", DefaultConsigneeId, consigneeProvider.Id);

			AssertNull("ConsigneeName", consigneeProvider.Name);

			bill.ABL_OA_Shipper = Factory.New<OrgAddress>().PK;
			bill.ABL_ShipperRegNo = "9876";
			AssertNotEquals("Precondition: Shipper Id is Not default", DefaultShipperId, shipperProvider.Id);
			AssertNull("ShipperName", shipperProvider.Name);
		}

		public void TestAddress_WhenIdIsDefault()
		{
			SetUpTestData();

			AssertEquals("Precondition: Consignee Id is default", DefaultConsigneeId, consigneeProvider.Id);
			AssertEquals("Precondition: Shipper Id is default", DefaultShipperId, shipperProvider.Id);

			AssertNotNull("ConsigneeAddress", consigneeProvider.Address);
			AssertNotNull("ShipperAddress", shipperProvider.Address);
		}

		public void TestAddress_WhenIdIsNotDefault()
		{
			SetUpTestData();

			bill.ABL_ConsigneeRegNo = "123456";
			bill.ABL_ConsigneeRegNoType = ImporterIdentificationTypes.Codes.EOR;

			AssertNotEquals("Precondition: Consignee Id is Not default", DefaultConsigneeId, consigneeProvider.Id);

			AssertNull("ConsigneeAddress", consigneeProvider.Address);

			bill.ABL_OA_Shipper = Factory.New<OrgAddress>().PK;
			bill.ABL_ShipperRegNo = "9876";
			AssertNotEquals("Precondition: Shipper Id is Not default", DefaultShipperId, shipperProvider.Id);
			AssertNull("ShipperAddress", shipperProvider.Address);
		}

		public void TestContact()
		{
			SetUpTestData();
			AssertNull(consigneeProvider.Contact);
			AssertNull(shipperProvider.Contact);
		}

		void SetUpTestData(string consigneeName = "ConsigneeName", string shipperName = "ShipperName")
		{
			header = Factory.New<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
			bill.ABL_ConsigneeName = consigneeName;
			bill.ABL_ShipperName = shipperName;
			consigneeProvider = GenerateProvider(bill, AsycudaBillAddress.AddressType.Consignee);
			shipperProvider = GenerateProvider(bill, AsycudaBillAddress.AddressType.Shipper);
		}

		AsycudaManifestHeader header;
		AsycudaBill bill;
		BillPartyProvider consigneeProvider;
		BillPartyProvider shipperProvider;
		const string DefaultConsigneeId = "NR";
		const string DefaultShipperId = "";

		BillPartyProvider GenerateProvider(AsycudaBill bill, AsycudaBillAddress.AddressType addressType) => new BillPartyProvider(bill, addressType);

		protected sealed override BillPartyProvider GetProvider()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return new BillPartyProvider(bill, AsycudaBillAddress.AddressType.Consignee);
		}
	}
}
