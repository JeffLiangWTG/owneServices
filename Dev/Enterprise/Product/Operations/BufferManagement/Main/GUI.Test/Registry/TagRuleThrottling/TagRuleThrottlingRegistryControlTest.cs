using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(TagRuleThrottlingRegistryControl))]
	public class TagRuleThrottlingRegistryControlTest : Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		public void TestGridReadOnly_ShouldBeBasedOnCheckBox()
		{
			using (var form = new Form())
			{
				using (var control = new TagRuleThrottlingRegistryControlForTest())
				{
					form.Controls.Add(control);

					var checkbox = (ZCheckBox)control.Controls.Find("IsThrottlingEnabledCheckBox", true).Single();
					var grid = (ZGrid)control.Controls.Find("ThresholdGrid", true).Single();

					control.SetReadOnly(true);

					AssertEquals(true, checkbox.ReadOnly);
					AssertEquals(true, grid.ReadOnly);

					control.SetReadOnly(false);

					AssertEquals(false, checkbox.ReadOnly);
					AssertEquals(false, checkbox.Checked);
					AssertEquals(true, grid.ReadOnly);

					checkbox.CheckState = CheckState.Checked;

					AssertEquals(false, checkbox.ReadOnly);
					AssertEquals(false, grid.ReadOnly);

					control.SetReadOnly(true);

					AssertEquals(true, checkbox.ReadOnly);
					AssertEquals(true, checkbox.Checked);
					AssertEquals(true, grid.ReadOnly);
				}
			}
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new TagRuleThrottlingHeader();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			var checkbox = (ZCheckBox)control.Controls.Find("IsThrottlingEnabledCheckBox", true).Single();

			return checkbox.ReadOnly;
		}

		class TagRuleThrottlingRegistryControlForTest : TagRuleThrottlingRegistryControl
		{
			public void SetReadOnly(bool readOnly)
			{
				SetControlOrBusinessEntityReadOnly(readOnly);
			}
		}
	}
}
