using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.GB.GVMS
{
	public partial class GVMSManifestNature
	{
		public static Dictionary<ZString, ZString> NatureFullNameMapping =
			new Dictionary<ZString, ZString>()
			{
				{ Codes.Import, "UK_INBOUND" },
				{ Codes.Export, "UK_OUTBOUND" },
				{ Codes.GBtoNI, "GB_TO_NI" },
				{ Codes.NItoGB, "NI_TO_GB" },
			};
	}
}
