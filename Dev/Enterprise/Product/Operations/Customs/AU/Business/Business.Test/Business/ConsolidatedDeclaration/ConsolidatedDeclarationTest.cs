using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.AU;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(ConsolidatedDeclaration))]
	sealed class ConsolidatedDeclarationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetConsolidatedEntryMemberID() => CombineAssertions(() =>
		{
			consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(new BusinessObjectFactory(), 2);
			var leadDeclaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
			var leadDeclarationEntryHeader = leadDeclaration.EntryHeader;
			var otherDeclaration1 = (JobDeclaration)consolidatedDeclaration.JobDeclarations[1];
			var otherDeclaration1EntryHeader = otherDeclaration1.EntryHeader;
			var otherDeclaration2 = (JobDeclaration)consolidatedDeclaration.JobDeclarations[2];
			var otherDeclaration2EntryHeader = otherDeclaration2.EntryHeader;
			AssertEquals("leadDeclarationEntryHeader ConsolidatedEntryMemberID initially 0", ZShort.Zero, leadDeclarationEntryHeader.ConsolidatedEntryMemberID);
			AssertEquals("otherDeclaration1EntryHeader ConsolidatedEntryMemberID initially 0", ZShort.Zero, otherDeclaration1EntryHeader.ConsolidatedEntryMemberID);
			AssertEquals("otherDeclaration2EntryHeader ConsolidatedEntryMemberID initially 0", ZShort.Zero, otherDeclaration2EntryHeader.ConsolidatedEntryMemberID);

			var message = consolidatedDeclaration.Factory.New<CMRIMDRMessage>();
			consolidatedDeclaration.Messages.Add(message);
			using (Env.SetTemporaryUserContext(new UserContext(User.ServiceUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				consolidatedDeclaration.SyncStatusAfterMessageProcessing(message);
				AssertEquals("leadDeclarationEntryHeader - DeclarationNumber is empty", ZShort.Zero, leadDeclarationEntryHeader.ConsolidatedEntryMemberID);
				AssertEquals("otherDeclaration1EntryHeader - DeclarationNumber is empty", ZShort.Zero, otherDeclaration1EntryHeader.ConsolidatedEntryMemberID);
				AssertEquals("otherDeclaration2EntryHeader - DeclarationNumber is empty", ZShort.Zero, otherDeclaration2EntryHeader.ConsolidatedEntryMemberID);

				leadDeclaration.DeclarationNumber = "123";
				consolidatedDeclaration.SyncStatusAfterMessageProcessing(message);
				AssertEquals("leadDeclarationEntryHeader ConsolidatedEntryMemberID = 1", (ZShort)1, leadDeclarationEntryHeader.ConsolidatedEntryMemberID);
				AssertEquals("otherDeclaration1EntryHeader ConsolidatedEntryMemberID = 2", (ZShort)2, otherDeclaration1EntryHeader.ConsolidatedEntryMemberID);
				AssertEquals("otherDeclaration2EntryHeader ConsolidatedEntryMemberID = 3", (ZShort)3, otherDeclaration2EntryHeader.ConsolidatedEntryMemberID);
			}
		});

		public void TestDefaultAttachDeclarationFilter()
		{
			const string importerFilterName = "Importer/Supplier";
			const string transportModeFilterName = "Transport Mode";
			const string messageSubTypeFilterName = "Shipment Sub-Type";
			const string masterBillFilterName = "Master Bill";
			const string arrivalAtDischargePortFilterName = "Arrival at Discharge Port";

			consolidatedDeclaration.LeadDeclaration.JE_MessageSubType = "AAA";
			consolidatedDeclaration.LeadDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			consolidatedDeclaration.LeadDeclaration.JE_OH_Importer = new ZGuid("29B7C3B6-1578-498A-B107-1C546F0F5FB5");
			consolidatedDeclaration.LeadDeclaration.JE_MasterBill = "MB000";
			consolidatedDeclaration.LeadDeclaration.JE_DateAtFinalDestination = new ZDateTime(2024, 12, 18);

			var defaults = consolidatedDeclaration.DefaultAttachDeclarationFilter.ToList<FilterBusinessObjectDefault>();
			CombineAssertions(() =>
			{
				var importerDefault = defaults.Single(x => x.FilterName == importerFilterName);
				var transportModeDefault = defaults.Single(x => x.FilterName == transportModeFilterName);
				var messageSubTypeDefault = defaults.Single(x => x.FilterName == messageSubTypeFilterName);
				var masterBillDefault = defaults.Single(x => x.FilterName == masterBillFilterName);
				var arrivalAtDischargePortDefault = defaults.Count(x => x.FilterName == arrivalAtDischargePortFilterName);

				AssertEquals("Importer filter value", new ZGuid("29B7C3B6-1578-498A-B107-1C546F0F5FB5"), importerDefault.Value);
				AssertEquals("TransportMode filter value", Core.Constants.TransportModes.Sea, transportModeDefault.Value);
				AssertEquals("MessageSubType filter value", "AAA", messageSubTypeDefault.Value);
				AssertEquals("Master Bill filter value", "MB000", masterBillDefault.Value);
				AssertEquals("ETA filter value", 3, arrivalAtDischargePortDefault);
			});
		}

		public void TestSetDefaultValues()
		{
			AssertEquals("CRD_ApplicationCode", Customs.Business.ConsolidatedDeclaration.ApplicationCodes.CMR, consolidatedDeclaration.CRD_ApplicationCode);
		}

		public void TestLoadFromRef()
		{
			consolidatedDeclaration.CRD_JobReferenceNumber = "135";
			AssertNotNull(ConsolidatedDeclaration.LoadFromRef(Factory, "135"));
		}

		public void TestSyncJobDeclarationsStatus()
		{
			CombineAssertions(() =>
			{
				var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(new BusinessObjectFactory(), 2);
				var declaration1 = consolidatedDeclaration.JobDeclarations[0] as JobDeclaration;
				declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
				var declaration2 = consolidatedDeclaration.JobDeclarations[1] as JobDeclaration;
				declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
				consolidatedDeclaration.CRD_JobReferenceNumber = "CE00000002";
				var message = consolidatedDeclaration.Messages.AddNew(typeof(CMRIMDRMessage));
				consolidatedDeclaration.Messages.Add(message);
				AssertExceptionThrown<DeveloperNotificationException>("Syncing from a front-end user is prohibited", () => consolidatedDeclaration.SyncStatusAfterMessageProcessing(message));
				using (Env.SetTemporaryUserContext(new UserContext(User.ServiceUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					AssertExceptionThrown<DeveloperNotificationException>("Syncing without a message is prohibited", () => consolidatedDeclaration.SyncStatusAfterMessageProcessing(null));
					message.HasChanges = false;
					AssertExceptionThrown<DeveloperNotificationException>("Syncing without accompanying message changes is prohibited", () => consolidatedDeclaration.SyncStatusAfterMessageProcessing(message));
					AssertExceptionThrown<DeveloperNotificationException>("Syncing without linked message is prohibited", () => consolidatedDeclaration.SyncStatusAfterMessageProcessing(consolidatedDeclaration.Factory.New<EDIMessage>()));

					consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(new BusinessObjectFactory(), 2);
					consolidatedDeclaration.Factory.Save();
					message = consolidatedDeclaration.Factory.New<CMRIMDRMessage>();
					consolidatedDeclaration.Messages.Add(message);
					var leadDeclaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
					leadDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					var leadDeclarationEntryHeader = leadDeclaration.EntryHeader;
					var otherDeclaration = (JobDeclaration)consolidatedDeclaration.JobDeclarations[1];
					otherDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					var otherDeclarationEntryHeader = otherDeclaration.EntryHeader;
					leadDeclaration.JE_EntryStatus = "XXX";
					leadDeclaration.JE_MessageStatus = Common.AU.CustomsEntryStatus.ClearFormalLodge.Code;
					leadDeclaration.JE_EDITransmitDate = new ZDateTime(2024, 5, 1);
					leadDeclarationEntryHeader.EntryNumber = "Test123";
					leadDeclarationEntryHeader.CH_EntryStatus = "XXX";
					leadDeclarationEntryHeader.CH_Status = "SPY";
					leadDeclarationEntryHeader.CH_CustomsDeliveryInstructions = "Some instruction";
					leadDeclarationEntryHeader.AddInfo.ZA_PaymentStatus_Hidden = "PPE";
					leadDeclarationEntryHeader.AddInfo.ZA_ScheduledPaymentDate_Hidden = new ZDateTime(2024, 07, 30, 15, 45, 00);
					consolidatedDeclaration.SyncStatusAfterMessageProcessing(message);
					AssertEquals("Post message processing: Entry Status should sync from job declaration to consolidated declarations", leadDeclaration.JE_EntryStatus, consolidatedDeclaration.CRD_CustomsStatus);
					AssertEquals("Post message processing: Message Status should sync from job declaration to consolidated declarations", leadDeclaration.JE_MessageStatus, consolidatedDeclaration.CRD_MessageStatus);
					AssertEquals("Entry Status should sync from lead job declaration to other declarations", leadDeclaration.JE_EntryStatus, otherDeclaration.JE_EntryStatus);
					AssertEquals("Message Status should sync from lead job declaration to other declarations", leadDeclaration.JE_MessageStatus, otherDeclaration.JE_MessageStatus);
					AssertEquals("Transmit Date should sync from lead job declaration to other declarations", leadDeclaration.JE_EDITransmitDate, otherDeclaration.JE_EDITransmitDate);
					AssertEquals("Customs Delivery Instruction should sync from lead job declaration to other declarations", leadDeclarationEntryHeader.CH_CustomsDeliveryInstructions, otherDeclarationEntryHeader.CH_CustomsDeliveryInstructions);
					AssertEquals("Entry Status should sync from lead job declaration to other declarations", leadDeclarationEntryHeader.CH_EntryStatus, otherDeclarationEntryHeader.CH_EntryStatus);
					AssertEquals("Message Status should sync from lead job declaration to other declarations", leadDeclarationEntryHeader.CH_Status, otherDeclarationEntryHeader.CH_Status);
					AssertEquals("Customs Pay Status should sync from lead job declaration to other declarations", leadDeclarationEntryHeader.AddInfo.ZA_PaymentStatus_Hidden, otherDeclarationEntryHeader.AddInfo.ZA_PaymentStatus_Hidden);
					AssertEquals("Scheduled Payment Date should sync from lead job declaration to other declarations", leadDeclarationEntryHeader.AddInfo.ZA_ScheduledPaymentDate_Hidden, otherDeclarationEntryHeader.AddInfo.ZA_ScheduledPaymentDate_Hidden);
					AssertEquals("Entry Number should sync from lead job declaration to other declarations", leadDeclaration.DeclarationNumber, otherDeclaration.DeclarationNumber);

					AssertEquals("CustomsStatusDescription", "Unknown", consolidatedDeclaration.CustomsStatusDescription);
					AssertEquals("DeclarationNumber", "Test123", consolidatedDeclaration.DeclarationNumber);
					AssertEquals("MessageStatusDescription", "Cleared Formal Lodge", consolidatedDeclaration.MessageStatusDescription);
				}
			});
		}

		public void TestBuildAggregateDeclaration()
		{
			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 1);
			var declaration1 = (JobDeclaration)consolidatedDeclaration.JobDeclarations[0];
			var declaration2 = (JobDeclaration)consolidatedDeclaration.JobDeclarations[1];
			PrepareDeclarationForAggregation(declaration1, "HB111");
			PrepareDeclarationForAggregation(declaration2, "HB112");
			var wareHouseOrg = Factory.New<OrgHeader>();
			wareHouseOrg.OH_Code = "WAREHOUSE";
			consolidatedDeclaration.LeadDeclaration.DocAddresses.AddNew().MakePersistentEvenIfEmpty();
			var entryLine1 = declaration1.EntryHeader.AllEntryLines.Single();
			entryLine1.CL_LineNumber = 1;
			entryLine1.ZA_AggregateEntryLineNumber = 1;
			var entryLine2 = declaration1.EntryHeader.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			declaration1.EntryHeader.CH_HighestLineNumber = 1;
			var entryLine3 = declaration2.EntryHeader.AllEntryLines.Single();
			entryLine3.CL_LineNumber = 1;
			entryLine3.ZA_AggregateEntryLineNumber = 2;
			var entryLine4 = declaration2.EntryHeader.AllEntryLines.AddNew();
			entryLine4.CL_LineNumber = 2;
			declaration2.EntryHeader.CH_HighestLineNumber = 1;
			var leadDeclarationEntryHeader = consolidatedDeclaration.LeadDeclaration.ActiveEntryHeaders[0] as CusEntryHeader;
			leadDeclarationEntryHeader.EntryNumber = "12345";

			leadDeclarationEntryHeader.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.PayRejected;
			var iMDRMessage = Factory.New<CMRIMDRMessage>();
			iMDRMessage.EM_MessageText = TestMessages.IMDRMessageText;
			iMDRMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			consolidatedDeclaration.Messages.Add(iMDRMessage);

			var deletedPack1 = declaration1.EntryHeader.DeletedPackingGroups[0];
			deletedPack1.CY_Data = "1";
			var deletedPack2 = declaration2.EntryHeader.DeletedPackingGroups[0];
			deletedPack2.CY_Data = "2";
			declaration1.EntryHeader.Logs.AddNew(Events.StatusChange, CustomsEntryStatus.AwaitingFormalLodge.Code);
			declaration2.EntryHeader.Logs.AddNew(Events.StatusChange, CustomsEntryStatus.AwaitingFormalLodge.Code);
			Factory.Save();
			consolidatedDeclaration.CRD_JobReferenceNumber = "CRD";
			AssertEquals("Pre-condition: Entry line number for first declaration", (ZShort)1, declaration1.EntryHeader.MergedLines[0].CL_LineNumber);
			AssertEquals("Pre-condition: Entry line number for second declaration", (ZShort)1, declaration2.EntryHeader.MergedLines[0].CL_LineNumber);

			var baselineChangeNumber = Factory.LastChangeNumber;
			var aggregateDeclaration = (JobDeclaration)consolidatedDeclaration.BuildAggregateJobDeclaration();
			CombineAssertions(() =>
			{
				AssertType<ReadOnlyBusinessObjectFactory>(aggregateDeclaration.Factory);
				AssertEquals("Build does not make factory dirty", baselineChangeNumber, Factory.LastChangeNumber);
				AssertEquals("JE_DeclarationReference", "CRD", aggregateDeclaration.JE_DeclarationReference);
				AssertEquals("Weight", new ZArchitecture.ZWeight(200m, "KG"), aggregateDeclaration.GrossWeight);
				AssertEquals("Volume", new ZArchitecture.ZVolume(400m, ""), aggregateDeclaration.Volume);
				AssertEquals("TotalPayable", 70m, aggregateDeclaration.EntryHeader.CH_TotalPaid);
				AssertEquals("Invoices", 2, aggregateDeclaration.Invoices.Select(_ => _.PK).Distinct().Count());
				AssertEquals("Invoice Lines", 2, aggregateDeclaration.InvoiceLines.Select(_ => _.PK).Distinct().Count());
				AssertEquals("Invoice 1 Lines", 1, aggregateDeclaration.Invoices[0].InvoiceLines.Select(_ => _.PK).Distinct().Count());
				AssertEquals("Invoice 2 Lines", 1, aggregateDeclaration.Invoices[1].InvoiceLines.Select(_ => _.PK).Distinct().Count());
				AssertEquals("DocAddresses", 1, aggregateDeclaration.DocAddresses.Count);
				AssertEquals("MergedLines", 4, aggregateDeclaration.EntryHeader.MergedLines.Count);
				AssertEquals("AllEntryLines", 4, aggregateDeclaration.EntryHeader.AllEntryLines.Count);
				AssertContainsExactElementsInAnyOrder("Bills", new[] { "MB000", "HB111", "MB000", "HB112" }, aggregateDeclaration.Bills.Cast<Bill>().Select(x => x.CU_BillNum));
				AssertEquals("Packages", 2, aggregateDeclaration.Packages.Count);
				AssertEquals("Containers", 2, aggregateDeclaration.CusContainers.Count);
				AssertEquals("Entry line number is recalculated and in sync with addinfo", (ZShort)1, aggregateDeclaration.EntryHeader.MergedLines.FindByLineNumber(1).EntryLineAddInfo.ZA_AggregateEntryLineNumber_Hidden);
				AssertEquals("Entry line number is recalculated and in sync with addinfo", (ZShort)2, aggregateDeclaration.EntryHeader.MergedLines.FindByLineNumber(2).EntryLineAddInfo.ZA_AggregateEntryLineNumber_Hidden);
				AssertEquals("Entry line number is recalculated and in sync with addinfo", (ZShort)3, aggregateDeclaration.EntryHeader.MergedLines.FindByLineNumber(3).EntryLineAddInfo.ZA_AggregateEntryLineNumber_Hidden);
				AssertEquals("Entry line number is recalculated and in sync with addinfo", (ZShort)4, aggregateDeclaration.EntryHeader.MergedLines.FindByLineNumber(4).EntryLineAddInfo.ZA_AggregateEntryLineNumber_Hidden);
				AssertEquals("Aggregate CH_HighestLineNumber", (ZShort)2, aggregateDeclaration.EntryHeader.CH_HighestLineNumber);
				AssertEquals("Entry line number is not changed if it's smaller than aggregated HighestLineNumber", (ZShort)1, entryLine1.ZA_AggregateEntryLineNumber);
				AssertEquals("Entry line number is not changed if it's smaller than aggregated HighestLineNumber", (ZShort)2, entryLine3.ZA_AggregateEntryLineNumber);
				AssertNotNull("Lead entry header log is moved to aggregate entry header", aggregateDeclaration.EntryHeader.Logs.GetAllLogs().Cast<StmALog>().Single(l => l.SL_SE_NKEvent == Events.StatusChangeCode && l.SL_Reference == CustomsEntryStatus.AwaitingFormalLodge.Code));
				AssertEquals("Number of aggregated DeletedPackingGroups", 2, aggregateDeclaration.EntryHeader.DeletedPackingGroups.Count);
				AssertEquals("DeletedPackingGroups are moved to aggregate entry header", 1, aggregateDeclaration.EntryHeader.DeletedPackingGroups.Find(d => d.CY_Data == "1").Count());
				AssertEquals("DeletedPackingGroups are moved to aggregate entry header", 1, aggregateDeclaration.EntryHeader.DeletedPackingGroups.Find(d => d.CY_Data == "2").Count());
				AssertEquals("ZA_PaymentStatus_Hidden is copied", CMREntryPaymentStatusList.Codes.PayRejected, aggregateDeclaration.EntryHeader.AddInfo.ZA_PaymentStatus_Hidden);
				AssertNotNull("Messages from consolidated entry are copied to aggregateDeclaration entryHeader", aggregateDeclaration.EntryHeader.Messages.Cast<EDIMessage>().SingleOrDefault(x => x.EM_MessageType == CMRMessage.CMRMessageTypes.IMD));
				AssertEquals("TotalPayableAdvisedInLastClearanceMessage is correct", 191.90m, aggregateDeclaration.EntryHeader.TotalPayableAdvisedInLastClearanceMessage);
				AssertEquals("TotalPayableDueAdvisedInLastClearanceMessage is correct", 191.90m, aggregateDeclaration.EntryHeader.TotalPayableDueAdvisedInLastClearanceMessage);
			});
		}

		public void TestBuildAggregateDeclarationWithTILV()
		{
			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 1);
			var declaration1 = (JobDeclaration)consolidatedDeclaration.JobDeclarations[0];
			var declaration2 = (JobDeclaration)consolidatedDeclaration.JobDeclarations[1];
			PrepareDeclarationForAggregation(declaration1, "HB111");
			PrepareDeclarationForAggregation(declaration2, "HB112");

			var dec1Invoice = declaration1.Invoices[0];
			dec1Invoice.JZ_InvoiceNumber = "1";
			dec1Invoice.JZ_InvoiceAmount = 10000m;
			dec1Invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			dec1Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m);
			var line11 = dec1Invoice.JobComInvoiceLines[0];
			line11.Charges.RemoveAndDeleteAll();
			line11.JI_LinePrice = 6000m;
			var line12 = dec1Invoice.JobComInvoiceLines.AddNew();
			line12.JI_LinePrice = 4000m;

			var entryHeader1 = declaration1.EntryHeader;
			var entryLine11 = entryHeader1.AllEntryLines.Single();
			entryLine11.CL_LineNumber = 1;
			entryLine11.CL_Description = "EntryLine11";
			var entryLine12 = entryHeader1.MergedLines.AddNew();
			entryLine12.CL_LineNumber = 2;
			entryLine12.CL_Description = "EntryLine12";
			line12.JI_CL = entryLine12.PK;

			var dec2Invoice = declaration2.Invoices[0];
			dec2Invoice.JZ_InvoiceNumber = "1";
			dec2Invoice.JZ_InvoiceAmount = 10000m;
			dec2Invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			dec2Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m);
			var line21 = dec2Invoice.JobComInvoiceLines[0];
			line21.Charges.RemoveAndDeleteAll();
			line21.JI_LinePrice = 3000m;
			var line22 = dec2Invoice.JobComInvoiceLines.AddNew();
			line22.JI_LinePrice = 7000m;

			var entryHeader2 = declaration2.EntryHeader;
			var entryLine21 = entryHeader2.AllEntryLines.Single();
			entryLine21.CL_LineNumber = 1;
			entryLine21.CL_Description = "EntryLine21";
			var entryLine22 = entryHeader2.MergedLines.AddNew();
			entryLine22.CL_LineNumber = 2;
			entryLine22.CL_Description = "EntryLine22";
			line22.JI_CL = entryLine22.PK;

			Factory.Save();

			consolidatedDeclaration.CRD_JobReferenceNumber = "CRD";
			AssertEquals("Pre-condition: Entry line number for first declaration", (ZShort)1, declaration1.EntryHeader.MergedLines[0].CL_LineNumber);
			AssertEquals("Pre-condition: Entry line number for second declaration", (ZShort)1, declaration2.EntryHeader.MergedLines[0].CL_LineNumber);
			AssertEquals("Invoice 1 line 1 apportioned freight", 60m, line11.ApportionedCharges.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight, JobDeclaration.GetLocalCurrency()));
			AssertEquals("Invoice 1 line 2 apportioned freight", 40m, line12.ApportionedCharges.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight, JobDeclaration.GetLocalCurrency()));
			AssertEquals("Invoice 2 line 1 apportioned freight", 30m, line21.ApportionedCharges.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight, JobDeclaration.GetLocalCurrency()));
			AssertEquals("Invoice 2 line 2 apportioned freight", 70m, line22.ApportionedCharges.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight, JobDeclaration.GetLocalCurrency()));

			var baselineChangeNumber = Factory.LastChangeNumber;
			var aggregateDeclaration = (JobDeclaration)consolidatedDeclaration.BuildAggregateJobDeclaration();
			CombineAssertions(() =>
			{
				AssertEquals("Build does not make factory dirty", baselineChangeNumber, Factory.LastChangeNumber);

				var aggregateEntryHeader = aggregateDeclaration.EntryHeader;
				var entryLine1 = (CusEntryLine)aggregateEntryHeader.MergedLines.Find(x => x.CL_Description == "EntryLine11").First();
				var entryLine2 = (CusEntryLine)aggregateEntryHeader.MergedLines.Find(x => x.CL_Description == "EntryLine12").First();
				var entryLine3 = (CusEntryLine)aggregateEntryHeader.MergedLines.Find(x => x.CL_Description == "EntryLine21").First();
				var entryLine4 = (CusEntryLine)aggregateEntryHeader.MergedLines.Find(x => x.CL_Description == "EntryLine22").First();

				AssertEquals("TAndI for Header Currency", "AUD", aggregateEntryHeader.TransportAndInsuranceForMessage.Currency.Code);
				AssertEquals("TAndI for Header Amount", 200m, aggregateEntryHeader.TransportAndInsuranceForMessage.Amount);
				AssertEquals("TAndI for Entry Print", 200m, aggregateEntryHeader.TransportAndInsuranceInLocalCurrency.Amount);
				AssertEquals("TAndI for Line11 Currency", "AUD", entryLine1.TransportAndInsuranceForMessage.Currency.Code);
				AssertEquals("TAndI for Line11 Amount", 60m, entryLine1.TransportAndInsuranceForMessage.Amount);
				AssertEquals("TAndI for Line12 Currency", "AUD", entryLine2.TransportAndInsuranceForMessage.Currency.Code);
				AssertEquals("TAndI for Line12 Amount", 40m, entryLine2.TransportAndInsuranceForMessage.Amount);
				AssertEquals("TAndI for Line21 Currency", "AUD", entryLine3.TransportAndInsuranceForMessage.Currency.Code);
				AssertEquals("TAndI for Line21 Amount", 30m, entryLine3.TransportAndInsuranceForMessage.Amount);
				AssertEquals("TAndI for Line22 Currency", "AUD", entryLine4.TransportAndInsuranceForMessage.Currency.Code);
				AssertEquals("TAndI for Line22 Amount", 70m, entryLine4.TransportAndInsuranceForMessage.Amount);
			});
		}

		public void TestBuildAggregateDeclarationWithTILVAndChargesFromIMDR()
		{
			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 1);
			var declaration1 = (JobDeclaration)consolidatedDeclaration.JobDeclarations[0];
			var declaration2 = (JobDeclaration)consolidatedDeclaration.JobDeclarations[1];
			PrepareDeclarationForAggregation(declaration1, "HB111");
			PrepareDeclarationForAggregation(declaration2, "HB112");

			var dec1Invoice = declaration1.Invoices[0];
			dec1Invoice.JZ_InvoiceNumber = "1";
			dec1Invoice.JZ_InvoiceAmount = 10000m;
			dec1Invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			dec1Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m);
			var line11 = dec1Invoice.JobComInvoiceLines[0];
			line11.Charges.RemoveAndDeleteAll();
			line11.JI_LinePrice = 6000m;
			var line12 = dec1Invoice.JobComInvoiceLines.AddNew();
			line12.JI_LinePrice = 4000m;

			var entryHeader1 = declaration1.EntryHeader;
			var entryLine11 = entryHeader1.AllEntryLines.Single();
			entryLine11.CL_LineNumber = 1;
			entryLine11.CL_Description = "EntryLine11";
			var entryLine12 = entryHeader1.MergedLines.AddNew();
			entryLine12.CL_LineNumber = 2;
			entryLine12.CL_Description = "EntryLine12";
			line12.JI_CL = entryLine12.PK;
			entryHeader1.Charges.SetAmount(CusEntryChargeTypeList.Codes.AQISProcessingCharge, 20m);
			entryHeader1.Charges.SetAmount(CusEntryChargeTypeList.Codes.DeclarationProcessingCharge, 40m);
			entryHeader1.Charges.SetAmount(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 60m);

			var dec2Invoice = declaration2.Invoices[0];
			dec2Invoice.JZ_InvoiceNumber = "1";
			dec2Invoice.JZ_InvoiceAmount = 10000m;
			dec2Invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			dec2Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m);
			var line21 = dec2Invoice.JobComInvoiceLines[0];
			line21.Charges.RemoveAndDeleteAll();
			line21.JI_LinePrice = 3000m;
			var line22 = dec2Invoice.JobComInvoiceLines.AddNew();
			line22.JI_LinePrice = 7000m;

			var entryHeader2 = declaration2.EntryHeader;
			var entryLine21 = entryHeader2.AllEntryLines.Single();
			entryLine21.CL_LineNumber = 1;
			entryLine21.CL_Description = "EntryLine21";
			var entryLine22 = entryHeader2.MergedLines.AddNew();
			entryLine22.CL_LineNumber = 2;
			entryLine22.CL_Description = "EntryLine22";
			line22.JI_CL = entryLine22.PK;
			entryHeader2.Charges.SetAmount(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 80m);

			consolidatedDeclaration.UpdateTILV("256.00AUD");
			Factory.Save();

			consolidatedDeclaration.CRD_JobReferenceNumber = "CRD";
			AssertEquals("Pre-condition: Entry line number for first declaration", (ZShort)1, declaration1.EntryHeader.MergedLines[0].CL_LineNumber);
			AssertEquals("Pre-condition: Entry line number for second declaration", (ZShort)1, declaration2.EntryHeader.MergedLines[0].CL_LineNumber);
			AssertEquals("Invoice 1 line 1 apportioned freight", 60m, line11.ApportionedCharges.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight, JobDeclaration.GetLocalCurrency()));
			AssertEquals("Invoice 1 line 2 apportioned freight", 40m, line12.ApportionedCharges.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight, JobDeclaration.GetLocalCurrency()));
			AssertEquals("Invoice 2 line 1 apportioned freight", 30m, line21.ApportionedCharges.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight, JobDeclaration.GetLocalCurrency()));
			AssertEquals("Invoice 2 line 2 apportioned freight", 70m, line22.ApportionedCharges.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight, JobDeclaration.GetLocalCurrency()));

			var baselineChangeNumber = Factory.LastChangeNumber;
			var aggregateDeclaration = (JobDeclaration)consolidatedDeclaration.BuildAggregateJobDeclaration();
			CombineAssertions(() =>
			{
				AssertEquals("Build does not make factory dirty", baselineChangeNumber, Factory.LastChangeNumber);

				var aggregateEntryHeader = aggregateDeclaration.EntryHeader;
				var entryLine1 = (CusEntryLine)aggregateEntryHeader.MergedLines.Find(x => x.CL_Description == "EntryLine11").First();
				var entryLine2 = (CusEntryLine)aggregateEntryHeader.MergedLines.Find(x => x.CL_Description == "EntryLine12").First();
				var entryLine3 = (CusEntryLine)aggregateEntryHeader.MergedLines.Find(x => x.CL_Description == "EntryLine21").First();
				var entryLine4 = (CusEntryLine)aggregateEntryHeader.MergedLines.Find(x => x.CL_Description == "EntryLine22").First();

				AssertEquals("TAndI for Header Currency", "AUD", aggregateEntryHeader.TransportAndInsuranceForMessage.Currency.Code);
				AssertEquals("TAndI for Header Amount", 200m, aggregateEntryHeader.TransportAndInsuranceForMessage.Amount);
				AssertEquals("TAndI for Entry Print", 256m, aggregateEntryHeader.TransportAndInsuranceInLocalCurrency.Amount);
				AssertEquals("TAndI for Line11 Currency", "AUD", entryLine1.TransportAndInsuranceForMessage.Currency.Code);
				AssertEquals("TAndI for Line11 Amount", 60m, entryLine1.TransportAndInsuranceForMessage.Amount);
				AssertEquals("TAndI for Line12 Currency", "AUD", entryLine2.TransportAndInsuranceForMessage.Currency.Code);
				AssertEquals("TAndI for Line12 Amount", 40m, entryLine2.TransportAndInsuranceForMessage.Amount);
				AssertEquals("TAndI for Line21 Currency", "AUD", entryLine3.TransportAndInsuranceForMessage.Currency.Code);
				AssertEquals("TAndI for Line21 Amount", 30m, entryLine3.TransportAndInsuranceForMessage.Amount);
				AssertEquals("TAndI for Line22 Currency", "AUD", entryLine4.TransportAndInsuranceForMessage.Currency.Code);
				AssertEquals("TAndI for Line22 Amount", 70m, entryLine4.TransportAndInsuranceForMessage.Amount);

				AssertEquals("AQISProcessingCharge", 20m, aggregateEntryHeader.Charges.GetAmount(CusEntryChargeTypeList.Codes.AQISProcessingCharge));
				AssertEquals("DeclarationProcessingCharge", 40m, aggregateEntryHeader.Charges.GetAmount(CusEntryChargeTypeList.Codes.DeclarationProcessingCharge));
				AssertEquals("DutyDeferredAmount (60 + 80)", 140m, aggregateEntryHeader.Charges.GetAmount(CusEntryChargeTypeList.Codes.DutyDeferredAmount));
			});
		}

		public void TestImportAggregateDeclaration()
		{
			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 1);
			var declaration1 = (JobDeclaration)consolidatedDeclaration.JobDeclarations[0];
			var declaration2 = (JobDeclaration)consolidatedDeclaration.JobDeclarations[1];
			PrepareDeclarationForAggregation(declaration1, "HB111");
			PrepareDeclarationForAggregation(declaration2, "HB112");
			var entryLine1 = declaration1.EntryHeader.AllEntryLines.Single();
			var entryLine2 = declaration2.EntryHeader.AllEntryLines.Single();
			var bill1 = declaration1.Bills.AddNew();
			declaration1.Invoices[0].JZ_CU_RelatedHouseBill = bill1.PK;
			var packingGroup1 = declaration1.PackingGroups.AddNew();
			packingGroup1.CR_CU_HouseBill = bill1.PK;
			var bill2 = declaration2.Bills.AddNew();
			declaration2.Invoices[0].JZ_CU_RelatedHouseBill = bill2.PK;
			var packingGroup2 = declaration2.PackingGroups.AddNew();
			packingGroup2.CR_CU_HouseBill = bill2.PK;
			Factory.Save();

			var aggregateDeclaration = (JobDeclaration)consolidatedDeclaration.BuildAggregateJobDeclaration();
			var readonlyFactory = aggregateDeclaration.Factory;
			var entryLineNumber1 = readonlyFactory.Load<CusEntryLine>(entryLine1.PK).ZA_AggregateEntryLineNumber;
			var entryLineNumber2 = readonlyFactory.Load<CusEntryLine>(entryLine2.PK).ZA_AggregateEntryLineNumber;
			readonlyFactory.Load<PackingGroup>(packingGroup1.PK).CR_HouseContainerNumber = 1;
			readonlyFactory.Load<PackingGroup>(packingGroup2.PK).CR_HouseContainerNumber = 2;
			aggregateDeclaration.EntryHeader.AddInfo.ZA_HighHouseContPivotNo_Hidden = 2;
			consolidatedDeclaration.ImportAggregateDeclaration(aggregateDeclaration);

			var newFactory = new BusinessObjectFactory();
			CombineAssertions(() =>
			{
				AssertEquals("ZA_AggregateEntryLineNumber is saved in database", entryLineNumber1, newFactory.Load<CusEntryLine>(entryLine1.PK).ZA_AggregateEntryLineNumber);
				AssertEquals("ZA_AggregateEntryLineNumber is saved in database", entryLineNumber2, newFactory.Load<CusEntryLine>(entryLine2.PK).ZA_AggregateEntryLineNumber);
				AssertEquals("CR_HouseContainerNumber is saved in database", (ZShort)1, newFactory.Load<PackingGroup>(packingGroup1.PK).CR_HouseContainerNumber);
				AssertEquals("CR_HouseContainerNumber is saved in database", (ZShort)2, newFactory.Load<PackingGroup>(packingGroup2.PK).CR_HouseContainerNumber);
				AssertEquals("HighHouseContPivotNo is saved in database", (ZShort)2, newFactory.Load<CusEntryHeader>(declaration1.EntryHeader.PK).HighHouseContPivotNo);
				AssertEquals("HighHouseContPivotNo is saved in database", (ZShort)2, newFactory.Load<CusEntryHeader>(declaration2.EntryHeader.PK).HighHouseContPivotNo);
			});
		}

		public void TestLineNumberingDoesNotRevertThroughRefreshBus()
		{
			var declaration1 = (JobDeclaration)consolidatedDeclaration.JobDeclarations[0];
			var declaration2 = (JobDeclaration)consolidatedDeclaration.JobDeclarations[1];
			PrepareDeclarationForAggregation(declaration1, "HB1");
			PrepareDeclarationForAggregation(declaration2, "HB2");

			var entryLine1 = declaration1.EntryHeader.AllEntryLines.Single();
			entryLine1.CL_LineNumber = 1;
			entryLine1.CL_Description = "L1";
			entryLine1.ZA_AggregateEntryLineNumber = 1;
			declaration1.EntryHeader.CH_HighestLineNumber = 1;
			declaration1.EntryHeader.EntryNumber = "12345";
			var entryLine2 = declaration2.EntryHeader.AllEntryLines.Single();
			entryLine2.CL_LineNumber = 1;
			entryLine2.ZA_AggregateEntryLineNumber = 2;
			entryLine2.CL_Description = "L2";
			declaration2.EntryHeader.CH_HighestLineNumber = 1;

			Factory.Save();
			var aggregateDeclaration = (JobDeclaration)consolidatedDeclaration.BuildAggregateJobDeclaration();
			var entryLines = aggregateDeclaration.EntryHeader.AllEntryLines;
			AssertContainsExactElementsInAnyOrder("Line numbering is applied", new ZShort[] { 1, 2 }, entryLines.Select(x => x.CL_LineNumber));
			var entryLine2Copy = entryLines.First(x => x.CL_Description == "L2");
			AssertEquals("CL_LineNumber is correctly assigned from ZA_AggregateEntryLineNumber", (ZShort)2, entryLine2Copy.CL_LineNumber);

			entryLine2.CL_Description = "U";
			Factory.Save();
			AssertEquals("Uncommitted LineNumber is retained after Refresh Bus Update is posted", (ZShort)2, entryLine2Copy.CL_LineNumber);
			AssertEquals("Description is unchanged after Refresh Bus Update is posted", "L2", entryLine2Copy.CL_Description);
		}

		[TestDate(2024, 1, 1)]
		public void TestAggregateJobDeclarationQuestions()
		{
			new CMRReferenceFileUpdateLog(Factory).LogUpdateSuccess();

			var profile = CMRCommunityProtectionProfile.New(Factory);
			profile.CP_TariffClassificationNumberfield = "000000";
			profile.CP_CommunityProtectionRiskIdentifier = 500;
			var risk = Factory.New<CMRCommunityProtectionRisk>();
			risk.CK_Identifier = 500;
			risk.CK_LodgementQuestionIdentifier = 400;
			risk.CK_StartDate = new ZDateTime(2005, 1, 1);
			risk.CK_EndDate = ZDateTime.Empty;

			CreateQuestionIfNotExists(1);
			CreateQuestionIfNotExists(3);
			CreateQuestionIfNotExists(375);
			CreateQuestionIfNotExists(400);

			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 1);
			var declaration1 = consolidatedDeclaration.JobDeclarations[0] as JobDeclaration;
			PrepareDeclarationForAggregation(declaration1, "HB111");
			declaration1.InvoiceLines[0].JI_Tariff = "00000000";
			declaration1.CPQAManager.GenerateQuestionsForOriginalOrAmendment();
			var declaration2 = consolidatedDeclaration.JobDeclarations[1] as JobDeclaration;
			PrepareDeclarationForAggregation(declaration2, "HB112");
			declaration2.InvoiceLines[0].JI_Tariff = "00000000";
			declaration2.CPQAManager.GenerateQuestionsForOriginalOrAmendment();
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new ZInt[] { 400 }, declaration1.EntryHeader.AllEntryLines[0].Questions.Select(q => q.ON_CPDecNum));
			AssertContainsExactElementsInAnyOrder(new ZInt[] { 400 }, declaration2.EntryHeader.AllEntryLines[0].Questions.Select(q => q.ON_CPDecNum));

			consolidatedDeclaration.CRD_JobReferenceNumber = "CRD";
			var baselineChangeNumber = Factory.LastChangeNumber;
			var aggregateDeclaration = (JobDeclaration)consolidatedDeclaration.BuildAggregateJobDeclaration();
			var aggregateEntryHeader = aggregateDeclaration.EntryHeader;

			CombineAssertions("aggregateDeclaration built correctly", () =>
			{
				AssertType<ReadOnlyBusinessObjectFactory>(aggregateDeclaration.Factory);
				AssertEquals("Build does not make factory dirty", baselineChangeNumber, Factory.LastChangeNumber);
				AssertEquals("JE_DeclarationReference", "CRD", aggregateDeclaration.JE_DeclarationReference);
				AssertEquals("Weight", new ZArchitecture.ZWeight(200m, "KG"), aggregateDeclaration.GrossWeight);
				AssertEquals("Volume", new ZArchitecture.ZVolume(400m, ""), aggregateDeclaration.Volume);
				AssertEquals("TotalPayable", 70m, aggregateDeclaration.EntryHeader.CH_TotalPaid);
				AssertEquals("Invoices", 2, aggregateDeclaration.Invoices.Select(x => x.PK).Distinct().Count());
				AssertEquals("Invoice Lines", 2, aggregateDeclaration.InvoiceLines.Select(x => x.PK).Distinct().Count());

				AssertEquals("MergedLines", 2, aggregateEntryHeader.MergedLines.Count);
				AssertEquals("AllEntryLines", 2, aggregateEntryHeader.AllEntryLines.Count);
			});

			// first cycle - creating
			aggregateDeclaration.CPQAManager.GenerateQuestionsForConsolidatedEntryOriginalOrAmendment();

			CombineAssertions("Questions are generated correctly", () =>
			{
				AssertEquals("Questions are generated", 3, aggregateEntryHeader.Questions.Count);
				AssertContainsExactElementsInAnyOrder(new ZInt[] { 1, 3, 375 }, aggregateEntryHeader.Questions.Select(q => q.ON_CPDecNum));
				AssertEquals("AddInfo.ZA_CPQuestionGenDate_Hidden", new ZDateTime(2024, 01, 01), aggregateDeclaration.AddInfo.ZA_CPQuestionGenDate_Hidden);

				var entryLineQuestions = aggregateEntryHeader.AllEntryLines.SelectMany(x => x.Questions).Cast<CMRCusEntryCPDec>().ToArray();
				AssertContainsExactElementsInAnyOrder(new ZInt[] { 400, 400 }, entryLineQuestions.Select(q => q.ON_CPDecNum));

				var entryLineDescriptions = string.Join(", ", entryLineQuestions.Select(q => q.EntryLineDescription));
				AssertContains("Job number is included on entry line description", $"Entry Line No: {declaration1.JE_DeclarationReference}", entryLineDescriptions);
				AssertContains("Job number is included on entry line description", $"Entry Line No: {declaration2.JE_DeclarationReference}", entryLineDescriptions);
			});

			aggregateEntryHeader.Questions.GetQuestionWithID(1).ON_AnswerCode = "Y";
			aggregateEntryHeader.Questions.GetQuestionWithID(3).ON_AnswerCode = "Y";
			aggregateEntryHeader.Questions.GetQuestionWithID(375).ON_AnswerCode = "N";
			aggregateEntryHeader.AllEntryLines[0].Questions.GetQuestionWithID(400).ON_AnswerCode = "Y";
			aggregateEntryHeader.AllEntryLines[0].Questions.GetQuestionWithID(400).ON_Permit = "1234";

			consolidatedDeclaration.UpdateQuestionsFromAggregateDeclaration(aggregateDeclaration);
			Factory.Save();

			CombineAssertions("Answers are stored correctly", () =>
			{
				AssertEquals("Answer 1 is applied", "Y", consolidatedDeclaration.Questions.GetQuestionWithID(1).ON_AnswerCode);
				AssertEquals("Answer 3 is applied", "Y", consolidatedDeclaration.Questions.GetQuestionWithID(3).ON_AnswerCode);
				AssertEquals("Answer 375 is applied", "N", consolidatedDeclaration.Questions.GetQuestionWithID(375).ON_AnswerCode);
				AssertEquals("AddInfo.ZA_CPQuestionGenDate_Hidden", new ZDateTime(2024, 01, 01), consolidatedDeclaration.AddInfo.ZA_CPQuestionGenDate_Hidden);
				AssertEquals("consolidatedDeclaration ZA_CPQuestionGenDate_Hidden serialised", "CPQuestionGenDate_Hidden=01/01/2024", consolidatedDeclaration.CRD_AddInfo);

				AssertEquals("Answer is written back to source declaration1", "Y", declaration1.EntryHeader.AllEntryLines[0].Questions.GetQuestionWithID(400).ON_AnswerCode);
				AssertEquals("Answer is written back to source declaration1", "1234", declaration1.EntryHeader.AllEntryLines[0].Questions.GetQuestionWithID(400).ON_Permit);
				AssertEquals("Answer is written back to source declaration2", "", declaration2.EntryHeader.AllEntryLines[0].Questions.GetQuestionWithID(400).ON_AnswerCode);
			});

			// Second Cycle - updating
			var aggregateDeclaration2 = (JobDeclaration)consolidatedDeclaration.BuildAggregateJobDeclaration();
			var aggregateEntryHeader2 = aggregateDeclaration2.EntryHeader;

			CombineAssertions("Answers are retrieved correctly", () =>
			{
				AssertEquals("Answer 1 is applied", "Y", aggregateEntryHeader2.Questions.GetQuestionWithID(1).ON_AnswerCode);
				AssertEquals("Answer 3 is applied", "Y", aggregateEntryHeader2.Questions.GetQuestionWithID(3).ON_AnswerCode);
				AssertEquals("Answer 375 is applied", "N", aggregateEntryHeader2.Questions.GetQuestionWithID(375).ON_AnswerCode);
				AssertEquals("AddInfo.ZA_CPQuestionGenDate_Hidden", new ZDateTime(2024, 01, 01), aggregateDeclaration2.AddInfo.ZA_CPQuestionGenDate_Hidden);

				var entryLines = aggregateEntryHeader2.AllEntryLines;

				AssertEquals("Answer is retrieved from declaration1", "Y", aggregateEntryHeader2.AllEntryLines[0].Questions.GetQuestionWithID(400).ON_AnswerCode);
				AssertEquals("Answer is retrieved from declaration2", "", aggregateEntryHeader2.AllEntryLines[1].Questions.GetQuestionWithID(400).ON_AnswerCode);
			});

			aggregateEntryHeader2.Questions.GetQuestionWithID(3).ON_AnswerCode = "N";
			aggregateEntryHeader2.Questions.GetQuestionWithID(375).ON_AnswerCode = "";
			aggregateEntryHeader2.AllEntryLines[0].Questions.GetQuestionWithID(400).ON_AnswerCode = "N";

			consolidatedDeclaration.UpdateQuestionsFromAggregateDeclaration(aggregateDeclaration2);

			CombineAssertions("Answers are updated correctly", () =>
			{
				AssertEquals("Answer 1 is unchanged", "Y", consolidatedDeclaration.Questions.GetQuestionWithID(1).ON_AnswerCode);
				AssertEquals("Answer 3 is updated", "N", consolidatedDeclaration.Questions.GetQuestionWithID(3).ON_AnswerCode);
				AssertEquals("Answer 375 is cleared", "", consolidatedDeclaration.Questions.GetQuestionWithID(375).ON_AnswerCode);
				AssertEquals("AddInfo.ZA_CPQuestionGenDate_Hidden", new ZDateTime(2024, 01, 01), consolidatedDeclaration.AddInfo.ZA_CPQuestionGenDate_Hidden);

				AssertEquals("Answer is written back to source declaration1", "N", declaration1.EntryHeader.AllEntryLines[0].Questions.GetQuestionWithID(400).ON_AnswerCode);
				AssertEquals("Answer is written back to source declaration2", "", declaration2.EntryHeader.AllEntryLines[0].Questions.GetQuestionWithID(400).ON_AnswerCode);
			});
		}

		[TestDate(2024, 1, 1)]
		public void TestAggregateJobDeclarationWithdrawlQuestions()
		{
			CreateQuestionIfNotExists(12);
			CreateQuestionIfNotExists(13);
			CreateQuestionIfNotExists(14);
			CreateQuestionIfNotExists(15);

			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 1);
			var declaration1 = consolidatedDeclaration.JobDeclarations[0] as JobDeclaration;
			PrepareDeclarationForAggregation(declaration1, "HB111");
			declaration1.CPQAManager.GenerateQuestionsForWithdrawal();
			var declaration2 = consolidatedDeclaration.JobDeclarations[1] as JobDeclaration;
			PrepareDeclarationForAggregation(declaration2, "HB112");
			declaration2.CPQAManager.GenerateQuestionsForWithdrawal();
			Factory.Save();

			consolidatedDeclaration.CRD_JobReferenceNumber = "CRD";
			var baselineChangeNumber = Factory.LastChangeNumber;
			var aggregateDeclaration = consolidatedDeclaration.BuildAggregateJobDeclaration() as JobDeclaration;
			AssertEquals("Build does not make factory dirty", baselineChangeNumber, Factory.LastChangeNumber);
			aggregateDeclaration.CPQAManager.GenerateQuestionsForConsolidatedEntryWithdrawal();
			aggregateDeclaration.EntryHeader.Questions.GetQuestionWithID(12).ON_AnswerCode = "Y";
			aggregateDeclaration.EntryHeader.Questions.GetQuestionWithID(13).ON_AnswerCode = "N";
			aggregateDeclaration.EntryHeader.Questions.GetQuestionWithID(14).ON_AnswerCode = "Y";
			aggregateDeclaration.EntryHeader.Questions.GetQuestionWithID(15).ON_AnswerCode = "Y";
			consolidatedDeclaration.UpdateQuestionsFromAggregateDeclaration(aggregateDeclaration);
			var consolidatedEntryQuestions = consolidatedDeclaration.Questions.Select(q => q.ON_CPDecNum);
			AssertContainsExactElementsInAnyOrder(new ZInt[] { 12, 13, 14, 15 }, consolidatedEntryQuestions);

			CombineAssertions(() =>
			{
				AssertType<ReadOnlyBusinessObjectFactory>(aggregateDeclaration.Factory);
				AssertEquals("JE_DeclarationReference", "CRD", aggregateDeclaration.JE_DeclarationReference);
				AssertEquals("Weight", new ZArchitecture.ZWeight(200m, "KG"), aggregateDeclaration.GrossWeight);
				AssertEquals("Volume", new ZArchitecture.ZVolume(400m, ""), aggregateDeclaration.Volume);
				AssertEquals("TotalPayable", 70m, aggregateDeclaration.EntryHeader.CH_TotalPaid);
				AssertEquals("Invoices", 2, aggregateDeclaration.Invoices.Select(x => x.PK).Distinct().Count());
				AssertEquals("Invoice Lines", 2, aggregateDeclaration.InvoiceLines.Select(x => x.PK).Distinct().Count());

				var entryHeader = aggregateDeclaration.EntryHeader;
				AssertEquals("MergedLines", 2, entryHeader.MergedLines.Count);
				AssertEquals("AllEntryLines", 2, entryHeader.AllEntryLines.Count);
				AssertEquals("Questions are generated", 4, entryHeader.Questions.Count);
			});

			aggregateDeclaration.EntryHeader.Questions.GetQuestionWithID(13).ON_AnswerCode = "Y";
			aggregateDeclaration.EntryHeader.Questions.GetQuestionWithID(14).ON_AnswerCode = "N";
			consolidatedDeclaration.UpdateQuestionsFromAggregateDeclaration(aggregateDeclaration);

			CombineAssertions(() =>
			{
				AssertEquals("Answer is updated", "Y", consolidatedDeclaration.Questions.GetQuestionWithID(12).ON_AnswerCode);
				AssertEquals("Answer is updated", "Y", consolidatedDeclaration.Questions.GetQuestionWithID(13).ON_AnswerCode);
				AssertEquals("Answer is updated", "N", consolidatedDeclaration.Questions.GetQuestionWithID(14).ON_AnswerCode);
				AssertEquals("Answer is updated", "Y", consolidatedDeclaration.Questions.GetQuestionWithID(15).ON_AnswerCode);
			});
		}

		public void TestCPQuestionGenDateIsUpdatedFromDataRefresh()
		{
			var cpQuestionGenDate = ZDate.Today;
			var conDec = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory);
			AssertEquals(ZDateTime.Empty, conDec.AddInfo.ZA_CPQuestionGenDate_Hidden);
			Factory.Save();

			var factoryB = new BusinessObjectFactory();
			var conDecB = factoryB.Load<ConsolidatedDeclaration>(conDec.PK);
			conDecB.AddInfo.ZA_CPQuestionGenDate_Hidden = cpQuestionGenDate;
			factoryB.Save();

			AssertEquals("AddInfo property reflects published value", cpQuestionGenDate, conDec.AddInfo.ZA_CPQuestionGenDate_Hidden);
			AssertEquals("Update doesn't raise HasChanges", false, conDec.HasChanges);
		}

		public void TestPaymentStatus()
		{
			consolidatedDeclaration.LeadDeclaration.JE_MessageSubType = "AAA";
			consolidatedDeclaration.LeadDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var declaration1 = consolidatedDeclaration.JobDeclarations[0] as JobDeclaration;
			PrepareDeclarationForAggregation(declaration1, "HB111");
			declaration1.InvoiceLines[0].JI_Tariff = "00000000";
			var declaration2 = consolidatedDeclaration.JobDeclarations[1] as JobDeclaration;
			PrepareDeclarationForAggregation(declaration2, "HB112");
			Factory.Save();
			AssertEquals("PaymentStatus - pre-condition", "Not Paid", consolidatedDeclaration.PaymentStatus);

			var leadEntryHeader = declaration1.CustomsEntryHeaders[0];
			leadEntryHeader.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.PayPending;
			AssertEquals("PaymentStatus", "Pay Pending", consolidatedDeclaration.PaymentStatus);
		}

		public void TestCreateDSMEventForQueuedEntryLodgementMessage()
		{
			consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 1);
			Factory.Save();
			var aggregateDeclaration = (JobDeclaration)consolidatedDeclaration.BuildAggregateJobDeclaration();
			aggregateDeclaration.JE_MessageStatus = CustomsEntryStatus.ScheduledLodgeWithPayment.Code;
			aggregateDeclaration.JE_EDITransmitDate = new ZDateTime(2024, 4, 1);
			consolidatedDeclaration.ImportAggregateDeclaration(aggregateDeclaration);
			var dsmEventLog = consolidatedDeclaration.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(AutoEvents.DeferredScheduledMessage).Single();
			var eventTime = new ZDateTime(2024, 4, 1, 8, 0, 0);
			CombineAssertions(() =>
			{
				AssertEquals("DSM event SL_Reference:", nameof(CMRMessageTypes.LodgeWithPay), dsmEventLog.SL_Reference);
				AssertEquals("DSM event SL_EventTime:", eventTime, dsmEventLog.SL_EventTime);
			});

			consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 1);
			Factory.Save();
			aggregateDeclaration = (JobDeclaration)consolidatedDeclaration.BuildAggregateJobDeclaration();
			aggregateDeclaration.JE_MessageStatus = CustomsEntryStatus.ScheduledLodgeWithoutPayment.Code;
			aggregateDeclaration.JE_EDITransmitDate = new ZDateTime(2024, 5, 1);
			consolidatedDeclaration.ImportAggregateDeclaration(aggregateDeclaration);
			dsmEventLog = consolidatedDeclaration.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(AutoEvents.DeferredScheduledMessage).Single();
			eventTime = new ZDateTime(2024, 5, 1, 8, 0, 0);
			CombineAssertions(() =>
			{
				AssertEquals("DSM event SL_Reference:", nameof(CMRMessageTypes.LodgeWithoutPay), dsmEventLog.SL_Reference);
				AssertEquals("DSM event SL_EventTime:", eventTime, dsmEventLog.SL_EventTime);
			});

			consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 1);
			Factory.Save();
			aggregateDeclaration = (JobDeclaration)consolidatedDeclaration.BuildAggregateJobDeclaration();
			aggregateDeclaration.JE_MessageStatus = CustomsEntryStatus.AwaitingFormalLodge.Code;
			consolidatedDeclaration.ImportAggregateDeclaration(aggregateDeclaration);
			AssertEquals("No DSM event is created", 0, consolidatedDeclaration.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(AutoEvents.DeferredScheduledMessage).Count());
		}

		public void TestCreateDSMEventForQueuedPaymentMessage()
		{
			consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory);
			var entryHeader = (CusEntryHeader)consolidatedDeclaration.LeadDeclaration.ActiveEntryHeaders[0];
			entryHeader.EntryNumber = "1";
			entryHeader.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.NotPaid;
			entryHeader.ScheduledPaymentDate = new ZDateTime(2024, 07, 25, 15, 21, 00);
			Factory.Save();
			var aggregateDeclaration = (JobDeclaration)consolidatedDeclaration.BuildAggregateJobDeclaration();
			aggregateDeclaration.JE_MessageStatus = CustomsEntryStatus.ScheduledPayment.Code;
			aggregateDeclaration.JE_EDITransmitDate = new ZDateTime(2024, 4, 1);

			consolidatedDeclaration.ImportAggregateDeclaration(aggregateDeclaration);
			var dsmEventLog = consolidatedDeclaration.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(AutoEvents.DeferredScheduledMessage).Single();
			CombineAssertions(() =>
			{
				AssertEquals("DSM event SL_Reference:", nameof(CMRMessageTypes.Payment), dsmEventLog.SL_Reference);
				AssertEquals("DSM event SL_EventTime:", new ZDateTime(2024, 07, 25, 15, 21, 00), dsmEventLog.SL_EventTime);
			});
		}

		public void TestDequeueScheduledMessages()
		{
			consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 1);
			Factory.Save();
			var aggregateDeclaration = (JobDeclaration)consolidatedDeclaration.BuildAggregateJobDeclaration();
			aggregateDeclaration.JE_MessageStatus = CustomsEntryStatus.ScheduledLodgeWithPayment.Code;
			aggregateDeclaration.JE_EDITransmitDate = ZDateTime.Now.AddDays(1);
			consolidatedDeclaration.ImportAggregateDeclaration(aggregateDeclaration);

			CombineAssertions(() =>
			{
				consolidatedDeclaration.DequeueScheduledMessages();
				var dsmEventLog = consolidatedDeclaration.Logs.GetAllLogs().Find(l => l.SL_SE_NKEvent == AutoEvents.DeferredScheduledMessage.Code).Single();
				AssertEquals("DSM event Cancelled", true, dsmEventLog.IsCancelled);
				var leadDeclaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
				AssertEquals("JE_MessageStatus", CustomsEntryStatus.NotSent.Code, leadDeclaration.JE_MessageStatus);
				AssertEquals("CH_Status", CustomsEntryStatus.NotSent.Code, leadDeclaration.EntryHeader.CH_Status);
				var linkedDeclaration = (JobDeclaration)consolidatedDeclaration.JobDeclarations[1];
				AssertNotEquals("Not the leadDeclaration", leadDeclaration, linkedDeclaration);
				AssertEquals("JE_MessageStatus", CustomsEntryStatus.NotSent.Code, linkedDeclaration.JE_MessageStatus);
				AssertEquals("CH_Status", CustomsEntryStatus.NotSent.Code, linkedDeclaration.EntryHeader.CH_Status);
			});
		}

		[TestDate(2024, 1, 1)]
		public void TestCreateCECEventWhenMemberDeclarationRequiresAmendment()
		{
			var leadDec = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
			PrepareDeclarationForAggregation(leadDec, "DEC1");
			leadDec.EntryHeader.CH_Status = Common.AU.CustomsEntryStatus.ClearFormalLodge.Code;
			leadDec.EntryHeader.CH_EntryStatus = Common.AU.CustomsEntryStatus.ClearFormalLodge.Code;

			var otherDec = (JobDeclaration)consolidatedDeclaration.JobDeclarations[1];
			PrepareDeclarationForAggregation(otherDec, "DEC2");
			otherDec.EntryHeader.CH_Status = Common.AU.CustomsEntryStatus.ClearFormalLodge.Code;
			otherDec.EntryHeader.CH_EntryStatus = Common.AU.CustomsEntryStatus.ClearFormalLodge.Code;
			Factory.Save();

			leadDec.JE_ExportDate = new ZDateTime(2024, 1, 2);
			AssertEquals("needs merge", true, leadDec.MergeManager.RequiresMerge);
			AssertEquals("NOT HasConsolidatedEntryChanges", false, consolidatedDeclaration.HasConsolidatedEntryChanges);
			AssertEquals(ContinueWithDetection.No, (leadDec as IMessageManageableBizObj).ProcessBeforeDetectingAmendmentAndContinue());
			AssertEquals("has merged", false, leadDec.MergeManager.RequiresMerge);

			var cecEventLead = leadDec.Logs.GetAllLogs().Find(l => l.SL_SE_NKEvent == AutoEvents.ConsolidatedEntryChanged.Code).Single();
			AssertEquals("CEC Event created on Lead Dec", new ZDateTime(2024, 1, 1), cecEventLead.SL_EventTime);
			var cecEventsCon = consolidatedDeclaration.Logs.GetAllLogs().Find(l => l.SL_SE_NKEvent == AutoEvents.ConsolidatedEntryChanged.Code);
			AssertEquals("CEC Event created on Consolidated Entry", 1, cecEventsCon.Count());
			AssertEquals("HasConsolidatedEntryChanges", true, consolidatedDeclaration.HasConsolidatedEntryChanges);

			otherDec.JE_ExportDate = new ZDateTime(2024, 1, 2);
			AssertEquals("needs merge", true, otherDec.MergeManager.RequiresMerge);
			AssertEquals(ContinueWithDetection.No, (otherDec as IMessageManageableBizObj).ProcessBeforeDetectingAmendmentAndContinue());
			AssertEquals("has merged", false, otherDec.MergeManager.RequiresMerge);

			var cecEventOther = otherDec.Logs.GetAllLogs().Find(l => l.SL_SE_NKEvent == AutoEvents.ConsolidatedEntryChanged.Code).Single();
			AssertEquals("CEC Event created on Other Dec", new ZDateTime(2024, 1, 1), cecEventOther.SL_EventTime);
			cecEventsCon = consolidatedDeclaration.Logs.GetAllLogs().Find(l => l.SL_SE_NKEvent == AutoEvents.ConsolidatedEntryChanged.Code);
			AssertEquals("CEC Event created on Consolidated Entry", 1, cecEventsCon.Count());
		}

		public void TestAggregatedDeclarationHighestLineNumberNotWriteBackToLeadDeclaration()
		{
			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 1);
			var leadDeclaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
			var memberDeclaration = (JobDeclaration)consolidatedDeclaration.JobDeclarations[1];
			PrepareDeclarationForAggregation(leadDeclaration, "HB111");
			PrepareDeclarationForAggregation(memberDeclaration, "HB112");
			var leadDeclarationEntryHeader = leadDeclaration.EntryHeader;
			leadDeclarationEntryHeader.EntryNumber = "12345";
			var memberDeclarationEntryHeader = memberDeclaration.EntryHeader;
			memberDeclarationEntryHeader.AllEntryLines.AddNew();

			leadDeclaration.DoMerge();
			memberDeclaration.DoMerge();
			leadDeclarationEntryHeader.CH_Status = ZString.Empty;
			memberDeclarationEntryHeader.CH_Status = ZString.Empty;
			leadDeclarationEntryHeader.CH_Status = CustomsEntryStatus.ClearAmendment.Code;
			memberDeclarationEntryHeader.CH_Status = CustomsEntryStatus.ClearAmendment.Code;
			AssertEquals("Lead Declaration highest line number", (ZShort)1, leadDeclarationEntryHeader.CH_HighestLineNumber);
			AssertEquals("Member Declaration highest line number", (ZShort)2, memberDeclarationEntryHeader.CH_HighestLineNumber);
			Factory.Save();

			var aggregateDeclaration = (JobDeclaration)consolidatedDeclaration.BuildAggregateJobDeclaration();
			AssertEquals("Aggregate Declaration highest line number", (ZShort)3, aggregateDeclaration.EntryHeader.CH_HighestLineNumber);
			consolidatedDeclaration.ImportAggregateDeclaration(aggregateDeclaration);

			var newFactory1 = new BusinessObjectFactory();
			CombineAssertions(() =>
			{
				AssertEquals("Lead Declaration highest line number is unchanged", (ZShort)1, newFactory1.Load<CusEntryHeader>(leadDeclarationEntryHeader.PK).CH_HighestLineNumber);
				AssertEquals("Member Declaration highest line number is unchanged", (ZShort)2, newFactory1.Load<CusEntryHeader>(memberDeclarationEntryHeader.PK).CH_HighestLineNumber);
			});

			leadDeclarationEntryHeader.CH_Status = ZString.Empty;
			leadDeclarationEntryHeader.AllEntryLines.AddNew();
			leadDeclaration.DoMerge();
			leadDeclarationEntryHeader.CH_Status = CustomsEntryStatus.ClearAmendment.Code;
			AssertEquals("Lead Declaration highest line number", (ZShort)2, leadDeclarationEntryHeader.CH_HighestLineNumber);
			AssertEquals("Member Declaration highest line number", (ZShort)2, memberDeclarationEntryHeader.CH_HighestLineNumber);
			Factory.Save();

			aggregateDeclaration = (JobDeclaration)consolidatedDeclaration.BuildAggregateJobDeclaration();
			AssertEquals("Aggregate Declaration highest line number", (ZShort)4, aggregateDeclaration.EntryHeader.CH_HighestLineNumber);
			consolidatedDeclaration.ImportAggregateDeclaration(aggregateDeclaration);
			var newFactory2 = new BusinessObjectFactory();
			CombineAssertions(() =>
			{
				AssertEquals("Lead Declaration highest line number is unchanged", (ZShort)2, newFactory2.Load<CusEntryHeader>(leadDeclarationEntryHeader.PK).CH_HighestLineNumber);
				AssertEquals("Member Declaration highest line number is unchanged", (ZShort)2, newFactory2.Load<CusEntryHeader>(memberDeclarationEntryHeader.PK).CH_HighestLineNumber);
			});
		}

		void PrepareDeclarationForAggregation(JobDeclaration declaration, ZString houseBillNum)
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			declaration.JE_MasterBill = "MB000";
			declaration.JE_HouseBill = houseBillNum;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2024, 1, 2);
			declaration.GrossWeight = new ZArchitecture.ZWeight(100m, "KG");
			declaration.Volume = new ZArchitecture.ZVolume(200m, "");
			var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 385m, "AUD");
			var entryLine1 = declaration.EntryHeader.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entryLine1.InvoiceLines.AddRange(declaration.InvoiceLines);
			invoiceLine1.JI_CL = entryLine1.PK;
			declaration.EntryHeader.MergedLines.Add(entryLine1);
			declaration.EntryHeader.CH_TotalPaid = 35m;

			declaration.CusContainers.AddNew();
			declaration.Packages[0].CW_PackQty = 1;
			declaration.ActiveEntryHeaders[0].Packages.AddRange(declaration.Packages);
		}

		void CreateQuestionIfNotExists(ZInt questionIdentifier)
		{
			var question = Factory.LoadTop1<CMRLodgementQuestion>(new ZQuery(CMRLodgementQuestionSchema.CQ_LodgementQuestionIdentifier, questionIdentifier));
			if (question == null)
			{
				question = Factory.New<CMRLodgementQuestion>();
				question.CQ_LodgementQuestionIdentifier = questionIdentifier;
				question.CQ_LodgementQuestionStartDate = new ZDateTime(2005, 1, 1);
			}
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => consolidatedDeclaration;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(factory);
		}

		protected override BusinessObject GetNewBusinessObject() => consolidatedDeclaration;

		protected override void SetUp()
		{
			base.SetUp();
			consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory);
		}
		ConsolidatedDeclaration consolidatedDeclaration;
	}
}
