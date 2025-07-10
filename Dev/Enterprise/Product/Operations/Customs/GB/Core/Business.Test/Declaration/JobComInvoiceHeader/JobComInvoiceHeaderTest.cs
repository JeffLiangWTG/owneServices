using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceHeader))]
	sealed class JobComInvoiceHeaderTest : EU.Business.Declaration.Testing.JobComInvoiceHeaderTest<JobDeclaration, JobComInvoiceHeader, JobComInvoiceLine>
	{
		public override void TestCFRCalculationWithFreightAdjustedFlag()
		{
			var dec = (JobDeclaration)GetNewDeclaration();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			dec.JE_ExportDate = new ZDateTime(2004, 10, 30);
			var invoice = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			invoice.JZ_InvoiceAmount = 10500m;
			invoice.JZ_RX_NKInvoice_Currency = dec.LocalCurrencyCode;

			var oFT = invoice.Charges.AddNew();
			oFT.J7_ChargeType = OverseasFreightCode;
			oFT.J7_Amount = 500m;
			oFT.J7_RX_NKCurrency = dec.LocalCurrencyCode;
			oFT.J7_IsDutiable = true;
			oFT.J7_IsIncludedInITOT = false;

			var adjustedOFT = invoice.Charges.AddNew();
			adjustedOFT.J7_ChargeType = OverseasFreightCode;
			adjustedOFT.J7_Amount = 300m;
			adjustedOFT.J7_RX_NKCurrency = dec.LocalCurrencyCode;
			adjustedOFT.J7_AdjustedCharge = true;
			adjustedOFT.J7_IsIncludedInITOT = false;

			AssertEquals("InvoiceLineTotal should only take charges without adjusted on", 10000m, invoice.InvoiceLineTotal);
			AssertEquals("FOB", 10000m, invoice.JZ_Calc_FOBAmount);
			AssertEquals("CIF", 10000m, invoice.JZ_Calc_CIFAmount);

			oFT.J7_IsIncludedInITOT = true;
			adjustedOFT.J7_IsIncludedInITOT = true;

			AssertEquals("InvoiceLineTotal should only take charges without adjusted on", 10500m, invoice.InvoiceLineTotal);
			AssertEquals("FOB", 10500m, invoice.JZ_Calc_FOBAmount);
			AssertEquals("CIF", 10500m, invoice.JZ_Calc_CIFAmount);
		}

		public void TestZG_HouseSplitReference_SetDeclarationEmpty()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			dec.ZG_HouseSplitReference = "AA";
			invoice.ZG_HouseSplitReference = "BB";
			AssertEquals(ZString.Empty, dec.ZG_HouseSplitReference);
		}

		public void TestGroupCharges()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			AssertType<JobComInvApportionedChargeCollection<InvoiceApportionCharge>>(invoice.GroupCharges);
		}

		public void TestCharges()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			AssertType<InvoiceChargeCollection<InvoiceCharge>>(invoice.Charges);
		}

		public void TestInvoiceNumberChangeUpdatesSupportingDocuments()
		{
			JobComInvoiceHeader invoice = ((JobDeclaration)declaration).Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "123";
			AssertEquals("123", invoice.PreviousDocuments[0].CSI_ReferenceNumber);
			AssertEquals(PreviousDocumentClassList.Codes.PreviousDocument, invoice.PreviousDocuments[0].CSI_SubType);
			AssertEquals("380", invoice.PreviousDocuments[0].CSI_Code); //CommercialInvoice
			invoice.JZ_InvoiceNumber = "124";
			AssertEquals("124", invoice.PreviousDocuments[0].CSI_ReferenceNumber);
		}

		public void TestProcedureAndWarehouseFunctionality()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedureA = helper.CreateRefCusProcedure(currentCountry, "A", "44", "44", "444", "Four", "EXP", group: "EFD");
			procedureA.ZZ6_IntoWarehouse = Universal.WarehouseMoveStatus.Codes.Yes;
			var procedureB = helper.CreateRefCusProcedure(currentCountry, "A", "55", "55", "555", "Five", "EXP", group: "EFD");
			procedureB.ZZ6_OutOfWarehouse = Universal.WarehouseMoveStatus.Codes.Yes;
			var procedureC = helper.CreateRefCusProcedure(currentCountry, "A", "66", "66", "666", "Six", "EXP", group: "EFD");
			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsWarehouseClient = true;
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			dec.CusEntryInstruction.CEI_Style = "EFD";

			var invoiceHeader = dec.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			dec.WarehouseDocAddress.E2_OA_Address = org.MainAddress.PK;
			AssertEquals(ZGuid.Empty, dec.CusEntryInstruction.CEI_OA_Warehouse);

			invoiceLine.JI_Procedure = procedureA.FullCodeCurrentPlusPreviousPlusConcession;
			AssertEquals(dec.WarehouseDocAddress.Address.PK, dec.CusEntryInstruction.CEI_OA_Warehouse);

			dec.CusEntryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
			invoiceLine.JI_Procedure = procedureB.FullCodeCurrentPlusPreviousPlusConcession;
			AssertEquals(dec.WarehouseDocAddress.Address.PK, dec.CusEntryInstruction.CEI_OA_Warehouse);

			dec.CusEntryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
			invoiceLine.JI_Procedure = procedureC.FullCodeCurrentPlusPreviousPlusConcession;
			AssertEquals(ZGuid.Empty, dec.CusEntryInstruction.CEI_OA_Warehouse);

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_Procedure = procedureA.FullCodeCurrentPlusPreviousPlusConcession;
			AssertEquals(dec.WarehouseDocAddress.Address.PK, dec.CusEntryInstruction.CEI_OA_Warehouse);

			invoiceLine2.JI_Procedure = procedureC.FullCodeCurrentPlusPreviousPlusConcession;
			AssertEquals(ZGuid.Empty, dec.CusEntryInstruction.CEI_OA_Warehouse);

			invoiceLine2.Delete();
			dec.WarehouseDocAddress.E2_OA_Address = ZGuid.Empty;
			invoiceLine.JI_Procedure = procedureA.FullCodeCurrentPlusPreviousPlusConcession;
			dec.WarehouseDocAddress.E2_OA_Address = org.MainAddress.PK;
			AssertEquals(dec.WarehouseDocAddress.Address.PK, dec.CusEntryInstruction.CEI_OA_Warehouse);

			invoiceLine.Delete();
			AssertEquals("CEI_OA_Warehouse is not cleared by removing an invoice line", dec.WarehouseDocAddress.Address.PK, dec.CusEntryInstruction.CEI_OA_Warehouse);

			invoiceHeader.InvoiceLines.AddNew();
			AssertEquals("CEI_OA_Warehouse is not cleared by adding an invoice line", dec.WarehouseDocAddress.Address.PK, dec.CusEntryInstruction.CEI_OA_Warehouse);
		}

		public override void TestChargeTypeList()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			Customs.Business.ICommonInvoice commonInvoice = dec.TopGroupInvoice;
			var chargeTypeList1 = commonInvoice.ChargeTypeList;
			var chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));

			var customsChargeTypeList = new Customs.Business.CustomsChargeTypeList();
			customsChargeTypeList.RemoveCode(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight);
			customsChargeTypeList.RemoveCode(Customs.Business.CustomsChargeTypeList.Codes.Commission);
			customsChargeTypeList.RemoveCode(Customs.Business.CustomsChargeTypeList.Codes.ExWorks);
			customsChargeTypeList.RemoveCode(Customs.Business.CustomsChargeTypeList.Codes.ForeignInlandFreight);
			customsChargeTypeList.RemoveCode(Customs.Business.CustomsChargeTypeList.Codes.LandingCharges);
			customsChargeTypeList.RemoveCode(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges);
			customsChargeTypeList.RemoveCode(Customs.Business.CustomsChargeTypeList.Codes.PackingCost);
			customsChargeTypeList.AddPair(ChargesProvider.OverseasFreightInChiefTerminologyCode_AWB, ChargesProvider.ChargeDescriptionsForGB.OverseasFreight);
			customsChargeTypeList.AddPair(ChargesProvider.AirFreightCode, ChargesProvider.AirFreightDesc);
			customsChargeTypeList.AddPair(ChargesProvider.VATAdjustmentCode, ChargesProvider.VATAdjustmentDesc);
			customsChargeTypeList.Sort();
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);
		}

		public void TestInvoiceAndGroupInvoiceChargeTypeListsAreTheSame()
		{
			var dec = Factory.New<JobDeclaration>();
			Customs.Business.ICommonInvoice invoice = dec.Invoices.AddNew();
			Customs.Business.ICommonInvoice commonInvoice = dec.TopGroupInvoice;
			var chargeTypeList1 = invoice.ChargeTypeList;
			var chargeTypeList2 = commonInvoice.ChargeTypeList;

			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
		}

		public override string GetLocalCurrencyCode() => Core.Constants.CurrencyCodes.UnitedKingdom;

		public void TestSupportingDocumentsCorrectType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			AssertType<SupportingDocumentCollection>(invoice.SupportingDocuments);
		}

		public void TestAdditionalInfoCorrectType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			AssertType<AdditionalInfoCollection>(invoice.AdditionalInfos);
		}

		public void TestPreviousDocumentsCorrectType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			AssertType<PreviousDocumentCollection>(invoice.PreviousDocuments);
		}

		public void TestUpdateWhenAnInvoiceIsLinkedToADeclaration()
		{
			var standalone = Factory.New<JobComInvoiceHeader>();
			var standaloneLine = standalone.InvoiceLines.AddNew();
			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationType = "ABC";
			dec.Invoices.Add(standalone);
			AssertEquals(dec.CusEntryInstruction.PK, standaloneLine.JI_CEI);
			dec.Invoices.RemoveFromRelationship(standalone);
			AssertEquals(ZGuid.Empty, standaloneLine.JI_CEI);
		}

		public void TestJobComInvoiceLines()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			AssertType<JobComInvoiceLineViewCollection>(invoice.JobComInvoiceLines);
		}

		public new void TestInvoiceLines()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			AssertType<JobComInvoiceLineViewCollection>(invoice.InvoiceLines);
		}

		public void TestZG_HouseSplitReference_ReadOnly()
		{
			CombineAssertions(() =>
			{
				var invoice = Factory.New<JobComInvoiceHeader>();
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				declaration.Invoices.Add(invoice);
				AssertEquals("CDS", true, invoice.ZG_HouseSplitReferenceInfo.ReadOnly);
			});
		}

		public void TestAddingSupportingDocumentAutomaticallyEnabled()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			declaration.Invoices.Add(invoice);

			using (GBCustomsDataRegistry.Instance.N935_AddSupportingDocToInvoiceHeader.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				AssertEquals("AddingSupportingDocumentAutomaticallyEnabled for GB", false, invoice.AddingSupportingDocumentAutomaticallyEnabled);

				using (GBCustomsDataRegistry.Instance.N935_AddSupportingDocToInvoiceHeader.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertEquals("AddingSupportingDocumentAutomaticallyEnabled for GB", true, invoice.AddingSupportingDocumentAutomaticallyEnabled);
				}
			}
		}

		public void TestAddDefaultSupportingDocumentIfNecessary()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var supplierAddress = supplier.MainAddress;
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = supplier.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			supplierAddress.OA_RN_NKCountryCode = "IT";
			invoice.JZ_InvoiceDate = ZDateTime.Today;

			CombineAssertions(() =>
			{
				declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				declaration.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;
				invoice.JZ_InvoiceNumber = "3";
				AssertEquals("Supporting Documents count", 0, invoice.SupportingDocuments.Count);
				var defaultSupportingDocument = invoice.SupportingDocuments.Cast<SupportingDocument>().SingleOrDefault(x => x.CSI_Code == SupportingDocumentTypes.N935);
				AssertNull(defaultSupportingDocument);

				declaration.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;
				invoice.JZ_InvoiceNumber = "4";
				AssertEquals("Supporting Documents count", 1, invoice.SupportingDocuments.Count);
				defaultSupportingDocument = invoice.SupportingDocuments.Cast<SupportingDocument>().SingleOrDefault(x => x.CSI_Code == SupportingDocumentTypes.N935);
				AssertNotNull(defaultSupportingDocument);
			});
		}

		public void TestAdding9WKSDoc_ON_Import_JZ_RX_NKInvoice_Currency_Change()
		{
			var usdCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates);
			var audCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.Australia);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_DeclarationReference = "REF/A95";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice1 = declaration.Invoices.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoice3 = declaration.Invoices.AddNew();

			CombineAssertions(() =>
			{
				invoice1.JZ_RX_NKInvoice_Currency = usdCurrency.RX_Code;
				AssertEquals("Supporting Documents count", 0, invoice1.SupportingDocuments.Count);
				invoice2.JZ_RX_NKInvoice_Currency = usdCurrency.RX_Code;
				AssertEquals("Supporting Documents count", 0, invoice2.SupportingDocuments.Count);
				invoice3.JZ_RX_NKInvoice_Currency = audCurrency.RX_Code;
				AssertEquals("Supporting Documents count", 1, invoice3.SupportingDocuments.Count);
				AssertEquals("Supporting Documents count", 1, invoice2.SupportingDocuments.Count);
				AssertEquals("Supporting Documents count", 1, invoice1.SupportingDocuments.Count);

				AssertEquals("Expecting Description for invoice1/SD", "SEE ATTACHED WORKSHEET REF/A95", invoice1.SupportingDocuments[0].CSI_Description);
				AssertEquals("Expecting Description for invoice2/SD", "SEE ATTACHED WORKSHEET REF/A95", invoice2.SupportingDocuments[0].CSI_Description);
				AssertEquals("Expecting Description for invoice3/SD", "SEE ATTACHED WORKSHEET REF/A95", invoice3.SupportingDocuments[0].CSI_Description);
			});
		}

		public void TestNotAdding9WKSDoc_ON_Export_JZ_RX_NKInvoice_Currency_Change()
		{
			var usdCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates);
			var audCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.Australia);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var invoice1 = declaration.Invoices.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoice3 = declaration.Invoices.AddNew();

			CombineAssertions(() =>
			{
				invoice1.JZ_RX_NKInvoice_Currency = usdCurrency.RX_Code;
				AssertEquals("Supporting Documents count", 0, invoice1.SupportingDocuments.Count);
				invoice2.JZ_RX_NKInvoice_Currency = usdCurrency.RX_Code;
				AssertEquals("Supporting Documents count", 0, invoice2.SupportingDocuments.Count);
				invoice3.JZ_RX_NKInvoice_Currency = audCurrency.RX_Code;
				AssertEquals("Supporting Documents count", 0, invoice3.SupportingDocuments.Count);
				AssertEquals("Supporting Documents count", 0, invoice2.SupportingDocuments.Count);
				AssertEquals("Supporting Documents count", 0, invoice1.SupportingDocuments.Count);
			});
		}

		public void TestFakeDeclarationApplicationCodeForStandaloneInvoice()
		{
			var registryItem = Registry.GBCustomsDataRegistry.Instance.CDSEnabledForExports;
			registryItem.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);

			var expectedCDSChargeTypeList = "ADD, ADV, AFT, BCM, CBR, CEA, COM, CPA, DED, DIS, EDA, INT, IPO, MAC, MCP, OFT, ONS, PSR, RLF, TDM";

			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			var charge = invoice.Charges.AddNew();
			var fakeDeclaration = new FakeDeclarationCreatorForInvoice(invoice);
			AssertEquals("Application Code should always be CDS for imports", Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, ((JobDeclaration)fakeDeclaration.HeaderData).JE_ApplicationCode);
			var chargeCodes = charge.Lookups.ChargeTypeList;
			AssertEquals(expectedCDSChargeTypeList, chargeCodes.CodesAsString);

			invoice.JZ_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Application Code should be CDS for exports", Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, ((JobDeclaration)fakeDeclaration.HeaderData).JE_ApplicationCode);
			chargeCodes = charge.Lookups.ChargeTypeList;
			AssertEquals(expectedCDSChargeTypeList, chargeCodes.CodesAsString);

			registryItem.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			fakeDeclaration = new FakeDeclarationCreatorForInvoice(invoice);
			AssertEquals("Application Code should be CDS when enabled for exports", Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, ((JobDeclaration)fakeDeclaration.HeaderData).JE_ApplicationCode);
			chargeCodes = charge.Lookups.ChargeTypeList;
			AssertEquals(expectedCDSChargeTypeList, chargeCodes.CodesAsString);
		}

		[TestDate(2024, 3, 1)]
		public void TestEffectiveValuationDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			AssertEquals(new ZDate(2024, 3, 1), invoice.EffectiveValuationDate);

			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = "H1";
			entryInstruction1.CEI_SubStyle = "A";
			entryInstruction1.CEI_DateForDuty = new ZDate(2024, 3, 10);
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Style = "H2";
			entryInstruction2.CEI_SubStyle = "D";
			entryInstruction2.CEI_DateForDuty = new ZDate(2024, 3, 14);

			invoiceLine1.JI_CEI = entryInstruction1.PK;
			invoiceLine2.JI_CEI = entryInstruction2.PK;
			AssertEquals(new ZDate(2024, 3, 10), invoice.EffectiveValuationDate);
		}

		#region Base tests
		public new void TestZGFieldsCaption()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			CombineAssertions("Payment Properties Caption (CHIEF)", () =>
			{
				AssertCaption(invoiceHeader.ZG_TransportChargesMethodOfPaymentInfo, JobDeclaration.MultipleKeyChief, "MoP", "[S29] Transport charges MoP");
				AssertCaption(invoiceHeader.ZG_AgreedPlaceCodeInfo, JobDeclaration.MultipleKeyChief, "Incoterm Place", "Incoterm Place Code");
			});
			CombineAssertions("Payment Properties Caption (CDS)", () =>
			{
				AssertCaption(invoiceHeader.ZG_TransportChargesMethodOfPaymentInfo, JobDeclaration.MultipleKeyCdsImport, "MoP", "[UCC 4/2] Transport charges MoP");
				AssertCaption(invoiceHeader.ZG_AgreedPlaceCodeInfo, JobDeclaration.MultipleKeyCdsImport, "Incoterm Place", "Incoterm Place Code");
			});
		}

		void AssertCaption(ZPropertyInfo info, string multipleKey, string shortCaption, string caption)
		{
			var propertyData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(info, multipleKey);
			AssertEquals($"{info.Name} Caption", caption, propertyData.Caption);
			AssertEquals($"{info.Name} ShortCaption", shortCaption, propertyData.ShortCaption);
		}
		#endregion

		protected override BaseJobDeclaration GetNewDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			return declaration;
		}

		protected override IReadOnlyList<(string messageType, string document)> DefaultInvoiceDocuments =>
			new[]
			{
				(EUJobMessageTypeList.Codes.Import, EU.Business.UniversalReferenceConstants.SupportingDocumentTypes.N935)
			};

		protected override void AssertDefaultDocumentWhenApplicationCodeChanges(EU.Business.Declaration.JobDeclaration declaration, EU.Business.Declaration.JobComInvoiceHeader invoice, string messageType, string defaultInvoiceDocumentCode)
		{
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			invoice.JZ_InvoiceNumber = "6";
			AssertEquals($"{messageType}, In CDS, we add {defaultInvoiceDocumentCode} document without considering the application code.", 1, invoice.SupportingDocuments.Count);
		}

		protected override string OverseasFreightCode => ChargesProvider.OverseasFreightInChiefTerminologyCode_AWB;

		protected override Type ExpectedTypeOfGroupCharges => typeof(JobComInvApportionedChargeCollection<InvoiceApportionCharge>);

		protected override Type ExpectedTypeOfCharges => typeof(InvoiceChargeCollection<InvoiceCharge>);

		protected override string GetDiscountChargeDescriptionForTest() => ChargesProvider.ChargeDescriptionsForGB.Discount;

		protected override EU.Business.Declaration.JobDeclaration GetNewDeclarationForTesting()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			return declaration;
		}
	}
}
