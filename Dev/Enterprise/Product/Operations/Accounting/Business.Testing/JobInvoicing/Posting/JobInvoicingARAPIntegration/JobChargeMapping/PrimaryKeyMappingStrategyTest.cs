using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class PrimaryKeyMappingStrategyTest : TestCaseWithFactory
	{
		public void TestTryMapTransactionLineByPrimaryKey_InvalidParameter()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001", true);
			var job = TestObjectCreator.CreateJob(shipment);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100m, 100m);
			Factory.Save();

			var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "00001001", TestObjectCreator.AUD, 1m, 150m, 0m, 150m, 0m);
			var line = header.Lines[0];
			line.AL_AC = charge.JR_AC;
			line.AL_JH = job.PK;

			var additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);
			AssertEquals("Pre-condition: the line should not match any apportioned charges.", null, additionalInfoProvider.GetApportionedChargePK(line.PK));
			AssertEquals("Pre-condition: the line Should not have OriginalJobCharge", null, line.OriginalJobCharge);

			var universalLine = JobChargeMappingTestHelper.CreateUniversalLine(null, jobNumber: "S0001");
			var matchedCharges = PrimaryKeyMappingStrategy.TryMapTransactionLine(Factory, universalLine, line, additionalInfoProvider);
			CombineAssertions("Posting Journal has null Primary Key.", () =>
			{
				AssertEquals(0, matchedCharges.Length);
				AssertEquals(null, additionalInfoProvider.GetApportionedChargePK(line.PK));
				AssertCollectedInfo(null);
			});

			additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);
			universalLine = JobChargeMappingTestHelper.CreateUniversalLine("Invalid PK", jobNumber: "S0001");
			matchedCharges = PrimaryKeyMappingStrategy.TryMapTransactionLine(Factory, universalLine, line, additionalInfoProvider);
			CombineAssertions("Posting Journal has invalid Primary Key.", () =>
			{
				AssertEquals(0, matchedCharges.Length);
				AssertEquals(null, additionalInfoProvider.GetApportionedChargePK(line.PK));
				AssertCollectedInfo(JobChargeMappingInfoType.InvalidKey);
			});

			additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);
			universalLine = JobChargeMappingTestHelper.CreateUniversalLine(charge.PK.ToString());
			matchedCharges = PrimaryKeyMappingStrategy.TryMapTransactionLine(Factory, universalLine, line, additionalInfoProvider);
			CombineAssertions("Posting Journal has no Job number nor consol number.", () =>
			{
				AssertEquals(0, matchedCharges.Length);
				AssertEquals(null, additionalInfoProvider.GetApportionedChargePK(line.PK));
				AssertCollectedInfo(null);
			});

			additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);
			universalLine = JobChargeMappingTestHelper.CreateUniversalLine(Guid.NewGuid().ToString());
			matchedCharges = PrimaryKeyMappingStrategy.TryMapTransactionLine(Factory, universalLine, line, additionalInfoProvider);
			CombineAssertions("XUT Primary key does NOT match any charge in DB.", () =>
			{
				AssertEquals(0, matchedCharges.Length);
				AssertEquals(null, additionalInfoProvider.GetApportionedChargePK(line.PK));
				AssertCollectedInfo(null);
			});

			void AssertCollectedInfo(JobChargeMappingInfoType? infoType)
			{
				if (infoType.HasValue)
				{
					AssertEquals(infoType, additionalInfoProvider.TransactionLineMappingInfoCollection.Single(x => x.LinePK == line.PK).InfoType);
				}
				else
				{
					AssertEquals(false, additionalInfoProvider.TransactionLineMappingInfoCollection.Any(x => x.LinePK == line.PK));
				}
			}
		}

		public void TestTryMapTransactionLineByPrimaryKey_Charge()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001", true);
			var job = TestObjectCreator.CreateJob(shipment);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1);
			Factory.Save();

			var universalLine = JobChargeMappingTestHelper.CreateUniversalLine(charge.PK.ToString(), jobNumber: "S0001");
			var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "00001001", TestObjectCreator.AUD, 1m, 150m, 0m, 150m, 0m);
			var line = header.Lines[0];
			line.AL_AC = charge.JR_AC;
			line.AL_JH = job.PK;
			var additionalInfoProvider = new TransactionImportAdditionalInfoProvider();

			AssertEquals("Pre-condition: the line should not match any apportioned charges.", null, additionalInfoProvider.GetApportionedChargePK(line.PK));
			AssertEquals("Pre-condition: the line Should not have OriginalJobCharge", null, line.OriginalJobCharge);

			var matchedCharges = PrimaryKeyMappingStrategy.TryMapTransactionLine(Factory, universalLine, line, additionalInfoProvider);
			AssertEquals("Should successfully match a shipment-level charge", 1, matchedCharges.Length);
			AssertEquals("Should match shipment charge when PostingJournal has Job number but has no consol number.", charge.PK, matchedCharges.First().PK);
			AssertEquals(null, additionalInfoProvider.GetApportionedChargePK(line.PK));
			AssertEquals(null, additionalInfoProvider.GetConsolCostPK(line.PK));
		}

		public void TestTryMapTransactionLineByPrimaryKey_ApportionedCharge()
		{
			var consol = TestObjectCreator.CreateConsol(consolNum: "C0001");
			var shipment1 = TestObjectCreator.CreateShipment("S0001", consol);
			var shipment2 = TestObjectCreator.CreateShipment("S0002", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1);
			var job2 = TestObjectCreator.CreateJob(shipment2);
			var consolCost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100m);
			var consolCost2 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC2, 200m);
			Factory.Save();

			var apportionedCharge1 = consolCost1.ApportionmentCharges.Where(x => x.JR_JH == job1.PK).Single();
			var universalLine = JobChargeMappingTestHelper.CreateUniversalLine(apportionedCharge1.PK.ToString(), jobNumber: "S0001", consolNumber: "C0001");
			var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "00001001", TestObjectCreator.AUD, 1m, 150m, 0m, 150m, 0m);
			var line = header.Lines[0];
			line.AL_AC = apportionedCharge1.JR_AC;
			line.AL_JH = job1.PK;
			var additionalInfoProvider = new TransactionImportAdditionalInfoProvider();

			var matchedCharges = PrimaryKeyMappingStrategy.TryMapTransactionLine(Factory, universalLine, line, additionalInfoProvider);

			AssertEquals("Should not match to a shipment-level charge", 0, matchedCharges.Length);
			AssertEquals("Should match consol apportioned charge when posting journal has both Job number and consol number.", apportionedCharge1.PK, additionalInfoProvider.GetApportionedChargePK(line.PK));
			AssertEquals(null, additionalInfoProvider.GetConsolCostPK(line.PK));
		}

		public void TestTryMapTransactionLineByPrimaryKey_ConsolCost()
		{
			var consol = TestObjectCreator.CreateConsol(consolNum: "C0001");
			var shipment1 = TestObjectCreator.CreateShipment("S0001", consol);
			var shipment2 = TestObjectCreator.CreateShipment("S0002", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1);
			var job2 = TestObjectCreator.CreateJob(shipment2);
			var consolCost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100m);
			var consolCost2 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC2, 200m);
			Factory.Save();

			var apportionedCharge1 = consolCost1.ApportionmentCharges.Where(x => x.JR_JH == job1.PK).Single();
			var universalLine = JobChargeMappingTestHelper.CreateUniversalLine(consolCost1.PK.ToString(), jobNumber: null, consolNumber: "C0001");
			var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "00001001", TestObjectCreator.AUD, 1m, 150m, 0m, 150m, 0m);
			var line = header.Lines[0];
			line.AL_AC = apportionedCharge1.JR_AC;
			line.AL_JH = job1.PK;
			var additionalInfoProvider = new TransactionImportAdditionalInfoProvider();

			var matchedCharges = PrimaryKeyMappingStrategy.TryMapTransactionLine(Factory, universalLine, line, additionalInfoProvider);

			AssertEquals("Should not match to a shipment-level charge", 0, matchedCharges.Length);
			AssertEquals(null, additionalInfoProvider.GetApportionedChargePK(line.PK));
			AssertEquals("Should match consol cost when posting journal has no Job number but has consol number.", consolCost1.PK, additionalInfoProvider.GetConsolCostPK(line.PK));
		}

		public void TestGetRelatedApportionedChargesFilter()
		{
			var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "00001001", TestObjectCreator.AUD, 1m, 150m, 0m, 150m, 0m);
			var line = header.Lines[0];
			var additionalInfoProvider = new TransactionImportAdditionalInfoProvider();
			var apportionedChargePK = ZGuid.NewZGuid();

			var filter = PrimaryKeyMappingStrategy.GetRelatedApportionedChargesFilter(line, additionalInfoProvider);
			AssertEquals("Should be no result query.", true, filter.IsNoResultQuery);
			AssertEquals("Sql Text should be equal.", "", filter.LiteralTextSqlFormatted);

			additionalInfoProvider.MapTransactionLineWithApportionedCharge(line.PK, apportionedChargePK);
			filter = PrimaryKeyMappingStrategy.GetRelatedApportionedChargesFilter(line, additionalInfoProvider);
			AssertEquals("Should not be no result query.", false, filter.IsNoResultQuery);
			AssertEqualsIgnoreLineBreaks("Sql Text should be equal", $"JR_PK = '{apportionedChargePK.ToString()}'", filter.LiteralTextSqlFormatted);
		}

		public void TestGetRelatedConsolCostsFilter()
		{
			var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "00001001", TestObjectCreator.AUD, 1m, 150m, 0m, 150m, 0m);
			var line = header.Lines[0];
			var additionalInfoProvider = new TransactionImportAdditionalInfoProvider();
			var consolCostPK = ZGuid.NewZGuid();

			var filter = PrimaryKeyMappingStrategy.GetRelatedConsolCostsFilter(line, additionalInfoProvider);
			AssertEquals("Should be no result query.", true, filter.IsNoResultQuery);
			AssertEquals("Sql Text should be equal.", "", filter.LiteralTextSqlFormatted);

			additionalInfoProvider.MapTransactionLineWithConsolCost(line.PK, consolCostPK);
			filter = PrimaryKeyMappingStrategy.GetRelatedConsolCostsFilter(line, additionalInfoProvider);
			AssertEquals("Should not be no result query.", false, filter.IsNoResultQuery);
			AssertEqualsIgnoreLineBreaks("Sql Text should be equal", $"E6_PK = '{consolCostPK.ToString()}'", filter.LiteralTextSqlFormatted);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		PrimaryKeyMappingStrategy PrimaryKeyMappingStrategy => primaryKeyMappingStrategy ?? (primaryKeyMappingStrategy = new PrimaryKeyMappingStrategy());
		PrimaryKeyMappingStrategy primaryKeyMappingStrategy;
	}
}
