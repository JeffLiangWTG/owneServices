using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(NationalAdditionalCodesForm))]
	public class NationalAdditionalCodesFormTest : ZFormBasherTest
	{
		public void TestValidate()
		{
			var declaration = Factory.New<Business.Declaration.JobDeclaration>();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var collection = new NationalAdditionalCodeCollection(invoiceLine.JI_AdditionalSupplementsInfo, 8);
			var code = collection.AddNew();
			var nationalAdditionalCodeSupporterMock = new Mock<INationalAdditionalCodeSupporter>();
			nationalAdditionalCodeSupporterMock
				.Setup(x => x.NationalAdditionalCodes)
				.Returns(collection);
			using (var form = new TestNationalAdditionalCodesForm(nationalAdditionalCodeSupporterMock.Object))
			{
				var e = new CancelEventArgs(false);
				code.CY_Code = "zzzz";
				form.OnClosing(e);
				AssertEquals("No validation errors, should not be cancelled", false, e.Cancel);

				e = new CancelEventArgs(false);
				var nationalAdditionalCodeProvider = NationalAdditionalCodeProvider.GetByNationalAdditionalCodeSupporter(invoiceLine);
				for (int i = 0; i < nationalAdditionalCodeProvider.NumberOfCodes; i++)
				{
					collection.AddNew();
				}
				form.OnClosing(e);
				AssertEquals("Validation errors exists, should be cancelled", true, e.Cancel);
			}
		}

		public void TestCancel()
		{
			var declaration = Factory.New<Business.Declaration.JobDeclaration>();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var collection = new NationalAdditionalCodeCollection(invoiceLine.JI_AdditionalSupplementsInfo, 8);
			var nationalAdditionalCodeSupporterMock = new Mock<INationalAdditionalCodeSupporter>();
			nationalAdditionalCodeSupporterMock
				.Setup(x => x.NationalAdditionalCodes)
				.Returns(collection);
			using (var form = new TestNationalAdditionalCodesForm(nationalAdditionalCodeSupporterMock.Object))
			{
				var initialItem = form.nationalAdditionalCodes.AddNew();
				initialItem.CY_Code = "zzz";
				form.Show();
				var newItem = form.nationalAdditionalCodes.AddNew();
				newItem.CY_Code = "yyy";
				AssertEquals("Should have 2 while editing", 2, form.nationalAdditionalCodes.Count);

				form.DialogResult = DialogResult.Cancel;
				form.Close();
				AssertEquals("Should have 1 again after cancel", 1, form.nationalAdditionalCodes.Count);
			}
		}

		public void TestNationalAdditionalCodeColumn()
		{
			var declaration = Factory.New<Business.Declaration.JobDeclaration>();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var collection = new NationalAdditionalCodeCollection(invoiceLine.JI_AdditionalSupplementsInfo, 8);
			var nationalAdditionalCodeSupporterMock = new Mock<INationalAdditionalCodeSupporter>();
			nationalAdditionalCodeSupporterMock
				.Setup(x => x.NationalAdditionalCodes)
				.Returns(collection);
			using (var form = new TestNationalAdditionalCodesForm(nationalAdditionalCodeSupporterMock.Object))
			{
				var control = (ZArchitecture.ZGrid)form.Controls.Find("nationalAdditionalCodesGrid", true).FirstOrDefault();
				var nationalAdditionalCodeColumn = control.GetColumnStyle(NationalAdditionalCode.Schema.CY_Code);
				AssertType<ZDropEditColumnStyleInfo>(nationalAdditionalCodeColumn);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.New<Business.Declaration.JobDeclaration>();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var collection = new NationalAdditionalCodeCollection(invoiceLine.JI_AdditionalSupplementsInfo, 8);
			var nationalAdditionalCodeSupporterMock = new Mock<INationalAdditionalCodeSupporter>();
			nationalAdditionalCodeSupporterMock
				.Setup(x => x.NationalAdditionalCodes)
				.Returns(collection);
			return new NationalAdditionalCodesForm(nationalAdditionalCodeSupporterMock.Object);
		}

		protected class TestNationalAdditionalCodesForm : NationalAdditionalCodesForm
		{
			public TestNationalAdditionalCodesForm(INationalAdditionalCodeSupporter supporter)
				: base(supporter)
			{
			}

			public new void OnClosing(CancelEventArgs e)
			{
				base.OnClosing(e);
			}
		}
	}
}
