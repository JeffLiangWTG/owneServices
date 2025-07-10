using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(GlowOpportunityStatusRegistryControl))]
	sealed class GlowOpportunityStatusRegistryControlTestCase : RegistryZUserControlTestCase
	{
		protected override CargoWise.EntityFramework.IBusiness GetNewBusinessEntity()
		{
			return new GlowOpportunityStatusCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, CargoWise.EntityFramework.IBusiness businessEntity)
		{
			return ((GlowOpportunityStatusRegistryControl)control).GlowOpportunityStatusGrid.ReadOnly;
		}

		public void TestHasChanges_UpDown()
		{
			using (var testForm = new RegistryFormForTest())
			using (var control = new GlowOpportunityStatusRegistryControl_ForTest())
			{
				testForm.Controls.Add(control);
				var list = GetNewBusinessEntity() as GlowOpportunityStatusCollection;
				list.Add(new GlowOpportunityStatus());
				list.Add(new GlowOpportunityStatus());
				control.SetDataBinding(list, null);
				testForm.Show();

				Assert(!testForm.IsUpdateHasChangesTriggered);
				control.ClickDown();
				Assert(testForm.IsUpdateHasChangesTriggered);
			}
		}

		public class GlowOpportunityStatusRegistryControl_ForTest : GlowOpportunityStatusRegistryControl
		{
			public void ClickDown()
			{
				DownButton_Click(null, System.EventArgs.Empty);
			}
		}
	}
}
