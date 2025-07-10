using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.CommissionManagement.Business;
using Enterprise.CommissionManagement.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.CommissionManagement.Business.Test
{
	internal class EDINonJobRelatedTransactionCommissionCreatorTest : CommissionCreatorTestCase
	{
		public void TestCreateCommissions_ChargeCodeDefaultItemsResponsibleBySingleAgreement()
		{
			var chargeCode_XXX_XXX_XXX = NewChargeCodeWithDefaultItem("XXX", "XXX", "XXX");
			var chargeCode_XXX_XXX_YYY = NewChargeCodeWithDefaultItem("XXX", "XXX", "YYY");

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = customer.PK;
			AddNewLine(invoice, chargeCode_XXX_XXX_XXX);
			AddNewLine(invoice, chargeCode_XXX_XXX_XXX);
			AddNewLine(invoice, chargeCode_XXX_XXX_YYY);

			Factory.Save();

			var opportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, customer);
			var agreement_XXX_XXX_ALL = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunity, customer, "XXX", "XXX", "ALL");
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(agreement_XXX_XXX_ALL, "ADL", 10);

			Factory.Save();

			EDINonJobRelatedTransactionCommissionCreator.New(invoice).CreateCommissions();

			var commissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery());
			var commissionHeader = commissionHeaders.Single();
			AssertEquals("Should create commission for the agreement responsible for all charge code default items.", agreement_XXX_XXX_ALL.PK, commissionHeader.CH0_CA0);

			var ambiguousCommissions = Factory.Load<AccAmbiguousCommission>(new ZQuery());
			AssertEquals(0, ambiguousCommissions.Length);
		}

		public void TestCreateCommissions_ChargeCodeDefaultItemsResponsibleBySingleAgreement_InvoiceDebtorMatchesOpportunityClient()
		{
			var chargeCode_XXX_XXX_XXX = NewChargeCodeWithDefaultItem("XXX", "XXX", "XXX");
			var chargeCode_XXX_XXX_YYY = NewChargeCodeWithDefaultItem("XXX", "XXX", "YYY");

			var parentOrg = Factory.NewWithValidTestData<OrgHeader>();
			var childOrg = Factory.NewWithValidTestData<OrgHeader>();
			var invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = parentOrg.PK;
			AddNewLine(invoice, chargeCode_XXX_XXX_XXX);
			AddNewLine(invoice, chargeCode_XXX_XXX_XXX);
			AddNewLine(invoice, chargeCode_XXX_XXX_YYY);

			Factory.Save();

			var parentOrgOpportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, parentOrg);
			var childOrgAgreement_XXX_XXX_ALL = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(parentOrgOpportunity, childOrg, "XXX", "XXX", "ALL");
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(childOrgAgreement_XXX_XXX_ALL, "ADL", 10);

			Factory.Save();

			EDINonJobRelatedTransactionCommissionCreator.New(invoice).CreateCommissions();

			var commissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery());
			var commissionHeader = commissionHeaders.Single();
			AssertEquals("Should create commission for the agreement responsible for all charge code default items.", childOrgAgreement_XXX_XXX_ALL.PK, commissionHeader.CH0_CA0);

			var ambiguousCommissions = Factory.Load<AccAmbiguousCommission>(new ZQuery());
			AssertEquals(0, ambiguousCommissions.Length);
		}

		public void TestCreateCommissions_ChargeCodeDefaultItemsResponsibleBySingleAgreement_AndWithExistingCommission()
		{
			var chargeCode_XXX_XXX_XXX = NewChargeCodeWithDefaultItem("XXX", "XXX", "XXX");
			var chargeCode_XXX_XXX_YYY = NewChargeCodeWithDefaultItem("XXX", "XXX", "YYY");

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = customer.PK;
			AddNewLine(invoice, chargeCode_XXX_XXX_XXX);
			AddNewLine(invoice, chargeCode_XXX_XXX_XXX);
			AddNewLine(invoice, chargeCode_XXX_XXX_YYY);

			Factory.Save();

			var opportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, customer);
			var agreement_XXX_XXX_ALL = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunity, customer, "XXX", "XXX", "ALL");
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(agreement_XXX_XXX_ALL, "ADL", 10);

			var existingCommissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			existingCommissionHeader.CH0_AH_Source = invoice.PK;
			existingCommissionHeader.CH0_GroupingSourceID = invoice.PK;
			existingCommissionHeader.CH0_GroupingSourceTableCode = invoice.TablePrefix;
			existingCommissionHeader.CH0_OH_Customer = customer.PK;
			existingCommissionHeader.CH0_Product = OrgCommissionAgreementItemLookups.AllProductsCode;
			existingCommissionHeader.CH0_Service = OrgCommissionAgreementItemLookups.AllServicesCode;
			existingCommissionHeader.CH0_SubModule = OrgCommissionAgreementItemLookups.AllSubModulesCode;
			existingCommissionHeader.Lines.AddNew().FillWithValidTestData();

			Factory.Save();

			EDINonJobRelatedTransactionCommissionCreator.New(invoice).CreateCommissions();

			var newCommissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.PK, SQLComparisonOperator.NotEqual, existingCommissionHeader.PK));
			AssertEquals("Should not create commission header - commission already exists", 0, newCommissionHeaders.Length);

			var ambiguousCommissions = Factory.Load<AccAmbiguousCommission>(new ZQuery());
			AssertEquals("Should not have flagged invoice as unresolved commission - commission already exists", 0, ambiguousCommissions.Length);
		}

		public void TestCreateCommissions_ChargeCodeDefaultItemsResponsibleBySingleAgreement_AndWithExistingCommission_AndOverwriteOldValues()
		{
			var chargeCode_XXX_XXX_XXX = NewChargeCodeWithDefaultItem("XXX", "XXX", "XXX");
			var chargeCode_XXX_XXX_YYY = NewChargeCodeWithDefaultItem("XXX", "XXX", "YYY");

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = customer.PK;
			AddNewLine(invoice, chargeCode_XXX_XXX_XXX);
			AddNewLine(invoice, chargeCode_XXX_XXX_XXX);
			AddNewLine(invoice, chargeCode_XXX_XXX_YYY);

			Factory.Save();

			var opportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, customer);
			var agreement_XXX_XXX_ALL = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunity, customer, "XXX", "XXX", "ALL");
			var recipient = OrgCommissionAgreementTestHelper.AddPercentageRecipient(agreement_XXX_XXX_ALL, "ADL", 10);

			var existingCommissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			existingCommissionHeader.CH0_AH_Source = invoice.PK;
			existingCommissionHeader.CH0_GroupingSourceID = invoice.PK;
			existingCommissionHeader.CH0_GroupingSourceTableCode = invoice.TablePrefix;
			existingCommissionHeader.CH0_OH_Customer = customer.PK;
			existingCommissionHeader.CH0_Product = OrgCommissionAgreementItemLookups.AllProductsCode;
			existingCommissionHeader.CH0_Service = OrgCommissionAgreementItemLookups.AllServicesCode;
			existingCommissionHeader.CH0_SubModule = OrgCommissionAgreementItemLookups.AllSubModulesCode;

			var existingLineGroup = existingCommissionHeader.LineGroups.AddNew();
			existingLineGroup.FillWithValidTestData();
			existingLineGroup.CLG_AC = chargeCode_XXX_XXX_XXX.PK;

			var existingLine = existingLineGroup.Lines.AddNew();
			existingLine.FillWithValidTestData();
			existingLine.CL0_GS_NKStaff = recipient.CAR_GS_NKStaff;
			existingLine.CL0_OH_Party = recipient.CAR_OH_Party;
			existingLine.CL0_RX_NKTransactionCurrency = "AUD";

			Factory.Save();

			agreement_XXX_XXX_ALL.CA0_CommissionBasis = CommissionBasisType.Codes.PRF; // Make a change
			Factory.Save();

			var createContext = new CreateCommissionContext();
			createContext.OverwriteOldValues = true;
			EDINonJobRelatedTransactionCommissionCreator.New(invoice).CreateCommissions(createContext);

			var newCommissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.PK, SQLComparisonOperator.NotEqual, existingCommissionHeader.PK));
			AssertEquals("Should override old commission and create a new one", true, existingCommissionHeader.IsOverriden);
			AssertEquals("Should override old commission and create a new one", 1, newCommissionHeaders.Length);
			AssertEquals("New Commission should be for same agreement as old one", agreement_XXX_XXX_ALL.PK, newCommissionHeaders[0].CH0_CA0);

			var ambiguousCommissions = Factory.Load<AccAmbiguousCommission>(new ZQuery());
			AssertEquals("Should not have flagged invoice as unresolved commission - commission already exists", 0, ambiguousCommissions.Length);
		}

		public void TestCreateCommissions_ChargeCodeDefaultItemsResponsibleBySingleAgreement_AndWithExistingUnresolvedAmbiguousCommission()
		{
			var chargeCode_XXX_XXX_XXX = NewChargeCodeWithDefaultItem("XXX", "XXX", "XXX");
			var chargeCode_XXX_XXX_YYY = NewChargeCodeWithDefaultItem("XXX", "XXX", "YYY");

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = customer.PK;
			AddNewLine(invoice, chargeCode_XXX_XXX_XXX);
			AddNewLine(invoice, chargeCode_XXX_XXX_YYY);

			Factory.Save();

			var opportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, customer);
			var agreement_XXX_XXX_ALL = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunity, customer, "XXX", "XXX", "ALL");
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(agreement_XXX_XXX_ALL, "ADL", 10);

			var existingUnresolvedAmbiguousCommission = Factory.New<AccAmbiguousCommission>();
			existingUnresolvedAmbiguousCommission.AC0_AH_Source = invoice.PK;
			existingUnresolvedAmbiguousCommission.AC0_CA0_SelectedAgreement = ZGuid.Empty;

			Factory.Save();

			var createContext = new CreateCommissionContext();
			createContext.OverwriteOldValues = false;
			EDINonJobRelatedTransactionCommissionCreator.New(invoice).CreateCommissions(createContext);

			var commissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery());
			AssertEquals("Should create commission header - all line charge codes have a default item responsible by the same agreement", 1, commissionHeaders.Length);

			AssertEquals("Commission should not longer be unresolved", true, existingUnresolvedAmbiguousCommission.IsDeleted);
			AssertEquals(0, Factory.Load<AccAmbiguousCommission>(new ZQuery()).Length);
		}

		public void TestCreateCommissions_ChargeCodeDefaultItemsResponsibleBySingleAgreement_AndWithExistingUnresolvedAmbiguousCommission_AgreementAndRatesOverride()
		{
			var chargeCode_XXX_XXX_XXX = NewChargeCodeWithDefaultItem("XXX", "XXX", "XXX");
			var chargeCode_XXX_XXX_YYY = NewChargeCodeWithDefaultItem("XXX", "XXX", "YYY");

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = customer.PK;
			AddNewLine(invoice, chargeCode_XXX_XXX_XXX);
			AddNewLine(invoice, chargeCode_XXX_XXX_YYY);

			Factory.Save();

			var opportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, customer);
			var agreement_XXX_XXX_ALL = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunity, customer, "XXX", "XXX", "ALL");
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(agreement_XXX_XXX_ALL, "ADL", 10);

			var existingUnresolvedAmbiguousCommission = Factory.New<AccAmbiguousCommission>();
			existingUnresolvedAmbiguousCommission.AC0_AH_Source = invoice.PK;
			existingUnresolvedAmbiguousCommission.AC0_CA0_SelectedAgreement = ZGuid.Empty;

			Factory.Save();

			var createContext = new CreateCommissionContext();
			createContext.AgreementAndRatesOverride = new Dictionary<ZString, ICommissionAgreementAndRates>() { { ZString.Empty, new CommissionAgreementAndRates(agreement_XXX_XXX_ALL, new ZDate(2000, 1, 1)) } };
			EDINonJobRelatedTransactionCommissionCreator.New(invoice).CreateCommissions(createContext);

			var commissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery());
			AssertEquals("Should create commission header - all line charge codes have a default item responsible by the same agreement", 1, commissionHeaders.Length);

			AssertEquals("Commission should not longer be unresolved", agreement_XXX_XXX_ALL.PK, existingUnresolvedAmbiguousCommission.AC0_CA0_SelectedAgreement);
			AssertEquals(1, Factory.Load<AccAmbiguousCommission>(new ZQuery()).Length);
		}

		public void TestCreateCommissions_ChargeCodeDefaultItemsResponsibleByMultipleAgreements()
		{
			var chargeCode_XXX_XXX_XXX = NewChargeCodeWithDefaultItem("XXX", "XXX", "XXX");
			var chargeCode_XXX_XXX_YYY = NewChargeCodeWithDefaultItem("XXX", "XXX", "YYY");
			var chargeCode_ZZZ_ALL_ALL = NewChargeCodeWithDefaultItem("ZZZ", "ALL", "ALL");

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = customer.PK;
			AddNewLine(invoice, chargeCode_XXX_XXX_XXX);
			AddNewLine(invoice, chargeCode_XXX_XXX_YYY);
			AddNewLine(invoice, chargeCode_ZZZ_ALL_ALL);

			Factory.Save();

			var opportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, customer);
			var agreement_XXX_XXX_ALL = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunity, customer, "XXX", "XXX", "ALL");
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(agreement_XXX_XXX_ALL, "ADL", 10);
			var agreement_ZZZ_ALL_ALL = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunity, customer, "ZZZ", "ALL", "ALL");
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(agreement_ZZZ_ALL_ALL, "ADL", 10);

			Factory.Save();

			EDINonJobRelatedTransactionCommissionCreator.New(invoice).CreateCommissions();

			var commissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery());
			AssertEquals("Should not create commission header - Not all line charge codes have a default item responsible by the same agreement", 0, commissionHeaders.Length);

			var ambiguousCommissions = Factory.Load<AccAmbiguousCommission>(new ZQuery());
			var ambiguousCommission = ambiguousCommissions.Single();
			AssertEquals("Should have flagged invoice as unresolved commission", invoice.PK, ambiguousCommission.AC0_AH_Source);
			AssertEquals("Should have flagged invoice as unresolved commission", ZGuid.Empty, ambiguousCommission.AC0_CA0_SelectedAgreement);
		}

		public void TestCreateCommissions_ChargeCodeDefaultItemsResponsibleByMultipleAgreements_InvoiceDebtorMatchesMultipleOpportunityClients()
		{
			var chargeCode_XXX_XXX_XXX = NewChargeCodeWithDefaultItem("XXX", "XXX", "XXX");

			var parentOrg = Factory.NewWithValidTestData<OrgHeader>();
			var childOrgA = Factory.NewWithValidTestData<OrgHeader>();
			var childOrgB = Factory.NewWithValidTestData<OrgHeader>();
			var invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = parentOrg.PK;
			AddNewLine(invoice, chargeCode_XXX_XXX_XXX);
			AddNewLine(invoice, chargeCode_XXX_XXX_XXX);

			Factory.Save();

			var parentOrgOpportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, parentOrg);
			var childOrgAAgreement = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(parentOrgOpportunity, childOrgA, "XXX", "XXX", "XXX");
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(childOrgAAgreement, "ADL", 10);
			var childOrgBAgreement = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(parentOrgOpportunity, childOrgB, "XXX", "XXX", "ALL");
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(childOrgBAgreement, "ADL", 10);

			Factory.Save();

			EDINonJobRelatedTransactionCommissionCreator.New(invoice).CreateCommissions();

			var commissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery());
			AssertEquals("Should not create commission header - Not all line charge codes have a default item responsible by the same agreement.", 0, commissionHeaders.Length);

			var ambiguousCommissions = Factory.Load<AccAmbiguousCommission>(new ZQuery());
			var ambiguousCommission = ambiguousCommissions.Single();
			AssertEquals("Should have flagged invoice as unresolved commission", invoice.PK, ambiguousCommission.AC0_AH_Source);
			AssertEquals("Should have flagged invoice as unresolved commission", ZGuid.Empty, ambiguousCommission.AC0_CA0_SelectedAgreement);
		}

		public void TestCreateCommissions_ChargeCodeDefaultItemsResponsibleByMultipleAgreements_InvoiceDebtorMatchesMixtureOfOpportunityClientAndAgreementCustomer()
		{
			var chargeCode_XXX_XXX_XXX = NewChargeCodeWithDefaultItem("XXX", "XXX", "XXX");
			var chargeCode_XXX_XXX_YYY = NewChargeCodeWithDefaultItem("XXX", "XXX", "YYY");

			var parentOrg = Factory.NewWithValidTestData<OrgHeader>();
			var childOrg = Factory.NewWithValidTestData<OrgHeader>();
			var subChildOrg = Factory.NewWithValidTestData<OrgHeader>();
			var invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = childOrg.PK;
			AddNewLine(invoice, chargeCode_XXX_XXX_XXX);
			AddNewLine(invoice, chargeCode_XXX_XXX_XXX);
			AddNewLine(invoice, chargeCode_XXX_XXX_YYY);

			Factory.Save();

			var parentOrgOpportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, parentOrg);
			var parentOrgOpportunityChildOrgAgreement = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(parentOrgOpportunity, childOrg, "XXX", "XXX", "ALL");
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(parentOrgOpportunityChildOrgAgreement, "ADL", 10);

			var childOrgOppotunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, childOrg);
			var childOrgOppotunitySubChildOrgAgreement = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(childOrgOppotunity, subChildOrg, "XXX", "XXX", "ALL");
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(childOrgOppotunitySubChildOrgAgreement, "ADL", 10);

			Factory.Save();

			EDINonJobRelatedTransactionCommissionCreator.New(invoice).CreateCommissions();

			var commissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery());
			AssertEquals("Should not create commission header - Not all line charge codes have a default item responsible by the same agreement.", 0, commissionHeaders.Length);

			var ambiguousCommissions = Factory.Load<AccAmbiguousCommission>(new ZQuery());
			var ambiguousCommission = ambiguousCommissions.Single();
			AssertEquals("Should have flagged invoice as unresolved commission", invoice.PK, ambiguousCommission.AC0_AH_Source);
			AssertEquals("Should have flagged invoice as unresolved commission", ZGuid.Empty, ambiguousCommission.AC0_CA0_SelectedAgreement);
		}

		public void TestCreateCommissions_ChargeCodeDefaultItemsResponsibleByMultipleAgreements_AndWithExistingCommission()
		{
			var chargeCode_XXX_XXX_XXX = NewChargeCodeWithDefaultItem("XXX", "XXX", "XXX");
			var chargeCode_XXX_XXX_YYY = NewChargeCodeWithDefaultItem("XXX", "XXX", "YYY");
			var chargeCode_ZZZ_ALL_ALL = NewChargeCodeWithDefaultItem("ZZZ", "ALL", "ALL");

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = customer.PK;
			AddNewLine(invoice, chargeCode_XXX_XXX_XXX);
			AddNewLine(invoice, chargeCode_XXX_XXX_YYY);
			AddNewLine(invoice, chargeCode_ZZZ_ALL_ALL);

			Factory.Save();

			var opportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, customer);
			var agreement_XXX_XXX_ALL = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunity, customer, "XXX", "XXX", "ALL");
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(agreement_XXX_XXX_ALL, "ADL", 10);
			var agreement_ZZZ_ALL_ALL = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunity, customer, "ZZZ", "ALL", "ALL");
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(agreement_ZZZ_ALL_ALL, "ADL", 10);

			var existingCommissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			existingCommissionHeader.CH0_AH_Source = invoice.PK;
			existingCommissionHeader.CH0_GroupingSourceID = invoice.PK;
			existingCommissionHeader.CH0_GroupingSourceTableCode = invoice.TablePrefix;
			existingCommissionHeader.CH0_OH_Customer = customer.PK;
			existingCommissionHeader.CH0_Product = OrgCommissionAgreementItemLookups.AllProductsCode;
			existingCommissionHeader.CH0_Service = OrgCommissionAgreementItemLookups.AllServicesCode;
			existingCommissionHeader.CH0_SubModule = OrgCommissionAgreementItemLookups.AllSubModulesCode;
			existingCommissionHeader.Lines.AddNew().FillWithValidTestData();

			Factory.Save();

			var createContext = new CreateCommissionContext();
			createContext.OverwriteOldValues = false;
			EDINonJobRelatedTransactionCommissionCreator.New(invoice).CreateCommissions(createContext);

			var newCommissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.PK, SQLComparisonOperator.NotEqual, existingCommissionHeader.PK));
			AssertEquals("Should not create commission header - commission already exists", 0, newCommissionHeaders.Length);

			var ambiguousCommissions = Factory.Load<AccAmbiguousCommission>(new ZQuery());
			AssertEquals("Should not have flagged invoice as unresolved commission - commission already exists", 0, ambiguousCommissions.Length);
		}

		public void TestCreateCommissions_ChargeCodeDefaultItemsResponsibleByMultipleAgreements_AndWithExistingCommission_AndOverwriteOldValues()
		{
			var chargeCode_XXX_XXX_XXX = NewChargeCodeWithDefaultItem("XXX", "XXX", "XXX");
			var chargeCode_XXX_XXX_YYY = NewChargeCodeWithDefaultItem("XXX", "XXX", "YYY");
			var chargeCode_ZZZ_ALL_ALL = NewChargeCodeWithDefaultItem("ZZZ", "ALL", "ALL");

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = customer.PK;
			AddNewLine(invoice, chargeCode_XXX_XXX_XXX);
			AddNewLine(invoice, chargeCode_XXX_XXX_YYY);
			AddNewLine(invoice, chargeCode_ZZZ_ALL_ALL);

			Factory.Save();

			var opportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, customer);
			var agreement_XXX_XXX_ALL = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunity, customer, "XXX", "XXX", "ALL");
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(agreement_XXX_XXX_ALL, "ADL", 10);
			var agreement_ZZZ_ALL_ALL = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunity, customer, "ZZZ", "ALL", "ALL");
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(agreement_ZZZ_ALL_ALL, "ADL", 10);

			var existingCommissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			existingCommissionHeader.CH0_AH_Source = invoice.PK;
			existingCommissionHeader.CH0_GroupingSourceID = invoice.PK;
			existingCommissionHeader.CH0_GroupingSourceTableCode = invoice.TablePrefix;
			existingCommissionHeader.CH0_OH_Customer = customer.PK;
			existingCommissionHeader.CH0_Product = OrgCommissionAgreementItemLookups.AllProductsCode;
			existingCommissionHeader.CH0_Service = OrgCommissionAgreementItemLookups.AllServicesCode;
			existingCommissionHeader.CH0_SubModule = OrgCommissionAgreementItemLookups.AllSubModulesCode;
			existingCommissionHeader.CH0_CA0 = agreement_ZZZ_ALL_ALL.PK;
			existingCommissionHeader.Lines.AddNew().FillWithValidTestData();

			Factory.Save();

			agreement_ZZZ_ALL_ALL.CA0_CommissionBasis = CommissionBasisType.Codes.PRF; // Make a change
			Factory.Save();

			var createContext = new CreateCommissionContext();
			createContext.OverwriteOldValues = true;
			EDINonJobRelatedTransactionCommissionCreator.New(invoice).CreateCommissions(createContext);

			var newCommissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.PK, SQLComparisonOperator.NotEqual, existingCommissionHeader.PK));
			AssertEquals("Should override old commission and flag commission an unresolved commission", true, existingCommissionHeader.IsOverriden);
			AssertEquals("Should override old commission and flag commission an unresolved commission", 0, newCommissionHeaders.Length);

			var ambiguousCommissions = Factory.Load<AccAmbiguousCommission>(new ZQuery());
			var ambiguousCommission = ambiguousCommissions.Single();
			AssertEquals("Should have flagged invoice as unresolved commission", invoice.PK, ambiguousCommission.AC0_AH_Source);
			AssertEquals("Should have flagged invoice as unresolved commission", ZGuid.Empty, ambiguousCommission.AC0_CA0_SelectedAgreement);
		}

		public void TestCreateCommissions_ChargeCodeDefaultItemsResponsibleByMultipleAgreements_AndWithExistingCommission_AndOverwriteOldValues_AndWithSpecificAgreementsOnly()
		{
			var chargeCode_XXX_XXX_XXX = NewChargeCodeWithDefaultItem("XXX", "XXX", "XXX");
			var chargeCode_XXX_XXX_YYY = NewChargeCodeWithDefaultItem("XXX", "XXX", "YYY");
			var chargeCode_ZZZ_ALL_ALL = NewChargeCodeWithDefaultItem("ZZZ", "ALL", "ALL");

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = customer.PK;
			AddNewLine(invoice, chargeCode_XXX_XXX_XXX);
			AddNewLine(invoice, chargeCode_XXX_XXX_YYY);
			AddNewLine(invoice, chargeCode_ZZZ_ALL_ALL);

			Factory.Save();

			var opportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, customer);
			var agreement_XXX_XXX_ALL = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunity, customer, "XXX", "XXX", "ALL");
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(agreement_XXX_XXX_ALL, "ADL", 10);
			var agreement_ZZZ_ALL_ALL = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunity, customer, "ZZZ", "ALL", "ALL");
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(agreement_ZZZ_ALL_ALL, "ADL", 10);

			var existingCommissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			existingCommissionHeader.CH0_AH_Source = invoice.PK;
			existingCommissionHeader.CH0_GroupingSourceID = invoice.PK;
			existingCommissionHeader.CH0_GroupingSourceTableCode = invoice.TablePrefix;
			existingCommissionHeader.CH0_OH_Customer = customer.PK;
			existingCommissionHeader.CH0_Product = OrgCommissionAgreementItemLookups.AllProductsCode;
			existingCommissionHeader.CH0_Service = OrgCommissionAgreementItemLookups.AllServicesCode;
			existingCommissionHeader.CH0_SubModule = OrgCommissionAgreementItemLookups.AllSubModulesCode;
			existingCommissionHeader.CH0_CA0 = agreement_ZZZ_ALL_ALL.PK;
			existingCommissionHeader.Lines.AddNew().FillWithValidTestData();

			Factory.Save();

			agreement_ZZZ_ALL_ALL.CA0_CommissionBasis = CommissionBasisType.Codes.PRF; // Make a change
			Factory.Save();

			var createContext = new CreateCommissionContext();
			createContext.OverwriteOldValues = true;
			createContext.AgreementsBeingApproved = new HashSet<OrgCommissionAgreement>() { agreement_XXX_XXX_ALL };
			EDINonJobRelatedTransactionCommissionCreator.New(invoice).CreateCommissions(createContext);

			var newCommissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.PK, SQLComparisonOperator.NotEqual, existingCommissionHeader.PK));
			AssertEquals("Should override old commission and flag commission an unresolved commission", true, existingCommissionHeader.IsOverriden);
			AssertEquals("Should override old commission and flag commission an unresolved commission", 0, newCommissionHeaders.Length);

			var ambiguousCommissions = Factory.Load<AccAmbiguousCommission>(new ZQuery());
			var ambiguousCommission = ambiguousCommissions.Single();
			AssertEquals("Should have flagged invoice as unresolved commission", invoice.PK, ambiguousCommission.AC0_AH_Source);
			AssertEquals("Should have flagged invoice as unresolved commission", ZGuid.Empty, ambiguousCommission.AC0_CA0_SelectedAgreement);
		}

		public void TestCreateCommissions_ChargeCodeDefaultItemsResponsibleByMultipleAgreements_AndWithExistingCommissionWhereAgreementIsReversed_AndOverwriteOldValues()
		{
			var chargeCode_XXX_XXX_XXX = NewChargeCodeWithDefaultItem("XXX", "XXX", "XXX");
			var chargeCode_XXX_XXX_YYY = NewChargeCodeWithDefaultItem("XXX", "XXX", "YYY");
			var chargeCode_YYY_ALL_ALL = NewChargeCodeWithDefaultItem("YYY", "ALL", "ALL");
			var chargeCode_ZZZ_ALL_ALL = NewChargeCodeWithDefaultItem("ZZZ", "ALL", "ALL");

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = customer.PK;
			AddNewLine(invoice, chargeCode_XXX_XXX_XXX);
			AddNewLine(invoice, chargeCode_XXX_XXX_YYY);
			AddNewLine(invoice, chargeCode_YYY_ALL_ALL);
			AddNewLine(invoice, chargeCode_ZZZ_ALL_ALL);

			Factory.Save();

			var opportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, customer);
			var agreement_XXX_XXX_ALL = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunity, customer, "XXX", "XXX", "ALL");
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(agreement_XXX_XXX_ALL, "ADL", 10);
			var agreement_YYY_ALL_ALL = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunity, customer, "YYY", "ALL", "ALL");
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(agreement_YYY_ALL_ALL, "ADL", 10);
			var agreement_ZZZ_ALL_ALL = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunity, customer, "ZZZ", "ALL", "ALL");
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(agreement_ZZZ_ALL_ALL, "ADL", 10);

			var existingCommissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			existingCommissionHeader.CH0_AH_Source = invoice.PK;
			existingCommissionHeader.CH0_GroupingSourceID = invoice.PK;
			existingCommissionHeader.CH0_GroupingSourceTableCode = invoice.TablePrefix;
			existingCommissionHeader.CH0_OH_Customer = customer.PK;
			existingCommissionHeader.CH0_Product = OrgCommissionAgreementItemLookups.AllProductsCode;
			existingCommissionHeader.CH0_Service = OrgCommissionAgreementItemLookups.AllServicesCode;
			existingCommissionHeader.CH0_SubModule = OrgCommissionAgreementItemLookups.AllSubModulesCode;
			existingCommissionHeader.CH0_CA0 = agreement_ZZZ_ALL_ALL.PK;
			existingCommissionHeader.Lines.AddNew().FillWithValidTestData();

			Factory.Save();

			agreement_ZZZ_ALL_ALL.Reverse();
			Factory.Save();

			var createContext = new CreateCommissionContext();
			createContext.OverwriteOldValues = true;
			EDINonJobRelatedTransactionCommissionCreator.New(invoice).CreateCommissions(createContext);

			var newCommissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.PK, SQLComparisonOperator.NotEqual, existingCommissionHeader.PK));
			AssertEquals("Should overriden old commission", true, existingCommissionHeader.IsOverriden);
			AssertEquals("Should not create new commission", 0, newCommissionHeaders.Length);

			var ambiguousCommissions = Factory.Load<AccAmbiguousCommission>(new ZQuery());
			var ambiguousCommission = ambiguousCommissions.Single();
			AssertEquals("Should have flagged invoice as unresolved commission", invoice.PK, ambiguousCommission.AC0_AH_Source);
			AssertEquals("Should have flagged invoice as unresolved commission", ZGuid.Empty, ambiguousCommission.AC0_CA0_SelectedAgreement);
		}

		public void TestCreateCommissions_ChargeCodeDefaultItemsResponsibleByTwoAgreements_AndWithExistingCommissionWhereAgreementIsReversed_AndOverwriteOldValues()
		{
			var chargeCode_XXX_XXX_XXX = NewChargeCodeWithDefaultItem("XXX", "XXX", "XXX");
			var chargeCode_XXX_XXX_YYY = NewChargeCodeWithDefaultItem("XXX", "XXX", "YYY");
			var chargeCode_ZZZ_ALL_ALL = NewChargeCodeWithDefaultItem("ZZZ", "ALL", "ALL");

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = customer.PK;
			AddNewLine(invoice, chargeCode_XXX_XXX_XXX);
			AddNewLine(invoice, chargeCode_XXX_XXX_YYY);
			AddNewLine(invoice, chargeCode_ZZZ_ALL_ALL);

			Factory.Save();

			var opportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, customer);
			var agreement_XXX_XXX_ALL = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunity, customer, "XXX", "XXX", "ALL");
			var recipient = OrgCommissionAgreementTestHelper.AddPercentageRecipient(agreement_XXX_XXX_ALL, "ADL", 10);
			var agreement_ZZZ_ALL_ALL = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunity, customer, "ZZZ", "ALL", "ALL");
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(agreement_ZZZ_ALL_ALL, "ADL", 10);

			var existingCommissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			existingCommissionHeader.CH0_AH_Source = invoice.PK;
			existingCommissionHeader.CH0_GroupingSourceID = invoice.PK;
			existingCommissionHeader.CH0_GroupingSourceTableCode = invoice.TablePrefix;
			existingCommissionHeader.CH0_OH_Customer = customer.PK;
			existingCommissionHeader.CH0_Product = OrgCommissionAgreementItemLookups.AllProductsCode;
			existingCommissionHeader.CH0_Service = OrgCommissionAgreementItemLookups.AllServicesCode;
			existingCommissionHeader.CH0_SubModule = OrgCommissionAgreementItemLookups.AllSubModulesCode;
			existingCommissionHeader.CH0_CA0 = agreement_ZZZ_ALL_ALL.PK;

			var existingLineGroup = existingCommissionHeader.LineGroups.AddNew();
			existingLineGroup.FillWithValidTestData();
			existingLineGroup.CLG_AC = chargeCode_XXX_XXX_XXX.PK;

			var existingLine = existingLineGroup.Lines.AddNew();
			existingLine.FillWithValidTestData();
			existingLine.CL0_GS_NKStaff = recipient.CAR_GS_NKStaff;
			existingLine.CL0_OH_Party = recipient.CAR_OH_Party;
			existingLine.CL0_RX_NKTransactionCurrency = "AUD";

			Factory.Save();

			agreement_ZZZ_ALL_ALL.Reverse();
			Factory.Save();

			var createContext = new CreateCommissionContext();
			createContext.OverwriteOldValues = true;
			EDINonJobRelatedTransactionCommissionCreator.New(invoice).CreateCommissions(createContext);

			var newCommissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.PK, SQLComparisonOperator.NotEqual, existingCommissionHeader.PK));
			AssertEquals("Should overriden old commission and create a new one", true, existingCommissionHeader.IsOverriden);
			AssertEquals("Should overriden old commission and create a new one", 1, newCommissionHeaders.Length);
			AssertEquals("New Commission should be for the other agreement which hasn't been reversed", agreement_XXX_XXX_ALL.PK, newCommissionHeaders[0].CH0_CA0);

			var ambiguousCommissions = Factory.Load<AccAmbiguousCommission>(new ZQuery());
			AssertEquals("Should not have flagged invoice as unresolved commission - new commission created", 0, ambiguousCommissions.Length);
		}

		public void TestCreateCommissions_ChargeCodeDefaultItemsResponsibleByMultipleAgreements_AndWithExistingUnresolvedAmbiguousCommission()
		{
			var chargeCode_XXX_XXX_XXX = NewChargeCodeWithDefaultItem("XXX", "XXX", "XXX");
			var chargeCode_XXX_XXX_YYY = NewChargeCodeWithDefaultItem("XXX", "XXX", "YYY");
			var chargeCode_ZZZ_ALL_ALL = NewChargeCodeWithDefaultItem("ZZZ", "ALL", "ALL");

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = customer.PK;
			AddNewLine(invoice, chargeCode_XXX_XXX_XXX);
			AddNewLine(invoice, chargeCode_XXX_XXX_YYY);
			AddNewLine(invoice, chargeCode_ZZZ_ALL_ALL);

			Factory.Save();

			var opportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, customer);
			var agreement_XXX_XXX_ALL = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunity, customer, "XXX", "XXX", "ALL");
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(agreement_XXX_XXX_ALL, "ADL", 10);
			var agreement_ZZZ_ALL_ALL = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunity, customer, "ZZZ", "ALL", "ALL");
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(agreement_ZZZ_ALL_ALL, "ADL", 10);

			var existingUnresolvedCommission = Factory.New<AccAmbiguousCommission>();
			existingUnresolvedCommission.AC0_AH_Source = invoice.PK;
			existingUnresolvedCommission.AC0_CA0_SelectedAgreement = ZGuid.Empty;

			Factory.Save();

			var createContext = new CreateCommissionContext();
			createContext.OverwriteOldValues = true;
			EDINonJobRelatedTransactionCommissionCreator.New(invoice).CreateCommissions(createContext);

			var commissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery());
			AssertEquals("Should not create commission header - Not all line charge codes have a default item responsible by the same agreement", 0, commissionHeaders.Length);

			var newUnresolvedCommissions = Factory.Load<AccAmbiguousCommission>(new ZQuery(AccAmbiguousCommissionSchema.PK, SQLComparisonOperator.NotEqual, existingUnresolvedCommission.PK));
			AssertEquals("Should not create another unresolved commission - one already exists", 0, newUnresolvedCommissions.Length);
		}

		public void TestCreateCommissions_ChargeCodeDefaultItemsResponsibleByNoAgreements_NoAllAllAllAgreement()
		{
			var chargeCode_XXX_XXX_XXX = NewChargeCodeWithDefaultItem("XXX", "XXX", "XXX");
			var chargeCode_XXX_XXX_YYY = NewChargeCodeWithDefaultItem("XXX", "XXX", "YYY");
			var chargeCode_ZZZ_ALL_ALL = NewChargeCodeWithDefaultItem("ZZZ", "ALL", "ALL");

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = customer.PK;
			AddNewLine(invoice, chargeCode_XXX_XXX_XXX);
			AddNewLine(invoice, chargeCode_XXX_XXX_YYY);
			AddNewLine(invoice, chargeCode_ZZZ_ALL_ALL);

			Factory.Save();

			EDINonJobRelatedTransactionCommissionCreator.New(invoice).CreateCommissions();

			var commissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery());
			AssertEquals("Should not create commission header - No agreement responsible for charge code default items AND no fallback agreement either", 0, commissionHeaders.Length);

			var ambiguousCommissions = Factory.Load<AccAmbiguousCommission>(new ZQuery());
			AssertEquals("Should not have flagged invoice as unresolved commission - there were no responsible agreements", 0, ambiguousCommissions.Length);
		}

		public void TestCreateCommissions_ChargeCodeDefaultItemsResponsibleByNoAgreements_HasAllAllAllAgreement()
		{
			var chargeCode_XXX_XXX_XXX = NewChargeCodeWithDefaultItem("XXX", "XXX", "XXX");
			var chargeCode_XXX_XXX_YYY = NewChargeCodeWithDefaultItem("XXX", "XXX", "YYY");
			var chargeCode_ZZZ_ALL_ALL = NewChargeCodeWithDefaultItem("ZZZ", "ALL", "ALL");

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = customer.PK;
			AddNewLine(invoice, chargeCode_XXX_XXX_XXX);
			AddNewLine(invoice, chargeCode_XXX_XXX_YYY);
			AddNewLine(invoice, chargeCode_ZZZ_ALL_ALL);

			Factory.Save();

			var opportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, customer);
			var agreement_ALL_ALL_ALL = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunity, customer, "ALL", "ALL", "ALL");
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(agreement_ALL_ALL_ALL, "ADL", 10);

			Factory.Save();

			EDINonJobRelatedTransactionCommissionCreator.New(invoice).CreateCommissions();

			var commissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery());
			var commissionHeader = commissionHeaders.Single();
			AssertEquals("Should fall back to all all all agreement when can there are no agreements responsible for the charge code default items", agreement_ALL_ALL_ALL.PK, commissionHeader.CH0_CA0);

			var ambiguousCommissions = Factory.Load<AccAmbiguousCommission>(new ZQuery());
			AssertEquals("Should not have flagged invoice as unresolved commission", 0, ambiguousCommissions.Length);
		}

		public void TestCreateCommissions_WithAgreementAndRatesOverride()
		{
			var chargeCode_XXX_XXX_XXX = NewChargeCodeWithDefaultItem("XXX", "XXX", "XXX");
			var chargeCode_XXX_XXX_YYY = NewChargeCodeWithDefaultItem("XXX", "XXX", "YYY");
			var chargeCode_ZZZ_ALL_ALL = NewChargeCodeWithDefaultItem("ZZZ", "ALL", "ALL");

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = customer.PK;
			AddNewLine(invoice, chargeCode_XXX_XXX_XXX);
			AddNewLine(invoice, chargeCode_XXX_XXX_YYY);
			AddNewLine(invoice, chargeCode_ZZZ_ALL_ALL);

			Factory.Save();

			var opportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, customer);
			var agreement_XXX_XXX_ALL = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunity, customer, "XXX", "XXX", "ALL");
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(agreement_XXX_XXX_ALL, "ADL", 10);
			var agreement_ZZZ_ALL_ALL = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunity, customer, "ZZZ", "ALL", "ALL");
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(agreement_ZZZ_ALL_ALL, "ADL", 10);

			Factory.Save();

			var createContext = new CreateCommissionContext();
			createContext.AgreementAndRatesOverride = new Dictionary<ZString, ICommissionAgreementAndRates>() { { ZString.Empty, new CommissionAgreementAndRates(agreement_XXX_XXX_ALL, invoice.AH_PostDate.Date) } };
			EDINonJobRelatedTransactionCommissionCreator.New(invoice).CreateCommissions(createContext);

			var commissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery());
			AssertEquals("Should create commission header even though not all line charge codes have a default item responsible by the same agreement", 1, commissionHeaders.Length);

			var ambiguousCommissions = Factory.Load<AccAmbiguousCommission>(new ZQuery());
			AssertEquals("Should not have flagged invoice as unresolved commission", 0, ambiguousCommissions.Length);
		}

		AccChargeCode NewChargeCodeWithDefaultItem(ZString product, ZString service, ZString subModule)
		{
			var result = Factory.NewWithValidTestData<AccChargeCode>();
			result.AC_IsCommissionable = true;
			result.AC_DefaultCommissionProduct = product;
			result.AC_DefaultCommissionService = service;
			result.AC_DefaultCommissionSubModule = subModule;

			return result;
		}

		InvoicingLineBase AddNewLine(ARInvoice invoice, AccChargeCode chargeCode)
		{
			return TestObjectCreator.CreateARInvoiceLine(invoice, null, chargeCode, null, 1, "", 1);
		}
	}
}
