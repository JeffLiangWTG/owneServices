using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

sealed class JobDeclarationLookupsTest : TestCaseWithFactory
{
	public void TestPaymentPartyList_CodesAsString()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
		var lookups = declaration.Lookups;
		AssertEquals("A, B, C, D, E, F, G, H, J, K, M, O, P, R, S, T, U, V", lookups.PaymentPartyList.CodesAsString);
	}

	public void TestDeclarationLanguageList()
	{
		AssertEquals("NL, FR, DE, EN", Factory.New<JobDeclaration>().Lookups.DeclarationLanguageList.CodesAsString);
	}

	public void TestConsigneeList()
	{
		AssertType<ConsigneeCollection>(Factory.New<JobDeclaration>().Lookups.ConsigneeList);
	}

	public void TestBELocationOfGoodsList()
	{
		var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType("FAC", "FAC");
		helper.CreateNewOrGetExistingCusCodeType("DIF", "DIF");
		new List<string> { "D&A Locatie", "Luchthaven", "Zeehaven", "Pakhuis", "failforall" }.ForEach(li => helper.CreateCusCodeListWithAttribute("BE", "FAC", li + "_code", "desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "Type", li));
		helper.CreateCusCodeListWithAttribute("BE", "DIF", "DIF", "desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "Type", "D&A Locatie");
		helper.CreateCusCodeListWithAttribute("DE", "FAC", "DE", "desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "Type", "D&A Locatie");
		helper.CreateCusCodeListWithAttribute("BE", "FAC", "NotType", "desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "NotType", "D&A Locatie");
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();

		CombineAssertions(() =>
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			var collection = declaration.Lookups.BELocationOfGoodsList;
			collection.Reload(true);
			AssertContainsExactElementsInAnyOrder("AIR", new[] { "D&A Locatie_code", "Luchthaven_code", "Pakhuis_code" }, collection.Select(c => c.ZZD_Code));

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			collection = declaration.Lookups.BELocationOfGoodsList;
			collection.Reload(true);
			AssertContainsExactElementsInAnyOrder("SEA", new[] { "D&A Locatie_code", "Pakhuis_code", "Zeehaven_code" }, collection.Select(c => c.ZZD_Code));

			declaration.JE_TransportMode = "RND";
			collection = declaration.Lookups.BELocationOfGoodsList;
			collection.Reload(true);
			AssertContainsExactElementsInAnyOrder("RND", new[] { "D&A Locatie_code", "Luchthaven_code", "Pakhuis_code", "Zeehaven_code" }, collection.Select(c => c.ZZD_Code));
		});
	}

	public void TestJE_CustomsOfficeList()
	{
		var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "FAC");

		new List<string> { "D&A Locatie", "Luchthaven", "Zeehaven", "Pakhuis", "failforall" }.ForEach(li =>
		{
			var codeList = helper.CreateCusCodeListWithAttribute("BE", "FAC", li + "_code", "desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "Type", li);
			helper.CreateCusCodeListAttribute(codeList.PK, "SubType", "Kantoor");

			helper.CreateCusCodeListWithAttribute("BE", "FAC", li + "_invalid1", "desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "Type", li);

			codeList = helper.CreateCusCodeListWithAttribute("BE", "FAC", li + "_invalid2", "desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "NotType", li);
			helper.CreateCusCodeListAttribute(codeList.PK, "SubType", "Kantoor");
		});

		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
		var collection = declaration.Lookups.JE_CustomsOfficeList;
		collection.Reload(true);

		AssertEquals("D&A Locatie_code", ((ZZRefCusCodeListCombined)collection.Single()).ZZD_Code);
	}
}
