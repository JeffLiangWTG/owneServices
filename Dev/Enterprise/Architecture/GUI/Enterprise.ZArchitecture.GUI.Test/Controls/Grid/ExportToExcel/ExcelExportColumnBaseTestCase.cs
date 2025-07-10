using System;
using System.Data;
using System.Drawing;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Excel.Testing
{
	public abstract class ExcelExportColumnBaseTestCase : TestCaseWithDummy
	{
		SchemaColumn GetSchemaColumn(BusinessObject bizo, string name)
		{
			return new SchemaStringColumn(ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema(bizo.TableName), name, 99, SqlDbType.VarChar, ZString.Empty, true, 100);
		}

		public void TestDoesNotSwallowExceptions()
		{
			var column = Excel.ExcelExportColumn.New(GetSchemaColumn(Dummy, "I_Dont_Exist"));

			AssertExceptionThrown<CouldNotGetValueForExportException>(() => column.GetValueForExport(Dummy));
		}

		public void TestGetValueFormattedFromPropertyDescriptor()
		{
			var column = Excel.ExcelExportColumn.New(GetSchemaColumn(Dummy, "blah"));
			column.PropertyDescriptor = new ConstantValuePropertyDescriptor(null, "blah", typeof(DummyBusinessObject), new ZString("HELLO world"));

			using (var gridColumn = new ZGridColumnStyleThatFormatsToLowerCase())
			{
				AssertEquals("hello world", column.GetValueForExport(Dummy, gridColumn));
			}
		}

		public void TestGetValueFormattedFromInvalidPropertyDescriptor()
		{
			var column = Excel.ExcelExportColumn.New(GetSchemaColumn(Dummy, "blah"));
			column.PropertyDescriptor = new DummyConstantValuePropertyDescriptor(null, "blah", typeof(DummyBusinessObject), new ZString("Test"));

			using (var gridColumn = new ZGridColumnStyleThatFormatsToLowerCase())
			{
				AssertExceptionThrown<CouldNotGetValueForExportException>(() => column.GetValueForExport(Dummy, gridColumn));
			}
		}

		public void TestGetValueFromPropertyDescriptorForDecimal()
		{
			var column = Excel.ExcelExportColumn.New(GetSchemaColumn(Dummy, "IamADecimal"));
			var decimalValue = new ZDecimal(3.25);
			column.PropertyDescriptor = new ConstantValuePropertyDescriptor(null, "IamADecimal", typeof(DummyBusinessObject), decimalValue);

			using (var gridColumn = new ZTextBoxColumnStyle(new ZTextBoxColumnStyleInfo()))
			{
				AssertEquals(decimalValue, column.GetValueForExport(Dummy, gridColumn));
			}
		}

		class ZGridColumnStyleThatFormatsToLowerCase : ZGridColumnStyle
		{
			public ZGridColumnStyleThatFormatsToLowerCase()
				: base(new ZTextBoxColumnStyleInfo())
			{
			}

			protected override string FormatValueObjectCore(object source, object propertyValue)
			{
				return ((ZString)propertyValue).ToLower();
			}
		}

		public void TestDescription()
		{
			ExcelExportColumn.Description = "SomeDescription";
			AssertEquals("Description should have assigned Value", "SomeDescription", ExcelExportColumn.Description);

			TestDefaultDescription();
		}

		public void TestFormat()
		{
			AssertNotNull("Should return not null as the CellFormat", ExcelExportColumn.GetFormat(new ZDecimal(6)));

			AssertEquals("Format Pattern should be as expected", ExpectedFormatPattern, ExcelExportColumn.GetFormat(new ZDecimal(6)).FormatPattern);

			TestDefaultFormat();
		}

		public virtual void TestComment()
		{
			var withComment = new DummyCommentImplementation();

			var columnWithComment = GetNewExcelExportColumn(withComment);
			var columnWithOutComment = GetNewExcelExportColumn();

			var bizObj = GetBizObjForTest();

			AssertEquals("Comment should be as expected", "Test Comment", columnWithComment.GetComment(bizObj));
			AssertEquals("Comment should be empty", "", columnWithOutComment.GetComment(bizObj));
		}

		public virtual void TestColor()
		{
			var withComment = new DummyCommentImplementation();

			var columnWithColor = GetNewExcelExportColumn(withComment);
			var columnWithOutColor = GetNewExcelExportColumn();

			var bizObj = GetBizObjForTest();

			AssertEquals("Color should be as expected", Color.Red, columnWithColor.GetColor(bizObj));
			AssertEquals("Color should be White, because it is our DefaultCellColor", null, columnWithOutColor.GetColor(bizObj));
		}

		#region Implementation

		protected virtual void TestDefaultDescription()
		{
		}

		protected virtual void TestDefaultFormat()
		{
		}

		protected virtual ExcelExportColumnBase GetNewExcelExportColumn()
		{
			return GetNewExcelExportColumn(null);
		}

		protected abstract ExcelExportColumnBase GetNewExcelExportColumn(IExcelExportCustomFunction baseColumn);

		protected abstract ZString ExpectedFormatPattern { get; }

		#region TestGetValueForExport

		public abstract void TestGetValueForExport();

		public void TestGetValueForExportWithSupoerException()
		{
			var bizo = GetBizObjForTest();
			ExcelExportColumnBase column = new ExcelExportColumnSuperExTest();

			try
			{
				AssertExceptionThrown(typeof(OutOfMemoryException), () => column.GetValueForExport(bizo));
			}
			catch (OutOfMemoryException)
			{
				// Prevent throwing critical exception further
			}
		}

		class ExcelExportColumnSuperExTest : ExcelExportColumnBase
		{
			protected override string GetDescription()
			{
				throw new NotImplementedException();
			}

			protected override IZType GetValueForExportCore(BusinessObject bizObj)
			{
				throw new OutOfMemoryException();
			}

			public override CellFormat GetFormat(IZType value)
			{
				throw new NotImplementedException();
			}
		}

		#region TestExportValueFromAutoGeneratedColumn

		public void TestExportValueFromAutoGeneratedColumn()
		{
			var collection = new MasterDummyCollection(Factory);
			var dummy = collection.AddNew();

			var childDummy = dummy.ReferenceableDummies.AddNew();
			childDummy.Z0_Code = "XYZ";
			childDummy.Z0_Description = "Something else";

			dummy.DummyReferenceableByDescCode = childDummy.Z0_Code;

			using (var form = new ZForm(dummy))
			using (var grid = new ZGrid())
			{
				grid.ColumnStyles.Add(new ZCodeFindBoxColumnStyleInfo { ColumnName = "DummyReferenceableByDescCode" });
				form.Controls.Add(grid);

				grid.SetDataBinding(collection, "");
				AssertEquals(2, grid.Columns.Count);

				var gridColumn1 = grid.Columns.First();
				var column1 = Excel.ExcelExportColumn.New(GetSchemaColumn(dummy, gridColumn1.ColumnName));
				var style1 = (ZGridColumnStyle)gridColumn1.ColumnStyle;

				column1.PropertyDescriptor = style1.PropertyDescriptor;
				AssertEquals("XYZ", column1.GetValueForExport(dummy, style1));

				var gridColumn2 = grid.Columns.Skip(1).First();
				var column2 = Excel.ExcelExportColumn.New(GetSchemaColumn(dummy, gridColumn2.ColumnName));
				var style2 = (ZGridColumnStyle)gridColumn2.ColumnStyle;

				column2.PropertyDescriptor = style2.PropertyDescriptor;
				AssertEquals("Something else", column2.GetValueForExport(dummy, style2));
			}
		}

		[DescriptionProperty(DummyBizoSchema.Constants.Z0_Description, CanBeReferencedBy = true)]
		class DummyReferenceableByDesc : DummyBusinessObject
		{
			public DummyReferenceableByDesc(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		}

		class DummyReferenceableByDescCollection : BusinessObjectCollection<DummyReferenceableByDesc>
		{
			public DummyReferenceableByDescCollection(BusinessObjectFactory factory) : base(factory) { }
		}

		class DummyWithDescReferenceableChild : DummyBusinessObject
		{
			public DummyWithDescReferenceableChild(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			[List("ReferenceableDummies")]
			public ZString DummyReferenceableByDescCode
			{
				get { return dummyReferenceableByDescCode; }
				set
				{
					SetNonPersistentPropertyValue(DummyReferenceableByDescCodeInfo, ref dummyReferenceableByDescCode, value);
				}
			}
			ZString dummyReferenceableByDescCode;

			public ZPropertyInfo DummyReferenceableByDescCodeInfo
			{
				get
				{
					return GetZPropertyInfo(nameof(DummyReferenceableByDescCode));
				}
			}

			public DummyReferenceableByDescCollection ReferenceableDummies
			{
				get
				{
					if (referenceableDummies == null)
					{
						referenceableDummies = new DummyReferenceableByDescCollection(Factory);
						RegisterEditableChildObject(referenceableDummies);
					}
					return referenceableDummies;
				}
			}
			DummyReferenceableByDescCollection referenceableDummies;
		}

		class MasterDummyCollection : BusinessObjectCollection<DummyWithDescReferenceableChild>
		{
			public MasterDummyCollection(BusinessObjectFactory factory) : base(factory) { }
		}

		#endregion

		#endregion

		protected virtual DummyBusinessObject GetBizObjForTest()
		{
			return Factory.New<DummyBusinessObject>();
		}

		protected ExcelExportColumnBase ExcelExportColumn
		{
			get { return fExcelExportColumn ?? (fExcelExportColumn = GetNewExcelExportColumn()); }
		}

		ExcelExportColumnBase fExcelExportColumn;

		protected void AssertFormattedResult(BusinessObject bizObj, IZType expectedValueFormattedForExport)
		{
			AssertFormattedResult(bizObj, expectedValueFormattedForExport, ExcelExportColumn);
		}

		protected void AssertFormattedResult(BusinessObject bizObj, IZType expectedValueFormattedForExport, ExcelExportColumnBase column)
		{
			AssertEquals(expectedValueFormattedForExport, column.GetValueForExport(bizObj));
		}

		#region DummyCommentImplementation

		protected class DummyCommentImplementation : IExcelExportCellComment, IExcelExportCellColor
		{
			#region IExcelExportCellComment Members

			public ZString GetComment(BusinessObject bizObj)
			{
				return CommentForTest;
			}

			#endregion

			public string CommentForTest = "Test Comment";

			#region IExcelExportCellColor Members

			public Color? GetCustomColor(BusinessObject bizObj)
			{
				return Color.Red;
			}

			#endregion
		}

		public class DummyConstantValuePropertyDescriptor : ConstantValuePropertyDescriptor
		{
			public DummyConstantValuePropertyDescriptor(KPropertyDescriptorCollection collection, string propertyName, Type componentType, object constantValue) : base(collection, propertyName, componentType, constantValue)
			{
			}

			protected override object GetValueCore(object component)
			{
				throw new InvalidCastException();
			}
		}

		#endregion

		#endregion
	}
}
