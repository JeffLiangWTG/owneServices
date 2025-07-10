using System.Collections.Specialized;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS460;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.MessageProcessors;
using static Enterprise.Messaging.MessageProcessors.HtmlTreeCreator;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IETS460MessagePrettier : PNTSMessagePrettier<Iets460>
	{
		public IETS460MessagePrettier(IETS460MessageDataObject messageDataObject) : base(messageDataObject)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Core strings")]
		protected override ZString GetMessageInterpretationCore(Iets460 messageObject)
		{
			var result = ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				("Status", MessageDataObject.GetCustomsStatusDescriptionFromMessage()),
				("CRN", messageObject.Crn),
				("MRN", messageObject.Mrn),
				("Notification Date", messageObject.NotificationDate.ToString()),
				("Scheduled Control Date", messageObject.ScheduledControlDate.ToString()),
				("Customs Office Of Control", messageObject.CustomsOfficeOfControl?.ReferenceNumber),
			}, true);

			if (messageObject.TypeOfControls != null && messageObject.TypeOfControls.Count > 0)
			{
				var tableCreator = GetHtmlTableCreator("70%");
				var cell1 = new CellWithFormatting("Type", "width", "50%");
				var cell2 = new CellWithFormatting("Description", "width", "50%");
				tableCreator.WriteRow(new NameValueCollection { { "align", "center" } }, cell1, cell2);
				foreach (var typeOfControl in messageObject.TypeOfControls)
				{
					tableCreator.WriteRow(new NameValueCollection { { "align", "center" } }, $"{typeOfControl.Type} ({new PNTSControlTypes()[typeOfControl.Type]?.Description})", typeOfControl.Text);
				}
				result += ToTableSection("Control Type", tableCreator);
			}

			if (messageObject.RequestedDocument != null && messageObject.RequestedDocument.Count > 0)
			{
				var tableCreator = GetHtmlTableCreator("70%");
				var cell1 = new CellWithFormatting("Document Type", "width", "20%");
				var cell2 = new CellWithFormatting("Description", "width", "80%");
				tableCreator.WriteRow(new NameValueCollection { { "align", "center" } }, cell1, cell2);
				foreach (var requestedDocument in messageObject.RequestedDocument)
				{
					tableCreator.WriteRow(new NameValueCollection { { "align", "center" } }, requestedDocument.DocumentType, requestedDocument.Description);
				}
				result += ToTableSection("Requested Document", tableCreator);
			}

			if (messageObject.Control != null && messageObject.Control.Count > 0)
			{
				result += ToStrongIfNotEmpty("Control Data - Master");
				foreach (var control in messageObject.Control)
				{
					var consignmentMasterLevelControl = control.ControlSubject?.ConsignmentMasterLevel;
					if (consignmentMasterLevelControl != null)
					{
						var controlContent = ZString.Empty;
						var root = new TreeNode
						{
							Text = ToStrongIfNotEmpty("Reference Number: ") + consignmentMasterLevelControl.TransportDocument?.DocumentNumber,
							Children =
							{
								new TreeNode { Text = ToStrongIfNotEmpty("Type: ") + ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(MessageDataObject.Factory, consignmentMasterLevelControl.TransportDocument?.Type ?? ZString.Empty, Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TransportDocumentTemporaryStorage, ZDateTime.Today)?.ZZD_Description ?? ZString.Empty },
								GetMasterLevelTransportEquipmentTreeNodeIfExists(consignmentMasterLevelControl),
								GetMasterLevelGoodsItemTreeNodeIfExists(consignmentMasterLevelControl),
								GetConsignmentHouseLevelTreeNodeIfExists(consignmentMasterLevelControl),
							}
						};

						result += new HtmlTreeCreator(root).ToHtml();
					}
				}
			}

			return result;

			TreeNode GetMasterLevelTransportEquipmentTreeNodeIfExists(Iets460ControlControlSubjectConsignmentMasterLevel consignmentMasterLevelControl)
			{
				TreeNode node = null;
				if (consignmentMasterLevelControl.TransportEquipment != null && consignmentMasterLevelControl.TransportEquipment.Count > 0)
				{
					node = new TreeNode
					{
						Text = ToStrongIfNotEmpty("Transport Equipment"),
						Children =
						{
							new TreeNode { Text = ToStrongIfNotEmpty("Container: ") + string.Join(",", consignmentMasterLevelControl.TransportEquipment.Select(transportEquipment => transportEquipment.ContainerIdentificationNumber).ToArray()) },
						}
					};
				}
				return node;
			}

			TreeNode GetMasterLevelGoodsItemTreeNodeIfExists(Iets460ControlControlSubjectConsignmentMasterLevel consignmentMasterLevelControl)
			{
				TreeNode node = null;
				if (consignmentMasterLevelControl.GoodsItem != null && consignmentMasterLevelControl.GoodsItem.Count > 0)
				{
					node = new TreeNode
					{
						Text = ToStrongIfNotEmpty("Goods Item"),
						Children = consignmentMasterLevelControl.GoodsItem.Select(goodsItem => GetGoodsItemTreeNode(goodsItem)).ToList(),
					};
				}
				return node;
			}

			TreeNode GetGoodsItemTreeNode(Iets460ControlControlSubjectConsignmentMasterLevelGoodsItem goodsItem)
			{
				var node = new TreeNode { Text = ToStrongIfNotEmpty("Goods Item Number: ") + goodsItem.GoodsItemNumber };
				if (goodsItem.Packaging != null && goodsItem.Packaging.Count > 0)
				{
					var tableCreator = GetHtmlTableCreator("70%");
					var cell1 = new CellWithFormatting("ShippingMarks", "width", "50%");
					var cell2 = new CellWithFormatting("Type Of Package", "width", "50%");
					tableCreator.WriteRow(new NameValueCollection { { "align", "center" } }, cell1, cell2);
					foreach (var packing in goodsItem.Packaging)
					{
						tableCreator.WriteRow(new NameValueCollection { { "align", "center" } }, packing.ShippingMarks, packing.TypeOfPackages);
					}
					var packingNode = new TreeNode { Text = ToStrongIfNotEmpty("Packing: ") + ToTableSection("", tableCreator, false) };
					node.Children.Add(packingNode);
				}

				if (goodsItem.TransportEquipment != null && goodsItem.TransportEquipment.Count > 0)
				{
					var transportEquipmentNode = new TreeNode
					{
						Text = ToStrongIfNotEmpty("Transport Equipment"),
						Children =
							{
								new TreeNode() { Text = ToStrongIfNotEmpty("Container: ") + string.Join(",", goodsItem.TransportEquipment.Select(transportEquipment => transportEquipment.ContainerIdentificationNumber).ToArray()) }
							}
					};
					node.Children.Add(transportEquipmentNode);
				}
				return node;
			}

			TreeNode GetConsignmentHouseLevelTreeNodeIfExists(Iets460ControlControlSubjectConsignmentMasterLevel consignmentMasterLevelControl)
			{
				TreeNode node = null;
				if (consignmentMasterLevelControl.ConsignmentHouseLevel != null && consignmentMasterLevelControl.ConsignmentHouseLevel.Count > 0)
				{
					node = new TreeNode
					{
						Text = ToStrongIfNotEmpty("Consignment House"),
						Children = consignmentMasterLevelControl.ConsignmentHouseLevel.Select(consignmentHouseLevelControl => GetConsignmentHouseLevelControlTreeNode(consignmentHouseLevelControl)).ToList(),
					};
				}
				return node;
			}

			TreeNode GetConsignmentHouseLevelControlTreeNode(Iets460ControlControlSubjectConsignmentMasterLevelConsignmentHouseLevel consignmentHouseLevelControl)
			{
				var node = new TreeNode
				{
					Text = ToStrongIfNotEmpty("Reference Number: ") + consignmentHouseLevelControl.TransportDocument?.DocumentNumber,
					Children =
					{
						new TreeNode { Text = ToStrongIfNotEmpty("Type: ") + ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(MessageDataObject.Factory, consignmentHouseLevelControl.TransportDocument?.Type ?? ZString.Empty, Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TransportDocumentTemporaryStorage, ZDateTime.Today)?.ZZD_Description ?? ZString.Empty },
						GetHouseLevelTransportEquipmentTreeNodeIfExists(consignmentHouseLevelControl),
						GetHouseLevelGoodsItemTreeNodeIfExists(consignmentHouseLevelControl),
					}
				};
				return node;
			}

			TreeNode GetHouseLevelTransportEquipmentTreeNodeIfExists(Iets460ControlControlSubjectConsignmentMasterLevelConsignmentHouseLevel consignmentHouseLevelControl)
			{
				TreeNode node = null;
				if (consignmentHouseLevelControl.TransportEquipment != null && consignmentHouseLevelControl.TransportEquipment.Count > 0)
				{
					node = new TreeNode
					{
						Text = ToStrongIfNotEmpty("Transport Equipment"),
						Children =
						{
							new TreeNode { Text = ToStrongIfNotEmpty("Container: ") + string.Join(",", consignmentHouseLevelControl.TransportEquipment.Select(transportEquipment => transportEquipment.ContainerIdentificationNumber).ToArray()) },
						}
					};
				}
				return node;
			}

			TreeNode GetHouseLevelGoodsItemTreeNodeIfExists(Iets460ControlControlSubjectConsignmentMasterLevelConsignmentHouseLevel consignmentHouseLevelControl)
			{
				TreeNode node = null;
				if (consignmentHouseLevelControl.GoodsItem != null && consignmentHouseLevelControl.GoodsItem.Count > 0)
				{
					node = new TreeNode
					{
						Text = ToStrongIfNotEmpty("Goods Item"),
						Children = consignmentHouseLevelControl.GoodsItem.Select(goodsItem => GetConsignmentHouseLevelGoodsItemTreeNode(goodsItem)).ToList(),
					};
				}
				return node;
			}

			TreeNode GetConsignmentHouseLevelGoodsItemTreeNode(Iets460ControlControlSubjectConsignmentMasterLevelConsignmentHouseLevelGoodsItem goodsItem)
			{
				var node = new TreeNode { Text = ToStrongIfNotEmpty("Goods Item Number: ") + goodsItem.GoodsItemNumber };
				if (goodsItem.Packaging != null && goodsItem.Packaging.Count > 0)
				{
					var tableCreator = GetHtmlTableCreator("70%");
					var cell1 = new CellWithFormatting("ShippingMarks", "width", "50%");
					var cell2 = new CellWithFormatting("Type Of Package", "width", "50%");
					tableCreator.WriteRow(new NameValueCollection { { "align", "center" } }, cell1, cell2);
					foreach (var packing in goodsItem.Packaging)
					{
						tableCreator.WriteRow(new NameValueCollection { { "align", "center" } }, packing.ShippingMarks, packing.TypeOfPackages);
					}
					var packingNode = new TreeNode { Text = ToStrongIfNotEmpty("Packing: ") + ToTableSection("", tableCreator, false) };
					node.Children.Add(packingNode);
				}

				if (goodsItem.TransportEquipment != null && goodsItem.TransportEquipment.Count > 0)
				{
					var transportEquipmentNode = new TreeNode
					{
						Text = ToStrongIfNotEmpty("Transport Equipment"),
						Children =
							{
								new TreeNode() { Text = ToStrongIfNotEmpty("Container: ") + string.Join(",", goodsItem.TransportEquipment.Select(transportEquipment => transportEquipment.ContainerIdentificationNumber).ToArray()) }
							}
					};
					node.Children.Add(transportEquipmentNode);
				}
				return node;
			}
		}
	}
}
