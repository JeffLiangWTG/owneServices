using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class ModifiableMacrosTest : DocumentMacroTest
	{
		#region ShowInEditor

		public void TestMakeCellEditable_ShowInEditor()
		{
			AssertEditableCell("ShowInEditor(Collection, \"" + DummyBusinessObject.Schema.Z0_Description + "\")", null);
		}

		public void TestMakeCellEditable__Collection_ShowInEditor()
		{
			AssertEditableCell("ShowInEditor(" + DummyBusinessObject.Schema.Z0_Description + ", \"Description\")", null);
		}

		public void TestMakeCellEditable_InString_ShowInEditor()
		{
			AssertEditableCell("\"<ShowInEditor(Collection, \"" + DummyBusinessObject.Schema.Z0_Description + "\")>\"", string.Empty);
		}

		public void TestMakeCellEditable_ShowInEditor_ReadOnly()
		{
			var factory = new BusinessObjectFactory();

			var dummy = factory.New<DummyBusinessObject>();

			var data = dummy.MakeDynamic();

			var property = data.GetDynamicProperty(DummyBusinessObject.Schema.Z0_Description);
			property.SetReadOnly(true);

			const string macro = "ShowInEditor(" + DummyBusinessObject.Schema.Z0_Description + ", \"Description\")";

			var cell = new DummyCell();

			var macroRun = new MacroRun
			{
				Data = data,
				ExpectedResult = null,
				Variables =
					{
						{ VariableNames.CellInternal, cell }
					}
			};

			AssertMacroRun(macro, macroRun);
			AssertEquals("readonly data has been excluded from modification", 0, cell.EditableData.Count);
		}

		public void TestMakeCellEditable_ShowInEditor_Collection_ReadOnly()
		{
			var factory = new BusinessObjectFactory();

			var dummy = factory.New<DummyBusinessObject>();
			dummy.Collection.AddNew();
			dummy.Collection.AddNew();

			var data = dummy.MakeDynamic();

			var collection = (IDynamicDataCollection)data.Properties.GetOrCreate("Collection");
			collection.ElementAt(0).SetReadOnly(true);

			const string macro = "ShowInEditor(Collection, \"Z0_Description\")";

			var cell = new DummyCell();

			var macroRun = new MacroRun
			{
				Data = data,
				ExpectedResult = null,
				Variables =
					{
						{ VariableNames.CellInternal, cell }
					}
			};

			AssertMacroRun(macro, macroRun);
			AssertEquals("readonly element has been excluded from modification", 1, cell.EditableData.Count);
		}

		#endregion

		#region Modifiable

		public void TestMakeCellEditable_Modifiable()
		{
			AssertEditableCell("Modifiable(Z0_Description, \"Description\")", "Default");
		}

		public void TestMakeCellEditable_InString_Modifiable()
		{
			AssertEditableCell("\"<Modifiable(Z0_Description, \"Description\")>\"", "Default");
		}

		public void TestMakeCellEditable_Modifiable_ReadOnly()
		{
			var dummy = new DummyNonPersistentBusinessObject();
			dummy.Code = "zzz";

			var data = dummy.MakeDynamic();
			var property = data.GetDynamicProperty("Code");

			property.SetReadOnly(true);

			const string macro = "\"<Modifiable(Code, \"Code\")>\"";

			var cell = new DummyCell();

			var macroRun = new MacroRun
			{
				Data = data,
				ExpectedResult = "zzz",
				Variables =
					{
						{ VariableNames.CellInternal, cell }
					}
			};

			AssertMacroRun(macro, macroRun);
			AssertEquals("readonly data has been excluded from modification", 0, cell.EditableData.Count);
		}

		public void TestMultipleModifiablesWithoutName()
		{
			var dummy = new DummyNonPersistentBusinessObject
			{
				Description = "description",
				Code = "cde"
			};

			var data = dummy.MakeDynamic();

			const string macro = "\"<Modifiable(Code)> : <Modifiable(Description)>\"";

			var cell = new DummyCell();

			var expr = macro
				.With<DocumentLibrary>()
				.CreateExpression();

			using (var scope = new MacroScope(data))
			{
				scope.SetVariable(VariableNames.CellInternal, cell);

				expr.Evaluate(scope);
			}

			AssertContainsExactElementsInAnyOrder("Editable data",
				new[]
				{
					"Code|cde",
					"Description|description"
				},
				cell.EditableData.Select(ed => string.Format("{0}|{1}", ed.Key, ed.Value)));
		}

		public void TestMultipleModifiablesWithDuplicatedName()
		{
			var dummy = new DummyNonPersistentBusinessObject
			{
				Description = "description",
				Code = "cde"
			};

			var data = dummy.MakeDynamic();

			const string macro = "\"<Modifiable(Code, \"CustomFieldName\")> : <Modifiable(Description, \"CustomFieldName\")>\"";

			var cell = new DummyCell();

			var expr = macro
				.With<DocumentLibrary>()
				.CreateExpression();

			using (var scope = new MacroScope(data))
			{
				scope.SetVariable(VariableNames.CellInternal, cell);

				expr.Evaluate(scope);
			}

			AssertContainsExactElementsInAnyOrder("Editable data",
				new[]
				{
					"CustomFieldName|cde",
					"CustomFieldName2|description"
				},
				cell.EditableData.Select(ed => string.Format("{0}|{1}", ed.Key, ed.Value)));
		}

		public void TestMultipleModifiablesDuplicateProperties()
		{
			var dummy = new DummyNonPersistentBusinessObject
			{
				Description = "description",
				Code = "cde"
			};

			var data = dummy.MakeDynamic();

			const string macro = "\"<Modifiable(Code)> : <Modifiable(Code)>\"";

			var cell = new DummyCell();

			var expr = macro
				.With<DocumentLibrary>()
				.CreateExpression();

			using (var scope = new MacroScope(data))
			{
				scope.SetVariable(VariableNames.CellInternal, cell);

				expr.Evaluate(scope);
			}

			AssertContainsExactElementsInAnyOrder("Editable data",
				new[]
				{
					"Code|cde"
				},
				cell.EditableData.Select(ed => string.Format("{0}|{1}", ed.Key, ed.Value)));
		}

		public void TestModifiableDependingOnIfCondition()
		{
			var dummy = new DummyNonPersistentBusinessObject
			{
				Description = "description",
				Code = "AAA"
			};

			var data = dummy.MakeDynamic();

			const string macro = "\"<if Code == \"AAA\" then Modifiable(Code) else Modifiable(Description)>\"";

			var document = new DummyDocument();

			using (var scope = new MacroScope(data))
			{
				var cell = new DocumentCell(document,
					new Range(1, 1, 1, 1),
					macro,
					data,
					scope);

				cell.Evaluate();

				AssertContainsExactElementsInAnyOrder("Editable data",
					new[]
					{
						"Code|AAA"
					},
					cell.EditableData.Select(ed => string.Format("{0}|{1}", ed.Key, ed.Value)));

				data.Properties["Code"].SetValue("BBB");

				cell.Evaluate();

				AssertContainsExactElementsInAnyOrder("Editable data",
					new[]
					{
						"Description|description"
					},
					cell.EditableData.Select(ed => string.Format("{0}|{1}", ed.Key, ed.Value)));
			}
		}

		public void TestModifiableSetValue()
		{
			var dummy = new DummyNonPersistentBusinessObject
			{
				Description = "description",
				Code = "cde"
			};

			var data = dummy.MakeDynamic();

			const string macro = "\"<Modifiable(Code)>\"";

			var cell = new DummyCell();

			var expr = macro
				.With<DocumentLibrary>()
				.CreateExpression();

			using (var scope = new MacroScope(data))
			{
				scope.SetVariable(VariableNames.CellInternal, cell);

				expr.Evaluate(scope);

				AssertMultilineASCIIEquals("no errors", "", expr.ToFormatString());
				AssertNoExceptionThrown(() => data.GetDynamicProperty("Code").SetValue("xxx"));

				const string key = "0|0|Code";

				AssertEquals("Should not contains value changed subscription", false, cell.SubscribedObservers.ContainsKey(key));

				var code = data.GetDynamicProperty("Code");

				AssertNoExceptionThrown("no stack overflow exception thrown", () => code.SetValue("yyy"));
				AssertEquals("value has been set", "yyy", code.Value);
			}
		}

		#endregion

		#region Implementation

		void AssertEditableCell(string macro, object expectedResult)
		{
			var cell = new DummyCell();

			var factory = new BusinessObjectFactory();

			var dummy = factory.New<DummyBusinessObject>();
			dummy.Collection.AddNew();

			var data = dummy.MakeDynamic();

			AssertEquals("cellValue.IsEditable", false, cell.EditableData.Any());

			var macroRun = new MacroRun
				{
					Data = data,
					ExpectedResult = expectedResult,
					Variables =
					{
						{ VariableNames.CellInternal, cell }
					}
				};

			AssertMacroRun(macro, macroRun);

			AssertEquals("cellValue.IsEditable", true, cell.EditableData.Any());
		}

		#endregion
	}
}