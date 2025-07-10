using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.DB.Sql;

namespace Enterprise.DataTransfer.Native.DB
{
	public interface IRowRepository
	{
		DataRow New(Table table);
		DataRow Create(Table table, Guid id);
		DataRow Create(Table table);
		void Delete(DataRow row);
		DataRow Show(Criteria criteria, Table table);
		DataRow Show(IEnumerable<Criteria> criterias, Table table);
		DataRow Show(Guid pk, Table table);
		DataRow[] Load(string tableName, ZQuery filterQuery);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		IList<DataRow> LoadMany(IList<IEnumerable<Criteria>> criterias, Table table);

		void Save();
	}
}