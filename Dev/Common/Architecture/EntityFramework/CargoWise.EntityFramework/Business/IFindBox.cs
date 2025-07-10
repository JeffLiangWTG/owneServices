using System;
using System.Collections.Generic;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public interface ICodePropertyNameProvider
	{
		string GetCodePropertyName(Type type);
	}

	public interface IFindBoxListProvider
	{
		(string, bool) NearestMatch(string code, bool explicitAutoComplete, int cursor);
		(string, bool) NearestMatchCore(string code, bool explicitAutoComplete);
		string DescriptionFromCode(string code);
		string DescriptionFromPrimaryKey(ZGuid pK);
		ZGuid PrimaryKeyFromCode(string code);
		BusinessObject GetBusinessObjectFromCode(string code);
		BusinessObject GetBusinessObjectFromCodeWithoutFilter(string code);
		IEnumerable<BusinessObject> GetBusinessObjectsFromCode(string code);
		IEnumerable<BusinessObject> GetBusinessObjectsFromCodeWithoutFilter(string code);
		string CodeFromPrimaryKey(ZGuid pK);
		IBusinessObjectCollection List { get; }
		bool AutoCompleteOnCommit { get; }

		ICodeDescription GetCustomCodeDescription(BusinessObject bizo);
	}

	public interface IFindBoxListProviderEx
	{
		IList<AlternateKey> AlternateKeys { get; }
		ZGuid PrimaryKeyFromAlternateKey(string columnName, IZType value);
		IZType AlternateKeyFromPrimaryKey(string columnName, ZGuid pk);
	}

	public interface IFindBoxListProviderDescriptionEx
	{
		string NearestDescriptionMatch(string description, bool explicitAutoComplete);
		string CodeFromDescription(string description);
	}

	public struct AlternateKey
	{
		public AlternateKey(string columnName, SchemaColumnType columnType, string columnDescription)
		{
			this.ColumnName = columnName;
			this.ColumnType = columnType;
			this.ColumnDescription = columnDescription;
		}

		public readonly string ColumnName;
		public readonly SchemaColumnType ColumnType;
		public readonly string ColumnDescription;
	}
}
