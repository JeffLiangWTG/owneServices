using System;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(IETS215DataProvider))]
sealed class IETS215DataProviderTest : PNTSMessageHeaderProviderAbstractTest<IETS215DataProvider>
{
	public void TestConsignmentHeaderMasterLevel()
	{
		AssertNotNull(provider.ConsignmentHeaderMasterLevel);
	}

	public void TestDeclarant()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345", Core.Constants.CountryCodes.Belgium);
		orgHeader.OH_FullName = "TestCompany";
		var orgAddress = orgHeader.Addresses.AddNew();
		orgAddress.OA_OH = orgHeader.PK;
		temporaryStorageHeader.AMA_OA_Declarant = orgAddress.PK;
		CombineAssertions(() =>
		{
			AssertNotNull("Declarant should not be null", provider.Declarant);
			AssertEquals("Name of Declarant matched", "TestCompany", provider.Declarant.Name);
			AssertEquals("IdentificationNumber of Declarant matched", "BE12345", provider.Declarant.IdentificationNumber);
		});
	}

	[TestDate(2024, 10, 18, 12, 0, 0)]
	public void TestDeclarationDate()
	{
		AssertEquals(new DateTime(2024, 10, 18, 12, 0, 0), provider.DeclarationDate);
	}

	public void TestLanguageCode()
	{
		AssertEquals("EN", provider.LanguageCode);
	}

	public void TestLrn()
	{
		temporaryStorageHeader.LRN = "TestLRN";
		AssertEquals("TestLRN", provider.Lrn);
	}

	public void TestMrn()
	{
		temporaryStorageHeader.PreviousDocuments.AddNew().CSI_ReferenceNumber = "TestMRN";
		AssertEquals("TestMRN", provider.Mrn);
	}

	public void TestRefToMessageId()
	{
		AssertEquals(ZString.Empty, provider.RefToMessageId);
	}

	public void TestRepresentative()
	{
		temporaryStorageHeader.AMA_OA_Representative = Factory.New<OrgAddress>().PK;

		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345", Core.Constants.CountryCodes.Belgium);
		orgHeader.OH_FullName = "TestCompany";
		var orgAddress = orgHeader.Addresses.AddNew();
		orgAddress.OA_OH = orgHeader.PK;
		temporaryStorageHeader.AMA_OA_Representative = orgAddress.PK;
		CombineAssertions(() =>
		{
			AssertNotNull("Representative should not be null", provider.Representative);
			AssertEquals("Name of Representative matched", "TestCompany", provider.Representative.Name);
			AssertEquals("IdentificationNumber of Representative matched", "BE12345", provider.Representative.IdentificationNumber);
		});
	}

	protected override IETS215DataProvider GetProvider() => new IETS215DataProvider(new TemporaryStorageMessageSendingObject(Factory.NewWithValidTestData<TemporaryStorageHeader>()));
}
