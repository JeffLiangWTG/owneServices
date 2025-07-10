using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Design;
using CargoWise.Common.Design.Testing;
using CargoWise.ComponentModel.Design;
using CargoWise.Windows.UI;
using NUnit.Framework;

namespace CargoWise.Design.TypeIntellisense.Testing
{
	class TypeIntellisenseAutoListSourceTest : TestCase
	{
		[GuiTest]
		public void TestListBoxSelectsItemsAppropriately()
		{
			try
			{
				DoTestListBoxSelectsItemsAppropriately();
			}
			catch
			{
				try
				{
					TearDown();
					SetUp();
					DoTestListBoxSelectsItemsAppropriately();
				}
				catch
				{
					KSendKeys.SendWait("{CAPSLOCK}");
					TearDown();
					SetUp();
					DoTestListBoxSelectsItemsAppropriately();
				}
			}
		}

		void DoTestListBoxSelectsItemsAppropriately()
		{
			bool canActivateForm = FullyPopulatedAutoListManager != null;
			if (canActivateForm)
			{
				FullyPopulatedAutoListManager.TextBox.Text = "";
				KSendKeys.SendWait("Sy", TextBox);
				System.Threading.Thread.Sleep(1000);
				Application.DoEvents();
				AssertEquals("Should select the 'System' namespace", "System", ((IntellisenseDataSourceItem)AutoListSource.LastCreatedListBox.SelectedItem).DisplayName);
				AssertEquals("Should select the 'System' namespace", false, AutoListSource.LastCreatedListBox.OutlineSelectedItem);
				TextBox.Text = "";
				KSendKeys.SendWait("System.", TextBox);
				DoEvents();
				Application.DoEvents();
				AssertEquals("Should not show highlist selected item because the dot has just been pressed", true, AutoListSource.LastCreatedListBox.OutlineSelectedItem);
				TextBox.Text = "";
				KSendKeys.SendWait("System.I", TextBox);
				DoEvents();
				AssertEquals("Should select the 'Int32' type", "Int32", ((IntellisenseDataSourceItem)AutoListSource.LastCreatedListBox.SelectedItem).DisplayName);
				TextBox.Text = "";
				KSendKeys.SendWait("System.Iz", TextBox);
				DoEvents();
				AssertEquals("Should select the 'Int32' type, but should not highlight it", "Int32", ((IntellisenseDataSourceItem)AutoListSource.LastCreatedListBox.SelectedItem).DisplayName);
				AssertEquals("Should select the 'Int32' type, but should not highlight it", true, AutoListSource.LastCreatedListBox.OutlineSelectedItem);
			}
		}

		[GuiTest]
		public void TestListBoxUpDownWhenNotHighlighting_HighlightsButDoesntMove()
		{
			try
			{
				DoTestListBoxUpDownWhenNotHighlighting_HighlightsButDoesntMove();
			}
			catch
			{
				try
				{
					TearDown();
					SetUp();
					DoTestListBoxUpDownWhenNotHighlighting_HighlightsButDoesntMove();
				}
				catch
				{
					TearDown();
					SetUp();
					KSendKeys.SendWait("{CAPSLOCK}");
					DoTestListBoxUpDownWhenNotHighlighting_HighlightsButDoesntMove();
				}
			}
		}

		void DoTestListBoxUpDownWhenNotHighlighting_HighlightsButDoesntMove()
		{
			DoListBoxUpDownWhenNotHighlighting_HighlightsButDoesntMove("{UP}");
			DoListBoxUpDownWhenNotHighlighting_HighlightsButDoesntMove("{DOWN}");
		}

		void DoListBoxUpDownWhenNotHighlighting_HighlightsButDoesntMove(string upDownKeys)
		{
			bool canActivateForm = FullyPopulatedAutoListManager != null;
			if (canActivateForm)
			{
				FullyPopulatedAutoListManager.TextBox.Text = "";
				KSendKeys.SendWait("System.Iw", TextBox);
				DoEvents();
				AssertEquals("Should select the 'Int32' type, but should not highlight it", "Int32", ((IntellisenseDataSourceItem)AutoListSource.LastCreatedListBox.SelectedItem).DisplayName);
				AssertEquals("Should select the 'Int32' type, but should not highlight it", true, AutoListSource.LastCreatedListBox.OutlineSelectedItem);
				KSendKeys.SendWait(upDownKeys, TextBox);
				DoEvents();
				AssertEquals("Should select the 'Int32' type, but should not highlight it", "Int32", ((IntellisenseDataSourceItem)AutoListSource.LastCreatedListBox.SelectedItem).DisplayName);
				AssertEquals("Should select the 'Int32' type, but should not highlight it", false, AutoListSource.LastCreatedListBox.OutlineSelectedItem);
			}
		}

