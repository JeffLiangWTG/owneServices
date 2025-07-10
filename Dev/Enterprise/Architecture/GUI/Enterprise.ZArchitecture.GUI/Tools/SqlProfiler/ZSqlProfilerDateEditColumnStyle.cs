using System;
using System.ComponentModel;
using Enterprise.Core.Forms;

namespace Enterprise.ZArchitecture
{
	sealed class ZSqlProfilerDateEditColumnStyleInfo : ZDateEditColumnStyleInfo
	{
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType
		{
			get { return typeof(ZSqlProfilerDateEditColumnStyle); }
		}
	}

	sealed class ZSqlProfilerDateEditColumnStyle : ZDateEditColumnStyle
	{
		public ZSqlProfilerDateEditColumnStyle(ZDateEditColumnStyleInfo columnInfo)
			: base(columnInfo)
		{
		}

		protected override GridColumnNotificationProvider GetNewGridColumnNotificationProvider()
		{
			return new ZSqlProfilerGridColumnNotificationProvider(this);
		}
	}
}