using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.MX.Business.Testing
{
	class MXRefCusProfileTest : TestCaseWithFactory
	{
		public void TestFindByCodeAndProfileType()
		{
			ReferenceTestDataHelper.CreateIdentifiersRefCusProfileQuestions(Factory);
			var invLine = Factory.New<JobComInvoiceLine>();
			var identifier = invLine.Identifiers.AddNew();
			identifier.CSI_Code = "AI";
			AssertEquals("CO1", MXRefCusProfile.FindByQuestionCode(identifier.Questions, "CO1").XQ2_Code);
			AssertEquals("CO2", MXRefCusProfile.FindByQuestionCode(identifier.Questions, "CO2").XQ2_Code);
			identifier.CSI_Code = "AC";
			AssertEquals("CO1", MXRefCusProfile.FindByQuestionCode(identifier.Questions, "CO1").XQ2_Code);
			identifier.CSI_Code = "B2";
			AssertEquals("CO3", MXRefCusProfile.FindByQuestionCode(identifier.Questions, "CO3").XQ2_Code);
		}

		public void TestGetProfileTypes()
		{
			ReferenceTestDataHelper.CreateIdentifiersRefCusProfileQuestions(Factory);
			AssertEquals(0, MXRefCusProfile.GetProfileTypes(Factory, ZString.Empty).Length);

			var profileTypes = MXRefCusProfile.GetProfileTypes(Factory, "IDL");
			AssertContainsExactElementsInAnyOrder(new ZString[] { "AI", "AC", "B2" }, profileTypes.Select(x => x.XXX_ProfileType));
			AssertSame("cached", profileTypes, MXRefCusProfile.GetProfileTypes(Factory, "IDL"));
		}

		public void TestGetRefCusProfileQuestions()
		{
			ReferenceTestDataHelper.CreateIdentifiersRefCusProfileQuestions(Factory);
			AssertEquals(0, MXRefCusProfile.GetProfileQuestions(Factory, ZString.Empty, ZDateTime.Today).Length);

			var profileQuestions = MXRefCusProfile.GetProfileQuestions(Factory, "AI", ZDateTime.Today);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "CO1", "CO2", "CO3" }, profileQuestions.Select(x => x.XQ2_Code));
			AssertSame("cached", profileQuestions, MXRefCusProfile.GetProfileQuestions(Factory, "AI", ZDateTime.Today));

			AssertContainsExactElementsInAnyOrder(new ZString[] { "CO3" }, MXRefCusProfile.GetProfileQuestions(Factory, "B2", ZDateTime.Today).Select(x => x.XQ2_Code));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "CO1" }, MXRefCusProfile.GetProfileQuestions(Factory, "AC", ZDateTime.Today).Select(x => x.XQ2_Code));
		}
	}
}
