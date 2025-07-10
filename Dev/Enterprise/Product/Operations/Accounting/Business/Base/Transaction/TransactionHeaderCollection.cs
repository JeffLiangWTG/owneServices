using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public class TransactionHeaderCollection : AccTransactionHeaderCollection
	{
		public TransactionHeaderCollection(BusinessObjectFactory factory, ZQuery filter, bool loadInAnyComapny)
			: this(factory, filter)
		{
			fLoadInAnyComapny = loadInAnyComapny;
		}

		public TransactionHeaderCollection(BusinessObjectFactory factory, ZQuery filter)
			: this(factory, filter, GlbCompany.CurrentCompany)
		{
		}

		public TransactionHeaderCollection(BusinessObjectFactory factory, ZQuery filter, GlbCompany company)
			: base(factory, filter)
		{
			fCompany = company;
		}

		public TransactionHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			fCompany = GlbCompany.CurrentCompany;
		}

		public TransactionHeaderCollection(IQueryClaim queryClaim, ZQuery filter)
			: base(queryClaim.Factory, filter)
		{
			fQueryClaim = queryClaim;
			fCompany = GlbCompany.CurrentCompany;
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Transaction Type", "Property", new ZString(TransactionTypes.Invoice), false));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(fQueryClaim.Ledger == LedgerTypes.AccountsPayable ? (NoResString)"Creditor" : (NoResString)"Debtor", "Property", fQueryClaim.AY_OH_Debtor));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Branch", "Property", fQueryClaim.AY_GB));
		}

		protected readonly IQueryClaim fQueryClaim;
		protected readonly GlbCompany fCompany;
		protected readonly bool fLoadInAnyComapny;

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery query = base.CreateAdditionalFilter();
			if (fQueryClaim != null)
			{
				ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
				filter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, fQueryClaim.Ledger);
				filter.AddToFilter(AccTransactionHeaderSchema.AH_OH, fQueryClaim.AY_OH_Debtor);
				filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Invoice);

				query.AddToFilter(filter);
			}
			return query;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			if (fLoadInAnyComapny)
			{
				return new ZQuery();
			}
			else
			{
				return new ZQuery(AccTransactionHeaderSchema.AH_GC, fCompany == null ? ZGuid.Invalid : fCompany.PK);
			}
		}

		public void LoadTransactionsFromMatchLinks(TransactionMatchLinkCollection matchLinks)
		{
			foreach (TransactionMatchLink matchLink in matchLinks)
			{
				TransactionHeader transaction = Factory.Load<TransactionHeader>(matchLink.AP_AH);
				if (transaction != null)
				{
					this.Add(transaction);
				}
			}
		}

		public ZBool ContainsAll(TransactionHeaderCollection headers)
		{
			ZBool containsAll = true;
			if (this.Count == 0 || headers.Count == 0)
			{
				containsAll = false;
			}
			else
			{
				foreach (TransactionHeader header in headers)
				{
					if (!this.Contains(header))
					{
						containsAll = false;
					}
				}
			}
			return containsAll;
		}

		public new TransactionHeader this[int index]
		{
			get
			{
				return (TransactionHeader)(Elements[index]);
			}
		}

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException("This collection contains abstract type entity.");
		}

		protected BusinessObject AddNewCore_Base()
		{
			return base.AddNewCore();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		const string RelatedTransactionDebtorsQuery =
@"IF OBJECT_ID('tempdb..#GroupedPrimaryLines') IS NOT NULL DROP TABLE #GroupedPrimaryLines;

SELECT AH_PK, AH_Ledger, AL_JH, AL_AC, AL_GB, AL_GE
INTO #GroupedPrimaryLines
FROM
	dbo.AccTransactionHeader AS PrimaryHeader
	INNER JOIN dbo.AccTransactionLines AS PrimaryLines ON PrimaryLines.AL_AH = PrimaryHeader.AH_PK
WHERE PrimaryHeader.AH_PK IN (SELECT value from @TransactionHeaderPKs)
GROUP BY AH_PK, AH_Ledger, AL_JH, AL_AC, AL_GB, AL_GE
OPTION (RECOMPILE);

SELECT DISTINCT #GroupedPrimaryLines.AH_PK,
	SecondaryOrgHeader.OH_Code,
	SecondaryOrgHeader.OH_FullName
FROM #GroupedPrimaryLines
INNER JOIN dbo.AccTransactionLines AS SecondaryLines ON
	SecondaryLines.AL_AC = #GroupedPrimaryLines.AL_AC
	AND SecondaryLines.AL_JH = #GroupedPrimaryLines.AL_JH
	AND SecondaryLines.AL_GB = #GroupedPrimaryLines.AL_GB
	AND SecondaryLines.AL_GE = #GroupedPrimaryLines.AL_GE
INNER JOIN dbo.AccTransactionHeader AS SecondaryHeader ON 
	SecondaryHeader.AH_PK = SecondaryLines.AL_AH
	AND SecondaryHeader.AH_Ledger != #GroupedPrimaryLines.AH_Ledger
	AND (SecondaryHeader.AH_Ledger = 'AR' OR SecondaryHeader.AH_Ledger = 'AP') 
	AND (SecondaryHeader.AH_TransactionType = 'INV' OR SecondaryHeader.AH_TransactionType = 'CRD')
INNER JOIN dbo.OrgHeader AS SecondaryOrgHeader ON
	SecondaryOrgHeader.OH_PK = SecondaryHeader.AH_OH
OPTION (RECOMPILE)

DROP TABLE #GroupedPrimaryLines";

		void LoadRelatedTransactionDebtors(ZGuid pk)
		{
			bool load = false;
			if (!pk.IsEmpty)
			{
				if (relatedTransactionsDictionary != null && !relatedTransactionsDictionary.ContainsKey(pk))	//loadRelatedTransactionDebtors called once before but PK not in dictionary
				{
					relatedTransactions = new DynamicBusinessObjectCollection(Factory);
					load = true;
				}
				else if (relatedTransactionsDictionary == null)	//loadRelatedTransactionDebtors hasn't yet been called or is reset
				{
					relatedTransactionsDictionary = new Dictionary<ZGuid, List<RelatedTransactionValue>>();
					relatedTransactions = new DynamicBusinessObjectCollection(Factory);
					load = true;
				}
			}
			else if (pk.IsEmpty && relatedTransactions == null)	//loadRelatedTransactionDebtors hasn't been called or is reset
			{
				relatedTransactionsDictionary = new Dictionary<ZGuid, List<RelatedTransactionValue>>();
				relatedTransactions = new DynamicBusinessObjectCollection(Factory);
				load = true;
			}

			if (load)
			{
				ZSqlParameterCollection parameters = new ZSqlParameterCollection();
				parameters.Add(ZSqlParameter.New("@TransactionHeaderPKs", (pk.IsEmpty ? this.Select(x => x.PK).ToArray() : new ZGuid[] { pk }), AccTransactionHeaderSchema.PK, true));
				relatedTransactions.Load(RelatedTransactionDebtorsQuery, parameters);

				foreach (DynamicBusinessObject bo in relatedTransactions)
				{
					ZGuid key = (ZGuid)bo[AccTransactionHeaderSchema.Constants.PK];
					RelatedTransactionValue value = new RelatedTransactionValue((ZString)bo[OrgHeaderSchema.Constants.OH_Code], (ZString)bo[OrgHeaderSchema.Constants.OH_FullName]);
					List<RelatedTransactionValue> list;

					if (relatedTransactionsDictionary.TryGetValue(key, out list))
					{
						if (!list.Contains(value))
						{
							list.Add(value);
						}
					}
					else
					{
						list = new List<RelatedTransactionValue>();
						list.Add(value);
						relatedTransactionsDictionary.Add(key, list);
					}
				}
			}
		}

		bool containsPK(ZGuid pk)
		{
			foreach (TransactionHeader header in this)
			{
				if (header.PK == pk)
				{
					return true;
				}
			}
			return false;
		}

		public ZString RelatedTransactionDebtors(ZGuid pK)
		{
			if (containsPK(pK))
			{
				LoadRelatedTransactionDebtors(ZGuid.Empty);	//load all related debtors for each header in the collection
			}
			else
			{
				LoadRelatedTransactionDebtors(pK);	//load related debtor for a single PK
			}

			ZString result = ZString.Empty;
			List<RelatedTransactionValue> related;

			if (relatedTransactionsDictionary.TryGetValue(pK, out related))
			{
				foreach (RelatedTransactionValue value in related)
				{
					if (result != ZString.Empty)
					{
						result += ", ";
					}
					result += value.Code + ":" + value.FullName;
				}
			}
			return result;
		}

		public ZString RelatedClaimStatus(ZGuid pK)
		{
			ZString result = ZString.Empty;
			var relatedClaimsForStatus = RelatedClaims(pK);

			if (relatedClaimsForStatus != null)
			{
				result = string.Join(", ", relatedClaimsForStatus.Select(x => x.Status).Distinct());
			}

			return result;
		}

		public ZString RelatedClaimQueryNumber(ZGuid pK)
		{
			ZString result = ZString.Empty;
			var relatedClaimsForQueryNumber = RelatedClaims(pK);

			if (relatedClaimsForQueryNumber != null)
			{
				result = string.Format(CultureInfo.InvariantCulture, "{0}{1}", string.Join(", ", relatedClaimsForQueryNumber.Select(x => x.ReferenceNumber).Take(5)), relatedClaims.Count > 5 ? (NoResString)"…" : string.Empty);
			}

			return result;
		}

		IEnumerable<RelatedClaimValue> RelatedClaims(ZGuid pK)
		{
			if (containsPK(pK))
			{
				loadRelatedClaims(ZGuid.Empty);	//load all  in the collection
			}
			else
			{
				loadRelatedClaims(pK);	//load related claim for a single PK
			}
			List<RelatedClaimValue> result;
			if (relatedClaimsDictionary.TryGetValue(pK, out result))
			{
				return result;
			}
			return null;
		}

		void loadRelatedClaims(ZGuid pk)
		{
			bool load = false;
			if (!pk.IsEmpty)
			{
				if (relatedClaimsDictionary == null) //loadRelatedClaim hasn't yet been called or is reset
				{
					relatedClaimsDictionary = new Dictionary<ZGuid, List<RelatedClaimValue>>();
				}
				if (!relatedClaimsDictionary.ContainsKey(pk))	//loadRelatedClaim called once before but PK not in dictionary
				{
					relatedClaims = new DynamicBusinessObjectCollection(Factory);
					load = true;
				}
			}
			else if (pk.IsEmpty && relatedClaims == null)	//loadRelatedClaim hasn't been called or is reset
			{
				relatedClaimsDictionary = new Dictionary<ZGuid, List<RelatedClaimValue>>();
				relatedClaims = new DynamicBusinessObjectCollection(Factory);
				load = true;
			}

			if (load)
			{
#pragma warning disable CW1161
				ZString query = "SELECT " + AccQueryClaimSchema.Constants.AY_AH + (NoResString)", " + AccQueryClaimSchema.Constants.AY_QueryClaimStatus + ", " + AccQueryClaimSchema.Constants.AY_QueryClaimReference
													+ " FROM " + AccQueryClaimSchema.Constants.SqlSchemaName + "." + AccQueryClaimSchema.Constants.TableName
													+ " JOIN @TVP_U uniquePKs ON uniquePKs.[value] = " + AccQueryClaimSchema.Constants.TableName + "." + AccQueryClaimSchema.Constants.AY_AH;
#pragma warning restore CW1161

				List<ZGuid> headers = new List<ZGuid>();

				if (pk.IsEmpty)
				{
					headers.AddRange(this.Select(x => x.PK));
				}
				else
				{
					headers.Add(pk);
				}

				ZSqlParameter headerParam = ZSqlParameter.New("@TVP_U", headers.ToArray(), AccQueryClaimSchema.AY_AH, true);

				relatedClaims.Load(query, new ZSqlParameter[] { headerParam });

				foreach (DynamicBusinessObject bo in relatedClaims)
				{
					ZGuid key = (ZGuid)bo[AccQueryClaimSchema.Constants.AY_AH];
					RelatedClaimValue value = new RelatedClaimValue((ZString)bo[AccQueryClaimSchema.Constants.AY_QueryClaimStatus], (ZString)bo[AccQueryClaimSchema.Constants.AY_QueryClaimReference]);
					List<RelatedClaimValue> list;

					if (relatedClaimsDictionary.TryGetValue(key, out list))
					{
						if (!list.Contains(value))
						{
							list.Add(value);
						}
					}
					else
					{
						list = new List<RelatedClaimValue>();
						list.Add(value);
						relatedClaimsDictionary.Add(key, list);
					}
				}
			}
		}

		public string[] RelatedTransactionDebtorsCodes(ZGuid pK)
		{
			if (containsPK(pK))
			{
				LoadRelatedTransactionDebtors(ZGuid.Empty);	//load all related debtors for each header in the collection
			}
			else
			{
				LoadRelatedTransactionDebtors(pK);	//load related debtor for a single PK
			}

			List<string> result = new List<string>();
			List<RelatedTransactionValue> related;

			if (relatedTransactionsDictionary.TryGetValue(pK, out related))
			{
				foreach (RelatedTransactionValue value in related)
				{
					result.Add(value.Code);
				}
			}
			return result.ToArray();
		}

		public void ResetRelatedTransactionsCollection()
		{
			relatedTransactions = null;
			relatedTransactionsDictionary = null;
		}

		public void ResetRelatedClaimsCollection()
		{
			relatedClaims = null;
			relatedClaimsDictionary = null;
		}

		DynamicBusinessObjectCollection relatedClaims;

		Dictionary<ZGuid, List<RelatedClaimValue>> relatedClaimsDictionary;

		DynamicBusinessObjectCollection relatedTransactions;

		Dictionary<ZGuid, List<RelatedTransactionValue>> relatedTransactionsDictionary;

		class RelatedTransactionValue
		{
			public ZString Code;
			public ZString FullName;

			public RelatedTransactionValue(ZString code, ZString fullName)
			{
				Code = code;
				FullName = fullName;
			}
		}

		class RelatedClaimValue
		{
			public ZString Status;
			public ZString ReferenceNumber;

			public RelatedClaimValue(ZString status, ZString referenceNumber)
			{
				Status = status;
				ReferenceNumber = referenceNumber;
			}
		}
	}
}
