using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.GatewayBilling.Testing;
using Enterprise.Accounting.Business.Testing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Accounting.Business.ConsolCosting.JobConsolCost;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.Accounting.Business.ConsolCosting
{
	public class JobConsolCostStrategyTest : TestCaseWithFactory
	{
		public void TestStrategyWhenUpdatingConsolCostExchangeRateDuringPosting()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.Shipments.AddNew();
			var apps = new ApportionmentListing(Factory, consol);
			try
			{
				var cost = apps.CostsCollection.TryAddNew();

				Assert(!cost.IsPosting);
				Assert(!cost.HasContext(ConsolCostStrategy.ConsolCostCalculationStrategyWithCalculationsDuringPosting));
				Assert(cost.CalculationStrategy is ConsolCostCalculationStrategyWithCalculations);

				cost.IsPosting = true;
				Assert(cost.IsPosting);
				Assert(!cost.HasContext(ConsolCostStrategy.ConsolCostCalculationStrategyWithCalculationsDuringPosting));
				Assert(cost.CalculationStrategy is ConsolCostCalculationStrategyWithoutCalculations);

				cost.SetContext(ConsolCostStrategy.ConsolCostCalculationStrategyWithCalculationsDuringPosting);
				Assert(cost.IsPosting);
				Assert(cost.HasContext(ConsolCostStrategy.ConsolCostCalculationStrategyWithCalculationsDuringPosting));
				Assert(cost.CalculationStrategy is ConsolCostCalculationStrategyWithCalculations);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestStrategyWhenPosting()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.Shipments.AddNew();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();
				apps.IsPosting = true;
				Assert(cost.CalculationStrategy is ConsolCostCalculationStrategyWithoutCalculations);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestStrategyWhenPostingWithFilteredCollection()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.Shipments.AddNew();
			var apps = new ApportionmentListing(Factory, consol);
			try
			{
				var cost = apps.CostsCollection.TryAddNew();
				((IBusinessObjectInternals)cost).ParentCollections[0] = apps.CostsFilteredCollection;
				apps.IsPosting = true;
				Assert(cost.CalculationStrategy is ConsolCostCalculationStrategyWithoutCalculations);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestSplitApproportionAmount()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			Factory.Save();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();
				cost.E6_ApportionmentMethod = AllocationMethod.Shipment;
				AssertEquals("Prequisite", consol.Shipments.Count, apps.CostsCollection[0].ApportionmentCharges.Count);
				apps.CostsCollection[0].ApportionmentCharges[0].JR_IsIncludedInProfitShare = true;
				apps.CostsCollection[0].ApportionmentCharges[1].JR_IsIncludedInProfitShare = true;
				cost.E6_OSCostAmount = 100;
				AssertEquals(50m, apps.CostsCollection[0].ApportionmentCharges[0].JR_OSCostAmt);
				AssertEquals(50m, apps.CostsCollection[0].ApportionmentCharges[0].JR_AgentDeclaredCostAmt);
				AssertEquals(50m, apps.CostsCollection[0].ApportionmentCharges[1].JR_OSCostAmt);
				AssertEquals(50m, apps.CostsCollection[0].ApportionmentCharges[1].JR_AgentDeclaredCostAmt);
				cost.E6_OSCostAmount = 300;
				AssertEquals(150m, apps.CostsCollection[0].ApportionmentCharges[0].JR_OSCostAmt);
				AssertEquals(150m, apps.CostsCollection[0].ApportionmentCharges[0].JR_AgentDeclaredCostAmt);
				AssertEquals(150m, apps.CostsCollection[0].ApportionmentCharges[1].JR_OSCostAmt);
				AssertEquals(150m, apps.CostsCollection[0].ApportionmentCharges[1].JR_AgentDeclaredCostAmt);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestTryToDefaultSupplyType()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var jobConsolCost = Factory.NewWithValidTestData<JobConsolCost>();
			Assert("Precondition: Supply Type is Empty", jobConsolCost.E6_SupplyType.IsEmpty);

			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.AlwaysSetSupplyTypetoDSBIfChargeTypeIsDSB.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var supplyTypeOverride = objectCreator.FRT.SupplyTypeOverrides.AddNew();
				supplyTypeOverride.ACS_JobType = "FCN";
				supplyTypeOverride.ACS_TransportMode = Constants.FreightShipmentDirection.Code.All;
				supplyTypeOverride.ACS_Direction = Constants.TransportModes.All;
				supplyTypeOverride.ACS_IncoTerm = AccountingMasterFilesConstants.INCOTermCodes.All;
				supplyTypeOverride.ACS_GE = GlbDepartment.CurrentDepartment.PK.ToGuid();
				supplyTypeOverride.LineDepartmentPK = ZGuid.Empty;
				supplyTypeOverride.ACS_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.INT;
				Factory.Save();

				jobConsolCost.E6_AC_ChargeCode = objectCreator.FRT.PK;
				AssertEquals(AccountingMasterFilesConstants.SupplyTypeClassificationCodes.INT, jobConsolCost.E6_SupplyType);

				jobConsolCost.E6_AC_ChargeCode = objectCreator.GSTFREE1.PK;
				Assert("Precondition: No override rule for GSTFREE1 Charge Code",jobConsolCost.E6_SupplyType.IsEmpty);

				jobConsolCost.E6_AC_ChargeCode = objectCreator.FRT.PK;
				AssertEquals(AccountingMasterFilesConstants.SupplyTypeClassificationCodes.INT, jobConsolCost.E6_SupplyType);

				jobConsolCost.E6_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOA;
				jobConsolCost.E6_AC_ChargeCode = objectCreator.FRT.PK;
				AssertEquals("Precondition: Charge Code is set with the same value", AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOA, jobConsolCost.E6_SupplyType);
			}
		}
	}

	[TestedType(typeof(JobConsolCost))]
	public abstract class JobConsolCostTest : EnterpriseBusinessObjectTestCase
	{
		#region ReadOnly when posted/unposted

		public void TestReadOnly_WhenNotPosted_EditablePropertiesShouldNotBeReadOnly()
		{
			var jobConsolCost = Factory.NewWithValidTestData<JobConsolCost>();
			jobConsolCost.E6_AH_APInvoice = ZGuid.Empty;
			Factory.Save();

			Assert("Precondition: Cost is not posted.", !jobConsolCost.IsPosted);

			var editablePropertyInfos = jobConsolCost.ZPropertyInfoHash
				.OfType<ZPropertyInfo>()
				.Where(p =>
				{
					var readOnlyAttribute = Enterprise.Customs.Business.ZPropertyInfoExtensions.GetAttribute<ReadOnlyAttribute>(p);
					if (readOnlyAttribute != null && readOnlyAttribute.IsReadOnly)
					{
						return false;
					}

					return p.HasSetter;
				})
				.ToList();

			Assert(editablePropertyInfos.Any());
			foreach (var propertyInfo in editablePropertyInfos.Where(propertyInfo => !ExcludedPropertiesForTestingReadOnly.Contains(propertyInfo.Name)))
			{
				Assert($"{propertyInfo.Name} should be editable when cost is not posted", !propertyInfo.ReadOnly);
			}
		}

		/// <summary>
		/// This test does not take Gateway Sell Apportionment into account.
		/// E6_RatingBehaviour is editable in this case
		/// but it should not be when cost is apportioned from Gateway revenue which makes cost automatically posted.
		/// <see cref="TestGatewayConsolCostFieldsAlwaysReadOnly"/>
		/// </summary>
		public void TestReadOnly_WhenPosted_AllPropertiesExceptE6_RatingBehaviour_ShouldBeReadOnly()
		{
			var jobConsolCost = Factory.NewWithValidTestData<JobConsolCost>();
			var consolAPInvoice = Factory.New<APInvoice>();
			consolAPInvoice.AH_TransactionNum = "1111";
			jobConsolCost.E6_AH_APInvoice = consolAPInvoice.PK;
			Factory.Save();

			Assert("Precondition: Cost is posted.", jobConsolCost.IsPosted);

			var allPropertyInfos = jobConsolCost.ZPropertyInfoHash.OfType<ZPropertyInfo>();
			foreach (var propertyInfo in allPropertyInfos)
			{
				if (propertyInfo.Name == "E6_RatingBehaviour")
				{
					Assert("E6_RatingBehaviour should not be read-only when cost is posted.", !propertyInfo.ReadOnly);
				}
				else
				{
					Assert($"{propertyInfo.Name} should be read-only when cost is posted.", propertyInfo.ReadOnly);
				}
			}
		}

		public void TestReadOnly_WhenPostedThenUnposted_EditablePropertiesShouldNotBeReadOnly()
		{
			var jobConsolCost = Factory.NewWithValidTestData<JobConsolCost>();
			var consolAPInvoice = Factory.New<APInvoice>();
			consolAPInvoice.AH_TransactionNum = "1111";
			jobConsolCost.E6_AH_APInvoice = consolAPInvoice.PK;
			Factory.Save();

			Assert("Precondition: Cost is posted.", jobConsolCost.IsPosted);

			jobConsolCost.E6_AH_APInvoice = ZGuid.Empty;
			Assert("Precondition: Cost is now unposted.", !jobConsolCost.IsPosted);

			var editablePropertyInfos = jobConsolCost.ZPropertyInfoHash
				.OfType<ZPropertyInfo>()
				.Where(p =>
				{
					var readOnlyAttribute = Enterprise.Customs.Business.ZPropertyInfoExtensions.GetAttribute<ReadOnlyAttribute>(p);
					if (readOnlyAttribute != null && readOnlyAttribute.IsReadOnly)
					{
						return false;
					}

					return p.HasSetter;
				})
				.ToList();

			Assert(editablePropertyInfos.Any());
			foreach (var propertyInfo in editablePropertyInfos.Where(propertyInfo => !ExcludedPropertiesForTestingReadOnly.Contains(propertyInfo.Name)))
			{
				Assert($"{propertyInfo.Name} should be editable when cost is not posted", !propertyInfo.ReadOnly);
			}
		}

		// Exclude properties from the tests because they require other conditions to make them editable. To be tested individually.
		static readonly HashSet<string> ExcludedPropertiesForTestingReadOnly = new HashSet<string>
		{
			"E6_OSGSTAmount", "E6_OSGSTAmount_Calc",	// TestE6_OSGSTAmountFieldsReadOnly_WhenUnposted_GSTApplicable
			"E6_AT_TaxRate", "E6_TaxDate",				// TestSetE6_AT_TaxRateAndE6_TaxDateReadonly
			"E6_AW",									// TestE6_AWReadOnly_WhenUnposted_CostWHTApplicable
			"E6_GB_CostTaxBranch",						// TestE6_GB_CostTaxBranch_ReadOnly
			"E6_A9_VATClass",							// TestSetE6_A9_VatClassReadonly not related to Posted status
			"E6_AK_ChequeBook",							// TestCostPaymentType not related to Posted status
			"GSTInclusiveAmount",						// TestGSTInclusiveAmountInfo not related to Posted status
		};

		public void TestE6_OSGSTAmountFieldsReadOnly_WhenUnposted_GSTApplicable()
		{
			var jobConsolCost = Factory.NewWithValidTestData<JobConsolCost>();
			var consolAPInvoice = Factory.New<APInvoice>();
			consolAPInvoice.AH_TransactionNum = "1111";
			jobConsolCost.E6_AH_APInvoice = consolAPInvoice.PK;
			Factory.Save();

			jobConsolCost.E6_AH_APInvoice = ZGuid.Empty;
			jobConsolCost.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			ObjectCreator.AALSHI.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			var defaultIsGSTRegistered = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			try
			{
				using (AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
				{
					GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
					jobConsolCost.E6_AT_TaxRate = ObjectCreator.GST1.PK;
					jobConsolCost.E6_IsTaxAmountOverridden = true;

					Assert("E6_OSGSTAmount should be editable", !jobConsolCost.E6_OSGSTAmountInfo.ReadOnly);
					Assert("E6_OSGSTAmount_Calc should be editable", !jobConsolCost.E6_OSGSTAmount_CalcInfo.ReadOnly);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = defaultIsGSTRegistered;
			}
		}

		public void TestE6_AWReadOnly_WhenUnposted_CostWHTApplicable()
		{
			var jobConsolCost = Factory.NewWithValidTestData<JobConsolCost>();
			var consolAPInvoice = Factory.New<APInvoice>();
			consolAPInvoice.AH_TransactionNum = "1111";
			jobConsolCost.E6_AH_APInvoice = consolAPInvoice.PK;
			Factory.Save();

			jobConsolCost.E6_AH_APInvoice = ZGuid.Empty;
			jobConsolCost.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			ObjectCreator.AALSHI.MiscServ.OM_APWHTApplicable = true;
			var defaultIsWHTRegistered = GlbCompany.CurrentCompany.GC_IsWHTRegistered;
			try
			{
				using (AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyWHTId.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
				{
					GlbCompany.CurrentCompany.GC_IsWHTRegistered = true;
					jobConsolCost.E6_AT_TaxRate = ObjectCreator.GST1.PK;

					Assert(!jobConsolCost.E6_AWInfo.ReadOnly);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsWHTRegistered = defaultIsWHTRegistered;
			}
		}

		public void TestReadOnlyRegisteredChildrenWhenNotPosted() => AssertReadOnlyRegisteredChildren();

		public void TestReadOnlyRegisteredChildrenWhenPosted_NotApproving_NotDeleted() => AssertReadOnlyRegisteredChildren(isPosted: true);

		public void TestReadOnlyRegisteredChildrenWhenPosted_Approving_NotDeleted() => AssertReadOnlyRegisteredChildren(isPosted: true, isApprovingPosting: true);

		public void TestReadOnlyRegisteredChildrenWhenPosted_Deleted() => AssertReadOnlyRegisteredChildren(isPosted: true, isDeleted: true);

		void AssertReadOnlyRegisteredChildren(bool isPosted = false, bool isApprovingPosting = false, bool isDeleted = false)
		{
			var jobConsolCost = Factory.NewWithValidTestData<JobConsolCost>();
			if (isPosted)
			{
				if (!isApprovingPosting)
				{
					var consolAPInvoice = Factory.New<APInvoice>();
					consolAPInvoice.AH_TransactionNum = "1111";
					jobConsolCost.E6_AH_APInvoice = consolAPInvoice.PK;

					Assert("precondition: when posted", jobConsolCost.IsPosted);
					Assert("precondition: when not approving posting", !jobConsolCost.IsApprovingPosting);
				}
				else
				{
					UAInvoice invoice = Factory.NewWithValidTestData<UAInvoice>();
					invoice.AH_Ledger = "AP";
					invoice.AH_TransactionType = "INV";
					Factory.Save();
					jobConsolCost.ParentAPInvoice = invoice;
					jobConsolCost.E6_AH_APInvoice = invoice.PK;

					Assert("precondition: when posted", jobConsolCost.IsPosted);
					Assert("precondition: when approving posting", jobConsolCost.IsApprovingPosting);
				}
			}
			else
			{
				jobConsolCost.E6_AH_APInvoice = ZGuid.Empty;

				Assert("precondition: when posted", !jobConsolCost.IsPosted);
				Assert("precondition: when not approving posting", !jobConsolCost.IsApprovingPosting);
			}

			var paymentBasis1 = jobConsolCost.PaymentBases.AddNew();
			paymentBasis1.PBS_PerUnitRate = 2.0m;
			paymentBasis1.PBS_ChargeableUnit = "20GP";
			paymentBasis1.PBS_ChargeableAmount = 1;
			paymentBasis1.PBS_ChargeableDescription = "CONT001";
			paymentBasis1.PBS_RateUnit = QuantityUnit.CN;

			var paymentBasis2 = jobConsolCost.PaymentBases.AddNew();
			paymentBasis2.PBS_FlatRate = 10;
			paymentBasis2.PBS_ChargeableAmount = 1;
			paymentBasis2.PBS_ChargeableDescription = "Flat rate";

			Factory.Save();

			if (isDeleted)
			{
				jobConsolCost.Delete();
			}

			var expectedReadOnlyStatus = !isDeleted && isPosted && !isApprovingPosting;

			AssertEquals(expectedReadOnlyStatus, jobConsolCost.ApportionmentCharges.ReadOnly);
			AssertEquals(expectedReadOnlyStatus, jobConsolCost.FilteredApportionmentCharges.ReadOnly);
			AssertEquals(expectedReadOnlyStatus, jobConsolCost.CostExchangeRate.CurrencyInfo.ReadOnly);

			foreach (var jobPaymentBasis in jobConsolCost.PaymentBases)
			{
				AssertEquals(expectedReadOnlyStatus, jobPaymentBasis.ReadOnly);
			}
		}

		#endregion

		#region IAutoRatingChargeInfo

		public void TestRateAttributes()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol();
			var cost = creator.CreateConsolCost(consol, creator.CC1);

			Assert("No attributes attached to the cost yet", !((IAutoRatingChargeInfo)cost).RateAttributes.Attributes.Any());

			cost.Attributes.Add(JobChargeAttribTypeList.Codes.Commodity, "CAR");
			cost.Attributes.Add(JobChargeAttribTypeList.Codes.ContainerCode, "20GP");

			var expectedAttributes = new[]
			{
				new RateAttribute(JobChargeAttribTypeList.Codes.Commodity, "CAR"),
				new RateAttribute(JobChargeAttribTypeList.Codes.ContainerCode, "20GP")
			};

			AssertContainsExactElementsInAnyOrder("Should return attributes attached to this cost", expectedAttributes, ((IAutoRatingChargeInfo)cost).RateAttributes.Attributes);
		
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newCost = newFactory.Load<IJobConsolCost>(cost.PK);

			AssertContainsExactElementsInAnyOrder("Should return saved attributes for this cost", expectedAttributes, ((IAutoRatingChargeInfo)newCost).RateAttributes.Attributes);
		}

		#endregion

		public void TestCreateMonitorOnFactorySavingBeforeTransactionCoreInCompanyContext()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S0001", consol);
			var job = TestObjectCreator.CreateJob(shipment);
			base.Factory.Save();

			var consolCost = Factory.NewWithValidTestData<JobConsolCostForOnFactorySavingBeforeTransactionTest>();
			var charge = consolCost.ApportionmentCharges.AddNew();
			charge.JR_JH = job.PK;
			consolCost.E6_AC_ChargeCode = testObjectCreator.DSBChargeCode.PK;
			consolCost.E6_ExchangeRate = 1;
			consolCost.E6_OSCostAmount = 100m;
			consolCost.E6_LocalCostAmount = 100m;
			charge.JR_LocalCostAmt = 100m;
			charge.JR_OSCostAmt = 100m;
			consolCost.AssertMethodToRun += AssertCreateMonitor;

			using (DeleteApportionmentChargesWhenSaveJobConsolCostMonitor.AddTempService(Factory))
			{
				Factory.Save();
			}

			void AssertCreateMonitor()
			{
				var monitor = Factory.ServiceContainer.GetService<DeleteApportionmentChargesWhenSaveJobConsolCostMonitor>();
				AssertNotNull(monitor);

				var result = monitor.TryGetRelativeJobConsolCostPK(consolCost.ApportionmentCharges[0], out ZGuid consolCostPK);
				AssertEquals("Should record charge info because consol cost has changes.", true, result);
				AssertEquals(consolCost.PK, consolCostPK);
			}
		}

		public void TestApportionChargeCanBeCollectedDuringSaving()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S0001", consol);
			var job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			var consolCost = Factory.NewWithValidTestData<JobConsolCostForOnFactorySavingBeforeTransactionTest>();
			var charge = consolCost.ApportionmentCharges.AddNew();
			charge.JR_JH = job.PK;
			consolCost.E6_AC_ChargeCode = testObjectCreator.DSBChargeCode.PK;
			consolCost.E6_ExchangeRate = 1;
			consolCost.E6_OSCostAmount = 100m;
			consolCost.E6_LocalCostAmount = 100m;
			charge.JR_LocalCostAmt = 100m;
			charge.JR_OSCostAmt = 100m;

			using (DeleteApportionmentChargesWhenSaveJobConsolCostMonitor.AddTempService(Factory))
			{
				var monitor = Factory.ServiceContainer.GetService<DeleteApportionmentChargesWhenSaveJobConsolCostMonitor>();
				AssertNotNull(monitor);
				monitor.CollectApportionmentChargesInfo(consolCost);

				consolCost.PrepareForPosting();

				var result = monitor.TryGetRelativeJobConsolCostPK(consolCost.ApportionmentCharges[0], out ZGuid consolCostPK1);
				AssertEquals("Should record new apportion charge info.", true, result);
				AssertEquals(consolCost.PK, consolCostPK1);

				AssertNotEquals("original charge is deleted", charge.PK, consolCost.ApportionmentCharges[0].PK);
				result = monitor.TryGetRelativeJobConsolCostPK(charge, out ZGuid consolCostPK2);
				AssertEquals("Should record original charge info.", true, result);
				AssertEquals(consolCost.PK, consolCostPK2);
			}
		}

		public void TestJobConsolCostTaxIdAndTaxMessageMapping()
		{
			var taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate1.AT_Type = AccTaxRate.Types.Rated;
			taxRate1.AT_Code = "TaxRate01";
			var taxRate2 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate2.AT_Type = AccTaxRate.Types.Rated;
			taxRate2.AT_Code = "TaxRate02";
			var taxMsg1 = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg1.A9_Code = "TaxMsg01";
			var taxMsg2 = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg2.A9_Code = "TaxMsg02";
			taxRate1.AT_A9_DefaultVatClass = taxMsg1.PK;
			taxRate2.AT_A9_DefaultVatClass = taxMsg1.PK;
			Factory.Save();

			var config = TestObjectCreator.CreateTaxIdAndTaxMessageCombinationRulesConfiguration(
				(TransactionLineTypes.Cost, taxRate1, taxMsg1),
				(TransactionLineTypes.Cost, taxRate2, taxMsg2));
			AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			var consol = ObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var consolCost = GetCost(consol);

			consolCost.E6_AT_TaxRate = taxRate1.PK;
			consolCost.E6_A9_VATClass = taxMsg1.PK;

			AssertNoErrors(consolCost.E6_A9_VATClassInfo);

			consolCost.E6_AT_TaxRate = taxRate2.PK;
			AssertEquals("Pre-condition", taxMsg1.PK, consolCost.E6_A9_VATClass);

			var expectedMsg = @"Transaction cannot be saved/posted due to invalid Tax ID and Tax Message combination.
Line Type=CST, Tax ID=TaxRate01, Tax Message=TaxMsg02";
			AssertNoErrors(consolCost.E6_AT_TaxRateInfo);
			AssertHasErrors(expectedMsg, consolCost.E6_A9_VATClassInfo);

			consolCost.E6_AT_TaxRate = taxRate1.PK;
			AssertNoErrors(consolCost.E6_A9_VATClassInfo);
		}

		public void TestRatingBehaviour_ManuallyCreatedCharge_DefaultsToNew()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = Factory.New<ForwardingConsol>();
			consol.Shipments.AddNew();
			Factory.Save();
			var apps = new ApportionmentListing(Factory, consol);
			try
			{
				var cost = apps.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = Guid.Empty;
				AssertEquals("", cost.E6_RatingBehaviour);

				cost.E6_AC_ChargeCode = testObjectCreator.CC1.PK;
				AssertEquals("NEW", cost.E6_RatingBehaviour);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestCreditorDefaulting_ManuallyCreatedCharge_WithConsolIsDomesticAndNotCoLoad()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CC1.AC_IsGroupageCharge = true;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;

			var forwardingConsol = Factory.New<ForwardingConsol>();
			forwardingConsol.JK_RL_NKLoadPort = "AUSYD";
			forwardingConsol.JK_RL_NKDischargePort = "AUADL";
			forwardingConsol.JK_OA_ShippingLineAddress = carrier.Addresses[0].PK;
			forwardingConsol.CreditorPK = creditor.PK;

			Factory.Save();
			var apps = new ApportionmentListing(Factory, forwardingConsol);

			try
			{
				var cost = apps.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = testObjectCreator.CC1.PK;
				AssertEquals("When the not Co-Load Domestic Consol has a creditor, then the charge creditor becomes the Carrier Export Creditor"
					, forwardingConsol.CarrierExportCreditorAddress.OrganisationPK
					, cost.E6_OH_Creditor);

				forwardingConsol.CreditorPK = ZGuid.Empty;
				var cost2 = apps.CostsCollection.TryAddNew();
				cost2.E6_AC_ChargeCode = testObjectCreator.CC1.PK;
				AssertEquals("When the not Co-Load Domestic Consol has no creditor, then the charge creditor becomes the Consol Carrier"
					, forwardingConsol.JK_OA_ShippingLineAddress_ZAddress.OrgHeader.PK
					, cost2.E6_OH_Creditor);

				AssertEquals("The first charge creditor will not change as a result of removing the consol creditor"
					, creditor.PK
					, cost.E6_OH_Creditor);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestCreditorDefaulting_ManuallyCreatedCharge_WithConsolIsDomesticAndCoLoad()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CC1.AC_IsGroupageCharge = true;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;

			var coLoadWith = Factory.NewWithValidTestData<OrgHeader>();
			coLoadWith.OH_IsCreditor = true;

			var exportCreditorAddress = Factory.NewWithValidTestData<OrgHeader>();
			exportCreditorAddress.OH_IsCreditor = true;

			var forwardingConsol = Factory.New<ForwardingConsol>();
			forwardingConsol.JK_RL_NKLoadPort = "AUSYD";
			forwardingConsol.JK_RL_NKDischargePort = "AUADL";
			forwardingConsol.JK_AgentType = "CLD";
			forwardingConsol.JK_OA_ShippingLineAddress = carrier.Addresses[0].PK;
			forwardingConsol.CreditorPK = coLoadWith.PK;
			forwardingConsol.CarrierExportCreditorAddress.OrganisationPK = exportCreditorAddress.PK;

			Factory.Save();
			var apps = new ApportionmentListing(Factory, forwardingConsol);

			try
			{
				var cost = apps.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = testObjectCreator.CC1.PK;
				AssertEquals("When the Co-Load Domestic Consol has a Carrier Export Creditor, then the charge creditor becomes the Carrier Export Creditor"
					, forwardingConsol.CarrierExportCreditorAddress.OrganisationPK
					, cost.E6_OH_Creditor);

				forwardingConsol.CarrierExportCreditorAddress.OrganisationPK = ZGuid.Empty;
				var cost2 = apps.CostsCollection.TryAddNew();
				cost2.E6_AC_ChargeCode = testObjectCreator.CC1.PK;
				AssertEquals("When the Co-Load Domestic Consol has no creditor, then the charge creditor becomes the Consol Co-Load With"
					, forwardingConsol.CreditorPK
					, cost2.E6_OH_Creditor);

				AssertEquals("The first charge creditor will not change as a result of removing the Carrier Export Creditor"
					, exportCreditorAddress.PK
					, cost.E6_OH_Creditor);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestCreditorDefaulting_ManuallyCreatedCharge_WithNotCLDConsolIsCrossTrade()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CC1.AC_IsGroupageCharge = true;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;

			var carrierImportCreditor = Factory.NewWithValidTestData<OrgHeader>();
			carrierImportCreditor.OH_IsCreditor = true;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "UAACK";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_OA_ShippingLineAddress = carrier.Addresses[0].PK;
			Factory.Save();
			AssertEquals("Pre-condition", consol.IsCrossTrade(), true);
			AssertEquals("Pre-condition", consol.IsCoLoad, false);

			var apps1 = new ApportionmentListing(Factory, consol);

			try
			{
				var cost = apps1.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = testObjectCreator.CC1.PK;
				AssertEquals("Consol Charge Creditor's PK should be equal to Carrier when Carrier Import Creditor is empty", carrier.PK, cost.E6_OH_Creditor);

				consol.CreditorPK = carrierImportCreditor.PK;
				Factory.Save();

				var cost2 = apps1.CostsCollection.TryAddNew();
				cost2.E6_AC_ChargeCode = testObjectCreator.CC1.PK;
				AssertEquals("Consol Charge Creditor's PK should be equal to Carrier Import Creditor when Carrier Import Creditor is not empty", carrierImportCreditor.PK, cost2.E6_OH_Creditor);
			}
			finally
			{
				apps1.ReleaseMutexes();
			}
		}

		public void TestCreditorDefaulting_ManuallyCreatedCharge_WithCLDConsolIsCrossTrade()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CC1.AC_IsGroupageCharge = true;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;

			var exportCreditorAddress = Factory.NewWithValidTestData<OrgHeader>();
			exportCreditorAddress.OH_IsCreditor = true;

			var coLoadWith = Factory.New<OrgHeader>();
			coLoadWith.OH_FullName = "Transport Provider 2";
			coLoadWith.MainAddress.OA_Address1 = "123 Fake Street";
			coLoadWith.MainAddress.OA_City = "Sydney";
			coLoadWith.MainAddress.OA_State = "NSW";
			coLoadWith.MainAddress.OA_PostCode = "2000";
			coLoadWith.OH_RL_NKClosestPort = "AUSYD";
			coLoadWith.OH_Code = "TRASPROV2";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "UAACK";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_OA_ShippingLineAddress = carrier.Addresses[0].PK;
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.CreditorPK = coLoadWith.PK;

			Factory.Save();
			AssertEquals("Pre-condition", consol.IsCrossTrade(), true);
			AssertEquals("Pre-condition", consol.IsCoLoad, true);

			var apps = new ApportionmentListing(Factory, consol);

			try
			{
				var cost = apps.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = testObjectCreator.CC1.PK;
				AssertEquals("Consol Charge Creditor's PK should be equal to co-load with when Carrier Import Creditor is empty", coLoadWith.PK, cost.E6_OH_Creditor);

				consol.CreditorPK = exportCreditorAddress.PK;
				Factory.Save();

				var cost2 = apps.CostsCollection.TryAddNew();
				cost2.E6_AC_ChargeCode = testObjectCreator.CC1.PK;
				AssertEquals("Consol Charge Creditor's PK should be equal to Carrier Import Creditor with when Carrier Import Creditor is not empty", exportCreditorAddress.PK, cost2.E6_OH_Creditor);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestRatingBehaviour_InvoiceReversed_REA_Then_REA()
		{
			AssertInvoiceReversedRatingBehaviour("REA", new[] { "REA", "NEW", "STP" });
		}

		public void TestRatingBehaviour_InvoiceReversed_NEW_Then_REA()
		{
			AssertInvoiceReversedRatingBehaviour("NEW", new[] { "REA", "NEW", "STP" });
		}

		public void TestRatingBehaviour_InvoiceReversed_STP_Then_REA()
		{
			AssertInvoiceReversedRatingBehaviour("STP", new[] { "REA", "NEW", "STP" });
		}

		public void TestRatingBehaviour_InvoiceReversed_SPT_Then_REA()
		{
			AssertInvoiceReversedRatingBehaviour(RatingBehaviours.Spot, new[] { "REA", "NEW", "STP" });
		}

		void AssertInvoiceReversedRatingBehaviour(string currentRatingBehaviour, IEnumerable<string> expectedList)
		{
			var jobConsolCost = Factory.NewWithValidTestData<JobConsolCost>();
			jobConsolCost.E6_RatingBehaviour = currentRatingBehaviour;
			var consolAPInvoice = Factory.New<APInvoice>();
			consolAPInvoice.AH_TransactionNum = "1111";

			jobConsolCost.E6_AH_APInvoice = consolAPInvoice.PK; // posting
			Factory.Save();

			AssertEquals("STP", jobConsolCost.E6_RatingBehaviour);
			Assert(jobConsolCost.IsPosted);
			AssertContainsExactElementsInAnyOrder(new string[] { "STP", "NEW" }, jobConsolCost.Lookups.RatingBehaviourList.GetAllCodes());

			jobConsolCost.E6_AH_APInvoice = Guid.Empty; // unpost
			Factory.Save();

			AssertEquals("REA", jobConsolCost.E6_RatingBehaviour);
			Assert(!jobConsolCost.IsPosted);
			AssertContainsExactElementsInAnyOrder(expectedList, jobConsolCost.Lookups.RatingBehaviourList.GetAllCodes());
		}

		public void TestRatingBehaviour_ManuallyUpdateCostAmt()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = testObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var jobConsolCost = testObjectCreator.CreateConsolCost(consol, testObjectCreator.CC1);
			jobConsolCost.E6_LocalCostAmount = 100M;
			jobConsolCost.E6_RatingBehaviour = "REA";

			AssertEquals("REA", jobConsolCost.E6_RatingBehaviour);
			jobConsolCost.E6_LocalCostAmount = 200m; // Manually update cost
			AssertEquals("NEW", jobConsolCost.E6_RatingBehaviour);
		}

		public void TestRatingBehaviour_UpdatingExchangeRateShouldNotUpdateRatingBehaviour()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = testObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var jobConsolCost = testObjectCreator.CreateConsolCost(consol, testObjectCreator.CC1);

			AssertRatingBehaviorWithExchangeRate(RatingBehaviours.ReAutorateCharge);
			AssertRatingBehaviorWithExchangeRate(RatingBehaviours.CreateNewCharge);

			void AssertRatingBehaviorWithExchangeRate(string ratingBehaviour)
			{
				jobConsolCost.CostExchangeRate.Currency = "AUD";
				jobConsolCost.CostExchangeRate.Rate = 1m;
				jobConsolCost.E6_OSCostAmount = 100M;
				jobConsolCost.E6_RatingBehaviour = ratingBehaviour;

				AssertEquals("Pre-condition: E6_LocalCostAmount", 100M, jobConsolCost.E6_LocalCostAmount);

				jobConsolCost.CostExchangeRate.Currency = "USD";
				jobConsolCost.CostExchangeRate.Rate = 0.8m;

				AssertEquals("Pre-condition: E6_OSCostAmount", 100M, jobConsolCost.E6_OSCostAmount);
				AssertEquals("Pre-condition: E6_LocalCostAmount", 125M, jobConsolCost.E6_LocalCostAmount);
				AssertEquals(ratingBehaviour, jobConsolCost.E6_RatingBehaviour);
			}
		}

		public void TestRatingBehaviour_WhenGatewayConsolCost()
		{
			var gatewayAgentPort = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(gatewayAgentPort);
			consol.JK_UniqueConsignRef = "C00001111";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			consol.Shipments.Add(shipment);
			using (var job = new Job.Loader(Factory, consol).TryCreateWithoutMutexForTestOnly())
			{
				var charge = job.Charges.AddNew();
				charge.JR_AC = Env.Registry.FreightChargeCode;
				charge.JR_OSSellAmt = 250m;
				charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
				charge.JR_JH_InternalJob = job.PK;
				charge.JR_GE_InternalDept = charge.JR_GE;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				Factory.Save();

				Assert("Should be a gateway consol", consol.IsGateway());
				var cost = (JobConsolCost)consol.GetApportionments(true).CostsCollection.Single();
				Factory.Save();

				AssertEquals("E6_RatingBehaviour value should be empty", ZString.Empty, cost.E6_RatingBehaviour);
				Assert("E6_RatingBehaviour should be read-only", cost.E6_RatingBehaviourInfo.ReadOnly);
			}
		}

		public void TestConsolCostIsNotDeletedWhenActiveARCashAdvanceRequestExistForApportionChargeAndCashAdvanceFunctionalityIsEnabled()
		{
			AssertConsolCostIsNotDeletedWhenActiveCashAdvanceRequestExistForApportionCharge(true, true);
		}

		public void TestConsolCostIsNotDeletedWhenActiveAPCashAdvanceRequestExistForApportionChargeAndCashAdvanceFunctionalityIsEnabled()
		{
			AssertConsolCostIsNotDeletedWhenActiveCashAdvanceRequestExistForApportionCharge(false, true);
		}

		public void TestConsolCostIsNotDeletedWhenActiveARCashAdvanceRequestExistForApportionChargeAndCashAdvanceFunctionalityIsDisabled()
		{
			AssertConsolCostIsNotDeletedWhenActiveCashAdvanceRequestExistForApportionCharge(true, false);
		}

		public void TestConsolCostIsNotDeletedWhenActiveAPCashAdvanceRequestExistForApportionChargeAndCashAdvanceFunctionalityIsDisabled()
		{
			AssertConsolCostIsNotDeletedWhenActiveCashAdvanceRequestExistForApportionCharge(false, false);
		}

		void AssertConsolCostIsNotDeletedWhenActiveCashAdvanceRequestExistForApportionCharge(bool isARCashAdvance, bool isCashAdvanceFunctionalityEnabled)
		{
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isCashAdvanceFunctionalityEnabled))
			using (AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isCashAdvanceFunctionalityEnabled))
			{
				var consol = ObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
				var shipment1 = ObjectCreator.CreateShipment("S00001", consol);
				var consolCost1 = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, 100m, TestObjectCreator.AALSHI);
				consolCost1.ApportionmentCharges[0].JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
				consolCost1.ApportionmentCharges[0].JR_OSSellAmt = 100m;
				consolCost1.ApportionmentCharges[0].JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
				Factory.Save();

				Assert("cashAdvanceLine1 is not created yet.", consolCost1.CanDelete);

				if (isARCashAdvance)
				{
					consolCost1.ApportionmentCharges[0].JR_IsARCashAdvance = true;
				}
				else
				{
					consolCost1.ApportionmentCharges[0].JR_IsAPCashAdvance = true;
				}
				Factory.Save();

				Assert("cashAdvanceLine1 is not created yet.", consolCost1.CanDelete);

				var ledger = isARCashAdvance ? LedgerTypes.AccountsReceivable : LedgerTypes.AccountsPayable;
				var organization = isARCashAdvance ? TestObjectCreator.ABIGAS : TestObjectCreator.AALSHI;
				var cashAdvanceHeader = TestObjectCreator.CreateCashAdvanceRequestHeader(shipment1.Job.PK, organization.PK, ledger, 100m, 100m, TestObjectCreator.AUD.RX_Code, CashAdvanceStatusCodes.RequestHeader.Requested);
				var cashAdvanceLine1 = TestObjectCreator.CreateCashAdvanceRequestLine(cashAdvanceHeader.PK, 100m, 100m, CashAdvanceStatusCodes.RequestLine.Requested);
				if (isARCashAdvance)
				{
					consolCost1.ApportionmentCharges[0].JR_CAL_ARLine = cashAdvanceLine1.PK;
				}
				else
				{
					consolCost1.ApportionmentCharges[0].JR_CAL_APLine = cashAdvanceLine1.PK;
				}
				Factory.Save();

				var expectedReasonForNotAbleToDelete = $"Unable to delete the {ObjectCreator.CC1.AC_Code} charge as it has an active " + (isARCashAdvance ? "AR" : "AP") + " Advance Payment. Please cancel the Advance Payment if you need to delete this charge.";
				if (isCashAdvanceFunctionalityEnabled)
				{
					Assert("cashAdvanceLine1 is in REQ status.", !consolCost1.CanDelete);
					AssertEquals(expectedReasonForNotAbleToDelete, consolCost1.ReasonForNotAbleToDelete);
				}
				else
				{
					Assert(consolCost1.CanDelete);
				}

				cashAdvanceLine1.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;
				cashAdvanceLine1.CAL_LocalPaidAmount = cashAdvanceLine1.CAL_OSPaidAmount = 100m;
				cashAdvanceHeader.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Paid;
				cashAdvanceHeader.CAH_LocalPaidAmount = cashAdvanceHeader.CAH_OSPaidAmount = 100m;
				Factory.Save();

				if (isCashAdvanceFunctionalityEnabled)
				{
					Assert("cashAdvanceLine1 is in PAI status.", !consolCost1.CanDelete);
					AssertEquals(expectedReasonForNotAbleToDelete, consolCost1.ReasonForNotAbleToDelete);
				}
				else
				{
					Assert(consolCost1.CanDelete);
				}

				cashAdvanceLine1.CAL_Status = CashAdvanceStatusCodes.RequestLine.Invoiced;
				cashAdvanceHeader.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Invoiced;
				Factory.Save();

				if (isCashAdvanceFunctionalityEnabled)
				{
					Assert("cashAdvanceLine1 is in INV status.", !consolCost1.CanDelete);
					AssertEquals(expectedReasonForNotAbleToDelete, consolCost1.ReasonForNotAbleToDelete);
				}
				else
				{
					Assert(consolCost1.CanDelete);
				}

				cashAdvanceLine1.CAL_Status = CashAdvanceStatusCodes.RequestLine.Cancelled;
				cashAdvanceLine1.CAL_LocalPaidAmount = cashAdvanceLine1.CAL_OSPaidAmount = 0m;
				cashAdvanceHeader.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Cancelled;
				cashAdvanceHeader.CAH_LocalPaidAmount = cashAdvanceHeader.CAH_OSPaidAmount = 0m;
				Factory.Save();

				Assert("cashAdvanceLine1 is in CAN status.", consolCost1.CanDelete);
			}
		}

		public void TestErrorIsReportedWhenNonApplicableApportionSplitChargeIsNotDeleted()
		{
			var consol = ObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var shipment1 = ObjectCreator.CreateShipment("S00001", consol);
			var shipment2 = ObjectCreator.CreateShipment("S00002", consol);
			shipment2.JS_ActualWeight = shipment2.JS_ActualVolume = 0m;
			var consolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, 100m);
			AssertEquals(2, consolCost.ApportionmentCharges.Count);
			var apportionSplitCharges = consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>();
			var shipment1Charge = apportionSplitCharges.FirstOrDefault(x => x.JR_JobNumber == shipment1.JS_UniqueConsignRef && x.JR_OSCostAmt == 100m);
			AssertNotNull(shipment1Charge);
			var shipment2Charge = apportionSplitCharges.FirstOrDefault(x => x.JR_JobNumber == shipment2.JS_UniqueConsignRef && x.JR_OSCostAmt == 0m);
			AssertNotNull(shipment2Charge);
			ErrorReporter.Clear();
			using (new DisposableAction(
				() => JobConsolCost.IsForceToSkipDeletingApportionSplitCharge_ForTestOnly = true,
				() => JobConsolCost.IsForceToSkipDeletingApportionSplitCharge_ForTestOnly = false))
			{
				consolCost.RemoveNonApplicableCharges();
			}
			Assert(!shipment2Charge.IsDeleted);
			AssertEquals("NonApplicableApportionSplitChargeNotDeleted", ErrorReporter.LastKeyReported);
			AssertContains($"Charge: PK = {shipment2Charge.PK}", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
			shipment1.Job.Dispose();
			shipment2.Job.Dispose();
		}

		public void TestCriticalValidationInfoWhenParentIdIsSetToEmptyFromNonempty()
		{
			var consol = ObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var shipment = ObjectCreator.CreateShipment("S00001", consol);
			ObjectCreator.CreateJob(shipment, false);
			var infoCollector = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			var expectedMessageWhenInfoNotCollected = "ConsolCostParentChangedFromNonEmptyToEmpty: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.";
			var expectedMessageWhenInfoCollected = FormattableString.Invariant($@"ConsolCostParentChangedFromNonEmptyToEmpty:

E6_ParentId has been changed from {consol.PK} to 00000000-0000-0000-0000-000000000000.
   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)
   at System.Environment.get_StackTrace()
");

			var consolCost1 = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, 100m);
			AssertContains("Info should not be collected because E6_ParentId is not empty.", expectedMessageWhenInfoNotCollected, GetCriticalInfo(consolCost1));

			using (consolCost1.ReportSettingParentSuspender.GetSuspender())
			{
				consolCost1.SetE6_ParentIDAndE6_ParentTableCodeTogether(ZGuid.Empty, ZString.Empty);
			}
			AssertContains("Info should be collected because E6_ParentId is empty.", expectedMessageWhenInfoCollected, GetCriticalInfo(consolCost1));

			var consolCost2 = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC2, 200m);
			AssertContains("Info should not be collected because E6_ParentId is not empty.", expectedMessageWhenInfoNotCollected, GetCriticalInfo(consolCost2));

			using (consolCost2.ReportSettingParentSuspender.GetSuspender())
			{
				consolCost2.E6_ParentID = ZGuid.Empty;
			}
			AssertContains("Info should be collected because E6_ParentId is empty.", expectedMessageWhenInfoCollected, GetCriticalInfo(consolCost2));

			string GetCriticalInfo(JobConsolCost consolCost) => infoCollector.GetInfo(consolCost.PK, CriticalValidationInfoCollectorServiceKeyType.ConsolCostParentChangedFromNonEmptyToEmpty);
		}

		public void TestErrorIsNotReportedWhenThereIsMismatchBetweenChargeAndConsolTaxDate_DateUpdatedFromConsol()
		{
			var consol = ObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var consolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, 100m);

			consolCost.E6_AT_TaxRate = ObjectCreator.GST1.PK;
			consolCost.E6_TaxDate = ZDateTime.Today.Date;
			consolCost.E6_AT_TaxRate = ZGuid.Empty;

			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestExchangeRateConfigurationRateConsumer()
		{
			var forwardingConsol = ObjectCreator.CreateConsol();
			forwardingConsol.JK_TransportMode = Core.Constants.TransportModes.SeaAir;
			var transport = forwardingConsol.Transports[0];
			transport.JW_ETD = new ZDateTime(2018, 3, 10);
			transport.JW_ETA = new ZDateTime(2018, 2, 9);
			var forwardingConsolCost1 = ObjectCreator.CreateConsolCost(forwardingConsol, ObjectCreator.CC1, ObjectCreator.USD, 1.342m, 100m);
			var freightChargeCode = Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);
			var forwardingConsolCost2 = ObjectCreator.CreateConsolCost(forwardingConsol, freightChargeCode, ObjectCreator.USD, 0.187m, 100m);

			var rateConsumer = forwardingConsolCost1.ExchangeRateConfigurationRateConsumer;
			AssertEquals(JobInvoicingConsumerTypes.ForwardingConsol.Code, rateConsumer.JobType);
			AssertEquals(Constants.FreightShipmentDirection.Code.Export, rateConsumer.Direction);
			AssertEquals(Core.Constants.TransportModes.SeaAir, rateConsumer.TransportMode);
			AssertEquals(ZGuid.Empty, rateConsumer.LocalClientPK);
			AssertEquals(GlbCompany.CurrentCompany.PK, rateConsumer.Company.PK);
			AssertEquals(ZDateTime.Today, rateConsumer.ConsolExchangeRateDate);
			AssertEquals(new ZDateTime(2018, 2, 9), rateConsumer.HistoricalRateFromActualArrivalDate);
			AssertEquals(new ZDateTime(2018, 3, 10), rateConsumer.HistoricalRateFromActualDepartureDate);
			AssertEquals(new ZDateTime(2018, 2, 9), rateConsumer.HistoricalRateFromEstimatedArrivalDate);
			AssertEquals(new ZDateTime(2018, 3, 10), rateConsumer.HistoricalRateFromEstimatedDepartureDate);
			AssertEquals(ZDateTime.Empty, rateConsumer.HistoricalRateFromEstimatedArrivalAtLoadPortDate);
			AssertEquals(ZDateTime.Empty, rateConsumer.HistoricalRateFromActualArrivalAtLoadPortDate);
			AssertEquals(ZDateTime.Empty, rateConsumer.PickupDate);
			AssertEquals(ZDateTime.Empty, rateConsumer.DeliveryDate);
			AssertEquals(ZDateTime.Empty, rateConsumer.RequiredDate);
			AssertEquals(ZDateTime.Empty, rateConsumer.FinalizedDate);
			AssertEquals(0.187m, rateConsumer.GetExchangeRateForConsolExchangeRatePreference(ObjectCreator.USD));
		}

		public void TestExchangeRateConfigurationRateConsumerJobType()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var gatewayConsol = testObjectCreator.CreateGatewayConsol(consolNum: "C0123", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment = testObjectCreator.CreateShipment("S001", gatewayConsol);
			var gatewayJob = testObjectCreator.CreateJob(gatewayConsol, false);
			var shipmentJob = testObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var gatewayConsolCost = testObjectCreator.CreateConsolCost(gatewayConsol, testObjectCreator.FRT);

			var sellAppCharge = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, testObjectCreator.FRT.PK, 100);
			GatewaySellToCostSynchroniser.Synchronise(gatewayJob);
			Assert("Pre-condition: Valid Gateway app is generated", sellAppCharge.JR_E6_GatewaySellHeader.IsValid);
			var gatewaySellApportionment = Factory.Load<JobConsolCost>(sellAppCharge.JR_E6_GatewaySellHeader);

			var forwardingConsol = testObjectCreator.CreateConsol();
			var forwardingConsolCost = testObjectCreator.CreateConsolCost(forwardingConsol, testObjectCreator.CC11, 100);

			var transportBookingConsol = Factory.NewWithValidTestData(ObjectFactory.GetType<IDtbBookingConsolidation>());
			transportBookingConsol[DtbBookingConsolidationSchema.KB_JobType] = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation;
			var transportBookingConsolCost = Factory.New<JobConsolCost>();
			using (transportBookingConsolCost.ReportSettingParentSuspender.GetSuspender())
			{
				transportBookingConsolCost.SetE6_ParentIDAndE6_ParentTableCodeTogether(transportBookingConsol.PK, DtbBookingConsolidationSchema.Constants.Prefix);
			}

			var landTransportRunSheet = Factory.NewWithValidTestData<DtbConsignmentRunSheet>();
			var landTransportRunSheetConsolCost = Factory.New<JobConsolCost>();
			using (landTransportRunSheetConsolCost.ReportSettingParentSuspender.GetSuspender())
			{
				landTransportRunSheetConsolCost.SetE6_ParentIDAndE6_ParentTableCodeTogether(landTransportRunSheet.PK, DtbConsignmentRunSheetSchema.Constants.Prefix);
			}

			var manifest = Factory.NewWithValidTestData<DtbLinehaulManifest>();
			var manifestConsolCost = Factory.New<JobConsolCost>();
			using (manifestConsolCost.ReportSettingParentSuspender.GetSuspender())
			{
				manifestConsolCost.SetE6_ParentIDAndE6_ParentTableCodeTogether(manifest.PK, DtbLinehaulManifestSchema.Constants.Prefix);
			}

			var postTransportRunSheet = Factory.NewWithValidTestData<CommonWorkSheet>();
			var postTransportRunSheetConsolCost = Factory.New<JobConsolCost>();
			using (postTransportRunSheetConsolCost.ReportSettingParentSuspender.GetSuspender())
			{
				postTransportRunSheetConsolCost.SetE6_ParentIDAndE6_ParentTableCodeTogether(postTransportRunSheet.PK, JobCartageRunSheetSchema.Constants.Prefix);
			}

			AssertEquals(JobInvoicingConsumerTypes.ForwardingConsolCode, gatewayConsolCost.ExchangeRateConfigurationRateConsumer.JobType);
			AssertEquals(JobInvoicingConsumerTypes.GatewayConsolCode, gatewaySellApportionment.ExchangeRateConfigurationRateConsumer.JobType);
			AssertEquals(JobInvoicingConsumerTypes.ForwardingConsolCode, forwardingConsolCost.ExchangeRateConfigurationRateConsumer.JobType);
			AssertEquals(ZString.Empty, transportBookingConsolCost.ExchangeRateConfigurationRateConsumer.JobType);
			AssertEquals(ZString.Empty, landTransportRunSheetConsolCost.ExchangeRateConfigurationRateConsumer.JobType);
			AssertEquals(ZString.Empty, manifestConsolCost.ExchangeRateConfigurationRateConsumer.JobType);
			AssertEquals(ZString.Empty, postTransportRunSheetConsolCost.ExchangeRateConfigurationRateConsumer.JobType);
		}

		public void TesChangingCreditorWillCalculateExchangeRateAccordingToJobBillingExchangeRateConfiguration()
		{
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate(LedgerTypes.AccountsPayable, JobInvoicingConsumerTypes.ForwardingConsol.Code, Constants.TransportModes.All, Constants.FreightShipmentDirection.Code.All, Constants.ExchangeRateTypes.Code.SellRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate, 0, true);
			GlbCompany.CurrentCompany.Factory.Save();

			ObjectCreator.AALSHI.CompanyData.AccAPExchangeRateConfigurations.SetExRate(JobInvoicingConsumerTypes.ForwardingConsol.Code, Constants.TransportModes.All, Constants.FreightShipmentDirection.Code.All, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate, 0, true);
			ObjectCreator.ABIGAS.CompanyData.AccAPExchangeRateConfigurations.SetExRate(JobInvoicingConsumerTypes.ForwardingConsol.Code, Constants.TransportModes.All, Constants.FreightShipmentDirection.Code.All, Constants.ExchangeRateTypes.Code.CustomsRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate, 0, true);

			var sellRate = 1.1222m;
			var buyRate = 1.2333m;
			var customsRate = 1.3444m;
			ObjectCreator.CreateExchangeRate(ObjectCreator.USD, Constants.ExchangeRateTypes.Code.SellRate, sellRate, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			ObjectCreator.CreateExchangeRate(ObjectCreator.USD, Constants.ExchangeRateTypes.Code.BuyRate, buyRate, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			ObjectCreator.CreateExchangeRate(ObjectCreator.USD, Constants.ExchangeRateTypes.Code.CustomsRate, customsRate, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));

			Factory.Save();

			var consol = ObjectCreator.CreateConsol("AUSYD", "KRSEL", "C00001001");
			var consolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1);
			consolCost.E6_OSCostAmount = 100m;
			AssertEquals(ZGuid.Empty, consolCost.E6_OH_Creditor);
			AssertEquals(1m, consolCost.E6_ExchangeRate);

			consolCost.E6_RX_NKCurrency = Constants.CurrencyCodes.UnitedStates;
			AssertEquals(sellRate, consolCost.E6_ExchangeRate);

			consolCost.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			AssertEquals(buyRate, consolCost.E6_ExchangeRate);

			consolCost.E6_OH_Creditor = ObjectCreator.ABIGAS.PK;
			AssertEquals(customsRate, consolCost.E6_ExchangeRate);
		}

		public void TesChangingCurrencyWillCalculateExchangeRateAccordingToJobBillingExchangeRateConfiguration()
		{
			ObjectCreator.AALSHI.CompanyData.AccAPExchangeRateConfigurations.SetExRate(JobInvoicingConsumerTypes.ForwardingConsol.Code, Constants.TransportModes.All, Constants.FreightShipmentDirection.Code.All, Constants.ExchangeRateTypes.Code.SellRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate, 0, true);
			ObjectCreator.ABIGAS.CompanyData.AccAPExchangeRateConfigurations.SetExRate(JobInvoicingConsumerTypes.ForwardingConsol.Code, Constants.TransportModes.All, Constants.FreightShipmentDirection.Code.All, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate, 0, true);

			var sellRateForUSD = 1.1222m;
			var sellRateForEUR = 1.2333m;
			var sellRateForGBP = 1.3444m;
			CreateExchangeRateForMultipleCurrencies(Constants.ExchangeRateTypes.Code.SellRate, sellRateForUSD, sellRateForEUR, sellRateForGBP);
			var buyRateForUSD = 2.4555m;
			var buyRateForEUR = 2.5666m;
			var buyRateForGBP = 2.6777m;
			CreateExchangeRateForMultipleCurrencies(Constants.ExchangeRateTypes.Code.BuyRate, buyRateForUSD, buyRateForEUR, buyRateForGBP);
			Factory.Save();

			var consol = ObjectCreator.CreateConsol("AUSYD", "KRSEL", "C00001001");
			var consolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1);
			consolCost.E6_OSCostAmount = 100m;
			consolCost.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			AssertConsolCostExchangeRate(sellRateForUSD, sellRateForEUR, sellRateForGBP);

			consolCost.E6_OH_Creditor = ObjectCreator.ABIGAS.PK;
			AssertConsolCostExchangeRate(buyRateForUSD, buyRateForEUR, buyRateForGBP);

			void CreateExchangeRateForMultipleCurrencies(ZString rateType, ZDecimal rateForUSD, ZDecimal rateForEUR, ZDecimal rateForGBP)
			{
				ObjectCreator.CreateExchangeRate(ObjectCreator.USD, rateType, rateForUSD, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
				ObjectCreator.CreateExchangeRate(ObjectCreator.EUR, rateType, rateForEUR, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
				ObjectCreator.CreateExchangeRate(ObjectCreator.GBP, rateType, rateForGBP, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			}

			void AssertConsolCostExchangeRate(decimal expectedRateForUSD, decimal expectedRateForEUR, decimal expectedRateForGBP)
			{
				consolCost.E6_RX_NKCurrency = Constants.CurrencyCodes.UnitedStates;
				AssertEquals(expectedRateForUSD, consolCost.E6_ExchangeRate);

				consolCost.E6_RX_NKCurrency = Constants.CurrencyCodes.EuropeanUnion;
				AssertEquals(expectedRateForEUR, consolCost.E6_ExchangeRate);

				consolCost.E6_RX_NKCurrency = Constants.CurrencyCodes.UnitedKingdom;
				AssertEquals(expectedRateForGBP, consolCost.E6_ExchangeRate);
			}
		}

		public void TestJR_IsUsedForApportionmentAlwaysInitialized()
		{
			var consol = ObjectCreator.CreateConsol();
			var shipment1 = ObjectCreator.CreateShipment("S001", consol);
			var shipment2 = ObjectCreator.CreateShipment("S002", consol);
			var shipment3 = ObjectCreator.CreateShipment("S003", consol);
			ObjectCreator.CreateJob(shipment1, false);
			ObjectCreator.CreateJob(shipment2, false);
			ObjectCreator.CreateJob(shipment3, false);
			Factory.Save();

			var consolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.FRT, 100);
			AssertEquals("Precondition: ApportionmentCharges.Count", 3, consolCost.ApportionmentCharges.Count);
			GetCharge(consolCost, "S002").JR_IsUsedForApportionment = false;

			Factory.Save();

			AssertEquals("Precondition: ApportionmentCharges.Count", 3, consolCost.ApportionmentCharges.Count);
			Assert("Precondition: charge 1 JR_IsUsedForApportionment", GetCharge(consolCost, "S001").JR_IsUsedForApportionment);
			Assert("Precondition: charge 2 JR_IsUsedForApportionment", !GetCharge(consolCost, "S002").JR_IsUsedForApportionment);
			Assert("Precondition: charge 3 JR_IsUsedForApportionment", GetCharge(consolCost, "S003").JR_IsUsedForApportionment);

			var newFactory = new BusinessObjectFactory();
			var consolCostInNewFactory = newFactory.Load<JobConsolCost>(consolCost.PK);
			AssertEquals("ApportionmentCharges.Count", 2, consolCostInNewFactory.ApportionmentCharges.Count);
			Assert("charge 1 JR_IsUsedForApportionment", GetCharge(consolCostInNewFactory, "S001").JR_IsUsedForApportionment);
			Assert("charge 3 JR_IsUsedForApportionment", GetCharge(consolCostInNewFactory, "S003").JR_IsUsedForApportionment);

			ApportionSplitCharge GetCharge(JobConsolCost cost, ZString jobNumber) => cost.ApportionmentCharges.Cast<ApportionSplitCharge>().First(x => x.Job.JH_JobNum == jobNumber);
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesJobConsolCost()
		{
			var cost = Factory.New<JobConsolCost>();

			var localList = new List<string>
			{
				nameof(cost.E6_LocalCostAmount),
				nameof(cost.E6_LocalCostAmount),
				nameof(cost.E6_LocalCostAmount),
				nameof(cost.E6_LocalCostAmount),
				nameof(cost.E6_LocalCostAmount),
				nameof(cost.E6_LocalCostAmount),
				nameof(cost.E6_LocalCostAmount),
				nameof(cost.E6_LocalCostAmount)
			};

			var osList = new List<string>
			{
				nameof(cost.AgentDeclaredOSAmount),
				nameof(cost.AgentDeclaredOSAmount),
				nameof(cost.AgentDeclaredOSAmount),
				nameof(cost.AgentDeclaredOSAmount),
				nameof(cost.AgentDeclaredOSAmount),
				nameof(cost.AgentDeclaredOSAmount),
				nameof(cost.AgentDeclaredOSAmount),
				nameof(cost.GSTInclusiveAmount)
			};

			var invoiceList = new List<string>
			{
				nameof(cost.InvoiceOSTotal),
				nameof(cost.InvoiceOSTotal)
			};

			var exList = new List<string>
			{
				nameof(cost.E6_ExchangeRate)
			};

			var tester = new DecimalPlacesAttributeTester(cost, cost.Company);
			tester.CheckLocalCurrency(localList, nameof(cost.LocalCurrencyDecimals));
			tester.CheckNonLocalCurrency(osList, nameof(cost.CurrencyDecimals), nameof(cost.E6_RX_NKCurrency), cost);
			tester.CheckNonLocalCurrency(invoiceList, nameof(cost.InvoiceCurrencyDecimals), nameof(cost.E6_RX_NKCurrency), cost);
			tester.CheckExchangeRate(exList, nameof(cost.ExchangeRateDecimalPlaces));
		}

		public void TestSaveJobConsolCostFromOtherCompanyContext()
		{
			var creator = new TestObjectCreator(Factory);
			var currencyCompanyA = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			var otherCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RX_NKLocalCurrency, SQLComparisonOperator.NotEqual, currencyCompanyA) { OrderBy = GlbCompanySchema.GC_Code.Name });

			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			Factory.Save();
			var apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost1;
			try
			{
				cost1 = apps.CostsCollection.TryAddNew();
				cost1.E6_ApportionmentMethod = AllocationMethod.ChargeableUnits;
				cost1.E6_AC_ChargeCode = creator.FRT.PK;
				cost1.E6_OSCostAmount = 100;
				Factory.Save();
			}
			finally
			{
				apps.ReleaseMutexes();
			}

			JobConsolCost cost2;
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, otherCompany.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var factory2 = new BusinessObjectFactory();
				var consol2 = factory2.Load<ForwardingConsol>(consol.PK);
				var apps2 = new ApportionmentListing(factory2, consol2);
				try
				{
					cost2 = apps2.CostsCollection.TryAddNew();
					cost2.E6_ApportionmentMethod = AllocationMethod.ChargeableUnits;
					cost2.E6_AC_ChargeCode = creator.FRT.PK;
					cost2.E6_RX_NKCurrency = currencyCompanyA;
					cost2.E6_ExchangeRate = 1.5m;
					cost2.E6_OSCostAmount = 200;
					factory2.Save();
				}
				finally
				{
					apps2.ReleaseMutexes();
				}
			}

			var factory3 = new BusinessObjectFactory();
			var consol3 = factory3.Load<ForwardingConsol>(consol.PK);
			var apps3 = new ApportionmentListing(factory3, consol3);
			factory3.Load<JobConsolCost>(cost1.PK);
			apps3.CostsCollection.Load();
			apps3.ReleaseMutexes();

			JobConsolCost costInWrongContext;
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, otherCompany.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				costInWrongContext = factory3.Load<JobConsolCost>(cost2.PK);
				var apps4 = new ApportionmentListing(factory3, consol3);
				apps4.CostsCollection.Load();
				apps4.ReleaseMutexes();
			}

			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, costInWrongContext.E6_RX_NKCurrency);
			AssertNotEquals(1m, costInWrongContext.E6_ExchangeRate);

			ErrorReporter.Instance.Clear();
			factory3.Save();
			AssertEquals("costInWrongContext in factory3 should be saved using the context from CompanyB and not the current context.", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestFreeSpaceContributionApportionment()
		{
			var consol = ObjectCreator.CreateConsol();
			var shipment1 = ObjectCreator.CreateShipment("S001", consol);
			shipment1.JS_ActualWeight = 300;
			shipment1.JS_ActualVolume = 0.308;
			shipment1.JS_ActualChargeable = 300;
			ObjectCreator.CreateJob(shipment1, false);
			var shipment2 = ObjectCreator.CreateShipment("S002", consol);
			shipment2.JS_ActualWeight = 5.8;
			shipment2.JS_ActualVolume = 0.028;
			shipment2.JS_ActualChargeable = 6;
			ObjectCreator.CreateJob(shipment2, false);
			var shipment3 = ObjectCreator.CreateShipment("S003", consol);
			shipment3.JS_ActualWeight = 66;
			shipment3.JS_ActualVolume = 0.384;
			shipment3.JS_ActualChargeable = 66;
			ObjectCreator.CreateJob(shipment3, false);
			var shipment4 = ObjectCreator.CreateShipment("S004", consol);
			shipment4.JS_ActualWeight = 1072;
			shipment4.JS_ActualVolume = 6.912;
			shipment4.JS_ActualChargeable = 1152;
			ObjectCreator.CreateJob(shipment4, false);
			var shipment5 = ObjectCreator.CreateShipment("S005", consol);
			shipment5.JS_ActualWeight = 93.9;
			shipment5.JS_ActualVolume = 0.682;
			shipment5.JS_ActualChargeable = 114;
			ObjectCreator.CreateJob(shipment5, false);
			var shipment6 = ObjectCreator.CreateShipment("S006", consol);
			shipment6.JS_ActualWeight = 292.7;
			shipment6.JS_ActualVolume = 2.539;
			shipment6.JS_ActualChargeable = 423.5;
			ObjectCreator.CreateJob(shipment6, false);
			var shipment7 = ObjectCreator.CreateShipment("S007", consol);
			shipment7.JS_ActualWeight = 39.6;
			shipment7.JS_ActualVolume = 0.372;
			shipment7.JS_ActualChargeable = 62;
			ObjectCreator.CreateJob(shipment7, false);
			Factory.Save();
			var consolCost = GetCost(consol);
			consolCost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			consolCost.E6_OSCostAmount = 5163.96;
			consolCost.E6_ApportionmentMethod = AllocationMethod.FreeSpaceContribution;
			AssertEquals("charge 1 JR_LocalCostAmt", 483.7m, consolCost.ApportionmentCharges[0].JR_LocalCostAmt);
			AssertEquals("charge 1 ChargeableRate", "1.6123", consolCost.ApportionmentCharges[0].ChargeableRate);
			AssertEquals("charge 2 JR_LocalCostAmt", 14.99m, consolCost.ApportionmentCharges[1].JR_LocalCostAmt);
			AssertEquals("charge 2 ChargeableRate", "2.4983", consolCost.ApportionmentCharges[1].ChargeableRate);
			AssertEquals("charge 3 JR_LocalCostAmt", 179.41m, consolCost.ApportionmentCharges[2].JR_LocalCostAmt);
			AssertEquals("charge 3 ChargeableRate", "2.7183", consolCost.ApportionmentCharges[2].ChargeableRate);
			AssertEquals("charge 4 JR_LocalCostAmt", 3069.38m, consolCost.ApportionmentCharges[3].JR_LocalCostAmt);
			AssertEquals("charge 4 ChargeableRate", "2.6644", consolCost.ApportionmentCharges[3].ChargeableRate);
			AssertEquals("charge 5 JR_LocalCostAmt", 287.38m, consolCost.ApportionmentCharges[4].JR_LocalCostAmt);
			AssertEquals("charge 5 ChargeableRate", "2.5209", consolCost.ApportionmentCharges[4].ChargeableRate);
			AssertEquals("charge 6 JR_LocalCostAmt", 988.88m, consolCost.ApportionmentCharges[5].JR_LocalCostAmt);
			AssertEquals("charge 6 ChargeableRate", "2.335", consolCost.ApportionmentCharges[5].ChargeableRate);
			AssertEquals("charge 7 JR_LocalCostAmt", 140.22m, consolCost.ApportionmentCharges[6].JR_LocalCostAmt);
			AssertEquals("charge 7 ChargeableRate", "2.2616", consolCost.ApportionmentCharges[6].ChargeableRate);
		}

		public void TestReadonlyFieldsForForeignCurrencyParentAPInvoice()
		{
			var consolCost = (JobConsolCost)GetNewBusinessObject();
			Assert("E6_RX_NKCurrency read only state", !consolCost.E6_RX_NKCurrencyInfo.ReadOnly);
			Assert("E6_ExchangeRate read only state", !consolCost.E6_ExchangeRateInfo.ReadOnly);
			Assert("E6_LocalCostAmount read only state", !consolCost.E6_LocalCostAmountInfo.ReadOnly);
			consolCost.ParentAPInvoice = Factory.New<APInvoice>();
			Assert("E6_RX_NKCurrency read only state", !consolCost.E6_RX_NKCurrencyInfo.ReadOnly);
			Assert("E6_ExchangeRate read only state", !consolCost.E6_ExchangeRateInfo.ReadOnly);
			Assert("E6_LocalCostAmount read only state", !consolCost.E6_LocalCostAmountInfo.ReadOnly);
			consolCost.ParentAPInvoice.AH_RX_NKTransactionCurrency = "USD";
			consolCost.ParentAPInvoice.AH_PostedToEFT = false;
			AssertNotEquals("Precondition: current company and invoice currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, consolCost.ParentAPInvoice.AH_RX_NKTransactionCurrency);
			Assert("E6_RX_NKCurrency read only state", consolCost.E6_RX_NKCurrencyInfo.ReadOnly);
			Assert("E6_ExchangeRate read only state", consolCost.E6_ExchangeRateInfo.ReadOnly);
			Assert("E6_LocalCostAmount read only state", consolCost.E6_LocalCostAmountInfo.ReadOnly);
			consolCost.ParentAPInvoice.AH_PostedToEFT = true;
			Assert("E6_RX_NKCurrency read only state", consolCost.E6_RX_NKCurrencyInfo.ReadOnly);
			Assert("E6_ExchangeRate read only state", !consolCost.E6_ExchangeRateInfo.ReadOnly);
			Assert("E6_LocalCostAmount read only state", consolCost.E6_LocalCostAmountInfo.ReadOnly);
			Env.Security.AllowAPInvoiceConsolCostExchangeRateOverride.IsAllowed = false;
			Assert("E6_RX_NKCurrency read only state", consolCost.E6_RX_NKCurrencyInfo.ReadOnly);
			Assert("E6_ExchangeRate read only state", consolCost.E6_ExchangeRateInfo.ReadOnly);
			Assert("E6_LocalCostAmount read only state", consolCost.E6_LocalCostAmountInfo.ReadOnly);
			consolCost.ParentAPInvoice.AH_RX_NKTransactionCurrency = "AUD";
			AssertEquals("Precondition: current company and invoice currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, consolCost.ParentAPInvoice.AH_RX_NKTransactionCurrency);
			Assert("E6_RX_NKCurrency read only state", !consolCost.E6_RX_NKCurrencyInfo.ReadOnly);
			Assert("E6_ExchangeRate read only state", consolCost.E6_ExchangeRateInfo.ReadOnly);
			Assert("E6_LocalCostAmount read only state", !consolCost.E6_LocalCostAmountInfo.ReadOnly);
			Env.Security.AllowAPInvoiceConsolCostExchangeRateOverride.IsAllowed = true;
			Assert("E6_RX_NKCurrency read only state", !consolCost.E6_RX_NKCurrencyInfo.ReadOnly);
			Assert("E6_ExchangeRate read only state", !consolCost.E6_ExchangeRateInfo.ReadOnly);
			Assert("E6_LocalCostAmount read only state", !consolCost.E6_LocalCostAmountInfo.ReadOnly);
		}

		public void TestCreditorSetDefaultCurrency()
		{
			var consol = ObjectCreator.CreateConsol("KRSEL", "AUSEL", "C00001");
			var creditor1 = ObjectCreator.ABIGAS;
			creditor1.CompanyData.OB_RX_NKAPDefltCurrency = Constants.CurrencyCodes.KoreaRepublicOf;
			var creditor2 = ObjectCreator.AALSHI;
			creditor2.CompanyData.OB_RX_NKAPDefltCurrency = Constants.CurrencyCodes.UnitedStates;
			var creditor3 = Factory.NewWithValidTestData<OrgHeader>();
			creditor3.CompanyData.OB_RX_NKAPDefltCurrency = Constants.CurrencyCodes.EuropeanUnion;
			var job = CreateJobWithCharge(ObjectCreator.ABIGAS);
			var consolCost = Factory.New<JobConsolCost>();
			using (consolCost.ReportSettingParentSuspender.GetSuspender())
			{
				consolCost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.CostSupporter.PK, consol.TablePrefix);
			}

			consolCost.E6_RX_NKCurrency = Constants.CurrencyCodes.Australia;
			var apCharge = CreateApportionmentCharge(consolCost, job);
			consolCost.E6_OH_Creditor = creditor1.PK;
			AssertEquals("Currency should not default", Constants.CurrencyCodes.Australia, consolCost.E6_RX_NKCurrency);
			AssertEquals("Currency should not default to the shipment (apportionment charge)", Constants.CurrencyCodes.Australia, consolCost.ApportionmentCharges[0].JR_RX_NKCostCurrency);
			consolCost.E6_OH_Creditor = creditor2.PK;
			AssertEquals("Currency should not default", Constants.CurrencyCodes.Australia, consolCost.E6_RX_NKCurrency);
			AssertEquals("Currency should not default to the shipment (apportionment charge)", Constants.CurrencyCodes.Australia, consolCost.ApportionmentCharges[0].JR_RX_NKCostCurrency);
			BusinessObjectCollection collection = new JobConsolCostCollection(Factory, consol);
			collection.Add(consolCost);
			consolCost.IsPosting = true;
			AssertEquals(typeof(ConsolCostCalculationStrategyWithoutCalculations), consolCost.CalculationStrategy.GetType());
			consolCost.E6_OH_Creditor = creditor1.PK;
			AssertEquals("Currency should not default", Constants.CurrencyCodes.Australia, consolCost.E6_RX_NKCurrency);
			AssertEquals("Currency should not default to the shipment (apportionment charge)", Constants.CurrencyCodes.Australia, consolCost.ApportionmentCharges[0].JR_RX_NKCostCurrency);
			consolCost.IsPosting = false;
			AssertEquals(typeof(ConsolCostCalculationStrategy), consolCost.CalculationStrategy.GetType());
			consolCost.E6_OH_Creditor = creditor2.PK;
			AssertEquals("Currency should default", Constants.CurrencyCodes.UnitedStates, consolCost.E6_RX_NKCurrency);
			AssertEquals("Currency should default to the shipment (apportionment charge)", Constants.CurrencyCodes.UnitedStates, consolCost.ApportionmentCharges[0].JR_RX_NKCostCurrency);
			collection.Remove(consolCost);
			collection = new APInvoiceConsolCostCollection(Factory, Factory.New<APInvoice>());
			collection.Add(consolCost);
			AssertEquals(typeof(InvoicingBaseConsolCostCalculationStrategy), consolCost.CalculationStrategy.GetType());
			consolCost.E6_OH_Creditor = creditor3.PK;
			AssertEquals("Currency should not default", Constants.CurrencyCodes.UnitedStates, consolCost.E6_RX_NKCurrency);
			AssertEquals("Currency should not default to the shipment (apportionment charge)", Constants.CurrencyCodes.UnitedStates, consolCost.ApportionmentCharges[0].JR_RX_NKCostCurrency);
			collection.Remove(consolCost);
			collection = new InvoicingBaseConsolCostCollectionForImporting(Factory);
			var invoicingBaseConsolCostForImporting = Factory.Load<InvoicingBaseConsolCostForImporting>(consolCost.PK);
			collection.Add(invoicingBaseConsolCostForImporting);
			AssertEquals(typeof(ConsolCostCalculationStrategyWithoutCalculations), invoicingBaseConsolCostForImporting.CalculationStrategy.GetType());
			invoicingBaseConsolCostForImporting.E6_OH_Creditor = creditor2.PK;
			AssertEquals("Currency should not default", Constants.CurrencyCodes.UnitedStates, invoicingBaseConsolCostForImporting.E6_RX_NKCurrency);
			AssertEquals("Currency should not default to the shipment (apportionment charge)", Constants.CurrencyCodes.UnitedStates, invoicingBaseConsolCostForImporting.ApportionmentCharges[0].JR_RX_NKCostCurrency);
			consolCost.SetContext(ConsolCostStrategy.ConsolCostCalculationStrategyWithoutCalculations);
			AssertEquals(typeof(ConsolCostCalculationStrategyWithoutCalculations), consolCost.CalculationStrategy.GetType());
			consolCost.E6_OH_Creditor = creditor1.PK;
			AssertEquals("Currency should not default", Constants.CurrencyCodes.UnitedStates, consolCost.E6_RX_NKCurrency);
			AssertEquals("Currency should not default to the shipment (apportionment charge)", Constants.CurrencyCodes.UnitedStates, consolCost.ApportionmentCharges[0].JR_RX_NKCostCurrency);
		}

		public void TestIsCostGSTIsApplicableWhenCreditorIsNotAnOrgProxy()
		{
			var consolCost = Factory.New<JobConsolCost>();
			consolCost.E6_GC = GlbCompany.CurrentCompany.PK;
			var newOrg = Factory.New<OrgHeader>();
			newOrg.CompanyData.OB_IsCreditor = true;
			newOrg.CompanyData.SetAPTaxApplicable(true);
			consolCost.E6_OH_Creditor = newOrg.PK;
			AssertEquals(true, consolCost.IsCostGSTApplicable);
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			{
				AssertEquals(true, consolCost.IsCostGSTApplicable);
			}
		}

		public void TestIsCostGSTIsNotApplicableWhenCreditorIsAnOrgProxy()
		{
			var consolCost = Factory.New<JobConsolCost>();
			consolCost.E6_GC = GlbCompany.CurrentCompany.PK;
			var orgProxy = Factory.Load<OrgHeader>(GlbBranch.CurrentBranch.GB_OH_OrgProxy);
			orgProxy.CompanyData.OB_IsCreditor = true;
			orgProxy.CompanyData.SetAPTaxApplicable(true);
			consolCost.E6_OH_Creditor = orgProxy.PK;
			AssertEquals(true, consolCost.IsCostGSTApplicable);
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			{
				AssertEquals(false, consolCost.IsCostGSTApplicable);
			}
		}

		public void TestChargeCodeDescription()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("AUSYD", "KRSEL", "C00001001");
			var consolCost = creator.CreateConsolCost(consol, creator.CC1);
			AssertEquals(creator.CC1.AC_Desc, consolCost.ChargeCodeDescription);
		}

		public void TestChargeGroupAndSubGroup()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("AUSYD", "KRSEL", "C00001001");
			var consolCost = creator.CreateConsolCost(consol, creator.CC1);

			AssertEquals(creator.CC1.AC_ChargeGroup, consolCost.ChargeGroup);
			AssertEquals(creator.CC1.AC_ChargeSubGroup, consolCost.ChargeCodeSubGroup);

			consolCost.E6_AC_ChargeCode = creator.CC2.PK;
			AssertEquals(creator.CC2.AC_ChargeGroup, consolCost.ChargeGroup);
			AssertEquals(creator.CC2.AC_ChargeSubGroup, consolCost.ChargeCodeSubGroup);
		}

		public void TestInvoiceAmountsAndCurrencies()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("AUSYD", "KRSEL", "C00001001");
			var shipment = consol.Shipments.AddNew();
			var job = creator.CreateJob(shipment, false);
			var listing = new ApportionmentListing(Factory, consol);
			var consolCost1 = listing.CostsCollection.TryAddNew();
			consolCost1.E6_RX_NKCurrency = Constants.CurrencyCodes.KoreaRepublicOf;
			consolCost1.E6_OH_Creditor = creator.AALSHI.PK;
			consolCost1.E6_IsTaxAmountOverridden = true;
			consolCost1.E6_OSCostAmount = 1000m;
			consolCost1.E6_OSGSTAmount_Calc = 100m;
			consolCost1.E6_ExchangeRate = 1.7m;
			consolCost1.E6_InvoiceNum = "APInv1";
			var consolCost2 = listing.CostsCollection.TryAddNew();
			consolCost2.E6_RX_NKCurrency = Constants.CurrencyCodes.UnitedStates;
			consolCost2.E6_OH_Creditor = creator.AALSHI.PK;
			consolCost2.E6_IsTaxAmountOverridden = true;
			consolCost2.E6_OSCostAmount = 2000m;
			consolCost2.E6_OSGSTAmount_Calc = 200m;
			consolCost2.E6_ExchangeRate = 1.6m;
			consolCost2.E6_InvoiceNum = "APInv1";
			var consolCost3 = listing.CostsCollection.TryAddNew();
			consolCost3.E6_RX_NKCurrency = Constants.CurrencyCodes.Greece;
			consolCost3.E6_OH_Creditor = creator.AALSHI.PK;
			consolCost3.E6_IsTaxAmountOverridden = true;
			consolCost3.E6_OSCostAmount = 3000m;
			consolCost3.E6_OSGSTAmount_Calc = 300m;
			consolCost3.E6_ExchangeRate = 1.5m;
			consolCost3.E6_InvoiceNum = "APInv2";
			var consolCost4 = listing.CostsCollection.TryAddNew();
			consolCost4.E6_IsTaxAmountOverridden = true;
			consolCost4.E6_OSCostAmount = 4000m;
			consolCost4.E6_OSGSTAmount_Calc = 400m;
			consolCost4.E6_InvoiceNum = "";
			var consolCost5 = listing.CostsCollection.TryAddNew();
			consolCost5.E6_IsTaxAmountOverridden = true;
			consolCost5.E6_OSCostAmount = 5000m;
			consolCost5.E6_OSGSTAmount_Calc = 500m;
			consolCost5.E6_InvoiceNum = "";
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, consolCost1.InvoiceOSCurrency);
			AssertEquals(2022.06m, consolCost1.InvoiceOSTotal);
			AssertEquals(183.82m, consolCost1.InvoiceOSTax);
			AssertEquals(2022.06m, consolCost1.InvoiceLocalTotal);
			AssertEquals(183.82m, consolCost1.InvoiceLocalTax);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, consolCost2.InvoiceOSCurrency);
			AssertEquals(2022.06m, consolCost2.InvoiceOSTotal);
			AssertEquals(183.82m, consolCost2.InvoiceOSTax);
			AssertEquals(2022.06m, consolCost2.InvoiceLocalTotal);
			AssertEquals(183.82m, consolCost2.InvoiceLocalTax);
			AssertEquals(Constants.CurrencyCodes.Greece, consolCost3.InvoiceOSCurrency);
			AssertEquals(3300m, consolCost3.InvoiceOSTotal);
			AssertEquals(300m, consolCost3.InvoiceOSTax);
			AssertEquals(2200m, consolCost3.InvoiceLocalTotal);
			AssertEquals(200m, consolCost3.InvoiceLocalTax);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, consolCost4.InvoiceOSCurrency);
			AssertEquals(4400m, consolCost4.InvoiceOSTotal);
			AssertEquals(400m, consolCost4.InvoiceOSTax);
			AssertEquals(4400m, consolCost4.InvoiceLocalTotal);
			AssertEquals(400m, consolCost4.InvoiceLocalTax);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, consolCost5.InvoiceOSCurrency);
			AssertEquals(5500m, consolCost5.InvoiceOSTotal);
			AssertEquals(500m, consolCost5.InvoiceOSTax);
			AssertEquals(5500m, consolCost5.InvoiceLocalTotal);
			AssertEquals(500m, consolCost5.InvoiceLocalTax);
		}

		#region Test Use Invoice Amounts From Posted Invoice

		public void TestInvoiceAmountsFromPostedInvoice_AmountsAreSameFromConsolCostingAndPayableTransactionModules()
		{
			var company = TestObjectCreator.CreateCompanyAndBranch("TWTPE");
			company.SetCountry(Core.Constants.CountryCodes.Taiwan);
			company.GC_IsReciprocal = true;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			company.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Taiwan;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, company.FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var consol1 = TestObjectCreator.CreateConsol("TWTPE", "SGSIN", "C00001001");
				var shipment1 = TestObjectCreator.CreateShipment("S001", consol1);
				var shipment2 = TestObjectCreator.CreateShipment("S002", consol1);
				shipment1.JS_ActualVolume = 1m;
				shipment2.JS_ActualVolume = 12.65m;
				var job1 = TestObjectCreator.CreateJob(shipment1, createWithMutex: false);
				var job2 = TestObjectCreator.CreateJob(shipment2, createWithMutex: false);

				var listing = new ApportionmentListing(Factory, consol1);
				var consolCost1 = CreateConsolCost(listing);
				var taxRate = TestObjectCreator.CreateTaxRate("VAT", "VAT", AccTaxRate.Types.Rated, 5, string.Empty, 0, 1);

				consolCost1.E6_ApportionmentMethod = AllocationMethod.GrossVolume;
				consolCost1.E6_RX_NKCurrency = "USD";
				consolCost1.E6_ExchangeRate = 32.15m;
				consolCost1.E6_AT_TaxRate = taxRate.PK;
				consolCost1.E6_OSCostAmount = 189.75m;

				Factory.Save();

				var postManager = new ConsolInvoicingPostManager(Factory, new[] { job1, job2 }, consol1, listing);
				postManager.CreateTransactions(JobInvoicingPostingOption.ConsolCosts);
				Factory.Save();

				var consol2 = TestObjectCreator.CreateConsol("TWTPE", "SGSIN", "C00001002");
				var shipment3 = TestObjectCreator.CreateShipment("S003", consol2);
				var shipment4 = TestObjectCreator.CreateShipment("S004", consol2);
				shipment3.JS_ActualVolume = 1m;
				shipment4.JS_ActualVolume = 12.65m;
				var job3 = TestObjectCreator.CreateJob(shipment3, createWithMutex: false);
				var job4 = TestObjectCreator.CreateJob(shipment4, createWithMutex: false);
				Factory.Save();

				var invoice = Factory.NewWithValidTestData<APInvoice>();
				invoice.AH_OH = TestObjectCreator.AALSHI.PK;

				var consolCost2 = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol2);

				SetUpConsolCost(consolCost2, TestObjectCreator.CC1.PK, TestObjectCreator.AALSHI.PK, taxRate.PK);

				invoice.ConsolCosting.ConsolCosts.Add(consolCost2);
				invoice.ImportAllApportionmentsFromCosting();
				invoice.SubmittedFromInvoicingForm = true;

				Factory.Save();

				CombineAssertions(
					"Amounts should be same when posting from Consol Costing and Payable Transaction Modules.",
					() =>
					{
						AssertEquals(199.24m, consolCost1.InvoiceOSTotal);
						AssertEquals(9.49m, consolCost1.InvoiceOSTax);
						AssertEquals(6405m, consolCost1.InvoiceLocalTotal);
						AssertEquals(305m, consolCost1.InvoiceLocalTax);
						AssertEquals(199.24m, consolCost2.InvoiceOSTotal);
						AssertEquals(9.49m, consolCost2.InvoiceOSTax);
						AssertEquals(6405m, consolCost2.InvoiceLocalTotal);
						AssertEquals(305m, consolCost2.InvoiceLocalTax);
					}
				);
			}
		}

		public void TestInvoiceAmountsFromPostedInvoice_InvoiceLocalTotalAndInvoiceOSTotal()
		{
			var company = TestObjectCreator.CreateCompanyAndBranch("TWTPE");
			company.SetCountry(Core.Constants.CountryCodes.Taiwan);
			company.GC_IsReciprocal = true;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			company.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Taiwan;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, company.FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var consol = TestObjectCreator.CreateConsol("TWTPE", "SGSIN", "C00001001");
				var shipment1 = TestObjectCreator.CreateShipment("S001", consol);
				var shipment2 = TestObjectCreator.CreateShipment("S002", consol);
				shipment1.JS_ActualVolume = 1m;
				shipment2.JS_ActualVolume = 12.65m;
				var job1 = TestObjectCreator.CreateJob(shipment1, createWithMutex: false);
				var job2 = TestObjectCreator.CreateJob(shipment2, createWithMutex: false);

				var listing = new ApportionmentListing(Factory, consol);
				var consolCost = CreateConsolCost(listing);
				var taxRate = TestObjectCreator.CreateTaxRate("VAT", "VAT", AccTaxRate.Types.Rated, 5, string.Empty, 0, 1);

				consolCost.E6_ApportionmentMethod = AllocationMethod.GrossVolume;
				consolCost.E6_RX_NKCurrency = "USD";
				consolCost.E6_ExchangeRate = 32.15m;
				consolCost.E6_AT_TaxRate = taxRate.PK;
				consolCost.E6_OSCostAmount = 189.75m;

				Factory.Save();

				var postManager = new ConsolInvoicingPostManager(Factory, new[] { job1, job2 }, consol, listing);
				postManager.CreateTransactions(JobInvoicingPostingOption.ConsolCosts);
				Factory.Save();

				CombineAssertions(
					"Should be amount sumed up from posted invoice, not from ConsolCosts with same invoice.",
					() =>
					{
						AssertEquals(6405m, consolCost.InvoiceLocalTotal);
						AssertEquals(305m, consolCost.InvoiceLocalTax);
					}
				);

				listing.ReleaseMutexes();
			}
		}

		public void TestInvoiceAmountsFromPostedInvoice_InvoiceOSTotalAndInvoiceOSTax()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment1 = TestObjectCreator.CreateShipment("S001", consol);
			var shipment2 = TestObjectCreator.CreateShipment("S002", consol);
			shipment1.JS_ActualVolume = 1m;
			shipment2.JS_ActualVolume = 12.65m;
			var job1 = TestObjectCreator.CreateJob(shipment1, createWithMutex: false);
			var job2 = TestObjectCreator.CreateJob(shipment2, createWithMutex: false);

			var listing = new ApportionmentListing(Factory, consol);
			var consolCost = CreateConsolCost(listing);
			var taxRate = TestObjectCreator.CreateTaxRate("VAT", "VAT", AccTaxRate.Types.Rated, 5, string.Empty, 0, 1);

			consolCost.E6_ApportionmentMethod = AllocationMethod.GrossVolume;
			consolCost.E6_RX_NKCurrency = "TWD";
			consolCost.E6_ExchangeRate = 32.15m;
			consolCost.E6_AT_TaxRate = taxRate.PK;
			consolCost.E6_OSCostAmount = 6100m;

			Factory.Save();

			var postManager = new ConsolInvoicingPostManager(Factory, new[] { job1, job2 }, consol, listing);
			postManager.CreateTransactions(JobInvoicingPostingOption.ConsolCosts);
			Factory.Save();

			CombineAssertions(
				"Should be amount sumed up from posted invoice, not from ConsolCosts with same invoice.",
				() =>
				{
					AssertEquals(6405m, consolCost.InvoiceOSTotal);
					AssertEquals(305m, consolCost.InvoiceOSTax);
				}
			);

			listing.ReleaseMutexes();
		}

		public void TestInvoiceAmountsFromPostedInvoice_MultiConsolCostsInDifferentConsol()
		{
			var company = TestObjectCreator.CreateCompanyAndBranch("TWTPE");
			company.SetCountry(Core.Constants.CountryCodes.Taiwan);
			company.GC_IsReciprocal = true;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			company.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Taiwan;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, company.FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var consol1 = TestObjectCreator.CreateConsol("TWTPE", "SGSIN", "C00001001");
				var consol2 = TestObjectCreator.CreateConsol("TWTPE", "SGSIN", "C00001002");
				var shipment1 = TestObjectCreator.CreateShipment("S001", consol1);
				var shipment2 = TestObjectCreator.CreateShipment("S002", consol1);
				var shipment3 = TestObjectCreator.CreateShipment("S003", consol2);
				var shipment4 = TestObjectCreator.CreateShipment("S004", consol2);
				shipment1.JS_ActualVolume = 1m;
				shipment2.JS_ActualVolume = 12.65m;
				shipment3.JS_ActualVolume = 1m;
				shipment4.JS_ActualVolume = 12.65m;
				var job1 = TestObjectCreator.CreateJob(shipment1, createWithMutex: false);
				var job2 = TestObjectCreator.CreateJob(shipment2, createWithMutex: false);
				var job3 = TestObjectCreator.CreateJob(shipment3, createWithMutex: false);
				var job4 = TestObjectCreator.CreateJob(shipment4, createWithMutex: false);
				Factory.Save();

				var invoice = Factory.NewWithValidTestData<APInvoice>();
				invoice.AH_OH = TestObjectCreator.AALSHI.PK;

				var consolCost1 = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol1);
				var consolCost2 = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol2);
				var taxRate = TestObjectCreator.CreateTaxRate("VAT", "VAT", AccTaxRate.Types.Rated, 5, string.Empty, 0, 1);

				SetUpConsolCost(consolCost1, TestObjectCreator.CC1.PK, TestObjectCreator.AALSHI.PK, taxRate.PK);
				SetUpConsolCost(consolCost2, TestObjectCreator.CC2.PK, TestObjectCreator.AALSHI.PK, taxRate.PK);

				invoice.ConsolCosting.ConsolCosts.Add(consolCost1);
				invoice.ConsolCosting.ConsolCosts.Add(consolCost2);
				invoice.ImportAllApportionmentsFromCosting();
				invoice.SubmittedFromInvoicingForm = true;

				Factory.Save();

				CombineAssertions(
					"Should be amount sumed up from posted invoice, not from ConsolCosts with same invoice.",
					() =>
					{
						AssertEquals(199.24m, consolCost1.InvoiceOSTotal);
						AssertEquals(9.49m, consolCost1.InvoiceOSTax);
						AssertEquals(6405m, consolCost1.InvoiceLocalTotal);
						AssertEquals(305m, consolCost1.InvoiceLocalTax);
						AssertEquals(199.24m, consolCost2.InvoiceOSTotal);
						AssertEquals(9.49m, consolCost2.InvoiceOSTax);
						AssertEquals(6405m, consolCost2.InvoiceLocalTotal);
						AssertEquals(305m, consolCost2.InvoiceLocalTax);
					}
				);
			}
		}

		public void TestInvoiceAmountsFromPostedInvoice_MultiConsolCostsInSameConsol()
		{
			var company = TestObjectCreator.CreateCompanyAndBranch("TWTPE");
			company.SetCountry(Core.Constants.CountryCodes.Taiwan);
			company.GC_IsReciprocal = true;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			company.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Taiwan;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, company.FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var consol = TestObjectCreator.CreateConsol("TWTPE", "SGSIN", "C00001001");
				var shipment1 = TestObjectCreator.CreateShipment("S001", consol);
				var shipment2 = TestObjectCreator.CreateShipment("S002", consol);
				shipment1.JS_ActualVolume = 1m;
				shipment2.JS_ActualVolume = 12.65m;
				var job1 = TestObjectCreator.CreateJob(shipment1, createWithMutex: false);
				var job2 = TestObjectCreator.CreateJob(shipment2, createWithMutex: false);

				var listing = new ApportionmentListing(Factory, consol);
				var consolCost1 = CreateConsolCost(listing);
				var consolCost2 = CreateConsolCost(listing);
				var taxRate = TestObjectCreator.CreateTaxRate("VAT", "VAT", AccTaxRate.Types.Rated, 5, string.Empty, 0, 1);

				SetUpConsolCost(consolCost1, TestObjectCreator.CC1.PK, TestObjectCreator.AALSHI.PK, taxRate.PK);
				SetUpConsolCost(consolCost2, TestObjectCreator.CC2.PK, TestObjectCreator.ABIGAS.PK, taxRate.PK);

				Factory.Save();

				var postManager = new ConsolInvoicingPostManager(Factory, new[] { job1, job2 }, consol, listing);
				postManager.CreateTransactions(JobInvoicingPostingOption.ConsolCosts);
				Factory.Save();

				CombineAssertions(
					"Should be amount sumed up from posted invoice, not from ConsolCosts with same invoice.",
					() =>
					{
						AssertEquals(199.24m, consolCost1.InvoiceOSTotal);
						AssertEquals(9.49m, consolCost1.InvoiceOSTax);
						AssertEquals(6405m, consolCost1.InvoiceLocalTotal);
						AssertEquals(305m, consolCost1.InvoiceLocalTax);
						AssertEquals(199.24m, consolCost2.InvoiceOSTotal);
						AssertEquals(9.49m, consolCost2.InvoiceOSTax);
						AssertEquals(6405m, consolCost2.InvoiceLocalTotal);
						AssertEquals(305m, consolCost2.InvoiceLocalTax);
					}
				);

				listing.ReleaseMutexes();
			}
		}

		public void TestInvoiceAmountsFromPostedInvoice_MultiConsolCostsInSameConsol_WithMixedCurrency()
		{
			var company = TestObjectCreator.CreateCompanyAndBranch("TWTPE");
			company.SetCountry(Core.Constants.CountryCodes.Taiwan);
			company.GC_IsReciprocal = true;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			company.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Taiwan;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, company.FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var consol = TestObjectCreator.CreateConsol("TWTPE", "SGSIN", "C00001001");
				var shipment1 = TestObjectCreator.CreateShipment("S001", consol);
				var shipment2 = TestObjectCreator.CreateShipment("S002", consol);
				shipment1.JS_ActualVolume = 1m;
				shipment2.JS_ActualVolume = 12.65m;
				var job1 = TestObjectCreator.CreateJob(shipment1, createWithMutex: false);
				var job2 = TestObjectCreator.CreateJob(shipment2, createWithMutex: false);

				var listing = new ApportionmentListing(Factory, consol);
				var consolCost1 = CreateConsolCost(listing);
				var consolCost2 = CreateConsolCost(listing);
				var taxRate = TestObjectCreator.CreateTaxRate("VAT", "VAT", AccTaxRate.Types.Rated, 5, string.Empty, 0, 1);

				SetUpConsolCost(consolCost1, TestObjectCreator.CC1.PK, TestObjectCreator.AALSHI.PK, taxRate.PK);
				SetUpConsolCost(consolCost2, TestObjectCreator.CC2.PK, TestObjectCreator.AALSHI.PK, taxRate.PK);
				consolCost2.E6_RX_NKCurrency = "TWD";
				consolCost2.E6_ExchangeRate = 1m;
				consolCost2.E6_OSCostAmount = 4048m;

				Factory.Save();

				var postManager = new ConsolInvoicingPostManager(Factory, new[] { job1, job2 }, consol, listing);
				postManager.CreateTransactions(JobInvoicingPostingOption.ConsolCosts);
				Factory.Save();

				CombineAssertions(
					"Should be amount sumed up from posted invoice, not from ConsolCosts with same invoice.",
					() =>
					{
						AssertEquals(10655m, consolCost1.InvoiceOSTotal);
						AssertEquals(507m, consolCost1.InvoiceOSTax);
						AssertEquals(10655m, consolCost1.InvoiceLocalTotal);
						AssertEquals(507m, consolCost1.InvoiceLocalTax);
						AssertEquals(10655m, consolCost2.InvoiceOSTotal);
						AssertEquals(507m, consolCost2.InvoiceOSTax);
						AssertEquals(10655m, consolCost2.InvoiceLocalTotal);
						AssertEquals(507m, consolCost2.InvoiceLocalTax);
					}
				);

				listing.ReleaseMutexes();
			}
		}

		public void TestInvoiceAmountsFromPostedInvoice_MultiConsolCostsInSameConsol_WithMixedCurrency_PostedFromPayableTransacionModule()
		{
			var company = TestObjectCreator.CreateCompanyAndBranch("TWTPE");
			company.SetCountry(Core.Constants.CountryCodes.Taiwan);
			company.GC_IsReciprocal = true;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			company.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Taiwan;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, company.FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var consol = TestObjectCreator.CreateConsol("TWTPE", "SGSIN", "C00001001");
				var shipment1 = TestObjectCreator.CreateShipment("S001", consol);
				var shipment2 = TestObjectCreator.CreateShipment("S002", consol);
				shipment1.JS_ActualVolume = 1m;
				shipment2.JS_ActualVolume = 12.65m;
				var job1 = TestObjectCreator.CreateJob(shipment1, createWithMutex: false);
				var job2 = TestObjectCreator.CreateJob(shipment2, createWithMutex: false);
				Factory.Save();

				var invoice = Factory.NewWithValidTestData<APInvoice>();
				invoice.AH_OH = TestObjectCreator.AALSHI.PK;

				var consolCost1 = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				var consolCost2 = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				var taxRate = TestObjectCreator.CreateTaxRate("VAT", "VAT", AccTaxRate.Types.Rated, 5, string.Empty, 0, 1);

				SetUpConsolCost(consolCost1, TestObjectCreator.CC1.PK, TestObjectCreator.AALSHI.PK, taxRate.PK);
				SetUpConsolCost(consolCost2, TestObjectCreator.CC2.PK, TestObjectCreator.AALSHI.PK, taxRate.PK);
				consolCost2.E6_RX_NKCurrency = "TWD";
				consolCost2.E6_ExchangeRate = 1m;
				consolCost2.E6_OSCostAmount = 4048m;

				invoice.ConsolCosting.ConsolCosts.Add(consolCost1);
				invoice.ConsolCosting.ConsolCosts.Add(consolCost2);
				invoice.ImportAllApportionmentsFromCosting();
				invoice.SubmittedFromInvoicingForm = true;

				Factory.Save();

				var consolCostCollection = new JobConsolCostCollection(Factory, consol);
				consolCostCollection.Add(consolCost1);
				consolCostCollection.Add(consolCost2);
				((IBusinessObjectInternals)consolCost1).ParentCollections.Append(consolCostCollection);
				((IBusinessObjectInternals)consolCost2).ParentCollections.Append(consolCostCollection);

				CombineAssertions(
					"Should be amount sumed up from posted invoice, not from ConsolCosts with same invoice.",
					() =>
					{
						AssertEquals(10655m, consolCost1.InvoiceOSTotal);
						AssertEquals(507m, consolCost1.InvoiceOSTax);
						AssertEquals(10655m, consolCost1.InvoiceLocalTotal);
						AssertEquals(507m, consolCost1.InvoiceLocalTax);
						AssertEquals(10655m, consolCost2.InvoiceOSTotal);
						AssertEquals(507m, consolCost2.InvoiceOSTax);
						AssertEquals(10655m, consolCost2.InvoiceLocalTotal);
						AssertEquals(507m, consolCost2.InvoiceLocalTax);
					}
				);
			}
		}

		void SetUpConsolCost(JobConsolCost consolCost, ZGuid chargePK, ZGuid creditorPK, ZGuid taxRatePK)
		{
			consolCost.E6_AC_ChargeCode = chargePK;
			consolCost.E6_OH_Creditor = creditorPK;
			consolCost.E6_ApportionmentMethod = AllocationMethod.GrossVolume;
			consolCost.E6_RX_NKCurrency = "USD";
			consolCost.E6_ExchangeRate = 32.15m;
			consolCost.E6_AT_TaxRate = taxRatePK;
			consolCost.E6_OSCostAmount = 189.75m;
		}

		#endregion

		public void TestApportionmentConcurrency()
		{
			BusinessObjectFactory factoryInSession1 = new BusinessObjectFactory();
			BusinessObjectFactory factoryInSession2 = new BusinessObjectFactory();
			factoryInSession1.RefreshEnabled = false;
			factoryInSession2.RefreshEnabled = false;
			ForwardingConsol consol = factoryInSession2.New<ForwardingConsol>();
			factoryInSession2.Save();
			consol.Shipments.AddNew();
			ApportionmentListing listing = new ApportionmentListing(factoryInSession2, consol);
			JobConsolCost consolCost = listing.CostsCollection.TryAddNew();
			var query = new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(AccChargeCodeSchema.AC_AG_AccrualAccount, SQLComparisonOperator.NotEqual, null);
			consolCost.E6_AC_ChargeCode = factoryInSession2.LoadTop1<AccChargeCode>(query).PK;
			consolCost.E6_OSCostAmount = 100m;
			Assert("This should create an apportionment charge", consolCost.ApportionmentCharges.Count == 1);
			factoryInSession2.Save();
			Charge chargeInFactory2 = factoryInSession2.LoadTop1<Charge>(new ZQuery(JobChargeSchema.PK, consolCost.ApportionmentCharges[0].PK));
			Charge chargeInFactory1 = factoryInSession1.Load<Charge>(chargeInFactory2.PK);
			AccTransactionLines revenueLine = factoryInSession1.New<AccTransactionLines>();
			revenueLine.CopyPersistentValuesFrom(chargeInFactory1.APLine);
			revenueLine.AL_LineType = "REV";
			revenueLine.AL_LineAmount = -revenueLine.AL_LineAmount;
			chargeInFactory1.ReverseWIP(ZDateTime.Today);
			chargeInFactory1.JR_AL_ARLine = revenueLine.PK;
			chargeInFactory1.SetAmountsFromLinkedLinesForTests();
			chargeInFactory1.ARLine.AL_OSAmount = chargeInFactory1.ARLine.AL_LineAmount = chargeInFactory1.JR_OSSellAmt;
			ARInvoice invoice = factoryInSession1.NewWithValidTestData<ARInvoice>();
			revenueLine.AL_AH = invoice.PK;
			factoryInSession1.Save();
			consolCost.Delete();
			var exception = AssertExceptionThrown<ZSaveConcurrencyException>(() => BusinessObjectFactory.SaveTogether(factoryInSession2));
			var errorMessage = $@"
**CONCURRENCY Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Tablename: JobCharge
PK: {chargeInFactory2.PK}
RowState: Deleted
";
			Assert(string.Format(@"Message
{0}
should be part of error message
{1}", errorMessage, exception.Message), exception.Message.Contains(errorMessage));
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			chargeInFactory1 = newFactory.LoadTop1<Charge>(new ZQuery(JobChargeSchema.PK, chargeInFactory1.PK));
			AssertNotNull("JobCharge Should still exist in session 1", chargeInFactory1);
		}

		public void TestApportionmentReloadingOnDeleting()
		{
			BusinessObjectFactory factoryInSession2 = new BusinessObjectFactory();
			factoryInSession2.RefreshEnabled = false;
			ForwardingConsol consol = factoryInSession2.New<ForwardingConsol>();
			factoryInSession2.Save();
			consol.Shipments.AddNew();
			ApportionmentListing listing = new ApportionmentListing(factoryInSession2, consol);
			JobConsolCost consolCost = listing.CostsCollection.TryAddNew();
			var query = new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(AccChargeCodeSchema.AC_AG_AccrualAccount, SQLComparisonOperator.NotEqual, null);
			consolCost.E6_AC_ChargeCode = factoryInSession2.LoadTop1<AccChargeCode>(query).PK;
			consolCost.E6_OSCostAmount = 100m;
			Assert("This should create an apportionment charge", consolCost.ApportionmentCharges.Count == 1);
			factoryInSession2.Save();
			AccTransactionLines revenueLine = factoryInSession2.New<AccTransactionLines>();
			Charge chargeInFactory2 = factoryInSession2.LoadTop1<Charge>(new ZQuery(JobChargeSchema.PK, consolCost.ApportionmentCharges[0].PK));
			revenueLine.CopyPersistentValuesFrom(chargeInFactory2.APLine);
			revenueLine.AL_LineType = "REV";
			revenueLine.AL_LineAmount = -revenueLine.AL_LineAmount;
			chargeInFactory2.ReverseWIP(ZDateTime.Today);
			chargeInFactory2.JR_AL_ARLine = revenueLine.PK;
			chargeInFactory2.SetAmountsFromLinkedLinesForTests();
			chargeInFactory2.ARLine.AL_OSAmount = chargeInFactory2.ARLine.AL_LineAmount = chargeInFactory2.JR_OSSellAmt;
			ARInvoice invoice = factoryInSession2.NewWithValidTestData<ARInvoice>();
			revenueLine.AL_AH = invoice.PK;
			factoryInSession2.Save();
			BusinessObjectFactory factoryInSession1 = new BusinessObjectFactory();
			factoryInSession1.RefreshEnabled = false;
			Charge chargeInFactory1 = factoryInSession1.Load<Charge>(chargeInFactory2.PK);
			chargeInFactory1.JR_Desc = "Changed desc";
			factoryInSession1.Save();
			consolCost.Delete();
			Assert("Charge should not be deleted", !chargeInFactory2.IsDeleted);
			AssertNotEquals("Charge should not be reloaded on consol cost deleting", chargeInFactory1.JR_Desc, chargeInFactory2.JR_Desc);
		}

		public void TestSetE6_AT_TaxRateAndE6_TaxDateReadonly()
		{
			var securityRight = Env.Security.MaintainConsolJobInvoicingAllowOverrideCostTaxId;
			var registry = AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId;
			var securityValueCache = securityRight.IsAllowed;

			try
			{
				var creator = new TestObjectCreator(Factory);
				var consol = Factory.New<ForwardingConsol>();
				var cost = consol.GetApportionments().CostsCollection.TryAddNew();

				ObjectCreator.ABIGAS.CompanyData.SetAPTaxApplicable(true);
				cost.E6_GC = GlbCompany.CurrentCompany.PK;
				cost.E6_AC_ChargeCode = creator.CC1.PK;
				cost.E6_RX_NKCurrency = creator.AUD.RX_Code;
				cost.E6_OH_Creditor = ObjectCreator.ABIGAS.PK;
				cost.E6_AT_TaxRate = creator.GST1.PK;

				AssertEquals("Pre-condition", true, cost.IsCostGSTApplicable);
				AssertEquals(false, cost.IsGatewayConsolCost);
				AssertEquals(false, cost.IsPosted);

				using (registry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					securityRight.IsAllowed = true;
					AssertEquals("Allow override only when both security and registry are configured.", false, cost.E6_AT_TaxRateInfo.ReadOnly);
					AssertEquals("Allow override only when both security and registry are configured.", false, cost.E6_TaxDateInfo.ReadOnly);

					securityRight.IsAllowed = false;
					AssertEquals("Not allow override", true, cost.E6_AT_TaxRateInfo.ReadOnly);
					AssertEquals("Not allow override", true, cost.E6_TaxDateInfo.ReadOnly);
				}

				using (registry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					securityRight.IsAllowed = true;
					AssertEquals("Not allow override", true, cost.E6_AT_TaxRateInfo.ReadOnly);
					AssertEquals("Not allow override", true, cost.E6_TaxDateInfo.ReadOnly);

					securityRight.IsAllowed = false;
					AssertEquals("Not allow override", true, cost.E6_AT_TaxRateInfo.ReadOnly);
					AssertEquals("Not allow override", true, cost.E6_TaxDateInfo.ReadOnly);
				}
			}
			finally
			{
				securityRight.IsAllowed = securityValueCache;
			}
		}

		public void TestSetE6_A9_VatClassReadonly()
		{
			var securityRight = Env.Security.MaintainConsolJobInvoicingAllowOverrideCostTaxMsg;
			var registry = AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyTaxMessage;
			var securityValueCache = securityRight.IsAllowed;

			try
			{
				var creator = new TestObjectCreator(Factory);
				var consol = Factory.New<ForwardingConsol>();
				var cost = consol.GetApportionments().CostsCollection.TryAddNew();

				ObjectCreator.ABIGAS.CompanyData.SetAPTaxApplicable(true);
				cost.E6_GC = GlbCompany.CurrentCompany.PK;
				cost.E6_AC_ChargeCode = creator.CC1.PK;
				cost.E6_RX_NKCurrency = creator.AUD.RX_Code;
				cost.E6_OH_Creditor = ObjectCreator.ABIGAS.PK;
				cost.E6_AT_TaxRate = creator.GST1.PK;

				AssertEquals("Pre-condition", false, cost.E6_AT_TaxRate.IsEmpty);
				AssertEquals(false, cost.IsGatewayConsolCost);

				using (registry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					securityRight.IsAllowed = true;
					AssertEquals("Allow override only when both security and registry are configured.", false, cost.E6_A9_VATClassInfo.ReadOnly);

					securityRight.IsAllowed = false;
					AssertEquals("Not allow override", true, cost.E6_A9_VATClassInfo.ReadOnly);
				}

				using (registry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					securityRight.IsAllowed = true;
					AssertEquals("Not allow override", true, cost.E6_A9_VATClassInfo.ReadOnly);

					securityRight.IsAllowed = false;
					AssertEquals("Not allow override", true, cost.E6_A9_VATClassInfo.ReadOnly);
				}
			}
			finally
			{
				securityRight.IsAllowed = securityValueCache;
			}
		}

		public void TestSetE6_A9_VatClass_Readonly_WhenIsGatewayConsolCost()
		{
			var gatewayAgentPort = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.First().PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(gatewayAgentPort);
			consol.JK_UniqueConsignRef = "C00001111";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			consol.Shipments.Add(shipment);
			using (var job = new Job.Loader(Factory, consol).TryCreateWithoutMutexForTestOnly())
			{
				var chargeCode = ObjectCreator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK);
				var charge = job.Charges.AddNew();
				charge.JR_AC = chargeCode.PK;
				charge.JR_OSSellAmt = 250m;
				charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
				charge.JR_JH_InternalJob = job.PK;
				charge.JR_GE_InternalDept = charge.JR_GE;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				Assert(consol.IsGateway());
				Factory.Save();
				AssertEquals(1, consol.GetApportionments(true).CostsCollection.Count);
				var cost = consol.GetApportionments(true).CostsCollection[0];
				Assert(cost.IsGatewayConsolCost);
				Assert(cost.E6_A9_VATClassInfo.ReadOnly);
			}
		}

		public void TestGatewayConsolCostFieldsAlwaysReadOnly()
		{
			var gatewayAgentPort = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(gatewayAgentPort);
			consol.JK_UniqueConsignRef = "C00001111";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			consol.Shipments.Add(shipment);
			using (var job = new Job.Loader(Factory, consol).TryCreateWithoutMutexForTestOnly())
			{
				var charge = job.Charges.AddNew();
				charge.JR_AC = Env.Registry.FreightChargeCode;
				charge.JR_OSSellAmt = 250m;
				charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
				charge.JR_JH_InternalJob = job.PK;
				charge.JR_GE_InternalDept = charge.JR_GE;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				Factory.Save();
				Assert(consol.IsGateway());
				AssertGatewayFieldsAreReadOnly(consol, true);
				consol.JK_OA_SendingForwarderAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				Assert("Consol is still gateway as job exists", consol.IsGateway());
				AssertGatewayFieldsAreReadOnly(consol, true);
			}
		}

		void AssertGatewayFieldsAreReadOnly(ForwardingConsol consol, bool isReadOnly)
		{
			var costCollection = consol.GetApportionments(true).CostsCollection;
			AssertEquals(1, costCollection.Count);
			var costCharge = costCollection[0];
			Assert(costCharge.IsGatewayConsolCost);
			AssertEquals(isReadOnly, costCharge.E6_AC_ChargeCodeInfo.ReadOnly);
			AssertEquals(isReadOnly, costCharge.E6_RX_NKCurrencyInfo.ReadOnly);
			AssertEquals(isReadOnly, costCharge.E6_ExchangeRateInfo.ReadOnly);
			AssertEquals(isReadOnly, costCharge.E6_LocalCostAmountInfo.ReadOnly);
			AssertEquals(isReadOnly, costCharge.E6_OSCostAmountInfo.ReadOnly);
			AssertEquals(isReadOnly, costCharge.E6_AB_BankAccountInfo.ReadOnly);
			AssertEquals(isReadOnly, costCharge.E6_AK_ChequeBookInfo.ReadOnly);
			AssertEquals(isReadOnly, costCharge.E6_InvoiceNumInfo.ReadOnly);
			AssertEquals(isReadOnly, costCharge.E6_A9_VATClassInfo.ReadOnly);
			AssertEquals(isReadOnly, costCharge.E6_AT_TaxRateInfo.ReadOnly);
			AssertEquals(isReadOnly, costCharge.E6_InvoiceDateInfo.ReadOnly);
			AssertEquals(isReadOnly, costCharge.E6_PaymentDateInfo.ReadOnly);
			AssertEquals(isReadOnly, costCharge.E6_ChequeOrReferenceInfo.ReadOnly);
			AssertEquals(isReadOnly, costCharge.E6_OH_CreditorInfo.ReadOnly);
			AssertEquals(isReadOnly, costCharge.E6_PaymentTypeInfo.ReadOnly);
			AssertEquals(isReadOnly, costCharge.E6_OSGSTAmount_CalcInfo.ReadOnly);
			AssertEquals(isReadOnly, costCharge.E6_RatingBehaviourInfo.ReadOnly);
		}

		public void TestSetE6_AC_ChargeCodeWillUpdateE6_A9_VatClass()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			JobConsolCost cost1 = consol.GetApportionments().CostsCollection.TryAddNew();
			cost1.E6_GC = GlbCompany.CurrentCompany.PK;
			cost1.E6_AC_ChargeCode = creator.CC1.PK;
			cost1.E6_RX_NKCurrency = creator.AUD.RX_Code;
			AssertEquals("precondition - E6_A9_VATClass is empty", ZGuid.Empty, cost1.E6_A9_VATClass);
			creator.GST1.AT_A9_DefaultVatClass = creator.TaxMsg1.PK;
			cost1.E6_AT_TaxRate = creator.GST1.PK;
			AssertEquals("postcondition - E6_A9_VATClass should be copied from tax rate", creator.TaxMsg1.PK, cost1.E6_A9_VATClass);
		}

		public void TestSetE6_AC_ChargeCode_Creditor()
		{
			SetGSTAndVATInfoChargeCode(TestObjectCreator, TestObjectCreator.CC1, TestObjectCreator.GST1);

			var consol = TestObjectCreator.CreateConsol(origin: "AUSYD", destination: "USLAX");
			AddShipmentAndJobToConsol(consol);
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.ForwardingConsolCode, direction: "ALL", transportMode: "ALL", paymentTerm: "PPD", creditor: TestObjectCreator.Creditor1.PK);

			Factory.Save();

			var listing = new ApportionmentListing(Factory, consol);
			var consolCost = listing.CostsCollection.TryAddNew();
			consolCost.E6_OSCostAmount = 100m;
			consolCost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			AssertEquals("Creditor", TestObjectCreator.Creditor1.OH_Code, consolCost.Creditor.OH_Code);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		public void TestSetE6_AC_ChargeCode_CreditorRole_Import()
		{
			SetGSTAndVATInfoChargeCode(TestObjectCreator, TestObjectCreator.CC1, TestObjectCreator.GST1);

			var sendingForwarderAddress = Factory.NewWithValidTestData<OrgAddress>();
			sendingForwarderAddress.OA_Code = "SendingForwarderAdr";
			sendingForwarderAddress.OA_OH = TestObjectCreator.Creditor1.PK;

			var consol = TestObjectCreator.CreateConsol(origin: "USLAX", destination: "AUSYD");
			consol.JK_OA_SendingForwarderAddress = sendingForwarderAddress.PK;

			AddShipmentAndJobToConsol(consol);
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.ForwardingConsolCode, direction: "ALL", transportMode: "ALL", paymentTerm: "PPD", creditorRole: DocAddressTypes.Codes.OverseasAgent);

			Factory.Save();

			var listing = new ApportionmentListing(Factory, consol);
			var consolCost = listing.CostsCollection.TryAddNew();
			consolCost.E6_OSCostAmount = 100m;
			consolCost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			AssertEquals("Creditor", TestObjectCreator.Creditor1.OH_Code, consolCost.Creditor.OH_Code);
		}

		public void TestSetE6_AC_ChargeCode_CreditorRole_Export()
		{
			SetGSTAndVATInfoChargeCode(TestObjectCreator, TestObjectCreator.CC1, TestObjectCreator.GST1);

			var receivingForwarderAddress = Factory.NewWithValidTestData<OrgAddress>();
			receivingForwarderAddress.OA_Code = "ReceivingForwarderAdr";
			receivingForwarderAddress.OA_OH = TestObjectCreator.Creditor1.PK;

			var consol = TestObjectCreator.CreateConsol(origin: "AUSYD", destination: "USLAX");
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarderAddress.PK;

			AddShipmentAndJobToConsol(consol);
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.ForwardingConsolCode, direction: "ALL", transportMode: "ALL", paymentTerm: "PPD", creditorRole: DocAddressTypes.Codes.OverseasAgent);

			Factory.Save();

			var listing = new ApportionmentListing(Factory, consol);
			var consolCost = listing.CostsCollection.TryAddNew();
			consolCost.E6_OSCostAmount = 100m;
			consolCost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			AssertEquals("Creditor", TestObjectCreator.Creditor1.OH_Code, consolCost.Creditor.OH_Code);
		}

		public void TestSetE6_AC_ChargeCodeUpdatesTaxAndVATOnApportionmentCharges()
		{
			var creator = new TestObjectCreator(Factory);
			//Preparing Charges
			SetGSTAndVATInfoChargeCode(creator, creator.CC1, creator.GST1);
			SetGSTAndVATInfoChargeCode(creator, creator.CC3, creator.GSTFREE1);
			creator.ABIGAS.OH_IsCreditor = true;
			creator.ABIGAS.CompanyData.SetAPTaxApplicable(true);
			//Preparing Shipments
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			AddShipmentAndJobToConsol(consol);
			AddShipmentAndJobToConsol(consol);
			Factory.Save();
			ApportionmentListing listing = new ApportionmentListing(Factory, consol);
			JobConsolCost consolCost = listing.CostsCollection.TryAddNew();
			consolCost.E6_OSCostAmount = 100m;
			consolCost.E6_OH_Creditor = creator.ABIGAS.PK;
			//When Charge Code CC1
			AssertForAChargeCode(creator.CC1, creator.GST1, consolCost);
			//When Charge Code CC3
			AssertForAChargeCode(creator.CC3, creator.GSTFREE1, consolCost);
		}

		public void TestSetE6_ATAlwaysUpdatesTaxOnApportionmentCharges()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";
			Job job1 = new Job.Loader(shipment1).TryCreateWithoutMutexForTestOnly();
			job1.JH_GB = GlbBranch.CurrentBranch.PK;
			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_RL_NKOrigin = "AUSYD";
			shipment2.JS_RL_NKDestination = "USCHI";
			Job job2 = new Job.Loader(shipment2).TryCreateWithoutMutexForTestOnly();
			job2.JH_GB = GlbBranch.CurrentBranch.PK;
			job2.JH_GE = creator.FISDepartment.PK;
			creator.CC1.AC_AT_GSTRate = creator.GST1.PK;
			creator.ABIGAS.OH_IsCreditor = true;
			creator.ABIGAS.CompanyData.SetAPTaxApplicable(true);
			creator.CC3.AC_AT_GSTRate = creator.GST1.PK;
			// Tax Override for CC3 to match second Shipment
			var taxOverride = creator.CC3.TaxOverrides.AddNew();
			taxOverride.AO_AT = creator.GSTFREE1.PK;
			taxOverride.AO_TaxRegCntryOrGroup = "ALL";
			taxOverride.AO_JobType = "SHP";
			taxOverride.AO_Destination = "EXP";
			taxOverride.AO_CostSellAll = "COS";
			taxOverride.AO_HomeCountryOrZone = "AU";
			taxOverride.AO_Origin = "AU";
			taxOverride.AO_Destination = "USMW";
			// AccChargeTaxOverride is not valid with empty AO_IncoTerm.
			taxOverride.AO_IncoTerm = "ALL";
			Factory.Save();
			ApportionmentListing listing = new ApportionmentListing(Factory, consol);
			JobConsolCost consolCost = listing.CostsCollection.TryAddNew();
			consolCost.E6_OSCostAmount = 100m;
			consolCost.E6_ApportionmentMethod = "SHP";
			consolCost.E6_OH_Creditor = creator.ABIGAS.PK;
			consolCost.E6_AC_ChargeCode = creator.CC1.PK;
			AssertEquals("JR_AT_CostGSTRate on first Charge", creator.GST1.PK, consolCost.ApportionmentCharges[0].JR_AT_CostGSTRate);
			AssertEquals("JR_AT_CostGSTRate on second Charge", creator.GST1.PK, consolCost.ApportionmentCharges[1].JR_AT_CostGSTRate);
			// Set different CHarge Code which has the same default Tax Rate but an oveeride to match second Shipment
			consolCost.E6_AC_ChargeCode = creator.CC3.PK;
			AssertEquals("JR_AT_CostGSTRate on first Charge", creator.GST1.PK, consolCost.ApportionmentCharges[0].JR_AT_CostGSTRate);
			AssertEquals("JR_AT_CostGSTRate on second Charge must be re-set from Consol Cost after Tax Override set a different value", creator.GST1.PK, consolCost.ApportionmentCharges[1].JR_AT_CostGSTRate);
		}

		public void TestSetE6_AC_ChargeCodeWillTriggerCalculateOSCostAmount()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			Job job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Charge charge1 = job.Charges.AddNew();
			charge1.JR_AC = creator.CC1.PK;
			charge1.JR_OSCostAmt = 50m;
			charge1.JR_OSSellAmt = 50m;
			Charge charge2 = job.Charges.AddNew();
			charge2.JR_AC = creator.CC3.PK;
			charge2.JR_OSCostAmt = 60m;
			charge2.JR_OSSellAmt = 60m;
			ApportionmentListing listing = new ApportionmentListing(Factory, consol);
			JobConsolCost cost1 = listing.CostsCollection.TryAddNew();
			cost1.E6_AC_ChargeCode = creator.CC1.PK;
			cost1.E6_OSCostAmount = 50m;
			JobConsolCost cost2 = listing.CostsCollection.TryAddNew();
			cost2.E6_AC_ChargeCode = creator.CC3.PK;
			cost2.E6_OSCostAmount = 60m;
			Factory.Save();
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			APInvoiceConsolCostCollection testCollection = new APInvoiceConsolCostCollection(Factory, invoice);
			var consolCost1 = AddNewConsolCostToInvoice(consol, invoice);
			consolCost1.E6_AC_ChargeCode = creator.CC1.PK;
			AssertEquals(50m, consolCost1.E6_OSCostAmount);
			var consolCost2 = AddNewConsolCostToInvoice(consol, invoice);
			AssertEquals("E6_ParentID should be copied from previous element", consolCost1.E6_ParentID, consolCost2.E6_ParentID);
			AssertEquals("Precondition - E6_OSCostAmount is 0", 0m, consolCost2.E6_OSCostAmount);
			consolCost2.E6_AC_ChargeCode = creator.CC3.PK;
			AssertEquals("Postcondition - E6_OSCostAmount shoulbe be calculated", 60m, consolCost2.E6_OSCostAmount);
		}

		public void TestE6_MasterBillNumber()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_MasterBillNum = "01623456554";
			Factory.Save();
			JobConsolCost cost1 = consol.GetApportionments().CostsCollection.TryAddNew();
			AssertEquals("01623456554", cost1.E6_MasterBillNumber);
		}

		public void TestE6_ConsolCostAndTotalAccrual()
		{
			Factory.SetContext(BusinessContext.APInvoiceApportionToConsol);
			var consol = ObjectCreator.CreateConsol("AUSYD", "NZAKL", "C0001");
			var shipment = ObjectCreator.CreateShipment("S0001", consol);
			var job = ObjectCreator.CreateJob(shipment, false);
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, "cc1", ObjectCreator.AUD, 10, ObjectCreator.Creditor1, ObjectCreator.AUD, 10, ObjectCreator.Debtor);
			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "cc3", ObjectCreator.AUD, 20, ObjectCreator.Creditor1, ObjectCreator.AUD, 20, ObjectCreator.Debtor);
			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, "cc1", ObjectCreator.AUD, 3, null, ObjectCreator.AUD, 3, ObjectCreator.Debtor);
			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "cc3", ObjectCreator.AUD, 2, null, ObjectCreator.AUD, 2, ObjectCreator.Debtor);
			Factory.Save();
			var invoice = ObjectCreator.CreateInvoice(typeof(APInvoice), "inv1", ObjectCreator.AUD, 1.0m, ObjectCreator.Creditor1);
			var cost1 = invoice.ConsolCosting.ConsolCosts.AddNew();
			cost1.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, JobConsolSchema.Constants.Prefix);
			cost1.E6_GC = GlbCompany.CurrentCompany.PK;
			cost1.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			cost1.E6_RX_NKCurrency = ObjectCreator.AUD.RX_Code;
			cost1.E6_LocalCostAmount = 1000m;
			var cost2 = invoice.ConsolCosting.ConsolCosts.AddNew();
			cost2.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, JobConsolSchema.Constants.Prefix);
			cost2.E6_GC = GlbCompany.CurrentCompany.PK;
			cost2.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			cost2.E6_RX_NKCurrency = ObjectCreator.AUD.RX_Code;
			cost2.E6_LocalCostAmount = 300m;
			var cost3 = invoice.ConsolCosting.ConsolCosts.AddNew();
			cost3.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, JobConsolSchema.Constants.Prefix);
			cost3.E6_AC_ChargeCode = ObjectCreator.CC3.PK;
			cost3.E6_RX_NKCurrency = ObjectCreator.AUD.RX_Code;
			cost3.E6_LocalCostAmount = 400m;
			Factory.Save();
			AssertEquals(1300m, cost1.E6_ConsolCostAccrual);
			AssertEquals(1300m, cost2.E6_ConsolCostAccrual);
			AssertEquals(400m, cost3.E6_ConsolCostAccrual);
			AssertEquals(1313m, cost1.E6_ConsolTotalAccrual);
			AssertEquals(1313m, cost2.E6_ConsolTotalAccrual);
			AssertEquals(422m, cost3.E6_ConsolTotalAccrual);
		}

		public void TestAddNewConsolCostWillSetParentTableCodeWhenSetParentID()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = Factory.New<ForwardingConsol>();
			JobConsolCost cost1 = consol.GetApportionments().CostsCollection.TryAddNew();
			cost1.E6_GC = GlbCompany.CurrentCompany.PK;
			cost1.E6_AC_ChargeCode = creator.CC1.PK;
			cost1.E6_RX_NKCurrency = creator.AUD.RX_Code;
			JobConsolCost cost2 = GetCost(consol);
			cost2.E6_GC = GlbCompany.CurrentCompany.PK;
			cost2.E6_AC_ChargeCode = creator.CC2.PK;
			cost2.E6_RX_NKCurrency = creator.AUD.RX_Code;
			Factory.Save();
			APInvoice invoice = Factory.New<APInvoice>();
			var newCost = invoice.ConsolCosting.ConsolCosts.AddNew();
			AssertEquals("Cost should have APInvoiceConsolCostCollection ParentCollection", invoice.ConsolCosting.ConsolCosts, ((IBusinessObjectInternals)newCost).ParentCollections.FirstOrDefault(x => x is APInvoiceConsolCostCollection));
			bool isEventCalled = false;
			invoice.ConsolCosting.ConsolCosts.OnConsolChanged += (sender, args) =>
			{
				isEventCalled = true;
				AssertEquals("E6_ParentTableCode should be set", "JK", newCost.E6_ParentTableCode);
			}

			;
			using (newCost.ReportSettingParentSuspender.GetSuspender())
			{
				newCost.E6_ParentID = consol.PK;
			}

			Assert("Event should be called during the test", isEventCalled);
		}

		public void TestExchangeRateDecimalPlaces()
		{
			JobConsolCost cost = Factory.New<JobConsolCost>();
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			AssertEquals("Reciprocal", 6, cost.ExchangeRateDecimalPlaces);
			GlbCompany.CurrentCompany.GC_IsReciprocal = false;
			AssertEquals("Not reciprocal", 6, cost.ExchangeRateDecimalPlaces);
		}

		public void TestSetE6_ParentIDAndE6_ParentTableCodeTogether()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			JobConsolCost cost = Factory.NewWithValidTestData<JobConsolCost>();
			using (cost.ReportSettingParentSuspender.GetSuspender())
			{
				cost.E6_ParentID = ZGuid.Empty;
				cost.E6_ParentTableCode = ZString.Empty;
				AssertEquals("Precondition - E6_ParentId should be empty", ZGuid.Empty, cost.E6_ParentID);
				AssertEquals("Precondition - E6_ParentTableCode should be empty", ZString.Empty, cost.E6_ParentTableCode);
				cost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, "JK");
				AssertEquals("Postcondition - E6_ParentId should be Consol's PK", consol.PK, cost.E6_ParentID);
				AssertEquals("Postcondition - E6_ParentTableCode should be JK", "JK", cost.E6_ParentTableCode);
			}
		}

		public void TestSetE6_ParentIDWillSetParentTableCode()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			JobConsolCost cost = Factory.NewWithValidTestData<JobConsolCost>();
			using (cost.ReportSettingParentSuspender.GetSuspender())
			{
				cost.E6_ParentID = ZGuid.Empty;
				cost.E6_ParentTableCode = ZString.Empty;
				AssertEquals("Precondition - E6_ParentTableCode is empty", ZString.Empty, cost.E6_ParentTableCode);
				cost.E6_ParentID = consol.PK;
				AssertEquals("Postcondition - E6_ParentTableCode is NOT empty", consol.TablePrefix, cost.E6_ParentTableCode);
			}
		}

		public void TestE6_Calc_LocalExTaxAmount()
		{
			JobConsolCost cost = Factory.NewWithValidTestData<JobConsolCost>();
			cost.E6_LocalCostAmount = 505m;
			AssertEquals(505m, cost.E6_Calc_LocalExTaxAmount);
		}

		public void TestE6_Calc_LocalGSTAmount()
		{
			var consol = ObjectCreator.CreateConsol();
			foreach (var isOverridden in new[] { true, false })
			{
				var cost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1);
				cost.E6_ExchangeRate = 0.5m;
				cost.E6_OSCostAmount = 5000m;
				cost.E6_AT_TaxRate = ObjectCreator.GST1.PK;
				cost.E6_IsTaxAmountOverridden = isOverridden;
				if (isOverridden)
				{
					cost.E6_OSGSTAmount_Calc = 499m;
					AssertEquals("local cost GST amount when override is ticked", 998m, cost.E6_Calc_LocalGSTAmount);
				}
				else
				{
					AssertEquals("local cost GST amount when override is not ticked", 1000m, cost.E6_Calc_LocalGSTAmount);
				}
			}
		}

		public void TestE6_Calc_LocalGSTAmount_WithTax()
		{
			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var consol = ObjectCreator.CreateConsol();
			var cost = GetCost(consol);
			cost.E6_LocalCostAmount = 500m;
			cost.E6_AT_TaxRate = ObjectCreator.GSTANDQST1WithDates.PK;
			AssertEquals(74.88m, cost.E6_Calc_LocalGSTAmount);

			cost.E6_TaxDate = ObjectCreator.GSTANDQST1WithDates_DateWithNoRate;
			AssertEquals(47.5m, cost.E6_Calc_LocalGSTAmount);

			cost.E6_TaxDate = ObjectCreator.GSTANDQST1WithDates_DateWithNoRateAndExtraRate;
			AssertEquals(0m, cost.E6_Calc_LocalGSTAmount);
		}

		[SuspendCriticalValidation]
		public void TestLocalGstAmountCalculation()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Chile);
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Chile;
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Chile;
			TestObjectCreator creator = new TestObjectCreator(Factory);
			creator.CreateExchangeRate(creator.USD, 664.987M);
			var taxRate = creator.CreateTaxRate("IVA", "Chile IVA", 19);
			ForwardingConsol consol = creator.CreateConsol("CLSAA", "USCHI", "C001001");
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "CLSAA";
			shipment.JS_RL_NKDestination = "USCHI";
			Job job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Charge charge1 = job.Charges.AddNew();
			charge1.JR_AC = creator.CC1.PK;
			charge1.JR_RX_NKCostCurrency = "USD";
			charge1.JR_OSCostAmt = 2.51M;
			charge1.JR_AT_CostGSTRate = taxRate.PK;
			ApportionmentListing listing = new ApportionmentListing(Factory, consol);
			JobConsolCost cost1 = listing.CostsCollection.TryAddNew();
			cost1.E6_AC_ChargeCode = creator.CC1.PK;
			cost1.E6_RX_NKCurrency = "USD";
			cost1.E6_OSCostAmount = 2.51M;
			cost1.E6_ExchangeRate = 664.987M;
			cost1.E6_AT_TaxRate = taxRate.PK;
			Factory.Save();
			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			var consolCost1 = AddNewConsolCostToInvoice(consol, invoice);
			consolCost1.E6_AC_ChargeCode = creator.CC1.PK;
			consolCost1.E6_RX_NKCurrency = "USD";
			consolCost1.E6_AT_TaxRate = taxRate.PK;
			AssertEquals(2.51M, consolCost1.E6_OSCostAmount);
			AssertEquals(0.48M, consolCost1.E6_OSGSTAmount_Calc);
			AssertEquals(1669M, consolCost1.E6_LocalCostAmount);
			AssertEquals("Local gst amount is calculated by applying 19% to local cost amount", 317M, consolCost1.InvoiceLocalTax);
			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			invoice = Factory.NewWithValidTestData<APInvoice>();
			consolCost1 = AddNewConsolCostToInvoice(consol, invoice);
			consolCost1.E6_AC_ChargeCode = creator.CC1.PK;
			consolCost1.E6_RX_NKCurrency = "USD";
			consolCost1.E6_AT_TaxRate = taxRate.PK;
			AssertEquals(2.51M, consolCost1.E6_OSCostAmount);
			AssertEquals(0.48M, consolCost1.E6_OSGSTAmount_Calc);
			AssertEquals(1669M, consolCost1.E6_LocalCostAmount);
			AssertEquals("Since local gst amount is being calculated by applying exchange rate to OS gst amount, local gst amount is not 317M", 319M, consolCost1.InvoiceLocalTax);
		}

		public void TestE6_Calc_LocalTotalAmount()
		{
			foreach (var isOverridden in new[] { true, false })
			{
				JobConsolCost cost = Factory.NewWithValidTestData<JobConsolCost>();
				cost.E6_ExchangeRate = 0.5m;
				cost.E6_LocalCostAmount = 5000m;
				cost.E6_OSCostAmount = 2500m;
				cost.E6_IsTaxAmountOverridden = isOverridden;
				if (isOverridden)
				{
					cost.E6_OSGSTAmount_Calc = 250m;
				}
				else
				{
					cost.E6_AT_TaxRate = ObjectCreator.GST1.PK;
				}

				AssertEquals(5500m, cost.E6_Calc_LocalTotalAmount);
			}
		}

		public void TestAdjustRoundingDifferencesOnLargestCharge_LocalCostAmount()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			shipment1.FillWithValidTestData();
			shipment1.JS_ActualWeight = 351.73m;
			shipment1.JS_ActualChargeable = 43m;
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			shipment2.FillWithValidTestData();
			shipment2.JS_ActualWeight = 786.23m;
			shipment2.JS_ActualChargeable = 5m;
			ForwardingShipment shipment3 = consol.Shipments.AddNew();
			shipment3.FillWithValidTestData();
			shipment3.JS_ActualWeight = 76.31m;
			shipment3.JS_ActualChargeable = 37m;
			ForwardingShipment shipment4 = consol.Shipments.AddNew();
			shipment4.FillWithValidTestData();
			shipment4.JS_ActualWeight = 486.73m;
			shipment4.JS_ActualChargeable = 31m;
			ForwardingShipment shipment5 = consol.Shipments.AddNew();
			shipment5.FillWithValidTestData();
			shipment5.JS_ActualWeight = 151.73m;
			shipment5.JS_ActualChargeable = 43m;
			ForwardingShipment shipment6 = consol.Shipments.AddNew();
			shipment6.FillWithValidTestData();
			shipment6.JS_ActualWeight = 737.23m;
			shipment6.JS_ActualChargeable = 52m;
			ForwardingShipment shipment7 = consol.Shipments.AddNew();
			shipment7.FillWithValidTestData();
			shipment7.JS_ActualWeight = 976.31m;
			shipment7.JS_ActualChargeable = 337m;
			ForwardingShipment shipment8 = consol.Shipments.AddNew();
			shipment8.FillWithValidTestData();
			shipment8.JS_ActualWeight = 86.73m;
			shipment8.JS_ActualChargeable = 1m;
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			AssertNotNull(cost);
			AssertEquals("Precondition: Number of Charges in list", 8, cost.ApportionmentCharges.Count);
			try
			{
				ApportionSplitCharge charge1 = FindChargeForJob(cost, shipment1);
				ApportionSplitCharge charge2 = FindChargeForJob(cost, shipment2);
				ApportionSplitCharge charge3 = FindChargeForJob(cost, shipment3);
				ApportionSplitCharge charge4 = FindChargeForJob(cost, shipment4);
				ApportionSplitCharge charge5 = FindChargeForJob(cost, shipment5);
				ApportionSplitCharge charge6 = FindChargeForJob(cost, shipment6);
				ApportionSplitCharge charge7 = FindChargeForJob(cost, shipment7);
				ApportionSplitCharge charge8 = FindChargeForJob(cost, shipment8);
				cost.E6_AC_ChargeCode = ObjectCreator.CC12.PK;
				cost.E6_RX_NKCurrency = "USD";
				cost.E6_OSCostAmount = 1840m;
				cost.E6_ApportionmentMethod = AllocationMethod.ChargeableUnits;
				cost.E6_ExchangeRate = 10m;
				AssertEquals(14.41m, charge1.JR_LocalCostAmt);
				AssertEquals(1.68m, charge2.JR_LocalCostAmt);
				AssertEquals(12.40m, charge3.JR_LocalCostAmt);
				AssertEquals(10.39m, charge4.JR_LocalCostAmt);
				AssertEquals(14.41m, charge5.JR_LocalCostAmt);
				AssertEquals(17.43m, charge6.JR_LocalCostAmt);
				AssertEquals(112.94m, charge7.JR_LocalCostAmt);
				AssertEquals(.34m, charge8.JR_LocalCostAmt);
			}
			finally
			{
				cost.CalculationStrategy.ReleaseMutexes();
			}

			cost.Validation.ValidateAll();
			cost.RunPreSaveValidation();
			((ISupportCriticalValidation)cost).CriticalValidation.RunOnSavingCheck();
			AssertNoErrors("Cost", cost);
			Factory.Save();
			Assert("Save was successful", true);
		}

		public void TestCalculateGSTIDForInterOfficeCosts()
		{
			JobConsolCost cost = Factory.NewWithValidTestData<JobConsolCost>();
			ObjectCreator.CC1.AC_AT_GSTRate = ObjectCreator.GST1.PK;
			cost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			ObjectCreator.ABIGAS.CompanyData.SetAPTaxApplicable(true);
			cost.E6_OH_Creditor = ObjectCreator.ABIGAS.PK;
			AssertEquals(ObjectCreator.CC1.AC_AT_GSTRate, cost.E6_AT_TaxRate);
			OrgHeader orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			orgProxy.CompanyData.SetAPTaxApplicable(true);
			cost.E6_OH_Creditor = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			AssertEquals(AccTaxRate.GetNOTREPORTTaxID(Factory, GlbCompany.CurrentCompany).PK, cost.E6_AT_TaxRate);
			OrgHeader orgWithSameTaxRegDetails = Factory.NewWithValidTestData<OrgHeader>();
			orgWithSameTaxRegDetails.CompanyData.SetAPTaxApplicable(true);
			orgWithSameTaxRegDetails.CustomsCodes.AddNew(Country.GetConsumptionTaxRegistrationOrgCusCode(GlbCompany.CurrentCompany.GC_RN_NKCountryCode), "ABC123");
			GlbCompany.CurrentCompany.GC_BusinessRegNo = "ABC123";
			cost.E6_OH_Creditor = orgWithSameTaxRegDetails.PK;
			AssertEquals(AccTaxRate.GetNOTREPORTTaxID(Factory, GlbCompany.CurrentCompany).PK, cost.E6_AT_TaxRate);
		}

		public void TestCalculateGSTIDFromTaxOverrides()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			Factory.Save();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ApportionmentListing apportionments = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apportionments.CostsCollection.TryAddNew();
			ObjectCreator.CC1.AC_AT_GSTRate = ObjectCreator.GST1.PK;
			cost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			ObjectCreator.ABIGAS.CompanyData.SetAPTaxApplicable(true);
			cost.E6_OH_Creditor = ObjectCreator.ABIGAS.PK;
			AssertEquals(ObjectCreator.CC1.AC_AT_GSTRate, cost.E6_AT_TaxRate);
			AccChargeTaxOverride taxOverride = ObjectCreator.CC1.TaxOverrides.AddNew();
			taxOverride.AO_AT = ObjectCreator.GSTFREE1.PK;
			taxOverride.AO_CostSellAll = "ALL";
			taxOverride.AO_Direction = "EXP";
			taxOverride.AO_Destination = "US";
			taxOverride.AO_Origin = "AU";
			taxOverride.AO_IncoTerm = "ALL";
			taxOverride.AO_JobType = JobInvoicingConsumerTypes.ForwardingConsol.Code;
			Factory.Save();
			cost.E6_OH_Creditor = ObjectCreator.AALSHI.PK; // Have to change one of the cache key component to eliminate caching on the ChargeCode level
			AssertEquals(ObjectCreator.GSTFREE1.PK, cost.E6_AT_TaxRate);
		}

		public void TestIsRevenuePosted()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = testObjectCreator.CreateConsol();
			var shipment1 = testObjectCreator.CreateShipment("S001001", consol);
			var shipment2 = testObjectCreator.CreateShipment("S001002", consol);
			var job1 = testObjectCreator.CreateJob(shipment1, testObjectCreator.LocalClient, 0M, testObjectCreator.Agent, 0M);
			var job2 = testObjectCreator.CreateJob(shipment2, testObjectCreator.LocalClient, 0M, testObjectCreator.Agent, 0M);
			var apps = new ApportionmentListing(Factory, consol);
			var cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = testObjectCreator.CC1.PK;
			cost.E6_RX_NKCurrency = "AUD";
			cost.E6_OSCostAmount = 100m;
			cost.E6_ApportionmentMethod = AllocationMethod.ChargeableUnits;
			AssertEquals("Precondition", 2, cost.ApportionmentCharges.Count);
			Assert("Revenue is not posted", !cost.IsRevenuePosted);
			var charge1 = cost.ApportionmentCharges[0];
			var arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			var arInvLine = (ARInvoiceLine)arInvoice.Lines.AddNew();
			arInvLine.AL_OSAmount = 200m;
			charge1.JR_AL_ARLine = arInvLine.PK;
			Assert("Precondition", charge1.JR_IsRevenuePosted);
			Assert("Revenue is posted", cost.IsRevenuePosted);
		}

		public void TestIsApproved()
		{
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			JobConsolCost cost = Factory.NewWithValidTestData<JobConsolCost>();
			Assert("No Invoice", cost.E6_AH_APInvoice.IsEmpty);
			Assert("IsApproved", !cost.IsApproved);
			cost.E6_AH_APInvoice = invoice.PK;
			Assert("IsApproved", cost.IsApproved);
			invoice.AH_TransactionType = TransactionTypes.CreditNote;
			Assert("IsApproved", cost.IsApproved);
			invoice.AH_TransactionType = TransactionTypes.UAInvoice;
			Assert("IsApproved", !cost.IsApproved);
			invoice.AH_TransactionType = TransactionTypes.UACreditNote;
			Assert("IsApproved", !cost.IsApproved);
		}

		public void TestIUpdateableCharge()
		{
			UAInvoice invoice = Factory.NewWithValidTestData<UAInvoice>();
			invoice.AH_Ledger = "AP";
			invoice.AH_TransactionType = "INV";
			Factory.Save();
			JobConsolCost cost = (JobConsolCost)GetNewBusinessObject();
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost.ParentAPInvoice = invoice;
			cost.E6_AH_APInvoice = invoice.PK;
			IQuickCalculatorCharge updateableCost = cost;
			AssertEquals(false, updateableCost.CanUpdateSell);
			AssertNull(updateableCost.InvoicingJob);
			AssertEquals(true, updateableCost.CanUpdateCost);
			AssertEquals(Env.Registry.FreightChargeCode, updateableCost.ChargeCode.PK);
			AssertEquals(cost.Factory, updateableCost.Factory);
			AutoRateInfo result = new AutoRateInfo(Factory);
			result.AddFlatPaymentBasis(300m, "C00001000", "AUD");
			updateableCost.SetAmount(CostSell.Cost, result, null);
			AssertEquals(300m, cost.E6_OSCostAmount);
			Assert(!cost.CostCalculationDescription.ToAscii().Contains("Cost was autocosted but cost amount was subsequently changed."));
			cost.E6_AH_APInvoice = ZGuid.NewZGuid();
			cost.ParentAPInvoice = null;
			AssertEquals(true, cost.IsPosted);
			AssertEquals(false, updateableCost.CanUpdateCost);
		}

		public void TestGetNewValidation()
		{
			UAInvoice invoice = Factory.NewWithValidTestData<UAInvoice>();
			invoice.AH_Ledger = "AP";
			invoice.AH_TransactionType = "INV";
			Factory.Save();
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.ParentAPInvoice = invoice;
			cost.E6_AH_APInvoice = invoice.PK;
			JobConsolCostValidation validation = cost.GetNewValidation_ForTestOnly();
			AssertEquals("Enterprise.Accounting.Business.ConsolCosting.ForwardingConsolCostingValidation", validation.ToString());
		}

		public void TestGetNewValidationForJobConsolCostFilteredCollection()
		{
			var invoice = Factory.NewWithValidTestData<UAInvoice>();
			invoice.AH_Ledger = "AP";
			invoice.AH_TransactionType = "INV";
			Factory.Save();
			var consol = Factory.New<ForwardingConsol>();
			var apps = new ApportionmentListing(Factory, consol);
			var cost1 = apps.CostsCollection.TryAddNew();
			var cost2 = apps.CostsCollection.TryAddNew();
			((IBusinessObjectInternals)cost1).ParentCollections[0] = apps.CostsFilteredCollection;
			AssertEquals("Enterprise.Accounting.Business.ConsolCosting.ForwardingConsolCostingValidation", (cost1.GetNewValidation_ForTestOnly()).ToString());
			var validation = (ForwardingConsolCostingValidation)(cost1.GetNewValidation_ForTestOnly());
			AssertContainsExactElementsInAnyOrder(new[] { cost1, cost2 }, validation.GetRelatedCostCollectionOfValidationForTest());
			apps.CostsFilteredCollection.Remove(cost2);
			validation = (ForwardingConsolCostingValidation)(cost1.GetNewValidation_ForTestOnly());
			AssertContainsExactElementsInAnyOrder(new[] { cost1 }, validation.GetRelatedCostCollectionOfValidationForTest());
		}

		public void TestIsApprovingPosting()
		{
			UAInvoice invoice = Factory.NewWithValidTestData<UAInvoice>();
			invoice.AH_Ledger = "AP";
			invoice.AH_TransactionType = "INV";
			Factory.Save();
			JobConsolCost cost = Factory.NewWithValidTestData<JobConsolCost>();
			cost.ParentAPInvoice = invoice;
			cost.E6_AH_APInvoice = invoice.PK;
			Assert(cost.IsApprovingPosting);
			APInvoice aPinvoice = Factory.NewWithValidTestData<APInvoice>();
			cost.ParentAPInvoice = aPinvoice;
			cost.E6_AH_APInvoice = aPinvoice.PK;
			Assert(!cost.IsApprovingPosting);
		}

		public void TestIsFinalGetsValidated()
		{
			bool oldValue = Env.Security.AllowPayablesInvoiceFinalFlag.IsAllowed;
			try
			{
				var expectError = @"You do not have appropriate security rights to tick this flag.
Please contact your system administrator for the following security right: Manage > Payables > Payables Transactions > New Transactions > Invoice > Allow Tick Final Flag on Payables Invoice";
				TestObjectCreator creator = new TestObjectCreator(Factory);
				ForwardingConsol consol = creator.CreateConsol("KRS", "AUM", "C0001");
				JobConsolCost cost = creator.CreateConsolCost(consol, creator.CC1, creator.ABIGAS);
				Env.Security.AllowPayablesInvoiceFinalFlag.IsAllowed = true;
				cost.IsFinal = true;
				AssertNoError(cost.IsFinalInfo, expectError);
				Env.Security.AllowPayablesInvoiceFinalFlag.IsAllowed = false;
				cost.IsFinal = true;
				AssertHasError(cost.IsFinalInfo, expectError);
			}
			finally
			{
				Env.Security.AllowPayablesInvoiceFinalFlag.IsAllowed = oldValue;
			}
		}

		public void TestE6_AT_TaxRate_ReadOnly()
		{
			bool oldValue = AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			try
			{
				UAInvoice invoice = Factory.NewWithValidTestData<UAInvoice>();
				invoice.AH_Ledger = "AP";
				invoice.AH_TransactionType = "INV";
				Factory.Save();
				var cost = Factory.NewWithValidTestData<JobConsolCost>();
				OrgHeader creditor = ObjectCreator.ABIGAS;
				creditor.CompanyData.SetAPTaxApplicable(true);
				cost.E6_OH_Creditor = creditor.PK;
				cost.ParentAPInvoice = invoice;
				cost.E6_AH_APInvoice = invoice.PK;
				AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				Env.Security.MaintainConsolJobInvoicingAllowOverrideCostTaxId.IsAllowed = false;
				Assert(cost.IsTaxRateReadOnly);
				Env.Security.MaintainConsolJobInvoicingAllowOverrideCostTaxId.IsAllowed = true;
				Assert("Tax Rate must be NOT readonly because IsApprovingPosting is true", !cost.IsTaxRateReadOnly);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, oldValue);
			}
		}

		public void TestIsTaxRateReadOnly_WithAutoJRJTaxRegNum()
		{
			TestObjectCreator.PrepareAutoJRJTestEnvironment();
			TestObjectCreator.AutoJRJCreditorOrDebtor.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var consol = TestObjectCreator.CreateConsol();
			var shipment1 = TestObjectCreator.CreateShipment("S00000001", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1);
			var shipment2 = TestObjectCreator.CreateShipment("S00000002", consol);
			var job2 = TestObjectCreator.CreateJob(shipment2);
			Factory.Save();

			var apportionList = new ApportionmentListing(Factory, consol);

			AssertIsTaxRateReadOnly(apportionList, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Tax, true, true);
			AssertIsTaxRateReadOnly(apportionList, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Tax, false, false);
			AssertIsTaxRateReadOnly(apportionList, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Yes, true, true);
			AssertIsTaxRateReadOnly(apportionList, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Yes, false, true);
			AssertIsTaxRateReadOnly(apportionList, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Non, true, false);
			AssertIsTaxRateReadOnly(apportionList, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Non, false, false);
		}

		public void TestIsTaxRateReadOnly_WithAutoJRJTaxRegNum_IsUsedForApportionment()
		{
			TestObjectCreator.PrepareAutoJRJTestEnvironment(false);
			TestObjectCreator.AutoJRJCreditorOrDebtor.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Factory.Save();

			var consol = TestObjectCreator.CreateConsol();
			var shipment1 = TestObjectCreator.CreateShipment("S00000001", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1);
			var shipment2 = TestObjectCreator.CreateShipment("S00000002", consol);
			var job2 = TestObjectCreator.CreateJob(shipment2);
			Factory.Save();

			var apportionList = new ApportionmentListing(Factory, consol);

			var consolCost = apportionList.CostsCollection.TryAddNew();
			consolCost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			consolCost.E6_OH_Creditor = TestObjectCreator.AutoJRJChargeBranchOrgProxy.PK;
			consolCost.E6_LocalCostAmount = 190m;
			consolCost.E6_InvoiceNum = "INV000001";
			consolCost.E6_InvoiceDate = ZDateTime.Today;

			var charge1 = consolCost.ApportionmentCharges[0];
			var charge2 = consolCost.ApportionmentCharges[1];

			charge1.JR_GB = TestObjectCreator.NonCurrentBranch.PK;
			charge1.JR_IsUsedForApportionment = false;
			charge2.JR_GB = GlbBranch.CurrentBranch.PK;

			AssertEquals("Apportion charge not in use should not impact the tax rate readonly", true, consolCost.IsTaxRateReadOnly);
		}

		public void TestIsTaxRateReadOnly_WithAutoJRJTaxRegNum_ApportionedChargesHaveDifferentBranches()
		{
			TestObjectCreator.PrepareAutoJRJTestEnvironment();
			TestObjectCreator.AutoJRJCreditorOrDebtor.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var consol = TestObjectCreator.CreateConsol();
			var shipment1 = TestObjectCreator.CreateShipment("S00000001", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1);
			var shipment2 = TestObjectCreator.CreateShipment("S00000002", consol);
			var job2 = TestObjectCreator.CreateJob(shipment2);
			Factory.Save();

			var apportionList = new ApportionmentListing(Factory, consol);

			Action<JobConsolCost> additionalConfigAction = (consolCost) =>
			{
				consolCost.ApportionmentCharges[0].JR_GB = TestObjectCreator.NonCurrentBranch.PK;
				consolCost.ApportionmentCharges[1].JR_GB = GlbBranch.CurrentBranch.PK;
			};

			AssertIsTaxRateReadOnly(apportionList, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Tax, false, true, additionalConfigAction);
			AssertIsTaxRateReadOnly(apportionList, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Tax, true, true, additionalConfigAction);
			AssertIsTaxRateReadOnly(apportionList, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Yes, false, true, additionalConfigAction);
			AssertIsTaxRateReadOnly(apportionList, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Yes, true, true, additionalConfigAction);
			AssertIsTaxRateReadOnly(apportionList, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Non, false, false, additionalConfigAction);
			AssertIsTaxRateReadOnly(apportionList, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Non, true, false, additionalConfigAction);
		}

		void AssertIsTaxRateReadOnly(ApportionmentListing apportionList, string autoJRJStatus, bool isTaxRegNumTheSame, bool expectedValue, Action<JobConsolCost> additionalConfig = null)
		{
			AccountingConfigurationRegistry.Instance.EnableAutoJobRevenueJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, autoJRJStatus);
			TestObjectCreator.PrepareAutoJRJTaxRegistrationNumbers(isTaxRegNumTheSame);

			var consolCost = apportionList.CostsCollection.TryAddNew();
			consolCost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			consolCost.E6_OH_Creditor = TestObjectCreator.AutoJRJCreditorOrDebtor.PK;
			consolCost.E6_LocalCostAmount = 190m;
			consolCost.E6_InvoiceNum = "INV000001";
			consolCost.E6_InvoiceDate = ZDateTime.Today;

			additionalConfig?.Invoke(consolCost);

			AssertEquals(expectedValue, consolCost.IsTaxRateReadOnly);

			consolCost.Delete();
			Factory.Save();
		}

		public void TestE6_GB_CostTaxBranch_ReadOnly()
		{
			AssertEquals(true, TestTaxBranchReadonly(true, false, false).E6_GB_CostTaxBranchInfo.ReadOnly);
			AssertEquals(true, TestTaxBranchReadonly(false, true, false).E6_GB_CostTaxBranchInfo.ReadOnly);
			AssertEquals(true, TestTaxBranchReadonly(false, false, true).E6_GB_CostTaxBranchInfo.ReadOnly);
			AssertEquals(false, TestTaxBranchReadonly(false, false, false).E6_GB_CostTaxBranchInfo.ReadOnly);
			AssertEquals(true, TestTaxBranchReadonly(false, false, false, false).E6_GB_CostTaxBranchInfo.ReadOnly);

			JobConsolCost TestTaxBranchReadonly(bool isTaxBranchNotApplicable, bool isAccountGSTNotRegistered, bool isPosted, bool isSecurityAllowed = true)
			{
				var cost = Factory.NewWithValidTestData<JobConsolCost>();
				cost.E6_OH_Creditor = TestObjectCreator.Creditor1.PK;

				TestObjectCreator.SetUpTaxBranchRegistry(!isTaxBranchNotApplicable);
				AssertEquals(isTaxBranchNotApplicable, !AccountingMasterFilesUtils.IsTaxBranchApplicable);

				if (!isTaxBranchNotApplicable)
				{
					TestObjectCreator.ResetSecurityCore();
					Env.Security.MaintainConsolJobInvoicingAllowOverrideCostTaxBranch.IsAllowed = isSecurityAllowed;
				}

				TestObjectCreator.Creditor1.CompanyData.OB_APVATConfig = isAccountGSTNotRegistered ?
					AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code :
					AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
				AssertEquals(isAccountGSTNotRegistered, !cost.IsCostGSTApplicable);

				var header = Factory.NewWithValidTestData<AccTransactionHeader>();
				if (isPosted)
				{
					cost.E6_AH_APInvoice = header.PK;
				}
				AssertEquals(isPosted, cost.IsPosted);

				return cost;
			}
		}

		public void TestE6_AC_ChargeCode_ReadOnly_WhenIsGatewayConsolCost()
		{
			var gatewayAgentPort = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.First().PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(gatewayAgentPort);
			consol.JK_UniqueConsignRef = "C00001111";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			consol.Shipments.Add(shipment);
			using (var job = new Job.Loader(Factory, consol).TryCreateWithoutMutexForTestOnly())
			{
				var chargeCode = ObjectCreator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK);
				var charge = job.Charges.AddNew();
				charge.JR_AC = chargeCode.PK;
				charge.JR_OSSellAmt = 250m;
				charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
				charge.JR_JH_InternalJob = job.PK;
				charge.JR_GE_InternalDept = charge.JR_GE;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				Assert(consol.IsGateway());
				Factory.Save();
				AssertEquals(1, consol.GetApportionments(true).CostsCollection.Count);
				var cost = consol.GetApportionments(true).CostsCollection[0];
				Assert(cost.IsGatewayConsolCost);
				Assert(cost.E6_AC_ChargeCodeInfo.ReadOnly);
			}
		}

		public void TestReadOnlyFields_WhenIsImportedConsol()
		{
			var cost = Factory.NewWithValidTestData<JobConsolCost>();
			cost.RelatedConsolCostPK = ZGuid.NewZGuid();
			Assert(cost.E6_AC_ChargeCodeInfo.ReadOnly);
			Assert(cost.E6_ParentIDInfo.ReadOnly);
			Assert(cost.E6_ApportionToRelatedShipmentsInfo.ReadOnly);
		}

		public void TestE6_AB_BankAccount_ReadOnly_WhenIsGatewayConsolCost()
		{
			var gatewayAgentPort = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.First().PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(gatewayAgentPort);
			consol.JK_UniqueConsignRef = "C00001111";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			consol.Shipments.Add(shipment);
			using (var job = new Job.Loader(Factory, consol).TryCreateWithoutMutexForTestOnly())
			{
				var chargeCode = ObjectCreator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK);
				var charge = job.Charges.AddNew();
				charge.JR_AC = chargeCode.PK;
				charge.JR_OSSellAmt = 250m;
				charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
				charge.JR_JH_InternalJob = job.PK;
				charge.JR_GE_InternalDept = charge.JR_GE;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				Assert(consol.IsGateway());
				Factory.Save();
				AssertEquals(1, consol.GetApportionments(true).CostsCollection.Count);
				var cost = consol.GetApportionments(true).CostsCollection[0];
				Assert(cost.IsGatewayConsolCost);
				Assert(cost.E6_AB_BankAccountInfo.ReadOnly);
			}
		}

		public void TestPaymentTypeIsCashWhenAccountTypeIsCash()
		{
			var cashAccount = Factory.NewWithValidTestData<AccBankAccount>();
			cashAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;

			var cost = Factory.New<JobConsolCost>();
			cost.E6_AB_BankAccount = cashAccount.PK;
			var errorSelectCSH = $"For Cash Account, please select CSH - Cash {cost.E6_PaymentTypeInfo.HumanReadableName}.";

			cost.E6_PaymentType = ReceiptTypes.DirectDebit;
			AssertHasError(cost.E6_PaymentTypeInfo, errorSelectCSH);

			cost.E6_PaymentType = ReceiptTypes.Cash;
			AssertNoError(cost.E6_PaymentTypeInfo, errorSelectCSH);
		}

		public void TestPaymentTypeIsSetToCashWhenCashAccountIsSelected()
		{
			var cost = Factory.New<JobConsolCost>();
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();

			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.BNK;
			cost.E6_AB_BankAccount = bankAccount.PK;
			AssertNotEquals(ReceiptTypes.Cash, cost.E6_PaymentType);

			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;
			cost.E6_AB_BankAccount = bankAccount.PK;
			AssertEquals(ReceiptTypes.Cash, cost.E6_PaymentType);
		}

		public void TestE6_AK_ChequeBook_ReadOnly_WhenIsGatewayConsolCost()
		{
			var gatewayAgentPort = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.First().PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(gatewayAgentPort);
			consol.JK_UniqueConsignRef = "C00001111";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			consol.Shipments.Add(shipment);
			using (var job = new Job.Loader(Factory, consol).TryCreateWithoutMutexForTestOnly())
			{
				var chargeCode = ObjectCreator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK);
				var charge = job.Charges.AddNew();
				charge.JR_AC = chargeCode.PK;
				charge.JR_OSSellAmt = 250m;
				charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
				charge.JR_JH_InternalJob = job.PK;
				charge.JR_GE_InternalDept = charge.JR_GE;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				Assert(consol.IsGateway());
				Factory.Save();
				AssertEquals(1, consol.GetApportionments(true).CostsCollection.Count);
				var cost = consol.GetApportionments(true).CostsCollection[0];
				Assert(cost.IsGatewayConsolCost);
				Assert(cost.E6_AK_ChequeBookInfo.ReadOnly);
			}
		}

		public void TestE6_InvoiceNum_ReadOnly_WhenIsGatewayConsolCost()
		{
			var gatewayAgentPort = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.First().PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(gatewayAgentPort);
			consol.JK_UniqueConsignRef = "C00001111";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			consol.Shipments.Add(shipment);
			using (var job = new Job.Loader(Factory, consol).TryCreateWithoutMutexForTestOnly())
			{
				var chargeCode = ObjectCreator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK);
				var charge = job.Charges.AddNew();
				charge.JR_AC = chargeCode.PK;
				charge.JR_OSSellAmt = 250m;
				charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
				charge.JR_JH_InternalJob = job.PK;
				charge.JR_GE_InternalDept = charge.JR_GE;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				Assert(consol.IsGateway());
				Factory.Save();
				AssertEquals(1, consol.GetApportionments(true).CostsCollection.Count);
				var cost = consol.GetApportionments(true).CostsCollection[0];
				Assert(cost.IsGatewayConsolCost);
				Assert(cost.E6_InvoiceNumInfo.ReadOnly);
			}
		}

		public void TestE6_DocumentReceivedDate_ReadOnly_WhenIsGatewayConsolCost()
		{
			var gatewayAgentPort = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.First().PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(gatewayAgentPort);
			consol.JK_UniqueConsignRef = "C00001111";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			consol.Shipments.Add(shipment);
			using (var job = new Job.Loader(Factory, consol).TryCreateWithoutMutexForTestOnly())
			{
				var chargeCode = ObjectCreator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK);
				var charge = job.Charges.AddNew();
				charge.JR_AC = chargeCode.PK;
				charge.JR_OSSellAmt = 250m;
				charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
				charge.JR_JH_InternalJob = job.PK;
				charge.JR_GE_InternalDept = charge.JR_GE;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				Assert(consol.IsGateway());
				Factory.Save();
				AssertEquals(1, consol.GetApportionments(true).CostsCollection.Count);
				var cost = consol.GetApportionments(true).CostsCollection[0];
				Assert(cost.IsGatewayConsolCost);
				Assert(cost.E6_DocumentReceivedDateInfo.ReadOnly);
			}
		}

		public void TestE6_InvoiceDate_ReadOnly_WhenIsGatewayConsolCost()
		{
			var gatewayAgentPort = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.First().PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(gatewayAgentPort);
			consol.JK_UniqueConsignRef = "C00001111";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			consol.Shipments.Add(shipment);
			using (var job = new Job.Loader(Factory, consol).TryCreateWithoutMutexForTestOnly())
			{
				var chargeCode = ObjectCreator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK);
				var charge = job.Charges.AddNew();
				charge.JR_AC = chargeCode.PK;
				charge.JR_OSSellAmt = 250m;
				charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
				charge.JR_JH_InternalJob = job.PK;
				charge.JR_GE_InternalDept = charge.JR_GE;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				Assert(consol.IsGateway());
				Factory.Save();
				AssertEquals(1, consol.GetApportionments(true).CostsCollection.Count);
				var cost = consol.GetApportionments(true).CostsCollection[0];
				Assert(cost.IsGatewayConsolCost);
				Assert(cost.E6_InvoiceDateInfo.ReadOnly);
			}
		}

		public void TestE6_PaymentDate_ReadOnly_WhenIsGatewayConsolCost()
		{
			var gatewayAgentPort = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.First().PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(gatewayAgentPort);
			consol.JK_UniqueConsignRef = "C00001111";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			consol.Shipments.Add(shipment);
			using (var job = new Job.Loader(Factory, consol).TryCreateWithoutMutexForTestOnly())
			{
				var chargeCode = ObjectCreator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK);
				var charge = job.Charges.AddNew();
				charge.JR_AC = chargeCode.PK;
				charge.JR_OSSellAmt = 250m;
				charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
				charge.JR_JH_InternalJob = job.PK;
				charge.JR_GE_InternalDept = charge.JR_GE;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				Assert(consol.IsGateway());
				Factory.Save();
				AssertEquals(1, consol.GetApportionments(true).CostsCollection.Count);
				var cost = consol.GetApportionments(true).CostsCollection[0];
				Assert(cost.IsGatewayConsolCost);
				Assert(cost.E6_PaymentDateInfo.ReadOnly);
			}
		}

		public void TestE6_ChequeOrReference_ReadOnly_WhenIsGatewayConsolCost()
		{
			var gatewayAgentPort = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.First().PK;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(gatewayAgentPort);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.JK_UniqueConsignRef = "C00001111";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			consol.Shipments.Add(shipment);
			using (var job = new Job.Loader(Factory, consol).TryCreateWithoutMutexForTestOnly())
			{
				var chargeCode = ObjectCreator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK);
				var charge = job.Charges.AddNew();
				charge.JR_AC = chargeCode.PK;
				charge.JR_OSSellAmt = 250m;
				charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
				charge.JR_JH_InternalJob = job.PK;
				charge.JR_GE_InternalDept = charge.JR_GE;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				Assert(consol.IsGateway());
				Factory.Save();
				AssertEquals(1, consol.GetApportionments(true).CostsCollection.Count);
				var cost = consol.GetApportionments(true).CostsCollection[0];
				Assert(cost.IsGatewayConsolCost);
				Assert(cost.E6_ChequeOrReferenceInfo.ReadOnly);
			}
		}

		public void TestE6_OH_Creditor_NotCausingApportionChargeValidationError()
		{
			GlbDepartment.CurrentDepartment.GE_Misc = false;
			GlbDepartment.CurrentDepartment.Factory.Save();

			var creditor = TestObjectCreator.Creditor1;
			creditor.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;

			var branchForCost = TestObjectCreator.CreateBranch("CST", GlbCompany.CurrentCompany);
			TestObjectCreator.CC1.AC_AT_GSTRate = ZGuid.Empty;
			TestObjectCreator.CC1.TaxOverrides.RemoveAndDeleteAll();
			TestObjectCreator.CreateTaxOverrides(TestObjectCreator.CC1
				, taxOverride => {
					taxOverride.AO_AT = TestObjectCreator.FREEVAT.PK;
					taxOverride.AO_A9_DefaultVATClass = TestObjectCreator.TaxMsg1.PK;
				});
			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consol = TestObjectCreator.CreateConsol("INDEL", "AUSYD", "C0001", false);
			consol.Shipments.Add(shipment);

			var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1);

			var apportionCharge = cost.ApportionmentCharges[0];
			apportionCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;

			AssertEquals("PreCondition", ZGuid.Empty, cost.E6_AT_TaxRate);
			AssertEquals("PreCondition", ZGuid.Empty, cost.E6_A9_VATClass);
			AssertEquals("PreCondition", ZGuid.Empty, apportionCharge.JR_AT_CostGSTRate);
			AssertEquals("PreCondition", ZGuid.Empty, apportionCharge.JR_A9_CostVATClass);
			AssertEquals("PreCondition", false, apportionCharge.HasErrors);

			cost.E6_OH_Creditor = creditor.PK;

			AssertEquals(TestObjectCreator.FREEVAT.PK, cost.E6_AT_TaxRate);
			AssertEquals(TestObjectCreator.TaxMsg1.PK, cost.E6_A9_VATClass);
			AssertEquals(TestObjectCreator.FREEVAT.PK, apportionCharge.JR_AT_CostGSTRate);
			AssertEquals(TestObjectCreator.TaxMsg1.PK, apportionCharge.JR_A9_CostVATClass);
			AssertEquals(false, apportionCharge.HasErrors);
			shipment.Job.Dispose();
		}

		public void TestE6_OH_Creditor_NotCausingApportionChargeValidationError_WithPlaceOfSupplyOverride()
		{
			GlbDepartment.CurrentDepartment.GE_Misc = false;
			GlbDepartment.CurrentDepartment.Factory.Save();

			var creditor = TestObjectCreator.Creditor1;
			creditor.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;

			var branchForCost = TestObjectCreator.CreateBranch("CST", GlbCompany.CurrentCompany);
			TestObjectCreator.CC1.AC_AT_GSTRate = ZGuid.Empty;
			TestObjectCreator.CC1.TaxOverrides.RemoveAndDeleteAll();
			TestObjectCreator.CreateTaxOverrides(TestObjectCreator.CC1
				, taxOverride => {
					taxOverride.AO_AT = TestObjectCreator.FREEVAT.PK;
					taxOverride.AO_A9_DefaultVATClass = TestObjectCreator.TaxMsg1.PK;
					taxOverride.AO_HomeCountryOrZone = "AU";
				});

			AssertEquals(AccountingMasterFilesRegistry.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.DefaultCode, AccountingMasterFilesRegistry.Instance.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRules.Value);
			PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code);
			AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(null, CostSell.Cost, PlaceOfSupplyTypes.State.Code, "NSW", branch: GlbBranch.CurrentBranch);

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consol = TestObjectCreator.CreateConsol("INDEL", "AUSYD", "C0001", false);
			consol.Shipments.Add(shipment);

			var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1);

			var apportionCharge = cost.ApportionmentCharges[0];
			apportionCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;

			cost.E6_OH_Creditor = ZGuid.Empty;
			cost.E6_PlaceOfSupply = ZString.Empty;
			AssertEquals("PreCondition", ZString.Empty, cost.E6_PlaceOfSupply);
			AssertEquals("PreCondition", ZGuid.Empty, cost.E6_AT_TaxRate);
			AssertEquals("PreCondition", ZGuid.Empty, cost.E6_A9_VATClass);
			AssertEquals("PreCondition", ZGuid.Empty, apportionCharge.JR_AT_CostGSTRate);
			AssertEquals("PreCondition", ZGuid.Empty, apportionCharge.JR_A9_CostVATClass);
			AssertEquals("PreCondition", false, apportionCharge.HasErrors);

			cost.E6_OH_Creditor = creditor.PK;

			AssertEquals("PreCondition POS will be overrided and trigger tax overriding", "NSW", cost.E6_PlaceOfSupply);
			AssertEquals(TestObjectCreator.FREEVAT.PK, cost.E6_AT_TaxRate);
			AssertEquals(TestObjectCreator.TaxMsg1.PK, cost.E6_A9_VATClass);
			AssertEquals(TestObjectCreator.FREEVAT.PK, apportionCharge.JR_AT_CostGSTRate);
			AssertEquals(TestObjectCreator.TaxMsg1.PK, apportionCharge.JR_A9_CostVATClass);
			AssertEquals(false, apportionCharge.HasErrors);
			shipment.Job.Dispose();
		}

		public void TestE6_OH_Creditor_NotCausingApportionChargeValidationError_WithTaxBranchOverride()
		{
			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			GlbDepartment.CurrentDepartment.GE_Misc = false;
			GlbDepartment.CurrentDepartment.Factory.Save();

			var creditor = TestObjectCreator.Creditor1;
			creditor.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			TestObjectCreator.SetUpTaxBranchRegistry(true);

			var branchForCost = TestObjectCreator.CreateBranch("CST", GlbCompany.CurrentCompany);
			TestObjectCreator.CC1.AC_AT_GSTRate = ZGuid.Empty;
			TestObjectCreator.CC1.TaxOverrides.RemoveAndDeleteAll();
			TestObjectCreator.CreateTaxOverrides(TestObjectCreator.CC1
				, taxOverride => {
					taxOverride.AO_AT = TestObjectCreator.FREEVAT.PK;
					taxOverride.AO_A9_DefaultVATClass = TestObjectCreator.TaxMsg1.PK;
					taxOverride.AO_GB = GlbBranch.CurrentBranch.PK;
				});
			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consol = TestObjectCreator.CreateConsol("INDEL", "AUSYD", "C0001", false);
			consol.Shipments.Add(shipment);

			var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1);
			cost.E6_GB_CostTaxBranch = ZGuid.Empty;

			var apportionCharge = cost.ApportionmentCharges[0];
			apportionCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;

			AssertEquals("PreCondition", ZGuid.Empty, cost.E6_AT_TaxRate);
			AssertEquals("PreCondition", ZGuid.Empty, cost.E6_GB_CostTaxBranch);
			AssertEquals("PreCondition", ZGuid.Empty, cost.E6_A9_VATClass);
			AssertEquals("PreCondition", ZGuid.Empty, apportionCharge.JR_AT_CostGSTRate);
			AssertEquals("PreCondition", ZGuid.Empty, apportionCharge.JR_A9_CostVATClass);
			AssertEquals("PreCondition", false, apportionCharge.HasErrors);

			cost.E6_OH_Creditor = creditor.PK;

			AssertEquals("PreCondition taxBranch will be overrided and trigger tax overriding", GlbBranch.CurrentBranch.PK, cost.E6_GB_CostTaxBranch);
			AssertEquals(TestObjectCreator.FREEVAT.PK, cost.E6_AT_TaxRate);
			AssertEquals(TestObjectCreator.TaxMsg1.PK, cost.E6_A9_VATClass);
			AssertEquals(TestObjectCreator.FREEVAT.PK, apportionCharge.JR_AT_CostGSTRate);
			AssertEquals(TestObjectCreator.TaxMsg1.PK, apportionCharge.JR_A9_CostVATClass);
			AssertEquals(false, apportionCharge.HasErrors);
			shipment.Job.Dispose();
		}

		public void TestE6_OH_Creditor_ReadOnly_WhenIsGatewayConsolCost()
		{
			var gatewayAgentPort = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.First().PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(gatewayAgentPort);
			consol.JK_UniqueConsignRef = "C00001111";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			consol.Shipments.Add(shipment);
			using (var job = new Job.Loader(Factory, consol).TryCreateWithoutMutexForTestOnly())
			{
				var chargeCode = ObjectCreator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK);
				var charge = job.Charges.AddNew();
				charge.JR_AC = chargeCode.PK;
				charge.JR_OSSellAmt = 250m;
				charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
				charge.JR_JH_InternalJob = job.PK;
				charge.JR_GE_InternalDept = charge.JR_GE;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				Assert(consol.IsGateway());
				Factory.Save();
				consol.Factory.ClearCachedValue<ApportionmentListing>("ApportionmentListing|" + consol.PK.ToString());
				AssertEquals(1, consol.GetApportionments(true).CostsCollection.Count);
				var cost = consol.GetApportionments(true).CostsCollection[0];
				Assert(cost.IsGatewayConsolCost);
				Assert(cost.E6_OH_CreditorInfo.ReadOnly);
			}
		}

		public void TestE6_RX_NKCurrency_ReadOnly_WhenIsGatewayConsolCost()
		{
			var gatewayAgentPort = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.First().PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(gatewayAgentPort);
			consol.JK_UniqueConsignRef = "C00001111";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			consol.Shipments.Add(shipment);
			using (var job = new Job.Loader(Factory, consol).TryCreateWithoutMutexForTestOnly())
			{
				var chargeCode = ObjectCreator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK);
				var charge = job.Charges.AddNew();
				charge.JR_AC = chargeCode.PK;
				charge.JR_OSSellAmt = 250m;
				charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
				charge.JR_JH_InternalJob = job.PK;
				charge.JR_GE_InternalDept = charge.JR_GE;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				Assert(consol.IsGateway());
				Factory.Save();
				AssertEquals(1, consol.GetApportionments(true).CostsCollection.Count);
				var cost = consol.GetApportionments(true).CostsCollection[0];
				Assert(cost.IsGatewayConsolCost);
				Assert(cost.E6_RX_NKCurrencyInfo.ReadOnly);
			}
		}

		public void TestE6_ExchangeRate_ReadOnly_WhenIsGatewayConsolCost()
		{
			var gatewayAgentPort = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.First().PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(gatewayAgentPort);
			consol.JK_UniqueConsignRef = "C00001111";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			consol.Shipments.Add(shipment);
			using (var job = new Job.Loader(Factory, consol).TryCreateWithoutMutexForTestOnly())
			{
				var chargeCode = ObjectCreator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK);
				var charge = job.Charges.AddNew();
				charge.JR_AC = chargeCode.PK;
				charge.JR_OSSellAmt = 250m;
				charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
				charge.JR_JH_InternalJob = job.PK;
				charge.JR_GE_InternalDept = charge.JR_GE;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				Assert(consol.IsGateway());
				Factory.Save();
				AssertEquals(1, consol.GetApportionments(true).CostsCollection.Count);
				var cost = consol.GetApportionments(true).CostsCollection[0];
				Assert(cost.IsGatewayConsolCost);
				Assert(cost.E6_ExchangeRateInfo.ReadOnly);
			}
		}

		public void TestE6_PaymentType_ReadOnly_WhenIsGatewayConsolCost()
		{
			var gatewayAgentPort = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.First().PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(gatewayAgentPort);
			consol.JK_UniqueConsignRef = "C00001111";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			consol.Shipments.Add(shipment);
			using (var job = new Job.Loader(Factory, consol).TryCreateWithoutMutexForTestOnly())
			{
				var chargeCode = ObjectCreator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK);
				var charge = job.Charges.AddNew();
				charge.JR_AC = chargeCode.PK;
				charge.JR_OSSellAmt = 250m;
				charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
				charge.JR_JH_InternalJob = job.PK;
				charge.JR_GE_InternalDept = charge.JR_GE;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				Assert(consol.IsGateway());
				Factory.Save();
				AssertEquals(1, consol.GetApportionments(true).CostsCollection.Count);
				var cost = consol.GetApportionments(true).CostsCollection[0];
				Assert(cost.IsGatewayConsolCost);
				Assert(cost.E6_PaymentTypeInfo.ReadOnly);
			}
		}

		public void TestE6_LocalCostAmount_ReadOnly_WhenIsGatewayConsolCost()
		{
			var gatewayAgentPort = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.First().PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(gatewayAgentPort);
			consol.JK_UniqueConsignRef = "C00001111";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			consol.Shipments.Add(shipment);
			using (var job = new Job.Loader(Factory, consol).TryCreateWithoutMutexForTestOnly())
			{
				var chargeCode = ObjectCreator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK);
				var charge = job.Charges.AddNew();
				charge.JR_AC = chargeCode.PK;
				charge.JR_OSSellAmt = 250m;
				charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
				charge.JR_JH_InternalJob = job.PK;
				charge.JR_GE_InternalDept = charge.JR_GE;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				Assert(consol.IsGateway());
				Factory.Save();
				AssertEquals(1, consol.GetApportionments(true).CostsCollection.Count);
				var cost = consol.GetApportionments(true).CostsCollection[0];
				Assert(cost.IsGatewayConsolCost);
				Assert(cost.E6_LocalCostAmountInfo.ReadOnly);
			}
		}

		public void TestE6_AT_TaxRate_ReadOnly_WhenIsGatewayConsolCost()
		{
			var gatewayAgentPort = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.First().PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(gatewayAgentPort);
			consol.JK_UniqueConsignRef = "C00001111";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			consol.Shipments.Add(shipment);
			using (var job = new Job.Loader(Factory, consol).TryCreateWithoutMutexForTestOnly())
			{
				var chargeCode = ObjectCreator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK);
				var charge = job.Charges.AddNew();
				charge.JR_AC = chargeCode.PK;
				charge.JR_OSSellAmt = 250m;
				charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
				charge.JR_JH_InternalJob = job.PK;
				charge.JR_GE_InternalDept = charge.JR_GE;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				Assert(consol.IsGateway());
				Factory.Save();
				AssertEquals(1, consol.GetApportionments(true).CostsCollection.Count);
				var cost = consol.GetApportionments(true).CostsCollection[0];
				Assert(cost.IsGatewayConsolCost);
				Assert(cost.IsTaxRateReadOnly);
			}
		}

		public void TestE6_OSGSTAmount_ReadOnly()
		{
			bool oldValue = AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
			try
			{
				UAInvoice invoice = Factory.NewWithValidTestData<UAInvoice>();
				invoice.AH_Ledger = "AP";
				invoice.AH_TransactionType = "INV";
				Factory.Save();
				AccTaxRate tax = ObjectCreator.GSTFREE1;
				tax.SetRateNumerator_ForTestOnly(100);
				var cost = Factory.NewWithValidTestData<JobConsolCost>();
				OrgHeader creditor = ObjectCreator.ABIGAS;
				creditor.CompanyData.SetAPTaxApplicable(true);
				cost.E6_OH_Creditor = creditor.PK;
				cost.ParentAPInvoice = invoice;
				cost.E6_AH_APInvoice = invoice.PK;
				cost.E6_AT_TaxRate = tax.PK;
				AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				Assert("OSGSTAmount must be NOT readonly because IsApprovingPosting is true", !cost.IsOSGSTAmountReadOnly);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, oldValue);
			}
		}

		public void TestE6_OSGSTAmount_ReadOnly_WhenIsGatewayConsolCost()
		{
			var gatewayAgentPort = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.First().PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(gatewayAgentPort);
			consol.JK_UniqueConsignRef = "C00001111";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			consol.Shipments.Add(shipment);
			using (var job = new Job.Loader(Factory, consol).TryCreateWithoutMutexForTestOnly())
			{
				var chargeCode = ObjectCreator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK);
				var charge = job.Charges.AddNew();
				charge.JR_AC = chargeCode.PK;
				charge.JR_OSSellAmt = 250m;
				charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
				charge.JR_JH_InternalJob = job.PK;
				charge.JR_GE_InternalDept = charge.JR_GE;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				Assert(consol.IsGateway());
				Factory.Save();
				AssertEquals(1, consol.GetApportionments(true).CostsCollection.Count);
				var cost = consol.GetApportionments(true).CostsCollection[0];
				Assert(cost.IsGatewayConsolCost);
				Assert(cost.IsOSGSTAmountReadOnly);
			}
		}

		public void TestTaxAmountCalculatedAsZeroWhenReverseTaxRate()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			AccTaxRate gstRevRate = GetGSTREVTRate();
			JobConsolCost cost = Factory.NewWithValidTestData<JobConsolCost>();
			OrgHeader creditor = creator.ABIGAS;
			cost.E6_AC_ChargeCode = creator.CC1.PK;
			cost.E6_OSCostAmount = 200m;
			creditor.CompanyData.SetAPTaxApplicable(true);
			cost.E6_OH_Creditor = creditor.PK;
			cost.E6_AT_TaxRate = gstRevRate.PK;
			AssertEquals(0m, cost.E6_OSGSTAmount_Calc);
		}

		AccTaxRate GetGSTREVTRate()
		{
			ZQuery gstRevRateQuery = new ZQuery(AccTaxRateSchema.AT_Type, AccTaxRate.Types.ReverseRated);
			gstRevRateQuery.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			AccTaxRate result = Factory.Load<AccTaxRate>(gstRevRateQuery).FirstOrDefault(x => x.GetRate_ForTestOnly() == 20);
			if (result == null)
			{
				result = Factory.New<AccTaxRate>();
				result.AT_Code = "GSTREV";
				result.AT_Type = AccTaxRate.Types.ReverseRated;
				result.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				result.SetRateNumerator_ForTestOnly(20);
			}

			return result;
		}

		public void TestReadOnly()
		{
			UAInvoice invoice = Factory.NewWithValidTestData<UAInvoice>();
			invoice.AH_Ledger = "AP";
			invoice.AH_TransactionType = "INV";
			Factory.Save();
			JobConsolCost cost = Factory.NewWithValidTestData<JobConsolCost>();
			cost.ParentAPInvoice = invoice;
			cost.E6_AH_APInvoice = invoice.PK;
			cost.ReadOnly = false;
			Assert("Cost must be NOT readonly because IsApprovingPosting is true", !cost.ReadOnly);
		}

		public void TestUpdateApportionmentMethod()
		{
			ApportionmentMethodFallBackSetting();

			var testObjectCreator = new TestObjectCreator(Factory);
			var invoiceUA = Factory.NewWithValidTestData<UAInvoice>();
			invoiceUA.AH_Ledger = "AP";
			invoiceUA.AH_TransactionType = "INV";

			var cost = AddNewConsolCostToInvoice(null, invoiceUA);
			cost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			AssertEquals("Pre-Condition", ZString.Empty, cost.E6_ApportionmentMethod);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertNotChangeWhenValidConsolAndChargeCode(cost, consol.PK, testObjectCreator.FRT.PK, AllocationMethod.Shipment, ZString.Empty);

			AssertNotChangeWhenInvalidConsol(cost, AllocationMethod.Shipment);

			consol.JK_TransportMode = Constants.TransportModes.Road;
			AssertNotChangeWhenValidConsolAndChargeCode(cost, consol.PK, testObjectCreator.FRT.PK, AllocationMethod.ContainerCount, AllocationMethod.Shipment);

			AssertNotChangeWhenInvalidChargeCode(cost, AllocationMethod.ContainerCount);

			void AssertNotChangeWhenValidConsolAndChargeCode(JobConsolCost testingJobConsolCost, ZGuid consolPK, ZGuid chargeCodePK, ZString expectedResult, ZString previousResult)
			{
				AssertEquals("PreCondition", previousResult, testingJobConsolCost.E6_ApportionmentMethod);

				testingJobConsolCost.E6_AC_ChargeCode = chargeCodePK;

				testingJobConsolCost.E6_ParentID = consolPK;
				testingJobConsolCost.E6_ParentTableCode = JobConsolSchema.Constants.Prefix;

				testingJobConsolCost.UpdateApportionmentMethod();
				AssertEquals("Value is updated since doing UpdateApportionmentMethod", expectedResult, testingJobConsolCost.E6_ApportionmentMethod);
			}

			void AssertNotChangeWhenInvalidConsol(JobConsolCost testingJobConsolCost, ZString previousValue)
			{
				AssertEquals("PreCondition", previousValue, testingJobConsolCost.E6_ApportionmentMethod);
				testingJobConsolCost.E6_ParentID = ZGuid.Empty;
				testingJobConsolCost.UpdateApportionmentMethod();
				AssertEquals("E6_ApportionmentMethod will not change since consol is invalid", previousValue, testingJobConsolCost.E6_ApportionmentMethod);
			}

			void AssertNotChangeWhenInvalidChargeCode(JobConsolCost testingJobConsolCost, ZString previousValue)
			{
				AssertEquals("PreCondition", previousValue, testingJobConsolCost.E6_ApportionmentMethod);
				testingJobConsolCost.E6_AC_ChargeCode = ZGuid.Empty;
				testingJobConsolCost.UpdateApportionmentMethod();
				AssertEquals("E6_ApportionmentMethod will not change since charge code is invalid", previousValue, testingJobConsolCost.E6_ApportionmentMethod);
			}
		}

		public void TestE6_ApportionmentMethod_FallBack()
		{
			ApportionmentMethodFallBackSetting();

			var testObjectCreator = new TestObjectCreator(Factory);

			var invoiceUA = Factory.NewWithValidTestData<UAInvoice>();
			invoiceUA.AH_Ledger = "AP";
			invoiceUA.AH_TransactionType = "INV";

			var cost = AddNewConsolCostToInvoice(null, invoiceUA);
			AssertEquals("Pre-Condition", ZString.Empty, cost.E6_ApportionmentMethod);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertNotChangeWhenValidConsolAndChargeCode(cost, consol.PK, testObjectCreator.FRT.PK, AllocationMethod.Shipment, ZString.Empty);

			AssertNotChangeWhenInvalidConsol(cost, AllocationMethod.Shipment);

			consol.JK_TransportMode = Constants.TransportModes.Road;
			AssertNotChangeWhenValidConsolAndChargeCode(cost, consol.PK, testObjectCreator.FRT.PK, AllocationMethod.ContainerCount, AllocationMethod.Shipment);

			AssertNotChangeWhenInvalidChargeCode(cost, AllocationMethod.ContainerCount);

			void AssertNotChangeWhenValidConsolAndChargeCode(JobConsolCost testingJobConsolCost, ZGuid consolPK, ZGuid chargeCodePK, ZString expectedResult, ZString previousResult)
			{
				AssertEquals("PreCondition", previousResult, testingJobConsolCost.E6_ApportionmentMethod);

				testingJobConsolCost.E6_AC_ChargeCode = chargeCodePK;

				testingJobConsolCost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consolPK, JobConsolSchema.Constants.Prefix);
				AssertEquals("Value should be updated", expectedResult, testingJobConsolCost.E6_ApportionmentMethod);
			}

			void AssertNotChangeWhenInvalidConsol(JobConsolCost testingJobConsolCost, ZString previousValue)
			{
				AssertEquals("PreCondition", previousValue, testingJobConsolCost.E6_ApportionmentMethod);
				testingJobConsolCost.SetE6_ParentIDAndE6_ParentTableCodeTogether(ZGuid.Empty, JobConsolSchema.Constants.Prefix);
				AssertEquals("E6_ApportionmentMethod will not change since consol is invalid", previousValue, testingJobConsolCost.E6_ApportionmentMethod);
			}

			void AssertNotChangeWhenInvalidChargeCode(JobConsolCost testingJobConsolCost, ZString previousValue)
			{
				AssertEquals("PreCondition", previousValue, testingJobConsolCost.E6_ApportionmentMethod);
				testingJobConsolCost.E6_AC_ChargeCode = ZGuid.Empty;
				AssertEquals("E6_ApportionmentMethod will not change since charge code is invalid", previousValue, testingJobConsolCost.E6_ApportionmentMethod);
			}
		}

		void ApportionmentMethodFallBackSetting()
		{
			var config = new ConsolCostDefaultApportionmentMethodConfiguration();
			config.ConsolCostDefaultApportionmentMethodCollection.RemoveAndDeleteAll();

			var specificMethod = new ConsolCostDefaultApportionmentMethod
			{
				Module = ApportionmentMethod.AllCode,
				TransportMode = Constants.TransportModes.Sea,
				ContainerMode = ApportionmentMethod.AllCode,
				Direction = ApportionmentMethod.AllCode,
				Apportionment = AllocationMethod.Shipment
			};
			config.ConsolCostDefaultApportionmentMethodCollection.Add(specificMethod);

			var generalMethod = new ConsolCostDefaultApportionmentMethod
			{
				Module = ApportionmentMethod.AllCode,
				TransportMode = Constants.TransportModes.Road,
				ContainerMode = ApportionmentMethod.AllCode,
				Direction = ApportionmentMethod.AllCode,
				Apportionment = AllocationMethod.ContainerCount
			};
			config.ConsolCostDefaultApportionmentMethodCollection.Add(generalMethod);

			AccountingConfigurationRegistry.Instance.ConsolCostDefaultApportionmentMethod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, config);
			Factory.Save();
		}

		public void TestE6_ApportionmentMethod_ReadOnly()
		{
			UAInvoice invoiceUA = Factory.NewWithValidTestData<UAInvoice>();
			invoiceUA.AH_Ledger = "AP";
			invoiceUA.AH_TransactionType = "INV";
			Factory.Save();
			var consol = Factory.New<ForwardingConsol>();
			var cost = AddNewConsolCostToInvoice(consol, invoiceUA);
			cost.E6_AH_APInvoice = invoiceUA.PK;
			AssertEquals("IsApprovingPosting", true, cost.IsApprovingPosting);
			AssertEquals("ReadOnly", true, cost.E6_ApportionmentMethodInfo.ReadOnly);
			consol.Shipments.AddNew();
			var apps = consol.GetApportionments();
			try
			{
				cost = apps.CostsCollection.TryAddNew();
				Assert(cost.CalculationStrategy is ConsolCostCalculationStrategy);
				AssertEquals("IsApprovingPosting", false, cost.IsApprovingPosting);
				Env.Security.MaintainConsolJobInvoicingEditMethod.IsAllowed = false;
				AssertEquals("ReadOnly", true, cost.E6_ApportionmentMethodInfo.ReadOnly);
				Env.Security.MaintainConsolJobInvoicingEditMethod.IsAllowed = true;
				AssertEquals("ReadOnly", false, cost.E6_ApportionmentMethodInfo.ReadOnly);
			}
			finally
			{
				apps.ReleaseMutexes();
			}

			var invoiceAP = Factory.NewWithValidTestData<APInvoice>();
			cost = AddNewConsolCostToInvoice(consol, invoiceAP);
			Assert(cost.CalculationStrategy is InvoicingBaseConsolCostCalculationStrategy);
			AssertEquals("IsApprovingPosting", false, cost.IsApprovingPosting);
			Env.Security.AllowAPInvoiceConsolCostApportionmentMethodOverride.IsAllowed = false;
			AssertEquals("ReadOnly", true, cost.E6_ApportionmentMethodInfo.ReadOnly);
			Env.Security.AllowAPInvoiceConsolCostApportionmentMethodOverride.IsAllowed = true;
			AssertEquals("ReadOnly", false, cost.E6_ApportionmentMethodInfo.ReadOnly);
		}

		public void TestHasSaveBeenRun()
		{
			JobConsolCost cost = (JobConsolCost)GetNewBusinessObject();
			Assert(!cost.HasSaveBeenRun);
			cost.PrepareForPosting();
			cost.OnFactorySaved_ForTestOnly(false);
			Assert(cost.HasSaveBeenRun);
			cost.OnFactorySaved_ForTestOnly(true);
			Assert(!cost.HasSaveBeenRun);
		}

		public void TestGSTPropagatedToSplitChargesWhenChargeCodeHasOverrides()
		{
			AccChargeCode chargeCode = ObjectCreator.CC1;
			chargeCode.AC_AT_GSTRate = ObjectCreator.GST1.PK;
			AccChargeTaxOverride taxOverride = chargeCode.TaxOverrides.AddNew();
			taxOverride.AO_CostSellAll = "ALL";
			taxOverride.AO_Direction = "ALL";
			taxOverride.AO_IncoTerm = "ALL";
			taxOverride.AO_JobType = "ALL";
			taxOverride.AO_TaxRegCntryOrGroup = "ALL";
			taxOverride.AO_AT = ObjectCreator.GSTFREE1.PK;
			taxOverride.AO_A9_DefaultVATClass = ObjectCreator.TaxMsg1.PK;
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			Factory.Save();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = chargeCode.PK;
				cost.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
				AssertEquals(taxOverride.AO_AT, cost.E6_AT_TaxRate);
				AssertEquals(taxOverride.AO_A9_DefaultVATClass, cost.E6_A9_VATClass);
				AssertEquals(taxOverride.AO_AT, cost.ApportionmentCharges[0].JR_AT_CostGSTRate);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestConsolCostE6_AC_ChargeCodeSetterSetsIncludedInProfitShareFlagOnCharges()
		{
			BusinessObjectFactory orgFactory = new BusinessObjectFactory();
			OrgHeader testOrganisation = orgFactory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignee = orgFactory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignor = orgFactory.NewWithValidTestData<OrgHeader>();
			OrgHeader sendingAgent = orgFactory.NewWithValidTestData<OrgHeader>();
			var chargeCode = orgFactory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			orgFactory.Save();
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(sendingAgent);
			Factory.Save();
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OH_DeliveryAgent = testOrganisation.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_TransportMode = "AIR";
			Assert("Precondition: Shipment is an export", shipment.IsExport());
			Job testJob = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			testJob.PlugInData = shipment;
			AssertNull("No profit share agreement found", testJob.ProfitShareAgreement);
			BusinessObjectFactory profitShareFactory = new BusinessObjectFactory();
			OrgAgentRelationship agentRelationship = profitShareFactory.New<OrgAgentRelationship>();
			agentRelationship.O3_OH_SendingAgent = sendingAgent.PK;
			agentRelationship.O3_OH_ReceivingAgent = testOrganisation.PK;
			OrgProfitShareDetails profitShareAgreement = agentRelationship.ProfitShareDetails.AddNew();
			profitShareAgreement.O4_FreightMode = "AIR";
			profitShareAgreement.O4_SendingPortOrCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			profitShareAgreement.O4_ReceivingPortOrCountry = "US";
			profitShareAgreement.O4_EndDate = ZDateTime.Today.AddDays(10);
			profitShareAgreement.O4_StartDate = ZDateTime.Today.AddDays(-10);
			profitShareFactory.Save();
			testJob = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			testJob.PlugInData = shipment;
			ApportionmentListing listing = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = listing.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = chargeCode.PK;
			cost.E6_OSCostAmount = 105m;
			AssertEquals("Charge.JR_AC", chargeCode.PK, cost.ApportionmentCharges[0].JR_AC);
			AssertEquals("Charge.JR_OSCostAmt", 105m, cost.ApportionmentCharges[0].JR_OSCostAmt);
			AssertEquals("Charge.JR_IsIncludedInProfitShare", true, cost.ApportionmentCharges[0].JR_IsIncludedInProfitShare);
			JobConsolCost cost2 = listing.CostsCollection.TryAddNew();
			using (cost2.GetSuspenderForConsolCostImporter())
			{
				cost2.E6_AC_ChargeCode = chargeCode.PK;
				cost2.E6_OSCostAmount = 205m;
			}

			AssertEquals("Charge.JR_AC", chargeCode.PK, cost2.ApportionmentCharges[0].JR_AC);
			AssertEquals("Charge.JR_OSCostAmt", 205m, cost2.ApportionmentCharges[0].JR_OSCostAmt);
			AssertEquals("Charge.JR_IsIncludedInProfitShare should not be set because it was suspended for ConsolCostImporter", false, cost2.ApportionmentCharges[0].JR_IsIncludedInProfitShare);
		}

		public void TestConsolCostingDoesntOverrideIncludedInProfitShareFlagOnShipment()
		{
			BusinessObjectFactory orgFactory = new BusinessObjectFactory();
			OrgHeader testOrganisation = orgFactory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignee = orgFactory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignor = orgFactory.NewWithValidTestData<OrgHeader>();
			OrgHeader sendingAgent = orgFactory.NewWithValidTestData<OrgHeader>();
			var freightChargeCode = orgFactory.NewWithValidTestData<AccChargeCode>();
			freightChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			orgFactory.Save();
			TestObjectCreator creator = new TestObjectCreator(Factory);
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(sendingAgent);
			Factory.Save();
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OH_DeliveryAgent = testOrganisation.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_TransportMode = "AIR";
			Assert("Precondition: Shipment is an export", shipment.IsExport());
			Job job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Charge charge = job.Charges.AddNew();
			charge.JR_AC = creator.CC1.PK;
			charge.JR_OSCostAmt = 100m;
			charge.JR_OSSellAmt = 120m;
			charge.JR_IsIncludedInProfitShare = true;
			charge.JR_AgentDeclaredCostAmt = 110m;
			charge.JR_AgentDeclaredSellAmt = 115m;
			Factory.Save();
			ApportionmentListing listing = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = listing.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = creator.CC1.PK;
			cost.E6_OSCostAmount = 105m;
			cost.ApportionmentCharges[0].JR_GB = GlbBranch.CurrentBranch.PK;
			cost.ApportionmentCharges[0].JR_GE = GlbDepartment.CurrentDepartment.PK;
			AssertEquals(105m, cost.ApportionmentCharges[0].JR_OSCostAmt);
			cost.PrepareForPosting();
			Factory.Save();
			Charge reloadedCharge = Factory.Load<Charge>(charge.PK);
			Assert(reloadedCharge.JR_IsIncludedInProfitShare);
			AssertEquals(110m, reloadedCharge.JR_AgentDeclaredCostAmt);
			AssertEquals(115m, reloadedCharge.JR_AgentDeclaredSellAmt);
			// Add Profit Share Agreement
			BusinessObjectFactory profitShareFactory = new BusinessObjectFactory();
			OrgAgentRelationship agentRelationship = profitShareFactory.New<OrgAgentRelationship>();
			agentRelationship.O3_OH_SendingAgent = sendingAgent.PK;
			agentRelationship.O3_OH_ReceivingAgent = testOrganisation.PK;
			OrgProfitShareDetails profitShareAgreement = agentRelationship.ProfitShareDetails.AddNew();
			profitShareAgreement.O4_FreightMode = "AIR";
			profitShareAgreement.O4_SendingPortOrCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			profitShareAgreement.O4_ReceivingPortOrCountry = "US";
			profitShareAgreement.O4_EndDate = ZDateTime.Today.AddDays(10);
			profitShareAgreement.O4_StartDate = ZDateTime.Today.AddDays(-10);
			profitShareFactory.Save();
			charge = job.Charges.AddNew();
			charge.JR_AC = freightChargeCode.PK;
			charge.JR_OSCostAmt = 100m;
			charge.JR_OSSellAmt = 120m;
			AssertEquals("Pre-condition: Should be included in Profit Share", true, charge.JR_IsIncludedInProfitShare);
			// Exclude from Profit Share
			charge.JR_IsIncludedInProfitShare = false;
			charge.JR_AgentDeclaredCostAmt = 0m;
			charge.JR_AgentDeclaredSellAmt = 0m;
			Factory.Save();
			cost = listing.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = freightChargeCode.PK;
			cost.E6_OSCostAmount = 105m;
			AssertEquals(1, cost.ApportionmentCharges.Count);
			AssertEquals("Charge.JR_AC", freightChargeCode.PK, cost.ApportionmentCharges[0].JR_AC);
			AssertEquals("Charge.JR_OSCostAmt", 105m, cost.ApportionmentCharges[0].JR_OSCostAmt);
			AssertEquals("Charge.JR_IsIncludedInProfitShare", true, cost.ApportionmentCharges[0].JR_IsIncludedInProfitShare);
			cost.PrepareForPosting();
			Factory.Save();
			reloadedCharge = Factory.Load<Charge>(charge.PK);
			AssertEquals("Should not be overriden", false, reloadedCharge.JR_IsIncludedInProfitShare);
			AssertEquals(105m, reloadedCharge.JR_AgentDeclaredCostAmt);
			AssertEquals(0m, reloadedCharge.JR_AgentDeclaredSellAmt);
		}

		public void TestConsolCostSavingDoesNotOverridePostedJobChargeSellAccount()
		{
			OrgHeader debtor = ObjectCreator.CreateOrgHeader("TST", true, true);
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_ParentTableCode = "JS";
			job.JH_ParentID = shipment.PK;
			job.LocalChargesPK = ObjectCreator.AALSHI.PK;
			job.AgentCollectPK = ObjectCreator.ABIGAS.PK;
			JobCharge charge = job.Charges.AddNew();
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_OSSellAmt = 100;
			charge.JR_OH_SellAccount = debtor.PK;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = ObjectCreator.FESDepartment.PK;
			InvoicingPostManager poster = new InvoicingPostManager(job);
			poster.CreateTransactions(JobInvoicingPostingOption.Revenue);
			Assert("Charge revenue should be posted", charge.IsRevenuePosted);
			Factory.Save();
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			Factory.Save();
			consol.Shipments.Add(shipment);
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost.E6_OSCostAmount = 200M;
			cost.ApportionmentCharges[0].JR_GB = GlbBranch.CurrentBranch.PK;
			cost.ApportionmentCharges[0].JR_GE = ObjectCreator.FESDepartment.PK;
			cost.ApportionmentCharges[0].JR_OH_SellAccount = ObjectCreator.ABIGAS.PK;
			AssertEquals("Existing charge sell account must not be empty.", false, charge.JR_OH_SellAccount.IsEmpty);
			AssertEquals("Existing charge must be in db.", true, charge.IsInDatabase);
			Factory.Save();
			AssertEquals("Precondition: there are no new charges created.", 1, job.Charges.Count);
			AssertEquals("Precondition: existed charge linked to consol.", cost.PK, charge.JR_E6);
			AssertEquals("Charge Sell account should not changed", debtor.PK, charge.JR_OH_SellAccount);
		}

		public void TestConsolCostSavingDoesNotOverrideSellAccountIfItsAlreadySet()
		{
			OrgHeader debtor = ObjectCreator.CreateOrgHeader("TST", true, true);
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_ParentTableCode = "JS";
			job.JH_ParentID = shipment.PK;
			job.LocalChargesPK = ObjectCreator.AALSHI.PK;
			job.AgentCollectPK = ObjectCreator.ABIGAS.PK;
			Factory.Save();
			JobCharge charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_OSSellAmt = 100;
			charge.JR_OH_SellAccount = debtor.PK;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			Factory.Save();
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			Factory.Save();
			consol.Shipments.Add(shipment);
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost.E6_OSCostAmount = 200M;
			cost.ApportionmentCharges[0].JR_OH_SellAccount = ObjectCreator.ABIGAS.PK;
			AssertEquals("Existing charge sell account must not be empty.", false, charge.JR_OH_SellAccount.IsEmpty);
			AssertEquals("Existing charge must be in db.", true, charge.IsInDatabase);
			Factory.Save();
			AssertEquals("Precondition: there are no new charges created.", 1, job.Charges.Count);
			AssertEquals("Precondition: existed charge linked to consol.", cost.PK, charge.JR_E6);
			AssertEquals("Charge Sell account should not changed", debtor.PK, charge.JR_OH_SellAccount);
		}

		public void TestConsolCostSavingDoOverrideSellAccountIfItsEmpty()
		{
			OrgHeader debtor = ObjectCreator.CreateOrgHeader("TST", true, true);
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_ParentTableCode = "JS";
			job.JH_ParentID = shipment.PK;
			job.LocalChargesPK = ObjectCreator.AALSHI.PK;
			job.AgentCollectPK = ObjectCreator.ABIGAS.PK;
			Factory.Save();
			JobCharge charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_OSSellAmt = 100;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_OH_SellAccount = ZGuid.Empty;
			Factory.Save();
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			Factory.Save();
			consol.Shipments.Add(shipment);
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost.E6_OSCostAmount = 200M;
			cost.ApportionmentCharges[0].JR_OH_SellAccount = debtor.PK;
			AssertEquals("Existing charge sell account must be empty.", true, charge.JR_OH_SellAccount.IsEmpty);
			AssertEquals("Existing charge must be in db.", true, charge.IsInDatabase);
			Factory.Save();
			AssertEquals("Precondition: there are no new charges created.", 1, job.Charges.Count);
			AssertEquals("Precondition: existed charge linked to consol.", cost.PK, charge.JR_E6);
			AssertEquals("Charge Sell account should not changed", debtor.PK, charge.JR_OH_SellAccount);
		}

		public void TestConsolCostSavedAndSetE6_AC_ChargeCodeWithRevenuePostedShouldNotReportError()
		{
			var debtor = ObjectCreator.CreateOrgHeader("TST", true, true);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_ParentTableCode = "JS";
			job.JH_ParentID = shipment.PK;
			job.LocalChargesPK = ObjectCreator.AALSHI.PK;
			job.AgentCollectPK = ObjectCreator.ABIGAS.PK;
			var charge = job.Charges.AddNew();
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_OSSellAmt = 100;
			charge.JR_OH_SellAccount = debtor.PK;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = ObjectCreator.FESDepartment.PK;
			var poster = new InvoicingPostManager(job);
			poster.CreateTransactions(JobInvoicingPostingOption.Revenue);
			Assert("Charge revenue should be posted", charge.IsRevenuePosted);
			Factory.Save();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			Factory.Save();
			consol.Shipments.Add(shipment);
			var apps = new ApportionmentListing(Factory, consol);
			var cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost.E6_OSCostAmount = 200M;
			cost.ApportionmentCharges[0].JR_GB = GlbBranch.CurrentBranch.PK;
			cost.ApportionmentCharges[0].JR_GE = ObjectCreator.FESDepartment.PK;
			cost.ApportionmentCharges[0].JR_OH_SellAccount = ObjectCreator.ABIGAS.PK;
			AssertEquals("Existing charge sell account must not be empty.", false, charge.JR_OH_SellAccount.IsEmpty);
			AssertEquals("Existing charge must be in db.", true, charge.IsInDatabase);
			Factory.Save();
			AssertEquals("Precondition: there are no new charges created.", 1, job.Charges.Count);
			AssertEquals("Precondition: existed charge linked to consol.", cost.PK, charge.JR_E6);
			AssertEquals("Charge Sell account should not changed", debtor.PK, charge.JR_OH_SellAccount);
			ErrorReporter.Clear();
			cost.E6_AC_ChargeCode = ZGuid.Invalid;
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			AssertHasError(cost.E6_AC_ChargeCodeInfo, string.Format("The revenue on one or more of the apportioned charges is flagged as posted. You cannot change the charge code on this consol cost. Please change the value back to the original value of '{0}'.", charge.ChargeCode.AC_Code));
			ErrorReporter.Clear();
			cost.E6_AC_ChargeCode = ObjectCreator.DSBChargeCode.PK;
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			AssertHasError(cost.E6_AC_ChargeCodeInfo, string.Format("The revenue on one or more of the apportioned charges is flagged as posted. You cannot change the charge code on this consol cost. Please change the value back to the original value of '{0}'.", charge.ChargeCode.AC_Code));
			ErrorReporter.Clear();
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			AssertNoErrors(cost.E6_AC_ChargeCodeInfo);
			ErrorReporter.Clear();
		}

		public void TestPostConsolCostWithSameChargeCodeShouldNotReportError()
		{
			using (AccountingConfigurationRegistry.Instance.CreateWIPWhenCostIsGreaterThanAccrual.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var fRT = ObjectCreator.FRT;
				var creditor = ObjectCreator.AALSHI;
				var taxRate = ObjectCreator.CreateTaxRate("GSTTest", "GST Test 1", AccTaxRate.Types.Rated, 19, string.Empty, 0, 1);
				var consol = ObjectCreator.CreateConsol("AUSYD", "NZAKL", "C0001");
				consol.Shipments.AddNew();
				consol.Shipments.AddNew();
				var appListing = new ApportionmentListing(Factory, consol);
				var consolCost1 = ObjectCreator.CreateConsolCost(consol, fRT, 0.09m, null, "SHP", appListing);
				consolCost1.E6_ExchangeRate = 1m;
				consolCost1.ApportionmentCharges[0].JR_OSCostAmt = 0.09m;
				consolCost1.ApportionmentCharges[1].JR_OSCostAmt = 0m;
				var consolCost2 = ObjectCreator.CreateConsolCost(consol, fRT, 770m, creditor, "SHP", appListing);
				consolCost2.E6_ExchangeRate = 1m;
				consolCost2.E6_AT_TaxRate = taxRate.PK;
				consolCost2.ApportionmentCharges[0].JR_OSCostAmt = 128.93m;
				consolCost2.ApportionmentCharges[1].JR_OSCostAmt = 641.07m;
				Factory.Save();
				var invoice = Factory.NewWithValidTestData<APInvoice>();
				invoice.AH_OH = creditor.PK;
				invoice.AH_RX_NKTransactionCurrency = "AUD";
				invoice.AH_ExchangeRate = 1m;
				invoice.SubmittedFromInvoicingForm = true;
				var invoiceConsolCost1 = ObjectCreator.CreateConsolCost(invoice, consol, fRT, 0.1, creditor);
				invoiceConsolCost1.RelatedConsolCostPK = consolCost1.PK; //simulate import window
				invoiceConsolCost1.E6_AT_TaxRate = taxRate.PK;
				invoiceConsolCost1.IsFinal = false;
				invoiceConsolCost1.ApportionmentCharges[0].JR_OSCostAmt = 0.1m;
				invoiceConsolCost1.ApportionmentCharges[1].JR_OSCostAmt = 0m;
				invoice.ImportAllApportionmentsFromCosting();
				Factory.Save();
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			}
		}

		public void TestPrepareForPostingLooksAtCharges_NotInDatabase()
		{
			JobConsolCost cost = (JobConsolCost)GetNewBusinessObject();
			cost.E6_GC = GlbCompany.CurrentCompany.PK;
			ApportionSplitCharge charge = cost.ApportionmentCharges.AddNew();
			Job job = Factory.NewJobForTesting<Job>();
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost.E6_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge.JR_JH = job.PK;
			charge.JR_IsIncludedInProfitShare = true;
			JobCharge charge2 = job.Charges.AddNew();
			charge2.JR_AC = Env.Registry.FreightChargeCode;
			charge2.JR_GB = GlbBranch.CurrentBranch.PK;
			charge2.JR_GE = GlbDepartment.CurrentDepartment.PK;
			cost.PrepareForPosting();
			AssertEquals(1, cost.ApportionmentCharges.Count);
			AssertNotEquals(charge.PK, cost.ApportionmentCharges[0].PK);
			//The next 2 assertions were negated because PrepareForPosting() originally excluded charges not in the database.
			//An investigation by the Product Manager (IK), Accounting Development Team Leader (BK) and developer (LDH) could not ascertain the purpose of this original behaviour.
			//Functional testing did not identify any problems introduced by this change.
			AssertEquals(charge2.PK, cost.ApportionmentCharges[0].PK);
			Assert(charge2.JR_IsIncludedInProfitShare);
		}

		public void TestPrepareForPostingDisbursementCharge_DescriptionAndRatingLogAuditAreCopied()
		{
			//Arrange
			var taxRate = ObjectCreator.CreateTaxRate("GSTFREE", "GST Free", 10);
			var disbursementChargeCode = ObjectCreator.CreateChargeCode("DSB", "Description", Constants.ChargeType.Disbursement, 100.00m, taxRate, null, "ALL");

			var consol = ObjectCreator.CreateConsol("AUSYD", "NZAKL", "C0001");
			var shipment = ObjectCreator.CreateShipment("S0001", consol);
			var job = ObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var cost = ObjectCreator.CreateConsolCost(consol, disbursementChargeCode, 100);
			var message = "Pre-condition: cost should be apportioned to the only shipment by default";
			AssertEquals(message, 1, cost.ApportionmentCharges.Count);

			var charge = cost.ApportionmentCharges[0];
			charge.JR_AC = disbursementChargeCode.PK;
			charge.JR_Desc = "COST DESCRIPTION";
			var calculationDescription = "Disbursement Charge Calculation Description";
			charge.CostCalculationDescription = ZBlob.FromAscii(calculationDescription);

			var charge2 = job.Charges.AddNew();
			charge2.JR_AC = disbursementChargeCode.PK;
			charge2.JR_Desc = "REVENUE DESCRIPTION";
			charge2.RevenueCalculationDescription = ZBlob.FromAscii("REVENUE DESCRIPTION");

			//Act
			cost.PrepareForPosting();

			//Assert
			AssertEquals(1, cost.ApportionmentCharges.Count);

			var apportionedCharge = cost.ApportionmentCharges[0];
			AssertEquals(charge2.PK, apportionedCharge.PK);
			AssertEquals("COST DESCRIPTION", apportionedCharge.JR_Desc);
			AssertEquals(calculationDescription, apportionedCharge.CostCalculationDescription.ToAscii());
			AssertEquals(calculationDescription, apportionedCharge.RevenueCalculationDescription.ToAscii());
			Assert(!apportionedCharge.JR_CostRatingOverride);
		}

		public void TestPrepareForPostingDisbursementCharge_DescriptionAndRatingLogAuditAreCopiedOnNewCharge()
		{
			//Arrange
			var taxRate = ObjectCreator.CreateTaxRate("GSTFREE", "GST Free", 10);
			var disbursementChargeCode = ObjectCreator.CreateChargeCode("DSB", "Description", Constants.ChargeType.Disbursement, 100.00m, taxRate, null, "ALL");

			var consol = ObjectCreator.CreateConsol("AUSYD", "NZAKL", "C0001");
			var shipment = ObjectCreator.CreateShipment("S0001", consol);
			var job = ObjectCreator.CreateJob(shipment, false);
			Factory.Save();
			var cost = ObjectCreator.CreateConsolCost(consol, disbursementChargeCode, 100);
			var message = "Pre-condition: cost should be apportioned to the only shipment by default";
			AssertEquals(message, 1, cost.ApportionmentCharges.Count);

			var charge = cost.ApportionmentCharges[0];
			charge.JR_AC = disbursementChargeCode.PK;
			charge.JR_Desc = "COST DESCRIPTION";
			var calculationDescription = "Disbursement Charge Calculation Description";
			charge.CostCalculationDescription = ZBlob.FromAscii(calculationDescription);

			//Act
			cost.PrepareForPosting();

			//Assert
			AssertEquals(1, cost.ApportionmentCharges.Count);

			var apportionedCharge = cost.ApportionmentCharges[0];
			AssertNotEquals(charge.PK, apportionedCharge.PK);
			AssertEquals("COST DESCRIPTION", apportionedCharge.JR_Desc);
			AssertEquals(calculationDescription, apportionedCharge.CostCalculationDescription.ToAscii());
			AssertEquals(calculationDescription, apportionedCharge.RevenueCalculationDescription.ToAscii());
			Assert(!apportionedCharge.JR_CostRatingOverride);
		}

		public void TestPrepareForPosting_SimilarCharges_ShouldCompareAttributesToGetBestMatchedOne()
		{
			var freightChargeCode = Env.Registry.FreightChargeCode;

			var cost = (JobConsolCost)GetNewBusinessObject();
			cost.E6_GC = GlbCompany.CurrentCompany.PK;
			cost.E6_AC_ChargeCode = freightChargeCode;
			cost.E6_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			cost.Attributes.Add(JobChargeAttribTypeList.Codes.ContainerCode, "20GP");

			var shipment = Factory.New<CommonShipment>();
			var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			job.PlugInData = shipment;
			job.JH_GC = GlbCompany.CurrentCompany.PK;

			// apportioned charge
			var charge = cost.ApportionmentCharges.AddNew();
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge.JR_JH = job.PK;
			charge.JR_AC = freightChargeCode;
			var chargeAttrib = charge.JobChargeAttributes.AddNew();
			chargeAttrib.EC_Name = JobChargeAttribTypeList.Codes.ContainerCode;
			chargeAttrib.EC_Value = "20GP";

			JobCharge existingCharge1 = job.Charges.AddNew();
			existingCharge1.JR_AC = freightChargeCode;
			existingCharge1.JR_GB = GlbBranch.CurrentBranch.PK;
			existingCharge1.JR_GE = GlbDepartment.CurrentDepartment.PK;

			// comparable attribute
			var jobChargeAttrib11 = existingCharge1.JobChargeAttributes.AddNew();
			jobChargeAttrib11.EC_Name = JobChargeAttribTypeList.Codes.ContainerCode;
			jobChargeAttrib11.EC_Value = "40GP";
			// other attribute
			var jobChargeAttrib12 = existingCharge1.JobChargeAttributes.AddNew();
			jobChargeAttrib12.EC_Name = JobChargeAttribTypeList.Codes.ContainerNumber;
			jobChargeAttrib12.EC_Value = "1111";

			JobCharge existingCharge2 = job.Charges.AddNew();
			existingCharge2.JR_AC = freightChargeCode;
			existingCharge2.JR_GB = GlbBranch.CurrentBranch.PK;
			existingCharge2.JR_GE = GlbDepartment.CurrentDepartment.PK;

			var jobChargeAttrib21 = existingCharge2.JobChargeAttributes.AddNew();
			jobChargeAttrib21.EC_Name = JobChargeAttribTypeList.Codes.ContainerCode;
			jobChargeAttrib21.EC_Value = "20GP";
			var jobChargeAttrib22 = existingCharge2.JobChargeAttributes.AddNew();
			jobChargeAttrib22.EC_Name = JobChargeAttribTypeList.Codes.ContainerNumber;
			jobChargeAttrib22.EC_Value = "2222";

			cost.PrepareForPosting();
			AssertEquals("Charge 2 should be the best matched one", existingCharge2.PK, cost.ApportionmentCharges[0].PK);

			// swap attribute values
			jobChargeAttrib11.EC_Value = "20GP";
			jobChargeAttrib21.EC_Value = "40GP";

			cost.HasSaveBeenRun = false;
			cost.PrepareForPosting();
			AssertEquals("Charge 1 should be the best matched one", existingCharge1.PK, cost.ApportionmentCharges[0].PK);
		}

		public void TestPrepareForPosting_SimilarCharges_WhenCannotCompareCompareAttributes_ShouldPickTheTheMinPK()
		{
			var freightChargeCode = Env.Registry.FreightChargeCode;

			var cost = (JobConsolCost)GetNewBusinessObject();
			cost.E6_GC = GlbCompany.CurrentCompany.PK;
			cost.E6_AC_ChargeCode = freightChargeCode;
			cost.E6_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			cost.Attributes.Add(JobChargeAttribTypeList.Codes.ContainerCode, "20GP");
			cost.Attributes.Add(JobChargeAttribTypeList.Codes.ContainerNumber, "ABC");

			var shipment = Factory.New<CommonShipment>();
			var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			job.PlugInData = shipment;
			job.JH_GC = GlbCompany.CurrentCompany.PK;

			var charge = cost.ApportionmentCharges.AddNew();
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge.JR_JH = job.PK;

			JobCharge existingCharge1 = job.Charges.AddNew();
			existingCharge1.JR_AC = freightChargeCode;
			existingCharge1.JR_GB = GlbBranch.CurrentBranch.PK;
			existingCharge1.JR_GE = GlbDepartment.CurrentDepartment.PK;

			JobCharge existingCharge2 = job.Charges.AddNew();
			existingCharge2.JR_AC = freightChargeCode;
			existingCharge2.JR_GB = GlbBranch.CurrentBranch.PK;
			existingCharge2.JR_GE = GlbDepartment.CurrentDepartment.PK;

			// an incomparable attribute presents but it should not dictate charge picking
			var jobChargeAttrib22 = existingCharge2.JobChargeAttributes.AddNew();
			jobChargeAttrib22.EC_Name = JobChargeAttribTypeList.Codes.ContainerNumber;
			jobChargeAttrib22.EC_Value = "ABC";

			cost.PrepareForPosting();

			var expectedCharge = job.Charges.MinBy(x => x.PK);
			AssertEquals(expectedCharge.PK, cost.ApportionmentCharges[0].PK);
		}

		public void TestDBHitCountForUpdateChargeDebtorIfMatchedChargeFound()
		{
			var job1 = CreateJobWithCharge(ObjectCreator.ABIGAS);
			var job2 = CreateJobWithCharge(ObjectCreator.Debtor);
			var job3 = CreateJobWithCharge(ObjectCreator.AALSHI);
			Factory.Save();
			var job4 = CreateJobWithCharge(ObjectCreator.ActiveOrg);
			JobConsolCost cost = (JobConsolCost)GetNewBusinessObject();
			cost.E6_GC = GlbCompany.CurrentCompany.PK;
			var apCharge1 = CreateApportionmentCharge(cost, job1);
			var apCharge2 = CreateApportionmentCharge(cost, job2);
			var apCharge3 = CreateApportionmentCharge(cost, job3);
			var apCharge4 = CreateApportionmentCharge(cost, job4);
			AssertCollectionContains(apCharge1, cost.ApportionmentCharges);
			AssertCollectionContains(apCharge2, cost.ApportionmentCharges);
			AssertCollectionContains(apCharge3, cost.ApportionmentCharges);
			AssertCollectionContains(apCharge4, cost.ApportionmentCharges);
			AssertEquals("Precondition: should be 4 charges", 4, cost.ApportionmentCharges.Count);
			AssertEquals(ZGuid.Empty, apCharge1.JR_OH_SellAccount);
			AssertEquals(ZGuid.Empty, apCharge2.JR_OH_SellAccount);
			AssertEquals(ZGuid.Empty, apCharge3.JR_OH_SellAccount);
			AssertEquals(ZGuid.Empty, apCharge4.JR_OH_SellAccount);
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			var dbHitCountAfterUpdatingDebtor = Factory.GetTableHitCount(JobChargeSchema.Constants.TableName);
			AssertEquals("Expected Number of DB Hit", 0, dbHitCountAfterUpdatingDebtor);
			AssertEquals("Debtor is not set when cost is populated. It was set before to solve WIP must have Debtor logic. Now this logic is simplified and debtor setting on cost changes is not required.", ZGuid.Empty, apCharge1.JR_OH_SellAccount);
			AssertEquals("Debtor is not set when cost is populated. It was set before to solve WIP must have Debtor logic. Now this logic is simplified and debtor setting on cost changes is not required.", ZGuid.Empty, apCharge2.JR_OH_SellAccount);
			AssertEquals("Debtor is not set when cost is populated. It was set before to solve WIP must have Debtor logic. Now this logic is simplified and debtor setting on cost changes is not required.", ZGuid.Empty, apCharge3.JR_OH_SellAccount);
			AssertEquals("Debtor is not set when cost is populated. It was set before to solve WIP must have Debtor logic. Now this logic is simplified and debtor setting on cost changes is not required.", ZGuid.Empty, apCharge4.JR_OH_SellAccount);
		}

		Job CreateJobWithCharge(OrgHeader org)
		{
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			JobCharge charge = job.Charges.AddNew();
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge.JR_OH_SellAccount = org.PK;
			return job;
		}

		ApportionSplitCharge CreateApportionmentCharge(JobConsolCost cost, Job job)
		{
			ApportionSplitCharge charge = cost.ApportionmentCharges.AddNew();
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge.JR_JH = job.PK;
			charge.JR_IsIncludedInProfitShare = true;
			return charge;
		}

		public void TestPrepareForPostingLooksAtCharges_InDatabase()
		{
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			JobCharge charge2 = job.Charges.AddNew();
			charge2.JR_GB = GlbBranch.CurrentBranch.PK;
			charge2.JR_AC = Env.Registry.FreightChargeCode;
			charge2.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge2.JR_IsIncludedInProfitShare = true;
			Factory.Save();
			JobConsolCost cost = (JobConsolCost)GetNewBusinessObject();
			cost.E6_GC = GlbCompany.CurrentCompany.PK;
			ApportionSplitCharge charge = cost.ApportionmentCharges.AddNew();
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost.E6_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge.JR_JH = job.PK;
			charge.JR_IsIncludedInProfitShare = false;
			cost.PrepareForPosting();
			AssertEquals(1, cost.ApportionmentCharges.Count);
			AssertEquals(charge2.PK, cost.ApportionmentCharges[0].PK);
			AssertEquals(true, cost.ApportionmentCharges[0].JR_IsIncludedInProfitShare);
		}

		public void TestPrepareForPostingDoesntRunForIncompleteInvoice()
		{
			var consol = ObjectCreator.CreateConsol("AUSYD", "NZAKL", "C0001");
			var shipment = ObjectCreator.CreateShipment("S0001", consol);
			var job = ObjectCreator.CreateJob(shipment, false);
			var chargeInDb = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 10, 10);
			chargeInDb.JR_IsIncludedInProfitShare = true;
			Factory.Save();
			var cost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, 100, apportionmentMethod: AllocationMethod.Shipment);
			var charge = cost.ApportionmentCharges[0];
			charge.JR_IsIncludedInProfitShare = false;
			var invoice = ObjectCreator.CreateInvoice(typeof(APInvoice), "inv1");
			ObjectCreator.CreateInvoiceLine(invoice, 100);
			invoice.SaveAsIncomplete();
			Factory.Save();
			AssertEquals(1, cost.ApportionmentCharges.Count);
			AssertNotEquals(chargeInDb.PK, cost.ApportionmentCharges[0].PK);
			AssertEquals(false, cost.ApportionmentCharges[0].JR_IsIncludedInProfitShare);
			invoice.MoveFromIncompleteToPayableLedger();
			Factory.Save();
			AssertEquals(1, cost.ApportionmentCharges.Count);
			AssertEquals(chargeInDb.PK, cost.ApportionmentCharges[0].PK);
			AssertEquals(true, cost.ApportionmentCharges[0].JR_IsIncludedInProfitShare);
		}

		public void TestPrepareForPostingCopiesEstimatedCost()
		{
			var consol = ObjectCreator.CreateConsol("AUSYD", "NZAKL", "C0001");
			var shipment1 = ObjectCreator.CreateShipment("S0001", consol);
			var shipment2 = ObjectCreator.CreateShipment("S0002", consol);
			var job1 = ObjectCreator.CreateJob(shipment1, false);
			var job2 = ObjectCreator.CreateJob(shipment2, false);
			var chargeInDb = ObjectCreator.CreateCharge(job1, ObjectCreator.CC1, 10, 10);
			Factory.Save();
			var cost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, 100, apportionmentMethod: AllocationMethod.Shipment);
			AssertEquals(2, cost.ApportionmentCharges.Count);
			var charge1 = cost.ApportionmentCharges[0];
			var expectedJR_EstimatedCost1 = 123m;
			charge1.JR_EstimatedCost = expectedJR_EstimatedCost1;
			var charge2 = cost.ApportionmentCharges[1];
			var expectedJR_EstimatedCost2 = 321m;
			charge2.JR_EstimatedCost = expectedJR_EstimatedCost2;
			cost.PrepareForPosting();
			AssertEquals(2, cost.ApportionmentCharges.Count);
			AssertEquals(chargeInDb.PK, cost.ApportionmentCharges[0].PK);
			AssertEquals(expectedJR_EstimatedCost1, cost.ApportionmentCharges[0].JR_EstimatedCost);
			AssertNotEquals(charge2.PK, cost.ApportionmentCharges[1].PK);
			AssertEquals(expectedJR_EstimatedCost2, cost.ApportionmentCharges[1].JR_EstimatedCost);
		}

		public void TestPrepareForPosting_JobInternalInformation()
		{
			var newOrgProxy = ObjectCreator.CreateOrgHeader("TSTORG", true, true);
			var newCompany = ObjectCreator.CreateNewCompany("ZZZ");
			var newBranch = ObjectCreator.CreateBranch("ZZZ", "Branch ZZZ", newCompany, newOrgProxy);
			Factory.Save();

			using (newBranch.SetAsTemporaryContext())
			using (var job1 = Factory.NewJobWithValidTestDataForTesting<Job>())
			using (var job2 = Factory.NewJobWithValidTestDataForTesting<Job>())
			using (var job3 = Factory.NewJobWithValidTestDataForTesting<Job>())
			{
				var charge1 = job2.Charges.AddNew();
				var charge2 = job3.Charges.AddNew();
				charge1.JR_AC = charge2.JR_AC = ObjectCreator.FRT.PK;
				charge1.JR_GB = charge2.JR_GB = GlbBranch.CurrentBranch.PK;
				charge1.JR_GE = charge2.JR_GE = GlbDepartment.CurrentDepartment.PK;
				AssertEquals(true, !charge1.IsCostPosted && !charge1.IsRevenuePostedWithManualJobRevenueJournal);
				AssertEquals(true, !charge2.IsCostPosted && !charge2.IsRevenuePostedWithManualJobRevenueJournal);

				charge1.JR_OH_SellAccount = newBranch.OrgProxy.PK;
				AssertEquals(false, charge1.IsInternalJobInfoDisabled);
				AssertEquals(true, charge2.IsInternalJobInfoDisabled);
				AssertEquals(1, job2.Charges.Count);
				AssertEquals(1, job3.Charges.Count);
				Factory.Save();

				var cost = CreateConsolCostForJobAndCreditor(job1.PK, ObjectCreator.Creditor2.PK);
				var apportionSplitCharge1 = cost.ApportionmentCharges[0];
				var apportionSplitCharge2 = cost.ApportionmentCharges.AddNew();
				apportionSplitCharge1.JR_AC = apportionSplitCharge2.JR_AC = ObjectCreator.FRT.PK;
				apportionSplitCharge1.JR_JH_InternalJob = apportionSplitCharge2.JR_JH_InternalJob = job1.PK;
				apportionSplitCharge1.JR_GE_InternalDept = apportionSplitCharge2.JR_GE_InternalDept = job1.JH_GE;
				apportionSplitCharge1.JR_GB_InternalBranch = apportionSplitCharge2.JR_GB_InternalBranch = job1.JH_GB;
				apportionSplitCharge1.JR_GB = apportionSplitCharge2.JR_GB = GlbBranch.CurrentBranch.PK;
				apportionSplitCharge1.JR_GE = apportionSplitCharge2.JR_GE = GlbDepartment.CurrentDepartment.PK;
				apportionSplitCharge1.JR_JH = job2.PK;
				apportionSplitCharge2.JR_JH = job3.PK;

				cost.PrepareForPosting();
				AssertEquals(1, job2.Charges.Count);
				AssertEquals(1, job3.Charges.Count);
				AssertEquals(job1.PK, job2.Charges[0].JR_JH_InternalJob);
				AssertEquals(job1.JH_GE, job2.Charges[0].JR_GE_InternalDept);
				AssertEquals(job1.JH_GB, job2.Charges[0].JR_GB_InternalBranch);
				AssertEquals(ZGuid.Empty, job3.Charges[0].JR_JH_InternalJob);
				AssertEquals(ZGuid.Empty, job3.Charges[0].JR_GE_InternalDept);
				AssertEquals(ZGuid.Empty, job3.Charges[0].JR_GB_InternalBranch);
			}
		}

		public void TestPrepareForPosting_JR_E6_GatewaySellHeader()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			using (Job job = Factory.NewJobWithValidTestDataForTesting<Job>())
			{
				JobConsolCost cost = CreateConsolCostForJobAndCreditor(job.PK, testObjectCreator.Creditor2.PK);
				var apportionSplitCharge = cost.ApportionmentCharges[0];
				apportionSplitCharge.JR_JH = job.PK;
				var gatewaySellHeaderPK = ZGuid.NewZGuid();
				apportionSplitCharge.JR_E6_GatewaySellHeader = gatewaySellHeaderPK;
				cost.PrepareForPosting();
				AssertEquals(job.Charges.Count, 1);
				AssertEquals(gatewaySellHeaderPK, job.Charges[0].JR_E6_GatewaySellHeader);
			}
		}

		public void TestPrepareForPosting_JR_E6_GovtChargeCode()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			using (Job job = Factory.NewJobWithValidTestDataForTesting<Job>())
			{
				JobConsolCost cost = CreateConsolCostForJobAndCreditor(job.PK, testObjectCreator.Creditor2.PK);
				cost.E6_CostGovtChargeCode = "GVTCC2";
				cost.E6_SellGovtChargeCode = "GVTCC3";
				var apportionSplitCharge = cost.ApportionmentCharges[0];
				apportionSplitCharge.JR_JH = job.PK;
				cost.PrepareForPosting();
				AssertEquals(job.Charges.Count, 1);
				AssertEquals("GVTCC2", job.Charges[0].JR_CostGovtChargeCode);
				AssertEquals("GVTCC3", job.Charges[0].JR_SellGovtChargeCode);
			}
		}

		public void TestPrepareForPostingChecksCreditorAccount()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			JobCharge charge2 = job.Charges.AddNew();
			charge2.JR_GB = GlbBranch.CurrentBranch.PK;
			charge2.JR_AC = Env.Registry.FreightChargeCode;
			charge2.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge2.JR_OH_CostAccount = testObjectCreator.Creditor1.PK;
			Factory.Save();
			JobConsolCost cost = CreateConsolCostForJobAndCreditor(job.PK, testObjectCreator.Creditor2.PK);
			cost.PrepareForPosting();
			AssertEquals(1, cost.ApportionmentCharges.Count);
			AssertNotEquals("Charge is not used for apportions", charge2.PK, cost.ApportionmentCharges[0].PK);
			AssertEquals("Charge keeps it own Creditor", testObjectCreator.Creditor1.PK, charge2.JR_OH_CostAccount);
			cost = CreateConsolCostForJobAndCreditor(job.PK, testObjectCreator.Creditor3.PK);
			charge2.JR_OH_CostAccount = ZGuid.Empty;
			cost.PrepareForPosting();
			AssertEquals(1, cost.ApportionmentCharges.Count);
			AssertEquals("Charge is used for apportions", charge2.PK, cost.ApportionmentCharges[0].PK);
			AssertEquals("Charge got Creditor1", testObjectCreator.Creditor3.PK, charge2.JR_OH_CostAccount);
			cost = CreateConsolCostForJobAndCreditor(job.PK, ZGuid.Empty);
			charge2.JR_OH_CostAccount = testObjectCreator.Creditor2.PK;
			cost.PrepareForPosting();
			AssertEquals(1, cost.ApportionmentCharges.Count);
			AssertNotEquals("Charge is not used for apportions", charge2.PK, cost.ApportionmentCharges[0].PK);
			AssertEquals("Charge keeps it own Creditor", testObjectCreator.Creditor2.PK, charge2.JR_OH_CostAccount);
		}

		JobConsolCost CreateConsolCostForJobAndCreditor(ZGuid jobPK, ZGuid creditorPK)
		{
			JobConsolCost cost = (JobConsolCost)GetNewBusinessObject();
			cost.E6_GC = GlbCompany.CurrentCompany.PK;
			cost.E6_OH_Creditor = creditorPK;
			ApportionSplitCharge charge = cost.ApportionmentCharges.AddNew();
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost.E6_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge.JR_JH = jobPK;
			charge.JR_OH_CostAccount = creditorPK;
			return cost;
		}

		public void TestPrepareForPostingForMultipleConsolShipments()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_ParentTableCode = "JS";
			job.JH_ParentID = shipment.PK;
			ForwardingConsol consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			consol1.Shipments.Add(shipment);
			ApportionmentListing apps1 = new ApportionmentListing(Factory, consol1);
			JobConsolCost cost1 = apps1.CostsCollection.TryAddNew();
			cost1.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			ApportionSplitCharge charge = cost1.ApportionmentCharges[0];
			charge.JR_OSCostAmt = 100;
			cost1.E6_OSCostAmount = 100;
			cost1.E6_LocalCostAmount = 100;
			cost1.PrepareForPosting();
			AssertEquals(1, cost1.ApportionmentCharges.Count);
			AssertEquals(1, job.Charges.Count);
			AssertEquals(job.Charges[0].PK, cost1.ApportionmentCharges[0].PK);
			AssertEquals(job.Charges[0].JR_OSCostAmt, 100M);
			Factory.Save();
			ForwardingConsol consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			consol2.Shipments.Add(shipment);
			ApportionmentListing apps2 = new ApportionmentListing(Factory, consol2);
			JobConsolCost cost2 = apps2.CostsCollection.TryAddNew();
			cost2.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			charge = cost2.ApportionmentCharges[0];
			charge.JR_OSCostAmt = 200;
			cost2.PrepareForPosting();
			AssertEquals(1, cost2.ApportionmentCharges.Count);
			AssertEquals(2, job.Charges.Count);
			AssertEquals(job.Charges[0].PK, cost1.ApportionmentCharges[0].PK);
			AssertEquals(job.Charges[0].JR_OSCostAmt, 100M);
			AssertEquals(job.Charges[1].PK, cost2.ApportionmentCharges[0].PK);
			AssertEquals(job.Charges[1].JR_OSCostAmt, 200M);
		}

		public void TestPrepareForPostingPreservesLocalCostAmountOnApportions()
		{
			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			Job job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_ParentTableCode = "JS";
			job1.JH_ParentID = shipment1.PK;
			Job job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_ParentTableCode = "JS";
			job2.JH_ParentID = shipment2.PK;
			JobCharge charge11 = job1.Charges.AddNew();
			charge11.JR_GB = GlbBranch.CurrentBranch.PK;
			charge11.JR_AC = Env.Registry.FreightChargeCode;
			Factory.Save();
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost.E6_RX_NKCurrency = "USD";
			cost.E6_ExchangeRate = 1.3555M;
			cost.E6_ApportionmentMethod = "SHP";
			cost.E6_OSCostAmount = 100m;
			AssertEquals("Cost.E6_LocalCostAmount should be as expected", 73.77m, cost.E6_LocalCostAmount);
			AssertEquals("Should be two apportions", 2, cost.ApportionmentCharges.Count);
			ApportionSplitCharge charge1 = cost.ApportionmentCharges[0];
			ApportionSplitCharge charge2 = cost.ApportionmentCharges[1];
			AssertEquals("Charge1.JR_OSCostAmt", 50M, charge1.JR_OSCostAmt);
			AssertEquals("Charge1.JR_LocalCostAmt", 36.88M, charge1.JR_LocalCostAmt);
			AssertEquals("Charge2.JR_OSCostAmt", 50M, charge2.JR_OSCostAmt);
			AssertEquals("Charge2.JR_LocalCostAmt", 36.89M, charge2.JR_LocalCostAmt);
			charge11.JR_GB = charge1.JR_GB;
			charge11.JR_GE = charge1.JR_GE;
			AssertEquals("Charge.IsCostPosted should be False to be updated by PrepareForPosting", false, charge11.IsCostPosted);
			AssertEquals("Charge.IsInDatabase should be True to be updated by PrepareForPosting", true, charge11.IsInDatabase);
			cost.PrepareForPosting();
			AssertEquals("Should be two apportions", 2, cost.ApportionmentCharges.Count);
			AssertNotEquals("Should be other Charge", charge1.PK, cost.ApportionmentCharges[0].PK);
			charge1 = cost.ApportionmentCharges[0];
			charge2 = cost.ApportionmentCharges[1];
			AssertEquals("Charge1.JR_OSCostAmt", 50M, charge1.JR_OSCostAmt);
			AssertEquals("Charge1.JR_LocalCostAmt", 36.88M, charge1.JR_LocalCostAmt);
			AssertEquals("Charge2.JR_OSCostAmt", 50M, charge2.JR_OSCostAmt);
			AssertEquals("Charge2.JR_LocalCostAmt", 36.89M, charge2.JR_LocalCostAmt);
		}

		public void TestPrepareForPostingFirstLooksForChargesInDbToUpdate()
		{
			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			Job job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_ParentTableCode = "JS";
			job1.JH_ParentID = shipment1.PK;
			Job job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_ParentTableCode = "JS";
			job2.JH_ParentID = shipment2.PK;
			JobCharge charge11 = job1.Charges.AddNew();
			charge11.JR_GB = GlbBranch.CurrentBranch.PK;
			charge11.JR_AC = Env.Registry.FreightChargeCode;
			Factory.Save();
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost.E6_RX_NKCurrency = "USD";
			cost.E6_ExchangeRate = 1.3555M;
			cost.E6_ApportionmentMethod = "SHP";
			cost.E6_OSCostAmount = 100m;
			AssertEquals("Cost.E6_LocalCostAmount should be as expected", 73.77m, cost.E6_LocalCostAmount);
			AssertEquals("Should be two apportions", 2, cost.ApportionmentCharges.Count);
			ApportionSplitCharge charge1 = cost.ApportionmentCharges[0];
			ApportionSplitCharge charge2 = cost.ApportionmentCharges[1];
			AssertEquals("Charge1.JR_OSCostAmt", 50M, charge1.JR_OSCostAmt);
			AssertEquals("Charge1.JR_LocalCostAmt", 36.88M, charge1.JR_LocalCostAmt);
			AssertEquals("Charge2.JR_OSCostAmt", 50M, charge2.JR_OSCostAmt);
			AssertEquals("Charge2.JR_LocalCostAmt", 36.89M, charge2.JR_LocalCostAmt);
			charge11.JR_AC = charge1.JR_AC;
			charge11.JR_GB = charge1.JR_GB;
			charge11.JR_GE = charge1.JR_GE;
			var charge12 = job1.Charges.AddNew();
			charge12.JR_AC = charge1.JR_AC;
			charge12.JR_GB = charge1.JR_GB;
			charge12.JR_GE = charge1.JR_GE;
			AssertEquals("Charge.IsInDatabase", false, charge12.IsInDatabase);
			AssertEquals("Charge.IsCostPosted", false, charge12.IsCostPosted);
			AssertEquals("Charge.IsRevenuePostedWithManualJobRevenueJournal", false, charge12.IsRevenuePostedWithManualJobRevenueJournal);
			cost.PrepareForPosting();
			AssertEquals("Should be two apportions", 2, cost.ApportionmentCharges.Count);
			AssertEquals("Charge in DB must not be used", true, cost.ApportionmentCharges.Any(x => x.PK == charge11.PK));
			AssertEquals("Not in DB Charge must not be used because we have matching Charge in Db", false, cost.ApportionmentCharges.Any(x => x.PK == charge12.PK));
		}

		public void TestPrepareForPostingSynchroniseInvoiceDetails()
		{
			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_GC = GlbCompany.CurrentCompany.PK;
			var jobCharge1 = job1.Charges.AddNew();
			jobCharge1.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge1.JR_AC = Env.Registry.FreightChargeCode;
			jobCharge1.JR_GE = GlbDepartment.CurrentDepartment.PK;
			jobCharge1.JR_OH_SellAccount = ObjectCreator.ABIGAS.PK;
			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_GC = GlbCompany.CurrentCompany.PK;
			var jobCharge2 = job2.Charges.AddNew();
			jobCharge2.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge2.JR_AC = Env.Registry.FreightChargeCode;
			jobCharge2.JR_GE = GlbDepartment.CurrentDepartment.PK;
			jobCharge2.JR_OH_CostAccount = ObjectCreator.AALSHI.PK;
			jobCharge2.JR_OH_SellAccount = ObjectCreator.ABIGAS.PK;
			Factory.Save();
			JobConsolCost cost = (JobConsolCost)GetNewBusinessObject();
			cost.E6_GC = GlbCompany.CurrentCompany.PK;
			var appCharge1 = Factory.Load<ApportionSplitCharge>(jobCharge1.PK);
			cost.ApportionmentCharges.Add(appCharge1);
			AssertEquals(cost.PK, appCharge1.JR_E6);
			AssertEquals("Is in database", true, appCharge1.IsInDatabase);
			var appCharge2 = cost.ApportionmentCharges.AddNew();
			appCharge2.JR_GB = GlbBranch.CurrentBranch.PK;
			appCharge2.JR_GE = GlbDepartment.CurrentDepartment.PK;
			appCharge2.JR_JH = job2.PK;
			AssertEquals("Is not in database", false, appCharge2.IsInDatabase);
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			cost.E6_InvoiceNum = "12345";
			cost.E6_InvoiceDate = ZDateTime.Today.AddDays(1);
			cost.E6_PaymentDate = ZDateTime.Today.AddDays(2);
			cost.E6_CostReference = "COST";
			cost.E6_AT_TaxRate = ObjectCreator.GST1.PK;
			cost.E6_A9_VATClass = Factory.NewWithValidTestData<AccInvMsg>().PK;
			jobCharge1.JR_APInvoiceNum = "345";
			jobCharge1.JR_APInvoiceDate = ZDateTime.Today;
			jobCharge1.JR_PaymentDate = ZDateTime.Today.AddDays(1);
			jobCharge1.JR_OH_CostAccount = ObjectCreator.Creditor1.PK;
			jobCharge1.JR_CostReference = "COST1";
			jobCharge1.JR_AT_CostGSTRate = ObjectCreator.GSTFREE1.PK;
			jobCharge1.JR_A9_CostVATClass = Factory.NewWithValidTestData<AccInvMsg>().PK;
			jobCharge2.JR_APInvoiceNum = "567";
			jobCharge2.JR_APInvoiceDate = ZDateTime.Today;
			jobCharge2.JR_PaymentDate = ZDateTime.Today.AddDays(3);
			jobCharge2.JR_CostReference = "COST1";
			jobCharge2.JR_AT_CostGSTRate = ObjectCreator.GSTWithExtraRate.PK;
			jobCharge2.JR_A9_CostVATClass = Factory.NewWithValidTestData<AccInvMsg>().PK;
			cost.PrepareForPosting();
			AssertEquals("ApportionmentCharges.Count", 2, cost.ApportionmentCharges.Count);
			AssertEquals(jobCharge1.PK, cost.ApportionmentCharges[0].PK);
			AssertEquals("jobCharge1.JR_APInvoiceNum", cost.E6_InvoiceNum, jobCharge1.JR_APInvoiceNum);
			AssertEquals("jobCharge1.JR_APInvoiceDate", cost.E6_InvoiceDate, jobCharge1.JR_APInvoiceDate);
			AssertEquals("jobCharge1.JR_PaymentDate", cost.E6_PaymentDate, jobCharge1.JR_PaymentDate);
			AssertEquals("jobCharge1.JR_OH_CostAccount", cost.E6_OH_Creditor, jobCharge1.JR_OH_CostAccount);
			AssertEquals("jobCharge1.JR_CostReference", cost.E6_CostReference, jobCharge1.JR_CostReference);
			AssertEquals("jobCharge1.JR_AT_CostGSTRate", cost.E6_AT_TaxRate, jobCharge1.JR_AT_CostGSTRate);
			AssertEquals("jobCharge1.JR_A9_CostVATClass", cost.E6_A9_VATClass, jobCharge1.JR_A9_CostVATClass);
			AssertEquals(jobCharge2.PK, cost.ApportionmentCharges[1].PK);
			AssertEquals("jobCharge2.JR_APInvoiceNum", cost.E6_InvoiceNum, jobCharge2.JR_APInvoiceNum);
			AssertEquals("jobCharge2.JR_APInvoiceDate", cost.E6_InvoiceDate, jobCharge2.JR_APInvoiceDate);
			AssertEquals("jobCharge2.JR_PaymentDate", cost.E6_PaymentDate, jobCharge2.JR_PaymentDate);
			AssertEquals("jobCharge2.JR_OH_CostAccount", cost.E6_OH_Creditor, jobCharge2.JR_OH_CostAccount);
			AssertEquals("jobCharge2.JR_CostReference", cost.E6_CostReference, jobCharge2.JR_CostReference);
			AssertEquals("jobCharge2.JR_AT_CostGSTRate", cost.E6_AT_TaxRate, jobCharge2.JR_AT_CostGSTRate);
			AssertEquals("jobCharge2.JR_A9_CostVATClass", cost.E6_A9_VATClass, jobCharge2.JR_A9_CostVATClass);
		}

		public void TestPrepareForPostingSynchroniseInvoiceDetailsForAPInvoice()
		{
			PrepareForPostingSynchroniseInvoiceDetailsCore(true);
		}

		public void TestPrepareForPostingSynchroniseInvoiceDetailsForOthers()
		{
			PrepareForPostingSynchroniseInvoiceDetailsCore(false);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestPrepareForPosting_HandlesZeroSellRateOnCharge_WhenCostCurrencyMatches_MRG()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol();
			var shipment1 = creator.CreateShipment("S0001", consol);
			var shipment2 = creator.CreateShipment("S0002", consol);
			var shipment3 = creator.CreateShipment("S0003", consol);
			var job1 = creator.CreateJob(shipment1, creator.LocalClient, 0m, creator.Agent, 0m);
			var job2 = creator.CreateJob(shipment2, creator.LocalClient, 0m, creator.ABIGAS, 0m);
			var job3 = creator.CreateJob(shipment3, creator.LocalClient, 0m, creator.Agent, 0m);
			var charge1 = creator.CreateCharge(job1, creator.FRT, 50m, 55m);

			creator.Agent.CompanyData.AccARExchangeRateConfigurations.SetExRate(LedgerTypes.AccountsReceivable, JobInvoicingConsumerTypes.Shipment.Code, OrgConstants.ServiceDirection.Code.All, OrgConstants.ModesForGroupOrSubTotal.Codes.All,
					Constants.ExchangeRateTypes.Code.BuyRate, preference: Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedDepartureDate);
			creator.Agent.CompanyData.OB_RX_NKARDDefltCurrency = creator.USD.Code;
			shipment1.JS_E_DEP = ZDateTime.Today.AddDays(-5);

			creator.ABIGAS.CompanyData.OB_RX_NKARDDefltCurrency = creator.USD.Code;

			var rate = creator.USD.ExchangeRates.AddNew();
			rate.RE_StartDate = ZDateTime.Today.AddDays(-2);
			rate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			rate.RE_ExRateType = Constants.ExchangeRateTypes.Code.BuyRate;
			rate.RE_SellRate = 1.56m;

			Factory.Save();
			ExchangeRateReader.GetReaderInstance().ClearCache();

			var cost = creator.CreateConsolCost(consol, creator.FRT, creator.Creditor1);
			AssertEquals(Constants.ChargeType.Margin, cost.ChargeCode.AC_ChargeType);
			cost.E6_RX_NKCurrency = creator.USD.Code;
			cost.E6_ExchangeRate = 1.3555m;
			cost.E6_ApportionmentMethod = "SHP";
			cost.E6_OSCostAmount = 150m;

			cost.PrepareForPosting();

			AssertEquals(charge1.PK, job1.Charges[0].PK);
			AssertEquals(creator.Agent.PK, job1.Charges[0].JR_OH_SellAccount);
			AssertEquals(creator.USD.Code, job1.Charges[0].JR_RX_NKSellCurrency);
			AssertNotNull(job1.Charges[0].RevenueExchangeRate);
			AssertEquals(1.3555m, job1.Charges[0].RevenueExchangeRate.Rate);
			AssertEquals(50m, job1.Charges[0].JR_OSSellAmt);
			AssertEquals(36.89m, job1.Charges[0].JR_LocalSellAmt);
			AssertEquals(36.88m, job1.Charges[0].JR_LocalCostAmt);

			AssertEquals(creator.ABIGAS.PK, job2.Charges[0].JR_OH_SellAccount);
			AssertEquals(creator.USD.Code, job2.Charges[0].JR_RX_NKSellCurrency);
			AssertNotNull(job2.Charges[0].RevenueExchangeRate);
			AssertEquals(1.56m, job2.Charges[0].RevenueExchangeRate.Rate);
			AssertEquals(50m, job1.Charges[0].JR_OSSellAmt);
			AssertEquals(32.05m, job2.Charges[0].JR_LocalSellAmt);
			AssertNotEquals(job2.Charges[0].JR_LocalCostAmt, job2.Charges[0].JR_LocalSellAmt);

			AssertEquals(creator.Agent.PK, job3.Charges[0].JR_OH_SellAccount);
			AssertEquals(creator.USD.Code, job3.Charges[0].JR_RX_NKSellCurrency);
			AssertNotNull(job3.Charges[0].RevenueExchangeRate);
			AssertEquals(1.3555m, job3.Charges[0].RevenueExchangeRate.Rate);
			AssertEquals(50m, job3.Charges[0].JR_OSSellAmt);
			AssertEquals(36.89m, job3.Charges[0].JR_LocalSellAmt);
			AssertEquals(job3.Charges[0].JR_LocalCostAmt, job3.Charges[0].JR_LocalSellAmt);

			Factory.Save();

			AssertNotNull(job1.Charges[0].RevenueExchangeRate);
			AssertEquals(1.3555m, job1.Charges[0].RevenueExchangeRate.Rate);
			AssertEquals(50m, job1.Charges[0].JR_OSSellAmt);
			AssertEquals(36.89m, job1.Charges[0].JR_LocalSellAmt);

			AssertNotNull(job2.Charges[0].RevenueExchangeRate);
			AssertEquals(1.56m, job2.Charges[0].RevenueExchangeRate.Rate);
			AssertEquals(50m, job2.Charges[0].JR_OSSellAmt);
			AssertEquals(32.05m, job2.Charges[0].JR_LocalSellAmt);

			AssertNotNull(job3.Charges[0].RevenueExchangeRate);
			AssertEquals(1.3555m, job3.Charges[0].RevenueExchangeRate.Rate);
			AssertEquals(50m, job3.Charges[0].JR_OSSellAmt);
			AssertEquals(36.89m, job3.Charges[0].JR_LocalSellAmt);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestPrepareForPosting_HandlesZeroSellRateOnCharge_WhenCostCurrencyMatches_DSB()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol();
			var shipment1 = creator.CreateShipment("S0001", consol);
			var shipment2 = creator.CreateShipment("S0002", consol);
			var shipment3 = creator.CreateShipment("S0003", consol);
			var job1 = creator.CreateJob(shipment1, creator.LocalClient, 0m, creator.Agent, 0m);
			var job2 = creator.CreateJob(shipment2, creator.LocalClient, 0m, creator.ABIGAS, 0m);
			var job3 = creator.CreateJob(shipment3, creator.LocalClient, 0m, creator.Agent, 0m);
			var charge1 = creator.CreateCharge(job1, creator.DSBChargeCode, 50m, 50m);

			creator.LocalClient.CompanyData.AccARExchangeRateConfigurations.SetExRate(LedgerTypes.AccountsReceivable, JobInvoicingConsumerTypes.Shipment.Code, OrgConstants.ServiceDirection.Code.All, OrgConstants.ModesForGroupOrSubTotal.Codes.All,
					Constants.ExchangeRateTypes.Code.BuyRate, preference: Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedDepartureDate);
			creator.LocalClient.CompanyData.OB_RX_NKARDDefltCurrency = creator.USD.Code;
			shipment1.JS_E_DEP = ZDateTime.Today.AddDays(-5);

			creator.ABIGAS.CompanyData.OB_RX_NKARDDefltCurrency = creator.USD.Code;

			var rate = creator.USD.ExchangeRates.AddNew();
			rate.RE_StartDate = ZDateTime.Today.AddDays(-2);
			rate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			rate.RE_ExRateType = Constants.ExchangeRateTypes.Code.BuyRate;
			rate.RE_SellRate = 1.56m;

			Factory.Save();
			ExchangeRateReader.GetReaderInstance().ClearCache();

			var cost = creator.CreateConsolCost(consol, creator.DSBChargeCode, creator.Creditor1);
			AssertEquals(Constants.ChargeType.Disbursement, cost.ChargeCode.AC_ChargeType);
			cost.E6_RX_NKCurrency = creator.USD.Code;
			cost.E6_ExchangeRate = 1.3555m;
			cost.E6_ApportionmentMethod = "SHP";
			cost.E6_OSCostAmount = 150m;

			cost.PrepareForPosting();

			AssertEquals(charge1.PK, job1.Charges[0].PK);
			AssertEquals(creator.LocalClient.PK, job1.Charges[0].JR_OH_SellAccount);
			AssertEquals(creator.USD.Code, job1.Charges[0].JR_RX_NKSellCurrency);
			AssertNotNull(job1.Charges[0].RevenueExchangeRate);
			AssertEquals(1.3555m, job1.Charges[0].RevenueExchangeRate.Rate);
			AssertEquals(50m, job1.Charges[0].JR_OSSellAmt);
			AssertEquals(36.88m, job1.Charges[0].JR_LocalSellAmt);
			AssertEquals(job1.Charges[0].JR_LocalCostAmt, job1.Charges[0].JR_LocalSellAmt);

			AssertEquals(creator.LocalClient.PK, job2.Charges[0].JR_OH_SellAccount);
			AssertEquals(creator.USD.Code, job2.Charges[0].JR_RX_NKSellCurrency);
			AssertNotNull(job2.Charges[0].RevenueExchangeRate);
			AssertEquals(1.3555m, job2.Charges[0].RevenueExchangeRate.Rate);
			AssertEquals(50m, job2.Charges[0].JR_OSSellAmt);
			AssertEquals(36.89m, job2.Charges[0].JR_LocalSellAmt);
			AssertEquals(job2.Charges[0].JR_LocalCostAmt, job2.Charges[0].JR_LocalSellAmt);

			AssertEquals(creator.LocalClient.PK, job3.Charges[0].JR_OH_SellAccount);
			AssertEquals(creator.USD.Code, job3.Charges[0].JR_RX_NKSellCurrency);
			AssertNotNull(job3.Charges[0].RevenueExchangeRate);
			AssertEquals(1.3555m, job3.Charges[0].RevenueExchangeRate.Rate);
			AssertEquals(50m, job3.Charges[0].JR_OSSellAmt);
			AssertEquals(36.89m, job3.Charges[0].JR_LocalSellAmt);
			AssertEquals(job3.Charges[0].JR_LocalCostAmt, job3.Charges[0].JR_LocalSellAmt);

			Factory.Save();

			AssertNotNull(job1.Charges[0].RevenueExchangeRate);
			AssertEquals(1.3555m, job1.Charges[0].RevenueExchangeRate.Rate);
			AssertEquals(50m, job1.Charges[0].JR_OSSellAmt);
			AssertEquals(36.88m, job1.Charges[0].JR_LocalSellAmt);

			AssertNotNull(job2.Charges[0].RevenueExchangeRate);
			AssertEquals(1.3555m, job2.Charges[0].RevenueExchangeRate.Rate);
			AssertEquals(50m, job2.Charges[0].JR_OSSellAmt);
			AssertEquals(36.89m, job2.Charges[0].JR_LocalSellAmt);

			AssertNotNull(job3.Charges[0].RevenueExchangeRate);
			AssertEquals(1.3555m, job3.Charges[0].RevenueExchangeRate.Rate);
			AssertEquals(50m, job3.Charges[0].JR_OSSellAmt);
			AssertEquals(36.89m, job3.Charges[0].JR_LocalSellAmt);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestPrepareForPosting_HandlesZeroSellRateOnCharge_ForExistingCharge_MRG()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol();
			var shipment1 = creator.CreateShipment("S0001", consol);
			var job1 = creator.CreateJob(shipment1, creator.LocalClient, 0m, creator.Agent, 0m);
			var charge1 = creator.CreateCharge(job1, creator.FRT, 50m, 55m);

			creator.Agent.CompanyData.AccARExchangeRateConfigurations.SetExRate(LedgerTypes.AccountsReceivable, JobInvoicingConsumerTypes.Shipment.Code, OrgConstants.ServiceDirection.Code.All, OrgConstants.ModesForGroupOrSubTotal.Codes.All,
					Constants.ExchangeRateTypes.Code.SellRate, preference: Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedDepartureDate);
			creator.Agent.CompanyData.OB_RX_NKARDDefltCurrency = creator.EUR.Code;
			shipment1.JS_E_DEP = ZDateTime.Today.AddDays(-5);

			var rate = creator.EUR.ExchangeRates.AddNew();
			rate.RE_StartDate = ZDateTime.Today.AddDays(-2);
			rate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			rate.RE_ExRateType = Constants.ExchangeRateTypes.Code.SellRate;
			rate.RE_SellRate = 1.56m;

			Factory.Save();
			ExchangeRateReader.GetReaderInstance().ClearCache();

			var cost = creator.CreateConsolCost(consol, creator.FRT, creator.Creditor1);
			AssertEquals(Constants.ChargeType.Margin, cost.ChargeCode.AC_ChargeType);
			cost.E6_RX_NKCurrency = creator.USD.Code;
			cost.E6_ExchangeRate = 1.3555m;
			cost.E6_ApportionmentMethod = "SHP";
			cost.E6_OSCostAmount = 100m;

			cost.PrepareForPosting();

			AssertEquals(charge1.PK, job1.Charges[0].PK);
			AssertEquals(creator.Agent.PK, job1.Charges[0].JR_OH_SellAccount);
			AssertEquals(creator.EUR.Code, job1.Charges[0].JR_RX_NKSellCurrency);
			AssertNotNull(job1.Charges[0].RevenueExchangeRate);
			Assert(job1.Charges[0].RevenueExchangeRate.Rate.IsEmpty);
			Assert(job1.Charges[0].JR_OSSellAmt.IsEmpty);
			Assert(job1.Charges[0].JR_LocalSellAmt.IsEmpty);

			var exRate = Factory.Load<JobInvoicing.ExchangeRate>(job1.Charges[0].RevenueExchangeRate.ExchangeRatePk);
			AssertEquals(false, exRate.IsSavedByFactory);
			AssertEquals(false, exRate.HasChanges);

			Factory.Save();

			AssertEquals(1, job1.Charges.Count);
			AssertEquals(creator.Agent.PK, job1.Charges[0].JR_OH_SellAccount);
			AssertEquals(creator.EUR.Code, job1.Charges[0].JR_RX_NKSellCurrency);
			AssertNotNull(job1.Charges[0].RevenueExchangeRate);
			Assert(job1.Charges[0].RevenueExchangeRate.Rate.IsEmpty);
			Assert(job1.Charges[0].JR_OSSellAmt.IsEmpty);
			Assert(job1.Charges[0].JR_LocalSellAmt.IsEmpty);

			AssertEquals(exRate.PK, job1.Charges[0].RevenueExchangeRate.ExchangeRatePk);
			exRate = Factory.Load<JobInvoicing.ExchangeRate>(job1.Charges[0].RevenueExchangeRate.ExchangeRatePk);
			exRate.JF_BaseRate = 1.6m;

			AssertEquals(1, job1.Charges.Count);
			AssertEquals(creator.Agent.PK, job1.Charges[0].JR_OH_SellAccount);
			AssertEquals(creator.EUR.Code, job1.Charges[0].JR_RX_NKSellCurrency);
			AssertNotNull(job1.Charges[0].RevenueExchangeRate);
			AssertEquals(1.6m, job1.Charges[0].RevenueExchangeRate.Rate);
			Assert(job1.Charges[0].JR_OSSellAmt.IsEmpty);
			Assert(job1.Charges[0].JR_LocalSellAmt.IsEmpty);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestPrepareForPosting_HandlesZeroSellRateOnCharge_ForExistingCharge_DSB()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol();
			var shipment1 = creator.CreateShipment("S0001", consol);
			var job1 = creator.CreateJob(shipment1, creator.LocalClient, 0m, creator.Agent, 0m);
			var charge1 = creator.CreateCharge(job1, creator.DSBChargeCode, 50m, 50m);

			creator.LocalClient.CompanyData.OB_RX_NKARDDefltCurrency = creator.EUR.Code;

			var rate = creator.EUR.ExchangeRates.AddNew();
			rate.RE_StartDate = ZDateTime.Today.AddDays(-2);
			rate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			rate.RE_ExRateType = Constants.ExchangeRateTypes.Code.SellRate;
			rate.RE_SellRate = 1.56m;

			Factory.Save();
			ExchangeRateReader.GetReaderInstance().ClearCache();

			var cost = creator.CreateConsolCost(consol, creator.DSBChargeCode, creator.Creditor1);
			AssertEquals(Constants.ChargeType.Disbursement, cost.ChargeCode.AC_ChargeType);
			cost.E6_RX_NKCurrency = creator.USD.Code;
			cost.E6_ExchangeRate = 1.3555m;
			cost.E6_ApportionmentMethod = "SHP";
			cost.E6_OSCostAmount = 100m;

			cost.PrepareForPosting();

			AssertEquals(charge1.PK, job1.Charges[0].PK);
			AssertEquals(creator.LocalClient.PK, job1.Charges[0].JR_OH_SellAccount);
			AssertEquals(creator.EUR.Code, job1.Charges[0].JR_RX_NKSellCurrency);
			AssertNotNull(job1.Charges[0].RevenueExchangeRate);
			Assert(job1.Charges[0].RevenueExchangeRate.Rate.IsEmpty);
			Assert(job1.Charges[0].JR_OSSellAmt.IsEmpty);
			AssertEquals(job1.Charges[0].LocalCostAmount, job1.Charges[0].JR_LocalSellAmt);       // For DSB Charge, the LocalSellAmount is set as the LocalCostAmount

			var exRate = Factory.Load<JobInvoicing.ExchangeRate>(job1.Charges[0].RevenueExchangeRate.ExchangeRatePk);
			AssertEquals(false, exRate.IsSavedByFactory);
			AssertEquals(false, exRate.HasChanges);

			Factory.Save();

			AssertEquals(1, job1.Charges.Count);
			AssertEquals(creator.LocalClient.PK, job1.Charges[0].JR_OH_SellAccount);
			AssertEquals(creator.EUR.Code, job1.Charges[0].JR_RX_NKSellCurrency);
			AssertNotNull(job1.Charges[0].RevenueExchangeRate);
			Assert(job1.Charges[0].RevenueExchangeRate.Rate.IsEmpty);
			Assert(job1.Charges[0].JR_OSSellAmt.IsEmpty);
			AssertEquals(job1.Charges[0].LocalCostAmount, job1.Charges[0].JR_LocalSellAmt);      // For DSB Charge, the LocalSellAmount is set as the LocalCostAmount

			AssertEquals(exRate.PK, job1.Charges[0].RevenueExchangeRate.ExchangeRatePk);
			exRate = Factory.Load<JobInvoicing.ExchangeRate>(job1.Charges[0].RevenueExchangeRate.ExchangeRatePk);
			exRate.JF_BaseRate = 1.6m;

			AssertEquals(1, job1.Charges.Count);
			AssertEquals(creator.LocalClient.PK, job1.Charges[0].JR_OH_SellAccount);
			AssertEquals(creator.EUR.Code, job1.Charges[0].JR_RX_NKSellCurrency);
			AssertNotNull(job1.Charges[0].RevenueExchangeRate);
			AssertEquals(1.6m, job1.Charges[0].RevenueExchangeRate.Rate);
			AssertEquals(118.04m, job1.Charges[0].JR_OSSellAmt);
			AssertEquals(73.77m, job1.Charges[0].JR_LocalSellAmt);
			AssertEquals(job1.Charges[0].JR_LocalCostAmt, job1.Charges[0].JR_LocalSellAmt);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestPrepareForPosting_HandlesZeroSellRateOnCharge_ForNewCharge_MRG()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol();
			var shipment1 = creator.CreateShipment("S0001", consol);
			var job1 = creator.CreateJob(shipment1, creator.LocalClient, 0m, creator.Agent, 0m);

			creator.LocalClient.CompanyData.OB_RX_NKARDDefltCurrency = creator.EUR.Code;

			var rate = creator.EUR.ExchangeRates.AddNew();
			rate.RE_StartDate = ZDateTime.Today.AddDays(-2);
			rate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			rate.RE_ExRateType = Constants.ExchangeRateTypes.Code.SellRate;
			rate.RE_SellRate = 1.56m;

			Factory.Save();
			ExchangeRateReader.GetReaderInstance().ClearCache();

			var cost = creator.CreateConsolCost(consol, creator.DSBChargeCode, creator.Creditor1);
			AssertEquals(Constants.ChargeType.Disbursement, cost.ChargeCode.AC_ChargeType);
			cost.E6_RX_NKCurrency = creator.USD.Code;
			cost.E6_ExchangeRate = 1.3555m;
			cost.E6_ApportionmentMethod = "SHP";
			cost.E6_OSCostAmount = 100m;

			cost.PrepareForPosting();

			AssertEquals(1, job1.Charges.Count);
			AssertEquals(creator.LocalClient.PK, job1.Charges[0].JR_OH_SellAccount);
			AssertEquals(creator.EUR.Code, job1.Charges[0].JR_RX_NKSellCurrency);
			AssertNotNull(job1.Charges[0].RevenueExchangeRate);
			Assert(job1.Charges[0].RevenueExchangeRate.Rate.IsEmpty);
			Assert(job1.Charges[0].JR_OSSellAmt.IsEmpty);
			AssertEquals(job1.Charges[0].JR_LocalCostAmt, job1.Charges[0].JR_LocalSellAmt);           // For DSB Charge, the LocalSellAmount is set as the LocalCostAmount

			var exRate = Factory.Load<JobInvoicing.ExchangeRate>(job1.Charges[0].RevenueExchangeRate.ExchangeRatePk);
			AssertEquals(false, exRate.IsSavedByFactory);
			AssertEquals(false, exRate.HasChanges);

			Factory.Save();

			AssertEquals(1, job1.Charges.Count);
			AssertEquals(creator.LocalClient.PK, job1.Charges[0].JR_OH_SellAccount);
			AssertEquals(creator.EUR.Code, job1.Charges[0].JR_RX_NKSellCurrency);
			AssertNotNull(job1.Charges[0].RevenueExchangeRate);
			Assert(job1.Charges[0].RevenueExchangeRate.Rate.IsEmpty);
			Assert(job1.Charges[0].JR_OSSellAmt.IsEmpty);
			AssertEquals(job1.Charges[0].JR_LocalCostAmt, job1.Charges[0].JR_LocalSellAmt);           // For DSB Charge, the LocalSellAmount is set as the LocalCostAmount

			AssertEquals(exRate.PK, job1.Charges[0].RevenueExchangeRate.ExchangeRatePk);             
			exRate = Factory.Load<JobInvoicing.ExchangeRate>(job1.Charges[0].RevenueExchangeRate.ExchangeRatePk);
			exRate.JF_BaseRate = 1.6m;

			AssertEquals(1, job1.Charges.Count);
			AssertEquals(creator.LocalClient.PK, job1.Charges[0].JR_OH_SellAccount);
			AssertEquals(creator.EUR.Code, job1.Charges[0].JR_RX_NKSellCurrency);
			AssertNotNull(job1.Charges[0].RevenueExchangeRate);
			AssertEquals(1.6m, job1.Charges[0].RevenueExchangeRate.Rate);
			AssertEquals(118.04m, job1.Charges[0].JR_OSSellAmt);
			AssertEquals(73.77m, job1.Charges[0].JR_LocalSellAmt);
			AssertEquals(job1.Charges[0].JR_LocalCostAmt, job1.Charges[0].JR_LocalSellAmt);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestPrepareForPosting_HandlesZeroSellRateOnCharge_ForNewCharge_DSB()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol();
			var shipment1 = creator.CreateShipment("S0001", consol);
			var job1 = creator.CreateJob(shipment1, creator.LocalClient, 0m, creator.Agent, 0m);

			creator.Agent.CompanyData.AccARExchangeRateConfigurations.SetExRate(LedgerTypes.AccountsReceivable, JobInvoicingConsumerTypes.Shipment.Code, OrgConstants.ServiceDirection.Code.All, OrgConstants.ModesForGroupOrSubTotal.Codes.All,
					Constants.ExchangeRateTypes.Code.SellRate, preference: Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedDepartureDate);
			creator.Agent.CompanyData.OB_RX_NKARDDefltCurrency = creator.EUR.Code;
			shipment1.JS_E_DEP = ZDateTime.Today.AddDays(-5);

			var rate = creator.EUR.ExchangeRates.AddNew();
			rate.RE_StartDate = ZDateTime.Today.AddDays(-2);
			rate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			rate.RE_ExRateType = Constants.ExchangeRateTypes.Code.SellRate;
			rate.RE_SellRate = 1.56m;

			Factory.Save();
			ExchangeRateReader.GetReaderInstance().ClearCache();

			var cost = creator.CreateConsolCost(consol, creator.FRT, creator.Creditor1);
			AssertEquals(Constants.ChargeType.Margin, cost.ChargeCode.AC_ChargeType);
			cost.E6_RX_NKCurrency = creator.USD.Code;
			cost.E6_ExchangeRate = 1.3555m;
			cost.E6_ApportionmentMethod = "SHP";
			cost.E6_OSCostAmount = 100m;

			cost.PrepareForPosting();

			AssertEquals(1, job1.Charges.Count);
			AssertEquals(creator.Agent.PK, job1.Charges[0].JR_OH_SellAccount);
			AssertEquals(creator.EUR.Code, job1.Charges[0].JR_RX_NKSellCurrency);
			AssertNotNull(job1.Charges[0].RevenueExchangeRate);
			Assert(job1.Charges[0].RevenueExchangeRate.Rate.IsEmpty);
			Assert(job1.Charges[0].JR_OSSellAmt.IsEmpty);
			Assert(job1.Charges[0].JR_LocalSellAmt.IsEmpty);

			var exRate = Factory.Load<JobInvoicing.ExchangeRate>(job1.Charges[0].RevenueExchangeRate.ExchangeRatePk);
			AssertEquals(false, exRate.IsSavedByFactory);
			AssertEquals(false, exRate.HasChanges);

			Factory.Save();

			AssertEquals(1, job1.Charges.Count);
			AssertEquals(creator.Agent.PK, job1.Charges[0].JR_OH_SellAccount);
			AssertEquals(creator.EUR.Code, job1.Charges[0].JR_RX_NKSellCurrency);
			AssertNotNull(job1.Charges[0].RevenueExchangeRate);
			Assert(job1.Charges[0].RevenueExchangeRate.Rate.IsEmpty);
			Assert(job1.Charges[0].JR_OSSellAmt.IsEmpty);
			Assert(job1.Charges[0].JR_LocalSellAmt.IsEmpty);

			AssertEquals(exRate.PK, job1.Charges[0].RevenueExchangeRate.ExchangeRatePk);
			exRate = Factory.Load<JobInvoicing.ExchangeRate>(job1.Charges[0].RevenueExchangeRate.ExchangeRatePk);
			exRate.JF_BaseRate = 1.6m;

			AssertEquals(1, job1.Charges.Count);
			AssertEquals(creator.Agent.PK, job1.Charges[0].JR_OH_SellAccount);
			AssertEquals(creator.EUR.Code, job1.Charges[0].JR_RX_NKSellCurrency);
			AssertNotNull(job1.Charges[0].RevenueExchangeRate);
			AssertEquals(1.6m, job1.Charges[0].RevenueExchangeRate.Rate);
			Assert(job1.Charges[0].JR_OSSellAmt.IsEmpty);
			Assert(job1.Charges[0].JR_LocalSellAmt.IsEmpty);
		}

		public void TestSynchroniseInvoiceDetails()
		{
			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_GC = GlbCompany.CurrentCompany.PK;
			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();
			JobConsolCost cost = (JobConsolCost)GetNewBusinessObject();
			cost.E6_GC = GlbCompany.CurrentCompany.PK;
			var appCharge1 = cost.ApportionmentCharges.AddNew();
			appCharge1.JR_GB = GlbBranch.CurrentBranch.PK;
			appCharge1.JR_GE = GlbDepartment.CurrentDepartment.PK;
			appCharge1.JR_JH = job1.PK;
			AssertEquals("Is not in database", false, appCharge1.IsInDatabase);
			var appCharge2 = cost.ApportionmentCharges.AddNew();
			appCharge2.JR_GB = GlbBranch.CurrentBranch.PK;
			appCharge2.JR_GE = GlbDepartment.CurrentDepartment.PK;
			appCharge2.JR_JH = job2.PK;
			AssertEquals("Is not in database", false, appCharge2.IsInDatabase);
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			cost.E6_InvoiceNum = "12345";
			cost.E6_InvoiceDate = ZDateTime.Today.AddDays(1);
			cost.E6_PaymentDate = ZDateTime.Today.AddDays(2);
			cost.E6_CostReference = "COST";
			cost.E6_AT_TaxRate = ObjectCreator.GST1.PK;
			cost.E6_A9_VATClass = Factory.NewWithValidTestData<AccInvMsg>().PK;
			AssertEquals("SynchroniseInvoiceDetailsIfNecessary", false, cost.SynchroniseUnpostedInvoiceDetailsIfNecessary());
			appCharge1.JR_APInvoiceNum = "345";
			appCharge1.JR_APInvoiceDate = ZDateTime.Today;
			appCharge1.JR_PaymentDate = ZDateTime.Today.AddDays(1);
			appCharge1.JR_OH_CostAccount = ObjectCreator.Creditor1.PK;
			appCharge1.JR_CostReference = "COST1";
			appCharge1.JR_AT_CostGSTRate = ObjectCreator.GSTFREE1.PK;
			appCharge1.JR_A9_CostVATClass = Factory.NewWithValidTestData<AccInvMsg>().PK;
			appCharge2.JR_APInvoiceNum = "567";
			appCharge2.JR_APInvoiceDate = ZDateTime.Today;
			appCharge2.JR_PaymentDate = ZDateTime.Today.AddDays(3);
			appCharge2.JR_OH_CostAccount = ObjectCreator.Creditor2.PK;
			appCharge2.JR_CostReference = "COST1";
			appCharge2.JR_AT_CostGSTRate = ObjectCreator.GSTWithExtraRate.PK;
			appCharge2.JR_A9_CostVATClass = Factory.NewWithValidTestData<AccInvMsg>().PK;
			Assert("SynchroniseInvoiceDetailsIfNecessary", cost.SynchroniseUnpostedInvoiceDetailsIfNecessary());
			AssertEquals("ApportionmentCharges.Count", 2, cost.ApportionmentCharges.Count);
			AssertEquals(appCharge1.PK, cost.ApportionmentCharges[0].PK);
			AssertEquals("jobCharge1.JR_APInvoiceNum", cost.E6_InvoiceNum, appCharge1.JR_APInvoiceNum);
			AssertEquals("jobCharge1.JR_APInvoiceDate", cost.E6_InvoiceDate, appCharge1.JR_APInvoiceDate);
			AssertEquals("jobCharge1.JR_PaymentDate", cost.E6_PaymentDate, appCharge1.JR_PaymentDate);
			AssertEquals("jobCharge1.JR_OH_CostAccount", cost.E6_OH_Creditor, appCharge1.JR_OH_CostAccount);
			AssertEquals("jobCharge1.JR_CostReference", cost.E6_CostReference, appCharge1.JR_CostReference);
			AssertEquals("jobCharge1.JR_AT_CostGSTRate", cost.E6_AT_TaxRate, appCharge1.JR_AT_CostGSTRate);
			AssertEquals("jobCharge1.JR_A9_CostVATClass", cost.E6_A9_VATClass, appCharge1.JR_A9_CostVATClass);
			AssertEquals(appCharge2.PK, cost.ApportionmentCharges[1].PK);
			AssertEquals("jobCharge2.JR_APInvoiceNum", cost.E6_InvoiceNum, appCharge2.JR_APInvoiceNum);
			AssertEquals("jobCharge2.JR_APInvoiceDate", cost.E6_InvoiceDate, appCharge2.JR_APInvoiceDate);
			AssertEquals("jobCharge2.JR_PaymentDate", cost.E6_PaymentDate, appCharge2.JR_PaymentDate);
			AssertEquals("jobCharge2.JR_OH_CostAccount", cost.E6_OH_Creditor, appCharge2.JR_OH_CostAccount);
			AssertEquals("jobCharge2.JR_CostReference", cost.E6_CostReference, appCharge2.JR_CostReference);
			AssertEquals("jobCharge2.JR_AT_CostGSTRate", cost.E6_AT_TaxRate, appCharge2.JR_AT_CostGSTRate);
			AssertEquals("jobCharge2.JR_A9_CostVATClass", cost.E6_A9_VATClass, appCharge2.JR_A9_CostVATClass);
		}

		public void TestSynchroniseInvoiceDetailsFixTaxRate()
		{
			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_GC = GlbCompany.CurrentCompany.PK;
			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();
			JobConsolCost cost = (JobConsolCost)GetNewBusinessObject();
			cost.E6_GC = GlbCompany.CurrentCompany.PK;
			var appCharge1 = cost.ApportionmentCharges.AddNew();
			appCharge1.JR_GB = GlbBranch.CurrentBranch.PK;
			appCharge1.JR_GE = GlbDepartment.CurrentDepartment.PK;
			appCharge1.JR_JH = job1.PK;
			AssertEquals("Is not in database", false, appCharge1.IsInDatabase);
			var appCharge2 = cost.ApportionmentCharges.AddNew();
			appCharge2.JR_GB = GlbBranch.CurrentBranch.PK;
			appCharge2.JR_GE = GlbDepartment.CurrentDepartment.PK;
			appCharge2.JR_JH = job2.PK;
			AssertEquals("Is not in database", false, appCharge2.IsInDatabase);
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			cost.E6_InvoiceNum = "12345";
			cost.E6_InvoiceDate = ZDateTime.Today.AddDays(1);
			cost.E6_PaymentDate = ZDateTime.Today.AddDays(2);
			cost.E6_CostReference = "COST";
			cost.E6_AT_TaxRate = ObjectCreator.GST1.PK;
			cost.E6_A9_VATClass = Factory.NewWithValidTestData<AccInvMsg>().PK;
			AssertEquals("SynchroniseInvoiceDetailsIfNecessary", false, cost.SynchroniseUnpostedInvoiceDetailsIfNecessary());
			appCharge1.JR_AT_CostGSTRate = ObjectCreator.GSTFREE1.PK;
			appCharge1.JR_A9_CostVATClass = Factory.NewWithValidTestData<AccInvMsg>().PK;
			appCharge2.JR_AT_CostGSTRate = ObjectCreator.GSTWithExtraRate.PK;
			appCharge2.JR_A9_CostVATClass = Factory.NewWithValidTestData<AccInvMsg>().PK;
			Assert("SynchroniseInvoiceDetailsIfNecessary", cost.SynchroniseUnpostedInvoiceDetailsIfNecessary());
			AssertEquals("ApportionmentCharges.Count", 2, cost.ApportionmentCharges.Count);
			AssertEquals(appCharge1.PK, cost.ApportionmentCharges[0].PK);
			AssertEquals("jobCharge1.JR_AT_CostGSTRate", cost.E6_AT_TaxRate, appCharge1.JR_AT_CostGSTRate);
			AssertEquals("jobCharge1.JR_A9_CostVATClass", cost.E6_A9_VATClass, appCharge1.JR_A9_CostVATClass);
			AssertEquals(appCharge2.PK, cost.ApportionmentCharges[1].PK);
			AssertEquals("jobCharge2.JR_AT_CostGSTRate", cost.E6_AT_TaxRate, appCharge2.JR_AT_CostGSTRate);
			AssertEquals("jobCharge2.JR_A9_CostVATClass", cost.E6_A9_VATClass, appCharge2.JR_A9_CostVATClass);
		}

		public void TestSyncConsolCostAndChargesFromInvoiceAlertFailedMessage_WhenConsolCostIsNotPostedAndNotAllChargesAreCostPosted()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol();
			consol.JK_RL_NKLoadPort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
			var shipment1 = creator.CreateShipment("S00001", consol);
			shipment1.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.ABIGAS.PK;
			var job1 = creator.CreateJob(shipment1);
			job1.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			var shipment2 = creator.CreateShipment("S00002", consol);
			shipment2.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
			var job2 = creator.CreateJob(shipment2);
			job2.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			var consolCost = creator.CreateConsolCost(consol, creator.CC1);
			consolCost.E6_OH_Creditor = creator.Creditor1.PK;
			consolCost.E6_AT_TaxRate = creator.FREECAPGST.PK;
			consolCost.E6_OSCostAmount = 10;
			consolCost.E6_InvoiceNum = "ABC123";
			consolCost.E6_InvoiceDate = ZDateTime.Now;

			creator.CreateTestPeriodsForEntireYear(2022);
			ConsolInvoicingPostManager postManager = new ConsolInvoicingPostManager(Factory, new[] { job1, job2 }, consol, new ApportionmentListing(Factory, consol));
			postManager.CreateTransactions(JobInvoicingPostingOption.ConsolCosts);
			Factory.Save();

			var charge1 = consolCost.ApportionmentCharges[0];
			var charge2 = consolCost.ApportionmentCharges[1];
			TestConnection.ExecuteNonQuery(@$"
UPDATE dbo.JobConsolCost
SET
	E6_SystemLastEditTimeUtc = GETUTCDATE(),
	E6_SystemLastEditUser = '~BP',
	E6_AH_APInvoice = NULL,
	E6_AT_TaxRate = NULL
WHERE
	E6_PK = '{consolCost.PK}';");
			TestConnection.ExecuteNonQuery($"UPDATE dbo.AccTransactionLines SET AL_LineType = 'ACR', AL_AH = NULL, AL_ReverseDate = NULL, AL_SystemLastEditTimeUtc = GETUTCDATE(), AL_SystemLastEditUser = 'TST' WHERE AL_PK = '{charge2.APLine.PK}';");

			var newFactory = NewFactory();
			consol = newFactory.Load<ForwardingConsol>(consol.PK);
			consolCost = consol.GetApportionments().CostsFilteredCollection[0];
			charge1 = consolCost.ApportionmentCharges.FirstOrDefault(n => n.PK == charge1.PK) as ApportionSplitCharge;
			charge2 = consolCost.ApportionmentCharges.FirstOrDefault(n => n.PK == charge2.PK) as ApportionSplitCharge;

			AssertEquals("Tax Rate between consol cost and charge is not equal", true, consolCost.E6_AT_TaxRate != charge1.JR_AT_CostGSTRate);
			AssertEquals("Not All charges are cost posted", false, charge1.JR_IsCostPosted && charge2.JR_IsCostPosted);
			AssertEquals("All charges are not revenue posted", false, charge1.JR_IsRevenuePosted || charge2.JR_IsRevenuePosted);
			AssertEquals("Not need sync as unposted", false, consolCost.SynchroniseUnpostedInvoiceDetailsIfNecessary());
			string message = string.Empty;
			AssertEquals("hasToSync when sync as posted: false", false, consolCost.SynchronisePostedInvoiceDetailsIfPossible(out message));
			AssertEquals("Alert failed message when sync as posted ", "Consol Cost with Charge Code ZZCC1, Invoice # ABC123 for Creditor ZCreditor1\r\n- Has Charge(s) not posted or posted to different Invoice.", message);

			var apLine1 = charge1.APLine;
			var apLine2 = charge2.APLine;
			AssertEquals("Before delete consol cost, consol cost is in database", true, consolCost.IsInDatabase);
			AssertEquals("Before delete consol cost, charge1 is in database", true, charge1.IsInDatabase);
			AssertEquals("Before delete consol cost, charge2 is in database", true, charge2.IsInDatabase);
			AssertEquals("Before delete consol cost, apLine1 is in database", true, apLine1.IsInDatabase);
			AssertEquals("Before delete consol cost, apLine2 is in database", true, apLine2.IsInDatabase);
			AssertEquals("Before delete consol cost, apLine2's AL_ReverseDate is empty", true, apLine2.AL_ReverseDate == ZDateTime.Empty);
			consolCost.Delete();
			newFactory.Save();

			consol.JK_AgentsReference = "ref123";
			AssertNoExceptionThrown("After sync and delete consol cost, changed consol can save correctly", newFactory.Save);

			AssertEquals("Should not report any error", string.Empty, ErrorReporter.LastMessageReported);
			AssertEquals("After delete consol cost, consol cost is not in database", false, consolCost.IsInDatabase);
			AssertEquals("After delete consol cost, charge1 is in database", true, charge1.IsInDatabase);
			AssertEquals("After delete consol cost, charge2 is not in database", false, charge2.IsInDatabase);
			AssertEquals("After delete consol cost, apLine1 is in database", true, apLine1.IsInDatabase);
			AssertEquals("After delete consol cost, apLine2 is in database", true, apLine2.IsInDatabase);
			AssertEquals("After delete consol cost, apLine2's AL_ReverseDate is not empty", true, apLine2.AL_ReverseDate != ZDateTime.Empty);
		}

		public void TestSyncConsolCostAndChargesFromInvoiceSucceed_WhenConsolCostIsNotPostedAndAllChargesAreCostPostedToSameInvoice()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol();
			consol.JK_RL_NKLoadPort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
			var shipment1 = creator.CreateShipment("S00001", consol);
			shipment1.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.ABIGAS.PK;
			var job1 = creator.CreateJob(shipment1);
			job1.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			var shipment2 = creator.CreateShipment("S00002", consol);
			shipment2.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
			var job2 = creator.CreateJob(shipment2);
			job2.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			var consolCost = creator.CreateConsolCost(consol, creator.CC1);
			consolCost.E6_OH_Creditor = creator.Creditor1.PK;
			consolCost.E6_AT_TaxRate = creator.FREECAPGST.PK;
			consolCost.E6_OSCostAmount = 10;
			consolCost.E6_InvoiceNum = "ABC123";
			consolCost.E6_InvoiceDate = ZDateTime.Now;

			creator.CreateTestPeriodsForEntireYear(2022);
			ConsolInvoicingPostManager postManager = new ConsolInvoicingPostManager(Factory, new[] { job1, job2 }, consol, new ApportionmentListing(Factory, consol));
			postManager.CreateTransactions(JobInvoicingPostingOption.ConsolCosts);
			Factory.Save();

			var charge1 = consolCost.ApportionmentCharges[0];
			var charge2 = consolCost.ApportionmentCharges[1];
			TestConnection.ExecuteNonQuery(@$"
UPDATE dbo.JobConsolCost
SET
	E6_SystemLastEditTimeUtc = GETUTCDATE(),
	E6_SystemLastEditUser = '~BP',
	E6_AH_APInvoice = NULL,
	E6_AT_TaxRate = NULL
WHERE
	E6_PK = '{consolCost.PK}';");

			var newFactory = NewFactory();
			consol = newFactory.Load<ForwardingConsol>(consol.PK);
			consolCost = consol.GetApportionments().CostsFilteredCollection[0];
			charge1 = consolCost.ApportionmentCharges.FirstOrDefault(n => n.PK == charge1.PK) as ApportionSplitCharge;
			charge2 = consolCost.ApportionmentCharges.FirstOrDefault(n => n.PK == charge2.PK) as ApportionSplitCharge;

			AssertEquals("Tax Rate between consol cost and charge is not equal", true, consolCost.E6_AT_TaxRate != charge1.JR_AT_CostGSTRate);
			AssertEquals("All charges are cost posted", true, charge1.JR_IsCostPosted && charge2.JR_IsCostPosted);
			AssertEquals("All charges are not revenue posted", false, charge1.JR_IsRevenuePosted || charge2.JR_IsRevenuePosted);
			AssertEquals("Not need sync as unposted", false, consolCost.SynchroniseUnpostedInvoiceDetailsIfNecessary());
			string message = string.Empty;
			AssertEquals("Before sync, consol cost is unposted", false, consolCost.IsPosted);
			AssertEquals("Need sync as posted", true, consolCost.SynchronisePostedInvoiceDetailsIfPossible(out message));
			AssertEquals("No error message when sync", true, string.IsNullOrEmpty(message));
			AssertEquals("After sync, consol cost is posted", true, consolCost.IsPosted);
			newFactory.Save();

			consol.JK_AgentsReference = "ref123";
			AssertNoExceptionThrown("After sync, changed consol can save correctly", newFactory.Save);

			AssertEquals("Should not report any error", string.Empty, ErrorReporter.LastMessageReported);
		}

		[TestDate(2021, 6, 30)]
		public void TestSyncChargeAndInvoice()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol();
			Factory.Save();
			var shipment = creator.CreateShipment("S001", consol);
			var job = creator.CreateJob(shipment);
			Factory.Save();
			var consolCost = creator.CreateConsolCost(consol, creator.FRT);
			consolCost.E6_AT_TaxRate = creator.FREEVAT.PK;
			consolCost.E6_LocalCostAmount = 100m;
			consolCost.E6_RX_NKCurrency = creator.AUD.Code;
			Factory.Save();

			var charge = consolCost.ApportionmentCharges[0];
			var transaction = creator.CreateAPInvoice<APInvoice>("I001", creator.AUD, 1m, 100m, 0m, 0m, 100m, 0m, 0m);
			transaction.Lines[0].AL_AT = creator.FREEVAT.PK;
			charge.ReverseAccrual(ZDate.Today);
			charge.JR_AL_APLine = transaction.Lines[0].PK;
			consolCost.E6_AH_APInvoice = transaction.PK;
			AssertEquals(ZDate.Today, transaction.Lines[0].AL_TaxDate);
			AssertEquals(ZDate.Empty, consolCost.E6_TaxDate);
			AssertEquals(ZDate.Empty, charge.JR_CostTaxDate);

			var message = string.Empty;
			consolCost.SynchronisePostedInvoiceDetailsIfPossible(out message);
			AssertEquals(ZDate.Today, consolCost.E6_TaxDate);
			AssertEquals(ZDate.Today, charge.JR_CostTaxDate);

			var costsToFix = consol.GetApportionments().CostsCollection.Where(x => !x.HasSynchronisedAPInvoiceDetails);
			AssertEquals(0, costsToFix.Count());
			AssertEquals(false, consolCost.SynchronisePostedInvoiceDetailsIfPossible(out message));

			charge.JR_CostTaxDate = ZDate.Empty;
			AssertEquals(1, costsToFix.Count());
			AssertEquals(true, consolCost.SynchronisePostedInvoiceDetailsIfPossible(out message));
			AssertEquals(ZDate.Today, charge.JR_CostTaxDate);
			AssertNullOrEmpty(message);
		}

		public void TestSynchroniseInvoiceDetailsForPostedCosts()
		{
			var creator = new TestObjectCreator(Factory);
			//Creating Consol
			var consol = creator.CreateConsol("", "", "TSTCNSL");
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUMEL";
			CommonShipment shipment1 = creator.CreateShipment("S00010001", "USLAX", "AUMEL");
			consol.Shipments.Add(shipment1);
			Job job1 = new Job.Loader(Factory, shipment1).TryCreateWithoutMutexForTestOnly();
			CommonShipment shipment2 = creator.CreateShipment("S00010002", "USLAX", "AUMEL");
			consol.Shipments.Add(shipment2);
			Job job2 = new Job.Loader(Factory, shipment2).TryCreateWithoutMutexForTestOnly();
			Factory.Save();
			//Creating Consol Cost
			var apportionmentListing = consol.GetApportionments();
			var cost = CreateConsolCost(apportionmentListing);
			//Creating Invoices
			var invoice1 = creator.CreateInvoice(typeof(APInvoice), creator.AUD, 1.0m, creator.AALSHI) as APInvoice;
			invoice1.AH_TransactionNum = "INV0001";
			var apline1 = creator.CreateAPInvoiceLine(invoice1, cost.ApportionmentCharges[0].Job as Job, cost.ChargeCode, creator.AUD, 1m, "Test Line 1", 100m);
			var invoice2 = creator.CreateInvoice(typeof(APInvoice), creator.AUD, 1.0m, creator.AALSHI) as APInvoice;
			invoice2.AH_TransactionNum = "INV0002";
			var apline2 = creator.CreateAPInvoiceLine(invoice2, cost.ApportionmentCharges[1].Job as Job, cost.ChargeCode, creator.AUD, 1m, "Test Line 2", 110m);
			var message = string.Empty;
			//Partial Posting :one apportioned charge is not linked to any Invoice
			cost.E6_AH_APInvoice = invoice1.PK;
			cost.ApportionmentCharges[0].JR_AL_APLine = invoice1.Lines[0].PK;
			Assert("SynchroniseInvoiceDetailsIfNecessary", !cost.SynchronisePostedInvoiceDetailsIfPossible(out message));
			Assert("SynchroniseInvoiceDetailsIfNecessary Message", !string.IsNullOrEmpty(message));
			AssertEquals(string.Format(@"Consol Cost with Charge Code {0}, Invoice # {1} for Creditor {2}
- Has Charge(s) not posted or posted to different Invoice.
- Has Charge(s) posted with different Tax Rate.", cost.ChargeCode.AC_Code, cost.E6_InvoiceNum, cost.Creditor.OH_Code), message);
			//Charges are Linked to different Invoices
			cost.E6_AH_APInvoice = invoice1.PK;
			cost.ApportionmentCharges[0].JR_AL_APLine = invoice1.Lines[0].PK;
			cost.ApportionmentCharges[1].JR_AL_APLine = invoice2.Lines[0].PK;
			AssertEquals("SynchroniseInvoiceDetailsIfNecessary", false, cost.SynchronisePostedInvoiceDetailsIfPossible(out message));
			AssertEquals("SynchroniseInvoiceDetailsIfNecessary Message", false, string.IsNullOrEmpty(message));
			AssertEquals(string.Format(@"Consol Cost with Charge Code {0}, Invoice # {1} for Creditor {2}
- Has Charge(s) not posted or posted to different Invoice.", cost.ChargeCode.AC_Code, cost.E6_InvoiceNum, cost.Creditor.OH_Code), message);
			//Cost is linked to different Invoice
			cost = CreateConsolCost(apportionmentListing);
			cost.E6_AH_APInvoice = invoice1.PK;
			cost.ApportionmentCharges[0].JR_AL_APLine = invoice2.Lines[0].PK;
			cost.ApportionmentCharges[1].JR_AL_APLine = invoice2.Lines[0].PK;
			Assert("SynchroniseInvoiceDetailsIfNecessary", cost.SynchronisePostedInvoiceDetailsIfPossible(out message));
			Assert("SynchroniseInvoiceDetailsIfNecessary Message", string.IsNullOrEmpty(message));
			AssertEquals("ApportionmentCharges.Count", 2, cost.ApportionmentCharges.Count);
			AssertSynchronizedConsolCost(invoice2, cost, invoice2.Lines[0].AL_AT, invoice2.Lines[0].AL_A9_VATClass);
			AssertSynchronizedApportionedCharges(invoice2, cost.ApportionmentCharges[0], invoice2.Lines[0]);
			AssertSynchronizedApportionedCharges(invoice2, cost.ApportionmentCharges[1], invoice2.Lines[0]);
			//Posting Cost
			//Cost and charges have same info But Invoice has different info so sync is required
			cost = CreateConsolCost(apportionmentListing);
			var invoice3 = creator.CreateInvoice(typeof(APInvoice), creator.AUD, 1.0m, creator.AALSHI) as APInvoice;
			invoice3.AH_TransactionNum = "INV0003";
			invoice3.AH_OH = creator.Creditor1.PK;
			invoice3.AH_InvoiceDate = ZDateTime.Today.AddDays(-1);
			invoice3.AH_DueDate = ZDateTime.Today.AddDays(5);
			invoice3.AH_TransactionReference = "NWREF#";
			apline1 = creator.CreateAPInvoiceLine(invoice3, cost.ApportionmentCharges[0].Job as Job, cost.ChargeCode, creator.AUD, 1m, "Test Line 1", 100m);
			apline2 = creator.CreateAPInvoiceLine(invoice3, cost.ApportionmentCharges[1].Job as Job, cost.ChargeCode, creator.AUD, 1m, "Test Line 2", 110m);
			cost.E6_AH_APInvoice = invoice3.PK;
			cost.ApportionmentCharges[0].JR_AL_APLine = invoice3.Lines[0].PK;
			cost.ApportionmentCharges[1].JR_AL_APLine = invoice3.Lines[1].PK;
			Assert("SynchroniseInvoiceDetailsIfNecessary", cost.SynchronisePostedInvoiceDetailsIfPossible(out message));
			Assert("SynchroniseInvoiceDetailsIfNecessary Message", string.IsNullOrEmpty(message));
			AssertEquals("ApportionmentCharges.Count", 2, cost.ApportionmentCharges.Count);
			AssertSynchronizedConsolCost(invoice3, cost, invoice3.Lines[0].AL_AT, invoice3.Lines[0].AL_A9_VATClass);
			AssertSynchronizedApportionedCharges(invoice3, cost.ApportionmentCharges[0], invoice3.Lines[0]);
			AssertSynchronizedApportionedCharges(invoice3, cost.ApportionmentCharges[1], invoice3.Lines[1]);
			//Nothing to Sync
			Assert("SynchroniseInvoiceDetailsIfNecessary", !cost.SynchronisePostedInvoiceDetailsIfPossible(out message));
			Assert("SynchroniseInvoiceDetailsIfNecessary Message", string.IsNullOrEmpty(message));
		}

		[TestDate(2021, 06, 29)]
		public virtual void TestDeleteHandling()
		{
			var creator = new TestObjectCreator(Factory);
			//Creating Consol
			var consol = creator.CreateConsol("", "", "TSTCNSL");
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUMEL";
			CommonShipment shipment1 = creator.CreateShipment("S00010001", "USLAX", "AUMEL");
			consol.Shipments.Add(shipment1);
			Job job1 = new Job.Loader(Factory, shipment1).TryCreateWithoutMutexForTestOnly();
			Factory.Save();
			//Creating Cost
			var apportionmentListing = consol.GetApportionments();
			var cost = CreateConsolCost(apportionmentListing);
			//Creating Invoice
			var invoice = creator.CreateInvoice(typeof(APInvoice), creator.AUD, 1.0m, creator.AALSHI) as APInvoice;
			invoice.AH_TransactionNum = "ABC123";
			invoice.AH_OH = creator.AALSHI.PK;
			invoice.AH_InvoiceDate = ZDateTime.Today;
			invoice.AH_DueDate = ZDateTime.Today.AddDays(2);
			invoice.AH_TransactionReference = "COST";
			var apLine1 = creator.CreateAPInvoiceLine(invoice, cost.ApportionmentCharges[0].Job as Job, cost.ChargeCode, creator.AUD, 1m, "Test Line 1", 100m);
			apLine1.AL_AT = cost.ApportionmentCharges[0].JR_AT_CostGSTRate;
			apLine1.AL_A9_VATClass = cost.ApportionmentCharges[0].JR_A9_CostVATClass;
			//Posting
			cost.E6_AH_APInvoice = invoice.PK;
			cost.E6_TaxDate = ZDate.Today;
			cost.ApportionmentCharges[0].JR_AL_APLine = invoice.Lines[0].PK;
			Assert(cost.IsPostedCorrectly);
			try
			{
				cost.Delete();
			}
			catch (Exception e)
			{
				Assert("Cannot delete consol cost because it is posted", e is CannotDeleteException);
			}
		}

		[TestDate(2021, 06, 29)]
		[SuspendCriticalValidation]
		public void TestTaxIdIsNotModifiedForPostedCost()
		{
			var creator = new TestObjectCreator(Factory);
			//Creating Consol
			var consol = creator.CreateConsol("", "", "TSTCNSL");
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUMEL";
			CommonShipment shipment1 = creator.CreateShipment("S00010001", "USLAX", "AUMEL");
			CommonShipment shipment2 = creator.CreateShipment("S00010002", "USLAX", "AUMEL");
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);
			new Job.Loader(Factory, shipment1).TryCreateWithoutMutexForTestOnly();
			new Job.Loader(Factory, shipment2).TryCreateWithoutMutexForTestOnly();

			var taxOverride = creator.CC1.TaxOverrides.AddNew();
			taxOverride.AO_AT = creator.GSTFREE1.PK;
			taxOverride.AO_TaxRegCntryOrGroup = "ALL";
			taxOverride.AO_JobType = "ALL";
			taxOverride.AO_Direction = "ALL";
			taxOverride.AO_Destination = "ALL";
			taxOverride.AO_CostSellAll = "COS";
			taxOverride.AO_HomeCountryOrZone = "ALL";
			taxOverride.AO_Origin = "ALL";
			taxOverride.AO_Destination = "ALL";
			// AccChargeTaxOverride is not valid with empty AO_IncoTerm.
			taxOverride.AO_IncoTerm = "ALL";
			Factory.Save();

			//Creating Cost
			var apportionmentListing = consol.GetApportionments();
			var cost = CreateConsolCost(apportionmentListing);
			cost.E6_AT_TaxRate = creator.GST1.PK;
			cost.E6_IsTaxAmountOverridden = true;
			Factory.Save();

			//Creating Invoice
			var invoice = creator.CreateInvoice(typeof(APInvoice), creator.AUD, 1.0m, creator.AALSHI) as APInvoice;
			invoice.AH_TransactionNum = "ABC123";
			invoice.AH_OH = creator.AALSHI.PK;
			invoice.AH_InvoiceDate = ZDateTime.Today;
			invoice.AH_DueDate = ZDateTime.Today.AddDays(2);
			invoice.AH_TransactionReference = "COST";
			var apLine1 = creator.CreateAPInvoiceLine(invoice, cost.ApportionmentCharges[0].Job as Job, cost.ChargeCode, creator.AUD, 1m, "Test Line 1", 50m);
			apLine1.AL_AT = cost.ApportionmentCharges[0].JR_AT_CostGSTRate;
			apLine1.AL_A9_VATClass = cost.ApportionmentCharges[0].JR_A9_CostVATClass;
			var apLine2 = creator.CreateAPInvoiceLine(invoice, cost.ApportionmentCharges[1].Job as Job, cost.ChargeCode, creator.AUD, 1m, "Test Line 2", 50m);
			apLine2.AL_AT = cost.ApportionmentCharges[1].JR_AT_CostGSTRate;
			apLine2.AL_A9_VATClass = cost.ApportionmentCharges[1].JR_A9_CostVATClass;
			//Posting
			cost.E6_AH_APInvoice = invoice.PK;
			cost.E6_TaxDate = ZDate.Today;
			cost.ApportionmentCharges[0].JR_AL_APLine = invoice.Lines[0].PK;
			cost.ApportionmentCharges[1].JR_AL_APLine = invoice.Lines[1].PK;
			Assert(cost.IsPostedCorrectly);
			Factory.Save();

			ExceptionReporterTestListener.Instance.Clear();
			cost.E6_OH_Creditor = creator.AALSHI.PK;
			AssertEquals(creator.GST1.PK, cost.E6_AT_TaxRate);
			Assert("Modifying Tax Rate on Posted Charge... error should not be reported", !ErrorReporter.LastExceptionsReported().Any(x => x.Contains("Modifying Tax Rate on Posted Charge")));
		}

		[ExpectNoExceptions("No CargoWise.EntityFramework.ZSaveException should occur")]
		public void TestDeleteHandlingWithInvalidChargeCode()
		{
			var creator = new TestObjectCreator(Factory);
			//Creating Consol
			var consol = creator.CreateConsol("", "", "TSTCNSL");
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUMEL";
			CommonShipment shipment1 = creator.CreateShipment("S00010001", "USLAX", "AUMEL");
			consol.Shipments.Add(shipment1);
			Job job1 = new Job.Loader(Factory, shipment1).TryCreateWithoutMutexForTestOnly();
			Factory.Save();
			//Creating Cost
			var apportionmentListing = consol.GetApportionments();
			var cost = CreateConsolCost(apportionmentListing);
			Factory.Save();
			AssertEquals("ApprotionmentListing should be Empty", 1, apportionmentListing.CostsCollection.Count);
			cost.E6_AC_ChargeCode = ZGuid.Empty;
			cost.Delete();
			Factory.Save();
			apportionmentListing = consol.GetApportionments();
			AssertEquals("ApprotionmentListing should be Empty", 0, apportionmentListing.CostsCollection.Count);
		}

		public void TestDeleteAlsoDeletesChargesWhenParentCollectionIsFromAConsol()
		{
			TestObjectCreator objectCreator = new TestObjectCreator(Factory);
			var consol = objectCreator.CreateConsol("AUSYD", "AUMEL", "001");
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			var consolCost = objectCreator.CreateConsolCost(consol, objectCreator.CC1, objectCreator.AALSHI);
			consolCost.E6_OSCostAmount = 100;
			Factory.Save();
			consol.Reload();
			Assert("Some charges are created which must be deleted also", ((Job)consol.Shipments[0].Job).Charges.Count > 0);
			consolCost.Delete();
			Factory.Save();
			consol.Reload();
			Assert("Charges are deleted", ((Job)consol.Shipments[0].Job).Charges.Count == 0);
		}

		public void TestDeleteCostAndChargesOnConsol()
		{
			TestObjectCreator objectCreator = new TestObjectCreator(Factory);
			var consol = objectCreator.CreateConsol("AUSYD", "AUMEL", "001");
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			var consolCost = objectCreator.CreateConsolCost(consol, objectCreator.CC1, objectCreator.AALSHI);
			consolCost.E6_OSCostAmount = 100;
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var consolCostReloadedOutsideOfCollection = factory2.Load<JobConsolCost>(consolCost.PK);
			var jobReloaded = factory2.Load<Job>(consol.Shipments[0].Job.PK);
			Assert("Some charges are created which must be deleted also", jobReloaded.Charges.Count > 0);
			consolCostReloadedOutsideOfCollection.DeleteCostAndCharges();
			factory2.Save();
			jobReloaded.Reload();
			Assert("Charges are deleted", jobReloaded.Charges.Count == 0);
		}

		public void TestDeleteCostAndChargesOnConsolAlsoDeletesPaymentBases()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var consol = objectCreator.CreateConsol("AUSYD", "AUMEL", "001");
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			var consolCost = objectCreator.CreateConsolCost(consol, objectCreator.CC1, objectCreator.AALSHI);
			consolCost.E6_OSCostAmount = 100;
			var pbs1 = consolCost.PaymentBases.AddNew();
			pbs1.PBS_AdapterID = "A1";
			pbs1.PBS_AdapterType = "T1";
			pbs1.PBS_ChargeableAmount = 5m;
			pbs1.PBS_ChargeableUnit = "KG";
			pbs1.PBS_PerUnitRate = 20m;
			pbs1.PBS_RX_NKRateCurrency = "AUD";
			pbs1.PBS_RateUnit = "KG";
			pbs1.PBS_RateUnitType = "Weight";

			var pbs2 = consolCost.PaymentBases.AddNew();
			pbs2.PBS_AdapterID = "A1";
			pbs2.PBS_AdapterType = "T1";
			pbs2.PBS_ChargeableAmount = 5m;
			pbs2.PBS_ChargeableUnit = "KG";
			pbs2.PBS_PerUnitRate = 10m;
			pbs2.PBS_RX_NKRateCurrency = "AUD";
			pbs2.PBS_RateUnit = "KG";
			pbs2.PBS_RateUnitType = "Weight";

			Factory.Save();

			var consolCostPK = consolCost.PK;
			var factory2 = new BusinessObjectFactory();
			var consolCostReloadedOutsideOfCollection = factory2.Load<JobConsolCost>(consolCost.PK);
			AssertEquals("Should have Payment Bases", 2, consolCostReloadedOutsideOfCollection.PaymentBases.Count);
			consolCostReloadedOutsideOfCollection.DeleteCostAndCharges();
			factory2.Save();

			var paymentBases = factory2.Load<JobPaymentBasis>(new ZQuery(JobPaymentBasisSchema.PBS_E6, consolCostPK));
			AssertEquals("Should not have any payment bases", 0, paymentBases.Length);
		}

		public void TestConsolChangedSuspender()
		{
			JobConsolCost cost = (JobConsolCost)GetNewBusinessObject();
			ConsolChangedSuspender suspender = cost.GetConsolChangedSuspender();
			Assert("Consol Changed should be suspended", cost.IsConsolChangedSuspended);
			((IDisposable)suspender).Dispose();
			Assert("Consol Changed should be suspended", !cost.IsConsolChangedSuspended);
		}

		public void TestApportionmentCharges()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "SGSIN";
			Factory.Save();
			var shipment1 = Factory.New<ForwardingShipment>();
			var shipment2 = Factory.New<ForwardingShipment>();
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);
			var cost = GetCost(consol);
			try
			{
				if (IsUsedToAcquireJobMutexes)
				{
					AssertEquals(2, cost.ApportionmentCharges.Count);
					AssertNotNull(FindChargeForJob(cost, shipment1));
					AssertNotNull(FindChargeForJob(cost, shipment2));
				}
				else
				{
					AssertEquals(0, cost.ApportionmentCharges.Count);
					AssertNull(FindChargeForJob(cost, shipment1));
					AssertNull(FindChargeForJob(cost, shipment2));
				}
			}
			finally
			{
				cost.CalculationStrategy.ReleaseMutexes();
			}
		}

		protected virtual bool IsUsedToAcquireJobMutexes
		{
			get
			{
				return true;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			ForwardingConsol consol = factory.New<ForwardingConsol>();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			factory.Save();
			JobConsolCost result = GetCost(consol);
			result.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			result.CalculationStrategy.ReleaseMutexes();
			return result;
		}

		public override void TestBizObjectFields()
		{
			Factory.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				base.TestBizObjectFields();
			}
			finally
			{
				Factory.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}
		}

		public void TestApportionAmount()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "SGSIN";
			Factory.Save();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 351.73m;
			shipment1.JS_ActualChargeable = 40m;
			shipment1.OuterPackLines[0].JL_PackageCount = 2;
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ActualWeight = 786.23m;
			shipment2.JS_ActualChargeable = 60m;
			shipment2.OuterPackLines[0].JL_PackageCount = 3;
			var cost = GetCost(consol);
			try
			{
				var charge1 = FindChargeForJob(cost, shipment1);
				var charge2 = FindChargeForJob(cost, shipment2);
				RatingDataRegistry.Instance.UnspecifiedCostShouldForceZeroToBePulledThrough.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				cost.E6_OSCostAmount = 200m;
				cost.E6_ApportionmentMethod = AllocationMethod.Shipment;
				AssertEquals(100m, charge1.JR_OSCostAmt);
				AssertEquals(100m, charge2.JR_OSCostAmt);
				AssertEquals(100m, charge1.JR_AgentDeclaredCostAmt);
				AssertEquals(100m, charge2.JR_AgentDeclaredCostAmt);
				RatingDataRegistry.Instance.UnspecifiedCostShouldForceZeroToBePulledThrough.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				cost.E6_OSCostAmount = 200m;
				cost.E6_ApportionmentMethod = AllocationMethod.Shipment;
				AssertEquals(100m, charge1.JR_OSCostAmt);
				AssertEquals(100m, charge2.JR_OSCostAmt);
				AssertEquals(100m, charge1.JR_AgentDeclaredCostAmt);
				AssertEquals(100m, charge2.JR_AgentDeclaredCostAmt);
				cost.AgentDeclaredOSAmount = 300m;
				cost.E6_ApportionmentMethod = AllocationMethod.Shipment;
				AssertEquals(100m, charge1.JR_OSCostAmt);
				AssertEquals(100m, charge2.JR_OSCostAmt);
				AssertEquals(100m, charge1.JR_AgentDeclaredCostAmt);
				AssertEquals(100m, charge2.JR_AgentDeclaredCostAmt);
				cost.E6_ApportionmentMethod = AllocationMethod.ChargeableUnits;
				AssertEquals(80m, charge1.JR_OSCostAmt);
				AssertEquals(120m, charge2.JR_OSCostAmt);
				AssertEquals(80m, charge1.JR_AgentDeclaredCostAmt);
				AssertEquals(120m, charge2.JR_AgentDeclaredCostAmt);
				cost.E6_ApportionmentMethod = AllocationMethod.Manual;
				AssertEquals(80m, charge1.JR_OSCostAmt);
				AssertEquals(120m, charge2.JR_OSCostAmt);
				AssertEquals(80m, charge1.JR_AgentDeclaredCostAmt);
				AssertEquals(120m, charge2.JR_AgentDeclaredCostAmt);
				cost.E6_ApportionmentMethod = AllocationMethod.GrossWeight;
				AssertEquals(61.82m, charge1.JR_OSCostAmt);
				AssertEquals(138.18m, charge2.JR_OSCostAmt); // This also tests redistribution of amounts that don't exactly apportion
				AssertEquals(61.82m, charge1.JR_AgentDeclaredCostAmt);
				AssertEquals(138.18m, charge2.JR_AgentDeclaredCostAmt);
				cost.E6_ApportionmentMethod = AllocationMethod.OuterPackTotal;
				AssertEquals(80m, charge1.JR_OSCostAmt);
				AssertEquals(120m, charge2.JR_OSCostAmt);
				AssertEquals(80m, charge1.JR_AgentDeclaredCostAmt);
				AssertEquals(120m, charge2.JR_AgentDeclaredCostAmt);
			}
			finally
			{
				cost.CalculationStrategy.ReleaseMutexes();
			}
		}

		public void TestApportionForeignCurrencyAmounts()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "SGSIN";
			Factory.Save();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 351.73m; // 31.91858 percent of total weight
			shipment1.JS_ActualChargeable = 40m;
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ActualWeight = 786.23m; // Total 1137.96
			shipment2.JS_ActualChargeable = 60m;
			var cost = GetCost(consol);
			try
			{
				var charge1 = FindChargeForJob(cost, shipment1);
				var charge2 = FindChargeForJob(cost, shipment2);
				cost.E6_OSCostAmount = 103.61m;
				cost.E6_RX_NKCurrency = "USD";
				cost.E6_ExchangeRate = 0.3333m;
				AssertEquals("Local Value should equal exact conversion", 310.86m, cost.E6_LocalCostAmount);
				cost.E6_LocalCostAmount = 310.87m;
				AssertEquals("OS Value should stay the same", 103.61m, cost.E6_OSCostAmount);
				AssertEquals("Ex Rate should stay the same", 0.33329m, cost.E6_ExchangeRate);
				cost.E6_ApportionmentMethod = AllocationMethod.Shipment;
				AssertEquals(51.80m, charge1.JR_OSCostAmt);
				AssertEquals(155.42m, charge1.JR_LocalCostAmt);
				AssertEquals(51.81m, charge2.JR_OSCostAmt);
				AssertEquals(155.45m, charge2.JR_LocalCostAmt);
				cost.E6_ApportionmentMethod = AllocationMethod.ChargeableUnits;
				AssertEquals(41.44m, charge1.JR_OSCostAmt);
				AssertEquals(124.34m, charge1.JR_LocalCostAmt);
				AssertEquals(62.17m, charge2.JR_OSCostAmt);
				AssertEquals(186.53m, charge2.JR_LocalCostAmt);
				cost.E6_ApportionmentMethod = AllocationMethod.Manual;
				AssertEquals(41.44m, charge1.JR_OSCostAmt);
				AssertEquals(124.34m, charge1.JR_LocalCostAmt);
				AssertEquals(62.17m, charge2.JR_OSCostAmt);
				AssertEquals(186.53m, charge2.JR_LocalCostAmt);
				cost.E6_ApportionmentMethod = AllocationMethod.GrossWeight;
				AssertEquals(32.02m, charge1.JR_OSCostAmt);
				AssertEquals(96.07m, charge1.JR_LocalCostAmt);
				AssertEquals(71.59m, charge2.JR_OSCostAmt); // This also tests redistribution of amounts that don't exactly apportion
				AssertEquals(214.80m, charge2.JR_LocalCostAmt);
			}
			finally
			{
				cost.CalculationStrategy.ReleaseMutexes();
			}
		}

		public void TestApportionDepartmentOverridden()
		{
			TestObjectCreator objectCreator = new TestObjectCreator(Factory);
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			AssertNotNull(cost);
			cost.E6_AC_ChargeCode = objectCreator.CC1.PK;
			cost.E6_OSCostAmount = 123.34m;
			AssertEquals("Should one new charge be created", 1, cost.ApportionmentCharges.Count);
			cost.ApportionmentCharges[0].JR_GE = objectCreator.NonCurrentDepartment.PK;
			cost.ApportionmentCharges[0].JR_GB = objectCreator.NonCurrentBranch.PK;
			Factory.Save();
			AssertEquals("Apportionment department should be overridden", cost.ApportionmentCharges[0].JR_GE, objectCreator.NonCurrentDepartment.PK);
			AssertEquals("Apportionment branch should be overridden", cost.ApportionmentCharges[0].JR_GB, objectCreator.NonCurrentBranch.PK);
		}

		public void TestE6_OSCostAmount_ShouldValidateUnApportionedAmount()
		{
			var consolCost = Factory.New<JobConsolCost>();
			Assert(!consolCost.UnApportionedAmountInfo.HasErrors());

			consolCost.E6_OSCostAmount = 10m;
			Assert(consolCost.UnApportionedAmountInfo.HasError("Please ensure that this Cost Amount is fully apportioned."));

			consolCost.E6_OSCostAmount = 0m;
			Assert(!consolCost.UnApportionedAmountInfo.HasErrors());

			consolCost.SuspendValidation();
			consolCost.E6_OSCostAmount = 10m;
			Assert(!consolCost.UnApportionedAmountInfo.HasErrors());
		}

		public void TestApportionForManualAllocationMethod()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "SGSIN";
			Factory.Save();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 351.73m;
			shipment1.JS_ActualChargeable = 40m;
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ActualWeight = 786.23m;
			shipment2.JS_ActualChargeable = 60m;
			var cost = GetCost(consol);
			var charge1 = FindChargeForJob(cost, shipment1);
			var charge2 = FindChargeForJob(cost, shipment2);
			cost.E6_OSCostAmount = 200m;
			cost.E6_ApportionmentMethod = AllocationMethod.Shipment;
			AssertEquals(100m, charge1.JR_OSCostAmt);
			AssertEquals(100m, charge2.JR_OSCostAmt);
			cost.E6_ApportionmentMethod = AllocationMethod.Manual;
			AssertEquals(100m, charge1.JR_OSCostAmt);
			AssertEquals(100m, charge2.JR_OSCostAmt);
			charge1.JR_OSCostAmt = 90M;
			AssertEquals(10M, cost.UnApportionedAmount);
			cost.E6_OSCostAmount = 180m;
			AssertEquals(-10M, cost.UnApportionedAmount);
			AssertEquals(90m, charge1.JR_OSCostAmt);
			AssertEquals(100m, charge2.JR_OSCostAmt);
			charge2.JR_OSCostAmt = 90M;
			AssertEquals(0M, cost.UnApportionedAmount);
			AssertEquals(90m, charge1.JR_OSCostAmt);
			AssertEquals(90m, charge2.JR_OSCostAmt);
			cost.E6_OSCostAmount = 200m;
			AssertEquals(100m, charge1.JR_OSCostAmt);
			AssertEquals(100m, charge2.JR_OSCostAmt);
			foreach (Job job in cost.JobsWithMutexes)
			{
				job.Dispose();
			}
		}

		ApportionSplitCharge FindChargeForJob(JobConsolCost cost, ForwardingShipment shipment)
		{
			foreach (ApportionSplitCharge charge in cost.ApportionmentCharges)
			{
				if (charge.Job.JH_ParentID == shipment.PK)
				{
					charge.JR_IsUsedForApportionment = true;
					return charge;
				}
			}

			return null;
		}

		protected abstract JobConsolCost GetCost(IJobCostingPlugIn consol);
		public void TestApportionGSTAmount()
		{
			TestObjectCreator objectCreator = new TestObjectCreator(Factory);
			OrgHeader costAccount = objectCreator.AALSHI;
			AccTaxRate gSTRate = objectCreator.CreateTaxRate("GSTFREE", "GST Free", 10);
			AccInvMsg msg1 = objectCreator.TaxMsg1;
			gSTRate.AT_A9_DefaultVatClass = msg1.PK;
			AccChargeCode disbursementChargeCode = objectCreator.CreateChargeCode("DSB", "Description", Core.Constants.ChargeType.Disbursement, 100.00m, gSTRate, null, "ALL");
			AccChargeCode mRGChargeCode = objectCreator.CreateChargeCode("MRG", "Description", Core.Constants.ChargeType.Margin, 100.00m, gSTRate, null, "ALL");
			RefCurrency uSD = objectCreator.LocalCurrency;
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			ZGuid consolPK = consol.PK;
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();
				AssertEquals("Should be 2 charges in list", 2, cost.ApportionmentCharges.Count);
				cost.E6_AC_ChargeCode = disbursementChargeCode.PK;
				cost.E6_OH_Creditor = costAccount.PK;
				cost.E6_ApportionmentMethod = "SHP";
				AssertEquals("Should have defaulted GST rate on apportionment", gSTRate.PK, cost.E6_AT_TaxRate);
				AssertEquals("Should have defaulted Tax Msg on apportionment", msg1.PK, cost.E6_A9_VATClass);
				cost.E6_OSCostAmount = 100m;
				AssertEquals("Amount Should be 50 on first line", 50m, cost.ApportionmentCharges[0].JR_OSCostAmt);
				AssertEquals("GST should be 5 on first line", 5m, cost.ApportionmentCharges[0].JR_OSCostGSTAmt_Calc);
				AssertEquals("Amount Should be 50 on second line", 50m, cost.ApportionmentCharges[1].JR_OSCostAmt);
				AssertEquals("GST should be 5 on second line", 5m, cost.ApportionmentCharges[1].JR_OSCostGSTAmt_Calc);
				cost.E6_AC_ChargeCode = mRGChargeCode.PK;
				cost.E6_OSCostAmount = 100m;
				AssertEquals("GST rate should be defaulted for both lines", gSTRate.PK, cost.ApportionmentCharges[0].JR_AT_CostGSTRate);
				AssertEquals("Tax Msg should be defaulted for both lines", msg1.PK, cost.ApportionmentCharges[0].JR_A9_CostVATClass);
				AssertEquals("GST Amount should be defaulted for both lines", 5m, cost.ApportionmentCharges[0].JR_OSCostGSTAmt_Calc);
				AssertEquals("GST rate should be defaulted for both lines", gSTRate.PK, cost.ApportionmentCharges[1].JR_AT_CostGSTRate);
				AssertEquals("Tax Msg should be defaulted for both lines", msg1.PK, cost.ApportionmentCharges[1].JR_A9_CostVATClass);
				AssertEquals("GST Amount should be defaulted for both lines", 5m, cost.ApportionmentCharges[1].JR_OSCostGSTAmt_Calc);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestChangingCreditorForcesCostGSTToApportion()
		{
			TestObjectCreator objectCreator = new TestObjectCreator(Factory);
			OrgHeader costAccount = objectCreator.AALSHI;
			OrgHeader otherCostAccount = objectCreator.ABIGAS;
			otherCostAccount.CompanyData.SetAPTaxApplicable(true);
			AccTaxRate gSTRate = objectCreator.CreateTaxRate("GST", "GST", 10);
			AccTaxRate gSTFreeRate = objectCreator.CreateTaxRate("GSTFREE", "GST Free", 0);
			AccInvMsg msg1 = objectCreator.TaxMsg1;
			AccInvMsg msg2 = objectCreator.TaxMsg2;
			gSTRate.AT_A9_DefaultVatClass = msg1.PK;
			AccChargeCode mRGChargeCode = objectCreator.CreateChargeCode("MRG", "Description", Core.Constants.ChargeType.Margin, 100.00m, gSTRate, null, "ALL");
			AccChargeTaxOverride taxOverride = mRGChargeCode.TaxOverrides.AddNew();
			taxOverride.AO_AT = gSTFreeRate.PK;
			taxOverride.AO_A9_DefaultVATClass = msg2.PK;
			taxOverride.AO_CostSellAll = "COS";
			taxOverride.AO_Direction = "EXP";
			taxOverride.AO_IncoTerm = "ALL";
			taxOverride.AO_JobType = "SHP";
			taxOverride.AO_Origin = "AU";
			taxOverride.AO_Destination = "US";
			taxOverride.AO_TaxRegCntryOrGroup = "ALL";
			RefCurrency uSD = objectCreator.LocalCurrency;
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USCHI";
			Factory.Save();
			ZGuid consolPK = consol.PK;
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			shipment1.JS_RL_NKOrigin = shipment2.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = shipment2.JS_RL_NKDestination = "USCHI";
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();
				AssertEquals("Should be 2 charges in list", 2, cost.ApportionmentCharges.Count);
				cost.E6_AC_ChargeCode = mRGChargeCode.PK;
				cost.E6_OH_Creditor = costAccount.PK;
				cost.E6_ApportionmentMethod = "SHP";
				AssertEquals("Should have defaulted GST rate on apportionment", gSTRate.PK, cost.E6_AT_TaxRate);
				AssertEquals("Should have defaulted Tax Msg on apportionment", msg1.PK, cost.E6_A9_VATClass);
				cost.E6_OSCostAmount = 100m;
				AssertEquals("Amount Should be 50 on first line", 50m, cost.ApportionmentCharges[0].JR_OSCostAmt);
				AssertEquals("GST should be 5 on first line", 5m, cost.ApportionmentCharges[0].JR_OSCostGSTAmt_Calc);
				AssertEquals("Amount Should be 50 on second line", 50m, cost.ApportionmentCharges[1].JR_OSCostAmt);
				AssertEquals("GST should be 5 on second line", 5m, cost.ApportionmentCharges[1].JR_OSCostGSTAmt_Calc);
				Factory.Save();
				AssertEquals("Amount Should be 50 on first line", 50m, cost.ApportionmentCharges[0].JR_OSCostAmt);
				AssertEquals("GST should be 5 on first line", 5m, cost.ApportionmentCharges[0].JR_OSCostGSTAmt_Calc);
				AssertEquals("Amount Should be 50 on second line", 50m, cost.ApportionmentCharges[1].JR_OSCostAmt);
				AssertEquals("GST should be 5 on second line", 5m, cost.ApportionmentCharges[1].JR_OSCostGSTAmt_Calc);
				cost.E6_OSCostAmount = 200m;
				cost.E6_OH_Creditor = otherCostAccount.PK;
				AssertEquals("GST rate should be defaulted for both lines", gSTRate.PK, cost.ApportionmentCharges[0].JR_AT_CostGSTRate);
				AssertEquals("Tax Msg should be defaulted for both lines", msg1.PK, cost.ApportionmentCharges[0].JR_A9_CostVATClass);
				AssertEquals("GST Amount should be defaulted for both lines", 10m, cost.ApportionmentCharges[0].JR_OSCostGSTAmt_Calc);
				AssertEquals("GST rate should be defaulted for both lines", gSTRate.PK, cost.ApportionmentCharges[1].JR_AT_CostGSTRate);
				AssertEquals("Tax Msg should be defaulted for both lines", msg1.PK, cost.ApportionmentCharges[1].JR_A9_CostVATClass);
				AssertEquals("GST Amount should be defaulted for both lines", 10m, cost.ApportionmentCharges[1].JR_OSCostGSTAmt_Calc);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestForeignExchangeCalculations()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
				cost.E6_OSCostAmount = 100m;
				AssertEquals("Local amount should be 100", 100m, cost.E6_LocalCostAmount);
				cost.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
				AssertNotNull("GST Rate should be set", cost.TaxRate);
				AssertEquals("GST Amount should be set", 10m, cost.E6_OSGSTAmount_Calc);
				cost.CostExchangeRate.Currency = "USD";
				cost.CostExchangeRate.Rate = 0.6173m;
				AssertEquals("OS Cost Amount", 100m, cost.E6_OSCostAmount);
				AssertEquals("OS Cost GST Amount", 10m, cost.E6_OSGSTAmount_Calc);
				AssertEquals("Local Cost Amount", 162m, cost.E6_LocalCostAmount);
				cost.E6_LocalCostAmount = 193.37m;
				AssertEquals("Exchange Rate should change", 0.517143m, cost.E6_ExchangeRate);
				AssertEquals("OS Cost Amount", 100m, cost.E6_OSCostAmount);
				AssertEquals("OS Cost GST Amount", 10m, cost.E6_OSGSTAmount_Calc);
				AssertEquals("Local Cost Amount", 193.37m, cost.E6_LocalCostAmount);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestOSTaxIsEqualToApportionedTaxWithCurrencyChange()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USCHI";
			AccTaxRate gSTRate = ObjectCreator.CreateTaxRate("GST", "GST", 10);

			Factory.Save();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USCHI";
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();
				AssertEquals("Should be 1 charge in list", 1, cost.ApportionmentCharges.Count);
				cost.E6_AC_ChargeCode = ObjectCreator.FRT.PK;
				cost.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
				cost.E6_RX_NKCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_SubUnitRatio, 100)).Code;
				AssertEquals("2 dec pts", 2, cost.CurrencyDecimals);
				cost.E6_ApportionmentMethod = "SHP";
				cost.E6_AT_TaxRate = gSTRate.PK;

				cost.E6_OSCostAmount = 11m;
				AssertEquals("Amount Should be 11 on consol", 11m, cost.E6_OSCostAmount);
				AssertEquals("Tax should be 1.1 on consol", 1.1m, cost.E6_OSGSTAmount_Calc);

				AssertEquals("Amount Should be 11 on first line", 11m, cost.ApportionmentCharges[0].JR_OSCostAmt);
				AssertEquals("Tax should be 1.1 on first line", 1.1m, cost.ApportionmentCharges[0].JR_OSCostGSTAmt_Calc);

				cost.E6_RX_NKCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_SubUnitRatio, 1)).Code;
				AssertEquals("0 dec pts", 0, cost.CurrencyDecimals);

				AssertEquals("Amount Should be 11 on consol", 11m, cost.E6_OSCostAmount);
				AssertEquals("Tax should be 1 on consol", 1m, cost.E6_OSGSTAmount_Calc);

				AssertEquals("Amount Should be 11 on first line", 11m, cost.ApportionmentCharges[0].JR_OSCostAmt);
				AssertEquals("Tax should be 1 on first line", 1m, cost.ApportionmentCharges[0].JR_OSCostGSTAmt_Calc);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestGetPropertiesRequiringRoundingConsolCost()
		{
			var zDecimalPropertiesNotRequiringUpdateWithOSCurrencyChange = new[] {
					nameof(JobConsolCost.E6_ExchangeRate),
					nameof(JobConsolCost.AgentDeclaredOSAmount),
					nameof(JobConsolCost.E6_LocalCostAmount)
			};

			var consol = Factory.NewWithValidTestData<JobConsolCost>();
			var foundProperties = zDecimalPropertiesNotRequiringUpdateWithOSCurrencyChange.Append(consol.OSPropertiesRequiringRounding().ToArray());

			var allDecimalProperties = GetExpectedBusinessObjectType().GetProperties().Where(p => p.PropertyType == typeof(ZDecimal) && p.CanWrite).Select(p => p.Name);

			AssertContainsExactElementsInAnyOrder(string.Format(
@"There is a decimal property in consol cost which may need rounding, please add it to {0} or {1} in the relevant class.
Please consider whether or not your property will be affected by other rounding/setting logic in other properties.
For example, if a setter is called due to currecy changing, but detects no change to its own value, it may not bother to propagate the rounding change to the other properties it needs to.
If you are including a check whether property has changes or not please use method {2} in {3}.
", nameof(JobConsolCost.OSPropertiesRequiringRounding), nameof(zDecimalPropertiesNotRequiringUpdateWithOSCurrencyChange), nameof(AccountingValuesRoundingHelper.PropertyHasChanges), nameof(AccountingValuesRoundingHelper)), foundProperties, allDecimalProperties);
		}

		public void TestIncludeOnAgentInvoice_DiffrentLoginBranchCountry()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("CNAAT", "KRSEL", "C00001");
			consol.SetDefaultSendingForwarderAddress(ObjectCreator.ABIGAS);
			consol.SetDefaultReceivingForwarderAddress(ObjectCreator.AALSHI);
			Factory.Save();
			consol.Shipments.AddNew();
			var apps = new ApportionmentListing(Factory, consol);
			try
			{
				var cost = apps.CostsCollection.TryAddNew();
				cost.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
				cost.E6_IsForCollectInvoice = true;
				Assert(!consol.IsLoadPortLocal());
				Assert(!cost.E6_Calc_IncludeOnAgentInvoice);
				var countryCN = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.China));
				FreightDataRegistry.Instance.CommunityRegionsForDirectionCalculation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new Guid[] { countryCN.PK.ToGuid() });
				Assert(consol.IsLoadPortLocal());
				Assert(cost.E6_Calc_IncludeOnAgentInvoice);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestIncludeOnAgentInvoice()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();
				consol.JK_RL_NKLoadPort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
				consol.JK_RL_NKDischargePort = "USLAX";
				consol.SetDefaultSendingForwarderAddress(ObjectCreator.ABIGAS);
				consol.SetDefaultReceivingForwarderAddress(ObjectCreator.AALSHI);
				AccountingConfigurationRegistry.Instance.AutoPostMasterCollectCharge.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				Assert(consol.IsLoadPortLocal());
				cost.E6_OH_Creditor = ObjectCreator.ZECTRA.PK;
				Assert(!cost.E6_IsForCollectInvoice);
				Assert(!cost.E6_Calc_IncludeOnAgentInvoice);
				cost.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
				Assert(cost.E6_IsForCollectInvoice);
				Assert(cost.E6_Calc_IncludeOnAgentInvoice);
				cost.E6_OH_Creditor = ObjectCreator.ABIGAS.PK;
				Assert(!cost.E6_IsForCollectInvoice);
				Assert(!cost.E6_Calc_IncludeOnAgentInvoice);
				consol.JK_RL_NKLoadPort = "USLAX";
				consol.JK_RL_NKDischargePort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
				Assert(!consol.IsLoadPortLocal());
				consol.SetDefaultSendingForwarderAddress(ObjectCreator.ABIGAS);
				consol.SetDefaultReceivingForwarderAddress(ObjectCreator.AALSHI);
				cost.E6_OH_Creditor = ObjectCreator.ZECTRA.PK;
				Assert(!cost.E6_IsForCollectInvoice);
				Assert(!cost.E6_Calc_IncludeOnAgentInvoice);
				cost.E6_OH_Creditor = ObjectCreator.ABIGAS.PK;
				Assert(cost.E6_IsForCollectInvoice);
				Assert(cost.E6_Calc_IncludeOnAgentInvoice);
				cost.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
				Assert(!cost.E6_IsForCollectInvoice);
				Assert(!cost.E6_Calc_IncludeOnAgentInvoice);
				cost.E6_IsForCollectInvoice = false;
				Assert(!cost.E6_Calc_IncludeOnAgentInvoice);
				cost.E6_IsForCollectInvoice = true;
				cost.OverrideIsCreditorOverseasAgentCheck(true);
				Assert(cost.E6_Calc_IncludeOnAgentInvoice);
				cost.E6_IsForCollectInvoice = true;
				cost.OverrideIsCreditorOverseasAgentCheck(false);
				Assert(!cost.E6_Calc_IncludeOnAgentInvoice);
				AccountingConfigurationRegistry.Instance.AutoPostMasterCollectCharge.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				cost.E6_OH_Creditor = ObjectCreator.ABIGAS.PK;
				Assert(!cost.E6_IsForCollectInvoice);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestRemoveNonApplicableCharges()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupSinglePeriod(200401, new ZDateTime(2004, 1, 1), new ZDateTime(2004, 1, 31));
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			try
			{
				var query = new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
				query.AddToFilter(AccChargeCodeSchema.AC_AG_AccrualAccount, SQLComparisonOperator.NotEqual, null);
				JobConsolCost cost = apps.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = Factory.LoadTop1<AccChargeCode>(query).PK;
				cost.E6_OSCostAmount = 300m;
				cost.ApportionmentCharges[0].JR_OSCostAmt = 100m;
				cost.ApportionmentCharges[1].JR_OSCostAmt = 200m;
				cost.ApportionmentCharges[2].JR_OSCostAmt = 0m;
				cost.RemoveNonApplicableCharges();
				AssertEquals(2, cost.ApportionmentCharges.Count);
				Factory.Save();
				var arLine = (ARInvoiceLine)Factory.NewWithValidTestData<ARInvoice>().Lines.AddNew();
				arLine.AL_OSExTaxAmount = 10;
				arLine.AL_LineType = TransactionLineTypes.Revenue;
				arLine.AL_AG = ObjectCreator.GLHeader1.PK;
				cost.ApportionmentCharges[1].ReverseWIP(ZDateTime.Today);
				cost.ApportionmentCharges[1].JR_AL_ARLine = arLine.PK;
				cost.ApportionmentCharges[1].JR_OSSellAmt = arLine.AL_OSExTaxAmount;
				cost.ApportionmentCharges[1].JR_LocalSellAmt = arLine.AL_LocalExTaxAmount;
				AccTransactionLines acrLine = cost.ApportionmentCharges[1].APLine;
				AssertNotNull("ACR line", acrLine);
				Factory.Save();
				cost.E6_OSCostAmount = 150m;
				cost.ApportionmentCharges[0].JR_OSCostAmt = 150m;
				cost.ApportionmentCharges[1].JR_OSCostAmt = 0m;
				Assert("ApportionmentCharge.IsInDatabase", cost.ApportionmentCharges[1].IsInDatabase);
				Assert("ApportionmentCharge.IsRevenuePosted", cost.ApportionmentCharges[1].IsRevenuePosted);
				AssertNotNull("APLine", cost.ApportionmentCharges[1].APLine);
				AssertEquals("Is acrLine", acrLine, cost.ApportionmentCharges[1].APLine);
				AssertEquals("ACR", ZArchitecture.Core.TransactionLineTypes.Accrual, acrLine.AL_LineType);
				Assert("ACR is not reversed", acrLine.AL_ReverseDate.IsEmpty);
				ZDateTime revRecognitionDate = new ZDateTime(2004, 01, 14);
				acrLine.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate;
				JobChargeRevRecognition revRecognition = Factory.New<JobChargeRevRecognition>();
				revRecognition.D3_JH = cost.ApportionmentCharges[1].Job.PK;
				revRecognition.D3_RecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate;
				revRecognition.D3_RecognitionDate = revRecognitionDate;
				int apportionmentListChangedHitCount = 0;
				var listChangedHandler = new ListChangedEventHandler((sender, e) =>
				{
					apportionmentListChangedHitCount++;
				}

				);
				((IBindingList)cost.ApportionmentCharges).ListChanged += listChangedHandler;
				cost.RemoveNonApplicableCharges();
				AssertEquals("ListChanged on ConsolCost.ApportionmentCharges should be called once", 1, apportionmentListChangedHitCount);
				AssertEquals(1, cost.ApportionmentCharges.Count);
				AssertEquals("ACR should be reversed using RevenueRecognitionDate", revRecognitionDate, acrLine.AL_ReverseDate);
				Factory.Save();
				AssertEquals("ListChanged on ConsolCost.ApportionmentCharges should be called twice by Calculation Strategy AddDefaultCharges and Logging Strategy UpdateEditAndCreateLogFields", 3, apportionmentListChangedHitCount);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestRemoveNonApplicableChargesDoNotDeleteChargeWithUnpostedRevenue()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupSinglePeriod(200401, new ZDateTime(2004, 1, 1), new ZDateTime(2004, 1, 31));
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = ObjectCreator.FRT.PK;
				cost.E6_OSCostAmount = 320m;
				cost.ApportionmentCharges[0].JR_OSCostAmt = 100m;
				cost.ApportionmentCharges[1].JR_OSCostAmt = 200m;
				cost.ApportionmentCharges[2].JR_OSCostAmt = 20m;
				Factory.Save();
				AssertEquals(3, cost.ApportionmentCharges.Count);
				cost.E6_OSCostAmount = 1m;
				cost.ApportionmentCharges[0].JR_OSCostAmt = 1m;
				var chargeWithoutRev = cost.ApportionmentCharges[1];
				Assert("Apportioned Charge should be in Database", chargeWithoutRev.IsInDatabase);
				chargeWithoutRev.JR_OSCostAmt = 0m;
				chargeWithoutRev.JR_OSSellAmt = 0m;
				var chargeWithRev = cost.ApportionmentCharges[2];
				Assert("Apportioned Charge should be in Database", chargeWithRev.IsInDatabase);
				chargeWithRev.JR_OSCostAmt = 0m;
				chargeWithRev.JR_OSSellAmt = 20m;
				cost.RemoveNonApplicableCharges();
				AssertEquals(1, cost.ApportionmentCharges.Count);
				AssertEquals("Charge without Revenue should be Deleted", true, chargeWithoutRev.IsDeleted);
				AssertEquals("Charge with Revenue should not be Deleted", false, chargeWithRev.IsDeleted);
				AssertCollectionNotContains("Charge with Revenue should not be in apportionment", chargeWithRev, cost.ApportionmentCharges);
				Factory.Save();
				AssertNotNull("Charge With Revenue should be in Database after saving", Factory.Load<JobCharge>(chargeWithRev.PK));
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestIsUsedForApportionment_IsRecalculatedWhenConsolCostAmountIsChangedFromZero()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();
				AssertEquals("No Cost Amount should be on Consol Cost", 0M, cost.E6_OSCostAmount);
				AssertEquals("Should be 2 charges", 2, cost.ApportionmentCharges.Count);
				//When a new consol cost is added then the default behaviour is to set JR_IsUsedForApportionment flag to true
				AssertEquals("JR_IsUsedForApportionment", true, cost.ApportionmentCharges[0].JR_IsUsedForApportionment);
				AssertEquals("JR_IsUsedForApportionment", true, cost.ApportionmentCharges[1].JR_IsUsedForApportionment);
				cost.E6_OSCostAmount = 120M;
				//JR_IsUsedForApportionment flag remains checked
				AssertEquals("JR_IsUsedForApportionment", true, cost.ApportionmentCharges[0].JR_IsUsedForApportionment);
				AssertEquals("JR_IsUsedForApportionment", true, cost.ApportionmentCharges[1].JR_IsUsedForApportionment);
				//manually untick is used checkbox
				cost.ApportionmentCharges[0].JR_IsUsedForApportionment = false;
				cost.ApportionmentCharges[1].JR_IsUsedForApportionment = false;
				cost.E6_OSCostAmount = 0M;
				//still the check boxes are unticked
				AssertEquals("JR_IsUsedForApportionment", false, cost.ApportionmentCharges[0].JR_IsUsedForApportionment);
				AssertEquals("JR_IsUsedForApportionment", false, cost.ApportionmentCharges[1].JR_IsUsedForApportionment);
				cost.E6_OSCostAmount = 150M;
				//Both the check boxes are ticked as the consol cost amount is apportioned when consol cost amount is set to any value from zero
				AssertEquals("JR_IsUsedForApportionment", true, cost.ApportionmentCharges[0].JR_IsUsedForApportionment);
				AssertEquals("JR_IsUsedForApportionment", true, cost.ApportionmentCharges[1].JR_IsUsedForApportionment);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestSetIsUsedForApportionment()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			ForwardingShipment shipment3 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "Master";
			shipment3.JS_JS_ColoadMasterShipment = shipment2.PK;
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();
				AssertEquals("No Cost Amount should be on Consol Cost", 0M, cost.E6_OSCostAmount);
				AssertEquals("E6_ApportionToRelatedShipments", false, cost.E6_ApportionToRelatedShipments);
				AssertEquals("Should be 2 charges", 2, cost.ApportionmentCharges.Count);
				cost.SetIsUsedForApportionment();
				AssertEquals("JR_IsUsedForApportionment", true, cost.ApportionmentCharges[0].JR_IsUsedForApportionment);
				AssertEquals("JR_IsUsedForApportionment", true, cost.ApportionmentCharges[1].JR_IsUsedForApportionment);
				cost.E6_ApportionToRelatedShipments = true;
				AssertEquals("Should be 3 charges", 3, cost.ApportionmentCharges.Count);
				cost.SetIsUsedForApportionment();
				AssertEquals("JR_IsUsedForApportionment", true, cost.ApportionmentCharges[0].JR_IsUsedForApportionment);
				AssertEquals("JR_IsUsedForApportionment", true, cost.ApportionmentCharges[1].JR_IsUsedForApportionment);
				AssertEquals("JR_IsUsedForApportionment", false, cost.ApportionmentCharges[2].JR_IsUsedForApportionment);
				cost.E6_OSCostAmount = 200m;
				cost.ApportionmentCharges[0].JR_OSCostAmt = 100m;
				cost.ApportionmentCharges[1].JR_OSCostAmt = 0m;
				cost.ApportionmentCharges[2].JR_OSCostAmt = 1000m;
				cost.SetIsUsedForApportionment();
				AssertEquals("JR_IsUsedForApportionment", true, cost.ApportionmentCharges[0].JR_IsUsedForApportionment);
				AssertEquals("JR_IsUsedForApportionment", false, cost.ApportionmentCharges[1].JR_IsUsedForApportionment);
				AssertEquals("JR_IsUsedForApportionment", true, cost.ApportionmentCharges[2].JR_IsUsedForApportionment);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			JobConsolCost cost = Factory.New<JobConsolCost>();
			return cost;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var consolCost = (JobConsolCost)base.GetNewBusinessObject();
			consolCost.E6_IsTaxAmountOverridden = true;

			return consolCost;
		}

		public void TestAutoratingNotes()
		{
			AutoRateInfo info = new AutoRateInfo(Factory);
			info.CalculationDescription = GetDesc("BBB1");
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			JobConsolCost cost = GetCost(consol);
			var consolNumber = consol.JK_UniqueConsignRef;
			try
			{
				cost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
				Assert(cost.CostCalculationDescription.IsEmpty);
				cost.SetCalculationDescription(info);
				cost.E6_LocalCostAmount = 0m; //If amount is not changed description stays

				AssertCostCalculationDescription(cost, consolNumber, "BBB1", true);

				cost.E6_LocalCostAmount = 10m;

				AssertCostCalculationDescription(cost, consolNumber, "Cost was autocosted but cost amount was subsequently changed", false);

				cost.CostCalculationDescription = ZBlob.Empty;
				cost.E6_LocalCostAmount = 20m;
				Assert(cost.CostCalculationDescription.IsEmpty);
			}
			finally
			{
				cost.CalculationStrategy.ReleaseMutexes();
			}
		}

		public void TestCostCalculationDescriptionString()
		{
			var cost = Factory.New<JobConsolCost>();
			cost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			ZString description = "abcdefg!@#$";
			cost.CostCalculationDescription = ZBlob.FromUTF8(description);
			AssertEquals(description, cost.CostCalculationDescriptionString);
		}

		public void TestCostCalculationDescriptionString_WithRTFDescription()
		{
			var cost = Factory.New<JobConsolCost>();
			cost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			ZString description = "abcdefg!@#$";
			cost.CostCalculationDescription = ZBlob.FromUTF8(new FormattedRtfString().Add(description, System.Drawing.FontStyle.Bold).ToString());
			AssertEquals(description, cost.CostCalculationDescriptionString);
		}

		public void TestAutoratingNotes_IsNotClearedByUpdatingExchangeRate()
		{
			var calculationDescriptionText = "Blah blah been auto rated";
			var rateInfo = new AutoRateInfo(Factory);
			rateInfo.CalculationDescription = GetDesc(calculationDescriptionText);

			var consol = Factory.New<ForwardingConsol>();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			var consolCost = GetCost(consol);
			var consolNumber = consol.JK_UniqueConsignRef;

			try
			{
				consolCost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
				consolCost.E6_OSCostAmount = 100m;
				consolCost.E6_RX_NKCurrency = Constants.CurrencyCodes.Uganda;
				consolCost.SetCalculationDescription(rateInfo);

				AssertCostCalculationDescription(consolCost, consolNumber, calculationDescriptionText, true);

				consolCost.CostExchangeRate.Currency = Constants.CurrencyCodes.Uganda;
				consolCost.CostExchangeRate.Rate = 0.5;

				AssertCostCalculationDescription(consolCost, consolNumber, calculationDescriptionText, true);

				consolCost.E6_OSCostAmount = 444m;

				AssertCostCalculationDescription(consolCost, consolNumber, "Cost was autocosted but cost amount was subsequently changed", false);
			}
			finally
			{
				consolCost.CalculationStrategy.ReleaseMutexes();
			}
		}

		static void AssertCostCalculationDescription(JobConsolCost consolCost, string consolNumber, string expectedDescription, bool wasApportioned)
		{
			var actualDescription = consolCost.CostCalculationDescription.ToAscii();

			AssertContains(expectedDescription, actualDescription);

			foreach (ApportionSplitCharge charge in consolCost.ApportionmentCharges)
			{
				actualDescription = charge.CostCalculationDescription.ToAscii();
				AssertContains(expectedDescription, actualDescription);

				var apportionedText = $"This cost was autocosted and apportioned from Consol {consolNumber}. Details listed are Consol details.";
				AssertEquals(wasApportioned, actualDescription.Contains(apportionedText));
			}
		}

		public virtual void TestChequeNumberSetToPaddedNumberOnCharges()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			consol.JK_UniqueConsignRef = "X00001000";
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			JobConsolCost cost = GetCost(consol);
			cost.E6_OSCostAmount = 100m;
			cost.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			cost.E6_InvoiceNum = "ABC123";
			cost.E6_InvoiceDate = ZDateTime.Now;
			cost.E6_PaymentType = ReceiptTypes.Cheque;
			ObjectCreator.AUDBankAccount.AB_ChequeNumDigits = (ZByte)6;
			cost.E6_AB_BankAccount = ObjectCreator.AUDBankAccount.PK;
			cost.E6_AK_ChequeBook = ObjectCreator.AUDChequeBook.PK;
			cost.E6_ChequeOrReference = "101";
			AssertEquals("000101", cost.E6_ChequeOrReference);
			AssertEquals("000101", cost.ApportionmentCharges[0].JR_ChequeNo);
			AssertEquals("000101", cost.ApportionmentCharges[1].JR_ChequeNo);
		}

		ZString GetDesc(ZString text)
		{
			FormattedRtfString desc = new FormattedRtfString();
			desc += text;
			return desc.ToRtf();
		}

		public void TestExchangeRateForApportionmentUseConsolExchangeRate()
		{
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate(JobInvoicingConsumerTypes.ForwardingConsol.Code, Constants.FreightShipmentDirection.Code.All, Constants.TransportModes.All, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.ConsolExchangeRate);
			GlbCompany.CurrentCompany.Factory.Save();

			var usdExchangeRate = 0.916m;
			var gbpExchangeRate = 2.016m;
			var voyageRate = 2.816m;
			ObjectCreator.CreateExchangeRate(ObjectCreator.USD, Constants.ExchangeRateTypes.Code.BuyRate, usdExchangeRate, ZDateTime.Today, ZDateTime.Today);
			ObjectCreator.CreateExchangeRate(ObjectCreator.GBP, Constants.ExchangeRateTypes.Code.BuyRate, gbpExchangeRate, ZDateTime.Today, ZDateTime.Today);
			Factory.Save();

			var consol = ObjectCreator.CreateConsol("USLAX", "AUSYD");
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.Transports[0].JW_Vessel = "QF";
			consol.Transports[0].JW_VoyageFlight = "1234";
			consol.Transports[0].JW_RL_NKLoadPort = "USLAX";
			consol.Transports[0].JW_RL_NKDiscPort = "AUSYD";
			consol.Transports[0].JW_ETA = ZDateTime.Now;
			consol.Transports[0].JW_ETD = ZDateTime.Now.AddDays(1);
			consol.Transports[0].JW_IsLinked = ZBool.True;
			Factory.Save();

			AssertNotNull(consol.Transports[0].Sailing);
			AssertNotNull(consol.Transports[0].Sailing.Voyage);

			var rate = consol.Transports[0].Sailing.Voyage.ExRates.AddNew();
			rate.E8_GC = GlbCompany.CurrentCompany.PK;
			rate.E8_RX_NKExCurrency = Constants.CurrencyCodes.UnitedKingdom;
			rate.E8_VoyageExchangeRate = voyageRate;

			var apps = new ApportionmentListing(Factory, consol);
			var cost1 = apps.CostsCollection.TryAddNew();
			AssertEquals("Default currency", cost1.E6_RX_NKCurrency, Core.Constants.CurrencyCodes.Australia);
			AssertEquals("Default exchange rate", cost1.E6_ExchangeRate, 1m);
			cost1.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost1.E6_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			AssertEquals("Exchange rate should be current currency exchange rate", cost1.E6_ExchangeRate, usdExchangeRate);
			cost1.E6_ExchangeRate = 1.016m;
			var cost2 = apps.CostsCollection.TryAddNew();
			cost2.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			cost2.E6_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			AssertEquals("Exchange rate should be taken from freight charge", cost2.E6_ExchangeRate, 1.016m);
			cost1.E6_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedKingdom;
			AssertEquals("Exchange rate should be taken from voyage rate", cost1.E6_ExchangeRate, voyageRate);
		}

		public void TestE6_ExchangeRate()
		{
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate(JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.All, Core.Constants.TransportModes.All, preference: Constants.JobBillingExchangeRatePreference.Code.ConsolExchangeRate);
			GlbCompany.CurrentCompany.Factory.Save();

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			try
			{
				apps.OnReplaceShipmentExchangeRate += new EventHandler<ReplaceShipmentExchangeRateEventArgs>(OnReplaceShipmentExchangeRateHandler);
				JobConsolCost cost1 = apps.CostsCollection.TryAddNew();
				cost1.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
				ZString currencyCode = Core.Constants.CurrencyCodes.Afghanistan;
				cost1.E6_RX_NKCurrency = currencyCode;
				ApportionSplitCharge charge1 = FindChargeForJob(cost1, shipment1);
				ApportionSplitCharge charge2 = FindChargeForJob(cost1, shipment2);
				charge1.JR_OSCostAmt = 10M;
				charge2.JR_OSCostAmt = 10M;
				ExchangeRatesCollection rates1 = ((Job)shipment1.ShipmentJobHeader).ExchangeRates;
				ExchangeRatesCollection rates2 = ((Job)shipment2.ShipmentJobHeader).ExchangeRates;
				AssertEquals("Shipment1 should not contain a currency", 0, rates1.Count);
				AssertEquals("Shipment2 should not contain a currency", 0, rates2.Count);
				//One not saved FRT ConsolCost
				ZBool prevDoesReplaceRate = DoesReplaceRate; //false
				cost1.E6_ExchangeRate = 2M;
				AssertNotEquals("Event should be raised", prevDoesReplaceRate, DoesReplaceRate);
				AssertEquals("Shipment1 should not contain a currency", 0, rates1.Count);
				AssertEquals("Shipment2 should not contain a currency", 0, rates2.Count);
				prevDoesReplaceRate = DoesReplaceRate; //true
				cost1.E6_ExchangeRate = 3M;
				AssertNotEquals("Event should be raised", prevDoesReplaceRate, DoesReplaceRate);
				AssertEquals("Shipment1 should contain a currency", 1, rates1.Count);
				AssertEquals("Shipment2 should contain a currency", 1, rates2.Count);
				JobInvoicing.ExchangeRate rate1 = rates1[0];
				JobInvoicing.ExchangeRate rate2 = rates2[0];
				AssertEquals("Shipment1 should contain the currency", currencyCode, rate1.JF_RX_NKRateCurrency);
				AssertEquals("Shipment2 should contain the currency", currencyCode, rate2.JF_RX_NKRateCurrency);
				AssertEquals("Shipment1 should change Exchange Rate", 3m, rate1.JF_BaseRate);
				AssertEquals("Shipment2 should change Exchange Rate", 3m, rate2.JF_BaseRate);
				//One saved FRT ConsolCost
				prevDoesReplaceRate = DoesReplaceRate; //false
				cost1.E6_ExchangeRate = 4M;
				AssertNotEquals("Event should be raised", prevDoesReplaceRate, DoesReplaceRate);
				AssertEquals("Shipment1 should contain a currency", 1, rates1.Count);
				AssertEquals("Shipment2 should contain a currency", 1, rates2.Count);
				AssertEquals("Shipment1 should contain the currency", currencyCode, rate1.JF_RX_NKRateCurrency);
				AssertEquals("Shipment2 should contain the currency", currencyCode, rate2.JF_RX_NKRateCurrency);
				AssertEquals("Shipment1 should not change Exchange Rate", 3m, rate1.JF_BaseRate);
				AssertEquals("Shipment2 should not change Exchange Rate", 3m, rate2.JF_BaseRate);
				prevDoesReplaceRate = DoesReplaceRate; //true
				cost1.E6_ExchangeRate = 5M;
				AssertNotEquals("Event should be raised", prevDoesReplaceRate, DoesReplaceRate);
				AssertEquals("Shipment1 should contain a currency", 1, rates1.Count);
				AssertEquals("Shipment2 should contain a currency", 1, rates2.Count);
				AssertEquals("Shipment1 should contain the currency", currencyCode, rate1.JF_RX_NKRateCurrency);
				AssertEquals("Shipment2 should contain the currency", currencyCode, rate2.JF_RX_NKRateCurrency);
				AssertEquals("Shipment1 should not change Exchange Rate", 5m, rate1.JF_BaseRate);
				AssertEquals("Shipment2 should not change Exchange Rate", 5m, rate2.JF_BaseRate);
				//One saved and one not saved FRT ConsolCostAddNewIfAllowed
				JobConsolCost cost2 = apps.CostsCollection.TryAddNew();
				cost2.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
				cost2.E6_RX_NKCurrency = currencyCode;
				prevDoesReplaceRate = DoesReplaceRate = true; //true
				cost2.E6_ExchangeRate = 10M;
				AssertEquals("Event should not be raised", prevDoesReplaceRate, DoesReplaceRate);
				AssertEquals("Shipment1 should contain a currency", 1, rates1.Count);
				AssertEquals("Shipment2 should contain a currency", 1, rates2.Count);
				AssertEquals("Shipment1 should not change Exchange Rate", 5m, rate1.JF_BaseRate);
				AssertEquals("Shipment2 should not change Exchange Rate", 5m, rate2.JF_BaseRate);
				prevDoesReplaceRate = DoesReplaceRate; //true
				cost2.E6_ExchangeRate = 11M;
				AssertEquals("Event should not be raised", prevDoesReplaceRate, DoesReplaceRate);
				AssertEquals("Shipment1 should contain a currency", 1, rates1.Count);
				AssertEquals("Shipment2 should contain a currency", 1, rates2.Count);
				AssertEquals("Shipment1 should not change Exchange Rate", 5m, rate1.JF_BaseRate);
				AssertEquals("Shipment2 should not change Exchange Rate", 5m, rate2.JF_BaseRate);
				//Two not saved FRT ConsolCosts
				apps.CostsCollection.RemoveAndDeleteAll();
				cost1 = apps.CostsCollection.TryAddNew();
				cost1.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
				cost1.E6_RX_NKCurrency = currencyCode;
				cost2 = apps.CostsCollection.TryAddNew();
				cost2.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
				cost2.E6_RX_NKCurrency = currencyCode;
				prevDoesReplaceRate = DoesReplaceRate; //true
				cost1.E6_ExchangeRate = 20M;
				AssertEquals("Event should not be raised", prevDoesReplaceRate, DoesReplaceRate);
				AssertEquals("Shipment1 should contain a currency", 1, rates1.Count);
				AssertEquals("Shipment2 should contain a currency", 1, rates2.Count);
				AssertEquals("Shipment1 should not change Exchange Rate", 5m, rate1.JF_BaseRate);
				AssertEquals("Shipment2 should not change Exchange Rate", 5m, rate2.JF_BaseRate);
				prevDoesReplaceRate = DoesReplaceRate; //true
				cost1.E6_ExchangeRate = 21M;
				AssertEquals("Event should not be raised", prevDoesReplaceRate, DoesReplaceRate);
				AssertEquals("Shipment1 should contain a currency", 1, rates1.Count);
				AssertEquals("Shipment2 should contain a currency", 1, rates2.Count);
				AssertEquals("Shipment1 should not change Exchange Rate", 5m, rate1.JF_BaseRate);
				AssertEquals("Shipment2 should not change Exchange Rate", 5m, rate2.JF_BaseRate);
				prevDoesReplaceRate = DoesReplaceRate; //true
				cost2.E6_ExchangeRate = 30M;
				AssertEquals("Event should not be raised", prevDoesReplaceRate, DoesReplaceRate);
				AssertEquals("Shipment1 should contain a currency", 1, rates1.Count);
				AssertEquals("Shipment2 should contain a currency", 1, rates2.Count);
				AssertEquals("Shipment1 should not change Exchange Rate", 5m, rate1.JF_BaseRate);
				AssertEquals("Shipment2 should not change Exchange Rate", 5m, rate2.JF_BaseRate);
				prevDoesReplaceRate = DoesReplaceRate; //true
				cost2.E6_ExchangeRate = 31M;
				AssertEquals("Event should not be raised", prevDoesReplaceRate, DoesReplaceRate);
				AssertEquals("Shipment1 should contain a currency", 1, rates1.Count);
				AssertEquals("Shipment2 should contain a currency", 1, rates2.Count);
				AssertEquals("Shipment1 should not change Exchange Rate", 5m, rate1.JF_BaseRate);
				AssertEquals("Shipment2 should not change Exchange Rate", 5m, rate2.JF_BaseRate);
				//One not saved FRT ConsolCosts but without user rights
				apps.CostsCollection.RemoveAndDeleteAll();
				cost1 = apps.CostsCollection.TryAddNew();
				cost1.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
				cost1.E6_RX_NKCurrency = currencyCode;
				prevDoesReplaceRate = DoesReplaceRate; //true
				cost1.E6_ExchangeRate = 43M;
				AssertNotEquals("Event should be raised", prevDoesReplaceRate, DoesReplaceRate);
				AssertEquals("Shipment1 should contain a currency", 1, rates1.Count);
				AssertEquals("Shipment2 should contain a currency", 1, rates2.Count);
				AssertEquals("Shipment1 should change Exchange Rate", 43m, rate1.JF_BaseRate);
				AssertEquals("Shipment2 should change Exchange Rate", 43m, rate2.JF_BaseRate);

				GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate(JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.All, Core.Constants.TransportModes.All, preference: Constants.JobBillingExchangeRatePreference.Code.TodaysRate);
				GlbCompany.CurrentCompany.Factory.Save();
				prevDoesReplaceRate = DoesReplaceRate = true; //true
				cost1.E6_ExchangeRate = 50M;
				AssertEquals("Event should not be raised", prevDoesReplaceRate, DoesReplaceRate);
				AssertEquals("Shipment1 should contain a currency", 1, rates1.Count);
				AssertEquals("Shipment2 should contain a currency", 1, rates2.Count);
				AssertEquals("Shipment1 should not change Exchange Rate", 43m, rate1.JF_BaseRate);
				AssertEquals("Shipment2 should not change Exchange Rate", 43m, rate2.JF_BaseRate);
				prevDoesReplaceRate = DoesReplaceRate; //true
				cost1.E6_ExchangeRate = 51M;
				AssertEquals("Event should not be raised", prevDoesReplaceRate, DoesReplaceRate);
				AssertEquals("Shipment1 should contain a currency", 1, rates1.Count);
				AssertEquals("Shipment2 should contain a currency", 1, rates2.Count);
				AssertEquals("Shipment1 should not change Exchange Rate", 43m, rate1.JF_BaseRate);
				AssertEquals("Shipment2 should not change Exchange Rate", 43m, rate2.JF_BaseRate);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestExchangeRateRoundingErrorSpreadAmongChargesNotEcxceedingMaxCorrectionOf5()
		{
			// Make CurrentCompany Japan based
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Japan;
			// A hack to force no numbers after decimal point
			GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = 0;
			GlbCompany.CurrentCompany.Factory.Save();

			Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			for (int i = 0; i < 26; i++)
			{
				consol.Shipments.AddNew();
			}

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
				cost.E6_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				cost.E6_OSCostAmount = 750m;
				cost.E6_ExchangeRate = 102.94m;
				cost.E6_ApportionmentMethod = ZArchitecture.Core.AllocationMethod.Manual;
				AssertEquals("ApportionmentCharges.Count", 26, cost.ApportionmentCharges.Count);
				var osAmounts = new ZDecimal[26] { 27.50m, 103.43m, 92.65m, 16.78m, 10.70m, 11.91m, 58.78m, 25.89m, 41.68m, 10.70m, 10.70m, 17.17m, 16.83m, 35.79m, 10.70m, 27.00m, 10.70m, 10.70m, 35.81m, 14.67m, 10.70m, 32.19m, 64.34m, 31.28m, 10.70m, 10.70m };
				for (int i = 0; i < 26; i++)
				{
					cost.ApportionmentCharges[i].JR_OSCostAmt = osAmounts[i];
				}

				AssertEquals("Sum of JR_OSCostAmt", 750m, cost.ApportionmentCharges.ToArray<ApportionSplitCharge>().Sum(x => x.JR_OSCostAmt));
				AssertEquals("Sum of JR_LocalCostAmt", 77205m, cost.ApportionmentCharges.ToArray<ApportionSplitCharge>().Sum(x => x.JR_LocalCostAmt));
				var expectedLocalAmounts = new ZDecimal[26] { 2831m, 10652m, 9538m, 1727m, 1101m, 1226m, 6051m, 2665m, 4291m, 1101m, 1101m, 1767m, 1732m, 3684m, 1101m, 2779m, 1101m, 1101m, 3686m, 1510m, 1101m, 3314m, 6623m, 3220m, 1101m, 1101m };
				for (int i = 0; i < 26; i++)
				{
					AssertEquals(string.Format("Apportioned JR_LocalCostAmt {0}", i), expectedLocalAmounts[i], cost.ApportionmentCharges[i].JR_LocalCostAmt);
				}
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestExchangeRateRoundingErrorWithNegativeAmount()
		{
			// Make CurreentCompany Japan based
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Japan;
			// A hack to force no numbers after decimal point
			GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = 0;
			GlbCompany.CurrentCompany.Factory.Save();

			Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			for (int i = 0; i < 5; i++)
			{
				consol.Shipments.AddNew();
			}

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
				cost.E6_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				cost.E6_OSCostAmount = 500m;
				cost.E6_ExchangeRate = 102.94m;
				cost.E6_ApportionmentMethod = ZArchitecture.Core.AllocationMethod.Manual;
				AssertEquals("ApportionmentCharges.Count", 5, cost.ApportionmentCharges.Count);
				var osAmounts = new ZDecimal[] { 189.50m, 203.43m, -240.32m, 200.78m, 146.61m };
				for (int i = 0; i < 5; i++)
				{
					cost.ApportionmentCharges[i].JR_OSCostAmt = osAmounts[i];
				}

				AssertEquals("Sum of JR_OSCostAmt", 500m, cost.ApportionmentCharges.ToArray<ApportionSplitCharge>().Sum(x => x.JR_OSCostAmt));
				AssertEquals("Sum of JR_LocalCostAmt", 51470m, cost.ApportionmentCharges.ToArray<ApportionSplitCharge>().Sum(x => x.JR_LocalCostAmt));
				var expectedLocalAmounts = new ZDecimal[] { 19507m, 20941m, -24738m, 20668m, 15092m };
				for (int i = 0; i < 5; i++)
				{
					AssertEquals(string.Format("Apportioned JR_LocalCostAmt {0}", i), expectedLocalAmounts[i], cost.ApportionmentCharges[i].JR_LocalCostAmt);
				}
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestExchangeRateRoundingErrorSpreadAmongChargresWithoutZeroingOrChangingAmountSign()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			for (int i = 0; i < 34; i++)
			{
				consol.Shipments.AddNew();
			}

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
				cost.E6_RX_NKCurrency = Core.Constants.CurrencyCodes.Indonesia;
				cost.E6_OSCostAmount = 33000m;
				cost.E6_ExchangeRate = 11366m;
				cost.E6_LocalCostAmount = 2.90m;
				cost.E6_ApportionmentMethod = ZArchitecture.Core.AllocationMethod.Shipment;
				cost.UpdateApportionmentChargesListing();
				AssertEquals("ApportionmentCharges.Count", 34, cost.ApportionmentCharges.Count);
				var apportionedCharges = cost.ApportionmentCharges.ToArray<ApportionSplitCharge>();
				Assert("All apportioned charges must be In Use", apportionedCharges.All(x => x.JR_IsUsedForApportionment));
				Assert("All OS Amounts must be greater than 0", apportionedCharges.All(x => x.JR_OSCostAmt > 0m));
				Assert("All Local Amounts must be greater than 0", apportionedCharges.All(x => x.JR_LocalCostAmt > 0m));
				// Make sure it works with negative cost as well
				cost.E6_OSCostAmount = -33000m;
				cost.E6_LocalCostAmount = -2.90m;
				cost.E6_ApportionmentMethod = ZArchitecture.Core.AllocationMethod.Shipment;
				cost.UpdateApportionmentChargesListing();
				AssertEquals("ApportionmentCharges.Count", 34, cost.ApportionmentCharges.Count);
				apportionedCharges = cost.ApportionmentCharges.ToArray<ApportionSplitCharge>();
				Assert("All apportioned charges must be In Use", apportionedCharges.All(x => x.JR_IsUsedForApportionment));
				Assert("All OS Amounts must be less than 0", apportionedCharges.All(x => x.JR_OSCostAmt < 0m));
				Assert("All Local Amounts must be less than 0", apportionedCharges.All(x => x.JR_LocalCostAmt < 0m));
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestLocalCurrencyMinAmount()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			AssertEquals("AUD", Env.CurrentCompany.LocalCurrency.Code);
			AssertEquals("AUD Decimal Points", 2, Env.CurrentCompany.LocalCurrency.Decimals);
			AssertEquals("LocalCurrencyMinAmount for AUD", 0.01m, cost.LocalCurrencyMinAmount);
			ObjectCreator.NonCurrentCompany.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Japan;
			Factory.Save();
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, ObjectCreator.NonCurrentCompany.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				AssertEquals("JPY Decimal Points", 0, Env.CurrentCompany.LocalCurrency.Decimals);
				AssertEquals("LocalCurrencyMinAmount for JPY", 1m, cost.LocalCurrencyMinAmount);
			}
		}

		public void TestRoundingErrorSpreadAmongChargresWithoutZeroingOrChangingOSAmountSign()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			for (int i = 0; i < 34; i++)
			{
				consol.Shipments.AddNew();
			}

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
				cost.E6_RX_NKCurrency = Core.Constants.CurrencyCodes.Indonesia;
				cost.E6_OSCostAmount = 54m;
				cost.E6_ExchangeRate = 1.3m;
				cost.E6_LocalCostAmount = 41.54m;
				cost.E6_ApportionmentMethod = ZArchitecture.Core.AllocationMethod.Shipment;
				cost.UpdateApportionmentChargesListing();
				AssertEquals("ApportionmentCharges.Count", 34, cost.ApportionmentCharges.Count);
				var apportionedCharges = cost.ApportionmentCharges.ToArray<ApportionSplitCharge>();
				Assert("All apportioned charges must be In Use", apportionedCharges.All(x => x.JR_IsUsedForApportionment));
				Assert("All OS Amounts must be greater than 0", apportionedCharges.All(x => x.JR_OSCostAmt > 0m));
				Assert("All Local Amounts must be greater than 0", apportionedCharges.All(x => x.JR_LocalCostAmt > 0m));
				// Make sure it works with negative cost as well
				cost.E6_OSCostAmount = -54m;
				cost.E6_LocalCostAmount = -41.54m;
				cost.E6_ApportionmentMethod = ZArchitecture.Core.AllocationMethod.Shipment;
				cost.UpdateApportionmentChargesListing();
				AssertEquals("ApportionmentCharges.Count", 34, cost.ApportionmentCharges.Count);
				apportionedCharges = cost.ApportionmentCharges.ToArray<ApportionSplitCharge>();
				Assert("All apportioned charges must be In Use", apportionedCharges.All(x => x.JR_IsUsedForApportionment));
				Assert("All OS Amounts must be less than 0", apportionedCharges.All(x => x.JR_OSCostAmt < 0m));
				Assert("All Local Amounts must be less than 0", apportionedCharges.All(x => x.JR_LocalCostAmt < 0m));
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestRoundingErrorSpreadAmongChargresWithoutExceedingMaxLocalAmountCorrection_5()
		{
			var companyQuery = new ZQuery(GlbCompanySchema.GC_IsReciprocal, true);
			companyQuery.AddToFilter(GlbCompanySchema.GC_IsActive, true);
			var company = Factory.LoadTop1<GlbCompany>(companyQuery);
			AssertNotNull("Company", company);
			AssertNotNull("Branch", company.FirstActiveBranch);
			company.SetCountry(Core.Constants.CountryCodes.Indonesia);
			company.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Indonesia;
			Factory.Save();
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, company.FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				ForwardingConsol consol = Factory.New<ForwardingConsol>();
				Factory.Save();
				for (int i = 0; i < 22; i++)
				{
					consol.Shipments.AddNew();
				}

				ApportionmentListing apps = new ApportionmentListing(Factory, consol);
				try
				{
					JobConsolCost cost = apps.CostsCollection.TryAddNew();
					cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
					cost.E6_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
					cost.E6_OSCostAmount = 405m;
					cost.E6_ExchangeRate = 103.65m;
					cost.E6_ApportionmentMethod = ZArchitecture.Core.AllocationMethod.Shipment;
					AssertEquals("E6_LocalCostAmount", 41978m, cost.E6_LocalCostAmount);
					AssertEquals("ApportionmentCharges Sum of Local Amounts same as Consol Cost Local Amount", cost.E6_LocalCostAmount, cost.ApportionmentCharges.Cast<ApportionSplitCharge>().Sum(x => x.JR_LocalCostAmt));
					cost.E6_ExchangeRate = 103.56m;
					AssertEquals("E6_LocalCostAmount", 41942m, cost.E6_LocalCostAmount);
					AssertEquals("ApportionmentCharges Sum of Local Amounts same as Consol Cost Local Amount as we split rounding error 9 between 2 Charges to do not exceed max correction 5", cost.E6_LocalCostAmount, cost.ApportionmentCharges.Cast<ApportionSplitCharge>().Sum(x => x.JR_LocalCostAmt));
				}
				finally
				{
					apps.ReleaseMutexes();
				}
			}
		}

		public void TestTaxAmountRoundingErrorSpreadAmongChargresWithZeroTaxAmountCalculatedOnEveryCharge()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			for (int i = 0; i < 34; i++)
			{
				consol.Shipments.AddNew();
			}

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
				cost.E6_RX_NKCurrency = Core.Constants.CurrencyCodes.Indonesia;
				cost.E6_IsTaxAmountOverridden = true;
				cost.E6_OSCostAmount = 55m;
				cost.E6_ExchangeRate = 1.3m;
				cost.E6_LocalCostAmount = 41.54m;
				cost.E6_OSGSTAmount_Calc = 11m;
				cost.E6_ApportionmentMethod = ZArchitecture.Core.AllocationMethod.Shipment;
				cost.UpdateApportionmentChargesListing();
				AssertEquals("ApportionmentCharges.Count", 34, cost.ApportionmentCharges.Count);
				var apportionedCharges = cost.ApportionmentCharges.ToArray<ApportionSplitCharge>();
				Assert("All apportioned charges must be In Use", apportionedCharges.All(x => x.JR_IsUsedForApportionment));
				Assert("All OS Amounts must be greater than 0", apportionedCharges.All(x => x.JR_OSCostAmt > 0m));
				Assert("Should be some Charges with Tax Amounts greater than 0", apportionedCharges.Any(x => x.JR_OSCostGSTAmt_Calc > 0m));
				AssertEquals("Sum of Charges Tax Amounts should match Consol Cost Tax Amounts", cost.E6_OSGSTAmount_Calc, apportionedCharges.Sum(x => x.JR_OSCostGSTAmt_Calc));
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		ZBool DoesReplaceRate;
		void OnReplaceShipmentExchangeRateHandler(object sender, ReplaceShipmentExchangeRateEventArgs e)
		{
			e.DoesReplaceShipmentExchangeRate = DoesReplaceRate;
			DoesReplaceRate = !DoesReplaceRate;
		}

		public void TestUpdateApportionmentChargesListingUntickIsUsedWhenLocalAmountIsTooSmall()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			var job1 = AddNewJobShipment(consol);
			var job2 = AddNewJobShipment(consol);
			var job3 = AddNewJobShipment(consol);
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost.E6_RX_NKCurrency = Core.Constants.CurrencyCodes.Indonesia;
			cost.E6_ExchangeRate = 10000m;
			cost.E6_ApportionmentMethod = ZArchitecture.Core.AllocationMethod.Shipment;
			cost.E6_OSCostAmount = 210m;
			cost.UpdateApportionmentChargesListing();
			AssertEquals("Local Amount", 0.02m, cost.E6_LocalCostAmount);
			AssertEquals(3, cost.ApportionmentCharges.Count);
			var apportionedCharges = cost.ApportionmentCharges.ToArray<ApportionSplitCharge>();
			var chargesInUse = apportionedCharges.Where(x => x.JR_IsUsedForApportionment);
			var chargesNotInUse = apportionedCharges.Where(x => !x.JR_IsUsedForApportionment);
			AssertEquals("Two apportioned charges should be In Use", 2, chargesInUse.Count());
			AssertEquals("One apportioned charges should not be In Use", 1, chargesNotInUse.Count());
			Assert("All OS Amounts In Use must be 105 IDR", chargesInUse.All(x => x.JR_OSCostAmt == 105m));
			Assert("All Local Amounts In Use must be 0.01", chargesInUse.All(x => x.JR_LocalCostAmt == 0.01m));
			AssertEquals("Charge Not In Use with 0 OS Amount", ZDecimal.Zero, chargesNotInUse.FirstOrDefault().JR_OSCostAmt);
			AssertEquals("Charge Not In Use with 0 Local Amount", ZDecimal.Zero, chargesNotInUse.FirstOrDefault().JR_LocalCostAmt);
			cost.Validation.ValidateE6_LocalCostAmount();
			AssertNoErrors("Should not be any error as we fixed issue by unticking one of the Charges", cost.E6_LocalCostAmountInfo);
			cost.E6_OSCostAmount = 60m;
			cost.UpdateApportionmentChargesListing();
			AssertEquals("Local Amount", 0.01m, cost.E6_LocalCostAmount);
			AssertEquals(3, cost.ApportionmentCharges.Count);
			apportionedCharges = cost.ApportionmentCharges.ToArray<ApportionSplitCharge>();
			chargesInUse = apportionedCharges.Where(x => x.JR_IsUsedForApportionment);
			chargesNotInUse = apportionedCharges.Where(x => !x.JR_IsUsedForApportionment);
			AssertEquals("Two apportioned charges should be In Use", 1, chargesInUse.Count());
			AssertEquals("One apportioned charges should not be In Use", 2, chargesNotInUse.Count());
			Assert("All OS Amounts In Use must be 60 IDR", chargesInUse.All(x => x.JR_OSCostAmt == 60m));
			Assert("All Local Amounts In Use must be 0.01", chargesInUse.All(x => x.JR_LocalCostAmt == 0.01m));
			AssertEquals("Charge Not In Use with 0 OS Amount", ZDecimal.Zero, chargesNotInUse.FirstOrDefault().JR_OSCostAmt);
			AssertEquals("Charge Not In Use with 0 Local Amount", ZDecimal.Zero, chargesNotInUse.FirstOrDefault().JR_LocalCostAmt);
			cost.Validation.ValidateE6_LocalCostAmount();
			AssertNoErrors("Should not be any error as we fixed issue by unticking one of the Charges", cost.E6_LocalCostAmountInfo);
		}

		public void TestUpdateApportionmentChargesListingUntickIsUsedWhenLocalAmountIsTooSmall_NoRoundingError()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			var job1 = AddNewJobShipment(consol);
			var job2 = AddNewJobShipment(consol);
			var job3 = AddNewJobShipment(consol);
			var job4 = AddNewJobShipment(consol);
			var job5 = AddNewJobShipment(consol);
			SetShipmentWeightAndVolume(job1, 2500m, 3.14m);
			SetShipmentWeightAndVolume(job2, 3400m, 4.71m);
			SetShipmentWeightAndVolume(job3, 1240m, 2.1m);
			SetShipmentWeightAndVolume(job4, 4210m, 1.68m);
			SetShipmentWeightAndVolume(job5, 2705m, 2.9m);
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost.E6_RX_NKCurrency = Core.Constants.CurrencyCodes.Indonesia;
			cost.E6_ExchangeRate = 10000m;
			cost.E6_ApportionmentMethod = ZArchitecture.Core.AllocationMethod.ChargeableUnits;
			cost.E6_OSCostAmount = 400m;
			cost.UpdateApportionmentChargesListing();
			AssertEquals("Local Amount", 0.04m, cost.E6_LocalCostAmount);
			AssertEquals(5, cost.ApportionmentCharges.Count);
			var apportionedCharges = cost.ApportionmentCharges.ToArray<ApportionSplitCharge>();
			var chargesInUse = apportionedCharges.Where(x => x.JR_IsUsedForApportionment);
			var chargesNotInUse = apportionedCharges.Where(x => !x.JR_IsUsedForApportionment);
			AssertEquals("Two apportioned charges should be In Use", 4, chargesInUse.Count());
			AssertEquals("One apportioned charges should not be In Use", 1, chargesNotInUse.Count());
			Assert("All Local Amounts In Use must be 0.01", chargesInUse.All(x => x.JR_LocalCostAmt == 0.01m));
			AssertEquals("Charge Not In Use with 0 OS Amount", ZDecimal.Zero, chargesNotInUse.FirstOrDefault().JR_OSCostAmt);
			AssertEquals("Charge Not In Use with 0 Local Amount", ZDecimal.Zero, chargesNotInUse.FirstOrDefault().JR_LocalCostAmt);
		}

		Job AddNewJobShipment(ForwardingConsol consol)
		{
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = "JS";
			AssertNotNull("Shipment should have a Job", shipment.Job);
			return job;
		}

		void SetShipmentWeightAndVolume(Job job, ZDecimal weight, ZDecimal volume)
		{
			var shipment = Factory.Load<ForwardingShipment>(job.JH_ParentID);
			AssertNotNull("Job Shipment", shipment);
			shipment.JS_ActualWeight = weight;
			shipment.JS_ActualVolume = volume;
		}

		public void TestDetachedShipmentValidationErrorRaised()
		{
			AssertDetachedShipmentValidationErrorRaised(false);
		}

		public void TestDetachedShipmentValidationErrorIsNotRaised()
		{
			AssertDetachedShipmentValidationErrorRaised(true);
		}

		void AssertDetachedShipmentValidationErrorRaised(bool isShipmentLinkedToConsol)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00009999";
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00009999";
			consol.Shipments.Add(shipment);
			Factory.Save();
			var apportionmentListing = new ApportionmentListing(Factory, consol);
			var cost = apportionmentListing.CostsCollection.TryAddNew();
			AssertNotNull(cost);
			cost.E6_OSCostAmount = 100m;
			cost.E6_ApportionmentMethod = "SHP";
			cost.E6_PPDCLT = "ALL";
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost.E6_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals("Precondition: ApportionSplitCharge should be created.", 1, cost.ApportionmentCharges.Count);
			Factory.Save();
			if (!isShipmentLinkedToConsol)
			{
				consol.Shipments.Remove(shipment);
				Factory.Save();
				AssertEquals("Precondition: no shipments should be linked to consol.", 0, consol.Shipments.Count);
			}

			consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			apportionmentListing = new ApportionmentListing(consol.Factory, consol);
			cost = apportionmentListing.CostsCollection[0];
			cost.UpdateApportionmentChargesListing();
			var charge = cost.ApportionmentCharges[0];
			charge.RunPreSaveValidation();
			if (isShipmentLinkedToConsol)
			{
				AssertNoErrors(charge.JR_IsUsedForApportionmentInfo);
			}
			else
			{
				AssertHasError("JR_IsUsedForApportionment should have error", charge.JR_IsUsedForApportionmentInfo, "This apportioned charge belongs to job: " + charge.JR_JobNumber + " which is no longer attached to the consol or is inactive. Please untick Is Used or re-attach job to consol/activate job.");
			}
		}

		public void TestGSTInclusiveAmountInfo()
		{
			var consol = Factory.New<ForwardingConsol>();
			var currentCompanyCost = consol.GetApportionments().CostsCollection.TryAddNew();
			currentCompanyCost.E6_GC = GlbCompany.CurrentCompany.PK;
			var invoice = Factory.New<APInvoice>();
			var testCost = AddNewConsolCostToInvoice(consol, invoice);
			invoice.GSTInclusiveAmounts = false;
			Assert("GSTInclusiveAmount should be readonly.", testCost.GSTInclusiveAmountInfo.ReadOnly);
			invoice.GSTInclusiveAmounts = true;
			Assert("GSTInclusiveAmount shouldn't be readonly.", !testCost.GSTInclusiveAmountInfo.ReadOnly);
		}

		public void TestGSTInclusiveAmount()
		{
			var consol = Factory.New<ForwardingConsol>();
			var invoice = Factory.New<APInvoice>();
			var testCost = AddNewConsolCostToInvoice(consol, invoice);
			testCost.E6_AT_TaxRate = ObjectCreator.CreateTaxRate("TestGST", "", 10).PK;
			testCost.E6_RX_NKCurrency = ObjectCreator.AUD.RX_Code;
			testCost.E6_OSCostAmount = 120m;
			invoice.GSTInclusiveAmounts = false;
			testCost.GSTInclusiveAmount = 77m;
			AssertEquals("AL_OSExTaxAmount should not be changed.", 120m, testCost.E6_OSCostAmount);
			AssertEquals("AL_OSTaxAmount should not be changed.", 12m, testCost.E6_OSGSTAmount_Calc);
			invoice.GSTInclusiveAmounts = true;
			testCost.GSTInclusiveAmount = 11m;
			AssertEquals("AL_OSExTaxAmount should be changed.", 10m, testCost.E6_OSCostAmount);
			AssertEquals("AL_OSTaxAmount should be changed.", 1m, testCost.E6_OSGSTAmount_Calc);
			testCost.GSTInclusiveAmount = 11.333m;
			AssertEquals("GSTInclusiveAmount should be rounded.", 11.33m, testCost.GSTInclusiveAmount);
			AssertEquals("AL_OSExTaxAmount should be rounded.", 10.30m, testCost.E6_OSCostAmount);
			AssertEquals("AL_OSTaxAmount should be rounded.", 1.03m, testCost.E6_OSGSTAmount_Calc);
			invoice.GSTInclusiveAmountNeedUpdate = false;
			invoice.GSTInclusiveAmounts = false;
			testCost.E6_OSCostAmount = 11m;
			AssertEquals("GSTInclusiveAmount should be updated.", 12.1m, testCost.GSTInclusiveAmount);
			testCost.E6_OSGSTAmount_Calc = 11m;
			AssertEquals("GSTInclusiveAmount should be updated.", 22m, testCost.GSTInclusiveAmount);
			invoice.GSTInclusiveAmounts = true;
			testCost.GSTInclusiveAmount = 333m;
			AssertEquals("GSTInclusiveAmount should be updated.", 333m, testCost.GSTInclusiveAmount);
			testCost.E6_OSCostAmount = 22m;
			AssertEquals("GSTInclusiveAmount should not be updated.", 333m, testCost.GSTInclusiveAmount);
			testCost.E6_OSGSTAmount_Calc = 22m;
			AssertEquals("GSTInclusiveAmount should not be updated.", 333m, testCost.GSTInclusiveAmount);
			invoice.GSTInclusiveAmountNeedUpdate = true;
			invoice.GSTInclusiveAmounts = false;
			testCost.E6_OSCostAmount = 11m;
			AssertEquals("GSTInclusiveAmount should be updated.", 12.1m, testCost.GSTInclusiveAmount);
			testCost.E6_OSGSTAmount_Calc = 11m;
			AssertEquals("GSTInclusiveAmount should be updated.", 22m, testCost.GSTInclusiveAmount);
			invoice.GSTInclusiveAmounts = true;
			testCost.GSTInclusiveAmount = 333m;
			AssertEquals("GSTInclusiveAmount should be updated.", 22m, testCost.GSTInclusiveAmount);
			testCost.E6_OSCostAmount = 22m;
			AssertEquals("GSTInclusiveAmount should be updated.", 24.2m, testCost.GSTInclusiveAmount);
			testCost.E6_OSGSTAmount_Calc = 22m;
			AssertEquals("GSTInclusiveAmount should be updated.", 44m, testCost.GSTInclusiveAmount);
		}

		[ExpectNoExceptions]
		public void TestOSCostAmountDoesNotOverflow()
		{
			var consol = Factory.New<ForwardingConsol>();
			var invoice = Factory.New<APInvoice>();
			var testCost = AddNewConsolCostToInvoice(consol, invoice);
			testCost.E6_AT_TaxRate = ObjectCreator.CreateTaxRate("TestGST", "", 10).PK;
			testCost.E6_RX_NKCurrency = ObjectCreator.AUD.RX_Code;
			testCost.E6_OSCostAmount = Decimal.MaxValue / 2;
		}

		public void TestGSTInclusiveAmounts()
		{
			var consol = Factory.New<ForwardingConsol>();
			var invoice = Factory.New<APInvoice>();
			var testCost = AddNewConsolCostToInvoice(consol, invoice);
			invoice.GSTInclusiveAmounts = true;
			testCost.E6_OSCostAmount = 120m;
			testCost.E6_OSGSTAmount_Calc = 50m;
			Assert(testCost.IsGSTInclusiveAmount);
			testCost.Validation.ValidateGSTInclusiveAmount();
			Assert("Should has errors.", testCost.GSTInclusiveAmountInfo.HasErrors());
			invoice.GSTInclusiveAmounts = false;
			Assert(!testCost.IsGSTInclusiveAmount);
			testCost.Validation.ValidateGSTInclusiveAmount();
			Assert("Should hasn't errors.", !testCost.GSTInclusiveAmountInfo.HasErrors());
		}

		public void TestCostCurrencyDecimals()
		{
			var consol = Factory.New<ForwardingConsol>();
			var invoice = Factory.New<APInvoice>();
			var testCost = AddNewConsolCostToInvoice(consol, invoice);
			testCost.E6_RX_NKCurrency = ZString.Empty;
			AssertNull("Currency", testCost.Currency);
			AssertEquals("Pre-condition: Decimal places of current company local currency", GlbCompany.CurrentCompany.LocalCurrency.Decimals, testCost.CurrencyDecimals);
			testCost.E6_RX_NKCurrency = "IDR";
			AssertEquals("Decimal places of assigned currency", RefCurrency.LoadFromCurrencyCode(Factory, "IDR").Decimals, testCost.CurrencyDecimals);
		}

		public void TestOSCostTaxAmounts_WhenOverridden()
		{
			var consol = Factory.New<ForwardingConsol>();
			var testGSTRate = ObjectCreator.CreateTaxRate("TestGST", "", 10);
			var testGSTChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			testGSTChargeCode.AC_AT_GSTRate = testGSTRate.PK;
			testGSTChargeCode.AC_Code = "GSTCC";
			testGSTChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			var invoice = Factory.NewWithValidTestData<APInvoice>();

			#region GST

			var testCostGST = AddNewConsolCostToInvoice(consol, invoice);
			testCostGST.E6_AC_ChargeCode = testGSTChargeCode.PK;
			testCostGST.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			testCostGST.E6_AT_TaxRate = testGSTRate.PK;
			testCostGST.E6_IsTaxAmountOverridden = true;
			testCostGST.E6_RX_NKCurrency = ObjectCreator.AUD.RX_Code;
			testCostGST.E6_OSCostAmount = 100m;
			testCostGST.E6_OSGSTAmount_Calc = 10M;

			ApportionSplitCharge charge = testCostGST.ApportionmentCharges.AddNew();
			charge.JR_OH_CostAccount = ObjectCreator.AALSHI.PK;
			charge.JR_AC = testGSTChargeCode.PK;
			charge.JR_APInvoiceDate = testCostGST.E6_InvoiceDate;
			charge.JR_PaymentDate = testCostGST.E6_PaymentDate;
			charge.JR_OSCostAmt = 100m;
			charge.JR_LocalCostAmt = 100m;
			charge.JR_OSCostGSTAmt_Calc = 10m;
			ObjectCreator.JobHeader1.JH_JobNum = "TST555";
			ObjectCreator.JobHeader1.JH_ParentID = ObjectCreator.CreateShipment("99898900").PK;
			ObjectCreator.JobHeader1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			charge.JR_JH = ObjectCreator.JobHeader1.PK;
			AssertEquals("E6_OSCostAmount should not be changed.", 100m, testCostGST.E6_OSCostAmount);
			AssertEquals("E6_OSGSTAmount_Calc should be set.", 10m, testCostGST.E6_OSGSTAmount_Calc);
			AssertEquals("E6_OSExtraTaxAmount Amount should not be set.", ZDecimal.Zero, testCostGST.E6_OSExtraTaxAmount);
			AssertEquals("E6_Calc_OSTotalAmount should be set.", 110m, testCostGST.E6_Calc_OSTotalAmount);

			Factory.Save();

			testCostGST = new BusinessObjectFactory().Load<JobConsolCost>(testCostGST.PK);
			Assert(!testCostGST.HasChanges);
			AssertEquals("E6_OSCostAmount should not be changed.", 100m, testCostGST.E6_OSCostAmount);
			AssertEquals("E6_OSGSTAmount_Calc should be set.", 10m, testCostGST.E6_OSGSTAmount_Calc);
			AssertEquals("E6_OSExtraTaxAmount Amount should not be set.", ZDecimal.Zero, testCostGST.E6_OSExtraTaxAmount);
			AssertEquals("E6_Calc_OSTotalAmount should be set", 110m, testCostGST.E6_Calc_OSTotalAmount);

			#endregion

			#region GSTANDQST

			AccTaxRate gSTANDQSTRate = Factory.New<AccTaxRate>();
			gSTANDQSTRate.AT_Code = "GSTANDQST";
			gSTANDQSTRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			gSTANDQSTRate.SetRateNumerator_ForTestOnly(5);
			gSTANDQSTRate.SetExtraRate_ForTestOnly(75, 10);
			gSTANDQSTRate.AT_Type = AccTaxRate.Types.Rated;
			gSTANDQSTRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQST;

			AccChargeCode gSTANDQSTChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			gSTANDQSTChargeCode.AC_AT_GSTRate = gSTANDQSTRate.PK;
			gSTANDQSTChargeCode.AC_Code = "QSTCC";
			gSTANDQSTChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;

			var testCostQST = AddNewConsolCostToInvoice(consol, invoice);
			testCostQST.E6_AC_ChargeCode = gSTANDQSTChargeCode.PK;
			testCostQST.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			testCostQST.E6_AT_TaxRate = gSTANDQSTRate.PK;
			testCostQST.E6_IsTaxAmountOverridden = true;
			testCostQST.E6_RX_NKCurrency = ObjectCreator.AUD.RX_Code;
			testCostQST.E6_OSCostAmount = 100m;
			testCostQST.E6_OSGSTAmount_Calc = 12.88m;
			AssertEquals("E6_OSCostAmount should not be changed.", 100m, testCostQST.E6_OSCostAmount);
			AssertEquals("E6_OSGSTAmount_Calc should be set.", 12.88m, testCostQST.E6_OSGSTAmount_Calc);
			AssertEquals("E6_OSExtraTaxAmount Amount should be set.", 7.88m, testCostQST.E6_OSExtraTaxAmount);
			AssertEquals("E6_Calc_OSTotalAmount should be set.", 112.88m, testCostQST.E6_Calc_OSTotalAmount);
			charge = testCostQST.ApportionmentCharges.AddNew();
			charge.JR_OH_CostAccount = ObjectCreator.AALSHI.PK;
			charge.JR_AC = testGSTChargeCode.PK;
			charge.JR_APInvoiceDate = testCostQST.E6_InvoiceDate;
			charge.JR_PaymentDate = testCostQST.E6_PaymentDate;
			charge.JR_JH = ObjectCreator.JobHeader1.PK;
			charge.JR_OSCostAmt = 100m;
			charge.JR_LocalCostAmt = 100m;
			charge.JR_OSCostGSTAmt_Calc = 12.88m;
			Factory.Save();
			testCostQST = new BusinessObjectFactory().Load<JobConsolCost>(testCostQST.PK);
			Assert(!testCostQST.HasChanges);
			AssertEquals("AL_OSExTaxAmount should not be changed.", 100m, testCostQST.E6_OSCostAmount);
			AssertEquals("E6_OSGSTAmount_Calc should be set.", 12.88m, testCostQST.E6_OSGSTAmount_Calc);
			AssertEquals("E6_OSExtraTaxAmount Amount should be set.", 7.88m, testCostQST.E6_OSExtraTaxAmount);
			AssertEquals("E6_Calc_OSTotalAmount should be set.", 112.88m, testCostQST.E6_Calc_OSTotalAmount);

			#endregion

			#region GSTANDEDU

			AccTaxRate gSTANDEDURate = Factory.New<AccTaxRate>();
			gSTANDEDURate.AT_Code = "GSTANDEDU";
			gSTANDEDURate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			gSTANDEDURate.SetRateNumerator_ForTestOnly(10);
			gSTANDEDURate.SetExtraRate_ForTestOnly(3, 1);
			gSTANDEDURate.AT_Type = AccTaxRate.Types.Rated;
			gSTANDEDURate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;

			AccChargeCode gSTANDEDUChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			gSTANDEDUChargeCode.AC_AT_GSTRate = gSTANDEDURate.PK;
			gSTANDEDUChargeCode.AC_Code = "EDUCC";
			gSTANDEDUChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;

			var testCostEDU = AddNewConsolCostToInvoice(consol, invoice);
			testCostEDU.E6_AC_ChargeCode = gSTANDEDUChargeCode.PK;
			testCostEDU.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			testCostEDU.E6_AT_TaxRate = gSTANDEDURate.PK;
			testCostEDU.E6_RX_NKCurrency = ObjectCreator.AUD.RX_Code;
			testCostEDU.E6_IsTaxAmountOverridden = true;
			testCostEDU.E6_OSCostAmount = 100m;
			testCostEDU.E6_OSGSTAmount_Calc = 10.3m;
			AssertEquals("E6_OSCostAmount should not be changed.", 100m, testCostEDU.E6_OSCostAmount);
			AssertEquals("E6_OSGSTAmount_Calc should be set.", 10.3m, testCostEDU.E6_OSGSTAmount_Calc);
			AssertEquals("E6_OSExtraTaxAmount Amount should be set.", 0.3M, testCostEDU.E6_OSExtraTaxAmount);
			AssertEquals("E6_Calc_OSTotalAmount should be set.", 110.3m, testCostEDU.E6_Calc_OSTotalAmount);
			charge = testCostEDU.ApportionmentCharges.AddNew();
			charge.JR_OH_CostAccount = ObjectCreator.AALSHI.PK;
			charge.JR_APInvoiceDate = testCostEDU.E6_InvoiceDate;
			charge.JR_PaymentDate = testCostEDU.E6_PaymentDate;
			charge.JR_AC = testGSTChargeCode.PK;
			charge.JR_JH = ObjectCreator.JobHeader1.PK;
			charge.JR_OSCostAmt = 100m;
			charge.JR_LocalCostAmt = 100m;
			charge.JR_OSCostGSTAmt_Calc = 10.3m;
			Factory.Save();
			testCostEDU = new BusinessObjectFactory().Load<JobConsolCost>(testCostEDU.PK);
			Assert(!testCostEDU.HasChanges);
			AssertEquals("AL_OSExTaxAmount should not be changed.", 100m, testCostEDU.E6_OSCostAmount);
			AssertEquals("E6_OSGSTAmount_Calc should be set.", 10.3m, testCostEDU.E6_OSGSTAmount_Calc);
			AssertEquals("E6_OSExtraTaxAmount Amount should be set.", 0.3M, testCostEDU.E6_OSExtraTaxAmount);
			AssertEquals("E6_Calc_OSTotalAmount should be set.", 110.3m, testCostEDU.E6_Calc_OSTotalAmount);

			#endregion

			#region RET

			AccTaxRate rETRate = Factory.New<AccTaxRate>();
			rETRate.AT_Code = "RET";
			rETRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			rETRate.SetRateNumerator_ForTestOnly(16);
			rETRate.SetExtraRate_ForTestOnly(4, 1);
			rETRate.AT_Type = AccTaxRate.Types.Rated;
			rETRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetention;
			AccChargeCode rETChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			rETChargeCode.AC_AT_GSTRate = rETRate.PK;
			rETChargeCode.AC_Code = "RETCC";
			rETChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			var restCostRET = AddNewConsolCostToInvoice(consol, invoice);
			restCostRET.E6_AC_ChargeCode = rETChargeCode.PK;
			restCostRET.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			restCostRET.E6_AT_TaxRate = rETRate.PK;
			restCostRET.E6_IsTaxAmountOverridden = true;
			restCostRET.E6_RX_NKCurrency = ObjectCreator.AUD.RX_Code;
			restCostRET.E6_OSCostAmount = 100m;
			restCostRET.E6_OSGSTAmount_Calc = 12m;
			AssertEquals("E6_OSCostAmount should not be changed.", 100m, restCostRET.E6_OSCostAmount);
			AssertEquals("E6_OSGSTAmount_Calc should be set.", 12m, restCostRET.E6_OSGSTAmount_Calc);
			AssertEquals("E6_OSExtraTaxAmount Amount should be set.", -4m, restCostRET.E6_OSExtraTaxAmount);
			AssertEquals("E6_Calc_OSTotalAmount should be set.", 112m, restCostRET.E6_Calc_OSTotalAmount);
			charge = restCostRET.ApportionmentCharges.AddNew();
			charge.JR_OH_CostAccount = ObjectCreator.AALSHI.PK;
			charge.JR_AC = testGSTChargeCode.PK;
			charge.JR_APInvoiceDate = restCostRET.E6_InvoiceDate;
			charge.JR_PaymentDate = restCostRET.E6_PaymentDate;
			charge.JR_JH = ObjectCreator.JobHeader1.PK;
			charge.JR_OSCostAmt = 100m;
			charge.JR_LocalCostAmt = 100m;
			charge.JR_OSCostGSTAmt_Calc = 12m;
			Factory.Save();
			restCostRET = new BusinessObjectFactory().Load<JobConsolCost>(restCostRET.PK);
			Assert(!restCostRET.HasChanges);
			AssertEquals("AL_OSExTaxAmount should not be changed.", 100m, restCostRET.E6_OSCostAmount);
			AssertEquals("E6_OSGSTAmount_Calc should be set.", 12m, restCostRET.E6_OSGSTAmount_Calc);
			AssertEquals("E6_OSExtraTaxAmount Amount should be set.", -4m, restCostRET.E6_OSExtraTaxAmount);
			AssertEquals("E6_Calc_OSTotalAmount should be set.", 112m, restCostRET.E6_Calc_OSTotalAmount);

			#endregion

			#region GSTAndQST

			AccTaxRate gSTAndQSTBasedOnQCTRate = Factory.New<AccTaxRate>();
			gSTAndQSTBasedOnQCTRate.AT_Code = "QCT";
			gSTAndQSTBasedOnQCTRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			gSTAndQSTBasedOnQCTRate.SetRateNumerator_ForTestOnly(5);
			gSTAndQSTBasedOnQCTRate.SetExtraRate_ForTestOnly(9975, 1000);
			gSTAndQSTBasedOnQCTRate.AT_Type = AccTaxRate.Types.Rated;
			gSTAndQSTBasedOnQCTRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
			AccChargeCode gSTAndQSTBasedOnQCTChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			gSTAndQSTBasedOnQCTChargeCode.AC_AT_GSTRate = gSTAndQSTBasedOnQCTRate.PK;
			gSTAndQSTBasedOnQCTChargeCode.AC_Code = "QCTCC";
			gSTAndQSTBasedOnQCTChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;

			var testCostGSTAndQSTBasedOnQCT = AddNewConsolCostToInvoice(consol, invoice);
			testCostGSTAndQSTBasedOnQCT.E6_AC_ChargeCode = gSTAndQSTBasedOnQCTChargeCode.PK;
			testCostGSTAndQSTBasedOnQCT.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			testCostGSTAndQSTBasedOnQCT.E6_AT_TaxRate = gSTAndQSTBasedOnQCTRate.PK;
			testCostGSTAndQSTBasedOnQCT.E6_IsTaxAmountOverridden = true;
			testCostGSTAndQSTBasedOnQCT.E6_RX_NKCurrency = ObjectCreator.AUD.RX_Code;
			testCostGSTAndQSTBasedOnQCT.E6_OSCostAmount = 100m;
			testCostGSTAndQSTBasedOnQCT.E6_OSGSTAmount_Calc = 14.98m;
			AssertEquals("E6_OSCostAmount should not be changed.", 100m, testCostGSTAndQSTBasedOnQCT.E6_OSCostAmount);
			AssertEquals("E6_OSGSTAmount_Calc should be set.", 14.98m, testCostGSTAndQSTBasedOnQCT.E6_OSGSTAmount_Calc);
			AssertEquals("E6_OSExtraTaxAmount Amount should be set.", 9.98m, testCostGSTAndQSTBasedOnQCT.E6_OSExtraTaxAmount);
			AssertEquals("E6_Calc_OSTotalAmount should be set.", 114.98m, testCostGSTAndQSTBasedOnQCT.E6_Calc_OSTotalAmount);
			charge = testCostGSTAndQSTBasedOnQCT.ApportionmentCharges.AddNew();
			charge.JR_OH_CostAccount = ObjectCreator.AALSHI.PK;
			charge.JR_AC = testGSTChargeCode.PK;
			charge.JR_APInvoiceDate = testCostGSTAndQSTBasedOnQCT.E6_InvoiceDate;
			charge.JR_PaymentDate = testCostGSTAndQSTBasedOnQCT.E6_PaymentDate;
			charge.JR_JH = ObjectCreator.JobHeader1.PK;
			charge.JR_OSCostAmt = 100m;
			charge.JR_LocalCostAmt = 100m;
			charge.JR_OSCostGSTAmt_Calc = 14.98m;
			Factory.Save();
			testCostGSTAndQSTBasedOnQCT = new BusinessObjectFactory().Load<JobConsolCost>(testCostGSTAndQSTBasedOnQCT.PK);
			Assert(!testCostGSTAndQSTBasedOnQCT.HasChanges);
			AssertEquals("AL_OSExTaxAmount should not be changed.", 100m, testCostGSTAndQSTBasedOnQCT.E6_OSCostAmount);
			AssertEquals("E6_OSGSTAmount_Calc should be set.", 14.98m, testCostGSTAndQSTBasedOnQCT.E6_OSGSTAmount_Calc);
			AssertEquals("E6_OSExtraTaxAmount Amount should be set.", 9.98m, testCostGSTAndQSTBasedOnQCT.E6_OSExtraTaxAmount);
			AssertEquals("E6_Calc_OSTotalAmount should be set.", 114.98m, testCostGSTAndQSTBasedOnQCT.E6_Calc_OSTotalAmount);

			#endregion

			#region OTO6Rate

			AccTaxRate oTO6Rate = Factory.New<AccTaxRate>();
			oTO6Rate.AT_Code = "OTO6";
			oTO6Rate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			oTO6Rate.SetRateNumerator_ForTestOnly(0);
			oTO6Rate.SetExtraRate_ForTestOnly(6, 1);
			oTO6Rate.AT_Type = AccTaxRate.Types.Rated;
			oTO6Rate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.ChinaInputVATOffsetAgainstOutputTax;
			AccChargeCode oTO6ChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			oTO6ChargeCode.AC_AT_GSTRate = gSTAndQSTBasedOnQCTRate.PK;
			oTO6ChargeCode.AC_Code = "OTOCC";
			oTO6ChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			var testCostOTO6 = AddNewConsolCostToInvoice(consol, invoice);
			testCostOTO6.E6_AC_ChargeCode = oTO6ChargeCode.PK;
			testCostOTO6.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			testCostOTO6.E6_AT_TaxRate = oTO6Rate.PK;
			testCostOTO6.E6_IsTaxAmountOverridden = true;
			testCostOTO6.E6_RX_NKCurrency = ObjectCreator.AUD.RX_Code;
			testCostOTO6.E6_OSCostAmount = 100m;
			testCostOTO6.E6_OSGSTAmount_Calc = 6m;
			AssertEquals("E6_OSCostAmount should not be changed.", 100m, testCostOTO6.E6_OSCostAmount);
			AssertEquals("E6_OSGSTAmount_Calc should be set.", 6m, testCostOTO6.E6_OSGSTAmount_Calc);
			AssertEquals("E6_OSExtraTaxAmount Amount should be set.", 6m, testCostOTO6.E6_OSExtraTaxAmount);
			AssertEquals("E6_Calc_OSTotalAmount should be set.", 106m, testCostOTO6.E6_Calc_OSTotalAmount);
			charge = testCostOTO6.ApportionmentCharges.AddNew();
			charge.JR_AC = testGSTChargeCode.PK;
			charge.JR_APInvoiceDate = testCostOTO6.E6_InvoiceDate;
			charge.JR_PaymentDate = testCostOTO6.E6_PaymentDate;
			charge.JR_OH_CostAccount = ObjectCreator.AALSHI.PK;
			charge.JR_JH = ObjectCreator.JobHeader1.PK;
			charge.JR_OSCostAmt = 100m;
			charge.JR_LocalCostAmt = 100m;
			charge.JR_OSCostGSTAmt_Calc = 6m;
			Factory.Save();
			testCostOTO6 = new BusinessObjectFactory().Load<JobConsolCost>(testCostOTO6.PK);
			Assert(!testCostOTO6.HasChanges);
			AssertEquals("AL_OSExTaxAmount should not be changed.", 100m, testCostOTO6.E6_OSCostAmount);
			AssertEquals("E6_OSGSTAmount_Calc should be set.", 6m, testCostOTO6.E6_OSGSTAmount_Calc);
			AssertEquals("E6_OSExtraTaxAmount Amount should be set.", 6m, testCostOTO6.E6_OSExtraTaxAmount);
			AssertEquals("E6_Calc_OSTotalAmount should be set.", 106m, testCostOTO6.E6_Calc_OSTotalAmount);

			#endregion
		}

		public void TestOSCostTaxAmounts_WhenNotOverridden()
		{
			var consol = Factory.New<ForwardingConsol>();
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			ObjectCreator.JobHeader1.JH_JobNum = "S0001";
			ObjectCreator.JobHeader1.JH_ParentID = ObjectCreator.CreateShipment("S0001").PK;
			ObjectCreator.JobHeader1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			#region GST

			var testGSTRate = ObjectCreator.CreateTaxRate("TestGST", "", 10);
			var testGSTChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			testGSTChargeCode.AC_AT_GSTRate = testGSTRate.PK;
			testGSTChargeCode.AC_Code = "GSTCC";
			testGSTChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;

			CreateCostAndAssertGST(testGSTChargeCode, testGSTRate, 100M, 10M, 0M);

			#endregion

			#region GSTANDQST

			var gSTANDQSTRate = Factory.New<AccTaxRate>();
			gSTANDQSTRate.AT_Code = "GSTANDQST";
			gSTANDQSTRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			gSTANDQSTRate.SetRateNumerator_ForTestOnly(5);
			gSTANDQSTRate.SetExtraRate_ForTestOnly(75, 10);
			gSTANDQSTRate.AT_Type = AccTaxRate.Types.Rated;
			gSTANDQSTRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQST;

			var gSTANDQSTChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			gSTANDQSTChargeCode.AC_AT_GSTRate = gSTANDQSTRate.PK;
			gSTANDQSTChargeCode.AC_Code = "QSTCC";
			gSTANDQSTChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;

			CreateCostAndAssertGST(gSTANDQSTChargeCode, gSTANDQSTRate, 100M, 12.88M, 7.88M);

			#endregion

			#region GSTANDEDU

			AccTaxRate gSTANDEDURate = Factory.New<AccTaxRate>();
			gSTANDEDURate.AT_Code = "GSTANDEDU";
			gSTANDEDURate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			gSTANDEDURate.SetRateNumerator_ForTestOnly(10);
			gSTANDEDURate.SetExtraRate_ForTestOnly(3, 1);
			gSTANDEDURate.AT_Type = AccTaxRate.Types.Rated;
			gSTANDEDURate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;

			AccChargeCode gSTANDEDUChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			gSTANDEDUChargeCode.AC_AT_GSTRate = gSTANDEDURate.PK;
			gSTANDEDUChargeCode.AC_Code = "EDUCC";
			gSTANDEDUChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;

			CreateCostAndAssertGST(gSTANDEDUChargeCode, gSTANDEDURate, 100M, 10.3M, 0.3M);

			#endregion

			#region RET

			var rETRate = Factory.New<AccTaxRate>();
			rETRate.AT_Code = "RET";
			rETRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			rETRate.SetRateNumerator_ForTestOnly(16);
			rETRate.SetExtraRate_ForTestOnly(4, 1);
			rETRate.AT_Type = AccTaxRate.Types.Rated;
			rETRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetention;

			var rETChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			rETChargeCode.AC_AT_GSTRate = rETRate.PK;
			rETChargeCode.AC_Code = "RETCC";
			rETChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;

			CreateCostAndAssertGST(rETChargeCode, rETRate, 100M, 12M, -4M);

			#endregion

			#region GSTAndQST

			AccTaxRate gSTAndQSTBasedOnQCTRate = Factory.New<AccTaxRate>();
			gSTAndQSTBasedOnQCTRate.AT_Code = "QCT";
			gSTAndQSTBasedOnQCTRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			gSTAndQSTBasedOnQCTRate.SetRateNumerator_ForTestOnly(5);
			gSTAndQSTBasedOnQCTRate.SetExtraRate_ForTestOnly(9975, 1000);
			gSTAndQSTBasedOnQCTRate.AT_Type = AccTaxRate.Types.Rated;
			gSTAndQSTBasedOnQCTRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;

			AccChargeCode gSTAndQSTBasedOnQCTChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			gSTAndQSTBasedOnQCTChargeCode.AC_AT_GSTRate = gSTAndQSTBasedOnQCTRate.PK;
			gSTAndQSTBasedOnQCTChargeCode.AC_Code = "QCTCC";
			gSTAndQSTBasedOnQCTChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;

			CreateCostAndAssertGST(gSTAndQSTBasedOnQCTChargeCode, gSTAndQSTBasedOnQCTRate, 100M, 14.98M, 9.98M);

			#endregion

			#region OTO6Rate

			AccTaxRate oTO6Rate = Factory.New<AccTaxRate>();
			oTO6Rate.AT_Code = "OTO6";
			oTO6Rate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			oTO6Rate.SetRateNumerator_ForTestOnly(0);
			oTO6Rate.SetExtraRate_ForTestOnly(6, 1);
			oTO6Rate.AT_Type = AccTaxRate.Types.Rated;
			oTO6Rate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.ChinaInputVATOffsetAgainstOutputTax;

			AccChargeCode oTO6ChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			oTO6ChargeCode.AC_AT_GSTRate = gSTAndQSTBasedOnQCTRate.PK;
			oTO6ChargeCode.AC_Code = "OTOCC";
			oTO6ChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;

			CreateCostAndAssertGST(oTO6ChargeCode, oTO6Rate, 100M, 6M, 6M);

			#endregion

			void CreateCostAndAssertGST(AccChargeCode chargeCode, AccTaxRate taxRate, ZDecimal costAmount, ZDecimal expectedTaxAmount, ZDecimal expectedExtraTaxAmount)
			{
				var cost = AddNewConsolCostToInvoice(consol, invoice);
				cost.E6_AC_ChargeCode = chargeCode.PK;
				cost.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
				cost.E6_AT_TaxRate = taxRate.PK;
				cost.E6_RX_NKCurrency = ObjectCreator.AUD.RX_Code;
				cost.E6_OSCostAmount = costAmount;

				ApportionSplitCharge apCharge = cost.ApportionmentCharges.AddNew();
				apCharge.JR_OH_CostAccount = ObjectCreator.AALSHI.PK;
				apCharge.JR_AC = chargeCode.PK;
				apCharge.JR_APInvoiceDate = cost.E6_InvoiceDate;
				apCharge.JR_PaymentDate = cost.E6_PaymentDate;
				apCharge.JR_OSCostAmt = costAmount;
				apCharge.JR_LocalCostAmt = costAmount;
				apCharge.JR_JH = ObjectCreator.JobHeader1.PK;
				AssertEquals("E6_OSCostAmount should not be changed.", costAmount, cost.E6_OSCostAmount);
				AssertEquals("E6_OSGSTAmount_Calc should be set.", expectedTaxAmount, cost.E6_OSGSTAmount_Calc);
				var notString = expectedExtraTaxAmount > 0 ? string.Empty : "NOT";
				AssertEquals($"E6_OSExtraTaxAmount Amount should {notString} be set.", expectedExtraTaxAmount, cost.E6_OSExtraTaxAmount);
				AssertEquals("E6_Calc_OSTotalAmount should be set.", costAmount + expectedTaxAmount, cost.E6_Calc_OSTotalAmount);

				Factory.Save();

				var reloadeCost = new BusinessObjectFactory().Load<JobConsolCost>(cost.PK);
				Assert(!reloadeCost.HasChanges);
				AssertEquals("E6_OSCostAmount should not be changed.", costAmount, reloadeCost.E6_OSCostAmount);
				AssertEquals("E6_OSGSTAmount_Calc should be set.", expectedTaxAmount, reloadeCost.E6_OSGSTAmount_Calc);
				AssertEquals("E6_OSExtraTaxAmount Amount should not be set.", expectedExtraTaxAmount, reloadeCost.E6_OSExtraTaxAmount);
				AssertEquals("E6_Calc_OSTotalAmount should be set", costAmount + expectedTaxAmount, reloadeCost.E6_Calc_OSTotalAmount);
			}
		}

		public void TestCostPaymentType()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			consol.JK_UniqueConsignRef = "X00001000";
			JobConsolCost cost = GetCost(consol);
			cost.E6_OSCostAmount = 100m;
			cost.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			cost.E6_InvoiceNum = "ABC123";
			cost.E6_InvoiceDate = ZDateTime.Now;
			Assert("ChequeBook field should not be readonly", cost.E6_AK_ChequeBookInfo.ReadOnly);
			foreach (ICodeDescription paymentMethod in cost.Lookups.PaymentMethodList)
			{
				cost.E6_AK_ChequeBook = ObjectCreator.AUDChequeBook.PK;
				cost.E6_PaymentType = paymentMethod.Code;
				if (paymentMethod.Code == ReceiptTypes.Cheque)
				{
					Assert("ChequeBook should not be readonly", !cost.E6_AK_ChequeBookInfo.ReadOnly);
				}
				else
				{
					Assert("ChequeBook field should be cleared", cost.E6_AK_ChequeBook.IsEmpty);
					Assert("ChequeBook should be readonly", cost.E6_AK_ChequeBookInfo.ReadOnly);
				}
			}
		}

		public void TestIsChequeNumberAutoAllocated()
		{
			var testBank = Factory.NewWithValidTestData<AccBankAccount>();
			var testBookWithAutoAllocation = GetAutoPrintChequeBook(testBank);
			testBookWithAutoAllocation.AK_StartNo = 1;
			testBookWithAutoAllocation.AK_CurrentNo = 1;
			testBookWithAutoAllocation.AK_LastNo = 3;
			var testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_StartNo = 1;
			testChequeBook.AK_CurrentNo = 1;
			testChequeBook.AK_LastNo = 3;
			var consol = Factory.New<ForwardingConsol>();
			var invoice = Factory.New<APInvoice>();
			var testCost = AddNewConsolCostToInvoice(consol, invoice);
			testCost.E6_PaymentType = ReceiptTypes.Cash;
			testCost.E6_AB_BankAccount = testBank.PK;
			Assert("ChequeBook is not set, should return False", !testCost.IsChequeNumberAutoAllocated);
			Assert("ChequeOrReference field should not be read only", !testCost.E6_ChequeOrReferenceInfo.ReadOnly);
			testCost.E6_AK_ChequeBook = testBookWithAutoAllocation.PK;
			Assert("Payment if of type Cash, should return False", !testCost.IsChequeNumberAutoAllocated);
			Assert("ChequeOrReference field should not be read only", !testCost.E6_ChequeOrReferenceInfo.ReadOnly);
			testCost.E6_PaymentType = ReceiptTypes.Cheque;
			testCost.E6_AK_ChequeBook = testBookWithAutoAllocation.PK;
			Assert("AutoAllocation should be enabled", testCost.IsChequeNumberAutoAllocated);
			testCost.E6_AK_ChequeBook = testChequeBook.PK;
			Assert("Cheque Book is not IsAutoPrint, should return False", !testCost.IsChequeNumberAutoAllocated);
			Assert("ChequeOrReference field should not be read only", !testCost.E6_ChequeOrReferenceInfo.ReadOnly);
		}

		public void TestCalc_ChequeNumberIsAutoAllocatedLabel()
		{
			var testBank = Factory.NewWithValidTestData<AccBankAccount>();
			var testBookWithAutoAllocation = GetAutoPrintChequeBook(testBank);
			var testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			var consol = Factory.New<ForwardingConsol>();
			var invoice = Factory.New<APInvoice>();
			var testCost = AddNewConsolCostToInvoice(consol, invoice);
			testCost.E6_PaymentType = ReceiptTypes.Cheque;
			testCost.E6_AB_BankAccount = testBank.PK;
			Assert("ChequeBook is not set, should return False", !testCost.IsChequeNumberAutoAllocated);
			Assert("Label should be empty yet", testCost.Calc_ChequeNumberIsAutoAllocatedLabel.IsEmpty);
			testCost.E6_AK_ChequeBook = testBookWithAutoAllocation.PK;
			Assert("AutoAllocation should be enabled", testCost.IsChequeNumberAutoAllocated);
			AssertEquals("Label should be set to right value", AccountingConstants.ChequeLabelConstants.ChequeNumberIsAutoAllocatedLabel, testCost.Calc_ChequeNumberIsAutoAllocatedLabel);
			testCost.E6_ChequeOrReference = "BLAHBLAHBLAH!";
			Assert("Label should become empty as E6_ChequeOrReference is not empty (means it was autoallocated already)", testCost.Calc_ChequeNumberIsAutoAllocatedLabel.IsEmpty);
		}

		public void TestPushUnApportionedAmountToGreatestCharge()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "SGSIN";
			Factory.Save();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 351.73m;
			shipment1.JS_ActualChargeable = 43m;
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ActualWeight = 786.23m;
			shipment2.JS_ActualChargeable = 5m;
			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_ActualWeight = 76.31m;
			shipment3.JS_ActualChargeable = 37m;
			var shipment4 = consol.Shipments.AddNew();
			shipment4.JS_ActualWeight = 486.73m;
			shipment4.JS_ActualChargeable = 31m;
			var shipment5 = consol.Shipments.AddNew();
			shipment5.JS_ActualWeight = 151.73m;
			shipment5.JS_ActualChargeable = 43m;
			var shipment6 = consol.Shipments.AddNew();
			shipment6.JS_ActualWeight = 737.23m;
			shipment6.JS_ActualChargeable = 52m;
			var shipment7 = consol.Shipments.AddNew();
			shipment7.JS_ActualWeight = 976.31m;
			shipment7.JS_ActualChargeable = 337m;
			var shipment8 = consol.Shipments.AddNew();
			shipment8.JS_ActualWeight = 86.73m;
			shipment8.JS_ActualChargeable = 1m;
			var cost = GetCost(consol);
			try
			{
				var infoCollector = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
				cost.E6_IsTaxAmountOverridden = true;
				ApportionSplitCharge charge1 = FindChargeForJob(cost, shipment1);
				ApportionSplitCharge charge2 = FindChargeForJob(cost, shipment2);
				ApportionSplitCharge charge3 = FindChargeForJob(cost, shipment3);
				ApportionSplitCharge charge4 = FindChargeForJob(cost, shipment4);
				ApportionSplitCharge charge5 = FindChargeForJob(cost, shipment5);
				ApportionSplitCharge charge6 = FindChargeForJob(cost, shipment6);
				ApportionSplitCharge charge7 = FindChargeForJob(cost, shipment7);
				ApportionSplitCharge charge8 = FindChargeForJob(cost, shipment8);
				cost.E6_OSCostAmount = 1840m;
				cost.E6_OSGSTAmount_Calc = 184m;
				cost.E6_ApportionmentMethod = AllocationMethod.ChargeableUnits;
				AssertEquals(144.11m, charge1.JR_OSCostAmt);
				AssertEquals(14.41m, charge1.JR_OSCostGSTAmt_Calc);
				AssertEquals(16.76m, charge2.JR_OSCostAmt);
				AssertEquals(1.68m, charge2.JR_OSCostGSTAmt_Calc);
				AssertEquals(124.01m, charge3.JR_OSCostAmt);
				AssertEquals(12.40m, charge3.JR_OSCostGSTAmt_Calc);
				AssertEquals(103.9m, charge4.JR_OSCostAmt);
				AssertEquals(10.39m, charge4.JR_OSCostGSTAmt_Calc);
				AssertEquals(144.12m, charge5.JR_OSCostAmt);
				AssertEquals(14.41m, charge5.JR_OSCostGSTAmt_Calc);
				AssertEquals(174.28m, charge6.JR_OSCostAmt);
				AssertEquals(17.43m, charge6.JR_OSCostGSTAmt_Calc);
				AssertEquals(1129.47m, charge7.JR_OSCostAmt);
				AssertEquals(112.95m, charge7.JR_OSCostGSTAmt_Calc);
				AssertEquals(3.35m, charge8.JR_OSCostAmt);
				AssertEquals(.33m, charge8.JR_OSCostGSTAmt_Calc);

				cost.E6_OSCostAmount = -1840m;
				cost.E6_OSGSTAmount_Calc = -184m;
				AssertEquals(-144.11m, charge1.JR_OSCostAmt);
				AssertEquals(-14.41m, charge1.JR_OSCostGSTAmt_Calc);
				AssertEquals(-16.76m, charge2.JR_OSCostAmt);
				AssertEquals(-1.68m, charge2.JR_OSCostGSTAmt_Calc);
				AssertEquals(-124.01m, charge3.JR_OSCostAmt);
				AssertEquals(-12.40m, charge3.JR_OSCostGSTAmt_Calc);
				AssertEquals(-103.9m, charge4.JR_OSCostAmt);
				AssertEquals(-10.39m, charge4.JR_OSCostGSTAmt_Calc);
				AssertEquals(-144.12m, charge5.JR_OSCostAmt);
				AssertEquals(-14.41m, charge5.JR_OSCostGSTAmt_Calc);
				AssertEquals(-174.28m, charge6.JR_OSCostAmt);
				AssertEquals(-17.43m, charge6.JR_OSCostGSTAmt_Calc);
				AssertEquals(-1129.47m, charge7.JR_OSCostAmt);
				AssertEquals(-112.95m, charge7.JR_OSCostGSTAmt_Calc);
				AssertEquals(-3.35m, charge8.JR_OSCostAmt);
				AssertEquals(-.33m, charge8.JR_OSCostGSTAmt_Calc);

				cost.E6_RX_NKCurrency = "USD";
				cost.E6_ExchangeRate = 10m;
				cost.E6_ApportionmentMethod = AllocationMethod.ChargeableUnits;
				AssertEquals(-14.41m, charge1.JR_LocalCostAmt);
				AssertEquals(-1.68m, charge2.JR_LocalCostAmt);
				AssertEquals(-12.40m, charge3.JR_LocalCostAmt);
				AssertEquals(-10.39m, charge4.JR_LocalCostAmt);
				AssertEquals(-14.41m, charge5.JR_LocalCostAmt);
				AssertEquals(-17.43m, charge6.JR_LocalCostAmt);
				AssertEquals(-112.94m, charge7.JR_LocalCostAmt);
				AssertEquals(-.34m, charge8.JR_LocalCostAmt);

				cost.E6_OSCostAmount = 1840m;
				cost.E6_OSGSTAmount_Calc = 184m;
				AssertEquals(14.41m, charge1.JR_LocalCostAmt);
				AssertEquals(1.68m, charge2.JR_LocalCostAmt);
				AssertEquals(12.40m, charge3.JR_LocalCostAmt);
				AssertEquals(10.39m, charge4.JR_LocalCostAmt);
				AssertEquals(14.41m, charge5.JR_LocalCostAmt);
				AssertEquals(17.43m, charge6.JR_LocalCostAmt);
				AssertEquals(112.94m, charge7.JR_LocalCostAmt);
				AssertEquals(.34m, charge8.JR_LocalCostAmt);

				foreach (ApportionSplitCharge charge in cost.ApportionmentCharges)
				{
					ZDecimal localCost = charge.JR_LocalCostAmt;
					charge.JR_OSCostExRate = 0m;
					charge.JR_LocalCostAmt = localCost;
					AssertEquals("OSCostEx should be 0 to prevent recalculation of LocalCost when is set to 0", 0m, charge.JR_OSCostExRate);
					AssertEquals("LocalCost should be preserved", localCost, charge.JR_LocalCostAmt);
				}

				cost.E6_LocalCostAmount = 0m;
				AssertEquals(0m, charge1.JR_LocalCostAmt);
				AssertEquals(0m, charge2.JR_LocalCostAmt);
				AssertEquals(0m, charge3.JR_LocalCostAmt);
				AssertEquals(0m, charge4.JR_LocalCostAmt);
				AssertEquals(0m, charge5.JR_LocalCostAmt);
				AssertEquals(0m, charge6.JR_LocalCostAmt);
				AssertEquals(0m, charge7.JR_LocalCostAmt);
				AssertEquals(0m, charge8.JR_LocalCostAmt);
			}
			finally
			{
				cost.CalculationStrategy.ReleaseMutexes();
			}
		}

		[ExpectNoExceptions]
		public void TestShouldNotRaiseCriticalValidationErrorDueToSetJR_OSCostAmtSameValueMultipleTimes()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.India))
			{
				var originalGC_IsReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;
				GlbCompany.CurrentCompany.Factory.Save();

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_UniqueConsignRef = "C00001001";
				var shipment1 = consol.Shipments.AddNew();
				var shipment2 = consol.Shipments.AddNew();
				var shipment3 = consol.Shipments.AddNew();
				var shipment4 = consol.Shipments.AddNew();
				var shipment5 = consol.Shipments.AddNew();
				var shipment6 = consol.Shipments.AddNew();
				var job1 = ObjectCreator.CreateJob(shipment1);
				var job2 = ObjectCreator.CreateJob(shipment2);
				var job3 = ObjectCreator.CreateJob(shipment3);
				var job4 = ObjectCreator.CreateJob(shipment4);
				var job5 = ObjectCreator.CreateJob(shipment5);
				var job6 = ObjectCreator.CreateJob(shipment6);
				Factory.Save();

				var listing = new ApportionmentListing(Factory, consol);
				var cost = listing.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
				cost.E6_GC = GlbCompany.CurrentCompany.PK;
				cost.E6_RX_NKCurrency = ObjectCreator.USD.RX_Code;
				cost.E6_ExchangeRate = 74.75M;
				cost.E6_OH_Creditor = ObjectCreator.ABIGAS.PK;
				cost.E6_AT_TaxRate = ObjectCreator.STAGST.PK;
				cost.E6_OSCostAmount = 5M;
				cost.E6_ApportionToRelatedShipments = true;
				cost.E6_ApportionmentMethod = AllocationMethod.Manual;
				cost.E6_IsTaxAmountOverridden = true;

				var charge1 = FindChargeForJob(cost, shipment1);
				var charge2 = FindChargeForJob(cost, shipment2);
				var charge3 = FindChargeForJob(cost, shipment3);
				var charge4 = FindChargeForJob(cost, shipment4);
				var charge5 = FindChargeForJob(cost, shipment5);
				var charge6 = FindChargeForJob(cost, shipment6);

				charge1.JR_OSCostAmt = 0.20M;
				charge2.JR_OSCostAmt = 1.39M;
				charge3.JR_OSCostAmt = 0.67M;
				charge4.JR_OSCostAmt = 1.29M;
				charge5.JR_OSCostAmt = 0.54M;
				charge6.JR_OSCostAmt = 0.91M;

				charge2.JR_OSCostAmt = 1.39M;
				charge4.JR_OSCostAmt = 1.29M;

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var invoice = newFactory.New<APInvoice>();
				invoice.AH_OH = ObjectCreator.ABIGAS.PK;
				invoice.AH_TransactionNum = "TEST001";
				invoice.SubmittedFromInvoicingForm = true;
				var importer = new InvoicingBaseBulkConsolCostImporter(invoice.ConsolCosting);
				importer.LoadConsolsCollection();

				importer.Import();
				invoice.ImportAllApportionmentsFromCosting();
				newFactory.Save();

				GlbCompany.CurrentCompany.GC_IsReciprocal = originalGC_IsReciprocal;
			}
		}

		public void TestPushUnApportionedAmountToGreatestChargeForCurrencyWithoutDecimalPlaces()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var consol = Factory.New<ForwardingConsol>();

			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			var shipment3 = consol.Shipments.AddNew();
			var shipment4 = consol.Shipments.AddNew();

			testObjectCreator.CreateJob(shipment1, false);
			testObjectCreator.CreateJob(shipment2, false);
			testObjectCreator.CreateJob(shipment3, false);
			testObjectCreator.CreateJob(shipment4, false);
			Factory.Save();

			var cost = GetCost(consol);
			try
			{
				cost.E6_IsTaxAmountOverridden = true;
				cost.E6_AC_ChargeCode = testObjectCreator.CC1.PK;
				cost.E6_RX_NKCurrency = "TWD";
				cost.E6_ExchangeRate = 1m;
				cost.E6_OSCostAmount = 40m;
				cost.E6_OSGSTAmount_Calc = 2m;
				cost.E6_ApportionmentMethod = AllocationMethod.Shipment;
				var sumOfChargeCostAmount = cost.ApportionmentCharges.Sum(x => ((ApportionSplitCharge)x).JR_OSCostAmt);
				var sumOfChargeCostGSTAmount = cost.ApportionmentCharges.Sum(x => ((ApportionSplitCharge)x).JR_OSCostGSTAmt_Calc);
				AssertEquals("All of the charge amount should be set to 10", 4, cost.ApportionmentCharges.Count(x => ((ApportionSplitCharge)x).JR_OSCostAmt == 10m));
				AssertEquals("2 of the charge GST should be set to zero", 2, cost.ApportionmentCharges.Count(x => ((ApportionSplitCharge)x).JR_OSCostGSTAmt_Calc == 0));
				AssertEquals("2 of the charge GST should be set to 1", 2, cost.ApportionmentCharges.Count(x => ((ApportionSplitCharge)x).JR_OSCostGSTAmt_Calc == 1m));
				AssertEquals("Sum of apportioned charge amount is equal to cost amount", cost.E6_OSCostAmount, sumOfChargeCostAmount);
				AssertEquals("Sum of apportioned charge GST amount is equal to cost GST amount", cost.E6_OSGSTAmount_Calc, sumOfChargeCostGSTAmount);
				AssertNoExceptionThrown("Should not throw any exception", () =>
				{
					Factory.Save();
				});
			}
			finally
			{
				cost.CalculationStrategy.ReleaseMutexes();
			}
		}

		public void TestPushUnApportionedAmountToGreatestChargeWhenIsUsedForApportionmentUntickedOnSomeCharges()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "SGSIN";
			Factory.Save();
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			var shipment3 = consol.Shipments.AddNew();
			var shipment4 = consol.Shipments.AddNew();
			var cost = GetCost(consol);
			try
			{
				cost.E6_IsTaxAmountOverridden = true;
				cost.E6_AC_ChargeCode = creator.CC1.PK;
				cost.E6_RX_NKCurrency = "JPY";
				cost.E6_ExchangeRate = 1m;
				cost.E6_OSCostAmount = 120m;
				cost.E6_OSGSTAmount_Calc = 2m;
				cost.E6_ApportionmentMethod = AllocationMethod.Shipment;
				cost.ApportionmentCharges[cost.ApportionmentCharges.Count - 1].JR_IsUsedForApportionment = false;
				var sumOfChargeCostAmount = cost.ApportionmentCharges.Sum(x => ((ApportionSplitCharge)x).JR_OSCostAmt);
				var sumOfChargeCostGSTAmount = cost.ApportionmentCharges.Sum(x => ((ApportionSplitCharge)x).JR_OSCostGSTAmt_Calc);
				AssertEquals("3 of the charge amount should be set to 40", 3, cost.ApportionmentCharges.Count(x => ((ApportionSplitCharge)x).JR_OSCostAmt == 40m));
				AssertEquals("2 of the charge GST should be set to zero", 2, cost.ApportionmentCharges.Count(x => ((ApportionSplitCharge)x).JR_OSCostGSTAmt_Calc == 0));
				AssertEquals("2 of the charge GST should be set to 1", 2, cost.ApportionmentCharges.Count(x => ((ApportionSplitCharge)x).JR_OSCostGSTAmt_Calc == 1m));
				AssertEquals("Sum of apportioned charge amount is equal to cost amount", cost.E6_OSCostAmount, sumOfChargeCostAmount);
				AssertEquals("Sum of apportioned charge GST amount is equal to cost GST amount", cost.E6_OSGSTAmount_Calc, sumOfChargeCostGSTAmount);
			}
			finally
			{
				cost.CalculationStrategy.ReleaseMutexes();
			}
		}

		[ExpectNoExceptions]
		public void TestPushUnApportionedAmountToGreatestChargeDoesNoCauseIntOverflow()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			JobConsolCost cost = GetCost(consol);
			try
			{
				ApportionSplitCharge charge1 = FindChargeForJob(cost, shipment1);
				ApportionSplitCharge charge2 = FindChargeForJob(cost, shipment2);
				cost.E6_RX_NKCurrency = "USD";
				cost.E6_ExchangeRate = 0.99m;
				cost.E6_ApportionmentMethod = AllocationMethod.Shipment;
				cost.E6_OSCostAmount = new ZDecimal(int.MaxValue);
				// There should not be any exception caused by E6_LocalCostAmount being greater than int.MaxValue
				Assert(cost.E6_LocalCostAmount > (new ZDecimal(int.MaxValue)));
			}
			finally
			{
				cost.CalculationStrategy.ReleaseMutexes();
			}
		}

		public void TestReadOnlyPropertiesForCreditorThatHasSelfBillingInvoicesFlagSet()
		{
			JobConsolCost cost = Factory.New<JobConsolCost>();
			AssertEquals("Invoice Date readonly", false, cost.E6_InvoiceDateInfo.ReadOnly);
			AssertEquals("Invoice Num readonly", false, cost.E6_InvoiceNumInfo.ReadOnly);
			AssertEquals("Payment Date readonly", false, cost.E6_PaymentDateInfo.ReadOnly);
			cost.E6_InvoiceNum = "123456";
			cost.E6_InvoiceDate = cost.E6_PaymentDate = ZDateTime.Today;
			ObjectCreator.ABIGAS.CompanyData.OB_APCostsSelfBilled = true;
			cost.E6_OH_Creditor = ObjectCreator.ABIGAS.PK;
			AssertEquals("Invoice Date", ZDateTime.Empty, cost.E6_InvoiceDate);
			AssertEquals("Invoice Num", ZString.Empty, cost.E6_InvoiceNum);
			AssertEquals("Payment Date", ZDateTime.Empty, cost.E6_PaymentDate);
			AssertEquals("Invoice Date readonly", true, cost.E6_InvoiceDateInfo.ReadOnly);
			AssertEquals("Invoice Num readonly", true, cost.E6_InvoiceNumInfo.ReadOnly);
			AssertEquals("Payment Date readonly", true, cost.E6_PaymentDateInfo.ReadOnly);
		}

		public void TestIsSavedByFactoryForIncomplete()
		{
			JobConsolCost cost = Factory.New<JobConsolCost>();
			AssertEquals("No parent", true, cost.IsSavedByFactory);
			APInvoice invoice = Factory.New<APInvoice>();
			cost.ParentAPInvoice = invoice;
			AssertEquals("Parent is a regular AP Invoice", true, cost.IsSavedByFactory);
			invoice.AH_Ledger = LedgerTypes.IncompleteTransactions;
			AssertEquals("Parent is an IN Invoice", false, cost.IsSavedByFactory);
			invoice.Delete();
			AssertEquals("Parent is deleted", true, cost.IsSavedByFactory);
		}

		public void TestShipmentsToApportion()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "Master1";
			shipment1.JS_ActualWeight = 150m;
			shipment1.JS_ActualChargeable = 100m;
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "Master2";
			shipment2.JS_ActualWeight = 425m;
			shipment2.JS_ActualChargeable = 375m;
			ForwardingShipment shipment21 = consol.Shipments.AddNew();
			shipment21.JS_JS_ColoadMasterShipment = shipment2.PK;
			shipment21.JS_ActualWeight = 200m;
			shipment21.JS_ActualChargeable = 150m;
			ForwardingShipment shipment22 = consol.Shipments.AddNew();
			shipment22.JS_JS_ColoadMasterShipment = shipment2.PK;
			shipment22.JS_ActualWeight = 100m;
			shipment22.JS_ActualChargeable = 100m;
			ForwardingShipment shipment23 = consol.Shipments.AddNew();
			shipment23.JS_JS_ColoadMasterShipment = shipment2.PK;
			Factory.Save();
			shipment23.JS_IsCancelled = true;
			shipment23.JS_ActualWeight = 125m;
			shipment23.JS_ActualChargeable = 125m;
			ForwardingShipment shipment3 = consol.Shipments.AddNew();
			shipment3.JS_UniqueConsignRef = "Master3";
			Factory.Save();
			shipment3.JS_IsCancelled = true;
			shipment3.JS_ActualWeight = 400m;
			shipment3.JS_ActualChargeable = 375m;
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();
				AssertEquals("Pre-condition: E6_ApportionToRelatedShipments", false, cost.E6_ApportionToRelatedShipments);
				AssertEquals("Should be 2 shipments", 2, cost.ShipmentsToApportion.Length);
				AssertEquals("Should be 2 charges in list", 2, cost.ApportionmentCharges.Count);
				cost.E6_ApportionToRelatedShipments = true;
				AssertEquals("Should be 4 shipments", 4, cost.ShipmentsToApportion.Length);
				AssertEquals("Should be 4 charges in list", 4, cost.ApportionmentCharges.Count);
				foreach (ApportionSplitCharge charge in cost.ApportionmentCharges)
				{
					if (!charge.JR_IsUsedForApportionment)
					{
						charge.JR_IsUsedForApportionment = true;
						break;
					}
				}

				cost.E6_ApportionToRelatedShipments = false;
				AssertEquals("Should be 2 shipments", 2, cost.ShipmentsToApportion.Length);
				AssertEquals("Should be 3 charges in list", 3, cost.ApportionmentCharges.Count);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestE6_ApportionToRelatedShipmentsDefaultValue()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			AssertEquals("Pre-condition: ConsolCostDefaultRelatedShipmentsApportionment Registry item default value", false, AccountingConfigurationRegistry.Instance.ConsolCostDefaultRelatedShipmentsApportionment.Value);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			AssertEquals("E6_ApportionToRelatedShipments Default Value", false, cost.E6_ApportionToRelatedShipments);
			AccountingConfigurationRegistry.Instance.ConsolCostDefaultRelatedShipmentsApportionment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			JobConsolCost secondCost = apps.CostsCollection.TryAddNew();
			AssertEquals("E6_ApportionToRelatedShipments Devault Value", true, secondCost.E6_ApportionToRelatedShipments);
			AccountingConfigurationRegistry.Instance.ConsolCostDefaultRelatedShipmentsApportionment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			cost.E6_ApportionToRelatedShipments = true;
			secondCost.E6_ApportionToRelatedShipments = false;
			AssertEquals("E6_ApportionToRelatedShipments Assigned Value", true, cost.E6_ApportionToRelatedShipments);
			AssertEquals("E6_ApportionToRelatedShipments Assigned Value", false, secondCost.E6_ApportionToRelatedShipments);
		}

		public void TestE6_ApportionToRelatedShipments()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "Master1";
			shipment1.JS_ActualWeight = 150m;
			shipment1.JS_ActualChargeable = 100m;
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "Master2";
			shipment2.JS_ActualWeight = 300m;
			shipment2.JS_ActualChargeable = 250m;
			ForwardingShipment shipment21 = consol.Shipments.AddNew();
			shipment21.JS_JS_ColoadMasterShipment = shipment2.PK;
			shipment21.JS_ActualWeight = 200m;
			shipment21.JS_ActualChargeable = 150m;
			ForwardingShipment shipment22 = consol.Shipments.AddNew();
			shipment22.JS_JS_ColoadMasterShipment = shipment2.PK;
			shipment22.JS_ActualWeight = 100m;
			shipment22.JS_ActualChargeable = 100m;
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			try
			{
				TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
				JobConsolCost cost = apps.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = testObjectCreator.CC1.PK;
				cost.E6_OSCostAmount = 400m;
				AssertEquals("Pre-condition: E6_ApportionToRelatedShipments", false, cost.E6_ApportionToRelatedShipments);
				AssertEquals("Should be 2 charges in list", 2, cost.ApportionmentCharges.Count);
				cost.E6_ApportionToRelatedShipments = true;
				AssertEquals("Should be 4 charges in list", 4, cost.ApportionmentCharges.Count);
				foreach (ApportionSplitCharge charge in cost.ApportionmentCharges)
				{
					charge.JR_IsUsedForApportionment = true;
				}

				Factory.Save();
				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				JobConsolCost reloadedCost = newFactory.Load<JobConsolCost>(cost.PK);
				AssertEquals("E6_ApportionToRelatedShipments", false, reloadedCost.E6_ApportionToRelatedShipments);
				AssertEquals("ApportionmentCharges.Count", 4, reloadedCost.ApportionmentCharges.Count);
				AssertEquals("E6_ApportionToRelatedShipments after loading ApportionmentCharges should be as pecified in Registry", AccountingConfigurationRegistry.Instance.ConsolCostDefaultRelatedShipmentsApportionment.Value, reloadedCost.E6_ApportionToRelatedShipments);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestE6_RX_NKSellCurrencyConcurrency()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			JobConsolCost cost = (JobConsolCost)GetNewBusinessObject();
			cost.FillWithValidTestData();
			RefCurrency currency1 = creator.USD;
			RefCurrency currency2 = creator.GBP;
			Factory.Save();
			AssertConcurrency(cost, JobConsolCostSchema.E6_RX_NKCurrency, currency1.RX_Code, currency2.RX_Code);
		}

		public void TestE6_AH_APInvoiceConcurrency()
		{
			JobConsolCost cost = (JobConsolCost)GetNewBusinessObject();
			cost.FillWithValidTestData();
			APInvoice invoice1 = Factory.NewWithValidTestData<APInvoice>();
			APInvoice invoice2 = Factory.NewWithValidTestData<APInvoice>();
			Factory.Save();
			AssertConcurrency(cost, JobConsolCostSchema.E6_AH_APInvoice, invoice1.PK, invoice2.PK);
		}

		public void TestE6_AH_ARInvoiceConcurrency()
		{
			JobConsolCost cost = (JobConsolCost)GetNewBusinessObject();
			cost.FillWithValidTestData();
			ARInvoice invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			ARInvoice invoice2 = Factory.NewWithValidTestData<ARInvoice>();
			Factory.Save();
			AssertConcurrency(cost, JobConsolCostSchema.E6_AH_ARInvoice, invoice1.PK, invoice2.PK);
		}

		public void TestE6_SupplyType()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.Shipments.AddNew();
				Factory.Save();
				var apps = new ApportionmentListing(Factory, consol);
				try
				{
					var cost = apps.CostsCollection.TryAddNew();
					AssertEquals(string.Empty, cost.E6_SupplyType);
					AssertEquals(1, cost.ApportionmentCharges.Count);

					cost.E6_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.DSB;
					AssertEquals("SupplyType on Consol Cost should have been copied to Apportionment Split Charge", cost.E6_SupplyType, cost.ApportionmentCharges[0].JR_CostSupplyType);
				}
				finally
				{
					apps.ReleaseMutexes();
				}
			}
		}

		public void TestGSTRateIsDefaultedWhenE6SupplyTypeIsSet()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var accountingTestObjectCreator = new MasterFiles.Business.Testing.Accounting.Helpers.AccountingTestObjectCreator(Factory);
				var chargeCode = ObjectCreator.CC1;
				var consol = ObjectCreator.CreateConsol("INDEL", "AUSYD", "C0001", false);
				consol.Shipments.AddNew();
				var cost = ObjectCreator.CreateConsolCost(consol, chargeCode, ObjectCreator.Creditor1);
				Factory.Save();

				var taxOverride1 = chargeCode.TaxOverrides.AddNew();
				accountingTestObjectCreator.PopulateTaxOverride(taxOverride1, ObjectCreator.GST1.PK);
				taxOverride1.AO_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC;

				var taxOverride2 = chargeCode.TaxOverrides.AddNew();
				accountingTestObjectCreator.PopulateTaxOverride(taxOverride2, ObjectCreator.GST2.PK);
				taxOverride2.AO_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.DSB;

				Factory.Save();

				cost.E6_SupplyType = "";
				cost.ChargeCode.ClearGSTRateCacheForTesting();
				cost.E6_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC;
				AssertEquals(ObjectCreator.GST1.PK, cost.E6_AT_TaxRate);
				AssertEquals(ObjectCreator.GST1.AT_A9_DefaultVatClass, cost.E6_A9_VATClass);

				cost.ChargeCode.ClearGSTRateCacheForTesting();
				cost.E6_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.DSB;
				AssertEquals(ObjectCreator.GST2.PK, cost.E6_AT_TaxRate);
				AssertEquals(ObjectCreator.GST2.AT_A9_DefaultVatClass, cost.E6_A9_VATClass);
			}
		}

		public void TestE6_PlaceOfSupply()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.Shipments.AddNew();
				Factory.Save();
				var apps = new ApportionmentListing(Factory, consol);
				try
				{
					var cost = apps.CostsCollection.TryAddNew();
					AssertEquals(string.Empty, cost.E6_PlaceOfSupply);
					AssertEquals(string.Empty, cost.E6_PlaceOfSupplyType);
					AssertEquals(1, cost.ApportionmentCharges.Count);

					cost.E6_PlaceOfSupply = "DL";
					AssertEquals(PlaceOfSupplyTypes.State.Code, cost.E6_PlaceOfSupplyType);
					AssertEquals("DL", cost.E6_PlaceOfSupply);
					AssertEquals("PlaceOfSupply on Consol Cost should have been copied to Apportionment Split Charge", cost.E6_PlaceOfSupply, cost.ApportionmentCharges[0].JR_CostPlaceOfSupply);
					AssertNotNull("Location should not be null", cost.PlaceOfSupplyLocation);
					AssertEquals("Code", "DL", cost.PlaceOfSupplyLocation.Code);
					AssertEquals("IsRule", false, cost.PlaceOfSupplyLocation.IsLocationRule());

					cost.E6_PlaceOfSupply = PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry;
					AssertEquals(PlaceOfSupplyTypes.PredefinedRule.Code, cost.E6_PlaceOfSupplyType);
					AssertEquals(PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry, cost.E6_PlaceOfSupply);
					AssertEquals(cost.E6_PlaceOfSupply, cost.ApportionmentCharges[0].JR_CostPlaceOfSupply);
					AssertNotNull("Location should not be null", cost.PlaceOfSupplyLocation);
					AssertEquals("Code", "ALX", cost.PlaceOfSupplyLocation.Code);
					AssertEquals("IsRule", true, cost.PlaceOfSupplyLocation.IsLocationRule());

					cost.E6_PlaceOfSupply = "";
					AssertEquals(string.Empty, cost.E6_PlaceOfSupply);
					AssertEquals(string.Empty, cost.E6_PlaceOfSupplyType);
					AssertEquals(string.Empty, cost.ApportionmentCharges[0].JR_CostPlaceOfSupply);
					AssertNull("Location should be null", cost.PlaceOfSupplyLocation);
				}
				finally
				{
					apps.ReleaseMutexes();
				}
			}
		}

		public void TestGSTRateIsDefaultedWhenE6PlaceOfSupplyIsSet()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var newBranch = ObjectCreator.CreateNewBranch(GlbCompany.CurrentCompany, "BR1");
				newBranch.GB_OH_OrgProxy = ObjectCreator.ActiveOrg.PK;
				GlbCompany.CurrentCompany.Factory.Save();
				using (EnvProxy.Instance.SetTemporaryUserContext(Env.CurrentUser.LoginName, newBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					ObjectCreator.ActiveOrg.MainAddress.OA_RL_NKRelatedPortCode = "INDEL";
					ObjectCreator.ActiveOrg.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.India;
					ObjectCreator.ActiveOrg.MainAddress.OA_State = "DL";
					Factory.Save();

					var chargeCode = ObjectCreator.FRT;
					var consol = ObjectCreator.CreateConsol("INDEL", "AUSYD", "C0001", false);
					consol.Shipments.AddNew();
					var cost = ObjectCreator.CreateConsolCost(consol, chargeCode, ObjectCreator.Creditor1);
					Factory.Save();

					var taxOverride1 = chargeCode.TaxOverrides.AddNew();
					taxOverride1.AO_CostSellAll = "ALL";
					taxOverride1.AO_Direction = "ALL";
					taxOverride1.AO_IncoTerm = "ALL";
					taxOverride1.AO_JobType = "ALL";
					taxOverride1.AO_Origin = "ALL";
					taxOverride1.AO_Destination = "ALL";
					taxOverride1.AO_TaxRegCntryOrGroup = "ALL";
					taxOverride1.AO_HomeCountryOrZone = "BST";
					taxOverride1.AO_AT = ObjectCreator.GST11.PK;

					var taxOverride2 = chargeCode.TaxOverrides.AddNew();
					taxOverride2.AO_CostSellAll = "ALL";
					taxOverride2.AO_Direction = "ALL";
					taxOverride2.AO_IncoTerm = "ALL";
					taxOverride2.AO_JobType = "ALL";
					taxOverride2.AO_Origin = "ALL";
					taxOverride2.AO_Destination = "ALL";
					taxOverride2.AO_TaxRegCntryOrGroup = "ALL";
					taxOverride2.AO_HomeCountryOrZone = "BSX";
					taxOverride2.AO_AT = ObjectCreator.GST2.PK;

					var taxOverride3 = chargeCode.TaxOverrides.AddNew();
					taxOverride3.AO_CostSellAll = "ALL";
					taxOverride3.AO_Direction = "ALL";
					taxOverride3.AO_IncoTerm = "ALL";
					taxOverride3.AO_JobType = "ALL";
					taxOverride3.AO_Origin = "ALL";
					taxOverride3.AO_Destination = "ALL";
					taxOverride3.AO_TaxRegCntryOrGroup = "ALL";
					taxOverride3.AO_HomeCountryOrZone = "ALX";
					taxOverride3.AO_AT = ObjectCreator.GST1.PK;
					Factory.Save();

					cost.E6_PlaceOfSupply = "";
					cost.ChargeCode.ClearGSTRateCacheForTesting();
					cost.E6_PlaceOfSupply = "DL";
					AssertEquals(ObjectCreator.GST11.PK, cost.E6_AT_TaxRate);

					cost.ChargeCode.ClearGSTRateCacheForTesting();
					cost.E6_PlaceOfSupply = "KL";
					AssertEquals(ObjectCreator.GST2.PK, cost.E6_AT_TaxRate);

					cost.ChargeCode.ClearGSTRateCacheForTesting();
					cost.E6_PlaceOfSupply = "ALX";
					AssertEquals(ObjectCreator.GST1.PK, cost.E6_AT_TaxRate);
				}
			}
		}

		public void TestClearPlaceOfSupplyWhenNoMatchingResult()
		{
			var branch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			AssertEquals(AccountingMasterFilesRegistry.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.DefaultCode, AccountingMasterFilesRegistry.Instance.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRules.Value);
			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code);
			AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(null, CostSell.Cost, PlaceOfSupplyTypes.State.Code, "NSW", branch: branch);
			AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(null, CostSell.Cost, PlaceOfSupplyTypes.State.Code, "NSW", branch: GlbBranch.CurrentBranch);

			var consol = ObjectCreator.CreateConsol("INDEL", "AUSYD", "C0001", false);
			consol.Shipments.AddNew();
			var cost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, ObjectCreator.Creditor1);

			Factory.Save();

			cost.E6_AC_ChargeCode = ZGuid.Empty;
			cost.E6_OH_Creditor = ZGuid.Empty;
			cost.E6_PlaceOfSupply = ZString.Empty;
			AssertEquals("PreCondition", ZString.Empty, cost.E6_PlaceOfSupply);

			cost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			cost.E6_OH_Creditor = ObjectCreator.Creditor1.PK;
			AssertPosMatchingWithChargeCodeAndOrgHeader();

			cost.E6_AC_ChargeCode = ZGuid.Empty;
			AssertPosMatchingWithoutChargeCode();

			cost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			cost.E6_OH_Creditor = ZGuid.Empty;
			cost.E6_PlaceOfSupply = "NSW";
			AssertPosMatchingWithoutOrgHeader();

			cost.E6_OH_Creditor = ObjectCreator.Creditor1.PK;
			AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(null, CostSell.Cost, PlaceOfSupplyTypes.State.Code, "StateNotExisted", branch: branch);
			AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(null, CostSell.Cost, PlaceOfSupplyTypes.State.Code, "StateNotExisted", branch: GlbBranch.CurrentBranch);
			cost.E6_PlaceOfSupply = "NSW";
			AssertPostmatchingWithInvalidState();

			void AssertPosMatchingWithChargeCodeAndOrgHeader()
			{
				AssertNotEquals("PreCondition", ZGuid.Empty, cost.E6_AC_ChargeCode);
				AssertNotEquals("PreCondition", ZGuid.Empty, cost.E6_OH_Creditor);
				AssertEquals("PreCondition", "NSW", cost.E6_PlaceOfSupply);

				cost.E6_OH_Creditor = ZGuid.Empty;
				AssertEquals("Should be empty when matching result empty", ZString.Empty, cost.E6_PlaceOfSupply);
				cost.E6_OH_Creditor = ObjectCreator.Creditor1.PK;
				AssertEquals("Should not be empty when matching result not empty", "NSW", cost.E6_PlaceOfSupply);
			}

			void AssertPostmatchingWithInvalidState()
			{
				AssertNull("PreCondition", branch.OrgProxy.MainAddress.RelatedState);
				AssertNull("PreCondition", GlbBranch.CurrentBranch.OrgProxy.MainAddress.RelatedState);
				AssertNotEquals("PreCondition", ZGuid.Empty, cost.E6_AC_ChargeCode);
				AssertEquals("PreCondition", "NSW", cost.E6_PlaceOfSupply);

				cost.E6_OH_Creditor = ZGuid.Empty;
				AssertEquals("Should be empty when matching result empty", ZString.Empty, cost.E6_PlaceOfSupply);
				cost.E6_OH_Creditor = ObjectCreator.Creditor1.PK;
				AssertEquals("Should be empty when matching result empty, that branch's state is not existed", ZString.Empty, cost.E6_PlaceOfSupply);
			}

			void AssertPosMatchingWithoutChargeCode()
			{
				AssertEquals("PreCondition", ZGuid.Empty, cost.E6_AC_ChargeCode);
				AssertEquals("PreCondition", "NSW", cost.E6_PlaceOfSupply);

				cost.E6_OH_Creditor = ZGuid.Empty;
				AssertEquals("Should be empty when matching result empty", ZString.Empty, cost.E6_PlaceOfSupply);
				cost.E6_OH_Creditor = ObjectCreator.Creditor1.PK;
				AssertEquals("Should be empty when matching result empty", ZString.Empty, cost.E6_PlaceOfSupply);
			}

			void AssertPosMatchingWithoutOrgHeader()
			{
				AssertNotEquals("PreCondition", ZGuid.Empty, cost.E6_AC_ChargeCode);
				AssertEquals("PreCondition", ZGuid.Empty, cost.E6_OH_Creditor);
				AssertEquals("PreCondition", "NSW", cost.E6_PlaceOfSupply);

				cost.E6_AC_ChargeCode = ZGuid.Empty;
				AssertEquals("Will not do matching when charge code is empty.", "NSW", cost.E6_PlaceOfSupply);
				cost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
				AssertEquals("Should be empty when matching result empty", ZString.Empty, cost.E6_PlaceOfSupply);
			}
		}

		public void TestPlaceOfSupplyDefaultedOnChangingChargeCode_State()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code))
			{
				var chargeCode = ObjectCreator.FRT;
				var consol = ObjectCreator.CreateConsol("INDEL", "AUSYD", "C0001", false);
				consol.Shipments.AddNew();

				var apps = new ApportionmentListing(Factory, consol);
				try
				{
					var currentBranch = GlbBranch.CurrentBranch;
					AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(consol, PlaceOfSupplyTypes.State.Code, "NSW", currentBranch);
					var cost = ObjectCreator.CreateConsolCost(consol, chargeCode, ObjectCreator.Creditor1, apps);
					Assert(!cost.E6_OH_Creditor.IsEmpty);
					AssertEquals("NSW", cost.E6_PlaceOfSupply);

					AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(consol, PlaceOfSupplyTypes.State.Code, "QLD", currentBranch);
					cost.E6_PlaceOfSupply = "ACT";
					cost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
					Assert(!cost.E6_OH_Creditor.IsEmpty);
					AssertEquals("QLD", cost.E6_PlaceOfSupply);
				}
				finally
				{
					apps.ReleaseMutexes();
				}
			}
		}

		public void TestPlaceOfSupplyDefaultedOnChangingChargeCode_TaxZone()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CA"))
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.TaxZone.Code))
			{
				var chargeCode = ObjectCreator.FRT;
				var consol = ObjectCreator.CreateConsol("AUSYD", "CATOR", "C0001", false);
				consol.Shipments.AddNew();

				var apps = new ApportionmentListing(Factory, consol);
				try
				{
					var currentBranch = GlbBranch.CurrentBranch;
					AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(consol, PlaceOfSupplyTypes.TaxZone.Code, "ONTZ", currentBranch);
					var cost = ObjectCreator.CreateConsolCost(consol, chargeCode, ObjectCreator.Creditor1, apps);
					Assert(!cost.E6_OH_Creditor.IsEmpty);
					AssertEquals("ONTZ", cost.E6_PlaceOfSupply);

					AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(consol, PlaceOfSupplyTypes.TaxZone.Code, "QUBC", currentBranch);
					cost.E6_PlaceOfSupply = "HSTC";
					cost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
					Assert(!cost.E6_OH_Creditor.IsEmpty);
					AssertEquals("QUBC", cost.E6_PlaceOfSupply);
				}
				finally
				{
					apps.ReleaseMutexes();
				}
			}
		}

		public void TestPlaceOfSupplyDefaultedOnChangingCreditor_State()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code))
			{
				var chargeCode = ObjectCreator.FRT;
				var consol = ObjectCreator.CreateConsol("INDEL", "AUSYD", "C0001", false);
				consol.Shipments.AddNew();

				var apps = new ApportionmentListing(Factory, consol);
				try
				{
					var currentBranch = GlbBranch.CurrentBranch;
					AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(consol, PlaceOfSupplyTypes.State.Code, "NSW", currentBranch);
					var cost = ObjectCreator.CreateConsolCost(consol, chargeCode, ObjectCreator.Creditor1, apps);
					Assert(!cost.E6_OH_Creditor.IsEmpty);
					AssertEquals("NSW", cost.E6_PlaceOfSupply);

					AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(consol, PlaceOfSupplyTypes.State.Code, "QLD", currentBranch);
					cost.E6_PlaceOfSupply = "ACT";
					cost.E6_OH_Creditor = ObjectCreator.Creditor2.PK;
					AssertEquals("QLD", cost.E6_PlaceOfSupply);
				}
				finally
				{
					apps.ReleaseMutexes();
				}
			}
		}

		public void TestPlaceOfSupplyDefaultedOnChangingCreditor_TaxZone()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CA"))
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.TaxZone.Code))
			{
				var chargeCode = ObjectCreator.FRT;
				var consol = ObjectCreator.CreateConsol("AUSYD", "CATOR", "C0001", false);
				consol.Shipments.AddNew();

				var apps = new ApportionmentListing(Factory, consol);
				try
				{
					var currentBranch = GlbBranch.CurrentBranch;
					AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(consol, PlaceOfSupplyTypes.TaxZone.Code, "ONTZ", currentBranch);
					var cost = ObjectCreator.CreateConsolCost(consol, chargeCode, ObjectCreator.Creditor1, apps);
					Assert(!cost.E6_OH_Creditor.IsEmpty);
					AssertEquals("ONTZ", cost.E6_PlaceOfSupply);

					AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(consol, PlaceOfSupplyTypes.TaxZone.Code, "QUBC", currentBranch);
					cost.E6_PlaceOfSupply = "HSTC";
					cost.E6_OH_Creditor = ObjectCreator.Creditor2.PK;
					AssertEquals("QUBC", cost.E6_PlaceOfSupply);
				}
				finally
				{
					apps.ReleaseMutexes();
				}
			}
		}

		public void TestGSTRateIsDefaultedWhenTaxBranchIsSet()
		{
			var branchForCost = TestObjectCreator.CreateBranch("CST", GlbCompany.CurrentCompany);
			TestObjectCreator.CC1.AC_AT_GSTRate = ZGuid.Empty;
			TestObjectCreator.CC1.TaxOverrides.RemoveAndDeleteAll();
			TestObjectCreator.CreateTaxOverrides(TestObjectCreator.CC1
				, taxOverride => {
					taxOverride.AO_AT = TestObjectCreator.SVAT1.PK;
					taxOverride.AO_GB = branchForCost.PK;
				});
			Factory.Save();

			var consol = TestObjectCreator.CreateConsol("INDEL", "AUSYD", "C0001", false);
			var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.Creditor1);
			cost.E6_AT_TaxRate = ZGuid.Empty;

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			cost.E6_GB_CostTaxBranch = ZGuid.Empty;
			CombineAssertions("When EnableTaxBranchReporting is not enabled,E6_GB_CostTaxBranch should not trigger E6_AT_TaxRate resetting.", () => {
				cost.E6_GB_CostTaxBranch = branchForCost.PK;
				AssertEquals(ZGuid.Empty, cost.E6_AT_TaxRate);
			});

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			cost.E6_GB_CostTaxBranch = ZGuid.Empty;
			CombineAssertions("When EnableTaxBranchReporting is enabled, E6_GB_CostTaxBranch should trigger E6_AT_TaxRate resetting.", () => {
				cost.E6_GB_CostTaxBranch = branchForCost.PK;
				AssertEquals(TestObjectCreator.SVAT1.PK, cost.E6_AT_TaxRate);
			});
		}

		public void TestSetGSTRateAndTaxMessage_Branch()
		{
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var branchForCost = TestObjectCreator.CreateBranch("CST", GlbCompany.CurrentCompany);
			TestObjectCreator.CC1.AC_AT_GSTRate = ZGuid.Empty;
			TestObjectCreator.CC1.TaxOverrides.RemoveAndDeleteAll();
			TestObjectCreator.CreateTaxOverrides(TestObjectCreator.CC1
				, taxOverride => {
					taxOverride.AO_AT = TestObjectCreator.FREEVAT.PK;
					taxOverride.AO_GB = GlbBranch.CurrentBranch.PK;
				}
				, taxOverride => {
					taxOverride.AO_AT = TestObjectCreator.SVAT1.PK;
					taxOverride.AO_SupplyType = SupplyTypeClassificationCodes.LOC;
				});
			Factory.Save();

			var consol = TestObjectCreator.CreateConsol("INDEL", "AUSYD", "C0001", false);
			var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.Creditor1);
			cost.E6_SupplyType = SupplyTypeClassificationCodes.LOC;

			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			TestObjectCreator.CC1.ClearGSTRateCacheForTesting();
			cost.SetGSTRateAndTaxMessage();
			AssertEquals("When EnableBranchLevelTaxOverrideRuleConfigurations is not enabled, E6_AT_TaxRate should not be filtered by branch."
				, TestObjectCreator.SVAT1.PK, cost.E6_AT_TaxRate);

			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			TestObjectCreator.CC1.ClearGSTRateCacheForTesting();
			cost.SetGSTRateAndTaxMessage();
			AssertEquals("When EnableBranchLevelTaxOverrideRuleConfigurations is enabled, E6_AT_TaxRate should be filtered by current branch."
				, TestObjectCreator.FREEVAT.PK, cost.E6_AT_TaxRate);
		}

		public void TestSetGSTRateAndTaxMessage_TaxBranch()
		{
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var branchForCost = TestObjectCreator.CreateBranch("CST", GlbCompany.CurrentCompany);
			TestObjectCreator.CC1.AC_AT_GSTRate = ZGuid.Empty;
			TestObjectCreator.CC1.TaxOverrides.RemoveAndDeleteAll();
			TestObjectCreator.CreateTaxOverrides(TestObjectCreator.CC1
				, taxOverride => {
					taxOverride.AO_AT = TestObjectCreator.FREEVAT.PK;
					taxOverride.AO_GB = GlbBranch.CurrentBranch.PK;
				}
				, taxOverride => {
					taxOverride.AO_AT = TestObjectCreator.SVAT2.PK;
					taxOverride.AO_GB = branchForCost.PK;
				}
				, taxOverride => {
					taxOverride.AO_AT = TestObjectCreator.VATSPV.PK;
					taxOverride.AO_SupplyType = SupplyTypeClassificationCodes.LOC;
				});
			Factory.Save();

			var consol = TestObjectCreator.CreateConsol("INDEL", "AUSYD", "C0001", false);
			var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.Creditor1);
			cost.E6_GB_CostTaxBranch = branchForCost.PK;
			cost.E6_SupplyType = SupplyTypeClassificationCodes.LOC;

			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			TestObjectCreator.CC1.ClearGSTRateCacheForTesting();
			cost.SetGSTRateAndTaxMessage();
			AssertEquals("When EnableBranchLevelTaxOverrideRuleConfigurations is not enabled, E6_AT_TaxRate should not be filtered by branch."
				, TestObjectCreator.VATSPV.PK, cost.E6_AT_TaxRate);

			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			TestObjectCreator.CC1.ClearGSTRateCacheForTesting();
			cost.SetGSTRateAndTaxMessage();
			AssertEquals("When EnableBranchLevelTaxOverrideRuleConfigurations is not enabled, E6_AT_TaxRate should not be filtered by branch."
				, TestObjectCreator.VATSPV.PK, cost.E6_AT_TaxRate);

			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			TestObjectCreator.CC1.ClearGSTRateCacheForTesting();
			cost.SetGSTRateAndTaxMessage();
			AssertEquals("When EnableBranchLevelTaxOverrideRuleConfigurations is enabled but EnableTaxBranchReporting is not enabled, E6_AT_TaxRate should be filtered by current branch, instead of E6_GB_CostTaxBranch."
				, TestObjectCreator.FREEVAT.PK, cost.E6_AT_TaxRate);

			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			TestObjectCreator.CC1.ClearGSTRateCacheForTesting();
			cost.SetGSTRateAndTaxMessage();
			AssertEquals("When EnableBranchLevelTaxOverrideRuleConfigurations and EnableTaxBranchReporting are enabled, E6_AT_TaxRate should be filtered by E6_GB_CostTaxBranch, instead of current branch."
				, TestObjectCreator.SVAT2.PK, cost.E6_AT_TaxRate);
		}

		public void TestCalculateDueDateWithDocumentReceivedDate()
		{
			var currentDate = ZDateTime.Now;
			var cost = Factory.New<JobConsolCost>();
			cost.E6_InvoiceNum = "123456";
			cost.E6_InvoiceDate = currentDate.Date.AddDays(-1);
			cost.E6_OH_Creditor = ObjectCreator.AALSHI.PK;

			AssertEquals(ZDateTime.Empty, cost.E6_PaymentDate);

			cost.E6_DocumentReceivedDate = currentDate.AddDays(1);
			AssertEquals("Due Date should be yestday based on Invoice Date", currentDate.Date.AddDays(-1), cost.E6_PaymentDate.Date);

			using (AccountingMasterFilesRegistry.Instance.APInvoiceDueDateCalculationRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				cost.E6_DocumentReceivedDate = currentDate.AddDays(1);
				AssertEquals("Due Date should be tomorrow based on Document Received Date", currentDate.Date.AddDays(1), cost.E6_PaymentDate.Date);
			}
		}

		public void TestDefaultE6_DocumentReceivedDate()
		{
			var cost = Factory.New<JobConsolCost>();
			cost.E6_InvoiceNum = "123456";

			AssertEquals(ZDateTime.Empty, cost.E6_DocumentReceivedDate);

			using (AccountingMasterFilesRegistry.Instance.DocumentReceivedDateDefaultingLogic.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.DocReceivedDateDefaultLogics.Code.CreateDate))
			{
				cost.E6_InvoiceDate = ZDateTime.Today.AddDays(-1);
				AssertEquals(ZDateTime.Now.Date, cost.E6_DocumentReceivedDate.Date);

				cost.E6_DocumentReceivedDate = ZDateTime.Today.AddDays(1);
				cost.E6_InvoiceDate = ZDateTime.Today.AddDays(-1);
				AssertEquals(ZDateTime.Today.AddDays(1), cost.E6_DocumentReceivedDate);
			}

			using (AccountingMasterFilesRegistry.Instance.DocumentReceivedDateDefaultingLogic.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.DocReceivedDateDefaultLogics.Code.InvoiceDate))
			{
				cost.E6_DocumentReceivedDate = ZDateTime.Empty;
				cost.E6_InvoiceDate = ZDateTime.Today.AddDays(-1);
				AssertEquals(cost.E6_InvoiceDate, cost.E6_DocumentReceivedDate);
			}
		}

		public void TestE6_PlaceOfSupplyType()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.Shipments.AddNew();
				Factory.Save();
				var apps = new ApportionmentListing(Factory, consol);
				try
				{
					var cost = apps.CostsCollection.TryAddNew();
					AssertEquals(string.Empty, cost.E6_PlaceOfSupply);
					AssertEquals(string.Empty, cost.E6_PlaceOfSupplyType);
					AssertEquals(1, cost.ApportionmentCharges.Count);

					cost.E6_PlaceOfSupply = "DL";
					AssertEquals(PlaceOfSupplyTypes.State.Code, cost.E6_PlaceOfSupplyType);
					AssertEquals("DL", cost.E6_PlaceOfSupply);
					AssertEquals(cost.E6_PlaceOfSupplyType, cost.ApportionmentCharges[0].JR_CostPlaceOfSupplyType);

					cost.E6_PlaceOfSupplyType = PlaceOfSupplyTypes.PredefinedRule.Code;
					AssertEquals(PlaceOfSupplyTypes.PredefinedRule.Code, cost.E6_PlaceOfSupplyType);
					AssertEquals("No changes to Place when we change Type", "DL", cost.E6_PlaceOfSupply);
					AssertEquals(PlaceOfSupplyTypes.PredefinedRule.Code, cost.ApportionmentCharges[0].JR_CostPlaceOfSupplyType);

					cost.E6_PlaceOfSupplyType = "";
					AssertEquals(string.Empty, cost.E6_PlaceOfSupplyType);
					AssertEquals("No changes to Place when we change Type", "DL", cost.E6_PlaceOfSupply);
					AssertEquals(string.Empty, cost.ApportionmentCharges[0].JR_CostPlaceOfSupplyType);
				}
				finally
				{
					apps.ReleaseMutexes();
				}
			}
		}

		public void TestSynchroniseUnpostedInvoiceDetailsIfNecessary_PlaceOfSupply()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var consol = ObjectCreator.CreateConsol();
				var shipment = ObjectCreator.CreateShipment("S001", consol);
				var job = ObjectCreator.CreateJob(shipment, false);
				var charge = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, "Desc 01", ObjectCreator.AUD, 100M, ObjectCreator.Creditor1, ObjectCreator.AUD, 100M, ObjectCreator.Debtor);
				charge.JR_CostPlaceOfSupply = "DL";

				var consolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, 100, ObjectCreator.Creditor1);
				consolCost.E6_PlaceOfSupply = "JH";
				AssertEquals(PlaceOfSupplyTypes.State.Code, consolCost.E6_PlaceOfSupplyType);
				consolCost.PrepareForPosting();

				AssertEquals("JH", charge.JR_CostPlaceOfSupply);
				AssertEquals(PlaceOfSupplyTypes.State.Code, charge.JR_CostPlaceOfSupplyType);
			}
		}

		public void TestSynchronisePostedInvoiceDetailsIfPossible_PlaceOfSupply()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var creator = new TestObjectCreator(Factory);
				var consol = creator.CreateConsol();
				var shipment1 = creator.CreateShipment("S001", consol);
				var shipment2 = creator.CreateShipment("S002", consol);
				creator.CreateJob(shipment1, false);
				creator.CreateJob(shipment2, false);
				Factory.Save();

				var apportionmentListing = consol.GetApportionments();
				var cost = CreateConsolCost(apportionmentListing);

				var invoice1 = creator.CreateInvoice(typeof(APInvoice), creator.AUD, 1.0m, creator.AALSHI) as APInvoice;
				invoice1.AH_TransactionNum = "INV0001";
				var apline1 = creator.CreateAPInvoiceLine(invoice1, cost.ApportionmentCharges[0].Job as Job, cost.ChargeCode, creator.AUD, 1m, "Test Line 1", 100m);

				var invoice2 = creator.CreateInvoice(typeof(APInvoice), creator.AUD, 1.0m, creator.AALSHI) as APInvoice;
				invoice2.AH_TransactionNum = "INV0002";
				invoice2.AH_PlaceOfSupply = "DL";
				invoice2.AH_PlaceOfSupplyType = PlaceOfSupplyTypes.State.Code;
				var apline2 = creator.CreateAPInvoiceLine(invoice2, cost.ApportionmentCharges[1].Job as Job, cost.ChargeCode, creator.AUD, 1m, "Test Line 2", 110m);

				cost = CreateConsolCost(apportionmentListing);
				cost.E6_AH_APInvoice = invoice1.PK;
				cost.ApportionmentCharges[0].JR_AL_APLine = invoice2.Lines[0].PK;
				cost.ApportionmentCharges[1].JR_AL_APLine = invoice2.Lines[0].PK;
				var message = string.Empty;
				Assert("SynchroniseInvoiceDetailsIfNecessary", cost.SynchronisePostedInvoiceDetailsIfPossible(out message));
				AssertEquals("DL", cost.E6_PlaceOfSupply);
				AssertEquals(PlaceOfSupplyTypes.State.Code, cost.E6_PlaceOfSupplyType);
			}
		}

		public void TestSynchroniseUnpostedInvoiceDetailsIfNecessary_TaxBranch()
		{
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();

			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var consol = ObjectCreator.CreateConsol();
				var shipment = ObjectCreator.CreateShipment("S001", consol);
				var job = ObjectCreator.CreateJob(shipment, false);

				var consolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, 100, ObjectCreator.Creditor1);
				consolCost.E6_GB_CostTaxBranch = branch1.PK;

				var appCharge1 = consolCost.ApportionmentCharges.AddNew();
				appCharge1.JR_JH = job.PK;
				appCharge1.JR_GB_CostTaxBranch = branch2.PK;
				consolCost.SynchroniseUnpostedInvoiceDetailsIfNecessary();

				AssertEquals(branch1.PK, appCharge1.JR_GB_CostTaxBranch);
			}
		}

		public void TestSynchronisePostedInvoiceDetailsIfPossible_TaxBranch()
		{
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var branch3 = Factory.NewWithValidTestData<GlbBranch>();
			var branch4 = Factory.NewWithValidTestData<GlbBranch>();

			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var consol = ObjectCreator.CreateConsol();
				var shipment1 = ObjectCreator.CreateShipment("S001", consol);
				var shipment2 = ObjectCreator.CreateShipment("S002", consol);
				ObjectCreator.CreateJob(shipment1, false);
				ObjectCreator.CreateJob(shipment2, false);
				Factory.Save();

				var apportionmentListing = consol.GetApportionments();
				var cost = CreateConsolCost(apportionmentListing);

				var invoice1 = ObjectCreator.CreateInvoice(typeof(APInvoice), ObjectCreator.AUD, 1.0m, ObjectCreator.AALSHI) as APInvoice;
				invoice1.AH_GB_TaxBranch = branch1.PK;
				var apline1 = ObjectCreator.CreateAPInvoiceLine(invoice1, cost.ApportionmentCharges[0].Job as Job, cost.ChargeCode, ObjectCreator.AUD, 1m, "Test Line 1", 100m);
				apline1.AL_GB_TaxBranch = branch2.PK;

				var invoice2 = ObjectCreator.CreateInvoice(typeof(APInvoice), ObjectCreator.AUD, 1.0m, ObjectCreator.AALSHI) as APInvoice;
				invoice2.AH_GB_TaxBranch = branch3.PK;
				var apline2 = ObjectCreator.CreateAPInvoiceLine(invoice2, cost.ApportionmentCharges[1].Job as Job, cost.ChargeCode, ObjectCreator.AUD, 1m, "Test Line 2", 110m);
				apline2.AL_GB_TaxBranch = branch4.PK;

				cost = CreateConsolCost(apportionmentListing);
				cost.E6_AH_APInvoice = invoice2.PK;
				cost.ApportionmentCharges[0].JR_AL_APLine = invoice2.Lines[0].PK;
				cost.ApportionmentCharges[1].JR_AL_APLine = invoice2.Lines[0].PK;

				AssertNotEquals(branch4.PK, cost.E6_GB_CostTaxBranch);
				Assert(cost.ApportionmentCharges.Cast<ApportionSplitCharge>().All(x => x.JR_GB_CostTaxBranch != branch4.PK));

				var message = string.Empty;
				Assert("SynchroniseInvoiceDetailsIfNecessary", cost.SynchronisePostedInvoiceDetailsIfPossible(out message));
				AssertEquals(branch4.PK, cost.E6_GB_CostTaxBranch);
				Assert(cost.ApportionmentCharges.Cast<ApportionSplitCharge>().All(x => x.JR_GB_CostTaxBranch == branch4.PK));
			}
		}

		public void TestE6_CostReference()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.Shipments.AddNew();
			Factory.Save();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();
				cost.E6_CostReference = "ABC";
				AssertEquals("Supplier Cost Reference on Consol Cost should have been copied to Apportionment Split Charge", cost.E6_CostReference, apps.CostsCollection[0].ApportionmentCharges[0].JR_CostReference);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestCopyValues_JR_CostReference()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			Factory.Save();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();
				cost.E6_CostReference = "ABC";
				cost.PrepareForPosting();
				AssertEquals("Supplier Cost Reference on Consol Cost should have been copied to Job Charge", cost.E6_CostReference, apps.CostsCollection[0].ApportionmentCharges[0].InvoicingJob.Charges[0].JR_CostReference);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestCopyValues_JR_APDocumentReceivedDate()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			Factory.Save();

			var apps = new ApportionmentListing(Factory, consol);
			try
			{
				var cost = apps.CostsCollection.TryAddNew();
				cost.E6_DocumentReceivedDate = ZDateTime.Today;
				cost.PrepareForPosting();
				AssertEquals("Document Received Date on Consol Cost should have been copied to Job Charge", cost.E6_DocumentReceivedDate, apps.CostsCollection[0].ApportionmentCharges[0].InvoicingJob.Charges[0].JR_APDocumentReceivedDate);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestSettingCreditorWhenSettingChargeCode()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_IsCreditor = true;
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Code = "FOR TEST CREDITOR";
			Factory.Save();
			consol.JK_OA_CreditorAddress = orgAddress.PK;
			Assert("Console precondition", !consol.CreditorPK.IsEmpty);
			var allChargeGroups = new[] { ChargeCodeGroupList.Codes.Brokerage, ChargeCodeGroupList.Codes.BrokerageOnly, ChargeCodeGroupList.Codes.CFSLoadList, ChargeCodeGroupList.Codes.CFSShipment, ChargeCodeGroupList.Codes.ContainerStorage, ChargeCodeGroupList.Codes.CustomsDuty, ChargeCodeGroupList.Codes.Destination, ChargeCodeGroupList.Codes.Freight, ChargeCodeGroupList.Codes.Insurance, ChargeCodeGroupList.Codes.Loading, ChargeCodeGroupList.Codes.NonJobRelated, ChargeCodeGroupList.Codes.NotGrouped, ChargeCodeGroupList.Codes.Origin, ChargeCodeGroupList.Codes.OriginBrokerage, ChargeCodeGroupList.Codes.OriginBrokerageOnly, ChargeCodeGroupList.Codes.ShippingDisbursements, ChargeCodeGroupList.Codes.Transport, ChargeCodeGroupList.Codes.Unloading, ChargeCodeGroupList.Codes.WHSInwards, ChargeCodeGroupList.Codes.WHSOutwards, ChargeCodeGroupList.Codes.WHSStorage };
			var apps = new ApportionmentListing(Factory, consol);
			var newFactory = NewFactory();
			foreach (var chargeGroup in allChargeGroups)
			{
				var chargeGroupCode = ObjectCreator.LoadAccChargeCode(chargeGroup, GlbCompany.CurrentCompany.PK);
				if (chargeGroupCode == null)
				{
					chargeGroupCode = newFactory.NewWithValidTestData<AccChargeCode>();
					chargeGroupCode.AC_Code = chargeGroup;
				}

				chargeGroupCode.AC_IsGroupageCharge = true;
				newFactory.Save();
				var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				chargeCode.AC_ChargeGroup = chargeGroup;
				chargeCode.AC_IsGroupageCharge = true;
				var cost = apps.CostsCollection.TryAddNew();
				Assert("Cost precondition", cost.E6_OH_Creditor.IsEmpty);
				cost.E6_AC_ChargeCode = chargeCode.PK;
				AssertEquals("Creditor was set", consol.CreditorPK, cost.E6_OH_Creditor);
				var otherChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				otherChargeCode.AC_IsGroupageCharge = true;
				chargeCode.AC_ChargeGroup = chargeGroup;
				cost.E6_AC_ChargeCode = otherChargeCode.PK;
				AssertEquals("Creditor was not overriden", consol.CreditorPK, cost.E6_OH_Creditor);
				AssertNoExceptionThrown("Setting empty creditor must not throw exception", () =>
				{
					cost.E6_AC_ChargeCode = ZGuid.Empty;
				}

				);
			}
		}

		(OrgHeader creditor,
		 OrgHeader carrier,
		 OrgHeader sendingAgent,
		 OrgHeader receivingAgent,
		 OrgHeader creditorOriginAgent,
		 OrgHeader creditorDestinationAgent,
		 OrgHeader carrierOriginAgent,
		 OrgHeader carrierDestinationAgent) CreateConsolOrgs(string origin, string destination)
		{
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;

			var creditorOriginAgentOrg = Factory.NewWithValidTestData<OrgHeader>();
			creditorOriginAgentOrg.OH_IsCreditor = true;
			var creditorOriginAgentPort = creditor.CarrierAppointedAgentPorts_Agency.AddNew();
			creditorOriginAgentPort.O5_PortOrCountry = origin;
			creditorOriginAgentPort.O5_OA_AgentOfficeAddress = creditorOriginAgentOrg.MainAddress.PK;

			var creditorDestinationAgentOrg = Factory.NewWithValidTestData<OrgHeader>();
			creditorDestinationAgentOrg.OH_IsCreditor = true;
			var creditorDestinationAgentPort = creditor.CarrierAppointedAgentPorts_Agency.AddNew();
			creditorDestinationAgentPort.O5_PortOrCountry = destination;
			creditorDestinationAgentPort.O5_OA_AgentOfficeAddress = creditorDestinationAgentOrg.MainAddress.PK;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;

			var carrierOriginAgentOrg = Factory.NewWithValidTestData<OrgHeader>();
			carrierOriginAgentOrg.OH_IsCreditor = true;
			var carrierOriginAgentPort = carrier.CarrierAppointedAgentPorts_Agency.AddNew();
			carrierOriginAgentPort.O5_PortOrCountry = origin;
			carrierOriginAgentPort.O5_OA_AgentOfficeAddress = carrierOriginAgentOrg.MainAddress.PK;

			var carrierDestinationAgentOrg = Factory.NewWithValidTestData<OrgHeader>();
			carrierDestinationAgentOrg.OH_IsCreditor = true;
			var carrierDestinationAgentPort = carrier.CarrierAppointedAgentPorts_Agency.AddNew();
			carrierDestinationAgentPort.O5_PortOrCountry = destination;
			carrierDestinationAgentPort.O5_OA_AgentOfficeAddress = carrierDestinationAgentOrg.MainAddress.PK;

			var sendingAgent = Factory.NewWithValidTestData<OrgHeader>();
			sendingAgent.OH_IsCreditor = true;

			var receivingAgent = Factory.NewWithValidTestData<OrgHeader>();
			receivingAgent.OH_IsCreditor = true;

			return (creditor, carrier, sendingAgent, receivingAgent, creditorOriginAgentOrg,
				creditorDestinationAgentOrg, carrierOriginAgentOrg, carrierDestinationAgentOrg);
		}

		public void TestSettingCreditor_AgentConsol()
		{
			var origin = "AUSYD";
			var destination = "USLAX";

			var orgs = CreateConsolOrgs(origin, destination);
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = origin;
			consol.JK_RL_NKDischargePort = destination;
			consol.JK_OA_ShippingLineAddress = orgs.carrier.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = orgs.sendingAgent.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = orgs.receivingAgent.MainAddress.PK;
			consol.JK_OA_CreditorAddress = orgs.creditor.MainAddress.PK;

			Factory.Save();

			var costCollection = new ApportionmentListing(Factory, consol).CostsCollection;
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_IsGroupageCharge = true;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
				consol.JK_OA_CreditorAddress = orgs.creditor.MainAddress.PK;

				AssertEquals("Consol precondition : Export", true, consol.IsExport());

				var cost1 = costCollection.TryAddNew();
				Assert("Cost precondition", cost1.E6_OH_Creditor.IsEmpty);

				cost1.E6_AC_ChargeCode = chargeCode.PK;
				AssertEquals(orgs.creditor.PK, cost1.E6_OH_Creditor);

				consol.JK_OA_CreditorAddress = orgs.creditor.MainAddress.PK;
				consol.JK_PrepaidCollect = Constants.PaymentType.Collect;

				var cost2 = costCollection.TryAddNew();
				Assert("Cost precondition", cost2.E6_OH_Creditor.IsEmpty);

				cost2.E6_AC_ChargeCode = chargeCode.PK;
				AssertEquals(consol.CreditorPK, consol.CarrierExportCreditorAddress.OrganisationPK);
				AssertEquals(consol.CarrierExportCreditorAddress.OrganisationPK, cost2.E6_OH_Creditor);  // New logic introduced in WI00489897 - !CR7 DHL: Creditor Defaulting for Manual Input Charges in Consol Costing
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
			{
				consol.JK_OA_CreditorAddress = orgs.creditor.MainAddress.PK;
				consol.JK_PrepaidCollect = Constants.PaymentType.Collect;
				AssertEquals("Consol precondition: Import", true, consol.IsImport());

				var cost1 = costCollection.TryAddNew();
				Assert("Cost precondition", cost1.E6_OH_Creditor.IsEmpty);

				cost1.E6_AC_ChargeCode = chargeCode.PK;
				AssertEquals(orgs.creditor.PK, cost1.E6_OH_Creditor);

				consol.JK_OA_CreditorAddress = orgs.creditor.MainAddress.PK;
				consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;

				var cost2 = costCollection.TryAddNew();
				Assert("Cost precondition", cost2.E6_OH_Creditor.IsEmpty);

				cost2.E6_AC_ChargeCode = chargeCode.PK;
				AssertEquals(consol.CreditorPK, consol.CarrierImportCreditorAddress.OrganisationPK);
				AssertEquals(consol.CarrierImportCreditorAddress.OrganisationPK, cost2.E6_OH_Creditor);  // New logic introduced in WI00489897 - !CR7 DHL: Creditor Defaulting for Manual Input Charges in Consol Costing
			}
		}

		public void TestSettingCreditor_AgentConsol_EmptyCreditor_FallbackToCarrier()
		{
			var origin = "AUSYD";
			var destination = "USLAX";

			var orgs = CreateConsolOrgs(origin, destination);
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = origin;
			consol.JK_RL_NKDischargePort = destination;
			consol.JK_OA_ShippingLineAddress = orgs.carrier.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = orgs.sendingAgent.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = orgs.receivingAgent.MainAddress.PK;
			consol.JK_OA_CreditorAddress = ZGuid.Empty;

			Factory.Save();

			var costCollection = new ApportionmentListing(Factory, consol).CostsCollection;
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_IsGroupageCharge = true;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
				consol.CreditorPK = ZGuid.Empty;

				AssertEquals("Consol precondition : Export", true, consol.IsExport());
				Assert("Consol precondition: Creditor is Empty", consol.CreditorPK.IsEmpty);

				var cost = costCollection.TryAddNew();
				Assert("Cost precondition", cost.E6_OH_Creditor.IsEmpty);

				cost.E6_AC_ChargeCode = chargeCode.PK;
				AssertEquals(orgs.carrier.PK, cost.E6_OH_Creditor);

				consol.JK_PrepaidCollect = Constants.PaymentType.Collect;
				consol.CreditorPK = ZGuid.Empty;

				var cost2 = costCollection.TryAddNew();
				Assert("Cost precondition", cost2.E6_OH_Creditor.IsEmpty);

				cost2.E6_AC_ChargeCode = chargeCode.PK;
				AssertEquals(orgs.carrier.PK, cost2.E6_OH_Creditor);  // New logic introduced in WI00489897 - !CR7 DHL: Creditor Defaulting for Manual Input Charges in Consol Costing
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
			{
				consol.JK_PrepaidCollect = Constants.PaymentType.Collect;
				consol.CreditorPK = ZGuid.Empty;

				AssertEquals("Consol precondition: Import", true, consol.IsImport());
				Assert("Consol precondition: Creditor is Empty", consol.CreditorPK.IsEmpty);

				var cost = costCollection.TryAddNew();
				Assert("Cost precondition", cost.E6_OH_Creditor.IsEmpty);

				cost.E6_AC_ChargeCode = chargeCode.PK;
				AssertEquals(orgs.carrier.PK, cost.E6_OH_Creditor);

				consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
				consol.CreditorPK = ZGuid.Empty;

				var cost2 = costCollection.TryAddNew();
				Assert("Cost precondition", cost2.E6_OH_Creditor.IsEmpty);

				cost2.E6_AC_ChargeCode = chargeCode.PK;
				AssertEquals(orgs.carrier.PK, cost2.E6_OH_Creditor);  // New logic introduced in WI00489897 - !CR7 DHL: Creditor Defaulting for Manual Input Charges in Consol Costing
			}
		}

		public void TestSettingCreditor_CoLoadConsol_HasCreditor()
		{
			var origin = "AUSYD";
			var destination = "USLAX";

			var orgs = CreateConsolOrgs(origin, destination);
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_RL_NKLoadPort = origin;
			consol.JK_RL_NKDischargePort = destination;
			consol.JK_OA_CreditorAddress = orgs.creditor.MainAddress.PK;
			consol.JK_OA_ShippingLineAddress = orgs.carrier.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = orgs.sendingAgent.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = orgs.receivingAgent.MainAddress.PK;
			Factory.Save();

			var costCollection = new ApportionmentListing(Factory, consol).CostsCollection;
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_IsGroupageCharge = true;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				consol.JK_OA_CreditorAddress = orgs.creditor.MainAddress.PK;
				consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
				var cost = costCollection.TryAddNew();
				Assert("Cost precondition", cost.E6_OH_Creditor.IsEmpty);
				cost.E6_AC_ChargeCode = chargeCode.PK;
				AssertEquals(orgs.creditor.PK, cost.E6_OH_Creditor);

				consol.JK_OA_CreditorAddress = orgs.creditor.MainAddress.PK;
				consol.JK_PrepaidCollect = Constants.PaymentType.Collect;
				var cost2 = costCollection.TryAddNew();
				Assert("Cost precondition", cost2.E6_OH_Creditor.IsEmpty);
				cost2.E6_AC_ChargeCode = chargeCode.PK;
				AssertEquals(orgs.creditor.PK, cost2.E6_OH_Creditor);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
			{
				consol.JK_OA_CreditorAddress = orgs.creditor.MainAddress.PK;
				consol.JK_PrepaidCollect = Constants.PaymentType.Collect;
				var cost = costCollection.TryAddNew();
				Assert("Cost precondition", cost.E6_OH_Creditor.IsEmpty);
				cost.E6_AC_ChargeCode = chargeCode.PK;
				AssertEquals(orgs.creditor.PK, cost.E6_OH_Creditor);

				consol.JK_OA_CreditorAddress = orgs.creditor.MainAddress.PK;
				consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
				var cost2 = costCollection.TryAddNew();
				Assert("Cost precondition", cost2.E6_OH_Creditor.IsEmpty);
				cost2.E6_AC_ChargeCode = chargeCode.PK;
				AssertEquals(orgs.creditor.PK, cost2.E6_OH_Creditor);
			}
		}

		public void TestSettingCreditor_CoLoadConsol_EmptyCreditor_FallbackToCarrier()
		{
			var origin = "AUSYD";
			var destination = "USLAX";

			var orgs = CreateConsolOrgs(origin, destination);
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_RL_NKLoadPort = origin;
			consol.JK_RL_NKDischargePort = destination;
			consol.JK_OA_ShippingLineAddress = orgs.carrier.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = orgs.sendingAgent.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = orgs.receivingAgent.MainAddress.PK;
			Factory.Save();

			var costCollection = new ApportionmentListing(Factory, consol).CostsCollection;
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_IsGroupageCharge = true;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				consol.JK_OA_CreditorAddress = ZGuid.Empty;
				consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
				var cost = costCollection.TryAddNew();
				Assert("Cost precondition", cost.E6_OH_Creditor.IsEmpty);
				cost.E6_AC_ChargeCode = chargeCode.PK;
				AssertEquals(orgs.carrier.PK, cost.E6_OH_Creditor);

				consol.JK_OA_CreditorAddress = ZGuid.Empty;
				consol.JK_PrepaidCollect = Constants.PaymentType.Collect;
				var cost2 = costCollection.TryAddNew();
				Assert("Cost precondition", cost2.E6_OH_Creditor.IsEmpty);
				cost2.E6_AC_ChargeCode = chargeCode.PK;
				AssertEquals(orgs.receivingAgent.PK, cost2.E6_OH_Creditor);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
			{
				consol.JK_OA_CreditorAddress = ZGuid.Empty;
				consol.JK_PrepaidCollect = Constants.PaymentType.Collect;
				var cost = costCollection.TryAddNew();
				Assert("Cost precondition", cost.E6_OH_Creditor.IsEmpty);
				cost.E6_AC_ChargeCode = chargeCode.PK;
				AssertEquals(orgs.carrier.PK, cost.E6_OH_Creditor);

				consol.JK_OA_CreditorAddress = ZGuid.Empty;
				consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
				var cost2 = costCollection.TryAddNew();
				Assert("Cost precondition", cost2.E6_OH_Creditor.IsEmpty);
				cost2.E6_AC_ChargeCode = chargeCode.PK;
				AssertEquals(orgs.sendingAgent.PK, cost2.E6_OH_Creditor);
			}
		}

		/// <summary>
		/// This method reloads the charge in 2 separate factories (that have data refresh turned off), sets 'value1' and 'value2' on each copy respectively.
		/// The first factory is saved and then the second, which will cause a concurrency issue.
		/// The method then asserts that the exception handler reported that the error was critical and couldn't be merged.
		/// </summary>
		/// <param name = "cost">This is the JobConsolCost created for testing. It will get reloaded in other factories in this method so that the concurrency can be tested</param>
		/// <param name = "column">This is the column that should be set by value1 and value2</param>
		/// <param name = "value1">This is the value that gets set on the first reloaded copy of the cost.</param>
		/// <param name = "value2">This is the value that gets set on the second reloaded copy of the reloaded cost, which when saved will cause a concurrency exception</param>
		void AssertConcurrency(JobConsolCost cost, SchemaColumn column, IZType value1, IZType value2)
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			JobConsolCost costInFactory1 = factory1.Load<JobConsolCost>(cost.PK);
			JobConsolCost costInFactory2 = factory2.Load<JobConsolCost>(cost.PK);
			costInFactory1[column] = value1;
			costInFactory2[column] = value2;
			factory1.Save();
			try
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				factory2.Save();
			}
			catch (Exception e)
			{
				ZExceptionReporting.HandleSaveException(e);
			}

			Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("The system cannot automatically merge your changes because there are conflicts with critical fields."));
		}

		public void TestIApportionedChargesHeaderImplementation()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 351.73m;
			shipment1.JS_ActualChargeable = 40m;
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ActualWeight = 786.23m;
			shipment2.JS_ActualChargeable = 60m;
			Job job1 = new Job.Loader(shipment1).TryCreateWithoutMutexForTestOnly();
			Job job2 = new Job.Loader(shipment2).TryCreateWithoutMutexForTestOnly();
			JobConsolCost cost = GetCost(consol);
			var testObjectCreator = new TestObjectCreator(Factory);
			try
			{
				ApportionSplitCharge charge1 = FindChargeForJob(cost, shipment1);
				ApportionSplitCharge charge2 = FindChargeForJob(cost, shipment2);
				RatingDataRegistry.Instance.UnspecifiedCostShouldForceZeroToBePulledThrough.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				if (cost.E6_AC_ChargeCode.IsEmpty)
				{
					cost.E6_AC_ChargeCode = testObjectCreator.CC1.PK;
				}

				cost.E6_OSCostAmount = 200m;
				cost.E6_ApportionmentMethod = AllocationMethod.Shipment;
				IApportionedChargesHeader costAsHeader = cost;
				AssertEquals("ApportionmentMethod", cost.E6_ApportionmentMethod, costAsHeader.ApportionmentMethod);
				AssertEquals("ChargeCode", cost.ChargeCode, costAsHeader.ChargeCode);
				AssertEquals("Currency", cost.Currency, costAsHeader.Currency);
				AssertEquals("Charges.Length", cost.ApportionmentCharges.Count, costAsHeader.Charges.Length);
				AssertEquals("Expect to have 2 Charges", 2, costAsHeader.Charges.Length);
				AssertEquals("IsChargeReadyToPost Charge 1", true, costAsHeader.IsChargeReadyToPost(costAsHeader.Charges[0]));
				AssertEquals("IsChargeReadyToPost Charge 2", true, costAsHeader.IsChargeReadyToPost(costAsHeader.Charges[1]));
				cost.ApportionmentCharges[0].JR_IsUsedForApportionment = false;
				AssertEquals("IsChargeReadyToPost Charge 1", false, costAsHeader.IsChargeReadyToPost(costAsHeader.Charges[0]));
				AssertEquals("IsChargeReadyToPost Charge 2", true, costAsHeader.IsChargeReadyToPost(costAsHeader.Charges[1]));
				var invoice = testObjectCreator.CreateAPInvoice<APInvoice>("001", testObjectCreator.AUD, 1M, 200M, 0M, 0M, 200M, 0M, 0M, testObjectCreator.AALSHI);
				var line = testObjectCreator.CreateAPInvoiceLine(invoice, job2, cost.ChargeCode, testObjectCreator.AUD, 1M, "Cost", 200M);
				cost.ApportionmentCharges[1].JR_AL_APLine = line.PK;
				Assert(cost.ApportionmentCharges[1].IsCostPosted);
				AssertEquals("IsChargeReadyToPost Charge 1", false, costAsHeader.IsChargeReadyToPost(costAsHeader.Charges[0]));
				AssertEquals("IsChargeReadyToPost Charge 2", false, costAsHeader.IsChargeReadyToPost(costAsHeader.Charges[1]));
				cost.E6_ApportionmentMethod = AllocationMethod.Manual;
				AssertEquals("ApportionmentMethod", cost.E6_ApportionmentMethod, costAsHeader.ApportionmentMethod);
			}
			finally
			{
				cost.CalculationStrategy.ReleaseMutexes();
			}
		}

		public void TestE6_PaymentDateIfCreditorHasDefaultTerm()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			consol.Shipments.AddNew();
			OrgHeader creditor = Factory.New<OrgHeader>();
			creditor.APSettlementGroupPK = Factory.New<OrgHeader>().PK;
			creditor.APSettlementGroup.CompanyData.OB_APPaymentTerms = InvoiceTermsList.FromInvoiceDate.Code;
			creditor.APSettlementGroup.CompanyData.OB_APPaymentTermDays = 5;
			creditor.CompanyData.OB_APPaymentTerms = OrgCompanyDataLookups.DefaultInvoiceTerm.Code;
			JobConsolCost cost = GetCost(consol);
			try
			{
				cost.E6_OH_Creditor = creditor.PK;
				cost.E6_InvoiceDate = ZDateTime.Now;
				AssertEquals(cost.E6_InvoiceDate.AddDays(5), cost.E6_PaymentDate);
			}
			finally
			{
				cost.CalculationStrategy.ReleaseMutexes();
			}
		}

		public void TestPopulateMissingApportionments()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001001";
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S00001001";
			shipment1.JS_ActualWeight = 100.00m;
			shipment1.JS_TransportMode = Constants.TransportModes.Sea;
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S00001002";
			shipment2.JS_ActualWeight = 200.00m;
			shipment2.JS_TransportMode = Constants.TransportModes.Sea;
			ForwardingShipment shipment3 = shipment2.CoLoadShipments.AddNew();
			shipment3.JS_UniqueConsignRef = "S00001003";
			shipment3.JS_ActualWeight = 300.00m;
			shipment3.JS_TransportMode = Constants.TransportModes.Sea;
			ForwardingShipment shipment4 = shipment2.CoLoadShipments.AddNew();
			shipment4.JS_UniqueConsignRef = "S00001004";
			shipment4.JS_ActualWeight = 400.00m;
			shipment4.JS_TransportMode = Constants.TransportModes.Sea;
			Job job1 = ObjectCreator.CreateJob(shipment1);
			Job job2 = ObjectCreator.CreateJob(shipment2);
			Job job3 = ObjectCreator.CreateJob(shipment3);
			Job job4 = ObjectCreator.CreateJob(shipment4);
			Factory.Save();
			AccChargeCode chargeCode = ObjectCreator.CC1;
			OrgHeader orgHeader = ObjectCreator.AALSHI;
			ApportionmentListing listing = new ApportionmentListing(Factory, consol);
			JobConsolCost jobConsolCost = listing.CostsCollection.TryAddNew();
			jobConsolCost.E6_AC_ChargeCode = chargeCode.PK;
			jobConsolCost.E6_GC = GlbCompany.CurrentCompany.PK;
			jobConsolCost.E6_RX_NKCurrency = ObjectCreator.AUD.RX_Code;
			jobConsolCost.E6_ExchangeRate = 1.0m;
			jobConsolCost.E6_OSCostAmount = 2000.00m;
			jobConsolCost.E6_OH_Creditor = orgHeader.PK;
			jobConsolCost.E6_ApportionToRelatedShipments = true;
			jobConsolCost.E6_ApportionmentMethod = AllocationMethod.ChargeableUnits;
			AssertEquals("jobConsolCost.ApportionmentCharges.Count", 4, jobConsolCost.ApportionmentCharges.Count);
			AssertEquals("IsUsedForApportionment", false, jobConsolCost.ApportionmentCharges[2].JR_IsUsedForApportionment);
			jobConsolCost.ApportionmentCharges[2].JR_IsUsedForApportionment = true;
			jobConsolCost.E6_ApportionToRelatedShipments = false;
			AssertEquals("jobConsolCost.ApportionmentCharges.Count", 3, jobConsolCost.ApportionmentCharges.Count);
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			APInvoice invoice = newFactory.New<APInvoice>();
			invoice.AH_OH = orgHeader.PK;
			invoice.AH_TransactionNum = "001";
			invoice.AH_ChequeOrReference = "ABC";
			invoice.SubmittedFromInvoicingForm = true;
			InvoicingBaseBulkConsolCostImporter importer = new InvoicingBaseBulkConsolCostImporter(invoice.ConsolCosting);
			importer.LoadConsolsCollection();
			importer.Import();
			AssertEquals("Should be one ConsolCost imported", 1, invoice.ConsolCosting.ConsolCosts.Count);
			AssertEquals("Should have two charges imported", 3, invoice.ConsolCosting.ConsolCosts[0].ApportionmentCharges.Count);
			AssertEquals("Supplier Cost Reference should be copied to new charge", invoice.AH_ChequeOrReference, invoice.ConsolCosting.ConsolCosts[0].ApportionmentCharges[2].JR_CostReference);
			invoice.ImportAllApportionmentsFromCosting();
			AssertEquals("Should have three invoice lines", 3, invoice.Lines.Count);
			newFactory.Save();
		}

		public void TestDontValidatePostedAndReversedCosts()
		{
			ForwardingConsol consol = ObjectCreator.CreateConsol("AUD", "LAX", "C0001");
			ForwardingShipment shipment1 = ObjectCreator.CreateShipment("S0001", consol);
			ForwardingShipment shipment2 = ObjectCreator.CreateShipment("S0002", consol);
			Job job1 = ObjectCreator.CreateJob(shipment1);
			Job job2 = ObjectCreator.CreateJob(shipment2);
			JobConsolCost consolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC12, ObjectCreator.AALSHI);
			consolCost.E6_RX_NKCurrency = "AUD";
			consolCost.E6_OSCostAmount = 100M;
			consolCost.E6_ApportionmentMethod = AllocationMethod.Shipment;
			consolCost.E6_InvoiceNum = "ABC123";
			consolCost.E6_InvoiceDate = ZDateTime.Now;
			consolCost.E6_PaymentType = ReceiptTypes.Cheque;
			consolCost.E6_AB_BankAccount = ObjectCreator.AUDBankAccount.PK;
			consolCost.E6_AK_ChequeBook = ObjectCreator.AUDChequeBook.PK;
			consolCost.RunPreSaveValidation();
			AssertEquals("Precondition: cost must not have errors.", false, consolCost.HasErrors);
			ObjectCreator.AUDChequeBook.AK_IsActive = false;
			consolCost.RunPreSaveValidation();
			AssertEquals("Do validate non posted cost.", true, consolCost.HasErrors);
			APInvoice invoice = ObjectCreator.CreateAPInvoice<APInvoice>("INV1", ObjectCreator.AUD, 1M, 10M, 0M, 0M, 10M, 0M, 0M, ObjectCreator.AALSHI);
			APInvoiceLine line1 = ObjectCreator.CreateAPInvoiceLine(invoice, job1, ObjectCreator.CC1, ObjectCreator.AUD, 1M, "Desc", 5M);
			APInvoiceLine line2 = ObjectCreator.CreateAPInvoiceLine(invoice, job2, ObjectCreator.CC1, ObjectCreator.AUD, 1M, "Desc", 5M);
			consolCost.E6_AH_APInvoice = invoice.PK;
			consolCost.ApportionmentCharges[0].JR_AL_APLine = line1.PK;
			consolCost.ApportionmentCharges[1].JR_AL_APLine = line1.PK;
			consolCost.RunPreSaveValidation();
			AssertEquals("Don't validate posted cost.", false, consolCost.HasErrors);
			job1.Dispose();
			job2.Dispose();
		}

		public void TestRunPreSaveValidationAfterE6_OSCostAmountChanged()
		{
			ForwardingConsol consol = ObjectCreator.CreateConsol("AUD", "LAX", "C0001");
			ForwardingShipment shipment1 = ObjectCreator.CreateShipment("S0001", consol);
			ForwardingShipment shipment2 = ObjectCreator.CreateShipment("S0002", consol);
			Job job1 = ObjectCreator.CreateJob(shipment1);
			Job job2 = ObjectCreator.CreateJob(shipment2);
			JobConsolCost consolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC12, ObjectCreator.AALSHI);
			consolCost.E6_RX_NKCurrency = "AUD";
			consolCost.E6_OSCostAmount = 100M;
			consolCost.E6_AT_TaxRate = ObjectCreator.GST1.PK;
			consolCost.E6_ApportionmentMethod = AllocationMethod.Shipment;
			consolCost.E6_InvoiceNum = "ABC123";
			consolCost.E6_InvoiceDate = ZDateTime.Now;
			consolCost.E6_PaymentType = ReceiptTypes.Cheque;
			consolCost.E6_AB_BankAccount = ObjectCreator.AUDBankAccount.PK;
			consolCost.E6_AK_ChequeBook = ObjectCreator.AUDChequeBook.PK;
			AssertEquals("Light Validation is disabled", false, consolCost.EnableLightValidationIfAvailable_ForTestOnly());

			consolCost.RunPreSaveValidation();
			AssertEquals("Precondition: cost must not have errors.", false, consolCost.HasErrors);

			Factory.Save();
			consolCost.E6_OSCostAmount = 20M;
			consolCost.ApportionmentCharges[0].JR_OSCostAmt = 0m; // Reset OS Cost Amount on one of the Apportionments
			AssertNotEquals("Unapportioned Amount", 0m, consolCost.UnApportionedAmount);

			consolCost.RunPreSaveValidation();
			AssertEquals("Do validate Consol Cost as a key value was changed.", true, consolCost.HasErrors);
			AssertHasError(consolCost.UnApportionedAmountInfo, "Please ensure that this Cost Amount is fully apportioned.");

			consolCost.SplitApportionAmount();
			AssertEquals("Unapportioned Amount", 0m, consolCost.UnApportionedAmount);

			consolCost.RunPreSaveValidation();
			AssertEquals("Consol Cost got forced to run Validation and found no errors.", false, consolCost.HasErrors);

			Factory.Save();
			consolCost.E6_OSCostAmount = 10M;
			AssertEquals("Unapportioned Amount", 0m, consolCost.UnApportionedAmount);
			APInvoice invoice = ObjectCreator.CreateAPInvoice<APInvoice>("ABC123", ObjectCreator.AUD, 1M, 10M, 0M, 0M, 10M, 0M, 0M, ObjectCreator.AALSHI);
			APInvoiceLine line1 = ObjectCreator.CreateAPInvoiceLine(invoice, job1, ObjectCreator.CC1, ObjectCreator.AUD, 1M, "Desc", 5M);
			APInvoiceLine line2 = ObjectCreator.CreateAPInvoiceLine(invoice, job2, ObjectCreator.CC1, ObjectCreator.AUD, 1M, "Desc", 5M);
			consolCost.E6_AH_APInvoice = invoice.PK;
			consolCost.ApportionmentCharges[0].ReverseAccrual(ZDateTime.Now);
			consolCost.ApportionmentCharges[0].JR_AL_APLine = line1.PK;
			consolCost.ApportionmentCharges[1].ReverseAccrual(ZDateTime.Now);
			consolCost.ApportionmentCharges[1].JR_AL_APLine = line2.PK;
			ObjectCreator.AUDChequeBook.AK_IsActive = false;
			ObjectCreator.AALSHI.CompanyData.OB_IsCreditor = false;
			consolCost.RunPreSaveValidation();
			AssertEquals("Don't validate posted cost.", false, consolCost.HasErrors);

			job1.Dispose();
			job2.Dispose();
		}

		public void TestDontAttachConsolCostToChargePostedWithJobRevenueJournal()
		{
			ForwardingConsol consol = ObjectCreator.CreateConsol("AUSYD", "USLAX", "C0001");
			ForwardingShipment shipment = ObjectCreator.CreateShipment("S0001", consol);
			Job job = ObjectCreator.CreateJob(shipment);
			JobRevenueJournal journal = ObjectCreator.CreateJobRevenueJournal(ObjectCreator.CC1, job, 100M);
			Factory.Save();
			AssertEquals("Precondition: Charges.Count", 2, job.Charges.Count);
			Charge charge1 = job.Charges[0];
			Charge charge2 = job.Charges[1];
			AssertEquals("Precondition: charge1.JR_IsApportioned", false, charge1.JR_IsApportioned);
			AssertEquals("Precondition: charge1.IsRevenuePostedWithManualJobRevenueJournal", true, charge1.IsRevenuePostedWithManualJobRevenueJournal);
			AssertEquals("Precondition: charge2.JR_IsApportioned", false, charge2.JR_IsApportioned);
			AssertEquals("Precondition: charge2.IsRevenuePostedWithManualJobRevenueJournal", true, charge2.IsRevenuePostedWithManualJobRevenueJournal);
			ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, null).E6_OSCostAmount = 100M;
			Factory.Save();
			AssertEquals("Charge with job revenue journal posted can't be chosen for apportionment.", false, charge1.JR_IsApportioned);
			AssertEquals("Charge with job revenue journal posted can't be chosen for apportionment.", false, charge2.JR_IsApportioned);
			AssertEquals("Charges.Count", 3, job.Charges.Count);
			Charge newCharge = (
				from Charge charge in job.Charges
				where charge.PK != charge1.PK && charge.PK != charge2.PK
				select charge).First();
			AssertEquals("newCharge.JR_IsApportioned ", true, newCharge.JR_IsApportioned);
		}

		public void TestDisplaySequenceWithChargeCodeAlreadyUsedOnShipments()
		{
			var consol = ObjectCreator.CreateConsol("AUSYD", "USLAX", "C0001");
			var shipment = ObjectCreator.CreateShipment("S0001", consol);
			var job = ObjectCreator.CreateJob(shipment);
			var charge1 = job.Charges.AddNew();
			charge1.JR_AC = ObjectCreator.CC1.PK;
			var charge2 = job.Charges.AddNew();
			charge2.JR_AC = ObjectCreator.CC2.PK;
			var charge3 = job.Charges.AddNew();
			charge3.JR_AC = ObjectCreator.CC3.PK;
			var charge4 = job.Charges.AddNew();
			charge4.JR_AC = ObjectCreator.CC4.PK;
			var charge5 = job.Charges.AddNew();
			charge5.JR_AC = ObjectCreator.CC5.PK;
			var charge6 = job.Charges.AddNew();
			charge6.JR_AC = ObjectCreator.CC6.PK;
			job.Charges[0].JR_DisplaySequence = 1;
			job.Charges[1].JR_DisplaySequence = 2;
			job.Charges[2].JR_DisplaySequence = 3;
			job.Charges[3].JR_DisplaySequence = 4;
			job.Charges[4].JR_DisplaySequence = 5;
			job.Charges[5].JR_DisplaySequence = 6;
			Factory.Save();
			var cost1 = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, ObjectCreator.ABIGAS);
			cost1.E6_OSCostAmount = 100M;
			var cost2 = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC2, ObjectCreator.ABIGAS);
			cost2.E6_OSCostAmount = 100M;
			var cost3 = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC3, ObjectCreator.ABIGAS);
			cost3.E6_OSCostAmount = 100M;
			Factory.Save();
			for (var i = 0; i < job.Charges.Count; i++)
			{
				AssertEquals("There should be no gaps in the display sequence", i + 1, (int)job.Charges[i].JR_DisplaySequence);
			}
		}

		public void TestDisplaySequenceWithMultipleShipments()
		{
			var consol = ObjectCreator.CreateConsol("AUSYD", "USLAX", "C0001");
			var shipment = ObjectCreator.CreateShipment("S0001", consol);
			var shipment2 = ObjectCreator.CreateShipment("S0002", consol);
			var job = ObjectCreator.CreateJob(shipment);
			var job2 = ObjectCreator.CreateJob(shipment2);
			var charge1 = job.Charges.AddNew();
			charge1.JR_AC = ObjectCreator.CC1.PK;
			var charge2 = job.Charges.AddNew();
			charge2.JR_AC = ObjectCreator.CC2.PK;
			var charge3 = job.Charges.AddNew();
			charge3.JR_AC = ObjectCreator.CC3.PK;
			job.Charges[0].JR_DisplaySequence = 1;
			job.Charges[1].JR_DisplaySequence = 2;
			job.Charges[2].JR_DisplaySequence = 3;
			Factory.Save();
			var cost1 = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC5, ObjectCreator.ABIGAS);
			cost1.E6_OSCostAmount = 100M;
			var cost2 = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC6, ObjectCreator.AALSHI);
			cost2.E6_OSCostAmount = 100M;
			var cost3 = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC7, null);
			cost3.E6_OSCostAmount = 100M;
			Factory.Save();
			for (var i = 0; i < job.Charges.Count; i++)
			{
				AssertEquals("There should be no gaps in the display sequence", i + 1, (int)job.Charges[i].JR_DisplaySequence);
			}
		}

		public void TestIsForIncompleteInvoice()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			Job job = ObjectCreator.CreateJob(shipment, false);
			Factory.Save();
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			var cost = AddNewConsolCostToInvoice(consol, invoice);
			cost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			cost.E6_OSCostAmount = 75m;
			cost.ApportionmentCharges[0].JR_OSCostAmt = 75m;
			invoice.ImportAllApportionmentsFromCosting();
			AssertEquals("IsForIncompleteInvoice", false, cost.IsForIncompleteInvoice);
			AssertEquals("IsSavedByFactory", true, cost.IsSavedByFactory);
			invoice.SaveAsIncomplete();
			AssertEquals("IsForIncompleteInvoice", true, cost.IsForIncompleteInvoice);
			AssertEquals("IsSavedByFactory", false, cost.IsSavedByFactory);
		}

		[TestDate(2021, 06, 29)]
		public void TestIsPostedCorrectlyAndHasSynchronisedAPInvoiceDetails()
		{
			var creator = new TestObjectCreator(Factory);
			//Creating Consol
			var consol = creator.CreateConsol("", "", "TSTCNSL");
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUMEL";
			CommonShipment shipment1 = creator.CreateShipment("S00010001", "USLAX", "AUMEL");
			consol.Shipments.Add(shipment1);
			Job job1 = new Job.Loader(Factory, shipment1).TryCreateWithoutMutexForTestOnly();
			CommonShipment shipment2 = creator.CreateShipment("S00010002", "USLAX", "AUMEL");
			consol.Shipments.Add(shipment2);
			Job job2 = new Job.Loader(Factory, shipment2).TryCreateWithoutMutexForTestOnly();
			Factory.Save();
			//Creating Consol Cost
			var apportionmentListing = consol.GetApportionments();
			var cost = CreateConsolCost(apportionmentListing);
			AssertHasSynchronisedAPInvoiceDetailsAndIsPostedCorrectly(cost, true, false);
			AssertForConsoCostAndChargeAfterChangingInvoiceDetails(cost.ApportionmentCharges[0], cost);
			//Creating Invoices
			var invoice1 = creator.CreateInvoice(typeof(APInvoice), creator.AUD, 1.0m, creator.AALSHI) as APInvoice;
			invoice1.AH_TransactionNum = "ABC123";
			var apline1 = creator.CreateAPInvoiceLine(invoice1, cost.ApportionmentCharges[0].Job as Job, cost.ChargeCode, creator.AUD, 1m, "Test Line 1", 100m);
			var invoice2 = creator.CreateInvoice(typeof(APInvoice), creator.AUD, 1.0m, creator.AALSHI) as APInvoice;
			invoice2.AH_TransactionNum = "INV0002";
			var apline2 = creator.CreateAPInvoiceLine(invoice2, cost.ApportionmentCharges[1].Job as Job, cost.ChargeCode, creator.AUD, 1m, "Test Line 2", 110m);
			//Partial Posting :one apportioned charge is not linked to any Invoice
			cost.E6_AH_APInvoice = invoice1.PK;
			cost.ApportionmentCharges[0].JR_AL_APLine = invoice1.Lines[0].PK;
			AssertHasSynchronisedAPInvoiceDetailsAndIsPostedCorrectly(cost, false, false);
			//Charges are Linked to different Invoices
			cost.E6_AH_APInvoice = invoice1.PK;
			cost.ApportionmentCharges[0].JR_AL_APLine = invoice1.Lines[0].PK;
			cost.ApportionmentCharges[1].JR_AL_APLine = invoice2.Lines[0].PK;
			AssertHasSynchronisedAPInvoiceDetailsAndIsPostedCorrectly(cost, false, false);
			//Cost is linked to different Invoice
			cost = CreateConsolCost(apportionmentListing);
			cost.E6_AH_APInvoice = invoice1.PK;
			cost.ApportionmentCharges[0].JR_AL_APLine = invoice2.Lines[0].PK;
			cost.ApportionmentCharges[1].JR_AL_APLine = invoice2.Lines[0].PK;
			AssertHasSynchronisedAPInvoiceDetailsAndIsPostedCorrectly(cost, false, false);
			//Cost and charges are linked to correct APInvoice with sync Information
			cost = CreateConsolCost(apportionmentListing);
			var invoice3 = creator.CreateInvoice(typeof(APInvoice), creator.AUD, 1.0m, creator.AALSHI) as APInvoice;
			invoice3.AH_TransactionNum = "ABC123";
			invoice3.AH_OH = creator.AALSHI.PK;
			invoice3.AH_InvoiceDate = ZDateTime.Today;
			invoice3.AH_DueDate = ZDateTime.Today.AddDays(2);
			invoice3.AH_TransactionReference = "COST";
			apline1 = creator.CreateAPInvoiceLine(invoice3, cost.ApportionmentCharges[0].Job as Job, cost.ChargeCode, creator.AUD, 1m, "Test Line 1", 100m);
			apline1.AL_AT = cost.ApportionmentCharges[0].JR_AT_CostGSTRate;
			apline1.AL_A9_VATClass = cost.ApportionmentCharges[0].JR_A9_CostVATClass;
			apline2 = creator.CreateAPInvoiceLine(invoice3, cost.ApportionmentCharges[1].Job as Job, cost.ChargeCode, creator.AUD, 1m, "Test Line 2", 110m);
			apline2.AL_AT = cost.ApportionmentCharges[1].JR_AT_CostGSTRate;
			apline2.AL_A9_VATClass = cost.ApportionmentCharges[1].JR_A9_CostVATClass;
			cost.E6_AH_APInvoice = invoice3.PK;
			cost.E6_TaxDate = ZDate.Today;
			cost.ApportionmentCharges[0].JR_AL_APLine = invoice3.Lines[0].PK;
			cost.ApportionmentCharges[1].JR_AL_APLine = invoice3.Lines[1].PK;
			AssertHasSynchronisedAPInvoiceDetailsAndIsPostedCorrectly(cost, true, true);
			// Now Change Invoice Details Information
			AssertForConsoCostAndChargeAfterChangingInvoiceDetails(cost.ApportionmentCharges[0], cost);
			AssertForAPInvoiceAndChargeAfterChangingInvoiceDetails(cost.ApportionmentCharges[0], cost);
		}

		public void TestSetShipmentInfoWhenPrepareForPosting()
		{
			var consol = TestObjectCreator.CreateConsol();
			Factory.Save();

			var shipment1 = TestObjectCreator.CreateShipment("S001", consol);
			var shipment2 = TestObjectCreator.CreateShipment("S002", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			var job2 = TestObjectCreator.CreateJob(shipment2, false);

			var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100, TestObjectCreator.Creditor1);

			var apportionmentCharges = cost.ApportionmentCharges.Cast<ApportionSplitCharge>();

			AssertEquals("Precondition", 2, apportionmentCharges.Count());
			Assert("Precondition", apportionmentCharges.All(x => x.ShipmentInfo != null));

			cost.PrepareForPosting();

			AssertEquals("After Calling PrepareForPosting", 2, apportionmentCharges.Count());
			Assert("Should Have ShipmentInfo After Calling PrepareForPosting", apportionmentCharges.All(x => x.ShipmentInfo != null));
			Assert("IsUsedForApportionment Should Be True If They Have ShipmentInfo", apportionmentCharges.All(x => x.JR_IsUsedForApportionment));
		}

		public void TestIsPostedCorrectlyWithJobRevenueJournal()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());
			var creator = new TestObjectCreator(Factory);
			//Creating Consol
			var consol = creator.CreateConsol("", "", "TSTCNSL");
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUMEL";
			CommonShipment shipment1 = creator.CreateShipment("S00010001", "USLAX", "AUMEL");
			consol.Shipments.Add(shipment1);
			Job job1 = new Job.Loader(Factory, shipment1).TryCreateWithoutMutexForTestOnly();
			CommonShipment shipment2 = creator.CreateShipment("S00010002", "USLAX", "AUMEL");
			consol.Shipments.Add(shipment2);
			Job job2 = new Job.Loader(Factory, shipment2).TryCreateWithoutMutexForTestOnly();
			Factory.Save();
			//Creating Consol Cost
			var apportionmentListing = consol.GetApportionments();
			var cost = CreateConsolCost(apportionmentListing);
			cost.E6_OH_Creditor = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();
			AssertEquals("IsPosted", false, cost.IsPosted);
			AssertEquals("IsPostedCorrectly", false, cost.IsPostedCorrectly);
			AssertEquals("One Charge", 1, job1.Charges.Count);
			var charge1 = job1.Charges[0];
			charge1.JR_JH_InternalJob = job1.PK;
			charge1.JR_GB_InternalBranch = creator.NonCurrentBranch.PK;
			charge1.JR_GE_InternalDept = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();
			var journal = Factory.Load<JobRevenueJournal>(charge1.APLine.AL_AH);
			AssertNotNull("Job Revenue Journal", journal);
			AssertEquals("Auto Job Revenue Journal", true, journal.IsAutoJRJ);
			AssertEquals("IsPosted", true, cost.IsPosted);
			AssertEquals("IsPostedCorrectly", true, cost.IsPostedCorrectly);
			journal.AH_TransactionCategory = "";
			AssertEquals("Auto Job Revenue Journal", false, journal.IsAutoJRJ);
			AssertEquals("IsPosted", true, cost.IsPosted);
			AssertEquals("IsPostedCorrectly", true, cost.IsPostedCorrectly);
		}

		public void TestContainerServiceSelectionAsDefaultApportionmentFilter()
		{
			var chargeCodeCLN = ObjectCreator.CreateChargeCode("CL1", "Cleaning", Constants.ChargeType.Margin, 50m, ObjectCreator.GST1, null, "FEA");
			chargeCodeCLN.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			chargeCodeCLN.AC_ChargeSubGroup = Constants.FreightServiceType.Codes.Cleaning;
			var chargeCodeFFM = ObjectCreator.CreateChargeCode("FFM", "Fumigation", Constants.ChargeType.Margin, 50m, ObjectCreator.GST1, null, "FEA");
			chargeCodeFFM.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			chargeCodeFFM.AC_ChargeSubGroup = Constants.FreightServiceType.Codes.Fumigation;
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			var shipment1 = AddShipmentToConsolWithJob(consol);
			var shipment2 = AddShipmentToConsolWithJob(consol);
			var shipment3 = AddShipmentToConsolWithJob(consol);
			Factory.Save();
			var container1 = Factory.NewWithValidTestData<ForwardingContainer>();
			var container2 = Factory.NewWithValidTestData<ForwardingContainer>();
			var container3 = Factory.NewWithValidTestData<ForwardingContainer>();
			consol.Containers.Add(container1);
			consol.Containers.Add(container2);
			consol.Containers.Add(container3);
			PackLine packLine1 = shipment1.OuterPackLines.AddNew();
			packLine1.Containers.RemoveAll();
			packLine1.Containers.Add(container1);
			PackLine packLine2 = shipment2.OuterPackLines.AddNew();
			packLine2.Containers.RemoveAll();
			packLine2.Containers.Add(container2);
			PackLine packLine3 = shipment3.OuterPackLines.AddNew();
			packLine3.Containers.RemoveAll();
			packLine3.Containers.Add(container3);
			var service1 = container1.Services.AddNew();
			service1.ES_ServiceCode = Constants.FreightServiceType.Codes.Cleaning;
			var service2 = container2.Services.AddNew();
			service2.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			ApportionmentListing listing = new ApportionmentListing(Factory, consol);
			//First Cost
			JobConsolCost consolCost = listing.CostsCollection.TryAddNew();
			consolCost.E6_AC_ChargeCode = chargeCodeFFM.PK;
			AssertEquals("Apportionment Filter of Cost 1", "CTS", consolCost.E6_PPDCLT);
			AssertFilterForApportionCharges(consolCost, 3, shipment2.PK);
			//Second Cost
			consolCost = listing.CostsCollection.TryAddNew();
			consolCost.E6_AC_ChargeCode = chargeCodeCLN.PK;
			AssertEquals("Apportionment Filter of Cost 2", "CTS", consolCost.E6_PPDCLT);
			AssertFilterForApportionCharges(consolCost, 3, shipment1.PK);
			//Changing Charge Code of 2nd cost to check whether it triggers Apportionment Filter change
			consolCost.E6_AC_ChargeCode = ObjectCreator.CC2.PK;
			AssertEquals("Apportionment Filter of Cost 2", "ALL", consolCost.E6_PPDCLT);
			AssertFilterForApportionCharges(consolCost, 3, shipment1.PK, shipment2.PK, shipment3.PK);
			//Changing back
			consolCost.E6_AC_ChargeCode = chargeCodeCLN.PK;
			AssertEquals("Apportionment Filter of Cost 2", "CTS", consolCost.E6_PPDCLT);
			AssertFilterForApportionCharges(consolCost, 3, shipment1.PK);
			//Third Cost
			consolCost = listing.CostsCollection.TryAddNew();
			consolCost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			AssertEquals("Apportionment Filter of Cost 3", "ALL", consolCost.E6_PPDCLT);
			AssertFilterForApportionCharges(consolCost, 3, shipment1.PK, shipment2.PK, shipment3.PK);
			//Changing Apportionment Filter of third cost to check whether it throws an error as the charge codes's Acc Sub Group doesn't match any Container service
			consolCost.E6_PPDCLT = "CTS";
			AssertEquals("Should not allow CTS filter", true, consolCost.E6_PPDCLTInfo.HasError("CTS Apportionment filter cannot be applied on this cost, as selected charge code cannot be mapped to any container service."));
		}

		void AssertFilterForApportionCharges(JobConsolCost cost, int expectedNumberofTotalApportionedCharge, params ZGuid[] shipmentPKs)
		{
			AssertEquals("Number of Charges", expectedNumberofTotalApportionedCharge, cost.ApportionmentCharges.Count);
			AssertEquals("Number of Charges used for Apportionment", shipmentPKs.Length, cost.ApportionmentCharges.Cast<ApportionSplitCharge>().Count(x => x.JR_IsUsedForApportionment));
			foreach (ZGuid shipmentPK in shipmentPKs)
			{
				AssertEquals("Charge used for Apportionment", true, cost.ApportionmentCharges.Cast<ApportionSplitCharge>().Any(x => x.ShipmentInfo.PK == shipmentPK && x.JR_IsUsedForApportionment));
			}
		}

		public void TestManuallyApportionSmallAmountConsolCostWithForeignCurrency()
		{
			var consol = Factory.New<ForwardingConsol>();
			var weights = new[] { 1, 2, 4, 3 };
			for (int i = 0; i < 4; i++)
			{
				var shipment = consol.Shipments.AddNew();
				shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
				shipment.JS_ActualWeight = weights[i];
			}

			Factory.Save();
			var listing = new ApportionmentListing(Factory, consol);
			try
			{
				var consolCost = listing.CostsCollection.TryAddNew();
				consolCost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
				consolCost.CostExchangeRate.Currency = Constants.CurrencyCodes.UnitedStates;
				consolCost.CostExchangeRate.Rate = 0.5m;

				//Manual
				consolCost.E6_ApportionmentMethod = AllocationMethod.Manual;
				consolCost.ApportionmentCharges[0].JR_OSCostAmt = 0.03m;
				consolCost.ApportionmentCharges[1].JR_OSCostAmt = -0.01m;
				consolCost.ApportionmentCharges[2].JR_OSCostAmt = -0.01m;
				consolCost.ApportionmentCharges[3].JR_OSCostAmt = 0m;
				consolCost.E6_OSCostAmount = 0.01m;
				AssertEquals(consol.Shipments.Count, consolCost.ApportionmentCharges.Count);
				AssertEquals("Shouldn't be changed when chooses Manual method.", 0.03m, consolCost.ApportionmentCharges[0].JR_OSCostAmt);
				AssertEquals("Shouldn't be changed when chooses Manual method.", -0.01m, consolCost.ApportionmentCharges[1].JR_OSCostAmt);
				AssertEquals("Shouldn't be changed when chooses Manual method.", -0.01m, consolCost.ApportionmentCharges[2].JR_OSCostAmt);
				AssertEquals("Shouldn't be changed when chooses Manual method.", 0m, consolCost.ApportionmentCharges[3].JR_OSCostAmt);
				//Shipment
				consolCost.E6_ApportionmentMethod = AllocationMethod.GrossWeight;
				AssertEquals(consol.Shipments.Count, consolCost.ApportionmentCharges.Count);
				AssertEquals("Should be changed when chooses Shipment method.", 0m, consolCost.ApportionmentCharges[0].JR_OSCostAmt);
				AssertEquals("Should be changed when chooses Shipment method.", 0m, consolCost.ApportionmentCharges[1].JR_OSCostAmt);
				AssertEquals("Should be changed when chooses Shipment method.", 0.01m, consolCost.ApportionmentCharges[2].JR_OSCostAmt);
				AssertEquals("Should be changed when chooses Shipment method.", 0m, consolCost.ApportionmentCharges[3].JR_OSCostAmt);
			}
			finally
			{
				listing.ReleaseMutexes();
			}
		}

		public void TestMatchWithTNFJournal()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var job = ObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var cost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, ObjectCreator.AALSHI);
			cost.E6_OSCostAmount = 250m;
			cost.ApportionmentCharges[0].JR_OSCostAmt = 250m;
			cost.E6_InvoiceNum = "T001";
			Factory.Save();

			AssertEquals("should not found any matched journal", ZString.Empty, cost.MatchedWithTNFJournalNum);
			var otherFactory1 = new BusinessObjectFactory();
			var journal1 = otherFactory1.NewWithValidTestData<APJournal>();
			journal1.AH_TransactionCategory = Constants.TransactionCategory.Codes.TransactionNotFound;
			journal1.AH_OH = ObjectCreator.AALSHI.PK;
			journal1.AH_ChequeOrReference = "T001";
			journal1.AH_InvoiceAmount = 200;
			journal1.AH_OutstandingAmount = 200;
			journal1.AH_OSTotal = 200;
			otherFactory1.Save();

			var costInOtherFactory1 = otherFactory1.Load<JobConsolCost>(cost.PK);
			AssertEquals("should found the matched journal", journal1.AH_TransactionNum, costInOtherFactory1.MatchedWithTNFJournalNum);
			costInOtherFactory1.E6_InvoiceNum = "I001";
			AssertEquals("should not found any matched journal", ZString.Empty, costInOtherFactory1.MatchedWithTNFJournalNum);
			costInOtherFactory1.E6_InvoiceNum = "T001";

			journal1.AH_ChequeOrReference = "T002";
			var journal2 = otherFactory1.NewWithValidTestData<APJournal>();
			journal2.AH_TransactionCategory = Constants.TransactionCategory.Codes.TransactionNotFound;
			journal2.AH_OH = ObjectCreator.AALSHI.PK;
			journal2.AH_ChequeOrReference = "T001";
			journal2.AH_InvoiceAmount = 0;
			journal2.AH_OutstandingAmount = 0;
			journal2.AH_OSTotal = 0;
			otherFactory1.Save();
			var otherFactory2 = new BusinessObjectFactory();
			var costInOtherFactory2 = otherFactory2.Load<JobConsolCost>(cost.PK);
			AssertEquals("should not found any matched journal", ZString.Empty, costInOtherFactory2.MatchedWithTNFJournalNum);
		}

		public void TestDeveloperExceptionIsReportedWhenSettingParentDirectly()
		{
			var creator = new TestObjectCreator(Factory);
			//Creating Consol
			var consol = creator.CreateConsol("", "", "TSTCNSL");
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUMEL";
			CommonShipment shipment1 = creator.CreateShipment("S00010001", "USLAX", "AUMEL");
			consol.Shipments.Add(shipment1);
			Job job1 = new Job.Loader(Factory, shipment1).TryCreateWithoutMutexForTestOnly();
			CommonShipment shipment2 = creator.CreateShipment("S00010002", "USLAX", "AUMEL");
			consol.Shipments.Add(shipment2);
			Job job2 = new Job.Loader(Factory, shipment2).TryCreateWithoutMutexForTestOnly();
			Factory.Save();
			//Creating Consol Cost
			var apportionmentListing = consol.GetApportionments();
			var cost = CreateConsolCost(apportionmentListing);
			cost.E6_OH_Creditor = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();
			// Create standalone Consol Cost
			var newCost = consol.GetApportionments().CostsCollection.TryAddNew();
			var reportMessage = @"Use JobConsolCostCollection.TryAddNew() method to add a new JobConsolCost to a Consol.
Use APInvoiceConsolCostCollection.TryAddNewForConsol(Consol) method to add a new Consol Cost.
They will check if adding Consol Cost is allowed by Consol and return null when it is not (e.g. Gateway Consol).
The JobConsolCost.ReportSettingParentSuspender can be used to wrap later setting of E6_ParentID and E6_ParentTablecode in cases when JobConsolCost parent is not set at its creation.
Setting BusinessContext.EnableDirectSettingConsolCostParent on the Consol BusinessObject level should be used to allow adding new JobConsolCost to JobConsolCostCollection in GUI.
Setting same BusinessContext on AP Invoice should be used for GUI when we have Consol Costs linked to AP Invoice in GUI.
Make sure there is a validation to prevent saving illegitimate Consol Costs for Gateway Consols.";
			ErrorReporter.Clear();
			newCost.E6_ParentTableCode = consol.TablePrefix;
			AssertStartsWith("Report Message", reportMessage, ErrorReporter.LastMessageReported);
			AssertContains("Call Stack", "ReportDeveloperExceptionIfSettingParentWithoutSuspender", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
			newCost.E6_ParentID = consol.PK;
			AssertStartsWith("Report Message", reportMessage, ErrorReporter.LastMessageReported);
			AssertContains("Call Stack", "ReportDeveloperExceptionIfSettingParentWithoutSuspender", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
			newCost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, consol.TablePrefix);
			AssertStartsWith("Report Message", reportMessage, ErrorReporter.LastMessageReported);
			AssertContains("Call Stack", "ReportDeveloperExceptionIfSettingParentWithoutSuspender", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestTaxInclusiveAmountsOnConsolCost()
		{
			var consol = Factory.New<ForwardingConsol>();
			var invoice = Factory.New<APInvoice>();
			invoice.GSTInclusiveAmounts = true;
			var testCost = invoice.ConsolCosting.ConsolCosts.AddNew();
			testCost.E6_ParentID = consol.PK;
			testCost.E6_ParentTableCode = "JK";
			testCost.E6_RX_NKCurrency = ObjectCreator.USD.RX_Code;
			testCost.E6_ExchangeRate = 0.7125M;
			testCost.GSTInclusiveAmount = 500M;
			testCost.E6_AT_TaxRate = ObjectCreator.GST1WithDates.PK;
			AssertEquals("E6_OSCostAmount", 454.55M, testCost.E6_OSCostAmount);
			AssertEquals("E6_OSGSTAmount_Calc", 45.45M, testCost.E6_OSGSTAmount_Calc);

			testCost.E6_TaxDate = ObjectCreator.GST1WithDates_DateWithNoRate;
			AssertEquals("E6_OSCostAmount", 500M, testCost.E6_OSCostAmount);
			AssertEquals("E6_OSGSTAmount_Calc", 0m, testCost.E6_OSGSTAmount_Calc);
		}

		public void TestValidationIsRunWhenApporitonSplitChargeIsChangedByTheDataRefreshBus()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "Master1";
			shipment.JS_ActualWeight = 150m;
			shipment.JS_ActualChargeable = 100m;
			var job = ObjectCreator.CreateJob(shipment, ObjectCreator.LocalClient, 0M, ObjectCreator.Agent, 0M);
			Factory.Save();
			var cost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, ObjectCreator.Creditor1);
			cost.E6_RX_NKCurrency = "USD";
			cost.E6_OSCostAmount = 3756M;
			cost.E6_LocalCostAmount = 5165.73M;
			cost.E6_ExchangeRate = 0.7271M;
			foreach (ApportionSplitCharge charge in cost.ApportionmentCharges)
			{
				charge.JR_IsUsedForApportionment = true;
			}

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var reloadedJob = newFactory.Load<Job>(job.PK);
			reloadedJob.Charges[0].JR_LocalSellAmt = 45.20M;
			cost.E6_LocalCostAmount = 5165.69M;
			cost.ApportionmentCharges[0].JR_IsUsedForApportionment = true;
			newFactory.Save();
			cost.RunPreSaveValidation();
			AssertHasRowError("Data Refresh Bus update is skipped because apportionment charge has changes.", cost.ApportionmentCharges[0], "This record was modified by this user during another operation. Please cancel your changes and reload the form.");
			Assert(cost.ApportionmentCharges[0].HasContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange));
		}

		public void TestE6_CostGovtChargeCodeReadOnly()
		{
			var consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			consol.Shipments.AddNew();
			var cost = GetCost(consol);
			try
			{
				var securityCheckPoint = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainConsolJobInvoicing, SecurityCore.ConsolInvAllowOverrideCostGovtCrgCode);
				securityCheckPoint.IsAllowed = false;
				AssertEquals("E6_CostGovtChargeCodeInfo.ReadOnly", true, cost.E6_CostGovtChargeCodeInfo.ReadOnly);
				securityCheckPoint.IsAllowed = true;
				AssertEquals("E6_CostGovtChargeCodeInfo.ReadOnly", false, cost.E6_CostGovtChargeCodeInfo.ReadOnly);
			}
			finally
			{
				cost.CalculationStrategy.ReleaseMutexes();
			}
		}

		public void TestE6_SellGovtChargeCodeReadOnly()
		{
			var consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			consol.Shipments.AddNew();
			var cost = GetCost(consol);
			try
			{
				var securityCheckPoint = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainConsolJobInvoicing, SecurityCore.ConsolInvAllowOverrideSellGovtCrgCode);
				securityCheckPoint.IsAllowed = false;
				AssertEquals("E6_SellGovtChargeCodeInfo.ReadOnly", true, cost.E6_SellGovtChargeCodeInfo.ReadOnly);
				securityCheckPoint.IsAllowed = true;
				AssertEquals("E6_SellGovtChargeCodeInfo.ReadOnly", false, cost.E6_SellGovtChargeCodeInfo.ReadOnly);
			}
			finally
			{
				cost.CalculationStrategy.ReleaseMutexes();
			}
		}

		public void TestGovtChargeCodeIsSameInBothConsolCostAndApportionmentCharge()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			Factory.Save();
			foreach (var enableGovtChargeCode in new[] { false, true })
			{
				using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, enableGovtChargeCode))
				{
					var listing = new ApportionmentListing(Factory, consol);
					var charge = Factory.NewWithValidTestData<AccChargeCode>();
					charge.AC_GovtChargeCode = "GVTCC1";
					Factory.Save();
					var consolCost = listing.CostsCollection.TryAddNew();
					consolCost.E6_OSCostAmount = 200m;
					consolCost.E6_ApportionmentMethod = AllocationMethod.Manual;
					consolCost.ApportionmentCharges[0].JR_OSCostAmt = 100m;
					consolCost.ApportionmentCharges[1].JR_OSCostAmt = 100m;
					AssertEquals("Before: E6_CostGovtChargeCode.", string.Empty, consolCost.E6_CostGovtChargeCode);
					AssertEquals("Before: First JR_CostGovtChargeCode.", string.Empty, consolCost.ApportionmentCharges[0].JR_CostGovtChargeCode);
					AssertEquals("Before: Second JR_CostGovtChargeCode.", string.Empty, consolCost.ApportionmentCharges[1].JR_CostGovtChargeCode);
					AssertEquals("Before: E6_SellGovtChargeCode.", string.Empty, consolCost.E6_SellGovtChargeCode);
					AssertEquals("Before: First JR_SellGovtChargeCode.", string.Empty, consolCost.ApportionmentCharges[0].JR_SellGovtChargeCode);
					AssertEquals("Before: Second JR_SellGovtChargeCode.", string.Empty, consolCost.ApportionmentCharges[1].JR_SellGovtChargeCode);
					consolCost.E6_AC_ChargeCode = charge.PK;
					AssertEquals("After: E6_CostGovtChargeCode.", enableGovtChargeCode ? "GVTCC1" : string.Empty, consolCost.E6_CostGovtChargeCode);
					AssertEquals("After: First JR_CostGovtChargeCode.", enableGovtChargeCode ? "GVTCC1" : string.Empty, consolCost.ApportionmentCharges[0].JR_CostGovtChargeCode);
					AssertEquals("After: Second JR_CostGovtChargeCode.", enableGovtChargeCode ? "GVTCC1" : string.Empty, consolCost.ApportionmentCharges[1].JR_CostGovtChargeCode);
					AssertEquals("After: E6_SellGovtChargeCode.", enableGovtChargeCode ? "GVTCC1" : string.Empty, consolCost.E6_SellGovtChargeCode);
					AssertEquals("After: First JR_SellGovtChargeCode.", enableGovtChargeCode ? "GVTCC1" : string.Empty, consolCost.ApportionmentCharges[0].JR_SellGovtChargeCode);
					AssertEquals("After: Second JR_SellGovtChargeCode.", enableGovtChargeCode ? "GVTCC1" : string.Empty, consolCost.ApportionmentCharges[1].JR_SellGovtChargeCode);
				}
			}
		}

		public void TestGovtChargeCodeIsSavedInDBFromApportionmentCharges()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			Factory.Save();
			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var listing = new ApportionmentListing(Factory, consol);
				try
				{
					var charge = Factory.NewWithValidTestData<AccChargeCode>();
					charge.AC_GovtChargeCode = "GVTCC1";
					Factory.Save();
					var consolCost = listing.CostsCollection.TryAddNew();
					consolCost.E6_OSCostAmount = 200m;
					consolCost.E6_ApportionmentMethod = AllocationMethod.Manual;
					consolCost.ApportionmentCharges[0].JR_OSCostAmt = 100m;
					consolCost.ApportionmentCharges[1].JR_OSCostAmt = 100m;
					AssertEquals("Before: E6_CostGovtChargeCode.", string.Empty, consolCost.E6_CostGovtChargeCode);
					AssertEquals("Before: First JR_CostGovtChargeCode.", string.Empty, consolCost.ApportionmentCharges[0].JR_CostGovtChargeCode);
					AssertEquals("Before: Second JR_CostGovtChargeCode.", string.Empty, consolCost.ApportionmentCharges[1].JR_CostGovtChargeCode);
					consolCost.E6_AC_ChargeCode = charge.PK;
					AssertEquals("After: E6_CostGovtChargeCode.", "GVTCC1", consolCost.E6_CostGovtChargeCode);
					AssertEquals("After: E6_SellGovtChargeCode.", "GVTCC1", consolCost.E6_SellGovtChargeCode);
					AssertEquals("After: First JR_CostGovtChargeCode.", "GVTCC1", consolCost.ApportionmentCharges[0].JR_CostGovtChargeCode);
					AssertEquals("After: First JR_SellGovtChargeCode.", "GVTCC1", consolCost.ApportionmentCharges[0].JR_SellGovtChargeCode);
					AssertEquals("After: Second JR_CostGovtChargeCode.", "GVTCC1", consolCost.ApportionmentCharges[1].JR_CostGovtChargeCode);
					AssertEquals("After: Second JR_SellGovtChargeCode.", "GVTCC1", consolCost.ApportionmentCharges[1].JR_SellGovtChargeCode);
					AssertEquals("Before Saving: Job1 No Charges.", 0, (shipment1.Job as Job).Charges.Count);
					AssertEquals("Before Saving: Job2 No Charges.", 0, (shipment2.Job as Job).Charges.Count);
					Factory.Save();
					AssertEquals("After Saving: Job1.JR_CostGovtChargeCode", "GVTCC1", (shipment1.Job as Job).Charges[0].JR_CostGovtChargeCode);
					AssertEquals("After Saving: Job1.JR_SellGovtChargeCode", "GVTCC1", (shipment1.Job as Job).Charges[0].JR_SellGovtChargeCode);
					AssertEquals("After Saving: Job2.JR_CostGovtChargeCode", "GVTCC1", (shipment2.Job as Job).Charges[0].JR_CostGovtChargeCode);
					AssertEquals("After Saving: Job2.JR_SellGovtChargeCode", "GVTCC1", (shipment2.Job as Job).Charges[0].JR_SellGovtChargeCode);
				}
				finally
				{
					listing.ReleaseMutexes();
				}
			}
		}

		public void TestUpdateGovtChargeCode()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CC1.AC_GovtChargeCode = "DefaultGovt";
			testObjectCreator.SetupOrCreateGovtChargeCodeOverride(testObjectCreator.CC1, "COS", "FCN", "ALL", "ALL", "CodeForCost");
			testObjectCreator.SetupOrCreateGovtChargeCodeOverride(testObjectCreator.CC1, "REV", "FCN", "ALL", "ALL", "CodeForRevenue");
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			var testObj = testObjectCreator.CreateConsolCost(consol, testObjectCreator.CC1);
			var shipment = consol.Shipments.AddNew();
			var testObjJob = new Job.Loader(shipment).TryLoadOrCreate();
			AssertWhenRegistryOn(testObj, testObjJob);

			AssertWhenRegistryOff(testObj, testObjJob);

			void AssertWhenRegistryOn(JobConsolCost consolCost, Job shipmentJob)
			{
				AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);

				consolCost.E6_AC_ChargeCode = ZGuid.Empty;
				consolCost.E6_CostGovtChargeCode = "12345";
				consolCost.E6_SellGovtChargeCode = "54321";
				var apportionmentCharge = Factory.NewWithValidTestData<ApportionSplitCharge>();
				apportionmentCharge.JR_JH = shipmentJob.PK;
				apportionmentCharge.JR_AC = testObjectCreator.CC1.PK;

				AssertEquals("DefaultGovt", apportionmentCharge.JR_CostGovtChargeCode);
				AssertEquals("DefaultGovt", apportionmentCharge.JR_SellGovtChargeCode);
				consolCost.ApportionmentCharges.Add(apportionmentCharge);
				consolCost.UpdateGovtChargeCode();
				AssertEquals("DefaultGovt", consolCost.E6_CostGovtChargeCode);
				AssertEquals("DefaultGovt", consolCost.E6_CostGovtChargeCode);
				AssertEquals("DefaultGovt", apportionmentCharge.JR_CostGovtChargeCode);
				AssertEquals("DefaultGovt", apportionmentCharge.JR_SellGovtChargeCode);
				consolCost.ApportionmentCharges.RemoveAndDeleteAll();
				AssertEquals("12345", consolCost.E6_CostGovtChargeCode);
				AssertEquals("54321", consolCost.E6_SellGovtChargeCode);

				consolCost.E6_AC_ChargeCode = testObjectCreator.CC1.PK;
				consolCost.E6_CostGovtChargeCode = "12345";
				consolCost.E6_SellGovtChargeCode = "54321";
				var apportionmentCharge2 = Factory.NewWithValidTestData<ApportionSplitCharge>();
				apportionmentCharge2.JR_JH = shipmentJob.PK;
				apportionmentCharge2.JR_AC = testObjectCreator.CC1.PK;

				AssertEquals("DefaultGovt", apportionmentCharge2.JR_CostGovtChargeCode);
				AssertEquals("DefaultGovt", apportionmentCharge2.JR_SellGovtChargeCode);
				consolCost.ApportionmentCharges.Add(apportionmentCharge2);
				consolCost.UpdateGovtChargeCode();
				AssertEquals("CodeForCost", consolCost.E6_CostGovtChargeCode);
				AssertEquals("CodeForRevenue", consolCost.E6_SellGovtChargeCode);
				AssertEquals("CodeForCost", apportionmentCharge2.JR_CostGovtChargeCode);
				AssertEquals("CodeForRevenue", apportionmentCharge2.JR_SellGovtChargeCode);
				consolCost.ApportionmentCharges.RemoveAndDeleteAll();
				AssertEquals("CodeForCost", consolCost.E6_CostGovtChargeCode);
				AssertEquals("CodeForRevenue", consolCost.E6_SellGovtChargeCode);
			}

			void AssertWhenRegistryOff(JobConsolCost consolCost, Job shipmentJob)
			{
				AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);

				consolCost.E6_AC_ChargeCode = ZGuid.Empty;
				consolCost.E6_CostGovtChargeCode = "12345";
				consolCost.E6_SellGovtChargeCode = "54321";
				var apportionmentCharge = Factory.NewWithValidTestData<ApportionSplitCharge>();
				apportionmentCharge.JR_JH = shipmentJob.PK;
				apportionmentCharge.JR_AC = testObjectCreator.CC1.PK;

				AssertEquals(ZString.Empty, apportionmentCharge.JR_CostGovtChargeCode);
				AssertEquals(ZString.Empty, apportionmentCharge.JR_SellGovtChargeCode);
				consolCost.ApportionmentCharges.Add(apportionmentCharge);
				consolCost.UpdateGovtChargeCode();
				AssertEquals(ZString.Empty, consolCost.E6_CostGovtChargeCode);
				AssertEquals(ZString.Empty, consolCost.E6_CostGovtChargeCode);
				AssertEquals(ZString.Empty, apportionmentCharge.JR_CostGovtChargeCode);
				AssertEquals(ZString.Empty, apportionmentCharge.JR_SellGovtChargeCode);
				consolCost.ApportionmentCharges.RemoveAndDeleteAll();
				AssertEquals("12345", consolCost.E6_CostGovtChargeCode);
				AssertEquals("54321", consolCost.E6_SellGovtChargeCode);

				consolCost.E6_AC_ChargeCode = testObjectCreator.CC1.PK;
				consolCost.E6_CostGovtChargeCode = "12345";
				consolCost.E6_SellGovtChargeCode = "54321";
				var apportionmentCharge2 = Factory.NewWithValidTestData<ApportionSplitCharge>();
				apportionmentCharge2.JR_JH = shipmentJob.PK;
				apportionmentCharge2.JR_AC = testObjectCreator.CC1.PK;

				AssertEquals(ZString.Empty, apportionmentCharge2.JR_CostGovtChargeCode);
				AssertEquals(ZString.Empty, apportionmentCharge2.JR_SellGovtChargeCode);
				consolCost.ApportionmentCharges.Add(apportionmentCharge2);
				consolCost.UpdateGovtChargeCode();
				AssertEquals(ZString.Empty, consolCost.E6_CostGovtChargeCode);
				AssertEquals(ZString.Empty, consolCost.E6_SellGovtChargeCode);
				AssertEquals(ZString.Empty, apportionmentCharge2.JR_CostGovtChargeCode);
				AssertEquals(ZString.Empty, apportionmentCharge2.JR_SellGovtChargeCode);
				consolCost.ApportionmentCharges.RemoveAndDeleteAll();
				AssertEquals("12345", consolCost.E6_CostGovtChargeCode);
				AssertEquals("54321", consolCost.E6_SellGovtChargeCode);
			}
		}

		public void TestIsValidForGovtChargeCode()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var consolCost = testObjectCreator.CreateConsolCost(Factory.New<ForwardingConsol>(), testObjectCreator.CC1);

			AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);

			consolCost.E6_AC_ChargeCode = ZGuid.Empty;
			AssertEquals(false, consolCost.IsValidForGovtChargeCode);

			consolCost.E6_AC_ChargeCode = testObjectCreator.CC1.PK;
			AssertEquals(true, consolCost.IsValidForGovtChargeCode);

			AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);

			consolCost.E6_AC_ChargeCode = ZGuid.Empty;
			AssertEquals(false, consolCost.IsValidForGovtChargeCode);

			consolCost.E6_AC_ChargeCode = testObjectCreator.CC1.PK;
			AssertEquals(false, consolCost.IsValidForGovtChargeCode);
		}

		public void TestGetMatchedGovtChargeCode_Consol()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			testObjectCreator.CC1.GovtChargeCodeOverrides.RemoveAndDeleteAll();
			testObjectCreator.CC1.AC_GovtChargeCode = "GVTCC1_Default";
			testObjectCreator.SetupOrCreateGovtChargeCodeOverride(testObjectCreator.CC1, "COS", "FCN", "ALL", "ALL", "CodeForCost");
			testObjectCreator.SetupOrCreateGovtChargeCodeOverride(testObjectCreator.CC1, "REV", "FCN", "ALL", "ALL", "CodeForRevenue");
			Factory.Save();

			AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);

			var consolCost = testObjectCreator.CreateConsolCost(Factory.New<ForwardingConsol>(), testObjectCreator.CC1);
			AssertEquals("CodeForCost", consolCost.GetMatchedGovtChargeCode(CostSell.Cost));
			AssertEquals("CodeForRevenue", consolCost.GetMatchedGovtChargeCode(CostSell.Revenue));

			consolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			consolCost.E6_ParentID = ZGuid.Empty;
			AssertEquals("GVTCC1_Default", consolCost.GetMatchedGovtChargeCode(CostSell.Cost));
			AssertEquals("GVTCC1_Default", consolCost.GetMatchedGovtChargeCode(CostSell.Revenue));

			consolCost.E6_AC_ChargeCode = ZGuid.Empty;
			AssertEquals(ZString.Empty, consolCost.GetMatchedGovtChargeCode(CostSell.Cost));
			AssertEquals(ZString.Empty, consolCost.GetMatchedGovtChargeCode(CostSell.Revenue));

			consolCost.E6_AC_ChargeCode = testObjectCreator.CC1.PK;
			AssertEquals("GVTCC1_Default", consolCost.GetMatchedGovtChargeCode(CostSell.Cost));
			AssertEquals("GVTCC1_Default", consolCost.GetMatchedGovtChargeCode(CostSell.Revenue));

			AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);
			AssertEquals(ZString.Empty, consolCost.GetMatchedGovtChargeCode(CostSell.Cost));
			AssertEquals(ZString.Empty, consolCost.GetMatchedGovtChargeCode(CostSell.Revenue));
		}

		public void TestGetMatchedGovtChargeCode_GetwayConsol()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			testObjectCreator.CC1.GovtChargeCodeOverrides.RemoveAndDeleteAll();
			testObjectCreator.CC1.AC_GovtChargeCode = "GVTCC1_Default";
			testObjectCreator.SetupOrCreateGovtChargeCodeOverride(testObjectCreator.CC1, "COS", "FCN", "ALL", "ALL", "CodeForCost");
			testObjectCreator.SetupOrCreateGovtChargeCodeOverride(testObjectCreator.CC1, "REV", "FCN", "ALL", "ALL", "CodeForRevenue");
			testObjectCreator.SetupOrCreateGovtChargeCodeOverride(testObjectCreator.CC1, "ALL", "SHP", "ALL", "ALL", "66666666");
			testObjectCreator.SetupOrCreateGovtChargeCodeOverride(testObjectCreator.CC1, "ALL", "GCN", "ALL", "ALL", "55555555");
			Factory.Save();

			var gatewayConsol = Factory.New<ForwardingConsol>();
			gatewayConsol.JK_UniqueConsignRef = "C00001";
			gatewayConsol.JK_AgentType = Constants.AgentType.Agent;
			gatewayConsol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			gatewayConsol.JK_RL_NKLoadPort = "USLAX";
			gatewayConsol.JK_RL_NKDischargePort = "AUMEL";
			gatewayConsol.JK_TransportMode = "AIR";
			gatewayConsol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;

			var gatewayAgentPort = gatewayConsol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			gatewayAgentPort.O5_OA_AgentOfficeAddress = gatewayConsol.JK_OA_SendingForwarderAddress;
			gatewayAgentPort.O5_PortOrCountry = "USLAX";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;

			AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);

			var consolCostGetway = testObjectCreator.CreateConsolCost(gatewayConsol, testObjectCreator.CC1);
			AssertEquals(false, consolCostGetway.IsGatewayConsolCost);
			AssertEquals("CodeForCost", consolCostGetway.GetMatchedGovtChargeCode(CostSell.Cost));
			AssertEquals("CodeForRevenue", consolCostGetway.GetMatchedGovtChargeCode(CostSell.Revenue));

			gatewayConsol.SendingForwarder.AppointedGatewayAgentPorts.Add(gatewayAgentPort);
			var apportionmentCharge = consolCostGetway.ApportionmentCharges.AddNew();
			apportionmentCharge.JR_E6_GatewaySellHeader = consolCostGetway.PK;
			AssertEquals(true, consolCostGetway.IsGatewayConsolCost);
			AssertEquals("CodeForCost", consolCostGetway.GetMatchedGovtChargeCode(CostSell.Cost));
			AssertEquals("CodeForRevenue", consolCostGetway.GetMatchedGovtChargeCode(CostSell.Revenue));

			consolCostGetway.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			consolCostGetway.E6_ParentID = ZGuid.Empty;
			AssertEquals("GVTCC1_Default", consolCostGetway.GetMatchedGovtChargeCode(CostSell.Cost));
			AssertEquals("GVTCC1_Default", consolCostGetway.GetMatchedGovtChargeCode(CostSell.Revenue));

			consolCostGetway.E6_AC_ChargeCode = ZGuid.Empty;
			AssertEquals(ZString.Empty, consolCostGetway.GetMatchedGovtChargeCode(CostSell.Cost));
			AssertEquals(ZString.Empty, consolCostGetway.GetMatchedGovtChargeCode(CostSell.Revenue));

			consolCostGetway.E6_AC_ChargeCode = testObjectCreator.CC1.PK;
			AssertEquals("GVTCC1_Default", consolCostGetway.GetMatchedGovtChargeCode(CostSell.Cost));
			AssertEquals("GVTCC1_Default", consolCostGetway.GetMatchedGovtChargeCode(CostSell.Revenue));

			AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);
			AssertEquals(ZString.Empty, consolCostGetway.GetMatchedGovtChargeCode(CostSell.Cost));
			AssertEquals(ZString.Empty, consolCostGetway.GetMatchedGovtChargeCode(CostSell.Revenue));
		}

		public void TestLightValidationIsDisabledWhenEnableLightValidationForChargeAndConsolCostRegistryIsFalse()
		{
			AssertLightValidationResultForCharge(false);
		}

		public void TestLightValidationIsEnabledWhenEnableLightValidationForChargeAndConsolCostRegistryIsTrue()
		{
			AssertLightValidationResultForCharge(true);
		}

		public void TestDBHitsForEnableLightValidationForChargeAndConsolCostRegistry()
		{
			var consolCost = (JobConsolCost)GetNewBusinessObject();
			var dbHitsBefore = Db.Connection.ExecutedCommandCount;
			var result = consolCost.EnableLightValidationIfAvailable_ForTestOnly();
			var dbHitsAfter = Db.Connection.ExecutedCommandCount;
			AssertEquals(dbHitsAfter, dbHitsBefore + 1);
			result = consolCost.EnableLightValidationIfAvailable_ForTestOnly();
			dbHitsAfter = Db.Connection.ExecutedCommandCount;
			AssertEquals(dbHitsAfter, dbHitsBefore + 1);
		}

		void AssertLightValidationResultForCharge(bool shouldEnableLightValidation)
		{
			AccountingConfigurationRegistry.Instance.EnableLightValidationForChargeAndConsolCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, shouldEnableLightValidation);
			var creditor = ObjectCreator.CreateOrgHeader("CROWN", true, false, "AUSYD", false);
			Factory.Save();
			var consol = ObjectCreator.CreateConsol("AUSYD", "AUMEL", "C001", false, false);
			var shipment = ObjectCreator.CreateShipment("S001", consol);
			var apportionmentListing = new ApportionmentListing(Factory, consol);
			try
			{
				var consolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, creditor, apportionmentListing);
				consolCost.E6_OSCostAmount = 10m;
				consolCost.RunPreSaveValidation();
				Assert(!consolCost.HasErrors);
				var anotherUserFactory = new BusinessObjectFactory()
				{ RefreshEnabled = false };
				var reloadedCreditor = anotherUserFactory.Load<OrgHeader>(creditor.PK);
				reloadedCreditor.OH_IsCreditor = false;
				anotherUserFactory.Save();
				Assert(!reloadedCreditor.OH_IsCreditor);
				consolCost.RunPreSaveValidation();
				AssertEquals(!shouldEnableLightValidation, consolCost.Notifications.ContainsNotificationContaining("Enter a valid Creditor."));
			}
			finally
			{
				apportionmentListing.ReleaseMutexes();
			}
		}

		public void TestOrphanedAccrualIsDeletedAndCriticalExceptionNotThrownWhenUserTryToDeleteConsolCostAndSaveAfterConcurrencyError()
		{
			var consol = ObjectCreator.CreateConsol();
			var shipment = ObjectCreator.CreateShipment("S0001", consol);
			var consolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, ObjectCreator.Creditor1);
			consolCost.E6_OSCostAmount = 3756M;
			consolCost.ApportionmentCharges[0].JR_IsUsedForApportionment = true;
			Factory.Save();
			var apportionCharge = consolCost.ApportionmentCharges[0];
			var originalACR = apportionCharge.Accrual;
			var originalWIP = apportionCharge.WIP;
			var expectedConcurrencyMessageForFirstFactorySave = $@"
**CONCURRENCY Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Tablename: AccTransactionLines
PK: {originalACR.PK}
RowState: Modified";
			var expectedConcurrencyMessageForSecondFactorySave = $@"
**CONCURRENCY Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Tablename: JobCharge
PK: {apportionCharge.PK}
RowState: Deleted";
			var differentUserFactory = new BusinessObjectFactory()
			{ RefreshEnabled = false };
			var consolCostInDifferentUserFactory = differentUserFactory.Load<JobConsolCost>(consolCost.PK);
			consolCostInDifferentUserFactory.ApportionmentCharges[0].JR_IsUsedForApportionment = true;
			consolCostInDifferentUserFactory.E6_LocalCostAmount = 4938M;
			consolCostInDifferentUserFactory.E6_OSCostAmount = 4938M;
			differentUserFactory.Save();
			consolCost.E6_OSCostAmount = 3534M;
			try
			{
				Factory.Save();
			}
			catch (ZSaveConcurrencyException concurrencyException)
			{
				AssertContains(expectedConcurrencyMessageForFirstFactorySave, concurrencyException.Message);
			}

			var orphanACR = apportionCharge.Accrual;
			AssertNotEquals(orphanACR, originalACR);
			Assert(originalACR.IsReversed);
			Assert(!orphanACR.IsReversed);
			Assert(!orphanACR.IsDeleted);
			AssertEquals("This concurrency scenario will not create an orphan WIP", originalWIP, apportionCharge.WIP);
			Assert(!originalWIP.IsReversed);
			consolCost.Delete();
			Assert("Charge is deleted because cost/sell is not posted and sell amount is not changed", apportionCharge.IsDeleted);
			Assert(!consolCost.ApportionmentCharges.Any());
			Assert(originalACR.IsReversed);
			Assert(originalWIP.IsReversed);
			Assert(orphanACR.IsDeleted);
			try
			{
				Factory.Save();
			}
			catch (ZSaveConcurrencyException concurrencyException)
			{
				AssertContains(expectedConcurrencyMessageForSecondFactorySave, concurrencyException.Message);
			}
		}

		public void TestOrphanedWIPAccrualIsDeletedAndCriticalExceptionNotThrownWhenUserTryToDeleteConsolCostAndSaveAfterConcurrencyError()
		{
			var consol = ObjectCreator.CreateConsol();
			var shipment = ObjectCreator.CreateShipment("S0001", consol);
			var consolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, ObjectCreator.Creditor1);
			consolCost.E6_OSCostAmount = 3756M;
			consolCost.ApportionmentCharges[0].JR_IsUsedForApportionment = true;
			Factory.Save();
			var apportionCharge = consolCost.ApportionmentCharges[0];
			var originalWIP = apportionCharge.WIP;
			var originalACR = apportionCharge.Accrual;
			var expectedConcurrencyMessageForFirstFactorySave = $@"
**CONCURRENCY Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Tablename: AccTransactionLines
PK: {originalACR.PK}
RowState: Modified";
			var expectedConcurrencyMessageForSecondFactorySave = $@"
**CONCURRENCY Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Tablename: JobCharge
PK: {apportionCharge.PK}
RowState: Deleted";
			var differentUserFactory = new BusinessObjectFactory()
			{ RefreshEnabled = false };
			var consolCostInDifferentUserFactory = differentUserFactory.Load<JobConsolCost>(consolCost.PK);
			var differentDepartment = differentUserFactory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.PK, SQLComparisonOperator.NotEqual, apportionCharge.JR_GE));
			consolCostInDifferentUserFactory.ApportionmentCharges[0].JR_GE = differentDepartment.PK;
			differentUserFactory.Save();
			apportionCharge.JR_GE = differentDepartment.PK;
			try
			{
				Factory.Save();
			}
			catch (ZSaveConcurrencyException concurrencyException)
			{
				AssertContains(expectedConcurrencyMessageForFirstFactorySave, concurrencyException.Message);
			}

			var orphanWIP = apportionCharge.WIP;
			AssertNotEquals(orphanWIP, originalWIP);
			Assert(originalWIP.IsReversed);
			Assert(!orphanWIP.IsReversed);
			Assert(!orphanWIP.IsDeleted);
			var orphanACR = apportionCharge.Accrual;
			AssertNotEquals(orphanACR, originalACR);
			Assert(originalACR.IsReversed);
			Assert(!orphanACR.IsReversed);
			Assert(!orphanACR.IsDeleted);
			consolCost.Delete();
			Assert("Charge is deleted because cost/sell is not posted and sell amount is not changed", apportionCharge.IsDeleted);
			Assert(!consolCost.ApportionmentCharges.Any());
			Assert(originalACR.IsReversed);
			Assert(originalWIP.IsReversed);
			Assert(orphanACR.IsDeleted);
			Assert(orphanWIP.IsDeleted);
			try
			{
				Factory.Save();
			}
			catch (ZSaveConcurrencyException concurrencyException)
			{
				AssertContains(expectedConcurrencyMessageForSecondFactorySave, concurrencyException.Message);
			}
		}

		public void TestTaxOverriddenFlagIsTrueForPostedConsolCost_WhenFlagWasNotTickedManually()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("", "", "TSTCNSL");
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUMEL";
			CommonShipment shipment1 = creator.CreateShipment("S00010001", "USLAX", "AUMEL");
			consol.Shipments.Add(shipment1);
			Job job1 = new Job.Loader(Factory, shipment1).TryCreateWithoutMutexForTestOnly();
			Factory.Save();

			var apportionmentListing = consol.GetApportionments();
			var cost = CreateConsolCost(apportionmentListing);
			AssertEquals("JR_IsCostTaxAmountOverridden", false, cost.ApportionmentCharges[0].JR_IsCostTaxAmountOverridden);
			AssertEquals("E6_IsTaxAmountOverridden", false, cost.E6_IsTaxAmountOverridden);

			var invoice = creator.CreateInvoice(typeof(APInvoice), creator.AUD, 1.0m, creator.AALSHI) as APInvoice;
			invoice.AH_TransactionNum = "ABC123";
			invoice.AH_OH = creator.AALSHI.PK;
			invoice.AH_InvoiceDate = ZDateTime.Today;
			invoice.AH_DueDate = ZDateTime.Today.AddDays(2);
			invoice.AH_TransactionReference = "COST";
			var apline1 = creator.CreateAPInvoiceLine(invoice, cost.ApportionmentCharges[0].Job as Job, cost.ChargeCode, creator.AUD, 1m, "Test Line 1", 100m);
			apline1.AL_AT = cost.ApportionmentCharges[0].JR_AT_CostGSTRate;
			apline1.AL_A9_VATClass = cost.ApportionmentCharges[0].JR_A9_CostVATClass;
			cost.E6_AH_APInvoice = invoice.PK;
			cost.ApportionmentCharges[0].JR_AL_APLine = invoice.Lines[0].PK;
			AssertEquals("JR_IsCostTaxAmountOverridden", true, cost.ApportionmentCharges[0].JR_IsCostTaxAmountOverridden);
			AssertEquals("E6_IsTaxAmountOverridden", true, cost.E6_IsTaxAmountOverridden);
		}

		public void TestTaxOverriddenFlagIsTrueForPostedConsolCost_WhenFlagWasAlreadyTickedManually()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("", "", "TSTCNSL");
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUMEL";
			CommonShipment shipment1 = creator.CreateShipment("S00010001", "USLAX", "AUMEL");
			consol.Shipments.Add(shipment1);
			Job job1 = new Job.Loader(Factory, shipment1).TryCreateWithoutMutexForTestOnly();
			Factory.Save();

			var apportionmentListing = consol.GetApportionments();
			var cost = CreateConsolCost(apportionmentListing);
			cost.E6_IsTaxAmountOverridden = true;
			AssertEquals("JR_IsCostTaxAmountOverridden", true, cost.ApportionmentCharges[0].JR_IsCostTaxAmountOverridden);
			AssertEquals("E6_IsTaxAmountOverridden", true, cost.E6_IsTaxAmountOverridden);

			var invoice = creator.CreateInvoice(typeof(APInvoice), creator.AUD, 1.0m, creator.AALSHI) as APInvoice;
			invoice.AH_TransactionNum = "ABC123";
			invoice.AH_OH = creator.AALSHI.PK;
			invoice.AH_InvoiceDate = ZDateTime.Today;
			invoice.AH_DueDate = ZDateTime.Today.AddDays(2);
			invoice.AH_TransactionReference = "COST";
			var apline1 = creator.CreateAPInvoiceLine(invoice, cost.ApportionmentCharges[0].Job as Job, cost.ChargeCode, creator.AUD, 1m, "Test Line 1", 100m);
			apline1.AL_AT = cost.ApportionmentCharges[0].JR_AT_CostGSTRate;
			apline1.AL_A9_VATClass = cost.ApportionmentCharges[0].JR_A9_CostVATClass;
			cost.E6_AH_APInvoice = invoice.PK;
			cost.ApportionmentCharges[0].JR_AL_APLine = invoice.Lines[0].PK;
			AssertEquals("JR_IsCostTaxAmountOverridden", true, cost.ApportionmentCharges[0].JR_IsCostTaxAmountOverridden);
			AssertEquals("E6_IsTaxAmountOverridden", true, cost.E6_IsTaxAmountOverridden);
		}

		public void TestResettingE6_IsTaxAmountOverriddenToTrueResetsGSTAmountAsWell()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("", "", "TSTCNSL");
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUMEL";
			CommonShipment shipment1 = creator.CreateShipment("S00010001", "USLAX", "AUMEL");
			consol.Shipments.Add(shipment1);
			Job job1 = new Job.Loader(Factory, shipment1).TryCreateWithoutMutexForTestOnly();
			Factory.Save();

			var apportionmentListing = consol.GetApportionments();
			var cost = CreateConsolCost(apportionmentListing);
			cost.E6_OSCostAmount = 250m;
			cost.E6_AT_TaxRate = creator.GST2.PK;
			AssertEquals("E6_IsTaxAmountOverridden", false, cost.E6_IsTaxAmountOverridden);
			AssertEquals("JR_OSCostGSTAmt", 50m, cost.ApportionmentCharges[0].JR_OSCostGSTAmt_Calc);
			AssertEquals("E6_OSGSTAmount_Calc", 50m, cost.E6_OSGSTAmount_Calc);

			cost.E6_IsTaxAmountOverridden = true;
			AssertEquals("E6_IsTaxAmountOverridden", true, cost.E6_IsTaxAmountOverridden);
			AssertEquals("JR_OSCostGSTAmt", 50m, cost.ApportionmentCharges[0].JR_OSCostGSTAmt_Calc);
			AssertEquals("E6_OSGSTAmount_Calc", 50m, cost.E6_OSGSTAmount_Calc);
		}

		public void TestResettingE6_IsTaxAmountOverriddenToFalseResetsGSTAmountAsWell()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("", "", "TSTCNSL");
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUMEL";
			CommonShipment shipment1 = creator.CreateShipment("S00010001", "USLAX", "AUMEL");
			consol.Shipments.Add(shipment1);
			Job job1 = new Job.Loader(Factory, shipment1).TryCreateWithoutMutexForTestOnly();
			Factory.Save();

			var apportionmentListing = consol.GetApportionments();
			var cost = CreateConsolCost(apportionmentListing);
			cost.E6_OSCostAmount = 250m;
			cost.E6_AT_TaxRate = creator.GST2.PK;
			cost.E6_IsTaxAmountOverridden = true;
			cost.E6_OSGSTAmount_Calc = 65m;
			AssertEquals("E6_IsTaxAmountOverridden", true, cost.E6_IsTaxAmountOverridden);
			AssertEquals("JR_OSCostGSTAmt", 65m, cost.ApportionmentCharges[0].JR_OSCostGSTAmt_Calc);
			AssertEquals("E6_OSGSTAmount_Calc", 65m, cost.E6_OSGSTAmount_Calc);

			cost.E6_IsTaxAmountOverridden = false;
			AssertEquals("E6_IsTaxAmountOverridden", false, cost.E6_IsTaxAmountOverridden);
			AssertEquals("JR_OSCostGSTAmt", 50m, cost.ApportionmentCharges[0].JR_OSCostGSTAmt_Calc);
			AssertEquals("E6_OSGSTAmount_Calc", 50m, cost.E6_OSGSTAmount_Calc);
		}

		public void TestChangingTaxRateRecalculatesGSTAmount_WhenWhenOverriddenFlagIsFalse()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("", "", "TSTCNSL");
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUMEL";
			CommonShipment shipment1 = creator.CreateShipment("S00010001", "USLAX", "AUMEL");
			consol.Shipments.Add(shipment1);
			Job job1 = new Job.Loader(Factory, shipment1).TryCreateWithoutMutexForTestOnly();
			Factory.Save();

			var apportionmentListing = consol.GetApportionments();
			var cost = CreateConsolCost(apportionmentListing);
			cost.E6_OSCostAmount = 250m;
			cost.E6_AT_TaxRate = creator.GST2.PK;
			AssertEquals("E6_IsTaxAmountOverridden", false, cost.E6_IsTaxAmountOverridden);
			AssertEquals("JR_OSCostGSTAmt", 50m, cost.ApportionmentCharges[0].JR_OSCostGSTAmt_Calc);
			AssertEquals("E6_OSGSTAmount_Calc", 50m, cost.E6_OSGSTAmount_Calc);

			cost.E6_AT_TaxRate = creator.GST1.PK;
			AssertEquals("E6_IsTaxAmountOverridden", false, cost.E6_IsTaxAmountOverridden);
			AssertEquals("JR_OSCostGSTAmt", 25m, cost.ApportionmentCharges[0].JR_OSCostGSTAmt_Calc);
			AssertEquals("E6_OSGSTAmount_Calc", 25m, cost.E6_OSGSTAmount_Calc);
		}

		public void TestChangingTaxRateRecalculatesGSTAmount_WhenWhenOverriddenFlagIsTrue()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("", "", "TSTCNSL");
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUMEL";
			CommonShipment shipment1 = creator.CreateShipment("S00010001", "USLAX", "AUMEL");
			consol.Shipments.Add(shipment1);
			Job job1 = new Job.Loader(Factory, shipment1).TryCreateWithoutMutexForTestOnly();
			Factory.Save();

			var apportionmentListing = consol.GetApportionments();
			var cost = CreateConsolCost(apportionmentListing);
			cost.E6_OSCostAmount = 250m;
			cost.E6_AT_TaxRate = creator.GST2.PK;
			cost.E6_IsTaxAmountOverridden = true;
			cost.E6_OSGSTAmount_Calc = 65m;

			AssertEquals("E6_IsTaxAmountOverridden", true, cost.E6_IsTaxAmountOverridden);
			AssertEquals("JR_OSCostGSTAmt", 65m, cost.ApportionmentCharges[0].JR_OSCostGSTAmt_Calc);
			AssertEquals("E6_OSGSTAmount_Calc", 65m, cost.E6_OSGSTAmount_Calc);

			cost.E6_AT_TaxRate = creator.GST1.PK;
			AssertEquals("E6_IsTaxAmountOverridden", false, cost.E6_IsTaxAmountOverridden);
			AssertEquals("JR_OSCostGSTAmt", 25m, cost.ApportionmentCharges[0].JR_OSCostGSTAmt_Calc);
			AssertEquals("E6_OSGSTAmount_Calc", 25m, cost.E6_OSGSTAmount_Calc);
		}

		public void TestChangingOSCostAmountRecalculatesGSTAmount_WhenWhenOverriddenFlagIsFalse()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("", "", "TSTCNSL");
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUMEL";
			CommonShipment shipment1 = creator.CreateShipment("S00010001", "USLAX", "AUMEL");
			consol.Shipments.Add(shipment1);
			Job job1 = new Job.Loader(Factory, shipment1).TryCreateWithoutMutexForTestOnly();
			Factory.Save();

			var apportionmentListing = consol.GetApportionments();
			var cost = CreateConsolCost(apportionmentListing);
			cost.E6_OSCostAmount = 250m;
			cost.E6_AT_TaxRate = creator.GST2.PK;

			AssertEquals("E6_IsTaxAmountOverridden", false, cost.E6_IsTaxAmountOverridden);
			AssertEquals("JR_OSCostGSTAmt", 50m, cost.ApportionmentCharges[0].JR_OSCostGSTAmt_Calc);
			AssertEquals("E6_OSGSTAmount_Calc", 50m, cost.E6_OSGSTAmount_Calc);

			cost.E6_OSCostAmount = 300m;
			AssertEquals("E6_IsTaxAmountOverridden", false, cost.E6_IsTaxAmountOverridden);
			AssertEquals("JR_OSCostGSTAmt", 60m, cost.ApportionmentCharges[0].JR_OSCostGSTAmt_Calc);
			AssertEquals("E6_OSGSTAmount_Calc", 60m, cost.E6_OSGSTAmount_Calc);
		}

		public void TestChangingOSCostAmountRecalculatesGSTAmount_WhenWhenOverriddenFlagIsTrue()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("", "", "TSTCNSL");
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUMEL";
			CommonShipment shipment1 = creator.CreateShipment("S00010001", "USLAX", "AUMEL");
			consol.Shipments.Add(shipment1);
			Job job1 = new Job.Loader(Factory, shipment1).TryCreateWithoutMutexForTestOnly();
			Factory.Save();

			var apportionmentListing = consol.GetApportionments();
			var cost = CreateConsolCost(apportionmentListing);
			cost.E6_OSCostAmount = 250m;
			cost.E6_AT_TaxRate = creator.GST2.PK;
			cost.E6_IsTaxAmountOverridden = true;
			cost.E6_OSGSTAmount_Calc = 65m;

			AssertEquals("E6_IsTaxAmountOverridden", true, cost.E6_IsTaxAmountOverridden);
			AssertEquals("JR_OSCostGSTAmt", 65m, cost.ApportionmentCharges[0].JR_OSCostGSTAmt_Calc);
			AssertEquals("E6_OSGSTAmount_Calc", 65m, cost.E6_OSGSTAmount_Calc);

			cost.E6_OSCostAmount = 300m;
			AssertEquals("E6_IsTaxAmountOverridden", true, cost.E6_IsTaxAmountOverridden);
			AssertEquals("JR_OSCostGSTAmt", 60m, cost.ApportionmentCharges[0].JR_OSCostGSTAmt_Calc);
			AssertEquals("E6_OSGSTAmount_Calc", 60m, cost.E6_OSGSTAmount_Calc);
		}

		public void TestE6_TaxDateChange()
		{
			var consol = ObjectCreator.CreateConsol();
			var consolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1);
			consolCost.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			consolCost.E6_AT_TaxRate = CreateTaxRate().PK;
			consolCost.E6_TaxDate = ZDate.Today;
			consolCost.E6_OSCostAmount = 8m;
			AssertAmounts(ZDate.Today, 8.8M, 0.8M, 8.8M, 0.8M);

			consolCost.E6_TaxDate = ZDate.Today.AddDays(1);
			AssertAmounts(ZDate.Today.AddDays(1), 8.24M, 0.24M, 8.24M, 0.24M);

			void AssertAmounts(ZDate taxDate, decimal totalAmount, decimal taxAmount, decimal invoiceTotal, decimal invoiceTax)
			{
				AssertEquals(taxDate, consolCost.E6_TaxDate);
				AssertEquals(totalAmount, consolCost.E6_Calc_OSTotalAmount);
				AssertEquals(taxAmount, consolCost.E6_OSGSTAmount_Calc);
				AssertEquals(invoiceTotal, consolCost.InvoiceOSTotal);
				AssertEquals(invoiceTax, consolCost.InvoiceOSTax);
			}

			AccTaxRate CreateTaxRate()
			{
				var rate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
				rate.SetRate_ForTestOnly(10, 1, ZDate.Today.AddDays(-1), ZDate.Today);
				rate.SetRate_ForTestOnly(18, 6, ZDate.Today.AddDays(1), ZDate.Today.AddDays(2));
				return rate;
			}
		}

		public void TestE6_TaxDatePopulatedOnCharges()
		{
			var consol = ObjectCreator.CreateConsol();
			var shipment = ObjectCreator.CreateShipment("S001", consol);
			var job = ObjectCreator.CreateJob(shipment, false);
			var consolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, ObjectCreator.Creditor1);
			consolCost.E6_AT_TaxRate = ObjectCreator.GST1.PK;
			AssertDate(ZDate.BrettsBirthday);
			AssertDate(ZDate.Empty);
			AssertDate(ZDate.Today);

			void AssertDate(ZDate date)
			{
				consolCost.E6_TaxDate = date;
				AssertEquals("Precondition: E6_TaxDate", date, consolCost.E6_TaxDate);
				AssertEquals("JR_CostTaxDate", date, consolCost.ApportionmentCharges[0].JR_CostTaxDate);
			}
		}

		public void TestE6_AT_TaxRate()
		{
			var consol = ObjectCreator.CreateConsol();
			var consolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1);

			consolCost.E6_AT_TaxRate = ObjectCreator.GST1.PK;
			consolCost.E6_TaxDate = ZDate.Today;
			AssertEquals(ZDate.Today, consolCost.E6_TaxDate);

			consolCost.E6_AT_TaxRate = ZGuid.Empty;
			AssertEquals(ZDate.Empty, consolCost.E6_TaxDate);
		}

		public void TestPopulateMissingApportionments_JR_CostTaxDate()
		{
			var consol = ObjectCreator.CreateConsol();
			var shipment = ObjectCreator.CreateShipment("S001", consol);
			var job = ObjectCreator.CreateJob(shipment, false);
			var consolCost1 = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, 100, ObjectCreator.Creditor1);

			var consolCost2 = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, 100, ObjectCreator.Creditor1);

			var destinationConsolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, 100);
			destinationConsolCost.E6_AT_TaxRate = ObjectCreator.GST1.PK;
			destinationConsolCost.E6_TaxDate = ZDate.Today.AddDays(2);
			destinationConsolCost.ApportionmentCharges.RemoveAndDeleteAll();

			destinationConsolCost.PopulateMissingApportionments(consolCost1);
			AssertEquals("JR_AT_CostGSTRate", ObjectCreator.GST1.PK, destinationConsolCost.ApportionmentCharges[0].JR_AT_CostGSTRate);
			AssertEquals("JR_CostTaxDate", ZDate.Today.AddDays(2), destinationConsolCost.ApportionmentCharges[0].JR_CostTaxDate);

			destinationConsolCost.E6_TaxDate = ZDate.Empty;
			destinationConsolCost.ApportionmentCharges.RemoveAndDeleteAll();
			destinationConsolCost.PopulateMissingApportionments(consolCost2);
			AssertEquals("JR_AT_CostGSTRate", ObjectCreator.GST1.PK, destinationConsolCost.ApportionmentCharges[0].JR_AT_CostGSTRate);
			AssertEquals("JR_CostTaxDate", ZDate.Empty, destinationConsolCost.ApportionmentCharges[0].JR_CostTaxDate);
		}

		public void TestCopyValues_JR_CostTaxDate()
		{
			var consol = ObjectCreator.CreateConsol();
			var shipment = ObjectCreator.CreateShipment("S001", consol);
			var job = ObjectCreator.CreateJob(shipment, false);
			var consolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, 100, ObjectCreator.Creditor1);
			consolCost.E6_AT_TaxRate = ObjectCreator.VATSPV.PK;
			consolCost.E6_TaxDate = ZDate.Today.AddDays(2);

			var pK = consolCost.ApportionmentCharges[0].PK;

			consolCost.PrepareForPosting();

			AssertNotEquals(pK, consolCost.ApportionmentCharges[0].PK);
			AssertEquals(ObjectCreator.VATSPV.PK, consolCost.ApportionmentCharges[0].JR_AT_CostGSTRate);
			AssertEquals(ZDate.Today.AddDays(2), consolCost.ApportionmentCharges[0].JR_CostTaxDate);
		}

		public void TestSynchroniseUnpostedInvoiceDetailsIfNecessary_JR_CostTaxDate()
		{
			var consol = ObjectCreator.CreateConsol();
			var shipment = ObjectCreator.CreateShipment("S001", consol);
			var job = ObjectCreator.CreateJob(shipment, false);
			var charge = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, "Desc 01", ObjectCreator.AUD, 100M, ObjectCreator.Creditor1, ObjectCreator.AUD, 100M, ObjectCreator.Debtor);
			charge.JR_AT_CostGSTRate = ObjectCreator.GST1.PK;
			charge.JR_CostTaxDate = ZDate.Today.AddDays(2);

			var consolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, 100, ObjectCreator.Creditor1);
			consolCost.E6_AT_TaxRate = ObjectCreator.VATSPV.PK;
			consolCost.E6_TaxDate = ZDate.Today.AddDays(5);
			consolCost.PrepareForPosting();

			AssertEquals(ObjectCreator.VATSPV.PK, charge.JR_AT_CostGSTRate);
			AssertEquals(ZDate.Today.AddDays(5), charge.JR_CostTaxDate);
		}

		public void TestSyncConsolCostAndInvoice_E6_TaxDate()
		{
			var creator = new TestObjectCreator(Factory);
			//Creating Consol
			var consol = creator.CreateConsol("", "", "TSTCNSL");
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUMEL";
			CommonShipment shipment1 = creator.CreateShipment("S00010001", "USLAX", "AUMEL");
			consol.Shipments.Add(shipment1);
			Job job1 = new Job.Loader(Factory, shipment1).TryCreateWithoutMutexForTestOnly();
			CommonShipment shipment2 = creator.CreateShipment("S00010002", "USLAX", "AUMEL");
			consol.Shipments.Add(shipment2);
			Job job2 = new Job.Loader(Factory, shipment2).TryCreateWithoutMutexForTestOnly();
			Factory.Save();
			//Creating Consol Cost
			var apportionmentListing = consol.GetApportionments();
			var cost = CreateConsolCost(apportionmentListing);
			//Creating Invoices
			var invoice1 = creator.CreateInvoice(typeof(APInvoice), creator.AUD, 1.0m, creator.AALSHI) as APInvoice;
			invoice1.AH_TransactionNum = "INV0001";
			var apline1 = creator.CreateAPInvoiceLine(invoice1, cost.ApportionmentCharges[0].Job as Job, cost.ChargeCode, creator.AUD, 1m, "Test Line 1", 100m);
			var invoice2 = creator.CreateInvoice(typeof(APInvoice), creator.AUD, 1.0m, creator.AALSHI) as APInvoice;
			invoice2.AH_TransactionNum = "INV0002";
			var apline2 = creator.CreateAPInvoiceLine(invoice2, cost.ApportionmentCharges[1].Job as Job, cost.ChargeCode, creator.AUD, 1m, "Test Line 2", 110m);
			apline2.AL_AT = creator.VATSPV.PK;
			apline2.AL_TaxDate = ZDate.Today.AddDays(6);
			var message = string.Empty;

			//Cost is linked to different Invoice
			cost = CreateConsolCost(apportionmentListing);
			cost.E6_AH_APInvoice = invoice1.PK;
			cost.ApportionmentCharges[0].JR_AL_APLine = invoice2.Lines[0].PK;
			cost.ApportionmentCharges[1].JR_AL_APLine = invoice2.Lines[0].PK;
			Assert("SynchroniseInvoiceDetailsIfNecessary", cost.SynchronisePostedInvoiceDetailsIfPossible(out message));
			AssertEquals(creator.VATSPV.PK, cost.E6_AT_TaxRate);
			AssertEquals(ZDate.Today.AddDays(6), cost.E6_TaxDate);
		}

		public void TestE6_OSCostAmountRoundingErrorReproterFunctionalitySuspenderReportError()
		{
			var consol = ObjectCreator.CreateConsol();
			var shipment = ObjectCreator.CreateShipment("SHP001", consol);
			var job = ObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var apInvoice = ObjectCreator.CreateInvoice(typeof(APInvoice), ObjectCreator.AUD);
			apInvoice.SubmittedFromInvoicingForm = true;
			apInvoice.AH_OH = ObjectCreator.AALSHI.PK;

			var invoiceCost = ObjectCreator.CreateConsolCost(apInvoice, consol, ObjectCreator.CC1, 258.45M);
			invoiceCost.E6_OSGSTAmount = 258.45M;
			invoiceCost.SetContext(ConsolCostStrategy.ConsolCostCalculationStrategyWithoutCalculations);

			using (invoiceCost.E6_OSCostAmountRoundingErrorReproterFunctionalitySuspender.GetSuspender())
			{
				invoiceCost.E6_RX_NKCurrency = "VND";
				Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			}
			AssertErrorReport(true);

			invoiceCost.E6_RX_NKCurrency = "AUD";

			using (invoiceCost.E6_OSCostAmountRoundingErrorReproterFunctionalitySuspender.GetSuspender())
			{
				invoiceCost.ApportionmentCharges[0].JR_OSCostAmt = 245M;
				invoiceCost.E6_RX_NKCurrency = "VND";
				Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			}
			AssertErrorReport(false);

			void AssertErrorReport(bool isErrorForCharge)
			{
				const string errorForCharge =
@"JR_OSCostAmt: 258.45
JR_OSCostGSTAmt: 26.85
JR_OSCostGSTAmt_Calc: 26.85";

				const string errorForConsolCost =
	@"E6_OSCostAmount: 258.45
E6_OSGSTAmount: 258.45
E6_OSGSTAmount_Calc: 258.45
GSTInclusiveAmount: 516.90";

				AssertMultilineASCIIEquals(
$@"In Enterprise.Accounting.Business.{(isErrorForCharge ? "JobInvoicing.ApportionSplitCharge" : "ConsolCosting.JobConsolCost")} -
Properties requiring to be rounded on currency change have more decimal places than allowed, this may be due to setter not propagating values due to has changes check.
Incorrectly rounded values are:
{(isErrorForCharge ? errorForCharge : errorForConsolCost)}
Currency AUD was changed to VND
Only 0 decimals are allowed.",
ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();
			}
		}

		[TestDate(2018, 8, 6)]
		public void TestWhenChangedE6_RX_NKCurrencyAndIsGSTInclusiveAmountIsTrueRecalculationGSTInclusiveAmount()
		{
			GlbCompany.CurrentCompany.SetCurrency(ObjectCreator.CurrencyWithoutCents.Code);
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			GlbCompany.CurrentCompany.Factory.Save();

			var consol = Factory.New<ForwardingConsol>();

			var invoice = Factory.New<APInvoice>();

			var testCost = AddNewConsolCostToInvoice(consol, invoice);

			invoice.GSTInclusiveAmounts = true;

			testCost.E6_AT_TaxRate = ObjectCreator.CreateTaxRate("TestGST", "", 10).PK;
			testCost.E6_RX_NKCurrency = ObjectCreator.AUD.RX_Code;
			testCost.E6_ExchangeRate = 100M;
			testCost.GSTInclusiveAmount = 11.22m;

			AssertEquals(10.2m, testCost.E6_OSCostAmount);
			AssertEquals(1.02m, testCost.E6_OSGSTAmount_Calc);
			AssertEquals(11.22m, testCost.GSTInclusiveAmount);

			testCost.E6_RX_NKCurrency = ObjectCreator.CurrencyWithoutCents.Code;

			AssertEquals(1020m, testCost.E6_OSCostAmount);
			AssertEquals(102m, testCost.E6_OSGSTAmount_Calc);
			AssertEquals("When currency changes, only update decimal at present.", 11m, testCost.GSTInclusiveAmount);
			AssertEquals(testCost.E6_RX_NKCurrency, ObjectCreator.CurrencyWithoutCents.Code);
			Assert("The decimal places of GSTInclusiveAmount should be less than or equal to the decimal places of currency.", testCost.GSTInclusiveAmount.DecimalPlaces <= ObjectCreator.CurrencyWithoutCents.Decimals);
		}

		public void TestDeleteJobConsolCost_ConsolidatedAndShipmentFreightCostChargeableIsUpdated()
		{
			var nonFreightCharge = Factory.NewWithValidTestData<AccChargeCode>();
			nonFreightCharge.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			nonFreightCharge.AC_Code = "OTT";
			nonFreightCharge.Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualChargeable = 1;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ActualChargeable = 2;

			var apps = new ApportionmentListing(Factory, consol);
			try
			{
				var decimalPoints = consol.LocalCurrencyDecimals;

				AssertEquals(consol.JK_Calc_ConsolidatedFreightCostChargeable, 0m);
				AssertEquals(consol.JK_Calc_ShipmentFreightCostChargeable, 0m);

				var freightCost1 = apps.CostsCollection.TryAddNew();
				freightCost1.E6_GC = GlbCompany.CurrentCompany.PK;
				freightCost1.E6_LocalCostAmount = 2000;
				freightCost1.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;

				var freightCost2 = apps.CostsCollection.TryAddNew();
				freightCost2.E6_GC = GlbCompany.CurrentCompany.PK;
				freightCost2.E6_LocalCostAmount = 1000;
				freightCost2.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;

				var nonFreightCost = apps.CostsCollection.TryAddNew();
				nonFreightCost.E6_GC = GlbCompany.CurrentCompany.PK;
				nonFreightCost.E6_LocalCostAmount = 5000;
				nonFreightCost.E6_AC_ChargeCode = nonFreightCharge.PK;

				AssertEquals(consol.JK_Calc_ConsolidatedFreightCostChargeable.Round(decimalPoints), 1000m);
				AssertEquals(consol.JK_Calc_ShipmentFreightCostChargeable.Round(decimalPoints), 1000m);

				apps.CostsCollection.RemoveAndDelete(freightCost1);
				AssertEquals(consol.JK_Calc_ConsolidatedFreightCostChargeable.Round(decimalPoints), 333.33m);
				AssertEquals(consol.JK_Calc_ShipmentFreightCostChargeable.Round(decimalPoints), 333.33m);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestTaxAmountShouldEqualLocalTaxAmountWithLocalCurrencyAndIndiaStateGST()
		{
			var countryCode = Constants.CountryCodes.India;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var consol = ObjectCreator.CreateConsol();
				var shipment = ObjectCreator.CreateShipment("S001", consol);
				var job = ObjectCreator.CreateJob(shipment, false);
				var consolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, 100, ObjectCreator.Creditor1);

				var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "INR"));

				Assert("Precondition", !consolCost.E6_IsTaxAmountOverridden);

				consolCost.E6_AC_ChargeCode = ObjectCreator.FRT.PK;
				consolCost.E6_AT_TaxRate = ObjectCreator.STAGST.PK;
				consolCost.E6_RX_NKCurrency = currency.RX_Code;
				consolCost.E6_OSCostAmount = 502.82M;

				AssertEquals(90.50m, consolCost.E6_OSGSTAmount_Calc);
				AssertEquals(consolCost.E6_OSGSTAmount_Calc, consolCost.InvoiceLocalTax);
			}
		}

		public void TestSetCostTaxDateWhenSetE6_AT_TaxRate()
		{
			var consol = ObjectCreator.CreateConsol();
			var shipment = ObjectCreator.CreateShipment("S001", consol);
			var job = ObjectCreator.CreateJob(shipment, false);
			var consolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, ObjectCreator.Creditor1);
			var today = ZDate.Today;

			consolCost.E6_AT_TaxRate = Guid.Empty;
			consolCost.E6_TaxDate = today;
			consolCost.ApportionmentCharges[0].JR_CostTaxDate = ZDate.Empty;
			consolCost.E6_AT_TaxRate = ObjectCreator.GST1WithDates.PK;
			AssertEquals("Precondition: E6_TaxDate", today, consolCost.E6_TaxDate);
			AssertEquals("JR_CostTaxDate", today, consolCost.ApportionmentCharges[0].JR_CostTaxDate);
		}

		public void TestRedeaultExchangeRateWhenConsolCostIsPosted()
		{
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate(LedgerTypes.AccountsPayable, JobInvoicingConsumerTypes.ForwardingConsol.Code, Constants.TransportModes.All, Constants.FreightShipmentDirection.Code.All, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate, 0, true);
			GlbCompany.CurrentCompany.Factory.Save();
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());

			var creator = new TestObjectCreator(Factory);
			var currency = NewFactory().Load<RefCurrency>(ObjectCreator.USD.PK);
			var exchangeRate = currency.ExchangeRates.AddNew();
			exchangeRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.BuyRate;
			exchangeRate.RE_SellRate = 1.333333m;
			exchangeRate.RE_StartDate = ZDateTime.Today;
			exchangeRate.RE_ExpiryDate = ZDateTime.Today;
			exchangeRate.RE_OH_Client = ZGuid.Empty;
			currency.Factory.Save();
			ExchangeRateReader.GetReaderInstance().ClearCache();

			var consol = ObjectCreator.CreateConsol();
			var shipment = ObjectCreator.CreateShipment("S00000001", consol);
			var job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			Factory.Save();

			var consolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.FRT, ObjectCreator.USD, 1.242m, 1000m);
			consolCost.E6_OH_Creditor = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();

			AssertEquals("IsPosted", false, consolCost.IsPosted);
			AssertEquals(1.333333m, consolCost.E6_ExchangeRate);

			exchangeRate.RE_SellRate = 1.444444m;
			exchangeRate.Factory.Save();

			consolCost.RedeaultExchangeRate();
			AssertEquals(1.444444m, consolCost.E6_ExchangeRate);

			var charge1 = job.Charges[0];
			charge1.JR_JH_InternalJob = job.PK;
			charge1.JR_GB_InternalBranch = creator.NonCurrentBranch.PK;
			charge1.JR_GE_InternalDept = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			AssertEquals("IsPosted", true, consolCost.IsPosted);

			exchangeRate.RE_SellRate = 1.555555m;
			exchangeRate.Factory.Save();

			consolCost.RedeaultExchangeRate();
			AssertEquals(1.444444m, consolCost.E6_ExchangeRate);
		}

		public void TestIsCostTaxBranchActualAndIsCostGSTRateActual()
		{
			var consol = ObjectCreator.CreateConsol();
			var consolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, 100, ObjectCreator.Creditor1);

			var creditorIsTaxApplicable = ObjectCreator.Creditor1;
			creditorIsTaxApplicable.CompanyData.OB_APVATConfig = OrganisationTaxConfiguartionTypes.Default.Code;

			var creditorNotTaxApplicable = ObjectCreator.Creditor2;
			creditorNotTaxApplicable.CompanyData.OB_APVATConfig = OrganisationTaxConfiguartionTypes.NotApplicable.Code;

			var orgForNotCompanyOrgProxy = ObjectCreator.Creditor3;

			AssertCases(isAutoJRJ: false, enabletaxBranch: false);
			AssertCases(isAutoJRJ: false, enabletaxBranch: true);
			AssertCases(isAutoJRJ: true, enabletaxBranch: false);
			AssertCases(isAutoJRJ: true, enabletaxBranch: true);

			void AssertCases(bool isAutoJRJ, bool enabletaxBranch)
			{
				var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();
				if (isAutoJRJ)
				{
					AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(companyPK);
				}
				else
				{
					AutoJRJRegistryStatusHelper.SetAutoJRJDisabled_ForTestOnly(companyPK);
				}

				AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, enabletaxBranch);
				string commentPrepend = isAutoJRJ ? "[AutoJRJ]" : "[Not AutoJRJ]";
				commentPrepend += enabletaxBranch ? "[Enable TaxBranch]" : "[Disable TaxBranch]";

				AssertCore(companyGstRegistered: true, companyOrgProxy: orgForNotCompanyOrgProxy, creditor: creditorIsTaxApplicable
					, true, enabletaxBranch, $"{commentPrepend}");
				AssertCore(companyGstRegistered: true, companyOrgProxy: orgForNotCompanyOrgProxy, creditor: creditorNotTaxApplicable
					, false, false, $"{commentPrepend}");
				AssertCore(companyGstRegistered: true, companyOrgProxy: orgForNotCompanyOrgProxy, creditor: null
					, false, false, $"{commentPrepend}");

				if (isAutoJRJ)
				{
					AssertCore(companyGstRegistered: true, companyOrgProxy: creditorIsTaxApplicable, creditor: creditorIsTaxApplicable
						, false, false, $"{commentPrepend}");
				}
				else
				{
					AssertCore(companyGstRegistered: true, companyOrgProxy: creditorIsTaxApplicable, creditor: creditorIsTaxApplicable
						, true, enabletaxBranch, $"{commentPrepend}");
				}

				AssertCore(companyGstRegistered: true, companyOrgProxy: creditorNotTaxApplicable, creditor: creditorNotTaxApplicable
					, false, false, $"{commentPrepend}");

				AssertCore(companyGstRegistered: false, companyOrgProxy: orgForNotCompanyOrgProxy, creditor: creditorIsTaxApplicable
					, false, false, $"{commentPrepend}");
				AssertCore(companyGstRegistered: false, companyOrgProxy: orgForNotCompanyOrgProxy, creditor: creditorNotTaxApplicable
					, false, false, $"{commentPrepend}");
				AssertCore(companyGstRegistered: false, companyOrgProxy: orgForNotCompanyOrgProxy, creditor: null
					, false, false, $"{commentPrepend}");
				AssertCore(companyGstRegistered: false, companyOrgProxy: creditorIsTaxApplicable, creditor: creditorIsTaxApplicable
					, false, false, $"{commentPrepend}");
				AssertCore(companyGstRegistered: false, companyOrgProxy: creditorNotTaxApplicable, creditor: creditorNotTaxApplicable
					, false, false, $"{commentPrepend}");
			}

			void AssertCore(bool companyGstRegistered, OrgHeader companyOrgProxy, OrgHeader creditor, bool shouldBeTaxApplicable, bool shouldHaveTaxBranch, string comment)
			{
				Argument.NotNull(companyOrgProxy, "companyOrgProxy");

				string commentPreppend = $"[CompanyGstRegistered:{companyGstRegistered}]";
				commentPreppend += $"[Creditor {(companyOrgProxy.PK.Equals(creditor?.PK) ? "Is" : "Not")} CompanyOrgProxy]";
				commentPreppend += creditor != null
					? $"[Creditor {(creditor.CompanyData.IsAPTaxApplicable ? "Is" : "Not")} AP Tax applicable]"
					: "[Creditor Is Empty]";

				GlbCompany.CurrentCompany.GC_IsGSTRegistered = companyGstRegistered;

				consolCost.Company.GC_OH_OrgProxy = companyOrgProxy.PK;
				consolCost.E6_OH_Creditor = creditor?.PK ?? ZGuid.Empty;

				consolCost.E6_GB_CostTaxBranch = ObjectCreator.NonCurrentBranch.PK;
				AssertEquals($"{commentPreppend}[With TaxBranch]{comment} IsCostTaxBranchActual", shouldHaveTaxBranch, consolCost.IsCostTaxBranchActual);
				consolCost.E6_GB_CostTaxBranch = ZGuid.Empty;
				AssertEquals($"{commentPreppend}[No TaxBranch]{comment} IsCostTaxBranchActual", !shouldHaveTaxBranch, consolCost.IsCostTaxBranchActual);

				consolCost.E6_AT_TaxRate = ObjectCreator.GST1.PK;
				AssertEquals($"{commentPreppend}[With TaxRate]{comment} IsCostGSTRateActual", shouldBeTaxApplicable, consolCost.IsCostGSTRateActual);
				consolCost.E6_AT_TaxRate = ZGuid.Empty;
				AssertEquals($"{commentPreppend}[No TaxRate]{comment} IsCostGSTRateActual", !shouldBeTaxApplicable, consolCost.IsCostGSTRateActual);

				AssertChargeCodeIsComment();
			}

			void AssertChargeCodeIsComment()
			{
				Argument.NotNull(consolCost.ChargeCode, "consolCost.ChargeCode");
				var originalChargeCodeType = consolCost.ChargeCode.AC_ChargeType;

				consolCost.ChargeCode.AC_ChargeType = Constants.ChargeType.Comment;

				consolCost.E6_GB_CostTaxBranch = ObjectCreator.NonCurrentBranch.PK;
				AssertEquals("true in any case when charge code AC_ChargeType is CMT", true, consolCost.IsCostTaxBranchActual);
				consolCost.E6_GB_CostTaxBranch = ZGuid.Empty;
				AssertEquals("true in any case when charge code AC_ChargeType is CMT", true, consolCost.IsCostTaxBranchActual);

				consolCost.E6_AT_TaxRate = ObjectCreator.GST1.PK;
				AssertEquals("true in any case when charge code AC_ChargeType is CMT", true, consolCost.IsCostGSTRateActual);
				consolCost.E6_AT_TaxRate = ZGuid.Empty;
				AssertEquals("true in any case when charge code AC_ChargeType is CMT", true, consolCost.IsCostGSTRateActual);

				consolCost.ChargeCode.AC_ChargeType = originalChargeCodeType;
			}
		}

		[TestDate(2021, 9, 20)]
		public void TestResetCostTaxInfo()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = testObjectCreator.CreateConsol();
			var consolCost = testObjectCreator.CreateConsolCost(consol, testObjectCreator.CC1, 100, testObjectCreator.Creditor1);

			var creditorIsTaxApplicable = testObjectCreator.Creditor1;
			creditorIsTaxApplicable.CompanyData.OB_APVATConfig = OrganisationTaxConfiguartionTypes.Default.Code;

			var creditorNotTaxApplicable = testObjectCreator.Creditor2;
			creditorNotTaxApplicable.CompanyData.OB_APVATConfig = OrganisationTaxConfiguartionTypes.NotApplicable.Code;

			var orgNotCompanyOrgProxy = testObjectCreator.Creditor3;

			AssertCases(isAutoJRJ: false, enableTaxBranch: false);
			AssertCases(isAutoJRJ: false, enableTaxBranch: true);
			AssertCases(isAutoJRJ: true, enableTaxBranch: false);
			AssertCases(isAutoJRJ: true, enableTaxBranch: true);

			void AssertCases(bool isAutoJRJ, bool enableTaxBranch)
			{
				var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();
				if (isAutoJRJ)
				{
					AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(companyPK);
				}
				else
				{
					AutoJRJRegistryStatusHelper.SetAutoJRJDisabled_ForTestOnly(companyPK);
				}

				AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, enableTaxBranch);
				string commentPrepend = isAutoJRJ ? "[AutoJRJ]" : "[Not AutoJRJ]";
				commentPrepend += enableTaxBranch ? "[Enable TaxBranch]" : "[Disable TaxBranch]";

				AssertResetCostTaxInfo(consolCost, companyGstRegistered: true, companyOrgProxy: orgNotCompanyOrgProxy, creditor: creditorIsTaxApplicable, enableTaxBranch
					, true, $"{commentPrepend}");
				AssertResetCostTaxInfo(consolCost, companyGstRegistered: true, companyOrgProxy: orgNotCompanyOrgProxy, creditor: creditorNotTaxApplicable, enableTaxBranch
					, false, $"{commentPrepend}");
				AssertResetCostTaxInfo(consolCost, companyGstRegistered: true, companyOrgProxy: orgNotCompanyOrgProxy, creditor: null, enableTaxBranch
					, false, $"{commentPrepend}");

				if (isAutoJRJ)
				{
					AssertResetCostTaxInfo(consolCost, companyGstRegistered: true, companyOrgProxy: creditorIsTaxApplicable, creditor: creditorIsTaxApplicable, enableTaxBranch
						, false, $"{commentPrepend}");
				}
				else
				{
					AssertResetCostTaxInfo(consolCost, companyGstRegistered: true, companyOrgProxy: creditorIsTaxApplicable, creditor: creditorIsTaxApplicable, enableTaxBranch
						, true, $"{commentPrepend}");
				}

				AssertResetCostTaxInfo(consolCost, companyGstRegistered: true, companyOrgProxy: creditorNotTaxApplicable, creditor: creditorNotTaxApplicable, enableTaxBranch
					, false, $"{commentPrepend}");

				AssertResetCostTaxInfo(consolCost, companyGstRegistered: false, companyOrgProxy: orgNotCompanyOrgProxy, creditor: creditorIsTaxApplicable, enableTaxBranch
					, false, $"{commentPrepend}");
				AssertResetCostTaxInfo(consolCost, companyGstRegistered: false, companyOrgProxy: orgNotCompanyOrgProxy, creditor: creditorNotTaxApplicable, enableTaxBranch
					, false, $"{commentPrepend}");
				AssertResetCostTaxInfo(consolCost, companyGstRegistered: false, companyOrgProxy: orgNotCompanyOrgProxy, creditor: null, enableTaxBranch
					, false, $"{commentPrepend}");
				AssertResetCostTaxInfo(consolCost, companyGstRegistered: false, companyOrgProxy: creditorIsTaxApplicable, creditor: creditorIsTaxApplicable, enableTaxBranch
					, false, $"{commentPrepend}");
				AssertResetCostTaxInfo(consolCost, companyGstRegistered: false, companyOrgProxy: creditorNotTaxApplicable, creditor: creditorNotTaxApplicable, enableTaxBranch
					, false, $"{commentPrepend}");
			}
		}

		[TestDate(2018, 03, 01)]
		public void TestValidationWhenJobConsolCostIsImportedToAnIncompleteInvoice()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S0001", consol);
			_ = TestObjectCreator.CreateJob(shipment);
			var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 250M);
			Factory.Save();

			var incompleteInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "00001");
			var importer = new InvoicingBaseBulkConsolCostImporter(incompleteInvoice.ConsolCosting);
			importer.LoadConsolsCollection();
			importer.Import();
			incompleteInvoice.ImportAllApportionmentsFromCosting();
			incompleteInvoice.SaveAsIncomplete();

			var newFactory = new BusinessObjectFactory();
			var invoice2 = new TestObjectCreator(newFactory).CreateInvoice(typeof(APInvoice), "111", TestObjectCreator.AUD, 1M);
			importer = new InvoicingBaseBulkConsolCostImporter(invoice2.ConsolCosting);
			importer.LoadConsolsCollection();
			importer.Import();
			AssertEquals("Imported Cost Count", 1, invoice2.ConsolCosting.ConsolCosts.Count);

			var importedCost = invoice2.ConsolCosting.ConsolCosts[0];
			AssertType<APInvoiceConsolCostValidation>(importedCost.Validation);
			importedCost.Validation.ValidateAll();
			var expectedError = "The associated apportion charges are already used in an Incomplete Invoice 00001 dated 01 Mar 2018. Please delete this row. If required, you can manually enter a new cost without importing the existing one.";
			AssertHasRowError("Should have an error", importedCost, expectedError);
		}

		[TestDate(2018, 03, 01)]
		public void TestValidationWhenJobConsolCostIsImportedAndIncompleteInvoiceIsReloaded()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S0001", consol);
			_ = TestObjectCreator.CreateJob(shipment);
			var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 250M);
			Factory.Save();

			var incompleteInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "00001");
			var importer = new InvoicingBaseBulkConsolCostImporter(incompleteInvoice.ConsolCosting);
			importer.LoadConsolsCollection();
			importer.Import();
			incompleteInvoice.ImportAllApportionmentsFromCosting();
			incompleteInvoice.SaveAsIncomplete();

			var newFactory = new BusinessObjectFactory();
			var apInvoice = newFactory.Load<APInvoice>(incompleteInvoice.PK);
			apInvoice.RestoreSavedData();
			AssertEquals("Imported Cost Count", 1, apInvoice.ConsolCosting.ConsolCosts.Count);

			var importedCost = apInvoice.ConsolCosting.ConsolCosts[0];
			AssertType<APInvoiceConsolCostValidation>(importedCost.Validation);
			importedCost.Validation.ValidateAll();
			AssertNoRowErrors("Should have no error", importedCost);
		}

		public void TestSetTaxAmountCalc_ValueDecimalPlacesNotSameWithCurrencyDecimalPlaces()
		{
			var currency1 = Factory.New<RefCurrency>();
			var currency2 = Factory.New<RefCurrency>();
			var currency3 = Factory.New<RefCurrency>();
			var currency4 = Factory.New<RefCurrency>();

			currency1.RX_Code = "TE1";
			currency2.RX_Code = "TE2";
			currency3.RX_Code = "TE3";
			currency4.RX_Code = "TE4";

			currency1.RX_SubUnitRatio = 1;
			currency2.RX_SubUnitRatio = 10;
			currency3.RX_SubUnitRatio = 100;
			currency4.RX_SubUnitRatio = 1000;

			Factory.Save();

			var consolCost = Factory.New<JobConsolCost>();
			consolCost.E6_IsTaxAmountOverridden = true;

			AssertTaxAmount("TE1");

			AssertTaxAmount("TE2");

			AssertTaxAmount("TE3");

			AssertTaxAmount("TE4");

			void AssertTaxAmount(string currencyCode)
			{
				consolCost.E6_RX_NKCurrency = currencyCode;
				consolCost.E6_OSGSTAmount_Calc = 1000.1111m;
				AssertEquals((ZDecimal)Utilities.Round(1000.1111m, consolCost.CurrencyDecimals), consolCost.E6_OSGSTAmount_Calc);
			}
		}

		void AssertResetCostTaxInfo(JobConsolCost consolCost, bool companyGstRegistered, OrgHeader companyOrgProxy, OrgHeader creditor, bool enableTaxBranch, bool shouldHaveTaxInfo, string comment)
		{
			Argument.NotNull(consolCost, "consolCost");
			Argument.NotNull(consolCost.ChargeCode, "consolCost.ChargeCode");
			Argument.NotNull(companyOrgProxy, "companyOrgProxy");

			string commentPreppend = $"[consolCost , companyGstRegistered:{companyGstRegistered}]";
			commentPreppend += $"[Creditor {(companyOrgProxy.PK.Equals(creditor?.PK) ? "Is" : "Not")} CompanyOrgProxy]";
			commentPreppend += creditor != null
				? $"[Creditor {(creditor.CompanyData.IsAPTaxApplicable ? "Is" : "Not")} AP Tax applicable]"
				: "[Creditor Is Empty]";

			commentPreppend += $"[{(enableTaxBranch ? "Enable" : "Disable")}  TaxBranch]";

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = companyGstRegistered;
			consolCost.Company.GC_IsGSTRegistered = companyGstRegistered;

			consolCost.Company.GC_OH_OrgProxy = companyOrgProxy.PK;
			consolCost.E6_OH_Creditor = creditor?.PK ?? ZGuid.Empty;

			if (shouldHaveTaxInfo)
			{
				SetConsolWithoutTaxInfo();
				consolCost.ResetCostTaxInfo();
				AssertConsolWithTaxInfo($"{commentPreppend}[Empty->Valued]", false, enableTaxBranch);

				SetConsolWithTaxInfo();
				consolCost.ResetCostTaxInfo();
				AssertConsolWithTaxInfo($"{commentPreppend}[Valued->Valued]", true, enableTaxBranch);
			}
			else
			{
				SetConsolWithTaxInfo();
				consolCost.ResetCostTaxInfo();
				AssertConsolWithoutTaxInfo($"{commentPreppend}[Valued->Empty]");

				SetConsolWithoutTaxInfo();
				consolCost.ResetCostTaxInfo();
				AssertConsolWithoutTaxInfo($"{commentPreppend}[Empty->Empty]");
			}
			AssertChargeCodeIsComment();

			void AssertChargeCodeIsComment()
			{
				var originalChargeCodeType = consolCost.ChargeCode.AC_ChargeType;

				consolCost.ChargeCode.AC_ChargeType = Constants.ChargeType.Comment;

				SetConsolWithoutTaxInfo();
				consolCost.ResetCostTaxInfo();
				AssertConsolWithoutTaxInfo("should not change any Tax Info in any case when charge code AC_ChargeType is CMT.");

				SetConsolWithTaxInfo();
				consolCost.ResetCostTaxInfo();
				AssertConsolWithTaxInfo("should not change any Tax Info in any case when charge code AC_ChargeType is CMT.");

				consolCost.ChargeCode.AC_ChargeType = originalChargeCodeType;
			}

			void SetConsolWithoutTaxInfo()
			{
				consolCost.E6_GB_CostTaxBranch = ZGuid.Empty;
				consolCost.E6_AT_TaxRate = ZGuid.Empty;
				consolCost.E6_TaxDate = ZDate.Empty;
				AssertConsolWithoutTaxInfo("PreCondition");
			}

			void AssertConsolWithoutTaxInfo(string commentInner)
			{
				AssertEquals($"{commentInner}E6_AT_TaxRate", ZGuid.Empty, consolCost.E6_AT_TaxRate);
				AssertEquals($"{commentInner}E6_TaxDate", ZDate.Empty, consolCost.E6_TaxDate);
				AssertEquals($"{commentInner}E6_GB_CostTaxBranch", ZGuid.Empty, consolCost.E6_GB_CostTaxBranch);
			}

			void SetConsolWithTaxInfo()
			{
				consolCost.E6_GB_CostTaxBranch = GlbBranch.CurrentBranch.PK;
				consolCost.E6_AT_TaxRate = TestObjectCreator.GST1.PK;
				consolCost.E6_TaxDate = ZDate.Today;
				AssertConsolWithTaxInfo("PreCondition");
			}

			void AssertConsolWithTaxInfo(string commentInner, bool isTaxDateHaveValue = true, bool isTaxBranchHaveValue = true)
			{
				AssertEquals($"{commentInner}E6_AT_TaxRate", TestObjectCreator.GST1.PK, consolCost.E6_AT_TaxRate);

				if (isTaxDateHaveValue)
				{
					AssertEquals($"{commentInner}E6_TaxDate", ZDate.Today, consolCost.E6_TaxDate);
				}
				else
				{
					AssertEquals($"{commentPreppend}E6_TaxDate", ZDate.Empty, consolCost.E6_TaxDate);
				}

				if (isTaxBranchHaveValue)
				{
					AssertEquals($"{commentInner}E6_GB_CostTaxBranch", GlbBranch.CurrentBranch.PK, consolCost.E6_GB_CostTaxBranch);
				}
				else
				{
					AssertEquals($"{commentPreppend}E6_GB_CostTaxBranch", ZGuid.Empty, consolCost.E6_GB_CostTaxBranch);
				}
			}
		}

		JobConsolCost CreateConsolCost(ApportionmentListing apportionmentListing)
		{
			var cost = apportionmentListing.CostsCollection.TryAddNew();
			if (cost != null)
			{
				cost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
				cost.E6_ApportionmentMethod = ZArchitecture.Core.AllocationMethod.ChargeableUnits;
				cost.E6_OSCostAmount = 100m;
				cost.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
				cost.E6_InvoiceNum = "ABC123";
				cost.E6_InvoiceDate = ZDateTime.Today;
				cost.E6_PaymentDate = ZDateTime.Today.AddDays(2);
				cost.E6_CostReference = "COST";
				cost.E6_RX_NKCurrency = "AUD";
				cost.E6_AT_TaxRate = ObjectCreator.GSTFREE1.PK;
				cost.E6_A9_VATClass = Factory.NewWithValidTestData<AccInvMsg>().PK;
			}

			return cost;
		}

		JobConsolCost AddNewConsolCostToInvoice(ForwardingConsol consol, InvoicingBase invoiceToAddTo)
		{
			var cost = invoiceToAddTo.ConsolCosting.ConsolCosts.AddNew();

			if (consol != null)
			{
				using (cost.ReportSettingParentSuspender.GetSuspender())
				{
					cost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, JobConsolSchema.Constants.Prefix);
				}
			}

			return cost;
		}

		void AssertForConsoCostAndChargeAfterChangingInvoiceDetails(ApportionSplitCharge charge, JobConsolCost cost)
		{
			charge.JR_APInvoiceNum = "INV 256";
			AssertHasSynchronisedAPInvoiceDetailsAndIsPostedCorrectly(cost, false, false);
			charge.JR_APInvoiceNum = cost.E6_InvoiceNum;
			AssertHasSynchronisedAPInvoiceDetailsAndIsPostedCorrectly(cost, true, cost.IsPosted);
			charge.JR_OH_CostAccount = ObjectCreator.Creditor1.PK;
			AssertHasSynchronisedAPInvoiceDetailsAndIsPostedCorrectly(cost, false, false);
			charge.JR_OH_CostAccount = cost.E6_OH_Creditor;
			AssertHasSynchronisedAPInvoiceDetailsAndIsPostedCorrectly(cost, true, cost.IsPosted);
			charge.JR_APInvoiceDate = DateTime.Today.AddDays(1);
			if (cost.E6_AH_APInvoice.IsValid)
			{
				AssertEquals("count of error", 1, ExceptionReporterTestListener.Instance.Count);
				AssertEquals("key of error", "APInvoiceDateOnAppChargeSetToDifferentValueThanOnPostedParentConsolCost", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
			}

			AssertHasSynchronisedAPInvoiceDetailsAndIsPostedCorrectly(cost, false, false);
			charge.JR_APInvoiceDate = cost.E6_InvoiceDate;
			AssertHasSynchronisedAPInvoiceDetailsAndIsPostedCorrectly(cost, true, cost.IsPosted);
			charge.JR_PaymentDate = DateTime.Today.AddDays(5);
			AssertHasSynchronisedAPInvoiceDetailsAndIsPostedCorrectly(cost, false, false);
			charge.JR_PaymentDate = cost.E6_PaymentDate;
			AssertHasSynchronisedAPInvoiceDetailsAndIsPostedCorrectly(cost, true, cost.IsPosted);
			charge.JR_CostReference = "COSTREF##";
			AssertHasSynchronisedAPInvoiceDetailsAndIsPostedCorrectly(cost, false, false);
			charge.JR_CostReference = cost.E6_CostReference;
			AssertHasSynchronisedAPInvoiceDetailsAndIsPostedCorrectly(cost, true, cost.IsPosted);
			charge.JR_AT_CostGSTRate = ObjectCreator.GSTWithExtraRate.PK;
			AssertHasSynchronisedAPInvoiceDetailsAndIsPostedCorrectly(cost, false, false);
			charge.JR_AT_CostGSTRate = cost.E6_AT_TaxRate;
			charge.JR_A9_CostVATClass = cost.E6_A9_VATClass;
			AssertHasSynchronisedAPInvoiceDetailsAndIsPostedCorrectly(cost, true, cost.IsPosted);
			charge.JR_A9_CostVATClass = ObjectCreator.SVAT1.PK;
			AssertHasSynchronisedAPInvoiceDetailsAndIsPostedCorrectly(cost, false, false);
			charge.JR_A9_CostVATClass = cost.E6_A9_VATClass;
			AssertHasSynchronisedAPInvoiceDetailsAndIsPostedCorrectly(cost, true, cost.IsPosted);
		}

		void AssertForAPInvoiceAndChargeAfterChangingInvoiceDetails(ApportionSplitCharge charge, JobConsolCost cost)
		{
			var invoice = charge.APLine.TransactionHeader;
			invoice.AH_TransactionNum = "INV 256";
			AssertHasSynchronisedAPInvoiceDetailsAndIsPostedCorrectly(cost, false, false);
			invoice.AH_TransactionNum = charge.JR_APInvoiceNum;
			AssertHasSynchronisedAPInvoiceDetailsAndIsPostedCorrectly(cost, true, cost.IsPosted);
			invoice.AH_OH = ObjectCreator.Creditor1.PK;
			AssertHasSynchronisedAPInvoiceDetailsAndIsPostedCorrectly(cost, false, false);
			invoice.AH_OH = charge.JR_OH_CostAccount;
			AssertHasSynchronisedAPInvoiceDetailsAndIsPostedCorrectly(cost, true, cost.IsPosted);
			invoice.AH_InvoiceDate = DateTime.Today.AddDays(1);
			AssertHasSynchronisedAPInvoiceDetailsAndIsPostedCorrectly(cost, false, false);
			invoice.AH_InvoiceDate = charge.JR_APInvoiceDate;
			AssertHasSynchronisedAPInvoiceDetailsAndIsPostedCorrectly(cost, true, cost.IsPosted);
			invoice.AH_DueDate = DateTime.Today.AddDays(5);
			AssertHasSynchronisedAPInvoiceDetailsAndIsPostedCorrectly(cost, false, false);
			invoice.AH_DueDate = charge.JR_PaymentDate;
			AssertHasSynchronisedAPInvoiceDetailsAndIsPostedCorrectly(cost, true, cost.IsPosted);
			invoice.AH_TransactionReference = "COSTREF##";
			AssertHasSynchronisedAPInvoiceDetailsAndIsPostedCorrectly(cost, false, false);
			invoice.AH_TransactionReference = charge.JR_CostReference;
			AssertHasSynchronisedAPInvoiceDetailsAndIsPostedCorrectly(cost, true, cost.IsPosted);
			charge.APLine.AL_AT = ObjectCreator.GSTWithExtraRate.PK;
			AssertHasSynchronisedAPInvoiceDetailsAndIsPostedCorrectly(cost, false, false);
			charge.APLine.AL_AT = charge.JR_AT_CostGSTRate;
			charge.APLine.AL_A9_VATClass = charge.JR_A9_CostVATClass;
			AssertHasSynchronisedAPInvoiceDetailsAndIsPostedCorrectly(cost, true, cost.IsPosted);
			charge.APLine.AL_A9_VATClass = ObjectCreator.SVAT1.PK;
			AssertHasSynchronisedAPInvoiceDetailsAndIsPostedCorrectly(cost, false, false);
			charge.APLine.AL_A9_VATClass = charge.JR_A9_CostVATClass;
			AssertHasSynchronisedAPInvoiceDetailsAndIsPostedCorrectly(cost, true, cost.IsPosted);
		}

		void AssertHasSynchronisedAPInvoiceDetailsAndIsPostedCorrectly(JobConsolCost cost, bool expectedHasSynchronisedAPInvoiceDetailsPropValue, bool expectedIsPostedCorrectlyPropValue)
		{
			AssertEquals(string.Format("Expected HasSynchronisedAPInvoiceDetailsPropValue: {0}", expectedHasSynchronisedAPInvoiceDetailsPropValue.ToString()), expectedHasSynchronisedAPInvoiceDetailsPropValue, cost.HasSynchronisedAPInvoiceDetails);
			AssertEquals(string.Format("Expected IsPostedCorrectlyPropValue: {0}", expectedIsPostedCorrectlyPropValue.ToString()), expectedIsPostedCorrectlyPropValue, cost.IsPostedCorrectly);
		}

		void AssertSynchronizedApportionedCharges(APInvoice apInvoice, ApportionSplitCharge charge, AccTransactionLines aPLine)
		{
			AssertEquals(apInvoice.AH_TransactionNum, charge.JR_APInvoiceNum);
			AssertEquals(apInvoice.AH_OH, charge.JR_OH_CostAccount);
			AssertEquals(apInvoice.AH_InvoiceDate, charge.JR_APInvoiceDate);
			AssertEquals(apInvoice.AH_DocumentReceivedDate, charge.JR_APDocumentReceivedDate);
			AssertEquals(apInvoice.AH_DueDate, charge.JR_PaymentDate);
			AssertEquals(apInvoice.AH_TransactionReference, charge.JR_CostReference);
			AssertEquals(aPLine.AL_AT, charge.JR_AT_CostGSTRate);
			AssertEquals(aPLine.AL_A9_VATClass, charge.JR_A9_CostVATClass);
		}

		void AssertSynchronizedConsolCost(APInvoice apInvoice, JobConsolCost cost, ZGuid taxRatePK, ZGuid vATClassPK)
		{
			AssertEquals(apInvoice.AH_TransactionNum, cost.E6_InvoiceNum);
			AssertEquals(apInvoice.AH_OH, cost.E6_OH_Creditor);
			AssertEquals(apInvoice.AH_InvoiceDate, cost.E6_InvoiceDate);
			AssertEquals(apInvoice.AH_DocumentReceivedDate, cost.E6_DocumentReceivedDate);
			AssertEquals(apInvoice.AH_DueDate, cost.E6_PaymentDate);
			AssertEquals(apInvoice.AH_TransactionReference, cost.E6_CostReference);
			AssertEquals(taxRatePK, cost.E6_AT_TaxRate);
			AssertEquals(vATClassPK, cost.E6_A9_VATClass);
		}

		protected override void SetUp()
		{
			base.SetUp();
			ObjectCreator = new TestObjectCreator(Factory);

			var rate = AccTaxRate.Helper.FindTaxRate(new BusinessObjectFactory(), AccTaxRate.Helper.MainFreeGSTTaxRegistryID, Env.CurrentCompanyPK);
			rate.SetRateNumerator_ForTestOnly(0);
			rate.Factory.Save();
		}

		protected TestObjectCreator ObjectCreator;
		protected AccChequeBook GetAutoPrintChequeBook(AccBankAccount bankAccount)
		{
			bankAccount.AB_ChequeNumDigits = 1;
			bankAccount.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			BusinessObject printQueue = Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintQueue>());
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_AB = bankAccount.PK;
			chequeBook.AK_SQ = printQueue.PK;
			Assert("Cheque Book should be AutoPrint", chequeBook.IsAutoPrint);
			Factory.Save();
			return chequeBook;
		}

		void PrepareForPostingSynchroniseInvoiceDetailsCore(bool forAPInvoice)
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			var job1 = CreateJobAndCharge(shipment1);
			var job2 = CreateJobAndCharge(shipment2);
			Factory.Save();
			JobConsolCost cost = null;
			if (forAPInvoice)
			{
				APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
				cost = invoice.ConsolCosting.ConsolCosts.AddNew();
			}
			else
			{
				cost = (JobConsolCost)GetNewBusinessObject();
				using (cost.ReportSettingParentSuspender.GetSuspender())
				{
					cost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, consol.TablePrefix);
				}

				AddApportionedCharge(cost, job1.Charges[0]);
				AddApportionedCharge(cost, job2.Charges[0]);
			}

			SetConsolCostData(consol, cost);
			ResetCharge(cost.ApportionmentCharges[0], "345", ZDateTime.Today.AddDays(1), ObjectCreator.GSTFREE1);
			ResetCharge(cost.ApportionmentCharges[1], "567", ZDateTime.Today.AddDays(3), ObjectCreator.GSTFREE1);
			Factory.Save();
			AssertEquals("ApportionmentCharges.Count", 2, cost.ApportionmentCharges.Count);
			AssertInvoiceDetailsInfo(cost, cost.ApportionmentCharges[0]);
			AssertInvoiceDetailsInfo(cost, cost.ApportionmentCharges[1]);
		}

		void AddApportionedCharge(JobConsolCost cost, Charge jobCharge)
		{
			var appCharge = Factory.Load<ApportionSplitCharge>(jobCharge.PK);
			appCharge.JR_IsUsedForApportionment = true;
			cost.ApportionmentCharges.Add(appCharge);
			AssertEquals(cost.PK, appCharge.JR_E6);
		}

		Job CreateJobAndCharge(ForwardingShipment shipment)
		{
			var job = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			var jobCharge = job.Charges.AddNew();
			jobCharge.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge.JR_AC = ObjectCreator.CC1.PK;
			jobCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			jobCharge.JR_AT_CostGSTRate = ObjectCreator.GSTFREE1.PK;
			jobCharge.JR_OH_CostAccount = ObjectCreator.AALSHI.PK;
			jobCharge.JR_OH_SellAccount = ObjectCreator.ABIGAS.PK;
			return job;
		}

		void SetConsolCostData(ForwardingConsol consol, JobConsolCost cost)
		{
			cost.E6_GC = GlbCompany.CurrentCompany.PK;
			using (cost.ReportSettingParentSuspender.GetSuspender())
			{
				cost.E6_ParentID = consol.PK;
				cost.E6_ParentTableCode = "JK";
			}

			cost.E6_ApportionmentMethod = "SHP";
			cost.E6_OSCostAmount = 2560m;
			cost.E6_LocalCostAmount = 2560m;
			cost.E6_RX_NKCurrency = "AUD";
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			cost.E6_InvoiceNum = "12345";
			cost.E6_InvoiceDate = ZDateTime.Today.AddDays(1);
			cost.E6_PaymentDate = ZDateTime.Today.AddDays(2);
			cost.E6_CostReference = "COST";
			cost.E6_AT_TaxRate = ObjectCreator.GSTFREE1.PK;
			cost.E6_A9_VATClass = Factory.NewWithValidTestData<AccInvMsg>().PK;
		}

		void ResetCharge(ApportionSplitCharge jobCharge, string invoiceNum, ZDateTime paymentDate, AccTaxRate taxRate)
		{
			jobCharge.JR_APInvoiceNum = invoiceNum;
			jobCharge.JR_APInvoiceDate = ZDateTime.Today;
			jobCharge.JR_PaymentDate = paymentDate;
			jobCharge.JR_OH_CostAccount = ObjectCreator.Creditor1.PK;
			jobCharge.JR_CostReference = "COST1";
			jobCharge.JR_AT_CostGSTRate = taxRate.PK;
			jobCharge.JR_A9_CostVATClass = Factory.NewWithValidTestData<AccInvMsg>().PK;
		}

		void AssertInvoiceDetailsInfo(JobConsolCost cost, ApportionSplitCharge charge)
		{
			AssertEquals("JR_APInvoiceNum", cost.E6_InvoiceNum, charge.JR_APInvoiceNum);
			AssertEquals("JR_APInvoiceDate", cost.E6_InvoiceDate, charge.JR_APInvoiceDate);
			AssertEquals("JR_PaymentDate", cost.E6_PaymentDate, charge.JR_PaymentDate);
			AssertEquals("JR_OH_CostAccount", cost.E6_OH_Creditor, charge.JR_OH_CostAccount);
			AssertEquals("JR_CostReference", cost.E6_CostReference, charge.JR_CostReference);
			AssertEquals("JR_AT_CostGSTRate", cost.E6_AT_TaxRate, charge.JR_AT_CostGSTRate);
			AssertEquals("JR_A9_CostVATClass", cost.E6_A9_VATClass, charge.JR_A9_CostVATClass);
		}

		void SetGSTAndVATInfoChargeCode(TestObjectCreator creator, AccChargeCode code, AccTaxRate taxRate)
		{
			var taxRateMessage = Factory.NewWithValidTestData<AccInvMsg>();
			taxRateMessage.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate.AT_A9_DefaultVatClass = taxRateMessage.PK;
			code.AC_AT_GSTRate = taxRate.PK;
		}

		Job AddShipmentAndJobToConsol(ForwardingConsol consol)
		{
			var shipment = consol.Shipments.AddNew();
			var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			return job;
		}

		ForwardingShipment AddShipmentToConsolWithJob(ForwardingConsol consol)
		{
			var shipment = consol.Shipments.AddNew();
			var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			return shipment;
		}

		void AssertForAChargeCode(AccChargeCode code, AccTaxRate taxRate, JobConsolCost consolCost)
		{
			consolCost.E6_AC_ChargeCode = code.PK;
			AssertEquals("JR_AT_CostGSTRate has no Errors", false, consolCost.ApportionmentCharges[0].JR_AT_CostGSTRateInfo.HasErrors());
			AssertEquals("JR_AT_CostGSTRate has no Errors", false, consolCost.ApportionmentCharges[1].JR_AT_CostGSTRateInfo.HasErrors());
			AssertEquals("JR_AT_CostGSTRate on first Charge", taxRate.PK, consolCost.ApportionmentCharges[0].JR_AT_CostGSTRate);
			AssertEquals("JR_AT_CostGSTRate on second Charge", taxRate.PK, consolCost.ApportionmentCharges[1].JR_AT_CostGSTRate);
			AssertEquals("JR_A9_CostVATClass on first Charge", taxRate.AT_A9_DefaultVatClass, consolCost.ApportionmentCharges[0].JR_A9_CostVATClass);
			AssertEquals("JR_A9_CostVATClass on second Charge", taxRate.AT_A9_DefaultVatClass, consolCost.ApportionmentCharges[1].JR_A9_CostVATClass);
		}

		protected override void SetupDataForSettingValueCallsRefreshBindingTestIfNeeded(ZPropertyInfo info)
		{
			info.BizObj[JobConsolCost.Schema.E6_ParentTableCode] = JobConsolSchema.Constants.Prefix;
		}

		class JobConsolCostForOnFactorySavingBeforeTransactionTest : JobConsolCost
		{
			public JobConsolCostForOnFactorySavingBeforeTransactionTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override void OnFactorySavingBeforeTransactionCore2()
			{
				AssertMethodToRun();
			}

			public Action AssertMethodToRun { get; set; }
		}
	}

	[TestedType(typeof(JobConsolCost))]
	class JobConsolCostSupportCriticalValidationTest : SupportCriticalValidationTestBase
	{
		protected override IConflictWithCriticalFields GetNewBusinessObject()
		{
			return Factory.New<JobConsolCost>();
		}
	}

	class OSGSTCalculationStrategyStaticHelperTest : TestCaseWithFactory
	{
		public void TestCalculateTaxAmountCore()
		{
			var creator = new TestObjectCreator(Factory);

			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "INR"));

			var inCountryCode = Constants.CountryCodes.India;
			var inGST = creator.CreateTaxRate("INGST", "State GST", AccTaxRate.Types.Rated, 9, AccTaxRate.ExtraTypes.StateGST, 9, 1, inCountryCode);
			Assert("Precondition", inGST.IsIndiaStateTax);
			var taxAmountForIndia = OSGSTCalculationStrategyStaticHelper.CalculateTaxAmountCore(502.82m, 9m, 9m, currency, Factory, inGST.PK);
			AssertEquals(90.50m, taxAmountForIndia);

			var auCountryCode = Constants.CountryCodes.Australia;
			var auGST = creator.CreateTaxRate("AUGST", "State GST", AccTaxRate.Types.Rated, 9, AccTaxRate.ExtraTypes.StateGST, 9, 1, auCountryCode);
			Assert("Precondition", !auGST.IsIndiaStateTax);
			var taxAmountForNonIndia = OSGSTCalculationStrategyStaticHelper.CalculateTaxAmountCore(502.82m, 9m, 9m, currency, Factory, auGST.PK);
			AssertEquals(90.51m, taxAmountForNonIndia);

			var inGST2 = creator.CreateTaxRate("INGST2", "Service Tax", AccTaxRate.Types.Rated, 9, AccTaxRate.ExtraTypes.ServiceTax, 9, 1, inCountryCode);
			Assert("Precondition", !inGST2.IsIndiaStateTax);
			var taxAmountForIndia2 = OSGSTCalculationStrategyStaticHelper.CalculateTaxAmountCore(502.82m, 9m, 9m, currency, Factory, inGST2.PK);
			AssertEquals(90.51m, taxAmountForIndia2);

			var inGST3 = creator.CreateTaxRate("INGST3", "State GST", AccTaxRate.Types.ReverseRated, 9, AccTaxRate.ExtraTypes.StateGST, 9, 1, inCountryCode);
			Assert("Precondition", !inGST3.IsIndiaStateTax);
			var taxAmountForIndia3 = OSGSTCalculationStrategyStaticHelper.CalculateTaxAmountCore(502.82m, 9m, 9m, currency, Factory, inGST3.PK);
			AssertEquals(90.51m, taxAmountForIndia3);

			var taxAmountForIndia4 = OSGSTCalculationStrategyStaticHelper.CalculateTaxAmountCore(502.82m, 9m, 9m, currency, Factory, ZGuid.Empty);
			AssertEquals(0m, taxAmountForIndia4);
		}
	}
}

namespace Enterprise.Accounting.Business.ConsolCosting.Testing
{
	public class JobConsolCostPerformanceTest : TestCaseWithFactory
	{
		public void TestHaveConstructorStackTrace()
		{
			JobConsolCost cost = Factory.NewWithValidTestData<JobConsolCost>();
			IHaveConstructorStackTrace hasTrace = cost;
			AssertNotNull("Should be IHaveConstructorStackTrace", hasTrace);
			AssertNull("Should be no ConstructorStackTrace by default", hasTrace.ConstructorStackTrace);
			StackTrace trace = new StackTrace();
			hasTrace.ConstructorStackTrace = trace;
			AssertEquals("Should be assigned StackTrace", trace, hasTrace.ConstructorStackTrace);
			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.CollectConstructorCallStackDetails).Returns(true);
			using (ObjectFactory.Substitute(mock.Object))
			{
				hasTrace = Factory.NewWithValidTestData<JobConsolCost>();
				AssertNotNull("Should be IHaveConstructorStackTrace", hasTrace);
				AssertNotNull("Should have ConstructorStackTrace", hasTrace.ConstructorStackTrace);
				AssertContains("Trace should be as expected", trace.ToString(), hasTrace.ConstructorStackTrace.ToString());
			}
		}

		[ExpectNoExceptions]
		public void TestCostExchangeRate_LoadingCostExchangeRateCorrectlyWithProperProperties()
		{
			JobConsolCost cost = Factory.NewWithValidTestData<JobConsolCost>();
			cost.E6_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			var costExchangeRate = cost.CostExchangeRate;
			AssertNotNull(costExchangeRate);
			AssertEquals(true, costExchangeRate.IsRateRequired);
			AssertEquals(true, costExchangeRate.IsCurrencyRequired);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, cost.CostExchangeRate.Currency);
		}

		public void TestE6_Calc_PostingGroupId()
		{
			var cost = Factory.NewWithValidTestData<JobConsolCost>();
			cost.E6_AT_TaxRate = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, cost.E6_AT_TaxRate);
			AssertEquals(cost.E6_Calc_PostingGroupId, AccTaxRate.DefaultPostingGroupID);
			var taxRate = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "GST"));
			taxRate.AT_PostingGroupId = 1;
			cost.E6_AT_TaxRate = taxRate.PK;
			AssertEquals(taxRate.PK, cost.TaxRate.PK);
			AssertEquals(cost.E6_Calc_PostingGroupId, (ZShort)1);
		}

		AccTransactionHeader GetAPInvoice(AccTransactionHeader header)
		{
			var cost = Factory.NewWithValidTestData<JobConsolCost>();
			cost.E6_AH_APInvoice = header.PK;
			return cost.APInvoice;
		}

		public void TestAPInvoiceShouldBeNullForJC()
		{
			var creator = new TestObjectCreator(Factory);
			AssertNull("JC/JRJ", GetAPInvoice(creator.InsertTransaction(TransactionTypes.JobRevenueJournal, LedgerTypes.JobCosting)));
			AssertNotNull("JC/INV", GetAPInvoice(creator.InsertTransaction(TransactionTypes.Invoice, LedgerTypes.JobCosting)));
			AssertNotNull("GL/JRJ", GetAPInvoice(creator.InsertTransaction(TransactionTypes.JobRevenueJournal, LedgerTypes.General)));
		}

		public void TestChargeIsRemovedFromChargesToSearchWhenRemoveNonApplicableChargeSafe()
		{
			ErrorReporter.Clear();
			var charge = Factory.New<Charge>();
			using (var chargesToSearch = new TransactionLineJobChargeTransformer.ChargesByPK())
			{
				var cost = Factory.NewWithValidTestData<JobConsolCost>();
				var apportionedCharge = cost.ApportionmentCharges.AddNew();
				var loadedCharge = Factory.Load<Charge>(apportionedCharge.PK);
				chargesToSearch.AddCharge(loadedCharge);
				AssertEquals("PreCondition", 1, chargesToSearch.Charges.Count());
				Assert("PreCondition", loadedCharge.HasContext(BusinessContext.ReportDeletingCharges));
				AssertEquals("PreCondition", apportionedCharge.HasContext(BusinessContext.ReportDeletingCharges), loadedCharge.HasContext(BusinessContext.ReportDeletingCharges));
				cost.RemoveNonApplicableChargeSafe(apportionedCharge);
				AssertEquals("Charge removed from the collection", 0, chargesToSearch.Charges.Count());
				AssertEquals("No Error", 0, ErrorReporter.TotalErrorCount);
			}
		}

		public void TestCopyValues_JR_CostSupplyType()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = testObjectCreator.CreateConsol();
			var shipment = consol.Shipments.AddNew();
			var shipmentJob = testObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var consolCost = testObjectCreator.CreateConsolCost(consol, testObjectCreator.CC1, testObjectCreator.AUD, 1M, 500M);
			consolCost.E6_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC;

			AssertEquals("Precondition", 0, consolCost.ApportionmentCharges[0].InvoicingJob.Charges.Count);
			AssertEquals("Precondition", AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC, consolCost.ApportionmentCharges[0].JR_CostSupplyType);
			consolCost.PrepareForPosting();
			AssertEquals(1, consolCost.ApportionmentCharges[0].InvoicingJob.Charges.Count);
			AssertEquals(AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC, consolCost.ApportionmentCharges[0].InvoicingJob.Charges[0].JR_CostSupplyType);
		}

		public void TestPopulateMissingApportionments_JR_CostSupplyType()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = testObjectCreator.CreateConsol();
			var shipment = testObjectCreator.CreateShipment("S001", consol);
			var job = testObjectCreator.CreateJob(shipment, false);
			var consolCost1 = testObjectCreator.CreateConsolCost(consol, testObjectCreator.CC1, 100, testObjectCreator.Creditor1);
			var consolCost2 = testObjectCreator.CreateConsolCost(consol, testObjectCreator.CC1, 100);
			consolCost2.E6_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC;
			consolCost2.ApportionmentCharges.RemoveAndDeleteAll();

			AssertEquals("Precondition", 0, consolCost2.ApportionmentCharges.Count);
			consolCost2.PopulateMissingApportionments(consolCost1);
			AssertEquals(AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC, consolCost2.ApportionmentCharges[0].JR_CostSupplyType);
		}

		public void TestJobConsolCostAPInvoiceChangeWhenCostAccountIsDifferentFromJobChargeCreditorCritivalValidationError()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.ChargeSetsCreditorDifferentToConsolCost);

			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			testObjectCreator.CreateJob(shipment, false);
			var consolCost = testObjectCreator.CreateConsolCost(consol, testObjectCreator.CC1, 100, testObjectCreator.Creditor1);

			var appotionmentCharge = consolCost.ApportionmentCharges[0];
			appotionmentCharge.JR_OH_CostAccount = ZGuid.NewZGuid();

			consolCost.E6_AH_APInvoice = ZGuid.NewZGuid();
			consolCost.E6_OH_Creditor = ZGuid.Empty;
			var charge = Factory.Load<Charge>(appotionmentCharge.PK);
			charge.JR_OH_CostAccount = ZGuid.NewZGuid();

			var errorInfo = CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfoSafe(consolCost.PK, CriticalValidationInfoCollectorServiceKeyType.ChargeSetsCreditorDifferentToConsolCost);

			AssertContains($@"Original AP invoice '00000000-0000-0000-0000-000000000000' New AP invoice '{consolCost.E6_AH_APInvoice}'.
Call stack:
", errorInfo);
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).ClearServiceCache();
		}

		#region Tax Branch

		public void TestSetTaxBranchDefault()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.Creditor1.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			testObjectCreator.Creditor2.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;
			Factory.Save();

			var consol = testObjectCreator.CreateConsol();
			var shipment = testObjectCreator.CreateShipment("S001", consol);
			var job = testObjectCreator.CreateJob(shipment, false);
			var consolCost = testObjectCreator.CreateConsolCost(consol, testObjectCreator.CC1, 100, testObjectCreator.Creditor1);

			AssertCheckE6_GB_CostTaxBranch(true, true);
			AssertCheckE6_GB_CostTaxBranch(true, false);
			AssertCheckE6_GB_CostTaxBranch(false, true);
			AssertCheckE6_GB_CostTaxBranch(false, false);

			void AssertCheckE6_GB_CostTaxBranch(bool enableTaxBranchReporting, bool gstRegistered)
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = gstRegistered;
				using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableTaxBranchReporting))
				{
					testObjectCreator.ResetSecurityCore();

					consolCost.E6_OH_Creditor = ZGuid.Empty;
					AssertEquals("Precondition", ZGuid.Empty, consolCost.E6_GB_CostTaxBranch);

					consolCost.E6_OH_Creditor = testObjectCreator.Creditor1.PK;
					AssertEquals(enableTaxBranchReporting && gstRegistered ? GlbBranch.CurrentBranch.PK : ZGuid.Empty, consolCost.E6_GB_CostTaxBranch);

					consolCost.E6_OH_Creditor = testObjectCreator.Creditor2.PK;
					AssertEquals(ZGuid.Empty, consolCost.E6_GB_CostTaxBranch);
				}
			}
		}

		public void TestSetTaxBranchDefaultWhenReadOnly()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.Creditor1.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			testObjectCreator.Creditor2.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;
			Factory.Save();

			var consol = testObjectCreator.CreateConsol();
			var shipment = testObjectCreator.CreateShipment("S001", consol);
			var job = testObjectCreator.CreateJob(shipment, false);
			var consolCost = testObjectCreator.CreateConsolCost(consol, testObjectCreator.CC1, 100, testObjectCreator.Creditor1);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			using (testObjectCreator.SetUpTaxBranchRegistry(true))
			{
				testObjectCreator.ResetSecurityCore();
				Env.Security.MaintainConsolJobInvoicingAllowOverrideCostTaxBranch.IsAllowed = false;
				Assert(consolCost.E6_GB_CostTaxBranchInfo.ReadOnly);

				consolCost.E6_OH_Creditor = ZGuid.Empty;
				AssertEquals("Precondition", ZGuid.Empty, consolCost.E6_GB_CostTaxBranch);

				consolCost.E6_OH_Creditor = testObjectCreator.Creditor1.PK;
				AssertEquals(GlbBranch.CurrentBranch.PK, consolCost.E6_GB_CostTaxBranch);

				consolCost.E6_OH_Creditor = testObjectCreator.Creditor2.PK;
				AssertEquals(ZGuid.Empty, consolCost.E6_GB_CostTaxBranch);
			}
		}

		public void TestE6_GB_CostTaxBranch()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var job = new Job.Loader(shipment).TryLoadOrCreate();
			Factory.Save();

			AssertE6_GB_CostTaxBranch(true);
			AssertE6_GB_CostTaxBranch(false);

			void AssertE6_GB_CostTaxBranch(bool enableTaxBranchReporting)
			{
				using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableTaxBranchReporting))
				{
					var apps = new ApportionmentListing(Factory, consol);

					var cost = apps.CostsCollection.TryAddNew();
					AssertEquals(ZGuid.Empty, cost.E6_GB_CostTaxBranch);
					AssertEquals(1, cost.ApportionmentCharges.Count);

					cost.E6_GB_CostTaxBranch = testObjectCreator.NonCurrentBranch.PK;
					AssertEquals(enableTaxBranchReporting ? cost.E6_GB_CostTaxBranch : ZGuid.Empty, cost.ApportionmentCharges[0].JR_GB_CostTaxBranch);
				}
			}
		}

		public void TestCopyValues_JR_GB_CostTaxBranch()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = testObjectCreator.CreateConsol();
			Factory.Save();

			AssertCopyValues_JR_GB_CostTaxBranch(true);
			AssertCopyValues_JR_GB_CostTaxBranch(false);

			void AssertCopyValues_JR_GB_CostTaxBranch(bool enableTaxBranchReporting)
			{
				using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableTaxBranchReporting))
				{
					consol.Shipments.ForEach(x => ((ForwardingShipment)x).Job.Delete());
					consol.Shipments.RemoveAndDeleteAll();
					var shipment = consol.Shipments.AddNew();
					var shipmentJob = testObjectCreator.CreateJob(shipment, false);
					var consolCost = testObjectCreator.CreateConsolCost(consol, testObjectCreator.CC1, testObjectCreator.AUD, 1M, 500M);
					consolCost.ApportionmentCharges[0].InvoicingJob.Charges.RemoveAndDeleteAll();
					if (enableTaxBranchReporting)
					{
						consolCost.E6_GB_CostTaxBranch = testObjectCreator.NonCurrentBranch.PK;
					}
					consolCost.ApportionmentCharges[0].JR_GB_CostTaxBranch = testObjectCreator.NonCurrentBranch.PK;

					AssertEquals("Precondition", 0, consolCost.ApportionmentCharges[0].InvoicingJob.Charges.Count);
					AssertEquals("Precondition", testObjectCreator.NonCurrentBranch.PK, consolCost.ApportionmentCharges[0].JR_GB_CostTaxBranch);
					consolCost.PrepareForPosting();
					AssertEquals(1, consolCost.ApportionmentCharges[0].InvoicingJob.Charges.Count);
					AssertEquals(enableTaxBranchReporting ? testObjectCreator.NonCurrentBranch.PK : ZGuid.Empty, consolCost.ApportionmentCharges[0].InvoicingJob.Charges[0].JR_GB_CostTaxBranch);
				}
			}
		}

		public void TestPopulateMissingApportionments_JR_GB_CostTaxBranch()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = testObjectCreator.CreateConsol();
			var shipment = testObjectCreator.CreateShipment("S001", consol);
			var job = testObjectCreator.CreateJob(shipment, false);

			AssertPopulateMissingApportionments_JR_GB_CostTaxBranch(true);
			AssertPopulateMissingApportionments_JR_GB_CostTaxBranch(false);

			void AssertPopulateMissingApportionments_JR_GB_CostTaxBranch(bool enableTaxBranchReporting)
			{
				using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableTaxBranchReporting))
				{
					var consolCost1 = testObjectCreator.CreateConsolCost(consol, testObjectCreator.CC1, 100, testObjectCreator.Creditor1);
					var consolCost2 = testObjectCreator.CreateConsolCost(consol, testObjectCreator.CC1, 100);
					consolCost2.E6_GB_CostTaxBranch = testObjectCreator.NonCurrentBranch.PK;
					consolCost2.ApportionmentCharges.RemoveAndDeleteAll();

					AssertEquals("Precondition", 0, consolCost2.ApportionmentCharges.Count);
					consolCost2.PopulateMissingApportionments(consolCost1);
					AssertEquals(enableTaxBranchReporting ? testObjectCreator.NonCurrentBranch.PK : ZGuid.Empty, consolCost2.ApportionmentCharges[0].JR_GB_CostTaxBranch);
				}
			}
		}

		#endregion

		#region Test HTML Properties

		public void TestHtmlProperty()
		{
			var jobConsolCost = Factory.NewWithValidTestData<JobConsolCost>();
			AssertEquals(ZBlob.Empty, jobConsolCost.CostCalculationDescription);
			AssertEquals(ZBlob.Empty, jobConsolCost.CostCalculationDescription_HTML);

			jobConsolCost.CostCalculationDescription_HTML = ZBlob.FromUTF8("<p>123</p>");

			AssertEquals(@"{\rtf1\ansi\ansicpg1252\deflang3081\nouicompat\uc0{\fonttbl}{\colortbl}{{123}\par}}", ORtfTextUtil.GeneratorInfoRegex.Replace(jobConsolCost.CostCalculationDescription.ToUTF8(), string.Empty));
			AssertEquals("<p>123</p>", jobConsolCost.CostCalculationDescription_HTML.ToUTF8());
		}

		public void TestHtmlFromTextProperty()
		{
			var jobConsolCost = Factory.NewWithValidTestData<JobConsolCost>();
			AssertEquals(ZBlob.Empty, jobConsolCost.CostCalculationDescription);
			AssertEquals(ZBlob.Empty, jobConsolCost.CostCalculationDescription_HTML);

			jobConsolCost.CostCalculationDescription = ZBlob.FromUTF8("1234\r\n5678");

			AssertEquals("<p>1234</p><p>5678</p>", jobConsolCost.CostCalculationDescription_HTML.ToUTF8());

			jobConsolCost.CostCalculationDescription = ZBlob.FromUTF8("{\\rtf1\\test\\ansi\\ansicpg1252\\nouicompat\\deflang3081\r\n{\\*\\generator Riched20 10.0.19041}\\viewkind4\\uc1 \\pard rtf\\par\r\n}\r\n");

			AssertEquals("<p>rtf</p>", jobConsolCost.CostCalculationDescription_HTML.ToUTF8());
		}

		#endregion
	}
}
