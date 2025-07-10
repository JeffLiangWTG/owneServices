using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.Testing
{
	[TestedType(typeof(BulkCostApportionmentFormHollywood))]
	public class BulkCostApportionmentFormHollywoodTest : ZFormBasherTest
	{
		#region Overrides

		protected override Form GetFormToBashCore()
		{
			APBulkInvoicePoster bulkPoster = new APBulkInvoicePoster(Factory);
			BulkCostApportionmentFormHollywood form = new BulkCostApportionmentFormHollywood(bulkPoster);
			form.ControllerID = ControllerIDs.APBulkInvoicePosting;
			return form;
		}

		#endregion

		public void TestFormControls()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			using (BulkCostApportionmentFormHollywood form = (BulkCostApportionmentFormHollywood)GetFormToBashCore())
			{
				form.Show();
				AssertContains("Bulk AP Invoice Posting", form.Text);

				AssertIsColumnsVisible(form, "TaxRate", true);
				AssertIsColumnsVisible(form, "TaxDate", false);
				AssertIsColumnsVisible(form, "AH_OSTaxAmount", true);
			}

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			using (BulkCostApportionmentFormHollywood form = (BulkCostApportionmentFormHollywood)GetFormToBashCore())
			{
				form.InvoicesGrid_ForTestOnly.SetColumnVisible(true, "TaxDate");

				form.Show();

				AssertIsColumnsVisible(form, "TaxRate", false);
				AssertIsColumnsVisible(form, "TaxDate", false);
				AssertIsColumnsVisible(form, "AH_OSTaxAmount", false);
			}

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			string oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.India);
			AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.CountryCodes.India);
			using (BulkCostApportionmentFormHollywood form = (BulkCostApportionmentFormHollywood)GetFormToBashCore())
			{
				form.Show();
				AssertIsColumnsVisible(form, "AH_OSExtraTaxAmount", true);
				AssertEquals("AH_OSExtraTaxAmount should not be unavailable", false, form.InvoicesGrid_ForTestOnly.GetColumnStyle("AH_OSExtraTaxAmount").IsUnavailable);
			}

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Mexico);
			AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.CountryCodes.Mexico);
			using (BulkCostApportionmentFormHollywood form = (BulkCostApportionmentFormHollywood)GetFormToBashCore())
			{
				form.Show();
				AssertIsColumnsVisible(form, "AH_OSExtraTaxAmount", true);
				AssertEquals("AH_OSExtraTaxAmount should not be unavailable", false, form.InvoicesGrid_ForTestOnly.GetColumnStyle("AH_OSExtraTaxAmount").IsUnavailable);
			}

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Canada);

			Factory.Save();
			using (BulkCostApportionmentFormHollywood form = (BulkCostApportionmentFormHollywood)GetFormToBashCore())
			{
				form.Show();
				AssertIsColumnsVisible(form, "AH_OSExtraTaxAmount", true);
				AssertEquals("AH_OSExtraTaxAmount should not be unavailable", false, form.InvoicesGrid_ForTestOnly.GetColumnStyle("AH_OSExtraTaxAmount").IsUnavailable);
			}
		}

		public void TestAccrualsAndInvoicesGridIsDisabledAfterSuccessfulSave()
		{
			AssertAccrualsAndInvoicesGridIsDisabledAfterSaving(true);
		}

		public void TestAccrualsAndInvoicesGridIsDisabledAfterUnsuccessfulSave()
		{
			AssertAccrualsAndInvoicesGridIsDisabledAfterSaving(false);
		}

		[TestDate(2019, 10, 20)]
		public void TestHandleNegativeCompliancesFailedToCreate()
		{
			AssertHandleNegativeCompliancesFailedToCreate(true);
		}

		[TestDate(2019, 10, 20)]
		public void TestHandleNegativeComplianceLinesFailedToCreate()
		{
			AssertHandleNegativeCompliancesFailedToCreate(false);
		}

		void AssertHandleNegativeCompliancesFailedToCreate(bool flag)
		{
			var fesDepartment = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FES");
			var org = TestObjectCreator.Creditor1;
			var job1 = TestObjectCreator.CreateJob(org, 0, null, 0);
			var accrual1 = TestObjectCreator.CreateAccrual(job1, TestObjectCreator.CC1, 1, "S00001001", -110m, -100m, -10m);
			accrual1.AL_GE = fesDepartment.PK;
			var job2 = TestObjectCreator.CreateJob(org, 0, null, 0);
			var accrual2 = TestObjectCreator.CreateAccrual(job2, TestObjectCreator.CC1, 1, "S00001002", -110m, -100m, -10m);
			accrual2.AL_GE = fesDepartment.PK;
			Factory.Save();

			var poster = TestObjectCreator.CreateAPBulkInvoicePoster(new[] { accrual1, accrual2 }, true);
			TestObjectCreator.CreateAPInvoiceForBulkPoster(poster, "1", -100m, -10m, org);
			TestObjectCreator.CreateAPInvoiceForBulkPoster(poster, "2", -100m, -10m, org);

			poster.RunPreSaveValidation();
			AssertNoErrors("Precondition", poster);

			TestObjectCreator.Creditor1.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = "PCD";

			var currCompany = GlbCompany.CurrentCompany;
			var currCompanyGuid = currCompany.PK.ToGuid();
			var registry = AccountingMasterFilesRegistry.Instance;
			using (currCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			using (registry.EnableComplianceDocumentModule.SetTemporaryValue(currCompanyGuid, Guid.Empty, Guid.Empty, true))
			using (registry.AllowNegativeComplianceDocumentLines.SetTemporaryValue(currCompanyGuid, Guid.Empty, Guid.Empty, flag))
			using (registry.ComplianceDocumentNumberAllocation_Payables.SetTemporaryValue(currCompanyGuid, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			using (var form = new BulkCostApportionmentFormHollywood(poster))
			{
				var userNotif = UnitTestUserNotification.Instance;
				userNotif.ClearMessagesAndAnswers();
				form.Show();
				form.TabControl_ForTestOnly.SelectTab(1);
				Application.DoEvents();

				Assertion.CombineAssertions(() =>
				{
					AssertEquals("Save should be allowed", ContinueWithSave.Yes, form.ValidateAndSave_ForTestOnly());
					Assert(!form.BusinessEntityForValidation_ForTestOnly.HasMessageErrors());
					Assert(userNotif.PreviousMessages.ContainsMessageWithThisText(AccountingConstants.GetComplianceDocumentNegativeMessage()));
				});

				userNotif.ClearMessages();
			}
		}

		public void TestHandleComplianceSequenceOrderByPostDate()
		{
			var currComp = GlbCompany.CurrentCompany;
			using (currComp.TemporarilySetCountry(CountryCodes.Italy))
			{
				AccountingConfigurationRegistry.Instance.DiscrepancyGLAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());

				const string subType = "APS";
				var sequence = TestObjectCreator.SetupComplianceSequence(ZGuid.Empty, subType, subType, 1, 99, 25, currComp.PK, GlbBranch.CurrentBranch.PK);

				var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
				var config = collection.AddNew();
				config.Country = currComp.Country.Code;
				config.SubType = subType;
				config.LedgerType = LedgerTypes.AccountsPayable;
				config.InvoiceType = TransactionTypes.Invoice;
				config.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAnAmountOfTax;
				config.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				config.OriginalRule = OriginalRuleCodes.AllTransactions;

				var org = TestObjectCreator.Creditor1;
				org.CompanyData.SetAPTaxApplicableIgnoringRegistrySetting(true);

				var job1 = TestObjectCreator.CreateJob(org, 0, null, 0);
				var accrual1 = TestObjectCreator.CreateAccrual(job1, TestObjectCreator.CC1, 1, "S00001001", 110m, 100m, 10m);
				var fesDepartment = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FES");
				accrual1.AL_GE = fesDepartment.PK;

				Factory.Save();

				var postingController = ZControllerFactory.Create(ControllerIDs.APBulkInvoicePosting);
				var poster = new APBulkInvoicePoster(postingController.Factory);
				using (poster.GetValidationSuspender())
				{
					poster.Accruals.Add(accrual1);
					poster.UpdateRetrievedAccrualsTotal();
					poster.ExpectedBatchTotal = poster.RetrievedAccrualTotal;
				}

				var invoice1 = TestObjectCreator.CreateAPInvoiceForBulkPoster(poster, "1", 100m, 10m, org);
				invoice1.AH_ComplianceSubType = subType;
				var invoiceLine = TestObjectCreator.CreateAPInvoiceLine(invoice1, job1, TestObjectCreator.CC1,
					TestObjectCreator.EUR, 1m, "for test", accrual1.AL_OSExTaxAmount);
				invoiceLine.AL_OSTaxAmount = accrual1.AL_OSTaxAmount;
				invoiceLine.AL_AT = ZGuid.Empty;
				invoiceLine.AL_GE = fesDepartment.PK;
				invoiceLine.AL_TaxDate = ZDate.Today;
				var registry = AccountingMasterFilesRegistry.Instance;
				var currCompGuid = currComp.PK.ToGuid();
				using (registry.ComplianceSubTypeAttributionRuleConfiguration.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, collection))
				using (registry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code))
				using (registry.ComplianceDocumentNumberAllocation_Payables.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
				{
					Assert(currComp.Country.SupportComplianceSubType);

					using (var form = (BulkCostApportionmentFormHollywood)postingController.ShowFormForNewEntity(poster))
					{
						form.Show();
						poster.ExpectedBatchTotal = 110m;
						poster.TotalOSAmount = 110m;
						poster.TotalLocalExTaxAmount = 100m;
						poster.RunPreSaveValidation();
						AssertNoErrors("Postcondition", poster);
					}

					sequence.XD_ExpiryDate = ZDateTime.Today.AddDays(-1);
					Factory.Save();

					using (var form = (BulkCostApportionmentFormHollywood)postingController.ShowFormForNewEntity(poster))
					{
						var userNotif = UnitTestUserNotification.Instance;
						userNotif.ClearMessagesAndAnswers();
						form.Show();
						form.TabControl_ForTestOnly.SelectTab(1);
						Application.DoEvents();

						AssertEquals("Save should NOT be allowed", ContinueWithSave.No, form.ValidateAndSave_ForTestOnly());
						Assert(userNotif.PreviousMessages.ContainsMessageContainingThisText(ComplianceSequenceNumberAllocationErrorMessages.APBulkInvoiceRelatedExceptionMessage));

						userNotif.ClearMessages();
					}
					invoice1.RemoveRowError(ComplianceSequenceNumberAllocationErrorMessages.APBulkInvoiceRelatedExceptionMessage);
					invoice1.RemoveRowError(ComplianceSequenceNumberAllocationErrorMessages.ComplianceSequenceIsFullOrExpiredExceptionMessage);

					sequence.XD_ExpiryDate = ZDateTime.Empty;
					var apInvoice = poster.Invoices[0];
					apInvoice.AH_OSExTaxAmount = 110m;
					Factory.Save();

					using (var form = (BulkCostApportionmentFormHollywood)postingController.ShowFormForNewEntity(poster))
					{
						var userNotif = UnitTestUserNotification.Instance;
						userNotif.ClearMessagesAndAnswers();
						form.Show();
						form.TabControl_ForTestOnly.SelectTab(1);
						Application.DoEvents();

						apInvoice = poster.Invoices[0];
						apInvoice.AH_OSExTaxAmount = 110m;
						apInvoice.Lines[0].AL_OSExTaxAmount = accrual1.AL_OSExTaxAmount;
						apInvoice.Lines[0].AL_OSTaxAmount = accrual1.AL_OSTaxAmount;

						var errMsg = invoice1.NotificationsIncludingChildren.Aggregate("Save should be allowed:\n", (str, err) => str += err.Message + "\n");
						AssertEquals(errMsg, ContinueWithSave.Yes, form.ValidateAndSave_ForTestOnly());
						Assert(!userNotif.PreviousMessages.ContainsMessageContainingThisText(ComplianceSequenceNumberAllocationErrorMessages.APBulkInvoiceRelatedExceptionMessage));

						userNotif.ClearMessages();
					}
				}
			}
		}

		[TestDate(2019, 10, 20)]
		public void TestNotCreateDuplicatedCompliancesWhenAllowNegativeComplianceIsTrue()
		{
			var fesDepartment = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FES");
			var org = TestObjectCreator.Creditor1;
			var job1 = TestObjectCreator.CreateJob(org, 0, null, 0);
			var accrual1 = TestObjectCreator.CreateAccrual(job1, TestObjectCreator.CC1, 1, "S00001001", 110m, 100m, 10m);
			accrual1.AL_GE = fesDepartment.PK;
			Factory.Save();

			var poster = TestObjectCreator.CreateAPBulkInvoicePoster(new[] { accrual1 }, true);
			TestObjectCreator.CreateAPInvoiceForBulkPoster(poster, "1", 100m, 10m, org);

			poster.RunPreSaveValidation();
			AssertNoErrors("Precondition", poster);

			TestObjectCreator.Creditor1.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = "PCD";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.AllowNegativeComplianceDocumentLines.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Payables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			using (var form = new BulkCostApportionmentFormHollywood(poster))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Show();
				form.TabControl_ForTestOnly.SelectTab(1);
				Application.DoEvents();

				Assertion.CombineAssertions(() =>
				{
					AssertEquals("Save should be allowed", ContinueWithSave.Yes, form.ValidateAndSave_ForTestOnly());
					Assert(!form.BusinessEntityForValidation_ForTestOnly.HasMessageErrors());
					AssertEquals(false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(AccountingConstants.GetComplianceDocumentNegativeMessage()));
				});

				var invoices = Factory.Load<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.AH_OH, org.PK));
				AssertEquals(1, invoices.Length);

				var compliances = Factory.Load<AccComplianceDocumentHeader>(new ZQuery(AccComplianceDocumentHeaderSchema.ADH_OH_Organisation, org.PK));
				AssertEquals(1, compliances.Length);

				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		public void TestAccrualCollectionSorterRemoved()
		{
			var fesDepartment = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FES");
			var org = TestObjectCreator.Creditor1;
			var job1 = TestObjectCreator.CreateJob(org, 0, null, 0);
			var accrual1 = TestObjectCreator.CreateAccrual(job1, TestObjectCreator.CC1, 1, "S00001001", 110, 100, 10);
			accrual1.AL_GE = fesDepartment.PK;
			var job2 = TestObjectCreator.CreateJob(org, 0, null, 0);
			var accrual2 = TestObjectCreator.CreateAccrual(job1, TestObjectCreator.CC1, 1, "S00001002", 110, 100, 10);
			accrual2.AL_GE = fesDepartment.PK;
			Factory.Save();

			var poster = TestObjectCreator.CreateAPBulkInvoicePoster(new[] { accrual1, accrual2 }, true);
			TestObjectCreator.CreateAPInvoiceForBulkPoster(poster, "1", 100m, 10m, org);
			TestObjectCreator.CreateAPInvoiceForBulkPoster(poster, "2", 100m, 10m, org);

			poster.RunPreSaveValidation();
			AssertNoErrors("Precondition", poster);

			using (BulkCostApportionmentFormHollywood form = new BulkCostApportionmentFormHollywood(poster))
			{
				form.Show();
				form.TabControl_ForTestOnly.SelectTab(1);
				Application.DoEvents();

				AssertEquals("Precondition. Accruals is not sorted.", false, ((System.ComponentModel.IBindingList)poster.Accruals).IsSorted);
				form.FilterControl_ForTestOnly.FirePerformSearch();
				Application.DoEvents();
				AssertEquals("Postcondition. Accruals is not sorted to improve load performance.", false, ((System.ComponentModel.IBindingList)poster.Accruals).IsSorted);
			}
		}

		public void TestColumnPrefixOfAuditFiltersShouldBeAL()
		{
			using (BulkCostApportionmentFormHollywood form = (BulkCostApportionmentFormHollywood)GetFormToBashCore())
			{
				form.Show();
				var filterControl = form.FilterControl_ForTestOnly;
				var filterBizO = filterControl.FilterBusinessObject;

				foreach (ModuleFilter filter in filterBizO.ModuleFilters)
				{
					if (filter.Category == FilterCategories.AuditInformation && filter.FilterColumn != null)
					{
						AssertEquals("AL", filter.FilterColumn.ColumnPrefix);
					}
				}
			}
		}

		public void TestCharacterCasingOfInvoiceNumInGrid()
		{
			using (BulkCostApportionmentFormHollywood form = (BulkCostApportionmentFormHollywood)GetFormToBashCore())
			{
				form.Show();
				var invoiceNumTextBoxInfo = (ZTextBoxColumnStyleInfo)form.InvoicesGrid_ForTestOnly.GetColumnStyle(AccTransactionHeader.Schema.AH_TransactionNum);
				AssertNotNull(invoiceNumTextBoxInfo);
				AssertEquals(CharacterCasing.Upper, invoiceNumTextBoxInfo.CharacterCasing);
			}
		}

		public void TestTaxDateControl()
		{
			using (BulkCostApportionmentFormHollywood form = (BulkCostApportionmentFormHollywood)GetFormToBashCore())
			{
				form.Show();
				AssertIsColumnsVisible(form, "TaxDate", false);
			}
		}

		void AssertAccrualsAndInvoicesGridIsDisabledAfterSaving(bool successfulSave)
		{
			GlbDepartment fesDepartment = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FES");
			var org = TestObjectCreator.Creditor1;
			Job jH_S00001001 = TestObjectCreator.CreateJob(org, 0, null, 0);
			Accrual accrual01 = TestObjectCreator.CreateAccrual(jH_S00001001, TestObjectCreator.CC1, 1, "S00001001", 110, 100, 10);
			accrual01.AL_GE = fesDepartment.PK;
			Factory.Save();

			var poster = TestObjectCreator.CreateAPBulkInvoicePoster(new[] { accrual01 }, true);
			TestObjectCreator.CreateAPInvoiceForBulkPoster(poster, "1", 100m, 10m, org);

			poster.RunPreSaveValidation();
			AssertNoErrors("Precondition", poster);

			Action<bool> assert = shouldBeEnabled =>
			{
				using (BulkCostApportionmentFormHollywood form = new BulkCostApportionmentFormHollywood(poster))
				{
					form.Show();
					form.TabControl_ForTestOnly.SelectTab(1);
					Application.DoEvents();
					AssertEquals("AccrualsGrid_ForTestOnly readonlyness", false, form.AccrualsGrid_ForTestOnly.ReadOnly);
					AssertEquals("InvoicesGrid_ForTestOnly readonlyness", false, form.InvoicesGrid_ForTestOnly.ReadOnly);

					form.PostButton_ForTestOnly.PerformClick();
					Application.DoEvents();
					AssertEquals("AccrualsGrid_ForTestOnly readonlyness", shouldBeEnabled, !form.AccrualsGrid_ForTestOnly.ReadOnly);
					AssertEquals("InvoicesGrid_ForTestOnly readonlyness", shouldBeEnabled, !form.InvoicesGrid_ForTestOnly.ReadOnly);
				}
			};
			accrual01.ShouldReverese = false;
			poster.RunPreSaveValidation();
			Assert("Precondition: poster.HasErrors", poster.HasErrors);
			assert(true);

			accrual01.ShouldReverese = true;
			if (successfulSave)
			{
				poster.RunPreSaveValidation();
				AssertNoErrors("Precondition", poster);
				assert(false);
			}
			else
			{
				BusinessObjectFactory.SavingEventHandler onSavingHandler = f =>
				{
					throw new ZCannotSaveException("Test Exception", "");
				};
				Factory.Saving += onSavingHandler;
				try
				{
					poster.RunPreSaveValidation();
					AssertNoErrors("Precondition", poster);
					assert(false);
				}
				finally
				{
					Factory.Saving -= onSavingHandler;
				}
			}
		}

		#region Implementaton

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator.CreateTestPeriods(ZDateTime.Now.AddMonths(-2));
		}

		void AssertIsColumnsVisible(BulkCostApportionmentFormHollywood form, ZString colunmName, ZBool isVisible)
		{
			foreach (ZGridColumnInfo columnInfo in form.InvoicesGrid_ForTestOnly.ColumnStyles)
			{
				if (columnInfo.ColumnName == colunmName)
				{
					AssertEquals(isVisible, columnInfo.IsVisible);
				}
			}
		}

		#endregion

		TestObjectCreator TestObjectCreator
		{
			get { return fCreator ?? (fCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator fCreator;
	}
}
