using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRRefCusProfileTest : TestCaseWithFactory
	{
		public void TestIsMandatory()
		{
			var profile = Factory.New<RefCusProfile>();
			var attribute = profile.Attributes.AddNew();
			Assert("IsMandatory should be FALSE", !profile.IsMandatory());

			attribute.XXY_Name = "Mandatory";
			Assert("IsMandatory should be FALSE", !profile.IsMandatory());

			attribute.XXY_Value = "err";
			Assert("IsMandatory should be FALSE", !profile.IsMandatory());

			attribute.XXY_Value = "false";
			Assert("IsMandatory should be FALSE", !profile.IsMandatory());

			attribute.XXY_Value = "true";
			Assert("IsMandatory should be TRUE", profile.IsMandatory());
		}

		public void TestGetLegalCode()
		{
			var profile = Factory.New<RefCusProfile>();
			var attribute = profile.Attributes.AddNew();
			Assert("GetLegalCode should be Empty", profile.GetLegalCode().IsEmpty);

			attribute.XXY_Name = "LegalCode";
			Assert("GetLegalCode should be Empty", profile.GetLegalCode().IsEmpty);

			attribute.XXY_Value = "123";
			AssertEquals("GetLegalCode should be", "123", profile.GetLegalCode());
		}

		public void TestGetRegime()
		{
			var profile = Factory.New<RefCusProfile>();
			var attribute = profile.Attributes.AddNew();
			Assert("GetRegime should be Empty", profile.GetRegime().IsEmpty);

			attribute.XXY_Name = "Regime";
			Assert("GetRegime should be Empty", profile.GetRegime().IsEmpty);

			attribute.XXY_Value = "123";
			AssertEquals("GetRegime should be", "123", profile.GetRegime());
		}

		public void TestGetTaxType()
		{
			var profile = Factory.New<RefCusProfile>();
			var attribute = profile.Attributes.AddNew();
			Assert("GetTaxType should be Empty", profile.GetTaxType().IsEmpty);

			attribute.XXY_Name = "TaxType";
			Assert("GetTaxType should be Empty", profile.GetTaxType().IsEmpty);

			attribute.XXY_Value = "123";
			AssertEquals("GetTaxType should be", "123", profile.GetTaxType());
		}

		public void TestGetProfileQuestions()
		{
			var profiles = ReferenceTestDataHelper.CreateTTRefCusProfileAndQuestions(Factory);

			AssertEquals(0, BRRefCusProfile.GetProfileQuestions(Array.Empty<RefCusProfile>(), ZDateTime.Now).Length);
			AssertEquals(13, BRRefCusProfile.GetProfileQuestions(profiles.ToArray(), ZDateTime.Now.AddDays(-2)).Length);

			AssertContainsExactElementsInAnyOrder(new[] { "ATT1", "ATT1", "ATT2" }, BRRefCusProfile.GetProfileQuestions(profiles.Where(s => s.XX0_QuestionCode == "ATT1" || s.XX0_QuestionCode == "ATT2").ToArray(), ZDateTime.Now).Select(x => x.Code));
		}
	}
}
