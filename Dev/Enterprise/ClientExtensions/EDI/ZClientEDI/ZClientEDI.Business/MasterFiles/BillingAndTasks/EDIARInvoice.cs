using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public interface IARInvoiceSavingNotificationSubscriber
	{
		string[] GetRecipientsForNotification();

		ZString Identifier { get; }
		ZString HTMLLinkForDirectOpen { get; }
		ZString Description { get; }
		ZString ClientCode { get; }
		ZString ClientName { get; }
	}

	public class EDIARInvoice : ARInvoice
	{
		public EDIARInvoice(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void OnSavingCore()
		{
			base.OnSavingCore();
			if
			(
				(
					(AH_OutstandingAmountHasChangedSinceFactoryLoad && AH_OutstandingAmount > 0)
					|| AH_FullyPaidDateSetSinceFactoryLoad
				)
				&& !AH_IsCancelled
				&& Job != null
			)
			{
				NotifyJobIfRequired();
			}
		}

		protected override Type TypeOfReverseTransaction
		{
			get { return typeof(EDIARCreditNote); }
		}

		void NotifyJobIfRequired()
		{
			if (Job.JH_ParentTableCode == IncidentMainSchema.Constants.Prefix && !Job.JH_ParentID.IsEmpty)
			{
				IARInvoiceSavingNotificationSubscriber subscriber = Factory.Load<IncidentMainBase>(Job.JH_ParentID) as IARInvoiceSavingNotificationSubscriber;
				if (subscriber != null)
				{
					SendEmailNotification(subscriber);
				}
			}
		}

		bool AH_FullyPaidDateSetSinceFactoryLoad
		{
			get { return !AH_FullyPaidDate.IsEmpty && AH_FullyPaidDateInfo.OriginalValue.IsEmpty; }
		}

		bool AH_OutstandingAmountHasChangedSinceFactoryLoad
		{
			get { return AH_OutstandingAmount != (ZDecimal)AH_OutstandingAmountInfo.OriginalValue; }
		}

		protected override TransactionHeaderValidation GetNewValidationCore()
		{
			return new EdiInvoiceValidation(this);
		}

		public bool DisableExchangeRateValidation { get; set; }

		#region Implementation

		void SendEmailNotification(IARInvoiceSavingNotificationSubscriber subscriber)
		{
			HtmlEmailDef email = new HtmlEmailDef();
			string bodyText = IncidentConstants.GetTextFromResource(InvoicePaidNotificationLetter);
			string subjectText = "<IncidentNumber>: <FullOrPartial> <PaymentOrRefund> (<CurrencyCode> <TransactionAmount>).";

			email.AddRecipientForUserCommunication(subscriber.GetRecipientsForNotification());
			email.Subject = InsertValuesIntoEmailTags(subjectText, subscriber);
			email.Body = InsertValuesIntoEmailTags(bodyText, subscriber);
			email.ReplyTo = GlbStaff.CurrentUser.GS_EmailAddress.IsEmpty ? IncidentConstants.ManagerEmailAddress : (string)GlbStaff.CurrentUser.GS_EmailAddress;

			if (email.Recipients.Count > 0)
			{
				Env.OutgoingMailManager.Create(Factory, email);
			}
		}

		#region Macros

		ZString InvoiceJobNo
		{
			get { return AH_ConsolidatedInvoiceRef; }
		}

		ZString OutstandingAmount
		{
			get { return Highlight(AH_Calc_OSOutstandingAmount.ToString(2), AH_Calc_OSOutstandingAmount == 0); }
		}

		ZString InvoicedAmount
		{
			get { return AH_OSTotalAmount.ToString(2); }
		}

		ZString CurrencyCoce
		{
			get { return AH_RX_NKTransactionCurrency; }
		}

		ZString PaymentOrRefund
		{
			get { return IsPayment ? "Payment Received" : "Refund Made"; }
		}

		ZString FullOrPartial
		{
			get { return IsFull ? "FULL" : "PARTIAL"; }
		}

		ZString HighlightedFullOrPartial
		{
			get { return Highlight(FullOrPartial, IsFull && IsPayment); }
		}

		ZString TransactionAmount
		{
			get
			{
				ZDecimal result = ((ZDecimal)AH_OutstandingAmountInfo.OriginalValue - AH_OutstandingAmount) * AH_ExchangeRate;
				return result.Round(2).ToString(2);
			}
		}

		ZBool IsFull
		{
			get { return AH_OutstandingAmount == 0 || AH_InvoiceAmount == AH_OutstandingAmount; }
		}

		ZBool IsPayment
		{
			get { return (ZDecimal)AH_OutstandingAmountInfo.OriginalValue > AH_OutstandingAmount; }
		}

		#endregion

		string InvoicePaidNotificationLetter
		{
			get { return "Enterprise.Client.EDI.MasterFiles.BillingAndTasks.InvoicePaidNotificationLetter.txt"; }
		}

		string InsertValuesIntoEmailTags(string body, IARInvoiceSavingNotificationSubscriber subscriber)
		{
			body = body.Replace("<InvoiceJobNo>", InvoiceJobNo);
			body = body.Replace("<CurrencyCode>", CurrencyCoce);
			body = body.Replace("<IncidentNumber>", subscriber.Identifier);
			body = body.Replace("<HTMLLinkIncidentNumber>", subscriber.HTMLLinkForDirectOpen);
			body = body.Replace("<IncidentDescription>", subscriber.Description);
			body = body.Replace("<InvoicedAmount>", InvoicedAmount);
			body = body.Replace("<TransactionAmount>", TransactionAmount);
			body = body.Replace("<FullOrPartial>", FullOrPartial);
			body = body.Replace("<PaymentOrRefund>", PaymentOrRefund);
			body = body.Replace("<OutstandingAmount>", OutstandingAmount);
			body = body.Replace("<HighlightedFullOrPartial>", HighlightedFullOrPartial);
			body = body.Replace("<ClientCode>", subscriber.ClientCode);
			body = body.Replace("<ClientName>", subscriber.ClientName);

			return body;
		}

		ZString Highlight(ZString value, ZBool greenIfTrueElseRed)
		{
			return "<span class=" + (greenIfTrueElseRed ? "green" : "red") + ">" + value + "</span>";
		}

		#endregion
	}
}

