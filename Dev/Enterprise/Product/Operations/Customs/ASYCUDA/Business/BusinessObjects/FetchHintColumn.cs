using System;
using CargoWise.Schema;

namespace Enterprise.Customs.ASYCUDA.Business;

public class FetchHintColumn<T>
	where T : SchemaColumn
{
	public FetchHintColumn(T column, T parentColumn, Type childType)
	{
		this.column = column;
		this.parentColumn = parentColumn;
		this.childType = childType;
	}

	public T Column => column;
	readonly T column;

	public T ParentColumn => parentColumn;
	readonly T parentColumn;

	public Type ChildType => childType;
	readonly Type childType;

	public class FetchHintColumForeignKey : FetchHintColumn<SchemaGuidColumn>
	{
		public FetchHintColumForeignKey(SchemaGuidColumn column, SchemaGuidColumn parentColumn, Type childType)
			: base(column, parentColumn, childType)
		{ }
	}

	public class FetchHintColumNaturalKey : FetchHintColumn<SchemaStringColumn>
	{
		public FetchHintColumNaturalKey(SchemaStringColumn column, SchemaStringColumn parentColumn, Type childType)
			: base(column, parentColumn, childType)
		{ }
	}
}
