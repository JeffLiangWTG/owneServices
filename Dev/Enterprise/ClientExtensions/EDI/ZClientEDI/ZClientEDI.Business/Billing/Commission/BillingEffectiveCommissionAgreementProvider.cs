using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.CommissionManagement.Business;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class BillingEffectiveCommissionAgreementProvider : IMultipleCommissionAgreementAndRatesProvider
	{
		#region New

		public static BillingEffectiveCommissionAgreementProvider New(ZGuid clientCompanyPk, ZGuid licenceDatabasePk, ZDate date, Dictionary<(string Stream, ZGuid PivotPk), EdiCommissionAgreement> responsibleAgreements)
		{
			return new BillingEffectiveCommissionAgreementProvider(clientCompanyPk, licenceDatabasePk, date, responsibleAgreements);
		}

		#endregion

		#region Constructor

		protected BillingEffectiveCommissionAgreementProvider(ZGuid clientCompanyPk, ZGuid licenceDatabasePk, ZDate date, Dictionary<(string Stream, ZGuid PivotPk), EdiCommissionAgreement> responsibleAgreements)
		{
			this.clientCompanyPk = clientCompanyPk;
			this.licenceDatabasePk = licenceDatabasePk;
			this.date = date;
			this.responsibleAgreements = responsibleAgreements;
		}

		#endregion

		#region Fields

		readonly ZGuid clientCompanyPk;
		readonly ZGuid licenceDatabasePk;
		readonly ZDate date;
		readonly Dictionary<(string Stream, ZGuid PivotPk), EdiCommissionAgreement> responsibleAgreements;

		#endregion

		#region ByStream

		public Dictionary<ZString, ICommissionAgreementAndRates> ByStream
		{
			get
			{
				if (byStream == null)
				{
					byStream = GetCommissionAgreementAndRatesByStream();
				}

				return byStream;
			}
		}

		Dictionary<ZString, ICommissionAgreementAndRates> byStream;

		public Dictionary<ZString, ICommissionAgreementAndRates> GetCommissionAgreementAndRatesByStream()
		{
			var result = new Dictionary<ZString, ICommissionAgreementAndRates>();

			var streamGrouping = from a in responsibleAgreements
													 group a by a.Key.Stream;
			foreach (var streamGroup in streamGrouping)
			{
				var agreementPair = !clientCompanyPk.IsEmpty
							? streamGroup.FirstOrDefault(a => a.Key.PivotPk == clientCompanyPk)
							: streamGroup.FirstOrDefault(a => a.Key.PivotPk == licenceDatabasePk);

				if (!agreementPair.Equals(default(KeyValuePair<(string Stream, ZGuid PivotPk), EdiCommissionAgreement>)))
				{
					result[streamGroup.Key] = new CommissionAgreementAndRates(agreementPair.Value, date);
				}
			}

			return result;
		}

		#endregion
	}
}

