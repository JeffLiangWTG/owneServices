using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IL.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Integration.Customs.IL;

namespace Enterprise.Customs.IL.Manifest.Module
{
	public class ManifestInboundInterchangeImporter : ILManifestInboundInterchangeImporter
	{
		readonly BusinessObjectFactory factory;

		public ManifestInboundInterchangeImporter(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public bool Precondition()
		{
			return GlbStaff.CurrentUser.IsSupportUser;
		}

		public object GetNewMenuItem()
		{
			var menuItem = new ZMenuItem((NoResString)ManifestResponseInterchangeAddActionText, AddManifestInboundInterchange);
#if DEBUG
			TypeDescriptor.AddAttributes(menuItem, new SuppressFormsLocalizedTestAttribute());
#endif
			return menuItem;
		}

		void AddManifestInboundInterchange(object sender, EventArgs e)
		{
			using (var stream = GetXmlFileStream())
			{
				if (stream != null)
				{
					try
					{
						var interchange = CreateManifestInterchange(stream);
						try
						{
							factory.Save();
							Globals.Message.ShowInformation(Res.GetString("78F9EFB9-112E-4E7C-89CE-BC71A7DC0FA8", "An IL Manifest Inbound interchange was saved with interchange number {0}.", interchange.EI_InterchangeNum));
						}
						catch (ZSaveException exception)
						{
							ZExceptionReporting.HandleSaveException(exception);
						}
					}
					catch (Exception exception)
					{
						Globals.Message.ShowError(exception.Message);
					}
				}
			}
		}

		EDIInterchange CreateManifestInterchange(Stream stream)
		{
			var messageContent = new StreamReader(stream).ReadToEnd();
			var result = factory.New<EDIInterchange>();
			result.EI_ApplicationCode = EDIInterchange.ApplicationCodes.ILCustoms;
			result.EI_BodyText = messageContent;
			result.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			result.EI_Status = EDIInterchangeStatusList.Codes.Queued;
			result.EI_InterchangeType = IL.Business.ILMessageTypeList.Codes.MAN;
			result.EI_From = MessageProviderHelper.MessageTo;
			result.EI_To = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
			result.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;
			result.EI_SessionGUID = ZGuid.NewZGuid();
			return result;
		}

		protected virtual Stream GetXmlFileStream()
		{
			using (var openFileDialog = new ZOpenFileDialog())
			{
				openFileDialog.CheckFileExists = true;
				openFileDialog.DefaultExt = ".xml"; // May be a path or url.
				openFileDialog.Filter = (NoResString)"XML Message File (*.xml)|*.xml"; // May be a path or url.
				openFileDialog.Multiselect = false;
				return openFileDialog.ShowDialog() == DialogResult.OK ? openFileDialog.OpenFile() : null;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "CWSupport only Menu Item")]
		public const string ManifestResponseInterchangeAddActionText = "Add IL Manifest Inbound interchange (CWSupport Only)";
	}
}
