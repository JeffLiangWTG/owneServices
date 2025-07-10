using System.Windows.Forms;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.HotCheque.Testing
{
	[TestedType(typeof(AccHotChequeForm))]
	public class AccHotChequeFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			AccHotCheque cheque = Factory.New<AccHotCheque>();
			return new AccHotChequeForm(cheque);
		}
	}
}
