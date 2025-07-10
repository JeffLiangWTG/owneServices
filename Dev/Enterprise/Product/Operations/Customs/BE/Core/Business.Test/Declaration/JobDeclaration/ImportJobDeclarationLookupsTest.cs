using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

sealed class ImportJobDeclarationLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestJE_CustomsOfficeList()
	{
		using (ConfigurationTestHelper.TemporarilySetDeclarationConfiguration(Factory, "IsUCC6Core", true))
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeType(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BE SCO", "desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, RefCusCodeListAttributeTypes.Codes.ROLE, "SCO");

			Factory.Save();

			var declaration = GetImportDeclaration();
			var mainOfficeRequirement = declaration.CustomsOfficeRequirementHelper.MainOffice;
			mainOfficeRequirement.OfficeRole = "SCO";

			var customsOffice = declaration.Lookups.CustomsOffices;
			customsOffice.Load();
			AssertContainsExactElementsInAnyOrder(new[] { "BE SCO" }, customsOffice.Select(x => x.ZZD_Code));
		}
	}

	JobDeclaration GetImportDeclaration()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
		return jobDeclaration;
	}
}
