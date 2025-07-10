using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using MetaDataType = Enterprise.DocumentVisualizer.Core.MetaDataType;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	sealed class DynamicContentEditorBuilderTest : TestCaseWithFactory
	{
		#region TestBuild_PlainTextBox

		public void TestBuild_PlainTextBox()
		{
			var dummy = new Dummy(Factory)
			{
				Shipper = new Dummy(Factory)
				{
					Name = "Bob"
				}
			};

			var data = dummy.MakeDynamic();

			var shipperName = data.GetDynamicProperty("Shipper.Name");

			AssertEquals("DynamicData created object graph", "Bob", shipperName.Value);

			var properties = new []
			{
				new MacroBusinessObjectProperty("Name", shipperName)
			};

			var cell = new DummyCell();
			var element = new DynamicContent(cell, new PointF(0f, 0f), new SizeF(10f, 10f));
			var content = new DynamicContentLayoutElement(element);
			var eventHandler = new DummyEditorPresenter();

			var builder = new DynamicContentEditorBuildService();

			var editor = builder.Build(content, eventHandler, properties, 1f);
			AssertNotNull("editor created", editor);

			using (var form = new ZForm())
			{
				var contentParent = (Control)editor.Control;

				AssertNotNull("content parent found", contentParent);

				form.Controls.Add(contentParent);
				form.Show();

				AssertContainsExactElementsInAnyOrder("content controls",
					new[]
					{
@"Type: VisualizerTextBox
Text: Bob
DataBindings: 
prop_Name愛ReadOnly
prop_Name愛MaxLength
prop_Name"
					},
					Format(contentParent.Controls));
			}
		}

		#endregion

		#region TestBuild_DropEdit

		public void TestBuild_DropEdit()
		{
			var dummy = new Dummy(Factory)
			{
				Shipper = new Dummy(Factory)
				{
					Type = "AAA"
				}
			};

			var metaDataProvider = new MetaDataProviderForTest();
			metaDataProvider.GetMetaDataImplementer = (data, type) =>
			{
				if (type == MetaDataType.ListDataSource)
				{
					return new CodeDescriptionPairList();
				}

				return null;
			};

			var dynamicData = dummy.MakeDynamic(metaDataProvider);
			var shipperType = dynamicData.GetDynamicProperty("Shipper.Type");

			var properties = new[]
			{
				new MacroBusinessObjectProperty("Type", shipperType)
			};

			var cell = new DummyCell();
			var element = new DynamicContent(cell, new PointF(0f, 0f), new SizeF(10f, 10f));
			var content = new DynamicContentLayoutElement(element);
			var eventHandler = new DummyEditorPresenter();

			var builder = new DynamicContentEditorBuildService();

			var editor = builder.Build(content, eventHandler, properties, 1f);
			AssertNotNull("editor created", editor);

			using (var form = new ZForm())
			{
				var contentParent = (Control)editor.Control;

				AssertNotNull("content parent found", contentParent);

				form.Controls.Add(contentParent);
				form.Show();

				Assert(contentParent.Controls.Cast<Control>().FirstOrDefault() is VisualizerDropEdit);
			}
		}

		public void TestBuild_DropEdit_OnlyShowCodesWhenAllDescriptionsAreEmpty()
		{
			var dummy = new Dummy(Factory)
			{
				Shipper = new Dummy(Factory)
				{
					Type = "AAA"
				}
			};

			var metaDataProvider = new MetaDataProviderForTest();
			metaDataProvider.GetMetaDataImplementer = (data, type) =>
			{
				if (type == MetaDataType.ListDataSource)
				{
					var list = new CodeDescriptionPairList();
					list.Add(new CodeDescriptionPair("AAA", ""));
					list.Add(new CodeDescriptionPair("BBB", ""));

					return list;
				}

				return null;
			};

			var dynamicData = dummy.MakeDynamic(metaDataProvider);
			var shipperType = dynamicData.GetDynamicProperty("Shipper.Type");

			var properties = new[]
			{
				new MacroBusinessObjectProperty("Type", shipperType)
			};

			var cell = new DummyCell();
			var element = new DynamicContent(cell, new PointF(0f, 0f), new SizeF(10f, 10f));
			var content = new DynamicContentLayoutElement(element);
			var eventHandler = new DummyEditorPresenter();

			var builder = new DynamicContentEditorBuildService();

			var editor = builder.Build(content, eventHandler, properties, 1f);
			AssertNotNull("editor created", editor);

			using (var form = new ZForm())
			{
				var contentParent = (Control)editor.Control;

				AssertNotNull("content parent found", contentParent);

				form.Controls.Add(contentParent);
				form.Show();

				var dropEditor = contentParent.Controls.Cast<Control>().FirstOrDefault() as VisualizerDropEdit;
				AssertNotNull(dropEditor);
				AssertEquals(ZDropEdit.ShowInDropDownList.OnlyShowCode, dropEditor.ShowInDropDown);
			}
		}

		#endregion

		#region TestBuild_DropDown

		public void TestBuild_DropDown()
		{
			var dummy = new Dummy(Factory)
			{
				Shipper = new Dummy(Factory)
				{
					Type = "AAA"
				}
			};

			var data = dummy.MakeDynamic();

			var shipperType = data.GetDynamicProperty("Shipper.Type");

			AssertEquals("DynamicData created object graph", "AAA", shipperType.Value);

			var properties = new[]
			{
				new MacroBusinessObjectProperty("Type", shipperType)
			};

			var cell = new DummyCell();
			var element = new DynamicContent(cell, new PointF(0f, 0f), new SizeF(10f, 10f));
			var content = new DynamicContentLayoutElement(element);
			var eventHandler = new DummyEditorPresenter();

			var builder = new DynamicContentEditorBuildService();

			using (var form = new ZForm())
			{
				var editor = builder.Build(content, eventHandler, properties, 1f);
				AssertNotNull("editor created", editor);

				var contentParent = (Control)editor.Control;

				AssertNotNull("content parent found", contentParent);

				form.Controls.Add(contentParent);
				form.Show();

				AssertContainsExactElementsInAnyOrder("content controls",
					new[]
					{
@"Type: VisualizerTextBox
Text: AAA
DataBindings: 
prop_Type愛ReadOnly
prop_Type愛MaxLength
prop_Type",
					},
					Format(contentParent.Controls));
			}
		}

		#endregion

		#region TestBuild_CodeLookup

		public void TestBuild_CodeLookup()
		{
			var dummy = new Dummy(Factory)
			{
				Shipper = new Dummy(Factory)
				{
					Unloco = "AUSYD"
				}
			};

			var data = dummy.MakeDynamic();

			var shipperUnloco = data.GetDynamicProperty("Shipper.Unloco");

			AssertEquals("DynamicData created object graph", "AUSYD", shipperUnloco.Value);

			var properties = new[]
			{
				new MacroBusinessObjectProperty("Unloco", shipperUnloco)
			};

			var cell = new DummyCell();
			var element = new DynamicContent(cell, new PointF(0f, 0f), new SizeF(10f, 10f));
			var content = new DynamicContentLayoutElement(element);
			var eventHandler = new DummyEditorPresenter();

			var builder = new DynamicContentEditorBuildService();

			using (var form = new ZForm())
			{
				var editor = builder.Build(content, eventHandler, properties, 1f);
				AssertNotNull("editor created", editor);

				var contentParent = (Control)editor.Control;

				AssertNotNull("content parent found", contentParent);

				form.Controls.Add(contentParent);
				form.Show();

				AssertContainsExactElementsInAnyOrder("content controls",
					new[]
					{
@"Type: VisualizerTextBox
Text: AUSYD
DataBindings: 
prop_Unloco愛ReadOnly
prop_Unloco愛MaxLength
prop_Unloco",
					},
					Format(contentParent.Controls));
			}
		}

		#endregion

		#region TestBuild_CheckBox

		public void TestBuild_CheckBox()
		{
			var dummy = new Dummy(Factory)
			{
				Shipper = new Dummy(Factory)
				{
					IsBlue = true
				}
			};

			var data = dummy.MakeDynamic();

			var shipperIsBlue = data.GetDynamicProperty("Shipper.IsBlue");

			AssertEquals("DynamicData created object graph", true, shipperIsBlue.Value);

			var properties = new[]
			{
				new MacroBusinessObjectProperty("IsBlue", shipperIsBlue)
			};

			var cell = new DummyCell();
			var element = new DynamicContent(cell, new PointF(0f, 0f), new SizeF(10f, 10f));
			var content = new DynamicContentLayoutElement(element);
			var eventHandler = new DummyEditorPresenter();

			var builder = new DynamicContentEditorBuildService();

			var editor = builder.Build(content, eventHandler, properties, 1f);
			AssertNotNull("editor created", editor);

			var contentParent = (Control)editor.Control;
			AssertNull("there's no control for bools", contentParent);
		}

		#endregion

		#region TestBuild_DecimalEditor

		public void TestBuild_DecimalEditor()
		{
			var dummy = new Dummy(Factory)
			{
				Shipper = new Dummy(Factory)
				{
					Salary = 1234.56
				}
			};

			var data = dummy.MakeDynamic();

			var shipperSalary = data.GetDynamicProperty("Shipper.Salary");

			AssertEquals("DynamicData created object graph", new ZDecimal(1234.56), shipperSalary.Value);

			var properties = new[]
			{
				new MacroBusinessObjectProperty("Salary", shipperSalary)
			};

			var cell = new DummyCell();
			var element = new DynamicContent(cell, new PointF(0f, 0f), new SizeF(10f, 10f));
			var content = new DynamicContentLayoutElement(element);
			var eventHandler = new DummyEditorPresenter();

			var builder = new DynamicContentEditorBuildService();

			using (var form = new ZForm())
			{
				var editor = builder.Build(content, eventHandler, properties, 1f);
				AssertNotNull("editor created", editor);

				var contentParent = (Control)editor.Control;

				AssertNotNull("content parent found", contentParent);

				form.Controls.Add(contentParent);
				form.Show();

				AssertContainsExactElementsInAnyOrder("content controls",
					new[]
					{
@"Type: ZCalcEdit
Text: 1,234.56
DataBindings: 
prop_Salary愛ReadOnly
prop_Salary"
					},
					Format(contentParent.Controls));
			}
		}

		#endregion

		#region TestBuild_NumberEditor

		public void TestBuild_NumberEditor()
		{
			var dummy = new Dummy(Factory)
			{
				Shipper = new Dummy(Factory)
				{
					Age = 21
				}
			};

			var data = dummy.MakeDynamic();

			var shipperAge = data.GetDynamicProperty("Shipper.Age");

			AssertEquals("DynamicData created object graph", 21, shipperAge.Value);

			var properties = new[]
			{
				new MacroBusinessObjectProperty("Age", shipperAge)
			};

			var cell = new DummyCell();
			var element = new DynamicContent(cell, new PointF(0f, 0f), new SizeF(10f, 10f));
			var content = new DynamicContentLayoutElement(element);
			var eventHandler = new DummyEditorPresenter();

			var builder = new DynamicContentEditorBuildService();

			using (var form = new ZForm())
			{
				var editor = builder.Build(content, eventHandler, properties, 1f);
				AssertNotNull("editor created", editor);

				var contentParent = (Control)editor.Control;

				AssertNotNull("content parent found", contentParent);

				form.Controls.Add(contentParent);
				form.Show();

				AssertContainsExactElementsInAnyOrder("content controls",
					new[]
					{
@"Type: ZCalcEdit
Text: 21
DataBindings: 
prop_Age愛ReadOnly
prop_Age"
					},
					Format(contentParent.Controls));
			}
		}

		#endregion

		#region TestBuild_MultipleEditors

		public void TestBuild_MultipleEditors()
		{
			var dummy = new Dummy(Factory)
			{
				Shipper = new Dummy(Factory)
				{
					Name = "Bob",
					Type = "BBB",
					IsBlue = true
				},
				Manufacturer = new Dummy(Factory)
				{
					Name = "Dr Phil"
				}
			};

			var data = dummy.MakeDynamic();

			var shipperName = data.GetDynamicProperty("Shipper.Name");
			var shipperType = data.GetDynamicProperty("Shipper.Type");
			var shipperIsBlue = data.GetDynamicProperty("Shipper.IsBlue");

			var manufacturerName = data.GetDynamicProperty("Manufacturer.Name");

			AssertEquals("DynamicData created object graph", "Bob", shipperName.Value);
			AssertEquals("DynamicData created object graph", "BBB", shipperType.Value);
			AssertEquals("DynamicData created object graph", true, shipperIsBlue.Value);

			AssertEquals("DynamicData created object graph", "Dr Phil", manufacturerName.Value);

			var properties = new[]
			{
				new MacroBusinessObjectProperty("ShipperName", shipperName),
				new MacroBusinessObjectProperty("ShipperType", shipperType),
				new MacroBusinessObjectProperty("ShipperIsBlue", shipperIsBlue),
				new MacroBusinessObjectProperty("ManufacturerName", manufacturerName)
			};

			var cell = new DummyCell();
			var element = new DynamicContent(cell, new PointF(0f, 0f), new SizeF(10f, 10f));
			var content = new DynamicContentLayoutElement(element);
			var eventHandler = new DummyEditorPresenter();

			var builder = new DynamicContentEditorBuildService();

			using (var form = new ZForm())
			{
				var editor = builder.Build(content, eventHandler, properties, 1f);
				AssertNotNull("editor created", editor);

				var contentParent = (Control)editor.Control;

				AssertNotNull("content parent found", contentParent);

				form.Controls.Add(contentParent);
				form.Show();

				var contentPanel = contentParent.Controls["ContentPanel"];
				AssertNotNull("content panel found", contentPanel);

				AssertContainsExactElementsInAnyOrder("content controls",
					new[]
					{
@"Type: VisualizerTextBox
Text: Bob
DataBindings: 
prop_ShipperName愛ReadOnly
prop_ShipperName愛MaxLength
prop_ShipperName",

@"Type: VisualizerTextBox
Text: BBB
DataBindings: 
prop_ShipperType愛ReadOnly
prop_ShipperType愛MaxLength
prop_ShipperType",

@"Type: ZCheckBox
Text: ShipperIsBlue
DataBindings: 
prop_ShipperIsBlue愛ReadOnly
prop_ShipperIsBlue",

@"Type: VisualizerTextBox
Text: Dr Phil
DataBindings: 
prop_ManufacturerName愛ReadOnly
prop_ManufacturerName愛MaxLength
prop_ManufacturerName",
					},
					Format(contentPanel.Controls));
			}
		}

		#endregion

		#region TestBuild_CustomFieldEditor

		public void TestBuild_CustomFieldEditor()
		{
			var dummy = new Dummy(Factory)
			{
				Shipper = new Dummy(Factory)
				{
					Name = "Bill"
				}
			};

			var data = dummy.MakeDynamic();

			var property1 = data.Properties.GetOrCreate("Name", () => (ZString)"First Name");
			var property2 = data.Properties.GetOrCreate("Surname", () => (ZString)"Last Name");

			var properties = new[]
			{
				new MacroBusinessObjectProperty("Name", property1),
				new MacroBusinessObjectProperty("Surname", property2)
			};

			var cell = new DummyCell();
			var element = new DynamicContent(cell, new PointF(0f, 0f), new SizeF(10f, 10f));
			var content = new DynamicContentLayoutElement(element);
			var eventHandler = new DummyEditorPresenter();

			var builder = new DynamicContentEditorBuildService();

			using (var form = new ZForm())
			{
				var editor = builder.Build(content, eventHandler, properties, 1f);
				AssertNotNull("editor created", editor);

				var contentParent = (Control)editor.Control;

				AssertNotNull("content parent found", contentParent);

				form.Controls.Add(contentParent);
				form.Show();

				var contentPanel = contentParent.Controls["ContentPanel"];
				AssertNotNull("content panel found", contentPanel);

				AssertContainsExactElementsInAnyOrder("content controls",
					new[]
					{
@"Type: VisualizerTextBox
Text: First Name
DataBindings: 
prop_Name愛ReadOnly
prop_Name愛MaxLength
prop_Name",

@"Type: VisualizerTextBox
Text: Last Name
DataBindings: 
prop_Surname愛ReadOnly
prop_Surname愛MaxLength
prop_Surname",
					},
					Format(contentPanel.Controls));
			}
		}

		#endregion

		#region TestEditorOnNoProperties

		public void TestEditorOnNoProperties()
		{
			var cell = new DummyCell();
			var element = new DynamicContent(cell, new PointF(0f, 0f), new SizeF(10f, 10f));
			var content = new DynamicContentLayoutElement(element);
			var eventHandler = new DummyEditorPresenter();

			var builder = new DynamicContentEditorBuildService();

			var editor = builder.Build(content, eventHandler, System.Array.Empty<IMacroBusinessObjectProperty>(), 1f);

			AssertNull("Expected editor when there's no properties", editor);
		}

		#endregion

		#region TestEditorOnSingleProperty

		public void TestEditorOnSingleProperty()
		{
			var dummy = new Dummy(Factory)
			{
				Shipper = new Dummy(Factory)
				{
					Name = "Bob"
				}
			};

			var data = dummy.MakeDynamic();

			var shipperName = data.GetDynamicProperty("Shipper.Name");

			AssertEquals("DynamicData created object graph", "Bob", shipperName.Value);

			var properties = new[]
			{
				new MacroBusinessObjectProperty("Name", shipperName)
			};

			var cell = new DummyCell();
			var element = new DynamicContent(cell, new PointF(0f, 0f), new SizeF(10f, 10f));
			var content = new DynamicContentLayoutElement(element);
			var eventHandler = new DummyEditorPresenter();

			var builder = new DynamicContentEditorBuildService();

			using (var form = new ZForm())
			{
				var editor = builder.Build(content, eventHandler, properties, 1f);

				form.Controls.Add((Control)editor.Control);
				form.Show();

				Assert("Expected editor when there's a single property", editor is InPlaceDynamicEditorView);
			}
		}

		#endregion

		#region TestEditorOnMultipleProperties

		public void TestEditorOnMultipleProperties()
		{
			var dummy = new Dummy(Factory)
			{
				Shipper = new Dummy(Factory)
				{
					Name = "Bob",
					Type = "BBB",
					IsBlue = true
				},
				Manufacturer = new Dummy(Factory)
				{
					Name = "Dr Phil"
				}
			};

			var data = dummy.MakeDynamic();

			var shipperName = data.GetDynamicProperty("Shipper.Name");
			var shipperType = data.GetDynamicProperty("Shipper.Type");
			var shipperIsBlue = data.GetDynamicProperty("Shipper.IsBlue");

			var manufacturerName = data.GetDynamicProperty("Manufacturer.Name");

			AssertEquals("DynamicData created object graph", "Bob", shipperName.Value);
			AssertEquals("DynamicData created object graph", "BBB", shipperType.Value);
			AssertEquals("DynamicData created object graph", true, shipperIsBlue.Value);

			AssertEquals("DynamicData created object graph", "Dr Phil", manufacturerName.Value);

			var properties = new[]
			{
				new MacroBusinessObjectProperty("ShipperName", shipperType),
				new MacroBusinessObjectProperty("ShipperType", shipperType),
				new MacroBusinessObjectProperty("ShipperType", shipperType),
				new MacroBusinessObjectProperty("ManufacturerName", shipperType)
			};

			var cell = new DummyCell();
			var element = new DynamicContent(cell, new PointF(0f, 0f), new SizeF(10f, 10f));
			var content = new DynamicContentLayoutElement(element);
			var eventHandler = new DummyEditorPresenter();

			var builder = new DynamicContentEditorBuildService();

			using (var form = new ZForm())
			{
				var editor = builder.Build(content, eventHandler, properties, 1f);

				form.Controls.Add((Control)editor.Control);
				form.Show();

				Assert("Expected editor when there's a several properties", editor is PopupDynamicEditorView);
			}
		}

		#endregion

		#region Implementation

		class Dummy : NonPersistentBusinessObject
		{
			public Dummy(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			#region Related

			public Dummy Shipper
			{
				get; set;
			}

			public Dummy Manufacturer
			{
				get; set;
			}

			#endregion

			#region Name

			public ZString Name
			{
				get { return name; }
				set { SetNonPersistentPropertyValue(NameInfo, ref name, value); }
			}

			ZString name;

			public ZPropertyInfo NameInfo
			{
				get { return GetZPropertyInfo(nameof(Name)); }
			}

			#endregion

			#region Type

			[List("TypeList")]
			public ZString Type
			{
				get { return type; }
				set { SetNonPersistentPropertyValue(TypeInfo, ref type, value); }
			}

			ZString type;

			public ZPropertyInfo TypeInfo
			{
				get { return GetZPropertyInfo(nameof(Type)); }
			}

			public CodeDescriptionPairList TypeList
			{
				get
				{
					if (typeList == null)
					{
						typeList = new CodeDescriptionPairList();
						typeList.AddPair("AAA", "Good");
						typeList.AddPair("BBB", "Bad");
					}

					return typeList;
				}
			}

			CodeDescriptionPairList typeList;

			#endregion

			#region Unloco

			[List("UnlocoList")]
			public ZString Unloco
			{
				get { return unloco; }
				set { SetNonPersistentPropertyValue(UnlocoInfo, ref unloco, value); }
			}

			ZString unloco;

			public ZPropertyInfo UnlocoInfo
			{
				get { return GetZPropertyInfo(nameof(Unloco)); }
			}

			public RefUNLOCOCollection UnlocoList
			{
				get { return unlocos ?? (unlocos = new RefUNLOCOCollection(Factory)); }
			}

			RefUNLOCOCollection unlocos;

			#endregion

			#region IsBlue

			public ZBool IsBlue
			{
				get { return isBlue; }
				set { SetNonPersistentPropertyValue(IsBlueInfo, ref isBlue, value); }
			}

			ZBool isBlue;

			public ZPropertyInfo IsBlueInfo
			{
				get { return GetZPropertyInfo(nameof(IsBlue)); }
			}

			#endregion

			#region Salary

			public ZDecimal Salary
			{
				get { return salary; }
				set { SetNonPersistentPropertyValue(SalaryInfo, ref salary, value); }
			}

			ZDecimal salary;

			public ZPropertyInfo SalaryInfo
			{
				get { return GetZPropertyInfo(nameof(Salary)); }
			}

			#endregion

			#region Salary

			public ZInt Age
			{
				get { return age; }
				set { SetNonPersistentPropertyValue(AgeInfo, ref age, value); }
			}

			ZInt age;

			public ZPropertyInfo AgeInfo
			{
				get { return GetZPropertyInfo(nameof(Age)); }
			}

			#endregion
		}

		string[] Format(Control.ControlCollection controls)
		{
			return controls
				.Cast<Control>()
				.Where(ctrl => ctrl.Visible)
				.Select(Format)
				.ToArray();
		}

		string Format(Control control)
		{
			var builder = new StringBuilder();

			builder.AppendLine(string.Concat("Type: ", control.GetType().Name));
			builder.AppendLine(string.Concat("Text: ", control.Text));

			builder.AppendLine("DataBindings: ");

			var bindings = FindBoundFields(control);

			if (bindings.Any())
			{
				foreach (var binding in bindings)
				{
					builder.AppendLine(binding);
				}
			}
			else
			{
				builder.Append("none");
			}

			return builder.ToString().TrimEnd();
		}

		string[] FindBoundFields(Control control)
		{
			string[] result = null;

			if (control is ZDropEdit)
			{
				var dropEdit = (ZDropEdit)control;

				result = FindBoundFieldForControl(dropEdit.CodeBox);
			}
			else if (control is ZCodeFindBox)
			{
				var codeFindBox = (ZCodeFindBox)control;

				result = FindBoundFieldForControl(codeFindBox.CodeBox);
			}
			else
			{
				result = FindBoundFieldForControl(control);
			}

			return result ?? System.Array.Empty<string>();
		}

		string[] FindBoundFieldForControl(Control control)
		{
			return control.DataBindings
				.Cast<CargoWise.Windows.UI.KBinding>()
				.Select(binding => binding.BindingMemberInfo.BindingField)
				.OrderByDescending(bindingField => bindingField)
				.ToArray();
		}

		#endregion
	}
}
