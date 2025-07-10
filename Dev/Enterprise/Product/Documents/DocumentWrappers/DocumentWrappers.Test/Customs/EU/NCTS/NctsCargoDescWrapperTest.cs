using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Customs.EU.NCTS.Testing
{
	sealed class NctsCargoDescWrapperTest : TestCaseWithFactory
	{
		public void TestBOX313CONTAINERS()
		{
			var header = Factory.New<NctsHeader>();
			var bill = header.Bills.AddNew();
			var goodsItem1 = bill.GoodsItems.AddNew();
			var goodsItem2 = bill.GoodsItems.AddNew();
			var goodsItem3 = bill.GoodsItems.AddNew();

			var container1 = header.DepartureHeaderContainers.AddNew();
			container1.BC_Mode = Core.Constants.ContainerModes.Containerised;
			container1.BC_ContainerNum = "MERU1234567";
			var container2 = header.DepartureHeaderContainers.AddNew();
			container2.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
			container2.BC_ContainerNum = "EQUIPMENT01";
			var container3 = header.DepartureHeaderContainers.AddNew();
			container3.BC_Mode = Core.Constants.ContainerModes.Containerised;
			container3.BC_ContainerNum = "CNT2";
			var container4 = header.DepartureHeaderContainers.AddNew();
			container4.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
			container4.BC_ContainerNum = "NCT2";
			var container5 = header.DepartureHeaderContainers.AddNew();
			container5.BC_Mode = Core.Constants.ContainerModes.Containerised;
			container5.BC_ContainerNum = "CNT3";

			var package1 = goodsItem1.Packages.AddNew();
			package1.B5_MarksAndNumbers = "m1";
			package1.B5_UnitCount = 1;
			package1.B5_UnitType = "3A";
			var package2 = goodsItem2.Packages.AddNew();
			package2.B5_MarksAndNumbers = "M2";
			package2.B5_UnitCount = 2;
			package2.B5_UnitType = "1B";
			var package3 = goodsItem3.Packages.AddNew();
			package3.B5_MarksAndNumbers = "m3";
			package3.B5_UnitCount = 3;
			package3.B5_UnitType = "1F";

			foreach (NonPersistentContainerPivotPhase5 pivot in package1.ContainersPivotsForBindingOnly)
			{
				pivot.ContainerSelected = true;
			}

			var pivot21 = package2.ContainersPivotsForBindingOnly[0];
			pivot21.ContainerSelected = true;
			var pivot22 = package2.ContainersPivotsForBindingOnly[2];
			pivot22.ContainerSelected = true;
			var pivot23 = package2.ContainersPivotsForBindingOnly[4];
			pivot23.ContainerSelected = true;

			var pivot31 = package3.ContainersPivotsForBindingOnly[1];
			pivot31.ContainerSelected = true;
			var pivot32 = package3.ContainersPivotsForBindingOnly[3];
			pivot32.ContainerSelected = true;

			var wrapper1 = NctsDepartureCargoDescWrapper.New(goodsItem1);
			var wrapper2 = NctsDepartureCargoDescWrapper.New(goodsItem2);
			var wrapper3 = NctsDepartureCargoDescWrapper.New(goodsItem3);

			AssertEquals("Precondition: NCTS Phase 5", CusInBondApplicationCodeList.Codes.NCTS5, header.BH_ApplicationCode);

			AssertEquals("Item 1, use CNT and NCT containers", "MERU1234567, CNT2, CNT3", wrapper1.BOX313CONTAINERS);
			AssertEquals("Item 2, use CNT containers", "MERU1234567, CNT2, CNT3", wrapper2.BOX313CONTAINERS);
			AssertEquals("Item 3, use NCT containers", ZString.Empty, wrapper3.BOX313CONTAINERS);
		}

		public void TestWrapperProperties()
		{
			CombineAssertions(() =>
			{
				var (header, item, wrapper) = SetUpData();
				AssertEquals("CONSIGNOR NAME, CONSIGNOR STREET, 000000000 CONSIGNOR CITY, FR", wrapper.BOX2CONSIGNOR);
				AssertEquals("SECURITYCONSIGNOR NAME, SECURITYCONSIGNOR STREET, 444444444 SECURITYCONSIGNOR CITY, DE", wrapper.BOX2CONSIGNORSECURITY);
				AssertEquals("", wrapper.BOX7REFERENCE);
				AssertEquals("Commercial Ref.", wrapper.BOX7UCR);
				AssertEquals("CONSIGNEE NAME, CONSIGNEE STREET, 111111111 CONSIGNEE CITY, GB", wrapper.BOX8CONSIGNEE);
				AssertEquals("SECURITYCONSIGNEE NAME, SECURITYCONSIGNEE STREET, 555555555 SECURITYCONSIGNEE CITY, TR", wrapper.BOX8CONSIGNEESECURITY);
				AssertEquals("A", wrapper.BOX1REGIME);
				AssertEquals("DE", wrapper.BOX15COUNTRYOFORIGIN);
				AssertEquals("GB", wrapper.BOX17COUNTRYOFDESTINATION);
				AssertEquals("1", wrapper.BOX32ITEM);
				AssertEquals("1000000099", wrapper.BOX33COMMODITY);
				AssertEquals("30", wrapper.BOX35GROSSMASS);
				AssertEquals("20", wrapper.BOX38NETTMASS);
				AssertEquals("0004a", wrapper.BOX444UNDG);
				AssertEquals("CODE1; CODE2", wrapper.BOX442SPECIALMENTIONS);
				AssertContains("TestMark1", wrapper.BOX311MARKS);
				AssertContains("TestMark2", wrapper.BOX311MARKS);
				AssertContains("99 - MatchBox", wrapper.BOX312NUMBERS);
				AssertContains("88 - Bag", wrapper.BOX312NUMBERS);
				AssertContains("CONTAINER1", wrapper.BOX313CONTAINERS);
				AssertContains("CONTAINER2", wrapper.BOX313CONTAINERS);
				AssertContains("S1", wrapper.BOX314SENSITIVE);
				AssertContains("S2", wrapper.BOX314SENSITIVE);
				AssertContains("S1", wrapper.BOX315SENSITIVE);
				AssertContains("S2", wrapper.BOX315SENSITIVE);
				AssertContains("30", wrapper.BOX315SENSITIVEQTY);
				AssertContains("40", wrapper.BOX315SENSITIVEQTY);
				AssertEquals("Item description", wrapper.BOX314DESCRIPTION);
				AssertEquals("Item description", wrapper.BOX312DESCRIPTION);
				AssertEquals("Container1Seal1, Container1Seal2, Cont1AdditionalSeal1, Cont1AdditionalSeal2, Container2Seal1, Container2Seal2, Cont2AdditionalSeal1", wrapper.BOXS28SEALS);
				AssertContains("TestMark1\r\nTestMark2 99 - MatchBox\r\n88 - Bag", wrapper.BOX311MARKSANDNUMBERS);
				AssertEquals("B", wrapper.BOXS29TRANSPORTCHARGESMOP);
			});
		}

		public void TestGetSpecialMentions_Phase5()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			var bill = header.Bills.AddNew();
			var document1 = bill.AdditionalDocuments.AddNew();
			document1.CSI_SubType = "TRA";
			document1.CSI_Code = "TY1";
			document1.CSI_Description = "DESC1";

			var document2 = bill.AdditionalDocuments.AddNew();
			document2.CSI_SubType = "REF";
			document2.CSI_Code = "TY2";
			document2.CSI_Description = "DESC2";

			var item = bill.GoodsItems.AddNew();
			var wrapper = NctsDepartureCargoDescWrapper.New(item);

			AssertEquals(nameof(NctsDepartureCargoDescWrapper.BOX442SPECIALMENTIONS), "TY1-DESC1", wrapper.BOX442SPECIALMENTIONS);
		}

		public void TestGetPreviousDocumentsFormatted_Phase4()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			var item = header.MovementHeader.GoodsItems.AddNew();
			SetUpPreviousDocumentForTest(item);

			var wrapper = NctsDepartureCargoDescWrapper.New(item);
			AssertEquals(nameof(NctsDepartureCargoDescWrapper.BOX40DOCUMENTS), "380 - INV001 - Invoice; 420 - 123", wrapper.BOX40DOCUMENTS);
		}

		public void TestGetPreviousDocumentsFormatted_Phase5()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			var bill = header.Bills.AddNew();
			var billDocument = bill.PreviousDocuments.AddNew();
			billDocument.CSI_Code = "A20";
			billDocument.CSI_ReferenceNumber = "79";

			var item = bill.GoodsItems.AddNew();
			SetUpPreviousDocumentForTest(item);

			var wrapper = NctsDepartureCargoDescWrapper.New(item);
			AssertEquals(nameof(NctsDepartureCargoDescWrapper.BOX40DOCUMENTS), "A20-79; 380-INV001; 420-123", wrapper.BOX40DOCUMENTS);
		}

		public void TestGetSupportingDocumentsFormatted_Phase4()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			var item = header.MovementHeader.GoodsItems.AddNew();
			SetUpSuppportingDocumentsForTest(item);

			var wrapper = NctsDepartureCargoDescWrapper.New(item);
			AssertEquals(nameof(NctsDepartureCargoDescWrapper.BOX44), "N271-ABC123-Manifest; N825-DEF456-Packing list", wrapper.BOX44);
			AssertEquals(nameof(NctsDepartureCargoDescWrapper.BOX441DOCSANDCERTS), "N271-ABC123-Manifest; N825-DEF456-Packing list", wrapper.BOX441DOCSANDCERTS);
		}

		public void TestGetSupportingDocumentsFormatted_Phase5()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			var bill = header.Bills.AddNew();
			var billDocument = bill.SupportingDocuments.AddNew();
			billDocument.CSI_Code = "420";
			billDocument.CSI_ReferenceNumber = "123";

			var item = bill.GoodsItems.AddNew();
			SetUpSuppportingDocumentsForTest(item);

			var wrapper = NctsDepartureCargoDescWrapper.New(item);
			AssertEquals(nameof(NctsDepartureCargoDescWrapper.BOX44), "420-123; N271-ABC123; N825-DEF456", wrapper.BOX44);
			AssertEquals(nameof(NctsDepartureCargoDescWrapper.BOX441DOCSANDCERTS), "420-123; N271-ABC123; N825-DEF456", wrapper.BOX441DOCSANDCERTS);
		}

		public void TestGetWrappedPackageMarksAndNumbers_Phase5()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			var bill = header.Bills.AddNew();
			var item = bill.GoodsItems.AddNew();
			SetUpPackagesForTest(item);

			var wrapper = NctsDepartureCargoDescWrapper.New(item);
			AssertContains("TestMark1, 99 - MatchBox\r\nTestMark2, 88 - Bag", wrapper.BOX311MARKSANDNUMBERS);
		}

		public void TestGetTransportChargesMethodOfPayment_Phase5()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			var bill = header.Bills.AddNew();
			var item = bill.GoodsItems.AddNew();
			item.BY_TransportChargesMethodOfPayment = "B";
			item.MoveHeader.BM_MethodOfPayment = "Y";

			var wrapper = NctsDepartureCargoDescWrapper.New(item);
			AssertEquals("When TransportChargesMethodOfPayment is set, it should be returned", "B", wrapper.BOXS29TRANSPORTCHARGESMOP);

			item.BY_TransportChargesMethodOfPayment = string.Empty;
			bill.B0_TransportPaymentMethod = "C";
			AssertEquals("When TransportChargesMethodOfPayment is empty, it should fallback to Bill's TransportPaymentMethod", "C", wrapper.BOXS29TRANSPORTCHARGESMOP);

			bill.B0_TransportPaymentMethod = string.Empty;
			AssertEquals("When both TransportChargesMethodOfPayment and TransportPaymentMethod are empty, it should fallback to Header's MethodOfPayment", "Y", wrapper.BOXS29TRANSPORTCHARGESMOP);

			item.BY_TransportChargesMethodOfPayment = string.Empty;
			bill.B0_TransportPaymentMethod = string.Empty;
			item.MoveHeader.BM_MethodOfPayment = string.Empty;
			AssertEquals("When all are empty", "---", wrapper.BOXS29TRANSPORTCHARGESMOP);
		}

		(NctsHeader header, NctsDepartureCargoDesc item, NctsDepartureCargoDescWrapper wrapper) SetUpData()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var item = header.MovementHeader.GoodsItems.AddNew();
			item.BY_Description = "Item description";
			item.BY_Type = "A";
			item.BY_CommercialReferenceNumber = "Commercial Ref.";
			item.BY_RN_NKCountryOfOrigin = Enterprise.Core.Constants.CountryCodes.France;
			item.BY_RN_NKCountryOfDestination = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			item.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.Germany;
			item.BY_HarmonisedTariff = "1000000099";
			item.BY_GrossWeight = 30;
			item.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			item.BY_NetWeight = 20;
			item.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;
			item.BY_DeclarationGoodsItemNumber = 1;
			item.BY_TransportChargesMethodOfPayment = "B";
			item.MoveHeader.BM_MethodOfPayment = "Y";
			item.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			SetUpConsignorForTest(item);
			SetUpSecurityConsignorForTest(item);
			SetUpConsigneeForTest(item);
			SetUpSecurityConsigneeForTest(item);
			SetUpSpecialMentionsForTest(item);
			SetUpPackagesForTest(item);
			SetUpContainersAndSeals(header, item);
			SetUpSgi(item);
			var wrapper = NctsDepartureCargoDescWrapper.New(item);
			return (header, item, wrapper);
		}

		void SetUpSgi(NctsDepartureCargoDesc item)
		{
			var sgi1 = item.AdditionalInfos.AddNew();
			sgi1.CSI_Code = "SGIS1";
			sgi1.CSI_Description = "30";
			var sgi2 = item.AdditionalInfos.AddNew();
			sgi2.CSI_Code = "SGIS2";
			sgi2.CSI_Description = "40";
		}

		void SetUpContainersAndSeals(NctsHeader header, NctsDepartureCargoDesc item)
		{
			var container1 = header.DepartureHeaderContainers.AddNew();
			container1.BC_ContainerNum = "CONTAINER1";
			container1.BC_Seal1 = "Container1Seal1";
			container1.BC_Seal2 = "Container1Seal2";
			var additionalSeal1 = container1.AdditionalSeals.AddNew();
			additionalSeal1.BK_SealNumber = "Cont1AdditionalSeal1";
			var additionalSeal2 = container1.AdditionalSeals.AddNew();
			additionalSeal2.BK_SealNumber = "Cont1AdditionalSeal2";

			var container2 = header.DepartureHeaderContainers.AddNew();
			container2.BC_ContainerNum = "CONTAINER2";
			container2.BC_Seal1 = "Container2Seal1";
			container2.BC_Seal2 = "Container2Seal2";
			var additionalSeal3 = container2.AdditionalSeals.AddNew();
			additionalSeal3.BK_SealNumber = "Cont2AdditionalSeal1";

			var pivot1 = item.ContainersPivots[0];
			pivot1.ContainerNumber = "CONTAINER1";
			pivot1.ContainerSelected = true;
			var pivot2 = item.ContainersPivots[1];
			pivot2.ContainerNumber = "CONTAINER2";
			pivot2.ContainerSelected = true;
		}

		void SetUpPackagesForTest(NctsDepartureCargoDesc item)
		{
			const string packageType1 = "MX";
			const string packageType2 = "BG";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UnitedNationsPackageTypes");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, packageType1, "MatchBox", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, packageType2, "Bag", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var pack1 = item.Packages.AddNew();
			pack1.B5_MarksAndNumbers = "TestMark1";
			pack1.B5_UnitCount = 99;
			pack1.B5_UnitType = packageType1;
			var pack2 = item.Packages.AddNew();
			pack2.B5_MarksAndNumbers = "TestMark2";
			pack2.B5_UnitCount = 88;
			pack2.B5_UnitType = packageType2;
		}

		void SetUpSpecialMentionsForTest(NctsDepartureCargoDesc item)
		{
			var additionalInfo1 = item.AdditionalInfos.AddNew();
			additionalInfo1.CSI_Code = "CODE1";
			var additionalInfo2 = item.AdditionalInfos.AddNew();
			additionalInfo2.CSI_Code = "CODE2";
		}

		void SetUpSuppportingDocumentsForTest(NctsDepartureCargoDesc item)
		{
			var doc1 = item.SupportingDocuments.AddNew();
			doc1.CSI_Code = "N271";
			doc1.CSI_ReferenceNumber = "ABC123";
			doc1.CSI_Description = "Manifest";

			var doc2 = item.SupportingDocuments.AddNew();
			doc2.CSI_Code = "N825";
			doc2.CSI_ReferenceNumber = "DEF456";
			doc2.CSI_Description = "Packing list";
		}

		void SetUpPreviousDocumentForTest(NctsDepartureCargoDesc item)
		{
			var doc1 = item.PreviousDocuments.AddNew();
			doc1.CSI_Code = "380";
			doc1.CSI_ReferenceNumber = "INV001";
			doc1.CSI_Description = "Invoice";
			doc1.CSI_PackQty = 0;
			doc1.CSI_PackType = "BX";
			doc1.CSI_Quantity = 0.0m;
			doc1.CSI_UnitOfQuantity = "KG";
			doc1.CSI_ItemNumber = 12345;

			var doc2 = item.PreviousDocuments.AddNew();
			doc2.CSI_Code = "420";
			doc2.CSI_ReferenceNumber = "123";
			doc2.CSI_PackQty = 6;
			doc2.CSI_PackType = "CR";
			doc2.CSI_Quantity = 10.7m;
			doc2.CSI_UnitOfQuantity = "LIR";
			doc2.CSI_ItemNumber = 67865;
		}

		void SetUpSecurityConsigneeForTest(NctsDepartureCargoDesc item)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.MainAddress;
			orgHeader.OH_FullName = "SECURITYCONSIGNEE NAME";
			address.OA_Address1 = "SECURITYCONSIGNEE STREET";
			address.OA_PostCode = "555555555";
			address.OA_City = "SECURITYCONSIGNEE CITY";
			address.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Turkey;
			item.SecurityConsignee.E2_OA_Address = address.PK;
			var eori = orgHeader.CustomsCodes.AddNew();
			eori.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			eori.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.Turkey;
			eori.OK_CustomsRegNo = "55555555555555555";
		}

		void SetUpSecurityConsignorForTest(NctsDepartureCargoDesc item)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.MainAddress;
			orgHeader.OH_FullName = "SECURITYCONSIGNOR NAME";
			address.OA_Address1 = "SECURITYCONSIGNOR STREET";
			address.OA_PostCode = "444444444";
			address.OA_City = "SECURITYCONSIGNOR CITY";
			address.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Germany;
			item.SecurityConsignor.E2_OA_Address = address.PK;
			var eori = orgHeader.CustomsCodes.AddNew();
			eori.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			eori.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.Germany;
			eori.OK_CustomsRegNo = "44444444444444444";
		}

		void SetUpConsigneeForTest(NctsDepartureCargoDesc item)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.MainAddress;
			orgHeader.OH_FullName = "CONSIGNEE NAME";
			address.OA_Address1 = "CONSIGNEE STREET";
			address.OA_PostCode = "111111111";
			address.OA_City = "CONSIGNEE CITY";
			address.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			item.Consignee.E2_OA_Address = address.PK;
			var eori = orgHeader.CustomsCodes.AddNew();
			eori.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			eori.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			eori.OK_CustomsRegNo = "11111111111111111";
		}

		void SetUpConsignorForTest(NctsDepartureCargoDesc item)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.MainAddress;
			orgHeader.OH_FullName = "CONSIGNOR NAME";
			address.OA_Address1 = "CONSIGNOR STREET";
			address.OA_PostCode = "000000000";
			address.OA_City = "CONSIGNOR CITY";
			address.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.France;
			item.Consignor.E2_OA_Address = address.PK;
			var eori = orgHeader.CustomsCodes.AddNew();
			eori.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			eori.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.France;
			eori.OK_CustomsRegNo = "00000000000000000";
		}
	}
}
