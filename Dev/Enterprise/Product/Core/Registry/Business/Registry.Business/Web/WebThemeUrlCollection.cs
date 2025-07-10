using System;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class WebThemeUrlCollection : RegistryBusinessObjectCollectionTemplate<WebThemeUrl>
	{
		public WebThemeUrlCollection()
			: base(null, null)
		{ }

		protected override bool AllowNewCore
		{
			get { return true; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new WebThemeUrl();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new WebThemeUrlCollection();
		}

		public WebThemeUrl Find(ZString url, ZString companyCode)
		{
			WebThemeUrl result = null;

			WebThemeUrl matchedByUrlAndCompany = null;
			WebThemeUrl matchedByUrl = null;

			foreach (WebThemeUrl themeUrl in this)
			{
				if (StartsWithWebUrl(url, themeUrl.Url))
				{
					if (themeUrl.CompanyCode.EqualsIgnoringCase(companyCode))
					{
						if (matchedByUrlAndCompany == null || matchedByUrlAndCompany.Url.Length < themeUrl.Url.Length)
						{
							matchedByUrlAndCompany = themeUrl;
						}
					}
					else if (themeUrl.CompanyCode.IsEmpty)
					{
						if (matchedByUrl == null || matchedByUrl.Url.Length < themeUrl.Url.Length)
						{
							matchedByUrl = themeUrl;
						}
					}
				}
			}

			result = matchedByUrlAndCompany ?? matchedByUrl;

			return result;
		}

		static bool StartsWithWebUrl(string url, string webUrl)
		{
			return UriWithoutScheme(url).StartsWith(UriWithoutScheme(webUrl), StringComparison.OrdinalIgnoreCase);

			string UriWithoutScheme(string uriString)
			{
				if (Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
				{
					return uri.GetComponents(UriComponents.AbsoluteUri & ~UriComponents.Scheme, UriFormat.UriEscaped);
				}

				return uriString;
			}
		}
	}
}
