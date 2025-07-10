using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Excel.Testing
{
	class ExcelExportColumnGuidTest : ExcelExportColumnBaseTestCase
	{
		public override void TestGetValueForExport()
		{
			var relatedDummy = Factory.New<DummyBusinessObject>();
			relatedDummy.Z0_Code = "oppo";

			Dummy.Z0_Guid = ZGuid.Invalid;
			AssertFormattedResult(Dummy, ZString.Empty);

			Dummy.Z0_Guid = ZGuid.Empty;
			AssertFormattedResult(Dummy, ZString.Empty);

			Dummy.Z0_Guid = ZGuid.Missing;
			AssertFormattedResult(Dummy, ZString.Empty);

			Dummy.Z0_Guid = relatedDummy.PK;
			AssertFormattedResult(Dummy, (ZString)"oppo");

			Dummy.Z0_Guid = ZGuid.NewZGuid();
			AssertFormattedResult(Dummy, ZString.Empty); // no Code found
		}

		public void TestGetValueForExportUsingDisplayStyle()
		{
			Dummy.Z0_Guid = ZGuid.Invalid;

			var guidExcelExportColumn = base.ExcelExportColumn as ExcelExportGuidColumn;
			AssertEquals("DisplayStyle should default to CodeOnly", guidExcelExportColumn.DisplayStyle, OComboBoxDropDownStyle.CodeOnly);

			guidExcelExportColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;

			var relatedDummy = Factory.New<DummyBusinessObject>();
			relatedDummy.Z0_Code = "oppo";
			relatedDummy.Z0_Description = "Splatty Splatty Blah Blah";

			Dummy.Z0_Guid = ZGuid.Invalid;
			AssertFormattedResult(Dummy, ZString.Empty);

			Dummy.Z0_Guid = ZGuid.Empty;
			AssertFormattedResult(Dummy, ZString.Empty);

			Dummy.Z0_Guid = ZGuid.Missing;
			AssertFormattedResult(Dummy, ZString.Empty);

			Dummy.Z0_Guid = relatedDummy.PK;
			AssertFormattedResult(Dummy, (ZString)"Splatty Splatty Blah Blah");

			Dummy.Z0_Guid = ZGuid.NewZGuid();
			AssertFormattedResult(Dummy, ZString.Empty); // no Code found
		}

		public void TestWithNonZGuidValue()
		{
			ErrorReporter.Clear();
			AssertEquals("Precondition - ErrorReporter.LastMessageReported should be empty.", true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			Dummy.Z0_Description = "non-guid";
			AssertEquals("", GetNewExcelExportColumn().GetValueForExport(Dummy));
			//AssertEquals("The value 'non-guid' is not a valid type for formatting by an ExcelExportGuidColumn.", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		protected override ZString ExpectedFormatPattern
		{
			get { return ""; }
		}

		protected override ExcelExportColumnBase GetNewExcelExportColumn()
		{
			return new ExcelExportGuidColumn(DummyBizoSchema.Z0_Guid);
		}

		protected override ExcelExportColumnBase GetNewExcelExportColumn(IExcelExportCustomFunction baseColumn)
		{
			if ((baseColumn != null) && (baseColumn is IExcelExportCellComment))
			{
				return new ExcelExportGuidColumn(DummyBizoSchema.Z0_Guid, (IExcelExportCellComment)baseColumn, (IExcelExportCellColor)baseColumn);
			}
			else
			{
				return new ExcelExportGuidColumn(DummyBizoSchema.Z0_Guid);
			}
		}
	}
}
