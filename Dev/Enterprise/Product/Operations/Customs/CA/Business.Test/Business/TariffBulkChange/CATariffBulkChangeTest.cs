using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CAHTSTariffBulkChange))]
	sealed class CATariffBulkChangeTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new CAHTSTariffBulkChange(Factory);
		}

		public void TestCountrySpecificOverrides()
		{
			var topLevelObject = new CAHTSTariffBulkChange(Factory);
			AssertEquals("ReferenceKey", "HS2022 HS IMP", topLevelObject.ReferenceKey);
			AssertEquals("CountryPK", Core.Constants.CountryGuids.Canada, topLevelObject.CountryPK);
			AssertEquals("CountryCode", Enterprise.Core.Constants.CountryCodes.Canada, topLevelObject.CountryCode);
		}

		public void TestOverridenProperties()
		{
			var topLevelObject = new CAHTSTariffBulkChange(Factory);
			AssertEquals("The HS2022 concordance is not available yet. Save is not allowed.", topLevelObject.SaveNotAllowedMessage);
			AssertEquals(@"Your Data Base will now be updated with Pending Tariff Changes.
NOTE: If you are applying pending changes for the HS2022 change then you should only apply these changes on, or after, the 1st January 2022.
Do you wish to continue?", topLevelObject.ApplyTariffChangesMessage);
			AssertEquals(true, topLevelObject.ReferenceKey.StartsWith("HS2022"));
		}
	}
}
