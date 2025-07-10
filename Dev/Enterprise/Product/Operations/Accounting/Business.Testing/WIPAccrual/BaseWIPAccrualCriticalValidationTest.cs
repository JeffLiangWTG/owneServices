using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.WIPAccrual.Testing
{
	public class BaseWIPAccrualCriticalValidationTest : AccTransactionLinesCriticalValidationTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:Do Not Cast Factory Method", Justification = "Baseline")]
		protected override List<TestCaseDefinitionWithDelegate_Obsolete> GetTestCases()
		{
			List<TestCaseDefinitionWithDelegate_Obsolete> result = new List<TestCaseDefinitionWithDelegate_Obsolete>();
			SetUpData();
			var reverseDate_cachedForDelegates = ZDateTime.Today;
			foreach (Type type in new[] { typeof(WIP), typeof(Accrual) })
			{
				Type type_cachedForDelegates = type;
				foreach (bool fail in new[] { true, false })
				{
					bool fail_cachedForDelegates = fail;
					string lineType = type == typeof(WIP) ? TransactionLineTypes.WIP : TransactionLineTypes.Accrual;
					result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("CheckWIPAccrualLinkedToJobChargeUnlessItIsReversed, type = {0}, linked to JobCharge = {1}", type.Name, !fail), factory =>
					{
						BaseWIPAccrual parent = (BaseWIPAccrual)factory.New(type_cachedForDelegates, new Guid("bb288034-7636-42f0-a089-3e7bda00e5f4"));
						parent.FillWithValidTestData();
						parent.AL_LineType = lineType;
						parent.AL_PostDate = ZDateTime.Empty;
						parent.AL_RX_NKTransactionCurrency = "AUD";
						if (!fail_cachedForDelegates)
						{
							JobCharge charge = factory.NewWithValidTestData<JobCharge>();
							if (type_cachedForDelegates == typeof(WIP))
							{
								charge.JR_AL_ARLine = parent.PK;
							}

							if (type_cachedForDelegates == typeof(Accrual))
							{
								charge.JR_AL_APLine = parent.PK;
							}

							parent.AL_JH = charge.JR_JH;
						}

						return parent;
					}

					, fail, CriticalValidationErrorType.ReversedTransactionLineShouldBeOrNotLinkedToJobCharge_3, string.Format("{0} should be linked to a Job Charge unless it is Reversed", lineType), string.Format("Line: PK = bb288034-7636-42f0-a089-3e7bda00e5f4, Charge Code = , GL Account = , Type = {0}, OS Amount = 0, Local Amount = 0, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 0, Currency = AUD, Post Date = , Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = 00000000-0000-0000-0000-000000000000, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Revenue Recognition Type = , Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.", lineType)));
					result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("CheckWIPAccrualLinkedToJobChargeUnlessItIsReversed, type = {0}, reversed and linked to JobCharge = {1}", type.Name, fail), factory =>
					{
						BaseWIPAccrual parent = (BaseWIPAccrual)factory.New(type_cachedForDelegates, new Guid("0241d4fb-6dd9-48e7-968b-f3c5c7e1c4d8"));
						parent.FillWithValidTestData();
						parent.AL_LineType = lineType;
						parent.AL_PostDate = ZDateTime.Empty;
						parent.AL_RX_NKTransactionCurrency = "AUD";
						JobCharge charge = (JobCharge)factory.New(typeof(JobCharge), new Guid("1fd3a92c-009f-4994-bc17-a2de1f9b669a"));
						charge.FillWithValidTestData();
						var job = factory.NewJobWithPrimaryKeyForTesting<JobHeader>(new Guid("31a84607-9d67-4d7f-9d56-bbd6730828c0"));
						job.FillWithValidTestData();
						charge.JR_JH = job.PK;
						if (fail_cachedForDelegates)
						{
							if (type_cachedForDelegates == typeof(WIP))
							{
								charge.JR_AL_ARLine = parent.PK;
							}

							if (type_cachedForDelegates == typeof(Accrual))
							{
								charge.JR_AL_APLine = parent.PK;
							}

							parent.AL_JH = charge.JR_JH;
						}

						using (SkipReportingWhenReversedWIPACRLinkedToJobChargeAttribute.ActivateTemporary())
						{
							parent.AL_ReverseDate = reverseDate_cachedForDelegates;
						}
						return parent;
					}

					, fail, CriticalValidationErrorType.ReversedTransactionLineShouldBeOrNotLinkedToJobCharge_3, string.Format("reversed {0} should not be linked to a Job Charge", lineType), "Charge: PK = 1fd3a92c-009f-4994-bc17-a2de1f9b669a",
string.Format("Line: PK = 0241d4fb-6dd9-48e7-968b-f3c5c7e1c4d8, Charge Code = , GL Account = , Type = {0}, OS Amount = 0, Local Amount = 0, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 0, Currency = AUD, Post Date = , Reverse Date = {1}, Post To GL = N, Reverse To GL = N, Header PK = 00000000-0000-0000-0000-000000000000, Job PK = 31a84607-9d67-4d7f-9d56-bbd6730828c0, Organization = , Revenue Recognition Type = IMM, Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.", lineType, reverseDate_cachedForDelegates.ToAUString())));
					result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("CheckWIPAccrualRelatedToSameJobAsLinkedJobCharge, type = {0}, linked to JobCharge and refers the same Job = {1}", type.Name, !fail), factory =>
					{
						BaseWIPAccrual parent = (BaseWIPAccrual)factory.New(type_cachedForDelegates, new Guid("0241d4fb-6dd9-48e7-968b-f3c5c7e1c4d8"));
						parent.FillWithValidTestData();
						parent.AL_LineType = lineType;
						parent.AL_PostDate = ZDateTime.Empty;
						parent.AL_RX_NKTransactionCurrency = "AUD";
						JobCharge charge = (JobCharge)factory.New(typeof(JobCharge), new Guid("1fd3a92c-009f-4994-bc17-a2de1f9b669a"));
						charge.FillWithValidTestData();
						var job = factory.NewJobWithPrimaryKeyForTesting<JobHeader>(new Guid("31a84607-9d67-4d7f-9d56-bbd6730828c0"));
						job.FillWithValidTestData();
						charge.JR_JH = job.PK;
						if (type_cachedForDelegates == typeof(WIP))
						{
							charge.JR_AL_ARLine = parent.PK;
						}

						if (type_cachedForDelegates == typeof(Accrual))
						{
							charge.JR_AL_APLine = parent.PK;
						}

						if (!fail_cachedForDelegates)
						{
							parent.AL_JH = charge.JR_JH;
						}

						return parent;
					}

					, fail, CriticalValidationErrorType.LineShouldBeRelatedToSameJobAsLinkedJobCharge_3, string.Format("{0} should be related to the same Job as a linked Job Charge", lineType), "PK = 1fd3a92c-009f-4994-bc17-a2de1f9b669a",
