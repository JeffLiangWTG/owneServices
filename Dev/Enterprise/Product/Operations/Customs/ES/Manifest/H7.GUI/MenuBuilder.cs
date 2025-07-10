using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Manifest.H7.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.Manifest.H7.GUI
{
	public class MenuBuilder : EU.H7.GUI.MenuBuilder
	{
		public MenuBuilder(ASYCUDA.Business.AsycudaManifestHeader header, ZForm mainForm) : base(header, mainForm)
		{
		}

		protected override EU.H7.GUI.MessageSendingForm GetMessageSendingForm(BaseMessageSendingObjectParent messageSendingParent)
		{
			return new MessageSendingForm(messageSendingParent);
		}

		protected override List<ZMenuItem> BuildMenuCore()
		{
			var menuItems = base.BuildMenuCore();

			AddCreateG3DeclarationMenuItem(menuItems);
			AddRevokeG3DeclarationMenuItem(menuItems);

			return menuItems;
		}

		protected override string NoRequestedDocumentsToSendMessage => Res.GetString("ca830f6e-d252-4b88-bb93-5bb24d733451", "No documents can be requested for the bills on this header.");

		void AddCreateG3DeclarationMenuItem(List<ZMenuItem> menuItems)
		{
			var caption = ResString.GetMultilingualString("20566A6A-7CEF-49BC-8B58-D7AC275B99A1", "Create G3 Declaration");
			AddG3DeclarationMenuItem(menuItems, caption, false);
		}

		void AddRevokeG3DeclarationMenuItem(List<ZMenuItem> menuItems)
		{
			var caption = ResString.GetMultilingualString("B8BCF265-DB1B-4D4B-990F-20BF8E952F4C", "Revoke G3 Declaration");
			AddG3DeclarationMenuItem(menuItems, caption, true);
		}

		bool CertificateHasMessageErrors(AsycudaManifestHeader header)
		{
			header.Validation.ValidateAMA_CustomsProfile();
			return header.AMA_CustomsProfileInfo.HasMessageErrors();
		}

		bool HasValidBrokerAndValidateCertificate()
		{
			var header = Header as AsycudaManifestHeader;
			var brokerStaff = header.CustomsAgent;

			if (brokerStaff == null || CertificateHasMessageErrors(header))
			{
				Globals.Message.Show(MissingOrWrongBrokerOrCertificate);
				return false;
			}

			return true;
		}

		static ZString MissingOrWrongBrokerOrCertificate => Res.GetString("fed0e194-ba82-4f88-930e-3c69c9e983d2", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate.");

		protected override bool CheckBeforeSending() => base.CheckBeforeSending() && HasValidBrokerAndValidateCertificate();

		void AddG3DeclarationMenuItem(List<ZMenuItem> menuItems, ZArchitecture.Core.ResourceString caption, bool isRevoke = false)
		{
			MenuBuilderHelper.AddMenuItem(mainForm, menuItems, caption, Header, () =>
			{
				if (CheckBeforeSending() && Header.ApplicationBusinessProvider is H7ApplicationBusinessProvider)
				{
					var messageSendingParent = new G3MessageSendingObjectParent((AsycudaManifestHeader)Header, isRevoke);

					if (isRevoke && messageSendingParent.SendingObjectsCollection.Count == 0)
					{
						Globals.Message.ShowInformation(NoAcceptedG3DeclarationToSendMessage);
						return;
					}

					AppendHeaderLevelMessageErrorToMessageSendingObject(messageSendingParent);

					var form = new G3MessageSendingForm(messageSendingParent, isRevoke);
					ZFormModaliser.ShowDialogAndDispose(form, mainForm);
				}
			}, validateManifest: false);
		}

		protected override EU.H7.GUI.DocumentRequestForm GetDocumentRequestForm(BaseMessageSendingObjectParent messageSendingParent)
		{
			return new DocumentRequestForm(messageSendingParent);
		}

		protected override EU.H7.GUI.UploadDocumentsForm GetUploadDocumentsForm(BaseMessageSendingObjectParent messageSendingParent)
		{
			return new UploadDocumentsForm(messageSendingParent);
		}

		protected override bool RequiresDocumentRequestMenuItem => true;

		static ZString NoAcceptedG3DeclarationToSendMessage => Res.GetString("63c62db0-1e3f-4926-9a30-f20f10da435f", "No bills on this header have any 'G3D - G3D Declaration' message accepted with MRN = G3 MRN to Revoke.");
	}
}
