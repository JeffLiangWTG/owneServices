using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.BufferManagement.GUI
{
	public class ZDurationConvertsColumnStyleInfo : ZTextBoxColumnStyleInfo, IZColumnStyleInfo
	{
		public override Type ColumnStyleType
		{
			get { return typeof(ZDurationConvertsColumnStyle); }
		}
	}
	public class ZDurationConvertsColumnStyle : ZTextBoxColumnStyle, IZColumnStyleInfo
	{
		public ZDurationConvertsColumnStyle(ZDurationConvertsColumnStyleInfo columnInfo) : base(columnInfo)
		{
		}
		protected override string FormatValueObjectCore(object source, object propertyValue)
		{
			return TimeSpan.FromHours((double)(ZDecimal)propertyValue).ToHoursAndMinutesString();
		}
	}
}
