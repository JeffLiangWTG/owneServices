using System;
using System.ComponentModel;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI
{
	public class AlternateGLAccountFindBoxColumnStyle : ZGuidFindBoxColumnStyle
	{
		public AlternateGLAccountFindBoxColumnStyle(AlternateGLAccountFindBoxColumnStyleInfo columnInfo)
			: base(() => new AlternateGLAccountGridFindBox(), columnInfo)
		{
		}

		public class AlternateGLAccountGridFindBox : ZGridGuidFindBox
		{
			protected override IModuleDecisionProvider GetModuleDecisionProvider(ZFilterModule module)
			{
				return new AlternateGLAccountModuleDecisionProvider(this);
			}
		}
	}

	public class AlternateGLAccountFindBoxColumnStyleInfo : ZGuidFindBoxColumnStyleInfo, IZColumnStyleInfo
	{
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType
		{
			get { return typeof(AlternateGLAccountFindBoxColumnStyle); }
		}
	}
}
