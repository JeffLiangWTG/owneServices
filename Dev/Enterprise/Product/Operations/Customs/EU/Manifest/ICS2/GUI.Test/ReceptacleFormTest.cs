using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI.Test
{
	[TestedType(typeof(ReceptacleForm))]
	sealed class ReceptacleFormTest : ZFormBasherTest
	{
		public void TestReceptacleCodeColumn()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			using var form = new TestReceptacleForm(manifestHeader);
			form.Show();
			var gridControl = form.FindSingle<ZGrid>("ReceptaclesGrid");
			var receptacleCodeColumn = gridControl.GetColumnStyle(Receptacle.Schema.CY_Data);
			AssertType<ZTextBoxColumnStyleInfo>(receptacleCodeColumn);
		}

		public void TestCancel()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			using var form = new TestReceptacleForm(manifestHeader);
			var header = form.manifestHeader;
			var receptacleIds = header.Receptacles;
			var initialItem = receptacleIds.AddNew();
			initialItem.CY_Data = "zzz";
			form.Show();
			var newItem = receptacleIds.AddNew();
			newItem.CY_Data = "yyy";
			AssertEquals("Should have 2 while editing", 2, receptacleIds.Count);

			form.DialogResult = DialogResult.Cancel;
			form.Close();
			AssertEquals("Should have 1 again after cancel", 1, receptacleIds.Count);
		}

		protected override Form GetFormToBashCore()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.HasChanges = false;
			Factory.Save();
			return new TestReceptacleForm(header);
		}

		class TestReceptacleForm : ReceptacleForm
		{
			public TestReceptacleForm(AsycudaManifestHeader manifestHeader)
				: base(manifestHeader)
			{
			}

			public new void OnClosing(CancelEventArgs e)
			{
				base.OnClosing(e);
			}
		}
	}
}


