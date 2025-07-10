using System;
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
	public class DeltaGInboundInterchangeImporter
	{
		readonly BusinessObjectFactory factory;

		public DeltaGInboundInterchangeImporter(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public static bool IsMenuItemVisible()
		{
			return GlbStaff.CurrentUser.IsSupportUser;
		}

		public MenuItem GetNewMenuItem()
		{
			var menuItem = new ZMenuItem((NoResString)DeltaGResponseInterchangeAddActionText, AddDeltaGInboundInterchange);
			return menuItem;
		}

		void AddDeltaGInboundInterchange(object sender, EventArgs e)
		{
			using (var stream = GetXmlFileStream())
			{
				if (stream != null)
				{
					try
					{
						var interchange = CreateDeltaGInterchange(stream);
						try
						{
							factory.Save();
							Globals.Message.ShowInformation(Res.GetString("DD62BB9E-B2DE-4C87-AE61-DB8ECD68E388", "A Delta G Inbound interchange was saved with interchange number {0}.", interchange.EI_InterchangeNum));
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

		EDIInterchange CreateDeltaGInterchange(Stream stream)
		{
			var messageContent = new StreamReader(stream).ReadToEnd();
			var result = factory.New<FRInterchange>();
			result.EI_ApplicationCode = EDIInterchange.ApplicationCodes.GenericMessageDelivery;
			result.EI_BodyText = messageContent;
			result.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			result.EI_Status = EDIInterchangeStatusList.Codes.Queued;
			result.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustoms;
			result.EI_From = FRCustomsDataRegistry.Instance.RecipientID.Value;
			result.EI_To = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
			return result;
		}

		protected virtual Stream GetXmlFileStream()
		{
			using (var openFileDialog = new ZOpenFileDialog())
			{
				openFileDialog.CheckFileExists = true;
				openFileDialog.DefaultExt = ".xml"; //May be a path or url.
				openFileDialog.Filter = (NoResString)"XML Message File (*.xml)|*.xml"; //May be a path or url.
				openFileDialog.Multiselect = false;
				return openFileDialog.ShowDialog() == DialogResult.OK ? openFileDialog.OpenFile() : null;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public const string DeltaGResponseInterchangeAddActionText = "Add Delta G Inbound Interchange (CWSupport Only)"; //CWSupport only Menu Item
	}
}
