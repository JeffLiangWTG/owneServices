using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.Universal;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;
using NctsMovementType = Enterprise.Customs.EU.NCTS.Business.NctsMovementType;

namespace Enterprise.Customs.ES.DocumentWrappers.NCTS.Testing
{
	[TestedType(typeof(ESNctsDepartureCargoDescWrapper))]
	sealed class ESNctsDepartureCargoDescWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestBox40Documents_Phase4()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var item = header.MovementHeader.GoodsItems.AddNew();

			CombineAssertions(() =>
			{
				var wrapper = ESNctsDepartureCargoDescWrapper.New(item, Factory);
				AssertEquals(nameof(ESNctsDepartureCargoDescWrapper.BOX40DOCUMENTS), "", wrapper.BOX40DOCUMENTS);

				var documentOnFirstLine = item.PreviousDocuments.AddNew();
				documentOnFirstLine.CSI_Procedure = "7";
				documentOnFirstLine.CSI_SubType = Enterprise.Customs.EU.Business.PreviousDocumentClassList.Codes.PreviousDocument;
				documentOnFirstLine.CSI_Code = "380";
				documentOnFirstLine.CSI_ReferenceNumber = "34217890";
				documentOnFirstLine.CSI_DateOfIssue = new ZDateTime(2000, 1, 2);
				documentOnFirstLine.CSI_Status = "G";
				documentOnFirstLine.CSI_CustomsOffice = "IT279100";
				documentOnFirstLine.CSI_LineNo = 1;

				var documentOnSecondLine = item.PreviousDocuments.AddNew();
				documentOnSecondLine.CSI_Procedure = "7";
				documentOnSecondLine.CSI_SubType = Enterprise.Customs.EU.Business.PreviousDocumentClassList.Codes.SummaryDeclaration;
				documentOnSecondLine.CSI_Code = "CLE";
				documentOnSecondLine.CSI_ReferenceNumber = "20070701";
				documentOnSecondLine.CSI_DateOfIssue = new ZDateTime(2000, 1, 3);
				documentOnSecondLine.CSI_Status = "G";
				documentOnSecondLine.CSI_CustomsOffice = "IT279100";
				documentOnSecondLine.CSI_LineNo = 1;

				wrapper = ESNctsDepartureCargoDescWrapper.New(item, Factory);

				AssertEquals("BOX40DOCUMENTS", "Z 380 34217890; X CLE 20070701 1", wrapper.BOX40DOCUMENTS);
			});
		}

		public void TestBox40Documents_Phase5()
		{
			var bill = header.Bills.AddNew();
			var item = bill.GoodsItems.AddNew();

			CombineAssertions(() =>
			{
				var wrapper = ESNctsDepartureCargoDescWrapper.New(item, Factory);
				AssertEquals(nameof(ESNctsDepartureCargoDescWrapper.BOX40DOCUMENTS), "", wrapper.BOX40DOCUMENTS);

				var billsPreviousDocument = bill.PreviousDocuments.AddNew();
				billsPreviousDocument.CSI_Procedure = "7";
				billsPreviousDocument.CSI_SubType = "Z";
				billsPreviousDocument.CSI_Code = "380";
				billsPreviousDocument.CSI_ReferenceNumber = "34217890";
				billsPreviousDocument.CSI_DateOfIssue = new ZDateTime(2000, 1, 2);
				billsPreviousDocument.CSI_Status = "G";
				billsPreviousDocument.CSI_CustomsOffice = "IT279100";
				billsPreviousDocument.CSI_LineNo = 1;

				var itemsPreviousDocument = item.PreviousDocuments.AddNew();
				itemsPreviousDocument.CSI_Procedure = "7";
				itemsPreviousDocument.CSI_SubType = "X";
				itemsPreviousDocument.CSI_Code = "CLE";
				itemsPreviousDocument.CSI_ReferenceNumber = "20070701";
				itemsPreviousDocument.CSI_DateOfIssue = new ZDateTime(2000, 1, 3);
				itemsPreviousDocument.CSI_Status = "G";
				itemsPreviousDocument.CSI_CustomsOffice = "IT279100";
				itemsPreviousDocument.CSI_LineNo = 1;

				wrapper = ESNctsDepartureCargoDescWrapper.New(item, Factory);

				AssertEquals(nameof(ESNctsDepartureCargoDescWrapper.BOX40DOCUMENTS), "380-34217890; CLE-20070701", wrapper.BOX40DOCUMENTS);
			});
		}

		public void TestGetSupportingDocumentsFormatted_Phase4()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var item = header.MovementHeader.GoodsItems.AddNew();

			CombineAssertions(() =>
			{
				var wrapper = ESNctsDepartureCargoDescWrapper.New(item, Factory);
				AssertEquals(nameof(ESNctsDepartureCargoDescWrapper.BOX44), "", wrapper.BOX44);

				var supportingDocument = item.SupportingDocuments.AddNew();
				supportingDocument.CSI_Code = "N380";
				supportingDocument.CSI_ReferenceNumber = "A0023";
				supportingDocument.CSI_RN_NKCountryCode = "IT";
				supportingDocument.CSI_UnitOfQuantity = "XYZ";
				supportingDocument.CSI_Quantity = 343.43m;

				wrapper = ESNctsDepartureCargoDescWrapper.New(item, Factory);
				AssertEquals(nameof(ESNctsDepartureCargoDescWrapper.BOX44), "N380: A0023", wrapper.BOX44);
			});
		}

		public void TestGetSupportingDocumentsFormatted_Phase5()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = header.Bills.AddNew();
			var item = bill.GoodsItems.AddNew();

			CombineAssertions(() =>
			{
				var wrapper = ESNctsDepartureCargoDescWrapper.New(item, Factory);
				AssertEquals(nameof(ESNctsDepartureCargoDescWrapper.BOX44), "", wrapper.BOX44);

				var billDocument = bill.SupportingDocuments.AddNew();
				billDocument.CSI_Code = "420";
				billDocument.CSI_ReferenceNumber = "123";

				var itemDocument = item.SupportingDocuments.AddNew();
				itemDocument.CSI_Code = "N380";
				itemDocument.CSI_ReferenceNumber = "A0023";
				itemDocument.CSI_RN_NKCountryCode = "IT";
				itemDocument.CSI_UnitOfQuantity = "XYZ";
				itemDocument.CSI_Quantity = 343.43m;

				wrapper = ESNctsDepartureCargoDescWrapper.New(item, Factory);
				AssertEquals(nameof(ESNctsDepartureCargoDescWrapper.BOX44), "420: 123; N380: A0023", wrapper.BOX44);
			});
		}

		public void TestBox33Commodity()
		{
			item.BY_HarmonisedTariff = "1234567890";

			CombineAssertions("BY_HarmonisedTariff is truncated only if it is longer than 8 characters", () =>
			{
				var wrapper = ESNctsDepartureCargoDescWrapper.New(item, Factory);
				AssertEquals(nameof(ESNctsDepartureCargoDescWrapper.BOX33COMMODITY), "12345678", wrapper.BOX33COMMODITY);

				item.BY_HarmonisedTariff = "1234";
				AssertEquals(nameof(ESNctsDepartureCargoDescWrapper.BOX33COMMODITY), "1234", wrapper.BOX33COMMODITY);

				item.BY_HarmonisedTariff = "";
				AssertEquals(nameof(ESNctsDepartureCargoDescWrapper.BOX33COMMODITY), "---", wrapper.BOX33COMMODITY);
			});
		}

		public void TestBox441DocsAndCerts()
		{
			CombineAssertions(() =>
			{
				var wrapper = ESNctsDepartureCargoDescWrapper.New(item, Factory);
				AssertEquals(nameof(ESNctsDepartureCargoDescWrapper.BOX441DOCSANDCERTS), "", wrapper.BOX441DOCSANDCERTS);

				var supportingDocument1 = item.SupportingDocuments.AddNew();
				var supportingDocument2 = item.SupportingDocuments.AddNew();
				var supportingDocument3 = item.SupportingDocuments.AddNew();
				var supportingDocument4 = item.SupportingDocuments.AddNew();
				var supportingDocument5 = item.SupportingDocuments.AddNew();

				supportingDocument1.CSI_Code = "N380";
				supportingDocument1.CSI_ReferenceNumber = "A0023";
				supportingDocument1.CSI_RN_NKCountryCode = "IT";
				supportingDocument1.CSI_UnitOfQuantity = "XYZ";
				supportingDocument1.CSI_Quantity = 343.43m;

				supportingDocument2.CSI_Code = "C601";
				supportingDocument2.CSI_ReferenceNumber = "A0050";
				supportingDocument2.CSI_RN_NKCountryCode = "DE";
				supportingDocument2.CSI_UnitOfQuantity = "KGM";

				supportingDocument3.CSI_Code = "N381";
				supportingDocument3.CSI_ReferenceNumber = "A0033";
				supportingDocument3.CSI_RN_NKCountryCode = "IT";
				supportingDocument3.CSI_UnitOfQuantity = "KGM";

				supportingDocument4.CSI_Code = "C888";
				supportingDocument4.CSI_ReferenceNumber = "A0987";

				supportingDocument5.CSI_Code = "CXXX";

				wrapper = ESNctsDepartureCargoDescWrapper.New(item, Factory);
				AssertEquals(nameof(ESNctsDepartureCargoDescWrapper.BOX441DOCSANDCERTS), "N380: A0023; C601: A0050; N381: A0033; C888: A0987; CXXX:", wrapper.BOX441DOCSANDCERTS);
			});
		}

		public void TestBOX35GROSSMASSFormat()
		{
			var wrapper = ESNctsDepartureCargoDescWrapper.New(item, Factory);

			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
							Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
				{
					item.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
					item.BY_GrossWeight = 1.1;
					AssertEquals(nameof(ESNctsDepartureCargoDescWrapper.BOX35GROSSMASS) + " is 1,100000 when gross mass is 1.1 and final period", "1,100000", wrapper.BOX35GROSSMASS);

					item.BY_GrossWeight = 0.01;
					AssertEquals(nameof(ESNctsDepartureCargoDescWrapper.BOX35GROSSMASS) + " is 0,010000 when gross mass is 0.01 and final period", "0,010000", wrapper.BOX35GROSSMASS);

					item.BY_GrossWeight = 100.01;
					AssertEquals(nameof(ESNctsDepartureCargoDescWrapper.BOX35GROSSMASS) + " is 100,010000 when gross mass is 100.01 and final period", "100,010000", wrapper.BOX35GROSSMASS);

					item.BY_GrossWeight = 30.1234m;
					AssertEquals(nameof(ESNctsDepartureCargoDescWrapper.BOX35GROSSMASS) + " is 30.123400 when final period", "30,123400", wrapper.BOX35GROSSMASS);

					item.BY_GrossWeightUnit = Core.Constants.Weight.ShortTons;
					item.BY_GrossWeight = 9;
					AssertEquals(nameof(ESNctsDepartureCargoDescWrapper.BOX35GROSSMASS) + " is 8.164,664963 with conversion from ShortTons (9) to KG when final period", "8.164,664963", wrapper.BOX35GROSSMASS);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
							Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
				{
					item.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
					item.BY_GrossWeight = 1.1;
					AssertEquals(nameof(ESNctsDepartureCargoDescWrapper.BOX35GROSSMASS) + " is 1,100 when gross mass is 1.1 and transition period", "1,100", wrapper.BOX35GROSSMASS);

					item.BY_GrossWeight = 0.01;
					AssertEquals(nameof(ESNctsDepartureCargoDescWrapper.BOX35GROSSMASS) + " is 0,010 when gross mass is 0.01 and transition period", "0,010", wrapper.BOX35GROSSMASS);

					item.BY_GrossWeight = 100.01;
					AssertEquals(nameof(ESNctsDepartureCargoDescWrapper.BOX35GROSSMASS) + " is 100,010 when gross mass is 100.01 and transition period", "100,010", wrapper.BOX35GROSSMASS);

					item.BY_GrossWeight = 30.1234m;
					AssertEquals(nameof(ESNctsDepartureCargoDescWrapper.BOX35GROSSMASS) + " is 30.123 when transition period", "30,123", wrapper.BOX35GROSSMASS);

					item.BY_GrossWeightUnit = Core.Constants.Weight.ShortTons;
					item.BY_GrossWeight = 9;
					AssertEquals(nameof(ESNctsDepartureCargoDescWrapper.BOX35GROSSMASS) + " is 8.164,665 with conversion from ShortTons (9) to KG when transition period", "8.164,665", wrapper.BOX35GROSSMASS);
				}
			});
		}

		public void TestBOX38NETTMASSFormat()
		{
			item.BY_NetWeight = 30;
			item.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;

			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
							Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
				{
					var wrapper = ESNctsDepartureCargoDescWrapper.New(item, Factory);
					AssertEquals(nameof(ESNctsDepartureCargoDescWrapper.BOX38NETTMASS) + " is 30,000000 when final period", "30,000000", wrapper.BOX38NETTMASS);

					item.BY_NetWeight = 3000;
					AssertEquals(nameof(ESNctsDepartureCargoDescWrapper.BOX38NETTMASS) + " is 3000,000000 when final period", "3.000,000000", wrapper.BOX38NETTMASS);

					item.BY_NetWeight = 30.1234m;
					AssertEquals(nameof(ESNctsDepartureCargoDescWrapper.BOX38NETTMASS) + " is 30.123400 when final period", "30,123400", wrapper.BOX38NETTMASS);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
							Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
				{
					item.BY_NetWeight = 30;
					var wrapper = ESNctsDepartureCargoDescWrapper.New(item, Factory);
					AssertEquals(nameof(ESNctsDepartureCargoDescWrapper.BOX38NETTMASS) + " is 30,000 when transition period", "30,000", wrapper.BOX38NETTMASS);

					item.BY_NetWeight = 3000;
					AssertEquals(nameof(ESNctsDepartureCargoDescWrapper.BOX38NETTMASS) + " is 3000,000 when transition period", "3.000,000", wrapper.BOX38NETTMASS);

					item.BY_NetWeight = 30.1234m;
					AssertEquals(nameof(ESNctsDepartureCargoDescWrapper.BOX38NETTMASS) + " is 30.123 when transition period", "30,123", wrapper.BOX38NETTMASS);
				}
			});
		}

		public void TestBOX1REGIME()
		{
			var moveHeader = header.MovementHeader;
			CombineAssertions(() =>
			{
				var wrapper = ESNctsDepartureCargoDescWrapper.New(item, Factory);
				AssertEquals(nameof(ESNctsDepartureCargoDescWrapper.BOX1REGIME) + " is empty (---) when BM_InBondEntryType is empty", "---", wrapper.BOX1REGIME);

				moveHeader.BM_InBondEntryType = "T1";
				AssertEquals(nameof(ESNctsDepartureCargoDescWrapper.BOX1REGIME) + " has the correct value when BM_InBondEntryType is not empty", "T1", wrapper.BOX1REGIME);

				item.BY_Type = "A";
				moveHeader.BM_InBondEntryType = "TIR";
				AssertEquals(nameof(ESNctsDepartureCargoDescWrapper.BOX1REGIME) + " has the correct value when BM_InBondEntryType is not empty, even if BY_Type is not empty", "TIR", wrapper.BOX1REGIME);
			});
		}

		public void TestBOX312NUMBERS_Packages()
		{
			SetUpPackagesForTest(item);
			var wrapper = ESNctsDepartureCargoDescWrapper.New(item, Factory);

			CombineAssertions(() =>
			{
				AssertContains("99 - Caja1", wrapper.BOX312NUMBERS);
				AssertContains("88 - Caja2", wrapper.BOX312NUMBERS);
			});
		}

		public void TestBOX312NUMBERS_Vehicles()
		{
			SetUpVehiclesForTest(item);

			var wrapper = ESNctsDepartureCargoDescWrapper.New(item, Factory);
			AssertContains("1 - BASTIDOR\r\n1 - BASTIDOR\r\n1 - BASTIDOR\r\n1 - BASTIDOR", wrapper.BOX312NUMBERS);
		}

		public void TestBOX311MARKS_Packages()
		{
			SetUpPackagesForTest(item);

			var wrapper = ESNctsDepartureCargoDescWrapper.New(item, Factory);
			AssertContains("TestMark1\r\nTestMark2", wrapper.BOX311MARKS);
		}

		public void TestBOX311MARKS_Vehicles()
		{
			SetUpVehiclesForTest(item);

			var wrapper = ESNctsDepartureCargoDescWrapper.New(item, Factory);
			AssertContains("VINCODE1:BRAND1:MODEL1\r\n:BRAND2:MODEL2\r\nVINCODE3::MODEL3\r\nVINCODE4:BRAND4:", wrapper.BOX311MARKS);
		}

		public void TestBOX311MARKSANDNUMBERS_Packages()
		{
			SetUpPackagesForTest(item);

			var wrapper = ESNctsDepartureCargoDescWrapper.New(item, Factory);
			AssertContains("TestMark1, 99 - Caja1\r\nTestMark2, 88 - Caja2", wrapper.BOX311MARKSANDNUMBERS);
		}

		public void TestBOX311MARKSANDNUMBERS_Vehicles()
		{
			SetUpVehiclesForTest(item);

			var wrapper = ESNctsDepartureCargoDescWrapper.New(item, Factory);
			AssertContains("VINCODE1:BRAND1:MODEL1, 1 - BASTIDOR\r\n:BRAND2:MODEL2, 1 - BASTIDOR\r\nVINCODE3::MODEL3, 1 - BASTIDOR\r\nVINCODE4:BRAND4:, 1 - BASTIDOR", wrapper.BOX311MARKSANDNUMBERS);
		}

		void SetUpPackagesForTest(NctsDepartureCargoDesc item)
		{
			const string packageType1 = "MX";
			const string packageType2 = "BG";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetLanguage("ES", "Spanish");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UnitedNationsPackageTypes");
			var codeList1 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, packageType1, "MatchBox", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var codeList2 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, packageType2, "Bag", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListLanguage(codeList1, TranslationHelper.GetLanguageCode(Core.SharedConstants.Languages.Spanish), "Caja1");
			helper.CreateCusCodeListLanguage(codeList2, TranslationHelper.GetLanguageCode(Core.SharedConstants.Languages.Spanish), "Caja2");
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

		void SetUpVehiclesForTest(NctsDepartureCargoDesc item)
		{
			const string vehicleType = "FR";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetLanguage("ES", "Spanish");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UnitedNationsPackageTypes");
			var codeList = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, vehicleType, "Frame", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListLanguage(codeList, TranslationHelper.GetLanguageCode(Enterprise.Core.SharedConstants.Languages.Spanish), "BASTIDOR");
			Factory.Save();

			item.IsVehicles = true;

			var vehicle1 = item.Packages.AddNew();
			vehicle1.B5_PackageID = "VINCODE1";
			vehicle1.B5_Brand = "BRAND1";
			vehicle1.B5_Model = "MODEL1";

			var vehicle2 = item.Packages.AddNew();
			vehicle2.B5_PackageID = ZString.Empty;
			vehicle2.B5_Brand = "BRAND2";
			vehicle2.B5_Model = "MODEL2";

			var vehicle3 = item.Packages.AddNew();
			vehicle3.B5_PackageID = "VINCODE3";
			vehicle3.B5_Brand = ZString.Empty;
			vehicle3.B5_Model = "MODEL3";

			var vehicle4 = item.Packages.AddNew();
			vehicle4.B5_PackageID = "VINCODE4";
			vehicle4.B5_Brand = "BRAND4";
			vehicle4.B5_Model = ZString.Empty;
		}

		protected override BusinessObject GetNewBusinessObject() => ESNctsDepartureCargoDescWrapper.New(Factory.New<NctsDepartureCargoDesc>(), Factory);

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			item = header.Bills.AddNew().GoodsItems.AddNew();
		}
		NctsHeader header;
		NctsDepartureCargoDesc item;
	}
}
