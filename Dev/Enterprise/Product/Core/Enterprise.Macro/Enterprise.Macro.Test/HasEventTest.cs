using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Macro.Test
{
	sealed class HasEventTest : TestCaseWithFactory
	{
		public void TestHasEventMacro()
		{
			var nestedBizo = Factory.New<DummyEnterpriseBusinessObject>();
			var bizo = Factory.New<DummyBizo>();
			bizo.NestedBusinessObject = nestedBizo;

			using (var scope = new MacroScope(bizo))
			{
				var bizoExpr = "HasEvent(\"Z00\")"
					.With(Context)
					.CreateExpression();
				var bizoResult = bizoExpr.Evaluate(scope);
				Assert("BusinessObject does not have Z00 Event", !(bool)bizoResult);

				var nestedBizoExpr = "NestedBusinessObject.HasEvent(\"Z00\")"
	.With(Context)
	.CreateExpression();
				var nestedBizoResult = nestedBizoExpr.Evaluate(scope);
				Assert("NestedBusinessObject does not have Z00 Event", !(bool)nestedBizoResult);
			}

			bizo.Logs.AddNew(Events.CustomisableEvent00, "Status A", ZDateTimeOffset.Now);
			using (var scope = new MacroScope(bizo))
			{
				var bizoExpr = "HasEvent(\"Z00\")"
					.With(Context)
					.CreateExpression();
				var bizoResult = bizoExpr.Evaluate(scope);
				Assert("BusinessObject does have Z00 Event", (bool)bizoResult);

				var nestedBizoExpr = "NestedBusinessObject.HasEvent(\"Z00\")"
	.With(Context)
	.CreateExpression();
				var nestedBizoResult = nestedBizoExpr.Evaluate(scope);
				Assert("NestedBusinessObject does not have Z00 Event", !(bool)nestedBizoResult);
			}

			nestedBizo.Logs.AddNew(Events.CustomisableEvent00, "Status B", ZDateTimeOffset.Now);
			using (var scope = new MacroScope(bizo))
			{
				var bizoExpr = "HasEvent(\"Z00\")"
					.With(Context)
					.CreateExpression();
				var bizoResult = bizoExpr.Evaluate(scope);
				Assert("BusinessObject does have Z00 Event", (bool)bizoResult);

				var nestedBizoExpr = "NestedBusinessObject.HasEvent(\"Z00\")"
	.With(Context)
	.CreateExpression();
				var nestedBizoResult = nestedBizoExpr.Evaluate(scope);
				Assert("NestedBusinessObject does have Z00 Event", (bool)nestedBizoResult);
			}
		}

		public void TestHasEventMacro_NotIncludeIsEstimateOrIsCancelledEvent()
		{
			var bizo = Factory.New<DummyEnterpriseBusinessObject>();
			var log = bizo.Logs.AddNew(Events.CustomisableEvent00, "Status A", ZDateTimeOffset.Now);
			var expr = "HasEvent(\"Z00\")".With(Context).CreateExpression();

			using (var scope = new MacroScope(bizo))
			{
				Assert("Precondition", (bool)expr.Evaluate(scope));
			}
			Assert("Precondition", !log.SL_IsEstimate);
			Assert("Precondition", !log.SL_IsCancelled);

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				using (var scope = new MacroScope(bizo))
				{
					log.SL_IsEstimate = true;
					Assert("Should not show IsEstimate Event", !(bool)expr.Evaluate(scope));
					log.SL_IsEstimate = false;
					Assert("Should show IsEstimate Event", (bool)expr.Evaluate(scope));
					log.Cancel();
					Assert(log.SL_IsCancelled);
					Assert("Should not show IsCancelled Event", !(bool)expr.Evaluate(scope));
				}
			}
		}

		public void TestHasEventMacro_InvalidArgument()
		{
			var nestedBizo = Factory.New<DummyEnterpriseBusinessObject>();
			var bizo = Factory.New<DummyBizo>();
			nestedBizo.Logs.AddNew(Events.CustomisableEvent00, "Status A", ZDateTimeOffset.Now);
			var expr = "NestedBusinessObject.HasEvent(\"Z00\")".With(Context).CreateExpression();
			var emptyEventCode = "NestedBusinessObject.HasEvent(\"\")".With(Context).CreateExpression();
			var invalidEventCode = "NestedBusinessObject.HasEvent(\"ZZZ\")".With(Context).CreateExpression();

			using (var scope = new MacroScope(bizo))
			{
				Assert("Return false for null Business Object", !(bool)expr.Evaluate(scope));
				bizo.NestedBusinessObject = nestedBizo;
				Assert("Should show this event", (bool)expr.Evaluate(scope));
				Assert("Return false for Empty Event Code", !(bool)emptyEventCode.Evaluate(scope));
				Assert("Return false for Invalid Event Code", !(bool)invalidEventCode.Evaluate(scope));
			}
		}

		IMacroEvaluationContext Context
		{
			get
			{
				if (context == null)
				{
					context = new IMacroLibrary[]
					{
						new CargoWiseOneStandardLibrary()
					}
					.CreateContext();
				}

				return context;
			}
		}
		IMacroEvaluationContext context;

		public sealed class DummyBizo : DummyEnterpriseBusinessObject
		{
			public DummyBizo(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public DummyEnterpriseBusinessObject NestedBusinessObject { get; set; }
		}
	}
}