"PK = 0241d4fb-6dd9-48e7-968b-f3c5c7e1c4d8"));
					result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("CheckNewRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount, type = {0}, amounts the same = {1}", type.Name, !fail), factory =>
					{
						BaseWIPAccrual parent = (BaseWIPAccrual)factory.New(type_cachedForDelegates, new Guid("fcb2c006-ca45-4106-94a0-35227aaae854"));
						parent.FillWithValidTestData();
						parent.AL_LineType = lineType;
						parent.AL_AH = ZGuid.Empty;
						parent.AL_RX_NKTransactionCurrency = "AUD";
						JobCharge charge = (JobCharge)factory.New(typeof(JobCharge), new Guid("1e62b4e5-84bc-48c5-af62-b2182aa37281"));
						charge.FillWithValidTestData();
						var job = factory.NewJobWithPrimaryKeyForTesting<JobHeader>(new Guid("4caede15-eccf-4b28-a850-7aff85959630"));
						job.FillWithValidTestData();
						charge.JR_JH = job.PK;
						if (type_cachedForDelegates == typeof(WIP))
						{
							charge.JR_LocalSellAmt = 10M;
							charge.JR_AL_ARLine = parent.PK;
							ARInvoiceLine cfxline = (ARInvoiceLine)factory.New(typeof(ARInvoiceLine), new Guid("2a9ff46c-b344-4987-8ba5-6f343c501432"));
							cfxline.FillWithValidTestData();
							cfxline.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
							cfxline.AL_LineAmount = -6M;
							charge.JR_AL_CFXLine = cfxline.PK;
							if (!fail_cachedForDelegates)
							{
								parent.AL_LocalExTaxAmount = 4M;
							}
						}

						if (type_cachedForDelegates == typeof(Accrual))
						{
							charge.JR_LocalCostAmt = 10M;
							charge.JR_AL_APLine = parent.PK;
							if (!fail_cachedForDelegates)
							{
								parent.AL_LocalExTaxAmount = 10M;
							}
						}

						parent.AL_JH = charge.JR_JH;
						return parent;
					}

					, fail, CriticalValidationErrorType.LineShouldHaveSameAmountAsJobCharge_11, string.Format("Related {0} amount is not the same as charge amount.", lineType), "PK = 1e62b4e5-84bc-48c5-af62-b2182aa37281", "PK = fcb2c006-ca45-4106-94a0-35227aaae854"));
					result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("CheckWIPAccrualCurrencyIsLocalOne, type = {0}, currency is local = {1}", type.Name, !fail), factory =>
					{
						BaseWIPAccrual parent = (BaseWIPAccrual)factory.New(type_cachedForDelegates, new Guid("fcb2c006-ca45-4106-94a0-35227aaae854"));
						parent.FillWithValidTestData();
						parent.AL_LineType = lineType;
						parent.AL_AH = ZGuid.Empty;
						parent.AL_LocalExTaxAmount = 10M;
						JobCharge charge = (JobCharge)factory.New(typeof(JobCharge), new Guid("1e62b4e5-84bc-48c5-af62-b2182aa37281"));
						charge.FillWithValidTestData();
						var job = factory.NewJobWithPrimaryKeyForTesting<JobHeader>(new Guid("4caede15-eccf-4b28-a850-7aff85959630"));
						job.FillWithValidTestData();
						job.JH_GC = Env.CurrentCompany.PK;
						charge.JR_JH = job.PK;
						parent.AL_JH = charge.JR_JH;
						if (type_cachedForDelegates == typeof(WIP))
						{
							charge.JR_LocalSellAmt = 10M;
							charge.JR_AL_ARLine = parent.PK;
						}

						if (type_cachedForDelegates == typeof(Accrual))
						{
							charge.JR_LocalCostAmt = 10M;
							charge.JR_AL_APLine = parent.PK;
						}

						if (!fail_cachedForDelegates)
						{
							parent.AL_RX_NKTransactionCurrency = "AUD";
						}

						AssertNotNull("Precondition: job must have company set.", job.Company);
						return parent;
					}

					, fail, CriticalValidationErrorType.LineCurrencyShouldHaveSameCurrencyAsLocalCurrency_3, string.Format("{0} currency is not the same as local currency.", lineType), string.Format("Line: PK = fcb2c006-ca45-4106-94a0-35227aaae854, Charge Code = , GL Account = , Type = {0}, OS Amount = 0, Local Amount = {1}10, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 0, Currency = , Post Date = , Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = 00000000-0000-0000-0000-000000000000, Job PK = 4caede15-eccf-4b28-a850-7aff85959630, Organization = , Revenue Recognition Type = IMM, Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.", lineType, lineType == TransactionLineTypes.WIP ? "-" : "")));
				}

				result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("DoNotCheckSavedAndNotChangedWIPAccrualLinkedToJobChargeUnlessItIsReversed, type = {0}", type.Name), factory =>
				{
					BaseWIPAccrual parent = null;
					string column = null;
					if (type_cachedForDelegates == typeof(WIP))
					{
						parent = TestWIP;
						column = JobChargeSchema.Constants.JR_AL_ARLine;
					}

					if (type_cachedForDelegates == typeof(Accrual))
					{
						parent = TestAccrual;
						column = JobChargeSchema.Constants.JR_AL_APLine;
					}

					ClearChargeRefColumn(TestCharge.PK, column);
					return parent;
				}

				));
				result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("DoNotCheckSavedAndNotChangedWIPAccrualRelatedToSameJobAsLinkedJobCharge, type = {0}", type.Name), factory =>
				{
					BaseWIPAccrual parent = null;
					if (type_cachedForDelegates == typeof(WIP))
					{
						parent = TestWIP;
					}

					if (type_cachedForDelegates == typeof(Accrual))
					{
						parent = TestAccrual;
					}

					JobHeader anotherJob = FactoryForSaving.NewJobWithValidTestDataForTesting<JobHeader>();
					FactoryForSaving.Save();
					DbCommand command = ((IDbConnected)factory).Connection.Command(string.Format("update dbo.JobCharge set {0} = @JobPK, JR_SystemLastEditTimeUtc = GETUTCDATE(), JR_SystemLastEditUser = '~BP' where JR_PK = @ChargePK", JobChargeSchema.Constants.JR_JH));
					command.AddParameterBasedOnDbColumn("@JobPK", anotherJob.PK.ToGuid(), JobHeaderSchema.PK);
					command.AddParameterBasedOnDbColumn("@ChargePK", TestCharge.PK.ToGuid(), JobChargeSchema.PK);
					command.ExecuteNonQuery();
					return parent;
				}

				));
			}

			return result;
		}

		public void TestCheckWIPAccrualLinkedToJobCharge_ChargeIsSavedByFactoryIsFalse()
		{
			ForwardingShipment shipment;
			Charge charge;
			SetupDataForIsSavedByFactoryTests(out shipment, out charge);
			AssertNotNull(charge.WIP);
			var reloadedCharge = Factory.Load<Charge>(charge.PK);
			var reloadedWIP = Factory.Load<WIP>(reloadedCharge.WIP.PK);
			Assert(!reloadedCharge.JR_AL_ARLineInfo.HasChanges);
			Assert(!reloadedCharge.IsSavedByFactory);
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckWIPAccrualLinkedToJobCharge_ChargeIsSavedByFactoryIsFalseAndARLineHasNoChanges");
			AssertOnSavingCheck(reloadedWIP, testCase);
			reloadedCharge.ReverseWIP(ZDateTime.Now);
			var newWIP = Factory.NewWithValidTestData<WIP>();
			newWIP.AL_AG = new TestObjectCreator(Factory).GLHeader1.PK;
			reloadedCharge.JR_AL_ARLine = newWIP.PK;
			var reloadedShipment = Factory.Load<ForwardingShipment>(shipment.PK);
			reloadedShipment.JS_IsForwardRegistered = false;
			reloadedCharge.Job.Parent = reloadedShipment;
			((IBusinessObjectInternals)reloadedShipment).MarkAsDeleted();
			Assert(reloadedCharge.JR_AL_ARLineInfo.HasChanges);
			Assert(!reloadedCharge.IsSavedByFactory);
			var userErrorMessage = @"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: WIP should be linked to a Job Charge unless it is Reversed";
			var techDetails = @"Developer Details (Critical Validation Failure): 

WIP should be linked to a Job Charge unless it is Reversed";
			var chargeMessage = string.Format("Charge: PK = {0}", reloadedCharge.PK);
			var lineMessage = string.Format("Line: PK = {0}", newWIP.PK);

			testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckWIPAccrualLinkedToJobCharge_ChargeIsSavedByFactoryIsFalseAndARLineHasChanges", true, CriticalValidationErrorType.ReversedTransactionLineShouldBeOrNotLinkedToJobCharge_3, userErrorMessage, techDetails, chargeMessage, lineMessage);
			AssertOnSavingCheck(newWIP, testCase);
		}

		public void TestCheckWIPAccrualRelatedToSameJobAsLinkedJobCharge_ChargeIsSavedByFactoryIsFalse()
		{
			ForwardingShipment shipment;
			Charge charge;
			SetupDataForIsSavedByFactoryTests(out shipment, out charge);
			AssertNotNull(charge.WIP);
			var reloadedCharge = Factory.Load<Charge>(charge.PK);
			var reloadedWIP = Factory.Load<WIP>(reloadedCharge.WIP.PK);
			Assert(!reloadedCharge.JR_JHInfo.HasChanges);
			Assert(!reloadedCharge.IsSavedByFactory);
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckWIPAccrualRelatedToSameJobAsLinkedJobCharge_ChargeIsSavedByFactoryIsFalseAndJR_JHHasNoChanges");
			AssertOnSavingCheck(reloadedWIP, testCase);
			var reloadedShipment = Factory.Load<ForwardingShipment>(shipment.PK);
			var newJobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			((Job)newJobHeader).PlugInData = reloadedShipment;
			reloadedCharge.JR_JH = newJobHeader.PK;
			reloadedWIP.AL_JH = newJobHeader.PK;
			reloadedShipment.JS_IsForwardRegistered = false;
			((IBusinessObjectInternals)reloadedShipment).MarkAsDeleted();
			Assert(reloadedCharge.JR_JHInfo.HasChanges);
			Assert(!reloadedCharge.IsSavedByFactory);
			var userErrorMessage = @"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: WIP should be related to the same Job as a linked Job Charge";
			var techDetails1 = @"Developer Details (Critical Validation Failure): 

WIP should be related to the same Job as a linked Job Charge

Charge: PK = ";
			var techDetails2 = $"	Fields with changes: JR_JH ({charge.JR_JH}, {reloadedCharge.JR_JH}).";
			var techDetails3 = "Line: PK = ";
			var techDetails4 = $"	Fields with changes: AL_GE (57f778c1-daf6-46e0-b7dc-af01c161c936, 86bb1c22-0865-4685-996e-d56cbd136491), AL_JH ({charge.JR_JH}, {reloadedCharge.JR_JH}).";
			testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckWIPAccrualRelatedToSameJobAsLinkedJobCharge_ChargeIsSavedByFactoryIsFalseAndJR_JHHasChanges", true, CriticalValidationErrorType.LineShouldBeRelatedToSameJobAsLinkedJobCharge_3, userErrorMessage, techDetails1, techDetails2, techDetails3, techDetails4);
			AssertOnSavingCheck(reloadedWIP, testCase);
		}

		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestCheckRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_ChargeChangedByInCorrectCompany()
		{
			using (AccountingConfigurationRegistry.Instance.AutoImportIntercompanyInvoices.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			using (AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var newFactoryAvoidCaching = new BusinessObjectFactory();
				var testObjectCreator = new TestObjectCreator(newFactoryAvoidCaching);
				var debtorOrg = testObjectCreator.CreateOrgHeader("ABCPROXY", true, true);
				var debtorCompany = testObjectCreator.CreateNewCompany("ABC");
				debtorCompany.GC_RX_NKLocalCurrency = "TWD";
				debtorCompany.GC_Name = "GC_Name";
				debtorCompany.GC_OH_OrgProxy = debtorOrg.PK;
				var debtorBranch = testObjectCreator.CreateNewBranch(debtorCompany, "BR1");
				AssertEquals("Precondition: current company local decimals", 2, GlbCompany.CurrentCompany.GetLocalDecimals());
				AssertEquals("Precondition: other company local decimals", 0, debtorCompany.GetLocalDecimals());

				SetupDataForIsSavedByFactoryTests(out var _, out var chargeTemp, false);
				var charge = newFactoryAvoidCaching.Load<Charge>(chargeTemp.PK);

				var exchangeRate = charge.InvoicingJob.ExchangeRates.AddNew();
				exchangeRate.JF_BaseRate = 1.23555m;
				exchangeRate.JF_RX_NKRateCurrency = "USD";
				exchangeRate.JF_CFXPercent = 2M;
				exchangeRate.JF_OH_Org = debtorOrg.PK;
				exchangeRate.JF_OrgType = ExchangeRateOrgTypeEnum.Debtor.ToCode();

				charge.JR_OH_SellAccount = debtorOrg.PK;
				charge.JR_RX_NKSellCurrency = "AUD";
				charge.JR_RX_NKSellInvoiceCurrency = "USD";
				charge.SellCurrency.RX_SubUnitRatio = 100;
				charge.SellInvoiceCurrency.RX_SubUnitRatio = 100;
				charge.JR_LineCFX = 2M;
				charge.JR_OSSellAmt = 196.0M;

				newFactoryAvoidCaching.Save();

				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, debtorBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					var chargeLoadedByInCorrectCompany = Factory.Load<ChargeWithCost>(chargeTemp.PK);

					var oldWIP = charge.WIP;

					((IBusinessObjectInternals)chargeLoadedByInCorrectCompany).Row[nameof(Charge.JR_GE)] = testObjectCreator.NonCurrentDepartment.PK.ToGuid();
					chargeLoadedByInCorrectCompany.Job.HasChanges = true;
					chargeLoadedByInCorrectCompany.InvoicingJob.Charges.ForEach(c => c.HasChanges = true);
					AssertNoExceptionThrown("Critical Validation is not triggered because JR_LocalSellInvoiceAmt is calculated using Charge Company local decimals", () => Factory.Save());
					AssertNotEquals("A new WIP should be created successfully", oldWIP.PK, chargeLoadedByInCorrectCompany.WIP.PK);
				}
			}
		}

		public void TestCheckRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_ChargeLinkedToNewWIPWithDifferentAmount()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(Guid.NewGuid(), CriticalValidationInfoCollectorServiceKeyType.LineLocalAmountNotEqualRelatedJobChargeLocalSellAmount);

			ForwardingShipment shipment;
			Charge charge;
			SetupDataForIsSavedByFactoryTests(out shipment, out charge);
			AssertNotNull(charge.WIP);
			var previousWipPK = charge.WIP.PK;

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_ChargeLinkedToNewWIPWithDifferentAmount");
			AssertOnSavingCheck(charge.WIP, testCase);
			AssertEquals("AL_LocalExTaxAmount", 50M, charge.WIP.AL_LocalExTaxAmount);

			var wip = charge.Factory.NewWithValidTestData<WIP>();
			wip.AL_AG = charge.WIP.AL_AG;
			charge.ReverseWIP(ZDateTime.Now);
			wip.AL_JH = charge.JR_JH;
			wip.AL_AC = charge.JR_AC;
			wip.AL_OH = charge.JR_OH_SellAccount;

			charge.JR_AL_ARLine = wip.PK;
			wip.AL_OSAmount = 51m;
			wip.AL_LocalExTaxAmount = 51m;

			var userErrorMessage = @"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: Related WIP amount is not the same as charge amount.";

			var techDetails = @"Developer Details (Critical Validation Failure): 

