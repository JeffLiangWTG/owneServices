using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.DocumentEngineCore.Exceptions;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CustomsWare.Business
{
	public class IncomingApplicationMessageProcessor : ApplicationTypeMessageProcessor, INotifications
	{
		public IncomingApplicationMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore
		{
			get { return Res.GetString("3DD0CD50-3B28-4CEE-A34B-F9C0009EF7EA", "CustomsWare Incoming Message Processor"); }
		}

		protected override string ApplicationCodeCore
		{
			get { return ApplicationCodeList.Codes.CustomsWare; }
		}

		protected override void ProcessMessageCore(EDIMessage message)
		{
			var dataAdapter = InputDocumentValueObjectDataAdapter.New();
			var decCollection = new BaseJobDeclarationCollection(message.Factory);
			var serializer = new InputDocumentXMLValueObjectSerializer();

			using (var stream = LargeMessageHelper.CopyAndDispose(message.GetEM_MessageTextReader()))
			{
				try
				{
					serializer.ImportXmlData(stream,
						dataAdapter,
						decCollection,
						new SingleBusinessObjectFactoryProvider(message.Factory),
						this);
				}
				catch (ImageFormatException ex)
				{
					Logger?.LogWarning($"Message {message.EM_MessageNum} ({message.Interchange?.EI_InterchangeNum} / {message.Interchange?.eHubID} / {message.Interchange?.EI_SessionGUID}) contains invalid image. {ex.Message}");
				}
			}

			var declaration = decCollection.Cast<BaseJobDeclaration>().FirstOrDefault();
			if (declaration != null)
			{
				declaration.Messages.Add(message);
				Logger?.Log(Integration.LogType.Information, $"Linked message {message.EM_MessageNum} ({message.Interchange?.EI_InterchangeNum} / {message.Interchange?.eHubID} / {message.Interchange?.EI_SessionGUID}) to declaration {declaration.JE_DeclarationReference} for company {declaration.Company.GC_Code}");
				CreateNotificationEmail(declaration, serializer.InputDocument);
			}

			message.EM_Status = EDIMessage.Status.Received;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Only used in HTML body")]
		void CreateNotificationEmail(BaseJobDeclaration declaration, XSD.InputDocument inputDocument)
		{
			try
			{
				if (CustomsWareRegistry.Instance.SendNotifications.Value)
				{
					EmailDef email = null;
					var subject = "CustomsWare Response for Job: " + declaration.JE_DeclarationReference;
					var url = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(declaration);
					var hrefJobNumber = "<a href=\"" + url + "\">" + declaration.JE_DeclarationReference + "</a>";

					HtmlTableCreator table = new HtmlTableCreator(new string[] { "Reference Number", "Status", "Status Description", "Release Date" });
					table.EnableHTMLEncoding = false;

					foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
					{
						table.WriteRow(entry.CH_BGMReference, entry.CH_EntryStatus, entry.EntryHeaderStatusDescription, entry.CH_EntryReleaseDate);
					}

					var html = $"<br /><strong>{hrefJobNumber}</strong><br /><br />"
						+ table.ToHtml()
						+ $"<br />Regards,<br /><br />{Core.Constants.ProductName} Automated Messages Sender<br />";

					HtmlNotificationEmailSender emailSender = new HtmlNotificationEmailSender();

					email = emailSender.CreateEmail(subject, html);

					ZString emailAddress = "";

					var senderCode = declaration.Messages.LastOutgoingMessage?.EM_SystemCreateUser ?? ZString.Empty;
					if (!senderCode.IsEmpty && senderCode != User.ServiceUserCode)
					{
						var sender = declaration.Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, senderCode);
						emailAddress = sender?.GS_EmailAddress ?? ZString.Empty;
					}

					if (emailAddress.IsEmpty && declaration.CusAgent != null)
					{
						emailAddress = declaration.CusAgent.GS_EmailAddress;
					}

					if (inputDocument != null && inputDocument.AttachmentList.Count > 0)
					{
						inputDocument.AttachmentList.OfType<XSD.Attachment>().ForEach(doc => email.Attachments.Add(new AttachmentDef(doc.FileName, System.Convert.FromBase64String(doc.AttachmentStream))));
					}

					if (!emailAddress.IsEmpty)
					{
						email.AddRecipientForSystemCommunication(emailAddress);
						Env.OutgoingCustomsMailManager.CreateAndSave(email);
					}
					else
					{
						var registryKey = CustomsDataRegistry.Instance.ThirdPartyCustomsResponseEmailNotificationGroup;
						Env.OutgoingCustomsMailManager.CreateAndSave(email, registryKey.Value, GroupSourceLocator.GetFromRegistryItem(registryKey));
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				//dont fail because of this
			}
		}

		#region INotifications Members

		void INotifications.Add(INotification notification)
		{
			if (notification != null && Logger != null)
			{
				if (notification is ErrorNotification)
				{
					Logger.LogError(notification.Message);
				}
				else
				{
					Logger.Log(notification.Message);
				}
			}
		}

		#endregion
	}
}
