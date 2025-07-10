using System;
using System.ComponentModel;
using Enterprise.Core.Forms;

namespace Enterprise.ZArchitecture
{
	sealed class ZSqlProfilerTextBoxColumnStyleInfo : ZTextBoxColumnStyleInfo
	{
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType
		{
			get { return typeof(ZSqlProfilerTextBoxColumnStyle); }
		}
	}

	sealed class ZSqlProfilerTextBoxColumnStyle : ZTextBoxColumnStyle
	{
		public ZSqlProfilerTextBoxColumnStyle(ZTextBoxColumnStyleInfo columnInfo)
			: base(columnInfo)
		{
		}

		protected override GridColumnNotificationProvider GetNewGridColumnNotificationProvider()
		{
			return new ZSqlProfilerGridColumnNotificationProvider(this);
		}
	}
}