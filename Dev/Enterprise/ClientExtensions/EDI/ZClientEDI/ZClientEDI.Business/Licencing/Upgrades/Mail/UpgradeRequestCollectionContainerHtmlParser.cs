using System;
using System.Net;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentParsing;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class UpgradeRequestCollectionContainerHtmlParser : DocumentParser<UpgradeRequestCollectionContainer>
	{
		public UpgradeRequestCollectionContainerHtmlParser(BusinessObjectFactory factory)
			: base(factory)
		{ }

		protected override Type TypeOfWrapper
		{
			get { return typeof(DocUpgradeRequestCollectionContainer); }
		}

		protected override string GetPropertyValue(BusinessObject docWrapper, string codeWithCorrectCasing)
		{
			return WebUtility.HtmlEncode(base.GetPropertyValue(docWrapper, codeWithCorrectCasing));
		}
	}
}