		#region GetTypesToAutoList
		[RequiresSoftware(RequiredSoftware.VisualStudio)]
		public void TestGetTypesToAutoList_FilteredByBaseTypes()
		{
			Instance = this;
			PropertyDescriptor = TypeDescriptor.GetProperties(this)["TypePropertyFilteredByBaseType"];
			List<Type> all_types = new List<Type>();
			foreach (Type type in AutoListSource.GetTypesToAutoList())
			{
				all_types.Add(type);
			}

			AssertEquals("Should find 2 types only", 2, all_types.Count);
			AssertEquals(true, all_types.Contains(typeof(TestImplementedIntf1)));
			AssertEquals(true, all_types.Contains(typeof(TestImplementedIntf2and3)));
		}

		// TestInterface1 or (TestInterface2 and TestInterface3)
		[TypeValueIntellisenseEditorSubtypeFilter(typeof(TestInterface1))]
		[TypeValueIntellisenseEditorSubtypeFilter(typeof(TestInterface2), typeof(TestInterface3))]
		public Type TypePropertyFilteredByBaseType
		{
			get
			{
				return null;
			}
		}

		[RequiresSoftware(RequiredSoftware.VisualStudio)]
		public void TestGetTypesToAutoList_FilteredByMethod()
		{
			Instance = this;
			PropertyDescriptor = TypeDescriptor.GetProperties(this)["TypePropertyFilteredByMethod"];
			List<Type> all_types = new List<Type>();
			foreach (Type type in AutoListSource.GetTypesToAutoList())
			{
				all_types.Add(type);
			}

			AssertEquals("Should find 3 types", 3, all_types.Count);
			AssertEquals("AdditionalType1", all_types[0].Name);
			AssertEquals("AdditionalType2", all_types[1].Name);
			AssertEquals(typeof(TestImplementedIntf1), all_types[2]);
		}

		[TypeValueIntellisenseEditorSubtypeFilter("GetTypePropertyFilteredByMethodTypes", typeof(TestInterface1))]
		public Type TypePropertyFilteredByMethod
		{
			get
			{
				return null;
			}
		}

		protected IEnumerable<Type> GetTypePropertyFilteredByMethodTypes(ITypeResolutionService typeResolutionService, IEnumerable<Type> types)
		{
			yield return new TypeNameHolder("AdditionalType1");
			yield return new TypeNameHolder("AdditionalType2");
			foreach (Type type in types)
			{
				yield return type;
			}
		}

		#endregion
		#region GetUsingNamespaces
		[RequiresSoftware(RequiredSoftware.VisualStudio)]
		public void TestGetUsingNamespaces_WithMemberOnDeclaringClass()
		{
			Instance = new TestClassWithTypePropertyWithUsingNamespaces();
			PropertyDescriptor = TypeDescriptor.GetProperties(Instance)["Type"];
			AssertEquals(2, AutoListSource.GetUsingNamespaces().Length);
			AssertEquals("System", AutoListSource.GetUsingNamespaces()[0]);
			AssertEquals("CargoWise.Business", AutoListSource.GetUsingNamespaces()[1]);
		}

		#endregion
		#region Test Classes
		public class TestAutoListManager : AutoListManager
		{
			public new Form AutoListForm
			{
				get
				{
					return base.AutoListForm;
				}
			}
		}

		public class TestTypeIntellisenseAutoListSource : TypeIntellisenseAutoListSource
		{
			public TestTypeIntellisenseAutoListSource(ITypeDescriptorContext context) : base(context)
			{
			}

			public IntellisenseListBox LastCreatedListBox;
			protected override ListBox NewListControl()
			{
				IntellisenseListBox result = (IntellisenseListBox)base.NewListControl();
				LastCreatedListBox = result;
				return result;
			}

			public new string[] GetUsingNamespaces()
			{
				return base.GetUsingNamespaces();
			}

			protected override IEnumerable<Type> GetUnfilteredTypesToAutoList()
			{
				List<Type> result = new List<Type>();
				result.Add(typeof(int));
				result.Add(typeof(double));
				result.Add(typeof(bool));
				result.Add(typeof(EnvDTE.ProjectItem));
				result.Add(typeof(TestImplementedIntf1));
				result.Add(typeof(TestImplementedIntf2and3));
				return result;
			}
		}

		interface TestInterface1
		{
		}

		interface TestInterface2
		{
		}

		interface TestInterface3
		{
		}

		class TestImplementedIntf1 : TestInterface1
		{
		}

		class TestImplementedIntf2and3 : TestInterface2, TestInterface3
		{
		}

