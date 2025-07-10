using System;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Interop.OutlookIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.Licencing
{
	public abstract class BaseExportAndEmailAction
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope",
			Justification = "http://stackoverflow.com/questions/41980285/exception-filter-causes-ca2000-despite-a-using-statement")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:DoNotDisposeObjectsMultipleTimes",
			Justification = "http://stackoverflow.com/questions/35203767/exception-filter-triggers-ca2202")]

		protected virtual void SendEmailCore(string fileName, EDIOrgHeader organisation, string subject)
		{
			var emailObject = new EmailToContactBusinessObject(organisation);
			using (emailObject.GetValidationSuspender())
			{
				emailObject.AddAttachment(fileName);
				emailObject.Subject = subject;
				emailObject.FromDisplayName = GlbStaff.CurrentUser.GS_FullName;
				emailObject.FromEmailAddress = GlbStaff.CurrentUser.GS_EmailAddress;
			}

			ShowForm(new EmailContactForm(emailObject));
		}

		protected virtual void ShowForm(ZForm form)
		{
			form.Show();
		}

		protected string SendEmail(EDIOrgHeader organisation, string subject, string fileName)
		{
			try
			{
				SendEmailCore(fileName, organisation, subject);
			}
			catch (UnauthorizedAccessException ex)
			{
				return Res.GetString("7ce52ba2-a1e1-4c7d-97f6-74d82658a6af",
					"An error occurred while attempting to save the file: \r\n\r\n{0}", ex.Message);
			}
			catch (OutlookException ex)
			{
				return Res.GetString("dea65a3b-832f-49a4-8854-aab65aca913f",
					"There was an unexpected error trying to show the Send Email dialog: \r\n\r\n{0}", ex.Message);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowDeveloperException(ex);
				return Res.GetString("552bdb8a-aaab-4387-8df3-7859db52f94f",
					"There was an unexpected error when attempting to save the file: \r\n\r\n{0}", ex.Message);
			}

			return "";
		}

		protected string GetDirectory(string defaultPath = "")
		{
			string directory = "";
			using (var browser = new ZFolderBrowserDialog())
			{
				browser.ShowNewFolderButton = true;
				browser.Description = Res.GetString("85f17693-7fab-49ad-904a-f99772ee7993",
					"Please select the folder into which the file should be saved:");

				if (!string.IsNullOrEmpty(defaultPath))
				{
					browser.SelectedPath = defaultPath;
				}

				if (ShowDialog(browser) == DialogResult.OK)
				{
					directory = GetPath(browser);
				}
			}
			return directory;
		}

		protected virtual DialogResult ShowDialog(ZFolderBrowserDialog browser)
		{
			browser.RequireMappablePath = true;
			return browser.ShowDialog();
		}

		protected virtual string GetPath(ZFolderBrowserDialog browser)
		{
			return browser.MappedSelectedPath;
		}
	}
}
