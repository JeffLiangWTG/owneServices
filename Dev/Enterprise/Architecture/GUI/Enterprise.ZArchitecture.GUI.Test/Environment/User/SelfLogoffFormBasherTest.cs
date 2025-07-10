using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(SelfLogoffForm))]
	sealed class SelfLogoffFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new SelfLogoffForm();
		}

		public override bool AllowUntranslatableFormTitle()
		{
			return true;
		}

		protected override bool AllowFormSizeFixed => true;

		#endregion
	}
}
