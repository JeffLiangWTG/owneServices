using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.Types;
using NUnit.Framework;

namespace CargoWise.Macros.Testing
{
	sealed class MacroTest : TestCaseWithMacros
	{
		#region GetMemberValue

		public void TestGetMacroMemberValue_MacroObjectParameter()
		{
			var data = new MacroObject(new ZString());

			const string macro = "PokeMe";

			var result = macro
				.With<Lib1>()
				.CreateExpression()
				.Evaluate(data);

			AssertEquals("macro eval result", "poked!", result);
		}

		public void TestGetMacroMemberValue_MacroObjectParameter_WithScope()
		{
			var data = new MacroObject(new ZString());

			const string macro = "PokeMeWithScope";

			var result = macro
				.With<Lib1>()
				.CreateExpression()
				.Evaluate(data);

			AssertEquals("macro eval result", "scoped and poked!", result);
		}

		#endregion

		#region GetMacroFunctionValue

		public void TestGetMacroFunctionValue_WithMacroObject_NoScope()
		{
			var data = new MacroObject(new ZString());

			const string macro = "Poke(\"merrick\")";

			var result = macro
				.With<Lib2>()
				.CreateExpression()
				.Evaluate(data);

			AssertEquals("macro eval result", "merrick has been poked!", result);
		}

		public void TestGetMacroFunctionValue_NoScopeWithImplicitConversion_ZString()
		{
			var data = new TestClassC();
			data.Name = "merrick";

			const string macro = "Poke(Name)";

			var result = macro
				.With<Lib2>()
				.CreateExpression()
				.Evaluate(data);

			AssertEquals("macro eval result", "merrick has been poked!", result);
		}

		public void TestGetMacroFunctionValue_WithScopeAndWithImplicitConversion_ZString()
		{
			var data = new TestClassC();
			data.Name = "merrick";

			const string macro = "PokeWithScope(Name)";

			var result = macro
				.With<Lib2>()
				.CreateExpression()
				.Evaluate(data);

			AssertEquals("macro eval result", "merrick has been scoped and poked!", result);
		}

		public void TestGetMacroFunctionValue_NoScopeWithImplicitConversion_ZInt()
		{
			var data = new TestClassC();
			data.Number = 66;

			const string macro = "Poke(Number)";

			var result = macro
				.With<Lib2>()
				.CreateExpression()
				.Evaluate(data);

			AssertEquals("macro eval result", "poked 66 times!", result);
		}

		public void TestGetMacroFunctionValue_WithMacroObject_WithScope()
		{
			var data = new MacroObject(new ZString());

			const string macro = "PokeWithScope(\"merrick\")";

			var result = macro
				.With<Lib2>()
				.CreateExpression()
				.Evaluate(data);

			AssertEquals("macro eval result", "merrick has been scoped and poked!", result);
		}

		#endregion

		#region If

		public void TestIfExpression_LazyEvaluationOfBranches()
		{
			const string macro = "if 1==1 then Bool1 else Bool2";

			var dummy = new TestClassD
			{
				Bool1 = true,
				Bool2 = false
			};

			AssertArrayEqualsByElements("properties called",
				Array.Empty<string>(),
				dummy.PropertiesCalled.ToArray());

			using (var scope = new MacroScope(dummy))
			{
				var expr = macro.CreateExpression();
				var result = expr.Evaluate(scope);

				AssertHasNoNotifications(expr);

				AssertEquals("Expression value", true, result);

				AssertArrayEqualsByElements("properties called",
				new[] { "Bool1" },
				dummy.PropertiesCalled.ToArray());
			}
		}

		public void TestIfExpression_SuccessfullConversion()
		{
			const string macro = "if Bool1 then 1 else 2";

			var data = new TestClassD
			{
				Bool1 = false
			};

			using (var scope = new MacroScope(data))
			{
				var expr = macro.CreateExpression();
				var result = expr.Evaluate(scope);

				AssertHasNoNotifications(expr);

				AssertEquals("Expression value", 2, result);
			}
		}

