using System.Windows.Forms;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APInvoiceRequisitionForm))]
	internal sealed class APInvoiceRequisitionFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new APInvoiceRequisitionForm(new APTransactionHeaderCollectionHolder(Factory, new APTransactionHeaderCollection(Factory)));
		}

		#endregion
	}
}
