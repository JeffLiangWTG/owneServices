using System.Collections.Generic;
using CargoWise.Types;

using Enterprise.ZArchitecture.Web.Business.Utilities;

namespace Enterprise.DocumentScanning.Web
{
	public class eDocsRequestHelper : DataRequestHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public const string DocumentKey = "Doc";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public const string ParentKey = "Ref";

		public string GetHandlerUrl(ZGuid documentPK, ZGuid parentPK)
		{
			Dictionary<string, ZGuid> keys = new Dictionary<string, ZGuid>();

			keys.Add(DocumentKey, documentPK);
			keys.Add(ParentKey, parentPK);

			return base.GetHandlerUrl(keys);
		}

		public override string BaseUrl
		{
			get { return "eDocsRequestHandler.axd"; }
		}

		public override bool EnableCache
		{
			get { return true; }
		}

		public override bool UseSecureQueryString
		{
			get { return false; }
		}
	}
}
