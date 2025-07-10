using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CusClassification))]
class CusClassificationTest : Customs.Business.Testing.BaseCusClassificationTest
{
	public void TestDefaultValues()
	{
		AssertEquals("Country is set", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Classification.CC_RN_NKCountryCode);
		AssertEquals("Type is set", CusClassification.ClassificationType.Both, Classification.CC_ClassificationType);
	}

	public override void TestCC_FormattedTariffNum()
	{
		CusClassification classification = Factory.New<CusClassification>();
		ZString tariff = "1234567890";
		classification.CC_FormattedTariffNum = tariff;
		AssertEquals("CC_FormattedTariffNum", "1234.5678 90", classification.CC_FormattedTariffNum);
		tariff = "9876.5432 10";
		classification.CC_FormattedTariffNum = "9876.5432 10";
		AssertEquals("CC_FormattedTariffNum", tariff, classification.CC_FormattedTariffNum);
	}

	protected override Type GetExpectedTariffFormatterType => typeof(TariffFormatterCH);

	new CusClassification Classification => (CusClassification)base.Classification;
}
