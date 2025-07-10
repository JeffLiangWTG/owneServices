using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(GlowOpportunityStageRegistryControl))]
	sealed class GlowOpportunityStageRegistryControlTest : RegistryZUserControlTestCase
	{
		protected override CargoWise.EntityFramework.IBusiness GetNewBusinessEntity()
		{
			return new GlowOpportunityStageCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, CargoWise.EntityFramework.IBusiness businessEntity)
		{
			return ((GlowOpportunityStageRegistryControl)control).GlowOpportunityStageRegistryGrid.ReadOnly;
		}

		public void TestHasChanges_UpDown()
		{
			using (var testForm = new RegistryFormForTest())
			using (var control = new GlowOpportunityStageRegistryControl_ForTest())
			{
				testForm.Controls.Add(control);
				var list = GetNewBusinessEntity() as GlowOpportunityStageCollection;
				list.Add(new GlowOpportunityStage());
				list.Add(new GlowOpportunityStage());
				control.SetDataBinding(list, null);
				testForm.Show();

				Assert(!testForm.IsUpdateHasChangesTriggered);
				control.ClickDown();
				Assert(testForm.IsUpdateHasChangesTriggered);
			}
		}

		public class GlowOpportunityStageRegistryControl_ForTest : GlowOpportunityStageRegistryControl
		{
			public void ClickDown()
			{
				DownButton_Click(null, System.EventArgs.Empty);
			}
		}
	}
}
