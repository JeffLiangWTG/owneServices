using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.WIPAccrual;

namespace Enterprise.Accounting.Business.Testing.Base.Transaction.Testing
{
	class ModifiedWIPAccrualRelatedChargeFetchHintServiceTest : TestCaseWithFactory
	{
		[StressTest]
		public void TestPreFetchRelatedChargesWithLargeNumberOfLines()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var shipment = testObjectCreator.CreateShipment("S00001", true);
			var job = testObjectCreator.CreateJob(shipment);
			Factory.Save();

			var linePKList = new List<ZGuid>();

			for (int i = 0; i < 2000; i++)
			{
				var charge = testObjectCreator.CreateCharge(job, testObjectCreator.FRT, 100m + i, 0m);
				var line = testObjectCreator.CreateWIP(charge);
				linePKList.Add(line.PK);
			}
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			newFactory.RefreshEnabled = false;
			TransactionLine targetLine = null;

			foreach (var pk in linePKList)
			{
				var lineReload = newFactory.Load<Accrual>(pk);
				lineReload.AL_Desc += "TST";

				if (targetLine == null)
				{
					targetLine = lineReload;
				}
			}

			var jobReload = newFactory.Load<Job>(job.PK);

			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			using (TestConnection.TrackExecutedCommands())
			{
				var relatedCharge = targetLine.RelatedJobCharge;    // Create ModifiedWIPAccrualRelatedChargeFetchHintService
				var jobCharges = jobReload.Charges;                 // Run FetchHint load

				var actualCommands = TestConnection.ExecutedCommands.Where(x => x.ToUpperInvariant().Contains(@"FROM dbo.JobCharge
	WHERE (JR_AL_ARLine in (@CWO1_, @CWO2_".ToUpperInvariant()));

				AssertEquals("Large fetch hint query should be split into multiple smaller ones.", 32, actualCommands.Count());
				AssertLessThan("Each small fetch hint query should has limited number of elements, linking with OR operator.", actualCommands.Count(x => x == ","), 64);
			}
		}
	}
}
