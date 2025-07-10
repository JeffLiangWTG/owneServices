using System;
using System.Net;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentParsing;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class SupportIncidentHtmlParser : DocumentParser<SupportIncident>
	{
		public SupportIncidentHtmlParser(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override Type TypeOfWrapper
		{
			get { return typeof(DocSupportIncident); }
		}

		protected override string GetPropertyValue(BusinessObject docWrapper, string codeWithCorrectCasing)
		{
			return WebUtility.HtmlEncode(base.GetPropertyValue(docWrapper, codeWithCorrectCasing));
		}
	}
}

