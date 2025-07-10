using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.Customs.FR.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.Module
{
	public class PntsInboundInterchangeImporter
	{
		readonly BusinessObjectFactory factory;

		public PntsInboundInterchangeImporter(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public static bool IsMenuItemVisible()
		{
			return GlbStaff.CurrentUser.IsSupportUser;
		}

		public MenuItem GetNewMenuItem()
		{
			var menuItem = new ZMenuItem((NoResString)PntsResponseInterchangeAddActionText, AddPntsInboundInterchange);
#if DEBUG
			TypeDescriptor.AddAttributes(menuItem, new SuppressFormsLocalizedTestAttribute());
#endif
			return menuItem;
		}

		void AddPntsInboundInterchange(object sender, EventArgs e)
		{
			using (var stream = GetXmlFileStream())
			{
				if (stream != null)
				{
					try
					{
						var interchange = CreatePntsInterchange(stream);
						try
						{
							factory.Save();
							Globals.Message.ShowInformation(Res.GetString("467297C3-7DCD-49AC-9B3D-5AAE78E0614B", "A PNTS Inbound interchange was saved with interchange number {0}.", interchange.EI_InterchangeNum));
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

		EDIInterchange CreatePntsInterchange(Stream stream)
		{
			var messageContent = new StreamReader(stream).ReadToEnd();
			var result = factory.New<FRInterchange>();
			result.EI_ApplicationCode = EDIInterchange.ApplicationCodes.GenericMessageDelivery;
			result.EI_BodyText = messageContent;
			result.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			result.EI_Status = EDIInterchangeStatusList.Codes.Queued;
			result.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsPNTS;
			result.EI_From = FRCustomsDataRegistry.Instance.RecipientID.Value;
			result.EI_To = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
			return result;
		}

		protected virtual Stream GetXmlFileStream()
		{
			using (var openFileDialog = new ZOpenFileDialog())
			{
				openFileDialog.CheckFileExists = true;
				openFileDialog.DefaultExt = ".xml";
				openFileDialog.Filter = (NoResString)"XML Message File (*.xml)|*.xml";
				openFileDialog.Multiselect = false;
				return openFileDialog.ShowDialog() == DialogResult.OK ? openFileDialog.OpenFile() : null;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "CWSupport only Menu Item")]
		public const string PntsResponseInterchangeAddActionText = "Add PNTS Inbound Interchange (CWSupport Only)";
	}
}
