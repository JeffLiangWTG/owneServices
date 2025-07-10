using System;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using OrgAddress = Enterprise.MasterFiles.Business.OrgAddress;
using OrgCusCode = Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(IETS115DataProvider))]
sealed class IETS115DataProviderTest : PNTSMessageHeaderProviderAbstractTest<IETS115DataProvider>
{
	public void TestConsignmentHeaderMasterLevel()
	{
		AssertType<PNTSConsignmentHeaderMasterLevelProvider>(provider.ConsignmentHeaderMasterLevel);
	}

	public void TestCorrelationIdentifier()
	{
		AssertEquals(ZString.Empty, provider.CorrelationIdentifier);
	}

	public void TestCustomsOfficeOfPresentationReferenceNumber()
	{
		AssertEquals("NB", provider.CustomsOfficeOfPresentationReferenceNumber);
	}

	[TestDate(2024, 10, 18, 11, 0, 0)]
	public void TestDateAndTimeOfPresentationOfTheGoods()
	{
		AssertEquals(new DateTime(2024, 10, 18, 11, 0, 0), provider.DateAndTimeOfPresentationOfTheGoods);
	}

	public void TestDeclarant()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345", Core.Constants.CountryCodes.Belgium);
		orgHeader.OH_FullName = "SBCompany";
		var orgAddress = orgHeader.Addresses.AddNew();
		orgAddress.OA_OH = orgHeader.PK;
		temporaryStorageHeader.AMA_OA_Declarant = orgAddress.PK;
		CombineAssertions(() =>
		{
			AssertNotNull("Declarant should not be null", provider.Declarant);
			AssertEquals("Name of Declarant matched", "SBCompany", provider.Declarant.Name);
			AssertEquals("IdentificationNumber of Declarant matched", "BE12345", provider.Declarant.IdentificationNumber);
		});
	}

	[TestDate(2024, 10, 18, 12, 0, 0)]
	public void TestDeclarationDate()
	{
		AssertEquals(new DateTime(2024, 10, 18, 12, 0, 0), provider.DeclarationDate);
	}

	public void TestENSReUseIndicator()
	{
		AssertEquals(0, provider.ENSReUseIndicator);
	}

	public void TestLanguageCode()
	{
		AssertEquals("EN", provider.LanguageCode);
	}

	public void TestLrn()
	{
		AssertEquals("222", provider.Lrn);
	}

	public void TestMessageRecipient()
	{
		AssertEquals(ZString.Empty, provider.MessageRecipient);
	}

	public void TestMessageSender()
	{
		AssertEquals(ZString.Empty, provider.MessageSender);
	}

	public void TestPersonPresentingTheGoodsIdentificationNumber()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORI123");
		var orgAddress = orgHeader.MainAddress;
		temporaryStorageHeader.AMA_OA_Presenter = orgAddress.PK;
		AssertEquals("BEEORI123", provider.PersonPresentingTheGoodsIdentificationNumber);
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
		orgHeader.OH_FullName = "SBCompany";
		var orgAddress = orgHeader.Addresses.AddNew();
		orgAddress.OA_OH = orgHeader.PK;
		temporaryStorageHeader.AMA_OA_Representative = orgAddress.PK;
		CombineAssertions(() =>
		{
			AssertNotNull("Representative should not be null", provider.Representative);
			AssertEquals("Name of Representative matched", "SBCompany", provider.Representative.Name);
			AssertEquals("IdentificationNumber of Representative matched", "BE12345", provider.Representative.IdentificationNumber);
		});
	}

	public void TestSupervisingCustomsOffice()
	{
		AssertEquals("SB", provider.SupervisingCustomsOffice);
	}

	protected override IETS115DataProvider GetProvider() => new IETS115DataProvider(new TemporaryStorageMessageSendingObject(Factory.New<TemporaryStorageHeader>()));

	protected override void SetUp()
	{
		base.SetUp();
		temporaryStorageHeader.LRN = "222";
		temporaryStorageHeader.AMA_CustomsOffice = "SB";
		temporaryStorageHeader.PresentationCustomsOffice = "NB";
	}
}
