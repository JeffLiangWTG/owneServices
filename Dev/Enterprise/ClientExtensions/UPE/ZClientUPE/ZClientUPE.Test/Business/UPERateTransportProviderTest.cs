using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(UPERateTransportProvider))]
	public class UPERateTransportProviderTest : EnterpriseBusinessObjectTestCase
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		public void TestCustomisationsLoadedForRegistry()
		{
			ErrorReporter.Clear();
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			Factory.New<UPERateTransportProvider>();
			AssertEquals("", ErrorReporter.LastMessageReported);
		}

		public void TestLoadCODPostcodeTransportProvider()
		{
			var nullLoadedCodPostcodeZoneSet = UPERateTransportProvider.LoadCODPostcodeTransportProvider(Factory);
			AssertNull("Should return null if no COD transport provider is set up", nullLoadedCodPostcodeZoneSet);
			var decoyTransportOrg = Factory.NewWithValidTestData<OrgHeader>();
			var decoyZoneSet = Factory.NewWithValidTestData<UPERateTransportProvider>();
			decoyZoneSet.TP_OH_RelatedParty = decoyTransportOrg.PK;
			var codTransportOrg = Factory.NewWithValidTestData<OrgHeader>();
			var codPostcodeZoneSet = Factory.NewWithValidTestData<UPERateTransportProvider>();
			codPostcodeZoneSet.TP_OH_RelatedParty = codTransportOrg.PK;
			codTransportOrg.OH_Code = UPERateTransportProvider.CODPostcodeTransportOrgCode;
			Factory.Save();
			var loadedBrownPostcodeZoneSet = UPERateTransportProvider.LoadCODPostcodeTransportProvider(Factory);
			AssertEquals("Should load the correct COD transport provider", codPostcodeZoneSet.PK, loadedBrownPostcodeZoneSet.PK);
		}

		public void TestLoadBrownPostCodeTransportProvider()
		{
			var nullLoadedBrownPostcodeTransportProvider = UPERateTransportProvider.LoadBrownPostcodeTransportProvider(Factory);
			AssertNull("Should return null if no brown transport provider is set up", nullLoadedBrownPostcodeTransportProvider);
			var decoyTransportOrg = Factory.NewWithValidTestData<OrgHeader>();
			var decoyZoneSet = Factory.NewWithValidTestData<UPERateTransportProvider>();
			decoyZoneSet.TP_OH_RelatedParty = decoyTransportOrg.PK;
			var brownTransportOrg = Factory.NewWithValidTestData<OrgHeader>();
			var brownPostcodeZoneSet = Factory.NewWithValidTestData<UPERateTransportProvider>();
			brownPostcodeZoneSet.TP_OH_RelatedParty = brownTransportOrg.PK;
			brownTransportOrg.OH_Code = UPERateTransportProvider.BrownPostcodeTransportOrgCode;
			Factory.Save();
			var loadedBrownPostcodeZoneSet = UPERateTransportProvider.LoadBrownPostcodeTransportProvider(Factory);
			AssertEquals("Should load the correct Brown transport provider", brownPostcodeZoneSet.PK, loadedBrownPostcodeZoneSet.PK);
		}
	}
}
