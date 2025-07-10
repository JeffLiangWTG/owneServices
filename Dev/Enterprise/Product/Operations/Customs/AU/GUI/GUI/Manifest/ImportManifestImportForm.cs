using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	internal partial class ImportManifestImportForm : ZChildForm
	{
		public ImportManifestImportForm(ImportManifestItemCollection gridItems)
			: base(gridItems)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitializeComponent();
			base.InitialiseForm();
		}

		void SaveButton_Click(object sender, EventArgs e)
		{
			if (FireSaveButton() == ContinueWithSave.Yes)
			{
				Close();
			}
		}

		void CancellButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void SelectButton_Click(object sender, EventArgs e)
		{
			SelectItems(ArrivalPortsDropEdit.Text, true);
		}

		void DeselectButton_Click(object sender, EventArgs e)
		{
			SelectItems(ArrivalPortsDropEdit.Text, false);
		}

		void FindButton_Click(object sender, EventArgs e)
		{
			((ImportManifestItemCollection)BusinessEntity).RefreshItemsView();
		}

		public void SelectItems(ZString arrivalPort, bool shouldSave)
		{
			if (ArrivalPortsDropEdit.Text != "ALL")
			{
				((ImportManifestItemCollection)BusinessEntity).SelectItems(ArrivalPortsDropEdit.Text, shouldSave);
			}
			else
			{
				((ImportManifestItemCollection)BusinessEntity).SelectAll(shouldSave);
			}
		}
	}
}
