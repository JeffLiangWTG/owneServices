using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.UPE.Business.BISI
{
	public interface IShipmentData : ILineKey
	{
		ZString ImporterAccountNumber { get; }
		ZString DutyType { get; }
		ZString BillingTerms { get; }
		ZString MasterBillNumber { get; }
		ZString DischargePort { get; }
		ZDecimal CustomsValue { get; }
		ZString DVCCurrencyCode { get; }
		ZDecimal CustomsExchangeRate { get; }
		ZString BISICustomsEntryStatus { get; }
		ZString EntryType { get; }
		ZString CustomsStatus { get; }
		ZString CustomsEntryNumber { get; }
		ZDateTime CustomsEntryDate { get; }
		ZString ThirdPartyIndicator { get; }
		ZDateTime BisiDeclarationUploadDate { get; set; }
		bool IsAlreadyUploaded { get; }
		bool ShouldBeUploaded { get; }
		bool ShouldBeDownloaded { get; }
		IReadOnlyList<CommodityDetailData> CommoditiesData { get; }
		IReadOnlyList<ShipmentChargeData> ChargesData { get; }
		IReadOnlyList<ShipmentReceiptData> ReceiptsData { get; }

		ZDecimal StatisticalValue { get; }
		ZString CustomsOfficeNumber { get; }
		ZString VATNumber { get; }
		ZString ImporterVATDefermentNumber { get; }
		ZString SplitDutyDefermentNumber { get; }
		Logs Logs { get; }

		void MarkShipmentAsSplitShipmentIfApplicable();
	}
}
