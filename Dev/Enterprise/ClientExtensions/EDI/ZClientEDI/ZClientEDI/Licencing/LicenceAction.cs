using System;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Client.EDI.LicenceKeyBuilder;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Licencing.GUI;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.MasterFiles.Progress;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.Licencing.Module
{
	public class LicenceAction : BaseExportAndEmailAction
	{
		#region Properties

		ZFilterGridModule FilterGridModule
		{
			get { return filterGridModule; }
		}
		readonly ZFilterGridModule filterGridModule;

		Form MainForm
		{
			get { return mainForm; }
		}
		readonly Form mainForm;

		LicenceDatabase LicDatabase
		{
			get { return licDatabase; }
		}
		readonly LicenceDatabase licDatabase;

		EDIOrgHeader Organisation
		{
			get { return organisation; }
		}
		readonly EDIOrgHeader organisation;

		bool IsBatchProcess
		{
			get
			{
				return isBatchProcess;
			}
		}
		readonly bool isBatchProcess;

		#endregion

		#region Constructors

		public LicenceAction(Form mForm, ZFilterGridModule filterGridMod, LicenceDatabase licDb, EDIOrgHeader org, bool isBatch)
		{
			mainForm = mForm;
			filterGridModule = filterGridMod;
			licDatabase = licDb;
			organisation = org;
			isBatchProcess = isBatch;
		}

		#endregion

		#region Methods

		#region Update Licence Remotely

		public void UpdateLicenceRemotely(object sender, EventArgs e)
		{
			try
			{
				// Processing
				string error = "";
				int successCount = 0;
				if (IsBatchProcess)
				{
					using (ProgressForm progressForm = new ProgressForm())
					{
						ProgressFormAdapter progress = new ProgressFormAdapter(progressForm);
						progressForm.ShowCancelButton = true;
						ZFormModaliser.Show(progressForm, MainForm);
						int progressCount = 0;
						int totalLicence = FilterGridModule.GetSelectedBusinessObjects().Length;
						foreach (LicenceHeader licence in FilterGridModule.GetSelectedBusinessObjects())
						{
							if (progress.SafeIsCancelled())
							{
								break;
							}
							progressForm.SetStatusAndPercentComplete("Update licence " + licence.LicenceCode + "(Org " + licence.Company.Header.OH_Code + ") remotely", progressCount++ * 100 / totalLicence);
							string currError = "";
							LicenceKeyChecker licKeyChecker = new LicenceKeyChecker(licence.Company.Header, licence.Database, false);
							if (licence != null && licence.Database != null && licKeyChecker.CheckCanGenerateLicenceKey && licKeyChecker.CheckCanAutoDeployToClient(licence))
							{
								currError = UpdateOneLicenceRemotely(licence.Company.Header, licence);
							}
							else
							{
								currError = licKeyChecker.ErrorMessage;
							}

							if (string.IsNullOrEmpty(currError))
							{
								successCount++;
							}
							else
							{
								error += string.IsNullOrEmpty(error) ? currError : "\r\n\r\n" + currError;
							}
						}
					}
				}
				else
				{
					string currError = "";
					LicenceKeyChecker licKeyChecker = new LicenceKeyChecker(Organisation, LicDatabase, isBatchProcess);
					var licHeader = LicDatabase != null ? Organisation.LicCompany.GetHeader(LicDatabase) : null;
					if (licHeader != null && licKeyChecker.CheckCanGenerateLicenceKey && licKeyChecker.CheckCanAutoDeployToClient(licHeader))
					{
						if (!LicenceKeyChecker.ConfirmLicenceGeneration(Organisation, LicDatabase))
						{
							licKeyChecker.IsCancel = true;
						}
						else
						{
							currError = UpdateOneLicenceRemotely(Organisation, licHeader);
						}
					}
					else
					{
						currError = licKeyChecker.ErrorMessage;
					}

					if (!licKeyChecker.IsCancel)
					{
						if (string.IsNullOrEmpty(currError))
						{
							successCount++;
						}
						else
						{
							error += string.IsNullOrEmpty(error) ? currError : "\r\n\r\n" + currError;
						}
					}
				}

				// Show Message
				if (!string.IsNullOrEmpty(error))
				{
					Globals.Message.ShowError(error);
				}

				if (successCount > 0)
				{
					Globals.Message.Show(Res.GetString("21639ed1-f64c-459d-aa66-545c49bc635f", "{0} license key(s) successfully sent. Changes to the license won't be enforced at the client until operators log out and log back into Enterprise.", successCount), Res.GetString("4d633ad3-6f77-4581-8902-aa5dfd5ea0ab", "License Key Deployed"), MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError("Error updating licence remotely:\r\n" + ex.Message);
			}
		}

		string UpdateOneLicenceRemotely(EDIOrgHeader org, LicenceHeader licence)
		{
			try
			{
				GenerateAndAutoDeployLicenceKey(org, licence);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return Res.GetString("9f5138c0-1632-4cf5-b99c-8c61046682a1", @"The license key {0} failed to send. This could be because the Mail Batch Processor is not running/mis-configured, or the connection to The Internet is unavailable.", org.OH_Code);
			}
			return "";
		}

		protected virtual bool GenerateAndAutoDeployLicenceKey(EDIOrgHeader org, LicenceHeader licence)
		{
			return org.GenerateAndAutoDeployLicenceKey(licence);
		}

		#endregion

		#region Send Email Licence

		public void SendEmailLicence(object sender, EventArgs e)
		{
			int successCount = 0;
			string directory = "";
			string fileName = "";
			string error = "";

			try
			{
				// Processing
				if (IsBatchProcess)
				{
					int totalLicenceKeys = FilterGridModule.GetSelectedBusinessObjects().Length;
					if ((totalLicenceKeys > 0) && LicenceKeyChecker.ConfirmBatchLicenceGeneration(totalLicenceKeys))
					{
						using (ProgressForm progressForm = new ProgressForm())
						{
							directory = GetDirectoryAndSave();
							if (!string.IsNullOrEmpty(directory))
							{
								ProgressFormAdapter progress = new ProgressFormAdapter(progressForm);
								progressForm.ShowCancelButton = true;
								ZFormModaliser.Show(progressForm, MainForm);
								int progressCount = 0;
								int totalLicence = FilterGridModule.GetSelectedBusinessObjects().Length;
								foreach (LicenceHeader licence in FilterGridModule.GetSelectedBusinessObjects())
								{
									if (progress.IsCancelled)
									{
										break;
									}
									progressForm.SetStatusAndPercentComplete("Update licence " + licence.LicenceCode + "(Org " + licence.Company.Header.OH_Code + ") remotely", progressCount++ * 100 / totalLicence);
									LicenceDatabase currentLicDatabase = licence.Database;
									EDIOrgHeader currentOrganisation = licence.Company.Header;
									LicenceKeyChecker licKeyChecker = new LicenceKeyChecker(currentOrganisation, currentLicDatabase, isBatchProcess);
									string currError = "";
									if (licKeyChecker.CheckCanGenerateLicenceKey)
									{
										currError = SendOneEmailLicence(currentOrganisation, currentLicDatabase, licence, directory, ref fileName);
									}
									else
									{
										currError += licKeyChecker.ErrorMessage;
									}

									if (string.IsNullOrEmpty(currError))
									{
										successCount++;
									}
									else
									{
										error += string.IsNullOrEmpty(error) ? currError : "\r\n\r\n" + currError;
									}
								}
							}
						}
					}
				}
				else
				{
					string currError = "";
					LicenceKeyChecker licKeyChecker = new LicenceKeyChecker(organisation, licDatabase, isBatchProcess);
					if (licKeyChecker.CheckCanGenerateLicenceKey)
					{
						if (!LicenceKeyChecker.ConfirmLicenceGeneration(Organisation, LicDatabase))
						{
							licKeyChecker.IsCancel = true;
						}
						else
						{
							directory = GetDirectoryAndSave();
							if (!string.IsNullOrEmpty(directory))
							{
								currError = SendOneEmailLicence(Organisation, LicDatabase, null, directory, ref fileName);
							}
							else
							{
								licKeyChecker.IsCancel = true;
							}
						}
					}
					else
					{
						currError += licKeyChecker.ErrorMessage;
					}

					if (!licKeyChecker.IsCancel)
					{
						if (string.IsNullOrEmpty(currError))
						{
							successCount++;
						}
						else
						{
							error += string.IsNullOrEmpty(error) ? currError : "\r\n\r\n" + currError;
						}
					}
				}

				// Show Messages
				if ((!string.IsNullOrEmpty(error)) && (error != "-1"))
				{
					Globals.Message.ShowError(error, Res.GetString("dd1711da-3fa5-48ea-8a13-f03a11b8e04d", "Error Saving File"));
				}
				if (successCount > 0)
				{
					if (IsBatchProcess)
					{
						Globals.Message.ShowInformation(Res.GetString("74f09933-0c19-4c3e-98b1-bf94d503b400", "The License Keys were successfully generated.\r\nThese files can be found at {0}", directory), Res.GetString("57f3e2ed-7dae-4156-82e6-43de639e0cb3", "Key Generation Successful"));
					}
					else
					{
						Globals.Message.ShowInformation(Res.GetString("caddeac9-55cb-4fa9-a297-d34e9e46a2e2", "The License Key for {0} was successfully generated.\r\nThis file can be found at {1}", Organisation.OH_Code, fileName), Res.GetString("57f3e2ed-7dae-4156-82e6-43de639e0cb3", "Key Generation Successful"));
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError("Error updating licence remotely:\r\n" + ex.Message);
			}
		}

		string GetDirectoryAndSave()
		{
			var directory = GetDirectory(EDIDataRegistry.Instance.LicenceKeyDirectoryPath);
			if (!string.IsNullOrEmpty(directory))
			{
				EDIDataRegistry.Instance.LicenceKeyDirectoryPath = directory;
			}

			return directory;
		}

		string SendOneEmailLicence(EDIOrgHeader org, LicenceDatabase licDatabase, LicenceHeader licence, string directory, ref string fileName)
		{
			if (null == licence)
			{
				licence = org.LicCompany.GetHeader(licDatabase);
			}

			try
			{
				fileName = org.GenerateLicenceKeyToFileSystem(directory, licence);
				string subject = Res.GetString("a0e389cc-94b0-4dea-801c-b0d02fe889f9", "ediEnterprise License Key for {0}", org.OH_FullNameTruncated);
				return SendEmail(org, subject, fileName);
			}
			catch (UnauthorizedAccessException ex)
			{
				return Res.GetString("7ce52ba2-a1e1-4c7d-97f6-74d82658a6af", "An error occurred while attempting to save the file: \r\n\r\n{0}", ex.Message);
			}
		}

		#endregion

		#region SetLicenceModuleFeeBasis

		public void SetLicenceModuleFeeBasis(object sender, EventArgs e)
		{
			if (!EDISecurityCheckpoints.OrgLicenceModify.IsAllowed)
			{
				Globals.Message.Show(EDISecurityCheckpoints.OrgLicenceModify.ErrorMessageForNotAllowed);
				return;
			}

			try
			{
				var selection = FilterGridModule.GetSelectedBusinessObjects();
				if (selection.Length == 0)
				{
					Globals.Message.ShowWarning("No records are selected.");
					return;
				}

				LicenceModuleFeeBasis feeBasis = new LicenceModuleFeeBasis();
				if (DialogResult.OK != ZFormModaliser.ShowDialogAndDispose(new LicenceModuleFeeBasisForm(feeBasis)))
				{
					return;
				}

				int successCount;
				StringBuilder errors = new StringBuilder();
				using (ProgressForm progressForm = new ProgressForm())
				{
					ProgressFormAdapter progress = new ProgressFormAdapter(progressForm);
					progressForm.ShowCancelButton = true;
					ZFormModaliser.Show(progressForm, MainForm);
					successCount = feeBasis.Update(selection, progress, errors);
					if (successCount > 0)
					{
						progress.SetStatusAndPercentComplete("Saving...", 99);
						selection[0].Factory.Save();
					}
				}

				if (successCount > 0)
				{
					Globals.Message.Show(string.Format("{0} license key(s) updated.", successCount), "Licence Module Fee Basis", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				if (errors.Length > 0)
				{
					Globals.Message.ShowError(errors.ToString());
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError("Error:\r\n" + ex.ToString());
			}
		}

		#endregion

		#endregion

	}
}
