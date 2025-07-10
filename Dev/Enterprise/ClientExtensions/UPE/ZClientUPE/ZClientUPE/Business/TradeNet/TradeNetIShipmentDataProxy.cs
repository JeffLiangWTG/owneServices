using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Client.UPE.Business.BISI;
using Enterprise.Client.UPE.Business.SGAccess;
using Enterprise.Customs.Business;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.ZArchitecture.Business;
using CusEntryHeader = Enterprise.Customs.SG.V4.Business.CusEntryHeader;

namespace Enterprise.Client.UPE.Business.Asycuda
{
	public class TradeNetIShipmentDataProxy : NonPersistentBusinessObject, IShipmentData, IBisiUpload
	{
		public TradeNetIShipmentDataProxy(ZString customsStatus, ZDateTime entryDate, JobDeclaration declaration)
		{
			CustomsStatus = customsStatus;
			CustomsEntryDate = entryDate;
			Declaration = declaration;
		}
		readonly ZString CustomsStatus;
		readonly ZDateTime CustomsEntryDate;
		readonly JobDeclaration Declaration;

		#region IShipmentData Members

		bool IShipmentData.IsAlreadyUploaded => !((IShipmentData)this).BisiDeclarationUploadDate.IsEmpty;

		bool IShipmentData.ShouldBeUploaded
		{
			get
			{
				return !TotalLocalCharges.IsEmpty
					|| BillingTerms == BillingTermsCodeDescriptionPairList.Codes.FreightCollect
					|| UPEUtility.UploadableDebtorGroups.Contains(ThirdPartyIndicator)
					|| ShipmentDataProxyHelper.IsCustomChargeShouldbeUploaded;
			}
		}

		public IShipmentDataProxyHelper ShipmentDataProxyHelper => shipmentDataProxyHelper ?? (shipmentDataProxyHelper = new IShipmentDataProxyHelper(Declaration));
		IShipmentDataProxyHelper shipmentDataProxyHelper;

		bool IShipmentData.ShouldBeDownloaded
		{
			get
			{
				bool result = ((IShipmentData)this).ShouldBeUploaded;
				result = result && (BillingTerms != BillingTermsCodeDescriptionPairList.Codes.FreeDomicile);
				return result;
			}
		}

		ZString ILineKey.ShipmentRef => Declaration.JE_HouseBill.IsEmpty ? Declaration.JE_DeclarationReference : Declaration.JE_HouseBill;

		ZString ILineKey.FlightNo => Declaration.JE_VoyageFlightNo;

		ZDateTime ILineKey.ImportDate => Declaration.JE_DateOfArrival;

		ZString ILineKey.ConsigneePostCode => Declaration.ConsigneeAddress?.Postcode ?? ZString.Empty;

		ZString IShipmentData.DutyType => DutyType;

		ZString IShipmentData.MasterBillNumber => Declaration.JE_MasterBill;

		ZString IShipmentData.DischargePort => Declaration.JE_RL_NKPortOfArrival;

		ZDecimal IShipmentData.CustomsValue => Declaration.TotalCustomsValueInLocalCurrency;

		ZString IShipmentData.DVCCurrencyCode => Declaration.LocalCurrencyCode;

		ZDecimal IShipmentData.CustomsExchangeRate
		{
			get
			{
				var result = 0m;
				var invoice = Declaration.Invoices.FirstOrDefault();
				if (invoice != null)
				{
					var rate = invoice.JZ_InvoiceCurrExRate;
					if (!rate.IsEmpty)
					{
						result = rate;
					}
					else
					{
						var converter = ShipmentDataProxyHelper.GetCurrencyConverter(invoice.Factory);
						converter.DateForRate = invoice.EffectiveValuationDate.IsEmpty ? ZDateTime.Today : invoice.EffectiveValuationDate;
						result = converter.GetExchangeRate(invoice.Invoice_Currency);
					}
				}
				return result;
			}
		}

		ZString IShipmentData.BISICustomsEntryStatus => !TotalLocalCharges.IsEmpty ? "02" : string.Empty;

		ZString IShipmentData.CustomsStatus => CustomsStatus;

		ZString IShipmentData.CustomsEntryNumber => Declaration.DeclarationNumber;

		ZDateTime IShipmentData.CustomsEntryDate => CustomsEntryDate;

		ZString IShipmentData.ThirdPartyIndicator => ThirdPartyIndicator;

