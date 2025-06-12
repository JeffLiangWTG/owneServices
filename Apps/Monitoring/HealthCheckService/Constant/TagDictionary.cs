using System.Collections.Generic;
using XH.Framework.XT.REST.API.Common;

namespace XH.XT.Monitoring.HealthCheckService.Constant
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Meaning of class name is unambiguous.")]
	public static class TagDictionary
	{
		public static readonly Dictionary<XtType, string> XtTagDict = new Dictionary<XtType, string>
		{
			{ XtType.Folder, "xt-folder:/" },
			{ XtType.xView, "xt-xview:/" }
		};
	}
}
