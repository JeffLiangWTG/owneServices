using System.Windows.Forms;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(CalculateFreightNonAirForm))]
	class CalculateFreightNonAirFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var bizobj = CalculateFreightNonAirBizObj.New(invoice.Charges, declaration);
			return new CalculateFreightNonAirForm(bizobj);
		}
	}
}
