using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(SalesRelationDirectionRulesRegistryControl))]
	sealed class SalesRelationRulesRegistryControlTest : RegistryZUserControlTestCase
	{
		public void TestDeleteMenuItem()
		{
			using (var control = new SalesRelationDirectionRulesRegistryControlForTest())
			{
				control.ReadOnly = false;
				AssertEquals(true, control.DirectionRuleGrid_Exposed.AllowReadOnlyRowsToBeDeleted);
				AssertEquals(RemoveAction.RemoveAndDelete, control.DirectionRuleGrid_Exposed.RemoveAction);
				AssertEquals("&Delete", control.DirectionRuleGrid_Exposed.DeleteMenuItem.Text);

				control.ReadOnly = true;
				AssertEquals(false, control.DirectionRuleGrid_Exposed.AllowReadOnlyRowsToBeDeleted);
			}
		}

		#region Implementation

		protected override IBusiness GetNewBusinessEntity()
		{
			return new SalesRelationDirectionRuleCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return control.ReadOnly;
		}

		protected override RegistryZUserControl GetNewControl()
		{
			return new SalesRelationDirectionRulesRegistryControl();
		}

		class SalesRelationDirectionRulesRegistryControlForTest : SalesRelationDirectionRulesRegistryControl
		{
			public ZGrid DirectionRuleGrid_Exposed
			{
				get { return base.directionRuleGrid; }
			}
		}

		#endregion
	}
}
