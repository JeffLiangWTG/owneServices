using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Module;

public class ASYCUDAManifestBillModuleCollectionFetchStrategy<T> : AsycudaCollectionFetchStrategy<ASYCUDAManifestBillModuleCollection<T>>
	where T : AsycudaBill
{
	public ASYCUDAManifestBillModuleCollectionFetchStrategy(ASYCUDAManifestBillModuleCollection<T> collection)
		: base(collection)
	{
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	protected override void FetchForViewAsycuda(BusinessObject[] businessObjects, TableColumn[] columns)
	{
		businessObjects = businessObjects.Where(x => x.IsInDatabase).ToArray();
		if (businessObjects.Length > 0)
		{
			var cusEntryNumRequiredFetchForView = false;
			var billCountryGenAddOnColumnRequiredFetchForView = false;
			var billGenAddOnColumnRequiredFetchForView = false;
			var billRegistrationNumRequiredFetchForView = false;
			var headerGenAddOnColumnRequiredFetchForView = false;
			var headerRegistrationNumRequiredFetchForView = false;
			var manifestBillRequiredFetchForView = false;
			var headerRequiredFetchForView = false;
			var stmALogRequiredFetchForView = false;

			var billCountryGenAddOnColumnsToFetch = new List<ZString>();
			var billGenAddOnColumnsToFetch = new List<ZString>();
			var headerGenAddOnColumnsToFetch = new List<ZString>();

			foreach (var tableColumn in columns)
			{
				if (!cusEntryNumRequiredFetchForView && IsCusEntryNumRelatedColumn(tableColumn.ColumnName))
				{
					cusEntryNumRequiredFetchForView = true;
				}
				if (IsBillCountryGenAddOnColumnRelatedColumn(tableColumn.ColumnName))
				{
					billCountryGenAddOnColumnsToFetch.Add(tableColumn.ColumnName);
				}
				if (!billRegistrationNumRequiredFetchForView && IsBillRegistrationNumRelatedColumn(tableColumn.ColumnName))
				{
					billRegistrationNumRequiredFetchForView = true;
				}
				if (IsBillGenAddOnColumnRelatedColumn(tableColumn.ColumnName))
				{
					billGenAddOnColumnsToFetch.Add(new ZString(tableColumn.ColumnName));
				}

				if (!headerRegistrationNumRequiredFetchForView && IsHeaderRegistrationNumRelatedColumn(tableColumn.ColumnName))
				{
					headerRegistrationNumRequiredFetchForView = true;
					headerRequiredFetchForView = true;
				}
				if (!manifestBillRequiredFetchForView && IsManifestBillRelatedColumn(tableColumn.ColumnName))
				{
					manifestBillRequiredFetchForView = true;
					headerRequiredFetchForView = true;
				}
				if (!headerRequiredFetchForView && IsHeaderRelatedColumn(tableColumn.ColumnName))
				{
					headerRequiredFetchForView = true;
				}
				if (!stmALogRequiredFetchForView && IsStmALogRelatedColumn(tableColumn.ColumnName))
				{
					stmALogRequiredFetchForView = true;
				}
			}
			if (billCountryGenAddOnColumnsToFetch.Count > 0)
			{
				billCountryGenAddOnColumnRequiredFetchForView = true;
			}
			if (billGenAddOnColumnsToFetch.Count > 0)
			{
				billGenAddOnColumnRequiredFetchForView = true;
			}
			if (headerGenAddOnColumnsToFetch.Count > 0)
			{
				headerGenAddOnColumnRequiredFetchForView = true;
			}

			var fetchForViewIsRequired = cusEntryNumRequiredFetchForView
										 || billCountryGenAddOnColumnRequiredFetchForView
										 || headerGenAddOnColumnRequiredFetchForView
										 || headerRegistrationNumRequiredFetchForView
										 || manifestBillRequiredFetchForView
										 || headerRequiredFetchForView
										 || stmALogRequiredFetchForView
										 || billGenAddOnColumnRequiredFetchForView
										 || billRegistrationNumRequiredFetchForView;

			if (fetchForViewIsRequired)
			{
				var billPks = new List<ZGuid>();
				foreach (AsycudaBill bill in businessObjects)
				{
					if (headerRequiredFetchForView
						|| headerGenAddOnColumnRequiredFetchForView
						|| headerRegistrationNumRequiredFetchForView
						|| manifestBillRequiredFetchForView)
					{
						billPks.Add(bill.PK);
					}
					AddCusEntryNumFetch(cusEntryNumRequiredFetchForView, bill.PK);
					AddRegistrationNumFetch(billRegistrationNumRequiredFetchForView, bill.PK);
					AddGenAddOnColumnFetch(billCountryGenAddOnColumnRequiredFetchForView, bill.PK, billCountryGenAddOnColumnsToFetch);
					AddGenAddOnColumnFetch(billGenAddOnColumnRequiredFetchForView, bill.PK, billGenAddOnColumnsToFetch);
					AddStmALogFetch(stmALogRequiredFetchForView, bill);
				}
				billPks = billPks.Distinct().ToList();
				if (billPks.Count > 0)
				{
					var headerPKs = Factory.Load<AsycudaBill>(new ZQuery(AsycudaBillSchema.PK, billPks)).Select(x => x.ABL_AMA).Distinct().ToList();
					headerPKs.ForEach(headerPK =>
					{
						AddHeaderFetch(headerRequiredFetchForView, headerPK);
						AddManifestBillFetch(manifestBillRequiredFetchForView, headerPK);
					});

					if (headerPKs.Count > 0 && (headerGenAddOnColumnRequiredFetchForView || headerRegistrationNumRequiredFetchForView))
					{
						foreach (var header in Factory.Load<AsycudaManifestHeader>(new ZQuery(AsycudaManifestHeaderSchema.PK, headerPKs)))
						{
							AddGenAddOnColumnFetch(headerGenAddOnColumnRequiredFetchForView, header.PK, headerGenAddOnColumnsToFetch);
							AddRegistrationNumFetch(headerRegistrationNumRequiredFetchForView, header.PK);
						}
					}
				}
			}
		}
	}

	BusinessObjectFactory Factory => Collection.Factory;

	void AddCusEntryNumFetch(bool fetchRequired, ZGuid parentID)
	{
		if (fetchRequired)
		{
			Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, parentID);
		}
	}

	void AddRegistrationNumFetch(bool fetchRequired, ZGuid parentID)
	{
		if (fetchRequired)
		{
			var query = new ZQuery(CusEntryNumSchema.CE_ParentID, parentID);
			query.AddToFilter(CusEntryNumSchema.CE_EntryType, Common.CusEntryNumberTypes.ASYCUDA.AsycudaRegistration);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, ZString.Empty);
			Factory.AddFetchHint(CusEntryNumSchema.Instance, query);
		}
	}

	void AddGenAddOnColumnFetch(bool fetchRequired, ZGuid parentID, IEnumerable<ZString> columnList)
	{
		if (fetchRequired)
		{
			foreach (var column in columnList)
			{
				var query = new ZQuery(GenAddOnColumnSchema.XA_ParentID, parentID);
				query.AddToFilter(GenAddOnColumnSchema.XA_Name, column);
				Factory.AddFetchHint(GenAddOnColumnSchema.Instance, query);
			}
		}
	}

	void AddStmALogFetch(bool fetchRequired, AsycudaBill country)
	{
		if (fetchRequired)
		{
			var statusDescriptionQuery = new ZQuery();
			statusDescriptionQuery.AddToFilter(StmALogSchema.SL_Table, (ZString)AsycudaBillSchema.Constants.TableName);
			statusDescriptionQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.MessageStatusChange.Code);
			statusDescriptionQuery.AddToFilter(StmALogSchema.SL_Parent, country.PK);
			statusDescriptionQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Like, "ERR - %");
			Factory.AddFetchHint(typeof(StmALog), statusDescriptionQuery);
		}
	}

	void AddHeaderFetch(bool fetchRequired, ZGuid headerPK)
	{
		if (fetchRequired)
		{
			Factory.AddFetchHint(AsycudaManifestHeaderSchema.PK, headerPK);
		}
	}

	void AddManifestBillFetch(bool fetchRequired, ZGuid headerPK)
	{
		if (fetchRequired)
		{
			var query = new ZQuery(AsycudaBillSchema.ABL_AMA, headerPK);
			query.AddToFilter(AsycudaBillSchema.ABL_BolType, AsycudaBill.ChildBolCode);
			Factory.AddFetchHint(AsycudaBillSchema.Instance, query);
		}
	}

	protected virtual bool IsBillCountryGenAddOnColumnRelatedColumn(ZString columnName)
	{
		return columnName == AsycudaBill.Schema.CustomsJobNumber;
	}

	bool IsBillGenAddOnColumnRelatedColumn(ZString columnName)
	{
		return billGenAddOnColumnList.Contains(columnName);
	}

	bool IsHeaderRegistrationNumRelatedColumn(ZString columnName)
	{
		return columnName == AsycudaManifestHeader.Schema.RegistrationStatus;
	}

	bool IsCusEntryNumRelatedColumn(ZString columnName)
	{
		return columnName == AsycudaBill.Schema.RegistrationDate || columnName == AsycudaBill.Schema.RegistrationNumber ||
			   columnName == AsycudaBill.Schema.CustomsEntryNumber || columnName == AsycudaBill.Schema.CustomsEntryNumberType ||
			   columnName.StartsWith("CusEntryNumber+", System.StringComparison.Ordinal);
	}

	bool IsBillRegistrationNumRelatedColumn(ZString columnName)
	{
		return columnName == AsycudaBill.Schema.RegistrationDate || columnName == AsycudaBill.Schema.RegistrationNumber;
	}

	bool IsManifestBillRelatedColumn(ZString columnName)
	{
		return columnName == "Header+AMA_MasterBill";
	}

	bool IsHeaderRelatedColumn(ZString columnName)
	{
		return columnName.StartsWith("Header+", System.StringComparison.Ordinal);
	}

	bool IsStmALogRelatedColumn(ZString columnName)
	{
		return columnName == AsycudaBill.Schema.StatusDescription;
	}

	readonly ZString[] billGenAddOnColumnList = {
		AsycudaBill.Schema.DiscountValue,
		AsycudaBill.Schema.DiscountValueCurrency,
		AsycudaBill.Schema.OtherChargesValue,
		AsycudaBill.Schema.OtherChargesValueCurrency
	};
}
