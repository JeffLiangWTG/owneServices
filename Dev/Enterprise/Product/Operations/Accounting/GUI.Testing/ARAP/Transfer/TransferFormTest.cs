using System;
using System.Windows.Forms;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Testing
{
	[TestedType(typeof(TransferForm))]
	abstract class TransferFormTest : AccountingZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var testTransfer = Transfer.New(transferType, Factory);
			testTransfer.HasChanges = false;
			return new TransferForm(testTransfer);
		}

		public void TestFormBorderStyle()
		{
			var transfer = Business.ARAP.Transfer.New(transferType, Factory);
			transfer.IsReverseTransaction = true;
			using (var transferForm = new TransferForm(transfer))
			{
				AssertEquals("Should be the default value", FormBorderStyle.Sizable, transferForm.FormBorderStyle);
			}
		}

		public abstract Type transferType { get; }

		protected override bool ShouldHaveAuditPlugIn => true;
	}
}
