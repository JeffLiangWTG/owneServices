using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.Customs.MY.GUI.Testing
{
	[TestedType(typeof(CusClassificationForm))]
	sealed class CusClassificationFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new CusClassificationForm(Factory.New<Business.CusClassification>());
	}
}
