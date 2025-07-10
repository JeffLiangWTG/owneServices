using System.Windows.Forms;
using Enterprise.Customs.CH.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(CusClassificationForm))]
sealed class CusClassificationFormBasherTest : ZArchitecture.GUI.Testing.ZFormBasherTest
{
	protected override Form GetFormToBashCore() => new CusClassificationForm(Factory.New<CusClassification>());
}
