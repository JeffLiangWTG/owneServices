using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Accounting.ARAP.Invoicing.Testing
{
	public class SurchargeCalculatorTest : TestCaseWithFactory
	{
		#region GetSurchargeCalculationData

		public void TestGetSurchargeCalculationData()
		{
			var registryValue = new CodeDescriptionBoolCollection(AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.DefaultValue);
			var registryValueEnumerable = registryValue.OfType<CodeDescriptionBool>();
			registryValueEnumerable.ForEach(x => x.Bool = true);

			AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryValue);
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "AU", "LOC", "ABC");
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "AU", "LOX", "DEF");
			TestObjectCreator.Debtor.OH_RL_NKClosestPort = "AUSYD";
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("1000");
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.LocalChargesPK = TestObjectCreator.Debtor.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			job.IsManuallyCreated = true;

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR0001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
			var invoiceLine1 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "test", 100m);
			invoiceLine1.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine1.AL_SupplyType = "LOC";
			invoiceLine1.AL_PlaceOfSupply = "NSW";

			var invoiceLine2 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC2, TestObjectCreator.AUD, 1m, "test", 123m);
			invoiceLine2.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine2.AL_SupplyType = "LOX";
			invoiceLine2.AL_PlaceOfSupply = "VIC";

			var testData1 = new SurchargeTestData("ABC", 5m, invoiceLine1.AL_AC, TestObjectCreator.CC10.PK, job.PK);
			var testData2 = new SurchargeTestData("DEF", 7m, invoiceLine2.AL_AC, TestObjectCreator.CC11.PK, job.PK);

			var mock = new Mock<ISurchargeConfig>();
			ObjectFactory.Substitute(mock.Object);
			mock.Setup(x => x.GetSurcharge(testData1.SurchargeCode, testData1.LineChargePK)).Returns((testData1.Rate, testData1.SurchargeChargePK));
			mock.Setup(x => x.GetSurcharge(testData2.SurchargeCode, testData2.LineChargePK)).Returns((testData2.Rate, testData2.SurchargeChargePK));

			ISurchargeCalculator calculator = new SurchargeCalculator();
			var result = calculator.GetSurchargeCalculationData(invoice).OrderBy(x => x.SurchargeCode).ToList();
			AssertEquals("2 surchargeCalculationData returned.", 2, result.Count);

			AssertEquals("Surcharge code should be 'ABC'.", testData1.SurchargeCode, result[0].SurchargeCode);
			AssertEquals("Calculated amount should be 100m * 5m / 100 = 5m.", Utilities.Round(100m * testData1.Rate / 100, TestObjectCreator.AUD.Decimals), result[0].CalculatedAmount);
			AssertEquals("Charge code pk should be expectedChargeCode.", testData1.SurchargeChargePK, result[0].ChargePK);
			AssertEquals("Job pk should be job's pk", testData1.JobPK, result[0].JobPK);

			AssertEquals("Surcharge code should be 'DEF'.", testData2.SurchargeCode, result[1].SurchargeCode);
			AssertEquals("Calculated amount should be 123m * 7m / 100 = 8.61m.", Utilities.Round(123m * testData2.Rate / 100, TestObjectCreator.AUD.Decimals), result[1].CalculatedAmount);
			AssertEquals("Charge code pk should be expectedChargeCode.", testData2.SurchargeChargePK, result[1].ChargePK);
			AssertEquals("Job pk should be job's pk", testData2.JobPK, result[1].JobPK);

			mock.VerifyAll();
		}

		public void TestGetSurchargeCalculationDataWithEmptyTaxId()
		{
			var surchargeApplicationOne = TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "", "", "", "ABC");

			var shipment = TestObjectCreator.CreateShipment("1000");
			var job = TestObjectCreator.CreateJob(shipment, false);

			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), organisation: TestObjectCreator.Debtor);
			var invoiceLine = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 100m);
			invoiceLine.AL_AT = TestObjectCreator.FREEVAT.PK;

			Assert("Pre condition: ", surchargeApplicationOne.ASP_AT.IsEmpty);

			var mock = new Mock<ISurchargeConfig>();
			ObjectFactory.Substitute(mock.Object);
			mock.Setup(x => x.GetSurcharge("ABC", invoiceLine.AL_AC)).Returns((5m, TestObjectCreator.CC10.PK));

			ISurchargeCalculator calculator = new SurchargeCalculator();
			var result = calculator.GetSurchargeCalculationData(invoice).OrderBy(x => x.SurchargeCode).ToList();

			AssertEquals("result.count.", 1, result.Count);
			AssertEquals("Calculated amount should be 100m * 5m / 100 = 5m.", 5m, result[0].CalculatedAmount);
		}

		public void TestGetSurchargeCalculationDataWithSpecificTaxId()
		{
			var surchargeApplicationOne = TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "", "", "", "ABC");
			var surchargeApplicationTwo = TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "", "", "", "DEF");

			surchargeApplicationOne.ASP_AT = TestObjectCreator.FREEVAT.PK;

			var shipment = TestObjectCreator.CreateShipment("1000");
			var job = TestObjectCreator.CreateJob(shipment, false);

			Factory.Save();

			var expectedLineChargeCode = TestObjectCreator.CC1;

			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), organisation: TestObjectCreator.Debtor);
			var invoiceLine1 = TestObjectCreator.CreateInvoiceLine(invoice, job, expectedLineChargeCode, 100m);
			invoiceLine1.AL_AT = TestObjectCreator.FREEVAT.PK;

			var invoiceLine2 = TestObjectCreator.CreateInvoiceLine(invoice, job, expectedLineChargeCode, 123m);
			invoiceLine2.AL_AT = ZGuid.Empty;

			var invoiceLine3 = TestObjectCreator.CreateInvoiceLine(invoice, job, expectedLineChargeCode, 150m);
			invoiceLine3.AL_AT = TestObjectCreator.GSTFREE1.PK;

			var mock = new Mock<ISurchargeConfig>();
			ObjectFactory.Substitute(mock.Object);
			mock.Setup(x => x.GetSurcharge("ABC", invoiceLine1.AL_AC)).Returns((5m, TestObjectCreator.CC10.PK));
			mock.Setup(x => x.GetSurcharge("DEF", invoiceLine2.AL_AC)).Returns((7m, TestObjectCreator.CC11.PK));
			mock.Setup(x => x.GetSurcharge("DEF", invoiceLine3.AL_AC)).Returns((8m, TestObjectCreator.CC11.PK));

			ISurchargeCalculator calculator = new SurchargeCalculator();

			var result = calculator.GetSurchargeCalculationData(invoice).OrderBy(x => x.SurchargeCode).ToList();

			AssertEquals("result.Count", 2, result.Count);

			AssertEquals("result 0 SurchargeCode", "ABC", result[0].SurchargeCode);
			AssertEquals("Calculated amount should be 100m * 5m / 100 = 5m.", 5m, result[0].CalculatedAmount);
			AssertEquals("Charge code pk should be expectedChargeCode.", TestObjectCreator.CC10.PK, result[0].ChargePK);
			AssertEquals("Job pk should be job's pk", job.PK, result[0].JobPK);

			AssertEquals("result 1 SurchargeCode", "DEF", result[1].SurchargeCode);
			AssertEquals("Calculated amount should be 373m * 8m / 100 = 29.84m.", 29.84m, result[1].CalculatedAmount);
			AssertEquals("Charge code pk should be expectedChargeCode.", TestObjectCreator.CC11.PK, result[1].ChargePK);
			AssertEquals("Job pk should be job's pk", job.PK, result[1].JobPK);
		}

		public void TestGetSurchargeCalculationDataWhenLineHasMultipleApplications()
		{
			var registryValue = new CodeDescriptionBoolCollection(AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.DefaultValue);
			var registryValueEnumerable = registryValue.OfType<CodeDescriptionBool>();
			registryValueEnumerable.ForEach(x => x.Bool = true);

			AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryValue);
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			TestObjectCreator.CreateAccSurchargeApplication("SHP", "ALL", "ALL", "CON", "AU", "LOC", "ABC");
			TestObjectCreator.CreateAccSurchargeApplication("SHP", "ALL", "ALL", "CON", "AU", "LOC", "DEF");
			TestObjectCreator.Debtor.OH_RL_NKClosestPort = "AUSYD";
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("1000");
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.LocalChargesPK = TestObjectCreator.Debtor.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			job.IsManuallyCreated = true;

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR0001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
			var invoiceLine1 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "test", 100m);
			invoiceLine1.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine1.AL_SupplyType = "LOC";
			invoiceLine1.AL_PlaceOfSupply = "NSW";

			var testData1 = new SurchargeTestData("ABC", 5m, invoiceLine1.AL_AC, TestObjectCreator.CC10.PK, job.PK);
			var testData2 = new SurchargeTestData("DEF", 7m, invoiceLine1.AL_AC, TestObjectCreator.CC11.PK, job.PK);

			var mock = new Mock<ISurchargeConfig>();
			mock.Setup(x => x.GetSurcharge(testData1.SurchargeCode, testData1.LineChargePK)).Returns((testData1.Rate, testData1.SurchargeChargePK));
			mock.Setup(x => x.GetSurcharge(testData2.SurchargeCode, testData2.LineChargePK)).Returns((testData2.Rate, testData2.SurchargeChargePK));

			ISurchargeCalculator calculator = new SurchargeCalculator();
			ObjectFactory.Substitute(mock.Object);
			var result = calculator.GetSurchargeCalculationData(invoice).OrderBy(x => x.SurchargeCode).ToList();
			AssertEquals("2 surchargeCalculationData returned.", 2, result.Count);

			AssertEquals("Surcharge code should be 'ABC'.", testData1.SurchargeCode, result[0].SurchargeCode);
			AssertEquals("Calculated amount should be 100m * 5m / 100 = 5m.", Utilities.Round(100m * testData1.Rate / 100, TestObjectCreator.AUD.Decimals), result[0].CalculatedAmount);
			AssertEquals("Charge code pk should be expectedChargeCode.", testData1.SurchargeChargePK, result[0].ChargePK);
			AssertEquals("Job pk should be job's pk", testData1.JobPK, result[0].JobPK);

			AssertEquals("Surcharge code should be 'DEF'.", testData2.SurchargeCode, result[1].SurchargeCode);
			AssertEquals("Calculated amount should be 100m * 7m / 100 = 7m.", Utilities.Round(100m * testData2.Rate / 100, TestObjectCreator.AUD.Decimals), result[1].CalculatedAmount);
			AssertEquals("Charge code pk should be expectedChargeCode.", testData2.SurchargeChargePK, result[1].ChargePK);
			AssertEquals("Job pk should be job's pk", testData2.JobPK, result[1].JobPK);

			mock.VerifyAll();
		}

		public void TestGetSurchargeCalculationDataGroupByChargeCode()
		{
			var registryValue = new CodeDescriptionBoolCollection(AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.DefaultValue);
			var registryValueEnumerable = registryValue.OfType<CodeDescriptionBool>();
			registryValueEnumerable.ForEach(x => x.Bool = true);

			AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryValue);
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "AU", "LOC", "ABC");
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "AU", "LOX", "DEF");
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("1000");
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.LocalChargesPK = TestObjectCreator.Debtor.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			job.IsManuallyCreated = true;

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR0001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
			var invoiceLine1 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "test", 100.77m);
			invoiceLine1.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine1.AL_SupplyType = "LOC";
			invoiceLine1.AL_PlaceOfSupply = "NSW";

			var invoiceLine2 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC2, TestObjectCreator.AUD, 1m, "test", 123.88m);
			invoiceLine2.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine2.AL_SupplyType = "LOC";
			invoiceLine2.AL_PlaceOfSupply = "NSW";

			var invoiceLine3 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC3, TestObjectCreator.AUD, 1m, "test", 321.99m);
			invoiceLine3.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine3.AL_SupplyType = "LOX";
			invoiceLine3.AL_PlaceOfSupply = "VIC";

			var testData1 = new SurchargeTestData("ABC", 5m, invoiceLine1.AL_AC, TestObjectCreator.CC10.PK, job.PK);
			var testData2 = new SurchargeTestData("ABC", 5m, invoiceLine2.AL_AC, TestObjectCreator.CC10.PK, job.PK);
			var testData3 = new SurchargeTestData("DEF", 7m, invoiceLine3.AL_AC, TestObjectCreator.CC11.PK, job.PK);

			var mock = new Mock<ISurchargeConfig>();
			ObjectFactory.Substitute(mock.Object);
			mock.Setup(x => x.GetSurcharge(testData1.SurchargeCode, testData1.LineChargePK)).Returns((testData1.Rate, testData1.SurchargeChargePK));
			mock.Setup(x => x.GetSurcharge(testData2.SurchargeCode, testData2.LineChargePK)).Returns((testData2.Rate, testData2.SurchargeChargePK));
			mock.Setup(x => x.GetSurcharge(testData3.SurchargeCode, testData3.LineChargePK)).Returns((testData3.Rate, testData3.SurchargeChargePK));

			ISurchargeCalculator calculator = new SurchargeCalculator();
			var result = calculator.GetSurchargeCalculationData(invoice).OrderBy(x => x.SurchargeCode).ToList();
			AssertEquals("2 surchargeCalculationData returned as invoiceLine1 and invoiceLine2 are grouped.", 2, result.Count);

			AssertEquals("Surcharge code should be 'ABC'.", testData1.SurchargeCode, result[0].SurchargeCode);
			AssertEquals("Calculated amount should be (100.77m + 123.88m) * 5m / 100 = 11.23m.", Utilities.Round((100.77m + 123.88m) * testData1.Rate / 100, TestObjectCreator.AUD.Decimals), result[0].CalculatedAmount);
			AssertEquals("Charge code pk should be expectedChargeCode.", testData1.SurchargeChargePK, result[0].ChargePK);
			AssertEquals("Job pk should be job's pk", testData1.JobPK, result[0].JobPK);

			AssertEquals("Surcharge code should be 'DEF'.", testData3.SurchargeCode, result[1].SurchargeCode);
			AssertEquals("Calculated amount should be 321.99 * 7m / 100 = 16.10m.", Utilities.Round(321.99m * testData3.Rate / 100, TestObjectCreator.AUD.Decimals), result[1].CalculatedAmount);
			AssertEquals("Charge code pk should be expectedChargeCode.", testData3.SurchargeChargePK, result[1].ChargePK);
			AssertEquals("Job pk should be job's pk", testData3.JobPK, result[1].JobPK);

			mock.VerifyAll();
		}

		public void TestGetSurchargeCalculationDataGroupByJob()
		{
			var registryValue = new CodeDescriptionBoolCollection(AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.DefaultValue);
			var registryValueEnumerable = registryValue.OfType<CodeDescriptionBool>();
			registryValueEnumerable.ForEach(x => x.Bool = true);

			AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryValue);
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "AU", "LOC", "ABC");
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("1000");
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.LocalChargesPK = TestObjectCreator.Debtor.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			job.IsManuallyCreated = true;

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR0001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
			var invoiceLine1 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "test", 100.77m);
			invoiceLine1.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine1.AL_SupplyType = "LOC";
			invoiceLine1.AL_PlaceOfSupply = "NSW";

			var invoiceLine2 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC2, TestObjectCreator.AUD, 1m, "test", 123.88m);
			invoiceLine2.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine2.AL_SupplyType = "LOC";
			invoiceLine2.AL_PlaceOfSupply = "NSW";

			var shipment1 = TestObjectCreator.CreateShipment("2000");
			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.LocalChargesPK = TestObjectCreator.Debtor.PK;
			job1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job1.JH_ParentID = shipment1.PK;
			job1.IsManuallyCreated = true;

			var invoiceLine3 = TestObjectCreator.CreateARInvoiceLine(invoice, job1, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "test", 100m);
			invoiceLine3.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine3.AL_SupplyType = "LOC";
			invoiceLine3.AL_PlaceOfSupply = "NSW";

			var testData1 = new SurchargeTestData("ABC", 5m, TestObjectCreator.CC1.PK, TestObjectCreator.CC10.PK, job.PK);
			var testData2 = new SurchargeTestData("ABC", 5m, TestObjectCreator.CC2.PK, TestObjectCreator.CC10.PK, job1.PK);

			var mock = new Mock<ISurchargeConfig>();
			mock.Setup(x => x.GetSurcharge(testData1.SurchargeCode, testData1.LineChargePK)).Returns((testData1.Rate, testData1.SurchargeChargePK));
			mock.Setup(x => x.GetSurcharge(testData1.SurchargeCode, testData2.LineChargePK)).Returns((testData1.Rate, testData1.SurchargeChargePK));

			ISurchargeCalculator calculator = new SurchargeCalculator();
			ObjectFactory.Substitute(mock.Object);
			var result = calculator.GetSurchargeCalculationData(invoice).OrderByDescending(x => x.CalculatedAmount).ToList();
			AssertEquals("2 surchargeCalculationData returned as invoiceLine1 and invoiceLine2 are grouped.", 2, result.Count);

			AssertEquals("Surcharge code should be 'ABC'.", testData1.SurchargeCode, result[0].SurchargeCode);
			AssertEquals("Calculated amount should be (100.77m + 123.88m) * 5m / 100 = 10.04m.", Utilities.Round((100.77m + 123.88m) * testData1.Rate / 100, TestObjectCreator.AUD.Decimals), result[0].CalculatedAmount);
			AssertEquals("Charge code pk should be expectedChargeCode.", testData1.SurchargeChargePK, result[0].ChargePK);
			AssertEquals("Job pk should be job's pk", testData1.JobPK, result[0].JobPK);

			AssertEquals("Surcharge code should be 'ABC'.", testData2.SurchargeCode, result[1].SurchargeCode);
			AssertEquals("Calculated amount should be 100m * 7m / 100 = 7m.", Utilities.Round(100m * testData2.Rate / 100, TestObjectCreator.AUD.Decimals), result[1].CalculatedAmount);
			AssertEquals("Charge code pk should be expectedChargeCode.", testData2.SurchargeChargePK, result[1].ChargePK);
			AssertEquals("Job pk should be job1's pk", testData2.JobPK, result[1].JobPK);

			mock.VerifyAll();
		}

		public void TestGetSurchargeCalculationDataWhenNoConfig()
		{
			var registryValue = new CodeDescriptionBoolCollection(AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.DefaultValue);
			var registryValueEnumerable = registryValue.OfType<CodeDescriptionBool>();
			registryValueEnumerable.ForEach(x => x.Bool = true);

			AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryValue);
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "AU", "LOC", "ABC");
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("1000");
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.LocalChargesPK = TestObjectCreator.Debtor.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			job.IsManuallyCreated = true;

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR0001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
			var invoiceLine1 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "test", 100m);
			invoiceLine1.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine1.AL_SupplyType = "LOC";
			invoiceLine1.AL_PlaceOfSupply = "NSW";

			var invoiceLine2 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC2, TestObjectCreator.AUD, 1m, "test", 123m);
			invoiceLine2.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine2.AL_SupplyType = "LOX";
			invoiceLine2.AL_PlaceOfSupply = "VIC";

			var testData1 = new SurchargeTestData("ABC", 0m, invoiceLine1.AL_AC, TestObjectCreator.CC10.PK, job.PK);

			var mock = new Mock<ISurchargeConfig>();
			ObjectFactory.Substitute(mock.Object);
			mock.Setup(x => x.GetSurcharge(testData1.SurchargeCode, testData1.LineChargePK)).Returns((testData1.Rate, testData1.SurchargeChargePK));

			ISurchargeCalculator calculator = new SurchargeCalculator();
			var result = calculator.GetSurchargeCalculationData(invoice).OrderBy(x => x.SurchargeCode).ToList();
			AssertEquals("0 surchargeCalculationData returned as no surcharge config.", 0, result.Count);

			mock.VerifyAll();
		}

		public void TestLineWithEmptyChargeCodeWontBeCalculated()
		{
			var registryValue = new CodeDescriptionBoolCollection(AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.DefaultValue);
			var registryValueEnumerable = registryValue.OfType<CodeDescriptionBool>();
			registryValueEnumerable.ForEach(x => x.Bool = true);

			AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryValue);
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "AU", "LOC", "ABC");
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "AU", "LOX", "DEF");
			TestObjectCreator.Debtor.OH_RL_NKClosestPort = "AUSYD";
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("1000");
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.LocalChargesPK = TestObjectCreator.Debtor.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			job.IsManuallyCreated = true;

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR0001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
			var invoiceLine1 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "test", 100m);
			invoiceLine1.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine1.AL_SupplyType = "LOC";
			invoiceLine1.AL_PlaceOfSupply = "NSW";
			invoiceLine1.AL_AC = ZGuid.Empty;

			var testData1 = new SurchargeTestData("ABC", 5m, invoiceLine1.AL_AC, TestObjectCreator.CC10.PK, job.PK);

			var mock = new Mock<ISurchargeConfig>();
			ObjectFactory.Substitute(mock.Object);
			mock.Setup(x => x.GetSurcharge(testData1.SurchargeCode, testData1.LineChargePK)).Returns((testData1.Rate, testData1.SurchargeChargePK));

			ISurchargeCalculator calculator = new SurchargeCalculator();
			var result = calculator.GetSurchargeCalculationData(invoice).OrderBy(x => x.SurchargeCode).ToList();
			AssertEquals("0 surchargeCalculationData returned.", 0, result.Count);

			mock.Verify(x => x.GetSurcharge(testData1.SurchargeCode, testData1.LineChargePK), Times.Never());
		}

		public void TestLineWithNegativeOSAmountWontBeCalculated()
		{
			var registryValue = new CodeDescriptionBoolCollection(AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.DefaultValue);
			var registryValueEnumerable = registryValue.OfType<CodeDescriptionBool>();
			registryValueEnumerable.ForEach(x => x.Bool = true);

			AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryValue);
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "AU", "LOC", "ABC");
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "AU", "LOX", "DEF");
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "AU", "LOA", "GHI");
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "AU", "DSB", "JKL");
			TestObjectCreator.Debtor.OH_RL_NKClosestPort = "AUSYD";
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("1000");
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.LocalChargesPK = TestObjectCreator.Debtor.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			job.IsManuallyCreated = true;

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR0001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
			var invoiceLine1 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "test", 100m);
			invoiceLine1.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine1.AL_SupplyType = "LOC";
			invoiceLine1.AL_PlaceOfSupply = "NSW";

			var invoiceLine2 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC2, TestObjectCreator.AUD, 1m, "test", -90m);
			invoiceLine2.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine2.AL_SupplyType = "LOC";
			invoiceLine2.AL_PlaceOfSupply = "NSW";

			var invoiceLine3 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC3, TestObjectCreator.AUD, 1m, "test", -100m);
			invoiceLine3.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine3.AL_SupplyType = "LOX";
			invoiceLine3.AL_PlaceOfSupply = "NSW";

			var invoiceLine4 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC4, TestObjectCreator.AUD, 1m, "test", 90m);
			invoiceLine4.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine4.AL_SupplyType = "LOX";
			invoiceLine4.AL_PlaceOfSupply = "NSW";

			var invoiceLine5 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC5, TestObjectCreator.AUD, 1m, "test", 90m);
			invoiceLine5.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine5.AL_SupplyType = "LOA";
			invoiceLine5.AL_PlaceOfSupply = "NSW";

			var invoiceLine6 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC6, TestObjectCreator.AUD, 1m, "test", -90m);
			invoiceLine6.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine6.AL_SupplyType = "LOA";
			invoiceLine6.AL_PlaceOfSupply = "NSW";

			var invoiceLine7 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC7, TestObjectCreator.AUD, 1m, "test", -90m);
			invoiceLine7.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine7.AL_SupplyType = "DSB";
			invoiceLine7.AL_PlaceOfSupply = "NSW";

			var testData1 = new SurchargeTestData("ABC", 2m, invoiceLine1.AL_AC, TestObjectCreator.CC10.PK, job.PK);
			var testData2 = new SurchargeTestData("ABC", 2m, invoiceLine2.AL_AC, TestObjectCreator.CC10.PK, job.PK);
			var testData3 = new SurchargeTestData("DEF", 4m, invoiceLine3.AL_AC, TestObjectCreator.CC11.PK, job.PK);
			var testData4 = new SurchargeTestData("DEF", 4m, invoiceLine4.AL_AC, TestObjectCreator.CC11.PK, job.PK);
			var testData5 = new SurchargeTestData("GHI", 5m, invoiceLine5.AL_AC, TestObjectCreator.CC12.PK, job.PK);
			var testData6 = new SurchargeTestData("GHI", 5m, invoiceLine6.AL_AC, TestObjectCreator.CC12.PK, job.PK);
			var testData7 = new SurchargeTestData("JKL", 6m, invoiceLine7.AL_AC, TestObjectCreator.CC13.PK, job.PK);

			var mock = new Mock<ISurchargeConfig>();
			mock.Setup(x => x.GetSurcharge(testData1.SurchargeCode, testData1.LineChargePK)).Returns((testData1.Rate, testData1.SurchargeChargePK));
			mock.Setup(x => x.GetSurcharge(testData2.SurchargeCode, testData2.LineChargePK)).Returns((testData2.Rate, testData2.SurchargeChargePK));
			mock.Setup(x => x.GetSurcharge(testData3.SurchargeCode, testData3.LineChargePK)).Returns((testData3.Rate, testData3.SurchargeChargePK));
			mock.Setup(x => x.GetSurcharge(testData4.SurchargeCode, testData4.LineChargePK)).Returns((testData4.Rate, testData4.SurchargeChargePK));
			mock.Setup(x => x.GetSurcharge(testData5.SurchargeCode, testData5.LineChargePK)).Returns((testData5.Rate, testData5.SurchargeChargePK));
			mock.Setup(x => x.GetSurcharge(testData6.SurchargeCode, testData6.LineChargePK)).Returns((testData6.Rate, testData6.SurchargeChargePK));
			mock.Setup(x => x.GetSurcharge(testData7.SurchargeCode, testData7.LineChargePK)).Returns((testData7.Rate, testData7.SurchargeChargePK));

			ISurchargeCalculator calculator = new SurchargeCalculator();
			ObjectFactory.Substitute(mock.Object);
			var result = calculator.GetSurchargeCalculationData(invoice).OrderBy(x => x.SurchargeCode).ToList();
			AssertEquals("1 surchargeCalculationData returned as the grouped amount is positive.", 1, result.Count);

			AssertEquals("Surcharge code should be 'ABC'.", testData1.SurchargeCode, result[0].SurchargeCode);
			AssertEquals("Calculated amount should be (100m + -90m) * 2 / 100m.", Utilities.Round((100m + -90m) * testData1.Rate / 100, TestObjectCreator.AUD.Decimals), result[0].CalculatedAmount);

			mock.VerifyAll();
		}

		public void TestGetSurchargeCalculationData_DifferentSupplyType()
		{
			AssertGetSurchargeCalculationData_SupplyType("", "LOC", "LOX");
		}

		public void TestGetSurchargeCalculationData_SameSupplyType()
		{
			AssertGetSurchargeCalculationData_SupplyType("LOC", "LOC", "LOC");
		}

		void AssertGetSurchargeCalculationData_SupplyType(ZString expectSupplyType, params ZString[] supplyTypes)
		{
			var registryValue = new CodeDescriptionBoolCollection(AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.DefaultValue);
			var registryValueEnumerable = registryValue.OfType<CodeDescriptionBool>();
			registryValueEnumerable.ForEach(x => x.Bool = true);

			AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryValue);
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "AU", "", "ABC");
			TestObjectCreator.Debtor.OH_RL_NKClosestPort = "AUSYD";
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("1000");
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.LocalChargesPK = TestObjectCreator.Debtor.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			job.IsManuallyCreated = true;

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR0001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
			var invoiceLine1 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "test", 100m);
			invoiceLine1.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine1.AL_SupplyType = supplyTypes[0];
			invoiceLine1.AL_PlaceOfSupply = "NSW";

			var invoiceLine2 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC2, TestObjectCreator.AUD, 1m, "test", 123m);
			invoiceLine2.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine2.AL_SupplyType = supplyTypes[1];
			invoiceLine2.AL_PlaceOfSupply = "VIC";

			var testData1 = new SurchargeTestData("ABC", 5m, invoiceLine1.AL_AC, TestObjectCreator.CC10.PK, job.PK);
			var testData2 = new SurchargeTestData("ABC", 5m, invoiceLine2.AL_AC, TestObjectCreator.CC10.PK, job.PK);

			var mock = new Mock<ISurchargeConfig>();
			ObjectFactory.Substitute(mock.Object);
			mock.Setup(x => x.GetSurcharge(testData1.SurchargeCode, testData1.LineChargePK)).Returns((testData1.Rate, testData1.SurchargeChargePK));
			mock.Setup(x => x.GetSurcharge(testData2.SurchargeCode, testData2.LineChargePK)).Returns((testData2.Rate, testData2.SurchargeChargePK));

			ISurchargeCalculator calculator = new SurchargeCalculator();
			var result = calculator.GetSurchargeCalculationData(invoice).OrderBy(x => x.SurchargeCode).ToList();
			AssertEquals("1 surchargeCalculationData returned.", 1, result.Count);
			AssertEquals("Surcharge code should be 'ABC'.", testData1.SurchargeCode, result[0].SurchargeCode);

			mock.VerifyAll();
		}

		public void TestGetSurchargeCalculationData_DifferentPlaceOfSupply()
		{
			AssertGetSurchargeCalculationData_PlaceOfSupply("", "VIC", "NSW");
		}

		public void TestGetSurchargeCalculationData_SamePlaceOfSupply()
		{
			AssertGetSurchargeCalculationData_PlaceOfSupply("NSW", "NSW", "NSW");
		}

		void AssertGetSurchargeCalculationData_PlaceOfSupply(ZString expectPlaceOfSupply, params ZString[] placeOfSupplies)
		{
			var registryValue = new CodeDescriptionBoolCollection(AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.DefaultValue);
			var registryValueEnumerable = registryValue.OfType<CodeDescriptionBool>();
			registryValueEnumerable.ForEach(x => x.Bool = true);

			AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryValue);
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "AU", "", "ABC");
			TestObjectCreator.Debtor.OH_RL_NKClosestPort = "AUSYD";
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("1000");
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.LocalChargesPK = TestObjectCreator.Debtor.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			job.IsManuallyCreated = true;

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR0001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
			var invoiceLine1 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "test", 100m);
			invoiceLine1.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine1.AL_PlaceOfSupply = placeOfSupplies[0];

			var invoiceLine2 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC2, TestObjectCreator.AUD, 1m, "test", 123m);
			invoiceLine2.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine2.AL_PlaceOfSupply = placeOfSupplies[1];

			var testData1 = new SurchargeTestData("ABC", 5m, invoiceLine1.AL_AC, TestObjectCreator.CC10.PK, job.PK);
			var testData2 = new SurchargeTestData("ABC", 5m, invoiceLine2.AL_AC, TestObjectCreator.CC10.PK, job.PK);

			var mock = new Mock<ISurchargeConfig>();
			ObjectFactory.Substitute(mock.Object);
			mock.Setup(x => x.GetSurcharge(testData1.SurchargeCode, testData1.LineChargePK)).Returns((testData1.Rate, testData1.SurchargeChargePK));
			mock.Setup(x => x.GetSurcharge(testData2.SurchargeCode, testData2.LineChargePK)).Returns((testData2.Rate, testData2.SurchargeChargePK));

			ISurchargeCalculator calculator = new SurchargeCalculator();
			var result = calculator.GetSurchargeCalculationData(invoice).OrderBy(x => x.SurchargeCode).ToList();
			AssertEquals("1 surchargeCalculationData returned.", 1, result.Count);
			AssertEquals("Surcharge code should be 'ABC'.", testData1.SurchargeCode, result[0].SurchargeCode);

			mock.VerifyAll();
		}

		public void TestGetSurchargeCalculationData_MultipleInvoices()
		{
			var registryValue = new CodeDescriptionBoolCollection(AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.DefaultValue);
			var registryValueEnumerable = registryValue.OfType<CodeDescriptionBool>();
			registryValueEnumerable.ForEach(x => x.Bool = true);

			AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryValue);
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "AU", "LOC", "ABC");
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "AU", "LOX", "DEF");
			TestObjectCreator.Debtor.OH_RL_NKClosestPort = "AUSYD";
			TestObjectCreator.Debtor.CompanyData.OB_ARApplicableSurcharges = "ALL";
			testObjectCreator.AALSHI.CompanyData.OB_ARApplicableSurcharges = "ABC";
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("1000");
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.LocalChargesPK = TestObjectCreator.Debtor.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			job.IsManuallyCreated = true;

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR0001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
			var invoiceLine1 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "test", 100m);
			invoiceLine1.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine1.AL_SupplyType = "LOC";
			invoiceLine1.AL_PlaceOfSupply = "NSW";

			var invoiceLine2 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC2, TestObjectCreator.AUD, 1m, "test", 123m);
			invoiceLine2.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine2.AL_SupplyType = "LOX";
			invoiceLine2.AL_PlaceOfSupply = "VIC";

			var invoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("AR0001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			var invoiceLine3 = TestObjectCreator.CreateARInvoiceLine(invoice1, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "test", 100m);
			invoiceLine3.AL_OH = TestObjectCreator.AALSHI.PK;
			invoiceLine3.AL_SupplyType = "LOC";
			invoiceLine3.AL_PlaceOfSupply = "NSW";

			var invoiceLine4 = TestObjectCreator.CreateARInvoiceLine(invoice1, job, TestObjectCreator.CC2, TestObjectCreator.AUD, 1m, "test", 123m);
			invoiceLine4.AL_OH = TestObjectCreator.AALSHI.PK;
			invoiceLine4.AL_SupplyType = "LOX";
			invoiceLine4.AL_PlaceOfSupply = "VIC";

			var testData1 = new SurchargeTestData("ABC", 5m, invoiceLine1.AL_AC, TestObjectCreator.CC10.PK, job.PK);
			var testData2 = new SurchargeTestData("DEF", 7m, invoiceLine2.AL_AC, TestObjectCreator.CC11.PK, job.PK);

			var mock = new Mock<ISurchargeConfig>();
			mock.Setup(x => x.GetSurcharge(testData1.SurchargeCode, testData1.LineChargePK)).Returns((testData1.Rate, testData1.SurchargeChargePK));
			mock.Setup(x => x.GetSurcharge(testData2.SurchargeCode, testData2.LineChargePK)).Returns((testData2.Rate, testData2.SurchargeChargePK));

			ISurchargeCalculator calculator = new SurchargeCalculator();
			ObjectFactory.Substitute(mock.Object);
			var result = calculator.GetSurchargeCalculationData(invoice).OrderBy(x => x.SurchargeCode).ToList();
			var result1 = calculator.GetSurchargeCalculationData(invoice1).OrderBy(x => x.SurchargeCode).ToList();
			AssertEquals("2 surchargeCalculationData returned.", 2, result.Count);
			AssertEquals("1 surchargeCalculationData returned.", 1, result1.Count);

			AssertEquals("'DEF' should be excluded.", "ABC", result1[0].SurchargeCode);

			mock.VerifyAll();
		}

		public void TestGetSurchargeCalculationData_NonJobCharge()
		{
			var registryValue = new CodeDescriptionBoolCollection(AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.DefaultValue);
			var registryValueEnumerable = registryValue.OfType<CodeDescriptionBool>();
			registryValueEnumerable.ForEach(x => x.Bool = true);

			AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryValue);
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			TestObjectCreator.CreateAccSurchargeApplication("NJR", "ALL", "ALL", "CON", "AU", "LOC", "ABC");
			TestObjectCreator.Debtor.OH_RL_NKClosestPort = "AUSYD";
			Factory.Save();

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR0001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
			var invoiceLine1 = (ARInvoiceLine)invoice.Lines.AddNew();
			invoiceLine1.AL_AC = TestObjectCreator.CC1.PK;
			invoiceLine1.AL_OSExTaxAmount = 100m;
			invoiceLine1.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine1.AL_SupplyType = "";
			invoiceLine1.AL_PlaceOfSupply = "NSW";

			var testData1 = new SurchargeTestData("ABC", 5m, invoiceLine1.AL_AC, TestObjectCreator.CC10.PK, ZGuid.Empty);
			var mock = new Mock<ISurchargeConfig>();
			mock.Setup(x => x.GetSurcharge(testData1.SurchargeCode, testData1.LineChargePK)).Returns((testData1.Rate, testData1.SurchargeChargePK));
			AssertNull("Precondition:", invoice.InvoicingJob?.JobType);

			ISurchargeCalculator calculator = new SurchargeCalculator();
			ObjectFactory.Substitute(mock.Object);
			var result = calculator.GetSurchargeCalculationData(invoice).OrderBy(x => x.SurchargeCode).ToList();
			AssertEquals("0 surchargeCalculationData returned.", 0, result.Count);
			mock.Verify(x => x.GetSurcharge(testData1.SurchargeCode, testData1.LineChargePK), Times.Never());
		}

		#endregion

		#region ARApplicableSurcharges related

		public void TestGetSurchargeCalculationDataWhenARApplicableSurchargesIsNON()
		{
			var registryValue = new CodeDescriptionBoolCollection(AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.DefaultValue);
			var registryValueEnumerable = registryValue.OfType<CodeDescriptionBool>();
			registryValueEnumerable.ForEach(x => x.Bool = true);

			AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryValue);
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "AU", "LOC", "ABC");
			TestObjectCreator.Debtor.CompanyData.OB_ARApplicableSurcharges = "NON";
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("1000");
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.LocalChargesPK = TestObjectCreator.Debtor.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			job.IsManuallyCreated = true;

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR0001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
			var invoiceLine1 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "test", 100m);
			invoiceLine1.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine1.AL_SupplyType = "LOC";
			invoiceLine1.AL_PlaceOfSupply = "NSW";

			var testData = new SurchargeTestData("ABC", 5m, invoiceLine1.AL_AC, TestObjectCreator.CC10.PK, job.PK);

			var mock = new Mock<ISurchargeConfig>();
			mock.Setup(x => x.GetSurcharge(testData.SurchargeCode, testData.LineChargePK)).Returns((testData.Rate, testData.SurchargeChargePK));

			ISurchargeCalculator calculator = new SurchargeCalculator();
			ObjectFactory.Substitute(mock.Object);
			var result = calculator.GetSurchargeCalculationData(invoice).OrderBy(x => x.SurchargeCode).ToList();
			AssertEquals("0 surchargeCalculationData returned as OB_ARApplicableSurcharges is NON.", 0, result.Count);
			mock.Verify(x => x.GetSurcharge(testData.SurchargeCode, testData.LineChargePK), Times.Never());
		}

		public void TestGetSurchargeCalculationDataWhenOnlyOneARApplicableSurchargeIncluded()
		{
			var registryValue = new CodeDescriptionBoolCollection(AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.DefaultValue);
			var registryValueEnumerable = registryValue.OfType<CodeDescriptionBool>();
			registryValueEnumerable.ForEach(x => x.Bool = true);

			AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryValue);
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "AU", "LOC", "ABC");
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "AU", "LOX", "DEF");
			TestObjectCreator.Debtor.CompanyData.OB_ARApplicableSurcharges = "DEF";
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("1000");
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.LocalChargesPK = TestObjectCreator.Debtor.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			job.IsManuallyCreated = true;

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR0001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
			var invoiceLine1 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "test", 100m);
			invoiceLine1.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine1.AL_SupplyType = "LOC";
			invoiceLine1.AL_PlaceOfSupply = "NSW";

			var invoiceLine2 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC2, TestObjectCreator.AUD, 1m, "test", 123m);
			invoiceLine2.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine2.AL_SupplyType = "LOX";
			invoiceLine2.AL_PlaceOfSupply = "VIC";

			var testData1 = new SurchargeTestData("ABC", 5m, invoiceLine1.AL_AC, TestObjectCreator.CC10.PK, job.PK);
			var testData2 = new SurchargeTestData("DEF", 7m, invoiceLine2.AL_AC, TestObjectCreator.CC11.PK, job.PK);

			var mock = new Mock<ISurchargeConfig>();
			mock.Setup(x => x.GetSurcharge(testData1.SurchargeCode, testData1.LineChargePK)).Returns((testData1.Rate, testData1.SurchargeChargePK));
			mock.Setup(x => x.GetSurcharge(testData2.SurchargeCode, testData2.LineChargePK)).Returns((testData2.Rate, testData2.SurchargeChargePK));

			ISurchargeCalculator calculator = new SurchargeCalculator();
			ObjectFactory.Substitute(mock.Object);
			var result = calculator.GetSurchargeCalculationData(invoice).OrderBy(x => x.SurchargeCode).ToList();
			AssertEquals("1 surchargeCalculationData returned as surcharge 'DEF' included only.", 1, result.Count);
			AssertEquals("Surcharge code should be 'ABC'.", testData2.SurchargeCode, result[0].SurchargeCode);

			mock.Verify(x => x.GetSurcharge(testData1.SurchargeCode, testData1.LineChargePK), Times.Never());
			mock.Verify(x => x.GetSurcharge(testData2.SurchargeCode, testData2.LineChargePK));
		}

		public void TestGetSurchargeCalculationDataWhenPartARApplicableSurchargesIncluded()
		{
			var registryValue = new CodeDescriptionBoolCollection(AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.DefaultValue);
			var registryValueEnumerable = registryValue.OfType<CodeDescriptionBool>();
			registryValueEnumerable.ForEach(x => x.Bool = true);

			AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryValue);
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "AU", "LOC", "ABC");
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "AU", "LOX", "DEF");
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "AU", "LOX", "GHI");
			TestObjectCreator.Debtor.CompanyData.OB_ARApplicableSurcharges = "ABC,GHI";
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("1000");
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.LocalChargesPK = TestObjectCreator.Debtor.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			job.IsManuallyCreated = true;

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR0001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
			var invoiceLine1 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "test", 100m);
			invoiceLine1.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine1.AL_SupplyType = "LOC";
			invoiceLine1.AL_PlaceOfSupply = "NSW";

			var invoiceLine2 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC2, TestObjectCreator.AUD, 1m, "test", 123m);
			invoiceLine2.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine2.AL_SupplyType = "LOX";
			invoiceLine2.AL_PlaceOfSupply = "VIC";

			var invoiceLine3 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC3, TestObjectCreator.AUD, 1m, "test", 321m);
			invoiceLine3.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine3.AL_SupplyType = "LOX";
			invoiceLine3.AL_PlaceOfSupply = "TAS";

			var testData1 = new SurchargeTestData("ABC", 5m, invoiceLine1.AL_AC, TestObjectCreator.CC10.PK, job.PK);
			var testData2 = new SurchargeTestData("DEF", 7m, invoiceLine2.AL_AC, TestObjectCreator.CC11.PK, job.PK);
			var testData3 = new SurchargeTestData("GHI", 9m, invoiceLine3.AL_AC, TestObjectCreator.CC12.PK, job.PK);

			var mock = new Mock<ISurchargeConfig>();
			mock.Setup(x => x.GetSurcharge(testData1.SurchargeCode, testData1.LineChargePK)).Returns((testData1.Rate, testData1.SurchargeChargePK));
			mock.Setup(x => x.GetSurcharge(testData2.SurchargeCode, testData2.LineChargePK)).Returns((testData2.Rate, testData2.SurchargeChargePK));
			mock.Setup(x => x.GetSurcharge(testData3.SurchargeCode, testData3.LineChargePK)).Returns((testData3.Rate, testData3.SurchargeChargePK));

			ISurchargeCalculator calculator = new SurchargeCalculator();
			ObjectFactory.Substitute(mock.Object);
			var result = calculator.GetSurchargeCalculationData(invoice).OrderBy(x => x.SurchargeCode).ToList();
			AssertEquals("2 surchargeCalculationData returned as surcharge 'ABC' and 'GHI' included", 2, result.Count);

			AssertEquals("Surcharge code should be 'ABC'.", testData1.SurchargeCode, result[0].SurchargeCode);
			AssertEquals("Surcharge code should be 'GHI'.", testData3.SurchargeCode, result[1].SurchargeCode);

			mock.Verify(x => x.GetSurcharge(testData1.SurchargeCode, testData1.LineChargePK));
			mock.Verify(x => x.GetSurcharge(testData2.SurchargeCode, testData2.LineChargePK), Times.Never());
			mock.Verify(x => x.GetSurcharge(testData3.SurchargeCode, testData3.LineChargePK));
		}

		public void TestGetSurchargeCalculationDataWhenAllARApplicableSurchargesIncluded()
		{
			var registryValue = new CodeDescriptionBoolCollection(AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.DefaultValue);
			var registryValueEnumerable = registryValue.OfType<CodeDescriptionBool>();
			registryValueEnumerable.ForEach(x => x.Bool = true);

			AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryValue);
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "AU", "LOC", "ABC");
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "AU", "LOX", "DEF");
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "AU", "LOX", "GHI");
			TestObjectCreator.Debtor.CompanyData.OB_ARApplicableSurcharges = "ALL";
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("1000");
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.LocalChargesPK = TestObjectCreator.Debtor.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			job.IsManuallyCreated = true;

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR0001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
			var invoiceLine1 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "test", 100m);
			invoiceLine1.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine1.AL_SupplyType = "LOC";
			invoiceLine1.AL_PlaceOfSupply = "NSW";

			var invoiceLine2 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC2, TestObjectCreator.AUD, 1m, "test", 123m);
			invoiceLine2.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine2.AL_SupplyType = "LOX";
			invoiceLine2.AL_PlaceOfSupply = "VIC";

			var invoiceLine3 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC3, TestObjectCreator.AUD, 1m, "test", 321m);
			invoiceLine3.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine3.AL_SupplyType = "LOX";
			invoiceLine3.AL_PlaceOfSupply = "TAS";

			var testData1 = new SurchargeTestData("ABC", 5m, invoiceLine1.AL_AC, TestObjectCreator.CC10.PK, job.PK);
			var testData2 = new SurchargeTestData("DEF", 7m, invoiceLine2.AL_AC, TestObjectCreator.CC11.PK, job.PK);
			var testData3 = new SurchargeTestData("GHI", 9m, invoiceLine3.AL_AC, TestObjectCreator.CC12.PK, job.PK);

			var mock = new Mock<ISurchargeConfig>();
			mock.Setup(x => x.GetSurcharge(testData1.SurchargeCode, testData1.LineChargePK)).Returns((testData1.Rate, testData1.SurchargeChargePK));
			mock.Setup(x => x.GetSurcharge(testData2.SurchargeCode, testData2.LineChargePK)).Returns((testData2.Rate, testData2.SurchargeChargePK));
			mock.Setup(x => x.GetSurcharge(testData3.SurchargeCode, testData3.LineChargePK)).Returns((testData3.Rate, testData3.SurchargeChargePK));

			ISurchargeCalculator calculator = new SurchargeCalculator();
			ObjectFactory.Substitute(mock.Object);
			var result = calculator.GetSurchargeCalculationData(invoice).OrderBy(x => x.SurchargeCode).ToList();
			AssertEquals("3 surchargeCalculationData returned as surcharges 'ABC' and 'DEF' and 'GHI' included.", 3, result.Count);

			AssertEquals("Surcharge code should be 'ABC'.", testData1.SurchargeCode, result[0].SurchargeCode);
			AssertEquals("Surcharge code should be 'DEF'.", testData2.SurchargeCode, result[1].SurchargeCode);
			AssertEquals("Surcharge code should be 'GHI'.", testData3.SurchargeCode, result[2].SurchargeCode);
			mock.VerifyAll();
		}

		#endregion

		#region SurchargeApplication related

		public void TestSurchargeApplications_OrganizationCategory_DifferentSurchargeCode()
		{
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "AU", "LOC", "ABC");
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "BUS", "CON", "AU", "LOC", "DEF");
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "NAT", "CON", "AU", "LOC", "GHI");

			AssertSurchargeApplications(true);
		}

		public void TestSurchargeApplications_OrganizationCategory_SameSurchargeCode()
		{
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "AU", "LOC", "ABC");
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "BUS", "CON", "AU", "LOC", "ABC");
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "NAT", "CON", "AU", "LOC", "GHI");

			AssertSurchargeApplications(false);
		}

		public void TestSurchargeApplications_JobType_DifferentSurchargeCode()
		{
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "AU", "LOC", "ABC");
			TestObjectCreator.CreateAccSurchargeApplication("SHP", "ALL", "ALL", "CON", "AU", "LOC", "DEF");
			TestObjectCreator.CreateAccSurchargeApplication("QSH", "ALL", "ALL", "CON", "AU", "LOC", "GHI");

			AssertSurchargeApplications(true);
		}

		public void TestSurchargeApplications_JobType_SameSurchargeCode()
		{
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "AU", "LOC", "ABC");
			TestObjectCreator.CreateAccSurchargeApplication("SHP", "ALL", "ALL", "CON", "AU", "LOC", "ABC");
			TestObjectCreator.CreateAccSurchargeApplication("QSH", "ALL", "NAT", "CON", "AU", "LOC", "GHI");

			AssertSurchargeApplications(false);
		}

		public void TestSurchargeApplications_HomeCountryOrZone_DifferentSurchargeCode()
		{
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "AU", "LOC", "ABC");
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "AU", "ALL", "CON", "AU", "LOC", "DEF");
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "EUX", "ALL", "CON", "AU", "LOC", "GHI");

			AssertSurchargeApplications(true);
		}

		public void TestSurchargeApplications_HomeCountryOrZone_SameSurchargeCode()
		{
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "AU", "LOC", "ABC");
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "AU", "ALL", "CON", "AU", "LOC", "ABC");
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "EUX", "ALL", "CON", "AU", "LOC", "GHI");

			AssertSurchargeApplications(false);
		}

		public void TestSurchargeApplications_PlaceOfSupply_DifferentSurchargeCode()
		{
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "ALL", "LOC", "ABC");
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "AU", "LOC", "DEF");
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "CN", "LOC", "GHI");

			AssertSurchargeApplications(true);
		}

		public void TestSurchargeApplications_PlaceOfSupply_SameSurchargeCode()
		{
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "ALL", "LOC", "ABC");
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "AU", "ALL", "CON", "AU", "LOC", "ABC");
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "EUX", "ALL", "CON", "CN", "LOC", "GHI");

			AssertSurchargeApplications(false);
		}

		public void TestSurchargeApplications_SupplyType_DifferentSurchargeCode()
		{
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "ALL", "", "ABC");
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "ALL", "LOC", "DEF");
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "ALL", "LOX", "GHI");

			AssertSurchargeApplications(true);
		}

		public void TestSurchargeApplications_SupplyType_SameSurchargeCode()
		{
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "ALL", "", "ABC");
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "ALL", "LOC", "ABC");
			TestObjectCreator.CreateAccSurchargeApplication("ALL", "ALL", "ALL", "CON", "ALL", "LOX", "GHI");

			AssertSurchargeApplications(false);
		}

		void AssertSurchargeApplications(bool containsDifferentApplications)
		{
			var registryValue = new CodeDescriptionBoolCollection(AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.DefaultValue);
			var registryValueEnumerable = registryValue.OfType<CodeDescriptionBool>();
			registryValueEnumerable.ForEach(x => x.Bool = true);

			AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryValue);
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			TestObjectCreator.Debtor.OH_RL_NKClosestPort = "AUSYD";
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("1000");
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.LocalChargesPK = TestObjectCreator.Debtor.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			job.IsManuallyCreated = true;

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR0001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
			var invoiceLine1 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "test", 100m);
			invoiceLine1.AL_OH = TestObjectCreator.Debtor.PK;
			invoiceLine1.AL_SupplyType = "LOC";
			invoiceLine1.AL_PlaceOfSupply = "NSW";

			var testData1 = new SurchargeTestData("ABC", 2m, invoiceLine1.AL_AC, TestObjectCreator.CC10.PK, job.PK);
			var testData2 = new SurchargeTestData("DEF", 3m, invoiceLine1.AL_AC, TestObjectCreator.CC10.PK, job.PK);
			var testData3 = new SurchargeTestData("GHI", 4m, invoiceLine1.AL_AC, TestObjectCreator.CC10.PK, job.PK);

			var mock = new Mock<ISurchargeConfig>();
			mock.Setup(x => x.GetSurcharge(testData1.SurchargeCode, testData1.LineChargePK)).Returns((testData1.Rate, testData1.SurchargeChargePK));
			mock.Setup(x => x.GetSurcharge(testData2.SurchargeCode, testData2.LineChargePK)).Returns((testData2.Rate, testData2.SurchargeChargePK));
			mock.Setup(x => x.GetSurcharge(testData3.SurchargeCode, testData3.LineChargePK)).Returns((testData3.Rate, testData3.SurchargeChargePK));

			ISurchargeCalculator calculator = new SurchargeCalculator();
			ObjectFactory.Substitute(mock.Object);
			var result = calculator.GetSurchargeCalculationData(invoice).OrderBy(x => x.SurchargeCode).ToList();

			if (containsDifferentApplications)
			{
				AssertEquals("2 surchargeCalculationData returned.", 2, result.Count);
				Assert(result.Any(x => x.SurchargeCode == "ABC"));
				AssertEquals(100m * (testData1.Rate / 100), result.First(x => x.SurchargeCode == "ABC").CalculatedAmount);
				Assert(result.Any(x => x.SurchargeCode == "DEF"));
				AssertEquals(100m * (testData2.Rate / 100), result.First(x => x.SurchargeCode == "DEF").CalculatedAmount);
				mock.Verify(x => x.GetSurcharge(testData1.SurchargeCode, testData1.LineChargePK));
				mock.Verify(x => x.GetSurcharge(testData2.SurchargeCode, testData2.LineChargePK));
			}
			else
			{
				AssertEquals("1 surchargeCalculationData returned.", 1, result.Count);
				Assert(result.Any(x => x.SurchargeCode == "ABC"));
				AssertEquals(100m * (testData1.Rate / 100), result.First().CalculatedAmount);
				mock.Verify(x => x.GetSurcharge(testData1.SurchargeCode, testData1.LineChargePK));
			}
		}

		#endregion

		#region SurchargeApplication related

		public void TestGetSurchargeLines()
		{
			var jobHeader = new Job.Loader(TestObjectCreator.CreateShipment("1000")).TryLoadOrCreate();

			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "A00001", organisation: TestObjectCreator.Debtor);

			var line1 = TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, jobHeader, TestObjectCreator.CC1, invoice.TransactionCurrency, invoice.AH_ExchangeRate, "ARInvoiceLine1", 1000m, TestObjectCreator.GST1.PK);
			TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, jobHeader, TestObjectCreator.CC2, invoice.TransactionCurrency, invoice.AH_ExchangeRate, "ARInvoiceLine2", 1000m, TestObjectCreator.GST1.PK);
			TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, jobHeader, TestObjectCreator.CC3, invoice.TransactionCurrency, invoice.AH_ExchangeRate, "ARInvoiceLine3", 1000m, TestObjectCreator.GST1.PK);
			TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, jobHeader, TestObjectCreator.CC4, invoice.TransactionCurrency, invoice.AH_ExchangeRate, "ARInvoiceLine4", 1000m, TestObjectCreator.GST1.PK);
			TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, jobHeader, TestObjectCreator.CC5, invoice.TransactionCurrency, invoice.AH_ExchangeRate, "ARInvoiceLine5", 1000m, TestObjectCreator.GST1.PK);
			var line6A = TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, jobHeader, TestObjectCreator.CC6, invoice.TransactionCurrency, invoice.AH_ExchangeRate, "ARInvoiceLine6A", 1000m, TestObjectCreator.GST1.PK);
			var line6B = TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, jobHeader, TestObjectCreator.CC6, invoice.TransactionCurrency, invoice.AH_ExchangeRate, "ARInvoiceLine6B", 1000m, TestObjectCreator.GST1.PK);

			var surchargeConfigCC1 = invoice.Company.AccSurchargeConfigurations.AddNew();
			surchargeConfigCC1.ASC_Code = "TST";
			surchargeConfigCC1.ASC_AC_ChargeCode = TestObjectCreator.CC1.PK;

			var surchargeConfigCC6 = invoice.Company.AccSurchargeConfigurations.AddNew();
			surchargeConfigCC6.ASC_Code = "TST";
			surchargeConfigCC6.ASC_AC_ChargeCode = TestObjectCreator.CC6.PK;

			ISurchargeCalculator calculator = new SurchargeCalculator();

			CombineAssertions("Should only return transaction lines which those's charge code are set in Surcharge Configuration.", () => {
				var surchargeLinePKs = calculator.GetSurchargeLines(invoice).Select(x => x.PK);

				AssertEquals(3, surchargeLinePKs.Count());
				Assert("ARInvoiceLine1", surchargeLinePKs.Contains(line1.PK));
				Assert("ARInvoiceLine6A", surchargeLinePKs.Contains(line6A.PK));
				Assert("ARInvoiceLine6B", surchargeLinePKs.Contains(line6B.PK));
			});

			invoice.Company.AccSurchargeConfigurations.Remove(surchargeConfigCC1);
			CombineAssertions("Should only return transaction lines which those's charge code are set in Surcharge Configuration.", () => {
				var surchargeLinePKs = calculator.GetSurchargeLines(invoice).Select(x => x.PK);

				AssertEquals(2, surchargeLinePKs.Count());
				Assert("ARInvoiceLine6A", surchargeLinePKs.Contains(line6A.PK));
				Assert("ARInvoiceLine6B", surchargeLinePKs.Contains(line6B.PK));
			});
		}

		public void TestGetSurchargeLines_ArgumentNullException()
		{
			ISurchargeCalculator calculator = new SurchargeCalculator();
			var exceptionForInvoice = AssertExceptionThrown<ArgumentNullException>(() => calculator.GetSurchargeLines(null));

			AssertEquals("Value cannot be null.\r\nParameter name: invoiceBase", exceptionForInvoice.Message);
		}
		#endregion

		#region Implementation

		readonly struct SurchargeTestData
		{
			public SurchargeTestData(ZString surchargeCode, ZDecimal rate, ZGuid lineChargePK, ZGuid surchargeChargePK, ZGuid jobPK)
			{
				SurchargeCode = surchargeCode;
				Rate = rate;
				LineChargePK = lineChargePK;
				SurchargeChargePK = surchargeChargePK;
				JobPK = jobPK;
			}

			public ZString SurchargeCode { get; }
			public ZDecimal Rate { get; }
			public ZGuid LineChargePK { get; }
			public ZGuid SurchargeChargePK { get; }
			public ZGuid JobPK { get; }
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
			}
		}
		TestObjectCreator testObjectCreator;

		#endregion
	}
}
