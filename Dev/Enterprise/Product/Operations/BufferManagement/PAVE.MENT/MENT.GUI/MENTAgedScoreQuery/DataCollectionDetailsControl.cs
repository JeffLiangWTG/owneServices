using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.PAVE.MENT.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.PAVE.MENT.GUI
{
	public partial class DataCollectionDetailsControl : ZUserControl
	{
		public DataCollectionDetailsControl()
		{
			InitializeComponent();
		}

		MENTAcceptabilityBandViewModel ViewModel
		{
			get { return DataSource as MENTAcceptabilityBandViewModel; }
		}

		MENTAgedScoreQuery Query
		{
			get { return DataSource as MENTAgedScoreQuery; }
		}

		void openLinkedEntityZButton_Click(object sender, System.EventArgs e)
		{
			if (ViewModel != null)
			{
				if (ViewModel.Query.IsInDatabase)
				{
					OpenQuery(ViewModel.Query);
				}
				else
				{
					if (!ViewModel.MentEnabled)
					{
						Globals.Message.Show(Res.GetString("20cd8892-aa69-4a1b-bc3b-4d8f1d5a4ed4", "MENT is not enabled. To enable, check the \"MENT Enabled\" checkbox."));
					}
					else
					{
						var needsSave = ViewModel.Query.HasChanges || !ViewModel.Query.IsInDatabase;
						if (needsSave)
						{
							var result = Globals.Message.Show(Res.GetString("40ba0f70-ca12-4266-a5d9-031aad26f2da", "You must save this form before opening the linked entity. Would you like to save now?"), Res.GetString("2c52c331-908e-4c6b-b239-0f2b00a0fd76", "Cannot Open Linked Entity"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
							if (result == DialogResult.Yes)
							{
								needsSave = ((ZForm)FindForm()).FireSaveButton() == ContinueWithSave.No;
							}
						}

						if (needsSave)
						{
							return;
						}

						OpenQuery(ViewModel.Query);
					}
				}
			}
			else if (Query != null && Query.Linked && Query.RelatedAcceptabilityBand != null)
			{
				OpenAcceptabilityBand(Query.RelatedAcceptabilityBand);
			}
			else
			{
				Globals.Message.Show(Res.GetString("2b3b8505-e0b0-4879-8cfb-c765efb34fa5", "No linked entity exists for this query."));
			}
		}

		void OpenAcceptabilityBand(BMComponentAcceptabilityBand band)
		{
			ZControllerFactory.Create(ControllerIDs.AcceptabilityBand).ShowEditForm(band);
		}

		void OpenQuery(MENTAgedScoreQuery query)
		{
			ZControllerFactory.Create(ControllerIDs.MENTAgedScoreQuery).ShowEditForm(query);
		}
	}
}