Related WIP amount is not the same as charge amount.

50 - 0 = 50 != 51";
			var chargeMessage = $"PK = {charge.PK}";
			var fieldsWithChanges = $"Fields with changes: JR_AL_ARLine ({previousWipPK}, {wip.PK}).";
			var lineMessage = $"PK = {wip.PK}";
			var previousWipMessage = "Previous Line:";
			var previousWipPKMessage = $"PK = {previousWipPK}";
			var additionalInfo1 = "JobChargeLocalSellAmtNotEqualRelatedWIPAmount: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.";
			var additionalInfo2 = "LineLocalAmountNotEqualRelatedJobChargeLocalSellAmount:";
			var stackTrace = "Stacktrace -->";

			testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_ChargeLinkedToNewWIPWithDifferentAmount", true, CriticalValidationErrorType.LineShouldHaveSameAmountAsJobCharge_11, userErrorMessage, techDetails, chargeMessage, fieldsWithChanges, lineMessage, previousWipMessage, previousWipPKMessage, additionalInfo1, additionalInfo2, stackTrace);
			AssertOnSavingCheck(wip, testCase);
		}

		public void TestCheckRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_ChargeLinkedToNewWIPWithDifferentAmount_ChargeLinkedAfterSettingWIPAmount()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(Guid.NewGuid(), CriticalValidationInfoCollectorServiceKeyType.LineLocalAmountNotEqualRelatedJobChargeLocalSellAmount);

			ForwardingShipment shipment;
			Charge charge;
			SetupDataForIsSavedByFactoryTests(out shipment, out charge);
			AssertNotNull(charge.WIP);
			var previousWipPK = charge.WIP.PK;

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_ChargeLinkedToNewWIPWithDifferentAmount");
			AssertOnSavingCheck(charge.WIP, testCase);
			AssertEquals("AL_LocalExTaxAmount", 50M, charge.WIP.AL_LocalExTaxAmount);

			var wip = charge.Factory.NewWithValidTestData<WIP>();
			wip.AL_AG = charge.WIP.AL_AG;
			charge.ReverseWIP(ZDateTime.Now);
			wip.AL_JH = charge.JR_JH;
			wip.AL_AC = charge.JR_AC;
			wip.AL_OH = charge.JR_OH_SellAccount;

			wip.AL_OSAmount = 51m;
			wip.AL_LocalExTaxAmount = 51m;

			charge.JR_AL_ARLine = wip.PK;

			var userErrorMessage = @"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: Related WIP amount is not the same as charge amount.";

			var techDetails = @"Developer Details (Critical Validation Failure): 

