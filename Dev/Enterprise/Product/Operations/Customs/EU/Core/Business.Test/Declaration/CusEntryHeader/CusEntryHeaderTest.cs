using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public abstract class CusEntryHeaderTest<T> : Customs.Business.Testing.CusEntryHeaderTest
		where T : CusEntryHeader
	{
		public void TestCRN()
		{
			AssertEntryNumber(CusEntryNumberTypes.EU.CustomsRegistrationNumber, nameof(TemporaryStorageHeader.CRN));
		}

		protected void AssertEntryNumber(ZString entryType, ZString propertyName)
		{
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			var entryNumber = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNumber.CE_ParentID = entryHeader.PK;
			entryNumber.CE_ParentTable = entryHeader.TableName;
			entryNumber.CE_EntryType = entryType;
			entryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.Country.Code;
			entryNumber.CE_EntryNum = "970628";
			Factory.Save();

			entryHeader.Reload();
			AssertEquals($"{entryType} number can be loaded correctly", "970628", entryHeader.GetType().GetProperty(propertyName).GetValue(entryHeader));
		}

		public void TestTotalCustomsUQ()
		{
			var entry = Factory.New<CusEntryHeader>();
			AssertEquals("Default value for UQ", "KG", entry.TotalCustomsUQ);
		}

		public void TestTotalNetWeightUQ()
		{
			var entry = Factory.New<CusEntryHeader>();
			AssertEquals("Default value for UQ", "KG", entry.TotalNetWeightUQ);
		}

		public void TestTotalGrossWeightUQ()
		{
			var entry = Factory.New<CusEntryHeader>();
			AssertEquals("Default value for UQ", "KG", entry.TotalGrossWeightUQ);
		}

		public virtual void TestGetTaxBoxSupporterList()
		{
			var entry = Factory.New<CusEntryHeader>();
			AssertEquals("Default as null", null, entry.GetTaxBoxSupporterList());
		}

		[ExpectNoExceptions]
		public virtual void TestAllAddInfoColumnsAreInModelView()
		{
			var header = Factory.New<T>();

			ModelViewTestHelper.AssertAllAddInfoColumnsAreInModelView(header, "EUCusEntryHeader", fieldName => !fieldName.StartsWith(CusEntryHeaderSchema.Constants.Prefix) && !fieldName.StartsWith(CusEUEntryHeaderSchema.Constants.Prefix));
		}

		public void TestSumLineFee_ConfirmedFeeUsage()
		{
			var declaration = (JobDeclaration)GetNewDeclaration();
			declaration.JE_MergeBy = "TRF";
			declaration.JE_ApplicationCode = "BLT";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var entryLineFee1 = entryLine.Fees.AddOrUpdate("A00", 20m);
			var entryLineFee2 = entryLine.Fees.AddOrUpdate("B00", 30m);

			AssertEquals("No confirmed fees present, use fees", 30m, entry.VAT);

			var confirmedEntryLineFee = entryLine.ConfirmedFees.AddOrUpdate("B00", 60m);
			AssertEquals("Confirmed fees present, use confirmed fees", 60m, entry.VAT);
		}

		public void TestSumLineConfirmedFees()
		{
			var declaration = (JobDeclaration)GetNewDeclaration();
			declaration.JE_MergeBy = "TRF";
			declaration.JE_ApplicationCode = "BLT";
			var inv = declaration.Invoices.AddNew();

			var invLine1 = inv.JobComInvoiceLines.AddNew();
			invLine1.JI_Tariff = "2203001010";
			invLine1.JI_CustomsQuantity = 100.00m;
			var invLine2 = inv.JobComInvoiceLines.AddNew();
			invLine2.JI_Tariff = "2203001010";
			invLine2.JI_CustomsQuantity = 200.00m;

			DoMerge(declaration);

			AssertEquals("Total Customs quantity is sum of single invoice lines", 300.00m, ((CusEntryHeader)declaration.ActiveEntryHeaders[0]).TotalCustomsQuantity);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var entryLineFee1 = entryLine.Fees.AddOrUpdate("A00", 20m);
			var confirmedEntryLineFee1 = entryLine.ConfirmedFees.AddOrUpdate("B00", 30m);
			var confirmedEntryLineFee2 = entryLine.ConfirmedFees.AddOrUpdate("B01", 40m);
			AssertEquals(70m, entry.SumLineConfirmedFees(x => true));
		}

		public virtual void TestShouldCompletelyReassignNumbers()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			AssertEquals(true, entryHeader.ShouldCompletelyReassignNumbers);
			entryHeader.EntryNumber = "EN001";
			AssertEquals(false, entryHeader.ShouldCompletelyReassignNumbers);
		}

		public void TestCustomsDocStatus()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var header = declaration.CustomsEntryHeaders.AddNew();
			header.CH_CEI_Instruction = instruction.PK;

			AssertEquals("Should be empty with no RequestedDocuments", ZString.Empty, declaration.CustomsDocStatus);

			var requestedDocument1 = instruction.RequestedDocuments.AddNew();
			var requestedDocument2 = instruction.RequestedDocuments.AddNew();

			requestedDocument1.CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;
			requestedDocument2.CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;
			AssertEquals(RequestedDocumentStatusList.Codes.RequestOpened, header.CustomsDocStatus);
			AssertEquals(RequestedDocumentStatusList.Descriptions.RequestOpened, header.CustomsDocStatusDesc);

			requestedDocument1.CSI_Status = RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived;
			requestedDocument2.CSI_Status = RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived;
			AssertEquals(RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived, header.CustomsDocStatus);
			AssertEquals(RequestedDocumentStatusList.Descriptions.DocumentsConfirmedReceived, header.CustomsDocStatusDesc);

			requestedDocument1.CSI_Status = RequestedDocumentStatusList.Codes.PhysicallyPresentDocument;
			requestedDocument2.CSI_Status = RequestedDocumentStatusList.Codes.PhysicallyPresentDocument;
			AssertEquals(RequestedDocumentStatusList.Codes.PhysicallyPresentDocument, header.CustomsDocStatus);
			AssertEquals(RequestedDocumentStatusList.Descriptions.PhysicallyPresentDocument, header.CustomsDocStatusDesc);

			requestedDocument1.CSI_Status = RequestedDocumentStatusList.Codes.RequestCancelled;
			requestedDocument2.CSI_Status = RequestedDocumentStatusList.Codes.RequestCancelled;
			AssertEquals(RequestedDocumentStatusList.Codes.RequestCancelled, header.CustomsDocStatus);
			AssertEquals(RequestedDocumentStatusList.Descriptions.RequestCancelled, header.CustomsDocStatusDesc);

			var requestedDocument3 = instruction.RequestedDocuments.AddNew();
			requestedDocument1.CSI_Status = RequestedDocumentStatusList.Codes.RequestCancelled;
			requestedDocument2.CSI_Status = RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived;
			requestedDocument3.CSI_Status = RequestedDocumentStatusList.Codes.RequestCancelled;
			AssertEquals(RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived, header.CustomsDocStatus);
			AssertEquals(RequestedDocumentStatusList.Descriptions.DocumentsConfirmedReceived, header.CustomsDocStatusDesc);

			requestedDocument1.CSI_Status = RequestedDocumentStatusList.Codes.RequestCancelled;
			requestedDocument2.CSI_Status = RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived;
			requestedDocument3.CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;
			AssertEquals(RequestedDocumentStatusList.Codes.RequestOpened, header.CustomsDocStatus);
			AssertEquals(RequestedDocumentStatusList.Descriptions.RequestOpened, header.CustomsDocStatusDesc);
		}

		public void TestTotalGrossWeight()
		{
			var declaration = (JobDeclaration)GetNewDeclaration();
			declaration.JE_MergeBy = "TRF";
			declaration.JE_ApplicationCode = "BLT";
			var inv = declaration.Invoices.AddNew();

			var invLine1 = inv.JobComInvoiceLines.AddNew();
			invLine1.JI_Tariff = "2203001010";
			invLine1.JI_Weight = 100.00m;
			var invLine2 = inv.JobComInvoiceLines.AddNew();
			invLine2.JI_Tariff = "2203001010";
			invLine2.JI_Weight = 200.00m;

			DoMerge(declaration);

			AssertEquals("Total gross weight is sum of single invoice lines", 300.00m, ((CusEntryHeader)declaration.ActiveEntryHeaders[0]).TotalGrossWeight);
		}

		public void TestEntryInstructionWarehouseCode()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ONEADDRESS";
			var address1 = org1.MainAddress;
			address1.OA_Address1 = "Address1";

			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_JE = declaration.PK;

			var entryHeader = (CusEntryHeader)(declaration.ActiveEntryHeaders.AddNew());
			AssertNoExceptionThrown("No exception if instruction is empty", (() => _ = entryHeader.EntryInstructionWarehouseCode));
			AssertEquals("When no instruction is available, EntryInstructionWarehouseCode is empty", ZString.Empty, entryHeader.EntryInstructionWarehouseCode);

			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			entryInstruction.CEI_OA_Warehouse2_ZAddress.OrgPK = org1.PK;
			AssertEquals("With available instruction, EntryInstructionWarehouseCode must be ONEADDRESS", "ONEADDRESS", entryHeader.EntryInstructionWarehouseCode);
		}

		public void TestInvoiceCurrency()
		{
			var europeanCountry = RefCountry.LoadFromCountryCode(Factory, GlbCompany.CurrentCompany.Country.Code);
			var europeanCurrency = RefCurrency.LoadFromCurrencyCode(Factory, europeanCountry.RN_RX_NKLocalCurrency);
			var aud = RefCurrency.LoadFromCurrencyCode(Factory, Enterprise.Core.Constants.CurrencyCodes.Australia);
			var usd = RefCurrency.LoadFromCurrencyCode(Factory, Enterprise.Core.Constants.CurrencyCodes.UnitedStates);
			var declaration = (JobDeclaration)GetNewDeclaration();
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;

			invoiceHeader1.JZ_InvoiceAmount = 100.00m;
			invoiceHeader1.JZ_RX_NKInvoice_Currency = usd.RX_Code;
			invoiceLine1.JI_LinePrice = 100.00m;

			AssertEquals("", usd.Code, entryHeader.InvoiceCurrency);

			invoiceHeader1.JZ_RX_NKInvoice_Currency = aud.RX_Code;
			AssertEquals("", aud.Code, entryHeader.InvoiceCurrency);
		}

		public void TestTotalCustomsQuantityWithNoinvoiceLines()
		{
			var declaration = (JobDeclaration)GetNewDeclaration();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			AssertNoExceptionThrown("No exception if invoice lines are empty", (() => _ = entryHeader.TotalCustomsQuantity));

			AssertEquals("When there are no invoice lines, Total Customs Quantity is zero", ZDecimal.Zero, entryHeader.TotalCustomsQuantity);
		}

		public void TestMergedLinesCount()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MergedLines.AddNew();
			entryHeader.MergedLines.AddNew();

			AssertEquals("Entrylines as sum of available entry lines", 2, entryHeader.MergedLinesCount);
		}

		public virtual void TestOfficeOfExit()
		{
			var declaration = (JobDeclaration)GetNewDeclaration();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsOffices.RemoveAndDeleteAll();
			declaration.JE_CustomsOffice = "LV001000";
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, "LV002000");
			AssertEquals("LV002000", declaration.OfficeOfExit);
			AssertEquals("LV002000", entry.OfficeOfExit);
		}

		public virtual void TestOfficeOfEntry()
		{
			var declaration = (JobDeclaration)GetNewDeclaration();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsOffices.RemoveAndDeleteAll();
			declaration.JE_CustomsOffice = "LV001000";
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, "LV002000");
			AssertEquals("", declaration.OfficeOfEntry);
			AssertEquals("", entry.OfficeOfEntry);
		}

		public void TestReferenceNumber()
		{
			var declaration = (JobDeclaration)GetNewDeclaration();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "123456";
			entry.CH_BGMReference = "654321";
			AssertEquals("123456", entry.ReferenceNumber);
			entry.EntryNumber = "";
			AssertEquals("654321", entry.ReferenceNumber);
		}

		public virtual void TestAdditionalInfos()
		{
			CusEntryHeaderTestHelper.CreateAdditionalInfos(Factory);

			var dec = (JobDeclaration)GetNewDeclaration();
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			dec.JE_MergeBy = "TRF";
			var inv = dec.Invoices.AddNew();
			var inv2 = dec.Invoices.AddNew();
			var invLine1 = inv.JobComInvoiceLines.AddNew();
			invLine1.JI_Tariff = "2203001010";
			var invLine2 = inv2.JobComInvoiceLines.AddNew();
			invLine2.JI_Tariff = "2203001010";

			var addInfo1 = inv.AdditionalInfos.AddNew();
			addInfo1.CSI_Code = "9002";
			addInfo1.CSI_Description = "9002 Desc";

			var addInfo2 = inv2.AdditionalInfos.AddNew();
			addInfo2.CSI_Code = "9002";
			addInfo2.CSI_Description = "9002 Desc";

			var addInfo3 = inv.AdditionalInfos.AddNew();
			addInfo3.CSI_Code = "9003";
			addInfo3.CSI_Description = "9003 Desc";

			DoMerge(dec);
			//dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals(1, dec.ActiveEntryHeaders.Count);
			AssertEquals(1, ((T)dec.ActiveEntryHeaders[0]).AdditionalInfos.Count());

			addInfo2.CSI_Code = "9004";
			addInfo2.CSI_Description = "9004 Desc";

			DoMerge(dec);
			//dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(2, dec.ActiveEntryHeaders.Count);
			AssertEquals(1, ((T)dec.ActiveEntryHeaders[0]).AdditionalInfos.Count());
			AssertEquals(1, ((T)dec.ActiveEntryHeaders[1]).AdditionalInfos.Count());
		}

		public override void TestTotalDutyAmount()
		{
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Universal.Constants.RateTypes.Duty, FeeTypeList.Codes.A00);

			var entry = SetupEntryForFeesTest(Factory);
			entry.CH_JE = Factory.New<JobDeclaration>().PK;

			AssertEquals("Total Duty Amount", 500m, entry.TotalDutyAmount);
			AssertEquals("Duty Amount", 500m, entry.Duty);
			AssertEquals("VAT", 150m, entry.VAT);

			AssertEquals("B00", entry.TaxCode);
			AssertEquals("A00", entry.DutyCode);

			AssertEquals(300m, entry.DutyImmediate);
			AssertEquals(200m, entry.DutyDeferred);
			AssertEquals(100m, entry.VATImmediate);
			AssertEquals(50m, entry.VATDeferred);
			AssertEquals(35m, entry.AllOtherFeesDeferred);
			AssertEquals(30m, entry.AllOtherFeesImmediate);
			AssertEquals(65m, entry.AllOtherFees);
		}

		public void TestEntryInstruction()
		{
			var declaration = (JobDeclaration)GetNewDeclaration();
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_JE = declaration.PK;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = entryInstruction.PK;

			Assert(declaration.CustomsEntryInstructions.Contains(entry.EntryInstruction));
		}

		public virtual void TestRemergeDoesNotClobberCharges()
		{
			var dec = (JobDeclaration)GetNewDeclaration();
			if (!IntegratedCountryHelper.CountryHasBuiltInDeclaration(dec.CountryCode) || dec.IsDeclarationIntegrated)
			{
				Assert("For integrated countries, merge is turned off.", true);
				return;
			}
			var invHeader = dec.Invoices.AddNew();
			invHeader.InvoiceLines.AddNew();
			DoMerge(dec);
			var ceh = dec.CustomsEntryHeaders[0];
			ceh.Charges.AddNew("DAN", 69m);
			ceh.MergedLines[0].Fees.AddOrUpdate("JAM", 79m);
			ceh.MergedLines[0].Fees[0].CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Additional;

			DoMerge(dec);
			AssertEquals(69m, ceh.Charges[0].C1_ChargeAmount);
			AssertEquals("Fee Charge Amount after re-merge", 79m, ceh.MergedLines[0].Fees[0].CF_ChargeAmount);
		}

		public virtual void TestRepresentativeOrDeclarantEoriOfMainOffice()
		{
			var declaration = (JobDeclaration)GetNewDeclaration();
			var uk = Factory.Load<RefCountry>(Core.Constants.CountryGuids.UnitedKingdom);

			var declarant = Factory.New<OrgHeader>();
			declarant.SetCustomsCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, uk, "999999999888");
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("Eori number comes out verbatim", "GB999999999888", entryHeader.RepresentativeOrDeclarantEoriOfMainOffice);

			declarant.CustomsCodes.RemoveAll();
			declarant.SetCustomsCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Turn, uk, "999999999888");
			AssertEquals("Turn number comes out as 000", "GB999999999000", entryHeader.RepresentativeOrDeclarantEoriOfMainOffice);
		}

		[TestDate(2008, 6, 8)]
		public void TestTotalPrice()
		{
			var europeanCountry = RefCountry.LoadFromCountryCode(Factory, GlbCompany.CurrentCompany.Country.Code);
			var europeanCurrency = RefCurrency.LoadFromCurrencyCode(Factory, europeanCountry.RN_RX_NKLocalCurrency);
			var aud = RefCurrency.LoadFromCurrencyCode(Factory, Enterprise.Core.Constants.CurrencyCodes.Australia);
			var usd = RefCurrency.LoadFromCurrencyCode(Factory, Enterprise.Core.Constants.CurrencyCodes.UnitedStates);
			var declaration = (JobDeclaration)GetNewDeclaration();
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			var invoiceHeader2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine2 = invoiceHeader2.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;

			invoiceHeader1.JZ_InvoiceAmount = 100.00m;
			invoiceHeader1.JZ_RX_NKInvoice_Currency = usd.RX_Code;
			invoiceLine1.JI_LinePrice = 100.00m;

			invoiceHeader2.JZ_InvoiceAmount = 200.00m;
			invoiceHeader2.JZ_RX_NKInvoice_Currency = usd.RX_Code;
			invoiceLine2.JI_LinePrice = 200.00m;

			AssertEquals("entryHeader.TotalPrice", new Money(300.00m, usd), entryHeader.TotalPrice);

			invoiceHeader1.JZ_RX_NKInvoice_Currency = aud.RX_Code;
			AssertEquals("entryHeader.TotalPrice", new Money(192.90m, europeanCurrency), entryHeader.TotalPrice);

			invoiceHeader2.JZ_RX_NKInvoice_Currency = aud.RX_Code;
			AssertEquals("entryHeader.TotalPrice", new Money(300.00m, aud), entryHeader.TotalPrice);
		}

		public virtual void TestEntryStatusChangingToClearRecordsLog()
		{
			var declaration = (JobDeclaration)GetNewDeclaration();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals(true, entryHeader.ClearanceDate.IsEmpty);
			entryHeader.CH_EntryStatus = EntryStatusList.Codes.Clear;
			AssertEquals(true, entryHeader.ClearanceDate.IsEmpty);
			entryHeader.CH_EntryReleaseDate = ZDateTime.BrettsBirthday;
			AssertEquals(true, entryHeader.ClearanceDate.IsEmpty);
			Factory.Save();
			AssertEquals(ZDateTime.BrettsBirthday, entryHeader.ClearanceDate);
		}

		public virtual void TestBGMReferencesAreSetWhenSavedAndThenUCRIsChanged()
		{
			if (IgnoreTestsThatRequireEntryShouldSetUCRinBGMReferenceNumber)
			{
				var entry = ((JobDeclaration)GetNewDeclaration()).CustomsEntryHeaders.AddNew();
				AssertEquals(false, entry.ShouldSetUCRinBGMReferenceNumber);
			}
			else
			{
				var declaration = (JobDeclaration)GetNewDeclaration();
				var entry = declaration.CustomsEntryHeaders.AddNew();
				Factory.Save();
				AssertEquals(declaration.JE_UCR, entry.CH_BGMReference);
				var entry2 = declaration.CustomsEntryHeaders.AddNew();
				Factory.Save();
				AssertEquals(declaration.JE_UCR + "/1", entry2.CH_BGMReference);

				var cusEntryNumberFilter = new ZQuery(CusEntryNumSchema.CE_ParentID, entry.PK);
				cusEntryNumberFilter.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.UniqueConsignementReference);
				var cusEntryNumberFilter2 = new ZQuery(CusEntryNumSchema.CE_ParentID, entry2.PK);
				cusEntryNumberFilter2.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.UniqueConsignementReference);
				var cusEntryNum = Factory.LoadTop1<CusEntryNumber>(cusEntryNumberFilter);
				var cusEntryNum2 = Factory.LoadTop1<CusEntryNumber>(cusEntryNumberFilter2);
				AssertNotNull(cusEntryNum);
				AssertNotNull(cusEntryNum2);
				AssertEquals(CusEntryHeaderSchema.Constants.TableName, cusEntryNum.CE_ParentTable);
				AssertEquals(CusEntryHeaderSchema.Constants.TableName, cusEntryNum2.CE_ParentTable);

				declaration.JE_UCR = "New DUCR";
				Factory.Save();
				cusEntryNum = Factory.LoadTop1<CusEntryNumber>(cusEntryNumberFilter);
				cusEntryNum2 = Factory.LoadTop1<CusEntryNumber>(cusEntryNumberFilter2);
				AssertEquals("New DUCR", cusEntryNum.CE_EntryNum);
				AssertEquals("New DUCR/1", cusEntryNum2.CE_EntryNum);
				AssertEquals("DUCR propagates to entry", "New DUCR", entry.CH_BGMReference);
				AssertEquals("DUCR propagates to entry", "New DUCR/1", entry2.CH_BGMReference);
				AssertEquals("Customs Entry New DUCR", entry.HumanReadableName);
				AssertEquals("DUCR propagates to entry", "New DUCR", entry.DeclarationUCR);
				AssertEquals("DUCR propagates to entry", "", entry.DeclarationUCRPartSuffix);
				AssertEquals("DUCR propagates to entry", "New DUCR", entry2.DeclarationUCR);
				AssertEquals("DUCR propagates to entry", "1", entry2.DeclarationUCRPartSuffix);

				entry.Messages.AddNew().EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				entry2.Messages.AddNew().EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				declaration.JE_UCR = "New DUCR 2";
				Factory.Save();
				cusEntryNum = Factory.LoadTop1<CusEntryNumber>(cusEntryNumberFilter);
				cusEntryNum2 = Factory.LoadTop1<CusEntryNumber>(cusEntryNumberFilter2);
				AssertEquals("New DUCR 2", cusEntryNum.CE_EntryNum);
				AssertEquals("Propagates even after messages exist", "New DUCR 2", entry.CH_BGMReference);
				AssertEquals("New DUCR 2/1", cusEntryNum2.CE_EntryNum);
				AssertEquals("Propagates even after messages exist", "New DUCR 2/1", entry2.CH_BGMReference);

				entry.CH_Status = MessageStatusList.Codes.AwaitingResponse;
				entry2.CH_Status = MessageStatusList.Codes.AwaitingResponse;
				declaration.JE_UCR = "New DUCR 3";
				Factory.Save();
				cusEntryNum = Factory.LoadTop1<CusEntryNumber>(cusEntryNumberFilter);
				cusEntryNum2 = Factory.LoadTop1<CusEntryNumber>(cusEntryNumberFilter2);
				entry.Reload();
				entry2.Reload();
				cusEntryNum.Reload();
				cusEntryNum2.Reload();
				AssertEquals("Waiting for a response, no propagation", "New DUCR 2", entry.CH_BGMReference);
				AssertEquals("Waiting for a response, no propagation", "New DUCR 2", entry.DeclarationUCR);
				AssertEquals("New DUCR 2", cusEntryNum.CE_EntryNum);
				AssertEquals("Waiting for a response, no propagation", "New DUCR 2/1", entry2.CH_BGMReference);
				AssertEquals("Waiting for a response, no propagation", "New DUCR 2", entry2.DeclarationUCR);
				AssertEquals("Waiting for a response, no propagation", "1", entry2.DeclarationUCRPartSuffix);
				AssertEquals("New DUCR 2/1", cusEntryNum2.CE_EntryNum);

				entry.CH_Status = MessageStatusList.Codes.SentAndRejected;
				entry2.CH_Status = MessageStatusList.Codes.SentAndRejected;
				declaration.JE_UCR = "New DUCR 4";
				Factory.Save();
				entry.Reload();
				cusEntryNum.Reload();
				entry2.Reload();
				cusEntryNum2.Reload();
				AssertEquals("Not waiting for a response, propagation", "New DUCR 4", entry.CH_BGMReference);
				AssertEquals("Not waiting for a response, propagation", "New DUCR 4", entry.DeclarationUCR);
				AssertEquals("Not waiting for a response, propagation", "New DUCR 4", cusEntryNum.CE_EntryNum);
				AssertEquals("Not waiting for a response, propagation", "New DUCR 4/1", entry2.CH_BGMReference);
				AssertEquals("Not waiting for a response, propagation", "New DUCR 4", entry2.DeclarationUCR);
				AssertEquals("Not waiting for a response, propagation", "1", entry2.DeclarationUCRPartSuffix);
				AssertEquals("Not waiting for a response, propagation", "New DUCR 4/1", cusEntryNum2.CE_EntryNum);

				entry.EntryNumber = "I AM LODGED";
				entry2.EntryNumber = "I AM LODGED";
				declaration.JE_UCR = "New DUCR 5";
				Factory.Save();
				entry.Reload();
				cusEntryNum.Reload();
				entry2.Reload();
				cusEntryNum2.Reload();
				AssertEquals("After lodging, DUCR does not propagate. Still old value.", "New DUCR 4", entry.CH_BGMReference);
				AssertEquals("After lodging, DUCR does not propagate. Still old value.", "New DUCR 4", entry.DeclarationUCR);
				AssertEquals("After lodging, DUCR does not propagate. Still old value.", "New DUCR 4", cusEntryNum.CE_EntryNum);
				AssertEquals("After lodging, DUCR does not propagate. Still old value.", "New DUCR 4/1", entry2.CH_BGMReference);
				AssertEquals("After lodging, DUCR does not propagate. Still old value.", "New DUCR 4", entry2.DeclarationUCR);
				AssertEquals("After lodging, DUCR does not propagate. Still old value.", "1", entry2.DeclarationUCRPartSuffix);
				AssertEquals("After lodging, DUCR does not propagate. Still old value.", "New DUCR 4/1", cusEntryNum2.CE_EntryNum);
			}
		}

		public void TestEntryNumber()
		{
			var dec = (JobDeclaration)GetNewDeclaration();
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			AssertEquals(ZString.Empty, entryHeader.EntryNumber);
			entryHeader.EntryNumber = "123456789012";
			AssertEquals("123456789012", entryHeader.EntryNumber);
		}

		public void TestCountriesOfRouting()
		{
			// First check the shipment's consols....
			ForwardingShipment shipment = Enterprise.Freight.Forwarding.Business.Testing.ForwardingShipmentTest.CreateForwardingShipmentWithManyLegsForTesting(Factory);
			JobDeclaration dec = (JobDeclaration)GetNewDeclaration();
			var entry = (T)dec.CustomsEntryHeaders.AddNew();
			var snipOriginAndDest = !entry.IsOriginAndDestinationRequiredInItinerary_ForTest;

			dec.JE_JS = shipment.PK;
			dec.JE_RL_NKPortOfLoading = "NZABY";
			dec.JE_RL_NKPortOfArrival = "ESMAD";
			AssertEquals("The shipment goes through 5 countries - AU, HK, DE, FR, GB", 5, shipment.CountriesOfRouting.Count);
			List<ZString> routes = entry.CountriesOfRouting;
			if (snipOriginAndDest)
			{
				AssertEquals("But the entryHeader excludes the origin and dest (when snipping)", 5, routes.Count);
				// And only shows the interim countries
				AssertEquals("AU", routes[0]);
				AssertEquals("HK", routes[1]);
				AssertEquals("DE", routes[2]);
				AssertEquals("FR", routes[3]);
				AssertEquals("GB", routes[4]);
			}
			else
			{
				AssertEquals("But the entryHeader still includes the origin and dest", 7, routes.Count);
				AssertEquals("NZ", routes[0]);
				AssertEquals("AU", routes[1]);
				AssertEquals("HK", routes[2]);
				AssertEquals("DE", routes[3]);
				AssertEquals("FR", routes[4]);
				AssertEquals("GB", routes[5]);
				AssertEquals("ES", routes[6]);
			}

			//Check it doesn't blow up when we have a legless shipment
			dec = (JobDeclaration)GetNewDeclaration();
			entry = (T)dec.CustomsEntryHeaders.AddNew();
			shipment = Factory.New<ForwardingShipment>();
			dec.JE_JS = shipment.PK;
			if (snipOriginAndDest)
			{
				AssertEquals(0, entry.CountriesOfRouting.Count);
			}
			else
			{
				AssertEquals(1, entry.CountriesOfRouting.Count); // Both ports are empty and routng doesn't accept repeated strings
			}
			// Check it doesn't blow up with we have no shipment:
			dec.JE_JS = ZGuid.Empty;
			if (snipOriginAndDest)
			{
				AssertEquals(0, entry.CountriesOfRouting.Count);
			}
			else
			{
				AssertEquals(1, entry.CountriesOfRouting.Count); // Both ports are empty and routng doesn't accept repeated strings
			}

			// Now have a look at the scenario of a standalone dec without shipment and its own transports explicitly defined:
			dec.JE_JS = ZGuid.Empty;
			Transport trans1 = dec.Transports.AddNew();
			Transport trans2 = dec.Transports.AddNew();
			Transport trans3 = dec.Transports.AddNew();
			Transport trans4 = dec.Transports.AddNew();
			trans1.JW_ETA = ZDate.Today.AddDays(1);
			trans2.JW_ETA = ZDate.Today.AddDays(2);
			trans3.JW_ETA = ZDate.Today.AddDays(3);
			trans4.JW_ETA = ZDate.Today.AddDays(4);
			trans1.JW_RL_NKLoadPort = "AUSYD";
			trans1.JW_RL_NKDiscPort = "INBOM";
			trans2.JW_RL_NKLoadPort = "INBOM";
			trans2.JW_RL_NKDiscPort = "TRIST";
			trans3.JW_RL_NKLoadPort = "TRIST";
			trans3.JW_RL_NKDiscPort = "DEHAM";
			trans4.JW_RL_NKLoadPort = "DEHAM";
			trans4.JW_RL_NKDiscPort = "GBLBA";
			dec.JE_RL_NKOrigin = "AUSYD";
			dec.JE_RL_NKFinalDestination = "GBLBA";

			snipOriginAndDest = !entry.IsOriginAndDestinationRequiredInItinerary_ForTest;
			routes = entry.CountriesOfRouting;
			if (snipOriginAndDest)
			{
				AssertEquals(3, routes.Count);
				AssertEquals("IN", routes[0]);
				AssertEquals("TR", routes[1]);
				AssertEquals("DE", routes[2]);

				dec.JE_RL_NKOrigin = "ausyd";
				routes = entry.CountriesOfRouting;
				AssertEquals(3, routes.Count);
			}
			else
			{
				AssertEquals(5, routes.Count);
				AssertEquals("AU", routes[0]);
				AssertEquals("IN", routes[1]);
				AssertEquals("TR", routes[2]);
				AssertEquals("DE", routes[3]);
				AssertEquals("GB", routes[4]);
				dec.JE_RL_NKOrigin = "ausyd";
				routes = entry.CountriesOfRouting;
				AssertEquals(5, routes.Count);
			}
		}

		public void TestCountriesOfRoutingRepeatCountries()
		{
			JobDeclaration dec = (JobDeclaration)GetNewDeclaration();
			var entry = (T)dec.CustomsEntryHeaders.AddNew();

			dec.JE_JS = ZGuid.Empty;
			Transport trans1 = dec.Transports.AddNew();
			Transport trans2 = dec.Transports.AddNew();
			Transport trans3 = dec.Transports.AddNew();
			Transport trans4 = dec.Transports.AddNew();
			trans1.JW_ETA = ZDate.Today.AddDays(1);
			trans2.JW_ETA = ZDate.Today.AddDays(2);
			trans3.JW_ETA = ZDate.Today.AddDays(3);
			trans4.JW_ETA = ZDate.Today.AddDays(4);
			trans1.JW_RL_NKLoadPort = "AUSYD";
			trans1.JW_RL_NKDiscPort = "DEFRA";
			trans2.JW_RL_NKLoadPort = "DEFRA";
			trans2.JW_RL_NKDiscPort = "TRIST";
			trans3.JW_RL_NKLoadPort = "TRIST";
			trans3.JW_RL_NKDiscPort = "DEHAM";
			trans4.JW_RL_NKLoadPort = "DEHAM";
			trans4.JW_RL_NKDiscPort = "GBLBA";
			dec.JE_RL_NKOrigin = "AUSYD";
			dec.JE_RL_NKFinalDestination = "GBLON";

			var snipOriginAndDest = !entry.IsOriginAndDestinationRequiredInItinerary_ForTest;
			var repeatCountry = entry.CanRepeatCountriesInItinerary_ForTest;
			var routes = entry.CountriesOfRouting;
			if (snipOriginAndDest)
			{
				if (repeatCountry)
				{
					AssertEquals(3, routes.Count);
					AssertEquals("DE", routes[0]);
					AssertEquals("TR", routes[1]);
					AssertEquals("DE", routes[2]);

					dec.JE_RL_NKOrigin = "ausyd";
					routes = entry.CountriesOfRouting;
					AssertEquals(3, routes.Count);
				}
				else
				{
					AssertEquals(2, routes.Count);
					AssertEquals("DE", routes[0]);
					AssertEquals("TR", routes[1]);

					dec.JE_RL_NKOrigin = "ausyd";
					routes = entry.CountriesOfRouting;
					AssertEquals(2, routes.Count);
				}
			}
			else
			{
				if (repeatCountry)
				{
					AssertEquals(6, routes.Count);
					AssertEquals("AU", routes[0]);
					AssertEquals("DE", routes[1]);
					AssertEquals("TR", routes[2]);
					AssertEquals("DE", routes[3]);
					AssertEquals("GB", routes[4]);
					AssertEquals("GB", routes[5]);
					dec.JE_RL_NKOrigin = "ausyd";
					routes = entry.CountriesOfRouting;
					AssertEquals(6, routes.Count);
				}
				else
				{
					AssertEquals(4, routes.Count);
					AssertEquals("AU", routes[0]);
					AssertEquals("DE", routes[1]);
					AssertEquals("TR", routes[2]);
					AssertEquals("GB", routes[3]);
					dec.JE_RL_NKOrigin = "ausyd";
					routes = entry.CountriesOfRouting;
					AssertEquals(4, routes.Count);
				}
			}
		}

		public void TestCountriesOfRoutingDefaultTerritories()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.EUCTY, MapDirectionList.Codes.BTH, "Description", false);
			helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "ESCUE", "XC", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.Spain);
			helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "RS", "XS", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.Spain);
			helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "ESMLN", "XL", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.Spain);

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			{
				var dec = (JobDeclaration)GetNewDeclaration();
				var entry = dec.CustomsEntryHeaders.AddNew();
				var snipOriginAndDest = !entry.IsOriginAndDestinationRequiredInItinerary_ForTest;
				dec.JE_JS = ZGuid.Empty;
				Transport trans1 = dec.Transports.AddNew();
				Transport trans2 = dec.Transports.AddNew();
				Transport trans3 = dec.Transports.AddNew();
				trans1.JW_ETA = ZDate.Today.AddDays(1);
				trans2.JW_ETA = ZDate.Today.AddDays(2);
				trans3.JW_ETA = ZDate.Today.AddDays(3);
				trans1.JW_RL_NKLoadPort = "ESCUE";
				trans1.JW_RL_NKDiscPort = "ESMAD";
				trans2.JW_RL_NKLoadPort = "ESMAD";
				trans2.JW_RL_NKDiscPort = "DEHAM";
				trans3.JW_RL_NKLoadPort = "DEHAM";
				trans3.JW_RL_NKDiscPort = "RSBEG";
				trans3.JW_RL_NKLoadPort = "RSBEG";
				trans3.JW_RL_NKDiscPort = "AUMEL"; // Should not appear since it's the same country as destination
				dec.JE_RL_NKOrigin = "ESMLN";
				dec.JE_RL_NKFinalDestination = "AUSYD";

				var routes = entry.CountriesOfRouting;
				if (snipOriginAndDest)
				{
					AssertEquals(4, routes.Count);
					AssertEquals("XC", routes[0]);
					AssertEquals("ES", routes[1]);
					AssertEquals("DE", routes[2]);
					AssertEquals("XS", routes[3]);
					dec.JE_RL_NKOrigin = "esmln";
					routes = entry.CountriesOfRouting;
					AssertEquals(4, routes.Count);
				}
				else
				{
					AssertEquals(6, routes.Count);
					AssertEquals("XL", routes[0]);
					AssertEquals("XC", routes[1]);
					AssertEquals("ES", routes[2]);
					AssertEquals("DE", routes[3]);
					AssertEquals("XS", routes[4]);
					AssertEquals("AU", routes[5]);
					dec.JE_RL_NKOrigin = "esmln";
					routes = entry.CountriesOfRouting;
					AssertEquals(6, routes.Count);
				}
			}
		}

		public void TestZG_TransportChargesMethodOfPayment()
		{
			var dec = (JobDeclaration)GetNewDeclaration();
			var invHeader = dec.Invoices.AddNew();
			invHeader.ZG_TransportChargesMethodOfPayment = TransportChargesModeOfPayment.Codes.CreditCard;
			var invLine = invHeader.InvoiceLines.AddNew();

			var entry = dec.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			entryLine.InvoiceLines.Add(invLine);

			var entryNoMergedLines = dec.CustomsEntryHeaders.AddNew();

			AssertEquals(TransportChargesModeOfPayment.Codes.CreditCard, entry.TransportChargesMoP);
			AssertEquals(ZString.Empty, entryNoMergedLines.TransportChargesMoP);
		}

		public void TestErrorLineNumbersFromLastResponseMessage()
		{
			var dec = (JobDeclaration)GetNewDeclaration();
			if (!IntegratedCountryHelper.CountryHasBuiltInDeclaration(dec.CountryCode) || dec.IsDeclarationIntegrated)
			{
				Assert("For integrated countries, merge is turned off.", true);
				return;
			}
			var inv = dec.Invoices.AddNew();
			var invLine1 = inv.InvoiceLines.AddNew();
			var invLine2 = inv.InvoiceLines.AddNew();
			var invLine3 = inv.InvoiceLines.AddNew();
			invLine1.JI_Tariff = "1000";
			invLine2.JI_Tariff = "999";
			invLine3.JI_Tariff = "1000";
			dec.JE_MergeBy = "TRF";
			DoMerge(dec);
			var ceh = dec.CustomsEntryHeaders[0];
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("Pre req - no of merged lines", 2, ceh.MergedLines.Count);
				AssertEquals("Pre req - no of invoices", 2, ceh.MergedLines[0].InvoiceLines.Count);
				AssertEquals("Pre req - no of invoice lines on merged line", 1, ceh.MergedLines[1].InvoiceLines.Count);
				AssertEquals("Pre req - no errors on line 1", false, ceh.HasMessageResponseError(1));
				AssertEquals("Pre req - no errors on line 2", false, ceh.HasMessageResponseError(2));
				AssertEquals("Pre req - no errors on line 3", false, ceh.HasMessageResponseError(3));

				AssertEquals("Invoice line should start with no error flagged", false, invLine2.ZG_HadErrorInLastResponse);
				invLine2.ZG_HadErrorInLastResponse = true;
				AssertEquals("Property should be stored", true, invLine2.ZG_HadErrorInLastResponse);
				Factory.Save();
				var factory2 = new BusinessObjectFactory();
				var cehWithPublicPropertyReloaded = factory2.Load<CusEntryHeader>(ceh.PK); //HasMessageResponseError uses a cached collection
				AssertEquals("No Error Line 1", false, cehWithPublicPropertyReloaded.HasMessageResponseError(1));
				AssertEquals("Error Line 2", true, cehWithPublicPropertyReloaded.HasMessageResponseError(2));
				AssertEquals("No Error Line 3", false, cehWithPublicPropertyReloaded.HasMessageResponseError(3));
			});
		}

		public void TestUpdateLinesThatArePendingDeleteWhenStatusBecomesOk()
		{
			var dec = (JobDeclaration)GetNewDeclaration();
			var ceh = dec.CustomsEntryHeaders.AddNew();
			var line1 = ceh.AllEntryLines.AddNew();
			var line2 = ceh.AllEntryLines.AddNew();
			var line3 = ceh.AllEntryLines.AddNew();
			line1.CL_CustomsPostedStatus = Enterprise.Customs.Business.EntryLineStatusList.Codes.Active;
			line2.CL_CustomsPostedStatus = Enterprise.Customs.Business.EntryLineStatusList.Codes.DeletePending;
			line3.CL_CustomsPostedStatus = Enterprise.Customs.Business.EntryLineStatusList.Codes.Deleted;
			ceh.CH_Status = MessageStatusList.Codes.SentAndRejected;
			AssertEquals(Enterprise.Customs.Business.EntryLineStatusList.Codes.Active, line1.CL_CustomsPostedStatus);
			AssertEquals(Enterprise.Customs.Business.EntryLineStatusList.Codes.DeletePending, line2.CL_CustomsPostedStatus);
			AssertEquals(Enterprise.Customs.Business.EntryLineStatusList.Codes.Deleted, line3.CL_CustomsPostedStatus);
			ceh.CH_Status = MessageStatusList.Codes.OK;
			AssertEquals(Enterprise.Customs.Business.EntryLineStatusList.Codes.Active, line1.CL_CustomsPostedStatus);
			AssertEquals(Enterprise.Customs.Business.EntryLineStatusList.Codes.Deleted, line2.CL_CustomsPostedStatus);
			AssertEquals(Enterprise.Customs.Business.EntryLineStatusList.Codes.Deleted, line3.CL_CustomsPostedStatus);
		}

		public void TestBgmReferenceWithTrainingSuffix_Test()
		{
			if (IgnoreTestsThatRequireEntryShouldSetUCRinBGMReferenceNumber)
			{
				var entry = ((JobDeclaration)GetNewDeclaration()).CustomsEntryHeaders.AddNew();
				AssertEquals(false, entry.ShouldSetUCRinBGMReferenceNumber);
			}
			else
			{
				var dec = (JobDeclaration)GetNewDeclaration();
				dec.JE_UCR = "3GB1234-B0001000";
				dec.ZG_IsTrainingDeclaration = true;
				var ceh = dec.CustomsEntryHeaders.AddNew();
				Factory.Save();
				AssertEquals("3GB1234-B0001000T", ceh.CH_BGMReference);
				dec.JE_UCR = "3GB1234-B0002000";
				Factory.Save();
				AssertEquals("3GB1234-B0002000T", ceh.CH_BGMReference);
				dec.ZG_IsTrainingDeclaration = false;
				Factory.Save();
				AssertEquals("3GB1234-B0002000", ceh.CH_BGMReference);
				ceh.CH_BGMReference = "3GB1234-B0002000/1";
				Factory.Save();
				AssertEquals("3GB1234-B0002000/1", ceh.CH_BGMReference);
				dec.ZG_IsTrainingDeclaration = true;
				Factory.Save();
				AssertEquals("3GB1234-B0002000T/1", ceh.CH_BGMReference);
			}
		}

		public void TestBgmReferenceWithTrainingSuffixWithTwoCallsToSetter()
		{
			if (IgnoreTestsThatRequireEntryShouldSetUCRinBGMReferenceNumber)
			{
				var entry = ((JobDeclaration)GetNewDeclaration()).CustomsEntryHeaders.AddNew();
				AssertEquals(false, entry.ShouldSetUCRinBGMReferenceNumber);
			}
			else
			{
				var dec = (JobDeclaration)GetNewDeclaration();
				dec.JE_UCR = "3GB1234-B0001000";
				dec.ZG_IsTrainingDeclaration = true;
				dec.ZG_IsTrainingDeclaration = true;  // Second call with same value simulates the crappy way the GUI calls the setter a second time when the focus is lost from the tickbox
				var ceh = dec.CustomsEntryHeaders.AddNew();
				Factory.Save();
				AssertEquals("3GB1234-B0001000T", ceh.CH_BGMReference);
				dec.JE_UCR = "3GB1234-B0002000";
				Factory.Save();
				AssertEquals("3GB1234-B0002000T", ceh.CH_BGMReference);
				dec.ZG_IsTrainingDeclaration = false;
				dec.ZG_IsTrainingDeclaration = false;
				Factory.Save();
				AssertEquals("3GB1234-B0002000", ceh.CH_BGMReference);
				ceh.CH_BGMReference = "3GB1234-B0002000/1";
				Factory.Save();
				AssertEquals("3GB1234-B0002000/1", ceh.CH_BGMReference);
				dec.ZG_IsTrainingDeclaration = true;
				dec.ZG_IsTrainingDeclaration = true;
				Factory.Save();
				AssertEquals("3GB1234-B0002000T/1", ceh.CH_BGMReference);
			}
		}

		public virtual void TestIsIndirectExport()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				var dec = (JobDeclaration)GetNewDeclaration();
				var entry = dec.CustomsEntryHeaders.AddNew();
				dec.CustomsOffices.RemoveAndDeleteAll();
				Factory.Save();
				var cusOffice = dec.CustomsOffices.AddNew();
				cusOffice.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfExit;

				dec.JE_MessageType = "EXP";
				cusOffice.CY_Data = "GB00001";
				Assert(entry.IsIndirectExport);

				cusOffice.CY_Data = "FR00001";
				Assert(!entry.IsIndirectExport);

				cusOffice.CY_Data = "";
				Assert(!entry.IsIndirectExport);

				dec.JE_MessageType = "IMP";
				cusOffice = dec.CustomsOffices.AddNew();
				cusOffice.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfExit;
				cusOffice.CY_Data = "GB00001";
				Assert(!entry.IsIndirectExport);
			}
		}

		public virtual void TestComplementaryJob()
		{
			var declaration = (JobDeclaration)GetNewDeclaration();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var cusEntryNum1 = GetEntryNumForComplementaryJob("123", declaration);
			var cusEntryNum2 = GetEntryNumForComplementaryJob("456", declaration);
			var cusEntryNum3 = Factory.New<CusEntryNumber>();
			cusEntryNum3.CE_RN_NKCountryCode = declaration.CountryCode;
			cusEntryNum3.CE_EntryNum = "789";
			cusEntryNum3.CE_EntryType = "ABC";
			cusEntryNum3.CE_ParentTable = declaration.TableName;
			Factory.Save();

			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();

			var pd1 = GetPreviousDocumentForComplementaryJob(invoice1);

			var pd2 = invoice1.PreviousDocuments.AddNew();
			pd2.CSI_Code = "ABC";
			pd2.CSI_ReferenceNumber = "REF2";
			var pd3 = invoice1.PreviousDocuments.AddNew();
			pd3.CSI_Code = "ABC";
			pd3.CSI_ReferenceNumber = "REF3";
			var pd4 = invoice1.PreviousDocuments.AddNew();
			pd4.CSI_Code = "ABC";
			pd4.CSI_ReferenceNumber = "REF4";

			DoMerge(declaration);

			var entry = declaration.ActiveEntryHeaders[0] as T;

			cusEntryNum2.CE_ParentID = entry.PK;
			cusEntryNum2.CE_ParentTable = "CusEntryHeader";

			Factory.Save();

			var complementaryJob = entry.ComplementaryJob;

			CombineAssertions(() =>
			{
				AssertNotNull(complementaryJob);
				AssertEquals("456", complementaryJob.Reference);
				AssertEquals(entry, complementaryJob.Parent);
				AssertEquals("CusEntryHeader", complementaryJob.ParentTableCode);
			});

			cusEntryNum2.CE_ParentID = ZGuid.BrettsGuid;
			Factory.Save();
			complementaryJob = entry.ComplementaryJob;

			CombineAssertions(() =>
			{
				AssertNotNull(complementaryJob);
				AssertEquals("456", complementaryJob.Reference);
				AssertNull(complementaryJob.Parent);
				AssertEquals("CusEntryHeader", complementaryJob.ParentTableCode);
			});

			cusEntryNum2.CE_EntryNum = "ZZZ";
			Factory.Save();
			complementaryJob = entry.ComplementaryJob;

			CombineAssertions(() =>
			{
				AssertNotNull(complementaryJob);
				AssertEquals("456", complementaryJob.Reference);
				AssertNull(complementaryJob.Parent);
				AssertNullOrEmpty(complementaryJob.ParentTableCode);
			});

			pd1.CSI_Code = "ZZZ";
			Factory.Save();
			complementaryJob = entry.ComplementaryJob;
			AssertNull(complementaryJob);
		}

		public virtual void TestAmountAndTypeToBeGuaranteeds()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			helper.CreateNewOrGetExistingDataGrouping(currentCountry);
			var procedure = Factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = "Ye";
			procedure.ZZ6_PreviousProcedureCode = "12";
			procedure.ZZ6_ShipmentType = "EXP";
			procedure.ZZ6_IsGuaranteeConsumed = YesNoList.Codes.Yes;
			procedure.ZZ6_IsGuaranteeReleased = YesNoList.Codes.Yes;
			procedure.ZZ6_ZZZ_NKDataGrouping = currentCountry;
			procedure.ZZ6_Concession = "367";
			procedure.ZZ6_Description = "description";

			var dty = helper.CreateNewOrGetExistingRateType(currentCountry, Constants.RateTypes.Duty, "DTY");
			helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, dty.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty, dty.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty, dty.PK);
			Factory.Save();

			var declaration = GetNewDeclarationForTesting();
			var entryHeader = (T)declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "Ye12367";
			invoiceLine.JI_CL = entryLine.PK;

			AssertEquals(true, entryHeader.HasConsumingGuaranteeProcedure);
			var entryLine1 = entryHeader.MergedLines.AddNew();
			var fee1 = entryLine1.Fees.AddNew();
			fee1.CF_ChargeAmount = 10;
			fee1.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.Vat;
			var fee2 = entryLine1.Fees.AddNew();
			fee2.CF_ChargeAmount = 20;
			fee2.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			var fee3 = entryLine1.Fees.AddNew();
			fee3.CF_ChargeAmount = 30;
			fee3.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty;
			var fee4 = entryLine1.Fees.AddNew();
			fee4.CF_ChargeAmount = 40;
			fee4.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			var fee5 = entryLine2.Fees.AddNew();
			fee5.CF_ChargeAmount = 50;
			AssertEquals(1, entryHeader.AmountAndTypeToBeGuaranteeds.Count());
			AssertEquals(GuaranteeDebitType.NORMAL, entryHeader.AmountAndTypeToBeGuaranteeds.ToArray()[0].DebitType);
			AssertEquals(100m, entryHeader.AmountAndTypeToBeGuaranteeds.ToArray()[0].AmountInDeclarationCurrency);
		}

		protected virtual JobDeclaration GetNewDeclarationForTesting() => Factory.NewWithValidTestData<JobDeclaration>();

		public void TestDutyTotalUnion()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			helper.CreateNewOrGetExistingDataGrouping(currentCountry);
			Factory.Save();

			var dty = helper.CreateNewOrGetExistingRateType(currentCountry, Constants.RateTypes.Duty, "DTY");
			var cvd = helper.CreateNewOrGetExistingRateType(currentCountry, Constants.RateTypes.Countervailing, "CVD");
			var add = helper.CreateNewOrGetExistingRateType(currentCountry, Constants.RateTypes.AntiDumping, "ADD");
			var other = helper.CreateNewOrGetExistingRateType(currentCountry, Constants.RateTypes.Excise, "Other");

			helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, dty.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty, add.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty, cvd.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.ExportRefund, other.PK);
			Factory.Save();

			var declaration = GetNewDeclarationForTesting();
			var entryHeader = (T)declaration.ActiveEntryHeaders.AddNew();

			var entryLine1 = entryHeader.MergedLines.AddNew();
			var fee1 = entryLine1.Fees.AddNew();
			fee1.CF_ChargeAmount = 10;
			fee1.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.ExportRefund;
			var fee2 = entryLine1.Fees.AddNew();
			fee2.CF_ChargeAmount = 20;
			fee2.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			var fee3 = entryLine1.Fees.AddNew();
			fee3.CF_ChargeAmount = 30;
			fee3.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty;
			var fee4 = entryLine1.Fees.AddNew();
			fee4.CF_ChargeAmount = 40;
			fee4.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty;

			AssertEquals(90m, entryHeader.DutyTotalUnion);
		}

		public void TestTotalCustomsQuantity()
		{
			var declaration = (JobDeclaration)GetNewDeclaration();
			declaration.JE_MergeBy = "TRF";
			declaration.JE_ApplicationCode = "BLT";
			var inv = declaration.Invoices.AddNew();

			var invLine1 = inv.JobComInvoiceLines.AddNew();
			invLine1.JI_Tariff = "2203001010";
			invLine1.JI_CustomsQuantity = 100.00m;
			var invLine2 = inv.JobComInvoiceLines.AddNew();
			invLine2.JI_Tariff = "2203001010";
			invLine2.JI_CustomsQuantity = 200.00m;

			DoMerge(declaration);

			AssertEquals("Total Customs quantity is sum of single invoice lines", 300.00m, ((CusEntryHeader)declaration.ActiveEntryHeaders[0]).TotalCustomsQuantity);
		}

		#region SimplifiedDeclarationMRN

		public void TestSimplifiedDeclarationMRN()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertNull(CusEntryNumber.Load(entryHeader, CusEntryNumberTypes.EU.SimplifiedDeclarationMRN, entryHeader.CountryCode));
			AssertNullOrEmpty(entryHeader.SimplifiedDeclarationMRN);

			entryHeader.SetSimplifiedDeclarationMRN("12MRN345CDEFG678R9");

			var cusEntryNumbers = Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, SQLComparisonOperator.Equal, entryHeader.PK));
			AssertEquals(1, cusEntryNumbers.Length);
			var simplifiedDeclarationMRN = cusEntryNumbers.FirstOrDefault();
			AssertEquals(CusEntryNumberTypes.EU.SimplifiedDeclarationMRN, simplifiedDeclarationMRN.CE_EntryType);
			AssertEquals("12MRN345CDEFG678R9", entryHeader.SimplifiedDeclarationMRN);
		}

		public void TestSetSimplifiedDeclarationMRN()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryNumbers = Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, SQLComparisonOperator.Equal, entryHeader.PK));
			AssertEquals(0, cusEntryNumbers.Length);
			AssertNullOrEmpty(entryHeader.SimplifiedDeclarationMRN);

			entryHeader.SetSimplifiedDeclarationMRN("12MRN345CDEFG678R9");
			cusEntryNumbers = Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, SQLComparisonOperator.Equal, entryHeader.PK));
			AssertEquals(1, cusEntryNumbers.Length);
			var simplifiedDeclarationMRN = cusEntryNumbers.FirstOrDefault();
			AssertEquals(CusEntryNumberTypes.EU.SimplifiedDeclarationMRN, simplifiedDeclarationMRN.CE_EntryType);
			AssertEquals("12MRN345CDEFG678R9", entryHeader.SimplifiedDeclarationMRN);

			entryHeader.SetSimplifiedDeclarationMRN("00MRN345CDEFG678R9");
			cusEntryNumbers = Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, SQLComparisonOperator.Equal, entryHeader.PK));
			AssertEquals(1, cusEntryNumbers.Length);
			simplifiedDeclarationMRN = cusEntryNumbers.FirstOrDefault();
			AssertEquals(CusEntryNumberTypes.EU.SimplifiedDeclarationMRN, simplifiedDeclarationMRN.CE_EntryType);
			AssertEquals("00MRN345CDEFG678R9", entryHeader.SimplifiedDeclarationMRN);
		}

		#endregion

		protected virtual ZBool IgnoreTestsThatRequireEntryShouldSetUCRinBGMReferenceNumber => false;

		protected override Type ExpectedChargeCollectionType => typeof(CusEntryHeaderChargesCollection<CusEntryHeaderCharges>);

		protected override Type ExpectedChargeType => typeof(CusEntryHeaderCharges);

		protected virtual PreviousDocument GetPreviousDocumentForComplementaryJob(JobComInvoiceHeader invoice)
		{
			var prevDoc = invoice.PreviousDocuments.AddNew();
			prevDoc.CSI_ReferenceNumber = "456";
			prevDoc.CSI_Code = EUCommonConstants.PreviousDocumentCodeList.Cleared;

			return prevDoc;
		}

		protected virtual CusEntryNumber GetEntryNumForComplementaryJob(ZString entryNum, JobDeclaration declaration)
		{
			var cusEntryNum = Factory.New<CusEntryNumber>();
			cusEntryNum.CE_RN_NKCountryCode = declaration.CountryCode;
			cusEntryNum.CE_EntryNum = entryNum;
			cusEntryNum.CE_EntryType = Common.CusEntryNumberTypes.Standard.MovementReferenceNumber;
			cusEntryNum.CE_ParentTable = declaration.TableName;

			return cusEntryNum;
		}

		protected override IChargesCurrencyTestSetup GetChargesCurrencyTestSetup() => new ChargesCurrencyTestSetup();

		sealed class ChargesCurrencyTestSetup : IChargesCurrencyTestSetup
		{
			void IChargesCurrencyTestSetup.SetupJobDecWithOFTAndCIFCharges(BaseJobDeclaration declaration, ZString currencyCode)
			{
				declaration.AutoCreateChargesBasedOnIncoTerm = false;

				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_InvoiceAmount = 10000m;
				invoiceHeader.JZ_RX_NKInvoice_Currency = currencyCode;
				invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 10000m;

				var nonDutiableCharge = invoiceHeader.Charges.AddNew();
				nonDutiableCharge.J7_ChargeType = Enterprise.Customs.Business.CustomsChargeTypeList.Codes.ForeignInlandFreight;
				nonDutiableCharge.J7_Amount = 200m;
				nonDutiableCharge.J7_IsDutiable = false;
				nonDutiableCharge.J7_IsIncludedInITOT = true;

				var oft = invoiceHeader.Charges.AddNew();
				oft.J7_ChargeType = Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight;
				oft.J7_Amount = 500m;
				oft.J7_RX_NKCurrency = invoiceHeader.Invoice_Currency.RX_Code;
			}

			ZDecimal IChargesCurrencyTestSetup.ExpectedFOB => 10300m;
			ZDecimal IChargesCurrencyTestSetup.ExpectedCIF => 10800m;
		}

		T SetupEntryForFeesTest(BusinessObjectFactory factory) => CusEntryHeaderTestHelper.SetupEntryForFeesTest<T>(factory);
	}
}
