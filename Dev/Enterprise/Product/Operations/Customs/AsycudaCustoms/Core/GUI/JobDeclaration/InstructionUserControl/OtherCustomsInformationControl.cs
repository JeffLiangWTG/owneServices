using System;
using System.Windows.Forms;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	public partial class OtherCustomsInformationControl : ZUserControl
	{
		public OtherCustomsInformationControl()
		{
			InitializeComponent();
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			var instruction = EntryInstruction;
			if (instruction != null)
			{
				instruction.OnLocalReferenceNumberChangedToReMerge -= OnLocalReferenceNumberChangedToReMerge;
				instruction.OnLocalReferenceNumberChangedToEmpty -= OnLocalReferenceNumberChangedToEmpty;
				instruction.OnLocalReferenceNumberChangedWhenHasWarehouseTransaction -= OnLocalReferenceNumberChangedWhenHasWarehouseTransaction;
			}
			base.OnCurrentDataItemChanging(e);
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			var instruction = EntryInstruction;
			if (instruction != null)
			{
				instruction.OnLocalReferenceNumberChangedToReMerge += OnLocalReferenceNumberChangedToReMerge;
				instruction.OnLocalReferenceNumberChangedToEmpty += OnLocalReferenceNumberChangedToEmpty;
				instruction.OnLocalReferenceNumberChangedWhenHasWarehouseTransaction += OnLocalReferenceNumberChangedWhenHasWarehouseTransaction;
			}
		}

		void OnLocalReferenceNumberChangedToReMerge(object sender, System.ComponentModel.CancelEventArgs e)
		{
			var confirmationMessage = Res.GetString("7C552068-0A98-49BB-852A-AAD9EB65B9AB", "Please note that changing the Local Reference Number may invalidate various external documents. Please take the necessary precautions to ensure compliance. The entries will be re-merged.\r\n\r\nSelect 'Yes' to change the Local Reference Number");
			if (Globals.Message.Show(confirmationMessage, Res.GetString("ACAB4D0D-7E41-4A27-8A76-BADDE0A06BE1", "Local Reference Number Change"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes) != DialogResult.Yes)
			{
				e.Cancel = true;
			}
		}

		void OnLocalReferenceNumberChangedToEmpty(object sender, EventArgs e)
		{
			Globals.Message.Show(Res.GetString("3F3765CC-E707-4B4E-A83E-E17EE4828633", "Local Reference Number cannot be empty as it may invalidate external documents."));
		}

		void OnLocalReferenceNumberChangedWhenHasWarehouseTransaction(object sender, EventArgs e)
		{
			Globals.Message.Show(Res.GetString("B05884AA-4848-40C2-B926-FA98D13C3BD2", "Local Reference Number cannot be changed as the entry header linked with this entry instruction has been synchronized to Bonded Warehouse."));
		}

		CusEntryInstruction EntryInstruction => CurrentDataItem as CusEntryInstruction;
	}
}
