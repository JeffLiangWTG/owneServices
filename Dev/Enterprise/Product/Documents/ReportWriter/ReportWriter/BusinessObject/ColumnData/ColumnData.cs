using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.ReportWriter
{
	public class ColumnData : AutoColumnData, IColumnHeadingRelatedData
	{
		public ColumnData(ReportBizObj reportBizObj)
			: base(reportBizObj.Factory)
		{
			this.reportBizObj = reportBizObj;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public new class Schema : AutoColumnData.Schema
		{
			public const string ColumnNumberString = "ColumnNumberString";
			public const string DisplayLabel = "DisplayLabel";
		}

		[List("ColumnHeadingsList")]
		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:ReportWriter.ColumnData|ColumnNumberString", ShortCaption = "No.", MediumCaption = "Number", Caption = "Column Number")]
		[BusinessObjectTestExclude]
		public ZString ColumnNumberString
		{
			get { return ColumnNumber.ToString(); }
			set
			{
				var oldValue = value;
				ColumnNumber = ZInt.ParseSafe(value, ZInt.Zero);
				ColumnNumberStringInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo ColumnNumberStringInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ColumnNumberString, (x) => ColumnNumberInfo); }
		}

		public ICodeDescriptionPairList ColumnHeadingsList
		{
			get { return reportBizObj.ColumnHeadingsList; }
		}

		public override ZInt ColumnNumber
		{
			get { return base.ColumnNumber; }
			set
			{
				var oldValue = value;
				base.ColumnNumber = value;
				if (!IsCopying && oldValue != value)
				{
					DisplayLabelInfo.RefreshBinding();
				}
			}
		}

		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:ReportWriter.ColumnData|DisplayLabel", Caption = "Display Label")]
		public ZString DisplayLabel
		{
			get { return reportBizObj.ColumnHeadingsList.GetDescriptionFromCode(ColumnNumber.ToString()); }
		}

		public ZPropertyInfo DisplayLabelInfo
		{
			get { return GetZPropertyInfo(Schema.DisplayLabel); }
		}

		readonly ReportBizObj reportBizObj;
	}
}
