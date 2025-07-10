using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.CashBook
{
	public class BankReconTransCollection : BusinessObjectCollection<BankReconTransaction>
	{
		public BankReconTransCollection(BusinessObjectFactory factory, ZGuid bankPK)
			: base(factory)
		{
			fBankPK = bankPK;
		}

		protected override bool AllowNewCore
		{
			get
			{
				return false;
			}
		}

		#region Implementation

		protected readonly ZGuid fBankPK;

		protected override ZQuery CreateRelationshipFilter()
		{
			return new ZQuery(AccTransactionHeaderSchema.AH_AB, fBankPK);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		const int CommandTimeOutInSeconds = 600;

		public override void Load(ZQuery alternativeAdditionalFilter)
		{
			var columnList = CargoWise.Schema.Schema.CsvColumnList(AccTransactionHeaderSchema.Instance);

			var sqlText = string.Format(@"
SELECT AH_PK
FROM
	(
		SELECT {0}
		FROM dbo.AccTransactionHeader
			WHERE 
			(
				AH_Ledger = @LedgerCB AND AH_TransactionType = @TransactionTypeRCB
			) 
			and AH_GC = @CompanyPK
			
		UNION ALL

		SELECT {0}
		FROM dbo.AccTransactionHeader
			WHERE 
			(
				AH_TransactionType in (@TransactionTypePAY, @TransactionTypeDPY) and  AH_ReceiptType <> @ReceiptTypeDDL and AH_ReceiptType <> @ReceiptTypeDDR and AH_ReceiptType <> @ReceiptTypeEND
			) 
			and AH_GC = @CompanyPK
			
		UNION ALL

		SELECT {0}
		FROM dbo.AccTransactionHeader
			WHERE 
			(
				(AH_TransactionType in (@TransactionTypePAY, @TransactionTypeDPY) and AH_ReceiptType = @ReceiptTypeDDR and AH_ReceiptBatchNo <> '') 
			) 
			and AH_GC = @CompanyPK
			
		UNION ALL

		SELECT {0}
		FROM dbo.AccTransactionHeader
			WHERE 
			(
				AH_TransactionType = @TransactionTypeORC 
			) 
			and AH_GC = @CompanyPK
			
		UNION ALL

		SELECT {0}
		FROM dbo.AccTransactionHeader
			WHERE 
			(
				AH_TransactionType = @TransactionTypeOPY 
			) 
			and AH_GC = @CompanyPK
			
		UNION ALL

		SELECT {0}
		FROM dbo.AccTransactionHeader
			WHERE 
			(
				(AH_TransactionType = @TransactionTypeDDB and AH_IsCancelled = 0 and AH_ReceiptType <> @ReceiptTypeNRB) 
			) 
			and AH_GC = @CompanyPK
			
		UNION ALL

		SELECT {0}
		FROM dbo.AccTransactionHeader
			WHERE 
			(
				(AH_TransactionType = @TransactionTypeTRF and AH_Ledger = @LedgerCB)
			)
			and AH_GC = @CompanyPK
	) T", columnList);
			ZSqlParameterCollection parameters = new ZSqlParameterCollection();
			parameters.Add("@CompanyPK", Env.CurrentCompany.PK, AccTransactionHeaderSchema.AH_GC);
			parameters.Add("@LedgerCB", LedgerTypes.CashBook, AccTransactionHeaderSchema.AH_Ledger);
			parameters.Add("@TransactionTypeRCB", TransactionTypes.ReceiptBatch, AccTransactionHeaderSchema.AH_TransactionType);
			parameters.Add("@TransactionTypePAY", TransactionTypes.Payment, AccTransactionHeaderSchema.AH_TransactionType);
			parameters.Add("@TransactionTypeDPY", TransactionTypes.DirectPayment, AccTransactionHeaderSchema.AH_TransactionType);
			parameters.Add("@TransactionTypeORC", TransactionTypes.OpeningReceipt, AccTransactionHeaderSchema.AH_TransactionType);
			parameters.Add("@TransactionTypeOPY", TransactionTypes.OpeningPayment, AccTransactionHeaderSchema.AH_TransactionType);
			parameters.Add("@TransactionTypeDDB", TransactionTypes.DDRBatch, AccTransactionHeaderSchema.AH_TransactionType);
			parameters.Add("@TransactionTypeTRF", TransactionTypes.Transfer, AccTransactionHeaderSchema.AH_TransactionType);
			parameters.Add("@ReceiptTypeDDL", ReceiptTypes.DirectDebitLine, AccTransactionHeaderSchema.AH_ReceiptType);
			parameters.Add("@ReceiptTypeDDR", ReceiptTypes.DirectDebit, AccTransactionHeaderSchema.AH_ReceiptType);
			parameters.Add("@ReceiptTypeEND", ReceiptTypes.eNettDirectDebit, AccTransactionHeaderSchema.AH_ReceiptType);
			parameters.Add("@ReceiptTypeNRB", ReceiptTypes.NonRolledUpBatch, AccTransactionHeaderSchema.AH_ReceiptType);

			ZQuery additionalFilter = CreateRelationshipFilter();
			additionalFilter.AddToFilter(alternativeAdditionalFilter);

			string whereClause = additionalFilter.GetAsWhereAndOrderByClause(false);
			if (!string.IsNullOrEmpty(whereClause))
			{
				sqlText += whereClause;
				parameters.AddRange(additionalFilter.Params);
			}

			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Factory.GetCachedReadOnlyFactory());
			using (new AccountingUtils.CommandTimeoutInitializer(CommandTimeOutInSeconds))
			{
				collection.Load(sqlText, parameters);
			}

			if (collection.Count > 0)
			{
				var chunkedDBOs = AccountingUtils.ChunksOf(collection, AccountingUtils.ChunkBatchSize);
				foreach (IList<DynamicBusinessObject> groupOfDBOs in chunkedDBOs)
				{
					List<ZGuid> zguids = new List<ZGuid>();
					foreach (DynamicBusinessObject dynamicBizo in groupOfDBOs)
					{
						zguids.Add((ZGuid)dynamicBizo[AccTransactionHeaderSchema.PK]);
					}
					BankReconTransaction[] transactions = Factory.Load<BankReconTransaction>(new ZQuery(AccTransactionHeaderSchema.PK, zguids));
					AddRange(transactions);
				}
			}
		}

		#endregion
	}
}
