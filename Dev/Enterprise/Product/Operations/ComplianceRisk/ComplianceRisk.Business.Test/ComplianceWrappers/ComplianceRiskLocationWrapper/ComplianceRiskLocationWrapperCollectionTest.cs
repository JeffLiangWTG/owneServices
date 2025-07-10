using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestedType(typeof(ComplianceRiskLocationWrapperCollection))]
	public class ComplianceRiskLocationWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ComplianceRiskLocationWrapperCollection>
	{
		public void TestAllowNew()
		{
			var result = new ComplianceRiskLocationWrapperCollection();
			AssertEquals(false, result.AllowNew);
		}

		protected override ComplianceRiskLocationWrapperCollection GetCollectionToTest()
		{
			return new ComplianceRiskLocationWrapperCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var country = RefCountry.LoadFromCountryCode(Factory, "AU");
			return new ComplianceRiskLocationWrapper(new ScreeningParty(country, "", country));
		}
	}
}
