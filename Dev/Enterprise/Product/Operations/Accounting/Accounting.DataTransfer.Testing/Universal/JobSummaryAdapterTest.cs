using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.Testing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.ShipmentProcessing;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using UniversalCodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;

namespace Enterprise.Accounting.DataTransfer.Universal.Testing
{
	public partial class JobSummaryAdapterTest : TestCaseWithFactory
	{
		#region Cost/Sell Rating Behavior

		public void TestCostSellRatingBehavior_Create()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001001", "NZAKL", "AUSYD");
			var job = TestObjectCreator.CreateJob(shipment, false);

			var chargeLine = GetChargeLine("SYD", "FRT", "001", InvoiceDate, "GST", InvoiceDate, 100.00m, 100.00m, "AUD", 10.00m, "AALSHI", "ABIGAS", "FIS", "Charge Description 1", 1, "GST", "FIN", 100.00m, 100.00m, "AUD", 10.00m, "REA", "REA", true, false);
			chargeLine.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine.ImportMetaData.Instruction = InstructionType.Insert;

			SetLogger();

			var universalShipment = GetUniversalShipmentWithCharges(Factory, TestObjectCreator, GlbCompany.CurrentCompany, null);
			universalShipment.JobCosting.ChargeLineCollection.Add(chargeLine);

			AssertCostSellRatingBehavior_JobCharges
			(
				job,
				universalShipment,
				shipment,
				preConditionJobCharges: Array.Empty<string>(),
				expectedJobCharges: new string[] { "FRT => CostRatingBehavior: REA, SellRatingBehavior: REA" },
				message: "GIVEN ChargeLine with REA Cost/SellRatingBehavior WHEN ImportCharges THEN job charge Cost/SellRatingBehavior should be REA"
			);
		}

