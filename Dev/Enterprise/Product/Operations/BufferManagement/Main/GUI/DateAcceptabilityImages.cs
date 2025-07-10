using System.Drawing;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.GUI
{
	public static class DateAcceptabilityImages
	{
		public static Bitmap Get(string code)
		{
			switch (code)
			{
				case DateAcceptabilityList.Codes.ExtendedStartExtendedFinish:
					return Properties.Resources.DA1;
				case DateAcceptabilityList.Codes.GraduatedStartExtendedFinish:
					return Properties.Resources.DA2;
				case DateAcceptabilityList.Codes.SharpStartExtendedFinish:
					return Properties.Resources.DA3;
				case DateAcceptabilityList.Codes.ExtendedStartGraduatedFinish:
					return Properties.Resources.DA4;
				case DateAcceptabilityList.Codes.GraduatedStartGraduatedFinish:
					return Properties.Resources.DA5;
				case DateAcceptabilityList.Codes.SharpStartGraduatedFinish:
					return Properties.Resources.DA6;
				case DateAcceptabilityList.Codes.ExtendedStartSharpFinish:
					return Properties.Resources.DA7;
				case DateAcceptabilityList.Codes.GraduatedStartSharpFinish:
					return Properties.Resources.DA8;
				case DateAcceptabilityList.Codes.SharpStartSharpFinish:
					return Properties.Resources.DA9;

				default:
					return null;
			}
		}
	}
}
