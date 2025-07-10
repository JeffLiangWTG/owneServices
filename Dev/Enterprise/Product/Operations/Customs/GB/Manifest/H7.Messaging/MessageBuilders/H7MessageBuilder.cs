using System.Collections.Generic;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.CDS.Messaging;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.GB.H7.Business.AsycudaBill;

namespace Enterprise.Customs.GB.H7.Messaging
{
	public class H7MessageBuilder : CDS.Messaging.MessageBuilders.H7MessageBuilder
	{
		public H7MessageBuilder(BusinessObject asycudaBill, ErrorCollector errorCollector, string functionCode) : base(asycudaBill, errorCollector, functionCode)
		{
		}

		BusinessObjectFactory Factory => messagingParent.Factory;

		protected override IDeclaration GetCdsDeclarationFromEntry()
		{
			return new GbCDSH7ImportDeclarationWrapper((MessageSendingObject)messagingParent);
		}

		protected override void PopulateTotalPackageQuantity()
		{
		}

		protected override void PopulateCustomsValueAmount(IGovernmentAgencyGoodsItem goodsItem)
		{
			const string requiredCurrencyCode = CurrencyCodes.UnitedKingdom;
			var amountAndCurrency = (goodsItem as ICommodity)?.InvoiceLineItemCharge;
			if (amountAndCurrency != null)
			{
				var currency = RefCurrency.LoadFromCurrencyCode(Factory, amountAndCurrency.Currency);
				var money = new Money(amountAndCurrency.Amount, currency);
				if (money.Currency?.Code != requiredCurrencyCode)
				{
					var requiredCurrency = RefCurrency.LoadFromCurrencyCode(Factory, requiredCurrencyCode);
					var messageSendingObject = (MessageSendingObject)messagingParent;
					money = ((AsycudaManifestHeader)(messageSendingObject.Bill.Header)).CurrencyConverter.ConvertRounded(money, requiredCurrency);
				}
				decGoodsItem.CustomsValueAmount = new GovernmentAgencyGoodsItemCustomsValueAmountType { currencyID = money.Currency?.Code, Value = money.Amount };
			}
		}

		protected override void PopulateDomesticDutyTaxParties()
		{
			var messageSendingObject = messagingParent as MessageSendingObject;
			var bill = messageSendingObject?.Bill as AsycudaBill;
			var domesticDutyTaxParties = new List<DeclarationGoodsShipmentDomesticDutyTaxParty>();

			if (bill != null)
			{
				var fr5Condition = !bill.ABL_SellerRegNo.IsEmpty && bill.ABL_SellerRegNoType == OrgCusCode.EuropeanUnionSharedCodeTypes.ImportOneStopShopVatRegistration;
				var fr1Condition = !bill.VATNumber.IsEmpty && bill.GetSystemDefinedValue<ZBool>(GenAddOnColumnConstants.PostponedVatAccountingEntryColumnName);

				if (fr5Condition && !fr1Condition)
				{
					domesticDutyTaxParties.Add(new DeclarationGoodsShipmentDomesticDutyTaxParty
					{
						ID = new DomesticDutyTaxPartyIdentificationIDType { Value = bill.ABL_SellerRegNo },
						RoleCode = new DomesticDutyTaxPartyRoleCodeType { Value = FiscalReferenceCodeList.Codes.FR5_Vendor }
					});
				}
				else if (!fr5Condition && fr1Condition)
				{
					domesticDutyTaxParties.Add(new DeclarationGoodsShipmentDomesticDutyTaxParty
					{
						ID = new DomesticDutyTaxPartyIdentificationIDType { Value = bill.VATNumber },
						RoleCode = new DomesticDutyTaxPartyRoleCodeType { Value = FiscalReferenceCodeList.Codes.FR1_Importer }
					});
				}
			}
			else
			{
				domesticDutyTaxParties.Add(new DeclarationGoodsShipmentDomesticDutyTaxParty());
			}

			decShipment.DomesticDutyTaxParty = domesticDutyTaxParties.ToArray();
		}
	}
}
