using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Macro.Test
{
	sealed class SharedMacrosTest : TestCaseWithFactory
	{
		public void TestContains_StringFromProperty()
		{
			var bizo = Factory.New<DummyBizo>();
			var expr = "Text.Contains(Substring)".With(Context).CreateExpression();
			bizo.Text = new ZString("one two");
			bizo.Substring = new ZString("one");
			using (var scope = new MacroScope(bizo))
			{
				Assert("ZString is not correctly evaluated", (bool)expr.Evaluate(scope));
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

			public ZString Text { get; set; }
			public ZString Substring { get; set; }
		}
	}
}
