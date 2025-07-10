using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.CommissionManagement.Business
{
	public class CommissionAgreementAndRates : ICommissionAgreementAndRates
	{
		public CommissionAgreementAndRates(OrgCommissionAgreement commissionAgreement, ZDate date)
		{
			Argument.NotNull(commissionAgreement, "commissionAgreement");
			if (date.IsEmpty)
			{
				throw new ArgumentException("Passed in empty date, but can not be");
			}

			this.commissionAgreement = commissionAgreement;
			this.CommissionDateByChargeDictionary = new Dictionary<ZGuid, ZDate>();
			CommissionDateByChargeDictionary[ZGuid.Empty] = date;
		}

		public CommissionAgreementAndRates(OrgCommissionAgreement commissionAgreement, Dictionary<ZGuid, ZDate> commissionDateByChargeDictionary)
		{
			Argument.NotNull(commissionAgreement, "commissionAgreement");
			Argument.NotNull(commissionDateByChargeDictionary, "commissionDateByChargeDictionary");

			this.commissionAgreement = commissionAgreement;
			this.CommissionDateByChargeDictionary = commissionDateByChargeDictionary;
		}

		public readonly Dictionary<ZGuid, ZDate> CommissionDateByChargeDictionary;

		#region CommissionAgreement

		public OrgCommissionAgreement CommissionAgreement
		{
			get { return commissionAgreement; }
		}
		readonly OrgCommissionAgreement commissionAgreement;

		#endregion

		#region RecipientRatePairs

		public IReadOnlyCollection<RecipientRatePair> RecipientRatePairs
		{
			get
			{
				if (!recipientRatePairsInitialized)
				{
					recipientRatePairsInitialized = true;
					recipientRatePairs = GetRecipientRatePairs(ZGuid.Empty);
				}

				return recipientRatePairs;
			}
		}
		IReadOnlyCollection<RecipientRatePair> recipientRatePairs;
		ZBool recipientRatePairsInitialized;

		public IReadOnlyCollection<RecipientRatePair> GetRecipientRatePairs(ZGuid chargeCodePK)
		{
			var date = GetCommissionDate(chargeCodePK);

			return
				(from recipient in CommissionAgreement.Recipients
				 where recipient.CAR_EndDate.IsEmpty || date <= recipient.CAR_EndDate
				 let rate = recipient.Rates.Where(x => x.CommissionDateCovered(date)).OrderByDescending(x => x.CAT_CommissionStartDate).FirstOrDefault()
				 where rate != null
				 select new RecipientRatePair(recipient, rate)).ToArray();
		}

		ZDate GetCommissionDate(ZGuid chargeCodePK)
		{
			if (chargeCodePK.IsEmpty)
			{
				if (CommissionDateByChargeDictionary.ContainsKey(ZGuid.Empty))
				{
					return CommissionDateByChargeDictionary[ZGuid.Empty];
				}
				else
				{
					return CommissionDateByChargeDictionary.Where(d => !d.Key.IsEmpty).Min(d => d.Value);
				}
			}
			else
			{
				if (CommissionDateByChargeDictionary.ContainsKey(ZGuid.Empty) && CommissionDateByChargeDictionary.Keys.Count == 1)
				{
					return CommissionDateByChargeDictionary[ZGuid.Empty];
				}
				else if (CommissionDateByChargeDictionary.ContainsKey(chargeCodePK))
				{
					return CommissionDateByChargeDictionary.Where(d => !d.Key.IsEmpty).Min(d => d.Value);
				}
				else
				{
					return ZDate.Empty;
				}
			}
		}

		#endregion
	}
}
