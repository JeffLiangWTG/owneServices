using System;
using System.IO;
using CargoWise.Common;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using MessageDisplayConstants = Enterprise.Customs.JP.AFR.Business.MessageDisplayConstants;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal
{
	public class HtmlResponseEmailGenerator
	{
		public HtmlResponseEmailGenerator()
		{
			ResponseDescription = DefaultResponseDescription;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings")]
		public EmailDef GenerateEmail(string uri, string jobNumber, string messageTypeInSubject, string bodyDetailHtml, bool isFailure, IGlbBranch branch)
		{
			return GenerateEmail(uri, jobNumber, messageTypeInSubject, bodyDetailHtml, "", isFailure, branch);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings")]
		public EmailDef GenerateEmail(string uri, string jobNumber, string messageTypeInSubject, string bodyDetailHtml, string footerHtml, bool isFailure, IGlbBranch branch)
		{
			var failureText = isFailure ? "(Failure) " : "";
			var hrefJobNumber = jobNumber;
			if (!string.IsNullOrEmpty(uri))
			{
				hrefJobNumber = FormattableString.Invariant($"<a href=\"{uri}\">{jobNumber}</a>");
			}

			string subject;
			if (string.IsNullOrEmpty(jobNumber))
			{
				subject = FormattableString.Invariant($"{messageTypeInSubject} Response {failureText}").Trim();
			}
			else
			{
				subject = FormattableString.Invariant($"{messageTypeInSubject} Response {failureText}for {jobNumber}");
			}

			string responseHeader;
			if (string.IsNullOrEmpty(hrefJobNumber))
			{
				responseHeader = FormattableString.Invariant($"{messageTypeInSubject} Response {failureText}").Trim();
			}
			else
			{
				responseHeader = FormattableString.Invariant($"{messageTypeInSubject} Response {failureText}for {hrefJobNumber}");
			}

			return GenerateEmail(subject, responseHeader, ResponseDescription, bodyDetailHtml, footerHtml, branch);
		}

		public EmailDef GenerateEmail(string subject, string header, string description, string bodyDetails, string footerDetails, IGlbBranch branch)
		{
			EmailDef email = null;
			try
			{
				string emailTemplateHtml;
				using (var stream = GetType().Assembly.GetManifestResourceStream("Enterprise.Customs.JP.AFR.DataTransfer.Universal.EventProcessor.Template.Response.htm"))
				{
					emailTemplateHtml = new StreamReader(stream).ReadToEnd();
				}

				emailTemplateHtml = emailTemplateHtml.Replace("<!--Response Header-->", header);
				emailTemplateHtml = emailTemplateHtml.Replace("<!--Response Description Section-->", description);
				emailTemplateHtml = emailTemplateHtml.Replace("<!--Response Body Details Section-->", bodyDetails);
				emailTemplateHtml = emailTemplateHtml.Replace("<!--Response Footer Details Section-->", footerDetails);

				var emailSender = new HtmlNotificationEmailSender();

				email = emailSender.CreateEmail(subject, emailTemplateHtml, branch?.Company?.PK.ToGuid(), branch?.PK.ToGuid(), Guid.Empty);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ErrorReporter.ReportOnce(subject, "Error creating email", e);
			}
			return email;
		}

		public string ResponseDescription;
		public const string DefaultResponseDescription = MessageDisplayConstants.AResponseMessage + MessageDisplayConstants.ShownBelow;
	}
}
