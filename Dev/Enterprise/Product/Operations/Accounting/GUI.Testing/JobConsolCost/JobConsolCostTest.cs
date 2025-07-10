using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.RatingTests.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	class JobConsolCostTest : TestCaseWithFactory
	{
		public void TestFilteredIndexCanBeUsed()
		{
			var query = new ZQuery(JobConsolCostSchema.E6_CostReference, "123");
			Factory.Load<Business.ConsolCosting.JobConsolCost>(query);
			Assert("Filtered index requires either literal use, or proof the parameter does not contain the excluded values",
				SqlEventTracker.Instance.LastSqlEvent.Contains("E6_CostReference <> ''") ||
				SqlEventTracker.Instance.LastSqlEvent.Contains("E6_CostReference = '"));
		}

		public void TestGetUserContextForCorrectCompanyWhenCostCompanyBranchesAreAllInactive()
		{
			var consolCostPK = CreateConsolCostInADifferentCompany();
			var consolCostReloaded = Factory.Load<Business.ConsolCosting.JobConsolCost>(consolCostPK);
			consolCostReloaded.Company.ActiveBranches.ForEach(x => x.GB_IsActive = false);

			AssertNotEquals(GlbCompany.CurrentCompany.PK, consolCostReloaded.E6_GC);
			Assert(consolCostReloaded.E6_GC.IsValid);
			AssertEquals("Cost Company Branches Are All Inactive", 0, consolCostReloaded.Company.ActiveBranches.ToList().Count);

			ErrorReporter.Clear();
			Factory.Save();
			AssertEquals("No dev exception thrown", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestGetUserContextForCorrectCompanyWhenCostCompanyPKIsNull()
		{
			var consolCostPK = CreateConsolCostInADifferentCompany();
			var consolCostReloaded = Factory.Load<Business.ConsolCosting.JobConsolCost>(consolCostPK);
			consolCostReloaded.E6_GC = ZGuid.BrettsGuid;

			AssertNotEquals(GlbCompany.CurrentCompany.PK, consolCostReloaded.E6_GC);
			Assert(consolCostReloaded.E6_GC.IsValid);
			AssertNull("Cost company is null", consolCostReloaded.Company);

			ErrorReporter.Clear();
			AssertExceptionThrown<ZSaveException>(() => Factory.Save());
			AssertEquals("No dev exception thrown", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestGetUserContextForCorrectCompanyWhenCostCompanyPKIsInvalid()
		{
			var consolCostPK = CreateConsolCostInADifferentCompany();
			var consolCostReloaded = Factory.Load<Business.ConsolCosting.JobConsolCost>(consolCostPK);
			consolCostReloaded.E6_GC = ZGuid.Invalid;

			AssertNotEquals(GlbCompany.CurrentCompany.PK, consolCostReloaded.E6_GC);
			Assert("Cost company PK is invalid", !consolCostReloaded.E6_GC.IsValid);

			ErrorReporter.Clear();
			AssertExceptionThrown<ZSaveException>(() => Factory.Save());
			AssertEquals("No dev exception thrown", 0, ErrorReporter.TotalErrorCount);
		}

		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[DisableZeroExchangeRateOverriding]
		public void TestGetUserContextForCorrectCompanyWhenCostApportionmentChargeBranchIsNull()
		{
			var consolCostPK = CreateConsolCostInADifferentCompany();
			var consolCostReloaded = Factory.Load<Business.ConsolCosting.JobConsolCost>(consolCostPK);
			var chargeReloaded = consolCostReloaded.ApportionmentCharges.Cast<ApportionSplitCharge>().FirstOrDefault();
			chargeReloaded.JR_GB = ZGuid.Empty;

			AssertNotEquals(GlbCompany.CurrentCompany.PK, consolCostReloaded.E6_GC);
			Assert(consolCostReloaded.E6_GC.IsValid);
			AssertNull("Cost apportionment charge branch is null", chargeReloaded.Branch);

			ErrorReporter.Clear();
			AssertExceptionThrown<ZSaveException>(() => Factory.Save());
			AssertEquals("No dev exception thrown", 0, ErrorReporter.TotalErrorCount);
		}

		ZGuid CreateConsolCostInADifferentCompany()
		{
			var differentCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK) { OrderBy = GlbCompanySchema.Constants.GC_Code });
			var differentCompanyBranch = differentCompany.ActiveBranches.FirstOrDefault();
			AssertNotNull(differentCompanyBranch);
			Business.ConsolCosting.JobConsolCost consolCost;

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var newFactory = new BusinessObjectFactory();
				var testObjectCreator = new TestObjectCreator(newFactory);
				var consol = testObjectCreator.CreateConsol();
				var shipment = testObjectCreator.CreateShipment("S0001", consol);
				var job = testObjectCreator.CreateJob(shipment);
				testObjectCreator.AALSHI.CompanyData.OB_RX_NKAPDefltCurrency = "AUD";

				consolCost = testObjectCreator.CreateConsolCost(consol, testObjectCreator.CC11, 100);
				var charge = consolCost.ApportionmentCharges.AddNew();
				charge.JR_JH = job.PK;

				consolCost.E6_AC_ChargeCode = testObjectCreator.DSBChargeCode.PK;
				consolCost.E6_RX_NKCurrency = "USD";
				consolCost.E6_ExchangeRate = 5;
				consolCost.E6_OSCostAmount = 100;
				newFactory.Save();
			}
			return consolCost.PK;
		}

		public void TestJobChargeExchangeRateDeveloperExceptionShouldNotBeReportedWhenSettingCreditor()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = testObjectCreator.CreateConsol();
			var shipment = testObjectCreator.CreateShipment("S0001", consol);
			var job = testObjectCreator.CreateJob(shipment);
			testObjectCreator.AALSHI.CompanyData.OB_RX_NKAPDefltCurrency = "AUD";

			var consolCost = testObjectCreator.CreateConsolCost(consol, testObjectCreator.CC11, 100);
			var charge = consolCost.ApportionmentCharges.AddNew();
			charge.JR_JH = job.PK;

			consolCost.E6_AC_ChargeCode = testObjectCreator.DSBChargeCode.PK;
			consolCost.E6_RX_NKCurrency = "USD";
			consolCost.E6_ExchangeRate = 5;
			consolCost.E6_OSCostAmount = 100;

			Factory.Save();

			AssertEquals("Precondition: Local currency of current company should be AUD.", "AUD", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			Assert("Precondition: Charge type should be DSB", consolCost.ChargeCode.IsDisbursement);

			ErrorReporter.Clear();
			consolCost.E6_OH_Creditor = testObjectCreator.AALSHI.PK;
			AssertEquals("No dev exception thrown", 0, ErrorReporter.TotalErrorCount);
			AssertEquals("the exchange rate is changed according to the new creditor company currency", 1m, consolCost.E6_ExchangeRate);
		}

		public void TestJobConsolCost_WithholdingTax()
		{
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = true;

			var testObjectCreator = new TestObjectCreator(Factory);

			var chargeCode = testObjectCreator.CC3;
			var wht = testObjectCreator.WHT1;
			AssertEquals("Precondition", wht.PK, chargeCode.AC_AW_WithholdingTaxRate);

			var creditor = testObjectCreator.AALSHI;
			Assert("Precondition", creditor.CompanyData.OB_APWHTApplicable);

			var nonAPWHTOrg = testObjectCreator.ABIGAS;
			Assert("Precondition", !nonAPWHTOrg.CompanyData.OB_APWHTApplicable);

			var consol = testObjectCreator.CreateConsol();

			var shipment1 = testObjectCreator.CreateShipment("S0001", consol);
			var job1 = testObjectCreator.CreateJob(shipment1, false);

			var shipment2 = testObjectCreator.CreateShipment("S0002", consol);
			var job2 = testObjectCreator.CreateJob(shipment2, false);

			var consolCost = testObjectCreator.CreateConsolCost(consol, chargeCode, 0);

			consolCost.E6_AC_ChargeCode = chargeCode.PK;
			AssertEquals("WHT tax not set as creditor is not set", ZGuid.Empty, consolCost.E6_AW);

			consolCost.E6_OH_Creditor = creditor.PK;
			AssertEquals("WHT tax set as creditor is set and creditor is AP WHT applicable", wht.PK, consolCost.E6_AW);

			consolCost.E6_OH_Creditor = nonAPWHTOrg.PK;
			AssertEquals("WHT tax not set as creditor is not AP WHT applicable", ZGuid.Empty, consolCost.E6_AW);

			consolCost.E6_OH_Creditor = creditor.PK;
			AssertEquals("WHT tax set as creditor is set and creditor is AP WHT applicable", wht.PK, consolCost.E6_AW);

			AssertEquals(0M, consolCost.E6_OSWHTAmount);

			consolCost.E6_OSCostAmount = 100M;
			AssertEquals(5M, consolCost.E6_OSWHTAmount);

			consolCost.E6_OSCostAmount = 200M;
			AssertEquals(10M, consolCost.E6_OSWHTAmount);

			foreach (ApportionSplitCharge apportionmentCharge in consolCost.ApportionmentCharges)
			{
				AssertEquals(wht.PK, apportionmentCharge.JR_AW_CostWHTRate);
				AssertEquals(5M, apportionmentCharge.JR_OSCostWHTAmt);
			}

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var consolCostInNewFactory = newFactory.Load<Business.ConsolCosting.JobConsolCost>(consolCost.PK);

			AssertEquals(wht.PK, consolCostInNewFactory.E6_AW);
			AssertEquals(10M, consolCostInNewFactory.E6_OSWHTAmount);

			foreach (ApportionSplitCharge apportionmentCharge in consolCostInNewFactory.ApportionmentCharges)
			{
				AssertEquals(wht.PK, apportionmentCharge.JR_AW_CostWHTRate);
				AssertEquals(5M, apportionmentCharge.JR_OSCostWHTAmt);
			}
		}

		public void TestE6_AW_ReadOnly()
		{
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = true;
			AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyWHTId.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var testObjectCreator = new TestObjectCreator(Factory);

			var chargeCode = testObjectCreator.CC3;
			var wht = testObjectCreator.WHT1;
			AssertEquals("Precondition", wht.PK, chargeCode.AC_AW_WithholdingTaxRate);

			var creditor = testObjectCreator.AALSHI;
			Assert("Precondition", creditor.CompanyData.OB_APWHTApplicable);

			var nonAPWHTOrg = testObjectCreator.ABIGAS;
			Assert("Precondition", !nonAPWHTOrg.CompanyData.OB_APWHTApplicable);

			var consol = testObjectCreator.CreateConsol();

			var shipment = testObjectCreator.CreateShipment("S0001", consol);
			var job1 = testObjectCreator.CreateJob(shipment, false);

			var consolCost = testObjectCreator.CreateConsolCost(consol, chargeCode, 0);
			Assert("E6_AW readonly since no creditor or charge code is selected", consolCost.E6_AWInfo.ReadOnly);

			consolCost.E6_AC_ChargeCode = chargeCode.PK;
			Assert("E6_AW readonly since charge code is selected", consolCost.E6_AWInfo.ReadOnly);

			consolCost.E6_OH_Creditor = nonAPWHTOrg.PK;
			Assert("E6_AW readonly since non AP WHT creditor is selected", consolCost.E6_AWInfo.ReadOnly);

			consolCost.E6_OH_Creditor = creditor.PK;
			Assert("E6_AW is editable", !consolCost.E6_AWInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyWHTId.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			Assert("E6_AW readonly since user cannot override", consolCost.E6_AWInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyWHTId.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;
			Assert("E6_AW readonly since company is not WHT registered", consolCost.E6_AWInfo.ReadOnly);

			GlbCompany.CurrentCompany.GC_IsWHTRegistered = true;
			consolCost.E6_AH_APInvoice = ZGuid.NewZGuid();

			Assert("E6_AW readonly since consol cost is already posted", consolCost.E6_AWInfo.ReadOnly);
		}

		public void TestApportionmentByCapacityPerContainer_ImportConsolCostWithAutoRatingLog()
		{
			var creator = new TestObjectCreator(Factory);
			var ratingTest = new RatingTest();

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;

			var costing = Factory.New<Costing>();
			costing.TH_OH = creditor.PK;

			var costEntry20GP = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "USLAX", "", "20GP");
			costEntry20GP.TI_RX_NKCurrency = "AUD";
			costEntry20GP.RateLines.RemoveAndDeleteAll();
			costEntry20GP.AddRateLine("FRT", UnitCalculator.Code, "CN").GetCalculator<UnitCalculator>().PerUnit = 1250m;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
			consol.JK_OA_ShippingLineAddress = creditor.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_Vessel = "ADMIRAL";
			consol.Transports[0].JW_VoyageFlight = "123S";
			consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			consol.Transports[0].JW_RL_NKDiscPort = "USLAX";
			consol.Transports[0].JW_ETA = ZDateTime.Now;
			consol.Transports[0].JW_ETD = ZDateTime.Now.AddDays(1);
			consol.Transports[0].JW_IsLinked = ZBool.True;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT00001";
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			var container2 = consol.Containers.AddNew();
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container2.JC_ContainerNum = "CONT00002";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_FreightSpotRateAutoratingMode = Constants.FreightRateAutoratingModes.Code.StandardRate;
			shipment1.JS_TransportMode = Constants.TransportModes.Sea;
			shipment1.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment1.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";
			shipment1.JS_INCO = "CFR";
			shipment1.JS_OH_DeliveryAgent = consignee.PK;
			shipment1.JS_ActualVolume = 15m;
			shipment1.ConsignorPK = consignor.PK;
			shipment1.ConsigneePK = consignee.PK;

			var packline1_1 = shipment1.OuterPackLines.AddNew();
			packline1_1.JL_JC = container1.PK;
			packline1_1.JL_RH_NKCommodityCode = "GEN";
			packline1_1.JL_ActualVolumeUQ = "M3";
			packline1_1.JL_ActualVolume = 10m;

			var packline1_2 = shipment1.OuterPackLines.AddNew();
			packline1_2.JL_JC = container2.PK;
			packline1_2.JL_RH_NKCommodityCode = "GEN";
			packline1_2.JL_ActualVolumeUQ = "M3";
			packline1_2.JL_ActualVolume = 5m;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_FreightSpotRateAutoratingMode = Constants.FreightRateAutoratingModes.Code.StandardRate;
			shipment2.JS_TransportMode = Constants.TransportModes.Sea;
			shipment2.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment2.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment2.JS_RL_NKOrigin = "AUSYD";
			shipment2.JS_RL_NKDestination = "USLAX";
			shipment2.JS_INCO = "CFR";
			shipment2.JS_OH_DeliveryAgent = consignee.PK;
			shipment2.JS_ActualVolume = 15m;
			shipment2.ConsignorPK = consignor.PK;
			shipment2.ConsigneePK = consignee.PK;

			var packline2_1 = shipment2.OuterPackLines.AddNew();
			packline2_1.JL_JC = container1.PK;
			packline2_1.JL_RH_NKCommodityCode = "GEN";
			packline2_1.JL_ActualVolumeUQ = "M3";
			packline2_1.JL_ActualVolume = 7m;

			var packline2_2 = shipment1.OuterPackLines.AddNew();
			packline2_2.JL_JC = container2.PK;
			packline2_2.JL_RH_NKCommodityCode = "GEN";
			packline2_2.JL_ActualVolumeUQ = "M3";
			packline2_2.JL_ActualVolume = 8m;

			Factory.Save();

			using (var job1 = new Job.Loader(shipment1).TryLoadOrCreateWithoutMutexForTestOnly())
			using (var job2 = new Job.Loader(shipment2).TryLoadOrCreateWithoutMutexForTestOnly())
			{
				job1.JH_OA_LocalChargesAddr = consignor.MainAddress.PK;
				job2.JH_OA_LocalChargesAddr = consignor.MainAddress.PK;

				Factory.Save();

				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 2500m,
					}
				};

				ratingTest.AutoCostAndAssert_Exposed("Total cost of both rate entries with FRT charge applied to the consol", null, expectedCosts, consol);

				var consolCost = Factory.Load<Business.ConsolCosting.JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK)).FirstOrDefault();

				AssertEquals(2, consolCost.ApportionmentCharges.Count);

				consolCost.ApportionmentCharges[0].JR_IsUsedForApportionment = true;
				consolCost.ApportionmentCharges[1].JR_IsUsedForApportionment = true;

				consolCost.E6_ApportionmentMethod = AllocationMethod.CapacityPerContainer;

				Factory.Save();

				var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
				{
					{
						shipment1, new[]
						{
							new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSCostAmt = 1914.89m
							}
						}
					},
					{
						shipment2, new[]
						{
							new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSCostAmt = 585.11m
							}
						}
					}
				};

				ratingTest.AssertCharges_Exposed("Charges should be apportioned based on what fraction of total volume loaded into a container each shipment has contributed.", expectedCharges);

				Factory.Save();

				var invoice = Factory.NewWithValidTestData<APInvoice>();
				invoice.AH_OH = creditor.PK;
				using (var invoiceForm = new InvoiceForm(invoice))
				{
					invoiceForm.Show();
					invoiceForm.InvoiceDetails.ApportionChargesButton.PerformClick();
					AssertEquals("Should be showing the apportionment form", typeof(APInvoiceConsolCostingForm), ZFormModaliser.ActiveForm.GetType());
					var cost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
					cost.E6_AC_ChargeCode = creator.FRT.PK;

					//Can't use AssertNoWarning because it needs a AssertHasWarning as a match...
					Assert("Shouldn't have the warning any more, we have used the original JobConsolCost.", !cost.E6_ApportionmentMethodInfo.Notifications.Any());
					AssertEquals("Should be equal to the value in the consol.", 1914.89m, cost.ApportionmentCharges[0].JR_OSCostAmt);
					AssertEquals("Should be equal to the value in the consol.", 585.11m, cost.ApportionmentCharges[1].JR_OSCostAmt);
				}
			}
		}

		[TestDate(2018, 11, 21)]
		public void TestGetExchangeRateIgnoreLocalClientInSetCurrency()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "SOME ORG";

			Factory.Save();

			var testObjectCreator = new TestObjectCreator(Factory);

			testObjectCreator.CreateExchangeRate(Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD"), "BUY", 1.2M, new ZDateTime(2018, 11, 02), ZDateTime.MaxSmallDateTime, orgHeader.PK);

			testObjectCreator.CreateExchangeRate(Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD"), "BUY", 1.3M, new ZDateTime(2018, 11, 01), ZDateTime.MaxSmallDateTime);

			var consol = testObjectCreator.CreateConsol();

			var consolCost = testObjectCreator.CreateConsolCost(consol, testObjectCreator.CC11, 100);

			consolCost.E6_RX_NKCurrency = "USD";

			AssertEquals("Ignore any exchange rate linked to local client for consol cost.", 1.3M, consolCost.E6_ExchangeRate);
		}

		[TestDate(2020, 8, 1)]
		public void TestE6_AT_TaxRate_DelayedListForCheckingDatesMismatch()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C0001");
			var shipment = TestObjectCreator.CreateShipment("S0001", consol);
			TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			var jobConsolCost = consol.GetApportionments().CostsCollection.TryAddNew();
			jobConsolCost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			jobConsolCost.E6_OH_Creditor = TestObjectCreator.Creditor1.PK;
			jobConsolCost.E6_AT_TaxRate = TestObjectCreator.GSTFREE1.PK;
			jobConsolCost.E6_TaxDate = ZDate.Today;

			jobConsolCost.ApportionmentCharges.AddNew();

			ErrorReporter.Clear();
			jobConsolCost.E6_AT_TaxRate = TestObjectCreator.GST1.PK;
			AssertEquals("An error should not be reported", string.Empty, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			using (jobConsolCost.GetSuspenderForConsolCostImporter())
			{
				jobConsolCost.E6_AT_TaxRate = TestObjectCreator.GST2.PK;
				AssertEquals("An error should not be reported", string.Empty, ErrorReporter.LastMessageReported);
			}
		}

		public class RatingTest : BaseRatingIntegrationTest
		{
			public void AutoCostAndAssert_Exposed(string message,
				Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>> expectedInvoicingCharges,
				IEnumerable<AssertionCost> expectedCosts, IGenericJobCostPlugIn costsSupporter, bool autorateRevenue = true,
				bool autorateCosts = true, bool deleteExistingCosts = true)
			{
				AutoCostAndAssert(message, expectedInvoicingCharges, expectedCosts, costsSupporter, autorateRevenue, autorateCosts, deleteExistingCosts);
			}

			public void AssertCharges_Exposed(string message, Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>> expectedInvoicingCharges)
			{
				AssertCharges(message, expectedInvoicingCharges);
			}
		}

		TestObjectCreator TestObjectCreator => fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator fTestObjectCreator;
	}
}
