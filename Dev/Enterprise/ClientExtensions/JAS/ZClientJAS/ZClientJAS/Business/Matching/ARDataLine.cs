using System.Text;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Client.JAS.Business.Matching
{
	public class ARDataLine : PreMatchedDataLine
	{
		public ARDataLine(AccTransactionHeaderWithJobInfo transaction)
			: base(transaction)
		{
		}

		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendFormat("{0,-5}", PayingSubsidiary.Left(5));
			builder.AppendFormat("{0,-5}", ReceivingSubsidiary.Left(5));
			builder.AppendFormat("{0,15:###########0.00}", Amount);
			builder.Append((CreditNote) ? "-" : "+");
			builder.AppendFormat("{0,8:MM/dd/yy}", MaturityDate);
			builder.AppendFormat("{0,-3}", CurrencyCode.Left(3));
			builder.AppendFormat("{0,-6}", OtherRefNumber.Left(6));
			builder.Append(Category.Left(1));
			builder.Append(' ');
			builder.AppendFormat("{0,-34}", Denomination.Left(34));
			builder.AppendFormat("{0,-3}", TransactionType.Left(3));
			builder.Append(new ZString(' ', 11));
			builder.AppendFormat("{0,8:MM/dd/yy}", InvoiceDate);
			builder.AppendFormat("{0,-14}", MasterBill.Left(14));
			builder.AppendFormat("{0,-14}", HouseBill.Left(14));
			builder.AppendFormat("{0,-14}", FullInvoiceNumber.Left(14));
			builder.Append('R');
			return builder.ToString();
		}

		#region Properties

		ZString fDenomination;
		protected ZString Denomination
		{
			get
			{
				if (fDenomination.IsEmpty)
				{
					SetDenomination();
				}
				return fDenomination;
			}
		}

		protected override ZString PayingSubsidiary
		{
			get { return CounterpartSubsidiary; }
		}

		protected override ZString ReceivingSubsidiary
		{
			get { return CurrentSubsidiary; }
		}

		#endregion

		#region Implementation

		void SetDenomination()
		{
			CommonShipment shipment = Transaction.Shipment;
			ForwardingConsol consol = Transaction.Consol;
			StringBuilder builder = new StringBuilder();
			if (shipment != null)
			{
				builder.Append(shipment.JS_RL_NKOrigin);
				builder.Append(shipment.JS_RL_NKDestination);
			}
			else
			{
				builder.Append(new string(' ', 6));
			}

			if (consol != null)
			{
				builder.Append(consol.JK_JX_JV_VoyageFlight);
				builder.AppendFormat("{0:yyyyMMdd}", consol.JK_JX_JA_A_DEP);
				builder.AppendFormat("{0:yyyyMMdd}", consol.JK_JX_JB_A_ARV);
			}

			fDenomination = builder.ToString();
		}

#endregion
			}
}

#region Expected Exported Transaction Strings
#endregion
