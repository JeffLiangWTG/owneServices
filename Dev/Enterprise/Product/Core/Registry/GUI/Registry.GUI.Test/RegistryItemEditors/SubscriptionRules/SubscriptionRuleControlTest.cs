using System.Drawing;
using System.Windows.Forms;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(SubscriptionRuleControlTest.SubscriptionRuleBuilderControlFormForTesting))]
	sealed class SubscriptionRuleControlTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var rule = new SubscriptionRule();
			return new SubscriptionRuleBuilderControlFormForTesting(rule);
		}

		#endregion

		#region Classes

		public class SubscriptionRuleBuilderControlFormForTesting : ZForm
		{
			public SubscriptionRuleBuilderControlFormForTesting(SubscriptionRule rule)
				: base(rule)
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				Size = new Size(1200, 768);

				Controls.Add(control);
				BindingSource.SetBindingMember(control, ".");
				CaptionRenderingEnabled = true;
			}

			readonly SubscriptionRuleControl control = new SubscriptionRuleControl();
		}

		#endregion

	}
}