		protected ZString ThirdPartyIndicator => "0";

		ZDateTime IShipmentData.BisiDeclarationUploadDate { get; set; }

		public ZDecimal TotalLocalCharges
		{
			get
			{
				ZDecimal result = 0m;
				var allCharges = ((IShipmentData)this).ChargesData;

				foreach (ShipmentChargeData shipmentChargeData in allCharges)
				{
					result += shipmentChargeData.GrossAmount;
				}
				return result;
			}
		}

		IReadOnlyList<CommodityDetailData> IShipmentData.CommoditiesData
		{
			get
			{
				ArrayList result = new ArrayList();
				foreach (var invoice in Declaration.Invoices)
				{
					foreach (BaseJobComInvoiceLine invoiceLine in invoice.JobComInvoiceLines)
					{
						CommodityDetailData commodityData = new CommodityDetailData(invoiceLine.JI_Description, invoiceLine.JI_Tariff, invoiceLine.JI_CountryOfOrigin, invoiceLine.JI_LinePrice);
						result.Add(commodityData);
					}
				}
				return (CommodityDetailData[])result.ToArray(typeof(CommodityDetailData));
			}
		}

		IReadOnlyList<ShipmentChargeData> IShipmentData.ChargesData
		{
			get
			{
				ShipmentChargeDataList list = new ShipmentChargeDataList();
				if (UploadCustomsCharges)
				{
					AddChargesFromCustomsCharges(list);
				}

				ShipmentChargeDataList res = new ShipmentChargeDataList();
				foreach (var l in list.ToArray())
				{
					res.AddCharge(l.TypeCodeEnum, l.GrossAmount, Core.Constants.CurrencyCodes.Singapore);
				}

				return res.ToArray();
			}
		}

		IReadOnlyList<ShipmentReceiptData> IShipmentData.ReceiptsData
		{
			get
			{
				var list = new List<ShipmentReceiptData>();
				var tradeNetEntry = Declaration.ActiveEntryHeaders.Count == 1 ? Declaration.ActiveEntryHeaders[0] as CusEntryHeader : null;
				if (tradeNetEntry != null)
				{
					GetReceiptData(tradeNetEntry, list);
				}

				return list.ToArray();
			}
		}

