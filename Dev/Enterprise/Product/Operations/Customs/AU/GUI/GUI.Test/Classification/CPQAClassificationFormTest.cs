using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(CPQAClassificationForm))]
	sealed class CPQAClassificationFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var classification = Factory.New<Classification>();
			classification.Questions.AddNew();
			return new CPQAClassificationForm(classification);
		}
	}
}
