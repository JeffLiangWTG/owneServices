using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.Uruguay
{
	public interface IItemsBuilder
	{
		Item_Det_Fact[] BuildItems(TransactionInfo transaction);
	}

	class ItemsBuilder : IItemsBuilder
	{
		Item_Det_Fact[] IItemsBuilder.BuildItems(TransactionInfo transaction)
		{
			var items = new List<Item_Det_Fact>();

			if (transaction.TransactionType.HasValue)
			{
				int nroLinDet = 1;

				foreach (var line in transaction.PostingJournalCollection)
				{
					var taxTypeExcludedFromTheTaxBase = (line.VATTaxID?.TaxType?.Code ?? string.Empty) == AccTaxRate.Types.ExcludedFromTheTaxBase;
					var montoItem = GetValueToReport(line.OSAmount ?? 0, transaction.TransactionType == TransactionType.CRD);
					montoItem = taxTypeExcludedFromTheTaxBase ? Math.Abs(montoItem) : montoItem;
					var indFact = GetIndFact(line, transaction.TransactionType.Value);
					var description = line.Description?.Substring(0, 80);
					var taxMessage = line.TaxMessageID?.EnglishTaxMessage;

					if (indFact.HasValue)
					{
						AddItem(montoItem, nroLinDet++, description, indFact.Value, taxMessage);

						var percents = GetPercentsCalculate(line.VATTaxID?.TaxCode.ToString());

						if (percents.percent1.HasValue)
						{
							var lastItem = items.Last();
							lastItem.MontoItem *= percents.percent1.Value;
							lastItem.PrecioUnitario *= percents.percent1.Value;

							AddItem(montoItem * percents.percent2.Value, nroLinDet++, description, Item_Det_FactIndFact.Item3, taxMessage);
						}
					}
				}
			}

			return items.Count != 0 ? items.ToArray() : null;

			void AddItem(decimal montoItem, int nroLinDet, string description, Item_Det_FactIndFact item, string taxMessage)
			{
				var cantidad = montoItem >= 0 ? 1 : -1;
				var dscItem = !string.IsNullOrEmpty(taxMessage) ? "{ " + taxMessage + " }" : null;

				items.Add(new Item_Det_Fact()
				{
					MontoItem = montoItem,
					UniMed = "N/A",
					PrecioUnitario = Math.Abs(montoItem),
					Cantidad = cantidad,
					NroLinDet = nroLinDet.ToString(),
					IndFact = item,
					NomItem = description,
					DscItem = dscItem
				});
			}
		}

		static (decimal? percent1, decimal? percent2) GetPercentsCalculate(string taxCode)
		{
			switch (taxCode)
			{
				case UruguayConstants.IVA66:
					return (0.97m, 0.03m);
				case UruguayConstants.IVA77:
					return (0.965m, 0.035m);
				case UruguayConstants.IVA396:
					return (0.982m, 0.018m);
			}
			return (null, null);
		}

		decimal GetValueToReport(decimal value, bool changeSign)
		{
			return (changeSign) ? value * -1 : value;
		}

		Item_Det_FactIndFact? GetIndFact(PostingJournal line, TransactionType transactionType)
		{
			var taxType = line.VATTaxID?.TaxType?.Code;
			var taxCode = line.VATTaxID?.TaxCode;

			if (taxType.HasValue)
			{
				if (taxType.Value == AccTaxRate.Types.Exempt || taxType.Value == AccTaxRate.Types.NotReportable)
				{
					return Item_Det_FactIndFact.Item1;
				}
				else if (taxType.Value == AccTaxRate.Types.ExcludedFromTheTaxBase && line.OSAmount.HasValue)
				{
					if (transactionType == TransactionType.INV)
					{
						return line.OSAmount > 0 ? Item_Det_FactIndFact.Item6 : Item_Det_FactIndFact.Item7;
					}
					else
					{
						return line.OSAmount > 0 ? Item_Det_FactIndFact.Item7 : Item_Det_FactIndFact.Item6;
					}
				}
				else if (taxType.Value == AccTaxRate.Types.CapitalRated)
				{
					return Item_Det_FactIndFact.Item3;
				}
				else if (taxType.Value == AccTaxRate.Types.Rated && taxCode.HasValue)
				{
					if (taxCode.Value == UruguayConstants.IVA)
					{
						return Item_Det_FactIndFact.Item3;
					}
					else if (taxCode.Value == UruguayConstants.IVA10)
					{
						return Item_Det_FactIndFact.Item2;
					}
					else if (taxCode.Value == UruguayConstants.FREEIVA)
					{
						return Item_Det_FactIndFact.Item10;
					}
					else if (taxCode.Value == UruguayConstants.IVA66 || taxCode.Value == UruguayConstants.IVA77 || taxCode.Value == UruguayConstants.IVA396)
					{
						return Item_Det_FactIndFact.Item1;
					}
				}
			}

			return null;
		}
	}
}
