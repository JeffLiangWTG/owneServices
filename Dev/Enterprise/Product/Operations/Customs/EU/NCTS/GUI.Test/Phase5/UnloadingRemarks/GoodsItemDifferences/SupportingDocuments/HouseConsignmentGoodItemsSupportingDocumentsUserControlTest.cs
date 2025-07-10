using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class HouseConsignmentGoodItemsSupportingDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestSequenceNo()
		{
			using (var form = new ZForm(supportingInfo))
			using (var control = new HouseConsignmentGoodItemsSupportingDocumentsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var sequenceNumberTextBox = control.FindSingle<ZTextBox>("SequenceNumberTextBox");

				AssertType<ZTextBox>("Sequence Number text box of the correct type", sequenceNumberTextBox);
			}
		}

		[RequiresSTA]
		public void TestDocumentType()
		{
			using (var form = new ZForm(supportingInfo))
			using (var control = new HouseConsignmentGoodItemsSupportingDocumentsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var docTypeCodeFindBox = control.FindSingle<ZCodeFindBox>("DocTypeCodeFindBox");

				AssertType<ZCodeFindBox>("Document type drop edit of the correct type", docTypeCodeFindBox);
			}
		}

		public void TestReferenceNumber()
		{
			using (var form = new ZForm(supportingInfo))
			using (var control = new HouseConsignmentGoodItemsSupportingDocumentsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var referenceNumberTextBox = control.FindSingle<ZTextBox>("ReferenceNumberTextBox");

				AssertType<ZTextBox>("Reference number text box of the correct type", referenceNumberTextBox);
			}
		}

		public void TestComplementOfInformation()
		{
			using (var form = new ZForm(supportingInfo))
			using (var control = new HouseConsignmentGoodItemsSupportingDocumentsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var complementInfoTextBox = control.FindSingle<ZTextBox>("ComplementInfoTextBox");

				AssertType<ZTextBox>("Complement info text box of the correct type", complementInfoTextBox);
			}
		}

		public void TestStatusLabel()
		{
			using (var form = new ZForm(supportingInfo))
			using (var control = new HouseConsignmentGoodItemsSupportingDocumentsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var statusLabelControl = control.FindSingle<ZLabel>("StatusLabel");

				AssertType<ZLabel>("StatusLabel of the correct type", statusLabelControl);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			supportingInfo = Factory.NewWithValidTestData<NctsSupportingDocument>();
		}
		NctsSupportingDocument supportingInfo;
	}
}
