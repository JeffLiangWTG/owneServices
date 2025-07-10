using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using Microsoft.CSharp;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Design.Testing
{
	sealed class ControlCodeDomSerializerTestCase : TestCase
	{
		[RequiresSTA]
		public void TestSerializeCompareToWinFormsSerializer()
		{
			using (var designSurface = PrepareDesigner())
			{
				var codeWinForms = GenerateCS(SerializeControls(designSurface, false));
				var codeOverridden = GenerateCS(SerializeControls(designSurface, true));

				AssertMultilineASCIIEquals(codeWinForms, codeOverridden);
			}
		}

		#region Implementation

		DesignSurface PrepareDesigner()
		{
			var designSurface = new DesignSurface(typeof(Form));
			var host = (IDesignerHost)designSurface.GetService(typeof(IDesignerHost));
			DesignForm(host);
			return designSurface;
		}

		void DesignForm(IDesignerHost host)
		{
			var form = (Form)host.RootComponent;
			TypeDescriptor.GetProperties(form)["Name"].SetValue(form, "Form1");
			form.Text = "Form1";
			form.Size = new Size(400, 300);

			var button1 = (Button)host.CreateComponent(typeof(Button), "button1");
			button1.Text = "Click me";
			button1.Location = new Point(8, 8);
			form.Controls.Add(button1);

			var panel1 = (Panel)host.CreateComponent(typeof(Panel), "panel1");
			panel1.Text = "";
			panel1.Location = new Point(8, 40);
			panel1.Size = new Size(300, 200);
			form.Controls.Add(panel1);

			var groupBox1 = (GroupBox)host.CreateComponent(typeof(GroupBox), "groupBox1");
			groupBox1.Text = "Group 1";
			groupBox1.Dock = DockStyle.Fill;
			panel1.Controls.Add(groupBox1);

			var textBox1 = (TextBox)host.CreateComponent(typeof(TextBox), "textBox1");
			textBox1.Text = "ABC";
			textBox1.Location = new Point(8, 8);
			groupBox1.Controls.Add(textBox1);

			var grid1 = (DataGrid)host.CreateComponent(typeof(DataGrid), "dataGrid1");
			grid1.Location = new Point(40, 8);
			grid1.Size = new Size(200, 160);
			groupBox1.Controls.Add(grid1);

			// Some non-ui component
			var timer1 = (Timer)host.CreateComponent(typeof(Timer), "timer1");
			timer1.Interval = 2000;
		}

		CodeTypeDeclaration SerializeControls(DesignSurface designSurface, bool overrideControlCodeDomSerializer)
		{
			var host = (IDesignerHost)designSurface.GetService(typeof(IDesignerHost));
			var root = host.RootComponent;

			CodeTypeDeclaration codeTypeDeclaration;
			var internalManager = new DesignerSerializationManager(host);
			var manager = new DesignerSerializationManagerForTest(internalManager) { OverrideControlCodeDomSerializer = overrideControlCodeDomSerializer };
			using (internalManager.CreateSession())
			{
				var serializer = (TypeCodeDomSerializer)manager.GetSerializer(root.GetType(), typeof(TypeCodeDomSerializer));
				codeTypeDeclaration = serializer.Serialize(manager, root, host.Container.Components);
				codeTypeDeclaration.IsPartial = true;
				codeTypeDeclaration.Members.OfType<CodeConstructor>().First().Attributes = MemberAttributes.Public;
			}

			return codeTypeDeclaration;
		}

		string GenerateCS(CodeTypeDeclaration codeTypeDeclaration)
		{
			var codeBuilder = new StringBuilder();
			CodeGeneratorOptions option = new CodeGeneratorOptions
			{
				BracingStyle = "C",
				BlankLinesBetweenMembers = false,
			};
			using (var writer = new StringWriter(codeBuilder, CultureInfo.InvariantCulture))
			using (var codeDomProvider = new CSharpCodeProvider())
			{
				codeDomProvider.GenerateCodeFromType(codeTypeDeclaration, writer, option);
			}

			return codeBuilder.ToString();
		}

		#endregion

		#region Test classes

		class DesignerSerializationManagerForTest : IDesignerSerializationManager
		{
			public DesignerSerializationManagerForTest(IDesignerSerializationManager internalManager)
			{
				Argument.NotNull(internalManager, nameof(internalManager));

				this.internalManager = internalManager;
			}

			readonly IDesignerSerializationManager internalManager;

			public bool OverrideControlCodeDomSerializer { get; set; }

			public object GetService(Type serviceType) => internalManager.GetService(serviceType);

			public void AddSerializationProvider(IDesignerSerializationProvider provider) => internalManager.AddSerializationProvider(provider);

			public object CreateInstance(Type type, ICollection arguments, string name, bool addToContainer) => internalManager.CreateInstance(type, arguments, name, addToContainer);

			public object GetInstance(string name) => internalManager.GetInstance(name);

			public string GetName(object value) => internalManager.GetName(value);

			public object GetSerializer(Type objectType, Type serializerType)
			{
				var serializer = internalManager.GetSerializer(objectType, serializerType);
				if (OverrideControlCodeDomSerializer && serializer != null)
				{
					if (serializer.GetType().FullName == ControlCodeDomSerializer.WinFormsControlCodeDomSerializerTypeFullName)
					{
						serializer = new ControlCodeDomSerializer();
					}
					else if (serializer.GetType().FullName == ComponentCodeDomSerializer.WinFormsComponentCodeDomSerializerTypeFullName)
					{
						serializer = new ComponentCodeDomSerializer();
					}
				}
				return serializer;
			}

			public Type GetType(string typeName) => internalManager.GetType(typeName);

			public void RemoveSerializationProvider(IDesignerSerializationProvider provider) => internalManager.RemoveSerializationProvider(provider);

			public void ReportError(object errorInformation) => internalManager.ReportError(errorInformation);

			public void SetName(object instance, string name) => internalManager.SetName(instance, name);

			public ContextStack Context => internalManager.Context;

			public PropertyDescriptorCollection Properties => internalManager.Properties;

			event ResolveNameEventHandler IDesignerSerializationManager.ResolveName
			{
				add => internalManager.ResolveName += value;
				remove => internalManager.ResolveName -= value;
			}

			event EventHandler IDesignerSerializationManager.SerializationComplete
			{
				add => internalManager.SerializationComplete += value;
				remove => internalManager.SerializationComplete -= value;
			}
		}

		#endregion
	}
}
