using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(CodeDescriptionIncidentEmailTemplatePairCollectionRegistryControl))]
	sealed class CodeDescriptionIncidentEmailTemplatePairCollectionRegistryControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new CodeDescriptionIncidentEmailTemplatePairCollection(typeof(DocSupportIncident));
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control1, IBusiness businessEntity1)
		{
			var control = (CodeDescriptionIncidentEmailTemplatePairCollectionRegistryControl)control1;
			var businessEntity = (CodeDescriptionIncidentEmailTemplatePairCollection)businessEntity1;
			return (control.CategoryGrid.ReadOnly && control.IncidentEmailTemplatePairRegistryControl.ReadOnly) || businessEntity.ReadOnly;
		}

		public void TestNeedUpgradeColumnVisible()
		{
			using (var control = new CodeDescriptionIncidentEmailTemplatePairCollectionRegistryControl())
			{
				AssertEquals(false, control.CategoryGrid.GetColumnStyle(nameof(CodeDescriptionIncidentEmailTemplatePair.NeedUpgrade)).IsUnavailable);
			}

			using (var control = new CodeDescriptionIncidentEmailTemplatePairCollectionRegistryControl(true))
			{
				AssertEquals(false, control.CategoryGrid.GetColumnStyle(nameof(CodeDescriptionIncidentEmailTemplatePair.NeedUpgrade)).IsUnavailable);
			}

			using (var control = new CodeDescriptionIncidentEmailTemplatePairCollectionRegistryControl(false))
			{
				AssertEquals(true, control.CategoryGrid.GetColumnStyle(nameof(CodeDescriptionIncidentEmailTemplatePair.NeedUpgrade)).IsUnavailable);
			}
		}
	}
}
