using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters
{
	class WebURLValueObjectHelper
	{
		public WebURLValueObjectHelper(string errorContext)
		{
			this.ErrorContext = errorContext;
		}

		public readonly string ErrorContext;

		#region Import

		public void ImportFromValueObjectCollection(Xsd.OrgWebURLCollection urlValueCollection, OrgHeader organisation, IValueObjectImportContext context)
		{
			if (urlValueCollection.IsSpecified)
			{
				for (int i = 0; i < urlValueCollection.Count; i++)
				{
					Xsd.OrgWebURL urlValue = urlValueCollection[i];
					ImportFromValueObject(organisation, urlValue, context);
				}
			}
		}

		public void ImportMainWebURL(OrgHeader organisation, IValueObjectImportContext context, string urlValue)
		{
			Xsd.OrgWebURL url = new Enterprise.DataTransfer.Xml.XsdVersion1.OrgWebURL();
			url.URL = urlValue;
			url.Type = Enterprise.DataTransfer.Xml.XsdVersion1.OrgWebURLType.MAI;
			url.IsPrimary = true;
			url.Description = Res.GetString("164c058b-a170-4fce-b66f-5c16650ea10b", "Main Website");
			ImportFromValueObject(organisation, url, context);
		}

		public OrgWebURL ImportFromValueObject(OrgHeader organisation, Xsd.OrgWebURL urlValue, IValueObjectImportContext context)
		{
			OrgWebURL urlToUpdate = null;
			OrgWebURL[] foundUrls = System.Array.Empty<OrgWebURL>();

			foreach (OrgWebURL orgURL in organisation.OrgWebURLs)
			{
				if (urlValue.URL.EqualsIgnoringCase(orgURL.PU_URL))
				{
					urlToUpdate = orgURL;
					break;
				}
			}
			if (urlToUpdate == null)
			{
				urlToUpdate = organisation.OrgWebURLs.AddNew();
			}

			SetURLDetails(urlToUpdate, urlValue, context);
			return urlToUpdate;
		}

		void SetURLDetails(OrgWebURL urlToUpdate, Xsd.OrgWebURL urlValue, IValueObjectImportContext context)
		{
			context.SetPropertyInfoValueIfValueNotEmpty(urlToUpdate.PU_URLInfo, urlValue.URL);
			context.SetPropertyInfoValueIfValueNotEmpty(urlToUpdate.PU_TypeInfo, urlValue.Type.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(urlToUpdate.PU_IsPrimaryInfo, urlValue.IsPrimary.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(urlToUpdate.PU_DescriptionInfo, urlValue.Description);
		}

		#endregion

		#region Export

		public void ExportToValueObjectCollection(OrgWebURLDependentCollection urls, Xsd.OrgWebURLCollection urlValueCollection, INotifications notifications)
		{
			OrgWebURL[] urlsInExportOrder = GetURLsInExportOrder(urls, notifications);

			foreach (OrgWebURL url in urlsInExportOrder)
			{
				Xsd.OrgWebURL newURL = ExportToValueObject(url, notifications);
				urlValueCollection.Add(newURL);
			}
		}

		OrgWebURL[] GetURLsInExportOrder(OrgWebURLDependentCollection urls, INotifications notifications)
		{
			List<OrgWebURL> result = new List<OrgWebURL>();
			if (!urls.MainURL.MainDefaultAdded && !urls.MainURL.PU_URL.IsEmpty)
			{
				result.Add(urls.MainURL);
			}

			foreach (OrgWebURL url in urls)
			{
				if (url != urls.MainURL)
				{
					result.Add(url);
				}
			}
			return result.ToArray();
		}

		public Xsd.OrgWebURL ExportToValueObject(OrgWebURL url, INotifications notifications)
		{
			Xsd.OrgWebURL result = new Xsd.OrgWebURL();

			if (!url.PU_URL.IsEmpty)
			{
				result.URL = url.PU_URL;
			}

			if (!url.PU_Type.IsEmpty)
			{
				result.Type = URLTypeXmlMappings.Instance.GetExternalCode(url, ErrorContext, notifications);
			}

			if (!url.PU_IsPrimary.IsEmpty)
			{
				result.IsPrimary = url.PU_IsPrimary;
			}

			if (!url.PU_Description.IsEmpty)
			{
				result.Description = url.PU_Description;
			}

			return result;
		}

		#endregion
	}
}