Related WIP amount is not the same as charge amount.

50 - 0 = 50 != 51";
			var chargeMessage = $"PK = {charge.PK}";
			var fieldsWithChanges = $"Fields with changes: JR_AL_ARLine ({previousWipPK}, {wip.PK}).";
			var lineMessage = $"PK = {wip.PK}";
			var previousWipMessage = "Previous Line:";
			var previousWipPKMessage = $"PK = {previousWipPK}";
			var additionalInfo1 = "JobChargeLocalSellAmtNotEqualRelatedWIPAmount: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.";
			var additionalInfo2 = "LineLocalAmountNotEqualRelatedJobChargeLocalSellAmount:";
			var stackTrace = "Related charge is not linked yet. AL_LineAmount: -51";

			testCase = new TestCaseDefinition_ForSeparateTestsMethods("TestCheckRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_ChargeLinkedToNewWIPWithDifferentAmount_ChargeLinkedAfterSettingWIPAmount", true, CriticalValidationErrorType.LineShouldHaveSameAmountAsJobCharge_11, userErrorMessage, techDetails, chargeMessage, fieldsWithChanges, lineMessage, previousWipMessage, previousWipPKMessage, additionalInfo1, additionalInfo2, stackTrace);
			AssertOnSavingCheck(wip, testCase);
		}

		public void TestCheckNewRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_ChargeIsSavedByFactoryIsFalse_WIP_LocalSellAmt()
		{
			ForwardingShipment shipment;
			Charge charge;
			SetupDataForIsSavedByFactoryTests(out shipment, out charge);
			AssertNotNull(charge.WIP);
			var reloadedCharge = Factory.Load<Charge>(charge.PK);
			var reloadedWIP = Factory.Load<WIP>(reloadedCharge.WIP.PK);
			reloadedCharge.JR_OSSellAmt = 20m;
			reloadedWIP.AL_LineAmount = -20m;
			reloadedWIP.AL_OSAmount = -20m;
			Assert(reloadedWIP.AL_LineAmountInfo.HasChanges);
			Assert(reloadedCharge.JR_LocalSellAmtInfo.HasChanges);
			Assert(reloadedCharge.IsSavedByFactory);
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckNewRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_ChargeIsSavedByFactoryIsTrueFalseAndJR_LocalSellAmtHasChanges_WIP");
			AssertOnSavingCheck(reloadedWIP, testCase);
			var reloadedShipment = Factory.Load<ForwardingShipment>(shipment.PK);
			reloadedShipment.JS_IsForwardRegistered = false;
			((IBusinessObjectInternals)reloadedShipment).MarkAsDeleted();
			Assert(reloadedWIP.AL_LineAmountInfo.HasChanges);
			Assert(reloadedCharge.JR_LocalSellAmtInfo.HasChanges);
			Assert(!reloadedCharge.IsSavedByFactory);
			var userErrorMessage = @"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: Related WIP amount is not the same as charge amount.";
			var techDetails1 = @"Developer Details (Critical Validation Failure): 

Related WIP amount is not the same as charge amount.

Amounts are equal, but changed charge will not be saved in db.
Charge: PK = ";
			var techDetails2 = "	Fields with changes: JR_AgentDeclaredSellAmt (50.0000, 20), JR_LocalSellAmt (50.0000, 20), JR_OSSellAmt (50.0000, 20).";
			var techDetails3 = "Line: PK = ";
			var techDetails4 = "	Fields with changes: AL_LineAmount (-50.0000, -20), AL_OSAmount (-50.0000, -20).";
			testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckNewRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_ChargeIsSavedByFactoryIsFalseAndJR_LocalSellAmtHasChanges_WIP", true, CriticalValidationErrorType.LineShouldHaveSameAmountAsJobCharge_11, userErrorMessage, techDetails1, techDetails2, techDetails3, techDetails4);
			AssertOnSavingCheck(reloadedWIP, testCase);
		}

		public void TestCheckNewRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_ChargeIsSavedByFactoryIsFalse_Accrual()
		{
			ForwardingShipment shipment;
			Charge charge;
			SetupDataForIsSavedByFactoryTests(out shipment, out charge);
			AssertNotNull(charge.Accrual);
			var reloadedCharge = Factory.Load<Charge>(charge.PK);
			var reloadedAccrual = Factory.Load<Accrual>(reloadedCharge.Accrual.PK);
			reloadedCharge.JR_OSCostAmt = 20m;
			reloadedAccrual.AL_LineAmount = 20m;
			reloadedAccrual.AL_OSAmount = 20m;
			Assert(reloadedAccrual.AL_LineAmountInfo.HasChanges);
			Assert(reloadedCharge.JR_LocalCostAmtInfo.HasChanges);
			Assert(reloadedCharge.IsSavedByFactory);
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckNewRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_ChargeIsSavedByFactoryIsTruendJR_LocalCostAmtHasChanges_Accrual");
			AssertOnSavingCheck(reloadedAccrual, testCase);
			var reloadedShipment = Factory.Load<ForwardingShipment>(shipment.PK);
			reloadedShipment.JS_IsForwardRegistered = false;
			((IBusinessObjectInternals)reloadedShipment).MarkAsDeleted();
			Assert(reloadedAccrual.AL_LineAmountInfo.HasChanges);
			Assert(reloadedCharge.JR_LocalCostAmtInfo.HasChanges);
			Assert(!reloadedCharge.IsSavedByFactory);
			var userErrorMessage = @"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: Related ACR amount is not the same as charge amount.";
			var techDetails = @"Developer Details (Critical Validation Failure): 

Related ACR amount is not the same as charge amount.

Amounts are equal, but changed charge will not be saved in db.";
			var chargeMessage = ZString.Format(@"Charge: PK = {0}", reloadedCharge.PK);
			var lineMessage = ZString.Format(@"Line: PK = {0}", reloadedAccrual.PK);
			testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckNewRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_ChargeIsSavedByFactoryIsFalseAndJR_LocalCostAmtHasChanges_Accrual", true, CriticalValidationErrorType.LineShouldHaveSameAmountAsJobCharge_11, userErrorMessage, techDetails, chargeMessage, lineMessage);
			AssertOnSavingCheck(reloadedAccrual, testCase);
		}

		public void TestCheckNewRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_ChargeIsSavedByFactoryIsFalse_WIP_CFXAmt_WithCFXLine()
		{
			var creationFactory = new BusinessObjectFactory();
			var objectCreator = new TestObjectCreator(creationFactory);
			var shipment = objectCreator.CreateShipment("S0001001", false);
			var job = objectCreator.CreateJob(shipment, objectCreator.LocalClient, 0m, objectCreator.Agent, 0m);
			var charge = job.Charges.AddNew();
			charge.JR_AC = objectCreator.CC1.PK;
			charge.JR_OSSellAmt = 50m;
			creationFactory.Save();
			var cfxLine = creationFactory.NewWithPrimaryKey<ARInvoiceLine>(new Guid("2a9ff46c-b344-4987-8ba5-6f343c501432"));
			cfxLine.FillWithValidTestData();
			cfxLine.AL_AH = creationFactory.New<ARInvoice>().PK;
			cfxLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			cfxLine.AL_LineAmount = -6M;
			cfxLine.AL_AG = objectCreator.GLHeader1.PK;
			charge.JR_AL_CFXLine = cfxLine.PK;
			creationFactory.Save();
			AssertNotNull(charge.WIP);
			var reloadedCharge = Factory.Load<Charge>(charge.PK);
			var reloadedWIP = Factory.Load<WIP>(reloadedCharge.WIP.PK);
			var reloadedCFXLine = Factory.Load<ARInvoiceLine>(cfxLine.PK);
			reloadedCFXLine.AL_LineAmount = -8m;
			reloadedWIP.AL_LineAmount = -42m;
			reloadedWIP.AL_OSAmount = -42m;
			Assert(reloadedWIP.AL_LineAmountInfo.HasChanges);
			Assert(reloadedCharge.IsCFXPosted);
			Assert(!reloadedCharge.JR_AL_CFXLineInfo.HasChanges);
			Assert(!reloadedCharge.IsSavedByFactory);
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckNewRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_ChargeIsSavedByFactoryIsFalseAndJR_AL_CFXLineHasNoChanges_WIP_CFXAmt");
			AssertOnSavingCheck(reloadedWIP, testCase);
			var newCFXLine = Factory.NewWithPrimaryKey<ARInvoiceLine>(new Guid("fcdd446f-c0f5-44e4-ac1f-c8f5bf4b3c27"));
			newCFXLine.FillWithValidTestData();
			newCFXLine.AL_AH = Factory.New<ARInvoice>().PK;
			newCFXLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			newCFXLine.AL_LineAmount = -10M;
			reloadedCharge.JR_AL_CFXLine = newCFXLine.PK;
			reloadedWIP.AL_LineAmount = -40m;
			reloadedWIP.AL_OSAmount = -40m;
			var reloadedShipment = Factory.Load<ForwardingShipment>(shipment.PK);
			reloadedShipment.JS_IsForwardRegistered = false;
			reloadedCharge.Job.Parent = reloadedShipment;
			((IBusinessObjectInternals)reloadedShipment).MarkAsDeleted();
			Assert(reloadedWIP.AL_LineAmountInfo.HasChanges);
			Assert(reloadedCharge.IsCFXPosted);
			Assert(reloadedCharge.JR_AL_CFXLineInfo.HasChanges);
			Assert(!reloadedCharge.IsSavedByFactory);
			var userErrorMessage = @"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: Related WIP amount is not the same as charge amount.";
			var techDetails = @"Developer Details (Critical Validation Failure): 

Related WIP amount is not the same as charge amount.";
			var chargeMessage = ZString.Format(@"Charge: PK = {0}", reloadedCharge.PK);
			var lineMessage = ZString.Format(@"Line: PK = {0}", reloadedWIP.PK);
			testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckNewRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_ChargeIsSavedByFactoryIsFalseAndJR_AL_CFXLineHasChanges_WIP_CFXAmt", true, CriticalValidationErrorType.LineShouldHaveSameAmountAsJobCharge_11, userErrorMessage, techDetails, chargeMessage, lineMessage);
			AssertOnSavingCheck(reloadedWIP, testCase);
		}

		public void TestCheckNewRelatedAccrualLineAmountIsTheSameAsChargeAmount_ChargeIsSavedByFactoryIsTrue_Accrual()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.JobChargeLocalCostAmtNotEqualRelatedAccrualAmount);

			ForwardingShipment shipment;
			Charge charge;
			SetupDataForIsSavedByFactoryTests(out shipment, out charge);
			AssertNotNull(charge.Accrual);
			var oldAccrualPK = charge.Accrual.PK;

			var accrual = charge.Factory.NewWithValidTestData<Accrual>();
			accrual.AL_AG = charge.Accrual.AL_AG;
			charge.ReverseAccrual(ZDateTime.Now);
			accrual.AL_JH = charge.JR_JH;
			accrual.AL_OSAmount = 20m;
			accrual.AL_LocalExTaxAmount = 20m;
			accrual.AL_AC = charge.JR_AC;

			charge.JR_AL_APLine = accrual.PK;
			charge.JR_LocalCostAmt = 21m;

			var userErrorMessage = @"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: Related ACR amount is not the same as charge amount.";
			var techDetails = ZString.Format(@"Developer Details (Critical Validation Failure): 

Related ACR amount is not the same as charge amount.

21 != 20
Charge: PK = {0}", charge.PK);
			var extraMessage = ZString.Format(@"Local Cost Amount is changed from 50.00 to 21 of charge with PK: {0}.
Accrual Line Amount: 20
Charge: PK = {0}", charge.PK);
			var stackTrace = "Stacktrace -->";
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckNewRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_ChargeIsSavedByFactoryIsFalseAndJR_LocalCostAmtHasChanges_Accrual", true, CriticalValidationErrorType.LineShouldHaveSameAmountAsJobCharge_11, userErrorMessage, techDetails, extraMessage, stackTrace);
			AssertOnSavingCheck(accrual, testCase);
		}

		[TestDate(2018, 2, 12)]
		public void TestCheckNewRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_ChargeIsSavedByFactoryIsFalse_WIP_CFXAmt_WithoutCFXLine()
		{
			var creationFactory = new BusinessObjectFactory();
			var objectCreator = new TestObjectCreator(creationFactory);
			var shipment = objectCreator.CreateShipment("S0001001", false);
			var job = objectCreator.CreateJob(shipment, objectCreator.LocalClient, 0m, objectCreator.Agent, 0m);
			var exchangeRate = job.ExchangeRates.AddNew();
			exchangeRate.JF_RX_NKRateCurrency = "USD";
			exchangeRate.JF_BaseRate = 0.75m;
			exchangeRate = job.ExchangeRates.AddNew();
			exchangeRate.JF_RX_NKRateCurrency = "EUR";
			exchangeRate.JF_BaseRate = 2m;
			var charge = job.Charges.AddNew();
			charge.JR_AC = objectCreator.CC1.PK;
			charge.JR_RX_NKSellCurrency = "USD";
			charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(0.5m);
			AssertEquals(0.5m, charge.JR_OSSellExRate);
			var cfxConfig = charge.SellAccount.CompanyData.AccCFXConfigurations.AddNew();
			cfxConfig.JCF_CFXPercentage = 1m;
			cfxConfig.JCF_ServiceDirection = "ALL";
			cfxConfig.JCF_TransportMode = "ALL";
			charge.JR_LocalSellAmt = 50m;
			charge.JR_LineCFX = 1m;
			creationFactory.Save();
			AssertNotNull(charge.WIP);
			var reloadedCharge = Factory.Load<Charge>(charge.PK);
			reloadedCharge.JR_OSSellExRate = 0.5m;
			var modified = reloadedCharge.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(info => info.HasChanges).ToArray();
			Assert(!modified.Any());
			reloadedCharge.HasChanges = false;

			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var reloadedWIP = Factory.Load<WIP>(reloadedCharge.WIP.PK);
			reloadedWIP.AL_LineAmount = -49.5m;
			reloadedWIP.AL_OSAmount = -49.5m;
			Assert(reloadedWIP.AL_LineAmountInfo.HasChanges);
			Assert(reloadedCharge.IsApplyCFX);
			Assert(!reloadedCharge.JR_RX_NKSellCurrencyInfo.HasChanges);
			Assert(!reloadedCharge.IsSavedByFactory);
			// Unable to set up data for passing test below
			//var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckNewRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_ChargeIsSavedByFactoryIsFalseAndJR_RX_NKSellCurrencyHasNoChanges_WIP_CFXAmt");
			//AssertOnSavingCheck(reloadedWIP, testCase);

			reloadedCharge.JR_RX_NKSellCurrency = "EUR";
			reloadedCharge.RevenueExchangeRate.SetBuyRate_ForTestOnly(0.82m);
			AssertEquals("JR_OSSellExRate with CFX", 0.8118m, reloadedCharge.JR_OSSellExRate);
			reloadedCharge.JR_LocalSellAmt = 50m;
			reloadedCharge.JR_LineCFX = 1m;
			reloadedWIP.AL_LineAmount = -20.5m;
			reloadedWIP.AL_OSAmount = -20.5m;
			var reloadedShipment = Factory.Load<ForwardingShipment>(shipment.PK);
			reloadedShipment.JS_IsForwardRegistered = false;
			((IBusinessObjectInternals)reloadedShipment).MarkAsDeleted();
			Assert(reloadedWIP.AL_LineAmountInfo.HasChanges);
			Assert(reloadedCharge.IsApplyCFX);
			Assert(reloadedCharge.JR_RX_NKSellCurrencyInfo.HasChanges);
			Assert(!reloadedCharge.IsSavedByFactory);
			var userErrorMessage = @"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: Related WIP amount is not the same as charge amount.";
			var techDetails = @"Developer Details (Critical Validation Failure): 

Related WIP amount is not the same as charge amount.";
			var chargeMessage = $"PK = {reloadedCharge.PK}";
			var lineMessage = $"PK = {reloadedWIP.PK}";
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckNewRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_ChargeIsSavedByFactoryIsFalseAndJR_RX_NKSellCurrencyHasChanges_WIP_CFXAmt", true, CriticalValidationErrorType.LineShouldHaveSameAmountAsJobCharge_11, userErrorMessage, techDetails, chargeMessage, lineMessage);
			AssertOnSavingCheck(reloadedWIP, testCase);
		}

		[TestDate(2018, 2, 12)]
		public void TestCheckNewRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_WIP_AL_LineAmount()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(Guid.NewGuid(), CriticalValidationInfoCollectorServiceKeyType.LineLocalAmountNotEqualRelatedJobChargeLocalSellAmount);

			var creationFactory = new BusinessObjectFactory();
			var objectCreator = new TestObjectCreator(creationFactory);
			var shipment = objectCreator.CreateShipment("S0001001", false);
			var job = objectCreator.CreateJob(shipment, objectCreator.LocalClient, 0m, objectCreator.Agent, 0m);
			var charge = job.Charges.AddNew();
			charge.JR_AC = objectCreator.CC1.PK;
			charge.JR_RX_NKSellCurrency = "AUD";
			charge.JR_LocalSellAmt = 10m;
			charge.JR_OSSellAmt = 10m;
			creationFactory.Save();

			AssertNotNull(charge.WIP);
			var reloadedCharge = Factory.Load<Charge>(charge.PK);
			reloadedCharge.HasChanges = false;

			var reloadedWIP = Factory.Load<WIP>(reloadedCharge.WIP.PK);
			reloadedWIP.AL_LineAmount = -11m;
			reloadedWIP.AL_OSAmount = -11m;
			Assert(reloadedWIP.AL_LineAmountInfo.HasChanges);

			var userErrorMessage = @"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: Related WIP amount is not the same as charge amount.";
			var techDetails = @"Developer Details (Critical Validation Failure): 

Related WIP amount is not the same as charge amount.";
			var chargeMessage = $"PK = {reloadedCharge.PK}";
			var lineMessage = $"PK = {reloadedWIP.PK}";
			var stackTrace = @"10.0000 - 0 = 10.0000 != 11
Stacktrace -->";
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckNewRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_WIP_AL_LineAmount", true, CriticalValidationErrorType.LineShouldHaveSameAmountAsJobCharge_11, userErrorMessage, techDetails, chargeMessage, lineMessage, stackTrace);
			AssertOnSavingCheck(reloadedWIP, testCase);
		}

		public void TestCheckNewRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_WithCurrencyChangeLogAdded()
		{
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "JPY";
			GlbCompany.CurrentCompany.Factory.Save();
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "AUD";
			GlbCompany.CurrentCompany.Factory.Save();

			var objectCreator = new TestObjectCreator(Factory);
			var shipment = objectCreator.CreateShipment("S000010", true);
			shipment.CreateShipmentJobHeaderWithMutex();
			Factory.Save();

			var charge = objectCreator.CreateCharge(shipment.Job as Job, objectCreator.CC1, osSellAmt: 44.44m, debtor: objectCreator.Debtor1);
			Factory.Save();
			var wip = Factory.Load<WIP>(charge.WIP.PK);
			wip.AL_LineAmount = -11m;
			wip.AL_OSAmount = -11m;
			var userErrorMessage = @"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: Related WIP amount is not the same as charge amount.";
			var developerMsg = @"Company Currency Change Log (Refer to issue with Exception Key JobChargeLocalAmountDecimalsDoNotMatchCurrencySubUnitRatio for more information):
Current Company[EDI] Currency Code Change Log: Company Currency: JPY to AUD.";
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckNewRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_WIP_AL_LineAmount",
				true,
				CriticalValidationErrorType.LineShouldHaveSameAmountAsJobCharge_11,
				userErrorMessage, developerMsg);
			AssertOnSavingCheck(wip, testCase);
		}

		[TestDate(2018, 2, 12)]
		public void TestCheckNewRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_Charge_JR_OSSellAmt()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(Guid.NewGuid(), CriticalValidationInfoCollectorServiceKeyType.JobChargeLocalSellAmtNotEqualRelatedWIPAmount);

			ForwardingShipment shipment;
			Charge charge;
			SetupDataForIsSavedByFactoryTests(out shipment, out charge);
			AssertNotNull(charge.WIP);

			var wip = charge.Factory.NewWithValidTestData<WIP>();
			wip.AL_AG = charge.WIP.AL_AG;
			charge.ReverseWIP(ZDateTime.Now);
			wip.AL_JH = charge.JR_JH;
			wip.AL_OSAmount = 20m;
			wip.AL_LocalExTaxAmount = 20m;
			wip.AL_AC = charge.JR_AC;
			wip.AL_OH = charge.JR_OH_SellAccount;

			foreach (var info in new (BaseCharge Charge, ZDecimal Amount, string SellInvoiceRateWithoutCFX, string SellRateWithoutCFX)[]
				{
					(charge.Factory.Load<Charge>(charge.PK), 21, "0", "0"),
					(charge.Factory.Load<ApportionSplitCharge>(charge.PK), 25, "Not available", "Not available")
				})
			{
				info.Charge.JR_AL_ARLine = wip.PK;
				info.Charge.JR_OSSellAmt = info.Amount;

				var userErrorMessage = @"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: Related WIP amount is not the same as charge amount.";
				var techDetails = @"Developer Details (Critical Validation Failure): 

Related WIP amount is not the same as charge amount.";
				var chargeMessage = $"PK = {charge.PK}";
				var lineMessage = $"PK = {wip.PK}";
				var stackTrace = FormattableString.Invariant($@"21 - 0 = 21 != 20
ExchangeRate info: 
SellInvoiceRateWithoutCFX: {info.SellInvoiceRateWithoutCFX}
SellInvoiceExchangeRate: 1
SellRateWithoutCFX: {info.SellRateWithoutCFX}
RevenueExchangeRate: 1
Stacktrace -->");
				var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckNewRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_Charge_JR_OSSellAmt", true, CriticalValidationErrorType.LineShouldHaveSameAmountAsJobCharge_11, userErrorMessage, techDetails, chargeMessage, lineMessage, stackTrace);
				AssertOnSavingCheck(wip, testCase);
			}
		}

		[TestDate(2018, 2, 12)]
		public void TestCheckNewRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_Charge_JR_LocalSellAmt()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(Guid.NewGuid(), CriticalValidationInfoCollectorServiceKeyType.JobChargeLocalSellAmtNotEqualRelatedWIPAmount);

			ForwardingShipment shipment;
			Charge charge;
			SetupDataForIsSavedByFactoryTests(out shipment, out charge);
			AssertNotNull(charge.WIP);

			var wip = charge.Factory.NewWithValidTestData<WIP>();
			wip.AL_AG = charge.WIP.AL_AG;
			charge.ReverseWIP(ZDateTime.Now);
			wip.AL_JH = charge.JR_JH;
			wip.AL_OSAmount = 20m;
			wip.AL_LocalExTaxAmount = 20m;
			wip.AL_AC = charge.JR_AC;
			wip.AL_OH = charge.JR_OH_SellAccount;

			foreach (var info in new (BaseCharge Charge, ZDecimal Amount, string SellInvoiceRateWithoutCFX, string SellRateWithoutCFX)[]
				{
					(charge.Factory.Load<Charge>(charge.PK), 21, "0", "0"),
					(charge.Factory.Load<ApportionSplitCharge>(charge.PK), 25, "Not available", "Not available")
				})
			{
				info.Charge.JR_AL_ARLine = wip.PK;
				info.Charge.JR_LocalSellAmt = info.Amount;

				var userErrorMessage = @"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: Related WIP amount is not the same as charge amount.";
				var techDetails = @"Developer Details (Critical Validation Failure): 

Related WIP amount is not the same as charge amount.";
				var chargeMessage = $"PK = {charge.PK}";
				var lineMessage = $"PK = {wip.PK}";
				var stackTrace = FormattableString.Invariant($@"{info.Amount} - 0 = {info.Amount} != 20
ExchangeRate info: 
SellInvoiceRateWithoutCFX: {info.SellInvoiceRateWithoutCFX}
SellInvoiceExchangeRate: 1
SellRateWithoutCFX: {info.SellRateWithoutCFX}
RevenueExchangeRate: 1
Stacktrace -->");
				var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckNewRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_Charge_JR_LocalSellAmt", true, CriticalValidationErrorType.LineShouldHaveSameAmountAsJobCharge_11, userErrorMessage, techDetails, chargeMessage, lineMessage, stackTrace);
				AssertOnSavingCheck(wip, testCase);
			}
		}

		[TestDate(2018, 2, 12)]
		public void TestCheckNewRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_Charge_JR_LineCFX()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(Guid.NewGuid(), CriticalValidationInfoCollectorServiceKeyType.JobChargeLocalSellAmtNotEqualRelatedWIPAmount);

			ForwardingShipment shipment;
			Charge charge;
			SetupDataForIsSavedByFactoryTests(out shipment, out charge, true);
			AssertNotNull(charge.WIP);

			var wip = charge.Factory.NewWithValidTestData<WIP>();
			wip.AL_AG = charge.WIP.AL_AG;
			charge.ReverseWIP(ZDateTime.Now);
			wip.AL_JH = charge.JR_JH;
			wip.AL_OSAmount = 47m;
			wip.AL_LocalExTaxAmount = 47m;
			wip.AL_AC = charge.JR_AC;
			wip.AL_OH = charge.JR_OH_SellAccount;

			foreach (var info in new (BaseCharge Charge, ZDecimal Amount, string SellInvoiceRateWithoutCFX, string SellRateWithoutCFX)[]
				{
					(charge.Factory.Load<Charge>(charge.PK), 5, "1", "0"),
					(charge.Factory.Load<ApportionSplitCharge>(charge.PK), 10, "Not available", "Not available")
				})
			{
				info.Charge.JR_AL_ARLine = wip.PK;
				info.Charge.JR_RX_NKSellInvoiceCurrency = "USD";
				info.Charge.JR_LineCFX = info.Amount;
				var chargeLocalSellInvoiceAmt = info.Charge.JR_LocalSellInvoiceAmt;

				var userErrorMessage = @"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: Related WIP amount is not the same as charge amount.";
				var techDetails = @"Developer Details (Critical Validation Failure): 

Related WIP amount is not the same as charge amount.";
				var chargeMessage = $"PK = {charge.PK}";
				var lineMessage = $"PK = {wip.PK}";
				var stackTrace = FormattableString.Invariant($@"{chargeLocalSellInvoiceAmt} - 6 = {(chargeLocalSellInvoiceAmt - 6)} != 47
ExchangeRate info: 
SellInvoiceRateWithoutCFX: {info.SellInvoiceRateWithoutCFX}
SellInvoiceExchangeRate: 1
SellRateWithoutCFX: {info.SellRateWithoutCFX}
RevenueExchangeRate: 1
Stacktrace -->");
				var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckNewRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_Charge_JR_LineCFX", true, CriticalValidationErrorType.LineShouldHaveSameAmountAsJobCharge_11, userErrorMessage, techDetails, chargeMessage, lineMessage, stackTrace);
				AssertOnSavingCheck(wip, testCase);
			}
		}

		[TestDate(2018, 10, 24)]
		public void TestCheckACRWIPCurrencyNotSameWithLocalCurrency()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var shipment = objectCreator.CreateShipment("S0001001", false);
			var job = objectCreator.CreateJob(shipment, objectCreator.LocalClient, 0m, objectCreator.Agent, 0m);
			var wip = Factory.NewWithValidTestData<WIP>();
			wip.AL_AH = ZGuid.Empty;
			wip.AL_LocalExTaxAmount = 10M;
			wip.AL_AG = objectCreator.GLHeader1.PK;
			var charge = Factory.NewWithValidTestData<JobCharge>();
			job.JH_GC = Env.CurrentCompany.PK;
			charge.JR_JH = job.PK;
			wip.AL_JH = job.PK;
			charge.JR_LocalSellAmt = 10M;
			charge.JR_AL_ARLine = wip.PK;
			using (Globals.TemporaryOverrideForIsTest(false))
			{
				wip.AL_RX_NKTransactionCurrency = "ZZZ";
			}

			var userMessage = @"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: WIP currency is not the same as local currency.";
			var expectMessage1 = $@"Developer Details (Critical Validation Failure): 

