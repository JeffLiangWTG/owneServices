using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.DocumentWrappers;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;
using ESNctsHeader = Enterprise.Customs.ES.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.ES.DocumentWrappers.NCTS.Testing
{
	[TestedType(typeof(SecurityNctsHeaderDocumentWrapper))]
	sealed class SecurityNctsHeaderDocumentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestNew()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

			AssertExceptionThrown<ArgumentNullException>("Expected exception when NctsHeader parameter is null", () => SecurityNctsHeaderDocumentWrapper.New(null, Factory));
			AssertExceptionThrown<ArgumentNullException>("Expected exception when Factory parameter is null", () => SecurityNctsHeaderDocumentWrapper.New(header, null));

			AssertNotNull("Instance of SecurityNctsHeaderDocumentWrapper expected", SecurityNctsHeaderDocumentWrapper.New(header, Factory));
		}

		public void TestBOX5ITEMSFormat()
		{
			var header = Factory.New<ESNctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var moveHeader = header.MovementHeader;

			var item1 = moveHeader.GoodsItems.AddNew();
			var item2 = moveHeader.GoodsItems.AddNew();

			CombineAssertions(() =>
			{
				var wrapper = SecurityNctsHeaderDocumentWrapper.New(header, Factory);
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX5ITEMS) + " is 2", "2", wrapper.BOX5ITEMS);

				for (int i = 0; i <= 1000; i++)
				{
					moveHeader.GoodsItems.AddNew();
				}
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX5ITEMS) + " is 1003", "1.003", wrapper.BOX5ITEMS);
			});
		}

		public void TestBOX6PACKAGESFormat()
		{
			var header = Factory.New<ESNctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var moveHeader = header.MovementHeader;

			var item1 = moveHeader.GoodsItems.AddNew();
			item1.Packages.AddNew();

			var item2 = moveHeader.GoodsItems.AddNew();
			item2.Packages.AddNew();

			CombineAssertions(() =>
			{
				var wrapper = SecurityNctsHeaderDocumentWrapper.New(header, Factory);
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX6PACKAGES) + " is 2", "2", wrapper.BOX6PACKAGES);

				for (int i = 0; i <= 1000; i++)
				{
					item1.Packages.AddNew();
				}
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX6PACKAGES) + " is 1003", "1.003", wrapper.BOX6PACKAGES);
			});
		}

		public void TestBOX35GROSSMASSFormat()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var moveHeader = header.MovementHeader;
			var item1 = moveHeader.GoodsItems.AddNew();
			item1.BY_GrossWeight = 30.1;
			item1.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;

			var item2 = moveHeader.GoodsItems.AddNew();
			item2.BY_GrossWeight = 30.2;
			item2.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;

			var wrapper = SecurityNctsHeaderDocumentWrapper.New(header, Factory);

			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
							Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
				{
					moveHeader.BM_GrossWeight = 40.123456789m;
					AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX35GROSSMASS) + " is 40,123457 when gross mass is declared in moveHeader and final period", "40,123457", wrapper.BOX35GROSSMASS);

					moveHeader.BM_GrossWeight = 0.03m;
					AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX35GROSSMASS) + " is 0,030000 when gross mass is declared in moveHeader and final period", "0,030000", wrapper.BOX35GROSSMASS);

					moveHeader.BM_GrossWeight = 0m;
					AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX35GROSSMASS) + " is 0,000000 when gross mass is 0 in moveHeader (even when there is gross weight in items) and final period", "0,000000", wrapper.BOX35GROSSMASS);
				}

				moveHeader.GoodsItems.DeleteAll();

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
							Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
				{
					moveHeader.BM_GrossWeight = 40.123456789m;
					AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX35GROSSMASS) + " is 40,123 when gross mass is declared in moveHeader and transition period", "40,123", wrapper.BOX35GROSSMASS);

					moveHeader.BM_GrossWeight = 0.03m;
					AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX35GROSSMASS) + " is 0,030 when gross mass is declared in moveHeader and transition period", "0,030", wrapper.BOX35GROSSMASS);

					moveHeader.BM_GrossWeight = 0m;
					AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX35GROSSMASS) + " is 0,000 when gross mass is 0 in moveHeader (even when there is gross weight in items) and transition period", "0,000", wrapper.BOX35GROSSMASS);
				}
			});
		}

		public void TestBOX35GROSSMASS()
		{
			var header = Factory.New<ESNctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var moveHeader = header.MovementHeader;
			var item = moveHeader.GoodsItems.AddNew();

			var wrapper = SecurityNctsHeaderDocumentWrapper.New(header, Factory);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
							Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
			{
				moveHeader.BM_GrossWeight = 1.1;
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX35GROSSMASS) + " is 1,100000 when gross mass is 1.1 and final period", "1,100000", wrapper.BOX35GROSSMASS);

				moveHeader.BM_GrossWeight = 0.01;
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX35GROSSMASS) + " is 0,010000 when gross mass is 0.01 and final period", "0,010000", wrapper.BOX35GROSSMASS);

				moveHeader.BM_GrossWeight = 100.01;
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX35GROSSMASS) + " is 100,010000 when gross mass is 100.01 and final period", "100,010000", wrapper.BOX35GROSSMASS);

				moveHeader.BM_GrossWeight = 30.1234m;
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX35GROSSMASS) + " is 30.123400 when final period", "30,123400", wrapper.BOX35GROSSMASS);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				moveHeader.BM_GrossWeight = 1.1;
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX35GROSSMASS) + " is 1,100 when gross mass is 1.1 and transition period", "1,100", wrapper.BOX35GROSSMASS);

				moveHeader.BM_GrossWeight = 0.01;
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX35GROSSMASS) + " is 0,010 when gross mass is 0.01 and transition period", "0,010", wrapper.BOX35GROSSMASS);

				moveHeader.BM_GrossWeight = 100.01;
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX35GROSSMASS) + " is 100,010 when gross mass is 100.01 and transition period", "100,010", wrapper.BOX35GROSSMASS);

				moveHeader.BM_GrossWeight = 30.1234m;
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX35GROSSMASS) + " is 30.123 when transition period", "30,123", wrapper.BOX35GROSSMASS);
			}
		}

		public void TestBOX38NETTMASSFormat()
		{
			var header = Factory.New<ESNctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var moveHeader = header.MovementHeader;

			var item1 = moveHeader.GoodsItems.AddNew();
			item1.BY_NetWeight = 30;
			item1.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;

			var item2 = moveHeader.GoodsItems.AddNew();
			item2.BY_NetWeight = 30;
			item2.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;

			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
							Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
				{
					var wrapper = SecurityNctsHeaderDocumentWrapper.New(header, Factory);
					AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX38NETTMASS) + " is 60,000000 when final period", "60,000000", wrapper.BOX38NETTMASS);

					item1.BY_NetWeight = 3000;
					AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX38NETTMASS) + " is 3030,000000 when final period", "3.030,000000", wrapper.BOX38NETTMASS);

					item1.BY_NetWeight = 30.1234m;
					AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX38NETTMASS) + " is 60.123400 when final period", "60,123400", wrapper.BOX38NETTMASS);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
							Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
				{
					item1.BY_NetWeight = 30;
					var wrapper = SecurityNctsHeaderDocumentWrapper.New(header, Factory);
					AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX38NETTMASS) + " is 60,000 when transition period", "60,000", wrapper.BOX38NETTMASS);

					item1.BY_NetWeight = 3000;
					AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX38NETTMASS) + " is 3030,000 when transition period", "3.030,000", wrapper.BOX38NETTMASS);

					item1.BY_NetWeight = 30.1234m;
					AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX38NETTMASS) + " is 60.123 when transition period", "60,123", wrapper.BOX38NETTMASS);
				}
			});
		}

		public void TestBOXDSEALSAFFIXEDNUMBERFormat()
		{
			var header = Factory.New<ESNctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var moveHeader = header.MovementHeader;
			moveHeader.BM_SealType = "CON";

			var headerContainers = header.DepartureHeaderContainers;
			var cnt1 = headerContainers.AddNew();
			var cnt2 = headerContainers.AddNew();
			SetSeals(cnt1, "S1", "S2");
			SetSeals(cnt2, "", "S2");

			var item1 = moveHeader.GoodsItems.AddNew();
			var item2 = moveHeader.GoodsItems.AddNew();

			AddContainerToGoodsItem(item1, cnt1);
			AddContainerToGoodsItem(item2, cnt2);

			CombineAssertions(() =>
			{
				var wrapper = SecurityNctsHeaderDocumentWrapper.New(header, Factory);
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOXDSEALSAFFIXEDNUMBER) + " is 2", "2", wrapper.BOXDSEALSAFFIXEDNUMBER);

				for (int i = 0; i <= 1000; i++)
				{
					var cnt = headerContainers.AddNew();
					SetSeals(cnt, "SA" + i, "");
					AddContainerToGoodsItem(item1, cnt);
				}
				wrapper = SecurityNctsHeaderDocumentWrapper.New(header, Factory);
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOXDSEALSAFFIXEDNUMBER) + " is 1003", "1.003", wrapper.BOXDSEALSAFFIXEDNUMBER);
			});

			void SetSeals(NctsDepartureHeaderContainer container, ZString seal1, ZString seal2)
			{
				container.BC_Seal1 = seal1;
				container.BC_Seal2 = seal2;
			}

			void AddContainerToGoodsItem(NctsDepartureCargoDesc goodsItem, NctsDepartureHeaderContainer container)
			{
				var cntPivot = goodsItem.ContainersPivots.AddNew();
				cntPivot.Container = container;
				cntPivot.ContainerSelected = true;
			}
		}

		public void TestBOXS28SEALSNUMBERFormat()
		{
			var header = Factory.New<ESNctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var moveHeader = header.MovementHeader;
			moveHeader.BM_SealQty = 2;

			CombineAssertions(() =>
			{
				var wrapper = SecurityNctsHeaderDocumentWrapper.New(header, Factory);
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOXS28SEALSNUMBER) + " is 2", "2", wrapper.BOXS28SEALSNUMBER);

				moveHeader.BM_SealQty = 1000;
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOXS28SEALSNUMBER) + " is 1000", "1.000", wrapper.BOXS28SEALSNUMBER);
			});
		}

		public void TestBOXCOFFICEOFDEPARTURE()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			var countryCode = Core.Constants.CountryCodes.Spain;
			helper.CreateNewOrGetExistingDataGrouping(countryCode, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");

			var officeCode = "ES002801";
			var description = "Office Description";

			var cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_Code = officeCode;
			cusCodeList.ZZD_Description = description;
			cusCodeList.ZZD_CodeType = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
			cusCodeList.ZZD_CountryOrGrouping = countryCode;
			cusCodeList.ZZD_EndDate = ZDateTime.Today.AddDays(2);
			cusCodeList.ZZD_StartDate = ZDateTime.Today.AddDays(-2);

			var header = Factory.New<ESNctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			header.CustomsOffices.RemoveAndDeleteAll();

			CombineAssertions(() =>
			{
				var wrapper = SecurityNctsHeaderDocumentWrapper.New(header, Factory);
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOXCOFFICEOFDEPARTURE) + " returns empty string when no DEP customs office exists", ZString.Empty, wrapper.BOXCOFFICEOFDEPARTURE);

				var cusOffice = header.CustomsOffices.AddNew();
				cusOffice.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
				cusOffice.CY_Data = officeCode;
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOXCOFFICEOFDEPARTURE) + " shows office's description and code when DEP customs office exists but mrn doesn't", description + "\n" + officeCode, wrapper.BOXCOFFICEOFDEPARTURE);

				var entryNum = CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, header.CountryCode);
				entryNum.CE_EntryNum = "21ES00999912345678";
				entryNum.CE_IssueDate = new ZDateTime(2022, 02, 16);
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOXCOFFICEOFDEPARTURE) + " shows office's description, code and arrival date when DEP customs office and mrn exist and mrn has issue date", description + "\n" + officeCode + "\n16-02-2022", wrapper.BOXCOFFICEOFDEPARTURE);

				header.CustomsOffices.RemoveAndDeleteAll();
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOXCOFFICEOFDEPARTURE) + " shows arrival date mrn exists and has issue date but no DEP customs office exists", "\n16-02-2022", wrapper.BOXCOFFICEOFDEPARTURE);
			});
		}

		public void TestBOXCDATEFormat()
		{
			var header = Factory.New<ESNctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			CombineAssertions(() =>
			{
				var wrapper = SecurityNctsHeaderDocumentWrapper.New(header, Factory);
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOXCDATE) + " returns empty string when there is no event", ZString.Empty, wrapper.BOXCDATE);

				SetUpEvent(header);
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOXCDATE) + " returns empty string even if there is an event", ZString.Empty, wrapper.BOXCDATE);
			});

			void SetUpEvent(ESNctsHeader nctsHeader)
			{
				var log = nctsHeader.Logs.AddNew();
				using (log.LockForUpdatingKeyFieldsForTesting())
				{
					log.SL_SE_NKEvent = Events.CustomsEntryStatus.Code;
					log.SL_Reference = NctsTransitStatusList.Codes.DeclarationAccepted;
					log.SL_EventTime = new ZDateTime(2021, 3, 12);
				}
			}
		}

		public void TestBOXDRESULT()
		{
			var header = Factory.New<ESNctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			CombineAssertions(() =>
			{
				var wrapper = SecurityNctsHeaderDocumentWrapper.New(header, Factory);
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOXDRESULT) + " returns empty string when no clearance criteria is set", ZString.Empty, wrapper.BOXDRESULT);

				header.ClearanceCriteria = "A2";
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOXDRESULT) + " shows the correct description when clearance criteria is A2", "A2 Normal Procedure", wrapper.BOXDRESULT);

				header.ClearanceCriteria = "A3";
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOXDRESULT) + " shows the correct description when clearance criteria is A3", "A3 Simplified Procedure", wrapper.BOXDRESULT);

				header.ClearanceCriteria = "A1";
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOXDRESULT) + " returns empty string when clearance criteria is not A2 or A3", ZString.Empty, wrapper.BOXDRESULT);
			});
		}

		public void TestBOXDTIMELIMITDATE()
		{
			var header = Factory.New<ESNctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			CombineAssertions(() =>
			{
				var wrapper = SecurityNctsHeaderDocumentWrapper.New(header, Factory);
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOXDTIMELIMITDATE) + " returns empty string when no csv clearance is set", ZString.Empty, wrapper.BOXDTIMELIMITDATE);

				var entryNum = CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.Spain.ClearanceCSV, header.CountryCode);
				entryNum.CE_EntryNum = "B9026422646FE2AB";
				entryNum.CE_IssueDate = new ZDateTime(2022, 02, 16);
				entryNum.CE_ExpiryDate = new ZDateTime(2025, 02, 16);
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOXDTIMELIMITDATE) + " shows the correct date when a csv clearance number is set with expiry date", "16-02-2025", wrapper.BOXDTIMELIMITDATE);

				entryNum.CE_ExpiryDate = ZDateTime.Empty;
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOXDTIMELIMITDATE) + " returns empty string when  a csv clearance number is set with empty expiry date", ZString.Empty, wrapper.BOXDTIMELIMITDATE);
			});
		}

		public void TestBOXDCLEARANCE()
		{
			var header = Factory.New<ESNctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			CombineAssertions(() =>
			{
				var wrapper = SecurityNctsHeaderDocumentWrapper.New(header, Factory);
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOXDCLEARANCE) + " returns only expected Spanish text when no csv clearance is set", "----------------------------------------------------------------------------------------------------\nAutentificación", wrapper.BOXDCLEARANCE);

				var entryNum = CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.Spain.ClearanceCSV, header.CountryCode);
				entryNum.CE_EntryNum = "B9026422646FE2AB";
				entryNum.CE_IssueDate = new ZDateTime(2022, 02, 16);
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOXDCLEARANCE) + " shows the Spanish text and the csv clrarance code when a csv clearance number is set", "----------------------------------------------------------------------------------------------------\nAutentificación: B9026422646FE2AB", wrapper.BOXDCLEARANCE);
			});
		}

		public void TestBOX30LOCATIONOFGOODS()
		{
			var header = Factory.New<ESNctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var moveHeader = header.MovementHeader;

			CombineAssertions(() =>
			{
				var wrapper = SecurityNctsHeaderDocumentWrapper.New(header, Factory);
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX30LOCATIONOFGOODS) + " is empty", ZString.Empty, wrapper.BOX30LOCATIONOFGOODS);

				moveHeader.BM_LocationOfGoodsCode = "LOCATIONCODE";
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX30LOCATIONOFGOODS) + " has the correct value", "LOCATIONCODE", wrapper.BOX30LOCATIONOFGOODS);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return SecurityNctsHeaderDocumentWrapper.New(Factory.New<NctsHeader>(), Factory);
		}

		public void TestLinesCollectionType()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.Bills.AddNew().GoodsItems.AddNew();
			var wrapper = new SecurityNctsHeaderDocumentWrapperForTest(header);
			AssertType<ESNctsDepartureCargoDescWrapperCollection>("The returned lines collection is ESNctsDepartureCargoDescWrapperCollection", wrapper.GetLinesCore_Exposed());
		}

		class SecurityNctsHeaderDocumentWrapperForTest : SecurityNctsHeaderDocumentWrapper
		{
			public SecurityNctsHeaderDocumentWrapperForTest(NctsHeader nctsHeader) : base(nctsHeader, nctsHeader.Factory)
			{
			}

			public DocBaseWrapperCollection<Enterprise.DocumentWrappers.Customs.EU.NCTS.NctsDepartureCargoDescWrapper> GetLinesCore_Exposed() => base.GetLinesCore();
		}
	}
}
