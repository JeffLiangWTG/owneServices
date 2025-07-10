using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class HouseConsignmentAdditionalDocumentsDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestSequenceNo()
		{
			using (var form = new ZForm(document))
			using (var control = new HouseConsignmentAdditionalDocumentsDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var sequenceNumberTextBox = control.FindSingle<ZTextBox>("SequenceNumberTextBox");

				AssertType<ZTextBox>("Sequence Number text box of the correct type", sequenceNumberTextBox);
				AssertEquals("Sequence Number text box should be disabled", true, sequenceNumberTextBox.ReadOnly);
			}
		}

		public void TestStatusLabels()
		{
			using (var form = new ZForm(document))
			using (var control = new HouseConsignmentAdditionalDocumentsDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var statusLabel = control.FindSingle<ZLabel>("StatusLabel");

				AssertType<ZLabel>("Kind status label of the correct type", statusLabel);
				AssertEquals(false, new LabelCaptionRenderProvider().GetLabelCaptionVisible(statusLabel));
			}
		}

		public void TestKind()
		{
			using (var form = new ZForm(document))
			using (var control = new HouseConsignmentAdditionalDocumentsDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var kindDropEdit = control.FindSingle<ZDropEdit>("KindDropEdit");

				AssertType<ZDropEdit>("Kind drop edit of the correct type", kindDropEdit);
				AssertEquals("Kind drop edit should be disabled", true, kindDropEdit.ReadOnly);
			}
		}

		[RequiresSTA]
		public void TestDocumentType()
		{
			using (var form = new ZForm(document))
			using (var control = new HouseConsignmentAdditionalDocumentsDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var docTypeCodeFindBox = control.FindSingle<ZCodeFindBox>("DocTypeCodeFindBox");

				AssertType<ZCodeFindBox>("Document type drop edit of the correct type", docTypeCodeFindBox);
			}
		}

		public void TestReferenceNumber()
		{
			using (var form = new ZForm(document))
			using (var control = new HouseConsignmentAdditionalDocumentsDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var referenceNumberTextBox = control.FindSingle<ZTextBox>("ReferenceNumberTextBox");

				AssertType<ZTextBox>("Reference number text box of the correct type", referenceNumberTextBox);
			}
		}

		[RequiresSTA]
		public void TestText()
		{
			using (var form = new ZForm(document))
			using (var control = new HouseConsignmentAdditionalDocumentsDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var textTextBox = control.FindSingle<ZTextBox>("TextTextBox");

				AssertType<ZTextBox>("Text text box of the correct type", textTextBox);
			}
		}

		public void TestFieldsDisabledForUnloadedStateDEC()
		{
			using (var form = new ZForm(document))
			using (var control = new HouseConsignmentAdditionalDocumentsDetailsUserControl())
			{
				document.CSI_Status = "DEC";

				form.Controls.Add(control);
				form.Show();

				var sequenceNumberTextBox = control.FindSingle<ZTextBox>("SequenceNumberTextBox");
				var kindDropEdit = control.FindSingle<ZDropEdit>("KindDropEdit");
				var documentTypeDropEdit = control.FindSingle<ZCodeFindBox>("DocTypeCodeFindBox");
				var referenceNumberTextBox = control.FindSingle<ZTextBox>("ReferenceNumberTextBox");
				var textTextBox = control.FindSingle<ZTextBox>("TextTextBox");

				AssertEquals("Text text box should be disabled for unloaded state DEC", true, textTextBox.ReadOnly);
				AssertEquals("Reference number text box should be disabled for unloaded state DEC", true, referenceNumberTextBox.ReadOnly);
				AssertEquals("Document type drop edit should be disabled for unloaded state DEC", true, documentTypeDropEdit.ReadOnly);
				AssertEquals("Kind drop edit should be disabled for unloaded state DEC", true, kindDropEdit.ReadOnly);
				AssertEquals("Sequence number text box should be disabled for unloaded state DEC", true, sequenceNumberTextBox.ReadOnly);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var bill = nctsHeader.Bills.AddNew();
			document = bill.AdditionalDocuments.AddNew();
		}
		NctsBillAdditionalDocument document;
	}
}
