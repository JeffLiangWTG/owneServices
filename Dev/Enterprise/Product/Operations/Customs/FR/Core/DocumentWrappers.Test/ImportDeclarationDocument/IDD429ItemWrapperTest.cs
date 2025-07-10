using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE429;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.FR.DocumentWrappers.ImportDeclarationDocument.Testing;

class IDD429ItemWrapperTest : Enterprise.DocumentWrappers.Testing.DocBaseWrapperTest
{
	protected override DocBaseWrapper GetNewDocumentWrapper()
	{
		return IDD429ItemWrapper.New(1, goodsShipmentItem, Factory);
	}

	public void TestItemAmountInvoiced()
	{
		goodsShipmentItem.Commodity.InvoiceLine = new MInvoiceLineType();
		goodsShipmentItem.Commodity.InvoiceLine.ItemAmountInvoiced = 123;
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("ItemAmountInvoiced", "123", wrapper.ItemAmountInvoiced);
	}

	public void TestSupplementaryUnits()
	{
		goodsShipmentItem.Commodity.GoodsMeasure = new MGoodsMeasureType01FR();
		goodsShipmentItem.Commodity.GoodsMeasure.SupplementaryUnits = 10;
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("SupplementaryUnits", "10", wrapper.SupplementaryUnits);
	}

	public void TestGrossMass()
	{
		goodsShipmentItem.Commodity.GoodsMeasure = new MGoodsMeasureType01FR();
		goodsShipmentItem.Commodity.GoodsMeasure.GrossMass = 200;
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("GrossMass", "200", wrapper.GrossMass);
	}

	public void TestNetMass()
	{
		goodsShipmentItem.Commodity.GoodsMeasure = new MGoodsMeasureType01FR();
		goodsShipmentItem.Commodity.GoodsMeasure.NetMass = 150;
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("NetMass", "150", wrapper.NetMass);
	}

	public void TestCusCode()
	{
		goodsShipmentItem.Commodity.CUSCode = "CUS123";
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("CusCode", "CUS123", wrapper.CusCode);
	}

	public void TestGoodsDescription()
	{
		goodsShipmentItem.Commodity.DescriptionOfGoods = "Sample Goods Description";
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("GoodsDescription", "Sample Goods Description", wrapper.GoodsDescription);
	}

	public void TestCountryOfOrigin()
	{
		goodsShipmentItem.Origin = new MOriginType();
		goodsShipmentItem.Origin.CountryOfOrigin = "CN";
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("CountryOfOrigin", "CN", wrapper.CountryOfOrigin);
	}

	public void TestCountryOfPrefentialOrigin()
	{
		goodsShipmentItem.Origin = new MOriginType();
		goodsShipmentItem.Origin.CountryOfPreferentialOrigin = "US";
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("CountryOfPrefentialOrigin", "US", wrapper.CountryOfPrefentialOrigin);
	}

	public void TestPreference()
	{
		goodsShipmentItem.Commodity.CalculationOfTaxes.Preference = "PreferenceValue";
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("Preference", "PreferenceValue", wrapper.Preference);
	}

	public void TestStatisticalValue()
	{
		goodsShipmentItem.StatisticalValue = 1000;
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("StatisticalValue", "1000", wrapper.StatisticalValue);
	}

	public void TestExporterName()
	{
		goodsShipmentItem.Exporter = new MExporterType
		{
			Name = "Sample Exporter"
		};
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("ExporterName", "Sample Exporter", wrapper.ExporterName);
	}

	public void TestExporterEORI()
	{
		goodsShipmentItem.Exporter = new MExporterType
		{
			IdentificationNumber = "EORI123456"
		};
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("ExporterEORI", "EORI123456", wrapper.ExporterEORI);
	}

	public void TestExporterAddress()
	{
		goodsShipmentItem.Exporter = new MExporterType
		{
			Address = new MAddressType01()
			{
				StreetAndNumber = "123 Export Street"
			}
		};
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("ExporterAddress", "123 Export Street", wrapper.ExporterAddress);
	}

	public void TestExporterPostCode()
	{
		goodsShipmentItem.Exporter = new MExporterType
		{
			Address = new MAddressType01()
			{
				Postcode = "12345"
			}
		};
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("ExporterPostCode", "12345", wrapper.ExporterPostCode);
	}

	public void TestExporterCity()
	{
		goodsShipmentItem.Exporter = new MExporterType
		{
			Address = new MAddressType01()
			{
				City = "Exporter City"
			}
		};
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("ExporterCity", "Exporter City", wrapper.ExporterCity);
	}

