using System.Windows.Forms;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(ReleaseStatusPrintForm))]
	sealed class ReleaseStatusPrintFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new ReleaseStatusPrintForm(Factory.New<JobDeclaration>());
	}
}
