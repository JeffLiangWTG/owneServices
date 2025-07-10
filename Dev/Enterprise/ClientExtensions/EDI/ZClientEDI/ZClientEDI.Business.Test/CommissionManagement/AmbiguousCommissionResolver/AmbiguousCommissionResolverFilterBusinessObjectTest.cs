using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.CommissionManagement.Business.Test
{
	[TestedType(typeof(AmbiguousCommissionResolverFilterBusinessObject))]
	internal class AmbiguousCommissionResolverFilterBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		#region Properties

		public void TestPropertyMaxLength()
		{
			AssertEquals(AccTransactionHeader.Schema.AH_TransactionNumMaxLength, AmbiguousCommissionResolverFilterBusinessObject.Schema.InvoiceNumberToResolveMaxLength);
		}

		#endregion

		#region Default Values

		public void TestDefaultValues()
		{
			var filter = new AmbiguousCommissionResolverFilterBusinessObject(Factory);
			AssertEquals(true, filter.IsResolvingUnresolved);
			AssertEquals(false, filter.IsResolvingFiltered);
			AssertEquals(GlbCompany.CurrentCompany.PK, filter.CompanyPk);
		}

		#endregion

		#region Filter

		public void TestFilter()
		{
			var agreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			var otherBranch = otherCompany.Branches.AddNew();

			var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			var invoice2 = Factory.NewWithValidTestData<ARInvoice>();
			var invoice3 = Factory.NewWithValidTestData<ARInvoice>();
			var invoice4 = Factory.NewWithValidTestData<ARInvoice>();
			invoice4.AH_GC = otherCompany.PK;
			invoice4.AH_GB = otherBranch.PK;
			invoice1.AH_TransactionNum = "AAAA";
			invoice4.AH_TransactionNum = "AAAA";
			invoice1.IsManuallySetTransactionNumber_ForTestOnly = true;
			invoice4.IsManuallySetTransactionNumber_ForTestOnly = true;

			Factory.Save();

			var unresolvedCommission1 = Factory.New<AccAmbiguousCommission>();
			unresolvedCommission1.AC0_AH_Source = invoice1.PK;

			var unresolvedCommission2 = Factory.New<AccAmbiguousCommission>();
			unresolvedCommission2.AC0_AH_Source = invoice2.PK;

			var resolvedCommission1 = Factory.New<AccAmbiguousCommission>();
			resolvedCommission1.AC0_AH_Source = invoice3.PK;
			resolvedCommission1.AC0_CA0_SelectedAgreement = agreement.PK;

			var resolvedCommission2 = Factory.New<AccAmbiguousCommission>();
			resolvedCommission2.AC0_AH_Source = invoice4.PK;
			resolvedCommission2.AC0_CA0_SelectedAgreement = agreement.PK;

			Factory.Save();

			var filterBizObj = new AmbiguousCommissionResolverFilterBusinessObject(Factory);
			filterBizObj.IsResolvingFiltered = false;
			filterBizObj.IsResolvingUnresolved = true;
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					unresolvedCommission1,
					unresolvedCommission2
				},
				Factory.Load<AccAmbiguousCommission>(filterBizObj.Filter));

			filterBizObj.IsResolvingUnresolved = false;
			filterBizObj.IsResolvingFiltered = true;
			filterBizObj.CompanyPk = GlbCompany.CurrentCompany.PK;
			filterBizObj.InvoicePkToResolve = invoice3.PK;
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					resolvedCommission1
				},
				Factory.Load<AccAmbiguousCommission>(filterBizObj.Filter));

			filterBizObj.IsResolvingUnresolved = false;
			filterBizObj.IsResolvingFiltered = true;
			filterBizObj.CompanyPk = otherCompany.PK;
			filterBizObj.InvoiceNumberToResolve = "AAAA";
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					resolvedCommission2
				},
				Factory.Load<AccAmbiguousCommission>(filterBizObj.Filter));
		}

		#endregion
	}
}
