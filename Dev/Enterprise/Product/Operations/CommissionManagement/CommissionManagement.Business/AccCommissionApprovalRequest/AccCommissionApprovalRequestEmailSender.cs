using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business
{
	public class AccCommissionApprovalRequestEmailSender
	{
		public AccCommissionApprovalRequestEmailSender(AccCommissionApprovalRequest request)
		{
			this.request = request;
		}

		readonly AccCommissionApprovalRequest request;

		public void CreateCommissionApprovalRequestEmail()
		{
			var documentRecipientsEmails = GetCommissionApprovalRequestRecipientEmails();
			if (!documentRecipientsEmails.Any())
			{
				return;
			}

			var email = new EmailDef();
			email.FromAddress = string.IsNullOrEmpty(GlbStaff.CurrentUser.GS_EmailAddress) ? (ZString)Env.Instance.Registry.SMTPDefaultDoNotReplyEmailAddress : GlbStaff.CurrentUser.GS_EmailAddress;
			email.FromDisplayName = GlbStaff.CurrentUser.GS_FullName;
			email.Subject = CommissionApprovalRequestReportName;
			email.Body = Res.GetString("1279ce43-9362-42ad-ac8b-634132417582", @"Please view <a href=""{0}"">{1}</a> for approval.",
				ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.CommissionApprovalRequest, request.PK.ToGuid()),
				request.HumanReadableName);

			email.ContentType = EmailContentTypes.HTML;

			if (request.IncludeSummaryAsEmailAttachment)
			{
				if (CommissionApprovalRequestTemplate == null)
				{
					ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "Could not load Template SO_Name:[{0}]", CommissionApprovalRequestTemplateName));
				}
				else
				{
					var rawDocumentExcel = GetRawCommissionApprovalRequestDocumentInExcel();
					email.Attachments.Add(new AttachmentDef(CommissionApprovalRequestReportName + ".xls", rawDocumentExcel));
				}
			}

			email.AddRecipientForUserCommunication(documentRecipientsEmails.ToArray());

			Env.OutgoingMailManager.Create(request.Factory, email);
		}

		IEnumerable<string> GetCommissionApprovalRequestRecipientEmails()
		{
			var emails = new HashSet<string>();
			if (request.ApprovingStaff1 != null && !request.ApprovingStaff1.GS_EmailAddress.IsEmpty)
			{
				emails.Add(request.ApprovingStaff1.GS_EmailAddress);
			}

			if (request.ApprovingStaff2 != null && !request.ApprovingStaff2.GS_EmailAddress.IsEmpty)
			{
				emails.Add(request.ApprovingStaff2.GS_EmailAddress);
			}

			return emails;
		}

		byte[] GetRawCommissionApprovalRequestDocumentInExcel()
		{
			using (DocumentPack documentPack = new DocumentPack())
			using (Report report = GetNewReport(documentPack, CommissionApprovalRequestTemplate, CommissionApprovalRequestReportName))
			using (MemoryStream outputStream = new MemoryStream())
			{
				report.Save(outputStream);
				return outputStream.ToArray();
			}
		}

		Report GetNewReport(DocumentPack documentPack, StmTemplate template, string reportName)
		{
			var documentWrappers = request.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CommissionApprovalReq, null);
			var docWrapper = documentWrappers.Single();
			var excelTemplate = new ExcelTemplateReadFromStmTemplateTable(template);
			return new Report(documentPack, excelTemplate, docWrapper, reportName, null, DocumentDirection.ANY, false);
		}

		ZString CommissionApprovalRequestReportName
		{
			get { return Res.GetString("8ae612d6-3cb0-4e93-850d-9b49b249063b", "Commission Approval Request {0}", request.CRQ_BatchNumber); }
		}

		public StmTemplate CommissionApprovalRequestTemplate
		{
			get
			{
				var query = new ZQuery(StmTemplateSchema.SO_Name, CommissionApprovalRequestTemplateName);
				query.AddToFilter(StmTemplateSchema.SO_DataContext, Constants.DataContext.CommissionApprovalReq);
				return request.Factory.LoadTop1<StmTemplate>(query);
			}
		}

		public static string CommissionApprovalRequestTemplateName
		{
			get { return OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.Value ? LocalCommissionApprovalRequestTemplateName : GlobalCommissionApprovalRequestTemplateName; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		const string LocalCommissionApprovalRequestTemplateName = "Commission Approval Request";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		const string GlobalCommissionApprovalRequestTemplateName = "Commission Approval Request (Global)";
	}
}
