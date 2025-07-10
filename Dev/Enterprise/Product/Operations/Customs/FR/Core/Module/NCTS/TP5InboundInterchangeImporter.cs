using System;
using System.IO;
using System.Text.Json;
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

namespace Enterprise.Customs.FR.Module.NCTS
{
	public class TP5InboundInterchangeImporter
	{
		readonly BusinessObjectFactory factory;

		public TP5InboundInterchangeImporter(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public static bool IsMenuItemVisible()
		{
			return GlbStaff.CurrentUser.IsSupportUser;
		}

		public MenuItem GetNewMenuItem() => new ZMenuItem((NoResString)TP5ResponseInterchangeAddActionText, AddTP5InboundInterchange);

		void AddTP5InboundInterchange(object sender, EventArgs e)
		{
			using (var stream = GetXmlFileStream())
			{
				if (stream != null)
				{
					try
					{
						var interchange = CreateTP5Interchange(stream);
						try
						{
							factory.Save();
							Globals.Message.ShowInformation(Res.GetString("631B1448-48F6-4BA9-800B-993AF136BDF4", "A TP5 interchange was saved with interchange number {0}.", interchange.EI_InterchangeNum));
						}
						catch (ZSaveException exception)
						{
							ZExceptionReporting.HandleSaveException(exception);
						}
					}
					catch (JsonException exception)
					{
						Globals.Message.ShowError(exception.Message);
					}
				}
			}
		}

		EDIInterchange CreateTP5Interchange(Stream stream)
		{
			var messageContent = new StreamReader(stream).ReadToEnd();
			var result = factory.New<FRInterchange>();
			result.EI_ApplicationCode = EDIInterchange.ApplicationCodes.GenericMessageDelivery;
			result.EI_BodyText = messageContent;
			result.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			result.EI_Status = EDIInterchangeStatusList.Codes.Queued;
			result.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsTP5;
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "CWSupport only Menu Item")]
		public const string TP5ResponseInterchangeAddActionText = "Add TP5 Inbound Interchange (CWSupport Only)";
	}
}
