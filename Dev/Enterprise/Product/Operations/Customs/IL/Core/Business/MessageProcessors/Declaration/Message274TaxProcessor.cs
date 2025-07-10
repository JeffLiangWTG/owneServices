using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Business.MessageProcessors
{
	sealed class Message274TaxProcessor
	{
		public Message274TaxProcessor(CusEntryHeader linkedEntryHeader)
		{
			this.linkedEntryHeader = Argument.NotNull(linkedEntryHeader, nameof(linkedEntryHeader));
		}

		public bool DoEntryLineKeysMatch(Collection<DeclarationGoodsShipment> responseGoodsShipmentList)
		{
			responseGoodsShipmentList = responseGoodsShipmentList ?? new Collection<DeclarationGoodsShipment>();
			var responseGoodsItems = GetCustomsGoodsItemsDictionary(responseGoodsShipmentList);
			var responseGoodsItemSortedKeys = responseGoodsItems.Keys.OrderBy(l => l).ToList();
			var entryLineSortedKeys = linkedEntryHeader.MergedLines.Select(el => el.CL_LineNumber).OrderBy(l => l).ToList();
			return AreKeysMatching(responseGoodsItemSortedKeys, entryLineSortedKeys);
		}

		public void UpdateEntryLineConfirmedFees(Collection<DeclarationGoodsShipment> responseGoodsShipmentList)
		{
			responseGoodsShipmentList = responseGoodsShipmentList ?? new Collection<DeclarationGoodsShipment>();
			var responseGoodsItems = GetCustomsGoodsItemsDictionary(responseGoodsShipmentList);

			foreach (var entryLine in linkedEntryHeader.MergedLines)
			{
				var entryLineNumber = entryLine.CL_LineNumber;
				var responseGoodsItem = responseGoodsItems[entryLineNumber];
				UpdateEntryLineFees(responseGoodsItem, entryLine);
			}
		}

		internal void UpdateConfirmedCharges(Collection<DeclarationDutyTaxFee> declarationDutyTaxFeeList)
		{
			linkedEntryHeader.ConfirmedCharges.RemoveAndDeleteAll();
			if (declarationDutyTaxFeeList == null)
			{
				return;
			}

			foreach (var typeCodeGrouping in declarationDutyTaxFeeList.GroupBy(r => r.TypeCode?.Value))
			{
				var chargeType = typeCodeGrouping.Key;
				if (chargeType.IsNullOrEmpty())
				{
					continue;
				}

				var amount = typeCodeGrouping.Sum(r => r.DmExtensions?.CalculatedTax?.Amount?.Value ?? ZDecimal.Zero);
				var newCharge = linkedEntryHeader.ConfirmedCharges.AddNew();
				newCharge.C1_Source = CusEntryHeaderChargesSourceCodeList.Codes.CUS;
				newCharge.C1_ChargeType = chargeType;
				newCharge.C1_ChargeAmount = (ZDecimal)amount;
				newCharge.C1_IsLandedCostOnly = false;
			}
		}

		static void UpdateEntryLineFees(DeclarationGoodsShipmentGovernmentAgencyGoodsItem responseGoodsItem, CusEntryLine entryLine)
		{
			var fees = entryLine.ConfirmedFees.Cast<CusEntryLineFee>();
			var dutyTaxFees = responseGoodsItem.Commodity?.DutyTaxFee?.Where(dtf => (bool)!dtf.TypeCode?.Value.IsNullOrEmpty());

			entryLine.ConfirmedFees.RemoveAndDeleteAll();

			foreach (var typeCodeGrouping in dutyTaxFees?.GroupBy(r => r.TypeCode?.Value))
			{
				var chargeType = typeCodeGrouping.Key;
				var entryLineFee = (CusEntryLineFee)entryLine.ConfirmedFees.AddNew();
				entryLineFee.CF_ChargeType = chargeType;

				var dutyTaxFee = typeCodeGrouping.First();
				entryLineFee.CF_BaseValue = dutyTaxFee.AdValoremTaxBaseAmount?.Value ?? ZDecimal.Zero;
				entryLineFee.CF_ChargeAmount = typeCodeGrouping.Sum(r => r.DmExtensions?.CalculatedTax?.Amount?.Value ?? ZDecimal.Zero);
				entryLineFee.CF_MethodOfCalculation = Constants.MethodOfCalculationTypes.Percentage;
				entryLineFee.CF_Rate = dutyTaxFee.TaxRate ?? ZDecimal.Zero;
			}
		}

		static bool AreKeysMatching(List<ZShort> customsGoodItemSortedKeys, List<ZShort> entryLineSortedKeys)
		{
			if (customsGoodItemSortedKeys.Count != entryLineSortedKeys.Count)
			{
				return false;
			}

			for (var i = 0; i < customsGoodItemSortedKeys.Count; i++)
			{
				if (!customsGoodItemSortedKeys[i].Equals(entryLineSortedKeys[i]))
				{
					return false;
				}
			}

			return true;
		}

		static Dictionary<ZShort, DeclarationGoodsShipmentGovernmentAgencyGoodsItem> GetCustomsGoodsItemsDictionary(Collection<DeclarationGoodsShipment> responseGoodsShipmentList)
		{
			var customsGoodsItemsDictionary = new Dictionary<ZShort, DeclarationGoodsShipmentGovernmentAgencyGoodsItem>();
			foreach (var goodsShipment in responseGoodsShipmentList.Where(x => x.SequenceNumeric is not null))
			{
				foreach (var goodsItem in goodsShipment.GovernmentAgencyGoodsItem.Where(r => r.SequenceNumeric is not null))
				{
					var itemSequenceNumber = (ZShort)goodsItem.SequenceNumeric;
					customsGoodsItemsDictionary.Add(itemSequenceNumber, goodsItem);
				}
			}

			return customsGoodsItemsDictionary;
		}

		readonly CusEntryHeader linkedEntryHeader;
	}
}
