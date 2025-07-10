using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(GlbExternalPasswordCUS))]
	sealed class GlbExternalPasswordCUSTest : GlbExternalPasswordWithPasswordTypeTest<GlbExternalPasswordCUS>
	{
		public override void TestPasswordTypeCodeAndDescription()
		{
			CombineAssertions(() =>
			{
				AssertEquals(JPPasswordType.Codes.CUS, GlbExternalPassword.PasswordTypeCode);
				AssertEquals(JPPasswordType.Descriptions.CUS, GlbExternalPassword.PasswordTypeDescription);
			});
		}

		public void TestValidationType()
		{
			AssertType<GlbExternalPasswordCUSValidation>(GlbExternalPassword.Validation);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.New<GlbExternalPasswordCUS>();
		}
	}
}
