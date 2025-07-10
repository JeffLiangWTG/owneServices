using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(PermitCountrySpecificInstruction))]
	sealed class PermitCountrySpecificInstructionTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestIsQtyValIndicatorMandatory()
		{
			var instruction = PermitCountrySpecificInstruction.GetByCountryCode(Factory, Core.Constants.CountryCodes.EuropeanUnion);
			NUnit.Framework.Assert.That(instruction.IsQtyValIndicatorMandatory, NUnit.Framework.Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestGetFullTypeList_ReturnType()
		{
			var permitHeader = Factory.New<CusPermitHeader>();
			var countrySpecificInstruction = permitHeader.CountrySpecificInstruction;
			NUnit.Framework.Assert.That(countrySpecificInstruction.GetFullTypeList(permitHeader.CPH_RN_NKCountryCode), NUnit.Framework.Is.TypeOf<ZZRefCusCodeListCombinedCollection>());
		}

		public void TestGetFullTypeList_ShouldLoadThoseHavePermitAttributeDefined()
		{
			Factory.CreateSupportingDocumentCodeLists(new TestSupportingDocumentCodeList("2800", hasPermitAttribute: true), new TestSupportingDocumentCodeList("3200", hasPermitAttribute: false));
			Factory.Save();

			var permit = Factory.New<CusPermitHeader>();
			var fullTypeList = permit.CountrySpecificInstruction.GetFullTypeList(permit.CPH_RN_NKCountryCode);
			fullTypeList.Load();
			AssertContainsExactElementsInAnyOrder("It should only load those have permit attribute defined.", new[]
			{
				"2800"
			}, fullTypeList.Select(x => x.ZZD_Code));
		}

		public void TestGetFullTypeList_ShouldLoadAllIfNoOneHasPermitAttributeDefined()
		{
			Factory.CreateSupportingDocumentCodeLists(new TestSupportingDocumentCodeList("2800", hasPermitAttribute: false), new TestSupportingDocumentCodeList("3200", hasPermitAttribute: false));
			Factory.Save();

			var permit = Factory.New<CusPermitHeader>();
			var fullTypeList = permit.CountrySpecificInstruction.GetFullTypeList(permit.CPH_RN_NKCountryCode);
			fullTypeList.Load();
			AssertContainsExactElementsInAnyOrder("It should load all records only if no one has permit attribute defined.", new[]
			{
				"2800", "3200"
			}, fullTypeList.Select(x => x.ZZD_Code));
		}
	}
}
