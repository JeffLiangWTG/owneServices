using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(ZEditAssayCodeForm))]
	sealed class ZEditAssayCodeFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new ZEditAssayCodeForm(Factory.New<JobComInvoiceLine>());
	}
}
