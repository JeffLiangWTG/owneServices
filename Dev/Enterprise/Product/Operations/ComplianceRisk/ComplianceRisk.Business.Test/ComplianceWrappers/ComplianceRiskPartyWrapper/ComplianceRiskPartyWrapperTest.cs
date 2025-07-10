using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestedType(typeof(ComplianceRiskPartyWrapper))]
	public class ComplianceRiskPartyWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var org = Factory.New<OrgHeader>();
			var screeningParty = new ScreeningParty(org, "", org);
			return new ComplianceRiskPartyWrapper(screeningParty);
		}

		public void TestParty()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_ScreeningStatus = "MAT";
			var party = new ScreeningParty(shipment, "consignee", org);
			var wrapper = new ComplianceRiskPartyWrapper(party);
			CombineAssertions(() =>
			{
				AssertEquals(org.OH_Code, wrapper.OrgCode);
				AssertEquals("MAT", wrapper.ScreeningStatus);
				AssertEquals(org.OH_FullName, wrapper.Code);
				AssertEquals("Matched", wrapper.ScreeningStatusDescription);
				AssertEquals((shipment as IForwardingShipment).JS_UniqueConsignRef + ": consignee", wrapper.ParentsDescription);
			});

			org.OH_ScreeningStatus = "CLR";
			party = new ScreeningParty(org, "party", org);
			wrapper = new ComplianceRiskPartyWrapper(party);
			CombineAssertions(() =>
			{
				AssertEquals(org.OH_Code, wrapper.OrgCode);
				AssertEquals("CLR", wrapper.ScreeningStatus);
				AssertEquals(org.OH_FullName, wrapper.Code);
				AssertEquals("Clear", wrapper.ScreeningStatusDescription);
				AssertEquals(org.OH_Code + ": party", wrapper.ParentsDescription);
			});
		}
	}
}
