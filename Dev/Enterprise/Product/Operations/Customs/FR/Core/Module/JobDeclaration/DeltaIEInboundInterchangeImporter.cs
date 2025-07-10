using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;
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
	public class DeltaIEInboundInterchangeImporter
	{
		readonly BusinessObjectFactory factory;

		public DeltaIEInboundInterchangeImporter(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public static bool IsMenuItemVisible()
		{
			return GlbStaff.CurrentUser.IsSupportUser;
		}

		public MenuItem GetNewMenuItem()
		{
			var menuItem = new ZMenuItem((NoResString)DeltaIEResponseInterchangeAddActionText);
			var messageSubTypeCodeList = new DeltaIEResponseMessageSubTypeList();
			foreach (ICodeDescription pair in messageSubTypeCodeList)
			{
				var schemaId = DeltaIEMessageSubTypeAndSchemaIdConverter.ConvertMessageSubTypeToSchemaId(pair.Code);
				var subMenuItem = menuItem.MenuItems.Add($"{schemaId} ({pair.Description})", AddDeltaIEInboundInterchange);
				subMenuItem.Tag = schemaId;
			}
			return menuItem;
		}

		void AddDeltaIEInboundInterchange(object sender, EventArgs e)
		{
			var schemaId = ZString.Empty;
			if (sender is MenuItem menuItem)
			{
				schemaId = (ZString)menuItem.Tag;
			}
			else if (sender is ZToolStripMenuItem toolStripMenuItem)
			{
				schemaId = (ZString)toolStripMenuItem.Tag;
			}
			if (!schemaId.IsEmpty)
			{
				using (var stream = GetJsonFileStream())
				{
					if (stream != null)
					{
						try
						{
							var interchange = CreateDeltaIEInterchange(stream, schemaId);
							try
							{
								factory.Save();
								Globals.Message.ShowInformation(Res.GetString("ADCEBA62-CF7E-4C89-A34F-475B11F4FC92", "An {0} interchange was saved with interchange number {1}.", schemaId, interchange.EI_InterchangeNum));
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
		}

		EDIInterchange CreateDeltaIEInterchange(Stream stream, string schemaId)
		{
			var messageContent = new StreamReader(stream).ReadToEnd();
			var messageEnvelope = new DeltaIEMessageEnvelope
			{
				SchemaId = schemaId,
				MessageJson = messageContent
			};

			var result = factory.New<FRInterchange>();
			result.EI_ApplicationCode = EDIInterchange.ApplicationCodes.GenericMessageDelivery;
			result.EI_BodyText = JsonSerializer.Serialize(messageEnvelope, SerializeOptions);
			result.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			result.EI_Status = EDIInterchange.Status.Queued;
			result.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsDeltaIE;
			result.EI_From = FRCustomsDataRegistry.Instance.RecipientID.Value;
			result.EI_To = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
			return result;
		}

		protected virtual Stream GetJsonFileStream()
		{
			using (var openFileDialog = new ZOpenFileDialog())
			{
				openFileDialog.CheckFileExists = true;
				openFileDialog.DefaultExt = ".json";
				openFileDialog.Filter = (NoResString)"JSON Message File (*.json)|*.json";
				openFileDialog.Multiselect = false;
				return openFileDialog.ShowDialog() == DialogResult.OK ? openFileDialog.OpenFile() : null;
			}
		}

		static readonly JsonSerializerOptions SerializeOptions = new JsonSerializerOptions { WriteIndented = true };

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "CWSupport only Menu Item")]
		public const string DeltaIEResponseInterchangeAddActionText = "Add Delta I/E Inbound Interchange (CWSupport Only)";
	}
}
