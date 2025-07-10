using System.Windows.Forms;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(LPCOCollectionForm))]
	public class LPCOCollectionFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			JobComInvoiceLine oJobComInvoiceLine = Factory.New<JobComInvoiceLine>();
			return new LPCOCollectionForm(oJobComInvoiceLine);
		}
	}
}
