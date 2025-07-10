using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using CusEntryPayInfoStatusList = Enterprise.Customs.Business.CusEntryPayInfoStatusList;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class PAYRECMessageProcessorTest : BaseImportDeclarationMessageProcessorTest
	{
		public void TestPublishUniversalShipmentToBondedWarehouseInwardOnPaid_NoWarehouseTransactionStatus()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				var messageText = @"UNH+000002+CUSRES:D:99B:UN'BGM+961:::PAYREC+420C 5E9E FC66:1+11'DTM+138:20161020:102'NAD+MR+AAA374M::95'NAD+CM+33380054835::95'NAD+IM+12090995252::95'NAD+VT+AA33HF::95'NAD+COQ+242200::215'NAD+AO+323232::215++DEBORAH SPAGARINO TEST ACCOUNT'RFF+ABO:B00160215/1/CMT1::1'RFF+ABQ:DONG 1550'RFF+ADU:B00160215/1'RFF+ABT:AAAECLJLY'RFF+RA:AAAECLJNE'TAX+3'MOA+7:0000000000000.00'TAX+3'MOA+23:0000000000152.00'TAX+3'MOA+9:0000000000000.00'TAX+3'MOA+58:0000000000000.00'TAX+3'MOA+149:0000000000000.00'TAX+3'MOA+369:0000000000000.00'TAX+3'MOA+371:0000000000000.00'TAX+3'MOA+26:0000000000042.00'TAX+3'MOA+206:0000000000000.00'TAX+3'MOA+304:0000000000000.00'TAX+3'MOA+128:0000000000194.00'UNT+37+000002";

				var message = CreateDataForUniversalTesting(JobMessageTypeList.Codes.Import, messageText, "B00160215", "ENT0123456", 110m, out var invoiceLine);
				Factory.Save();

				var entryHeader = invoiceLine.CusEntryLine.Header;
				AssertEquals("Default to blank.", string.Empty, entryHeader.CH_WarehouseTransactionStatus);

				ErrorReporter.Clear();

				GetMessageProcessor().ProcessMessage(message);
				Factory.Save();

				CombineAssertions("It should set the WHS status to Inward Created instead of crashing with an error like it does now.", () =>
				{
					AssertEquals(Customs.Business.WarehouseTransactionStatusList.Codes.InwardCreated, entryHeader.CH_WarehouseTransactionStatus);
					AssertNotContains("Cannot Handle Bonded Warehouse Inward when warehouse status is not pending", ErrorReporter.LastKeyReported);
				});

				ErrorReporter.Clear();
			}
		}

		public void TestSetCusEntryPayInfoRecord()
		{
			entryHeader.EntryPayInfos.RemoveAndDeleteAll();
			AssertEquals("No payment information", 0, entryHeader.EntryPayInfos.Count);

			entryHeader.CH_BGMReference = "B00122382/1";
			entryHeader.Declaration.JE_PaymentMethod = "IMP";
			incomingMessage.EM_MessageText = CMRImportDeclarationTestData.PAYREC;
			incomingMessage.EM_MessageNum = "0001007";
			GetMessageProcessor().ProcessMessage(incomingMessage);
			AssertEquals("There should be one PayInfo", 1, entryHeader.EntryPayInfos.Count);

			var payInfo = entryHeader.EntryPayInfos[0];
			AssertEquals("Message Number", "0001007", payInfo.C9_IncomingPayResponseNo);
			AssertEquals(true, payInfo.C9_CusResReceived);
			AssertEquals(true, payInfo.C9_RemAdvReceived);
			AssertEquals(181.75m, payInfo.C9_PaymentAmount);
			AssertEquals(new ZDateTime(2005, 2, 8), payInfo.C9_PaymentDate);
			AssertEquals("IMP", payInfo.C9_PaymentParty);
			AssertEquals("AAAANTKMA", payInfo.C9_PaymentReference);
			AssertEquals(CusEntryPayInfoStatusList.Codes.Clear, payInfo.C9_PaymentStatus);
			AssertEquals(PaymentTransactionTypeList.Codes.CustomsAQISPayment, payInfo.C9_TransactionType);

			CMRPAYRECMessage incomingMessage2 = Factory.New<CMRPAYRECMessage>();
			incomingMessage2.EM_MessageNum = "0001008";
			incomingMessage2.EM_MessageText = CMRImportDeclarationTestData.PAYRECForAQIS;
			GetMessageProcessor().ProcessMessage(incomingMessage2);
			AssertEquals("There should be two PayInfos", 2, entryHeader.EntryPayInfos.Count);
			payInfo = entryHeader.EntryPayInfos[1];
			AssertEquals(55m, payInfo.C9_PaymentAmount);
			AssertEquals(PaymentTransactionTypeList.Codes.AQISPayment, payInfo.C9_TransactionType);
		}

		public void TestAQISServicePaymentAmountSetForEntryHeader()
		{
			entryHeader.AQISServicePaymentAmount = 100m;
			entryHeader.AddInfo.ZA_IsPAYRECAck_Hidden = false;
			entryHeader.CH_TotalPaid = 2500m;

			Assert(!entryHeader.NeedsAutoRateASPOnSaved);

			incomingMessage.EM_MessageNum = "0001007";
			incomingMessage.EM_MessageText = CMRImportDeclarationTestData.PAYREC;
			GetMessageProcessor().ProcessMessage(incomingMessage);
			AssertEquals("AQIS Service payment amount is added", 155m, entryHeader.AQISServicePaymentAmount);
			AssertEquals("IsCustomsChargePaid", true, entryHeader.AddInfo.ZA_IsPAYRECAck_Hidden);
			AssertEquals("Total paid", 2555m, entryHeader.CH_TotalPaid);
			Assert(entryHeader.NeedsAutoRateASPOnSaved);

			entryHeader.ClearNeedsAutoRateASP();
			var incomingMessage2 = Factory.New<CMRPAYRECMessage>();
			incomingMessage2.EM_MessageNum = "0001008";
			incomingMessage2.EM_MessageText = CMRImportDeclarationTestData.PAYREC.Replace("0000000000055", "0000000000000");
			GetMessageProcessor().ProcessMessage(incomingMessage2);
			Assert(!entryHeader.NeedsAutoRateASPOnSaved);
		}

		public void TestConsolidatedEntry() => CombineAssertions(() =>
		{
			GlbStaff.CurrentUser.GS_Code = User.ServiceUserCode;
			var consolidatedDeclaration = Customs.Business.Testing.ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 1);
			var leadDeclaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
			leadDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			leadDeclaration.JE_MessageStatus = CustomsEntryStatus.AwaitingPayment.Code;
			leadDeclaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			leadDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			var leadDeclarationEntryHeader = leadDeclaration.EntryHeader;
			leadDeclarationEntryHeader.AllEntryLines.AddNew().InvoiceLines.AddRange(leadDeclaration.InvoiceLines);
			leadDeclarationEntryHeader.MergedLines.Add(leadDeclarationEntryHeader.AllEntryLines[0]);
			leadDeclarationEntryHeader.AQISServicePaymentAmount = 100m;
			leadDeclarationEntryHeader.CH_TotalPaid = 2500m;
			leadDeclarationEntryHeader.EntryNumber = "1";
			leadDeclarationEntryHeader.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.PayAckPending;
			var otherDeclaration = (JobDeclaration)consolidatedDeclaration.JobDeclarations[1];
			otherDeclaration.JE_ApplicationCode = AUCustoms.ImportMessagingMode.ForceCMRMessages;
			otherDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			otherDeclaration.JE_MessageStatus = CustomsEntryStatus.AwaitingPayment.Code;
			var otherDeclarationEntryHeader = otherDeclaration.EntryHeader;
			otherDeclarationEntryHeader.EntryNumber = "1";
			otherDeclarationEntryHeader.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.PayAckPending;
			Factory.Save();

			var outgoingMessage = Factory.New<CMRPAYSTDMessage>();
			outgoingMessage.EM_MessageText = TestMessages.PAYSTDMessage.Replace("BGM+481:::PAYSTD+B00001019/8/SYD1", $"BGM+481:::PAYSTD+{consolidatedDeclaration.CRD_JobReferenceNumber}/SYD1");
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_SystemCreateTimeUtc = new ZDateTime(2024, 07, 01);
			consolidatedDeclaration.Messages.Add(outgoingMessage);

			incomingMessage.EM_SystemCreateTimeUtc = new ZDateTime(2024, 07, 05);
			incomingMessage.EM_MessageNum = "0001007";
			incomingMessage.EM_MessageText = CMRImportDeclarationTestData.PAYREC;
			incomingMessage.EM_MessageText = incomingMessage.EM_MessageText.Replace("RFF+ABO:B00122382/1/SYD3", $"RFF+ABO:{consolidatedDeclaration.CRD_JobReferenceNumber}/SYD1");

			GetMessageProcessor().ProcessMessage(incomingMessage);
			AssertEquals("Message is linked to consolidated entry", consolidatedDeclaration, incomingMessage.EM_LinkedObject);
			AssertEquals("Total paid", 2555m, leadDeclarationEntryHeader.CH_TotalPaid);
			AssertEquals("AQIS Service payment amount is added", 155m, leadDeclarationEntryHeader.AQISServicePaymentAmount);
			AssertEquals("IsCustomsChargePaid", true, leadDeclarationEntryHeader.AddInfo.ZA_IsPAYRECAck_Hidden);
			AssertEquals("EntryNumber", "AAAA6RHTE", leadDeclarationEntryHeader.EntryNumber);
			AssertEquals("NeedsAutoRateASPOnSaved", true, leadDeclarationEntryHeader.NeedsAutoRateASPOnSaved);
			AssertEquals("Lead Declaration Message Status", "Payment Response Received (Check the Payment Status for Details)", leadDeclaration.JE_MessageStatusDescription);
			AssertEquals("Lead Declaration EntryHeader Message Status", "Payment Response Received (Check the Payment Status for Details)", leadDeclarationEntryHeader.MessageStatusDescription);
			AssertEquals("Lead Declaration EntryHeader Customs Pay", "Paid", leadDeclarationEntryHeader.PaymentStatus);
			AssertEquals("Other Declaration Message Status", "Payment Response Received (Check the Payment Status for Details)", otherDeclaration.JE_MessageStatusDescription);
			AssertEquals("Other Declaration EntryHeader Message Status", "Payment Response Received (Check the Payment Status for Details)", otherDeclarationEntryHeader.MessageStatusDescription);
			AssertEquals("Other Declaration EntryHeader Customs Pay", "Paid", otherDeclarationEntryHeader.PaymentStatus);
		});

		public void TestEntryNumberGetsSet()
		{
			incomingMessage.EM_MessageText = CMRImportDeclarationTestData.PAYREC;
			var processor = GetMessageProcessor();
			processor.ProcessMessage(incomingMessage);
			AssertEquals("EntryNumber", "AAAA6RHTE", entryHeader.EntryNumber);
			AssertEquals("ErrorEmailCount", 0, processor.ErrorEmailSendCount);
			AssertEquals("AcknowledgementEmailCount", 1, processor.AcknowledgementEmailSendCount);
		}

		public void TestPublishUniversalShipmentToBondedWarehouseInwardOnPaid()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddYears(-1).ToDateTime());
				string pAYRECData = "UNH+000002+CUSRES:D:99B:UN'BGM+961:::PAYREC+420C 5E9E FC66:1+11'DTM+138:20161020:102'NAD+MR+AAA374M::95'NAD+CM+33380054835::95'NAD+IM+12090995252::95'NAD+VT+AA33HF::95'NAD+COQ+242200::215'NAD+AO+323232::215++DEBORAH SPAGARINO TEST ACCOUNT'RFF+ABO:B00160215/1/CMT1::1'RFF+ABQ:DONG 1550'RFF+ADU:B00160215/1'RFF+ABT:AAAECLJLY'RFF+RA:AAAECLJNE'TAX+3'MOA+7:0000000000000.00'TAX+3'MOA+23:0000000000152.00'TAX+3'MOA+9:0000000000000.00'TAX+3'MOA+58:0000000000000.00'TAX+3'MOA+149:0000000000000.00'TAX+3'MOA+369:0000000000000.00'TAX+3'MOA+371:0000000000000.00'TAX+3'MOA+26:0000000000042.00'TAX+3'MOA+206:0000000000000.00'TAX+3'MOA+304:0000000000000.00'TAX+3'MOA+128:0000000000194.00'UNT+37+000002";
				JobComInvoiceLine invoiceLine;
				var payrecMessage = CreateDataForUniversalTesting(JobMessageTypeList.Codes.Import, pAYRECData, "B00160215", "ENT0123456", 110m, out invoiceLine);
				Factory.Save();
				GetMessageProcessor().ProcessMessage(payrecMessage);
				invoiceLine.Declaration.WarehouseTransactionStatus = Customs.Business.WarehouseTransactionStatusList.Codes.InwardCreatedPending;
				Factory.Save();
				AssertEquals(true, Env.OutgoingCustomsMailManager.EmailsCreated.Exists(x => x.Body.Contains("<p>Stock Levels have been updated. (WHS Receipt: <a href=")));
				var declaration = invoiceLine.Declaration;
				AssertNotNull(declaration.Logs.MostRecentLogByEventTime(Events.DataExport));
			}
		}

		public void TestNoPublishUniversalShipmentToBondedWarehouseInwardOnPayingQuarantineOnly()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddYears(-1).ToDateTime());
			string pAYRECData = "UNH+000002+CUSRES:D:99B:UN'BGM+961:::PAYREC+420C 5E9E FC66:1+11'DTM+138:20161020:102'NAD+MR+AAA374M::95'NAD+CM+33380054835::95'NAD+IM+12090995252::95'NAD+VT+AA33HF::95'NAD+COQ+242200::215'NAD+AO+323232::215++DEBORAH SPAGARINO TEST ACCOUNT'RFF+ABO:B00160215/1/CMT1::1'RFF+ABQ:DONG 1550'RFF+ADU:B00160215/1'RFF+ABT:AAAECLJLY'RFF+RA:AAAECLJNE'TAX+3'MOA+7:0000000000000.00'TAX+3'MOA+23:0000000000000.00'TAX+3'MOA+9:0000000000000.00'TAX+3'MOA+58:0000000000000.00'TAX+3'MOA+149:0000000000000.00'TAX+3'MOA+369:0000000000000.00'TAX+3'MOA+371:0000000000000.00'TAX+3'MOA+26:0000000000000.00'TAX+3'MOA+206:0000000000152.00'TAX+3'MOA+304:0000000000000.00'TAX+3'MOA+128:0000000000152.00'UNT+37+000002'";
			JobComInvoiceLine invoiceLine;
			var payrecMessage = CreateDataForUniversalTesting(JobMessageTypeList.Codes.Import, pAYRECData, "B00160215", "ENT0123456", 110m, out invoiceLine);
			Factory.Save();
			GetMessageProcessor().ProcessMessage(payrecMessage);
			Factory.Save();
			AssertEquals(false, Env.OutgoingCustomsMailManager.EmailsCreated.Exists(x => x.Body.Contains("<p>Stock Levels have been updated. (WHS Receipt: <a href=")));
			var declaration = invoiceLine.Declaration;
			AssertNull(declaration.Logs.MostRecentLogByEventTime(Events.DataExport));
		}

		protected override ZString GetExpectedMessageCode() => CMRMessage.CMRMessageTypes.PAYREC;

		protected override ZString GetExpectedMessageName() => "Payment Receipt Response (PAYREC)";

		protected override CMRMessageResponseProcessor GetMessageProcessor() => new PAYRECMessageProcessor(logger);

		protected override Type IncomingMessageType => typeof(CMRPAYRECMessage);

		CMRPAYRECMessage CreateDataForUniversalTesting(ZString messageType, ZString messageText, ZString declarationReference, ZString entryNumber, ZDecimal quantity, out JobComInvoiceLine invoiceLine)
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddYears(-1).ToDateTime());
			var entryHeader = GetNewEntryHeader(messageType, declarationReference, entryNumber, quantity);
			var pAYRECMessage = (CMRPAYRECMessage)entryHeader.Messages.AddNew(typeof(CMRPAYRECMessage));
			pAYRECMessage.EM_MessageText = messageText;
			pAYRECMessage.EM_LinkedObject = entryHeader;
			var entryLine = entryHeader.MergedLines[0];
			invoiceLine = entryLine.RandomLine;
			return pAYRECMessage;
		}

		CusEntryHeader GetNewEntryHeader(ZString messageType, ZString declarationReference, ZString entryNumber, ZDecimal quantity)
		{
			AssertNotNull(PostMasterGroup);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_DeclarationReference = declarationReference;
			declaration.WarehouseDocAddress.E2_OA_Address = Warehouse.MainAddress.PK;

			var warehouse = Factory.LoadTop1<IWhsWarehouse>(new ZQuery(WhsWarehouseSchema.WW_WarehouseCode, "WHS"));
			if (warehouse == null)
			{
				var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
				warehouse = (IWhsWarehouse)helper.CreateWarehouse("WHS", "BOND");
				warehouse.WW_OA_WarehouseAddress = Warehouse.MainAddress.PK;
				warehouse.WW_IsBondedWarehouse = true;
				warehouse.WW_IsVirtualWarehouse = true;
				((IWhsArea)warehouse.Areas[0]).WA_AreaType = "BON";
				warehouse.WW_GB_RelatedCompanyBranch = base.entryHeader.RegistryBranchPK;
				warehouse.WW_AutoPrintPackingSlip = false;
			}

			declaration.Invoices.DeleteAll();
			var invoice = declaration.Invoices.AddNew();
			invoice.AddInfo.ZA_ORG = Core.Constants.CountryCodes.France;
			invoice.JZ_InvoiceAmount = quantity * 100m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			invoice.Charges.AddNew("OFT", quantity * 10m, Core.Constants.CurrencyCodes.Australia);

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = Part.OP_PartNum;
			invoiceLine.JI_IsPackToBondForLine = true;
			invoiceLine.JI_InvoiceQuantity = quantity;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = quantity * 10m;
			invoiceLine.JI_LinePrice = quantity * 100m;
			invoiceLine.AddInfo.ZA_WRQ = quantity;
			invoiceLine.AddInfo.ZA_WRU = "NO";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.EntryNumber = entryNumber;
			return entryHeader;
		}

		GlbGroup PostMasterGroup
		{
			get
			{
				if (postMasterGroup == null)
				{
					postMasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
					postMasterGroup.Staff[0].GS_EmailAddress = "test@cargowise.com";
					var groupNotification = new AutoBillingGroupNotification();
					groupNotification.SendGroupPK = postMasterGroup.PK;
					CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);
				}
				return postMasterGroup;
			}
		}
		GlbGroup postMasterGroup;

		OrgHeader Importer
		{
			get
			{
				if (importer == null)
				{
					importer = Factory.New<OrgHeader>();
					importer.OH_Code = "IMP";
					importer.MiscServ.OM_IMPartAttrib1Name = "VIN1";
					importer.MiscServ.OM_IMPartAttrib1Type = "NON";
					importer.CompanyData.OB_IMUsedBondedWhs = true;
					importer.OH_IsWarehouseClient = true;
				}
				return importer;
			}
		}
		OrgHeader importer;

		OrgHeader Warehouse
		{
			get
			{
				if (warehouse == null)
				{
					warehouse = Factory.New<OrgHeader>();
					warehouse.OH_Code = "W1";
					warehouse.MainAddress.LocalControlledPremisesID = "23423";
				}
				return warehouse;
			}
		}
		OrgHeader warehouse;

		AUOrgSupplierPart Part
		{
			get
			{
				if (part == null)
				{
					part = Factory.New<AUOrgSupplierPart>();
					part.OP_PartNum = "~~1";
					part.OP_StockKeepingUnit = "NO";
					part.RelatedOrganisations.AddOrganisationIfNotExist(Importer.PK, OrgPartRelation.RelationshipTypes.Owner);
					part.AddNewImportPivotWithClassification(Classification.PK);
				}
				return part;
			}
		}
		AUOrgSupplierPart part;

		Classification Classification
		{
			get
			{
				if (classification == null)
				{
					classification = Factory.New<Classification>();
					classification.CC_ClassificationType = Classification.ClassificationType.IMP;
					classification.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					classification.CC_LookupCode = "~~1L";
					classification.CC_TariffNum = StatTariff.SC_TariffClassificationNumber + StatTariff.SC_StatisticalClassificationCode;
				}
				return classification;
			}
		}
		Classification classification;

		CMRStatisticalClassificationPeriodSnapshot StatTariff
		{
			get
			{
				if (statTariff == null)
				{
					statTariff = Factory.New<CMRStatisticalClassificationPeriodSnapshot>();
					statTariff.SC_TariffClassificationNumber = "00000000";
					statTariff.SC_StatisticalClassificationCode = "00";
					statTariff.SC_QuantityUnit = "KG";
					statTariff.SC_StartDate = new ZDateTime(2005, 1, 1);
				}
				return statTariff;
			}
		}
		CMRStatisticalClassificationPeriodSnapshot statTariff;
	}
}
