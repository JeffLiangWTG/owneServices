using System.Windows.Forms;
using Enterprise.Accounting.Business.GLAccountFormat;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.GLAccountFormat.Testing
{
	[TestedType(typeof(FormatChangeForm))]
	public class FormatChangeFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new FormatChangeForm(new GLAccountFormatter());
		}
	}
}
