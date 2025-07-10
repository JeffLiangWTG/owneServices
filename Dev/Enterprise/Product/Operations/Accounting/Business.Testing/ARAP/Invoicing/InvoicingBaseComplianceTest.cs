using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.RSACryptography;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;
using TaxRegRuleCodes = Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxRegistrationLocationRuleCodes;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	class InvoicingBaseComplianceTest : TestCaseWithFactory
	{
		public void TestAH_ComplianceSubType_ReadOnly_KoreaSouth()
		{
			var aPInvoice = Factory.NewWithValidTestData<APInvoice>();
			var aPCRD = Factory.NewWithValidTestData<APCreditNote>();
			var aPADJ = Factory.NewWithValidTestData<APAdjustmentNote>();
			var aRInvoice = Factory.NewWithValidTestData<ARInvoice>();

			AssertEquals("Default value", false, aPInvoice.AH_ComplianceSubType_ReadOnly);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.KoreaSouth))
			{
				AssertEquals(true, aPInvoice.AH_ComplianceSubType_ReadOnly);
				AssertEquals(true, aPCRD.AH_ComplianceSubType_ReadOnly);
				AssertEquals(true, aPADJ.AH_ComplianceSubType_ReadOnly);
				AssertEquals(false, aRInvoice.AH_ComplianceSubType_ReadOnly);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Romania))
			{
				AssertEquals(false, aPInvoice.AH_ComplianceSubType_ReadOnly);
				AssertEquals(false, aPCRD.AH_ComplianceSubType_ReadOnly);
				AssertEquals(false, aPADJ.AH_ComplianceSubType_ReadOnly);
				AssertEquals(false, aRInvoice.AH_ComplianceSubType_ReadOnly);
			}
		}

		public void TestShouldApplyCompliancePolicyForPrintingAuthorizationNumberInPortugal_Company()
		{
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			{
				var testObjCreator = new TestObjectCreator(Factory);
				var arInvoice = Factory.NewWithValidTestData<ARInvoice>();
				arInvoice.AH_GC = testObjCreator.NonCurrentCompany.PK;
				arInvoice.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TXI;
				arInvoice.AH_InvoiceDate = new ZDateTime(2024, 1, 1);

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
				{
					AssertEquals(false, arInvoice.ShouldApplyCompliancePolicyForPrintingAuthorizationNumber);
				}

				using (testObjCreator.NonCurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
				{
					AssertEquals(true, arInvoice.ShouldApplyCompliancePolicyForPrintingAuthorizationNumber);
				}
			}
		}

		public void TestShouldApplyCompliancePolicyForPrintingAuthorizationNumberInPortugal()
		{
			string complianceSubType = string.Empty;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
			{
				AssertEquals("Percondition", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post, AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.Value);
				using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Payables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
				{
					AssertEquals("Percondition", false, AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value);

					complianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TXI;
					AssertShouldApplyCompliancePolicyForPrintingAuthorizationNumber(false, typeof(APInvoice));
					AssertShouldApplyCompliancePolicyForPrintingAuthorizationNumber(false, typeof(APCreditNote));
					AssertShouldApplyCompliancePolicyForPrintingAuthorizationNumber(false, typeof(APAdjustmentNote));
					AssertShouldApplyCompliancePolicyForPrintingAuthorizationNumber(false, typeof(UAInvoice));
					AssertShouldApplyCompliancePolicyForPrintingAuthorizationNumber(true, typeof(ARInvoice));
					AssertShouldApplyCompliancePolicyForPrintingAuthorizationNumber(true, typeof(ARCreditNote));
					AssertShouldApplyCompliancePolicyForPrintingAuthorizationNumber(true, typeof(ARAdjustmentNote));
					AssertShouldApplyCompliancePolicyForPrintingAuthorizationNumber(false, typeof(UAInvoice));

					var invoice = AssertShouldApplyCompliancePolicyForPrintingAuthorizationNumber(true, typeof(ARInvoice));
					invoice.AH_InvoiceDate = new ZDateTime(2022, 12, 30);
					AssertEquals(false, invoice.ShouldApplyCompliancePolicyForPrintingAuthorizationNumber);

					invoice = AssertShouldApplyCompliancePolicyForPrintingAuthorizationNumber(true, typeof(ARInvoice));
					invoice.AH_ComplianceSubType = string.Empty;
					AssertEquals(false, invoice.ShouldApplyCompliancePolicyForPrintingAuthorizationNumber);

					using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
					{
						AssertShouldApplyCompliancePolicyForPrintingAuthorizationNumber(false, typeof(ARInvoice));
					}
				}

				AssertExceptionThrown<RegistryValidationException>("Cannot change default value for country 'PT'.", () => { AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print); });
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Indonesia))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			{
				AssertEquals("Percondition", false, AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value);

				complianceSubType = IndonesiaComplianceInfo.ComplianceSubTypeCodes.BKP;
				AssertShouldApplyCompliancePolicyForPrintingAuthorizationNumber(false, typeof(ARInvoice));
			}

			InvoicingBase AssertShouldApplyCompliancePolicyForPrintingAuthorizationNumber(bool exceptedValue, Type type)
			{
				var factory = new BusinessObjectFactory();
				var transaction = factory.NewWithValidTestData(type) as InvoicingBase;
				transaction.AH_ComplianceSubType = complianceSubType;
				transaction.AH_InvoiceDate = new ZDateTime(2023, 1, 1);
				AssertEquals(exceptedValue, transaction.ShouldApplyCompliancePolicyForPrintingAuthorizationNumber);
				return transaction;
			}
		}

		#region Usage of IComplianceNumberProvider.CanAllocateComplianceNumber

		public void TestAH_TransactionReferenceSettingOnSavingUsesCanAllocateComplianceNumberForInvoiceCountry_Japan()
		{
			AssertAH_TransactionReferenceSettingOnSavingUsesCanAllocateComplianceNumberForInvoiceCountry(CountryCodes.Japan);
		}

		public void TestAH_TransactionReferenceSettingOnSavingUsesCanAllocateComplianceNumberForInvoiceCountry_NewZeland()
		{
			AssertAH_TransactionReferenceSettingOnSavingUsesCanAllocateComplianceNumberForInvoiceCountry(CountryCodes.NewZealand);
		}

		void AssertAH_TransactionReferenceSettingOnSavingUsesCanAllocateComplianceNumberForInvoiceCountry(string countryCode)
		{
			// This precondition is required because RefCountry caches ComplianceSubTypeRules registry value on startup of CW1 application when ModulesTree checks the registry to add accounting modules to the tree.
			// CW1 test adaptor for VisualStudio calls that before running tests. So, changes to the registry for Australia does not impact RefCountry.SupportComplianceSubType value but SupportComplianceSubType is
			// the precondition for tests in this region. One simple way to overcome this problem is to use another RefCountry instance and in turn, another country.
			AssertNotEquals("Precondition: Country is not equal to cached system startup country", CountryCodes.Australia, countryCode);

			var mockIComplianceNumberProvider = new Mock<IComplianceNumberProvider>();
			mockIComplianceNumberProvider.Setup(o => o.CanAllocateComplianceNumber(It.IsAny<bool>())).Returns(true);

			var mockIGlobalAccountingCountryFactory = new Mock<IAccountingCountryComplianceGlobalFactory>();
			mockIGlobalAccountingCountryFactory.Setup(o => o.GetFeatureInterface<IComplianceNumberProvider>(It.IsAny<ZString>())).Returns(mockIComplianceNumberProvider.Object);
			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				TestMockObjectCreator.SetupSupportComplianceSubType(countryCode);

				AssertEquals("Percondition", true, GlbCompany.CurrentCompany.Country.SupportComplianceSubType);
				AssertEquals("Percondition", false, AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value);

				var menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;
				var sequence = ObjectCreator.SetupComplianceSequence(menuPK, "ABC", "", 1, 100, 1);
				Factory.Save();

				var receivableInvoice = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID }, false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZString.Empty, false);
				receivableInvoice.AH_ComplianceSubType = "ABC";
				Factory.Save();
				AssertEquals("Postcondition: AH_XD_ComplianceBook", sequence.PK, receivableInvoice.AH_XD_ComplianceBook);

				mockIGlobalAccountingCountryFactory.Verify(o => o.GetFeatureInterface<IComplianceNumberProvider>(countryCode));
			}
		}

		public void TestAH_TransactionReferenceIsSetOnSaving_WhenCanAllocateComplianceNumberIsTrue()
		{
			AssertAH_TransactionReferencesSettingOnSavingDependsOnCanAllocateComplianceNumberResult(true);
		}

		public void TestAH_TransactionReferenceIsNotSetOnSaving_WhenCanAllocateComplianceNumberIsFalse()
		{
			AssertAH_TransactionReferencesSettingOnSavingDependsOnCanAllocateComplianceNumberResult(false);
		}

		void AssertAH_TransactionReferencesSettingOnSavingDependsOnCanAllocateComplianceNumberResult(bool canAllocateComplianceNumber)
		{
			var mockIComplianceNumberProvider = new Mock<IComplianceNumberProvider>();
			mockIComplianceNumberProvider.Setup(o => o.CanAllocateComplianceNumber(It.IsAny<bool>())).Returns(canAllocateComplianceNumber);

			var mockIGlobalAccountingCountryFactory = new Mock<IAccountingCountryComplianceGlobalFactory>();
			mockIGlobalAccountingCountryFactory.Setup(o => o.GetFeatureInterface<IComplianceNumberProvider>(It.IsAny<ZString>())).Returns(mockIComplianceNumberProvider.Object);
			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);

			var countryOtherThanCachedSystemStartupCountry = CountryCodes.ChristmasIsland;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryOtherThanCachedSystemStartupCountry))
			{
				TestMockObjectCreator.SetupSupportComplianceSubType(countryOtherThanCachedSystemStartupCountry);

				AssertEquals("Percondition", true, GlbCompany.CurrentCompany.Country.SupportComplianceSubType);
				AssertEquals("Percondition", false, AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value);

				var menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;
				var sequence = ObjectCreator.SetupComplianceSequence(menuPK, "ABC", "", 1, 100, 1);
				Factory.Save();

				var receivableInvoice = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID }, false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZString.Empty, false);
				receivableInvoice.AH_ComplianceSubType = "ABC";
				Factory.Save();
				var expectedValue = canAllocateComplianceNumber ? sequence.PK : ZGuid.Empty;
				AssertEquals("AH_XD_ComplianceBook", expectedValue, receivableInvoice.AH_XD_ComplianceBook);
			}
		}

		#endregion

		#region Argentina Related Tests

		public void TestComplianceSubType_Argentina_Ruleset1_TipoABForOriginalTransaction()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Argentina))
			{
				var fallback = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleSet.SetValue(fallback.CompanyPK(false), Guid.Empty, Guid.Empty, new ComplianceSubTypeAttributionRuleSet(fallback, Factory, ArgentinaComplianceInfo.RuleSetCodes.TipoABE));
				SetupAndAssertTipoABForOriginalTransaction(ZString.Empty, ZString.Empty);
			}
		}

		public void TestComplianceSubType_Argentina_Ruleset2_TipoABForOriginalTransaction()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Argentina))
			{
				var fallback = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleSet.SetValue(fallback.CompanyPK(false), Guid.Empty, Guid.Empty, new ComplianceSubTypeAttributionRuleSet(fallback, Factory, ArgentinaComplianceInfo.RuleSetCodes.TipoABExcludedSupply));
				SetupAndAssertTipoABForOriginalTransaction(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXB, ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCB);
			}
		}

		void SetupAndAssertTipoABForOriginalTransaction(string expectedInvoiceSubTypeWhenOrgIsForeign, string expectedCreditNoteSubTypeWhenOrgIsForeign)
		{
			var org1 = ObjectCreator.CreateOrgHeader("ORG1", true, true, "ARABA");
			org1.CustomsCodes.AddNew(ArgentinaOrgCusCodeInfo.OrgCusCodes.IVR, CountryCodes.Argentina);

			var org2 = ObjectCreator.CreateOrgHeader("ORG2", true, true, "ARABA");

			var foreignOrg = ObjectCreator.CreateOrgHeader("FORG", true, true, "USLAX");

			var invoice1 = CreateInvoiceAndLineWithGST(typeof(ARInvoice), org1);
			Factory.Save();
			AssertEquals("should use TXA because org1 is VAT recoverable", ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXA, invoice1.AH_ComplianceSubType);

			var invoice2 = CreateInvoiceAndLineWithGST(typeof(ARInvoice), org2);
			Factory.Save();
			AssertEquals("should use TXB because org2 is VAT not recoverable", ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXB, invoice2.AH_ComplianceSubType);

			var invoice3 = CreateInvoiceAndLineWithGST(typeof(ARInvoice), foreignOrg);
			Factory.Save();
			AssertEquals($"should use {expectedInvoiceSubTypeWhenOrgIsForeign} because org3 is foreign", expectedInvoiceSubTypeWhenOrgIsForeign, invoice3.AH_ComplianceSubType);

			var creditNote1 = CreateInvoiceAndLineWithGST(typeof(ARCreditNote), org1);
			Factory.Save();
			AssertEquals("should use TCA because org1 is VAT recoverable", ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCA, creditNote1.AH_ComplianceSubType);

			var creditNote2 = CreateInvoiceAndLineWithGST(typeof(ARCreditNote), org2);
			Factory.Save();
			AssertEquals("should use TCB because org2 is Not VAT recoverable", ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCB, creditNote2.AH_ComplianceSubType);

			var creditNote3 = CreateInvoiceAndLineWithGST(typeof(ARCreditNote), foreignOrg);
			Factory.Save();
			AssertEquals($"should use {expectedCreditNoteSubTypeWhenOrgIsForeign} because org3 is foreign", expectedCreditNoteSubTypeWhenOrgIsForeign, creditNote3.AH_ComplianceSubType);
		}

		public void TestComplianceSubType_Argentina_Rulseset1_ForeignOrganisation_NotVatRecoverable()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Argentina))
			{
				var fallback = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleSet.SetValue(fallback.CompanyPK(false), Guid.Empty, Guid.Empty, new ComplianceSubTypeAttributionRuleSet(fallback, Factory, ArgentinaComplianceInfo.RuleSetCodes.TipoABE));
				SetupAndAssertForeignOrganisation_NotVatRecoverable(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE, ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDE, ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCE, ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCE);
			}
		}

		public void TestComplianceSubType_Argentina_Rulseset2_ForeignOrganisation_NotVatRecoverable()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Argentina))
			{
				var fallback = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleSet.SetValue(fallback.CompanyPK(false), Guid.Empty, Guid.Empty, new ComplianceSubTypeAttributionRuleSet(fallback, Factory, ArgentinaComplianceInfo.RuleSetCodes.TipoABExcludedSupply));
				SetupAndAssertForeignOrganisation_NotVatRecoverable(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXB, ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDB, ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCB, ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCB);
			}
		}

		void SetupAndAssertForeignOrganisation_NotVatRecoverable(string expectedOriginalInvoiceSubType, string expectedAmendedInvoiceSubType, string expectedOriginalCreditNoteSubType, string expectedAmendedCreditNoteSubType)
		{
			var foreignOrg = ObjectCreator.CreateOrgHeader("FORG", true, true, "USLAX");

			var invoice1 = CreateInvoiceAndLineWithFreeGST(typeof(ARInvoice), foreignOrg);
			Factory.Save();
			AssertEquals($"should use {expectedOriginalInvoiceSubType} because org is VAT not recoverable and located outside Argentina", expectedOriginalInvoiceSubType, invoice1.AH_ComplianceSubType);

			var amendingInvoice = ObjectCreator.AmendARTransaction(TransactionTypes.Invoice, invoice1);
			Factory.Save();
			AssertNullOrEmpty("Error Message", amendingInvoice.errorMessage);
			AssertNotNull("Amended Invoice2 must'n be null ", amendingInvoice.amendTransaction);
			AssertEquals($"should use {expectedAmendedInvoiceSubType} because org is VAT not recoverable and located outside Argentina", expectedAmendedInvoiceSubType, ((InvoicingBase)amendingInvoice.amendTransaction).AH_ComplianceSubType);

			var creditNote1 = CreateInvoiceAndLineWithFreeGST(typeof(ARCreditNote), foreignOrg);
			Factory.Save();
			AssertEquals($"should use {expectedOriginalCreditNoteSubType} because org is VAT not recoverable and located outside Argentina", expectedOriginalCreditNoteSubType, creditNote1.AH_ComplianceSubType);

			var amendingCreditNote = ObjectCreator.AmendARTransaction(TransactionTypes.CreditNote, invoice1);
			Factory.Save();
			AssertNullOrEmpty("Error Message", amendingCreditNote.errorMessage);
			AssertNotNull("Amend Credit Note must'n be null ", amendingCreditNote.amendTransaction);
			AssertEquals($"should use {expectedAmendedCreditNoteSubType} because org is VAT not recoverable and located outside Argentina", expectedAmendedCreditNoteSubType, ((InvoicingBase)amendingCreditNote.amendTransaction).AH_ComplianceSubType);
		}

		public void TestComplianceSubType_Argentina_Rulseset1_LocalOrganisation_VatRecoverable()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Argentina))
			{
				var fallback = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleSet.SetValue(fallback.CompanyPK(false), Guid.Empty, Guid.Empty, new ComplianceSubTypeAttributionRuleSet(fallback, Factory, ArgentinaComplianceInfo.RuleSetCodes.TipoABE));
				SetupAndAssertLocalOrganisation_VatRecoverable();
			}
		}

		public void TestComplianceSubType_Argentina_Rulseset2_LocalOrganisation_VatRecoverable()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Argentina))
			{
				var fallback = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleSet.SetValue(fallback.CompanyPK(false), Guid.Empty, Guid.Empty, new ComplianceSubTypeAttributionRuleSet(fallback, Factory, ArgentinaComplianceInfo.RuleSetCodes.TipoABExcludedSupply));
				SetupAndAssertLocalOrganisation_VatRecoverable();
			}
		}

		void SetupAndAssertLocalOrganisation_VatRecoverable()
		{
			var org = ObjectCreator.CreateOrgHeader("ORG", true, true, "ARABA");
			org.CustomsCodes.AddNew(ArgentinaOrgCusCodeInfo.OrgCusCodes.IVR, CountryCodes.Argentina);

			var invoice1 = CreateInvoiceAndLineWithFreeGST(typeof(ARInvoice), org);
			Factory.Save();
			AssertEquals("should use TXA because org is VAT recoverable and located in Argentina", ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXA, invoice1.AH_ComplianceSubType);

			var amendingInvoice = ObjectCreator.AmendARTransaction(TransactionTypes.Invoice, invoice1);
			Factory.Save();
			AssertNullOrEmpty("Error Message", amendingInvoice.errorMessage);
			AssertNotNull("Amended Invoice2 must'n be null ", amendingInvoice.amendTransaction);
			AssertEquals("should use TDA because org is VAT recoverable and located in Argentina", ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDA, ((InvoicingBase)amendingInvoice.amendTransaction).AH_ComplianceSubType);

			var creditNote1 = CreateInvoiceAndLineWithFreeGST(typeof(ARCreditNote), org);
			Factory.Save();
			AssertEquals("should use TCA because org is VAT recoverable and located in Argentina", ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCA, creditNote1.AH_ComplianceSubType);

			var amendingCreditNote2 = ObjectCreator.AmendARTransaction(TransactionTypes.CreditNote, invoice1);
			Factory.Save();
			AssertNullOrEmpty("Error Message", amendingCreditNote2.errorMessage);
			AssertNotNull("Amended Invoice2 must'n be null ", amendingCreditNote2.amendTransaction);
			AssertEquals("should use TCA because org is VAT recoverable and located in Argentina", ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCA, ((InvoicingBase)amendingCreditNote2.amendTransaction).AH_ComplianceSubType);
		}

		public void TestComplianceSubType_Argentina_Rulseset1_ForeignOrganisation_VatRecoverable()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Argentina))
			{
				var fallback = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleSet.SetValue(fallback.CompanyPK(false), Guid.Empty, Guid.Empty, new ComplianceSubTypeAttributionRuleSet(fallback, Factory, ArgentinaComplianceInfo.RuleSetCodes.TipoABE));
				SetupAndAssertForeignOrganisation_VatRecoverable();
			}
		}

		public void TestComplianceSubType_Argentina_Rulseset2_ForeignOrganisation_VatRecoverable()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Argentina))
			{
				var fallback = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleSet.SetValue(fallback.CompanyPK(false), Guid.Empty, Guid.Empty, new ComplianceSubTypeAttributionRuleSet(fallback, Factory, ArgentinaComplianceInfo.RuleSetCodes.TipoABExcludedSupply));
				SetupAndAssertForeignOrganisation_VatRecoverable();
			}
		}

		void SetupAndAssertForeignOrganisation_VatRecoverable()
		{
			var foreignOrg = ObjectCreator.CreateOrgHeader("FORG", true, true, "USLAX");
			foreignOrg.CustomsCodes.AddNew(ArgentinaOrgCusCodeInfo.OrgCusCodes.IVR, CountryCodes.Argentina);

			var invoice1 = CreateInvoiceAndLineWithFreeGST(typeof(ARInvoice), foreignOrg);
			Factory.Save();
			AssertEquals("should not use any subtype because org is VAT recoverable but located outside Argentina", "", invoice1.AH_ComplianceSubType);

			var creditNote1 = CreateInvoiceAndLineWithFreeGST(typeof(ARCreditNote), foreignOrg);
			Factory.Save();
			AssertEquals("should not use any subtype because org is VAT recoverable but located outside Argentina", "", creditNote1.AH_ComplianceSubType);
		}

		public void TestComplianceSubType_Argentina_Ruleset2_ExcludedTax_OriginalTransaction()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Argentina))
			{
				var fallback = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleSet.SetValue(fallback.CompanyPK(false), Guid.Empty, Guid.Empty, new ComplianceSubTypeAttributionRuleSet(fallback, Factory, ArgentinaComplianceInfo.RuleSetCodes.TipoABExcludedSupply));

				var org1 = ObjectCreator.CreateOrgHeader("ORG1", true, true, "ARABA");
				org1.CustomsCodes.AddNew(ArgentinaOrgCusCodeInfo.OrgCusCodes.IVR, CountryCodes.Argentina);

				var org2 = ObjectCreator.CreateOrgHeader("ORG2", true, true, "ARABA");

				var foreignOrg = ObjectCreator.CreateOrgHeader("FORG", true, true, "USLAX");

				var invoice1 = CreateInvoiceAndLineWithExcludedTax(typeof(ARInvoice), org1);
				Factory.Save();

				var expectedInvoiceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.XCL;
				AssertEquals("should use XCL because org1 is VAT recoverable", expectedInvoiceSubType, invoice1.AH_ComplianceSubType);

				var invoice2 = CreateInvoiceAndLineWithExcludedTax(typeof(ARInvoice), org2);
				Factory.Save();
				AssertEquals("should use XCL because org2 is VAT not recoverable", expectedInvoiceSubType, invoice2.AH_ComplianceSubType);

				var invoice3 = CreateInvoiceAndLineWithExcludedTax(typeof(ARInvoice), foreignOrg);
				Factory.Save();
				AssertEquals("should use XCL because org3 is foreign", expectedInvoiceSubType, invoice3.AH_ComplianceSubType);

				var creditNote1 = CreateInvoiceAndLineWithExcludedTax(typeof(ARCreditNote), org1);
				Factory.Save();
				AssertEquals("should use XCL because org1 is VAT recoverable", expectedInvoiceSubType, creditNote1.AH_ComplianceSubType);

				var creditNote2 = CreateInvoiceAndLineWithExcludedTax(typeof(ARCreditNote), org2);
				Factory.Save();
				AssertEquals("should use XCL because org2 is Not VAT recoverable", expectedInvoiceSubType, creditNote2.AH_ComplianceSubType);

				var creditNote3 = CreateInvoiceAndLineWithExcludedTax(typeof(ARCreditNote), foreignOrg);
				Factory.Save();
				AssertEquals("should use XCL because org3 is foreign", expectedInvoiceSubType, creditNote3.AH_ComplianceSubType);
			}
		}

		#endregion

		public void TestSetComplianceSubTypeIfIsNecessary_NoDefaultValue()
		{
			// Arrange
			var italyCompany = ObjectCreator.CreateCompanyAndBranch("ITROM");
			Factory.Save();

			var emptyCollection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
			AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetValue(italyCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, emptyCollection);

			using (ObjectCreator.SwitchEnvToCompany(italyCompany))
			{
				AssertEquals("There should be no default configurations", 0, AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.Value.Count);

				var apAdjustmentNote = ObjectCreator.CreateInvoice(typeof(APAdjustmentNote), "APAN01", null, null, ObjectCreator.AALSHI);
				apAdjustmentNote.AH_ComplianceSubType = "ZZZ";

				var arAdjustmentNote = ObjectCreator.CreateInvoice(typeof(ARAdjustmentNote), "ARAN01", null, null, ObjectCreator.AALSHI);
				arAdjustmentNote.AH_ComplianceSubType = "ZZZ";

				// Act
				apAdjustmentNote.SetComplianceSubTypeIfIsNecessary();
				arAdjustmentNote.SetComplianceSubTypeIfIsNecessary();

				// Assert
				AssertEquals("System should not override user set compliance sub-type", "ZZZ", apAdjustmentNote.AH_ComplianceSubType);
				AssertEquals("System should not override user set compliance sub-type", "ZZZ", arAdjustmentNote.AH_ComplianceSubType);

				var expectedReference = "Compliance Sub Type was manually set to ZZZ. No default Compliance Sub Type was found.";
				var actualLogReferences = apAdjustmentNote.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.EditedARecord.Code).Select(log => log.SL_Reference).ToArray();
				AssertCollectionContains(expectedReference, actualLogReferences);

				actualLogReferences = arAdjustmentNote.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.EditedARecord.Code).Select(log => log.SL_Reference).ToArray();
				AssertCollectionContains(expectedReference, actualLogReferences);
			}
		}

		public void TestSetComplianceSubTypeIfIsNecessary_Payable()
		{
			// Arrange
			var italyCompany = ObjectCreator.CreateCompanyAndBranch("ITMIL");
			Factory.Save();

			using (ObjectCreator.SwitchEnvToCompany(italyCompany))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Payables.SetTemporaryValue(italyCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			{
				Assert("There should be some default configurations for Italy", AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.Value.Count > 0);

				var taxRate = new AccTaxRate[] { ExemptTaxID, NotReportableTaxID, StandardRateTaxID };
				var invoice = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsPayable, TransactionTypes.Invoice, taxRate, false, CountryCodes.Italy, ZString.Empty, false);
				invoice.AH_ComplianceSubType = "XXX";

				// Act
				invoice.Factory.Save();

				// Assert
				AssertEquals("System should not override user set compliance sub-type", "XXX", invoice.AH_ComplianceSubType);
				var expectedReference = "Compliance Sub Type was manually set to XXX. The default was API.";
				var actualLogReferences = invoice.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.EditedARecord.Code).Select(log => log.SL_Reference).ToArray();
				AssertCollectionContains("Add edit log should say what the user set value was and that it would have been a different value", expectedReference, actualLogReferences);

				// Arrange
				invoice = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsPayable, TransactionTypes.Invoice, taxRate, false, CountryCodes.Italy, ZString.Empty, false);
				invoice.AH_ComplianceSubType = ZString.Empty;

				// Act
				invoice.Factory.Save();

				// Assert
				AssertEquals("System should override user set compliance sub-type with the one matched from the Registry", ItalyComplianceInfo.ComplianceSubTypeCodes.API, invoice.AH_ComplianceSubType);
				expectedReference = "Compliance Sub Type was defaulted to API.";
				actualLogReferences = invoice.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.EditedARecord.Code).Select(log => log.SL_Reference).ToArray();
				AssertCollectionContains("Add edit log should say value was empty and system assigned the one from the Registry", expectedReference, actualLogReferences);

				// Arrange
				invoice = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsPayable, TransactionTypes.Invoice, taxRate, false, CountryCodes.Italy, ZString.Empty, false);
				invoice.AH_ComplianceSubType = "ABC";

				// Act
				invoice.SaveAsIncomplete();

				// Assert
				AssertEquals("Shouldn't override compliance sub type when allocating saving invoice as incomplete", "ABC", invoice.AH_ComplianceSubType);

				var newFactory = new BusinessObjectFactory();
				invoice = newFactory.Load<APInvoice>(invoice.PK);
				invoice.RestoreSavedData();
				invoice.MoveFromIncompleteToPayableLedger();
				invoice.AH_ComplianceSubType = "ZZZ";
				invoice.Factory.Save();

				expectedReference = "Compliance Sub Type was manually set to ZZZ. The default was API.";
				actualLogReferences = invoice.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.EditedARecord.Code).Select(log => log.SL_Reference).ToArray();
				AssertCollectionContains("Should be at least one EDT event created with the desired reference", expectedReference, actualLogReferences);
			}
		}

		public void TestSetComplianceSubTypeIfIsNecessary_Receivable()
		{
			// Arrange
			var italyCompany = ObjectCreator.CreateCompanyAndBranch("ITMIL");
			Factory.Save();

			using (ObjectCreator.SwitchEnvToCompany(italyCompany))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(italyCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			{
				Assert("There should be some default configurations for Italy", AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.Value.Count > 0);

				var taxRate = new AccTaxRate[] { ExemptTaxID, NotReportableTaxID, StandardRateTaxID };
				var invoice = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, taxRate, false, CountryCodes.Italy, ZString.Empty, false);
				invoice.AH_ComplianceSubType = "XXX";

				// Act
				invoice.Factory.Save();

				// Assert
				AssertEquals("System should not override user set compliance sub-type", "XXX", invoice.AH_ComplianceSubType);
				var expectedReference = "Compliance Sub Type was manually set to XXX. The default was ARI.";
				var actualLogReferences = invoice.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.EditedARecord.Code).Select(log => log.SL_Reference).ToArray();
				AssertCollectionContains("Add edit log should say what the user set value was and that it would have been a different value", expectedReference, actualLogReferences);

				// Arrange
				invoice = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, taxRate, false, CountryCodes.Italy, ZString.Empty, false);
				invoice.AH_ComplianceSubType = ZString.Empty;

				// Act
				invoice.Factory.Save();

				// Assert
				AssertEquals("System should override user set compliance sub-type with the one matched from the Registry", ItalyComplianceInfo.ComplianceSubTypeCodes.ARI, invoice.AH_ComplianceSubType);
				expectedReference = "Compliance Sub Type was defaulted to ARI.";
				actualLogReferences = invoice.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.EditedARecord.Code).Select(log => log.SL_Reference).ToArray();
				AssertCollectionContains("Add edit log should say value was empty and system assigned the one from the Registry", expectedReference, actualLogReferences);
			}
		}

		public void TestSignInvoicesRecursivelyInBackwardDirection_Succeed()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				SetupInvoiceSequenceForSigning();
				var result = invoice5.SignInvoicesRecursivelyInBackwardDirection();
				AssertNullOrEmpty("Expect no error", result.errorMessage);
				Assert("Expect invoice 3 signed", !invoice3.AH_DigitalSignature_COMPRESSED.IsEmpty);
				Assert("Expect invoice 4 signed", !invoice4.AH_DigitalSignature_COMPRESSED.IsEmpty);
				Assert("Expect invoice 5 signed", !invoice5.AH_DigitalSignature_COMPRESSED.IsEmpty);
			}
		}

		public void TestSignInvoicesRecursivelyInBackwardDirection_WithError()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				SetupInvoiceSequenceForSigning();
				invoice3.AH_TransactionReference = "XXXXXXXXX";
				Factory.Save();

				var result = invoice5.SignInvoicesRecursivelyInBackwardDirection();
				AssertEquals("Cannot sign invoice 00001003 due to failed to find the previous invoice based on transaction reference TXI abc/000000004", result.errorMessage);
				Assert("Expect invoice 3 not signed", invoice3.AH_DigitalSignature_COMPRESSED.IsEmpty);
				Assert("Expect invoice 4 not signed", invoice4.AH_DigitalSignature_COMPRESSED.IsEmpty);
				Assert("Expect invoice 5 not signed", invoice5.AH_DigitalSignature_COMPRESSED.IsEmpty);
			}
		}

		[TestDate(2023, 05, 29)]
		public void TestAllocateComplianceSequenceNumberForInvoice_ComplianceNumberAllocationDateOptions_AR()
		{
			var currCompany = GlbCompany.CurrentCompany;
			var currBranch = GlbBranch.CurrentBranch;
			var registry = AccountingMasterFilesRegistry.Instance;

			using (currCompany.TemporarilySetCountry(CountryCodes.Portugal))
			{
				ObjectCreator.SetupComplianceSequence(ZGuid.Empty, PortugalComplianceInfo.ComplianceSubTypeCodes.TXI, "abc", 1, 100, 1, currCompany.PK, currBranch.PK,
					new ZDate(2021, 1, 1), new ZDate(2021, 12, 31));
				ObjectCreator.SetupComplianceSequence(ZGuid.Empty, PortugalComplianceInfo.ComplianceSubTypeCodes.TXI, "def", 1, 100, 1, currCompany.PK, currBranch.PK,
					new ZDate(2022, 1, 1), new ZDate(2022, 12, 31));
				ObjectCreator.SetupComplianceSequence(ZGuid.Empty, PortugalComplianceInfo.ComplianceSubTypeCodes.TXI, "ghi", 1, 100, 1, currCompany.PK, currBranch.PK,
					new ZDate(2023, 1, 1), new ZDate(2023, 12, 31));

				AssertInvoiceUseTheCorrectSequenceBook(ComplianceNumberAllocationDateOptions.InvoiceDate.Code, "TXI abc/000000001");
				AssertInvoiceUseTheCorrectSequenceBook(ComplianceNumberAllocationDateOptions.PostDate.Code, "TXI def/000000001");
				AssertInvoiceUseTheCorrectSequenceBook(ComplianceNumberAllocationDateOptions.NoControl.Code, "TXI ghi/000000001");
			}

			void AssertInvoiceUseTheCorrectSequenceBook(string complianceNumberAllocationDateOption, string expectedTransactionReference)
			{
				var otherOption = complianceNumberAllocationDateOption == ComplianceNumberAllocationDateOptions.NoControl.Code ? ComplianceNumberAllocationDateOptions.PostDate.Code : ComplianceNumberAllocationDateOptions.NoControl.Code;

				using (registry.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(currCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
				using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(currCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceNumberAllocationDateOption))
				using (registry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(currCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, otherOption))
				{
					var invoice = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID, NotReportableTaxID, StandardRateTaxID }, false, currCompany.GC_RN_NKCountryCode, ZString.Empty, false);
					invoice.AH_InvoiceDate = new ZDateTime(2021, 12, 31);
					invoice.AH_PostDate = new ZDateTime(2022, 1, 1);
					Factory.Save();
					AssertEquals(expectedTransactionReference, invoice.AH_TransactionReference);
				}
			}
		}

		[TestDate(2024, 05, 29)]
		public void TestAllocateComplianceSequenceNumberForInvoice_ComplianceNumberAllocationDateOptions_AP()
		{
			var currCompany = GlbCompany.CurrentCompany;
			var registry = AccountingMasterFilesRegistry.Instance;

			using (currCompany.TemporarilySetCountry(CountryCodes.Portugal))
			{
				ObjectCreator.SetupComplianceSequence(ZGuid.Empty, PortugalComplianceInfo.ComplianceSubTypeCodes.SBI, "jkl", 1, 100, 1);

				using (registry.ComplianceDocumentNumberAllocation_Payables.SetTemporaryValue(currCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
				using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(currCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.InvoiceDate.Code))
				using (registry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(currCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.PostDate.Code))
				{
					var invoice = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsPayable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID, NotReportableTaxID, StandardRateTaxID }, false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZString.Empty, false);
					invoice.AH_InvoiceDate = new ZDateTime(2020, 9, 11);
					invoice.IsSelfBillingInvoice = true;
					invoice.AH_SystemCreateTimeUtc = new ZDateTime(2020, 9, 11, 05, 30, 59);
					Factory.Save();
					AssertEquals("SBI jkl/000000001", invoice.AH_TransactionReference);
				}
			}
		}

		[TestDate(2023, 7, 1)]
		public void TestAllocateComplianceSequenceNumberForInvoice_ComplianceNumberAllocationDateOptions_WrongSubType()
		{
			var today = ZDateTime.Today;
			var currCompany = GlbCompany.CurrentCompany;
			var currBranch = GlbBranch.CurrentBranch;
			var registry = AccountingMasterFilesRegistry.Instance;

			var subType = PortugalComplianceInfo.ComplianceSubTypeCodes.TXI;
			var wrongSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.PTX;
			using (currCompany.TemporarilySetCountry(CountryCodes.Portugal))
			{
				new AccountingPeriodTestHelper(Factory).SetupPeriods();
				registry.ComplianceDocumentNumberAllocation_Receivables.SetValue(currCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);

				ObjectCreator.SetupComplianceSequence(ZGuid.Empty, wrongSubType, "xyz", 1, 100, 1, currCompany.PK, currBranch.PK,
					new ZDate(today.Year, 1, 1), new ZDate(today.Year, 12, 31));
				ObjectCreator.SetupComplianceSequence(ZGuid.Empty, subType, "abc", 1, 100, 1, currCompany.PK, currBranch.PK,
					new ZDate(today.Year, 1, 1), today.AddDays(-1).Date);
				ObjectCreator.SetupComplianceSequence(ZGuid.Empty, subType, "def", 1, 100, 1, currCompany.PK, currBranch.PK,
					today.Date, new ZDate(today.Year, 12, 31));

				AssertInvoiceUseTheCorrectSequenceBook(ComplianceNumberAllocationDateOptions.InvoiceDate.Code, subType + " abc/000000001");
				AssertInvoiceUseTheCorrectSequenceBook(ComplianceNumberAllocationDateOptions.PostDate.Code, subType + " def/000000001");
			}

			void AssertInvoiceUseTheCorrectSequenceBook(string complianceNumberAllocationDate, string expectedTransactionReference)
			{
				using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(currCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceNumberAllocationDate))
				{
					var invoice = ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INVx", ObjectCreator.EUR, 1, 100, 10, 100, 10, ObjectCreator.TestOrganisation, ObjectCreator.GLHeader1.PK,
						today, today.AddDays(30), today.AddDays(-1), false);
					invoice.Lines[0].AL_AT = ObjectCreator.GSTFREE1.PK;
					invoice.Lines[0].AL_Desc = "desc";

					invoice.AH_ComplianceSubType = wrongSubType;
					invoice.RunPreSaveValidation();
					AssertHasErrors("Error in Compliance Sequence on Posting", invoice.AH_ComplianceSubTypeInfo);

					invoice.AH_ComplianceSubType = subType;
					Factory.Save();
					AssertNoErrors("No more errors with right SubType", invoice);
					AssertEquals(expectedTransactionReference, invoice.AH_TransactionReference);
				}
			}
		}

		public void TestIsComplianceNumberAllocationMandatory_AR()
		{
			var currentCompany = GlbCompany.CurrentCompany;
			var anotherCompany = ObjectCreator.NonCurrentCompany;
			var registry = AccountingMasterFilesRegistry.Instance;

			var invoice1 = CreateInvoiceAndLineWithGST(typeof(ARInvoice), ObjectCreator.AALSHI);
			invoice1.AH_GC = anotherCompany.PK;

			using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.NoControl.Code))
			using (registry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.PostDate.Code))
			{
				using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(anotherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.PostDate.Code))
				using (registry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(anotherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.NoControl.Code))
				{
					AssertEquals(true, invoice1.IsComplianceNumberAllocationMandatory);
				}

				using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(anotherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.InvoiceDate.Code))
				using (registry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(anotherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.NoControl.Code))
				{
					AssertEquals(true, invoice1.IsComplianceNumberAllocationMandatory);
				}
			}

			using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.PostDate.Code))
			using (registry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(anotherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.NoControl.Code))
			{
				using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(anotherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.NoControl.Code))
				using (registry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(anotherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.PostDate.Code))
				{
					AssertEquals(false, invoice1.IsComplianceNumberAllocationMandatory);
				}
			}
		}

		public void TestIsComplianceNumberAllocationMandatory_AP()
		{
			var currentCompany = GlbCompany.CurrentCompany;
			var anotherCompany = ObjectCreator.NonCurrentCompany;
			var registry = AccountingMasterFilesRegistry.Instance;

			var invoice = CreateInvoiceAndLineWithGST(typeof(APInvoice), ObjectCreator.AALSHI);
			invoice.AH_GC = anotherCompany.PK;

			using (registry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.NoControl.Code))
			using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.PostDate.Code))
			{
				using (registry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(anotherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.PostDate.Code))
				using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(anotherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.NoControl.Code))
				{
					AssertEquals(true, invoice.IsComplianceNumberAllocationMandatory);
				}
			}

			using (registry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.PostDate.Code))
			using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(anotherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.NoControl.Code))
			{
				using (registry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(anotherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.NoControl.Code))
				using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(anotherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.PostDate.Code))
				{
					AssertEquals(false, invoice.IsComplianceNumberAllocationMandatory);
				}
			}
		}

		public void TestSignInvoicesRecursivelyInBackwardDirection_ExceedsMaxLimit()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				SetupInvoiceSequenceForSigning();
				var invoice6 = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID, NotReportableTaxID, StandardRateTaxID }, false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZString.Empty, false);
				invoice6.AH_InvoiceDate = new ZDateTime(2018, 09, 13);
				invoice6.AH_SystemCreateTimeUtc = new ZDateTime(2018, 09, 13, 8, 9, 23);
				Factory.Save();
				var invoice7 = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID, NotReportableTaxID, StandardRateTaxID }, false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZString.Empty, false);
				invoice7.AH_InvoiceDate = new ZDateTime(2018, 09, 13);
				invoice7.AH_SystemCreateTimeUtc = new ZDateTime(2018, 09, 13, 8, 9, 23);
				Factory.Save();
				var invoice8 = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID, NotReportableTaxID, StandardRateTaxID }, false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZString.Empty, false);
				invoice8.AH_InvoiceDate = new ZDateTime(2018, 09, 13);
				invoice8.AH_SystemCreateTimeUtc = new ZDateTime(2018, 09, 13, 8, 9, 23);
				Factory.Save();

				var result = invoice8.SignInvoicesRecursivelyInBackwardDirection();
				AssertEquals("The number of invoices that you want to sign exceeds the maximum limit of 5, please choose an invoice with a smaller compliance number and try again", result.errorMessage);
				Assert("Expect invoice 3 not signed", invoice3.AH_DigitalSignature_COMPRESSED.IsEmpty);
				Assert("Expect invoice 4 not signed", invoice4.AH_DigitalSignature_COMPRESSED.IsEmpty);
				Assert("Expect invoice 5 not signed", invoice5.AH_DigitalSignature_COMPRESSED.IsEmpty);
				Assert("Expect invoice 6 not signed", invoice6.AH_DigitalSignature_COMPRESSED.IsEmpty);
				Assert("Expect invoice 7 not signed", invoice7.AH_DigitalSignature_COMPRESSED.IsEmpty);
				Assert("Expect invoice 8 not signed", invoice8.AH_DigitalSignature_COMPRESSED.IsEmpty);
			}
		}

		public void TestSignInvoiceAndVerifySignature()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				{
					AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);
					ZGuid menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;
					var sequence = ObjectCreator.SetupComplianceSequence(menuPK, PortugalComplianceInfo.ComplianceSubTypeCodes.TXI, "abc", 1, 100, 1);
					Factory.Save();

					var invoice1 = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID, NotReportableTaxID, StandardRateTaxID }, false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZString.Empty, false);
					invoice1.AH_InvoiceDate = new ZDateTime(2018, 09, 11);
					invoice1.AH_SystemCreateTimeUtc = new ZDateTime(2018, 09, 11, 05, 30, 59);
					Factory.Save();
					AssertEquals("TXI", invoice1.AH_ComplianceSubType);
					AssertEquals("TXI abc/000000001", invoice1.AH_TransactionReference);
					AssertEquals("2018-09-11", invoice1.AH_InvoiceDate.ToString("yyyy-MM-dd"));
					AssertEquals("310.00", invoice1.AH_LocalTotal.ToString(2));
					Assert(!invoice1.AH_DigitalSignature_COMPRESSED.IsEmpty);
					var dataToSign = string.Format("2018-09-11;{0};TXI abc/000000001;310.00;", GetTransactionCreationLogTime(invoice1));
					var provider = new RSASecurityProvider();
					var signature = Convert.ToBase64String(invoice1.AH_DigitalSignature_COMPRESSED);
					Assert(provider.SignatureVerifier.VerifyHash(dataToSign, signature));

					var invoice2 = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID, NotReportableTaxID, StandardRateTaxID }, false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZString.Empty, false);
					invoice2.AH_InvoiceDate = new ZDateTime(2018, 09, 12);
					invoice2.AH_SystemCreateTimeUtc = new ZDateTime(2018, 09, 12, 11, 36, 45);
					Factory.Save();
					AssertEquals("TXI", invoice2.AH_ComplianceSubType);
					AssertEquals("TXI abc/000000002", invoice2.AH_TransactionReference);
					AssertEquals("2018-09-12", invoice2.AH_InvoiceDate.ToString("yyyy-MM-dd"));
					AssertEquals("310.00", invoice1.AH_LocalTotal.ToString(2));
					Assert(!invoice2.AH_DigitalSignature_COMPRESSED.IsEmpty);
					dataToSign = string.Format("2018-09-12;{0};TXI abc/000000002;310.00;{1}", GetTransactionCreationLogTime(invoice2), Convert.ToBase64String(invoice1.AH_DigitalSignature_COMPRESSED));
					signature = Convert.ToBase64String(invoice2.AH_DigitalSignature_COMPRESSED);
					Assert(provider.SignatureVerifier.VerifyHash(dataToSign, signature));
				}
			}
		}

		public void TestSignCreditNoteAndVerifySignature_StandaloneARCreditNote()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				{
					AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);
					var menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;
					var sequence = ObjectCreator.SetupComplianceSequence(menuPK, PortugalComplianceInfo.ComplianceSubTypeCodes.TCR, "abc", 1, 100, 1);
					Factory.Save();

					var creditNote = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, new AccTaxRate[] { ExemptTaxID, NotReportableTaxID, StandardRateTaxID }, false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZString.Empty, false);
					creditNote.AH_InvoiceDate = new ZDateTime(2018, 09, 11);
					creditNote.AH_SystemCreateTimeUtc = new ZDateTime(2018, 09, 11, 05, 30, 59);
					Factory.Save();
					AssertNotEquals("TCR", creditNote.AH_ComplianceSubType);
					AssertNotEquals("TCR abc/000000001", creditNote.AH_TransactionReference);
					AssertEquals("2018-09-11", creditNote.AH_InvoiceDate.ToString("yyyy-MM-dd"));
					AssertEquals("-310.00", creditNote.AH_LocalTotal.ToString(2));
					Assert("No signature expected", creditNote.AH_DigitalSignature_COMPRESSED.IsEmpty);
				}
			}
		}

		[TestDate(2018, 09, 11, 05, 33, 22)]
		public void TestSignCreditNoteAndVerifySignature_ReversingCreditNode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				{
					AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);
					ZGuid menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;
					var sequence = ObjectCreator.SetupComplianceSequence(menuPK, PortugalComplianceInfo.ComplianceSubTypeCodes.TCR, "abc", 1, 100, 1);
					Factory.Save();

					var invoice = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID, NotReportableTaxID, StandardRateTaxID }, false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZString.Empty, false);

					var reversalFactory = new ReversingFactory();
					var reversing = reversalFactory.NewReversing(invoice);
					reversing.Reverse();
					var creditNote = reversing.ReverseTransaction as ARCreditNote;
					creditNote.AH_InvoiceDate = new ZDateTime(2018, 09, 11);
					creditNote.AH_SystemCreateTimeUtc = new ZDateTime(2018, 09, 11, 05, 30, 59);
					Factory.Save();
					AssertEquals("TCR", creditNote.AH_ComplianceSubType);
					AssertEquals("TCR abc/000000001", creditNote.AH_TransactionReference);
					AssertEquals("2018-09-11", creditNote.AH_InvoiceDate.ToString("yyyy-MM-dd"));
					AssertEquals("-310.00", creditNote.AH_LocalTotal.ToString(2));
					Assert(!creditNote.AH_DigitalSignature_COMPRESSED.IsEmpty);
					var dataToSign = string.Format("2018-09-11;{0};TCR abc/000000001;310.00;", GetTransactionCreationLogTime(creditNote), new ZDateTime(2018, 9, 11, 5, 33, 22));
					var provider = new RSASecurityProvider();
					var signature = Convert.ToBase64String(creditNote.AH_DigitalSignature_COMPRESSED);
					Assert(provider.SignatureVerifier.VerifyHash(dataToSign, signature));
				}
			}
		}

		[TestDate(2018, 09, 11, 05, 33, 22)]
		public void TestSignCreditNoteAndVerifySignature_AmendingARCreditNote()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				{
					AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);
					ZGuid menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;
					var sequence = ObjectCreator.SetupComplianceSequence(menuPK, PortugalComplianceInfo.ComplianceSubTypeCodes.TCR, "abc", 1, 100, 1);
					Factory.Save();

					var invoice = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID, NotReportableTaxID, StandardRateTaxID }, false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZString.Empty, false);

					var creditNote = (invoice as IAmending)?.GenerateAmendingTransaction(TransactionTypes.CreditNote) as InvoicingBase;
					AssertNotNull(creditNote);
					creditNote.AH_InvoiceDate = new ZDateTime(2018, 09, 11);
					creditNote.AH_SystemCreateTimeUtc = new ZDateTime(2018, 09, 11, 05, 30, 59);
					Factory.Save();
					AssertEquals("TCR", creditNote.AH_ComplianceSubType);
					AssertEquals("TCR abc/000000001", creditNote.AH_TransactionReference);
					AssertEquals("2018-09-11", creditNote.AH_InvoiceDate.ToString("yyyy-MM-dd"));
					AssertEquals("-310.00", creditNote.AH_LocalTotal.ToString(2));
					AssertEquals("o7Cvm09fv8o9ZIJxTXTVLt2Mk5LCS3ZBWxRifE5j/1mW3/ax58/nMG0iGQhL1ogxE5ItqL/MkehpdYZzSsaWS7gRIgASKdfk6dOWUX04hCT0nq70L4WBWr73nKt6Jpk6r+a5FuRtFZXJWYcouqpKufVSk+awFsArsaAbkttpMhw=", Convert.ToBase64String(creditNote.AH_DigitalSignature_COMPRESSED));
					Assert(!creditNote.AH_DigitalSignature_COMPRESSED.IsEmpty);

					var dataToSign = string.Format("2018-09-11;{0};TCR abc/000000001;310.00;", GetTransactionCreationLogTime(creditNote), new ZDateTime(2018, 9, 11, 5, 33, 22));
					var provider = new RSASecurityProvider();
					var signature = Convert.ToBase64String(creditNote.AH_DigitalSignature_COMPRESSED);
					Assert(provider.SignatureVerifier.VerifyHash(dataToSign, signature));
				}
			}
		}

		[TestDate(2018, 10, 10)]
		public void TestSignInvoiceWithRSASignatureInSequence()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "PTLIS";
				AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);
				var menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;
				var sequence = ObjectCreator.SetupComplianceSequence(menuPK, PortugalComplianceInfo.ComplianceSubTypeCodes.TXI, "abc", 1, 100, 1);
				Factory.Save();

				var invoice1 = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID, NotReportableTaxID, StandardRateTaxID }, false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZString.Empty, false);
				AssertEquals(ZString.Empty, invoice1.AH_ComplianceSubType);
				Assert(invoice1.AH_DigitalSignature_COMPRESSED.IsEmpty);
				Factory.Save();
				AssertEquals("TXI", invoice1.AH_ComplianceSubType);
				AssertEquals("TXI abc/000000001", invoice1.AH_TransactionReference);
				Assert(!invoice1.AH_DigitalSignature_COMPRESSED.IsEmpty);
				AssertEquals("Expect 1st invoice signed successfully", 128, invoice1.AH_DigitalSignature_COMPRESSED.Length);

				var invoice2 = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID, NotReportableTaxID, StandardRateTaxID }, false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZString.Empty, false);
				invoice2.AH_PostDate = invoice2.AH_PostDate.AddHours(1);
				AssertEquals(ZString.Empty, invoice2.AH_ComplianceSubType);
				Assert(invoice2.AH_DigitalSignature_COMPRESSED.IsEmpty);
				Factory.Save();
				AssertEquals("TXI", invoice2.AH_ComplianceSubType);
				AssertEquals("TXI abc/000000002", invoice2.AH_TransactionReference);
				Assert(!invoice2.AH_DigitalSignature_COMPRESSED.IsEmpty);
				AssertEquals("Expect 2nd invoice signed successfully as previous invoice has valid signature", 128, invoice2.AH_DigitalSignature_COMPRESSED.Length);

				var invoice3 = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID, NotReportableTaxID, StandardRateTaxID }, false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZString.Empty, false);
				invoice3.AH_PostDate = invoice3.AH_PostDate.AddHours(2);
				AssertEquals(ZString.Empty, invoice3.AH_ComplianceSubType);
				Assert(invoice3.AH_DigitalSignature_COMPRESSED.IsEmpty);
				invoice2.AH_DigitalSignature_COMPRESSED = null;
				Factory.Save();
				AssertEquals("TXI", invoice3.AH_ComplianceSubType);
				AssertEquals("TXI abc/000000003", invoice3.AH_TransactionReference);
				Assert("Expect 3rd invoice signing failed due to previous invoice has blank signature", invoice3.AH_DigitalSignature_COMPRESSED.IsEmpty);

				var invoice4 = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID, NotReportableTaxID, StandardRateTaxID }, false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZString.Empty, false);
				invoice4.AH_PostDate = invoice4.AH_PostDate.AddHours(3);
				AssertEquals(ZString.Empty, invoice4.AH_ComplianceSubType);
				Assert(invoice4.AH_DigitalSignature_COMPRESSED.IsEmpty);
				invoice3.AH_TransactionReference = ZString.Empty;
				Factory.Save();
				AssertEquals("TXI", invoice4.AH_ComplianceSubType);
				AssertEquals("TXI abc/000000004", invoice4.AH_TransactionReference);
				Assert("Expect 4th invoice signing failed due to failed to allocate the previous invoice", invoice4.AH_DigitalSignature_COMPRESSED.IsEmpty);
			}
		}

		public void TestSignSelfBillingInvoiceAndVerifySignature()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				{
					AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Payables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);
					ZGuid menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;
					var sequence = ObjectCreator.SetupComplianceSequence(menuPK, PortugalComplianceInfo.ComplianceSubTypeCodes.SBI, "abc", 1, 100, 1);
					Factory.Save();

					var invoice1 = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsPayable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID, NotReportableTaxID, StandardRateTaxID }, false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZString.Empty, false);
					invoice1.AH_InvoiceDate = new ZDateTime(2018, 09, 11);
					invoice1.IsSelfBillingInvoice = true;
					invoice1.AH_SystemCreateTimeUtc = new ZDateTime(2018, 09, 11, 05, 30, 59);
					Factory.Save();
					AssertEquals("SBI", invoice1.AH_ComplianceSubType);
					AssertEquals("SBI abc/000000001", invoice1.AH_TransactionReference);
					Assert("IsSelfBillingInvoice", invoice1.IsSelfBillingInvoice);
					AssertEquals("2018-09-11", invoice1.AH_InvoiceDate.ToString("yyyy-MM-dd"));
					AssertEquals("-310.00", invoice1.AH_LocalTotal.ToString(2));
					Assert(!invoice1.AH_DigitalSignature_COMPRESSED.IsEmpty);
					var dataToSign = string.Format("2018-09-11;{0};SBI abc/000000001;310.00;", GetTransactionCreationLogTime(invoice1));
					var provider = new RSASecurityProvider();
					var signature = Convert.ToBase64String(invoice1.AH_DigitalSignature_COMPRESSED);
					Assert(provider.SignatureVerifier.VerifyHash(dataToSign, signature));

					var invoice2 = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsPayable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID, NotReportableTaxID, StandardRateTaxID }, false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZString.Empty, false);
					invoice2.AH_InvoiceDate = new ZDateTime(2018, 09, 12);
					invoice2.IsSelfBillingInvoice = true;
					invoice2.AH_SystemCreateTimeUtc = new ZDateTime(2018, 09, 12, 11, 36, 45);
					Factory.Save();
					AssertEquals("SBI", invoice2.AH_ComplianceSubType);
					Assert("IsSelfBillingInvoice", invoice2.IsSelfBillingInvoice);
					AssertEquals("SBI abc/000000002", invoice2.AH_TransactionReference);
					AssertEquals("2018-09-12", invoice2.AH_InvoiceDate.ToString("yyyy-MM-dd"));
					AssertEquals("-310.00", invoice2.AH_LocalTotal.ToString(2));
					Assert(!invoice2.AH_DigitalSignature_COMPRESSED.IsEmpty);
					dataToSign = string.Format("2018-09-12;{0};SBI abc/000000002;310.00;{1}", GetTransactionCreationLogTime(invoice2), Convert.ToBase64String(invoice1.AH_DigitalSignature_COMPRESSED));
					signature = Convert.ToBase64String(invoice2.AH_DigitalSignature_COMPRESSED);
					Assert(provider.SignatureVerifier.VerifyHash(dataToSign, signature));

					var invoice3 = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsPayable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID, NotReportableTaxID, StandardRateTaxID }, false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZString.Empty, false);
					invoice3.AH_InvoiceDate = new ZDateTime(2018, 09, 12);
					AssertEquals("Pre-condition: IsSelfBillingInvoice", false, invoice3.IsSelfBillingInvoice);
					invoice3.AH_SystemCreateTimeUtc = new ZDateTime(2018, 09, 12, 11, 36, 45);
					Factory.Save();
					AssertEquals("IsSelfBillingInvoice", false, invoice3.IsSelfBillingInvoice);
					Assert("AH_ComplianceSubType.IsEmpty", invoice3.AH_ComplianceSubType.IsEmpty);
					Assert("-AH_TransactionReference.IsEmpty", invoice3.AH_TransactionReference.IsEmpty);
					Assert("No signature if not Self Billing", invoice3.AH_DigitalSignature_COMPRESSED.IsEmpty);
				}
			}
		}

		public void TestSignSelfBillingCreditNoteAndVerifySignature()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				{
					AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Payables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);
					ZGuid menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;
					var sequence = ObjectCreator.SetupComplianceSequence(menuPK, PortugalComplianceInfo.ComplianceSubTypeCodes.SBC, "abc", 1, 100, 1);
					Factory.Save();

					var originalInvoice1 = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsPayable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID, NotReportableTaxID, StandardRateTaxID }, false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZString.Empty, false);
					originalInvoice1.IsSelfBillingInvoice = true;

					var originalInvoice2 = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsPayable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID, NotReportableTaxID, StandardRateTaxID }, false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZString.Empty, false);
					originalInvoice2.IsSelfBillingInvoice = true;

					var originalInvoice3 = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsPayable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID, NotReportableTaxID, StandardRateTaxID }, false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZString.Empty, false);
					Factory.Save();

					var reversalFactory = new ReversingFactory();
					var reversing = reversalFactory.NewReversing(originalInvoice1);
					reversing.Reverse();
					var creditNote1 = reversing.ReverseTransaction as APCreditNote;
					creditNote1.IsSelfBillingInvoice = true;
					creditNote1.AH_TransactionNum = "APCRD0001";
					creditNote1.AH_InvoiceDate = new ZDateTime(2018, 09, 11);
					creditNote1.AH_SystemCreateTimeUtc = new ZDateTime(2018, 09, 11, 05, 30, 59);
					Factory.Save();
					AssertEquals("SBC", creditNote1.AH_ComplianceSubType);
					AssertEquals("SBC abc/000000001", creditNote1.AH_TransactionReference);
					Assert("IsSelfBillingInvoice", creditNote1.IsSelfBillingInvoice);
					AssertEquals("2018-09-11", creditNote1.AH_InvoiceDate.ToString("yyyy-MM-dd"));
					AssertEquals("310.00", creditNote1.AH_LocalTotal.ToString(2));
					Assert(!creditNote1.AH_DigitalSignature_COMPRESSED.IsEmpty);
					var dataToSign = string.Format("2018-09-11;{0};SBC abc/000000001;310.00;", GetTransactionCreationLogTime(creditNote1));
					var provider = new RSASecurityProvider();
					var signature = Convert.ToBase64String(creditNote1.AH_DigitalSignature_COMPRESSED);
					Assert(provider.SignatureVerifier.VerifyHash(dataToSign, signature));

					reversalFactory = new ReversingFactory();
					reversing = reversalFactory.NewReversing(originalInvoice2);
					reversing.Reverse();
					var creditNote2 = reversing.ReverseTransaction as APCreditNote;
					creditNote2.IsSelfBillingInvoice = true;
					creditNote2.AH_TransactionNum = "APCRD0002";
					creditNote2.AH_InvoiceDate = new ZDateTime(2018, 09, 12);
					creditNote2.AH_SystemCreateTimeUtc = new ZDateTime(2018, 09, 12, 11, 36, 45);
					Factory.Save();
					AssertEquals("SBC", creditNote2.AH_ComplianceSubType);
					Assert("IsSelfBillingInvoice", creditNote2.IsSelfBillingInvoice);
					AssertEquals("SBC abc/000000002", creditNote2.AH_TransactionReference);
					AssertEquals("2018-09-12", creditNote2.AH_InvoiceDate.ToString("yyyy-MM-dd"));
					AssertEquals("310.00", creditNote2.AH_LocalTotal.ToString(2));
					Assert(!creditNote2.AH_DigitalSignature_COMPRESSED.IsEmpty);
					dataToSign = string.Format("2018-09-12;{0};SBC abc/000000002;310.00;{1}", GetTransactionCreationLogTime(creditNote2), Convert.ToBase64String(creditNote1.AH_DigitalSignature_COMPRESSED));
					signature = Convert.ToBase64String(creditNote2.AH_DigitalSignature_COMPRESSED);
					Assert(provider.SignatureVerifier.VerifyHash(dataToSign, signature));

					reversalFactory = new ReversingFactory();
					reversing = reversalFactory.NewReversing(originalInvoice3);
					reversing.Reverse();
					var creditNote3 = reversing.ReverseTransaction as APCreditNote;
					creditNote3.AH_TransactionNum = "APCRD0003";
					creditNote3.AH_InvoiceDate = new ZDateTime(2018, 09, 12);
					creditNote3.AH_SystemCreateTimeUtc = new ZDateTime(2018, 09, 12, 11, 36, 45);
					Factory.Save();
					AssertEquals("IsSelfBillingInvoice", false, creditNote3.IsSelfBillingInvoice);
					Assert("AH_ComplianceSubType.IsEmpty", creditNote3.AH_ComplianceSubType.IsEmpty);
					Assert("AH_TransactionReference.IsEmpty", creditNote3.AH_TransactionReference.IsEmpty);
					Assert("No signature if not Self Billing", creditNote3.AH_DigitalSignature_COMPRESSED.IsEmpty);

					var creditNote4 = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, new AccTaxRate[] { ExemptTaxID, NotReportableTaxID, StandardRateTaxID }, false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZString.Empty, false);
					creditNote4.AH_InvoiceDate = new ZDateTime(2018, 09, 12);
					creditNote4.IsSelfBillingInvoice = true;
					creditNote4.AH_SystemCreateTimeUtc = new ZDateTime(2018, 09, 12, 11, 36, 45);
					Factory.Save();
					Assert("IsSelfBillingInvoice", creditNote4.IsSelfBillingInvoice);
					Assert("AH_ComplianceSubType.IsEmpty as it is not reversal and does not match any Compliance Sub Type configuration", creditNote4.AH_ComplianceSubType.IsEmpty);
					Assert("AH_TransactionReference.IsEmpty", creditNote4.AH_TransactionReference.IsEmpty);
					Assert("No signature if not a reversal", creditNote4.AH_DigitalSignature_COMPRESSED.IsEmpty);

					var creditNote5 = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, new AccTaxRate[] { ExemptTaxID, NotReportableTaxID, StandardRateTaxID }, false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZString.Empty, false);
					creditNote5.AH_InvoiceDate = new ZDateTime(2018, 09, 12);
					creditNote5.IsSelfBillingInvoice = true;
					creditNote5.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.SBC; // Simulate manual setting Compliance Sub Type
					creditNote5.AH_SystemCreateTimeUtc = new ZDateTime(2018, 09, 12, 11, 36, 45);
					Factory.Save();
					AssertEquals("SBC", creditNote5.AH_ComplianceSubType);
					Assert("IsSelfBillingInvoice", creditNote5.IsSelfBillingInvoice);
					Assert("AH_TransactionReference is not empty", !creditNote5.AH_TransactionReference.IsEmpty);
					Assert(!creditNote5.AH_DigitalSignature_COMPRESSED.IsEmpty);
					AssertEquals("SBC abc/000000003", creditNote5.AH_TransactionReference);
					AssertEquals("2018-09-12", creditNote5.AH_InvoiceDate.ToString("yyyy-MM-dd"));
					AssertEquals("310.00", creditNote5.AH_LocalTotal.ToString(2));
					dataToSign = string.Format("2018-09-12;{0};SBC abc/000000003;310.00;{1}", GetTransactionCreationLogTime(creditNote5), Convert.ToBase64String(creditNote2.AH_DigitalSignature_COMPRESSED));
					signature = Convert.ToBase64String(creditNote5.AH_DigitalSignature_COMPRESSED);
					Assert(provider.SignatureVerifier.VerifyHash(dataToSign, signature));
				}
			}
		}

		public void TestSignAfterCopyNewComplianceSequence()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);
				var menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;
				var sequence = ObjectCreator.SetupComplianceSequence(menuPK, PortugalComplianceInfo.ComplianceSubTypeCodes.TXI, "CS1", 1, 100, 1);
				Factory.Save();

				var invoice1 = CreateNewInvoice();
				var invoice2 = CreateNewInvoice();
				var invoice3 = CreateNewInvoice();
				Factory.Save();

				AssertEquals("TXI CS1/000000001", invoice1.AH_TransactionReference);
				Assert(!invoice1.AH_DigitalSignature_COMPRESSED.IsEmpty);
				AssertEquals("TXI CS1/000000002", invoice2.AH_TransactionReference);
				Assert(!invoice2.AH_DigitalSignature_COMPRESSED.IsEmpty);
				AssertEquals("TXI CS1/000000003", invoice3.AH_TransactionReference);
				Assert(!invoice3.AH_DigitalSignature_COMPRESSED.IsEmpty);

				ZExceptionReporting.ProcessWithConcurrencyHandling(() =>
				{
					sequence.XD_IsActive = false;
					Factory.Save();
				}, null);

				var newSequence = (AccComplianceSequence)((ITemplateCopyable)sequence).TemplateCopy();
				newSequence.XD_Prefix = "CS2";
				newSequence.XD_IsActive = true;
				Factory.Save();

				var invoice4 = CreateNewInvoice();
				Factory.Save();

				AssertEquals("TXI CS2/000000004", invoice4.AH_TransactionReference);
				Assert(!invoice4.AH_DigitalSignature_COMPRESSED.IsEmpty);
			}

			InvoicingBase CreateNewInvoice()
			{
				return CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID, NotReportableTaxID, StandardRateTaxID }, false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZString.Empty, false);
			}
		}

		string GetTransactionCreationLogTime(InvoicingBase transaction, ZDateTime? expectedTime = null)
		{
			var log = Factory.LoadTop1<StmALog>(new ZQuery(StmALogSchema.SL_Parent, transaction.PK).AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.AddedARecordToTheSystemCode));
			AssertNotNull(log);
			if (expectedTime.HasValue)
			{
				AssertEquals(expectedTime.Value, log.SL_EventTime);
			}
			return log.SL_EventTime.ToString("yyyy-MM-ddTHH:mm:ss");
		}

		public void TestAPComplianceNumberAllocation()
		{
			var italyCompany = ObjectCreator.CreateCompanyAndBranch("ITMIL");
			Factory.Save();

			using (ObjectCreator.SwitchEnvToCompany(italyCompany))
			{
				Assert("There should be some default configurations for Italy", AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.Value.Count > 0);

				var menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;
				var sequence = ObjectCreator.SetupComplianceSequence(menuPK, ItalyComplianceInfo.ComplianceSubTypeCodes.API, "-", 1, 100, 25);
				sequence.Factory.Save();

				AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Payables.SetValue(italyCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print);
				AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(italyCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);

				var taxRate = new AccTaxRate[] { ExemptTaxID, NotReportableTaxID, StandardRateTaxID };

				var invoice = AssertComplianceSubTypeAllocatedCorrectly("Creditor Invoice From Italian Creditor", ItalyComplianceInfo.ComplianceSubTypeCodes.API, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, taxRate, false, CountryCodes.Italy, ZString.Empty, false);
				Assert("AP Allocation Registry is Enabled on Printing, Compliance Number should be empty on Posting", invoice.AH_TransactionReference.IsEmpty);

				AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Payables.SetValue(italyCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);

				invoice = AssertComplianceSubTypeAllocatedCorrectly("Creditor Credit Note From Italian Creditor", ItalyComplianceInfo.ComplianceSubTypeCodes.API, LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, taxRate, false, CountryCodes.Italy, ZString.Empty, false);
				Assert("AP Allocation Registry is Enabled on Posting, Compliance Number should not be empty", !invoice.AH_TransactionReference.IsEmpty);
			}
		}

		#region China related tests

		public void TestUpdateComplianceSubTypeForChina()
		{
			var chinaCompany = ObjectCreator.CreateNewCompany("CHN", Core.Constants.CountryCodes.China);
			var chinaBranch = ObjectCreator.CreateNewBranch(chinaCompany, "SHA");
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), chinaBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Assert("There should be some default configurations for China", AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.Value.Count > 0);

				AssertComplianceSubTypeForCN(ChinaComplianceInfo.ComplianceSubTypeCodes.TXA, typeof(ARInvoice), "AROrg01", ObjectCreator.GST1, new string[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.ChinaCodeTypes.VAG });
				AssertComplianceSubTypeForCN(ChinaComplianceInfo.ComplianceSubTypeCodes.TXB, typeof(ARInvoice), "AROrg02", ObjectCreator.GSTFREE1, new string[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.ChinaCodeTypes.VAG });
				AssertComplianceSubTypeForCN(ChinaComplianceInfo.ComplianceSubTypeCodes.TXB, typeof(ARInvoice), "AROrg03", ObjectCreator.GST1, new string[] { OrgCusCode.CodeTypes.VATCode });
				AssertComplianceSubTypeForCN(ZString.Empty, typeof(ARInvoice), "AROrg04", ObjectCreator.GST1, Array.Empty<string>());

				AssertComplianceSubTypeForCN(ChinaComplianceInfo.ComplianceSubTypeCodes.TXA, typeof(APInvoice), "APOrg01", ObjectCreator.GST1, new string[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.ChinaCodeTypes.VAG });
				AssertComplianceSubTypeForCN(ChinaComplianceInfo.ComplianceSubTypeCodes.TXB, typeof(APInvoice), "APOrg02", ObjectCreator.GSTFREE1, new string[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.ChinaCodeTypes.VAG });
				AssertComplianceSubTypeForCN(ChinaComplianceInfo.ComplianceSubTypeCodes.TXB, typeof(APInvoice), "APOrg03", ObjectCreator.GST1, new string[] { OrgCusCode.CodeTypes.VATCode });
				AssertComplianceSubTypeForCN(ZString.Empty, typeof(APInvoice), "APOrg04", ObjectCreator.GST1, Array.Empty<string>());
			}
		}

		void AssertComplianceSubTypeForCN(ZString expectedComplianceSubType, Type invoiceType, ZString orgCode, AccTaxRate taxID, string[] customCodes)
		{
			var org = ObjectCreator.CreateOrgHeader(orgCode, true, true, true, false, true, false);
			foreach (var code in customCodes)
			{
				ObjectCreator.SetCustomsCodeForOrgHeader(org, code, Core.Constants.CountryCodes.China, "1111111");
			}
			var invoice = ObjectCreator.CreateInvoice(invoiceType, RandomNumberGenerator.Next(10000000).ToString(), ObjectCreator.CNY, 1M, org);
			var line = ObjectCreator.CreateInvoiceLine(invoice, 100M, ObjectCreator.CNY, 1M, setTaxes: false);
			line.AL_AT = taxID.PK;
			Factory.Save();
			AssertEquals(expectedComplianceSubType, invoice.AH_ComplianceSubType);
		}

		#endregion

		#region Indonesia related tests

		public void TestUpdateComplianceSubTypeForIndonesia()
		{
			var indomesiaCompany = ObjectCreator.CreateNewCompany("IDN", Core.Constants.CountryCodes.Indonesia);
			var indomesiaBranch = ObjectCreator.CreateNewBranch(indomesiaCompany, "YJD");
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), indomesiaBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Assert("There should be some default configurations for Indonesia", AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.Value.Count > 0);

				AssertComplianceSubTypeForID(IndonesiaComplianceInfo.ComplianceSubTypeCodes.TXI, typeof(ARInvoice), "AROrg01", ObjectCreator.GST1, new string[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.ChinaCodeTypes.VAG });
				AssertComplianceSubTypeForID(IndonesiaComplianceInfo.ComplianceSubTypeCodes.TXI, typeof(ARInvoice), "AROrg02", ObjectCreator.GST1, Array.Empty<string>());

				AssertComplianceSubTypeForID(ZString.Empty, typeof(APInvoice), "APOrg01", ObjectCreator.GST1, new string[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.ChinaCodeTypes.VAG });
				AssertComplianceSubTypeForID(ZString.Empty, typeof(APInvoice), "APOrg02", ObjectCreator.GST1, Array.Empty<string>());
			}
		}

		void AssertComplianceSubTypeForID(ZString expectedComplianceSubType, Type invoiceType, ZString orgCode, AccTaxRate taxID, string[] customCodes)
		{
			var org = ObjectCreator.CreateOrgHeader(orgCode, true, true, true, false, true, false);
			foreach (var code in customCodes)
			{
				ObjectCreator.SetCustomsCodeForOrgHeader(org, code, Core.Constants.CountryCodes.Indonesia, "1111111");
			}
			var invoice = ObjectCreator.CreateInvoice(invoiceType, RandomNumberGenerator.Next(10000000).ToString(), ObjectCreator.IDR, 1M, org);
			var line = ObjectCreator.CreateInvoiceLine(invoice, 100M, ObjectCreator.IDR, 1M, setTaxes: false);
			line.AL_AT = taxID.PK;
			Factory.Save();
			AssertEquals(expectedComplianceSubType, invoice.AH_ComplianceSubType);
		}

		#endregion

		#region Portugal related tests

		[TestDate(2021, 8, 3, 10, 0, 0)]
		public void TestInvoiceDateLessThanPreviousExceptionOnSaving()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
			using (AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AR.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.NoControl.Code))
			{
				SetUpComplianceSequenceBook();

				var invoice1 = ObjectCreator.SetupComplianceInvoice("TXI", false, ZDateTime.Now.AddDays(1));
				Factory.Save();

				AssertEquals("TXI ABC/000000001", invoice1.AH_TransactionReference);

				var invoice2 = ObjectCreator.SetupComplianceInvoice("TXI", false, ZDateTime.Now);

				var ex = AssertExceptionThrown<InvoiceDateLessThanPreviousException>(() => Factory.Save());
				var expectedMessage = "Invoice date must be equal or higher than previous document.";
				AssertEquals(expectedMessage, ex.UserFriendlyMessage);
				AssertEquals(expectedMessage, ex.Message);
			}
		}

		public void TestPostDateLessThanPreviousExceptionOnSaving()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
			{
				SetUpComplianceSequenceBook();

				var invoice1 = ObjectCreator.SetupComplianceInvoice("TXI", false);
				invoice1.AH_PostDate = ZDateTime.Now.AddDays(1);
				Factory.Save();

				var invoice2 = ObjectCreator.SetupComplianceInvoice("TXI", false);
				invoice2.AH_PostDate = ZDateTime.Now;

				try
				{
					Factory.Save();

					Fail("Should never reach this line");
				}
				catch (PostDateLessThanPreviousException ex)
				{
					var expectedMessage = "Post date must be equal or higher than previous document.";
					AssertEquals(expectedMessage, ex.UserFriendlyMessage);
					AssertEquals(expectedMessage, ex.Message);
				}
			}
		}

		public void TestInvoiceDateGreaterThanPostDateExceptionOnSaving()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
			{
				SetUpComplianceSequenceBook();

				var invoice1 = ObjectCreator.SetupComplianceInvoice("TXI", false);
				invoice1.AH_InvoiceDate = ZDateTime.Now.AddDays(1);
				invoice1.AH_PostDate = ZDateTime.Now;

				try
				{
					Factory.Save();

					Fail("Should never reach this line");
				}
				catch (InvoiceDateGreaterThanPostDateException ex)
				{
					var expectedMessage = "Invoice date must be equal or lower than post date.";
					AssertEquals(expectedMessage, ex.UserFriendlyMessage);
					AssertEquals(expectedMessage, ex.Message);
				}
			}
		}

		public void TestHasNonCMTChargeZeroAmountLineExceptionOnSaving()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.VietNam);
			SetUpComplianceSequenceBook();

			var invoice1 = ObjectCreator.SetupComplianceInvoice("TXI", false);
			ObjectCreator.CreateARInvoiceLine(invoice1, null, ObjectCreator.CC1, ObjectCreator.VND, 1m, "ArInvoice Line", 0m);

			var collection = new ComplianceSubTypeAllocationOverrideConfigurationCollection(null, Factory, Constants.CountryCodes.VietNam);
			var complianceSubTypeAllocationOverrideConfiguration = collection.AddNew();
			complianceSubTypeAllocationOverrideConfiguration.SubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
			complianceSubTypeAllocationOverrideConfiguration.BranchPK = Env.CurrentBranchPK;
			complianceSubTypeAllocationOverrideConfiguration.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;

			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationOverride_Receivables.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, collection))
			{
				try
				{
					Factory.Save();

					Fail("Should never reach this line");
				}
				catch (HasNonCMTChargeZeroAmountLineException ex)
				{
					var expectedMessage = "Compliance Number cannot be allocated to this transaction as it contains a Zero Amount Non-Comment Charge Line.";
					AssertEquals(expectedMessage, ex.UserFriendlyMessage);
					AssertEquals(expectedMessage, ex.Message);
				}
			}
		}

		ZGuid SetUpComplianceSequenceBook()
		{
			var menuPK = ObjectCreator.SetupComplianceMenuAndPivot("Govt Compliance Inv");
			var book = ObjectCreator.SetupComplianceSequence(menuPK, "TXI", "ABC", 1, 100, 1);
			book.XD_PrintingAuthorizationNumber = "Test";
			Factory.Save();

			return book.PK;
		}

		#endregion

		public void TestUpdateComplianceSubTypeForItaly()
		{
			var italyCompany = ObjectCreator.CreateCompanyAndBranch("ITMIL");
			Factory.Save();

			using (ObjectCreator.SwitchEnvToCompany(italyCompany))
			{
				Assert("There should be some default configurations for Italy", AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.Value.Count > 0);

				var taxIDsToUse = new AccTaxRate[] { ExemptTaxID, NotReportableTaxID };
				AssertComplianceSubTypeAllocatedCorrectly("Invoice To Italy Debtor", ItalyComplianceInfo.ComplianceSubTypeCodes.ARI, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, taxIDsToUse, false, CountryCodes.Italy, CountryCodes.Italy, false);
				AssertComplianceSubTypeAllocatedCorrectly("Credit Note Note To Italy Debtor", ItalyComplianceInfo.ComplianceSubTypeCodes.ARI, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, taxIDsToUse, false, CountryCodes.Italy, CountryCodes.Italy, false);

				AssertComplianceSubTypeAllocatedCorrectly("Invoice To Not Italy, EU Debtor", ItalyComplianceInfo.ComplianceSubTypeCodes.ARI, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, taxIDsToUse, false, TaxRegRuleCodes.EUExcludingLoginCountry, TaxRegRuleCodes.EUExcludingLoginCountry, false);
				AssertComplianceSubTypeAllocatedCorrectly("Credit Note To Not Italy, EU Debtor", ItalyComplianceInfo.ComplianceSubTypeCodes.ARI, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, taxIDsToUse, false, TaxRegRuleCodes.EUExcludingLoginCountry, TaxRegRuleCodes.EUExcludingLoginCountry, false);

				AssertComplianceSubTypeAllocatedCorrectly("Invoice To Non EU Debtor", ItalyComplianceInfo.ComplianceSubTypeCodes.ARI, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, taxIDsToUse, false, TaxRegRuleCodes.NotEU, TaxRegRuleCodes.NotEU, false);
				AssertComplianceSubTypeAllocatedCorrectly("Credit Note To Non EU Debtor", ItalyComplianceInfo.ComplianceSubTypeCodes.ARI, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, taxIDsToUse, false, TaxRegRuleCodes.NotEU, TaxRegRuleCodes.NotEU, false);

				AssertComplianceSubTypeAllocatedCorrectly("Self-Billing Invoice To Italy Debtor", ItalyComplianceInfo.ComplianceSubTypeCodes.ARS, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, taxIDsToUse, true, CountryCodes.Italy, CountryCodes.Italy, false);
				AssertComplianceSubTypeAllocatedCorrectly("Self-Billing Credit Note To Italy Debtor", ItalyComplianceInfo.ComplianceSubTypeCodes.ARS, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, taxIDsToUse, true, CountryCodes.Italy, CountryCodes.Italy, false);

				AssertComplianceSubTypeAllocatedCorrectly("Self-Billing Invoice To Not Italy, EU Debtor", ItalyComplianceInfo.ComplianceSubTypeCodes.ARS, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, taxIDsToUse, true, TaxRegRuleCodes.EUExcludingLoginCountry, TaxRegRuleCodes.EUExcludingLoginCountry, false);
				AssertComplianceSubTypeAllocatedCorrectly("Self-Billing Credit Note To Not Italy, EU Debtor", ItalyComplianceInfo.ComplianceSubTypeCodes.ARS, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, taxIDsToUse, true, TaxRegRuleCodes.EUExcludingLoginCountry, TaxRegRuleCodes.EUExcludingLoginCountry, false);

				AssertComplianceSubTypeAllocatedCorrectly("Self-Billing Invoice To Non EU Debtor", ItalyComplianceInfo.ComplianceSubTypeCodes.ARS, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, taxIDsToUse, true, TaxRegRuleCodes.NotEU, TaxRegRuleCodes.NotEU, false);
				AssertComplianceSubTypeAllocatedCorrectly("Self-Billing Credit Note To Non EU Debtor", ItalyComplianceInfo.ComplianceSubTypeCodes.ARS, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, taxIDsToUse, true, TaxRegRuleCodes.NotEU, TaxRegRuleCodes.NotEU, false);

				AssertComplianceSubTypeAllocatedCorrectly("Creditor Invoice From Italian Creditor", ItalyComplianceInfo.ComplianceSubTypeCodes.API, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID, NotReportableTaxID, StandardRateTaxID }, false, CountryCodes.Italy, ZString.Empty, false);
				AssertComplianceSubTypeAllocatedCorrectly("Creditor Credit Note From Italian Creditor", ItalyComplianceInfo.ComplianceSubTypeCodes.API, LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, new AccTaxRate[] { ExemptTaxID, NotReportableTaxID, StandardRateTaxID }, false, CountryCodes.Italy, ZString.Empty, false);

				AssertComplianceSubTypeAllocatedCorrectly("Invoices with a mixture of Reverse Charge and Normal Tax IDs", ItalyComplianceInfo.ComplianceSubTypeCodes.API, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, new AccTaxRate[] { ReverseChargeStandardTaxID, StandardRateTaxID }, false, CountryCodes.Italy, ZString.Empty, false);
				AssertComplianceSubTypeAllocatedCorrectly("Credit Notes with a mixture of Reverse Charge and Normal Tax IDs", ItalyComplianceInfo.ComplianceSubTypeCodes.API, LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, new AccTaxRate[] { ReverseChargeStandardTaxID, StandardRateTaxID }, false, CountryCodes.Italy, ZString.Empty, false);

				AssertComplianceSubTypeAllocatedCorrectly("Invoices with a mixture of Reverse Charge and Normal Tax IDs from Non-Italy Orgs Can't Be Defaulted", ZString.Empty, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, new AccTaxRate[] { ReverseChargeStandardTaxID, StandardRateTaxID }, false, TaxRegRuleCodes.EUExcludingLoginCountry, ZString.Empty, false);
				AssertComplianceSubTypeAllocatedCorrectly("Credit Notes with a mixture of Reverse Charge and Normal Tax IDs from Non-Italy Orgs Can't Be Defaulted", ZString.Empty, LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, new AccTaxRate[] { ReverseChargeStandardTaxID, StandardRateTaxID }, false, TaxRegRuleCodes.EUExcludingLoginCountry, ZString.Empty, false);

				taxIDsToUse = new AccTaxRate[] { ReverseChargeStandardTaxID, ReverseChargeZeroRatedTaxID };
				AssertComplianceSubTypeAllocatedCorrectly("Invoice from NonEU Creditors", ItalyComplianceInfo.ComplianceSubTypeCodes.APS, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, taxIDsToUse, false, TaxRegRuleCodes.NotEU, ZString.Empty, false);
				AssertComplianceSubTypeAllocatedCorrectly("Credit Note from NonEU Creditors", ItalyComplianceInfo.ComplianceSubTypeCodes.APS, LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, taxIDsToUse, false, TaxRegRuleCodes.NotEU, ZString.Empty, false);

				AssertComplianceSubTypeAllocatedCorrectly("Invoice from NonEU Creditors", ItalyComplianceInfo.ComplianceSubTypeCodes.APS, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, taxIDsToUse, false, TaxRegRuleCodes.NotEU, ZString.Empty, false, true);
				AssertComplianceSubTypeAllocatedCorrectly("Credit Note from NonEU Creditors", ItalyComplianceInfo.ComplianceSubTypeCodes.APS, LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, taxIDsToUse, false, TaxRegRuleCodes.NotEU, ZString.Empty, false, true);

				AssertComplianceSubTypeAllocatedCorrectly("Self Billing AP Invoice", ItalyComplianceInfo.ComplianceSubTypeCodes.APS, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, taxIDsToUse, true, ZString.Empty, ZString.Empty, false);
				AssertComplianceSubTypeAllocatedCorrectly("Self Billing AP Credit Note", ItalyComplianceInfo.ComplianceSubTypeCodes.APS, LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, taxIDsToUse, true, ZString.Empty, ZString.Empty, false);

				AssertComplianceSubTypeAllocatedCorrectly("Self Billing AP Invoice", ItalyComplianceInfo.ComplianceSubTypeCodes.APS, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, taxIDsToUse, true, ZString.Empty, ZString.Empty, false, true);
				AssertComplianceSubTypeAllocatedCorrectly("Self Billing AP Credit Note", ItalyComplianceInfo.ComplianceSubTypeCodes.APS, LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, taxIDsToUse, true, ZString.Empty, ZString.Empty, false, true);

				AssertComplianceSubTypeAllocatedCorrectly("AP Integration Invoice (Domestic)", ItalyComplianceInfo.ComplianceSubTypeCodes.INI, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, taxIDsToUse, false, ZString.Empty, CountryCodes.Italy, false);
				AssertComplianceSubTypeAllocatedCorrectly("AP Integration Credit Note (Domestic)", ItalyComplianceInfo.ComplianceSubTypeCodes.INI, LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, taxIDsToUse, false, ZString.Empty, CountryCodes.Italy, false);

				AssertComplianceSubTypeAllocatedCorrectly("AP Integration Invoice (Non-Domestic, EU)", ItalyComplianceInfo.ComplianceSubTypeCodes.INT, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, taxIDsToUse, false, ZString.Empty, TaxRegRuleCodes.EUExcludingLoginCountry, false);
				AssertComplianceSubTypeAllocatedCorrectly("AP Integration Credit Note (Non-Domestic, EU)", ItalyComplianceInfo.ComplianceSubTypeCodes.INT, LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, taxIDsToUse, false, ZString.Empty, TaxRegRuleCodes.EUExcludingLoginCountry, false);

				AssertComplianceSubTypeAllocatedCorrectly("When Invoice is Part of VAT Group, Sub Type should not be defaulted in Italy", ZString.Empty, LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, new AccTaxRate[] { ReverseChargeStandardTaxID, StandardRateTaxID }, false, ZString.Empty, TaxRegRuleCodes.EUExcludingLoginCountry, true);

				AssertComplianceSubTypeAllocatedCorrectly("Invoices with only Exclude Charge Tax IDs", ItalyComplianceInfo.ComplianceSubTypeCodes.XAP, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, new AccTaxRate[] { ExcludeChargeTaxID, ExcludeChargeTaxID }, false, ZString.Empty, ZString.Empty, false);
				AssertComplianceSubTypeAllocatedCorrectly("Credit Notes with only Exclude Charge Tax IDs", ItalyComplianceInfo.ComplianceSubTypeCodes.XAP, LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, new AccTaxRate[] { ExcludeChargeTaxID, ExcludeChargeTaxID }, false, ZString.Empty, ZString.Empty, false);

				AssertComplianceSubTypeAllocatedCorrectly("Invoices with only Exclude Charge Tax IDs", ItalyComplianceInfo.ComplianceSubTypeCodes.XAP, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, new AccTaxRate[] { ExcludeChargeTaxID, ExcludeChargeTaxID }, false, CountryCodes.Italy, ZString.Empty, false);
				AssertComplianceSubTypeAllocatedCorrectly("Credit Notes with only Exclude Charge Tax IDs", ItalyComplianceInfo.ComplianceSubTypeCodes.XAP, LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, new AccTaxRate[] { ExcludeChargeTaxID, ExcludeChargeTaxID }, false, CountryCodes.Italy, ZString.Empty, false);

				AssertComplianceSubTypeAllocatedCorrectly("Invoices with mixture of Exclude and Charge Tax IDs and Comment charge code", ItalyComplianceInfo.ComplianceSubTypeCodes.XAP, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, new AccTaxRate[] { ExcludeChargeTaxID }, false, ZString.Empty, ZString.Empty, false, true);
				AssertComplianceSubTypeAllocatedCorrectly("Credit Notes  with mixture of Exclude Charge Tax IDs and Comment charge code", ItalyComplianceInfo.ComplianceSubTypeCodes.XAP, LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, new AccTaxRate[] { ExcludeChargeTaxID }, false, ZString.Empty, ZString.Empty, false, true);

				AssertComplianceSubTypeAllocatedCorrectly("Invoices with only Comment charge code", ZString.Empty, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, Array.Empty<AccTaxRate>(), false, ZString.Empty, ZString.Empty, false, true);
				AssertComplianceSubTypeAllocatedCorrectly("Credit Notes  with only Comment charge code", ZString.Empty, LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, Array.Empty<AccTaxRate>(), false, ZString.Empty, ZString.Empty, false, true);
			}
		}

		public void TestLastDateUsedGreaterThanPostDateStoredExceptionOnSaving_PST()
		{
			AssertLastDateUsedGreaterThanPostDateStoredExceptionOnSaving(ComplianceNumberAllocationDateOptions.PostDate.Code);
		}

		public void TestLastDateUsedGreaterThanPostDateStoredExceptionOnSaving_INV()
		{
			AssertLastDateUsedGreaterThanPostDateStoredExceptionOnSaving(ComplianceNumberAllocationDateOptions.InvoiceDate.Code);
		}

		void AssertLastDateUsedGreaterThanPostDateStoredExceptionOnSaving(string dateOption)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				var subType = ItalyComplianceInfo.ComplianceSubTypeCodes.ARI;
				var dateLabel = dateOption == ComplianceNumberAllocationDateOptions.InvoiceDate.Code ? "Invoice" : "Post";

				var registry = AccountingMasterFilesRegistry.Instance;
				var guid_GC = GlbCompany.CurrentCompany.PK.ToGuid();

				var collection = GetComplianceSubTypeAttribRuleConfigCollection(CountryCodes.Italy, "AR", subType);
				registry.ComplianceSubTypeAttributionRuleConfiguration.SetValue(guid_GC, Guid.Empty, Guid.Empty, collection);

				ZGuid menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "VN Govt Tax Invoice")).PK;
				var lastPostDate = new ZDate(2019, 10, 15);
				var sequence = ObjectCreator.SetupComplianceSequence(menuPK, subType, "TXI.19-", 1, 100, 25);
				Factory.Save();

				var previousInvoiceWithEarlierDate = CreateInvoice(dateOption, lastPostDate);
				previousInvoiceWithEarlierDate.AH_XD_ComplianceBook = sequence.PK;
				Factory.Save();

				var invoice = CreateInvoice(dateOption, new ZDate(2019, 10, 10));
				Factory.Save();
				AssertInvoiceComplianceNumberIsNotAllocated(invoice);

				using (AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AR.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, dateOption))
				{
					var invoice2 = CreateInvoice(dateOption, new ZDate(2019, 10, 10));
					using (registry.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
					{
						Factory.Save();
						AssertAnErrorWasThrownDuringComplianceNumberAllocation(invoice2, $@"Compliance Numbers cannot be allocated.
 Last posted transaction with the same Compliance Sub Type ARI has {dateLabel} Date = 15-Oct-19, that is greater than the current one(s).");
					}

					var invoice3 = CreateInvoice(dateOption, new ZDate(2019, 10, 10));
					using (registry.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print))
					{
						Factory.Save();
						AssertInvoiceComplianceNumberIsNotAllocated(invoice3);
					}
				}
			}
		}

		public void TestSparseBookStoredExceptionOnSaving_PST()
		{
			AssertSparseBookStoredExceptionOnSaving(ComplianceNumberAllocationDateOptions.PostDate.Code, "Post Date");
		}

		public void TestSparseBookStoredExceptionOnSaving_INV()
		{
			AssertSparseBookStoredExceptionOnSaving(ComplianceNumberAllocationDateOptions.InvoiceDate.Code, "Invoice Date");
		}

		public void AssertSparseBookStoredExceptionOnSaving(string dateOption, string dateLabel)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				var subType = ItalyComplianceInfo.ComplianceSubTypeCodes.ARI;
				var registry = AccountingMasterFilesRegistry.Instance;
				var guid_GC = GlbCompany.CurrentCompany.PK.ToGuid();

				var collection = GetComplianceSubTypeAttribRuleConfigCollection(CountryCodes.Italy, "AR", subType);
				registry.ComplianceSubTypeAttributionRuleConfiguration.SetValue(guid_GC, Guid.Empty, Guid.Empty, collection);

				var menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "VN Govt Tax Invoice")).PK;
				var lastPostDate = new ZDate(2019, 10, 15);
				ObjectCreator.SetupComplianceSequence(menuPK, subType, "TXI.19-", 1, 100, 25);
				Factory.Save();

				var invoice1 = CreateInvoice(dateOption, new ZDate(2019, 10, 14));
				Factory.Save();
				AssertInvoiceComplianceNumberIsNotAllocated(invoice1);

				var invoice2 = CreateInvoice(dateOption, new ZDate(2019, 10, 15));
				using (registry.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
				{
					Factory.Save();
					AssertInvoiceComplianceNumberWasAllocatedWithoutError(invoice2);
				}

				using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, dateOption))
				{
					var invoice3 = CreateInvoice(dateOption, new ZDate(2019, 10, 15));
					using (registry.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
					{
						Factory.Save();
						AssertAnErrorWasThrownDuringComplianceNumberAllocation(invoice3, $@"Compliance Numbers cannot be allocated.
 There is some transaction with the same Compliance Sub Type ARI in earlier {dateLabel} and Compliance Number empty.
 Please allocate Compliance Number to all transactions with {dateLabel} < 15-Oct-19.");
					}

					var invoice4 = CreateInvoice(dateOption, new ZDate(2019, 10, 15));
					using (registry.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print))
					{
						Factory.Save();
						AssertInvoiceComplianceNumberIsNotAllocated(invoice4);
					}
				}
			}
		}

		void AssertInvoiceComplianceNumberIsNotAllocated(InvoicingBase invoice)
		{
			AssertNull("No exception stored in the EventArgsForCompliance", invoice.EventArgsForCompliance);
			AssertNullOrEmpty("Invoice transaction reference is empty", invoice.AH_TransactionReference);
		}

		void AssertInvoiceComplianceNumberWasAllocatedWithoutError(InvoicingBase invoice)
		{
			AssertNull("No exception stored in the EventArgsForCompliance", invoice.EventArgsForCompliance);
			AssertNotNullOrEmpty("Invoice transaction reference is not empty", invoice.AH_TransactionReference);
		}

		void AssertAnErrorWasThrownDuringComplianceNumberAllocation(InvoicingBase invoice, string errorMessage)
		{
			AssertNotNull("There is an exception stored in the EventArgsForCompliance", invoice.EventArgsForCompliance);
			AssertEquals(errorMessage, invoice.EventArgsForCompliance.complianceSequenceRelatedException.UserFriendlyMessage);
			AssertNullOrEmpty("Invoice transaction reference is empty", invoice.AH_TransactionReference);
		}

		InvoicingBase CreateInvoice(string dateOption, ZDateTime allocationDate)
		{
			var invoice = ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), RandomNumberGenerator.Next(10000000).ToString(), ObjectCreator.EUR, 1, 100, 10, 100, 10);
			invoice.AH_ComplianceSubType = ItalyComplianceInfo.ComplianceSubTypeCodes.ARI;
			invoice.AH_TransactionReference = "";
			SetInvoiceDate(invoice, dateOption, allocationDate, allocationDate.AddDays(-370));
			invoice.Lines[0].AL_AT = ObjectCreator.GSTFREE1.PK;
			return invoice;
		}

		void SetInvoiceDate(InvoicingBase invoice, string dateOption, ZDateTime allocationDate, ZDateTime otherDate)
		{
			if (dateOption == ComplianceNumberAllocationDateOptions.InvoiceDate.Code)
			{
				invoice.AH_PostDate = otherDate;
				invoice.AH_InvoiceDate = allocationDate;
			}
			else
			{
				invoice.AH_PostDate = allocationDate;
				invoice.AH_InvoiceDate = otherDate;
			}
		}

		public void TestEnsureNoPastTransactionsWithEmptyComplNum_PST()
		{
			AssertEnsureNoPastTransactionsWithEmptyComplNum(ComplianceNumberAllocationDateOptions.PostDate.Code);
		}

		public void TestEnsureNoPastTransactionsWithEmptyComplNum_INV()
		{
			AssertEnsureNoPastTransactionsWithEmptyComplNum(ComplianceNumberAllocationDateOptions.InvoiceDate.Code);
		}

		void AssertEnsureNoPastTransactionsWithEmptyComplNum(string dateOption)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Spain))
			{
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "ESMAD";
				var menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;

				var sequence = ObjectCreator.SetupComplianceSequence(menuPK, SpainComplianceInfo.ComplianceSubTypeCodes.TXI, "TXI.19-", 1, 100, 2);
				sequence.XD_StartDate = new ZDate(2019, 10, 1);
				sequence.XD_ExpiryDate = new ZDateTime(2019, 12, 31);
				sequence.XD_IsActive = true;

				var alterSeq = ObjectCreator.SetupComplianceSequence(menuPK, SpainComplianceInfo.ComplianceSubTypeCodes.TXI, "TXI.20-", 1, 100, 2);
				alterSeq.XD_StartDate = new ZDate(2020, 1, 1);
				alterSeq.XD_ExpiryDate = new ZDateTime(2020, 3, 31);
				alterSeq.XD_IsActive = true;

				var invNoSeq = CreateInvoice(null, "INV01", new ZDate(2019, 10, 1), new ZDate(2018, 10, 1), dateOption);

				var invoice1 = CreateInvoice(sequence, "INV1", new ZDate(2019, 10, 1), new ZDate(2018, 10, 1), dateOption);
				invoice1.AH_TransactionReference = "TXC.19-0001";

				var invoice2 = CreateInvoice(sequence, "INV2", new ZDate(2019, 10, 10), new ZDate(2018, 10, 1), dateOption);

				var thirdPostDate = new ZDate(2019, 10, 15);
				var invoice3 = CreateInvoice(sequence, "INV3", thirdPostDate);

				var invoice4 = CreateInvoice(sequence, "INV4", new ZDateTime(2019, 10, 20, 10, 30, 0), new ZDate(2018, 10, 1), dateOption);
				var invoice5 = CreateInvoice(sequence, "INV5", new ZDateTime(2019, 10, 20, 16, 30, 0), new ZDate(2018, 10, 1), dateOption);

				var invAlterSeq = CreateInvoice(alterSeq, "INV11", new ZDate(2020, 01, 01), new ZDate(2018, 10, 1), dateOption);

				Factory.Save();

				var currTrans = Array.Empty<InvoicingBase>();
				var gcPK = GlbCompany.CurrentCompany.PK.ToGuid();
				var regComplAssignNrByPostDate = AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AR;
				var regCompDocNrAllocation_Receivables = AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables;

				using (regCompDocNrAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print))
				{
					foreach (var complianceNumberAllocationDateOption in new[] {
						ComplianceNumberAllocationDateOptions.NoControl.Code,
						ComplianceNumberAllocationDateOptions.PostDate.Code,
						ComplianceNumberAllocationDateOptions.InvoiceDate.Code })
					{
						using (regComplAssignNrByPostDate.SetTemporaryValue(gcPK, Guid.Empty, Guid.Empty, complianceNumberAllocationDateOption))
						{
							ExpectNoExceptionForSparseBook(currTrans, "for no Invoices selected or Compliance Sequence assignment no by Post Date");
						}
					}
				}

				using (regCompDocNrAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
				{
					using (regComplAssignNrByPostDate.SetTemporaryValue(gcPK, Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.NoControl.Code))
					{
						ExpectNoExceptionForSparseBook(currTrans, "for no Invoices selected or Compliance Sequence assignment no by Post Date");
					}

					using (regComplAssignNrByPostDate.SetTemporaryValue(gcPK, Guid.Empty, Guid.Empty, dateOption))
					{
						currTrans = new[] { invNoSeq };
						ExpectExceptionForSparseBook<FailedToFindComplianceSequenceException>(currTrans, "for an Invoice without Compliance Sequence.", ComplianceSequenceNumberAllocationErrorMessages.FailedToFindComplianceSequenceMessage);
						AssertEquals(ComplianceSequenceNumberAllocationErrorMessages.FailedToFindComplianceSequenceMessage, invNoSeq.AssignComplianceSubTypeAndCheckComplianceErrors());

						currTrans = new[] { invoice1 };
						ExpectNoExceptionForSparseBook(currTrans, "for the 1st Invoice in the Compliance Sequence");
						AssertNullOrEmpty(invoice1.AssignComplianceSubTypeAndCheckComplianceErrors());

						currTrans = new[] { invoice1, invAlterSeq };
						ExpectExceptionForSparseBook<MultipleComplianceSequenceFoundException>(currTrans, "if Invoices have different Compliance Sequence.", ComplianceSequenceNumberAllocationErrorMessages.MultipleComplianceSequenceFoundMessage);

						currTrans = new[] { invoice2 };
						ExpectNoExceptionForSparseBook(currTrans, "for an Invoice just after the unique valid one in the Compliance Sequence");
						AssertNullOrEmpty(invoice2.AssignComplianceSubTypeAndCheckComplianceErrors());

						currTrans = new[] { invoice3 };
						var expErrMsg = string.Format(ComplianceSequenceNumberAllocationErrorMessages.GetUnableToAllocateNumberDueToSparseComplianceBookMessage(dateOption),
							SpainComplianceInfo.ComplianceSubTypeCodes.TXI, thirdPostDate.ToShortDateString());
						ExpectExceptionForSparseBook<UnableToAllocateNumberDueToSparseComplianceBookException>(currTrans, "for an Invoice just after a non-valid one in the Compliance Sequence.", expErrMsg);
						AssertEquals(expErrMsg, invoice3.AssignComplianceSubTypeAndCheckComplianceErrors());

						currTrans = new[] { invoice3, invoice4, invoice5 };
						ExpectExceptionForSparseBook<UnableToAllocateNumberDueToSparseComplianceBookException>(currTrans, "for a list of Invoices after a non-valid one in the Compliance Sequence.",
							string.Format(ComplianceSequenceNumberAllocationErrorMessages.GetUnableToAllocateNumberDueToSparseComplianceBookMessage(dateOption), SpainComplianceInfo.ComplianceSubTypeCodes.TXI, thirdPostDate.ToShortDateString()));
						AssertEquals(expErrMsg, invoice3.AssignComplianceSubTypeAndCheckComplianceErrors());

						currTrans = new[] { invoice2, invoice3, invoice5 };
						ExpectNoExceptionForSparseBook(currTrans, "for a list of Invoices just after the unique valid one in the Compliance Sequence, but with no valid ones out of range");
						AssertNullOrEmpty(invoice2.AssignComplianceSubTypeAndCheckComplianceErrors());

						CreateInvoice(sequence, "INV0", new ZDate(2019, 09, 30), new ZDate(2018, 10, 1), dateOption);
						CreateInvoice(sequence, "INV10", new ZDate(2020, 01, 01), new ZDate(2018, 10, 1), dateOption);
						Factory.Save();

						ExpectNoExceptionForSparseBook(currTrans, "if any Invoice outside the validity range of the Compliance Sequence");
						AssertNullOrEmpty(invoice2.AssignComplianceSubTypeAndCheckComplianceErrors());

						var invExtra = CreateInvoice(sequence, "INV00", new ZDate(2019, 10, 12), new ZDate(2018, 10, 1), dateOption);
						currTrans.Append(invExtra);
						ExpectNoExceptionForSparseBook(currTrans, "if any Invoice not saved in DB");
					}
				}
			}
		}

		public void TestCheckIfComplianceBookIsFullOrExpired()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Spain))
			{
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "ESMAD";
				var menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;

				var alterSeq = ObjectCreator.SetupComplianceSequence(menuPK, SpainComplianceInfo.ComplianceSubTypeCodes.TXI, "TXI.20-", 1, 100, 2);
				alterSeq.XD_StartDate = new ZDate(2020, 1, 1);
				alterSeq.XD_ExpiryDate = new ZDateTime(2020, 3, 30);
				alterSeq.XD_IsActive = true;

				var fullSeq = ObjectCreator.SetupComplianceSequence(menuPK, SpainComplianceInfo.ComplianceSubTypeCodes.TXI, "TXI.20_", 1, 100, 101);
				fullSeq.XD_StartDate = new ZDate(2020, 4, 1);
				fullSeq.XD_ExpiryDate = new ZDateTime(2020, 6, 30);
				fullSeq.XD_IsActive = true;

				var invAlterSeq = CreateInvoice(alterSeq, "INV1", new ZDate(2020, 01, 01));
				invAlterSeq.AH_TransactionReference = "TXE.20-0001";

				var invFullSeq = CreateInvoice(fullSeq, "INV0", new ZDate(2020, 4, 1));

				Factory.Save();

				var currTrans = Array.Empty<InvoicingBase>();
				var gcPK = GlbCompany.CurrentCompany.PK.ToGuid();
				var regComplAssignNrByPostDate = AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AR;
				var regCompDocNrAllocation_Receivables = AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables;

				using (regCompDocNrAllocation_Receivables.SetTemporaryValue(gcPK, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print))
				{
					foreach (var dateOption in new[] {
						ComplianceNumberAllocationDateOptions.NoControl.Code,
						ComplianceNumberAllocationDateOptions.PostDate.Code,
						ComplianceNumberAllocationDateOptions.InvoiceDate.Code })
					{
						using (regComplAssignNrByPostDate.SetTemporaryValue(gcPK, Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.NoControl.Code))
						{
							ExpectNoExceptionForSparseBook(currTrans, "expect no exception no matther the ComplianceNumberAllocationDateOption");
						}
					}
				}

				using (regCompDocNrAllocation_Receivables.SetTemporaryValue(gcPK, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
				{
					using (regComplAssignNrByPostDate.SetTemporaryValue(gcPK, Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.NoControl.Code))
					{
						ExpectNoExceptionForSparseBook(currTrans, "for no Invoices selected and Compliance Sequence assignment no by Post Date");
					}
					foreach (var dateOption in new[] { ComplianceNumberAllocationDateOptions.PostDate.Code, ComplianceNumberAllocationDateOptions.InvoiceDate.Code })
					{
						using (regComplAssignNrByPostDate.SetTemporaryValue(gcPK, Guid.Empty, Guid.Empty, dateOption))
						{
							ExpectExceptionForComplianceSequence<AllocationComplianceSequenceFullException>(invFullSeq, "full", ComplianceSequenceNumberAllocationErrorMessages.ComplianceSequenceIsFullOrExpiredExceptionMessage);
							AssertEquals(ComplianceSequenceNumberAllocationErrorMessages.ComplianceSequenceIsFullOrExpiredExceptionMessage, invFullSeq.AssignComplianceSubTypeAndCheckComplianceErrors());

							var invExpSeq1 = CreateInvoice(alterSeq, "INV21", new ZDate(2019, 12, 31), new ZDate(2018, 5, 10), dateOption);
							ExpectExceptionForComplianceSequence<FailedToFindComplianceSequenceException>(invExpSeq1, "not started", ComplianceSequenceNumberAllocationErrorMessages.FailedToFindComplianceSequenceMessage);
							AssertEquals(ComplianceSequenceNumberAllocationErrorMessages.FailedToFindComplianceSequenceMessage, invExpSeq1.AssignComplianceSubTypeAndCheckComplianceErrors());
							SetInvoiceDate(invExpSeq1, dateOption, new ZDateTime(2020, 1, 1, 11, 30, 0), new ZDateTime(2019, 1, 1));
							ExpectNoExceptionForComplianceSequence(invExpSeq1, "for Compliance Seq. Post Date >= Start Date");

							var invExpSeq2 = CreateInvoice(alterSeq, "INV22", new ZDate(2020, 3, 31));
							ExpectExceptionForComplianceSequence<FailedToFindComplianceSequenceException>(invExpSeq2, "expired", ComplianceSequenceNumberAllocationErrorMessages.FailedToFindComplianceSequenceMessage);
							AssertEquals(ComplianceSequenceNumberAllocationErrorMessages.FailedToFindComplianceSequenceMessage, invExpSeq2.AssignComplianceSubTypeAndCheckComplianceErrors());
							SetInvoiceDate(invExpSeq2, dateOption, new ZDateTime(2020, 3, 30, 11, 30, 0), new ZDateTime(2019, 1, 1));
							ExpectNoExceptionForComplianceSequence(invExpSeq2, "for Compliance Seq. Post Date <= Expiry Date");

							ZDateTime wrongPostDate;
							ZDateTime.TryParseExact("1213212", out wrongPostDate, "DDMMYYYY");
							Assert(!wrongPostDate.IsValid);
							if (dateOption == ComplianceNumberAllocationDateOptions.InvoiceDate.Code)
							{
								invExpSeq2.AH_InvoiceDate = wrongPostDate;
							}
							else
							{
								invExpSeq2.AH_PostDate = wrongPostDate;
							}
							ExpectExceptionForComplianceSequence<FailedToFindComplianceSequenceException>(invExpSeq2, "Post Date not valid for", ComplianceSequenceNumberAllocationErrorMessages.FailedToFindComplianceSequenceMessage);
						}
					}
				}
			}
		}

		public void TestCheckPostDateEarlierThanLastDateUsed_PST()
		{
			AssertCheckPostDateEarlierThanLastDateUsed(ComplianceNumberAllocationDateOptions.PostDate.Code);
		}

		public void TestCheckPostDateEarlierThanLastDateUsed_INV()
		{
			AssertCheckPostDateEarlierThanLastDateUsed(ComplianceNumberAllocationDateOptions.InvoiceDate.Code);
		}

		public void AssertCheckPostDateEarlierThanLastDateUsed(string dateOption)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Spain))
			{
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "ESMAD";
				var menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;

				var complSeq = ObjectCreator.SetupComplianceSequence(menuPK, SpainComplianceInfo.ComplianceSubTypeCodes.TXI, "00233", 1, 100, 2);
				complSeq.XD_StartDate = new ZDate(2020, 1, 1);
				complSeq.XD_ExpiryDate = new ZDateTime(2020, 3, 30);
				complSeq.XD_IsActive = true;

				var invSeq1 = CreateInvoice(complSeq, "INV1", new ZDate(2020, 1, 5), new ZDate(2019, 1, 5), dateOption);
				var invSeq2 = CreateInvoice(complSeq, "INV2", new ZDate(2020, 1, 10), new ZDate(2019, 1, 5), dateOption);

				var lastDateUsed = new ZDate(2020, 1, 10);
				var previousInvoiceWithEarlierDate = CreateInvoice(complSeq, "INV3", lastDateUsed, new ZDate(2019, 1, 5), dateOption);
				previousInvoiceWithEarlierDate.AH_XD_ComplianceBook = complSeq.PK;

				Factory.Save();

				var currTrans = Array.Empty<InvoicingBase>();
				var gcPK = GlbCompany.CurrentCompany.PK.ToGuid();
				var regComplAssignNrByPostDate = AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AR;
				var regCompDocNrAllocation_Receivables = AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables;

				using (regCompDocNrAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print))
				{
					using (regComplAssignNrByPostDate.SetTemporaryValue(gcPK, Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.NoControl.Code))
					{
						ExpectNoExceptionForSparseBook(currTrans, "for no Invoices selected or Compliance Sequence assignment no by Post Date");
					}
					using (regComplAssignNrByPostDate.SetTemporaryValue(gcPK, Guid.Empty, Guid.Empty, dateOption))
					{
						ExpectNoExceptionForSparseBook(currTrans, "for no Invoices selected or Compliance Sequence assignment no by Post Date");
					}
				}

				using (regCompDocNrAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
				{
					using (regComplAssignNrByPostDate.SetTemporaryValue(gcPK, Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.NoControl.Code))
					{
						ExpectNoExceptionForSparseBook(currTrans, "for no Invoices selected or Compliance Sequence assignment no by Post Date");
					}
					using (regComplAssignNrByPostDate.SetTemporaryValue(gcPK, Guid.Empty, Guid.Empty, dateOption))
					{
						var expErrMsg = string.Format(ComplianceSequenceNumberAllocationErrorMessages.GetUnableToAllocateNumberDueToAllocationDateEarlierThanLastDateUsedMessage(dateOption),
						SpainComplianceInfo.ComplianceSubTypeCodes.TXI, lastDateUsed.ToShortDateString());
						try
						{
							invSeq1.CheckAllocationDateEarlierThanLastDateUsed();
							Fail("Exception should be thrown for an Invoice posted before last posted one.");
						}
						catch (Exception ex)
						{
							AssertEquals("Should be of correct exception type", typeof(UnableToAllocateNumberDueToPostDateEarlierThanLastDateUsedException), ex.GetType());
							AssertEquals("The exception message should be correct", expErrMsg, ((ComplianceSequenceRelatedException)ex).UserFriendlyMessage);
						}
						AssertEquals(expErrMsg, invSeq1.AssignComplianceSubTypeAndCheckComplianceErrors());

						invSeq1.AH_TransactionReference = "ARI.20-0002";
						Factory.Save();

						ExpectNoExceptionForPostDateVsLastDateUsed(invSeq2, "for an Invoice just after the Last Date used in the Compliance Sequence");
						AssertNullOrEmpty(invSeq2.AssignComplianceSubTypeAndCheckComplianceErrors());

						ZDateTime wrongPostDate;
						ZDateTime.TryParseExact("1213212", out wrongPostDate, "DDMMYYYY");
						Assert(!wrongPostDate.IsValid);
						if (dateOption == ComplianceNumberAllocationDateOptions.InvoiceDate.Code)
						{
							invSeq2.AH_InvoiceDate = wrongPostDate;
						}
						else
						{
							invSeq2.AH_PostDate = wrongPostDate;
						}
						ExpectExceptionForComplianceSequence<FailedToFindComplianceSequenceException>(invSeq2, "Post Date not valid for", ComplianceSequenceNumberAllocationErrorMessages.FailedToFindComplianceSequenceMessage);
					}
				}
			}
		}

		InvoicingBase AssertComplianceSubTypeAllocatedCorrectly(ZString error, ZString expectedComplianceSubType, ZString ledger, ZString transactionType, AccTaxRate[] taxIDsToUse, bool isSelfBillingInvoice, ZString organisationLocationRule, ZString taxRegistrationLocationRule, bool makeOrgPartOfLoginCompanyVATGroup, bool addCMTLine = false)
		{
			var testInvoice = CreateInvoiceForComplianceSubTypeTest(ledger, transactionType, taxIDsToUse, isSelfBillingInvoice, organisationLocationRule, taxRegistrationLocationRule, makeOrgPartOfLoginCompanyVATGroup, addCMTLine);
			testInvoice.Factory.Save();

			AssertEquals(error, expectedComplianceSubType, testInvoice.AH_ComplianceSubType);

			if (!expectedComplianceSubType.IsEmpty)
			{
				var expectedEventLogDescription = $"Compliance Sub Type was defaulted to {expectedComplianceSubType}.";
				var editLogs = testInvoice.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.EditedARecord.Code && log.SL_Reference == expectedEventLogDescription).ToArray();
				AssertEquals("Should be one EDT event created where the user has pre-set a value", 1, editLogs.Length);
			}

			return testInvoice;
		}

		void ExpectExceptionForSparseBook<T>(InvoicingBase[] transactions, string message, string exMessage) where T : ComplianceSequenceRelatedException
		{
			try
			{
				InvoicingBase.EnsureNoPastTransactionsWithEmptyComplNum(transactions);
				Fail($"Exception should be thrown {message}");
			}
			catch (Exception ex)
			{
				AssertEquals("Should be of correct exception type", typeof(T), ex.GetType());
				AssertEquals("The exception message should be correct", exMessage, ((ComplianceSequenceRelatedException)ex).UserFriendlyMessage);
			}
		}

		void ExpectNoExceptionForSparseBook(InvoicingBase[] transactions, string message)
		{
			try
			{
				InvoicingBase.EnsureNoPastTransactionsWithEmptyComplNum(transactions);
			}
			catch (Exception ex)
			{
				Fail($"No exception should be thrown {message}, instead was: " + ex.Message);
			}
		}

		void ExpectExceptionForComplianceSequence<T>(InvoicingBase transaction, string message, string exMessage) where T : ComplianceSequenceRelatedException
		{
			try
			{
				transaction.CheckIfComplianceBookIsFullOrExpired();
				Fail($"Exception should be thrown for an Invoice with {message} Compliance Sequence.");
			}
			catch (Exception ex)
			{
				AssertEquals("Should be of correct exception type", typeof(T), ex.GetType());
				AssertEquals("The exception message should be correct", exMessage, ((ComplianceSequenceRelatedException)ex).UserFriendlyMessage);
			}
		}

		void ExpectNoExceptionForComplianceSequence(InvoicingBase transaction, string message)
		{
			try
			{
				transaction.CheckIfComplianceBookIsFullOrExpired();
			}
			catch (Exception ex)
			{
				Fail($"No exception should be thrown {message}, instead was: " + ex.Message);
			}
		}

		void ExpectNoExceptionForPostDateVsLastDateUsed(InvoicingBase transaction, string message)
		{
			try
			{
				transaction.CheckAllocationDateEarlierThanLastDateUsed();
			}
			catch (Exception ex)
			{
				Fail($"No exception should be thrown {message}, instead was: " + ex.Message);
			}
		}

		[TestDate(2023, 1, 1)]
		public void TestAllocateTransactionHeaderAuthorizationNumberReference_WhenCurrentCompanyIsPortugal()
		{
			using (new DisposableAction(() => InvoicingBase.ShouldCheckTransactionHeaderReferenceATH_ForTestOnly = true, () => InvoicingBase.ShouldCheckTransactionHeaderReferenceATH_ForTestOnly = false))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Payables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			{
				AssertEquals("Percondition", true, GlbCompany.CurrentCompany.Country.SupportComplianceSubType);
				AssertEquals("Percondition", false, AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value);

				ZGuid menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;
				var sequence = ObjectCreator.SetupComplianceSequence(menuPK, PortugalComplianceInfo.ComplianceSubTypeCodes.TXI, "PRE", 1, 100, 1);
				sequence.XD_PrintingAuthorizationNumber = "PAN";
				var sequenceWithoutPrintingAuthorizationNumber = ObjectCreator.SetupComplianceSequence(menuPK, PortugalComplianceInfo.ComplianceSubTypeCodes.TCM, "TCM", 1, 100, 1);
				Factory.Save();

				var payableInvoice1 = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsPayable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID }, false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZString.Empty, false);
				payableInvoice1.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TXI;
				Factory.Save();
				AssertEquals(sequence.PK, payableInvoice1.AH_XD_ComplianceBook);
				AssertEquals("TXI PRE/000000001", payableInvoice1.AH_TransactionReference);
				AssertNullOrEmpty("Will NOT allocate reference ATH when Ledger is Payable", payableInvoice1.AuthorizationNumberReference);

				var receivableInvoice1 = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID }, false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZString.Empty, false);
				receivableInvoice1.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TXI;
				receivableInvoice1.AH_PostDate = ZDateTime.Today.AddDays(1);
				Factory.Save();
				AssertEquals(sequence.PK, receivableInvoice1.AH_XD_ComplianceBook);
				AssertEquals("TXI PRE/000000002", receivableInvoice1.AH_TransactionReference);
				AssertEquals("Will allocate reference ATH", "PAN-000000002", receivableInvoice1.AuthorizationNumberReference);

				var receivableInvoice2 = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID }, false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZString.Empty, false);
				receivableInvoice2.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TCM;
				receivableInvoice2.AH_PostDate = ZDateTime.Today.AddDays(2);
				var ex = AssertExceptionThrown<OnSavingCriticalCheckException>(() => Factory.Save());
				AssertEquals(nameof(CriticalValidationErrorType.AbortOnSavingProcess), ex.ErrorType);
				AssertEquals(sequenceWithoutPrintingAuthorizationNumber.PK, receivableInvoice2.AH_XD_ComplianceBook);
				AssertNullOrEmpty("Will set AH_TransactionReference to empty in OnSaved()", receivableInvoice2.AH_TransactionReference);
				AssertNullOrEmpty("Will NOT allocate reference ATH when Compliance Sequence has empty XD_PrintingAuthorizationNumber", receivableInvoice2.AuthorizationNumberReference);
			}
		}

		public void TestAllocateTransactionHeaderAuthorizationNumberReference_WhenCurrentCompanyIsNotPortugal()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Indonesia))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			{
				AssertEquals("Percondition", true, GlbCompany.CurrentCompany.Country.SupportComplianceSubType);
				AssertEquals("Percondition", false, AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value);

				ZGuid menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;
				var sequence = ObjectCreator.SetupComplianceSequence(menuPK, IndonesiaComplianceInfo.ComplianceSubTypeCodes.BKP, "BKP", 1, 100, 1);
				sequence.XD_PrintingAuthorizationNumber = "PAN";
				Factory.Save();

				var receivableInvoice = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID }, false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZString.Empty, false);
				receivableInvoice.AH_ComplianceSubType = IndonesiaComplianceInfo.ComplianceSubTypeCodes.BKP;
				Factory.Save();
				AssertEquals(sequence.PK, receivableInvoice.AH_XD_ComplianceBook);
				AssertEquals("BKP000000001", receivableInvoice.AH_TransactionReference);
				AssertNullOrEmpty(receivableInvoice.AuthorizationNumberReference);
			}
		}

		[TestDate(2025, 5, 5)]
		public void TestExporterExemption()
		{
			OrgHeader createOrg(bool creditorOtherwiseDebtor = false, bool? docExpired = null)
			{
				bool docAttached = docExpired.HasValue;
				var org = ObjectCreator.CreateOrgHeader("A" + ObjectCreator.UniqueNumber, creditorOtherwiseDebtor, !creditorOtherwiseDebtor);
				org.CustomsCodes.AddNew(ElSalvadorOrgCusCodeInfo.OrgCusCodes.NRC, Core.Constants.CountryCodes.ElSalvador);

				if (docAttached)
				{
					var end = ZDate.Today.AddDays(docExpired.Value ? -5 : 5);
					var start = end.AddDays(-100);

					var jobDocument = org.RequiredDocuments.AddNew();
					jobDocument.EQ_DocCategory = ReferenceTypes.ClientSupplierRelationship;
					jobDocument.EQ_DocType = RefDocTypes.VATExporterExemption;
					jobDocument.EQ_DateReceived = start.ToZDateTime().ToDateTimeOffset(null);
					jobDocument.EQ_ValidToDate = end;
					jobDocument.EQ_DocUsage = creditorOtherwiseDebtor ? JobRequiredDocument.DocUsage.Creditor : JobRequiredDocument.DocUsage.Debtor;
					jobDocument.EQ_RN_NKRelatedCountry = CountryCodes.ElSalvador;
				}

				return org;
			}

			void assert(string message, OrgHeader org, Type invoiceType, string expectComplianceSubType)
			{
				var invoice = ObjectCreator.CreateInvoice(invoiceType, ObjectCreator.UniqueNumber.ToString(), ObjectCreator.AUD, 1m, org);
				ObjectCreator.CreateInvoiceLine(invoice, 100M, ObjectCreator.AUD, 10M);
				Factory.Save();
				AssertEquals($"{invoice.AH_Ledger} {invoice.AH_TransactionType} {message}", expectComplianceSubType, invoice.AH_ComplianceSubType);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.ElSalvador))
			{
				var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
				void addItem(string ledger, string invoiceType, string exemption, string subType)
				{
					var item = collection.AddNew();
					item.Country = CountryCodes.ElSalvador;
					item.LedgerType = ledger;
					item.InvoiceType = invoiceType;
					item.ExporterExemption = exemption;
					item.SubType = subType;
					item.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
					item.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
					item.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
				}

				addItem("AR", "INV", ExporterExemptionCodes.Exempt, ElSalvadorComplianceInfo.ComplianceSubTypeCodes.TCD);
				addItem("AR", "INV", ExporterExemptionCodes.NotExempt, ElSalvadorComplianceInfo.ComplianceSubTypeCodes.TCR);
				addItem("AR", "CRD", "", ElSalvadorComplianceInfo.ComplianceSubTypeCodes.TXE);
				addItem("AP", "CRD", ExporterExemptionCodes.Exempt, ElSalvadorComplianceInfo.ComplianceSubTypeCodes.TCD);
				addItem("AP", "CRD", ExporterExemptionCodes.NotExempt, ElSalvadorComplianceInfo.ComplianceSubTypeCodes.TCR);
				addItem("AP", "INV", "", ElSalvadorComplianceInfo.ComplianceSubTypeCodes.TXE);

				AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

				var debtorNoDoc = createOrg(false);
				var debtorExempt = createOrg(false, false);
				var debtorExpired = createOrg(false, true);
				var creditorNoDoc = createOrg(true);
				var creditorExempt = createOrg(true, false);
				var creditorExpired = createOrg(true, true);

				Factory.Save();

				assert($"debtor no doc", debtorNoDoc, typeof(ARInvoice), "TCR");
				assert($"debtor valid doc", debtorExempt, typeof(ARInvoice), "TCD");
				assert($"debtor expired doc", debtorExpired, typeof(ARInvoice), "TCR");

				assert($"debtor no doc", debtorNoDoc, typeof(ARCreditNote), "TXE");
				assert($"debtor valid doc", debtorExempt, typeof(ARCreditNote), "TXE");
				assert($"debtor expired doc", debtorExpired, typeof(ARCreditNote), "TXE");

				assert($"creditor no doc", creditorNoDoc, typeof(APInvoice), "TXE");
				assert($"creditor valid doc", creditorExempt, typeof(APInvoice), "TXE");
				assert($"creditor expired doc", creditorExpired, typeof(APInvoice), "TXE");

				assert($"creditor no doc", creditorNoDoc, typeof(APCreditNote), "TCR");
				assert($"creditor valid doc", creditorExempt, typeof(APCreditNote), "TCD");
				assert($"creditor expired doc", creditorExpired, typeof(APCreditNote), "TCR");
			}
		}

		public void TestEvaluateEligibilityAndPivotQueueing_Turkey()
		{
			ObjectCreator.DebtorTR.Factory.Save();
			ObjectCreator.CreateCustomsCodes(GlbCompany.CurrentCompany.OrgProxy, CountryCodes.Turkey, "VAT", "1234567891");
			GlbCompany.CurrentCompany.OrgProxy.Factory.Save();

			using (ObjectCreator.SetUpForTestingEInvoicingTurkey_ARAP(Env.CurrentBranchPK, ZDateTime.Now.AddDays(-1).ToDateTime(), AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual))
			{
				AssertEvaluateEligibilityAndPivotQueueing_Turkey<ARInvoice>("101", TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR, pivotShouldBeCreated: true, expectedActionType: EInvoicingPivotActionType.Submit, expectedStatus: EInvoicingPivotState.Queued);
				AssertEvaluateEligibilityAndPivotQueueing_Turkey<ARInvoice>("102", TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN, pivotShouldBeCreated: true, expectedActionType: EInvoicingPivotActionType.Submit, expectedStatus: EInvoicingPivotState.Queued);
				AssertEvaluateEligibilityAndPivotQueueing_Turkey<ARInvoice>("103", TurkeyComplianceInfo.ComplianceSubTypeCodes.EIC, pivotShouldBeCreated: true, expectedActionType: EInvoicingPivotActionType.Submit, expectedStatus: EInvoicingPivotState.Queued);
				AssertEvaluateEligibilityAndPivotQueueing_Turkey<ARInvoice>("104", TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN, parentComplianceSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR, pivotShouldBeCreated: true, expectedActionType: EInvoicingPivotActionType.Cancel, expectedStatus: EInvoicingPivotState.Queued);

				AssertEvaluateEligibilityAndPivotQueueing_Turkey<ARInvoice>("1001", TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR, pivotShouldBeCreated: true, expectedActionType: EInvoicingPivotActionType.Submit, expectedStatus: EInvoicingPivotState.Queued);
				AssertEvaluateEligibilityAndPivotQueueing_Turkey<ARInvoice>("1002", TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN, pivotShouldBeCreated: true, expectedActionType: EInvoicingPivotActionType.Submit, expectedStatus: EInvoicingPivotState.Queued);
				AssertEvaluateEligibilityAndPivotQueueing_Turkey<ARInvoice>("1003", TurkeyComplianceInfo.ComplianceSubTypeCodes.EIC, pivotShouldBeCreated: true, expectedActionType: EInvoicingPivotActionType.Submit, expectedStatus: EInvoicingPivotState.Queued);
				AssertEvaluateEligibilityAndPivotQueueing_Turkey<ARInvoice>("1004", TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN, parentComplianceSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR, hasComplianceNumber: false, pivotShouldBeCreated: false);
				AssertEvaluateEligibilityAndPivotQueueing_Turkey<ARInvoice>("1005", TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN, parentComplianceSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR, pivotShouldBeCreated: true, expectedActionType: EInvoicingPivotActionType.Cancel, expectedStatus: EInvoicingPivotState.Queued);
				AssertEvaluateEligibilityAndPivotQueueing_Turkey<ARInvoice>("1006", TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN, parentComplianceSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN, pivotShouldBeCreated: false);
				AssertEvaluateEligibilityAndPivotQueueing_Turkey<ARInvoice>("1007", TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN, parentComplianceSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.EIC, pivotShouldBeCreated: false);
				AssertEvaluateEligibilityAndPivotQueueing_Turkey<ARInvoice>("1008", TurkeyComplianceInfo.ComplianceSubTypeCodes.XCL, pivotShouldBeCreated: false);

				AssertEvaluateEligibilityAndPivotQueueing_Turkey<APInvoice>("1101", TurkeyComplianceInfo.ComplianceSubTypeCodes.PAR, pivotShouldBeCreated: false);
				AssertEvaluateEligibilityAndPivotQueueing_Turkey<APInvoice>("1102", TurkeyComplianceInfo.ComplianceSubTypeCodes.PIN, pivotShouldBeCreated: false);
				AssertEvaluateEligibilityAndPivotQueueing_Turkey<APInvoice>("1103", TurkeyComplianceInfo.ComplianceSubTypeCodes.PIC, pivotShouldBeCreated: false);
				AssertEvaluateEligibilityAndPivotQueueing_Turkey<APInvoice>("1104", TurkeyComplianceInfo.ComplianceSubTypeCodes.DAR, parentComplianceSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.PAR, hasComplianceNumber: false, pivotShouldBeCreated: false);
				AssertEvaluateEligibilityAndPivotQueueing_Turkey<APInvoice>("1105", TurkeyComplianceInfo.ComplianceSubTypeCodes.DIN, parentComplianceSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.PIN, hasComplianceNumber: false, pivotShouldBeCreated: false);
				AssertEvaluateEligibilityAndPivotQueueing_Turkey<APInvoice>("1106", TurkeyComplianceInfo.ComplianceSubTypeCodes.DIN, parentComplianceSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.PIC, hasComplianceNumber: false, pivotShouldBeCreated: false);
				// The next three cases cover GetPivotActionType implementation of Turkey.
				AssertEvaluateEligibilityAndPivotQueueing_Turkey<APInvoice>("1107", TurkeyComplianceInfo.ComplianceSubTypeCodes.DAR, parentComplianceSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.PAR, pivotShouldBeCreated: true, expectedActionType: EInvoicingPivotActionType.Submit, expectedStatus: EInvoicingPivotState.Queued);
				AssertEvaluateEligibilityAndPivotQueueing_Turkey<APInvoice>("1108", TurkeyComplianceInfo.ComplianceSubTypeCodes.DIN, parentComplianceSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.PIN, pivotShouldBeCreated: true, expectedActionType: EInvoicingPivotActionType.Submit, expectedStatus: EInvoicingPivotState.Queued);
				AssertEvaluateEligibilityAndPivotQueueing_Turkey<APInvoice>("1109", TurkeyComplianceInfo.ComplianceSubTypeCodes.DIN, parentComplianceSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.PIC, pivotShouldBeCreated: true, expectedActionType: EInvoicingPivotActionType.Submit, expectedStatus: EInvoicingPivotState.Queued);
				AssertEvaluateEligibilityAndPivotQueueing_Turkey<APInvoice>("1110", TurkeyComplianceInfo.ComplianceSubTypeCodes.PCL, pivotShouldBeCreated: false);

				AssertEvaluateEligibilityAndPivotQueueing_Turkey<APCreditNote>("1201", TurkeyComplianceInfo.ComplianceSubTypeCodes.DAR, hasComplianceNumber: false, pivotShouldBeCreated: false);
				AssertEvaluateEligibilityAndPivotQueueing_Turkey<APCreditNote>("1202", TurkeyComplianceInfo.ComplianceSubTypeCodes.DIN, hasComplianceNumber: false, pivotShouldBeCreated: false);
				AssertEvaluateEligibilityAndPivotQueueing_Turkey<APCreditNote>("1203", TurkeyComplianceInfo.ComplianceSubTypeCodes.DCN, parentComplianceSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.DAR, hasComplianceNumber: false, pivotShouldBeCreated: false);
				AssertEvaluateEligibilityAndPivotQueueing_Turkey<APCreditNote>("1204", TurkeyComplianceInfo.ComplianceSubTypeCodes.DAR, pivotShouldBeCreated: true, expectedActionType: EInvoicingPivotActionType.Submit, expectedStatus: EInvoicingPivotState.Queued);
				AssertEvaluateEligibilityAndPivotQueueing_Turkey<APCreditNote>("1205", TurkeyComplianceInfo.ComplianceSubTypeCodes.DIN, pivotShouldBeCreated: true, expectedActionType: EInvoicingPivotActionType.Submit, expectedStatus: EInvoicingPivotState.Queued);
				AssertEvaluateEligibilityAndPivotQueueing_Turkey<APCreditNote>("1206", TurkeyComplianceInfo.ComplianceSubTypeCodes.DCN, parentComplianceSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.DAR, pivotShouldBeCreated: true, expectedActionType: EInvoicingPivotActionType.Cancel, expectedStatus: EInvoicingPivotState.Queued);
				AssertEvaluateEligibilityAndPivotQueueing_Turkey<APCreditNote>("1207", TurkeyComplianceInfo.ComplianceSubTypeCodes.DCN, parentComplianceSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.DIN, pivotShouldBeCreated: false);
				AssertEvaluateEligibilityAndPivotQueueing_Turkey<APCreditNote>("1208", TurkeyComplianceInfo.ComplianceSubTypeCodes.DCL, pivotShouldBeCreated: false);

				AssertEvaluateEligibilityAndPivotQueueing_Turkey<ARCreditNote>("1301", TurkeyComplianceInfo.ComplianceSubTypeCodes.CAR, pivotShouldBeCreated: false);
				AssertEvaluateEligibilityAndPivotQueueing_Turkey<ARCreditNote>("1302", TurkeyComplianceInfo.ComplianceSubTypeCodes.CIN, pivotShouldBeCreated: false);
				AssertEvaluateEligibilityAndPivotQueueing_Turkey<ARCreditNote>("1303", TurkeyComplianceInfo.ComplianceSubTypeCodes.CCN, parentComplianceSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.CAR, hasComplianceNumber: false, pivotShouldBeCreated: false);
				AssertEvaluateEligibilityAndPivotQueueing_Turkey<ARCreditNote>("1304", TurkeyComplianceInfo.ComplianceSubTypeCodes.CCN, parentComplianceSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.CIN, hasComplianceNumber: false, pivotShouldBeCreated: false);
				// The next three cases cover GetPivotActionType implementation of Turkey.
				AssertEvaluateEligibilityAndPivotQueueing_Turkey<ARCreditNote>("1305", TurkeyComplianceInfo.ComplianceSubTypeCodes.CCN, parentComplianceSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.CAR, pivotShouldBeCreated: true, expectedActionType: EInvoicingPivotActionType.Submit, expectedStatus: EInvoicingPivotState.Queued);
				AssertEvaluateEligibilityAndPivotQueueing_Turkey<ARCreditNote>("1306", TurkeyComplianceInfo.ComplianceSubTypeCodes.CCN, parentComplianceSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.CIN, pivotShouldBeCreated: true, expectedActionType: EInvoicingPivotActionType.Submit, expectedStatus: EInvoicingPivotState.Queued);
				AssertEvaluateEligibilityAndPivotQueueing_Turkey<ARCreditNote>("1307", TurkeyComplianceInfo.ComplianceSubTypeCodes.CCL, pivotShouldBeCreated: false);
			}

			T CreateInvoicingBaseTransaction<T>(string transactionNumber, string complianceSubType, bool hasComplianceNumber) where T : InvoicingBase
			{
				var transaction = Factory.NewWithValidTestData<T>();
				transaction.AH_ComplianceSubType = complianceSubType;
				transaction.AH_TransactionNum = transactionNumber;
				if (hasComplianceNumber)
				{
					transaction.AH_TransactionReference = transactionNumber;
				}
				transaction.AH_OH = ObjectCreator.DebtorTR.PK;
				Factory.Save();
				return transaction;
			}

			T2 CreateReversalTransaction<T1, T2>(T1 invoice, string transactionNumber = "", string complianceSubType = "", bool hasComplianceNumber = true)
				where T1 : InvoicingBase
				where T2 : InvoicingBase
			{
				var reversalFactory = new ReversingFactory();
				var reversing = reversalFactory.NewReversing(invoice);
				reversing.Reverse();
				var returnValue = reversing.ReverseTransaction as T2;
				if (!string.IsNullOrEmpty(transactionNumber))
				{
					returnValue.AH_TransactionNum = transactionNumber;
					if (hasComplianceNumber)
					{
						returnValue.AH_TransactionReference = transactionNumber;
					}
				}
				if (!string.IsNullOrEmpty(complianceSubType))
				{
					returnValue.AH_ComplianceSubType = complianceSubType;
				}
				returnValue.Factory.Save();
				return returnValue;
			}

			void AssertEvaluateEligibilityAndPivotQueueing_Turkey<T>(string transactionNumber, string complianceSubType, bool hasComplianceNumber = true, string parentComplianceSubType = "", bool pivotShouldBeCreated = false, string expectedActionType = "", string expectedStatus = "") where T : InvoicingBase
			{
				var transaction = CreateInvoicingBaseTransaction<T>(transactionNumber, string.IsNullOrEmpty(parentComplianceSubType) ? complianceSubType : parentComplianceSubType, hasComplianceNumber);
				var transactionPK = transaction.PK;
				switch (complianceSubType)
				{
					case TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN:
						transactionPK = CreateReversalTransaction<T, ARCreditNote>(transaction, transactionNumber, complianceSubType, hasComplianceNumber).PK;
						break;

					case TurkeyComplianceInfo.ComplianceSubTypeCodes.DAR:
					case TurkeyComplianceInfo.ComplianceSubTypeCodes.DIN:
						if (typeof(T) == typeof(APInvoice))
						{
							transactionPK = CreateReversalTransaction<T, APCreditNote>(transaction, transactionNumber, complianceSubType, hasComplianceNumber).PK;
						}
						break;

					case TurkeyComplianceInfo.ComplianceSubTypeCodes.DCN:
						transactionPK = CreateReversalTransaction<T, APInvoice>(transaction, transactionNumber, complianceSubType, hasComplianceNumber).PK;
						break;

					case TurkeyComplianceInfo.ComplianceSubTypeCodes.CCN:
						transactionPK = CreateReversalTransaction<T, ARInvoice>(transaction, transactionNumber, complianceSubType, hasComplianceNumber).PK;
						break;
				}

				var pivotQuery = GetPivotQueryForTransaction(transactionPK);
				var pivot = Factory.Load<AccEInvoicingTransactionPivot>(pivotQuery).FirstOrDefault();
				AssertEquals("Pivot", pivotShouldBeCreated, pivot != null);
				AssertEquals("Pivot Action Type", expectedActionType, pivot?.AIP_ActionType ?? string.Empty);
				AssertEquals("Pivot Status", expectedStatus, pivot?.AIP_Status ?? string.Empty);

				AssertExpectedActionTypeWithGovernmentInvoice_Turkey(transactionPK, pivot?.PK ?? ZGuid.Empty, transactionNumber, expectedActionType, complianceSubType);
			}

			void AssertExpectedActionTypeWithGovernmentInvoice_Turkey(ZGuid transactionPK, ZGuid pivotPK, string transactionNumber, string expectedActionType, string complianceSubType)
			{
				if (string.IsNullOrEmpty(expectedActionType))
				{
					return;
				}

				var transaction = Factory.Load<GovernmentInvoice>(transactionPK);
				AssertNotNull("GovernmentInvoice", transactionPK);

				transaction.AH_TransactionReference = $"GI{transactionNumber}";
				Factory.Save();

				var pivotQuery = GetPivotQueryForTransaction(transactionPK);
				var newPivot = Factory.Load<AccEInvoicingTransactionPivot>(pivotQuery).FirstOrDefault(p => p.AIP_Status == EInvoicingPivotState.Queued);
				AssertNotNull("Pivot created.", newPivot);
				AssertNotEquals("A new pivot please.", pivotPK, newPivot.PK);
				AssertEquals($"Pivot Action Type for {complianceSubType};", expectedActionType, newPivot?.AIP_ActionType ?? string.Empty);
			}

			ZQuery GetPivotQueryForTransaction(ZGuid transactionPk)
				=> new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, transactionPk)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ParentTableCode, AccTransactionHeaderSchema.Constants.Prefix);
		}

		#region El Salvador related tests

		[TestDate(2025, 5, 5)]
		public void TestElSalvadorDefaultConfiguration()
		{
			OrgHeader createOrg(bool creditorOtherwiseDebtor = false, bool? docExpired = null)
			{
				bool docAttached = docExpired.HasValue;
				var org = ObjectCreator.CreateOrgHeader("A" + ObjectCreator.UniqueNumber, creditorOtherwiseDebtor, !creditorOtherwiseDebtor);
				org.CustomsCodes.AddNew(ElSalvadorOrgCusCodeInfo.OrgCusCodes.NRC, Core.Constants.CountryCodes.ElSalvador);

				if (docAttached)
				{
					var end = ZDate.Today.AddDays(docExpired.Value ? -5 : 5);
					var start = end.AddDays(-100);

					var jobDocument = org.RequiredDocuments.AddNew();
					jobDocument.EQ_DocCategory = ReferenceTypes.ClientSupplierRelationship;
					jobDocument.EQ_DocType = RefDocTypes.VATExporterExemption;
					jobDocument.EQ_DateReceived = start.ToZDateTime().ToDateTimeOffset(null);
					jobDocument.EQ_ValidToDate = end;
					jobDocument.EQ_DocUsage = creditorOtherwiseDebtor ? JobRequiredDocument.DocUsage.Creditor : JobRequiredDocument.DocUsage.Debtor;
					jobDocument.EQ_RN_NKRelatedCountry = CountryCodes.ElSalvador;
				}

				return org;
			}

			void assert(string message, OrgHeader org, Type invoiceType, string expectComplianceSubType)
			{
				var invoice = ObjectCreator.CreateInvoice(invoiceType, ObjectCreator.UniqueNumber.ToString(), ObjectCreator.AUD, 1m, org);
				ObjectCreator.CreateInvoiceLine(invoice, 100M, ObjectCreator.AUD, 10M);
				Factory.Save();
				AssertEquals($"{invoice.AH_Ledger} {invoice.AH_TransactionType} {message}", expectComplianceSubType, invoice.AH_ComplianceSubType);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.ElSalvador))
			{
				var debtorNoDoc = createOrg(false);
				var debtorExempt = createOrg(false, false);
				var debtorExpired = createOrg(false, true);
				var creditorNoDoc = createOrg(true);
				var creditorExempt = createOrg(true, false);
				var creditorExpired = createOrg(true, true);

				Factory.Save();

				CombineAssertions(() =>
				{
					assert($"debtor no doc", debtorNoDoc, typeof(ARInvoice), "TXI");
					assert($"debtor valid doc", debtorExempt, typeof(ARInvoice), "TXE");
					assert($"debtor expired doc", debtorExpired, typeof(ARInvoice), "TXI");

					assert($"debtor no doc", debtorNoDoc, typeof(ARCreditNote), "TCR");
					assert($"debtor valid doc", debtorExempt, typeof(ARCreditNote), "");
					assert($"debtor expired doc", debtorExpired, typeof(ARCreditNote), "TCR");

					assert($"creditor no doc", creditorNoDoc, typeof(APInvoice), "");
					assert($"creditor valid doc", creditorExempt, typeof(APInvoice), "");
					assert($"creditor expired doc", creditorExpired, typeof(APInvoice), "");

					assert($"creditor no doc", creditorNoDoc, typeof(APCreditNote), "");
					assert($"creditor valid doc", creditorExempt, typeof(APCreditNote), "");
					assert($"creditor expired doc", creditorExpired, typeof(APCreditNote), "");
				});
			}
		}

		#endregion

		#region Taiwan related tests

		public void TestUpdateComplianceSubTypeForTaiwan()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				var orgWithVAT = CreateOrgHeaderForTW(true);
				var orgWithoutVAT = CreateOrgHeaderForTW(false);

				//1
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.XCL, orgWithVAT, typeof(ARInvoice), ExcludeChargeTaxID);
				//2
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC, orgWithVAT, typeof(ARInvoice), StandardRateTaxID);
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC, orgWithVAT, typeof(ARInvoice), ExemptTaxID);
				//3
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.NTI, orgWithVAT, typeof(ARInvoice), NotReportableTaxID);
				//4
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.XCL, orgWithVAT, typeof(ARCreditNote), ExcludeChargeTaxID);
				//5
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.TCR, orgWithVAT, typeof(ARCreditNote), StandardRateTaxID);
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.TCR, orgWithVAT, typeof(ARCreditNote), ExemptTaxID);
				//6
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.NTC, orgWithVAT, typeof(ARCreditNote), NotReportableTaxID);
				//7
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.XCL, orgWithoutVAT, typeof(ARInvoice), ExcludeChargeTaxID);
				//8
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.TDP, orgWithoutVAT, typeof(ARInvoice), StandardRateTaxID);
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.TDP, orgWithoutVAT, typeof(ARInvoice), ExemptTaxID);
				//9
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.NTI, orgWithoutVAT, typeof(ARInvoice), NotReportableTaxID);
				//10
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.XCL, orgWithoutVAT, typeof(ARCreditNote), ExcludeChargeTaxID);
				//11
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.TCD, orgWithoutVAT, typeof(ARCreditNote), StandardRateTaxID);
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.TCD, orgWithoutVAT, typeof(ARCreditNote), ExemptTaxID);
				//12
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.NTC, orgWithoutVAT, typeof(ARCreditNote), NotReportableTaxID);
				//13
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.XCL, orgWithVAT, typeof(APInvoice), ExcludeChargeTaxID);
				//14
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC, orgWithVAT, typeof(APInvoice), StandardRateTaxID);
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC, orgWithVAT, typeof(APInvoice), ExemptTaxID);
				//15
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.NTI, orgWithVAT, typeof(APInvoice), NotReportableTaxID);
				//16
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.XCL, orgWithVAT, typeof(APCreditNote), ExcludeChargeTaxID);
				//17
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.TCR, orgWithVAT, typeof(APCreditNote), StandardRateTaxID);
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.TCR, orgWithVAT, typeof(APCreditNote), ExemptTaxID);
				//18
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.NTC, orgWithVAT, typeof(APCreditNote), NotReportableTaxID);
				//19
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC, orgWithVAT, typeof(ARInvoice), new AccTaxRate[] { ExcludeChargeTaxID, NotReportableTaxID, StandardRateTaxID });
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC, orgWithVAT, typeof(ARInvoice), new AccTaxRate[] { NotReportableTaxID, StandardRateTaxID, ObjectCreator.GSTFREE1 });
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC, orgWithVAT, typeof(ARInvoice), new AccTaxRate[] { StandardRateTaxID, ObjectCreator.GSTFREE1, ExemptTaxID });
				//20
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.TCR, orgWithVAT, typeof(ARCreditNote), new AccTaxRate[] { ExcludeChargeTaxID, NotReportableTaxID, StandardRateTaxID });
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.TCR, orgWithVAT, typeof(ARCreditNote), new AccTaxRate[] { NotReportableTaxID, StandardRateTaxID, ObjectCreator.GSTFREE1 });
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.TCR, orgWithVAT, typeof(ARCreditNote), new AccTaxRate[] { StandardRateTaxID, ObjectCreator.GSTFREE1, ExemptTaxID });
				//21
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.TDP, orgWithoutVAT, typeof(ARInvoice), new AccTaxRate[] { ExcludeChargeTaxID, NotReportableTaxID, StandardRateTaxID });
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.TDP, orgWithoutVAT, typeof(ARInvoice), new AccTaxRate[] { NotReportableTaxID, StandardRateTaxID, ObjectCreator.GSTFREE1 });
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.TDP, orgWithoutVAT, typeof(ARInvoice), new AccTaxRate[] { StandardRateTaxID, ObjectCreator.GSTFREE1, ExemptTaxID });
				//22
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.TCD, orgWithoutVAT, typeof(ARCreditNote), new AccTaxRate[] { ExcludeChargeTaxID, NotReportableTaxID, StandardRateTaxID });
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.TCD, orgWithoutVAT, typeof(ARCreditNote), new AccTaxRate[] { NotReportableTaxID, StandardRateTaxID, ObjectCreator.GSTFREE1 });
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.TCD, orgWithoutVAT, typeof(ARCreditNote), new AccTaxRate[] { StandardRateTaxID, ObjectCreator.GSTFREE1, ExemptTaxID });
				//23
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC, orgWithVAT, typeof(APInvoice), new AccTaxRate[] { ExcludeChargeTaxID, NotReportableTaxID, StandardRateTaxID });
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC, orgWithVAT, typeof(APInvoice), new AccTaxRate[] { NotReportableTaxID, StandardRateTaxID, ObjectCreator.GSTFREE1 });
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC, orgWithVAT, typeof(APInvoice), new AccTaxRate[] { StandardRateTaxID, ObjectCreator.GSTFREE1, ExemptTaxID });
				//24
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.TCR, orgWithVAT, typeof(APCreditNote), new AccTaxRate[] { ExcludeChargeTaxID, NotReportableTaxID, StandardRateTaxID });
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.TCR, orgWithVAT, typeof(APCreditNote), new AccTaxRate[] { NotReportableTaxID, StandardRateTaxID, ObjectCreator.GSTFREE1 });
				AssertComplianceSubTypeForTW(TaiwanComplianceInfo.ComplianceSubTypeCodes.TCR, orgWithVAT, typeof(APCreditNote), new AccTaxRate[] { StandardRateTaxID, ObjectCreator.GSTFREE1, ExemptTaxID });
			}
		}

		void AssertComplianceSubTypeForTW(ZString expectComplianceSubType, OrgHeader org, Type invoiceType, AccTaxRate taxID)
		{
			AssertComplianceSubTypeForTW(expectComplianceSubType, org, invoiceType, new AccTaxRate[] { taxID });
		}

		void AssertComplianceSubTypeForTW(ZString expectComplianceSubType, OrgHeader org, Type invoiceType, AccTaxRate[] taxIDs)
		{
			var invoice = ObjectCreator.CreateInvoice(invoiceType, ObjectCreator.UniqueNumber.ToString(), ObjectCreator.TWD, 1m, org);
			foreach (var taxID in taxIDs)
			{
				var line = ObjectCreator.CreateInvoiceLine(invoice, 100M, ObjectCreator.TWD, 1M, setTaxes: false);
				line.AL_AT = taxID.PK;
			}

			Factory.Save();
			AssertEquals(expectComplianceSubType, invoice.AH_ComplianceSubType);
		}

		#endregion

		#region Turkey related tests

		#region Amended Transactions With No Excluded Tax

		public void TestComplianceSubType_Turkey_RuleSet1_Receivables_CreditNote_EArchive()
			=> SetupAndAssertAmendedTransaction<ARInvoice>(TurkeyComplianceInfo.RuleSetCodes.eFactura, TransactionTypes.CreditNote, TurkeyComplianceInfo.ComplianceSubTypeCodes.CAR, expectedParentSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR);

		public void TestComplianceSubType_Turkey_RuleSet1_Receivables_CreditNote_Basic_Previously_EArchive_Organization()
			=> SetupAndAssertAmendedTransaction<ARInvoice>(TurkeyComplianceInfo.RuleSetCodes.eFactura, TransactionTypes.CreditNote, TurkeyComplianceInfo.ComplianceSubTypeCodes.CIN, expectedParentSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR, postDefinedOrgCusCode: TurkeyOrgCusCodeInfo.OrgCusCodes.VTE);

		public void TestComplianceSubType_Turkey_RuleSet1_Receivables_CreditNote_Commercial_Previously_EArchive_Organization()
			=> SetupAndAssertAmendedTransaction<ARInvoice>(TurkeyComplianceInfo.RuleSetCodes.eFactura, TransactionTypes.CreditNote, TurkeyComplianceInfo.ComplianceSubTypeCodes.CIN, expectedParentSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR, postDefinedOrgCusCode: TurkeyOrgCusCodeInfo.OrgCusCodes.VTC);

		public void TestComplianceSubType_Turkey_RuleSet1_Receivables_CreditNote_Basic()
			=> SetupAndAssertAmendedTransaction<ARInvoice>(TurkeyComplianceInfo.RuleSetCodes.eFactura, TransactionTypes.CreditNote, TurkeyComplianceInfo.ComplianceSubTypeCodes.CIN, expectedParentSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN, preDefinedOrgCusCode: TurkeyOrgCusCodeInfo.OrgCusCodes.VTE);

		public void TestComplianceSubType_Turkey_RuleSet1_Receivables_CreditNote_Commercial()
			=> SetupAndAssertAmendedTransaction<ARInvoice>(TurkeyComplianceInfo.RuleSetCodes.eFactura, TransactionTypes.CreditNote, TurkeyComplianceInfo.ComplianceSubTypeCodes.CIN, expectedParentSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.EIC, preDefinedOrgCusCode: TurkeyOrgCusCodeInfo.OrgCusCodes.VTC);

		public void TestComplianceSubType_Turkey_RuleSet1_Receivables_Invoice_EArchive()
			=> SetupAndAssertAmendedTransaction<ARInvoice>(TurkeyComplianceInfo.RuleSetCodes.eFactura, TransactionTypes.Invoice, TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR);

		public void TestComplianceSubType_Turkey_RuleSet1_Receivables_Invoice_Basic()
			=> SetupAndAssertAmendedTransaction<ARInvoice>(TurkeyComplianceInfo.RuleSetCodes.eFactura, TransactionTypes.Invoice, TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN, preDefinedOrgCusCode: TurkeyOrgCusCodeInfo.OrgCusCodes.VTE);

		public void TestComplianceSubType_Turkey_RuleSet1_Receivables_Invoice_Commercial()
			=> SetupAndAssertAmendedTransaction<ARInvoice>(TurkeyComplianceInfo.RuleSetCodes.eFactura, TransactionTypes.Invoice, TurkeyComplianceInfo.ComplianceSubTypeCodes.EIC, preDefinedOrgCusCode: TurkeyOrgCusCodeInfo.OrgCusCodes.VTC);

		public void TestComplianceSubType_Turkey_RuleSet1_Payables_CreditNote_EArchive()
			=> SetupAndAssertAmendedTransaction<APInvoice>(TurkeyComplianceInfo.RuleSetCodes.eFactura, TransactionTypes.CreditNote, TurkeyComplianceInfo.ComplianceSubTypeCodes.DAR, expectedParentSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.PAR);

		public void TestComplianceSubType_Turkey_RuleSet1_Payables_CreditNote_Basic()
			=> SetupAndAssertAmendedTransaction<APInvoice>(TurkeyComplianceInfo.RuleSetCodes.eFactura, TransactionTypes.CreditNote, TurkeyComplianceInfo.ComplianceSubTypeCodes.DIN, expectedParentSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.PIN, preDefinedOrgCusCode: TurkeyOrgCusCodeInfo.OrgCusCodes.VTE);

		public void TestComplianceSubType_Turkey_RuleSet1_Payables_CreditNote_Commercial()
			=> SetupAndAssertAmendedTransaction<APInvoice>(TurkeyComplianceInfo.RuleSetCodes.eFactura, TransactionTypes.CreditNote, TurkeyComplianceInfo.ComplianceSubTypeCodes.DIN, expectedParentSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.PIC, preDefinedOrgCusCode: TurkeyOrgCusCodeInfo.OrgCusCodes.VTC);

		public void TestComplianceSubType_Turkey_RuleSet1_ReturnOfSales_Invoice_EArchive()
			=> SetupAndAssertAmendedTransaction<ARCreditNote>(TurkeyComplianceInfo.RuleSetCodes.eFactura, TransactionTypes.Invoice, TurkeyComplianceInfo.ComplianceSubTypeCodes.CCN, preDefinedSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.CAR);

		public void TestComplianceSubType_Turkey_RuleSet1_ReturnOfSales_Invoice_Basic()
			=> SetupAndAssertAmendedTransaction<ARCreditNote>(TurkeyComplianceInfo.RuleSetCodes.eFactura, TransactionTypes.Invoice, TurkeyComplianceInfo.ComplianceSubTypeCodes.CCN, preDefinedSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.CIN);

		public void TestComplianceSubType_Turkey_RuleSet2_Receivables_CreditNote_EArchive()
			=> SetupAndAssertAmendedTransaction<ARInvoice>(TurkeyComplianceInfo.RuleSetCodes.eArchive, TransactionTypes.CreditNote, TurkeyComplianceInfo.ComplianceSubTypeCodes.CAR, expectedParentSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR);

		public void TestComplianceSubType_Turkey_RuleSet2_Receivables_Invoice_EArchive()
			=> SetupAndAssertAmendedTransaction<ARInvoice>(TurkeyComplianceInfo.RuleSetCodes.eArchive, TransactionTypes.Invoice, TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR);

		public void TestComplianceSubType_Turkey_RuleSet2_Payables_CreditNote_EArchive()
			=> SetupAndAssertAmendedTransaction<APInvoice>(TurkeyComplianceInfo.RuleSetCodes.eArchive, TransactionTypes.CreditNote, TurkeyComplianceInfo.ComplianceSubTypeCodes.DAR, expectedParentSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.PAR);

		public void TestComplianceSubType_Turkey_RuleSet2_ReturnOfSales_Invoice_EArchive()
			=> SetupAndAssertAmendedTransaction<ARCreditNote>(TurkeyComplianceInfo.RuleSetCodes.eArchive, TransactionTypes.Invoice, TurkeyComplianceInfo.ComplianceSubTypeCodes.CCN, preDefinedSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.CAR);

		public void TestComplianceSubType_Turkey_RuleSet2_Receivables_OrgHasVTECode()
			=> SetupAndAssertAmendedTransaction<ARInvoice>(TurkeyComplianceInfo.RuleSetCodes.eArchive, TransactionTypes.Invoice, preDefinedOrgCusCode: TurkeyOrgCusCodeInfo.OrgCusCodes.VTE);

		public void TestComplianceSubType_Turkey_RuleSet2_Receivables_OrgHasVTCCode()
			=> SetupAndAssertAmendedTransaction<ARInvoice>(TurkeyComplianceInfo.RuleSetCodes.eArchive, TransactionTypes.Invoice, preDefinedOrgCusCode: TurkeyOrgCusCodeInfo.OrgCusCodes.VTC);

		#endregion

		#region Amended Transactions With Excluded Tax

		public void TestComplianceSubType_Turkey_RuleSet1_Receivables_Invoice_ToInvoiceWithExcludedTax()
			=> SetupAndAssertAmendedTransaction<ARInvoice>(TurkeyComplianceInfo.RuleSetCodes.eFactura, TransactionTypes.Invoice, TurkeyComplianceInfo.ComplianceSubTypeCodes.XCL, isTransactionWithExcludedTax: true);

		public void TestComplianceSubType_Turkey_RuleSet1_Receivables_CreditNote_ToCreditNoteWithExcludedTax()
			=> SetupAndAssertAmendedTransaction<ARCreditNote>(TurkeyComplianceInfo.RuleSetCodes.eFactura, TransactionTypes.CreditNote, TurkeyComplianceInfo.ComplianceSubTypeCodes.XCL, isTransactionWithExcludedTax: true);

		public void TestComplianceSubType_Turkey_RuleSet1_Receivables_CreditNote_ToInvoiceWithExcludedTax()
			=> SetupAndAssertAmendedTransaction<ARCreditNote>(TurkeyComplianceInfo.RuleSetCodes.eFactura, TransactionTypes.Invoice, TurkeyComplianceInfo.ComplianceSubTypeCodes.XCL, isTransactionWithExcludedTax: true);

		public void TestComplianceSubType_Turkey_RuleSet1_Receivables_Invoice_ToCreditNoteWithExcludedTax()
			=> SetupAndAssertAmendedTransaction<ARInvoice>(TurkeyComplianceInfo.RuleSetCodes.eFactura, TransactionTypes.CreditNote, TurkeyComplianceInfo.ComplianceSubTypeCodes.XCL, isTransactionWithExcludedTax: true);

		public void TestComplianceSubType_Turkey_RuleSet1_ReturnOfPurchases_Invoice_ToCreditNoteWithExcludedTax()
			=> SetupAndAssertAmendedTransaction<APInvoice>(TurkeyComplianceInfo.RuleSetCodes.eFactura, TransactionTypes.CreditNote, TurkeyComplianceInfo.ComplianceSubTypeCodes.DCL, preDefinedSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.DCL, isTransactionWithExcludedTax: true);

		public void TestComplianceSubType_Turkey_RuleSet1_Payables_Invoice_ToCreditNoteWithExcludedTax()
			=> SetupAndAssertAmendedTransaction<APInvoice>(TurkeyComplianceInfo.RuleSetCodes.eFactura, TransactionTypes.CreditNote, TurkeyComplianceInfo.ComplianceSubTypeCodes.PCL, isTransactionWithExcludedTax: true);

		public void TestComplianceSubType_Turkey_RuleSet1_ReturnOfSales_Invoice_ToInvoiceWithExcludedTax()
			=> SetupAndAssertAmendedTransaction<ARInvoice>(TurkeyComplianceInfo.RuleSetCodes.eFactura, TransactionTypes.Invoice, TurkeyComplianceInfo.ComplianceSubTypeCodes.CCL, preDefinedSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.CCL, isTransactionWithExcludedTax: true);

		public void TestComplianceSubType_Turkey_RuleSet1_ReturnOfSales_CreditNote_ToCreditNoteWithExcludedTax()
			=> SetupAndAssertAmendedTransaction<ARCreditNote>(TurkeyComplianceInfo.RuleSetCodes.eFactura, TransactionTypes.CreditNote, TurkeyComplianceInfo.ComplianceSubTypeCodes.CCL, preDefinedSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.CCL, isTransactionWithExcludedTax: true);

		public void TestComplianceSubType_Turkey_RuleSet1_ReturnOfSales_CreditNote_ToInvoiceWithExcludedTax()
			=> SetupAndAssertAmendedTransaction<ARCreditNote>(TurkeyComplianceInfo.RuleSetCodes.eFactura, TransactionTypes.Invoice, TurkeyComplianceInfo.ComplianceSubTypeCodes.CCL, preDefinedSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.CCL, isTransactionWithExcludedTax: true);

		public void TestComplianceSubType_Turkey_RuleSet1_ReturnOfSales_Invoice_ToCreditNoteWithExcludedTax()
			=> SetupAndAssertAmendedTransaction<ARInvoice>(TurkeyComplianceInfo.RuleSetCodes.eFactura, TransactionTypes.CreditNote, TurkeyComplianceInfo.ComplianceSubTypeCodes.CCL, preDefinedSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.CCL, isTransactionWithExcludedTax: true);

		public void TestComplianceSubType_Turkey_RuleSet2_Receivables_Invoice_ToInvoiceWithExcludedTax()
			=> SetupAndAssertAmendedTransaction<ARInvoice>(TurkeyComplianceInfo.RuleSetCodes.eArchive, TransactionTypes.Invoice, TurkeyComplianceInfo.ComplianceSubTypeCodes.XCL, isTransactionWithExcludedTax: true);

		public void TestComplianceSubType_Turkey_RuleSet2_Receivables_CreditNote_ToCreditNoteWithExcludedTax()
			=> SetupAndAssertAmendedTransaction<ARCreditNote>(TurkeyComplianceInfo.RuleSetCodes.eArchive, TransactionTypes.CreditNote, TurkeyComplianceInfo.ComplianceSubTypeCodes.XCL, isTransactionWithExcludedTax: true);

		public void TestComplianceSubType_Turkey_RuleSet2_Receivables_CreditNote_ToInvoiceWithExcludedTax()
			=> SetupAndAssertAmendedTransaction<ARCreditNote>(TurkeyComplianceInfo.RuleSetCodes.eArchive, TransactionTypes.Invoice, TurkeyComplianceInfo.ComplianceSubTypeCodes.XCL, isTransactionWithExcludedTax: true);

		public void TestComplianceSubType_Turkey_RuleSet2_Receivables_Invoice_ToCreditNoteWithExcludedTax()
			=> SetupAndAssertAmendedTransaction<ARInvoice>(TurkeyComplianceInfo.RuleSetCodes.eArchive, TransactionTypes.CreditNote, TurkeyComplianceInfo.ComplianceSubTypeCodes.XCL, isTransactionWithExcludedTax: true);

		public void TestComplianceSubType_Turkey_RuleSet2_ReturnOfPurchases_Invoice_ToCreditNoteWithExcludedTax()
			=> SetupAndAssertAmendedTransaction<APInvoice>(TurkeyComplianceInfo.RuleSetCodes.eArchive, TransactionTypes.CreditNote, TurkeyComplianceInfo.ComplianceSubTypeCodes.DCL, preDefinedSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.DCL, isTransactionWithExcludedTax: true);

		public void TestComplianceSubType_Turkey_RuleSet2_Payables_Invoice_ToCreditNoteWithExcludedTax()
			=> SetupAndAssertAmendedTransaction<APInvoice>(TurkeyComplianceInfo.RuleSetCodes.eArchive, TransactionTypes.CreditNote, TurkeyComplianceInfo.ComplianceSubTypeCodes.PCL, isTransactionWithExcludedTax: true);

		public void TestComplianceSubType_Turkey_RuleSet2_ReturnOfSales_Invoice_ToInvoiceWithExcludedTax()
			=> SetupAndAssertAmendedTransaction<ARInvoice>(TurkeyComplianceInfo.RuleSetCodes.eArchive, TransactionTypes.Invoice, TurkeyComplianceInfo.ComplianceSubTypeCodes.CCL, preDefinedSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.CCL, isTransactionWithExcludedTax: true);

		public void TestComplianceSubType_Turkey_RuleSet2_ReturnOfSales_CreditNote_ToCreditNoteWithExcludedTax()
			=> SetupAndAssertAmendedTransaction<ARCreditNote>(TurkeyComplianceInfo.RuleSetCodes.eArchive, TransactionTypes.CreditNote, TurkeyComplianceInfo.ComplianceSubTypeCodes.CCL, preDefinedSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.CCL, isTransactionWithExcludedTax: true);

		public void TestComplianceSubType_Turkey_RuleSet2_ReturnOfSales_CreditNote_ToInvoiceWithExcludedTax()
			=> SetupAndAssertAmendedTransaction<ARCreditNote>(TurkeyComplianceInfo.RuleSetCodes.eArchive, TransactionTypes.Invoice, TurkeyComplianceInfo.ComplianceSubTypeCodes.CCL, preDefinedSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.CCL, isTransactionWithExcludedTax: true);

		public void TestComplianceSubType_Turkey_RuleSet2_ReturnOfSales_CreditNote_ReversedTransactionWithExcludedTax()
			=> SetupAndAssertAmendedTransaction<ARInvoice>(TurkeyComplianceInfo.RuleSetCodes.eArchive, TransactionTypes.CreditNote, TurkeyComplianceInfo.ComplianceSubTypeCodes.CCL, preDefinedSubType: TurkeyComplianceInfo.ComplianceSubTypeCodes.CCL, isTransactionWithExcludedTax: true);

		#endregion

		void SetupAndAssertAmendedTransaction<T>(string ruleSet, string amendTo, string expectedSubType = "", string expectedParentSubType = "", string preDefinedSubType = "", string preDefinedOrgCusCode = "", string postDefinedOrgCusCode = "", bool isTransactionWithExcludedTax = false)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyAPComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.ToDateTime()))
			{
				var fallback = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleSet.SetValue(fallback.CompanyPK(false), Guid.Empty, Guid.Empty, new ComplianceSubTypeAttributionRuleSet(fallback, Factory, ruleSet));
				var org = ObjectCreator.CreateOrgHeader("ORG1", true, true, "TRGRS");
				if (!string.IsNullOrEmpty(preDefinedOrgCusCode))
				{
					org.CustomsCodes.AddNew(preDefinedOrgCusCode, CountryCodes.Turkey);
				}

				var trn1 = !isTransactionWithExcludedTax ? CreateInvoiceAndLineWithGST(typeof(T), org) : CreateInvoiceAndLineWithExcludedTax(typeof(T), org);
				trn1.AH_ComplianceSubType = preDefinedSubType;
				Factory.Save();

				if (!string.IsNullOrEmpty(postDefinedOrgCusCode))
				{
					org.CustomsCodes.AddNew(postDefinedOrgCusCode, CountryCodes.Turkey);
					Factory.Save();
				}

				expectedParentSubType = !string.IsNullOrEmpty(preDefinedSubType)
					? preDefinedSubType
					: string.IsNullOrEmpty(expectedParentSubType)
					? expectedSubType
					: expectedParentSubType;
				AssertEquals(expectedParentSubType, trn1.AH_ComplianceSubType);

				var trn2 = (trn1 as IAmending)?.GenerateAmendingTransaction(amendTo) as InvoicingBase;
				AssertNotNull(trn2);
				trn2.AH_TransactionNum = (trn2 is APCreditNote) ? (ZString)"100" : trn2.AH_TransactionNum;

				Factory.Save();

				AssertEquals(expectedSubType, trn2.AH_ComplianceSubType);
			}
		}

		public void TestReversalComplianceNumberAssignmentIfComplianceAllocationRegistryIsSetAsManual()
		{
			var complianceDate = ZDateTime.Today.AddDays(-10);
			using (ObjectCreator.SetUpForTestingEInvoicingTurkey_Receivables(GlbBranch.CurrentBranch.PK.ToGuid(), complianceDate.ToDateTime(), AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual))
			{
				var objectCreator = new TestObjectCreator(Factory);
				objectCreator.CreateNewComplianceSequence(GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN);

				var arInvoice = objectCreator.CreateARInvoice<ARInvoice>("0001", objectCreator.TRY, 1m, objectCreator.AALSHI);
				arInvoice.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR;
				Factory.Save();

				AssertEquals(true, arInvoice.AH_TransactionReference.IsEmpty);

				var arCreditNote = CreateReversalTransaction(arInvoice);
				arCreditNote.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN;
				Factory.Save();

				AssertEquals(true, arCreditNote.AH_TransactionReference.IsEmpty);

				var arInvoice2 = objectCreator.CreateARInvoice<ARInvoice>("0002", objectCreator.TRY, 1m, objectCreator.AALSHI);
				arInvoice2.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR;
				arInvoice2.AH_TransactionReference = "Compliance Number";
				Factory.Save();

				var arCreditNote2 = CreateReversalTransaction(arInvoice2);
				arCreditNote2.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN;
				Factory.Save();

				AssertEquals(false, arCreditNote2.AH_TransactionReference.IsEmpty);
			}
		}

		public void TestReversalComplianceNumberAssignmentIfComplianceAllocationRegistryIsSetAsGVT()
		{
			var countryCode = CountryCodes.Argentina;
			var complianceDate = ZDateTime.Today.AddDays(-10);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate))
			{
				var objectCreator = new TestObjectCreator(Factory);
				objectCreator.CreateNewComplianceSequence(GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXC);

				var arInvoice = objectCreator.CreateARInvoice<ARInvoice>("0001", objectCreator.TRY, 1m, objectCreator.AALSHI);
				arInvoice.AH_ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXA;
				arInvoice.AH_TransactionReference = "TXA2022000001";

				Factory.Save();

				var arCreditNote = CreateReversalTransaction(arInvoice);
				arCreditNote.AH_ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXC;
				Factory.Save();

				AssertEquals(true, arCreditNote.AH_TransactionReference.IsEmpty);
			}
		}

		public void TestReversalComplianceNumberAssignmentIfComplianceAllocationRegistryIsSetAsPST()
		{
			var complianceDate = ZDateTime.Today.AddDays(-10);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Turkey))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Payables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			{
				var objectCreator = new TestObjectCreator(Factory);
				objectCreator.CreateNewComplianceSequence(GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR);
				objectCreator.CreateNewComplianceSequence(GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN);

				var arInvoice = objectCreator.CreateARInvoice<ARInvoice>("0001", objectCreator.TRY, 1m, objectCreator.AALSHI);
				arInvoice.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR;

				var apInvoice = Factory.NewWithValidTestData<APInvoice>();
				apInvoice.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR;
				apInvoice.AH_TransactionNum = "0001";
				Factory.Save();

				AssertEquals(false, arInvoice.AH_TransactionReference.IsEmpty);
				AssertEquals(false, apInvoice.AH_TransactionReference.IsEmpty);

				var arCreditNote = CreateReversalTransaction(arInvoice);
				arCreditNote.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN;

				var apCreditNote = CreateReversalTransaction(apInvoice);
				apCreditNote.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN;
				apCreditNote.AH_TransactionNum = "0001";
				Factory.Save();

				AssertEquals(false, arCreditNote.AH_TransactionReference.IsEmpty);
				AssertEquals(false, apCreditNote.AH_TransactionReference.IsEmpty);
			}
		}

		public void TestReversalComplianceNumberAssignmentIfComplianceAllocationRegistryIsSetAsPRN()
		{
			var complianceDate = ZDateTime.Today.AddDays(-10);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Payables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print))
			{
				var objectCreator = new TestObjectCreator(Factory);
				objectCreator.CreateNewComplianceSequence(GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN);

				var arInvoice = objectCreator.CreateARInvoice<ARInvoice>("0001", objectCreator.TRY, 1m, objectCreator.AALSHI);
				arInvoice.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR;

				var apInvoice = Factory.NewWithValidTestData<APInvoice>();
				apInvoice.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR;
				apInvoice.AH_TransactionNum = "0001";
				Factory.Save();

				AssertEquals(true, arInvoice.AH_TransactionReference.IsEmpty);
				AssertEquals(true, apInvoice.AH_TransactionReference.IsEmpty);

				var arCreditNote = CreateReversalTransaction(arInvoice);
				arCreditNote.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN;

				var apCreditNote = CreateReversalTransaction(apInvoice);
				apCreditNote.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN;
				apCreditNote.AH_TransactionNum = "0001";
				Factory.Save();

				AssertEquals(true, arCreditNote.AH_TransactionReference.IsEmpty);
				AssertEquals(true, arCreditNote.AH_TransactionReference.IsEmpty);

				var arInvoice2 = objectCreator.CreateARInvoice<ARInvoice>("0002", objectCreator.TRY, 1m, objectCreator.AALSHI);
				arInvoice2.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR;
				arInvoice2.AH_TransactionReference = "Compliance Number";

				var apInvoice2 = Factory.NewWithValidTestData<APInvoice>();
				apInvoice2.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR;
				apInvoice2.AH_TransactionNum = "0002";
				arInvoice2.AH_TransactionReference = "Compliance Number 2";
				Factory.Save();

				var arCreditNote2 = CreateReversalTransaction(arInvoice2);
				arCreditNote2.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN;

				var apCreditNote2 = CreateReversalTransaction(apInvoice2);
				apCreditNote2.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN;
				apCreditNote2.AH_TransactionNum = "0002";
				Factory.Save();

				AssertEquals(true, arCreditNote2.AH_TransactionReference.IsEmpty);
				AssertEquals(true, apCreditNote2.AH_TransactionReference.IsEmpty);
			}
		}

		public void TestPivotIsNotCreatedForReversalTransactionIfOriginalTransactionDoesNotHaveEInvoicingPivot()
		{
			var complianceDate = ZDateTime.Today.AddDays(-10);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Turkey))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual))
			{
				var objectCreator = new TestObjectCreator(Factory);
				var signatureCredential = Factory.NewWithValidTestData<GlbCompanySignatureCredential>();
				signatureCredential.GP_GC = GlbCompany.CurrentCompany.PK;
				signatureCredential.GP_UserID = "abc";
				signatureCredential.CurrentDecryptedPassword = "1234";
				GlbCompany.CurrentCompany.SignatureCredentials.Add(signatureCredential);
				Factory.Save();

				var cusCode = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(x => x.OK_RN_NKCodeCountry == CountryCodes.Turkey && x.OK_CodeType == OrgCusCode.CodeTypes.VATCode);
				if (cusCode == null)
				{
					cusCode = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "1234567890");
					cusCode.OK_RN_NKCodeCountry = CountryCodes.Turkey;
					GlbCompany.CurrentCompany.Factory.Save();
				}

				objectCreator.CreateNewComplianceSequence(GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN);

				var arInvoice = objectCreator.CreateARInvoice<ARInvoice>("0001", objectCreator.TRY, 1m, objectCreator.AALSHI);
				arInvoice.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR;
				Factory.Save();

				AssertEquals(true, arInvoice.AH_TransactionReference.IsEmpty);
				AssertNull(arInvoice.EInvoicingTransactionPivotSubmitted);

				var creditNote = CreateReversalTransaction(arInvoice);
				creditNote.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN;

				AssertEquals(false, creditNote.IsEligibleToCreateEInvoicingTransactionPivot);
				Factory.Save();
				AssertEquals(true, creditNote.AH_TransactionReference.IsEmpty);
				var pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, creditNote.PK));
				AssertNull(pivot);

				creditNote.AH_TransactionReference = "ComplianceNumber";
				AssertEquals(false, creditNote.IsEligibleToCreateEInvoicingTransactionPivot);
				Factory.Save();
				pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, creditNote.PK));
				AssertNull(pivot);
			}
		}

		ARCreditNote CreateReversalTransaction(ARInvoice invoice)
		{
			var reversalFactory = new ReversingFactory();
			var reversing = reversalFactory.NewReversing(invoice);
			reversing.Reverse();
			return reversing.ReverseTransaction as ARCreditNote;
		}

		APCreditNote CreateReversalTransaction(APInvoice invoice)
		{
			var reversalFactory = new ReversingFactory();
			var reversing = reversalFactory.NewReversing(invoice);
			reversing.Reverse();
			return reversing.ReverseTransaction as APCreditNote;
		}

		#endregion

		#region IComplianceRuleParentTransaction Tests

		public void TestIComplianceRuleParentTransaction_ParentTransaction_ForReversedTransaction()
		{
			var invoice = ObjectCreator.CreateInvoice(typeof(ARInvoice), organisation: ObjectCreator.Debtor);
			ObjectCreator.CreateInvoiceLine(invoice, ObjectCreator.GLHeader1.PK, 10);

			var creditNote = ObjectCreator.ReverseTransaction(invoice, out string reverseError);
			AssertNull("Postcondition: reverseError", reverseError);
			AssertEquals("Parent Transaction", ((IComplianceRuleParentTransaction)creditNote).ParentTransaction, invoice);
		}

		public void TestIComplianceRuleParentTransaction_ParentTransaction_ForAmendedTransaction_With_CreditNote()
		{
			var invoice = CreateInvoiceWithRelatedJob();

			var amendCreditNote = ObjectCreator.AmendARTransaction(TransactionTypes.CreditNote, invoice);
			AssertNullOrEmpty("Error Message", amendCreditNote.errorMessage);
			AssertNotNull("Amend Credit Note must'n be null ", amendCreditNote.amendTransaction);
			AssertEquals("Parent Transaction", ((IComplianceRuleParentTransaction)amendCreditNote.amendTransaction).ParentTransaction, invoice);
		}

		public void TestIComplianceRuleParentTransaction_ParentTransaction_ForAmendedTransaction_With_Invoice()
		{
			var invoice = CreateInvoiceWithRelatedJob();

			var amendInvoice = ObjectCreator.AmendARTransaction(TransactionTypes.Invoice, invoice);
			AssertNullOrEmpty("Error Message", amendInvoice.errorMessage);
			AssertNotNull("Amend Invoice must'n be null ", amendInvoice.amendTransaction);
			AssertEquals("Parent Transaction", ((IComplianceRuleParentTransaction)amendInvoice.amendTransaction).ParentTransaction, invoice);
		}

		public void TestIComplianceRuleParentTransaction_ParentTransaction_InvalidAmendedTransaction()
		{
			var invoice = CreateInvoiceWithRelatedJob();

			var amendInvoice = ObjectCreator.AmendARTransaction(TransactionTypes.Payment, invoice);
			AssertNull("Cannot amend transaction", amendInvoice.amendTransaction);
		}

		#endregion

		#region Dominican Republic Related Tests

		public void TestComplianceSubType_DominicanRepublic_RuleSet_TaxDocumentsAndExcludedSupply_OrgHasRCS()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.DominicanRepublic))
			{
				var orgRCS = ObjectCreator.CreateOrgHeader("ORG1", true, true, "DOSQD");
				orgRCS.CustomsCodes.AddNew(DominicanRepublicOrgCusCodeInfo.OrgCusCodes.RCS, CountryCodes.DominicanRepublic);

				var invoice = CreateInvoiceAndLineWithGST(typeof(ARInvoice), orgRCS);

				var creditNote = CreateInvoiceAndLineWithGST(typeof(ARCreditNote), orgRCS);
				creditNote.AH_TransactionBelongsToGroup = invoice.PK;

				Factory.Save();

				AssertEquals($"{invoice.AH_Ledger} {invoice.AH_TransactionType}", DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.TXI, invoice.AH_ComplianceSubType);
				AssertEquals($"{creditNote.AH_Ledger} {creditNote.AH_TransactionType}", DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.TCR, creditNote.AH_ComplianceSubType);
			}
		}

		public void TestComplianceSubType_DominicanRepublic_RuleSet_TaxDocumentsAndExcludedSupply_OrgHasREG()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.DominicanRepublic))
			{
				var orgRCS = ObjectCreator.CreateOrgHeader("ORG1", true, true, "DOSQD");
				orgRCS.CustomsCodes.AddNew(DominicanRepublicOrgCusCodeInfo.OrgCusCodes.REG, CountryCodes.DominicanRepublic);

				var invoice = CreateInvoiceAndLineWithGST(typeof(ARInvoice), orgRCS);

				var creditNote = CreateInvoiceAndLineWithGST(typeof(ARCreditNote), orgRCS);
				creditNote.AH_TransactionBelongsToGroup = invoice.PK;

				Factory.Save();

				AssertEquals($"{invoice.AH_Ledger} {invoice.AH_TransactionType}", DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.TXG, invoice.AH_ComplianceSubType);
				AssertEquals($"{creditNote.AH_Ledger} {creditNote.AH_TransactionType}", DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.TCR, creditNote.AH_ComplianceSubType);
			}
		}

		public void TestComplianceSubType_DominicanRepublic_RuleSet_TaxDocumentsAndExcludedSupply_ExporterExemption()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.DominicanRepublic))
			{
				var org = ObjectCreator.CreateOrgHeader("ORG1", true, true, "DOSQD");

				var exemptReport = org.RequiredDocuments.AddNew();
				exemptReport.EQ_DocCategory = ReferenceTypes.ComplianceReport;
				exemptReport.EQ_DocType = "EXV";
				exemptReport.EQ_RN_NKRelatedCountry = CountryCodes.DominicanRepublic;
				exemptReport.EQ_DocPeriod = JobRequiredDocuments.DocumentPeriods.Periodic;
				exemptReport.EQ_DocUsage = JobRequiredDocument.DocUsage.Debtor;
				exemptReport.EQ_DateReceived = ZDateTimeOffset.Today.AddDays(-7);
				exemptReport.EQ_ValidToDate = ZDateTime.Today.AddDays(1);

				var invoice = CreateInvoiceAndLineWithFreeGST(typeof(ARInvoice), org);

				var creditNote = CreateInvoiceAndLineWithFreeGST(typeof(ARCreditNote), org);
				creditNote.AH_TransactionBelongsToGroup = invoice.PK;

				Factory.Save();

				AssertEquals($"{invoice.AH_Ledger} {invoice.AH_TransactionType}", DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.TXS, invoice.AH_ComplianceSubType);
				AssertEquals($"{creditNote.AH_Ledger} {creditNote.AH_TransactionType}", DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.TCR, creditNote.AH_ComplianceSubType);
			}
		}

		public void TestComplianceSubType_DominicanRepublic_RuleSet_TaxDocumentsAndExcludedSupply_NonExporterExemption()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.DominicanRepublic))
			{
				var org = ObjectCreator.CreateOrgHeader("ORG1", true, true, "DOSQD");

				var invoice = CreateInvoiceAndLineWithGST(typeof(ARInvoice), org);

				var creditNote = CreateInvoiceAndLineWithGST(typeof(ARCreditNote), org);
				creditNote.AH_TransactionBelongsToGroup = invoice.PK;

				Factory.Save();

				AssertEquals($"{invoice.AH_Ledger} {invoice.AH_TransactionType}", DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.TXF, invoice.AH_ComplianceSubType);
				AssertEquals($"{creditNote.AH_Ledger} {creditNote.AH_TransactionType}", DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.TCR, creditNote.AH_ComplianceSubType);
			}
		}

		public void TestComplianceSubType_DominicanRepublic_RuleSet_TaxDocumentsAndExcludedSupply_TaxRegistrationType_ExporterExemption_Empty()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.DominicanRepublic))
			{
				var org = ObjectCreator.CreateOrgHeader("ORG1", true, true, "DOSQD");

				var invoice = CreateInvoiceAndLineWithExcludedTax(typeof(APInvoice), org);

				var creditNote = CreateInvoiceAndLineWithExcludedTax(typeof(ARCreditNote), org);
				creditNote.AH_TransactionBelongsToGroup = invoice.PK;

				Factory.Save();

				AssertEquals($"{invoice.AH_Ledger} {invoice.AH_TransactionType}", DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.XCL, invoice.AH_ComplianceSubType);
				AssertEquals($"{creditNote.AH_Ledger} {creditNote.AH_TransactionType}", DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.XCL, creditNote.AH_ComplianceSubType);
			}
		}

		#endregion

		#region AR Compliance Number Allocation Tests with branch and sub-type level overrides

		[TestDate(2022, 1, 1)]
		public void TestAllocateARTransactionReference_AndCurrentLoginAllocation_AndBranchRegistryOverride()
		{
			var branch1 = ObjectCreator.CreateBranch("1PR", GlbCompany.CurrentCompany);
			var branch2 = ObjectCreator.CreateBranch("2PR", GlbCompany.CurrentCompany);
			Factory.Save();

			var collection = new ComplianceSubTypeAllocationOverrideConfigurationCollection(null, Factory, CountryCodes.Brazil);
			var nfsAndCurrentBranch = collection.AddNew();
			nfsAndCurrentBranch.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS;
			nfsAndCurrentBranch.BranchPK = Env.CurrentBranchPK;
			nfsAndCurrentBranch.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;

			var nfsAndBranch1 = collection.AddNew();
			nfsAndBranch1.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS;
			nfsAndBranch1.BranchPK = branch1.PK;
			nfsAndBranch1.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual;

			var nfsAndBranch2 = collection.AddNew();
			nfsAndBranch2.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS;
			nfsAndBranch2.BranchPK = branch2.PK;
			nfsAndBranch2.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Brazil))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationRuleReceivables.SetTemporaryValue(
					Env.CurrentCompanyPK, Guid.Empty, Guid.Empty,
					ComplianceDocumentNumberAllocationRuleTypes.LBD.Code))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationOverride_Receivables.SetTemporaryValue(
					Env.CurrentCompanyPK, Guid.Empty, Guid.Empty,
					collection))
			{
				AssertEquals("Precondition", true, GlbCompany.CurrentCompany.Country.SupportComplianceSubType);
				AssertEquals("Precondition", false, AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value);
				AssertNotEquals("Precondition: default allocation level will not post", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post, AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.Value);

				var currentBranchSequence = ObjectCreator.SetupComplianceSequence(ZGuid.Empty, BrazilComplianceInfo.ComplianceSubTypeCodes.NFS, "CUR", 1, 100, 1, allocationLevel: "BRN");

				var branch1Sequence = ObjectCreator.SetupComplianceSequence(ZGuid.Empty, BrazilComplianceInfo.ComplianceSubTypeCodes.NFS, "B01", 101, 200, 101, allocationLevel: "BRN");
				branch1Sequence.XD_GB_BranchOwner = branch1.PK;

				var branch2Sequence = ObjectCreator.SetupComplianceSequence(ZGuid.Empty, BrazilComplianceInfo.ComplianceSubTypeCodes.NFS, "B02", 201, 300, 201, allocationLevel: "BRN");
				branch2Sequence.XD_GB_BranchOwner = branch2.PK;

				Factory.Save();

				var currentLoginInvoice = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID }, false, CountryCodes.Brazil, ZString.Empty, false);
				currentLoginInvoice.AH_ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS;
				currentLoginInvoice.AH_PostDate = ZDateTime.Today.AddDays(1);
				Factory.Save();
				AssertEquals("Compliance Sequence allocated for current login branch + NFS rule", currentBranchSequence.PK, currentLoginInvoice.AH_XD_ComplianceBook);
				AssertEquals("Compliance Number allocated for current login branch + NFS rule", "CUR000000001", currentLoginInvoice.AH_TransactionReference);

				var branch1Invoice = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID }, false, CountryCodes.Brazil, ZString.Empty, false);
				branch1Invoice.AH_GB = branch1.PK;
				branch1Invoice.AH_ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS;
				branch1Invoice.AH_PostDate = ZDateTime.Today.AddDays(2);
				Factory.Save();
				AssertEquals("Compliance Sequence allocated for current login branch + NFS rule", currentBranchSequence.PK, branch1Invoice.AH_XD_ComplianceBook);
				AssertEquals("Compliance Number allocated for current login branch + NFS rule", "CUR000000002", branch1Invoice.AH_TransactionReference);

				var branch2Invoice = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID }, false, CountryCodes.Brazil, ZString.Empty, false);
				branch2Invoice.AH_GB = branch2.PK;
				branch2Invoice.AH_ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS;
				branch2Invoice.AH_PostDate = ZDateTime.Today.AddDays(3);
				Factory.Save();
				AssertEquals("Compliance Sequence allocated for current login branch + NFS rule", currentBranchSequence.PK, branch2Invoice.AH_XD_ComplianceBook);
				AssertEquals("Compliance Number allocated for current login branch + NFS rule", "CUR000000003", branch2Invoice.AH_TransactionReference);
			}
		}

		[TestDate(2022, 1, 1)]
		public void TestAllocateARTransactionReference_AndTransactionHeaderAllocation_AndBranchRegistryOverride()
		{
			var branch1 = ObjectCreator.CreateBranch("1PR", GlbCompany.CurrentCompany);
			var branch2 = ObjectCreator.CreateBranch("2PR", GlbCompany.CurrentCompany);
			Factory.Save();

			var collection = new ComplianceSubTypeAllocationOverrideConfigurationCollection(null, Factory, CountryCodes.Brazil);
			var nfsAndCurrentBranch = collection.AddNew();
			nfsAndCurrentBranch.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS;
			nfsAndCurrentBranch.BranchPK = Env.CurrentBranchPK;
			nfsAndCurrentBranch.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;

			var nfsAndBranch1 = collection.AddNew();
			nfsAndBranch1.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS;
			nfsAndBranch1.BranchPK = branch1.PK;
			nfsAndBranch1.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;

			var nfsAndBranch2 = collection.AddNew();
			nfsAndBranch2.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS;
			nfsAndBranch2.BranchPK = branch2.PK;
			nfsAndBranch2.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Brazil))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationRuleReceivables.SetTemporaryValue(
					Env.CurrentCompanyPK, Guid.Empty, Guid.Empty,
					ComplianceDocumentNumberAllocationRuleTypes.HBD.Code))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationOverride_Receivables.SetTemporaryValue(
					Env.CurrentCompanyPK, Guid.Empty, Guid.Empty,
					collection))
			{
				AssertEquals("Precondition", true, GlbCompany.CurrentCompany.Country.SupportComplianceSubType);
				AssertEquals("Precondition", false, AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value);
				AssertNotEquals("Precondition: default allocation level will not post", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post, AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.Value);

				var currentBranchSequence = ObjectCreator.SetupComplianceSequence(ZGuid.Empty, BrazilComplianceInfo.ComplianceSubTypeCodes.NFS, "CUR", 1, 100, 1, allocationLevel: "BRN");

				var branch1Sequence = ObjectCreator.SetupComplianceSequence(ZGuid.Empty, BrazilComplianceInfo.ComplianceSubTypeCodes.NFS, "01B", 101, 200, 101, allocationLevel: "BRN");
				branch1Sequence.XD_GB_BranchOwner = branch1.PK;

				var branch2Sequence = ObjectCreator.SetupComplianceSequence(ZGuid.Empty, BrazilComplianceInfo.ComplianceSubTypeCodes.NFS, "02B", 201, 300, 201, allocationLevel: "BRN");
				branch2Sequence.XD_GB_BranchOwner = branch2.PK;

				Factory.Save();

				var currentLoginInvoice = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID }, false, CountryCodes.Brazil, ZString.Empty, false);
				currentLoginInvoice.AH_ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS;
				currentLoginInvoice.AH_PostDate = ZDateTime.Today.AddDays(1);
				Factory.Save();
				AssertEquals("Compliance Sequence allocated for transaction header branch (same as login branch) + NFS rule", currentBranchSequence.PK, currentLoginInvoice.AH_XD_ComplianceBook);
				AssertEquals("Compliance Number allocated for transaction header branch (same as login branch) + NFS rule", "CUR000000001", currentLoginInvoice.AH_TransactionReference);

				var branch1Invoice = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID }, false, CountryCodes.Brazil, ZString.Empty, false);
				branch1Invoice.AH_GB = branch1.PK;
				branch1Invoice.AH_ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS;
				branch1Invoice.AH_PostDate = ZDateTime.Today.AddDays(2);
				Factory.Save();
				AssertEquals("Compliance Sequence allocated for transaction header branch + NFS rule", branch1Sequence.PK, branch1Invoice.AH_XD_ComplianceBook);
				AssertEquals("Compliance Number allocated for transaction header branch + NFS rule", "01B000000101", branch1Invoice.AH_TransactionReference);

				var branch2Invoice = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID }, false, CountryCodes.Brazil, ZString.Empty, false);
				branch2Invoice.AH_GB = branch2.PK;
				branch2Invoice.AH_ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS;
				branch2Invoice.AH_PostDate = ZDateTime.Today.AddDays(3);
				Factory.Save();
				AssertEquals("Compliance Sequence not allocated for transaction header branch as registry rule for branch2 is MAN", ZGuid.Empty, branch2Invoice.AH_XD_ComplianceBook);
				AssertEquals("Compliance Number not allocated for current login branch as registry rule for branch2 is MAN", "", branch2Invoice.AH_TransactionReference);
			}
		}

		[TestDate(2022, 1, 1)]
		public void TestAllocateARTransactionReference_AndTransactionHeaderAllocation_AndSubTypeRegistryOverride()
		{
			var branch1 = ObjectCreator.CreateBranch("1PR", GlbCompany.CurrentCompany);
			Factory.Save();

			var collection = new ComplianceSubTypeAllocationOverrideConfigurationCollection(null, Factory, CountryCodes.Brazil);
			var cneAndCurrentBranch = collection.AddNew();
			cneAndCurrentBranch.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.CNE;
			cneAndCurrentBranch.BranchPK = Env.CurrentBranchPK;
			cneAndCurrentBranch.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual;

			var cneNoBranch = collection.AddNew();
			cneNoBranch.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.CNE;
			cneNoBranch.BranchPK = ZGuid.Empty;
			cneNoBranch.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;

			var xndNoBranch = collection.AddNew();
			xndNoBranch.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.XND;
			xndNoBranch.BranchPK = ZGuid.Empty;
			xndNoBranch.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Brazil))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationRuleReceivables.SetTemporaryValue(
					Env.CurrentCompanyPK, Guid.Empty, Guid.Empty,
					ComplianceDocumentNumberAllocationRuleTypes.HBD.Code))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationOverride_Receivables.SetTemporaryValue(
					Env.CurrentCompanyPK, Guid.Empty, Guid.Empty,
					collection))
			{
				AssertEquals("Precondition", true, GlbCompany.CurrentCompany.Country.SupportComplianceSubType);
				AssertEquals("Precondition", false, AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value);
				AssertNotEquals("Precondition: default allocation level will not post", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post, AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.Value);

				var currentBranchCNESequence = ObjectCreator.SetupComplianceSequence(ZGuid.Empty, BrazilComplianceInfo.ComplianceSubTypeCodes.CNE, "CNE", 1, 100, 1, allocationLevel: "BRN");
				var currentBranchXNDSequence = ObjectCreator.SetupComplianceSequence(ZGuid.Empty, BrazilComplianceInfo.ComplianceSubTypeCodes.XND, "XND", 1, 100, 1, allocationLevel: "BRN");
				var branch1CNESequence = ObjectCreator.SetupComplianceSequence(ZGuid.Empty, BrazilComplianceInfo.ComplianceSubTypeCodes.CNE, "01B", 101, 200, 101, allocationLevel: "BRN");
				branch1CNESequence.XD_GB_BranchOwner = branch1.PK;

				Factory.Save();

				var currentLoginXNDInvoice = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID }, false, CountryCodes.Brazil, ZString.Empty, false);
				currentLoginXNDInvoice.AH_ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.XND;
				currentLoginXNDInvoice.AH_PostDate = ZDateTime.Today.AddDays(2);
				Factory.Save();
				AssertEquals("Compliance Sequence allocated for XND rule", currentBranchXNDSequence.PK, currentLoginXNDInvoice.AH_XD_ComplianceBook);
				AssertEquals("Compliance Number allocated for XND rule", "XND000000001", currentLoginXNDInvoice.AH_TransactionReference);

				var branch1Invoice = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID }, false, CountryCodes.Brazil, ZString.Empty, false);
				branch1Invoice.AH_GB = branch1.PK;
				branch1Invoice.AH_ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.CNE;
				branch1Invoice.AH_PostDate = ZDateTime.Today.AddDays(3);
				Factory.Save();
				AssertEquals("Compliance Sequence allocated for CNE + branch1 rule", branch1CNESequence.PK, branch1Invoice.AH_XD_ComplianceBook);
				AssertEquals("Compliance Number allocated for CNE + branch1 rule", "01B000000101", branch1Invoice.AH_TransactionReference);

				var currentLoginCNEInvoice = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID }, false, CountryCodes.Brazil, ZString.Empty, false);
				currentLoginCNEInvoice.AH_ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.CNE;
				currentLoginCNEInvoice.AH_PostDate = ZDateTime.Today.AddDays(1);
				Factory.Save();
				AssertEquals("Compliance Sequence not allocated because CNE + current branch override is MAN allocation method", ZGuid.Empty, currentLoginCNEInvoice.AH_XD_ComplianceBook);
				AssertEquals("Compliance Number not allocated because CNE + current branch override is MAN allocation method", ZString.Empty, currentLoginCNEInvoice.AH_TransactionReference);
			}
		}

		[TestDate(2022, 1, 1)]
		public void TestAllocateARTransactionReference_AndTransactionHeaderAllocation_AndDefaultRegistryOverride()
		{
			var collection = new ComplianceSubTypeAllocationOverrideConfigurationCollection(null, Factory, CountryCodes.Brazil);

			var cneNoBranch = collection.AddNew();
			cneNoBranch.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.CNE;
			cneNoBranch.BranchPK = ZGuid.Empty;
			cneNoBranch.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Brazil))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationRuleReceivables.SetTemporaryValue(
					Env.CurrentCompanyPK, Guid.Empty, Guid.Empty,
					ComplianceDocumentNumberAllocationRuleTypes.HBD.Code))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationOverride_Receivables.SetTemporaryValue(
					Env.CurrentCompanyPK, Guid.Empty, Guid.Empty,
					collection))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(
					Env.CurrentCompanyPK, Guid.Empty, Guid.Empty,
					AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			{
				AssertEquals("Precondition", true, GlbCompany.CurrentCompany.Country.SupportComplianceSubType);
				AssertEquals("Precondition", false, AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value);
				AssertEquals("Precondition: default allocation level will post", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post, AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.Value);

				var currentBranchCNESequence = ObjectCreator.SetupComplianceSequence(ZGuid.Empty, BrazilComplianceInfo.ComplianceSubTypeCodes.CNE, "CNE", 1, 100, 1, allocationLevel: "BRN");
				var currentBranchXNDSequence = ObjectCreator.SetupComplianceSequence(ZGuid.Empty, BrazilComplianceInfo.ComplianceSubTypeCodes.XND, "XND", 1, 100, 1, allocationLevel: "BRN");

				Factory.Save();

				var cneInvoice = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID }, false, CountryCodes.Brazil, ZString.Empty, false);
				cneInvoice.AH_ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.CNE;
				cneInvoice.AH_PostDate = ZDateTime.Today.AddDays(1);
				Factory.Save();
				AssertEquals("Compliance Sequence not allocated because CNE + branch override is MAN allocation method", ZGuid.Empty, cneInvoice.AH_XD_ComplianceBook);
				AssertEquals("Compliance Number not allocated because CNE + branch override is MAN allocation method", ZString.Empty, cneInvoice.AH_TransactionReference);

				var xndInvoice = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID }, false, CountryCodes.Brazil, ZString.Empty, false);
				xndInvoice.AH_ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.XND;
				xndInvoice.AH_PostDate = ZDateTime.Today.AddDays(2);
				Factory.Save();
				AssertEquals("Compliance Sequence allocated because no XND rules mean fallback to ComplianceDocumentNumberAllocation registry", currentBranchXNDSequence.PK, xndInvoice.AH_XD_ComplianceBook);
				AssertEquals("Compliance Number allocated because no XND rules mean fallback to ComplianceDocumentNumberAllocation registry", "XND000000001", xndInvoice.AH_TransactionReference);
			}
		}

		#endregion

		#region IDocAddresses

		public void TestCreateJobDocAddress_IsPrintBranchAddressInFooter()
		{
			TestCreateJobDocAddress(true);
		}

		public void TestCreateJobDocAddress_NotPrintBranchAddressInFooter()
		{
			TestCreateJobDocAddress(false);
		}

		void TestCreateJobDocAddress(bool printBranchAddressInFooter)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
			{
				AccountingConfigurationRegistry.Instance.PrintBranchAddressInFooter.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, printBranchAddressInFooter);
				AccountingConfigurationRegistry.Instance.ARStoreAndUseInvoiceIssuerAndRecepientInformationDuringPostWhenPrinting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var invoice = ObjectCreator.CreateARInvoice<ARInvoice>("AR001", ObjectCreator.AUD, 1m, ObjectCreator.Debtor);

				var companyAddressForSendingARDocuments = ObjectCreator.AALSHI.Addresses.AddNew(OrgAddressType.Receivables, true);
				companyAddressForSendingARDocuments.OA_Address1 = "CompanyARAddress1";
				companyAddressForSendingARDocuments.OA_Address2 = "CompanyARAddress2";
				companyAddressForSendingARDocuments.OA_City = "CompanyARCity";
				companyAddressForSendingARDocuments.OA_State = "CompanyARState";
				companyAddressForSendingARDocuments.OA_PostCode = "1";
				companyAddressForSendingARDocuments.CompanyName = "CompanyARCompany";

				var branchAddressForSendingARDocuments = ObjectCreator.ABIGAS.Addresses.AddNew(OrgAddressType.Receivables, true);
				branchAddressForSendingARDocuments.OA_Address1 = "BranchARAddress1";
				branchAddressForSendingARDocuments.OA_Address2 = "BranchARAddress2";
				branchAddressForSendingARDocuments.OA_City = "BranchARCity";
				branchAddressForSendingARDocuments.OA_State = "BranchARState";
				branchAddressForSendingARDocuments.OA_PostCode = "2";
				branchAddressForSendingARDocuments.CompanyName = "BranchARCompany";

				var debtorOverrideAddress = ObjectCreator.CreateAddress(ObjectCreator.Debtor, CountryCodes.Australia, SharedConstants.Languages.English, "Debtor Test Address", "", OrgAddressType.Receivables, "");
				debtorOverrideAddress.OA_Address1 = "DebtorARAddress1";
				debtorOverrideAddress.OA_Address2 = "DebtorARAddress2";
				debtorOverrideAddress.OA_City = "DebtorARCity";
				debtorOverrideAddress.OA_State = "DebtorARState";
				debtorOverrideAddress.OA_PostCode = "3";
				debtorOverrideAddress.CompanyName = "DebtorARCompany";
				debtorOverrideAddress.OA_RN_NKCountryCode = "PT";
				invoice.AH_OA_InvoiceAddressOverride = debtorOverrideAddress.PK;

				invoice.Company.GC_OH_OrgProxy = ObjectCreator.AALSHI.PK;
				invoice.Branch.GB_OH_OrgProxy = ObjectCreator.ABIGAS.PK;
				invoice.AH_OC_InvoiceContactOverride = ObjectCreator.CreateContact(ObjectCreator.Debtor, "Test contact").PK;

				Factory.Save();

				var debtorDocAddressQuery = new ZQuery(JobDocAddressSchema.E2_ParentID, invoice.PK);
				debtorDocAddressQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, "DBT");

				var debtorDocAddress = Factory.LoadTop1<JobDocAddress>(debtorDocAddressQuery);
				AssertNotNull(debtorDocAddress);
				AssertEquals(debtorDocAddress.E2_Address1, "DebtorARAddress1");
				AssertEquals(debtorDocAddress.E2_Address2, "DebtorARAddress2");
				AssertEquals(debtorDocAddress.E2_City, "DebtorARCity");
				AssertEquals(debtorDocAddress.E2_State, "DebtorARState");
				AssertEquals(debtorDocAddress.E2_Postcode, "3");
				AssertEquals(debtorDocAddress.E2_CompanyName, "DebtorARCompany");
				AssertEquals(debtorDocAddress.E2_ParentID, invoice.PK);
				AssertEquals(debtorDocAddress.E2_ParentTableCode, "AH");
				AssertEquals(debtorDocAddress.E2_OA_Address, OrganisationsDataRegistry.Instance.MiscOrganisation.Value.PrimaryOfficeAddress);
				AssertEquals(debtorDocAddress.E2_AddressOverride, false);
				AssertEquals(debtorDocAddress.E2_ValidationStatus, AddressValidationStatus.NotRequired);
				AssertEquals(debtorDocAddress.E2_RN_NKCountryCode, "PT");
				AssertEquals(debtorDocAddress.E2_Contact, "Test contact");

				var docAddressPK = printBranchAddressInFooter ? branchAddressForSendingARDocuments.PK : companyAddressForSendingARDocuments.PK;

				var docAddressQuery = new ZQuery(JobDocAddressSchema.E2_ParentID, invoice.PK);
				docAddressQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, "BCP");

				var docAddress = Factory.LoadTop1<JobDocAddress>(docAddressQuery);
				AssertNotNull(docAddress);
				AssertEquals(docAddress.E2_Address1, printBranchAddressInFooter ? "BranchARAddress1" : "CompanyARAddress1");
				AssertEquals(docAddress.E2_Address2, printBranchAddressInFooter ? "BranchARAddress2" : "CompanyARAddress2");
				AssertEquals(docAddress.E2_City, printBranchAddressInFooter ? "BranchARCity" : "CompanyARCity");
				AssertEquals(docAddress.E2_State, printBranchAddressInFooter ? "BranchARState" : "CompanyARState");
				AssertEquals(docAddress.E2_Postcode, printBranchAddressInFooter ? "2" : "1");
				AssertEquals(docAddress.E2_CompanyName, printBranchAddressInFooter ? "BranchARCompany" : "CompanyARCompany");
				AssertEquals(docAddress.E2_ParentID, invoice.PK);
				AssertEquals(docAddress.E2_ParentTableCode, "AH");
				AssertEquals(docAddress.E2_OA_Address, OrganisationsDataRegistry.Instance.MiscOrganisation.Value.PrimaryOfficeAddress);
				AssertEquals(docAddress.E2_AddressOverride, false);
				AssertEquals(docAddress.E2_ValidationStatus, AddressValidationStatus.NotRequired);
				AssertEquals(docAddress.E2_RN_NKCountryCode, "PT");
			}
		}

		#endregion

		#region Helpers

		InvoicingBase CreateInvoice(AccComplianceSequence sequence, ZString transNum, ZDateTime allocationDate, ZDateTime otherDate, string dateOption)
		{
			var invoice = ObjectCreator.CreateInvoice(typeof(ARInvoice), invoiceDate: ZDateTime.Now);
			invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoice.AH_GC = sequence?.XD_GC_Company ?? GlbCompany.CurrentCompany.PK;
			invoice.AH_GB = sequence?.XD_GB_BranchOwner ?? GlbBranch.CurrentBranch.PK;
			invoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
			invoice.AH_ComplianceSubType = sequence?.XD_SequenceClass ?? ZString.Empty;

			if (dateOption == ComplianceNumberAllocationDateOptions.InvoiceDate.Code)
			{
				invoice.AH_PostDate = otherDate;
				invoice.AH_InvoiceDate = allocationDate;
			}
			else
			{
				invoice.AH_PostDate = allocationDate;
				invoice.AH_InvoiceDate = otherDate;
			}

			invoice.AH_TransactionNum = transNum;

			return invoice;
		}

		InvoicingBase CreateInvoice(AccComplianceSequence sequence, ZString transNum, ZDateTime allocationDate)
		{
			var invoice = ObjectCreator.CreateInvoice(typeof(ARInvoice), invoiceDate: ZDateTime.Now);
			invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoice.AH_GC = sequence?.XD_GC_Company ?? GlbCompany.CurrentCompany.PK;
			invoice.AH_GB = sequence?.XD_GB_BranchOwner ?? GlbBranch.CurrentBranch.PK;
			invoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
			invoice.AH_ComplianceSubType = sequence?.XD_SequenceClass ?? ZString.Empty;
			invoice.AH_PostDate = allocationDate;
			invoice.AH_InvoiceDate = allocationDate;
			invoice.AH_TransactionNum = transNum;

			return invoice;
		}

		InvoicingBase CreateInvoiceWithRelatedJob()
		{
			var shipment = ObjectCreator.CreateShipment("S001");
			var job = ObjectCreator.CreateJob(shipment, false);
			var charge = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, osSellAmt: 100, debtor: ObjectCreator.Debtor);
			AssertNotNull(charge);
			var transactions = ObjectCreator.PostJobAsBillingTab(job, JobInvoicingPostingOption.Revenue);
			var invoice = transactions.GetAllARInvoicesAndCreditNotes()[0];
			AssertNotNull(invoice);
			Factory.Save();
			return invoice;
		}

		InvoicingBase CreateInvoiceAndLineWithGST(Type type, OrgHeader org)
		{
			var invoice = ObjectCreator.CreateInvoice(type, organisation: org);
			ObjectCreator.CreateInvoiceLine(invoice, 100M, ObjectCreator.AUD, 10M, setTaxes: true);

			return invoice;
		}

		InvoicingBase CreateInvoiceAndLineWithExcludedTax(Type type, OrgHeader org)
		{
			var invoice = ObjectCreator.CreateInvoice(type, organisation: org);
			var line = ObjectCreator.CreateInvoiceLine(invoice, 100M, ObjectCreator.AUD, 10M, setTaxes: false);
			line.AL_AT = ObjectCreator.ExcludedTax.PK;

			return invoice;
		}

		InvoicingBase CreateInvoiceAndLineWithFreeGST(Type type, OrgHeader org)
		{
			var invoice = ObjectCreator.CreateInvoice(type, organisation: org);
			var line = ObjectCreator.CreateInvoiceLine(invoice, 100M, ObjectCreator.AUD, 10M, setTaxes: false);
			line.AL_AT = ObjectCreator.GSTFREE1.PK;
			return invoice;
		}

		InvoicingBase CreateInvoiceForComplianceSubTypeTest(ZString ledger, ZString transactionType, AccTaxRate[] taxIDsToUse, bool isSelfBillingInvoice, ZString organisationLocationRule, ZString taxRegistrationLocationRule, bool makeOrgPartOfLoginCompanyVATGroup, bool addCMTLine = false)
		{
			OrgHeader invoiceOrg = ObjectCreator.CreateOrgHeader(RandomNumberGenerator.Next(1000000).ToString(), true, true, true, false, true, false);
			if (!organisationLocationRule.IsEmpty)
			{
				invoiceOrg.OH_RL_NKClosestPort = GetPortBasedOnLocationRule(organisationLocationRule);
			}
			if (!taxRegistrationLocationRule.IsEmpty)
			{
				ZString port = GetPortBasedOnLocationRule(taxRegistrationLocationRule);
				invoiceOrg.OH_RL_NKClosestPort = port;

				var cusCode = invoiceOrg.CustomsCodes.AddNew();
				cusCode.OK_RN_NKCodeCountry = port.Left(2);
				cusCode.OK_CodeType = ZArchitecture.Environment.Country.GetConsumptionTaxRegistrationOrgCusCode(cusCode.OK_RN_NKCodeCountry);
				cusCode.OK_CustomsRegNo = "VATREG123";
			}
			if (makeOrgPartOfLoginCompanyVATGroup)
			{
				ZString vATGroupRegistration = "VATGROUPREG123";
				if (taxRegistrationLocationRule.IsEmpty)
				{
					ZString port = GetPortBasedOnLocationRule(TaxRegRuleCodes.EUExcludingLoginCountry);
					invoiceOrg.OH_RL_NKClosestPort = port;

					var cusCode = invoiceOrg.CustomsCodes.AddNew();
					cusCode.OK_RN_NKCodeCountry = port.Left(2);
					cusCode.OK_CodeType = ZArchitecture.Environment.Country.GetConsumptionTaxRegistrationOrgCusCode(cusCode.OK_RN_NKCodeCountry);
					cusCode.OK_CustomsRegNo = vATGroupRegistration;
				}
				else
				{
					invoiceOrg.CustomsCodes[0].OK_CustomsRegNo = vATGroupRegistration;
				}

				GlbCompany.CurrentCompany.GC_BusinessRegNo = vATGroupRegistration;
			}
			InvoicingBase testInvoice = null;
			int randomTransactionNumber = RandomNumberGenerator.Next(10000000);
			if (ledger == LedgerTypes.AccountsReceivable)
			{
				if (transactionType == TransactionTypes.Invoice)
				{
					testInvoice = ObjectCreator.CreateInvoice(typeof(ARInvoice), randomTransactionNumber.ToString(), null, null, invoiceOrg);
				}
				else if (transactionType == TransactionTypes.CreditNote)
				{
					testInvoice = ObjectCreator.CreateInvoice(typeof(ARCreditNote), randomTransactionNumber.ToString(), null, null, invoiceOrg);
				}
				else
				{
					throw new NotSupportedException(string.Format("This method only supports AR INV/CRD and AP INV/CRD. You provided Ledger = {0}, Transaction Type = {1}", ledger, transactionType));
				}
			}
			else if (ledger == LedgerTypes.AccountsPayable)
			{
				if (transactionType == TransactionTypes.Invoice)
				{
					testInvoice = ObjectCreator.CreateInvoice(typeof(APInvoice), randomTransactionNumber.ToString(), null, null, invoiceOrg);
				}
				else if (transactionType == TransactionTypes.CreditNote)
				{
					testInvoice = ObjectCreator.CreateInvoice(typeof(APCreditNote), randomTransactionNumber.ToString(), null, null, invoiceOrg);
				}
				else
				{
					throw new NotSupportedException(string.Format("This method only supports AR INV/CRD and AP INV/CRD. You provided Ledger = {0}, Transaction Type = {1}", ledger, transactionType));
				}
			}
			else
			{
				throw new NotSupportedException(string.Format("This method only supports AR INV/CRD and AP INV/CRD. You provided Ledger = {0}, Transaction Type = {1}", ledger, transactionType));
			}
			foreach (AccTaxRate taxRate in taxIDsToUse)
			{
				InvoicingLineBase line = (InvoicingLineBase)testInvoice.Lines.AddNew();
				line.GenericCharge = ObjectCreator.CC1.PK;
				line.AL_JH = ObjectCreator.Job1.PK;
				line.AL_OSExTaxAmount = 100m;
				line.AL_AT = taxRate.PK;

				ObjectCreator.CreateCharge(line);
			}
			if (addCMTLine)
			{
				var line = (InvoicingLineBase)testInvoice.Lines.AddNew();
				line.GenericCharge = ObjectCreator.CommentChargeCode.PK;
				line.AL_JH = ObjectCreator.Job1.PK;
				ObjectCreator.CreateCharge(line);
			}

			if (isSelfBillingInvoice)
			{
				testInvoice.AH_TransactionCategory = InvoiceTypesList.Codes.SelfBillingInvoice;
			}
			return testInvoice;
		}

		ZString GetPortBasedOnLocationRule(ZString locationRule)
		{
			RefUNLOCO uNLOCO = null;
			if (locationRule.Length == 2 && RefCountry.LoadFromCountryCode(Factory, locationRule) != null)
			{
				uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, locationRule));
			}
			else if (locationRule == TaxRegRuleCodes.EUExcludingLoginCountry)
			{
				ZQuery eUCountryFilter = new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				eUCountryFilter.AddToFilter(RefCountrySchema.RN_EconomicGrouping, "EUN");
				RefCountry eUCountry = Factory.LoadTop1<RefCountry>(eUCountryFilter);

				uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, eUCountry.RN_Code));
			}
			else if (locationRule == TaxRegRuleCodes.NotEU)
			{
				ZQuery nonEUCountryFilter = new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				nonEUCountryFilter.AddToFilter(RefCountrySchema.RN_EconomicGrouping, SQLComparisonOperator.NotEqual, "EUN");
				RefCountry nonEUCountry = Factory.LoadTop1<RefCountry>(nonEUCountryFilter);

				uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, nonEUCountry.RN_Code));
			}
			else if (!locationRule.IsEmpty)
			{
				throw new NotSupportedException("");
			}
			return uNLOCO == null ? ZString.Empty : uNLOCO.RL_Code;
		}

		OrgHeader CreateOrgHeaderForTW(bool hasRegistrationNumber)
		{
			var twOrg = Factory.New<OrgHeader>();
			twOrg.OH_IsActive = true;
			twOrg.OH_IsDebtor = true;
			twOrg.OH_IsConsignee = true;
			twOrg.OH_IsConsignor = true;
			twOrg.OH_IsShippingProvider = true;
			twOrg.OH_IsMiscFreightServices = true;
			twOrg.OH_RL_NKClosestPort = "TWTPE";
			twOrg.OH_FullName = "TAIWAN TEST COMPANY";
			twOrg.OH_Code = "TAITESTPE";
			twOrg.MainAddress.OA_Address1 = "184 Bourke Road";
			twOrg.MainAddress.OA_City = "Taipei";
			twOrg.MainAddress.OA_PostCode = "123456";

			if (hasRegistrationNumber)
			{
				var cusCode = twOrg.CustomsCodes.AddNew();
				cusCode.OK_RN_NKCodeCountry = "TW";
				cusCode.OK_CodeType = "VAT";
				cusCode.OK_CustomsRegNo = "12345675";
			}

			twOrg.CompanyData.SetAPTaxApplicable(true);
			twOrg.CompanyData.SetARTaxApplicable(true);

			Factory.Save();

			return Factory.Load<OrgHeader>(twOrg.PK);
		}

		TestObjectCreator ObjectCreator
		{
			get
			{
				if (fObjectCreator == null)
				{
					fObjectCreator = new TestObjectCreator(Factory);
				}
				return fObjectCreator;
			}
		}
		TestObjectCreator fObjectCreator;

		Random RandomNumberGenerator
		{
			get
			{
				if (fRandomNumberGenerator == null)
				{
					fRandomNumberGenerator = new Random();
				}
				return fRandomNumberGenerator;
			}
		}
		Random fRandomNumberGenerator;

		AccTaxRate StandardRateTaxID
		{
			get
			{
				if (fStandardRateTaxID == null)
				{
					fStandardRateTaxID = ObjectCreator.CreateTaxRate("VAT1", "10% Standard VAT", AccTaxRate.Types.Rated, 10, ZString.Empty, 0, 1);
				}
				return fStandardRateTaxID;
			}
		}
		AccTaxRate fStandardRateTaxID;

		AccTaxRate ExemptTaxID
		{
			get
			{
				if (fExemptTaxID == null)
				{
					fExemptTaxID = ObjectCreator.CreateTaxRate("EXEMPT1", "Exempt", AccTaxRate.Types.Exempt, 0, ZString.Empty, 0, 1);
				}
				return fExemptTaxID;
			}
		}
		AccTaxRate fExemptTaxID;

		AccTaxRate NotReportableTaxID
		{
			get
			{
				if (fNotReportableTaxID == null)
				{
					fNotReportableTaxID = ObjectCreator.CreateTaxRate("NOTRPRT1", "Not Reportable", AccTaxRate.Types.NotReportable, 0, ZString.Empty, 0, 1);
				}
				return fNotReportableTaxID;
			}
		}
		AccTaxRate fNotReportableTaxID;

		AccTaxRate ReverseChargeStandardTaxID
		{
			get
			{
				if (fReverseChargeStandardTaxID == null)
				{
					fReverseChargeStandardTaxID = ObjectCreator.CreateTaxRate("REVVAT1", "10% Reverse Charge", AccTaxRate.Types.ReverseRated, 10, ZString.Empty, 0, 1);
				}
				return fReverseChargeStandardTaxID;
			}
		}
		AccTaxRate fReverseChargeStandardTaxID;

		AccTaxRate ExcludeChargeTaxID
		{
			get
			{
				if (fExcludeChargeTaxID == null)
				{
					fExcludeChargeTaxID = ObjectCreator.CreateTaxRate("EXLVAT1", "Exclude Charge", AccTaxRate.Types.ExcludedFromTheTaxBase, 0, ZString.Empty, 0, 1);
				}
				return fExcludeChargeTaxID;
			}
		}
		AccTaxRate fExcludeChargeTaxID;

		AccTaxRate ReverseChargeZeroRatedTaxID
		{
			get
			{
				if (fReverseChargeZeroRatedTaxID == null)
				{
					fReverseChargeZeroRatedTaxID = ObjectCreator.CreateTaxRate("REVZERO1", "Zero Rated Reverse Charge", AccTaxRate.Types.ReverseRated, 0, ZString.Empty, 0, 1);
				}
				return fReverseChargeZeroRatedTaxID;
			}
		}
		AccTaxRate fReverseChargeZeroRatedTaxID;

		static ComplianceSubTypeAttributionRuleConfigurationCollection GetComplianceSubTypeAttribRuleConfigCollection(ZString country, ZString ledgerType, ZString subType)
		{
			var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
			var item = collection.AddNew();
			item.Country = country;
			item.SubType = subType;
			item.LedgerType = ledgerType;
			item.InvoiceType = "INV";
			item.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID; // "TID";
			item.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly; // "NDB";
			item.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly; //"OTO";
			item.OrganisationLocation = "";
			return collection;
		}

		void SetupInvoiceSequenceForSigning()
		{
			AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);
			ZGuid menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;
			AccComplianceSequence sequence = ObjectCreator.SetupComplianceSequence(menuPK, PortugalComplianceInfo.ComplianceSubTypeCodes.TXI, "abc", 1, 100, 1);
			Factory.Save();

			for (int i = 1; i <= 2; i++)
			{
				var invoice = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID, NotReportableTaxID, StandardRateTaxID }, false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZString.Empty, false);
				invoice.AH_InvoiceDate = new ZDateTime(2018, 09, 11);
				invoice.AH_SystemCreateTimeUtc = new ZDateTime(2018, 09, 11, 05, 30, 59);
				Factory.Save();
			}

			invoice3 = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID, NotReportableTaxID, StandardRateTaxID }, false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZString.Empty, false);
			invoice3.AH_InvoiceDate = new ZDateTime(2018, 09, 11);
			invoice3.AH_SystemCreateTimeUtc = new ZDateTime(2018, 09, 11, 05, 30, 59);
			Factory.Save();

			invoice4 = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID, NotReportableTaxID, StandardRateTaxID }, false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZString.Empty, false);
			invoice4.AH_InvoiceDate = new ZDateTime(2018, 09, 12);
			invoice4.AH_SystemCreateTimeUtc = new ZDateTime(2018, 09, 12, 11, 36, 45);
			Factory.Save();

			invoice5 = CreateInvoiceForComplianceSubTypeTest(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, new AccTaxRate[] { ExemptTaxID, NotReportableTaxID, StandardRateTaxID }, false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZString.Empty, false);
			invoice5.AH_InvoiceDate = new ZDateTime(2018, 09, 13);
			invoice5.AH_SystemCreateTimeUtc = new ZDateTime(2018, 09, 13, 8, 9, 23);
			Factory.Save();

			AssertEquals("TXI abc/000000003", invoice3.AH_TransactionReference);
			AssertEquals("TXI abc/000000004", invoice4.AH_TransactionReference);
			AssertEquals("TXI abc/000000005", invoice5.AH_TransactionReference);

			invoice3.AH_DigitalSignature_COMPRESSED = null;
			invoice4.AH_DigitalSignature_COMPRESSED = null;
			invoice5.AH_DigitalSignature_COMPRESSED = null;
			Factory.Save();

			Assert(invoice3.AH_DigitalSignature_COMPRESSED.IsEmpty);
			Assert(invoice4.AH_DigitalSignature_COMPRESSED.IsEmpty);
			Assert(invoice5.AH_DigitalSignature_COMPRESSED.IsEmpty);
		}

		InvoicingBase invoice3;
		InvoicingBase invoice4;
		InvoicingBase invoice5;

		#endregion
	}
}
