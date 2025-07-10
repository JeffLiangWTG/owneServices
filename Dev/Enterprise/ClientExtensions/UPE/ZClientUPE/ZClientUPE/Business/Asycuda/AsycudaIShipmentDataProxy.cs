using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.BISI;
using Enterprise.Client.UPE.Business.DataImport;
using Enterprise.Client.UPE.Business.DataImport.Level1FileFormat;
using Enterprise.Client.UPE.Business.SGAccess;
using Enterprise.Customs.Business;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using CustomsEntryType = Enterprise.Customs.ASYCUDA.Business.Constants.CustomsEntryType;

namespace Enterprise.Client.UPE.Business.Asycuda
{
	public class AsycudaIShipmentDataProxy : NonPersistentBusinessObject, IShipmentData, IBisiUpload
	{
		readonly AsycudaBill bill;

		readonly IEnumerable<AsycudaPack> asycudaPackCollection;
		readonly Customs.ASYCUDA.Business.ABLEntryNum asycudaBillCountryEntryNum;
		readonly List<AsycudaPackedItem> asycudaPackedItemCollection;
		readonly Customs.ASYCUDA.Business.AsycudaPackedItemEntryNum tradeNetPermitEntryNum;

		public AsycudaIShipmentDataProxy(AsycudaBill bill)
			: base(Argument.NotNull(bill, nameof(bill)).Factory)
		{
			this.bill = bill;
			asycudaPackedItemCollection = new List<AsycudaPackedItem>();

			asycudaPackCollection = bill.Packs.Cast<AsycudaPack>();
			foreach (var asycudaPack in asycudaPackCollection)
			{
				var asycudaPackedItem = asycudaPack.PackedItem;
				asycudaPackedItemCollection.Add(asycudaPackedItem);
			}

			foreach (var packedItemCountry in asycudaPackedItemCollection)
			{
				tradeNetPermitEntryNum = packedItemCountry.CustomsEntryNumbers.OfType<Customs.ASYCUDA.Business.AsycudaPackedItemEntryNum>().FirstOrDefault(x => x.CE_EntryType == CustomsEntryType.TradeNetPermit && !x.CE_EntryNum.IsEmpty);
				if (tradeNetPermitEntryNum != null)
				{
					break;
				}
			}

			var sgBill = bill;
			if (sgBill.CustomsEntryNumberType == CustomsEntryType.ACCESSPermit)
			{
				asycudaBillCountryEntryNum = sgBill.CusEntryNumber;
			}
		}

		public ZDateTime BisiUploadDate { get; set; }

		public ZBool IsSubsequentSplitShipment
		{
			get { return ZBool.False; }
		}

		#region Level 1 Record
		public Level1Record Level1Record
		{
			get
			{
				if (fLevel1Record == null && Level1RecordNote != null)
				{
					ZString level1RecordAsString = Level1RecordNote.Text;
					if (level1RecordAsString.Length > 0)
					{
						fLevel1Record = new Level1Record();
						fLevel1Record.AddRecordLines(level1RecordAsString.Split('\n'));
					}
				}
				return fLevel1Record;
			}
			set
			{
				fLevel1Record = value;
			}
		}
		Level1Record fLevel1Record;

		Level1RecordNote Level1RecordNote
		{
			get
			{
				if (fLevel1RecordNote == null)
				{
					fLevel1RecordNote = new Level1RecordNote(bill);
				}
				return fLevel1RecordNote;
			}
		}

		Level1RecordNote fLevel1RecordNote;
		#endregion

		#region IShipmentData Members

		bool IShipmentData.IsAlreadyUploaded
		{
			get
			{
				return !((IShipmentData)this).BisiDeclarationUploadDate.IsEmpty;
			}
		}

		bool IShipmentData.ShouldBeUploaded => IsHighValue || BillingTerms == BillingTermsCodeDescriptionPairList.Codes.FreightCollect || bill.DutyAmount > 0 || ShipmentDataProxyHelper.IsCustomChargeShouldbeUploaded;

		public IShipmentDataProxyHelper ShipmentDataProxyHelper => shipmentDataProxyHelper ?? (shipmentDataProxyHelper = new IShipmentDataProxyHelper(bill));
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

		ZString ILineKey.ShipmentRef
		{
			get { return bill.ABL_BillNumber; }
		}

