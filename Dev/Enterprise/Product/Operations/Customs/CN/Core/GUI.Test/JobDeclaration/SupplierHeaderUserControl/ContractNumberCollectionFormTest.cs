using System.Windows.Forms;
using Enterprise.Customs.CN.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.GUI.Testing
{
	[TestedType(typeof(ContractNumberCollectionForm))]
	class ContractNumberCollectionFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var dec = Factory.New<JobDeclaration>();
			var header = dec.Invoices.AddNew();
			header.ContractNumbersAsString = "123";
			Factory.Save();
			return new ContractNumberCollectionForm(header.ContractNumbers);
		}
	}
}
