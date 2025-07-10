using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ReportWriter
{
	public class Area : AutoArea, IReportBizObjProvider
	{
		public Area(ReportBizObj parent)
			: base(parent.Factory)
		{
			this.parent = parent;
		}

		[List("TypeList")]
		public override ZString Type
		{
			get { return base.Type; }
			set
			{
				var oldValue = Type;
				base.Type = value;
				if (!IsCopying && oldValue != Type)
				{
					ClearIrrelevantData();
				}
			}
		}

		public AreaTypeList TypeList
		{
			get { return Factory.GetCachedValue<AreaTypeList>(); }
		}

		public override ZString TypeDesc
		{
			get { return TypeList.GetDescriptionFromCode(Type); }
		}

		public RowDataCollection Rows
		{
			get
			{
				if (rows == null)
				{
					rows = new RowDataCollection(this);
					RegisterEditableChildObject(rows);
				}
				return rows;
			}
		}
		RowDataCollection rows;

		public GroupByColumnCollection GroupByColumns
		{
			get
			{
				if (groupByColumns == null)
				{
					groupByColumns = new GroupByColumnCollection(this);
					RegisterEditableChildObject(groupByColumns);
				}
				return groupByColumns;
			}
		}
		GroupByColumnCollection groupByColumns;

		protected override bool DataSource_ReadOnly
		{
			get { return Type != AreaTypeList.Codes.SectionBody; }
		}

		protected override bool Sticky_ReadOnly
		{
			get
			{
				return Type != AreaTypeList.Codes.SectionBody && Type != AreaTypeList.Codes.SectionHeader && Type != AreaTypeList.Codes.SectionPageHeader
					&& Type != AreaTypeList.Codes.GroupBy;
			}
		}

		protected override bool MaximumNumberOfRowsToShow_ReadOnly
		{
			get { return Type != AreaTypeList.Codes.SectionBody; }
		}

		protected override bool NumberOfRowsToShow_ReadOnly
		{
			get { return Type != AreaTypeList.Codes.SectionBody; }
		}

		protected override bool RowCountAMultipleOf_ReadOnly
		{
			get { return Type != AreaTypeList.Codes.SectionBody; }
		}

		protected override bool StartingRowToShow_ReadOnly
		{
			get { return Type != AreaTypeList.Codes.SectionBody; }
		}

		protected override bool StartFromSecondPage_ReadOnly
		{
			get { return Type != AreaTypeList.Codes.PageHeader; }
		}

		protected override bool ShowEvenWithNoData_ReadOnly
		{
			get { return Type != AreaTypeList.Codes.SectionHeader && Type != AreaTypeList.Codes.SectionFooter; }
		}

		protected override bool PageBreak_ReadOnly
		{
			get { return Type != AreaTypeList.Codes.SectionHeader && Type != AreaTypeList.Codes.GroupBy; }
		}

		protected override bool GroupTitle_ReadOnly
		{
			get { return Type != AreaTypeList.Codes.GroupBy; }
		}

		protected override bool KeepInSamePage_ReadOnly
		{
			get { return Type != AreaTypeList.Codes.GroupBy; }
		}

		protected override bool DontSplit_ReadOnly
		{
			get { return Type != AreaTypeList.Codes.DocumentFooter; }
		}

		void ClearIrrelevantData()
		{
			ClearIrrelevantData(DataSourceInfo);
			ClearIrrelevantData(StickyInfo);
			ClearIrrelevantData(MaximumNumberOfRowsToShowInfo);
			ClearIrrelevantData(NumberOfRowsToShowInfo);
			ClearIrrelevantData(RowCountAMultipleOfInfo);
			ClearIrrelevantData(StartingRowToShowInfo);
			ClearIrrelevantData(StartFromSecondPageInfo);
			ClearIrrelevantData(ShowEvenWithNoDataInfo);
			ClearIrrelevantData(PageBreakInfo);
			ClearIrrelevantData(GroupTitleInfo);
			ClearIrrelevantData(KeepInSamePageInfo);
			ClearIrrelevantData(DontSplitInfo);
			if (Type != AreaTypeList.Codes.GroupBy)
			{
				GroupByColumns.RemoveAndDeleteAll();
			}
		}

		void ClearIrrelevantData(ZPropertyInfo info)
		{
			if (info.ReadOnly && !info.Value.IsEmpty)
			{
				info.Value = info.DefaultValue;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		internal string GetConfigDetail()
		{
			switch (Type)
			{
				case AreaTypeList.Codes.BackPage:
					return Constants.AreaIdentifierTags.BackPage;
				case AreaTypeList.Codes.DocumentFooter:
					return GetConfigDocumentFooterDetail();
				case AreaTypeList.Codes.FirstPageFooter:
					return Constants.AreaIdentifierTags.FirstPageFooter;
				case AreaTypeList.Codes.GroupBy:
					return GetConfigGroupByDetail();
				case AreaTypeList.Codes.LastPageFooter:
					return Constants.AreaIdentifierTags.LastPageFooter;
				case AreaTypeList.Codes.OnlyOnePageFooter:
					return Constants.AreaIdentifierTags.OnlyOnePageFooter;
				case AreaTypeList.Codes.PageFooter:
					return Constants.AreaIdentifierTags.PageFooter;
				case AreaTypeList.Codes.PageHeader:
					return GetConfigPageHeaderDetail();
				case AreaTypeList.Codes.SectionBody:
					return GetConfigSectionBodyDetail();
				case AreaTypeList.Codes.SectionFooter:
					return GetConfigSectionFooterDetail();
				case AreaTypeList.Codes.SectionHeader:
					return GetConfigSectionHeaderDetail();
				case AreaTypeList.Codes.SectionPageFooter:
					return Constants.AreaIdentifierTags.SectionPageFooter;
				case AreaTypeList.Codes.SectionPageHeader:
					return GetConfigSectionPageHeaderDetail();
				default:
					return Type;
			}
		}

		string GetConfigGroupByDetail()
		{
			var result = new ZStringBuilder(Constants.AreaIdentifierTags.GroupBy + string.Join("+", GroupByColumns.Cast<GroupByColumn>().OrderBy(x => x.Order).Select(x => x.Expression)));
			if (GroupTitle)
			{
				result.Append(Constants.GroupByAreaParameters.GroupTitle);
			}
			if (Sticky)
			{
				result.Append(Constants.CommonAreaParameters.Sticky);
			}
			if (PageBreak)
			{
				result.Append(Constants.CommonAreaParameters.PageBreakSignature);
			}
			if (KeepInSamePage)
			{
				result.Append(Constants.GroupByAreaParameters.KeepInSamePage);
			}
			return result.ToStringWithDelimiterBetweenAppends(":");
		}

		string GetConfigSectionBodyDetail()
		{
			var result = new ZStringBuilder(Constants.AreaIdentifierTags.SectionBody + DataSource);
			if (MaximumNumberOfRowsToShow > 0)
			{
				result.AppendFormat("{0}={1}", Constants.SectionBodyAreaParameters.MaximumNumberOfRowsToShow, MaximumNumberOfRowsToShow.ToString());
			}
			if (NumberOfRowsToShow > 0)
			{
				result.AppendFormat("{0}={1}", Constants.SectionBodyAreaParameters.NumberOfRowsToShow, NumberOfRowsToShow.ToString());
			}
			if (RowCountAMultipleOf > 0)
			{
				result.AppendFormat("{0}={1}", Constants.SectionBodyAreaParameters.RowCountAMultipleof, RowCountAMultipleOf.ToString());
			}
			if (StartingRowToShow > 0)
			{
				result.AppendFormat("{0}={1}", Constants.SectionBodyAreaParameters.StartingRowToShow, StartingRowToShow.ToString());
			}
			if (Sticky)
			{
				result.Append(Constants.CommonAreaParameters.Sticky);
			}
			return result.ToStringWithDelimiterBetweenAppends(":");
		}

		string GetConfigDocumentFooterDetail()
		{
			var result = new ZStringBuilder(Constants.AreaIdentifierTags.DocumentFooter);
			if (DontSplit)
			{
				result.Append(Constants.DocumentFooterParameters.DontSplitSignature);
			}
			return result.ToStringWithDelimiterBetweenAppends(":");
		}

		string GetConfigPageHeaderDetail()
		{
			var result = new ZStringBuilder(Constants.AreaIdentifierTags.PageHeader);
			if (StartFromSecondPage)
			{
				result.Append(Constants.Area.StartFromSecondPage);
			}
			return result.ToStringWithDelimiterBetweenAppends(":");
		}

		string GetConfigSectionFooterDetail()
		{
			var result = new ZStringBuilder(Constants.AreaIdentifierTags.SectionFooter);
			if (ShowEvenWithNoData)
			{
				result.Append(Constants.CommonAreaParameters.ShowEvenWithNoDataSignature);
			}
			return result.ToStringWithDelimiterBetweenAppends(":");
		}

		string GetConfigSectionPageHeaderDetail()
		{
			var result = new ZStringBuilder(Constants.AreaIdentifierTags.SectionPageHeader);
			if (Sticky)
			{
				result.Append(Constants.CommonAreaParameters.Sticky);
			}
			return result.ToStringWithDelimiterBetweenAppends(":");
		}

		string GetConfigSectionHeaderDetail()
		{
			var result = new ZStringBuilder(Constants.AreaIdentifierTags.SectionHeader);
			if (ShowEvenWithNoData)
			{
				result.Append(Constants.CommonAreaParameters.ShowEvenWithNoDataSignature);
			}
			if (PageBreak)
			{
				result.Append(Constants.CommonAreaParameters.PageBreakSignature);
			}
			if (Sticky)
			{
				result.Append(Constants.CommonAreaParameters.Sticky);
			}
			return result.ToStringWithDelimiterBetweenAppends(":");
		}

		ReportBizObj IReportBizObjProvider.GetReportBizObj()
		{
			return parent;
		}
		readonly ReportBizObj parent;
	}
}