		ZString ILineKey.FlightNo
		{
			get { return bill.Header.AMA_Voyage; }
		}

		ZDateTime ILineKey.ImportDate
		{
			get { return bill.Header.AMA_E_ARV; }
		}

		ZString ILineKey.ConsigneePostCode => bill.Consignee?.Postcode ?? bill.ABL_ConsigneePostcode;

		ZString IShipmentData.DutyType => DutyTypeCodeDescriptionPairList.Codes.Dutiable;

		ZString IShipmentData.MasterBillNumber
		{
			get { return bill.Header.AMA_MasterBill; }
		}

		ZString IShipmentData.DischargePort
		{
			get { return bill.Header.AMA_RL_NKPortOfDischarge; }
		}

		ZDecimal IShipmentData.CustomsValue => bill.ABL_CustomsValue;

		ZString IShipmentData.DVCCurrencyCode
		{
			get
			{
				var result = ZString.Empty;

				if (Level1Record != null && Level1Record._200000 != null)
				{
					result = Level1Record._200000.CurrencyCodeForDeclaredValue;
				}

				if (result.IsEmpty)
				{
					result = bill.ABL_RX_NKCustomsValueCurrency;
				}

				return result;
			}
		}

		ZString IShipmentData.BISICustomsEntryStatus
		{
			get { return !TotalLocalCharges.IsEmpty ? "02" : string.Empty; }
		}

		ZString IShipmentData.CustomsStatus
		{
			get
			{
				return asycudaPackedItemCollection.Any(x => x.API_PackStatus != "CR") ? "IP" : "CR";
				// API_PackStatus indicates Customs Status.  IP = “Inspection Required”, CR = “Consignment Clear”
				// if any of the PackedItem has a value other than "CR" use it, if there are many then "IP" rules.  Otherwise if all "CR" then "CR".
			}
		}

		ZString IShipmentData.CustomsEntryNumber => tradeNetPermitEntryNum?.CE_EntryNum ?? asycudaBillCountryEntryNum?.CE_EntryNum ?? ZString.Empty;

		ZDateTime IShipmentData.CustomsEntryDate => asycudaBillCountryEntryNum?.CE_SystemCreateTimeUtc ?? ZDateTime.Empty;

		ZString IShipmentData.ThirdPartyIndicator
		{
			get { return (Level1Record != null && Level1Record._200000 != null) ? (ZString)Level1Record._200000.ThirdPartyIndicator : ZString.Empty; }
		}

		ZDateTime IShipmentData.BisiDeclarationUploadDate
		{
			get;
			set;
		}

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

		bool IsHighValue
		{
			get
			{
				var result = false;
				if (bill != null)
				{
					var deminimusValue = bill.IsImport ? SGDecisionSupporter.GetCurrentSGCustomsDeminimusValue(bill.Factory) : SGDecisionSupporter.GetSGCustomsExportDeminimusValue(bill.Factory);
					result = bill.ABL_CustomsValue > deminimusValue;
				}

				return result;
			}
		}

		IReadOnlyList<CommodityDetailData> IShipmentData.CommoditiesData
		{
			get
			{
				var result = new CommodityDetailData[asycudaPackedItemCollection.Count];
				var count = 0;
				foreach (var item in asycudaPackedItemCollection)
				{
					result[count] = new CommodityDetailData(item.API_GoodsDescription, item.API_Tariff, item.API_RN_NKGoodsOrigin, item.API_CustomsValue);
					count++;
				}
				return result;
			}
		}

		IReadOnlyList<ShipmentChargeData> IShipmentData.ChargesData
		{
			get
			{
				ShipmentChargeDataList list = new ShipmentChargeDataList();

				foreach (var packedItemCountry in asycudaPackedItemCollection)
				{
					list.AddCharge(ShipmentChargeTypeCode.Duty, packedItemCountry.API_DutyAmount, Core.Constants.CurrencyCodes.Singapore);
					list.AddCharge(ShipmentChargeTypeCode.VAT, packedItemCountry.API_TaxAmount, Core.Constants.CurrencyCodes.Singapore);
				}

				return list.ToArray();
			}
		}

		readonly ZString[] permitTypesNeedToBeAssignedWithGSTExempted = new ZString[] { SGDecisionSupporter.MC, SGDecisionSupporter.ME, "IE", "II", "IM", "IN", "IR", "IT", "EX", "TT" };

