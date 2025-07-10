using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CN.MessageContracts;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business
{
	public class AcdAgrRequestImportInfoProvider : IAcdAgrRequestImportInfo
	{
		readonly CusEntryHeader entryHeader;

		public AcdAgrRequestImportInfoProvider(CusEntryHeader entryHeader)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		}

		ICustomsEntryHeader CustomsEntryHeader => entryHeader;

		ICustomsEntryLine RandomCustomsEntryLine => CustomsEntryHeader.EntryLines.FirstOrDefault();

		public string NameOfMainGoods => RandomCustomsEntryLine?.NameOfGoods ?? string.Empty;

		public string TariffCode => RandomCustomsEntryLine?.TariffCode ?? string.Empty;

		public decimal TotalPriceOfGoods
		{
			get
			{
				if (!fTotalPriceOfGoods.HasValue)
				{
					var currencyCode = RandomCustomsEntryLine?.CurrencyCode ?? string.Empty;
					if (!currencyCode.IsEmpty)
					{
						var totalPrice = new Money(0, RefCurrency.LoadFromCurrencyCode(entryHeader.Factory, currencyCode));
						entryHeader.MergedLines.ForEach(x => totalPrice = x.CurrencyConverter.Add(totalPrice, x.TotalPriceMoney));
						fTotalPriceOfGoods = totalPrice.Amount;
					}
					else
					{
						fTotalPriceOfGoods = 0;
					}
				}
				return fTotalPriceOfGoods.Value;
			}
		}
		decimal? fTotalPriceOfGoods;

		public DateTime? ImportExportDate
		{
			get
			{
				if (entryHeader.IsImport)
				{
					var importOrExportDate = CustomsEntryHeader.ImportOrExportDate;
					if (importOrExportDate.IsEmpty)
					{
						return null;
					}
					else
					{
						return importOrExportDate.ToDateTime();
					}
				}
				else
				{
					return ZDateTime.Today.ToDateTime();
				}
			}
		}

		public string BillOfLading => CustomsEntryHeader.BillOfLading;

		public string TradeMode => CustomsEntryHeader.CustomsProcedureCode;

		public string CurrencyCode => CNRefCusMapper.MapCW1CurrencyCodeToCustomsCode(entryHeader.Factory, RandomCustomsEntryLine?.CurrencyCode ?? ZString.Empty);

		public string GoodsOrigin => CNRefCusMapper.MapCW1CountryCodeToCustomsCode(entryHeader.Factory, entryHeader.RandomEntryLine?.RandomLine?.JI_CountryOfOrigin ?? ZString.Empty);

		public string TraderCustomsCode => CustomsEntryHeader.TradePartyCCD;

		public string DeclarantCustomsCode => CustomsEntryHeader.DeclarantCCD;

		public decimal QuantityOrWeight => CustomsEntryHeader.NoOfPacks;

		public string PackingInformation => string.Empty;

		public string OtherNodes => string.Empty;

		public string EntrustingPartyTelephone
		{
			get
			{
				var contact = entryHeader.TradeParty;
				var phoneNumber = contact.PhoneNumber.FormattedForBinding;
				if (!phoneNumber.IsEmpty)
				{
					return phoneNumber;
				}

				return contact.MobilePhoneNumber.FormattedForBinding;
			}
		}

		public string EntryNumber => string.Empty;

		public DateTime ReceivingDate => ZDateTime.Today.ToDateTime();

		public string ReceivingInformation => string.Empty;

		public string OtherInformation => string.Empty;

		public decimal AngencyFee => 0m;

		public string PromiseNotes => string.Empty;

		public string EntrustedPartyTelephone
		{
			get
			{
				var contact = entryHeader.Declarant?.Contacts.GetContactForAllocation(OrgConstants.ContactAllocationType.CNCUS);
				if (contact == null)
				{
					return string.Empty;
				}

				var phone = contact.OC_Phone;
				if (!phone.IsEmpty)
				{
					return phone;
				}

				return contact.OC_Mobile;
			}
		}
	}
}
