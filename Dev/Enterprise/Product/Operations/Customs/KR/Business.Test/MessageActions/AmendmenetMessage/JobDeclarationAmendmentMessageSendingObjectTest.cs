using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(JobDeclarationAmendmentMessageSendingObject))]
	sealed class JobDeclarationAmendmentMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{ 
		protected override BusinessObject GetNewBusinessObject()
		{
			SetupImportDecHavingStatement(out var declaration, out var entry, out var invoice, out var entryLine, out var statement929);
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			Create929Snapshot(entry);
			var specialConsumptionTax = entry.Charges.AddNew();
			specialConsumptionTax.C1_ChargeType = ChargeTypeList.Codes.SpecialConsumptionTax;
			specialConsumptionTax.C1_ChargeAmount = 30;
			entry.Messages.AddNew().EM_MessageType = ElectronicDocumentTypeList.Codes._5BA;

			var result = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			return result;
		}

		public void TestAmendmentReason()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas5BASnapshot();
			var sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5BB);
			sendingObj.AmendmentReason = "기재오류";
			AssertEquals("기재오류", sendingObj.AmendmentReason);
		}

		public void TestAmendmentTypeDescription()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas5BASnapshot();
			entry.EntryInstruction.CEI_AgreedDutyRate = 3.21m;
			var sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5BB);
			AssertEquals(_5BBAmendmentType.Descriptions.Update, sendingObj.AmendmentTypeDescription);

			sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5AS);
			AssertEquals(_5ASAmendmentType.Descriptions.Amendment, sendingObj.AmendmentTypeDescription);

			sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			AssertEquals("해당없음(X), 해당없음(X)", sendingObj.AmendmentTypeDescription);
		}

		public void TestAmendedItems()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas5BASnapshot();
			entry.EntryInstruction.CEI_AgreedDutyRate = 3.21m;
			entry.EntryInstruction.CEI_AgreedDutyRatePreferenceCode = "FAU1";
			var entryLine = entry.MergedLines[0];
			entryLine.CL_AdValoremTariff = "8523292991";
			entryLine.RandomLine.JI_Tariff = "8523292991";
			entryLine.RandomLine.JI_Description = "Those recorded video";
			var sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5BB);
			AssertEquals("Five amended item exists", 5, sendingObj.AmendedItems.Count);
			AssertEquals(GOVCBR5BADataItemIDList.Codes._04, sendingObj.AmendedItems[0].DataItemID);
			AssertEquals(GOVCBR5BADataItemIDList.Descriptions._04, sendingObj.AmendedItems[0].DataItemDescription);
			AssertEquals(GOVCBR5BADataItemIDList.Codes._05, sendingObj.AmendedItems[1].DataItemID);
			AssertEquals(GOVCBR5BADataItemIDList.Descriptions._05, sendingObj.AmendedItems[1].DataItemDescription);
			AssertEquals(GOVCBR5BADataItemIDList.Codes._01, sendingObj.AmendedItems[2].DataItemID);
			AssertEquals(GOVCBR5BADataItemIDList.Descriptions._01, sendingObj.AmendedItems[2].DataItemDescription);
			AssertEquals(GOVCBR5BADataItemIDList.Codes._02, sendingObj.AmendedItems[3].DataItemID);
			AssertEquals(GOVCBR5BADataItemIDList.Descriptions._02, sendingObj.AmendedItems[3].DataItemDescription);
			AssertEquals(GOVCBR5BADataItemIDList.Codes._03, sendingObj.AmendedItems[4].DataItemID);
			AssertEquals(GOVCBR5BADataItemIDList.Descriptions._03, sendingObj.AmendedItems[4].DataItemDescription);
		}

		public void Test5ASAmendedItems()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();
			entry.Declaration.JE_ExporterType = "A";
			entry.Declaration.JE_ExportGoodsType = "10";
			entry.Declaration.JE_ReturnReason = "AB";
			var entryLine1 = entry.MergedLines[0];
			entryLine1.CL_CustomsValue = 100m;
			var invoiceLine1 = entryLine1.RandomLine;
			invoiceLine1.JI_Model = "HYUNDAI ROBEX3000LC-7A-CHANGED";

			var sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5AS);
			AssertEquals("Five amended item exists", 5, sendingObj.AmendedItems.Count);
			var amendedItems = sendingObj.AmendedItems.Cast<AmendedItem>();
			AssertNotNull(amendedItems.Any(x => x.DataItemID == ExportAmendmentDataItemIDList.Codes.A105));
			AssertNotNull(amendedItems.Any(x => x.DataItemID == ExportAmendmentDataItemIDList.Codes.A203));
			AssertNotNull(amendedItems.Any(x => x.DataItemID == ExportAmendmentDataItemIDList.Codes.AB03));
			AssertNotNull(amendedItems.Any(x => x.DataItemID == ExportAmendmentDataItemIDList.Codes.B301));
			AssertNotNull(amendedItems.Any(x => x.DataItemID == ExportAmendmentDataItemIDList.Codes.B102));
		}

		public void Test5DSAmendedItems()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas5DQSnapshot();
			var declaration = entry.Declaration;
			declaration.JE_SubLocationOfGoods = "새 반입장소";
			declaration.JE_ExportGoodsType = "2";
			declaration.Invoices[0].JZ_Weight = 90;
			declaration.Invoices[0].JZ_NoOfPacks = 10;
			declaration.Invoices[0].JZ_DRWApplicantType = "2";
			declaration.DeclarationRefs[0].J3_ReferenceNumber = "NEW 20GLKO0080I";
			declaration.JE_NoOfCrew = 30;
			declaration.JE_VoyageDuration = 17;

			var sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5DS);
			AssertEquals(8, sendingObj.AmendedItems.Count);
			var sortAmendItemList = sendingObj.AmendedItems.Cast<AmendedItem>().OrderBy(x => x.DataItemID).ToList();
			AssertEquals(LocalExportAmendmentDataItemIDList.Codes._11A, sortAmendItemList[0].DataItemID);
			AssertEquals(LocalExportAmendmentDataItemIDList.Codes._13, sortAmendItemList[1].DataItemID);
			AssertEquals(LocalExportAmendmentDataItemIDList.Codes._14, sortAmendItemList[2].DataItemID);
			AssertEquals(LocalExportAmendmentDataItemIDList.Codes._15, sortAmendItemList[3].DataItemID);
			AssertEquals(LocalExportAmendmentDataItemIDList.Codes._16, sortAmendItemList[4].DataItemID);
			AssertEquals(LocalExportAmendmentDataItemIDList.Codes._18, sortAmendItemList[5].DataItemID);
			AssertEquals(LocalExportAmendmentDataItemIDList.Codes._38, sortAmendItemList[6].DataItemID);
			AssertEquals(LocalExportAmendmentDataItemIDList.Codes._39, sortAmendItemList[7].DataItemID);
		}

		public void Test5DRAmendedItems()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas5DPSnapshot();
			var declaration = entry.Declaration;
			declaration.JE_SubLocationOfGoods = "새 경남창고";
			declaration.JE_ExportGoodsType = "2";
			declaration.Invoices[0].JZ_DRWApplicantType = "2";
			declaration.Invoices[0].JZ_Weight = 90;
			declaration.Invoices[0].JZ_NoOfPacks = 10;

			var sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5DR);
			AssertEquals(5, sendingObj.AmendedItems.Count);
			var sortAmendItemList = sendingObj.AmendedItems.Cast<AmendedItem>().OrderBy(x => x.DataItemID).ToList();
			AssertEquals(LocalExportAmendmentDataItemIDList.Codes._11A, sortAmendItemList[0].DataItemID);
			AssertEquals(LocalExportAmendmentDataItemIDList.Codes._14, sortAmendItemList[1].DataItemID);
			AssertEquals(LocalExportAmendmentDataItemIDList.Codes._15, sortAmendItemList[2].DataItemID);
			AssertEquals(LocalExportAmendmentDataItemIDList.Codes._16, sortAmendItemList[3].DataItemID);
			AssertEquals(LocalExportAmendmentDataItemIDList.Codes._18, sortAmendItemList[4].DataItemID);
		}

		public void TestAmendmentReasonCodeListAndFaultPartyList()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();
			var sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5AS);
			AssertEquals("Export list", typeof(ExportAmendmentReasonCodeList), sendingObj.Lookups.AmendmentReasonCodeList.GetType());
			AssertEquals("Export list", typeof(ExportImputationReasonCodeList), sendingObj.Lookups.FaultPartyList.GetType());

			entry = new TestDataSetupHelper(Factory).GetEntryHas5BASnapshot();
			sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5BB);
			AssertEquals("Not relevant for 5BB", 0, sendingObj.Lookups.AmendmentReasonCodeList.Count);
			AssertEquals("Not relevant for 5BB", 0, sendingObj.Lookups.FaultPartyList.Count);
		}

		public void TestDateOfFinalPriceIsReadOnly()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();
			var declaration = entry.Declaration;
			declaration.JE_ExportGoodsType = TransactionTypeCodeList.Codes._15;
			var sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5AS);
			sendingObj.ReasonCode = ExportAmendmentReasonCodeList.Codes._28;
			AssertEquals(false, sendingObj.DateOfFinalPriceInfo.ReadOnly);

			sendingObj.DateOfFinalPrice = ZDate.Today;
			sendingObj.ReasonCode = ExportAmendmentReasonCodeList.Codes._24;
			AssertEquals(ZDate.Empty, sendingObj.DateOfFinalPrice);
			AssertEquals(true, sendingObj.DateOfFinalPriceInfo.ReadOnly);
		}

		public void TestAmendItemsReadonly()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();
			var sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5AS);
			AssertEquals(true, sendingObj.AmendedItems.ReadOnly);
		}

		public void TestAmendmentVersion()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_VersionID = 1;

			Assert(!ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(ElectronicDocumentTypeList.Codes._5AS));
			var sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5AS);
			AssertEquals("entry.CH_VersionID", 1u, sendingObj.AmendmentVersion);

			Assert(ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(ElectronicDocumentTypeList.Codes._5BB));
			sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5BB);
			AssertEquals("entryNum5BA.CE_EntryLineReference", 0u, sendingObj.AmendmentVersion);

			var entryNum5BA = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5BA);
			entryNum5BA.CE_EntryLineReference = "2";
			sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5BB);
			AssertEquals("entryNum5BA.CE_EntryLineReference", 2u, sendingObj.AmendmentVersion);

			var message5FE = CreateEDIMessage(ElectronicDocumentTypeList.Codes._5FE, ZString.Empty, "5", ZDateTime.Today.AddDays(-1));
			var messageR99 = CreateEDIMessage(ElectronicDocumentTypeList.Codes._R99, ElectronicDocumentTypeList.Codes._5FE, message5FE.EM_MessageNum, ZDateTime.Today.AddDays(-1));
			entry.CH_VersionID = 5;
			sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			AssertEquals("CH_VersionID + 1 - EM_ApplicationReferenc(5FE Message.CreateTime < Today)", 1u, sendingObj.AmendmentVersion);

			EDIMessage CreateEDIMessage(ZString messageType, ZString messageSubType, ZString applicationReference, ZDateTime createTime)
			{
				var message = entry.Messages.AddNew();
				message.EM_MessageType = messageType;
				message.EM_MessageSubType = messageSubType;
				message.EM_ApplicationReference = applicationReference;
				message.EM_SystemCreateTimeUtc = createTime;

				return message;
			}
		}

		public void Test5FEItemsForCusEntryInstructionColumns()
		{
			SetupImportDecHavingStatement(out var declaration, out var entry, out var invoice, out var entryLine, out _);
			Create929Snapshot(entry);
			var specialConsumptionTax = entry.Charges.AddNew();
			specialConsumptionTax.C1_ChargeType = ChargeTypeList.Codes.SpecialConsumptionTax;
			specialConsumptionTax.C1_ChargeAmount = 30;
			entry.CH_VersionID = 1;
			var sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			AssertEquals(ZString.Empty, sendingObj.TaxPenaltyCause);
			AssertEquals(ZString.Empty, sendingObj.DutyPenaltyCause);
			AssertEquals(ZString.Empty, sendingObj.ApplyDutyPenaltyReduction);

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			AssertEquals(1, instruction.AmendmentSessionalDataCollection.Count);
			AssertEquals(ZString.Empty, sendingObj.TaxPenaltyCause);
			AssertEquals(ZString.Empty, sendingObj.DutyPenaltyCause);
			AssertEquals(ZString.Empty, sendingObj.ApplyDutyPenaltyReduction);

			var amendmentSessionalData = instruction.AmendmentSessionalDataCollection.FirstOrDefault();
			amendmentSessionalData.TaxPenaltyCause = "01";
			amendmentSessionalData.DutyPenaltyCause = "02";
			amendmentSessionalData.CSI_Code = "A";
			amendmentSessionalData.PenaltyExemptionSessionalData.ApplyDutyPenaltyReduction = "Y";
			sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			AssertEquals("01", sendingObj.TaxPenaltyCause);
			AssertEquals("02", sendingObj.DutyPenaltyCause);
			AssertEquals("Y", sendingObj.ApplyDutyPenaltyReduction);
		}

		public void TestPenaltyExemptionReqSequence()
		{
			SetupImportDecHavingStatement(out var declaration, out var entry, out var invoice, out var entryLine, out _);
			Create929Snapshot(entry);
			var specialConsumptionTax = entry.Charges.AddNew();
			specialConsumptionTax.C1_ChargeType = ChargeTypeList.Codes.SpecialConsumptionTax;
			specialConsumptionTax.C1_ChargeAmount = 30;
			entry.CH_VersionID = 1;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			sendingObj.PenaltyExemptionIndicator = Constants.YesNo.No;
			Assert(!sendingObj.IsIncluding5UAIn5FE);
			AssertEquals(0, sendingObj.PenaltyExemptionReqSequence);
			sendingObj.PenaltyExemptionReasonCode = PenaltyExemptionReasonCodeList.Codes.A2;
			Assert(!sendingObj.IsIncluding5UAIn5FE);
			AssertEquals(0, sendingObj.PenaltyExemptionReqSequence);

			entry.CH_VersionID = 2;
			sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			sendingObj.PenaltyExemptionIndicator = Constants.YesNo.Yes;
			sendingObj.PenaltyExemptionReasonCode = PenaltyExemptionReasonCodeList.Codes.A2;
			Assert(!sendingObj.IsIncluding5UAIn5FE);
			AssertEquals(0, sendingObj.PenaltyExemptionReqSequence);
			sendingObj.PenaltyExemptionReasonCode = PenaltyExemptionReasonCodeList.Codes.B5;
			Assert(sendingObj.IsIncluding5UAIn5FE);
			AssertEquals(1, sendingObj.PenaltyExemptionReqSequence);
			sendingObj.PenaltyExemptionIndicator = Constants.YesNo.No;
			Assert(!sendingObj.IsIncluding5UAIn5FE);
			AssertEquals(0, sendingObj.PenaltyExemptionReqSequence);

			var entryNum5UA = entry.EntryNumbers.AddNew();
			entryNum5UA.CE_EntryType = ElectronicDocumentTypeList.Codes._5UA;
			entryNum5UA.CE_EntryLineReference = "2";

			sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			sendingObj.PenaltyExemptionIndicator = Constants.YesNo.Yes;
			sendingObj.PenaltyExemptionReasonCode = PenaltyExemptionReasonCodeList.Codes.A2;
			Assert(!sendingObj.IsIncluding5UAIn5FE);
			AssertEquals(0, sendingObj.PenaltyExemptionReqSequence);
			sendingObj.PenaltyExemptionReasonCode = PenaltyExemptionReasonCodeList.Codes.A4;
			Assert(sendingObj.IsIncluding5UAIn5FE);
			AssertEquals(3, sendingObj.PenaltyExemptionReqSequence);
			sendingObj.PenaltyExemptionIndicator = Constants.YesNo.No;
			Assert(!sendingObj.IsIncluding5UAIn5FE);
			AssertEquals(0, sendingObj.PenaltyExemptionReqSequence);
		}

		public void TestAmendedTaxItems()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntry929FullData(PaymentMethodCodeList.Codes._00, ZBool.True);

			using (var stream = KRXmlObjectSerializer.Serialize(new ImportEntryHeaderCreator().Create(entry)))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._929, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._929);
				Factory.Save();
			}

			UpdateOrCreateEntryHeaderCharges(ChargeTypeList.Codes.EducationTax, 1m);
			UpdateOrCreateEntryHeaderCharges(ChargeTypeList.Codes.PenaltyForLateDeclaration, 2m);
			UpdateOrCreateEntryHeaderCharges(ChargeTypeList.Codes.PenaltyForMissedDeclaration, 3m);
			UpdateOrCreateEntryHeaderCharges(ChargeTypeList.Codes.LiquorTax, 4m);
			UpdateOrCreateEntryHeaderCharges(ChargeTypeList.Codes.AgricultureTax, 5m);
			UpdateOrCreateEntryHeaderCharges(ChargeTypeList.Codes.Duty, 6m);
			UpdateOrCreateEntryHeaderCharges(ChargeTypeList.Codes.TransportationTax, 7m);
			UpdateOrCreateEntryHeaderCharges(ChargeTypeList.Codes.SpecialConsumptionTax, 8m);
			UpdateOrCreateEntryHeaderCharges(ChargeTypeList.Codes.VAT, 9m);

			Factory.Save();
			var sendingObject = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			AssertEquals(9, sendingObject.AmendedDutyTaxItems.Count);

			AssertTaxItem(sendingObject.AmendedDutyTaxItems, EntryTaxTypeList.Codes._5AB, EntryTaxTypeList.Descriptions._5AB, 0, 1m);
			AssertTaxItem(sendingObject.AmendedDutyTaxItems, EntryTaxTypeList.Codes._5AC, EntryTaxTypeList.Descriptions._5AC, 0, 2m);
			AssertTaxItem(sendingObject.AmendedDutyTaxItems, EntryTaxTypeList.Codes._5AY, EntryTaxTypeList.Descriptions._5AY, 0, 3m);
			AssertTaxItem(sendingObject.AmendedDutyTaxItems, EntryTaxTypeList.Codes.ACT, EntryTaxTypeList.Descriptions.ACT, 0, 4m);
			AssertTaxItem(sendingObject.AmendedDutyTaxItems, EntryTaxTypeList.Codes.CAP, EntryTaxTypeList.Descriptions.CAP, 0, 5m);
			AssertTaxItem(sendingObject.AmendedDutyTaxItems, EntryTaxTypeList.Codes.CUD, EntryTaxTypeList.Descriptions.CUD, 1999999999990m, 6m);
			AssertTaxItem(sendingObject.AmendedDutyTaxItems, EntryTaxTypeList.Codes.ENV, EntryTaxTypeList.Descriptions.ENV, 0, 7m);
			AssertTaxItem(sendingObject.AmendedDutyTaxItems, EntryTaxTypeList.Codes.IND, EntryTaxTypeList.Descriptions.IND, 0, 8m);
			AssertTaxItem(sendingObject.AmendedDutyTaxItems, EntryTaxTypeList.Codes.VAT, EntryTaxTypeList.Descriptions.VAT, 1999999999990m, 9m);

			void UpdateOrCreateEntryHeaderCharges(string chargeType, decimal chargeAmount)
			{
				var charge = entry.Charges.FirstOrDefault(x => x.C1_ChargeType == chargeType);
				if (charge == null)
				{
					charge = entry.Charges.AddNew();
					charge.C1_ChargeType = chargeType;
				}
				charge.C1_ChargeAmount = chargeAmount;
			}

			void AssertTaxItem(AmendedItemCollection taxItems, ZString type, ZString typeDescription, ZDecimal before, ZDecimal after)
			{
				var taxItem = taxItems.Cast<AmendedItem>().FirstOrDefault(x => x.DutyTaxType == type);

				AssertEquals(typeDescription, taxItem.DutyTaxTypeDescription);
				AssertEquals(before, taxItem.BeforeAmount);
				AssertEquals(after, taxItem.AfterAmount);
				AssertEquals(after - before, taxItem.AmountDifference);
			}
		}

		public void Test5FEAmendedItems()
		{
			SetupImportDecHavingStatement(out var declaration, out var entry, out var invoice, out var entryLine, out _);
			Create929Snapshot(entry);

			declaration.JE_MessageSubType = "A";
			declaration.JE_TradeType = "11";
			invoice.JZ_ValuationDecAttachCode = "Y";
			AddEntryLineFee(ChargeTypeList.Codes.SpecialConsumptionTax);
			AddEntryLineFee(ChargeTypeList.Codes.TransportationTax);
			AddEntryLineFee(ChargeTypeList.Codes.EducationTax);
			AddEntryLineFee(ChargeTypeList.Codes.AgricultureTax);
			AddEntryLineFee(ChargeTypeList.Codes.LiquorTax);
			UpdateOrCreateEntryCharge(ChargeTypeList.Codes.SpecialConsumptionTax);
			UpdateOrCreateEntryCharge(ChargeTypeList.Codes.TransportationTax);
			UpdateOrCreateEntryCharge(ChargeTypeList.Codes.EducationTax);
			UpdateOrCreateEntryCharge(ChargeTypeList.Codes.AgricultureTax);
			UpdateOrCreateEntryCharge(ChargeTypeList.Codes.LiquorTax);

			var sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			var amendedItems = sendingObj.AmendedItems.Cast<AmendedItem>();
			AssertEquals(11, sendingObj.AmendedItems.Count);

			AssertEquals(11, sendingObj.TotalAmendedItemsCount);
			AssertNotNull(amendedItems.Any(x => x.DataItemID == ImportAmendmentDataItemIDList.Codes.A402));
			AssertNotNull(amendedItems.Any(x => x.DataItemID == ImportAmendmentDataItemIDList.Codes.A403));
			AssertNotNull(amendedItems.Any(x => x.DataItemID == ImportAmendmentDataItemIDList.Codes.A407));
			AssertNotNull(amendedItems.Any(x => x.DataItemID == ImportAmendmentDataItemIDList.Codes.A824));
			AssertNotNull(amendedItems.Any(x => x.DataItemID == ImportAmendmentDataItemIDList.Codes.B601));
			AssertNotNull(amendedItems.Any(x => x.DataItemID == ImportAmendmentDataItemIDList.Codes.B701));
			AssertNotNull(amendedItems.Any(x => x.DataItemID == ImportAmendmentDataItemIDList.Codes.A816));
			AssertNotNull(amendedItems.Any(x => x.DataItemID == ImportAmendmentDataItemIDList.Codes.A817));
			AssertNotNull(amendedItems.Any(x => x.DataItemID == ImportAmendmentDataItemIDList.Codes.A818));
			AssertNotNull(amendedItems.Any(x => x.DataItemID == ImportAmendmentDataItemIDList.Codes.A820));
			AssertNotNull(amendedItems.Any(x => x.DataItemID == ImportAmendmentDataItemIDList.Codes.A821));

			AssertEquals(5, sendingObj.TotalAmendedTaxCount);
			AssertNotNull(amendedItems.Any(x => x.DataItemID == ImportAmendmentDataItemIDList.Codes.A816));
			AssertNotNull(amendedItems.Any(x => x.DataItemID == ImportAmendmentDataItemIDList.Codes.A817));
			AssertNotNull(amendedItems.Any(x => x.DataItemID == ImportAmendmentDataItemIDList.Codes.A818));
			AssertNotNull(amendedItems.Any(x => x.DataItemID == ImportAmendmentDataItemIDList.Codes.A820));
			AssertNotNull(amendedItems.Any(x => x.DataItemID == ImportAmendmentDataItemIDList.Codes.A821));

			void AddEntryLineFee(string chargeType)
			{
				var fee = entryLine.Fees.AddNew();
				fee.CF_ChargeType = chargeType;
				fee.CF_ChargeAmount = 10;
			}
			void UpdateOrCreateEntryCharge(string chargeType)
			{
				var charge = entry.Charges.AddNew();
				charge.C1_ChargeType = chargeType;
				charge.C1_ChargeAmount = 10;
			}
		}

		void SetupImportDecHavingStatement(out JobDeclaration declaration, out CusEntryHeader entry, out JobComInvoiceHeader invoice, out CusEntryLine entryLine, out CusStatementHeader statement929)
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			entry = declaration.CustomsEntryHeaders[0];
			invoice = declaration.Invoices[0];
			entryLine = entry.MergedLines[0];

			entry.EntryNumber = "1234525000001M";
			statement929 = Factory.New<CusStatementHeader>();
			statement929.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statement929.B2_PaymentAuthorizationDate = new ZDateTime(2025, 01, 14);

			var statementLine = statement929.StatementLines.AddNew();
			statementLine.B3_EntryType = KRJobMessageTypeList.Codes.Import;
			statementLine.B3_EntryNum = entry.EntryNumber;
		}
		void Create929Snapshot(CusEntryHeader entry)
		{
			var import929 = new ImportEntryHeaderCreator().Create(entry);
			import929.RoundDecimalValueRoundedWithDecimalPlaces();
			using (var stream = KRXmlObjectSerializer.Serialize(import929))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._929, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._929);
				Factory.Save();
			}
		}

		public void TestDeclarantType()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var entry = declaration.CustomsEntryHeaders.AddNew();
				var sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
				AssertEquals(false, declaration.IsSelfDeclaringOwner);
				AssertEquals("01", sendingObj.DeclarantType);
			}

			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "62345"))
			{
				var entry = declaration.CustomsEntryHeaders.AddNew();
				var sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
				AssertEquals(true, declaration.IsSelfDeclaringOwner);
				AssertEquals("05", sendingObj.DeclarantType);
			}
		}
		public void TestSubmissionDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			AssertEquals(ZDateTime.Today, sendingObj.SubmissionDate);
		}
		public void Test5FEItemsForCusEntryColumnsExceptInstruction()
		{
			SetupImportDecHavingStatement(out var declaration, out var entry, out var invoice, out var entryLine, out _);

			var educationTax = entry.Charges.AddNew();
			educationTax.C1_ChargeType = ChargeTypeList.Codes.EducationTax;
			educationTax.C1_ChargeAmount = 10;
			entryLine.CL_CustomsValue = 100;

			entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5UA).CE_EntryLineReference = "1";

			Create929Snapshot(entry);

			entryLine.CL_CustomsValue = 300;
			entry.ResetIsCustomsValueCalculated();

			var specialConsumptionTax = entry.Charges.AddNew();
			specialConsumptionTax.C1_ChargeType = ChargeTypeList.Codes.SpecialConsumptionTax;
			specialConsumptionTax.C1_ChargeAmount = 30;

			var sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			sendingObj.PenaltyExemptionIndicator = "Y";
			AssertEquals(10m, sendingObj.BeforeTotalDutyTaxAmount);
			AssertEquals(40m, sendingObj.AfterTotalDutyTaxAmount);
			AssertEquals(30m, sendingObj.DutyTaxDifference);
			AssertEquals(100m, sendingObj.BeforeCustomsValue);
			AssertEquals(300m, sendingObj.AfterCustomsValue);
			AssertEquals(200m, sendingObj.CustomsValueDifference);
			AssertEquals("Requires EntryInstruction.AmendmentSessionalData.PenaltyExemptionSessionalData.", 0, sendingObj.PenaltyExemptionReqSequence);
		}

		[TestDate(2025, 2, 20)]
		public void TestIsPenaltyExemptionIrrelevant()
		{
			SetupImportDecHavingStatement(out var declaration, out var entry, out var invoice, out var entryLine, out var statement929);
			var sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var amendmentSessionalData = instruction.AmendmentSessionalDataCollection.First();

			AssertNull(amendmentSessionalData);
			Assert(sendingObj.PenaltyExemptionIndicatorInfo.ReadOnly);

			entry.CH_CEI_Instruction = instruction.PK;
			sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			amendmentSessionalData = instruction.AmendmentSessionalDataCollection.First();

			AssertEquals(DutyTaxCorrectionCodeList.Codes.X, amendmentSessionalData.CSI_Code);
			AssertNull(amendmentSessionalData.PenaltyExemptionSessionalData);
			AssertNull(amendmentSessionalData.ValidPenaltyExemptionSessionalData);
			Assert(sendingObj.PenaltyExemptionIndicatorInfo.ReadOnly);

			Create929Snapshot(entry);
			var specialConsumptionTax = entry.Charges.AddNew();
			specialConsumptionTax.C1_ChargeType = ChargeTypeList.Codes.SpecialConsumptionTax;
			specialConsumptionTax.C1_ChargeAmount = 30;
			sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			amendmentSessionalData = instruction.AmendmentSessionalDataCollection.First();
			AssertEquals(DutyTaxCorrectionCodeList.Codes.A, amendmentSessionalData.CSI_Code);
			AssertNotNull(amendmentSessionalData.PenaltyExemptionSessionalData);
			AssertNull(amendmentSessionalData.ValidPenaltyExemptionSessionalData);
			Assert(!sendingObj.PenaltyExemptionIndicatorInfo.ReadOnly);

			amendmentSessionalData.PenaltyExemptionSessionalData.CSI_Code = DutyPenaltyExemptionCodeList.Codes.Y;
			AssertNotNull(amendmentSessionalData.ValidPenaltyExemptionSessionalData);

			instruction.AmendmentSessionalDataCollection.RemoveAndDeleteAll();
			statement929.B2_PaymentAuthorizationDate = ZDateTime.Today.AddMonths(-6).AddDays(-1);
			sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			amendmentSessionalData = instruction.AmendmentSessionalDataCollection.First();
			AssertEquals(DutyTaxCorrectionCodeList.Codes.B, amendmentSessionalData.CSI_Code);
			AssertNotNull(amendmentSessionalData.PenaltyExemptionSessionalData);
			Assert(!sendingObj.PenaltyExemptionIndicatorInfo.ReadOnly);

			instruction.AmendmentSessionalDataCollection.RemoveAndDeleteAll();
			specialConsumptionTax.C1_ChargeAmount = -30;
			sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			amendmentSessionalData = instruction.AmendmentSessionalDataCollection.First();
			AssertEquals(DutyTaxCorrectionCodeList.Codes.C, amendmentSessionalData.CSI_Code);
			AssertNull(amendmentSessionalData.PenaltyExemptionSessionalData);
			Assert(sendingObj.PenaltyExemptionIndicatorInfo.ReadOnly);
		}

		public void TestIsNoPenaltyExemptionRequestReadonly()
		{
			SetupImportDecHavingStatement(out var declaration, out var entry, out var invoice, out var entryLine, out _);
			Create929Snapshot(entry);
			var specialConsumptionTax = entry.Charges.AddNew();
			specialConsumptionTax.C1_ChargeType = ChargeTypeList.Codes.SpecialConsumptionTax;
			specialConsumptionTax.C1_ChargeAmount = 30;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;

			var amendmentSessionalData = instruction.AmendmentSessionalDataCollection.AddNew();
			var sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			sendingObj.PenaltyExemptionIndicator = ZString.Empty;
			AssertReadOnly(true);

			sendingObj.PenaltyExemptionIndicator = "Z";
			AssertReadOnly(true);

			sendingObj.PenaltyExemptionIndicator = DutyPenaltyExemptionCodeList.Codes.X;
			AssertReadOnly(true);

			sendingObj.PenaltyExemptionIndicator = DutyPenaltyExemptionCodeList.Codes.N;
			AssertReadOnly(true);

			sendingObj.PenaltyExemptionIndicator = DutyPenaltyExemptionCodeList.Codes.Y;
			AssertReadOnly(false);

			void AssertReadOnly(bool value)
			{
				AssertEquals(value, sendingObj.PenaltyExemptionReasonCodeInfo.ReadOnly);
				AssertEquals(value, sendingObj.PenaltyExemptionReasonInfo.ReadOnly);
				AssertEquals(true, sendingObj.PenaltyExemptionAmountInfo.ReadOnly);
				AssertEquals(value, sendingObj.PenaltyPaymentReasonCodeInfo.ReadOnly);
			}
		}

		public void TestProxyFields()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.JE_TaxOffice = "613";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "1";

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_RefundCauseCode = RefundCauseCodeList.Codes._12;
			instruction.CEI_RefundReasonCode = RefundReasonCodeList.Codes._03;
			instruction.CEI_StatementNumber5WN = "192113334901920";
			entry.CH_CEI_Instruction = instruction.PK;
			entry.CH_VersionID = 1;

			var message929 = entry.Messages.AddNew();
			message929.EM_MessageType = KRJobMessageTypeList.Codes.Import;
			message929.EM_MessageNum = "1";
			message929.EM_ApplicationReference = "1";

			var message5FE = entry.Messages.AddNew();
			message5FE.EM_MessageType = ElectronicDocumentTypeList.Codes._5FE;
			message5FE.EM_MessageNum = "2";
			message5FE.EM_ApplicationReference = "2";

			var statement929 = Factory.New<CusStatementHeader>();
			statement929.B2_IncomingMessageNo = message5FE.EM_MessageNum;
			statement929.B2_ProcessDate = new ZDateTime(2024, 1, 1);
			statement929.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statement929.B2_StatementNumber = "1234567890123456789";
			statement929.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
			var statementLine = statement929.StatementLines.AddNew();
			statementLine.B3_AssociatedEntry = "9876543210987654321";
			statementLine.B3_EntryType = SharedJobMessageTypeList.Codes.Import;
			statementLine.B3_EntryNum = "1";
			Factory.Save();

			AssertEquals(statement929.PK, entry.Statement929.PK);
			var sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			AssertEquals(ZString.Empty, sendingObj.RefundType);
			AssertEquals(ZString.Empty, instruction.CEI_RefundType);

			sendingObj.RefundRequestSubmissionYN = YesNoList.Codes.Yes;
			AssertEquals(RefundTypeList.Codes.A, sendingObj.RefundType);
			AssertEquals(RefundTypeList.Codes.A, instruction.CEI_RefundType);
			AssertEquals(RefundCauseCodeList.Codes._12, sendingObj.RefundCause);
			AssertEquals(RefundReasonCodeList.Codes._03, sendingObj.RefundReason);
			AssertEquals("613", sendingObj.TaxOffice);
			AssertEquals("192113334901920", sendingObj.CustomsDisbursementBillNumber);
			AssertEquals("1234-567-89-01-2-345678-9", sendingObj.FormattedCustomsDisbursementBill);
			AssertEquals((ZShort)0, sendingObj.AmendSeqNo5WN);

			sendingObj.RefundRequestSubmissionYN = YesNoList.Codes.No;
			AssertEquals("N", sendingObj.Is5ULSentWith5FE);
			AssertEquals(RefundTypeList.Codes.A, sendingObj.RefundType);
			AssertEquals(RefundTypeList.Codes.A, instruction.CEI_RefundType);

			instruction.CEI_RefundType = RefundTypeList.Codes.E;
			AssertEquals(RefundTypeList.Codes.E, sendingObj.RefundType);
			sendingObj.RefundRequestSubmissionYN = YesNoList.Codes.Yes;
			AssertEquals("Y", sendingObj.Is5ULSentWith5FE);
			AssertEquals(RefundTypeList.Codes.E, sendingObj.RefundType);
			AssertEquals(RefundTypeList.Codes.E, instruction.CEI_RefundType);

			sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5AS);
			sendingObj.RefundRequestSubmissionYN = YesNoList.Codes.Yes;
			AssertEquals("N", sendingObj.Is5ULSentWith5FE);

			statement929.B2_StatementType = StatementHeaderTypeList.Codes.Invoice;
			instruction.CEI_RefundType = RefundTypeList.Codes.B;
			sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			AssertEquals("9876-543-21-09-8-765432-1", sendingObj.FormattedCustomsDisbursementBill);
		}

		public void TestRefundAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.EntryNumber = "1";
			SetEntryChargeAndLineCharge(entry1, true);

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.EntryNumber = "2";
			SetEntryChargeAndLineCharge(entry2, false);

			AssertNull(entry1.StatementLine929);
			var sendingObj1 = new JobDeclarationAmendmentMessageSendingObject(entry1, ElectronicDocumentTypeList.Codes._5FE);
			AssertEquals(8m, sendingObj1.RefundAmountOfPenaltyForLateDeclaration);
			AssertEquals(9m, sendingObj1.RefundAmountOfPenaltyForMissedDeclaration);
			AssertEquals(1m, sendingObj1.RefundAmountOfDutyAmount);
			AssertEquals(2m, sendingObj1.RefundAmountOfLiquorTax);
			AssertEquals(3m, sendingObj1.RefundAmountOfSpecialConsumptionTax);
			AssertEquals(4m, sendingObj1.RefundAmountOfTransportationTax);
			AssertEquals(5m, sendingObj1.RefundAmountOfEducationTax);
			AssertEquals(6m, sendingObj1.RefundAmountOfAgricultureTax);
			AssertEquals(7m, sendingObj1.RefundAmountOfVAT);
			AssertEquals(70m, sendingObj1.RefundAmountOfValueForVAT);
			AssertEquals(45m, sendingObj1.TotalAmountOfRefundAmount);

			AssertNull(entry2.StatementLine929);
			var sendingObj2 = new JobDeclarationAmendmentMessageSendingObject(entry2, ElectronicDocumentTypeList.Codes._5FE);
			AssertEquals(0m, sendingObj2.RefundAmountOfPenaltyForLateDeclaration);
			AssertEquals(0m, sendingObj2.RefundAmountOfPenaltyForMissedDeclaration);
			AssertEquals(0m, sendingObj2.RefundAmountOfDutyAmount);
			AssertEquals(0m, sendingObj2.RefundAmountOfLiquorTax);
			AssertEquals(0m, sendingObj2.RefundAmountOfSpecialConsumptionTax);
			AssertEquals(0m, sendingObj2.RefundAmountOfTransportationTax);
			AssertEquals(0m, sendingObj2.RefundAmountOfEducationTax);
			AssertEquals(0m, sendingObj2.RefundAmountOfAgricultureTax);
			AssertEquals(0m, sendingObj2.RefundAmountOfVAT);
			AssertEquals(0m, sendingObj2.RefundAmountOfValueForVAT);
			AssertEquals(0m, sendingObj2.TotalAmountOfRefundAmount);

			var statement929 = Factory.New<CusStatementHeader>();
			statement929.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			var statementLine = statement929.StatementLines.AddNew();
			statementLine.B3_EntryType = SharedJobMessageTypeList.Codes.Import;
			statementLine.B3_EntryNum = entry2.EntryNumber;
			AddStatementLineCharge(statementLine, ChargeTypeList.Codes.PenaltyForLateDeclaration, 80m);
			AddStatementLineCharge(statementLine, ChargeTypeList.Codes.PenaltyForMissedDeclaration, 90m);
			AddStatementLineCharge(statementLine, ChargeTypeList.Codes.Duty, 10m);
			AddStatementLineCharge(statementLine, ChargeTypeList.Codes.LiquorTax, 20m);
			AddStatementLineCharge(statementLine, ChargeTypeList.Codes.SpecialConsumptionTax, 30m);
			AddStatementLineCharge(statementLine, ChargeTypeList.Codes.TransportationTax, 40m);
			AddStatementLineCharge(statementLine, ChargeTypeList.Codes.EducationTax, 50m);
			AddStatementLineCharge(statementLine, ChargeTypeList.Codes.AgricultureTax, 60m);
			AddStatementLineCharge(statementLine, ChargeTypeList.Codes.VAT, 70m);
			Factory.Save();

			AssertNotNull(entry2.StatementLine929);
			sendingObj2 = new JobDeclarationAmendmentMessageSendingObject(entry2, ElectronicDocumentTypeList.Codes._5FE);
			AssertEquals(0m, sendingObj2.RefundAmountOfPenaltyForLateDeclaration);
			AssertEquals(0m, sendingObj2.RefundAmountOfPenaltyForMissedDeclaration);
			AssertEquals(0m, sendingObj2.RefundAmountOfDutyAmount);
			AssertEquals(0m, sendingObj2.RefundAmountOfLiquorTax);
			AssertEquals(0m, sendingObj2.RefundAmountOfSpecialConsumptionTax);
			AssertEquals(0m, sendingObj2.RefundAmountOfTransportationTax);
			AssertEquals(0m, sendingObj2.RefundAmountOfEducationTax);
			AssertEquals(0m, sendingObj2.RefundAmountOfAgricultureTax);
			AssertEquals(0m, sendingObj2.RefundAmountOfVAT);
			AssertEquals(0m, sendingObj2.RefundAmountOfValueForVAT);
			AssertEquals(0m, sendingObj2.TotalAmountOfRefundAmount);

			sendingObj2.RefundAmountOfPenaltyForLateDeclaration = 10m;
			sendingObj2.RefundAmountOfPenaltyForMissedDeclaration = 20m;
			sendingObj2.RefundAmountOfDutyAmount = 30m;
			sendingObj2.RefundAmountOfLiquorTax = 40m;
			sendingObj2.RefundAmountOfSpecialConsumptionTax = 50m;
			sendingObj2.RefundAmountOfTransportationTax = 60m;
			sendingObj2.RefundAmountOfEducationTax = 70m;
			sendingObj2.RefundAmountOfAgricultureTax = 80m;
			sendingObj2.RefundAmountOfVAT = 90m;
			sendingObj2.RefundAmountOfPenaltyForLatePayment = 100m;
			sendingObj2.RefundAmountOfNonDutyTaxRevenue = 110m;
			AssertEquals(900m, sendingObj2.RefundAmountOfValueForVAT);
			AssertEquals(660m, sendingObj2.TotalAmountOfRefundAmount);
			AssertEquals(240m, sendingObj2.TotalLateRefundAmount);

			void SetEntryChargeAndLineCharge(CusEntryHeader entry, bool isNegative)
			{
				if (isNegative)
				{
					AddEntryCharge(entry, ChargeTypeList.Codes.Duty, -1m);
					AddEntryCharge(entry, ChargeTypeList.Codes.LiquorTax, -2m);
					AddEntryCharge(entry, ChargeTypeList.Codes.SpecialConsumptionTax, -3m);
					AddEntryCharge(entry, ChargeTypeList.Codes.TransportationTax, -4m);
					AddEntryCharge(entry, ChargeTypeList.Codes.EducationTax, -5m);
					AddEntryCharge(entry, ChargeTypeList.Codes.AgricultureTax, -6m);
					AddEntryCharge(entry, ChargeTypeList.Codes.VAT, -7m);
					AddEntryCharge(entry, ChargeTypeList.Codes.PenaltyForLateDeclaration, -8m);
					AddEntryCharge(entry, ChargeTypeList.Codes.PenaltyForMissedDeclaration, -9m);
				}
				else
				{
					AddEntryCharge(entry, ChargeTypeList.Codes.Duty, 1m);
					AddEntryCharge(entry, ChargeTypeList.Codes.LiquorTax, 2m);
					AddEntryCharge(entry, ChargeTypeList.Codes.SpecialConsumptionTax, 3m);
					AddEntryCharge(entry, ChargeTypeList.Codes.TransportationTax, 4m);
					AddEntryCharge(entry, ChargeTypeList.Codes.EducationTax, 5m);
					AddEntryCharge(entry, ChargeTypeList.Codes.AgricultureTax, 6m);
					AddEntryCharge(entry, ChargeTypeList.Codes.VAT, 7m);
					AddEntryCharge(entry, ChargeTypeList.Codes.PenaltyForLateDeclaration, 8m);
					AddEntryCharge(entry, ChargeTypeList.Codes.PenaltyForMissedDeclaration, 9m);
				}

				void AddEntryCharge(CusEntryHeader entry, string chargeType, decimal chargeAmount)
				{
					var charge = entry.Charges.AddNew();
					charge.C1_ChargeType = chargeType;
					charge.C1_ChargeAmount = chargeAmount;
				}
			}

			void AddStatementLineCharge(CusStatementLine line, string chargeType, decimal chargeAmount)
			{
				var charge = line.Charges.AddNew();
				charge.B4_ChargeType = chargeType;
				charge.B4_ChargeAmount = chargeAmount;
			}
		}

		[TestDate(2025, 2, 20)]
		public void TestIsIncluding5UAIn5FE()
		{
			SetupImportDecHavingStatement(out var declaration, out var entry, out var invoice, out var entryLine, out _);
			Create929Snapshot(entry);
			var specialConsumptionTax = entry.Charges.AddNew();
			specialConsumptionTax.C1_ChargeType = ChargeTypeList.Codes.SpecialConsumptionTax;
			specialConsumptionTax.C1_ChargeAmount = 30;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;

			var sendingObject = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			sendingObject.PenaltyExemptionIndicator = Constants.YesNo.No;
			AssertEquals(false, sendingObject.IsIncluding5UAIn5FE);
			sendingObject.PenaltyExemptionIndicator = Constants.YesNo.Yes;
			AssertEquals(false, sendingObject.IsIncluding5UAIn5FE);
			sendingObject.PenaltyExemptionReasonCode = PenaltyExemptionReasonCodeList.Codes.A1;
			AssertEquals(false, sendingObject.IsIncluding5UAIn5FE);
			sendingObject.PenaltyExemptionReasonCode = PenaltyExemptionReasonCodeList.Codes.A3;
			AssertEquals(true, sendingObject.IsIncluding5UAIn5FE);
			sendingObject.PenaltyExemptionIndicator = Constants.YesNo.No;
			AssertEquals(false, sendingObject.IsIncluding5UAIn5FE);
		}
		[TestDate(2024, 5, 1)]
		public void TestRefundRequestSubmissionYNIsReadOnly()
		{
			var entry = new TestDataSetupHelper(Factory).Create929SnapShot(BondedFactoryUseCodeList.Codes.A);

			var statement929 = Factory.New<CusStatementHeader>();
			statement929.B2_ProcessDate = new ZDateTime(2024, 2, 1);
			statement929.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statement929.B2_StatementNumber = "1234567890123456789";
			statement929.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
			var statementLine = statement929.StatementLines.AddNew();
			statementLine.B3_AssociatedEntry = "9876543210987654321";
			statementLine.B3_EntryType = SharedJobMessageTypeList.Codes.Import;
			statementLine.B3_EntryNum = "1234520000045M";
			Factory.Save();

			var sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			AssertEquals(true, sendingObj.RefundRequestSubmissionYNInfo.ReadOnly);

			statement929.B2_PaymentAuthorizationDate = ZDateTime.Today.AddMonths(-7);
			Factory.Save();
			entry.Reload();
			var vat = entry.Charges[0];
			vat.C1_ChargeAmount = 500m;
			sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			AssertEquals(false, sendingObj.RefundRequestSubmissionYNInfo.ReadOnly);
		}

		public void TestDecimalPlaces()
		{
			var entry = Factory.New<CusEntryHeader>();
			var sendingObject = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);

			AssertHasCustomAttribute<DecimalPlacesAttribute>(sendingObject.GetType(), "RefundAmountOfDutyAmount", true, attrib => attrib.DecimalPlaces == 0);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(sendingObject.GetType(), "RefundAmountOfLiquorTax", true, attrib => attrib.DecimalPlaces == 0);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(sendingObject.GetType(), "RefundAmountOfSpecialConsumptionTax", true, attrib => attrib.DecimalPlaces == 0);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(sendingObject.GetType(), "RefundAmountOfTransportationTax", true, attrib => attrib.DecimalPlaces == 0);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(sendingObject.GetType(), "RefundAmountOfEducationTax", true, attrib => attrib.DecimalPlaces == 0);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(sendingObject.GetType(), "RefundAmountOfAgricultureTax", true, attrib => attrib.DecimalPlaces == 0);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(sendingObject.GetType(), "RefundAmountOfVAT", true, attrib => attrib.DecimalPlaces == 0);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(sendingObject.GetType(), "RefundAmountOfValueForVAT", true, attrib => attrib.DecimalPlaces == 0);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(sendingObject.GetType(), "RefundAmountOfVATExemptionValue", true, attrib => attrib.DecimalPlaces == 0);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(sendingObject.GetType(), "RefundAmountOfPenaltyForLateDeclaration", true, attrib => attrib.DecimalPlaces == 0);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(sendingObject.GetType(), "RefundAmountOfPenaltyForMissedDeclaration", true, attrib => attrib.DecimalPlaces == 0);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(sendingObject.GetType(), "RefundAmountOfPenaltyForLatePayment", true, attrib => attrib.DecimalPlaces == 0);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(sendingObject.GetType(), "RefundAmountOfNonDutyTaxRevenue", true, attrib => attrib.DecimalPlaces == 0);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(sendingObject.GetType(), "TotalAmountOfRefundAmount", true, attrib => attrib.DecimalPlaces == 0);
		}

		public void TestPenaltyExemptionIndicatorWhenValueIsN()
		{
			SetupImportDecHavingStatement(out var declaration, out var entry, out var invoice, out var entryLine, out _);
			Create929Snapshot(entry);
			var specialConsumptionTax = entry.Charges.AddNew();
			specialConsumptionTax.C1_ChargeType = ChargeTypeList.Codes.SpecialConsumptionTax;
			specialConsumptionTax.C1_ChargeAmount = 30;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;

			var sendingObject = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			AssertEquals("AX", sendingObject.AmendmentType);
			sendingObject.PenaltyExemptionIndicator = DutyPenaltyExemptionCodeList.Codes.Y;
			sendingObject.PenaltyExemptionReasonCode = PenaltyExemptionReasonCodeList.Codes.A1;
			sendingObject.PenaltyExemptionReason = "TEST";
			AssertEquals(PenaltyExemptionReasonCodeList.Codes.A1, sendingObject.PenaltyExemptionReasonCode);
			AssertEquals("TEST", sendingObject.PenaltyExemptionReason);

			sendingObject.PenaltyExemptionIndicator = DutyPenaltyExemptionCodeList.Codes.N;
			AssertEquals(ZString.Empty, sendingObject.PenaltyExemptionReasonCode);
			AssertEquals(ZString.Empty, sendingObject.PenaltyExemptionReason);
		}

		public void TestPenaltyExemptionIndicatorDefaultValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals(0, instruction.AmendmentSessionalDataCollection.Count);

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var sendingObject = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			AssertEquals(1, entry.EntryInstruction.AmendmentSessionalDataCollection.Count);

			var amendmentSessionalData = entry.EntryInstruction.AmendmentSessionalDataCollection[0];
			AssertNull(amendmentSessionalData.PenaltyExemptionSessionalData);
			AssertEquals("When PenaltyExemptionSessionalData is null, return value is X", DutyPenaltyExemptionCodeList.Codes.X, sendingObject.PenaltyExemptionIndicator);

			sendingObject.PenaltyExemptionIndicator = DutyPenaltyExemptionCodeList.Codes.Y;
			AssertEquals(DutyPenaltyExemptionCodeList.Codes.X, sendingObject.PenaltyExemptionIndicator);

			sendingObject.PenaltyExemptionIndicator = DutyPenaltyExemptionCodeList.Codes.N;
			AssertEquals(DutyPenaltyExemptionCodeList.Codes.X, sendingObject.PenaltyExemptionIndicator);
		}
	}
}
