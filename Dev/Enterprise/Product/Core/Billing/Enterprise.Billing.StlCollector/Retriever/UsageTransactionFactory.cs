using System;
using System.Data;
using System.Text;
using CargoWise.Application;
using Enterprise.Billing.Integration;
using Enterprise.Integration.Billing;
using Enterprise.Integration.Licensing;

namespace Enterprise.Billing.StlCollector.Retriever
{
	public class UsageTransactionFactory : IStlTransactionFactory
	{
		readonly Lazy<IProductRegistrationKey> productKey = new Lazy<IProductRegistrationKey>(() => ObjectFactory.Get<IProductRegistration>()?.Key);

		public IStlTransaction CreateTransaction(IStlScript script, DataRow dataRow)
		{
			var itemCount = GetColumnValue<int>(dataRow, "ItemCount");
			if (itemCount <= 0)
			{
				return null;
			}

			var encoding = Encoding.GetEncoding(script.AdditionalRefsEncoding);
			var additionalRefsObj = encoding.GetString((byte[])dataRow["AdditionalRefs"]);
			var companyCode = GetColumnValue<string>(dataRow, "CompanyCode");
			var companyName = GetColumnValue<string>(dataRow, "CompanyName");
			var branchCode = GetColumnValue<string>(dataRow, "BranchCode");

			var ref1 = GetColumnValue<string>(dataRow, "TransactionReference01");
			var ref2 = GetColumnValue<string>(dataRow, "TransactionReference02");
			var ref3 = GetColumnValue<string>(dataRow, "TransactionReference03");
			var ref4 = GetColumnValue<string>(dataRow, "TransactionReference04");
			var ref5 = GetColumnValue<string>(dataRow, "TransactionGuidReference");
			var userCode = GetColumnValue<string>(dataRow, "UserCode");

			var transaction = new UsageTransaction()
			{
				UsageCount = itemCount,
				ServiceOccuredUTC = (script.DateType == StlDateType.DateTimeOffset) ? GetColumnValue<DateTimeOffset>(dataRow, "TransactionDateUtc").UtcDateTime : GetColumnValue<DateTime>(dataRow, "TransactionDateUtc"),
				EnterpriseCode = productKey.Value?.EnterpriseCode,
				ServerCode = productKey.Value?.ServerCode,
				Environment = productKey.Value?.DatabaseType,
				CompanyCode = string.IsNullOrEmpty(companyCode) ? null : companyCode,
				CompanyName = string.IsNullOrEmpty(companyName) ? null : companyName,
				BranchCode = string.IsNullOrEmpty(branchCode) ? null : branchCode,
				Reference1 = string.IsNullOrEmpty(ref1) ? null : ref1,
				Reference2 = string.IsNullOrEmpty(ref2) ? null : ref2,
				Reference3 = string.IsNullOrEmpty(ref3) ? null : ref3,
				Reference4 = string.IsNullOrEmpty(ref4) ? null : ref4,
				Reference5 = ref5,
				ClientStaffCode = userCode,
				UsageCode = script.Code,
				AdditionalRefs = string.IsNullOrEmpty(additionalRefsObj) ? null : additionalRefsObj
			};

			return transaction;
		}

		static T GetColumnValue<T>(DataRow row, string name)
		{
			if (!row.Table.Columns.Contains(name) || row[name] is DBNull)
			{
				return default;
			}

			return (T)row[name];
		}
	}
}