WIP currency is not the same as local currency.

Line: PK = {wip.PK}, Charge Code = , GL Account = {wip.GLHeader.AccountNum}, Type = WIP, OS Amount = -10, Local Amount = -10, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 1, Currency = ZZZ, Post Date = 24-Oct-18 00:00:00, Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = 00000000-0000-0000-0000-000000000000, Job PK = {job.PK}, Organization = , Revenue Recognition Type = IMM, Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.";
			var expectMessage2 = "ACRWIPCurrencyNotSameWithLocalCurrency: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.";
			var expectMessage3 = $@"Current user context: company PK: {Env.CurrentCompany.PK}, code: EDI, currency: AUD.
Parent Job Company info: company PK: {Env.CurrentCompany.PK}, code: EDI, currency: AUD.";

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckWIPAccrualCurrencyIsLocalOne", true, CriticalValidationErrorType.LineCurrencyShouldHaveSameCurrencyAsLocalCurrency_3, userMessage, expectMessage1, expectMessage2, expectMessage3);
			AssertOnSavingCheck(wip, testCase);

			wip.AL_RX_NKTransactionCurrency = "ZZZ";

			expectMessage2 = $@"ACRWIPCurrencyNotSameWithLocalCurrency:
Local currency from job: company PK: {Env.CurrentCompany.PK} currency: AUD.
Current user context in AL_RX_NKTransactionCurrency setter: company PK: {Env.CurrentCompany.PK}, currency: AUD.
Stacktrace:
   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)
