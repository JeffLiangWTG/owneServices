using System;
using System.Collections.Specialized;
using System.Text;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.FaxRouter.MailSecurity;
using Enterprise.Integration.Licensing;
using Enterprise.PrintProcessing.Mailer.Email;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.PrintProcessing.Mailer.Fax
{
	public class FaxDef
	{
		public static FaxDef New()
		{
#if DEBUG
			return (FaxDef)Activator.CreateInstance(TypeToCreateForTesting);
#else
			return new FaxDef();
#endif
		}

#if DEBUG
		static Type TypeToCreateForTesting;

		internal static void SetTypeToCreateForTesting(Type type)
		{
			TypeToCreateForTesting = type;
		}

		public static void ResetTypeToCreateForTesting()
		{
			TypeToCreateForTesting = typeof(FaxDef);
		}

		static FaxDef()
		{
			ResetTypeToCreateForTesting();
		}
#endif
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Check with Zubin")]
		public const string EDI_FAX_DOC_SUBJECT = "EDI Fax Document";
		public const string EDI_FAX_DEFAULT_SENDER = "edifax@edi.com.au";

		public virtual void Send()
		{
			ICryptographicProvider cryptProvider = new CryptProvider();
			StringCollection faxGateway = new();
			_ = faxGateway.Add(Core.Constants.EmailAddresses.EDI_FAX_GATEWAY);

			var sendDateTime = ZDateTime.Now.ToDateTime();
			StringBuilder faxCommands = new();

			var faxnum = string.IsNullOrEmpty(Env.Registry.FaxDestinationOverride) ? FaxNumber.ToString() : Env.Registry.FaxDestinationOverride;

			_ = faxCommands.Append("FAXNUMBER=");
			_ = faxCommands.Append(faxnum);
			_ = faxCommands.Append(PrintProcessingConstants.NEWLINE);

			_ = faxCommands.Append("FAXATTENTION=");
			_ = faxCommands.Append(FaxAttention);
			_ = faxCommands.Append(PrintProcessingConstants.NEWLINE);

			_ = faxCommands.Append("FAXATTENTIONCOMPANY=");
			_ = faxCommands.Append(FaxAttentionCompany);
			_ = faxCommands.Append(PrintProcessingConstants.NEWLINE);

			_ = faxCommands.Append("SYSFAXJOBID=");
			_ = faxCommands.Append(SysFaxJobId);
			_ = faxCommands.Append(PrintProcessingConstants.NEWLINE);

			_ = faxCommands.Append("SYSID=");
			_ = faxCommands.Append(SysId);
			_ = faxCommands.Append(PrintProcessingConstants.NEWLINE);

			_ = faxCommands.Append("SENTDATETIME=");
			_ = faxCommands.Append(sendDateTime.ToString(cryptProvider.GetDateTimeFormat()));
			_ = faxCommands.Append(PrintProcessingConstants.NEWLINE);

			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			_ = faxCommands.Append("ENTERPRISECODE=");
			_ = faxCommands.Append(registrationKey.EnterpriseCode);
			_ = faxCommands.Append(PrintProcessingConstants.NEWLINE);

			_ = faxCommands.Append("COMPANYCODE=");
			_ = faxCommands.Append(SendingCompanyCode);
			_ = faxCommands.Append(PrintProcessingConstants.NEWLINE);

			_ = faxCommands.Append("PHYSICALSERVERID=");
			_ = faxCommands.Append(registrationKey.ServerCode);
			_ = faxCommands.Append(PrintProcessingConstants.NEWLINE);

			_ = faxCommands.Append("FAXKEY=");
			if (!FaxFileName.IsEmpty)
			{
				_ = faxCommands.Append(cryptProvider.GenerateKeyFromFile(FaxFileName.ToString(), sendDateTime));
			}
			_ = faxCommands.Append(PrintProcessingConstants.NEWLINE);

			EmailDocument faxEmail = new();

			var faxCommandBase64Encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(faxCommands.ToString()));
			AttachmentDef faxCommandsAttachment = new("FaxCommand.base64", Encoding.ASCII.GetBytes(faxCommandBase64Encoded));
			_ = faxEmail.Attachments.Add(faxCommandsAttachment);

			if (!FaxFileName.IsEmpty)
			{
				AttachmentDef faxAttachment = new(FaxFileName);
				_ = faxEmail.Attachments.Add(faxAttachment);
			}

			faxEmail.FromAddress = Env.Registry.MailboxEmailAddress;
			faxEmail.Subject = $"{EDI_FAX_DOC_SUBJECT} {sendDateTime:yyyMMddHHmmss}"; // just need the subject to be somewhat unique, so that index on subject could work. 
			faxEmail.Body = (NoResString)"ediEnterprise Fax"; // Key used by fax gateway
			faxEmail.Recipients = faxGateway;
			faxEmail.Send();
		}

		public ZString FaxNumber;
		public ZString FaxAttention;
		public ZString FaxAttentionCompany;
		public ZString SysFaxJobId;
		public ZString SysId;
		public ZString FaxFileName;
		public ZString SendingCompanyCode;
	}
}
