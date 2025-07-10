using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(ReimportCountryCode))]
	sealed class ReimportCountryCodeTestTest : CusCodeDataTest<ReimportCountryCode>
	{
		[ExpectNoExceptions]
		public void TestCY_Code()
		{
			NUnit.Framework.Assert.That(reimportCountryCode.CY_CodeInfo.MaxLength, NUnit.Framework.Is.EqualTo(2));
		}

		public void TestCY_Code_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(reimportCountryCode.CY_CodeInfo, multipleResourceKey: null, "Country/Region", "Ctry./Rgn.");
		}

		[ExpectNoExceptions]
		public void TestCountryDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eUGroup = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eUGroup);

			helper.CreateCusCodeType(Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DEReimportCountry, "I0809 CodeList  - Country list (re-import)");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DEReimportCountry, "BE", "Belgien", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DEReimportCountry, "BG", "Bulgarien", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			Factory.Save();

			reimportCountryCode.CY_Code = "BE";
			NUnit.Framework.Assert.That(reimportCountryCode.CountryDescription, NUnit.Framework.Is.EqualTo("Belgien").Using(CustomComparers.TypeComparison), "CountryDescription");

			reimportCountryCode.CY_Code = "BG";
			NUnit.Framework.Assert.That(reimportCountryCode.CountryDescription, NUnit.Framework.Is.EqualTo("Bulgarien").Using(CustomComparers.TypeComparison), "CountryDescription");
		}

		[ExpectNoExceptions]
		public void TestHumanReadableName()
		{
			NUnit.Framework.Assert.That(reimportCountryCode.HumanReadableName, NUnit.Framework.Is.EqualTo("Reimport Country/Region").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestSetDefaultValues()
		{
			NUnit.Framework.Assert.That(reimportCountryCode.CY_Type, NUnit.Framework.Is.EqualTo(CusCodeDataTypeList.Codes.ReimportCountryCode).Using(CustomComparers.TypeComparison));
		}

		protected override IEnumerable<ReimportCountryCode> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return (ReimportCountryCode)GetNewBusinessObjectForDeleteTest(factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._110000;
			return entryInstruction.ReimportCountryCodes.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			reimportCountryCode = Factory.CreateReimportCountryCode();
		}

		ReimportCountryCode reimportCountryCode;
	}
}
