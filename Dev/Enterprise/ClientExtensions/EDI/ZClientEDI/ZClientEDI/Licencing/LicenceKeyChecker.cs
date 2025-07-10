using System.Windows.Forms;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Customs.Business;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Environment;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.LicenceKeyBuilder
{
	public class LicenceKeyChecker
	{
		EDIOrgHeader Organisation { get; }
		LicenceDatabase LicenceDatabase { get; }
		LicenceHeader LicHeader { get; }
		bool IsBatch { get; }
		public string ErrorMessage { get; private set; }
		public bool IsCancel { get; set; }

		public LicenceKeyChecker(EDIOrgHeader orgHeader, LicenceDatabase licDatabase, bool isBatchProcess)
		{
			Organisation = orgHeader;
			LicenceDatabase = licDatabase;
			LicHeader = licDatabase != null ? orgHeader?.LicCompany?.GetHeader(licDatabase) : null;
			IsBatch = isBatchProcess;
		}

		public bool CheckCanGenerateLicenceKey
		{
			get
			{
				return CheckDatabaseAssignedToCompany() &&
					CheckOrgNameLength() &&
					CheckOrganisationSaved() &&
					CheckOrgIsInAllowedCountry() &&
					CheckIfNewLineLevelLicenceTypesSupported();
			}
		}

		bool CheckOrgNameLength()
		{
			bool result = true;
			if (Organisation.OH_FullName.Length > 50)
			{
				ErrorMessage = "Organization name exceeds 50 characters: " + Organisation.OH_FullName;
				result = false;
			}
			return result;
		}

		public bool CheckDatabaseAssignedToCompany()
		{
			bool result = true;
			if (Organisation.LicCompany.LicDatabases == null || Organisation.LicCompany.LicDatabases.Count == 0)
			{
				ErrorMessage = Res.GetString("6e6d6e59-db1f-4d56-9ccd-c0e7754b24b2", "You cannot generate the License Key until you have assigned a Database to the Company");
				result = false;
			}
			return result;
		}

		bool CheckOrganisationSaved()
		{
			bool result = true;
			if (Organisation.HasChanges)
			{
				DialogResult confirmationResult;
				if (IsBatch)
				{
					confirmationResult = DialogResult.OK;
				}
				else
				{
					confirmationResult = Globals.Message.Show(Res.GetString("9510f108-fe89-477a-a01d-d48e14ed9ba4", "You cannot generate the License Key until the Organization has been saved.\r\n\r\nDo you wish to save changes before generating?"), Res.GetString("1cb6b103-e0f9-4b4f-8a93-871b306343d8", "Save Before Generating Key?"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
				}

				IsCancel = false;
				if (confirmationResult == DialogResult.OK)
				{
					Organisation.RunPreSaveValidation();
					if (Organisation.HasErrors)
					{
						result = false;
					}
					else
					{
						Organisation.Factory.Save();
					}
				}
				else // Cancelled Generating
				{
					IsCancel = true;
					result = false;
				}
			}

			return result;
		}

		bool CheckOrgIsInAllowedCountry()
		{
			bool result = true;
			if (!Organisation.OrgIsInAllowedCountry())
			{
				ErrorMessage = Res.GetString("a422115c-da0a-454f-855a-396f9c28bae1", "The selected Organization is not located in a country/region supported by {0}.\r\n\r\nA License Key cannot be generated for {1}.", Core.Constants.ProductName, Organisation.OH_FullName);
				result = false;
			}
			return result;
		}

		bool CheckIfNewLineLevelLicenceTypesSupported()
		{
			bool result = true;

			LicenceDatabase database = LicenceDatabase;
			LicenceHeader header = LicHeader;
			var supportsLineLevelOnDemandLicenceTypes = header.SupportsLineLevelOnDemandLicenceTypes;
			if (header.Modules.HasLineLevelOnDemandLicenceType && supportsLineLevelOnDemandLicenceTypes != TriState.True)
			{
				string message = null;
				if (supportsLineLevelOnDemandLicenceTypes == TriState.NotDetermined)
				{
					message = Res.GetString("b1c303c0-0ec3-45c9-b9b9-4013dcd2dac5", "Cannot determine if the system for the selected organization {0} on server {1} would support the new line level On Demand license types.\r\nPlease check the Current Running Version.", header.OrganisationCode, database.LD_ServerCode) + "\r\n";
				}
				else
				{
					ReleaseBuild currentVersion = database.CurrentVersion;
					message = Res.GetString("af24334c-ed4c-481f-92b4-6003c9c12ed2", "Cannot generate the new license key for the selected organization {0} on server {1}.\r\nThe system will need to be upgraded first to {2} version later than {3} to be able to support the new line level On Demand license types.",
								header.OrganisationCode, database.LD_ServerCode, "a", LicenceHeader.LineLevelOnDemandLicenceVersion.AddRelease(-1)) + "\r\n";
				}

				if (!string.IsNullOrEmpty(message))
				{
					message += Res.GetString("15db1bf7-ee99-4430-b144-2773ab35da34", "If you insist on sending anyway, the new license key would most likely fail to load. Try using '{0}' instead, which actually means 'Always Allow' in the older systems", LicenceTypes.Codes.NON);
					IsCancel = false;
					if (IsBatch)
					{
						result = true;
					}
					else
					{
						result = Globals.Message.ShowConfirmation(message, Res.GetString("f86c7368-924a-4107-996e-246442c20ef1", "Possible Unsupported On Demand License Types"), Res.GetString("ef0b713e-e0ab-4980-ab26-79f5de2ac071", "To confirm, please type") + " ", Res.GetString("bb0f5e96-c33f-4c67-aa77-f82898a3e664", "Yes"), MessageBoxIcon.Exclamation) == DialogResult.OK;
						IsCancel = !result;
					}
				}
			}
			else if (supportsLineLevelOnDemandLicenceTypes == TriState.True
				&& header.IsOnDemandModuleTypeAllowed
				&& header.GetCoreModule().LM_LicenceType == LicenceTypes.Codes.NON)
			{
				string message = "This On Demand licence for organisation " + header.OrganisationCode + " on server " + database.LD_ServerCode + " does not have Core enabled.\r\nDo not send it unless you really want to lock out all users.\r\nIt may be an old style On Demand licence.\r\nTo convert it to the new style you first need to enable ALL modules at the line level.";
				IsCancel = false;
				if (IsBatch)
				{
					result = true;
				}
				else
				{
					result = Globals.Message.ShowConfirmation(message, "Disabled On Demand License", "To lock out all users, please type  ", "LOCKOUTALLUSERS", MessageBoxIcon.Exclamation) == DialogResult.OK;
					IsCancel = !result;
				}
			}
			return result;
		}

		static public bool ConfirmLicenceGeneration(EDIOrgHeader organisation, LicenceDatabase licenceDatabase)
		{
			LicenceCompany licCompany = organisation.LicCompany;
			string message = Res.GetString("c07e5c3b-2a76-4725-b387-c103e292fe7f", "Please confirm you wish to generate the license key with the following information:\r\n     Organization: {0}\r\n     Server: {1}\r\n     Exchange rates", organisation.OH_Code, licenceDatabase.LD_ServerCode) + " " + (licCompany.LC_IsReciprocal ? Res.GetString("e47c03a4-021b-40a1-886b-12caee4b384b", "are reciprocal") : Res.GetString("a76b8bc0-37a2-47c1-a713-e7299c360534", "are not reciprocal")) + "\r\n     " + Res.GetString("61362ff7-30b9-4d17-9af2-70e06f686f64", "GST Registered:") + " " + (licCompany.LC_IsGSTRegistered ? Res.GetString("bb0f5e96-c33f-4c67-aa77-f82898a3e664", "Yes") : Res.GetString("d5ef4d42-a516-422a-ae36-bd1d5f9047b1", "No")) + "\r\n     " + Res.GetString("a02ca7b6-7e5e-4148-85e8-dc7cc9399a4c", "GST Cash Basis:") + " " + (licCompany.LC_IsGSTCashBasis ? Res.GetString("bb0f5e96-c33f-4c67-aa77-f82898a3e664", "Yes") : Res.GetString("d5ef4d42-a516-422a-ae36-bd1d5f9047b1", "No")) + "\r\n     " + Res.GetString("e52fcd00-dce4-40ec-8fbf-fe3510229171", "WHT Registered:") + " " + (licCompany.LC_IsWHTRegistered ? Res.GetString("bb0f5e96-c33f-4c67-aa77-f82898a3e664", "Yes") : Res.GetString("d5ef4d42-a516-422a-ae36-bd1d5f9047b1", "No")) + "\r\n     " + Res.GetString("7eb5d6a3-015d-4ee5-8ebc-42d2978afd84", "WHT Cash Basis:") + " " + (licCompany.LC_IsWHTCashBasis ? Res.GetString("2533f31d-4b3c-4645-a381-2219610247c7", "Yes") : Res.GetString("d5ef4d42-a516-422a-ae36-bd1d5f9047b1", "No")) + "\r\n     " + Res.GetString("9c016e2a-77c4-42d1-88f6-9d6df43cb11b", "Currency: {0}\r\n     Taxation Reg No ({1}): {2}\r\n     Business Reg No ({3}): {4}\r\n\r\nNB: This license will be generated for the database", licCompany.LC_RX_NKCurrency, organisation.LicenceTaxationRegNoType, organisation.LicenceTaxationRegNo, organisation.LicenceBusinessRegNoType, organisation.LicenceBusinessRegNo) + " " + licenceDatabase.LD_ServerCode + ".";
			return (Globals.Message.Show(message, Res.GetString("3e2e2b6d-4226-434e-a6e4-4ca5c93ce689", "Are you sure you want to generate this license?"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes);
		}

		static public bool ConfirmBatchLicenceGeneration(int totalLicenceKeys)
		{
			string message = Res.GetString("936fef0d-1f1c-43ef-bb95-820e553d62bb", "Please confirm you wish to generate {0} licenses key(s).\r\nThis will create an Outlook Email for each license, potentially creating many Email windows.", totalLicenceKeys);
			return (Globals.Message.Show(message, Res.GetString("ddbfbf27-ca2e-4d12-8f9c-139aec61bd55", "Save & Email Licenses"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes);
		}

		public bool CheckCanAutoDeployToClient(LicenceHeader licenceToDeployTo)
		{
			bool result = true;
			var databaseToDeployTo = licenceToDeployTo.Database;
			if (!databaseToDeployTo.PublicEmailIsDeployable || !databaseToDeployTo.VersionIsDeployable) // This is when the new system was checked-in
			{
				ErrorMessage = Res.GetString("1c041914-a3c3-4380-b4eb-e42ebf90960b", "The license key {0} could not be automatically deployed.\r\n\r\nPlease check the following details:\r\n The Public Email Address must be filled, in order to send an email to the client's batch processor\r\n The client's Current Version must be on or after {1} for this feature to work.\r\n\r\nPlease check that these details are correct before trying again, or deploy the license key manually.", licenceToDeployTo.OrganisationCode, LicenceDatabase.DateFromWhichLicenceIsAutoDeployable.ToShortDateString());
				result = false;
			}
			return result;
		}
	}
}
