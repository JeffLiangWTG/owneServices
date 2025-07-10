using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Enterprise.RemoteDesktopServices;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Core.Forms
{
	[TestedType(typeof(EnterpriseInformationForm))]
	sealed class EnterpriseInformationFormTest : ZFormBasherTest
	{
		public void TestInformationDetails()
		{
			AssertInformationDetails(false);
		}

		public void TestInformationDetailsWithComplianceVersionNumber()
		{
			using (RawDataRegistry.Instance.ComplianceVersionNumber.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "18.20.32"))
			{
				AssertInformationDetails(true);
			}
		}

		void AssertInformationDetails(bool expectComplianceVersion)
		{
			var env = EnvProxy.Instance;
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.SystemExpiryDateForTest = env.Time.CurrentLocalDate.AddDays(env.Licence.DefaultLicenceGracePeriodInDays);
			registrationKey.DatabaseTypeForTest = DatabaseTypes.Codes.Test;
			registrationKey.DbSecurityModeForTest = DatabaseSecurityModePairList.Codes.Locked;
			var retriever = new EnterpriseInformationRetriever();

			using (var form = new EnterpriseInformationForm())
			{
				((Form)form).Show();

#if !WINZOR
				var terminalServerMode = ObjectFactory.Get<TerminalService>().IsWTSSession ? "Yes, Without RD Services" : "No";
#else
				var terminalServerMode = "No";
#endif

				AssertEquals("Release:", ReleaseInfo.Instance.ReleaseDisplayText, form.ReleaseLabel.Text);
				AssertEquals("DB Version contains Schema version", true, form.DBVersionNumber.Text.Contains("Schema = "));
				AssertEquals("DB Version contains Script version", true, form.DBVersionNumber.Text.Contains("Script = "));
				AssertEquals("DB Version contains Data version", true, form.DBVersionNumber.Text.Contains("Data = "));
				AssertEquals("DB Version contains Transformation version", true, form.DBVersionNumber.Text.Contains("Transformation = "));
				AssertEquals("DB Version contains CLR Assemblies version", true, form.DBVersionNumber.Text.Contains("CLR Assemblies = "));
				AssertEquals("Exe Version:", ReleaseInfo.Instance.VersionNumber.ToString(), form.VersionNumberLabel.Text);
				AssertEquals("Exe Version Time:", ReleaseInfo.Instance.ExeDate.ToString("dd-MMM-yyyy h:mm:ss tt"), form.VersionDateLabel.Text);
				AssertEquals("DB Server:", Db.ServerName, form.DBServerLabel.Text);
				AssertEquals("DB Name:", Db.DatabaseName, form.DBNameLabel.Text);
				AssertEquals("Country:", env.CurrentCompany.Country.Description, form.CountryLabel.Text);
				AssertEquals("System Licence Type:", DatabaseTypes.Descriptions.Test, form.SystemTypeDataLabel.Text);
				AssertEquals("Company:", env.CurrentCompany.Name, form.CompanyNameLabel.Text);
				AssertEquals("Terminal Server Mode:", terminalServerMode, form.TerminalServerModeLabel.Text);
				Assert("Framework Version:", form.FrameworkVersionLabel.Text.Length > 0);
				AssertEquals("Copyright Label:", "Copyright © 2001-" + ReleaseInfo.Instance.ExeDate.Year.ToString() + " WiseTech Global", form.CopyrightLabel.Text);

				AssertEquals("Client Doc Version:", env.Registry.ClientDocumentName + " - Version " + env.Registry.ClientDocumentVersion, form.ClientDocVersionLabel.Text);
				Assert("Sys Doc Version:", form.SysDocVersionLabel.Text.Length > 0);
				Assert("SQL Server Version", form.SQLServerVersionLabel.Text.IndexOf(Db.Connection.ServerVersionNumber.ToString()) > -1);
				AssertEquals("Database Security Mode:", DatabaseSecurityModePairList.Descriptions.Locked, form.DbSecurityDataLabel.Text);
				AssertEquals("Application Id:", SystemDataRegistry.Instance.AzureApplicationClientId, form.AzureApplicationClientIdLabel.Text);
				AssertEquals(retriever.LicenceCode, form.LicenceCodeLabel.Text);

				if (expectComplianceVersion)
				{
					AssertEquals("Compliance Version:", form.FindSingle<ZLabel>("ComplianceVersionLabel").Text);
					AssertEquals("18.20.32", form.FindSingle<ZLabel>("ComplianceVersionValue").Text);
				}
				else
				{
					AssertNull(form.FindSingleOrDefault<ZLabel>("ComplianceVersionLabel"));
					AssertNull(form.FindSingleOrDefault<ZLabel>("ComplianceVersionValue"));
				}
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new EnterpriseInformationForm();
		}

		protected override bool ShouldTestFormIsFullyTranslatable { get; }
	}
}
