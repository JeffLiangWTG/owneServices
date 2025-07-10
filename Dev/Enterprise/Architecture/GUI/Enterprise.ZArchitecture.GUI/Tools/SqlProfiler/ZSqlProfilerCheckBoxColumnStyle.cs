using System;
using System.ComponentModel;
using Enterprise.Core.Forms;

namespace Enterprise.ZArchitecture
{
	sealed class ZSqlProfilerCheckBoxColumnStyleInfo : ZCheckBoxColumnStyleInfo
	{
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType
		{
			get { return typeof(ZSqlProfilerCheckBoxColumnStyle); }
		}
	}

	sealed class ZSqlProfilerCheckBoxColumnStyle : ZCheckBoxColumnStyle
	{
		public ZSqlProfilerCheckBoxColumnStyle(ZCheckBoxColumnStyleInfo columnInfo)
			: base(columnInfo)
		{
		}

		protected override GridColumnNotificationProvider GetNewGridColumnNotificationProvider()
		{
			return new ZSqlProfilerGridColumnNotificationProvider(this);
		}
	}
}