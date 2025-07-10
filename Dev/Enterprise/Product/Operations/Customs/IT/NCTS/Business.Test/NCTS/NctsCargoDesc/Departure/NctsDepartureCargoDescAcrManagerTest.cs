using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsDepartureCargoDescAcrManagerTest : BusinessObjectValidationTestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when nctsCargoDesc parameter is null", () => new NctsDepartureCargoDescAcrManager(null));
		AssertExceptionThrown<ArgumentNullException>("Exception expected when nctsCargoDesc.Header parameter is null", () => new NctsDepartureCargoDescAcrManager(Factory.New<NctsDepartureCargoDesc>()));
	}

	#region Validation

	public void TestValidateGoodsItemAcrSupportingDocumentSettingConsignorAtGoodsItemLevel()
	{
		var consignorWithAcr = GetOrganisationWithAcr();
		var consignor = Factory.NewWithValidTestData<OrgHeader>();

		var nctsHeader = Factory.NewDepartureNctsHeader();
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();

		goodsItem.Consignor.OrganisationPK = consignor.PK;
		goodsItem.Validation.ValidateAll();
		AssertNoRowMessageError("When Goods Item>[2] Consignor Address does not have a valid ACR Authorisation", goodsItem, ExpectedMessageError);

		goodsItem.Consignor.OrganisationPK = consignorWithAcr.PK;
		goodsItem.SupportingDocuments.RemoveAll();
		goodsItem.Validation.ValidateAll();
		AssertHasRowMessageError($"When Goods Item>[2] Consignor Address has a valid ACR Authorisation but no {AcrDocValue} Supporting Document is present", goodsItem, ExpectedMessageError);

		var c521Sup = goodsItem.SupportingDocuments.AddNew();
		c521Sup.CSI_Code = AcrDocValue;
		goodsItem.Validation.ValidateAll();
		AssertNoRowMessageError($"When Goods Item>[2] Consignor Address has a valid ACR Authorisation but have {AcrDocValue} Supporting Document is present", goodsItem, ExpectedMessageError);
	}

	public void TestValidateGoodsItemAcrSupportingDocumentSettingConsignorAtDepatureDeclarationLevel()
	{
		var consignorWithAcr = GetOrganisationWithAcr();
		var consignor = Factory.New<OrgHeader>();

		var nctsHeader = Factory.NewDepartureNctsHeader();
		var goodsItem1 = nctsHeader.MovementHeader.GoodsItems.AddNew();
		var goodsItem2 = nctsHeader.MovementHeader.GoodsItems.AddNew();

		nctsHeader.Consignor.OrganisationPK = consignor.PK;
		ValidateAll();
		CombineAssertions("When Departure Declaration>[2] Consignor Address does not have a valid ACR Authorisation", () =>
		{
			AssertNoRowMessageError(goodsItem1, ExpectedMessageError);
			AssertNoRowMessageError(goodsItem2, ExpectedMessageError);
		});

		nctsHeader.Consignor.OrganisationPK = consignorWithAcr.PK;
		ValidateAll();
		CombineAssertions($"When Declaration>[2] Consignor Address has a valid ACR Authorisation but no {AcrDocValue} Supporting Documents are present", () =>
		{
			AssertHasRowMessageError(goodsItem1, ExpectedMessageError);
			AssertHasRowMessageError(goodsItem2, ExpectedMessageError);
		});

		var c521Sup = goodsItem1.SupportingDocuments.AddNew();
		c521Sup.CSI_Code = AcrDocValue;
		ValidateAll();
		CombineAssertions("When Declaration>[2] Consignor Address has a valid ACR Authorisation", () =>
		{
			AssertNoRowMessageError($"{AcrDocValue} supporting document is present", goodsItem1, ExpectedMessageError);
			AssertHasRowMessageError($"No {AcrDocValue} Supporing Document", goodsItem2, ExpectedMessageError);
		});

		void ValidateAll()
		{
			goodsItem1.Validation.ValidateAll();
			goodsItem2.Validation.ValidateAll();
		}
	}

	public void TestValidateGoodsItemAcrSupportingFilterByConsignorAddress()
	{
		var consignorWithAcr = GetOrganisationWithAcr();
		var addressWithoutAcr = consignorWithAcr.Addresses.AddNew();

		var nctsHeader = Factory.NewDepartureNctsHeader();
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
		var nctsConsignor = nctsHeader.Consignor;

		nctsConsignor.E2_OA_Address = consignorWithAcr.MainAddress.PK;
		goodsItem.Validation.ValidateAll();
		AssertHasRowMessageError($"When Goods Item>[2] Consignor Address has a valid ACR Authorisation but no {AcrDocValue} Supporting Document is present", goodsItem, ExpectedMessageError);

		nctsConsignor.E2_OA_Address = addressWithoutAcr.PK;
		goodsItem.Validation.ValidateAll();
		AssertNoRowMessageError($"When Goods Item>[2] Consignor Address has not a valid ACR Authorisation but have {AcrDocValue} Supporting Document is present", goodsItem, ExpectedMessageError);
	}

	public void TestValidateAcrGoodsItemConsignorWinsAgainstDepartureDeclarationConsignor()
	{
		var consignorWithAcr = GetOrganisationWithAcr();
		var consignor = Factory.New<OrgHeader>();

		var nctsHeader = Factory.NewDepartureNctsHeader();
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();

		nctsHeader.Consignor.OrganisationPK = consignorWithAcr.PK;
		goodsItem.Validation.ValidateAll();
		AssertHasRowMessageError($"When Declaration>[2] Consignor Address has a valid ACR Authorisation but no {AcrDocValue} Supporting Documents are present", goodsItem, ExpectedMessageError);

		goodsItem.Consignor.OrganisationPK = consignor.PK;
		goodsItem.Validation.ValidateAll();
		AssertNoRowMessageError($"When Goods Item>[2] Consignor Address has not a valid ACR Authorisation but have {AcrDocValue} Supporting Document is present", goodsItem, ExpectedMessageError);
	}

	ZString ExpectedMessageError => $"For Transit Authorized Consignor add document type [Authorization> Rule Code [DOC]> {AcrDocValue}]";

	#endregion

	#region Manage

	public void TestAddSupportingDocumentIfNeededOnChangedConsignor()
	{
		var consignorWithAcr = GetOrganisationWithAcr();
		var consignor = Factory.New<OrgHeader>();

		var nctsHeader = Factory.NewDepartureNctsHeader();
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();

		goodsItem.Consignor.OrganisationPK = consignor.PK;
		AssertEquals("When Goods Item>[2] Consignor Address has not a valid ACR Authorisation", 0, goodsItem.SupportingDocuments.Count);

		goodsItem.Consignor.OrganisationPK = consignorWithAcr.PK;
		AssertSupportingDocumentAdded("When Goods Item>[2] Consignor Address has a valid ACR Authorisation", goodsItem, 0);

		goodsItem.Consignor.OrganisationPK = consignor.PK;
		AssertOnlyOneSupportingDocumentExists("When Goods Item>[2] Consignor Address has not anymore a valid ACR Authorisation and previous SupportingDocument is not removed", goodsItem);

		goodsItem.SupportingDocuments.RemoveAll();

		var addressWithoutAcr = consignorWithAcr.Addresses.AddNew();
		goodsItem.Consignor.E2_OA_Address = addressWithoutAcr.PK;
		AssertEquals("When Goods Item>[2] Consignor Address has not a valid ACR Authorisation - New address", 0, goodsItem.SupportingDocuments.Count);
	}

	public void TestAddSupportingDocumentIfNeededOnChangedGoodsDescription()
	{
		var consignorWithAcr = GetOrganisationWithAcr();
		var consignor = Factory.New<OrgHeader>();

		var nctsHeader = Factory.NewDepartureNctsHeader();
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();

		goodsItem.BY_Description = "AAA";
		AssertEquals("When no Declaration>[2] Consignor Address", 0, goodsItem.SupportingDocuments.Count);

		nctsHeader.Consignor.OrganisationPK = consignor.PK;
		goodsItem.BY_Description = "BBB";
		AssertEquals("When Declaration>[2] Consignor Address has not a valid ACR Authorisation", 0, goodsItem.SupportingDocuments.Count);

		nctsHeader.Consignor.OrganisationPK = consignorWithAcr.PK;
		goodsItem.BY_Description = "CCC";
		AssertSupportingDocumentAdded("When Declaration>[2] Consignor Address has a valid ACR Authorisation", goodsItem, 0);

		goodsItem.BY_Description = "DDD";
		AssertOnlyOneSupportingDocumentExists("When Declaration>[2] Consignor Address has a valid ACR Authorisation and GoodsDescription has changed", goodsItem);

		goodsItem.SupportingDocuments.RemoveAll();

		goodsItem.Consignor.OrganisationPK = consignor.PK;
		goodsItem.BY_Description = "BBB";
		AssertEquals("When Goods Item>[2] Consignor Address has not a valid ACR Authorisation", 0, goodsItem.SupportingDocuments.Count);

		goodsItem.Consignor.OrganisationPK = consignorWithAcr.PK;
		goodsItem.BY_Description = "CCC";
		AssertSupportingDocumentAdded("When Goods Item>[2] Consignor Address has a valid ACR Authorisation", goodsItem, 0);

		goodsItem.BY_Description = "DDD";
		AssertOnlyOneSupportingDocumentExists("When Goods Item>[2] Consignor Address has a valid ACR Authorisation and GoodsDescription has changed", goodsItem);
	}

	public void TestAddSupportingDocumentIfNeededOnChangedTariff()
	{
		var consignorWithAcr = GetOrganisationWithAcr();
		var consignor = Factory.New<OrgHeader>();

		var nctsHeader = Factory.NewDepartureNctsHeader();
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();

		goodsItem.BY_HarmonisedTariff = "000";
		AssertEquals("When no Declaration >[2] Consignor Address", 0, goodsItem.SupportingDocuments.Count);

		nctsHeader.Consignor.OrganisationPK = consignor.PK;
		goodsItem.BY_HarmonisedTariff = "111";
		AssertEquals("When Declaration>[2] Consignor Address has not a valid ACR Authorisation", 0, goodsItem.SupportingDocuments.Count);

		nctsHeader.Consignor.OrganisationPK = consignorWithAcr.PK;
		goodsItem.BY_HarmonisedTariff = "222";
		AssertSupportingDocumentAdded("When Declaration>[2] Consignor Address has a valid ACR Authorisation", goodsItem, 0);

		goodsItem.BY_HarmonisedTariff = "333";
		AssertOnlyOneSupportingDocumentExists("When Declaration>[2] Consignor Address has a valid ACR Authorisation and GoodsDescription has changed", goodsItem);

		goodsItem.SupportingDocuments.RemoveAll();

		goodsItem.Consignor.OrganisationPK = consignor.PK;
		goodsItem.BY_HarmonisedTariff = "111";
		AssertEquals("When Goods Item>[2] Consignor Address has not a valid ACR Authorisation", 0, goodsItem.SupportingDocuments.Count);

		goodsItem.Consignor.OrganisationPK = consignorWithAcr.PK;
		goodsItem.BY_HarmonisedTariff = "222";
		AssertSupportingDocumentAdded("When Goods Item>[2] Consignor Address has a valid ACR Authorisation", goodsItem, 0);

		goodsItem.BY_HarmonisedTariff = "333";
		AssertOnlyOneSupportingDocumentExists("When Goods Item>[2] Consignor Address has a valid ACR Authorisation and GoodsDescription has changed", goodsItem);
	}

	public void TestAddSupportingDocumentIfNeededCombinedConsignors()
	{
		var consignorWithAcr = GetOrganisationWithAcr();
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();

		nctsHeader.Consignor.OrganisationPK = consignorWithAcr.PK;
		goodsItem.BY_Description = "AAA";
		AssertSupportingDocumentAdded("When Declaration>[2] Consignor Address has a valid ACR Authorisation", goodsItem, 0);
		goodsItem.BY_HarmonisedTariff = "111";
		AssertOnlyOneSupportingDocumentExists("When Declaration>[2] Consignor Address has a valid ACR Authorisation and Tariff has changed", goodsItem);

		var consignorWithAcr2 = GetOrganisationWithAcr(AcrDocValue, "1111112");
		goodsItem.Consignor.OrganisationPK = consignorWithAcr2.PK;
		AssertOnlyOneSupportingDocumentExists("When Goods Item>[2] Consignor Address has another ACR Authorisation with the same DocType of Declaration>[2] Consignor Address", goodsItem);
		goodsItem.BY_Description = "BBB";
		AssertOnlyOneSupportingDocumentExists("When Goods Item>[2] Consignor Address has another ACR Authorisation with the same DocType of Declaration>[2] Consignor Address and GoodsDescription has changed", goodsItem);
		goodsItem.BY_HarmonisedTariff = "222";
		AssertOnlyOneSupportingDocumentExists("When Goods Item>[2] Consignor Address has another ACR Authorisation with the same DocType of Declaration>[2] Consignor Address and Tariff has changed", goodsItem);

		var anotherAcrDocValue = "18";
		var consignorWithAcr3 = GetOrganisationWithAcr(anotherAcrDocValue, "1111113");
		goodsItem.Consignor.OrganisationPK = consignorWithAcr3.PK;
		AssertSupportingDocumentAdded("When Goods Item>[2] Consignor Address has another ACR Authorisation with a different DocType of Declaration>[2] Consignor Address", goodsItem, 1, anotherAcrDocValue, "1111113");
		goodsItem.BY_Description = "CCC";
		AssertOnlyOneSupportingDocumentExists("When Goods Item>[2] Consignor Address has another ACR Authorisation with a different DocType of Declaration>[2] Consignor Address and GoodsDescription has changed", goodsItem, anotherAcrDocValue);
		goodsItem.BY_HarmonisedTariff = "333";
		AssertOnlyOneSupportingDocumentExists("When Goods Item>[2] Consignor Address has another ACR Authorisation with a different DocType of Declaration>[2] Consignor Address and Tariff has changed", goodsItem, anotherAcrDocValue);
	}

	void AssertSupportingDocumentAdded(ZString assertionMessage, NctsDepartureCargoDesc goodsItem, int previousSupportingDocumentCount, string acrDocValue = AcrDocValue, string acrAuthNumber = AcrAuthNumber)
	{
		AssertEquals($"{assertionMessage} - SupportingDocument count", previousSupportingDocumentCount + 1, goodsItem.SupportingDocuments.Count);
		var newSupportingDocument = goodsItem.SupportingDocuments[previousSupportingDocumentCount];

		CombineAssertions(assertionMessage, () =>
		{
			AssertEquals("SupportingDocument type", acrDocValue, newSupportingDocument.CSI_Code);
			AssertEquals("SupportingDocument reference", acrAuthNumber, newSupportingDocument.CSI_ReferenceNumber);
		});
	}

	void AssertOnlyOneSupportingDocumentExists(ZString assertionMessage, NctsDepartureCargoDesc goodsItem, string acrDocValue = AcrDocValue)
	{
		AssertEquals(assertionMessage, 1, goodsItem.SupportingDocuments.Cast<NctsSupportingDocument>().Count(d => d.CSI_Code == acrDocValue));
	}

	#endregion

	OrgHeader GetOrganisationWithAcr(string acrDocValue = AcrDocValue, string acrAuthNumber = AcrAuthNumber)
	{
		var organisationWithAcr = Factory.New<OrgHeader>();
		organisationWithAcr.OH_Code = "ACR";
		var acrAuthorisation = Factory.NewAuthorisation(organisationWithAcr, acrAuthNumber, "ACR", organisationWithAcr.MainAddress);
		var docRule = acrAuthorisation.CusAuthorisationRules.AddNew();
		docRule.CPR_RuleCode = "DOC";
		docRule.CPR_ValueFrom = acrDocValue;
		return organisationWithAcr;
	}

	const string AcrDocValue = "C521";
	const string AcrAuthNumber = "111111";
}
