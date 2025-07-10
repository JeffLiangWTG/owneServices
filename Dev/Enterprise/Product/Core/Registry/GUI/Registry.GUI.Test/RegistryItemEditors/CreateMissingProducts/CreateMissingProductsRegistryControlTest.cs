using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CreateMissingProductsRegistryControl))]
	sealed class CreateMissingProductsRegistryControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		public new void TestReadOnly()
		{
			Assert(true);
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new CreateMissingProductsInfo();
		}

		[RequiresSTA]
		public void TestControlState()
		{
			using (var control = new CreateMissingProductsRegistryControlForTest())
			{
				AssertEquals("YesRadioButton.Checked", false, control.YesRadioButton.Checked);
				AssertEquals("NoRadioButton.Checked", false, control.NoRadioButton.Checked);

				var info = new CreateMissingProductsInfo();
				info.IsOverrideToYes = true;
				control.SetDataBinding(info, null);
				Assert("YesRadioButton.Checked", control.YesRadioButton.Checked);
				Assert("NoRadioButton.Checked", !control.NoRadioButton.Checked);
				Assert("DefaultRelationshipDropEdit.Visible", control.DefaultRelationshipDropEdit.Visible);

				info.IsOverrideToYes = false;
				control.SetDataBinding(info, null);
				Assert("YesRadioButton.Checked", !control.YesRadioButton.Checked);
				Assert("NoRadioButton.Checked", control.NoRadioButton.Checked);
				Assert("DefaultRelationshipDropEdit.Visible", !control.DefaultRelationshipDropEdit.Visible);
			}
		}

		internal class CreateMissingProductsRegistryControlForTest : CreateMissingProductsRegistryControl
		{
			public ZRadioButton YesRadioButton
			{
				get { return yesRadioButton; }
			}

			public ZRadioButton NoRadioButton
			{
				get { return noRadioButton; }
			}

			public ZDropEdit DefaultRelationshipDropEdit
			{
				get { return defaultRelationshipDropEdit; }
			}
		}
	}
}
