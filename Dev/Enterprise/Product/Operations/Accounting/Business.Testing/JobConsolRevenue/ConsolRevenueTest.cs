using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ConsolRevenue.Testing
{
	using System;
	using System.Linq;
	using CargoWise.Application;
	using CargoWise.Integration;
	using Enterprise.Accounting.Business.ARAP.Invoicing;
	using Enterprise.Accounting.Registry.Business;
	using Enterprise.Integration.TransportBooking;
	using Enterprise.TransportCommon.Shared;
	using NUnit.Framework;

	[TestedType(typeof(ConsolRevenue))]
	public class ConsolRevenueTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ConsolRevenue(new ConsolRevenueMaster(Factory.New<ForwardingConsol>(), Factory));
		}

		public void TestDescription_ReadOnly()
		{
			TestObjectCreator.CC1.AC_AllowDescriptionOvertype = true;
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var consolRevenueMaster = new ConsolRevenueMaster(consol, Factory);
			var consolRevenue = new ConsolRevenue(consolRevenueMaster);
			consolRevenue.ChargeCode = TestObjectCreator.CC1.PK;

			var securityCheckPoint = Env.Security.ApportionRevToShipmentModifyDefaultChargeCodeDescription;

			Assert(securityCheckPoint.IsAllowed);
			Assert(!consolRevenue.DescriptionInfo.ReadOnly);

			securityCheckPoint.IsAllowed = false;
			Assert(!securityCheckPoint.IsAllowed);
			Assert(consolRevenue.DescriptionInfo.ReadOnly);

			TestObjectCreator.CC1.AC_AllowDescriptionOvertype = false;

			securityCheckPoint.IsAllowed = true;
			Assert(securityCheckPoint.IsAllowed);
			Assert(consolRevenue.DescriptionInfo.ReadOnly);

			securityCheckPoint.IsAllowed = false;
			Assert(!securityCheckPoint.IsAllowed);
			Assert(consolRevenue.DescriptionInfo.ReadOnly);
		}

		public void TestChargeableRateForRevenueApportionmentUpdatedOnApportionmentMethodChange()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();
			var consolRevenueMaster = new ConsolRevenueMaster(consol, Factory);
			var consolRevenue = new ConsolRevenue(consolRevenueMaster)
			{ ApportionmentMethod = AllocationMethod.ChargeableUnits };
			AssertEquals("0", consolRevenue.SplitCharges[0].ChargeableRateForRevenueApportionment);
			foreach (ICodeDescription allocationMethodPair in new CodeDescriptionPairList(OLookUpEditType.AllocationMethod))
			{
				consolRevenue.ApportionmentMethod = allocationMethodPair.Code;
				if (ApportionmentCreator.IsApportionmentMethodPerChargeableUnit(allocationMethodPair.Code))
				{
					AssertEquals("0", consolRevenue.SplitCharges[0].ChargeableRateForRevenueApportionment);
				}
				else
				{
					AssertEquals("Not Applicable", consolRevenue.SplitCharges[0].ChargeableRateForRevenueApportionment);
				}
			}

			ReleaseFactory();
			consolRevenueMaster = new ConsolRevenueMaster(consol, Factory);
			consolRevenue = new ConsolRevenue(consolRevenueMaster)
			{ ApportionmentMethod = AllocationMethod.Manual };
			AssertEquals("Not Applicable", consolRevenue.SplitCharges[0].ChargeableRateForRevenueApportionment);
		}

		public void TestChargeableUnitForRevenueApportionment()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			ForwardingConsol consol = creator.CreateConsol("AUABC", "KRABC", "C00001");
			ForwardingShipment shipment = creator.CreateShipment("S00001", consol);
			shipment.JS_ActualWeight = 100m;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			Factory.Save();
			var consolRevenueMaster = new ConsolRevenueMaster(consol, Factory);
			ConsolRevenue rev = new ConsolRevenue(consolRevenueMaster);
			rev.ChargeCode = creator.CC1.PK;
			AssertEquals(Core.Constants.Weight.Kilograms, rev.SplitCharges[0].ChargeableUnitForRevenueApportionment);
			consolRevenueMaster.ReleaseMutexes();
		}

		public void TestCostGovtChargeCodeReadOnlyness()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("AUABC", "KRABC", "C00001");
			var shipment = creator.CreateShipment("S00001", consol);
			Factory.Save();
			var consolRevenueMaster = new ConsolRevenueMaster(consol, Factory);
			ConsolRevenue rev = new ConsolRevenue(consolRevenueMaster);
			rev.ChargeCode = creator.CC1.PK;
			Env.Security.ApportionRevToShipmentCostGovtCode.IsAllowed = false;
			AssertEquals("CostGovtChargeCodeInfo.ReadOnly", true, rev.CostGovtChargeCodeInfo.ReadOnly);
			Env.Security.ApportionRevToShipmentCostGovtCode.IsAllowed = true;
			AssertEquals("CostGovtChargeCodeInfo.ReadOnly", false, rev.CostGovtChargeCodeInfo.ReadOnly);
			consolRevenueMaster.ReleaseMutexes();
		}

		public void TestSellGovtChargeCodeReadOnlyness()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("AUABC", "KRABC", "C00001");
			var shipment = creator.CreateShipment("S00001", consol);
			Factory.Save();
			var consolRevenueMaster = new ConsolRevenueMaster(consol, Factory);
			ConsolRevenue rev = new ConsolRevenue(consolRevenueMaster);
			rev.ChargeCode = creator.CC1.PK;
			Env.Security.ApportionRevToShipmentSellGovtCode.IsAllowed = false;
			AssertEquals("SellGovtChargeCodeInfo.ReadOnly", true, rev.SellGovtChargeCodeInfo.ReadOnly);
			Env.Security.ApportionRevToShipmentSellGovtCode.IsAllowed = true;
			AssertEquals("SellGovtChargeCodeInfo.ReadOnly", false, rev.SellGovtChargeCodeInfo.ReadOnly);
			consolRevenueMaster.ReleaseMutexes();
		}

		public void TestGovtChargeCode()
		{
			AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			TestObjectCreator creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("AUABC", "KRABC", "C00001");
			var shipment = creator.CreateShipment("S00001", consol);
			var chargeCode = creator.CC1;
			chargeCode.AC_GovtChargeCode = "11";
			Factory.Save();
			var consolRevenueMaster = new ConsolRevenueMaster(consol, Factory);
			ConsolRevenue rev = new ConsolRevenue(consolRevenueMaster);
			AssertEquals(string.Empty, rev.CostGovtChargeCode);
			AssertEquals(string.Empty, rev.SellGovtChargeCode);
			rev.ChargeCode = creator.CC1.PK;
			AssertEquals("11", rev.CostGovtChargeCode);
			AssertEquals("11", rev.SellGovtChargeCode);
			consolRevenueMaster.ReleaseMutexes();
		}

		public void TestSplitChargesFactoryReload()
		{
			var creator = new TestObjectCreator(Factory);
			var forwardingConsol = creator.CreateConsol("AUABC", "KRABC", "C00001");
			var forwardingShipment = creator.CreateShipment("S00001", forwardingConsol);
			var multiJobConsolidation = (IDtbBookingConsolidation)Factory.New(ObjectFactory.GetType<IDtbBookingConsolidation>());
			multiJobConsolidation.KB_JobType = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation;
			var singleBookingConsolidation = (IDtbBookingConsolidation)Factory.New(ObjectFactory.GetType<IDtbBookingConsolidation>());
			var shipmentBookingConsolidation = (IDtbBookingConsolidation)Factory.New(ObjectFactory.GetType<IDtbBookingConsolidation>());
			singleBookingConsolidation.KB_JobType = TransportConsolidationJobTypes.Codes.Booking;
			shipmentBookingConsolidation.KB_JobType = TransportConsolidationJobTypes.Codes.Booking;
			shipmentBookingConsolidation.KB_ParentID = forwardingShipment.PK;
			shipmentBookingConsolidation.KB_ParentTableCode = forwardingShipment.TablePrefix;
			var standalongeBooking = (IDtbBooking)Factory.New(ObjectFactory.GetType<IDtbBooking>());
			standalongeBooking.KM_KB_Booking = singleBookingConsolidation.PK;
			standalongeBooking.KM_KB_BookingConsolidationMultiJob = multiJobConsolidation.PK;
			var shipmentBooking = (IDtbBooking)Factory.New(ObjectFactory.GetType<IDtbBooking>());
			shipmentBooking.KM_KB_Booking = shipmentBookingConsolidation.PK;
			shipmentBooking.KM_KB_BookingConsolidationMultiJob = multiJobConsolidation.PK;
			var newFactory = new BusinessObjectFactory();
			var consolRevenueMaster = new ConsolRevenueMaster((IJobCostingPlugIn)multiJobConsolidation, newFactory);
			var consolRevenue = new ConsolRevenue(consolRevenueMaster);
			consolRevenue.ChargeCode = creator.CC1.PK;
			AssertEquals("Ensure no exception and correct result.", 2, consolRevenue.SplitCharges.Count);
			consolRevenueMaster.ReleaseMutexes();
		}

		public void TestDescriptionAndLocalDescription()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			OrgHeader oRG = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "F"));
			oRG.OH_RL_NKClosestPort = "NZAKL";
			ZQuery queery = new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			queery.AddToFilter(AccChargeCodeSchema.AC_Desc, "Customs Clearance / Agency Fees");
			AccChargeCode charge = Factory.LoadTop1<AccChargeCode>(queery);
			charge.AC_LocalLanguageDescription = "Local Description";
			AssertNotNull("cant find Customs Clearance / Agency Fees charge", charge);
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			ForwardingShipment shipment3 = consol.Shipments.AddNew();
			Job shipment1job = new Job.Loader(shipment1).TryCreateWithoutMutexForTestOnly();
			Job shipment2job = new Job.Loader(shipment2).TryCreateWithoutMutexForTestOnly();
			shipment1.Job.LocalChargesPK = oRG.PK;
			shipment2.Job.LocalChargesPK = oRG.PK;
			var consolRevenueMaster = new ConsolRevenueMaster(consol, Factory);
			ConsolRevenue rev = new ConsolRevenue(consolRevenueMaster);
			rev.ChargeCode = charge.PK;
			AssertEquals("Customs Clearance / Agency Fees", rev.Description);
			AssertEquals("Customs Clearance / Agency Fees", rev.SplitCharges[0].JR_Desc);
			AssertEquals("Customs Clearance / Agency Fees", rev.SplitCharges[1].JR_Desc);
			AssertEquals("Customs Clearance / Agency Fees", rev.SplitCharges[2].JR_Desc);
			oRG.OH_RL_NKClosestPort = "AUSYD";
			rev.ChargeCode = Factory.New<AccChargeCode>().PK;
			rev.ChargeCode = charge.PK;
			AssertEquals("Local Description", rev.Description);
			AssertEquals("Local Description", rev.SplitCharges[0].JR_Desc);
			AssertEquals("Local Description", rev.SplitCharges[1].JR_Desc);
			AssertEquals("Customs Clearance / Agency Fees", rev.SplitCharges[2].JR_Desc);
			rev.Description = "New Description";
			AssertEquals("New Description", rev.SplitCharges[0].JR_Desc);
			AssertEquals("New Description", rev.SplitCharges[1].JR_Desc);
			AssertEquals("New Description", rev.SplitCharges[2].JR_Desc);
			consolRevenueMaster.ReleaseMutexes();
		}

		public void TestUnApportionedAmount()
		{
			var (rev, master) = prepareConsolRevenue();
			AssertEquals(0m, rev.UnApportionedAmount);

			rev.SplitCharges[0].JR_OSSellAmt = 0m;
			AssertEquals(50m, rev.UnApportionedAmount);

			master.ReleaseMutexes();
		}

		public void TestRemoveNonApplicableCharges()
		{
			var (rev, master) = prepareConsolRevenue();
			AssertEquals(0m, rev.UnApportionedAmount);

			rev.SplitCharges[0].IsUsedForApportionment = false;
			AssertEquals(0m, rev.UnApportionedAmount);

			var charge1Pk = rev.SplitCharges[0].PK;
			var charge2Pk = rev.SplitCharges[1].PK;

			rev.Factory.Save();

			var charge1 = Factory.Load<Charge>(charge1Pk);
			AssertNull(charge1);

			var charge2 = Factory.Load<Charge>(charge2Pk);
			AssertNotNull(charge2);
			AssertEquals(rev.SplitCharges[0].JR_OSSellAmt, charge2.JR_OSSellAmt);
			AssertEquals(charge2.JR_OSSellAmt, charge2.JR_OSCostAmt);

			master.ReleaseMutexes();
		}

		public void TestRemoveNonApplicableJobs()
		{
			var (rev, master) = prepareConsolRevenue();
			rev.SplitCharges[0].IsUsedForApportionment = false;

			var job1Pk = rev.SplitCharges[0].Job.PK;
			var job2Pk = rev.SplitCharges[1].Job.PK;

			rev.Factory.Save();

			AssertNull(Factory.Load<JobHeader>(job1Pk));
			AssertNotNull(Factory.Load<JobHeader>(job2Pk));
			master.ReleaseMutexes();
		}

		public void TestDefaultSetIsUsedForApportionmentToTrue()
		{
			var (rev, master) = prepareConsolRevenue();

			AssertEquals(2, rev.SplitCharges.Count);
			Assert(rev.SplitCharges[0].IsUsedForApportionment);
			Assert(rev.SplitCharges[1].IsUsedForApportionment);

			master.ReleaseMutexes();
		}

		(ConsolRevenue, ConsolRevenueMaster) prepareConsolRevenue()
		{
			IJobCostingPlugIn consol = Factory.New<ForwardingConsol>();
			var shipment1 = ((ForwardingConsol)consol).Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S1";
			var shipment2 = ((ForwardingConsol)consol).Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S2";

			var creator = new TestObjectCreator(Factory);
			var master = new ConsolRevenueMaster(consol, Factory);
			var rev = new ConsolRevenue(master);
			rev.ChargeCode = creator.CC1.PK;
			rev.SellAmount = 100m;
			rev.SplitCharges[0].JR_GE = GlbDepartment.CurrentDepartment.PK;
			rev.SplitCharges[1].JR_GE = GlbDepartment.CurrentDepartment.PK;
			return (rev, master);
		}

		public void TestRevenueApportion()
		{
			RefCurrency currency = Factory.New<RefCurrency>();
			currency.RX_Code = "ART";
			RefCurrency currency2 = Factory.New<RefCurrency>();
			currency2.RX_Code = "TRA";
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 2;
			shipment2.JS_ActualWeight = 5;
			var consolRevenueMaster = new ConsolRevenueMaster(consol, Factory);
			ConsolRevenue rev = new ConsolRevenue(consolRevenueMaster);
			AssertEquals(2, rev.SplitCharges.Count);
			rev.ChargeCode = Factory.Load<AccChargeCode>(new ZQuery())[0].PK;
			rev.ApportionmentMethod = AllocationMethod.Shipment;
			rev.SellAmount = 7000;
			Job job1 = new Job.Loader(shipment1).TryLoadOrCreateWithoutMutexForTestOnly();
			Job job2 = new Job.Loader(shipment2).TryLoadOrCreateWithoutMutexForTestOnly();
			ExchangeRate rate1 = job1.ExchangeRates.AddNew();
			ExchangeRate rate2 = job2.ExchangeRates.AddNew();
			rate1.JF_RX_NKRateCurrency = currency.RX_Code;
			rate1.JF_BaseRate = 2;
			rate2.JF_RX_NKRateCurrency = currency.RX_Code;
			rate2.JF_BaseRate = 4;
			rev.Currency = currency.RX_Code;
			AssertEquals((decimal)3500, System.Math.Round(rev.SplitCharges[0].JR_OSSellAmt));
			AssertEquals((decimal)3500, System.Math.Round(rev.SplitCharges[1].JR_OSSellAmt));
			AssertEquals((decimal)1750, System.Math.Round(rev.SplitCharges[0].JR_LocalSellAmt));
			AssertEquals((decimal)875, System.Math.Round(rev.SplitCharges[1].JR_LocalSellAmt));
			rev.ApportionmentMethod = AllocationMethod.GrossWeight;
			AssertEquals((decimal)2000, System.Math.Round(rev.SplitCharges[0].JR_OSSellAmt));
			AssertEquals((decimal)5000, System.Math.Round(rev.SplitCharges[1].JR_OSSellAmt));
			AssertEquals((decimal)1000, System.Math.Round(rev.SplitCharges[0].JR_LocalSellAmt));
			AssertEquals((decimal)1250, System.Math.Round(rev.SplitCharges[1].JR_LocalSellAmt));
			using (rev.SplitCharges[0].SuspendSplittingApportionAmountChangeIsUsedForApportionment())
			{
				rev.SplitCharges[0].JR_OSSellAmt = 0;
			}
			using (rev.SplitCharges[1].SuspendSplittingApportionAmountChangeIsUsedForApportionment())
			{
				rev.SplitCharges[1].JR_OSSellAmt = 0;
			}
			rev.SplitCharges[0].JR_LocalSellAmt = 0;
			rev.SplitCharges[1].JR_LocalSellAmt = 0;
			rev.ChargeCode = Factory.Load<AccChargeCode>(new ZQuery())[1].PK;
			AssertEquals((decimal)2000, System.Math.Round(rev.SplitCharges[0].JR_OSSellAmt));
			AssertEquals((decimal)5000, System.Math.Round(rev.SplitCharges[1].JR_OSSellAmt));
			AssertEquals((decimal)2000, System.Math.Round(rev.SplitCharges[0].JR_LocalSellAmt));
			AssertEquals((decimal)5000, System.Math.Round(rev.SplitCharges[1].JR_LocalSellAmt));
			using (rev.SplitCharges[0].SuspendSplittingApportionAmountChangeIsUsedForApportionment())
			{
				rev.SplitCharges[0].JR_OSSellAmt = 0;
			}
			using (rev.SplitCharges[1].SuspendSplittingApportionAmountChangeIsUsedForApportionment())
			{
				rev.SplitCharges[1].JR_OSSellAmt = 0;
			}
			rev.SplitCharges[0].JR_LocalSellAmt = 0;
			rev.SplitCharges[1].JR_LocalSellAmt = 0;
			rate1 = job1.ExchangeRates.AddNew();
			rate2 = job2.ExchangeRates.AddNew();
			rate1.JF_RX_NKRateCurrency = currency2.RX_Code;
			rate1.JF_BaseRate = 2;
			rate2.JF_RX_NKRateCurrency = currency2.RX_Code;
			rate2.JF_BaseRate = 4;
			rev.Currency = currency2.RX_Code;
			AssertEquals((decimal)2000, System.Math.Round(rev.SplitCharges[0].JR_OSSellAmt));
			AssertEquals((decimal)5000, System.Math.Round(rev.SplitCharges[1].JR_OSSellAmt));
			AssertEquals((decimal)1000, System.Math.Round(rev.SplitCharges[0].JR_LocalSellAmt));
			AssertEquals((decimal)1250, System.Math.Round(rev.SplitCharges[1].JR_LocalSellAmt));
			consolRevenueMaster.ReleaseMutexes();
		}

		public void TestChargeCodeCurrency()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			var consolRevenueMaster = new ConsolRevenueMaster(consol, Factory);
			ConsolRevenue rev = new ConsolRevenue(consolRevenueMaster);
			rev.ChargeCode = Factory.Load<AccChargeCode>(new ZQuery())[0].PK;
			rev.Currency = Factory.Load<RefCurrency>(new ZQuery())[0].RX_Code;
			AssertEquals(rev.ChargeCode, rev.SplitCharges[0].JR_AC);
			AssertEquals(rev.ChargeCode, rev.SplitCharges[1].JR_AC);
			AssertEquals(rev.Currency, rev.SplitCharges[0].JR_RX_NKSellCurrency);
			AssertEquals(rev.Currency, rev.SplitCharges[1].JR_RX_NKSellCurrency);
			consolRevenueMaster.ReleaseMutexes();
		}

		public void TestSetDefaultsForNew()
		{
			IJobCostingPlugIn consol = Factory.New<ForwardingConsol>();
			ConsolRevenue rev = new ConsolRevenue(new ConsolRevenueMaster(consol, Factory));
			AssertEquals(AllocationMethod.ChargeableUnits, rev.ApportionmentMethod);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, rev.Currency);
		}

		public void TestCurrencyValidates()
		{
			IJobCostingPlugIn consol = Factory.New<ForwardingConsol>();
			ConsolRevenue rev = new ConsolRevenue(new ConsolRevenueMaster(consol, Factory));
			rev.Currency = ZString.Empty;
			AssertHasErrors(rev.CurrencyInfo);
		}

		public void TestChargeCodeValidates()
		{
			IJobCostingPlugIn consol = Factory.New<ForwardingConsol>();
			ConsolRevenue rev = new ConsolRevenue(new ConsolRevenueMaster(consol, Factory));
			rev.ChargeCode = ZGuid.Empty;
			AssertHasErrors(rev.ChargeCodeInfo);
		}

		public void TestSellAmountValidates()
		{
			IJobCostingPlugIn consol = Factory.New<ForwardingConsol>();
			ConsolRevenue rev = new ConsolRevenue(new ConsolRevenueMaster(consol, Factory));
			rev.SellAmount = 0;
			AssertHasErrors(rev.SellAmountInfo);
		}

		public void TestApprMethodValidates()
		{
			IJobCostingPlugIn consol = Factory.New<ForwardingConsol>();
			ConsolRevenue rev = new ConsolRevenue(new ConsolRevenueMaster(consol, Factory));
			rev.ApportionmentMethod = "";
			AssertHasErrors(rev.ApportionmentMethodInfo);
		}

		public void TestChargeCodeSetsInvoiceType()
		{
			IJobCostingPlugIn consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = ((ForwardingConsol)consol).Shipments.AddNew();
			shipment.JS_TransportMode = "SEA";
			OrgHeader oRG = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
			AccChargeCode charge = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK));
			OrgCompanyData data = Factory.LoadTop1<OrgCompanyData>(new ZQuery(OrgCompanyDataSchema.OB_OH, oRG.PK));
			OrgInvoiceRollupOrGroup group = Factory.New<OrgInvoiceRollupOrGroup>();
			group.PG_OB = data.PK;
			group.PG_InvoicePostingStyle = InvoicePostingOptionsList.Codes.FinalInvoiceOnly;
			group.PG_JobType = "ALL";
			group.PG_ServiceDirection = "ALL";
			group.PG_TransportMode = "ALL";
			Job.Loader loader = new Job.Loader(shipment);
			Job job = loader.TryCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_OA_LocalChargesAddr = oRG.MainAddress.PK;
			Factory.Save();
			ConsolRevenue rev = new ConsolRevenue(new ConsolRevenueMaster(consol, Factory));
			rev.ChargeCode = charge.PK;
			AssertEquals("setting charge code should get invoice type from dbo.orgHeader", InvoiceTypesList.Codes.FinalInvoice, rev.SplitCharges[0].JR_InvoiceType);
		}

		public void TestConsolRevenueSequenceNumber()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S1";
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S2";
			ForwardingShipment shipment3 = consol.Shipments.AddNew();
			shipment3.JS_UniqueConsignRef = "S3";
			Job job = new Job.Loader(shipment2).TryLoadOrCreateWithoutMutexForTestOnly();
			Charge ch1 = job.Charges.AddNew();
			ch1.JR_DisplaySequence = 1;
			job = new Job.Loader(shipment3).TryLoadOrCreateWithoutMutexForTestOnly();
			Charge ch2 = job.Charges.AddNew();
			Charge ch3 = job.Charges.AddNew();
			Charge ch4 = job.Charges.AddNew();
			ch2.JR_DisplaySequence = 1;
			ch3.JR_DisplaySequence = 2;
			ch4.JR_DisplaySequence = 3;
			ConsolRevenueMaster master = new ConsolRevenueMaster(consol, Factory);
			ConsolRevenue rev1 = new ConsolRevenue(master);
			master.Revenues.Add(rev1);
			AssertEquals(3, rev1.SplitCharges.Count);
			rev1.SplitCharges.ApplySort("JR_DisplaySequence", System.ComponentModel.ListSortDirection.Ascending);
			AssertEquals((ZShort)1, rev1.SplitCharges[0].JR_DisplaySequence);
			AssertEquals((ZShort)2, rev1.SplitCharges[1].JR_DisplaySequence);
			AssertEquals((ZShort)4, rev1.SplitCharges[2].JR_DisplaySequence);
			master.ReleaseMutexes();
		}

		public void TestIApportionedChargesHeaderImplementation()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 2;
			shipment2.JS_ActualWeight = 5;
			Job job1 = new Job.Loader(shipment1).TryCreateWithoutMutexForTestOnly();
			Job job2 = new Job.Loader(shipment2).TryCreateWithoutMutexForTestOnly();
			ConsolRevenue revenue = new ConsolRevenue(new ConsolRevenueMaster(consol, Factory));
			AssertEquals("SplitCharges.Count", 2, revenue.SplitCharges.Count);
			revenue.ChargeCode = Factory.Load<AccChargeCode>(new ZQuery())[0].PK;
			revenue.ApportionmentMethod = AllocationMethod.Shipment;
			revenue.Currency = testObjectCreator.AUD.RX_Code;
			revenue.ChargeCode = testObjectCreator.CC4.PK;
			IApportionedChargesHeader revenueAsHeader = revenue;
			AssertEquals("ApportionmentMethod", revenue.ApportionmentMethod, revenueAsHeader.ApportionmentMethod);
			AssertEquals("ChargeCode", revenue.ChargeCode, revenueAsHeader.ChargeCode.PK);
			AssertEquals("Currency", revenue.Currency, revenueAsHeader.Currency.RX_Code);
			AssertEquals("Charges.Length", revenue.SplitCharges.Count, revenueAsHeader.Charges.Length);
			AssertEquals("IsChargeReadyToPost Charge 1", revenue.SplitCharges[0].JR_OSSellAmt != 0M, revenueAsHeader.IsChargeReadyToPost(revenueAsHeader.Charges[0]));
			AssertEquals("IsChargeReadyToPost Charge 2", revenue.SplitCharges[1].JR_OSSellAmt != 0M, revenueAsHeader.IsChargeReadyToPost(revenueAsHeader.Charges[1]));
			revenue.SellAmount = 200M;
			AssertEquals("IsChargeReadyToPost Charge 1", true, revenueAsHeader.IsChargeReadyToPost(revenueAsHeader.Charges[0]));
			AssertEquals("IsChargeReadyToPost Charge 2", true, revenueAsHeader.IsChargeReadyToPost(revenueAsHeader.Charges[1]));
			var revenueInvoice = testObjectCreator.CreateARInvoice<ARInvoice>("001", testObjectCreator.AUD, 1M, testObjectCreator.AALSHI);
			var revenueLine = testObjectCreator.CreateARInvoiceLine(revenueInvoice, job1, testObjectCreator.CC4, testObjectCreator.AUD, 1M, "Revenue", 100M);
			revenue.SplitCharges[0].JR_AL_ARLine = revenueLine.PK;
			Assert(revenue.SplitCharges[0].IsRevenuePosted);
			AssertEquals("IsChargeReadyToPost Charge 1", false, revenueAsHeader.IsChargeReadyToPost(revenueAsHeader.Charges[0]));
			AssertEquals("IsChargeReadyToPost Charge 2", true, revenueAsHeader.IsChargeReadyToPost(revenueAsHeader.Charges[1]));
		}

		public void TestConsolRevenueSequenceNumber_CaseWhenSequenceNumberNearShortMaxValue()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S1";
			Job job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			Charge ch1 = job.Charges.AddNew();
			Charge ch2 = job.Charges.AddNew();
			Charge ch3 = job.Charges.AddNew();
			ch1.JR_DisplaySequence = 1;
			ch2.JR_DisplaySequence = short.MaxValue;
			ch3.JR_DisplaySequence = 3;
			ConsolRevenueMaster master = new ConsolRevenueMaster(consol, Factory);
			ConsolRevenue revenue = new ConsolRevenue(master);
			master.Revenues.Add(revenue);
			AssertEquals(1, revenue.SplitCharges.Count);
			AssertEquals((ZShort)2, revenue.SplitCharges[0].JR_DisplaySequence);
		}

		public void TestSplitChargesIsRegisteredEditableChildObject()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ConsolRevenueMaster master = new ConsolRevenueMaster(consol, Factory);
			ConsolRevenue revenue = new ConsolRevenue(master);
			AssertEquals("SplitCharges IsRegisteredEditableChildObject", true, revenue.IsRegisteredEditableChildObject(revenue.SplitCharges));
		}

		public void TestSplitChargesLoadOrCreateJobsWithMutex()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("AUABC", "KRABC", "C00001");
			var shipment = creator.CreateShipment("S00001", consol);
			shipment.JS_ActualWeight = 100m;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			Factory.Save();
			//user opens the billing tab
			var loader = new JobHeader.Loader(shipment);
			var job = loader.TryLoadOrCreateWithMutex();
			//user click the 'Apportion Revenue To Shipments' menu
			var newFactory = new BusinessObjectFactory();
			var consolRevMaster = new ConsolRevenueMaster(consol, newFactory);
			string mutexExceptionMessage = null;
			consolRevMaster.JobCreationExceptionEvent += (sender, e) =>
			{
				mutexExceptionMessage = e.Message;
			}

			;
			var consolRevenue = new ConsolRevenue(consolRevMaster);
			consolRevenue.ChargeCode = creator.CC1.PK;
			var splitCharges = consolRevenue.SplitCharges;
			Assert("Mutex error is generated", !string.IsNullOrWhiteSpace(mutexExceptionMessage));
			AssertEquals(nameof(mutexExceptionMessage), @"You have created the job S00001 on another form, but haven't saved it yet.
Please close or save other forms that use job S00001 to continue.", mutexExceptionMessage);
			consolRevMaster.ReleaseMutexes();
			job.Dispose();
		}

		public void TestRevenueApportionmentShipments_HeaderWithNoChanges()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment1 = TestObjectCreator.CreateShipment("S00001", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			var shipment2 = TestObjectCreator.CreateShipment("S00002", consol);
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			Factory.Save();
			var newFactory = Factory.CreateNewFactory();
			newFactory.RefreshEnabled = true;
			var consolRevenueMaster = new ConsolRevenueMaster(consol, newFactory);
			var consolRevenue = new ConsolRevenue(consolRevenueMaster);
			consolRevenue.ChargeCode = TestObjectCreator.CC1.PK;
			consolRevenue.ApportionmentMethod = AllocationMethod.Shipment;
			consolRevenue.SellAmount = 100M;
			newFactory.Save();
			AssertEquals("Ensure no exception and correct result.", 2, consolRevenue.SplitCharges.Count);
			AssertEquals("job1 should have 1 charge from data refresh", 1, job1.Charges.Count);
			AssertEquals("job1 should have no changes", false, job1.HasChanges);
			AssertEquals("job2 should have 1 charge from data refresh", 1, job2.Charges.Count);
			AssertEquals("job2 should have no changes", false, job2.HasChanges);
		}

		public void TestRevenueApportionmentShipments_WillNotRemainJobHeaderInConsolFactoryWhenNoJobHeader()
		{
			var consol = TestObjectCreator.CreateConsol();
			TestObjectCreator.CreateShipment("S00001", consol);
			TestObjectCreator.CreateShipment("S00002", consol);
			Factory.Save();

			var consolRevenueMaster = new ConsolRevenueMaster(consol, Factory.CreateNewFactory());
			var revenue = consolRevenueMaster.Revenues.AddNew();
			revenue.ChargeCode = TestObjectCreator.CC1.PK;

			AssertEquals("Factory should have no JobHeader after run revenue.SplitCharges", 0, Factory.Load<JobHeader>(new ZQuery()).Length);
			consolRevenueMaster.ReleaseMutexes();
		}

		public void TestRevenueApportionmentShipments_CreateAccrualOnlyWithChargeCodeMRG100()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("AUABC", "KRABC", "C00001");
			var shipment1 = creator.CreateShipment("S00001", consol);
			var shipment2 = creator.CreateShipment("S00002", consol);
			shipment1.JS_ActualWeight = 2;
			shipment2.JS_ActualWeight = 5;
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			var job2 = TestObjectCreator.CreateJob(shipment2, false);

			var chargeCodeMRG100 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCodeMRG000 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCodeMJA000 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCodeMRG100.AC_ChargeType = "MRG";
			chargeCodeMRG100.AC_MarginPercentage = 100;
			chargeCodeMRG000.AC_ChargeType = "MRG";
			chargeCodeMRG000.AC_MarginPercentage = 0;
			chargeCodeMJA000.AC_ChargeType = "MJA";
			chargeCodeMJA000.AC_MarginPercentage = 0;

			Factory.Save();

			var revenueMaster = new ConsolRevenueMaster(consol, Factory);
			var revenueMRG100 = revenueMaster.Revenues.AddNew();
			var revenueMRG000 = revenueMaster.Revenues.AddNew();
			var revenueMJA000 = revenueMaster.Revenues.AddNew();
			AssertEquals("SplitCharges.Count", 2, revenueMRG100.SplitCharges.Count);
			AssertEquals("SplitCharges.Count", 2, revenueMRG000.SplitCharges.Count);
			AssertEquals("SplitCharges.Count", 2, revenueMJA000.SplitCharges.Count);

			revenueMRG100.ApportionmentMethod = AllocationMethod.ChargeableUnits;
			revenueMRG100.SellAmount = 300M;
			revenueMRG100.ChargeCode = chargeCodeMRG100.PK;
			revenueMRG100.SplitCharges[0].JR_OSSellAmt = 100M;
			revenueMRG100.SplitCharges[0].IsUsedForApportionment = true;
			revenueMRG100.SplitCharges[1].JR_OSSellAmt = 200M;
			revenueMRG100.SplitCharges[1].IsUsedForApportionment = true;

			revenueMRG000.ApportionmentMethod = AllocationMethod.ChargeableUnits;
			revenueMRG000.SellAmount = 3000M;
			revenueMRG000.ChargeCode = chargeCodeMRG000.PK;
			revenueMRG000.SplitCharges[0].JR_OSSellAmt = 1000M;
			revenueMRG000.SplitCharges[0].IsUsedForApportionment = true;
			revenueMRG000.SplitCharges[1].JR_OSSellAmt = 2000M;
			revenueMRG000.SplitCharges[1].IsUsedForApportionment = true;

			revenueMJA000.ApportionmentMethod = AllocationMethod.ChargeableUnits;
			revenueMJA000.SellAmount = 3000M;
			revenueMJA000.ChargeCode = chargeCodeMJA000.PK;
			revenueMJA000.SplitCharges[0].JR_OSSellAmt = 10000M;
			revenueMJA000.SplitCharges[0].IsUsedForApportionment = true;
			revenueMJA000.SplitCharges[1].JR_OSSellAmt = 20000M;
			revenueMJA000.SplitCharges[1].IsUsedForApportionment = true;

			Factory.Save();

			var testFactory = new BusinessObjectFactory();
			job1 = testFactory.Load<Job>(job1.PK);
			job2 = testFactory.Load<Job>(job2.PK);

			AssertEquals("job1 should have a profit loss with MRG 100%", -100M, job1.ProfitLoss[0].ProfitLossDetails.First(x => x.ZY_Calc_AC == chargeCodeMRG100.PK && x.ZY_Calc_LineType == "ACR").ZY_Calc_LineAmount);
			AssertEquals("job1 should not have a profit loss with MRG 0%", 0, job1.ProfitLoss[0].ProfitLossDetails.Count(x => x.ZY_Calc_AC == chargeCodeMRG000.PK && x.ZY_Calc_LineType == "ACR"));
			AssertEquals("job1 should not have a profit loss with MJA", 0, job1.ProfitLoss[0].ProfitLossDetails.Count(x => x.ZY_Calc_AC == revenueMJA000.PK && x.ZY_Calc_LineType == "ACR"));

			AssertEquals("job2 should have a profit loss with MRG 100%", -200M, job2.ProfitLoss[0].ProfitLossDetails.First(x => x.ZY_Calc_AC == chargeCodeMRG100.PK && x.ZY_Calc_LineType == "ACR").ZY_Calc_LineAmount);
			AssertEquals("job2 should not have a profit loss with MRG 0%", 0, job2.ProfitLoss[0].ProfitLossDetails.Count(x => x.ZY_Calc_AC == chargeCodeMRG000.PK && x.ZY_Calc_LineType == "ACR"));
			AssertEquals("job2 should not have a profit loss with MJA", 0, job2.ProfitLoss[0].ProfitLossDetails.Count(x => x.ZY_Calc_AC == revenueMJA000.PK && x.ZY_Calc_LineType == "ACR"));
		}

		public void TestUpdateSellSupplyType_SetApportionmentChargesSellSupplyType()
		{
			var consol = TestObjectCreator.CreateConsol("AUABC", "KRABC", "C00001");
			var shipment1 = TestObjectCreator.CreateShipment("S00001", consol);
			var shipment2 = TestObjectCreator.CreateShipment("S00002", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			Factory.Save();

			var master = new ConsolRevenueMaster(consol, Factory);
			var consolRevenue = new ConsolRevenue(master);

			AssertNullOrEmpty("Precondition", consolRevenue.SellSupplyType);
			AssertEquals("Precondition", 2, consolRevenue.SplitCharges.Count);
			AssertEquals("Precondition", true, consolRevenue.SplitCharges.OfType<JobCharge>().All(x => x.JR_CostSupplyType.IsEmpty));
			AssertEquals("Precondition", true, consolRevenue.SplitCharges.OfType<JobCharge>().All(x => x.JR_SellSupplyType.IsEmpty));

			consolRevenue.SellSupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC;
			AssertEquals(true, consolRevenue.SplitCharges.OfType<JobCharge>().All(x => x.JR_SellSupplyType == AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC));
			AssertEquals(true, consolRevenue.SplitCharges.OfType<JobCharge>().All(x => x.JR_CostSupplyType.IsEmpty));
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
