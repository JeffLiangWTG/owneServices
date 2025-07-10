using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	sealed class ShipmentDataForTest : LineKeyForTest, IShipmentData
	{
		#region IShipmentData Members
		public ZString ImporterAccountNumber
		{
			get
			{
				return fImporterAccountNumber;
			}

			set
			{
				fImporterAccountNumber = value;
			}
		}

		public ZString DutyType
		{
			get
			{
				return fDutyType;
			}

			set
			{
				fDutyType = value;
			}
		}

		public ZString BillingTerms
		{
			get
			{
				return fBillingTerms;
			}

			set
			{
				fBillingTerms = value;
			}
		}

		public ZString MasterBillNumber
		{
			get
			{
				return fMasterBillNumber;
			}

			set
			{
				fMasterBillNumber = value;
			}
		}

		public ZString DischargePort
		{
			get
			{
				return fDischargePort;
			}

			set
			{
				fDischargePort = value;
			}
		}

		public ZDecimal CustomsValue
		{
			get
			{
				return fCustomsValue;
			}

			set
			{
				fCustomsValue = value;
			}
		}

		public ZString DVCCurrencyCode
		{
			get
			{
				return fDVCCurrencyCode;
			}

			set
			{
				fDVCCurrencyCode = value;
			}
		}

		public ZDecimal CustomsExchangeRate
		{
			get
			{
				return fCustomsExchangeRate;
			}

			set
			{
				fCustomsExchangeRate = value;
			}
		}

		public ZString BISICustomsEntryStatus
		{
			get
			{
				return fBISICustomsEntryStatus;
			}

			set
			{
				fBISICustomsEntryStatus = value;
			}
		}

		public ZString EntryType
		{
			get
			{
				return fEntryType;
			}

			set
			{
				fEntryType = value;
			}
		}

		public ZString CustomsStatus
		{
			get
			{
				return fCustomsStatus;
			}

			set
			{
				fCustomsStatus = value;
			}
		}

		public ZString CustomsEntryNumber
		{
			get
			{
				return fCustomsEntryNumber;
			}

			set
			{
				fCustomsEntryNumber = value;
			}
		}

		public ZDateTime CustomsEntryDate
		{
			get
			{
				return fCustomsEntryDate;
			}

			set
			{
				fCustomsEntryDate = value;
			}
		}

		public ZString ThirdPartyIndicator
		{
			get
			{
				return fThirdPartyIndicator;
			}

			set
			{
				fThirdPartyIndicator = value;
			}
		}

		public ZDateTime BisiDeclarationUploadDate
		{
			get
			{
				return fDateCusHAWBOrDeclarationUploadedToBISI;
			}

			set
			{
				fDateCusHAWBOrDeclarationUploadedToBISI = value;
			}
		}

		public bool IsAlreadyUploaded
		{
			get
			{
				return fIsAlreadyUploaded;
			}

			set
			{
				fIsAlreadyUploaded = value;
			}
		}

		public bool ShouldBeUploaded
		{
			get
			{
				return fShouldBeUploaded;
			}

			set
			{
				fShouldBeUploaded = value;
			}
		}

		public bool ShouldBeDownloaded
		{
			get
			{
				return fShouldBeDownloaded;
			}

			set
			{
				fShouldBeDownloaded = value;
			}
		}

		public IReadOnlyList<CommodityDetailData> CommoditiesData
		{
			get
			{
				return fCommoditiesData;
			}

			set
			{
				fCommoditiesData = value;
			}
		}

		public IReadOnlyList<ShipmentChargeData> ChargesData
		{
			get
			{
				return fChargesData;
			}

			set
			{
				fChargesData = value;
			}
		}

		public IReadOnlyList<ShipmentReceiptData> ReceiptsData
		{
			get
			{
				return fReceiptsData;
			}

			set
			{
				fReceiptsData = value;
			}
		}

		public bool IsSplitShipment
		{
			get
			{
				return fIsSplitShipment;
			}

			set
			{
				fIsSplitShipment = value;
			}
		}

		public ZDecimal StatisticalValue
		{
			get
			{
				return fStatisticalValue;
			}

			set
			{
				fStatisticalValue = value;
			}
		}

		public ZString CustomsOfficeNumber
		{
			get
			{
				return fCustomsOfficeNumber;
			}

			set
			{
				fCustomsOfficeNumber = value;
			}
		}

		public ZString VATNumber
		{
			get
			{
				return fVATNumber;
			}

			set
			{
				fVATNumber = value;
			}
		}

		public ZString ImporterVATDefermentNumber
		{
			get
			{
				return fImporterVATDefermentNumber;
			}

			set
			{
				fImporterVATDefermentNumber = value;
			}
		}

		public ZString SplitDutyDefermentNumber
		{
			get
			{
				return fSplitDutyDefermentNumber;
			}

			set
			{
				fSplitDutyDefermentNumber = value;
			}
		}

		public Logs Logs
		{
			get
			{
				return fLogs;
			}

			set
			{
				fLogs = value;
			}
		}

		public void MarkShipmentAsSplitShipmentIfApplicable()
		{
		}

		ZString fImporterAccountNumber;
		ZString fDutyType;
		ZString fBillingTerms;
		ZString fMasterBillNumber;
		ZString fDischargePort;
		ZDecimal fCustomsValue;
		ZString fDVCCurrencyCode;
		ZDecimal fCustomsExchangeRate;
		ZString fBISICustomsEntryStatus;
		ZString fEntryType;
		ZString fCustomsStatus;
		ZString fCustomsEntryNumber;
		ZDateTime fCustomsEntryDate;
		ZDateTime fDateCusHAWBOrDeclarationUploadedToBISI;
		bool fIsAlreadyUploaded;
		bool fShouldBeUploaded;
		bool fShouldBeDownloaded;
		ZString fThirdPartyIndicator;
		IReadOnlyList<CommodityDetailData> fCommoditiesData;
		IReadOnlyList<ShipmentChargeData> fChargesData;
		IReadOnlyList<ShipmentReceiptData> fReceiptsData;
		bool fIsSplitShipment;
		ZDecimal fStatisticalValue;
		ZString fCustomsOfficeNumber;
		ZString fVATNumber;
		ZString fImporterVATDefermentNumber;
		ZString fSplitDutyDefermentNumber;
		Logs fLogs;
		#endregion
	}
}
