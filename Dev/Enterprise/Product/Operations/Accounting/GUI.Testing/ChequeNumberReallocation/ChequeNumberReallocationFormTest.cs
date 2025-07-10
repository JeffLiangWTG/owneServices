using System.Windows.Forms;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(ChequeNumberReallocationForm))]
	public class ChequeNumberReallocationFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ChequeNumberReallocationForm(new ChequeNumberReallocator(Factory, Factory.New<AccChequeBook>(), System.Array.Empty<TransactionHeader>(), false));
		}
	}
}
