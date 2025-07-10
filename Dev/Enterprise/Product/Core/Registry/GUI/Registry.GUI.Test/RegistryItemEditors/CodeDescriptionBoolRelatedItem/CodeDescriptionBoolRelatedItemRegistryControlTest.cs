using System.Windows.Forms;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CodeDescriptionBoolRelatedItemRegistryControl))]
	sealed class CodeDescriptionBoolRelatedItemRegistryControlTest : RegistryZUserControlTestCase
	{
		public void TestData()
		{
			var collection = new CodeDescriptionBoolRelatedItemCollection();

			using (var control = new CodeDescriptionBoolRelatedItemRegistryControl())
			{
				control.Data = collection;
				AssertEquals(collection, control.Data);
			}
		}

		public void TestReadOnlySetsReadOnlyOnGrid()
		{
			using (var form = new TestForm())
			using (var control = new CodeDescriptionBoolRelatedItemRegistryControlForTest())
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				control.ReadOnly = false;
				AssertEquals(false, control.ReadOnly);
				AssertEquals(false, control.CodeDescriptionBoolRelatedItemGridExposed.ReadOnly);
				control.ReadOnly = true;
				AssertEquals(true, control.ReadOnly);
				AssertEquals(true, control.CodeDescriptionBoolRelatedItemGridExposed.ReadOnly);
				control.ReadOnly = false;
				AssertEquals(false, control.ReadOnly);
				AssertEquals(false, control.CodeDescriptionBoolRelatedItemGridExposed.ReadOnly);
			}
		}

		protected override CargoWise.EntityFramework.IBusiness GetNewBusinessEntity()
		{
			return null;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, CargoWise.EntityFramework.IBusiness businessEntity)
		{
			return ((CodeDescriptionBoolRelatedItemRegistryControl)control).ReadOnly;
		}

		protected override RegistryZUserControl GetNewControl()
		{
			return new CodeDescriptionBoolRelatedItemRegistryControl();
		}
	}
}