";

			testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckWIPAccrualCurrencyIsLocalOne", true, CriticalValidationErrorType.LineCurrencyShouldHaveSameCurrencyAsLocalCurrency_3, userMessage, expectMessage1, expectMessage2, expectMessage3);
			AssertOnSavingCheck(wip, testCase);
		}

		[TestDate(2018, 10, 24)]
		public void TestCheckNewRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_CFXAndExchangeRateAndDecimalMessages()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(Guid.NewGuid(), CriticalValidationInfoCollectorServiceKeyType.WIPLocalSellInvoiceAmtInfo);

			var testObjectCreator = new TestObjectCreator(Factory);
			var debtorOrg = testObjectCreator.CreateOrgHeader("ABCPROXY", true, true);
			var debtorCompany = testObjectCreator.CreateNewCompany("ABC");
			debtorCompany.GC_RX_NKLocalCurrency = "TWD";
			debtorCompany.GC_Name = "GC_Name";
			debtorCompany.GC_OH_OrgProxy = debtorOrg.PK;
			var debtorBranch = testObjectCreator.CreateNewBranch(debtorCompany, "BR1");
			AssertEquals("Precondition: current company local decimals", 2, GlbCompany.CurrentCompany.GetLocalDecimals());
			AssertEquals("Precondition: other company local decimals", 0, debtorCompany.GetLocalDecimals());
			Factory.Save();

			ForwardingShipment shipment;
			Charge charge;
			SetupDataForIsSavedByFactoryTests(out shipment, out charge);
			charge.JR_OH_SellAccount = debtorOrg.PK;
			charge.JR_RX_NKSellCurrency = "AUD";
			charge.JR_RX_NKSellInvoiceCurrency = "USD";

			var exchangeRate = charge.InvoicingJob.ExchangeRates.AddNew();
			exchangeRate.JF_BaseRate = 1.23555m;
			exchangeRate.JF_RX_NKRateCurrency = "USD";
			exchangeRate.JF_CFXPercent = 2M;
			exchangeRate.JF_OH_Org = debtorOrg.PK;
			exchangeRate.JF_OrgType = ExchangeRateOrgTypeEnum.Debtor.ToCode();

			AssertNotNull(charge.WIP);
			var previousWipPK = charge.WIP.PK;

			charge.ReverseWIP(ZDateTime.Now);
			var wip = charge.Factory.NewWithValidTestData<WIP>();
			wip.SetValues(charge.InvoicingJob, charge);
			charge.JR_AL_ARLine = wip.PK;

			wip.AL_OSAmount = 52m;
			wip.AL_LocalExTaxAmount = 52m;

			var userErrorMessage = @"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: Related WIP amount is not the same as charge amount.";
			var techDetails = @"Developer Details (Critical Validation Failure): 

