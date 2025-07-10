using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Web.UI;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Excel;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Utilities;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// used to present timelines in the grid
	/// </summary>
	public class ZTimelineColumn : ZDateTimeColumn, ISupportCustomSorter, IExcelExportCellColor, IExcelExportCellComment
	{
		public ZTimelineColumn(string headerText, string bindTo)
			: base(headerText, bindTo)
		{
		}

		public ZTimelineColumn(string headerText, string bindTo, string bindToEstimated, ZDateTimePickerFormat dateFormat)
			: base(headerText, bindTo, dateFormat)
		{
			this.BindToEstimated = bindToEstimated;
		}

		protected internal override ITemplate GetItemTemplate()
		{
			return new ZTimelineColumnItemTemplate(this);
		}

		protected internal override ITemplate GetEditItemTemplate()
		{
			return new ZTimelineColumnEditItemTemplate(this);
		}

		public string BindToEstimated
		{
			get { return bindToEstimated; }
			set { bindToEstimated = value; }
		}

		string bindToEstimated;

		public ZGuid CustomSortedColumnID
		{
			get { return ZGuid.Empty; }
		}

		public override string SortExpression
		{
			get { return CustomSorterSupportConst.IsCustomSorter + BindTo + BindToEstimated; }
		}

		#region ISupportCustomSorter Members

		public IComparer GetCustomSorter(ListSortDirection sortDirection)
		{
			return new ActualEstimateCollectionSorter(BindTo, BindToEstimated, sortDirection);
		}

		#endregion

		#region IExcelExportCustomValue Members

		protected override IZType GetCustomValueCore(BusinessObject bizObj)
		{
			IZType result = (IZType)ZPropertyAccessor.Get(bizObj, BindTo);
			if (result.IsEmpty)
			{
				result = (IZType)ZPropertyAccessor.Get(bizObj, BindToEstimated);
			}

			if (result == null)
			{
				ErrorReporter.ReportOnce("NullResultInZTimelineColumnGetCustomValueCore", string.Format(
					"Returning null IZType is bad form and should get fixed. BizObj type: {0} PK: {1} BindTo: {2} BindToEstimated: {3} HeaderText: {4}"
					, bizObj.GetType().ToString(), bizObj.PK, BindTo, BindToEstimated, HeaderText));
				return ZString.Empty;
			}
			return result;
		}

		#endregion

		#region IExcelExportCellColor Members

		public Color? GetCustomColor(BusinessObject bizObj)
		{
			var actualValue = AsOffset(ZPropertyAccessor.Get(bizObj, BindTo));
			var estimatedValue = AsOffset(ZPropertyAccessor.Get(bizObj, BindToEstimated));

			return TimelineHelper.GetColor(actualValue, estimatedValue);
		}

		#endregion

		#region IExcelExportCellComment Members

		public ZString GetComment(BusinessObject bizObj)
		{
			var actualValue = AsOffset(ZPropertyAccessor.Get(bizObj, BindTo));
			var estimatedValue = AsOffset(ZPropertyAccessor.Get(bizObj, BindToEstimated));

			return TimelineHelper.GetComment(actualValue, estimatedValue, DateTimeFormat);
		}

		internal static ZDateTimeOffset AsOffset(object obj)
		{
			return obj is ZDateTimeOffset offset ? offset : new ZDateTimeOffset(obj);
		}

		#endregion
	}
}
