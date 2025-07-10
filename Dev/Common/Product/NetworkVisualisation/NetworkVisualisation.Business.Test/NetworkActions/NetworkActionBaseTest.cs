using System.Collections.Generic;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	public class NetworkActionBaseTest : TestCase
	{
		public void TestEnabledness_ShouldDependOnApplicability()
		{
			var action = new TestAction(isApplicable: false, isEnabled: true);
			Assert("Precondition", !action.IsApplicable().IsAllowed);

			Assert("Should not be enabled as not applicable", !action.IsEnabled().IsAllowed);
			Assert("Should not perform enabledness checks as applicability checks failed", !action.IsEnabledInvoked);
		}

		public void TestCanStartExecutionExecutionChecks_ShouldDependOnEnabledness()
		{
			var action = new TestAction(isApplicable: true, isEnabled: false, canExecute: true);
			Assert("Precondition", !action.IsEnabled().IsAllowed);

			Assert("Should not be able to execute as not enabled", !action.CheckCanStartExecution().IsAllowed);
			Assert("Should not perform can execute checks as enabledness checks failed", !action.CheckCanStartExecutionInvoked);
		}

		protected class TestAction : NetworkActionBase
		{
			public TestAction(bool isApplicable = true, bool isEnabled = true, bool canExecute = true)
				: base()
			{
				this.isApplicable = isApplicable;
				this.isEnabled = isEnabled;
				this.canExecute = canExecute;
			}

			readonly bool isApplicable;
			readonly bool isEnabled;
			readonly bool canExecute;

			public bool IsEnabledInvoked;
			public bool CheckCanStartExecutionInvoked;

			protected override ResourceString GetNameCore()
			{
				throw new System.NotImplementedException();
			}

			protected override ResourceString GetDescriptionCore()
			{
				throw new System.NotImplementedException();
			}

			protected override string GetIconNameCore() => string.Empty;

			protected override bool IsActivatedCore()
			{
				throw new System.NotImplementedException();
			}

			protected override IEnumerable<INetworkAction> GetChildActionsCore()
			{
				throw new System.NotImplementedException();
			}

			protected override INetworkActionAccessibility IsApplicableCore()
			{
				return new NetworkActionAccessibility(isApplicable, null, () => "Action is not applicable.");
			}

			protected override INetworkActionAccessibility IsEnabledCore()
			{
				IsEnabledInvoked = true;
				return new NetworkActionAccessibility(isEnabled, null, () => "Action is not enabled.");
			}

			protected override INetworkActionAccessibility CheckCanStartExecutionCore()
			{
				CheckCanStartExecutionInvoked = true;
				return new NetworkActionAccessibility(canExecute, null, () => "Action cannot execute.");
			}

			protected override INetworkActionResult ExecuteCore()
			{
				return null;
			}
		}
	}
}
