using System.Windows.Forms;
using Enterprise.Customs.AE.Business;
using Enterprise.Customs.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.AE.GUI.Testing;

[TestedType(typeof(BaseClassificationForm))]
sealed class CusClassificationFormBasherTest : ZArchitecture.GUI.Testing.ZFormBasherTest
{
	protected override Form GetFormToBashCore()
	{
		var classification = Factory.New<CusClassification>();
		return new BaseClassificationForm(classification);
	}
}
