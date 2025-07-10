using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class AsycudaJobDeclarationUniversalMessagingHelper : JobDeclarationUniversalMessagingHelper
	{
		public AsycudaJobDeclarationUniversalMessagingHelper(IJobDeclarationMessageSendingObjectParent messageSendingObject)
			: base(messageSendingObject)
		{
		}

		protected override ZString UniversalCustomsMessagingRecipientID => "ASYCUDADECLARATION";// English only message content

		const string asycudaDeclaration = "ASYCUDA Declaration"; // English only message content

		public int SendAsycudaDeclarationUniversalMessage()
		{
			int messagesCount = 0;
			var declaration = MessageSendingObjectParent.ParentDeclaration;

			foreach (var sendingObject in MessageSendingObjectParent.SendingObjectsCollection.Cast<JobDeclarationMessageSendingObject>().Where(x => x.ShouldSend))
			{
				var entryHeader = sendingObject.Header;

				if (entryHeader != null)
				{
					var declarationContent = declaration.GetUniversalShipment(
						new DeclarationDataObjectWriterConfiguration { EntryHeaderPKsToPopulate = new List<ZGuid> { entryHeader.PK } },
						filteredDataContextType: DataContextType.CustomsDeclaration,
						roleType: RecipientRoleType.ASY);
					var dataContext = declarationContent.DataContext;

					dataContext.SetWorkflowInfo(
						new WorkflowInfo
						{
							ActionPurpose = new CodeDescriptionPair() { Code = "Y" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Description = asycudaDeclaration },
							RecipientRoles = new List<RecipientRoleDetail> { new RecipientRoleDetail { Type = RecipientRoleType.ASY } },
						}
					);

					var recipientRole = dataContext.RecipientRoleCollection.Cast<RecipientRole>().FirstOrDefault(x => x.Code == RecipientRoleType.ASY);
					if (recipientRole != null)
					{
						recipientRole.Description = asycudaDeclaration;
					}

					if (dataContext.DataSourceCollection.FirstOrDefault(x => x.Type.GetValueOrDefault() == nameof(DataContextType.CustomsDeclaration)) is IDataSourceDataObject dataSource)
					{
						dataSource.Type = Constants.AsycudaDeclarationDataContextType;
						dataSource.Key = entryHeader.CH_BGMReference;
					}

					if (CreateEDIEnterchage(entryHeader, declarationContent, UniversalCustomsMessagingRecipientID))
					{
						messagesCount++;
					}

					try
					{
						declaration.Factory.Save();
					}
					catch (ZSaveException ex)
					{
						ZExceptionReporting.HandleSaveException(ex);
					}

					entryHeader.Messages.Reload(false);
				}
			}

			return messagesCount;
		}

		public void SendEmail(Customs.Business.CusEntryHeader entry, Stream stream, IGlbStaff user)
		{
			try
			{
				var subject = "Asycuda World Brokerage File – " + entry.CH_BGMReference; // only used in HTML Body
				var content = @"Dear Client,
<br />Enclosed please find the mapped ASYCUDA World Brokerage file.
<br />
<br />Regards,
<br />WiseTech Global
"; // only used in HTML Body

				var emailSender = new HtmlNotificationEmailSender();
				var email = emailSender.CreateEmail(subject, content);
				var receiver = user.GS_EmailAddress;
				email.AddRecipientForSystemCommunication(receiver);

				var fileName = GetAttachmentFileName(entry);
				var data = AttachmentDef.StreamToByteArray(stream);
				email.Attachments.Add(new AttachmentDef(fileName, data));

				Env.OutgoingCustomsMailManager.CreateAndSave(email, entry.Factory);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
			}
		}

		public string GetAttachmentFileName(Customs.Business.CusEntryHeader entry) => entry.CH_BGMReference + "_" + entry.Declaration.JE_AgentsReference + "_Asycuda.xml";

		protected override ZString ValidateCanSubmitCore()
			=> MessageSendingObjectParent.SendingObjectsCollection.Cast<JobDeclarationMessageSendingObject>()
				.Any(x => x.Header?.CH_BGMReference.IsEmpty ?? true)
				? Res.GetString("92C1CD7B-2A33-4A01-B36E-C806AD8DDADC", "All entries should have references. Please go to menu 'Brokerage - Allocate Entry Reference Number' to allocate references.")
				: string.Empty;
	}
}
