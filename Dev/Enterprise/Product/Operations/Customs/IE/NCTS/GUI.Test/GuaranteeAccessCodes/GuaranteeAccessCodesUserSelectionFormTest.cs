using System.Windows.Forms;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.GUI.Testing
{
	[TestedType(typeof(GuaranteeAccessCodesUserSelectionForm))]
	sealed class GuaranteeAccessCodesUserSelectionFormTest : ZFormBasherTest
	{
		public void TestShowFormType()
		{
			GuaranteeAccessCodesUserSelectionForm.ShowForm(header);
			AssertType<GuaranteeAccessCodesUserSelectionForm>("Dialog form type = GuaranteeAccessCodesUserSelectionForm", ZFormModaliser.LastFormShownDialogForTest);
		}

		[RequiresSTA]
		public void TestConfirmButton_ClickCore()
		{
			using (var form = new GuaranteeAccessCodesUserSelectionFormForTest(guaranteesObjectCollection))
			{
				form.Show();
				form.ConfirmButton_ClickCore_ForTest(Factory.NewWithValidTestData<CusGuaranteeHeader>());
				AssertType<GuaranteeAccessCodesSendingForm>("Dialog form type = GuaranteeAccessCodesSendingForm", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals(true, form.IsDisposed);
			}
		}

		protected override Form GetFormToBashCore()
		{
			SetUp();
			return GetForm();
		}

		GuaranteeAccessCodesUserSelectionForm GetForm()
		{
			return new GuaranteeAccessCodesUserSelectionForm(guaranteesObjectCollection);
		}
		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			guaranteesObjectCollection = new EU.NCTS.Business.GuaranteeAccessCodesUserSelectionObjectCollection(header);
			guaranteesObjectCollection.PopulateElements();
		}
		NctsHeader header;
		EU.NCTS.Business.GuaranteeAccessCodesUserSelectionObjectCollection guaranteesObjectCollection;

		sealed class GuaranteeAccessCodesUserSelectionFormForTest : GuaranteeAccessCodesUserSelectionForm
		{
			public GuaranteeAccessCodesUserSelectionFormForTest(EU.NCTS.Business.GuaranteeAccessCodesUserSelectionObjectCollection guarantees) : base(guarantees)
			{
			}

			public void ConfirmButton_ClickCore_ForTest(CusGuaranteeHeader cusGuaranteeHeader) => ConfirmButton_ClickCore(cusGuaranteeHeader);
		}
	}
}
