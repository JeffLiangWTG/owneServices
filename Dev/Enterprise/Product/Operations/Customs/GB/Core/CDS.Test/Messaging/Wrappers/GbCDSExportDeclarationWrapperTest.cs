using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.CDS.Messaging.Testing;
using Enterprise.Customs.GB.CDS.Testing;

namespace Enterprise.Customs.GB.CDS.Messaging.Wrappers.Testing
{
	public class GbCDSExportDeclarationWrapperTest : TestCaseWithFactory
	{
		public void TestCarrier()
		{
			const string countryCodeGb = Core.Constants.CountryCodes.UnitedKingdom;
			var entry = GbCDSExportEntryHeaderWrapperTests.CreateSampleEntryHeader(Factory);
			var orgHeader = OrganisationWrapperTest.CreateOrgHeaderWithEori(Factory, "123", countryCodeGb);
			entry.Declaration.JE_OH_ShippingLine = orgHeader.PK;
			var provider = (IConsignment)new GbCDSExportDeclarationWrapper(entry);

			CombineAssertions(() =>
			{
				AssertEquals("ID is present so Name should be empty", ZString.Empty, provider.Carrier.Name);
				AssertNull("ID is present so Address should be null", provider.Carrier.Address);
				AssertEquals("ID is present so ID should be populated", "GB123", provider.Carrier.ID);
			});

			orgHeader.CustomsCodes.RemoveAndDeleteAll();
			provider = new GbCDSExportDeclarationWrapper(entry);

			CombineAssertions(() =>
			{
				AssertEquals("ID is NOT present so Name should not be empty", "Company Name 1", provider.Carrier.Name);
				AssertNotNull("ID is NOT present so Address should not be null", provider.Carrier.Address);
				AssertEquals("ID NOT is present so ID should not be populated", ZString.Empty, provider.Carrier.ID);
			});
		}
	}
}