	public void TestExporterCountry()
	{
		goodsShipmentItem.Exporter = new MExporterType
		{
			Address = new MAddressType01()
			{
				Country = "Exporter Country"
			}
		};
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("ExporterCountry", "Exporter Country", wrapper.ExporterCountry);
	}

	public void TestBuyerName()
	{
		goodsShipmentItem.Buyer = new MBuyerType
		{
			Name = "Sample Buyer"
		};
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("BuyerName", "Sample Buyer", wrapper.BuyerName);
	}

	public void TestBuyerEORI()
	{
		goodsShipmentItem.Buyer = new MBuyerType
		{
			IdentificationNumber = "EORI654321"
		};
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("BuyerEORI", "EORI654321", wrapper.BuyerEORI);
	}

	public void TestBuyerAddress()
	{
		goodsShipmentItem.Buyer = new MBuyerType
		{
			Address = new MAddressType01()
			{
				StreetAndNumber = "456 Buyer Avenue"
			}
		};
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("BuyerAddress", "456 Buyer Avenue", wrapper.BuyerAddress);
	}

	public void TestBuyerPostCode()
	{
		goodsShipmentItem.Buyer = new MBuyerType
		{
			Address = new MAddressType01()
			{
				Postcode = "54321"
			}
		};
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("BuyerPostCode", "54321", wrapper.BuyerPostCode);
	}

	public void TestBuyerCity()
	{
		goodsShipmentItem.Buyer = new MBuyerType
		{
			Address = new MAddressType01()
			{
				City = "Buyer City"
			}
		};
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("BuyerCity", "Buyer City", wrapper.BuyerCity);
	}

	public void TestBuyerCountry()
	{
		goodsShipmentItem.Buyer = new MBuyerType
		{
			Address = new MAddressType01()
			{
				Country = "Buyer Country"
			}
		};
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("BuyerCountry", "Buyer Country", wrapper.BuyerCountry);
	}

	public void TestConsigneeName()
	{
		goodsShipmentItem.Consignee = new MConsigneeType
		{
			Name = "Sample Consignee"
		};
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("ConsigneeName", "Sample Consignee", wrapper.ConsigneeName);
	}

	public void TestConsigneeEORI()
	{
		goodsShipmentItem.Consignee = new MConsigneeType
		{
			IdentificationNumber = "EORI987654"
		};
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("ConsigneeEORI", "EORI987654", wrapper.ConsigneeEORI);
	}

	public void TestConsigneeAddress()
	{
		goodsShipmentItem.Consignee = new MConsigneeType
		{
			Address = new MAddressType01()
			{
				StreetAndNumber = "789 Consignee Road"
			}
		};
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("ConsigneeAddress", "789 Consignee Road", wrapper.ConsigneeAddress);
	}

	public void TestConsigneePostCode()
	{
		goodsShipmentItem.Consignee = new MConsigneeType
		{
			Address = new MAddressType01()
			{
				Postcode = "67890"
			}
		};
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("ConsigneePostCode", "67890", wrapper.ConsigneePostCode);
	}

	public void TestConsigneeCity()
	{
		goodsShipmentItem.Consignee = new MConsigneeType
		{
			Address = new MAddressType01()
			{
				City = "Consignee City"
			}
		};
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("ConsigneeCity", "Consignee City", wrapper.ConsigneeCity);
	}

	public void TestConsigneeCountry()
	{
		goodsShipmentItem.Consignee = new MConsigneeType
		{
			Address = new MAddressType01()
			{
				Country = "Consignee Country"
			}
		};
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("ConsigneeCountry", "Consignee Country", wrapper.ConsigneeCountry);
	}

	public void TestSellerName()
	{
		goodsShipmentItem.Seller = new MSellerType
		{
			Name = "Sample Seller"
		};
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("SellerName", "Sample Seller", wrapper.SellerName);
	}

	public void TestSellerEORI()
	{
		goodsShipmentItem.Seller = new MSellerType
		{
			IdentificationNumber = "EORI123456"
		};
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("SellerEORI", "EORI123456", wrapper.SellerEORI);
	}

	public void TestSellerAddress()
	{
		goodsShipmentItem.Seller = new MSellerType
		{
			Address = new MAddressType01()
			{
				StreetAndNumber = "123 Seller Boulevard"
			}
		};
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("SellerAddress", "123 Seller Boulevard", wrapper.SellerAddress);
	}

	public void TestSellerPostCode()
	{
		goodsShipmentItem.Seller = new MSellerType
		{
			Address = new MAddressType01()
			{
				Postcode = "12345"
			}
		};
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("SellerPostCode", "12345", wrapper.SellerPostCode);
	}

