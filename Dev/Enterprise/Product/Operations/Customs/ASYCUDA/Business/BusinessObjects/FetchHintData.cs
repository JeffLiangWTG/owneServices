using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class FetchHintData
	{
		public void AddForeignKey(SchemaGuidColumn column, SchemaGuidColumn parentColumn, Type type = null)
		{
			foreignKeys = foreignKeys ??= new List<FetchHintColumn<SchemaGuidColumn>.FetchHintColumForeignKey>();
			foreignKeys.Add(new FetchHintColumn<SchemaGuidColumn>.FetchHintColumForeignKey(column, parentColumn, type));
		}

		public IEnumerable<FetchHintColumn<SchemaGuidColumn>.FetchHintColumForeignKey> ForeignKeys => foreignKeys?.ToArray() ?? Enumerable.Empty<FetchHintColumn<SchemaGuidColumn>.FetchHintColumForeignKey>();
		List<FetchHintColumn<SchemaGuidColumn>.FetchHintColumForeignKey> foreignKeys;

		public void AddNaturalKey(SchemaStringColumn column, SchemaStringColumn parentColumn, Type type = null)
		{
			naturalKeys = naturalKeys ??= new List<FetchHintColumn<SchemaStringColumn>.FetchHintColumNaturalKey>();
			naturalKeys.Add(new FetchHintColumn<SchemaStringColumn>.FetchHintColumNaturalKey(column, parentColumn, type));
		}

		public IEnumerable<FetchHintColumn<SchemaStringColumn>.FetchHintColumNaturalKey> NaturalKeys => naturalKeys?.ToArray() ?? Enumerable.Empty<FetchHintColumn<SchemaStringColumn>.FetchHintColumNaturalKey>();
		List<FetchHintColumn<SchemaStringColumn>.FetchHintColumNaturalKey> naturalKeys;
	}
}
