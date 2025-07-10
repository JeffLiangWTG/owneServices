using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(CPQAProductForm))]
	sealed class CPQAProductFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.Questions.AddNew();
			return new CPQAProductForm(pivot);
		}
	}
}
