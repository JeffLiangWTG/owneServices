using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	class AdditionalProcedureCodeLookupsTest : BusinessObjectLookupsTestCase
	{
		class CusClassPartPivotForTest : CusClassPartPivot
		{
			public CusClassPartPivotForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override ZString DataGroupingCodeForAdditionalProcedures => "CDS";
		}

		[ExpectNoExceptions]
		public void TestCY_CodeList()
		{
			var additionalProcedureCode = Factory.New<AdditionalProcedureCode>();
			var list = additionalProcedureCode.Lookups.CY_CodeList;
			NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(0));
		}

		[ExpectNoExceptions]
		public void TestCY_CodeList_JobComInvoiceLine_MainProcedureCodeIsEmpty()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure2 = helper.CreateRefCusProcedure(currentCountry, "A", "11", "11", "222", "Two", "IMP", group: "IFD");
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, "A", "11", "11", "111", "One", "IMP", group: "IFD");
			var procedure3 = helper.CreateRefCusProcedure(currentCountry, "B", "33", "33", "333", "Three", "IMP", group: "ICR");
			var procedure4 = helper.CreateRefCusProcedure(currentCountry, "A", "44", "44", "444", "Four", "EXP", group: "EFD");
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";
			var cei = dec.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = "IFD";
			var invLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
			invLine.JI_RN_NKCountryOfExport = currentCountry;

			var additionalProcedureCode = invLine.AdditionalProcedureCodes.AddNew();
			var list = additionalProcedureCode.Lookups.CY_CodeList;
			NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(2), "CPC List should have procedure codes filtered by shipmentType & declarationType");
			NUnit.Framework.Assert.That(list[0].Code, NUnit.Framework.Is.EqualTo("1111111"), "CPC List Sorted");
			NUnit.Framework.Assert.That(list[1].Code, NUnit.Framework.Is.EqualTo("1111222"), "CPC List Sorted");
			NUnit.Framework.Assert.That(additionalProcedureCode.Lookups.CY_CodeList, NUnit.Framework.Is.SameAs(list), "Cached");
		}

		[ExpectNoExceptions]
		public void TestCY_CodeList_JobComInvoiceLine_MainProcedureCodeIsNotEmpty()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure2 = helper.CreateRefCusProcedure(currentCountry, "A", "11", "11", "222", "Two", "IMP", group: "IFD");
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, "A", "11", "11", "111", "One", "IMP", group: "IFD");
			var procedure3 = helper.CreateRefCusProcedure(currentCountry, "B", "33", "33", "333", "Three", "IMP", group: "ICR");
			var procedure4 = helper.CreateRefCusProcedure(currentCountry, "A", "44", "44", "444", "Four", "EXP", group: "EFD");
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";
			var cei = dec.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = "IFD";
			var invLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
			invLine.JI_RN_NKCountryOfExport = currentCountry;
			invLine.JI_Procedure = "1111111";

			var additionalProcedureCode = invLine.AdditionalProcedureCodes.AddNew();
			var list = additionalProcedureCode.Lookups.CY_CodeList;
			NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(list.ContainsCode(procedure2.FullCodeCurrentPlusPreviousPlusConcession), NUnit.Framework.Is.True, "CPC List");
			NUnit.Framework.Assert.That(additionalProcedureCode.Lookups.CY_CodeList, NUnit.Framework.Is.SameAs(list), "Cached");
		}

		[ExpectNoExceptions]
		public void TestCY_CodeList_CusClassPartPivot()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedureCDS2 = helper.CreateRefCusProcedure("CDS", "A", "11", "11", "202", "Two", "IMP", group: "IFD");
			var procedureCDS1 = helper.CreateRefCusProcedure("CDS", "A", "11", "11", "101", "One", "IMP", group: "IFD");
			var procedureCDS3 = helper.CreateRefCusProcedure("CDS", "B", "33", "33", "303", "Three", "IMP", group: "ICR");
			var procedureCDS4 = helper.CreateRefCusProcedure("CDS", "A", "44", "44", "404", "Four", "EXP", group: "EFD");

			Factory.Save();

			var pivot = Factory.New<CusClassPartPivotForTest>();
			pivot.CI_ChildType = "IMP";
			pivot.CI_CPC = "1111111";
			var additionalProcedureCode = pivot.AdditionalProcedureCodes.AddNew();
			var list = additionalProcedureCode.Lookups.CY_CodeList;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(2), "Count");
				NUnit.Framework.Assert.That(list[0].Code, NUnit.Framework.Is.EqualTo("1111101"), "CPC List Sorted");
				NUnit.Framework.Assert.That(list[1].Code, NUnit.Framework.Is.EqualTo("1111202"), "CPC List Sorted");
				NUnit.Framework.Assert.That(additionalProcedureCode.Lookups.CY_CodeList, NUnit.Framework.Is.SameAs(list), "Cached");
			});
		}
	}
}