		internal ZString[] GetPermitTypesNeedToBeAssignedWithGSTExempted()
		{
			return permitTypesNeedToBeAssignedWithGSTExempted;
		}

		bool NeedToBeAssignedWithGSTExempted(ZString entryNum)
		{
			var permitType = entryNum.SubstringSafe(0, 2);
			return GetPermitTypesNeedToBeAssignedWithGSTExempted().Contains(permitType);
		}

		IReadOnlyList<ShipmentReceiptData> IShipmentData.ReceiptsData
		{
			get
			{
				var list = new List<ShipmentReceiptData>();
				var manifestHeader = bill.Header;
				var sgBillCountry = bill;

				var entryNum = tradeNetPermitEntryNum?.CE_EntryNum ?? ZString.Empty;
				if (NeedToBeAssignedWithGSTExempted(entryNum))
				{
					list.Add(new ShipmentReceiptData(ShipmentReceiptTypeCode.GIROCode, ReceiptGIROCodeTypes.GSTExempted));
				}
				else
				{
					AddGIROPaymentDetails(list, tradeNetPermitEntryNum);
				}

				if (!manifestHeader?.AMA_MasterBill.IsEmpty ?? false)
				{
					list.Add(new ShipmentReceiptData(ShipmentReceiptTypeCode.MAWBNumber, bill.Header.AMA_MasterBill));
				}
				if (!sgBillCountry.CycleDate.IsEmpty)
				{
					list.Add(new ShipmentReceiptData(ShipmentReceiptTypeCode.CycleDate, sgBillCountry.CycleDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
				}

				if (!sgBillCountry.CycleNumber.IsEmpty)
				{
					list.Add(new ShipmentReceiptData(ShipmentReceiptTypeCode.CycleNumber, sgBillCountry.CycleNumber.PadLeft(2, '0')));
				}

				var taxCertificateNumber = UPEDataRegistry.Instance.BISIOBCTaxCertificateNumber;
				if (UPEDataRegistry.Instance.BISIOBCTaxCertificateNumberItem.DataType is IntRegistryDataType dataType)
				{
					var minValue = (int)dataType.LowerBound;
					var maxValue = (int)dataType.UpperBound;

					if (taxCertificateNumber >= minValue && taxCertificateNumber <= maxValue)
					{
						list.Add(new ShipmentReceiptData(ShipmentReceiptTypeCode.OBCTaxCertificateNumber, Core.Constants.CountryCodes.Singapore + "-" + taxCertificateNumber.ToString().PadLeft(7, '0')));

						if (taxCertificateNumber < maxValue)
						{
							UPEDataRegistry.Instance.BISIOBCTaxCertificateNumber = taxCertificateNumber + 1;
						}
						else
						{
							ErrorReporter.ReportOnce(ZString.Format("BISI OBC Certificate Number exceeds the maximum allowed number {0} set in the registry", maxValue));
						}
					}
				}

				if (asycudaBillCountryEntryNum != null && !asycudaBillCountryEntryNum.CE_EntryNum.IsEmpty)
				{
					list.Add(new ShipmentReceiptData(ShipmentReceiptTypeCode.OBCPayDeclarationNumber, asycudaBillCountryEntryNum.CE_EntryNum));
				}

				list.Add(new ShipmentReceiptData(ShipmentReceiptTypeCode.InvoiceQuantity, "0001LOT"));

				return list.ToArray();
			}
		}

		void AddGIROPaymentDetails(List<ShipmentReceiptData> list, Customs.ASYCUDA.Business.AsycudaPackedItemEntryNum asyTradeNetPermitEntryNum)
		{
			var sgBillCountry = bill;
			if (sgBillCountry.SG_PartyStatus == SGPartyStatusList.Codes.A || sgBillCountry.SG_PartyStatus == SGPartyStatusList.Codes.Y)
			{
				list.Add(new ShipmentReceiptData(ShipmentReceiptTypeCode.GIROCode, ReceiptGIROCodeTypes.GSTPaidDeducted));
			}
			else if (IsHighValue)
			{
				CusEntryPayInfo entryPayInfo = null;
				if (asyTradeNetPermitEntryNum != null)
				{
					entryPayInfo = GetEntryPayInfo(asyTradeNetPermitEntryNum.CE_EntryNum);
				}

				if (asyTradeNetPermitEntryNum != null && entryPayInfo != null
					&& (entryPayInfo.C9_PaymentParty == UPEOrgRematch.OrgTypes.Importer || entryPayInfo.C9_PaymentParty == UPEOrgRematch.OrgTypes.Broker))
				{
					var giroCode = entryPayInfo.C9_PaymentParty == UPEOrgRematch.OrgTypes.Importer ? ReceiptGIROCodeTypes.GSTPaidDeducted : ReceiptGIROCodeTypes.GSTPaidThruGIRO;
					list.Add(new ShipmentReceiptData(ShipmentReceiptTypeCode.GIROCode, giroCode));
				}
				else
				{
					list.Add(new ShipmentReceiptData(ShipmentReceiptTypeCode.GIROCode, ReceiptGIROCodeTypes.GSTPaidAtCheckPoint));
				}
			}
			else
			{
				if (sgBillCountry.DutyAmount > 0)
				{
					list.Add(new ShipmentReceiptData(ShipmentReceiptTypeCode.GIROCode, ReceiptGIROCodeTypes.GSTPaidAtCheckPoint));
				}
				else
				{
					list.Add(new ShipmentReceiptData(ShipmentReceiptTypeCode.GIROCode, ReceiptGIROCodeTypes.GSTWaived));
				}
			}
		}

		Logs IShipmentData.Logs
		{
			get { return bill.Logs; }
		}

		void IShipmentData.MarkShipmentAsSplitShipmentIfApplicable()
		{
		}

		public void OnBeforeBisiUpload()
		{
		}

		ZDecimal IShipmentData.CustomsExchangeRate
		{
			get
			{
				var converter = ShipmentDataProxyHelper.GetCurrencyConverter(Factory);
				converter.DateForRate = bill.CycleDate.IsEmpty ? ZDateTime.Today : bill.CycleDate;
				return converter.GetExchangeRate(bill.FreightValueCurrency);
			}
		}

		#region ImporterOrConsigneeMatchedOrgPK

		public ZGuid ImporterOrConsigneeMatchedOrgPK
		{
			get => bill.ABL_OA_Consignee;
		}

		#endregion

		#region Not Required

		ZString IShipmentData.ImporterAccountNumber
		{
			get { return string.Empty; }
		}

		ZDecimal IShipmentData.StatisticalValue
		{
			get { return 0; }
		}

		ZString IShipmentData.EntryType
		{
			get { return string.Empty; }
		}

		ZString IShipmentData.CustomsOfficeNumber
		{
			get { return string.Empty; }
		}

		ZString IShipmentData.VATNumber
		{
			get { return string.Empty; }
		}

		ZString IShipmentData.ImporterVATDefermentNumber
		{
			get { return string.Empty; }
		}

		ZString IShipmentData.SplitDutyDefermentNumber
		{
			get { return string.Empty; }
		}

		#endregion

		CusEntryPayInfo GetEntryPayInfo(string entryNumber)
		{
			if (fEntryPayInfo == null)
			{
				fEntryPayInfo = Factory.LoadTop1<CusEntryPayInfo>(new ZQuery(CusEntryPayInfoSchema.C9_PaymentReference, entryNumber));
			}

			return fEntryPayInfo;
		}
		CusEntryPayInfo fEntryPayInfo;

		#endregion

		#region BillingTerms

		[MaxLength(3)]
		public ZString BillingTerms
		{
			get
			{
				var billingTerms = bill.GetUserDefinedValue<ZString>(Level1DataFileImporterForSGAccess.Constants.CustomizedFieldConstants.Bill.BillingTerms);

				return !billingTerms.IsEmpty
					? billingTerms
					: bill.ABL_PrepaidCollect;
			}
		}

		public virtual ZPropertyInfo BillingTermsInfo
		{
			get { return GetZPropertyInfo(nameof(BillingTerms)); }
		}

		protected bool IsBillingTermsFreeDomicile
		{
			get { return BillingTerms == BillingTermsCodeDescriptionPairList.Codes.FreeDomicile; }
		}

		public ZDateTime TransferredDateTime
		{
			get { return BisiUploadDate; }
			set { BisiUploadDate = value; }
		}
		#endregion
	}
}
