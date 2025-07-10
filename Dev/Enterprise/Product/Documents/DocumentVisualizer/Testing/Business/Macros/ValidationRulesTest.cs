using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class ValidationRulesTest : TestCaseWithFactory
	{
		#region IsValidUnloco

		public void TestIsValidUnloco()
		{
			var dummy = new Dummy
			{
				Code = "XXXX"
			};

			var data = dummy.MakeDynamic();

			var code = data.GetDynamicProperty("Code");

			var rules = GetRules(code);

			AssertEquals("no rules", 0, rules.Length);

			const string macro = "Code.ErrorIf(!IsValidUnloco, \"validation message\")";

			var expr = macro
				.With<MetaDataLibrary>()
				.And<DataLibrary>()
				.CreateExpression();

			using (var scope = new MacroScope(data))
			{
				expr.Evaluate(scope);
			}

			rules = GetRules(code);

			AssertEquals("one rule", 1, rules.Length);

			var validationResult = rules[0](code);

			AssertEquals("rule result", NotificationType.Error, validationResult.Type);
			AssertEquals("rule result", "validation message", validationResult.Message);

			code.SetValue("AUSYD");

			validationResult = rules[0](code);

			AssertNull("rule result", validationResult);
		}

		#endregion

		#region IsValidCountryCode

		public void TestIsValidCountryCode()
		{
			var dummy = new Dummy
			{
				Code = "XXXX"
			};

			var data = dummy.MakeDynamic();

			var code = data.GetDynamicProperty("Code");

			var rules = GetRules(code);

			AssertEquals("no rules", 0, rules.Length);

			const string macro = "Code.ErrorIf(!IsValidCountryCode, \"validation message\")";

			var expr = macro
				.With<MetaDataLibrary>()
				.And<DataLibrary>()
				.CreateExpression();

			using (var scope = new MacroScope(data))
			{
				expr.Evaluate(scope);
			}

			rules = GetRules(code);

			AssertEquals("one rule", 1, rules.Length);

			var validationResult = rules[0](code);

			AssertEquals("rule result", NotificationType.Error, validationResult.Type);
			AssertEquals("rule result", "validation message", validationResult.Message);

			code.SetValue("AU");

			validationResult = rules[0](code);

			AssertNull("rule result", validationResult);

			code.SetValue("A");

			validationResult = rules[0](code);

			AssertEquals("rule result", NotificationType.Error, validationResult.Type);
			AssertEquals("rule result", "validation message", validationResult.Message);
		}

		#endregion

		#region IsValidEmailFormat

		public void TestIsValidEmailFormat()
		{
			var dummy = new Dummy
			{
				Email = "invalid.email.com"
			};

			var data = dummy.MakeDynamic();

			var email = data.GetDynamicProperty("Email");

			var rules = GetRules(email);

			AssertEquals("no rules", 0, rules.Length);

			const string macro = "Email.ErrorIf(!IsValidEmailFormat, \"validation message\")";

			var expr = macro
				.With<MetaDataLibrary>()
				.And<DataLibrary>()
				.CreateExpression();

			using (var scope = new MacroScope(data))
			{
				expr.Evaluate(scope);
			}

			rules = GetRules(email);

			AssertEquals("one rule", 1, rules.Length);

			var validationResult = rules[0](email);

			AssertEquals("rule result", NotificationType.Error, validationResult.Type);
			AssertEquals("rule result", "validation message", validationResult.Message);

			email.SetValue("first.last@sydney.cargowise.com");

			validationResult = rules[0](email);

			AssertNull("rule result", validationResult);

			email.SetValue("AAA");

			validationResult = rules[0](email);

			AssertEquals("rule result", NotificationType.Error, validationResult.Type);
			AssertEquals("rule result", "validation message", validationResult.Message);
		}

		#endregion

		#region IsValidCurrency

		public void TestIsValidCurrency()
		{
			var dummy = new Dummy
			{
				Code = "XXX"
			};

			var data = dummy.MakeDynamic();

			var code = data.GetDynamicProperty("Code");

			var rules = GetRules(code);

			AssertEquals("no rules", 0, rules.Length);

			const string macro = "Code.ErrorIf(!IsValidCurrency, \"validation message\")";

			var expr = macro
				.With<MetaDataLibrary>()
				.And<DataLibrary>()
				.CreateExpression();

			using (var scope = new MacroScope(data))
			{
				expr.Evaluate(scope);
			}

			rules = GetRules(code);

			AssertEquals("one rule", 1, rules.Length);

			var validationResult = rules[0](code);

			AssertEquals("rule result", NotificationType.Error, validationResult.Type);
			AssertEquals("rule result", "validation message", validationResult.Message);

			code.SetValue("USD");

			validationResult = rules[0](code);

			AssertNull("rule result", validationResult);
		}

		#endregion

		#region IsNotOlderThan

		public void TestErrorIfMacro_IsNotOlderThan()
		{
			AssertIsNotOlderThanRule(NotificationType.Error);
		}

		public void TestMessageErrorIfMacro_IsNotOlderThan()
		{
			AssertIsNotOlderThanRule(NotificationType.MessageError);
		}

		public void TestWarningIfMacro_IsNotOlderThan()
		{
			AssertIsNotOlderThanRule(NotificationType.MessageError);
		}

		void AssertIsNotOlderThanRule(NotificationType notificationType)
		{
			var dummy = new Dummy
			{
				Date = ZDateTime.Today.AddDays(-50)
			};

			var data = dummy.MakeDynamic();

			var date = data.GetDynamicProperty("Date");

			var rules = GetRules(date);

			AssertEquals("no rules", 0, rules.Length);

			var macro = string.Format("Date.{0}If(!IsNotOlderThan(1), \"validation message\")", notificationType);

			var expr = macro
				.With<MetaDataLibrary>()
				.And<DataLibrary>()
				.CreateExpression();

			using (var scope = new MacroScope(data))
			{
				expr.Evaluate(scope);
			}

			rules = GetRules(date);

			AssertEquals("one rule", 1, rules.Length);

			var validationResult = rules[0](date);

			AssertEquals("rule result", notificationType, validationResult.Type);
			AssertEquals("rule result", "validation message", validationResult.Message);
		}

		public void TestIsNotOlderThanRule_InvalidDate()
		{
			var dummy = new Dummy
			{
				Date = ZDateTime.Invalid
			};

			var data = dummy.MakeDynamic();

			var date = data.GetDynamicProperty("Date");

			var rules = GetRules(date);

			AssertEquals("no rules", 0, rules.Length);

			var macro = string.Format("Date.{0}If(!IsNotOlderThan(1), \"validation message\")", NotificationType.Warning);

			var expr = macro
				.With<MetaDataLibrary>()
				.And<DataLibrary>()
				.CreateExpression();

			using (var scope = new MacroScope(data))
			{
				expr.Evaluate(scope);
			}

			rules = GetRules(date);

			AssertEquals("one rule", 1, rules.Length);

			var validationResult = rules[0](date);

			AssertEquals("rule result", NotificationType.Warning, validationResult.Type);
			AssertEquals("rule result", "validation message", validationResult.Message);
		}

		public void TestIsNotOlderThanRule_InvalidCast()
		{
			var dummy = new Dummy
			{
				Code = "42"
			};

			var data = dummy.MakeDynamic();

			var date = data.GetDynamicProperty("Code");

			var rules = GetRules(date);

			AssertEquals("no rules", 0, rules.Length);

			var macro = string.Format("Code.{0}If(!IsNotOlderThan(1), \"validation message\")", NotificationType.Warning);

			var expr = macro
				.With<MetaDataLibrary>()
				.And<DataLibrary>()
				.CreateExpression();

			using (var scope = new MacroScope(data))
			{
				expr.Evaluate(scope);
			}

			rules = GetRules(date);

			AssertEquals("one rule", 1, rules.Length);

			var validationResult = rules[0](date);

			AssertEquals("rule result", NotificationType.Warning, validationResult.Type);
			AssertEquals("rule result", "validation message", validationResult.Message);
		}

		#endregion

		#region Implementation

		class Dummy
		{
			public ZString Code { get; set; }
			public ZString Email { get; set; }
			public ZDateTime Date { get; set; }
		}

		ValidationRule[] GetRules(IDynamicData dynamicData)
		{
			return dynamicData.ValidationRules.ToArray();
		}

		#endregion
	}
}