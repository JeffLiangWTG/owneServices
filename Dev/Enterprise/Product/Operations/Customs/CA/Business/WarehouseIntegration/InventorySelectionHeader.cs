using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
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
			var caInvoiceLine = (JobComInvoiceLine)invoiceLine;

			if (!caInvoiceLine.CA_AMMVPerUnit.IsEmpty)
			{
				caInvoiceLine.JI_LinePrice = Round(caInvoiceLine.JI_LinePrice - (invoiceQuantity * caInvoiceLine.CA_AMMVPerUnit), 5);
			}
			else if (!caInvoiceLine.CA_AMMVPercentage.IsEmpty)
			{
				caInvoiceLine.JI_LinePrice = Round(caInvoiceLine.JI_LinePrice / (1 + caInvoiceLine.CA_AMMVPercentage / 100), 5);
			}

			PopulateCountrySpecificInvoiceLineData(caInvoiceLine, whsReceiveLine, whsBondedWarehouseAttribute, addInfos, ratio);
		}

		void PopulateCountrySpecificInvoiceLineData(JobComInvoiceLine invoiceLine, IWhsDocketLine whsReceiveLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute, Dictionary<ZString, ZString> addInfos, ZDecimal ratio)
		{
			using (invoiceLine.GetValidationSuspender())
			{
				PopulateCACustomsSecondThirdQuantityAndUnit(invoiceLine, whsBondedWarehouseAttribute, ratio);
				var entryKey = ZString.Empty;
				var entryLineNo = ZShort.Zero;
				if (whsBondedWarehouseAttribute != null)
				{
					invoiceLine.JI_PrimaryPreference = whsBondedWarehouseAttribute.WB_PrimaryPreference;
					invoiceLine.GetAddInfo().LoadPropertiesFromString(AddInfoParser.Serialise(addInfos));
					entryKey = whsBondedWarehouseAttribute.WB_EntryKey;
					entryLineNo = whsBondedWarehouseAttribute.WB_EntryLineNo;

					if (invoiceLine.JI_CustomsUnitQty.IsEmpty && !whsBondedWarehouseAttribute.WB_CustomsUnitOfQty.IsEmpty && !invoiceLine.JI_CustomsUnitQty_ReadOnly)
					{
						invoiceLine.JI_CustomsUnitQty = whsBondedWarehouseAttribute.WB_CustomsUnitOfQty;
						CalculateQtyByRatio(ratio, invoiceLine.JI_CustomsQuantityInfo, whsBondedWarehouseAttribute.WB_CustomsQty, 5);
					}
				}
				if (whsReceiveLine != null)
				{
					if (entryKey.IsEmpty)
					{
						var bondedEntryKey = whsReceiveLine.WE_BondedEntryKey;
						if (!bondedEntryKey.IsEmpty)
						{
							var separator = bondedEntryKey.IndexOf('-');
							if (separator > -1)
							{
								entryKey = bondedEntryKey.Left(separator);
								entryLineNo = ZShort.ParseSafe(bondedEntryKey.SubstringSafe(separator + 1), ZShort.Zero);
							}
						}
					}
				}

				invoiceLine.JI_PreviousEntryNumber = entryKey;
				invoiceLine.JI_PreviousEntryLineNumber = entryLineNo;

				if (invoiceLine.IsRemissionRepairLineIncludingParent)
				{
					invoiceLine.AddRemissionLine(invoiceLine.CA_CalculationMethod);
				}
			}
		}

		void PopulateCACustomsSecondThirdQuantityAndUnit(BaseJobComInvoiceLine invoiceLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute, ZDecimal ratio)
		{
			if (!invoiceLine.JI_CustomsSecondUnitQty.IsEmpty && !whsBondedWarehouseAttribute.WB_CustomsSecondUnitQty.IsEmpty && whsBondedWarehouseAttribute.WB_CustomsSecondUnitQty == invoiceLine.JI_CustomsSecondUnitQty)
			{
				CalculateQtyByRatio(ratio, invoiceLine.JI_CustomsSecondQuantityInfo, whsBondedWarehouseAttribute.WB_CustomsSecondQuantity, JobComInvoiceLineSchema.JI_CustomsSecondQuantity.Scale);
			}
			if (!invoiceLine.JI_CustomsThirdUnitQty.IsEmpty && !whsBondedWarehouseAttribute.WB_CustomsThirdUnitQty.IsEmpty && whsBondedWarehouseAttribute.WB_CustomsThirdUnitQty == invoiceLine.JI_CustomsThirdUnitQty)
			{
				CalculateQtyByRatio(ratio, invoiceLine.JI_CustomsThirdQuantityInfo, whsBondedWarehouseAttribute.WB_CustomsThirdQuantity, JobComInvoiceLineSchema.JI_CustomsThirdQuantity.Scale);
			}
		}

		protected override void UpdateOutwardLineWithInventoryDetailCore(BaseJobComInvoiceLine invoiceLine, IWhsInventoryView inventory, IWhsDocketLine whsReceiveLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute, ZDecimal ratio)
		{
			using (invoiceLine.GetNewLinePriceCalculationFieldSettingSupporter(LinePriceCalculationFieldSettingType.Quantity))
			{
				base.UpdateOutwardLineWithInventoryDetailCore(invoiceLine, inventory, whsReceiveLine, whsBondedWarehouseAttribute, ratio);
				var addInfos = PopulateAddInfoData(whsBondedWarehouseAttribute);
				SetHeaderData(invoiceLine, whsBondedWarehouseAttribute);
				PopulateCountrySpecificInvoiceLineData((JobComInvoiceLine)invoiceLine, whsReceiveLine, whsBondedWarehouseAttribute, addInfos, ratio);
			}
		}

		void CalculateQtyByRatio(ZDecimal ratio, ZPropertyInfo qtyInfo, ZDecimal qty, int scale)
		{
			if (!qty.IsEmpty)
			{
				qtyInfo.Value = Round(ratio * qty, scale);
			}
		}

		protected override ZString InwardEntryNumberHumanReadable => Res.GetString("9BD69A87-15E6-43F2-991E-F342F6381A4D", "Prev. Tran. #");

		protected override ZString InwardEntryLineNumberHumanReadable => Res.GetString("E33C1EA3-8951-4404-8C71-2317268C907D", "PTLN");

		protected override void ImportTearDown()
		{
			Declaration.ResumeApportionment();
		}

		protected override void SetHeaderData(BaseJobComInvoiceLine invoiceLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute)
		{
			base.SetHeaderData(invoiceLine, whsBondedWarehouseAttribute);
			if (invoiceLine.JI_CustomsSecondUnitQty.IsEmpty && !whsBondedWarehouseAttribute.WB_CustomsSecondUnitQty.IsEmpty)
			{
				invoiceLine.JI_CustomsSecondUnitQty = whsBondedWarehouseAttribute.WB_CustomsSecondUnitQty;
			}
			if (invoiceLine.JI_CustomsThirdUnitQty.IsEmpty && !whsBondedWarehouseAttribute.WB_CustomsThirdUnitQty.IsEmpty)
			{
				invoiceLine.JI_CustomsThirdUnitQty = whsBondedWarehouseAttribute.WB_CustomsThirdUnitQty;
			}
		}
	}
}
