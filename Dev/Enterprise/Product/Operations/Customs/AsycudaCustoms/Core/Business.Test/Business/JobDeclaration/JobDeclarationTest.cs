using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(JobDeclaration))]
	sealed class JobDeclarationTest : BaseJobDeclarationTest<JobDeclaration>
	{
		public void TestCorrectlySetupInterfaceIJobDeclaration()
		{
			AssertType<JobDeclaration>(Factory.New<Integration.Customs.AsycudaCustoms.IJobDeclaration>());
		}

		public void TestIsRiskManagementEnabled_True()
		{
			AssertIsRiskManagementEnabled(true);
		}

		public void TestIsRiskManagementEnabled_False()
		{
			AssertIsRiskManagementEnabled(false);
		}

		void AssertIsRiskManagementEnabled(bool isRiskEnabled)
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.Risk, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZDateTime.Today, isRiskEnabled))
			{
				var declaration = Factory.New<JobDeclaration>();
				AssertEquals(isRiskEnabled, declaration.IsRiskManagementEnabled);
			}
		}

		public void TestJE_PaymentMethod_Caption()
		{
			AssertEquals("Payment Method", DataBoundResourceStrings.GetDataForProperty(Factory.New<JobDeclaration>().JE_PaymentMethodInfo).Caption);
		}

		public void TestJE_TransportModeInland_Caption()
		{
			AssertEquals("Inland M.O.T", DataBoundResourceStrings.GetDataForProperty(Factory.New<JobDeclaration>().JE_TransportModeInlandInfo).Caption);
		}

		public void TestJE_OA_Representative_Caption()
		{
			AssertEquals("Representative", DataBoundResourceStrings.GetDataForProperty(Factory.New<JobDeclaration>().JE_OA_RepresentativeInfo).Caption);
		}

		public void TestJE_OA_DeclarantAddress_Caption()
		{
			AssertEquals("Declarant", DataBoundResourceStrings.GetDataForProperty(Factory.New<JobDeclaration>().JE_OA_DeclarantAddressInfo).Caption);
		}

		public void TestDocumentSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<JobDeclarationDocumentSupporter>(declaration.DocumentSupporter);
		}

		protected override List<string> CountryCodesForIsReciprocalRatesTest => new List<string> { Core.Constants.CountryCodes.Botswana, Core.Constants.CountryCodes.Lesotho, Core.Constants.CountryCodes.Namibia, Core.Constants.CountryCodes.Swaziland };

		public override void TestIsAutoUpdateBondedWarehouseEnabled()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			Assert(declaration.IsAutoUpdateBondedWarehouseEnabled);
		}

		public void TestJE_OH_DutyPayer_Caption()
		{
			AssertEquals("Duty Payer", DataBoundResourceStrings.GetDataForProperty(Factory.New<JobDeclaration>().JE_OH_DutyPayerInfo).Caption);
		}

		public void TestJE_ManifestNumber_Caption()
		{
			AssertEquals("Manifest Number", DataBoundResourceStrings.GetDataForProperty(Factory.New<JobDeclaration>().JE_ManifestNumberInfo).Caption);
		}

		public void TestInventorySelectionHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<InventorySelectionHeader>(declaration.InventorySelectionHeader);
		}

		public void TestIsImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			Assert(declaration.IsImport);
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			Assert(declaration.IsImport);
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			Assert(!declaration.IsImport);
		}

		public void TestBondedWarehouseProperties()
		{
			var helper = new WhsDataTestHelper(Factory);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_OH_Importer = helper.Importer.PK;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = helper.InwardCusProcedure.ZZ6_Group;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			AssertEquals("HasInwardInvoiceLine", false, declaration.HasInwardInvoiceLine);
			AssertEquals("declaration.IsInwardBondedWarehousingEnabled", false, declaration.IsInwardBondedWarehousingEnabled);
			AssertEquals("entry.IsInwardBondedWarehousingEnabled", false, entry.IsInwardBondedWarehousingEnabled);
			AssertEquals("HasOutwardInvoiceLine", false, declaration.HasOutwardInvoiceLine);
			AssertEquals("IsOutwardBondedWarehousingEnabled", false, declaration.IsOutwardBondedWarehousingEnabled);
			AssertEquals("entry.IsOutwardBondedWarehousingEnabled", false, entry.IsOutwardBondedWarehousingEnabled);
			AssertEquals("ShouldUpdateOutwardLinesWithInventoryDetails", false, declaration.ShouldUpdateOutwardLinesWithInventoryDetails);
			AssertEquals("IsInventorySelectionEnabled", false, declaration.IsInventorySelectionEnabled);
			AssertEquals("entryInstruction.IsInventorySelectionEnabled", false, entryInstruction.IsInventorySelectionEnabled);
			invoiceLine.JI_Procedure = helper.InwardCusProcedure.ZZ6_ProcedureCode + helper.InwardCusProcedure.ZZ6_PreviousProcedureCode;
			AssertEquals("HasInwardInvoiceLine", true, declaration.HasInwardInvoiceLine);
			AssertEquals("SupportsBondedWarehousing = false in Asycuda", false, declaration.IsInwardBondedWarehousingEnabled);
			AssertEquals("entry.IsInwardBondedWarehousingEnabled", true, entry.IsInwardBondedWarehousingEnabled);
			AssertEquals("HasOutwardInvoiceLine", false, declaration.HasOutwardInvoiceLine);
			AssertEquals("IsOutwardBondedWarehousingEnabled", false, declaration.IsOutwardBondedWarehousingEnabled);
			AssertEquals("entry.IsOutwardBondedWarehousingEnabled", false, entry.IsOutwardBondedWarehousingEnabled);
			AssertEquals("ShouldUpdateOutwardLinesWithInventoryDetails", false, declaration.ShouldUpdateOutwardLinesWithInventoryDetails);
			AssertEquals("IsInventorySelectionEnabled", false, declaration.IsInventorySelectionEnabled);
			AssertEquals("entryInstruction.IsInventorySelectionEnabled", false, entryInstruction.IsInventorySelectionEnabled);
			declaration.JE_MessageType = "EXP";
			declaration.JE_OH_Supplier = helper.Importer.PK;
			AssertEquals("HasInwardInvoiceLine", true, declaration.HasInwardInvoiceLine);
			AssertEquals("IsInwardBondedWarehousingEnabled", false, declaration.IsInwardBondedWarehousingEnabled);
			AssertEquals("entry.IsInwardBondedWarehousingEnabled", true, entry.IsInwardBondedWarehousingEnabled);
			AssertEquals("HasOutwardInvoiceLine", false, declaration.HasOutwardInvoiceLine);
			AssertEquals("IsOutwardBondedWarehousingEnabled", false, declaration.IsOutwardBondedWarehousingEnabled);
			AssertEquals("entry.IsOutwardBondedWarehousingEnabled", false, entry.IsOutwardBondedWarehousingEnabled);
			AssertEquals("ShouldUpdateOutwardLinesWithInventoryDetails", true, declaration.ShouldUpdateOutwardLinesWithInventoryDetails);
			AssertEquals("IsInventorySelectionEnabled", true, declaration.IsInventorySelectionEnabled);
			AssertEquals("entryInstruction.IsInventorySelectionEnabled", true, entryInstruction.IsInventorySelectionEnabled);
			declaration.JE_MessageType = "EXW";
			AssertEquals("HasInwardInvoiceLine", true, declaration.HasInwardInvoiceLine);
			AssertEquals("IsInwardBondedWarehousingEnabled", false, declaration.IsInwardBondedWarehousingEnabled);
			AssertEquals("entry.IsInwardBondedWarehousingEnabled", true, entry.IsInwardBondedWarehousingEnabled);
			AssertEquals("HasOutwardInvoiceLine", false, declaration.HasOutwardInvoiceLine);
			AssertEquals("IsOutwardBondedWarehousingEnabled", false, declaration.IsOutwardBondedWarehousingEnabled);
			AssertEquals("entry.IsOutwardBondedWarehousingEnabled", false, entry.IsOutwardBondedWarehousingEnabled);
			AssertEquals("ShouldUpdateOutwardLinesWithInventoryDetails", true, declaration.ShouldUpdateOutwardLinesWithInventoryDetails);
			AssertEquals("IsInventorySelectionEnabled", true, declaration.IsInventorySelectionEnabled);
			AssertEquals("entryInstruction.IsInventorySelectionEnabled", true, entryInstruction.IsInventorySelectionEnabled);
			entryInstruction.CEI_Style = helper.OutwardCusProcedure.ZZ6_Group;
			invoiceLine.JI_Procedure = helper.OutwardCusProcedure.ZZ6_ProcedureCode + helper.OutwardCusProcedure.ZZ6_PreviousProcedureCode;
			AssertEquals("HasInwardInvoiceLine", false, declaration.HasInwardInvoiceLine);
			AssertEquals("IsInwardBondedWarehousingEnabled", false, declaration.IsInwardBondedWarehousingEnabled);
			AssertEquals("entry.IsInwardBondedWarehousingEnabled", false, entry.IsInwardBondedWarehousingEnabled);
			AssertEquals("HasOutwardInvoiceLine", true, declaration.HasOutwardInvoiceLine);
			AssertEquals("IsOutwardBondedWarehousingEnabled", false, declaration.IsOutwardBondedWarehousingEnabled);
			AssertEquals("entry.IsOutwardBondedWarehousingEnabled", true, entry.IsOutwardBondedWarehousingEnabled);
			AssertEquals("ShouldUpdateOutwardLinesWithInventoryDetails", true, declaration.ShouldUpdateOutwardLinesWithInventoryDetails);
			AssertEquals("IsInventorySelectionEnabled", true, declaration.IsInventorySelectionEnabled);
			AssertEquals("entryInstruction.IsInventorySelectionEnabled", true, entryInstruction.IsInventorySelectionEnabled);
		}

		public void TestBondedWarehouseAutomationForImportJobs()
		{
			AssertBondedWarehouseAutomationForNonExpJobs(JobMessageTypeList.Codes.Import);
		}

		public void TestBondedWarehouseAutomationForExWarehouseJobs()
		{
			AssertBondedWarehouseAutomationForNonExpJobs(JobMessageTypeList.Codes.ExWarehouse);
		}

		public void TestBondedWarehouseAutomationForMiscellaneousCustomsJobs()
		{
			AssertBondedWarehouseAutomationForNonExpJobs(JobMessageTypeList.Codes.MiscellaneousCustoms);
		}

		public void TestBondedWarehouseAutomationForExpJobs()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			dec.JE_OH_Importer = importer.PK;
			dec.JE_OH_Supplier = supplier.PK;
			AssertEquals("BondedWarehouseAutomation is turned off", false, dec.IsBondedWarehouseAutomationOn);
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			AssertEquals($"BondedWarehouseAutomation is turned off when Importer Use Bonded Warehouse Automation for 'EXP' jobs", false, dec.IsBondedWarehouseAutomationOn);
			importer.CompanyData.OB_IMUsedBondedWhs = false;
			supplier.CompanyData.OB_IMUsedBondedWhs = true;
			AssertEquals($"BondedWarehouseAutomation is turned on when Supplier Use Bonded Warehouse Automation for 'EXP' jobs", true, dec.IsBondedWarehouseAutomationOn);
			supplier.CompanyData.OB_IMUsedBondedWhs = false;
			AssertToWarehouseAndFromWarehouse(dec);
		}

		void AssertBondedWarehouseAutomationForNonExpJobs(string messageType)
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_MessageType = messageType;
			dec.JE_OH_Importer = importer.PK;
			dec.JE_OH_Supplier = supplier.PK;
			AssertEquals("BondedWarehouseAutomation is turned off", false, dec.IsBondedWarehouseAutomationOn);
			supplier.CompanyData.OB_IMUsedBondedWhs = true;
			AssertEquals($"BondedWarehouseAutomation is turned off when Supplier Use Bonded Warehouse Automation for '{messageType}' jobs", false, dec.IsBondedWarehouseAutomationOn);
			supplier.CompanyData.OB_IMUsedBondedWhs = false;
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			AssertEquals($"BondedWarehouseAutomation is turned on when Importer Use Bonded Warehouse Automation for '{messageType}' jobs", true, dec.IsBondedWarehouseAutomationOn);
			importer.CompanyData.OB_IMUsedBondedWhs = false;
			AssertToWarehouseAndFromWarehouse(dec);
		}

		void AssertToWarehouseAndFromWarehouse(JobDeclaration dec)
		{
			var fromWarehouse = Factory.NewWithValidTestData<OrgHeader>();
			var fromWarehouseAddress = fromWarehouse.Addresses.AddNew();
			fromWarehouseAddress.OA_Address1 = "Address 1";
			var toWarehouse = Factory.NewWithValidTestData<OrgHeader>();
			var toWarehouseAddress = toWarehouse.Addresses.AddNew();
			toWarehouseAddress.OA_Address1 = "Address 2";
			var messageType = dec.JE_MessageType;
			var entry = (CusEntryInstruction)dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entry.CEI_OA_Warehouse = fromWarehouseAddress.PK;
			fromWarehouse.CompanyData.OB_IMUsedBondedWhs = true;
			AssertEquals($"BondedWarehouseAutomation is turned on when From Warehouse Use Bonded Warehouse Automation for '{messageType}' jobs", true, dec.IsBondedWarehouseAutomationOn);
			fromWarehouse.CompanyData.OB_IMUsedBondedWhs = false;
			entry.CEI_OA_Warehouse2 = toWarehouseAddress.PK;
			toWarehouse.CompanyData.OB_IMUsedBondedWhs = true;
			AssertEquals($"BondedWarehouseAutomation is turned on when To Warehouse Use Bonded Warehouse Automation for '{messageType}' jobs", true, dec.IsBondedWarehouseAutomationOn);
		}

		public void TestJE_ManifestNumberMaxLength()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("Max length of JE_ManifestNumber should be 28", 28, declaration.JE_ManifestNumberInfo.MaxLength);
		}

		public void TestJE_CustomsOffice_MaxLength()
		{
			var branch1 = CreateCompanyAndBranch("A!1", Core.Constants.CountryCodes.Afghanistan);
			var branch2 = CreateCompanyAndBranch("A!2", Core.Constants.CountryCodes.Bahamas);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var defaultCodeType = helper.CreateNewOrGetExistingCusCodeType("CUSOF", "Customs Office", Core.Constants.CountryCodes.Bahamas, 8);
			var afCodeType = helper.CreateNewOrGetExistingCusCodeType("CUSOF", "Customs Office", Core.Constants.CountryCodes.Afghanistan, 6);
			Factory.Save();
			var dec = Factory.New<JobDeclaration>();
			dec.JE_GB = branch1.PK;
			AssertEquals("Use ZZK_Maxlength", 6, dec.JE_CustomsOfficeInfo.MaxLength);
			dec.JE_GB = branch2.PK;
			AssertEquals("Use default ZZK_Maxlength", 8, dec.JE_CustomsOfficeInfo.MaxLength);
			defaultCodeType.Delete();
			afCodeType.Delete();
			Factory.Save();
			dec = new BusinessObjectFactory().New<JobDeclaration>();
			dec.JE_GB = branch1.PK;
			AssertEquals("Use schema maxlength if no ZZK_Maxlength", JobDeclaration.Schema.JE_CustomsOfficeMaxLength, dec.JE_CustomsOfficeInfo.MaxLength);
			dec.JE_GB = branch2.PK;
			AssertEquals("Use schema maxlength if no ZZK_Maxlength", JobDeclaration.Schema.JE_CustomsOfficeMaxLength, dec.JE_CustomsOfficeInfo.MaxLength);
		}

		[UseSnapshotProtection]
		public void TestAllocateAllBGMReferences()
		{
			BGMReferenceCounterProvider.Instance.ResetValue();

			var factory1 = new BusinessObjectFactory { RefreshEnabled = false };
			var declaration = factory1.New<JobDeclaration>();
			AssertEquals("", declaration.JE_DeclarationReference);
			var header1 = declaration.ActiveEntryHeaders.AddNew();
			var header2 = declaration.ActiveEntryHeaders.AddNew();
			declaration.AllocateAllBGMReferences();
			AssertEquals("B00001000", declaration.JE_DeclarationReference);
			AssertEquals("B00001000/1", header1.CH_BGMReference);
			AssertEquals("B00001000/2", header2.CH_BGMReference);
			factory1.Save();
			header2.Delete();
			factory1.Save();
			var header3 = declaration.ActiveEntryHeaders.AddNew();
			var header4 = declaration.ActiveEntryHeaders.AddNew();
			header3.Delete();
			factory1.Save();
			AssertEquals(2, declaration.ActiveEntryHeaders.Count);
			AssertEquals("B00001000/3", header4.CH_BGMReference);
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var declarationInNewFactory = newFactory.Load<JobDeclaration>(declaration.PK);
			AssertEquals("B00001000", declarationInNewFactory.JE_DeclarationReference);
			declarationInNewFactory.AllocateAllBGMReferences();
			AssertEquals("B00001000/1", header1.CH_BGMReference);
			AssertEquals("B00001000/3", header4.CH_BGMReference);
			var newFactory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var declaration2 = newFactory2.New<JobDeclaration>();
			declaration2.JE_DeclarationReference = "B00001002";
			var header5 = declaration2.ActiveEntryHeaders.AddNew();
			declaration2.AllocateAllBGMReferences();
			AssertEquals("B00001002/1", header5.CH_BGMReference);
			newFactory2.Save();
		}

		[UseSnapshotProtection]
		public void TestAllocateAllBGMReferences_Deadlock1()
		{
			BGMReferenceCounterProvider.Instance.ResetValue();

			var sw = System.Diagnostics.Stopwatch.StartNew();
			var factory = new BusinessObjectFactory();
			using (var mutex = new ZGlobalMutex(MutexIDs.CustomsTransactionIDAllocation, "SomeUnrelatedMutexLikeMutexForMerge"))
			{
				AssertEquals(true, mutex.Lock());
				var declaration = factory.New<JobDeclaration>();
				var header = declaration.ActiveEntryHeaders.AddNew();
				factory.Save();
				AssertLessThanOrEqualTo(sw.ElapsedMilliseconds, 60_000);
				AssertNotNullOrEmpty(header.CH_BGMReference);
			}
		}

		[UseSnapshotProtection]
		public void TestAllocateAllBGMReferences_Deadlock2()
		{
			BGMReferenceCounterProvider.Instance.ResetValue();

			var sw = System.Diagnostics.Stopwatch.StartNew();
			var factory = new BusinessObjectFactory();
			var declaration1 = factory.New<JobDeclaration>();
			var header1 = declaration1.ActiveEntryHeaders.AddNew();
			factory.Save();
			AssertNotNullOrEmpty(header1.CH_BGMReference);
			var declaration2 = factory.New<JobDeclaration>();
			var header2 = declaration2.ActiveEntryHeaders.AddNew();
			factory.Save();
			AssertNotNullOrEmpty(header2.CH_BGMReference);
			AssertLessThanOrEqualTo(sw.ElapsedMilliseconds, 60_000);
		}

		[UseSnapshotProtection]
		public void TestAllocateAllBGMReferences_Rollback()
		{
			BGMReferenceCounterProvider.Instance.ResetValue();

			var sw = System.Diagnostics.Stopwatch.StartNew();
			var factory = new BusinessObjectFactory();
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001000";
			factory.Save();
			var header1 = declaration.ActiveEntryHeaders.AddNew();
			declaration.JE_ContainerMode = "BLK";
			Db.Connection.ExecuteNonQuery(@"update dbo.JobDeclaration
set JE_ContainerMode='CNT',
    JE_SystemLastEditTimeUtc = GetUtcDate(),
    JE_SystemLastEditUser = '~BP'
where JE_PK=@pk", c =>
			{
				c.AddParameter("pk", SqlDbType.UniqueIdentifier, declaration.PK.ToGuid());
			}

			);
			AssertExceptionThrown<ZSaveConcurrencyException>(() => factory.Save());
			declaration.JE_ContainerMode = "CNT";
			factory.Save();
			AssertEquals(true, header1.IsInDatabase);
			AssertEquals(false, header1.HasChanges);
			AssertEquals("no skipped number on rollback", "B00001000/1", header1.CH_BGMReference);
			var header2 = declaration.ActiveEntryHeaders.AddNew();
			factory.Save();
			AssertEquals(true, header2.IsInDatabase);
			AssertEquals(false, header2.HasChanges);
			AssertEquals("B00001000/2", header2.CH_BGMReference);
			var otherFactory = new BusinessObjectFactory();
			var declaration2 = otherFactory.Load<JobDeclaration>(declaration.PK);
			var header3 = declaration2.ActiveEntryHeaders.AddNew();
			otherFactory.Save();
			AssertEquals(true, header3.IsInDatabase);
			AssertEquals(false, header3.HasChanges);
			AssertEquals("B00001000/3", header3.CH_BGMReference);
			AssertLessThanOrEqualTo(sw.ElapsedMilliseconds, 60_000);
		}

		public void TestJE_DeclarationType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			instruction1.CEI_Style = "E1";
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			instruction2.CEI_Style = "E2";
			var defaultInstruction = declaration.CustomsEntryInstructions.OfType<CusEntryInstruction>().OrderBy(x => x.PK).FirstOrDefault();
			AssertEquals(defaultInstruction.CEI_Style, declaration.JE_DeclarationType);
			declaration.JE_DeclarationType = "ZZ";
			AssertEquals("ZZ", defaultInstruction.CEI_Style);
			var declaration2 = Factory.New<JobDeclaration>();
			AssertEquals(0, declaration2.CustomsEntryInstructions.Count);
			declaration2.JE_DeclarationType = "ZZ";
			AssertEquals(1, declaration2.CustomsEntryInstructions.Count);
			AssertEquals("ZZ", declaration2.CustomsEntryInstructions[0].CEI_Style);
		}

		public override void TestIsDeclarationWithEntryInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			Assert("AsycudaCustoms Declaration should support EntryInstructions", !declaration.CustomsEntryInstructionProvider.IsNoEntryInstruction);
		}

		public void TestHouseBillsCollectionIsOfRightType()
		{
			var declaration = (JobDeclaration)GetNewBusinessObject();
			AssertEquals(typeof(BillCollection<Bill, JobDeclaration>), declaration.Bills.GetType());
		}

		public void TestLookupObjectIsCached()
		{
			var bizO = (JobDeclaration)GetNewBusinessObject();
			var firstLookup = bizO.Lookups;
			var secondLookup = bizO.Lookups;
			AssertEquals(secondLookup, firstLookup);
		}

		public void TestTypeDecider()
		{
			Assert("Update dbo.JobDeclaration to include a decider for this class", Factory.New<JobDeclaration>().GetType() == GetExpectedBusinessObjectType());
		}

		public void TestInvoicesRequiredToBeInSameIncoTerm()
		{
			const string msgError = "Incoterms should be the same for all Invoices.";
			var dec = Factory.New<JobDeclaration>();
			var invoice1 = dec.Invoices.AddNew();
			var invoice2 = dec.Invoices.AddNew();
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredAtTerminal;
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredAtPlace;
			AssertNoMessageError(invoice1.JZ_IncoTermInfo, msgError);
			AssertHasMessageError(invoice2.JZ_IncoTermInfo, msgError);
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredAtTerminal;
			AssertNoMessageError(invoice2.JZ_IncoTermInfo, msgError);
		}

		public void TestInvoicesRequiredToBeInSameCurrency()
		{
			const string msgError = "Currency should be the same for all Invoices.";
			var dec = Factory.New<JobDeclaration>();
			var invoice1 = dec.Invoices.AddNew();
			var invoice2 = dec.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Namibia;
			AssertNoMessageError(invoice1.JZ_RX_NKInvoice_CurrencyInfo, msgError);
			AssertHasMessageError(invoice2.JZ_RX_NKInvoice_CurrencyInfo, msgError);
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			AssertNoMessageError(invoice2.JZ_RX_NKInvoice_CurrencyInfo, msgError);
		}

		public override void TestBondedWarehouseEditable()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertEquals("BondedWarehouse is invisible", false, dec.BondedWarehouseEditable);
		}

		public void TestLocalCurrencyCodeCore()
		{
			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);
			var codeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Asycuda, Core.Constants.CountryCodes.Namibia, RefCountry.LoadFromCountryCode(factory, Core.Constants.CountryCodes.Namibia).RN_Desc, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(codeList.PK, "CustomsCurrency", "AAA");
			factory.Save();
			CombineAssertions(() =>
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
				{
					var dec = factory.New<JobDeclarationForTesting>();
					AssertEquals("empty", ZString.Empty, dec.LocalCurrencyCodeCoreExposed);
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Namibia))
				{
					var dec = factory.New<JobDeclarationForTesting>();
					AssertEquals("load from attribute", "AAA", dec.LocalCurrencyCodeCoreExposed);
				}
			});
		}

		public void TestMergeManagerType()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType(typeof(MergeManager), declaration.MergeManager);
		}

		public void TestIncoTermAndCustomsChargeFactory_ConfigurationDoesNotExist()
		{
			CombineAssertions(() =>
			{
				AssertIncoTermAndCustomsChargeFactory("IMP.EXP",
					(JobMessageTypeList.Codes.Import, typeof(FOBCIFIncoTermAndCustomsChargeFactory)),
					(JobMessageTypeList.Codes.Export, typeof(FOBCIFIncoTermAndCustomsChargeFactory))
				);
				AssertIncoTermAndCustomsChargeFactory("EXP.IMP",
					(JobMessageTypeList.Codes.Export, typeof(FOBCIFIncoTermAndCustomsChargeFactory)),
					(JobMessageTypeList.Codes.Import, typeof(FOBCIFIncoTermAndCustomsChargeFactory))
				);
			});
		}

		public void TestIncoTermAndCustomsChargeFactory_ConfigurationExists()
		{
			var configuration = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK)).Configuration;

			CombineAssertions(() =>
			{
				foreach (var customsValueCodeForExport in new[] { CustomsValueCodeList.Codes.FOB, CustomsValueCodeList.Codes.CIF })
				{
					foreach (var (customsValueCode, vatValueCode) in new[]
							{
								(CustomsValueCodeList.Codes.FOB, CustomsValueCodeList.Codes.FOB),
								(CustomsValueCodeList.Codes.FOB, CustomsValueCodeList.Codes.CIF),
								(CustomsValueCodeList.Codes.CIF, CustomsValueCodeList.Codes.CIF),
							})
					{
						configuration.ZZC_CustomsValueCodeForExport = customsValueCodeForExport;
						configuration.ZZC_CustomsValueCode = customsValueCode;
						configuration.ZZC_VATValueCode = vatValueCode;

						AssertIncoTermAndCustomsChargeFactory($"{customsValueCodeForExport}.{customsValueCode}.{vatValueCode}|IMP.EXP",
							(JobMessageTypeList.Codes.Import, GetImportIncoTermAndCustomsChargeFactory()),
							(JobMessageTypeList.Codes.Export, GetExportIncoTermAndCustomsChargeFactory())
						);
						AssertIncoTermAndCustomsChargeFactory($"{customsValueCodeForExport}.{customsValueCode}.{vatValueCode}|EXP.IMP",
							(JobMessageTypeList.Codes.Export, GetExportIncoTermAndCustomsChargeFactory()),
							(JobMessageTypeList.Codes.Import, GetImportIncoTermAndCustomsChargeFactory())
						);

						Type GetImportIncoTermAndCustomsChargeFactory()
						{
							switch (customsValueCode + vatValueCode)
							{
								case "FOBFOB":
									return typeof(FOBFOBIncoTermAndCustomsChargeFactory);
								case "FOBCIF":
									return typeof(FOBCIFIncoTermAndCustomsChargeFactory);
								case "CIFCIF":
									return typeof(CIFCIFIncoTermAndCustomsChargeFactory);
							}

							return null;
						}

						Type GetExportIncoTermAndCustomsChargeFactory()
						{
							return customsValueCodeForExport == CustomsValueCodeList.Codes.FOB
								? typeof(FOBFOBIncoTermAndCustomsChargeFactory)
								: typeof(CIFCIFIncoTermAndCustomsChargeFactory);
						}
					}
				}
			});
		}

		public void TestGetDocManagerInfo()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<DeclarationDocManagerInfo>(declaration.DocManagerInfo);
		}

		public void TestDateOfValuation()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_ValuationDate = new ZDate(2021, 1, 1);
			CombineAssertions(() =>
			{
				AssertEquals("DateOfValuation from JE_ValuationDate", testDec.JE_ValuationDate, testDec.DateOfValuation.Date);
				testDec.JE_ValuationDate = ZDate.Empty;
				AssertEquals("DateOfValuation from CachedTodaysDate", testDec.CachedTodaysDate, testDec.DateOfValuation);
			});
		}

		public void TestDefaultValuationDate()
		{
			var configuration = ZZRefCusConfiguration.New(GlbCompany.CurrentCompany);
			configuration.ZZC_DefaultExportValuationDate = "";
			configuration.Factory.Save();

			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_ExportDate = new ZDate(2021, 1, 1);
			testDec.JE_DateOfArrival = new ZDate(2021, 1, 2);
			CombineAssertions(() =>
			{
				testDec.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("JE_ValuationDate not defaulting", ZDate.Empty, testDec.JE_ValuationDate);

				configuration.ZZC_DefaultExportValuationDate = ValuationDateDefaultTypeList.Codes.DOE;
				configuration.Factory.Save();
				testDec.JE_ExportDate = new ZDate(2021, 2, 1);
				AssertEquals("JE_ValuationDate from JE_ExportDate, update when JE_ExportDate changes", new ZDate(2021, 2, 1), testDec.JE_ValuationDate);

				testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
				configuration.ZZC_DefaultImportValuationDate = "";
				configuration.Factory.Save();
				testDec.JE_ExportDate = new ZDate(2021, 3, 1);
				AssertEquals("JE_ValuationDate not defaulting", new ZDate(2021, 2, 1), testDec.JE_ValuationDate);

				configuration.ZZC_DefaultImportValuationDate = ValuationDateDefaultTypeList.Codes.DOE;
				configuration.Factory.Save();
				testDec.JE_ExportDate = new ZDate(2021, 3, 2);
				AssertEquals("JE_ValuationDate from JE_ExportDate, update when JE_ExportDate changes", new ZDate(2021, 3, 2), testDec.JE_ValuationDate);

				configuration.ZZC_DefaultImportValuationDate = ValuationDateDefaultTypeList.Codes.DOA;
				configuration.Factory.Save();
				testDec.JE_DateOfArrival = new ZDate(2021, 4, 1);
				AssertEquals("JE_ValuationDate from JE_DateOfArrival, update when JE_DateOfArrival changes", new ZDate(2021, 4, 1), testDec.JE_ValuationDate);

				testDec.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("JE_ValuationDate from JE_ExportDate, update when JE_MessageType changes", new ZDate(2021, 3, 2), testDec.JE_ValuationDate);
			});
		}

		public void TestJE_ValuationDate()
		{
			AssertEquals("Valuation Date", DataBoundResourceStrings.GetDataForProperty(Factory.New<JobDeclaration>().JE_ValuationDateInfo).Caption);
		}

		public void TestJE_EntryStatusDescription()
		{
			var countryCode = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(RefCusCodeListTypes.Codes.UserDefinedEntryStatus, "User Defined Entry Status");
			helper.CreateCusCodeList(countryCode, RefCusCodeListTypes.Codes.UserDefinedEntryStatus, "ST1", "STA1Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(countryCode, RefCusCodeListTypes.Codes.UserDefinedEntryStatus, "ST2", "STA2Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = "ST1";
			AssertEquals("STA1Desc", declaration.JE_EntryStatusDescription);

			var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
			entryHeader2.CH_EntryStatus = "ST2";
			AssertEquals("when entry status are different", CommonEntryStatusList.Descriptions.MultipleEntryStatus.ToString(), declaration.JE_EntryStatusDescription);

			entryHeader2.CH_EntryStatus = "ST1";
			AssertEquals("when all entry status are the same", "STA1Desc", declaration.JE_EntryStatusDescription);
		}

		public void TestFilteredInvoiceLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<InvoiceLineViewCollection<JobComInvoiceLine>>(declaration.FilteredInvoiceLines);
		}

		public override void TestEntryStatusChangedLogged()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = "ST1";
			Factory.Save();

			declaration.Reload();
			var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
			entryHeader2.CH_EntryStatus = "ST2";
			Factory.Save();

			declaration.Reload();
			AssertEquals("No Event Created on Dec", 0, declaration.Logs.Find(log => log.SL_SE_NKEvent == Events.CustomsEntryStatusCode && log.SL_Parent == declaration.PK).Count());
		}

		#region JE_MessageType

		public override void TestIsMessageTypeChangeAnError()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			Assert("[PreCondition]: IsMessageTypeChangeAnError should be false", !declaration.IsMessageTypeChangeAnError);

			_ = declaration.CustomsEntryHeaders.AddNew();

			Assert("IsMessageTypeChangeAnError should be true", declaration.IsMessageTypeChangeAnError);
		}

		#endregion

		GlbBranch CreateCompanyAndBranch(ZString code, ZString countryCode)
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = code;
			company.GC_Name = "TEST COMP " + code;
			company.GC_RN_NKCountryCode = countryCode;
			company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var branch = company.Branches.AddNew();
			branch.GB_Code = code;
			branch.GB_BranchName = "TEST BRANCH " + code;
			branch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			return branch;
		}

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			return declaration;
		}

		void AssertIncoTermAndCustomsChargeFactory(string testCase, (string MessageType, Type IncoTermAndChargeFactoryType) first, (string MessageType, Type IncoTermAndChargeFactoryType) second)
		{
			var declaration = GetDeclaration(first.MessageType);
			AssertType(testCase + "|" + first.MessageType, first.IncoTermAndChargeFactoryType, declaration.IncoTermAndChargeFactory);
			declaration.JE_MessageType = second.MessageType;
			AssertType(testCase + "|" + $"{first.MessageType}->{second.MessageType}", second.IncoTermAndChargeFactoryType, declaration.IncoTermAndChargeFactory);
		}

		JobDeclaration GetDeclaration(string messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			return declaration;
		}
	}

	class JobDeclarationForTesting : JobDeclaration
	{
		public JobDeclarationForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZString LocalCurrencyCodeCoreExposed => LocalCurrencyCodeCore;
	}
}
