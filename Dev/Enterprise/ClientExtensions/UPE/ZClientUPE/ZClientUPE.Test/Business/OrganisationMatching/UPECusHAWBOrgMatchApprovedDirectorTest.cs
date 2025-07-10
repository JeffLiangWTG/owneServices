using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business.Testing
{
	sealed class UPECusHAWBOrgMatchApprovedDirectorTest : TestCaseWithClientSpecificDocuments
	{
		public void TestRunMatchApprovedActivities_WithConsigneeAndConsignor()
		{
			AirCargo.CS_GoodsDescription = "Goods description";
			MatchApprovalDirector.RunMatchApprovedActivities();
			AssertDeclarationWithGoodsDescriptionCreated("No declaration should be created initially for the test", false, "Goods description");
			AirCargo.RequiresConsigneeMatch = true;
			AirCargo.UPEConsigneeAddress.P3_OH_MatchOrg = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			MatchApprovalDirector.RunMatchApprovedActivities();
			AssertDeclarationWithGoodsDescriptionCreated("No declaration should be created yet without both importer/consignee and consignor", false, "Goods description");
			AirCargo.CS_OA_ConsignorAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			MatchApprovalDirector.RunMatchApprovedActivities();
			AssertDeclarationWithGoodsDescriptionCreated("Declaration should be created now that both consignee and consignor are populated", true, "Goods description");
			Assert(!AirCargo.CS_JE_CustomsFormalEntry.IsEmpty);
		}

		public void TestRunMatchApprovedActivities_IfCusHAWBAlreadyHasADec()
		{
			AirCargo.CS_GoodsDescription = "Goods description";
			AirCargo.CS_JE_CustomsFormalEntry = Factory.NewWithValidTestData<JobDeclaration>().PK;
			MatchApprovalDirector.RunMatchApprovedActivities();
			AirCargo.RequiresConsigneeMatch = true;
			AirCargo.UPEConsigneeAddress.P3_OH_MatchOrg = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			MatchApprovalDirector.RunMatchApprovedActivities();
			AssertDeclarationWithGoodsDescriptionCreated("No declaration should be created yet without both importer/consignee and consignor", false, "Goods description");
			AirCargo.CS_OA_ConsignorAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			MatchApprovalDirector.RunMatchApprovedActivities();
			AssertDeclarationWithGoodsDescriptionCreated("No declaration should be created initially for the test", false, "Goods description");
		}

		public void TestRunMatchApprovedActivities_WithImporterAndConsignor()
		{
			AirCargo.CS_GoodsDescription = "Goods description";
			MatchApprovalDirector.RunMatchApprovedActivities();
			AssertDeclarationWithGoodsDescriptionCreated("No declaration should be created initially for the test", false, "Goods description");
			AirCargo.CS_OH_Consignor = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			CreateAndApproveAirCargoImporter(AirCargo);
			MatchApprovalDirector.RunMatchApprovedActivities();
			AssertDeclarationWithGoodsDescriptionCreated("Declaration should be created now that both importer and consignor are populated", false, "Goods description");
			Assert(AirCargo.CS_JE_CustomsFormalEntry.IsEmpty);
		}

		public void TestRunMatchApprovedActivities_SplitShipment()
		{
			AirCargo.MAWB.CM_MAWB = "08111111111";
			AirCargo.CS_HAWB = "HOUSEBILL";
			// test precondition
			MatchApprovalDirector.RunMatchApprovedActivities();
			Assert("Pre-condition", AirCargo.CS_JE_CustomsFormalEntry.IsEmpty);
			// create duplicate Air Cargo
			CusMAWB cusMAWB = Factory.New<CusMAWB>();
			cusMAWB.CM_MAWB = "23211111111";
			UPECusHAWB duplicateAirCargo = Factory.New<UPECusHAWB>();
			duplicateAirCargo.CS_CM = cusMAWB.PK;
			duplicateAirCargo.CS_HAWB = "HOUSEBILL";
			_ = (CusHAWBConsigneeConsignorMatchApproval)Loader.LoadOrCreate(duplicateAirCargo.PK, OrgMatchApprovalType.AirCargoConsignee);
			duplicateAirCargo.CS_OA_ConsignorAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			duplicateAirCargo.UPEConsigneeAddress.P3_OH_MatchOrg = Factory.NewWithValidTestData<OrgHeader>().PK;
			// create 1st declaration
			AirCargo.CS_OA_ConsignorAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			AirCargo.UPEConsigneeAddress.P3_OH_MatchOrg = Factory.NewWithValidTestData<OrgHeader>().PK;
			Factory.Save();
			MatchApprovalDirector.RunMatchApprovedActivities();
			Assert("Declaration Created", !AirCargo.CS_JE_CustomsFormalEntry.IsEmpty);
			AssertEquals(2, AirCargo.Declaration.Bills.Count);
			AssertNotNull(AirCargo.Declaration.Bills.FindByBillNumberAndType("08111111111", Enterprise.Customs.Business.BillTypeList.Codes.MasterBill));
			AssertNotNull(AirCargo.Declaration.Bills.FindByBillNumberAndType("HOUSEBILL", Enterprise.Customs.Business.BillTypeList.Codes.HouseBill));
			// this should be identified as a split shipment
			MatchApprovalDirector = new UPECusHAWBOrgMatchApprovedDirector(duplicateAirCargo);
			MatchApprovalDirector.RunMatchApprovedActivities();
			AssertEquals("A new dec should not be created", AirCargo.CS_JE_CustomsFormalEntry, duplicateAirCargo.CS_JE_CustomsFormalEntry);
			AssertEquals(3, duplicateAirCargo.Declaration.Bills.Count);
			AssertEquals("08111111111", duplicateAirCargo.Declaration.Bills[0].CU_MasterBill);
			AssertEquals("08111111111", duplicateAirCargo.Declaration.Bills[1].CU_MasterBill);
			AssertEquals("23211111111", duplicateAirCargo.Declaration.Bills[2].CU_MasterBill);
			AssertEquals("Masterbill Added = 23211111111", duplicateAirCargo.Declaration.Logs.MostRecentLogByEventTime(Events.EditedARecord).SL_Reference);
			Assert(duplicateAirCargo.Declaration.CurrentQueue.P4_CustomsQueue != DeclarationQueueCodeDescriptionPairList.Codes.Lodgement);
		}

		public void TestRunMatchApprovedActivities_SplitShipment_WithQueueMovement()
		{
			AirCargo.MAWB.CM_MAWB = "08111111111";
			AirCargo.CS_HAWB = "HOUSEBILL";
			// test precondition
			MatchApprovalDirector.RunMatchApprovedActivities();
			Assert("Pre-condition", AirCargo.CS_JE_CustomsFormalEntry.IsEmpty);
			// create 1st declaration
			AirCargo.CS_OA_ConsignorAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			AirCargo.UPEConsigneeAddress.P3_OH_MatchOrg = Factory.NewWithValidTestData<OrgHeader>().PK;
			MatchApprovalDirector.RunMatchApprovedActivities();
			Assert("Declaration Created", !AirCargo.CS_JE_CustomsFormalEntry.IsEmpty);
			AssertEquals(2, AirCargo.Declaration.Bills.Count);
			AssertNotNull(AirCargo.Declaration.Bills.FindByBillNumberAndType("08111111111", Enterprise.Customs.Business.BillTypeList.Codes.MasterBill));
			AssertNotNull(AirCargo.Declaration.Bills.FindByBillNumberAndType("HOUSEBILL", Enterprise.Customs.Business.BillTypeList.Codes.HouseBill));
			AirCargo.Declaration.JE_EntryStatus = CMRImportEntryAdvice.Finalised.Code;
			AssertEquals(DeclarationQueueCodeDescriptionPairList.Codes.Completed, AirCargo.Declaration.CurrentQueue.P4_CustomsQueue);
			// create duplicate Air Cargo
			CusMAWB cusMAWB = Factory.New<CusMAWB>();
			cusMAWB.CM_MAWB = "23211111111";
			UPECusHAWB duplicateAirCargo = Factory.New<UPECusHAWB>();
			duplicateAirCargo.CS_CM = cusMAWB.PK;
			duplicateAirCargo.CS_HAWB = "HOUSEBILL";
			_ = (CusHAWBConsigneeConsignorMatchApproval)Loader.LoadOrCreate(duplicateAirCargo.PK, OrgMatchApprovalType.AirCargoConsignee);
			MatchApprovalDirector = new UPECusHAWBOrgMatchApprovedDirector(duplicateAirCargo);
			duplicateAirCargo.CS_OA_ConsignorAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			duplicateAirCargo.UPEConsigneeAddress.P3_OH_MatchOrg = Factory.NewWithValidTestData<OrgHeader>().PK;
			Factory.Save();
			// this should be identified as a split shipment
			MatchApprovalDirector.RunMatchApprovedActivities();
			AssertEquals("A new dec should not be created", AirCargo.CS_JE_CustomsFormalEntry, duplicateAirCargo.CS_JE_CustomsFormalEntry);
			AssertEquals(3, duplicateAirCargo.Declaration.Bills.Count);
			Customs.Business.Bill bill1 = duplicateAirCargo.Declaration.Bills.FindByBillNumberAndType("08111111111", Enterprise.Customs.Business.BillTypeList.Codes.MasterBill);
			AssertNotNull(bill1);
			AssertEquals(1, bill1.ChildBills.Count);
			AssertEquals("HOUSEBILL", bill1.ChildBills[0].CU_BillNum);
			Customs.Business.Bill bill2 = duplicateAirCargo.Declaration.Bills.FindByBillNumberAndType("23211111111", Enterprise.Customs.Business.BillTypeList.Codes.MasterBill);
			AssertNotNull(bill2);
			AssertEquals(0, bill2.ChildBills.Count);
			ZQuery masterBillAddedFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code);
			masterBillAddedFilter.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "Masterbill Added =");
			StmALog[] masterBillAddedLogs = duplicateAirCargo.Declaration.Logs.Find(masterBillAddedFilter);
			AssertEquals(1, masterBillAddedLogs.Length);
			AssertEquals("Masterbill Added = 23211111111", masterBillAddedLogs[0].SL_Reference);
			AssertEquals(DeclarationQueueCodeDescriptionPairList.Codes.BCO, duplicateAirCargo.Declaration.CurrentQueue.P4_CustomsQueue);
			AssertEquals(ReasonCodeDescriptionPairList.Codes.DN_SplitShipment, duplicateAirCargo.Declaration.CurrentQueue.P4_CustomsStatus);
		}

		public void TestRunMatchApprovedActivities_SplitShipment_DeliverAlternateBrokerSplitNotification()
		{
			UPECusHAWB airCargo1 = NewAirCargoWithApprovedConsignor("08111111111", "HOUSEBILL");
			UPECusHAWB airCargo2 = NewAirCargoWithApprovedConsignor("23211111111", "HOUSEBILL");
			UPECusHAWBOrgMatchApprovedDirector matchApprovalDirector1 = new UPECusHAWBOrgMatchApprovedDirector(airCargo1);
			UPECusHAWBOrgMatchApprovedDirector matchApprovalDirector2 = new UPECusHAWBOrgMatchApprovedDirector(airCargo2);
			CreateAndApproveAirCargoImporter(airCargo1);
			airCargo1.UPEImporterAddress.MatchOrg.SetRelatedParty(Factory.NewWithValidTestData<OrgHeader>(), RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, ZString.Empty);
			matchApprovalDirector1.RunMatchApprovedActivities();
			Assert("Declaration created", !airCargo1.CS_JE_CustomsFormalEntry.IsEmpty);
			AssertAlternateBrokerSplitNotificationDelivered("Alternate Broker Split Notification should not yet be delivered", airCargo1, false);
			CreateAndApproveAirCargoImporter(airCargo2);
			Factory.Save();
			matchApprovalDirector2.RunMatchApprovedActivities();
			AssertAlternateBrokerSplitNotificationDelivered("Alternate Broker Split Notification should be delivered until Factory.Save", airCargo2, false);
			Factory.Save();
			AssertAlternateBrokerSplitNotificationDelivered("Alternate Broker Split Notification should be delivered after Factory.Save", airCargo2, true);
		}

		public void TestCalculateFreightRateOnDeclaration()
		{
			CreateLevelOneTariff();
			AirCargo.CS_GoodsDescription = "Goods description";
			AirCargo.CS_RL_NKOrigin = "USLAX";
			AirCargo.CS_RL_NKDestination = "AUSYD";
			AirCargo.MAWB.CM_RL_NKDischargePort = "AUSYD";
			AirCargo.MAWB.CM_RL_NKLoadPort = "USLAX";
			AirCargo.CS_RS_NK_ServiceLevel = "STD";
			AirCargo.CS_Weight = 15m;
			AirCargo.CS_WeightUQ = Core.Constants.Weight.Kilograms;
			AirCargo.Level1Record = new DataImport.Level1FileFormat.Level1Record();
			AirCargo.Level1Record.AddRecordLine("US3295AU9639050704              DAT2773T8Z8W5110001   EA G/RIDGE B/L BANJO BOLT 7/16-24                                                                          1217      USD300                 US8714190060          NLR            AU14138                                                                                                                                            ");
			AirCargo.CS_OA_ConsignorAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			AirCargo.CS_OA_ConsigneeAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			AirCargo.UPEConsigneeAddress.P3_OH_MatchOrg = AirCargo.Consignee.PK;
			AirCargo.Consignee.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			MatchApprovalDirector.RunMatchApprovedActivities();
			AssertEquals(2, AirCargo.Declaration.JobComInvoiceGroupHeaders[0].Charges.Count); //OFT
			Customs.Business.BaseJobComInvHeaderCharge baseJobComInvHeaderCharge = AirCargo.Declaration.JobComInvoiceGroupHeaders[0].Charges[AUChargeCodeList.Codes.OverseasFreight];
			AssertEquals(Core.Constants.CurrencyCodes.Australia, baseJobComInvHeaderCharge.J7_RX_NKCurrency);
			AssertEquals(AUChargeCodeList.Codes.OverseasFreight, baseJobComInvHeaderCharge.J7_ChargeType);
			AssertEquals(1425m, baseJobComInvHeaderCharge.J7_Amount);
		}

		public void TestCalculateFreightRateOnDeclaration_No500000Lines_MeansNoInvoiceHeader()
		{
			CreateLevelOneTariff();
			AirCargo.CS_GoodsDescription = "Goods description";
			AirCargo.CS_RL_NKOrigin = "USLAX";
			AirCargo.CS_RL_NKDestination = "AUSYD";
			AirCargo.MAWB.CM_RL_NKDischargePort = "AUSYD";
			AirCargo.MAWB.CM_RL_NKLoadPort = "USLAX";
			AirCargo.CS_RS_NK_ServiceLevel = "STD";
			AirCargo.CS_Weight = 15m;
			AirCargo.CS_WeightUQ = Core.Constants.Weight.Kilograms;
			AirCargo.CS_OA_ConsignorAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			AirCargo.CS_OA_ConsigneeAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			AirCargo.UPEConsigneeAddress.P3_OH_MatchOrg = AirCargo.Consignee.PK;
			AirCargo.Consignee.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			MatchApprovalDirector.RunMatchApprovedActivities();
			AssertEquals(1, AirCargo.Declaration.JobComInvoiceGroupHeaders[0].Charges.Count); //OFT
			Customs.Business.BaseJobComInvHeaderCharge baseJobComInvHeaderCharge = AirCargo.Declaration.JobComInvoiceGroupHeaders[0].Charges[AUChargeCodeList.Codes.OverseasFreight];
			AssertEquals(Core.Constants.CurrencyCodes.Australia, baseJobComInvHeaderCharge.J7_RX_NKCurrency);
			AssertEquals(AUChargeCodeList.Codes.OverseasFreight, baseJobComInvHeaderCharge.J7_ChargeType);
			AssertEquals(1425m, baseJobComInvHeaderCharge.J7_Amount);
		}

		void CreateLevelOneTariff()
		{
			CompanyTariff levelOneTariff = Helper.NewCompanyTariff();
			RateEntry aIREntry1 = levelOneTariff.AddRateEntry("AIR", "LSE", "US", "AU", "STD", "");
			aIREntry1.TI_RX_NKCurrency = "AUD";
			RateLine aIRRateLine1 = aIREntry1.RateLines[0];
			aIRRateLine1.RateLineItems.RemoveAndDeleteAll();
			aIRRateLine1.Calculator["-10"] = (ZDecimal)100m;
			aIRRateLine1.Calculator["+10"] = (ZDecimal)95m;
			levelOneTariff.Factory.Save();
		}

		#region Implementation
		UPECusHAWB NewAirCargoWithApprovedConsignor(ZString mAWB, ZString hAWB)
		{
			UPECusHAWB result = Factory.NewWithValidTestData<UPECusHAWB>();
			result.CS_CM = Factory.NewWithValidTestData<CusMAWB>().PK;
			result.MAWB.CM_MAWB = mAWB;
			result.CS_HAWB = hAWB;
			result.CS_OA_ConsignorAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			return result;
		}

		void AssertAlternateBrokerSplitNotificationDelivered(ZString message, UPECusHAWB airCargo, bool expectDocumentDelivered)
		{
			UPEPrintBatchItem printItem = new UPEPrintBatchItem.Loader(Factory).LoadPrintBatchItem(UPEPrintBatchTypes.Codes.AlternateBrokerSplitNotification, airCargo.PK, AlternateBrokerSplitNotificationDocument.PK);
			AssertEquals(message, expectDocumentDelivered, printItem != null);
		}

		void CreateAndApproveAirCargoImporter(UPECusHAWB airCargo)
		{
			OrgPatternMatchAddress importer = Factory.New<OrgPatternMatchAddress>();
			importer.P3_ParentID = airCargo.PK;
			importer.P3_AddressType = OrgPatternMatchAddress.Constants.AddressType.AirCargoImporter;
			OrgHeader importerOrg = Factory.NewWithValidTestData<OrgHeader>();
			importer.P3_OH_MatchOrg = importerOrg.PK;
		}

		void AssertDeclarationWithGoodsDescriptionCreated(string message, bool expectDeclarationToBeFound, string goodsDescription)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(JobDeclarationSchema.JE_GoodsDescription, goodsDescription);
			Customs.Business.BaseJobDeclaration declarationFound = Factory.LoadTop1<Customs.Business.BaseJobDeclaration>(filter);
			AssertEquals(message, expectDeclarationToBeFound, declarationFound != null);
		}

		DocumentCommand AlternateBrokerSplitNotificationDocument
		{
			get
			{
				if (fAlternateBrokerSplitNotificationDocument == null)
				{
					fAlternateBrokerSplitNotificationDocument = new UPEDocumentMenuItemLoader(Factory).LoadAlternateBrokerSplitNotification();
				}

				return fAlternateBrokerSplitNotificationDocument;
			}
		}

		DocumentCommand fAlternateBrokerSplitNotificationDocument;
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			Loader = new OrgMatchApproval.Loader(Factory);
			CusMAWB cusMAWB = Factory.New<CusMAWB>();
			cusMAWB.CM_MAWB = "08111111111";
			AirCargo = Factory.New<UPECusHAWB>();
			AirCargo.CS_CM = cusMAWB.PK;
			_ = (CusHAWBConsigneeConsignorMatchApproval)Loader.LoadOrCreate(AirCargo.PK, OrgMatchApprovalType.AirCargoConsignee);
			MatchApprovalDirector = new UPECusHAWBOrgMatchApprovedDirector(AirCargo);
			Factory.Save();
		}

		OrgMatchApproval.Loader Loader;
		UPECusHAWB AirCargo;
		UPECusHAWBOrgMatchApprovedDirector MatchApprovalDirector;
		TestHelper Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = new TestHelper(Factory);
				}

				return fHelper;
			}
		}

		TestHelper fHelper;
		#endregion
	}
}
