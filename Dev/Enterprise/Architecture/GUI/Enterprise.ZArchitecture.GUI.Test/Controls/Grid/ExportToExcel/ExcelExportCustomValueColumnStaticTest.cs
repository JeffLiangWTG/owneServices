using System;
using System.Drawing;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Excel.Testing
{
	sealed class ExcelExportCustomValueColumnStaticTest : TestCaseWithDummy
	{
		public void TestStaticConctractor()
		{
			var description = DummyCustomValueImplementation.DescriptionForTest;
			ExcelExportCustomValueColumn testColumn;

			var withAll = new DummyCustomValueImplementation();
			var customValueSupport = withAll as IExcelExportCustomValue;

			testColumn = ExcelExportCustomValueColumn.New(customValueSupport);

			AssertNotNull("TestColumn should be not null", testColumn);
			Assertion.AssertEquals("Type of Column shoud be ExcelExportCustomValueColumn", typeof(ExcelExportCustomValueColumn), testColumn.GetType());
			Assertion.AssertEquals("Coulumn Description should be " + description, description, testColumn.Description);
			Assertion.AssertEquals("Column Comment should be " + DummyCustomValueImplementation.CommentForTest, "", testColumn.GetComment(BizObj));
			Assertion.AssertEquals("Color should be nothing (nullable)", null, testColumn.GetColor(BizObj));

			testColumn = ExcelExportCustomValueColumn.New(customValueSupport, withAll);

			AssertNotNull("TestColumn should be not null", testColumn);
			Assertion.AssertEquals("Type of Column shoud be ExcelExportCustomValueColumn", typeof(ExcelExportCustomValueColumn), testColumn.GetType());
			Assertion.AssertEquals("Column Description should be " + description, description, testColumn.Description);
			Assertion.AssertEquals("Column Comment should be " + DummyCustomValueImplementation.CommentForTest, DummyCustomValueImplementation.CommentForTest, testColumn.GetComment(BizObj));
			Assertion.AssertEquals("Column Color should be " + Color.Red.ToString(), Color.Red, testColumn.GetColor(BizObj));

			try
			{
				testColumn = ExcelExportCustomValueColumn.New(null, new DummyCommentAndColorImplementation());
			}
			catch (ArgumentException e)
			{
				AssertEquals("Should be proper Exception Message", "Your Grid Column must support IExcelExportCustomValue", e.Message);
			}
			catch (Exception)
			{
				Fail("Should be no other exceptions");
			}
		}

		DummyBusinessObject BizObj
		{
			get
			{
				if (fBizObj == null)
				{
					fBizObj = Factory.New<DummyBusinessObject>();
				}
				return fBizObj;
			}
		}

		DummyBusinessObject fBizObj;
	}
}
