using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using ZClientEDI.Business.IncidentManager.ELearningDocument.Business;

namespace Enterprise.Client.EDI.IncidentManager.BatchProcessor
{
	public class IncidentAutoresponderDocumentTitle
	{
		bool IsValidDocumentUrl(string documentUrl)
		{
			if (string.IsNullOrEmpty(documentUrl))
			{
				return false;
			}

			if (!documentUrl.ToLower().Contains("myaccount-portal.cargowise.com"))
			{
				return false;
			}

			if (!documentUrl.ToLower().EndsWith(".pdf"))
			{
				return false;
			}

			return true;
		}

		string GetHttpsUrl(string url)
		{
			var urlWithHttps = url.Trim().Replace("http://", "https://");
			return urlWithHttps;
		}

		Dictionary<string, string> FetchDocumentTitle(BusinessObjectFactory factory, List<string> documentUrls)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			documentUrls = documentUrls.Where(IsValidDocumentUrl).Select(GetHttpsUrl).Distinct().ToList();
			var eLearningDocumentDescriptionQuery = new ZQuery(ELearningDocumentDescriptionSchema.ELD_Url, documentUrls);
			var documentTitleByUrl = factory.Load<ELearningDocumentDescription>(eLearningDocumentDescriptionQuery).GroupBy(row => row.ELD_Url.ToString()).ToDictionary(grp => grp.Key, grp => grp.First().ELD_Title.ToString());
			return documentTitleByUrl;
		}

		public IEnumerable<string> GetDocumentUrlsWithTitle(IEnumerable<string> urls)
		{
			if (urls == null)
			{
				throw new ArgumentNullException(nameof(urls));
			}

			var documentUrls = urls.ToList();
			var documentUrlsWithTitle = new List<string>();

			if (!documentUrls.Any())
			{
				return documentUrlsWithTitle;
			}

			var documentTitleByUrl = FetchDocumentTitle(new BusinessObjectFactory(), documentUrls);

			foreach (var url in documentUrls)
			{
				var urlWithHttps = GetHttpsUrl(url);
				if (documentTitleByUrl.ContainsKey(urlWithHttps) && (!string.IsNullOrEmpty(documentTitleByUrl[urlWithHttps])))
				{
					documentUrlsWithTitle.Add($"{documentTitleByUrl[urlWithHttps]}\n{url}");
				}
				else
				{
					documentUrlsWithTitle.Add(url);
				}
			}

			return documentUrlsWithTitle;
		}
	}
}
