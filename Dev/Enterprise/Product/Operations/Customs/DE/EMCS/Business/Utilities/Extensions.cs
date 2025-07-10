using System.Net;
using CargoWise.Types;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public static class Extensions
	{
		public static ZBool IsConsolidatedDocument(this EMCSJobDeclaration dec) => dec != null && dec.ZG_DeferredSubmission == EmcsDeferredSubmissionList.Codes.JaZusammengefasstesEVd;

		public static ZString EncodeToHTMLFormat(this ZString text)
		{
			var encodedHtml = WebUtility.HtmlEncode(text.Trim());

			return ZString.Format(template, encodedHtml);
		}

		const string template = @"
						<html>
							</head>
							<body>
								<pre>{0}<pre>
							</body>
						</html>";
	}
}
