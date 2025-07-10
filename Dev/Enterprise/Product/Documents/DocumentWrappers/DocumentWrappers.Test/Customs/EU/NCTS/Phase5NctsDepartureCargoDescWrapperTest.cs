using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.EU.NCTS.Testing
{
	[TestedType(typeof(Phase5NctsDepartureCargoDescWrapper))]
	sealed class Phase5NctsDepartureCargoDescWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestWrapperProperties()
		{
			CombineAssertions(() =>
			{
				var (header, wrapper) = SetUpData();
				AssertEquals(nameof(wrapper.BOX12_08UCR), "UCR001", wrapper.BOX12_08UCR);
				AssertEquals(nameof(wrapper.BOX13_02CONSIGNOREORI), "IT0007", wrapper.BOX13_02CONSIGNOREORI);
				AssertEquals(nameof(wrapper.BOX13_02CONSIGNORNAME), "BOB", wrapper.BOX13_02CONSIGNORNAME);
				AssertEquals(nameof(wrapper.BOX13_02CONSIGNORADDRESS), "STREET\r\nCITY\r\n123321\r\nIT", wrapper.BOX13_02CONSIGNORADDRESS);
				AssertEquals(nameof(wrapper.BOX13_02CONSIGNORCONTACTNAME), "TOM B", wrapper.BOX13_02CONSIGNORCONTACTNAME);
				AssertEquals(nameof(wrapper.BOX13_02CONSIGNORCONTACTPHONE), "011234567", wrapper.BOX13_02CONSIGNORCONTACTPHONE);
				AssertEquals(nameof(wrapper.BOX13_02CONSIGNORCONTACTEMAIL), "tom@test.com", wrapper.BOX13_02CONSIGNORCONTACTEMAIL);
				AssertEquals(nameof(wrapper.BOX13_03CONSIGNEEEORI), "TR987654", wrapper.BOX13_03CONSIGNEEEORI);
				AssertEquals(nameof(wrapper.BOX13_03CONSIGNEENAME), "PHILIP", wrapper.BOX13_03CONSIGNEENAME);
				AssertEquals(nameof(wrapper.BOX13_03CONSIGNEEADDRESS), "EE STREET\r\nCITY\r\n456654\r\nTR", wrapper.BOX13_03CONSIGNEEADDRESS);
				AssertEquals(nameof(wrapper.BOX13_14SUPPLYCHAINACTORROLE), "TS1\r\nTS2", wrapper.BOX13_14SUPPLYCHAINACTORROLE);
				AssertEquals(nameof(wrapper.BOX13_14SUPPLYCHAINACTORID), "REF1\r\nREF2", wrapper.BOX13_14SUPPLYCHAINACTORID);
				AssertEquals(nameof(wrapper.BOX16_06COUNTRYOFDISPATCH), "IT", wrapper.BOX16_06COUNTRYOFDISPATCH);
				AssertEquals(nameof(wrapper.BOX18_04GROSSMASS), "1515.75", wrapper.BOX18_04GROSSMASS);
				AssertEquals(nameof(wrapper.BOX19_05DEPTRANSPORTMEANSTYPEOFID), "3", wrapper.BOX19_05DEPTRANSPORTMEANSTYPEOFID);
				AssertEquals(nameof(wrapper.BOX19_05DEPTRANSPORTMEANSID), "DEP TRANSPORT", wrapper.BOX19_05DEPTRANSPORTMEANSID);
				AssertEquals(nameof(wrapper.BOX12_01PREVIOUSDOCUMENTTYPE), "C651", wrapper.BOX12_01PREVIOUSDOCUMENTTYPE);
				AssertEquals(nameof(wrapper.BOX12_01PREVIOUSDOCUMENTREFERENCENUMBER), "PRV001", wrapper.BOX12_01PREVIOUSDOCUMENTREFERENCENUMBER);
				AssertEquals(nameof(wrapper.BOX12_01PREVIOUSDOCUMENTCOMPLEMENTOFINFORMATION), "Sample Text", wrapper.BOX12_01PREVIOUSDOCUMENTCOMPLEMENTOFINFORMATION);
				AssertEquals(nameof(wrapper.BOX12_03SUPPORTINGDOCUMENTTYPE), "ABC1\r\nABC2", wrapper.BOX12_03SUPPORTINGDOCUMENTTYPE);
				AssertEquals(nameof(wrapper.BOX12_03SUPPORTINGDOCUMENTREFERENCENUMBER), "SUP01\r\nSUP02", wrapper.BOX12_03SUPPORTINGDOCUMENTREFERENCENUMBER);
				AssertEquals(nameof(wrapper.BOX12_03SUPPORTINGDOCUMENTLINEITEMNUMBER), "1\r\n2", wrapper.BOX12_03SUPPORTINGDOCUMENTLINEITEMNUMBER);
				AssertEquals(nameof(wrapper.BOX12_03SUPPORTINGDOCUMENTCOMPLEMENTOFINFORMATION), "Supporting document 1\r\nSupporting document 2", wrapper.BOX12_03SUPPORTINGDOCUMENTCOMPLEMENTOFINFORMATION);
				AssertEquals(nameof(wrapper.BOX12_05TRANSPORTDOCUMENTTYPE), "111", wrapper.BOX12_05TRANSPORTDOCUMENTTYPE);
				AssertEquals(nameof(wrapper.BOX12_05TRANSPORTDOCUMENTREFERENCENUMBER), "222", wrapper.BOX12_05TRANSPORTDOCUMENTREFERENCENUMBER);
				AssertEquals(nameof(wrapper.BOX12_04ADDITIONALREFERENCETYPE), "REF1", wrapper.BOX12_04ADDITIONALREFERENCETYPE);
				AssertEquals(nameof(wrapper.BOX12_04ADDITIONALREFERENCEREFERENCENUMBER), "Additional Reference 1", wrapper.BOX12_04ADDITIONALREFERENCEREFERENCENUMBER);
				AssertEquals(nameof(wrapper.BOX12_02ADDITIONALINFORMATIONCODE), "INF1", wrapper.BOX12_02ADDITIONALINFORMATIONCODE);
				AssertEquals(nameof(wrapper.BOX12_02ADDITIONALINFORMATIONTEXT), "INF123", wrapper.BOX12_02ADDITIONALINFORMATIONTEXT);
				AssertEquals(nameof(wrapper.BOX14_02TRANSPORTCHARGES), "C", wrapper.BOX14_02TRANSPORTCHARGES);
				AssertEquals(nameof(wrapper.BOX11_03CONSIGNMENTITEMGOODSITEMNUMBER), "1", wrapper.BOX11_03CONSIGNMENTITEMGOODSITEMNUMBER);
				AssertEquals(nameof(wrapper.BOX11_11CONSIGNMENTITEMDECLARATIONGOODSITEMNUMBER), "1", wrapper.BOX11_11CONSIGNMENTITEMDECLARATIONGOODSITEMNUMBER);
				AssertEquals(nameof(wrapper.BOX11_01CONSIGNMENTITEMDECLARATIONTYPE), "Z", wrapper.BOX11_01CONSIGNMENTITEMDECLARATIONTYPE);
				AssertEquals(nameof(wrapper.BOX16_06CONSIGNMENTITEMCOUNTRYOFDISPATCH), "GB", wrapper.BOX16_06CONSIGNMENTITEMCOUNTRYOFDISPATCH);
				AssertEquals(nameof(wrapper.BOX16_03CONSIGNMENTITEMCOUNTRYOFDESTINATION), "ZA", wrapper.BOX16_03CONSIGNMENTITEMCOUNTRYOFDESTINATION);
				AssertEquals(nameof(wrapper.BOX12_08CONSIGNMENTITEMREFERENCENUMBERUCR), "22222", wrapper.BOX12_08CONSIGNMENTITEMREFERENCENUMBERUCR);
				AssertEquals(nameof(wrapper.BOX13_03CONSIGNMENTITEMCONSIGNEEEORI), "TR987654", wrapper.BOX13_03CONSIGNMENTITEMCONSIGNEEEORI);
				AssertEquals(nameof(wrapper.BOX13_03CONSIGNMENTITEMCONSIGNEENAME), "PHILIP", wrapper.BOX13_03CONSIGNMENTITEMCONSIGNEENAME);
				AssertEquals(nameof(wrapper.BOX13_03CONSIGNMENTITEMCONSIGNEEADDRESS), "EE STREET\r\n456654\r\nCITY\r\nTR", wrapper.BOX13_03CONSIGNMENTITEMCONSIGNEEADDRESS);
				AssertEquals(nameof(wrapper.BOX13_14ADDITIONALSUPPLYCHAINACTORROLE), "TS1\r\nTS2", wrapper.BOX13_14ADDITIONALSUPPLYCHAINACTORROLE);
				AssertEquals(nameof(wrapper.BOX13_14ADDITIONALSUPPLYCHAINACTORIDENTIFICATIONNUMBER), "REF1\r\nREF2", wrapper.BOX13_14ADDITIONALSUPPLYCHAINACTORIDENTIFICATIONNUMBER);
				AssertEquals(nameof(wrapper.BOX18_05COMMODITYDESCRIPTIONOFGOODS), "TEST ITEM", wrapper.BOX18_05COMMODITYDESCRIPTIONOFGOODS);
				AssertEquals(nameof(wrapper.BOX18_08COMMODITYCUSCODE), "7", wrapper.BOX18_08COMMODITYCUSCODE);
				AssertEquals(nameof(wrapper.BOX18_09COMMODITYCODEHARMONIZEDSYSTEMSUBHEADING), "1234.56", wrapper.BOX18_09COMMODITYCODEHARMONIZEDSYSTEMSUBHEADING);
				AssertEquals(nameof(wrapper.BOX18_09COMMODITYCODECOMBINEDNOMENCLATURECODE), "78", wrapper.BOX18_09COMMODITYCODECOMBINEDNOMENCLATURECODE);
				AssertEquals(nameof(wrapper.BOX18_09COMMODITYCODE), "1234.56.78", wrapper.BOX18_09COMMODITYCODE);
				AssertEquals(nameof(wrapper.BOX18_07DANGEROUSGOODSUNNUMBER), dangerousGoodsGUID.ToString(), wrapper.BOX18_07DANGEROUSGOODSUNNUMBER);
				AssertEquals(nameof(wrapper.BOX18_04GOODSMEASUREGROSSMASS), "34.567000", wrapper.BOX18_04GOODSMEASUREGROSSMASS);
				AssertEquals(nameof(wrapper.BOX18_01GOODSMEASURENETTMASS), "40.000000", wrapper.BOX18_01GOODSMEASURENETTMASS);
				AssertEquals(nameof(wrapper.BOX18_02GOODSMEASURESUPPLEMENTARYUNITS), "2.123457", wrapper.BOX18_02GOODSMEASURESUPPLEMENTARYUNITS);
				AssertEquals(nameof(wrapper.BOX18_06PACKAGINGTYPE), "CN", wrapper.BOX18_06PACKAGINGTYPE);
				AssertEquals(nameof(wrapper.BOX18_06PACKAGINGNUMBEROFPACKAGES), "4", wrapper.BOX18_06PACKAGINGNUMBEROFPACKAGES);
				AssertEquals(nameof(wrapper.BOX18_06PACKAGINGSHIPPINGMARKS), "ABC123", wrapper.BOX18_06PACKAGINGSHIPPINGMARKS);
				AssertEquals(nameof(wrapper.BOX12_01GOODSITEMPREVIOUSDOCUMENTTYPE), "PR9", wrapper.BOX12_01GOODSITEMPREVIOUSDOCUMENTTYPE);
				AssertEquals(nameof(wrapper.BOX12_01GOODSITEMPREVIOUSDOCUMENTREFERENCENUMBER), "55555", wrapper.BOX12_01GOODSITEMPREVIOUSDOCUMENTREFERENCENUMBER);
				AssertEquals(nameof(wrapper.BOX12_01GOODSITEMPREVIOUSDOCUMENTGOODSITEMNUMBER), "1", wrapper.BOX12_01GOODSITEMPREVIOUSDOCUMENTGOODSITEMNUMBER);
				AssertEquals(nameof(wrapper.BOX12_01GOODSITEMPREVIOUSDOCUMENTTYPEOFPACKAGES), "BX", wrapper.BOX12_01GOODSITEMPREVIOUSDOCUMENTTYPEOFPACKAGES);
				AssertEquals(nameof(wrapper.BOX12_01GOODSITEMPREVIOUSDOCUMENTNUMBEROFPACKAGES), "6", wrapper.BOX12_01GOODSITEMPREVIOUSDOCUMENTNUMBEROFPACKAGES);
				AssertEquals(nameof(wrapper.BOX12_01GOODSITEMPREVIOUSDOCUMENTMEASUREMENTUNIT), "EA", wrapper.BOX12_01GOODSITEMPREVIOUSDOCUMENTMEASUREMENTUNIT);
				AssertEquals(nameof(wrapper.BOX12_01GOODSITEMPREVIOUSDOCUMENTQUANTITY), "4.560000", wrapper.BOX12_01GOODSITEMPREVIOUSDOCUMENTQUANTITY);
				AssertEquals(nameof(wrapper.BOX12_01GOODSITEMPREVIOUSDOCUMENTCOMPLEMENTOFINFORMATION), "TEXT", wrapper.BOX12_01GOODSITEMPREVIOUSDOCUMENTCOMPLEMENTOFINFORMATION);
				AssertEquals(nameof(wrapper.BOX12_03GOODSITEMSUPPORTINGDOCUMENTTYPE), "N380", wrapper.BOX12_03GOODSITEMSUPPORTINGDOCUMENTTYPE);
				AssertEquals(nameof(wrapper.BOX12_03GOODSITEMSUPPORTINGDOCUMENTREFERENCENUMBER), "2468", wrapper.BOX12_03GOODSITEMSUPPORTINGDOCUMENTREFERENCENUMBER);
				AssertEquals(nameof(wrapper.BOX12_03GOODSITEMSUPPORTINGDOCUMENTLINEITEMNUMBER), "9", wrapper.BOX12_03GOODSITEMSUPPORTINGDOCUMENTLINEITEMNUMBER);
				AssertEquals(nameof(wrapper.BOX12_03GOODSITEMSUPPORTINGDOCUMENTCOMPLEMENTOFINFORMATION), "SUPPORT", wrapper.BOX12_03GOODSITEMSUPPORTINGDOCUMENTCOMPLEMENTOFINFORMATION);
				AssertEquals(nameof(wrapper.BOX12_05_GOODSITEMTRANSPORTDOCUMENTTYPE), "22", wrapper.BOX12_05_GOODSITEMTRANSPORTDOCUMENTTYPE);
				AssertEquals(nameof(wrapper.BOX12_05_GOODSITEMTRANSPORTDOCUMENTREFERENCENUMBER), "987654", wrapper.BOX12_05_GOODSITEMTRANSPORTDOCUMENTREFERENCENUMBER);
				AssertEquals(nameof(wrapper.BOX12_04GOODSITEMADDITIONALREFERENCETYPE), "33", wrapper.BOX12_04GOODSITEMADDITIONALREFERENCETYPE);
				AssertEquals(nameof(wrapper.BOX12_04GOODSITEMADDITIONALREFERENCEREFERENCENUMBER), "54321", wrapper.BOX12_04GOODSITEMADDITIONALREFERENCEREFERENCENUMBER);
				AssertEquals(nameof(wrapper.BOX12_02GOODSITEMADDITIONALINFORMATIONCODE), "44", wrapper.BOX12_02GOODSITEMADDITIONALINFORMATIONCODE);
				AssertEquals(nameof(wrapper.BOX12_02GOODSITEMADDITIONALINFORMATIONTEXT), "242424", wrapper.BOX12_02GOODSITEMADDITIONALINFORMATIONTEXT);
			});
		}

		public void TestWrapperPropertiesWhenSourceIsEmptyObject()
		{
			var wrapper = (Phase5NctsDepartureCargoDescWrapper)GetNewBusinessObject();

			CombineAssertions("Test properties when Source is empty", () =>
			{
				AssertEquals(nameof(wrapper.BOX12_08UCR), string.Empty, wrapper.BOX12_08UCR);
				AssertEquals(nameof(wrapper.BOX13_02CONSIGNOREORI), string.Empty, wrapper.BOX13_02CONSIGNOREORI);
				AssertEquals(nameof(wrapper.BOX13_02CONSIGNORNAME), string.Empty, wrapper.BOX13_02CONSIGNORNAME);
				AssertEquals(nameof(wrapper.BOX13_02CONSIGNORADDRESS), string.Empty, wrapper.BOX13_02CONSIGNORADDRESS);
				AssertEquals(nameof(wrapper.BOX13_02CONSIGNORCONTACTNAME), string.Empty, wrapper.BOX13_02CONSIGNORCONTACTNAME);
				AssertEquals(nameof(wrapper.BOX13_02CONSIGNORCONTACTPHONE), string.Empty, wrapper.BOX13_02CONSIGNORCONTACTPHONE);
				AssertEquals(nameof(wrapper.BOX13_02CONSIGNORCONTACTEMAIL), string.Empty, wrapper.BOX13_02CONSIGNORCONTACTEMAIL);
				AssertEquals(nameof(wrapper.BOX13_03CONSIGNEEEORI), string.Empty, wrapper.BOX13_03CONSIGNEEEORI);
				AssertEquals(nameof(wrapper.BOX13_03CONSIGNEENAME), string.Empty, wrapper.BOX13_03CONSIGNEENAME);
				AssertEquals(nameof(wrapper.BOX13_03CONSIGNEEADDRESS), string.Empty, wrapper.BOX13_03CONSIGNEEADDRESS);
				AssertEquals(nameof(wrapper.BOX13_14SUPPLYCHAINACTORROLE), string.Empty, wrapper.BOX13_14SUPPLYCHAINACTORROLE);
				AssertEquals(nameof(wrapper.BOX13_14SUPPLYCHAINACTORID), string.Empty, wrapper.BOX13_14SUPPLYCHAINACTORID);
				AssertEquals(nameof(wrapper.BOX16_06COUNTRYOFDISPATCH), string.Empty, wrapper.BOX16_06COUNTRYOFDISPATCH);
				AssertEquals(nameof(wrapper.BOX18_04GROSSMASS), "0", wrapper.BOX18_04GROSSMASS);
				AssertEquals(nameof(wrapper.BOX19_05DEPTRANSPORTMEANSTYPEOFID), string.Empty, wrapper.BOX19_05DEPTRANSPORTMEANSTYPEOFID);
				AssertEquals(nameof(wrapper.BOX19_05DEPTRANSPORTMEANSID), string.Empty, wrapper.BOX19_05DEPTRANSPORTMEANSID);
				AssertEquals(nameof(wrapper.BOX12_01PREVIOUSDOCUMENTTYPE), "", wrapper.BOX12_01PREVIOUSDOCUMENTTYPE);
				AssertEquals(nameof(wrapper.BOX12_01PREVIOUSDOCUMENTREFERENCENUMBER), "", wrapper.BOX12_01PREVIOUSDOCUMENTREFERENCENUMBER);
				AssertEquals(nameof(wrapper.BOX12_01PREVIOUSDOCUMENTCOMPLEMENTOFINFORMATION), "", wrapper.BOX12_01PREVIOUSDOCUMENTCOMPLEMENTOFINFORMATION);
				AssertEquals(nameof(wrapper.BOX12_03SUPPORTINGDOCUMENTTYPE), "", wrapper.BOX12_03SUPPORTINGDOCUMENTTYPE);
				AssertEquals(nameof(wrapper.BOX12_03SUPPORTINGDOCUMENTREFERENCENUMBER), "", wrapper.BOX12_03SUPPORTINGDOCUMENTREFERENCENUMBER);
				AssertEquals(nameof(wrapper.BOX12_03SUPPORTINGDOCUMENTLINEITEMNUMBER), "", wrapper.BOX12_03SUPPORTINGDOCUMENTLINEITEMNUMBER);
				AssertEquals(nameof(wrapper.BOX12_03SUPPORTINGDOCUMENTCOMPLEMENTOFINFORMATION), "", wrapper.BOX12_03SUPPORTINGDOCUMENTCOMPLEMENTOFINFORMATION);
				AssertEquals(nameof(wrapper.BOX12_05TRANSPORTDOCUMENTTYPE), "", wrapper.BOX12_05TRANSPORTDOCUMENTTYPE);
				AssertEquals(nameof(wrapper.BOX12_05TRANSPORTDOCUMENTREFERENCENUMBER), "", wrapper.BOX12_05TRANSPORTDOCUMENTREFERENCENUMBER);
				AssertEquals(nameof(wrapper.BOX12_04ADDITIONALREFERENCETYPE), "", wrapper.BOX12_04ADDITIONALREFERENCETYPE);
				AssertEquals(nameof(wrapper.BOX12_04ADDITIONALREFERENCEREFERENCENUMBER), "", wrapper.BOX12_04ADDITIONALREFERENCEREFERENCENUMBER);
				AssertEquals(nameof(wrapper.BOX12_02ADDITIONALINFORMATIONCODE), "", wrapper.BOX12_02ADDITIONALINFORMATIONCODE);
				AssertEquals(nameof(wrapper.BOX12_02ADDITIONALINFORMATIONTEXT), "", wrapper.BOX12_02ADDITIONALINFORMATIONTEXT);
				AssertEquals(nameof(wrapper.BOX14_02TRANSPORTCHARGES), "", wrapper.BOX14_02TRANSPORTCHARGES);
				AssertEquals(nameof(wrapper.BOX11_03CONSIGNMENTITEMGOODSITEMNUMBER), "1", wrapper.BOX11_03CONSIGNMENTITEMGOODSITEMNUMBER);
				AssertEquals(nameof(wrapper.BOX11_11CONSIGNMENTITEMDECLARATIONGOODSITEMNUMBER), "0", wrapper.BOX11_11CONSIGNMENTITEMDECLARATIONGOODSITEMNUMBER);
				AssertEquals(nameof(wrapper.BOX11_01CONSIGNMENTITEMDECLARATIONTYPE), "", wrapper.BOX11_01CONSIGNMENTITEMDECLARATIONTYPE);
				AssertEquals(nameof(wrapper.BOX16_06CONSIGNMENTITEMCOUNTRYOFDISPATCH), "", wrapper.BOX16_06CONSIGNMENTITEMCOUNTRYOFDISPATCH);
				AssertEquals(nameof(wrapper.BOX16_03CONSIGNMENTITEMCOUNTRYOFDESTINATION), "", wrapper.BOX16_03CONSIGNMENTITEMCOUNTRYOFDESTINATION);
				AssertEquals(nameof(wrapper.BOX12_08CONSIGNMENTITEMREFERENCENUMBERUCR), "", wrapper.BOX12_08CONSIGNMENTITEMREFERENCENUMBERUCR);
				AssertEquals(nameof(wrapper.BOX13_03CONSIGNMENTITEMCONSIGNEEEORI), "", wrapper.BOX13_03CONSIGNMENTITEMCONSIGNEEEORI);
				AssertEquals(nameof(wrapper.BOX13_03CONSIGNMENTITEMCONSIGNEENAME), "", wrapper.BOX13_03CONSIGNMENTITEMCONSIGNEENAME);
				AssertEquals(nameof(wrapper.BOX13_03CONSIGNMENTITEMCONSIGNEEADDRESS), "", wrapper.BOX13_03CONSIGNMENTITEMCONSIGNEEADDRESS);
				AssertEquals(nameof(wrapper.BOX13_14ADDITIONALSUPPLYCHAINACTORROLE), "", wrapper.BOX13_14ADDITIONALSUPPLYCHAINACTORROLE);
				AssertEquals(nameof(wrapper.BOX13_14ADDITIONALSUPPLYCHAINACTORIDENTIFICATIONNUMBER), "", wrapper.BOX13_14ADDITIONALSUPPLYCHAINACTORIDENTIFICATIONNUMBER);
				AssertEquals(nameof(wrapper.BOX18_05COMMODITYDESCRIPTIONOFGOODS), "", wrapper.BOX18_05COMMODITYDESCRIPTIONOFGOODS);
				AssertEquals(nameof(wrapper.BOX18_08COMMODITYCUSCODE), "", wrapper.BOX18_08COMMODITYCUSCODE);
				AssertEquals(nameof(wrapper.BOX18_09COMMODITYCODEHARMONIZEDSYSTEMSUBHEADING), "", wrapper.BOX18_09COMMODITYCODEHARMONIZEDSYSTEMSUBHEADING);
				AssertEquals(nameof(wrapper.BOX18_09COMMODITYCODECOMBINEDNOMENCLATURECODE), "", wrapper.BOX18_09COMMODITYCODECOMBINEDNOMENCLATURECODE);
				AssertEquals(nameof(wrapper.BOX18_09COMMODITYCODE), "", wrapper.BOX18_09COMMODITYCODE);
				AssertEquals(nameof(wrapper.BOX18_07DANGEROUSGOODSUNNUMBER), "", wrapper.BOX18_07DANGEROUSGOODSUNNUMBER);
				AssertEquals(nameof(wrapper.BOX18_04GOODSMEASUREGROSSMASS), "0.000000", wrapper.BOX18_04GOODSMEASUREGROSSMASS);
				AssertEquals(nameof(wrapper.BOX18_01GOODSMEASURENETTMASS), "0.000000", wrapper.BOX18_01GOODSMEASURENETTMASS);
				AssertEquals(nameof(wrapper.BOX18_02GOODSMEASURESUPPLEMENTARYUNITS), "0.000000", wrapper.BOX18_02GOODSMEASURESUPPLEMENTARYUNITS);
				AssertEquals(nameof(wrapper.BOX18_06PACKAGINGTYPE), "", wrapper.BOX18_06PACKAGINGTYPE);
				AssertEquals(nameof(wrapper.BOX18_06PACKAGINGNUMBEROFPACKAGES), "", wrapper.BOX18_06PACKAGINGNUMBEROFPACKAGES);
				AssertEquals(nameof(wrapper.BOX18_06PACKAGINGSHIPPINGMARKS), "", wrapper.BOX18_06PACKAGINGSHIPPINGMARKS);
				AssertEquals(nameof(wrapper.BOX12_01GOODSITEMPREVIOUSDOCUMENTTYPE), "", wrapper.BOX12_01GOODSITEMPREVIOUSDOCUMENTTYPE);
				AssertEquals(nameof(wrapper.BOX12_01GOODSITEMPREVIOUSDOCUMENTREFERENCENUMBER), "", wrapper.BOX12_01GOODSITEMPREVIOUSDOCUMENTREFERENCENUMBER);
				AssertEquals(nameof(wrapper.BOX12_01GOODSITEMPREVIOUSDOCUMENTGOODSITEMNUMBER), "", wrapper.BOX12_01GOODSITEMPREVIOUSDOCUMENTGOODSITEMNUMBER);
				AssertEquals(nameof(wrapper.BOX12_01GOODSITEMPREVIOUSDOCUMENTTYPEOFPACKAGES), "", wrapper.BOX12_01GOODSITEMPREVIOUSDOCUMENTTYPEOFPACKAGES);
				AssertEquals(nameof(wrapper.BOX12_01GOODSITEMPREVIOUSDOCUMENTNUMBEROFPACKAGES), "", wrapper.BOX12_01GOODSITEMPREVIOUSDOCUMENTNUMBEROFPACKAGES);
				AssertEquals(nameof(wrapper.BOX12_01GOODSITEMPREVIOUSDOCUMENTMEASUREMENTUNIT), "", wrapper.BOX12_01GOODSITEMPREVIOUSDOCUMENTMEASUREMENTUNIT);
				AssertEquals(nameof(wrapper.BOX12_01GOODSITEMPREVIOUSDOCUMENTQUANTITY), "", wrapper.BOX12_01GOODSITEMPREVIOUSDOCUMENTQUANTITY);
				AssertEquals(nameof(wrapper.BOX12_01GOODSITEMPREVIOUSDOCUMENTCOMPLEMENTOFINFORMATION), "", wrapper.BOX12_01GOODSITEMPREVIOUSDOCUMENTCOMPLEMENTOFINFORMATION);
				AssertEquals(nameof(wrapper.BOX12_03GOODSITEMSUPPORTINGDOCUMENTTYPE), "", wrapper.BOX12_03GOODSITEMSUPPORTINGDOCUMENTTYPE);
				AssertEquals(nameof(wrapper.BOX12_03GOODSITEMSUPPORTINGDOCUMENTREFERENCENUMBER), "", wrapper.BOX12_03GOODSITEMSUPPORTINGDOCUMENTREFERENCENUMBER);
				AssertEquals(nameof(wrapper.BOX12_03GOODSITEMSUPPORTINGDOCUMENTLINEITEMNUMBER), "", wrapper.BOX12_03GOODSITEMSUPPORTINGDOCUMENTLINEITEMNUMBER);
				AssertEquals(nameof(wrapper.BOX12_03GOODSITEMSUPPORTINGDOCUMENTCOMPLEMENTOFINFORMATION), "", wrapper.BOX12_03GOODSITEMSUPPORTINGDOCUMENTCOMPLEMENTOFINFORMATION);
				AssertEquals(nameof(wrapper.BOX12_05_GOODSITEMTRANSPORTDOCUMENTTYPE), "", wrapper.BOX12_05_GOODSITEMTRANSPORTDOCUMENTTYPE);
				AssertEquals(nameof(wrapper.BOX12_05_GOODSITEMTRANSPORTDOCUMENTREFERENCENUMBER), "", wrapper.BOX12_05_GOODSITEMTRANSPORTDOCUMENTREFERENCENUMBER);
				AssertEquals(nameof(wrapper.BOX12_04GOODSITEMADDITIONALREFERENCETYPE), "", wrapper.BOX12_04GOODSITEMADDITIONALREFERENCETYPE);
				AssertEquals(nameof(wrapper.BOX12_04GOODSITEMADDITIONALREFERENCEREFERENCENUMBER), "", wrapper.BOX12_04GOODSITEMADDITIONALREFERENCEREFERENCENUMBER);
				AssertEquals(nameof(wrapper.BOX12_02GOODSITEMADDITIONALINFORMATIONCODE), "", wrapper.BOX12_02GOODSITEMADDITIONALINFORMATIONCODE);
				AssertEquals(nameof(wrapper.BOX12_02GOODSITEMADDITIONALINFORMATIONTEXT), "", wrapper.BOX12_02GOODSITEMADDITIONALINFORMATIONTEXT);
			});
		}

		public void TestCommodityCode()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var item = header.Bills.AddNew().GoodsItems.AddNew();

			item.BY_FormattedHarmonisedTariff = "1234.5";
			var wrapper = Phase5NctsDepartureCargoDescWrapper.New(item, Factory);
			AssertEquals("BOX18_09COMMODITYCODEHARMONIZEDSYSTEMSUBHEADING when commodity code is 5 digits", "1234.5", wrapper.BOX18_09COMMODITYCODEHARMONIZEDSYSTEMSUBHEADING);
			AssertEquals("BOX18_09COMMODITYCODECOMBINEDNOMENCLATURECODE when commodity code is 5 digits", ZString.Empty, wrapper.BOX18_09COMMODITYCODECOMBINEDNOMENCLATURECODE);
			AssertEquals("BOX18_09COMMODITYCODE when commodity code is 5 digits", "1234.5", wrapper.BOX18_09COMMODITYCODE);

			item.BY_FormattedHarmonisedTariff = "1234.56";
			wrapper = Phase5NctsDepartureCargoDescWrapper.New(item, Factory);
			AssertEquals("BOX18_09COMMODITYCODEHARMONIZEDSYSTEMSUBHEADING when commodity code is 6 digits", "1234.56", wrapper.BOX18_09COMMODITYCODEHARMONIZEDSYSTEMSUBHEADING);
			AssertEquals("BOX18_09COMMODITYCODECOMBINEDNOMENCLATURECODE when commodity code is 6 digits", ZString.Empty, wrapper.BOX18_09COMMODITYCODECOMBINEDNOMENCLATURECODE);
			AssertEquals("BOX18_09COMMODITYCODE when commodity code is 6 digits", "1234.56", wrapper.BOX18_09COMMODITYCODE);

			item.BY_FormattedHarmonisedTariff = "1234.56.78";
			wrapper = Phase5NctsDepartureCargoDescWrapper.New(item, Factory);
			AssertEquals("BOX18_09COMMODITYCODEHARMONIZEDSYSTEMSUBHEADING when commodity code is 8 digits", "1234.56", wrapper.BOX18_09COMMODITYCODEHARMONIZEDSYSTEMSUBHEADING);
			AssertEquals("BOX18_09COMMODITYCODECOMBINEDNOMENCLATURECODE when commodity code is 8 digits", "78", wrapper.BOX18_09COMMODITYCODECOMBINEDNOMENCLATURECODE);
			AssertEquals("BOX18_09COMMODITYCODE when commodity code is 8 digits", "1234.56.78", wrapper.BOX18_09COMMODITYCODE);

			item.BY_FormattedHarmonisedTariff = "1234.56.78 00";
			wrapper = Phase5NctsDepartureCargoDescWrapper.New(item, Factory);
			AssertEquals("BOX18_09COMMODITYCODEHARMONIZEDSYSTEMSUBHEADING when commodity code is 10 digits", "1234.56", wrapper.BOX18_09COMMODITYCODEHARMONIZEDSYSTEMSUBHEADING);
			AssertEquals("BOX18_09COMMODITYCODECOMBINEDNOMENCLATURECODE when commodity code is 10 digits", "78", wrapper.BOX18_09COMMODITYCODECOMBINEDNOMENCLATURECODE);
			AssertEquals("BOX18_09COMMODITYCODE when commodity code is 10 digits", "1234.56.78", wrapper.BOX18_09COMMODITYCODE);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var item = header.Bills.AddNew().GoodsItems.AddNew();
			return Phase5NctsDepartureCargoDescWrapper.New(item, Factory);
		}

		(NctsHeader header, Phase5NctsDepartureCargoDescWrapper wrapper) SetUpData()
		{
			dangerousGoodsGUID = ZGuid.NewZGuid();
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			var movementHeader = header.MovementHeader;
			movementHeader.TransportTypeAtDeparture = "3";
			movementHeader.TransportAtDeparture = "DEP TRANSPORT";

			var bill1 = header.Bills.AddNew();
			bill1.B0_RN_NKCountryOfExport = Core.Constants.CountryCodes.Italy;
			bill1.B0_Weight = 1515.75m;
			bill1.B0_ReferenceID = "UCR001";
			bill1.B0_TransportPaymentMethod = "C";
			SetUpPreviousDocumentsForTest(bill1);
			SetUpSupportingDocsForTest(bill1);
			SetUpTransportDocForTest(bill1);
			SetUpAdditionalReferenceForTest(bill1);
			SetUpAdditionalInfoForTest(bill1);
			SetUpConsignorForTest(bill1);
			SetUpConsigneeForTest(bill1);
			SetUpAdditionalSupplyChainActorsForTest(bill1);
			var item1 = bill1.GoodsItems.AddNew();
			item1.BY_LineNo = 1;
			item1.BY_DeclarationGoodsItemNumber = 1;
			item1.BY_Type = "Z";
			item1.BY_RN_NKCountryOfDispatch = "GB";
			item1.BY_RN_NKCountryOfDestination = "ZA";
			item1.BY_CommercialReferenceNumber = "22222";
			item1.BY_Description = "TEST ITEM";
			item1.BY_CusC4Number = "7";
			item1.BY_FormattedHarmonisedTariff = "1234.56.78 90";
			item1.BY_GrossWeight = 34.567m;
			item1.BY_NetWeight = 40;
			item1.BY_CustomsSecondQuantity = 2.123456789m;
			var pack = item1.Packages.AddNew();
			pack.B5_UnitType = "CN";
			pack.B5_UnitCount = 4;
			pack.B5_MarksAndNumbers = "ABC123";
			item1.UNDGs.AddNew().DI_DG = dangerousGoodsGUID;
			SetUpConsigneeForTest(item1);
			SetUpGoodsItemPreviousDocumentForTest(item1);
			SetUpGoodsItemSupportingDocumentForTest(item1);
			SetUpGoodsItemTransportDocumentForTest(item1);
			SetUpGoodsItemAdditionalReferenceForTest(item1);
			SetUpGoodsItemAdditionalInformationForTest(item1);
			return (header, Phase5NctsDepartureCargoDescWrapper.New(item1, Factory));
		}

		void SetUpConsignorForTest(NctsBill bill)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.MainAddress;
			orgHeader.OH_FullName = "BOB";
			address.OA_Address1 = "STREET";
			address.OA_PostCode = "123321";
			address.OA_City = "CITY";
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
			bill.Consignor.E2_OA_Address = address.PK;
			var eori = orgHeader.CustomsCodes.AddNew();
			eori.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			eori.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Italy;
			eori.OK_CustomsRegNo = "0007";
			var contact = Factory.New<OrgContact>();
			contact.OC_ContactName = "TOM B";
			contact.OC_Phone = "011234567";
			contact.OC_Email = "tom@test.com";
			bill.Consignor.ContactPK = contact.PK;
		}

		void SetUpConsigneeForTest(NctsBill bill)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.MainAddress;
			orgHeader.OH_FullName = "PHILIP";
			address.OA_Address1 = "EE STREET";
			address.OA_PostCode = "456654";
			address.OA_City = "CITY";
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Turkey;
			bill.Consignee.E2_OA_Address = address.PK;
			var eori = orgHeader.CustomsCodes.AddNew();
			eori.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			eori.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Turkey;
			eori.OK_CustomsRegNo = "987654";
		}

		void SetUpAdditionalSupplyChainActorsForTest(NctsBill bill)
		{
			var asca1 = bill.CusSupplyChainActorReferences.AddNew();
			asca1.CFR_Code = "TS1";
			asca1.CFR_Reference = "REF1";
			var asca2 = bill.CusSupplyChainActorReferences.AddNew();
			asca2.CFR_Code = "TS2";
			asca2.CFR_Reference = "REF2";
		}

		void SetUpPreviousDocumentsForTest(NctsBill bill)
		{
			var doc = bill.PreviousDocuments.AddNew();
			doc.CSI_Code = "C651";
			doc.CSI_ReferenceNumber = "PRV001";
			doc.CSI_ReferenceNumber2 = "Sample Text";
		}

		void SetUpSupportingDocsForTest(NctsBill bill)
		{
			var doc1 = bill.SupportingDocuments.AddNew();
			doc1.CSI_Code = "ABC1";
			doc1.CSI_ReferenceNumber = "SUP01";
			doc1.CSI_ItemNumber = 1;
			doc1.CSI_ReferenceNumber2 = "Supporting document 1";

			var doc2 = bill.SupportingDocuments.AddNew();
			doc2.CSI_Code = "ABC2";
			doc2.CSI_ReferenceNumber = "SUP02";
			doc2.CSI_ItemNumber = 2;
			doc2.CSI_ReferenceNumber2 = "Supporting document 2";
		}

		void SetUpTransportDocForTest(NctsBill bill)
		{
			var doc = bill.AdditionalDocuments.AddNew();
			doc.CSI_SubType = "TRA";
			doc.CSI_Code = "111";
			doc.CSI_ReferenceNumber = "222";
		}

		void SetUpAdditionalReferenceForTest(NctsBill bill)
		{
			var doc = bill.AdditionalDocuments.AddNew();
			doc.CSI_SubType = "REF";
			doc.CSI_Code = "REF1";
			doc.CSI_ReferenceNumber = "Additional Reference 1";
		}

		void SetUpAdditionalInfoForTest(NctsBill bill)
		{
			var doc = bill.AdditionalDocuments.AddNew();
			doc.CSI_SubType = "INF";
			doc.CSI_Code = "INF1";
			doc.CSI_Description = "INF123";
		}

		void SetUpGoodsItemPreviousDocumentForTest(NctsDepartureCargoDesc item)
		{
			var doc = item.PreviousDocuments.AddNew();
			doc.CSI_Code = "PR9";
			doc.CSI_ReferenceNumber = "55555";
			doc.CSI_ItemNumber = 1;
			doc.CSI_UnitOfQuantity2 = "BX";
			doc.CSI_Quantity2 = 6.0;
			doc.CSI_UnitOfQuantity = "EA";
			doc.CSI_Quantity = 4.56;
			doc.CSI_ReferenceNumber2 = "TEXT";
		}

		void SetUpGoodsItemSupportingDocumentForTest(NctsDepartureCargoDesc item)
		{
			var doc = item.SupportingDocuments.AddNew();
			doc.CSI_Code = "N380";
			doc.CSI_ReferenceNumber = "2468";
			doc.CSI_ItemNumber = 9;
			doc.CSI_ReferenceNumber2 = "SUPPORT";
		}

		void SetUpGoodsItemTransportDocumentForTest(NctsDepartureCargoDesc item)
		{
			var doc = item.AdditionalInfos.AddNew();
			doc.CSI_SubType = "TRA";
			doc.CSI_Code = "22";
			doc.CSI_ReferenceNumber = "987654";
		}

		void SetUpGoodsItemAdditionalReferenceForTest(NctsDepartureCargoDesc item)
		{
			var doc = item.AdditionalInfos.AddNew();
			doc.CSI_SubType = "REF";
			doc.CSI_Code = "33";
			doc.CSI_ReferenceNumber = "54321";
		}

		void SetUpGoodsItemAdditionalInformationForTest(NctsDepartureCargoDesc item)
		{
			var doc = item.AdditionalInfos.AddNew();
			doc.CSI_SubType = "INF";
			doc.CSI_Code = "44";
			doc.CSI_Description = "242424";
		}

		void SetUpConsigneeForTest(NctsDepartureCargoDesc item)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.MainAddress;
			orgHeader.OH_FullName = "PHILIP";
			address.OA_Address1 = "EE STREET";
			address.OA_PostCode = "456654";
			address.OA_City = "CITY";
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Turkey;
			item.Consignee.E2_OA_Address = address.PK;
			var eori = orgHeader.CustomsCodes.AddNew();
			eori.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			eori.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Turkey;
			eori.OK_CustomsRegNo = "987654";
		}

		ZGuid dangerousGoodsGUID;
	}
}
