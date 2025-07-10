using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.JAS.Business.Matching
{
	public abstract class PreMatchedDataLine : DataLine
	{
		public PreMatchedDataLine(AccTransactionHeaderWithJobInfo transaction)
		{
			this.Transaction = transaction;
		}

		public abstract override string ToString();

		#region Properties

		ZString fCategory;
		public override ZString Category
		{
			get
			{
				if (fCategory.IsEmpty)
				{
					SetShipmentRelatedInfo(Transaction.Shipment, Transaction.Consol);
				}
				return fCategory;
			}
		}

		ZString fHouseBill;
		public override ZString HouseBill
		{
			get
			{
				if (fHouseBill.IsEmpty)
				{
					SetShipmentRelatedInfo(Transaction.Shipment, Transaction.Consol);
				}
				return fHouseBill;
			}
		}

		ZString fMasterBill;
		public override ZString MasterBill
		{
			get
			{
				if (fMasterBill.IsEmpty)
				{
					SetShipmentRelatedInfo(Transaction.Shipment, Transaction.Consol);
				}
				return fMasterBill;
			}
		}

		public override ZDecimal Amount
		{
			get { return Math.Abs(Transaction.AH_OSOutstandingAmount); }
		}

		public override ZString CounterpartSubsidiary
		{
			get { return (Transaction.Header != null) ? Transaction.Header.CustomsCodes.GetUNC() : ZString.Empty; }
		}

		public override ZBool CreditNote
		{
			get { return IsCreditNote(Transaction.AH_TransactionType, Transaction.AH_Ledger, Transaction.AH_OSOutstandingAmount); }
		}

		public override ZString TransactionType
		{
			get { return Transaction.AH_TransactionType; }
		}

		public override ZString CurrencyCode
		{
			get { return (Transaction.Currency != null) ? Transaction.Currency.RX_Code : ZString.Empty; }
		}

		public override ZString FullInvoiceNumber
		{
			get
			{
				ZString result = (JASDataRegistry.Instance.UseJobInvoiceNumberAsPreMatchingInvoiceRef) ? Transaction.AH_ConsolidatedInvoiceRef : Transaction.AH_TransactionNum;
				if (result.IsEmpty)
				{
					result = Transaction.AH_TransactionNum;
				}
				return result;
			}
		}

		public override ZDateTime InvoiceDate
		{
			get { return Transaction.AH_InvoiceDate; }
		}

		public override ZDateTime MaturityDate
		{
			get { return Transaction.AH_DueDate; }
		}

		protected abstract ZString PayingSubsidiary { get; }
		protected abstract ZString ReceivingSubsidiary { get; }

		#endregion

		#region Implementation

		void SetShipmentRelatedInfo(CommonShipment shipment, JASForwardingConsol consol)
		{
			fMasterBill = GetMasterBill(consol);
			fHouseBill = GetHouseBill(shipment);

			fCategory = ConvertTransportModeToJASCategory((shipment != null) ? shipment.JS_TransportMode : ZString.Empty);
			if (fCategory == "O" || fCategory == "T")
			{
				fMasterBill = fHouseBill = FullInvoiceNumber;
			}
			else if (consol != null)
			{
				if (fCategory == "S" || fCategory == "M")
				{
					if (consol.Containers.Count > 0)
					{
						fMasterBill = consol.Containers[0].JC_ContainerNum;
					}
				}
				else
				{
					if (consol.JK_AgentType == Constants.AgentType.Direct)
					{
						fHouseBill = fMasterBill;
					}
				}
			}
		}

		ZBool IsCreditNote(ZString type, ZString ledger, ZDecimal amount)
		{
			ZBool result = false;
			if (type == TransactionTypes.CreditNote ||
				(type == TransactionTypes.AdjustmentNote &&
				((ledger == ZArchitecture.Core.LedgerTypes.AccountsReceivable && amount < 0) || (ledger == ZArchitecture.Core.LedgerTypes.AccountsPayable && amount > 0))))
			{
				result = true;
			}

			return result;
		}

		ZString GetHouseBill(CommonShipment shipment)
		{
			ZString result = ZString.Empty;
			if (shipment != null)
			{
				result = shipment.JS_HouseBill;
			}

			return result;
		}

		ZString GetMasterBill(JASForwardingConsol consol)
		{
			ZString result = ZString.Empty;
			if (consol != null)
			{
				result = consol.JK_MasterBillNum;
			}

			return result;
		}

		ZString ConvertTransportModeToJASCategory(ZString transportMode)
		{
			ZString result;
			switch (transportMode)
			{
				case Constants.TransportModes.Air: result = "A"; break;
				case Constants.TransportModes.Sea: result = "M"; break;
				case Constants.TransportModes.AirSea:
				case Constants.TransportModes.SeaAir: result = "S"; break;
				case Constants.TransportModes.Road: result = "T"; break;
				default: result = "O"; break;
			}
			return result;
		}

		protected readonly AccTransactionHeaderWithJobInfo Transaction;

#endregion
#region Implementation
#region DummyPreMatchedDataLine
#endregion
#endregion

			}
}

#region Implementation
#region Generate Transactions
#endregion
#endregion
