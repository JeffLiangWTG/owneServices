using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IdentityCertificate.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Res = ZClientEDI.Res;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.IdentityCertificate
{
	public class EdiIdentityCertificateModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ClientModuleRegistration.EdiIdentityCertificate;

		public override SecurityCheckpoint SecurityCheckpoint => EDISecurityCheckpoints.EdiIdentityCertificates;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ClientControllerRegistration.EdiIdentityCertificate);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new EdiIdentityCertificateFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new EdiIdentityCertificateFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new EdiIdentityCertificateCollection(Factory);
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var menuItemCollection = new List<MenuItem>(base.GetNewActionMenuItems());

			if (CanEditCertificate)
			{
				var cancelCertMenu = new ZMenuItem(Res.GetString("CBAFFA14-90E0-493C-A5FC-F6286F8B38A9", "Revoke Certificate"), RevokeCert_Click);
				menuItemCollection.Add(cancelCertMenu);
				var resetToQUEMenu = new ZMenuItem(Res.GetString("4B96030D-642C-4B96-A080-55BFEEA66796", "Reset Status To QUE"), ResetStatusToQUE);
				menuItemCollection.Add(resetToQUEMenu);
			}

			if (CanDownloadCertificate)
			{
				var downloadCertificateMenuItem = new ZMenuItem(ResString.GetMultilingualString("AB7AB745-7A20-4845-B917-F2C77D3434F0", "Download Certificate"), DownloadCertificate_Click);
				menuItemCollection.Add(downloadCertificateMenuItem);
			}
			return menuItemCollection.ToArray();
		}

		#region Handle Revoke Certificate

		bool CanEditCertificate => EDISecurityCheckpoints.EdiIdentityCertificateEditCertificate.IsAllowed;

		void RevokeCert_Click(object sender, EventArgs e)
		{
			var certSelections = SelectedBusinessObjects.Cast<EdiIdentityCertificate>().ToArray();
			if (certSelections.Length > 0)
			{
				var shouldGlobalTip = certSelections.Count(t => t.ICE_ProcessingStatus == EdiIdentityCertificateProcessingStatus.Codes.COM) != certSelections.Length;
				if (shouldGlobalTip)
				{
					Globals.Message.Show(Res.GetString("035FC937-15D1-4022-BDED-3555513BBFB4", "All selected records must be in the completed state."));
				}
				else
				{
					if (Globals.Message.ShowConfirmation(Res.GetString("AD614E71-CF13-4347-9B38-338566E5B640", "You are about to revoke this certificate and remove the corresponding certificate in application from our Azure AD B2C server.\r\nAre you sure you want to proceed?"), Res.GetString("74AA1EB0-8DDC-4042-A9BF-267069566606", "Warning"), "Yes", ZMessageBoxIcon.Warning) == ZDialogResult.OK)
					{
						CancelCertificateCore(certSelections);
					}
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("5B4D9683-DEA4-4765-9CAB-26FEE52BDEFC", "Please select Certificate to revoke."));
			}
		}

		void CancelCertificateCore(EdiIdentityCertificate[] certSelections)
		{
			foreach (var cert in certSelections)
			{
				cert.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.CAN;
			}
			try
			{
				Factory.Save();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("OnboardingCancelCertificate", ex.Message, ex);
			}
		}

		#endregion

		#region Handle Download Certificate

		void DownloadCertificate_Click(object sender, EventArgs e)
		{
			var downloadSelections = SelectedBusinessObjects.Cast<EdiIdentityCertificate>().ToArray();
			if (downloadSelections.Length > 0)
			{
				if (downloadSelections.Length == 1)
				{
					EdiIdentityCertificate ediIdentityCertificate = downloadSelections[0];
					if (ediIdentityCertificate.ICE_ProcessingStatus == EdiIdentityCertificateProcessingStatus.Codes.COM)
					{
						HandleDownloadCertificate(ediIdentityCertificate);
					}
					else
					{
						Globals.Message.ShowError(Res.GetString("9438C5BD-DC99-4218-A4A1-F4D69611D059", "Certificates are only available to download when the certificate status is set to 'COM' (Complete). Please try again by selecting certificates that have status set as 'COM' only."));
					}
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("C3993CFD-C16D-4BA2-BDBE-D08E8C83254E", "Please select only one certificate to download."));
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("152AF45E-24DC-40C4-A799-1E077905524A", "Please select one certificate to download."));
			}
		}

		void HandleDownloadCertificate(EdiIdentityCertificate ediIdentityCertificate)
		{
			if (ediIdentityCertificate.ICE_CertificateData == null)
			{
				Globals.Message.ShowError(Res.GetString("4D91306A-F4DB-4EAA-9148-7E07A88DD1B6", "This certificate data is null."));
			}
			else
			{
				if (DownloadCertificate(ediIdentityCertificate.ICE_CertificateData))
				{
					Globals.Message.ShowInformation(Res.GetString("E20F04D6-C77C-4F7D-9815-FED2D9142A04", "Download certificate successfully."));
				}
			}
		}

		protected virtual bool DownloadCertificate(ZBlob certificateData)
		{
			bool downloaded = false;
			using (var fileDialog = new ZSaveFileDialog())
			{
				fileDialog.DefaultExt = "cer";
				fileDialog.Filter = "CER Files (*.cer)|*.cer";

				if (fileDialog.ShowDialog() == DialogResult.OK)
				{
					using (var writer = new BinaryWriter(fileDialog.OpenFile()))
					{
						writer.Write(certificateData);
						writer.Flush();

						downloaded = true;
					}
				}
			}
			return downloaded;
		}

		bool CanDownloadCertificate => EDISecurityCheckpoints.EdiIdentityCertificateDownloadCertificate.IsAllowed;

		#endregion

		#region Handle Reset to QUE

		void ResetStatusToQUE(object sender, EventArgs e)
		{
			ResetStatusTo(EdiIdentityCertificateProcessingStatus.Codes.QUE);
		}

		void ResetStatusTo(string newStatus)
		{
			try
			{
				var selectedJobs = SelectedBusinessObjects.Cast<EdiIdentityCertificate>().ToArray();
				if (!selectedJobs.Any())
				{
					Globals.Message.ShowError(Res.GetString("77554A3E-AF31-4C43-80B1-E35C340214C0", "Please select one or more Jobs to reset the Status."));
					return;
				}

				if (selectedJobs.Any(c => c.ICE_ProcessingStatus != EdiIdentityCertificateProcessingStatus.Codes.FAL))
				{
					Globals.Message.ShowError(Res.GetString("2054E279-1FF9-480F-A89E-F15173CE3AAA", "Please Select Certificates with FAL Status Only."));
					return;
				}

				var query = new ZQuery();
				var selectedPks = selectedJobs.Select(j => j.PK);
				query.AddToFilter(EdiIdentityCertificateSchema.PK, SQLComparisonOperator.Equal, selectedPks);
				var jobs = Factory.Load<EdiIdentityCertificate>(query);

				jobs.ForEach(j =>
				{
					j.ICE_ProcessingStatus = newStatus;
				});

				Factory.Save();

				Globals.Message.ShowInformation(Res.GetString("9435C057-827C-4074-AAFD-96074E552E46", "Reset Status to {0} for {1} Job(s).", newStatus, selectedJobs.Length));
			}
			catch (ZSaveException e)
			{
				ErrorReporter.ReportOnce("Exception-Resetting-Job-Status", "Exception Resetting Job Status", e);
				Globals.Message.ShowError(Res.GetString("AA6DA31F-197C-41C9-88AA-49F39B2C3FC5", "Failed to reset the statuses. Error: {0}", e.Message));
			}
		}

		#endregion

		public override bool AllowNew => false;
		public override bool AllowEdit => false;
		public override bool AllowDelete => false;
		public override bool AllowView => false;
		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;
	}
}
