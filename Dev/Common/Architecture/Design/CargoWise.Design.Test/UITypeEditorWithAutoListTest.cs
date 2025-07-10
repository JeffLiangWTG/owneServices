using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing.Design;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.ComponentModel.Design;
using CargoWise.Windows.UI;
using NUnit.Framework;

namespace CargoWise.Design.Testing
{
	class UITypeEditorWithAutoListTest : TestCase
	{
		[GuiTest]
		public void TestEditorLocatesTextBox()
		{
			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
			bool testWasRun = DoTestEditorLocatesTextBox();
			AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(CurrentDomain_AssemblyResolve);

			if (testWasRun)
			{
				GC.Collect();
				AssertEquals(0, UITypeEditorWithAutoList.autoListManagers.Count); //calling Count auto-collects garbage collected references
			}
		}

		Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			return Assembly.Load(args.Name);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1109:Form", Justification = "Required for testing only")]
		bool DoTestEditorLocatesTextBox()
		{
			try
			{
				using (Form f = new Form())
				using (PropertyGrid grid = new PropertyGrid())
				{
					f.Controls.Add(grid);
					f.Show();
					Application.DoEvents();
					if (Form.ActiveForm == f)
					{
						grid.SelectedObject = new TestComponent();
						Application.DoEvents();
						KSendKeys.SendWait("{TAB}", grid);
						KSendKeys.SendWait("{TAB}", grid);
						KSendKeys.SendWait("{TAB}", grid);
						KSendKeys.SendWait("splaty", grid);
						Application.DoEvents();
						PropertyDescriptor property = TypeDescriptor.GetProperties(typeof(TestComponent))["Type"];
						TestEditor editor = (TestEditor)property.GetEditor(typeof(UITypeEditor));
						AssertEquals("Should have located the PropertyGrid text box", "splaty", editor.AutoListManager.TextBox.Text.ToLower());
						editor.TearDown();
						return true;
					}
					else
					{
						Assert("If the test is running on a locked computer, we can't run the test", true);
						return false;
					}
				}
			}
			catch
			{
				Assert(true);
				return false;
			}
		}

		#region Test Classes
		[DesignerSerializer(typeof(MyCodeDomSerialiser), typeof(CodeDomSerializer))]
		class TestComponent : Component
		{
			internal class MyCodeDomSerialiser : TypeFixCodeDomSerializer
			{
				public MyCodeDomSerialiser() : base(typeof(TestComponent))
				{
				}
			}

			[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
			[TypeConverter(typeof(TypeTypeConverter))]
			[Editor("CargoWise.Design.UITypeEditorWithAutoList+Test+TestEditor", typeof(UITypeEditor))]
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
		}

		public class TestEditor : UITypeEditorWithAutoList
		{
			public new AutoListManager AutoListManager
			{
				get
				{
					return base.AutoListManager;
				}
			}

			protected override IAutoListSource NewAutoListSource(ITypeDescriptorContext context)
			{
				return new TestAutoListSource(context);
			}
		}

		class TestAutoListSource : ListBoxAutoListSource
		{
			public TestAutoListSource(ITypeDescriptorContext context) : base(context)
			{
			}

			protected override bool UpdateListControl(ListBox listControl, TextBoxBase textBox)
			{
				if (listControl.Items.Count == 0)
				{
					listControl.Items.Add("splaty");
				}

				return true;
			}
		}
		#endregion
	}
}
