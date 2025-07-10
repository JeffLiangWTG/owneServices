using System;
using System.Windows.Forms;
using Enterprise.Accounting.Business.AccountingVoucherPrint;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.AccountingVoucherPrinting.Testing
{
	[TestedType(typeof(ChinaJournalListingPrintForm))]
	public class ChinaJournalListingPrintFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ChinaJournalListingPrintForm(new ChinaJournalListingPrintWrapper());
		}

		public void TestFormVerb()
		{
			var testWrapper = new ChinaJournalListingPrintWrapper();
			using (var testForm = new ChinaJournalListingPrintForm(testWrapper))
			{
				AssertEquals(testForm.FormVerb, "");
			}
		}

		[ExpectNoExceptions]
		public void TestPrintVoucherProceed()
		{
			var testWrapper = new ChinaJournalListingPrintWrapper();
			var formMock = new Mock<ChinaJournalListingPrintForm>(new object[] { testWrapper }) { CallBase = true };
			var testForm = formMock.Object;
			formMock
				.Protected()
				.Setup<bool>("HasWrapperValidationError")
				.Returns(false);
			formMock
				.Protected()
				.Setup<bool>("CanPrintChinaJournalListingProceed")
				.Returns(true);
			formMock.Protected()
				.Setup("PrintChinaJournalListingProceed");

			using (testForm)
			{
				testForm.GenerateButton_Click_ForTestOnly(null, new EventArgs());
				formMock.VerifyAll();
			}
		}

		[ExpectNoExceptions]
		public void TestPrintVoucherProceeIfPrintCannotProceed()
		{
			var testWrapper = new ChinaJournalListingPrintWrapper();
			var formMock = new Mock<ChinaJournalListingPrintForm>(new object[] { testWrapper }) { CallBase = true };
			var testForm = formMock.Object;

			formMock
				.Protected()
				.Setup<bool>("HasWrapperValidationError")
				.Returns(false);
			formMock
				.Protected()
				.Setup<bool>("CanPrintChinaJournalListingProceed")
				.Returns(false);

			using (testForm)
			{
				testForm.GenerateButton_Click_ForTestOnly(null, new EventArgs());
				formMock
					.Protected()
					.Verify("PrintChinaJournalListingProceed", Times.Never());
				formMock.VerifyAll();
			}
		}

		public void TestFormClose()
		{
			var testWrapper = new ChinaJournalListingPrintWrapper();
			using (var testForm = new ChinaJournalListingPrintForm(testWrapper))
			{
				testForm.Show();
				Assert(testForm.Visible);
				testForm.CloseButton_Click_ForTestOnly(this, new EventArgs());
				Assert(!testForm.Visible);
			}
		}
	}
}
