using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	[TestedType(typeof(FilterGroupNameForm))]
	sealed class FilterGroupNameFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new FilterGroupNameForm();
		}
	}
}
