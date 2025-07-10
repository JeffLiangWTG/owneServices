using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.CommissionManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.CommissionManagement.Business.Test
{
	[TestedType(typeof(AmbiguousCommissionResolver))]
	internal class AmbiguousCommissionResolverTest : NonPersistentBusinessObjectTestCase
	{
		#region Resolve

		public void TestResolve_Unresolved()
		{
			var agreementA = CreateOrgCommissionAgreement();
			var agreementB = CreateOrgCommissionAgreement();

			var unresolvedCommission1 = CreateAccAmbiguousCommission(null);
			var unresolvedCommission2 = CreateAccAmbiguousCommission(null);
			var unresolvedCommission3 = CreateAccAmbiguousCommission(null);
			var unresolvedCommission4 = CreateAccAmbiguousCommission(null);
			var resolvedCommission1 = CreateAccAmbiguousCommission(null);
			resolvedCommission1.AC0_CA0_SelectedAgreement = agreementA.PK;
			var commissionHeaderForPreviouslyResolved = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeaderForPreviouslyResolved.CH0_AH_Source = resolvedCommission1.AC0_AH_Source;

			Factory.Save();

			var filterBizObj = new AmbiguousCommissionResolverFilterBusinessObject(Factory);
			filterBizObj.IsResolvingUnresolved = true;
			var resolver = new AmbiguousCommissionResolver(filterBizObj);
			resolver.LoadResolveItems();
			AssertEquals(4, resolver.ResolveItemCollection.Count);

			var unresolvedItem1 = resolver.ResolveItems.First(x => x.AmbiguousCommission.PK == unresolvedCommission1.PK);
			var unresolvedItem2 = resolver.ResolveItems.First(x => x.AmbiguousCommission.PK == unresolvedCommission2.PK);
			var unresolvedItem3 = resolver.ResolveItems.First(x => x.AmbiguousCommission.PK == unresolvedCommission3.PK);
			var unresolvedItem4 = resolver.ResolveItems.First(x => x.AmbiguousCommission.PK == unresolvedCommission4.PK);
			unresolvedItem1.SelectedAgreementPk = agreementA.PK;
			unresolvedItem2.SelectedAgreementPk = agreementA.PK;
			unresolvedItem3.SelectedAgreementPk = agreementB.PK;
			unresolvedItem4.SelectedAgreementPk = ZGuid.Empty;

			resolver.Resolve(null);

			AssertEquals(1, resolver.ResolveItemCollection.Count);

			AssertEquals(agreementA.PK, unresolvedCommission1.AC0_CA0_SelectedAgreement);
			AssertEquals(agreementA.PK, unresolvedCommission2.AC0_CA0_SelectedAgreement);
			AssertEquals(agreementB.PK, unresolvedCommission3.AC0_CA0_SelectedAgreement);
			AssertEquals(ZGuid.Empty, unresolvedCommission4.AC0_CA0_SelectedAgreement);
			AssertEquals(agreementA.PK, resolvedCommission1.AC0_CA0_SelectedAgreement);
		}

		public void TestResolve_Filtered()
		{
			var agreement = CreateOrgCommissionAgreement();
			var resolvedCommission = CreateAccAmbiguousCommission(agreement);
			var commissionHeaderForPreviouslyResolved = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeaderForPreviouslyResolved.CH0_AH_Source = resolvedCommission.AC0_AH_Source;
			commissionHeaderForPreviouslyResolved.CH0_GroupingSourceID = resolvedCommission.AC0_AH_Source;
			commissionHeaderForPreviouslyResolved.CH0_GroupingSourceTableCode = resolvedCommission.Source.TablePrefix;
			commissionHeaderForPreviouslyResolved.CH0_CommissionStream = "WBP";
			commissionHeaderForPreviouslyResolved.CH0_OH_Customer = resolvedCommission.Source.AH_OH;
			commissionHeaderForPreviouslyResolved.CH0_Product = OrgCommissionAgreementItemLookups.AllProductsCode;
			commissionHeaderForPreviouslyResolved.CH0_Service = OrgCommissionAgreementItemLookups.AllServicesCode;
			commissionHeaderForPreviouslyResolved.CH0_SubModule = OrgCommissionAgreementItemLookups.AllSubModulesCode;

			Factory.Save();

			var filterBizObj = new AmbiguousCommissionResolverFilterBusinessObject(Factory);
			filterBizObj.IsResolvingUnresolved = false;
			filterBizObj.IsResolvingFiltered = true;
			filterBizObj.CompanyPk = GlbCompany.CurrentCompany.PK;
			filterBizObj.InvoicePkToResolve = resolvedCommission.AC0_AH_Source;
			var resolver = new AmbiguousCommissionResolver(filterBizObj);
			resolver.LoadResolveItems();
			AssertEquals(1, resolver.ResolveItemCollection.Count);

			var item = resolver.ResolveItems.Single();
			item.SelectedAgreementPk = ZGuid.Empty;

			resolver.Resolve(null);

			AssertEquals(1, resolver.ResolveItemCollection.Count);

			AssertEquals(ZGuid.Empty, resolvedCommission.AC0_CA0_SelectedAgreement);
			AssertEquals(true, commissionHeaderForPreviouslyResolved.IsOverriden);
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

		TestObjectCreator TestObjectCreator;
		AccChargeCode ChargeCode;
		OrgHeader Customer;
		OrgOpportunity Opportunity;

		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory);
			ChargeCode = NewChargeCodeWithDefaultItem("ALL", "ALL", "ALL");
			Customer = Factory.NewWithValidTestData<OrgHeader>();
			Opportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, Customer);
		}

		AccAmbiguousCommission CreateAccAmbiguousCommission(OrgCommissionAgreement agreement)
		{
			var commission = Factory.NewWithValidTestData<AccAmbiguousCommission>();
			TestObjectCreator.CreateARInvoiceLine(commission.Source as ARInvoice, null, ChargeCode, null, 1, "", 1);

			commission.AC0_CA0_SelectedAgreement = agreement?.PK ?? ZGuid.Empty;
			commission.AC0_CommissionStream = agreement?.CA0_CommissionStream ?? "WBP";
			commission.Source.AH_OH = Customer.PK;

			return commission;
		}

		OrgCommissionAgreement CreateOrgCommissionAgreement()
		{
			var agreement = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(Opportunity, Customer, "ALL", "ALL", "ALL");
			agreement.CA0_CommissionStream = "WBP";
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(agreement, "ADL", 10);
			return agreement;
		}

		#endregion

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			var filterBizObj = new AmbiguousCommissionResolverFilterBusinessObject(Factory);
			return new AmbiguousCommissionResolver(filterBizObj);
		}

		#endregion
	}
}
