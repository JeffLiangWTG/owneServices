using System;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(CusClassification))]
	sealed class CusClassificationTest : Customs.Business.Testing.BaseCusClassificationTest
	{
		public void TestDefaultValues()
		{
			AssertEquals("Country is set", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Classification.CC_RN_NKCountryCode);
			AssertEquals("Type is set", CusClassification.ClassificationType.Both, Classification.CC_ClassificationType);
		}

		public override void TestCC_FormattedTariffNum()
		{
			var cusClassification = Factory.New<CusClassification>();

			cusClassification.CC_FormattedTariffNum = "123456789";
			AssertEquals("CC_FormattedTariffNum", "1234.56.789", cusClassification.CC_FormattedTariffNum);

			cusClassification.CC_FormattedTariffNum = "9876.54.32 1";
			AssertEquals("CC_FormattedTariffNum", "9876.54.321", cusClassification.CC_FormattedTariffNum);
		}

		protected override Type GetExpectedTariffFormatterType => typeof(Common.TariffFormatter);
	}
}
