using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ClientSharedComponents;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	static class UPEUtility
	{
		internal static ZDBOnlyQuery GetDuplicateShipmentQuery(string shortTrackingNumber)
		{
			int checkingPeriod = 3;

			var query = new ZDBOnlyQuery(typeof(CusHAWB));
			query.AddToFilter(CusHAWBSchema.CS_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThan, ZDateTime.UtcNow.Date.AddMonths(-checkingPeriod));

			var subQuery = new ZDBOnlySubQuery(typeof(JobRelatedWayBill), JobRelatedWayBillSchema.EB_ParentID);
			subQuery.AddToFilter(JobRelatedWayBillSchema.EB_WaybillShortNumber, shortTrackingNumber);
			subQuery.AddToFilter(JobRelatedWayBillSchema.EB_WaybillType, JobRelatedWayBill.Constants.RelatedWayBillType.Parent);
			query.AddSubQuery(CusHAWBSchema.PK, subQuery, JoinCondition.And);

			var mawbSubQuery = new ZDBOnlySubQuery(typeof(CusMAWB), CusMAWBSchema.PK);
			mawbSubQuery.AddToFilter(CusMAWBSchema.CM_MAWB, SQLComparisonOperator.NotEqual, ZString.Empty);
			query.AddSubQuery(CusHAWBSchema.CS_CM, mawbSubQuery, JoinCondition.And);
			return query;
		}

		/// <summary>
		/// A list of Debtor Groups that are automatically allowed to be uploaded to BISI.  Regardless of any other condition.
		/// </summary>
		/// <Remarks>NB: DebtorGroup aka Account Group aka Account Class is also known else where in source code as ThirdPartyIndicator.</Remarks>
		public static ReadOnlyCollection<String> UploadableDebtorGroups
		{
			get
			{
				if (uploadableDebtorGroups == null)
				{
					uploadableDebtorGroups = new List<string>(5);
					uploadableDebtorGroups.Add(EnumUtil.GetDescription(AccountGroup.First_StandardAccount));
					uploadableDebtorGroups.Add(EnumUtil.GetDescription(AccountGroup.Second_PreferredCustomerAccount));
					uploadableDebtorGroups.Add(EnumUtil.GetDescription(AccountGroup.Fourth_StandardAccount));
					uploadableDebtorGroups.Add(EnumUtil.GetDescription(AccountGroup.Sixth_StandardAccountCreditCard));
					uploadableDebtorGroups.Add(EnumUtil.GetDescription(AccountGroup.Seventh_ARCheckAccount));
				}
				return new ReadOnlyCollection<String>(uploadableDebtorGroups);
			}
		}
		[ThreadStatic]
		static IList<String> uploadableDebtorGroups;

		#region HoldFinanceQueueDebtorGroups

		/// <summary>
		/// A list of Debtor Group'es to auto hold (OQ) in the Finance Queue upon BISI upload when the shipment Bill Term is Freight Collect.
		/// </summary>
		/// <Remarks>NB: DebtorGroup aka Account Group aka Account Class is also known else where in source code as ThirdPartyIndicator.</Remarks>
		public static ReadOnlyCollection<String> HoldFinanceQueueDebtorGroups
		{
			get
			{
				if (holdFinanceQueueDebtorGroups == null)
				{
					holdFinanceQueueDebtorGroups = new List<string>(2);
					holdFinanceQueueDebtorGroups.Add(EnumUtil.GetDescription(AccountGroup.Tenth_CODAccount));
					holdFinanceQueueDebtorGroups.Add(EnumUtil.GetDescription(AccountGroup.Blank));
				}
				return new ReadOnlyCollection<String>(holdFinanceQueueDebtorGroups);
			}
		}
		[ThreadStatic]
		static IList<String> holdFinanceQueueDebtorGroups;

		#endregion

		#region HoldARQueueDebtorGroups

		/// <summary>
		/// A list of Debtor Group'es to auto hold (OQ) in the Accounts Receivable Queue upon BISI upload when the shipment Bill Term is Freight Collect.
		/// </summary>
		/// <Remarks>NB: DebtorGroup aka Account Group aka Account Class is also known else where in source code as ThirdPartyIndicator.</Remarks>
		public static ReadOnlyCollection<String> HoldARQueueDebtorGroups
		{
			get
			{
				if (holdARQueueDebtorGroups == null)
				{
					holdARQueueDebtorGroups = new List<string>(4);
					holdARQueueDebtorGroups.Add(EnumUtil.GetDescription(AccountGroup.Seventh_ARCheckAccount));
					holdARQueueDebtorGroups.Add(EnumUtil.GetDescription(AccountGroup.Eighth_ARCheckAccount));
					holdARQueueDebtorGroups.Add(EnumUtil.GetDescription(AccountGroup.Ninth_ARCheckAccount));
					holdARQueueDebtorGroups.Add(EnumUtil.GetDescription(AccountGroup.Thirteenth_StandardAccountLegal));
				}
				return new ReadOnlyCollection<String>(holdARQueueDebtorGroups);
			}
		}
		[ThreadStatic]
		static IList<String> holdARQueueDebtorGroups;

		#endregion
	}

	public enum AccountGroup
	{
		[EnumDescription("0")]
		Zero,
		[EnumDescription("1")]
		First_StandardAccount,
		[EnumDescription("2")]
		Second_PreferredCustomerAccount,
		[EnumDescription("4")]
		Fourth_StandardAccount,
		[EnumDescription("5")]
		Fifth_StandardAccountCreditCard,
		[EnumDescription("6")]
		Sixth_StandardAccountCreditCard,
		[EnumDescription("7")]
		Seventh_ARCheckAccount,
		[EnumDescription("8")]
		Eighth_ARCheckAccount,
		[EnumDescription("9")]
		Ninth_ARCheckAccount,
		[EnumDescription("10")]
		Tenth_CODAccount,
		[EnumDescription("13")]
		Thirteenth_StandardAccountLegal,
		[EnumDescription("")]
		Blank,
		[EnumDescription("N")]
		N
	}
}
