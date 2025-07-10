using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(ProfitLossSummaryCollectionView))]
	public class ProfitLossSummaryCollectionViewTest : NonPersistentBusinessObjectCollectionTestCase<ProfitLossSummaryCollectionView>
	{
		protected override ProfitLossSummaryCollectionView GetCollectionToTest()
		{
			var consol = Factory.New<ForwardingConsol>();
			var ship1 = (CommonShipment)consol.Shipments.AddNew();
			var jobProfiltLoss = new JobProfitLoss(Factory);
			var profitLossSummaryCollection = new ProfitLossSummaryCollection(jobProfiltLoss, consol);
			return new ProfitLossSummaryCollectionView(profitLossSummaryCollection, consol);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var row = new DataTable().Rows.Add(System.Array.Empty<object>());
			var profitLossSummary = new ProfitLossSummaryDetail(Factory, row);
			return new ProfitLossSummaryDetailView(profitLossSummary);
		}
	}
}
