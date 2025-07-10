using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.DataMapping.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(FilteredChargeCollection))]
	public class FilteredChargeCollectionTest : BusinessObjectCollectionViewTestCase<FilteredChargeCollection>
	{
		#region TestIsThisPartOfTheCollection

		public void TestAllFiltersFunctionTogether()
		{
			var creator = new TestObjectCreator(Factory);

			var setup = creator.CreateGatewayConsolsAndShipments();
			using (var job = creator.CreateJob(setup.gC0002))
			{
				var chargeCollection = new ChargeCollection(job);
				var filteredCollection = new FilteredChargeCollection(chargeCollection);

				var chargeWithCostReference = chargeCollection.AddNew();
				chargeWithCostReference.JR_CostReference = "ABC";

				var chargeWithRelatedJob = chargeCollection.AddNew();
				chargeWithRelatedJob.JR_Calc_RelatedJobNumber = setup.s0001.JobNumber;

				var chargeWithCostReferenceAndRelatedJob = chargeCollection.AddNew();
				chargeWithCostReferenceAndRelatedJob.JR_CostReference = "ABC";
				chargeWithCostReferenceAndRelatedJob.JR_Calc_RelatedJobNumber = setup.s0001.JobNumber;

				filteredCollection.RelatedJobFilter = new HashSet<ZString> { setup.s0001.JobNumber };
				filteredCollection.EnableCostReferenceFilter("ABC");
				AssertContainsExactElementsInAnyOrder("Should contain only the charge matching both filters", new[] { chargeWithCostReferenceAndRelatedJob }, filteredCollection);
			}
		}

		public void TestRelatedJobFilter()
		{
			var creator = new TestObjectCreator(Factory);

			var setup = creator.CreateGatewayConsolsAndShipments();
			using (var job = creator.CreateJob(setup.gC0002))
			{
				var chargeCollection = new ChargeCollection(job);
				var filteredCollection = new FilteredChargeCollection(chargeCollection);
				var chargeWithEmptyRelatedJob = chargeCollection.AddNew();
				var chargeWithShipment1RelatedJob = chargeCollection.AddNew();
				chargeWithShipment1RelatedJob.JR_Calc_RelatedJobNumber = setup.s0001.JobNumber;
				var chargeWithShipment2RelatedJob = chargeCollection.AddNew();
				chargeWithShipment2RelatedJob.JR_Calc_RelatedJobNumber = setup.s0002.JobNumber;

				AssertContainsExactElementsInAnyOrder("Should contain the 3 new charges.", new[] { chargeWithEmptyRelatedJob, chargeWithShipment1RelatedJob, chargeWithShipment2RelatedJob }, filteredCollection);

				filteredCollection.RelatedJobFilter = new HashSet<ZString> { setup.s0001.JobNumber };
				AssertContainsExactElementsInAnyOrder("Should contain the the shipment 1 charge only.", new[] { chargeWithShipment1RelatedJob }, filteredCollection);

				filteredCollection.RelatedJobFilter = new HashSet<ZString> { setup.s0001.JobNumber, setup.s0002.JobNumber };
				AssertContainsExactElementsInAnyOrder("Should contain shipment 1 & 2 charges only.", new[] { chargeWithShipment1RelatedJob, chargeWithShipment2RelatedJob }, filteredCollection);

				filteredCollection.RelatedJobFilter = new HashSet<ZString>();
				AssertContainsExactElementsInAnyOrder("Should contain all charges because there is no filter.", new[] { chargeWithEmptyRelatedJob, chargeWithShipment1RelatedJob, chargeWithShipment2RelatedJob }, filteredCollection);
			}
		}

		public void TestCostReferenceFilter()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var chargeCollection = new ChargeCollection(job);
			var filteredCollection = new FilteredChargeCollection(chargeCollection);
			AssertEquals("Inner Charges Collection does not container any charges.", 0, filteredCollection.Count);

			var chargeNone = chargeCollection.AddNew();
			var chargeABC = chargeCollection.AddNew();
			var chargeDEF = chargeCollection.AddNew();
			chargeABC.JR_CostReference = "ABC";
			chargeDEF.JR_CostReference = "DEF";
			AssertContainsExactElementsInAnyOrder("Should contain the 3 new charges.", new[] { chargeNone, chargeABC, chargeDEF }, filteredCollection);

			filteredCollection.EnableCostReferenceFilter("ABC");
			AssertContainsExactElementsInAnyOrder("Should contain the the ABC charge only.", new[] { chargeABC }, filteredCollection);

			filteredCollection.EnableCostReferenceFilter("DEF");
			AssertContainsExactElementsInAnyOrder("Should contain the the DEF charge only.", new[] { chargeDEF }, filteredCollection);

			filteredCollection.DisableCostReferenceFilter();
			AssertContainsExactElementsInAnyOrder("Should contain all charges because there is no filter.", new[] { chargeNone, chargeABC, chargeDEF }, filteredCollection);
		}

		public void TestDataImportWizardInvokeUpdateTotalOncePerTotalProvider()
		{
			var totalProviderMock1 = new Mock<ITotalProvider>();
			var totalProviderMock2 = new Mock<ITotalProvider>();
			totalProviderMock1.Setup(tp => tp.UpdateTotals());
			totalProviderMock2.Setup(tp => tp.UpdateTotals());

			var row = 0;
			var getTotalProvider = new Func<ITotalProvider>(() =>
							{
								row++;
								return row % 2 == 0 ? totalProviderMock1.Object : totalProviderMock2.Object;
							});

			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var chargeCollection = new ChargeCollection(job);
			foreach (var filteredCollection in new[] {
														new FilteredChargeCollection(chargeCollection),
														new FilteredChargeCollection(new FilteredChargeCollectionView(chargeCollection))
													 })
			{
				var wizard = new ImportWizardForCharge_TestOnly(new ImportCollectionInfoImpl(filteredCollection), null, new FileMapperForTest(), getTotalProvider);
				wizard.ImportIntoCollection(filteredCollection, 100);
				AssertEquals("Item Count", 100, filteredCollection.Count);
				AssertContainsExactElementsInAnyOrder(new[] { totalProviderMock1.Object, totalProviderMock2.Object }, chargeCollection.OfType<BaseCharge>().Select(c => c.TotalProvider).Distinct());
				chargeCollection.RemoveAll();
			}
			totalProviderMock1.Verify(tp => tp.UpdateTotals(), Times.Exactly(2)); // for each collection once
			totalProviderMock2.Verify(tp => tp.UpdateTotals(), Times.Exactly(2)); // for each collection once
		}

		#endregion

		#region Implementation

		protected override FilteredChargeCollection GetCollectionToTest()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var chargeCollection = new ChargeCollection(job);
			return new FilteredChargeCollection(chargeCollection);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<Charge>();
		}

		class ImportWizardForCharge_TestOnly : ImportWizard
		{
			public ImportWizardForCharge_TestOnly(IImportCollectionInfo collectionInfo, ISettingsStorage settingsStorage, IFileMapper fileMapper, Func<ITotalProvider> getTotalProvider)
						: base(collectionInfo, settingsStorage, fileMapper)
			{
				GetTotalProvider = getTotalProvider;
			}
			Func<ITotalProvider> GetTotalProvider { get; }

			public override List<string[]> LoadFile(int startingRow, int maximumRows, bool forceFileLoad = false)
			{
				var rows = new List<string[]>();
				for (var i = startingRow; i <= maximumRows; i++)
				{
					rows.Add(new[] { string.Empty });
				}
				return rows;
			}

			protected override void ImportIntoBizObjCore(Action<BusinessObject, string, object> setValue, IEnumerable<ImportWizardMapping> mappedRecords, BusinessObject bizObj, string[] values)
			{
				var charge = bizObj as BaseCharge;
				charge.SubstituteTotalProvider_TestOnly(GetTotalProvider.Invoke());
				charge.JR_OSCostAmt = 25;
			}
		}

		#endregion
	}
}
