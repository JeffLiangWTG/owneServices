using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.GUI.PlugIn;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public class MenuBuilder : ASYCUDA.GUI.MenuBuilder
	{
		public MenuBuilder(ASYCUDA.Business.AsycudaManifestHeader header, ZForm mainForm)
			: base(header, mainForm)
		{
		}

		public override ResourceString MenuCaption => ResString.GetMultilingualString("b81c0604-6b44-4587-b656-4e617cbf1ea1", "EU H7");

		public sealed override ZMenuItem[] BuildMenu()
		{
			return BuildMenuCore().ToArray();
		}

		protected virtual List<ZMenuItem> BuildMenuCore()
		{
			var menuItems = new List<ZMenuItem>();

			AddSendMessageMenuItem(menuItems);
			AddImportTestMessageMenuItemIfRequired(menuItems);
			AddUploadDocumentsMenuItem(menuItems);
			AddDocumentRequestMenuItemIfRequired(menuItems);

			return menuItems;
		}

		void AddSendMessageMenuItem(List<ZMenuItem> menuItems)
		{
			var caption = ResString.GetMultilingualString("ea9a1c80-3b92-42cb-9d51-a18b6a936e6d", "Send Message to Customs");
			MenuBuilderHelper.AddMenuItem(mainForm, menuItems, caption, Header, () =>
			{
				if (CheckBeforeSending() && Header.ApplicationBusinessProvider is H7ApplicationBusinessProvider applicationBusinessProvider)
				{
					var messageSendingParent = applicationBusinessProvider.GetNewMessageSendingObjectParent((AsycudaManifestHeader)Header);
					AppendHeaderLevelMessageErrorToMessageSendingObject(messageSendingParent);
					using (ZFormPostingButtonsStrategy.DeferredUpdateSaveButtonsBasedOnHasChanges(mainForm, true))
					{
						using var form = GetMessageSendingForm(messageSendingParent);
						ZFormModaliser.ShowDialogWithoutDispose(form, mainForm);
					}
				}
			},
			validateManifest: false);
		}

		protected void AppendHeaderLevelMessageErrorToMessageSendingObject(BaseMessageSendingObjectParent messageSendingParent)
		{
			Header.Validation.ValidateAll();

			var headerLevelMessageErrors = new CustomsNotificationCollector(Header, false, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetMessageErrors();

			if (headerLevelMessageErrors.HasMessageErrors())
			{
				foreach (BusinessObject sendingObject in messageSendingParent.SendingObjectsCollection)
				{
					sendingObject.AddRowMessageError(System.Environment.NewLine + headerLevelMessageErrors.ToUniqueMessageListString());
				}
			}
		}

		void AddUploadDocumentsMenuItem(List<ZMenuItem> menuItems)
		{
			var caption = ResString.GetMultilingualString("8c2f8e2d-264e-4d24-a90e-aeb9a990da8e", "Upload Documents");
			MenuBuilderHelper.AddMenuItem(mainForm, menuItems, caption, Header, () =>
			{
				if (CheckBeforeSending() && Header.ApplicationBusinessProvider is H7ApplicationBusinessProvider applicationBusinessProvider)
				{
					var messageSendingParent = applicationBusinessProvider.GetNewUploadDocumentsMessageSendingObjectParent((AsycudaManifestHeader)Header);
					if (messageSendingParent.SendingObjectsCollection.Count > 0)
					{
						using (var form = GetUploadDocumentsForm(messageSendingParent))
						{
							ZFormModaliser.ShowDialogWithoutDispose(form, mainForm);
						}
					}
					else
					{
						Globals.Message.ShowInformation(NoRequestedDocumentsToSendMessage);
					}
				}
			},
			validateManifest: false);
		}

		void AddDocumentRequestMenuItemIfRequired(List<ZMenuItem> menuItems)
		{
			if (RequiresDocumentRequestMenuItem)
			{
				var caption = ResString.GetMultilingualString("0e7a4a12-d6ac-4f03-bc90-be0b5c2e2f16", "Request Documents");
				MenuBuilderHelper.AddMenuItem(mainForm, menuItems, caption, Header, () =>
				{
					if (CheckBeforeSending() && Header.ApplicationBusinessProvider is H7ApplicationBusinessProvider applicationBusinessProvider)
					{
						var messageSendingParent = applicationBusinessProvider.GetNewDocumentRequestMessageSendingObjectParent((AsycudaManifestHeader)Header);
						if (messageSendingParent.SendingObjectsCollection.Count > 0)
						{
							using (var form = GetDocumentRequestForm(messageSendingParent))
							{
								ZFormModaliser.ShowDialogWithoutDispose(form, mainForm);
							}
						}
						else
						{
							Globals.Message.ShowInformation(NoRequestedDocumentsToSendMessage);
						}
					}
				},
				validateManifest: false);
			}
		}

		protected virtual bool RequiresDocumentRequestMenuItem => false;

		protected virtual string NoRequestedDocumentsToSendMessage => Res.GetString("dc43d362-d489-4fa6-bf14-cc3b6d5b774a", "No bills on this header have any Requested Documents with status of 'Request Opened by Customs'.");

		protected virtual MessageSendingForm GetMessageSendingForm(BaseMessageSendingObjectParent messageSendingParent)
		{
			return new MessageSendingForm(messageSendingParent);
		}

		protected virtual UploadDocumentsForm GetUploadDocumentsForm(BaseMessageSendingObjectParent messageSendingParent)
		{
			return new UploadDocumentsForm(messageSendingParent);
		}

		protected virtual DocumentRequestForm GetDocumentRequestForm(BaseMessageSendingObjectParent messageSendingParent)
		{
			return new DocumentRequestForm(messageSendingParent);
		}

		protected virtual bool CheckBeforeSending() => CustomsPlugIn.FormPreSaved(Header, mainForm);

		void AddImportTestMessageMenuItemIfRequired(List<ZMenuItem> menuItems)
		{
			if (ObjectFactory.Get<IProductRegistration>().Key.DatabaseType == DatabaseTypes.Codes.Test && GlbStaff.CurrentUser.IsSupportUser)
			{
				var caption = ResString.GetMultilingualString("4a948784-57a4-406b-88a0-8b5d244c7c3b", "Import EU H7 Incoming Message[DEV Only]");

				var importer = new EUH7TestIncomingMessageImporter((AsycudaManifestHeader)Header);
				MenuBuilderHelper.AddMenuItem(mainForm, menuItems, caption, Header, () => importer.ImportEUICS2IncomingMessageFromXmlFile(), validateManifest: false);
			}
		}
	}
}
