using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Excel.Testing
{
	sealed class ExcelExportColumnMultiTest : ExcelExportColumnBaseTestCase
	{
		public override void TestGetValueForExport()
		{
			var dummy = Factory.New<DummyWithSchemaMultiColumn>();
			//SchemaMultiColumn schemaColumn = new SchemaMultiColumn("Moo", DummyBizoSchema.Instance);
			//ExcelExportColumn column = ExcelExportColumn;//.New(schemaColumn);

			dummy.Moo = "Some Text";
			AssertFormattedResult(dummy, (ZString)"Some Text");

			DummyBusinessObject dummy2 = Factory.New<DummyWithSchemaMultiColumn>();
			dummy2.Z0_Code = "leb";

			dummy.Moo = dummy2.PK.ToString();
			AssertFormattedResult(dummy, (ZString)"leb");
		}

		protected override ZString ExpectedFormatPattern
		{
			get { return ""; }
		}

		protected override ExcelExportColumnBase GetNewExcelExportColumn()
		{
			var multiColumn = new SchemaMultiColumn("Moo", DummyBizoSchema.Instance);
			return new ExcelExportMultiColumn(multiColumn);
		}

		#region DummyWithSchemaMultiColumn

		class DummyWithSchemaMultiColumn : DummyBusinessObject
		{
			public DummyWithSchemaMultiColumn(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			[RelatedBusinessObject("RelatedDummy")]
			[MaxLength(255)]
			public ZString Moo
			{
				get { return fMoo; }
				set { fMoo = value; }
			}

			public ZPropertyInfo MooInfo
			{
				get { return GetZPropertyInfo(nameof(Moo)); }
			}

			ZString fMoo;

			public override DummyBusinessObject RelatedDummy
			{
				get
				{
					try
					{
						var guid = new ZGuid(fMoo);
						return Factory.Load<DummyBusinessObject>(guid);
					}
					catch (FormatException)
					{
						return null;
					}
				}
			}
		}

		#endregion

		protected override ExcelExportColumnBase GetNewExcelExportColumn(IExcelExportCustomFunction baseColumn)
		{
			var multiColumn = new SchemaMultiColumn("Moo", DummyBizoSchema.Instance);
			if ((baseColumn != null) && (baseColumn is IExcelExportCellComment))
			{
				return new ExcelExportMultiColumn(multiColumn, (IExcelExportCellComment)baseColumn, (IExcelExportCellColor)baseColumn);
			}
			else
			{
				return new ExcelExportMultiColumn(multiColumn);
			}
		}
	}
}
