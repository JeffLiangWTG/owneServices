using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class ClientIncidentEstimate : AutoClientIncidentEstimate
	{
		public ClientIncidentEstimate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("Incident")]
		public override ZGuid CIE_IM
		{
			get { return base.CIE_IM; }
			set { base.CIE_IM = value; }
		}

		public SupportIncident Incident
		{
			get { return Factory.Load<SupportIncident>(CIE_IM); }
		}

		[DecimalPlaces(2)]
		public override ZDecimal CIE_MinDevelopmentHours
		{
			get { return base.CIE_MinDevelopmentHours; }
			set { base.CIE_MinDevelopmentHours = value; }
		}

		[DecimalPlaces(2)]
		public override ZDecimal CIE_MaxDevelopmentHours
		{
			get { return base.CIE_MaxDevelopmentHours; }
			set { base.CIE_MaxDevelopmentHours = value; }
		}

		public override ZDateTime CIE_EstimateSentDateUTC
		{
			get { return base.CIE_EstimateSentDateUTC; }
			set
			{
				base.CIE_EstimateSentDateUTC = value;
				if (value.IsValid)
				{
					if (value.AddDays(7).IsValidSmallDateTime)
					{
						CIE_ExpressDeliveryOptionCutOffDateUTC = value.AddDays(7);
					}
					if (value.AddDays(EDIDataRegistry.Instance.FeatureRequestEstimateAutoExpirePeriod.Value).IsValidSmallDateTime)
					{
					CIE_EstimateExpiryDateUTC = value.AddDays(EDIDataRegistry.Instance.FeatureRequestEstimateAutoExpirePeriod.Value);
					}
				}
			}
		}

		public ZDateTime CIE_EstimateSentDateLocal
		{
			get { return CIE_EstimateSentDateUTC.IsValid ? Env.Time.GetLocalTimeFromUtc(CIE_EstimateSentDateUTC.ToDateTime()).Date : CIE_EstimateSentDateUTC; }
			set { CIE_EstimateSentDateUTC = value.IsValid ? Env.Time.GetUtcFromLocalTime(value.ToDateTime()) : value; }
		}

		public ZDateTime CIE_EstimateExpiryDateLocal
		{
			get { return CIE_EstimateExpiryDateUTC.IsValid ? Env.Time.GetLocalTimeFromUtc(CIE_EstimateExpiryDateUTC.ToDateTime()).Date : CIE_EstimateExpiryDateUTC; }
			set { CIE_EstimateExpiryDateUTC = value.IsValid ? Env.Time.GetUtcFromLocalTime(value.ToDateTime()) : value; }
		}

		public ZDateTime CIE_ExpressDeliveryOptionCutOffDateLocal
		{
			get { return CIE_ExpressDeliveryOptionCutOffDateUTC.IsValid ? Env.Time.GetLocalTimeFromUtc(CIE_ExpressDeliveryOptionCutOffDateUTC.ToDateTime()).Date : CIE_ExpressDeliveryOptionCutOffDateUTC; }
			set { CIE_ExpressDeliveryOptionCutOffDateUTC = value.IsValid ? Env.Time.GetUtcFromLocalTime(value.ToDateTime()) : value; }
		}

		public ZPropertyInfo CIE_ExpressDeliveryOptionCutOffDateLocalInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(CIE_ExpressDeliveryOptionCutOffDateLocal), x => CIE_ExpressDeliveryOptionCutOffDateUTCInfo); }
		}

		public ZDateTime CIE_QuoteRequestedLocal
		{
			get { return CIE_QuoteRequestedUTC.IsValid ? Env.Time.GetLocalTimeFromUtc(CIE_QuoteRequestedUTC.ToDateTime()).Date : CIE_QuoteRequestedUTC; }
			set { CIE_QuoteRequestedUTC = value.IsValid ? Env.Time.GetUtcFromLocalTime(value.ToDateTime()) : value; }
		}

		[List("Lookups.PaymentTerms")]
		public override ZString CIE_PaymentTerms
		{
			get { return base.CIE_PaymentTerms; }
			set { base.CIE_PaymentTerms = value; }
		}

		[DecimalPlaces(2)]
		public override ZDecimal CIE_MinEstimateMonthly
		{
			get { return base.CIE_MinEstimateMonthly; }
			set { base.CIE_MinEstimateMonthly = value; }
		}

		[DecimalPlaces(2)]
		public override ZDecimal CIE_MaxEstimateMonthly
		{
			get { return base.CIE_MaxEstimateMonthly; }
			set { base.CIE_MaxEstimateMonthly = value; }
		}

		[DecimalPlaces(2)]
		public override ZDecimal CIE_MinEstimateOneoff
		{
			get { return base.CIE_MinEstimateOneoff; }
			set { base.CIE_MinEstimateOneoff = value; }
		}

		[DecimalPlaces(2)]
		public override ZDecimal CIE_MaxEstimateOneoff
		{
			get { return base.CIE_MaxEstimateOneoff; }
			set { base.CIE_MaxEstimateOneoff = value; }
		}

		[DecimalPlaces(2)]
		public override ZDecimal CIE_CancellationFee
		{
			get { return base.CIE_CancellationFee; }
			set { base.CIE_CancellationFee = value; }
		}

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new EstimateUniqueIndexFailureHandler(); }
		}

		class EstimateUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				notifier.ReportError("While you have been working, another user has made changes. Please close and reopen the form again.", "Save Error");
			}

			public IEnumerable<string> HandledUniqueIndexNames
			{
				get { yield return ClientIncidentEstimateSchema.Constants.Indexes.FK_UC__CIE_IM; }
			}
		}
	}
}
