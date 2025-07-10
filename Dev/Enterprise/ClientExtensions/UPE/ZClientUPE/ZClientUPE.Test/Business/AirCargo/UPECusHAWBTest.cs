using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.BISI;
using Enterprise.Client.UPE.Business.GSSI;
using Enterprise.Client.UPE.Business.Testing;
using Enterprise.Client.UPE.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.UPE.Business.AirCargo.Testing
{
	partial class UPECusHAWBTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();

			UPEDataRegistry.Instance.EnableUPECustomisations = true;
		}

		public void TestCS_ConsigneePostcode()
		{
			var houseBill = TestHelper.MasterBill.ChildBills.AddNew();
			AssertEquals("", houseBill.CS_ConsigneePostcode);

			houseBill.CS_ConsigneePostcode = " 2067  ";
			AssertEquals("2067", houseBill.CS_ConsigneePostcode);

			Factory.Save();
			AssertEquals("2067", houseBill.CS_ConsigneePostcode);

			var orgHeader = Factory.New<UPEOrgHeader>();
			orgHeader.OH_Code = "CODE";
			orgHeader.MainAddress.OA_Address1 = "ADDRESS 1";
			orgHeader.MainAddress.OA_PostCode = "4111";
			houseBill.CS_OA_ConsigneeAddress = orgHeader.MainAddress.PK;
			AssertEquals("4111", houseBill.CS_ConsigneePostcode);

			Factory.Save();
			AssertEquals("4111", houseBill.CS_ConsigneePostcode);

			houseBill.CS_OA_ConsigneeAddress = ZGuid.Empty;
			AssertEquals("The previous postcode will be left on the UI as it's already in the database", "4111", houseBill.CS_ConsigneePostcode);

			Factory.Save();
			AssertEquals("4111", houseBill.CS_ConsigneePostcode);

			var postcode1 = new string('1', houseBill.CS_ConsigneePostcodeInfo.MaxLength);
			houseBill.CS_ConsigneePostcode = postcode1;
			AssertEquals(postcode1, houseBill.CS_ConsigneePostcode);

			Factory.Save();
			AssertEquals("If we manually enter a postcode of max length, it can be saved to the db", postcode1, houseBill.CS_ConsigneePostcode);

			var postcode2 = new string('2', houseBill.CS_ConsigneePostcodeInfo.MaxLength);
			var orgHeader2 = Factory.New<UPEOrgHeader>();
			orgHeader2.OH_Code = "CODE2";
			orgHeader2.MainAddress.OA_Address1 = "ADDRESS 2";
			orgHeader2.MainAddress.OA_PostCode = postcode2;
			houseBill.CS_OA_ConsigneeAddress = orgHeader2.MainAddress.PK;
			AssertEquals("If the postcode is from the Consignee, the UI will only display MaxLength - 1 digits due to the implementation in the base class",
				postcode2.Substring(0, postcode2.Length - 1),
				houseBill.CS_ConsigneePostcode);

			Factory.Save();
			AssertEquals("The db value is saved with MaxLength - 1 digits due to the UI implementation above",
				postcode2.Substring(0, postcode2.Length - 1),
				houseBill.CS_ConsigneePostcode);
		}

		public void TestCreateGSSIMessage_CustomQueue()
		{
			var houseBill = TestHelper.HouseBill;
			houseBill.CS_HAWB = "master";
			Factory.Save();

			var ediMessages = Factory.Load<GSSMessage>(new ZQuery { OrderBy = "EM_MessageNum" });
			AssertEquals("New aircargo job is defaulted to be on 'INV' queue with reason code 'SR' which should trigger GSSI message creation)", 1, ediMessages.Length);

			var messagesContent = ediMessages[0].EM_MessageText;
			string expectedContent = "01ERR       7340      N7340master                             03SR";
			string actualContent = messagesContent.Substring(0, 66);

			Assert("Actual (" + actualContent + ") message valid", expectedContent.Contains(actualContent));

			ediMessages.ForEach(m => m.DeleteFromTest());

			houseBill.CurrentQueue.P4_QueueName = CargoReportQueueCodeDescriptionPairList.Codes.Hold;

			houseBill.CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.Codes._R0_Rebill;
			houseBill.CurrentQueue.P4_CustomsSubStatus = StatusCodeDescriptionPairList.Codes.BR_MessageLeft;

			Factory.Save();

			ediMessages = Factory.Load<GSSMessage>(new ZQuery { OrderBy = "EM_MessageNum" });
			Assert("GSSI should export reason code with 2 characters only.", ediMessages.Length == 0);

			UPEJobRelatedWayBill.CreateJobRelatedWaybill(Factory, houseBill.PK, houseBill.CS_HAWB, "short#", UPEJobRelatedWayBill.Constants.RelatedWayBillType.Parent);
			UPEJobRelatedWayBill.CreateJobRelatedWaybill(Factory, houseBill.PK, "ChildBill1", ZString.Empty, UPEJobRelatedWayBill.Constants.RelatedWayBillType.Child);
			UPEJobRelatedWayBill.CreateJobRelatedWaybill(Factory, houseBill.PK, "ChildBill2", ZString.Empty, UPEJobRelatedWayBill.Constants.RelatedWayBillType.Child);

			houseBill.CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.Codes.XH_DocumentInsufficient;
			houseBill.CurrentQueue.P4_CustomsSubStatus = StatusCodeDescriptionPairList.Codes.BR_MessageLeft;
			Factory.Save();

			ediMessages = Factory.Load<GSSMessage>(new ZQuery { OrderBy = "EM_MessageNum" });
			var messagesContents = ediMessages.Select(x => x.EM_MessageText).ToArray();

			foreach (string s in messagesContents)
			{
				string actualContents = s.Substring(0, 66);
				Assert("Actual (" + actualContents + ") message valid", GssiExpectedCustomsMessages.Contains(actualContents));
				GssiExpectedCustomsMessages.Remove(actualContents);
			}
			Assert("All expected messages found", GssiExpectedCustomsMessages.Count == 0);

			ediMessages.ForEach(m => m.DeleteFromTest());

			var houseBill2 = TestHelper.CreateCusHAWB("master", CargoReportQueueCodeDescriptionPairList.Codes.AwaitingDeclaration, "", DeclarationQueueCodeDescriptionPairList.Codes.Pending);
			UPEJobRelatedWayBill.CreateJobRelatedWaybill(Factory, houseBill2.PK, houseBill2.CS_HAWB, "short#", UPEJobRelatedWayBill.Constants.RelatedWayBillType.Parent);
			UPEJobRelatedWayBill.CreateJobRelatedWaybill(Factory, houseBill2.PK, "ChildBill1", ZString.Empty, UPEJobRelatedWayBill.Constants.RelatedWayBillType.Child);

			Factory.Save();

			AssertEquals("No GSSI Message is generated", 0, Factory.Load<GSSMessage>(new ZQuery()).Length);

			houseBill2.CurrentQueue.P4_QueueName = DefaultQueueCodeDescriptionPairList.Codes.Hold;
			houseBill2.CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.Codes.S1_ShipperConsigneeDetailsInsufficient;

			Factory.Save();

			messagesContents = Factory.Load<GSSMessage>(new ZQuery { OrderBy = "EM_MessageNum" }).Select(x => x.EM_MessageText).ToArray();

			foreach (string s in messagesContents)
			{
				string actualContents = s.Substring(0, 66);
				Assert("Actual (" + actualContents + ") message valid", GssiExpectedCustomsMessagesForSplit.Contains(actualContents));
				GssiExpectedCustomsMessagesForSplit.Remove(actualContents);
			}
			Assert("All expected messages found", GssiExpectedCustomsMessagesForSplit.Count == 0);

			var anotherFactory = new BusinessObjectFactory();
			TestHelper.AssertContainsHoldExportLog(houseBill2.PK, anotherFactory, "S1", "master");
			TestHelper.AssertContainsHoldExportLog(houseBill2.PK, anotherFactory, "S1", "ChildBill1");
			TestHelper.AssertContainsHoldExportLog(houseBill.PK, anotherFactory, "S1", "ChildBill2", houseBill2.PK.ToString());
		}

		public void TestCreateGSSIMessage_FinanceQueue()
		{
			var houseBill = TestHelper.HouseBill;
			houseBill.CS_HAWB = "master";
			houseBill.CurrentQueue.P4_CustomsQueue = CargoReportQueueCodeDescriptionPairList.Codes.Completed;
			Factory.Save();

			houseBill.CurrentQueue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Hold;
			houseBill.CurrentQueue.P4_SubStatus = AutoStatusCodeDescriptionPairList.Codes.BR_MessageLeft;
			houseBill.CurrentQueue.P4_Status = ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment;

			houseBill.CurrentQueue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Hold;
			houseBill.CurrentQueue.P4_SubStatus = AutoStatusCodeDescriptionPairList.Codes.BR_MessageLeft;
			houseBill.CurrentQueue.P4_Status = ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment;

			UPEJobRelatedWayBill.CreateJobRelatedWaybill(Factory, houseBill.PK, houseBill.CS_HAWB, "short#", UPEJobRelatedWayBill.Constants.RelatedWayBillType.Parent);
			UPEJobRelatedWayBill.CreateJobRelatedWaybill(Factory, houseBill.PK, "ChildBill1", ZString.Empty, UPEJobRelatedWayBill.Constants.RelatedWayBillType.Child);
			UPEJobRelatedWayBill.CreateJobRelatedWaybill(Factory, houseBill.PK, "ChildBill2", ZString.Empty, UPEJobRelatedWayBill.Constants.RelatedWayBillType.Child);

			Factory.Save();

			var messages = Factory.Load<GSSMessage>(new ZQuery { OrderBy = "EM_MessageNum" }).Select(x => x.EM_MessageText).ToArray();

			foreach (string s in messages)
			{
				string actualContents = s.Substring(0, 66);
				Assert("Actual (" + actualContents + ") message valid", GssiExpectedFinanceMessages.Contains(actualContents));
				GssiExpectedFinanceMessages.Remove(actualContents);
			}
			Assert("All expected messages found", GssiExpectedFinanceMessages.Count == 0);
		}

		List<string> GssiExpectedFinanceMessages
		{
			get
			{
				if (gssiExpectedFinanceMessages == null)
				{
					gssiExpectedFinanceMessages = new List<string>(3);
					gssiExpectedFinanceMessages.Add("01ERR       7340      N7340master                             03OQ");  // Parent house bill.
					gssiExpectedFinanceMessages.Add("01ERR       7340      N7340ChildBill1                         03OQ");  // Child house bill.
					gssiExpectedFinanceMessages.Add("01ERR       7340      N7340ChildBill2                         03OQ");  // JobRelatedWayBill virtual only.
				}
				return gssiExpectedFinanceMessages;
			}
		}
		List<string> gssiExpectedFinanceMessages;

		List<string> GssiExpectedCustomsMessages
		{
			get
			{
				if (gssiExpectedCustomsMessages == null)
				{
					gssiExpectedCustomsMessages = new List<string>(3);
					gssiExpectedCustomsMessages.Add("01ERR       7340      N7340master                             03XH");  // Parent house bill.
					gssiExpectedCustomsMessages.Add("01ERR       7340      N7340ChildBill2                         03XH");  // Child house bill.
					gssiExpectedCustomsMessages.Add("01ERR       7340      N7340ChildBill1                         03XH");  // JobRelatedWayBill virtual only.
				}
				return gssiExpectedCustomsMessages;
			}
		}
		List<string> gssiExpectedCustomsMessages;

		List<string> GssiExpectedCustomsMessagesForSplit
		{
			get
			{
				if (gssiExpectedCustomsMessagesForSplit == null)
				{
					gssiExpectedCustomsMessagesForSplit = new List<string>(4);
					gssiExpectedCustomsMessagesForSplit.Add("01ERR       7340      N7340master                             03S1");  // Parent house bill. (avoid duplicate 
					gssiExpectedCustomsMessagesForSplit.Add("01ERR       7340      N7340ChildBill2                         03S1");  // Child house bill.
					gssiExpectedCustomsMessagesForSplit.Add("01ERR       7340      N7340ChildBill1                         03S1");  // JobRelatedWayBill virtual only.
				}
				return gssiExpectedCustomsMessagesForSplit;
			}
		}
		List<string> gssiExpectedCustomsMessagesForSplit;

		public void TestCustomisationsLoadedForRegistry()
		{
			ErrorReporter.Clear();
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			Factory.New<UPECusHAWB>();
			AssertEquals("", ErrorReporter.LastMessageReported);
		}

		public void TestLookups()
		{
			AssertEquals("Invalid lookups", typeof(UPECusHAWBLookups), TestHelper.HouseBill.Lookups.GetType());
		}

		public void TestValidation()
		{
			Assert("Has to contain " + nameof(UPECusHAWBValidation), TestHelper.HouseBill.Validation.ContainsPiggybackedValidation(typeof(UPECusHAWBValidation)));
		}

		#region Business Object Overrides

		public void TestNoteTypeCollection()
		{
			AssertCollectionContains(UPEPredefinedNoteTypes.Instance.DeliveryInstructionsNote, TestHelper.HouseBill.NoteTypes);
			AssertCollectionContains(UPEPredefinedNoteTypes.Instance.Level1Record, TestHelper.HouseBill.NoteTypes);
			AssertCollectionContains(UPEPredefinedNoteTypes.Instance.CRNote, TestHelper.HouseBill.NoteTypes);
		}

		public void TestDelete_CascadeDeletesPrintBatchItems()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();

			var printBatch = new UPEPrintBatch.Loader(TestHelper.SharedFactory).CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.TaxInvoice);
			var item = printBatch.PrintItems.AddNew(new UPEDocumentMenuItemLoader(TestHelper.SharedFactory).LoadUPSTaxInvoice(), TestHelper.HouseBill);

			TestHelper.HouseBill.Delete();
			AssertEquals("All attached print batch items should be deleted", true, item.IsDeleted);
		}

		public void TestCurrentQueueInitialisedInConstructor()
		{
			AssertEquals("Wrong initial queue", CargoReportQueueCodeDescriptionPairList.Codes.Intervention, TestHelper.HouseBill.CurrentQueue.P4_CustomsQueue);
			AssertEquals("Wrong reason code", ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, TestHelper.HouseBill.CurrentQueue.P4_CustomsStatus);

			AssertEquals("Should not have changes", false, TestHelper.HouseBill.CurrentQueue.HasChanges);
			Factory.Save();

			TestHelper.HouseBill.CurrentQueue.P4_CustomsQueue = CargoReportQueueCodeDescriptionPairList.Codes.EIR;
			Factory.Save();

			UPECusHAWB newHouseBill = TestHelper.SharedFactory.Load<UPECusHAWB>(TestHelper.HouseBill.PK);
			AssertEquals("Should not be re-initialised once the Queue is persisted in DB", CargoReportQueueCodeDescriptionPairList.Codes.EIR, newHouseBill.CurrentQueue.P4_CustomsQueue);
		}

		public void TestDeliveryInstructionsNote()
		{
			TestHelper.HouseBill.Notes.AddNew(false, PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, "TestNotes");
			AssertEquals("TestNotes", TestHelper.HouseBill.DeliveryInstructionsNote.ST_NoteText);
		}

		#endregion

		#region Try Complete Commercial Queue on Upload

		public void TestCreateHeldForPaymentLogToSendToBISI_ChargesAboveCODConfirmPaymentThreshold()
		{
			UPECusHAWBWithDummyLocalCharges localHAWB = GetNewCusHAWBForBISIUploadQueueMovingTest();
			localHAWB.SetTotalLocalCharges(UPEDataRegistry.Instance.CODConfirmPaymentThreshold + 1);

			((IBisiUpload)localHAWB).OnBeforeBisiUpload();
			AssertCommercialQueue("Should NOT complete commercial queue for this test", localHAWB.CurrentQueue, string.Empty, string.Empty, string.Empty);
			AssertLastCommercialQueueChangeLog("Should add the held for payment queue log", localHAWB, string.Empty, ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment, string.Empty);

			// The following lines are temporarily commented out to highlight a previous feature that was also reversed out but needs to be revisited.
			//UPEDataRegistry.Instance.AllowReplaceBYWithGENewFunctionalityItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			//AssertLastCommercialQueueChangeLog("Should add the held for payment queue log", LocalHAWB, string.Empty, ResolutionCodeDescriptionPairList.Codes.DA_Released, string.Empty);

			//((IShipmentData)LocalHAWB).DateCusHAWBOrDeclarationUploadedToBISI = ZDateTime.Now;
			//AssertCommercialQueue("Should NOT complete commercial queue for this test", LocalHAWB.CurrentQueue, string.Empty, string.Empty, string.Empty);
			//AssertLastCommercialQueueChangeLog("Should add the held for payment queue log", LocalHAWB, string.Empty, "GE", string.Empty);
		}

		public void TestCreateHeldForPaymentLogToSendToBISI_ChargesBelowCODConfirmPaymentThreshold_AccountClass10()
		{
			UPECusHAWBWithDummyLocalCharges localHAWB = GetNewCusHAWBForBISIUploadQueueMovingTest();
			localHAWB.CS_JE_CustomsFormalEntry = Factory.New<UPEJobDeclaration>().PK;
			localHAWB.Declaration.JE_OH_Importer = Factory.New<UPEOrgHeader>().PK;
			localHAWB.Declaration.Importer.CompanyData.OB_OJ_ARDebtorGroup = Factory.New<OrgDebtorGroup>().PK;
			localHAWB.Declaration.Importer.CompanyData.ARDebtorGroup.OJ_Code = "10";

			localHAWB.SetTotalLocalCharges(UPEDataRegistry.Instance.CODConfirmPaymentThreshold + 1);

			((IBisiUpload)localHAWB).OnBeforeBisiUpload();
			AssertCommercialQueue("Should NOT complete commercial queue for this test", localHAWB.CurrentQueue, string.Empty, string.Empty, string.Empty);
			AssertLastCommercialQueueChangeLog("Should add the held for payment queue log", localHAWB, string.Empty, ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment, string.Empty);

			// The following lines are temporarily commented out to highlight a previous feature that was also reversed out but needs to be revisited.
			//UPEDataRegistry.Instance.AllowReplaceBYWithGENewFunctionalityItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			//AssertLastCommercialQueueChangeLog("Should add the held for payment queue log", LocalHAWB, string.Empty, ResolutionCodeDescriptionPairList.Codes.DA_Released, string.Empty);

			//((IShipmentData)LocalHAWB).DateCusHAWBOrDeclarationUploadedToBISI = ZDateTime.Now;
			//AssertCommercialQueue("Should NOT complete commercial queue for this test", LocalHAWB.CurrentQueue, string.Empty, string.Empty, string.Empty);
			//AssertLastCommercialQueueChangeLog("Should add the held for payment queue log", LocalHAWB, string.Empty, "GE", string.Empty);
		}

		public void TestCreateHeldForPaymentLogToSendToBISI_ChargesBelowCODConfirmPaymentThreshold_AccountClass2()
		{
			UPECusHAWBWithDummyLocalCharges localHAWB = GetNewCusHAWBForBISIUploadQueueMovingTest();
			localHAWB.CS_JE_CustomsFormalEntry = Factory.New<UPEJobDeclaration>().PK;
			localHAWB.Declaration.JE_OH_Importer = Factory.New<UPEOrgHeader>().PK;
			localHAWB.Declaration.Importer.CompanyData.OB_OJ_ARDebtorGroup = Factory.New<OrgDebtorGroup>().PK;
			localHAWB.Declaration.Importer.CompanyData.ARDebtorGroup.OJ_Code = "2";

			localHAWB.SetTotalLocalCharges(UPEDataRegistry.Instance.CODConfirmPaymentThreshold + 1);

			((IBisiUpload)localHAWB).OnBeforeBisiUpload();
			AssertCommercialQueue("Should complete commercial queue for this test", localHAWB.CurrentQueue, CommercialQueueCodeDescriptionPairList.Codes.Completed, string.Empty, string.Empty);
			Assert("Should not add the held for payment queue log", localHAWB.CurrentQueue.CommercialQueueLogs.GetLogsDescendinglySortedByEventTime().Length == 0);
		}

		#endregion

		#region IsHoldForCollection

		public void TestIsHoldForCollection()
		{
			UPECusHAWB houseBill = (UPECusHAWB)TestHelper.MasterBill.ChildBills.AddNew();
			Assert(!houseBill.IsHoldForCollection);

			houseBill.CurrentQueue.P4_CustomFlag5 = true;
			houseBill.HoldForCollectDepot = HFCDepotCodeDescriptionPairList.Codes.Sydney;
			houseBill.HFCContactName = "John";
			houseBill.HFCContactPhoneNumber = "02 90251100";
			AssertEquals(HFCDepotCodeDescriptionPairList.Codes.Sydney, houseBill.HoldForCollectDepot);
			AssertEquals("ContactName", "John", houseBill.HFCContactName);
			AssertEquals("ContactPhoneNumber", "02 90251100", houseBill.HFCContactPhoneNumber);
			Assert(houseBill.IsHoldForCollection);

			houseBill.IsHoldForCollection = false;
			Assert(!houseBill.IsHoldForCollection);
			Assert(!houseBill.CurrentQueue.P4_CustomFlag5);
			AssertEquals(string.Empty, houseBill.HoldForCollectDepot);
			AssertEquals("ContactName should be empty", string.Empty, houseBill.HFCContactName);
			AssertEquals("ContactPhoneNumber should be empty", string.Empty, houseBill.HFCContactPhoneNumber);
			AssertEquals("InnerInfo should be " + houseBill.CurrentQueue.P4_CustomFlag5Info.Name, houseBill.CurrentQueue.P4_CustomFlag5Info, ((ZWrappedPropertyInfo)houseBill.IsHoldForCollectionInfo).InnerInfo);
			TestHelper.MasterBill.ChildBills.Remove(houseBill.PK);
		}

		public void TestIsHoldForCollection_ReadOnly_WithAlternateBroker()
		{
			UPECusHAWB houseBill = (UPECusHAWB)TestHelper.MasterBill.ChildBills.AddNew();
			houseBill.CS_JE_CustomsFormalEntry = TestHelper.SharedFactory.NewWithValidTestData<BaseJobDeclaration>().PK;
			houseBill.Declaration.JE_OH_Importer = TestHelper.SharedFactory.NewWithValidTestData<OrgHeader>().PK;

			AssertEquals("!ReadOnly if no alternate broker", false, houseBill.IsHoldForCollectionInfo.ReadOnly);

			UPEOrgHeader alternateBroker = TestHelper.SharedFactory.NewWithValidTestData<UPEOrgHeader>();
			houseBill.Declaration.Importer.SetRelatedParty(alternateBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, ZString.Empty);

			alternateBroker.IsDeliveryHandledByUPSForThisAlternateBroker = true;
			AssertEquals("!ReadOnly if there is an alternate broker that handles delivery", false, houseBill.IsHoldForCollectionInfo.ReadOnly);

			alternateBroker.IsDeliveryHandledByUPSForThisAlternateBroker = false;
			AssertEquals("!ReadOnly if there is an alternate broker that handles delivery", true, houseBill.IsHoldForCollectionInfo.ReadOnly);
			TestHelper.MasterBill.ChildBills.Remove(houseBill.PK);
		}

		public void TestHoldForCollectDepot()
		{
			TestHelper.HouseBill.HoldForCollectDepot = HFCDepotCodeDescriptionPairList.Codes.Sydney;
			AssertEquals(HFCDepotCodeDescriptionPairList.Codes.Sydney, TestHelper.HouseBill.HoldForCollectDepot);
			AssertEquals(1m, TestHelper.HouseBill.CurrentQueue.P4_CustomDecimal3);

			TestHelper.HouseBill.HoldForCollectDepot = HFCDepotCodeDescriptionPairList.Codes.Melbourne;
			AssertEquals(HFCDepotCodeDescriptionPairList.Codes.Melbourne, TestHelper.HouseBill.HoldForCollectDepot);
			AssertEquals(2m, TestHelper.HouseBill.CurrentQueue.P4_CustomDecimal3);

			TestHelper.HouseBill.HoldForCollectDepot = HFCDepotCodeDescriptionPairList.Codes.Brisbane;
			AssertEquals(HFCDepotCodeDescriptionPairList.Codes.Brisbane, TestHelper.HouseBill.HoldForCollectDepot);
			AssertEquals(3m, TestHelper.HouseBill.CurrentQueue.P4_CustomDecimal3);

			TestHelper.HouseBill.HoldForCollectDepot = HFCDepotCodeDescriptionPairList.Codes.Perth;
			AssertEquals(HFCDepotCodeDescriptionPairList.Codes.Perth, TestHelper.HouseBill.HoldForCollectDepot);
			AssertEquals(4m, TestHelper.HouseBill.CurrentQueue.P4_CustomDecimal3);

			TestHelper.HouseBill.HoldForCollectDepot = HFCDepotCodeDescriptionPairList.Codes.Adelaide;
			AssertEquals(HFCDepotCodeDescriptionPairList.Codes.Adelaide, TestHelper.HouseBill.HoldForCollectDepot);
			AssertEquals(5m, TestHelper.HouseBill.CurrentQueue.P4_CustomDecimal3);

			TestHelper.HouseBill.HoldForCollectDepot = HFCDepotCodeDescriptionPairList.Codes.Tasmania;
			AssertEquals(HFCDepotCodeDescriptionPairList.Codes.Tasmania, TestHelper.HouseBill.HoldForCollectDepot);
			AssertEquals(6m, TestHelper.HouseBill.CurrentQueue.P4_CustomDecimal3);

			TestHelper.HouseBill.HoldForCollectDepot = string.Empty;
			AssertEquals(string.Empty, TestHelper.HouseBill.HoldForCollectDepot);
			AssertEquals(0m, TestHelper.HouseBill.CurrentQueue.P4_CustomDecimal3);
		}

		public void TestContactName()
		{
			TestHelper.HouseBill.HFCContactName = "Gregory";
			AssertEquals("ContactName", "Gregory", TestHelper.HouseBill.HFCContactName);
			AssertEquals(UPEProcessQueue.Schema.P4_CustomAttrib10, "Gregory", TestHelper.HouseBill.CurrentQueue.P4_CustomAttrib10);
		}

		public void TestContactPhoneNumber()
		{
			TestHelper.HouseBill.HFCContactPhoneNumber = "02 9025 1100";
			AssertEquals("ContactPhoneNmber", "02 9025 1100", TestHelper.HouseBill.HFCContactPhoneNumber);
			AssertEquals(UPEProcessQueue.Schema.P4_CustomAttrib9, "02 9025 1100", TestHelper.HouseBill.CurrentQueue.P4_CustomAttrib9);
		}

		public void TestZPropertyInfoReadonlyDependingOnIsHoldForCollection()
		{
			TestHelper.HouseBill.IsHoldForCollection = true;
			Assert("HoldForCollectDepotInfo.ReadOnly", !TestHelper.HouseBill.HoldForCollectDepotInfo.ReadOnly);
			Assert(UPEProcessQueue.Schema.P4_CustomDecimal3 + "Info.Readonly", !TestHelper.HouseBill.CurrentQueue.P4_CustomDecimal3Info.ReadOnly);

			Assert("ContactNameInfo.ReadOnly", !TestHelper.HouseBill.HFCContactNameInfo.ReadOnly);
			Assert(UPEProcessQueue.Schema.P4_CustomAttrib10 + "Info.ReadOnly", !TestHelper.HouseBill.CurrentQueue.P4_CustomAttrib10Info.ReadOnly);

			Assert("ContactPhoneNumberInfo.Readonly", !TestHelper.HouseBill.HFCContactPhoneNumberInfo.ReadOnly);
			Assert(UPEProcessQueue.Schema.P4_CustomAttrib9 + "Info.ReadOnly", !TestHelper.HouseBill.CurrentQueue.P4_CustomAttrib9Info.ReadOnly);

			TestHelper.HouseBill.IsHoldForCollection = false;
			Assert("HoldForCollectDepotInfo.ReadOnly", TestHelper.HouseBill.HoldForCollectDepotInfo.ReadOnly);
			Assert(UPEProcessQueue.Schema.P4_CustomDecimal3 + "Info.Readonly", TestHelper.HouseBill.CurrentQueue.P4_CustomDecimal3Info.ReadOnly);

			Assert("ContactNameInfo.ReadOnly", TestHelper.HouseBill.HFCContactNameInfo.ReadOnly);
			Assert(UPEProcessQueue.Schema.P4_CustomAttrib10 + "Info.ReadOnly", TestHelper.HouseBill.CurrentQueue.P4_CustomAttrib10Info.ReadOnly);

			Assert("ContactPhoneNumberInfo.Readonly", TestHelper.HouseBill.HFCContactPhoneNumberInfo.ReadOnly);
			Assert(UPEProcessQueue.Schema.P4_CustomAttrib9 + "Info.ReadOnly", TestHelper.HouseBill.CurrentQueue.P4_CustomAttrib9Info.ReadOnly);
		}

		public void TestEditLogReferenceWhenIsHoldForCollectChanges_One()
		{
			CargoWise.Data.Db.Connection.BeginTransaction();
			AssertEditLogReferenceForIsHoldForCollect("Event not raised initially", false);
			TestHelper.HouseBill.OnSaving();
			TestHelper.SharedFactory.Save();
			AssertEditLogReferenceForIsHoldForCollect("Event not raised initially after first save and no changes", false);
			CargoWise.Data.Db.Connection.CommitTransaction();
		}

		public void TestEditLogReferenceWhenIsHoldForCollectChanges_Two()
		{
			CargoWise.Data.Db.Connection.BeginTransaction();
			TestHelper.HouseBill.IsHoldForCollection = false;
			TestHelper.SharedFactory.Save();

			TestHelper.HouseBill.IsHoldForCollection = true;
			AssertEditLogReferenceForIsHoldForCollect("Event not raised until save", false);
			TestHelper.HouseBill.OnSaving();
			TestHelper.SharedFactory.Save();
			AssertEditLogReferenceForIsHoldForCollect("Event raised for false->true after save", true);
			CargoWise.Data.Db.Connection.CommitTransaction();
		}

		public void TestEditLogReferenceWhenIsHoldForCollectChanges_Three()
		{
			CargoWise.Data.Db.Connection.BeginTransaction();
			TestHelper.HouseBill.IsHoldForCollection = true;
			TestHelper.SharedFactory.Save();

			TestHelper.HouseBill.IsHoldForCollection = true;
			AssertEditLogReferenceForIsHoldForCollect("Event not raised until save", false);
			TestHelper.HouseBill.OnSaving();
			TestHelper.SharedFactory.Save();
			AssertEditLogReferenceForIsHoldForCollect("Event not raised for true->true after save", false);
			CargoWise.Data.Db.Connection.CommitTransaction();
		}

		public void TestEditLogReferenceWhenIsHoldForCollectChanges_Four()
		{
			CargoWise.Data.Db.Connection.BeginTransaction();
			TestHelper.HouseBill.IsHoldForCollection = true;
			TestHelper.SharedFactory.Save();

			TestHelper.HouseBill.IsHoldForCollection = false;
			AssertEditLogReferenceForIsHoldForCollect("Event not raised until save", false);
			TestHelper.HouseBill.OnSaving();
			TestHelper.SharedFactory.Save();
			AssertEditLogReferenceForIsHoldForCollect("Event raised for true->false after save", true);
			CargoWise.Data.Db.Connection.CommitTransaction();
		}

		void AssertEditLogReferenceForIsHoldForCollect(string message, bool expectEvent)
		{
			StmALog lastEditLog = TestHelper.HouseBill.Logs.MostRecentLogByEventTime(Events.EditedARecord);
			AssertEquals(message, expectEvent, lastEditLog != null && lastEditLog.SL_Reference.IndexOf("HFC") != -1);
		}

		#endregion

		#region Split Shipments

		public void TestSplitShipments()
		{
			Assert("Isn't split shipment", !TestHelper.HouseBill.IsSplitShipment);
			TestHelper.HouseBill.SetSplitShipment();
			TestHelper.HouseBill.Factory.Save();
			Assert("Is now split shipment", TestHelper.HouseBill.IsSplitShipment);
		}

		#endregion

		public void TestChildJobRelatedWayBills()
		{
			Assert("Pre condition", TestHelper.HouseBill.ChildJobRelatedWayBills.Count == 0);
			TestHelper.NewJobRelatedWayBillsWithValidTestData();
			Assert("Now has job related way bills", TestHelper.HouseBill.ChildJobRelatedWayBills.Count != 0);
		}

		static void AssertLastCommercialQueueChangeLog(ZString message, UPECusHAWB hAWB, ZString queueName, ZString reason, ZString status)
		{
			ProcessQueueLog lastQueueChangeLog = hAWB.CurrentQueue.CommercialQueueLogs.GetLogsDescendinglySortedByEventTime()[0];
			AssertEquals(message, queueName, lastQueueChangeLog.Queue);
			AssertEquals(message, reason, lastQueueChangeLog.Status);
			AssertEquals(message, status, lastQueueChangeLog.SubStatus);
		}

		UPECusHAWBWithDummyLocalCharges GetNewCusHAWBForBISIUploadQueueMovingTest()
		{
			UPECusHAWBWithDummyLocalCharges result = Factory.New<UPECusHAWBWithDummyLocalCharges>();
			result.CurrentQueue.P4_QueueName = string.Empty;
			result.CurrentQueue.P4_Status = string.Empty;
			result.CurrentQueue.P4_SubStatus = string.Empty;
			return result;
		}

		static void AssertCommercialQueue(string errorMessage, ProcessQueue queue, ZString expectedQueueName, ZString expectedReasonCode, ZString expectedStatusCode)
		{
			AssertEquals("Has been moved to the wrong Queue." + errorMessage, expectedQueueName, queue.P4_QueueName);
			AssertEquals("Queue has been moved with the wrong reason code." + errorMessage, expectedReasonCode, queue.P4_Status);
			AssertEquals("Queue has been moved with the wrong status code." + errorMessage, expectedStatusCode, queue.P4_SubStatus);
		}

		UPETestHelper TestHelper
		{
			get { return testHelper ?? (testHelper = new UPETestHelper(Factory)); }
		}
		UPETestHelper testHelper;

		public class UPECusHAWBWithDummyLocalCharges : UPECusHAWB, IShipmentData
		{
			public UPECusHAWBWithDummyLocalCharges(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public void SetTotalLocalCharges(ZDecimal totalLocalCharges)
			{
				fChargesData = new ShipmentChargeData[] { new ShipmentChargeData(ShipmentChargeTypeCode.GST, totalLocalCharges, Core.Constants.CurrencyCodes.Australia) };
				AssertEquals("Sanity check", totalLocalCharges, this.TotalLocalCharges);
			}

			IReadOnlyList<CommodityDetailData> IShipmentData.CommoditiesData
			{
				get { return System.Array.Empty<CommodityDetailData>(); }
			}

			IReadOnlyList<ShipmentChargeData> IShipmentData.ChargesData
			{
				get { return fChargesData; }
			}
			ShipmentChargeData[] fChargesData;
		}
	}
}