		public void TestCostSellRatingBehavior_Update()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001001", "NZAKL", "AUSYD");
			var job = TestObjectCreator.CreateJob(shipment, false);

			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, 100m, 200m);
			charge1.JR_OH_SellAccount = TestObjectCreator.Debtor1.PK;
			charge1.JR_Calc_CostRatingBehavior = "NEW";
			charge1.JR_Calc_SellRatingBehavior = "NEW";

			var chargeLine = GetChargeLine("SYD", "FRT", "001", InvoiceDate, "GST", InvoiceDate, 110.00m, 110.00m, "AUD", 10.00m, "AALSHI", "ABIGAS", "FIS", "Charge Description 1", 1, "GST", "FIN", 210.00m, 210.00m, "AUD", 10.00m, "REA", "REA", true, false);
			chargeLine.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine.ImportMetaData.Instruction = InstructionType.Update;
			chargeLine.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>() { new MatchingCriteria() { FieldName = "ChargeCode", Value = "FRT" } });

			SetLogger();

			var universalShipment = GetUniversalShipmentWithCharges(Factory, TestObjectCreator, GlbCompany.CurrentCompany, null);
			universalShipment.JobCosting.ChargeLineCollection.Add(chargeLine);

			AssertCostSellRatingBehavior_JobCharges
			(
				job,
				universalShipment,
				shipment,
				preConditionJobCharges: new string[] { "FRT => CostRatingBehavior: NEW, SellRatingBehavior: NEW" },
				expectedJobCharges: new string[] { "FRT => CostRatingBehavior: REA, SellRatingBehavior: REA" },
				message: "GIVEN ChargeLine with REA Cost/SellRatingBehavior WHEN ImportCharges THEN job charge Cost/SellRatingBehavior should be REA"
			);
		}

		void AssertCostSellRatingBehavior_JobCharges(Job job, Shipment universalShipment, ForwardingShipment shipment, string[] preConditionJobCharges, string[] expectedJobCharges, string message)
		{
			AssertContainsExactElementsInAnyOrder
			(
				"Precondition",
				preConditionJobCharges,
				job.Charges.Select(charge => $"{charge.ChargeCode.AC_Code} => CostRatingBehavior: {charge.JR_Calc_CostRatingBehavior}, SellRatingBehavior: {charge.JR_Calc_SellRatingBehavior}")
			);

			var adapter = new JobCostingAdapter();
			adapter.ImportCharges(Factory, Logger, universalShipment, shipment.PK, JobShipmentSchema.Constants.Prefix);

			Factory.Save();

			AssertContainsExactElementsInAnyOrder
			(
				"GIVEN ChargeLine with REA Cost/SellRatingBehavior WHEN ImportCharges THEN job charge Cost/SellRatingBehavior should be REA",
				expectedJobCharges,
				job.Charges.Select(charge => $"{charge.ChargeCode.AC_Code} => CostRatingBehavior: {charge.JR_Calc_CostRatingBehavior}, SellRatingBehavior: {charge.JR_Calc_SellRatingBehavior}")
			);
		}

		#endregion

		Shipment GetUniversalShipmentWithCharges(BusinessObjectFactory factory, TestObjectCreator creator, GlbCompany company, Job job, GlbBranch branch = null, GlbDepartment department = null)
		{
			Shipment universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New(); // Set DataContext but does not SetCompanyAndDataProviderDetails as it should be used from the TopLevelDataContext
			universalShipment.JobCosting = new JobCosting(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.JobCosting.Branch = new Branch();
			universalShipment.JobCosting.Branch.Code = branch?.GB_Code ?? "SYD";
			universalShipment.JobCosting.Department = new Department();
			universalShipment.JobCosting.Department.Code = department?.GE_Code ?? "FES";
			universalShipment.JobCosting.SetChargeLineCollection(() => new List<ChargeLine>());

			factory.Save();

			return universalShipment;
		}

		public void TestMaxLengths()
		{
			JobCostingAdapter jobCostingAdapter = new JobCostingAdapter();
			var keys = jobCostingAdapter.mapping.Keys.ToArray();

			CombineAssertions(() =>
			{
				foreach (var key in keys.Where(x => x is ChargeLineElementType))
				{
					var propertyInfo = typeof(ChargeLine).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.Name == ((ChargeLineElementType)key).ToString()).FirstOrDefault();
					AssertPropertyInfoMaxLength(key, ((ChargeLineElementType)key).ToString(), propertyInfo);
				}

				foreach (var key in keys.Where(x => x is JobCostingAdapter.CostPlaceOfSupplyElement))
				{
					var propertyInfo = typeof(PlaceOfSupply).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.Name == ((JobCostingAdapter.CostPlaceOfSupplyElement)key).ToString()).FirstOrDefault();
					propertyInfo = propertyInfo.PropertyType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.Name == "Code").FirstOrDefault();
					AssertPropertyInfoMaxLength(key, "Code", propertyInfo);
				}

				foreach (var key in keys.Where(x => x is JobCostingAdapter.SellPlaceOfSupplyElement))
				{
					var propertyInfo = typeof(PlaceOfSupply).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.Name == ((JobCostingAdapter.SellPlaceOfSupplyElement)key).ToString()).FirstOrDefault();
					propertyInfo = propertyInfo.PropertyType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.Name == "Code").FirstOrDefault();
					AssertPropertyInfoMaxLength(key, "Code", propertyInfo);
				}
			});

			void AssertPropertyInfoMaxLength(Enum key, string keyName, PropertyInfo info)
			{
				AssertNotNull($"{key} PropertyInfo", info);

				bool isZString = typeof(ZString?).IsAssignableFrom(info.PropertyType);
				var schemaStringColumn = jobCostingAdapter.mapping[key] as SchemaStringColumn;
				if (schemaStringColumn != null && isZString) // checks if property is string type in DB Schema and string type in XML schema  
				{
					var maxLength = GetMaxLengthFromXmlField(keyName, info);
					if (maxLength == 0)
					{
						Fail(keyName + " has no max length attribute");
					}
					else
					{
						AssertEquals(keyName + " max length does not match with DB schema", jobCostingAdapter.mapping[key].MaxLength, maxLength);
					}
				}
			}
		}

		static int GetMaxLengthFromXmlField(string xmlFieldName, PropertyInfo info)
		{
			var maxLengthAttribute = (MaxLengthAttribute)info.GetCustomAttributes(typeof(MaxLengthAttribute), false).FirstOrDefault();
			return (maxLengthAttribute != null) ? maxLengthAttribute.MaxLength : 0;
		}

		public void TestGenerateChargeLines()
			=> AssertGenerateChargeLines();

		public void TestGenerateChargeLines_EnableGovernmentChargeCode()
		{
			AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertGenerateChargeLines(enableGovernmentChargeCode: true);
		}

		public void TestGenerateChargeLines_EnableSupplyType()
		{
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertGenerateChargeLines(enableSupplyType: true);
		}

		void AssertGenerateChargeLines(bool enableGovernmentChargeCode = false, bool enableSupplyType = false)
		{
			Job job = Factory.NewJobForTesting<Job>();
			job.JH_JobNum = "TESTJOB1";
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var rateNZD = job.ExchangeRates.AddNew();
			rateNZD.JF_RX_NKRateCurrency = Constants.CurrencyCodes.NewZealand;
			rateNZD.JF_BaseRate = 1.259m;

			ChargeCodeGroupList chargeGroupList = new ChargeCodeGroupList();

			AssertNotEquals("PreCondition", "BON", TestObjectCreator.CC2.AC_ChargeGroup);
			TestObjectCreator.CC2.AC_ChargeGroup = "BON";
			JobCharge jobCharge1 = job.Charges.AddNew();
			jobCharge1.JR_AC = TestObjectCreator.CC2.PK;
			jobCharge1.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge1.JR_GE = GlbDepartment.CurrentDepartment.PK;
			jobCharge1.CostCurrency.RX_Code = "AUD";
			jobCharge1.CostCurrency.RX_Desc = "Australia, Dollars";
			jobCharge1.JR_OSCostAmt = 200m;
			jobCharge1.JR_LocalCostAmt = 200m;
			jobCharge1.JR_OH_CostAccount = TestObjectCreator.ABIGAS.PK;
			jobCharge1.SellCurrency.RX_Code = "AUD";
			jobCharge1.SellCurrency.RX_Desc = "Australia, Dollars";
			jobCharge1.JR_OSSellAmt = 200m;
			jobCharge1.JR_LocalSellAmt = 200m;
			jobCharge1.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			jobCharge1.JR_InvoiceType = "FID";
			jobCharge1.JR_DisplaySequence = 1;
			jobCharge1.SellAccount.CompanyData.OB_ARExternalDebtorCode = "AAA";
			jobCharge1.CostAccount.CompanyData.OB_APExternalCreditorCode = "BBB";
			jobCharge1.JR_CostReference = "ABC123";
			jobCharge1.JR_SellReference = "XYZ987";
			jobCharge1.JR_CostGovtChargeCode = "Cost Govt Chg Code 1";
			jobCharge1.JR_SellGovtChargeCode = "Sell Govt Chg Code 1";
			jobCharge1.JR_Calc_CostRatingBehavior = "NEW";
			jobCharge1.JR_CostSupplyType = "";
			jobCharge1.JR_SellSupplyType = "DSB";

			AssertNotEquals("PreCondition", "BON", TestObjectCreator.CC3.AC_ChargeGroup);
			TestObjectCreator.CC3.AC_ChargeGroup = "BON";
			JobCharge jobCharge2 = job.Charges.AddNew();
			jobCharge2.JR_AC = TestObjectCreator.CC3.PK;
			jobCharge2.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge2.JR_GE = GlbDepartment.CurrentDepartment.PK;
			jobCharge2.JR_RX_NKCostCurrency = Constants.CurrencyCodes.NewZealand;
			jobCharge2.JR_RX_NKSellCurrency = Constants.CurrencyCodes.NewZealand;
			jobCharge2.JR_LocalCostAmt = 100m;
			jobCharge2.JR_LocalSellAmt = 100m;
			jobCharge2.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			jobCharge2.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			jobCharge2.JR_InvoiceType = "FIN";
			jobCharge2.JR_DisplaySequence = 2;
			jobCharge2.SellAccount.CompanyData.OB_ARExternalDebtorCode = "CCC";
			jobCharge2.CostAccount.CompanyData.OB_APExternalCreditorCode = "DDD";
			jobCharge2.ChargeCode.AC_ChargeGroup = "BON";
			jobCharge2.JR_CostReference = "BCD234";
			jobCharge2.JR_SellReference = "CUX654";
			jobCharge2.JR_CostGovtChargeCode = "Cost Govt Chg Code 2";
			jobCharge2.JR_SellGovtChargeCode = "Sell Govt Chg Code 2";
			jobCharge1.JR_Calc_CostRatingBehavior = "NEW";
			jobCharge2.JR_CostSupplyType = "LOC";
			jobCharge2.JR_SellSupplyType = "";

			Factory.Save();

			JobCostingAdapter jobSummaryAdapter = new JobCostingAdapter();
			JobCosting loadedJob = jobSummaryAdapter.GenerateForTesting(job.PK);

			AssertItemEquals("[Charge Line 1]", jobCharge1, loadedJob.ChargeLineCollection[0]);
			AssertItemEquals("[Charge Line 2]", jobCharge2, loadedJob.ChargeLineCollection[1]);

			void AssertItemEquals(string comment, JobCharge jobCharge, ChargeLine chargeLine)
			{
				AssertEquals($"{comment} Branch Code", jobCharge.Branch.GB_Code, chargeLine.Branch.Code);
				AssertEquals($"{comment} Branch Name", jobCharge.Branch.GB_BranchName, chargeLine.Branch.Name);
				AssertEquals($"{comment} CostOSCurrency Code", jobCharge.CostCurrency.RX_Code, chargeLine.CostOSCurrency.Code);
				AssertEquals($"{comment} CostOSCurrency Description", jobCharge.CostCurrency.RX_Desc, chargeLine.CostOSCurrency.Description);
				AssertEquals($"{comment} Department Code", jobCharge.Department.GE_Code, chargeLine.Department.Code);
				AssertEquals($"{comment} Department Name", jobCharge.Department.GE_Desc, chargeLine.Department.Name);
				AssertEquals($"{comment} SellOSCurrency Code", jobCharge.SellCurrency.RX_Code, chargeLine.SellOSCurrency.Code);
				AssertEquals($"{comment} SellOSCurrency Description", jobCharge.SellCurrency.RX_Desc, chargeLine.SellOSCurrency.Description);
				AssertEquals($"{comment} ChargeCode Code", jobCharge.ChargeCode.AC_Code, chargeLine.ChargeCode.Code);
				AssertEquals($"{comment} ChargeCode Description", jobCharge.ChargeCode.AC_Desc, chargeLine.ChargeCode.Description);
				AssertEquals($"{comment} ChargeCodeGroup Code", jobCharge.ChargeCode.AC_ChargeGroup, chargeLine.ChargeCodeGroup.Code);
				AssertEquals($"{comment} ChargeCodeGroup Description", chargeGroupList.GetDescriptionFromCode("BON"), chargeLine.ChargeCodeGroup.Description);
				AssertEquals($"{comment} CostLocalAmount", jobCharge.JR_LocalCostAmt, chargeLine.CostLocalAmount);
				AssertEquals($"{comment} CostOSAmount", jobCharge.JR_OSCostAmt, chargeLine.CostOSAmount);
				AssertEquals($"{comment} Debtor Type", nameof(DataContextType.Organization), chargeLine.Debtor.Type);
				AssertEquals($"{comment} Debtor Key", jobCharge.SellAccount.OH_Code, chargeLine.Debtor.Key);
				AssertEquals($"{comment} SellInvoiceType", jobCharge.JR_InvoiceType, chargeLine.SellInvoiceType);
				AssertEquals($"{comment} SellLocalAmount", jobCharge.JR_LocalSellAmt, chargeLine.SellLocalAmount);
				AssertEquals($"{comment} SellOSAmount", jobCharge.JR_OSSellAmt, chargeLine.SellOSAmount);
				AssertEquals($"{comment} InvoiceType", jobCharge.JR_InvoiceType, chargeLine.SellInvoiceType);
				AssertEquals($"{comment} DisplaySequence", jobCharge.JR_DisplaySequence, chargeLine.DisplaySequence);
				AssertEquals($"{comment} ExternalDebtorCode", jobCharge.SellAccount.CompanyData.OB_ARExternalDebtorCode, chargeLine.ExternalDebtorCode);
				AssertEquals($"{comment} ExternalCreditorCode", jobCharge.CostAccount.CompanyData.OB_APExternalCreditorCode, chargeLine.ExternalCreditorCode);
				AssertEquals($"{comment} CostIsPosted", jobCharge.IsCostPosted, chargeLine.CostIsPosted);
				AssertEquals($"{comment} SellIsPosted", jobCharge.IsRevenuePosted, chargeLine.SellIsPosted);
				AssertEquals($"{comment} SupplierReference", jobCharge.JR_CostReference, chargeLine.SupplierReference);
				AssertEquals($"{comment} SellReference", jobCharge.JR_SellReference, chargeLine.SellReference);

				if (enableGovernmentChargeCode)
				{
					AssertEquals($"{comment} GovernmentReportingCostChargeCode", jobCharge.JR_CostGovtChargeCode, chargeLine.GovernmentReportingCostChargeCode);
					AssertEquals($"{comment} GovernmentReportingSellChargeCode", jobCharge.JR_SellGovtChargeCode, chargeLine.GovernmentReportingSellChargeCode);
				}
				else
				{
					AssertEquals($"{comment} GovernmentReportingCostChargeCode", null, chargeLine.GovernmentReportingCostChargeCode);
					AssertEquals($"{comment} GovernmentReportingSellChargeCode", null, chargeLine.GovernmentReportingSellChargeCode);
				}

				AssertEquals($"{comment} CostExchangeRate", jobCharge.JR_OSCostExRate, chargeLine.CostExchangeRate);
				AssertEquals($"{comment} SellExchangeRate", jobCharge.JR_OSSellExRate, chargeLine.SellExchangeRate);

				if (enableSupplyType)
				{
					var expectedCostSupplyType = AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.Value.FindByCode(jobCharge.JR_CostSupplyType);
					AssertEquals($"{comment} Consol Cost SupplyType", expectedCostSupplyType?.Code ?? jobCharge.JR_CostSupplyType, chargeLine.CostSupplyType.Code);
					AssertEquals($"{comment} Consol Cost SupplyType", expectedCostSupplyType?.Description ?? ZString.Empty, chargeLine.CostSupplyType.Description);

					var expectedSellSupplyType = AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.Value.FindByCode(jobCharge.JR_SellSupplyType);
					AssertEquals($"{comment} Consol Cost SupplyType", expectedSellSupplyType?.Code ?? jobCharge.JR_SellSupplyType, chargeLine.SellSupplyType.Code);
					AssertEquals($"{comment} Consol Cost SupplyType", expectedSellSupplyType?.Description ?? ZString.Empty, chargeLine.SellSupplyType.Description);
				}
				else
				{
					AssertEquals($"{comment} SellExchangeRate", null, chargeLine.CostSupplyType);
					AssertEquals($"{comment} SellExchangeRate", null, chargeLine.SellSupplyType);
				}
			}
		}

		public void TestGenerateChargeLinesWithTaxBranch()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var taxBranch = TestObjectCreator.CreateBranch("BB", "TaxBranch", GlbCompany.CurrentCompany);
				var costTaxBranch = TestObjectCreator.CreateBranch("B1", "CostTaxBranch", GlbCompany.CurrentCompany);
				var sellTaxBranch = TestObjectCreator.CreateBranch("B2", "SellTaxBranch", GlbCompany.CurrentCompany);

				var job = Factory.NewJobForTesting<Job>();
				job.JH_JobNum = "TESTJOB1";
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_GB_TaxBranch = taxBranch.PK;

				var jobCharge1 = job.Charges.AddNew();
				jobCharge1.JR_AC = TestObjectCreator.CC2.PK;
				jobCharge1.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
				jobCharge1.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
				jobCharge1.JR_LocalCostAmt = 100m;
				jobCharge1.JR_LocalSellAmt = 100m;
				jobCharge1.JR_GB_CostTaxBranch = costTaxBranch.PK;
				jobCharge1.JR_GB_SellTaxBranch = sellTaxBranch.PK;

				Factory.Save();

				var adapter = new JobCostingAdapter();
				JobCosting loadedJob = adapter.GenerateForTesting(job.PK);

				AssertEquals("BB", loadedJob.TaxBranch.Code);
				AssertEquals("TaxBranch", loadedJob.TaxBranch.Name);

				AssertEquals("B1", loadedJob.ChargeLineCollection[0].CostTaxBranch.Code);
				AssertEquals("CostTaxBranch", loadedJob.ChargeLineCollection[0].CostTaxBranch.Name);

				AssertEquals("B2", loadedJob.ChargeLineCollection[0].SellTaxBranch.Code);
				AssertEquals("SellTaxBranch", loadedJob.ChargeLineCollection[0].SellTaxBranch.Name);
			}
		}

		public void TestGenerateChargeLinesWithPlaceOfSupply()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				var job = Factory.NewJobForTesting<Job>();
				job.JH_JobNum = "TESTJOB1";
				var jobCharge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, osCostAmt: 100m, creditor: TestObjectCreator.AALSHI, osSellAmt: 100m, debtor: TestObjectCreator.AALSHI);
				jobCharge.JR_CostPlaceOfSupply = "JH";
				jobCharge.JR_SellPlaceOfSupply = PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry;

				Factory.Save();

				var loadedJob = new JobCostingAdapter().GenerateForTesting(job.PK);
				var chargeLine = loadedJob.ChargeLineCollection[0];

				AssertEquals("JH", chargeLine.CostPlaceOfSupply.Location.Code);
				AssertEquals("Jharkhand", chargeLine.CostPlaceOfSupply.Location.Description);
				AssertEquals(PlaceOfSupplyTypes.State.Code, chargeLine.CostPlaceOfSupply.LocationType.Code);
				AssertEquals(PlaceOfSupplyTypes.State.Description, chargeLine.CostPlaceOfSupply.LocationType.Description);

				AssertEquals(PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry, chargeLine.SellPlaceOfSupply.Location.Code);
				AssertEquals(PlaceOfSupplyListProvider.Descriptions.OutsideTheLoginCountry, chargeLine.SellPlaceOfSupply.Location.Description);
				AssertEquals(PlaceOfSupplyTypes.PredefinedRule.Code, chargeLine.SellPlaceOfSupply.LocationType.Code);
				AssertEquals(PlaceOfSupplyTypes.PredefinedRule.Description, chargeLine.SellPlaceOfSupply.LocationType.Description);
			}
		}

		public void TestGenerateChargeLinesWithPlaceOfSupplyIncludesOtherTerritories()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.PredefinedRule.Code))
			{
				var job = Factory.NewJobForTesting<Job>();
				job.JH_JobNum = "TESTJOB1";
				var jobCharge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, osCostAmt: 100m, creditor: TestObjectCreator.AALSHI, osSellAmt: 100m, debtor: TestObjectCreator.AALSHI);
				jobCharge.JR_CostPlaceOfSupply = PlaceOfSupplyListProvider.Codes.OtherTerritories;
				jobCharge.JR_SellPlaceOfSupply = PlaceOfSupplyListProvider.Codes.OtherTerritories;

				Factory.Save();

				var loadedJob = new JobCostingAdapter().GenerateForTesting(job.PK);
				var chargeLine = loadedJob.ChargeLineCollection[0];

				AssertEquals(PlaceOfSupplyListProvider.Codes.OtherTerritories, chargeLine.CostPlaceOfSupply.Location.Code);
				AssertEquals(PlaceOfSupplyListProvider.Descriptions.OtherTerritories, chargeLine.CostPlaceOfSupply.Location.Description);
				AssertEquals(PlaceOfSupplyTypes.PredefinedRule.Code, chargeLine.CostPlaceOfSupply.LocationType.Code);
				AssertEquals(PlaceOfSupplyTypes.PredefinedRule.Description, chargeLine.CostPlaceOfSupply.LocationType.Description);

				AssertEquals(PlaceOfSupplyListProvider.Codes.OtherTerritories, chargeLine.SellPlaceOfSupply.Location.Code);
				AssertEquals(PlaceOfSupplyListProvider.Descriptions.OtherTerritories, chargeLine.SellPlaceOfSupply.Location.Description);
				AssertEquals(PlaceOfSupplyTypes.PredefinedRule.Code, chargeLine.SellPlaceOfSupply.LocationType.Code);
				AssertEquals(PlaceOfSupplyTypes.PredefinedRule.Description, chargeLine.SellPlaceOfSupply.LocationType.Description);
			}
		}

		public void TestGenerateChargeLinesWithCashAdvanceRequestData()
		{
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipment = TestObjectCreator.CreateShipment("S00001001");
				var job = TestObjectCreator.CreateJob(shipment, false);
				Factory.Save();
				var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, 200m, 100m);
				charge1.JR_OH_SellAccount = TestObjectCreator.Debtor1.PK;
				charge1.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
				var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, 1000m, 500m);
				charge2.JR_OH_SellAccount = TestObjectCreator.Debtor1.PK;
				charge2.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
				var charge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, 33m, 33m);
				charge3.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
				charge3.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
				var charge4 = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, 55m, 55m);
				charge4.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
				charge4.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
				Factory.Save();
				var caheader1 = TestObjectCreator.CreateCashAdvanceRequestHeader(job, TestObjectCreator.Debtor1, LedgerTypes.AccountsReceivable, 0M, 0M, "USD", "0000001");
				TestObjectCreator.CreateCashAdvanceRequestLine(caheader1, 200M, 100M, CashAdvanceStatusCodes.RequestLine.Requested, charge1);
				TestObjectCreator.CreateCashAdvanceRequestLine(caheader1, 1000M, 500M, CashAdvanceStatusCodes.RequestLine.Requested, charge2);

				var caheader2 = TestObjectCreator.CreateCashAdvanceRequestHeader(job, TestObjectCreator.AALSHI, LedgerTypes.AccountsReceivable, 0M, 0M, "AUD", "0000002");
				caheader2.CAH_Status = CashAdvanceStatusCodes.RequestHeader.PartiallyPaid;
				TestObjectCreator.CreateCashAdvanceRequestLine(caheader2, 33M, 33M, CashAdvanceStatusCodes.RequestLine.Paid, charge3, 33M, 33M);
				TestObjectCreator.CreateCashAdvanceRequestLine(caheader2, 55M, 55M, CashAdvanceStatusCodes.RequestLine.Requested, charge4);

				Factory.Save();

				var adapter = new JobCostingAdapter();
				JobCosting loadedJob = adapter.GenerateForTesting(job.PK);

				AssertNotNull(loadedJob.CashAdvanceRequestHeaderCollection);
				AssertEquals(2, loadedJob.CashAdvanceRequestHeaderCollection.Count);
				var header1 = loadedJob.CashAdvanceRequestHeaderCollection[0];
				AssertCashAdvanceRequestHeader(header1, "0000001", CashAdvanceStatusCodes.RequestHeader.Requested, LedgerTypes.AccountsReceivable, TestObjectCreator.Debtor1.OH_Code, "USD", 1200M, 0M, 600M, 0M);

				var header2 = loadedJob.CashAdvanceRequestHeaderCollection[1];
				AssertCashAdvanceRequestHeader(header2, "0000002", CashAdvanceStatusCodes.RequestHeader.PartiallyPaid, LedgerTypes.AccountsReceivable, TestObjectCreator.AALSHI.OH_Code, "AUD", 88M, 33M, 88M, 33M);

				var chargeLine1 = loadedJob.ChargeLineCollection[0];
				AssertNotNull(chargeLine1);
				Assert(chargeLine1.ARCashAdvanceRequired.Value);
				Assert(!chargeLine1.APCashAdvanceRequired.Value);
				var arCashAdvanceLine1 = chargeLine1.ARCashAdvanceRequestLine;
				AssertCashAdvanceRequestLine(arCashAdvanceLine1, "0000001", CashAdvanceStatusCodes.RequestLine.Requested, 200M, 0M, 100M, 0M);
				AssertNull(chargeLine1.APCashAdvanceRequestLine);

				var chargeLine2 = loadedJob.ChargeLineCollection[1];
				AssertNotNull(chargeLine2);
				Assert(chargeLine2.ARCashAdvanceRequired.Value);
				var arCashAdvanceLine2 = chargeLine2.ARCashAdvanceRequestLine;
				AssertCashAdvanceRequestLine(arCashAdvanceLine2, "0000001", CashAdvanceStatusCodes.RequestLine.Requested, 1000M, 0M, 500M, 0M);
				Assert(!chargeLine2.APCashAdvanceRequired.Value);

				var chargeLine3 = loadedJob.ChargeLineCollection[2];
				AssertNotNull(chargeLine3);
				Assert(chargeLine3.ARCashAdvanceRequired.Value);
				Assert(!chargeLine3.APCashAdvanceRequired.Value);
				var arCashAdvanceLine3 = chargeLine3.ARCashAdvanceRequestLine;
				AssertCashAdvanceRequestLine(arCashAdvanceLine3, "0000002", CashAdvanceStatusCodes.RequestLine.Paid, 33M, 33M, 33M, 33M);
				AssertNull(chargeLine3.APCashAdvanceRequestLine);

				var chargeLine4 = loadedJob.ChargeLineCollection[3];
				AssertNotNull(chargeLine4);
				Assert(chargeLine4.ARCashAdvanceRequired.Value);
				var arCashAdvanceLine4 = chargeLine4.ARCashAdvanceRequestLine;
				AssertCashAdvanceRequestLine(arCashAdvanceLine4, "0000002", CashAdvanceStatusCodes.RequestLine.Requested, 55M, 0M, 55M, 0M);
				Assert(!chargeLine4.APCashAdvanceRequired.Value);
			}
		}

		void AssertCashAdvanceRequestHeader(CashAdvanceRequestHeader cashAdvanceHeader, ZString expectedRefNumber, ZString expectedStatus, ZString expectedLedger, ZString expectedOrgCode, ZString expectedCurrencyCode,
																			ZDecimal expectedLocalAmount, ZDecimal expectedLocalPaidAmount, ZDecimal expectedOSAmount, ZDecimal expectedOSPaidAmount)
		{
			AssertEquals(expectedRefNumber, cashAdvanceHeader.RequestReferenceNumber);
			AssertEquals(expectedStatus, cashAdvanceHeader.Status);
			AssertEquals(expectedLedger, cashAdvanceHeader.Ledger);
			AssertNotNull(cashAdvanceHeader.OrgHeader);
			AssertEquals(expectedOrgCode, cashAdvanceHeader.OrgHeader.Key);
			AssertEquals(nameof(DataContextType.Organization), cashAdvanceHeader.OrgHeader.Type);
			AssertNotNull(cashAdvanceHeader.Currency);
			AssertEquals(expectedCurrencyCode, cashAdvanceHeader.Currency.Code);
			AssertEquals(expectedLocalAmount, cashAdvanceHeader.LocalAmount);
			AssertEquals(expectedLocalPaidAmount, cashAdvanceHeader.LocalPaidAmount);
			AssertEquals(expectedOSAmount, cashAdvanceHeader.OSAmount);
			AssertEquals(expectedOSPaidAmount, cashAdvanceHeader.OSPaidAmount);
		}

		void AssertCashAdvanceRequestLine(CashAdvanceRequestLine cashAdvanceLine, ZString expectedRefNumber, ZString expectedStatus,
																		ZDecimal expectedLocalAmount, ZDecimal expectedLocalPaidAmount, ZDecimal expectedOSAmount, ZDecimal expectedOSPaidAmount)
		{
			AssertNotNull(cashAdvanceLine);
			AssertEquals(expectedRefNumber, cashAdvanceLine.RequestReferenceNumber);
			AssertEquals(expectedStatus, cashAdvanceLine.Status);
			AssertEquals(expectedLocalAmount, cashAdvanceLine.LocalAmount);
			AssertEquals(expectedLocalPaidAmount, cashAdvanceLine.LocalPaidAmount);
			AssertEquals(expectedOSAmount, cashAdvanceLine.OSAmount);
			AssertEquals(expectedOSPaidAmount, cashAdvanceLine.OSPaidAmount);
		}

		public void TestCostApportionmentConsolNumber()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			IJobInvoicingPlugIn shipment = consol.Shipments.AddNew();
			Job job = creator.CreateJob(shipment);
			Factory.Save();

			ApportionmentListing apportionments = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apportionments.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = creator.CC1.PK;
			cost.E6_OSCostAmount = 60m;
			cost.E6_ApportionmentMethod = AllocationMethod.Shipment;
			cost.ApportionmentCharges[0].JR_OSCostAmt = 60m;

			Factory.Save();

			JobCostingAdapter jobSummaryAdapter = new JobCostingAdapter();
			JobCosting loadedJob = jobSummaryAdapter.Generate(shipment, DefaultDataObjectWriterStrategy.TestInstance);

			AssertEquals("CostApportionmentConsolNumber Key", cost.GenericConsolBizO.VX_Code, loadedJob.ChargeLineCollection[0].CostApportionmentConsolNumber.Key);
			AssertEquals("CostApportionmentConsolNumber Type", nameof(DataContextType.ForwardingConsol), loadedJob.ChargeLineCollection[0].CostApportionmentConsolNumber.Type);
		}

		public void TestGenerate()
		{
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_ParentID = ZGuid.NewZGuid();
			job.JH_ParentTableCode = "XX";
			job.JH_JobNum = "TESTJOB1";
			job.JH_GE = TestObjectCreator.FESDepartment.PK;

			StaffSetup(job);
			BaseLineSetup(job, 120m, 35m, -140m, -65m, 135m, 50m, 35m, 55m);
			ChargeSetup(job);

			Factory.Save();

			JobCostingAdapter jobSummaryAdapter = new JobCostingAdapter();
			JobCosting loadedJob = jobSummaryAdapter.GenerateForTesting(job.PK);

			AssertEquals("Branch Code", job.Branch.GB_Code, loadedJob.Branch.Code);
			AssertEquals("Branch Name", job.Branch.GB_BranchName, loadedJob.Branch.Name);
			AssertEquals("Department Code", TestObjectCreator.FESDepartment.GE_Code, loadedJob.Department.Code);
			AssertEquals("Department Name", TestObjectCreator.FESDepartment.GE_Desc, loadedJob.Department.Name);
			AssertEquals("Sales Staff Code", "SS", loadedJob.SalesStaff.Code);
			AssertEquals("Sales Staff Name", "Sales", loadedJob.SalesStaff.Name);
			AssertEquals("Operations Staff Code", "OS", loadedJob.OperationsStaff.Code);
			AssertEquals("Operations Staff Name", "Operations", loadedJob.OperationsStaff.Name);
			AssertEquals("Home Branch Code", "SYD", loadedJob.HomeBranch.Code);
			AssertEquals("Home Branch Name", "Sydney Branch", loadedJob.HomeBranch.Name);
			AssertEquals("Total Revenue", 155m, loadedJob.TotalRevenue);
			AssertEquals("Total Cost", -205m, loadedJob.TotalCost);
			AssertEquals("Total WIP", 485m, loadedJob.TotalWIP);
			AssertEquals("Total Accrual", -390m, loadedJob.TotalAccrual);
			AssertEquals("WIP Recognized", 185m, loadedJob.WIPRecognized);
			AssertEquals("WIP Not Recognized", 300m, loadedJob.WIPNotRecognized);
			AssertEquals("Accrual Recognized", -90m, loadedJob.AccrualRecognized);
			AssertEquals("Accrual Not Recognized", -300m, loadedJob.AccrualNotRecognized);
			AssertEquals("Total Profit Loss", 45m, loadedJob.TotalJobProfit);

			Job job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_ParentID = ZGuid.NewZGuid();
			job2.JH_ParentTableCode = "XX";
			job2.JH_JobNum = "TESTJOB2";
			job2.AgentCollectPK = TestObjectCreator.AALSHI.PK;

			BaseLineSetup(job2, 100m, 20m, -100m, -80m, 150m, 40m, 30m, 50m);
			ChargeSetup(job2);

			Factory.Save();

			jobSummaryAdapter = new JobCostingAdapter();
			loadedJob = jobSummaryAdapter.GenerateForTesting(job2.PK);

			AssertEquals("Agent Revenue for the Overseas Agent", 120m, loadedJob.AgentRevenue);

			Job job3 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job3.JH_ParentID = ZGuid.NewZGuid();
			job3.JH_ParentTableCode = "XX";
			job3.JH_JobNum = "TESTJOB3";
			job3.AgentCollectPK = TestObjectCreator.AALSHI.PK;

			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			job3.PlugInData = shipment;

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Shipments.Add(shipment);
			consol.JK_OA_ReceivingForwarderAddress = TestObjectCreator.AALSHI.MainAddress.PK;

			BaseLineSetup(job3, 90m, 40m, -200m, -30m, 130m, 40m, 40m, 60m);
			ChargeSetup(job3);

			Factory.Save();

			jobSummaryAdapter = new JobCostingAdapter();

			loadedJob = jobSummaryAdapter.GenerateForTesting(job3.PK);

			AssertEquals("Agent Revenue for the Consol Receiving Agent", 130m, loadedJob.AgentRevenue);

			Job job4 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job4.JH_ParentID = ZGuid.NewZGuid();
			job4.JH_ParentTableCode = "XX";
			job4.JH_JobNum = "TESTJOB4";
			job4.LocalChargesPK = TestObjectCreator.AALSHI.PK;

			BaseLineSetup(job4, 70m, 100m, -70m, -100m, 200m, 20m, 10m, 60m);
			ChargeSetup(job4);

			Factory.Save();

			jobSummaryAdapter = new JobCostingAdapter();
			loadedJob = jobSummaryAdapter.GenerateForTesting(job4.PK);

			AssertEquals("Local Client Revenue", 170m, loadedJob.LocalClientRevenue);

			Job job5 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job5.JH_ParentID = ZGuid.NewZGuid();
			job5.JH_ParentTableCode = "XX";
			job5.JH_JobNum = "TESTJOB5";

			BaseLineSetup(job5, 90m, 50m, -70m, -100m, 200m, 20m, 10m, 60m);
			ChargeSetup(job5);

			Factory.Save();

			jobSummaryAdapter = new JobCostingAdapter();
			loadedJob = jobSummaryAdapter.GenerateForTesting(job5.PK);

			AssertEquals("Other Debtor Revenue", 140m, loadedJob.OtherDebtorRevenue);
		}

		public void TestGeneratePostingInfo()
		{
			Job job = Factory.NewJobForTesting<Job>();
			job.JH_JobNum = "TESTJOB1";
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			StaffSetup(job);
			BaseLineSetup(job, 120m, 35m, -140m, -65m, 135m, 50m, 35m, 55m);
			ChargeSetup(job);

			Factory.Save();

			JobCostingAdapter jobSummaryAdapter = new JobCostingAdapter();
			JobCosting loadedJob = jobSummaryAdapter.GenerateForTesting(job.PK);

			var costPosted = loadedJob.ChargeLineCollection.Where(x => x.CostIsPosted.HasValue && (bool)x.CostIsPosted.Value);
			AssertEquals("Two CostPosted Charges", 2, costPosted.Count());

			var sellPosted = loadedJob.ChargeLineCollection.Where(x => x.SellIsPosted.HasValue && (bool)x.SellIsPosted.Value);
			AssertEquals("Two SellPosted Charges", 2, sellPosted.Count());
			AssertEquals("SellPostedTransactionNumber", 2, sellPosted.Count(x => x.SellPostedTransactionNumber.HasValue && x.SellPostedTransactionNumber.ToString() == "00001000"));
			AssertEquals("SellPostedTransactionType", 2, sellPosted.Count(x => x.SellPostedTransactionType.HasValue && x.SellPostedTransactionType.ToString() == "INV"));
		}

		public void TestGeneratePostingInfo_SellPosted()
		{
			Job job = Factory.NewJobForTesting<Job>();
			job.JH_JobNum = "TESTJOB1";
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			StaffSetup(job);

			var revRecOverride = TestObjectCreator.CC2.RevenueRecOverrides.AddNew();
			revRecOverride.JobType = RevenueRecognitionLookups.JobTypeAdditionalCodes.All;
			revRecOverride.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure;
			revRecOverride.Offset = 0;

			var aRInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("123654" + job.JH_JobNum, GlbCompany.CurrentCompany.LocalCurrency, 1, TestObjectCreator.AALSHI);
			aRInvoice.AH_InvoiceDate = new DateTime(2021, 09, 06);
			aRInvoice.AH_DueDate = new DateTime(2021, 09, 06);
			var rev1 = (TransactionLine)aRInvoice.Lines.AddNew();
			SetLine(job, rev1, TransactionLineTypes.Revenue, 155, TestObjectCreator.CC1);
			var rev1JobCharge = TestObjectCreator.CreateJobCharge(rev1, job, rev1.ChargeCode, rev1.TransactionCurrency);
			rev1JobCharge.JR_OSCostAmt = 0;

			ChargeSetup(job);

			Factory.Save();

			var jobSummaryAdapter = new JobCostingAdapter();
			var loadedJob = jobSummaryAdapter.GenerateForTesting(job.PK);

			var sellPosted = loadedJob.ChargeLineCollection.Where(x => x.SellIsPosted.HasValue && (bool)x.SellIsPosted.Value);
			AssertEquals("One SellPosted Charges", 1, sellPosted.Count());
			AssertEquals("SellPostedTransactionNumber", 1, sellPosted.Count(x => x.SellPostedTransactionNumber.HasValue && x.SellPostedTransactionNumber.ToString() == "00001000"));
			AssertEquals("SellPostedTransactionType", 1, sellPosted.Count(x => x.SellPostedTransactionType.HasValue && x.SellPostedTransactionType.ToString() == "INV"));

			var sellPostedTransactions = sellPosted.Select(x => x.SellPostedTransaction);

			AssertEquals("SellPostedTransactionDate", 1, sellPostedTransactions.Count(x => x.Number.HasValue && x.Number.ToString() == "00001000"));
			AssertEquals("SellPostedTransactionDate", 1, sellPostedTransactions.Count(x => x.TransactionType.HasValue && x.TransactionType == TransactionType.INV));
			AssertEquals("SellPostedTransactionDate", 1, sellPostedTransactions.Count(x => x.TransactionDate.HasValue && x.TransactionDate == new ZDateTime(2021, 09, 06)));
			AssertEquals("SellPostedTransactionDueDate", 1, sellPostedTransactions.Count(x => x.DueDate.HasValue && x.DueDate == new ZDateTime(2021, 09, 06)));
			AssertEquals("SellPostedTransactionOutstandingAmount", 1, sellPostedTransactions.Count(x => x.OutstandingAmount.HasValue && x.OutstandingAmount == 155));

			TestObjectCreator.CreateAndMatchARReceiptForARInvoice(aRInvoice, ZDateTime.Today);
			aRInvoice.AH_FullyPaidDate = new ZDateTime(2021, 09, 06);
			Factory.Save();

			loadedJob = jobSummaryAdapter.GenerateForTesting(job.PK);
			sellPosted = loadedJob.ChargeLineCollection.Where(x => x.SellIsPosted.HasValue && (bool)x.SellIsPosted.Value);
			sellPostedTransactions = sellPosted.Select(x => x.SellPostedTransaction);

			AssertEquals("SellPostedTransactionFullyPaidDate", 1, sellPostedTransactions.Count(x => x.FullyPaidDate.HasValue && x.FullyPaidDate == new ZDateTime(2021, 09, 06)));
			AssertEquals("SellPostedTransactionOutstandingAmount", 1, sellPostedTransactions.Count(x => x.OutstandingAmount.HasValue && x.OutstandingAmount == 0));
		}

		public void TestSellPostedTransactionsMandatoryProperty()
		{
			var job = Factory.NewJobForTesting<Job>();
			job.JH_JobNum = "TESTJOB1";
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			StaffSetup(job);

			var revRecOverride = TestObjectCreator.CC2.RevenueRecOverrides.AddNew();
			revRecOverride.JobType = RevenueRecognitionLookups.JobTypeAdditionalCodes.All;
			revRecOverride.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure;
			revRecOverride.Offset = 0;

			var aRInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("123654" + job.JH_JobNum, GlbCompany.CurrentCompany.LocalCurrency, 1, TestObjectCreator.AALSHI);
			aRInvoice.AH_InvoiceDate = new DateTime(2021, 09, 06);
			aRInvoice.AH_DueDate = new DateTime(2021, 09, 06);
			var rev1 = (TransactionLine)aRInvoice.Lines.AddNew();
			SetLine(job, rev1, TransactionLineTypes.Revenue, 155, TestObjectCreator.CC1);
			var rev1JobCharge = TestObjectCreator.CreateJobCharge(rev1, job, rev1.ChargeCode, rev1.TransactionCurrency);
			rev1JobCharge.JR_OSCostAmt = 0;

			ChargeSetup(job);

			Factory.Save();

			var jobSummaryAdapter = new JobCostingAdapter();
			var loadedJob = jobSummaryAdapter.GenerateForTesting(job.PK);

			var sellPostedTransactions = loadedJob.ChargeLineCollection.Where(x => x.SellIsPosted.HasValue && (bool)x.SellIsPosted.Value).Select(x => x.SellPostedTransaction);

			foreach (var sellPostedTransaction in sellPostedTransactions)
			{
				foreach (var propertyInfo in sellPostedTransaction.GetType().GetProperties())
				{
					if (propertyInfo.GetCustomAttributes(typeof(MandatoryAttribute), false).Length > 0)
					{
						AssertNotNull(propertyInfo.GetValue(sellPostedTransaction));
					}
				}
			}

			Assert(true);
		}

		[ExpectNoExceptions]
		public void TestGenerateWithInvalidRefCurrency()
		{
			var branch = Factory.LoadTop1<GlbBranch>(new ZQuery()).PK.ToGuid();
			var department = Factory.LoadTop1<GlbDepartment>(new ZQuery()).PK.ToGuid();
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "EEE";
			staff.GS_LoginName = "E";
			Factory.Save();

			using (Env.SetTemporaryUserContext("E", branch, department))
			{
				var job = TestObjectCreator.Job1;

				var company = GlbCompany.GetCurrentCompany(Factory);
				var localCurrency = company.LocalCurrency;
				var originalValue = company.LocalCurrency.RX_Code;
				try
				{
					company.LocalCurrency.RX_Code = "Z";
					Factory.Save();

					var loadedJob = new JobCostingAdapter().GenerateForTesting(job.PK);
					AssertEquals("Local currency description should not exist as RefCurrency is invalid.", null, loadedJob.Currency.Description);
				}
				finally
				{
					localCurrency.RX_Code = originalValue;
					Factory.Save();
				}
			}
		}

		[TestDate(2020, 10, 20)]
		public void TestImportChargeOverrideCostTaxId()
		{
			AssertJobParentWhenImportCharge(false, true);
		}

		[TestDate(2020, 10, 20)]
		public void TestImportChargeOverrideCostTaxId_InServiceTask()
		{
			AssertJobParentWhenImportCharge(true, true);
		}

		[TestDate(2020, 10, 20)]
		public void TestImportChargeOverrideCostTaxId_WhenRevenuePosted()
		{
			AssertJobParentWhenImportCharge(false, true, isPostRevenue: true);
		}

		[TestDate(2020, 10, 20)]
		public void TestImportChargeOverrideCostTaxId_WhenRevenuePosted_InServiceTask()
		{
			AssertJobParentWhenImportCharge(true, true, isPostRevenue: true);
		}

		[TestDate(2020, 10, 20)]
		public void TestImportChargeOverrideSellTaxId()
		{
			AssertJobParentWhenImportCharge(false, false);
		}

		[TestDate(2020, 10, 20)]
		public void TestImportChargeOverrideSellTaxId_InServiceTask()
		{
			AssertJobParentWhenImportCharge(true, false);
		}

		[TestDate(2020, 10, 20)]
		public void TestImportChargeOverrideSellTaxId_WhenCostPosted()
		{
			AssertJobParentWhenImportCharge(false, false, isPostCost: true);
		}

		[TestDate(2020, 10, 20)]
		public void TestImportChargeOverrideSellTaxId_WhenCostPosted_InServiceTask()
		{
			AssertJobParentWhenImportCharge(true, false, isPostCost: true);
		}

		void AssertJobParentWhenImportCharge(bool isSimulateServiceTaskContext, bool isAssertCost, bool isPostCost = false, bool isPostRevenue = false)
		{
			AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var defaultTaxId = TestObjectCreator.GST1.PK;
			TestObjectCreator.Debtor1.CompanyData.SetARTaxApplicable(true);
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S00001001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, 100m, 100m);
			charge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			charge.JR_OH_SellAccount = TestObjectCreator.Debtor1.PK;
			Factory.Save();
			charge.JR_AT_CostGSTRate = defaultTaxId;
			charge.JR_AT_SellGSTRate = defaultTaxId;
			var apInvoiceNum = "TestImport001AP";
			charge.JR_APInvoiceNum = apInvoiceNum;
			Factory.Save();

			if (isPostCost)
			{
				var transaction = TestObjectCreator.CreateAPInvoice<APInvoice>("Test001AP", TestObjectCreator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.Creditor1);
				var line = TestObjectCreator.CreateCostLine(charge, transaction.PK);
				Factory.Save();
			}

			if (isPostRevenue)
			{
				var transaction = TestObjectCreator.CreateARInvoice<ARInvoice>("Test001AR", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor1);
				var line = TestObjectCreator.CreateRevenueLine(charge, transaction.PK);
				Factory.Save();
			}

			SetLogger();

			var universalShipment = GetUniversalShipmentWithCharges(Factory, TestObjectCreator, GlbCompany.CurrentCompany, job, AddChargeForJobParentTest, branch: job.Branch, department: job.Department);

			AssertEquals("Pre-condition", defaultTaxId, charge.JR_AT_CostGSTRate);
			AssertEquals("Pre-condition", defaultTaxId, charge.JR_AT_SellGSTRate);
			AssertEquals("Pre-condition", false, job.IsManuallyCreated);

			var adapter = new JobCostingAdapter();

			if (isSimulateServiceTaskContext)
			{
				var newFactory = Factory.CreateNewFactory();
				using (Env.SetTemporaryUserContext("CWService", Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				{
					adapter.ImportCharges(newFactory, Logger, universalShipment, shipment.PK, JobShipmentSchema.Constants.Prefix);
				}
				charge = newFactory.Load<Charge>(charge.PK);
			}
			else
			{
				adapter.ImportCharges(Factory, Logger, universalShipment, shipment.PK, JobShipmentSchema.Constants.Prefix);
			}

			if (isAssertCost)
			{
				AssertEquals("Charge Cost Tax Rate should be updated after importing", TestObjectCreator.FREECAPGST.PK, charge.JR_AT_CostGSTRate);
			}
			else
			{
				AssertEquals("Charge Sell Tax Rate should be updated after importing", TestObjectCreator.GST2.PK, charge.JR_AT_SellGSTRate);
			}

			void AddChargeForJobParentTest(BusinessObjectFactory factory, TestObjectCreator creator, Job jobInput, List<ChargeLine> chargeLineCollection)
			{
				var chargeLine = GetChargeLine(GlbBranch.CurrentBranch.GB_Code, creator.FRT.AC_Code, apInvoiceNum, InvoiceDate,
											   creator.FREECAPGST.AT_Code, InvoiceDate, 100m, 100m, creator.AUD.Code, 0m,  // Cost
											   creator.Creditor1.OH_Code, creator.Debtor1.OH_Code, creator.FISDepartment.GE_Code, "Charge Description 1", 1,
											   creator.GST2.AT_Code, "FIN", 100m, 100m, creator.AUD.Code, 20m,  // Revenue
											   "REA", "REA");
				chargeLineCollection.Add(chargeLine);

				chargeLine.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
				chargeLine.ImportMetaData.Instruction = InstructionType.UpdateAndInsertIfNotFound;

				var matchingCriteria = new MatchingCriteria();
				matchingCriteria.FieldName = "ChargeCode";
				matchingCriteria.Value = creator.FRT.AC_Code;
				chargeLine.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>() { matchingCriteria });
			}
		}

		public void TestImportCharge_JobDisposeService()
		{
			using (AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				JobCostingAdapter adapter = new JobCostingAdapter();
				BusinessObjectFactory factory = new BusinessObjectFactory();
				using (factory.AddDisposableService())
				{
					TestObjectCreator creator = new TestObjectCreator(factory);

					var shipment1 = creator.CreateShipment("S00001001", "NZAKL", "AUSYD");
					var shipment2 = creator.CreateShipment("S00001002", "NZAKL", "AUSYD");
					var shipment3 = creator.CreateShipment("S00001003", "NZAKL", "AUSYD");
					Job job = creator.CreateJob(shipment1, false);

					SetLogger();

					Shipment universalShipment1 = GetUniversalShipmentWithCharges(factory, creator, GlbCompany.CurrentCompany, job, AddChargesForInsert);
					Shipment universalShipment2 = GetUniversalShipmentWithCharges(factory, creator, GlbCompany.CurrentCompany, null, AddChargesForInsert);
					Shipment universalShipment3 = GetUniversalShipmentWithCharges(factory, creator, GlbCompany.CurrentCompany, null, AddChargesForInsert);

					adapter.ImportCharges(factory, Logger, universalShipment1, shipment1.PK, JobShipmentSchema.Constants.Prefix);
					var jobTrackerService = factory.ServiceContainer.GetService<JobTrackerService>();
					AssertNull("jobTrackerService should not in service container", jobTrackerService);

					adapter.ImportCharges(factory, Logger, universalShipment2, shipment2.PK, JobShipmentSchema.Constants.Prefix);
					jobTrackerService = factory.ServiceContainer.GetService<JobTrackerService>();
					AssertNotNull("jobTrackerService should in service container", jobTrackerService);
					AssertNotNull("jobTrackerService should contain 1 job", jobTrackerService.JobCount);

					adapter.ImportCharges(factory, Logger, universalShipment3, shipment3.PK, JobShipmentSchema.Constants.Prefix);
					jobTrackerService = factory.ServiceContainer.GetService<JobTrackerService>();
					AssertNotNull("jobTrackerService should in service container", jobTrackerService);
					AssertNotNull("jobTrackerService should contain 2 jobs", jobTrackerService.JobCount);

					factory.Save();

					AssertCharges(factory, universalShipment1, shipment1, null);
					AssertCharges(factory, universalShipment2, shipment2, null);
					AssertCharges(factory, universalShipment3, shipment3, null);
				}
			}
		}

		[DeveloperOnlyTest]
		// It is currently impossible to insert a charge without getting at least one warning
		public void TestImportCharges_Insert_WithNoWarningsOnCharges()
		{
			testImportCharges(AddChargesForInsert, null, null, expectNoWarningsOnCharges: true);
		}

		public void TestImportCharges_CodeMap()
		{
			testImportCharges(AddChargesForMapping, null, null);
		}

		public void TestImportCharges_Insert()
		{
			testImportCharges(AddChargesForInsert, null, null);
		}

		[TestDate(2019, 03, 24)]
		public void TestImportCharges_InsertWithPlaceOfSupply()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				testImportCharges(AddChargesForInsert, null, null);
			}
		}

		public void TestImportCharges_InsertWhileLocked()
		{
			testImportCharges(
				AddChargesForInsert,
				typeof(MessageProcessingBusinessFailureException),
				$"User CWSupport has created a Billing Job for S00001001 within {BrandingFactory.Instance.ProductName}, but hasn't saved it yet.\r\n" +
				"Please resubmit this message after the user has saved the Billing Job.",
				expectedExceptionCaption: "Failed to create Job.",
				lockShipmentBeforeImport: true
			);
		}

		public void TestImportCharges_WhenShipmentInGatewayConoslGetLocked()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany);
			var gatewayConsolJob = TestObjectCreator.CreateJob(gatewayConsol);
			gatewayConsolJob.JH_GE = TestObjectCreator.FEADepartment.PK;

			var shipment1 = TestObjectCreator.CreateShipment("S1111", gatewayConsol);
			TestObjectCreator.CreateJob(shipment1);
			var shipment2 = TestObjectCreator.CreateShipment("S2222", gatewayConsol);
			Factory.Save();

			var universalShipment = CreateUniversalShipmentWithSellJRJ(gatewayConsolJob);

			var originalIsUserInteractive = ZArchitecture.Environment.Globals.IsUserInteractive;
			using (new DisposableAction(() => ZArchitecture.Environment.Globals.IsUserInteractive = originalIsUserInteractive))
			using (new JobHeaderTestHelper(Factory).CreateJobInAnotherCW1(shipment2))
			{
				const string errorMsg = "User GS1 is in the process of creating the Job S2222. You cannot work on the job until he/she saves it or cancels the changes.";

				gatewayConsolJob.Charges.RemoveAll();
				ZArchitecture.Environment.Globals.IsUserInteractive = true;
				AssertLoadChildShipmentsAndAcquireMutexesWhereRequired(
					"No Silent Report for mutex locking issue when running CW1 GUI, which is not a service task.",
					universalShipment,
					gatewayConsolJob,
					expectedExceptionType: typeof(JobCreationException),
					expectedErrorMsg: errorMsg,
					expectHaveIssue: false
				);

				gatewayConsolJob.Charges.RemoveAll();
				ZArchitecture.Environment.Globals.IsUserInteractive = false;
				AssertLoadChildShipmentsAndAcquireMutexesWhereRequired(
					"Get MessageProcessingBusinessFailureException for mutex locking issue when running service task or web-service.",
					universalShipment,
					gatewayConsolJob,
					expectedExceptionType: typeof(MessageProcessingBusinessFailureException),
					expectedErrorMsg: errorMsg,
					expectHaveIssue: false
				);

				gatewayConsolJob.Charges.RemoveAll();
				ZArchitecture.Environment.Globals.IsUserInteractive = false;
				using (Env.Instance.TemporaryServiceTaskContext("UMI", true))
				{
					AssertLoadChildShipmentsAndAcquireMutexesWhereRequired(
						"Get MessageProcessingBusinessFailureException for mutex locking issue when running service task UMI.",
						universalShipment,
						gatewayConsolJob,
						expectedExceptionType: typeof(MessageProcessingBusinessFailureException),
						expectedErrorMsg: errorMsg,
						expectHaveIssue: false
					);
				}
			}
		}

		Shipment CreateUniversalShipmentWithSellJRJ(Job job)
		{
			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

			universalShipment.JobCosting = new JobCosting(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.JobCosting.Branch = new Branch();
			universalShipment.JobCosting.Branch.Code = GlbBranch.CurrentBranch.GB_Code;
			universalShipment.JobCosting.Department = new Department();
			universalShipment.JobCosting.Department.Code = TestObjectCreator.FEADepartment.GE_Code;

			var sellChargeJRJ = GetChargeLine(job.Branch.GB_Code, TestObjectCreator.FRT.AC_Code
					, costAPInvoiceNumber: null, costDueDate: null, "GST", costInvoiceDate: null, 100.00m, 100.00m, "AUD", 10.00m
					, creditor: null, debtor: GlbCompany.CurrentCompany.OrgProxy.OH_Code
					, departmentCode: TestObjectCreator.FEADepartment.GE_Code
					, "Charge Description 1", 1, "GST", "FIN", 100.00m, 100.00m, "AUD", 10.00m, "REA", "REA", true, false);

			sellChargeJRJ.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			sellChargeJRJ.ImportMetaData.Instruction = InstructionType.Insert;

			if (PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(GlbCompany.CurrentCompany))
			{
				sellChargeJRJ.CostPlaceOfSupply = new PlaceOfSupply();
				sellChargeJRJ.CostPlaceOfSupply.Location = new CodeDescriptionPair5Char { Code = "JH" };
				sellChargeJRJ.CostPlaceOfSupply.LocationType = new UniversalCodeDescriptionPair { Code = PlaceOfSupplyTypes.State.Code };
			}

			universalShipment.JobCosting.SetChargeLineCollection(() => new List<ChargeLine>());
			universalShipment.JobCosting.ChargeLineCollection.Add(sellChargeJRJ);

			return universalShipment;
		}

		void AssertLoadChildShipmentsAndAcquireMutexesWhereRequired(string comment, Shipment universalShipment, Job gatewayConsolJob, Type expectedExceptionType, string expectedErrorMsg, bool expectHaveIssue, bool isComplexError = false)
		{
			const string issueKey = "TryLoadOrCreateJobWithMutexAndTrackingCore_1";

			CombineAssertions("PreCondition", () =>
			{
				var exp = AssertExceptionThrown<Exception>(() => {
					new JobCostingAdapter().ImportCharges(Factory, Logger, universalShipment, gatewayConsolJob.Parent.PK, gatewayConsolJob.Parent.TablePrefix());
				});

				AssertType(expectedExceptionType, exp);
				AssertEquals(expectedErrorMsg, exp.Message);
			});

			AssertEquals(comment, expectHaveIssue, ErrorReporter.HasBeenReported(issueKey));

			ErrorReporter.Clear();
		}

		public void TestImportCharges_InsertNoJobDepartment()
		{
			testImportCharges(AddChargesForInsertEmptyJobDepartment, null, null);
		}

		public void TestImportCharges_Update()
		{
			testImportCharges(AddChargesForUpdate, null, null);
		}

		public void TestImportCharges_Update_WithExtraFiltering()
		{
			testImportCharges(AddChargesForUpdate_WithExtraFiltering, null, null);
		}

		public void TestImportCharges_UpdateAndInsertIfNotFound()
		{
			testImportCharges(AddChargesForUpdateAndInsertIfNotFound, null, null);
		}

		public void TestImportCharges_Delete()
		{
			testImportCharges(AddChargesForDelete, null, null, chargeCounts: new int[] { 0, 0, 0 });
		}

		public void TestImportCharges_Insert_WithWarnings()
		{
			testImportCharges(AddChargesForInsert_WithWarnings, null, null);
			Assert("Has No Errors", !Logger.HasErrors);
			Assert("Has Warnings", Logger.HasWarnings);
			var expectedWarning = "Warning - 'CostIsPosted', 'RevenueIsPosted' and 'ARInvoiceNumber' should not be populated for Import. Whilst importing Charge Line: Job Number=S00001001 Charge Code=FRT Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=100.00 Sell OS Amount=100.00";
			AssertContains(expectedWarning, Logger.Logs);
			expectedWarning = "Warning - 'CostIsPosted', 'RevenueIsPosted' and 'ARInvoiceNumber' should not be populated for Import. Whilst importing Charge Line: Job Number=S00001001 Charge Code=BAF Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=200.00 Sell OS Amount=250.00";
			AssertContains(expectedWarning, Logger.Logs);
		}

		public void TestImportCharges_Insert_ValidationError()
		{
			string exceptionMessage = @"Whilst importing Charge Line: Job Number=S00001001 Charge Code=FRT Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=100.00 Sell OS Amount=100.00
Warning - Description: Charge description was changed from default. This description will appear on AR Invoice without translation.
Error - Department: This department is not valid for the charge code specified on this Job.
Warning - Debtor: You have selected a Debtor that is neither your Local Client or your Overseas Agent.
Whilst invoicing any party is valid, this should be confirmed.

Whilst importing Charge Line: Job Number=S00001001 Charge Code=BAF Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=200.00 Sell OS Amount=250.00
Warning - Description: Charge description was changed from default. This description will appear on AR Invoice without translation.
Error - Department: This department is not valid for the charge code specified on this Job.
Warning - Debtor: You have selected a Debtor that is neither your Local Client or your Overseas Agent.
Whilst invoicing any party is valid, this should be confirmed.";
			testImportCharges(AddChargesForInsert_ValidationError, typeof(DataObjectReadFailureException), exceptionMessage);
		}

		[TestDate(2018, 10, 20)]
		public void TestImportCharges_EnableElectronicProcessingCharge_NoExchangeRateError()
		{
			SetupElectronicProcessingCharge(false);

			var exceptionMessage = "The Invoicing Job cannot be created due to missing CNY exchange rate required for the creation of the disbursement license fee transactions.";
			testImportCharges(AddChargesForUpdateAndInsertIfNotFound, typeof(DataObjectReadFailureException), exceptionMessage);
		}

		[TestDate(2018, 10, 20)]
		public void TestImportCharges_EnableElectronicProcessingCharge_HasExchangeRate()
		{
			SetupElectronicProcessingCharge(true);
			testImportCharges(AddChargesForUpdateAndInsertIfNotFound, null, null);
		}

		void SetupElectronicProcessingCharge(bool hasExchangeRate)
		{
			var chargeCurrencies = new ElectronicProcessingChargeCurrencyCollection();
			chargeCurrencies.Add(new ElectronicProcessingChargeCurrency { CurrencyPK = TestObjectCreator.CNY.PK, ValidFromDate = ZDateTime.Today.AddDays(-10) });
			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCurrency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, chargeCurrencies);

			var electronicProcessingChargeProviderMock = new Mock<IElectronicProcessingChargeProvider>();
				electronicProcessingChargeProviderMock
						.Setup(x => x.HasElectronicProcessingChargeCurrencyExchangeRate(It.IsAny<Job>()))
						.Returns(hasExchangeRate);
			ObjectFactory.Substitute(electronicProcessingChargeProviderMock.Object);
		}

		[TestDate(2018, 10, 20)]
		public void TestImportCharges_Insert_WithPostingInstruction()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2018);

			var records = Factory.Load<JobChargePostingQueue>(new ZQuery());
			AssertEquals("Precondition", 0, records.Length);

			Action<BusinessObjectFactory> assertion = (factory) =>
			{
				records = factory.Load<JobChargePostingQueue>(new ZQuery());
				AssertEquals("3 JobChargePostingQueue created", 3, records.Length);
				AssertContainsExactElementsInAnyOrder(new List<ZString>() {
						JobChargePostingQueueLookups.PostCost,
						JobChargePostingQueueLookups.PostCost,
						JobChargePostingQueueLookups.PostRevenue },
						records.Select(x => x.JPQ_PostingInstruction));
			};

			testImportCharges(AddChargesForInsert_WithPostingInstruction, null, null, additionalAssertion: assertion);
		}

		public void TestImportCharges_Update_NoMatchingCriteria()
		{
			string exceptionMessage = @"Whilst importing Charge Line: Job Number=S00001001 Charge Code=FRT Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=100.00 Sell OS Amount=100.00
No matching criteria specified.
Whilst importing Charge Line: Job Number=S00001001 Charge Code=BAF Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=200.00 Sell OS Amount=250.00
No matching criteria specified.";
			testImportCharges(AddChargesForUpdate_NoMatchingCriteria, typeof(DataObjectReadFailureException), exceptionMessage);
		}

		public void TestImportCharges_Update_MultipleCharges()
		{
			string exceptionMessage = @"Whilst importing Charge Line: Job Number=S00001001 Charge Code=FRT Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=100.00 Sell OS Amount=100.00
Multiple charges found when updating Charge Line.
Whilst importing Charge Line: Job Number=S00001001 Charge Code=BAF Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=200.00 Sell OS Amount=250.00
Multiple charges found when updating Charge Line.";
			testImportCharges(AddChargesForUpdate_MultipleCharges, typeof(DataObjectReadFailureException), exceptionMessage);
		}

		public void TestImportCharges_Update_ChargeNotFound()
		{
			string exceptionMessage = @"Whilst importing Charge Line: Job Number=S00001001 Charge Code=FRT Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=100.00 Sell OS Amount=100.00
Charge not found when updating Charge Line.
Whilst importing Charge Line: Job Number=S00001001 Charge Code=BAF Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=200.00 Sell OS Amount=250.00
Charge not found when updating Charge Line.
Whilst importing Charge Line: Job Number=S00001001 Charge Code=CAF Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=300.00 Sell OS Amount=350.00
Charge not found when updating Charge Line.
Whilst importing Charge Line: Job Number=S00001001 Charge Code=FSC Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=400.00 Sell OS Amount=450.00
Charge not found when updating Charge Line.
Whilst importing Charge Line: Job Number=S00001001 Charge Code=PSS Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=500.00 Sell OS Amount=550.00
Charge not found when updating Charge Line.";
			testImportCharges(AddChargesForUpdate_ChargeNotFound, typeof(DataObjectReadFailureException), exceptionMessage);
		}

		public void TestImportCharges_Update_UsingPrimaryKey()
		{
			Action<BusinessObjectFactory> assertion = (factory) =>
			{
				var charges = factory.Load<Charge>(new ZQuery());
				AssertNotNull("Charge 1 should have been updated", factory.Load<Charge>(new ZQuery(JobChargeSchema.JR_Desc, "Charge Description 1 has been updated")));
				AssertNotNull("Charge 2 should have been updated", factory.Load<Charge>(new ZQuery(JobChargeSchema.JR_Desc, "Charge Description 2 has been updated")));
			};

			testImportCharges(AddChargesForUpdate_MultipleCharges_UsingPrimaryKey, null, null, additionalAssertion: assertion);
		}

		public void TestImportCharges_Update_ValidationError()
		{
			string exceptionMessage = @"Whilst importing Charge Line: Job Number=S00001001 Charge Code=FRT Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=100.00 Sell OS Amount=100.00
Warning - Description: Charge description was changed from default. This description will appear on AR Invoice without translation.
Warning - Estimated Cost: The Amount will not be updated automatically because it has been previously saved and has currently a non-zero amount.
Warning - Estimated Revenue: The Amount will not be updated automatically because it has been previously saved and has currently a non-zero amount.
Error - Department: This department is not valid for the charge code specified on this Job.
Warning - Debtor: You have selected a Debtor that is neither your Local Client or your Overseas Agent.
Whilst invoicing any party is valid, this should be confirmed.

Whilst importing Charge Line: Job Number=S00001001 Charge Code=BAF Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=200.00 Sell OS Amount=250.00
Warning - Description: Charge description was changed from default. This description will appear on AR Invoice without translation.
Warning - Estimated Cost: The Amount will not be updated automatically because it has been previously saved and has currently a non-zero amount.
Warning - Estimated Revenue: The Amount will not be updated automatically because it has been previously saved and has currently a non-zero amount.
Error - Department: This department is not valid for the charge code specified on this Job.
Warning - Debtor: You have selected a Debtor that is neither your Local Client or your Overseas Agent.
Whilst invoicing any party is valid, this should be confirmed.";
			testImportCharges(AddChargesForUpdate_ValidationError, typeof(DataObjectReadFailureException), exceptionMessage);
		}

		public void TestImportCharges_NoMappingInvalidChargeCode_Update()
		{
			string errorMessage = "Unable to determine primary key from code: JR_AC, XXX";
			try
			{
				RunTestImportChargesWithOptionalMapping("ChargeCode", "XXX", InstructionType.Update);
			}
			catch (DataObjectReadFailureException)
			{
				AssertContains("Import Log should contain the following error message: " + errorMessage, errorMessage, Logger.Logs);
			}
		}

		public void TestImportCharges_NoMappingInvalidCreditor_Update()
		{
			string errorMessage = "Unable to determine primary key from code: JR_OH_CostAccount, INVALID";

			try
			{
				RunTestImportChargesWithOptionalMapping("Creditor", "INVALID", InstructionType.Update);
			}
			catch (DataObjectReadFailureException)
			{
				AssertContains("Import Log should contain the following error message: " + errorMessage, errorMessage, Logger.Logs);
			}
		}

		public void TestImportCharges_WithMappingOnChargeCode_UpdateAndInsert()
		{
			var addMapping = true;
			var mapMessage = "Information - Line 0: Mapped Charge Code code 'XXX' to 'FRT'.";

			RunTestImportChargesWithOptionalMapping("ChargeCode", "XXX", InstructionType.UpdateAndInsertIfNotFound, addMapping);
			AssertContains("Charge Code was not mapped correctly.", mapMessage, Logger.Logs);
		}

		public void TestImportCharges_WithMappingOnCreditor_UpdateAndInsert()
		{
			var addMapping = true;
			var mapMessage = "Information - Line 0: Mapped Organization code 'INVALID' to 'AALSHI'.";

			RunTestImportChargesWithOptionalMapping("Creditor", "INVALID", InstructionType.UpdateAndInsertIfNotFound, addMapping);
			AssertContains("Creditor was not mapped correctly.", mapMessage, Logger.Logs);
		}

		public void TestImportCharges_UpdateAndInsertIfNotFound_NoMatchingCriteria()
		{
			string exceptionMessage = @"Whilst importing Charge Line: Job Number=S00001001 Charge Code=FRT Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=100.00 Sell OS Amount=100.00
No matching criteria specified.
Whilst importing Charge Line: Job Number=S00001001 Charge Code=BAF Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=200.00 Sell OS Amount=250.00
No matching criteria specified.";
			testImportCharges(AddChargesForUpdateAndInsertIfNotFound_NoMatchingCriteria, typeof(DataObjectReadFailureException), exceptionMessage);
		}

		public void TestImportChargesWithSellReference()
		{
			// Create a mixture of charges with and without sell references
			// Attempt to update based on sell reference/s

			testImportCharges(AddChargesForUpdateBasedOnSellReference, null, null, chargeCounts: new int[] { 2, 2 });
		}

		public void TestImportChargesWithGovernmentChargeCode()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				testImportCharges(AddChargesForUpdateBasedOnGovernmentChargeCode, null, null, chargeCounts: new int[] { 2, 2 });
			}
		}

		public void TestImportCharges_UpdateAndInsertIfNotFound_MultipleCharges()
		{
			string exceptionMessage = @"Whilst importing Charge Line: Job Number=S00001001 Charge Code=FRT Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=100.00 Sell OS Amount=100.00
Multiple charges found when updating Charge Line.";
			testImportCharges(AddChargesForUpdateAndInsertIfNotFound_MultipleCharges, typeof(DataObjectReadFailureException), exceptionMessage);
		}

		public void TestImportCharges_UpdateAndInsertIfNotFound_ValidationError()
		{
			string exceptionMessage = @"Whilst importing Charge Line: Job Number=S00001001 Charge Code=FRT Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=100.00 Sell OS Amount=100.00
Warning - Description: Charge description was changed from default. This description will appear on AR Invoice without translation.
Warning - Estimated Cost: The Amount will not be updated automatically because it has been previously saved and has currently a non-zero amount.
Warning - Estimated Revenue: The Amount will not be updated automatically because it has been previously saved and has currently a non-zero amount.
Error - Department: This department is not valid for the charge code specified on this Job.
Warning - Debtor: You have selected a Debtor that is neither your Local Client or your Overseas Agent.
Whilst invoicing any party is valid, this should be confirmed.

Whilst importing Charge Line: Job Number=S00001001 Charge Code=BAF Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=200.00 Sell OS Amount=250.00
Warning - Description: Charge description was changed from default. This description will appear on AR Invoice without translation.
Error - Department: This department is not valid for the charge code specified on this Job.
Warning - Debtor: You have selected a Debtor that is neither your Local Client or your Overseas Agent.
Whilst invoicing any party is valid, this should be confirmed.";
			testImportCharges(AddChargesForUpdateAndInsertIfNotFound_ValidationError, typeof(DataObjectReadFailureException), exceptionMessage);
		}

		public void TestImportCharges_UpdateAndInsertIfNotFound_ExcludesJobRevenueJournal()
		{
			testImportCharges(AddChargesForUpdateAndInsertIfNotFound_ExcludeJobRevenueJournal, null, null, chargeCounts: new int[] { 1, 3 });
		}

		public void TestImportCharges_Delete_NoMatchingCriteria()
		{
			string exceptionMessage = @"Whilst importing Charge Line: Job Number=S00001001 Charge Code=FRT Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=100.00 Sell OS Amount=100.00
No matching criteria specified.
Whilst importing Charge Line: Job Number=S00001001 Charge Code=BAF Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=200.00 Sell OS Amount=250.00
No matching criteria specified.";
			testImportCharges(AddChargesForDelete_NoMatchingCriteria, typeof(DataObjectReadFailureException), exceptionMessage);
		}

		public void TestImportCharges_Delete_NoChargesFound()
		{
			string exceptionMessage = @"Whilst importing Charge Line: Job Number=S00001001 Charge Code=FRT Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=100.00 Sell OS Amount=100.00
No charges found when deleting Charge Line.
Whilst importing Charge Line: Job Number=S00001001 Charge Code=BAF Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=200.00 Sell OS Amount=250.00
No charges found when deleting Charge Line.
Whilst importing Charge Line: Job Number=S00001001 Charge Code=FSC Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=300.00 Sell OS Amount=350.00
No charges found when deleting Charge Line.";
			testImportCharges(AddChargesForDelete_NoChargesFound, typeof(DataObjectReadFailureException), exceptionMessage);
		}

		public void TestImportCharges_Delete_WhenActiveARCashAdvanceExists()
		{
			string exceptionMessage = @"Whilst importing Charge Line: Job Number=S00001001 Charge Code=BAF Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=200.00 Sell OS Amount=250.00
Unable to delete the BAF charge as it has an active AR Advance Payment. Please cancel the Advance Payment if you need to delete this charge.";
			testImportCharges(AddChargesForDelete_WithActiveARCashAdvanceExists, typeof(DataObjectReadFailureException), exceptionMessage);
		}

		public void TestImportCharges_InsertInvalidDebtorViaLocalCharges()
		{
			AssertEquals("PreConsidtion", false, AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.Value);

			testImportCharges(AddChargeLineWithInvalidDebtorViaJobLocalCharges, null, null, jobHeaderSetter: RemoveJobHeaderLocalCharges);
		}

		public void TestImportCharges_InsertInvalidDebtorViaLocalCharges_EnableWIPMustHaveDebtorCode()
		{
			AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			string exceptionMessage = @"Whilst importing Charge Line: Job Number=S00001001 Charge Code=FRT Creditor=AALSHI, Debtor= Cost OS Amount=100.00 Sell OS Amount=100.00
Warning - Description: Charge description was changed from default. This description will appear on AR Invoice without translation.
Error - Debtor: You must enter a debtor. Your system has been configured so that the 'debtor' is mandatory when entering an unposted sell of non zero value.

The registry setting that governs this rule is Accounting > Job Costing > WIP Must Have Debtor Code";

			testImportCharges(AddChargeLineWithInvalidDebtorViaJobLocalCharges, typeof(DataObjectReadFailureException), exceptionMessage, jobHeaderSetter: RemoveJobHeaderLocalCharges);
		}

		public void TestImportCharges_InsertEmptyDebtor()
		{
			AssertEquals("PreConsidtion", false, AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.Value);

			testImportCharges(AddChargeLineWithEmptyDebtor, null, null, jobHeaderSetter: RemoveJobHeaderLocalCharges);
		}

		public void TestImportCharges_InsertEmptyDebtor_WipMusthaveDebtor()
		{
			AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			string exceptionMessage = @"Whilst importing Charge Line: Job Number=S00001001 Charge Code=FRT Creditor=AALSHI, Debtor= Cost OS Amount=100.00 Sell OS Amount=100.00
Warning - Description: Charge description was changed from default. This description will appear on AR Invoice without translation.
Error - Debtor: You must enter a debtor. Your system has been configured so that the 'debtor' is mandatory when entering an unposted sell of non zero value.
The registry setting that governs this rule is Accounting > Job Costing > WIP Must Have Debtor Code";
			testImportCharges(AddChargeLineWithEmptyDebtor, typeof(DataObjectReadFailureException), exceptionMessage, jobHeaderSetter: RemoveJobHeaderLocalCharges);
		}

		public void TestImportCharges_InsertEmptyCreditor()
		{
			AssertEquals("PreConsidtion", false, AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.Value);

			string exceptionMessage = @"Whilst importing Charge Line: Job Number=S00001001 Charge Code=FRT Creditor=, Debtor=ABIGAS Cost OS Amount=100.00 Sell OS Amount=100.00
Error - Invoice Date: A valid Creditor must be entered before an Invoice date is entered
Error - Account Payable Invoice Number: A valid Creditor must be entered before an AP Invoice number
Warning - Description: Charge description was changed from default. This description will appear on AR Invoice without translation.
Warning - Debtor: You have selected a Debtor that is neither your Local Client or your Overseas Agent.
Whilst invoicing any party is valid, this should be confirmed.
Error - Payment Date: A valid Creditor must be entered before a Payment Date is entered";
			testImportCharges(AddChargeLineWithEmptyCreditor, typeof(DataObjectReadFailureException), exceptionMessage, jobHeaderSetter: RemoveJobHeaderLocalCharges);
		}

		public void TestImportCharges_InsertEmptyCreditor_AcrMusthaveCreditor()
		{
			AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			string exceptionMessage = @"Whilst importing Charge Line: Job Number=S00001001 Charge Code=FRT Creditor=, Debtor=ABIGAS Cost OS Amount=100.00 Sell OS Amount=100.00
Error - Invoice Date: A valid Creditor must be entered before an Invoice date is entered
Error - Account Payable Invoice Number: A valid Creditor must be entered before an AP Invoice number
Warning - Description: Charge description was changed from default. This description will appear on AR Invoice without translation.
Error - Creditor: You must enter a creditor. Your system has been configured so that the 'creditor' is mandatory when entering an unposted cost of non zero value.
The registry setting that governs this rule is Accounting > Job Costing > Accrual Must Have Creditor Code
Warning - Debtor: You have selected a Debtor that is neither your Local Client or your Overseas Agent.
Whilst invoicing any party is valid, this should be confirmed.
Error - Payment Date: A valid Creditor must be entered before a Payment Date is entered";
			testImportCharges(AddChargeLineWithEmptyCreditor, typeof(DataObjectReadFailureException), exceptionMessage, jobHeaderSetter: RemoveJobHeaderLocalCharges);
		}

		void RemoveJobHeaderLocalCharges(JobHeader job)
		{
			job.JH_OA_LocalChargesAddr = ZGuid.Empty;
		}

		void AddChargesForDelete_WithActiveARCashAdvanceExists(BusinessObjectFactory factory, TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			AddChargesForDelete(factory, creator, job, chargeLineCollection);
			var charge = job.Charges.OfType<Charge>().FirstOrDefault(x => x.ChargeCode.AC_Code == "BAF");
			var cah = creator.CreateCashAdvanceRequestHeader(job, creator.AALSHI, LedgerTypes.AccountsReceivable, 45m, 45m, "USD");
			var cal1 = creator.CreateCashAdvanceRequestLine(cah, 45m, 45m);
			charge.JR_CAL_ARLine = cal1.PK;
		}

		[ExpectNoExceptions]
		public void TestImportCharges_WithNoDebtor()
		{
			testImportCharges(AddChargesFor_WithNoDebtor, null, null);
		}

		public void TestImportCharges_TopLevelDataContextFallback()
		{
			JobCostingAdapter adapter = new JobCostingAdapter();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			TestObjectCreator creator = new TestObjectCreator(factory);
			var shipment = creator.CreateShipment("S00001001", "NZAKL", "AUSYD");
			var job = creator.CreateJob(shipment, false);
			Factory.Save();

			Shipment universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.JobCosting = new JobCosting(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.JobCosting.Branch = new Branch();
			universalShipment.JobCosting.Branch.Code = "SYD";
			universalShipment.JobCosting.Department = new Department();
			universalShipment.JobCosting.Department.Code = "FES";
			universalShipment.JobCosting.SetChargeLineCollection(() => new List<ChargeLine>());

			AssertNull("Precondition: Shipment does not have a data context", universalShipment.DataContext);
			Logger.TopLevelDataObject = universalShipment;
			AssertNull("Precondition: Logger.TopLevelDataContext is not set too, which should not ever happen", Logger.TopLevelDataContext);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Cannot import JobCosting element with Invalid Company Code being specified in the Shipment’s DataContext.",
				() => adapter.ImportCharges(factory, Logger, universalShipment, shipment.PK, JobShipmentSchema.Constants.Prefix));

			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

			Logger.ClearLogs();
			AssertEquals("Precondition: Top level data context has company code set", GlbCompany.CurrentCompany.GC_Code, Logger.TopLevelDataContext.CompanyCodeToImportInto);
			AssertNoExceptionThrown(() => adapter.ImportCharges(factory, Logger, universalShipment, shipment.PK, JobShipmentSchema.Constants.Prefix));
		}

		public void TestJobCostingFieldsOptional_Branch_LocalClient_BlankRegistry()
		{
			var shipment = SetUpUniversalShipment(TestObjectCreator.NonCurrentBranch, null, TestObjectCreator.LocalClient, true);
			AssertBranchAndDepartment(TestObjectCreator.NonCurrentBranch.GB_Code, "FES", shipment);
		}

		public void TestJobCostingFieldsOptional_Branch_LocalClient_DefaultRegistry()
		{
			var shipment = SetUpUniversalShipment(TestObjectCreator.NonCurrentBranch, null, TestObjectCreator.LocalClient, false);
			AssertBranchAndDepartment(TestObjectCreator.NonCurrentBranch.GB_Code, "FES", shipment);
		}

		public void TestJobCostingFieldsOptional_Branch_NoLocalClient_BlankRegistry()
		{
			var shipment = SetUpUniversalShipment(TestObjectCreator.NonCurrentBranch, null, null, true);
			AssertBranchAndDepartment(TestObjectCreator.NonCurrentBranch.GB_Code, "FES", shipment);
		}

		public void TestJobCostingFieldsOptional_Branch_NoLocalClient_DefaultRegistry()
		{
			var shipment = SetUpUniversalShipment(TestObjectCreator.NonCurrentBranch, null, null, false);
			AssertBranchAndDepartment(TestObjectCreator.NonCurrentBranch.GB_Code, "FES", shipment);
		}

		public void TestImportCharges_WithInvalidBranch()
		{
			using (Env.SetTemporaryUserContext("E", TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				TestObjectCreator.ABIGAS.OH_IsDebtor = false;
				TestObjectCreator.Factory.Save();
			}

			var chargeLine = new ChargeLine(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine.ImportMetaData.Instruction = InstructionType.Insert;
			chargeLine.ChargeCode = new ChargeCode();
			chargeLine.ChargeCode.Code = TestObjectCreator.CC1.AC_Code;
			chargeLine.ChargeCode.Description = TestObjectCreator.CC1.AC_Desc;

			chargeLine.Branch = new Branch();
			chargeLine.Branch.Code = TestObjectCreator.NonCurrentCompanyBranch.GB_Code;
			chargeLine.Branch.Name = TestObjectCreator.NonCurrentCompanyBranch.GB_BranchName;

			chargeLine.Department = new Department();
			chargeLine.Department.Code = GlbDepartment.CurrentDepartment.GE_Code;
			chargeLine.Department.Name = GlbDepartment.CurrentDepartment.GE_Desc;

			chargeLine.Debtor = new OrganizationReference();
			chargeLine.Debtor.Key = TestObjectCreator.ABIGAS.OH_Code;
			chargeLine.Debtor.Type = nameof(DataContextType.Organization);

			chargeLine.SellInvoiceType = "FIN";
			chargeLine.SellOSAmount = 10M;

			try
			{
				SetUpUniversalShipment(GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment, null, false, chargeLine);
				Fail("Expecting exception but none was thrown.");
			}
			catch (DataObjectReadFailureException ex)
			{
				AssertContains("Exception", "charge branch is invalid.", ex.Message);
			}
			finally
			{
				ReleaseMutexesOnJobs();
			}
		}

		public void TestJobCostingFieldsOptional_NoBranch_LocalClient_BlankRegistry()
		{
			try
			{
				SetUpUniversalShipment(null, null, TestObjectCreator.LocalClient, true);
				Fail("Expecting exception but none was thrown.");
			}
			catch (DataObjectReadFailureException ex)
			{
				AssertBranchException(ex);
			}
			finally
			{
				ReleaseMutexesOnJobs();
			}
		}

		public void TestJobCostingFieldsOptional_NoBranch_LocalClient_DefaultRegistry()
		{
			var shipment = SetUpUniversalShipment(null, null, TestObjectCreator.LocalClient, false);
			AssertBranchAndDepartment("SYD", "FES", shipment);
		}

		public void TestJobCostingFieldsOptional_NoBranch_NoLocalClient_BlankRegistry()
		{
			try
			{
				SetUpUniversalShipment(null, null, null, true);
				Fail("Expecting exception but none was thrown.");
			}
			catch (DataObjectReadFailureException ex)
			{
				AssertBranchException(ex);
			}
			finally
			{
				ReleaseMutexesOnJobs();
			}
		}

		public void TestJobCostingFieldsOptional_NoBranch_NoLocalClient_DefaultRegistry()
		{
			var shipment = SetUpUniversalShipment(null, null, null, false);
			AssertBranchAndDepartment("SYD", "FES", shipment);
		}

		#region Test Import Charges When Job Is Closed

		public void TestImportCharges_Insert_JobIsClosed()
		{
			testImportCharges(AddChargesForInsert, null, null, chargeCounts: new int[] { 0, 0 }, isJobClosedScenario: true);
			Assert("Has No Errors", !Logger.HasErrors);
			Assert("Has Warnings", Logger.HasWarnings);
			var expectedWarning = "Warning - The Job Charges cannot be inserted/updated as the Job S00001001 is closed.";
			AssertContains(expectedWarning, Logger.Logs);
		}

		public void TestImportCharges_ChangeCloesdJob_ShowWarning()
		{
			testImportCharges(AddChargesForInsert, null, null, chargeCounts: new int[] { 0, 0 }, isJobClosedScenario: true, createBranch: false);
			Assert("Has No Errors", !Logger.HasErrors);
			Assert("Has Warnings", Logger.HasWarnings);
			var expectedWarning = "Warning - The Job Department cannot be updated as the Job S00001001 is closed.";
			AssertContains(expectedWarning, Logger.Logs);

			Logger.ClearLogs();
			testImportCharges(AddChargesForInsert, null, null, chargeCounts: new int[] { 0, 0 }, isJobClosedScenario: true, createDepartment: false, shipmentNum: "S00001002");
			Assert("Has No Errors", !Logger.HasErrors);
			Assert("Has Warnings", Logger.HasWarnings);
			expectedWarning = "Warning - The Job Branch cannot be updated as the Job S00001002 is closed.";
			AssertContains(expectedWarning, Logger.Logs);

			Logger.ClearLogs();
			testImportCharges(AddChargesForInsert, null, null, chargeCounts: new int[] { 0, 0 }, isJobClosedScenario: true, shipmentNum: "S00001003");
			Assert("Has No Errors", !Logger.HasErrors);
			Assert("Has Warnings", Logger.HasWarnings);
			expectedWarning = "Warning - The Job Branch cannot be updated as the Job S00001003 is closed.";
			AssertContains(expectedWarning, Logger.Logs);
			expectedWarning = "Warning - The Job Department cannot be updated as the Job S00001003 is closed.";
			AssertContains(expectedWarning, Logger.Logs);
		}

		public void TestImportCharges_Update_JobIsClosed()
		{
			testImportCharges(AddChargesForUpdate, null, null, isJobClosedScenario: true);
			Assert("Has No Errors", !Logger.HasErrors);
			Assert("Has Warnings", Logger.HasWarnings);
			var expectedWarning = "Warning - The Job Charges cannot be inserted/updated as the Job S00001001 is closed.";
			AssertContains(expectedWarning, Logger.Logs);
		}

		public void TestImportCharges_Delete_JobIsClosed()
		{
			testImportCharges(AddChargesForDelete, null, null, isJobClosedScenario: true);
			Assert("Has No Errors", !Logger.HasErrors);
			Assert("Has Warnings", Logger.HasWarnings);
			var expectedWarning = "Warning - The Job Charges cannot be inserted/updated as the Job S00001001 is closed.";
			AssertContains(expectedWarning, Logger.Logs);
		}

		public void TestImportCharges_UpdateAndInsertIfNotFound_JobIsClosed()
		{
			testImportCharges(AddChargesForUpdateAndInsertIfNotFound, null, null, chargeCounts: new int[] { 1, 0 }, isJobClosedScenario: true);
			Assert("Has No Errors", !Logger.HasErrors);
			Assert("Has Warnings", Logger.HasWarnings);
			var expectedWarning = "Warning - The Job Charges cannot be inserted/updated as the Job S00001001 is closed.";
			AssertContains(expectedWarning, Logger.Logs);
		}

		#region Test Import Charges When Job Is Ready For Financial Closure

		public void TestImportCharges_Insert_WhenJobIsReadyForFinancialClosure()
		{
			var exceptionMessage = AccountingConstants.JobIsReadyForFinancialClosureWithoutModifySecurityErrorMessage;
			testImportCharges(PrepareForInsert_WhenJobIsReadyForFinancialClosure, typeof(DataObjectReadFailureException), exceptionMessage);
			Assert("Has Errors", Logger.HasErrors);
			Assert("Has No Warnings", !Logger.HasWarnings);
			AssertContains("Error - " + exceptionMessage, Logger.Logs);
		}

		public void TestImportCharges_Update_WhenJobIsReadyForFinancialClosure()
		{
			var exceptionMessage = AccountingConstants.JobIsReadyForFinancialClosureWithoutModifySecurityErrorMessage;
			testImportCharges(PrepareForUpdate_WhenJobIsReadyForFinancialClosure, typeof(DataObjectReadFailureException), exceptionMessage);
			Assert("Has Errors", Logger.HasErrors);
			Assert("Has No Warnings", !Logger.HasWarnings);
			AssertContains("Error - " + exceptionMessage, Logger.Logs);
		}

		public void TestImportCharges_Update_HasActiveARCashAdvanceWithDifferentCurrency()
		{
			var exceptionMessage = @"Whilst importing Charge Line: Job Number=S00001001 Charge Code=BAF Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=200.00 Sell OS Amount=250.00
Warning - Description: Charge description was changed from default. This description will appear on AR Invoice without translation.
Warning - Estimated Cost: The Amount will not be updated automatically because it has been previously saved and has currently a non-zero amount.
Warning - Estimated Revenue: The Amount will not be updated automatically because it has been previously saved and has currently a non-zero amount.
Warning - Debtor: You have selected a Debtor that is neither your Local Client or your Overseas Agent.
Whilst invoicing any party is valid, this should be confirmed.
Error - Sell Currency: This charge has an active AR Advance Payment with Currency USD. Please change the currency to match the Advance Payment, or cancel the Advance Payment to continue.";
			testImportCharges(PrepareForUpdate_WhenActiveARCashAdvanceExists, typeof(DataObjectReadFailureException), exceptionMessage);
		}

		void PrepareForUpdate_WhenActiveARCashAdvanceExists(BusinessObjectFactory factory, TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			AddChargesForUpdate(factory, creator, job, chargeLineCollection);
			var charge = job.Charges.OfType<Charge>().FirstOrDefault(x => x.ChargeCode.AC_Code == "BAF");
			var cah = creator.CreateCashAdvanceRequestHeader(job, creator.AALSHI, LedgerTypes.AccountsReceivable, 45m, 45m, "USD");
			var cal1 = creator.CreateCashAdvanceRequestLine(cah, 45m, 45m);
			charge.JR_CAL_ARLine = cal1.PK;
		}

		public void TestImportCharges_Delete_WhenJobIsReadyForFinancialClosure()
		{
			var exceptionMessage = AccountingConstants.JobIsReadyForFinancialClosureWithoutModifySecurityErrorMessage;
			testImportCharges(PrepareForDelete_WhenJobIsReadyForFinancialClosure, typeof(DataObjectReadFailureException), exceptionMessage);
			Assert("Has Errors", Logger.HasErrors);
			Assert("Has No Warnings", !Logger.HasWarnings);
			AssertContains("Error - " + exceptionMessage, Logger.Logs);
		}

		public void TestImportCharges_UpdateAndInsertIfNotFound_WhenJobIsReadyForFinancialClosure()
		{
			var exceptionMessage = AccountingConstants.JobIsReadyForFinancialClosureWithoutModifySecurityErrorMessage;
			testImportCharges(PrepareForUpdateAndInsertIfNotFound_WhenJobIsReadyForFinancialClosure, typeof(DataObjectReadFailureException), exceptionMessage);
			Assert("Has Errors", Logger.HasErrors);
			Assert("Has No Warnings", !Logger.HasWarnings);
			AssertContains("Error - " + exceptionMessage, Logger.Logs);
		}

		void PrepareForDelete_WhenJobIsReadyForFinancialClosure(BusinessObjectFactory factory, TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			AddChargesForDelete(factory, creator, job, chargeLineCollection);
			PrepareJobIsReadyForFinancialClosure(factory);
		}

		void PrepareForUpdate_WhenJobIsReadyForFinancialClosure(BusinessObjectFactory factory, TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			AddChargesForUpdate(factory, creator, job, chargeLineCollection);
			PrepareJobIsReadyForFinancialClosure(factory);
		}

		void PrepareForInsert_WhenJobIsReadyForFinancialClosure(BusinessObjectFactory factory, TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			AddChargesForInsert(factory, creator, job, chargeLineCollection);
			PrepareJobIsReadyForFinancialClosure(factory);
		}

		void PrepareForUpdateAndInsertIfNotFound_WhenJobIsReadyForFinancialClosure(BusinessObjectFactory factory, TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			AddChargesForUpdateAndInsertIfNotFound(factory, creator, job, chargeLineCollection);
			PrepareJobIsReadyForFinancialClosure(factory);
		}

		void PrepareJobIsReadyForFinancialClosure(BusinessObjectFactory factory)
		{
			var shipment = factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, "S00001001"));
			shipment.Job.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;

			Assert("Precondition", shipment.Job.IsReadyForFinancialClosure);
		}

		#endregion

		#endregion

		void ReleaseMutexesOnJobs()
		{
			ZQuery query = new ZQuery();
			query.FetchOnlyFromLocalCache = true;
			var jobs = UniversalObjectFactory.Load<Job>(query);
			foreach (Job job in jobs)
			{
				job.Dispose();
			}
		}

		void AssertBranchAndDepartment(string expectedBranchCode, string expectedDepartmentCode, ForwardingShipment shipment)
		{
			var job = (Job)shipment.Job;
			var charge = job.Charges[0];
			AssertEquals("Job branch should be equal to Universal Shipment branch.", expectedBranchCode, job.Branch.GB_Code);
			AssertEquals("Charge branch.", expectedBranchCode, charge.Branch.GB_Code);
			AssertEquals("Charge department.", expectedDepartmentCode, charge.Department.GE_Code);
		}

		void AssertBranchException(DataObjectReadFailureException ex)
		{
			AssertContains("Exception", "Job Costing Branch could not be defaulted.", ex.Message);
		}

		ForwardingShipment SetUpUniversalShipment(GlbBranch branch, GlbDepartment department, OrgHeader localClient, bool useBlankRegistry, ChargeLine chargeLine = null)
		{
			if (useBlankRegistry)
			{
				JobBranchDefaultOrderRule rule = new JobBranchDefaultOrderRule();
				rule.DefaultToBlank = 1;
				rule.DefaultToBranchOfOrganisation = 0;
				rule.DefaultToBranchRelatedToPortOrWarehouseBranch = 0;
				rule.DefaultToLoginUserDefault = 0;
				AccountingConfigurationRegistry.Instance.JobBranchDefaultOrderRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rule);
			}

			var shipmentDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			shipmentDataObject.DataContext.CodesMappedToTarget = true; // Required to import JobCosting
			Logger.TopLevelDataObject = shipmentDataObject;

			shipmentDataObject.TransportMode = new UniversalCodeDescriptionPair() { Code = "SEA", Description = "Sea Freight" };
			shipmentDataObject.PortOfOrigin = new UNLOCO() { Code = "AUSYD", Name = "Sydney" };
			shipmentDataObject.PortOfDestination = new UNLOCO() { Code = "USCHI", Name = "Chicago" };

			if (localClient != null)
			{
				shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
				OrganizationAddress orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
				orgAddress.AddressType = "LocalClient";
				orgAddress.OrganizationCode = localClient.OH_Code;
				shipmentDataObject.OrganizationAddressCollection.Add(orgAddress);
			}

			shipmentDataObject.JobCosting = new JobCosting(DefaultDataObjectWriterStrategy.TestInstance);

			if (branch != null)
			{
				shipmentDataObject.JobCosting.Branch = new Branch();
				shipmentDataObject.JobCosting.Branch.Code = branch.GB_Code;
			}

			if (department != null)
			{
				shipmentDataObject.JobCosting.Department = new Department();
				shipmentDataObject.JobCosting.Department.Code = department.GE_Code;
			}

			shipmentDataObject.JobCosting.SetChargeLineCollection(() => new List<ChargeLine>());
			if (chargeLine == null)
			{
				chargeLine = new ChargeLine(DefaultDataObjectWriterStrategy.TestInstance);
				chargeLine.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
				chargeLine.ImportMetaData.Instruction = InstructionType.Insert;
				chargeLine.ChargeCode = new ChargeCode();
				chargeLine.ChargeCode.Code = TestObjectCreator.CC1.AC_Code;
				chargeLine.ChargeCode.Description = TestObjectCreator.CC1.AC_Desc;
			}
			shipmentDataObject.JobCosting.ChargeLineCollection.Add(chargeLine);

			using (UniversalObjectFactory.BOFactory.AddDisposableService())
			{
				var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, UniversalObjectFactory, null);
				var shipmentBO = reader.ReadIntoBusinessObject();
				UniversalObjectFactory.SaveAtEndOfImport(Logger);
				return shipmentBO;
			}
		}

		void testImportCharges(AddChargesDelegate addChargesDelegate, Type expectedExceptionType, string expectedExceptionMessage, string expectedExceptionCaption = null, int[] chargeCounts = null, bool lockShipmentBeforeImport = false, bool isJobClosedScenario = false, Action<BusinessObjectFactory> additionalAssertion = null, bool expectNoWarningsOnCharges = false, Action<JobHeader> jobHeaderSetter = null, bool createBranch = true, bool createDepartment = true, string shipmentNum = "S00001001")
		{
			using (AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var adapter = new JobCostingAdapter();
				var factory = new BusinessObjectFactory();
				using (factory.AddDisposableService())
				{
					var creator = new TestObjectCreator(factory);

					var shipment = creator.CreateShipment(shipmentNum, "NZAKL", "AUSYD");
					Job job = null;
					if (addChargesDelegate != AddChargesForInsert || isJobClosedScenario)
					{
						job = creator.CreateJob(shipment, false);
						jobHeaderSetter?.Invoke(job);
					}

					SetLogger();
					var universalShipment = GetUniversalShipmentWithCharges(factory, creator, GlbCompany.CurrentCompany, job, addChargesDelegate, createBranch: createBranch, createDepartment: createDepartment);

					using (var mutex = JobHeader.GetMutex_ForTestOnly(shipment.PK))
					{
						if (lockShipmentBeforeImport)
						{
							mutex.Lock();
						}
						if (isJobClosedScenario)
						{
							job.JH_Status = JobHeaderStatus.Closed.Code;
						}
						try
						{
							var mock = new Mock<IServiceTaskNudger> { CallBase = true };
							mock.Setup(m => m.NudgeServiceTask(It.IsAny<string>(), It.IsAny<TimeSpan?>()));
							using (ObjectFactory.Substitute(mock.Object))
							{
								adapter.ImportCharges(factory, Logger, universalShipment, shipment.PK, JobShipmentSchema.Constants.Prefix);
								factory.Save();
								if (expectNoWarningsOnCharges)
								{
									AssertEquals("No charges should have warnings", false, Logger.HasWarnings);
								}
							}

							if (expectedExceptionType != null || expectedExceptionMessage != null)
							{
								Fail(string.Format("Expecting exception but none was thrown.\r\nExpected Exception Message: {0}", expectedExceptionMessage));
							}

							AssertCharges(factory, universalShipment, shipment, chargeCounts, isJobClosedScenario);
							if (additionalAssertion != null)
							{
								additionalAssertion(factory);
							}
						}
						catch (Exception ex)
						{
							if (expectedExceptionType == null || !expectedExceptionType.IsInstanceOfType(ex))
							{
								throw;
							}

							var messageProcessingBusinessFailureException = ex as MessageProcessingBusinessFailureException;
							string exceptionCaption = messageProcessingBusinessFailureException != null ? messageProcessingBusinessFailureException.Caption : null;

							AssertNotNull(string.Format("expectedExceptionMessage should not be null.\r\nThrown Exception Message: {0}", ex.Message), expectedExceptionMessage);
							AssertMultilineASCIIEquals("Exception message", expectedExceptionMessage, ex.Message);
							AssertEquals("Exception caption", expectedExceptionCaption, exceptionCaption);
						}
					}
				}
			}
		}

		void AssertCharges(BusinessObjectFactory factory, Shipment universalShipment, ForwardingShipment shipment, int[] chargeCounts = null, bool isJobClosedScenario = false)
		{
			Job.Loader loader = new Job.Loader(factory, shipment);
			Job job = loader.Load(false, GlbCompany.CurrentCompany);
			AssertNotNull("job should not be null", job);
			int i = 0;

			foreach (ChargeLine chargeLine in universalShipment.JobCosting.ChargeLineCollection)
			{
				ZQuery query = new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
				ZString chargeCode = chargeLine.ChargeCode != null ? (ZString)chargeLine.ChargeCode.Code.Value :
					(ZString)getMatchingCriteriaValue(chargeLine, ChargeLineElementType.ChargeCode);
				query.AddToFilter(AccChargeCodeSchema.AC_Code, chargeCode);
				AccChargeCode chargeCode1 = factory.LoadTop1<AccChargeCode>(query);
				query = new ZQuery(JobChargeSchema.JR_AC, chargeCode1.PK);
				query.AddToFilter(JobChargeSchema.JR_JH, job.PK);
				Charge[] charges = factory.Load<Charge>(query);

				int length = chargeCounts == null ? 1 : chargeCounts[i];
				AssertEquals("charges.Length", length, charges.Length);

				if (length > 0)
				{
					Charge charge = null;
					if (length > 1 && chargeLine.DisplaySequence.HasValue)
					{
						charge = charges.FirstOrDefault(x => x.JR_DisplaySequence == chargeLine.DisplaySequence.Value);
						AssertNotNull("Must be a charge with matching Display Sequence", charge);
					}
					else
					{
						charge = charges[0];
					}

					if (isJobClosedScenario)
					{
						AssertChargeWhenJobIsClosed(charge, chargeLine);
					}
					else
					{
						AssertCharge(charge, chargeLine);
					}
				}
				i++;
			}
		}

		string getMatchingCriteriaValue(ChargeLine chargeLine, ChargeLineElementType chargeLineElementType)
		{
			string matchingCriteriaValue = string.Empty;
			foreach (MatchingCriteria matchingCriteria in chargeLine.ImportMetaData.MatchingCriteriaCollection)
			{
				ChargeLineElementType key = (ChargeLineElementType)Enum.Parse(typeof(ChargeLineElementType), matchingCriteria.FieldName, true);
				if (key == chargeLineElementType)
				{
					matchingCriteriaValue = matchingCriteria.Value;
					break;
				}
			}
			return matchingCriteriaValue;
		}

		delegate void AddChargesDelegate(BusinessObjectFactory factory, TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection);

		Shipment GetUniversalShipmentWithCharges(BusinessObjectFactory factory, TestObjectCreator creator, GlbCompany company, Job job, AddChargesDelegate addChargesDelegate, GlbBranch branch = null, bool createBranch = true, GlbDepartment department = null, bool createDepartment = true)
		{
			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New(); // Set DataContext but does not SetCompanyAndDataProviderDetails as it should be used from the TopLevelDataContext
			universalShipment.JobCosting = new JobCosting(DefaultDataObjectWriterStrategy.TestInstance);

			if (createBranch)
			{
				universalShipment.JobCosting.Branch = new Branch();
				universalShipment.JobCosting.Branch.Code = branch?.GB_Code ?? "SYD";
			}

			if (createDepartment)
			{
				universalShipment.JobCosting.Department = new Department();
				universalShipment.JobCosting.Department.Code = department?.GE_Code ?? "FES";
			}

			universalShipment.JobCosting.SetChargeLineCollection(() => new List<ChargeLine>());

			addChargesDelegate(factory, creator, job, universalShipment.JobCosting.ChargeLineCollection);
			factory.Save();

			return universalShipment;
		}

		void AddChargesForInsert(BusinessObjectFactory factory, TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			ChargeLine chargeLine1 = GetChargeLine("SYD", "FRT", "001", InvoiceDate, "GST", InvoiceDate, 100.00m, 100.00m, "AUD", 10.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 1, "GST", "FIN", 100.00m, 100.00m, "AUD", 10.00m, "REA", "REA", true, false);
			chargeLine1.SellReference = "ABC123";
			chargeLineCollection.Add(chargeLine1);
			ChargeLine chargeLine2 = GetChargeLine("SYD", "BAF", "001", InvoiceDate, "GST", InvoiceDate, 200.00m, 200.00m, "AUD", 20.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 2, "GST", "FIN", 250.00m, 250.00m, "AUD", 25.00m, "REA", "REA", false, true);
			chargeLine2.SellReference = "XYZ987";
			chargeLineCollection.Add(chargeLine2);

			chargeLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine1.ImportMetaData.Instruction = InstructionType.Insert;
			chargeLine2.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine2.ImportMetaData.Instruction = InstructionType.Insert;

			if (PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(GlbCompany.CurrentCompany))
			{
				chargeLine1.CostPlaceOfSupply = new PlaceOfSupply();
				chargeLine1.CostPlaceOfSupply.Location = new CodeDescriptionPair5Char { Code = "JH" };
				chargeLine1.CostPlaceOfSupply.LocationType = new UniversalCodeDescriptionPair { Code = PlaceOfSupplyTypes.State.Code };

				chargeLine2.CostAPInvoiceNumber = "98745";
				chargeLine2.CostPlaceOfSupply = new PlaceOfSupply();
				chargeLine2.CostPlaceOfSupply.Location = new CodeDescriptionPair5Char { Code = PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry };
				chargeLine2.CostPlaceOfSupply.LocationType = new UniversalCodeDescriptionPair { Code = PlaceOfSupplyTypes.PredefinedRule.Code };
			}
		}

		void AddChargesForInsert_WithPostingInstruction(BusinessObjectFactory factory, TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			AddChargesForInsert(factory, creator, job, chargeLineCollection);
			chargeLineCollection[0].ImportMetaData.PostingInstruction = PostingInstruction.PostCost;
			chargeLineCollection[1].ImportMetaData.PostingInstruction = PostingInstruction.PostRevenueAndCost;
		}

		void AddChargesForInsert_WithWarnings(BusinessObjectFactory factory, TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			AddChargesForInsert(factory, creator, job, chargeLineCollection);
			chargeLineCollection[0].CostIsPosted = true;
			chargeLineCollection[0].SupplierReference = "SUPPLY123";
			chargeLineCollection[1].SellIsPosted = true;
			chargeLineCollection[1].SellPostedTransactionNumber = "0001000";
		}

		void AddChargesForInsertEmptyJobDepartment(BusinessObjectFactory factory, TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			ChargeLine chargeLine1 = GetChargeLine("SYD", "FRT", "001", InvoiceDate, "GST", InvoiceDate, 100.00m, 100.00m, "AUD", 10.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 1, "GST", "FIN", 100.00m, 100.00m, "AUD", 10.00m, "REA", "REA");
			chargeLineCollection.Add(chargeLine1);

			chargeLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine1.ImportMetaData.Instruction = InstructionType.Insert;

			job.JH_GE = ZGuid.Empty;
			AssertNull(job.Department);
		}

		void AddChargeLineWithInvalidDebtorViaJobLocalCharges(BusinessObjectFactory factory, TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			ChargeLine chargeLine1 = GetChargeLine("SYD", "FRT", "001", InvoiceDate, "GST", InvoiceDate, 100.00m, 100.00m, "AUD", 10.00m,
				creditor: "AALSHI", debtor: "",
				"FIS", "Charge Description 1", 1, "GST", "FIN", 100.00m, 100.00m, "AUD", 10.00m, "REA", "REA");
			chargeLineCollection.Add(chargeLine1);

			chargeLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine1.ImportMetaData.Instruction = InstructionType.Insert;

			AssertEquals("PreCondition", false, TestObjectCreator.Creditor1.CompanyData.OB_IsDebtor);
			job.JH_OA_LocalChargesAddr = TestObjectCreator.Creditor1.MainAddress.PK;
			TestObjectCreator.Factory.Save();
		}

		void AddChargeLineWithEmptyDebtor(BusinessObjectFactory factory, TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			ChargeLine chargeLine1 = GetChargeLine("SYD", "FRT", "001", InvoiceDate, "GST", InvoiceDate, 100.00m, 100.00m, "AUD", 10.00m,
				creditor: "AALSHI", debtor: "",
				"FIS", "Charge Description 1", 1, "GST", "FIN", 100.00m, 100.00m, "AUD", 10.00m, "REA", "REA");
			chargeLineCollection.Add(chargeLine1);

			chargeLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine1.ImportMetaData.Instruction = InstructionType.Insert;

			AssertEquals("creditor and debetor wil be defaulted to localcharge, so it should be null too.", null, job.LocalCharges);
		}

		void AddChargeLineWithEmptyCreditor(BusinessObjectFactory factory, TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			ChargeLine chargeLine1 = GetChargeLine("SYD", "FRT", "001", InvoiceDate, "GST", InvoiceDate, 100.00m, 100.00m, "AUD", 10.00m,
				creditor: "", debtor: "ABIGAS",
				"FIS", "Charge Description 1", 1, "GST", "FIN", 100.00m, 100.00m, "AUD", 10.00m, "REA", "REA");
			chargeLineCollection.Add(chargeLine1);

			chargeLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine1.ImportMetaData.Instruction = InstructionType.Insert;

			AssertEquals("creditor and debetor wil be defaulted to localcharge, so it should be null too.", null, job.LocalCharges);
		}

		void AddChargesForInsert_ValidationError(BusinessObjectFactory factory, TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			ChargeLine chargeLine1 = GetChargeLine("SYD", "FRT", "001", InvoiceDate, "GST", InvoiceDate, 100.00m, 100.00m, "AUD", 10.00m,
				"AALSHI", "ABIGAS", "BRN", "Charge Description 1", 1, "GST", "FIN", 100.00m, 100.00m, "AUD", 10.00m, "REA", "REA");
			chargeLineCollection.Add(chargeLine1);
			ChargeLine chargeLine2 = GetChargeLine("SYD", "BAF", "001", InvoiceDate, "GST", InvoiceDate, 200.00m, 200.00m, "AUD", 20.00m,
				"AALSHI", "ABIGAS", "BRN", "Charge Description 1", 1, "GST", "FIN", 250.00m, 250.00m, "AUD", 25.00m, "REA", "REA");
			chargeLineCollection.Add(chargeLine2);

			chargeLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine1.ImportMetaData.Instruction = InstructionType.Insert;
			chargeLine2.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine2.ImportMetaData.Instruction = InstructionType.Insert;
		}

		void AddChargesForMapping(BusinessObjectFactory factory, TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			NotificationBuffer notifications = new NotificationBuffer();
			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, notifications);
			var orgMapping = Factory.Load<OrgHeader>(importContext.Converter.MappingOrgPK);

			RefCurrency aUDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			RefCurrency sGDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "SGD");

			OrgPatternMatchOverride chargeOverride = orgMapping.CreatePatternMatchOverrideForTest();
			chargeOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ChargeCodes;
			chargeOverride.OO_LocalCode = creator.LoadAccChargeCode("DTHC", GlbCompany.CurrentCompany.PK).AC_Code;
			chargeOverride.OO_ForeignCode = "THC";

			chargeOverride = orgMapping.CreatePatternMatchOverrideForTest();
			chargeOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Currency;
			chargeOverride.OO_LocalGuid = aUDCurrency.PK;
			chargeOverride.OO_ForeignCode = "AUS";

			chargeOverride = orgMapping.CreatePatternMatchOverrideForTest();
			chargeOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Currency;
			chargeOverride.OO_LocalGuid = sGDCurrency.PK;
			chargeOverride.OO_ForeignCode = "SG";

			Factory.Save();

			ChargeLine chargeLine1 = GetChargeLine("SYD", "DTHC", "001", InvoiceDate, "GST", InvoiceDate, 100.00m, 100.00m, "AUD", 10.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 1, "GST", "FIN", 0m, 0m, "SGD", 10.00m, "REA", "REA");
			chargeLineCollection.Add(chargeLine1);

			chargeLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine1.ImportMetaData.Instruction = InstructionType.UpdateAndInsertIfNotFound;
			chargeLine1.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());

			MatchingCriteria matchingCriteria1 = new MatchingCriteria();
			matchingCriteria1.FieldName = "ChargeCode";
			matchingCriteria1.Value = "THC";
			chargeLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);

			matchingCriteria1 = new MatchingCriteria();
			matchingCriteria1.FieldName = "CostOSCurrency";
			matchingCriteria1.Value = "AUS";
			chargeLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);

			matchingCriteria1 = new MatchingCriteria();
			matchingCriteria1.FieldName = "SellOSCurrency";
			matchingCriteria1.Value = "SG";
			chargeLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);
		}

		void AddChargesForUpdate(BusinessObjectFactory factory, TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			var charge = creator.CreateCharge(job, creator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK), "Charge Desc 1", creator.USD, 50.00m, creator.AALSHI, "002", creator.USD, 45.00m, creator.ABIGAS);
			charge.JR_CostReference = "REF123";
			charge.JR_IsARCashAdvance = false;
			charge.JR_IsAPCashAdvance = false;
			charge = creator.CreateCharge(job, creator.LoadAccChargeCode("BAF", GlbCompany.CurrentCompany.PK), "Charge Desc 2", creator.USD, 50.00m, creator.AALSHI, "002", creator.USD, 45.00m, creator.ABIGAS);
			charge.JR_CostReference = "REF234";
			charge.JR_IsARCashAdvance = true;
			charge.JR_IsAPCashAdvance = true;

			ChargeLine chargeLine1 = GetChargeLine("SYD", "FRT", "001", InvoiceDate, "GST", InvoiceDate, 100.00m, 100.00m, "AUD", 10.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 1, "GST", "FIN", 100.00m, 100.00m, "AUD", 10.00m, "REA", "REA", true, true);
			chargeLineCollection.Add(chargeLine1);
			ChargeLine chargeLine2 = GetChargeLine("SYD", "BAF", "001", InvoiceDate, "GST", InvoiceDate, 200.00m, 200.00m, "AUD", 20.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 2, "GST", "FIN", 250.00m, 250.00m, "AUD", 25.00m, "REA", "REA", false, false);
			chargeLineCollection.Add(chargeLine2);

			chargeLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine1.ImportMetaData.Instruction = InstructionType.Update;
			chargeLine1.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria1 = new MatchingCriteria();
			matchingCriteria1.FieldName = "ChargeCode";
			matchingCriteria1.Value = "FRT";
			chargeLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);
			matchingCriteria1 = new MatchingCriteria();
			matchingCriteria1.FieldName = "CostIsPosted";
			matchingCriteria1.Value = "N";
			chargeLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);
			matchingCriteria1 = new MatchingCriteria();
			matchingCriteria1.FieldName = "SellIsPosted";
			matchingCriteria1.Value = "N";
			chargeLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);
			matchingCriteria1 = new MatchingCriteria();
			matchingCriteria1.FieldName = "SupplierReference";
			matchingCriteria1.Value = "REF123";
			chargeLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);

			chargeLine2.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine2.ImportMetaData.Instruction = InstructionType.Update;
			chargeLine2.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria2 = new MatchingCriteria();
			matchingCriteria2.FieldName = "ChargeCode";
			matchingCriteria2.Value = "BAF";
			chargeLine2.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria2);
			matchingCriteria2 = new MatchingCriteria();
			matchingCriteria2.FieldName = "CostIsPosted";
			matchingCriteria2.Value = "N";
			chargeLine2.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria2);
			matchingCriteria2 = new MatchingCriteria();
			matchingCriteria2.FieldName = "SellIsPosted";
			matchingCriteria2.Value = "N";
			chargeLine2.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria2);
			matchingCriteria2 = new MatchingCriteria();
			matchingCriteria2.FieldName = "SupplierReference";
			matchingCriteria2.Value = "REF234";
			chargeLine2.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria2);
		}

		void AddChargesForUpdate_WithExtraFiltering(BusinessObjectFactory factory, TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			ARInvoice aRInvoice = creator.CreateARInvoice<ARInvoice>("123654" + job.JH_JobNum, GlbCompany.CurrentCompany.LocalCurrency, 1, creator.AALSHI);
			TransactionLine rev1 = aRInvoice.Lines.AddNew();
			SetLine(job, rev1, TransactionLineTypes.Revenue, 10m, creator.CC1);
			JobCharge rev1JobCharge = creator.CreateJobCharge(rev1, job, rev1.ChargeCode, rev1.TransactionCurrency);
			rev1JobCharge.JR_Desc = creator.CC1.AC_Desc;
			rev1JobCharge.JR_OH_SellAccount = creator.ABIGAS.PK;
			rev1JobCharge.JR_DisplaySequence = 1;

			TransactionLine rev2 = aRInvoice.Lines.AddNew();
			SetLine(job, rev2, TransactionLineTypes.Revenue, 20m, creator.CC2);
			JobCharge rev2JobCharge = creator.CreateJobCharge(rev2, job, rev2.ChargeCode, rev2.TransactionCurrency);
			rev2JobCharge.JR_Desc = creator.CC2.AC_Desc;
			rev2JobCharge.JR_OH_SellAccount = creator.ABIGAS.PK;
			rev2JobCharge.JR_DisplaySequence = 2;

			ChargeLine chargeLine1 = GetChargeLine("BNE", "ZZCC1", "001", InvoiceDate, "GST", InvoiceDate.AddDays(-1), 8m, 8m, "AUD", 0.8m,
				"AALSHI", null, null, null, null, null, null, null, null, null, null, "REA", "REA");
			chargeLineCollection.Add(chargeLine1);
			ChargeLine chargeLine2 = GetChargeLine("BNE", "ZZCC2", "001", InvoiceDate, "GST", InvoiceDate.AddDays(-1), 15m, 15m, "AUD", 1.5m,
				"AALSHI", null, null, null, null, null, null, null, null, null, null, "REA", "REA");
			chargeLineCollection.Add(chargeLine2);

			chargeLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine1.ImportMetaData.Instruction = InstructionType.Update;
			chargeLine1.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria1 = new MatchingCriteria();
			matchingCriteria1.FieldName = "ChargeCode";
			matchingCriteria1.Value = creator.CC1.AC_Code;
			chargeLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);
			matchingCriteria1 = new MatchingCriteria();
			matchingCriteria1.FieldName = "CostIsPosted";
			matchingCriteria1.Value = "N";
			chargeLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);
			matchingCriteria1 = new MatchingCriteria();
			matchingCriteria1.FieldName = "SellIsPosted";
			matchingCriteria1.Value = "Y";
			chargeLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);
			matchingCriteria1 = new MatchingCriteria();
			matchingCriteria1.FieldName = "SellPostedTransactionNumber";
			matchingCriteria1.Value = "00001000";
			chargeLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);

			chargeLine2.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine2.ImportMetaData.Instruction = InstructionType.Update;
			chargeLine2.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria2 = new MatchingCriteria();
			matchingCriteria2.FieldName = "ChargeCode";
			matchingCriteria2.Value = creator.CC2.AC_Code;
			chargeLine2.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria2);
			matchingCriteria2 = new MatchingCriteria();
			matchingCriteria2.FieldName = "CostIsPosted";
			matchingCriteria2.Value = "N";
			chargeLine2.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria2);
			matchingCriteria2 = new MatchingCriteria();
			matchingCriteria2.FieldName = "SellIsPosted";
			matchingCriteria2.Value = "Y";
			chargeLine2.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria2);
			matchingCriteria2 = new MatchingCriteria();
			matchingCriteria2.FieldName = "SellPostedTransactionNumber";
			matchingCriteria2.Value = "00001000";
			chargeLine2.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria2);
		}

		void AddChargesForUpdate_NoMatchingCriteria(BusinessObjectFactory factory, TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			creator.CreateCharge(job, creator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK), "Charge Desc 1", creator.USD, 50.00m, creator.AALSHI, "002", creator.USD, 45.00m, creator.ABIGAS);
			creator.CreateCharge(job, creator.LoadAccChargeCode("BAF", GlbCompany.CurrentCompany.PK), "Charge Desc 2", creator.USD, 50.00m, creator.AALSHI, "002", creator.USD, 45.00m, creator.ABIGAS);

			ChargeLine chargeLine1 = GetChargeLine("SYD", "FRT", "001", InvoiceDate, "GST", InvoiceDate, 100.00m, 100.00m, "AUD", 10.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 1, "GST", "FIN", 100.00m, 100.00m, "AUD", 10.00m, "REA", "REA");
			chargeLineCollection.Add(chargeLine1);
			ChargeLine chargeLine2 = GetChargeLine("SYD", "BAF", "001", InvoiceDate, "GST", InvoiceDate, 200.00m, 200.00m, "AUD", 20.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 1, "GST", "FIN", 250.00m, 250.00m, "AUD", 25.00m, "REA", "REA");
			chargeLineCollection.Add(chargeLine2);

			chargeLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine1.ImportMetaData.Instruction = InstructionType.Update;

			chargeLine2.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine2.ImportMetaData.Instruction = InstructionType.Update;
		}

		void AddChargesForUpdate_MultipleCharges(BusinessObjectFactory factory, TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			creator.CreateCharge(job, creator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK), "Charge Desc 1", creator.USD, 50.00m, creator.AALSHI, "002", creator.USD, 45.00m, creator.ABIGAS);
			creator.CreateCharge(job, creator.LoadAccChargeCode("BAF", GlbCompany.CurrentCompany.PK), "Charge Desc 2", creator.USD, 50.00m, creator.AALSHI, "002", creator.USD, 45.00m, creator.ABIGAS);
			creator.CreateCharge(job, creator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK), "Charge Desc 1", creator.USD, 50.00m, creator.AALSHI, "002", creator.USD, 45.00m, creator.ABIGAS);
			creator.CreateCharge(job, creator.LoadAccChargeCode("BAF", GlbCompany.CurrentCompany.PK), "Charge Desc 2", creator.USD, 50.00m, creator.AALSHI, "002", creator.USD, 45.00m, creator.ABIGAS);

			ChargeLine chargeLine1 = GetChargeLine("SYD", "FRT", "001", InvoiceDate, "GST", InvoiceDate, 100.00m, 100.00m, "AUD", 10.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 1, "GST", "FIN", 100.00m, 100.00m, "AUD", 10.00m, "REA", "REA");
			chargeLineCollection.Add(chargeLine1);
			ChargeLine chargeLine2 = GetChargeLine("SYD", "BAF", "001", InvoiceDate, "GST", InvoiceDate, 200.00m, 200.00m, "AUD", 20.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 1, "GST", "FIN", 250.00m, 250.00m, "AUD", 25.00m, "REA", "REA");
			chargeLineCollection.Add(chargeLine2);

			chargeLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine1.ImportMetaData.Instruction = InstructionType.Update;
			chargeLine1.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria1 = new MatchingCriteria();
			matchingCriteria1.FieldName = "ChargeCode";
			matchingCriteria1.Value = "FRT";
			chargeLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);

			chargeLine2.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine2.ImportMetaData.Instruction = InstructionType.Update;
			chargeLine2.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria2 = new MatchingCriteria();
			matchingCriteria2.FieldName = "ChargeCode";
			matchingCriteria2.Value = "BAF";
			chargeLine2.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria2);
		}

		void AddChargesForUpdate_MultipleCharges_UsingPrimaryKey(BusinessObjectFactory factory, TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			var chargeCode1 = creator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK);
			var chargeBizObj1 = creator.CreateCharge(job, chargeCode1, "Charge Desc 1 original description", creator.USD, 50.00m, creator.AALSHI, "002", creator.USD, 45.00m, creator.ABIGAS);
			var chargeCode2 = creator.LoadAccChargeCode("BAF", GlbCompany.CurrentCompany.PK);
			var chargeBizObj2 = creator.CreateCharge(job, chargeCode2, "Charge Desc 2 original description", creator.USD, 50.00m, creator.AALSHI, "002", creator.USD, 45.00m, creator.ABIGAS);

			var importMetaData1 = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Instruction = InstructionType.Update,
			};

			importMetaData1.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>
			{
				new MatchingCriteria() { FieldName = "PrimaryKey", Value = chargeBizObj1.PK.ToString() }
			});

			var chargeLine1 = GetChargeLine(GlbBranch.CurrentBranch.GB_Code, "FRT", "001", InvoiceDate, "GST", InvoiceDate, 100.00m, 100.00m, "AUD", 10.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 1 has been updated", 1, "GST", "FIN", 100.00m, 100.00m, "AUD", 10.00m, "REA", "REA");
			chargeLine1.ImportMetaData = importMetaData1;
			chargeLineCollection.Add(chargeLine1);

			var importMetaData2 = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Instruction = InstructionType.Update,
			};

			importMetaData2.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>
			{
				new MatchingCriteria() { FieldName = "ChargeCode", Value = "FRT" },
				new MatchingCriteria() { FieldName = "PrimaryKey", Value = chargeBizObj2.PK.ToString() }
			});

			// Will update chargeBizObj2 by PK even though there is a (mis-)matching criteria ChargeCode = "FRT", and even if PrimaryKey is not the first criteria
			var chargeLine2 = GetChargeLine("SYD", "BAF", "001", InvoiceDate, "GST", InvoiceDate, 200.00m, 200.00m, "AUD", 20.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 2 has been updated", 1, "GST", "FIN", 250.00m, 250.00m, "AUD", 25.00m, "REA", "REA");
			chargeLine2.ImportMetaData = importMetaData2;
			chargeLineCollection.Add(chargeLine2);
		}

		void AddChargesForUpdate_ChargeNotFound(BusinessObjectFactory factory, TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			ChargeLine chargeLine1 = GetChargeLine("SYD", "FRT", "001", InvoiceDate, "GST", InvoiceDate, 100.00m, 100.00m, "AUD", 10.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 1, "GST", "FIN", 100.00m, 100.00m, "AUD", 10.00m, "REA", "REA");
			chargeLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Instruction = InstructionType.Update,
			};

			chargeLine1.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>
			{
				new MatchingCriteria()
				{
					FieldName = "ChargeCode",
					Value = "FRT"
				}
			});

			chargeLineCollection.Add(chargeLine1);

			ChargeLine chargeLine2 = GetChargeLine("SYD", "BAF", "001", InvoiceDate, "GST", InvoiceDate, 200.00m, 200.00m, "AUD", 20.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 2", 2, "GST", "FIN", 250.00m, 250.00m, "AUD", 25.00m, "REA", "REA");
			chargeLine2.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Instruction = InstructionType.Update,
			};

			chargeLine2.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>
			{
				new MatchingCriteria()
				{
					FieldName = "ChargeCode",
					Value = "BAF"
				}
			});

			chargeLineCollection.Add(chargeLine2);

			ChargeLine chargeLine3 = GetChargeLine("SYD", "CAF", "001", InvoiceDate, "GST", InvoiceDate, 300.00m, 300.00m, "AUD", 30.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 3", 2, "GST", "FIN", 350.00m, 350.00m, "AUD", 35.00m, "REA", "REA");
			chargeLine3.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Instruction = InstructionType.Update,
			};

			chargeLine3.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>
			{
				new MatchingCriteria()
				{
					FieldName = "PrimaryKey",
					Value = "00a3a65a-9aa7-403f-a3d1-46351b5aa5fe"
				}
			});

			chargeLineCollection.Add(chargeLine3);

			ChargeLine chargeLine4 = GetChargeLine("SYD", "FSC", "001", InvoiceDate, "GST", InvoiceDate, 400.00m, 400.00m, "AUD", 40.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 4", 2, "GST", "FIN", 450.00m, 450.00m, "AUD", 45.00m, "REA", "REA");
			chargeLine4.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Instruction = InstructionType.Update,
			};

			chargeLine4.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>
			{
				new MatchingCriteria()
				{
					FieldName = "PrimaryKey",
					Value = null
				}
			});

			chargeLineCollection.Add(chargeLine4);

			ChargeLine chargeLine5 = GetChargeLine("SYD", "PSS", "001", InvoiceDate, "GST", InvoiceDate, 500.00m, 500.00m, "AUD", 50.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 5", 2, "GST", "FIN", 550.00m, 550.00m, "AUD", 55.00m, "REA", "REA");
			chargeLine5.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Instruction = InstructionType.Update,
			};

			chargeLine5.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>
			{
				new MatchingCriteria()
				{
					FieldName = "PrimaryKey",
					Value = "xxxx"
				}
			});

			chargeLineCollection.Add(chargeLine5);
		}

		void AddChargesForUpdate_ValidationError(BusinessObjectFactory factory, TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			var charge = creator.CreateCharge(job, creator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK), "Charge Desc 1", creator.USD, 50.00m, creator.AALSHI, "002", creator.USD, 45.00m, creator.ABIGAS);
			charge.JR_GE = creator.FESDepartment.PK;
			charge = creator.CreateCharge(job, creator.LoadAccChargeCode("BAF", GlbCompany.CurrentCompany.PK), "Charge Desc 2", creator.USD, 50.00m, creator.AALSHI, "002", creator.USD, 45.00m, creator.ABIGAS);
			charge.JR_GE = creator.FESDepartment.PK;

			ChargeLine chargeLine1 = GetChargeLine("SYD", "FRT", "001", InvoiceDate, "GST", InvoiceDate, 100.00m, 100.00m, "AUD", 10.00m,
				"AALSHI", "ABIGAS", "BRN", "Charge Description 1", 1, "GST", "FIN", 100.00m, 100.00m, "AUD", 10.00m, "REA", "REA");
			chargeLineCollection.Add(chargeLine1);
			ChargeLine chargeLine2 = GetChargeLine("SYD", "BAF", "001", InvoiceDate, "GST", InvoiceDate, 200.00m, 200.00m, "AUD", 20.00m,
				"AALSHI", "ABIGAS", "BRN", "Charge Description 1", 2, "GST", "FIN", 250.00m, 250.00m, "AUD", 25.00m, "REA", "REA");
			chargeLineCollection.Add(chargeLine2);

			chargeLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine1.ImportMetaData.Instruction = InstructionType.Update;
			chargeLine1.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria1 = new MatchingCriteria();
			matchingCriteria1.FieldName = "ChargeCode";
			matchingCriteria1.Value = "FRT";
			chargeLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);

			chargeLine2.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine2.ImportMetaData.Instruction = InstructionType.Update;
			chargeLine2.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria2 = new MatchingCriteria();
			matchingCriteria2.FieldName = "ChargeCode";
			matchingCriteria2.Value = "BAF";
			chargeLine2.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria2);

			charge.JR_SellRatingOverride = false;
			charge.JR_CostRatingOverride = false;
		}

		void AddChargesForUpdateAndInsertIfNotFound(BusinessObjectFactory factory, TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			var charge = creator.CreateCharge(job, creator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK), "Charge Desc 1", creator.USD, 50.00m, creator.AALSHI, "002", creator.USD, 45.00m, creator.ABIGAS);
			charge.JR_IsARCashAdvance = false;
			charge.JR_IsAPCashAdvance = false;

			ChargeLine chargeLine1 = GetChargeLine("SYD", "FRT", "001", InvoiceDate, "GST", InvoiceDate, 100.00m, 100.00m, "AUD", 10.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 1, "GST", "FIN", 100.00m, 100.00m, "AUD", 10.00m, "REA", "REA", true, true);
			chargeLineCollection.Add(chargeLine1);
			ChargeLine chargeLine2 = GetChargeLine("SYD", "BAF", "001", InvoiceDate, "GST", InvoiceDate, 200.00m, 200.00m, "AUD", 20.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 2, "GST", "FIN", 250.00m, 250.00m, "AUD", 25.00m, "REA", "REA", false, false);
			chargeLineCollection.Add(chargeLine2);

			chargeLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine1.ImportMetaData.Instruction = InstructionType.UpdateAndInsertIfNotFound;
			chargeLine1.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria1 = new MatchingCriteria();
			matchingCriteria1.FieldName = "ChargeCode";
			matchingCriteria1.Value = "FRT";
			chargeLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);

			chargeLine2.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine2.ImportMetaData.Instruction = InstructionType.UpdateAndInsertIfNotFound;
			chargeLine2.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria2 = new MatchingCriteria();
			matchingCriteria2.FieldName = "ChargeCode";
			matchingCriteria2.Value = "BAF";
			chargeLine2.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria2);
		}

		void AddChargesForUpdateAndInsertIfNotFound_NoMatchingCriteria(BusinessObjectFactory factory, TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			creator.CreateCharge(job, creator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK), "Charge Desc 1", creator.USD, 50.00m, creator.AALSHI, "002", creator.USD, 45.00m, creator.ABIGAS);

			ChargeLine chargeLine1 = GetChargeLine("SYD", "FRT", "001", InvoiceDate, "GST", InvoiceDate, 100.00m, 100.00m, "AUD", 10.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 1, "GST", "FIN", 100.00m, 100.00m, "AUD", 10.00m, "REA", "REA");
			chargeLineCollection.Add(chargeLine1);
			ChargeLine chargeLine2 = GetChargeLine("SYD", "BAF", "001", InvoiceDate, "GST", InvoiceDate, 200.00m, 200.00m, "AUD", 20.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 2, "GST", "FIN", 250.00m, 250.00m, "AUD", 25.00m, "REA", "REA");
			chargeLineCollection.Add(chargeLine2);

			chargeLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine1.ImportMetaData.Instruction = InstructionType.UpdateAndInsertIfNotFound;

			chargeLine2.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine2.ImportMetaData.Instruction = InstructionType.UpdateAndInsertIfNotFound;
		}

		void AddChargesForUpdateAndInsertIfNotFound_MultipleCharges(BusinessObjectFactory factory, TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			creator.CreateCharge(job, creator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK), "Charge Desc 1", creator.USD, 50.00m, creator.AALSHI, "002", creator.USD, 45.00m, creator.ABIGAS);
			creator.CreateCharge(job, creator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK), "Charge Desc 1", creator.USD, 50.00m, creator.AALSHI, "002", creator.USD, 45.00m, creator.ABIGAS);

			ChargeLine chargeLine1 = GetChargeLine("SYD", "FRT", "001", InvoiceDate, "GST", InvoiceDate, 100.00m, 100.00m, "AUD", 10.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 1, "GST", "FIN", 100.00m, 100.00m, "AUD", 10.00m, "REA", "REA");
			chargeLineCollection.Add(chargeLine1);
			ChargeLine chargeLine2 = GetChargeLine("SYD", "BAF", "001", InvoiceDate, "GST", InvoiceDate, 200.00m, 200.00m, "AUD", 20.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 2, "GST", "FIN", 250.00m, 250.00m, "AUD", 25.00m, "REA", "REA");
			chargeLineCollection.Add(chargeLine2);

			chargeLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine1.ImportMetaData.Instruction = InstructionType.UpdateAndInsertIfNotFound;
			chargeLine1.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria1 = new MatchingCriteria();
			matchingCriteria1.FieldName = "ChargeCode";
			matchingCriteria1.Value = "FRT";
			chargeLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);

			chargeLine2.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine2.ImportMetaData.Instruction = InstructionType.UpdateAndInsertIfNotFound;
			chargeLine2.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria2 = new MatchingCriteria();
			matchingCriteria2.FieldName = "ChargeCode";
			matchingCriteria2.Value = "BAF";
			chargeLine2.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria2);
		}

		void AddChargesForUpdateBasedOnSellReference(BusinessObjectFactory factory, TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			var charge1 = creator.CreateCharge(job, creator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK), "Charge Desc 1", creator.USD, 50.00m, creator.AALSHI, "002", creator.USD, 45.00m, creator.ABIGAS);
			charge1.JR_SellReference = "ABC123";
			var charge2 = creator.CreateCharge(job, creator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK), "Charge Desc 1", creator.USD, 50.00m, creator.AALSHI, "002", creator.USD, 45.00m, creator.ABIGAS);
			charge2.JR_SellReference = "XYZ123";

			ChargeLine chargeLine1 = GetChargeLine("SYD", "FRT", "001", InvoiceDate, "GST", InvoiceDate, 100.00m, 100.00m, "AUD", 10.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 1, "GST", "FIN", 100.00m, 100.00m, "AUD", 10.00m, "REA", "REA");
			chargeLine1.SellReference = "ABC456";
			chargeLineCollection.Add(chargeLine1);
			ChargeLine chargeLine2 = GetChargeLine("SYD", "FRT", "001", InvoiceDate, "GST", InvoiceDate, 200.00m, 200.00m, "AUD", 20.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 2, "GST", "FIN", 250.00m, 250.00m, "AUD", 25.00m, "REA", "REA");
			chargeLine2.SellReference = "XYZ456";
			chargeLineCollection.Add(chargeLine2);

			chargeLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine1.ImportMetaData.Instruction = InstructionType.UpdateAndInsertIfNotFound;
			chargeLine1.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria1 = new MatchingCriteria();
			matchingCriteria1.FieldName = "SellReference";
			matchingCriteria1.Value = "ABC123"; //Update charge will sell reference ABC123 with sell reference ABC456
			chargeLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);

			chargeLine2.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine2.ImportMetaData.Instruction = InstructionType.UpdateAndInsertIfNotFound;
			chargeLine2.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria2 = new MatchingCriteria();
			matchingCriteria2.FieldName = "SellReference";
			matchingCriteria2.Value = "XYZ123"; //Update charge will sell reference XYZ123 with sell reference XYZ456
			chargeLine2.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria2);
		}

		void AddChargesForUpdateBasedOnGovernmentChargeCode(BusinessObjectFactory factory, TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			var charge1 = creator.CreateCharge(job, creator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK), "Charge Desc 1", creator.USD, 50.00m, creator.AALSHI, "002", creator.USD, 45.00m, creator.ABIGAS);
			charge1.JR_CostGovtChargeCode = "test a 1";
			charge1.JR_SellGovtChargeCode = "test b 1";
			var charge2 = creator.CreateCharge(job, creator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK), "Charge Desc 1", creator.USD, 50.00m, creator.AALSHI, "002", creator.USD, 45.00m, creator.ABIGAS);
			charge2.JR_CostGovtChargeCode = "test a 2";
			charge2.JR_SellGovtChargeCode = "test b 2";

			ChargeLine chargeLine1 = GetChargeLine("SYD", "FRT", "001", InvoiceDate, "GST", InvoiceDate, 100.00m, 100.00m, "AUD", 10.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 1, "GST", "FIN", 100.00m, 100.00m, "AUD", 10.00m, "REA", "REA");
			chargeLine1.GovernmentReportingCostChargeCode = "Cost Govt Chg Code 1";
			chargeLine1.GovernmentReportingSellChargeCode = "Sell Govt Chg Code 1";
			chargeLineCollection.Add(chargeLine1);
			ChargeLine chargeLine2 = GetChargeLine("SYD", "FRT", "001", InvoiceDate, "GST", InvoiceDate, 200.00m, 200.00m, "AUD", 20.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 2, "GST", "FIN", 250.00m, 250.00m, "AUD", 25.00m, "REA", "REA");
			chargeLine2.GovernmentReportingCostChargeCode = "Cost Govt Chg Code 2";
			chargeLine2.GovernmentReportingSellChargeCode = "Sell Govt Chg Code 2";
			chargeLineCollection.Add(chargeLine2);

			chargeLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine1.ImportMetaData.Instruction = InstructionType.UpdateAndInsertIfNotFound;
			chargeLine1.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria1 = new MatchingCriteria();
			matchingCriteria1.FieldName = "GovernmentReportingCostChargeCode";
			matchingCriteria1.Value = "test a 1";
			chargeLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);
			MatchingCriteria matchingCriteria2 = new MatchingCriteria();
			matchingCriteria2.FieldName = "GovernmentReportingSellChargeCode";
			matchingCriteria2.Value = "test b 1";
			chargeLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria2);

			chargeLine2.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine2.ImportMetaData.Instruction = InstructionType.UpdateAndInsertIfNotFound;
			chargeLine2.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria3 = new MatchingCriteria();
			matchingCriteria3.FieldName = "GovernmentReportingCostChargeCode";
			matchingCriteria3.Value = "test a 2";
			chargeLine2.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria3);
			MatchingCriteria matchingCriteria4 = new MatchingCriteria();
			matchingCriteria4.FieldName = "GovernmentReportingSellChargeCode";
			matchingCriteria4.Value = "test b 2";
			chargeLine2.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria4);
		}

		void RunTestImportChargesWithOptionalMapping(string matchingCriteriaField, string matchingCriteriaValue, InstructionType instruction, bool doMapping = false)
		{
			JobCostingAdapter adapter = new JobCostingAdapter();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			TestObjectCreator creator = new TestObjectCreator(factory);
			ForwardingConsol consol = creator.CreateConsol("AUSYD", "NZAKL", "C00001001");

			var shipment1 = creator.CreateShipment("S00001001", "AUSYD", "NZAKL", consol);
			var job1 = creator.CreateJob(shipment1, false);

			SetLogger();

			Shipment universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.JobCosting = new JobCosting(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.JobCosting.Branch = new Branch();
			universalShipment.JobCosting.Branch.Code = "SYD";
			universalShipment.JobCosting.Department = new Department();
			universalShipment.JobCosting.Department.Code = "FES";
			universalShipment.JobCosting.SetChargeLineCollection(() => new List<ChargeLine>());

			ChargeLine chargeLine1 = GetChargeLine("SYD", creator.FRT.AC_Code, "001", InvoiceDate, "GST", InvoiceDate, 100.00m, 100.00m, "AUD", 10.00m, "AALSHI", "ABIGAS", "FIS", "Charge Description 1", 1, "GST", "FIN", 100.00m, 100.00m, "AUD", 10.00m, "REA", "REA");
			universalShipment.JobCosting.ChargeLineCollection.Add(chargeLine1);
			chargeLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine1.ImportMetaData.Instruction = instruction;
			chargeLine1.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());

			MatchingCriteria matchingCriteria1 = new MatchingCriteria();
			matchingCriteria1.FieldName = matchingCriteriaField;
			matchingCriteria1.Value = matchingCriteriaValue;
			chargeLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);

			if (doMapping)
			{
				var notifications = new NotificationBuffer();
				var importContext = new ValueObjectImportContext(Factory, notifications);
				var orgMapping = Factory.Load<OrgHeader>(importContext.Converter.MappingOrgPK);

				OrgPatternMatchOverride chargeOverride = orgMapping.CreatePatternMatchOverrideForTest();
				chargeOverride.OO_Relationship = (matchingCriteriaField == "ChargeCode" ? Constants.OrgPatternMatchOverrideRelationships.ChargeCodes : Constants.OrgPatternMatchOverrideRelationships.Organisation);
				chargeOverride.OO_LocalCode = (matchingCriteriaField == "ChargeCode" ? creator.FRT.AC_Code : ZString.Empty);
				chargeOverride.OO_ForeignCode = matchingCriteriaValue;
				chargeOverride.OO_LocalGuid = (matchingCriteriaField == "Creditor" ? creator.AALSHI.PK : ZGuid.Empty);
			}
			Factory.Save();
			adapter.ImportCharges(factory, Logger, universalShipment, shipment1.PK, JobShipmentSchema.Constants.Prefix);
		}

		void AddChargesForUpdateAndInsertIfNotFound_ValidationError(BusinessObjectFactory factory, TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			var charge = creator.CreateCharge(job, creator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK), "Charge Desc 1", creator.USD, 50.00m, creator.AALSHI, "002", creator.USD, 45.00m, creator.ABIGAS);
			charge.JR_GE = creator.FESDepartment.PK;

			ChargeLine chargeLine1 = GetChargeLine("SYD", "FRT", "001", InvoiceDate, "GST", InvoiceDate, 100.00m, 100.00m, "AUD", 10.00m,
				"AALSHI", "ABIGAS", "BRN", "Charge Description 1", 1, "GST", "FIN", 100.00m, 100.00m, "AUD", 10.00m, "REA", "REA");
			chargeLineCollection.Add(chargeLine1);
			ChargeLine chargeLine2 = GetChargeLine("SYD", "BAF", "001", InvoiceDate, "GST", InvoiceDate, 200.00m, 200.00m, "AUD", 20.00m,
				"AALSHI", "ABIGAS", "BRN", "Charge Description 1", 1, "GST", "FIN", 250.00m, 250.00m, "AUD", 25.00m, "REA", "REA");
			chargeLineCollection.Add(chargeLine2);

			chargeLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine1.ImportMetaData.Instruction = InstructionType.UpdateAndInsertIfNotFound;
			chargeLine1.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria1 = new MatchingCriteria();
			matchingCriteria1.FieldName = "ChargeCode";
			matchingCriteria1.Value = "FRT";
			chargeLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);

			chargeLine2.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine2.ImportMetaData.Instruction = InstructionType.UpdateAndInsertIfNotFound;
			chargeLine2.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria2 = new MatchingCriteria();
			matchingCriteria2.FieldName = "ChargeCode";
			matchingCriteria2.Value = "BAF";
			chargeLine2.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria2);

			charge.JR_SellRatingOverride = false;
			charge.JR_CostRatingOverride = false;
		}

		void AddChargesForUpdateAndInsertIfNotFound_ExcludeJobRevenueJournal(BusinessObjectFactory factory, TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			var charge = creator.CreateCharge(job, creator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK), "Charge Desc 1", creator.USD, 50.00m, creator.AALSHI, "002", creator.USD, 45.00m, creator.ABIGAS);
			charge.JR_GE = creator.FESDepartment.PK;

			creator.CreateTestPeriodsForEntireYear(ZDateTime.Now.Year);
			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, creator.GLHeader1.PK.ToGuid());
			var jobRevenueJournal = factory.New<JobRevenueJournal>();

			var drLine = jobRevenueJournal.JournalLines.AddNew();
			drLine.DebitCreditSign = nameof(Business.DebitCredit.DR);
			drLine.AL_GE = creator.FISDepartment.PK;

			var crLine = jobRevenueJournal.JournalLines.AddNew();
			crLine.DebitCreditSign = nameof(Business.DebitCredit.CR);
			drLine.AL_GE = creator.FESDepartment.PK;

			drLine.AL_JH = crLine.AL_JH = job.PK;
			drLine.AL_AC = crLine.AL_AC = creator.CC1.PK;
			drLine.OSUnsignedLineAmount = crLine.OSUnsignedLineAmount = 120m;

			jobRevenueJournal.RunPreSaveValidation();
			AssertNoErrors(jobRevenueJournal);

			factory.Save();

			AssertNotNull(crLine.RelatedJobCharge);

			ChargeLine chargeLine1 = GetChargeLine("SYD", "FRT", "001", InvoiceDate, "GST", InvoiceDate, 100.00m, 100.00m, "AUD", 10.00m,
				"AALSHI", "ABIGAS", "FES", "Charge Description 1", 4, "GST", "FIN", 100.00m, 100.00m, "AUD", 10.00m, "REA", "REA");
			chargeLineCollection.Add(chargeLine1);
			ChargeLine chargeLine2 = GetChargeLine("SYD", creator.CC1.AC_Code, "001", InvoiceDate, "GST", InvoiceDate, 200.00m, 200.00m, "AUD", 20.00m,
				"AALSHI", "ABIGAS", "FES", "Charge Description 1", 5, "GST", "FIN", 250.00m, 250.00m, "AUD", 25.00m, "REA", "REA");
			chargeLineCollection.Add(chargeLine2);

			chargeLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine1.ImportMetaData.Instruction = InstructionType.UpdateAndInsertIfNotFound;
			chargeLine1.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria1 = new MatchingCriteria();
			matchingCriteria1.FieldName = "ChargeCode";
			matchingCriteria1.Value = "FRT";
			chargeLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);

			chargeLine2.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine2.ImportMetaData.Instruction = InstructionType.UpdateAndInsertIfNotFound;
			chargeLine2.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria2 = new MatchingCriteria();
			matchingCriteria2.FieldName = "ChargeCode";
			matchingCriteria2.Value = "BAF";
			chargeLine2.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria2);
		}

		void AddChargesForDelete(BusinessObjectFactory factory, TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			creator.CreateCharge(job, creator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK), "Charge Desc 1", creator.USD, 50.00m, creator.AALSHI, "002", creator.USD, 45.00m, creator.ABIGAS);
			creator.CreateCharge(job, creator.LoadAccChargeCode("BAF", GlbCompany.CurrentCompany.PK), "Charge Desc 2", creator.USD, 50.00m, creator.AALSHI, "002", creator.USD, 45.00m, creator.ABIGAS);
			var createdCharge3 = creator.CreateCharge(job, creator.LoadAccChargeCode("FSC", GlbCompany.CurrentCompany.PK), "Charge Desc 3", creator.USD, 100.00m, creator.AALSHI, "002", creator.USD, 90.00m, creator.ABIGAS);

			ChargeLine chargeLine1 = GetChargeLine("SYD", "FRT", "001", InvoiceDate, "GST", InvoiceDate, 100.00m, 100.00m, "AUD", 10.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 1, "GST", "FIN", 100.00m, 100.00m, "AUD", 10.00m, "REA", "REA");
			chargeLineCollection.Add(chargeLine1);
			ChargeLine chargeLine2 = GetChargeLine("SYD", "BAF", "001", InvoiceDate, "GST", InvoiceDate, 200.00m, 200.00m, "AUD", 20.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 2, "GST", "FIN", 250.00m, 250.00m, "AUD", 25.00m, "REA", "REA");
			chargeLineCollection.Add(chargeLine2);
			ChargeLine chargeLine3 = GetChargeLine("SYD", "FSC", "001", InvoiceDate, "GST", InvoiceDate, 300.00m, 300.00m, "AUD", 20.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 2, "GST", "FIN", 250.00m, 250.00m, "AUD", 25.00m, "REA", "REA");

			chargeLineCollection.Add(chargeLine3);

			chargeLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Instruction = InstructionType.Delete,
			};

			chargeLine1.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>
			{
				new MatchingCriteria()
				{
					FieldName = "ChargeCode",
					Value = "FRT"
				}
			});

			chargeLine2.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Instruction = InstructionType.Delete,
			};

			chargeLine2.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>
			{
				new MatchingCriteria()
				{
					FieldName = "ChargeCode",
					Value = "BAF"
				}
			});

			chargeLine3.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Instruction = InstructionType.Delete,
			};

			chargeLine3.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>
			{
				new MatchingCriteria()
				{
					FieldName = "PrimaryKey",
					Value = createdCharge3.PK.ToString()
				}
			});
		}

		void AddChargesForDelete_NoMatchingCriteria(BusinessObjectFactory factory, TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			creator.CreateCharge(job, creator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK), "Charge Desc 1", creator.USD, 50.00m, creator.AALSHI, "002", creator.USD, 45.00m, creator.ABIGAS);
			creator.CreateCharge(job, creator.LoadAccChargeCode("BAF", GlbCompany.CurrentCompany.PK), "Charge Desc 2", creator.USD, 50.00m, creator.AALSHI, "002", creator.USD, 45.00m, creator.ABIGAS);

			ChargeLine chargeLine1 = GetChargeLine("SYD", "FRT", "001", InvoiceDate, "GST", InvoiceDate, 100.00m, 100.00m, "AUD", 10.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 1, "GST", "FIN", 100.00m, 100.00m, "AUD", 10.00m, "REA", "REA");
			chargeLineCollection.Add(chargeLine1);
			ChargeLine chargeLine2 = GetChargeLine("SYD", "BAF", "001", InvoiceDate, "GST", InvoiceDate, 200.00m, 200.00m, "AUD", 20.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 1, "GST", "FIN", 250.00m, 250.00m, "AUD", 25.00m, "REA", "REA");
			chargeLineCollection.Add(chargeLine2);

			chargeLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine1.ImportMetaData.Instruction = InstructionType.Delete;

			chargeLine2.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine2.ImportMetaData.Instruction = InstructionType.Delete;
		}

		void AddChargesForDelete_NoChargesFound(BusinessObjectFactory factory, TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			ChargeLine chargeLine1 = GetChargeLine("SYD", "FRT", "001", InvoiceDate, "GST", InvoiceDate, 100.00m, 100.00m, "AUD", 10.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 1, "GST", "FIN", 100.00m, 100.00m, "AUD", 10.00m, "REA", "REA");
			chargeLineCollection.Add(chargeLine1);
			ChargeLine chargeLine2 = GetChargeLine("SYD", "BAF", "001", InvoiceDate, "GST", InvoiceDate, 200.00m, 200.00m, "AUD", 20.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 1, "GST", "FIN", 250.00m, 250.00m, "AUD", 25.00m, "REA", "REA");
			chargeLineCollection.Add(chargeLine2);
			ChargeLine chargeLine3 = GetChargeLine("SYD", "FSC", "001", InvoiceDate, "GST", InvoiceDate, 300.00m, 300.00m, "AUD", 30.00m,
				"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 1, "GST", "FIN", 350.00m, 350.00m, "AUD", 35.00m, "REA", "REA");
			chargeLineCollection.Add(chargeLine3);

			chargeLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine1.ImportMetaData.Instruction = InstructionType.Delete;
			chargeLine1.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria1 = new MatchingCriteria();
			matchingCriteria1.FieldName = "ChargeCode";
			matchingCriteria1.Value = "FRT";
			chargeLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);

			chargeLine2.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine2.ImportMetaData.Instruction = InstructionType.Delete;
			chargeLine2.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria2 = new MatchingCriteria();
			matchingCriteria2.FieldName = "ChargeCode";
			matchingCriteria2.Value = "BAF";
			chargeLine2.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria2);

			chargeLine3.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Instruction = InstructionType.Delete,
			};

			chargeLine3.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>
			{
				new MatchingCriteria()
				{
					FieldName = "PrimaryKey",
					Value = Guid.NewGuid().ToString()
				}
			});
		}

		void AddChargesFor_WithNoDebtor(BusinessObjectFactory factory, TestObjectCreator creator, Job job, List<ChargeLine> chargeLineCollection)
		{
			ChargeLine chargeLine1 = GetChargeLine("SYD", "FRT", "001", InvoiceDate, "GST", InvoiceDate, 100.00m, 100.00m, "AUD", 10.00m,
				"AALSHI", null, "FIS", "Charge Description 1", 1, "GST", "FIN", 100.00m, 100.00m, "AUD", 10.00m, "REA", "REA");
			chargeLineCollection.Add(chargeLine1);

			chargeLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine1.ImportMetaData.Instruction = InstructionType.Insert;
		}

		ChargeLine GetChargeLine(ZString? branchCode, ZString? chargeCode, ZString? costAPInvoiceNumber, ZDateTime? costDueDate, ZString? costGSTVATID, ZDateTime? costInvoiceDate,
			ZDecimal? costLocalAmount, ZDecimal? costOSAmount, ZString? costOSCurrency, ZDecimal? costOSGSTVATAmount, ZString? creditor, ZString? debtor, ZString? departmentCode,
			ZString? description, ZShort? displaySequence, ZString? sellGSTVATID, ZString? sellInvoiceType, ZDecimal? sellLocalAmount, ZDecimal? sellOSAmount,
			ZString? sellOSCurrency, ZDecimal? sellOSGSTVATAmount, ZString? costRatingBehaviour, ZString? sellRatingBehaviour, bool arCashAdvanceRequired = false, bool apCashAdvanceRequired = false)
		{
			ChargeLine chargeLine = new ChargeLine(DefaultDataObjectWriterStrategy.TestInstance);

			if (branchCode.HasValue)
			{
				chargeLine.Branch = new Branch();
				chargeLine.Branch.Code = branchCode;
				chargeLine.Branch.Name = "EDIHQ";
			}

			if (costRatingBehaviour.HasValue)
			{
				chargeLine.CostRatingBehaviour = new UniversalCodeDescriptionPair
				{
					Code = costRatingBehaviour,
					Description = JobChargeLookups.GetDescriptionForRatingBehaviourCode(costRatingBehaviour.Value)
				};
			}

			if (sellRatingBehaviour.HasValue)
			{
				chargeLine.SellRatingBehaviour = new UniversalCodeDescriptionPair
				{
					Code = sellRatingBehaviour,
					Description = JobChargeLookups.GetDescriptionForRatingBehaviourCode(sellRatingBehaviour.Value)
				};
			}

			if (chargeCode.HasValue)
			{
				chargeLine.ChargeCode = new ChargeCode();
				chargeLine.ChargeCode.Code = chargeCode;
				chargeLine.ChargeCode.Description = "International Freight";
			}

			chargeLine.CostAPInvoiceNumber = costAPInvoiceNumber;
			chargeLine.CostDueDate = costDueDate;

			if (costGSTVATID.HasValue)
			{
				chargeLine.CostGSTVATID = new TaxID();
				chargeLine.CostGSTVATID.TaxCode = costGSTVATID;
				chargeLine.CostGSTVATID.Description = "Cost Tax Description";
			}

			chargeLine.CostInvoiceDate = costInvoiceDate;
			chargeLine.CostLocalAmount = costLocalAmount;
			chargeLine.CostOSAmount = costOSAmount;

			if (costOSCurrency.HasValue)
			{
				chargeLine.CostOSCurrency = new Currency();
				chargeLine.CostOSCurrency.Code = costOSCurrency;
				chargeLine.CostOSCurrency.Description = "Cost Currency Description";
			}

			chargeLine.CostOSGSTVATAmount = costOSGSTVATAmount;

			if (creditor.HasValue)
			{
				chargeLine.Creditor = new OrganizationReference();
				chargeLine.Creditor.Key = creditor;
				chargeLine.Creditor.Type = nameof(DataContextType.Organization);
			}

			if (debtor.HasValue)
			{
				chargeLine.Debtor = new OrganizationReference();
				chargeLine.Debtor.Key = debtor;
				chargeLine.Debtor.Type = nameof(DataContextType.Organization);
			}

			if (departmentCode.HasValue)
			{
				chargeLine.Department = new Department();
				chargeLine.Department.Code = departmentCode;
				chargeLine.Department.Name = "Test Department";
			}

			chargeLine.Description = description;
			chargeLine.DisplaySequence = displaySequence;

			if (sellGSTVATID.HasValue)
			{
				chargeLine.SellGSTVATID = new TaxID();
				chargeLine.SellGSTVATID.TaxCode = sellGSTVATID;
				chargeLine.SellGSTVATID.Description = "Sell Tax Description";
			}

			chargeLine.SellInvoiceType = sellInvoiceType;
			chargeLine.SellOSAmount = sellOSAmount;

			if (sellOSCurrency.HasValue)
			{
				chargeLine.SellOSCurrency = new Currency();
				chargeLine.SellOSCurrency.Code = sellOSCurrency;
				chargeLine.SellOSCurrency.Description = "Sell Currency Description";
			}

			chargeLine.SellOSGSTVATAmount = sellOSGSTVATAmount;

			chargeLine.ARCashAdvanceRequired = arCashAdvanceRequired;
			chargeLine.APCashAdvanceRequired = apCashAdvanceRequired;

			return chargeLine;
		}

		void AssertCharge(Charge charge, ChargeLine chargeLine)
		{
			if (chargeLine.Branch != null && chargeLine.Branch.Code.HasValue)
			{
				AssertNotNull("charge.Branch should not be null", charge.Branch);
				AssertEquals("Branch should be equal", chargeLine.Branch.Code, charge.Branch.GB_Code);
			}

			if (chargeLine.ChargeCode != null && chargeLine.ChargeCode.Code.HasValue)
			{
				AssertNotNull("charge.ChargeCode should not be null", charge.ChargeCode);
				AssertEquals("ChargeCode should be equal", chargeLine.ChargeCode.Code, charge.ChargeCode.AC_Code);
			}

			if (chargeLine.CostAPInvoiceNumber.HasValue)
			{
				AssertEquals("APInvoiceNum should be equal", chargeLine.CostAPInvoiceNumber, charge.JR_APInvoiceNum);
			}

			if (chargeLine.CostDueDate.HasValue)
			{
				AssertEquals("CostDueDate should be equal", chargeLine.CostDueDate, charge.JR_PaymentDate);
			}

			if (chargeLine.CostGSTVATID != null && chargeLine.CostGSTVATID.TaxCode.HasValue && !charge.JR_AT_CostGSTRate.IsEmpty)
			{
				AssertNotNull("CostGSTRate should not be null", charge.CostGSTRate);
				AssertEquals("CostGSTRate should be equal", chargeLine.CostGSTVATID.TaxCode, charge.CostGSTRate.AT_Code);
			}

			if (chargeLine.CostInvoiceDate.HasValue)
			{
				AssertEquals("CostInvoiceDate should be equal", chargeLine.CostInvoiceDate, charge.JR_APInvoiceDate);
			}

			if (chargeLine.CostLocalAmount.HasValue)
			{
				AssertEquals("Cost Local Amount should be equal", chargeLine.CostLocalAmount, charge.JR_LocalCostAmt);
			}

			if (chargeLine.CostOSAmount.HasValue)
			{
				AssertEquals("Cost Local Amount should be equal", chargeLine.CostOSAmount, charge.JR_OSCostAmt);
			}

			if (chargeLine.CostOSCurrency != null && chargeLine.CostOSCurrency.Code.HasValue)
			{
				AssertEquals("Cost Currency should be equal", chargeLine.CostOSCurrency.Code, charge.JR_RX_NKCostCurrency);
			}

			if (chargeLine.CostOSGSTVATAmount.HasValue && !charge.JR_AT_CostGSTRate.IsEmpty)
			{
				AssertEquals("Cost OS GST VAT Amount should be equal", chargeLine.CostOSGSTVATAmount, charge.JR_OSCostGSTAmt_Calc);
			}

			if (chargeLine.Creditor != null && chargeLine.Creditor.Key.HasValue)
			{
				AssertNotNull("CostAccount should not be null", charge.CostAccount);
				AssertEquals("Creditor should be equal", chargeLine.Creditor.Key, charge.CostAccount.OH_Code);
			}

			if (chargeLine.Debtor != null && chargeLine.Debtor.Key.HasValue)
			{
				if (string.IsNullOrEmpty(chargeLine.Debtor.Key.Value))
				{
					using (charge.Factory.SetTempContext(BusinessContext.JobChargeImportingFromEDIMessage))
					{
						if (charge.Job.LocalCharges == null || !charge.IsValidDebtorForDefaulting(charge.Job.LocalCharges.PK))
						{
							AssertEquals("SellAccount should be null when Debtor key is empty", null, charge.SellAccount);
						}
						else
						{
							AssertNotNull("SellAccount should not be null", charge.SellAccount);
							AssertEquals("SellAccount should be local charges account with empty Debtor key", charge.Job.LocalCharges.PK, charge.SellAccount.PK);
						}
					}
				}
				else
				{
					AssertNotNull("SellAccount should not be null", charge.SellAccount);
					AssertEquals("Debtor should be equal", chargeLine.Debtor.Key, charge.SellAccount.OH_Code);
				}
			}

			if (chargeLine.Department != null && chargeLine.Department.Code.HasValue)
			{
				AssertNotNull("Department should not be null", charge.Department);
				AssertEquals("Department should be equal", chargeLine.Department.Code, charge.Department.GE_Code);
			}

			if (chargeLine.Description.HasValue)
			{
				AssertEquals("Description should be equal", chargeLine.Description, charge.JR_Desc);
			}

			if (chargeLine.DisplaySequence.HasValue)
			{
				AssertEquals("DisplaySequence should be equal", chargeLine.DisplaySequence, charge.JR_DisplaySequence);
			}

			if (chargeLine.SupplierReference.HasValue)
			{
				AssertEquals("SupplierReference should be equal to JR_CostReference", chargeLine.SupplierReference, charge.JR_CostReference);
			}

			if (chargeLine.SellGSTVATID != null && chargeLine.SellGSTVATID.TaxCode.HasValue && !charge.JR_AT_SellGSTRate.IsEmpty)
			{
				AssertNotNull("SellGSTRate should not be null", charge.SellGSTRate);
				AssertEquals("SellGSTRate should be equal", chargeLine.SellGSTVATID.TaxCode, charge.SellGSTRate.AT_Code);
			}

			if (chargeLine.SellInvoiceType.HasValue)
			{
				AssertEquals("InvoiceType should be equal", chargeLine.SellInvoiceType, charge.JR_InvoiceType);
			}

			if (chargeLine.SellLocalAmount.HasValue)
			{
				AssertEquals("Sell Local Amount should be equal", chargeLine.SellLocalAmount, charge.JR_LocalSellAmt);
			}

			if (chargeLine.SellOSAmount.HasValue)
			{
				AssertEquals("Sell OS Amount should be equal", chargeLine.SellOSAmount, charge.JR_OSSellAmt);
			}

			if (chargeLine.SellOSCurrency != null && chargeLine.SellOSCurrency.Code.HasValue)
			{
				AssertEquals("Sell OS Currency should be equal", chargeLine.SellOSCurrency.Code, charge.JR_RX_NKSellCurrency);
			}

			if (chargeLine.SellOSGSTVATAmount.HasValue && !charge.JR_AT_SellGSTRate.IsEmpty)
			{
				AssertEquals("SellOSGSTVAT Amount should be equal", chargeLine.SellOSGSTVATAmount, charge.JR_OSSellGSTAmt_Calc);
			}

			if (chargeLine.SellReference.HasValue)
			{
				AssertEquals("Sell Reference should be equal", chargeLine.SellReference, charge.JR_SellReference);
			}

			if (chargeLine.GovernmentReportingCostChargeCode.HasValue)
			{
				AssertEquals("Cost Govt Charge Code should be equal", chargeLine.GovernmentReportingCostChargeCode, charge.JR_CostGovtChargeCode);
			}

			if (chargeLine.GovernmentReportingSellChargeCode.HasValue)
			{
				AssertEquals("Sell Govt Charge Code should be equal", chargeLine.GovernmentReportingSellChargeCode, charge.JR_SellGovtChargeCode);
			}

			if (chargeLine.CostSupplyType != null)
			{
				var expectedSupplyType = AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value
					? (chargeLine.CostSupplyType.Code ?? ZString.Empty)
					: ZString.Empty;
				AssertEquals("Cost SupplyType Code should be equal", expectedSupplyType, charge.JR_CostSupplyType);
			}

			if (chargeLine.SellSupplyType != null)
			{
				var expectedSupplyType = AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value
					? (chargeLine.SellSupplyType.Code ?? ZString.Empty)
					: ZString.Empty;
				AssertEquals("Sell SupplyType Code should be equal", expectedSupplyType, charge.JR_SellSupplyType);
			}

			if (PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(GlbCompany.CurrentCompany))
			{
				if (chargeLine.CostPlaceOfSupply != null)
				{
					AssertEquals("Cost Place Of Supply Charge Code should be equal", chargeLine.CostPlaceOfSupply.Location.Code, charge.JR_CostPlaceOfSupply);
					AssertEquals("Cost Place Of Supply Type Charge Code should be equal", chargeLine.CostPlaceOfSupply.LocationType.Code, charge.JR_CostPlaceOfSupplyType);
				}
				if (chargeLine.SellPlaceOfSupply != null)
				{
					AssertEquals("Sell Place Of Supply Charge Code should be equal", chargeLine.SellPlaceOfSupply.Location.Code, charge.JR_SellPlaceOfSupply);
					AssertEquals("Sell Place Of Supply Type Charge Code should be equal", chargeLine.SellPlaceOfSupply.LocationType.Code, charge.JR_SellPlaceOfSupplyType);
				}
			}

			if (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.Value)
			{
				if (chargeLine.ARCashAdvanceRequired.HasValue)
				{
					AssertEquals("AR Advance Payment Required should be equal", chargeLine.ARCashAdvanceRequired, charge.JR_IsARCashAdvance);
				}
				if (chargeLine.APCashAdvanceRequired.HasValue)
				{
					AssertEquals("AP Advance Payment Required should be equal", chargeLine.APCashAdvanceRequired, charge.JR_IsAPCashAdvance);
				}
			}
		}

		void AssertChargeWhenJobIsClosed(Charge charge, ChargeLine chargeLine)
		{
			if (chargeLine.Branch != null && chargeLine.Branch.Code.HasValue)
			{
				AssertNotNull("charge.Branch should not be null", charge.Branch);
				AssertNotEquals("Branch should not be changed", chargeLine.Branch.Code, charge.Branch.GB_Code);
			}

			if (chargeLine.CostAPInvoiceNumber.HasValue)
			{
				AssertNotEquals("APInvoiceNum should not be changed", chargeLine.CostAPInvoiceNumber, charge.JR_APInvoiceNum);
			}

			if (chargeLine.CostDueDate.HasValue)
			{
				AssertNotEquals("CostDueDate should not be changed", chargeLine.CostDueDate, charge.JR_PaymentDate);
			}

			if (chargeLine.CostGSTVATID != null && chargeLine.CostGSTVATID.TaxCode.HasValue && !charge.JR_AT_CostGSTRate.IsEmpty)
			{
				AssertNotNull("CostGSTRate should not be null", charge.CostGSTRate);
				AssertNotEquals("CostGSTRate should not be changed", chargeLine.CostGSTVATID.TaxCode, charge.CostGSTRate.AT_Code);
			}

			if (chargeLine.CostInvoiceDate.HasValue)
			{
				AssertNotEquals("CostInvoiceDate should not be changed", chargeLine.CostInvoiceDate, charge.JR_APInvoiceDate);
			}

			if (chargeLine.CostLocalAmount.HasValue)
			{
				AssertNotEquals("Cost Local Amount should not be changed", chargeLine.CostLocalAmount, charge.JR_LocalCostAmt);
			}

			if (chargeLine.CostOSAmount.HasValue)
			{
				AssertNotEquals("Cost OS Amount should not be changed", chargeLine.CostOSAmount, charge.JR_OSCostAmt);
			}

			if (chargeLine.CostOSCurrency != null && chargeLine.CostOSCurrency.Code.HasValue)
			{
				AssertNotEquals("Cost Currency should not be changed", chargeLine.CostOSCurrency.Code, charge.JR_RX_NKCostCurrency);
			}

			if (chargeLine.CostOSGSTVATAmount.HasValue && !charge.JR_AT_CostGSTRate.IsEmpty)
			{
				AssertNotEquals("Cost OS GST VAT Amount should not be changed", chargeLine.CostOSGSTVATAmount, charge.JR_OSCostGSTAmt_Calc);
			}

			if (chargeLine.Department != null && chargeLine.Department.Code.HasValue)
			{
				AssertNotNull("Department should not be null", charge.Department);
				AssertNotEquals("Department should not be changed", chargeLine.Department.Code, charge.Department.GE_Code);
			}

			if (chargeLine.Description.HasValue)
			{
				AssertNotEquals("Description should not be changed", chargeLine.Description, charge.JR_Desc);
			}

			if (chargeLine.SellGSTVATID != null && chargeLine.SellGSTVATID.TaxCode.HasValue && !charge.JR_AT_SellGSTRate.IsEmpty)
			{
				AssertNotNull("SellGSTRate should not be null", charge.SellGSTRate);
				AssertNotEquals("SellGSTRate should not be changed", chargeLine.SellGSTVATID.TaxCode, charge.SellGSTRate.AT_Code);
			}

			if (chargeLine.SellOSAmount.HasValue)
			{
				AssertNotEquals("Sell OS Amount should not be changed", chargeLine.SellOSAmount, charge.JR_OSSellAmt);
			}

			if (chargeLine.SellOSCurrency != null && chargeLine.SellOSCurrency.Code.HasValue)
			{
				AssertNotEquals("Sell OS Currency should not be changed", chargeLine.SellOSCurrency.Code, charge.JR_RX_NKSellCurrency);
			}

			if (chargeLine.SellOSGSTVATAmount.HasValue && !charge.JR_AT_SellGSTRate.IsEmpty)
			{
				AssertNotEquals("SellOSGSTVAT Amount should not be changed", chargeLine.SellOSGSTVATAmount, charge.JR_OSSellGSTAmt_Calc);
			}
		}

		void StaffSetup(JobHeader jobToSave)
		{
			jobToSave.JH_GS_NKRepOps = "OS";
			jobToSave.JH_GS_NKRepSales = "SS";

			GlbStaff salesStaff = Factory.NewWithValidTestData<GlbStaff>();
			salesStaff.GS_Code = "SS";
			salesStaff.GS_FullName = "Sales";

			GlbStaff operationsStaff = Factory.NewWithValidTestData<GlbStaff>();
			operationsStaff.GS_Code = "OS";
			operationsStaff.GS_FullName = "Operations";

			GlbBranch homeBranch = TestObjectCreator.NonCurrentBranch;
			homeBranch.GB_Code = "SYD";
			homeBranch.GB_BranchName = "Sydney Branch";

			operationsStaff.GS_GB_HomeBranch = homeBranch.PK;
		}

		void BaseLineSetup(JobHeader jobToSave, ZDecimal rev1Amt, ZDecimal rev2Amt, ZDecimal cost1Amt, ZDecimal cost2Amt, ZDecimal wIP1Amt, ZDecimal wIP2Amt, ZDecimal accrual1Amt, ZDecimal accrual2Amt)
		{
			AccChargeRevRecOverride revRecOverride = TestObjectCreator.CC2.RevenueRecOverrides.AddNew();
			revRecOverride.JobType = RevenueRecognitionLookups.JobTypeAdditionalCodes.All;
			revRecOverride.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure;
			revRecOverride.Offset = 0;

			ARInvoice aRInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("123654" + jobToSave.JH_JobNum, GlbCompany.CurrentCompany.LocalCurrency, 1, TestObjectCreator.AALSHI);
			TransactionLine rev1 = aRInvoice.Lines.AddNew();
			SetLine(jobToSave, rev1, TransactionLineTypes.Revenue, rev1Amt, TestObjectCreator.CC1);
			JobCharge rev1JobCharge = TestObjectCreator.CreateJobCharge(rev1, jobToSave, rev1.ChargeCode, rev1.TransactionCurrency);
			rev1JobCharge.JR_OSCostAmt = 0;

			TransactionLine rev2 = aRInvoice.Lines.AddNew();
			SetLine(jobToSave, rev2, TransactionLineTypes.Revenue, rev2Amt, TestObjectCreator.CC2);
			JobCharge rev2JobCharge = TestObjectCreator.CreateJobCharge(rev2, jobToSave, rev2.ChargeCode, rev2.TransactionCurrency);
			rev2JobCharge.JR_OSCostAmt = 0;

			APInvoice aPInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("789456" + jobToSave.JH_JobNum, GlbCompany.CurrentCompany.LocalCurrency, 1, 0, 0, 0, 0, 0, 0);
			TransactionLine cost1 = aPInvoice.Lines.AddNew();
			SetLine(jobToSave, cost1, TransactionLineTypes.Cost, cost1Amt, TestObjectCreator.CC1);
			JobCharge cost1JobCharge = TestObjectCreator.CreateJobCharge(cost1, jobToSave, cost1.ChargeCode, cost1.TransactionCurrency);
			cost1JobCharge.JR_OSSellAmt = 0;

			TransactionLine cost2 = aPInvoice.Lines.AddNew();
			SetLine(jobToSave, cost2, TransactionLineTypes.Cost, cost2Amt, TestObjectCreator.CC2);
			JobCharge cost2JobCharge = TestObjectCreator.CreateJobCharge(cost2, jobToSave, cost2.ChargeCode, cost2.TransactionCurrency);
			cost2JobCharge.JR_OSSellAmt = 0;

			BaseCharge charge1 = Factory.NewWithValidTestData<BaseCharge>();
			charge1.JR_JH = jobToSave.PK;
			WIP wIP1 = Factory.New<WIP>();
			wIP1.AL_OSExTaxAmount = wIP1Amt;
			wIP1.AL_JH = jobToSave.PK;
			charge1.JR_AL_ARLine = wIP1.PK;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(wIP1);

			BaseCharge charge2 = Factory.NewWithValidTestData<BaseCharge>();
			charge2.JR_JH = jobToSave.PK;
			WIP wIP2 = Factory.New<WIP>();
			wIP2.AL_OSExTaxAmount = wIP2Amt;
			wIP2.AL_JH = jobToSave.PK;
			charge2.JR_AL_ARLine = wIP2.PK;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(wIP2);

			Accrual accrual1 = Factory.New<Accrual>();
			accrual1.AL_OSExTaxAmount = accrual1Amt;
			accrual1.AL_JH = jobToSave.PK;
			charge1.JR_AL_APLine = accrual1.PK;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(accrual1);

			Accrual accrual2 = Factory.New<Accrual>();
			accrual2.AL_OSExTaxAmount = accrual2Amt;
			accrual2.AL_JH = jobToSave.PK;
			charge2.JR_AL_APLine = accrual2.PK;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(accrual2);
		}

		void ChargeSetup(JobHeader jobToSave)
		{
			JobCharge jobCharge1 = Factory.New<JobCharge>();
			jobCharge1.JR_AC = TestObjectCreator.CC2.PK;
			jobCharge1.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge1.JR_GE = GlbDepartment.CurrentDepartment.PK;
			jobCharge1.JR_JH = jobToSave.PK;
			jobCharge1.JR_RX_NKCostCurrency = TestObjectCreator.AUD.RX_Code;
			jobCharge1.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			jobCharge1.JR_LocalCostAmt = 200m;
			jobCharge1.JR_OSCostAmt = 200m;

			JobCharge jobCharge2 = Factory.New<JobCharge>();
			jobCharge2.JR_AC = TestObjectCreator.CC2.PK;
			jobCharge2.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge2.JR_GE = GlbDepartment.CurrentDepartment.PK;
			jobCharge2.JR_JH = jobToSave.PK;
			jobCharge2.JR_RX_NKCostCurrency = TestObjectCreator.AUD.RX_Code;
			jobCharge2.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			jobCharge2.JR_LocalSellAmt = 100m;
			jobCharge2.JR_OSSellAmt = 100m;
		}

		void SetLine(JobHeader jobToSave, AccTransactionLines line, ZString lineType, ZDecimal amount, AccChargeCode chargeCode)
		{
			line.AL_OH = TestObjectCreator.AALSHI.PK;
			line.AL_JH = jobToSave.PK;
			line.AL_LineType = lineType;
			line.AL_LineAmount = amount;
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_AC = chargeCode.PK;
			if (line.AL_ExchangeRate == 1m)
			{
				line.AL_OSAmount = line.AL_LineAmount + line.AL_GSTVAT;
			}
		}

		public void TestImportJobCosting_WithBranchNotInCurrentCompany()
		{
			var differentCompany = Factory.New<GlbCompany>();
			differentCompany.GC_Code = "JCN";
			differentCompany.GC_RN_NKCountryCode = "CN";
			differentCompany.SetCurrency("CNY");
			differentCompany.GC_IsReciprocal = true;

			var differentBranch = differentCompany.Branches.AddNew();
			differentBranch.GB_Code = "FOC";
			differentBranch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, differentCompany.GC_RN_NKCountryCode)).Code;
			differentBranch.GB_BranchName = "Test Branch";

			Factory.Save();

			var chargeLine = new ChargeLine(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine.ImportMetaData.Instruction = InstructionType.Insert;
			chargeLine.ChargeCode = new ChargeCode();
			chargeLine.ChargeCode.Code = TestObjectCreator.CC1.AC_Code;
			chargeLine.ChargeCode.Description = TestObjectCreator.CC1.AC_Desc;
			chargeLine.CostOSAmount = 10M;

			try
			{
				SetUpUniversalShipment(differentBranch, GlbDepartment.CurrentDepartment, null, false, chargeLine);
				Fail("Expecting exception but none was thrown.");
			}
			catch (Exception ex)
			{
				if (!typeof(MessageProcessingBusinessFailureException).IsInstanceOfType(ex))
				{
					throw;
				}

				MessageProcessingBusinessFailureException messageProcessingBusinessFailureException = ex as MessageProcessingBusinessFailureException;
				string exceptionCaption = messageProcessingBusinessFailureException != null ? messageProcessingBusinessFailureException.Caption : null;

				AssertMultilineASCIIEquals("Exception message", @"The branch 'FOC' in the <JobCosting> does not belong to the system company 'EDI' processing the XML import.
Please ensure that the correct <Company> data is specified in the <DataTargetCollection>.
", ex.Message);
				AssertEquals("Exception caption", "Failed to create Job.", exceptionCaption);
			}
			finally
			{
				ReleaseMutexesOnJobs();
			}
		}

		#region XML Import

		public void TestImportShipmentWithChargeAndPostingInstruction()
		{
			using (Factory.AddDisposableService())
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				TestCaseHelper.ClearTable(JobShipmentSchema.Constants.TableName);

				var uniFactory = new UniversalObjectFactory();
				var message = TestCaseWithFactoryAndMessagingHelpers.GetQueuedUniversalShipmentMessage(uniFactory, ShipmentXMLWithChargeAndPostingInstruction);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalDataBuss.Management.UniversalMessageProcessingManager(new UniversalObjectFactory(Factory), serviceTaskLog);

				AssertEquals("Precondition", 0, Factory.CreateNewFactory().Load<JobChargePostingQueue>(new ZQuery()).Length);

				manager.Process(message);

				var result = Factory.CreateNewFactory().Load<JobChargePostingQueue>(new ZQuery());
				AssertEquals("Should have 1 JobChargePostingQueue created.", 1, result.Length);
				AssertEquals(1, result[0].JPQ_GroupID);
				AssertEquals("CST", result[0].JPQ_PostingInstruction);
				AssertEquals(EDIMessageStatusList.Codes.Warning, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log",
	@"Added Shipment (House Bill='NEW SHIPMENT') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='NEW SHIPMENT').", serviceTaskLog.ToString());
			}
		}

		const string ShipmentXMLWithChargeAndPostingInstruction = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
      </DataTargetCollection>
      <CodesMappedToTarget>true</CodesMappedToTarget>
    </DataContext>

    <JobCosting>
      <AccrualNotRecognized>0</AccrualNotRecognized>
      <AccrualRecognized>0</AccrualRecognized>
      <AgentRevenue>0</AgentRevenue>
      <Branch>
        <Code>BNE</Code>
        <Name>BN - AUBNE</Name>
      </Branch>
      <Currency>
        <Code>NZD</Code>
        <Description>New Zealand, Dollars</Description>
      </Currency>
      <LocalClientRevenue>0</LocalClientRevenue>
      <OperationsStaff>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </OperationsStaff>
      <OtherDebtorRevenue>0</OtherDebtorRevenue>
      <TotalAccrual>0</TotalAccrual>
      <TotalCost>0</TotalCost>
      <TotalJobProfit>0</TotalJobProfit>
      <TotalRevenue>0</TotalRevenue>
      <TotalWIP>0</TotalWIP>
      <WIPNotRecognized>0</WIPNotRecognized>
      <WIPRecognized>0</WIPRecognized>
      <ChargeLineCollection>
        <ChargeLine>
          <ChargeCode>
            <Code>BAF</Code>
          </ChargeCode>
          <ImportMetaData>
            <Instruction>Insert</Instruction>
            <PostingInstruction>PostCost</PostingInstruction>
            <MatchingCriteriaCollection>
              <MatchingCriteria>
                <FieldName>ChargeCode</FieldName>
                <Value>BAF</Value>
              </MatchingCriteria>
            </MatchingCriteriaCollection>
          </ImportMetaData>
          <SellOSAmount>00000001418</SellOSAmount>
          <SellOSCurrency>
            <Code>NZD</Code>
          </SellOSCurrency>
        </ChargeLine>
      </ChargeLineCollection>
    </JobCosting>

    <ContainerMode>
      <Code>LSE</Code>
      <Description>Loose</Description>
    </ContainerMode>
    <GoodsDescription>BIG FAT FISH</GoodsDescription>
    <PortOfDestination>
      <Code>AUBNE</Code>
      <Name>Brisbane</Name>
    </PortOfDestination>
    <PortOfOrigin>
      <Code>NZABY</Code>
      <Name>Albany</Name>
    </PortOfOrigin>
    <ShipmentIncoTerm>
      <Code>FOB</Code>
      <Description>Free On Board</Description>
    </ShipmentIncoTerm>
    <TotalNoOfPacks>0</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>CTN</Code>
      <Description>Carton</Description>
    </TotalNoOfPacksPackageType>
    <TotalVolume>0.300</TotalVolume>
    <TotalVolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Meters</Description>
    </TotalVolumeUnit>
    <TotalWeight>234.000</TotalWeight>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TransportMode>
      <Code>AIR</Code>
      <Description>Air Freight</Description>
    </TransportMode>
    <WayBillNumber>New Shipment</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>

    <CustomizedFieldCollection>
      <CustomizedField>
        <Key>CustomBlaString</Key>
        <DataType>String</DataType>
        <Value>TEST</Value>
      </CustomizedField>
    </CustomizedFieldCollection>

    <DateCollection>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2017-02-23T00:00:00</Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2017-02-23T00:00:00</Value>
      </Date>
    </DateCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <AddressShortCode>Pick Up Address</AddressShortCode>
        <OrganizationCode>BAROPT</OrganizationCode>
        <Address1>12 COOLIBAH DRIVE</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>PALM BEACH</City>
        <CompanyName>BARZ OPTICS</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Port>
          <Code>AUBNE</Code>
          <Name>Brisbane</Name>
        </Port>
        <Postcode>4221</Postcode>
        <State>QLD</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <AddressShortCode>PST: DIESLSTR 11</AddressShortCode>
        <OrganizationCode>ABABEU</OrganizationCode>
        <Address1>DIESLSTR 11</Address1>
        <Address2>57439 ATTENDORN, GERMANY</Address2>
        <AddressOverride>false</AddressOverride>
        <City>MOSCOW</City>
        <CompanyName>ABA BEUL</CompanyName>
        <Country>
          <Code>DE</Code>
          <Name>Germany</Name>
        </Country>
        <Port>
          <Code>DEFRA</Code>
          <Name>Frankfurt am Main</Name>
        </Port>
        <Postcode>113186</Postcode>
        <State>BE</State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

		#endregion

		#region Implementation

		TestErrorLogger Logger;
		protected UniversalObjectFactory UniversalObjectFactory;
		protected DateTime InvoiceDate;

		protected override void SetUp()
		{
			base.SetUp();

			Logger = new TestErrorLogger();

			UniversalObjectFactory = new UniversalObjectFactory();
			InvoiceDate = ZDateTime.Now.ToDateTime();

			var query = new ZQuery(AccTaxRateSchema.AT_Code, "GST").AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, Core.Constants.CountryCodes.Australia);

			var gst = Factory.LoadTop1<AccTaxRate>(query);
			if (gst != null)
			{
				gst.SetRate_ForTestOnly(10, 1);

				Factory.Save();
			}
		}

		TestObjectCreator TestObjectCreator => (testObjectCreator ??= new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		void SetLogger()
		{
			var topLevelShipmentForContext = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			topLevelShipmentForContext.DataContext = DataContextFactory.New();
			topLevelShipmentForContext.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			Logger.TopLevelDataObject = topLevelShipmentForContext;
		}

		#endregion
	}
}
