
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class InventorySelectionHeader : DeclarationInventorySelectionHeader
	{
		public InventorySelectionHeader(JobDeclaration declaration)
			: base(declaration)
		{
			IsGroupByCartonSupported = false;
		}

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override void PopulateCountrySpecificInvoiceLineData(BaseJobComInvoiceLine invoiceLine, WhsInventoryWrapper inventoryWrapper, IWhsDocketLine whsReceiveLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute, Dictionary<ZString, ZString> addInfos, ZDecimal invoiceQuantity, ZDecimal ratio)
		{
			var auInvoiceLine = (JobComInvoiceLine)invoiceLine;
			var addInfo = auInvoiceLine.AddInfo;
			var hasAttribute = whsBondedWarehouseAttribute != null;
			if (hasAttribute)
			{
				addInfo.LoadPropertiesFromString(AddInfoParser.Serialise(addInfos));
				addInfo.ZA_WRL = whsBondedWarehouseAttribute.WB_EntryLineNo;
				addInfo.ZA_WRN = whsBondedWarehouseAttribute.WB_EntryKey;
				if (!addInfo.ZA_WRQ.IsEmpty)
				{
					addInfo.ZA_WRQ = Round(ratio * addInfo.ZA_WRQ, 5);
				}
				string tILVCurrencyCode = "";
				if (!whsBondedWarehouseAttribute.WB_RX_NKTILVCurrency.IsEmpty)
				{
					tILVCurrencyCode = whsBondedWarehouseAttribute.WB_RX_NKTILVCurrency;
				}
				addInfo.ZA_TILV = new ZDecimal(ratio * whsBondedWarehouseAttribute.WB_TILV).ToString(2) + tILVCurrencyCode;
			}
			addInfo.ZA_IsPackToBondForLine_Hidden = "N";
			addInfo.ZA_ADJ = "";
			addInfo.UseBondedWarehouseAutomation = true;

			if (hasAttribute)
			{
				if (auInvoiceLine.JI_CustomsUnitQty.IsEmpty && auInvoiceLine.JI_CustomsQuantity == 0m)
				{
					auInvoiceLine.JI_InvoiceQuantity = Round(ratio * whsBondedWarehouseAttribute.WB_BondedWhsQty, 5);
					auInvoiceLine.JI_InvoiceUQ = whsBondedWarehouseAttribute.WB_BondedWhsUnitOfQty;
				}

				if (HasWRQandWRU(addInfos) && auInvoiceLine.JI_CustomsQuantity > 0 && !auInvoiceLine.JI_CustomsUnitQty.IsEmpty)
				{
					addInfo.ZA_WRQ = Round(ratio * whsBondedWarehouseAttribute.WB_CustomsQty, 5);
					addInfo.ZA_WRU = whsBondedWarehouseAttribute.WB_CustomsUnitOfQty;
				}
			}
			auInvoiceLine.JI_BondedWarehouseLineKey = whsReceiveLine.PK;
		}

		bool HasWRQandWRU(Dictionary<ZString, ZString> addInfos)
		{
			return addInfos.ContainsKey("WRU") && addInfos.ContainsKey("WRQ");
		}

		protected override void UpdateOutwardLineWithInventoryDetailCore(BaseJobComInvoiceLine invoiceLine, IWhsInventoryView inventory, IWhsDocketLine whsReceiveLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute, ZDecimal ratio)
		{
			ZDecimal linePrice = ratio * (whsBondedWarehouseAttribute?.WB_ValueForDuty ?? ZDecimal.Zero);
			invoiceLine.JI_LinePrice = linePrice.Round(JobComInvoiceLineSchema.JI_LinePrice.Scale);
			var auInvoiceLine = (JobComInvoiceLine)invoiceLine;
			var addInfo = auInvoiceLine.AddInfo;
			var wrn = ZString.Empty;
			var wrl = ZShort.Zero;
			var iss = addInfo.ZA_ISS;
			var packType = whsReceiveLine.WE_F3_NKPackType;
			if (invoiceLine.JI_InvoiceUQ != packType)
			{
				invoiceLine.JI_InvoiceUQ = packType;
			}
			if (whsBondedWarehouseAttribute != null)
			{
				if (!invoiceLine.JI_CustomsQuantity_ReadOnly && !whsBondedWarehouseAttribute.WB_CustomsUnitOfQty.IsEmpty && whsBondedWarehouseAttribute.WB_CustomsUnitOfQty == invoiceLine.JI_CustomsUnitQty)
				{
					ZDecimal customsQty = ratio * whsBondedWarehouseAttribute.WB_CustomsQty;
					invoiceLine.JI_CustomsQuantity = customsQty.Round(JobComInvoiceLineSchema.JI_CustomsQuantity.Scale);
				}

				var addInfos = PopulateAddInfoData(whsBondedWarehouseAttribute);
				addInfo.LoadPropertiesFromString(AddInfoParser.Serialise(addInfos));
				addInfo.ZA_ISS = iss;
				wrn = whsBondedWarehouseAttribute.WB_EntryKey;
				wrl = whsBondedWarehouseAttribute.WB_EntryLineNo;
				if (!addInfo.ZA_WRQ.IsEmpty)
				{
					addInfo.ZA_WRQ = Round(ratio * addInfo.ZA_WRQ, 5);
				}
				if (!addInfo.ZA_QT2.IsEmpty)
				{
					addInfo.ZA_QT2 = Round(ratio * addInfo.ZA_QT2, 5);
				}
				addInfo.ZA_TILV = new ZDecimal(ratio * whsBondedWarehouseAttribute.WB_TILV).ToString(2) + whsBondedWarehouseAttribute.WB_RX_NKTILVCurrency;
				addInfo.ZA_IsPackToBondForLine_Hidden = "N";
				addInfo.ZA_ADJ = "";
				if (HasWRQandWRU(addInfos) && auInvoiceLine.JI_CustomsQuantity > 0 && !auInvoiceLine.JI_CustomsUnitQty.IsEmpty)
				{
					addInfo.ZA_WRQ = Round(ratio * whsBondedWarehouseAttribute.WB_CustomsQty, 5);
					addInfo.ZA_WRU = whsBondedWarehouseAttribute.WB_CustomsUnitOfQty;
				}
				var countryOfOrigin = whsBondedWarehouseAttribute.WB_RN_NKCountryOfOrigin;
				if (!countryOfOrigin.IsEmpty && invoiceLine.JI_CountryOfOrigin != countryOfOrigin)
				{
					invoiceLine.JI_CountryOfOrigin = countryOfOrigin;
				}
			}
			addInfo.UseBondedWarehouseAutomation = true;
			auInvoiceLine.JI_BondedWarehouseLineKey = whsReceiveLine.PK;
			if (wrn.IsEmpty)
			{
				var bondedEntryKey = whsReceiveLine.WE_BondedEntryKey;
				if (!bondedEntryKey.IsEmpty)
				{
					var separator = bondedEntryKey.IndexOf('-');
					if (separator > -1)
					{
						wrn = bondedEntryKey.Left(separator);
						wrl = ZShort.ParseSafe(bondedEntryKey.SubstringSafe(separator + 1), ZShort.Zero);
					}
				}
			}

			addInfo.ZA_WRN = wrn;
			addInfo.ZA_WRL = wrl;
		}

		protected override ZString GetInwardEntryNumber(BaseJobComInvoiceLine invoiceLine) => ((JobComInvoiceLine)invoiceLine).AddInfo.ZA_WRN;

		protected override ZInt GetInwardEntryLineNumber(BaseJobComInvoiceLine invoiceLine) => ((JobComInvoiceLine)invoiceLine).AddInfo.ZA_WRL;

		protected override ZString InwardEntryNumberHumanReadable => Res.GetString("5D3A97E9-11E9-43FD-BB75-101EDF089759", "WRN");

		protected override ZString InwardEntryLineNumberHumanReadable => Res.GetString("C6E87235-C066-4CD5-BD89-BB307CB4B9BC", "WRL");
	}
}
