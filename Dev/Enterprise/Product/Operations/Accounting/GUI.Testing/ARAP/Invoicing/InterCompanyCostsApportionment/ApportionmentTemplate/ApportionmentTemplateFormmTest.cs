using System.Windows.Forms;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.IntercompanyCostsApportionment.Testing
{
	[TestedType(typeof(ApportionmentTemplateForm))]
	public class ApportionmentTemplateFormmTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ApportionmentTemplateForm(Factory.New<AccApportionmentTemplate>());
		}
	}
}
