using System;
using System.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Services.OperationalActions.GUI
{
	internal sealed class FieldFindBoxColumnStyleInfo : ZTextBoxColumnStyleInfo, IZColumnStyleInfo
	{
		public FieldFindBoxColumnStyleInfo() { }

		[ZColumnBindingMemberType(typeof(ZString))]
		public override string ColumnName
		{
			get { return base.ColumnName; }
			set { base.ColumnName = value; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType
		{
			get { return typeof(FieldFindBoxColumnStyle); }
		}
	}
}