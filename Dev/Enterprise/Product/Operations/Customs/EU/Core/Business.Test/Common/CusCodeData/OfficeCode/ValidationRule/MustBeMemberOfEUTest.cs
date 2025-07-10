using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(MustBeMemberOfEU))]
	class MustBeMemberOfEUTest : ValidationRuleAbstractTest<MustBeMemberOfEU>
	{
		[ExpectNoExceptions]
		public override void TestIsApplied() => NUnit.Framework.Assert.That(Rule.IsApplied, NUnit.Framework.Is.EqualTo(true));

		[ExpectNoExceptions]
		public override void TestValidate()
		{
			CombineAssertions(() =>
			{
				var result = Rule.Validate(country);
				NUnit.Framework.Assert.That(result.IsValid, NUnit.Framework.Is.EqualTo(false), "IsValid");
				NUnit.Framework.Assert.That(result.Message, NUnit.Framework.Does.Contain("is not listed as being in the European Union. Check the value of your office code or check that your list of economic groupings is up to date."), "Message");
			});
		}

		protected override MustBeMemberOfEU Rule => new MustBeMemberOfEU();

		protected override void SetUp()
		{
			base.SetUp();
			country = Factory.New<RefCountry>();
			country.Code = Core.Constants.CountryCodes.Switzerland;
		}
		RefCountry country;
	}
}
