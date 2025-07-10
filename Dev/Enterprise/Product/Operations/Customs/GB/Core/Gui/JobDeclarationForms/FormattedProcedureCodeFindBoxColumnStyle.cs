using System;
using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI
{
	public class FormattedProcedureCodeFindBoxColumnStyle : ZCodeFindBoxColumnStyle
	{
		public FormattedProcedureCodeFindBoxColumnStyle(FormattedProcedureCodeFindBoxColumnStyleInfo columnInfo)
			: base(() => new FormattedProcedureGridFindBox(), columnInfo)
		{
		}
	}

	public class FormattedProcedureCodeFindBoxColumnStyleInfo : ZCodeFindBoxColumnStyleInfo
	{
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType
		{
			get { return typeof(FormattedProcedureCodeFindBoxColumnStyle); }
		}
	}
}
