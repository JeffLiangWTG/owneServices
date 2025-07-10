using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ARComplianceDocumentFilterStripBusinessObject))]
	public class ARComplianceDocumentFilterStripBusinessObjectTest : ComplianceDocumentFilterStripBusinessObjectTest
	{
		protected override ZString LedgerType => LedgerTypes.AccountsReceivable;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ARComplianceDocumentFilterStripBusinessObject();
		}

		protected override AccComplianceDocumentHeader CreateNewComplianceDocumentHeader(BusinessObjectFactory factory)
		{
			return Factory.NewWithValidTestData<ARComplianceDocumentHeader>();
		}

		#region Test EInvoicing Pivot Filters

		public void TestEInvoicingPivotStatusVisibilityWhenEInvoicingFunctionalityDisabled()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				AssertEInvoicingPivotStatusVisibility(false);
			}
		}

		public void TestEInvoicingPivotStatusVisibilityWhenEInvoicingFunctionalityEnabled()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertEInvoicingPivotStatusVisibility(true);
			}
		}

		void AssertEInvoicingPivotStatusVisibility(bool shouldBeVisible)
		{
			var eInvoicingFilter = "EInvoicing Pivot Status";

			var filter = TestFilterBizO[eInvoicingFilter];
			if (shouldBeVisible)
			{
				AssertNotNull("Filter should be available", filter);
			}
			else
			{
				AssertNull("Filter should not be available", filter);
			}
		}

		public void TestEInvoicingPivotStatus()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var eInvoicingStatus = (ModuleTextFilter)TestFilterBizO["EInvoicing Pivot Status"];
				eInvoicingStatus.Property = EInvoicingPivotState.Queued;
				eInvoicingStatus.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				eInvoicingStatus.IsActive = true;

				var collection = new AccComplianceDocumentHeaderCollection(Factory, TestFilterBizO.Filter);
				collection.Load();

				AssertEquals("There should be 0 compliance document in the collection", 0, collection.Count);

				SetupEInvoicingData();

				collection.Load();

				AssertEquals("There should be 1 compliance document in the collection", 1, collection.Count);

				void SetupEInvoicingData()
				{
					var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.TWD, 1.0m, TestObjectCreator.Debtor);
					var invoiceLine = TestObjectCreator.CreateARInvoiceLine(invoice, null, TestObjectCreator.FRT, TestObjectCreator.TWD, 1.0m, "desc", 10m);
					invoiceLine.AL_AT = TestObjectCreator.GST1.PK;
					Factory.Save();

					new ComplianceDocumentCreator(new[] { invoice }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();

					var header = Factory.Load<ARComplianceDocumentHeader>(new ZQuery(AccComplianceDocumentHeaderSchema.ADH_OH_Organisation, TestObjectCreator.Debtor.PK)).First();
					header.ADH_DocumentNumber = "AA00000001";
					header.ADH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE;
					header.ADH_DocumentStatus = ComplianceDocumentStatus.NumberSet;
					Factory.Save();
				}
			}
		}

		#endregion

		#region Test EInvoicing Batch Filters

		public void TestEInvoicingBatchStatusVisibilityWhenEInvoicingFunctionalityDisabled()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				AssertEInvoicingBatchStatusVisibility(false);
			}
		}

		public void TestEInvoicingBatchStatusVisibilityWhenEInvoicingFunctionalityEnabled()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertEInvoicingBatchStatusVisibility(true);
			}
		}

		void AssertEInvoicingBatchStatusVisibility(bool shouldBeVisible)
		{
			var eInvoicingFilter = "EInvoicing Batch Status";

			var filter = TestFilterBizO[eInvoicingFilter];
			if (shouldBeVisible)
			{
				AssertNotNull("Filter should be available", filter);
			}
			else
			{
				AssertNull("Filter should not be available", filter);
			}
		}

		public void TestEInvoicingBatchStatus()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var eInvoicingStatus = (ModuleTextFilter)TestFilterBizO["EInvoicing Batch Status"];
				eInvoicingStatus.Property = EInvoicingBatchState.Ready;
				eInvoicingStatus.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				eInvoicingStatus.IsActive = true;

				var collection = new AccComplianceDocumentHeaderCollection(Factory, TestFilterBizO.Filter);
				collection.Load();

				AssertEquals("There should be 0 compliance document in the collection", 0, collection.Count);

				SetupEInvoicingData();

				collection.Load();

				AssertEquals("There should be 1 compliance document in the collection", 1, collection.Count);

				void SetupEInvoicingData()
				{
					var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.TWD, 1.0m, TestObjectCreator.Debtor);
					var invoiceLine = TestObjectCreator.CreateARInvoiceLine(invoice, null, TestObjectCreator.FRT, TestObjectCreator.TWD, 1.0m, "desc", 10m);
					invoiceLine.AL_AT = TestObjectCreator.GST1.PK;
					Factory.Save();

					new ComplianceDocumentCreator(new[] { invoice }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();

					var header = Factory.Load<ARComplianceDocumentHeader>(new ZQuery(AccComplianceDocumentHeaderSchema.ADH_OH_Organisation, TestObjectCreator.Debtor.PK)).First();
					header.ADH_DocumentNumber = "AA00000001";
					header.ADH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE;
					header.ADH_DocumentStatus = ComplianceDocumentStatus.NumberSet;
					Factory.Save();

					var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
					batch.AIB_GC = GlbCompany.CurrentCompany.PK;
					batch.AIB_Status = EInvoicingBatchState.Ready;

					var pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, header.PK));
					pivot.AIP_AIB = batch.PK;
					Factory.Save();
				}
			}
		}

		#endregion

		public void TestVoidReasonFilter()
		{
			var eInvoicingVoidingReason = (ModuleTextFilter)TestFilterBizO["Void Reason"];
			eInvoicingVoidingReason.Property = "987";
			eInvoicingVoidingReason.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			eInvoicingVoidingReason.IsActive = true;

			var startWithCollection = new AccComplianceDocumentHeaderCollection(Factory, TestFilterBizO.Filter);
			startWithCollection.Load();

			AssertEquals("There should be 1 compliance document in the collection", 1, startWithCollection.Count);

			eInvoicingVoidingReason.SqlComparisonOperator = SQLComparisonOperator.Contains;
			eInvoicingVoidingReason.Property = "54";

			var containsCollection = new AccComplianceDocumentHeaderCollection(Factory, TestFilterBizO.Filter);
			containsCollection.Load();

			AssertEquals("There should be 1 compliance document in the collection", 1, containsCollection.Count);

			eInvoicingVoidingReason.SqlComparisonOperator = SQLComparisonOperator.Equal;
			eInvoicingVoidingReason.Property = "987654321";

			var equalCollection = new AccComplianceDocumentHeaderCollection(Factory, TestFilterBizO.Filter);
			equalCollection.Load();

			AssertEquals("There should be 1 compliance document in the collection", 1, equalCollection.Count);
		}

		public void TestVoidApprovalFilter()
		{
			var approvalNumberFilter = (ModuleNumberFilter)TestFilterBizO["Void Approval Number"];
			approvalNumberFilter.Property = "987";
			approvalNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			approvalNumberFilter.IsActive = true;

			var startWithCollection = new AccComplianceDocumentHeaderCollection(Factory, TestFilterBizO.Filter);
			startWithCollection.Load();

			AssertEquals("There should be 1 compliance document in the collection", 1, startWithCollection.Count);

			approvalNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			approvalNumberFilter.Property = "54";

			var containsCollection = new AccComplianceDocumentHeaderCollection(Factory, TestFilterBizO.Filter);
			containsCollection.Load();

			AssertEquals("There should be 1 compliance document in the collection", 1, containsCollection.Count);

			approvalNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			approvalNumberFilter.Property = "987654321";

			var equalCollection = new AccComplianceDocumentHeaderCollection(Factory, TestFilterBizO.Filter);
			equalCollection.Load();

			AssertEquals("There should be 1 compliance document in the collection", 1, equalCollection.Count);
		}

		public void TestComplianceDocumentStatus()
		{
			var filterStripBusinessObject = GetNewFilterStripBusinessObject() as ARComplianceDocumentFilterStripBusinessObject;
			AssertCodeDescriptionPairList(filterStripBusinessObject.ComplianceStatusList,
				("ADD", "Document Record Added"),
				("VOD", "Document Record Voided"),
				("SET", "Document Number Set"),
				("FIN", "Document Record Finalized"),
				("VAF", "Document Record Special Voided After Finalized")
			);
		}
	}
}
