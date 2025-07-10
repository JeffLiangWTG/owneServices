using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(AdditionalProcedureCode))]
	class AdditionalProcedureCodeTest : Customs.Business.Testing.CusCodeDataTest<AdditionalProcedureCode>
	{
		[ExpectNoExceptions]
		public void TestHumanReadableName()
		{
			var result = Factory.NewWithValidTestData<AdditionalProcedureCode>();
			NUnit.Framework.Assert.That(result.HumanReadableName, NUnit.Framework.Is.EqualTo("Additional Procedure").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestSetDefaultValues()
		{
			var dec = Factory.New<JobDeclaration>();
			var line = dec.InvoiceLines.AddNew();
			var additionalProcedueCode = line.AdditionalProcedureCodes.AddNew();
			NUnit.Framework.Assert.That(additionalProcedueCode.CY_Type, NUnit.Framework.Is.EqualTo(CusCodeDataTypeList.Codes.AdditionalProcedureCode).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestLookups()
		{
			var dec = Factory.New<JobDeclaration>();
			var line = dec.InvoiceLines.AddNew();
			var additionalProcedueCode = line.AdditionalProcedureCodes.AddNew();
			NUnit.Framework.Assert.That(additionalProcedueCode.Lookups, NUnit.Framework.Is.TypeOf<AdditionalProcedureCodeLookups>());
		}

		[ExpectNoExceptions]
		public void TestValidation()
		{
			var dec = Factory.New<JobDeclaration>();
			var line = dec.InvoiceLines.AddNew();
			var additionalProcedueCode = line.AdditionalProcedureCodes.AddNew();
			NUnit.Framework.Assert.That(additionalProcedueCode.Validation, NUnit.Framework.Is.TypeOf<AdditionalProcedureCodeValidation>());
		}

		[ExpectNoExceptions]
		public void TestParent()
		{
			var dec = Factory.New<JobDeclaration>();
			var line = dec.InvoiceLines.AddNew();
			var additionalProcedueCode = line.AdditionalProcedureCodes.AddNew();
			NUnit.Framework.Assert.That(additionalProcedueCode.Parent, NUnit.Framework.Is.TypeOf<JobComInvoiceLine>());
		}

		[ExpectNoExceptions]
		public void TestCY_Code()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, "A", "11", "11", "111", "One", "IMP", group: "IFD");
			procedure1.ZZ6_CalculateVAT = false;
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";
			var cei = dec.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = "IFD";

			var header = dec.Invoices.AddNew();
			var line = header.InvoiceLines.AddNew();
			line.JI_ZZF_NKTaxType = "605";
			var additionalProcedueCode = line.AdditionalProcedureCodes.AddNew();
			NUnit.Framework.Assert.That(additionalProcedueCode.Lookups.CY_CodeList.ContainsCode(procedure1.FullCodeCurrentPlusPreviousPlusConcession), NUnit.Framework.Is.True, "CPC List");

			additionalProcedueCode.CY_Code = "1111111";
			NUnit.Framework.Assert.That(line.JI_ZZF_NKTaxType, NUnit.Framework.Is.EqualTo(ZString.Empty));

			line.JI_ZZF_NKTaxType = "605";
			additionalProcedueCode.CY_Code = "1111112";
			NUnit.Framework.Assert.That(line.JI_ZZF_NKTaxType, NUnit.Framework.Is.EqualTo("605").Using(CustomComparers.TypeComparison));

			procedure1.ZZ6_CalculateVAT = true;
			Factory.Save();
			additionalProcedueCode.CY_Code = "1111111";
			NUnit.Framework.Assert.That(line.JI_ZZF_NKTaxType, NUnit.Framework.Is.EqualTo("605").Using(CustomComparers.TypeComparison));
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<AdditionalProcedureCode>();
		}

		protected override IEnumerable<AdditionalProcedureCode> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var result = factory.NewWithValidTestData<AdditionalProcedureCode>();

			var declaration = factory.New<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			var line = header.InvoiceLines.AddNew();
			line.AdditionalProcedureCodes.Add(result);

			yield return result;
		}
	}
}
