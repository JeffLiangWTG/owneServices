using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.ExitControl.Business;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.GUI.PlugIn;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.ExitControl.GUI
{
	public class ExitControlDocRequestMenuCreator
	{
		public ExitControlDocRequestMenuCreator(CusExitHeader header)
		{
			this.header = Argument.NotNull(header, nameof(header));
		}

		readonly CusExitHeader header;
		ZMenuItem createdMenuItem;

		public ZMenuItem Create()
		{
			var obj = new ZMenuItem(ResString.GetMultilingualString("46E67AED-E2A6-4797-8755-C3FCABC84EDC", "Download EAL Clearance Document"), DownloadEALDocMenu_Click);
			createdMenuItem = obj;
			return obj;
		}

		void DownloadEALDocMenu_Click(object sender, EventArgs e)
		{ 
			if (SaveIfRequiredAndConfirmedByUser())
			{
				if (CheckDeclarationBeforeSending(out var broker))
				{
					var sendingObject = new MessageSendingObject(header, broker);
					SendDocumentRequestToCustoms(((ICertificateProvider)sendingObject).CertificateName);
				}
			}
		}

		void SendDocumentRequestToCustoms(ZString certificateName)
		{
			int messagesSentCount = ZInt.Zero;
			try
			{
				var factory = new BusinessObjectFactory();
				var newFactoryExitHeader = factory.Load<CusExitHeader>(header.PK);
				newFactoryExitHeader.Reload();

				foreach (var exitReport in newFactoryExitHeader.CusExitReports)
				{
					if (!(exitReport?.Consignment?.CXC_MovementReference ?? ZString.Empty).IsEmpty && !certificateName.IsEmpty)
					{
						var ealDocRequest = new ExitControlDocumentRequest(exitReport, certificateName);
						messagesSentCount += ealDocRequest.RequestMissingDocuments();
					}
				}
				factory.Save();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ZExceptionReporting.HandleSaveException(ex);
			}

			Globals.Message.Show(ResString.GetMultilingualString("0E0D55F7-9149-4772-92D7-F743DA87D1D7", "{0} Document Capture request(s) created", messagesSentCount.ToString(Culture.Current)));
		}

		bool SaveIfRequiredAndConfirmedByUser()
		{
			var zForm = (ZForm)createdMenuItem.GetMainMenu()?.GetForm();
			var topLevelBizObj = (zForm?.BusinessEntity as BusinessObject) ?? header.Parent ?? header;
			return CustomsPlugIn.FormPreSaved(topLevelBizObj, zForm);
		}

		bool CheckDeclarationBeforeSending(out GlbStaff broker)
		{
			broker = null;
			var continueWithSend = false;

			if (header.CusExitReports.Count == 0)
			{
				Globals.Message.Show(CommonPromptMessages.NoReportErrorMessage);
			}
			else
			{
				broker = header.CustomsAgent;
				if (broker == null || CertificateHasMessageErrors())
				{
					Globals.Message.ShowError(CommonPromptMessages.CredentialsErrorMessage);
				}
				else
				{
					continueWithSend = true;
				}
			}
			return continueWithSend;

			ZBool CertificateHasMessageErrors()
			{
				header.Validation.ValidateCXH_CustomsProfile();
				return header.CXH_CustomsProfileInfo.HasMessageErrors();
			}
		}
	}
}
