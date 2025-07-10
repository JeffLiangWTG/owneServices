using System;
using System.Data;
using System.Drawing;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Excel.Testing
{
	sealed class ExcelExportColumnTest : TestCaseWithDummy
	{
		public void TestPassword()
		{
			var bizo = BizObjForTest;
			var column = Excel.ExcelExportColumn.New(new SchemaStringColumn(ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema(bizo.TableName), "Z0_Password", 99, SqlDbType.VarChar, ZString.Empty, true, 100));
			column.PropertyDescriptor = new ConstantValuePropertyDescriptor(null, "Z0_Password", typeof(DummyBusinessObjectWithPassword), new ZString("HELLO world"));

			using (var gridColumn = new ZTextBoxColumnStyle(new ZTextBoxColumnStyleInfo()))
			{
				AssertEquals("***", column.GetValueForExport(bizo, gridColumn));
			}
		}

		public void TestStaticConstructor()
		{
			var description = "Test Description";
			SchemaColumn testSchema = DummyBizoSchema.Z0_AnotherDate;
			var testSchemaToString = "DummyBizo|Z0_AnotherDate";

			var withComment = new DummyCommentAndColorImplementation();

			ExcelExportColumn testColumn;

			testColumn = ExcelExportColumn.New(testSchema);
			AssertColumn(testColumn, typeof(ExcelExportDateTimeColumn), testSchemaToString, "", null);

			testColumn = ExcelExportColumn.New(testSchema, description);
			AssertColumn(testColumn, typeof(ExcelExportDateTimeColumn), description, "", null);

			testColumn = ExcelExportColumn.New(testSchema, withComment);
			AssertColumn(testColumn, typeof(ExcelExportDateTimeColumn), testSchemaToString, DummyCommentAndColorImplementation.CommentForTest, Color.Red);

			testColumn = ExcelExportColumn.New(testSchema, description, withComment);
			AssertColumn(testColumn, typeof(ExcelExportDateTimeColumn), description, DummyCommentAndColorImplementation.CommentForTest, Color.Red);

			testColumn = ExcelExportColumn.New(new SchemaDateColumn(DummyBizoSchema.Instance, "Moo", 0, SqlDbType.Date, null, true));
			AssertType(typeof(ExcelExportDateColumn), testColumn);

			testColumn = ExcelExportColumn.New(DummyBizoSchema.Z0_Guid, description, withComment);
			AssertColumn(testColumn, typeof(ExcelExportGuidColumn), description, DummyCommentAndColorImplementation.CommentForTest, Color.Red);

			testColumn = ExcelExportColumn.New(DummyBizoSchema.Z0_Decimal, description, withComment);
			AssertColumn(testColumn, typeof(ExcelExportDecimalColumn), description, DummyCommentAndColorImplementation.CommentForTest, Color.Red);

			var multiColumn = new SchemaMultiColumn("Moo", DummyBizoSchema.Instance);
			testColumn = ExcelExportColumn.New(multiColumn, description, withComment);
			AssertColumn(testColumn, typeof(ExcelExportMultiColumn), description, DummyCommentAndColorImplementation.CommentForTest, Color.Red);
		}

		void AssertColumn(ExcelExportColumnBase column, Type columnType, string description, string comment, Color? colors)
		{
			AssertNotNull("ExcelExportColumn should be not null", column);
			AssertEquals("Type of Column shoud be " + columnType.ToString(), columnType, column.GetType());
			AssertEquals("Column Description should be " + description, description, column.Description);
			AssertEquals("Column Comment should be " + comment, comment, column.GetComment(BizObjForTest));
			AssertEquals("Column Color should be " + colors.ToString(), colors, column.GetColor(BizObjForTest));
		}

		DummyBusinessObjectWithPassword BizObjForTest
		{
			get
			{
				if (fBizObjForTest == null)
				{
					fBizObjForTest = Factory.New<DummyBusinessObjectWithPassword>();
				}
				return fBizObjForTest;
			}
		}

		internal class DummyBusinessObjectWithPassword : DummyBusinessObject
		{
			public DummyBusinessObjectWithPassword(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			[Password]
			public ZString Z0_Password { get; set; }

			public ZPropertyInfo Z0_PasswordInfo
			{
				get { return GetZPropertyInfo(nameof(Z0_Password)); }
			}
		}

		DummyBusinessObjectWithPassword fBizObjForTest;
	}
}
