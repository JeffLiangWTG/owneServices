using System.Windows.Forms;
using Enterprise.Customs.DE.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(CreateDeclarationForm))]
	sealed class CreateDeclarationFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var bizO = new CreateDeclarationBizObj(createFromWarehouseOrder: false);
			return new CreateDeclarationForm(bizO);
		}

		protected override bool ShouldTestFormIsFullyTranslatable => false;
	}
}
