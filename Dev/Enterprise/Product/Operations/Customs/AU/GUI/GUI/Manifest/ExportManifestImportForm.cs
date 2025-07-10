using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.ExportManifest.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	internal partial class ExportManifestImportForm : ZChildForm
	{
		public ExportManifestImportForm(TemporaryManifestHolder holder)
			: base(holder)
		{
			InitializeComponent();
			this.holder = holder;
		}

		bool Generate()
		{
			foreach (TemporaryManifest manifest in holder.Manifests)
			{
				if (!manifest.ShouldSave)
				{
					continue;
				}

				if (manifest.ExportManifest == null)
				{
					manifest.ExportManifest = manifest.CalcExportManifest.ApplyTo(holder.Factory);
				}
				else if (manifest.ExportManifest.HaveMessagesBeenSentToCustoms)
				{
					const string messageFormat =
						"Manifest for {0} - {1} - {2} - {3} is already reported to Customs.\r\n" +
						"If you press OK you will be redirected to Customs Export Manifest screen to review the changes and generate an amendment message to Customs.\r\n\r\n" +
						"Note. If you have added any CAN's manually to the Customs Export Manifest, they will be OVERRIDDEN by the latest data gathered from Bookings/Bills.";

					ZString messageText = string.Format(messageFormat,
						manifest.ExportManifest.Vessel.RV_Code,
						manifest.ExportManifest.ED_VoyageNumber,
						manifest.ExportManifest.ED_RL_NKPortOfDeparture,
						manifest.ExportManifest.ED_RN_NKCountryOfDestination);

					if (Globals.Message.Show(messageText, "Manifest has already been submitted", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
					{
						BusinessObjectFactory newFactory = new BusinessObjectFactory();
						ExportCustomsManifestHeader newExportManifest = newFactory.Load<ExportCustomsManifestHeader>(manifest.ExportManifest.PK);
						using (ZForm form = new ExportManifestForm(manifest.CalcExportManifest.ApplyTo(newExportManifest)))
						{
							ZFormModaliser.ShowDialogWithoutDispose(form);
						}
					}
				}
				else
				{
					const string messageFormat =
						"The manifest for {0} - {1} - {2} - {3} already exists, but the customs process has not started yet.\r\n" +
						"Are you sure you would like to override the existing Customs Export Manifest for {4} with the latest data from Bookings/Bills?\r\n\r\n" +
						"Note. If you've added any CAN's manually to the Customs Export Manifest, they will be OVERRIDDEN by the latest data gathered from Bookings/Bills.";

					ZString messageText = string.Format(messageFormat,
						manifest.ExportManifest.Vessel.RV_Code,
						manifest.ExportManifest.ED_VoyageNumber,
						manifest.ExportManifest.ED_RL_NKPortOfDeparture,
						manifest.ExportManifest.ED_RN_NKCountryOfDestination,
						manifest.ExportManifest.ED_RL_NKPortOfDeparture);

					if (Globals.Message.Show(messageText, "Manifest already exists", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					{
						manifest.CalcExportManifest.ApplyTo(manifest.ExportManifest);
					}
				}
			}

			try
			{
				holder.Factory.Save();
				return true;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				HandleSaveException(ex);
			}

			return false;
		}

		public override string FormCaption
		{
			get { return string.Format("Create Export Manifests for Vessel: {0}, Voyage: {1}", holder.VesselName, holder.VoyageNumber); }
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		readonly TemporaryManifestHolder holder;

		void SaveButton_Click(object sender, EventArgs e)
		{
			ZString errorMessage = ZString.Empty;
			foreach (TemporaryManifest manifest in holder.Manifests)
			{
				if (manifest.IsDuplicated && manifest.ShouldSave)
				{
					errorMessage += string.Format("{0}-{1}-Port of Departure:{2} (Folio:{3})\r\n", manifest.ExportManifest.Vessel.RV_Code, manifest.ExportManifest.ED_VoyageNumber, manifest.ExportManifest.ED_RL_NKPortOfDeparture.IsEmpty ? new ZString("Blank") : manifest.ExportManifest.ED_RL_NKPortOfDeparture, manifest.ExportManifest.ED_FolioReference);
				}
			}

			if (errorMessage.IsEmpty)
			{
				if (Generate())
				{
					Close();
				}
			}
			else
			{
				ZString errorFormat = "You have already created under Customs Export Manifest manifests with the following identical details:\r\n" +
					"{0}\r\n" +
					"In order to continue with automated upgrade you can do one of the following:\r\n" +
					"1) manually combine the above manifests into a single manifest;\r\n" +
					"2) ensure each of the Customs Export Manifests have a unique combination of Vessel-Voyage-Port of Departure.\r\n" +
					"3) proceed with manual Export Manifest data entry.";

				Globals.Message.ShowError(string.Format(errorFormat, errorMessage));
			}
		}

		void CancellButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
