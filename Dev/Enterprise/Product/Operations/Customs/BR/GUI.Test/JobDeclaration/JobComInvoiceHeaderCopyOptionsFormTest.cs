using System.Windows.Forms;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(JobComInvoiceHeaderCopyOptionsForm))]
	class JobComInvoiceHeaderCopyOptionsFormTest : ZFormBasherTest
	{
		public void TestCaptions()
		{
			using (var form = GetFormToBashCore() as JobComInvoiceHeaderCopyOptionsForm)
			{
				AssertEquals("Caption should be", "Type of Copy", form.FormCaption);
			}
		}

		public void TestFields()
		{
			using (var control = new JobComInvoiceHeaderCopyOptionsForm(new JobComInvoiceHeaderCopyOptions()))
			{
				AssertType<ZRadioButton>("OnlyInvoiceItemsRadioButton must be ZRadioButton", control.OnlyLinesRequireLicenseRadioButton);
				AssertType<ZRadioButton>("AllItemsRadioButton must be ZRadioButton", control.AllLinesRadioButton);
				AssertType<ZButton>("ConfirmButton must be ZButton", control.ConfirmButton);
				AssertType<ZButton>("Cancel2Button must be ZButton", control.Cancel2Button);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new JobComInvoiceHeaderCopyOptionsForm(new JobComInvoiceHeaderCopyOptions());
		}

		#endregion
	}
}
