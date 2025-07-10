using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.DIF.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.DIF.GUI
{
	public partial class DIFForm : ZChildForm
	{
		public DIFForm(DIFHostWrapper difWrapper)
			: base(difWrapper)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, null, CancelOrCloseButton, SaveButton);
			SendButton.Enabled = Env.Security.CACustomsDIFEdit.IsAllowed;
			DifUserControl.DocumentsGrid.RowsDeleted += DocumentsGrid_RowsDeleted;
		}

		public DIFForm(DIFDocument dIFDocument)
			: this(dIFDocument.HostWrapper)
		{
			this.dIFDocument = dIFDocument;
		}
		readonly DIFDocument dIFDocument;

		void DocumentsGrid_RowsDeleted(object sender, ZArchitecture.RowsDeletingEventArgs e)
		{
			UpdateButtonStatus();
		}

		public new DIFHostWrapper BusinessEntity
		{
			get { return (DIFHostWrapper)base.BusinessEntity; }
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		public override string FormCaption
		{
			get { return Res.GetString("ff906aa6-469d-4427-a9a2-d5fdaf2fbfbe", "Document Image System"); }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (BusinessEntity != null)
			{
				BusinessEntity.HasChangesChanged += BusinessEntity_HasChangesChanged;

				UpdateButtonStatus();
			}
		}

		void BusinessEntity_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			UpdateButtonStatus();
		}

		void UpdateButtonStatus()
		{
			if (BusinessEntity != null)
			{
				if (BusinessEntity.HasChanges)
				{
					CancelOrCloseButton.CaptionResourceString = Enterprise.Customs.CA.DIF.GUI.Res.GetData("712deba6-cbd9-4543-acea-b7151af6f9e7", "&Cancel");
					SendButton.CaptionResourceString = Enterprise.Customs.CA.DIF.GUI.Res.GetData("2c01f235-9716-41e6-9aaa-77f03081058c", "Save && &Send");
					SaveButton.Enabled = true;
				}
				else
				{
					CancelOrCloseButton.CaptionResourceString = Enterprise.Customs.CA.DIF.GUI.Res.GetData("cb3a7b86-f851-4392-9556-fc2cbb60d0f6", "&Close");
					SendButton.CaptionResourceString = Enterprise.Customs.CA.DIF.GUI.Res.GetData("2b535fbb-68f1-45d2-b4f6-aee361e8a2a0", "&Send");
					SaveButton.Enabled = false;
				}
				CancelOrCloseButton.UpdateCaption();
				SendButton.UpdateCaption();
			}
		}

		void RefreshFromAccordingToDIFDocument()
		{
			if (dIFDocument == null)
			{
				DifUserControl.MessageSendPanel.Visible = false;
			}
			else
			{
				DifUserControl.RequiredDocumentsPanel.Visible = false;
			}
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			if (dIFDocument != null)
			{
				DifUserControl.DocumentsGrid.SelectSingleElement(dIFDocument);
			}
			RefreshFromAccordingToDIFDocument();
		}

		protected override void ZForm_Closing(object sender, CancelEventArgs e)
		{
			base.ZForm_Closing(sender, e);
			if (!e.Cancel)
			{
				BusinessEntity.IsActive = false; //by this time, save should be complete
			}
		}

		protected override void HandleSaveWhileClosing(CancelEventArgs e)
		{
			BusinessEntity.IsActive = true; //just in case save is not yet finished (i.e. BusinessEntity recycled)
			base.HandleSaveWhileClosing(e);
		}

		bool ValidateAndSaveData()
		{
			return FireSaveButton() == ContinueWithSave.Yes;
		}

		protected override void Dispose(bool disposing)
		{
			if (BusinessEntity != null)
			{
				BusinessEntity.HasChangesChanged -= BusinessEntity_HasChangesChanged;
			}
			base.Dispose(disposing);
		}

		void SendButton_Click(object sender, EventArgs e)
		{
			if (BusinessEntity.DISDocuments.Count == 0)
			{
				Globals.Message.ShowError(Res.GetString("5D7AFED8-D8B6-40B5-8057-18444A4F8AAB", "Please enter at least one DIF document first."));
			}
			else
			{
				if (!Env.Security.CACustomsDIFSendMessage.IsAllowed)
				{
					Env.Security.CACustomsDIFSendMessage.ShowError();
				}
				else if (ValidateAndSaveData())
				{
					var proceed = true;
					var bizObjToBeValidated = dIFDocument == null ? BusinessEntity : (BusinessObject)dIFDocument;
					if (bizObjToBeValidated.HasMessageErrors())
					{
						var validation = MessageSendingValidation.New(bizObjToBeValidated, new CustomsNotificationCollector(bizObjToBeValidated, true, true, ZNotificationCollector.PropertyDescriptionType.HumanReadableName));
						var notifications = validation.CheckBusinessObjectLevelValidation();

						if (notifications.ContainsError())
						{
							proceed = false;
							Globals.Message.ShowError(notifications.NotificationsAsString());
						}
						else if (notifications.ContainsWarning())
						{
							if (!Env.Security.CACustomsDIFSendWithMessageErrors.IsAllowed)
							{
								proceed = false;
								Globals.Message.ShowError(Customs.Business.SingleMessageManager.MessageErrorsExistWithNoSecurityRight + " " + Env.Security.CACustomsDIFSendWithMessageErrors.DisplayTextPathToSecurityRight);
							}
							else if (Globals.Message.Show(notifications.NotificationsAsString(), "Continue?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
							{
								proceed = false;
							}
						}
					}

					if (proceed)
					{
						(BusinessObjectFactory factory, ZString notificationMessage) saveFactoryAndNotify = (null, ZString.Empty);

						if (dIFDocument == null)
						{
							var coll = new MessageSendingActionCollection(BusinessEntity);
							using (MessageSendingForm sendForm = new MessageSendingForm(coll))
							{
								ZFormModaliser.ShowDialogWithoutDispose(sendForm);

								if (sendForm.ProceedWithSend)
								{
									coll.SendMessages();
									saveFactoryAndNotify = (coll.Factory, "Messages Sent.");
								}
							}
						}
						else
						{
							var coll = new MessageSendingActionCollection(dIFDocument.Factory);
							coll.Add(dIFDocument.MessageSendingAction);
							coll.SendMessages();
							saveFactoryAndNotify = (coll.Factory, "Message Sent.");
						}

						if (saveFactoryAndNotify.factory is BusinessObjectFactory factory)
						{
							try
							{
								factory.Save();
								Globals.Message.ShowInformation(saveFactoryAndNotify.notificationMessage);
								Close();
							}
							catch (ZSaveException exception)
							{
								ZExceptionReporting.HandleSaveException(exception);
							}
						}
					}
				}
			}
		}
	}
}
