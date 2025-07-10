using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class GridColumnReferenceTest : TestCase
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new GridColumnReference<ZTextBoxColumnStyleInfo>(null, 10, null));
			AssertExceptionThrown<ArgumentException>(() => new GridColumnReference<ZTextBoxColumnStyleInfo>(null, 10));
			AssertNoExceptionThrown(() => new GridColumnReference<ZTextBoxColumnStyleInfo>("SomeColumn", 10));
		}

		public void TestCreateGridColumnInfo()
		{
			IGridColumnReference columnReference = new GridColumnReference<ZTextBoxColumnStyleInfo>("SomeProperty", 10);
			var columnInfo = columnReference.CreateGridColumnInfo();
			AssertNotNull(columnInfo);

			CombineAssertions("When No configurator is specified", () =>
			{
				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "SomeProperty", columnInfo.ColumnName);
				AssertEquals("Width", 10, columnInfo.Width);
			});

			columnReference = new GridColumnReference<ZTextBoxColumnStyleInfo>("AnotherProperty", 10, c => c.IsMandatory = true);
			columnInfo = columnReference.CreateGridColumnInfo();
			AssertNotNull(columnInfo);
			CombineAssertions("When configurator is specified", () =>
			{
				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "AnotherProperty", columnInfo.ColumnName);
				AssertEquals("Width", 10, columnInfo.Width);
			});
		}

		public void TestColumnName()
		{
			IGridColumnReference columnReference = new GridColumnReference<ZTextBoxColumnStyleInfo>("SomeProperty", 10);
			AssertEquals("ColumnName", "SomeProperty", columnReference.ColumnName);
		}
	}
}
