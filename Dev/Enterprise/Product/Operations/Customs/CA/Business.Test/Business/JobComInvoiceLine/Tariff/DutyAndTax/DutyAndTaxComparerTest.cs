using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class DutyAndTaxComparerTest : TestCaseWithFactory
	{
		public void TestComprare()
		{
			var list = new List<DutyAndTax>
						{
							GetTax("GST", "901", "10", "S", 13, "KGM", 2, "2"),
							GetTax("GST", "901", "10", "S", 13, "KGM", 2, "1"),
							GetTax("GST", "901", "10", "S", 13, "KGM", 1, ""),
							GetTax("GST", "901", "10", "S", 13, "GRM", 0, ""),
							GetTax("GST", "901", "10", "S", 12, "", 0, ""),
							GetTax("GST", "901", "10", "F", 0, "", 0, ""),
							GetTax("GST", "901", "08", "", 0, "", 0, ""),
							GetTax("GST", "001", "", "", 0, "", 0, ""),
							GetTax("DTY", "", "", "", 0, "", 0, "")
						};

			var sortedList = list.OrderBy(a => a, new DutyAndTaxComparer()).ToList();
			AssertTax(sortedList[0], "DTY", "", "", "", 0, "", 0, "");
			AssertTax(sortedList[1], "GST", "001", "", "", 0, "", 0, "");
			AssertTax(sortedList[2], "GST", "901", "08", "", 0, "", 0, "");
			AssertTax(sortedList[3], "GST", "901", "10", "F", 0, "", 0, "");
			AssertTax(sortedList[4], "GST", "901", "10", "S", 12, "", 0, "");
			AssertTax(sortedList[5], "GST", "901", "10", "S", 13, "GRM", 0, "");
			AssertTax(sortedList[6], "GST", "901", "10", "S", 13, "KGM", 1, "");
			AssertTax(sortedList[7], "GST", "901", "10", "S", 13, "KGM", 2, "1");
			AssertTax(sortedList[8], "GST", "901", "10", "S", 13, "KGM", 2, "2");
		}

		void AssertTax(DutyAndTax tax, ZString taxType, ZString code, ZString exemptCode, ZString rateType, ZDecimal rate, ZString unitOfMeasure, int tranLine, ZString tranNum)
		{
			AssertEquals("C1_TaxType", taxType, tax.C1_TaxType);
			AssertEquals("C1_Code", code, tax.C1_Code);
			AssertEquals("C1_ExemptCode", exemptCode, tax.C1_ExemptCode);
			AssertEquals("C1_RateType", rateType, tax.C1_RateType);
			AssertEquals("C1_Rate", rate, tax.C1_Rate);
			AssertEquals("C1_UnitOfMeasure", unitOfMeasure, tax.C1_UnitOfMeasure);
			AssertEquals("C1_PreviousTranLine", tranLine, tax.C1_PreviousTranLine);
			AssertEquals("C1_PreviousTranNumber", tranNum, tax.C1_PreviousTranNumber);
		}

		DutyAndTax GetTax(ZString taxType, ZString code, ZString exemptCode, ZString rateType, ZDecimal rate, ZString unitOfMeasure, int tranLine, ZString tranNum)
		{
			var tax = Factory.New<DutyAndTax>();
			using (tax.GetValidationSuspender())
			{
				tax.C1_TaxType = taxType;
				tax.C1_Code = code;
				tax.C1_ExemptCode = exemptCode;
				tax.C1_RateType = rateType;
				tax.C1_Rate = rate;
				tax.C1_UnitOfMeasure = unitOfMeasure;
				tax.C1_PreviousTranLine = tranLine;
				tax.C1_PreviousTranNumber = tranNum;
			}
			return tax;
		}
	}
}
