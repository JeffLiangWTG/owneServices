using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	class CommonLookupsTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestEORILookup_DeclarationIsNull()
		{
			var list = CommonLookups.EORILookup(null, null);
			NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison), "CodeAsString");
		}

		[ExpectNoExceptions]
		public void TestEORILookup_DeclarationIsNotNull()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var list = CommonLookups.EORILookup(declaration, null);
				NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison), "Default-CodeAsString");

				declaration.JE_OA_DeclarantAddress = CreateOrgWithEori("Declarant", "111111").MainAddress.PK;
				declaration.JE_OA_Representative = CreateOrgWithEori("Representative", "222222").MainAddress.PK;
				declaration.JE_OH_Importer = CreateOrgWithEori("Importer", "333333").PK;
				declaration.JE_OH_Buyer = CreateOrgWithEori("Buyer", "444444").PK;
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				instruction.CEI_OA_Warehouse2 = CreateOrgWithEori("Warehouse2", "555555").MainAddress.PK;
				instruction.CEI_OA_Warehouse = CreateOrgWithEori("Warehouse1", "666666").MainAddress.PK;

				list = CommonLookups.EORILookup(declaration, null);
				NUnit.Framework.Assert.That(list.ElementsAsString, NUnit.Framework.Is.EqualTo(@"LV111111 - Declarant - Declarant
LV222222 - Representative - Representative
LV333333 - Importer - Importer
LV444444 - Buyer - Buyer
LV555555 - Warehouse - Warehouse2
LV666666 - Warehouse - Warehouse1"), "GetIdentificationNumberIsNull-ElementsAsString");

				list = CommonLookups.EORILookup(declaration, org => "HelloWorld");
				NUnit.Framework.Assert.That(list.ElementsAsString, NUnit.Framework.Is.EqualTo("HelloWorld - Declarant - Declarant"), "GetIdentificationNumberIsNotNull-ElementsAsString");
			});
		}

		[ExpectNoExceptions]
		public void TestEORILookup_HasFilter()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_OA_DeclarantAddress = CreateOrgWithEori("Declarant", "111111").MainAddress.PK;
				declaration.JE_OA_Representative = CreateOrgWithEori("Representative", "222222").MainAddress.PK;
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				instruction.CEI_OA_Warehouse2 = CreateOrgWithEori("Warehouse2", "555555").MainAddress.PK;
				instruction.CEI_OA_Warehouse = CreateOrgWithEori("Warehouse1", "666666").MainAddress.PK;

				var list = CommonLookups.EORILookup(declaration, new[] { CommonLookups.Declarant, CommonLookups.Warehouse }, null);
				NUnit.Framework.Assert.That(list.ElementsAsString, NUnit.Framework.Is.EqualTo(@"LV111111 - Declarant - Declarant
LV555555 - Warehouse - Warehouse2
LV666666 - Warehouse - Warehouse1"), "GetIdentificationNumberIsNull-ElementsAsString");

				list = CommonLookups.EORILookup(declaration, new[] { CommonLookups.Declarant, CommonLookups.Warehouse }, org => "HelloWorld");
				NUnit.Framework.Assert.That(list.ElementsAsString, NUnit.Framework.Is.EqualTo("HelloWorld - Declarant - Declarant"), "GetIdentificationNumberIsNotNull-ElementsAsString");

				list = CommonLookups.EORILookup(declaration, new[] { CommonLookups.Representative, CommonLookups.Warehouse }, org => "HelloWorld");
				NUnit.Framework.Assert.That(list.ElementsAsString, NUnit.Framework.Is.EqualTo("HelloWorld - Representative - Representative"), "GetIdentificationNumberIsNotNull-ElementsAsString");
			});
		}

		[ExpectNoExceptions]
		public void TestAddEORIToListFromOrgPk()
		{
			var result = new CodeDescriptionPairList();
			CommonLookups.AddEORIToListFromOrgPk(result, CommonLookups.Representative, CreateOrgWithEori("Header1", "111111").PK, Factory, null);
			NUnit.Framework.Assert.That(result.ElementsAsString, NUnit.Framework.Is.EqualTo("LV111111 - Representative - Header1"), "ElementsAsString");
		}

		[ExpectNoExceptions]
		public void TestAddEORIToListFromOrgPk_OrgPKIsNull()
		{
			var result = new CodeDescriptionPairList();
			CommonLookups.AddEORIToListFromOrgPk(result, CommonLookups.Representative, null, Factory, null);
			NUnit.Framework.Assert.That(result.CodesAsString, NUnit.Framework.Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison), "CodeAsString");
		}

		[ExpectNoExceptions]
		public void TestAddEORIToListFromOrgPk_GetIdentificationNumberIsNotNull()
		{
			var result = new CodeDescriptionPairList();
			CommonLookups.AddEORIToListFromOrgPk(result, CommonLookups.Representative, CreateOrgWithEori("Header1", "111111").PK, Factory, org => "HelloWorld");
			NUnit.Framework.Assert.That(result.ElementsAsString, NUnit.Framework.Is.EqualTo("HelloWorld - Representative - Header1"), "ElementsAsString");
		}

		OrgHeader CreateOrgWithEori(ZString fullName, ZString eori)
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = fullName;
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eori);
			return orgHeader;
		}
	}
}
