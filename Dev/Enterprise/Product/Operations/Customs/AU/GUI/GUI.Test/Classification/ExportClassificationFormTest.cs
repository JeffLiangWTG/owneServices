using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(ExportClassificationForm))]
	sealed class ExportClassificationFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new ExportClassificationForm(Factory.New<Classification>());
	}
}
