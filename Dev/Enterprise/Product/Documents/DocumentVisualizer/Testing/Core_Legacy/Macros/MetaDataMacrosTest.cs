using System.Linq;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using ValidationRule = Enterprise.DocumentVisualizer.Core.ValidationRule;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class MetaDataMacrosTest : DocumentMacroTest
	{
		#region Closure rule

		public void TestErrorIfMacro()
		{
			AssertClosureMacro(NotificationType.Error);
		}

		public void TestMessageErrorIfMacro()
		{
			AssertClosureMacro(NotificationType.MessageError);
		}

		public void TestWarningIfMacro()
		{
			AssertClosureMacro(NotificationType.Warning);
		}

		void AssertClosureMacro(NotificationType notificationType)
		{
			var dummy = new Dummy
			{
				Description = "aaa"
			};

			var data = dummy.MakeDynamic();

			var rules = GetRules(data);

			AssertEquals("no rules", 0, rules.Length);

			var macro = string.Format("{0}If({{Description != \"bbb\"}}, \"validation message\")", notificationType);

			var expr = macro
				.With<MetaDataLibrary>()
				.CreateExpression();

			using (var scope = new MacroScope(data))
			{
				expr.Evaluate(scope);
			}

			rules = GetRules(data);

			AssertEquals("one rule", 1, rules.Length);

			var validationResult = rules[0](data);

			AssertEquals("rule result", notificationType, validationResult.Type);
			AssertEquals("rule result", "validation message", validationResult.Message);
		}

		#endregion

		#region IsValueNoneOrEmpty rule

		public void TestErrorIfMacro_IsValueNoneOrEmpty()
		{
			AssertIsValueNoneOrEmpty(NotificationType.Error);
		}

		public void TestMessageErrorIfMacro_IsValueNoneOrEmpty()
		{
			AssertIsValueNoneOrEmpty(NotificationType.Error);
		}

		public void TestWarningIfMacro_IsValueNoneOrEmpty()
		{
			AssertIsValueNoneOrEmpty(NotificationType.Error);
		}

		public void TestMessageErrorIfMacro_WithDataContainsWhiteSpace()
		{
			var dummy = new Dummy
			{
				Description = " "
			};

			var data = dummy.MakeDynamic();

			var description = data.GetDynamicProperty("Description");

			var rules = GetRules(description);

			AssertEquals("no rules", 0, rules.Length);

			const string macro = "Description.ErrorIf(IsValueNoneOrEmpty, \"validation message\")";

			var expr = macro
				.With<MetaDataLibrary>()
				.CreateExpression();

			using (var scope = new MacroScope(data))
			{
				expr.Evaluate(scope);
			}

			rules = GetRules(description);

			AssertEquals("one rule", 1, rules.Length);

			var validationResult = rules[0](description);

			AssertNotNull(validationResult);
			AssertEquals("rule result", NotificationType.Error, validationResult.Type);
			AssertEquals("rule result", "validation message", validationResult.Message);
		}

		void AssertIsValueNoneOrEmpty(NotificationType notificationType)
		{
			var dummy = new Dummy();

			var data = dummy.MakeDynamic();

			var description = data.GetDynamicProperty("Description");

			var rules = GetRules(description);

			AssertEquals("no rules", 0, rules.Length);

			var macro = string.Format("Description.{0}If(IsValueNoneOrEmpty, \"validation message\")", notificationType);

			var expr = macro
				.With<MetaDataLibrary>()
				.CreateExpression();

			using (var scope = new MacroScope(data))
			{
				expr.Evaluate(scope);
			}

			rules = GetRules(description);

			AssertEquals("one rule", 1, rules.Length);

			var validationResult = rules[0](description);

			AssertEquals("rule result", notificationType, validationResult.Type);
			AssertEquals("rule result", "validation message", validationResult.Message);
		}

		#endregion

		#region IsNotLongerThan rule

		public void TestErrorIfMacro_IsNotLongerThan()
		{
			AssertIsNotLongerThanRule(NotificationType.Error);
		}

		public void TestMessageErrorIfMacro_IsNotLongerThan()
		{
			AssertIsNotLongerThanRule(NotificationType.MessageError);
		}

		public void TestWarningIfMacro_IsNotLongerThan()
		{
			AssertIsNotLongerThanRule(NotificationType.MessageError);
		}

		void AssertIsNotLongerThanRule(NotificationType notificationType)
		{
			var dummy = new Dummy
			{
				Description = "aaa"
			};

			var data = dummy.MakeDynamic();

			var description = data.GetDynamicProperty("Description");

			var rules = GetRules(description);

			AssertEquals("no rules", 0, rules.Length);

			var macro = string.Format("Description.{0}If(!IsNotLongerThan(1), \"validation message\")", notificationType);

			var expr = macro
				.With<MetaDataLibrary>()
				.CreateExpression();

			using (var scope = new MacroScope(data))
			{
				expr.Evaluate(scope);
			}

			rules = GetRules(description);

			AssertEquals("one rule", 1, rules.Length);

			var validationResult = rules[0](description);

			AssertEquals("rule result", notificationType, validationResult.Type);
			AssertEquals("rule result", "validation message", validationResult.Message);
		}

		#endregion

		#region IsNotLongerThan rule with closure

		public void TestErrorIfMacro_IsNotLongerThan_WithClosure()
		{
			AssertIsNotLongerThan_WithClosure(NotificationType.Error);
		}

		public void TestMessageErrorIfMacro_IsNotLongerThan_WithClosure()
		{
			AssertIsNotLongerThan_WithClosure(NotificationType.MessageError);
		}

		public void TestWarningIfMacro_IsNotLongerThan_WithClosure()
		{
			AssertIsNotLongerThan_WithClosure(NotificationType.Warning);
		}

		void AssertIsNotLongerThan_WithClosure(NotificationType notificationType)
		{
			var dummy = new Dummy
			{
				Description = "aaa"
			};

			var data = dummy.MakeDynamic();

			var description = data.GetDynamicProperty("Description");

			var rules = GetRules(description);

			AssertEquals("no rules", 0, rules.Length);

			var macro = string.Format("Description.{0}If(!IsNotLongerThan({1}, 1), \"validation message\")", notificationType, "{@data}");

			var expr = macro
				.With<MetaDataLibrary>()
				.CreateExpression();

			using (var scope = new MacroScope(data))
			{
				expr.Evaluate(scope);
			}

			rules = GetRules(description);

			AssertEquals("one rule", 1, rules.Length);

			var validationResult = rules[0](description);

			AssertEquals("rule result", notificationType, validationResult.Type);
			AssertEquals("rule result", "validation message", validationResult.Message);
		}

		#endregion

		#region MatchesRegex rule

		public void TestErrorIfMacro_MatchesRegex()
		{
			AssertMatchesRegex(NotificationType.Error);
		}

		public void TestMessageErrorIfMacro_MatchesRegex()
		{
			AssertMatchesRegex(NotificationType.MessageError);
		}

		public void TestWarningIfMacro_MatchesRegex()
		{
			AssertMatchesRegex(NotificationType.Warning);
		}

		void AssertMatchesRegex(NotificationType notificationType)
		{
			var dummy = new Dummy
			{
				Description = "i'm wrong email address"
			};

			var data = dummy.MakeDynamic();

			var description = data.GetDynamicProperty("Description");

			var rules = GetRules(description);

			AssertEquals("no rules", 0, rules.Length);

			const string regex = "^([\\\\w\\\\.\\\\-]+)@([\\\\w\\\\-]+)((\\\\.(\\\\w){2,3})+)$";

			var macro = string.Format("Description.{0}If(!MatchesRegex(\"{1}\"), \"validation message\")", notificationType, regex);

			var expr = macro
				.With<MetaDataLibrary>()
				.CreateExpression();

			using (var scope = new MacroScope(data))
			{
				expr.Evaluate(scope);
			}

			rules = GetRules(description);

			AssertEquals("one rule", 1, rules.Length);

			var validationResult = rules[0](description);

			AssertEquals("rule result", notificationType, validationResult.Type);
			AssertEquals("rule result", "validation message", validationResult.Message);
		}

		#endregion

		#region HasASCIICharacters

		public void TestHasASCIICharacters_EmptyString()
		{
			AssertHasASCIICharactersNoError(string.Empty);
		}

		public void TestHasASCIICharacters_Null()
		{
			AssertHasASCIICharactersNoError(null);
		}

		public void TestHasASCIICharacters_PlainText()
		{
			AssertHasASCIICharactersNoError("hakuna matata");
		}

		public void TestErrorIfMacro_HasASCIICharacters()
		{
			AssertHasASCIICharactersHasError(NotificationType.Error);
		}

		public void TestMessageErrorIfMacro_HasASCIICharacters()
		{
			AssertHasASCIICharactersHasError(NotificationType.MessageError);
		}

		public void TestWarningIfMacro_HasASCIICharacters()
		{
			AssertHasASCIICharactersHasError(NotificationType.Warning);
		}

		void AssertHasASCIICharactersHasError(NotificationType notificationType)
		{
			AssertHasASCIICharacters(notificationType, "just some text containing \u5e72", true);
		}

		void AssertHasASCIICharactersNoError(string text)
		{
			AssertHasASCIICharacters(NotificationType.Error, text, false);
		}

		void AssertHasASCIICharacters(NotificationType notificationType, string text, bool expectHasError)
		{
			var dummy = new Dummy
			{
				Description = text
			};

			var data = dummy.MakeDynamic();

			var description = data.GetDynamicProperty("Description");

			var rules = GetRules(description);

			AssertEquals("no rules", 0, rules.Length);

			var macro = $"Description.{notificationType}If(!HasASCIICharacters, \"validation message\")";

			var expr = macro
				.With<MetaDataLibrary>()
				.CreateExpression();

			using (var scope = new MacroScope(data))
			{
				expr.Evaluate(scope);
			}

			rules = GetRules(description);

			AssertEquals("one rule", 1, rules.Length);

			var validationResult = rules[0](description);

			if (expectHasError)
			{
				AssertEquals("rule result", notificationType, validationResult.Type);
				AssertEquals("rule result", "validation message", validationResult.Message);
			}
			else
			{
				AssertNull(validationResult);
			}
		}

		#endregion

		#region Composite rule

		public void TestCompositeValidationRule_NegatePredefinedRule()
		{
			var dummy = new Dummy
			{
				Description = "aaa"
			};

			var data = dummy.MakeDynamic();

			var description = data.GetDynamicProperty("Description");

			var rules = GetRules(description);

			AssertEquals("no rules", 0, rules.Length);

			const string macro = "Description.ErrorIf(!IsValueNoneOrEmpty, \"validation message\")";

			var expr = macro
				.With<MetaDataLibrary>()
				.CreateExpression();

			using (var scope = new MacroScope(data))
			{
				expr.Evaluate(scope);
			}

			rules = GetRules(description);

			AssertEquals("one rule", 1, rules.Length);

			var validationResult = rules[0](description);

			AssertEquals("rule result", NotificationType.Error, validationResult.Type);
			AssertEquals("rule result", "validation message", validationResult.Message);
		}

		public void TestCompositeValidationRule_WithPredefinedRules()
		{
			var dummy = new Dummy
			{
				Description = "aaa"
			};

			var data = dummy.MakeDynamic();

			var description = data.GetDynamicProperty("Description");

			var rules = GetRules(description);

			AssertEquals("no rules", 0, rules.Length);

			const string macro = "Description.ErrorIf(IsValueNoneOrEmpty || !IsNotLongerThan(1), \"validation message\")";

			var expr = macro
				.With<MetaDataLibrary>()
				.CreateExpression();

			using (var scope = new MacroScope(data))
			{
				expr.Evaluate(scope);
			}

			rules = GetRules(description);

			AssertEquals("one rule", 1, rules.Length);

			var validationResult = rules[0](description);

			AssertEquals("rule result", NotificationType.Error, validationResult.Type);
			AssertEquals("rule result", "validation message", validationResult.Message);
		}

		public void TestCompositeValidationRule_WithClosureAndPredefinedRule()
		{
			var dummy = new Dummy
			{
				Description = "aaa"
			};

			var data = dummy.MakeDynamic();

			var rules = GetRules(data);

			AssertEquals("no rules", 0, rules.Length);

			const string macro = "ErrorIf(IsValueNoneOrEmpty || { Description == \"aaa\" }, \"validation message\")";

			var expr = macro
				.With<MetaDataLibrary>()
				.CreateExpression();

			using (var scope = new MacroScope(data))
			{
				expr.Evaluate(scope);
			}

			rules = GetRules(data);

			AssertEquals("one rule", 1, rules.Length);

			var validationResult = rules[0](data);

			AssertEquals("rule result", NotificationType.Error, validationResult.Type);
			AssertEquals("rule result", "validation message", validationResult.Message);
		}

		public void TestCompositeValidationRule_WithClosureAndPredefinedRuleAndScope()
		{
			var dummy = new Dummy
			{
				Code = "code",
				Description = "description",
			};

			var data = dummy.MakeDynamic();

			var rules = GetRules(data);

			AssertEquals("no rules", 0, rules.Length);

			const string macro = "def root = @data; Description.ErrorIf(IsValueNoneOrEmpty || { @root.Code == \"code\" }, \"validation message\")";

			var expr = macro
				.With<MetaDataLibrary>()
				.CreateExpression();

			using (var scope = new MacroScope(data))
			{
				expr.Evaluate(scope);
			}

			var description = data.GetDynamicProperty("Description");

			rules = GetRules(description);

			AssertEquals("one rule", 1, rules.Length);

			var validationResult = rules[0](description);

			AssertEquals("rule result", NotificationType.Error, validationResult.Type);
			AssertEquals("rule result", "validation message", validationResult.Message);
		}

		#endregion

		#region SetReadOnly

		public void TestSetReadOnly()
		{
			var dummy = new Dummy
			{
				Description = "Dummy Description"
			};

			var data = dummy.MakeDynamic();

			var description = data.GetDynamicProperty("Description");

			Assert(!description.IsReadOnly());

			const string macro = "Description.SetReadOnly(true);";

			var expr = macro
				.With<MetaDataLibrary>()
				.CreateExpression();

			using (var scope = new MacroScope(data))
			{
				expr.Evaluate(scope);
			}

			Assert(description.IsReadOnly());
		}

		#endregion

		#region TestSetNaturalKey

		public void TestSetNaturalKey()
		{
			const string macro = "NoteCollection.SetNaturalKey(\"Description\")";

			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			var data = consol.MakeDynamic();

			var notes = (IDynamicDataCollection)data.GetDynamicProperty(nameof(consol.NoteCollection));

			var expr = macro.With<MetaDataLibrary>().CreateExpression();
			expr.Evaluate(data);

			AssertEquals("notes collection hasn't been initialized", false, notes.IsInitialized);

			AssertEquals("NaturalKey has been set", "Description", notes.GetMetaData<string>(MetaDataType.NaturalKey));
		}

		public void TestSetNaturalKey_DisallowSettingOnInitializedCollection()
		{
			const string macro = "NoteCollection.SetNaturalKey(\"Description\")";

			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			var data = consol.MakeDynamic();

			var notes = (IDynamicDataCollection)data.GetDynamicProperty(nameof(consol.NoteCollection));

			AssertEquals("initialize notes collection", false, notes.Any());
			AssertEquals("notes collection has been initialized", true, notes.IsInitialized);

			var expr = macro.With<MetaDataLibrary>().CreateExpression();
			expr.Evaluate();

			AssertMultilineASCIIEquals("notifications",
				"",
				expr.ToFormatString());
		}

		#endregion

		#region Implementation

		class Dummy
		{
			public ZString Code { get; set; }
			public ZString Description { get; set; }
			public ZDateTime Date { get; set; }
		}

		ValidationRule[] GetRules(IDynamicData dynamicData)
		{
			return dynamicData.ValidationRules.ToArray();
		}

		#endregion
	}
}
