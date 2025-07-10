using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	class RuleConfigurationTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestWhen_RequiredRelationship()
		{
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = "INV";
				NUnit.Framework.Assert.That(ruleConfiguration.Required(entryInstruction), NUnit.Framework.Is.EqualTo(false), "Not Required");
				entryInstruction.CEI_Style = "TST";
				NUnit.Framework.Assert.That(ruleConfiguration.Required(entryInstruction), NUnit.Framework.Is.EqualTo(true), "Required");
			});
		}

		[ExpectNoExceptions]
		public void TestReturnHolderAndNumber_CalculateRelationship()
		{
			var expectedHolderPk = ZGuid.NewZGuid();
			entryInstruction.CEI_Style = "TST";
			ruleConfiguration.ReturnHolderAndNumber(e => (expectedHolderPk, "Result"));
			var (actualHolderPk, number) = ruleConfiguration.Calculate(entryInstruction).Single();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(actualHolderPk, NUnit.Framework.Is.EqualTo(expectedHolderPk), "Holder");
				NUnit.Framework.Assert.That(number, NUnit.Framework.Is.EqualTo("Result").Using(CustomComparers.TypeComparison), "Number");
			});
		}

		[ExpectNoExceptions]
		public void TestReturnHolderAndNumber_CalculateRelationshipEnumeration()
		{
			var expectedHolderPk1 = ZGuid.NewZGuid();
			var expectedHolderPk2 = ZGuid.NewZGuid();
			entryInstruction.CEI_Style = "TST";
			ruleConfiguration.ReturnHolderAndNumber(e => new[] { (new ZGuid(expectedHolderPk1), new ZString("Result1")), (new ZGuid(expectedHolderPk2), new ZString("Result2")) });
			var holderAndNumber = ruleConfiguration.Calculate(entryInstruction).ToArray();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(holderAndNumber[0].holder, NUnit.Framework.Is.EqualTo(expectedHolderPk1), "Holder");
				NUnit.Framework.Assert.That(holderAndNumber[0].number, NUnit.Framework.Is.EqualTo("Result1").Using(CustomComparers.TypeComparison), "Number");
				NUnit.Framework.Assert.That(holderAndNumber[1].holder, NUnit.Framework.Is.EqualTo(expectedHolderPk2), "Holder");
				NUnit.Framework.Assert.That(holderAndNumber[1].number, NUnit.Framework.Is.EqualTo("Result2").Using(CustomComparers.TypeComparison), "Number");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			entryInstruction = Factory.New<CusEntryInstruction>();
			ruleConfiguration = new RuleConfiguration();
			ruleConfiguration.When(e => e.CEI_Style == "TST");
		}
		CusEntryInstruction entryInstruction;
		RuleConfiguration ruleConfiguration;
	}
}
