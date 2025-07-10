using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business
{
	public class CommissionLinesHelper
	{
		readonly bool Regenerating;

		public CommissionLinesHelper(bool regenerating = false)
		{
			this.Regenerating = regenerating;
		}

		public AccTransactionLines[] GetTransactionLines(IJobHeader job)
		{
			if (job == null)
			{
				return System.Array.Empty<AccTransactionLines>();
			}

			var factory = job.Factory;
			var jobQuery = new ZQuery(JobHeaderSchema.PK, job.PK);
			jobQuery.AddToFilter(JoinCondition.Or, JobHeaderSchema.JH_JH_ParentJob, job.PK);

			var jobPKs = factory.Load<JobHeader>(jobQuery).Select(j => j.PK).ToArray();
			var lineQuery = new ZQuery(AccTransactionLinesSchema.AL_JH, jobPKs);

			var transactionHeaderPKsInJob = factory.Load<AccTransactionLines>(lineQuery).Select(l => l.AL_AH).Distinct().ToArray();

			var headerInJobQuery = new ZQuery(AccTransactionHeaderSchema.AH_JH, job.PK);
			headerInJobQuery.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.PK, transactionHeaderPKsInJob);

			var headerQuery = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, TransactionCommissionCreator.CommissionableLedgerTypes);
			headerQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, job.JH_GC);
			headerQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionCommissionCreator.CommissionableTransactionTypes);
			headerQuery.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, false);
			headerQuery.AddToFilter(headerInJobQuery);

			var transactionHeaderPKs = factory.Load<AccTransactionHeader>(headerQuery).Select(h => h.PK).ToArray();
			var transactionLinesQuery = new ZQuery(AccTransactionLinesSchema.AL_AH, transactionHeaderPKs);

			var lines = factory.Load<AccTransactionLines>(transactionLinesQuery).ToList();
			lines.RemoveAll(l => (l.AL_LineType == TransactionLineTypes.Revenue || l.AL_LineType == TransactionLineTypes.Cost) && l.AL_ReverseDate.IsEmpty);

			var sortedLines = lines.OrderBy(l =>
				(l.AL_LineType == TransactionLineTypes.Revenue || l.AL_LineType == TransactionLineTypes.Cost) ?
				l.AL_ReverseDate : l.AL_PostDate);
			return sortedLines.ToArray();
		}
		public void Add(AccCommissionHeader commissionHeader, ICommissionAgreementAndRates agreementAndRates)
		{
			foreach (var lineGroup in commissionHeader.LineGroups.ToArray())
			{
				AddPercentageCommissionLines(lineGroup, agreementAndRates);
			}

			CreateFixedCommissionLines(commissionHeader, agreementAndRates);
		}

		#region Percentage Commission Recipients

		protected void AddPercentageCommissionLines(AccCommissionLineGroup commissionLineGroup, ICommissionAgreementAndRates agreementAndRates)
		{
			if (commissionLineGroup.CLG_TotalCommissionableAmount != 0)
			{
				var effectiveAgreement = agreementAndRates.CommissionAgreement;
				if (effectiveAgreement != null)
				{
					var percentageShareTotal = (ZShort)effectiveAgreement.Recipients.Where(x => x.CAR_CommissionType == CommissionTypes.Codes.PCT).Sum(x => x.CAR_Share);
					foreach (var recipientRatePair in agreementAndRates.GetRecipientRatePairs(commissionLineGroup.CLG_AC))
					{
						var agreementRecipient = recipientRatePair.Recipient;
						if (agreementRecipient.CAR_Share > 0)
						{
							if (agreementRecipient.CAR_CommissionType == CommissionTypes.Codes.PCT)
							{
								var agreementRate = recipientRatePair.Rate;
								if (Regenerating || ShouldCreateCommissionLine(commissionLineGroup, null, recipientRatePair))
								{
									var commissionLine = commissionLineGroup.Lines.AddNew();
									using (commissionLine.GetValidationSuspender())
									{
										commissionLine.CL0_CAT = agreementRate.PK;
										commissionLine.CL0_GS_NKStaff = agreementRecipient.CAR_GS_NKStaff;
										commissionLine.CL0_OH_Party = agreementRecipient.CAR_OH_Party;

										commissionLine.CL0_CommissionType = CommissionTypes.Codes.PCT;
										commissionLine.CL0_RX_NKTransactionCurrency = commissionLineGroup.CLG_RX_NKTransactionCurrency;
										commissionLine.CL0_TransactionAmount = commissionLineGroup.CLG_TransactionAmount;
										commissionLine.CL0_RX_NKCommissionCurrency = commissionLineGroup.CLG_RX_NKCommissionCurrency;
										commissionLine.CL0_TotalCommissionableAmount = commissionLineGroup.CLG_TotalCommissionableAmount;
										commissionLine.CL0_ShareTotal = percentageShareTotal;
										commissionLine.CL0_SharePortion = agreementRecipient.CAR_Share;
										commissionLine.CL0_ShareCommissionAmount = (commissionLine.CL0_ShareTotal > 0) ? ((double)commissionLine.CL0_TotalCommissionableAmount * (double)commissionLine.CL0_SharePortion / (double)commissionLine.CL0_ShareTotal) : 0d;
										commissionLine.CL0_EntityPercentage = agreementRate.CAT_CommissionPercentage;
										commissionLine.CL0_EntityCommissionAmount = commissionLine.CL0_ShareCommissionAmount * commissionLine.CL0_EntityPercentage / 100m;
									}
								}
							}
						}
					}
				}
			}
		}

		#endregion

		#region Fixed Commission Recipients

		protected void CreateFixedCommissionLines(AccCommissionHeader commissionHeader, ICommissionAgreementAndRates agreementAndRates)
		{
			if (commissionHeader.LineGroups.Sum(x => x.CLG_TotalCommissionableAmount) <= 0)
			{
				return;
			}

			var effectiveAgreement = agreementAndRates.CommissionAgreement;
			var recipientRatePairs = agreementAndRates.RecipientRatePairs;
			if (effectiveAgreement == null || recipientRatePairs == null || recipientRatePairs.Count == 0)
			{
				return;
			}

			var agreementRecipientPksToExclude = new HashSet<ZGuid>(GetExistingFixedCommissionAgreementRecipients(commissionHeader).Select(x => x.PK));
			var recipientRatePairsThatDontHaveExistingFixedCommission = agreementAndRates.RecipientRatePairs.Where(x => !agreementRecipientPksToExclude.Contains(x.Recipient.PK));
			if (recipientRatePairsThatDontHaveExistingFixedCommission.Any())
			{
				var fixedShareTotal = (ZShort)effectiveAgreement.Recipients.Where(x => x.CAR_CommissionType == CommissionTypes.Codes.FIX).Sum(x => x.CAR_Share);
				foreach (var recipientRatePair in recipientRatePairsThatDontHaveExistingFixedCommission)
				{
					var agreementRecipient = recipientRatePair.Recipient;
					if (agreementRecipient.CAR_CommissionType == CommissionTypes.Codes.FIX)
					{
						var agreementRate = recipientRatePair.Rate;
						if (Regenerating || ShouldCreateCommissionLine(null, commissionHeader, recipientRatePair))
						{
							var commissionLine = commissionHeader.Lines.AddNew();
							using (commissionLine.GetValidationSuspender())
							{
								commissionLine.CL0_CAT = agreementRate.PK;
								commissionLine.CL0_GS_NKStaff = agreementRecipient.CAR_GS_NKStaff;
								commissionLine.CL0_OH_Party = agreementRecipient.CAR_OH_Party;

								commissionLine.CL0_CommissionType = CommissionTypes.Codes.FIX;
								commissionLine.CL0_RX_NKTransactionCurrency = agreementRate.CAT_RX_NKCommissionCurrency;
								commissionLine.CL0_TransactionAmount = agreementRate.CAT_CommissionAmount;
								commissionLine.CL0_RX_NKCommissionCurrency = agreementRate.CAT_RX_NKCommissionCurrency;
								commissionLine.CL0_TotalCommissionableAmount = agreementRate.CAT_CommissionAmount;
								commissionLine.CL0_ShareTotal = fixedShareTotal;
								commissionLine.CL0_SharePortion = agreementRecipient.CAR_Share;
								commissionLine.CL0_ShareCommissionAmount = (commissionLine.CL0_ShareTotal > 0) ? ((double)commissionLine.CL0_TotalCommissionableAmount * (double)commissionLine.CL0_SharePortion / (double)commissionLine.CL0_ShareTotal) : 0d;
								commissionLine.CL0_EntityPercentage = 100;
								commissionLine.CL0_EntityCommissionAmount = commissionLine.CL0_ShareCommissionAmount;
							}
						}
					}
				}
			}
		}

		static IEnumerable<OrgCommissionAgreementRecipient> GetExistingFixedCommissionAgreementRecipients(AccCommissionHeader commissionHeader)
		{
			var relatedCommissionHeaderQuery = new ZQuery(AccCommissionHeaderSchema.CH0_GroupingSourceID, commissionHeader.CH0_GroupingSourceID);
			relatedCommissionHeaderQuery.AddToFilter(AccCommissionHeaderSchema.CH0_OverridenDateTimeUtc, null);

			var factory = commissionHeader.Factory;
			var relatedCommissionHeaders = factory.Load<AccCommissionHeader>(relatedCommissionHeaderQuery);

			return
				from relatedHeader in relatedCommissionHeaders
				from commissionLine in relatedHeader.Lines
				let agreementRecipient = commissionLine.CommissionAgreementRecipient
				where
					commissionLine.CL0_CommissionType == CommissionTypes.Codes.FIX &&
					agreementRecipient != null
				select commissionLine.CommissionAgreementRecipient;
		}

		#endregion

		internal bool ShouldCreateCommissionLine(AccCommissionLineGroup lineGroup, AccCommissionHeader header, RecipientRatePair recipientRatePair)
		{
			var factory = lineGroup != null ? lineGroup.Factory : header.Factory;
			var transactionHeader = lineGroup != null ? lineGroup.CommissionHeader.Source : header.Source;
			var agreementPK = lineGroup != null ? lineGroup.CommissionHeader.CH0_CA0 : header.CH0_CA0;
			var groupingID = lineGroup != null ? lineGroup.CommissionHeader.CH0_GroupingSourceID : header.CH0_GroupingSourceID;
			var query = new ZDBOnlyQuery(typeof(ViewCommissionLine));
			query.ReLoadExistingRows = true;
			query.AddToFilter(ViewCommissionLineSchema.VCL_AH, transactionHeader.PK);
			query.AddToFilter(ViewCommissionLineSchema.VCL_GroupingSourceID, groupingID);
			query.AddToFilter(ViewCommissionLineSchema.VCL_CA0, agreementPK);
			var viewLines = factory.Load<ViewCommissionLine>(query);

			// If we haven't yet made commission lines for this transaction and commission agreement
			if (!viewLines.Any())
			{
				return true;
			}
			else
			{
				var linesToReinstate = viewLines.Where(line => line.VCL_ShouldReinstate);

				foreach (var existingLine in linesToReinstate)
				{
					var existingLineInvoiceNum = existingLine.TransactionHeader.AH_TransactionNum;
					var existingLineChargeCode = existingLine.VCL_AC;
					var existingLineCurrency = existingLine.VCL_RX_NKTransactionCurrency;

					var newLineInvoiceNum = lineGroup != null ? lineGroup.CommissionHeader.Source.AH_TransactionNum : header.Source.AH_TransactionNum;
					var newLineChargeCode = lineGroup != null ? lineGroup.CLG_AC : ZGuid.Empty;
					var newLineCurrency = lineGroup != null ? lineGroup.CLG_RX_NKTransactionCurrency : recipientRatePair.Rate.CAT_RX_NKCommissionCurrency;

					if (existingLineInvoiceNum == newLineInvoiceNum
						&& existingLineChargeCode == newLineChargeCode
						&& existingLineCurrency == newLineCurrency)
					{
						return true;
					}
				}

				return false;
			}
		}
	}
}
