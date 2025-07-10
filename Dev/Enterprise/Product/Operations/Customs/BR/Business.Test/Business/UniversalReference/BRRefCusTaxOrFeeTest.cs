using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class BRRefCusTaxOrFeeTest : TestCaseWithFactory
	{
		[TestDate(2023, 1, 1)]
		public void TestGetSiscomexUsageEntryFees()
		{
			ReferenceTestDataHelper.CreateSiscomexUsageEntryFees(Factory);
			var refCusTaxOrFees = BRRefCusTaxOrFee.GetSiscomexUsageEntryFees(Factory, ZDateTime.Today);
			AssertContainsExactElementsInExactOrder(new ZString[] { "ENT1", "ENT2", "ENT3", "ENT4", "ENT5", "ENT6" }, refCusTaxOrFees.Select(x => x.ZZF_Code));
			AssertSame("collection cahed", refCusTaxOrFees, BRRefCusTaxOrFee.GetSiscomexUsageEntryFees(Factory, ZDateTime.Today));
		}

		[TestDate(2023, 1, 1)]
		public void TestGetAfrmmTaxs()
		{
			ReferenceTestDataHelper.CreateAfrmmTaxes(Factory);
			var refCusTaxOrFees = BRRefCusTaxOrFee.GetAfrmmTaxs(Factory, ZDateTime.Today);
			AssertContainsExactElementsInExactOrder(new ZString[] { "FMM1", "FMM4" }, refCusTaxOrFees.Select(x => x.ZZF_Code));
			AssertSame("collection cahed", refCusTaxOrFees, BRRefCusTaxOrFee.GetAfrmmTaxs(Factory, ZDateTime.Today));
		}

		[TestDate(2023, 1, 1)]
		public void TestGetAfrmmTaxRate()
		{
			ReferenceTestDataHelper.CreateAfrmmTaxes(Factory);
			var refCusTaxOrFees = BRRefCusTaxOrFee.GetAfrmmTaxs(Factory, ZDateTime.Today);
			var listTaxRates = new List<ZDecimal>();
			foreach (var taxOrFee in refCusTaxOrFees)
			{
				var taxRate = BRRefCusTaxOrFee.GetAfrmmTaxRate(Factory, taxOrFee.ZZF_Code, ZDateTime.Today);
				listTaxRates.Add(taxRate);
			}

			AssertContainsExactElementsInExactOrder(new ZDecimal[] { 10, 16 }, listTaxRates.ToArray());

			var invalidTaxOrFee = BRRefCusTaxOrFee.GetAfrmmTaxRate(Factory, "XXXX", ZDateTime.Today);
			AssertEquals(invalidTaxOrFee, ZDecimal.Zero);
		}

		[TestDate(2023, 1, 1)]
		public void TestGetImportLicenseFees()
		{
			ReferenceTestDataHelper.CreateReferenceDataForILFFeeTypeList(Factory);

			var refCusTaxOrFees = BRRefCusTaxOrFee.GetImportLicenseFees(Factory, ZDateTime.Today);

			AssertContainsExactElementsInExactOrder(new ZString[] { "F1D5", "F1ND" }, refCusTaxOrFees.Select(x => x.ZZF_Code));
			AssertEquals("Collection length should be", 2, refCusTaxOrFees.Length);

			AssertEquals("F1D5", BRRefCusTaxOrFee.GetImportLicenseFee(Factory, "F1D5", ZDateTime.Today).ZZF_Code);
			AssertEquals("F1ND", BRRefCusTaxOrFee.GetImportLicenseFee(Factory, "F1ND", ZDateTime.Today).ZZF_Code);
			AssertNull("XXXX", BRRefCusTaxOrFee.GetImportLicenseFee(Factory, "XXXX", ZDateTime.Today));
		}
	}
}
