using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class DisplaySequenceStrategyTest : TestCaseWithFactory
	{
		public void TestTryMapTransactionLineByDisplaySequence_InvalidParameter()
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
			AssertEquals("Pre-condition: the line should not map any Display Sequences.", null, additionalInfoProvider.GetApportionedChargeDisplaySequence(line.PK));
			AssertEquals("Pre-condition: the line Should not have OriginalJobCharge", null, line.OriginalJobCharge);

			additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);
			var universalLine = JobChargeMappingTestHelper.CreateUniversalLine(null, null, jobNumber: "S0001");
			var matchedCharges = DisplaySequenceMappingStrategy.TryMapTransactionLine(Factory, universalLine, line, additionalInfoProvider);
			CombineAssertions("Posting Journal has null Display Sequence.", () =>
			{
				AssertEquals(0, matchedCharges.Length);
				AssertEquals(null, additionalInfoProvider.GetApportionedChargeDisplaySequence(line.PK));
				AssertCollectedInfo(null);
			});

			additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);
			universalLine = JobChargeMappingTestHelper.CreateUniversalLine(null, "Invalid Display Sequence", jobNumber: "S0001");
			matchedCharges = DisplaySequenceMappingStrategy.TryMapTransactionLine(Factory, universalLine, line, additionalInfoProvider);
			CombineAssertions("Posting Journal has invalid Display Sequence.", () =>
			{
				AssertEquals(0, matchedCharges.Length);
				AssertEquals(null, additionalInfoProvider.GetApportionedChargeDisplaySequence(line.PK));
				AssertCollectedInfo(JobChargeMappingInfoType.InvalidKey);
			});

			additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);
			universalLine = JobChargeMappingTestHelper.CreateUniversalLine(null, "2");
			matchedCharges = DisplaySequenceMappingStrategy.TryMapTransactionLine(Factory, universalLine, line, additionalInfoProvider);
			CombineAssertions("Posting Journal has no Job number nor consol number.", () =>
			{
				AssertEquals(0, matchedCharges.Length);
				AssertEquals(null, additionalInfoProvider.GetApportionedChargeDisplaySequence(line.PK));
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

		public void TestTryMapTransactionLineByDisplaySequence_Charge()
		{
			var consol = TestObjectCreator.CreateConsol(consolNum: "C0001");
			var shipment = TestObjectCreator.CreateShipment("S0001", consol);
			var job = TestObjectCreator.CreateJob(shipment);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1);
			Factory.Save();

			var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "00001001", TestObjectCreator.AUD, 1m, 150m, 0m, 150m, 0m);
			var line = header.Lines[0];
			line.AL_AC = charge1.JR_AC;
			line.AL_JH = job.PK;
			var additionalInfoProvider = new TransactionImportAdditionalInfoProvider();

			AssertEquals("Pre-condition: the line should not map any Display Sequence.", null, additionalInfoProvider.GetApportionedChargeDisplaySequence(line.PK));
			AssertEquals("Pre-condition: the line Should not have OriginalJobCharge", null, line.OriginalJobCharge);

			var universalLine = JobChargeMappingTestHelper.CreateUniversalLine(null, "1", jobNumber: "S0001");

			charge1.JR_DisplaySequence = 3;
			var matchedCharges = DisplaySequenceMappingStrategy.TryMapTransactionLine(Factory, universalLine, line, additionalInfoProvider);
			AssertEquals("Should not match if there is no target Display Sequence", 0, matchedCharges.Length);
			AssertEquals(null, additionalInfoProvider.GetApportionedChargeDisplaySequence(line.PK));

			charge1.JR_DisplaySequence = 1;
			matchedCharges = DisplaySequenceMappingStrategy.TryMapTransactionLine(Factory, universalLine, line, additionalInfoProvider);
			AssertEquals("Should match a shipment charge when posting journal has Job number but has no consol number.", 1, matchedCharges.Length);
			AssertEquals(null, additionalInfoProvider.GetApportionedChargeDisplaySequence(line.PK));

			charge1.JR_DisplaySequence = 1;
			charge2.JR_DisplaySequence = 1;
			matchedCharges = DisplaySequenceMappingStrategy.TryMapTransactionLine(Factory, universalLine, line, additionalInfoProvider);
			AssertEquals("Should have two matched shipment-level charges.", 2, matchedCharges.Length);
			AssertEquals(null, additionalInfoProvider.GetApportionedChargeDisplaySequence(line.PK));

			charge2.JR_DisplaySequence = 2;
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100m);
			var apportionedCharge = consolCost.ApportionmentCharges.Where(x => x.JR_JH == job.PK).Single();
			apportionedCharge.JR_DisplaySequence = 1;
			matchedCharges = DisplaySequenceMappingStrategy.TryMapTransactionLine(Factory, universalLine, line, additionalInfoProvider);
			AssertEquals("Should only match a shipment-level charge because the display sequences of shipment-level charges and apportioned charges do not interfere with each other.", 1, matchedCharges.Length);
			AssertEquals(null, additionalInfoProvider.GetApportionedChargeDisplaySequence(line.PK));
		}

		public void TestTryMapTransactionLineByDisplaySequence_ApportionedCharge()
		{
			var consol = TestObjectCreator.CreateConsol(consolNum: "C0001");
			var shipment = TestObjectCreator.CreateShipment("S0001", consol);
			var job = TestObjectCreator.CreateJob(shipment);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100m);
			Factory.Save();

			var apportionedCharge = consolCost.ApportionmentCharges.Where(x => x.JR_JH == job.PK).Single();
			var universalLine = JobChargeMappingTestHelper.CreateUniversalLine(null, "1", jobNumber: "S0001", consolNumber: "C0001");
			var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "00001001", TestObjectCreator.AUD, 1m, 150m, 0m, 150m, 0m);
			var line = header.Lines[0];
			line.AL_AC = apportionedCharge.JR_AC;
			line.AL_JH = job.PK;
			var additionalInfoProvider = new TransactionImportAdditionalInfoProvider();

			AssertEquals("Pre-condition: the line should not map any Display Sequence.", null, additionalInfoProvider.GetApportionedChargeDisplaySequence(line.PK));
			AssertEquals("Pre-condition: the line Should not have OriginalJobCharge", null, line.OriginalJobCharge);

			var matchedCharges = DisplaySequenceMappingStrategy.TryMapTransactionLine(Factory, universalLine, line, additionalInfoProvider);
			AssertEquals("Should not match to a shipment-level charge", 0, matchedCharges.Length);
			AssertEquals("Should map to a display sequence when posting journal has both Job number and consol number.", new ZShort(1), additionalInfoProvider.GetApportionedChargeDisplaySequence(line.PK));
		}

		public void TestGetRelatedApportionedChargesFilter()
		{
			var consol = TestObjectCreator.CreateConsol(consolNum: "C0001");
			var shipment = TestObjectCreator.CreateShipment("S0001", consol);
			var job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "00001001", TestObjectCreator.AUD, 1m, 150m, 0m, 150m, 0m);
			var line = header.Lines[0];
			line.AL_JH = job.PK;
			var additionalInfoProvider = new TransactionImportAdditionalInfoProvider();

			var filter = DisplaySequenceMappingStrategy.GetRelatedApportionedChargesFilter(line, additionalInfoProvider);
			AssertEquals("Should be no result query.", true, filter.IsNoResultQuery);
			AssertEquals("Sql Text should be equal.", "", filter.LiteralTextSqlFormatted);

			additionalInfoProvider.MapTransactionLineWithApportionedChargeDisplaySequence(line.PK, new ZShort(1));
			filter = DisplaySequenceMappingStrategy.GetRelatedApportionedChargesFilter(line, additionalInfoProvider);
			AssertEquals("Should not be no result query.", false, filter.IsNoResultQuery);
			var expectedSqlText = $@"JR_JH = '{job.PK.ToString()}' ANDJR_DisplaySequence = 1";
			AssertEqualsIgnoreLineBreaks("Sql Text should be equal", expectedSqlText, filter.LiteralTextSqlFormatted);
		}

		public void TestGetRelatedConsolCostsFilter()
		{
			var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "00001001", TestObjectCreator.AUD, 1m, 150m, 0m, 150m, 0m);
			var line = header.Lines[0];
			var additionalInfoProvider = new TransactionImportAdditionalInfoProvider();
			var consolCostPK = ZGuid.NewZGuid();

			var filter = DisplaySequenceMappingStrategy.GetRelatedConsolCostsFilter(line, additionalInfoProvider);
			AssertEquals("Should be no result query.", true, filter.IsNoResultQuery);
			AssertEquals("Sql Text should be equal.", "", filter.LiteralTextSqlFormatted);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		DisplaySequenceMappingStrategy DisplaySequenceMappingStrategy => displaySequenceMappingStrategy ?? (displaySequenceMappingStrategy = new DisplaySequenceMappingStrategy());
		DisplaySequenceMappingStrategy displaySequenceMappingStrategy;
	}
}