		#endregion

		#region Lambda

		public void TestLambdaExpression_ScopeVariableCapture()
		{
			const string macro =
@"def var1 = ""top var"";
Children.Select(
{
	def var2 = ""select closure var"";
	def closure = {""<@data.Name>|<@var1>|<@var2>""};
	Eval(@data, @closure)
})";

			var data = new TestClassC
			{
				Children =
				{
					new TestClassC
					{
						Name = "AAA"
					},
					new TestClassC
					{
						Name = "BBB"
					}
				}
			};

			using (var scope = new MacroScope(data))
			{
				var expr = macro.With<StandardLibrary>().CreateExpression();
				var result = expr.Evaluate(scope);

				AssertHasNoNotifications(expr);

				var enumerable = (IEnumerable<object>)result;

				AssertContainsExactElementsInAnyOrder("",
					new[]
					{
						"AAA|top var|select closure var",
						"BBB|top var|select closure var"
					},
					enumerable);
			}
		}

		public void TestLambdaExpression_ScopeVariableCapture_DeclareConfictingVariable()
		{
			const string macro =
@"def var1 = ""top var"";
Children.Select(
{
	def var2 = ""select closure var"";
	def closure = {def var2=""overriden var"";""<@data.Name>|<@var1>|<@var2>""};
	Eval(@data, @closure)
})";

			var data = new TestClassC
			{
				Children =
				{
					new TestClassC
					{
						Name = "AAA"
					},
					new TestClassC
					{
						Name = "BBB"
					}
				}
			};

			using (var scope = new MacroScope(data))
			{
				var expr = macro.With<StandardLibrary>().CreateExpression();
				var result = expr.Evaluate(scope);

				AssertHasNoNotifications(expr);

				var enumerable = (IEnumerable<object>)result;

				AssertContainsExactElementsInAnyOrder("",
					new[]
					{
						"AAA|top var|overriden var",
						"BBB|top var|overriden var"
					},
					enumerable);
			}
		}

		#endregion

		#region Handler matching

		[ExpectNoExceptions]
		public void TestFindBestMatch()
		{
			var metaData = new MacroMetaData("Poke");

			Expression<Func<int, string, decimal, int>> expr1 = (a, b, c) => 1;
			Expression<Func<IComparable, string, decimal, int>> expr2 = (a, b, c) => 2;
			Expression<Func<int, ZString, decimal, int>> expr3 = (a, b, c) => 3;
			Expression<Func<int, string, ZDecimal, int>> expr4 = (a, b, c) => 4;
			Expression<Func<int, ZString, ZDecimal, int>> expr5 = (a, b, c) => 5;

			var handler1 = new MacroEvaluationHandler(expr1);
			var handler2 = new MacroEvaluationHandler(expr2);
			var handler3 = new MacroEvaluationHandler(expr3);
			var handler4 = new MacroEvaluationHandler(expr4);
			var handler5 = new MacroEvaluationHandler(expr5);

			metaData.EvaluationHandlers.Add(handler1);
			metaData.EvaluationHandlers.Add(handler2);
			metaData.EvaluationHandlers.Add(handler3);
			metaData.EvaluationHandlers.Add(handler4);
			metaData.EvaluationHandlers.Add(handler5);

			AssertBestMatch(metaData, new object[] { 1, "", 1.2m }, handler1);
			AssertBestMatch(metaData, new object[] { "1", "", 1.2m }, handler2);
			AssertBestMatch(metaData, new object[] { 1, (ZString)"", 1.2m }, handler3);
			AssertBestMatch(metaData, new object[] { 1, "", (ZDecimal)1.2m }, handler4);
			AssertBestMatch(metaData, new object[] { 1, (ZString)"", (ZDecimal)1.2m }, handler5);

			AssertBestMatch(metaData, new object[] { null, null, null }, handler2);
			AssertBestMatch(metaData, new object[] { (ZInt)1, (ZString)"", (ZDecimal)1.2m }, handler5);
		}

