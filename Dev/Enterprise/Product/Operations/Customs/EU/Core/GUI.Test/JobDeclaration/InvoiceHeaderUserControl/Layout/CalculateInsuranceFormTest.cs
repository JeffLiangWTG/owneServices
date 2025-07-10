using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Moq;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class CalculateInsuranceFormTest : TestCaseWithFactory
	{
		public void TestInvoiceAmountTextBox()
		{
			var textBox = form.FindSingle<ZTextBox>("InvoiceAmountTextBox");
			CombineAssertions(() =>
			{
				AssertEquals("BindingMember", nameof(CalculateInsuranceBizObj.InvoiceAmount), textBox.GetBindingMember());
				AssertEquals("TabStop", false, textBox.TabStop);
			});
		}

		public void TestCurrencyTextBox()
		{
			var textBox = form.FindSingle<ZTextBox>("CurrencyTextBox");
			CombineAssertions(() =>
			{
				AssertEquals("BindingMember", nameof(CalculateInsuranceBizObj.Currency), textBox.GetBindingMember());
				AssertEquals("TabStop", false, textBox.TabStop);
			});
		}

		public void TestInsuranceAmountTextBox()
		{
			var textBox = form.FindSingle<ZTextBox>("InsuranceAmountTextBox");
			CombineAssertions(() =>
			{
				AssertEquals("BindingMember", nameof(CalculateInsuranceBizObj.InsuranceAmount), textBox.GetBindingMember());
				AssertEquals("TabStop", false, textBox.TabStop);
			});
		}

		public void TestInsurancePercentageCalcEdit()
		{
			var calcEdit = form.FindSingle<ZCalcEdit>("InsurancePercentageCalcEdit");
			CombineAssertions(() =>
			{
				AssertEquals("BindingMember", nameof(CalculateInsuranceBizObj.InsurancePercentage), calcEdit.GetBindingMember());
				AssertEquals("TabStop", true, calcEdit.TabStop);
			});
		}

		public void TestDutiablePercentCalcEdit()
		{
			var calcEdit = form.FindSingle<ZCalcEdit>("DutiablePercentCalcEdit");
			CombineAssertions(() =>
			{
				AssertEquals("BindingMember", nameof(CalculateInsuranceBizObj.DutiablePercent), calcEdit.GetBindingMember());
				AssertEquals("TabStop", true, calcEdit.TabStop);
			});
		}

		public void TestDutiablePercentCalcEdit_ShouldBeVisible_WhenIsDutiablePercentEnabled()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var chargeFactory = (EUIncoTermAndCustomsChargeFactory)declaration.IncoTermAndChargeFactory;
			var bizObjMock = new Mock<CalculateInsuranceBizObj>(chargeFactory, invoice) { CallBase = true };

			using (var calculateInsuranceForm = new CalculateInsuranceForm(bizObjMock.Object))
			{
				calculateInsuranceForm.Show();
				var calcEdit = calculateInsuranceForm.FindSingle<ZCalcEdit>("DutiablePercentCalcEdit");
				AssertEquals("Precondition", true, bizObjMock.Object.IsDutiablePercentEnabled);
				AssertEquals(true, calcEdit.Visible);
			}

			bizObjMock.Setup(m => m.IsDutiablePercentEnabled).Returns(false);
			using (var calculateInsuranceForm = new CalculateInsuranceForm(bizObjMock.Object))
			{
				calculateInsuranceForm.Show();
				var calcEdit = calculateInsuranceForm.FindSingle<ZCalcEdit>("DutiablePercentCalcEdit");
				AssertEquals(false, calcEdit.Visible);
			}
		}

		public void TestIncludedInLinesZCheckBox()
		{
			var checkBox = form.FindSingle<ZCheckBox>("IncludedInLinesZCheckBox");
			CombineAssertions(() =>
			{
				AssertEquals("BindingMember", nameof(CalculateInsuranceBizObj.IsInsuranceIncludedInLines), checkBox.GetBindingMember());
				AssertEquals("TabStop", true, checkBox.TabStop);
			});
		}

		public void TestFormProperties()
		{
			CombineAssertions(() =>
			{
				AssertEquals("DataSourceType", typeof(CalculateInsuranceBizObj), form.DataSourceType);
				AssertEquals("No verb shown on form", string.Empty, form.FormVerb);
				AssertEquals("Fixed Tool Window border style", form.FormBorderStyle, FormBorderStyle.FixedToolWindow);
				AssertEquals("Caption", "Calculate Insurance", form.CaptionResourceString.Caption);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var chargeFactory = (EUIncoTermAndCustomsChargeFactory)declaration.IncoTermAndChargeFactory;
			var bizObj = new CalculateInsuranceBizObj(chargeFactory, invoice);
			form = new CalculateInsuranceForm(bizObj);
		}
		CalculateInsuranceForm form;

		protected override void TearDown()
		{
			base.TearDown();
			form.Dispose();
		}
	}
}
