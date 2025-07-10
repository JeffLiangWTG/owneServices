using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(DocTraderDataWrapper))]
sealed class DocTraderDataWrapperTest : DocumentWrapperTestCase
{
	public void TestDocTraderDataWrapper_OrgAddress()
	{
		var organisation = Factory.New<OrgHeader>();
		organisation.OH_FullName = "Trader Name";
		organisation.OH_RL_NKClosestPort = "CHBSL";
		organisation.OH_Code = "OH123";
		organisation.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, "1000088059", "CH");
		organisation.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.UID, "CHE-123.456.788", "CH");

		var orgAddress = organisation.Addresses.AddNew();
		orgAddress.OA_Address1 = "Address 1";
		orgAddress.OA_Address2 = "Address 2.....26.....";
		orgAddress.OA_City = "City............26......";
		orgAddress.OA_PostCode = "9999";
		orgAddress.OA_RN_NKCountryCode = "CH";

		var docTraderDataWrapper = DocTraderDataWrapper.New(orgAddress, Factory, 29);

		CombineAssertions(() =>
		{
			AssertEquals("Name should match", "Trader Name", docTraderDataWrapper.Name);
			AssertEquals("StreetAndNumber should match", "Address 1 Address 2.....26...", docTraderDataWrapper.StreetAndNumber);
			AssertEquals("Destination should match", "CH-9999 City............26...", docTraderDataWrapper.Destination);
			AssertEquals("UID should match", "CHE-123.456.788", docTraderDataWrapper.UID);
			AssertEquals("BID should match", "1000088059", docTraderDataWrapper.BID);
		});
	}

	public void TestDocTraderDataWrapper_JobDocAddress()
	{
		var organisation = Factory.New<OrgHeader>();
		var jobDocAddress = Factory.New<JobDocAddress>();
		jobDocAddress.OrganisationPK = organisation.PK;

		organisation.OH_FullName = "Trader Name";
		organisation.OH_RL_NKClosestPort = "CHBSL";
		organisation.OH_Code = "OH123";
		organisation.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, "1000088059", "CH");
		organisation.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.UID, "CHE-123.456.788", "CH");

		jobDocAddress.Address.OA_Address1 = "Address 1";
		jobDocAddress.Address.OA_Address2 = "Address 2.....26.....";
		jobDocAddress.Address.OA_City = "City............26......";
		jobDocAddress.Address.OA_PostCode = "9999";
		jobDocAddress.Address.OA_RN_NKCountryCode = "CH";

		var docTraderDataWrapper = DocTraderDataWrapper.New(jobDocAddress, Factory, 29);

		CombineAssertions(() =>
		{
			AssertEquals("Name should match", "Trader Name", docTraderDataWrapper.Name);
			AssertEquals("StreetAndNumber should match", "Address 1 Address 2.....26...", docTraderDataWrapper.StreetAndNumber);
			AssertEquals("Destination should match", "CH-9999 City............26...", docTraderDataWrapper.Destination);
			AssertEquals("UID should match", "CHE-123.456.788", docTraderDataWrapper.UID);
			AssertEquals("BID should match", "1000088059", docTraderDataWrapper.BID);
		});
	}

	public override DocumentWrapper[] GetDocumentWrappers()
	{
		var orgAddress = Factory.New<OrgAddress>();
		var jobDocAddress = Factory.New<JobDocAddress>();
		return new DocumentWrapper[] { DocTraderDataWrapper.New(jobDocAddress, Factory), DocTraderDataWrapper.New(orgAddress, Factory) };
	}
}
