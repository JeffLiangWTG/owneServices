using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public partial class InvoicingBaseTest
	{
		#region TestCodeMappingRulesUpdateOnSaving

		public void TestCodeMappingRulesUpdateOnSaving_WithOrgCodeMatchingRule()
		{
			AssertCodeMappingRulesUpdateOnSaving_WithOrgCodeMatchingRule(false);
		}

		public void TestCodeMappingRulesUpdateOnSaving_WithOrgCodeMatchingRule_SaveAsIncomplete()
		{
			AssertCodeMappingRulesUpdateOnSaving_WithOrgCodeMatchingRule(true);
		}

		void AssertCodeMappingRulesUpdateOnSaving_WithOrgCodeMatchingRule(bool saveAsIncomplete)
		{
			using (AccountingConfigurationRegistry.Instance.EnableAutoAccrualMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				if (GetExpectedBusinessObjectType() != typeof(APInvoice) && GetExpectedBusinessObjectType() != typeof(APCreditNote))
				{
					Assert(true);
					return;
				}
				var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
				orgAddress.AddressType = nameof(DocAddressType.None);
				orgAddress.Address1 = "Street";
				orgAddress.OrganizationCode = "ORG";

				var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
				universalTransaction.Ledger = LedgerTypes.AccountsPayable;
				universalTransaction.OrganizationAddress = orgAddress;
				universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
				var universalLine1 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { ChargeCode = new ChargeCode { Code = "FF1" } };
				universalTransaction.PostingJournalCollection.Add(universalLine1);
				var universalLine2 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { ChargeCode = new ChargeCode { Code = "FF2" } };
				universalTransaction.PostingJournalCollection.Add(universalLine2);
				var universalLine3 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { ChargeCode = new ChargeCode { Code = "FF3" } };
				universalTransaction.PostingJournalCollection.Add(universalLine3);
				var universalLine4 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { ChargeCode = universalLine1.ChargeCode };
				universalTransaction.PostingJournalCollection.Add(universalLine4);
				var universalLine5 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { ChargeCode = universalLine2.ChargeCode };
				universalTransaction.PostingJournalCollection.Add(universalLine5);

				var matchingRuleFactory = new BusinessObjectFactory();
				var orgProxyInMatchingRuleFactory = matchingRuleFactory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
				var creditorPatternMatchOverride = TestObjectCreator.AddMatchingRuleForOrganization(orgProxyInMatchingRuleFactory, orgAddress.OrganizationCode.Value, TestObjectCreator.Creditor2);

				var chargePatternMatchOverride1 = TestObjectCreator.AddMatchingRuleForChargeCode(orgProxyInMatchingRuleFactory, universalLine1.ChargeCode.Code.Value, TestObjectCreator.CC1);
				var chargePatternMatchOverride2 = TestObjectCreator.AddMatchingRuleForChargeCode(orgProxyInMatchingRuleFactory, universalLine2.ChargeCode.Code.Value, TestObjectCreator.CC2);

				matchingRuleFactory.Save();

				TestObjectCreator.Job1.Factory.Save();

				int countOfRulesBefore = Factory.GetDatabaseCount(typeof(OrgPatternMatchOverride));

				InvoicingBase invoice = TestObjectCreator.CreateAndAllocateInvoiceWithUniversalTransactionInAllocationApprovalRequest(GetExpectedBusinessObjectType(), universalTransaction);

				AssertEquals("Precondition: invoice creditor", TestObjectCreator.Creditor1.OH_Code, invoice.Header.OH_Code);
				AssertEquals("Precondition: ImportedCreditorXmlCode", orgAddress.OrganizationCode, invoice.ImportedCreditorXmlCode);
				AssertEquals("Precondition: ImportedCreditor", TestObjectCreator.Creditor2.OH_Code, invoice.ImportedCreditor);
				AssertHasWarning(invoice.AH_OHInfo, "Creditor value is different to a value matched by default. Organization matching rule for foreign code 'ORG' will be updated.");
				AssertEquals("Precondition: invoice lines count", 5, invoice.Lines.Count);
				var line = invoice.Lines[0];
				line.AL_OSExTaxAmount = 20;
				line.AL_JH = TestObjectCreator.Job1.PK;
				AssertEquals("Precondition: line charge code", TestObjectCreator.CC1.AC_Code, line.GenericChargeBizO.VC_Code);
				AssertEquals("Precondition: ImportedChargeCodeXmlCode", universalLine1.ChargeCode.Code.Value, line.ImportedChargeCodeXmlCode);
				AssertEquals("Precondition: ImportedChargeCode", TestObjectCreator.CC1.AC_Code, line.ImportedChargeCode);
				line.GenericCharge = TestObjectCreator.CC2.PK;
				AssertHasWarning(line.GenericChargeInfo, "Charge code value is different to a value matched by default. However it also different to values in lines with the same charge code value in the imported XML. Charge code matching rule for foreign code 'FF1' won’t be updated.");
				line = invoice.Lines[1];
				line.AL_OSExTaxAmount = 20;
				line.AL_JH = TestObjectCreator.Job1.PK;
				AssertEquals("Precondition: line charge code", TestObjectCreator.CC2.AC_Code, line.GenericChargeBizO.VC_Code);
				AssertEquals("Precondition: ImportedChargeCodeXmlCode", universalLine2.ChargeCode.Code.Value, line.ImportedChargeCodeXmlCode);
				AssertEquals("Precondition: ImportedChargeCode", TestObjectCreator.CC2.AC_Code, line.ImportedChargeCode);
				line.GenericCharge = TestObjectCreator.CC1.PK;
				AssertHasWarning(line.GenericChargeInfo, "Charge code value is different to a value matched by default. However it also different to values in lines with the same charge code value in the imported XML. Charge code matching rule for foreign code 'FF2' won’t be updated.");
				line = invoice.Lines[2];
				line.AL_OSExTaxAmount = 20;
				line.AL_JH = TestObjectCreator.Job1.PK;
				AssertEquals("Precondition: line charge code", ZGuid.Empty, line.GenericCharge);
				AssertEquals("Precondition: ImportedChargeCodeXmlCode", universalLine3.ChargeCode.Code.Value, line.ImportedChargeCodeXmlCode);
				AssertEquals("Precondition: ImportedChargeCode", ZString.Empty, line.ImportedChargeCode);
				line.GenericCharge = TestObjectCreator.CC3.PK;
				AssertHasWarning(line.GenericChargeInfo, "Charge code is not found for imported XML code 'FF3'. New matching rule will be created for foreign code 'FF3'.");
				line = invoice.Lines[3];
				line.AL_OSExTaxAmount = 20;
				line.AL_JH = TestObjectCreator.Job1.PK;
				AssertEquals("Precondition: line charge code", TestObjectCreator.CC1.AC_Code, line.GenericChargeBizO.VC_Code);
				AssertEquals("Precondition: ImportedChargeCodeXmlCode", universalLine1.ChargeCode.Code.Value, line.ImportedChargeCodeXmlCode);
				AssertEquals("Precondition: ImportedChargeCode", TestObjectCreator.CC1.AC_Code, line.ImportedChargeCode);
				AssertNoWarnings(line.GenericChargeInfo);
				line = invoice.Lines[4];
				line.AL_OSExTaxAmount = 20;
				line.AL_JH = TestObjectCreator.Job1.PK;
				AssertEquals("Precondition: line charge code", TestObjectCreator.CC2.AC_Code, line.GenericChargeBizO.VC_Code);
				AssertEquals("Precondition: ImportedChargeCodeXmlCode", universalLine2.ChargeCode.Code.Value, line.ImportedChargeCodeXmlCode);
				AssertEquals("Precondition: ImportedChargeCode", TestObjectCreator.CC2.AC_Code, line.ImportedChargeCode);
				line.GenericCharge = TestObjectCreator.CC1.PK;
				AssertHasWarning(line.GenericChargeInfo, "Charge code value is different to a value matched by default. Charge code matching rule for foreign code 'FF2' will be updated.");

				var filter = new ZQuery(OrgPatternMatchOverrideSchema.OO_Relationship, chargePatternMatchOverride2.OO_Relationship);
				filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_ForeignCode, (ZString)universalLine3.ChargeCode.Code.Value);
				filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_OH, orgProxyInMatchingRuleFactory.PK);
				var loadedMatchingRules = orgProxyInMatchingRuleFactory.Factory.Load<OrgPatternMatchOverride>(filter);
				AssertEquals("Precondition: loadedMatchingRules.Length", 0, loadedMatchingRules.Length);

				if (saveAsIncomplete)
				{
					invoice.SaveAsIncomplete();
					invoice = new BusinessObjectFactory().Load<InvoicingBase>(invoice.PK);
					invoice.SubmittedFromInvoicingForm = true;
					invoice.RestoreSavedData();
					invoice.MoveFromIncompleteToPayableLedger();
				}
				invoice.Factory.Save();
				int countOfRulesAfter = Factory.GetDatabaseCount(typeof(OrgPatternMatchOverride));
				AssertEquals("Amount of new matching rules created.", 1, countOfRulesAfter - countOfRulesBefore);

				AssertEquals("Organization matching rule is updated", TestObjectCreator.Creditor1.PK, creditorPatternMatchOverride.OO_LocalGuid);
				AssertEquals("Charge Code FF1 matching rule is not updated", TestObjectCreator.CC1.AC_Code, chargePatternMatchOverride1.OO_LocalCode);
				AssertEquals("Charge Code FF2 matching rule is updated", TestObjectCreator.CC1.AC_Code, chargePatternMatchOverride2.OO_LocalCode);
				loadedMatchingRules = orgProxyInMatchingRuleFactory.Factory.Load<OrgPatternMatchOverride>(filter);
				AssertEquals("loadedMatchingRules.Length", 1, loadedMatchingRules.Length);
				var chargePatternMatchOverride3 = loadedMatchingRules[0];
				AssertEquals("Charge Code FF3 matching rule is created", TestObjectCreator.CC3.AC_Code, chargePatternMatchOverride3.OO_LocalCode);
			}
		}

		public void TestCodeMappingRulesUpdateOnSaving_WithOrgCodeMatchingRule_CrossLedger()
		{
			using (AccountingConfigurationRegistry.Instance.EnableAutoAccrualMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				if (GetExpectedBusinessObjectType() != typeof(APInvoice) && GetExpectedBusinessObjectType() != typeof(APCreditNote))
				{
					Assert(true);
					return;
				}
				var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = nameof(DocAddressType.None),
					Address1 = "Street"
				};

				var universalLine1 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { ChargeCode = new ChargeCode { Code = "FF1" } };
				var universalLine2 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { ChargeCode = new ChargeCode { Code = "FF2" } };
				var universalLine3 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { ChargeCode = new ChargeCode { Code = "FF3" } };
				var universalLine4 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { ChargeCode = universalLine1.ChargeCode };
				var universalLine5 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { ChargeCode = universalLine2.ChargeCode };
				var universalLine6 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { ChargeCode = new ChargeCode { Code = "FF4" } };
				var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = DataContextFactory.New(),
					Ledger = LedgerTypes.AccountsPayable,
					OrganizationAddress = orgAddress,
					BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), OrganizationCode = TestObjectCreator.NonCurrentCompany.OrgProxy.OH_Code, Country = Country.New(TestObjectCreator.NonCurrentCompany.Country) }
				};
				universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>
					{
						universalLine1,
						universalLine2,
						universalLine3,
						universalLine4,
						universalLine5,
						universalLine6,
					});
				universalTransaction.DataContext.SetCompanyAndDataProviderDetails(TestObjectCreator.NonCurrentCompany);

				var matchingRuleFactory = new BusinessObjectFactory();
				var orgProxyInMatchingRuleFactory = matchingRuleFactory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
				var creditorPatternMatchOverride = TestObjectCreator.AddMatchingRuleForOrganization(orgProxyInMatchingRuleFactory, universalTransaction.DataContext.DataProviderForCodeMapping, TestObjectCreator.Creditor2);

				var dataProviderOrgInMatchingRuleFactory = matchingRuleFactory.Load<OrgHeader>(TestObjectCreator.Creditor2.PK);
				var chargePatternMatchOverride1 = TestObjectCreator.AddMatchingRuleForChargeCode(dataProviderOrgInMatchingRuleFactory, universalLine1.ChargeCode.Code.Value, TestObjectCreator.CC1);
				var chargePatternMatchOverride2 = TestObjectCreator.AddMatchingRuleForChargeCode(dataProviderOrgInMatchingRuleFactory, universalLine2.ChargeCode.Code.Value, TestObjectCreator.CC2);
				var chargePatternMatchOverride3 = TestObjectCreator.AddMatchingRuleForChargeCode(dataProviderOrgInMatchingRuleFactory, universalLine6.ChargeCode.Code.Value, TestObjectCreator.CC3);

				//Next rules should not be used here as data provider organization is not OrgPoxy after patternMatchOverride1 rule is created
				var chargePatternMatchOverride_ShouldNotBeUsed1 = TestObjectCreator.AddMatchingRuleForChargeCode(orgProxyInMatchingRuleFactory, universalLine1.ChargeCode.Code.Value, TestObjectCreator.CC5);
				var chargePatternMatchOverride_ShouldNotBeUsed2 = TestObjectCreator.AddMatchingRuleForChargeCode(orgProxyInMatchingRuleFactory, universalLine2.ChargeCode.Code.Value, TestObjectCreator.CC5);
				var chargePatternMatchOverride_ShouldNotBeUsed3 = TestObjectCreator.AddMatchingRuleForChargeCode(orgProxyInMatchingRuleFactory, universalLine3.ChargeCode.Code.Value, TestObjectCreator.CC5);

				//Next rules will be used as invoice creditor is changed from Creditor2 to Creditor1 
				var changedDataProviderOrgInMatchingRuleFactory = matchingRuleFactory.Load<OrgHeader>(TestObjectCreator.Creditor1.PK);
				var chargePatternMatchOverride_UsedAsCreditorChanged1 = TestObjectCreator.AddMatchingRuleForChargeCode(changedDataProviderOrgInMatchingRuleFactory, universalLine1.ChargeCode.Code.Value, TestObjectCreator.CC1);
				var chargePatternMatchOverride_UsedAsCreditorChanged2 = TestObjectCreator.AddMatchingRuleForChargeCode(changedDataProviderOrgInMatchingRuleFactory, universalLine2.ChargeCode.Code.Value, TestObjectCreator.CC2);
				var chargePatternMatchOverride_UsedAsCreditorChanged3 = TestObjectCreator.AddMatchingRuleForChargeCode(changedDataProviderOrgInMatchingRuleFactory, universalLine6.ChargeCode.Code.Value, TestObjectCreator.CC2);

				matchingRuleFactory.Save();

				TestObjectCreator.Job1.Factory.Save();

				int countOfRulesBefore = Factory.GetDatabaseCount(typeof(OrgPatternMatchOverride));

				InvoicingBase invoice = TestObjectCreator.CreateAndAllocateInvoiceWithUniversalTransactionInAllocationApprovalRequest(GetExpectedBusinessObjectType(), universalTransaction, true);
				AssertEquals("Precondition: invoice creditor", TestObjectCreator.Creditor1.OH_Code, invoice.Header.OH_Code);
				AssertEquals("Precondition: ImportedCreditorXmlCode", universalTransaction.DataContext.DataProviderForCodeMapping, invoice.ImportedCreditorXmlCode);
				AssertEquals("Precondition: ImportedCreditor", TestObjectCreator.Creditor2.OH_Code, invoice.ImportedCreditor);
				AssertHasWarning(invoice.AH_OHInfo, "Creditor value is different to a value matched by default. Organization matching rule for foreign code 'EDIDATDEM' will be updated.");
				AssertEquals("Precondition: invoice lines count", 6, invoice.Lines.Count);
				var line = invoice.Lines[0];
				line.AL_OSExTaxAmount = 20;
				line.AL_JH = TestObjectCreator.Job1.PK;
				AssertEquals("Precondition: line charge code", TestObjectCreator.CC1.AC_Code, line.GenericChargeBizO.VC_Code);
				AssertEquals("Precondition: ImportedChargeCodeXmlCode", universalLine1.ChargeCode.Code.Value, line.ImportedChargeCodeXmlCode);
				AssertEquals("Precondition: ImportedChargeCode", TestObjectCreator.CC1.AC_Code, line.ImportedChargeCode);
				line.GenericCharge = TestObjectCreator.CC2.PK;
				AssertHasWarning(line.GenericChargeInfo, "Charge code value is different to a value matched by default. However it also different to values in lines with the same charge code value in the imported XML. Charge code matching rule for foreign code 'FF1' won’t be updated.");
				line = invoice.Lines[1];
				line.AL_OSExTaxAmount = 20;
				line.AL_JH = TestObjectCreator.Job1.PK;
				AssertEquals("Precondition: line charge code", TestObjectCreator.CC2.AC_Code, line.GenericChargeBizO.VC_Code);
				AssertEquals("Precondition: ImportedChargeCodeXmlCode", universalLine2.ChargeCode.Code.Value, line.ImportedChargeCodeXmlCode);
				AssertEquals("Precondition: ImportedChargeCode", TestObjectCreator.CC2.AC_Code, line.ImportedChargeCode);
				line.GenericCharge = TestObjectCreator.CC1.PK;
				AssertHasWarning(line.GenericChargeInfo, "Charge code value is different to a value matched by default. However it also different to values in lines with the same charge code value in the imported XML. Charge code matching rule for foreign code 'FF2' won’t be updated.");
				line = invoice.Lines[2];
				line.AL_OSExTaxAmount = 20;
				line.AL_JH = TestObjectCreator.Job1.PK;
				AssertEquals("Precondition: line charge code", ZGuid.Empty, line.GenericCharge);
				AssertEquals("Precondition: ImportedChargeCodeXmlCode", universalLine3.ChargeCode.Code.Value, line.ImportedChargeCodeXmlCode);
				AssertEquals("Precondition: ImportedChargeCode", ZString.Empty, line.ImportedChargeCode);
				line.GenericCharge = TestObjectCreator.CC3.PK;
				AssertHasWarning(line.GenericChargeInfo, "Charge code is not found for imported XML code 'FF3'. New matching rule will be created for foreign code 'FF3'.");
				line = invoice.Lines[3];
				line.AL_OSExTaxAmount = 20;
				line.AL_JH = TestObjectCreator.Job1.PK;
				AssertEquals("Precondition: line charge code", TestObjectCreator.CC1.AC_Code, line.GenericChargeBizO.VC_Code);
				AssertEquals("Precondition: ImportedChargeCodeXmlCode", universalLine1.ChargeCode.Code.Value, line.ImportedChargeCodeXmlCode);
				AssertEquals("Precondition: ImportedChargeCode", TestObjectCreator.CC1.AC_Code, line.ImportedChargeCode);
				AssertNoWarnings(line.GenericChargeInfo);
				line = invoice.Lines[4];
				line.AL_OSExTaxAmount = 20;
				line.AL_JH = TestObjectCreator.Job1.PK;
				AssertEquals("Precondition: line charge code", TestObjectCreator.CC2.AC_Code, line.GenericChargeBizO.VC_Code);
				AssertEquals("Precondition: ImportedChargeCodeXmlCode", universalLine2.ChargeCode.Code.Value, line.ImportedChargeCodeXmlCode);
				AssertEquals("Precondition: ImportedChargeCode", TestObjectCreator.CC2.AC_Code, line.ImportedChargeCode);
				line.GenericCharge = TestObjectCreator.CC1.PK;
				AssertHasWarning(line.GenericChargeInfo, "Charge code value is different to a value matched by default. Charge code matching rule for foreign code 'FF2' will be updated.");
				line = invoice.Lines[5];
				line.AL_OSExTaxAmount = 20;
				line.AL_JH = TestObjectCreator.Job1.PK;
				AssertEquals("Precondition: line charge code", TestObjectCreator.CC2.AC_Code, line.GenericChargeBizO.VC_Code);
				AssertEquals("Precondition: ImportedChargeCodeXmlCode", universalLine6.ChargeCode.Code.Value, line.ImportedChargeCodeXmlCode);
				AssertEquals("Precondition: ImportedChargeCode", TestObjectCreator.CC2.AC_Code, line.ImportedChargeCode);
				AssertNoWarnings(line.GenericChargeInfo);

				var filter = new ZQuery(OrgPatternMatchOverrideSchema.OO_Relationship, chargePatternMatchOverride2.OO_Relationship);
				filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_ForeignCode, (ZString)universalLine3.ChargeCode.Code.Value);
				filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_OH, changedDataProviderOrgInMatchingRuleFactory.PK);
				var loadedMatchingRules = changedDataProviderOrgInMatchingRuleFactory.Factory.Load<OrgPatternMatchOverride>(filter);
				AssertEquals("Precondition: loadedMatchingRules.Length", 0, loadedMatchingRules.Length);

				invoice.Factory.Save();
				int countOfRulesAfter = Factory.GetDatabaseCount(typeof(OrgPatternMatchOverride));
				AssertEquals("Amount of new matching rules created.", 1, countOfRulesAfter - countOfRulesBefore);

				AssertEquals("Organization matching rule is updated", TestObjectCreator.Creditor1.PK, creditorPatternMatchOverride.OO_LocalGuid);
				AssertEquals("Charge Code FF1 matching rule for creditor 2 is not updated", TestObjectCreator.CC1.AC_Code, chargePatternMatchOverride1.OO_LocalCode);
				AssertEquals("Charge Code FF2 matching rule for creditor 2 is not updated", TestObjectCreator.CC2.AC_Code, chargePatternMatchOverride2.OO_LocalCode);
				AssertEquals("Charge Code FF4 matching rule for creditor 2 is not updated", TestObjectCreator.CC3.AC_Code, chargePatternMatchOverride3.OO_LocalCode);
				AssertEquals("Charge Code FF1 matching rule for org proxy is not updated", TestObjectCreator.CC5.AC_Code, chargePatternMatchOverride_ShouldNotBeUsed1.OO_LocalCode);
				AssertEquals("Charge Code FF2 matching rule for org proxy is not updated", TestObjectCreator.CC5.AC_Code, chargePatternMatchOverride_ShouldNotBeUsed2.OO_LocalCode);
				AssertEquals("Charge Code FF3 matching rule for org proxy is not updated", TestObjectCreator.CC5.AC_Code, chargePatternMatchOverride_ShouldNotBeUsed3.OO_LocalCode);
				AssertEquals("Charge Code FF1 matching rule is not updated", TestObjectCreator.CC1.AC_Code, chargePatternMatchOverride_UsedAsCreditorChanged1.OO_LocalCode);
				AssertEquals("Charge Code FF2 matching rule is updated", TestObjectCreator.CC1.AC_Code, chargePatternMatchOverride_UsedAsCreditorChanged2.OO_LocalCode);
				AssertEquals("Charge Code FF4 matching rule is not updated", TestObjectCreator.CC2.AC_Code, chargePatternMatchOverride_UsedAsCreditorChanged3.OO_LocalCode);
				loadedMatchingRules = changedDataProviderOrgInMatchingRuleFactory.Factory.Load<OrgPatternMatchOverride>(filter);
				AssertEquals("loadedMatchingRules.Length", 1, loadedMatchingRules.Length);
				var chargePatternMatchOverride4 = loadedMatchingRules[0];
				AssertEquals("Charge Code FF3 matching rule is created", TestObjectCreator.CC3.AC_Code, chargePatternMatchOverride3.OO_LocalCode);
			}
		}

		public void TestCodeMappingRulesUpdateOnSaving_WithoutOrgCodeMatchingRule_WhenRegistrySetupYes()
		{
			AssertCodeMappingRulesUpdateOnSaving_WithoutOrgCodeMatchingRule(true);
		}

		public void TestCodeMappingRulesUpdateOnSaving_WithoutOrgCodeMatchingRule_WhenRegistrySetupNo()
		{
			AssertCodeMappingRulesUpdateOnSaving_WithoutOrgCodeMatchingRule(false);
		}

		public void AssertCodeMappingRulesUpdateOnSaving_WithoutOrgCodeMatchingRule(bool enableAutomaticOrganizationCodeMappingRule)
		{
			if (GetExpectedBusinessObjectType() != typeof(APInvoice) && GetExpectedBusinessObjectType() != typeof(APCreditNote))
			{
				Assert(true);
				return;
			}
			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			orgAddress.AddressType = nameof(DocAddressType.None);
			orgAddress.Address1 = "Street";
			var expectedOrgCode = "ORG";
			orgAddress.OrganizationCode = expectedOrgCode;

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.DataContext = DataContextFactory.New();
			universalTransaction.DataContext.SetCompanyAndDataProviderDetails(TestObjectCreator.NonCurrentCompany);
			universalTransaction.Ledger = LedgerTypes.AccountsPayable;
			universalTransaction.OrganizationAddress = orgAddress;
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());

			int countOfRulesBefore = Factory.GetDatabaseCount(typeof(OrgPatternMatchOverride));

			InvoicingBase invoice = TestObjectCreator.CreateAndAllocateInvoiceWithUniversalTransactionInAllocationApprovalRequest(GetExpectedBusinessObjectType(), universalTransaction);
			AssertEquals("Precondition: invoice creditor", TestObjectCreator.Creditor1.OH_Code, invoice.Header.OH_Code);
			AssertEquals("Precondition: ImportedCreditorXmlCode", expectedOrgCode, invoice.ImportedCreditorXmlCode);
			AssertEquals("Precondition: ImportedCreditor", ZString.Empty, invoice.ImportedCreditor);
			AssertEquals("Precondition: invoice lines count", 0, invoice.Lines.Count);

			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.GenericCharge = TestObjectCreator.GLHeader1.PK;
			line.AL_OSExTaxAmount = 20;

			var matchingRuleFactory = new BusinessObjectFactory();
			var orgProxyInMatchingRuleFactory = matchingRuleFactory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			var filter = new ZQuery(OrgPatternMatchOverrideSchema.OO_Relationship, Constants.OrgPatternMatchOverrideRelationships.Organisation);
			filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_ForeignCode, expectedOrgCode);
			filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_OH, orgProxyInMatchingRuleFactory.PK);
			var loadedMatchingRules = orgProxyInMatchingRuleFactory.Factory.Load<OrgPatternMatchOverride>(filter);
			AssertEquals("loadedMatchingRules.Length before invoice saving.", 0, loadedMatchingRules.Length);

			if (enableAutomaticOrganizationCodeMappingRule)
			{
				AccountingConfigurationRegistry.Instance.EnableAutomaticOrganizationCodeMappingForUnallocatedInvoices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				AssertHasWarning(invoice.AH_OHInfo, $"Organization is not found for imported XML code '{expectedOrgCode}'. New matching rule will be created for foreign code '{expectedOrgCode}'.");
				AssertEquals("Organisation Code Matching Rule Enabled", true, invoice.ShouldCreateOrganisationCodeMatchingRule);
				invoice.Factory.Save();
				int countOfRulesAfterWithSetupYes = Factory.GetDatabaseCount(typeof(OrgPatternMatchOverride));
				AssertEquals("Amount of new matching rules created should be 1", 1, countOfRulesAfterWithSetupYes - countOfRulesBefore);
				loadedMatchingRules = orgProxyInMatchingRuleFactory.Factory.Load<OrgPatternMatchOverride>(filter);
				AssertEquals("loadedMatchingRules.Length after invoice saving", 1, loadedMatchingRules.Length);
				var patternMatchOverride = loadedMatchingRules[0];
				AssertEquals("Organization matching rule is created", TestObjectCreator.Creditor1.PK, patternMatchOverride.OO_LocalGuid);
			}
			else
			{
				AccountingConfigurationRegistry.Instance.EnableAutomaticOrganizationCodeMappingForUnallocatedInvoices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				AssertEquals("Organisation Code Matching Rule Disabled", false, invoice.ShouldCreateOrganisationCodeMatchingRule);
				invoice.Factory.Save();
				int countOfRulesAfterWithSetupNo = Factory.GetDatabaseCount(typeof(OrgPatternMatchOverride));
				AssertEquals("Amount of new matching rules created should be 0", 0, countOfRulesAfterWithSetupNo - countOfRulesBefore);
				loadedMatchingRules = orgProxyInMatchingRuleFactory.Factory.Load<OrgPatternMatchOverride>(filter);
				AssertEquals("loadedMatchingRules.Length after invoice saving", 0, loadedMatchingRules.Length);
			}
		}

		public void TestCodeMappingRulesUpdateOnSaving_WithoutOrgCodeMatchingRule_CrossLedger()
		{
			if (GetExpectedBusinessObjectType() != typeof(APInvoice) && GetExpectedBusinessObjectType() != typeof(APCreditNote))
			{
				Assert(true);
				return;
			}
			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.None),
				Address1 = "Street"
			};

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
				Ledger = LedgerTypes.AccountsPayable,
				OrganizationAddress = orgAddress,
				BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), OrganizationCode = TestObjectCreator.NonCurrentCompany.OrgProxy.OH_Code, Country = Country.New(TestObjectCreator.NonCurrentCompany.Country) }
			};
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.DataContext.SetCompanyAndDataProviderDetails(TestObjectCreator.NonCurrentCompany);

			int countOfRulesBefore = Factory.GetDatabaseCount(typeof(OrgPatternMatchOverride));

			InvoicingBase invoice = TestObjectCreator.CreateAndAllocateInvoiceWithUniversalTransactionInAllocationApprovalRequest(GetExpectedBusinessObjectType(), universalTransaction, true);
			AssertEquals("Precondition: invoice creditor", TestObjectCreator.Creditor1.OH_Code, invoice.Header.OH_Code);
			AssertEquals("Precondition: ImportedCreditorXmlCode", universalTransaction.DataContext.DataProviderForCodeMapping, invoice.ImportedCreditorXmlCode);
			AssertEquals("Precondition: ImportedCreditor", ZString.Empty, invoice.ImportedCreditor);
			AssertHasWarning(invoice.AH_OHInfo, $"Organization is not found for imported XML code '{universalTransaction.DataContext.DataProviderForCodeMapping}'. New matching rule will be created for foreign code '{universalTransaction.DataContext.DataProviderForCodeMapping}'.");
			AssertEquals("Precondition: invoice lines count", 0, invoice.Lines.Count);
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.GenericCharge = TestObjectCreator.GLHeader1.PK;
			line.AL_OSExTaxAmount = 20;

			var matchingRuleFactory = new BusinessObjectFactory();
			var orgProxyInMatchingRuleFactory = matchingRuleFactory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			var filter = new ZQuery(OrgPatternMatchOverrideSchema.OO_Relationship, Constants.OrgPatternMatchOverrideRelationships.Organisation);
			filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_ForeignCode, universalTransaction.DataContext.DataProviderForCodeMapping);
			filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_OH, orgProxyInMatchingRuleFactory.PK);
			var loadedMatchingRules = orgProxyInMatchingRuleFactory.Factory.Load<OrgPatternMatchOverride>(filter);
			AssertEquals("loadedMatchingRules.Length before invoice saving.", 0, loadedMatchingRules.Length);

			invoice.Factory.Save();
			int countOfRulesAfter = Factory.GetDatabaseCount(typeof(OrgPatternMatchOverride));
			AssertEquals("Amount of new matching rules created.", 1, countOfRulesAfter - countOfRulesBefore);

			loadedMatchingRules = orgProxyInMatchingRuleFactory.Factory.Load<OrgPatternMatchOverride>(filter);
			AssertEquals("loadedMatchingRules.Length after invoice saving.", 1, loadedMatchingRules.Length);
			var patternMatchOverride = loadedMatchingRules[0];
			AssertEquals("Organization matching rule is created", TestObjectCreator.Creditor1.PK, patternMatchOverride.OO_LocalGuid);
		}

		#endregion

		#region TestCodeRemappingOnChaningCreditorForCrossLedgerImportFromXML

		public void TestCodeRemappingOnChaningCreditorForCrossLedgerImportFromXML_WithoutDataProviderCodeMapping()
		{
			AssertCodeRemappingOnChaningCreditorForCrossLedgerImportFromXML();
		}

		public void TestCodeRemappingOnChaningCreditorForCrossLedgerImportFromXML_WithDataProviderCodeMapping()
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(TestObjectCreator.NonCurrentCompany);

			var matchingRuleFactory = new BusinessObjectFactory();
			OrgPatternMatchOverride patternMatchOverride = TestObjectCreator.AddMatchingRuleForOrganization(matchingRuleFactory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy),
				dataContext.DataProviderForCodeMapping, TestObjectCreator.Creditor2);
			matchingRuleFactory.Save();

			AssertCodeRemappingOnChaningCreditorForCrossLedgerImportFromXML(TestObjectCreator.Creditor2);

			AssertEquals("DataProviderForCodeMapping matching rule was not updated after changes  that user reverted back.", TestObjectCreator.Creditor2.PK, patternMatchOverride.OO_LocalGuid);
		}

		void AssertCodeRemappingOnChaningCreditorForCrossLedgerImportFromXML(OrgHeader importedCreditor = null)
		{
			using (AccountingConfigurationRegistry.Instance.EnableAutoAccrualMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				if (GetExpectedBusinessObjectType() != typeof(APInvoice) && GetExpectedBusinessObjectType() != typeof(APCreditNote))
				{
					Assert(true);
					return;
				}

				bool isDataProviderOrgMatchingRuleExist = true;
				if (importedCreditor == null)
				{
					isDataProviderOrgMatchingRuleExist = false;
					importedCreditor = TestObjectCreator.NonCurrentCompany.OrgProxy;
				}
				var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = nameof(DocAddressType.None),
					Address1 = "Street"
				};

				var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
				{
					ChargeCode = new ChargeCode { Code = "FFF" }
				};

				var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = DataContextFactory.New(),
					Ledger = LedgerTypes.AccountsPayable,
					OrganizationAddress = orgAddress,
					BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), OrganizationCode = TestObjectCreator.NonCurrentCompany.OrgProxy.OH_Code, Country = Country.New(TestObjectCreator.NonCurrentCompany.Country) }
				};
				universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal> { universalLine });
				universalTransaction.DataContext.SetCompanyAndDataProviderDetails(TestObjectCreator.NonCurrentCompany);

				var matchingRuleFactory = new BusinessObjectFactory();
				var patternMatchOverride1 = TestObjectCreator.AddMatchingRuleForChargeCode(matchingRuleFactory.Load<OrgHeader>(importedCreditor.PK), universalLine.ChargeCode.Code.Value, TestObjectCreator.CC1);
				var patternMatchOverride2 = TestObjectCreator.AddMatchingRuleForChargeCode(matchingRuleFactory.Load<OrgHeader>(TestObjectCreator.Creditor1.PK), universalLine.ChargeCode.Code.Value, TestObjectCreator.CC2);
				matchingRuleFactory.Save();

				TestObjectCreator.Job1.Factory.Save();

				int countOfRulesBefore = Factory.GetDatabaseCount(typeof(OrgPatternMatchOverride));

				InvoicingBase invoice = TestObjectCreator.CreateAndAllocateInvoiceWithUniversalTransactionInAllocationApprovalRequest(GetExpectedBusinessObjectType(), universalTransaction, true);
				AssertEquals("Precondition: invoice creditor", TestObjectCreator.Creditor1.OH_Code, invoice.Header.OH_Code);
				AssertEquals("Precondition: invoice lines count", 1, invoice.Lines.Count);
				var line = invoice.Lines[0];
				AssertEquals("Precondition: line charge code", TestObjectCreator.CC2.AC_Code, line.GenericChargeBizO.VC_Code);

				AssertEquals("ImportedCreditorXmlCode", universalTransaction.DataContext.DataProviderForCodeMapping, invoice.ImportedCreditorXmlCode);
				AssertEquals("ImportedCreditor", isDataProviderOrgMatchingRuleExist ? importedCreditor.OH_Code : ZString.Empty, invoice.ImportedCreditor);
				AssertEquals("ImportedChargeCodeXmlCode", universalLine.ChargeCode.Code.Value, line.ImportedChargeCodeXmlCode);
				AssertEquals("ImportedChargeCode", TestObjectCreator.CC2.AC_Code, line.ImportedChargeCode);
				if (isDataProviderOrgMatchingRuleExist)
				{
					AssertHasWarning(invoice.AH_OHInfo, "Creditor value is different to a value matched by default. Organization matching rule for foreign code 'EDIDATDEM' will be updated.");
				}
				else
				{
					AssertHasWarning(invoice.AH_OHInfo, "Organization is not found for imported XML code 'EDIDATDEM'. New matching rule will be created for foreign code 'EDIDATDEM'.");
				}
				AssertNoWarnings(line.GenericChargeInfo);

				invoice.AH_OH = ZGuid.Empty;
				AssertEquals("ImportedCreditorXmlCode", universalTransaction.DataContext.DataProviderForCodeMapping, invoice.ImportedCreditorXmlCode);
				AssertEquals("ImportedCreditor", isDataProviderOrgMatchingRuleExist ? importedCreditor.OH_Code : ZString.Empty, invoice.ImportedCreditor);
				AssertEquals("ImportedChargeCodeXmlCode", universalLine.ChargeCode.Code.Value, line.ImportedChargeCodeXmlCode);
				AssertEquals("ImportedChargeCode: should not be recalculated if no org entered", TestObjectCreator.CC2.AC_Code, line.ImportedChargeCode);
				AssertNoWarnings(invoice.AH_OHInfo);
				AssertNoWarnings(line.GenericChargeInfo);

				invoice.AH_OH = importedCreditor.PK;
				AssertEquals("ImportedCreditorXmlCode", universalTransaction.DataContext.DataProviderForCodeMapping, invoice.ImportedCreditorXmlCode);
				AssertEquals("ImportedCreditor", isDataProviderOrgMatchingRuleExist ? importedCreditor.OH_Code : ZString.Empty, invoice.ImportedCreditor);
				AssertEquals("ImportedChargeCodeXmlCode", universalLine.ChargeCode.Code.Value, line.ImportedChargeCodeXmlCode);
				AssertEquals("ImportedChargeCode", TestObjectCreator.CC1.AC_Code, line.ImportedChargeCode);
				if (isDataProviderOrgMatchingRuleExist)
				{
					AssertNoWarnings(invoice.AH_OHInfo);
				}
				else
				{
					AssertHasWarning(invoice.AH_OHInfo, "Organization is not found for imported XML code 'EDIDATDEM'. New matching rule will be created for foreign code 'EDIDATDEM'.");
				}
				AssertHasWarning(line.GenericChargeInfo, "Charge code value is different to a value matched by default. Charge code matching rule for foreign code 'FFF' will be updated.");

				line.AL_OSExTaxAmount = 100;
				line.AL_JH = TestObjectCreator.Job1.PK;

				var orgProxyInMatchingRuleFactory = matchingRuleFactory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
				var filter = new ZQuery(OrgPatternMatchOverrideSchema.OO_Relationship, Constants.OrgPatternMatchOverrideRelationships.Organisation);
				filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_ForeignCode, universalTransaction.DataContext.DataProviderForCodeMapping);
				filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_OH, orgProxyInMatchingRuleFactory.PK);
				var loadedMatchingRules = orgProxyInMatchingRuleFactory.Factory.Load<OrgPatternMatchOverride>(filter);
				AssertEquals("loadedMatchingRules.Length before invoice saving.", isDataProviderOrgMatchingRuleExist ? 1 : 0, loadedMatchingRules.Length);

				invoice.Factory.Save();
				int countOfRulesAfter = Factory.GetDatabaseCount(typeof(OrgPatternMatchOverride));
				if (isDataProviderOrgMatchingRuleExist)
				{
					AssertEquals("No new matching rules should be created after all changes that user reverted back.", 0, countOfRulesAfter - countOfRulesBefore);
				}
				else
				{
					AssertEquals("New matching rule should be created as imported creditor is a calculated value and not set from a rule.", 1, countOfRulesAfter - countOfRulesBefore);
				}
				loadedMatchingRules = orgProxyInMatchingRuleFactory.Factory.Load<OrgPatternMatchOverride>(filter);
				AssertEquals("loadedMatchingRules.Length after invoice saving.", 1, loadedMatchingRules.Length);
				var patternMatchOverride = loadedMatchingRules[0];
				AssertEquals("Organization matching rule is created", importedCreditor.PK, patternMatchOverride.OO_LocalGuid);
				AssertEquals("Charge Code FFF matching rule for importedCreditor is updated", TestObjectCreator.CC2.AC_Code, patternMatchOverride1.OO_LocalCode);
				AssertEquals("Charge Code FFF matching rule for Creditor1 is not updated", TestObjectCreator.CC2.AC_Code, patternMatchOverride2.OO_LocalCode);
			}
		}

		#endregion

		public void TestImportedXMLCodeMappingForCodesWithoutMatchingRules()
		{
			if (GetExpectedBusinessObjectType() != typeof(APInvoice) && GetExpectedBusinessObjectType() != typeof(APCreditNote))
			{
				Assert(true);
				return;
			}
			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			orgAddress.AddressType = nameof(DocAddressType.None);
			orgAddress.Address1 = "Street";
			var expectedOrgCode = TestObjectCreator.Creditor1.OH_Code;
			orgAddress.OrganizationCode = expectedOrgCode;

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.Ledger = LedgerTypes.AccountsPayable;
			universalTransaction.OrganizationAddress = orgAddress;
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal> { new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) });

			InvoicingBase invoice = TestObjectCreator.CreateInvoiceWithUniversalTransactionInAllocationApprovalRequest(GetExpectedBusinessObjectType(), universalTransaction);
			var approvalRequest = Factory.LoadTop1<TransactionPendingAllocationApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoice.PK));
			AssertNotNull(nameof(approvalRequest), approvalRequest);
			var universalTransactionWrapper = approvalRequest.PostingDetails.UniversalTransaction;
			AssertEquals(nameof(universalTransactionWrapper.CreditorSource), expectedOrgCode, universalTransactionWrapper.CreditorSource);
			AssertEquals(nameof(universalTransactionWrapper.Creditor) + ". Mapping of codes without matching rules should not be triggered before accessing ImportedCreditor on invoice to not do a job if nobody need it.", ZString.Empty, universalTransactionWrapper.Creditor);

			AssertEquals(nameof(invoice.ImportedCreditorXmlCode), expectedOrgCode, invoice.ImportedCreditorXmlCode);
			AssertEquals(nameof(invoice.ImportedCreditor), expectedOrgCode, invoice.ImportedCreditor);
		}

		public void TestAllocationApprovalRequest()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), organisation: TestObjectCreator.Creditor1);
			TestObjectCreator.CreateInvoiceLine(invoice, 100);
			invoice.SaveAsIncomplete();

			//invoice should be loaded in new factory each time as in real life we can't have changes to allocation approval request ones invoice is allocated.
			Func<InvoicingBase> getInvoiceInNewFactory = () => new BusinessObjectFactory().Load<InvoicingBase>(invoice.PK);

			var request1 = new BusinessObjectFactory().New<TransactionPendingAllocationApprovalRequest>();
			request1.XP_ParentID = invoice.PK;
			request1.XP_ParentTableCode = invoice.TablePrefix;
			var expectedCreateTime1 = ZDateTime.Today.AddDays(-2);
			request1.XP_SystemCreateTimeUtc = expectedCreateTime1;
			request1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Cancelled;
			AssertNull("AllocationApprovalRequest", getInvoiceInNewFactory().AllocationApprovalRequest);
			request1.Factory.Save();
			AssertEquals("AllocationApprovalRequest", request1.PK, getInvoiceInNewFactory().AllocationApprovalRequest.PK);

			var request2 = new BusinessObjectFactory().New<TransactionPendingAllocationApprovalRequest>();
			request2.XP_ParentID = invoice.PK;
			request2.XP_ParentTableCode = invoice.TablePrefix;
			request2.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Rejected;
			var expectedCreateTime2 = ZDateTime.Today.AddDays(-5);
			request2.XP_SystemCreateTimeUtc = expectedCreateTime2;
			request2.Factory.Save();
			AssertEquals("Precondition: request1.XP_SystemCreateTimeUtc", expectedCreateTime1, request1.XP_SystemCreateTimeUtc);
			AssertEquals("Precondition: request2.XP_SystemCreateTimeUtc", expectedCreateTime2, request2.XP_SystemCreateTimeUtc);
			AssertEquals("AllocationApprovalRequest", request1.PK, getInvoiceInNewFactory().AllocationApprovalRequest.PK);

			expectedCreateTime2 = ZDateTime.Today.AddDays(-1);
			request2.XP_SystemCreateTimeUtc = expectedCreateTime2;
			request2.Factory.Save();
			AssertEquals("Precondition: request1.XP_SystemCreateTimeUtc", expectedCreateTime1, request1.XP_SystemCreateTimeUtc);
			AssertEquals("Precondition: request2.XP_SystemCreateTimeUtc", expectedCreateTime2, request2.XP_SystemCreateTimeUtc);
			AssertEquals("AllocationApprovalRequest", request2.PK, getInvoiceInNewFactory().AllocationApprovalRequest.PK);
		}
	}
}