	public void TestSellerCity()
	{
		goodsShipmentItem.Seller = new MSellerType
		{
			Address = new MAddressType01()
			{
				City = "Seller City"
			}
		};
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("SellerCity", "Seller City", wrapper.SellerCity);
	}

	public void TestSellerCountry()
	{
		goodsShipmentItem.Seller = new MSellerType
		{
			Address = new MAddressType01()
			{
				Country = "Seller Country"
			}
		};
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("SellerCountry", "Seller Country", wrapper.SellerCountry);
	}

	public void TestAuthorizationType()
	{
		goodsShipmentItem.Authorisation = new List<MAuthorisationType02>()
		{
			new MAuthorisationType02
			{
				Type = "Type A"
			}
		};
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("AuthorizationType", "Type A", wrapper.AuthorizationType);
	}

	public void TestAuthorizationNumber()
	{
		goodsShipmentItem.Authorisation = new List<MAuthorisationType02>
		{
			new MAuthorisationType02
			{
				ReferenceNumber = "AUTH123456"
			}
		};
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("AuthorizationNumber", "AUTH123456", wrapper.AuthorizationNumber);
	}

	public void TestAuthorizationHolder()
	{
		goodsShipmentItem.Authorisation = new List<MAuthorisationType02>
		{
			new MAuthorisationType02
			{
				HolderOfTheAuthorisation = "John Doe"
			}
		};
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("AuthorizationHolder", "John Doe", wrapper.AuthorizationHolder);
	}

	public void TestSpecialMentions()
	{
		goodsShipmentItem.AdditionalInformation = new List<MAdditionalInformationType>
		{
			new MAdditionalInformationType { Code = "Code1" },
			new MAdditionalInformationType { Code = "Code2" }
		};
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("SpecialMentions", "Code1 - Code2", wrapper.SpecialMentions);
	}

	public void TestFiscalReferences()
	{
		goodsShipmentItem.AdditionalFiscalReference = new List<MAdditionalFiscalReferenceType>
		{
			new MAdditionalFiscalReferenceType { FiscalReferenceIdentificationNumber = "FR123" },
			new MAdditionalFiscalReferenceType { FiscalReferenceIdentificationNumber = "FR456" }
		};
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("FiscalReferences", "FR123 - FR456", wrapper.FiscalReferences);
	}

	public void TestAdditionalReferences()
	{
		goodsShipmentItem.AdditionalReference = new List<MAdditionalReferenceType>
		{
			new MAdditionalReferenceType { Type = "ABC" ,ReferenceNumber = "123" },
			new MAdditionalReferenceType { Type = "DEF" ,ReferenceNumber = "456" }
		};
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("AdditionalReferences", "ABC 123 - DEF 456", wrapper.AdditionalReferences);
	}

	public void TestPreviousDocuments()
	{
		goodsShipmentItem.PreviousDocument = new List<MPreviousDocumentType03>
		{
			new MPreviousDocumentType03 { Type = "ABC" ,ReferenceNumber = "123" },
			new MPreviousDocumentType03 { Type = "DEF" ,ReferenceNumber = "456" }
		};
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("PreviousDocuments", "ABC 123 - DEF 456", wrapper.PreviousDocuments);
	}

	public void TestSupportingDocuments()
	{
		goodsShipmentItem.SupportingDocument = new List<MSupportingDocumentType01FR>
		{
			new MSupportingDocumentType01FR { Type = "ABC" ,ReferenceNumber = "123" },
			new MSupportingDocumentType01FR { Type = "DEF" ,ReferenceNumber = "456" }
		};
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("SupportingDocuments", "ABC 123 - DEF 456", wrapper.SupportingDocuments);
	}

	public void TestTransportDocuments()
	{
		goodsShipmentItem.TransportDocument = new List<MTransportDocumentType>
		{
			new MTransportDocumentType { Type = "ABC" ,ReferenceNumber = "123" },
			new MTransportDocumentType { Type = "DEF" ,ReferenceNumber = "456" }
		};
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("TransportDocuments", "ABC 123 - DEF 456", wrapper.TransportDocuments);
	}

	public void TestTaricAdditionalCode()
	{
		goodsShipmentItem.Commodity.CommodityCode = new MCommodityCodeType03();
		goodsShipmentItem.Commodity.CommodityCode.TaricAdditionalCode = new List<MTaricAdditionalCodeType>()
		{
			new MTaricAdditionalCodeType { TaricAdditionalCode = "TAC001" },
			new MTaricAdditionalCodeType { TaricAdditionalCode = "TAC002" }
		};

		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("TaricAdditionalCode", "TAC001 - TAC002", wrapper.TaricAdditionalCode);
	}

