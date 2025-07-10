using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.APAutomation.APReconciliation;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using WiseRates.Tools;

namespace Enterprise.Accounting.Business.Testing.APReconciliation
{
	[TestedType(typeof(ReconciliationLineConverter))]
	public class ReconciliationLineConverterTest : TestCaseWithFactory
	{
		public void TestDependencyInjection()
		{
			AssertNotNull(ReconciliationLineConverter);
			AssertType<ReconciliationLineConverter>(ReconciliationLineConverter);
		}

		public void TestImportChargeToInvoice()
		{
			var pureJobCharges = new List<Charge>();
			pureJobCharges.AddRange(CreateCharges("S00001000", TestObjectCreator.CC1, TestObjectCreator.CC2));
			pureJobCharges.AddRange(CreateCharges("S00001001", TestObjectCreator.CC3, TestObjectCreator.CC4));

			var consol = TestObjectCreator.CreateConsolWithShipmentJobs("C0001"
				, shipmentJobNums: new[] { "S00001002", "S00001003" }
				, shipmentInvoker: (shipment) => TestObjectCreator.CreateJob(shipment)
			);
			Factory.Save();

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, null, 1000, true, "MAN");
			AssertEquals("PreCondition", 2, consolCost.ApportionmentCharges.Count);

			var reconciliationLines = new List<APReconciliationLine>();
			reconciliationLines.AddRange(CreateAPReconciliationLines(APReconciliationLineTypes.JobCharge
				, pureJobCharges.Select(x => x.PK).ToArray())
			);
			reconciliationLines.AddRange(CreateAPReconciliationLines(APReconciliationLineTypes.ConsolCost
				, consolCost.PK)
			);
			reconciliationLines.AddRange(CreateAPReconciliationLines(APReconciliationLineTypes.JobCharge
				, new ZGuid("16B0DE6F-DF57-477D-86D7-67614C934492")
				, new ZGuid("FC174341-4292-444E-A4C8-450A563647C4")
				, new ZGuid("4C100F2C-A0BA-4903-996D-B26BB2D92239")
				, new ZGuid("15E0E997-4641-40C3-A257-099CB79BD86C"))
			);
			reconciliationLines.AddRange(CreateAPReconciliationLines(APReconciliationLineTypes.JobCharge
				, consolCost.ApportionmentCharges.Select(x => x.PK).ToArray())
			);

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m);
			AssertImportChargeToInvoice(
				"We only pass pure JobCharge and ConsolCost to Impoter, not ghost data nor apportioned charge."
				, invoice
				, reconciliationLines
				, pureJobCharges
				, new[] { consolCost });
		}

		APReconciliationLine[] CreateAPReconciliationLines(APReconciliationLineTypes type, params ZGuid[] bizoPKs)
		{
			return bizoPKs.Select(pk => new APReconciliationLine
			{
				LineIdentifier = pk,
				LineType = type
			}).ToArray();
		}

		Charge[] CreateCharges(string shipmentNum, params AccChargeCode[] chargeCodes)
		{
			var shipment = TestObjectCreator.CreateShipment(shipmentNum);
			var job = TestObjectCreator.CreateJob(shipment);
			return chargeCodes.Select(chargeCode
				=> TestObjectCreator.CreateCharge(job, chargeCode, null, TestObjectCreator.AUD, 100m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 200m, null)
			).ToArray();
		}

		void AssertImportChargeToInvoice(string comment, InvoicingBase passingInInvoice
			, IEnumerable<APReconciliationLine> passingInReconciliationLines
			, IEnumerable<Charge> expectedPassingInCharges
			, IEnumerable<JobConsolCost> expectedPassingInConsolCost)
		{
			var mockIInvoicingBaseLineImporter = CreateMockInvoicingBaseLineImporter(passingInInvoice, expectedPassingInCharges, expectedPassingInConsolCost);
			using (ObjectFactory.Substitute<IInvoicingBaseLineImporter>(mockIInvoicingBaseLineImporter.Object))
			{
				AssertNoExceptionThrown(comment, () => ReconciliationLineConverter.ImportReconciliationLinesToInvoice(passingInInvoice, passingInReconciliationLines));
			}
			mockIInvoicingBaseLineImporter.Verify(
				x => x.ImportLinesFromChargeCollection(It.IsAny<InvoicingBase>(), It.IsAny<IEnumerable<Charge>>())
				, Times.Exactly(1)
			);
			mockIInvoicingBaseLineImporter.Verify(
				x => x.ImportLinesFromConsolCostCollection(It.IsAny<IInvoicingBaseImporterTarget>(), It.IsAny<IEnumerable<JobConsolCost>>())
				, Times.Exactly(1)
			);
		}

		Mock<IInvoicingBaseLineImporter> CreateMockInvoicingBaseLineImporter(InvoicingBase expectedPassingInInvoice
			, IEnumerable<Charge> expectedPassingInCharges
			, IEnumerable<JobConsolCost> expectedPassingInConsolCost)
		{
			var mockIInvoicingBaseLineImporter = new Mock<IInvoicingBaseLineImporter>();
			mockIInvoicingBaseLineImporter.Setup(
				x => x.ImportLinesFromChargeCollection(
					expectedPassingInInvoice
					, It.Is<Charge[]>(charges => charges.SequenceEqualIgnoringOrder(expectedPassingInCharges, null, true))
				));
			mockIInvoicingBaseLineImporter.Setup(
				x => x.ImportLinesFromConsolCostCollection(
					expectedPassingInInvoice
					, It.Is<JobConsolCost[]>(consolCosts => consolCosts.SequenceEqualIgnoringOrder(expectedPassingInConsolCost, null, true))
				));
			return mockIInvoicingBaseLineImporter;
		}

		IReconciliationLineConverter ReconciliationLineConverter => reconciliationLineConverter ??= ObjectFactory.Get<IReconciliationLineConverter>();
		IReconciliationLineConverter reconciliationLineConverter;

		TestObjectCreator TestObjectCreator => testObjectCreator ??= new TestObjectCreator(Factory);
		TestObjectCreator testObjectCreator;
	}
}
