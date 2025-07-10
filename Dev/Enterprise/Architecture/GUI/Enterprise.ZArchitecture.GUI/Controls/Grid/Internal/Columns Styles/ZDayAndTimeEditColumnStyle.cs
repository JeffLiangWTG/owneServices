using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Core.Constants;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	public class ZDayAndTimeEditColumnStyleInfo : ZTimeEditExColumnStyleInfo
	{
		public ZDayAndTimeEditColumnStyleInfo() // required for the ZGrid column designer
		{
		}

		public ZDayAndTimeEditColumnStyleInfo(string columnName, int width) : base(columnName, width)
		{
		}

		[ZColumnBindingMemberType(typeof(ZDateTime))]
		public override string ColumnName
		{
			get { return base.ColumnName; }
			set { base.ColumnName = value; }
		}

		[ZColumnBindingMemberType(typeof(ZString))]
		public string BindToTimeUnit { get; set; }

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType
		{
			get { return typeof(ZDayAndTimeEditColumnStyle); }
		}
	}

	public class ZDayAndTimeEditColumnStyle : ZTimeEditExColumnStyle, IZColumn, IZDateTimePickerFormat
	{
		public ZDayAndTimeEditColumnStyle(ZDayAndTimeEditColumnStyleInfo columnInfo) : base(() => new ZDayAndTimeEdit.Bare(), columnInfo)
		{
		}

		protected override void OnInit(Control control)
		{
			base.OnInit(control);
			var timeControl = (ZDayAndTimeEdit.Bare)control;
			timeControl.BindToTimeUnit = ColumnInfo.BindToTimeUnit;
		}

		#region Stuff that I only need because I don't know how to bind BindToTimeUnit properly lmao (works similarly to ZMultiControlColumnStyle.cs)

		void UpdateTimeUnitFromSource(CurrencyManager source, int rowNum)
		{
			var bizObj = source.List[rowNum] as BusinessObject;
			UpdateTimeUnitFromBizO(bizObj);
		}

		void UpdateTimeUnitFromBizO(BusinessObject bizObj)
		{
			var timeUnitString = GetTimeUnitFromBizO(bizObj);
			if (timeUnitString != null)
			{
				EditControl.TimeUnit = timeUnitString;
			}
		}

		string GetTimeUnitFromSource(CurrencyManager source, int rowNum)
		{
			var bizObj = source.List[rowNum] as BusinessObject;
			return GetTimeUnitFromBizO(bizObj);
		}

		string GetTimeUnitFromBizO(BusinessObject bizObj)
		{
			if (bizObj != null && ColumnInfo.BindToTimeUnit != null)
			{
				return ColumnInfo != null ? bizObj[ColumnInfo.BindToTimeUnit]?.ToString() : null;
			}
			return null;
		}

		DisposableAction TemporarilyChangeTimeUnit(string timeUnit)
		{
			var oldTimeUnit = EditControl.TimeUnit;
			EditControl.TimeUnit = timeUnit;
			return new DisposableAction(() => { EditControl.TimeUnit = oldTimeUnit; });
		}

		protected override void PrepareEditControl(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly)
		{
			UpdateTimeUnitFromSource(source, rowNum);

			base.PrepareEditControl(source, rowNum, bounds, readOnly);
		}

		protected override string GetTextFromDateTimeWithColonIfEmpty(CurrencyManager source, int rowNum)
		{
			using (TemporarilyChangeTimeUnit(GetTimeUnitFromSource(source, rowNum)))
			{
				var result = GetTextFromDateTime(source, rowNum);

				if (String.IsNullOrEmpty(result) && Core.TimeUnit != ContainerPenaltyTimeUnit.Codes.Days)
				{
					result = Core.EmptyText;
				}

				return result;
			}
		}

		protected override string FormatValueObjectCore(object source, object propertyValue)
		{
			using (TemporarilyChangeTimeUnit(GetTimeUnitFromBizO(source as BusinessObject)))
			{
				return Core.GetTextFromTime((ZDateTime)propertyValue, ColumnInfo.AllowNegative);
			}
		}

		protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible)
		{
			UpdateTimeUnitFromSource(source, rowNum);

			base.Edit(source, rowNum, bounds, readOnly, instantText, cellIsVisible);
		}

		#endregion

		#region Implementation

		new ZDayAndTimeEditColumnStyleInfo ColumnInfo
		{
			get { return (ZDayAndTimeEditColumnStyleInfo)base.ColumnInfo; }
		}

		new ZDayAndTimeEdit EditControl
		{
			get { return (ZDayAndTimeEdit)base.EditControl; }
		}

		internal new ZDayAndTimeEditCore Core
		{
			get { return (ZDayAndTimeEditCore)EditControl.Core; }
		}

		#endregion
	}
}
