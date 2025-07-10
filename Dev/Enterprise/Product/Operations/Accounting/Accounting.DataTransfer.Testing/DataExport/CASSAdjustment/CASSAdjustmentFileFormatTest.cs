using System;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;
using static Enterprise.Accounting.DataTransfer.CASSAdjustmentFileFormat;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	public class CASSAdjustmentFileFormatTest : FlatFileFormatTestCase
	{
		[ExpectException(typeof(NotSupportedException))]
		public new void TestGetClientExtensionReturnsValueForImport()
		{
			base.TestGetClientExtensionReturnsValueForImport();
		}

		protected override Enterprise.DataTransfer.Business.FlatFileFormat GetFlatFileFormat()
		{
			return new CASSAdjustmentFileFormat();
		}

		public void TestAdjustmentReasonCommentOnLineEqualsCommentFieldInFileByLength()
		{
			AssertEquals(CASSAdjustmentFileLineRecord.Comment, new CASSCostExportLine(Factory, CASSCostLineType.Default).AdjustmentReasonCommentInfo.MaxLength);
		}
	}
}
