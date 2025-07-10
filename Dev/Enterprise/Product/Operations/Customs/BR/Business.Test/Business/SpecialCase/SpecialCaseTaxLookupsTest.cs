using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business.Testing
{
	class SpecialCaseTaxLookupsTest : TestCaseWithFactory
	{
		public void TestTaxTypeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

			var parent = invoiceLine.SpecialCaseTaxes.AddNew();
			AssertType<SpecialCaseTaxTypeList>(parent.Lookups.TaxTypeList);

			parent.TaxGroup = Constants.RateCodes.PIS;
			AssertContainsExactElementsInAnyOrder(new[] { "QPU", "RED" }, parent.Lookups.TaxTypeList.GetAllCodes());

			parent.TaxGroup = Constants.RateCodes.Antidumping;
			AssertContainsExactElementsInAnyOrder(new[] { "ADV", "QPU" }, parent.Lookups.TaxTypeList.GetAllCodes());

			parent.TaxGroup = Constants.RateCodes.Cofins;
			AssertContainsExactElementsInAnyOrder(new[] { "QPU", "RED" }, parent.Lookups.TaxTypeList.GetAllCodes());

			parent.TaxGroup = Constants.RateCodes.IPI;
			AssertContainsExactElementsInAnyOrder(new[] { "QPU", "RED" }, parent.Lookups.TaxTypeList.GetAllCodes());

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			parent.TaxGroup = Constants.RateCodes.PIS;
			AssertContainsExactElementsInAnyOrder(new[] { "ADV", "RED" }, parent.Lookups.TaxTypeList.GetAllCodes());

			parent.TaxGroup = Constants.RateCodes.Antidumping;
			AssertContainsExactElementsInAnyOrder(new[] { "ADV", "RED" }, parent.Lookups.TaxTypeList.GetAllCodes());

			parent.TaxGroup = Constants.RateCodes.Cofins;
			AssertContainsExactElementsInAnyOrder(new[] { "ADV", "RED" }, parent.Lookups.TaxTypeList.GetAllCodes());

			parent.TaxGroup = Constants.RateCodes.IPI;
			AssertContainsExactElementsInAnyOrder(new[] { "ADV", "RED" }, parent.Lookups.TaxTypeList.GetAllCodes());

			parent.TaxGroup = Constants.RateCodes.ImportDuty;
			AssertContainsExactElementsInAnyOrder(new[] { "ADV", "RED", "FTA", "MAR" }, parent.Lookups.TaxTypeList.GetAllCodes());
		}

		public void TestTaxGroupList()
		{
			ReferenceTestDataHelper.CreateRefCusRateCodeAndType(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

			var parent = invoiceLine.SpecialCaseTaxes.AddNew();
			var list = parent.Lookups.TaxGroupList;
			AssertContainsExactElementsInExactOrder(new [] { "1038", "5529", "5602", "5629" }, list.GetAllCodes());
			AssertType<CodeDescriptionPairList>(list);
			AssertSame(list, parent.Lookups.TaxGroupList);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			list = parent.Lookups.TaxGroupList;
			AssertContainsExactElementsInExactOrder(new[] { "0086", "1038", "5529", "5602", "5629" }, list.GetAllCodes());
			AssertType<CodeDescriptionPairList>(list);
			AssertSame(list, parent.Lookups.TaxGroupList);
		}
	}
}
