using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(RatesPrioritiesCollection))]
	sealed class RatesPrioritiesCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<RatesPrioritiesCollection>
	{
		public void TestGatewayCollectPrioritiesDefault()
		{
			RatesPrioritiesCollection defaultCollection = RatesPrioritiesCollection.GetDefault(RatingDataRegistry.Instance.GatewayCollectPriorities.Name);

			AssertEquals(2, defaultCollection.Count);

			int i = 0;
			AssertOrganizationTypesRatePriorities(RatingDebtorOrgTypes.RAG, defaultCollection[i++]);
			AssertOrganizationTypesRatePriorities(RatingDebtorOrgTypes.CNE, defaultCollection[i++]);
		}

		public void TestGatewayPrepaidPrioritiesDefault()
		{
			RatesPrioritiesCollection defaultCollection = RatesPrioritiesCollection.GetDefault(RatingDataRegistry.Instance.GatewayPrepaidPriorities.Name);

			AssertEquals(2, defaultCollection.Count);

			int i = 0;
			AssertOrganizationTypesRatePriorities(RatingDebtorOrgTypes.SAG, defaultCollection[i++]);
			AssertOrganizationTypesRatePriorities(RatingDebtorOrgTypes.CNR, defaultCollection[i++]);
		}

		public void TestImportPrepaidPrioritiesDefault()
		{
			RatesPrioritiesCollection defaultCollection = RatesPrioritiesCollection.GetDefault(RatingDataRegistry.Instance.ImportPrepaidPriorities.Name);

			AssertEquals(2, defaultCollection.Count);

			int i = 0;
			AssertOrganizationTypesRatePriorities(RatingDebtorOrgTypes.AG, defaultCollection[i++]);
			AssertOrganizationTypesRatePriorities(RatingDebtorOrgTypes.CNR, defaultCollection[i++]);
		}

		public void TestExportPrepaidPrioritiesDefault()
		{
			RatesPrioritiesCollection defaultCollection = RatesPrioritiesCollection.GetDefault(RatingDataRegistry.Instance.ExportPrepaidPriorities.Name);

			AssertEquals(3, defaultCollection.Count);

			int i = 0;
			AssertOrganizationTypesRatePriorities(RatingDebtorOrgTypes.LC, defaultCollection[i++]);
			AssertOrganizationTypesRatePriorities(RatingDebtorOrgTypes.CNR, defaultCollection[i++]);
			AssertOrganizationTypesRatePriorities(RatingDebtorOrgTypes.LCBK, defaultCollection[i++]);
		}

		public void TestCrossTradePrepaidPrioritiesDefault()
		{
			RatesPrioritiesCollection defaultCollection = RatesPrioritiesCollection.GetDefault(RatingDataRegistry.Instance.CrossTradePrepaidPriorities.Name);

			AssertEquals(4, defaultCollection.Count);

			int i = 0;
			AssertOrganizationTypesRatePriorities(RatingDebtorOrgTypes.LC, defaultCollection[i++]);
			AssertOrganizationTypesRatePriorities(RatingDebtorOrgTypes.AG, defaultCollection[i++]);
			AssertOrganizationTypesRatePriorities(RatingDebtorOrgTypes.LCBK, defaultCollection[i++]);
			AssertOrganizationTypesRatePriorities(RatingDebtorOrgTypes.CNR, defaultCollection[i++]);
		}

		public void TestDomesticPrepaidPrioritiesDefault()
		{
			RatesPrioritiesCollection defaultCollection = RatesPrioritiesCollection.GetDefault(RatingDataRegistry.Instance.DomesticPrepaidPriorities.Name);

			AssertEquals(3, defaultCollection.Count);

			int i = 0;
			AssertOrganizationTypesRatePriorities(RatingDebtorOrgTypes.CNR, defaultCollection[i++]);
			AssertOrganizationTypesRatePriorities(RatingDebtorOrgTypes.LC, defaultCollection[i++]);
			AssertOrganizationTypesRatePriorities(RatingDebtorOrgTypes.AG, defaultCollection[i++]);
		}

		public void TestImportCollectPrioritiesDefault()
		{
			RatesPrioritiesCollection defaultCollection = RatesPrioritiesCollection.GetDefault(RatingDataRegistry.Instance.ImportCollectPriorities.Name);

			AssertEquals(3, defaultCollection.Count);

			int i = 0;
			AssertOrganizationTypesRatePriorities(RatingDebtorOrgTypes.LC, defaultCollection[i++]);
			AssertOrganizationTypesRatePriorities(RatingDebtorOrgTypes.CNE, defaultCollection[i++]);
			AssertOrganizationTypesRatePriorities(RatingDebtorOrgTypes.LCBK, defaultCollection[i++]);
		}

		public void TestExportCollectPrioritiesDefault()
		{
			RatesPrioritiesCollection defaultCollection = RatesPrioritiesCollection.GetDefault(RatingDataRegistry.Instance.ExportCollectPriorities.Name);

			AssertEquals(2, defaultCollection.Count);

			int i = 0;
			AssertOrganizationTypesRatePriorities(RatingDebtorOrgTypes.AG, defaultCollection[i++]);
			AssertOrganizationTypesRatePriorities(RatingDebtorOrgTypes.CNE, defaultCollection[i++]);
		}

		public void TestCrossTradeCollectPrioritiesDefault()
		{
			RatesPrioritiesCollection defaultCollection = RatesPrioritiesCollection.GetDefault(RatingDataRegistry.Instance.CrossTradeCollectPriorities.Name);

			AssertEquals(4, defaultCollection.Count);

			int i = 0;
			AssertOrganizationTypesRatePriorities(RatingDebtorOrgTypes.LC, defaultCollection[i++]);
			AssertOrganizationTypesRatePriorities(RatingDebtorOrgTypes.AG, defaultCollection[i++]);
			AssertOrganizationTypesRatePriorities(RatingDebtorOrgTypes.LCBK, defaultCollection[i++]);
			AssertOrganizationTypesRatePriorities(RatingDebtorOrgTypes.CNE, defaultCollection[i++]);
		}

		public void TestDomesticCollectPrioritiesDefault()
		{
			RatesPrioritiesCollection defaultCollection = RatesPrioritiesCollection.GetDefault(RatingDataRegistry.Instance.DomesticCollectPriorities.Name);

			AssertEquals(3, defaultCollection.Count);

			int i = 0;
			AssertOrganizationTypesRatePriorities(RatingDebtorOrgTypes.CNE, defaultCollection[i++]);
			AssertOrganizationTypesRatePriorities(RatingDebtorOrgTypes.LC, defaultCollection[i++]);
			AssertOrganizationTypesRatePriorities(RatingDebtorOrgTypes.AG, defaultCollection[i++]);
		}

		void AssertOrganizationTypesRatePriorities(RatingDebtorOrgTypes expected, RatesPriorities actual)
		{
			AssertEquals(expected, Enum.Parse(typeof(RatingDebtorOrgTypes), actual.OrganizationType));
		}

		#region Implementation

		protected override RatesPrioritiesCollection GetCollectionToTest()
		{
			return new RatesPrioritiesCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new RatesPriorities();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
