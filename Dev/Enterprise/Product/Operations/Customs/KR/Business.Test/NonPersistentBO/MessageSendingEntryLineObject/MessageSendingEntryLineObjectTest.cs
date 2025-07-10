using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;
namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(MessageSendingEntryLineObject))]
	public class MessageSendingEntryLineObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new MessageSendingEntryLineObject(Factory);

		public void TestMessageSendingEntryLineObjectMapping()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "8429521022", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "PEN NIBS1");

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth);
			helper.CreateCusMapType("CNTRY", "BTH", "Country Code Mapping", false);
			helper.CreateCusMap("CNTRY", "KR", "R.KOREA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.KoreaSouth);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var supplier = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK1", "");
			var supplierCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.CertificateOfOriginExporterNumber, Number = "1234567890" },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(supplier, supplierCodes);
			Factory.Save();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = supplier.PK;

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_NetWeight = 1100m;
			invoiceLine1.JI_NetWeightUQ = Core.Constants.Weight.Grams;
			invoiceLine1.JI_Tariff = "8429521022";
			invoiceLine1.JI_Description = "모델 규격";
			invoiceLine1.JI_PrimaryPreference = "C2";
			invoiceLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.KoreaSouth;
			invoiceLine1.JI_RN_NKSecondCommercialInvoiceCountry = Core.Constants.CountryCodes.Comoros;
			invoiceLine1.JI_COOSupportingDocType = "4";
			invoiceLine1.CertificateOfOriginNo = "CERNO";
			invoiceLine1.CertificateOfOriginIssueDate = new ZDateTime(2024, 06, 18);
			invoiceLine1.CertificateOfOriginIssueStatus = CertificateOfOriginIssuedCodeList.Codes.Y;
			invoiceLine1.CertificateOfOriginIssuingCountry = Core.Constants.CountryCodes.Afghanistan;
			invoiceLine1.CertificateOfOriginAgencyName = "Agency Name";
			invoiceLine1.CertificateOfOriginData.CSI_Quantity2 = 100m;
			invoiceLine1.CertificateOfOriginData.CSI_UnitOfQuantity2 = "KG";
			invoiceLine1.COOSplitOrder = 3;
			invoiceLine1.JI_SerialNumber = "111111111";
			invoiceLine1.JI_RN_NKReExportDestinationCountry = "KR";

			var coo1 = invoiceLine1.CertificateOfOriginData;
			coo1.CSI_IssuerType = CertifiticateOfOriginIssuedTypeList.Codes._3;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_NetWeight = 2m;
			invoiceLine2.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine2.CertificateOfOriginNo = "CERNO";
			invoiceLine2.CertificateOfOriginIssueDate = new ZDateTime(2024, 06, 18);
			invoiceLine2.CertificateOfOriginIssueStatus = CertificateOfOriginIssuedCodeList.Codes.Y;
			invoiceLine2.CertificateOfOriginIssuingCountry = Core.Constants.CountryCodes.Afghanistan;
			invoiceLine2.CertificateOfOriginAgencyName = "Agency Name";
			invoiceLine2.CertificateOfOriginData.CSI_Quantity2 = 20000m;
			invoiceLine2.CertificateOfOriginData.CSI_UnitOfQuantity2 = "G";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			entryLine.CL_LineNumber = 1;
			entryLine.CL_AdValoremTariff = "8429521022";
			entryLine.CL_ValueForVAT = 200000m;
			entryLine.CL_FTASequenceNumber = 2;
			var fee = entryLine.Fees.AddNew();
			fee.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.VAT;
			fee.CF_ChargeAmount = 10000m;
			var duty = entryLine.Fees.AddNew();
			duty.CF_Rate = 1000m;
			duty.CF_ChargeType = ChargeTypeList.Codes.Duty;

			var sendingObject = new MessageSendingEntryLineObject(ElectronicDocumentTypeList.Codes._5BA, entry);
			sendingObject.DecorateFromCusEntryLine(entryLine, ElectronicDocumentTypeList.Codes._5BA);
			AssertEquals(1, sendingObject.EntryLineNo);
			AssertEquals("PEN NIBS1", sendingObject.HSDescription);
			AssertEquals(3.1m, sendingObject.NetWeightInKG);
			AssertEquals(200000m, sendingObject.ValueForVAT);
			AssertEquals(10000m, sendingObject.VAT);
			AssertEquals("8429.52-1022", sendingObject.FormattedHSCode);
			AssertEquals("모델 규격", sendingObject.ModelName);
			AssertEquals("C2", sendingObject.Preference);
			AssertEquals(2, sendingObject.FTASequenceNumber);
			AssertEquals(3, sendingObject.SplitOrder);
			AssertEquals("CERNO", sendingObject.COONo);
			AssertEquals(new ZDateTime(2024, 06, 18), sendingObject.COOIssuedDate);
			AssertEquals(Core.Constants.CountryCodes.KoreaSouth, sendingObject.CountryOfOrigin);
			AssertEquals(1000m, sendingObject.TariffRate);
			AssertEquals(CertificateOfOriginIssuedCodeList.Codes.Y, sendingObject.COOProductType);
			AssertEquals(Constants.YesNo.Yes, sendingObject.ThirdCountryAdditionalInvoiceIssued);
			AssertEquals(Core.Constants.CountryCodes.Comoros, sendingObject.ThirdCountry);
			AssertEquals("1234567890", sendingObject.COOExporterNumber);
			AssertEquals(Core.Constants.CountryCodes.Afghanistan, sendingObject.AssociatedCOOIssuingCountryCode);
			AssertEquals("9", sendingObject.COOIssuingAgencyType);
			AssertEquals("Agency Name", sendingObject.COOAgencyName);
			AssertEquals("4", sendingObject.COOSupportingDocType);
			AssertEquals("3", sendingObject.COOIssuerType);
			AssertEquals(120m, sendingObject.TotalNetWeight);
			AssertEquals("111111111", sendingObject.SerialNumber);
			sendingObject.DecorateFromCusEntryLine(entryLine, ElectronicDocumentTypeList.Codes._5SC);
			AssertEquals("8429.52", sendingObject.FormattedHSCode);
			AssertEquals(3100m, sendingObject.NetWeightInGrams);
			AssertEquals("R.KOREA", sendingObject.ReExportDestinationDescription);
		}

		public void TestMessageSending5FNEntryLineObjectMapping()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "4203400000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "PEN NIBS1");

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth);
			helper.CreateCusMapType("CNTRY", "BTH", "Country Code Mapping", false);
			helper.CreateCusMap("CNTRY", "FR", "R.KOREA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.KoreaSouth);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;

			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5FN);
			entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			entryNum.CE_EntryLineReference = "1";
			entryNum.CE_IssueDate = new ZDateTime(2024, 02, 26);

			var message = entry.Messages.AddNew();
			message.EM_MessageType = ElectronicDocumentTypeList.Codes._5FN;
			message.EM_SystemCreateTimeUtc = new ZDateTime(2024, 02, 26, 02, 00, 00);
			message.EM_ApplicationReference = "1";
			var fileReader = new TestFileReader(typeof(MessageSendingEntryLineObjectTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5FN_D4.xml");
			message.EM_MessageText = messageText;
			Factory.Save();

			var messageObject = new MessageSendingEntryLineObject(ElectronicDocumentTypeList.Codes._5FN, entry);
			messageObject.DecorateFrom5FNCusEntryLine(entryNum, message);

			AssertEquals(1, messageObject.EntryLineNo);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalAccepted, messageObject.MessageStatus);
			AssertEquals(CustomsMessageStatusTypeList.Descriptions.OriginalAccepted, messageObject.MessageStatusDesc);
			AssertEquals(new ZDateTime(2024, 02, 26), messageObject.AcceptedDate);
			AssertEquals("4203400000", messageObject.HSCode);
			AssertEquals("PEN NIBS1", messageObject.HSDescription);
			AssertEquals("T", messageObject.DutyReduction);
			AssertEquals("N", messageObject.PostClearanceYN);
			AssertEquals("4203.40-0000", messageObject.FormattedHSCode);
			AssertEquals("서울시 강남구 도산대로 458 (청담동,리츠타워 701호,801호) 리츠타워 701호,801호)", messageObject.GoodsLocationAddressLine);
			AssertEquals("02-513-3220", messageObject.GoodsLocationTelNo);
			AssertEquals("060-62", messageObject.FormattedGoodsLocationPostcode);
			AssertEquals("모델명", messageObject.ModelName);
			AssertEquals("123123112", messageObject.SerialNumber);
			AssertEquals("주문수집 후 재수출 예정", messageObject.UseCodeDescription);
			AssertEquals("020", messageObject.CustomsOffice);
			AssertEquals("040", messageObject.ReExportCustomsOffice);
			AssertEquals("FR", messageObject.DestinationCountry);
			AssertEquals("01", messageObject.GroupNumber);
			AssertEquals("45", messageObject.SequenceNumber);
			AssertEquals("12", messageObject.ItemNumber);
			AssertEquals(new ZDateTime(2020, 10, 31), messageObject.EstimateDate);
			AssertEquals("R.KOREA", messageObject.ReExportDestinationDescription);
		}

		public void TestDecorateFromImportEntryHeader()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0208100000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "HSDescription1");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "4203400000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "HSDescription2");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.CL_DutyReductionAmount = 1000m;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0208100000";

			var consignee = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "KRTEST1", "CompanyName");
			var consigneeAddress = consignee.MainAddress;

			invoiceLine.JI_OA_ConsigneeAddress = consigneeAddress.PK;
			invoiceLine.JI_SerialNumber = "010101010";
			invoiceLine.JI_SpecificUseCodeDescription = "주문수집 후 재수출 예정";
			invoiceLine.JI_JurisdictionalCusOffice = "010";
			invoiceLine.JI_ScheduledReExportCustomsOffice = "040";
			invoiceLine.JI_RN_NKReExportDestinationCountry = "CH";
			invoiceLine.DutyReductionGroupNumber = "01";
			invoiceLine.DutyReductionSeqNumber = "001";
			invoiceLine.DutyReductionItemNumber = "01";
			invoiceLine.JI_ScheduledReExportDate = new ZDateTime(2025, 1, 30);
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_InstallmentCode = "A";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_CustomsUnitQty = "U";
			invoiceLine.JI_Description = "Item Description";

			var entryNumber = entry.EntryNumbers.AddNew();
			entryNumber.CE_EntryType = ElectronicDocumentTypeList.Codes._5FN;
			entryNumber.CE_EntryLineReference = "1";
			entryNumber.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.AmendmentSent;
			entryNumber.CE_IssueDate = new ZDateTime(2025, 1, 1);

			var message = entry.Messages.AddNew();
			message.EM_MessageType = ElectronicDocumentTypeList.Codes._5FN;
			message.EM_ApplicationReference = "1";
			var fileReader = new TestFileReader(typeof(MessageSendingEntryLineObjectTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5FN_D1.xml");
			message.EM_MessageText = messageText;
			Factory.Save();

			CreateImportEntrySnapshot(entry);
			var snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._929, EntrySnapshotStatus.Lodged);
			var wrapper = new EntrySnapshotWrapper(snapshot, Factory);
			AssertEquals(1, wrapper.ImportEntryWrapper.EntryLineObjects5FN.Count);
			var messageObject = wrapper.ImportEntryWrapper.EntryLineObjects5FN.Cast<MessageSendingEntryLineObject>().First();

			invoiceLine.JI_Tariff = "0208100001";
			invoiceLine.JI_InstallmentCode = "B";
			invoiceLine.JI_CustomsQuantity = 200m;
			invoiceLine.JI_CustomsUnitQty = "L";
			invoiceLine.JI_Description = "Item Description2";
			entryLine.CL_DutyReductionAmount = 2000m;

			AssertEquals(1, messageObject.EntryLineNo);
			AssertEquals("A", messageObject.DutyReductionOrInstallmentCode);
			AssertEquals("HSDescription2", messageObject.HSDescription);
			AssertEquals("Item Description", messageObject.ItemDescription);
			AssertEquals(100m, messageObject.CustomsQuantity);
			AssertEquals("U", messageObject.CustomsUnitQty);
			AssertEquals(0m, messageObject.CustomsValue);
			AssertEquals(1000m, messageObject.DutyReductionAmount);

			void CreateImportEntrySnapshot(CusEntryHeader entry)
			{
				var header = new ImportEntryHeaderCreator().Create(entry);
				using (var stream = KRXmlObjectSerializer.Serialize(header))
				{
					AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._929, stream);
					Factory.Save();
				}
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._929);
				Factory.Save();
			}
		}

		public void TestShouldPropertiesBeReadOnlyOfMessageSendingEntryLineObject()
		{
			var messageObject = new MessageSendingEntryLineObject(Factory);
			AssertEquals(true, messageObject.EntryLineNoInfo.ReadOnly);
			AssertEquals(true, messageObject.HSCodeInfo.ReadOnly);
			AssertEquals(true, messageObject.FormattedHSCodeInfo.ReadOnly);
			AssertEquals(true, messageObject.HSDescriptionInfo.ReadOnly);
			AssertEquals(true, messageObject.NetWeightInKGInfo.ReadOnly);
			AssertEquals(true, messageObject.NetWeightUnitInfo.ReadOnly);
			AssertEquals(true, messageObject.ValueForVATInfo.ReadOnly);
			AssertEquals(true, messageObject.VATInfo.ReadOnly);
			AssertEquals(false, messageObject.IsGoldOrItsProductInfo.ReadOnly);
			AssertEquals(true, messageObject.ModelNameInfo.ReadOnly);
			AssertEquals(true, messageObject.PreferenceInfo.ReadOnly);
			AssertEquals(false, messageObject.ShouldSendInfo.ReadOnly);
			AssertEquals(true, messageObject.MessageStatusInfo.ReadOnly);
			AssertEquals(true, messageObject.MessageStatusDescInfo.ReadOnly);
			AssertEquals(true, messageObject.AcceptedDateInfo.ReadOnly);
			AssertEquals(true, messageObject.DutyReductionInfo.ReadOnly);
			AssertEquals(true, messageObject.PostClearanceYNInfo.ReadOnly);
			AssertEquals(true, messageObject.SerialNumberInfo.ReadOnly);
			AssertEquals(true, messageObject.DutyReductionCodeInfo.ReadOnly);
			AssertEquals(true, messageObject.InstalmentCodeInfo.ReadOnly);
			AssertEquals(true, messageObject.SpecificUseInfo.ReadOnly);
			AssertEquals(true, messageObject.RemarkInfo.ReadOnly);
			AssertEquals(true, messageObject.UseCodeDescriptionInfo.ReadOnly);
			AssertEquals(true, messageObject.ProductTypeInfo.ReadOnly);
			AssertEquals(true, messageObject.CustomsOfficeInfo.ReadOnly);
			AssertEquals(true, messageObject.GoodsLocationInfo.ReadOnly);
			AssertEquals(true, messageObject.GroupNumberInfo.ReadOnly);
			AssertEquals(true, messageObject.SequenceNumberInfo.ReadOnly);
			AssertEquals(true, messageObject.ItemNumberInfo.ReadOnly);
			AssertEquals(true, messageObject.ReExportCustomsOfficeInfo.ReadOnly);
			AssertEquals(true, messageObject.DestinationCountryInfo.ReadOnly);
			AssertEquals(true, messageObject.EstimateDateInfo.ReadOnly);
			AssertEquals(true, messageObject.FTASequenceNumberInfo.ReadOnly);
			AssertEquals(true, messageObject.TotalNetWeightInfo.ReadOnly);
			AssertEquals(true, messageObject.SplitOrderInfo.ReadOnly);
			AssertEquals(true, messageObject.COONoInfo.ReadOnly);
			AssertEquals(true, messageObject.COOIssuedDateInfo.ReadOnly);
			AssertEquals(true, messageObject.CountryOfOriginInfo.ReadOnly);
			AssertEquals(true, messageObject.TariffRateInfo.ReadOnly);
			AssertEquals(true, messageObject.COOProductTypeInfo.ReadOnly);
			AssertEquals(true, messageObject.ThirdCountryAdditionalInvoiceIssuedInfo.ReadOnly);
			AssertEquals(true, messageObject.ThirdCountryInfo.ReadOnly);
			AssertEquals(true, messageObject.COOExporterNumberInfo.ReadOnly);
			AssertEquals(true, messageObject.AssociatedCOOIssuingCountryCodeInfo.ReadOnly);
			AssertEquals(true, messageObject.COOIssuingAgencyTypeInfo.ReadOnly);
			AssertEquals(true, messageObject.COOAgencyNameInfo.ReadOnly);
			AssertEquals(true, messageObject.COOSupportingDocTypeInfo.ReadOnly);
			AssertEquals(true, messageObject.COOIssuerTypeInfo.ReadOnly);
			AssertEquals(true, messageObject.CustomsValueInfo.ReadOnly);
			AssertEquals(true, messageObject.CustomsQuantityInfo.ReadOnly);
			AssertEquals(true, messageObject.CustomsUnitQtyInfo.ReadOnly);
			AssertEquals(true, messageObject.DutyReductionAmountInfo.ReadOnly);
		}

		public void TestValidateEachEntryLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = -1;
			invoice1.JZ_PaymentTerms = "12";
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.ImmediateDeliveries.AddNew().CY_Data = "1";
			invoiceLine1.ImmediateDeliveries.AddNew().CY_Data = "1";
			var invoiceLine2 = invoice1.InvoiceLines.AddNew();
			var invoiceLine3 = invoice2.InvoiceLines.AddNew();

			var bill = declaration.Bills.AddNew();
			invoice1.JZ_CU_RelatedHouseBill = bill.PK;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;

			var entryLine1 = entry.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			invoiceLine1.JI_CL = entryLine1.PK;

			var entryLine2 = entry.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine3.JI_CL = entryLine2.PK;

			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5FN);
			entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			entryNum.CE_EntryLineReference = "1";
			entryNum.CE_IssueDate = new ZDateTime(2024, 02, 26);

			declaration.Validation.ValidateAll();
			entry.Validation.ValidateAll();
			instruction.Validation.ValidateAll();
			entryLine1.Validation.ValidateAll();
			entryLine2.Validation.ValidateAll();
			invoice1.Validation.ValidateAll();
			invoice2.Validation.ValidateAll();
			invoiceLine1.Validation.ValidateAll();
			invoiceLine2.Validation.ValidateAll();
			invoiceLine3.Validation.ValidateAll();
			invoiceLine1.GAApprovalDataCollection.AddNew().Validation.ValidateAll();
			bill.Validation.ValidateAll();

			var messageObject = new MessageSendingEntryLineObjectForTest(ElectronicDocumentTypeList.Codes._5FN, entry);
			AssertEquals(ZGuid.Empty, messageObject.entryLinePKExposed);
			AssertNull(messageObject.EntryLineNotificationCollectorExposed);
			messageObject.DecorateFromCusEntryLine(entryLine1, ElectronicDocumentTypeList.Codes._5FN);
			AssertNotEquals(ZGuid.Empty, messageObject.entryLinePKExposed);
			AssertNotNull(messageObject.EntryLineNotificationCollectorExposed);
			messageObject.ShouldSend = true;
			Assert("CusEntryLine message error", messageObject.MessageSendingValidation.MessageErrors.ContainsNotificationContaining("Customs Value (KRW) cannot be zero"));
			Assert("JobDeclaration message error", messageObject.MessageSendingValidation.MessageErrors.ContainsNotificationContaining("Container Pack: You have not entered a Container Pack"));
			Assert("CusEntryHeader message error", messageObject.MessageSendingValidation.MessageErrors.ContainsNotificationContaining("Freight (KRW) cannot be zero"));
			Assert("JobComInvoiceHeader message error", messageObject.MessageSendingValidation.MessageErrors.ContainsNotificationContaining("Please enter a 'Total Gross Weight' greater than 0"));
			Assert("JobComInvoiceLine message error", messageObject.MessageSendingValidation.MessageErrors.ContainsNotificationContaining("You have not entered a C/O Determination Rule"));
			Assert("AddInfoJobComInvoiceLine message error", messageObject.MessageSendingValidation.MessageErrors.ContainsNotificationContaining("Brand Code: You have not entered a Brand Code"));
			Assert("CusEntryInstruction message error", messageObject.MessageSendingValidation.MessageErrors.ContainsNotificationContaining("You have not entered an Agreed Rate"));
			Assert("ImmediateDeliveries message error", messageObject.MessageSendingValidation.MessageErrors.ContainsNotificationContaining("You cannot enter an 'Immediate Delivery Number' that already exists"));
			Assert("Bill message error", messageObject.MessageSendingValidation.MessageErrors.ContainsNotificationContaining("You have not entered a Bill Split YN"));
			Assert("GAApproval message error", messageObject.MessageSendingValidation.MessageErrors.ContainsNotificationContaining("You have not entered an Approval Number"));
			entryLine1.CL_CustomsValue = 1000m;
			invoiceLine1.CriteriaForDeterminingCountryOfOrigin = "A";
			invoiceLine2.CriteriaForDeterminingCountryOfOrigin = "A";
			invoiceLine2.JI_Weight = 100m;
			Factory.Save();
			messageObject = new MessageSendingEntryLineObjectForTest(ElectronicDocumentTypeList.Codes._5FN, entry);
			messageObject.ShouldSend = true;

			Assert("CusEntryLine1 doesn't have message error", !messageObject.MessageSendingValidation.MessageErrors.ContainsNotificationContaining("Customs Value (KRW) cannot be zero"));
			Assert("JobComInvoiceLine1 doesn't have message error", !messageObject.MessageSendingValidation.MessageErrors.ContainsNotificationContaining("You have not entered a C/O Determination Rule"));
			Assert("JobComInvoiceHeader1 doesn't have message error", !messageObject.MessageSendingValidation.MessageErrors.ContainsNotificationContaining("Please enter a 'Total Gross Weight' greater than 0"));
			Assert("JobComInvoiceHeader1 doesn't have message error", !messageObject.MessageSendingValidation.MessageErrors.ContainsNotificationContaining("You have not entered a Payment Method."));

			messageObject = new MessageSendingEntryLineObjectForTest(ElectronicDocumentTypeList.Codes._5FN, entry);
			messageObject.DecorateFromCusEntryLine(entryLine2, ElectronicDocumentTypeList.Codes._5FN);
			messageObject.ShouldSend = true;
			Assert("CusEntryLine2 has message error", messageObject.MessageSendingValidation.MessageErrors.ContainsNotificationContaining("Customs Value (KRW) cannot be zero"));
			Assert("JobComInvoiceLine3 has message error", messageObject.MessageSendingValidation.MessageErrors.ContainsNotificationContaining("You have not entered a C/O Determination Rule"));
			Assert("JobComInvoiceHeader1 has message error", messageObject.MessageSendingValidation.MessageErrors.ContainsNotificationContaining("Inv. Total Amount cannot be negative"));
			Assert("JobComInvoiceHeader2 has message error", messageObject.MessageSendingValidation.MessageErrors.ContainsNotificationContaining("You have not entered a Payment Method."));
		}

		const string TestFilesPath = "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";
	}

	class MessageSendingEntryLineObjectForTest : MessageSendingEntryLineObject
	{
		public MessageSendingEntryLineObjectForTest(BusinessObjectFactory factory) : base(factory) { }

		public MessageSendingEntryLineObjectForTest(string messageType, CusEntryHeader entry) : base(messageType, entry) { }

		public JobDeclarationEntryLineSelectiveNotificationCollector EntryLineNotificationCollectorExposed => EntryLineNotificationCollector;
		public ZGuid entryLinePKExposed => entryLinePK;
	}
}
