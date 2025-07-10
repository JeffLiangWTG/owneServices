using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(OrgSupplierPartCollection))]
	class OrgSupplierPartCollectionTest : Customs.Business.Testing.OrgSupplierPartCollectionTest
	{
		public void TestCopyValuesFromInvoiceLine()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCustomsTariff("1234567890", "00000", "00423", "00352", "99999");
			Factory.Save();
			var manufacturer1 = Factory.New<MasterFiles.Business.OrgHeader>();
			manufacturer1.OH_Code = "M1";
			var manufacturerAddr1 = manufacturer1.Addresses.AddNew();
			manufacturerAddr1.OA_Address1 = "M1 Addr1";
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1234567890";
			void FillInvoiceLineValues()
			{
				invoiceLine.JI_CountryOfOrigin = "CN";
				invoiceLine.JI_RN_NKCountryOfExport = "US";
				invoiceLine.JI_StateOrRegionOfOrigin = "CNX";
				invoiceLine.JI_CIQOriginState = "200001";
				invoiceLine.JI_OriginDistrict = "10001";
				invoiceLine.JI_OriginRegion = "20001";
				invoiceLine.JI_DestinationDistrict = "10002";
				invoiceLine.JI_DestinationRegion = "20002";
				invoiceLine.JI_CIQTariff = "100000010";
				var invoiceParent = new CodeDescriptionOptionCollectionParent(Factory, invoiceLine.CargoAttributes);
				invoiceParent.OptionCollection.SelectedCodes = new List<ZString> { "18", "23", "25", "30" };
				invoiceParent.OptionCollection.RefreshSelectionCollection();
				invoiceLine.JI_OA_ManufacturerAddress = manufacturerAddr1.PK;
				invoiceLine.JI_NDescription = "SPEC";
				invoiceLine.JI_Model = "MODEL";
				invoiceLine.JI_BrandName = "BRAND";
				invoiceLine.JI_CIQEndUse = "23";
				invoiceLine.CIQIngredient = "IngredientNote";
				invoiceLine.JI_PackageTypeOfUNDG = UNDGPackageTypeList.Codes._1B1;
				invoiceLine.JI_NonDangerousChemicalFlag = true;
				invoiceLine.JI_CIQQualityGuaranteePeriod = 99;
				invoiceLine.JI_TradeUnitQty = "001";
			}

			declaration.JE_MessageType = "IMP";
			declaration.JE_MessageSubType = "CUS";
			FillInvoiceLineValues();
			var collection = new OrgSupplierPartCollection(Factory, invoiceLine, invoiceLine.InvoiceHeader?.IsExport ?? ZBool.False);
			var pivot = collection.AddNew().PivotsForBinding[0];
			AssertEquals("Should have been copied from Invoice Line", "CN", pivot.CI_RN_NKCountryOfOrigin);
			AssertEquals("Should have been copied from Invoice Line", "US", pivot.CI_RN_NKCountryOfExport);
			AssertEquals("Should have been copied from Invoice Line", "CNX", pivot.CI_RW_NKOriginState);
			AssertEquals("Should have been copied from Invoice Line", "200001", pivot.CNC_OriginState);
			AssertEquals("Should NOT be copied from Invoice Line", ZString.Empty, pivot.CNC_OriginDistrict);
			AssertEquals("Should NOT be copied from Invoice Line", ZString.Empty, pivot.CNC_OriginRegion);
			AssertEquals("Should have been copied from Invoice Line", "10002", pivot.CNC_DestinationDistrict);
			AssertEquals("Should have been copied from Invoice Line", "20002", pivot.CNC_DestinationRegion);
			AssertEquals("Should have been copied from Invoice Line", "100000010", pivot.CNC_CIQTariff);
			AssertSequencesEqual("Empty value should not be copied from Pivot", new[] { "18", "23", "25", "30" }, invoiceLine.CargoAttributes.Cast<CargoAttribute>().Select(code => code.CY_Code.ToString()));
			AssertEquals("Should have been copied from Invoice Line", manufacturerAddr1.PK, pivot.CNC_OA_ManufacturerAddress);
			AssertEquals("Should have been copied from Invoice Line", "SPEC", pivot.CI_NDescription);
			AssertEquals("Should have been copied from Invoice Line", "MODEL", pivot.CNC_Model);
			AssertEquals("Should have been copied from Invoice Line", "BRAND", pivot.CNC_Brand);
			AssertEquals("Should have been copied from Invoice Line", "23", pivot.CNC_EndUse);
			AssertEquals("Should have been copied from Invoice Line", "IngredientNote", pivot.CIQIngredient);
			AssertEquals("Should have been copied from Invoice Line", UNDGPackageTypeList.Codes._1B1, pivot.CNC_UNPackageMarking);
			AssertEquals("Should have been copied from Invoice Line", true, pivot.CNC_NonDangerousChemicalFlag);
			AssertEquals("Should have been copied from Invoice Line", 99, pivot.CNC_QualityGuaranteePeriod);
			AssertEquals("Should have been copied from Invoice Line", "001", pivot.CNC_TradeUnitQty);
			declaration.JE_MessageType = "EXP";
			FillInvoiceLineValues();
			var pivot2 = collection.AddNew().PivotsForBinding[0];
			AssertEquals("Should NOT be copied from Invoice Line", ZString.Empty, pivot2.CI_RW_NKOriginState);
			AssertEquals("Should NOT be copied from Invoice Line", ZString.Empty, pivot2.CNC_OriginState);
			AssertEquals("Should NOT be copied from Invoice Line", ZString.Empty, pivot2.CNC_DestinationDistrict);
			AssertEquals("Should NOT be copied from Invoice Line", ZString.Empty, pivot2.CNC_DestinationRegion);
			declaration.JE_MessageSubType = "BTH";
			FillInvoiceLineValues();
			var pivot3 = collection.AddNew().PivotsForBinding[0];
			AssertEquals("Should have been copied from Invoice Line", "CN", pivot3.CI_RN_NKCountryOfOrigin);
			AssertEquals("Should have been copied from Invoice Line", "US", pivot3.CI_RN_NKCountryOfExport);
			AssertEquals("Should have been copied from Invoice Line", "CNX", pivot3.CI_RW_NKOriginState);
			AssertEquals("Should have been copied from Invoice Line", "200001", pivot3.CNC_OriginState);
			AssertEquals("Should have been copied from Invoice Line", "10001", pivot3.CNC_OriginDistrict);
			AssertEquals("Should have been copied from Invoice Line", "20001", pivot3.CNC_OriginRegion);
			AssertEquals("Should have been copied from Invoice Line", "10002", pivot3.CNC_DestinationDistrict);
			AssertEquals("Should have been copied from Invoice Line", "20002", pivot3.CNC_DestinationRegion);
			invoiceLine.TradeUnitPrice = 1.23m;
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = "CNY";
			using (CNCustomsDataRegistry.Instance.DefaultTradeUnitPriceOnProduct.SetTemporaryValue(new Guid(), new Guid(), new Guid(), true))
			{
				var pivot4 = collection.AddNew().PivotsForBinding[0];
				AssertEquals("DefaultTradeUnitPriceOnProduct true, should copy CNC_TradeUnitPrice", 1.23m, pivot4.CNC_TradeUnitPrice);
				AssertEquals("DefaultTradeUnitPriceOnProduct true, should copy CNC_RX_NKTradeUnitPriceCurrency", "CNY", pivot4.CNC_RX_NKTradeUnitPriceCurrency);
			}

			using (CNCustomsDataRegistry.Instance.DefaultTradeUnitPriceOnProduct.SetTemporaryValue(new Guid(), new Guid(), new Guid(), false))
			{
				var pivot5 = collection.AddNew().PivotsForBinding[0];
				AssertEquals("DefaultTradeUnitPriceOnProduct true, should copy CNC_TradeUnitPrice", 0m, pivot5.CNC_TradeUnitPrice);
				AssertEquals("DefaultTradeUnitPriceOnProduct true, should copy CNC_RX_NKTradeUnitPriceCurrency", "", pivot5.CNC_RX_NKTradeUnitPriceCurrency);
			}
		}

		public void TestCopyAdditionalInformationsFromInvoiceLine()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCustomsTariff("1234567890", "00000", "00423", "00352", "99999");
			helper.CreateAdditionalElement("00000", "品名");
			helper.CreateAdditionalElement("00423", "针入度");
			helper.CreateAdditionalElement("00352", "加工方法");
			helper.CreateAdditionalElement("00010", "包装规格");
			helper.CreateAdditionalElement("00020", "制作或保存方法");
			helper.CreateAdditionalElement("99999", "其他");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1234567890";
			invoiceLine.JI_NameOfGoods = "111";
			invoiceLine.XC_GoodsSpecModel = "333|444";
			var collection = new OrgSupplierPartCollection(Factory, invoiceLine, invoiceLine.InvoiceHeader?.IsExport ?? ZBool.False);
			var pivot = collection.AddNew().PivotsForBinding[0];
			AssertEquals("Set Tariff Code for new pivot", "1234567890", pivot.CI_TariffNum);
			AssertEquals(3, pivot.AdditionalInformationCodes.Count);
			AssertEquals("111", pivot.AdditionalInformationCodes["00000"].CY_Data);
			AssertEquals("333", pivot.AdditionalInformationCodes["00423"].CY_Data);
			AssertEquals("444", pivot.AdditionalInformationCodes["00352"].CY_Data);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			pivot = collection.AddNew().PivotsForBinding[0];
			AssertEquals("Set Tariff Code for new pivot", "1234567890", pivot.CI_TariffNum);
		}

		public override void TestAdditionalAddNewByOrgHeader()
		{
			Assert("It just test in Base class for create new product", true);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new OrgSupplierPartCollection(Factory);
	}
}
