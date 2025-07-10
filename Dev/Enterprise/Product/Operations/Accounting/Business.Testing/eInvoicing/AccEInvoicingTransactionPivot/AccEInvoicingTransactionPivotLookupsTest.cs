using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.EInvoicing.Testing
{
	internal class AccEInvoicingTransactionPivotLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestActionTypeList()
		{
			AssertNotNull(Lookups.ActionTypeList);
			AssertEquals(4, Lookups.ActionTypeList.Count);
			Assert(Lookups.ActionTypeList.ContainsCode(EInvoicingPivotActionType.Submit));
			Assert(Lookups.ActionTypeList.ContainsCode(EInvoicingPivotActionType.StatusCheck));
			Assert(Lookups.ActionTypeList.ContainsCode(EInvoicingPivotActionType.DocumentAction));
			Assert(Lookups.ActionTypeList.ContainsCode(EInvoicingPivotActionType.Cancel));
		}

		public void TestStatusList()
		{
			AssertNotNull(Lookups.StatusList);
			AssertEquals(12, Lookups.StatusList.Count);
			Assert(Lookups.StatusList.ContainsCode(EInvoicingPivotState.Queued));
			Assert(Lookups.StatusList.ContainsCode(EInvoicingPivotState.Batched));
			Assert(Lookups.StatusList.ContainsCode(EInvoicingPivotState.BatchedWithError));
			Assert(Lookups.StatusList.ContainsCode(EInvoicingPivotState.Sent));
			Assert(Lookups.StatusList.ContainsCode(EInvoicingPivotState.Delivered));
			Assert(Lookups.StatusList.ContainsCode(EInvoicingPivotState.Succeed));
			Assert(Lookups.StatusList.ContainsCode(EInvoicingPivotState.Failed));
			Assert(Lookups.StatusList.ContainsCode(EInvoicingPivotState.Discarded));
			Assert(Lookups.StatusList.ContainsCode(EInvoicingPivotState.Pending));
			Assert(Lookups.StatusList.ContainsCode(EInvoicingPivotState.AwaitingReview));
			Assert(Lookups.StatusList.ContainsCode(EInvoicingPivotState.InProcessing));
			Assert(Lookups.StatusList.ContainsCode(EInvoicingPivotState.NotEligible));
		}

		public void TestPivotStatusList()
		{
			AssertNotNull(AccEInvoicingTransactionPivotLookups.PivotStatusList);
			AssertContainsExactElementsInAnyOrder(Lookups.StatusList, AccEInvoicingTransactionPivotLookups.PivotStatusList);

			var countryCodeList = Country.LicenceKeyBuilderSupportedCountryCodes;
			var pendingDescriptionRegistryItem = AccountingMasterFilesRegistry.Instance.EReportingPivotPendingStatusDescription;
			foreach (var countryCode in countryCodeList)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					var currentPendingDescription = AccEInvoicingTransactionPivotLookups.PivotStatusList[EInvoicingPivotState.Pending].Description;
					var expectedPendingDescription = string.IsNullOrWhiteSpace(pendingDescriptionRegistryItem.Value) ? "Pending user action" : pendingDescriptionRegistryItem.Value;
					AssertEquals($"Test Pending Description for country {countryCode}", currentPendingDescription, expectedPendingDescription);
				}
			}
			var temporaryPendingDesciption = "PEN Temporary Description";
			using (pendingDescriptionRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, temporaryPendingDesciption))
			{
				AssertEquals(temporaryPendingDesciption, AccEInvoicingTransactionPivotLookups.PivotStatusList[EInvoicingPivotState.Pending].Description);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			var pivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			Lookups = new AccEInvoicingTransactionPivotLookups(pivot);
		}

		AccEInvoicingTransactionPivotLookups Lookups;

		#endregion
	}
}
