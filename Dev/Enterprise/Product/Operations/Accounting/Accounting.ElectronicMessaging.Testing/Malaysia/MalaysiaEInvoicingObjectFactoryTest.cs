using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using Enterprise.Accounting.ElectronicMessaging.Malaysia;
using Enterprise.Accounting.Export.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Testing.Malaysia
{
	[TestedType(typeof(MalaysiaEInvoicingObjectFactory))]
	class MalaysiaEInvoicingObjectFactoryTest : CountryEInvoicingObjectFactoryTest
	{
		public void TestGetPivotStatus_WhenEnableEInvoicingFunctionality()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Malaysia))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			{
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", GlbCompany.CurrentCompany.LocalCurrency, 100m, TestObjectCreator.Debtor, "03");
				TestObjectCreator.CreateInvoiceLine(invoice, 110m, invoice.TransactionCurrency, 1m);

				Factory.Save();

				var pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice.PK));
				AssertEquals("Status should be QUE.", EInvoicingPivotState.Queued, pivot.AIP_Status);
			}
		}

		public void TestGetPivotStatus_WhenDisableEInvoicingFunctionality()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Malaysia))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			{
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", GlbCompany.CurrentCompany.LocalCurrency, 100m, TestObjectCreator.Debtor, "01");
				TestObjectCreator.CreateInvoiceLine(invoice, 110m, invoice.TransactionCurrency, 1m);

				Factory.Save();

				var pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice.PK));
				AssertEquals("Status should be DCD.", EInvoicingPivotState.Discarded, pivot.AIP_Status);
			}
		}

		public void TestUpdateGEIBatchRequest()
		{
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV0001", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m, TestObjectCreator.AALSHI, TestObjectCreator.CC1.PK);
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice);
			var eInvoicingBatch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 10, EInvoicingBatchState.Ready);

			var countryFactory = (ICountryEInvoicingObjectFactory)GetTestCountryFactory();
			var batchRequest = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest();

			AssertEquals("Precondition: batchRequest.MessageType is empty", string.Empty, batchRequest.MessageType);

			countryFactory.UpdateGEIBatchRequest(eInvoicingBatch, batchRequest);

			AssertEquals(EInvoicingPivotActionType.Submit, pivot.AIP_ActionType);
			AssertEquals(MalaysiaEInvoiceAPICommandList.Codes.SubmitTransaction, batchRequest.MessageType);
			AssertEquals(ZString.Empty, batchRequest.GovernmentAllocatedNumber);
		}

		public void TestUpdateGEIBatchRequest_HasGovernmentAllocatedNumber()
		{
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV0001", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m, TestObjectCreator.AALSHI, TestObjectCreator.CC1.PK);
			var submitPivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice, EInvoicingPivotActionType.Submit, EInvoicingPivotState.Delivered);
			var submitBatch = TestObjectCreator.CreateEInvoicingBatchForPivot(submitPivot, 1, EInvoicingBatchState.Sent);
			submitBatch.AIB_GovernmentAllocatedNumber = "123";

			Factory.Save();

			var getsubmissionPivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice, EInvoicingPivotActionType.StatusCheck, EInvoicingPivotState.Queued);
			var getsubmissionBatch = TestObjectCreator.CreateEInvoicingBatchForPivot(getsubmissionPivot, 2, EInvoicingBatchState.Sent);

			var countryFactory = (ICountryEInvoicingObjectFactory)GetTestCountryFactory();
			var batchRequest = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest();

			countryFactory.UpdateGEIBatchRequest(getsubmissionBatch, batchRequest);

			AssertEquals(MalaysiaEInvoiceAPICommandList.Codes.GetSubmission, batchRequest.MessageType);
			AssertEquals(submitBatch.AIB_GovernmentAllocatedNumber, batchRequest.GovernmentAllocatedNumber);
		}

		public void TestGEIHasGovernmentAllocatedID_WhenGetDocument()
		{
			AssertGEIHasGovernmentAllocatedID(EInvoicingPivotActionType.DocumentAction);
		}

		public void TestGEIHasGovernmentAllocatedID_WhenGetDocumentDetail()
		{
			AssertGEIHasGovernmentAllocatedID(EInvoicingPivotActionType.DocumentDetail);
		}

		void AssertGEIHasGovernmentAllocatedID(string actionType)
		{
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV0001", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m, TestObjectCreator.AALSHI, TestObjectCreator.CC1.PK);
			arInvoice.AH_GovernmentAllocatedID = "F9D425P6DS7D8IU";
			var submitPivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice, EInvoicingPivotActionType.Submit, EInvoicingPivotState.Delivered);
			var submitBatch = TestObjectCreator.CreateEInvoicingBatchForPivot(submitPivot, 1, EInvoicingBatchState.Sent);

			var queryPivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice, actionType, EInvoicingPivotState.Batched);
			var queryBatch = TestObjectCreator.CreateEInvoicingBatchForPivot(queryPivot, 2, EInvoicingBatchState.Sent);

			Factory.Save();

			using (TestObjectCreator.SetTemporaryControlAccounts())
			{
				var dataAccess = new BatchExportDataAccess(((IDbConnectionInternals)TestConnection).ADOConnection, ((IDbConnectionInternals)TestConnection).ADOTransaction);
				var transactionBatch = new TransactionBatchExporter(dataAccess, GEIMessagePopulateOptionalXUTFieldsSetting).CreateTransactionBatch(queryBatch);
				var transactionInfo = transactionBatch.TransactionCollection[0];

				AssertEquals(arInvoice.AH_GovernmentAllocatedID, transactionInfo.GovernmentAllocatedID);
			}
		}

		#region override

		protected override Type GetExpectedCredentialsLoaderType() => typeof(MalaysiaCredentialLoader);

		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new MalaysiaEInvoicingObjectFactory();

		protected override IncludeUniversalTransactionStrategy GEIMessageShouldIncludeUniversalTransaction
			=> IncludeUniversalTransactionStrategy.SingleTransaction;

		protected override Type GetExpectedAdditionalDataItemsProviderType() => typeof(MalaysiaEInvoicingAdditionalDataItemsProvider);

		protected override Type GetExpectedEInvoicingDataValidatorType() => typeof(EInvoicingDataValidatorForMalaysia);

		protected override Type GetExpectedGlobalXUEFunctionalityProviderInterfaceType() => typeof(MalaysiaGlobalXUEFunctionalityProvider);

		#endregion

		TestObjectCreator TestObjectCreator => testObjectCreator = testObjectCreator ?? new TestObjectCreator(Factory);
		TestObjectCreator testObjectCreator;
	}
}
