
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class ClientIncidentQuote : AutoClientIncidentQuote
	{
		public ClientIncidentQuote(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("Incident")]
		public override ZGuid CIQ_IM
		{
			get { return base.CIQ_IM; }
			set { base.CIQ_IM = value; }
		}

		public SupportIncident Incident
		{
			get { return Factory.Load<SupportIncident>(CIQ_IM); }
		}

		[DecimalPlaces(2)]
		public override ZDecimal CIQ_MinDevelopmentHours
		{
			get { return base.CIQ_MinDevelopmentHours; }
			set { base.CIQ_MinDevelopmentHours = value; }
		}

		[DecimalPlaces(2)]
		public override ZDecimal CIQ_MaxDevelopmentHours
		{
			get { return base.CIQ_MaxDevelopmentHours; }
			set { base.CIQ_MaxDevelopmentHours = value; }
		}

		public override ZDateTime CIQ_QuoteSentDateUTC
		{
			get { return base.CIQ_QuoteSentDateUTC; }
			set
			{
				base.CIQ_QuoteSentDateUTC = value;
				if (value.IsValid)
				{
					if (value.AddDays(EDIDataRegistry.Instance.FeatureRequestQuotationAutoExpirePeriod.Value).IsValidSmallDateTime)
					{
						CIQ_QuoteExpiryDateUTC = value.AddDays(EDIDataRegistry.Instance.FeatureRequestQuotationAutoExpirePeriod.Value);
					}
				}
			}
		}

		public ZDateTime CIQ_QuoteSentDateLocal
		{
			get { return CIQ_QuoteSentDateUTC.IsValid ? Env.Time.GetLocalTimeFromUtc(CIQ_QuoteSentDateUTC.ToDateTime()).Date : CIQ_QuoteSentDateUTC; }
			set { CIQ_QuoteSentDateUTC = value.IsValid ? Env.Time.GetUtcFromLocalTime(value.ToDateTime()) : value; }
		}

		public ZDateTime CIQ_QuoteExpiryDateLocal
		{
			get { return CIQ_QuoteExpiryDateUTC.IsValid ? Env.Time.GetLocalTimeFromUtc(CIQ_QuoteExpiryDateUTC.ToDateTime()).Date : CIQ_QuoteExpiryDateUTC; }
			set { CIQ_QuoteExpiryDateUTC = value.IsValid ? Env.Time.GetUtcFromLocalTime(value.ToDateTime()) : value; }
		}

		public ZDateTime CIQ_QuoteAcceptedDateLocal
		{
			get { return CIQ_QuoteAcceptedDateUTC.IsValid ? Env.Time.GetLocalTimeFromUtc(CIQ_QuoteAcceptedDateUTC.ToDateTime()).Date : CIQ_QuoteAcceptedDateUTC; }
			set { CIQ_QuoteAcceptedDateUTC = value.IsValid ? Env.Time.GetUtcFromLocalTime(value.ToDateTime()) : value; }
		}

		public ZDateTime CIQ_DeliveredDateLocal
		{
			get { return CIQ_DeliveredDateUTC.IsValid ? Env.Time.GetLocalTimeFromUtc(CIQ_DeliveredDateUTC.ToDateTime()).Date : CIQ_DeliveredDateUTC; }
			set { CIQ_DeliveredDateUTC = value.IsValid ? Env.Time.GetUtcFromLocalTime(value.ToDateTime()) : value; }
		}

		[DecimalPlaces(2)]
		public override ZDecimal CIQ_CancellationFee
		{
			get { return base.CIQ_CancellationFee; }
			set { base.CIQ_CancellationFee = value; }
		}

		[List("Lookups.PaymentTypes")]
		public override ZString CIQ_Type
		{
			get { return base.CIQ_Type; }
			set
			{
				base.CIQ_Type = value;

				if (value.IsValid)
				{
					if (CIQ_Type == PaymentTypesAndTermsList.PaymentTypes.Codes.MonthlyType)
					{
						CIQ_PaymentTerms = PaymentTypesAndTermsList.PaymentTerms.Codes.MonthlyTerm;
					}
					else if (CIQ_Type == PaymentTypesAndTermsList.PaymentTypes.Codes.OneOffType)
					{
						CIQ_PaymentTerms = PaymentTypesAndTermsList.PaymentTerms.Codes.OneOffTerm;
					}
				}
			}
		}

		[List("Lookups.PaymentTerms")]
		public override ZString CIQ_PaymentTerms
		{
			get { return base.CIQ_PaymentTerms; }
			set { base.CIQ_PaymentTerms = value; }
		}

		[DecimalPlaces(2)]
		public override ZDecimal CIQ_QuoteAmount
		{
			get { return base.CIQ_QuoteAmount; }
			set { base.CIQ_QuoteAmount = value; }
		}

		[DecimalPlaces(2)]
		public override ZDecimal CIQ_OneoffUpfront
		{
			get { return base.CIQ_OneoffUpfront; }
			set { base.CIQ_OneoffUpfront = value; }
		}

		[DecimalPlaces(2)]
		public override ZDecimal CIQ_HeadStartSurcharge
		{
			get { return base.CIQ_HeadStartSurcharge; }
			set { base.CIQ_HeadStartSurcharge = value; }
		}

		[DecimalPlaces(2)]
		public override ZDecimal CIQ_ExpressDeliverySurcharge
		{
			get { return base.CIQ_ExpressDeliverySurcharge; }
			set { base.CIQ_ExpressDeliverySurcharge = value; }
		}

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new QuoteUniqueIndexFailureHandler(); }
		}

		class QuoteUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				notifier.ReportError("While you have been working, another user has made changes. Please close and reopen the form again.", "Save Error");
			}

			public IEnumerable<string> HandledUniqueIndexNames
			{
				get { yield return ClientIncidentQuoteSchema.Constants.Indexes.FK_UC__CIQ_IM; }
			}
		}
	}
}
