using System;
using System.Collections.Generic;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	public interface IExternalFetchHintSupporter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		void AddTableFetchHintCreator(ITableSchema tableSchema, Func<IColumnIndexer, IEnumerable<IFetchHint>> getFetchHints, bool applyToExistingRows = false);
		IDisposable SetupCreator();
		void AddFetchHint(IFetchHint fetchHint);
		void AddRelatedTableHints(string tableName, IColumnIndexer[] dataRows);
	}
}
