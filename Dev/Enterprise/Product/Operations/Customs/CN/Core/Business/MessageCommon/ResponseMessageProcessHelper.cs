using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.Common.Shared;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.Business
{
	public static class ResponseMessageProcessHelper
	{
		public static string GetHtmlInterpretation(IEnumerable<KeyValuePair<string, string>> items)
		{
			var builder = new ZStringBuilder();
			builder.Append(Constants.MessageHtmlInterpretation.HtmlInterpretationHeader);
			foreach (var item in items)
			{
				builder.Append("\t" + GetSegmentHtml(item.Key, item.Value));
			}
			builder.Append(Constants.MessageHtmlInterpretation.HtmlInterpretationTail);
			return builder.ToStringWithNewLineBetweenAppends();
		}

		static string GetSegmentHtml(string caption, string value)
		{
			return string.Format(CultureInfo.InvariantCulture, Constants.MessageHtmlInterpretation.SegmentHtmlTemplate, caption, value);
		}

		public static IEnumerable<ZString> GetEmailAddressesToNotify(EDIMessage outgoingMessage, CusEntryHeader entryHeader)
		{
			GlbStaff user = null;

			if (outgoingMessage != null)
			{
				user = entryHeader.Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, outgoingMessage.EM_SystemCreateUser);
			}

			if (user == null || user.GS_EmailAddress.IsEmpty && entryHeader.Declaration != null)
			{
				user = entryHeader.Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, entryHeader.Declaration.JE_SystemLastEditUser);
			}

			if (user != null && !user.GS_EmailAddress.IsEmpty)
			{
				yield return user.GS_EmailAddress;
			}

			var agent = entryHeader.Declaration?.CusAgent;
			if (agent != null && !agent.GS_EmailAddress.IsEmpty)
			{
				yield return agent.GS_EmailAddress;
			}
		}

		public static ZString GetEmailSubjectWithReferenceNumbers(CusEntryHeader entryHeader, ZString messageStatus, ZString messageDescription)
		{
			bool succeeded = messageStatus == MessageStatusList.Codes.AcknowledgedOriginal;
			return GetEmailSubjectWithReferenceNumbers(entryHeader, succeeded, messageDescription);
		}

		public static ZString GetEmailSubjectWithReferenceNumbers(CusEntryHeader entryHeader, bool succeeded, ZString messageDescription)
		{
			var succeededDescription = succeeded ? (NoResString)"成功" : (NoResString)"失败";
			var result = FormattableString.Invariant($"{messageDescription}{succeededDescription}: {entryHeader.LocalReferenceNumber}, {entryHeader.Declaration?.JE_DeclarationReference ?? ZString.Empty}");
			return result;
		}

		public static void CreateMail(ZString subject, ZString body, IEnumerable<ZString> recipients, JobDeclaration jobDeclaration)
		{
			Argument.NotNullOrEmpty(subject, nameof(subject));
			Argument.NotNullOrEmpty(body, nameof(body));
			Argument.NotNull(jobDeclaration, nameof(jobDeclaration));

			if (recipients.Any())
			{
				var emailBuilder = new EmailDefBuilder(subject, EmailDefBuilder.HtmlTemplates.EmptyWithDynamicHtml5);
				var jobLink = ZString.Format("<a href=\"{0}\">{1}</a>", ObjectFactory.Get<IShowEditFormUrlCreator>().Create(jobDeclaration), jobDeclaration.JE_DeclarationReference);
				emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtmlHeading, jobLink);
				emailBuilder.AddDynamicHtmlReplacement(body);

				var email = emailBuilder.ToEmail();
				foreach (var address in recipients.Distinct())
				{
					email.AddRecipientForSystemCommunication(address);
				}
				Env.OutgoingCustomsMailManager.Create(jobDeclaration.Factory, email);
			}
		}
	}
}
