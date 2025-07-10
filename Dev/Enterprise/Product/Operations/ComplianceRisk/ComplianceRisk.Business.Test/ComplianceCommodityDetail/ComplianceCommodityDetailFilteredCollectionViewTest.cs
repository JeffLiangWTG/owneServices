using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.ComplianceRisk.Business.Test.ComplianceCommodityDetailCollectionTest;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestedType(typeof(ComplianceCommodityDetailFilteredCollectionView))]
	public class ComplianceCommodityDetailFilteredCollectionViewTest : BusinessObjectCollectionViewTestCase<ComplianceCommodityDetailFilteredCollectionView>
	{
		protected override ComplianceCommodityDetailFilteredCollectionView GetCollectionToTest()
		{
			return new ComplianceCommodityDetailFilteredCollectionView(new ComplianceCommodityDetailCollection(Factory.New<ComplianceRiskStatus>(), Factory.New<ShipmentWithProvider>()));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<ComplianceCommodityDetail>();
		}

		public void TestFilter()
		{
			var collectionView = new ComplianceCommodityDetailFilteredCollectionView(new ComplianceCommodityDetailCollection(Factory.New<ComplianceRiskStatus>(), Factory.New<ShipmentWithProvider>()));
			var commodity1 = collectionView.AddNew();
			var commodity2 = collectionView.AddNew();

			commodity1.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;
			commodity2.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Released;

			collectionView.Filter = new ZQuery(ComplianceCommodityDetailSchema.CCD_RiskStatus, ComplianceRiskStatusCodeList.Codes.Blocked);
			collectionView.Rebuild();
			AssertEquals("When AssessmentInitialized has not started and filters include RiskStatus, no collection is displayed", 0, collectionView.Count);

			var eventLog = Factory.NewWithValidTestData<StmComplianceEvent>();
			eventLog.SCE_EventType = AutoEvents.ComplianceRiskInteractionCode;
			eventLog.SCE_EventSubType = ComplianceEventList.Codes.AssessmentInitialized;
			eventLog.SCE_ParentID = commodity1.ComplianceRiskStatus.COR_ParentID;

			collectionView.Rebuild();
			AssertEquals("Only contains first commodity", commodity1.PK, collectionView.Single().PK);
		}
	}
}
