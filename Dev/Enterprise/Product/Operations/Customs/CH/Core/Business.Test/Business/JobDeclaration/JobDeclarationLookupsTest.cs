using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.CH;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ContainerModes = Enterprise.Core.Constants.ContainerModes;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(JobDeclarationLookups))]
sealed class JobDeclarationLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestTransportTypeList()
	{
		AssertContainsExactElementsInAnyOrder(new string[] { "AIR", "SEA", "MAI", "ROA", "RAI", "FIX", "IWT", "OWN" }, lookups.TransportTypeList.GetAllCodes());
	}

	public void TestCustomsOfficeList()
	{
		AssertSame(CommonLookups.CustomsOfficeList(declaration), declaration.Lookups.CustomsOffices);
	}

	public void TestCargoIdTypeList()
	{
		declaration.JE_TransportMode = "AIR";
		AssertContainsExactElementsInAnyOrder("AIR", new[] { ContainerModes.Loose, ContainerModes.ULD, ContainerModes.NonContainerised }, lookups.CargoIdTypeList.GetAllCodes());
		declaration.JE_TransportMode = "FIX";
		AssertContainsExactElementsInAnyOrder("FIX", new[] { ContainerModes.Containerised, ContainerModes.NonContainerised, }, lookups.CargoIdTypeList.GetAllCodes());
		declaration.JE_TransportMode = "IWT";
		AssertContainsExactElementsInAnyOrder("IWT", new[] { ContainerModes.Containerised, ContainerModes.NonContainerised, }, lookups.CargoIdTypeList.GetAllCodes());
		declaration.JE_TransportMode = "OWN";
		AssertContainsExactElementsInAnyOrder("OWN", new[] { ContainerModes.Containerised, ContainerModes.NonContainerised, }, lookups.CargoIdTypeList.GetAllCodes());
		declaration.JE_TransportMode = "MAI";
		AssertContainsExactElementsInAnyOrder("MAI", new[] { ContainerModes.Containerised, ContainerModes.NonContainerised, }, lookups.CargoIdTypeList.GetAllCodes());
		declaration.JE_TransportMode = "RAI";
		AssertContainsExactElementsInAnyOrder("RAI", new[] { ContainerModes.FCL, ContainerModes.LCL, ContainerModes.Bulk, ContainerModes.Liquid, ContainerModes.BreakBulk, ContainerModes.Containerised, ContainerModes.NonContainerised }, lookups.CargoIdTypeList.GetAllCodes());
		declaration.JE_TransportMode = "ROA";
		AssertContainsExactElementsInAnyOrder("ROA", new[] { ContainerModes.FCL, ContainerModes.FTL, ContainerModes.LCL, ContainerModes.LTL, ContainerModes.Containerised, ContainerModes.NonContainerised }, lookups.CargoIdTypeList.GetAllCodes());
		declaration.JE_TransportMode = "SEA";
		AssertContainsExactElementsInAnyOrder("SEA", new[] { ContainerModes.FCL, ContainerModes.LCL, ContainerModes.Bulk, ContainerModes.Liquid, ContainerModes.BreakBulk, ContainerModes.RollOnRollOff, ContainerModes.Containerised, ContainerModes.NonContainerised }, lookups.CargoIdTypeList.GetAllCodes());
	}

	public void TestMessageTypeList()
	{
		var list1 = lookups.MessageTypeList;
		var list2 = lookups.MessageTypeList;
		AssertSame(list1, list2);
		AssertEquals("EDA - Export Declaration Activation|EXP - Export|IMP - Import|MSC - Miscellaneous Customs", list1.GetHumanReadableListOfElements("|"));
	}

	public void TestRepresentativeList()
	{
		AssertType<OrgHeaderCollection>(lookups.RepresentativeList);
	}

	public void TestConsignorList()
	{
		AssertType<ConsignorCollection>(lookups.ConsignorList);
	}

	public void TestPaymentPartyList()
	{
		AssertEquals("PaymentPartyList", "CSH, CSE, CON, DEC, FWD, IMP", lookups.PaymentPartyList.CodesAsString);
	}

	public void TestDeclarationLanguageList()
	{
		AssertSame(CommonLookups.CommunicationLanguageList(Factory), lookups.DeclarationLanguageList);
	}

	public void TestMessageStatusList()
	{
		AssertSame(CommonLookups.MessageStatusList(Factory), lookups.MessageStatusList);
	}

	public void TestMessageSubTypeList()
	{
		AssertSame(CommonLookups.ActivationTypeList(Factory), lookups.MessageSubTypeList);
	}

	public void TestTransportMeansList()
	{
		AssertSame(CommonLookups.TransportModeList(Factory), lookups.TransportMeansList);
	}

	public void TestAuthorizationsList_Export() => CombineAssertions(() =>
	{
		var declarant = Factory.NewWithValidTestData<OrgHeader>();
		declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

		SetDeclarationType(CHJobMessageTypeList.Codes.Export, ZString.Empty);
		AssertSame(AssertionMessage(), CommonLookups.ExportAuthorizationsList(declaration, false, declarant.PK), lookups.AuthorizationsList);

		SetDeclarationType(CHJobMessageTypeList.Codes.ExportDeclarationActivation, ActivationTypeList.Codes.Passar);
		AssertSame(AssertionMessage(), CommonLookups.ExportAuthorizationsList(declaration, false, declarant.PK), lookups.AuthorizationsList);

		SetDeclarationType(CHJobMessageTypeList.Codes.ExportDeclarationActivation, ActivationTypeList.Codes.Edec);
		AssertSame(AssertionMessage(), CommonLookups.ExportAuthorizationsList(declaration, true, declarant.PK), lookups.AuthorizationsList);

		void SetDeclarationType(string messageType, string messageSubType)
		{
			declaration.JE_MessageType = messageType;
			declaration.JE_MessageSubType = messageSubType;
		}

		string AssertionMessage() => $"MessageType: {declaration.JE_MessageType}, MessageSubType: {declaration.JE_MessageSubType}";
	});

	public void TestAuthorizationsList_Import() => CombineAssertions(() =>
	{
		var authorized1 = Factory.NewWithValidTestData<OrgHeader>();
		var authorized2 = Factory.NewWithValidTestData<OrgHeader>();
		var autoNumber = 0;

		CreateAuthorization(authorized1, ["A1R1", "A1R2"]);
		CreateAuthorization(authorized2, ["A2R1"]);
		CreateAuthorization(authorized2, ["A2R2"], validSinceDays: 2);
		CreateAuthorization(authorized2, ["A2R3"], isActive: false);

		declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;

		declaration.JE_OA_Representative = authorized1.MainAddress.PK;
		AssertEquals("All rules of consignee", "A1R1, A1R2", declaration.Lookups.AuthorizationsList.CodesAsString);

		declaration.JE_OA_Representative = authorized2.MainAddress.PK;
		AssertEquals("Other authorized consignee", "A2R1, A2R2", declaration.Lookups.AuthorizationsList.CodesAsString);

		declaration.JE_ValuationDate = ZDate.Today.AddDays(-5);
		AssertEquals("Validity date", "A2R1", declaration.Lookups.AuthorizationsList.CodesAsString);

		void CreateAuthorization(OrgHeader holder, string[] ruleValues, int validSinceDays = 10, Boolean isActive = true)
		{
			var authorization = Factory.New<CusAuthorisationHeader>();
			authorization.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeLocationsForEDec;
			authorization.CPH_Number = $"N{++autoNumber}";
			authorization.CPH_OH_PermitHolder = holder.PK;
			authorization.CPH_OA_AppliesTo = holder.MainAddress.PK;
			authorization.CPH_IsActive = true;
			authorization.CPH_StartDate = ZDate.Today.AddDays(-validSinceDays);
			authorization.CPH_IsActive = isActive;
			foreach (var ruleValue in ruleValues)
			{
				CreateRule(authorization, CusAuthorisationRuleTypeList.Codes.Location, ruleValue);
			}
			CreateRule(authorization, "XXX", "OTHER_CODE");
		}

		CusAuthorisationRule CreateRule(CusAuthorisationHeader authorization, string ruleCode, string ruleValue)
		{
			var rule = authorization.CusAuthorisationRules.AddNew();
			rule.CPR_RuleCode = ruleCode;
			rule.CPR_ValueFrom = ruleValue;
			rule.CPR_Description = "DESC" + ruleValue;
			return rule;
		}
	});

	public void TestPhaseStatusList() => AssertType<PassarDeclarationPhaseList>(lookups.EntryPhaseStatusList);

	public void TestSelectionResultList() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateSelectionResultCodeListAndFrenchLanguage(Factory);

		AssertEquals(2, lookups.SelectionResultList.Count);
		Assert(lookups.SelectionResultList.ContainsCode("123"));
	});

	public void TestEntryStatusList()
	{
		AssertSame(CommonLookups.CustomsStatusList(Factory), lookups.EntryStatusList);
	}

	public void TestSpecificCircumstanceList()
	{
		RefCusCodeTestHelper.CreateSpecificCircumstanceIndicatorList(Factory);
		var codeList = lookups.SpecificCircumstanceIndicatorList;
		CombineAssertions(() =>
		{
			AssertSame("Cached", codeList, codeList);
			AssertEquals("CodesAsString", "A20, XXX", codeList.CodesAsString);
		});
	}

	public void TestTransportationTypeList()
	{
		AssertSame(CommonLookups.TransportationTypeList(declaration), lookups.TransportationTypeList);
	}

	public void TestClearanceLocationList()
	{
		RefCusCodeTestHelper.CreateClearanceLocation(Factory);

		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var list = lookups.ClearanceLocationList;
			AssertEquals($"{declaration.JE_MessageType} Valid code", true, list.ContainsCode(RefCusCodeTestHelper.ValidClearanceLocation_CustomsOffice));
			AssertEquals($"{declaration.JE_MessageType} Import only code", true, list.ContainsCode(RefCusCodeTestHelper.ValidClearanceLocation_ImportOnly));
			AssertEquals($"{declaration.JE_MessageType} Export only code", false, list.ContainsCode(RefCusCodeTestHelper.ValidClearanceLocation_ExportOnly));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			list = lookups.ClearanceLocationList;
			AssertEquals($"{declaration.JE_MessageType} Valid code", true, list.ContainsCode(RefCusCodeTestHelper.ValidClearanceLocation_CustomsOffice));
			AssertEquals($"{declaration.JE_MessageType} Import only code", false, list.ContainsCode(RefCusCodeTestHelper.ValidClearanceLocation_ImportOnly));
			AssertEquals($"{declaration.JE_MessageType} Export only code", true, list.ContainsCode(RefCusCodeTestHelper.ValidClearanceLocation_ExportOnly));
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		lookups = declaration.Lookups;
	}
	JobDeclaration declaration;
	JobDeclarationLookups lookups;
}
