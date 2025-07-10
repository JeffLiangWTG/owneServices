using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.GUI.ARAP.UnapprovedAPTransaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.Accounting.GUI.Testing.ARAP.UnapprovedAPTransaction
{
	[TestedType(typeof(UnapprovedTransactionAuthorisationForm))]
	[MasterFiles.Integration.Test.MatchAgainstOnlineFlightsInUnitTest]
	public class UnapprovedAPInvoicesFormTest : ZFormBasherTest
	{
			#region Overrides
			UnapprovedTransactionTestHelper Helper;

			protected override void SetUp()
			{
				TestObjectCreator.CreateTestPeriods(new ZDateTime(ZDateTime.Now.Year, 1, 1));
				Factory.Save();
				Helper = new UnapprovedTransactionTestHelper(Factory, TestObjectCreator);
			}

			protected override Form GetFormToBashCore()
			{
				UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(Factory);
				UnapprovedTransactionAuthorisationForm form = new UnapprovedTransactionAuthorisationForm(converter);
				return form;
			}

			#endregion

			public override void TestBashingForm()
			{
				base.TestBashingForm();
				ErrorReporter.Clear();
			}

			public void TestApproveSisterCompanyTransactionWhenRelatedJobStausIsJobReadyForFinancialClosure()
			{
				var rate = AccTaxRate.Helper.FindTaxRate(new BusinessObjectFactory(), AccTaxRate.Helper.MainNotReportableTaxRegistryID, Env.CurrentCompanyPK);
				rate.SetRateNumerator_ForTestOnly(0);
				rate.Factory.Save();

				var differentCompany = TestObjectCreator.CreateNewCompany("ABC");
				GlbCompany.CurrentCompany.SetCurrency("USD");
				differentCompany.SetCurrency("CNY");

				differentCompany.GC_IsReciprocal = true;
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;
				var differentBranchOrgProxy = TestObjectCreator.CreateOrgHeader("ORGPROXYB", true, true);
				var differentBranch1 = TestObjectCreator.CreateNewBranch(differentCompany, "AB1");
				differentBranch1.GB_OH_OrgProxy = differentBranchOrgProxy.PK;
				var debtorPk = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

				var shipment = TestObjectCreator.CreateShipment("S0002");
				shipment.JS_HouseBill = "2222222222";
				shipment.JS_IsShipping = true;

				var job = TestObjectCreator.CreateJob(shipment, false);
				job.JH_GE = TestObjectCreator.FEADepartment.PK;
				job.ExchangeRates.AddRate(TestObjectCreator.CNY, 0.33M, debtorPk, ExchangeRateOrgTypeEnum.Creditor);
				var fEADepartmentChargeCode = TestObjectCreator.CreateChargeCode("FEACHRG", "Charge for Department Test", Constants.ChargeType.Disbursement, 0M, TestObjectCreator.GST1, TestObjectCreator.WHT1, "FEA, CEA");
				Factory.Save();

				decimal oldLocalCostAmt = 0M;
				JobCharge chargeAr = null;
				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, differentBranch1.PK.ToGuid(), TestObjectCreator.FEADepartment.PK.ToGuid()))
				{
					var jobCny = TestObjectCreator.CreateJob(shipment, false);
					jobCny.ExchangeRates.AddRate(TestObjectCreator.USD, 6.355M, debtorPk, ExchangeRateOrgTypeEnum.Creditor);
					Factory.Save();
					var aRInvoiceSource1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "121", TestObjectCreator.CNY, 1M, 3435.2M, 3435.2M, 3435.2M, 3435.2M);
					aRInvoiceSource1.AH_OH = debtorPk;
					aRInvoiceSource1.Department.GE_Misc = false;
					aRInvoiceSource1.AH_ConsolidatedInvoiceRef = "C001001";
					var aRLine1 = aRInvoiceSource1.Lines[0];
					aRLine1.AL_OSExTaxAmount = aRLine1.AL_LineAmount = 3435.2M;
					aRLine1.AL_JH = jobCny.PK;
					aRLine1.GenericCharge = fEADepartmentChargeCode.PK;
					aRLine1.AL_Desc = "Desc";
					aRLine1.AL_GB = aRInvoiceSource1.AH_GB;
					aRLine1.AL_GE = TestObjectCreator.FEADepartment.PK;
					aRLine1.AL_AT = TestObjectCreator.GSTFREE1.PK;
					chargeAr = TestObjectCreator.CreateJobCharge(aRLine1, jobCny, TestObjectCreator.CC1, TestObjectCreator.CNY);
					chargeAr.JR_OH_CostAccount = debtorPk;
					chargeAr.JR_RX_NKCostCurrency = TestObjectCreator.USD.Code;
					chargeAr.JR_OSCostExRate = 6.355M;
					chargeAr.JR_OSCostAmt = 535.01;
					chargeAr.JR_LocalCostAmt = 3400M;
					oldLocalCostAmt = chargeAr.JR_LocalCostAmt;
					Factory.Save();
				}

				job.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;
				Factory.Save();

				var cacheValue = Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed;
				using (new DisposableAction(() => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = false, () => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = cacheValue))
				using (var form = (UnapprovedTransactionAuthorisationForm)GetFormToBash())
				{
					form.Show();
					AssertEquals("Prerequisite: candidates collection should hold one item", 1, form.BusinessEntity_ForTestOnly.Candidates.Count);

					form.CandidatesGrid_ForTestOnly.Select(0);
					form.CandidatesGrid_ApproveInvoices_Click_ForTestOnly(form.CandidatesGrid_ForTestOnly, new EventArgs());
					AssertContains(@"There are some transactions with incorrect data. Please use 'Edit and Approve those invoices' to finalize the operation for them.

Reason: AP Invoice has validation errors.", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				using (new DisposableAction(() => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = true, () => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = cacheValue))
				using (var form = (UnapprovedTransactionAuthorisationForm)GetFormToBash())
				{
					form.Show();
					AssertEquals("Prerequisite: candidates collection should hold one item", 1, form.BusinessEntity_ForTestOnly.Candidates.Count);

					form.CandidatesGrid_ForTestOnly.Select(0);
					form.CandidatesGrid_ApproveInvoices_Click_ForTestOnly(form.CandidatesGrid_ForTestOnly, new EventArgs());
					Assert("Should be empty", string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
				}
			}

			public void TestEditApproveWithClaimsWithNoSecurityRight()
			{
				Helper.SetupSource();
				Helper.CreateJobAndPost(PostType.Invoice, "1");

				var expectedErrorMessage = Env.Security.PayablesClaimsAndQueriesNew.ErrorMessageForNotAllowed;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				using (UnapprovedTransactionAuthorisationForm form = (UnapprovedTransactionAuthorisationForm)this.GetFormToBash())
				{
					Env.Security.PayablesClaimsAndQueriesNew.IsAllowed = false;
					form.Show();

					form.CadidatesGrid_EditApproveTransactionsWithClaim_Click_ForTestOnly(form, EventArgs.Empty);
					AssertEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}

				Helper.ForceNewShipment();
				Helper.CreateJobAndPost(PostType.Invoice, "2");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				using (UnapprovedTransactionAuthorisationForm form = (UnapprovedTransactionAuthorisationForm)this.GetFormToBash())
				{
					Env.Security.PayablesClaimsAndQueriesNew.IsAllowed = false;
					form.Show();

					form.CandidatesGrid_ApproveInvoicesWithClaim_Click_ForTestOnly(form, EventArgs.Empty);
					AssertEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			public void TestEditApproveHandlerChoosesCorrectFormForCreditNotes()
			{
				TestEditApproveHandlerChoosesCorrectFormForInvoiceType(PostType.CreditNote);
			}

			public void TestEditApproveHandlerChoosesCorrectFormForInvoices()
			{
				TestEditApproveHandlerChoosesCorrectFormForInvoiceType(PostType.Invoice);
			}

			void TestEditApproveHandlerChoosesCorrectFormForInvoiceType(PostType postType)
			{
				GlbCompany currentCompany = GlbCompany.CurrentCompany;
				GlbBranch currentBranch = GlbBranch.CurrentBranch;

				try
				{
					Helper.SetupSource();
					Helper.ChangeCurrentCompanyViaBranch(Helper.SourceCompany, Helper.SourceCompanyBranch);
					Helper.CreateJobAndPost(postType, "1");

					Helper.ChangeCurrentCompanyViaBranch(Helper.TargetCompany, Helper.TargetBranch);
					Helper.EnsureCC1ChargeCodeInDBForCurrentCompany();
					using (UnapprovedTransactionAuthorisationForm form = (UnapprovedTransactionAuthorisationForm)this.GetFormToBash())
					{
						form.Show();
						form.CadidatesGrid_EditApproveTransactions_Click_ForTestOnly(form, EventArgs.Empty);
						if (postType == PostType.CreditNote)
						{
							Assert(ZFormModaliser.ActiveForm is CreditNoteForm);
						}
						else
						{
							Assert(ZFormModaliser.ActiveForm is InvoiceForm);
						}
						((InvoicingBase)(((BaseInvoicingForm)ZFormModaliser.ActiveForm).BusinessEntity)).ReleaseAllMutexOnInvoice();
					}
				}
				finally
				{
					Helper.ChangeCurrentCompanyViaBranch(currentCompany, currentBranch);
				}
			}

			public void TestInvoiceFormIsInEditModeWhenPoppedUp()
			{
				GlbCompany currentCompany = GlbCompany.CurrentCompany;
				GlbBranch currentBranch = GlbBranch.CurrentBranch;

				try
				{
					Helper.SetupSource();
					Helper.ChangeCurrentCompanyViaBranch(Helper.SourceCompany, Helper.SourceCompanyBranch);
					Helper.CreateJobAndPost(PostType.Invoice, "1");

					Helper.ChangeCurrentCompanyViaBranch(Helper.TargetCompany, Helper.TargetBranch);
					Helper.EnsureCC1ChargeCodeInDBForCurrentCompany();
					using (UnapprovedTransactionAuthorisationForm form = (UnapprovedTransactionAuthorisationForm)this.GetFormToBash())
					{
						form.Show();
						form.CadidatesGrid_EditApproveTransactions_Click_ForTestOnly(form, EventArgs.Empty);
						Assert(((IZForm)ZFormModaliser.ActiveForm).DisplayMode == ODisplayMode.Edit);
						ZFormModaliser.ActiveForm.Close();
					}
				}
				finally
				{
					Helper.ChangeCurrentCompanyViaBranch(currentCompany, currentBranch);
				}
			}

			public void TestSetAllowEditSecSettingToFalseWillDisableFormFieldsAndControls_Invoice()
			{
				TestSetAllowEditSecSettingToFalseWillDisableFormFieldsAndControls(PostType.Invoice);
			}

			public void TestSetAllowEditSecSettingToFalseWillDisableFormFieldsAndControls_CreditNote()
			{
				TestSetAllowEditSecSettingToFalseWillDisableFormFieldsAndControls(PostType.CreditNote);
			}

			void TestSetAllowEditSecSettingToFalseWillDisableFormFieldsAndControls(PostType postType)
			{
				var currentCompany = GlbCompany.CurrentCompany;
				var currentBranch = GlbBranch.CurrentBranch;
				InvoicingBase convertedInvoice = null;

				try
				{
					Helper.SetupSource();
					Helper.ChangeCurrentCompanyViaBranch(Helper.SourceCompany, Helper.SourceCompanyBranch);
					Helper.CreateJobAndPost(postType, "1");

					Helper.ChangeCurrentCompanyViaBranch(Helper.TargetCompany, Helper.TargetBranch);
					Helper.EnsureCC1ChargeCodeInDBForCurrentCompany();

					Env.Security.APUnapprovedInvoicesAllowEditWhenImportSisterCoInv.IsAllowed = false;
					using (UnapprovedTransactionAuthorisationForm form = (UnapprovedTransactionAuthorisationForm)this.GetFormToBash())
					{
						form.Show();
						form.CadidatesGrid_EditApproveTransactions_Click_ForTestOnly(form, EventArgs.Empty);
						var invoiceForm = ((BaseInvoicingForm)ZFormModaliser.ActiveForm);

						if (postType == PostType.Invoice)
						{
							AssertEquals("Correct type of form shown", typeof(InvoiceForm), invoiceForm.GetType());
						}
						if (postType == PostType.CreditNote)
						{
							AssertEquals("Correct type of form shown", typeof(CreditNoteForm), invoiceForm.GetType());
						}

						convertedInvoice = invoiceForm.BusinessEntity as InvoicingBase;
						AssertNotNull("BusinessEntity should be of type InvoicingBase", convertedInvoice);

						AssertEquals(true, convertedInvoice.AH_InvoiceDateInfo.ReadOnly);
						AssertEquals(true, convertedInvoice.AH_OHInfo.ReadOnly);
						AssertEquals(true, convertedInvoice.AH_DueDateInfo.ReadOnly);
						AssertEquals(true, convertedInvoice.AH_TransactionNumInfo.ReadOnly);

						if (postType == PostType.Invoice)
						{
							AssertEquals(false, ((InvoiceForm)invoiceForm).CashInvoiceOnCheckbox.Enabled);
						}

						AssertEquals(false, invoiceForm.InvoiceDetails.ApportionChargesButton.Enabled);
						AssertEquals(false, invoiceForm.InvoiceDetails.BulkChargeImportButton.Enabled);
						AssertEquals(false, invoiceForm.LineChargesGrid.Enabled);

						var menuItem = invoiceForm.Menu.MenuItems.FindByText("Auto-Allocate Discrepancy", true);
						if (menuItem != null)
						{
							AssertEquals(false, menuItem.Enabled);
						}

						menuItem = invoiceForm.Menu.MenuItems.FindByText("Save As 'Incomplete'", true);
						if (menuItem != null)
						{
							AssertEquals(false, menuItem.Enabled);
						}
						invoiceForm.Close();
					}
				}
				finally
				{
					if (convertedInvoice != null)
					{
						convertedInvoice.ReleaseAllMutexOnInvoice();
						convertedInvoice = null;
					}
					Helper.ChangeCurrentCompanyViaBranch(currentCompany, currentBranch);
				}
			}

			public void TestShouldMakeFormReadOnly()
			{
				AssertShouldMakeFormReadOnly(true, true, false);
				AssertShouldMakeFormReadOnly(true, false, false);
				AssertShouldMakeFormReadOnly(false, true, true);
				AssertShouldMakeFormReadOnly(false, false, false);
			}

			void AssertShouldMakeFormReadOnly(bool aPUnapprovedInvoicesAllowEditWhenImportSisterCoInvIsAllowed, bool isConvertedFromARInvoice, bool expectedValue)
			{
				InvoicingBase invoice = Factory.NewWithValidTestData<ARInvoice>();
				Env.Security.APUnapprovedInvoicesAllowEditWhenImportSisterCoInv.IsAllowed = aPUnapprovedInvoicesAllowEditWhenImportSisterCoInvIsAllowed;
				invoice.IsConvertedFromARInvoice = isConvertedFromARInvoice;
				using (UnapprovedTransactionAuthorisationForm form = (UnapprovedTransactionAuthorisationForm)this.GetFormToBash())
				{
					AssertEquals(expectedValue, form.ShouldMakeFormReadOnly_ForTestOnly(invoice));
				}
			}

			public void TestLineAmountsAreEditableOnInvoiceFormIsInEditMode_ForTransactionsCreatedInCurrentLoginCompany()
			{
				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(
					Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

				UAInvoice invoice = (UAInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(UAInvoice), "1234", TestObjectCreator.AUD, 1m, 10m, 1m, 10m, 1m);
				invoice.Lines[0].AL_AC = TestObjectCreator.OverheadChargeCode.PK;
				invoice.Factory.Save();

				using (UnapprovedTransactionAuthorisationForm form = (UnapprovedTransactionAuthorisationForm)GetFormToBash())
				{
					form.Show();
					form.CadidatesGrid_EditApproveTransactions_Click_ForTestOnly(form, EventArgs.Empty);

					InvoicingBase convertedInvoiceWithoutJob = ((ZForm)ZFormModaliser.ActiveForm).BusinessEntity as InvoicingBase;
					AssertNotNull("BusinessEntity should be of type InvoicingBase", convertedInvoiceWithoutJob);
					AssertEquals("Invoice should have lines", 1, convertedInvoiceWithoutJob.Lines.Count);
					AssertEquals(true, convertedInvoiceWithoutJob.Lines[0].AL_JH.IsEmpty);

					List<string> unwritablePropertiesNames = new List<string>
					{
						"GSTInclusiveAmount",
						"AL_OSTaxAmount",
						"AL_LocalTaxAmount",
						"AL_AT",
						"AL_TaxDate",
						"AL_A9_VATClass",
						"AL_ExchangeRate",
						"AL_AG",
						"AL_Sequence",
						"AL_TaxExtraRateDenominator",
						"AL_TaxExtraRateNumerator",
						"AL_TaxRateDenominator",
						"AL_TaxRateNumerator",
						"AL_AW",
						"AL_OSWHTAmount",
						"ComplianceDocumentNumber",
						"ComplianceSubType",
						"ComplianceDocumentOrganization",
						"ComplianceDocumentVATRegistrationNum",
						"ComplianceDocumentDate",
						"ComplianceDocumentReportingPeriod",
						"ComplianceDocumentSupportingReason",
						"ComplianceSupportingDocumentType",
						"ComplianceSupportingDocumentNumber",
						"AL_Calc_FirstSubClassParentId",
						"AL_Calc_SecondSubClassParentId",
						"AL_OverseasTotal",
						"AL_GB_TaxBranch"
					};

					foreach (ZPropertyInfo property in convertedInvoiceWithoutJob.Lines[0].ZPropertyInfoHash)
					{
						if (property.HasSetter)
						{
							AssertEquals(string.Format("{0} readonly status", property.Name), unwritablePropertiesNames.Contains(GetPropertyInfoDeepestName(property)), property.ReadOnly);
						}
					}
					AssertEquals(false, convertedInvoiceWithoutJob.AH_DescInfo.ReadOnly);
					AssertEquals(false, convertedInvoiceWithoutJob.AH_InvoiceDateInfo.ReadOnly);
					AssertEquals(false, convertedInvoiceWithoutJob.AH_TransactionNumInfo.ReadOnly);
					AssertEquals(false, convertedInvoiceWithoutJob.AH_OHInfo.ReadOnly);
					AssertEquals(false, convertedInvoiceWithoutJob.AH_DueDateInfo.ReadOnly);
				}

				invoice.Lines[0].AL_JH = Factory.NewJobWithValidTestDataForTesting<JobHeader>().PK;
				invoice.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
				TestObjectCreator.CreateCharge(invoice.Lines[0]);
				invoice.Factory.Save();

				InvoicingBase convertedInvoiceWithJob;
				using (UnapprovedTransactionAuthorisationForm form = (UnapprovedTransactionAuthorisationForm)GetFormToBash())
				{
					form.Show();
					form.CadidatesGrid_EditApproveTransactions_Click_ForTestOnly(form, EventArgs.Empty);

					convertedInvoiceWithJob = ((ZForm)ZFormModaliser.ActiveForm).BusinessEntity as InvoicingBase;
					AssertNotNull("BusinessEntity should be of type InvoicingBase", convertedInvoiceWithJob);
					AssertEquals("Invoice should have lines", 1, convertedInvoiceWithJob.Lines.Count);
					AssertEquals(false, convertedInvoiceWithJob.Lines[0].AL_JH.IsEmpty);

					List<string> writableInvoicePropertiesNames = new List<string> {
						"IncludeInTheBatch",
						"OSPartialPaymentAmount",
						"IncludeInThePeriodicInvoice",
						"AH_DueDate",
						"AH_InvoiceDate",
						"AH_TransactionNum",
						"AH_PostDate" ,
						"MatchStatus",
						"MatchStatusReasonCode"
					};

					CombineAssertions(() =>
					{
						foreach (ZPropertyInfo property in convertedInvoiceWithJob.ZPropertyInfoHash)
						{
							AssertEquals(string.Format("{0} readonly status", property.Name), !writableInvoicePropertiesNames.Contains(GetPropertyInfoDeepestName(property)), property.ReadOnly);
						}
					});

					convertedInvoiceWithJob.Factory.Save();
					JobCharge[] jobCharges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_AL_APLine, convertedInvoiceWithJob.Lines[0].PK));
					AssertEquals("There should be a job charge created after saving the converted invoice", 1, jobCharges.Length);
				}
				convertedInvoiceWithJob.Factory.Save();
				AssertEquals("DataSource is null.", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}

			string GetPropertyInfoDeepestName(ZPropertyInfo propertyInfo)
			{
				string propertyName = propertyInfo.Name;
				ZWrappedPropertyInfo wrappedProperty = propertyInfo as ZWrappedPropertyInfo;
				while (wrappedProperty != null)
				{
					propertyName = wrappedProperty.InnerInfo.Name;
					wrappedProperty = wrappedProperty.InnerInfo as ZWrappedPropertyInfo;
				}
				return propertyName;
			}

			public void TestLineAmountsAreEditableOnInvoiceFormIsInEditMode_ForTransactionsCreatedInCurrentLoginCompanyWithExistingJobCharge()
			{
				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(
					Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

				UAInvoice invoice = (UAInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(UAInvoice), "1234", TestObjectCreator.AUD, 1m, 10m, 1m, 10m, 1m);
				invoice.Lines[0].AL_JH = Factory.NewJobWithValidTestDataForTesting<JobHeader>().PK;
				invoice.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
				TestObjectCreator.CreateCharge(invoice.Lines[0]);
				invoice.Factory.Save();

				using (UnapprovedTransactionAuthorisationForm form = (UnapprovedTransactionAuthorisationForm)GetFormToBash())
				{
					form.Show();
					form.CadidatesGrid_EditApproveTransactions_Click_ForTestOnly(form, EventArgs.Empty);

					InvoicingBase convertedInvoice = ((ZForm)ZFormModaliser.ActiveForm).BusinessEntity as InvoicingBase;
					AssertNotNull("BusinessEntity should be of type InvoicingBase", convertedInvoice);
					AssertEquals("Invoice should have lines", 1, convertedInvoice.Lines.Count);

					List<string> writablePropertiesNames = new List<string> {
						InvoicingLineBase.Schema.AL_OSExTaxAmount,
						InvoicingLineBase.Schema.AL_OSTaxAmount,
						InvoicingLineBase.Schema.AL_LocalExTaxAmount,
						InvoicingLineBase.Schema.AL_LocalTaxAmount
					};
					IEnumerable<PropertyInfo> allLineProperties = convertedInvoice.Lines[0].GetType().GetProperties(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).Where(x => convertedInvoice.Lines[0].ZPropertyInfoHash.ContainsKey(x.Name));
					foreach (ZPropertyInfo property in convertedInvoice.Lines[0].ZPropertyInfoHash)
					{
						if (allLineProperties.Any(x => x.Name == property.Name))
						{
							AssertEquals(string.Format("{0} readonly status", property.Name), !writablePropertiesNames.Contains(property.Name), property.ReadOnly);
						}
					}
					List<string> writableInvoicePropertiesNames = new List<string> { "IncludeInTheBatch", "OSPartialPaymentAmount", "IncludeInThePeriodicInvoice", "AH_DueDate", "AH_InvoiceDate", "AH_TransactionNum", "AH_PostDate" };
					IEnumerable<PropertyInfo> allHeaderProperties = convertedInvoice.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => convertedInvoice.ZPropertyInfoHash.ContainsKey(x.Name));
					foreach (ZPropertyInfo property in convertedInvoice.ZPropertyInfoHash)
					{
						if (allHeaderProperties.Any(x => x.Name == property.Name))
						{
							AssertEquals(string.Format("{0} readonly status", property.Name), !writableInvoicePropertiesNames.Contains(property.Name), property.ReadOnly);
						}
					}
				}
			}

			public void TestGetSelectedObjectAsInvoicingBase()
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, Env.CurrentBranch.PK, TestObjectCreator.FESDepartment.PK.ToGuid()))
				{
					Helper.SetupSource();
					Helper.ChangeCurrentCompanyViaBranch(Helper.SourceCompany, Helper.SourceCompanyBranch);
					Helper.CreateJobAndPost(PostType.Invoice, "1");
					Helper.ForceNewShipment();
					Helper.CreateJobAndPost(PostType.Invoice, "2");
					Helper.ForceNewShipment();
					Helper.CreateJobAndPost(PostType.Invoice, "3");

					Helper.ChangeCurrentCompanyViaBranch(Helper.TargetCompany, Helper.TargetBranch);
					Helper.EnsureCC1ChargeCodeInDBForCurrentCompany();
					Helper.SetupTarget();
					Helper.CreateJobForTargetDebtor(PostType.Invoice, "1");
					using (UnapprovedTransactionAuthorisationForm form = (UnapprovedTransactionAuthorisationForm)this.GetFormToBash())
					{
						form.Show();
						UnapprovedTransactionConverter converter = form.BusinessEntity as UnapprovedTransactionConverter;
						AssertEquals("Pre condition: There should be 3 candidates currently", 3, converter.Candidates.Count);
						form.CandidatesGrid_ForTestOnly.SelectAllElements();
						form.CandidatesGrid_ForTestOnly.UnSelect(1);
						form.CandidatesGrid_ApproveInvoices_Click_ForTestOnly(form, EventArgs.Empty);
						form.CandidatesGrid_ForTestOnly.SelectAllElements();
						AssertEquals("There is only 1 element left because the others were converted", 1, form.CandidatesGrid_ForTestOnly.SelectedRowCount);
					}
				}
			}

			public void TestGetFirstObjectAsInvoicingBase()
			{
				GlbCompany currentCompany = GlbCompany.CurrentCompany;
				GlbBranch currentBranch = GlbBranch.CurrentBranch;

				try
				{
					Helper.SetupSource();
					Helper.ChangeCurrentCompanyViaBranch(Helper.SourceCompany, Helper.SourceCompanyBranch);
					Helper.CreateJobAndPost(PostType.Invoice, "1");

					Helper.ChangeCurrentCompanyViaBranch(Helper.TargetCompany, Helper.TargetBranch);
					Helper.EnsureCC1ChargeCodeInDBForCurrentCompany();
					Helper.SetupTarget();
					Helper.CreateJobForTargetDebtor(PostType.Invoice, "1");
					using (UnapprovedTransactionAuthorisationForm form = (UnapprovedTransactionAuthorisationForm)this.GetFormToBash())
					{
						form.Show();
						UnapprovedTransactionConverter converter = form.BusinessEntity as UnapprovedTransactionConverter;
						AssertEquals("Pre condition: There should be 1 candidate currently", 1, converter.Candidates.Count);
						form.CandidatesGrid_ForTestOnly.UnSelectAll();
						form.CandidatesGrid_ApproveInvoices_Click_ForTestOnly(form, EventArgs.Empty);
						AssertEquals("There is no one element left because the all were converted", 0, form.CandidatesGrid_ForTestOnly.List.Count);
					}
				}
				finally
				{
					Helper.ChangeCurrentCompanyViaBranch(currentCompany, currentBranch);
				}
			}

			public void TestFormCaption()
			{
				using (UnapprovedTransactionAuthorisationForm form = (UnapprovedTransactionAuthorisationForm)GetFormToBash())
				{
					AssertEquals("Form caption", "Invoice Approval Form", "Invoice Approval Form");
				}
			}

			public void TestFormVerb()
			{
				using (UnapprovedTransactionAuthorisationForm form = (UnapprovedTransactionAuthorisationForm)GetFormToBash())
				{
					AssertEquals("Form verb", string.Empty, form.FormVerb);
				}
			}

			public void TestMaxAuthorisationLevelTextBoxCaptionResourceStringFullDescription()
			{
				using (var form = (UnapprovedTransactionAuthorisationForm)GetFormToBash())
				{
					AssertEquals("Issuing Company's Maximum Variance Approval Level.", form.MaxAuthorisationLevelTextBox_ForTestOnly.CaptionResourceString.FullDescription);
				}
			}

			public void TestApproveAll()
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, Env.CurrentBranch.PK, TestObjectCreator.FESDepartment.PK.ToGuid()))
				{
					Helper.SetupSource();
					Helper.ChangeCurrentCompanyViaBranch(Helper.SourceCompany, Helper.SourceCompanyBranch);
					Helper.CreateJobAndPost(PostType.Invoice, "1");
					Helper.ForceNewShipment();
					Helper.CreateJobAndPost(PostType.Invoice, "2");

					Helper.ChangeCurrentCompanyViaBranch(Helper.TargetCompany, Helper.TargetBranch);
					Helper.EnsureCC1ChargeCodeInDBForCurrentCompany();
					Helper.SetupTarget();
					Helper.CreateJobForTargetDebtor(PostType.Invoice, "1");
					Factory.Save();
					using (UnapprovedTransactionAuthorisationForm form = (UnapprovedTransactionAuthorisationForm)GetFormToBash())
					{
						form.Show();
						AssertEquals("Prerequisite: candidates collection should hold two items", 2, form.BusinessEntity_ForTestOnly.Candidates.Count);
						AssertEquals("Prerequisite: No invoices should exist in the login company", 0, Factory.Load<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_GB, GlbBranch.CurrentBranch.PK.ToGuid()).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)).Length);
						form.CandidatesGrid_ForTestOnly.SelectAllElements();
						form.ZButtonApproveAll_ForTestOnly.PerformClick();
						AssertEquals("Two AP invoices must have been created in the login company", 2, Factory.Load<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_GB, GlbBranch.CurrentBranch.PK.ToGuid()).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)).Length);
					}
				}
			}

			public void TestApproveAllHandlesDuplicateReferenceException()
			{
				var invoice = Factory.NewWithValidTestData<APInvoice>();
				Factory.Save();
				invoice.AH_ConsolidatedInvoiceRef = "00001001";
				Factory.Save();

				using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, Env.CurrentBranch.PK, TestObjectCreator.FESDepartment.PK.ToGuid()))
				{
					Helper.SetupSource();
					Helper.ChangeCurrentCompanyViaBranch(Helper.SourceCompany, Helper.SourceCompanyBranch);
					Helper.CreateJobAndPost(PostType.Invoice, "1");

					Helper.ChangeCurrentCompanyViaBranch(Helper.TargetCompany, Helper.TargetBranch);
					Helper.EnsureCC1ChargeCodeInDBForCurrentCompany();
					Helper.SetupTarget();
					Helper.CreateJobForTargetDebtor(PostType.Invoice, "1");
					Factory.Save();
					var convertedQuery = new ZQuery(AccTransactionHeaderSchema.AH_GB, GlbBranch.CurrentBranch.PK.ToGuid()).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
					using (UnapprovedTransactionAuthorisationForm form = (UnapprovedTransactionAuthorisationForm)GetFormToBash())
					{
						form.Show();
						AssertEquals("Prerequisite: candidates collection should hold an item", 1, form.BusinessEntity_ForTestOnly.Candidates.Count);
						AssertEquals("Prerequisite: No invoices should exist in the login company", 0, Factory.Load<APInvoice>(convertedQuery).Length);
						form.CandidatesGrid_ForTestOnly.SelectAllElements();
						form.ZButtonApproveAll_ForTestOnly.PerformClick();
					}
					AssertEquals("No Converted invoice should be created", 0, Factory.Load<APInvoice>(convertedQuery).Length);
					AssertEquals("Exception for duplicate reference should not be reported", 0, ErrorReporter.TotalErrorCount);
				}
			}

			public void TestApproveAllWithAuthorization()
			{
				AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, Env.CurrentBranch.PK, TestObjectCreator.FESDepartment.PK.ToGuid()))
				{
					var shipments = new List<ForwardingShipment>();
					Helper.SetupSource();
					Helper.ChangeCurrentCompanyViaBranch(Helper.SourceCompany, Helper.SourceCompanyBranch);
					Helper.CreateJobAndPost(PostType.Invoice, "1", 500);
					shipments.Add(Helper.Shipment);
					Helper.ForceNewShipment();
					Helper.CreateJobAndPost(PostType.Invoice, "2", 2000);
					shipments.Add(Helper.Shipment);
					Helper.ForceNewShipment();
					Helper.CreateJobAndPost(PostType.CreditNote, "3", 500);
					shipments.Add(Helper.Shipment);
					Helper.ForceNewShipment();
					Helper.CreateJobAndPost(PostType.CreditNote, "4", 2000);
					shipments.Add(Helper.Shipment);
					Helper.ForceNewShipment();

					Helper.ChangeCurrentCompanyViaBranch(Helper.TargetCompany, Helper.TargetBranch);
					Helper.EnsureCC1ChargeCodeInDBForCurrentCompany();
					Helper.SetupTarget();
					Helper.CreateJobForTargetDebtor(PostType.Invoice, "1");
					Helper.SetUpTargetRegistryForTest();

					foreach (var shipment in shipments)
					{
						new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
					}

					Factory.Save();

					using (UnapprovedTransactionAuthorisationForm form = (UnapprovedTransactionAuthorisationForm)GetFormToBash())
					{
						form.Show();
						AssertEquals("Prerequisite: candidates collection should hold four items", 4, form.BusinessEntity_ForTestOnly.Candidates.Count);
						AssertEquals("Prerequisite: No invoices should exist in the login company", 0, Factory.Load<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_GB, GlbBranch.CurrentBranch.PK.ToGuid()).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)).Length);
						form.CandidatesGrid_ForTestOnly.SelectAllElements();
						form.ZButtonApproveAll_ForTestOnly.PerformClick();
						AssertEquals("2 AP invoices should be created in the login company Because Registry Values Set And User Not Authorize to Post Invoices With Amount more than 2000", 2, Factory.Load<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_GB, GlbBranch.CurrentBranch.PK.ToGuid()).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)).Length);
					}
				}
			}

			public void TestApproveSelected()
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, Env.CurrentBranch.PK, TestObjectCreator.FESDepartment.PK.ToGuid()))
				{
					Helper.SetupSource();
					Helper.ChangeCurrentCompanyViaBranch(Helper.SourceCompany, Helper.SourceCompanyBranch);
					Helper.CreateJobAndPost(PostType.Invoice, "1");
					Helper.ForceNewShipment();
					Helper.CreateJobAndPost(PostType.Invoice, "2");

					Helper.ChangeCurrentCompanyViaBranch(Helper.TargetCompany, Helper.TargetBranch);
					Helper.EnsureCC1ChargeCodeInDBForCurrentCompany();
					Helper.SetupTarget();
					Helper.CreateJobForTargetDebtor(PostType.Invoice, "1");
					using (UnapprovedTransactionAuthorisationForm form = (UnapprovedTransactionAuthorisationForm)GetFormToBash())
					{
						form.Show();
						AssertEquals("Prerequisite: candidates collection should hold two items", 2, form.BusinessEntity_ForTestOnly.Candidates.Count);
						AssertEquals("Prerequisite: No invoices should exist in the login company", 0, Factory.Load<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_GB, GlbBranch.CurrentBranch.PK.ToGuid()).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)).Length);
						form.CandidatesGrid_ForTestOnly.Select(0);
						form.CandidatesGrid_ApproveInvoices_Click_ForTestOnly(form.CandidatesGrid_ForTestOnly, new EventArgs());
						AssertEquals("One AP invoice must have been created in the login company", 1, Factory.Load<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_GB, GlbBranch.CurrentBranch.PK.ToGuid()).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)).Length);
						form.CandidatesGrid_ForTestOnly.UnSelectAll();
						form.CandidatesGrid_ApproveInvoices_Click_ForTestOnly(form.CandidatesGrid_ForTestOnly, new EventArgs());
						AssertEquals("Two AP invoice must have been created in the login company", 2, Factory.Load<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_GB, GlbBranch.CurrentBranch.PK.ToGuid()).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)).Length);
					}
				}
			}

			public void TestApproveSisterCompanyCreditNoteWithMiscellaneousDepartmentShouldGiveValidationError()
			{
				AssertEquals("Prerequisite: No transaction should exist in the login company", 0, Factory.Load<APCreditNote>(new ZQuery(AccTransactionHeaderSchema.AH_GB, GlbBranch.CurrentBranch.PK.ToGuid()).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)).Length);

				CaseApproveSisterCompanyTransactionWithMiscDepartmentShouldFail(PostType.CreditNote);

				AssertEquals("No transaction should be created as approval failed", 0, Factory.Load<APCreditNote>(new ZQuery(AccTransactionHeaderSchema.AH_GB, GlbBranch.CurrentBranch.PK.ToGuid()).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)).Length);
			}

			public void TestApproveSisterCompanyInvoiceWithMiscellaneousDepartmentShouldGiveValidationError()
			{
				AssertEquals("Prerequisite: No transaction should exist in the login company", 0, Factory.Load<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_GB, GlbBranch.CurrentBranch.PK.ToGuid()).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)).Length);

				CaseApproveSisterCompanyTransactionWithMiscDepartmentShouldFail(PostType.Invoice);

				AssertEquals("No transaction should be created as approval failed", 0, Factory.Load<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_GB, GlbBranch.CurrentBranch.PK.ToGuid()).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)).Length);
			}

			void CaseApproveSisterCompanyTransactionWithMiscDepartmentShouldFail(PostType postType)
			{
				GlbCompany currentCompany = GlbCompany.CurrentCompany;
				GlbBranch currentBranch = GlbBranch.CurrentBranch;

				try
				{
					Helper.SetupSource();
					Helper.ChangeCurrentCompanyViaBranch(Helper.SourceCompany, Helper.SourceCompanyBranch);
					Helper.CreateJobAndPost(postType, "1");

					Helper.ChangeCurrentCompanyViaBranch(Helper.TargetCompany, Helper.TargetBranch);
					Helper.EnsureCC1ChargeCodeInDBForCurrentCompany();
					Helper.SetupTarget();
					Helper.CreateJobForTargetDebtor(postType, "1", TestObjectCreator.MiscDepartment, true);

					using (UnapprovedTransactionAuthorisationForm form = (UnapprovedTransactionAuthorisationForm)GetFormToBash())
					{
						form.Show();
						AssertEquals("Prerequisite: candidates collection should hold one item", 1, form.BusinessEntity_ForTestOnly.Candidates.Count);

						form.CandidatesGrid_ForTestOnly.Select(0);
						form.CandidatesGrid_ApproveInvoices_Click_ForTestOnly(form.CandidatesGrid_ForTestOnly, new EventArgs());
						AssertContains(@"There are some transactions with incorrect data. Please use 'Edit and Approve those invoices' to finalize the operation for them.

Reason: AP Invoice has validation errors.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
				finally
				{
					Helper.ChangeCurrentCompanyViaBranch(currentCompany, currentBranch);
				}
			}

			public void TestApproveSisterCompanyTransactionFromConsolWithCannotCalculateBranchShouldFail()
			{
				ZGuid debtorPk = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

				var proxyCompany = TestObjectCreator.CreateNewCompany("PRX");
				proxyCompany.GC_OH_OrgProxy = debtorPk;

				var differentCompany = TestObjectCreator.CreateNewCompany("ABC");
				var differentBranch1 = TestObjectCreator.CreateNewBranch(differentCompany, "AB1");
				differentBranch1.GB_OH_OrgProxy = debtorPk;

				ForwardingConsol consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");

				ForwardingShipment shipment = TestObjectCreator.CreateShipment("S0000555", consol);
				Job job = TestObjectCreator.CreateJob(shipment, false);

				var fEADepartmentChargeCode = TestObjectCreator.CreateChargeCode("FEACHRG", "Charge for Department Test", Constants.ChargeType.Disbursement, 0M, TestObjectCreator.GST1, TestObjectCreator.WHT1, "FEA, CEA");
				Factory.Save();

				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, differentBranch1.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					InvoicingBase aRInvoiceSource1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "121", TestObjectCreator.AUD, 1M, 10M, 10M, 10M, 10M);
					aRInvoiceSource1.AH_OH = debtorPk;
					aRInvoiceSource1.AH_ConsolidatedInvoiceRef = "C001001";
					InvoicingLineBase aRLine1 = aRInvoiceSource1.Lines[0];
					aRLine1.AL_OSExTaxAmount = aRLine1.AL_LineAmount = -10m;
					aRLine1.AL_AT = TestObjectCreator.GSTFREE1.PK;
					aRLine1.AL_JH = job.PK;
					aRLine1.GenericCharge = fEADepartmentChargeCode.PK;
					aRLine1.AL_Desc = "Desc";
					aRLine1.AL_GB = aRInvoiceSource1.AH_GB;
					aRLine1.AL_GE = TestObjectCreator.FIADepartment.PK;
					TestObjectCreator.CreateJobCharge(aRLine1, job, TestObjectCreator.CC1, TestObjectCreator.AUD);
					Factory.Save();
				}

				using (UnapprovedTransactionAuthorisationForm form = (UnapprovedTransactionAuthorisationForm)GetFormToBash())
				{
					form.Show();
					AssertEquals("Prerequisite: candidates collection should hold one item", 1, form.BusinessEntity_ForTestOnly.Candidates.Count);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					form.CandidatesGrid_ForTestOnly.Select(0);
					form.CadidatesGrid_EditApproveTransactions_Click_ForTestOnly(form.CandidatesGrid_ForTestOnly, new EventArgs());
					AssertContains(@"Do you want to set current branch instead?", UnitTestUserNotification.Instance.LastMessage.Text);

					using (var editForm = ZFormModaliser.ActiveForm)
					{
						var invoiceForm = editForm as InvoiceForm;
						AssertNotNull("Must be an invoice form", invoiceForm);
						AssertEquals(((InvoicingBase)invoiceForm.BusinessEntity).AH_GB, GlbBranch.CurrentBranch.PK);
						editForm.Close();
					}

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

					form.CadidatesGrid_EditApproveTransactions_Click_ForTestOnly(form.CandidatesGrid_ForTestOnly, new EventArgs());
					using (var editForm = ZFormModaliser.ActiveForm)
					{
						var invoiceForm = editForm as InvoiceForm;
						AssertNull("Invoice form should not be shown", invoiceForm);
					}
					AssertEquals("invoice should not be approved", 1, form.BusinessEntity_ForTestOnly.Candidates.Count);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					form.CandidatesGrid_ApproveInvoices_Click_ForTestOnly(form.CandidatesGrid_ForTestOnly, new EventArgs());
					AssertContains("Intercompany Invoice cannot be imported as Transaction Branch or Company cannot be set with reference to the invoice debtor organization proxy", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("invoice should not be approved", 1, form.BusinessEntity_ForTestOnly.Candidates.Count);
				}
			}

			public void TestApproveInvoiceWithoutGuiHaveNotBranch()
			{
				var debtorPk = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
				var proxyCompany = TestObjectCreator.CreateNewCompany("PRX");
				proxyCompany.GC_OH_OrgProxy = debtorPk;

				var chinaCompany = TestObjectCreator.CreateNewCompany("ABC");
				var chinaBranch = TestObjectCreator.CreateNewBranch(chinaCompany, "AB1");
				chinaBranch.GB_OH_OrgProxy = debtorPk;

				var australiaCompany = TestObjectCreator.CreateNewCompany("AUC");
				var australiaBranch = TestObjectCreator.CreateNewBranch(australiaCompany, "AB2");
				australiaBranch.GB_OH_OrgProxy = debtorPk;

				var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");

				var shipment = TestObjectCreator.CreateShipment("S0000555", consol);
				var job = TestObjectCreator.CreateJob(shipment, false);

				var fEADepartmentChargeCode = TestObjectCreator.CreateChargeCode("FEACHRG", "Charge for Department Test", Constants.ChargeType.Disbursement, 0M, TestObjectCreator.GST1, TestObjectCreator.WHT1, "FEA, CEA");
				Factory.Save();

				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, chinaBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var aRInvoiceSource1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "121", TestObjectCreator.AUD, 1M, 10M, 10M, 10M, 10M);
					aRInvoiceSource1.AH_OH = debtorPk;
					aRInvoiceSource1.AH_ConsolidatedInvoiceRef = "C001001";
					var aRLine1 = aRInvoiceSource1.Lines[0];
					aRLine1.AL_OSExTaxAmount = aRLine1.AL_LineAmount = -10m;
					aRLine1.AL_AT = TestObjectCreator.GSTFREE1.PK;
					aRLine1.AL_JH = job.PK;
					aRLine1.GenericCharge = fEADepartmentChargeCode.PK;
					aRLine1.AL_Desc = "Desc";
					aRLine1.AL_GB = aRInvoiceSource1.AH_GB;
					aRLine1.AL_GE = TestObjectCreator.FIADepartment.PK;
					TestObjectCreator.CreateJobCharge(aRLine1, job, TestObjectCreator.CC1, TestObjectCreator.AUD);
					Factory.Save();
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
				using (var form = (UnapprovedTransactionAuthorisationForm)GetFormToBash())
				{
					form.Show();
					AssertEquals("Prerequisite: candidates collection should hold one item", 1, form.BusinessEntity_ForTestOnly.Candidates.Count);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNoExceptionThrown(() => form.CandidatesGrid_ApproveInvoices_Click_ForTestOnly(form.CandidatesGrid_ForTestOnly, new EventArgs()));
					AssertEquals("invoice should not be approved", 1, form.BusinessEntity_ForTestOnly.Candidates.Count);
				}
			}

			public void TestWhenApproveSisterCompanyTransactionFromConsolShowLocalAmoutError()
			{
				var rate = AccTaxRate.Helper.FindTaxRate(new BusinessObjectFactory(), AccTaxRate.Helper.MainNotReportableTaxRegistryID, Env.CurrentCompanyPK);
				rate.SetRateNumerator_ForTestOnly(0);
				rate.Factory.Save();

				var differentCompany = TestObjectCreator.CreateNewCompany("ABC");
				GlbCompany.CurrentCompany.SetCurrency("USD");
				differentCompany.SetCurrency("CNY");

				differentCompany.GC_IsReciprocal = true;
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;
				var differentBranchOrgProxy = TestObjectCreator.CreateOrgHeader("ORGPROXYB", true, true);
				var differentBranch1 = TestObjectCreator.CreateNewBranch(differentCompany, "AB1");
				differentBranch1.GB_OH_OrgProxy = differentBranchOrgProxy.PK;
				var debtorPk = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

				var shipment = TestObjectCreator.CreateShipment("S0002");
				shipment.JS_HouseBill = "2222222222";
				shipment.JS_IsShipping = true;

				var job = TestObjectCreator.CreateJob(shipment, false);
				job.JH_GE = TestObjectCreator.FEADepartment.PK;
				job.ExchangeRates.AddRate(TestObjectCreator.CNY, 0.33M, debtorPk, ExchangeRateOrgTypeEnum.Creditor);
				var fEADepartmentChargeCode = TestObjectCreator.CreateChargeCode("FEACHRG", "Charge for Department Test", Constants.ChargeType.Disbursement, 0M, TestObjectCreator.GST1, TestObjectCreator.WHT1, "FEA, CEA");
				Factory.Save();

				decimal oldLocalCostAmt = 0M;
				JobCharge chargeAr = null;
				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, differentBranch1.PK.ToGuid(), TestObjectCreator.FEADepartment.PK.ToGuid()))
				{
					var jobCny = TestObjectCreator.CreateJob(shipment, false);
					jobCny.ExchangeRates.AddRate(TestObjectCreator.USD, 6.355M, debtorPk, ExchangeRateOrgTypeEnum.Creditor);
					Factory.Save();
					var aRInvoiceSource1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "121", TestObjectCreator.CNY, 1M, 3435.2M, 3435.2M, 3435.2M, 3435.2M);
					aRInvoiceSource1.AH_OH = debtorPk;
					aRInvoiceSource1.Department.GE_Misc = false;
					aRInvoiceSource1.AH_ConsolidatedInvoiceRef = "C001001";
					var aRLine1 = aRInvoiceSource1.Lines[0];
					aRLine1.AL_OSExTaxAmount = aRLine1.AL_LineAmount = 3435.2M;
					aRLine1.AL_JH = jobCny.PK;
					aRLine1.GenericCharge = fEADepartmentChargeCode.PK;
					aRLine1.AL_Desc = "Desc";
					aRLine1.AL_GB = aRInvoiceSource1.AH_GB;
					aRLine1.AL_GE = TestObjectCreator.FEADepartment.PK;
					aRLine1.AL_AT = TestObjectCreator.GSTFREE1.PK;
					chargeAr = TestObjectCreator.CreateJobCharge(aRLine1, jobCny, TestObjectCreator.CC1, TestObjectCreator.CNY);
					chargeAr.JR_OH_CostAccount = debtorPk;
					chargeAr.JR_RX_NKCostCurrency = TestObjectCreator.USD.Code;
					chargeAr.JR_OSCostExRate = 6.355M;
					chargeAr.JR_OSCostAmt = 535.01;
					chargeAr.JR_LocalCostAmt = 3400M;
					oldLocalCostAmt = chargeAr.JR_LocalCostAmt;
					Factory.Save();
				}
				using (UnapprovedTransactionAuthorisationForm form = (UnapprovedTransactionAuthorisationForm)GetFormToBash())
				{
					form.Show();
					AssertEquals("Prerequisite: candidates collection should hold one item", 1, form.BusinessEntity_ForTestOnly.Candidates.Count);
					form.CandidatesGrid_ForTestOnly.Select(0);

					form.CadidatesGrid_EditApproveTransactions_Click_ForTestOnly(form.CandidatesGrid_ForTestOnly, new EventArgs());
					using (Form editForm = ZFormModaliser.ActiveForm)
					{
						var invoiceForm = editForm as InvoiceForm;
						AssertNotNull("Must be an invoice form", invoiceForm);
						AssertEquals(((InvoicingBase)invoiceForm.BusinessEntity).AH_GB, GlbBranch.CurrentBranch.PK);
						var invoice = invoiceForm.BusinessEntity as APInvoice;
						invoice.AH_ExchangeRate = 0.33;
						invoice.AH_OSTotalAmount = 3435.2M;
						invoiceForm.PostingButtonsUserControl.SaveButton.PerformClick();
						Assert(UnitTestUserNotification.Instance.LastMessage.Text, string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
						AssertEquals(oldLocalCostAmt, chargeAr.JR_LocalCostAmt);
						editForm.Close();
					}
				}
			}

			public void TestApproveSisterCompanyTransactionFromConsolWithDepartmentNotInChargeFilterListShouldFail()
			{
				var differentCompany = TestObjectCreator.CreateNewCompany("ABC");
				var differentBranchOrgProxy = TestObjectCreator.CreateOrgHeader("ORGPROXYB", true, true);
				var differentBranch1 = TestObjectCreator.CreateNewBranch(differentCompany, "AB1");
				differentBranch1.GB_OH_OrgProxy = differentBranchOrgProxy.PK;

				ForwardingConsol consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");

				ForwardingShipment shipment = TestObjectCreator.CreateShipment("S0000555", consol);
				Job job = TestObjectCreator.CreateJob(shipment, false);

				var fEADepartmentChargeCode = TestObjectCreator.CreateChargeCode("FEACHRG", "Charge for Department Test", Constants.ChargeType.Disbursement, 0M, TestObjectCreator.GST1, TestObjectCreator.WHT1, "FEA, CEA");
				Factory.Save();

				ZGuid debtorPk = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, differentBranch1.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					InvoicingBase aRInvoiceSource1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "121", TestObjectCreator.AUD, 1M, 10M, 10M, 10M, 10M);
					aRInvoiceSource1.AH_OH = debtorPk;
					aRInvoiceSource1.Department.GE_Misc = false;
					aRInvoiceSource1.AH_ConsolidatedInvoiceRef = "C001001";
					InvoicingLineBase aRLine1 = aRInvoiceSource1.Lines[0];
					aRLine1.AL_OSExTaxAmount = aRLine1.AL_LineAmount = -10m;
					aRLine1.AL_AT = TestObjectCreator.GSTFREE1.PK;
					aRLine1.AL_JH = job.PK;
					aRLine1.GenericCharge = fEADepartmentChargeCode.PK;
					aRLine1.AL_Desc = "Desc";
					aRLine1.AL_GB = aRInvoiceSource1.AH_GB;
					aRLine1.AL_GE = TestObjectCreator.FIADepartment.PK;
					TestObjectCreator.CreateJobCharge(aRLine1, job, TestObjectCreator.CC1, TestObjectCreator.AUD);

					Factory.Save();
				}

				using (UnapprovedTransactionAuthorisationForm form = (UnapprovedTransactionAuthorisationForm)GetFormToBash())
				{
					form.Show();
					AssertEquals("Prerequisite: candidates collection should hold one item", 1, form.BusinessEntity_ForTestOnly.Candidates.Count);

					form.CandidatesGrid_ForTestOnly.Select(0);
					form.CandidatesGrid_ApproveInvoices_Click_ForTestOnly(form.CandidatesGrid_ForTestOnly, new EventArgs());
					AssertContains(@"There are some transactions with incorrect data. Please use 'Edit and Approve those invoices' to finalize the operation for them.

Reason: AP Invoice has validation errors.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			public void TestEditAndApproveSisterCompanyInvoiceWithoutNewInvoiceRights()
			{
				GlbCompany currentCompany = GlbCompany.CurrentCompany;
				GlbBranch currentBranch = GlbBranch.CurrentBranch;

				Env.Security.NewPayablesInvoice.IsAllowed = false;
				try
				{
					Helper.SetupSource();
					Helper.ChangeCurrentCompanyViaBranch(Helper.SourceCompany, Helper.SourceCompanyBranch);
					Helper.CreateJobAndPost(PostType.Invoice, "1");

					Helper.ChangeCurrentCompanyViaBranch(Helper.TargetCompany, Helper.TargetBranch);
					Helper.EnsureCC1ChargeCodeInDBForCurrentCompany();
					Helper.SetupTarget();
					Helper.CreateJobForTargetDebtor(PostType.Invoice, "1");
					using (UnapprovedTransactionAuthorisationForm form = (UnapprovedTransactionAuthorisationForm)GetFormToBash())
					{
						form.Show();
						AssertEquals("Prerequisite: candidates collection should hold one item", 1, form.BusinessEntity_ForTestOnly.Candidates.Count);
						AssertEquals("Prerequisite: No invoices should exist in the login company", 0, Factory.Load<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_GB, GlbBranch.CurrentBranch.PK.ToGuid()).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)).Length);
						form.CandidatesGrid_ForTestOnly.Select(0);
						try
						{
							form.CadidatesGrid_EditApproveTransactions_Click_ForTestOnly(form.CandidatesGrid_ForTestOnly, new EventArgs());
							Fail("Exception should be thrown");
						}
						catch (RethrownByExceptionHandlerException ex)
						{
							AssertEquals("A new form should not have been created.", typeof(SecurityAccessDeniedException), ex.InnerException.GetType());
						}
						AssertEquals("No AP invoices must have been created in the login company", 0, Factory.Load<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_GB, GlbBranch.CurrentBranch.PK.ToGuid()).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)).Length);
					}
				}
				finally
				{
					Helper.ChangeCurrentCompanyViaBranch(currentCompany, currentBranch);
				}
			}

			public void TestDataSourceSetToNullUnexpectedly()
			{
				GlbCompany currentCompany = GlbCompany.CurrentCompany;
				GlbBranch currentBranch = GlbBranch.CurrentBranch;

				Env.Security.ReopenJob.IsAllowed = false;
				try
				{
					Helper.SetupSource();
					Helper.ChangeCurrentCompanyViaBranch(Helper.SourceCompany, Helper.SourceCompanyBranch);
					Helper.CreateJobAndPost(PostType.Invoice, "1");
					Helper.ChangeCurrentCompanyViaBranch(Helper.TargetCompany, Helper.TargetBranch);
					Helper.EnsureCC1ChargeCodeInDBForCurrentCompany();
					Helper.SetupTarget();

					var job2 = Helper.CreateJobForTargetDebtor(PostType.Invoice, "1");
					job2.Close(null, null);
					job2.Factory.Save();

					UnapprovedTransactionAuthorisationForm form;
					using (form = (UnapprovedTransactionAuthorisationForm)GetFormToBash())
					{
						form.Show();
						AssertNotNull(form.BusinessEntity);
						AssertEquals("Prerequisite: candidates collection should hold one item", 1, form.BusinessEntity_ForTestOnly.Candidates.Count);
						((CargoWise.Windows.UI.KForm)form).DataSource = null;
						AssertNull(form.BusinessEntity);
						AssertEquals("UnapprovedTransactionAuthorisationForm.ReportSetNullDataSource", ErrorReporter.LastKeyReported);
						Assert(ErrorReporter.LastMessageReported.StartsWith("Data Source is set to null, call stack:"));
						ErrorReporter.Clear();
					}
				}
				finally
				{
					Helper.ChangeCurrentCompanyViaBranch(currentCompany, currentBranch);
				}
			}

			public void TestEditAndApproveSisterCompanyInvoiceWithoutReopenInvoiceRights()
			{
				GlbCompany currentCompany = GlbCompany.CurrentCompany;
				GlbBranch currentBranch = GlbBranch.CurrentBranch;

				Env.Security.ReopenJob.IsAllowed = false;
				try
				{
					Helper.SetupSource();
					Helper.ChangeCurrentCompanyViaBranch(Helper.SourceCompany, Helper.SourceCompanyBranch);
					Helper.CreateJobAndPost(PostType.Invoice, "1");
					Helper.ChangeCurrentCompanyViaBranch(Helper.TargetCompany, Helper.TargetBranch);
					Helper.EnsureCC1ChargeCodeInDBForCurrentCompany();
					Helper.SetupTarget();

					var job2 = Helper.CreateJobForTargetDebtor(PostType.Invoice, "1");
					job2.Close(null, null);
					job2.Factory.Save();

					UnapprovedTransactionAuthorisationForm form;
					using (form = (UnapprovedTransactionAuthorisationForm)GetFormToBash())
					{
						form.Show();
						AssertEquals("Prerequisite: candidates collection should hold one item", 1, form.BusinessEntity_ForTestOnly.Candidates.Count);
						AssertEquals("Prerequisite: No invoices should exist in the login company", 0, Factory.Load<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_GB, GlbBranch.CurrentBranch.PK.ToGuid()).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)).Length);
						form.CandidatesGrid_ForTestOnly.Select(0);
						form.ZButtonApproveAll_Click_ForTestOnly(form, new EventArgs());
						AssertContains(@"There are some transactions with incorrect data. Please use 'Edit and Approve those invoices' to finalize the operation for them.

Reason: You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Job Costing -> Job Management -> Reopen Jobs", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals("No AP invoices must have been created in the login company", 0, Factory.Load<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_GB, GlbBranch.CurrentBranch.PK.ToGuid()).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)).Length);
					}
					form.HandleInvalidTransactions_ForTestOnly(new List<InvoicingBase>(), false, false, false, "");
					AssertEquals("DataSource is null.", ErrorReporter.LastMessageReported);
					ErrorReporter.Clear();
				}
				finally
				{
					Helper.ChangeCurrentCompanyViaBranch(currentCompany, currentBranch);
				}
			}

			public void TestEditAndApproveUnapprovedInvoiceWithoutNewInvoiceRights()
			{
				Env.Security.NewPayablesInvoice.IsAllowed = false;

				InvoicingBase approval = Factory.NewWithValidTestData<UAInvoice>();
				Factory.Save();

				using (UnapprovedTransactionAuthorisationForm form = (UnapprovedTransactionAuthorisationForm)GetFormToBash())
				{
					form.Show();
					Application.DoEvents();
					AssertEquals("Prerequisite: candidates collection should hold one item", 1, form.BusinessEntity_ForTestOnly.Candidates.Count);
					form.CandidatesGrid_ForTestOnly.Select(0);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					form.CadidatesGrid_EditApproveTransactions_Click_ForTestOnly(form.CandidatesGrid_ForTestOnly, new EventArgs());

					using (var editForm = ZFormModaliser.GetActiveChildFormForParentForm(form))
					{
						AssertNotNull("Must be an InvoiceForm", editForm as InvoiceForm);
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
						editForm.Close();
					}
					form.Close();
					Application.DoEvents();
				}

				AssertContains(@"This record has been modified.
Would you like to save the changes?", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			public void TestApproveInvoiceWithoutGuiReleasesMutexesForInvalidInvoices()
			{
				GlbCompany currentCompany = GlbCompany.CurrentCompany;
				GlbBranch currentBranch = GlbBranch.CurrentBranch;

				try
				{
					Helper.SetupSource();
					Helper.ChangeCurrentCompanyViaBranch(Helper.SourceCompany, Helper.SourceCompanyBranch);
					Helper.CreateConsolAndPost();

					Helper.ChangeCurrentCompanyViaBranch(Helper.TargetCompany, Helper.TargetBranch);
					//AssertEquals("Make sure CC4 exists in the Target Company", GlbCompany.CurrentCompany.PK, TestObjectCreator.CC4.AC_GC);
					TestObjectCreator.Factory.Save();
					Helper.SetupTarget();
					TestObjectCreator.CC4.AC_IsActive = false;
					TestObjectCreator.Factory.Save();
					using (UnapprovedTransactionAuthorisationForm form = (UnapprovedTransactionAuthorisationForm)GetFormToBash())
					{
						form.Show();
						AssertEquals("Prerequisite: candidates collection should hold one items", 1, form.BusinessEntity_ForTestOnly.Candidates.Count);
						AssertEquals("Prerequisite: No invoices should exist in the login company", 0, Factory.Load<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_GB, GlbBranch.CurrentBranch.PK.ToGuid()).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)).Length);
						form.CandidatesGrid_ForTestOnly.SelectAllElements();
						form.ZButtonApproveAll_ForTestOnly.PerformClick();
						AssertEquals("No AP invoices must have been created in the login company", 0, Factory.Load<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_GB, GlbBranch.CurrentBranch.PK.ToGuid()).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)).Length);

					ZArchitecture.Data.Mutex.ZGlobalMutex mutex = new ZArchitecture.Data.Mutex.ZGlobalMutex(MutexIDs.JobBeingCreatedForShipment, Helper.Shipment.PK + "_" + GlbCompany.CurrentCompany.GC_Code);
						AssertEquals("Should be released", false, mutex.IsLocked);
					}
				}
				finally
				{
					Helper.ChangeCurrentCompanyViaBranch(currentCompany, currentBranch);
				}
			}

		[TestDate(2022, 03, 23)]
		public void TestApproveInvoiceWithoutGuiForComplianceSequence()
		{
			Helper.SetupSource();
			Helper.SourceProxy.CompanyData.SetAPTaxApplicableIgnoringRegistrySetting(true);

			var currCompany = Helper.TargetCompany;
			var currBranch = Helper.TargetBranch;

			var differentCompany = Helper.SourceCompany;
			var differentBranch = Helper.SourceCompanyBranch;

			var chargeCode1 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1, "ALL", differentCompany);
			Factory.Save();

			var dept = TestObjectCreator.NonCurrentDepartment;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch.PK.ToGuid(), dept.PK.ToGuid()))
			{
				var job = new Job.Loader(Helper.Shipment).TryCreateWithoutMutexForTestOnly();
				job.JH_JobNum = "1";
				job.JH_GC = differentCompany.PK;
				job.JH_GB = differentBranch.PK;
				job.JH_GE = dept.PK;
				job.LocalChargesPK = Helper.SourceDebtor.PK;
				job.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;
				Factory.Save();

				var charge = job.Charges.AddNew();
				charge.JR_AC = chargeCode1.PK;
				charge.JR_OH_SellAccount = Helper.SourceDebtor.PK;
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				charge.JR_Desc = "Test Charge";
				charge.JR_LocalSellAmt = 50;
				charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
				{
					charge.RunPreSaveValidation();
					Assert(!charge.HasRowErrors);
					Assert(!charge.HasErrors);
					job.RunPreSaveValidation();
					Assert(!job.HasRowErrors);
					Assert(!job.HasErrors);
				}
				Factory.Save();

				var postManager = new InvoicingPostManager(job);
				postManager.Poster.Post(charge);
				Factory.Save();
				AssertEquals("Posting created and invoice", 1, postManager.Poster.PostedInvoices.Count);
			}

			using (currCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, currBranch.PK.ToGuid(), dept.PK.ToGuid()))
			{
				var countryCode = currCompany.Country.Code;
				var taxRate = TestObjectCreator.CreateTaxRate("NOT", "Not reportable", AccTaxRate.Types.NotReportable, 0, ZString.Empty, 0, 1, countryCode);
				var taxRate1 = TestObjectCreator.CreateTaxRate("NOT", "Not Reportable", AccTaxRate.Types.NotReportable, 10, AccTaxRate.ExtraTypes.StateGST, 0, 1, countryCode);
				var taxRate2 = TestObjectCreator.CreateTaxRate("NOT", "Not Reportable", AccTaxRate.Types.NotReportable, 10, AccTaxRate.ExtraTypes.ServiceTax, 0, 1, countryCode);
				AssertNotNull(taxRate);
				AssertNotNull(taxRate1);
				AssertNotNull(taxRate2);
				var chargeCode2 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Constants.ChargeType.Margin, 100, taxRate, TestObjectCreator.WHTFREE1, "ALL", currCompany);

				const string subType = "APS";
				var sequence = TestObjectCreator.CreateNewComplianceSequence(ZGuid.Empty, subType, 1, 99, 25);
				sequence.XD_Prefix = "APS-";
				sequence.XD_GC_Company = currCompany.PK;
				sequence.XD_GB_BranchOwner = currBranch.PK;
				Factory.Save();

				var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
				var config = collection.AddNew();
				config.Country = countryCode;
				config.SubType = subType;
				config.LedgerType = LedgerTypes.AccountsPayable;
				config.InvoiceType = TransactionTypes.Invoice;
				config.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				config.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				config.OriginalRule = OriginalRuleCodes.AllTransactions;

				var registry = AccountingMasterFilesRegistry.Instance;
				var currCompGuid = currCompany.PK.ToGuid();
				using (registry.ComplianceSubTypeAttributionRuleConfiguration.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, collection))
				using (registry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code))
				using (registry.ComplianceDocumentNumberAllocation_Payables.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
				using (var form = (UnapprovedTransactionAuthorisationForm)GetFormToBash())
				{
					form.Show();

					var uap = (InvoicingBase)form.BusinessEntity_ForTestOnly.Candidates.FirstOrDefault();
					AssertNotNull("Prerequisite: candidates collection should hold 1 item", uap);
					var query = new ZQuery(AccTransactionHeaderSchema.AH_GB, currBranch.PK).AddToFilter(AccTransactionHeaderSchema.AH_GC, currCompany.PK);
					AssertNull("Prerequisite: No invoices should exist in the login company", Factory.LoadTop1<APInvoice>(query));
					form.CandidatesGrid_ForTestOnly.SelectAllElements();
					form.ZButtonApproveAll_ForTestOnly.PerformClick();
					AssertNotNull("One AP invoice must have been created in the login company", Factory.LoadTop1<APInvoice>(query));
				}
			}
		}

		public void TestApproveSisterCompanyInvoiceWithClaim()
			{
				GlbCompany currentCompany = GlbCompany.CurrentCompany;
				GlbBranch currentBranch = GlbBranch.CurrentBranch;

				CostVarianceApproval valuesForTest = new CostVarianceApproval();
				valuesForTest.VarianceCalculationStyle = Core.Constants.CostVarianceCalculationStyle.LocalExTaxAmount;
				valuesForTest.VarianceComparisonOption = Core.Constants.CostVarianceComparisonOption.Job;
				CostVarianceApprovalAuthorisationRequirement upTo1 = valuesForTest.AuthorisationRequirements.AddNew();
				upTo1.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
				upTo1.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
				upTo1.Amount = 1M;
				CostVarianceApprovalAuthorisationRequirement above1 = valuesForTest.AuthorisationRequirements.AddNew();
				above1.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
				above1.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
				above1.Amount = 1M;
				AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

				IntercompanyPostingConfigurationCollection postingConfigCollection = new IntercompanyPostingConfigurationCollection();
				IntercompanyPostingConfiguration postingConfig = postingConfigCollection.AddNew();
				postingConfig.Company = Helper.SourceCompany.GC_Code;
				postingConfig.MaxCostVarianceApprovalLevel = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
				AccountingConfigurationRegistry.Instance.IntercompanyPostingConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, postingConfigCollection);

				try
				{
					Helper.SetupSource();
					Helper.ChangeCurrentCompanyViaBranch(Helper.SourceCompany, Helper.SourceCompanyBranch);
					Helper.CreateJobAndPost(PostType.Invoice, "1");

					Helper.ChangeCurrentCompanyViaBranch(Helper.TargetCompany, Helper.TargetBranch);
					Helper.EnsureCC1ChargeCodeInDBForCurrentCompany();
					Helper.SetupTarget();
					Helper.CreateJobForTargetDebtor(PostType.Invoice, "1");
					using (UnapprovedTransactionAuthorisationForm form = (UnapprovedTransactionAuthorisationForm)GetFormToBash())
					{
						form.Show();
						AssertEquals("Prerequisite: candidates collection should hold one item", 1, form.BusinessEntity_ForTestOnly.Candidates.Count);
						form.CandidatesGrid_ForTestOnly.Select(0);
						form.CandidatesGrid_ApproveInvoicesWithClaim_Click_ForTestOnly(form.CandidatesGrid_ForTestOnly, new EventArgs());
						AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals("Candidates_ForTestOnly.Count", 1, form.BusinessEntity_ForTestOnly.Candidates.Count);
						var uaCreditNote = form.BusinessEntity_ForTestOnly.Candidates[0] as UACreditNote;
						uaCreditNote.AH_TransactionNum = "Test";
						AssertNotNull("UACreditNote should be created", uaCreditNote);
						uaCreditNote.RunPreSaveValidation();
						AssertNoErrors("All UACreditNote fields should be valid", uaCreditNote);
						var relatedClaim = uaCreditNote.RelatedClaim;
						AssertNotNull("Related Claim should be created", relatedClaim);
						relatedClaim.RunPreSaveValidation();
						AssertNoErrors("All Claim fields should be valid", relatedClaim);
					}
				}
				finally
				{
					Helper.ChangeCurrentCompanyViaBranch(currentCompany, currentBranch);
				}
			}

			[TestDate(2013, 12, 10)]
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
			public void TestApproveSisterCompanyInvoiceWithClaimErrorMessages()
			{
				Env.SetUserContext(new UserContext(GlbStaff.CurrentUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), TestObjectCreator.FESDepartment.PK.ToGuid()));

				GlbCompany currentCompany = GlbCompany.CurrentCompany;
				GlbBranch currentBranch = GlbBranch.CurrentBranch;

				CostVarianceApproval valuesForTest = new CostVarianceApproval();
				valuesForTest.VarianceCalculationStyle = Core.Constants.CostVarianceCalculationStyle.LocalExTaxAmount;
				valuesForTest.VarianceComparisonOption = Core.Constants.CostVarianceComparisonOption.Job;
				CostVarianceApprovalAuthorisationRequirement upTo1 = valuesForTest.AuthorisationRequirements.AddNew();
				upTo1.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
				upTo1.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
				upTo1.Amount = 1M;
				CostVarianceApprovalAuthorisationRequirement above1 = valuesForTest.AuthorisationRequirements.AddNew();
				above1.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
				above1.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
				above1.Amount = 1M;
				AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

				try
				{
					Helper.SetupSource();
					Helper.ChangeCurrentCompanyViaBranch(Helper.SourceCompany, Helper.SourceCompanyBranch);
					Helper.CreateJobAndPost(PostType.Invoice, "1");
					Helper.ChangeCurrentCompanyViaBranch(Helper.TargetCompany, Helper.TargetBranch);
					Helper.EnsureCC1ChargeCodeInDBForCurrentCompany();
					Helper.SetupTarget();
					Helper.CreateJobForTargetDebtor(PostType.Invoice, "1");
					using (UnapprovedTransactionAuthorisationForm form = (UnapprovedTransactionAuthorisationForm)GetFormToBash())
					{
						form.Show();
						AssertEquals("Prerequisite: candidates collection should hold one item", 1, form.BusinessEntity_ForTestOnly.Candidates.Count);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						var testManager = new PeriodManager(Factory);
						testManager.DeleteAllPeriodsInCurrentCompany();
						form.CandidatesGrid_ForTestOnly.Select(0);
						form.CandidatesGrid_ApproveInvoicesWithClaim_Click_ForTestOnly(form.CandidatesGrid_ForTestOnly, new EventArgs());
						AssertEquals("AP Invoice validation error should be shown.",
@"Some invoices have not been approved with claim. 
This option should only be used when approving sister company AR invoices with a required Variance Approval Level greater than that sister company's defined Maximum Variance Approval Level. 
Please use 'Edit and Approve' to approve those invoices.

Reason: AP Invoice has validation errors.",
							UnitTestUserNotification.Instance.LastMessage.Text);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

						TestObjectCreator.CreateTestPeriods(new ZDateTime(ZDateTime.Now.Year, 1, 1));
						Factory.Save();
						form.CandidatesGrid_ForTestOnly.Select(0);
						form.CandidatesGrid_ApproveInvoicesWithClaim_Click_ForTestOnly(form.CandidatesGrid_ForTestOnly, new EventArgs());
						AssertEquals("Claim can't be created error should be shown.",
@"Some invoices have not been approved with claim. 
This option should only be used when approving sister company AR invoices with a required Variance Approval Level greater than that sister company's defined Maximum Variance Approval Level. 
Please use 'Edit and Approve' to approve those invoices.

Reason: A claim can't be created. Please check appropriate security settings.",
							UnitTestUserNotification.Instance.LastMessage.Text);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

						Helper.SourceProxy.Contacts.RemoveAndDeleteAll();
						Helper.SourceProxy.Contacts.Factory.Save();
						IntercompanyPostingConfigurationCollection postingConfigCollection = new IntercompanyPostingConfigurationCollection();
						IntercompanyPostingConfiguration postingConfig = postingConfigCollection.AddNew();
						postingConfig.Company = Helper.SourceCompany.GC_Code;
						postingConfig.MaxCostVarianceApprovalLevel = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
						AccountingConfigurationRegistry.Instance.IntercompanyPostingConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, postingConfigCollection);
						Factory.Save();
						form.CandidatesGrid_ForTestOnly.Select(0);
						form.CandidatesGrid_ApproveInvoicesWithClaim_Click_ForTestOnly(form.CandidatesGrid_ForTestOnly, new EventArgs());
						AssertEquals("Claim validation error should be shown.",
@"Some invoices have not been approved with claim. 
This option should only be used when approving sister company AR invoices with a required Variance Approval Level greater than that sister company's defined Maximum Variance Approval Level. 
Please use 'Edit and Approve' to approve those invoices.

Reason: A claim has validation errors.",
							UnitTestUserNotification.Instance.LastMessage.Text);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

						var contact = Helper.SourceProxy.Contacts.AddNew();
						var document = contact.Documents.AddNew();
						document.OD_DocumentGroup = ContactType.Payables.Code;
						document.OD_DefaultContact = true;
						contact.Factory.Save();
						form.CandidatesGrid_ForTestOnly.Select(0);
						form.CandidatesGrid_ApproveInvoicesWithClaim_Click_ForTestOnly(form.CandidatesGrid_ForTestOnly, new EventArgs());
						AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals("Candidates_ForTestOnly.Count", 1, form.BusinessEntity_ForTestOnly.Candidates.Count);
						var uaCreditNote = form.BusinessEntity_ForTestOnly.Candidates[0] as UACreditNote;
						uaCreditNote.AH_TransactionNum = "Test";
						AssertNotNull("UACreditNote should be created", uaCreditNote);
						uaCreditNote.RunPreSaveValidation();
						AssertNoErrors("All UACreditNote fields should be valid", uaCreditNote);
						var relatedClaim = uaCreditNote.RelatedClaim;
						AssertNotNull("Related Claim should be created", relatedClaim);
						relatedClaim.RunPreSaveValidation();
						AssertNoErrors("All Claim fields should be valid", relatedClaim);
					}
				}
				finally
				{
					Helper.ChangeCurrentCompanyViaBranch(currentCompany, currentBranch);
				}
			}

			public void TestDocumentMenuIsRemovedFromGridContextMenu()
			{
				using (UnapprovedTransactionAuthorisationForm testForm = (UnapprovedTransactionAuthorisationForm)GetFormToBash())
				{
					testForm.Show();
					testForm.CandidatesGrid_ForTestOnly.ContextMenu.MenuItems.Add("Documents");
					testForm.CandidatesGridContextMenu_Popup_ForTestOnly(this, null);
					bool doesDocumentsMenuExist = false;
					for (int i = 0; i < testForm.CandidatesGrid_ForTestOnly.ContextMenu.MenuItems.Count; i++)
					{
						if (testForm.CandidatesGrid_ForTestOnly.ContextMenu.MenuItems[i].Text == "Documents")
						{
							doesDocumentsMenuExist = true;
						}
					}
					Assert("Menu Item 'Documents' must be deleted", !doesDocumentsMenuExist);
				}
			}

			public void TestOutstandingAmountCaption()
			{
				using (var form = (UnapprovedTransactionAuthorisationForm)GetFormToBash())
				{
					form.Show();
					Application.DoEvents();

					var grid = form.CandidatesGrid_ForTestOnly;
					var columnName = "AH_OutstandingAmount";
					var columnInfo = grid.ColumnStyles.Cast<Core.Forms.ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == columnName);
					AssertNotNull(columnInfo);
					AssertNull(columnInfo.CaptionResourceString.Caption);
					AssertEquals("Outstanding Amount", grid.Columns[columnName].ColumnStyle.HeaderText);
				}
			}

			#region Implementation

			TestObjectCreator TestObjectCreator
			{
				get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
			}
			TestObjectCreator fTestObjectCreator;

			#endregion
		}
}
