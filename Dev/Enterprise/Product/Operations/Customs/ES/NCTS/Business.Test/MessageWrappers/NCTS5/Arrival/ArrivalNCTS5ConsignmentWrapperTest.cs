using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class ArrivalNCTS5ConsignmentWrapperTest : WrapperHelperTest<ArrivalNCTS5ConsignmentWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if header is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "header"), () => GetWrapper(null));

				AssertExceptionThrown("Constructor Throws Exception if ArrivalMovementHeader is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "ArrivalMovementHeader"), () => GetWrapper(Factory.New<NctsHeader>()));
			});
		}

		public void TestLocationOfGoods()
		{
			var locationOfGoods = wrapper.LocationOfGoods;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled locationOfGoods", locationOfGoods);
				AssertSame("Cached locationOfGoods", wrapper.LocationOfGoods, locationOfGoods);
			});
		}

		public void TestIncident()
		{
			var enRouteIncident = nctsHeader.EnRouteIncidents.AddNew();
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					wrapper = GetWrapper(nctsHeader);
					var incident = wrapper.Incident;
					AssertEquals("TransitionalPeriod: Expected filled incident", 1, incident.Count);
					AssertSame("Cached incident", wrapper.Incident, incident);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("FinalPeriod: Expected not filled incident", 0, wrapper.Incident.Count);
				}
			});
		}

		public void TestGetIncidentList()
		{
			var enRouteIncident0 = nctsHeader.EnRouteIncidents.AddNew().BN_IncidentCode = "A";
			var enRouteIncident1 = nctsHeader.EnRouteIncidents.AddNew().BN_IncidentCode = "B";
			var incidentList = wrapper.GetIncidentList(nctsHeader).ToList();
			CombineAssertions(() =>
			{
				AssertEquals("First incident have a BN_IncidentCode ", "A", incidentList[0].Code);
				AssertEquals("First incident have a Sequence ", "1", incidentList[0].SequenceNumber);
				AssertEquals("Second incident have a BN_IncidentCode ", "B", incidentList[1].Code);
				AssertEquals("Second incident have a Sequence ", "2", incidentList[1].SequenceNumber);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			wrapper = GetWrapper(nctsHeader);
		}

		ArrivalNCTS5ConsignmentWrapper wrapper;
		NctsHeader nctsHeader;

		ArrivalNCTS5ConsignmentWrapper GetWrapper(NctsHeader header) => new ArrivalNCTS5ConsignmentWrapper(header);

		protected override ArrivalNCTS5ConsignmentWrapper GetProvider() => wrapper;
	}
}
