using System;
using System.Collections;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(JobComInvoiceHeader))]
	sealed class JobComInvoiceHeaderTest : Customs.Business.Testing.BaseJobComInvoiceHeaderTest<JobDeclaration, JobComInvoiceHeader, JobComInvoiceLine>
	{
		public override void TestChargeTypeList()
		{
			var dec = Factory.New<JobDeclaration>();
			Customs.Business.ICommonInvoice commonInvoice = dec.Invoices.AddNew();
			var chargeTypeList1 = commonInvoice.ChargeTypeList;
			var chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			var customsChargeTypeList = new Customs.Business.CustomsChargeTypeList();
			customsChargeTypeList.Sort();
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);

			dec.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var importChargeTypeList = commonInvoice.ChargeTypeList;
			AssertContainsExactElementsInAnyOrder(new[] { "EIC", "DIS", "EXW", "FIF", "LCH", "OTH", "CBC", "DPA", "OAC", "DEC", "ADD", "CIA", "COM", "DED", "ENG", "FIN", "IFE", "IFI", "ILO", "INS", "ISE", "ISI", "LOA", "LOE", "LOI", "MAT", "MCP", "PAC", "PAR", "ROT", "ROY", "TMM", "ONS", "FCO", "FNT", "OFC", "OFP" }, importChargeTypeList.GetAllCodes());

			dec.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			importChargeTypeList = commonInvoice.ChargeTypeList;
			AssertContainsExactElementsInAnyOrder(new[] { "EIC", "DIS", "EXW", "FIF", "LCH", "OTH", "CBC", "DPA", "OAC", "DEC", "ADD", "CIA", "COM", "DED", "ENG", "FIN", "IFE", "IFI", "ILO", "INS", "ISE", "ISI", "LOA", "LOE", "LOI", "MAT", "MCP", "PAC", "PAR", "ROT", "ROY", "TMM", "ONS", "FCO", "FNT", "OFC", "OFP" }, importChargeTypeList.GetAllCodes());
		}

		public void TestIncoTermAndChargeFactory()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			CombineAssertions(() =>
			{
				foreach (var messageType in new BRJobMessageTypeList().GetAllCodesZString())
				{
					declaration.JE_MessageType = messageType;
					switch (messageType)
					{
						case BRJobMessageTypeList.Codes.Import:
						case BRJobMessageTypeList.Codes.ImportSiscomex:
						case BRJobMessageTypeList.Codes.WarehousedByExternalAgent:
							AssertType<ImportIncoTermAndCustomsChargeFactory>(messageType, declaration.IncoTermAndChargeFactory);
							break;
						case BRJobMessageTypeList.Codes.ImportLicense:
							AssertType<ImportLicenseIncoTermAndCustomsChargeFactory>(messageType, declaration.IncoTermAndChargeFactory);
							break;
						case BRJobMessageTypeList.Codes.Export:
							AssertType<ExportIncoTermAndCustomsChargeFactory>(messageType, declaration.IncoTermAndChargeFactory);
							break;
						default:
							AssertType<IncoTermAndCustomsChargeFactory>(messageType, declaration.IncoTermAndChargeFactory);
							break;
					}
				}
			});
		}

		public void TestTypeDecider()
		{
			Assert("Update BaseJobComInvoiceHeaderTypeDecider to include a decider for this class", Factory.New(typeof(BaseJobComInvoiceHeader)).GetType() == GetExpectedBusinessObjectType());
		}

		public void TestLookups()
		{
			AssertEquals(Factory.GetCachedValue<ValuationCodeList>(), invoiceHeader.Lookups.ValuationCodeList);
			AssertEquals(Factory.GetCachedValue<RelatedIndicatorList>(), invoiceHeader.Lookups.RelatedIndicatorList);
		}

		public void TestIsExchangeHedgeAppliable()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			Assert("EXP: IsExchangeHedgeAppliable", !invoiceHeader.IsExchangeHedgeAppliable);
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			Assert("IMP: IsExchangeHedgeAppliable", invoiceHeader.IsExchangeHedgeAppliable);
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			Assert("LIC: IsExchangeHedgeAppliable", invoiceHeader.IsExchangeHedgeAppliable);
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			Assert("ISW: IsExchangeHedgeAppliable", invoiceHeader.IsExchangeHedgeAppliable);
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
			Assert("LPC: IsExchangeHedgeAppliable", !invoiceHeader.IsExchangeHedgeAppliable);
		}

		public void TestExchangeHedge()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			AssertEquals(0, header.ExchangeHedgeCollection.Count);
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertEquals("Adding a default ExchangeHedge for mandatory check on ExchangeHedgeType", 1, header.ExchangeHedgeCollection.Count);
			header.ExchangeHedgeCollection.RemoveAndDeleteAll();
			Factory.Save();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			AssertEquals("Adding a default ExchangeHedge for mandatory check on ExchangeHedgeType", 1, header.ExchangeHedgeCollection.Count);
			Assert("Adding a default ExchangeHedge won't make HasChanges on Header", !header.HasChanges);

			header.ExchangeHedgeReason = "1";
			header.ExchangeHedgeROFBACENNumber = "1";
			header.ExchangeHedgeFinancialInstitution = "1";
			header.ExchangeHedgeValue = 1;
			header.ExchangeHedgeType = ExchangeHedgeList.Codes._1;
			Assert("Reason should be empty", header.ExchangeHedgeReason.IsEmpty);
			Assert("ROF/BACEN Number should be empty", header.ExchangeHedgeROFBACENNumber.IsEmpty);
			Assert("Value should be empty", header.ExchangeHedgeValue.IsEmpty);
			Assert("Financial Institution should be empty", header.ExchangeHedgeFinancialInstitution.IsEmpty);

			header.ExchangeHedgeReason = "1";
			header.ExchangeHedgeROFBACENNumber = "1";
			header.ExchangeHedgeFinancialInstitution = "1";
			header.ExchangeHedgeValue = 1;
			header.ExchangeHedgeType = ExchangeHedgeList.Codes._2;
			Assert("Reason should be empty", header.ExchangeHedgeReason.IsEmpty);
			Assert("ROF/BACEN Number should be empty", header.ExchangeHedgeROFBACENNumber.IsEmpty);
			Assert("Value should be empty", header.ExchangeHedgeValue.IsEmpty);
			Assert("Financial Institution should be empty", header.ExchangeHedgeFinancialInstitution.IsEmpty);

			header.ExchangeHedgeReason = "1";
			header.ExchangeHedgeROFBACENNumber = "1";
			header.ExchangeHedgeFinancialInstitution = "1";
			header.ExchangeHedgeValue = 1;
			header.ExchangeHedgeType = ExchangeHedgeList.Codes._3;
			Assert("Reason should be empty", header.ExchangeHedgeReason.IsEmpty);
			Assert("ROF/BACEN Number should NOT be empty", !header.ExchangeHedgeROFBACENNumber.IsEmpty);
			Assert("Value should NOT be empty", !header.ExchangeHedgeValue.IsEmpty);
			Assert("Financial Institution should NOT be empty", !header.ExchangeHedgeFinancialInstitution.IsEmpty);

			header.ExchangeHedgeReason = "1";
			header.ExchangeHedgeROFBACENNumber = "1";
			header.ExchangeHedgeFinancialInstitution = "1";
			header.ExchangeHedgeValue = 1;
			header.ExchangeHedgeType = ExchangeHedgeList.Codes._4;
			Assert("Reason should NOT be empty", !header.ExchangeHedgeReason.IsEmpty);
			Assert("ROF/BACEN Number NOT should be empty", !header.ExchangeHedgeROFBACENNumber.IsEmpty);
			Assert("Value should be empty", header.ExchangeHedgeValue.IsEmpty);
			Assert("Financial Institution should be empty", header.ExchangeHedgeFinancialInstitution.IsEmpty);
		}

		public void TestGetNewValidation()
		{
			var invoiceHeader = (JobComInvoiceHeader)GetNewBusinessObject();
			var declaration = invoiceHeader.JobDeclaration;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			AssertEquals("Export Validation", typeof(ExportJobComInvoiceHeaderValidation), invoiceHeader.Validation.GetType());
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			AssertEquals("Import Validation", typeof(ImportJobComInvoiceHeaderValidation), invoiceHeader.Validation.GetType());
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Drawback;
			AssertEquals("Other Message Type", typeof(JobComInvoiceHeaderValidation), invoiceHeader.Validation.GetType());
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			AssertEquals("Import License Validation", typeof(ImportLicenseJobComInvoiceHeaderValidation), invoiceHeader.Validation.GetType());
		}

		public void TestDefaultValuesFromSupplierBuyerLink()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var invoiceHeader = declaration.Invoices.AddNew();
			var importer = OrgHeader.New(Factory);
			var supplier = OrgHeader.New(Factory);

			var link = Factory.New<OrgSupplierBuyerLink>();
			link.OL_OH_Buyer = importer.PK;
			link.OL_OH_Supplier = supplier.PK;
			link.OL_RN_NKImporterCountry = Enterprise.Core.Constants.CountryCodes.Brazil;
			link.OL_RelatedParty = "Y";

			invoiceHeader.JobDeclaration.JE_OH_Importer = importer.PK;
			invoiceHeader.JZ_OA_SupplierAddress = supplier.MainAddress.PK;
			AssertEquals("Y", invoiceHeader.JZ_RelatedIndicator);
		}

		public void TestSupplierOrgPKDefaulting()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var invoiceHeader = (JobComInvoiceHeader)GetNewBusinessObject();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			invoiceHeader.SupplierOrgPK = supplier.PK;

			AssertEquals("JZ_OA_SupplierAddress_ZAddress.OrgPK should be equal to SupplierOrgPK", invoiceHeader.JZ_OA_SupplierAddress_ZAddress.OrgPK, invoiceHeader.SupplierOrgPK);
		}

		public void TestReadOnlyWhenClonedFromAttached()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_JE = declaration.PK;
			entryInstruction.CEI_Description = "TEST1";

			var licDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			licDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var licEntryInstruction = licDeclaration.CustomsEntryInstructions.AddNew();
			licEntryInstruction.CEI_JE = licDeclaration.PK;
			licEntryInstruction.CEI_Description = "TEST1";

			var licInvHeader = licDeclaration.Invoices.AddNew();
			var licInvLine = licInvHeader.InvoiceLines.AddNew();
			licInvLine.JI_CEI = licEntryInstruction.PK;

			var licEntryHeader = licDeclaration.CustomsEntryHeaders.AddNew();
			licEntryHeader.CH_CEI_Instruction = licEntryInstruction.PK;
			var licEntryLine = licEntryHeader.AllEntryLines.AddNew();
			licInvLine.JI_CL = licEntryLine.PK;

			licEntryInstruction.EntryHeader.MovementReferenceNumberSetter("TST_LIC", ZDateTime.Now);
			declaration.AttachImportLicense(new[] { new ImportLicenseAttachingObject(licEntryInstruction) });

			var clonedInvoiceHeader = declaration.Invoices.Cast<JobComInvoiceHeader>().FirstOrDefault();

			var invHeader = declaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = entryInstruction.PK;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			invLine.JI_CL = entryLine.PK;

			var editableProperties = invHeader.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(x => !x.ReadOnly).Select(x => x.Name);

			var readOnlyPropertiesList = new[] { JobComInvoiceHeader.Schema.IncoTerm, JobComInvoiceHeader.Schema.JZ_IncoTerm, JobComInvoiceHeader.Schema.JZ_RX_NKInvoice_Currency, JobComInvoiceHeader.Schema.SupplierOrgPK, JobComInvoiceHeader.Schema.JZ_OA_SupplierAddress, JobComInvoiceHeader.Schema.ExchangeHedgeType };

			CombineAssertions("Cloned Invoice Line", () =>
			{
				Assert("AnyLineHasLinkedInvoiceLine should be true", clonedInvoiceHeader.AnyLineHasLinkedInvoiceLine);

				foreach (var propertyName in editableProperties)
				{
					if (readOnlyPropertiesList.Contains(propertyName))
					{
						Assert($"{propertyName} should be ReadOnly", clonedInvoiceHeader.FindPropertyInfo(propertyName).ReadOnly);
					}
					else
					{
						Assert($"{propertyName} should NOT be ReadOnly", !clonedInvoiceHeader.FindPropertyInfo(propertyName).ReadOnly);
					}
				}

				Assert("ExchangeHedgeCollection should be ReadOnly", clonedInvoiceHeader.ExchangeHedgeCollection.ReadOnly);
			});

			var newInvoiceHeader = declaration.Invoices.Cast<JobComInvoiceHeader>().LastOrDefault();

			CombineAssertions("New Invoice Line", () =>
			{
				Assert("AnyLineHasLinkedInvoiceLine should be false", !newInvoiceHeader.AnyLineHasLinkedInvoiceLine);

				foreach (var propertyName in editableProperties)
				{
					Assert($"{propertyName} should NOT be ReadOnly", !newInvoiceHeader.FindPropertyInfo(propertyName).ReadOnly);
				}

				Assert("ExchangeHedgeCollection should NOT be ReadOnly", !newInvoiceHeader.ExchangeHedgeCollection.ReadOnly);
			});
		}

		public void TestAnyLineHasLinkedInvoiceLine()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoice1 = declaration1.Invoices.AddNew();
			AssertEquals(false, invoice1.AnyLineHasLinkedInvoiceLine);

			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine2 = declaration2.Invoices.AddNew().InvoiceLines.AddNew();
			AssertEquals(false, invoice1.AnyLineHasLinkedInvoiceLine);
			invoiceLine1.JI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			invoiceLine1.JI_ParentID = invoiceLine2.PK;
			AssertEquals(true, invoice1.AnyLineHasLinkedInvoiceLine);
			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			AssertEquals(true, invoice1.AnyLineHasLinkedInvoiceLine);
			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertEquals(true, invoice1.AnyLineHasLinkedInvoiceLine);
			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			AssertEquals(false, invoice1.AnyLineHasLinkedInvoiceLine);
		}

		public void TestAnyLineAttachedToImportLicenseLine()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoice1 = declaration1.Invoices.AddNew();
			AssertEquals(false, invoice1.AnyLineAttachedToImportLicenseLine);

			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var invoiceLine2 = declaration2.Invoices.AddNew().InvoiceLines.AddNew();
			AssertEquals(false, invoice1.AnyLineAttachedToImportLicenseLine);
			invoiceLine2.JI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			AssertEquals(true, invoice1.AnyLineAttachedToImportLicenseLine);
			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertEquals(true, invoice1.AnyLineAttachedToImportLicenseLine);

			declaration2.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			AssertEquals(false, invoice1.AnyLineAttachedToImportLicenseLine);
			declaration2.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertEquals(false, invoice1.AnyLineAttachedToImportLicenseLine);
			declaration2.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			AssertEquals(false, invoice1.AnyLineAttachedToImportLicenseLine);
		}

		public void TestAnyLineIsImportLicenseGeneratedFromImportSiscomexLine()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var invoice1 = declaration1.Invoices.AddNew();
			AssertEquals(false, invoice1.AnyLineIsImportLicenseGeneratedFromImportSiscomexLine);

			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine2 = declaration2.Invoices.AddNew().InvoiceLines.AddNew();
			AssertEquals(false, invoice1.AnyLineIsImportLicenseGeneratedFromImportSiscomexLine);
			invoiceLine1.JI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			invoiceLine1.JI_ParentID = invoiceLine2.PK;
			AssertEquals(false, invoice1.AnyLineIsImportLicenseGeneratedFromImportSiscomexLine);
			invoiceLine2.JI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			AssertEquals(true, invoice1.AnyLineIsImportLicenseGeneratedFromImportSiscomexLine);

			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			AssertEquals(false, invoice1.AnyLineIsImportLicenseGeneratedFromImportSiscomexLine);
			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertEquals(false, invoice1.AnyLineIsImportLicenseGeneratedFromImportSiscomexLine);
			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			AssertEquals(false, invoice1.AnyLineIsImportLicenseGeneratedFromImportSiscomexLine);
		}

		public void TestOnFactorySaving()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_IncoTerm = "CIF";
			invoiceHeader.JZ_InvoiceAmount = 1412.00m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";

			invoiceHeader.ExchangeHedgeType = "1";
			invoiceHeader.ExchangeHedgePaymentMethod = "3";
			invoiceHeader.ExchangeHedgePaymentDeadline = 339;
			invoiceHeader.ExchangeHedgeReason = "TE";
			invoiceHeader.ExchangeHedgeFinancialInstitution = "2";

			Factory.Save();

			AssertEquals("There are ExchangeHedgeCollection", true, invoiceHeader.ExchangeHedge.IsInDatabase);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Drawback;

			Factory.Save();

			AssertEquals("There are no ExchangeHedgeCollection", false, invoiceHeader.ExchangeHedge.IsInDatabase);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;

			invoiceHeader.ExchangeHedgeType = "1";
			invoiceHeader.ExchangeHedgePaymentMethod = "3";
			invoiceHeader.ExchangeHedgePaymentDeadline = 339;
			invoiceHeader.ExchangeHedgeReason = "TE";
			invoiceHeader.ExchangeHedgeFinancialInstitution = "2";

			Factory.Save();

			AssertEquals("There are no ExchangeHedgeCollection", false, invoiceHeader.ExchangeHedge.IsInDatabase);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			invoiceHeader.ExchangeHedgeType = "1";
			invoiceHeader.ExchangeHedgePaymentMethod = "3";
			invoiceHeader.ExchangeHedgePaymentDeadline = 339;
			invoiceHeader.ExchangeHedgeReason = "TE";
			invoiceHeader.ExchangeHedgeFinancialInstitution = "2";

			Factory.Save();

			AssertEquals("There are ExchangeHedgeCollection", true, invoiceHeader.ExchangeHedge.IsInDatabase);
		}

		[TestDate(2023, 1, 1)]
		public void TestExchangeRateDate()
		{
			var currency = Factory.New<RefCurrency>();
			currency.RX_Code = "XYZ";

			var rate = currency.ExchangeRates.AddNew();
			rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			rate.RE_StartDate = ZDateTime.Today.AddDays(-5);
			rate.RE_ExpiryDate = ZDateTime.Today.AddDays(-5);
			rate.RE_SellRate = 4m;

			rate = currency.ExchangeRates.AddNew();
			rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			rate.RE_StartDate = ZDateTime.Today.AddDays(-1);
			rate.RE_ExpiryDate = ZDateTime.Today.AddDays(-1);
			rate.RE_SellRate = 5m;

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var header = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			AssertEquals("IsJZ_InvoiceCurrExRateUserEnterable should be", false, header.IsJZ_InvoiceCurrExRateUserEnterable);
			AssertEquals("JZ_ValuationDateOverride should be empty", true, header.JZ_ValuationDateOverride.IsEmpty);
			AssertEquals("ExchangeRateDate should be empty", false, header.ExchangeRateDate.IsEmpty);
			AssertEquals("ExchangeRateDate_ReadOnly should be", true, header.ExchangeRateDateInfo.ReadOnly);
			AssertEquals("JZ_InvoiceCurrExRate should be", 0m, header.JZ_InvoiceCurrExRate);
			AssertEquals("JZ_InvoiceCurrExRate_ReadOnly should be", true, header.JZ_InvoiceCurrExRateInfo.ReadOnly);

			header.JZ_RX_NKInvoice_Currency = "XYZ";
			AssertEquals("IsJZ_InvoiceCurrExRateUserEnterable should be", false, header.IsJZ_InvoiceCurrExRateUserEnterable);
			AssertEquals("JZ_ValuationDateOverride should be empty", true, header.JZ_ValuationDateOverride.IsEmpty);
			AssertEquals("ExchangeRateDate should be", ZDateTime.Today.AddDays(-1), header.ExchangeRateDate);
			AssertEquals("ExchangeRateDate_ReadOnly should be", true, header.ExchangeRateDateInfo.ReadOnly);
			AssertEquals("JZ_InvoiceCurrExRate should be", 5m, header.JZ_InvoiceCurrExRate);
			AssertEquals("JZ_InvoiceCurrExRate_ReadOnly should be", true, header.JZ_InvoiceCurrExRateInfo.ReadOnly);

			header.IsJZ_InvoiceCurrExRateUserEnterable = true;
			header.ExchangeRateDate = ZDateTime.Today.AddDays(-5);
			AssertEquals("IsJZ_InvoiceCurrExRateUserEnterable should be", true, header.IsJZ_InvoiceCurrExRateUserEnterable);
			AssertEquals("JZ_ValuationDateOverride should be", ZDateTime.Today.AddDays(-5), header.JZ_ValuationDateOverride);
			AssertEquals("ExchangeRateDate should be", ZDateTime.Today.AddDays(-5), header.ExchangeRateDate);
			AssertEquals("ExchangeRateDate_ReadOnly should be", false, header.ExchangeRateDateInfo.ReadOnly);
			AssertEquals("JZ_InvoiceCurrExRate should be", 4m, header.JZ_InvoiceCurrExRate);
			AssertEquals("JZ_InvoiceCurrExRate_ReadOnly should be", true, header.JZ_InvoiceCurrExRateInfo.ReadOnly);
		}

		public override void TestInvoiceLineChargeAffectsBalanceCalculation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 25000m;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 10000m;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 20000m;
			declaration.ResumeApportionment();

			AssertEquals(0m, invoice.JZ_Calc_ChargesExcludedFromITOT);
			AssertNotEquals("Not balanced yet", 0m, invoice.JZ_Calc_Balance);

			var lineCharge = invoiceLine2.Charges.AddNew(GetDiscountChargeCodeForTest(), 5000m, declaration.LocalCurrencyCode);
			lineCharge.J7_IsDutiable = false;
			lineCharge.J7_IsGSTApplicable = false;
			declaration.ResumeApportionment();
			AssertEquals("line level Discount", -5000m, invoice.JZ_Calc_ChargesExcludedFromITOT);
			AssertEquals("Now balanced", 0m, invoice.JZ_Calc_Balance);

			var invoiceCharge = invoice.Charges.AddNew(GetDiscountChargeCodeForTest(), 5000m, declaration.LocalCurrencyCode);
			invoiceCharge.J7_IsDutiable = false;
			invoiceCharge.J7_IsGSTApplicable = false;
			declaration.ResumeApportionment();
			AssertEquals("Invoice Level Discount. Line level Discount is disregarded", -5000m, invoice.JZ_Calc_ChargesExcludedFromITOT);
			AssertEquals("Still balanced", 0m, invoice.JZ_Calc_Balance);
		}

		public void TestDefaultSupplierAddressFromSupplier()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			var supplier1 = Factory.New<OrgHeader>();
			supplier1.OH_FullName = "Supplier Org 1";
			supplier1.MainAddress.OA_Address1 = "Main Address Supp Org 1";

			var supplier2 = Factory.New<OrgHeader>();
			supplier2.OH_FullName = "Supplier Org 2";
			supplier2.MainAddress.OA_Address1 = "Main Address Supp Org 2";

			declaration.JE_OH_Supplier = supplier1.PK;

			foreach (var messageType in new[] { BRJobMessageTypeList.Codes.Import, BRJobMessageTypeList.Codes.ImportSiscomex, BRJobMessageTypeList.Codes.Export })
			{
				CombineAssertions(messageType, () =>
				{
					declaration.JE_MessageType = messageType;

					var invoiceHeader1 = declaration.Invoices.AddNew();
					AssertEquals("JZ_OA_SupplierAddress should be default to MainAddress of Decalration's Supplier", supplier1.MainAddress.PK, invoiceHeader1.JZ_OA_SupplierAddress);

					invoiceHeader1.JZ_OH_Supplier = supplier2.PK;
					AssertEquals("JZ_OA_SupplierAddress should be changed to MainAddress of Supplier", supplier2.MainAddress.PK, invoiceHeader1.JZ_OA_SupplierAddress);

					var invoiceHeader2 = declaration.Invoices.AddNew();
					AssertEquals("JZ_OA_SupplierAddress should be copied from Previous Invoice", supplier2.MainAddress.PK, invoiceHeader2.JZ_OA_SupplierAddress);

					invoiceHeader2.JZ_OH_Supplier = ZGuid.Empty;
					AssertEquals("JZ_OA_SupplierAddress should be empty", ZGuid.Empty, invoiceHeader2.JZ_OA_SupplierAddress);
					AssertEquals("JZ_OA_SupplierAddress should be empty", ZGuid.Empty, invoiceHeader2.JZ_OA_SupplierAddress_ZAddress.OrgPK);
				});
			}
		}

		public void TestSupplierAddressIsAvailable()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			var invoiceHeader = declaration.Invoices.AddNew();

			AssertEquals("SupplierAddressIsAvailable should be", true, invoiceHeader.SupplierAddressIsAvailable);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			AssertEquals("SupplierAddressIsAvailable should be", false, invoiceHeader.SupplierAddressIsAvailable);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			AssertEquals("SupplierAddressIsAvailable should be", true, invoiceHeader.SupplierAddressIsAvailable);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;

			AssertEquals("SupplierAddressIsAvailable should be", false, invoiceHeader.SupplierAddressIsAvailable);
		}

		public void TestSupplierDocOrgPK()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			var supplier2 = Factory.NewWithValidTestData<OrgHeader>();
			var supplierAddress2 = supplier2.Addresses.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.SupplierDocumentaryAddress.OrganisationPK = supplier1.PK;

			AssertEquals("Get SupplierDocOrgPK", supplier1.PK, invoiceHeader.SupplierDocOrgPK);
			AssertEquals("Get SupplierDocAddressPK", supplier1.MainAddress.PK, invoiceHeader.SupplierDocAddressPK);

			invoiceHeader.SupplierDocOrgPK = supplier2.PK;
			AssertEquals("Set SupplierDocOrgPK", supplier2.PK, invoiceHeader.SupplierDocumentaryAddress.OrganisationPK);
			AssertEquals("Set SupplierDocAddressPK", supplier2.MainAddress.PK, invoiceHeader.SupplierDocumentaryAddress.E2_OA_Address);

			invoiceHeader.SupplierDocAddressPK = supplierAddress2.PK;
			AssertEquals("Set SupplierDocOrgPK", supplier2.PK, invoiceHeader.SupplierDocumentaryAddress.OrganisationPK);
			AssertEquals("Set SupplierDocAddressPK", supplierAddress2.PK, invoiceHeader.SupplierDocumentaryAddress.E2_OA_Address);
		}

		public void TestSupplierDocumentaryAddress()
		{
			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			var supplierAddress1 = supplier1.Addresses.AddNew();
			supplierAddress1.OA_Address1 = "Supplier Address";

			var supplier2 = Factory.NewWithValidTestData<OrgHeader>();
			var supplierAddress2 = supplier2.Addresses.AddNew();
			supplierAddress2.OA_Address1 = "Supplier Address";

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_OH_Supplier = supplier1.PK;

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var supplierDocAddress = invoiceHeader.SupplierDocumentaryAddress;
			CombineAssertions("Defaulting SupplierDocumentaryAddress", () =>
			{
				AssertNotNull("SupplierDocumentaryAddress should NOT be null", invoiceHeader.SupplierDocumentaryAddress);
				AssertEquals("DocAddressType", DocAddressType.SupplierDocumentaryAddress, supplierDocAddress.DocAddressType);
				AssertEquals("DefaultAddressType", ZArchitecture.Business.AddressType.NoDefault, supplierDocAddress.DefaultAddressType);
				AssertEquals("SupplierDocumentaryAddress.OrganisationPK should default from JZ_OH_Supplier", supplier1.PK, invoiceHeader.SupplierDocumentaryAddress.OrganisationPK);
				AssertEquals("SupplierDocumentaryAddress.E2_OA_Address should default from JZ_OH_Supplier", supplier1.MainAddress.PK, invoiceHeader.SupplierDocumentaryAddress.E2_OA_Address);
			});

			invoiceHeader.JZ_OA_SupplierAddress = supplierAddress2.PK;
			CombineAssertions("Sync on setting JZ_OA_SupplierAddress", () =>
			{
				AssertEquals("JZ_OH_Supplier", supplier2.PK, invoiceHeader.JZ_OH_Supplier);
				AssertEquals("SupplierDocumentaryAddress", supplierAddress2.PK, invoiceHeader.SupplierDocumentaryAddress.E2_OA_Address);
			});

			invoiceHeader.SupplierDocumentaryAddress.E2_OA_Address = supplierAddress1.PK;
			CombineAssertions("Sync on setting SupplierDocumentaryAddress.E2_OA_Address", () =>
			{
				AssertEquals("JZ_OH_Supplier", supplier1.PK, invoiceHeader.JZ_OH_Supplier);
				AssertEquals("JZ_OA_SupplierAddress", supplierAddress1.PK, invoiceHeader.JZ_OA_SupplierAddress);
			});

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			Factory.Save();

			Assert("SupplierDocumentaryAddress should be deleted", supplierDocAddress.IsDeleted);
		}

		public void TestJZ_OA_SupplierAddress_Changed()
		{
			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			var supplierAddress1 = supplier1.Addresses.AddNew();
			supplierAddress1.OA_Address1 = "Supplier Address";

			var supplier2 = Factory.NewWithValidTestData<OrgHeader>();
			var supplierAddress2 = supplier2.Addresses.AddNew();
			supplierAddress2.OA_Address1 = "Supplier Address";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_OH_Supplier = supplier1.PK;

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._2;

			invoiceHeader.JZ_OA_SupplierAddress = supplierAddress2.PK;
			AssertEquals("JI_ManufacturerIndicator", ZString.Empty, invoiceLine.JI_ManufacturerIndicator);
		}

		public void TestSupplierAuthorityReadOnly()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			Assert("JZ_SupplierAuthorityIdentifier should NOT be ReadOnly", !invoiceHeader.JZ_SupplierAuthorityIdentifierInfo.ReadOnly);
			Assert("JZ_SupplierAuthorityVersion should NOT be ReadOnly", !invoiceHeader.JZ_SupplierAuthorityVersionInfo.ReadOnly);
		}

		public void TestCanDeleteAndReasonForNotAbleToDelete_AttachedToImportLicense()
		{
			var invoice = Factory.New<JobDeclaration>().Invoices.AddNew();
			invoice.JobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var comInvLine = invoice.InvoiceLines.AddNew();

			CombineAssertions(() =>
			{
				Assert("Can Delete when not attached to any LIC lines", ((ICanDelete)comInvLine).CanDelete);
				AssertNotEquals("ReasonForNotAbleToDelete", ZString.Empty, ((ICanDelete)comInvLine).ReasonForNotAbleToDelete);
			});

			var licDeclaration = Factory.New<JobDeclaration>();
			licDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			licDeclaration.JE_DeclarationReference = "LIC000001";
			var licInvoice = licDeclaration.Invoices.AddNew();
			var copyInvoiceBo = new BRJobComInvoiceHeaderCopyBO(invoice, licInvoice, null);
			copyInvoiceBo.CopyInvoice();

			foreach (var messageType in new[] { BRJobMessageTypeList.Codes.Import, BRJobMessageTypeList.Codes.ImportSiscomex, BRJobMessageTypeList.Codes.ImportLicense, BRJobMessageTypeList.Codes.Export })
			{
				invoice.JobDeclaration.JE_MessageType = messageType;

				CombineAssertions(() =>
				{
					Assert("Can NOT Delete when attached to any LIC lines", !((ICanDelete)invoice).CanDelete);
					AssertEquals("ReasonForNotAbleToDelete", "The Invoice Header cannot be deleted, because there is Import License line reference some Invoice Lines on it.", ((ICanDelete)invoice).ReasonForNotAbleToDelete);
				});
			}

			licDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				Assert("Can Delete for when attached to any IMP lines", ((ICanDelete)invoice).CanDelete);
				AssertEquals("ReasonForNotAbleToDelete", ZString.Empty, ((ICanDelete)invoice).ReasonForNotAbleToDelete);
			});
		}

		public void TestCanDeleteAndReasonForNotAbleToDelete_GeneratedImportLicense()
		{
			var reasonForNotAbleToDelete = "The Invoice Header cannot be deleted, because there is Import License line attached to some Import Entries.";

			var iswDeclaration = Factory.New<JobDeclaration>();
			iswDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var licDeclaration = Factory.New<JobDeclaration>();
			licDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			licDeclaration.JE_DeclarationReference = "LIC000001";
			var licInstruction = licDeclaration.CustomsEntryInstructions.AddNew();
			var licInvoice = licDeclaration.Invoices.AddNew();

			CombineAssertions(() =>
			{
				Assert("Can Delete when no lines", ((ICanDelete)licInvoice).CanDelete);
				AssertEquals("ReasonForNotAbleToDelete", ZString.Empty, ((ICanDelete)licInvoice).ReasonForNotAbleToDelete);
			});

			var licInvLine = licInvoice.InvoiceLines.AddNew();
			licInvLine.JI_CEI = licInstruction.PK;

			CombineAssertions(() =>
			{
				Assert("Can Delete when not attached to any lines", ((ICanDelete)licInvoice).CanDelete);
				AssertEquals("ReasonForNotAbleToDelete", ZString.Empty, ((ICanDelete)licInvoice).ReasonForNotAbleToDelete);
			});

			iswDeclaration.AttachImportLicense(new[] { new ImportLicenseAttachingObject(licInstruction) });

			CombineAssertions(() =>
			{
				Assert("Can Delete when attached to ISW Line", ((ICanDelete)licInvoice).CanDelete);
				AssertEquals("ReasonForNotAbleToDelete", ZString.Empty, ((ICanDelete)licInvoice).ReasonForNotAbleToDelete);
			});

			var iswInstruction = iswDeclaration.CustomsEntryInstructions[0];
			var iswInvLine = iswDeclaration.InvoiceLines[0];
			var iswEntryHeader = iswDeclaration.ActiveEntryHeaders.AddNew();
			iswEntryHeader.CH_JE = iswDeclaration.PK;
			iswEntryHeader.CH_CEI_Instruction = iswInstruction.PK;
			var iswEntryLine = iswEntryHeader.MergedLines.AddNew();
			iswInvLine.JI_CL = iswEntryLine.PK;

			licDeclaration.InvoiceLines.RemoveAndDeleteAll();
			var generator = new GenerateImportLicenseObject(iswEntryLine);
			generator.ImportLicenseDeclarationPK = licDeclaration.PK;
			generator.GenerateImportLicense();
			var generatedInvoice = licDeclaration.Invoices[0];

			CombineAssertions(() =>
			{
				Assert("Can NOT Delete when generated from ISW Line", !((ICanDelete)generatedInvoice).CanDelete);
				AssertEquals("ReasonForNotAbleToDelete", reasonForNotAbleToDelete, ((ICanDelete)generatedInvoice).ReasonForNotAbleToDelete);
			});

			iswDeclaration.DetachImportLicense(licDeclaration.CustomsEntryInstructions);

			CombineAssertions(() =>
			{
				Assert("Can Delete when detached to ISW Line", ((ICanDelete)generatedInvoice).CanDelete);
				AssertEquals("ReasonForNotAbleToDelete", ZString.Empty, ((ICanDelete)licInvoice).ReasonForNotAbleToDelete);
			});
		}

		public void TestUpdateTotalValuesFromInvoiceLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice.JZ_WeightUQ = "KG";
			invoice.JZ_NetWeightUQ = "KG";

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 100m;
			invoiceLine1.JI_Weight = 50M;
			invoiceLine1.JI_WeightUQ = "KG";
			invoiceLine1.JI_NetWeight = 50M;
			invoiceLine1.JI_NetWeightUQ = "KG";

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 100m;
			invoiceLine2.JI_Weight = 0.5M;
			invoiceLine2.JI_WeightUQ = Core.Constants.Weight.Tonnes;
			invoiceLine2.JI_NetWeight = 0.5M;
			invoiceLine2.JI_NetWeightUQ = Core.Constants.Weight.Tonnes;

			AssertEquals("JZ_Weight should be", 0m, invoice.JZ_Weight);
			AssertEquals("JZ_NetWeight should be", 0m, invoice.JZ_NetWeight);
			AssertEquals("JZ_InvoiceAmount should be", 0m, invoice.JZ_InvoiceAmount);

			invoice.UpdateTotalValuesFromInvoiceLines();
			AssertEquals("JZ_Weight should be", 550m, invoice.JZ_Weight);
			AssertEquals("JZ_NetWeight should be", 550m, invoice.JZ_NetWeight);
			AssertEquals("JZ_InvoiceAmount should be", 200m, invoice.JZ_InvoiceAmount);
		}

		public void TestDeserialiseMessageTypeToStandaloneMessageType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.MakeNonPersistent();

			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_JE = declaration.PK;
			AssertEquals(BRJobMessageTypeList.Codes.Import, invoice.JZ_MessageType);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_JE = declaration.PK;
			AssertEquals(BRJobMessageTypeList.Codes.Import, invoice.JZ_MessageType);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_JE = declaration.PK;
			AssertEquals(BRJobMessageTypeList.Codes.Import, invoice.JZ_MessageType);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
			invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_JE = declaration.PK;
			AssertEquals(BRJobMessageTypeList.Codes.Export, invoice.JZ_MessageType);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_JE = declaration.PK;
			AssertEquals(BRJobMessageTypeList.Codes.Export, invoice.JZ_MessageType);
		}

		public void TestICurrencyConverterDataProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();

			var invAsProvider = invoice as ICurrencyConverterDataProvider;

			CombineAssertions(() =>
			{
				AssertEquals("JE_MessageType = IMP, Rate Type should be", ExchangeRateType.Customs, invAsProvider.RateType);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
				AssertEquals("JE_MessageType = LIC, Rate Type should be", ExchangeRateType.Customs, invAsProvider.RateType);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
				AssertEquals("JE_MessageType = ISW, Rate Type should be", ExchangeRateType.Customs, invAsProvider.RateType);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
				AssertEquals("JE_MessageType = LPC, Rate Type should be", ExchangeRateType.Customs, invAsProvider.RateType);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				AssertEquals("JE_MessageType = EXP, Rate Type should be", ExchangeRateType.CustomsSecondary, invAsProvider.RateType);
			});
		}

		[TestDate(2023, 1, 1)]
		public void TestExchangeRate()
		{
			ReferenceTestDataHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.NewZealand, 33.16, ZDateTime.Today, ExchangeRateType.Customs);
			ReferenceTestDataHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.NewZealand, 21.41, ZDateTime.Today, ExchangeRateType.CustomsSecondary);

			Factory.Save();

			var decExp = Factory.New<JobDeclaration>();
			decExp.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var invExp = decExp.Invoices.AddNew();
			invExp.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			AssertEquals(21.41m, invExp.JZ_InvoiceCurrExRate);

			var decImp = Factory.New<JobDeclaration>();
			decImp.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invImp = decImp.Invoices.AddNew();
			invImp.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			AssertEquals(33.16m, invImp.JZ_InvoiceCurrExRate);
		}

		public void TestPopulateValuesFromForeignOperator()
		{
			var importer1 = Factory.New<OrgHeader>();
			var importer2 = Factory.New<OrgHeader>();
			var supplier1 = Factory.New<OrgHeader>();
			var supplier2 = Factory.New<OrgHeader>();

			var foreignOperator = Factory.New<CusBRForeignOperator>();
			foreignOperator.BFR_OH_Owner = importer1.PK;
			foreignOperator.BFR_OH_ForeignOperator = supplier1.PK;
			foreignOperator.BFR_AuthorityIdentifier = "OPE_1";
			foreignOperator.BFR_AuthorityVersion = "1";

			var invoice = Factory.New<JobComInvoiceHeader>();
			AssertNull("Declaration is null", invoice.ForeignOperator);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer1.PK;

			invoice.JZ_JE = declaration.PK;
			AssertNull("Importer=importer1, Supplier=null", invoice.ForeignOperator);

			declaration.JE_OH_Importer = importer1.PK;
			invoice.JZ_OH_Supplier = supplier1.PK;
			AssertEquals("Importer=importer1, Supplier=supplier1", foreignOperator, invoice.ForeignOperator);
			AssertEquals("JZ_SupplierAuthorityIdentifier", foreignOperator.BFR_AuthorityIdentifier, invoice.JZ_SupplierAuthorityIdentifier);
			AssertEquals("JZ_SupplierAuthorityVersion", foreignOperator.BFR_AuthorityVersion, invoice.JZ_SupplierAuthorityVersion);

			invoice.JZ_OH_Supplier = supplier2.PK;
			AssertNull("Importer=importer1, Supplier=supplier2", invoice.ForeignOperator);
			AssertEquals("JZ_SupplierAuthorityIdentifier", ZString.Empty, invoice.JZ_SupplierAuthorityIdentifier);
			AssertEquals("JZ_SupplierAuthorityVersion", ZString.Empty, invoice.JZ_SupplierAuthorityVersion);

			declaration.JE_OH_Importer = importer2.PK;
			invoice.JZ_OH_Supplier = supplier1.PK;
			AssertNull("Importer=importer2, Supplier=supplier1", invoice.ForeignOperator);
			AssertEquals("JZ_SupplierAuthorityIdentifier", ZString.Empty, invoice.JZ_SupplierAuthorityIdentifier);
			AssertEquals("JZ_SupplierAuthorityVersion", ZString.Empty, invoice.JZ_SupplierAuthorityVersion);

			declaration.JE_OH_Importer = ZGuid.Empty;
			invoice.JZ_OH_Supplier = supplier1.PK;
			AssertNull("Importer=null, Supplier=supplier1", invoice.ForeignOperator);
		}

		public void TestIsImportOnly()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			Assert("IsImportOnly", !invoice.IsImportOnly);

			invoice.JZ_MessageType = BRJobMessageTypeList.Codes.Import;
			Assert("IsImportOnly", invoice.IsImportOnly);

			var declaration = Factory.New<JobDeclaration>();
			invoice.JZ_JE = declaration.PK;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			Assert("IsImportOnly", invoice.IsImportOnly);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			Assert("IsImportOnly", !invoice.IsImportOnly);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			Assert("IsImportOnly", !invoice.IsImportOnly);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			Assert("IsImportOnly", !invoice.IsImportOnly);
		}

		public void TestAdditionalTermsMaxLength()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			AssertEquals(250, invoice.JZ_AdditionalTermsInfo.MaxLength);
		}

		#region Implementation

		public override string GetLocalCurrencyCode() => Core.Constants.CurrencyCodes.Brazil;

		protected override string GetDiscountChargeCodeForTest() => ImportCustomsChargeTypeList.Codes.OtherDeductionsCustomsValue;

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			return invoice;
		}

		protected override Type ExpectedTypeOfGroupCharges => typeof(JobComInvApportionedChargeCollection<InvoiceApportionCharge>);

		protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceCharge>);

		protected override Hashtable ExpectedDocAddressTypes
		{
			get
			{
				if (fExpectedDocAddressTypes == null)
				{
					fExpectedDocAddressTypes = base.ExpectedDocAddressTypes;
					fExpectedDocAddressTypes.Add(DocAddressTypes.Codes.Supplier, DocAddressType.SupplierDocumentaryAddress);
				}
				return fExpectedDocAddressTypes;
			}
		}
		Hashtable fExpectedDocAddressTypes;

		#endregion
	}
}