Related WIP amount is not the same as charge amount.";
			var details1 = @"51 - 0 = 51 != 52
JR_LocalSellInvoiceAmt = 51
Charge.CompanyLocalCurrencyDecimals = 2
Charge.LocalCurrencyDecimals = 0
----";
			var details2 = $@"WIPLocalSellInvoiceAmtInfo:

Values When Creating WIP:
  JR_LocalSellInvoiceAmt: 51
  Company.GC_RX_NKLocalCurrency: AUD
  Company.LocalCurrency.Decimals: 2
  BillInInvoiceCurrencyWithLocalSellCurrency: True
  BillInInvoiceCurrency: True
  IsSellLocal: Y
  SellInvoiceExchangeRate:
    ExchangeRatePk: {exchangeRate.PK}
    CurrencyCode: USD
    Rate: 1
    SellRate: 0.98
    OrgPk: {debtorOrg.PK}
    OrgType: Debtor
    CFXPercent: 2
    CFXMinimum: 0
    IsUserDefinedOrTransformed: Yes
    IsDeleted: No

SellInvoiceExchangeRate In Critical Validation:
  ExchangeRatePk: {exchangeRate.PK}
  CurrencyCode: USD
  Rate: 1
  SellRate: 0.98
  OrgPk: {debtorOrg.PK}
  OrgType: Debtor
  CFXPercent: 2
  CFXMinimum: 0
  IsUserDefinedOrTransformed: Yes
  IsDeleted: No";

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
					description: "TestCheckNewRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_CFXAndExchangeRateAndDecimalMessages",
					criticalCheckShouldFail: true,
					errorType: CriticalValidationErrorType.LineShouldHaveSameAmountAsJobCharge_11,
					userErrorMessage: userErrorMessage,
					techDetails,
					details1,
					details2);
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, debtorBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertOnSavingCheck(charge.WIP, testCase);
			}
		}

		[TestDate(2018, 10, 24)]
		public void TestCheckRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_WithJobheaderLogAdded()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(Guid.NewGuid(), CriticalValidationInfoCollectorServiceKeyType.JobChargeLocalSellAmtNotEqualRelatedWIPAmount);

			SetupDataForIsSavedByFactoryTests(out var shipment, out var charge);
			var wip = charge.Factory.NewWithValidTestData<WIP>();
			wip.AL_AG = charge.WIP.AL_AG;
			charge.ReverseWIP(ZDateTime.Now);
			wip.AL_JH = charge.JR_JH;
			wip.AL_AC = charge.JR_AC;
			wip.AL_OH = charge.JR_OH_SellAccount;

			charge.JR_AL_ARLine = wip.PK;
			wip.AL_OSAmount = 51m;
			wip.AL_LocalExTaxAmount = 51m;

			var userErrorMessage = @"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: Related WIP amount is not the same as charge amount.";

			var jobHeader = charge.Job;
			jobHeader.JH_Description = "test";

			var jobHeaderSeperator = "Parent.Job Properties:";
			var jobHeaderDescriptionLog = "JH_Description = test";

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_WithJobheaderLogAdded", true, CriticalValidationErrorType.LineShouldHaveSameAmountAsJobCharge_11, userErrorMessage, jobHeaderSeperator, jobHeaderDescriptionLog);
			AssertOnSavingCheck(wip, testCase);
		}

		[TestDate(2018, 10, 24)]
		public void TestCheckRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_WithJobChargeJR_LineCFXLogAdded()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(Guid.NewGuid(), CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_LineCFXHasChangesAfterSaving);

			ForwardingShipment shipment;
			Charge charge;
			SetupDataForIsSavedByFactoryTests(out shipment, out charge);
			var wip = charge.Factory.NewWithValidTestData<WIP>();
			wip.AL_AG = charge.WIP.AL_AG;
			charge.ReverseWIP(ZDateTime.Now);
			wip.AL_JH = charge.JR_JH;
			wip.AL_AC = charge.JR_AC;
			wip.AL_OH = charge.JR_OH_SellAccount;
			charge.JR_AL_ARLine = wip.PK;
			wip.AL_OSAmount = 51m;
			wip.AL_LocalExTaxAmount = 51m;
			charge.JR_LineCFX = 123m;

			var userErrorMessage = @"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: Related WIP amount is not the same as charge amount.";
			var techDetails = @"Developer Details (Critical Validation Failure): 

