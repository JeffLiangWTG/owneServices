using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.CryptoUtilities;
using Enterprise.CryptoUtilities.SMIME;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public interface IOutgoingMailManager
	{
		void Create(ITransactionParticipant factory, EmailDef emailToSend);
		void Create(ITransactionParticipant factory, EmailDef emailToSend, Guid recipientGroupPk, IGroupSourceLocator locator);
		void CreateAndSave(EmailDef emailToSend, BusinessObjectFactory factory);
		void CreateAndSave(EmailDef emailToSend);
		void CreateAndSave(EmailDef emailToSend, Guid recipientGroupPk, IGroupSourceLocator locator);
		void CreateAndSaveSimple(string subject, string body, string recipient);
		void CreateAndSaveToCompanyNotificationGroup(string subject, string body, EmailContentTypes emailContentType = null);
		void CreateAndSaveWithCopyToCompanyNotificationGroup(string subject, string body, string[] toRecipients, bool copyToNotificationGroupIfRecipientsNotEmpty = true, EmailContentTypes emailContentType = null);
		void CreateAndSaveToPostmasterGroup(EmailDef emailToSend);
		void CreateAndSaveToPostmasterGroup(EmailDef emailToSend, BusinessObjectFactory factory);
		void CreateAndAttachToEDocs(ITransactionParticipant factory, EmailDef emailToSend, BusinessObject bizOToAttach, string fileName, string docType, string description);

#if DEBUG
		List<EmailDef> EmailsCreated { get; }
#endif
	}

	public interface IOutgoingCustomsMailManager : IOutgoingMailManager
	{
	}

	public interface IOutgoingMIMEManager
	{
		void CreateAndSaveMIME(MIMEMessage message, string sender, string recipient, string subject);
		void CreateAndSaveSignedMIME(string innerAttachmentName, byte[] innerAttachmentContents, string contentType, string contentDisposition, string sender, string recipient, string subject, Store signingStore, Certificate encryptionCert);
	}
}
