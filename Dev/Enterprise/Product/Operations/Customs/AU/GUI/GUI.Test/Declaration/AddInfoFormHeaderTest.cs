using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(AddInfoForm))]
	sealed class AddInfoFormHeaderTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new AddInfoForm(Factory.New<JobDeclaration>().AddInfo);
	}
}
