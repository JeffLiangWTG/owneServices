using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsContainerCollection<EnRouteIncident>))]
	public class NctsContainerCollectionTest : ActiveBusinessObjectCollectionTestCase<NctsContainerCollection<EnRouteIncident>>
	{
		public virtual void TestMaster()
		{
			var collection = GetCollectionToTest();
			AssertType<EnRouteIncident>(collection.Master);
		}

		public void TestDefaultParentTableCode()
		{
			var collection = GetCollectionToTest();
			var container = collection.AddNew();
			AssertEquals(CusInBondEventSchema.Constants.Prefix, container.BC_ParentTableCode);
		}

		public void TestHasDuplicatesForSealNumberWithinSingleContainer()
		{
			var incident = CreatePhase5ArrivalEnRouteIncident();
			var container = incident.IncidentContainers.AddNew();
			var additionalSeal = container.Seals.AddNew();

			CombineAssertions(() =>
			{
				container.BC_Seal1 = "222";
				container.BC_Seal2 = "222";
				Assert("Seal1 duplicates Seal2", incident.IncidentContainers.HasDuplicatesForSealNumber("222"));

				container.BC_Seal1 = "111";
				additionalSeal.BK_SealNumber = "111";
				Assert("AdditionalSeal duplicates Seal1", incident.IncidentContainers.HasDuplicatesForSealNumber("111"));

				additionalSeal.BK_SealNumber = "222";
				Assert("AdditionalSeal duplicates Seal2", incident.IncidentContainers.HasDuplicatesForSealNumber("222"));

				additionalSeal.BK_SealNumber = "333";
				container.Seals.AddNew().BK_SealNumber = "333";
				Assert("AdditionalSeal duplicates another AdditionalSeal", incident.IncidentContainers.HasDuplicatesForSealNumber("333"));
			});
		}

		public void TestHasDuplicatesForSealNumberWithinMultipleContainers()
		{
			var incident = CreatePhase5ArrivalEnRouteIncident();
			var container1 = incident.IncidentContainers.AddNew();
			container1.BC_Seal1 = "111";
			container1.BC_Seal2 = "222";
			container1.Seals.AddNew().BK_SealNumber = "333";

			var container = incident.IncidentContainers.AddNew();

			CombineAssertions(() =>
			{
				container.BC_Seal1 = "111";
				Assert("Seal1 duplicates Seal1 from container1", incident.IncidentContainers.HasDuplicatesForSealNumber("111"));

				container.BC_Seal1 = "222";
				Assert("Seal1 duplicates Seal2 from container1", incident.IncidentContainers.HasDuplicatesForSealNumber("222"));

				container.BC_Seal1 = "333";
				Assert("Seal1 duplicates AdditionalSeal from container1", incident.IncidentContainers.HasDuplicatesForSealNumber("333"));

				container.BC_Seal1 = null;
				container.BC_Seal2 = "222";
				Assert("Seal2 duplicates Seal2 from container1", incident.IncidentContainers.HasDuplicatesForSealNumber("222"));

				container.BC_Seal2 = "333";
				Assert("Seal2 duplicates AdditionalSeal from container1", incident.IncidentContainers.HasDuplicatesForSealNumber("333"));

				container.BC_Seal2 = null;
				container.Seals.AddNew().BK_SealNumber = "333";
				Assert("AdditionalSeal duplicates AdditionalSeal from container1", incident.IncidentContainers.HasDuplicatesForSealNumber("333"));
			});
		}

		protected override NctsContainerCollection<EnRouteIncident> GetCollectionToTest()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var incident = header.EnRouteIncidents.AddNew();

			return new NctsContainerCollection<EnRouteIncident>(incident);
		}

		EnRouteIncident CreatePhase5ArrivalEnRouteIncident()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			return header.EnRouteIncidents.AddNew();
		}
	}
}
