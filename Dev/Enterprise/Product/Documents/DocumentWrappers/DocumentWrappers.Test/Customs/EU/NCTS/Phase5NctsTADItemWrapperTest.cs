using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.EU.NCTS.Testing
{
	[TestedType(typeof(Phase5NctsTADItemWrapper))]
	sealed class Phase5NctsTADItemWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var item = header.Bills.AddNew().GoodsItems.AddNew();
			return Phase5NctsTADItemWrapper.New(item, Factory);
		}

		public void TestGoodsItemNumber() => AssertEquals("GoodsItemNumber", "1", Wrapper.GoodsItemNumber);

		public void TestDeclarationGoodsItemNumber() => AssertEquals("DeclarationGoodsItemNumber", "2", Wrapper.DeclarationGoodsItemNumber);

		public void TestTypeNumberOfPackagesShippingMarks()
		{
			SetupPackagesForTest();
			AssertEquals("Packages", "1,CN,1,ABC1231; 2,CN,2,ABC1232; 3,CN,3,ABC1233; 4,CN,4,ABC1234; 5,CN,5,ABC1235", Wrapper.Packages);
		}

		public void TestConsignor() => CombineAssertions(() =>
		{
			SetUpConsignorForTest(true, true, true);
			AssertConsignor("Set on Header, Bill and GoodsItem", true, "_G");

			SetUpConsignorForTest(true, true, false);
			AssertConsignor("Set on Header and Bill", true, "_B");

			SetUpConsignorForTest(true, false, false);
			AssertConsignor("Set on Header", false);

			SetUpConsignorForTest(false, false, true);
			AssertConsignor("Set on GoodsItem", true, "_G");

			SetUpConsignorForTest(false, true, false);
			AssertConsignor("Set on Bill", true, "_B");

			void AssertConsignor(string condition, bool expected, string expectedSuffix = null)
			{
				var expectedId = ZString.Empty; 
				var expectedAddress = ZString.Empty;

				if (expected)
				{
					expectedId = $"IT0007{expectedSuffix}";
					expectedAddress = $@"BOB{expectedSuffix}
STREET{expectedSuffix}
123321{expectedSuffix} CITY{expectedSuffix} IT
TOM B{expectedSuffix} 011234567{expectedSuffix} {expectedSuffix}.tom@test.com";
				}

				AssertEquals($"{condition} - Consignor Id", expectedId, Wrapper.ConsignorId);
				AssertEquals($"{condition} - Consignor address", expectedAddress, Wrapper.ConsignorAddress);
			}
		});

		public void TestConsignee() => CombineAssertions(() =>
		{
			SetUpConsigneeForTest(true, true, true);
			AssertConsignee("Set on Header, Bill and GoodsItem", true, "_G");

			SetUpConsigneeForTest(true, true, false);
			AssertConsignee("Set on Header and Bill", true, "_B");

			SetUpConsigneeForTest(true, false, false);
			AssertConsignee("Set on Header", false);

			SetUpConsigneeForTest(false, false, true);
			AssertConsignee("Set on GoodsItem", true, "_G");

			SetUpConsigneeForTest(false, true, false);
			AssertConsignee("Set on Bill", true, "_B");

			void AssertConsignee(string condition, bool expected, string expectedSuffix = null)
			{
				var expectedId = ZString.Empty;
				var expectedAddress = ZString.Empty;

				if (expected)
				{
					expectedId = $"IT0007{expectedSuffix}";
					expectedAddress = $@"BOB{expectedSuffix}
STREET{expectedSuffix}
123321{expectedSuffix} CITY{expectedSuffix} IT
TOM B{expectedSuffix} 011234567{expectedSuffix} {expectedSuffix}.tom@test.com";
				}

				AssertEquals($"{condition} - Consignor Id", expectedId, Wrapper.ConsigneeId);
				AssertEquals($"{condition} - Consignor address", expectedAddress, Wrapper.ConsigneeAddress);
			}
		});

		public void TestAdditionalSupplyChainActor()
		{
			SetUpAdditionalSupplyChainActorsForTest();
			AssertEquals("Additional Supply Chain Actor", "1,TS1,REF1; 2,TS2,REF2", Wrapper.AdditionalSupplyChainActor);
		}

		public void TestDepartureTransportMeans()
		{
			SetupDepartureTransportMeansForTest();
			AssertEquals("Departure transport means", "1,A1,IDNumber1,ES; 2,A2,IDNumber2,ES; 3,A3,IDNumber3,ES; 4,A4,IDNumber4,ES; 5,A5,IDNumber5,ES", Wrapper.DepartureTransportMeans);
		}

		public void TestDangerousGoods()
		{
			SetupDangerousGoodsForTest();
			AssertEquals("Dangerous goods", $"1,0004a; 2,0004b; 3,0004c", Wrapper.DangerousGoods);
		}

		public void TestCusCode() => AssertEquals("Cus code", "7", Wrapper.CusCode);

		public void TestTransportCharges() => CombineAssertions(() =>
		{
			AssertEquals("Transport charges", "V", Wrapper.TransportCharges);
			GoodsItem.BY_TransportChargesMethodOfPayment = ZString.Empty;
			AssertEquals("Transport charges", "H", Wrapper.TransportCharges);
		});

		public void TestDescriptionOfGoods() => AssertEquals("Description of goods", "TEST ITEM", Wrapper.DescriptionOfGoods);

		public void TestPreviousDocuments()
		{
			SetUpPreviousDocumentsForTest();
			AssertEquals("Previous documents", "1,C652,PRV00HC,1,HCA,500,HCB,300,Sample Text HC; 2,C653,PRV003,3,,,BC,3003,Sample Text 3; 3,C651,PRV001,1,,,BC,3001,Sample Text 1; 4,C652,PRV002,2,AB,1052,,,Sample Text 2; 5,C654,PRV004,4,AB,1054,,,Sample Text 4; 6,C655,PRV005,5,,,BC,3005,Sample Text 5", Wrapper.PreviousDocuments);
		}

		public void TestSupportingDocument()
		{
			SetUpSupportingDocsForTest();
			AssertEquals("Supporting documents", "1,ABC2,HCSUP02,2,Supporting document 2; 2,ABC3,SUP03,3,Supporting document 3; 3,ABC1,SUP01,1,Supporting document 1; 4,ABC2,SUP02,2,Supporting document 2; 5,ABC4,SUP04,4,Supporting document 4; 6,ABC5,SUP05,,Supporting document 5", Wrapper.SupportingDocuments);
		}

		public void TestAdditionalReference()
		{
			SetUpAdditionalReferenceForTest();
			AssertEquals("Additional reference", "1,REF1,Additional Reference HC 1; 2,REF3,Additional Reference 3; 3,REF1,Additional Reference 1; 4,REF2,Additional Reference 2; 5,REF4,Additional Reference 4; 6,REF5,Additional Reference 5", Wrapper.AdditionalReference);
		}

		public void TestAdditionalInformation()
		{
			SetUpAdditionalInfoForTest();
			AssertEquals("Additional information", "1,INF1,INFHC1231; 2,INF3,INF1233; 3,INF1,INF1231; 4,INF2,INF1232; 5,INF4,INF1234; 6,INF5,INF1235", Wrapper.AdditionalInformation);
		}

		public void TestTransportDocument()
		{
			SetUpTransportDocForTest();
			AssertEquals("TransportDocuments", "1,111,HC221; 2,113,223; 3,111,221; 4,112,222; 5,114,224; 6,115,225", Wrapper.TransportDocuments);
		}

		public void TestReferenceNumberUCR() => AssertEquals("ReferenceNumberUCR", "22222", Wrapper.ReferenceNumberUCR);

		public void TestCommodityCode()
		{
			AssertEquals("Commodity code", "123456 78", Wrapper.CommodityCode);
		}

		public void TestDeclarationType() => AssertEquals("Declaration type", "Z", Wrapper.DeclarationType);

		public void TestCountryOfDispatch() => AssertEquals("Country of dispatch", Core.Constants.CountryCodes.UnitedKingdom, Wrapper.CountryOfDispatch);

		public void TestCountryOfDestination() => AssertEquals("Country of destination", Core.Constants.CountryCodes.Venezuela, Wrapper.CountryOfDestination);

		public void TestGrossMass() => AssertEquals("Gross mass", "34567", Wrapper.GrossMass);

		public void TestNetMass() => AssertEquals("Net mass", "0.04", Wrapper.NetMass);

		public void TestSupplementaryUnits() => AssertEquals("Supplementary units", "2.123457", Wrapper.SupplementaryUnits);

		NctsDepartureCargoDesc GoodsItem
		{
			get
			{
				if (goodsItem == null)
				{
					var nctsHeader = Factory.New<NctsHeader>();
					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
					nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
					var movementHeader = nctsHeader.MovementHeader;
					movementHeader.BM_BH = nctsHeader.PK;
					movementHeader.BM_InBondEntryType = "T1";
					movementHeader.BM_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.France;
					movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Spain;
					movementHeader.TransportTypeAtDeparture = "AB";
					movementHeader.TransportAtDeparture = "5";
					movementHeader.TransportCountryAtDeparture = Core.Constants.CountryCodes.Spain;
					var bill = nctsHeader.Bills.AddNew();
					bill.B0_ReferenceID = "UCR0001";
					bill.B0_TransportPaymentMethod = "H";
					goodsItem = bill.GoodsItems.AddNew();
					SetupGoodsItem(goodsItem);
				}
				return goodsItem;
			}
		}
		NctsDepartureCargoDesc goodsItem;

		NctsBill Bill => GoodsItem.Bill;
		NctsHeader NctsHeader => GoodsItem.Header;

		Phase5NctsTADItemWrapper Wrapper => wrapper ??= Phase5NctsTADItemWrapper.New(GoodsItem, Factory);
		Phase5NctsTADItemWrapper wrapper;

		void ResetWrapper() => wrapper = null;

		void SetupGoodsItem(NctsDepartureCargoDesc item)
		{
			item.BY_LineNo = 1;
			item.BY_DeclarationGoodsItemNumber = 2;
			item.BY_Type = "Z";
			item.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.UnitedKingdom;
			item.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Venezuela;
			item.BY_CommercialReferenceNumber = "22222";
			item.BY_Description = "TEST ITEM";
			item.BY_CusC4Number = "7";
			item.BY_HarmonisedTariff = "1234567890";
			item.BY_GrossWeight = 34.567m;
			item.BY_GrossWeightUnit = "T";
			item.BY_NetWeight = 40;
			item.BY_NetWeightUnit = "G";
			item.BY_CustomsSecondQuantity = 2.123456789m;
			item.BY_TransportChargesMethodOfPayment = "V";
		}

		void SetUpConsignorForTest(bool setupOnHeader, bool setupOnBill, bool setupOnGoodsItem)
		{
			var addressPK = ZGuid.Empty;

			if (setupOnHeader)
			{
				addressPK = PrepareAddressAndContact("_H");
			}
			NctsHeader.Consignor.E2_OA_Address = addressPK;

			addressPK = ZGuid.Empty;
			if (setupOnBill)
			{
				addressPK = PrepareAddressAndContact("_B");
			}
			Bill.Consignor.E2_OA_Address = addressPK;

			addressPK = ZGuid.Empty;
			if (setupOnGoodsItem)
			{
				addressPK = PrepareAddressAndContact("_G");
			}
			GoodsItem.Consignor.E2_OA_Address = addressPK;
		}

		void SetUpConsigneeForTest(bool setupOnHeader, bool setupOnBill, bool setupOnGoodsItem)
		{
			var addressPK = ZGuid.Empty;

			if (setupOnHeader)
			{
				addressPK = PrepareAddressAndContact("_H");
			}
			NctsHeader.Consignee.E2_OA_Address = addressPK;

			addressPK = ZGuid.Empty;
			if (setupOnBill)
			{
				addressPK = PrepareAddressAndContact("_B");
			}
			Bill.Consignee.E2_OA_Address = addressPK;

			addressPK = ZGuid.Empty;
			if (setupOnGoodsItem)
			{
				addressPK = PrepareAddressAndContact("_G");
			}
			GoodsItem.Consignee.E2_OA_Address = addressPK;
		}

		ZGuid PrepareAddressAndContact(string suffix)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.MainAddress;
			orgHeader.OH_FullName = $"BOB{suffix}";
			address.OA_Address1 = $"STREET{suffix}";
			address.OA_PostCode = $"123321{suffix}";
			address.OA_City = $"CITY{suffix}";
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
			var eori = orgHeader.CustomsCodes.AddNew();
			eori.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			eori.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Italy;
			eori.OK_CustomsRegNo = $"0007{suffix}";
			var contact = orgHeader.Contacts.AddNew();
			contact.OC_ContactName = $"TOM B{suffix}";
			contact.OC_Phone = $"011234567{suffix}";
			contact.OC_Email = $"{suffix}.tom@test.com";
			return address.PK;
		}

		void SetUpAdditionalSupplyChainActorsForTest()
		{
			var asca1 = GoodsItem.CusSupplyChainActorReferences.AddNew();
			asca1.CFR_Code = "TS1";
			asca1.CFR_Reference = "REF1";
			var asca2 = GoodsItem.CusSupplyChainActorReferences.AddNew();
			asca2.CFR_Code = "TS2";
			asca2.CFR_Reference = "REF2";
		}

		void SetUpPreviousDocumentsForTest()
		{
			var hcDoc = Bill.PreviousDocuments.AddNew();
			hcDoc.CSI_Code = "C652";
			hcDoc.CSI_ReferenceNumber = "PRV00HC";
			hcDoc.CSI_ReferenceNumber2 = "Sample Text HC";
			hcDoc.CSI_ItemNumber = 1;
			hcDoc.CSI_UnitOfQuantity2 = "HCA";
			hcDoc.CSI_Quantity2 = 500;
			hcDoc.CSI_UnitOfQuantity = "HCB";
			hcDoc.CSI_Quantity = 300;

			hcDoc = Bill.PreviousDocuments.AddNew();
			hcDoc.CSI_Code = "C653";
			hcDoc.CSI_ReferenceNumber = "PRV003";
			hcDoc.CSI_ReferenceNumber2 = "Sample Text 3";
			hcDoc.CSI_ItemNumber = 3;
			hcDoc.CSI_UnitOfQuantity = "BC";
			hcDoc.CSI_Quantity = 3003;

			for (int i = 1; i <= 5; ++i)
			{
				var doc = GoodsItem.PreviousDocuments.AddNew();
				doc.CSI_Code = $"C65{i}";
				doc.CSI_ReferenceNumber = $"PRV00{i}";
				doc.CSI_ReferenceNumber2 = $"Sample Text {i}";
				doc.CSI_ItemNumber = i;
				if (i % 2 == 0)
				{
					doc.CSI_UnitOfQuantity2 = "AB";
					doc.CSI_Quantity2 = 1050 + i;
				}
				else
				{
					doc.CSI_UnitOfQuantity = "BC";
					doc.CSI_Quantity = 3000 + i;
				}
			}
		}

		void SetUpSupportingDocsForTest()
		{
			var hcDoc = Bill.SupportingDocuments.AddNew();
			hcDoc.CSI_Code = "ABC2";
			hcDoc.CSI_ReferenceNumber = "HCSUP02";
			hcDoc.CSI_ReferenceNumber2 = "Supporting document 2";
			hcDoc.CSI_ItemNumber = 2;

			hcDoc = Bill.SupportingDocuments.AddNew();
			hcDoc.CSI_Code = "ABC3";
			hcDoc.CSI_ReferenceNumber = "SUP03";
			hcDoc.CSI_ReferenceNumber2 = "Supporting document 3";
			hcDoc.CSI_ItemNumber = 3;

			for (int i = 1; i <= 5; ++i)
			{
				var doc = GoodsItem.SupportingDocuments.AddNew();
				doc.CSI_Code = $"ABC{i}";
				doc.CSI_ReferenceNumber = $"SUP0{i}";
				doc.CSI_ItemNumber = i == 5 ? 0 : i;
				doc.CSI_ReferenceNumber2 = $"Supporting document {i}";
			}
		}

		void SetUpTransportDocForTest()
		{
			var hcDoc = Bill.AdditionalDocuments.AddNew();
			hcDoc.CSI_SubType = "TRA";
			hcDoc.CSI_Code = "111";
			hcDoc.CSI_ReferenceNumber = "HC221";

			hcDoc = Bill.AdditionalDocuments.AddNew();
			hcDoc.CSI_SubType = "TRA";
			hcDoc.CSI_Code = "113";
			hcDoc.CSI_ReferenceNumber = "223";

			for (int i = 1; i <= 5; ++i)
			{
				var doc = GoodsItem.AdditionalInfos.AddNew();
				doc.CSI_SubType = "TRA";
				doc.CSI_Code = $"11{i}";
				doc.CSI_ReferenceNumber = $"22{i}";
			}
		}

		void SetUpAdditionalReferenceForTest()
		{
			var hcDoc = Bill.AdditionalDocuments.AddNew();
			hcDoc.CSI_SubType = "REF";
			hcDoc.CSI_Code = "REF1";
			hcDoc.CSI_ReferenceNumber = "Additional Reference HC 1";

			hcDoc = Bill.AdditionalDocuments.AddNew();
			hcDoc.CSI_SubType = "REF";
			hcDoc.CSI_Code = "REF3";
			hcDoc.CSI_ReferenceNumber = "Additional Reference 3";

			for (var i = 1; i <= 5; ++i)
			{
				var doc = GoodsItem.AdditionalInfos.AddNew();
				doc.CSI_SubType = "REF";
				doc.CSI_Code = $"REF{i}";
				doc.CSI_ReferenceNumber = $"Additional Reference {i}";
			}
		}

		void SetUpAdditionalInfoForTest()
		{
			var hcDoc = Bill.AdditionalDocuments.AddNew();
			hcDoc.CSI_SubType = "INF";
			hcDoc.CSI_Code = "INF1";
			hcDoc.CSI_Description = "INFHC1231";

			hcDoc = Bill.AdditionalDocuments.AddNew();
			hcDoc.CSI_SubType = "INF";
			hcDoc.CSI_Code = "INF3";
			hcDoc.CSI_Description = "INF1233";

			for (var i = 1; i <= 5; ++i)
			{
				var doc = GoodsItem.AdditionalInfos.AddNew();
				doc.CSI_SubType = "INF";
				doc.CSI_Code = $"INF{i}";
				doc.CSI_Description = $"INF123{i}";
			}
		}

		void SetupDangerousGoodsForTest()
		{
			GoodsItem.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			GoodsItem.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "b", "IMO").First().PK;
			GoodsItem.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "c", "IMO").First().PK;
		}

		void SetupPackagesForTest()
		{
			for (var i = 1; i <= 5; ++i)
			{
				var pack = GoodsItem.Packages.AddNew();
				pack.B5_UnitType = "CN";
				pack.B5_UnitCount = i;
				pack.B5_MarksAndNumbers = $"ABC123{i}";
			}
		}

		void SetupDepartureTransportMeansForTest()
		{
			for (var i = 1; i <= 5; ++i)
			{
				var dpm = Bill.DepartureTransportInfos.AddNew();
				dpm.TPM_TypeOfIdentification = $"A{i}";
				dpm.TPM_IdentificationNumber = $"IDNumber{i}";
				dpm.TPM_RN_NKTransportNationality = Core.Constants.CountryCodes.Spain;
			}
		}
	}
}
