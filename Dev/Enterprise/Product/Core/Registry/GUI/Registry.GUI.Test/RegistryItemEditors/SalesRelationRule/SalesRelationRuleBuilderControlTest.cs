using System.Drawing;
using System.Windows.Forms;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(SalesRelationRuleBuilderControlTest.SalesRelationRuleBuilderControlFormForTesting))]
	sealed class SalesRelationRuleBuilderControlTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var rule = new SalesRelationDirectionRule();
			return new SalesRelationRuleBuilderControlFormForTesting(rule);
		}

		#region Classes

		public class SalesRelationRuleBuilderControlFormForTesting : ZForm
		{
			public SalesRelationRuleBuilderControlFormForTesting(SalesRelationDirectionRule rule)
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

			readonly SalesRelationDirectionRuleControl control = new SalesRelationDirectionRuleControl();
		}

		#endregion

		#endregion
	}
}