Related WIP amount is not the same as charge amount.";
			var details = @"JobChargeJR_LineCFXHasChangesAfterSaving:
JR_LineCFX Old Value = 0, New Value = 123.";

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
					description: "TestCheckRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount_WithJobChargeJR_LineCFXLogAdded",
					criticalCheckShouldFail: true,
					errorType: CriticalValidationErrorType.LineShouldHaveSameAmountAsJobCharge_11,
					userErrorMessage: userErrorMessage,
					techDetails,
					details);

			AssertOnSavingCheck(charge.WIP, testCase);
		}

		void SetupDataForIsSavedByFactoryTests(out ForwardingShipment shipment, out Charge charge, bool createCFXLine = false)
		{
			var creationFactory = new BusinessObjectFactory();
			var objectCreator = new TestObjectCreator(creationFactory);
			shipment = objectCreator.CreateShipment("S0001001", false);
			var job = objectCreator.CreateJob(shipment, objectCreator.LocalClient, 0m, objectCreator.Agent, 0m);
			charge = job.Charges.AddNew();
			charge.JR_AC = objectCreator.CC1.PK;
			charge.JR_OSSellAmt = 50m;
			creationFactory.Save();
			if (createCFXLine)
			{
				var cfxLine = creationFactory.NewWithPrimaryKey<ARInvoiceLine>(new Guid("c70fa1be-a67e-4e1d-ad0e-c59b31390a8d"));
				cfxLine.FillWithValidTestData();
				cfxLine.AL_AH = creationFactory.New<ARInvoice>().PK;
				cfxLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
				cfxLine.AL_LineAmount = -6M;
				cfxLine.AL_OSAmount = -6M;
				cfxLine.AL_AG = objectCreator.GLHeader1.PK;
				charge.JR_AL_CFXLine = cfxLine.PK;
				creationFactory.Save();
			}
		}

		void ClearChargeRefColumn(ZGuid chargePK, string linkColumn)
		{
			if (!string.IsNullOrEmpty(linkColumn) && chargePK.IsValid)
			{
				DbCommand command = ((IDbConnected)Factory).Connection.Command(string.Format("update dbo.JobCharge set {0} = null, JR_SystemLastEditTimeUtc = GETUTCDATE(), JR_SystemLastEditUser = '~BP' where JR_PK = @ChargePK", linkColumn));
				command.AddParameterBasedOnDbColumn("@ChargePK", chargePK.ToGuid(), JobChargeSchema.PK);
				command.ExecuteNonQuery();
			}
		}

		BusinessObjectFactory FactoryForSaving;
		BaseWIPAccrual TestWIP;
		BaseWIPAccrual TestAccrual;
		JobCharge TestCharge;
		void SetUpData()
		{
			FactoryForSaving = new BusinessObjectFactory();
			TestWIP = FactoryForSaving.NewWithValidTestData<WIP>();
			TestAccrual = FactoryForSaving.NewWithValidTestData<Accrual>();
			TestCharge = FactoryForSaving.NewWithValidTestData<JobCharge>();
			TestCharge.JR_AL_ARLine = TestWIP.PK;
			TestCharge.JR_AL_APLine = TestAccrual.PK;
			TestWIP.AL_JH = TestCharge.JR_JH;
			TestAccrual.AL_JH = TestCharge.JR_JH;
			TestWIP.AL_AG = FactoryForSaving.NewWithValidTestData<AccGLHeader>().PK;
			TestAccrual.AL_AG = FactoryForSaving.NewWithValidTestData<AccGLHeader>().PK;
			FactoryForSaving.Save();
		}
	}
}
