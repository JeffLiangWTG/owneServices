using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public class UPEDocumentMenuItemLoader : BusinessObject.Loader
	{
		public UPEDocumentMenuItemLoader(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public const string PortraitEntryPrintMenuName = "Entry Print (Portrait)";

		public DocumentCommand LoadPortraitEntryPrint()
		{
			return LoadJobDeclarationMenuItem(PortraitEntryPrintMenuName, false);
		}

		#region Tax Invoice And Commercial Invoice

		public const string UPSTaxInvoiceAndCommercialInvoiceMenuName = "Tax + Commercial Invoice";
		public const string UPSTaxInvoiceAndCommercialInvoiceQueueForBatchPrintMenuName = "Tax + Commercial Inv. - Queue for Print";

		public DocumentCommand LoadUPSTaxInvoiceAndCommercialInvoice()
		{
			return LoadAirCargoMenuItem(UPSTaxInvoiceAndCommercialInvoiceMenuName);
		}

		public DocumentCommand LoadUPSTaxInvoiceAndCommercialInvoiceQueueForBatchPrint()
		{
			return LoadAirCargoMenuItem(UPSTaxInvoiceAndCommercialInvoiceQueueForBatchPrintMenuName);
		}

		#endregion

		#region Tax Invoice

		public const string UPSTaxInvoiceMenuName = "Tax Invoice";
		public const string UPSTaxInvoiceQueueForBatchPrintMenuName = "Tax Invoice - Queue for Print";

		public DocumentCommand LoadUPSTaxInvoice()
		{
			return LoadAirCargoMenuItem(UPSTaxInvoiceMenuName);
		}

		public DocumentCommand LoadUPSTaxInvoiceQueueForBatchPrint()
		{
			return LoadAirCargoMenuItem(UPSTaxInvoiceQueueForBatchPrintMenuName);
		}

		#endregion

		#region Alternate Broker

		public const string AlternateBrokerDocumentPackMenuName = "Alternate Broker Documents";
		public const string AlternateBrokerSplitNotificationMenuName = "Alternate Broker Split Notification";

		public DocumentCommand LoadAlternateBrokerDocumentPack()
		{
			return LoadJobDeclarationMenuItem(AlternateBrokerDocumentPackMenuName, true);
		}

		public DocumentCommand LoadAlternateBrokerSplitNotification()
		{
			return LoadAirCargoMenuItem(AlternateBrokerSplitNotificationMenuName);
		}

		#endregion

		#region Letter of Authority

		public const string LetterOfAuthorityMenuName = "Letter of Authority";

		public DocumentCommand LoadLetterOfAuthority()
		{
			return LoadOrgHeaderMenuItem(LetterOfAuthorityMenuName);
		}

		#endregion

		#region Shipment Held Letter

		public const string ShipmentHeldLetterForConsigneeMenuName = "Consignee Customer Notification";
		public const string ShipmentHeldLetterForConsignorMenuName = "Consignor Customer Notification";

		public static bool IsShipmentHeldLetter(IStmMenuItem menuItem)
		{
			return
				menuItem.SU_MenuName == UPEDocumentMenuItemLoader.ShipmentHeldLetterForConsigneeMenuName ||
				menuItem.SU_MenuName == UPEDocumentMenuItemLoader.ShipmentHeldLetterForConsignorMenuName;
		}

		public DocumentCommand LoadCusHAWBHeldLetter(ShipmentHeldLetterRecipient recipient)
		{
			return LoadAirCargoMenuItem(recipient == ShipmentHeldLetterRecipient.Consignee ? ShipmentHeldLetterForConsigneeMenuName : ShipmentHeldLetterForConsignorMenuName, "UPECusHAWB");
		}

		public DocumentCommand LoadJobDeclarationHeldLetter(ShipmentHeldLetterRecipient recipient)
		{
			ZString menuName = recipient == ShipmentHeldLetterRecipient.Consignee ? ShipmentHeldLetterForConsigneeMenuName : ShipmentHeldLetterForConsignorMenuName;
			return LoadMenuItem(menuName, BusinessContext.Customs, "");
		}

		public DocumentCommand LoadFinanceHeldLetter(ShipmentHeldLetterRecipient recipient)
		{
			return LoadAirCargoMenuItem(recipient == ShipmentHeldLetterRecipient.Consignee ? ShipmentHeldLetterForConsigneeMenuName : ShipmentHeldLetterForConsignorMenuName, "UPECallout");
		}

		#endregion

		#region Electronic Credit Note

		public const string ElectronicCreditNote = "Electronic Credit Note";

		public DocumentCommand LoadElectronicCreditNote()
		{
			return LoadJobDeclarationMenuItem(ElectronicCreditNote, true);
		}

		#endregion

		#region Implementation

		DocumentCommand LoadAirCargoMenuItem(ZString menuItemName)
		{
			return LoadAirCargoMenuItem(menuItemName, "");
		}

		DocumentCommand LoadAirCargoMenuItem(ZString menuItemName, ZString filterListContains)
		{
			return LoadMenuItem(menuItemName, BusinessContext.CusHAWB, filterListContains);
		}

		DocumentCommand LoadOrgHeaderMenuItem(ZString menuItemName)
		{
			return LoadMenuItem(menuItemName, BusinessContext.Organisation, "");
		}

		DocumentCommand LoadJobDeclarationMenuItem(ZString menuItemName, ZBool isClientSpecific)
		{
			return LoadMenuItem(menuItemName, BusinessContext.Customs, "", isClientSpecific);
		}

		DocumentCommand LoadMenuItem(ZString menuItemName, BusinessContext context, ZString filterListContains)
		{
			return LoadMenuItem(menuItemName, context, filterListContains, true);
		}

		DocumentCommand LoadMenuItem(ZString menuItemName, BusinessContext context, ZString filterListContains, ZBool isClientSpecific)
		{
			DocumentZQuery filter = new DocumentZQuery(context, menuItemName);
			filter.AddToFilter(StmMenuItemSchema.SU_IsSystemDefined, SQLComparisonOperator.Equal, ZBool.True);
			filter.AddToFilter(StmMenuItemSchema.SU_IsClientSpecific, SQLComparisonOperator.Equal, isClientSpecific);
			filter.AddToFilter(StmMenuItemSchema.SU_FilterList, SQLComparisonOperator.Contains, filterListContains);
			return (DocumentCommand)Factory.LoadTop1(typeof(DocumentCommand), filter);
		}

		protected override Type GetTypeOfBusinessObjectToLoad()
		{
			return typeof(DocumentCommand);
		}

		#endregion
	}
}
