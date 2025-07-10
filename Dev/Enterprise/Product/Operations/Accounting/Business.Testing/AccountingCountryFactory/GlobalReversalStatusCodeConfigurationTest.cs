using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing.AccountingCountryFactory
{
	public class GlobalReversalStatusCodeConfigurationTest : TestCaseWithFactory
	{
		public void TestGetIsReversalStatusCodeAllowed_ReturnsFalse_WhenLedgerIsNotAccountsReceivable()
		{
			var codes = new CodeDescriptionPairList();
			codes.AddPair("01", "First test value");

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EInvoicingReversalCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, codes))
			{
				var globalReversalStatusCodeConfiguration = new GlobalReversalStatusCodeConfiguration();
				AssertEquals(false, globalReversalStatusCodeConfiguration.GetIsReversalStatusCodeAllowed("AP"));
			}
		}

		public void TestGetIsReversalStatusCodeAllowed_ReturnsFalse_WhenEnableEInvoicingFunctionalityIsDisabled()
		{
			var codes = new CodeDescriptionPairList();
			codes.AddPair("01", "First test value");

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.EInvoicingReversalCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, codes))
			{
				var globalReversalStatusCodeConfiguration = new GlobalReversalStatusCodeConfiguration();
				AssertEquals(false, globalReversalStatusCodeConfiguration.GetIsReversalStatusCodeAllowed("AR"));
			}
		}

		public void TestGetIsReversalStatusCodeAllowed_ReturnsFalse_WhenReversalCodesRegistryIsEmpty()
		{
			var codes = new CodeDescriptionPairList();

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EInvoicingReversalCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, codes))
			{
				var globalReversalStatusCodeConfiguration = new GlobalReversalStatusCodeConfiguration();
				AssertEquals(false, globalReversalStatusCodeConfiguration.GetIsReversalStatusCodeAllowed("AR"));
			}
		}

		public void TestGetIsReversalStatusCodeAllowed_ReturnsTrue_WhenAllConditionsAreMet()
		{
			var codes = new CodeDescriptionPairList();
			codes.AddPair("01", "First test value");

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EInvoicingReversalCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, codes))
			{
				var globalReversalStatusCodeConfiguration = new GlobalReversalStatusCodeConfiguration();
				AssertEquals(true, globalReversalStatusCodeConfiguration.GetIsReversalStatusCodeAllowed("AR"));
			}
		}

		public void TestGetReversalStatusCodeReferenceType_ReturnsEINV_REVERSAL_CODE()
		{
			var globalReversalStatusCodeConfiguration = new GlobalReversalStatusCodeConfiguration();
			AssertEquals("ERC", globalReversalStatusCodeConfiguration.GetReversalStatusCodeReferenceType());
		}

		public void TestGetReversalStatusCodeLookup_ReturnsEmptyList_WhenNoValuesAreStored()
		{
			var codes = new CodeDescriptionPairList();

			using (AccountingMasterFilesRegistry.Instance.EInvoicingReversalCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, codes))
			{
				var globalReversalStatusCodeConfiguration = new GlobalReversalStatusCodeConfiguration();
				var result = globalReversalStatusCodeConfiguration.GetReversalStatusCodeLookup();

				AssertEquals(0, result.Count);
			}
		}

		public void TestGetReversalStatusCodeLookup_ReturnsStoredValues()
		{
			var codes = new CodeDescriptionPairList();
			codes.AddPair("01", "First test value");
			codes.AddPair("02", "Second test value");

			using (AccountingMasterFilesRegistry.Instance.EInvoicingReversalCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, codes))
			{
				var globalReversalStatusCodeConfiguration = new GlobalReversalStatusCodeConfiguration();

				AssertContainsExactElementsInAnyOrder(codes, globalReversalStatusCodeConfiguration.GetReversalStatusCodeLookup());
			}
		}
	}
}