		void AssertBestMatch(MacroMetaData metaData, object[] paramtersToMatch, MacroEvaluationHandler expected)
		{
			var matched = metaData.FindBestMatchingHandler(paramtersToMatch);

			if (matched != expected)
			{
				var parametersDesc = string.Join(",", paramtersToMatch.Select(MacroExtensions.ToDebugString));

				var message = string.Format("Given the following parameters: {0}\r\nmatched: {1}\r\nbut expected: {2}",
					parametersDesc,
					matched.ToDebugString(),
					expected.ToDebugString());

				Fail(message);
			}
		}

		#endregion

		#region Implementation

		void AssertHasNoNotifications(IMacroExpression expr)
		{
			var notifications = expr.ToFormatString();

			AssertMultilineASCIIEquals(string.Format("Expression '{0}' has no notification errors", expr.Text),
				"", notifications);
		}

		class Lib1 : MacroLibrary
		{
			protected override IEnumerable<IHandler> MacroHandlers
			{
				get
				{
					yield return new Handler<Func<string>>("PokeMe", "", () => "poked!", "PokeMe()");
					yield return new Handler<Func<IMacroScope, string>>("PokeMeWithScope", "", (scope) => "scoped and poked!", "PokeMeWithScope()");
				}
			}
		}

		class Lib2 : MacroLibrary
		{
			protected override IEnumerable<IHandler> MacroHandlers
			{
				get
				{
					yield return new Handler<Func<string, string>>(
						"Poke",
						"",
						name => string.Format("{0} has been poked!", name),
						"name.Poke()");

					yield return new Handler<Func<int, string>>(
						"Poke",
						"",
						numberOfTimes => string.Format("poked {0} times!", numberOfTimes),
						"Number.Poke()");

					yield return new Handler<Func<IMacroScope, string, string>>(
						"PokeWithScope",
						"",
						(scope, name) => string.Format("{0} has been scoped and poked!", name),
						"PokeWithScope()");
				}
			}
		}

		class TestClassC
		{
			public virtual string TestClassCProperty { get; set; }

			public List<TestClassC> Children
			{
				get { return children; }
			}

			readonly List<TestClassC> children = new List<TestClassC>();

			public string this[string name]
			{
				get { return "hello " + name; }
			}

			public string this[string name, int index]
			{
				get { return "hello " + name + " from multidimensional at " + index; }
			}

			[MacroInvokable]
			public object Func()
			{
				return "func";
			}

			[MacroInvokable]
			public object Func(int val)
			{
				return "func " + val;
			}

			[MacroInvokable]
			public object Func(string val, bool optional = false)
			{
				return "func " + val + " " + optional;
			}

			[MacroInvokable]
			public object Func2(string val, TestClassC optional = null)
			{
				return "func2 " + val + " " + (optional?.ToString() ?? "nada");
			}

			[MacroInvokable]
			public object Func3(string val, string str)
			{
				return "func3 " + val + " " + (str ?? "nada");
			}

			[MacroInvokable]
			public object Return(object objToReturn)
			{
				return objToReturn;
			}

			public ZString Name { get; set; }
			public ZInt Number { get; set; }
		}

		class TestClassD
		{
			public ZBool Bool1
			{
				get
				{
					propertiesCalled.Add("Bool1");
					return bool1;
				}
				set { bool1 = value; }
			}

			ZBool bool1;

			public ZBool Bool2
			{
				get
				{
					propertiesCalled.Add("Bool2");
					return bool2;
				}
				set { bool2 = value; }
			}

			ZBool bool2;

			public IEnumerable<string> PropertiesCalled
			{
				get { return propertiesCalled; }
			}

			readonly List<string> propertiesCalled = new List<string>();
		}

		#endregion
	}
}