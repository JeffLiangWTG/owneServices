using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Messaging.Business
{
	public class HtmlResponseEmailGenerator
	{
		public HtmlResponseEmailGenerator()
		{
			ResponseDescription = DefaultResponseDescription;
		}

		public static class HtmlConstants
		{
			public const string Br = @"<br />";
		}

		[SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "We are using URL as string on the rest of the implementation.")]
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "By design.")]
		public bool TryGenerateEmail(string uri, string jobNumber, string messageTypeInSubject, string bodyDetailHtml, bool isFailure, out EmailDef email, IGlbBranch branchForEmailLogo, bool hasWarning = false, string warningMessage = "")
		{
			return TryGenerateEmail(uri, jobNumber, messageTypeInSubject, bodyDetailHtml, "", isFailure, out email, branchForEmailLogo, hasWarning, warningMessage);
		}

		[SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "We are using URL as string on the rest of the implementation.")]
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "By design.")]
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Html data format")]
		public bool TryGenerateEmail(string uri, string jobNumber, string messageTypeInSubject, string bodyDetailHtml, string footerHtml, bool isFailure, out EmailDef email, IGlbBranch branchForEmailLogo, bool hasWarning = false, string warningMessage = "")
		{
			string statusText = "";
			if (hasWarning)
			{
				statusText = warningMessage;
			}
			else if (isFailure)
			{
				statusText = Res.GetString("{A3AA4E28-89B3-4421-BF62-1AF0FE186503}", "(Failure) ");
			}
			string hrefJobNumber = jobNumber;
			if (!string.IsNullOrEmpty(uri))
			{
				hrefJobNumber = "<a href=\"" + uri + "\">" + jobNumber + "</a>";
			}
			var subject = GetResponseInformation(jobNumber, messageTypeInSubject, statusText);
			var responseHeader = GetResponseInformation(hrefJobNumber, messageTypeInSubject, statusText);
			return TryGenerateEmail(subject, responseHeader, ResponseDescription, bodyDetailHtml, footerHtml, out email, branchForEmailLogo);
		}

		static string GetResponseInformation(string jobNumber, string messageTypeInSubject, string statusText)
		{
			return ((messageTypeInSubject + " " + Res.GetString("{5BC0F704-801E-4314-B524-198EC8CF5109}", "Response") + " " + statusText) + (string.IsNullOrEmpty(jobNumber) ? string.Empty : Res.GetString("{D8205B5F-2D4A-4751-B80D-1DED7068A211}", "for {0}", jobNumber))).Trim();
		}

		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "By design.")]
		public bool TryGenerateEmail(string subject, string header, string description, string bodyDetails, string footerDetails, out EmailDef email, IGlbBranch branchForEmailLogo = null)
		{
			email = null;
#if DEBUG
			if (SetCreatingEmailDefFailForTest)
			{
				return false;
			}
#endif
			try
			{
				string emailTemplateHtml;
				using (var stream = GetType().Assembly.GetManifestResourceStream("Enterprise.Messaging.Business.MessageProcessor.Template.Response.htm"))
				{
					emailTemplateHtml = new StreamReader(stream).ReadToEnd();
				}

				emailTemplateHtml = emailTemplateHtml.Replace("<!--Response Header-->", header);
				emailTemplateHtml = emailTemplateHtml.Replace("<!--Response Description Section-->", description);
				emailTemplateHtml = emailTemplateHtml.Replace("<!--Response Body Details Section-->", bodyDetails);
				emailTemplateHtml = emailTemplateHtml.Replace("<!--Response Footer Details Section-->", footerDetails);

				var htmlemail = new HtmlEmailDef();
				htmlemail.Subject = subject;
				if (branchForEmailLogo != null)
				{
					var companyPK = branchForEmailLogo.GB_GC.ToGuid();
					htmlemail.LoadHtmlUsingTemplate(emailTemplateHtml, null, companyPK, branchForEmailLogo.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
					if (((IUser)GlbStaff.CurrentUser).IsBatchProcessor)
					{
						htmlemail.FromDisplayName = (string)DataRegistry.Instance.RawRegistry.MailboxDisplayName.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
					}
				}
				else
				{
					htmlemail.LoadHtmlUsingTemplate(emailTemplateHtml);
				}

				email = htmlemail;

				return true;
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ExceptionReporter.Instance.HandleUnhandledException(e);
			}
			return false;
		}

		public string ResponseDescription;
		public static string DefaultResponseDescription => AResponseMessage + ShownBelow;
		public static string ShownBelow => Res.GetString("{CE2420CB-2E37-42A0-90E2-255D3C940643}", "Shown below is a summary of relevant information received in the message.{0}", HtmlConstants.Br);
		public static string AResponseMessage => Res.GetString("{B3F21E6F-EF43-4053-942A-B742D32804DB}", "A response message has been received from Customs.{0}", HtmlConstants.Br);

#if DEBUG
		public bool SetCreatingEmailDefFailForTest { get; set; }
#endif
	}
}
