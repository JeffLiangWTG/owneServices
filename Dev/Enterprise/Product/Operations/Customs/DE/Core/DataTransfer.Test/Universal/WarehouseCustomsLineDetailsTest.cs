using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DE.DataTransfer.Universal.Testing
{
	sealed class WarehouseCustomsLineDetailsTest : TestCaseWithFactory
	{
		public void TestGetAddInfosApplicableForWarehousing()
		{
			var lineDetails = CreateWareHouseCustomsLineDetails();
			var wareHouseCustomslineDetails = CreateWareHouseCustomsLineDetails();
			var expectedResult = "LinePrice=2.3" +
				"*LinePriceCurrency=EUR" +
				"*CountryOfSupply=FR" +
				"*ValuationCode=22" +
				"*LineNetPrice=1.2" +
				"*InvoiceNumber=INV123" +
				"*InvoiceDate=21-Jun-23" +
				"*IncotermCode=FOB" +
				"*IncotermPlace=Frankfurt" +
				"*TransNature=21" +
				"*Supplier=456;WIE;Supplier ORG;Teststraße 2;Gebaude 2;Wiesbaden;DE;65195" +
				"*Importer=123;MAI;Importer ORG;Teststraße 1;Gebaude 1;Mainz;DE;55126" +
				"*Buyer=678;KOL;Buyer ORG;Teststraße 3;Gebaude 3;Koln;DE;65293" +
				"*Seller=098;BON;Seller ORG;Teststraße 4;;Bonn;DE;63453" +
				"*PortOfLoading=DEHAM" +
				"*FirstEUArrival=DEWIB" +
				"*Transport=SEA";
			AssertEquals(expectedResult, wareHouseCustomslineDetails.AddInfos);
			AssertEquals("AdditionalAddInfos", 2, lineDetails.AdditionalAddInfos.Count());
		}

		public void TestGetAddInfosWhenCountryNull()
		{
			var wareHouseCustomslineDetails = CreateWareHouseCustomsLineDetails();
			wareHouseCustomslineDetails.SupplierAddress.Country = null;

			AssertContains("Wiesbaden;;65195", wareHouseCustomslineDetails.AddInfos);
		}

		public void TestFallbackDetail() => AssertType<WarehouseCustomsFallbackDetailWithEntryInstruction>(CreateWareHouseCustomsLineDetails().FallbackDetail);

		public void TestPreviousEntryNumberAndLineOnWarehouseAdjustment()
		{
			var fallbackDetail = CreateFallbackDetail();
			var shipment = WarehouseCustomsTestHelper.Shipment;
			shipment.DataContext.RecipientRoleCollection = new RecipientRole[] { new RecipientRole { Code = UniversalDataBuss.Integration.RecipientRoleType.BWR } };
			var invoiceLine = WarehouseCustomsTestHelper.InvoiceLine;
			invoiceLine.PreviousEntryNumber = "ENTRYNUMBER";
			invoiceLine.PreviousEntryLineNumber = 123;
			var details = new WarehouseCustomsLineDetails(Factory, invoiceLine, fallbackDetail, shipment);
			CombineAssertions(() =>
			{
				AssertEquals("ENTRYNUMBER", details.PreviousEntryNumber);
				AssertEquals((ZShort)123, details.PreviousEntryLineNumber);
			});
		}

		WarehouseCustomsLineDetails CreateWareHouseCustomsLineDetails()
		{
			var invoiceLine = new CommercialInvoiceLine()
			{
				LinePrice = 2.3,
				ValuationCode = new CodeDescriptionPair { Code = "22" },
				AddInfoCollection = new List<AddInfo>()
				{
					new AddInfo
					{
						Key = Customs.DataTransfer.Universal.Constants.AddInfoKeys.InvoiceLine.NetPrice,
						Value = "1.2"
					},
					new AddInfo
					{
						Key = "CountryOfSupply",
						Value = "FR"
					}
				},
				CustomsSupportingInformationCollection = new List<CustomsSupportingInformation>
				{
					new CustomsSupportingInformation()
					{
						Type = new CodeDescriptionPair6Char() { Code = "C014" },
						DateOfExpiry = ZDateTime.BrettsBirthday,
						ReferenceNumber = "header-support1"
				}
				}
			};

			return new WarehouseCustomsLineDetails(Factory, invoiceLine, CreateFallbackDetail(), WarehouseCustomsTestHelper.Shipment);
		}

		static WarehouseCustomsFallbackDetailWithEntryInstruction CreateFallbackDetail()
		{
			return new WarehouseCustomsFallbackDetailWithEntryInstruction
			{
				LinePriceCurrency = "EUR",
				ImporterAddress = WarehouseCustomsTestHelper.Importer,
				SupplierAddress = WarehouseCustomsTestHelper.Supplier,
				BuyerAddress = WarehouseCustomsTestHelper.Buyer,
				SellerAddress = WarehouseCustomsTestHelper.Seller,
				IncotermCode = "FOB",
				IncotermPlace = "Frankfurt",
				InvoiceDate = new ZDateTime(2023, 06, 21),
				InvoiceNumber = "INV123",
				PortOfFirstEUArrival = "DEWIB",
				PortOfLoading = "DEHAM",
				TransportMode = "SEA",
				ValuationCode = "21",
				supportingInfos = new List<CustomsSupportingInformation>
				{
					new CustomsSupportingInformation()
					{
						Type = new CodeDescriptionPair6Char() { Code = "C014" },
						DateOfExpiry = ZDateTime.BrettsBirthday,
						ReferenceNumber = "header-support1"
					}
				}
			};
		}
	}
}
