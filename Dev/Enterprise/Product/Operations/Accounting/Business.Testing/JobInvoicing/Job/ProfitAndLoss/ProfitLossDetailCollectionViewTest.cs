using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(ProfitLossDetailCollectionView))]
	public class ProfitLossDetailCollectionViewTest : NonPersistentBusinessObjectCollectionTestCase<ProfitLossDetailCollectionView>
	{
		protected override ProfitLossDetailCollectionView GetCollectionToTest()
		{
			var consol = Factory.New<ForwardingConsol>();
			var ship1 = (CommonShipment)consol.Shipments.AddNew();
			var jobProfiltLoss = new JobProfitLoss(Factory);
			var profitLossCollection = new ProfitLossCollection(jobProfiltLoss, consol);
			return new ProfitLossDetailCollectionView(profitLossCollection, consol);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var row = new DataTable().Rows.Add(System.Array.Empty<object>());
			var profitLoss = new ProfitLossDetail(Factory, row);
			return new ProfitLossDetailView(profitLoss);
		}
	}
}
