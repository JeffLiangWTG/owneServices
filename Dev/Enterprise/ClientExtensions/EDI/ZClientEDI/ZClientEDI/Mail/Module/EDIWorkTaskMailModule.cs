using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.EDI.Mail.Business;
using Enterprise.Client.EDI.Mail.GUI;
using Enterprise.EConversation.ServiceTasks;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using MimeKit;

namespace Enterprise.Client.EDI.Mail.Module
{
	public abstract class EDIWorkTaskMailModule<T> : MailItemModule
		where T : BusinessObject, IAllowAttachEmailsToEDocs
	{
		#region Standard Overrides

		protected override IFilterControl GetNewFilterControl()
		{
			return new EDIMailItemFilterControl(GridCollection, (EDIWorkTaskMailItemFilterBusinessObject)FilterBusinessObject);
		}
		internal IFilterControl InternalGetNewFilterControl() => GetNewFilterControl();

		protected override sealed IBusinessObjectCollection GetNewGridCollection()
		{
			return GetNewEDIMailItemCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new EDIWorkTaskMailItemFilterBusinessObject();
		}
		internal FilterBusinessObject InternalGetNewFilterBusinessObject() => GetNewFilterBusinessObject();

		#endregion

		#region View

		protected override IZForm ShowViewForm(BusinessObject selectedBusinessObject)
		{
			MailItem mail = (MailItem)selectedBusinessObject;

			try
			{
				ShowMailInOutlookExpress(mail);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.Show("There was a problem showing this email in Microsoft Outlook Express.");
				return base.ShowViewForm(selectedBusinessObject);
			}

			return null;
		}
		internal IZForm InternalShowViewForm(BusinessObject selectedBusinessObject) => ShowViewForm(selectedBusinessObject);

		internal protected virtual
		void ShowMailInOutlookExpress(MailItem mail)
		{
			string filename = Temp.GetTempFileNameWithExtension("eml");     // Outlook Express file extension
			File.WriteAllText(filename, BusinessObjectEmailAttacher.GetEmailTextForOutlookExpressMessage(mail));
			FileOpener.Open(filename);
		}

		internal protected virtual
		void ShowMailInOutlookForForward(MailItem attachedEmail)
		{
			ShowMailInOutlookExpress(attachedEmail);
			ResetStatusTo(new MailItem[] { attachedEmail }, MailStatus.Processed);
		}

		#endregion

		#region Actions

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewStandardMenuItems());

			MenuItem replyMenuItem = new ZMenuItem((NoResString)"Reply", new EventHandler(ReplyAllClick));
			replyMenuItem.MenuItems.Add(new ZMenuItem((NoResString)"Reply", ReplyClick));
			replyMenuItem.MenuItems.Add(new ZMenuItem((NoResString)"Reply All", ReplyAllClick));

