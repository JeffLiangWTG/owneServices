using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.Accounting.Module.Testing
{
	public class TransactionFilterStripControlTest : TestCaseWithFactory
	{
		#region Implementation

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

		#endregion

		public void TestGovernmentAllocatedNumberColumnExistsForAP()
			=> AssertGovernmentAllocatedNumberColumnForAP(registryEnabled: true);

		public void TestGovernmentAllocatedNumberColumnNotExistsForAP()
			=> AssertGovernmentAllocatedNumberColumnForAP(registryEnabled: false);

		void AssertGovernmentAllocatedNumberColumnForAP(bool registryEnabled)
		{
			AccountingMasterFilesRegistry.Instance.EnableGovernmentAllocatedNumberBehavior.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value: registryEnabled);
			var gridCollection = new TransactionHeaderCollection(Factory);
			var aPFilterBusinessObject = new APTransactionFilterStripBusinessObject();

			using var control = new TransactionFilterStripControl(gridCollection, aPFilterBusinessObject);

			AssertEquals(registryEnabled, ColumnExistsInTheGrid(control.FilteredGrid, "AH_GovernmentAllocatedID", ResourceStringData.Empty));
		}

		public void TestGovernmentAllocatedNumberColumnForAR()
		{
			AccountingMasterFilesRegistry.Instance.EnableGovernmentAllocatedNumberBehavior.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value: true);
			var gridCollection = new TransactionHeaderCollection(Factory);
			var aRFilterBusinessObject = new ARTransactionFilterStripBusinessObject();

			using var control = new TransactionFilterStripControl(gridCollection, aRFilterBusinessObject);

			Assert(!ColumnExistsInTheGrid(control.FilteredGrid, "AH_GovernmentAllocatedID", ResourceStringData.Empty));
		}

		public void TestComplianceDocumentDateColumnExistForChina()
		{
			TransactionHeaderCollection gridCollection = new TransactionHeaderCollection(Factory);
			APTransactionFilterStripBusinessObject aPFilterBusinessObject = new APTransactionFilterStripBusinessObject();
			ARTransactionFilterStripBusinessObject aRFilterBusinessObject = new ARTransactionFilterStripBusinessObject();
			GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China);

			using (TransactionFilterStripControl filterControl = new TransactionFilterStripControl(gridCollection, aPFilterBusinessObject))
			{
				Assert("The Compliance Doc Date column should NOT exists for APInvoice", !ColumnExistsInTheGrid(filterControl.FilteredGrid, "AH_ComplianceDocumentDate", ResourceStringData.Empty));
			}
			using (TransactionFilterStripControl filterControl = new TransactionFilterStripControl(gridCollection, aRFilterBusinessObject))
			{
				Assert("The Compliance Doc Date column should exists for ARInvoice", ColumnExistsInTheGrid(filterControl.FilteredGrid, "AH_ComplianceDocumentDate", ResourceStringData.Empty));
			}
		}

		public void TestMatchStatusAndReasonColumns()
		{
			TransactionHeaderCollection gridCollection = new TransactionHeaderCollection(Factory);
			APTransactionFilterStripBusinessObject aPFilterBusinessObject = new APTransactionFilterStripBusinessObject();
			using (var control = new TransactionFilterStripControl(gridCollection, aPFilterBusinessObject))
			{
				Assert(ColumnExistsInTheGrid(control.FilteredGrid, "AH_MatchStatus", ResourceStringData.Empty));
				Assert(ColumnExistsInTheGrid(control.FilteredGrid, "AH_MatchStatusReasonCode", ResourceStringData.Empty));
			}
		}

		public void TestAvailabilityOfWHTColumns()
		{
			var gridCollection = new TransactionHeaderCollection(Factory);
			var aPFilterBusinessObject = new APTransactionFilterStripBusinessObject();
			var taxTestHelper = new AccountingTestObjectCreator(new BusinessObjectFactory());

			using (var control = new TransactionFilterStripControl(gridCollection, aPFilterBusinessObject))
			{
				AssertEquals(false, ColumnExistsInTheGrid(control.FilteredGrid, nameof(TransactionHeader.Schema.AH_NotionalWHTTax), ResourceStringData.Empty));
				AssertEquals(false, ColumnExistsInTheGrid(control.FilteredGrid, nameof(TransactionHeader.Schema.AH_RealizedWHTTax), ResourceStringData.Empty));
			}

			taxTestHelper.ConfigureTaxFrameworkAtCompanyLevel(GlbCompany.CurrentCompany, LedgerTypes.AccountsPayable, TaxSuperTypeList.StandardPaymentRetention.Code);

			using (var control = new TransactionFilterStripControl(gridCollection, aPFilterBusinessObject))
			{
				AssertEquals(true, ColumnExistsInTheGrid(control.FilteredGrid, nameof(TransactionHeader.Schema.AH_NotionalWHTTax), ResourceStringData.Empty));
				AssertEquals(true, ColumnExistsInTheGrid(control.FilteredGrid, nameof(TransactionHeader.Schema.AH_RealizedWHTTax), ResourceStringData.Empty));
			}
		}

		public void TestEInvoicingColumnsVisibility()
		{
			var gridCollection = new TransactionHeaderCollection(Factory);
			var aPFilterBusinessObject = new APTransactionFilterStripBusinessObject();
			var aRFilterBusinessObject = new ARTransactionFilterStripBusinessObject();

			var currCompany = GlbCompany.CurrentCompany;
			var registry = AccountingMasterFilesRegistry.Instance;
			var eInvoicingColumns = new string[]
			{ "EInvoicingStatus", "EInvoicingError", "EInvoicingBatchNumber", "EInvoicingGovernmentAllocatedNumber", "EInvoicingeHubAllocatedNumber", "EInvoicingLastSentTimeUtc", "EInvoicingLastResponseReceivedUtc", "EInvoicingAuthorisationNumber" };

			var countryCodeList = Country.LicenceKeyBuilderSupportedCountryCodes;
			foreach (var countryCode in countryCodeList)
			{
				using (currCompany.TemporarilySetCountry(countryCode))
				{
					var isSupportedCountry = ElectronicInvoicingEligibilityDecider.IsSupportedCountry(countryCode);

					foreach (var filterBizO in new TransactionFilterStripBusinessObject[] { aPFilterBusinessObject, aRFilterBusinessObject })
					{
						var regFunctionality = filterBizO.IsPayableModule ? registry.EnableEInvoicingFunctionalityForPayables : registry.EnableEInvoicingFunctionality;
						using (regFunctionality.SetTemporaryValue(currCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
						using (var filterControl = new TransactionFilterStripControl(gridCollection, filterBizO))
						{
							var grid = filterControl.FilteredGrid;
							foreach (var item in eInvoicingColumns)
							{
								AssertEquals($"{countryCode}: {item} column", isSupportedCountry, ColumnExistsInTheGrid(grid, item, ResourceStringData.Empty));
							}
						}
					}
				}
			}
		}

		public void TestManageColumnOrdering()
		{
			var gridCollection = new TransactionHeaderCollection(Factory);

			var aPFilterBusinessObject = new APTransactionFilterStripBusinessObject();
			using (var filterControl = new TransactionFilterStripControl(gridCollection, aPFilterBusinessObject))
			{
				var grid = filterControl.FilteredGrid;
				Assert("The Job Invoice Number column should not be deleted for AP ledger", ColumnExistsInTheGrid(grid, "AH_ConsolidatedInvoiceRef", new ResourceStringData("", "AP")));
				Assert("The Internal Reference column should be deleted for AP ledger", !ColumnExistsInTheGrid(grid, "AH_ConsolidatedInvoiceRef", new ResourceStringData("", "AR")));
				Assert("The DSB Invoice column should be deleted for AP ledger", !ColumnExistsInTheGrid(grid, "AH_IsDisbursementCalc", ResourceStringData.Empty));
				Assert("The Is Self Billing Invoice column should not be deleted for AP ledger", ColumnExistsInTheGrid(grid, "IsSelfBillingInvoice", ResourceStringData.Empty));
				Assert("The Additional Company Name column should not be deleted for AP ledger", ColumnExistsInTheGrid(grid, "AdditionalCompanyName", ResourceStringData.Empty));
				Assert("The Audited By column should not be deleted for AP ledger", ColumnExistsInTheGrid(grid, "AH_GS_NKAuditedBy", ResourceStringData.Empty));
				Assert("The Cashier column should not be deleted for AP ledger", ColumnExistsInTheGrid(grid, "AH_GS_NKCashier", ResourceStringData.Empty));
				Assert("The Invoice Transaction Reference column should not exist for AP ledger", !ColumnExistsInTheGrid(grid, "InvoiceTransactionReference", ResourceStringData.Empty));
				Assert("The Invoice Remittance Type column should not exist for AP ledger", !ColumnExistsInTheGrid(grid, "AH_InvoicePaymentReferenceCode", ResourceStringData.Empty));
			}

			var aRFilterBusinessObject = new ARTransactionFilterStripBusinessObject();
			using (var filterControl = new TransactionFilterStripControl(gridCollection, aRFilterBusinessObject))
			{
				var grid = filterControl.FilteredGrid;
				Assert("The Job Invoice Number column should be deleted for AP ledger", !ColumnExistsInTheGrid(grid, "AH_ConsolidatedInvoiceRef", new ResourceStringData("", "AP")));
				Assert("The Internal Reference column should not be deleted for AR ledger", ColumnExistsInTheGrid(grid, "AH_ConsolidatedInvoiceRef", new ResourceStringData("", "AR")));
				Assert("The DSB Invoice column should not be deleted for AR ledger", ColumnExistsInTheGrid(grid, "AH_IsDisbursementCalc", ResourceStringData.Empty));
				Assert("The Is Self Billing Invoice column should be deleted for AR ledger", !ColumnExistsInTheGrid(grid, "IsSelfBillingInvoice", ResourceStringData.Empty));
				Assert("The Additional Company Name column should not be deleted for AR ledger", ColumnExistsInTheGrid(grid, "AdditionalCompanyName", ResourceStringData.Empty));
				Assert("The Audited By column should not be deleted for AR ledger", ColumnExistsInTheGrid(grid, "AH_GS_NKAuditedBy", ResourceStringData.Empty));
				Assert("The Cashier column should not be deleted for AR ledger", ColumnExistsInTheGrid(grid, "AH_GS_NKCashier", ResourceStringData.Empty));
				Assert("The Invoice Transaction Reference column should not be deleted for AR ledger", ColumnExistsInTheGrid(grid, "InvoiceTransactionReference", ResourceStringData.Empty));
				Assert("The Invoice Remittance Type column should not be deleted for AR ledger", ColumnExistsInTheGrid(grid, "AH_InvoicePaymentReferenceCode", ResourceStringData.Empty));
			}

			var currCompany = GlbCompany.CurrentCompany;

			using (currCompany.TemporarilySetCountry(CountryCodes.China))
			using (var filterControl = new TransactionFilterStripControl(gridCollection, aPFilterBusinessObject))
			{
				Assert("The Transaction Reference column should exists for China company", ColumnExistsInTheGrid(filterControl.FilteredGrid, "AH_TransactionReference", ResourceStringData.Empty));
			}
			using (currCompany.TemporarilySetCountry(CountryCodes.Australia))
			using (var filterControl = new TransactionFilterStripControl(gridCollection, aPFilterBusinessObject))
			{
				Assert("The Transaction Reference column should be deleted for non-China company", !ColumnExistsInTheGrid(filterControl.FilteredGrid, "AH_TransactionReference", ResourceStringData.Empty));
			}

			using (currCompany.TemporarilySetCountry(CountryCodes.Peru))
			using (var filterControl = new TransactionFilterStripControl(gridCollection, aPFilterBusinessObject))
			{
				Assert("The Compliance SubType column should exists for Peru company", ColumnExistsInTheGrid(filterControl.FilteredGrid, "AH_ComplianceSubType", ResourceStringData.Empty));
				Assert("The Compliance Doc Date column should NOT exists for Peru company", !ColumnExistsInTheGrid(filterControl.FilteredGrid, "AH_ComplianceDocumentDate", ResourceStringData.Empty));
			}

			using (currCompany.TemporarilySetCountry(CountryCodes.VietNam))
			using (var filterControl = new TransactionFilterStripControl(gridCollection, aPFilterBusinessObject))
			{
				Assert("The Compliance SubType column should exists for VietNam company", ColumnExistsInTheGrid(filterControl.FilteredGrid, "AH_ComplianceSubType", ResourceStringData.Empty));
				Assert("The Compliance Doc Date column should NOT exists for VietNam company", !ColumnExistsInTheGrid(filterControl.FilteredGrid, "AH_ComplianceDocumentDate", ResourceStringData.Empty));
			}

			using (currCompany.TemporarilySetCountry(CountryCodes.China))
			using (var filterControl = new TransactionFilterStripControl(gridCollection, aPFilterBusinessObject))
			{
				Assert("The Compliance SubType column should exists for China company", ColumnExistsInTheGrid(filterControl.FilteredGrid, "AH_ComplianceSubType", ResourceStringData.Empty));
				Assert("The Compliance Doc Date column should NOT exists for China company", !ColumnExistsInTheGrid(filterControl.FilteredGrid, "AH_ComplianceDocumentDate", ResourceStringData.Empty));
			}

			using (currCompany.TemporarilySetCountry(CountryCodes.Indonesia))
			using (var filterControl = new TransactionFilterStripControl(gridCollection, aPFilterBusinessObject))
			{
				Assert("The Compliance SubType column should exists for Indonesia company", ColumnExistsInTheGrid(filterControl.FilteredGrid, "AH_ComplianceSubType", ResourceStringData.Empty));
				Assert("The Compliance Doc Date column should NOT exists for Indonesia company", !ColumnExistsInTheGrid(filterControl.FilteredGrid, "AH_ComplianceDocumentDate", ResourceStringData.Empty));
			}

			using (currCompany.TemporarilySetCountry(CountryCodes.Australia))
			using (var filterControl = new TransactionFilterStripControl(gridCollection, aPFilterBusinessObject))
			{
				Assert("The Compliance SubType column should be deleted for Australia company", !ColumnExistsInTheGrid(filterControl.FilteredGrid, "AH_ComplianceSubType", ResourceStringData.Empty));
				Assert("The Compliance Doc Date column should NOT exists for Australia company", !ColumnExistsInTheGrid(filterControl.FilteredGrid, "AH_ComplianceDocumentDate", ResourceStringData.Empty));
			}

			using (currCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			using (var filterControl = new TransactionFilterStripControl(gridCollection, aPFilterBusinessObject))
			{
				Assert("The Compliance SubType column should exists for Taiwan company", ColumnExistsInTheGrid(filterControl.FilteredGrid, "AH_ComplianceSubType", ResourceStringData.Empty));
				Assert("The Compliance Doc Date column should exists for Taiwan company", ColumnExistsInTheGrid(filterControl.FilteredGrid, "AH_ComplianceDocumentDate", ResourceStringData.Empty));
			}

			var mfRegistry = AccountingMasterFilesRegistry.Instance;
			var cfgRegistry = AccountingConfigurationRegistry.Instance;

			using (cfgRegistry.AllowIncludingRelatedTransasctionDebtorColumnOfAP.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var filterControl = new TransactionFilterStripControl(gridCollection, aPFilterBusinessObject))
			{
				Assert("Related Debtor column should not be deleted for AP Ledger", ColumnExistsInTheGrid(filterControl.FilteredGrid, "RelatedTransactionDebtorsAsString", ResourceStringData.Empty));
			}

			using (cfgRegistry.AllowIncludingRelatedTransasctionDebtorColumnOfAP.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var filterControl = new TransactionFilterStripControl(gridCollection, aPFilterBusinessObject))
			{
				Assert("Related Debtor column should be deleted for AP Ledger", !ColumnExistsInTheGrid(filterControl.FilteredGrid, "RelatedTransactionDebtorsAsString", ResourceStringData.Empty));
			}

			using (cfgRegistry.AllowIncludingRelatedTransasctionDebtorColumnOfAR.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var filterControl = new TransactionFilterStripControl(gridCollection, aRFilterBusinessObject))
			{
				Assert("Related Debtor column should not be deleted for AR Ledger", ColumnExistsInTheGrid(filterControl.FilteredGrid, "RelatedTransactionDebtorsAsString", ResourceStringData.Empty));
			}

			using (cfgRegistry.AllowIncludingRelatedTransasctionDebtorColumnOfAR.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var filterControl = new TransactionFilterStripControl(gridCollection, aRFilterBusinessObject))
			{
				Assert("Related Debtor column should be deleted for AR Ledger", !ColumnExistsInTheGrid(filterControl.FilteredGrid, "RelatedTransactionDebtorsAsString", ResourceStringData.Empty));
			}

			using (mfRegistry.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var filterControl = new TransactionFilterStripControl(gridCollection, aPFilterBusinessObject))
			{
				Assert("Tax Branch column should be deleted when the value of Registry item Enable Tax Branch Reporting is false", !ColumnExistsInTheGrid(filterControl.FilteredGrid, "AH_GB_TaxBranch", ResourceStringData.Empty));
			}

			using (mfRegistry.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var filterControl = new TransactionFilterStripControl(gridCollection, aPFilterBusinessObject))
			{
				Assert("Tax Branch column should not be deleted when the value of Registry item Enable Tax Branch Reporting is true", ColumnExistsInTheGrid(filterControl.FilteredGrid, "AH_GB_TaxBranch", ResourceStringData.Empty));
			}
		}

		public void TestExportToExcel_WithoutShowingRecords()
		{
			var testInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(Business.ARAP.Invoicing.ARInvoice), "Inv001", TestObjectCreator.AUD, 1, 10, 0, 10, 0);
			testInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			Factory.Save();

			using (ZForm zForm = new ZForm())
			{
				var gridCollection = new TransactionHeaderCollection(Factory);
				var filteredtransactionHeadercollection = new FilteredTransactionHeaderCollectionView(gridCollection);

				var filterBizObj = new APTransactionFilterStripBusinessObject();

				TransactionFilterStripControl filterControl = new TransactionFilterStripControl(filteredtransactionHeadercollection, filterBizObj);

				using (ARTransactionModuleStrip module = new ARTransactionModuleStrip())
				{
					filterControl.Grid.SetParentFilterGridModule(module);

					filterControl.FirePerformSearch();
					zForm.Controls.Add(filterControl);
					zForm.Show();

					try
					{
						AssertNoExceptionThrown(delegate
						{ filterControl.Grid.ContextMenu.MenuItems.FindByText("Export All Columns To Excel").PerformClick(); });
					}
					finally
					{
						DeleteIfExists(Enterprise.ZArchitecture.Excel.ExcelExporter.LastExportedFileNameStaticForTest);
					}

					try
					{
						AssertNoExceptionThrown(delegate
						{ filterControl.Grid.ContextMenu.MenuItems.FindByText("Export Visible Columns To Excel").PerformClick(); });
					}
					finally
					{
						DeleteIfExists(Enterprise.ZArchitecture.Excel.ExcelExporter.LastExportedFileNameStaticForTest);
					}
				}
			}
		}

		public void TestSupportingDocumentNumberColumnAddedCorrectly()
		{
			var gridCollection = new TransactionHeaderCollection(Factory);
			var apFilterBusinessObject = new APTransactionFilterStripBusinessObject();
			var arFilterBusinessObject = new ARTransactionFilterStripBusinessObject();

			AssertSupportingDocumentNumberColumnAddedCorrectly(apFilterBusinessObject, Constants.CountryCodes.Australia, false, false);
			AssertSupportingDocumentNumberColumnAddedCorrectly(apFilterBusinessObject, Constants.CountryCodes.Australia, true, false);
			AssertSupportingDocumentNumberColumnAddedCorrectly(arFilterBusinessObject, Constants.CountryCodes.Australia, false, false);
			AssertSupportingDocumentNumberColumnAddedCorrectly(arFilterBusinessObject, Constants.CountryCodes.Australia, true, false);

			AssertSupportingDocumentNumberColumnAddedCorrectly(apFilterBusinessObject, Constants.CountryCodes.VietNam, false, false);
			AssertSupportingDocumentNumberColumnAddedCorrectly(apFilterBusinessObject, Constants.CountryCodes.VietNam, true, false);
			AssertSupportingDocumentNumberColumnAddedCorrectly(arFilterBusinessObject, Constants.CountryCodes.VietNam, false, true);
			AssertSupportingDocumentNumberColumnAddedCorrectly(arFilterBusinessObject, Constants.CountryCodes.VietNam, true, true);

			void AssertSupportingDocumentNumberColumnAddedCorrectly(TransactionFilterStripBusinessObject filterBusinessObject, string countryCode, bool isEInvoicingEnabled, bool isAvailable)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isEInvoicingEnabled))
				using (var filterControl = new TransactionFilterStripControl(gridCollection, filterBusinessObject))
				{
					var supportingDocumentNumber = filterControl.FilteredGrid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == "SupportingDocumentNumber");
					if (isAvailable)
					{
						AssertNotNull("Supporting Document Number is available", supportingDocumentNumber);
					}
					else
					{
						AssertNull("Supporting Document Number is NOT available", supportingDocumentNumber);
					}
				}
			}
		}

		public void TestAH_PlaceOfSupplyColumnsAddedCorrectly()
		{
			var gridCollection = new TransactionHeaderCollection(Factory);
			var apFilterBusinessObject = new APTransactionFilterStripBusinessObject();
			var arFilterBusinessObject = new ARTransactionFilterStripBusinessObject();

			AssertAH_PlaceOfSupplyColumnsAddedCorrectly(apFilterBusinessObject, Core.Constants.CountryCodes.India, true);
			AssertAH_PlaceOfSupplyColumnsAddedCorrectly(apFilterBusinessObject, Core.Constants.CountryCodes.Australia, false);
			AssertAH_PlaceOfSupplyColumnsAddedCorrectly(arFilterBusinessObject, Core.Constants.CountryCodes.India, true);
			AssertAH_PlaceOfSupplyColumnsAddedCorrectly(arFilterBusinessObject, Core.Constants.CountryCodes.Australia, false);

			void AssertAH_PlaceOfSupplyColumnsAddedCorrectly(TransactionFilterStripBusinessObject filterBusinessObject, string countryCode, bool isColumnsAvailable)
			{
				using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly((isColumnsAvailable ? new string[] { PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code } : Array.Empty<string>())))
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				using (var filterControl = new TransactionFilterStripControl(gridCollection, filterBusinessObject))
				{
					var placeOfSupplyColumn = filterControl.FilteredGrid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == AccTransactionHeaderSchema.AH_PlaceOfSupply.Name);
					AssertEquals(isColumnsAvailable, placeOfSupplyColumn != null);
					if (isColumnsAvailable)
					{
						Assert("placeOfSupply column is not visible", !placeOfSupplyColumn.IsVisible);
					}
				}
			}
		}

		public void TestAmendStatusCodeColumnsVisibility()
		{
			var apGridCollection = new APTransactionHeaderCollection(Factory);
			var apFilterBusinessObject = new APTransactionFilterStripBusinessObject();
			using (TransactionFilterStripControl filterControl = new TransactionFilterStripControl(apGridCollection, apFilterBusinessObject))
			{
				AssertEquals($"Visibility of AmendStatusCodeAndDescription should be false", false, ColumnExistsInTheGrid(filterControl.FilteredGrid, "AmendStatusCodeAndDescription", ResourceStringData.Empty));
			}

			AssertAmendStatusCodeColumnsVisibility(Constants.CountryCodes.Australia, true, false);
			AssertAmendStatusCodeColumnsVisibility(Constants.CountryCodes.KoreaSouth, false, false);
			AssertAmendStatusCodeColumnsVisibility(Constants.CountryCodes.KoreaSouth, true, true);

			void AssertAmendStatusCodeColumnsVisibility(ZString countryCode, bool isEnableEInvoicingFunctionality, bool expected)
			{
				var arGridCollection = new ARTransactionHeaderCollection(Factory);
				var arFilterBusinessObject = new ARTransactionFilterStripBusinessObject();

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, isEnableEInvoicingFunctionality))
				using (TransactionFilterStripControl filterControl = new TransactionFilterStripControl(arGridCollection, arFilterBusinessObject))
				{
					AssertEquals($"Visibility of AmendStatusCodeAndDescription should be {expected}", expected, ColumnExistsInTheGrid(filterControl.FilteredGrid, "AmendStatusCodeAndDescription", ResourceStringData.Empty));
				}
			}
		}

		public void TestComplianceDocumentStatusColumnsVisibility()
		{
			var apGridCollection = new APTransactionHeaderCollection(Factory);
			var apFilterBusinessObject = new APTransactionFilterStripBusinessObject();
			using (TransactionFilterStripControl filterControl = new TransactionFilterStripControl(apGridCollection, apFilterBusinessObject))
			{
				AssertEquals($"Visibility of ComplianceDocumentStatus should be false", false, ColumnExistsInTheGrid(filterControl.FilteredGrid, "ComplianceDocumentStatus", ResourceStringData.Empty));
			}

			AssertComplianceDocumentStatusColumnsVisibility(CountryCodes.Australia, true, false);
			AssertComplianceDocumentStatusColumnsVisibility(CountryCodes.China, false, false);
			AssertComplianceDocumentStatusColumnsVisibility(CountryCodes.China, true, true);

			void AssertComplianceDocumentStatusColumnsVisibility(ZString countryCode, bool isEnableEInvoicingFunctionality, bool expected)
			{
				var arGridCollection = new ARTransactionHeaderCollection(Factory);
				var arFilterBusinessObject = new ARTransactionFilterStripBusinessObject();

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, isEnableEInvoicingFunctionality))
				using (TransactionFilterStripControl filterControl = new TransactionFilterStripControl(arGridCollection, arFilterBusinessObject))
				{
					AssertEquals($"Visibility of ComplianceDocumentStatus should be {expected}", expected, ColumnExistsInTheGrid(filterControl.FilteredGrid, "ComplianceDocumentStatus", ResourceStringData.Empty));
				}
			}
		}

		public void TestComplianceSequenceVisibility()
		{
			var gridCollection = new TransactionHeaderCollection(Factory);
			var aRFilterBusinessObject = new ARTransactionFilterStripBusinessObject();
			var currCompany = GlbCompany.CurrentCompany;
			var registry = AccountingMasterFilesRegistry.Instance;
			SetComplianceSubTypeAttributionRuleConfiguration();

			using (currCompany.TemporarilySetCountry(CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (var filterControl = new TransactionFilterStripControl(gridCollection, aRFilterBusinessObject))
			{
				Assert("The Compliance Sequence column should exsit in VietNam company", ColumnExistsInTheGrid(filterControl.FilteredGrid, "ComplianceSequenceWithCodeAndDesc", ResourceStringData.Empty));
			}

			using (currCompany.TemporarilySetCountry(CountryCodes.China))
			using (var filterControl = new TransactionFilterStripControl(gridCollection, aRFilterBusinessObject))
			{
				Assert("The Compliance Sequence column should NOT exsit in China company", !ColumnExistsInTheGrid(filterControl.FilteredGrid, "ComplianceSequenceWithCodeAndDesc", ResourceStringData.Empty));
			}

			using (currCompany.TemporarilySetCountry(CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var filterControl = new TransactionFilterStripControl(gridCollection, aRFilterBusinessObject))
			{
				Assert("The Compliance Sequence column should NOT exsit when EnableComplianceDocumentModule is true", !ColumnExistsInTheGrid(filterControl.FilteredGrid, "ComplianceSequenceWithCodeAndDesc", ResourceStringData.Empty));
			}

			void SetComplianceSubTypeAttributionRuleConfiguration()
			{
				var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
				var item = collection.AddNew();
				item.Country = CountryCodes.VietNam;
				item.SubType = "TXI";
				item.LedgerType = "AR";
				item.InvoiceType = "INV";
				item.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
				item.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				item.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
				AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			}
		}

		#region Test Disbursement Relating To Column

		public void TestDisbursementRelatingToColumnsVisibility()
		{
			AssertDisbursementRelatingToColumnsVisibility(CountryCodes.KoreaSouth, true, true,true);
			AssertDisbursementRelatingToColumnsVisibility(CountryCodes.KoreaSouth, false, false, true);

			AssertDisbursementRelatingToColumnsVisibility(CountryCodes.KoreaSouth, true, false, false);
			AssertDisbursementRelatingToColumnsVisibility(CountryCodes.KoreaSouth, false, false, false);

			AssertDisbursementRelatingToColumnsVisibility(CountryCodes.Australia, true, false, true);
			AssertDisbursementRelatingToColumnsVisibility(CountryCodes.Australia, false, false, true);

			AssertDisbursementRelatingToColumnsVisibility(CountryCodes.Australia, true, false, false);
			AssertDisbursementRelatingToColumnsVisibility(CountryCodes.Australia, false, false, false);

			void AssertDisbursementRelatingToColumnsVisibility(ZString countryCode, bool enableEInvoicingFunctionality, bool expected, bool isARTransactionModule)
			{
				TransactionHeaderCollection gridCollection;
				TransactionFilterStripBusinessObject filterBusinessObject;
				if (isARTransactionModule)
				{
					gridCollection = new ARTransactionHeaderCollection(Factory);
					filterBusinessObject = new ARTransactionFilterStripBusinessObject();
				}
				else
				{
					gridCollection = new APTransactionHeaderCollection(Factory);
					filterBusinessObject = new APTransactionFilterStripBusinessObject();
				}

				using (TestObjectCreator.SetUpForTestingEInvoicingAndAutomaticllyAppendingDisbursementFeesSummary(countryCode, enableEInvoicingFunctionality))
				using (var filterControl = new TransactionFilterStripControl(gridCollection, filterBusinessObject))
				{
					AssertEquals($"Visibility of RelatedDisbursementTransactions column should be {expected}", expected, ColumnExistsInTheGrid(filterControl.FilteredGrid, "RelatedDisbursementTransactions", ResourceStringData.Empty));
				}
			}
		}

		#endregion

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
