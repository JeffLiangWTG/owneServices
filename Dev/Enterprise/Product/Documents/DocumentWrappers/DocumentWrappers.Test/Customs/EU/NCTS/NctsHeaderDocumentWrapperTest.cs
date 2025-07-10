using System;
using System.Linq;
using CargoWise.BrandManager;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.EU.NCTS.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.EU.NCTS.Testing
{
	[TestedType(typeof(NctsHeaderDocumentWrapper))]
	class NctsHeaderDocumentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestWrapper()
		{
			(NctsHeader header, NctsHeaderDocumentWrapper wrapper) = SetUpData();
			AssertNotNull(wrapper);
			AssertType<NctsDepartureCargoDescWrapperCollection>(wrapper.Lines);

			CombineAssertions("Edge case, New()", () =>
			{
				AssertExceptionThrown<ArgumentNullException>("Exception expected when nctsHeader parameter is null", () => NctsHeaderDocumentWrapper.New(null, Factory));
				AssertExceptionThrown<ArgumentNullException>("Exception expected when factory parameter is null", () => NctsHeaderDocumentWrapper.New(header, null));
			});
		}

		public virtual void TestWrapperProperties()
		{
			CombineAssertions(() =>
			{
				(NctsHeader header, NctsHeaderDocumentWrapper wrapper) = SetUpData();
				AssertEquals(1, wrapper.Lines.Count);
				AssertEquals(nameof(wrapper.MOVEMENTREFERENCENUMBER), "21FR00007411BBC885", wrapper.MOVEMENTREFERENCENUMBER);
				AssertEquals(nameof(wrapper.EMAILSUBJECT), "21FR00007411BBC885", wrapper.EMAILSUBJECT);
				AssertEquals(nameof(wrapper.BOX1REGIME), "T1", wrapper.BOX1REGIME);
				AssertEquals(nameof(wrapper.BOX2CONSIGNOR), "CONSIGNOR NAME\nCONSIGNOR STREET\n000000000 CONSIGNOR CITY\nFR", wrapper.BOX2CONSIGNOR);
				AssertEquals(nameof(wrapper.BOX2CONSIGNOREORI), "FR000000000000000", wrapper.BOX2CONSIGNOREORI);
				AssertEquals(nameof(wrapper.BOX15COUNTRYOFORIGIN), "DE", wrapper.BOX15COUNTRYOFORIGIN);
				AssertEquals(nameof(wrapper.BOX8CONSIGNEE), "CONSIGNEE NAME\nCONSIGNEE STREET\n111111111 CONSIGNEE CITY\nGB", wrapper.BOX8CONSIGNEE);
				AssertEquals(nameof(wrapper.BOX8CONSIGNEEEORI), "GB111111111111111", wrapper.BOX8CONSIGNEEEORI);
				AssertEquals(nameof(wrapper.BOX17COUNTRYOFDESTINATION), "CN", wrapper.BOX17COUNTRYOFDESTINATION);
				AssertEquals(nameof(wrapper.BOXCOFFICEOFDEPARTURE), "FROffice1", wrapper.BOXCOFFICEOFDEPARTURE);
				AssertEquals(nameof(wrapper.RETURNOFFICEADDRESS), "FROffice1\nDeparture Office Street\nDepartureOfficePostCode DepartureofficeCity", wrapper.RETURNOFFICEADDRESS);
				AssertEquals(nameof(wrapper.BOXCOFFICEOFDEPARTURECODE), "FR000001", wrapper.BOXCOFFICEOFDEPARTURECODE);
				AssertEquals(nameof(wrapper.BOX53OFFICEOFDESTINATION), "GB000001 (GBOffice1)", wrapper.BOX53OFFICEOFDESTINATION);
				AssertEquals(nameof(wrapper.BOX5ITEMS), "1", wrapper.BOX5ITEMS);
				AssertEquals(nameof(wrapper.BOX6PACKAGES), "1", wrapper.BOX6PACKAGES);
				AssertEquals(nameof(wrapper.BOX18DEPARTURETRANSPORTID), "510PZ47", wrapper.BOX18DEPARTURETRANSPORTID);
				AssertEquals(nameof(wrapper.BOX18DEPARTURETRANSPORTFLAG), "GR", wrapper.BOX18DEPARTURETRANSPORTFLAG);
				AssertEquals(nameof(wrapper.BOX35GROSSMASS), "30", wrapper.BOX35GROSSMASS);
				AssertEquals(nameof(wrapper.BOX38NETTMASS), "20", wrapper.BOX38NETTMASS);
				AssertEquals(nameof(wrapper.BOX50PRINCIPAL), "PRINCIPAL NAME\nPRINCIPAL STREET\n333333333 PRINCIPAL CITY\nIT", wrapper.BOX50PRINCIPAL);
				AssertEquals(nameof(wrapper.BOX50PRINCIPALEORI), "IT333333333333333", wrapper.BOX50PRINCIPALEORI);
				AssertEquals(nameof(wrapper.BOX50SIGNATURE), ZString.Empty, wrapper.BOX50SIGNATURE);
				AssertEquals(nameof(wrapper.BOX51TRANSITOFFICE1), "TR000001 (TROffice1)", wrapper.BOX51TRANSITOFFICE1);
				AssertEquals(nameof(wrapper.BOX51TRANSITOFFICE2), "TR000002 (TROffice2)", wrapper.BOX51TRANSITOFFICE2);
				AssertEquals(nameof(wrapper.BOX51TRANSITOFFICE3), "TR000003 (TROffice3)", wrapper.BOX51TRANSITOFFICE3);
				AssertEquals(nameof(wrapper.BOX51TRANSITOFFICE4), "TR000004 (TROffice4)", wrapper.BOX51TRANSITOFFICE4);
				AssertEquals(nameof(wrapper.BOX51TRANSITOFFICE5), "TR000005 (TROffice5)", wrapper.BOX51TRANSITOFFICE5);
				AssertEquals(nameof(wrapper.BOX51TRANSITOFFICE6), "TR000006 (TROffice6)", wrapper.BOX51TRANSITOFFICE6);
				AssertEquals(nameof(wrapper.BOX52GUARANTEE), "GUAR1;GUAR2;GUAR3", wrapper.BOX52GUARANTEE);
				AssertEquals(nameof(wrapper.BOX52GUARANTEEVALIDITY), "AAA,BBB,CCC,DDD", wrapper.BOX52GUARANTEEVALIDITY);
				AssertEquals(nameof(wrapper.BOX52GUARANTEECODE), "Type1,Type2,Type3", wrapper.BOX52GUARANTEECODE);
				AssertEquals(nameof(wrapper.BOXCDATE), "12/03/2021", wrapper.BOXCDATE);
				AssertEquals(nameof(wrapper.SHOWSTAMPONBOXC), false, wrapper.SHOWSTAMPONBOXC);
				AssertEquals(nameof(wrapper.BOXCDEPARTUREOFFICECOUNTRYCODE), "FR", wrapper.BOXCDEPARTUREOFFICECOUNTRYCODE);
				AssertEquals(nameof(wrapper.BOXCAUTHORIZEDCONSIGNORNAME), "", wrapper.BOXCAUTHORIZEDCONSIGNORNAME);
				AssertEquals(nameof(wrapper.BOXCAUTHORISATIONNUMBER), "123456", wrapper.BOXCAUTHORISATIONNUMBER);
				AssertEquals(nameof(wrapper.BOXCUNIQUEREFERENCENUMBER), "BH_JOBREFERENCE", wrapper.BOXCUNIQUEREFERENCENUMBER);
				AssertEquals(nameof(wrapper.BOXDRESULT), "TT", wrapper.BOXDRESULT);
				AssertEquals(nameof(wrapper.BOXDTIMELIMITDATE), "17/12/2023", wrapper.BOXDTIMELIMITDATE);
				AssertEquals(nameof(wrapper.BOXDSIGNATURE), ZString.Empty, wrapper.BOXDSIGNATURE);
				AssertContains(nameof(wrapper.EDIENTERPRISEVERSION), $"WiseTechGlobal.com - {BrandingFactory.Instance.ProductName} v", wrapper.EDIENTERPRISEVERSION);
				AssertEquals(nameof(wrapper.FALLBACKINFORMATION), "Tomorrow is New Year's Eve.", wrapper.FALLBACKINFORMATION);
				AssertEquals(nameof(wrapper.LOCALREFERENCENUMBER), "BH_JOBREFERENCE", wrapper.LOCALREFERENCENUMBER);
			});
		}

		public void TestWrapperPropertiesWhenSourceIsEmptyObject()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var wrapper = NctsHeaderDocumentWrapper.New(header, Factory);

			CombineAssertions("Test properties when Source is empty", () =>
			{
				AssertType<NctsDepartureCargoDescWrapperCollection>("Lines", wrapper.Lines);
				AssertNotNull("Lines", wrapper.Lines);

				AssertEquals(nameof(wrapper.Mrn), "", wrapper.Mrn);
				AssertEquals(nameof(wrapper.Pending_MRN), "Declaration pending MRN", wrapper.Pending_MRN);
				AssertEquals(nameof(wrapper.MOVEMENTREFERENCENUMBER), "Declaration pending MRN", wrapper.MOVEMENTREFERENCENUMBER);
				AssertEquals(nameof(wrapper.EMAILSUBJECT), "Declaration pending MRN", wrapper.EMAILSUBJECT);
				AssertEquals(nameof(wrapper.BOX1REGIME), "", wrapper.BOX1REGIME);
				AssertEquals(nameof(wrapper.BOX2CONSIGNOR), "", wrapper.BOX2CONSIGNOR);
				AssertEquals(nameof(wrapper.BOX2CONSIGNOREORI), "", wrapper.BOX2CONSIGNOREORI);
				AssertEquals(nameof(wrapper.BOX15COUNTRYOFORIGIN), "", wrapper.BOX15COUNTRYOFORIGIN);
				AssertEquals(nameof(wrapper.BOX8CONSIGNEE), "", wrapper.BOX8CONSIGNEE);
				AssertEquals(nameof(wrapper.BOX8CONSIGNEEEORI), "", wrapper.BOX8CONSIGNEEEORI);
				AssertEquals(nameof(wrapper.BOX17COUNTRYOFDESTINATION), "", wrapper.BOX17COUNTRYOFDESTINATION);
				AssertEquals(nameof(wrapper.BOXCOFFICEOFDEPARTURE), "", wrapper.BOXCOFFICEOFDEPARTURE);
				AssertEquals(nameof(wrapper.RETURNOFFICEADDRESS), "\n", wrapper.RETURNOFFICEADDRESS);
				AssertEquals(nameof(wrapper.BOXCOFFICEOFDEPARTURECODE), "", wrapper.BOXCOFFICEOFDEPARTURECODE);
				AssertEquals(nameof(wrapper.BOX53OFFICEOFDESTINATION), "", wrapper.BOX53OFFICEOFDESTINATION);
				AssertEquals(nameof(wrapper.BOX5ITEMS), "0", wrapper.BOX5ITEMS);
				AssertEquals(nameof(wrapper.BOX6PACKAGES), "0", wrapper.BOX6PACKAGES);
				AssertEquals(nameof(wrapper.BOX18DEPARTURETRANSPORTID), "", wrapper.BOX18DEPARTURETRANSPORTID);
				AssertEquals(nameof(wrapper.BOX18DEPARTURETRANSPORTFLAG), "", wrapper.BOX18DEPARTURETRANSPORTFLAG);
				AssertEquals(nameof(wrapper.BOX35GROSSMASS), "0", wrapper.BOX35GROSSMASS);
				AssertEquals(nameof(wrapper.BOX38NETTMASS), "0", wrapper.BOX38NETTMASS);
				AssertEquals(nameof(wrapper.BOX50PRINCIPAL), "", wrapper.BOX50PRINCIPAL);
				AssertEquals(nameof(wrapper.BOX50PRINCIPALEORI), "", wrapper.BOX50PRINCIPALEORI);
				AssertEquals(nameof(wrapper.BOX51TRANSITOFFICE1), "", wrapper.BOX51TRANSITOFFICE1);
				AssertEquals(nameof(wrapper.BOX51TRANSITOFFICE2), "", wrapper.BOX51TRANSITOFFICE2);
				AssertEquals(nameof(wrapper.BOX51TRANSITOFFICE3), "", wrapper.BOX51TRANSITOFFICE3);
				AssertEquals(nameof(wrapper.BOX51TRANSITOFFICE4), "", wrapper.BOX51TRANSITOFFICE4);
				AssertEquals(nameof(wrapper.BOX51TRANSITOFFICE5), "", wrapper.BOX51TRANSITOFFICE5);
				AssertEquals(nameof(wrapper.BOX51TRANSITOFFICE6), "", wrapper.BOX51TRANSITOFFICE6);
				AssertEquals(nameof(wrapper.BOX52GUARANTEE), "", wrapper.BOX52GUARANTEE);
				AssertEquals(nameof(wrapper.BOX52GUARANTEEVALIDITY), "", wrapper.BOX52GUARANTEEVALIDITY);
				AssertEquals(nameof(wrapper.BOX52GUARANTEECODE), "", wrapper.BOX52GUARANTEECODE);
				AssertEquals(nameof(wrapper.BOXCDATE), "", wrapper.BOXCDATE);
				AssertEquals(nameof(wrapper.SHOWSTAMPONBOXC), false, wrapper.SHOWSTAMPONBOXC);
				AssertEquals(nameof(wrapper.BOXCDEPARTUREOFFICECOUNTRYCODE), "", wrapper.BOXCDEPARTUREOFFICECOUNTRYCODE);
				AssertEquals(nameof(wrapper.BOXCAUTHORIZEDCONSIGNORNAME), "", wrapper.BOXCAUTHORIZEDCONSIGNORNAME);
				AssertEquals(nameof(wrapper.BOXCAUTHORISATIONNUMBER), "", wrapper.BOXCAUTHORISATIONNUMBER);
				AssertEquals(nameof(wrapper.BOXCUNIQUEREFERENCENUMBER), "", wrapper.BOXCUNIQUEREFERENCENUMBER);
				AssertEquals(nameof(wrapper.BOXDRESULT), "", wrapper.BOXDRESULT);
				AssertEquals(nameof(wrapper.BOXDTIMELIMITDATE), "", wrapper.BOXDTIMELIMITDATE);
				AssertArrayEqualsByElements(nameof(wrapper.Seals), Array.Empty<ISealID>(), wrapper.Seals.ToArray());
				AssertEquals(nameof(wrapper.BOXDSEALSAFFIXEDNUMBER), "0", wrapper.BOXDSEALSAFFIXEDNUMBER);
				AssertEquals(nameof(wrapper.BOXDSEALSIDENTITY), "", wrapper.BOXDSEALSIDENTITY);
				AssertEquals(nameof(wrapper.NOTRELEASEDWATERMARK), "NOT RELEASED", wrapper.NOTRELEASEDWATERMARK);
				AssertEquals(nameof(wrapper.BOXDCLEARANCE), "", wrapper.BOXDCLEARANCE);
				AssertEquals(nameof(wrapper.LOCALREFERENCENUMBER), "", wrapper.LOCALREFERENCENUMBER);
			});
		}

		public virtual void TestWrapperPropertiesWhenFallBackIsActive()
		{
			CombineAssertions(() =>
			{
				(NctsHeaderForDocumentWrapperTest header, NctsHeaderDocumentWrapper wrapper) = SetUpData();
				AssertEquals("fallback is not active", false, wrapper.IsFallBackActive);
				AssertEquals("MOVEMENTREFERENCENUMBER", "21FR00007411BBC885", wrapper.MOVEMENTREFERENCENUMBER);
				AssertEquals("MOVEMENTREFERENCENUMBER", "21FR00007411BBC885", wrapper.EMAILSUBJECT);

				(NctsHeaderForDocumentWrapperTest header2, NctsHeaderDocumentWrapper wrapper2) = SetUpData(true);
				AssertEquals("fallback is active", true, wrapper2.IsFallBackActive);
				AssertEquals("MOVEMENTREFERENCENUMBER is empty", "", wrapper2.MOVEMENTREFERENCENUMBER);
				AssertEquals("MOVEMENTREFERENCENUMBER", "Fallback", wrapper2.EMAILSUBJECT);
			});
		}

		public void TestBoxSealsIdentityWhenSealTypeIsCON()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_SealType = "CON";

			var headerContainers = nctsHeader.DepartureHeaderContainers;
			var cnt1 = headerContainers.AddNew();
			var cnt2 = headerContainers.AddNew();
			var cnt3 = headerContainers.AddNew();
			SetSeals(cnt1, "S1", "S2");
			SetSeals(cnt2, "", "S2");
			SetSeals(cnt3, "S5", "S6");

			var goodsItemCollection = movementHeader.GoodsItems;
			var goodsItem1 = goodsItemCollection.AddNew();
			var goodsItem2 = goodsItemCollection.AddNew();

			AddContainerToGoodsItem(goodsItem1, cnt1);
			AddContainerToGoodsItem(goodsItem2, cnt2);

			var wrapper = NctsHeaderDocumentWrapper.New(nctsHeader, Factory);
			AssertEquals(nameof(wrapper.BOXDSEALSAFFIXEDNUMBER), "2", wrapper.BOXDSEALSAFFIXEDNUMBER);
			AssertEquals(nameof(wrapper.BOXDSEALSIDENTITY), "S1; S2", wrapper.BOXDSEALSIDENTITY);

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

		public void TestBoxSealsIdentityWhenSealTypeIsPAC()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_SealType = "PAC";

			var sealsCollection = nctsHeader.Seals;
			sealsCollection.AddNew().CY_Data = "S1";
			sealsCollection.AddNew().CY_Data = "S2";
			sealsCollection.AddNew().CY_Data = "S3";
			sealsCollection.AddNew().CY_Data = "S3";
			sealsCollection.AddNew().CY_Data = "S5";
			sealsCollection.AddNew().CY_Data = "";

			var wrapper = NctsHeaderDocumentWrapper.New(nctsHeader, Factory);
			AssertEquals(nameof(wrapper.BOXDSEALSAFFIXEDNUMBER), "4", wrapper.BOXDSEALSAFFIXEDNUMBER);
			AssertEquals(nameof(wrapper.BOXDSEALSIDENTITY), "S1; S2; S3; S5", wrapper.BOXDSEALSIDENTITY);
		}

		public void TestNOTRELEASEDWATERMARK()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			var wrapper = NctsHeaderDocumentWrapper.New(nctsHeader, Factory);
			AssertEquals("[PRE-CONDITION] BM_CustomsStatus", NctsTransitStatusList.Codes.Unknown, nctsHeader.MovementHeader.BM_CustomsStatus);
			AssertEquals("When BM_CustomsStatus is Unknown", "NOT RELEASED", wrapper.NOTRELEASEDWATERMARK);

			var emptyWatermarkStatuses = new[]
			{
				NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture,
				NctsTransitStatusList.Codes.GoodsReleasedFromTransitUponArrival,
				NctsTransitStatusList.Codes.GoodsWrittenOff,
				NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit,
				NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed
			};
			foreach (var status in emptyWatermarkStatuses)
			{
				nctsHeader.MovementHeader.BM_CustomsStatus = status;
				AssertEquals($"When BM_CustomsStatus is {status}", "", wrapper.NOTRELEASEDWATERMARK);
			}

			nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationAccepted;
			AssertEquals("When BM_CustomsStatus is DAC", "NOT RELEASED", wrapper.NOTRELEASEDWATERMARK);
		}

		public void TestShouldUseSecurityNctsHeaderDocumentWrapper_Phase5()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			var wrapper = NctsHeaderDocumentWrapper.New(nctsHeader, Factory);
			var departureMovement = nctsHeader.MovementHeader;

			CombineAssertions(() =>
			{
				departureMovement.BM_TypeOfSecurity = "NON";
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				AssertEquals("BM_TypeOfSecurity=NON;BH_ApplicationCode=NC5", true, NctsHeaderDocumentWrapper.ShouldUseSecurityNctsHeaderDocumentWrapper(nctsHeader));

				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				AssertEquals("BM_TypeOfSecurity=NON;BH_ApplicationCode=NCT", false, NctsHeaderDocumentWrapper.ShouldUseSecurityNctsHeaderDocumentWrapper(nctsHeader));
			});
		}

		public void TestAggregatedValuesForPhase5()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			var bill = nctsHeader.Bills.AddNew();
			var goodsItem1 = bill.GoodsItems.AddNew();
			goodsItem1.BY_GrossWeight = 12.3m;
			var package1 = goodsItem1.Packages.AddNew();
			package1.B5_UnitCount = 123;
			var goodsItem2 = bill.GoodsItems.AddNew();
			goodsItem2.BY_GrossWeight = 34.5m;
			var package2 = goodsItem2.Packages.AddNew();
			package2.B5_UnitCount = 234;

			bill.B0_Weight = bill.GoodsItems.Sum(item => item.BY_GrossWeight);
			nctsHeader.MovementHeader.UpdateBM_GrossWeightFromBills();

			var wrapper = NctsHeaderDocumentWrapper.New(nctsHeader, Factory);
			CombineAssertions(() =>
			{
				AssertEquals(nameof(wrapper.BOX35GROSSMASS), "46.8", wrapper.BOX35GROSSMASS);
				AssertEquals(nameof(wrapper.BOX5ITEMS), "2", wrapper.BOX5ITEMS);
				AssertEquals(nameof(wrapper.BOX6PACKAGES), "357", wrapper.BOX6PACKAGES);
			});
		}

		public void TestGoodsItemsWithDeclarationItemNumber()
		{
			var header = Factory.New<NctsHeader>();

			header.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = header.Bills.AddNew();
			bill.B0_ReferenceID = "sbb1";
			bill.SequenceNumber = 1;
			var billItem1 = bill.GoodsItems.AddNew();
			billItem1.BY_LineNo = 3;
			billItem1.BY_DeclarationGoodsItemNumber = 5;
			billItem1.BY_Description = "gi2";
			var billItem2 = bill.GoodsItems.AddNew();
			billItem2.BY_LineNo = 2;
			billItem2.BY_DeclarationGoodsItemNumber = 4;
			billItem2.BY_Description = "gi1";

			Factory.Save();
			var wrapper = NctsHeaderDocumentWrapper.New(header, Factory);
			var lines = wrapper.Lines;

			AssertEquals(2, lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("BOX32ITEM is BY_DeclarationGoodsItemNumber", "4", lines[0].BOX32ITEM);
				AssertEquals("BOX314DESCRIPTION is BY_Description", "gi1", lines[0].BOX314DESCRIPTION);
				AssertEquals("BOX32ITEM is BY_DeclarationGoodsItemNumber", "5", lines[1].BOX32ITEM);
				AssertEquals("BOX314DESCRIPTION is BY_Description", "gi2", lines[1].BOX314DESCRIPTION);
			});
		}

		public void TestGoodsItemsWithoutDeclarationItemNumber()
		{
			var header = Factory.New<NctsHeader>();

			header.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = header.Bills.AddNew();
			bill.B0_ReferenceID = "sbb1";
			bill.SequenceNumber = 1;
			var billItem1 = bill.GoodsItems.AddNew();
			billItem1.BY_LineNo = 3;
			billItem1.BY_DeclarationGoodsItemNumber = 5;
			billItem1.BY_Description = "gi2";
			var billItem2 = bill.GoodsItems.AddNew();
			billItem2.BY_LineNo = 2;
			billItem2.BY_DeclarationGoodsItemNumber = 0;
			billItem2.BY_Description = "gi1";

			Factory.Save();
			var wrapper = NctsHeaderDocumentWrapper.New(header, Factory);
			var lines = wrapper.Lines;

			AssertEquals(2, lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("BOX32ITEM is a generated progressive number", "1", lines[0].BOX32ITEM);
				AssertEquals("BOX314DESCRIPTION is BY_Description", "gi1", lines[0].BOX314DESCRIPTION);
				AssertEquals("BOX32ITEM is a generated progressive number", "2", lines[1].BOX32ITEM);
				AssertEquals("BOX314DESCRIPTION is BY_Description", "gi2", lines[1].BOX314DESCRIPTION);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return NctsHeaderDocumentWrapper.New(Factory.New<NctsHeader>(), Factory);
		}

		(NctsHeaderForDocumentWrapperTest header, NctsHeaderDocumentWrapper wrapper) SetUpData(bool isFallBackActive = false)
		{
			var header = Factory.New<NctsHeaderForDocumentWrapperTest>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.IsFallBackActiveForTest = isFallBackActive;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = header.MovementHeader;
			movementHeader.BM_GrossWeight = 30;
			movementHeader.BM_InBondEntryType = "T1";
			movementHeader.BM_TransportAtDeparture = "510PZ47";
			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._9_OwnPropulsion;
			movementHeader.BM_ExportDate = new ZDateTime(2023, 12, 17);
			SetUpConsignor(header);
			SetUpConsignee(header);
			SetUpPrincipal(header);
			SetUpMrn(header);
			SetUpOffices(header);
			SetUpGuarantees(header);
			SetUpEvent(header);
			SetUpItem(movementHeader);
			SetUpLrn(header);
			movementHeader.BM_GONumber = "TT";
			movementHeader.BM_RL_NKDestinationPort = "CN";
			movementHeader.BM_RN_NKTransportAtDepartureCountry = "GR";
			movementHeader.BM_LocationOfGoodsCode = "123456";
			header.BH_RL_NKImportLoadPort = "DE";
			var wrapper = NctsHeaderDocumentWrapper.New(header, Factory);
			return (header, wrapper);
		}

		void SetUpItem(NctsDepartureMovementHeader movementHeader)
		{
			var item = movementHeader.GoodsItems.AddNew();
			item.Packages.AddNew();
			item.BY_GrossWeight = 30;
			item.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			item.BY_NetWeight = 20;
			item.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;
		}

		void SetUpEvent(NctsHeader header)
		{
			var log = header.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.CustomsEntryStatus.Code;
				log.SL_Reference = NctsTransitStatusList.Codes.DeclarationAccepted;
				log.SL_EventTime = new ZDateTime(2021, 3, 12);
			}
		}

		void SetUpGuarantees(NctsHeader header)
		{
			var guarantee1 = header.Guarantees.AddNew();
			guarantee1.PW_BondNumber = "GUAR1";
			guarantee1.PW_ValidityLimitation = "AAA BBB";
			guarantee1.PW_BondType = "Type1";
			var guarantee2 = header.Guarantees.AddNew();
			guarantee2.PW_BondNumber = "GUAR2";
			guarantee2.PW_ValidityLimitation = "CCC DDD";
			guarantee2.PW_BondType = "Type2";
			var guarantee3 = header.Guarantees.AddNew();
			guarantee3.PW_BondNumber2 = "GUAR3";
			guarantee3.PW_BondType = "Type3";
			var guarantee4 = header.Guarantees.AddNew();
			guarantee4.PW_ValidityLimitation = "EEE FFF";
			guarantee4.PW_BondNumber = "GUAR4";
			guarantee4.PW_BondType = "Type4";
		}

		void SetUpOffice(NctsHeader header, string countryCode, string officeCode, string role, string description, string street, string postCode, string city)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(countryCode, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");

			var cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_Code = officeCode;
			cusCodeList.ZZD_Description = description;
			cusCodeList.ZZD_CodeType = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
			cusCodeList.ZZD_CountryOrGrouping = countryCode;
			cusCodeList.ZZD_EndDate = ZDateTime.Today.AddDays(2);
			cusCodeList.ZZD_StartDate = ZDateTime.Today.AddDays(-2);

			if (street != ZString.Empty)
			{
				var officeAttribute = cusCodeList.Attributes.AddNew();
				officeAttribute.ZZE_ZXE_NKName = "Street";
				officeAttribute.ZZE_Value = street;
			}

			if (postCode != ZString.Empty)
			{
				var officeAttribute2 = cusCodeList.Attributes.AddNew();
				officeAttribute2.ZZE_ZXE_NKName = "PostCode";
				officeAttribute2.ZZE_Value = postCode;
			}

			if (city != ZString.Empty)
			{
				var officeAttribute3 = cusCodeList.Attributes.AddNew();
				officeAttribute3.ZZE_ZXE_NKName = "CITY";
				officeAttribute3.ZZE_Value = city;
			}

			NctsEuOfficeCode cusOffice = null;
			if (role == EuOfficeCodesTypes.Codes.OfficeOfDeparture)
			{
				cusOffice = header.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDeparture);
			}
			else if (role == EuOfficeCodesTypes.Codes.OfficeOfDestination)
			{
				cusOffice = header.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDestination);
			}

			if (cusOffice == null)
			{
				cusOffice = header.CustomsOffices.AddNew();
				cusOffice.CY_Code = role;
			}

			cusOffice.CY_Data = officeCode;
		}

		void SetUpOffices(NctsHeader header)
		{
			SetUpOffice(header, Core.Constants.CountryCodes.France, "FR000001", OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "FROffice1", "Departure Office Street", "DepartureOfficePostCode", "DepartureofficeCity");
			SetUpOffice(header, Core.Constants.CountryCodes.UnitedKingdom, "GB000001", OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination, "GBOffice1", "", "", "");
			SetUpOffice(header, Core.Constants.CountryCodes.Turkey, "TR000001", OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, "TROffice1", "", "", "");
			SetUpOffice(header, Core.Constants.CountryCodes.Turkey, "TR000002", OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, "TROffice2", "", "", "");
			SetUpOffice(header, Core.Constants.CountryCodes.Turkey, "TR000003", OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, "TROffice3", "", "", "");
			SetUpOffice(header, Core.Constants.CountryCodes.Turkey, "TR000004", OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, "TROffice4", "", "", "");
			SetUpOffice(header, Core.Constants.CountryCodes.Turkey, "TR000005", OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, "TROffice5", "", "", "");
			SetUpOffice(header, Core.Constants.CountryCodes.Turkey, "TR000006", OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, "TROffice6", "", "", "");
		}

		void SetUpPrincipal(NctsHeader header)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.MainAddress;
			orgHeader.OH_FullName = "PRINCIPAL NAME";
			address.OA_Address1 = "PRINCIPAL STREET";
			address.OA_PostCode = "333333333";
			address.OA_City = "PRINCIPAL CITY";
			address.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Italy;
			header.Principal.E2_OA_Address = address.PK;
			var eori = orgHeader.CustomsCodes.AddNew();
			eori.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			eori.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.Italy;
			eori.OK_CustomsRegNo = "33333333333333333";
		}

		void SetUpConsignee(NctsHeader header)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.MainAddress;
			orgHeader.OH_FullName = "CONSIGNEE NAME";
			address.OA_Address1 = "CONSIGNEE STREET";
			address.OA_PostCode = "111111111";
			address.OA_City = "CONSIGNEE CITY";
			address.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			header.Consignee.E2_OA_Address = address.PK;
			var eori = orgHeader.CustomsCodes.AddNew();
			eori.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			eori.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			eori.OK_CustomsRegNo = "11111111111111111";
		}

		void SetUpConsignor(NctsHeader header)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.MainAddress;
			orgHeader.OH_FullName = "CONSIGNOR NAME";
			address.OA_Address1 = "CONSIGNOR STREET";
			address.OA_PostCode = "000000000";
			address.OA_City = "CONSIGNOR CITY";
			address.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.France;
			header.Consignor.E2_OA_Address = address.PK;
			var eori = orgHeader.CustomsCodes.AddNew();
			eori.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			eori.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.France;
			eori.OK_CustomsRegNo = "00000000000000000";
		}

		void SetUpMrn(NctsHeader header)
		{
			var entryNum = CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, header.CountryCode);
			entryNum.CE_EntryNum = "21FR00007411BBC885";
		}

		void SetUpLrn(NctsHeader header)
		{
			header.LocalReferenceNumber = "BH_JOBREFERENCE";
		}

		internal static NctsHeaderDocumentWrapper NctsHeaderDocumentWrapperWithoutSecurity(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			return NctsHeaderDocumentWrapper.New(nctsHeader, factory);
		}
	}
}