		void GetReceiptData(CusEntryHeader entry, List<ShipmentReceiptData> list)
		{
			var tradeNetPermitEntryNum = entry.CusEntryNumber;
			var tradeNetPermitEntryNumber = tradeNetPermitEntryNum?.CE_EntryNum ?? ZString.Empty;

			if (tradeNetPermitEntryNumber.StartsWith(SGDecisionSupporter.MC, StringComparison.OrdinalIgnoreCase)
				|| tradeNetPermitEntryNumber.StartsWith(SGDecisionSupporter.ME, StringComparison.OrdinalIgnoreCase))
			{
				list.Add(new ShipmentReceiptData(ShipmentReceiptTypeCode.GIROCode, ReceiptGIROCodeTypes.GSTExempted));
			}
			else if (tradeNetPermitEntryNum != null)
			{
				var entryPayInfo = entry.EntryPayInfos.Cast<CusEntryPayInfo>()
					.FirstOrDefault(x => x.C9_PaymentReference == tradeNetPermitEntryNumber);
				var paymentParty = entryPayInfo?.C9_PaymentParty ?? ZString.Empty;

				if (paymentParty == UPEOrgRematch.OrgTypes.Importer)
				{
					list.Add(new ShipmentReceiptData(ShipmentReceiptTypeCode.GIROCode, ReceiptGIROCodeTypes.GSTPaidDeducted));
				}
				else if (paymentParty == UPEOrgRematch.OrgTypes.Broker)
				{
					list.Add(new ShipmentReceiptData(ShipmentReceiptTypeCode.GIROCode, ReceiptGIROCodeTypes.GSTPaidThruGIRO));
				}
				else
				{
					list.Add(new ShipmentReceiptData(ShipmentReceiptTypeCode.GIROCode, ReceiptGIROCodeTypes.GSTWaived));
				}
			}
			else
			{
				list.Add(new ShipmentReceiptData(ShipmentReceiptTypeCode.GIROCode, ReceiptGIROCodeTypes.GSTWaived));
			}

			if (Declaration.IsImport && !Declaration.JE_MasterBill.IsEmpty)
			{
				list.Add(new ShipmentReceiptData(ShipmentReceiptTypeCode.MAWBNumber, Declaration.JE_MasterBill));
			}
			else if (Declaration.IsExport && !Declaration.SG_OutwardMAWB.IsEmpty)
			{
				list.Add(new ShipmentReceiptData(ShipmentReceiptTypeCode.MAWBNumber, Declaration.SG_OutwardMAWB));
			}

			var logs = Declaration.Logs;
			var docEvent = logs.MostRecentLogByEventTime(Events.CustomsEntryStatus, Core.SGConstants.DeclarationStatus.DeclarationPermitReceived) ??
				logs.MostRecentLogByEventTime(Events.DeclarationQueued, log => log.SL_Reference.StartsWith(IShipmentDataExtension.BISIReference, StringComparison.OrdinalIgnoreCase));
			var dokDate = docEvent?.SL_EventTime ?? ZDateTime.Empty;

			if (!dokDate.IsEmpty)
			{
				list.Add(new ShipmentReceiptData(ShipmentReceiptTypeCode.CycleDate,
					dokDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
			}

			list.Add(new ShipmentReceiptData(ShipmentReceiptTypeCode.CycleNumber, "01"));

			if (!tradeNetPermitEntryNumber.IsEmpty)
			{
				list.Add(new ShipmentReceiptData(ShipmentReceiptTypeCode.OBCPayDeclarationNumber, tradeNetPermitEntryNumber));
			}

			list.Add(new ShipmentReceiptData(ShipmentReceiptTypeCode.InvoiceQuantity, "0001LOT"));
		}

		Logs IShipmentData.Logs => Declaration.Logs;

		public bool UploadCustomsCharges => !IsCustomsEFTActive || IsFirstCusHAWBFreeDomicile;

		public bool IsCustomsEFTActive => (Declaration.Importer != null && Declaration.Importer.MiscServ != null
											&& !Declaration.Importer.MiscServ.OM_IMEFTBankBSB.IsEmpty
											&& !Declaration.Importer.MiscServ.OM_IMEFTBankAccount.IsEmpty
											&& Declaration.Importer.MiscServ.OM_IMEftCustomsFromImport);

		bool IsFirstCusHAWBFreeDomicile => BillingTerms == BillingTermsCodeDescriptionPairList.Codes.FreeDomicile;

		void AddChargesFromCustomsCharges(ShipmentChargeDataList charges)
		{
			foreach (CustomsCharge customsCharge in ServiceLocator.GetService<ICustomsCharges>(Declaration).GetCustomsCharges(null))
			{
				var chargeDescription = customsCharge.Description == Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount ? ShipmentChargeDescription.VAT : (string)customsCharge.Description;
				charges.AddCharge(chargeDescription, customsCharge.Amount);
			}
		}

		void IShipmentData.MarkShipmentAsSplitShipmentIfApplicable()
		{
			// not applicable
		}

		public void OnBeforeBisiUpload()
		{
		}

		public ZGuid ImporterOrConsigneeMatchedOrgPK => Declaration.JE_OH_Importer;

		public ZDecimal DutyValue
		{
			get { return Declaration.CustomsEntryHeaders.OfType<CusEntryHeader>().Sum(x => x.TotalDutyAmount); }
		}

		public ZString DutyType => DutyTypeCodeDescriptionPairList.Codes.Dutiable;

		public ZString ShipmentType => Declaration.Shipment?.JS_ShipmentType ?? ZString.Empty;

		#endregion

		#region Not Required

		ZString IShipmentData.ImporterAccountNumber => string.Empty;

		ZDecimal IShipmentData.StatisticalValue => 0;

		ZString IShipmentData.EntryType => string.Empty;

		ZString IShipmentData.CustomsOfficeNumber => string.Empty;

		ZString IShipmentData.VATNumber => string.Empty;

		ZString IShipmentData.ImporterVATDefermentNumber => string.Empty;

		ZString IShipmentData.SplitDutyDefermentNumber => string.Empty;

		#endregion

		#region BillingTerms

		[MaxLength(3)]
		public ZString BillingTerms => Declaration.IncoTerm;

		#endregion

		#region IBisiUpload

		ZDateTime IBisiUpload.TransferredDateTime
		{
			get => fBisiUploadDate;
			set => fBisiUploadDate = value;
		}

		ZDateTime fBisiUploadDate;

		#endregion
	}
}