	public void TestNationalAdditionalCode()
	{
		goodsShipmentItem.Commodity.CommodityCode = new MCommodityCodeType03();
		goodsShipmentItem.Commodity.CommodityCode.NationalAdditionalCode = new List<MNationalAdditionalCodeType>
		{
			new MNationalAdditionalCodeType { NationalAdditionalCode = "NAC001" },
			new MNationalAdditionalCodeType { NationalAdditionalCode = "NAC002" }
		};

		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("NationalAdditionalCode", "NAC001 - NAC002", wrapper.NationalAdditionalCode);
	}

	public void TestAdditionsAndDeductions()
	{
		goodsShipmentItem.CustomsValuation = new MCustomsValuationType();
		goodsShipmentItem.CustomsValuation.AdditionsAndDeductions = new List<MAdditionsAndDeductionsType>()
		{
			new MAdditionsAndDeductionsType { Code = "AD001", Amount = 100, Currency = "USD" },
			new MAdditionsAndDeductionsType { Code = "AD002", Amount = 200, Currency = "EUR" }
		};

		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("AdditionsAndDeductions", "AD001 100 USD, AD002 200 EUR", wrapper.AdditionsAndDeductions);
	}

	public void TestTariffCode()
	{
		var commodityCode = new MCommodityCodeType03()
		{
			HarmonizedSystemSubheadingCode = "HS1234",
			CombinedNomenclatureCode = "CN5678",
			TaricCode = "TAR9000"
		};
		goodsShipmentItem.Commodity.CommodityCode = commodityCode;

		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("TariffCode", "HS1234CN5678TAR9000", wrapper.TariffCode);
	}

	public void TestNatureOfTransaction()
	{
		goodsShipmentItem.NatureOfTransaction = "NatureOfTransaction";

		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("NatureOfTransaction", "NatureOfTransaction", wrapper.NatureOfTransaction);
	}

	public void TestProcedure()
	{
		var procedure = new MProcedureType02()
		{
			RequestedProcedure = "Request1",
			PreviousProcedure = "Prev1",
			AdditionalProcedure = new List<MAdditionalProcedureType>()
			{
				new MAdditionalProcedureType { AdditionalProcedure = "Add1" },
				new MAdditionalProcedureType { AdditionalProcedure = "Add2" }
			}
		};
		goodsShipmentItem.Procedure = procedure;

		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("Procedure", "Request1 Prev1 Add1 Add2", wrapper.Procedure);
	}

	public void TestLiquidation()
	{
		goodsShipmentItem.DeclarationGoodsItemNumber = "123";
		var goodsShipmentItemFromDetailedTaxation = new GoodsShipmentItemType
		{
			DeclarationGoodsItemNumber = "123",
			DutiesAndTaxes = new List<DutiesAndTaxesType>()
			{
				new DutiesAndTaxesType()
				{
					TaxType = "ABC"
				}
			}
		};
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, goodsShipmentItemFromDetailedTaxation, new List<DutiesAndTaxesSummariesType>(), Factory);
		AssertEquals("Liquidation", "ABC", (wrapper.Liquidation.DutiesAndTax.FirstOrDefault() as IDD429DutiesAndTaxWrapper).TaxType);
	}

	public void TestPackaging()
	{
		goodsShipmentItem.Packaging = new List<MPackagingType01>
		{
			new MPackagingType01 { TypeOfPackages = "CBP", NumberOfPackages = "2", ShippingMarks = "Marks" },
			new MPackagingType01 { TypeOfPackages = "OTH", NumberOfPackages = "5", ShippingMarks = "Marks2" }
		};

		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);

		AssertEquals("Packaging", "CBP / OTH", wrapper.TypeOfPackage);
		AssertEquals("Packaging", "7", wrapper.NumberOfPagkage);
		AssertEquals("Packaging", "Marks / Marks2", wrapper.ShippingMarks);
	}

	public void TestPayableTaxAmount()
	{
		goodsShipmentItem.Commodity.CalculationOfTaxes.TotalDutiesAndTaxesAmount = 100d;
		var wrapper = new IDD429ItemWrapper(1, goodsShipmentItem, Factory);

		AssertEquals("PayableTaxAmount", "100", wrapper.PayableTaxAmount);
	}

	protected override void SetUp()
	{
		base.SetUp();

		goodsShipmentItem = new MGoodsShipmentItemType04FR();
		goodsShipmentItem.Commodity = new MCommodityType04FR();
		goodsShipmentItem.Commodity.CalculationOfTaxes = new MCalculationOfTaxesType01FR();
	}

	MGoodsShipmentItemType04FR goodsShipmentItem;
}
