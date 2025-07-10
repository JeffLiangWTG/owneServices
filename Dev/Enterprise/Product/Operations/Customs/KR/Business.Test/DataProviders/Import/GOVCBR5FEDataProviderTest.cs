using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5FEDataProvidersTest : TestCaseWithFactory
	{
		[TestDate(2025, 1, 20)]
		public void TestHeaderData()
		{
			AssertEquals(1, entry.Snapshots.Count);
			ImportEntryHeaderWrapper snapshotData;
			using (var textReader = entry.Snapshots[0].GetCES_SnapshotXmlReader())
			{
				var header = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<ImportEntryHeader>(textReader);
				snapshotData = new ImportEntryHeaderWrapper(entry.Snapshots[0].CES_CH_EntryHeader, header, Factory);
				snapshotData.Decorate((CusEntryHeader)entry.Snapshots[0].EntryHeader);
			}
			AssertEquals(999999999999m, snapshotData.Header.TotalCustomsValueKRW);
			AssertEquals(3999999999980m, snapshotData.Header.TotalPayableAmount);

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(entry.Declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "64512");
			var amendmentEntryLine = entry.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_LineNumber == 2);
			amendmentEntryLine.CL_CustomsValue = 0;
			var charge_DTY = entry.Charges.FirstOrDefault(x => x.C1_ChargeType == "DTY");
			charge_DTY.C1_ChargeAmount = 2999999999990m;
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var loadEntry = factory.Load<CusEntryHeader>(entry.PK);
			loadEntry.CH_VersionID = 1;

			var entryNumber5UL = loadEntry.EntryNumbers.Cast<CusEntryNumber>().Single(x => x.CE_EntryNum == "2" && x.CE_EntryType == ElectronicDocumentTypeList.Codes._5UL);
			entryNumber5UL.CE_EntryLineReference = "1234567890123456789";

			var sendingObject = new JobDeclarationAmendmentMessageSendingObject(loadEntry, ElectronicDocumentTypeList.Codes._5FE);
			loadEntry.EntryInstruction.AmendmentSessionalDataCollection.First().PenaltyExemptionSessionalData.CSI_Value = 1720m;
			sendingObject.PenaltyExemptionReasonCode = "A3";
			sendingObject.PenaltyExemptionReason = "사유";
			sendingObject.ApplyDutyPenaltyReduction = "Y";
			sendingObject.PenaltyExemptionIndicator = "Y";
			sendingObject.RefundRequestSubmissionYN = "Y";

			var result = new Import5FECreator().Create(loadEntry, sendingObject);
			AssertEquals("4062001070010U", result.ImportDeclarationNumber);
			AssertEquals(new DateTime(2013, 01, 01), result.DeclarationDate);
			AssertEquals("010", result.DeclarationCustomsOffice);
			AssertEquals("20", result.DeclarationCustomsDivision);
			AssertEquals("05", result.DeclarantType);
			AssertEquals("모나리자(주)", result.Payer.CompanyName);
			AssertEquals("홍나리", result.Payer.RepresentativeName);
			AssertEquals("상호", result.Declarant.CompanyName);
			AssertEquals("신고인", result.Declarant.RepresentativeName);
			AssertEquals(0, result.TotalAmendedItemCount);
			AssertEquals(0, result.TotalAmendedTaxCount);
			AssertEquals(3999999999980m, result.BeforeTotalDutyTaxAmount);
			AssertEquals(4999999999980m, result.AfterTotalDutyTaxAmount);
			AssertEquals(1000000000000m, result.DutyTaxDifference);
			AssertEquals(999999999999m, result.BeforeCustomsValue);
			AssertEquals(999999999990m, result.AfterCustomsValue);
			AssertEquals(-9m, result.CustomsValueDifference);
			AssertEquals("01", result.DomesticTaxPenaltyType);
			AssertEquals("02", result.DutyPenaltyType);
			AssertEquals("Y", result.DutyPenaltyReducedYN);
			AssertEquals("Y", result.PenaltyExemptionIndicator);
			AssertEquals("A3", result.PenaltyExemptionReasonCode);
			AssertEquals("사유", result.PenaltyExemptionReason);
			AssertEquals(2, result.PenaltyExemptionReqSequence);
			AssertEquals(1720m, result.PenaltyExemptionAmount);
			AssertEquals("2", result.RefundRequestNumber);

			entryNumber5UL.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			factory.Save();

			result = new Import5FECreator().Create(loadEntry, sendingObject);
			AssertEquals(EDIMessage.RefundEntryNumberPlaceHolder, result.RefundRequestNumber);

			sendingObject.RefundRequestSubmissionYN = "N";
			entryNumber5UL.CE_EntryStatus = ZString.Empty;
			factory.Save();

			result = new Import5FECreator().Create(loadEntry, sendingObject);
			AssertEquals(ZString.Empty, result.RefundRequestNumber);
		}

		public void TestDeclarantType()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			var sendingObject1 = new JobDeclarationAmendmentMessageSendingObject(entry1, ElectronicDocumentTypeList.Codes._5FE);
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration1.RegistryCompanyPK, Guid.Empty, Guid.Empty, "60000");
			var result = new Import5FECreator().Create(entry1, sendingObject1);
			AssertEquals("If it starts with 6, 7, 8, P  the value is 05", "05", result.DeclarantType);

			var declaration2 = Factory.New<JobDeclaration>();
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			var sendingObject2 = new JobDeclarationAmendmentMessageSendingObject(entry2, ElectronicDocumentTypeList.Codes._5FE);
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration2.RegistryCompanyPK, Guid.Empty, Guid.Empty, "70000");
			result = new Import5FECreator().Create(entry2, sendingObject2);
			AssertEquals("If it starts with 6, 7, 8, P  the value is 05", "05", result.DeclarantType);

			var declaration3 = Factory.New<JobDeclaration>();
			var entry3 = declaration3.CustomsEntryHeaders.AddNew();
			var sendingObject3 = new JobDeclarationAmendmentMessageSendingObject(entry3, ElectronicDocumentTypeList.Codes._5FE);
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration3.RegistryCompanyPK, Guid.Empty, Guid.Empty, "80000");
			result = new Import5FECreator().Create(entry3, sendingObject3);
			AssertEquals("If it starts with 6, 7, 8, P  the value is 05", "05", result.DeclarantType);

			var declaration4 = Factory.New<JobDeclaration>();
			var entry4 = declaration4.CustomsEntryHeaders.AddNew();
			var sendingObject4 = new JobDeclarationAmendmentMessageSendingObject(entry4, ElectronicDocumentTypeList.Codes._5FE);
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration4.RegistryCompanyPK, Guid.Empty, Guid.Empty, "P0000");
			result = new Import5FECreator().Create(entry4, sendingObject4);
			AssertEquals("If it starts with 6, 7, 8, P  the value is 05", "05", result.DeclarantType);

			var declaration5 = Factory.New<JobDeclaration>();
			var entry5 = declaration5.CustomsEntryHeaders.AddNew();
			var sendingObject5 = new JobDeclarationAmendmentMessageSendingObject(entry5, ElectronicDocumentTypeList.Codes._5FE);
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration5.RegistryCompanyPK, Guid.Empty, Guid.Empty, "X0000");
			result = new Import5FECreator().Create(entry5, sendingObject5);
			AssertEquals("If it not starts with 6, 7, 8, P  the value is 01", "01", result.DeclarantType);
		}

		public void TestPenaltyExemptionRequestType()
		{
			ImportEntryHeaderWrapper snapshotData;
			using (var textReader = entry.Snapshots[0].GetCES_SnapshotXmlReader())
			{
				var header = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<ImportEntryHeader>(textReader);
				snapshotData = new ImportEntryHeaderWrapper(entry.Snapshots[0].CES_CH_EntryHeader, header, Factory);
				snapshotData.Decorate((CusEntryHeader)entry.Snapshots[0].EntryHeader);
			}

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(entry.Declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "64512");
			var amendmentEntryLine = entry.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_LineNumber == 2);
			amendmentEntryLine.CL_CustomsValue = 0;
			var amendmentEntryLineFee = amendmentEntryLine.Fees.Cast<CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == "DTY");
			amendmentEntryLineFee.CF_ChargeAmount = 0;
			var charge_DTY = entry.Charges.FirstOrDefault(x => x.C1_ChargeType == "DTY");
			charge_DTY.C1_ChargeAmount = 2999999999990m;
			Factory.Save();
			var loadEntry = Factory.Load<CusEntryHeader>(entry.PK);

			loadEntry.EntryInstruction.CEI_DutyPenaltyCause = "";
			var sendingObject = new JobDeclarationAmendmentMessageSendingObject(loadEntry, ElectronicDocumentTypeList.Codes._5FE);
			sendingObject.ApplyDutyPenaltyReduction = "Y";
			sendingObject.PenaltyExemptionIndicator = "X";
			var result = new Import5FECreator().Create(loadEntry, sendingObject);
			AssertEquals("Y", result.DutyPenaltyReducedYN);
			AssertEquals("X", result.PenaltyExemptionIndicator);

			loadEntry.EntryInstruction.CEI_DutyPenaltyCause = "01";
			sendingObject.PenaltyExemptionIndicator = "N";
			result = new Import5FECreator().Create(loadEntry, sendingObject);
			AssertEquals("N", result.PenaltyExemptionIndicator);

			sendingObject.ApplyDutyPenaltyReduction = "N";
			sendingObject.PenaltyExemptionIndicator = "Y";
			result = new Import5FECreator().Create(loadEntry, sendingObject);
			AssertEquals("N", result.DutyPenaltyReducedYN);
			AssertEquals("Y", result.PenaltyExemptionIndicator);

			sendingObject.ApplyDutyPenaltyReduction = "N";
			sendingObject.PenaltyExemptionIndicator = "N";
			result = new Import5FECreator().Create(loadEntry, sendingObject);
			AssertEquals("N", result.DutyPenaltyReducedYN);
			AssertEquals("N", result.PenaltyExemptionIndicator);
		}

		public void TestEntryLineAdded()
		{
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 3;
			entryLine.CL_AdValoremTariff = "899999999";
			entryLine.CL_CustomsValue = 100m;

			var container = entry.Declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "899";
			var containerLink = entry.PivotsToContainers.AddNew();
			containerLink.CCE_CO_Container = container.PK;
			containerLink.CCE_SequenceNumber = 1;

			var container2 = entry.Declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "999";
			var containerLink2 = entry.PivotsToContainers.AddNew();
			containerLink2.CCE_CO_Container = container2.PK;
			containerLink2.CCE_SequenceNumber = 2;

			var order = entry.EntryInstruction.OnlineOrders.AddNew();
			order.CY_Order = 1;
			order.CY_Data = "987654321M";

			var immediateDelivery = entryLine.ImmediateDeliveries.AddNew();
			immediateDelivery.CY_Order = 1;
			immediateDelivery.CY_Data = "437654321M";

			var invoiceLine = entry.Declaration.Invoices[0].JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_SequenceNumber = 8;
			invoiceLine.JI_Tariff = "899999999";
			invoiceLine.JI_BrandName = "상표명2";
			invoiceLine.JI_CountryOfOrigin = "US";
			invoiceLine.CriteriaForDeterminingCountryOfOrigin = "B";
			invoiceLine.JI_COOLabelLocation = "Y";

			invoiceLine.JI_Model = "모델규격3";
			invoiceLine.JI_Ingredient = "성분3";
			invoiceLine.JI_LotNumber = "ZZZZEEE";
			invoiceLine.JI_InvoiceUQ = "KG";
			invoiceLine.JI_InvoiceQuantity = 10m;
			invoiceLine.UnitPrice = 200m;
			invoiceLine.JI_LinePrice = 2000m;
			invoiceLine.JI_CustomsUnitQty = "U";
			invoiceLine.JI_CustomsQuantity = 1000m;

			var importPreviousExpDecLine = entryLine.PreviousExpDecLineCollection.AddNew();
			importPreviousExpDecLine.CSI_ReferenceNumber = "CD256855006JP";
			importPreviousExpDecLine.CSI_ReferenceNumber2 = "1";
			importPreviousExpDecLine.CSI_LineNo = 1;

			var gaApprovalData = invoiceLine.GAApprovalDataCollection.AddNew();
			gaApprovalData.CSI_LineNo = 1;
			gaApprovalData.CSI_ReferenceNumber = "AAA";
			gaApprovalData.CSI_ReferenceNumber2 = "BBB";

			var nonGAApprovalData = entryLine.NonGADetailCollection.AddNew();
			nonGAApprovalData.CSI_LineNo = 1;
			nonGAApprovalData.CSI_Code = "A";
			nonGAApprovalData.CSI_Description = "핵물질 수출입요건확인면제(신청)서";

			var sendingObject = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);

			var result = new Import5FECreator().Create(entry, sendingObject);
			AssertEquals(30, result.AmendedItems.Length);
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "B105" && x.EntryLineNo == 3), "01", "", "899999999");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "B102"), "01", "", "모델규격3");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "B104"), "01", "", "상표명2");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "B201"), "01", "", "US");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "B204"), "01", "", "B");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "B202"), "01", "", "Y");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "B306"), "01", "", "U");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "B305"), "01", "", "1000");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "B301"), "01", "", "100");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "B404"), "01", "", "3");

			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "C105" && x.InvoiceLineNo == 8), "01", "", "10");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "C106"), "01", "", "KG");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "C107"), "01", "", "200");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "C108"), "01", "", "2000");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "C103"), "01", "", "성분3");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "C104"), "01", "", "ZZZZEEE");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "D102"), "01", "", "AAA");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "D105"), "01", "", "BBB");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "H101" && x.ContainerNo == 1), "", "", "899");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "H101" && x.ContainerNo == 2), "", "", "999");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "H102" && x.ContainerNo == 1), "", "", "1");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "H102" && x.ContainerNo == 2), "", "", "2");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "I101"), "01", "", "1");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "I102"), "01", "", "437654321M");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "J101"), "", "", "1");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "J102" && x.OnlineOrderNo == 1), "", "", "987654321M");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "E104" && x.NonGASequnceNo == 1), "01", "", "A");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "E105"), "01", "", "핵물질 수출입요건확인면제(신청)서");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "G102"), "01", "", "CD256855006JP");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "G101"), "01", "", "1");

			AssertNotNull(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.ExportDeclarationNumber == "CD256855006JP"));
			//These cases will be implemented in WI00696133
			//AssertNotNull(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.ImmediateDeliveryNo == "437654321M"));
			//AssertNotNull(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.GARequirementApprovalNumber == "AAA"));
			//AssertNotNull(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.ExportDeclarationEntryLineNo == 1));
			//AssertNotNull(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.ExportDeclarationInvoiceLineNo == 1));
		}

		public void TestEntryLineDeleted()
		{
			entry.MergedLines[0].Delete();
			var sendingObject = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			var import5FEHeader = new Import5FECreator().Create(entry, sendingObject);

			AssertNotNull(import5FEHeader.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "B111"));
		}

		public void TestEntryLineChanged()
		{
			var entryLine = entry.MergedLines[0];
			entryLine.CL_AdValoremTariff = "999999999";
			entryLine.CL_CustomsValue = 200m;

			var invoiceLine = entry.MergedLines[0].RandomLine;
			invoiceLine.JI_Tariff = "999999999";
			invoiceLine.JI_BrandName = "상표명3";
			invoiceLine.JI_CountryOfOrigin = "US";
			invoiceLine.CriteriaForDeterminingCountryOfOrigin = "B";
			invoiceLine.JI_COOLabelLocation = "Y";

			invoiceLine.JI_Model = "모델규격4";
			invoiceLine.JI_Ingredient = "성분4";
			invoiceLine.JI_LotNumber = "ZZZZEEE";
			invoiceLine.JI_InvoiceUQ = "KG";
			invoiceLine.JI_InvoiceQuantity = 10m;
			invoiceLine.UnitPrice = 200m;
			invoiceLine.JI_LinePrice = 2000m;
			invoiceLine.JI_CustomsUnitQty = "U";
			invoiceLine.JI_CustomsQuantity = 1000m;

			var hsExtensionCodeData = invoiceLine.HSExtensionCodeCollection.AddNew();
			hsExtensionCodeData.CY_Order = (ZShort)0;
			hsExtensionCodeData.CY_Code = "02";

			var hsExtensionCodeData2 = invoiceLine.HSExtensionCodeCollection.AddNew();
			hsExtensionCodeData2.CY_Order = (ZShort)1;
			hsExtensionCodeData2.CY_Code = "102";
			Factory.Save();

			var sendingObject = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			var result = new Import5FECreator().Create(entry, sendingObject);

			AssertEquals(15, result.AmendedItems.Length);
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "B102"), "03", "RABBIT MEAT", "모델규격4");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "B104"), "03", "상표명", "상표명3");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "B106"), "03", "0208100000-01-101", "999999999-02-102");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "B105"), "03", "0208100000", "999999999");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "B201"), "03", "CN", "US");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "B204"), "03", "A", "B");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "B306"), "03", "DZ", "U");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "B305"), "03", "30", "1000");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "B301"), "03", "999999999990", "200");

			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "C103"), "03", "", "성분4");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "C106"), "03", "", "KG");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "C105"), "03", "0", "10");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "C107"), "03", "0", "200");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "C108"), "03", "0", "2000");
			AssertAmendItem(result.AmendedItems.Cast<IImport5FEItem>().FirstOrDefault(x => x.AmendDataItemID == "C104"), "03", "", "ZZZZEEE");
		}

		void AssertAmendItem(IImport5FEItem amendItem, ZString amendType, ZString beforeDescription, ZString afterDescription)
		{
			AssertNotNull(amendItem);
			AssertEquals(amendType, amendItem.AmendType);
			AssertEquals(beforeDescription, amendItem.BeforeDescription);
			AssertEquals(afterDescription, amendItem.AfterDescription);
		}

		public void TestTaxItems()
		{
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
			var result = new Import5FECreator().Create(entry, sendingObject);

			AssertEquals(9, result.TaxItems.Length);

			AssertTaxItem(result.TaxItems, EntryTaxTypeList.Codes._5AB, 0, 1m);
			AssertTaxItem(result.TaxItems, EntryTaxTypeList.Codes._5AC, 0, 2m);
			AssertTaxItem(result.TaxItems, EntryTaxTypeList.Codes._5AY, 0, 3m);
			AssertTaxItem(result.TaxItems, EntryTaxTypeList.Codes.ACT, 0, 4m);
			AssertTaxItem(result.TaxItems, EntryTaxTypeList.Codes.CAP, 0, 5m);
			AssertTaxItem(result.TaxItems, EntryTaxTypeList.Codes.CUD, 1999999999990m, 6m);
			AssertTaxItem(result.TaxItems, EntryTaxTypeList.Codes.ENV, 0, 7m);
			AssertTaxItem(result.TaxItems, EntryTaxTypeList.Codes.IND, 0, 8m);
			AssertTaxItem(result.TaxItems, EntryTaxTypeList.Codes.VAT, 1999999999990m, 9m);
		}
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

		void AssertTaxItem(Import5FETaxItem[] taxItmes, ZString type, ZDecimal before, ZDecimal after)
		{
			var taxItem = taxItmes.FirstOrDefault(x => x.DutyTaxType == type);

			AssertEquals(before, taxItem.BeforeAmount);
			AssertEquals(after, taxItem.AfterAmount);
			AssertEquals(after - before, taxItem.AmountDifference);
		}

		public void TestPenaltyType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_VersionID = 1;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;

			var amendmentSessionalData = entry.EntryInstruction.AmendmentSessionalDataCollection.AddNew();
			amendmentSessionalData.CSI_LineNo = 2;
			amendmentSessionalData.DutyPenaltyCause = TaxPenaltyTypeCodeList.Codes._0A;
			amendmentSessionalData.TaxPenaltyCause = TaxPenaltyTypeCodeList.Codes._0B;

			var sendingObject1 = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);

			var result = new Import5FECreator().Create(entry, sendingObject1);
			AssertEquals(TaxPenaltyTypeCodeList.Codes._01, result.DutyPenaltyType);
			AssertEquals(TaxPenaltyTypeCodeList.Codes._03, result.DomesticTaxPenaltyType);

			amendmentSessionalData.DutyPenaltyCause = TaxPenaltyTypeCodeList.Codes._01;
			amendmentSessionalData.TaxPenaltyCause = TaxPenaltyTypeCodeList.Codes._03;
			result = new Import5FECreator().Create(entry, sendingObject1);
			AssertEquals(TaxPenaltyTypeCodeList.Codes._01, result.DutyPenaltyType);
			AssertEquals(TaxPenaltyTypeCodeList.Codes._03, result.DomesticTaxPenaltyType);

			amendmentSessionalData.DutyPenaltyCause = TaxPenaltyTypeCodeList.Codes._02;
			amendmentSessionalData.TaxPenaltyCause = TaxPenaltyTypeCodeList.Codes._04;
			result = new Import5FECreator().Create(entry, sendingObject1);
			AssertEquals(TaxPenaltyTypeCodeList.Codes._02, result.DutyPenaltyType);
			AssertEquals(TaxPenaltyTypeCodeList.Codes._04, result.DomesticTaxPenaltyType);

			amendmentSessionalData.DutyPenaltyCause = TaxPenaltyTypeCodeList.Codes._05;
			amendmentSessionalData.TaxPenaltyCause = TaxPenaltyTypeCodeList.Codes._05;
			result = new Import5FECreator().Create(entry, sendingObject1);
			AssertEquals(TaxPenaltyTypeCodeList.Codes._05, result.DutyPenaltyType);
			AssertEquals(TaxPenaltyTypeCodeList.Codes._05, result.DomesticTaxPenaltyType);
		}

		protected override void SetUp()
		{
			base.SetUp();
			entry = new TestDataSetupHelper(Factory).GetEntry929FullData(PaymentMethodCodeList.Codes._00, ZBool.True);

			using var stream = KRXmlObjectSerializer.Serialize(new ImportEntryHeaderCreator().Create(entry));
			AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._929, stream);
			AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._929);

			var statement929 = Factory.New<CusStatementHeader>();
			statement929.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statement929.B2_PaymentAuthorizationDate = new ZDateTime(2025, 01, 14);
			statement929.B2_GC = entry.Declaration.JE_GC;
			statement929.B2_StatementNumber = "1234567890123456789";
			statement929.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;

			var statementLine = statement929.StatementLines.AddNew();
			statementLine.B3_AssociatedEntry = "9876543210987654321";
			statementLine.B3_EntryType = KRJobMessageTypeList.Codes.Import;
			statementLine.B3_EntryNum = entry.EntryNumber;
			Factory.Save();
		}
		CusEntryHeader entry;
	}
}
