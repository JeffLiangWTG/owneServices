using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.GUI.Testing
{
	[TestedType(typeof(ESH7ManifestFieldsUserControl))]
	sealed class ESH7ManifestFieldsUserControlTest : TestCaseWithFactory
	{
		public void TestCusAgentCodeFindBox()
		{
			using (var control = new ESH7ManifestFieldsUserControl())
			{
				control.Show();

				var cusAgentCodeFindBox = control.FindSingle<ZCodeFindBox>("CusAgentCodeFindBox");

				AssertEquals("BindingMember", "AMA_GS_NKCustomsAgent", cusAgentCodeFindBox.GetBindingMember());
			}
		}

		public void TestCertificateDropEdit()
		{
			using (var control = new ESH7ManifestFieldsUserControl())
			{
				control.Show();

				var certificateDropEdit = control.FindSingle<ZDropEdit>("CertificateDropEdit");

				AssertEquals("BindingMember", "AMA_CustomsProfile", certificateDropEdit.GetBindingMember());
			}
		}

		public void TestTrainingCheckBox()
		{
			using (var control = new ESH7ManifestFieldsUserControl())
			{
				control.Show();

				var trainingCheckBox = control.FindSingle<ZCheckBox>("TrainingCheckBox");

				AssertEquals("BindingMember", "TrainingEntry", trainingCheckBox.GetBindingMember());
			}
		}

		public void TestTransportDocumentTypeDropEdit()
		{
			using (var control = new ESH7ManifestFieldsUserControl())
			{
				control.Show();

				var transportDocumentTypeDropEdit = control.FindSingle<ZDropEdit>("TransportDocumentTypeDropEdit");

				AssertEquals("BindingMember", "TransportDocumentType", transportDocumentTypeDropEdit.GetBindingMember());
			}
		}

		public void TestTransportDocumentReferenceTextBox()
		{
			using (var control = new ESH7ManifestFieldsUserControl())
			{
				control.Show();

				var transportDocumentReferenceTextBox = control.FindSingle<ZTextBox>("TransportDocumentReferenceTextBox");

				AssertEquals("BindingMember", "TransportDocumentReference", transportDocumentReferenceTextBox.GetBindingMember());
			}
		}

		public void TestG3MRNToRevokeDropEdit()
		{
			using (var control = new ESH7ManifestFieldsUserControl())
			{
				control.Show();

				var g3MRNToRevokeDropEdit = control.FindSingle<ZDropEdit>("G3MRNToRevokeDropEdit");

				AssertEquals("BindingMember", "G3MRNToRevoke", g3MRNToRevokeDropEdit.GetBindingMember());
			}
		}

		public void TestEntryLineNumberTextBox()
		{
			using (var control = new ESH7ManifestFieldsUserControl())
			{
				control.Show();

				var entryLineNumberTextBox = control.FindSingle<ZTextBox>("EntryLineNumberTextBox");

				CombineAssertions(() =>
				{
					AssertEquals("BindingMember", "EntryLineNumber", entryLineNumberTextBox.GetBindingMember());

					KeySender.SendKeyPress(entryLineNumberTextBox, entryLineNumberTextBox.Handle, 'A');
					AssertEquals("Alphanumeric values are not allowed", "", entryLineNumberTextBox.Text);

					KeySender.SendKeyPress(entryLineNumberTextBox, entryLineNumberTextBox.Handle, '1');
					AssertEquals("Numeric values are allowed", "1", entryLineNumberTextBox.Text);
				});
			}
		}
	}
}
