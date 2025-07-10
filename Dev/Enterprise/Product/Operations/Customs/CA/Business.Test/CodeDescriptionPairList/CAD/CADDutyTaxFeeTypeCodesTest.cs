using Enterprise.ZArchitecture.Core.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	class CADDutyTaxFeeTypeCodesTest : CodeDescriptionPairListTest
	{
		public void TestIsTaxCode()
		{
			AssertEquals(CADDutyTaxFeeTypeCodes.Codes.AAD, true, CADDutyTaxFeeTypeCodes.IsTaxCode(CADDutyTaxFeeTypeCodes.Codes.AAD));
			AssertEquals(CADDutyTaxFeeTypeCodes.Codes.AAI, true, CADDutyTaxFeeTypeCodes.IsTaxCode(CADDutyTaxFeeTypeCodes.Codes.AAI));
			AssertEquals(CADDutyTaxFeeTypeCodes.Codes.ADD, false, CADDutyTaxFeeTypeCodes.IsTaxCode(CADDutyTaxFeeTypeCodes.Codes.ADD));
			AssertEquals(CADDutyTaxFeeTypeCodes.Codes.CUD, false, CADDutyTaxFeeTypeCodes.IsTaxCode(CADDutyTaxFeeTypeCodes.Codes.CUD));
			AssertEquals(CADDutyTaxFeeTypeCodes.Codes.CVD, false, CADDutyTaxFeeTypeCodes.IsTaxCode(CADDutyTaxFeeTypeCodes.Codes.CVD));
			AssertEquals(CADDutyTaxFeeTypeCodes.Codes.EXC, false, CADDutyTaxFeeTypeCodes.IsTaxCode(CADDutyTaxFeeTypeCodes.Codes.EXC));
			AssertEquals(CADDutyTaxFeeTypeCodes.Codes.EXD, false, CADDutyTaxFeeTypeCodes.IsTaxCode(CADDutyTaxFeeTypeCodes.Codes.EXD));
			AssertEquals(CADDutyTaxFeeTypeCodes.Codes.FET, true, CADDutyTaxFeeTypeCodes.IsTaxCode(CADDutyTaxFeeTypeCodes.Codes.FET));
			AssertEquals(CADDutyTaxFeeTypeCodes.Codes.GST, true, CADDutyTaxFeeTypeCodes.IsTaxCode(CADDutyTaxFeeTypeCodes.Codes.GST));
			AssertEquals(CADDutyTaxFeeTypeCodes.Codes.INT, false, CADDutyTaxFeeTypeCodes.IsTaxCode(CADDutyTaxFeeTypeCodes.Codes.INT));
			AssertEquals(CADDutyTaxFeeTypeCodes.Codes.OTH, true, CADDutyTaxFeeTypeCodes.IsTaxCode(CADDutyTaxFeeTypeCodes.Codes.OTH));
			AssertEquals(CADDutyTaxFeeTypeCodes.Codes.PAT, true, CADDutyTaxFeeTypeCodes.IsTaxCode(CADDutyTaxFeeTypeCodes.Codes.PAT));
			AssertEquals(CADDutyTaxFeeTypeCodes.Codes.SUR, true, CADDutyTaxFeeTypeCodes.IsTaxCode(CADDutyTaxFeeTypeCodes.Codes.SUR));
			AssertEquals(CADDutyTaxFeeTypeCodes.Codes.TAC, true, CADDutyTaxFeeTypeCodes.IsTaxCode(CADDutyTaxFeeTypeCodes.Codes.TAC));
			AssertEquals(CADDutyTaxFeeTypeCodes.Codes.TOT, false, CADDutyTaxFeeTypeCodes.IsTaxCode(CADDutyTaxFeeTypeCodes.Codes.TOT));
			AssertEquals(CADDutyTaxFeeTypeCodes.Codes.VFT, false, CADDutyTaxFeeTypeCodes.IsTaxCode(CADDutyTaxFeeTypeCodes.Codes.VFT));
		}

		public void TestConvertDutyAndTaxType()
		{
			AssertEquals(CADDutyTaxFeeTypeCodes.Codes.ADD, CADDutyTaxFeeTypeCodes.ConvertDutyAndTaxType(DutyAndTaxTypes.Codes.ADD));
			AssertEquals(CADDutyTaxFeeTypeCodes.Codes.CVD, CADDutyTaxFeeTypeCodes.ConvertDutyAndTaxType(DutyAndTaxTypes.Codes.CVD));
			AssertEquals(CADDutyTaxFeeTypeCodes.Codes.GST, CADDutyTaxFeeTypeCodes.ConvertDutyAndTaxType(DutyAndTaxTypes.Codes.GST));
			AssertEquals(CADDutyTaxFeeTypeCodes.Codes.SUR, CADDutyTaxFeeTypeCodes.ConvertDutyAndTaxType(DutyAndTaxTypes.Codes.SUR));
			AssertEquals(DutyAndTaxTypes.Codes.SIMADuty, CADDutyTaxFeeTypeCodes.ConvertDutyAndTaxType(DutyAndTaxTypes.Codes.SIMADuty));
			AssertEquals(DutyAndTaxTypes.Codes.CTA, CADDutyTaxFeeTypeCodes.ConvertDutyAndTaxType(DutyAndTaxTypes.Codes.CTA));
			AssertEquals(CADDutyTaxFeeTypeCodes.Codes.FET, CADDutyTaxFeeTypeCodes.ConvertDutyAndTaxType(DutyAndTaxTypes.Codes.ExciseTax));
			AssertEquals(CADDutyTaxFeeTypeCodes.Codes.AAI, CADDutyTaxFeeTypeCodes.ConvertDutyAndTaxType(DutyAndTaxTypes.Codes.CPT));
			AssertEquals(CADDutyTaxFeeTypeCodes.Codes.CUD, CADDutyTaxFeeTypeCodes.ConvertDutyAndTaxType(DutyAndTaxTypes.Codes.CustomsDuty));
			AssertEquals(CADDutyTaxFeeTypeCodes.Codes.OTH, CADDutyTaxFeeTypeCodes.ConvertDutyAndTaxType(DutyAndTaxTypes.Codes.SAF));
		}
	}
}
