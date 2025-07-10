using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestedType(typeof(ComplianceRiskLocationWrapper))]
	public class ComplianceRiskLocationWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var country = RefCountry.LoadFromCountryCode(Factory, "AU");
			var screeningParty = new ScreeningParty(country, "", country);
			return new ComplianceRiskLocationWrapper(screeningParty);
		}

		public void TestCountry()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU"));
			country.RN_IsSanctioned = true;
			var location = new ScreeningParty(shipment, "origin", country);
			var wrapper = new ComplianceRiskLocationWrapper(location);
			CombineAssertions(() =>
			{
				AssertEquals("AU", wrapper.Location);
				AssertEquals("Australia", wrapper.LocationDescription);
				AssertEquals("Blocked", wrapper.RiskStatus);
				AssertEquals((shipment as IForwardingShipment).JS_UniqueConsignRef + ": origin", wrapper.Description);
			});

			var consol = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingConsol>());
			country.RN_IsSanctioned = false;
			location = new ScreeningParty(consol, "destination", country);
			wrapper = new ComplianceRiskLocationWrapper(location);
			CombineAssertions(() =>
			{
				AssertEquals("AU", wrapper.Location);
				AssertEquals("Australia", wrapper.LocationDescription);
				AssertEquals("Clear", wrapper.RiskStatus);
				AssertEquals((consol as IForwardingConsol).JK_UniqueConsignRef + ": destination", wrapper.Description);
			});
		}
	}
}