			result.Add(replyMenuItem);
			result.Add(new ZMenuItem((NoResString)"Forward", new EventHandler(ForwardClick)));
			string workTaskTypeName = ((ZString)WorkTaskTypeName).ToTitleCase();
			result.Add(new ZMenuItem((NoResString)string.Format("Create {0}", workTaskTypeName), new EventHandler(CreateWorkTaskClick)));
			result.Add(new ZMenuItem((NoResString)string.Format("Attach to {0}", workTaskTypeName), new EventHandler(AttachWorkTaskClick)));
			return result.ToArray();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewActionMenuItems());
			result.Add(new ZMenuItem((NoResString)"Mark as PRS", new EventHandler(MarkAsProcessed)));
			return result.ToArray();
		}

		internal protected CustomerServiceEmail SetEmailAddressAndDisplayName(MailItem selectedMailItem)
		{
			CustomerServiceEmail result = GetNewServiceEmail(selectedMailItem);

			result.Subject = selectedMailItem.MI_Subject;

			string prefix = "RE: ";

			if (!result.Subject.ToUpper().Contains(prefix))
			{
				result.Subject = prefix + result.Subject.SubstringSafe(0, result.SubjectInfo.MaxLength - prefix.Length);
			}

			if (MailboxAddress.TryParse(selectedMailItem.MI_From, out var mailboxAddress))
			{
				result.ToDisplayName = (!string.IsNullOrEmpty(mailboxAddress.Name) ? mailboxAddress.Name : mailboxAddress.Address) + ";";
				result.ToEmailAddress = mailboxAddress.Address + ";";
			}
			else
			{
				result.ToDisplayName = SelectedMailItem.MI_From + ";";
				result.ToEmailAddress = ";";
			}

			return result;
		}

		protected virtual CustomerServiceEmail GetNewServiceEmail(MailItem selectedMailItem)
		{
			return new CustomerServiceEmail(selectedMailItem);
		}

		internal void ReplyAllClick(object sender, EventArgs e)
		{
			if (SelectedMailItem != null)
			{
				CustomerServiceEmail email = SetEmailAddressAndDisplayName(SelectedMailItem);

				foreach (MailRecipient mailRecipient in SelectedMailItem.MailRecipients)
				{
					var shouldIgnore = EmailAddressesToIgnoreWhenReplyingAll.Any(x => mailRecipient.MR_RecipientMailAddress.ToLower().Contains(x.ToLower()));
					if (!shouldIgnore)
					{
						if (mailRecipient.MR_RecipientMailAddress.Contains("<") && mailRecipient.MR_RecipientMailAddress.Contains(">"))
						{
							ZString recipient = mailRecipient.MR_RecipientMailAddress.Substring(mailRecipient.MR_RecipientMailAddress.IndexOf("<") + 1);
							recipient = recipient.Remove(recipient.Length - 1, 1);
							email.ToEmailAddress += recipient + ";";
							if (mailRecipient.MR_RecipientMailAddress.IndexOf("<") > 0)
							{
								email.ToDisplayName += mailRecipient.MR_RecipientMailAddress.Substring(0, mailRecipient.MR_RecipientMailAddress.IndexOf("<") - 1) + ";";
							}
							else
							{
								email.ToDisplayName += recipient + ";";
							}
						}
						else
						{
							email.ToDisplayName += mailRecipient.MR_RecipientMailAddress + ";";
							email.ToEmailAddress += mailRecipient.MR_RecipientMailAddress + ";";
						}
					}
				}

				CustomerServiceEmailForm form = new CustomerServiceEmailForm(email);
				form.Show();
				LastShownFormForTest = form;
				email.EmailSent += new EventHandler(Email_EmailSent);
			}
			else
			{
				Globals.Message.Show("Please select a mail item to reply to.");
			}
		}

		internal void ReplyClick(object sender, EventArgs e)
		{
			if (SelectedMailItem != null)
			{
				CustomerServiceEmail email = SetEmailAddressAndDisplayName(SelectedMailItem);

				CustomerServiceEmailForm form = new CustomerServiceEmailForm(email);
				form.Show();
				LastShownFormForTest = form;
				email.EmailSent += new EventHandler(Email_EmailSent);
			}
			else
			{
				Globals.Message.Show("Please select a mail item to reply to.");
			}
		}

		internal void ForwardClick(object sender, EventArgs e)
		{
			if (SelectedMailItem != null)
			{
				ShowMailInOutlookForForward(SelectedMailItem);
			}
			else
			{
				Globals.Message.Show("Please select a mail item to forward.");
			}
		}

		void Email_EmailSent(object sender, EventArgs e)
		{
			if (SelectedMailItem != null)
			{
				SelectedMailItem.MI_Status = MailStatus.Processed;
				SelectedMailItem.Factory.Save();
			}
		}

		const string DocType = "COR";

		internal void CreateWorkTaskClick(object sender, EventArgs e)
		{
			if (SelectedMailItem != null)
			{
				ZController controller = ZControllerFactory.Create(WorkTaskControllerID);
				ZForm form = (ZForm)controller.ShowNewForm();
				if (form != null)
				{
					BusinessObjectEmailAttacher.AttachEmail((IAllowAttachEmailsToEDocs)form.BusinessEntity, DocType, Grid.GetSelectedElements<MailItem>());
				}

				LastShownFormForTest = form;
			}
			else
			{
				string message = string.Format("Please select 1 or more mail items to attach to the new {0}.", WorkTaskTypeName);
				Globals.Message.Show(message);
			}
		}

		void AttachWorkTaskClick(object sender, EventArgs e)
		{
			var selectedMailItems = Grid.GetSelectedElements<MailItem>();
			if (selectedMailItems.Length > 0)
			{
				ZRecordChooser<T> recordChooser = new ZRecordChooser<T>(WorkTaskModuleID);
				recordChooser.ShowModal(Grid.FindForm(), delegate(T[] selectedTasks)
				{
					if (selectedTasks.Length > 0)
					{
						bool showSaveConcurrecyWarning = false;

						foreach (T selectedTask in selectedTasks)
						{
							BusinessObjectEmailAttacher.AttachEmail(selectedTask, DocType, selectedMailItems);

							try
							{
								selectedTask.Factory.Save();
							}
							catch (ZSaveConcurrencyException)
							{
								showSaveConcurrecyWarning = true;
								foreach (var selectedMailItem in selectedMailItems)
								{
									selectedMailItem.Reload();
								}
								var selectedBizO = selectedTask as BusinessObject;
								if (selectedBizO != null)
								{
									selectedBizO.Reload();
								}
							}
						}

						if (showSaveConcurrecyWarning)
						{
							Globals.Message.ShowWarning(string.Format("The selected email(s) or {0}(s) have been modified by another user. The other user's changes have been merged with yours. Please try again.", WorkTaskTypeName));
						}
					}
				});
			}
			else
			{
				string message = string.Format("Please select 1 or more mail items to attach to the {0}.", WorkTaskTypeName);
				Globals.Message.Show(message);
			}
		}

		void MarkAsProcessed(object sender, EventArgs e)
		{
			if (ResetStatusTo(Grid.SelectedElements, MailStatus.Processed) > 0)
			{
				PerformSearch();
			}
		}

		MailItem SelectedMailItem
		{
			get { return (Grid.SelectedElements.Length > 0) ? (MailItem)Grid.SelectedElements[0] : null; }
		}

		public override bool AllowDelete
		{
			get { return true; }
		}

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		#endregion

		#region Abstracts

		protected abstract string WorkTaskTypeName { get; }
		protected abstract IReadOnlyList<string> EmailAddressesToIgnoreWhenReplyingAll { get; }

		protected abstract EDIMailItemCollection GetNewEDIMailItemCollection(BusinessObjectFactory factory);

		protected abstract ControllerID WorkTaskControllerID { get; }
		protected abstract ModuleIdentifier WorkTaskModuleID { get; }

		#endregion

		internal ZForm LastShownFormForTest;

		// internals for tests
		internal void InternalPerformSearch() => PerformSearch();
		internal ZDisplayGrid InternalGrid => Grid;
	}
}
