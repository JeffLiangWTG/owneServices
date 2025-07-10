using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.BR.Business.Testing
{
	sealed class TariffProfileTest : TestCaseWithFactory
	{
		public void TestNew_RefCusProfile()
		{
			var profile = Factory.New<RefCusProfile>();
			profile.XX0_QuestionCode = "Q123";
			var attributeMandatory = profile.Attributes.AddNew();
			attributeMandatory.XXY_Name = "Mandatory";
			attributeMandatory.XXY_Value = "true";
			var attributeLegalCode = profile.Attributes.AddNew();
			attributeLegalCode.XXY_Name = "LegalCode";
			attributeLegalCode.XXY_Value = "123";
			var attributeRegime = profile.Attributes.AddNew();
			attributeRegime.XXY_Name = "Regime";
			attributeRegime.XXY_Value = "456";
			var attributeTaxType = profile.Attributes.AddNew();
			attributeTaxType.XXY_Name = "TaxType";
			attributeTaxType.XXY_Value = "789";

			var tariffProfile = TariffProfile.New(profile);
			CombineAssertions(() =>
			{
				AssertEquals("PK should be same", profile.PK, tariffProfile.PK);
				AssertEquals("QuestionCode should be ", "Q123", tariffProfile.QuestionCode);
				AssertEquals("Regime should be ", "456", tariffProfile.Regime);
				AssertEquals("LegalCode should be ", "123", tariffProfile.LegalCode);
				AssertEquals("TaxType should be ", "789", tariffProfile.TaxType);
				AssertEquals("IsMandatory should be ", true, tariffProfile.IsMandatory);
				AssertEquals("RefCusProfile should be ", profile, tariffProfile.RefCusProfile);
			});
		}
	}
}
