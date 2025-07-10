using System.Windows.Forms;
using Enterprise.Customs.JP.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(CusClassificationForm))]
	sealed class CusClassificationFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new CusClassificationForm(Factory.New<CusClassification>());
	}
}
