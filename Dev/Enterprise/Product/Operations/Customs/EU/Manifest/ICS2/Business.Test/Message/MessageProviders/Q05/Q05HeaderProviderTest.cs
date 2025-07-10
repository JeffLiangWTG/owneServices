using System;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test;

public class Q05HeaderProviderTest : Customs.Business.Testing.DataProviderTestCase<Q05HeaderProvider>
{
	[TestDate(2024, 12, 23)]
	public void TestFunctionalReference()
	{
		AssertEquals(Q05OutboundEDIMessage.LRNPlaceHolder, Provider.FunctionalReference);
	}

	public void TestLRN()
	{
		AssertEquals(null, Provider.LRN);
	}

	public void TestMRN()
	{
		manifestHeader.RegistrationNumber = "TestRegNo123";
		AssertEquals("TestRegNo123", Provider.MRN);
	}

	public void TestRequestIdentificationNumber()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_FullName = "Name123";
		var mainAddress = orgHeader.MainAddress;

		manifestHeader.AMA_OA_Declarant = orgHeader.MainAddress.PK;
		mainAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "654321", CountryCodes.Germany);

		AssertEquals("DE654321", Provider.RequestIdentificationNumber);
	}

	public void TestTransportDocumentMasterLevel()
	{
		AssertNull(Provider.TransportDocumentMasterLevel);
	}

	public void TestTransportDocumentHouseLevel()
	{
		AssertNull(Provider.TransportDocumentHouseLevel);
	}

	[TestDate(2025, 2, 25, 0, 0, 0)]
	public void TestCurrentDateTimeUtc()
	{
		AssertEquals(new DateTime(2025, 2, 25), Provider.CurrentDateTimeUtc);
	}

	public void TestRequestNotification()
	{
		AssertEquals("1", Provider.RequestNotification);
	}

	protected override void SetUp()
	{
		base.SetUp();
		manifestHeader = Factory.New<AsycudaManifestHeader>();
	}
	AsycudaManifestHeader manifestHeader;

	protected override Q05HeaderProvider GetProvider()
	{
		return new Q05HeaderProvider(manifestHeader);
	}
}
