using System.Windows.Forms;
using Enterprise.Customs.IN.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(CusClassificationForm))]
sealed class CusClassificationFormBasherTest : ZArchitecture.GUI.Testing.ZFormBasherTest
{
	protected override Form GetFormToBashCore() => new CusClassificationForm(Factory.New<CusClassification>());
}
