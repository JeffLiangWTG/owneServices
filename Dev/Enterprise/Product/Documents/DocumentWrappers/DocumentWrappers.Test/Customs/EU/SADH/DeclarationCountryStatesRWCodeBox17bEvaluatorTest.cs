using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.DocumentWrappers.Customs.EU.Testing
{
	[Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Core.Constants.CountryCodes.Latvia)]
	sealed class DeclarationCountryStatesRWCodeBox17bEvaluatorTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("declaration required", () => new DeclarationCountryStatesRWCodeBox17bEvaluator(null));
			AssertNoExceptionThrown("Valid declaration", () => new DeclarationCountryStatesRWCodeBox17bEvaluator(Factory.New<JobDeclaration>()));
		}

		public void TestEvaluate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKFinalDestination = "ITBRI";

			declaration.JE_MessageType = "IMP";
			var box17bEvaluator = new DeclarationCountryStatesRWCodeBox17bEvaluator(declaration);
			AssertEquals("Import case", "BA", box17bEvaluator.Evaluate());

			declaration.JE_MessageType = "EXP";
			AssertEquals("Export case", "", box17bEvaluator.Evaluate());
		}

		public void TestEvaluateWithDeclarationHavingNoFinalDestination()
		{
			var box17bEvaluator = new DeclarationCountryStatesRWCodeBox17bEvaluator(Factory.New<JobDeclaration>());
			AssertEquals("declaration", ZString.Empty, box17bEvaluator.Evaluate());
		}

		public void TestEvaluateWithDeclarationHavingInexistentDestination()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKFinalDestination = "QQQQQ";

			declaration.JE_MessageType = "IMP";
			var box17bEvaluator = new DeclarationCountryStatesRWCodeBox17bEvaluator(declaration);
			AssertEquals("inexistent province", ZString.Empty, box17bEvaluator.Evaluate());
		}
	}
}
