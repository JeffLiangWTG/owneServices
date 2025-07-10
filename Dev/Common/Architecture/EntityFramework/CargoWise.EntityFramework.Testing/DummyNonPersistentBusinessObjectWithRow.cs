using System;
using System.Data;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	public class DummyNonPersistentBusinessObjectWithRow : NonPersistentBusinessObject
	{
		public DummyNonPersistentBusinessObjectWithRow(BusinessObjectFactory factory)
			: base(factory, GetNewRow(factory))
		{
		}

		public static class Schema
		{
			public const string TableName = "DummyNonPersistentBusinessObjectWithRow";
			public const string PK = "Z0_PK";
			public const string Z0_String = "Z0_String";
		}

		protected static DataRow GetNewRow(BusinessObjectFactory factory)
		{
			DataTable table = ((INeedDataSet)factory).Data.Tables[Schema.TableName];
			if (table == null)
			{
				table = new NonPersistentDataTable();
				((INeedDataSet)factory).Data.Tables.Add(table);
			}
			return table.NewRow();
		}

		class NonPersistentDataTable : ZDataTable
		{
			public NonPersistentDataTable()
				: base(Schema.TableName)
			{
				DataColumn column;
				column = Columns.Add(Schema.PK, typeof(Guid));
				column.AllowDBNull = false;
				column = Columns.Add(Schema.Z0_String, typeof(System.String));
				column.MaxLength = 40;
				column.AllowDBNull = false;
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			//				((IBusinessObjectInternals)this).Row[Schema.PK] = ZGuid.NewZGuid().ToGuid();
			((IBusinessObjectInternals)this).Row[Schema.Z0_String] = "";
		}

		public override SchemaGuidColumn PKSchemaColumn
		{
			get { return new SchemaGuidColumn(CargoWise.Schema.Schema.GenericTableSchema, Schema.PK, 0, Guid.Empty, false); }
		}

		public virtual ZString Z0_String
		{
			get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(Z0_StringInfo)); }
			set
			{
				value = value.TrimEnd(' ');
				CheckMaximumLength(Z0_StringInfo, value);
				SetPropertyValue(Z0_StringInfo, value);
			}
		}

		public virtual ZPropertyInfo Z0_StringInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.Z0_String); }
		}

		public int OnSavingCount;
		public override void OnSaving()
		{
			OnSavingCount++;
			base.OnSaving();
		}

		public int OnSavedCount;
		public override void OnSaved(bool saveSucceeded)
		{
			OnSavedCount++;
			base.OnSaved(saveSucceeded);
		}

		public int OnFactorySavingCount;
		protected override void OnFactorySaving()
		{
			OnFactorySavingCount++;
			base.OnFactorySaving();
		}

		public int OnFactorySavedCount;
		protected override void OnFactorySaved(bool saveSucceeded)
		{
			OnFactorySavedCount++;
			base.OnFactorySaved(saveSucceeded);
		}
	}
}
