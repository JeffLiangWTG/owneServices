using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module.Testing
{
	public class ComplianceDocumentFilterStripControlTest : TestCaseWithFactory
	{
		public void TestColumnsAddedToSubGridCorrectly()
		{
			AssertColumnsAddedToSubGridCorrectly("Charge");
			AssertColumnsAddedToSubGridCorrectly("ADL_Description");
			AssertColumnsAddedToSubGridCorrectly("Currency");
			AssertColumnsAddedToSubGridCorrectly("Amount");
			AssertColumnsAddedToSubGridCorrectly("TaxAmount");
			AssertColumnsAddedToSubGridCorrectly("TotalAmount");
			AssertColumnsAddedToSubGridCorrectly("LocalAmount");
			AssertColumnsAddedToSubGridCorrectly("LocalTaxAmount");
			AssertColumnsAddedToSubGridCorrectly("LocalTotalAmount");
		}

		public void TestColumnsAddToMainGridCorrectly()
		{
			AssertColumnsAddedToMainGridCorrectly("ADH_ComplianceSubType");
			AssertColumnsAddedToMainGridCorrectly("ADH_XD_ComplianceBook");
			AssertColumnsAddedToMainGridCorrectly("ADH_DocumentNumber");
			AssertColumnsAddedToMainGridCorrectly("ADH_DocumentDate");
			AssertColumnsAddedToMainGridCorrectly("ADH_ReportingPeriod");
			AssertColumnsAddedToMainGridCorrectly("Amount");
			AssertColumnsAddedToMainGridCorrectly("TaxAmount");
			AssertColumnsAddedToMainGridCorrectly("TotalAmount");
			AssertColumnsAddedToMainGridCorrectly("OrgHeaderName");
			AssertColumnsAddedToMainGridCorrectly("OrgAddressCompanyName");
			AssertColumnsAddedToMainGridCorrectly("OrgHeaderCategory");
			AssertColumnsAddedToMainGridCorrectly("ADH_OA_AddressOverride");
			AssertColumnsAddedToMainGridCorrectly("ADH_OC_ContactOverride");
			AssertColumnsAddedToMainGridCorrectly("VATRegistrationNum");
			AssertColumnsAddedToMainGridCorrectly("ADH_PrintCount");
			AssertColumnsAddedToMainGridCorrectly("ADH_DocumentStatus");
			AssertColumnsAddedToMainGridCorrectly("ADH_CustomRelated");
			AssertColumnsAddedToMainGridCorrectly("ADH_SupportingReason");
			AssertColumnsAddedToMainGridCorrectly("ADH_SupportingDocumentType");
			AssertColumnsAddedToMainGridCorrectly("ADH_SupportingDocumentNumber");
			AssertColumnsAddedToMainGridCorrectly("ADH_TransactionType");
			AssertColumnsAddedToMainGridCorrectly("ADH_InternalReference");
			AssertColumnsAddedToMainGridCorrectly("IsSpecialVoiding");
			AssertColumnsAddedToMainGridCorrectly("ADH_VoidingReason");
			AssertColumnsAddedToMainGridCorrectly("ADH_ApprovalNumber");
		}

		public void TestColumnsRelevantToSpecialVoidNotAddToMainGridForAPModule()
		{
			AssertColumnsNotAddedToSubGridCorrectly("IsSpecialVoiding");
			AssertColumnsNotAddedToSubGridCorrectly("ADH_VoidingReason");
			AssertColumnsNotAddedToSubGridCorrectly("ADH_ApprovalNumber");
		}

		void AssertColumnsAddedToSubGridCorrectly(ZString columnName)
		{
			using (var filterControl = new ComplianceDocumentFilterStripControl(null, new ARComplianceDocumentFilterStripBusinessObject()))
			{
				var expectedColumn = columnName;
				var columns = filterControl.ComplianceDocumentLinesDisplayGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				Assert("New column should be added.", columns.Any(x => x.ColumnName == expectedColumn));
				Assert("New column should be visible", columns.FirstOrDefault(x => x.ColumnName == expectedColumn).IsVisible);
			}
		}

		void AssertColumnsNotAddedToSubGridCorrectly(ZString columnName)
		{
			using (var filterControl = new ComplianceDocumentFilterStripControl(null, new APComplianceDocumentFilterStripBusinessObject()))
			{
				var expectedColumn = columnName;
				var columns = filterControl.ComplianceDocumentLinesDisplayGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				Assert("New column should not be added.", !columns.Any(x => x.ColumnName == expectedColumn));
			}
		}

		void AssertColumnsAddedToMainGridCorrectly(ZString columnName)
		{
			using (var filterControl = new ComplianceDocumentFilterStripControl(null, new ARComplianceDocumentFilterStripBusinessObject()))
			{
				var expectedColumn = columnName;
				var columns = filterControl.FilteredGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				Assert("New column should be added.", columns.Any(x => x.ColumnName == expectedColumn));
				Assert("New column should be visible", columns.FirstOrDefault(x => x.ColumnName == expectedColumn).IsVisible);
			}
		}

		public void TestEInvoicingColumnsVisibilityWithEInvoicingComplianceDateRegistry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				AssertEInvoicingColumnsVisibilityWithEInvoicingComplianceDateRegistry(new ARComplianceDocumentFilterStripBusinessObject());
				AssertEInvoicingColumnsVisibilityWithEInvoicingComplianceDateRegistry(new APComplianceDocumentFilterStripBusinessObject());
			}
		}

		void AssertEInvoicingColumnsVisibilityWithEInvoicingComplianceDateRegistry(ComplianceDocumentFilterStripBusinessObject filterBisObj)
		{
			var gridCollection = new AccComplianceDocumentHeaderCollection(Factory);

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (ComplianceDocumentFilterStripControl filterControl = new ComplianceDocumentFilterStripControl(gridCollection, filterBisObj))
			{
				Assert(AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value);

				if (filterBisObj is ARComplianceDocumentFilterStripBusinessObject)
				{
					Assert("EReporting Status column", ColumnExistsInTheGrid(filterControl.FilteredGrid, "EInvoicingStatus", ResourceStringData.Empty));
					Assert("EReporting Error column", ColumnExistsInTheGrid(filterControl.FilteredGrid, "EInvoicingError", ResourceStringData.Empty));
					Assert("EReporting Last Response Received UTC column", ColumnExistsInTheGrid(filterControl.FilteredGrid, "EInvoicingLastResponseReceivedUtc", ResourceStringData.Empty));
					Assert("EReporting Last Sent Time UTC column", ColumnExistsInTheGrid(filterControl.FilteredGrid, "EInvoicingLastSentTimeUtc", ResourceStringData.Empty));
					Assert("EReporting Batch Number column", ColumnExistsInTheGrid(filterControl.FilteredGrid, "EInvoicingBatchNumber", ResourceStringData.Empty));
					Assert("EReporting Batch Status column", ColumnExistsInTheGrid(filterControl.FilteredGrid, "EInvoicingBatchStatus", ResourceStringData.Empty));
				}
				else
				{
					Assert("EReporting Status column", !ColumnExistsInTheGrid(filterControl.FilteredGrid, "EInvoicingStatus", ResourceStringData.Empty));
					Assert("EReporting Error column", !ColumnExistsInTheGrid(filterControl.FilteredGrid, "EInvoicingError", ResourceStringData.Empty));
					Assert("EReporting Last Response Received UTC column", !ColumnExistsInTheGrid(filterControl.FilteredGrid, "EInvoicingLastResponseReceivedUtc", ResourceStringData.Empty));
					Assert("EReporting Last Sent Time UTC column", !ColumnExistsInTheGrid(filterControl.FilteredGrid, "EInvoicingLastSentTimeUtc", ResourceStringData.Empty));
					Assert("EReporting Batch Number column", !ColumnExistsInTheGrid(filterControl.FilteredGrid, "EInvoicingBatchNumber", ResourceStringData.Empty));
					Assert("EReporting Batch Status column", !ColumnExistsInTheGrid(filterControl.FilteredGrid, "EInvoicingBatchStatus", ResourceStringData.Empty));
				}
			}

			using (ComplianceDocumentFilterStripControl filterControl = new ComplianceDocumentFilterStripControl(gridCollection, filterBisObj))
			{
				Assert(!AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value);
				Assert("EReporting Status column", !ColumnExistsInTheGrid(filterControl.FilteredGrid, "EInvoicingStatus", ResourceStringData.Empty));
				Assert("EReporting Error column", !ColumnExistsInTheGrid(filterControl.FilteredGrid, "EInvoicingError", ResourceStringData.Empty));
				Assert("EReporting Last Response Received UTC column", !ColumnExistsInTheGrid(filterControl.FilteredGrid, "EInvoicingLastResponseReceivedUtc", ResourceStringData.Empty));
				Assert("EReporting Last Sent Time UTC column", !ColumnExistsInTheGrid(filterControl.FilteredGrid, "EInvoicingLastSentTimeUtc", ResourceStringData.Empty));
				Assert("EReporting Batch Number column", !ColumnExistsInTheGrid(filterControl.FilteredGrid, "EInvoicingBatchNumber", ResourceStringData.Empty));
				Assert("EReporting Batch Status column", !ColumnExistsInTheGrid(filterControl.FilteredGrid, "EInvoicingBatchStatus", ResourceStringData.Empty));
			}
		}

		ZBool ColumnExistsInTheGrid(ZDisplayGrid grid, ZString columnName, ResourceStringData groupName)
		{
			foreach (ZGridColumnInfo columnStyle in grid.ColumnStyles)
			{
				if (columnStyle.ColumnName == columnName && (groupName.IsEmpty() || columnStyle.GroupName.Caption == groupName.Caption))
				{
					return ZBool.True;
				}
			}
			return ZBool.False;
		}

		public void TestColumnsCustomsRelatedCaption()
		{
			using (var filterControl = new ComplianceDocumentFilterStripControl(null, new ARComplianceDocumentFilterStripBusinessObject()))
			{
				var columns = filterControl.FilteredGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				AssertEquals("caption should be Customs Related", "Customs Related", columns.FirstOrDefault(x => x.ColumnName == "ADH_CustomRelated").CaptionResourceString.Caption);
			}
		}

		public void TestIgnoreParentFilterControl()
		{
			using (var filterControl = new ComplianceDocumentFilterStripControl(null, new ARComplianceDocumentFilterStripBusinessObject()))
			{
				AssertEquals(false, filterControl.FilteredGrid.IgnoreParentFilterControl);
			}
		}
	}
}
