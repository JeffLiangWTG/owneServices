using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.DocumentEngine.DataProviders.Testing
{
	sealed class ZExpressionEvaluatorSealedTest : TestCaseWithFactory
	{
		public void TestEvaluate_NoJS()
		{
			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEvaluate();
			}
		}

		public void TestEvaluateJS()
		{
			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEvaluate();
			}
		}

		void AssertEvaluate()
		{
			DummyBusinessObject dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_VarCharMax = "Laughing";

			Assert(ZExpressionEvaluator.Evaluate("\"<Z0_VarCharMax>\" == \"Laughing\"", dummyBO as IDocumentSupportable, BODocDataProvider.Get(dummyBO)));
			Assert(!ZExpressionEvaluator.Evaluate("\"<Z0_VarCharMax>\" == \"Crying\"", dummyBO as IDocumentSupportable, BODocDataProvider.Get(dummyBO)));
			Assert(ZExpressionEvaluator.Evaluate(string.Empty, dummyBO as IDocumentSupportable, BODocDataProvider.Get(dummyBO)));
			Assert(!ZExpressionEvaluator.Evaluate("\"Z0_VarCharMax>\" == \"Laughing\"", dummyBO as IDocumentSupportable, BODocDataProvider.Get(dummyBO)));

			var docSupportable = new Mock<IDocumentSupportable>(MockBehavior.Strict);
			var docSupporter = new Mock<DocumentSupporter>(dummyBO) { CallBase = true };

			docSupportable.Setup(m => m.DocumentSupporter).Returns(docSupporter.Object);
			docSupporter.SetupSequence(m => m.GetFilterValue(DocumentFilters.MOD)).Returns("SEA").Returns("40GP");
			docSupporter.Setup(m => m.GetFilterValue(DocumentFilters.CNT)).Returns("40GP");
			docSupporter.SetupSequence(m => m.MatchFilterValue("GREETING|HI")).Returns(true).Returns(false);
			Assert(ZExpressionEvaluator.Evaluate("MOD=SEA", docSupportable.Object, BODocDataProvider.Get(dummyBO)));
			Assert(!ZExpressionEvaluator.Evaluate("MOD=SEA", docSupportable.Object, BODocDataProvider.Get(dummyBO)));
			Assert(ZExpressionEvaluator.Evaluate("CNT=40GP", docSupportable.Object, BODocDataProvider.Get(dummyBO)));
			Assert(ZExpressionEvaluator.Evaluate("AdditionalMatch=GREETING|HI", docSupportable.Object, BODocDataProvider.Get(dummyBO)));
			Assert(!ZExpressionEvaluator.Evaluate("AdditionalMatch=GREETING|HI", docSupportable.Object, BODocDataProvider.Get(dummyBO)));
		}

		public void TestIsValidFilterEnumExpression_FilterTypeHasNotEquals()
		{
			var result = ZExpressionEvaluator.IsValidFilter("MSGBKRCTY!=IMPUS", false, out string message);
			AssertNotContains("'MSGBKRCTY!' code is incorrect.", message);
			Assert(result);
		}

		public void TestFilterExpressionIsNotMixed()
		{
			try
			{
				string message = string.Empty;
				Assert(!ZExpressionEvaluator.IsValidFilter("MSGBKRCTY=IMPUS&&\"<TransportMode>\"==\"AIR\"", false, out message));
				Assert(!ZExpressionEvaluator.IsValidFilter("MSGBKRCTY=IMPUS&&BKR=Y", false, out message));
				Assert(!ZExpressionEvaluator.IsValidFilter("\"<CurrentCompany.Country.Code>\"==\"AU\"&& MOD=AIR", false, out message));

				Assert(ZExpressionEvaluator.IsValidFilter("\"<CurrentCompany.Country.Code>\"==\"AU\"&&\"<CurrentCompany.Country.Code>\"==\"BE\"", false, out message));
				Assert(ZExpressionEvaluator.IsValidFilter("\"<TransportMode>\"==\"AIR\" && \"<TransportMode>\"==\"AIR\"", false, out message));
				Assert(ZExpressionEvaluator.IsValidFilter("\"<SubString(\"<DepartureConsol.JK_Calc_ReceivingAgentCode>\",0,6)>\"==\"AIR\"", false, out message));
				Assert(ZExpressionEvaluator.IsValidFilter("\"<SubString(\"AIRRRRRR\",0,3)>\"==\"AIR\"", false, out message));
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}
	}
}