		public class TestTypeDescriptorContext : ITypeDescriptorContext
		{
			readonly TypeIntellisenseAutoListSourceTest owner;
			readonly object instance;
			readonly PropertyDescriptor propertyDescriptor;
			public TestTypeDescriptorContext(TypeIntellisenseAutoListSourceTest owner, object instance, PropertyDescriptor propertyDescriptor)
			{
				this.owner = owner;
				this.instance = instance;
				this.propertyDescriptor = propertyDescriptor;
			}

			public PropertyDescriptor PropertyDescriptor
			{
				get
				{
					return propertyDescriptor;
				}
			}

			public object Instance
			{
				get
				{
					return instance;
				}
			}

			#region ITypeDescriptorContext Members
			public void OnComponentChanged()
			{
				throw new NotSupportedException();
			}

			public IContainer Container
			{
				get
				{
					throw new NotSupportedException();
				}
			}

			public bool OnComponentChanging()
			{
				throw new NotSupportedException();
			}

			#endregion
			#region IServiceProvider Members
			public object GetService(Type serviceType)
			{
				object result = null;
				if (serviceType == TypeResolutionServiceLocator.DynamicTypeServiceType)
				{
					result = new MockDynamicTypeService(owner.TypeResolutionService);
				}

				return result;
			}
			#endregion
		}

		class TestClassWithTypePropertyWithUsingNamespaces
		{
			[TypeValueIntellisenseEditorSubtypeFilter(typeof(object), ImportNamespacesMember = "GetUsingNamespaces")]
			public Type Type
			{
				get
				{
					return type;
				}

				set
				{
					type = value;
				}
			}

			Type type;

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Test")]
			string[] GetUsingNamespaces()
			{
				return new string[] { "System", "CargoWise.Business" };
			}
		}

		#endregion
		#region Implementation
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1109:Form", Justification = "Required for testing only")]
		Form Form
		{
			get
			{
				if (form == null)
				{
					form = new Form();
				}

				return form;
			}
		}

		Form form;
		TextBox TextBox
		{
			get
			{
				if (textBox == null)
				{
					textBox = new TextBox();
				}

				return textBox;
			}
		}

		TextBox textBox;
		TestTypeDescriptorContext TypeDescriptorContext
		{
			get
			{
				if (typeDescriptorContext == null)
				{
					typeDescriptorContext = new TestTypeDescriptorContext(this, Instance, PropertyDescriptor);
				}

				return typeDescriptorContext;
			}
		}

		TestTypeDescriptorContext typeDescriptorContext;
		object Instance;
		PropertyDescriptor PropertyDescriptor;
		TestAutoListManager FullyPopulatedAutoListManager
		{
			get
			{
				if (fullyPopulatedAutoListManager == null)
				{
					fullyPopulatedAutoListManager = new TestAutoListManager();
					fullyPopulatedAutoListManager.TextBox = TextBox;
					fullyPopulatedAutoListManager.AutoListSource = AutoListSource;
					Form.Controls.Add(TextBox);
					Form.Show();
					Application.DoEvents();
					if (Form.ActiveForm == Form)
					{
						FullyPopulatedAutoListManager.TextBox.Focus();
						TextBox.Text = "x";
						TextBox.Text = "";
						// ensure populatation is finished
						((IntellisenseListBox)FullyPopulatedAutoListManager.AutoListForm.Controls[0]).DataSource.PartialName = "";
					}
					else
					{
						fullyPopulatedAutoListManager.Dispose();
						fullyPopulatedAutoListManager = null;
						Assert("If the test is running on a locked computer, we can't run the test", true);
					}
				}

				return fullyPopulatedAutoListManager;
			}
		}

		TestAutoListManager fullyPopulatedAutoListManager;
		ITypeResolutionService TypeResolutionService
		{
			get
			{
				if (typeResolutionService == null)
				{
					typeResolutionService = new MockTypeResolutionService(GetType().Assembly);
				}

				return typeResolutionService;
			}
		}

		ITypeResolutionService typeResolutionService;
		TestTypeIntellisenseAutoListSource AutoListSource
		{
			get
			{
				if (autoListSource == null)
				{
					autoListSource = new TestTypeIntellisenseAutoListSource(TypeDescriptorContext);
				}

				return autoListSource;
			}
		}

		TestTypeIntellisenseAutoListSource autoListSource;
		void DoEvents()
		{
			DateTime before = DateTime.Now;
			while (DateTime.Now.Subtract(before).TotalMilliseconds < 500)
			{
				Application.DoEvents();
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (textBox != null)
			{
				textBox.Dispose();
				textBox = null;
			}

			if (form != null)
			{
				form.Dispose();
				form = null;
			}

			if (fullyPopulatedAutoListManager != null)
			{
				fullyPopulatedAutoListManager.Dispose();
				fullyPopulatedAutoListManager = null;
			}
		}
		#endregion
	}
}
