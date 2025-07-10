using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI
{
	[TestedType(typeof(HVLVPreScreeningRuleControl))]
	sealed class HVLVPreScreeningControlTest : Testing.RegistryZUserControlTestCase
	{
		[RequiresSTA]
		public void TestDeminimusPanelControlBindings()
		{
			using (ZForm form = new ZForm())
			using (RegistryZUserControl control = GetNewControl())
			{
				form.Controls.Add(control);
				form.Show();

				var deminimusPanel = form.Controls.Find("DeminimusPanel", true).First();
				var deminimusCheckedTextBox = deminimusPanel.Controls.Find("DeminimusCheckedTextBox", true).First() as ZCheckedTextBox;
				var deminimusCurrencyCodeFindBox = deminimusPanel.Controls.Find("DeminimusCurrencyCodeFindBox", true).First() as ZCodeFindBox;

				CombineAssertions(() =>
				{
					AssertEquals("IsDeminimusValueOverride", "Rules.Fields.IsDeminimusValueOverride", control.BindingSource.GetBindingMember(deminimusCheckedTextBox.CheckBox));
					AssertEquals("DeminimusValue", "Rules.Fields.DeminimusValue", control.BindingSource.GetBindingMember(deminimusCheckedTextBox));
					AssertEquals("DeminimusCurrency", "Rules.Fields.DeminimusCurrency", control.BindingSource.GetBindingMember(deminimusCurrencyCodeFindBox));
				});
			}
		}

		#region Implementations
		protected override IBusiness GetNewBusinessEntity()
		{
			return new HVLVDetailsPreScreeningConfiguration();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return (((HVLVPreScreeningRuleControl)control).ValidationRulesGrid.ReadOnly && ((HVLVPreScreeningRuleControl)control).IsEnabledCheckBox.ReadOnly);
		}
		#endregion
	}
}
