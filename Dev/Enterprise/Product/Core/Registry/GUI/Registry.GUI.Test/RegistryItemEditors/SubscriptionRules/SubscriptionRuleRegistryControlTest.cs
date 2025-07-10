using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(SubscriptionRulesRegistryControl))]
	sealed class SubscriptionRuleRegistryControlTest : RegistryZUserControlTestCase
	{
		public void TestDeleteMenuItem()
		{
			using (var control = new SubscriptionRulesRegistryControlForTest())
			{
				control.ReadOnly = false;
				AssertEquals(true, control.SubscriptionRuleGrid_Exposed.AllowReadOnlyRowsToBeDeleted);
				AssertEquals(RemoveAction.RemoveAndDelete, control.SubscriptionRuleGrid_Exposed.RemoveAction);
				AssertEquals("&Delete", control.SubscriptionRuleGrid_Exposed.DeleteMenuItem.Text);

				control.ReadOnly = true;
				AssertEquals(false, control.SubscriptionRuleGrid_Exposed.AllowReadOnlyRowsToBeDeleted);
			}
		}

		#region Implementation

		protected override IBusiness GetNewBusinessEntity()
		{
			return new SubscriptionRuleCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return control.ReadOnly;
		}

		protected override RegistryZUserControl GetNewControl()
		{
			return new SubscriptionRulesRegistryControl();
		}

		class SubscriptionRulesRegistryControlForTest : SubscriptionRulesRegistryControl
		{
			public ZGrid SubscriptionRuleGrid_Exposed
			{
				get { return base.subscriptionRuleGrid; }
			}
		}

		#endregion
	}
}
